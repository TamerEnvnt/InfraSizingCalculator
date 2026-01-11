using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// K3s lightweight Kubernetes licensing.
/// K3s is fully open source (Apache 2.0) by SUSE/Rancher Labs.
///
/// K3s is free and designed for edge, IoT, and resource-constrained environments.
/// Enterprise support available through SUSE Rancher Prime subscription.
/// </summary>
public sealed class K3sLicensing : DistributionLicensingBase
{
    private readonly bool _withRancherSupport;

    public K3sLicensing() : this(false) { }

    public K3sLicensing(bool withRancherSupport)
    {
        _withRancherSupport = withRancherSupport;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.K3s;

    /// <inheritdoc />
    public override bool RequiresLicense => false;

    /// <inheritdoc />
    public override string DisplayName => _withRancherSupport
        ? "K3s (SUSE Rancher Prime)"
        : "K3s";

    /// <inheritdoc />
    public override string Vendor => "SUSE (Rancher Labs)";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel => LicensingModel.OpenSource;

    /// <summary>
    /// K3s is free. SUSE Rancher support optional.
    /// Rancher Prime for K3s: ~$500/node/year
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear() => _withRancherSupport ? 500m : 0m;

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Community,
                Name = "Community",
                Hours = "GitHub Issues, Slack",
                ResponseSLA = "Best effort",
                CostMultiplier = 0m
            },
            new()
            {
                Tier = SupportTier.Standard,
                Name = "SUSE Rancher Prime",
                Hours = "12x5",
                ResponseSLA = "4 business hours",
                CostMultiplier = 1.0m,
                AdditionalAnnualCost = 500m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "SUSE Rancher Priority",
                Hours = "24x7",
                ResponseSLA = "1 hour",
                CostMultiplier = 1.4m,
                AdditionalAnnualCost = 700m
            }
        };
    }
}
