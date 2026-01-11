using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Vanilla Kubernetes licensing (upstream CNCF Kubernetes).
/// Kubernetes is fully open source (Apache 2.0).
///
/// No licensing costs, but you manage everything yourself.
/// Third-party support available from various vendors.
/// </summary>
public sealed class VanillaK8sLicensing : DistributionLicensingBase
{
    /// <inheritdoc />
    public override Distribution Distribution => Distribution.Kubernetes;

    /// <inheritdoc />
    public override bool RequiresLicense => false;

    /// <inheritdoc />
    public override string DisplayName => "Kubernetes (Vanilla)";

    /// <inheritdoc />
    public override string Vendor => "CNCF (Cloud Native Computing Foundation)";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel => LicensingModel.OpenSource;

    /// <summary>
    /// Vanilla K8s is always free
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear() => 0m;

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Community,
                Name = "Community",
                Hours = "Kubernetes Slack, GitHub, Stack Overflow",
                ResponseSLA = "Best effort",
                CostMultiplier = 0m
            },
            new()
            {
                Tier = SupportTier.Basic,
                Name = "Third-Party Basic",
                Hours = "Business hours",
                ResponseSLA = "4 business hours",
                CostMultiplier = 1.0m,
                AdditionalAnnualCost = 2000m  // Typical third-party support
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Third-Party Premium",
                Hours = "24x7",
                ResponseSLA = "1 hour",
                CostMultiplier = 1.0m,
                AdditionalAnnualCost = 10000m  // Typical third-party premium
            }
        };
    }

    /// <inheritdoc />
    public override LicensingCost CalculateLicensingCost(LicensingInput input)
    {
        // Vanilla K8s has no license cost, only optional third-party support
        var supportTier = GetSupportTiers().FirstOrDefault(s => s.Tier == input.SupportTier);
        var supportCost = supportTier?.AdditionalAnnualCost ?? 0m;

        var totalNodes = input.NodeCount > 0 ? input.NodeCount : 1;

        return new LicensingCost
        {
            BaseLicensePerYear = 0m,
            SupportCostPerYear = supportCost,
            AdditionalFeesPerYear = 0m,
            PerNodePerYear = supportCost / totalNodes,
            DiscountPercent = 0m,
            LicensingModel = "Open Source - CNCF Apache 2.0"
        };
    }
}
