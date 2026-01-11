using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Canonical Charmed Kubernetes licensing.
/// Pricing based on Canonical Ubuntu Pro pricing as of 2025.
///
/// Charmed Kubernetes is open source, but Ubuntu Pro support is available.
/// Includes Juju, MAAS, and OpenStack integration support.
/// </summary>
public sealed class CharmedLicensing : DistributionLicensingBase
{
    private readonly CharmedEdition _edition;

    public CharmedLicensing() : this(CharmedEdition.Pro) { }

    public CharmedLicensing(CharmedEdition edition)
    {
        _edition = edition;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.Charmed;

    /// <inheritdoc />
    public override bool RequiresLicense => _edition != CharmedEdition.Free;

    /// <inheritdoc />
    public override string DisplayName => _edition == CharmedEdition.Free
        ? "Charmed Kubernetes (Free)"
        : $"Charmed Kubernetes ({_edition})";

    /// <inheritdoc />
    public override string Vendor => "Canonical";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel =>
        _edition == CharmedEdition.Free ? LicensingModel.OpenSource : LicensingModel.PerNode;

    /// <summary>
    /// Per-node pricing based on edition (Ubuntu Pro pricing)
    /// Pro Desktop/Server: ~$500/node/year
    /// Pro + Support: ~$1,500/node/year
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear()
    {
        return _edition switch
        {
            CharmedEdition.Free => 0m,
            CharmedEdition.Pro => 500m,
            CharmedEdition.ProSupport => 1500m,
            _ => 500m
        };
    }

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        if (_edition == CharmedEdition.Free)
        {
            return new List<SupportTierInfo>
            {
                new()
                {
                    Tier = SupportTier.Community,
                    Name = "Community",
                    Hours = "Community forums, Ask Ubuntu",
                    ResponseSLA = "Best effort",
                    CostMultiplier = 0m
                }
            };
        }

        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Standard,
                Name = "Ubuntu Pro",
                Hours = "10x5",
                ResponseSLA = "4 business hours",
                CostMultiplier = 1.0m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Ubuntu Pro + 24x7",
                Hours = "24x7",
                ResponseSLA = "1 hour (Sev 1)",
                CostMultiplier = 2.0m
            },
            new()
            {
                Tier = SupportTier.Enterprise,
                Name = "Ubuntu Pro + TAM",
                Hours = "24x7 + Dedicated TAM",
                ResponseSLA = "15 minutes (Sev 1)",
                CostMultiplier = 2.5m,
                AdditionalAnnualCost = 40000m,
                IncludesTAM = true
            }
        };
    }
}

/// <summary>
/// Charmed Kubernetes edition tiers
/// </summary>
public enum CharmedEdition
{
    /// <summary>Free community edition</summary>
    Free,

    /// <summary>Ubuntu Pro - security patching, compliance</summary>
    Pro,

    /// <summary>Ubuntu Pro + Support - full enterprise support</summary>
    ProSupport
}
