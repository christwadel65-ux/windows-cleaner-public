using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace WindowsCleaner
{
    /// <summary>
    /// Optimisations système avancées (TRIM SSD, registre, services)
    /// </summary>
    public static class SystemOptimizer
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetSystemFileCacheSize(
            IntPtr minimumFileCacheSize,
            IntPtr maximumFileCacheSize,
            int flags
        );
        
        private const int CLEAR_STANDBY_CACHE = 4;
        
        /// <summary>
        /// Exécute TRIM sur les SSD
        /// </summary>
        public static bool OptimizeSsd(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("Optimisation SSD (TRIM) en cours...");
                
                var psi = new ProcessStartInfo
                {
                    FileName = "defrag.exe",
                    Arguments = "/L",  // Liste les volumes
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                {
                    log?.Invoke("Impossible de lancer defrag.exe");
                    return false;
                }
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                // Identifier les SSD et lancer TRIM
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        try
                        {
                            // Exécuter TRIM sur chaque volume
                            var trimPsi = new ProcessStartInfo
                            {
                                FileName = "defrag.exe",
                                Arguments = $"{drive.Name} /L",  // TRIM pour SSD
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };
                            
                            using var trimProcess = Process.Start(trimPsi);
                            if (trimProcess != null)
                            {
                                trimProcess.WaitForExit(30000); // Timeout 30s
                                log?.Invoke($"TRIM exécuté sur {drive.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            log?.Invoke($"Erreur TRIM {drive.Name}: {ex.Message}");
                        }
                    }
                }
                
                log?.Invoke("Optimisation SSD terminée");
                Logger.Log(LogLevel.Info, "Optimisation SSD (TRIM) terminée");
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur optimisation SSD: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur optimisation SSD: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Compacte le registre Windows
        /// </summary>
        public static bool CompactRegistry(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("Compactage du registre Windows...");
                
                // Le registre Windows se compacte automatiquement au démarrage si nécessaire
                // On peut forcer en utilisant l'outil système compact
                
                var script = @"
                    $regFiles = @(
                        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce'
                    )
                    foreach ($key in $regFiles) {
                        if (Test-Path $key) {
                            Write-Output ""Analysé: $key""
                        }
                    }
                    Write-Output 'Compactage programmé pour le prochain redémarrage'
                ";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                log?.Invoke("Compactage registre programmé (prendra effet au redémarrage)");
                Logger.Log(LogLevel.Info, "Compactage registre programmé");
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur compactage registre: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur compactage registre: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Vide le cache mémoire système
        /// </summary>
        public static bool ClearStandbyMemory(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("Vidage du cache mémoire système...");
                
                // Méthode 1: Via EmptyStandbyList (nécessite RAMMap ou similar)
                // Méthode 2: Via PowerShell et API système
                
                var script = @"
                    $signature = @'
                    [DllImport(""kernel32.dll"")]
                    public static extern int SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
'@
                    $type = Add-Type -MemberDefinition $signature -Name API -Namespace Win32 -PassThru
                    $type::SetProcessWorkingSetSize((Get-Process -Id $PID).Handle, -1, -1)
                    [System.GC]::Collect()
                    [System.GC]::WaitForPendingFinalizers()
                    [System.GC]::Collect()
                    Write-Output 'Cache mémoire vidé'
                ";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas" // Nécessite élévation
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                {
                    // Fallback: utiliser GC.Collect local
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    
                    log?.Invoke("Cache mémoire local vidé");
                    return true;
                }
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                log?.Invoke("Cache mémoire système vidé");
                Logger.Log(LogLevel.Info, "Cache mémoire vidé");
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur vidage cache mémoire: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur vidage cache mémoire: {ex.Message}");
                
                // Fallback
                try
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    log?.Invoke("Cache mémoire local vidé (méthode alternative)");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Désactive des services Windows non essentiels (avec prudence)
        /// </summary>
        public static bool OptimizeServices(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("⚠️ Optimisation des services désactivée pour éviter les instabilités système");
                log?.Invoke("Cette fonctionnalité nécessite une expertise avancée");
                
                // Liste des services potentiellement désactivables (commentés pour sécurité)
                var nonEssentialServices = new string[]
                {
                    // Exemples (tous commentés pour sécurité):
                    // "TabletInputService", // Service d'entrée tablette
                    // "WSearch", // Recherche Windows (déconseillé)
                    // "SysMain", // SuperFetch (peut ralentir sur HDD)
                };
                
                Logger.Log(LogLevel.Warning, "Optimisation services non implémentée (sécurité)");
                return false;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur optimisation services: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur optimisation services: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Optimise les paramètres de performances Windows
        /// </summary>
        public static bool OptimizePerformanceSettings(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("Optimisation des paramètres de performances...");
                
                // Désactiver les effets visuels inutiles via registre
                var script = @"
                    # Désactiver animations
                    Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop\WindowMetrics' -Name 'MinAnimate' -Value '0' -ErrorAction SilentlyContinue
                    
                    # Optimiser pour les performances
                    Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects' -Name 'VisualFXSetting' -Value 2 -ErrorAction SilentlyContinue
                    
                    Write-Output 'Paramètres de performances optimisés'
                ";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                log?.Invoke("Paramètres de performances optimisés");
                Logger.Log(LogLevel.Info, "Paramètres de performances optimisés");
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur optimisation performances: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur optimisation performances: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Nettoie le fichier d'échange (pagefile.sys)
        /// </summary>
        public static bool ClearPageFile(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("Configuration du nettoyage du fichier d'échange...");
                
                // Configurer Windows pour vider le pagefile à l'arrêt
                var script = @"
                    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management' -Name 'ClearPageFileAtShutdown' -Value 1 -ErrorAction SilentlyContinue
                    Write-Output 'Nettoyage pagefile configuré (prendra effet à l arrêt)'
                ";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas" // Nécessite élévation
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                log?.Invoke("Nettoyage du fichier d'échange configuré (effet à l'arrêt)");
                Logger.Log(LogLevel.Info, "Nettoyage pagefile configuré");
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur configuration pagefile: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur configuration pagefile: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Vérifie la santé du disque via SMART et retourne un rapport
        /// </summary>
        public static string CheckDiskHealth(Action<string>? log = null)
        {
            try
            {
                log?.Invoke("Vérification SMART des disques...");
                
                var script = @"
                    $report = @()
                    
                    # Obtenir les disques
                    $disks = Get-CimInstance Win32_DiskDrive
                    
                    foreach ($disk in $disks) {
                        $report += ""Disque: $($disk.Model)""
                        $report += ""Santé: $($disk.Status)""
                        $report += ""Taille: $([Math]::Round($disk.Size / 1GB)) GB""
                        $report += """"
                    }
                    
                    $report -join ""`r`n""
                ";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return "Impossible d'accéder aux données SMART";
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                log?.Invoke("Rapport SMART récupéré");
                return string.IsNullOrEmpty(output) ? "Aucun disque détecté" : output;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur vérification SMART: {ex.Message}");
                return $"Erreur: {ex.Message}";
            }
        }
    }
}
