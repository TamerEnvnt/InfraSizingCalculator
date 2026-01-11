using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing.Distributions;

/// <summary>
/// Managed Kubernetes licensing for cloud providers (EKS, AKS, GKE, etc.).
///
/// Managed K8s services typically have:
/// - Control plane fees (hourly or monthly)
/// - No separate K8s licensing (included in service)
/// - Support included in cloud provider support plans
/// </summary>
public sealed class ManagedK8sLicensing : DistributionLicensingBase
{
    private readonly Distribution _distribution;
    private readonly CloudProvider _cloudProvider;

    public ManagedK8sLicensing(Distribution distribution, CloudProvider cloudProvider)
    {
        _distribution = distribution;
        _cloudProvider = cloudProvider;
    }

    /// <inheritdoc />
    public override Distribution Distribution => _distribution;

    /// <inheritdoc />
    public override bool RequiresLicense => false;

    /// <inheritdoc />
    public override string DisplayName => GetDisplayName();

    /// <inheritdoc />
    public override string Vendor => GetVendor();

    /// <inheritdoc />
    protected override LicensingModel PrimaryLicensingModel => LicensingModel.UsageBased;

    /// <summary>
    /// No per-node licensing for managed K8s
    /// </summary>
    public override decimal GetLicenseCostPerNodeYear() => 0m;

    /// <summary>
    /// Get control plane cost per year
    /// </summary>
    public override decimal GetClusterFixedCostPerYear()
    {
        // Control plane hourly cost * hours per year
        var hourlyRate = GetControlPlaneHourlyCost();
        return hourlyRate * 8760; // 24 * 365
    }

    /// <inheritdoc />
    public override IReadOnlyList<SupportTierInfo> GetSupportTiers()
    {
        // Support is handled by cloud provider support plans
        return new List<SupportTierInfo>
        {
            new()
            {
                Tier = SupportTier.Basic,
                Name = "Cloud Provider Basic",
                Hours = "Online resources, forums",
                ResponseSLA = "Best effort",
                CostMultiplier = 0m
            },
            new()
            {
                Tier = SupportTier.Standard,
                Name = "Cloud Provider Business",
                Hours = "24x7",
                ResponseSLA = "1 hour (critical)",
                CostMultiplier = 1.0m  // Percentage of cloud spend
            },
            new()
            {
                Tier = SupportTier.Enterprise,
                Name = "Cloud Provider Enterprise",
                Hours = "24x7 + TAM",
                ResponseSLA = "15 minutes (critical)",
                CostMultiplier = 1.0m,
                IncludesTAM = true
            }
        };
    }

    /// <inheritdoc />
    public override LicensingCost CalculateLicensingCost(LicensingInput input)
    {
        // Managed K8s: control plane cost only, no per-node licensing
        var controlPlaneCost = GetClusterFixedCostPerYear();

        var totalNodes = input.NodeCount > 0 ? input.NodeCount : 1;

        return new LicensingCost
        {
            BaseLicensePerYear = 0m,
            SupportCostPerYear = 0m,  // Included in cloud support
            AdditionalFeesPerYear = controlPlaneCost,
            PerNodePerYear = controlPlaneCost / totalNodes,
            DiscountPercent = 0m,
            LicensingModel = $"Managed K8s - Control plane: ${GetControlPlaneHourlyCost():F2}/hour"
        };
    }

    private decimal GetControlPlaneHourlyCost()
    {
        return _cloudProvider switch
        {
            CloudProvider.AWS => 0.10m,          // EKS: $0.10/hr
            CloudProvider.Azure => 0m,           // AKS: Free tier, $0.10/hr Standard
            CloudProvider.GCP => 0.10m,          // GKE: $0.10/hr
            CloudProvider.OCI => 0m,             // OKE: Free basic
            CloudProvider.IBM => 0m,             // IKS: Free control plane
            CloudProvider.Alibaba => 0m,         // ACK: Free tier
            CloudProvider.Tencent => 0m,         // TKE: Free tier
            CloudProvider.Huawei => 0m,          // CCE: Free tier
            CloudProvider.DigitalOcean => 0m,    // DOKS: Free
            CloudProvider.Linode => 0m,          // LKE: Free
            CloudProvider.Vultr => 0m,           // VKE: Free
            CloudProvider.Hetzner => 0m,         // Self-managed
            CloudProvider.OVH => 0m,             // Free
            CloudProvider.Scaleway => 0m,        // Free
            CloudProvider.Civo => 0m,            // Free
            CloudProvider.Exoscale => 0m,        // SKS: Free
            _ => 0.10m                           // Default to EKS pricing
        };
    }

    private string GetDisplayName()
    {
        return _distribution switch
        {
            Distribution.EKS => "Amazon EKS",
            Distribution.AKS => "Azure AKS",
            Distribution.GKE => "Google GKE",
            Distribution.OKE => "Oracle OKE",
            Distribution.IKS => "IBM IKS",
            Distribution.ACK => "Alibaba ACK",
            Distribution.TKE => "Tencent TKE",
            Distribution.CCE => "Huawei CCE",
            Distribution.DOKS => "DigitalOcean DOKS",
            Distribution.LKE => "Linode LKE",
            Distribution.VKE => "Vultr VKE",
            Distribution.HetznerK8s => "Hetzner K8s",
            Distribution.OVHKubernetes => "OVH Kubernetes",
            Distribution.ScalewayKapsule => "Scaleway Kapsule",
            _ => $"{_cloudProvider} Managed K8s"
        };
    }

    private string GetVendor()
    {
        return _cloudProvider switch
        {
            CloudProvider.AWS => "Amazon Web Services",
            CloudProvider.Azure => "Microsoft",
            CloudProvider.GCP => "Google Cloud",
            CloudProvider.OCI => "Oracle",
            CloudProvider.IBM => "IBM",
            CloudProvider.Alibaba => "Alibaba Cloud",
            CloudProvider.Tencent => "Tencent Cloud",
            CloudProvider.Huawei => "Huawei Cloud",
            CloudProvider.DigitalOcean => "DigitalOcean",
            CloudProvider.Linode => "Akamai (Linode)",
            CloudProvider.Vultr => "Vultr",
            CloudProvider.Hetzner => "Hetzner",
            CloudProvider.OVH => "OVHcloud",
            CloudProvider.Scaleway => "Scaleway",
            CloudProvider.Civo => "Civo",
            CloudProvider.Exoscale => "Exoscale",
            _ => "Cloud Provider"
        };
    }
}
