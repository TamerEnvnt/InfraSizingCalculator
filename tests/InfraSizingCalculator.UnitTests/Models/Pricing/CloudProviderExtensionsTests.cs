using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

/// <summary>
/// Tests for CloudProviderExtensions covering all three extension methods:
/// - GetCloudProvider(Distribution)
/// - GetCrossAZCostMultiplier(CloudProvider, int)
/// - GetControlPlaneCostPerHour(CloudProvider, bool)
/// </summary>
public class CloudProviderExtensionsTests
{
    #region GetCloudProvider - On-Premises Distributions

    [Theory]
    [InlineData(Distribution.OpenShift, CloudProvider.OnPrem)]
    [InlineData(Distribution.Kubernetes, CloudProvider.OnPrem)]
    [InlineData(Distribution.Rancher, CloudProvider.OnPrem)]
    [InlineData(Distribution.RKE2, CloudProvider.OnPrem)]
    [InlineData(Distribution.K3s, CloudProvider.OnPrem)]
    [InlineData(Distribution.MicroK8s, CloudProvider.OnPrem)]
    [InlineData(Distribution.Charmed, CloudProvider.OnPrem)]
    [InlineData(Distribution.Tanzu, CloudProvider.OnPrem)]
    public void GetCloudProvider_OnPremDistributions_ReturnsOnPrem(Distribution distribution, CloudProvider expected)
    {
        var result = distribution.GetCloudProvider();
        result.Should().Be(expected);
    }

    #endregion

    #region GetCloudProvider - AWS Distributions

    [Theory]
    [InlineData(Distribution.EKS, CloudProvider.AWS)]
    [InlineData(Distribution.RancherEKS, CloudProvider.AWS)]
    [InlineData(Distribution.TanzuAWS, CloudProvider.AWS)]
    [InlineData(Distribution.CharmedAWS, CloudProvider.AWS)]
    [InlineData(Distribution.MicroK8sAWS, CloudProvider.AWS)]
    [InlineData(Distribution.K3sAWS, CloudProvider.AWS)]
    [InlineData(Distribution.RKE2AWS, CloudProvider.AWS)]
    public void GetCloudProvider_AWSDistributions_ReturnsAWS(Distribution distribution, CloudProvider expected)
    {
        var result = distribution.GetCloudProvider();
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCloudProvider_OpenShiftROSA_ReturnsROSA()
    {
        var result = Distribution.OpenShiftROSA.GetCloudProvider();
        result.Should().Be(CloudProvider.ROSA);
    }

    #endregion

    #region GetCloudProvider - Azure Distributions

    [Theory]
    [InlineData(Distribution.AKS, CloudProvider.Azure)]
    [InlineData(Distribution.RancherAKS, CloudProvider.Azure)]
    [InlineData(Distribution.TanzuAzure, CloudProvider.Azure)]
    [InlineData(Distribution.CharmedAzure, CloudProvider.Azure)]
    [InlineData(Distribution.MicroK8sAzure, CloudProvider.Azure)]
    [InlineData(Distribution.K3sAzure, CloudProvider.Azure)]
    [InlineData(Distribution.RKE2Azure, CloudProvider.Azure)]
    public void GetCloudProvider_AzureDistributions_ReturnsAzure(Distribution distribution, CloudProvider expected)
    {
        var result = distribution.GetCloudProvider();
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCloudProvider_OpenShiftARO_ReturnsARO()
    {
        var result = Distribution.OpenShiftARO.GetCloudProvider();
        result.Should().Be(CloudProvider.ARO);
    }

    #endregion

    #region GetCloudProvider - GCP Distributions

    [Theory]
    [InlineData(Distribution.GKE, CloudProvider.GCP)]
    [InlineData(Distribution.RancherGKE, CloudProvider.GCP)]
    [InlineData(Distribution.TanzuGCP, CloudProvider.GCP)]
    [InlineData(Distribution.CharmedGCP, CloudProvider.GCP)]
    [InlineData(Distribution.MicroK8sGCP, CloudProvider.GCP)]
    [InlineData(Distribution.K3sGCP, CloudProvider.GCP)]
    [InlineData(Distribution.RKE2GCP, CloudProvider.GCP)]
    public void GetCloudProvider_GCPDistributions_ReturnsGCP(Distribution distribution, CloudProvider expected)
    {
        var result = distribution.GetCloudProvider();
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCloudProvider_OpenShiftDedicated_ReturnsOSD()
    {
        var result = Distribution.OpenShiftDedicated.GetCloudProvider();
        result.Should().Be(CloudProvider.OSD);
    }

    #endregion

    #region GetCloudProvider - Other Major Clouds

    [Fact]
    public void GetCloudProvider_OKE_ReturnsOCI()
    {
        var result = Distribution.OKE.GetCloudProvider();
        result.Should().Be(CloudProvider.OCI);
    }

    [Fact]
    public void GetCloudProvider_IKS_ReturnsIBM()
    {
        var result = Distribution.IKS.GetCloudProvider();
        result.Should().Be(CloudProvider.IBM);
    }

    [Fact]
    public void GetCloudProvider_OpenShiftIBM_ReturnsROKS()
    {
        var result = Distribution.OpenShiftIBM.GetCloudProvider();
        result.Should().Be(CloudProvider.ROKS);
    }

    [Fact]
    public void GetCloudProvider_ACK_ReturnsAlibaba()
    {
        var result = Distribution.ACK.GetCloudProvider();
        result.Should().Be(CloudProvider.Alibaba);
    }

    [Fact]
    public void GetCloudProvider_TKE_ReturnsTencent()
    {
        var result = Distribution.TKE.GetCloudProvider();
        result.Should().Be(CloudProvider.Tencent);
    }

    [Fact]
    public void GetCloudProvider_CCE_ReturnsHuawei()
    {
        var result = Distribution.CCE.GetCloudProvider();
        result.Should().Be(CloudProvider.Huawei);
    }

    [Fact]
    public void GetCloudProvider_DOKS_ReturnsDigitalOcean()
    {
        var result = Distribution.DOKS.GetCloudProvider();
        result.Should().Be(CloudProvider.DigitalOcean);
    }

    [Fact]
    public void GetCloudProvider_LKE_ReturnsLinode()
    {
        var result = Distribution.LKE.GetCloudProvider();
        result.Should().Be(CloudProvider.Linode);
    }

    #endregion

    #region GetCloudProvider - Smaller/Special Clouds

    [Fact]
    public void GetCloudProvider_VKE_ReturnsVultr()
    {
        var result = Distribution.VKE.GetCloudProvider();
        result.Should().Be(CloudProvider.Vultr);
    }

    [Fact]
    public void GetCloudProvider_HetznerK8s_ReturnsHetzner()
    {
        var result = Distribution.HetznerK8s.GetCloudProvider();
        result.Should().Be(CloudProvider.Hetzner);
    }

    [Fact]
    public void GetCloudProvider_OVHKubernetes_ReturnsOVH()
    {
        var result = Distribution.OVHKubernetes.GetCloudProvider();
        result.Should().Be(CloudProvider.OVH);
    }

    [Fact]
    public void GetCloudProvider_ScalewayKapsule_ReturnsScaleway()
    {
        var result = Distribution.ScalewayKapsule.GetCloudProvider();
        result.Should().Be(CloudProvider.Scaleway);
    }

    [Fact]
    public void GetCloudProvider_RancherHosted_ReturnsManual()
    {
        var result = Distribution.RancherHosted.GetCloudProvider();
        result.Should().Be(CloudProvider.Manual);
    }

    [Fact]
    public void GetCloudProvider_TanzuCloud_ReturnsManual()
    {
        var result = Distribution.TanzuCloud.GetCloudProvider();
        result.Should().Be(CloudProvider.Manual);
    }

    #endregion

    #region GetCrossAZCostMultiplier - Single AZ

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OnPrem)]
    [InlineData(CloudProvider.OCI)]
    public void GetCrossAZCostMultiplier_SingleAZ_ReturnsZero(CloudProvider provider)
    {
        var result = provider.GetCrossAZCostMultiplier(1);
        result.Should().Be(0m);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    public void GetCrossAZCostMultiplier_ZeroAZ_ReturnsZero(CloudProvider provider)
    {
        var result = provider.GetCrossAZCostMultiplier(0);
        result.Should().Be(0m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - Azure (FREE)

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void GetCrossAZCostMultiplier_Azure_AlwaysFree(int azCount)
    {
        var result = CloudProvider.Azure.GetCrossAZCostMultiplier(azCount);
        result.Should().Be(0m);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void GetCrossAZCostMultiplier_ARO_AlwaysFree(int azCount)
    {
        var result = CloudProvider.ARO.GetCrossAZCostMultiplier(azCount);
        result.Should().Be(0m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - OnPrem (No Cost)

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void GetCrossAZCostMultiplier_OnPrem_AlwaysZero(int azCount)
    {
        var result = CloudProvider.OnPrem.GetCrossAZCostMultiplier(azCount);
        result.Should().Be(0m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - AWS

    [Fact]
    public void GetCrossAZCostMultiplier_AWS_TwoAZ_Returns2Percent()
    {
        var result = CloudProvider.AWS.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_AWS_ThreeAZ_Returns3Percent()
    {
        var result = CloudProvider.AWS.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_ROSA_TwoAZ_Returns2Percent()
    {
        var result = CloudProvider.ROSA.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_ROSA_ThreeAZ_Returns3Percent()
    {
        var result = CloudProvider.ROSA.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - GCP

    [Fact]
    public void GetCrossAZCostMultiplier_GCP_TwoAZ_Returns2Percent()
    {
        var result = CloudProvider.GCP.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_OSD_ThreeAZ_Returns3Percent()
    {
        var result = CloudProvider.OSD.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - OCI

    [Fact]
    public void GetCrossAZCostMultiplier_OCI_TwoAZ_Returns1Point5Percent()
    {
        var result = CloudProvider.OCI.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.015m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_OCI_ThreeAZ_Returns2Point5Percent()
    {
        var result = CloudProvider.OCI.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.025m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - IBM

    [Fact]
    public void GetCrossAZCostMultiplier_IBM_TwoAZ_Returns2Percent()
    {
        var result = CloudProvider.IBM.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_ROKS_ThreeAZ_Returns3Percent()
    {
        var result = CloudProvider.ROKS.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    #endregion

    #region GetCrossAZCostMultiplier - Default Case

    [Fact]
    public void GetCrossAZCostMultiplier_UnknownProvider_UsesDefaultAWSLike()
    {
        var result = CloudProvider.Alibaba.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_Manual_UsesDefault()
    {
        var result = CloudProvider.Manual.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - AWS

    [Fact]
    public void GetControlPlaneCostPerHour_AWS_Standard_Returns10Cents()
    {
        var result = CloudProvider.AWS.GetControlPlaneCostPerHour(isHA: false);
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_AWS_HA_Returns10Cents()
    {
        var result = CloudProvider.AWS.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_ROSA_Returns10Cents()
    {
        var result = CloudProvider.ROSA.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - Azure (Free tier available)

    [Fact]
    public void GetControlPlaneCostPerHour_Azure_Standard_ReturnsFree()
    {
        var result = CloudProvider.Azure.GetControlPlaneCostPerHour(isHA: false);
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Azure_HA_Returns10Cents()
    {
        var result = CloudProvider.Azure.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_ARO_Standard_ReturnsFree()
    {
        var result = CloudProvider.ARO.GetControlPlaneCostPerHour(isHA: false);
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_ARO_HA_Returns10Cents()
    {
        var result = CloudProvider.ARO.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.10m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - GCP

    [Fact]
    public void GetControlPlaneCostPerHour_GCP_Returns10Cents()
    {
        var result = CloudProvider.GCP.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_OSD_Returns10Cents()
    {
        var result = CloudProvider.OSD.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.10m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - OCI (Free basic tier)

    [Fact]
    public void GetControlPlaneCostPerHour_OCI_Standard_ReturnsFree()
    {
        var result = CloudProvider.OCI.GetControlPlaneCostPerHour(isHA: false);
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_OCI_HA_Returns10Cents()
    {
        var result = CloudProvider.OCI.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.10m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - IBM (Free)

    [Fact]
    public void GetControlPlaneCostPerHour_IBM_ReturnsFree()
    {
        var result = CloudProvider.IBM.GetControlPlaneCostPerHour();
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_ROKS_ReturnsFree()
    {
        var result = CloudProvider.ROKS.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - DigitalOcean

    [Fact]
    public void GetControlPlaneCostPerHour_DigitalOcean_Standard_ReturnsFree()
    {
        var result = CloudProvider.DigitalOcean.GetControlPlaneCostPerHour(isHA: false);
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_DigitalOcean_HA_Returns40PerMonth()
    {
        var result = CloudProvider.DigitalOcean.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.055m); // ~$40/mo
    }

    #endregion

    #region GetControlPlaneCostPerHour - Linode

    [Fact]
    public void GetControlPlaneCostPerHour_Linode_Standard_ReturnsFree()
    {
        var result = CloudProvider.Linode.GetControlPlaneCostPerHour(isHA: false);
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Linode_HA_Returns60PerMonth()
    {
        var result = CloudProvider.Linode.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.083m); // ~$60/mo
    }

    #endregion

    #region GetControlPlaneCostPerHour - OnPrem

    [Fact]
    public void GetControlPlaneCostPerHour_OnPrem_ReturnsFree()
    {
        var result = CloudProvider.OnPrem.GetControlPlaneCostPerHour();
        result.Should().Be(0m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_OnPrem_HA_StillFree()
    {
        var result = CloudProvider.OnPrem.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0m);
    }

    #endregion

    #region GetControlPlaneCostPerHour - Default Case

    [Fact]
    public void GetControlPlaneCostPerHour_Alibaba_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Alibaba.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Tencent_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Tencent.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Manual_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Manual.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    #endregion

    #region Default Parameter Tests

    [Fact]
    public void GetControlPlaneCostPerHour_DefaultParameter_IsFalse()
    {
        // Without specifying isHA, should use default (false)
        var result = CloudProvider.Azure.GetControlPlaneCostPerHour();
        result.Should().Be(0m); // Azure free tier for non-HA
    }

    #endregion

    #region GetCloudProvider - Undefined Distribution (Default Case)

    [Fact]
    public void GetCloudProvider_UndefinedDistribution_ReturnsManual()
    {
        // Arrange - Cast an undefined integer to Distribution to test default case
        var undefinedDistribution = (Distribution)999;

        // Act
        var result = undefinedDistribution.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Manual);
    }

    #endregion

    #region Additional Edge Case Coverage

    [Fact]
    public void GetCrossAZCostMultiplier_NegativeAZCount_ReturnsZero()
    {
        // Negative AZ count should be treated as <= 1
        var result = CloudProvider.AWS.GetCrossAZCostMultiplier(-1);
        result.Should().Be(0m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_Vultr_UsesDefaultAWSLike()
    {
        var result = CloudProvider.Vultr.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_Hetzner_UsesDefaultAWSLike()
    {
        var result = CloudProvider.Hetzner.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Vultr_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Vultr.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Hetzner_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Hetzner.GetControlPlaneCostPerHour(isHA: true);
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Huawei_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Huawei.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_Scaleway_UsesDefaultAWSLike()
    {
        var result = CloudProvider.Scaleway.GetCrossAZCostMultiplier(2);
        result.Should().Be(0.02m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_Civo_UsesDefaultAWSLike()
    {
        var result = CloudProvider.Civo.GetCrossAZCostMultiplier(3);
        result.Should().Be(0.03m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Scaleway_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Scaleway.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Exoscale_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Exoscale.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_OVH_DefaultsToAWSPricing()
    {
        var result = CloudProvider.OVH.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_Civo_DefaultsToAWSPricing()
    {
        var result = CloudProvider.Civo.GetControlPlaneCostPerHour();
        result.Should().Be(0.10m);
    }

    #endregion
}
