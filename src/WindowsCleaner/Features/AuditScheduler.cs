using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsCleaner.Core;

namespace WindowsCleaner.Features
{
    /// <summary>
    /// Planificateur d'audits automatiques
    /// Gère l'exécution périodique des audits selon la configuration
    /// </summary>
    public class AuditScheduler : IDisposable
    {
        private readonly AuditManager _auditManager;
        private readonly AuditConfiguration _config;
        private System.Threading.Timer? _dailyTimer;
        private System.Threading.Timer? _weeklyTimer;
        private System.Threading.Timer? _monthlyTimer;
        private readonly object _lock = new object();
        private bool _isRunning = false;

        public event EventHandler<AuditReport>? OnAuditCompleted;
        public event EventHandler<Exception>? OnAuditError;

        public AuditScheduler(AuditManager? auditManager = null, AuditConfiguration? config = null)
        {
            _auditManager = auditManager ?? new AuditManager();
            _config = config ?? AuditConfigurationManager.Load();
        }

        /// <summary>
        /// Démarre le planificateur d'audits
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    Logger.Log(LogLevel.Warning, "Le planificateur d'audits est déjà démarré");
                    return;
                }

                if (!_config.EnableAutomaticAudits)
                {
                    Logger.Log(LogLevel.Info, "Les audits automatiques sont désactivés");
                    return;
                }

                Logger.Log(LogLevel.Info, "Démarrage du planificateur d'audits");

                // Planifier les audits quotidiens
                if (_config.Schedule.EnableDaily)
                {
                    ScheduleDailyAudit();
                }

                // Planifier les audits hebdomadaires
                if (_config.Schedule.EnableWeekly)
                {
                    ScheduleWeeklyAudit();
                }

                // Planifier les audits mensuels
                if (_config.Schedule.EnableMonthly)
                {
                    ScheduleMonthlyAudit();
                }

                _isRunning = true;
                Logger.Log(LogLevel.Info, "Planificateur d'audits démarré avec succès");
            }
        }

        /// <summary>
        /// Arrête le planificateur d'audits
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning)
                    return;

                Logger.Log(LogLevel.Info, "Arrêt du planificateur d'audits");

                _dailyTimer?.Dispose();
                _weeklyTimer?.Dispose();
                _monthlyTimer?.Dispose();

                _dailyTimer = null;
                _weeklyTimer = null;
                _monthlyTimer = null;

                _isRunning = false;
                Logger.Log(LogLevel.Info, "Planificateur d'audits arrêté");
            }
        }

        /// <summary>
        /// Exécute un audit manuellement
        /// </summary>
        public async Task<AuditReport> RunManualAuditAsync()
        {
            Logger.Log(LogLevel.Info, "Exécution d'un audit manuel");
            
            try
            {
                var options = new AuditOptions
                {
                    AuditType = "Manual",
                    GenerateDetailedReport = true,
                    IncludeRecommendations = true,
                    CalculateHealthScore = true,
                    SaveToHistory = true
                };

                var report = await _auditManager.RunFullAuditAsync(options);
                OnAuditCompleted?.Invoke(this, report);
                
                // Vérifier si des alertes doivent être déclenchées
                CheckAndTriggerAlerts(report);
                
                return report;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit manuel: {ex.Message}");
                OnAuditError?.Invoke(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Exécute un audit rapide
        /// </summary>
        public async Task<AuditReport> RunQuickAuditAsync()
        {
            Logger.Log(LogLevel.Info, "Exécution d'un audit rapide");
            
            try
            {
                var report = await _auditManager.RunQuickAuditAsync();
                OnAuditCompleted?.Invoke(this, report);
                return report;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit rapide: {ex.Message}");
                OnAuditError?.Invoke(this, ex);
                throw;
            }
        }

        private void ScheduleDailyAudit()
        {
            var now = DateTime.Now;
            var scheduledTime = DateTime.Today.Add(_config.Schedule.DailyTime.ToTimeSpan());
            
            // Si l'heure est déjà passée aujourd'hui, planifier pour demain
            if (scheduledTime < now)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var timeUntilFirst = scheduledTime - now;
            var period = TimeSpan.FromDays(1);

            _dailyTimer = new System.Threading.Timer(
                async _ => await ExecuteScheduledAudit("Daily"),
                null,
                timeUntilFirst,
                period
            );

            Logger.Log(LogLevel.Info, $"Audit quotidien planifié pour {scheduledTime:HH:mm}");
        }

        private void ScheduleWeeklyAudit()
        {
            var now = DateTime.Now;
            var daysUntilTarget = ((int)_config.Schedule.WeeklyDay - (int)now.DayOfWeek + 7) % 7;
            
            var scheduledTime = DateTime.Today.AddDays(daysUntilTarget).Add(_config.Schedule.WeeklyTime.ToTimeSpan());
            
            // Si c'est le bon jour mais l'heure est passée, planifier pour la semaine prochaine
            if (daysUntilTarget == 0 && scheduledTime < now)
            {
                scheduledTime = scheduledTime.AddDays(7);
            }

            var timeUntilFirst = scheduledTime - now;
            var period = TimeSpan.FromDays(7);

            _weeklyTimer = new System.Threading.Timer(
                async _ => await ExecuteScheduledAudit("Weekly"),
                null,
                timeUntilFirst,
                period
            );

            Logger.Log(LogLevel.Info, $"Audit hebdomadaire planifié pour {_config.Schedule.WeeklyDay} à {_config.Schedule.WeeklyTime}");
        }

        private void ScheduleMonthlyAudit()
        {
            var now = DateTime.Now;
            var targetDay = Math.Min(_config.Schedule.MonthlyDay, DateTime.DaysInMonth(now.Year, now.Month));
            
            var scheduledTime = new DateTime(now.Year, now.Month, targetDay)
                .Add(_config.Schedule.MonthlyTime.ToTimeSpan());

            // Si la date est déjà passée ce mois-ci, planifier pour le mois prochain
            if (scheduledTime < now)
            {
                var nextMonth = now.AddMonths(1);
                targetDay = Math.Min(_config.Schedule.MonthlyDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                scheduledTime = new DateTime(nextMonth.Year, nextMonth.Month, targetDay)
                    .Add(_config.Schedule.MonthlyTime.ToTimeSpan());
            }

            var timeUntilFirst = scheduledTime - now;
            
            // Pour les audits mensuels, on recalcule à chaque fois car le nombre de jours varie
            _monthlyTimer = new System.Threading.Timer(
                async _ => 
                {
                    await ExecuteScheduledAudit("Monthly");
                    ScheduleMonthlyAudit(); // Replanifier pour le mois suivant
                },
                null,
                timeUntilFirst,
                Timeout.InfiniteTimeSpan
            );

            Logger.Log(LogLevel.Info, $"Audit mensuel planifié pour le {targetDay} à {_config.Schedule.MonthlyTime}");
        }

        private async Task ExecuteScheduledAudit(string auditType)
        {
            try
            {
                Logger.Log(LogLevel.Info, $"Démarrage de l'audit planifié ({auditType})");

                var options = new AuditOptions
                {
                    AuditType = $"Scheduled-{auditType}",
                    GenerateDetailedReport = true,
                    IncludeRecommendations = true,
                    CalculateHealthScore = true,
                    SaveToHistory = true
                };

                var report = await _auditManager.RunFullAuditAsync(options);
                OnAuditCompleted?.Invoke(this, report);

                // Vérifier si des alertes doivent être déclenchées
                CheckAndTriggerAlerts(report);

                Logger.Log(LogLevel.Info, $"Audit planifié ({auditType}) terminé - Score: {report.HealthScore}/100");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'audit planifié ({auditType}): {ex.Message}");
                OnAuditError?.Invoke(this, ex);
            }
        }

        private void CheckAndTriggerAlerts(AuditReport report)
        {
            if (!_config.Notifications.EnableNotifications)
                return;

            // Alerte sur score de santé faible
            if (_config.Notifications.NotifyOnLowHealthScore && 
                report.HealthScore < _config.Thresholds.MinHealthScore)
            {
                TriggerAlert($"Score de santé faible: {report.HealthScore}/100", 
                    "Le système nécessite une attention immédiate");
            }

            // Alerte sur problèmes critiques
            if (_config.Notifications.NotifyOnCriticalIssues)
            {
                var criticalIssues = report.Issues.Where(i => 
                    i.Severity == IssueSeverity.Critical || 
                    i.Severity == IssueSeverity.High).ToList();

                if (criticalIssues.Any())
                {
                    TriggerAlert($"{criticalIssues.Count} problème(s) critique(s) détecté(s)", 
                        string.Join(", ", criticalIssues.Select(i => i.Title).Take(3)));
                }
            }

            // Alerte sur espace disque
            var diskIssues = report.Issues.Where(i => i.Category == "DiskSpace" && 
                i.Severity >= IssueSeverity.High).ToList();
            
            if (diskIssues.Any())
            {
                TriggerAlert("Espace disque critique", diskIssues.First().Description);
            }
        }

        private void TriggerAlert(string title, string message)
        {
            Logger.Log(LogLevel.Warning, $"ALERTE: {title} - {message}");
            
            // TODO: Implémenter notifications Windows (Toast)
            // TODO: Implémenter notifications par email si configuré
            
            if (_config.Notifications.ShowDesktopNotifications)
            {
                // Placeholder pour notification desktop
                System.Diagnostics.Debug.WriteLine($"NOTIFICATION: {title}\n{message}");
            }
        }

        public void Dispose()
        {
            Stop();
            _dailyTimer?.Dispose();
            _weeklyTimer?.Dispose();
            _monthlyTimer?.Dispose();
        }
    }
}
