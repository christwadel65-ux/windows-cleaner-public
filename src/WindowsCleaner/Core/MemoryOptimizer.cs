using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WindowsCleaner
{
    /// <summary>
    /// Gestionnaire d'optimisation mémoire et de ressources
    /// </summary>
    public static class MemoryOptimizer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

        /// <summary>
        /// Pool d'objets FileInfo pour réduire les allocations
        /// </summary>
        private static class FileInfoPool
        {
            private static readonly Stack<FileInfo> _pool = new Stack<FileInfo>();
            private static readonly object _lockObj = new object();
            private const int MaxPoolSize = 256;

            public static FileInfo Rent(string path)
            {
                FileInfo info;
                lock (_lockObj)
                {
                    info = _pool.Count > 0 ? _pool.Pop() : new FileInfo(path);
                }
                if (info != null && info.FullName != path)
                {
                    info = new FileInfo(path);
                }
                return info;
            }

            public static void Return(FileInfo info)
            {
                if (info == null) return;
                lock (_lockObj)
                {
                    if (_pool.Count < MaxPoolSize)
                    {
                        _pool.Push(info);
                    }
                }
            }
        }

        /// <summary>
        /// Pool de DirectoryInfo pour réduire les allocations
        /// </summary>
        private static class DirectoryInfoPool
        {
            private static readonly Stack<DirectoryInfo> _pool = new Stack<DirectoryInfo>();
            private static readonly object _lockObj = new object();
            private const int MaxPoolSize = 128;

            public static DirectoryInfo Rent(string path)
            {
                DirectoryInfo info;
                lock (_lockObj)
                {
                    info = _pool.Count > 0 ? _pool.Pop() : new DirectoryInfo(path);
                }
                if (info != null && info.FullName != path)
                {
                    info = new DirectoryInfo(path);
                }
                return info;
            }

            public static void Return(DirectoryInfo info)
            {
                if (info == null) return;
                lock (_lockObj)
                {
                    if (_pool.Count < MaxPoolSize)
                    {
                        _pool.Push(info);
                    }
                }
            }
        }

        /// <summary>
        /// Optimise l'utilisation mémoire du processus actuel
        /// </summary>
        /// <param name="aggressive">Si true, effectue une optimisation agressive (GC complet)</param>
        public static void OptimizeMemory(bool aggressive = false)
        {
            try
            {
                if (aggressive)
                {
                    // Forçage du Garbage Collector 3 fois (compaction)
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                else
                {
                    // Nettoyage léger
                    GC.Collect(GC.GetTotalMemory(false) < GC.GetTotalMemory(true) ? GC.MaxGeneration : 1);
                }

                // Réduit le working set mémoire du processus
                var proc = System.Diagnostics.Process.GetCurrentProcess();
                SetProcessWorkingSetSize(proc.Handle, new IntPtr(-1), new IntPtr(-1));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de l'optimisation mémoire: {ex.Message}");
            }
        }

        /// <summary>
        /// Crée un énumérateur de fichiers optimisé qui minimise les allocations
        /// </summary>
        public static IEnumerable<(string path, long size)> EnumerateFilesOptimized(
            string directory, 
            string searchPattern = "*", 
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return Enumerable.Empty<(string, long)>();
            }

            return EnumerateFilesOptimizedIterator(directory, searchPattern, searchOption, cancellationToken);
        }

        private static IEnumerable<(string path, long size)> EnumerateFilesOptimizedIterator(
            string directory, 
            string searchPattern,
            SearchOption searchOption,
            CancellationToken cancellationToken)
        {
            var queue = new Queue<string>(16);
            queue.Enqueue(directory);

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string currentDir = queue.Dequeue();

                // Énumère les fichiers du répertoire actuel
                var files = SafeEnumerateFiles(currentDir, searchPattern);
                foreach (var fileWithSize in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return fileWithSize;
                }

                // Ajoute les sous-répertoires si nécessaire
                if (searchOption == SearchOption.AllDirectories)
                {
                    var dirs = SafeEnumerateDirectories(currentDir);
                    foreach (var dir in dirs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        queue.Enqueue(dir);
                    }
                }
            }
        }

        private static IEnumerable<(string, long)> SafeEnumerateFiles(string directory, string pattern)
        {
            var results = new List<(string, long)>();
            
            try
            {
                foreach (var file in Directory.EnumerateFiles(directory, pattern))
                {
                    try
                    {
                        var fileInfo = FileInfoPool.Rent(file);
                        long size = fileInfo.Length;
                        FileInfoPool.Return(fileInfo);
                        results.Add((file, size));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Erreur lors de la lecture du fichier {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de l'énumération des fichiers dans {directory}: {ex.Message}");
            }
            
            foreach (var result in results)
            {
                yield return result;
            }
        }

        private static IEnumerable<string> SafeEnumerateDirectories(string directory)
        {
            var results = new List<string>();
            
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(directory))
                {
                    results.Add(dir);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de l'énumération des répertoires dans {directory}: {ex.Message}");
            }
            
            foreach (var result in results)
            {
                yield return result;
            }
        }

        /// <summary>
        /// Traite les fichiers par batch pour optimiser la mémoire
        /// </summary>
        public static void ProcessFilesInBatches(
            string directory,
            Action<IEnumerable<string>> processBatch,
            int batchSize = 1000,
            string searchPattern = "*",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return;
            }

            var batch = new List<string>(batchSize);

            try
            {
                foreach (var file in Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    batch.Add(file);

                    if (batch.Count >= batchSize)
                    {
                        processBatch(batch);
                        batch.Clear();

                        // Optimise mémoire tous les batchs
                        if ((batchSize % 5000) == 0)
                        {
                            GC.Collect(0); // Collect generation 0 only
                        }
                    }
                }

                // Traite le dernier batch
                if (batch.Count > 0)
                {
                    processBatch(batch);
                }
            }
            finally
            {
                batch.Clear();
            }
        }

        /// <summary>
        /// Obtient l'utilisation mémoire actuelle du processus
        /// </summary>
        /// <returns>Utilisation mémoire en MB</returns>
        public static double GetMemoryUsageMB()
        {
            using (var proc = System.Diagnostics.Process.GetCurrentProcess())
            {
                return proc.WorkingSet64 / (1024.0 * 1024.0);
            }
        }

        /// <summary>
        /// Énumère les fichiers avec gestion efficace des ressources
        /// </summary>
        public static IEnumerable<FileInfo> EnumerateFileInfoOptimized(
            string directory,
            Predicate<FileInfo> filter,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return Enumerable.Empty<FileInfo>();
            }

            return EnumerateFileInfoOptimizedIterator(directory, filter, cancellationToken);
        }

        private static IEnumerable<FileInfo> EnumerateFileInfoOptimizedIterator(
            string directory,
            Predicate<FileInfo> filter,
            CancellationToken cancellationToken)
        {
            var queue = new Queue<string>(8);
            queue.Enqueue(directory);

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string currentDir = queue.Dequeue();

                var files = SafeEnumerateFileInfos(currentDir, filter);
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return file;
                }

                var dirs = SafeEnumerateSubDirectories(currentDir);
                foreach (var subDir in dirs)
                {
                    queue.Enqueue(subDir);
                }
            }
        }

        private static IEnumerable<FileInfo> SafeEnumerateFileInfos(string directory, Predicate<FileInfo> filter)
        {
            var dirInfo = DirectoryInfoPool.Rent(directory);
            
            try
            {
                foreach (var file in dirInfo.GetFiles())
                {
                    if (filter(file))
                    {
                        yield return file;
                    }
                }
            }
            finally
            {
                DirectoryInfoPool.Return(dirInfo);
            }
        }

        private static IEnumerable<string> SafeEnumerateSubDirectories(string directory)
        {
            var dirInfo = DirectoryInfoPool.Rent(directory);
            
            try
            {
                foreach (var subDir in dirInfo.GetDirectories())
                {
                    yield return subDir.FullName;
                }
            }
            finally
            {
                DirectoryInfoPool.Return(dirInfo);
            }
        }

        /// <summary>
        /// Compacte les chaînes de caractères pour libérer mémoire
        /// </summary>
        public static void CompactStrings()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch
            {
                // Silentieux en cas d'erreur
            }
        }
    }
}
