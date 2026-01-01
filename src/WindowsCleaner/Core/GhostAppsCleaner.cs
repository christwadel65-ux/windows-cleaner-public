using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32;

namespace WindowsCleaner
{
    /// <summary>
    /// Détecte et nettoie les applications fantômes (non complètement désinstallées)
    /// </summary>
    public static class GhostAppsCleaner
    {
        private const string UninstallRegistryPath32 = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UninstallRegistryPath64 = @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UninstallRegistryPathLocal = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

        // Liste des dossiers système à IGNORER absolument
        private static readonly HashSet<string> SystemFoldersToIgnore = new(StringComparer.OrdinalIgnoreCase)
        {
            // Dossiers Windows critiques
            "Reference Assemblies",
            "Common Files",
            "Windows Defender",
            "Windows Media Player",
            "Windows Photo Viewer",
            "WindowsApps",
            "WindowsPowerShell",
            "PowerShell",
            "Windows Kits",
            "Windows NT",
            "Windows Mail",
            "Windows Portable Devices",
            "Windows Security",
            "Microsoft.NET",
            "dotnet",
            "MSBuild",
            "IIS",
            "Internet Explorer",
            
            // Drivers et hardware
            "AMD",
            "AMD Chipset Drivers",
            "Intel",
            "NVIDIA",
            "Realtek",
            "DisplayLink",
            "Qualcomm",
            "Broadcom",
            "Ralink",
            "TP-LINK",
            "D-Link",
            "Logitech",
            "HP",
            "Dell",
            "Lenovo",
            "ASUS",
            "Acer",
            
            // Microsoft développement
            ".NET",
            ".NET Framework",
            "Microsoft Office",
            "Office 16",
            "Office 15",
            "Office 14",
            "Microsoft SQL Server",
            "Visual Studio",
            "Microsoft Visual Studio",
            "Microsoft SDKs",
            "Microsoft Help Viewer",
            "MSBuild",
            
            // Langages et outils développement
            "Git",
            "Python",
            "Ruby",
            "Nodejs",
            "Node.js",
            "Java",
            "OpenJDK",
            "JetBrains",
            "Perl",
            "Go",
            "Rust",
            "Kotlin",
            "Scala",
            "Gradle",
            "Maven",
            "CMake",
            
            // Outils Windows
            "Windows Resource Kits",
            "Windows Performance Toolkit",
            "Windows ADK",
            "Windows Subsystem for Linux",
            "WSL",
            
            // Applications communes
            "WinRAR",
            "7-Zip",
            "Putty",
            "VLC",
            "OBS",
            "Adobe",
            "Audacity",
            "GIMP",
            "Inkscape",
            "Blender",
            "Unity",
            "Unreal Engine",
            
            // Gaming
            "Steam",
            "Epic Games",
            "Launcher Epic Games",
            "Origin",
            "EA Games",
            "Uplay",
            "Ubisoft Game Launcher",
            "Battle.net",
            "GOG Galaxy",
            "Xbox",
            "Riot Games",
            "Electronic Arts",
            
            // Communication
            "Discord",
            "Slack",
            "Microsoft Teams",
            "Zoom",
            "Skype",
            "TeamViewer",
            
            // Antivirus
            "Avast",
            "AVG",
            "Norton",
            "McAfee",
            "Kaspersky",
            "Bitdefender",
            "ESET",
            "Malwarebytes",
            "Windows Defender Advanced Threat Protection"
        };

        /// <summary>
        /// Représente une application fantôme détectée
        /// </summary>
        public class GhostApp
        {
            public string Name { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string InstallLocation { get; set; } = string.Empty;
            public DateTime? InstallDate { get; set; }
            public string RegistryPath { get; set; } = string.Empty;
            public long SizeBytes { get; set; }
            public GhostAppType Type { get; set; }
        }

        /// <summary>
        /// Type d'application fantôme
        /// </summary>
        public enum GhostAppType
        {
            /// <summary>Dossier d'installation existe mais pas d'entrée registre</summary>
            OrphanedFolder,
            /// <summary>Entrée registre existe mais pas de dossier d'installation</summary>
            OrphanedRegistry,
            /// <summary>Entrée registre invalide (chemin n'existe pas)</summary>
            InvalidRegistry
        }

        /// <summary>
        /// Détecte toutes les applications fantômes
        /// </summary>
        public static List<GhostApp> DetectGhostApps(Action<string>? log = null, CancellationToken cancellationToken = default)
        {
            var ghostApps = new List<GhostApp>();

            try
            {
                log?.Invoke(LanguageManager.Get("log_detecting_ghost_apps"));

                // Déterminer les chemins standards d'installation
                var programFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                var programFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // Récupérer tous les chemins registre valides
                var registryPaths = GetValidRegistryInstallPaths(cancellationToken);
                var registryPathsLower = registryPaths.Select(p => p.ToLowerInvariant()).ToHashSet();

                // DÉSACTIVÉ: Détection dossiers orphelins trop risquée (détecte des dossiers système)
                // On ne garde que la détection des entrées registre invalides qui est beaucoup plus sûre
                // DetectOrphanedFolders(programFiles32, registryPathsLower, ghostApps, log, cancellationToken);
                // DetectOrphanedFolders(programFiles64, registryPathsLower, ghostApps, log, cancellationToken);

                // Chercher uniquement les entrées registre invalides (beaucoup plus sûr)
                DetectInvalidRegistryEntries(registryPaths, ghostApps, log, cancellationToken);

                // 3. Nettoyer les doublons
                ghostApps = ghostApps
                    .GroupBy(g => g.InstallLocation.ToLowerInvariant())
                    .Select(g => g.First())
                    .ToList();

                log?.Invoke(LanguageManager.Get("log_ghost_apps_found", ghostApps.Count.ToString()));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("error_detecting_ghost_apps", ex.Message));
            }

            return ghostApps;
        }

        /// <summary>
        /// Récupère tous les chemins d'installation valides depuis le registre
        /// </summary>
        private static HashSet<string> GetValidRegistryInstallPaths(CancellationToken cancellationToken)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Clé HKLM (local machine)
                using (var key = Registry.LocalMachine.OpenSubKey(UninstallRegistryPath32))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                var installLocation = subKey?.GetValue("InstallLocation") as string;
                                if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
                                {
                                    paths.Add(installLocation);
                                }
                            }
                        }
                    }
                }

                // Clé HKLM 64-bit
                using (var key = Registry.LocalMachine.OpenSubKey(UninstallRegistryPath64))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                var installLocation = subKey?.GetValue("InstallLocation") as string;
                                if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
                                {
                                    paths.Add(installLocation);
                                }
                            }
                        }
                    }
                }

                // Clé HKCU (utilisateur actuel)
                using (var key = Registry.CurrentUser.OpenSubKey(UninstallRegistryPathLocal))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                var installLocation = subKey?.GetValue("InstallLocation") as string;
                                if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
                                {
                                    paths.Add(installLocation);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de la lecture du registre: {ex.Message}");
            }

            return paths;
        }

        /// <summary>
        /// Détecte les dossiers d'applications sans entrée registre
        /// </summary>
        private static void DetectOrphanedFolders(string basePath, HashSet<string> registryPaths, List<GhostApp> ghostApps, Action<string>? log, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(basePath)) return;

            try
            {
                var directories = new DirectoryInfo(basePath).GetDirectories();

                foreach (var dir in directories)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Vérifier que le dossier n'est pas dans la liste blanche
                    if (SystemFoldersToIgnore.Any(ignored => dir.Name.Contains(ignored, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue; // Ignorer ce dossier
                    }

                    var dirPath = dir.FullName.ToLowerInvariant();

                    // Vérifier que le dossier n'est pas dans le registre
                    if (!registryPaths.Any(p => p.ToLowerInvariant().StartsWith(dirPath)))
                    {
                        // C'est un dossier orphelin potentiel
                        var size = GetDirectorySize(dir.FullName, cancellationToken);
                        
                        // Augmenter le seuil à 10 MB pour éviter les faux positifs
                        if (size > 10 * 1024 * 1024)
                        {
                            ghostApps.Add(new GhostApp
                            {
                                Name = dir.Name,
                                InstallLocation = dir.FullName,
                                Type = GhostAppType.OrphanedFolder,
                                SizeBytes = size,
                                InstallDate = dir.CreationTime
                            });

                            log?.Invoke(LanguageManager.Get("log_orphaned_folder_found", dir.FullName));
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorer les dossiers inaccessibles
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de la détection des dossiers orphelins: {ex.Message}");
            }
        }

        /// <summary>
        /// Détecte les entrées registre invalides
        /// </summary>
        private static void DetectInvalidRegistryEntries(HashSet<string> registryPaths, List<GhostApp> ghostApps, Action<string>? log, CancellationToken cancellationToken)
        {
            try
            {
                // Vérifier HKLM
                DetectInvalidRegistryEntriesFromKey(Registry.LocalMachine, UninstallRegistryPath32, registryPaths, ghostApps, log, cancellationToken);
                DetectInvalidRegistryEntriesFromKey(Registry.LocalMachine, UninstallRegistryPath64, registryPaths, ghostApps, log, cancellationToken);

                // Vérifier HKCU
                DetectInvalidRegistryEntriesFromKey(Registry.CurrentUser, UninstallRegistryPathLocal, registryPaths, ghostApps, log, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de la vérification du registre: {ex.Message}");
            }
        }

        /// <summary>
        /// Détecte les entrées registre invalides dans une clé spécifique
        /// </summary>
        private static void DetectInvalidRegistryEntriesFromKey(RegistryKey rootKey, string subKeyPath, HashSet<string> registryPaths, List<GhostApp> ghostApps, Action<string>? log, CancellationToken cancellationToken)
        {
            try
            {
                using (var key = rootKey.OpenSubKey(subKeyPath))
                {
                    if (key == null) return;

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            var installLocation = subKey?.GetValue("InstallLocation") as string;
                            var displayName = subKey?.GetValue("DisplayName") as string;

                            if (!string.IsNullOrWhiteSpace(installLocation) && !Directory.Exists(installLocation))
                            {
                                // L'entrée registre pointe vers un répertoire inexistant
                                ghostApps.Add(new GhostApp
                                {
                                    Name = displayName ?? subKeyName,
                                    InstallLocation = installLocation,
                                    Version = subKey?.GetValue("DisplayVersion") as string ?? string.Empty,
                                    Type = GhostAppType.InvalidRegistry,
                                    RegistryPath = $"{rootKey.Name}\\{subKeyPath}\\{subKeyName}"
                                });

                                log?.Invoke(LanguageManager.Get("log_invalid_registry_found", displayName ?? subKeyName));
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorer les clés inaccessibles
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de la vérification des entrées registre: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcule la taille d'un répertoire
        /// </summary>
        private static long GetDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;
            try
            {
                var di = new DirectoryInfo(path);
                var files = di.GetFiles("*", System.IO.SearchOption.AllDirectories);
                size = files.AsParallel().WithCancellation(cancellationToken).Sum(f => f.Length);
            }
            catch
            {
                // Ignorer les erreurs d'accès
            }
            return size;
        }

        /// <summary>
        /// Nettoie les applications fantômes détectées
        /// </summary>
        public static (int filesDeleted, long bytesFreed, int registryEntriesRemoved) CleanGhostApps(List<GhostApp> ghostApps, bool dryRun, Action<string>? log = null, CancellationToken cancellationToken = default)
        {
            int filesDeleted = 0;
            long bytesFreed = 0;
            int registryEntriesRemoved = 0;

            foreach (var ghostApp in ghostApps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (ghostApp.Type == GhostAppType.OrphanedFolder && Directory.Exists(ghostApp.InstallLocation))
                    {
                        log?.Invoke(LanguageManager.Get("log_removing_ghost_app", ghostApp.Name));

                        if (!dryRun)
                        {
                            try
                            {
                                // Supprimer le dossier orphelin
                                var (deletedCount, freedBytes) = DeleteDirectory(ghostApp.InstallLocation);
                                filesDeleted += deletedCount;
                                bytesFreed += freedBytes;
                            }
                            catch (Exception ex)
                            {
                                log?.Invoke(LanguageManager.Get("error_removing_ghost_app", ghostApp.Name, ex.Message));
                            }
                        }
                        else
                        {
                            bytesFreed += ghostApp.SizeBytes;
                        }
                    }
                    else if (ghostApp.Type == GhostAppType.InvalidRegistry && !string.IsNullOrEmpty(ghostApp.RegistryPath))
                    {
                        log?.Invoke(LanguageManager.Get("log_removing_invalid_registry", ghostApp.Name));

                        if (!dryRun)
                        {
                            try
                            {
                                RemoveRegistryEntry(ghostApp.RegistryPath);
                                registryEntriesRemoved++;
                            }
                            catch (Exception ex)
                            {
                                log?.Invoke(LanguageManager.Get("error_removing_registry_entry", ghostApp.Name, ex.Message));
                            }
                        }
                        else
                        {
                            registryEntriesRemoved++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Erreur lors du nettoyage de {ghostApp.Name}: {ex.Message}");
                }
            }

            return (filesDeleted, bytesFreed, registryEntriesRemoved);
        }

        /// <summary>
        /// Supprime un répertoire de manière récursive
        /// </summary>
        private static (int filesDeleted, long bytesFreed) DeleteDirectory(string path)
        {
            int filesDeleted = 0;
            long bytesFreed = 0;

            try
            {
                var di = new DirectoryInfo(path);

                // Supprimer tous les fichiers
                foreach (var file in di.GetFiles("*", System.IO.SearchOption.AllDirectories))
                {
                    try
                    {
                        bytesFreed += file.Length;
                        file.Delete();
                        filesDeleted++;
                    }
                    catch
                    {
                        // Continuer même en cas d'erreur
                    }
                }

                // Supprimer le répertoire
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de la suppression du répertoire {path}: {ex.Message}");
            }

            return (filesDeleted, bytesFreed);
        }

        /// <summary>
        /// Supprime une entrée du registre
        /// </summary>
        private static void RemoveRegistryEntry(string registryPath)
        {
            // Format: "HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Uninstall\{GUID}"
            var parts = registryPath.Split('\\');
            
            if (parts.Length < 2) return;

            var hive = parts[0];
            var subPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));
            var keyName = parts[parts.Length - 1];

            RegistryKey rootKey = hive.Contains("LOCAL_MACHINE") ? Registry.LocalMachine :
                                  hive.Contains("CURRENT_USER") ? Registry.CurrentUser :
                                  null;

            if (rootKey == null) return;

            try
            {
                using (var key = rootKey.OpenSubKey(subPath, true))
                {
                    key?.DeleteSubKey(keyName, false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors de la suppression de l'entrée registre {registryPath}: {ex.Message}");
            }
        }
    }
}
