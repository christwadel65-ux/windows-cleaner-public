using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WindowsCleaner
{
    /// <summary>
    /// Gère la désinstallation complète de programmes
    /// </summary>
    public class ProgramUninstaller
    {
        private static readonly string[] UninstallRegistryPaths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 16;

        /// <summary>
        /// Représente un programme installé
        /// </summary>
        public class InstalledProgram
        {
            public string DisplayName { get; set; } = string.Empty;
            public string DisplayVersion { get; set; } = string.Empty;
            public string Publisher { get; set; } = string.Empty;
            public string InstallLocation { get; set; } = string.Empty;
            public string UninstallString { get; set; } = string.Empty;
            public string QuietUninstallString { get; set; } = string.Empty;
            public DateTime? InstallDate { get; set; }
            public long EstimatedSize { get; set; } // en KB

            public override string ToString() => $"{DisplayName} ({DisplayVersion})";
        }

        /// <summary>
        /// Récupère la liste de tous les programmes installés
        /// </summary>
        public static List<InstalledProgram> GetInstalledPrograms()
        {
            var programs = new List<InstalledProgram>();

            foreach (var registryPath in UninstallRegistryPaths)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
                    {
                        if (key == null) continue;

                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey == null) continue;

                                    var displayName = subKey.GetValue("DisplayName") as string;
                                    if (string.IsNullOrWhiteSpace(displayName)) continue;

                                    var uninstallString = subKey.GetValue("UninstallString") as string;
                                    if (string.IsNullOrWhiteSpace(uninstallString)) continue;

                                    // Ignorer les mises à jour Windows et les éléments système
                                    if (displayName.Contains("KB") || displayName.Contains("Windows") && displayName.Contains("Update"))
                                        continue;

                                    var program = new InstalledProgram
                                    {
                                        DisplayName = displayName,
                                        DisplayVersion = subKey.GetValue("DisplayVersion") as string ?? string.Empty,
                                        Publisher = subKey.GetValue("Publisher") as string ?? string.Empty,
                                        InstallLocation = subKey.GetValue("InstallLocation") as string ?? string.Empty,
                                        UninstallString = uninstallString,
                                        QuietUninstallString = subKey.GetValue("QuietUninstallString") as string ?? uninstallString,
                                        EstimatedSize = Convert.ToInt64(subKey.GetValue("EstimatedSize") ?? 0L)
                                    };

                                    // Essayer de parser la date d'installation
                                    var installDateStr = subKey.GetValue("InstallDate") as string;
                                    if (!string.IsNullOrEmpty(installDateStr) && installDateStr.Length >= 8)
                                    {
                                        if (DateTime.TryParseExact(installDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                                        {
                                            program.InstallDate = date;
                                        }
                                    }

                                    programs.Add(program);
                                }
                            }
                            catch { /* Ignorer les erreurs de lecture */}
                        }
                    }
                }
                catch { /* Ignorer les erreurs d'accès au registre */ }
            }

            return programs.OrderBy(p => p.DisplayName).ToList();
        }

        /// <summary>
        /// Recherche un programme par son nom
        /// </summary>
        public static InstalledProgram? FindProgram(string programName)
        {
            var programs = GetInstalledPrograms();
            return programs.FirstOrDefault(p => p.DisplayName.Contains(programName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Désinstalle complètement un programme
        /// </summary>
        public static bool UninstallProgram(InstalledProgram program, bool silent = true, Action<string>? log = null)
        {
            try
            {
                log?.Invoke($"Désinstallation du programme : {program.DisplayName}");

                // Utiliser la désinstallation silencieuse si disponible
                var uninstallString = silent && !string.IsNullOrEmpty(program.QuietUninstallString)
                    ? program.QuietUninstallString
                    : program.UninstallString;

                // Nettoyer la chaîne de désinstallation
                uninstallString = uninstallString.Trim();

                // Extraire le fichier exécutable et les arguments
                string exePath;
                string arguments = string.Empty;

                if (uninstallString.StartsWith("\""))
                {
                    var endQuote = uninstallString.IndexOf("\"", 1);
                    if (endQuote > 0)
                    {
                        exePath = uninstallString.Substring(1, endQuote - 1);
                        arguments = uninstallString.Substring(endQuote + 1).Trim();
                    }
                    else
                    {
                        exePath = uninstallString;
                    }
                }
                else
                {
                    var parts = uninstallString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    exePath = parts[0];
                    if (parts.Length > 1)
                    {
                        arguments = string.Join(" ", parts.Skip(1));
                    }
                }

                if (!File.Exists(exePath))
                {
                    log?.Invoke($"Fichier de désinstallation introuvable : {exePath}");
                    return false;
                }

                log?.Invoke($"Exécution : {exePath} {arguments}");

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        log?.Invoke("Impossible de démarrer le processus de désinstallation");
                        return false;
                    }

                    process.WaitForExit(300000); // Timeout 5 minutes

                    if (process.ExitCode == 0 || process.ExitCode == 3010) // 3010 = reboot required
                    {
                        log?.Invoke($"Désinstallation réussie : {program.DisplayName}");
                        
                        // Nettoyer immédiatement les entrées du registre
                        System.Threading.Thread.Sleep(1000); // Attendre 1s que les fichiers se libèrent
                        log?.Invoke("Nettoyage des entrées du registre...");
                        CleanupRegistryEntries(program.DisplayName, log);
                        
                        return true;
                    }
                    else
                    {
                        log?.Invoke($"Erreur de désinstallation (code {process.ExitCode})");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur lors de la désinstallation : {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur désinstallation {program.DisplayName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Nettoie les restes d'un programme (fichiers, dossiers, registre)
        /// </summary>
        public static void CleanupProgramRemnants(InstalledProgram program, Action<string>? log = null)
        {
            try
            {
                // Nettoyer le dossier d'installation
                if (!string.IsNullOrEmpty(program.InstallLocation) && Directory.Exists(program.InstallLocation))
                {
                    try
                    {
                        log?.Invoke($"Suppression du dossier : {program.InstallLocation}");
                        Directory.Delete(program.InstallLocation, true);
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Impossible de supprimer le dossier : {ex.Message}");
                    }
                }

                // Nettoyer les entrées du registre utilisateur
                CleanupRegistryEntries(program.DisplayName, log);

                // Nettoyer les fichiers dans AppData
                CleanupAppDataFiles(program.DisplayName, log);

                log?.Invoke($"Nettoyage des restes terminé pour : {program.DisplayName}");
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur lors du nettoyage : {ex.Message}");
            }
        }

        /// <summary>
        /// Nettoie les entrées du registre liées au programme
        /// </summary>
        private static void CleanupRegistryEntries(string programName, Action<string>? log = null)
        {
            try
            {
                // Nettoyer les chemins d'uninstall du registre
                var uninstallPaths = new[]
                {
                    (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
                    (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")
                };

                foreach (var (hive, path) in uninstallPaths)
                {
                    try
                    {
                        using (var key = hive.OpenSubKey(path, true))
                        {
                            if (key == null) continue;

                            var subKeyNames = key.GetSubKeyNames().ToList();
                            foreach (var subKeyName in subKeyNames)
                            {
                                try
                                {
                                    using (var subKey = key.OpenSubKey(subKeyName))
                                    {
                                        if (subKey == null) continue;
                                        
                                        var displayName = subKey.GetValue("DisplayName") as string;
                                        if (!string.IsNullOrEmpty(displayName) && 
                                            displayName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            key.DeleteSubKeyTree(subKeyName);
                                            log?.Invoke($"Entrée Uninstall supprimée : {displayName}");
                                        }
                                    }
                                }
                                catch { /* Ignorer les erreurs */ }
                            }
                        }
                    }
                    catch { /* Ignorer les erreurs */ }
                }

                // Nettoyer Software hive utilisateur
                var userSoftwarePaths = new[]
                {
                    @"Software",
                    @"Software\Microsoft\Windows\CurrentVersion"
                };

                foreach (var path in userSoftwarePaths)
                {
                    try
                    {
                        using (var key = Registry.CurrentUser.OpenSubKey(path, true))
                        {
                            if (key == null) continue;

                            var subKeyNames = key.GetSubKeyNames().ToList();
                            foreach (var subKeyName in subKeyNames)
                            {
                                if (subKeyName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        key.DeleteSubKeyTree(subKeyName);
                                        log?.Invoke($"Entrée registre utilisateur supprimée : {subKeyName}");
                                    }
                                    catch { /* Ignorer les erreurs */ }
                                }
                            }
                        }
                    }
                    catch { /* Ignorer les erreurs */ }
                }

                // Nettoyer Uninstall registre local machine (dernière chance)
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
                    {
                        if (key != null)
                        {
                            var subKeyNames = key.GetSubKeyNames().ToList();
                            foreach (var subKeyName in subKeyNames)
                            {
                                try
                                {
                                    using (var subKey = key.OpenSubKey(subKeyName))
                                    {
                                        var displayName = subKey?.GetValue("DisplayName") as string;
                                        if (displayName != null && displayName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            key.DeleteSubKeyTree(subKeyName);
                                            log?.Invoke($"✓ Entrée Uninstall supprimée définitivement : {displayName}");
                                        }
                                    }
                                }
                                catch { /* Ignorer */ }
                            }
                        }
                    }
                }
                catch { /* Ignorer */ }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur nettoyage registre : {ex.Message}");
            }
        }

        /// <summary>
        /// Nettoie les fichiers du programme dans AppData
        /// </summary>
        private static void CleanupAppDataFiles(string programName, Action<string>? log = null)
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                var pathsToClean = new[] { appDataPath, localAppDataPath };

                foreach (var basePath in pathsToClean)
                {
                    try
                    {
                        if (!Directory.Exists(basePath)) continue;

                        var directories = Directory.GetDirectories(basePath)
                            .Where(d => Path.GetFileName(d).Contains(programName, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var dir in directories)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                                log?.Invoke($"Dossier AppData supprimé : {dir}");
                            }
                            catch { /* Ignorer les erreurs */ }
                        }
                    }
                    catch { /* Ignorer les erreurs */ }
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur nettoyage AppData : {ex.Message}");
            }
        }

        /// <summary>
        /// Exporte la liste des programmes installés en CSV
        /// </summary>
        public static bool ExportProgramsList(string filePath, Action<string>? log = null)
        {
            try
            {
                var programs = GetInstalledPrograms();

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Nom,Version,Editeur,Taille (MB),Date d'installation,Localisation");

                    foreach (var program in programs)
                    {
                        var size = program.EstimatedSize > 0 ? (program.EstimatedSize / 1024.0).ToString("F2") : "N/A";
                        var installDate = program.InstallDate?.ToString("yyyy-MM-dd") ?? "N/A";
                        var location = program.InstallLocation.Replace(",", ";");

                        writer.WriteLine($"\"{program.DisplayName}\",\"{program.DisplayVersion}\",\"{program.Publisher}\",{size},{installDate},\"{location}\"");
                    }
                }

                log?.Invoke($"Liste des programmes exportée vers : {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur lors de l'export : {ex.Message}");
                return false;
            }
        }
    }
}
