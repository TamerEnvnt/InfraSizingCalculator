using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// VMware Tanzu Kubernetes Grid licensing.
/// Pricing based on VMware/Broadcom public pricing as of 2025.
///
/// Tanzu uses per-core licensing model.
/// Tanzu Standard, Advanced, and Enterprise tiers available.
/// </summary>
public sealed class TanzuLicensing : DistributionLicensingBase
{
    private readonly TanzuEdition _edition;

    public TanzuLicensing() : this(TanzuEdition.Standard) { }

    public TanzuLicensing(TanzuEdition edition)
    {
        _edition = edition;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.Tanzu;

    /// <inheritdoc />
    public override bool RequiresLicense => true;

    /// <inheritdoc />
    public override string DisplayName => $"VMware Tanzu ({_edition})";

    /// <inheritdoc />
    public override string Vendor => "VMware (Broadcom)";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel => LicensingModel.PerCore;

    /// <summary>
    /// Per-core pricing based on edition
    /// Standard: ~$1,500/core/year
    /// Advanced: ~$2,000/core/year
    /// Enterprise: ~$2,500/core/year
    /// </summary>
    public override decimal GetLicenseCostPerCoreYear()
    {
        return _edition switch
        {
            TanzuEdition.Standard => 1500m,
            TanzuEdition.Advanced => 2000m,
            TanzuEdition.Enterprise => 2500m,
            _ => 1500m
        };
    }

    /// <summary>
    /// Per-node equivalent (assuming 8 cores per node average)
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear() => GetLicenseCostPerCoreYear() * 8;

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Basic,
                Name = "Production",
                Hours = "12x5",
                ResponseSLA = "4 business hours",
                CostMultiplier = 1.0m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Premier",
                Hours = "24x7",
                ResponseSLA = "30 minutes (Sev 1)",
                CostMultiplier = 1.25m
            },
            new()
            {
                Tier = SupportTier.Enterprise,
                Name = "Premier + TAM",
                Hours = "24x7 + Dedicated TAM",
                ResponseSLA = "15 minutes (Sev 1)",
                CostMultiplier = 1.5m,
                AdditionalAnnualCost = 75000m,
                IncludesTAM = true
            }
        };
    }

    /// <inheritdoc />
    public override LicensingCost CalculateLicensingCost(LicensingInput input)
    {
        // Tanzu requires minimum core counts
        var effectiveCores = Math.Max(input.TotalCores, 16);  // Minimum 16 cores

        var baseCost = effectiveCores * GetLicenseCostPerCoreYear();

        // Apply multi-year discount
        var discount = GetMultiYearDiscount(input.ContractYears);
        baseCost *= (1 - discount);

        // Calculate support cost
        var supportTier = GetSupportTiers().FirstOrDefault(s => s.Tier == input.SupportTier);
        var supportMultiplier = supportTier?.CostMultiplier ?? 1.0m;
        var supportCost = (baseCost * (supportMultiplier - 1.0m)) + (supportTier?.AdditionalAnnualCost ?? 0m);

        var totalNodes = input.NodeCount > 0 ? input.NodeCount : 1;

        return new LicensingCost
        {
            BaseLicensePerYear = baseCost,
            SupportCostPerYear = supportCost,
            AdditionalFeesPerYear = 0m,
            PerNodePerYear = (baseCost + supportCost) / totalNodes,
            DiscountPercent = discount * 100,
            LicensingModel = $"Per-core ({_edition}): ${GetLicenseCostPerCoreYear():N0}/core/year"
        };
    }
}

/// <summary>
/// Tanzu edition tiers
/// </summary>
public enum TanzuEdition
{
    /// <summary>Standard edition - basic K8s with lifecycle management</summary>
    Standard,

    /// <summary>Advanced edition - adds Harbor, service mesh</summary>
    Advanced,

    /// <summary>Enterprise edition - full platform with Aria, vSphere integration</summary>
    Enterprise
}
