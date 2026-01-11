using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Models.Pricing.Distributions;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for PricingProviderFactory which is the central factory for all pricing providers.
/// Tests verify the factory pattern implementation, caching, and provider registration.
/// </summary>
public class PricingProviderFactoryTests
{
    #region Singleton Pattern Tests

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = PricingProviderFactory.Instance;
        var instance2 = PricingProviderFactory.Instance;

        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        PricingProviderFactory.Instance.Should().NotBeNull();
    }

    #endregion

    #region GetCloudProviderPricing Tests

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    public void GetCloudProviderPricing_ReturnsValidProvider(CloudProvider provider)
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetCloudProviderPricing(provider);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
    }

    [Fact]
    public void GetCloudProviderPricing_ROSA_ReturnsAWSProvider()
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetCloudProviderPricing(CloudProvider.ROSA);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.AWS);
    }

    [Fact]
    public void GetCloudProviderPricing_ARO_ReturnsAzureProvider()
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetCloudProviderPricing(CloudProvider.ARO);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.Azure);
    }

    [Fact]
    public void GetCloudProviderPricing_OSD_ReturnsGCPProvider()
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetCloudProviderPricing(CloudProvider.OSD);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.GCP);
    }

    [Fact]
    public void GetCloudProviderPricing_ROKS_ReturnsIBMProvider()
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetCloudProviderPricing(CloudProvider.ROKS);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.IBM);
    }

    [Fact]
    public void GetCloudProviderPricing_ReturnsSameInstanceForSameProvider()
    {
        var factory = PricingProviderFactory.Instance;
        var pricing1 = factory.GetCloudProviderPricing(CloudProvider.AWS);
        var pricing2 = factory.GetCloudProviderPricing(CloudProvider.AWS);

        pricing1.Should().BeSameAs(pricing2);
    }

    #endregion

    #region GetDistributionLicensing Tests

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.MicroK8s)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Tanzu)]
    public void GetDistributionLicensing_ReturnsValidLicensing(Distribution distribution)
    {
        var factory = PricingProviderFactory.Instance;
        var licensing = factory.GetDistributionLicensing(distribution);

        licensing.Should().NotBeNull();
        licensing.Distribution.Should().Be(distribution);
    }

    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.OKE)]
    [InlineData(Distribution.IKS)]
    [InlineData(Distribution.DOKS)]
    [InlineData(Distribution.LKE)]
    [InlineData(Distribution.VKE)]
    public void GetDistributionLicensing_ManagedK8s_ReturnsValidLicensing(Distribution distribution)
    {
        var factory = PricingProviderFactory.Instance;
        var licensing = factory.GetDistributionLicensing(distribution);

        licensing.Should().NotBeNull();
        licensing.Distribution.Should().Be(distribution);
        licensing.RequiresLicense.Should().BeFalse();
    }

    [Theory]
    [InlineData(Distribution.OpenShiftROSA)]
    [InlineData(Distribution.OpenShiftARO)]
    [InlineData(Distribution.OpenShiftDedicated)]
    [InlineData(Distribution.OpenShiftIBM)]
    public void GetDistributionLicensing_ManagedOpenShift_ReturnsValidLicensing(Distribution distribution)
    {
        var factory = PricingProviderFactory.Instance;
        var licensing = factory.GetDistributionLicensing(distribution);

        licensing.Should().NotBeNull();
        licensing.RequiresLicense.Should().BeTrue();
    }

    [Theory]
    [InlineData(Distribution.RancherHosted)]
    [InlineData(Distribution.RancherEKS)]
    [InlineData(Distribution.RancherAKS)]
    [InlineData(Distribution.RancherGKE)]
    public void GetDistributionLicensing_RancherVariants_ReturnsRancherLicensing(Distribution distribution)
    {
        var factory = PricingProviderFactory.Instance;
        var licensing = factory.GetDistributionLicensing(distribution);

        licensing.Should().NotBeNull();
        licensing.Vendor.Should().Contain("SUSE");
    }

    [Theory]
    [InlineData(Distribution.TanzuCloud)]
    [InlineData(Distribution.TanzuAWS)]
    [InlineData(Distribution.TanzuAzure)]
    [InlineData(Distribution.TanzuGCP)]
    public void GetDistributionLicensing_TanzuVariants_ReturnsTanzuLicensing(Distribution distribution)
    {
        var factory = PricingProviderFactory.Instance;
        var licensing = factory.GetDistributionLicensing(distribution);

        licensing.Should().NotBeNull();
        licensing.Vendor.Should().Contain("VMware");
    }

    #endregion

    #region GetPricing Tests

    [Theory]
    [InlineData(CloudProvider.AWS, "us-east-1")]
    [InlineData(CloudProvider.Azure, "eastus")]
    [InlineData(CloudProvider.GCP, "us-central1")]
    [InlineData(CloudProvider.Hetzner, "fsn1")]
    public void GetPricing_WithRegion_ReturnsCorrectPricing(CloudProvider provider, string region)
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetPricing(provider, region);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
        pricing.Region.Should().Be(region);
    }

    [Fact]
    public void GetPricing_WithNullRegion_UsesDefaultRegion()
    {
        var factory = PricingProviderFactory.Instance;
        var pricing = factory.GetPricing(CloudProvider.AWS, null);

        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.AWS);
        pricing.Region.Should().Be("us-east-1");
    }

    #endregion

    #region CalculateLicensingCost Tests

    [Fact]
    public void CalculateLicensingCost_OpenShift_ReturnsPositiveCost()
    {
        var factory = PricingProviderFactory.Instance;
        var input = new LicensingInput
        {
            NodeCount = 10,
            TotalCores = 40,
            SupportTier = SupportTier.Standard
        };

        var cost = factory.CalculateLicensingCost(Distribution.OpenShift, input);

        cost.Should().NotBeNull();
        cost.BaseLicensePerYear.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateLicensingCost_VanillaK8s_ReturnsZeroBaseCost()
    {
        var factory = PricingProviderFactory.Instance;
        var input = new LicensingInput
        {
            NodeCount = 10,
            TotalCores = 40,
            SupportTier = SupportTier.Community
        };

        var cost = factory.CalculateLicensingCost(Distribution.Kubernetes, input);

        cost.Should().NotBeNull();
        cost.BaseLicensePerYear.Should().Be(0);
    }

    [Fact]
    public void CalculateLicensingCost_ManagedK8s_ReturnsControlPlaneCostOnly()
    {
        var factory = PricingProviderFactory.Instance;
        var input = new LicensingInput
        {
            NodeCount = 5,
            TotalCores = 20,
            SupportTier = SupportTier.Standard
        };

        var cost = factory.CalculateLicensingCost(Distribution.EKS, input);

        cost.Should().NotBeNull();
        cost.BaseLicensePerYear.Should().Be(0);
        cost.AdditionalFeesPerYear.Should().BeGreaterThan(0); // Control plane cost
    }

    #endregion

    #region GetAllCloudProviders Tests

    [Fact]
    public void GetAllCloudProviders_Returns16Providers()
    {
        var factory = PricingProviderFactory.Instance;
        var providers = factory.GetAllCloudProviders();

        providers.Should().NotBeNull();
        providers.Should().HaveCount(16);
    }

    [Fact]
    public void GetAllCloudProviders_ContainsAllMajorProviders()
    {
        var factory = PricingProviderFactory.Instance;
        var providers = factory.GetAllCloudProviders();

        var providerTypes = providers.Select(p => p.Provider).ToList();

        providerTypes.Should().Contain(CloudProvider.AWS);
        providerTypes.Should().Contain(CloudProvider.Azure);
        providerTypes.Should().Contain(CloudProvider.GCP);
        providerTypes.Should().Contain(CloudProvider.OCI);
        providerTypes.Should().Contain(CloudProvider.IBM);
    }

    [Fact]
    public void GetAllCloudProviders_ContainsAllDeveloperFriendlyProviders()
    {
        var factory = PricingProviderFactory.Instance;
        var providers = factory.GetAllCloudProviders();

        var providerTypes = providers.Select(p => p.Provider).ToList();

        providerTypes.Should().Contain(CloudProvider.DigitalOcean);
        providerTypes.Should().Contain(CloudProvider.Linode);
        providerTypes.Should().Contain(CloudProvider.Vultr);
        providerTypes.Should().Contain(CloudProvider.Hetzner);
        providerTypes.Should().Contain(CloudProvider.OVH);
        providerTypes.Should().Contain(CloudProvider.Scaleway);
        providerTypes.Should().Contain(CloudProvider.Civo);
        providerTypes.Should().Contain(CloudProvider.Exoscale);
    }

    #endregion

    #region GetAllDistributionLicensing Tests

    [Fact]
    public void GetAllDistributionLicensing_ReturnsNonEmptyList()
    {
        var factory = PricingProviderFactory.Instance;
        var distributions = factory.GetAllDistributionLicensing();

        distributions.Should().NotBeNull();
        distributions.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAllDistributionLicensing_ContainsOnPremDistributions()
    {
        var factory = PricingProviderFactory.Instance;
        var distributions = factory.GetAllDistributionLicensing();

        var distTypes = distributions.Select(d => d.Distribution).ToList();

        distTypes.Should().Contain(Distribution.OpenShift);
        distTypes.Should().Contain(Distribution.Kubernetes);
        distTypes.Should().Contain(Distribution.Rancher);
        distTypes.Should().Contain(Distribution.K3s);
        distTypes.Should().Contain(Distribution.RKE2);
    }

    [Fact]
    public void GetAllDistributionLicensing_ContainsManagedK8sDistributions()
    {
        var factory = PricingProviderFactory.Instance;
        var distributions = factory.GetAllDistributionLicensing();

        var distTypes = distributions.Select(d => d.Distribution).ToList();

        distTypes.Should().Contain(Distribution.EKS);
        distTypes.Should().Contain(Distribution.AKS);
        distTypes.Should().Contain(Distribution.GKE);
    }

    #endregion

    #region IsProviderSupported Tests

    [Theory]
    [InlineData(CloudProvider.AWS, true)]
    [InlineData(CloudProvider.Azure, true)]
    [InlineData(CloudProvider.GCP, true)]
    [InlineData(CloudProvider.ROSA, true)]
    [InlineData(CloudProvider.ARO, true)]
    [InlineData(CloudProvider.OSD, true)]
    [InlineData(CloudProvider.ROKS, true)]
    public void IsProviderSupported_ReturnsCorrectResult(CloudProvider provider, bool expected)
    {
        var factory = PricingProviderFactory.Instance;
        factory.IsProviderSupported(provider).Should().Be(expected);
    }

    #endregion

    #region IsDistributionSupported Tests

    [Theory]
    [InlineData(Distribution.OpenShift, true)]
    [InlineData(Distribution.Kubernetes, true)]
    [InlineData(Distribution.EKS, true)]
    [InlineData(Distribution.AKS, true)]
    [InlineData(Distribution.RancherEKS, true)]
    [InlineData(Distribution.TanzuAWS, true)]
    public void IsDistributionSupported_ReturnsCorrectResult(Distribution distribution, bool expected)
    {
        var factory = PricingProviderFactory.Instance;
        factory.IsDistributionSupported(distribution).Should().Be(expected);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void Factory_IsThreadSafe()
    {
        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var factory = PricingProviderFactory.Instance;
                var aws = factory.GetCloudProviderPricing(CloudProvider.AWS);
                var azure = factory.GetCloudProviderPricing(CloudProvider.Azure);
                var openshift = factory.GetDistributionLicensing(Distribution.OpenShift);

                return aws != null && azure != null && openshift != null;
            }));
        }

        Task.WaitAll(tasks.ToArray());

        tasks.Should().OnlyContain(t => t.Result == true);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullWorkflow_CalculateTotalCost_WorksCorrectly()
    {
        var factory = PricingProviderFactory.Instance;

        // Get cloud pricing
        var awsPricing = factory.GetPricing(CloudProvider.AWS, "us-east-1");

        // Get distribution licensing
        var licensingInput = new LicensingInput
        {
            NodeCount = 10,
            TotalCores = 40,
            SupportTier = SupportTier.Standard
        };
        var licensingCost = factory.CalculateLicensingCost(Distribution.OpenShift, licensingInput);

        // Verify we can calculate total costs
        var computeCostPerHour = awsPricing.Compute.CpuPerHour * 40 + awsPricing.Compute.RamGBPerHour * 80;
        var licenseCostPerYear = licensingCost.BaseLicensePerYear;

        computeCostPerHour.Should().BeGreaterThan(0);
        licenseCostPerYear.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ManagedOpenShift_CombinesCloudAndDistributionPricing()
    {
        var factory = PricingProviderFactory.Instance;

        // ROSA uses AWS infrastructure + OpenShift licensing
        var awsPricing = factory.GetCloudProviderPricing(CloudProvider.ROSA);
        var rosaLicensing = factory.GetDistributionLicensing(Distribution.OpenShiftROSA);

        awsPricing.Provider.Should().Be(CloudProvider.AWS);
        rosaLicensing.RequiresLicense.Should().BeTrue();
        rosaLicensing.Vendor.Should().Contain("Red Hat");
    }

    #endregion
}
