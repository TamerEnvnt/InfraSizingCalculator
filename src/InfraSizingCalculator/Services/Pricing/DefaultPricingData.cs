using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Models.Pricing.Base;
using InfraSizingCalculator.Models.Pricing.Distributions;

namespace InfraSizingCalculator.Services.Pricing;

/// <summary>
/// Default pricing data for all providers.
/// Delegates to PricingProviderFactory for actual pricing data.
/// Maintains backward compatibility with existing code.
/// </summary>
public static class DefaultPricingData
{
    private static readonly IPricingProviderFactory _factory = PricingProviderFactory.Instance;

    #region Major Cloud Providers

    /// <summary>
    /// Get default AWS pricing
    /// </summary>
    public static PricingModel GetAWSPricing(string region = "us-east-1")
        => _factory.GetPricing(CloudProvider.AWS, region);

    /// <summary>
    /// Get default Azure pricing
    /// </summary>
    public static PricingModel GetAzurePricing(string region = "eastus")
        => _factory.GetPricing(CloudProvider.Azure, region);

    /// <summary>
    /// Get default GCP pricing
    /// </summary>
    public static PricingModel GetGCPPricing(string region = "us-central1")
        => _factory.GetPricing(CloudProvider.GCP, region);

    /// <summary>
    /// Get default Oracle OCI pricing
    /// </summary>
    public static PricingModel GetOCIPricing(string region = "us-ashburn-1")
        => _factory.GetPricing(CloudProvider.OCI, region);

    /// <summary>
    /// Get default IBM Cloud pricing
    /// </summary>
    public static PricingModel GetIBMPricing(string region = "us-south")
        => _factory.GetPricing(CloudProvider.IBM, region);

    /// <summary>
    /// Get default Alibaba Cloud pricing
    /// </summary>
    public static PricingModel GetAlibabaPricing(string region = "cn-hangzhou")
        => _factory.GetPricing(CloudProvider.Alibaba, region);

    /// <summary>
    /// Get default Tencent Cloud pricing
    /// </summary>
    public static PricingModel GetTencentPricing(string region = "ap-guangzhou")
        => _factory.GetPricing(CloudProvider.Tencent, region);

    /// <summary>
    /// Get default Huawei Cloud pricing
    /// </summary>
    public static PricingModel GetHuaweiPricing(string region = "cn-north-4")
        => _factory.GetPricing(CloudProvider.Huawei, region);

    #endregion

    #region Managed OpenShift Services

    /// <summary>
    /// Get default ROSA (Red Hat OpenShift Service on AWS) pricing
    /// </summary>
    public static PricingModel GetROSAPricing(string region = "us-east-1")
        => _factory.GetPricing(CloudProvider.ROSA, region);

    /// <summary>
    /// Get default ARO (Azure Red Hat OpenShift) pricing
    /// </summary>
    public static PricingModel GetAROPricing(string region = "eastus")
        => _factory.GetPricing(CloudProvider.ARO, region);

    /// <summary>
    /// Get default OSD (OpenShift Dedicated on GCP) pricing
    /// </summary>
    public static PricingModel GetOSDPricing(string region = "us-central1")
        => _factory.GetPricing(CloudProvider.OSD, region);

    /// <summary>
    /// Get default ROKS (Red Hat OpenShift on IBM Cloud) pricing
    /// </summary>
    public static PricingModel GetROKSPricing(string region = "us-south")
        => _factory.GetPricing(CloudProvider.ROKS, region);

    #endregion

    #region Developer-Friendly Cloud Providers

    /// <summary>
    /// Get default DigitalOcean pricing
    /// </summary>
    public static PricingModel GetDigitalOceanPricing(string region = "nyc1")
        => _factory.GetPricing(CloudProvider.DigitalOcean, region);

    /// <summary>
    /// Get default Linode/Akamai pricing
    /// </summary>
    public static PricingModel GetLinodePricing(string region = "us-east")
        => _factory.GetPricing(CloudProvider.Linode, region);

    /// <summary>
    /// Get default Vultr pricing
    /// </summary>
    public static PricingModel GetVultrPricing(string region = "ewr")
        => _factory.GetPricing(CloudProvider.Vultr, region);

    /// <summary>
    /// Get default Hetzner pricing (very cost-effective European provider)
    /// </summary>
    public static PricingModel GetHetznerPricing(string region = "fsn1")
        => _factory.GetPricing(CloudProvider.Hetzner, region);

    /// <summary>
    /// Get default OVHcloud pricing
    /// </summary>
    public static PricingModel GetOVHPricing(string region = "gra")
        => _factory.GetPricing(CloudProvider.OVH, region);

    /// <summary>
    /// Get default Scaleway pricing
    /// </summary>
    public static PricingModel GetScalewayPricing(string region = "fr-par")
        => _factory.GetPricing(CloudProvider.Scaleway, region);

    /// <summary>
    /// Get default Civo Cloud pricing
    /// </summary>
    public static PricingModel GetCivoPricing(string region = "lon1")
        => _factory.GetPricing(CloudProvider.Civo, region);

    /// <summary>
    /// Get default Exoscale (SKS) pricing
    /// </summary>
    public static PricingModel GetExoscalePricing(string region = "ch-gva-2")
        => _factory.GetPricing(CloudProvider.Exoscale, region);

    #endregion

    #region On-Premises

    /// <summary>
    /// Get default on-premises pricing
    /// </summary>
    public static PricingModel GetOnPremPricing()
        => _factory.GetPricing(CloudProvider.OnPrem);

    #endregion

    #region Generic Access

    /// <summary>
    /// Get default pricing for any provider
    /// </summary>
    public static PricingModel GetDefaultPricing(CloudProvider provider, string? region = null)
        => _factory.GetPricing(provider, region);

    /// <summary>
    /// Get cloud provider pricing instance for direct access
    /// </summary>
    public static ICloudProviderPricing GetCloudProviderPricing(CloudProvider provider)
        => _factory.GetCloudProviderPricing(provider);

    /// <summary>
    /// Get distribution licensing for a K8s distribution
    /// </summary>
    public static IDistributionLicensing GetDistributionLicensing(Distribution distribution)
        => _factory.GetDistributionLicensing(distribution);

    /// <summary>
    /// Calculate licensing cost for a distribution
    /// </summary>
    public static LicensingCost CalculateLicensingCost(Distribution distribution, LicensingInput input)
        => _factory.CalculateLicensingCost(distribution, input);

    /// <summary>
    /// Get all available cloud providers
    /// </summary>
    public static IReadOnlyList<ICloudProviderPricing> GetAllCloudProviders()
        => _factory.GetAllCloudProviders();

    /// <summary>
    /// Get all distribution licensing configurations
    /// </summary>
    public static IReadOnlyList<IDistributionLicensing> GetAllDistributionLicensing()
        => _factory.GetAllDistributionLicensing();

    #endregion
}
