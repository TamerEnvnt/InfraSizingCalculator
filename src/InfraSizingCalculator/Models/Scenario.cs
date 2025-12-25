using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Represents a saved sizing scenario with all inputs, results, and cost estimates
/// </summary>
public class Scenario
{
    /// <summary>
    /// Unique identifier for the scenario
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-provided name for the scenario
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the scenario
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the scenario was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the scenario was last modified
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Type of scenario: "k8s" or "vm"
    /// </summary>
    public string Type { get; set; } = "k8s";

    /// <summary>
    /// K8s sizing input (if type is k8s)
    /// </summary>
    public K8sSizingInput? K8sInput { get; set; }

    /// <summary>
    /// VM sizing input (if type is vm)
    /// </summary>
    public VMSizingInput? VMInput { get; set; }

    /// <summary>
    /// K8s sizing result (if type is k8s)
    /// </summary>
    public K8sSizingResult? K8sResult { get; set; }

    /// <summary>
    /// VM sizing result (if type is vm)
    /// </summary>
    public VMSizingResult? VMResult { get; set; }

    /// <summary>
    /// Cost estimate for the scenario
    /// </summary>
    public CostEstimate? CostEstimate { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Whether this scenario is marked as favorite
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// Whether this scenario is a draft (not yet finalized)
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Version for future compatibility
    /// </summary>
    public int Version { get; set; } = 1;

    // Computed properties for quick access
    public int TotalNodes => K8sResult?.GrandTotal.TotalNodes ?? VMResult?.GrandTotal.TotalVMs ?? 0;
    public int TotalCpu => K8sResult?.GrandTotal.TotalCpu ?? VMResult?.GrandTotal.TotalCpu ?? 0;
    public double TotalRam => K8sResult?.GrandTotal.TotalRam ?? VMResult?.GrandTotal.TotalRam ?? 0;
    public decimal MonthlyEstimate => CostEstimate?.MonthlyTotal ?? 0;
    public string DistributionOrTechnology => K8sInput?.Distribution.ToString() ?? VMInput?.Technology.ToString() ?? "Unknown";
}

/// <summary>
/// Result of comparing multiple scenarios
/// </summary>
public class ScenarioComparison
{
    /// <summary>
    /// Scenarios being compared
    /// </summary>
    public Scenario[] Scenarios { get; set; } = Array.Empty<Scenario>();

    /// <summary>
    /// Comparison metrics for each scenario
    /// </summary>
    public List<ComparisonMetric> Metrics { get; set; } = new();

    /// <summary>
    /// ID of the recommended scenario (lowest cost with acceptable specs)
    /// </summary>
    public Guid? RecommendedScenarioId { get; set; }

    /// <summary>
    /// Reason for recommendation
    /// </summary>
    public string? RecommendationReason { get; set; }

    /// <summary>
    /// Insights derived from comparison
    /// </summary>
    public List<string> Insights { get; set; } = new();

    /// <summary>
    /// When comparison was performed
    /// </summary>
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A single comparison metric across scenarios
/// </summary>
public class ComparisonMetric
{
    /// <summary>
    /// Name of the metric (e.g., "Total Nodes", "Monthly Cost")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measurement (e.g., "nodes", "$", "GB")
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Category for grouping (e.g., "Resources", "Cost", "Infrastructure")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Values for each scenario (keyed by scenario ID)
    /// </summary>
    public Dictionary<Guid, decimal> Values { get; set; } = new();

    /// <summary>
    /// ID of scenario with best value for this metric
    /// </summary>
    public Guid? WinnerId { get; set; }

    /// <summary>
    /// Whether lower is better for this metric (true for cost, false for capacity)
    /// </summary>
    public bool LowerIsBetter { get; set; } = true;

    /// <summary>
    /// Percentage difference from best to worst
    /// </summary>
    public decimal DeltaPercentage { get; set; }
}

/// <summary>
/// Summary of a scenario for list displays
/// </summary>
public class ScenarioSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DistributionOrTechnology { get; set; } = string.Empty;
    public int TotalNodes { get; set; }
    public int TotalCpu { get; set; }
    public decimal MonthlyEstimate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsDraft { get; set; }
    public List<string> Tags { get; set; } = new();
}
