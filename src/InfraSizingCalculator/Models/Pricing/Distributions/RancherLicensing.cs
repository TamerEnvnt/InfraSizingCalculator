using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// SUSE Rancher licensing.
/// Pricing based on SUSE public pricing as of 2025.
///
/// Rancher is open source, but enterprise support is available per-node.
/// RKE2 and K3s are included in Rancher support.
/// </summary>
public sealed class RancherLicensing : DistributionLicensingBase
{
    private readonly RancherEdition _edition;

    public RancherLicensing() : this(RancherEdition.Prime) { }

    public RancherLicensing(RancherEdition edition)
    {
        _edition = edition;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.Rancher;

    /// <inheritdoc />
    public override bool RequiresLicense => _edition != RancherEdition.Community;

    /// <inheritdoc />
    public override string DisplayName => _edition == RancherEdition.Community
        ? "Rancher (Community)"
        : $"SUSE Rancher ({_edition})";

    /// <inheritdoc />
    public override string Vendor => "SUSE";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel =>
        _edition == RancherEdition.Community ? LicensingModel.OpenSource : LicensingModel.PerNode;

    /// <summary>
    /// Per-node pricing based on edition
    /// Prime: ~$1,000/node/year
    /// Government: ~$1,500/node/year
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear()
    {
        return _edition switch
        {
            RancherEdition.Community => 0m,
            RancherEdition.Prime => 1000m,
            RancherEdition.Government => 1500m,
            _ => 1000m
        };
    }

    /// <summary>
    /// Optional cluster-level Rancher Manager cost
    /// </summary>
    public override decimal GetClusterFixedCostPerYear()
    {
        // Rancher Manager is free, but HA setup might have costs
        return 0m;
    }

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        if (_edition == RancherEdition.Community)
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
                }
            };
        }

        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Standard,
                Name = "Standard",
                Hours = "12x5 (business hours)",
                ResponseSLA = "4 business hours (Sev 1)",
                CostMultiplier = 1.0m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Priority",
                Hours = "24x7",
                ResponseSLA = "1 hour (Sev 1)",
                CostMultiplier = 1.4m
            },
            new()
            {
                Tier = SupportTier.Enterprise,
                Name = "Premium",
                Hours = "24x7 + Dedicated SE",
                ResponseSLA = "15 minutes (Sev 1)",
                CostMultiplier = 1.8m,
                AdditionalAnnualCost = 25000m,
                IncludesTAM = true
            }
        };
    }
}

/// <summary>
/// Rancher edition tiers
/// </summary>
public enum RancherEdition
{
    /// <summary>Open source community edition</summary>
    Community,

    /// <summary>SUSE Rancher Prime - enterprise support</summary>
    Prime,

    /// <summary>SUSE Rancher Government - FedRAMP compliant</summary>
    Government
}
