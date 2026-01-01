using System;
using System.Collections.Generic;

namespace WindowsCleaner.Core
{
    /// <summary>
    /// Représente un rapport d'audit complet du système
    /// </summary>
    public class AuditReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string AuditType { get; set; } = "Full"; // Full, Quick, Scheduled, Manual
        public TimeSpan Duration { get; set; }
        public int HealthScore { get; set; } // 0-100
        public List<AuditCategory> Categories { get; set; } = new();
        public List<AuditIssue> Issues { get; set; } = new();
        public Dictionary<string, object> Metrics { get; set; } = new();
        public AuditRecommendations Recommendations { get; set; } = new();
        public string ComputerName { get; set; } = Environment.MachineName;
        public string UserName { get; set; } = Environment.UserName;
        public string Version { get; set; } = "1.0.0";
    }

    /// <summary>
    /// Catégorie d'audit (Disque, Registre, etc.)
    /// </summary>
    public class AuditCategory
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Score { get; set; } // 0-100
        public double Weight { get; set; } // Poids pour le calcul du score global
        public List<AuditCheck> Checks { get; set; } = new();
        public CategoryStatus Status { get; set; } = CategoryStatus.Unknown;
        public TimeSpan ExecutionTime { get; set; }
        public string Icon { get; set; } = "📊";
    }

    /// <summary>
    /// Vérification individuelle dans une catégorie
    /// </summary>
    public class AuditCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CheckStatus Status { get; set; } = CheckStatus.Unknown;
        public string Result { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
        public object ExpectedValue { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Recommandations basées sur l'audit
    /// </summary>
    public class AuditRecommendations
    {
        public List<CleaningAction> SuggestedCleanings { get; set; } = new();
        public List<OptimizationAction> SuggestedOptimizations { get; set; } = new();
        public long PotentialSpaceSavings { get; set; } // En bytes
        public int EstimatedPerformanceGain { get; set; } // Pourcentage
        public List<string> QuickWins { get; set; } = new(); // Actions rapides à fort impact
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Action de nettoyage recommandée
    /// </summary>
    public class CleaningAction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long SpaceSavings { get; set; }
        public int Priority { get; set; } // 1-5
        public bool IsSafe { get; set; } = true;
        public bool RequiresBackup { get; set; } = false;
        public List<string> AffectedItems { get; set; } = new();
    }

    /// <summary>
    /// Action d'optimisation recommandée
    /// </summary>
    public class OptimizationAction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ImpactLevel { get; set; } // 1-5
        public string ExpectedBenefit { get; set; } = string.Empty;
        public bool RequiresRestart { get; set; } = false;
        public List<string> Steps { get; set; } = new();
    }

    /// <summary>
    /// Statut d'une catégorie d'audit
    /// </summary>
    public enum CategoryStatus
    {
        Unknown,
        Excellent,
        Good,
        Warning,
        Critical,
        Error
    }

    /// <summary>
    /// Statut d'une vérification
    /// </summary>
    public enum CheckStatus
    {
        Unknown,
        Passed,
        Warning,
        Failed,
        Skipped,
        Error
    }

    /// <summary>
    /// Résumé d'audit pour affichage rapide
    /// </summary>
    public class AuditSummary
    {
        public int HealthScore { get; set; }
        public string HealthLevel { get; set; } = string.Empty; // Excellent, Good, Warning, Critical
        public int TotalIssues { get; set; }
        public int CriticalIssues { get; set; }
        public int WarningIssues { get; set; }
        public long PotentialSpaceSavings { get; set; }
        public DateTime LastAuditDate { get; set; }
        public string QuickSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comparaison entre deux audits
    /// </summary>
    public class AuditComparison
    {
        public Guid Audit1Id { get; set; }
        public Guid Audit2Id { get; set; }
        public DateTime Date1 { get; set; }
        public DateTime Date2 { get; set; }
        public int HealthScoreDelta { get; set; }
        public Dictionary<string, CategoryComparison> CategoryChanges { get; set; } = new();
        public List<string> NewIssues { get; set; } = new();
        public List<string> ResolvedIssues { get; set; } = new();
        public string OverallTrend { get; set; } = string.Empty; // Improving, Declining, Stable
    }

    /// <summary>
    /// Comparaison d'une catégorie entre deux audits
    /// </summary>
    public class CategoryComparison
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ScoreDelta { get; set; }
        public string Trend { get; set; } = string.Empty;
        public List<string> Changes { get; set; } = new();
    }
}
