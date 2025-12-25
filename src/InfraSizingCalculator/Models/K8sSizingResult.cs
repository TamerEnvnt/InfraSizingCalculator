using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Complete K8s sizing calculation result
/// BR-EX002: Exports shall include all environment results and grand totals
/// BR-EX003: Exports shall include configuration metadata
/// </summary>
public class K8sSizingResult
{
    public required List<EnvironmentResult> Environments { get; init; }
    public required GrandTotal GrandTotal { get; init; }

    /// <summary>
    /// Configuration metadata for export (BR-EX003)
    /// </summary>
    public required K8sSizingInput Configuration { get; init; }

    /// <summary>
    /// Distribution name for display
    /// </summary>
    public string DistributionName { get; init; } = string.Empty;

    /// <summary>
    /// Technology name for display
    /// </summary>
    public string TechnologyName { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp for export (BR-EX001)
    /// </summary>
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Node specifications used for calculation
    /// </summary>
    public DistributionConfig? NodeSpecs { get; init; }
}
