using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WindowsCleaner
{
    public class CleanerOptions
    {
        public bool DryRun { get; set; }
        public bool EmptyRecycleBin { get; set; }
        public bool IncludeSystemTemp { get; set; }
        public bool CleanBrowsers { get; set; }
        public bool CleanWindowsUpdate { get; set; }
        public bool CleanThumbnails { get; set; }
        public bool CleanPrefetch { get; set; }
        public bool FlushDns { get; set; }
        public bool Verbose { get; set; }
    }

    public class CleanerResult
    {
        public int FilesDeleted { get; set; }
        public long BytesFreed { get; set; }
    }

    public class ReportItem
    {
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
    }

    public class CleanerReport
    {
        public System.Collections.Generic.List<ReportItem> Items { get; } = new System.Collections.Generic.List<ReportItem>();
        public long TotalBytes => Items.Sum(i => i.Size);
        public int Count => Items.Count;
    }

    public static class Cleaner
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        // Flags for SHEmptyRecycleBin
        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI = 0x00000002;
        private const uint SHERB_NOSOUND = 0x00000004;

        public static CleanerResult RunCleanup(CleanerOptions options, Action<string>? log = null)
        {
            var result = new CleanerResult();
            var lockObj = new object();

            void Log(string s)
            {
                if (options.Verbose)
                    log?.Invoke(s);
            }

            void AddResult(int files, long bytes)
            {
                lock (lockObj)
                {
                    result.FilesDeleted += files;
                    result.BytesFreed += bytes;
                }
            }

            var tasks = new System.Collections.Generic.List<Task>();

            // User temp
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    string userTemp = Path.GetTempPath();
                    Log($"Nettoyage du dossier temporaire utilisateur: {userTemp}");
                    var r = DeleteDirectoryContents(userTemp, options.DryRun, Log);
                    AddResult(r.files, r.bytes);
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage user temp: " + ex.Message);
                }
            }));

            // LocalAppData\Temp
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var localTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
                    Log($"Nettoyage du dossier LocalAppData Temp: {localTemp}");
                    var r = DeleteDirectoryContents(localTemp, options.DryRun, Log);
                    AddResult(r.files, r.bytes);
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage LocalAppData Temp: " + ex.Message);
                }
            }));

            // System temp (optional)
            if (options.IncludeSystemTemp)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        var systemTemp = Path.Combine(windows, "Temp");
                        Log($"Nettoyage du dossier Temp système: {systemTemp}");
                        var r = DeleteDirectoryContents(systemTemp, options.DryRun, Log);
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage System Temp: " + ex.Message);
                    }
                }));
            }

            // Browser caches (Chrome, Edge, Firefox)
            if (options.CleanBrowsers)
            {
                // Chrome cache
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var chromeCache = Path.Combine(local, "Google", "Chrome", "User Data", "Default", "Cache");
                        Log($"Nettoyage cache Chrome: {chromeCache}");
                        var r = DeleteDirectoryContents(chromeCache, options.DryRun, log ?? (s => { }));
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage Chrome: " + ex.Message);
                    }
                }));

                // Edge cache
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var edgeCache = Path.Combine(local, "Microsoft", "Edge", "User Data", "Default", "Cache");
                        Log($"Nettoyage cache Edge: {edgeCache}");
                        var r = DeleteDirectoryContents(edgeCache, options.DryRun, log ?? (s => { }));
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage Edge: " + ex.Message);
                    }
                }));

                // Firefox cache
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var firefoxCache = Path.Combine(local, "Mozilla", "Firefox", "Profiles");
                        if (Directory.Exists(firefoxCache))
                        {
                            Log($"Nettoyage cache Firefox: {firefoxCache}");
                            var profiles = Directory.GetDirectories(firefoxCache);
                            foreach (var profile in profiles)
                            {
                                var cacheDir = Path.Combine(profile, "cache2");
                                if (Directory.Exists(cacheDir))
                                {
                                    var r = DeleteDirectoryContents(cacheDir, options.DryRun, log ?? (s => { }));
                                    AddResult(r.files, r.bytes);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage Firefox: " + ex.Message);
                    }
                }));
            }

            // Windows Update cache (SoftwareDistribution) - requires admin
            if (options.CleanWindowsUpdate)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        var sd = Path.Combine(windir, "SoftwareDistribution", "Download");
                        Log($"Nettoyage SoftwareDistribution\\Download: {sd}");
                        var r = DeleteDirectoryContents(sd, options.DryRun, log ?? (s => { }));
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage SoftwareDistribution: " + ex.Message);
                    }
                }));
            }

            // Thumbnails
            if (options.CleanThumbnails)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var thumbDir = Path.Combine(local, "Microsoft", "Windows", "Explorer");
                        Log($"Nettoyage vignettes: {thumbDir}");
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
                                log?.Invoke($"Supprimé vignette: {f}");
                            }
                            catch (Exception ex)
                            {
                                log?.Invoke($"Impossible de supprimer vignette {f}: {ex.Message}");
                            }
                        }
                        AddResult(filesDeleted, bytesFreed);
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage vignettes: " + ex.Message);
                    }
                }));
            }

            // Prefetch (requires admin)
            if (options.CleanPrefetch)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        var prefetch = Path.Combine(windir, "Prefetch");
                        Log($"Nettoyage Prefetch: {prefetch}");
                        var r = DeleteDirectoryContents(prefetch, options.DryRun, log ?? (s => { }));
                        AddResult(r.files, r.bytes);
                    }
                    catch (Exception ex)
                    {
                        Log("Erreur nettoyage Prefetch: " + ex.Message);
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Flush DNS (sequential, quick operation)
            if (options.FlushDns)
            {
                try
                {
                    Log("Exécution flush DNS...");
                    if (!options.DryRun)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo("ipconfig", "/flushdns") { CreateNoWindow = true, UseShellExecute = false };
                        var p = System.Diagnostics.Process.Start(psi);
                        p?.WaitForExit(5000);
                    }
                    else
                    {
                        Log("(dry-run) ipconfig /flushdns non exécuté");
                    }
                }
                catch (Exception ex)
                {
                    Log("Erreur flush DNS: " + ex.Message);
                }
            }

            // Empty Recycle Bin (sequential, requires UI thread in some cases)
            if (options.EmptyRecycleBin)
            {
                try
                {
                    Log("Vidage de la Corbeille...");
                    if (!options.DryRun)
                    {
                        uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
                        SHEmptyRecycleBin(IntPtr.Zero, null, flags);
                    }
                    else
                    {
                        Log("(dry-run) Corbeille non vidée");
                    }
                }
                catch (Exception ex)
                {
                    Log("Erreur vidage Corbeille: " + ex.Message);
                }
            }

            return result;
        }

        public static CleanerReport GenerateReport(CleanerOptions options, Action<string>? progress = null)
        {
            var report = new CleanerReport();
            var items = new ConcurrentBag<ReportItem>();

            void P(string s) => progress?.Invoke(s);

            var tasks = new System.Collections.Generic.List<Task>();

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    string userTemp = Path.GetTempPath();
                    P($"Scan du dossier temporaire utilisateur: {userTemp}");
                    ScanDirectoryParallel(userTemp, items, P);
                }
                catch (Exception ex) { P("Erreur scan user temp: " + ex.Message); }
            }));

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var localTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
                    P($"Scan LocalAppData Temp: {localTemp}");
                    ScanDirectoryParallel(localTemp, items, P);
                }
                catch (Exception ex) { P("Erreur scan LocalAppData Temp: " + ex.Message); }
            }));

            if (options.IncludeSystemTemp)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        var systemTemp = Path.Combine(windows, "Temp");
                        P($"Scan Temp système: {systemTemp}");
                        ScanDirectoryParallel(systemTemp, items, P);
                    }
                    catch (Exception ex) { P("Erreur scan System Temp: " + ex.Message); }
                }));
            }

            if (options.CleanBrowsers)
            {
                // Chrome
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var chromeCache = Path.Combine(local, "Google", "Chrome", "User Data", "Default", "Cache");
                        P($"Scan cache Chrome: {chromeCache}");
                        ScanDirectoryParallel(chromeCache, items, P);
                    }
                    catch (Exception ex) { P("Erreur scan Chrome: " + ex.Message); }
                }));

                // Edge
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var edgeCache = Path.Combine(local, "Microsoft", "Edge", "User Data", "Default", "Cache");
                        P($"Scan cache Edge: {edgeCache}");
                        ScanDirectoryParallel(edgeCache, items, P);
                    }
                    catch (Exception ex) { P("Erreur scan Edge: " + ex.Message); }
                }));

                // Firefox
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var firefoxCache = Path.Combine(local, "Mozilla", "Firefox", "Profiles");
                        if (Directory.Exists(firefoxCache))
                        {
                            P($"Scan cache Firefox: {firefoxCache}");
                            var profiles = Directory.GetDirectories(firefoxCache);
                            foreach (var profile in profiles)
                            {
                                var cacheDir = Path.Combine(profile, "cache2");
                                if (Directory.Exists(cacheDir))
                                {
                                    ScanDirectoryParallel(cacheDir, items, P);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { P("Erreur scan Firefox: " + ex.Message); }
                }));
            }

            if (options.CleanWindowsUpdate)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        var sd = Path.Combine(windir, "SoftwareDistribution", "Download");
                        P($"Scan SoftwareDistribution\\Download: {sd}");
                        ScanDirectoryParallel(sd, items, P);
                    }
                    catch (Exception ex) { P("Erreur scan SoftwareDistribution: " + ex.Message); }
                }));
            }

            if (options.CleanThumbnails)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var thumbDir = Path.Combine(local, "Microsoft", "Windows", "Explorer");
                        P($"Scan vignettes: {thumbDir}");
                        if (Directory.Exists(thumbDir))
                        {
                            foreach (var f in Directory.GetFiles(thumbDir, "thumbcache_*.db"))
                            {
                                try { var fi = new FileInfo(f); items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false }); }
                                catch { }
                            }
                        }
                    }
                    catch (Exception ex) { P("Erreur scan vignettes: " + ex.Message); }
                }));
            }

            if (options.CleanPrefetch)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        var prefetch = Path.Combine(windir, "Prefetch");
                        P($"Scan Prefetch: {prefetch}");
                        ScanDirectoryParallel(prefetch, items, P);
                    }
                    catch (Exception ex) { P("Erreur scan Prefetch: " + ex.Message); }
                }));
            }

            // Wait for all parallel scans to complete
            Task.WaitAll(tasks.ToArray());

            // Add all items to the report
            foreach (var item in items)
            {
                report.Items.Add(item);
            }

            if (options.EmptyRecycleBin)
            {
                // We cannot enumerate recycle bin easily without COM; just note action
                P("Note: la Corbeille sera vidée (taille non estimée)");
            }

            return report;
        }

        private static void ScanDirectoryParallel(string path, ConcurrentBag<ReportItem> items, Action<string>? progress)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            try
            {
                Parallel.ForEach(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories), f =>
                {
                    try { var fi = new FileInfo(f); items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false }); }
                    catch { }
                });
                
                // Also add directories as items (size 0) for visibility
                Parallel.ForEach(Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories), d =>
                {
                    try { items.Add(new ReportItem { Path = d, Size = 0, IsDirectory = true }); }
                    catch { }
                });
            }
            catch (Exception ex) { progress?.Invoke($"Erreur en scannant {path}: {ex.Message}"); }
        }

        private static (int files, long bytes) DeleteDirectoryContents(string path, bool dryRun, Action<string>? log)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return (0, 0);

            int deleted = 0;
            long bytes = 0;
            var lockObj = new object();

            try
            {
                var entries = Directory.GetFileSystemEntries(path);
                Parallel.ForEach(entries, new ParallelOptions { MaxDegreeOfParallelism = 4 }, entry =>
                {
                    try
                    {
                        if (File.Exists(entry))
                        {
                            var fi = new FileInfo(entry);
                            var (ok, freed) = TryDeleteFileWithRetries(entry, dryRun, log);
                            if (ok)
                            {
                                lock (lockObj)
                                {
                                    bytes += freed;
                                    deleted++;
                                }
                                log?.Invoke($"Supprimé: {entry}");
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
                                log?.Invoke($"Supprimé dossier: {entry}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Impossible de supprimer {entry}: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur en listant {path}: {ex.Message}");
            }

            return (deleted, bytes);
        }

        private static (bool deleted, long bytesFreed) TryDeleteFileWithRetries(string filePath, bool dryRun, Action<string>? log, int maxAttempts = 5)
        {
            if (!File.Exists(filePath)) return (false, 0);
            long size = 0;
            try
            {
                var fi = new FileInfo(filePath);
                size = fi.Length;
            }
            catch { }

            if (dryRun)
            {
                log?.Invoke($"(dry-run) Suppression planifiée: {filePath}");
                return (true, size);
            }

            int attempt = 0;
            var delay = 150;
            while (attempt < maxAttempts)
            {
                try
                {
                    File.Delete(filePath);
                    return (true, size);
                }
                catch (IOException ioEx)
                {
                    attempt++;
                    log?.Invoke($"Fichier verrouillé, tentative {attempt}/{maxAttempts}: {filePath} - {ioEx.Message}");
                    Thread.Sleep(delay);
                    delay *= 2;
                    continue;
                }
                catch (UnauthorizedAccessException ua)
                {
                    log?.Invoke($"Accès refusé à {filePath}: {ua.Message}");
                    return (false, 0);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Erreur suppression fichier {filePath}: {ex.Message}");
                    return (false, 0);
                }
            }

            log?.Invoke($"Échec suppression après {maxAttempts} tentatives: {filePath}");
            return (false, 0);
        }

        private static (bool deleted, long bytesFreed) TryDeleteDirectoryWithRetries(string dirPath, bool dryRun, Action<string>? log, int maxAttempts = 4)
        {
            if (!Directory.Exists(dirPath)) return (false, 0);

            long totalFreed = 0;
            try
            {
                // Try to calculate approximate size (sum of file lengths)
                foreach (var f in Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    try { totalFreed += new FileInfo(f).Length; } catch { }
                }
            }
            catch { }

            if (dryRun)
            {
                log?.Invoke($"(dry-run) Suppression planifiée du dossier: {dirPath}");
                return (true, totalFreed);
            }

            int attempt = 0;
            var delay = 200;
            while (attempt < maxAttempts)
            {
                try
                {
                    Directory.Delete(dirPath, true);
                    return (true, totalFreed);
                }
                catch (IOException ioEx)
                {
                    attempt++;
                    log?.Invoke($"Dossier verrouillé, tentative {attempt}/{maxAttempts}: {dirPath} - {ioEx.Message}");
                    Thread.Sleep(delay);
                    delay *= 2;
                    continue;
                }
                catch (UnauthorizedAccessException ua)
                {
                    log?.Invoke($"Accès refusé au dossier {dirPath}: {ua.Message}");
                    return (false, 0);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Erreur suppression dossier {dirPath}: {ex.Message}");
                    return (false, 0);
                }
            }

            log?.Invoke($"Échec suppression dossier après {maxAttempts} tentatives: {dirPath}");
            return (false, 0);
        }
    }
}
