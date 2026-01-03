using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Microsoft.Win32;
using System.ServiceProcess;
using System.Security.Principal;

namespace WindowsCleaner
{
    /// <summary>
    /// Gère la configuration système similaire à msconfig de Windows.
    /// Permet de gérer les programmes de démarrage, les services, et les tâches planifiées.
    /// </summary>
    public static class SystemConfigManager
    {
        #region Programmes de démarrage

        /// <summary>
        /// Représente un programme de démarrage
        /// </summary>
        public class StartupProgram
        {
            public string Name { get; set; } = string.Empty;
            public string Command { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public bool IsEnabled { get; set; }
            public StartupLocation LocationType { get; set; }
        }

        public enum StartupLocation
        {
            CurrentUserRun,
            LocalMachineRun,
            StartupFolder,
            TaskScheduler
        }

        /// <summary>
        /// Récupère tous les programmes de démarrage
        /// </summary>
        public static List<StartupProgram> GetStartupPrograms()
        {
            var programs = new List<StartupProgram>();

            try
            {
                // Current User Run (actifs)
                AddStartupFromRegistry(programs, Registry.CurrentUser, 
                    @"Software\Microsoft\Windows\CurrentVersion\Run", 
                    StartupLocation.CurrentUserRun, true);

                // Current User Run (désactivés)
                AddStartupFromRegistry(programs, Registry.CurrentUser, 
                    @"Software\Microsoft\Windows\CurrentVersion\Run-Disabled", 
                    StartupLocation.CurrentUserRun, false);

                // Current User RunOnce (actifs)
                AddStartupFromRegistry(programs, Registry.CurrentUser, 
                    @"Software\Microsoft\Windows\CurrentVersion\RunOnce", 
                    StartupLocation.CurrentUserRun, true);

                // Local Machine Run (actifs)
                AddStartupFromRegistry(programs, Registry.LocalMachine, 
                    @"Software\Microsoft\Windows\CurrentVersion\Run", 
                    StartupLocation.LocalMachineRun, true);

                // Local Machine Run (désactivés)
                AddStartupFromRegistry(programs, Registry.LocalMachine, 
                    @"Software\Microsoft\Windows\CurrentVersion\Run-Disabled", 
                    StartupLocation.LocalMachineRun, false);

                // Local Machine RunOnce (actifs)
                AddStartupFromRegistry(programs, Registry.LocalMachine, 
                    @"Software\Microsoft\Windows\CurrentVersion\RunOnce", 
                    StartupLocation.LocalMachineRun, true);

                // Wow6432Node (32-bit apps on 64-bit Windows) - actifs
                AddStartupFromRegistry(programs, Registry.LocalMachine, 
                    @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", 
                    StartupLocation.LocalMachineRun, true);

                // Current User Startup Folder
                var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                AddStartupFromFolder(programs, startupPath, "Startup Folder (User)");

                // Common Startup Folder (All Users)
                var commonStartupPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                AddStartupFromFolder(programs, commonStartupPath, "Startup Folder (All Users)");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la récupération des programmes de démarrage: {ex.Message}");
            }

            return programs;
        }

        private static void AddStartupFromFolder(List<StartupProgram> programs, string folderPath, string locationName)
        {
            try
            {
                if (System.IO.Directory.Exists(folderPath))
                {
                    foreach (var file in System.IO.Directory.GetFiles(folderPath, "*.*"))
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        var ext = System.IO.Path.GetExtension(file).ToLowerInvariant();
                        
                        // Vérifier si c'est un fichier désactivé
                        bool isDisabled = ext == ".disabled";
                        string actualExt = ext;
                        
                        if (isDisabled)
                        {
                            // Récupérer l'extension réelle (ex: .lnk.disabled -> .lnk)
                            var fileNameWithoutDisabled = System.IO.Path.GetFileNameWithoutExtension(file);
                            actualExt = System.IO.Path.GetExtension(fileNameWithoutDisabled).ToLowerInvariant();
                            fileName = System.IO.Path.GetFileNameWithoutExtension(fileNameWithoutDisabled);
                        }
                        
                        // Inclure .lnk, .exe, .bat, .cmd, .vbs, .ps1
                        if (actualExt == ".lnk" || actualExt == ".exe" || actualExt == ".bat" || 
                            actualExt == ".cmd" || actualExt == ".vbs" || actualExt == ".ps1")
                        {
                            programs.Add(new StartupProgram
                            {
                                Name = fileName,
                                Command = file,
                                Location = isDisabled ? locationName + " (Disabled)" : locationName,
                                IsEnabled = !isDisabled,
                                LocationType = StartupLocation.StartupFolder
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Impossible de lire le dossier {folderPath}: {ex.Message}");
            }
        }

        private static void AddStartupFromRegistry(List<StartupProgram> programs, RegistryKey rootKey, string subKey, StartupLocation location, bool isEnabled)
        {
            try
            {
                using var key = rootKey.OpenSubKey(subKey);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var value = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            var locationName = location == StartupLocation.CurrentUserRun ? "HKCU\\Run" : "HKLM\\Run";
                            if (!isEnabled)
                                locationName += " (Disabled)";
                                
                            programs.Add(new StartupProgram
                            {
                                Name = valueName,
                                Command = value,
                                Location = locationName,
                                IsEnabled = isEnabled,
                                LocationType = location
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Impossible de lire {subKey}: {ex.Message}");
            }
        }

        /// <summary>
        /// Désactive un programme de démarrage (le déplace vers une clé de sauvegarde)
        /// </summary>
        public static bool DisableStartupProgram(StartupProgram program)
        {
            try
            {
                if (program.LocationType == StartupLocation.CurrentUserRun)
                {
                    return DisableRegistryStartup(Registry.CurrentUser, program.Name, 
                        @"Software\Microsoft\Windows\CurrentVersion\Run",
                        @"Software\Microsoft\Windows\CurrentVersion\Run-Disabled");
                }
                else if (program.LocationType == StartupLocation.LocalMachineRun)
                {
                    if (!IsAdministrator())
                    {
                        Logger.Log(LogLevel.Warning, "Privilèges administrateur requis pour modifier HKLM");
                        return false;
                    }
                    
                    return DisableRegistryStartup(Registry.LocalMachine, program.Name,
                        @"Software\Microsoft\Windows\CurrentVersion\Run",
                        @"Software\Microsoft\Windows\CurrentVersion\Run-Disabled");
                }
                else if (program.LocationType == StartupLocation.StartupFolder)
                {
                    // Pour les fichiers, renommer avec .disabled
                    if (System.IO.File.Exists(program.Command))
                    {
                        var disabledPath = program.Command + ".disabled";
                        System.IO.File.Move(program.Command, disabledPath);
                        Logger.Log(LogLevel.Info, $"Fichier de démarrage désactivé: {program.Command}");
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Log(LogLevel.Error, $"Accès refusé lors de la désactivation de {program.Name}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la désactivation de {program.Name}: {ex.Message}");
                return false;
            }

            return false;
        }

        /// <summary>
        /// Désactive une entrée du registre en la déplaçant vers une clé de sauvegarde
        /// </summary>
        private static bool DisableRegistryStartup(RegistryKey rootKey, string valueName, string activeKey, string disabledKey)
        {
            try
            {
                using (var srcKey = rootKey.OpenSubKey(activeKey, true))
                using (var destKey = rootKey.CreateSubKey(disabledKey))
                {
                    if (srcKey == null || destKey == null)
                        return false;

                    var value = srcKey.GetValue(valueName);
                    if (value != null)
                    {
                        // Copier vers la clé désactivée
                        destKey.SetValue(valueName, value);
                        
                        // Supprimer de la clé active
                        srcKey.DeleteValue(valueName, false);
                        
                        Logger.Log(LogLevel.Info, $"Programme de démarrage désactivé: {valueName}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la désactivation de {valueName}: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Réactive un programme de démarrage désactivé
        /// </summary>
        public static bool EnableStartupProgram(StartupProgram program)
        {
            try
            {
                if (program.LocationType == StartupLocation.CurrentUserRun)
                {
                    return EnableRegistryStartup(Registry.CurrentUser, program.Name,
                        @"Software\Microsoft\Windows\CurrentVersion\Run",
                        @"Software\Microsoft\Windows\CurrentVersion\Run-Disabled");
                }
                else if (program.LocationType == StartupLocation.LocalMachineRun)
                {
                    if (!IsAdministrator())
                    {
                        Logger.Log(LogLevel.Warning, "Privilèges administrateur requis pour modifier HKLM");
                        return false;
                    }

                    return EnableRegistryStartup(Registry.LocalMachine, program.Name,
                        @"Software\Microsoft\Windows\CurrentVersion\Run",
                        @"Software\Microsoft\Windows\CurrentVersion\Run-Disabled");
                }
                else if (program.LocationType == StartupLocation.StartupFolder)
                {
                    // Pour les fichiers, retirer le .disabled
                    if (program.Command.EndsWith(".disabled") && System.IO.File.Exists(program.Command))
                    {
                        var enabledPath = program.Command.Substring(0, program.Command.Length - 9); // Retirer ".disabled"
                        System.IO.File.Move(program.Command, enabledPath);
                        Logger.Log(LogLevel.Info, $"Fichier de démarrage réactivé: {enabledPath}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la réactivation de {program.Name}: {ex.Message}");
                return false;
            }

            return false;
        }

        /// <summary>
        /// Réactive une entrée du registre en la déplaçant de la clé de sauvegarde vers la clé active
        /// </summary>
        private static bool EnableRegistryStartup(RegistryKey rootKey, string valueName, string activeKey, string disabledKey)
        {
            try
            {
                using (var srcKey = rootKey.OpenSubKey(disabledKey, true))
                using (var destKey = rootKey.CreateSubKey(activeKey))
                {
                    if (srcKey == null || destKey == null)
                        return false;

                    var value = srcKey.GetValue(valueName);
                    if (value != null)
                    {
                        // Copier vers la clé active
                        destKey.SetValue(valueName, value);
                        
                        // Supprimer de la clé désactivée
                        srcKey.DeleteValue(valueName, false);
                        
                        Logger.Log(LogLevel.Info, $"Programme de démarrage réactivé: {valueName}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la réactivation de {valueName}: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Ajoute un programme au démarrage
        /// </summary>
        public static bool AddStartupProgram(string name, string command, bool currentUser = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(command))
                    return false;

                var registryKey = currentUser 
                    ? Registry.CurrentUser 
                    : Registry.LocalMachine;

                using var key = registryKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (key == null)
                    return false;

                key.SetValue(name, command);
                Logger.Log(LogLevel.Info, $"Programme ajouté au démarrage: {name} -> {command}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'ajout de {name} au démarrage: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Services Windows

        /// <summary>
        /// Représente un service Windows
        /// </summary>
        public class WindowsServiceInfo
        {
            public string Name { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string StartType { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        /// <summary>
        /// Récupère tous les services Windows
        /// </summary>
        public static List<WindowsServiceInfo> GetWindowsServices()
        {
            var services = new List<WindowsServiceInfo>();

            try
            {
                // Récupérer toutes les infos WMI en une seule requête
                var wmiServices = new Dictionary<string, (string startMode, string description)>();
                
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Name, StartMode, Description FROM Win32_Service");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString() ?? "";
                        var startMode = obj["StartMode"]?.ToString() ?? "Unknown";
                        var description = obj["Description"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(name))
                        {
                            wmiServices[name] = (startMode, description);
                        }
                    }
                }
                catch { /* Ignore WMI errors */ }

                // Récupérer les services
                var allServices = ServiceController.GetServices();
                foreach (var service in allServices)
                {
                    try
                    {
                        var startType = "Unknown";
                        var description = "";
                        
                        if (wmiServices.TryGetValue(service.ServiceName, out var wmiInfo))
                        {
                            startType = wmiInfo.startMode;
                            description = wmiInfo.description;
                        }

                        services.Add(new WindowsServiceInfo
                        {
                            Name = service.ServiceName,
                            DisplayName = service.DisplayName,
                            Status = service.Status.ToString(),
                            StartType = startType,
                            Description = description
                        });
                    }
                    catch
                    {
                        // Ignorer les services inaccessibles
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la récupération des services: {ex.Message}");
            }

            return services.OrderBy(s => s.DisplayName).ToList();
        }

        /// <summary>
        /// Démarre un service Windows
        /// </summary>
        public static bool StartService(string serviceName)
        {
            try
            {
                using var service = new ServiceController(serviceName);
                if (service.Status != ServiceControllerStatus.Running)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    Logger.Log(LogLevel.Info, $"Service {serviceName} démarré avec succès");
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors du démarrage du service {serviceName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Arrête un service Windows
        /// </summary>
        public static bool StopService(string serviceName)
        {
            try
            {
                using var service = new ServiceController(serviceName);
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    Logger.Log(LogLevel.Info, $"Service {serviceName} arrêté avec succès");
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'arrêt du service {serviceName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Change le type de démarrage d'un service
        /// </summary>
        public static bool ChangeServiceStartType(string serviceName, string startType)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"config \"{serviceName}\" start= {startType}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Logger.Log(LogLevel.Info, $"Type de démarrage du service {serviceName} modifié en {startType}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors du changement du type de démarrage du service {serviceName}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Tâches planifiées

        /// <summary>
        /// Représente une tâche planifiée
        /// </summary>
        public class ScheduledTaskInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string NextRunTime { get; set; } = string.Empty;
            public string LastRunTime { get; set; } = string.Empty;
            public bool IsEnabled { get; set; }
        }

        /// <summary>
        /// Récupère toutes les tâches planifiées
        /// </summary>
        public static List<ScheduledTaskInfo> GetScheduledTasks()
        {
            var tasks = new List<ScheduledTaskInfo>();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = "/Query /FO CSV /V /NH",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Parse CSV output
                var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                        
                    var parts = ParseCsvLine(line);
                    
                    // Schtasks retourne (ordre strict): 
                    // [0]=HostName, [1]=TaskName, [2]=NextRunTime, [3]=Status, 
                    // [4]=LogonType, [5]=LastRunTime, [6]=LastResult, ...
                    
                    if (parts.Count >= 6)
                    {
                        try
                        {
                            var taskPath = parts[1].Replace("\"", "").Trim();
                            var taskName = taskPath.Contains("\\") 
                                ? taskPath.Substring(taskPath.LastIndexOf("\\") + 1)
                                : taskPath;
                            
                            var nextRun = parts[2].Replace("\"", "").Trim();
                            var statusRaw = parts[3].Replace("\"", "").Trim();
                            var lastRun = parts[5].Replace("\"", "").Trim();
                            
                            // Normaliser le statut brut de Windows (français ou anglais) en statut standard
                            var statusNormalized = NormalizeWindowsTaskStatus(statusRaw);
                            
                            // Formater les dates si elles sont valides
                            if (!string.IsNullOrEmpty(nextRun) && nextRun != "N/A" && !nextRun.Contains("Never"))
                            {
                                if (DateTime.TryParse(nextRun, out DateTime nextRunDate))
                                {
                                    nextRun = nextRunDate.ToString("dd/MM/yyyy HH:mm");
                                }
                            }
                            else if (string.IsNullOrEmpty(nextRun) || nextRun == "N/A")
                            {
                                nextRun = "N/A";
                            }
                            
                            if (!string.IsNullOrEmpty(lastRun) && lastRun != "N/A" && !lastRun.Contains("Never"))
                            {
                                if (DateTime.TryParse(lastRun, out DateTime lastRunDate))
                                {
                                    lastRun = lastRunDate.ToString("dd/MM/yyyy HH:mm");
                                }
                            }
                            else if (string.IsNullOrEmpty(lastRun) || lastRun == "N/A")
                            {
                                lastRun = "N/A";
                            }
                            
                            tasks.Add(new ScheduledTaskInfo
                            {
                                Name = taskName,
                                NextRunTime = nextRun,
                                Status = statusNormalized,
                                LastRunTime = lastRun,
                                Path = taskPath,
                                IsEnabled = statusNormalized != "Disabled"
                            });
                        }
                        catch
                        {
                            // Ignorer les lignes mal formatées
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la récupération des tâches planifiées: {ex.Message}");
            }

            return tasks.OrderBy(t => t.Name).ToList();
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    // Vérifier si c'est un guillemet échappé (deux guillemets d'affilée)
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++; // Sauter le prochain guillemet
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrEmpty(current))
                result.Add(current);

            return result;
        }

        /// <summary>
        /// Normalise les statuts bruts de Windows en remplaçant les accents
        /// </summary>
        private static string NormalizeWindowsTaskStatus(string statusRaw)
        {
            if (string.IsNullOrEmpty(statusRaw))
                return "Unknown";

            // Remplacer les caractères accentués directement
            var normalized = statusRaw
                .Replace("Ã©", "e")
                .Replace("é", "e")
                .Replace("È", "e")
                .Replace("è", "e")
                .Replace("Ê", "e")
                .Replace("ê", "e")
                .Replace("À", "a")
                .Replace("à", "a")
                .Replace("Â", "a")
                .Replace("â", "a")
                .Replace("Î", "i")
                .Replace("î", "i")
                .Replace("Ô", "o")
                .Replace("ô", "o")
                .Replace("Û", "u")
                .Replace("û", "u")
                .Replace("Ù", "u")
                .Replace("ù", "u")
                .Replace("Ç", "c")
                .Replace("ç", "c")
                .ToLowerInvariant()
                .Trim();
            
            // Matcher les statuts
            return normalized switch
            {
                "pret" => "Ready",
                "desactivee" => "Disabled",
                "en cours" => "Running",
                "en cours d'execution" => "Running",
                "en attente" => "Pending",
                "en file d'attente" => "Queued",
                "arretee" => "Stopped",
                "ready" => "Ready",
                "disabled" => "Disabled",
                "running" => "Running",
                "pending" => "Pending",
                "queued" => "Queued",
                "stopped" => "Stopped",
                _ => "Ready" // Par défaut Ready pour les statuts non reconnus
            };
        }

        /// <summary>
        /// Désactive une tâche planifiée
        /// </summary>
        public static bool DisableScheduledTask(string taskName)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Change /TN \"{taskName}\" /DISABLE",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Logger.Log(LogLevel.Info, $"Tâche planifiée {taskName} désactivée");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la désactivation de la tâche {taskName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Active une tâche planifiée
        /// </summary>
        public static bool EnableScheduledTask(string taskName)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Change /TN \"{taskName}\" /ENABLE",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Logger.Log(LogLevel.Info, $"Tâche planifiée {taskName} activée");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'activation de la tâche {taskName}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Vérifie si l'application s'exécute avec des privilèges administrateur
        /// </summary>
        public static bool IsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ouvre l'outil msconfig de Windows
        /// </summary>
        public static void OpenMsconfig()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "msconfig.exe",
                    UseShellExecute = true,
                    Verb = "runas"
                });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'ouverture de msconfig: {ex.Message}");
            }
        }

        /// <summary>
        /// Ouvre le gestionnaire de tâches Windows
        /// </summary>
        public static void OpenTaskManager()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'ouverture du gestionnaire de tâches: {ex.Message}");
            }
        }

        /// <summary>
        /// Ouvre le gestionnaire de services Windows
        /// </summary>
        public static void OpenServicesManager()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "services.msc",
                    UseShellExecute = true,
                    Verb = "runas"
                });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'ouverture du gestionnaire de services: {ex.Message}");
            }
        }

        #endregion
    }
}
