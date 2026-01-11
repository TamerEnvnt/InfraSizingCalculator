using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Red Hat OpenShift Container Platform licensing.
/// Pricing based on Red Hat public pricing as of 2025.
///
/// OpenShift licensing is per-node with support included.
/// Managed variants (ROSA, ARO, OSD) have different pricing models
/// with hourly worker node fees on top of cloud infrastructure.
/// </summary>
public sealed class OpenShiftLicensing : DistributionLicensingBase
{
    private readonly bool _isManagedVariant;
    private readonly CloudProvider? _cloudProvider;

    /// <summary>
    /// Create OpenShift licensing for on-premises deployment
    /// </summary>
    public OpenShiftLicensing() : this(false, null) { }

    /// <summary>
    /// Create OpenShift licensing for managed variant (ROSA, ARO, OSD)
    /// </summary>
    public OpenShiftLicensing(bool isManagedVariant, CloudProvider? cloudProvider = null)
    {
        _isManagedVariant = isManagedVariant;
        _cloudProvider = cloudProvider;
    }

    /// <inheritdoc />
    public override Distribution Distribution => Distribution.OpenShift;

    /// <inheritdoc />
    public override bool RequiresLicense => true;

    /// <inheritdoc />
    public override string DisplayName => _isManagedVariant
        ? $"OpenShift ({GetManagedVariantName()})"
        : "OpenShift Container Platform";

    /// <inheritdoc />
    public override string Vendor => "Red Hat";

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel =>
        _isManagedVariant ? LicensingModel.UsageBased : LicensingModel.PerNode;

    /// <summary>
    /// Per-node cost includes 24x7 support
    /// Standard: ~$2,500/node/year
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear() => 2500m;

    /// <summary>
    /// Per-core pricing available for larger deployments
    /// ~$200/core/year
    /// </summary>
    public override decimal GetLicenseCostPerCoreYear() => 200m;

    /// <summary>
    /// ROSA/ARO/OSD worker node fee per hour
    /// </summary>
    public override decimal GetWorkerNodeFeePerHour()
    {
        return _cloudProvider switch
        {
            CloudProvider.AWS or CloudProvider.ROSA => 0.171m,     // ROSA: $0.171/node/hr
            CloudProvider.Azure or CloudProvider.ARO => 0.21m,     // ARO: ~$0.21/node/hr
            CloudProvider.GCP or CloudProvider.OSD => 0.166m,      // OSD: ~$0.166/node/hr
            CloudProvider.IBM or CloudProvider.ROKS => 0.20m,      // ROKS: ~$0.20/node/hr
            _ => 0.171m  // Default to ROSA pricing
        };
    }

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Standard,
                Name = "Standard",
                Hours = "Business hours (Mon-Fri)",
                ResponseSLA = "4 business hours (Sev 1)",
                CostMultiplier = 1.0m
            },
            new()
            {
                Tier = SupportTier.Premium,
                Name = "Premium",
                Hours = "24x7x365",
                ResponseSLA = "1 hour (Sev 1)",
                CostMultiplier = 1.3m  // 30% premium
            },
            new()
            {
                Tier = SupportTier.Enterprise,
                Name = "Premium Plus (TAM)",
                Hours = "24x7x365 + Dedicated TAM",
                ResponseSLA = "30 minutes (Sev 1)",
                CostMultiplier = 1.5m,
                AdditionalAnnualCost = 50000m,  // TAM cost
                IncludesTAM = true
            }
        };
    }

    /// <inheritdoc />
    public override LicensingCost CalculateLicensingCost(LicensingInput input)
    {
        if (_isManagedVariant)
        {
            return CalculateManagedLicensingCost(input);
        }

        return base.CalculateLicensingCost(input);
    }

    private LicensingCost CalculateManagedLicensingCost(LicensingInput input)
    {
        // Managed OpenShift (ROSA, ARO, OSD): hourly worker node fees
        var hoursPerYear = 8760m;
        var workerFees = input.WorkerNodeCount * GetWorkerNodeFeePerHour() * hoursPerYear;

        // Control plane is often included in managed price
        var controlPlaneCost = GetManagedControlPlaneCostPerYear();

        var totalCost = workerFees + controlPlaneCost;
        var totalNodes = input.NodeCount > 0 ? input.NodeCount : 1;

        return new LicensingCost
        {
            BaseLicensePerYear = workerFees,
            SupportCostPerYear = 0m,  // Support included in managed service
            AdditionalFeesPerYear = controlPlaneCost,
            PerNodePerYear = totalCost / totalNodes,
            DiscountPercent = 0m,
            LicensingModel = $"Managed OpenShift: ${GetWorkerNodeFeePerHour():F3}/worker/hour"
        };
    }

    private decimal GetManagedControlPlaneCostPerYear()
    {
        // Managed control plane costs vary by provider
        return _cloudProvider switch
        {
            CloudProvider.AWS or CloudProvider.ROSA => 0m,     // Included in ROSA
            CloudProvider.Azure or CloudProvider.ARO => 0m,    // Included in ARO
            CloudProvider.GCP or CloudProvider.OSD => 0m,      // Included in OSD
            CloudProvider.IBM or CloudProvider.ROKS => 0m,     // Included in ROKS
            _ => 0m
        };
    }

    private string GetManagedVariantName()
    {
        return _cloudProvider switch
        {
            CloudProvider.AWS or CloudProvider.ROSA => "ROSA",
            CloudProvider.Azure or CloudProvider.ARO => "ARO",
            CloudProvider.GCP or CloudProvider.OSD => "Dedicated",
            CloudProvider.IBM or CloudProvider.ROKS => "ROKS",
            _ => "Managed"
        };
    }
}
