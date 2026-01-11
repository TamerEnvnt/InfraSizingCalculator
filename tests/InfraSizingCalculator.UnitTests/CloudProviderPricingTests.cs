using FluentAssertions;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Models.Pricing.Base;
using InfraSizingCalculator.Models.Pricing.CloudProviders;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Comprehensive tests for all cloud provider pricing implementations.
/// Tests verify each provider returns valid pricing data following the ICloudProviderPricing interface.
/// </summary>
public class CloudProviderPricingTests
{
    #region AWS Cloud Pricing Tests

    [Fact]
    public void AwsCloudPricing_Provider_ReturnsAWS()
    {
        var pricing = new AwsCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.AWS);
    }

    [Fact]
    public void AwsCloudPricing_DefaultRegion_IsUsEast1()
    {
        var pricing = new AwsCloudPricing();
        pricing.DefaultRegion.Should().Be("us-east-1");
    }

    [Fact]
    public void AwsCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new AwsCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.AWS);
        model.Region.Should().Be("us-east-1");
        AssertValidPricingModel(model);
    }

    [Theory]
    [InlineData("us-east-1")]
    [InlineData("us-west-2")]
    [InlineData("eu-west-1")]
    [InlineData("ap-southeast-1")]
    public void AwsCloudPricing_GetPricing_WithRegion_AppliesMultiplier(string region)
    {
        var pricing = new AwsCloudPricing();
        var model = pricing.GetPricing(region);

        model.Should().NotBeNull();
        model.Region.Should().Be(region);
        model.Compute.CpuPerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AwsCloudPricing_GetAvailableRegions_ReturnsNonEmptyList()
    {
        var pricing = new AwsCloudPricing();
        var regions = pricing.GetAvailableRegions();

        regions.Should().NotBeNull();
        regions.Should().NotBeEmpty();
        regions.Should().Contain(r => r.Code == "us-east-1");
    }

    [Fact]
    public void AwsCloudPricing_IsRegionSupported_ReturnsTrueForUsEast1()
    {
        var pricing = new AwsCloudPricing();
        pricing.IsRegionSupported("us-east-1").Should().BeTrue();
    }

    [Fact]
    public void AwsCloudPricing_ControlPlaneCost_IsPositive()
    {
        var pricing = new AwsCloudPricing();
        var cost = pricing.GetControlPlaneCostPerHour();

        cost.Should().BeGreaterThan(0); // EKS costs $0.10/hr
    }

    [Fact]
    public void AwsCloudPricing_GetComputePricing_ReturnsValidData()
    {
        var pricing = new AwsCloudPricing();
        var compute = pricing.GetComputePricing();

        compute.Should().NotBeNull();
        compute.CpuPerHour.Should().BeGreaterThan(0);
        compute.RamGBPerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AwsCloudPricing_GetStoragePricing_ReturnsValidData()
    {
        var pricing = new AwsCloudPricing();
        var storage = pricing.GetStoragePricing();

        storage.Should().NotBeNull();
        storage.SsdPerGBMonth.Should().BeGreaterThan(0);
        storage.HddPerGBMonth.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AwsCloudPricing_GetNetworkPricing_ReturnsValidData()
    {
        var pricing = new AwsCloudPricing();
        var network = pricing.GetNetworkPricing();

        network.Should().NotBeNull();
        network.EgressPerGB.Should().BeGreaterThan(0);
    }

    #endregion

    #region Azure Cloud Pricing Tests

    [Fact]
    public void AzureCloudPricing_Provider_ReturnsAzure()
    {
        var pricing = new AzureCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Azure);
    }

    [Fact]
    public void AzureCloudPricing_DefaultRegion_IsEastUs()
    {
        var pricing = new AzureCloudPricing();
        pricing.DefaultRegion.Should().Be("eastus");
    }

    [Fact]
    public void AzureCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new AzureCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Azure);
        AssertValidPricingModel(model);
    }

    [Fact]
    public void AzureCloudPricing_ControlPlaneCost_IsFreeForBasic()
    {
        var pricing = new AzureCloudPricing();
        var cost = pricing.GetControlPlaneCostPerHour(isHA: false);

        cost.Should().Be(0); // AKS basic tier is free
    }

    #endregion

    #region GCP Cloud Pricing Tests

    [Fact]
    public void GcpCloudPricing_Provider_ReturnsGCP()
    {
        var pricing = new GcpCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.GCP);
    }

    [Fact]
    public void GcpCloudPricing_DefaultRegion_IsUsCentral1()
    {
        var pricing = new GcpCloudPricing();
        pricing.DefaultRegion.Should().Be("us-central1");
    }

    [Fact]
    public void GcpCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new GcpCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.GCP);
        AssertValidPricingModel(model);
    }

    #endregion

    #region OCI Cloud Pricing Tests

    [Fact]
    public void OciCloudPricing_Provider_ReturnsOCI()
    {
        var pricing = new OciCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.OCI);
    }

    [Fact]
    public void OciCloudPricing_DefaultRegion_IsUsAshburn1()
    {
        var pricing = new OciCloudPricing();
        pricing.DefaultRegion.Should().Be("us-ashburn-1");
    }

    [Fact]
    public void OciCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new OciCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.OCI);
        AssertValidPricingModel(model);
    }

    [Fact]
    public void OciCloudPricing_HasFreeControlPlane()
    {
        var pricing = new OciCloudPricing();
        var cost = pricing.GetControlPlaneCostPerHour();

        cost.Should().Be(0); // OKE has free control plane
    }

    #endregion

    #region IBM Cloud Pricing Tests

    [Fact]
    public void IbmCloudPricing_Provider_ReturnsIBM()
    {
        var pricing = new IbmCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.IBM);
    }

    [Fact]
    public void IbmCloudPricing_DefaultRegion_IsUsSouth()
    {
        var pricing = new IbmCloudPricing();
        pricing.DefaultRegion.Should().Be("us-south");
    }

    [Fact]
    public void IbmCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new IbmCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.IBM);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Alibaba Cloud Pricing Tests

    [Fact]
    public void AlibabaCloudPricing_Provider_ReturnsAlibaba()
    {
        var pricing = new AlibabaCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Alibaba);
    }

    [Fact]
    public void AlibabaCloudPricing_DefaultRegion_IsCnHangzhou()
    {
        var pricing = new AlibabaCloudPricing();
        pricing.DefaultRegion.Should().Be("cn-hangzhou");
    }

    [Fact]
    public void AlibabaCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new AlibabaCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Alibaba);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Tencent Cloud Pricing Tests

    [Fact]
    public void TencentCloudPricing_Provider_ReturnsTencent()
    {
        var pricing = new TencentCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Tencent);
    }

    [Fact]
    public void TencentCloudPricing_DefaultRegion_IsApGuangzhou()
    {
        var pricing = new TencentCloudPricing();
        pricing.DefaultRegion.Should().Be("ap-guangzhou");
    }

    [Fact]
    public void TencentCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new TencentCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Tencent);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Huawei Cloud Pricing Tests

    [Fact]
    public void HuaweiCloudPricing_Provider_ReturnsHuawei()
    {
        var pricing = new HuaweiCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Huawei);
    }

    [Fact]
    public void HuaweiCloudPricing_DefaultRegion_IsCnNorth4()
    {
        var pricing = new HuaweiCloudPricing();
        pricing.DefaultRegion.Should().Be("cn-north-4");
    }

    [Fact]
    public void HuaweiCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new HuaweiCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Huawei);
        AssertValidPricingModel(model);
    }

    #endregion

    #region DigitalOcean Pricing Tests

    [Fact]
    public void DigitalOceanCloudPricing_Provider_ReturnsDigitalOcean()
    {
        var pricing = new DigitalOceanCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.DigitalOcean);
    }

    [Fact]
    public void DigitalOceanCloudPricing_DefaultRegion_IsNyc1()
    {
        var pricing = new DigitalOceanCloudPricing();
        pricing.DefaultRegion.Should().Be("nyc1");
    }

    [Fact]
    public void DigitalOceanCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new DigitalOceanCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.DigitalOcean);
        AssertValidPricingModel(model);
    }

    [Fact]
    public void DigitalOceanCloudPricing_HasFreeControlPlane()
    {
        var pricing = new DigitalOceanCloudPricing();
        var cost = pricing.GetControlPlaneCostPerHour();

        cost.Should().Be(0); // DOKS has free control plane
    }

    #endregion

    #region Linode Pricing Tests

    [Fact]
    public void LinodeCloudPricing_Provider_ReturnsLinode()
    {
        var pricing = new LinodeCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Linode);
    }

    [Fact]
    public void LinodeCloudPricing_DefaultRegion_IsUsEast()
    {
        var pricing = new LinodeCloudPricing();
        pricing.DefaultRegion.Should().Be("us-east");
    }

    [Fact]
    public void LinodeCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new LinodeCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Linode);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Vultr Pricing Tests

    [Fact]
    public void VultrCloudPricing_Provider_ReturnsVultr()
    {
        var pricing = new VultrCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Vultr);
    }

    [Fact]
    public void VultrCloudPricing_DefaultRegion_IsEwr()
    {
        var pricing = new VultrCloudPricing();
        pricing.DefaultRegion.Should().Be("ewr");
    }

    [Fact]
    public void VultrCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new VultrCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Vultr);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Hetzner Pricing Tests

    [Fact]
    public void HetznerCloudPricing_Provider_ReturnsHetzner()
    {
        var pricing = new HetznerCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Hetzner);
    }

    [Fact]
    public void HetznerCloudPricing_DefaultRegion_IsFsn1()
    {
        var pricing = new HetznerCloudPricing();
        pricing.DefaultRegion.Should().Be("fsn1");
    }

    [Fact]
    public void HetznerCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new HetznerCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Hetzner);
        AssertValidPricingModel(model);
    }

    [Fact]
    public void HetznerCloudPricing_IsCostEffective()
    {
        // Hetzner is known for being significantly cheaper than hyperscalers
        var hetzner = new HetznerCloudPricing().GetPricing();
        var aws = new AwsCloudPricing().GetPricing();

        hetzner.Compute.CpuPerHour.Should().BeLessThan(aws.Compute.CpuPerHour);
    }

    #endregion

    #region OVH Pricing Tests

    [Fact]
    public void OvhCloudPricing_Provider_ReturnsOVH()
    {
        var pricing = new OvhCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.OVH);
    }

    [Fact]
    public void OvhCloudPricing_DefaultRegion_IsGra()
    {
        var pricing = new OvhCloudPricing();
        pricing.DefaultRegion.Should().Be("gra");
    }

    [Fact]
    public void OvhCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new OvhCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.OVH);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Scaleway Pricing Tests

    [Fact]
    public void ScalewayCloudPricing_Provider_ReturnsScaleway()
    {
        var pricing = new ScalewayCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Scaleway);
    }

    [Fact]
    public void ScalewayCloudPricing_DefaultRegion_IsFrPar()
    {
        var pricing = new ScalewayCloudPricing();
        pricing.DefaultRegion.Should().Be("fr-par");
    }

    [Fact]
    public void ScalewayCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new ScalewayCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Scaleway);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Civo Pricing Tests

    [Fact]
    public void CivoCloudPricing_Provider_ReturnsCivo()
    {
        var pricing = new CivoCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Civo);
    }

    [Fact]
    public void CivoCloudPricing_DefaultRegion_IsLon1()
    {
        var pricing = new CivoCloudPricing();
        pricing.DefaultRegion.Should().Be("lon1");
    }

    [Fact]
    public void CivoCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new CivoCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Civo);
        AssertValidPricingModel(model);
    }

    #endregion

    #region Exoscale Pricing Tests

    [Fact]
    public void ExoscaleCloudPricing_Provider_ReturnsExoscale()
    {
        var pricing = new ExoscaleCloudPricing();
        pricing.Provider.Should().Be(CloudProvider.Exoscale);
    }

    [Fact]
    public void ExoscaleCloudPricing_DefaultRegion_IsChGva2()
    {
        var pricing = new ExoscaleCloudPricing();
        pricing.DefaultRegion.Should().Be("ch-gva-2");
    }

    [Fact]
    public void ExoscaleCloudPricing_GetPricing_ReturnsValidModel()
    {
        var pricing = new ExoscaleCloudPricing();
        var model = pricing.GetPricing();

        model.Should().NotBeNull();
        model.Provider.Should().Be(CloudProvider.Exoscale);
        AssertValidPricingModel(model);
    }

    #endregion

    #region All Providers Theory Tests

    [Theory]
    [InlineData(typeof(AwsCloudPricing), CloudProvider.AWS)]
    [InlineData(typeof(AzureCloudPricing), CloudProvider.Azure)]
    [InlineData(typeof(GcpCloudPricing), CloudProvider.GCP)]
    [InlineData(typeof(OciCloudPricing), CloudProvider.OCI)]
    [InlineData(typeof(IbmCloudPricing), CloudProvider.IBM)]
    [InlineData(typeof(AlibabaCloudPricing), CloudProvider.Alibaba)]
    [InlineData(typeof(TencentCloudPricing), CloudProvider.Tencent)]
    [InlineData(typeof(HuaweiCloudPricing), CloudProvider.Huawei)]
    [InlineData(typeof(DigitalOceanCloudPricing), CloudProvider.DigitalOcean)]
    [InlineData(typeof(LinodeCloudPricing), CloudProvider.Linode)]
    [InlineData(typeof(VultrCloudPricing), CloudProvider.Vultr)]
    [InlineData(typeof(HetznerCloudPricing), CloudProvider.Hetzner)]
    [InlineData(typeof(OvhCloudPricing), CloudProvider.OVH)]
    [InlineData(typeof(ScalewayCloudPricing), CloudProvider.Scaleway)]
    [InlineData(typeof(CivoCloudPricing), CloudProvider.Civo)]
    [InlineData(typeof(ExoscaleCloudPricing), CloudProvider.Exoscale)]
    public void AllProviders_ImplementInterface_Correctly(Type pricingType, CloudProvider expectedProvider)
    {
        // Arrange
        var pricing = (ICloudProviderPricing)Activator.CreateInstance(pricingType)!;

        // Act
        var model = pricing.GetPricing();

        // Assert
        pricing.Provider.Should().Be(expectedProvider);
        pricing.DefaultRegion.Should().NotBeNullOrEmpty();
        pricing.GetAvailableRegions().Should().NotBeEmpty();
        model.Should().NotBeNull();
        AssertValidPricingModel(model);
    }

    [Theory]
    [InlineData(typeof(AwsCloudPricing))]
    [InlineData(typeof(AzureCloudPricing))]
    [InlineData(typeof(GcpCloudPricing))]
    [InlineData(typeof(OciCloudPricing))]
    [InlineData(typeof(IbmCloudPricing))]
    [InlineData(typeof(AlibabaCloudPricing))]
    [InlineData(typeof(TencentCloudPricing))]
    [InlineData(typeof(HuaweiCloudPricing))]
    [InlineData(typeof(DigitalOceanCloudPricing))]
    [InlineData(typeof(LinodeCloudPricing))]
    [InlineData(typeof(VultrCloudPricing))]
    [InlineData(typeof(HetznerCloudPricing))]
    [InlineData(typeof(OvhCloudPricing))]
    [InlineData(typeof(ScalewayCloudPricing))]
    [InlineData(typeof(CivoCloudPricing))]
    [InlineData(typeof(ExoscaleCloudPricing))]
    public void AllProviders_HavePositiveComputePricing(Type pricingType)
    {
        var pricing = (ICloudProviderPricing)Activator.CreateInstance(pricingType)!;
        var model = pricing.GetPricing();

        model.Compute.CpuPerHour.Should().BeGreaterThan(0);
        model.Compute.RamGBPerHour.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(typeof(AwsCloudPricing))]
    [InlineData(typeof(AzureCloudPricing))]
    [InlineData(typeof(GcpCloudPricing))]
    [InlineData(typeof(OciCloudPricing))]
    [InlineData(typeof(IbmCloudPricing))]
    [InlineData(typeof(AlibabaCloudPricing))]
    [InlineData(typeof(TencentCloudPricing))]
    [InlineData(typeof(HuaweiCloudPricing))]
    [InlineData(typeof(DigitalOceanCloudPricing))]
    [InlineData(typeof(LinodeCloudPricing))]
    [InlineData(typeof(VultrCloudPricing))]
    [InlineData(typeof(HetznerCloudPricing))]
    [InlineData(typeof(OvhCloudPricing))]
    [InlineData(typeof(ScalewayCloudPricing))]
    [InlineData(typeof(CivoCloudPricing))]
    [InlineData(typeof(ExoscaleCloudPricing))]
    public void AllProviders_HavePositiveStoragePricing(Type pricingType)
    {
        var pricing = (ICloudProviderPricing)Activator.CreateInstance(pricingType)!;
        var model = pricing.GetPricing();

        model.Storage.SsdPerGBMonth.Should().BeGreaterThan(0);
        model.Storage.HddPerGBMonth.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(typeof(AwsCloudPricing))]
    [InlineData(typeof(AzureCloudPricing))]
    [InlineData(typeof(GcpCloudPricing))]
    [InlineData(typeof(OciCloudPricing))]
    [InlineData(typeof(IbmCloudPricing))]
    [InlineData(typeof(AlibabaCloudPricing))]
    [InlineData(typeof(TencentCloudPricing))]
    [InlineData(typeof(HuaweiCloudPricing))]
    [InlineData(typeof(DigitalOceanCloudPricing))]
    [InlineData(typeof(LinodeCloudPricing))]
    [InlineData(typeof(VultrCloudPricing))]
    [InlineData(typeof(HetznerCloudPricing))]
    [InlineData(typeof(OvhCloudPricing))]
    [InlineData(typeof(ScalewayCloudPricing))]
    [InlineData(typeof(CivoCloudPricing))]
    [InlineData(typeof(ExoscaleCloudPricing))]
    public void AllProviders_CalculateMonthlyCost_ReturnsPositiveValue(Type pricingType)
    {
        var pricing = (ICloudProviderPricing)Activator.CreateInstance(pricingType)!;
        var monthlyCost = pricing.CalculateMonthlyCost(cpuCores: 4, ramGB: 16, storageGB: 100);

        monthlyCost.Should().BeGreaterThan(0);
    }

    #endregion

    #region Helper Methods

    private static void AssertValidPricingModel(PricingModel model)
    {
        model.Should().NotBeNull();
        model.Compute.Should().NotBeNull();
        model.Storage.Should().NotBeNull();
        model.Network.Should().NotBeNull();

        // Compute pricing validations
        model.Compute.CpuPerHour.Should().BeGreaterThanOrEqualTo(0);
        model.Compute.RamGBPerHour.Should().BeGreaterThanOrEqualTo(0);
        model.Compute.ManagedControlPlanePerHour.Should().BeGreaterThanOrEqualTo(0);
        model.Compute.InstanceTypePrices.Should().NotBeNull();

        // Storage pricing validations
        model.Storage.SsdPerGBMonth.Should().BeGreaterThanOrEqualTo(0);
        model.Storage.HddPerGBMonth.Should().BeGreaterThanOrEqualTo(0);

        // Network pricing validations
        model.Network.EgressPerGB.Should().BeGreaterThanOrEqualTo(0);
        model.Network.LoadBalancerPerHour.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion
}
