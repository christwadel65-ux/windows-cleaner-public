using System;
using System.Collections.Generic;

namespace WindowsCleaner.Core
{
    /// <summary>
    /// Représente un problème détecté lors d'un audit
    /// </summary>
    public class AuditIssue
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public IssueSeverity Severity { get; set; } = IssueSeverity.Info;
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
        public bool AutoFixAvailable { get; set; } = false;
        public string AutoFixAction { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.Now;
        public string Location { get; set; } = string.Empty; // Chemin ou emplacement du problème
        public Dictionary<string, object> Details { get; set; } = new();
        public IssueStatus Status { get; set; } = IssueStatus.Active;
        public string Icon { get; set; } = "⚠️";
    }

    /// <summary>
    /// Sévérité d'un problème
    /// </summary>
    public enum IssueSeverity
    {
        Info,       // Information simple
        Low,        // Impact faible
        Medium,     // Impact moyen
        High,       // Impact élevé
        Critical    // Impact critique
    }

    /// <summary>
    /// Statut d'un problème
    /// </summary>
    public enum IssueStatus
    {
        Active,     // Problème actif
        Resolved,   // Résolu
        Ignored,    // Ignoré par l'utilisateur
        Pending     // En attente de correction
    }

    /// <summary>
    /// Helper pour créer des problèmes typiques
    /// </summary>
    public static class AuditIssueFactory
    {
        public static AuditIssue CreateDiskSpaceIssue(string driveName, long usedSpace, long totalSpace)
        {
            var percentUsed = (double)usedSpace / totalSpace * 100;
            var severity = percentUsed switch
            {
                >= 95 => IssueSeverity.Critical,
                >= 90 => IssueSeverity.High,
                >= 80 => IssueSeverity.Medium,
                _ => IssueSeverity.Low
            };

            return new AuditIssue
            {
                Severity = severity,
                Category = "DiskSpace",
                Title = $"Espace disque faible sur {driveName}",
                Description = $"Le disque {driveName} est utilisé à {percentUsed:F1}%",
                Impact = "Peut ralentir le système et empêcher l'installation de mises à jour",
                RecommendedActions = new List<string>
                {
                    "Nettoyer les fichiers temporaires",
                    "Désinstaller les applications inutilisées",
                    "Vider la corbeille",
                    "Analyser les gros fichiers"
                },
                AutoFixAvailable = true,
                AutoFixAction = "CleanTempFiles",
                Location = driveName,
                Icon = "💾"
            };
        }

        public static AuditIssue CreateTempFilesIssue(long totalSize, int fileCount)
        {
            var severity = totalSize switch
            {
                > 10_000_000_000 => IssueSeverity.High,      // > 10 GB
                > 5_000_000_000 => IssueSeverity.Medium,     // > 5 GB
                > 1_000_000_000 => IssueSeverity.Low,        // > 1 GB
                _ => IssueSeverity.Info
            };

            return new AuditIssue
            {
                Severity = severity,
                Category = "TempFiles",
                Title = "Fichiers temporaires accumulés",
                Description = $"{fileCount} fichiers temporaires occupent {FormatBytes(totalSize)}",
                Impact = "Gaspillage d'espace disque et ralentissement potentiel",
                RecommendedActions = new List<string>
                {
                    "Supprimer les fichiers temporaires",
                    "Nettoyer le cache système",
                    "Vider les dossiers temporaires"
                },
                AutoFixAvailable = true,
                AutoFixAction = "CleanTempFiles",
                Location = "C:\\Windows\\Temp, C:\\Users\\*\\AppData\\Local\\Temp",
                Icon = "🗑️"
            };
        }

        public static AuditIssue CreateRegistryIssue(int invalidKeys, string category)
        {
            var severity = invalidKeys switch
            {
                > 1000 => IssueSeverity.High,
                > 500 => IssueSeverity.Medium,
                > 100 => IssueSeverity.Low,
                _ => IssueSeverity.Info
            };

            return new AuditIssue
            {
                Severity = severity,
                Category = "Registry",
                Title = $"Clés de registre invalides détectées ({category})",
                Description = $"{invalidKeys} clés invalides ou orphelines trouvées",
                Impact = "Peut ralentir le système et causer des erreurs",
                RecommendedActions = new List<string>
                {
                    "Nettoyer le registre avec précaution",
                    "Créer une sauvegarde avant nettoyage",
                    "Utiliser un outil de nettoyage fiable"
                },
                AutoFixAvailable = true,
                AutoFixAction = "CleanRegistry",
                Location = $"HKEY_LOCAL_MACHINE\\SOFTWARE, HKEY_CURRENT_USER",
                Icon = "📝"
            };
        }

        public static AuditIssue CreateStartupIssue(int programCount)
        {
            var severity = programCount switch
            {
                > 20 => IssueSeverity.High,
                > 15 => IssueSeverity.Medium,
                > 10 => IssueSeverity.Low,
                _ => IssueSeverity.Info
            };

            return new AuditIssue
            {
                Severity = severity,
                Category = "Startup",
                Title = "Trop de programmes au démarrage",
                Description = $"{programCount} programmes se lancent au démarrage",
                Impact = "Ralentit considérablement le démarrage de Windows",
                RecommendedActions = new List<string>
                {
                    "Désactiver les programmes non essentiels",
                    "Utiliser le gestionnaire de tâches",
                    "Configurer les applications importantes uniquement"
                },
                AutoFixAvailable = false,
                Location = "msconfig > Démarrage",
                Icon = "🚀"
            };
        }

        public static AuditIssue CreateBrowserCacheIssue(string browser, long cacheSize)
        {
            var severity = cacheSize switch
            {
                > 5_000_000_000 => IssueSeverity.Medium,    // > 5 GB
                > 2_000_000_000 => IssueSeverity.Low,       // > 2 GB
                _ => IssueSeverity.Info
            };

            return new AuditIssue
            {
                Severity = severity,
                Category = "BrowserCache",
                Title = $"Cache {browser} volumineux",
                Description = $"Le cache de {browser} occupe {FormatBytes(cacheSize)}",
                Impact = "Gaspillage d'espace disque",
                RecommendedActions = new List<string>
                {
                    $"Vider le cache de {browser}",
                    "Configurer le nettoyage automatique",
                    "Limiter la taille du cache"
                },
                AutoFixAvailable = true,
                AutoFixAction = $"CleanBrowserCache_{browser}",
                Location = GetBrowserCachePath(browser),
                Icon = "🌐"
            };
        }

        public static AuditIssue CreateServiceIssue(string serviceName, string issue)
        {
            return new AuditIssue
            {
                Severity = IssueSeverity.Medium,
                Category = "Services",
                Title = $"Problème avec le service {serviceName}",
                Description = issue,
                Impact = "Peut affecter les performances ou la stabilité du système",
                RecommendedActions = new List<string>
                {
                    "Vérifier la configuration du service",
                    "Redémarrer le service",
                    "Vérifier les logs d'événements"
                },
                AutoFixAvailable = false,
                Location = $"services.msc > {serviceName}",
                Icon = "⚙️"
            };
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

        private static string GetBrowserCachePath(string browser)
        {
            return browser.ToLower() switch
            {
                "chrome" => "%LocalAppData%\\Google\\Chrome\\User Data\\Default\\Cache",
                "firefox" => "%LocalAppData%\\Mozilla\\Firefox\\Profiles\\*\\cache2",
                "edge" => "%LocalAppData%\\Microsoft\\Edge\\User Data\\Default\\Cache",
                _ => "%LocalAppData%\\{Browser}\\Cache"
            };
        }
    }
}
