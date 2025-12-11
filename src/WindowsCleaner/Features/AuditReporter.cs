using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsCleaner.Core;

namespace WindowsCleaner.Features
{
    /// <summary>
    /// Générateur de rapports d'audit dans différents formats
    /// </summary>
    public class AuditReporter
    {
        private readonly string _reportsDirectory;

        public AuditReporter(string? reportsDirectory = null)
        {
            _reportsDirectory = reportsDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WindowsCleaner", "AuditReports"
            );

            Directory.CreateDirectory(_reportsDirectory);
        }

        /// <summary>
        /// Génère un rapport HTML interactif
        /// </summary>
        public async Task<string> GenerateHtmlReportAsync(AuditReport report, string? outputPath = null)
        {
            outputPath ??= Path.Combine(_reportsDirectory, $"audit-report-{DateTime.Now:yyyyMMdd-HHmmss}.html");

            var html = GenerateHtmlContent(report);
            await File.WriteAllTextAsync(outputPath, html);

            Logger.Log(LogLevel.Info, $"Rapport HTML généré: {outputPath}");
            return outputPath;
        }

        /// <summary>
        /// Génère un rapport JSON
        /// </summary>
        public async Task<string> GenerateJsonReportAsync(AuditReport report, string? outputPath = null)
        {
            outputPath ??= Path.Combine(_reportsDirectory, $"audit-report-{DateTime.Now:yyyyMMdd-HHmmss}.json");

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(report, options);
            await File.WriteAllTextAsync(outputPath, json);

            Logger.Log(LogLevel.Info, $"Rapport JSON généré: {outputPath}");
            return outputPath;
        }

        /// <summary>
        /// Génère un rapport CSV des problèmes
        /// </summary>
        public async Task<string> GenerateCsvReportAsync(AuditReport report, string? outputPath = null)
        {
            outputPath ??= Path.Combine(_reportsDirectory, $"audit-issues-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

            var csv = new StringBuilder();
            csv.AppendLine("Sévérité,Catégorie,Titre,Description,Impact,Correction Auto,Emplacement");

            foreach (var issue in report.Issues)
            {
                csv.AppendLine($"{issue.Severity},{EscapeCsv(issue.Category)},{EscapeCsv(issue.Title)},{EscapeCsv(issue.Description)},{EscapeCsv(issue.Impact)},{issue.AutoFixAvailable},{EscapeCsv(issue.Location)}");
            }

            await File.WriteAllTextAsync(outputPath, csv.ToString());

            Logger.Log(LogLevel.Info, $"Rapport CSV généré: {outputPath}");
            return outputPath;
        }

        /// <summary>
        /// Génère un rapport texte simple
        /// </summary>
        public async Task<string> GenerateTextReportAsync(AuditReport report, string? outputPath = null)
        {
            outputPath ??= Path.Combine(_reportsDirectory, $"audit-report-{DateTime.Now:yyyyMMdd-HHmmss}.txt");

            var text = new StringBuilder();
            text.AppendLine("═══════════════════════════════════════════════════════════");
            text.AppendLine($"  RAPPORT D'AUDIT - {report.ComputerName}");
            text.AppendLine("═══════════════════════════════════════════════════════════");
            text.AppendLine();
            text.AppendLine($"Date: {report.Timestamp:dd/MM/yyyy HH:mm:ss}");
            text.AppendLine($"Type: {report.AuditType}");
            text.AppendLine($"Durée: {report.Duration.TotalSeconds:F2} secondes");
            text.AppendLine($"Score de santé: {report.HealthScore}/100 ({GetHealthLevel(report.HealthScore)})");
            text.AppendLine();

            // Résumé des catégories
            text.AppendLine("───────────────────────────────────────────────────────────");
            text.AppendLine("CATÉGORIES ANALYSÉES");
            text.AppendLine("───────────────────────────────────────────────────────────");
            foreach (var category in report.Categories.OrderByDescending(c => c.Weight))
            {
                var statusIcon = GetStatusIcon(category.Status);
                text.AppendLine($"{category.Icon} {category.DisplayName,-30} {statusIcon} {category.Score}/100");
            }
            text.AppendLine();

            // Problèmes détectés
            if (report.Issues.Any())
            {
                text.AppendLine("───────────────────────────────────────────────────────────");
                text.AppendLine($"PROBLÈMES DÉTECTÉS ({report.Issues.Count})");
                text.AppendLine("───────────────────────────────────────────────────────────");

                var groupedIssues = report.Issues.GroupBy(i => i.Severity).OrderByDescending(g => g.Key);
                foreach (var group in groupedIssues)
                {
                    text.AppendLine();
                    text.AppendLine($"[{group.Key}] - {group.Count()} problème(s)");
                    text.AppendLine();

                    foreach (var issue in group)
                    {
                        text.AppendLine($"  • {issue.Title}");
                        text.AppendLine($"    {issue.Description}");
                        if (!string.IsNullOrEmpty(issue.Impact))
                            text.AppendLine($"    Impact: {issue.Impact}");
                        if (issue.AutoFixAvailable)
                            text.AppendLine($"    ✓ Correction automatique disponible");
                        text.AppendLine();
                    }
                }
            }

            // Recommandations
            if (report.Recommendations.SuggestedCleanings.Any())
            {
                text.AppendLine("───────────────────────────────────────────────────────────");
                text.AppendLine("RECOMMANDATIONS");
                text.AppendLine("───────────────────────────────────────────────────────────");
                text.AppendLine();

                if (report.Recommendations.PotentialSpaceSavings > 0)
                {
                    text.AppendLine($"💾 Espace récupérable: {FormatBytes(report.Recommendations.PotentialSpaceSavings)}");
                    text.AppendLine();
                }

                text.AppendLine("Actions de nettoyage suggérées:");
                foreach (var action in report.Recommendations.SuggestedCleanings.OrderByDescending(a => a.Priority))
                {
                    text.AppendLine($"  {action.Priority}. {action.Description}");
                    if (action.SpaceSavings > 0)
                        text.AppendLine($"     Économie: {FormatBytes(action.SpaceSavings)}");
                }
            }

            text.AppendLine();
            text.AppendLine("═══════════════════════════════════════════════════════════");
            text.AppendLine($"Rapport généré par Windows Cleaner v{report.Version}");
            text.AppendLine("═══════════════════════════════════════════════════════════");

            await File.WriteAllTextAsync(outputPath, text.ToString());

            Logger.Log(LogLevel.Info, $"Rapport texte généré: {outputPath}");
            return outputPath;
        }

        private string GenerateHtmlContent(AuditReport report)
        {
            var healthLevel = GetHealthLevel(report.HealthScore);
            var healthColor = report.HealthScore switch
            {
                >= 90 => "#28a745",
                >= 70 => "#17a2b8",
                >= 50 => "#ffc107",
                >= 30 => "#fd7e14",
                _ => "#dc3545"
            };

            return $@"<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Rapport d'Audit - {report.ComputerName}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            color: #333;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 15px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        .header h1 {{
            font-size: 2.5em;
            margin-bottom: 10px;
        }}
        .header .subtitle {{
            opacity: 0.9;
            font-size: 1.1em;
        }}
        .health-score {{
            background: white;
            margin: 30px auto;
            padding: 30px;
            border-radius: 10px;
            width: 200px;
            height: 200px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
        }}
        .score-circle {{
            font-size: 4em;
            font-weight: bold;
            color: {healthColor};
        }}
        .score-label {{
            color: #666;
            margin-top: 10px;
        }}
        .content {{
            padding: 40px;
        }}
        .section {{
            margin-bottom: 40px;
        }}
        .section-title {{
            font-size: 1.8em;
            color: #667eea;
            margin-bottom: 20px;
            border-bottom: 3px solid #667eea;
            padding-bottom: 10px;
        }}
        .stats-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }}
        .stat-card {{
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            border-left: 4px solid #667eea;
        }}
        .stat-label {{
            color: #666;
            font-size: 0.9em;
            margin-bottom: 5px;
        }}
        .stat-value {{
            font-size: 1.5em;
            font-weight: bold;
            color: #333;
        }}
        .categories {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }}
        .category-card {{
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            transition: transform 0.3s;
        }}
        .category-card:hover {{
            transform: translateY(-5px);
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
        }}
        .category-header {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }}
        .category-name {{
            font-size: 1.3em;
            font-weight: 600;
        }}
        .category-score {{
            font-size: 1.5em;
            font-weight: bold;
            padding: 5px 15px;
            border-radius: 20px;
            background: #e9ecef;
        }}
        .issue {{
            background: white;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 15px;
            border-left: 4px solid #ffc107;
        }}
        .issue.critical {{ border-color: #dc3545; }}
        .issue.high {{ border-color: #fd7e14; }}
        .issue.medium {{ border-color: #ffc107; }}
        .issue.low {{ border-color: #17a2b8; }}
        .issue-title {{
            font-size: 1.2em;
            font-weight: 600;
            margin-bottom: 10px;
        }}
        .issue-description {{
            color: #666;
            margin-bottom: 10px;
        }}
        .issue-meta {{
            display: flex;
            gap: 15px;
            font-size: 0.9em;
            color: #999;
        }}
        .badge {{
            display: inline-block;
            padding: 5px 12px;
            border-radius: 15px;
            font-size: 0.85em;
            font-weight: 600;
        }}
        .badge-critical {{ background: #dc3545; color: white; }}
        .badge-high {{ background: #fd7e14; color: white; }}
        .badge-medium {{ background: #ffc107; color: #333; }}
        .badge-low {{ background: #17a2b8; color: white; }}
        .badge-info {{ background: #6c757d; color: white; }}
        .recommendations {{
            background: #e7f3ff;
            padding: 25px;
            border-radius: 10px;
            border-left: 4px solid #007bff;
        }}
        .recommendation-item {{
            padding: 10px 0;
            border-bottom: 1px solid #d0e8ff;
        }}
        .recommendation-item:last-child {{ border-bottom: none; }}
        .footer {{
            background: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            color: #666;
            font-size: 0.9em;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔍 Rapport d'Audit Système</h1>
            <div class='subtitle'>{report.ComputerName} - {report.Timestamp:dd/MM/yyyy HH:mm:ss}</div>
            <div class='health-score'>
                <div class='score-circle'>{report.HealthScore}</div>
                <div class='score-label'>{healthLevel}</div>
            </div>
        </div>

        <div class='content'>
            <div class='stats-grid'>
                <div class='stat-card'>
                    <div class='stat-label'>Durée de l'audit</div>
                    <div class='stat-value'>{report.Duration.TotalSeconds:F1}s</div>
                </div>
                <div class='stat-card'>
                    <div class='stat-label'>Problèmes détectés</div>
                    <div class='stat-value'>{report.Issues.Count}</div>
                </div>
                <div class='stat-card'>
                    <div class='stat-label'>Catégories analysées</div>
                    <div class='stat-value'>{report.Categories.Count}</div>
                </div>
                <div class='stat-card'>
                    <div class='stat-label'>Espace récupérable</div>
                    <div class='stat-value'>{FormatBytes(report.Recommendations.PotentialSpaceSavings)}</div>
                </div>
            </div>

            <div class='section'>
                <h2 class='section-title'>📊 Catégories Analysées</h2>
                <div class='categories'>
                    {GenerateCategoriesHtml(report.Categories)}
                </div>
            </div>

            {(report.Issues.Any() ? $@"
            <div class='section'>
                <h2 class='section-title'>⚠️ Problèmes Détectés</h2>
                {GenerateIssuesHtml(report.Issues)}
            </div>" : "")}

            {(report.Recommendations.SuggestedCleanings.Any() ? $@"
            <div class='section'>
                <h2 class='section-title'>💡 Recommandations</h2>
                <div class='recommendations'>
                    <h3>Actions de nettoyage suggérées:</h3>
                    {GenerateRecommendationsHtml(report.Recommendations)}
                </div>
            </div>" : "")}
        </div>

        <div class='footer'>
            <p>Rapport généré par Windows Cleaner v{report.Version}</p>
            <p>ID: {report.Id}</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCategoriesHtml(List<AuditCategory> categories)
        {
            var html = new StringBuilder();
            foreach (var category in categories.OrderByDescending(c => c.Weight))
            {
                var statusEmoji = category.Status switch
                {
                    CategoryStatus.Excellent => "✅",
                    CategoryStatus.Good => "👍",
                    CategoryStatus.Warning => "⚠️",
                    CategoryStatus.Critical => "🚨",
                    _ => "❓"
                };

                html.AppendLine($@"
                <div class='category-card'>
                    <div class='category-header'>
                        <div class='category-name'>{category.Icon} {category.DisplayName}</div>
                        <div class='category-score'>{category.Score}</div>
                    </div>
                    <div>{statusEmoji} {category.Status}</div>
                    <div style='margin-top: 10px; color: #666; font-size: 0.9em;'>
                        {category.Checks.Count} vérification(s) • {category.ExecutionTime.TotalSeconds:F2}s
                    </div>
                </div>");
            }
            return html.ToString();
        }

        private string GenerateIssuesHtml(List<AuditIssue> issues)
        {
            var html = new StringBuilder();
            foreach (var issue in issues.OrderByDescending(i => i.Severity))
            {
                var severityClass = issue.Severity.ToString().ToLower();
                var severityBadge = $"<span class='badge badge-{severityClass}'>{issue.Severity}</span>";

                html.AppendLine($@"
                <div class='issue {severityClass}'>
                    <div class='issue-title'>{issue.Icon} {issue.Title} {severityBadge}</div>
                    <div class='issue-description'>{issue.Description}</div>
                    {(!string.IsNullOrEmpty(issue.Impact) ? $"<div style='color: #666; margin-bottom: 10px;'><strong>Impact:</strong> {issue.Impact}</div>" : "")}
                    <div class='issue-meta'>
                        <span>📁 {issue.Location}</span>
                        {(issue.AutoFixAvailable ? "<span style='color: #28a745;'>✓ Correction auto</span>" : "")}
                    </div>
                </div>");
            }
            return html.ToString();
        }

        private string GenerateRecommendationsHtml(AuditRecommendations recommendations)
        {
            var html = new StringBuilder();
            foreach (var action in recommendations.SuggestedCleanings.OrderByDescending(a => a.Priority))
            {
                html.AppendLine($@"
                <div class='recommendation-item'>
                    <strong>Priorité {action.Priority}:</strong> {action.Description}
                    {(action.SpaceSavings > 0 ? $"<span style='color: #007bff;'> (Économie: {FormatBytes(action.SpaceSavings)})</span>" : "")}
                </div>");
            }
            return html.ToString();
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
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

        private string GetStatusIcon(CategoryStatus status)
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

        private static string FormatBytes(long bytes)
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
