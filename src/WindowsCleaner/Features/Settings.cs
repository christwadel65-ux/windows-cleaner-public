using System;
using System.IO;
using System.Text.Json;

namespace WindowsCleaner
{
    /// <summary>
    /// Paramètres de l'application sauvegardés localement
    /// </summary>
    public class AppSettings
    {
        /// <summary>Colonne de tri du rapport</summary>
        public string? ReportSortColumn { get; set; }
        /// <summary>Direction de tri ("ASC" ou "DESC")</summary>
        public string? ReportSortDirection { get; set; }
        
        // Options de nettoyage
        /// <summary>Vider la Corbeille</summary>
        public bool? CleanRecycleBin { get; set; }
        /// <summary>Nettoyer le répertoire Temp système</summary>
        public bool? CleanSystemTemp { get; set; }
        /// <summary>Nettoyer les caches des navigateurs</summary>
        public bool? CleanBrowsers { get; set; }
        /// <summary>Nettoyer Windows Update</summary>
        public bool? CleanWindowsUpdate { get; set; }
        /// <summary>Nettoyer les vignettes</summary>
        public bool? CleanThumbnails { get; set; }
        /// <summary>Nettoyer Prefetch</summary>
        public bool? CleanPrefetch { get; set; }
        /// <summary>Exécuter Flush DNS</summary>
        public bool? FlushDns { get; set; }
        /// <summary>Mode verbeux</summary>
        public bool? Verbose { get; set; }
        /// <summary>Mode avancé</summary>
        public bool? Advanced { get; set; }
        /// <summary>Nettoyer les fichiers orphelins</summary>
        public bool? CleanOrphanedFiles { get; set; }
        /// <summary>Effacer le cache mémoire</summary>
        public bool? ClearMemoryCache { get; set; }

        /// <summary>Nom du profil sélectionné dans l'interface</summary>
        public string? SelectedProfileName { get; set; }
    }

    /// <summary>
    /// Gestionnaire de sauvegarde et chargement des paramètres
    /// </summary>
    public static class SettingsManager
    {
        private static readonly string _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WindowsCleaner");
        private static readonly string _file = Path.Combine(_dir, "settings.json");

        /// <summary>
        /// Charge les paramètres sauvegardés depuis le disque
        /// </summary>
        /// <returns>Paramètres chargés ou nouveau AppSettings par défaut</returns>
        public static AppSettings Load()
        {
            try
            {
                if (!Directory.Exists(_dir)) 
                    Directory.CreateDirectory(_dir);
                    
                if (!File.Exists(_file)) 
                    return new AppSettings();
                    
                var txt = File.ReadAllText(_file);
                return JsonSerializer.Deserialize<AppSettings>(txt) ?? new AppSettings();
            }
            catch (Exception ex) 
            { 
                Logger.Log(LogLevel.Error, $"Erreur chargement settings: {ex.Message}");
                return new AppSettings(); 
            }
        }

        /// <summary>
        /// Sauvegarde les paramètres sur le disque
        /// </summary>
        /// <param name="s">Paramètres à sauvegarder</param>
        public static void Save(AppSettings s)
        {
            try
            {
                if (!Directory.Exists(_dir)) 
                    Directory.CreateDirectory(_dir);
                    
                var txt = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_file, txt);
                Logger.Log(LogLevel.Debug, "Paramètres sauvegardés avec succès");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur sauvegarde settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne le chemin du fichier de paramètres
        /// </summary>
        public static string SettingsFilePath => _file;
    }
}
