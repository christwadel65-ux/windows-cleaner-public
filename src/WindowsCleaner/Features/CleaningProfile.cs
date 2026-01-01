using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WindowsCleaner
{
    /// <summary>
    /// Profil de nettoyage personnalisé avec toutes les options sauvegardées
    /// </summary>
    public class CleaningProfile
    {
        public string Name { get; set; } = "Nouveau Profil";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        // Options de nettoyage standard
        public bool EmptyRecycleBin { get; set; }
        public bool IncludeSystemTemp { get; set; }
        public bool CleanBrowsers { get; set; }
        public bool CleanBrowserHistory { get; set; } = true;
        public bool CleanWindowsUpdate { get; set; }
        public bool CleanThumbnails { get; set; }
        public bool CleanPrefetch { get; set; }
        public bool FlushDns { get; set; }
        
        // Options avancées
        public bool CleanSystemLogs { get; set; }
        public bool CleanInstallerCache { get; set; }
        public bool CleanOrphanedFiles { get; set; }
        public bool CleanApplicationLogs { get; set; }
        public bool ClearMemoryCache { get; set; }
        
        // Nouvelles options - Logiciels spécifiques
        public bool CleanDocker { get; set; }
        public bool CleanNodeModules { get; set; }
        public bool CleanVisualStudio { get; set; }
        public bool CleanPythonCache { get; set; }
        public bool CleanGitCache { get; set; }
        
        // Nouvelles options - Vie privée
        public bool CleanRunHistory { get; set; }
        public bool CleanRecentDocuments { get; set; }
        public bool CleanWindowsTimeline { get; set; }
        public bool CleanSearchHistory { get; set; }
        public bool CleanClipboard { get; set; }
        
        // Application caches
        public bool CleanVsCodeCache { get; set; }
        public bool CleanNugetCache { get; set; }
        public bool CleanMavenCache { get; set; }
        public bool CleanNpmCache { get; set; }
        public bool CleanGameCaches { get; set; }
        
        // Disk optimization
        public bool OptimizeSsd { get; set; }
        public bool CheckDiskHealth { get; set; }
        
        // Broken shortcuts
        public bool CleanBrokenShortcuts { get; set; }
        
        // Ghost apps
        public bool CleanGhostApps { get; set; }
        
        // Options d'exécution
        public bool Verbose { get; set; }
        public bool CreateBackup { get; set; }
        
        /// <summary>
        /// Crée un profil "Rapide" prédéfini
        /// </summary>
        public static CleaningProfile CreateQuickProfile()
        {
            return new CleaningProfile
            {
                Name = LanguageManager.Get("profile_quick"),
                Description = LanguageManager.Get("profile_quick_desc"),
                EmptyRecycleBin = true,
                CleanBrowsers = true,
                CleanThumbnails = true,
                CleanBrowserHistory = true,
                Verbose = false
            };
        }
        
        /// <summary>
        /// Crée un profil "Complet" prédéfini
        /// </summary>
        public static CleaningProfile CreateCompleteProfile()
        {
            return new CleaningProfile
            {
                Name = LanguageManager.Get("profile_complete"),
                Description = LanguageManager.Get("profile_complete_desc"),
                EmptyRecycleBin = true,
                IncludeSystemTemp = true,
                CleanBrowsers = true,
                CleanWindowsUpdate = true,
                CleanThumbnails = true,
                CleanPrefetch = true,
                FlushDns = true,
                CleanOrphanedFiles = true,
                CleanApplicationLogs = true,
                ClearMemoryCache = true,
                CleanBrokenShortcuts = true,
                CleanGhostApps = true,
                OptimizeSsd = true,
                CheckDiskHealth = true,
                Verbose = true,
                CreateBackup = true
            };
        }
        
        /// <summary>
        /// Crée un profil "Développeur" prédéfini
        /// </summary>
        public static CleaningProfile CreateDeveloperProfile()
        {
            return new CleaningProfile
            {
                Name = LanguageManager.Get("profile_developer"),
                Description = LanguageManager.Get("profile_developer_desc"),
                EmptyRecycleBin = true,
                CleanBrowsers = true,
                CleanNodeModules = true,
                CleanVisualStudio = true,
                CleanPythonCache = true,
                CleanGitCache = true,
                CleanDocker = true,
                CleanVsCodeCache = true,
                CleanNugetCache = true,
                CleanMavenCache = true,
                CleanNpmCache = true,
                CleanOrphanedFiles = true,
                OptimizeSsd = true,
                Verbose = true
            };
        }
        
        /// <summary>
        /// Crée un profil "Vie Privée" prédéfini
        /// </summary>
        public static CleaningProfile CreatePrivacyProfile()
        {
            return new CleaningProfile
            {
                Name = LanguageManager.Get("profile_privacy"),
                Description = LanguageManager.Get("profile_privacy_desc"),
                CleanBrowsers = true,
                CleanRunHistory = true,
                CleanRecentDocuments = true,
                CleanWindowsTimeline = true,
                CleanSearchHistory = true,
                CleanClipboard = true,
                CleanOrphanedFiles = true,
                Verbose = false
            };
        }
        
        /// <summary>
        /// Convertit le profil en CleanerOptions
        /// </summary>
        public CleanerOptions ToCleanerOptions(bool dryRun = false)
        {
            return new CleanerOptions
            {
                DryRun = dryRun,
                EmptyRecycleBin = this.EmptyRecycleBin,
                IncludeSystemTemp = this.IncludeSystemTemp,
                CleanBrowsers = this.CleanBrowsers,
                CleanBrowserHistory = this.CleanBrowserHistory,
                CleanWindowsUpdate = this.CleanWindowsUpdate,
                CleanThumbnails = this.CleanThumbnails,
                CleanPrefetch = this.CleanPrefetch,
                FlushDns = this.FlushDns,
                CleanSystemLogs = this.CleanSystemLogs,
                CleanInstallerCache = this.CleanInstallerCache,
                CleanOrphanedFiles = this.CleanOrphanedFiles,
                CleanApplicationLogs = this.CleanApplicationLogs,
                ClearMemoryCache = this.ClearMemoryCache,
                Verbose = this.Verbose,
                // Nouvelles options
                CleanDocker = this.CleanDocker,
                CleanNodeModules = this.CleanNodeModules,
                CleanVisualStudio = this.CleanVisualStudio,
                CleanPythonCache = this.CleanPythonCache,
                CleanGitCache = this.CleanGitCache,
                CleanRunHistory = this.CleanRunHistory,
                CleanRecentDocuments = this.CleanRecentDocuments,
                CleanWindowsTimeline = this.CleanWindowsTimeline,
                CleanSearchHistory = this.CleanSearchHistory,
                CleanClipboard = this.CleanClipboard,
                // Application caches
                CleanVsCodeCache = this.CleanVsCodeCache,
                CleanNugetCache = this.CleanNugetCache,
                CleanMavenCache = this.CleanMavenCache,
                CleanNpmCache = this.CleanNpmCache,
                CleanGameCaches = this.CleanGameCaches,
                // Disk optimization
                OptimizeSsd = this.OptimizeSsd,
                CheckDiskHealth = this.CheckDiskHealth,
                // Broken shortcuts
                CleanBrokenShortcuts = this.CleanBrokenShortcuts,
                // Ghost apps
                CleanGhostApps = this.CleanGhostApps
            };
        }
    }
    
    /// <summary>
    /// Gestionnaire de profils de nettoyage
    /// </summary>
    public static class ProfileManager
    {
        private static readonly string ProfilesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WindowsCleaner",
            "Profiles"
        );
        
        static ProfileManager()
        {
            if (!Directory.Exists(ProfilesDirectory))
            {
                Directory.CreateDirectory(ProfilesDirectory);
            }
        }
        
        /// <summary>
        /// Sauvegarde un profil sur disque
        /// </summary>
        public static void SaveProfile(CleaningProfile profile)
        {
            profile.LastModified = DateTime.Now;
            var fileName = SanitizeFileName(profile.Name) + ".json";
            var filePath = Path.Combine(ProfilesDirectory, fileName);
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(filePath, json);
            
            Logger.Log(LogLevel.Info, $"Profil '{profile.Name}' sauvegardé");
        }
        
        /// <summary>
        /// Charge un profil depuis le disque
        /// </summary>
        public static CleaningProfile? LoadProfile(string profileName)
        {
            var fileName = SanitizeFileName(profileName) + ".json";
            var filePath = Path.Combine(ProfilesDirectory, fileName);
            
            if (!File.Exists(filePath))
                return null;
            
            try
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return JsonSerializer.Deserialize<CleaningProfile>(json, options);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur chargement profil '{profileName}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Liste tous les profils disponibles
        /// </summary>
        public static List<CleaningProfile> GetAllProfiles()
        {
            var profiles = new List<CleaningProfile>();
            
            // Ajouter les profils prédéfinis
            profiles.Add(CleaningProfile.CreateQuickProfile());
            profiles.Add(CleaningProfile.CreateCompleteProfile());
            profiles.Add(CleaningProfile.CreateDeveloperProfile());
            profiles.Add(CleaningProfile.CreatePrivacyProfile());
            
            // Ajouter les profils personnalisés
            if (Directory.Exists(ProfilesDirectory))
            {
                foreach (var file in Directory.GetFiles(ProfilesDirectory, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var options = new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var profile = JsonSerializer.Deserialize<CleaningProfile>(json, options);
                        if (profile != null)
                            profiles.Add(profile);
                    }
                    catch { /* Ignorer les profils corrompus */ }
                }
            }
            
            return profiles;
        }
        
        /// <summary>
        /// Supprime un profil personnalisé
        /// </summary>
        public static bool DeleteProfile(string profileName)
        {
            var fileName = SanitizeFileName(profileName) + ".json";
            var filePath = Path.Combine(ProfilesDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Logger.Log(LogLevel.Info, $"Profil '{profileName}' supprimé");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Erreur suppression profil: {ex.Message}");
                    return false;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Exporte un profil vers un fichier
        /// </summary>
        public static void ExportProfile(CleaningProfile profile, string exportPath)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(exportPath, json);
            
            Logger.Log(LogLevel.Info, $"Profil exporté vers: {exportPath}");
        }
        
        /// <summary>
        /// Importe un profil depuis un fichier
        /// </summary>
        public static CleaningProfile? ImportProfile(string importPath)
        {
            try
            {
                var json = File.ReadAllText(importPath);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var profile = JsonSerializer.Deserialize<CleaningProfile>(json, options);
                
                if (profile != null)
                {
                    SaveProfile(profile);
                    Logger.Log(LogLevel.Info, $"Profil '{profile.Name}' importé");
                }
                
                return profile;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur import profil: {ex.Message}");
                return null;
            }
        }
        
        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
