using System;
using System.IO;

namespace WindowsCleaner
{
    /// <summary>
    /// Classe centralisée pour gérer les chemins des navigateurs web.
    /// Élimine la duplication et facilite la maintenance.
    /// </summary>
    public static class BrowserPaths
    {
        private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Retourne le chemin du cache Google Chrome
        /// </summary>
        public static string ChromeCache => Path.Combine(LocalAppData, "Google", "Chrome", "User Data", "Default", "Cache");

        /// <summary>
        /// Retourne le chemin du cache Microsoft Edge
        /// </summary>
        public static string EdgeCache => Path.Combine(LocalAppData, "Microsoft", "Edge", "User Data", "Default", "Cache");

        /// <summary>
        /// Retourne le chemin des profils Mozilla Firefox
        /// </summary>
        public static string FirefoxProfiles => Path.Combine(LocalAppData, "Mozilla", "Firefox", "Profiles");

        /// <summary>
        /// Retourne le chemin du cache Firefox pour un profil spécifique
        /// </summary>
        /// <param name="profilePath">Chemin du profil Firefox</param>
        public static string GetFirefoxCache(string profilePath) => Path.Combine(profilePath, "cache2");

        /// <summary>
        /// Vérifie si Firefox est installé en vérifiant l'existence du dossier profils
        /// </summary>
        public static bool IsFirefoxInstalled => Directory.Exists(FirefoxProfiles);

        /// <summary>
        /// Retourne le chemin du cache temporaire utilisateur
        /// </summary>
        public static string UserTemp => Path.GetTempPath();

        /// <summary>
        /// Retourne le chemin du cache temporaire LocalAppData
        /// </summary>
        public static string LocalAppDataTemp => Path.Combine(LocalAppData, "Temp");

        /// <summary>
        /// Retourne le chemin du cache temporaire système
        /// </summary>
        public static string SystemTemp
        {
            get
            {
                var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                return Path.Combine(windows, "Temp");
            }
        }

        /// <summary>
        /// Retourne le chemin des vignettes Windows
        /// </summary>
        public static string ThumbnailsCache => Path.Combine(LocalAppData, "Microsoft", "Windows", "Explorer");

        /// <summary>
        /// Retourne le chemin du Prefetch système
        /// </summary>
        public static string Prefetch
        {
            get
            {
                var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                return Path.Combine(windows, "Prefetch");
            }
        }

        /// <summary>
        /// Retourne le chemin du cache Windows Update (SoftwareDistribution)
        /// </summary>
        public static string WindowsUpdateCache
        {
            get
            {
                var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                return Path.Combine(windows, "SoftwareDistribution", "Download");
            }
        }

        /// <summary>
        /// Retourne le chemin du cache des installateurs Windows
        /// </summary>
        public static string InstallerCache => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Installer");
    }
}
