using System.Collections.Frozen;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing.Base;
using InfraSizingCalculator.Models.Pricing.CloudProviders;
using InfraSizingCalculator.Models.Pricing.Distributions;

namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Factory for creating and managing pricing providers.
/// Implements Factory Pattern with lazy initialization and caching.
/// Thread-safe singleton access to pricing providers.
/// </summary>
public sealed class PricingProviderFactory : IPricingProviderFactory
{
    private static readonly Lazy<PricingProviderFactory> _instance =
        new(() => new PricingProviderFactory());

    private readonly FrozenDictionary<CloudProvider, ICloudProviderPricing> _cloudProviders;
    private readonly FrozenDictionary<Distribution, IDistributionLicensing> _distributionLicensing;

    /// <summary>
    /// Singleton instance of the factory
    /// </summary>
    public static PricingProviderFactory Instance => _instance.Value;

    private PricingProviderFactory()
    {
        _cloudProviders = InitializeCloudProviders();
        _distributionLicensing = InitializeDistributionLicensing();
    }

    /// <inheritdoc />
    public ICloudProviderPricing GetCloudProviderPricing(CloudProvider provider)
    {
        if (_cloudProviders.TryGetValue(provider, out var pricing))
        {
            return pricing;
        }

        // For managed OpenShift variants, return the underlying provider with OpenShift pricing
        return provider switch
        {
            CloudProvider.ROSA => _cloudProviders[CloudProvider.AWS],
            CloudProvider.ARO => _cloudProviders[CloudProvider.Azure],
            CloudProvider.OSD => _cloudProviders[CloudProvider.GCP],
            CloudProvider.ROKS => _cloudProviders[CloudProvider.IBM],
            _ => throw new ArgumentException($"Unknown cloud provider: {provider}", nameof(provider))
        };
    }

    /// <inheritdoc />
    public IDistributionLicensing GetDistributionLicensing(Distribution distribution)
    {
        if (_distributionLicensing.TryGetValue(distribution, out var licensing))
        {
            return licensing;
        }

        // For cloud-specific variants, get the base distribution licensing
        var baseLicensing = GetBaseLicensing(distribution);
        if (baseLicensing != null)
        {
            return baseLicensing;
        }

        // Default to managed K8s licensing for cloud-managed distributions
        var cloudProvider = distribution.GetCloudProvider();
        return new ManagedK8sLicensing(distribution, cloudProvider);
    }

    /// <inheritdoc />
    public IReadOnlyList<ICloudProviderPricing> GetAllCloudProviders()
    {
        return _cloudProviders.Values.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<IDistributionLicensing> GetAllDistributionLicensing()
    {
        return _distributionLicensing.Values.ToList();
    }

    /// <inheritdoc />
    public PricingModel GetPricing(CloudProvider provider, string? region = null)
    {
        var cloudPricing = GetCloudProviderPricing(provider);
        var pricing = cloudPricing.GetPricing(region);

        // For managed variants, override the provider and licensing
        if (IsManagedOpenShiftVariant(provider))
        {
            pricing.Provider = provider;

            // For managed OpenShift, license is included in service fee - no additional license cost
            pricing.Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 0,
                RancherEnterprisePerNodeYear = 0,
                TanzuPerCoreYear = 0,
                CharmedK8sPerNodeYear = 0
            };

            // Set the appropriate service fee
            pricing.Compute.OpenShiftServiceFeePerWorkerHour = GetManagedOpenShiftServiceFee(provider);
        }

        return pricing;
    }

    private static decimal GetManagedOpenShiftServiceFee(CloudProvider provider)
    {
        return provider switch
        {
            CloudProvider.ROSA => 0.171m,     // ROSA: $0.171/node/hr
            CloudProvider.ARO => 0.21m,       // ARO: ~$0.21/node/hr
            CloudProvider.OSD => 0.166m,      // OSD: ~$0.166/node/hr
            CloudProvider.ROKS => 0.20m,      // ROKS: ~$0.20/node/hr
            _ => 0m
        };
    }

    private static bool IsManagedOpenShiftVariant(CloudProvider provider)
    {
        return provider is CloudProvider.ROSA or CloudProvider.ARO or CloudProvider.OSD or CloudProvider.ROKS;
    }

    /// <inheritdoc />
    public LicensingCost CalculateLicensingCost(Distribution distribution, LicensingInput input)
    {
        var licensing = GetDistributionLicensing(distribution);
        return licensing.CalculateLicensingCost(input);
    }

    /// <inheritdoc />
    public bool IsProviderSupported(CloudProvider provider)
    {
        return _cloudProviders.ContainsKey(provider) ||
               provider is CloudProvider.ROSA or CloudProvider.ARO or CloudProvider.OSD or CloudProvider.ROKS;
    }

    /// <inheritdoc />
    public bool IsDistributionSupported(Distribution distribution)
    {
        return _distributionLicensing.ContainsKey(distribution) ||
               GetBaseLicensing(distribution) != null;
    }

    private static FrozenDictionary<CloudProvider, ICloudProviderPricing> InitializeCloudProviders()
    {
        var providers = new Dictionary<CloudProvider, ICloudProviderPricing>
        {
            // Major cloud providers
            [CloudProvider.AWS] = new AwsCloudPricing(),
            [CloudProvider.Azure] = new AzureCloudPricing(),
            [CloudProvider.GCP] = new GcpCloudPricing(),
            [CloudProvider.OCI] = new OciCloudPricing(),
            [CloudProvider.IBM] = new IbmCloudPricing(),
            [CloudProvider.Alibaba] = new AlibabaCloudPricing(),
            [CloudProvider.Tencent] = new TencentCloudPricing(),
            [CloudProvider.Huawei] = new HuaweiCloudPricing(),

            // Developer-friendly cloud providers
            [CloudProvider.DigitalOcean] = new DigitalOceanCloudPricing(),
            [CloudProvider.Linode] = new LinodeCloudPricing(),
            [CloudProvider.Vultr] = new VultrCloudPricing(),
            [CloudProvider.Hetzner] = new HetznerCloudPricing(),
            [CloudProvider.OVH] = new OvhCloudPricing(),
            [CloudProvider.Scaleway] = new ScalewayCloudPricing(),
            [CloudProvider.Civo] = new CivoCloudPricing(),
            [CloudProvider.Exoscale] = new ExoscaleCloudPricing(),

            // On-premises (self-hosted)
            [CloudProvider.OnPrem] = new OnPremCloudPricing()
        };

        return providers.ToFrozenDictionary();
    }

    private static FrozenDictionary<Distribution, IDistributionLicensing> InitializeDistributionLicensing()
    {
        var licensing = new Dictionary<Distribution, IDistributionLicensing>
        {
            // On-premises distributions
            [Distribution.OpenShift] = new OpenShiftLicensing(),
            [Distribution.Kubernetes] = new VanillaK8sLicensing(),
            [Distribution.Rancher] = new RancherLicensing(),
            [Distribution.RKE2] = new Rke2Licensing(),
            [Distribution.K3s] = new K3sLicensing(),
            [Distribution.MicroK8s] = new MicroK8sLicensing(),
            [Distribution.Charmed] = new CharmedLicensing(),
            [Distribution.Tanzu] = new TanzuLicensing(),

            // OpenShift managed variants
            [Distribution.OpenShiftROSA] = new OpenShiftLicensing(true, CloudProvider.ROSA),
            [Distribution.OpenShiftARO] = new OpenShiftLicensing(true, CloudProvider.ARO),
            [Distribution.OpenShiftDedicated] = new OpenShiftLicensing(true, CloudProvider.OSD),
            [Distribution.OpenShiftIBM] = new OpenShiftLicensing(true, CloudProvider.ROKS),

            // Managed K8s services
            [Distribution.EKS] = new ManagedK8sLicensing(Distribution.EKS, CloudProvider.AWS),
            [Distribution.AKS] = new ManagedK8sLicensing(Distribution.AKS, CloudProvider.Azure),
            [Distribution.GKE] = new ManagedK8sLicensing(Distribution.GKE, CloudProvider.GCP),
            [Distribution.OKE] = new ManagedK8sLicensing(Distribution.OKE, CloudProvider.OCI),
            [Distribution.IKS] = new ManagedK8sLicensing(Distribution.IKS, CloudProvider.IBM),
            [Distribution.ACK] = new ManagedK8sLicensing(Distribution.ACK, CloudProvider.Alibaba),
            [Distribution.TKE] = new ManagedK8sLicensing(Distribution.TKE, CloudProvider.Tencent),
            [Distribution.CCE] = new ManagedK8sLicensing(Distribution.CCE, CloudProvider.Huawei),
            [Distribution.DOKS] = new ManagedK8sLicensing(Distribution.DOKS, CloudProvider.DigitalOcean),
            [Distribution.LKE] = new ManagedK8sLicensing(Distribution.LKE, CloudProvider.Linode),
            [Distribution.VKE] = new ManagedK8sLicensing(Distribution.VKE, CloudProvider.Vultr),
            [Distribution.HetznerK8s] = new ManagedK8sLicensing(Distribution.HetznerK8s, CloudProvider.Hetzner),
            [Distribution.OVHKubernetes] = new ManagedK8sLicensing(Distribution.OVHKubernetes, CloudProvider.OVH),
            [Distribution.ScalewayKapsule] = new ManagedK8sLicensing(Distribution.ScalewayKapsule, CloudProvider.Scaleway)
        };

        return licensing.ToFrozenDictionary();
    }

    private IDistributionLicensing? GetBaseLicensing(Distribution distribution)
    {
        // Map cloud-specific variants to their base distributions
        return distribution switch
        {
            // Rancher variants
            Distribution.RancherHosted or
            Distribution.RancherEKS or
            Distribution.RancherAKS or
            Distribution.RancherGKE => _distributionLicensing[Distribution.Rancher],

            // Tanzu variants
            Distribution.TanzuCloud or
            Distribution.TanzuAWS or
            Distribution.TanzuAzure or
            Distribution.TanzuGCP => _distributionLicensing[Distribution.Tanzu],

            // Charmed variants
            Distribution.CharmedAWS or
            Distribution.CharmedAzure or
            Distribution.CharmedGCP => _distributionLicensing[Distribution.Charmed],

            // MicroK8s variants
            Distribution.MicroK8sAWS or
            Distribution.MicroK8sAzure or
            Distribution.MicroK8sGCP => _distributionLicensing[Distribution.MicroK8s],

            // K3s variants
            Distribution.K3sAWS or
            Distribution.K3sAzure or
            Distribution.K3sGCP => _distributionLicensing[Distribution.K3s],

            // RKE2 variants
            Distribution.RKE2AWS or
            Distribution.RKE2Azure or
            Distribution.RKE2GCP => _distributionLicensing[Distribution.RKE2],

            _ => null
        };
    }
}

/// <summary>
/// Interface for pricing provider factory
/// </summary>
public interface IPricingProviderFactory
{
    /// <summary>
    /// Get cloud provider pricing implementation
    /// </summary>
    ICloudProviderPricing GetCloudProviderPricing(CloudProvider provider);

    /// <summary>
    /// Get distribution licensing implementation
    /// </summary>
    IDistributionLicensing GetDistributionLicensing(Distribution distribution);

    /// <summary>
    /// Get all registered cloud providers
    /// </summary>
    IReadOnlyList<ICloudProviderPricing> GetAllCloudProviders();

    /// <summary>
    /// Get all registered distribution licensing
    /// </summary>
    IReadOnlyList<IDistributionLicensing> GetAllDistributionLicensing();

    /// <summary>
    /// Get complete pricing model for a provider and region
    /// </summary>
    PricingModel GetPricing(CloudProvider provider, string? region = null);

    /// <summary>
    /// Calculate licensing cost for a distribution
    /// </summary>
    LicensingCost CalculateLicensingCost(Distribution distribution, LicensingInput input);

    /// <summary>
    /// Check if a cloud provider is supported
    /// </summary>
    bool IsProviderSupported(CloudProvider provider);

    /// <summary>
    /// Check if a distribution is supported
    /// </summary>
    bool IsDistributionSupported(Distribution distribution);
}
