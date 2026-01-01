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
        
        /// <summary>
        /// Génère un rapport HTML professionnel de la recherche de doublons
        /// </summary>
        public static string GenerateHtmlReport(DuplicateSearchResult result, string rootPath)
        {
            var sb = new System.Text.StringBuilder();
            
            // En-tête HTML
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"fr\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <title>D\u00e9tection de Doublons - Windows Cleaner</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        * { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: #333; padding: 20px; }");
            sb.AppendLine("        .container { max-width: 1400px; margin: 0 auto; background: white; border-radius: 15px; box-shadow: 0 20px 60px rgba(0,0,0,0.3); overflow: hidden; }");
            sb.AppendLine("        .header { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; }");
            sb.AppendLine("        .header h1 { font-size: 2.5em; margin-bottom: 10px; }");
            sb.AppendLine("        .header p { font-size: 1.1em; opacity: 0.9; }");
            sb.AppendLine("        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; padding: 30px; background: #f8f9fa; }");
            sb.AppendLine("        .summary-card { background: white; padding: 25px; border-radius: 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); text-align: center; transition: transform 0.3s; }");
            sb.AppendLine("        .summary-card:hover { transform: translateY(-5px); box-shadow: 0 8px 25px rgba(0,0,0,0.15); }");
            sb.AppendLine("        .summary-card.highlight { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; }");
            sb.AppendLine("        .summary-card h3 { font-size: 2.5em; margin-bottom: 10px; }");
            sb.AppendLine("        .summary-card p { font-size: 1.1em; }");
            sb.AppendLine("        .summary-card.highlight p { color: rgba(255,255,255,0.9); }");
            sb.AppendLine("        .content { padding: 30px; }");
            sb.AppendLine("        .alert { background: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin-bottom: 30px; border-radius: 5px; }");
            sb.AppendLine("        .alert h3 { color: #856404; margin-bottom: 10px; }");
            sb.AppendLine("        .alert p { color: #856404; }");
            sb.AppendLine("        .section { margin-bottom: 40px; }");
            sb.AppendLine("        .section h2 { color: #f5576c; font-size: 1.8em; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 3px solid #f5576c; }");
            sb.AppendLine("        .duplicate-group { background: #f8f9fa; padding: 20px; border-radius: 10px; margin-bottom: 20px; border-left: 5px solid #f5576c; }");
            sb.AppendLine("        .duplicate-group-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }");
            sb.AppendLine("        .group-title { font-weight: bold; font-size: 1.2em; color: #333; }");
            sb.AppendLine("        .group-stats { color: #666; font-size: 0.95em; }");
            sb.AppendLine("        .duplicate-files { display: grid; gap: 10px; }");
            sb.AppendLine("        .file-item { background: white; padding: 15px; border-radius: 5px; display: flex; justify-content: space-between; align-items: center; transition: all 0.3s; }");
            sb.AppendLine("        .file-item:hover { background: #e9ecef; transform: translateX(5px); }");
            sb.AppendLine("        .file-info { flex: 1; }");
            sb.AppendLine("        .file-path { color: #f5576c; word-break: break-all; margin-bottom: 5px; }");
            sb.AppendLine("        .file-details { color: #666; font-size: 0.9em; }");
            sb.AppendLine("        .file-size { font-weight: bold; color: #333; font-size: 1.1em; }");
            sb.AppendLine("        .footer { background: #f8f9fa; padding: 20px; text-align: center; color: #666; font-size: 0.95em; }");
            sb.AppendLine("        .no-duplicates { text-align: center; padding: 60px 20px; }");
            sb.AppendLine("        .no-duplicates h3 { color: #28a745; font-size: 2em; margin-bottom: 10px; }");
            sb.AppendLine("        .no-duplicates p { color: #666; font-size: 1.1em; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            sb.AppendLine("    <div class=\"container\">");
            
            // Header
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <h1>D\u00e9tection de Fichiers Dupliqu\u00e9s</h1>");
            sb.AppendLine($"            <p>Dossier analys\u00e9 : {HtmlEncode(rootPath)}</p>");
            sb.AppendLine("        </div>");
            
            // Summary cards
            sb.AppendLine("        <div class=\"summary\">");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{result.TotalScannedFiles:N0}</h3>");
            sb.AppendLine("                <p>Fichiers scann\u00e9s</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{result.DuplicateGroups.Count:N0}</h3>");
            sb.AppendLine("                <p>Groupes de doublons</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{result.TotalDuplicates:N0}</h3>");
            sb.AppendLine("                <p>Fichiers dupliqu\u00e9s</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"summary-card highlight\">");
            sb.AppendLine($"                <h3>{FormatBytes(result.TotalWastedSpace)}</h3>");
            sb.AppendLine("                <p>Espace gaspill\u00e9</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
            
            sb.AppendLine("        <div class=\"content\">");
            
            // Warning if duplicates found
            if (result.DuplicateGroups.Count > 0)
            {
                sb.AppendLine("            <div class=\"alert\">");
                sb.AppendLine("                <h3>Attention</h3>");
                sb.AppendLine($"                <p>{result.TotalDuplicates} fichiers dupliqu\u00e9s ont \u00e9t\u00e9 d\u00e9tect\u00e9s, repr\u00e9sentant {FormatBytes(result.TotalWastedSpace)} d'espace disque gaspill\u00e9. Vous pouvez supprimer les doublons pour lib\u00e9rer de l'espace.</p>");
                sb.AppendLine("            </div>");
                
                // Duplicate groups
                sb.AppendLine("            <div class=\"section\">");
                sb.AppendLine("                <h2>Groupes de Fichiers Dupliqu\u00e9s</h2>");
                
                var groupIndex = 1;
                foreach (var group in result.DuplicateGroups.Take(100))
                {
                    var totalSize = group.Files.Sum(f => f.Size);
                    var wastedInGroup = totalSize - group.Files.First().Size;
                    
                    sb.AppendLine("                <div class=\"duplicate-group\">");
                    sb.AppendLine("                    <div class=\"duplicate-group-header\">");
                    sb.AppendLine($"                        <div class=\"group-title\">Groupe #{groupIndex}</div>");
                    sb.AppendLine($"                        <div class=\"group-stats\">{group.Files.Count} fichiers - {FormatBytes(group.Files.First().Size)} chacun - {FormatBytes(wastedInGroup)} gaspill\u00e9s</div>");
                    sb.AppendLine("                    </div>");
                    sb.AppendLine("                    <div class=\"duplicate-files\">");
                    
                    foreach (var file in group.Files)
                    {
                        sb.AppendLine("                        <div class=\"file-item\">");
                        sb.AppendLine("                            <div class=\"file-info\">");
                        sb.AppendLine($"                                <div class=\"file-path\">{HtmlEncode(file.Path)}</div>");
                        sb.AppendLine($"                                <div class=\"file-details\">Modifi\u00e9 : {file.LastModified:yyyy-MM-dd HH:mm:ss} | MD5 : {file.Hash.Substring(0, 16)}...</div>");
                        sb.AppendLine("                            </div>");
                        sb.AppendLine($"                            <div class=\"file-size\">{FormatBytes(file.Size)}</div>");
                        sb.AppendLine("                        </div>");
                    }
                    
                    sb.AppendLine("                    </div>");
                    sb.AppendLine("                </div>");
                    
                    groupIndex++;
                }
                
                if (result.DuplicateGroups.Count > 100)
                {
                    sb.AppendLine($"                <p style=\"text-align: center; color: #666; margin-top: 20px;\">... et {result.DuplicateGroups.Count - 100} autres groupes</p>");
                }
                
                sb.AppendLine("            </div>");
            }
            else
            {
                // No duplicates
                sb.AppendLine("            <div class=\"no-duplicates\">");
                sb.AppendLine("                <h3>Aucun doublon d\u00e9tect\u00e9</h3>");
                sb.AppendLine("                <p>Tous les fichiers analys\u00e9s sont uniques. Votre syst\u00e8me est optimis\u00e9 !</p>");
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine("        </div>");
            
            // Footer
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine($"            <p>Rapport g\u00e9n\u00e9r\u00e9 le {DateTime.Now:dd/MM/yyyy \u00e0 HH:mm:ss} par Windows Cleaner</p>");
            sb.AppendLine($"            <p>Dur\u00e9e du scan : {result.SearchDuration.TotalSeconds:F1} secondes</p>");
            sb.AppendLine("        </div>");
            
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Encode les caractères spéciaux HTML
        /// </summary>
        private static string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
                
            return text.Replace("&", "&amp;")
                      .Replace("<", "&lt;")
                      .Replace(">", "&gt;")
                      .Replace("\"", "&quot;")
                      .Replace("'", "&#39;");
        }
        
        /// <summary>
        /// Exporte le rapport HTML vers un fichier
        /// </summary>
        public static string ExportHtmlReport(DuplicateSearchResult result, string rootPath)
        {
            var html = GenerateHtmlReport(result, rootPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"DuplicateFiles_{timestamp}.html";
            var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            
            File.WriteAllText(filepath, html, System.Text.Encoding.UTF8);
            
            return filepath;
        }
    }
}
