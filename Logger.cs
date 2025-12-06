using System;
using System.IO;
using System.Text;

namespace WindowsCleaner
{
    /// <summary>
    /// Niveaux de verbosité des logs
    /// </summary>
    public enum LogLevel { Debug, Info, Warning, Error }

    /// <summary>
    /// Classe statique pour la gestion centralisée des logs
    /// Enregistre sur le disque et notifie les observateurs
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static string _logFile = Path.Combine(_logDir, "windows-cleaner.log");

        /// <summary>
        /// Événement déclenché à chaque log
        /// </summary>
        public static event Action<DateTime, LogLevel, string>? OnLog;

        /// <summary>
        /// Initialise le répertoire des logs
        /// </summary>
        /// <param name="logDirectory">Répertoire optionnel personnalisé pour les logs</param>
        public static void Init(string? logDirectory = null)
        {
            if (!string.IsNullOrEmpty(logDirectory))
            {
                _logDir = logDirectory;
                _logFile = Path.Combine(_logDir, "windows-cleaner.log");
            }

            try
            {
                Directory.CreateDirectory(_logDir);
            }
            catch (Exception ex)
            {
                // Silently fail - logging to console not available in this context
                System.Diagnostics.Debug.WriteLine($"Logger.Init() error: {ex.Message}");
            }
        }

        /// <summary>
        /// Enregistre un message de log
        /// </summary>
        /// <param name="level">Niveau du log (Debug, Info, Warning, Error)</param>
        /// <param name="message">Message à enregistrer</param>
        public static void Log(LogLevel level, string message)
        {
            var ts = DateTime.Now;
            try
            {
                var line = $"[{ts:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                lock (_lock)
                {
                    File.AppendAllText(_logFile, line + Environment.NewLine, Encoding.UTF8);
                }
                OnLog?.Invoke(ts, level, message);
            }
            catch (Exception ex)
            {
                // Log failure - try to write to debug
                System.Diagnostics.Debug.WriteLine($"Logger.Log() failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Efface tous les logs existants
        /// </summary>
        public static void Clear()
        {
            try
            {
                lock (_lock)
                {
                    if (File.Exists(_logFile)) 
                        File.Delete(_logFile);
                }
                OnLog?.Invoke(DateTime.Now, LogLevel.Info, "Logs effacés");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logger.Clear() failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Exporte les logs vers un fichier de destination
        /// </summary>
        /// <param name="destinationPath">Chemin du fichier de destination</param>
        /// <returns>Chemin du fichier exporté ou null si erreur</returns>
        public static string? Export(string destinationPath)
        {
            try
            {
                if (!File.Exists(_logFile)) 
                    return null;
                    
                File.Copy(_logFile, destinationPath, true);
                return destinationPath;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Erreur export logs: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retourne le contenu entier du fichier de log
        /// </summary>
        /// <returns>Contenu du log ou string vide</returns>
        public static string GetLogContent()
        {
            try
            {
                if (!File.Exists(_logFile))
                    return string.Empty;

                lock (_lock)
                {
                    return File.ReadAllText(_logFile, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                return $"Erreur lecture logs: {ex.Message}";
            }
        }

        /// <summary>
        /// Retourne le chemin du fichier de log courant
        /// </summary>
        public static string LogFilePath => _logFile;
    }
}

