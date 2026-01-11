using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// RKE2 (Rancher Kubernetes Engine 2) licensing.
/// RKE2 is fully open source by SUSE/Rancher Labs.
///
/// RKE2 is free and FIPS-140-2 compliant by default.
/// Designed for security-focused, government, and enterprise deployments.
/// Enterprise support available through SUSE Rancher Prime subscription.
/// </summary>
public sealed class Rke2Licensing : DistributionLicensingBase
{
    private readonly Rke2Edition _edition;

    public Rke2Licensing() : this(Rke2Edition.Community) { }

    public Rke2Licensing(Rke2Edition edition)
    {
        _edition = edition;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.RKE2;

    /// <inheritdoc />
    public override bool RequiresLicense => false;

    /// <inheritdoc />
    public override string DisplayName => _edition switch
    {
        Rke2Edition.Community => "RKE2",
        Rke2Edition.Prime => "RKE2 (SUSE Rancher Prime)",
        Rke2Edition.Government => "RKE2 (SUSE Rancher Government)",
        _ => "RKE2"
    };

    /// <inheritdoc />
    public override string Vendor => "SUSE (Rancher Labs)";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel => LicensingModel.OpenSource;

    /// <summary>
    /// RKE2 is free. SUSE Rancher support optional.
    /// Prime: ~$750/node/year
    /// Government (FIPS + STIG): ~$1,200/node/year
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear()
    {
        return _edition switch
        {
            Rke2Edition.Community => 0m,
            Rke2Edition.Prime => 750m,
            Rke2Edition.Government => 1200m,
            _ => 0m
        };
    }

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        if (_edition == Rke2Edition.Community)
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
                }
            };
        }

        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Standard,
                Name = _edition == Rke2Edition.Government ? "Government Standard" : "Rancher Prime",
                Hours = "12x5",
                ResponseSLA = "4 business hours",
                CostMultiplier = 1.0m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = _edition == Rke2Edition.Government ? "Government Priority" : "Rancher Priority",
                Hours = "24x7",
                ResponseSLA = "1 hour (Sev 1)",
                CostMultiplier = 1.4m
            },
            new()
            {
                Tier = SupportTier.Enterprise,
                Name = _edition == Rke2Edition.Government ? "Government Premium" : "Rancher Premium",
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
/// RKE2 edition tiers
/// </summary>
public enum Rke2Edition
{
    /// <summary>Open source community edition</summary>
    Community,

    /// <summary>SUSE Rancher Prime - enterprise support</summary>
    Prime,

    /// <summary>SUSE Rancher Government - FedRAMP/STIG compliant</summary>
    Government
}
