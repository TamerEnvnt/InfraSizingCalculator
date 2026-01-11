using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Canonical MicroK8s licensing.
/// Pricing based on Canonical Ubuntu Pro pricing as of 2025.
///
/// MicroK8s is free and open source.
/// Enterprise support available through Ubuntu Pro.
/// Ideal for edge, IoT, and development environments.
/// </summary>
public sealed class MicroK8sLicensing : DistributionLicensingBase
{
    private readonly bool _withUbuntuPro;

    public MicroK8sLicensing() : this(false) { }

    public MicroK8sLicensing(bool withUbuntuPro)
    {
        _withUbuntuPro = withUbuntuPro;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.MicroK8s;

    /// <inheritdoc />
    public override bool RequiresLicense => false;

    /// <inheritdoc />
    public override string DisplayName => _withUbuntuPro
        ? "MicroK8s (Ubuntu Pro)"
        : "MicroK8s";

    /// <inheritdoc />
    public override string Vendor => "Canonical";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel => LicensingModel.OpenSource;

    /// <summary>
    /// MicroK8s is free. Ubuntu Pro support optional.
    /// Pro pricing: ~$225/node/year for edge/IoT
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear() => _withUbuntuPro ? 225m : 0m;

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Community,
                Name = "Community",
                Hours = "Community forums",
                ResponseSLA = "Best effort",
                CostMultiplier = 0m
            },
            new()
            {
                Tier = SupportTier.Standard,
                Name = "Ubuntu Pro (Device)",
                Hours = "10x5",
                ResponseSLA = "4 business hours",
                CostMultiplier = 1.0m,
                AdditionalAnnualCost = 225m  // Per device
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Ubuntu Pro + Support",
                Hours = "24x7",
                ResponseSLA = "1 hour",
                CostMultiplier = 1.0m,
                AdditionalAnnualCost = 500m
            }
        };
    }
}
