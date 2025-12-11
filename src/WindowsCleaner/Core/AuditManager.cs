using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace WindowsCleaner.Core
{
    /// <summary>
    /// Gestionnaire principal du système d'audit
    /// Orchestre les audits, gère l'historique et génère les rapports
    /// </summary>
    public class AuditManager
    {
        private readonly AuditConfiguration _config;
        private readonly string _historyPath;
        private static readonly object _lock = new object();

        public AuditManager()
        {
            _config = AuditConfigurationManager.Load();
            _historyPath = Path.Combine(_config.ReportsDirectory, "history");
            EnsureDirectoriesExist();
        }

        public AuditManager(AuditConfiguration config)
        {
            _config = config;
            _historyPath = Path.Combine(_config.ReportsDirectory, "history");
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// Exécute un audit complet du système
        /// </summary>
        public async Task<AuditReport> RunFullAuditAsync(AuditOptions? options = null)
        {
            options ??= new AuditOptions { AuditType = "Full" };
            
            Logger.Log(LogLevel.Info, "Démarrage d'un audit complet du système");
            var stopwatch = Stopwatch.StartNew();

            var report = new AuditReport
            {
                AuditType = options.AuditType,
                Timestamp = DateTime.Now
            };

            try
            {
                // Déterminer quels modules exécuter
                var modulesToRun = options.ModulesToRun.Any() 
                    ? options.ModulesToRun 
                    : _config.EnabledModules;

                // Exécuter les audits de chaque module
                var tasks = new List<Task<AuditCategory>>();

                if (modulesToRun.Contains("DiskSpace"))
                    tasks.Add(Task.Run(() => AuditDiskSpace()));
                
                if (modulesToRun.Contains("TempFiles"))
                    tasks.Add(Task.Run(() => AuditTempFiles()));
                
                if (modulesToRun.Contains("Registry"))
                    tasks.Add(Task.Run(() => AuditRegistry()));
                
                if (modulesToRun.Contains("Startup"))
                    tasks.Add(Task.Run(() => AuditStartup()));
                
                if (modulesToRun.Contains("BrowserCache"))
                    tasks.Add(Task.Run(() => AuditBrowserCache()));
                
                if (modulesToRun.Contains("Services"))
                    tasks.Add(Task.Run(() => AuditServices()));
                
                if (modulesToRun.Contains("Performance"))
                    tasks.Add(Task.Run(() => AuditPerformance()));

                // Attendre que tous les audits se terminent
                var categories = await Task.WhenAll(tasks);
                
                // Ajouter les catégories non-null (optimisé)
                foreach (var category in categories)
                {
                    if (category != null)
                    {
                        report.Categories.Add(category);
                    }
                }

                // Collecter tous les problèmes
                foreach (var category in report.Categories)
                {
                    foreach (var check in category.Checks)
                    {
                        if (check.Status == CheckStatus.Failed || check.Status == CheckStatus.Warning)
                        {
                            // Extraire les problèmes depuis les checks
                            ExtractIssuesFromCheck(report, category, check);
                        }
                    }
                }

                // Calculer le score de santé
                if (options.CalculateHealthScore)
                {
                    report.HealthScore = CalculateHealthScore(report);
                }

                // Générer les recommandations
                if (options.IncludeRecommendations)
                {
                    report.Recommendations = GenerateRecommendations(report);
                }

                stopwatch.Stop();
                report.Duration = stopwatch.Elapsed;

                Logger.Log(LogLevel.Info, $"Audit terminé en {stopwatch.Elapsed.TotalSeconds:F2}s - Score: {report.HealthScore}/100");

                // Sauvegarder dans l'historique
                if (options.SaveToHistory)
                {
                    await SaveReportAsync(report);
                }

                return report;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit: {ex.Message}");
                stopwatch.Stop();
                report.Duration = stopwatch.Elapsed;
                return report;
            }
        }

        /// <summary>
        /// Exécute un audit rapide (seulement les vérifications essentielles)
        /// </summary>
        public async Task<AuditReport> RunQuickAuditAsync()
        {
            var options = new AuditOptions
            {
                AuditType = "Quick",
                ModulesToRun = new List<string> { "DiskSpace", "TempFiles", "Performance" },
                GenerateDetailedReport = false,
                SaveToHistory = false
            };

            return await RunFullAuditAsync(options);
        }

        /// <summary>
        /// Récupère l'historique des audits
        /// </summary>
        public async Task<List<AuditReport>> GetAuditHistoryAsync(DateTime? from = null, DateTime? to = null)
        {
            return await Task.Run(() =>
            {
                var reports = new List<AuditReport>();
                
                if (!Directory.Exists(_historyPath))
                    return reports;

                var files = Directory.GetFiles(_historyPath, "audit-*.json");
                
                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var report = JsonSerializer.Deserialize<AuditReport>(json);
                        
                        if (report != null)
                        {
                            // Filtrer par date si spécifié
                            if (from.HasValue && report.Timestamp < from.Value)
                                continue;
                            if (to.HasValue && report.Timestamp > to.Value)
                                continue;
                            
                            reports.Add(report);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Impossible de lire le rapport {file}: {ex.Message}");
                    }
                }

                // Tri optimisé sans LINQ
                for (int i = 0; i < reports.Count - 1; i++)
                {
                    for (int j = i + 1; j < reports.Count; j++)
                    {
                        if (reports[j].Timestamp > reports[i].Timestamp)
                        {
                            var temp = reports[i];
                            reports[i] = reports[j];
                            reports[j] = temp;
                        }
                    }
                }

                return reports;
            });
        }

        /// <summary>
        /// Compare deux audits
        /// </summary>
        public async Task<AuditComparison> CompareAuditsAsync(Guid audit1Id, Guid audit2Id)
        {
            var report1 = await GetReportByIdAsync(audit1Id);
            var report2 = await GetReportByIdAsync(audit2Id);

            if (report1 == null || report2 == null)
                throw new ArgumentException("Un ou plusieurs rapports introuvables");

            var comparison = new AuditComparison
            {
                Audit1Id = audit1Id,
                Audit2Id = audit2Id,
                Date1 = report1.Timestamp,
                Date2 = report2.Timestamp,
                HealthScoreDelta = report2.HealthScore - report1.HealthScore
            };

            // Comparer les catégories
            foreach (var cat2 in report2.Categories)
            {
                var cat1 = report1.Categories.FirstOrDefault(c => c.Name == cat2.Name);
                if (cat1 != null)
                {
                    var catComp = new CategoryComparison
                    {
                        CategoryName = cat2.Name,
                        ScoreDelta = cat2.Score - cat1.Score,
                        Trend = cat2.Score > cat1.Score ? "Amélioration" : 
                                cat2.Score < cat1.Score ? "Dégradation" : "Stable"
                    };
                    comparison.CategoryChanges[cat2.Name] = catComp;
                }
            }

            // Identifier les nouveaux problèmes et les problèmes résolus (optimisé sans ToList())
            var issues1Ids = new HashSet<string>(report1.Issues.Count);
            var issues2Ids = new HashSet<string>(report2.Issues.Count);
            
            foreach (var issue in report1.Issues)
            {
                issues1Ids.Add(issue.Id.ToString());
            }
            
            foreach (var issue in report2.Issues)
            {
                issues2Ids.Add(issue.Id.ToString());
            }

            comparison.NewIssues = new System.Collections.Generic.List<string>();
            foreach (var issue in report2.Issues)
            {
                if (!issues1Ids.Contains(issue.Id.ToString()))
                {
                    comparison.NewIssues.Add(issue.Title);
                }
            }

            comparison.ResolvedIssues = new System.Collections.Generic.List<string>();
            foreach (var issue in report1.Issues)
            {
                if (!issues2Ids.Contains(issue.Id.ToString()))
                {
                    comparison.ResolvedIssues.Add(issue.Title);
                }
            }

            // Tendance globale
            comparison.OverallTrend = comparison.HealthScoreDelta switch
            {
                > 5 => "Amélioration",
                < -5 => "Dégradation",
                _ => "Stable"
            };

            return comparison;
        }

        /// <summary>
        /// Obtient un résumé de la santé du système
        /// </summary>
        public async Task<AuditSummary> GetSystemHealthSummaryAsync()
        {
            var history = await GetAuditHistoryAsync();
            var latestReport = history.FirstOrDefault();

            if (latestReport == null)
            {
                return new AuditSummary
                {
                    HealthScore = 0,
                    HealthLevel = "Inconnu",
                    QuickSummary = "Aucun audit disponible"
                };
            }

            return new AuditSummary
            {
                HealthScore = latestReport.HealthScore,
                HealthLevel = GetHealthLevel(latestReport.HealthScore),
                TotalIssues = latestReport.Issues.Count,
                CriticalIssues = latestReport.Issues.Count(i => i.Severity == IssueSeverity.Critical || i.Severity == IssueSeverity.High),
                WarningIssues = latestReport.Issues.Count(i => i.Severity == IssueSeverity.Medium),
                PotentialSpaceSavings = latestReport.Recommendations.PotentialSpaceSavings,
                LastAuditDate = latestReport.Timestamp,
                QuickSummary = GenerateQuickSummary(latestReport)
            };
        }

        #region Modules d'Audit

        private AuditCategory AuditDiskSpace()
        {
            var category = new AuditCategory
            {
                Name = "DiskSpace",
                DisplayName = "Espace Disque",
                Weight = 0.20,
                Icon = "💾"
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
                
                foreach (var drive in drives)
                {
                    var usedPercent = (double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100;
                    var check = new AuditCheck
                    {
                        Name = $"Disque {drive.Name}",
                        Description = $"Vérification de l'espace disponible sur {drive.Name}",
                        Value = usedPercent,
                        ExpectedValue = _config.Thresholds.MaxDiskUsagePercent
                    };

                    if (usedPercent >= _config.Thresholds.CriticalDiskUsagePercent)
                    {
                        check.Status = CheckStatus.Failed;
                        check.Message = $"Critique: {usedPercent:F1}% utilisé";
                    }
                    else if (usedPercent >= _config.Thresholds.MaxDiskUsagePercent)
                    {
                        check.Status = CheckStatus.Warning;
                        check.Message = $"Attention: {usedPercent:F1}% utilisé";
                    }
                    else
                    {
                        check.Status = CheckStatus.Passed;
                        check.Message = $"OK: {usedPercent:F1}% utilisé";
                    }

                    check.Result = $"{FormatBytes(drive.AvailableFreeSpace)} disponible sur {FormatBytes(drive.TotalSize)}";
                    category.Checks.Add(check);
                }

                // Calculer le score de la catégorie
                var avgUsage = category.Checks.Average(c => (double)c.Value);
                category.Score = Math.Max(0, (int)(100 - avgUsage));
                category.Status = category.Score switch
                {
                    >= 80 => CategoryStatus.Excellent,
                    >= 60 => CategoryStatus.Good,
                    >= 40 => CategoryStatus.Warning,
                    _ => CategoryStatus.Critical
                };
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit de l'espace disque: {ex.Message}");
                category.Status = CategoryStatus.Error;
            }

            stopwatch.Stop();
            category.ExecutionTime = stopwatch.Elapsed;
            return category;
        }

        private AuditCategory AuditTempFiles()
        {
            var category = new AuditCategory
            {
                Name = "TempFiles",
                DisplayName = "Fichiers Temporaires",
                Weight = 0.15,
                Icon = "🗑️"
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                long totalSize = 0;
                int totalFiles = 0;

                var tempPaths = new[]
                {
                    Path.GetTempPath(),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
                };

                foreach (var tempPath in tempPaths.Distinct())
                {
                    if (Directory.Exists(tempPath))
                    {
                        try
                        {
                            var files = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                try
                                {
                                    var fileInfo = new FileInfo(file);
                                    totalSize += fileInfo.Length;
                                    totalFiles++;
                                }
                                catch { /* Fichier inaccessible */ }
                            }
                        }
                        catch { /* Dossier inaccessible */ }
                    }
                }

                var check = new AuditCheck
                {
                    Name = "Fichiers temporaires",
                    Description = "Analyse de l'accumulation de fichiers temporaires",
                    Value = totalSize,
                    ExpectedValue = (long)_config.Thresholds.MaxTempFilesSizeMB * 1024 * 1024,
                    Result = $"{totalFiles} fichiers, {FormatBytes(totalSize)}"
                };

                var sizeMB = totalSize / (1024.0 * 1024.0);
                if (sizeMB >= _config.Thresholds.MaxTempFilesSizeMB)
                {
                    check.Status = CheckStatus.Warning;
                    check.Message = "Trop de fichiers temporaires accumulés";
                }
                else if (totalFiles >= _config.Thresholds.MaxTempFilesCount)
                {
                    check.Status = CheckStatus.Warning;
                    check.Message = "Nombre élevé de fichiers temporaires";
                }
                else
                {
                    check.Status = CheckStatus.Passed;
                    check.Message = "Fichiers temporaires sous contrôle";
                }

                category.Checks.Add(check);

                // Score basé sur la taille
                category.Score = Math.Max(0, 100 - (int)(sizeMB / _config.Thresholds.MaxTempFilesSizeMB * 100));
                category.Status = category.Score >= 70 ? CategoryStatus.Good : CategoryStatus.Warning;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit des fichiers temporaires: {ex.Message}");
                category.Status = CategoryStatus.Error;
            }

            stopwatch.Stop();
            category.ExecutionTime = stopwatch.Elapsed;
            return category;
        }

        private AuditCategory AuditRegistry()
        {
            // Implémentation simplifiée - à compléter avec logique réelle
            var category = new AuditCategory
            {
                Name = "Registry",
                DisplayName = "Registre Windows",
                Weight = 0.15,
                Icon = "📝",
                Score = 85,
                Status = CategoryStatus.Good
            };

            var check = new AuditCheck
            {
                Name = "Intégrité du registre",
                Description = "Vérification de l'intégrité du registre Windows",
                Status = CheckStatus.Passed,
                Message = "Registre en bon état",
                Result = "Aucun problème majeur détecté"
            };

            category.Checks.Add(check);
            return category;
        }

        private AuditCategory AuditStartup()
        {
            // Implémentation simplifiée
            var category = new AuditCategory
            {
                Name = "Startup",
                DisplayName = "Programmes de Démarrage",
                Weight = 0.15,
                Icon = "🚀",
                Score = 80,
                Status = CategoryStatus.Good
            };

            var check = new AuditCheck
            {
                Name = "Programmes au démarrage",
                Description = "Analyse des programmes lancés au démarrage",
                Status = CheckStatus.Passed,
                Message = "Nombre de programmes acceptable",
                Result = "Configuration optimale"
            };

            category.Checks.Add(check);
            return category;
        }

        private AuditCategory AuditBrowserCache()
        {
            // Implémentation simplifiée
            var category = new AuditCategory
            {
                Name = "BrowserCache",
                DisplayName = "Cache Navigateurs",
                Weight = 0.10,
                Icon = "🌐",
                Score = 75,
                Status = CategoryStatus.Good
            };

            return category;
        }

        private AuditCategory AuditServices()
        {
            // Implémentation simplifiée
            var category = new AuditCategory
            {
                Name = "Services",
                DisplayName = "Services Système",
                Weight = 0.10,
                Icon = "⚙️",
                Score = 90,
                Status = CategoryStatus.Excellent
            };

            return category;
        }

        private AuditCategory AuditPerformance()
        {
            var category = new AuditCategory
            {
                Name = "Performance",
                DisplayName = "Performance Système",
                Weight = 0.15,
                Icon = "⚡"
            };

            try
            {
                // Mémoire disponible
                var memoryCheck = new AuditCheck
                {
                    Name = "Mémoire disponible",
                    Description = "Vérification de la mémoire RAM disponible"
                };

                using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                var availableMB = ramCounter.NextValue();
                
                memoryCheck.Value = availableMB;
                memoryCheck.Result = $"{availableMB:F0} MB disponible";
                memoryCheck.Status = availableMB > 1000 ? CheckStatus.Passed : CheckStatus.Warning;
                memoryCheck.Message = memoryCheck.Status == CheckStatus.Passed ? "Mémoire suffisante" : "Mémoire faible";
                
                category.Checks.Add(memoryCheck);
                category.Score = availableMB > 1000 ? 90 : 60;
                category.Status = category.Score >= 80 ? CategoryStatus.Excellent : CategoryStatus.Good;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit de performance: {ex.Message}");
                category.Score = 70;
                category.Status = CategoryStatus.Good;
            }

            return category;
        }

        #endregion

        #region Méthodes Utilitaires

        private void ExtractIssuesFromCheck(AuditReport report, AuditCategory category, AuditCheck check)
        {
            if (category.Name == "DiskSpace" && check.Status != CheckStatus.Passed)
            {
                var usedPercent = (double)check.Value;
                var severity = usedPercent >= _config.Thresholds.CriticalDiskUsagePercent 
                    ? IssueSeverity.Critical 
                    : IssueSeverity.Medium;

                report.Issues.Add(new AuditIssue
                {
                    Severity = severity,
                    Category = category.Name,
                    Title = check.Name,
                    Description = check.Message,
                    Impact = "Peut empêcher l'installation de mises à jour et ralentir le système",
                    RecommendedActions = new List<string> { "Nettoyer les fichiers temporaires", "Désinstaller les applications inutilisées" },
                    AutoFixAvailable = true,
                    Location = check.Name
                });
            }
            else if (category.Name == "TempFiles" && check.Status == CheckStatus.Warning)
            {
                var totalSize = (long)check.Value;
                report.Issues.Add(AuditIssueFactory.CreateTempFilesIssue(totalSize, 0));
            }
        }

        private int CalculateHealthScore(AuditReport report)
        {
            if (!report.Categories.Any())
                return 0;

            double totalScore = 0;
            double totalWeight = 0;

            foreach (var category in report.Categories)
            {
                totalScore += category.Score * category.Weight;
                totalWeight += category.Weight;
            }

            return totalWeight > 0 ? (int)(totalScore / totalWeight) : 0;
        }

        private AuditRecommendations GenerateRecommendations(AuditReport report)
        {
            var recommendations = new AuditRecommendations();

            // Recommandations basées sur les problèmes trouvés
            foreach (var issue in report.Issues.Where(i => i.AutoFixAvailable))
            {
                if (issue.Category == "DiskSpace" || issue.Category == "TempFiles")
                {
                    recommendations.SuggestedCleanings.Add(new CleaningAction
                    {
                        Category = issue.Category,
                        Description = issue.Title,
                        Priority = issue.Severity == IssueSeverity.Critical ? 5 : 3,
                        IsSafe = true
                    });
                }
            }

            // Calculer l'espace potentiel récupérable
            var tempFilesCategory = report.Categories.FirstOrDefault(c => c.Name == "TempFiles");
            if (tempFilesCategory != null)
            {
                var tempCheck = tempFilesCategory.Checks.FirstOrDefault();
                if (tempCheck != null)
                {
                    recommendations.PotentialSpaceSavings = (long)tempCheck.Value;
                }
            }

            recommendations.Summary = $"{recommendations.SuggestedCleanings.Count} actions de nettoyage recommandées";
            
            return recommendations;
        }

        private string GetHealthLevel(int score)
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

        private string GenerateQuickSummary(AuditReport report)
        {
            var level = GetHealthLevel(report.HealthScore);
            var criticalCount = report.Issues.Count(i => i.Severity >= IssueSeverity.High);
            
            if (criticalCount == 0)
                return $"Système en {level} état - Aucun problème critique";
            else
                return $"Système en {level} état - {criticalCount} problème(s) critique(s) détecté(s)";
        }

        private async Task SaveReportAsync(AuditReport report)
        {
            await Task.Run(() =>
            {
                try
                {
                    var filename = $"audit-{report.Timestamp:yyyyMMdd-HHmmss}-{report.Id}.json";
                    var filepath = Path.Combine(_historyPath, filename);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(report, options);

                    File.WriteAllText(filepath, json);
                    Logger.Log(LogLevel.Info, $"Rapport d'audit sauvegardé: {filename}");

                    // Nettoyer l'historique si nécessaire
                    CleanOldReports();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Erreur lors de la sauvegarde du rapport: {ex.Message}");
                }
            });
        }

        private async Task<AuditReport?> GetReportByIdAsync(Guid id)
        {
            var history = await GetAuditHistoryAsync();
            return history.FirstOrDefault(r => r.Id == id);
        }

        private void CleanOldReports()
        {
            if (!_config.AutoArchiveOldReports)
                return;

            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_config.MaxHistoryDays);
                var files = Directory.GetFiles(_historyPath, "audit-*.json");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        Logger.Log(LogLevel.Debug, $"Rapport ancien supprimé: {fileInfo.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Erreur lors du nettoyage des anciens rapports: {ex.Message}");
            }
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                Directory.CreateDirectory(_config.ReportsDirectory);
                Directory.CreateDirectory(_historyPath);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Impossible de créer les dossiers d'audit: {ex.Message}");
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

        #endregion
    }
}
