using FluentAssertions;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for DefaultPricingData static class that provides default pricing for all cloud providers
/// </summary>
public class DefaultPricingDataTests
{
    #region Major Cloud Providers Tests

    [Fact]
    public void GetAWSPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetAWSPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.AWS, "us-east-1");
    }

    [Fact]
    public void GetAWSPricing_WithCustomRegion_ReturnsCorrectRegion()
    {
        // Act
        var pricing = DefaultPricingData.GetAWSPricing("eu-west-1");

        // Assert
        pricing.Should().NotBeNull();
        pricing.Region.Should().Be("eu-west-1");
        pricing.Provider.Should().Be(CloudProvider.AWS);
    }

    [Fact]
    public void GetAWSPricing_HasManagedControlPlaneCost()
    {
        // Act
        var pricing = DefaultPricingData.GetAWSPricing();

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetAWSPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetAWSPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("t3.medium");
        pricing.Compute.InstanceTypePrices.Should().ContainKey("m6i.xlarge");
    }

    [Fact]
    public void GetAzurePricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetAzurePricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Azure, "eastus");
    }

    [Fact]
    public void GetAzurePricing_WithCustomRegion_ReturnsCorrectRegion()
    {
        // Act
        var pricing = DefaultPricingData.GetAzurePricing("westeurope");

        // Assert
        pricing.Should().NotBeNull();
        pricing.Region.Should().Be("westeurope");
        pricing.Provider.Should().Be(CloudProvider.Azure);
    }

    [Fact]
    public void GetAzurePricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetAzurePricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("Standard_D4s_v5");
        pricing.Compute.InstanceTypePrices.Should().ContainKey("Standard_B2ms");
    }

    [Fact]
    public void GetGCPPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetGCPPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.GCP, "us-central1");
    }

    [Fact]
    public void GetGCPPricing_WithCustomRegion_ReturnsCorrectRegion()
    {
        // Act
        var pricing = DefaultPricingData.GetGCPPricing("europe-west1");

        // Assert
        pricing.Should().NotBeNull();
        pricing.Region.Should().Be("europe-west1");
        pricing.Provider.Should().Be(CloudProvider.GCP);
    }

    [Fact]
    public void GetGCPPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetGCPPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("n2-standard-4");
        pricing.Compute.InstanceTypePrices.Should().ContainKey("e2-standard-2");
    }

    [Fact]
    public void GetOCIPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetOCIPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.OCI, "us-ashburn-1");
    }

    [Fact]
    public void GetOCIPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetOCIPricing();

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetOCIPricing_HasLowerEgressCost()
    {
        // Act
        var ociPricing = DefaultPricingData.GetOCIPricing();
        var awsPricing = DefaultPricingData.GetAWSPricing();

        // Assert - OCI is known for lower egress costs
        ociPricing.Network.EgressPerGB.Should().BeLessThan(awsPricing.Network.EgressPerGB);
    }

    [Fact]
    public void GetIBMPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetIBMPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.IBM, "us-south");
    }

    [Fact]
    public void GetIBMPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetIBMPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("bx2-4x16");
        pricing.Compute.InstanceTypePrices.Should().ContainKey("cx2-4x8");
    }

    [Fact]
    public void GetAlibabaPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetAlibabaPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Alibaba, "cn-hangzhou");
    }

    [Fact]
    public void GetAlibabaPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetAlibabaPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("ecs.g6.xlarge");
    }

    [Fact]
    public void GetTencentPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetTencentPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Tencent, "ap-guangzhou");
    }

    [Fact]
    public void GetTencentPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetTencentPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("S5.LARGE8");
    }

    [Fact]
    public void GetHuaweiPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetHuaweiPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Huawei, "cn-north-4");
    }

    [Fact]
    public void GetHuaweiPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetHuaweiPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("s6.xlarge.2");
    }

    #endregion

    #region Managed OpenShift Services Tests

    [Fact]
    public void GetROSAPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetROSAPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.ROSA, "us-east-1");
    }

    [Fact]
    public void GetROSAPricing_HasOpenShiftServiceFee()
    {
        // Act
        var pricing = DefaultPricingData.GetROSAPricing();

        // Assert
        pricing.Compute.OpenShiftServiceFeePerWorkerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetROSAPricing_HasNoLicenseCost()
    {
        // Act
        var pricing = DefaultPricingData.GetROSAPricing();

        // Assert - License is included in service fee
        pricing.Licenses.OpenShiftPerNodeYear.Should().Be(0);
    }

    [Fact]
    public void GetROSAPricing_HasControlPlaneCost()
    {
        // Act
        var pricing = DefaultPricingData.GetROSAPricing();

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetAROPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetAROPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.ARO, "eastus");
    }

    [Fact]
    public void GetAROPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetAROPricing();

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetAROPricing_HasOpenShiftServiceFee()
    {
        // Act
        var pricing = DefaultPricingData.GetAROPricing();

        // Assert
        pricing.Compute.OpenShiftServiceFeePerWorkerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetOSDPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetOSDPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.OSD, "us-central1");
    }

    [Fact]
    public void GetOSDPricing_HasOpenShiftServiceFee()
    {
        // Act
        var pricing = DefaultPricingData.GetOSDPricing();

        // Assert
        pricing.Compute.OpenShiftServiceFeePerWorkerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetROKSPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetROKSPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.ROKS, "us-south");
    }

    [Fact]
    public void GetROKSPricing_HasOpenShiftServiceFee()
    {
        // Act
        var pricing = DefaultPricingData.GetROKSPricing();

        // Assert
        pricing.Compute.OpenShiftServiceFeePerWorkerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetROKSPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetROKSPricing();

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    #endregion

    #region Developer Cloud Providers Tests

    [Fact]
    public void GetDigitalOceanPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetDigitalOceanPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.DigitalOcean, "nyc1");
    }

    [Fact]
    public void GetDigitalOceanPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetDigitalOceanPricing();

        // Assert - DOKS control plane is free
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetDigitalOceanPricing_HasCompetitiveEgress()
    {
        // Act
        var pricing = DefaultPricingData.GetDigitalOceanPricing();

        // Assert - DigitalOcean has very competitive egress pricing
        pricing.Network.EgressPerGB.Should().BeLessThanOrEqualTo(0.01m);
    }

    [Fact]
    public void GetLinodePricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetLinodePricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Linode, "us-east");
    }

    [Fact]
    public void GetLinodePricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetLinodePricing();

        // Assert - LKE control plane is free
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetLinodePricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetLinodePricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("g6-standard-4");
    }

    [Fact]
    public void GetVultrPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetVultrPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Vultr, "ewr");
    }

    [Fact]
    public void GetVultrPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetVultrPricing();

        // Assert - VKE is free
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetHetznerPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetHetznerPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Hetzner, "fsn1");
    }

    [Fact]
    public void GetHetznerPricing_IsVeryCompetitive()
    {
        // Act
        var hetznerPricing = DefaultPricingData.GetHetznerPricing();
        var awsPricing = DefaultPricingData.GetAWSPricing();

        // Assert - Hetzner is known for very competitive pricing
        hetznerPricing.Compute.CpuPerHour.Should().BeLessThan(awsPricing.Compute.CpuPerHour);
        hetznerPricing.Network.EgressPerGB.Should().BeLessThan(awsPricing.Network.EgressPerGB);
    }

    [Fact]
    public void GetHetznerPricing_HasInstanceTypePrices()
    {
        // Act
        var pricing = DefaultPricingData.GetHetznerPricing();

        // Assert
        pricing.Compute.InstanceTypePrices.Should().NotBeEmpty();
        pricing.Compute.InstanceTypePrices.Should().ContainKey("cx41");
        pricing.Compute.InstanceTypePrices.Should().ContainKey("ccx23");
    }

    [Fact]
    public void GetOVHPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetOVHPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.OVH, "gra");
    }

    [Fact]
    public void GetOVHPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetOVHPricing();

        // Assert - OVH Managed K8s is free
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetScalewayPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetScalewayPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Scaleway, "fr-par");
    }

    [Fact]
    public void GetScalewayPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetScalewayPricing();

        // Assert - Kapsule is free
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetCivoPricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetCivoPricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Civo, "lon1");
    }

    [Fact]
    public void GetCivoPricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetCivoPricing();

        // Assert - Free K3s control plane
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetExoscalePricing_WithDefaultRegion_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetExoscalePricing();

        // Assert
        AssertValidPricingModel(pricing, CloudProvider.Exoscale, "ch-gva-2");
    }

    [Fact]
    public void GetExoscalePricing_HasFreeControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetExoscalePricing();

        // Assert - Free SKS control plane
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    #endregion

    #region On-Premises Tests

    [Fact]
    public void GetOnPremPricing_ReturnsValidPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetOnPremPricing();

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.OnPrem);
        pricing.Region.Should().Be("On-Premises");
        pricing.Compute.Should().NotBeNull();
        pricing.Storage.Should().NotBeNull();
        pricing.Network.Should().NotBeNull();
        pricing.Licenses.Should().NotBeNull();
        pricing.Support.Should().NotBeNull();
    }

    [Fact]
    public void GetOnPremPricing_HasNoManagedControlPlane()
    {
        // Act
        var pricing = DefaultPricingData.GetOnPremPricing();

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().Be(0);
    }

    [Fact]
    public void GetOnPremPricing_IsNotLive()
    {
        // Act
        var pricing = DefaultPricingData.GetOnPremPricing();

        // Assert
        pricing.IsLive.Should().BeFalse();
    }

    #endregion

    #region GetDefaultPricing Method Tests

    [Theory]
    [InlineData(CloudProvider.AWS, "us-east-1")]
    [InlineData(CloudProvider.Azure, "eastus")]
    [InlineData(CloudProvider.GCP, "us-central1")]
    [InlineData(CloudProvider.OCI, "us-ashburn-1")]
    [InlineData(CloudProvider.IBM, "us-south")]
    [InlineData(CloudProvider.Alibaba, "cn-hangzhou")]
    [InlineData(CloudProvider.Tencent, "ap-guangzhou")]
    [InlineData(CloudProvider.Huawei, "cn-north-4")]
    public void GetDefaultPricing_MajorProviders_ReturnsCorrectProvider(CloudProvider provider, string expectedRegion)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
        pricing.Region.Should().Be(expectedRegion);
    }

    [Theory]
    [InlineData(CloudProvider.ROSA, "us-east-1")]
    [InlineData(CloudProvider.ARO, "eastus")]
    [InlineData(CloudProvider.OSD, "us-central1")]
    [InlineData(CloudProvider.ROKS, "us-south")]
    public void GetDefaultPricing_ManagedOpenShift_ReturnsCorrectProvider(CloudProvider provider, string expectedRegion)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
        pricing.Region.Should().Be(expectedRegion);
    }

    [Theory]
    [InlineData(CloudProvider.DigitalOcean, "nyc1")]
    [InlineData(CloudProvider.Linode, "us-east")]
    [InlineData(CloudProvider.Vultr, "ewr")]
    [InlineData(CloudProvider.Hetzner, "fsn1")]
    [InlineData(CloudProvider.OVH, "gra")]
    [InlineData(CloudProvider.Scaleway, "fr-par")]
    [InlineData(CloudProvider.Civo, "lon1")]
    [InlineData(CloudProvider.Exoscale, "ch-gva-2")]
    public void GetDefaultPricing_DeveloperProviders_ReturnsCorrectProvider(CloudProvider provider, string expectedRegion)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
        pricing.Region.Should().Be(expectedRegion);
    }

    [Fact]
    public void GetDefaultPricing_OnPrem_ReturnsOnPremPricing()
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(CloudProvider.OnPrem);

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.OnPrem);
    }

    [Fact]
    public void GetDefaultPricing_WithCustomRegion_UsesCustomRegion()
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(CloudProvider.AWS, "eu-west-1");

        // Assert
        pricing.Region.Should().Be("eu-west-1");
    }

    [Fact]
    public void GetDefaultPricing_WithNullRegion_UsesDefaultRegion()
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(CloudProvider.AWS, null);

        // Assert
        pricing.Region.Should().Be("us-east-1");
    }

    #endregion

    #region PricingModel Properties Tests

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveValidComputePricing(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Compute.Should().NotBeNull();
        pricing.Compute.CpuPerHour.Should().BeGreaterThanOrEqualTo(0);
        pricing.Compute.RamGBPerHour.Should().BeGreaterThanOrEqualTo(0);
        pricing.Compute.ManagedControlPlanePerHour.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveValidStoragePricing(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Storage.Should().NotBeNull();
        pricing.Storage.SsdPerGBMonth.Should().BeGreaterThan(0);
        pricing.Storage.HddPerGBMonth.Should().BeGreaterThanOrEqualTo(0);
        pricing.Storage.ObjectStoragePerGBMonth.Should().BeGreaterThanOrEqualTo(0);
        pricing.Storage.BackupPerGBMonth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveValidNetworkPricing(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Network.Should().NotBeNull();
        pricing.Network.EgressPerGB.Should().BeGreaterThanOrEqualTo(0);
        pricing.Network.LoadBalancerPerHour.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveValidLicensePricing(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Licenses.Should().NotBeNull();
        // Note: License costs can be 0 for managed services
        pricing.Licenses.OpenShiftPerNodeYear.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveValidSupportPricing(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Support.Should().NotBeNull();
        pricing.Support.BasicSupportPercent.Should().BeGreaterThanOrEqualTo(0);
        pricing.Support.DeveloperSupportPercent.Should().BeGreaterThanOrEqualTo(0);
        pricing.Support.BusinessSupportPercent.Should().BeGreaterThanOrEqualTo(0);
        pricing.Support.EnterpriseSupportPercent.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_UseUSDCurrency(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Currency.Should().Be(Currency.USD);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_UseOnDemandPricingType(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.PricingType.Should().Be(PricingType.OnDemand);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_AreNotLive(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.IsLive.Should().BeFalse();
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveSource(CloudProvider provider)
    {
        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);

        // Assert
        pricing.Source.Should().NotBeNullOrEmpty();
        pricing.Source.Should().Contain("Default");
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.ROSA)]
    [InlineData(CloudProvider.ARO)]
    [InlineData(CloudProvider.OSD)]
    [InlineData(CloudProvider.ROKS)]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.OnPrem)]
    public void AllProviders_HaveLastUpdated(CloudProvider provider)
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var pricing = DefaultPricingData.GetDefaultPricing(provider);
        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        pricing.LastUpdated.Should().BeAfter(before);
        pricing.LastUpdated.Should().BeBefore(after);
    }

    #endregion

    #region Region-Specific Pricing Tests

    [Theory]
    [InlineData("us-east-1")]
    [InlineData("us-west-2")]
    [InlineData("eu-west-1")]
    [InlineData("ap-southeast-1")]
    public void GetAWSPricing_DifferentRegions_ReturnsConsistentPricing(string region)
    {
        // Act
        var pricing = DefaultPricingData.GetAWSPricing(region);

        // Assert
        pricing.Region.Should().Be(region);
        pricing.Compute.CpuPerHour.Should().BeGreaterThan(0);
        pricing.Compute.RamGBPerHour.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("eastus")]
    [InlineData("westus2")]
    [InlineData("westeurope")]
    public void GetAzurePricing_DifferentRegions_ReturnsConsistentPricing(string region)
    {
        // Act
        var pricing = DefaultPricingData.GetAzurePricing(region);

        // Assert
        pricing.Region.Should().Be(region);
        pricing.Compute.CpuPerHour.Should().BeGreaterThan(0);
        pricing.Compute.RamGBPerHour.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("us-central1")]
    [InlineData("us-east1")]
    [InlineData("europe-west1")]
    public void GetGCPPricing_DifferentRegions_ReturnsConsistentPricing(string region)
    {
        // Act
        var pricing = DefaultPricingData.GetGCPPricing(region);

        // Assert
        pricing.Region.Should().Be(region);
        pricing.Compute.CpuPerHour.Should().BeGreaterThan(0);
        pricing.Compute.RamGBPerHour.Should().BeGreaterThan(0);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Asserts that a PricingModel has all required properties populated correctly
    /// </summary>
    private static void AssertValidPricingModel(PricingModel pricing, CloudProvider expectedProvider, string expectedRegion)
    {
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(expectedProvider);
        pricing.Region.Should().Be(expectedRegion);
        pricing.Currency.Should().Be(Currency.USD);
        pricing.PricingType.Should().Be(PricingType.OnDemand);
        pricing.IsLive.Should().BeFalse();
        pricing.Source.Should().NotBeNullOrEmpty();

        // Compute pricing
        pricing.Compute.Should().NotBeNull();
        pricing.Compute.CpuPerHour.Should().BeGreaterThanOrEqualTo(0);
        pricing.Compute.RamGBPerHour.Should().BeGreaterThanOrEqualTo(0);

        // Storage pricing
        pricing.Storage.Should().NotBeNull();
        pricing.Storage.SsdPerGBMonth.Should().BeGreaterThan(0);

        // Network pricing
        pricing.Network.Should().NotBeNull();
        pricing.Network.EgressPerGB.Should().BeGreaterThanOrEqualTo(0);

        // License pricing
        pricing.Licenses.Should().NotBeNull();

        // Support pricing
        pricing.Support.Should().NotBeNull();
    }

    #endregion
}
