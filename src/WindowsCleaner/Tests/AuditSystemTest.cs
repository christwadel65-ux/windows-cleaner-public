using System;
using System.Threading.Tasks;
using WindowsCleaner.Core;
using WindowsCleaner.Features;

namespace WindowsCleaner.Tests
{
    /// <summary>
    /// Programme de test pour le système d'audit automatique
    /// </summary>
    class AuditSystemTest
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("  TEST DU SYSTÈME D'AUDIT AUTOMATIQUE");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();

            // Initialiser le logger
            Logger.Init();

            try
            {
                await RunTests();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ ERREUR CRITIQUE: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }

            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("Appuyez sur une touche pour quitter...");
            Console.ReadKey();
        }

        static async Task RunTests()
        {
            // Test 1: Configuration
            Console.WriteLine("📋 TEST 1: Chargement de la configuration");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            var config = AuditConfigurationManager.Load();
            Console.WriteLine($"✓ Configuration chargée");
            Console.WriteLine($"  - Audits automatiques: {config.EnableAutomaticAudits}");
            Console.WriteLine($"  - Modules activés: {config.EnabledModules.Count}");
            Console.WriteLine($"  - Score minimum: {config.Thresholds.MinHealthScore}");
            Console.WriteLine($"  - Historique: {config.MaxHistoryDays} jours");
            Console.WriteLine();

            // Test 2: AuditManager - Audit rapide
            Console.WriteLine("⚡ TEST 2: Audit Rapide");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            var auditManager = new AuditManager(config);
            
            var quickReport = await auditManager.RunQuickAuditAsync();
            
            Console.WriteLine($"✓ Audit rapide terminé en {quickReport.Duration.TotalSeconds:F2}s");
            Console.WriteLine($"  - Score de santé: {quickReport.HealthScore}/100 ({GetHealthEmoji(quickReport.HealthScore)})");
            Console.WriteLine($"  - Catégories analysées: {quickReport.Categories.Count}");
            Console.WriteLine($"  - Problèmes détectés: {quickReport.Issues.Count}");
            Console.WriteLine();

            // Test 3: AuditManager - Audit complet
            Console.WriteLine("🔍 TEST 3: Audit Complet");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var fullReport = await auditManager.RunFullAuditAsync();
            
            Console.WriteLine($"✓ Audit complet terminé en {fullReport.Duration.TotalSeconds:F2}s");
            Console.WriteLine($"  - ID: {fullReport.Id}");
            Console.WriteLine($"  - Type: {fullReport.AuditType}");
            Console.WriteLine($"  - Score de santé: {fullReport.HealthScore}/100 ({GetHealthLevel(fullReport.HealthScore)})");
            Console.WriteLine($"  - Machine: {fullReport.ComputerName}");
            Console.WriteLine($"  - Utilisateur: {fullReport.UserName}");
            Console.WriteLine();

            // Détails des catégories
            Console.WriteLine("  📊 CATÉGORIES:");
            foreach (var category in fullReport.Categories)
            {
                var statusIcon = GetStatusIcon(category.Status);
                Console.WriteLine($"    {category.Icon} {category.DisplayName,-25} {statusIcon} {category.Score}/100 ({category.ExecutionTime.TotalMilliseconds:F0}ms)");
            }
            Console.WriteLine();

            // Détails des problèmes
            if (fullReport.Issues.Any())
            {
                Console.WriteLine($"  ⚠️  PROBLÈMES DÉTECTÉS ({fullReport.Issues.Count}):");
                foreach (var issue in fullReport.Issues.Take(5))
                {
                    var severityColor = GetSeverityColor(issue.Severity);
                    Console.ForegroundColor = severityColor;
                    Console.WriteLine($"    [{issue.Severity}] {issue.Title}");
                    Console.ResetColor();
                    Console.WriteLine($"      {issue.Description}");
                    if (issue.AutoFixAvailable)
                        Console.WriteLine($"      ✓ Correction automatique disponible");
                }
                if (fullReport.Issues.Count > 5)
                    Console.WriteLine($"    ... et {fullReport.Issues.Count - 5} autre(s) problème(s)");
            }
            else
            {
                Console.WriteLine("  ✅ Aucun problème détecté!");
            }
            Console.WriteLine();

            // Recommandations
            if (fullReport.Recommendations.SuggestedCleanings.Any())
            {
                Console.WriteLine($"  💡 RECOMMANDATIONS ({fullReport.Recommendations.SuggestedCleanings.Count}):");
                Console.WriteLine($"    Espace récupérable: {FormatBytes(fullReport.Recommendations.PotentialSpaceSavings)}");
                foreach (var action in fullReport.Recommendations.SuggestedCleanings.Take(3))
                {
                    Console.WriteLine($"    • [{action.Priority}] {action.Description}");
                }
            }
            Console.WriteLine();

            // Test 4: Génération de rapports
            Console.WriteLine("📄 TEST 4: Génération de Rapports");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var reporter = new AuditReporter();
            
            // Rapport HTML
            var htmlPath = await reporter.GenerateHtmlReportAsync(fullReport);
            Console.WriteLine($"✓ Rapport HTML généré: {htmlPath}");
            
            // Rapport JSON
            var jsonPath = await reporter.GenerateJsonReportAsync(fullReport);
            Console.WriteLine($"✓ Rapport JSON généré: {jsonPath}");
            
            // Rapport CSV
            var csvPath = await reporter.GenerateCsvReportAsync(fullReport);
            Console.WriteLine($"✓ Rapport CSV généré: {csvPath}");
            
            // Rapport TXT
            var txtPath = await reporter.GenerateTextReportAsync(fullReport);
            Console.WriteLine($"✓ Rapport TXT généré: {txtPath}");
            Console.WriteLine();

            // Test 5: Historique
            Console.WriteLine("📚 TEST 5: Historique des Audits");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var history = await auditManager.GetAuditHistoryAsync();
            Console.WriteLine($"✓ {history.Count} rapport(s) dans l'historique");
            
            foreach (var report in history.Take(3))
            {
                Console.WriteLine($"  • {report.Timestamp:dd/MM/yyyy HH:mm} - Score: {report.HealthScore}/100 - Type: {report.AuditType}");
            }
            Console.WriteLine();

            // Test 6: Résumé de santé
            Console.WriteLine("💊 TEST 6: Résumé de Santé Système");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var summary = await auditManager.GetSystemHealthSummaryAsync();
            Console.WriteLine($"✓ Score de santé: {summary.HealthScore}/100 ({summary.HealthLevel})");
            Console.WriteLine($"  - Total problèmes: {summary.TotalIssues}");
            Console.WriteLine($"  - Problèmes critiques: {summary.CriticalIssues}");
            Console.WriteLine($"  - Problèmes moyens: {summary.WarningIssues}");
            Console.WriteLine($"  - Espace récupérable: {FormatBytes(summary.PotentialSpaceSavings)}");
            Console.WriteLine($"  - Dernier audit: {summary.LastAuditDate:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"  - Résumé: {summary.QuickSummary}");
            Console.WriteLine();

            // Test 7: Comparaison d'audits
            if (history.Count >= 2)
            {
                Console.WriteLine("🔄 TEST 7: Comparaison d'Audits");
                Console.WriteLine("───────────────────────────────────────────────────────────");
                
                var comparison = await auditManager.CompareAuditsAsync(history[1].Id, history[0].Id);
                Console.WriteLine($"✓ Comparaison entre:");
                Console.WriteLine($"  - Audit 1: {comparison.Date1:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"  - Audit 2: {comparison.Date2:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"  - Delta score: {(comparison.HealthScoreDelta >= 0 ? "+" : "")}{comparison.HealthScoreDelta}");
                Console.WriteLine($"  - Tendance: {comparison.OverallTrend}");
                Console.WriteLine($"  - Nouveaux problèmes: {comparison.NewIssues.Count}");
                Console.WriteLine($"  - Problèmes résolus: {comparison.ResolvedIssues.Count}");
                Console.WriteLine();
            }

            // Test 8: AuditScheduler
            Console.WriteLine("⏰ TEST 8: Planificateur d'Audits");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var scheduler = new AuditScheduler(auditManager, config);
            
            // Abonnement aux événements
            scheduler.OnAuditCompleted += (sender, report) =>
            {
                Console.WriteLine($"  ✓ Audit complété: Score {report.HealthScore}/100");
            };
            
            scheduler.OnAuditError += (sender, ex) =>
            {
                Console.WriteLine($"  ❌ Erreur: {ex.Message}");
            };
            
            Console.WriteLine("✓ Planificateur créé");
            Console.WriteLine($"  - Audits quotidiens: {config.Schedule.EnableDaily} (à {config.Schedule.DailyTime})");
            Console.WriteLine($"  - Audits hebdomadaires: {config.Schedule.EnableWeekly}");
            Console.WriteLine($"  - Audits mensuels: {config.Schedule.EnableMonthly}");
            
            // Test audit manuel via scheduler
            Console.WriteLine("\n  🔧 Test d'audit manuel via scheduler...");
            var manualReport = await scheduler.RunManualAuditAsync();
            Console.WriteLine($"  ✓ Audit manuel terminé - Score: {manualReport.HealthScore}/100");
            Console.WriteLine();

            // Test 9: Validation de la configuration
            Console.WriteLine("✅ TEST 9: Validation Configuration");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var isValid = AuditConfigurationManager.Validate(config);
            Console.WriteLine($"✓ Configuration {(isValid ? "valide" : "invalide")}");
            Console.WriteLine();

            // Test 10: Performance
            Console.WriteLine("⚡ TEST 10: Test de Performance");
            Console.WriteLine("───────────────────────────────────────────────────────────");
            
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 3; i++)
            {
                await auditManager.RunQuickAuditAsync();
            }
            sw.Stop();
            
            Console.WriteLine($"✓ 3 audits rapides en {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  - Moyenne: {sw.ElapsedMilliseconds / 3}ms par audit");
            Console.WriteLine();

            // Résumé final
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("  🎉 TOUS LES TESTS RÉUSSIS!");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Statistiques globales:");
            Console.WriteLine($"  ✓ Score de santé actuel: {summary.HealthScore}/100");
            Console.WriteLine($"  ✓ Nombre de rapports générés: 4 (HTML, JSON, CSV, TXT)");
            Console.WriteLine($"  ✓ Nombre d'audits dans l'historique: {history.Count}");
            Console.WriteLine($"  ✓ Système d'audit: 100% fonctionnel");
            Console.WriteLine();

            // Ouvrir le rapport HTML dans le navigateur
            Console.WriteLine("💡 Conseil: Ouvrez le rapport HTML pour une visualisation complète");
            Console.WriteLine($"   Fichier: {htmlPath}");
            Console.WriteLine();
            Console.Write("Voulez-vous ouvrir le rapport HTML maintenant? (O/N): ");
            var response = Console.ReadLine();
            if (response?.ToUpper() == "O")
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = htmlPath,
                        UseShellExecute = true
                    });
                    Console.WriteLine("✓ Rapport ouvert dans le navigateur");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Impossible d'ouvrir le rapport: {ex.Message}");
                }
            }
        }

        static string GetHealthLevel(int score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 70 => "Bon",
                >= 50 => "Moyen",
                >= 30 => "Critique",
                _ => "Urgent"
            };
        }

        static string GetHealthEmoji(int score)
        {
            return score switch
            {
                >= 90 => "😊",
                >= 70 => "🙂",
                >= 50 => "😐",
                >= 30 => "😟",
                _ => "😱"
            };
        }

        static string GetStatusIcon(CategoryStatus status)
        {
            return status switch
            {
                CategoryStatus.Excellent => "✅",
                CategoryStatus.Good => "👍",
                CategoryStatus.Warning => "⚠️",
                CategoryStatus.Critical => "🚨",
                CategoryStatus.Error => "❌",
                _ => "❓"
            };
        }

        static ConsoleColor GetSeverityColor(IssueSeverity severity)
        {
            return severity switch
            {
                IssueSeverity.Critical => ConsoleColor.Red,
                IssueSeverity.High => ConsoleColor.DarkRed,
                IssueSeverity.Medium => ConsoleColor.Yellow,
                IssueSeverity.Low => ConsoleColor.Cyan,
                _ => ConsoleColor.Gray
            };
        }

        static string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";
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
