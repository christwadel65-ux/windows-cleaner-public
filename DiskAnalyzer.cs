using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleaner
{
    /// <summary>
    /// Information sur un fichier volumineux
    /// </summary>
    public class LargeFileInfo
    {
        public string Path { get; set; } = "";
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Extension { get; set; } = "";
        
        public string FormattedSize => FormatBytes(Size);
        
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
    
    /// <summary>
    /// Catégorie d'utilisation du disque
    /// </summary>
    public class DiskUsageCategory
    {
        public string Name { get; set; } = "";
        public long TotalSize { get; set; }
        public int FileCount { get; set; }
        public double Percentage { get; set; }
        
        public string FormattedSize => FormatBytes(TotalSize);
        
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
    
    /// <summary>
    /// Résultat de l'analyse du disque
    /// </summary>
    public class DiskAnalysisResult
    {
        public List<LargeFileInfo> LargestFiles { get; } = new List<LargeFileInfo>();
        public List<DiskUsageCategory> Categories { get; } = new List<DiskUsageCategory>();
        public long TotalScannedSize { get; set; }
        public int TotalScannedFiles { get; set; }
        public TimeSpan ScanDuration { get; set; }
    }
    
    /// <summary>
    /// Analyseur d'espace disque pour identifier les fichiers volumineux et catégories
    /// </summary>
    public static class DiskAnalyzer
    {
        private static readonly Dictionary<string, string> CategoryMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Vidéos
            { ".mp4", "Vidéos" }, { ".avi", "Vidéos" }, { ".mkv", "Vidéos" }, { ".mov", "Vidéos" },
            { ".wmv", "Vidéos" }, { ".flv", "Vidéos" }, { ".webm", "Vidéos" }, { ".m4v", "Vidéos" },
            
            // Audio
            { ".mp3", "Audio" }, { ".wav", "Audio" }, { ".flac", "Audio" }, { ".aac", "Audio" },
            { ".ogg", "Audio" }, { ".wma", "Audio" }, { ".m4a", "Audio" },
            
            // Images
            { ".jpg", "Images" }, { ".jpeg", "Images" }, { ".png", "Images" }, { ".gif", "Images" },
            { ".bmp", "Images" }, { ".tiff", "Images" }, { ".webp", "Images" }, { ".svg", "Images" },
            { ".psd", "Images" }, { ".raw", "Images" },
            
            // Documents
            { ".pdf", "Documents" }, { ".doc", "Documents" }, { ".docx", "Documents" },
            { ".xls", "Documents" }, { ".xlsx", "Documents" }, { ".ppt", "Documents" },
            { ".pptx", "Documents" }, { ".txt", "Documents" }, { ".rtf", "Documents" },
            
            // Archives
            { ".zip", "Archives" }, { ".rar", "Archives" }, { ".7z", "Archives" },
            { ".tar", "Archives" }, { ".gz", "Archives" }, { ".bz2", "Archives" },
            
            // Exécutables
            { ".exe", "Applications" }, { ".msi", "Applications" }, { ".dll", "Applications" },
            { ".sys", "Applications" }, { ".bat", "Applications" }, { ".cmd", "Applications" },
            
            // Développement
            { ".cs", "Code Source" }, { ".py", "Code Source" }, { ".js", "Code Source" },
            { ".java", "Code Source" }, { ".cpp", "Code Source" }, { ".c", "Code Source" },
            { ".h", "Code Source" }, { ".html", "Code Source" }, { ".css", "Code Source" },
            { ".json", "Code Source" }, { ".xml", "Code Source" },
            
            // Bases de données
            { ".db", "Bases de Données" }, { ".sqlite", "Bases de Données" },
            { ".mdb", "Bases de Données" }, { ".accdb", "Bases de Données" },
            
            // Images disque
            { ".iso", "Images Disque" }, { ".img", "Images Disque" }, { ".vhd", "Images Disque" },
            { ".vmdk", "Images Disque" }
        };
        
        /// <summary>
        /// Analyse un dossier et retourne les statistiques d'utilisation
        /// </summary>
        public static async Task<DiskAnalysisResult> AnalyzeDirectory(
            string path, 
            int topFileCount = 100,
            Action<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.Now;
            var result = new DiskAnalysisResult();
            var categoryDict = new Dictionary<string, DiskUsageCategory>();
            var largeFiles = new List<LargeFileInfo>();
            
            progress?.Invoke($"Démarrage de l'analyse de: {path}");
            
            await Task.Run(() =>
            {
                try
                {
                    ScanDirectory(path, largeFiles, categoryDict, result, progress, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Erreur analyse disque: {ex.Message}");
                }
            }, cancellationToken);
            
            // Trier et limiter les plus gros fichiers
            result.LargestFiles.AddRange(largeFiles.OrderByDescending(f => f.Size).Take(topFileCount));
            
            // Calculer les pourcentages
            foreach (var category in categoryDict.Values)
            {
                category.Percentage = result.TotalScannedSize > 0 
                    ? (category.TotalSize * 100.0 / result.TotalScannedSize) 
                    : 0;
                result.Categories.Add(category);
            }
            
            result.Categories.Sort((a, b) => b.TotalSize.CompareTo(a.TotalSize));
            result.ScanDuration = DateTime.Now - startTime;
            
            progress?.Invoke($"Analyse terminée: {result.TotalScannedFiles} fichiers, {FormatBytes(result.TotalScannedSize)}");
            
            return result;
        }
        
        private static void ScanDirectory(
            string path,
            List<LargeFileInfo> largeFiles,
            Dictionary<string, DiskUsageCategory> categories,
            DiskAnalysisResult result,
            Action<string>? progress,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            
            try
            {
                // Scanner les fichiers
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var extension = fileInfo.Extension.ToLower();
                        
                        // Ajouter aux fichiers volumineux
                        largeFiles.Add(new LargeFileInfo
                        {
                            Path = file,
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            Extension = extension
                        });
                        
                        // Catégoriser
                        var categoryName = GetCategory(extension);
                        if (!categories.ContainsKey(categoryName))
                        {
                            categories[categoryName] = new DiskUsageCategory { Name = categoryName };
                        }
                        
                        categories[categoryName].TotalSize += fileInfo.Length;
                        categories[categoryName].FileCount++;
                        
                        result.TotalScannedSize += fileInfo.Length;
                        result.TotalScannedFiles++;
                        
                        if (result.TotalScannedFiles % 1000 == 0)
                        {
                            progress?.Invoke($"Analysés: {result.TotalScannedFiles} fichiers...");
                        }
                    }
                    catch { /* Ignorer les fichiers inaccessibles */ }
                }
                
                // Scanner les sous-dossiers récursivement
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    
                    // Ignorer certains dossiers système
                    var dirName = Path.GetFileName(dir).ToLower();
                    if (dirName == "system volume information" || 
                        dirName == "$recycle.bin" ||
                        dirName == "windows" && path.Length <= 3) // Ignorer C:\Windows mais pas les sous-dossiers
                        continue;
                    
                    try
                    {
                        ScanDirectory(dir, largeFiles, categories, result, progress, cancellationToken);
                    }
                    catch { /* Ignorer les dossiers inaccessibles */ }
                }
            }
            catch { /* Ignorer les erreurs d'accès */ }
        }
        
        private static string GetCategory(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "Autres";
            
            if (CategoryMappings.TryGetValue(extension, out var category))
                return category;
            
            return "Autres";
        }
        
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Trouve les dossiers les plus volumineux dans un chemin donné
        /// </summary>
        public static List<(string Path, long Size)> FindLargestDirectories(
            string rootPath, 
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            var directorySizes = new Dictionary<string, long>();
            
            try
            {
                CalculateDirectorySizes(rootPath, directorySizes, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur calcul tailles dossiers: {ex.Message}");
            }
            
            return directorySizes
                .OrderByDescending(kvp => kvp.Value)
                .Take(count)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }
        
        private static long CalculateDirectorySizes(
            string path,
            Dictionary<string, long> directorySizes,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return 0;
            
            long totalSize = 0;
            
            try
            {
                // Calculer la taille des fichiers dans ce dossier
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    try
                    {
                        totalSize += new FileInfo(file).Length;
                    }
                    catch { }
                }
                
                // Calculer récursivement pour les sous-dossiers
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir).ToLower();
                    if (dirName == "system volume information" || dirName == "$recycle.bin")
                        continue;
                    
                    try
                    {
                        totalSize += CalculateDirectorySizes(dir, directorySizes, cancellationToken);
                    }
                    catch { }
                }
                
                directorySizes[path] = totalSize;
            }
            catch { }
            
            return totalSize;
        }
    }
}
