using System;
using System.Linq;
using System.Windows.Forms;

namespace WindowsCleaner
{
    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // Mode CLI si arguments fournis
            if (args.Length > 0)
            {
                return RunCliMode(args);
            }
            
            // Mode GUI par défaut
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return 0;
        }
        
        private static int RunCliMode(string[] args)
        {
            try
            {
                bool silent = args.Contains("--silent", StringComparer.OrdinalIgnoreCase);
                bool dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
                string? profileName = null;
                
                // Parser les arguments
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("--profile", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    {
                        profileName = args[i + 1];
                        i++;
                    }
                    else if (args[i].Equals("--help", StringComparison.OrdinalIgnoreCase) || args[i].Equals("-h", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowHelp();
                        return 0;
                    }
                    else if (args[i].Equals("--list-profiles", StringComparison.OrdinalIgnoreCase))
                    {
                        ListProfiles();
                        return 0;
                    }
                    else if (args[i].Equals("--stats", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowStatistics();
                        return 0;
                    }
                }
                
                // Initialiser le logger
                Logger.Init();
                
                if (!silent)
                {
                    Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║          Windows Cleaner v1.0.8 - Mode CLI               ║");
                    Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                    Console.WriteLine();
                }
                
                // Charger le profil
                CleaningProfile profile;
                
                if (!string.IsNullOrEmpty(profileName))
                {
                    var loadedProfile = LoadProfile(profileName);
                    if (loadedProfile == null)
                    {
                        Console.WriteLine($"❌ Profil '{profileName}' introuvable.");
                        Console.WriteLine("Utilisez --list-profiles pour voir les profils disponibles.");
                        return 1;
                    }
                    profile = loadedProfile;
                }
                else
                {
                    // Profil par défaut
                    profile = CleaningProfile.CreateQuickProfile();
                }
                
                if (!silent)
                {
                    Console.WriteLine($"Profil: {profile.Name}");
                    Console.WriteLine($"Description: {profile.Description}");
                    Console.WriteLine($"Mode: {(dryRun ? "Simulation (dry-run)" : "Nettoyage réel")}");
                    Console.WriteLine();
                }
                
                // Exécuter le nettoyage
                var options = profile.ToCleanerOptions(dryRun);
                var startTime = DateTime.Now;
                
                var result = Cleaner.RunCleanup(options, msg =>
                {
                    if (!silent)
                    {
                        Console.WriteLine(msg);
                    }
                });
                
                var duration = DateTime.Now - startTime;
                
                // Enregistrer les statistiques (seulement si pas dry-run)
                if (!dryRun)
                {
                    StatisticsManager.RecordCleaningSession(new CleaningStatistics
                    {
                        ProfileUsed = profile.Name,
                        FilesDeleted = result.FilesDeleted,
                        BytesFreed = result.BytesFreed,
                        Duration = duration,
                        WasDryRun = dryRun,
                        
                        // App cache stats
                        VsCodeCacheFilesDeleted = result.VsCodeCacheFilesDeleted,
                        NugetCacheFilesDeleted = result.NugetCacheFilesDeleted,
                        MavenCacheFilesDeleted = result.MavenCacheFilesDeleted,
                        NpmCacheFilesDeleted = result.NpmCacheFilesDeleted,
                        GameCachesFilesDeleted = result.GameCachesFilesDeleted,
                        AppCachesBytesFreed = result.AppCachesBytesFreed,
                        
                        // SSD stats
                        SsdOptimized = result.SsdOptimized,
                        DiskHealthChecked = result.DiskHealthChecked,
                        DiskHealthReport = result.DiskHealthReport
                    });
                }
                
                if (!silent)
                {
                    Console.WriteLine();
                    Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║                    RÉSULTATS                               ║");
                    Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                    Console.WriteLine($"Fichiers supprimés: {result.FilesDeleted:N0}");
                    Console.WriteLine($"Espace libéré: {FormatBytes(result.BytesFreed)}");
                    Console.WriteLine($"Durée: {duration.TotalSeconds:F1} secondes");
                    Console.WriteLine();
                    Console.WriteLine("✅ Nettoyage terminé avec succès.");
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur: {ex.Message}");
                Logger.Log(LogLevel.Error, $"Erreur CLI: {ex.Message}");
                return 1;
            }
        }
        
        private static CleaningProfile? LoadProfile(string profileName)
        {
            // Vérifier les profils prédéfinis
            var predefinedProfiles = new[]
            {
                CleaningProfile.CreateQuickProfile(),
                CleaningProfile.CreateCompleteProfile(),
                CleaningProfile.CreateDeveloperProfile(),
                CleaningProfile.CreatePrivacyProfile()
            };
            
            var profile = predefinedProfiles.FirstOrDefault(p => 
                p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
            
            if (profile != null)
                return profile;
            
            // Charger depuis le disque
            return ProfileManager.LoadProfile(profileName);
        }
        
        private static void ShowHelp()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          Windows Cleaner v1.0.8 - Aide CLI               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("UTILISATION:");
            Console.WriteLine("  windows-cleaner.exe [options]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  --profile <nom>      Utiliser un profil de nettoyage spécifique");
            Console.WriteLine("  --dry-run            Mode simulation (aucune suppression)");
            Console.WriteLine("  --silent             Mode silencieux (aucune sortie console)");
            Console.WriteLine("  --list-profiles      Afficher les profils disponibles");
            Console.WriteLine("  --stats              Afficher les statistiques de nettoyage");
            Console.WriteLine("  --help, -h           Afficher cette aide");
            Console.WriteLine();
            Console.WriteLine("EXEMPLES:");
            Console.WriteLine("  windows-cleaner.exe --profile \"Nettoyage Rapide\"");
            Console.WriteLine("  windows-cleaner.exe --profile \"Nettoyage Complet\" --dry-run");
            Console.WriteLine("  windows-cleaner.exe --profile \"Protection Vie Privée\" --silent");
            Console.WriteLine();
            Console.WriteLine("PROFILS PRÉDÉFINIS:");
            Console.WriteLine("  - Nettoyage Rapide       : Nettoyage basique et rapide");
            Console.WriteLine("  - Nettoyage Complet      : Nettoyage complet avec options avancées");
            Console.WriteLine("  - Nettoyage Développeur  : Nettoyage spécial développeurs");
            Console.WriteLine("  - Protection Vie Privée  : Effacement traces d'activité");
            Console.WriteLine();
        }
        
        private static void ListProfiles()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║             PROFILS DE NETTOYAGE DISPONIBLES              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            var profiles = ProfileManager.GetAllProfiles();
            
            foreach (var profile in profiles)
            {
                Console.WriteLine($"📋 {profile.Name}");
                Console.WriteLine($"   {profile.Description}");
                Console.WriteLine();
            }
        }
        
        private static void ShowStatistics()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                     STATISTIQUES                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            var totalBytes = StatisticsManager.GetTotalBytesFreed();
            var totalFiles = StatisticsManager.GetTotalFilesDeleted();
            var totalSessions = StatisticsManager.GetTotalSessions();
            
            var last30Bytes = StatisticsManager.GetTotalBytesFreed(30);
            var last30Files = StatisticsManager.GetTotalFilesDeleted(30);
            var last30Sessions = StatisticsManager.GetTotalSessions(30);
            
            Console.WriteLine("TOTAL:");
            Console.WriteLine($"  Espace libéré: {FormatBytes(totalBytes)}");
            Console.WriteLine($"  Fichiers supprimés: {totalFiles:N0}");
            Console.WriteLine($"  Sessions: {totalSessions}");
            Console.WriteLine();
            
            Console.WriteLine("DERNIERS 30 JOURS:");
            Console.WriteLine($"  Espace libéré: {FormatBytes(last30Bytes)}");
            Console.WriteLine($"  Fichiers supprimés: {last30Files:N0}");
            Console.WriteLine($"  Sessions: {last30Sessions}");
            Console.WriteLine();
            
            var recentStats = StatisticsManager.GetRecentStatistics(10);
            if (recentStats.Any())
            {
                Console.WriteLine("DERNIÈRES SESSIONS:");
                foreach (var stat in recentStats)
                {
                    Console.WriteLine($"  {stat.Timestamp:dd/MM/yyyy HH:mm} - {stat.ProfileUsed}");
                    Console.WriteLine($"    {stat.FilesDeleted} fichiers, {stat.FormattedSize}");
                }
            }
            Console.WriteLine();
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
    }
}
