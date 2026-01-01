using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace WindowsCleaner.Core
{
    /// <summary>
    /// Configuration du système d'audit automatique
    /// </summary>
    public class AuditConfiguration
    {
        public bool EnableAutomaticAudits { get; set; } = true;
        public AuditSchedule Schedule { get; set; } = new();
        public List<string> EnabledModules { get; set; } = new()
        {
            "DiskSpace",
            "TempFiles",
            "Registry",
            "Startup",
            "Services",
            "BrowserCache",
            "Performance"
        };
        public AuditThresholds Thresholds { get; set; } = new();
        public NotificationSettings Notifications { get; set; } = new();
        public bool AutoArchiveOldReports { get; set; } = true;
        public int MaxHistoryDays { get; set; } = 90;
        public string ReportsDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsCleaner", "AuditReports"
        );
        public bool CreateBackupBeforeAutoFix { get; set; } = true;
        public bool EnableDetailedLogging { get; set; } = true;
        public int MaxConcurrentAudits { get; set; } = 3;
    }

    /// <summary>
    /// Planification des audits automatiques
    /// </summary>
    public class AuditSchedule
    {
        public bool EnableDaily { get; set; } = true;
        public TimeOnly DailyTime { get; set; } = new TimeOnly(9, 0); // 9h00 par défaut
        
        public bool EnableWeekly { get; set; } = false;
        public DayOfWeek WeeklyDay { get; set; } = DayOfWeek.Sunday;
        public TimeOnly WeeklyTime { get; set; } = new TimeOnly(10, 0);
        
        public bool EnableMonthly { get; set; } = false;
        public int MonthlyDay { get; set; } = 1; // Premier jour du mois
        public TimeOnly MonthlyTime { get; set; } = new TimeOnly(10, 0);
        
        public bool RunOnStartup { get; set; } = false;
        public bool RunAfterCleaning { get; set; } = true;
        public bool RunOnSystemIdle { get; set; } = false;
        public int IdleMinutes { get; set; } = 15;
        
        public string CustomCronExpression { get; set; } = string.Empty; // Pour planification avancée
    }

    /// <summary>
    /// Seuils déclenchant des alertes
    /// </summary>
    public class AuditThresholds
    {
        // Score de santé global
        public int MinHealthScore { get; set; } = 70; // Alerte si en dessous
        public int CriticalHealthScore { get; set; } = 50;
        
        // Espace disque
        public int MaxDiskUsagePercent { get; set; } = 85;
        public int CriticalDiskUsagePercent { get; set; } = 95;
        public long MinFreeDiskSpaceGB { get; set; } = 10;
        
        // Fichiers temporaires
        public long MaxTempFilesSizeMB { get; set; } = 5000; // 5 GB
        public int MaxTempFilesCount { get; set; } = 10000;
        
        // Registre
        public int MaxRegistryIssues { get; set; } = 500;
        public int CriticalRegistryIssues { get; set; } = 1000;
        
        // Démarrage
        public int MaxStartupPrograms { get; set; } = 15;
        public int CriticalStartupPrograms { get; set; } = 25;
        
        // Cache navigateurs
        public long MaxBrowserCacheSizeMB { get; set; } = 3000; // 3 GB
        
        // Performance
        public int MinAvailableMemoryPercent { get; set; } = 20;
        public int MaxCpuUsagePercent { get; set; } = 80;
    }

    /// <summary>
    /// Configuration des notifications
    /// </summary>
    public class NotificationSettings
    {
        public bool EnableNotifications { get; set; } = true;
        public bool NotifyOnCriticalIssues { get; set; } = true;
        public bool NotifyOnAuditComplete { get; set; } = false;
        public bool NotifyOnLowHealthScore { get; set; } = true;
        public bool ShowDesktopNotifications { get; set; } = true;
        public bool PlaySoundOnAlert { get; set; } = false;
        
        // Notifications par email (future feature)
        public bool EnableEmailNotifications { get; set; } = false;
        public string EmailAddress { get; set; } = string.Empty;
        public bool EmailOnlyForCritical { get; set; } = true;
    }

    /// <summary>
    /// Options pour un audit spécifique
    /// </summary>
    public class AuditOptions
    {
        public string AuditType { get; set; } = "Full"; // Full, Quick, Custom
        public List<string> ModulesToRun { get; set; } = new();
        public bool GenerateDetailedReport { get; set; } = true;
        public bool IncludeRecommendations { get; set; } = true;
        public bool CalculateHealthScore { get; set; } = true;
        public bool CompareWithPrevious { get; set; } = true;
        public bool SaveToHistory { get; set; } = true;
        public int TimeoutMinutes { get; set; } = 30;
        public string CustomReportName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gestion de la configuration
    /// </summary>
    public static class AuditConfigurationManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsCleaner", "audit-config.json"
        );

        /// <summary>
        /// Charge la configuration depuis le fichier
        /// </summary>
        public static AuditConfiguration Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<AuditConfiguration>(json);
                    return config ?? CreateDefault();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors du chargement de la configuration d'audit: {ex.Message}");
            }

            return CreateDefault();
        }

        /// <summary>
        /// Sauvegarde la configuration dans le fichier
        /// </summary>
        public static void Save(AuditConfiguration config)
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
                
                Logger.Log(LogLevel.Info, "Configuration d'audit sauvegardée");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la sauvegarde de la configuration d'audit: {ex.Message}");
            }
        }

        /// <summary>
        /// Crée une configuration par défaut
        /// </summary>
        public static AuditConfiguration CreateDefault()
        {
            return new AuditConfiguration();
        }

        /// <summary>
        /// Réinitialise la configuration aux valeurs par défaut
        /// </summary>
        public static void Reset()
        {
            var defaultConfig = CreateDefault();
            Save(defaultConfig);
        }

        /// <summary>
        /// Valide la configuration
        /// </summary>
        public static bool Validate(AuditConfiguration config)
        {
            if (config == null) return false;
            if (config.MaxHistoryDays < 1) return false;
            if (config.Thresholds.MinHealthScore < 0 || config.Thresholds.MinHealthScore > 100) return false;
            if (config.Thresholds.MaxDiskUsagePercent < 0 || config.Thresholds.MaxDiskUsagePercent > 100) return false;
            
            return true;
        }
    }
}
