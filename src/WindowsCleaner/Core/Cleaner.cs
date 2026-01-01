using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleaner
{
    /// <summary>
    /// Configuration des options de nettoyage du système
    /// </summary>
    public class CleanerOptions
    {
        /// <summary>Effectue un nettoyage de simulation sans supprimer de fichiers</summary>
        public bool DryRun { get; set; }
        /// <summary>Vide la Corbeille</summary>
        public bool EmptyRecycleBin { get; set; }
        /// <summary>Inclut le nettoyage du répertoire Temp système (nécessite admin)</summary>
        public bool IncludeSystemTemp { get; set; }
        /// <summary>Nettoie les caches des navigateurs web</summary>
        public bool CleanBrowsers { get; set; }
        /// <summary>Nettoie l'historique des navigateurs (bases SQLite, sessions)</summary>
        public bool CleanBrowserHistory { get; set; } = true;
        /// <summary>Nettoie le cache Windows Update (nécessite admin)</summary>
        public bool CleanWindowsUpdate { get; set; }
        /// <summary>Nettoie les fichiers de vignettes</summary>
        public bool CleanThumbnails { get; set; }
        /// <summary>Nettoie le répertoire Prefetch (nécessite admin)</summary>
        public bool CleanPrefetch { get; set; }
        /// <summary>Exécute le flush DNS</summary>
        public bool FlushDns { get; set; }
        /// <summary>Affiche les messages de log détaillés</summary>
        public bool Verbose { get; set; }
        
        // Advanced options
        /// <summary>Nettoie les journaux système (désactivé par défaut - risqué)</summary>
        public bool CleanSystemLogs { get; set; }
        /// <summary>Nettoie le cache des installateurs (désactivé par défaut - risqué)</summary>
        public bool CleanInstallerCache { get; set; }
        /// <summary>Nettoie les fichiers orphelins datant de plus de 7 jours</summary>
        public bool CleanOrphanedFiles { get; set; }
        /// <summary>Nettoie les journaux des applications (désactivé par défaut - risqué)</summary>
        public bool CleanApplicationLogs { get; set; }
        /// <summary>Vide le cache mémoire via GC</summary>
        public bool ClearMemoryCache { get; set; }
        
        // Software-specific cleaning
        /// <summary>Nettoie les images et conteneurs Docker inutilisés</summary>
        public bool CleanDocker { get; set; }
        /// <summary>Nettoie les dossiers node_modules anciens</summary>
        public bool CleanNodeModules { get; set; }
        /// <summary>Nettoie les caches Visual Studio et build files</summary>
        public bool CleanVisualStudio { get; set; }
        /// <summary>Nettoie les fichiers __pycache__ et .pyc Python</summary>
        public bool CleanPythonCache { get; set; }
        /// <summary>Nettoie les objets Git non référencés</summary>
        public bool CleanGitCache { get; set; }
        
        // Application caches
        /// <summary>Nettoie le cache VS Code</summary>
        public bool CleanVsCodeCache { get; set; }
        /// <summary>Nettoie le cache NuGet</summary>
        public bool CleanNugetCache { get; set; }
        /// <summary>Nettoie le cache Maven</summary>
        public bool CleanMavenCache { get; set; }
        /// <summary>Nettoie le cache npm global</summary>
        public bool CleanNpmCache { get; set; }
        /// <summary>Nettoie les caches de jeux (Steam, Epic)</summary>
        public bool CleanGameCaches { get; set; }
        
        // Disk optimization
        /// <summary>Optimise SSD (TRIM, défragmentation légère)</summary>
        public bool OptimizeSsd { get; set; }
        /// <summary>Vérifie la santé du disque (SMART)</summary>
        public bool CheckDiskHealth { get; set; }
        
        // Privacy cleaning
        /// <summary>Nettoie l'historique Exécuter (Win+R)</summary>
        public bool CleanRunHistory { get; set; }
        /// <summary>Nettoie les documents récents</summary>
        public bool CleanRecentDocuments { get; set; }
        /// <summary>Nettoie la Timeline Windows 10/11</summary>
        public bool CleanWindowsTimeline { get; set; }
        /// <summary>Nettoie l'historique de recherche Windows</summary>
        public bool CleanSearchHistory { get; set; }
        /// <summary>Vide le presse-papiers Windows</summary>
        public bool CleanClipboard { get; set; }
        
        /// <summary>Supprime les raccourcis (.lnk) cassés dont la cible n'existe plus</summary>
        public bool CleanBrokenShortcuts { get; set; }
        
        /// <summary>Nettoie les applications fantômes (dossiers orphelins, entrées registre invalides)</summary>
        public bool CleanGhostApps { get; set; }
        
        /// <summary>Ferme automatiquement les navigateurs avant le nettoyage (recommandé)</summary>
        public bool CloseBrowsersIfNeeded { get; set; } = true;
    }

    /// <summary>
    /// Résultat du nettoyage contenant les statistiques
    /// </summary>
    public class CleanerResult
    {
        /// <summary>Nombre de fichiers supprimés</summary>
        public int FilesDeleted { get; set; }
        /// <summary>Nombre d'octets libérés</summary>
        public long BytesFreed { get; set; }
        
        // Statistiques d'app cache
        /// <summary>Fichiers VS Code cache supprimés</summary>
        public int VsCodeCacheFilesDeleted { get; set; }
        /// <summary>Fichiers NuGet cache supprimés</summary>
        public int NugetCacheFilesDeleted { get; set; }
        /// <summary>Fichiers Maven cache supprimés</summary>
        public int MavenCacheFilesDeleted { get; set; }
        /// <summary>Fichiers npm cache supprimés</summary>
        public int NpmCacheFilesDeleted { get; set; }
        /// <summary>Fichiers jeux cache supprimés</summary>
        public int GameCachesFilesDeleted { get; set; }
        /// <summary>Octets app cache libérés</summary>
        public long AppCachesBytesFreed { get; set; }
        
        // Statistiques SSD
        /// <summary>Indique si l'optimisation SSD a été exécutée</summary>
        public bool SsdOptimized { get; set; }
        /// <summary>Indique si une vérification SMART a été effectuée</summary>
        public bool DiskHealthChecked { get; set; }
        /// <summary>Rapport SMART de santé du disque</summary>
        public string DiskHealthReport { get; set; } = string.Empty;
        
        // Statistiques raccourcis
        /// <summary>Nombre de raccourcis cassés supprimés</summary>
        public int BrokenShortcutsDeleted { get; set; }
        
        // Statistiques applications fantômes
        /// <summary>Nombre d'applications fantômes nettoyées</summary>
        public int GhostAppsRemoved { get; set; }
        /// <summary>Nombre d'entrées registre invalides supprimées</summary>
        public int InvalidRegistryEntriesRemoved { get; set; }
    }

    /// <summary>
    /// Élément du rapport de nettoyage
    /// </summary>
    public class ReportItem
    {
        /// <summary>Chemin du fichier ou dossier</summary>
        public string Path { get; set; } = string.Empty;
        /// <summary>Taille en octets</summary>
        public long Size { get; set; }
        /// <summary>Indique si c'est un répertoire</summary>
        public bool IsDirectory { get; set; }
    }

    /// <summary>
    /// Rapport complet de nettoyage avec tous les éléments trouvés
    /// </summary>
    public class CleanerReport
    {
        /// <summary>Liste des éléments à nettoyer</summary>
        public System.Collections.Generic.List<ReportItem> Items { get; } = new System.Collections.Generic.List<ReportItem>();
        /// <summary>Total d'octets trouvés</summary>
        public long TotalBytes => Items.Sum(i => i.Size);
        /// <summary>Nombre d'éléments trouvés</summary>
        public int Count => Items.Count;
    }

    /// <summary>
    /// Classe principale pour exécuter les opérations de nettoyage système
    /// </summary>
    public static class Cleaner
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        // Flags for SHEmptyRecycleBin
        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI = 0x00000002;
        private const uint SHERB_NOSOUND = 0x00000004;

        // P/Invoke pour MoveFileEx (suppression au redémarrage)
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, uint dwFlags);
        private const uint MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;
        private const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;

        /// <summary>
        /// Exécute le nettoyage selon les options spécifiées
        /// </summary>
        /// <param name="options">Configuration des opérations à effectuer</param>
        /// <param name="log">Délégué optionnel pour recevoir les messages de log</param>
        /// <param name="cancellationToken">Token pour annuler l'opération</param>
        /// <returns>Résultat du nettoyage avec statistiques</returns>
        public static CleanerResult RunCleanup(CleanerOptions options, Action<string>? log = null, CancellationToken cancellationToken = default)
        {
            var result = new CleanerResult();
            var lockObj = new object();

            // Thread-safe logger wrapper
            Action<string> threadSafeLog = CreateThreadSafeLogger(options.Verbose, log);

            void AddResult(int files, long bytes)
            {
                lock (lockObj)
                {
                    result.FilesDeleted += files;
                    result.BytesFreed += bytes;
                }
            }

            var tasks = new System.Collections.Generic.List<Task>();

            // User temp + LocalAppData Temp (rattachés à l'option "Temp Système" pour plus de clarté UX)
            if (options.IncludeSystemTemp)
            {
                // User temp
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        string userTemp = BrowserPaths.UserTemp;
                        threadSafeLog(LanguageManager.Get("log_clean_user_temp", userTemp));
                        var r = DeleteDirectoryContents(userTemp, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "user temp", ex.Message));
                    }
                }, cancellationToken));

                // LocalAppData\Temp
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var localTemp = BrowserPaths.LocalAppDataTemp;
                        threadSafeLog(LanguageManager.Get("log_clean_local_temp", localTemp));
                        var r = DeleteDirectoryContents(localTemp, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "LocalAppData Temp", ex.Message));
                    }
                }, cancellationToken));
            }

            // System temp (optional)
            if (options.IncludeSystemTemp)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var systemTemp = BrowserPaths.SystemTemp;
                        threadSafeLog(LanguageManager.Get("log_clean_system_temp", systemTemp));
                        var r = DeleteDirectoryContents(systemTemp, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "System Temp", ex.Message));
                    }
                }, cancellationToken));
            }

            // Historique navigateurs et sessions récentes (option indépendante)
            if (options.CleanBrowsers && options.CleanBrowserHistory)
            {
                // Fermer les navigateurs pour éviter les verrous
                if (options.CloseBrowsersIfNeeded && !options.DryRun)
                {
                    threadSafeLog(LanguageManager.Get("log_closing_browsers_history"));
                    CloseBrowserProcesses(threadSafeLog);
                    Thread.Sleep(1200);
                }

                // Chrome/Edge: fichiers Last/Current Session & Tabs + dossier Sessions + base History
                void CleanChromiumHistory(string basePath, string browserName)
                {
                    try
                    {
                        if (!Directory.Exists(basePath)) return;
                        var targets = new System.Collections.Generic.List<string>();
                        var sessionsDir = Path.Combine(basePath, "Sessions");
                        if (Directory.Exists(sessionsDir)) targets.Add(sessionsDir);
                        string[] files = new[] {"Last Session","Last Tabs","Current Session","Current Tabs","History"};
                        foreach (var f in files)
                        {
                            var p = Path.Combine(basePath, f);
                            targets.Add(p);
                        }

                        foreach (var t in targets)
                        {
                            try
                            {
                                if (File.Exists(t))
                                {
                                    var fi = new FileInfo(t);
                                    if (!options.DryRun)
                                    {
                                        try { File.Delete(t); }
                                        catch (FileNotFoundException) { /* déjà supprimé */ }
                                    }
                                    AddResult(1, fi.Length);
                                    threadSafeLog(LanguageManager.Get("log_deleted_history", browserName, t));
                                }
                                else if (Directory.Exists(t))
                                {
                                    var r = DeleteDirectoryContents(t, options.DryRun, threadSafeLog, cancellationToken);
                                    AddResult(r.files, r.bytes);
                                    threadSafeLog(LanguageManager.Get("log_cleaned_sessions", browserName, t));
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Debug, LanguageManager.Get("error_cannot_delete", t, ex.Message));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", $"historique {browserName}", ex.Message));
                    }
                }

                // Chrome
                tasks.Add(Task.Run(() =>
                {
                    var chromeDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Google","Chrome","User Data","Default");
                    CleanChromiumHistory(chromeDefault, "Chrome");
                }, cancellationToken));

                // Edge
                tasks.Add(Task.Run(() =>
                {
                    var edgeDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Microsoft","Edge","User Data","Default");
                    CleanChromiumHistory(edgeDefault, "Edge");
                }, cancellationToken));

                // Firefox
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!BrowserPaths.IsFirefoxInstalled) return;
                        foreach (var profile in Directory.GetDirectories(BrowserPaths.FirefoxProfiles))
                        {
                            var sessionBackup = Path.Combine(profile, "sessionstore-backups");
                            var sessionFile = Path.Combine(profile, "sessionstore.jsonlz4");
                            var historyDb = Path.Combine(profile, "places.sqlite");

                            if (Directory.Exists(sessionBackup))
                            {
                                var r = DeleteDirectoryContents(sessionBackup, options.DryRun, threadSafeLog, cancellationToken);
                                AddResult(r.files, r.bytes);
                                threadSafeLog(LanguageManager.Get("log_cleaned_firefox_sessions", sessionBackup));
                            }
                            if (File.Exists(sessionFile))
                            {
                                var fi = new FileInfo(sessionFile);
                                if (!options.DryRun)
                                {
                                    try { File.Delete(sessionFile); }
                                    catch (FileNotFoundException) { }
                                }
                                AddResult(1, fi.Length);
                                threadSafeLog(LanguageManager.Get("log_deleted_firefox_session", sessionFile));
                            }
                            if (File.Exists(historyDb))
                            {
                                var fi = new FileInfo(historyDb);
                                if (!options.DryRun)
                                {
                                    try { File.Delete(historyDb); }
                                    catch (FileNotFoundException) { }
                                }
                                AddResult(1, fi.Length);
                                threadSafeLog(LanguageManager.Get("log_deleted_firefox_history", historyDb));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "historique Firefox", ex.Message));
                    }
                }, cancellationToken));
            }

            // Browser caches (Chrome, Edge, Firefox)
            if (options.CleanBrowsers)
            {
                // Fermer les navigateurs si demandé
                if (options.CloseBrowsersIfNeeded && !options.DryRun)
                {
                    threadSafeLog(LanguageManager.Get("log_closing_browsers"));
                    CloseBrowserProcesses(threadSafeLog);
                    Thread.Sleep(1500); // Attendre que les processus se terminent proprement
                }
                
                // Chrome cache
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!Directory.Exists(BrowserPaths.ChromeCache)) return;
                        threadSafeLog(LanguageManager.Get("log_clean_chrome_cache", BrowserPaths.ChromeCache));
                        var r = DeleteDirectoryContents(BrowserPaths.ChromeCache, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Chrome", ex.Message));
                    }
                }, cancellationToken));

                // Edge cache
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!Directory.Exists(BrowserPaths.EdgeCache)) return;
                        threadSafeLog(LanguageManager.Get("log_clean_edge_cache", BrowserPaths.EdgeCache));
                        var r = DeleteDirectoryContents(BrowserPaths.EdgeCache, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Edge", ex.Message));
                    }
                }, cancellationToken));

                // Firefox cache
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!BrowserPaths.IsFirefoxInstalled) return;
                        threadSafeLog(LanguageManager.Get("log_clean_firefox_cache", BrowserPaths.FirefoxProfiles));
                        var profiles = Directory.GetDirectories(BrowserPaths.FirefoxProfiles);
                        foreach (var profile in profiles)
                        {
                            var cacheDir = BrowserPaths.GetFirefoxCache(profile);
                            if (Directory.Exists(cacheDir))
                            {
                                var r = DeleteDirectoryContents(cacheDir, options.DryRun, threadSafeLog, cancellationToken);
                                AddResult(r.files, r.bytes);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Firefox", ex.Message));
                    }
                }, cancellationToken));
            }

            // Windows Update cache (SoftwareDistribution) - requires admin
            if (options.CleanWindowsUpdate)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var sd = BrowserPaths.WindowsUpdateCache;
                        threadSafeLog(LanguageManager.Get("log_clean_windows_update", sd));
                        var r = DeleteDirectoryContents(sd, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "SoftwareDistribution", ex.Message));
                    }
                }, cancellationToken));
            }

            // Thumbnails
            if (options.CleanThumbnails)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var thumbDir = BrowserPaths.ThumbnailsCache;
                        threadSafeLog(LanguageManager.Get("log_clean_thumbnails", thumbDir));
                        var files = Directory.Exists(thumbDir) ? Directory.GetFiles(thumbDir, "thumbcache_*.db") : Array.Empty<string>();
                        int filesDeleted = 0;
                        long bytesFreed = 0;
                        foreach (var f in files)
                        {
                            try
                            {
                                var fi = new FileInfo(f);
                                if (!options.DryRun) File.Delete(f);
                                filesDeleted++;
                                bytesFreed += fi.Length;
                                threadSafeLog(LanguageManager.Get("log_deleted_thumbnail", f));
                            }
                            catch (Exception ex)
                            {
                                threadSafeLog(LanguageManager.Get("log_cannot_delete_thumbnail", f, ex.Message));
                            }
                        }
                        AddResult(filesDeleted, bytesFreed);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "vignettes", ex.Message));
                    }
                }, cancellationToken));
            }

            // Prefetch (requires admin)
            if (options.CleanPrefetch)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var prefetch = BrowserPaths.Prefetch;
                        threadSafeLog(LanguageManager.Get("log_clean_prefetch", prefetch));
                        var r = DeleteDirectoryContents(prefetch, options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Prefetch", ex.Message));
                    }
                }, cancellationToken));
            }

            // Advanced: System logs
            if (options.CleanSystemLogs)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        // WARNING: Deleting system event logs can cause Windows instability
                        threadSafeLog(LanguageManager.Get("log_system_logs_disabled"));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "journaux système", ex.Message));
                    }
                }, cancellationToken));
            }

            // Advanced: Installer cache
            if (options.CleanInstallerCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        // WARNING: Windows\\Installer directory contains critical MSI files
                        threadSafeLog(LanguageManager.Get("log_installer_cache_disabled"));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "cache installeur", ex.Message));
                    }
                }, cancellationToken));
            }

            // Advanced: Application logs
            if (options.CleanApplicationLogs)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        // WARNING: Application logs in LocalState can be used by system services
                        threadSafeLog(LanguageManager.Get("log_app_logs_disabled"));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "journaux applications", ex.Message));
                    }
                }, cancellationToken));
            }

            // Advanced: Orphaned files (temp files without extensions or specific patterns)
            if (options.CleanOrphanedFiles)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var userTemp = BrowserPaths.UserTemp;
                        threadSafeLog(LanguageManager.Get("log_clean_orphaned_files", userTemp));
                        
                        int deleted = 0;
                        long freed = 0;
                        var cutoffDate = DateTime.Now.AddDays(-7);
                        
                        // Énumération optimisée sans allocations inutiles
                        foreach (var file in Directory.EnumerateFiles(userTemp, "*.*", SearchOption.TopDirectoryOnly))
                        {
                            try
                            {
                                var fi = new FileInfo(file);
                                if (fi.CreationTime >= cutoffDate) continue;
                                
                                if (!options.DryRun) File.Delete(file);
                                deleted++;
                                freed += fi.Length;
                                threadSafeLog(LanguageManager.Get("log_deleted_orphaned_file", Path.GetFileName(file)));
                            }
                            catch (Exception ex)
                            {
                                threadSafeLog(LanguageManager.Get("log_cannot_delete_file", file, ex.Message));
                            }
                        }
                        AddResult(deleted, freed);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "fichiers orphelins", ex.Message));
                    }
                }, cancellationToken));
            }

            // Wait for all tasks to complete
            try
            {
                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                threadSafeLog(LanguageManager.Get("log_cleaning_cancelled_user"));
            }

            // Clear memory cache (requires admin)
            if (options.ClearMemoryCache)
            {
                try
                {
                    threadSafeLog(LanguageManager.Get("log_clean_memory_cache"));
                    if (!options.DryRun)
                    {
                        // Safe memory cache clearing using GC only
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        
                        threadSafeLog(LanguageManager.Get("log_memory_cache_cleaned"));
                    }
                    else
                    {
                        threadSafeLog(LanguageManager.Get("log_memory_cache_dryrun"));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "cache mémoire", ex.Message));
                }
            }
            
            // Software-specific cleaning
            if (options.CleanDocker)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanDocker(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Docker", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanNodeModules)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanNodeModules(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Node.js", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanVisualStudio)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanVisualStudio(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Visual Studio", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanPythonCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanPythonCache(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Python", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanGitCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanGitCache(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Git", ex.Message));
                    }
                }, cancellationToken));
            }
            
            // Application caches
            if (options.CleanVsCodeCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanVsCodeCache(options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += r.files;
                            result.BytesFreed += r.bytes;
                            result.VsCodeCacheFilesDeleted = r.files;
                            result.AppCachesBytesFreed += r.bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "VS Code", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanNugetCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanNugetCache(options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += r.files;
                            result.BytesFreed += r.bytes;
                            result.NugetCacheFilesDeleted = r.files;
                            result.AppCachesBytesFreed += r.bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "NuGet", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanMavenCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanMavenCache(options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += r.files;
                            result.BytesFreed += r.bytes;
                            result.MavenCacheFilesDeleted = r.files;
                            result.AppCachesBytesFreed += r.bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Maven", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanNpmCache)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanNpmCache(options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += r.files;
                            result.BytesFreed += r.bytes;
                            result.NpmCacheFilesDeleted = r.files;
                            result.AppCachesBytesFreed += r.bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "npm", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanGameCaches)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanGameCaches(options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += r.files;
                            result.BytesFreed += r.bytes;
                            result.GameCachesFilesDeleted = r.files;
                            result.AppCachesBytesFreed += r.bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "jeux", ex.Message));
                    }
                }, cancellationToken));
            }
            
            // Disk optimization
            if (options.OptimizeSsd)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        threadSafeLog(LanguageManager.Get("log_ssd_optimization"));
                        SystemOptimizer.OptimizeSsd(threadSafeLog);
                        lock (lockObj)
                        {
                            result.SsdOptimized = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "optimisation SSD", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CheckDiskHealth)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        threadSafeLog(LanguageManager.Get("log_disk_health_check"));
                        var report = SystemOptimizer.CheckDiskHealth();
                        lock (lockObj)
                        {
                            result.DiskHealthChecked = true;
                            result.DiskHealthReport = report;
                        }
                        threadSafeLog(LanguageManager.Get("log_smart_check_success"));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "vérification SMART", ex.Message));
                    }
                }, cancellationToken));
            }
            
            // Privacy cleaning
            if (options.CleanRunHistory)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanRunHistory(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "historique Exécuter", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanRecentDocuments)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanRecentDocuments(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "documents récents", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanWindowsTimeline)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanWindowsTimeline(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Timeline", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanSearchHistory)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanSearchHistory(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "historique recherche", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanClipboard)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanClipboard(options.DryRun, threadSafeLog, cancellationToken);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "presse-papiers", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanBrokenShortcuts)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var r = CleanBrokenShortcuts(options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += r.files;
                            result.BytesFreed += r.bytes;
                            result.BrokenShortcutsDeleted = r.files;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "raccourcis cassés", ex.Message));
                    }
                }, cancellationToken));
            }
            
            if (options.CleanGhostApps)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var ghostApps = GhostAppsCleaner.DetectGhostApps(threadSafeLog, cancellationToken);
                        var (deletedFiles, freedBytes, removedRegistry) = GhostAppsCleaner.CleanGhostApps(ghostApps, options.DryRun, threadSafeLog, cancellationToken);
                        lock (lockObj)
                        {
                            result.FilesDeleted += deletedFiles;
                            result.BytesFreed += freedBytes;
                            result.GhostAppsRemoved = ghostApps.Count;
                            result.InvalidRegistryEntriesRemoved = removedRegistry;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "applications fantômes", ex.Message));
                    }
                }, cancellationToken));
            }

            // Flush DNS (sequential, quick operation)
            if (options.FlushDns)
            {
                try
                {
                    threadSafeLog(LanguageManager.Get("log_flush_dns"));
                    if (!options.DryRun)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo("ipconfig", "/flushdns") { CreateNoWindow = true, UseShellExecute = false };
                        var p = System.Diagnostics.Process.Start(psi);
                        p?.WaitForExit(5000);
                    }
                    else
                    {
                        threadSafeLog(LanguageManager.Get("log_flush_dns_dryrun"));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "flush DNS", ex.Message));
                }
            }

            // Empty Recycle Bin (sequential, requires UI thread in some cases)
            if (options.EmptyRecycleBin)
            {
                try
                {
                    threadSafeLog(LanguageManager.Get("log_empty_recycle_bin"));
                    if (!options.DryRun)
                    {
                        uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
                        SHEmptyRecycleBin(IntPtr.Zero, null, flags);
                    }
                    else
                    {
                        threadSafeLog(LanguageManager.Get("log_recycle_bin_dryrun"));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Corbeille", ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Génère un rapport détaillé des fichiers qui seraient nettoyés
        /// </summary>
        /// <param name="options">Configuration de scan</param>
        /// <param name="progress">Délégué optionnel pour recevoir les messages de progression</param>
        /// <param name="cancellationToken">Token pour annuler l'opération</param>
        /// <returns>Rapport contenant tous les éléments trouvés</returns>
        public static CleanerReport GenerateReport(CleanerOptions options, Action<string>? progress = null, CancellationToken cancellationToken = default)
        {
            var report = new CleanerReport();
            var items = new ConcurrentBag<ReportItem>();

            void P(string s) => progress?.Invoke(s);

            var tasks = new System.Collections.Generic.List<Task>();

            if (options.IncludeSystemTemp)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        string userTemp = BrowserPaths.UserTemp;
                        P($"Scan du dossier temporaire utilisateur: {userTemp}");
                        ScanDirectoryParallel(userTemp, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "user temp", ex.Message));
                    }
                }, cancellationToken));

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var localTemp = BrowserPaths.LocalAppDataTemp;
                        P($"Scan LocalAppData Temp: {localTemp}");
                        ScanDirectoryParallel(localTemp, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "LocalAppData Temp", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.IncludeSystemTemp)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var systemTemp = BrowserPaths.SystemTemp;
                        P($"Scan Temp système: {systemTemp}");
                        ScanDirectoryParallel(systemTemp, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "System Temp", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.CleanBrowsers && options.CleanBrowserHistory)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var chromeDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "Google","Chrome","User Data","Default");
                        var edgeDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "Microsoft","Edge","User Data","Default");
                        void ScanChromium(string basePath, string name)
                        {
                            if (!Directory.Exists(basePath)) return;
                            string[] files = new[]{"Last Session","Last Tabs","Current Session","Current Tabs","History"};
                            foreach (var f in files)
                            {
                                var p = Path.Combine(basePath, f);
                                if (File.Exists(p)) items.Add(new ReportItem{ Path = p, IsDirectory = false, Size = new FileInfo(p).Length });
                            }
                            var sessions = Path.Combine(basePath, "Sessions");
                            if (Directory.Exists(sessions))
                            {
                                P($"Scan {name} sessions: {sessions}");
                                ScanDirectoryParallel(sessions, items, P, cancellationToken);
                            }
                        }
                        ScanChromium(chromeDefault, "Chrome");
                        ScanChromium(edgeDefault, "Edge");

                        if (BrowserPaths.IsFirefoxInstalled)
                        {
                            foreach (var profile in Directory.GetDirectories(BrowserPaths.FirefoxProfiles))
                            {
                                var sessionBackup = Path.Combine(profile, "sessionstore-backups");
                                var sessionFile = Path.Combine(profile, "sessionstore.jsonlz4");
                                var historyDb = Path.Combine(profile, "places.sqlite");
                                if (Directory.Exists(sessionBackup)) ScanDirectoryParallel(sessionBackup, items, P, cancellationToken);
                                if (File.Exists(sessionFile)) items.Add(new ReportItem{ Path = sessionFile, IsDirectory = false, Size = new FileInfo(sessionFile).Length });
                                if (File.Exists(historyDb)) items.Add(new ReportItem{ Path = historyDb, IsDirectory = false, Size = new FileInfo(historyDb).Length });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "historique navigateurs", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.CleanBrowsers)
            {
                // Chrome
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!Directory.Exists(BrowserPaths.ChromeCache)) return;
                        P($"Scan cache Chrome: {BrowserPaths.ChromeCache}");
                        ScanDirectoryParallel(BrowserPaths.ChromeCache, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "Chrome", ex.Message));
                    }
                }, cancellationToken));

                // Edge
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!Directory.Exists(BrowserPaths.EdgeCache)) return;
                        P($"Scan cache Edge: {BrowserPaths.EdgeCache}");
                        ScanDirectoryParallel(BrowserPaths.EdgeCache, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "Edge", ex.Message));
                    }
                }, cancellationToken));

                // Firefox
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (!BrowserPaths.IsFirefoxInstalled) return;
                        P($"Scan cache Firefox: {BrowserPaths.FirefoxProfiles}");
                        var profiles = Directory.GetDirectories(BrowserPaths.FirefoxProfiles);
                        foreach (var profile in profiles)
                        {
                            var cacheDir = BrowserPaths.GetFirefoxCache(profile);
                            if (Directory.Exists(cacheDir))
                            {
                                ScanDirectoryParallel(cacheDir, items, P, cancellationToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "Firefox", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.CleanWindowsUpdate)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var sd = BrowserPaths.WindowsUpdateCache;
                        P($"Scan SoftwareDistribution\\Download: {sd}");
                        ScanDirectoryParallel(sd, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "SoftwareDistribution", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.CleanThumbnails)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var thumbDir = BrowserPaths.ThumbnailsCache;
                        P($"Scan vignettes: {thumbDir}");
                        if (Directory.Exists(thumbDir))
                        {
                            foreach (var f in Directory.GetFiles(thumbDir, "thumbcache_*.db"))
                            {
                                try 
                                { 
                                    var fi = new FileInfo(f);
                                    items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false }); 
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(LogLevel.Error, LanguageManager.Get("error_reading_file", f, ex.Message));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "vignettes", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.CleanPrefetch)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var prefetch = BrowserPaths.Prefetch;
                        P($"Scan Prefetch: {prefetch}");
                        ScanDirectoryParallel(prefetch, items, P, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "Prefetch", ex.Message));
                    }
                }, cancellationToken));
            }

            if (options.CleanOrphanedFiles)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var userTemp = BrowserPaths.UserTemp;
                        P($"Scan fichiers orphelins: {userTemp}");
                        var orphanFiles = Directory.GetFiles(userTemp, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(f => new FileInfo(f).CreationTime < DateTime.Now.AddDays(-7)).ToList();
                        foreach (var f in orphanFiles)
                        {
                            try 
                            { 
                                var fi = new FileInfo(f);
                                items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false }); 
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Error, LanguageManager.Get("error_reading_file", f, ex.Message));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_scan", "fichiers orphelins", ex.Message));
                    }
                }, cancellationToken));
            }

            // Wait for all parallel scans to complete
            try
            {
                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                P("Scan du rapport annulé par l'utilisateur");
            }

            // Add all items to the report
            foreach (var item in items)
            {
                report.Items.Add(item);
            }

            if (options.EmptyRecycleBin)
            {
                P("Note: la Corbeille sera vidée (taille non estimée)");
            }

            return report;
        }

        /// <summary>
        /// Crée un logger thread-safe qui respecte le flag verbose
        /// </summary>
        private static Action<string> CreateThreadSafeLogger(bool verbose, Action<string>? log)
        {
            if (!verbose) return _ => { };
            
            var lockObj = new object();
            return msg =>
            {
                lock (lockObj)
                {
                    log?.Invoke(msg);
                }
            };
        }

        /// <summary>
        /// Scanne un répertoire de manière parallèle et remplit le bag d'éléments
        /// </summary>
        private static void ScanDirectoryParallel(string path, ConcurrentBag<ReportItem> items, Action<string>? progress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            try
            {
                // Utilise EnumerateFiles pour éviter les allocations massives
                int fileCount = 0;
                foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    try 
                    { 
                        var fi = new FileInfo(f);
                        items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false });
                        
                        // Optimise la mémoire tous les 5000 fichiers
                        if ((++fileCount % 5000) == 0)
                        {
                            GC.Collect(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_reading_file", f, ex.Message));
                    }
                }
                
                // Traite les répertoires
                int dirCount = 0;
                foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    try 
                    { 
                        items.Add(new ReportItem { Path = d, Size = 0, IsDirectory = true });
                        
                        // Optimise la mémoire tous les 1000 répertoires
                        if ((++dirCount % 1000) == 0)
                        {
                            GC.Collect(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, $"Erreur lors du scan du répertoire {d}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) 
            { 
                progress?.Invoke($"Erreur en scannant {path}: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur scan répertoire {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime le contenu d'un répertoire de manière sécurisée
        /// </summary>
        private static (int files, long bytes) DeleteDirectoryContents(string path, bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return (0, 0);

            int deleted = 0;
            long bytes = 0;
            var lockObj = new object();

            try
            {
                // Utilise EnumerateFileSystemEntries pour une énumération progressive
                Parallel.ForEach(Directory.EnumerateFileSystemEntries(path), 
                    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cancellationToken }, 
                    entry =>
                {
                    try
                    {
                        if (File.Exists(entry))
                        {
                            var (ok, freed) = TryDeleteFileWithRetries(entry, dryRun, log);
                            if (ok)
                            {
                                lock (lockObj)
                                {
                                    bytes += freed;
                                    deleted++;
                                }
                                log(LanguageManager.Get("log_deleted", entry));
                            }
                        }
                        else if (Directory.Exists(entry))
                        {
                            var (ok, freed) = TryDeleteDirectoryWithRetries(entry, dryRun, log);
                            if (ok)
                            {
                                lock (lockObj)
                                {
                                    deleted++;
                                    bytes += freed;
                                }
                                log(LanguageManager.Get("log_deleted_folder", entry));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log(LanguageManager.Get("log_cannot_delete_entry", entry, ex.Message));
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", $"suppression {entry}", ex.Message));
                    }
                });
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("log_error_listing", path, ex.Message));
                Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", $"énumération {path}", ex.Message));
            }

            return (deleted, bytes);
        }

        /// <summary>
        /// Vérifie si un fichier est actuellement verrouillé par un processus
        /// </summary>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tente de supprimer un fichier avec retries intelligents et support MoveFileEx
        /// </summary>
        private static (bool deleted, long bytesFreed) TryDeleteFileWithRetries(string filePath, bool dryRun, Action<string> log, int maxAttempts = 8)
        {
            if (!File.Exists(filePath)) return (false, 0);
            long size = 0;
            try
            {
                var fi = new FileInfo(filePath);
                size = fi.Length;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Debug, $"Impossible de lire la taille de {filePath}: {ex.Message}");
                return (false, 0);
            }

            if (dryRun)
            {
                log(LanguageManager.Get("log_dryrun_deletion_planned", filePath));
                return (true, size);
            }

            int attempt = 0;
            var delay = 50; // Démarrer plus agressif
            var isLocked = false;

            while (attempt < maxAttempts)
            {
                try
                {
                    // Retirer l'attribut readonly
                    var attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                    }
                    
                    // Tenter suppression directe
                    File.Delete(filePath);
                    return (true, size);
                }
                catch (IOException ioEx)
                {
                    isLocked = IsFileLocked(filePath);
                    attempt++;
                    
                    if (attempt < maxAttempts)
                    {
                        Thread.Sleep(delay);
                        delay = Math.Min(delay * 2, 1000); // Exponential backoff: 50ms, 100ms, 200ms... max 1s
                        continue;
                    }

                    // Dernière tentative échouée - essayer MoveFileEx pour suppression au redémarrage
                    if (isLocked)
                    {
                        try
                        {
                            if (MoveFileEx(filePath, null, MOVEFILE_DELAY_UNTIL_REBOOT))
                            {
                                Logger.Log(LogLevel.Debug, $"Fichier verrouillé marqué pour suppression au redémarrage: {Path.GetFileName(filePath)}");
                                log(LanguageManager.Get("log_file_locked", Path.GetFileName(filePath)));
                                return (true, size); // Compte comme supprimé car sera supprimé au boot
                            }
                        }
                        catch (Exception moveEx)
                        {
                            Logger.Log(LogLevel.Debug, $"Impossible de marquer {Path.GetFileName(filePath)} pour suppression au boot: {moveEx.Message}");
                        }
                    }
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    // Fichier système ou protégé - ignorer
                    Logger.Log(LogLevel.Debug, $"Accès refusé (fichier protégé): {Path.GetFileName(filePath)}");
                    return (false, 0);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Debug, $"Erreur suppression {Path.GetFileName(filePath)}: {ex.Message}");
                    return (false, 0);
                }
            }

            // Fichier toujours verrouillé après retry et MoveFileEx
            Logger.Log(LogLevel.Debug, $"Fichier verrouillé (ignoré, normal): {Path.GetFileName(filePath)}");
            return (false, 0);
        }

        /// <summary>
        /// Tente de supprimer un répertoire avec retries en cas d'accès verrouillé
        /// </summary>
        private static (bool deleted, long bytesFreed) TryDeleteDirectoryWithRetries(string dirPath, bool dryRun, Action<string> log, int maxAttempts = 6)
        {
            if (!Directory.Exists(dirPath)) return (false, 0);

            long totalFreed = 0;
            try
            {
                // Try to calculate approximate size (sum of file lengths)
                foreach (var f in Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    try { totalFreed += new FileInfo(f).Length; } 
                    catch { /* Ignorer les erreurs d'accès individuelles */ }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Debug, $"Erreur énumération taille {Path.GetFileName(dirPath)}: {ex.Message}");
            }

            if (dryRun)
            {
                log(LanguageManager.Get("log_dryrun_folder_deletion", dirPath));
                return (true, totalFreed);
            }

            int attempt = 0;
            var delay = 150;
            while (attempt < maxAttempts)
            {
                try
                {
                    // Retirer l'attribut readonly des fichiers et du dossier
                    try
                    {
                        foreach (var f in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                var attr = File.GetAttributes(f);
                                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                    File.SetAttributes(f, attr & ~FileAttributes.ReadOnly);
                            }
                            catch { /* Ignorer */ }
                        }
                    }
                    catch { /* Ignorer */ }
                    
                    Directory.Delete(dirPath, true);
                    return (true, totalFreed);
                }
                catch (IOException)
                {
                    attempt++;
                    if (attempt < maxAttempts)
                    {
                        Thread.Sleep(delay);
                        delay = Math.Min(delay * 2, 2500); // Cap à 2.5s
                    }
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    Logger.Log(LogLevel.Debug, $"Accès refusé (protégé): {Path.GetFileName(dirPath)}");
                    return (false, 0);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Debug, $"Erreur suppression dossier {Path.GetFileName(dirPath)}: {ex.Message}");
                    return (false, 0);
                }
            }

            // Dossier verrouillé - ignorer silencieusement
            Logger.Log(LogLevel.Debug, $"Dossier verrouillé ignoré: {Path.GetFileName(dirPath)}");
            return (false, 0);
        }
        
        #region Software-Specific Cleaning Methods
        
        /// <summary>
        /// Nettoie les images et conteneurs Docker inutilisés
        /// </summary>
        private static (int files, long bytes) CleanDocker(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage Docker (images, conteneurs, volumes inutilisés)...");
            
            try
            {
                if (dryRun)
                {
                    log("(dry-run) Exécution simulée: docker system prune -af");
                    return (0, 0);
                }
                
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "system prune -af --volumes",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = System.Diagnostics.Process.Start(psi);
                if (process == null)
                {
                    log("Docker n'est pas installé ou accessible");
                    return (0, 0);
                }
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                log(LanguageManager.Get("log_docker_cleaned", output));
                Logger.Log(LogLevel.Info, "Nettoyage Docker terminé");
                
                return (1, 0); // Impossible de déterminer l'espace exact libéré
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("error_cleaning", "Docker", ex.Message));
                Logger.Log(LogLevel.Error, LanguageManager.Get("error_cleaning", "Docker", ex.Message));
                return (0, 0);
            }
        }
        
        /// <summary>
        /// Nettoie les dossiers node_modules anciens (> 30 jours)
        /// </summary>
        private static (int files, long bytes) CleanNodeModules(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Recherche de dossiers node_modules anciens...");
            
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var searchPaths = new[] 
            { 
                Path.Combine(userProfile, "Documents"),
                Path.Combine(userProfile, "Desktop"),
                Path.Combine(userProfile, "Downloads")
            };
            
            int totalDeleted = 0;
            long totalFreed = 0;
            
            foreach (var searchPath in searchPaths)
            {
                if (!Directory.Exists(searchPath)) continue;
                
                try
                {
                    FindAndCleanNodeModules(searchPath, dryRun, log, ref totalDeleted, ref totalFreed, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Erreur scan node_modules dans {searchPath}: {ex.Message}");
                }
            }
            
            log(LanguageManager.Get("log_node_modules_finished", totalDeleted, FormatBytes(totalFreed)));
            return (totalDeleted, totalFreed);
        }
        
        private static void FindAndCleanNodeModules(
            string path, 
            bool dryRun, 
            Action<string> log, 
            ref int totalDeleted, 
            ref long totalFreed,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    var dirName = Path.GetFileName(dir);
                    
                    // Si c'est un dossier node_modules
                    if (dirName.Equals("node_modules", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var dirInfo = new DirectoryInfo(dir);
                            var age = DateTime.Now - dirInfo.LastWriteTime;
                            
                            // Supprimer si > 30 jours
                            if (age.TotalDays > 30)
                            {
                                var size = GetDirectorySize(dir);
                                
                                if (!dryRun)
                                {
                                    Directory.Delete(dir, true);
                                }
                                
                                totalDeleted++;
                                totalFreed += size;
                                log(LanguageManager.Get("log_node_modules_deleted", FormatBytes(size), dir));
                            }
                        }
                        catch (Exception ex)
                        {
                            log(LanguageManager.Get("error_cleaning", $"node_modules {dir}", ex.Message));
                        }
                        
                        continue; // Ne pas entrer dans node_modules
                    }
                    
                    // Continuer la recherche récursive
                    FindAndCleanNodeModules(dir, dryRun, log, ref totalDeleted, ref totalFreed, cancellationToken);
                }
            }
            catch { /* Ignorer erreurs d'accès */ }
        }
        
        /// <summary>
        /// Nettoie les caches Visual Studio (obj, bin, .vs)
        /// </summary>
        private static (int files, long bytes) CleanVisualStudio(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage des caches Visual Studio...");
            
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            int totalDeleted = 0;
            long totalFreed = 0;
            
            // Nettoyer le cache VS dans AppData
            var vsCachePaths = new[]
            {
                Path.Combine(localAppData, "Microsoft", "VisualStudio"),
                Path.Combine(localAppData, "Microsoft", "VSApplicationInsights"),
                Path.Combine(localAppData, "Microsoft", "VisualStudio Services"),
                Path.Combine(userProfile, ".nuget", "packages")
            };
            
            foreach (var cachePath in vsCachePaths)
            {
                if (Directory.Exists(cachePath))
                {
                    try
                    {
                        var cacheDir = Path.Combine(cachePath, "Cache");
                        if (Directory.Exists(cacheDir))
                        {
                            var result = DeleteDirectoryContents(cacheDir, dryRun, log, cancellationToken);
                            totalDeleted += result.files;
                            totalFreed += result.bytes;
                        }
                    }
                    catch { }
                }
            }
            
            // Chercher et nettoyer obj/bin dans les projets
            var searchPaths = new[] 
            { 
                Path.Combine(userProfile, "source"),
                Path.Combine(userProfile, "Documents"),
                Path.Combine(userProfile, "Desktop")
            };
            
            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    try
                    {
                        FindAndCleanBuildFolders(searchPath, dryRun, log, ref totalDeleted, ref totalFreed, cancellationToken);
                    }
                    catch { }
                }
            }
            
            log(LanguageManager.Get("log_visual_studio_finished", FormatBytes(totalFreed)));
            return (totalDeleted, totalFreed);
        }
        
        private static void FindAndCleanBuildFolders(
            string path,
            bool dryRun,
            Action<string> log,
            ref int totalDeleted,
            ref long totalFreed,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    var dirName = Path.GetFileName(dir).ToLower();
                    
                    // Supprimer les dossiers obj, bin, .vs
                    if (dirName == "obj" || dirName == "bin" || dirName == ".vs")
                    {
                        try
                        {
                            var size = GetDirectorySize(dir);
                            
                            if (!dryRun)
                            {
                                Directory.Delete(dir, true);
                            }
                            
                            totalDeleted++;
                            totalFreed += size;
                            log(LanguageManager.Get("log_build_folder_deleted", FormatBytes(size), dir));
                        }
                        catch { }
                        
                        continue;
                    }
                    
                    FindAndCleanBuildFolders(dir, dryRun, log, ref totalDeleted, ref totalFreed, cancellationToken);
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Nettoie les caches Python (__pycache__, .pyc)
        /// </summary>
        private static (int files, long bytes) CleanPythonCache(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage des caches Python...");
            
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var searchPaths = new[] 
            { 
                Path.Combine(userProfile, "Documents"),
                Path.Combine(userProfile, "Desktop"),
                Path.Combine(userProfile, "Downloads")
            };
            
            int totalDeleted = 0;
            long totalFreed = 0;
            
            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    try
                    {
                        FindAndCleanPythonCache(searchPath, dryRun, log, ref totalDeleted, ref totalFreed, cancellationToken);
                    }
                    catch { }
                }
            }
            
            log(LanguageManager.Get("log_python_finished", totalDeleted, FormatBytes(totalFreed)));
            return (totalDeleted, totalFreed);
        }
        
        private static void FindAndCleanPythonCache(
            string path,
            bool dryRun,
            Action<string> log,
            ref int totalDeleted,
            ref long totalFreed,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            
            try
            {
                // Supprimer les fichiers .pyc
                foreach (var file in Directory.GetFiles(path, "*.pyc"))
                {
                    try
                    {
                        var size = new FileInfo(file).Length;
                        if (!dryRun) File.Delete(file);
                        totalDeleted++;
                        totalFreed += size;
                        log(LanguageManager.Get("log_pyc_deleted", file));
                    }
                    catch { }
                }
                
                foreach (var dir in Directory.GetDirectories(path))
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    var dirName = Path.GetFileName(dir);
                    
                    // Supprimer les dossiers __pycache__
                    if (dirName.Equals("__pycache__", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var size = GetDirectorySize(dir);
                            if (!dryRun) Directory.Delete(dir, true);
                            totalDeleted++;
                            totalFreed += size;
                            log(LanguageManager.Get("log_pycache_deleted", FormatBytes(size), dir));
                        }
                        catch { }
                        
                        continue;
                    }
                    
                    FindAndCleanPythonCache(dir, dryRun, log, ref totalDeleted, ref totalFreed, cancellationToken);
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Nettoie les objets Git non référencés
        /// </summary>
        private static (int files, long bytes) CleanGitCache(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage des caches Git...");
            
            if (dryRun)
            {
                log("(dry-run) Exécution simulée de git gc dans tous les repositories");
                return (0, 0);
            }
            
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var searchPaths = new[] 
            { 
                Path.Combine(userProfile, "source"),
                Path.Combine(userProfile, "Documents"),
                Path.Combine(userProfile, "Desktop")
            };
            
            int reposCleaned = 0;
            
            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    try
                    {
                        FindAndCleanGitRepos(searchPath, log, ref reposCleaned, cancellationToken);
                    }
                    catch { }
                }
            }
            
            log(LanguageManager.Get("log_git_finished", reposCleaned));
            return (reposCleaned, 0);
        }
        
        /// <summary>Nettoie le cache VS Code</summary>
        private static (int files, long bytes) CleanVsCodeCache(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage cache VS Code...");
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var vsCodePath = Path.Combine(userProfile, "Code", "Cache");
            
            if (!Directory.Exists(vsCodePath))
                return (0, 0);
            
            return dryRun 
                ? (0, 0) 
                : DeleteDirectoryContents(vsCodePath, dryRun, log, cancellationToken);
        }
        
        /// <summary>Nettoie le cache NuGet</summary>
        private static (int files, long bytes) CleanNugetCache(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage cache NuGet...");
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var nugetPath = Path.Combine(userProfile, ".nuget", "packages");
            
            if (!Directory.Exists(nugetPath))
                return (0, 0);
            
            // Supprimer les packages non référencés (plus de 30 jours)
            var cutoffDate = DateTime.Now.AddDays(-30);
            int deleted = 0;
            long freed = 0;
            
            try
            {
                foreach (var dir in Directory.GetDirectories(nugetPath))
                {
                    if (Directory.GetLastAccessTime(dir) < cutoffDate)
                    {
                        if (!dryRun)
                        {
                            var size = GetDirectorySize(dir);
                            Directory.Delete(dir, true);
                            deleted++;
                            freed += size;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Debug, $"Erreur nettoyage NuGet: {ex.Message}");
            }
            
            return (deleted, freed);
        }
        
        /// <summary>Nettoie le cache Maven</summary>
        private static (int files, long bytes) CleanMavenCache(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage cache Maven...");
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var mavenPath = Path.Combine(userProfile, ".m2", "repository");
            
            if (!Directory.Exists(mavenPath))
                return (0, 0);
            
            return dryRun 
                ? (0, 0) 
                : DeleteDirectoryContents(mavenPath, dryRun, log, cancellationToken);
        }
        
        /// <summary>Nettoie le cache npm global</summary>
        private static (int files, long bytes) CleanNpmCache(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage cache npm...");
            
            if (dryRun)
            {
                log("(dry-run) Exécution simulée de npm cache clean");
                return (0, 0);
            }
            
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "cache clean --force",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                
                using var proc = System.Diagnostics.Process.Start(psi);
                proc?.WaitForExit(10000);
                log("Cache npm vidé avec succès");
                return (1, 0);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Debug, $"Erreur nettoyage npm: {ex.Message}");
                return (0, 0);
            }
        }
        
        /// <summary>Nettoie les caches de jeux (Steam, Epic)</summary>
        private static (int files, long bytes) CleanGameCaches(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage caches de jeux...");
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            int totalDeleted = 0;
            long totalFreed = 0;
            
            // Steam
            var steamPath = Path.Combine(userProfile, "Steam", "htmlcache");
            if (Directory.Exists(steamPath))
            {
                var r = DeleteDirectoryContents(steamPath, dryRun, log, cancellationToken);
                totalDeleted += r.files;
                totalFreed += r.bytes;
            }
            
            // Epic Games
            var epicPath = Path.Combine(userProfile, "EpicGamesLauncher", "Saved", "webcache");
            if (Directory.Exists(epicPath))
            {
                var r = DeleteDirectoryContents(epicPath, dryRun, log, cancellationToken);
                totalDeleted += r.files;
                totalFreed += r.bytes;
            }
            
            log(LanguageManager.Get("log_game_caches_cleaned", totalDeleted));
            return (totalDeleted, totalFreed);
        }
        
        private static void FindAndCleanGitRepos(
            string path,
            Action<string> log,
            ref int reposCleaned,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    var dirName = Path.GetFileName(dir);
                    
                    // Si c'est un dossier .git
                    if (dirName.Equals(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // Exécuter git gc dans le repository parent
                            var repoPath = Path.GetDirectoryName(dir);
                            if (repoPath != null)
                            {
                                var psi = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "git",
                                    Arguments = "gc --aggressive --prune=now",
                                    WorkingDirectory = repoPath,
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                };
                                
                                using var process = System.Diagnostics.Process.Start(psi);
                                if (process != null)
                                {
                                    process.WaitForExit(30000); // Timeout 30s
                                    reposCleaned++;
                                    log(LanguageManager.Get("log_git_repo_optimized", repoPath));
                                }
                            }
                        }
                        catch { }
                        
                        continue; // Ne pas entrer dans .git
                    }
                    
                    FindAndCleanGitRepos(dir, log, ref reposCleaned, cancellationToken);
                }
            }
            catch { }
        }
        
        #endregion
        
        #region Privacy Cleaning Methods
        
        /// <summary>
        /// Nettoie l'historique Exécuter (Win+R)
        /// </summary>
        private static (int files, long bytes) CleanRunHistory(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage de l'historique Exécuter...");
            
            try
            {
                var runHistoryKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU";
                
                if (dryRun)
                {
                    log(LanguageManager.Get("log_dryrun_deletion_planned", runHistoryKey));
                    return (1, 0);
                }
                
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(runHistoryKey, true);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        if (valueName != "MRUList")
                        {
                            key.DeleteValue(valueName, false);
                        }
                    }
                    key.SetValue("MRUList", "");
                }
                
                log("Historique Exécuter nettoyé");
                return (1, 0);
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("error_cleaning", "historique Exécuter", ex.Message));
                return (0, 0);
            }
        }
        
        /// <summary>
        /// Nettoie les documents récents
        /// </summary>
        private static (int files, long bytes) CleanRecentDocuments(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage des documents récents...");
            
            var recentPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Microsoft\Windows\Recent"
            );
            
            try
            {
                if (!Directory.Exists(recentPath))
                    return (0, 0);
                
                var result = DeleteDirectoryContents(recentPath, dryRun, log, cancellationToken);
                log(LanguageManager.Get("log_recent_docs_cleaned", result.files));
                return result;
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("error_cleaning", "documents récents", ex.Message));
                return (0, 0);
            }
        }
        
        /// <summary>
        /// Nettoie la Timeline Windows
        /// </summary>
        private static (int files, long bytes) CleanWindowsTimeline(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage de la Timeline Windows...");
            
            var timelinePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"ConnectedDevicesPlatform"
            );
            
            try
            {
                if (!Directory.Exists(timelinePath))
                    return (0, 0);
                
                var result = DeleteDirectoryContents(timelinePath, dryRun, log, cancellationToken);
                log(LanguageManager.Get("log_timeline_cleaned", result.files));
                return result;
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("error_cleaning", "Timeline", ex.Message));
                return (0, 0);
            }
        }
        
        /// <summary>
        /// Nettoie l'historique de recherche Windows
        /// </summary>
        private static (int files, long bytes) CleanSearchHistory(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Nettoyage de l'historique de recherche...");
            
            try
            {
                var searchHistoryKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\WordWheelQuery";
                
                if (dryRun)
                {
                    log(LanguageManager.Get("log_dryrun_deletion_planned", searchHistoryKey));
                    return (1, 0);
                }
                
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(searchHistoryKey, true);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        if (valueName != "MRUListEx")
                        {
                            key.DeleteValue(valueName, false);
                        }
                    }
                }
                
                log("Historique de recherche nettoyé");
                return (1, 0);
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("error_cleaning", "historique recherche", ex.Message));
                return (0, 0);
            }
        }
        
        /// <summary>
        /// Vide le presse-papiers Windows
        /// </summary>
        private static (int files, long bytes) CleanClipboard(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Vidage du presse-papiers...");
            
            try
            {
                if (dryRun)
                {
                    log("(dry-run) Vidage simulé du presse-papiers");
                    return (1, 0);
                }
                
                System.Windows.Forms.Clipboard.Clear();
                log("Presse-papiers vidé");
                return (1, 0);
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("error_cleaning", "presse-papiers", ex.Message));
                return (0, 0);
            }
        }
        
        private static (int files, long bytes) CleanBrokenShortcuts(bool dryRun, Action<string> log, CancellationToken cancellationToken)
        {
            log("Recherche des raccourcis cassés...");
            int filesDeleted = 0;
            long bytesFreed = 0;

            var shortcutLocations = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Recent),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Recent"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Links")
            };

            foreach (var location in shortcutLocations)
            {
                if (string.IsNullOrEmpty(location) || !Directory.Exists(location))
                    continue;

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var shortcuts = Directory.GetFiles(location, "*.lnk", SearchOption.AllDirectories);
                    log(LanguageManager.Get("log_shortcuts_verified", shortcuts.Length, location));

                    foreach (var shortcut in shortcuts)
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            if (IsShortcutBroken(shortcut, log))
                            {
                                var fileInfo = new FileInfo(shortcut);
                                long size = fileInfo.Length;

                                if (!dryRun)
                                {
                                    File.Delete(shortcut);
                                    log(LanguageManager.Get("log_shortcut_deleted", Path.GetFileName(shortcut)));
                                }
                                else
                                {
                                    log(LanguageManager.Get("log_dryrun_would_delete", Path.GetFileName(shortcut)));
                                }

                                filesDeleted++;
                                bytesFreed += size;
                            }
                        }
                        catch (Exception ex)
                        {
                            log(LanguageManager.Get("error_cleaning", $"vérification {Path.GetFileName(shortcut)}", ex.Message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log(LanguageManager.Get("error_cleaning", $"scan {location}", ex.Message));
                }
            }

            log(LanguageManager.Get("log_broken_shortcuts_found", filesDeleted, FormatBytes(bytesFreed)));
            return (filesDeleted, bytesFreed);
        }

        private static bool IsShortcutBroken(string shortcutPath, Action<string> log)
        {
            try
            {
                // Utiliser IWshRuntimeLibrary pour lire le raccourci
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                    return false;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null)
                    return false;

                try
                {
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    string targetPath = shortcut.TargetPath;

                    // Si la cible est vide ou n'existe pas, le raccourci est cassé
                    if (string.IsNullOrWhiteSpace(targetPath))
                        return true;

                    // Vérifier si le fichier ou dossier cible existe
                    bool exists = File.Exists(targetPath) || Directory.Exists(targetPath);
                    return !exists;
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);
                }
            }
            catch (Exception ex)
            {
                log(LanguageManager.Get("log_cannot_verify_shortcut", Path.GetFileName(shortcutPath), ex.Message));
                return false; // En cas d'erreur, ne pas supprimer par sécurité
            }
        }
        
        #endregion
        
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
        
        /// <summary>
        /// Ferme les processus des navigateurs web (Chrome, Edge, Firefox)
        /// </summary>
        private static void CloseBrowserProcesses(Action<string> log)
        {
            string[] browserProcesses = { "chrome", "msedge", "firefox", "brave", "opera", "vivaldi" };
            int closedCount = 0;
            
            foreach (var browserName in browserProcesses)
            {
                try
                {
                    var processes = Process.GetProcessesByName(browserName);
                    if (processes.Length > 0)
                    {
                        log(LanguageManager.Get("log_closing_browsers", processes.Length, browserName));
                        foreach (var proc in processes)
                        {
                            try
                            {
                                proc.CloseMainWindow(); // Tentative de fermeture propre
                                if (!proc.WaitForExit(3000)) // Attendre 3s
                                {
                                    proc.Kill(); // Forcer si n\u00e9cessaire
                                }
                                closedCount++;
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Debug, $"Impossible de fermer {browserName}: {ex.Message}");
                            }
                            finally
                            {
                                proc.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Debug, $"Erreur recherche processus {browserName}: {ex.Message}");
                }
            }
            
            if (closedCount > 0)
            {
                log($"\u2713 {closedCount} navigateur(s) ferm\u00e9(s) avec succ\u00e8s");
            }
            else
            {
                log("Aucun navigateur en cours d'ex\u00e9cution");
            }
        }
    }
}
