using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleaner
{
    /// <summary>
    /// Information sur un fichier dupliqué
    /// </summary>
    public class DuplicateFileInfo
    {
        public string Path { get; set; } = "";
        public long Size { get; set; }
        public string Hash { get; set; } = "";
        public DateTime LastModified { get; set; }
        
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
    /// Groupe de fichiers dupliqués
    /// </summary>
    public class DuplicateGroup
    {
        public string Hash { get; set; } = "";
        public List<DuplicateFileInfo> Files { get; } = new List<DuplicateFileInfo>();
        public long TotalWastedSpace => Files.Count > 1 ? Files[0].Size * (Files.Count - 1) : 0;
        
        public string FormattedWastedSpace => FormatBytes(TotalWastedSpace);
        
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
    /// Résultat de la recherche de doublons
    /// </summary>
    public class DuplicateSearchResult
    {
        public List<DuplicateGroup> DuplicateGroups { get; } = new List<DuplicateGroup>();
        public int TotalDuplicates => DuplicateGroups.Sum(g => g.Files.Count - 1);
        public long TotalWastedSpace => DuplicateGroups.Sum(g => g.TotalWastedSpace);
        public int TotalScannedFiles { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }
    
    /// <summary>
    /// Détecteur de fichiers dupliqués par hash
    /// </summary>
    public static class DuplicateFinder
    {
        /// <summary>
        /// Recherche les fichiers dupliqués dans un dossier
        /// </summary>
        public static async Task<DuplicateSearchResult> FindDuplicates(
            string rootPath,
            long minFileSize = 1024, // 1 KB minimum par défaut
            string[]? extensions = null,
            Action<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.Now;
            var result = new DuplicateSearchResult();
            var filesBySize = new Dictionary<long, List<string>>();
            
            progress?.Invoke($"Recherche de fichiers dans: {rootPath}");
            
            // Étape 1: Grouper les fichiers par taille
            await Task.Run(() =>
            {
                try
                {
                    GroupFilesBySize(rootPath, minFileSize, extensions, filesBySize, result, progress, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Erreur lors du scan: {ex.Message}");
                }
            }, cancellationToken);
            
            progress?.Invoke($"Fichiers trouvés: {result.TotalScannedFiles}. Calcul des hash...");
            
            // Étape 2: Calculer les hash uniquement pour les fichiers de même taille
            var hashGroups = new Dictionary<string, List<DuplicateFileInfo>>();
            int processed = 0;
            
            foreach (var sizeGroup in filesBySize.Where(kvp => kvp.Value.Count > 1))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                foreach (var filePath in sizeGroup.Value)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    
                    try
                    {
                        var hash = await ComputeFileHashAsync(filePath, cancellationToken);
                        
                        if (!hashGroups.ContainsKey(hash))
                        {
                            hashGroups[hash] = new List<DuplicateFileInfo>();
                        }
                        
                        hashGroups[hash].Add(new DuplicateFileInfo
                        {
                            Path = filePath,
                            Size = sizeGroup.Key,
                            Hash = hash,
                            LastModified = File.GetLastWriteTime(filePath)
                        });
                        
                        processed++;
                        if (processed % 50 == 0)
                        {
                            progress?.Invoke($"Hash calculés: {processed} fichiers...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Erreur calcul hash {filePath}: {ex.Message}");
                    }
                }
            }
            
            // Étape 3: Créer les groupes de doublons
            foreach (var hashGroup in hashGroups.Where(kvp => kvp.Value.Count > 1))
            {
                var duplicateGroup = new DuplicateGroup
                {
                    Hash = hashGroup.Key
                };
                
                duplicateGroup.Files.AddRange(hashGroup.Value);
                result.DuplicateGroups.Add(duplicateGroup);
            }
            
            result.SearchDuration = DateTime.Now - startTime;
            progress?.Invoke($"Recherche terminée: {result.TotalDuplicates} doublons trouvés, {FormatBytes(result.TotalWastedSpace)} récupérables");
            
            return result;
        }
        
        private static void GroupFilesBySize(
            string path,
            long minFileSize,
            string[]? extensions,
            Dictionary<long, List<string>> filesBySize,
            DuplicateSearchResult result,
            Action<string>? progress,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            
            try
            {
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        
                        // Filtrer par taille minimale
                        if (fileInfo.Length < minFileSize)
                            continue;
                        
                        // Filtrer par extension si spécifié
                        if (extensions != null && extensions.Length > 0)
                        {
                            var ext = fileInfo.Extension.ToLower();
                            if (!extensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                                continue;
                        }
                        
                        if (!filesBySize.ContainsKey(fileInfo.Length))
                        {
                            filesBySize[fileInfo.Length] = new List<string>();
                        }
                        
                        filesBySize[fileInfo.Length].Add(file);
                        result.TotalScannedFiles++;
                        
                        if (result.TotalScannedFiles % 1000 == 0)
                        {
                            progress?.Invoke($"Scannés: {result.TotalScannedFiles} fichiers...");
                        }
                    }
                    catch { /* Ignorer les fichiers inaccessibles */ }
                }
                
                // Scanner les sous-dossiers
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    
                    var dirName = Path.GetFileName(dir).ToLower();
                    if (dirName == "system volume information" || dirName == "$recycle.bin")
                        continue;
                    
                    try
                    {
                        GroupFilesBySize(dir, minFileSize, extensions, filesBySize, result, progress, cancellationToken);
                    }
                    catch { /* Ignorer les dossiers inaccessibles */ }
                }
            }
            catch { /* Ignorer les erreurs d'accès */ }
        }
        
        private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            
            var hashBytes = await md5.ComputeHashAsync(stream, cancellationToken);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        
        /// <summary>
        /// Supprime les fichiers dupliqués sélectionnés
        /// </summary>
        public static int DeleteDuplicates(
            List<DuplicateFileInfo> filesToDelete,
            bool moveToRecycleBin = true,
            Action<string>? log = null)
        {
            int deletedCount = 0;
            
            foreach (var file in filesToDelete)
            {
                try
                {
                    if (moveToRecycleBin)
                    {
                        // Utiliser l'API Shell pour déplacer vers la corbeille
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                            file.Path,
                            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin
                        );
                    }
                    else
                    {
                        File.Delete(file.Path);
                    }
                    
                    deletedCount++;
                    log?.Invoke($"Supprimé: {file.Path}");
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Erreur suppression {file.Path}: {ex.Message}");
                }
            }
            
            return deletedCount;
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
    }
}
