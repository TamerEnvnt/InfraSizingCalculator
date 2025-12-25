using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Complete cost estimate result
/// </summary>
public class CostEstimate
{
    /// <summary>
    /// Provider used for this estimate
    /// </summary>
    public CloudProvider Provider { get; set; }

    /// <summary>
    /// Region for pricing
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Pricing type used
    /// </summary>
    public PricingType PricingType { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    public Currency Currency { get; set; } = Currency.USD;

    /// <summary>
    /// Total monthly cost
    /// </summary>
    public decimal MonthlyTotal { get; set; }

    /// <summary>
    /// Total yearly cost
    /// </summary>
    public decimal YearlyTotal => MonthlyTotal * 12;

    /// <summary>
    /// 3-year TCO (Total Cost of Ownership)
    /// </summary>
    public decimal ThreeYearTCO => YearlyTotal * 3;

    /// <summary>
    /// 5-year TCO
    /// </summary>
    public decimal FiveYearTCO => YearlyTotal * 5;

    /// <summary>
    /// Cost breakdown by category
    /// </summary>
    public Dictionary<CostCategory, CostBreakdown> Breakdown { get; set; } = new();

    /// <summary>
    /// Per-environment cost breakdown
    /// </summary>
    public Dictionary<EnvironmentType, EnvironmentCost> EnvironmentCosts { get; set; } = new();

    /// <summary>
    /// When this estimate was calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Pricing source information
    /// </summary>
    public string PricingSource { get; set; } = string.Empty;

    /// <summary>
    /// Notes or assumptions
    /// </summary>
    public List<string> Notes { get; set; } = new();

    /// <summary>
    /// Get compute cost
    /// </summary>
    public decimal ComputeCost => Breakdown.GetValueOrDefault(CostCategory.Compute)?.Monthly ?? 0;

    /// <summary>
    /// Get storage cost
    /// </summary>
    public decimal StorageCost => Breakdown.GetValueOrDefault(CostCategory.Storage)?.Monthly ?? 0;

    /// <summary>
    /// Get network cost
    /// </summary>
    public decimal NetworkCost => Breakdown.GetValueOrDefault(CostCategory.Network)?.Monthly ?? 0;

    /// <summary>
    /// Get license cost
    /// </summary>
    public decimal LicenseCost => Breakdown.GetValueOrDefault(CostCategory.License)?.Monthly ?? 0;

    /// <summary>
    /// Get support cost
    /// </summary>
    public decimal SupportCost => Breakdown.GetValueOrDefault(CostCategory.Support)?.Monthly ?? 0;
}

/// <summary>
/// Cost breakdown for a specific category
/// </summary>
public class CostBreakdown
{
    /// <summary>
    /// Category name
    /// </summary>
    public CostCategory Category { get; set; }

    /// <summary>
    /// Monthly cost
    /// </summary>
    public decimal Monthly { get; set; }

    /// <summary>
    /// Yearly cost
    /// </summary>
    public decimal Yearly => Monthly * 12;

    /// <summary>
    /// Percentage of total
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Line items making up this cost
    /// </summary>
    public List<CostLineItem> LineItems { get; set; } = new();

    /// <summary>
    /// Description of this category
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Individual line item in cost breakdown
/// </summary>
public class CostLineItem
{
    /// <summary>
    /// Description of the line item
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit (e.g., "vCPU-hours", "GB-months", "nodes")
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total for this line item
    /// </summary>
    public decimal Total => Quantity * UnitPrice;

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Cost for a specific environment
/// </summary>
public class EnvironmentCost
{
    /// <summary>
    /// Environment type
    /// </summary>
    public EnvironmentType Environment { get; set; }

    /// <summary>
    /// Environment display name
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// Monthly cost for this environment
    /// </summary>
    public decimal MonthlyCost { get; set; }

    /// <summary>
    /// Percentage of total cost
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Resource details
    /// </summary>
    public int Nodes { get; set; }
    public int TotalCpu { get; set; }
    public int TotalRamGB { get; set; }
    public int TotalDiskGB { get; set; }

    /// <summary>
    /// Cost per node in this environment
    /// </summary>
    public decimal CostPerNode => Nodes > 0 ? MonthlyCost / Nodes : 0;
}

/// <summary>
/// Cost comparison between multiple estimates
/// </summary>
public class CostComparison
{
    /// <summary>
    /// Estimates being compared
    /// </summary>
    public List<CostEstimate> Estimates { get; set; } = new();

    /// <summary>
    /// Cheapest option
    /// </summary>
    public CostEstimate? CheapestOption { get; set; }

    /// <summary>
    /// Most expensive option
    /// </summary>
    public CostEstimate? MostExpensiveOption { get; set; }

    /// <summary>
    /// Savings by switching to cheapest option
    /// </summary>
    public Dictionary<string, decimal> PotentialSavings { get; set; } = new();

    /// <summary>
    /// Comparison insights
    /// </summary>
    public List<string> Insights { get; set; } = new();

    /// <summary>
    /// When this comparison was made
    /// </summary>
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
}
