using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsCleaner
{
    /// <summary>
    /// Gestionnaire d'alertes intelligentes pour notifications proactives
    /// </summary>
    public static class SmartAlerts
    {
        private const long LOW_DISK_SPACE_THRESHOLD = 10L * 1024 * 1024 * 1024; // 10 GB
        private const long BROWSER_CACHE_THRESHOLD = 2L * 1024 * 1024 * 1024; // 2 GB
        private const int DAYS_SINCE_LAST_CLEAN = 7;
        
        /// <summary>
        /// Vérifie l'espace disque disponible sur tous les lecteurs
        /// </summary>
        public static (bool AlertNeeded, string Message) CheckDiskSpace()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
                
                foreach (var drive in drives)
                {
                    var freeSpacePercent = (drive.AvailableFreeSpace * 100.0) / drive.TotalSize;
                    
                    if (drive.AvailableFreeSpace < LOW_DISK_SPACE_THRESHOLD || freeSpacePercent < 10)
                    {
                        var message = $"⚠️ Espace disque faible sur {drive.Name}\n" +
                                     $"Disponible: {FormatBytes(drive.AvailableFreeSpace)} ({freeSpacePercent:F1}%)\n" +
                                     $"Total: {FormatBytes(drive.TotalSize)}\n\n" +
                                     $"Un nettoyage est recommandé pour libérer de l'espace.";
                        
                        return (true, message);
                    }
                }
                
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur vérification espace disque: {ex.Message}");
                return (false, string.Empty);
            }
        }
        
        /// <summary>
        /// Vérifie la taille des caches navigateurs
        /// </summary>
        public static (bool AlertNeeded, string Message) CheckBrowserCaches()
        {
            try
            {
                long totalCacheSize = 0;
                
                // Chrome
                if (Directory.Exists(BrowserPaths.ChromeCache))
                {
                    totalCacheSize += GetDirectorySize(BrowserPaths.ChromeCache);
                }
                
                // Edge
                if (Directory.Exists(BrowserPaths.EdgeCache))
                {
                    totalCacheSize += GetDirectorySize(BrowserPaths.EdgeCache);
                }
                
                // Firefox
                if (BrowserPaths.IsFirefoxInstalled)
                {
                    var profiles = Directory.GetDirectories(BrowserPaths.FirefoxProfiles);
                    foreach (var profile in profiles)
                    {
                        var cacheDir = BrowserPaths.GetFirefoxCache(profile);
                        if (Directory.Exists(cacheDir))
                        {
                            totalCacheSize += GetDirectorySize(cacheDir);
                        }
                    }
                }
                
                if (totalCacheSize > BROWSER_CACHE_THRESHOLD)
                {
                    var message = $"🌐 Cache des navigateurs volumineux\n" +
                                 $"Taille totale: {FormatBytes(totalCacheSize)}\n\n" +
                                 $"Le nettoyage des caches navigateurs peut libérer de l'espace significatif.";
                    
                    return (true, message);
                }
                
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur vérification caches navigateurs: {ex.Message}");
                return (false, string.Empty);
            }
        }
        
        /// <summary>
        /// Vérifie si un nettoyage régulier est nécessaire
        /// </summary>
        public static (bool AlertNeeded, string Message) CheckLastCleaningDate()
        {
            try
            {
                var stats = StatisticsManager.LoadAllStatistics()
                    .Where(s => !s.WasDryRun)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefault();
                
                if (stats == null)
                {
                    var message = $"🧹 Aucun nettoyage enregistré\n\n" +
                                 $"Il est recommandé d'effectuer un nettoyage régulier pour maintenir les performances de votre système.";
                    
                    return (true, message);
                }
                
                var daysSinceLastClean = (DateTime.Now - stats.Timestamp).TotalDays;
                
                if (daysSinceLastClean > DAYS_SINCE_LAST_CLEAN)
                {
                    var message = $"🕒 Dernier nettoyage il y a {(int)daysSinceLastClean} jours\n" +
                                 $"Date: {stats.Timestamp:dd/MM/yyyy à HH:mm}\n\n" +
                                 $"Un nettoyage régulier (hebdomadaire) est recommandé pour maintenir les performances.";
                    
                    return (true, message);
                }
                
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur vérification date nettoyage: {ex.Message}");
                return (false, string.Empty);
            }
        }
        
        /// <summary>
        /// Vérifie la taille des fichiers temporaires
        /// </summary>
        public static (bool AlertNeeded, string Message) CheckTempFiles()
        {
            try
            {
                long totalTempSize = 0;
                
                // User Temp
                var userTemp = BrowserPaths.UserTemp;
                if (Directory.Exists(userTemp))
                {
                    totalTempSize += GetDirectorySize(userTemp);
                }
                
                // LocalAppData Temp
                var localTemp = BrowserPaths.LocalAppDataTemp;
                if (Directory.Exists(localTemp))
                {
                    totalTempSize += GetDirectorySize(localTemp);
                }
                
                if (totalTempSize > 1L * 1024 * 1024 * 1024) // > 1 GB
                {
                    var message = $"📁 Fichiers temporaires volumineux\n" +
                                 $"Taille totale: {FormatBytes(totalTempSize)}\n\n" +
                                 $"Le nettoyage des fichiers temporaires peut améliorer les performances.";
                    
                    return (true, message);
                }
                
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur vérification fichiers temp: {ex.Message}");
                return (false, string.Empty);
            }
        }
        
        /// <summary>
        /// Effectue toutes les vérifications et affiche une alerte si nécessaire
        /// </summary>
        public static void PerformAllChecksAndAlert()
        {
            var alerts = new[]
            {
                CheckDiskSpace(),
                CheckBrowserCaches(),
                CheckLastCleaningDate(),
                CheckTempFiles()
            };
            
            var alertsNeeded = alerts.Where(a => a.AlertNeeded).ToList();
            
            if (alertsNeeded.Any())
            {
                var combinedMessage = string.Join("\n\n" + new string('-', 50) + "\n\n", 
                    alertsNeeded.Select(a => a.Message));
                
                var result = MessageBox.Show(
                    combinedMessage + "\n\n" + "Voulez-vous lancer Windows Cleaner maintenant ?",
                    "Windows Cleaner - Alertes Système",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                
                if (result == DialogResult.Yes)
                {
                    // L'application est déjà lancée, on informe juste
                    Logger.Log(LogLevel.Info, "Alertes affichées, utilisateur a choisi de nettoyer");
                }
            }
        }
        
        /// <summary>
        /// Génère un rapport de recommandations
        /// </summary>
        public static string GenerateRecommendations()
        {
            var recommendations = new System.Text.StringBuilder();
            recommendations.AppendLine("📋 RECOMMANDATIONS DE NETTOYAGE");
            recommendations.AppendLine(new string('=', 50));
            recommendations.AppendLine();
            
            var diskCheck = CheckDiskSpace();
            if (diskCheck.AlertNeeded)
            {
                recommendations.AppendLine("❗ PRIORITÉ HAUTE - Espace disque");
                recommendations.AppendLine(diskCheck.Message);
                recommendations.AppendLine();
            }
            
            var cacheCheck = CheckBrowserCaches();
            if (cacheCheck.AlertNeeded)
            {
                recommendations.AppendLine("⚠️ PRIORITÉ MOYENNE - Caches navigateurs");
                recommendations.AppendLine(cacheCheck.Message);
                recommendations.AppendLine();
            }
            
            var tempCheck = CheckTempFiles();
            if (tempCheck.AlertNeeded)
            {
                recommendations.AppendLine("⚠️ PRIORITÉ MOYENNE - Fichiers temporaires");
                recommendations.AppendLine(tempCheck.Message);
                recommendations.AppendLine();
            }
            
            var cleanCheck = CheckLastCleaningDate();
            if (cleanCheck.AlertNeeded)
            {
                recommendations.AppendLine("ℹ️ PRIORITÉ BASSE - Maintenance régulière");
                recommendations.AppendLine(cleanCheck.Message);
                recommendations.AppendLine();
            }
            
            if (!diskCheck.AlertNeeded && !cacheCheck.AlertNeeded && !tempCheck.AlertNeeded)
            {
                recommendations.AppendLine("✅ Aucune action urgente requise");
                recommendations.AppendLine("Votre système semble en bon état.");
            }
            
            return recommendations.ToString();
        }
        
        private static long GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            }
            catch
            {
                return 0;
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
