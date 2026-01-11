using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Abstract base class for K8s distribution licensing.
/// Provides common functionality and default implementations.
/// Follows Template Method Pattern.
/// </summary>
public abstract class DistributionLicensingBase : IDistributionLicensing
{
    /// <inheritdoc />
    public abstract Distribution Distribution { get; }

    /// <inheritdoc />
    public abstract bool RequiresLicense { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract string Vendor { get; }

    /// <summary>
    /// Primary licensing model (per-node, per-core, per-socket)
    /// </summary>
    protected abstract LicensingModel PrimaryLicensingModel { get; }

    /// <inheritdoc />
    public virtual decimal GetLicenseCostPerNodeYear() => 0m;

    /// <inheritdoc />
    public virtual decimal GetLicenseCostPerCoreYear() => 0m;

    /// <inheritdoc />
    public virtual decimal GetLicenseCostPerSocketYear() => 0m;

    /// <inheritdoc />
    public virtual decimal GetClusterFixedCostPerYear() => 0m;

    /// <inheritdoc />
    public virtual decimal GetWorkerNodeFeePerHour() => 0m;

    /// <inheritdoc />
    public virtual IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Standard,
                Name = "Standard",
                Hours = "Business hours",
                ResponseSLA = "4 hours",
                CostMultiplier = 1.0m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Premium",
                Hours = "24x7",
                ResponseSLA = "1 hour",
                CostMultiplier = 1.5m
            }
        };
    }

    /// <inheritdoc />
    public virtual LicensingCost CalculateLicensingCost(LicensingInput input)
    {
        if (!RequiresLicense)
        {
            return new LicensingCost
            {
                BaseLicensePerYear = 0m,
                SupportCostPerYear = 0m,
                AdditionalFeesPerYear = 0m,
                PerNodePerYear = 0m,
                DiscountPercent = 0m,
                LicensingModel = "Open Source - No License Required"
            };
        }

        // Calculate base license cost based on model
        var baseCost = PrimaryLicensingModel switch
        {
            LicensingModel.PerNode => input.NodeCount * GetLicenseCostPerNodeYear(),
            LicensingModel.PerCore => input.TotalCores * GetLicenseCostPerCoreYear(),
            LicensingModel.PerSocket => input.TotalSockets * GetLicenseCostPerSocketYear(),
            LicensingModel.PerWorkerNode => input.WorkerNodeCount * GetLicenseCostPerNodeYear(),
            LicensingModel.FlatRate => GetClusterFixedCostPerYear(),
            _ => 0m
        };

        // Add cluster fixed costs
        baseCost += GetClusterFixedCostPerYear();

        // Apply multi-year discount
        var discount = GetMultiYearDiscount(input.ContractYears);
        baseCost *= (1 - discount);

        // Calculate support cost
        var supportTier = GetSupportTiers().FirstOrDefault(s => s.Tier == input.SupportTier);
        var supportMultiplier = supportTier?.CostMultiplier ?? 1.0m;
        var supportCost = (baseCost * (supportMultiplier - 1.0m)) + (supportTier?.AdditionalAnnualCost ?? 0m);

        // Handle managed service additional fees
        var additionalFees = 0m;
        if (input.IsManagedService)
        {
            additionalFees = CalculateManagedServiceFees(input);
        }

        var totalNodes = input.NodeCount > 0 ? input.NodeCount : 1;

        return new LicensingCost
        {
            BaseLicensePerYear = baseCost,
            SupportCostPerYear = supportCost,
            AdditionalFeesPerYear = additionalFees,
            PerNodePerYear = (baseCost + supportCost + additionalFees) / totalNodes,
            DiscountPercent = discount * 100,
            LicensingModel = GetLicensingModelDescription()
        };
    }

    /// <summary>
    /// Get multi-year contract discount
    /// </summary>
    protected virtual decimal GetMultiYearDiscount(int years)
    {
        return years switch
        {
            1 => 0m,
            2 => 0.05m,    // 5% for 2-year
            3 => 0.10m,    // 10% for 3-year
            >= 4 => 0.15m, // 15% for 4+ years
            _ => 0m
        };
    }

    /// <summary>
    /// Calculate managed service fees (e.g., ROSA, ARO hourly worker fees)
    /// </summary>
    protected virtual decimal CalculateManagedServiceFees(LicensingInput input)
    {
        // Default: worker node fee * hours per year
        var hoursPerYear = 8760; // 24 * 365
        return input.WorkerNodeCount * GetWorkerNodeFeePerHour() * hoursPerYear;
    }

    /// <summary>
    /// Get description of licensing model
    /// </summary>
    protected virtual string GetLicensingModelDescription()
    {
        return PrimaryLicensingModel switch
        {
            LicensingModel.PerNode => $"Per-node: ${GetLicenseCostPerNodeYear():N0}/node/year",
            LicensingModel.PerCore => $"Per-core: ${GetLicenseCostPerCoreYear():N0}/core/year",
            LicensingModel.PerSocket => $"Per-socket: ${GetLicenseCostPerSocketYear():N0}/socket/year",
            LicensingModel.PerWorkerNode => $"Per-worker: ${GetLicenseCostPerNodeYear():N0}/worker/year",
            LicensingModel.FlatRate => $"Flat rate: ${GetClusterFixedCostPerYear():N0}/cluster/year",
            _ => "Custom licensing"
        };
    }
}

/// <summary>
/// Types of licensing models
/// </summary>
public enum LicensingModel
{
    /// <summary>Open source, no license required</summary>
    OpenSource,

    /// <summary>Per physical/virtual node</summary>
    PerNode,

    /// <summary>Per CPU core</summary>
    PerCore,

    /// <summary>Per CPU socket</summary>
    PerSocket,

    /// <summary>Per worker node only (masters free)</summary>
    PerWorkerNode,

    /// <summary>Flat rate per cluster</summary>
    FlatRate,

    /// <summary>Usage-based pricing</summary>
    UsageBased
}
