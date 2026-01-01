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
        
        /// <summary>
        /// Génère un rapport HTML professionnel de l'analyse disque
        /// </summary>
        public static string GenerateHtmlReport(DiskAnalysisResult result, string rootPath)
        {
            var sb = new System.Text.StringBuilder();
            
            // En-tête HTML
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"fr\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <title>Analyse d'Espace Disque - Windows Cleaner</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        * { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #333; padding: 20px; }");
            sb.AppendLine("        .container { max-width: 1400px; margin: 0 auto; background: white; border-radius: 15px; box-shadow: 0 20px 60px rgba(0,0,0,0.3); overflow: hidden; }");
            sb.AppendLine("        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }");
            sb.AppendLine("        .header h1 { font-size: 2.5em; margin-bottom: 10px; }");
            sb.AppendLine("        .header p { font-size: 1.1em; opacity: 0.9; }");
            sb.AppendLine("        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; padding: 30px; background: #f8f9fa; }");
            sb.AppendLine("        .summary-card { background: white; padding: 25px; border-radius: 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); text-align: center; transition: transform 0.3s; }");
            sb.AppendLine("        .summary-card:hover { transform: translateY(-5px); box-shadow: 0 8px 25px rgba(0,0,0,0.15); }");
            sb.AppendLine("        .summary-card h3 { color: #667eea; font-size: 2.5em; margin-bottom: 10px; }");
            sb.AppendLine("        .summary-card p { color: #666; font-size: 1.1em; }");
            sb.AppendLine("        .content { padding: 30px; }");
            sb.AppendLine("        .section { margin-bottom: 40px; }");
            sb.AppendLine("        .section h2 { color: #667eea; font-size: 1.8em; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 3px solid #667eea; }");
            sb.AppendLine("        .category-list { display: grid; gap: 15px; }");
            sb.AppendLine("        .category-item { background: #f8f9fa; padding: 20px; border-radius: 10px; display: flex; justify-content: space-between; align-items: center; transition: all 0.3s; }");
            sb.AppendLine("        .category-item:hover { background: #e9ecef; transform: translateX(5px); }");
            sb.AppendLine("        .category-info { flex: 1; }");
            sb.AppendLine("        .category-name { font-weight: bold; font-size: 1.2em; color: #333; margin-bottom: 5px; }");
            sb.AppendLine("        .category-details { color: #666; font-size: 0.95em; }");
            sb.AppendLine("        .progress-bar { flex: 0 0 200px; margin-left: 20px; }");
            sb.AppendLine("        .progress-bg { background: #e9ecef; height: 30px; border-radius: 15px; overflow: hidden; position: relative; }");
            sb.AppendLine("        .progress-fill { background: linear-gradient(90deg, #667eea 0%, #764ba2 100%); height: 100%; display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 0.9em; transition: width 0.5s; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
            sb.AppendLine("        thead { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; }");
            sb.AppendLine("        th { padding: 15px; text-align: left; font-weight: 600; }");
            sb.AppendLine("        td { padding: 12px 15px; border-bottom: 1px solid #e9ecef; }");
            sb.AppendLine("        tbody tr:hover { background: #f8f9fa; }");
            sb.AppendLine("        .file-path { color: #667eea; word-break: break-all; }");
            sb.AppendLine("        .file-size { font-weight: bold; color: #333; }");
            sb.AppendLine("        .footer { background: #f8f9fa; padding: 20px; text-align: center; color: #666; font-size: 0.95em; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            sb.AppendLine("    <div class=\"container\">");
            
            // Header
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <h1>Analyse d'Espace Disque</h1>");
            sb.AppendLine($"            <p>Dossier analys\u00e9 : {HtmlEncode(rootPath)}</p>");
            sb.AppendLine("        </div>");
            
            // Summary cards
            sb.AppendLine("        <div class=\"summary\">");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{result.TotalScannedFiles:N0}</h3>");
            sb.AppendLine("                <p>Fichiers analys\u00e9s</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{FormatBytes(result.TotalScannedSize)}</h3>");
            sb.AppendLine("                <p>Espace total</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{result.Categories.Count}</h3>");
            sb.AppendLine("                <p>Cat\u00e9gories</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"summary-card\">");
            sb.AppendLine($"                <h3>{result.ScanDuration.TotalSeconds:F1}s</h3>");
            sb.AppendLine("                <p>Dur\u00e9e du scan</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
            
            sb.AppendLine("        <div class=\"content\">");
            
            // Categories
            if (result.Categories.Count > 0)
            {
                sb.AppendLine("            <div class=\"section\">");
                sb.AppendLine("                <h2>R\u00e9partition par Cat\u00e9gorie</h2>");
                sb.AppendLine("                <div class=\"category-list\">");
                
                foreach (var category in result.Categories.Take(15))
                {
                    sb.AppendLine("                    <div class=\"category-item\">");
                    sb.AppendLine("                        <div class=\"category-info\">");
                    sb.AppendLine($"                            <div class=\"category-name\">{HtmlEncode(category.Name)}</div>");
                    sb.AppendLine($"                            <div class=\"category-details\">{category.FormattedSize} - {category.FileCount:N0} fichiers</div>");
                    sb.AppendLine("                        </div>");
                    sb.AppendLine("                        <div class=\"progress-bar\">");
                    sb.AppendLine("                            <div class=\"progress-bg\">");
                    sb.AppendLine($"                                <div class=\"progress-fill\" style=\"width: {category.Percentage:F1}%\">");
                    sb.AppendLine($"                                    {category.Percentage:F1}%");
                    sb.AppendLine("                                </div>");
                    sb.AppendLine("                            </div>");
                    sb.AppendLine("                        </div>");
                    sb.AppendLine("                    </div>");
                }
                
                sb.AppendLine("                </div>");
                sb.AppendLine("            </div>");
            }
            
            // Largest files
            if (result.LargestFiles.Count > 0)
            {
                sb.AppendLine("            <div class=\"section\">");
                sb.AppendLine("                <h2>Fichiers les Plus Volumineux</h2>");
                sb.AppendLine("                <table>");
                sb.AppendLine("                    <thead>");
                sb.AppendLine("                        <tr>");
                sb.AppendLine("                            <th>Fichier</th>");
                sb.AppendLine("                            <th>Taille</th>");
                sb.AppendLine("                            <th>Type</th>");
                sb.AppendLine("                            <th>Derni\u00e8re modification</th>");
                sb.AppendLine("                        </tr>");
                sb.AppendLine("                    </thead>");
                sb.AppendLine("                    <tbody>");
                
                foreach (var file in result.LargestFiles.Take(50))
                {
                    sb.AppendLine("                        <tr>");
                    sb.AppendLine($"                            <td class=\"file-path\">{HtmlEncode(file.Path)}</td>");
                    sb.AppendLine($"                            <td class=\"file-size\">{file.FormattedSize}</td>");
                    sb.AppendLine($"                            <td>{HtmlEncode(file.Extension.ToUpper())}</td>");
                    sb.AppendLine($"                            <td>{file.LastModified:yyyy-MM-dd HH:mm}</td>");
                    sb.AppendLine("                        </tr>");
                }
                
                sb.AppendLine("                    </tbody>");
                sb.AppendLine("                </table>");
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine("        </div>");
            
            // Footer
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine($"            <p>Rapport g\u00e9n\u00e9r\u00e9 le {DateTime.Now:dd/MM/yyyy \u00e0 HH:mm:ss} par Windows Cleaner</p>");
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
        public static string ExportHtmlReport(DiskAnalysisResult result, string rootPath)
        {
            var html = GenerateHtmlReport(result, rootPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"DiskAnalysis_{timestamp}.html";
            var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            
            File.WriteAllText(filepath, html, System.Text.Encoding.UTF8);
            
            return filepath;
        }
    }
}
