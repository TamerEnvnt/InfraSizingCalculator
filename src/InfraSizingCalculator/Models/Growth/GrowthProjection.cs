using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Growth;

/// <summary>
/// Complete growth projection result
/// </summary>
public class GrowthProjection
{
    /// <summary>
    /// When the projection was calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Baseline date (Year 0)
    /// </summary>
    public DateTime BaselineDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Settings used for this projection
    /// </summary>
    public GrowthSettings Settings { get; set; } = new();

    /// <summary>
    /// Baseline (current) metrics
    /// </summary>
    public ProjectionPoint Baseline { get; set; } = new();

    /// <summary>
    /// Projected points for each year
    /// </summary>
    public List<ProjectionPoint> Points { get; set; } = new();

    /// <summary>
    /// Warnings about approaching limits
    /// </summary>
    public List<ClusterLimitWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Scaling recommendations based on projections
    /// </summary>
    public List<ScalingRecommendation> Recommendations { get; set; } = new();

    /// <summary>
    /// Summary statistics
    /// </summary>
    public ProjectionSummary Summary { get; set; } = new();
}

/// <summary>
/// A single point in the growth projection timeline
/// </summary>
public class ProjectionPoint
{
    /// <summary>
    /// Year number (0 = baseline/current, 1 = year 1, etc.)
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Label for this point (e.g., "Year 1", "2026")
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Projected total applications
    /// </summary>
    public int ProjectedApps { get; set; }

    /// <summary>
    /// Projected total CPU cores required
    /// </summary>
    public int ProjectedCpu { get; set; }

    /// <summary>
    /// Projected total RAM in GB
    /// </summary>
    public int ProjectedRamGB { get; set; }

    /// <summary>
    /// Projected total storage in GB
    /// </summary>
    public int ProjectedStorageGB { get; set; }

    /// <summary>
    /// Projected total nodes
    /// </summary>
    public int ProjectedNodes { get; set; }

    /// <summary>
    /// Projected worker nodes
    /// </summary>
    public int ProjectedWorkerNodes { get; set; }

    /// <summary>
    /// Projected monthly cost
    /// </summary>
    public decimal ProjectedMonthlyCost { get; set; }

    /// <summary>
    /// Projected yearly cost
    /// </summary>
    public decimal ProjectedYearlyCost { get; set; }

    /// <summary>
    /// Growth percentage from previous year
    /// </summary>
    public double GrowthFromPrevious { get; set; }

    /// <summary>
    /// Cumulative growth from baseline
    /// </summary>
    public double CumulativeGrowth { get; set; }

    /// <summary>
    /// Per-environment breakdown
    /// </summary>
    public Dictionary<EnvironmentType, EnvironmentProjection> EnvironmentBreakdown { get; set; } = new();
}

/// <summary>
/// Projection for a specific environment
/// </summary>
public class EnvironmentProjection
{
    public EnvironmentType Environment { get; set; }
    public int Apps { get; set; }
    public int Nodes { get; set; }
    public int Cpu { get; set; }
    public int RamGB { get; set; }
    public decimal MonthlyCost { get; set; }
}

/// <summary>
/// Warning when approaching cluster limits
/// </summary>
public class ClusterLimitWarning
{
    /// <summary>
    /// Type of warning
    /// </summary>
    public WarningType Type { get; set; }

    /// <summary>
    /// Severity level
    /// </summary>
    public WarningSeverity Severity { get; set; }

    /// <summary>
    /// Year when the limit will be reached/exceeded
    /// </summary>
    public int YearTriggered { get; set; }

    /// <summary>
    /// Human-readable message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Current value
    /// </summary>
    public double CurrentValue { get; set; }

    /// <summary>
    /// Projected value when warning triggers
    /// </summary>
    public double ProjectedValue { get; set; }

    /// <summary>
    /// Limit that will be exceeded
    /// </summary>
    public double Limit { get; set; }

    /// <summary>
    /// Percentage of limit reached
    /// </summary>
    public double PercentageOfLimit { get; set; }

    /// <summary>
    /// Resource type (nodes, pods, etc.)
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;
}

public enum WarningType
{
    NodeLimit,
    PodLimit,
    CpuCapacity,
    MemoryCapacity,
    StorageCapacity,
    CostThreshold,
    ClusterSplit
}

public enum WarningSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>
/// Recommendation for scaling strategy
/// </summary>
public class ScalingRecommendation
{
    /// <summary>
    /// Type of recommendation
    /// </summary>
    public RecommendationType Type { get; set; }

    /// <summary>
    /// Year when this action should be considered
    /// </summary>
    public int RecommendedYear { get; set; }

    /// <summary>
    /// Priority (1 = highest)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Human-readable title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Estimated cost impact (positive = increase, negative = savings)
    /// </summary>
    public decimal EstimatedCostImpact { get; set; }

    /// <summary>
    /// Icon for UI display
    /// </summary>
    public string Icon { get; set; } = "💡";
}

public enum RecommendationType
{
    UpgradeNodeSize,
    AddWorkerNodes,
    SplitCluster,
    AddCluster,
    EnableAutoscaling,
    OptimizeResources,
    ReviewOverprovisioning,
    ConsiderManagedService
}

/// <summary>
/// Summary statistics for the projection
/// </summary>
public class ProjectionSummary
{
    /// <summary>
    /// Total growth in apps over projection period
    /// </summary>
    public int TotalAppGrowth { get; set; }

    /// <summary>
    /// Percentage growth in apps
    /// </summary>
    public double PercentageAppGrowth { get; set; }

    /// <summary>
    /// Total growth in nodes
    /// </summary>
    public int TotalNodeGrowth { get; set; }

    /// <summary>
    /// Percentage growth in nodes
    /// </summary>
    public double PercentageNodeGrowth { get; set; }

    /// <summary>
    /// Total cost over projection period
    /// </summary>
    public decimal TotalCostOverPeriod { get; set; }

    /// <summary>
    /// Average yearly cost
    /// </summary>
    public decimal AverageYearlyCost { get; set; }

    /// <summary>
    /// Cost increase from year 1 to final year
    /// </summary>
    public decimal CostIncrease { get; set; }

    /// <summary>
    /// Percentage cost increase
    /// </summary>
    public double PercentageCostIncrease { get; set; }

    /// <summary>
    /// Year when major scaling is needed
    /// </summary>
    public int? MajorScalingYear { get; set; }

    /// <summary>
    /// Number of warnings generated
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// Number of critical warnings
    /// </summary>
    public int CriticalWarningCount { get; set; }
}
