using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Interface for K8s distribution licensing calculations.
/// Handles subscription, support, and licensing costs for various distributions.
/// </summary>
public interface IDistributionLicensing
{
    /// <summary>
    /// The distribution this licensing applies to
    /// </summary>
    Distribution Distribution { get; }

    /// <summary>
    /// Whether this distribution requires paid licensing
    /// </summary>
    bool RequiresLicense { get; }

    /// <summary>
    /// Display name for the distribution
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Vendor/Provider name (e.g., "Red Hat", "VMware", "SUSE")
    /// </summary>
    string Vendor { get; }

    /// <summary>
    /// Get the license cost per node per year
    /// </summary>
    decimal GetLicenseCostPerNodeYear();

    /// <summary>
    /// Get the license cost per core per year (for per-core licensing)
    /// </summary>
    decimal GetLicenseCostPerCoreYear();

    /// <summary>
    /// Get the license cost per socket per year (for per-socket licensing)
    /// </summary>
    decimal GetLicenseCostPerSocketYear();

    /// <summary>
    /// Get the support tier options available
    /// </summary>
    IReadOnlyList<SupportTierInfo> GetSupportTiers();

    /// <summary>
    /// Calculate total licensing cost for a given configuration
    /// </summary>
    LicensingCost CalculateLicensingCost(LicensingInput input);

    /// <summary>
    /// Get any cluster-level fixed costs (e.g., control plane licensing)
    /// </summary>
    decimal GetClusterFixedCostPerYear();

    /// <summary>
    /// Get worker node OpenShift/distribution fee per hour (for cloud variants)
    /// </summary>
    decimal GetWorkerNodeFeePerHour();
}

/// <summary>
/// Input parameters for licensing cost calculation
/// </summary>
public class LicensingInput
{
    /// <summary>
    /// Number of nodes in the cluster
    /// </summary>
    public int NodeCount { get; set; }

    /// <summary>
    /// Total CPU cores across all nodes
    /// </summary>
    public int TotalCores { get; set; }

    /// <summary>
    /// Total sockets (for socket-based licensing)
    /// </summary>
    public int TotalSockets { get; set; }

    /// <summary>
    /// Number of master/control plane nodes
    /// </summary>
    public int MasterNodeCount { get; set; }

    /// <summary>
    /// Number of worker nodes
    /// </summary>
    public int WorkerNodeCount { get; set; }

    /// <summary>
    /// Number of infrastructure nodes (if applicable)
    /// </summary>
    public int InfraNodeCount { get; set; }

    /// <summary>
    /// Selected support tier
    /// </summary>
    public SupportTier SupportTier { get; set; } = SupportTier.Standard;

    /// <summary>
    /// Contract term in years (affects discounts)
    /// </summary>
    public int ContractYears { get; set; } = 1;

    /// <summary>
    /// Whether running as managed service (ROSA, ARO, etc.)
    /// </summary>
    public bool IsManagedService { get; set; }
}

/// <summary>
/// Calculated licensing cost breakdown
/// </summary>
public class LicensingCost
{
    /// <summary>
    /// Base license cost per year
    /// </summary>
    public decimal BaseLicensePerYear { get; set; }

    /// <summary>
    /// Support cost per year
    /// </summary>
    public decimal SupportCostPerYear { get; set; }

    /// <summary>
    /// Any additional fees per year
    /// </summary>
    public decimal AdditionalFeesPerYear { get; set; }

    /// <summary>
    /// Total cost per year
    /// </summary>
    public decimal TotalPerYear => BaseLicensePerYear + SupportCostPerYear + AdditionalFeesPerYear;

    /// <summary>
    /// Total cost per month
    /// </summary>
    public decimal TotalPerMonth => TotalPerYear / 12;

    /// <summary>
    /// Per-node effective cost per year
    /// </summary>
    public decimal PerNodePerYear { get; set; }

    /// <summary>
    /// Discount applied (percentage)
    /// </summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>
    /// Description of licensing model used
    /// </summary>
    public string LicensingModel { get; set; } = string.Empty;
}

/// <summary>
/// Support tier levels
/// </summary>
public enum SupportTier
{
    /// <summary>Community/self-support only</summary>
    Community,

    /// <summary>Basic support (business hours)</summary>
    Basic,

    /// <summary>Standard support (extended hours)</summary>
    Standard,

    /// <summary>Premium support (24x7)</summary>
    Premium,

    /// <summary>Enterprise support (dedicated TAM)</summary>
    Enterprise
}

/// <summary>
/// Support tier information
/// </summary>
public class SupportTierInfo
{
    /// <summary>
    /// Support tier level
    /// </summary>
    public SupportTier Tier { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Support hours description
    /// </summary>
    public string Hours { get; set; } = string.Empty;

    /// <summary>
    /// Response time SLA
    /// </summary>
    public string ResponseSLA { get; set; } = string.Empty;

    /// <summary>
    /// Cost multiplier (1.0 = base price)
    /// </summary>
    public decimal CostMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Additional annual cost (flat fee)
    /// </summary>
    public decimal AdditionalAnnualCost { get; set; }

    /// <summary>
    /// Whether this includes a dedicated TAM
    /// </summary>
    public bool IncludesTAM { get; set; }
}
