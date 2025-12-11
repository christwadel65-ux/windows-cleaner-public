using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WindowsCleaner
{
    /// <summary>
    /// Statistiques d'une session de nettoyage
    /// </summary>
    public class CleaningStatistics
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ProfileUsed { get; set; } = "Manuel";
        public int FilesDeleted { get; set; }
        public long BytesFreed { get; set; }
        public TimeSpan Duration { get; set; }
        public bool WasDryRun { get; set; }
        
        // App cache cleaning stats
        public int VsCodeCacheFilesDeleted { get; set; }
        public int NugetCacheFilesDeleted { get; set; }
        public int MavenCacheFilesDeleted { get; set; }
        public int NpmCacheFilesDeleted { get; set; }
        public int GameCachesFilesDeleted { get; set; }
        public long AppCachesBytesFreed { get; set; }
        
        // SSD optimization stats
        public bool SsdOptimized { get; set; }
        public bool DiskHealthChecked { get; set; }
        public string DiskHealthReport { get; set; } = string.Empty;
        
        public string FormattedSize => FormatBytes(BytesFreed);
        public string FormattedAppCachesSize => FormatBytes(AppCachesBytesFreed);
        public int TotalCachesDeleted => VsCodeCacheFilesDeleted + NugetCacheFilesDeleted + 
                                          MavenCacheFilesDeleted + NpmCacheFilesDeleted + 
                                          GameCachesFilesDeleted;
        
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
    /// Gestionnaire de statistiques et rapports
    /// </summary>
    public static class StatisticsManager
    {
        private static readonly string StatsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WindowsCleaner",
            "Statistics"
        );
        
        private static readonly string StatsFile = Path.Combine(StatsDirectory, "cleaning_stats.json");
        
        static StatisticsManager()
        {
            if (!Directory.Exists(StatsDirectory))
            {
                Directory.CreateDirectory(StatsDirectory);
            }
        }
        
        /// <summary>
        /// Enregistre une session de nettoyage
        /// </summary>
        public static void RecordCleaningSession(CleaningStatistics stats)
        {
            try
            {
                var allStats = LoadAllStatistics();
                allStats.Add(stats);
                
                var json = JsonSerializer.Serialize(allStats, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(StatsFile, json);
                Logger.Log(LogLevel.Info, $"Statistiques enregistrées: {stats.FilesDeleted} fichiers, {stats.FormattedSize}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur enregistrement statistiques: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Charge toutes les statistiques
        /// </summary>
        public static List<CleaningStatistics> LoadAllStatistics()
        {
            try
            {
                if (!File.Exists(StatsFile))
                    return new List<CleaningStatistics>();
                
                var json = File.ReadAllText(StatsFile);
                return JsonSerializer.Deserialize<List<CleaningStatistics>>(json) 
                       ?? new List<CleaningStatistics>();
            }
            catch
            {
                return new List<CleaningStatistics>();
            }
        }
        
        /// <summary>
        /// Obtient les statistiques des derniers jours
        /// </summary>
        public static List<CleaningStatistics> GetRecentStatistics(int days = 30)
        {
            var allStats = LoadAllStatistics();
            var cutoff = DateTime.Now.AddDays(-days);
            
            return allStats
                .Where(s => s.Timestamp >= cutoff && !s.WasDryRun)
                .OrderByDescending(s => s.Timestamp)
                .ToList();
        }
        
        /// <summary>
        /// Calcule le total d'espace libéré
        /// </summary>
        public static long GetTotalBytesFreed(int days = 0)
        {
            var stats = days > 0 
                ? GetRecentStatistics(days)
                : LoadAllStatistics().Where(s => !s.WasDryRun).ToList();
            
            return stats.Sum(s => s.BytesFreed);
        }
        
        /// <summary>
        /// Calcule le nombre total de fichiers supprimés
        /// </summary>
        public static int GetTotalFilesDeleted(int days = 0)
        {
            var stats = days > 0 
                ? GetRecentStatistics(days)
                : LoadAllStatistics().Where(s => !s.WasDryRun).ToList();
            
            return stats.Sum(s => s.FilesDeleted);
        }
        
        /// <summary>
        /// Obtient le nombre de sessions de nettoyage
        /// </summary>
        public static int GetTotalSessions(int days = 0)
        {
            var stats = days > 0 
                ? GetRecentStatistics(days)
                : LoadAllStatistics().Where(s => !s.WasDryRun).ToList();
            
            return stats.Count;
        }
        
        /// <summary>
        /// Génère un rapport HTML des statistiques
        /// </summary>
        public static string GenerateHtmlReport()
        {
            var allStats = LoadAllStatistics().Where(s => !s.WasDryRun).ToList();
            var last30Days = GetRecentStatistics(30);
            
            var totalBytes = GetTotalBytesFreed();
            var totalFiles = GetTotalFilesDeleted();
            var totalSessions = GetTotalSessions();
            
            var last30Bytes = last30Days.Sum(s => s.BytesFreed);
            var last30Files = last30Days.Sum(s => s.FilesDeleted);
            
            // Statistiques App Cache
            var totalAppCacheFiles = allStats.Sum(s => s.TotalCachesDeleted);
            var totalAppCacheBytes = allStats.Sum(s => s.AppCachesBytesFreed);
            var vsCodeTotal = allStats.Sum(s => s.VsCodeCacheFilesDeleted);
            var nugetTotal = allStats.Sum(s => s.NugetCacheFilesDeleted);
            var mavenTotal = allStats.Sum(s => s.MavenCacheFilesDeleted);
            var npmTotal = allStats.Sum(s => s.NpmCacheFilesDeleted);
            var gameTotal = allStats.Sum(s => s.GameCachesFilesDeleted);
            
            // Statistiques SSD
            var ssdOptimizationCount = allStats.Count(s => s.SsdOptimized);
            var diskHealthCheckCount = allStats.Count(s => s.DiskHealthChecked);
            var lastSmartReport = allStats.LastOrDefault(s => !string.IsNullOrEmpty(s.DiskHealthReport));
            
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Windows Cleaner - Rapport Statistiques</title>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 40px;
            background: #f5f5f5;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        h1 {{
            color: #0078d4;
            border-bottom: 3px solid #0078d4;
            padding-bottom: 10px;
        }}
        .stats-grid {{
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
            margin: 30px 0;
        }}
        .stat-card {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 25px;
            border-radius: 8px;
            text-align: center;
        }}
        .stat-card h3 {{
            margin: 0 0 10px 0;
            font-size: 14px;
            opacity: 0.9;
        }}
        .stat-card .value {{
            font-size: 32px;
            font-weight: bold;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 30px;
        }}
        th, td {{
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }}
        th {{
            background: #0078d4;
            color: white;
        }}
        tr:hover {{
            background: #f5f5f5;
        }}
        .footer {{
            margin-top: 30px;
            text-align: center;
            color: #666;
            font-size: 12px;
        }}
        .cache-section {{
            background: #f0f4ff;
            border-left: 4px solid #667eea;
            padding: 20px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .cache-grid {{
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 15px;
            margin-top: 15px;
        }}
        .cache-item {{
            background: white;
            padding: 10px;
            border-radius: 3px;
            border-left: 3px solid #667eea;
        }}
        .ssd-section {{
            background: #fff3f0;
            border-left: 4px solid #f5576c;
            padding: 20px;
            margin: 20px 0;
            border-radius: 5px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>📊 Rapport Statistiques - Windows Cleaner</h1>
        <p>Généré le: {DateTime.Now:dd/MM/yyyy à HH:mm:ss}</p>
        
        <h2>Statistiques Globales</h2>
        <div class=""stats-grid"">
            <div class=""stat-card"">
                <h3>ESPACE TOTAL LIBÉRÉ</h3>
                <div class=""value"">{FormatBytes(totalBytes)}</div>
            </div>
            <div class=""stat-card"">
                <h3>FICHIERS SUPPRIMÉS</h3>
                <div class=""value"">{totalFiles:N0}</div>
            </div>
            <div class=""stat-card"">
                <h3>SESSIONS DE NETTOYAGE</h3>
                <div class=""value"">{totalSessions}</div>
            </div>
        </div>
        
        <h2>Derniers 30 Jours</h2>
        <div class=""stats-grid"">
            <div class=""stat-card"" style=""background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);"">
                <h3>ESPACE LIBÉRÉ</h3>
                <div class=""value"">{FormatBytes(last30Bytes)}</div>
            </div>
            <div class=""stat-card"" style=""background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);"">
                <h3>FICHIERS SUPPRIMÉS</h3>
                <div class=""value"">{last30Files:N0}</div>
            </div>
            <div class=""stat-card"" style=""background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);"">
                <h3>NETTOYAGES</h3>
                <div class=""value"">{last30Days.Count}</div>
            </div>
        </div>
        
        <h2>🗂️ Nettoyage des Caches Applicatifs</h2>
        <div class=""cache-section"">
            <h3 style=""margin-top: 0;"">Statistiques Globales</h3>
            <div class=""cache-grid"">
                <div class=""cache-item"">
                    <strong>Total Fichiers Cache:</strong> {totalAppCacheFiles:N0}
                </div>
                <div class=""cache-item"">
                    <strong>Espace Libéré:</strong> {FormatBytes(totalAppCacheBytes)}
                </div>
            </div>
            
            <h3 style=""margin-top: 20px;"">Détails par Source</h3>
            <div class=""cache-grid"">
                <div class=""cache-item"">
                    <strong>VS Code:</strong> {vsCodeTotal:N0} fichiers
                </div>
                <div class=""cache-item"">
                    <strong>NuGet:</strong> {nugetTotal:N0} fichiers
                </div>
                <div class=""cache-item"">
                    <strong>Maven:</strong> {mavenTotal:N0} fichiers
                </div>
                <div class=""cache-item"">
                    <strong>npm:</strong> {npmTotal:N0} fichiers
                </div>
                <div class=""cache-item"">
                    <strong>Jeux (Steam/Epic):</strong> {gameTotal:N0} fichiers
                </div>
            </div>
        </div>
        
        <h2>💿 Optimisation SSD</h2>
        <div class=""ssd-section"">
            <div style=""display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px;"">
                <div>
                    <strong>Optimisations TRIM:</strong> {ssdOptimizationCount} session(s)
                </div>
                <div>
                    <strong>Vérifications SMART:</strong> {diskHealthCheckCount} session(s)
                </div>
            </div>";
            
            // Ajouter le dernier rapport SMART s'il existe
            if (lastSmartReport != null && !string.IsNullOrEmpty(lastSmartReport.DiskHealthReport))
            {
                var escapedReport = System.Web.HttpUtility.HtmlEncode(lastSmartReport.DiskHealthReport);
                html += $@"
            <h3 style=""margin-top: 20px;"">Dernier Rapport SMART</h3>
            <div style=""background: white; padding: 10px; border-radius: 3px; font-family: monospace; font-size: 12px; white-space: pre-wrap; overflow-x: auto; max-height: 200px;"">
{escapedReport}
            </div>";
            }
            
            html += @"
        </div>
        
        <h2>📋 Historique des Sessions</h2>
        <table>
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Profil</th>
                    <th>Fichiers</th>
                    <th>Espace Libéré</th>
                    <th>App Cache</th>
                    <th>SSD</th>
                    <th>Durée</th>
                </tr>
            </thead>
            <tbody>
";

            foreach (var stat in allStats.OrderByDescending(s => s.Timestamp).Take(50))
            {
                var appCacheIndicator = stat.TotalCachesDeleted > 0 ? $"✓ ({stat.TotalCachesDeleted})" : "-";
                var ssdIndicator = stat.SsdOptimized ? "✓" : "-";
                
                html += $@"
                <tr>
                    <td>{stat.Timestamp:dd/MM/yyyy HH:mm}</td>
                    <td>{stat.ProfileUsed}</td>
                    <td>{stat.FilesDeleted:N0}</td>
                    <td>{stat.FormattedSize}</td>
                    <td>{appCacheIndicator}</td>
                    <td>{ssdIndicator}</td>
                    <td>{stat.Duration.TotalSeconds:0.0}s</td>
                </tr>
";
            }

            html += @"
            </tbody>
        </table>
        
        <div class=""footer"">
            <p>Windows Cleaner v1.0.8 - © 2025</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
        
        /// <summary>
        /// Exporte le rapport HTML vers un fichier
        /// </summary>
        public static string ExportHtmlReport()
        {
            try
            {
                var html = GenerateHtmlReport();
                var fileName = $"WindowsCleaner_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                
                File.WriteAllText(filePath, html);
                Logger.Log(LogLevel.Info, $"Rapport HTML exporté: {filePath}");
                
                return filePath;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur export rapport HTML: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Efface toutes les statistiques
        /// </summary>
        public static void ClearAllStatistics()
        {
            try
            {
                if (File.Exists(StatsFile))
                {
                    File.Delete(StatsFile);
                    Logger.Log(LogLevel.Info, "Statistiques effacées");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur effacement statistiques: {ex.Message}");
            }
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
