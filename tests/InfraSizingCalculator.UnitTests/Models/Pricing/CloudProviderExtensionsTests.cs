using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

/// <summary>
/// Tests for CloudProviderExtensions which maps distributions to cloud providers
/// and calculates cloud-specific pricing factors.
/// </summary>
public class CloudProviderExtensionsTests
{
    #region GetCloudProvider Tests - On-Premises Distributions

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.MicroK8s)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Tanzu)]
    public void GetCloudProvider_OnPremDistributions_ReturnsOnPrem(Distribution distribution)
    {
        // Act
        var result = distribution.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.OnPrem);
    }

    #endregion

    #region GetCloudProvider Tests - AWS Distributions

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
        // Act
        var result = distribution.GetCloudProvider();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCloudProvider_ROSA_ReturnsROSA()
    {
        // Act
        var result = Distribution.OpenShiftROSA.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.ROSA);
    }

    #endregion

    #region GetCloudProvider Tests - Azure Distributions

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
        // Act
        var result = distribution.GetCloudProvider();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCloudProvider_ARO_ReturnsARO()
    {
        // Act
        var result = Distribution.OpenShiftARO.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.ARO);
    }

    #endregion

    #region GetCloudProvider Tests - GCP Distributions

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
        // Act
        var result = distribution.GetCloudProvider();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCloudProvider_OpenShiftDedicated_ReturnsOSD()
    {
        // Act
        var result = Distribution.OpenShiftDedicated.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.OSD);
    }

    #endregion

    #region GetCloudProvider Tests - Other Major Cloud Providers

    [Fact]
    public void GetCloudProvider_OKE_ReturnsOCI()
    {
        // Act
        var result = Distribution.OKE.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.OCI);
    }

    [Fact]
    public void GetCloudProvider_IKS_ReturnsIBM()
    {
        // Act
        var result = Distribution.IKS.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.IBM);
    }

    [Fact]
    public void GetCloudProvider_OpenShiftIBM_ReturnsROKS()
    {
        // Act
        var result = Distribution.OpenShiftIBM.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.ROKS);
    }

    [Fact]
    public void GetCloudProvider_ACK_ReturnsAlibaba()
    {
        // Act
        var result = Distribution.ACK.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Alibaba);
    }

    [Fact]
    public void GetCloudProvider_TKE_ReturnsTencent()
    {
        // Act
        var result = Distribution.TKE.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Tencent);
    }

    [Fact]
    public void GetCloudProvider_CCE_ReturnsHuawei()
    {
        // Act
        var result = Distribution.CCE.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Huawei);
    }

    #endregion

    #region GetCloudProvider Tests - Smaller Cloud Providers

    [Fact]
    public void GetCloudProvider_DOKS_ReturnsDigitalOcean()
    {
        // Act
        var result = Distribution.DOKS.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.DigitalOcean);
    }

    [Fact]
    public void GetCloudProvider_LKE_ReturnsLinode()
    {
        // Act
        var result = Distribution.LKE.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Linode);
    }

    [Fact]
    public void GetCloudProvider_VKE_ReturnsVultr()
    {
        // Act
        var result = Distribution.VKE.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Vultr);
    }

    [Fact]
    public void GetCloudProvider_HetznerK8s_ReturnsHetzner()
    {
        // Act
        var result = Distribution.HetznerK8s.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Hetzner);
    }

    [Fact]
    public void GetCloudProvider_OVHKubernetes_ReturnsOVH()
    {
        // Act
        var result = Distribution.OVHKubernetes.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.OVH);
    }

    [Fact]
    public void GetCloudProvider_ScalewayKapsule_ReturnsScaleway()
    {
        // Act
        var result = Distribution.ScalewayKapsule.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Scaleway);
    }

    #endregion

    #region GetCloudProvider Tests - Multi-Cloud/Generic

    [Fact]
    public void GetCloudProvider_RancherHosted_ReturnsManual()
    {
        // Act
        var result = Distribution.RancherHosted.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Manual);
    }

    [Fact]
    public void GetCloudProvider_TanzuCloud_ReturnsManual()
    {
        // Act
        var result = Distribution.TanzuCloud.GetCloudProvider();

        // Assert
        result.Should().Be(CloudProvider.Manual);
    }

    #endregion

    #region GetCrossAZCostMultiplier Tests

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OnPrem)]
    public void GetCrossAZCostMultiplier_SingleAZ_ReturnsZero(CloudProvider provider)
    {
        // Act
        var result = provider.GetCrossAZCostMultiplier(1);

        // Assert
        result.Should().Be(0m);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OnPrem)]
    public void GetCrossAZCostMultiplier_ZeroAZ_ReturnsZero(CloudProvider provider)
    {
        // Act
        var result = provider.GetCrossAZCostMultiplier(0);

        // Assert
        result.Should().Be(0m);
    }

    [Theory]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.ARO)]
    public void GetCrossAZCostMultiplier_AzureFamily_AlwaysFree(CloudProvider provider)
    {
        // Azure has FREE cross-AZ data transfer
        var result2AZ = provider.GetCrossAZCostMultiplier(2);
        var result3AZ = provider.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0m);
        result3AZ.Should().Be(0m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_OnPrem_AlwaysZero()
    {
        // On-prem has no cloud data transfer charges
        var result2AZ = CloudProvider.OnPrem.GetCrossAZCostMultiplier(2);
        var result3AZ = CloudProvider.OnPrem.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0m);
        result3AZ.Should().Be(0m);
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.ROSA)]
    public void GetCrossAZCostMultiplier_AWSFamily_Charged(CloudProvider provider)
    {
        // AWS charges ~$0.01/GB per direction
        var result2AZ = provider.GetCrossAZCostMultiplier(2);
        var result3AZ = provider.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0.02m);
        result3AZ.Should().Be(0.03m);
    }

    [Theory]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OSD)]
    public void GetCrossAZCostMultiplier_GCPFamily_Charged(CloudProvider provider)
    {
        // GCP similar to AWS
        var result2AZ = provider.GetCrossAZCostMultiplier(2);
        var result3AZ = provider.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0.02m);
        result3AZ.Should().Be(0.03m);
    }

    [Fact]
    public void GetCrossAZCostMultiplier_OCI_SlightlyLower()
    {
        // Oracle generally competitive
        var result2AZ = CloudProvider.OCI.GetCrossAZCostMultiplier(2);
        var result3AZ = CloudProvider.OCI.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0.015m);
        result3AZ.Should().Be(0.025m);
    }

    [Theory]
    [InlineData(CloudProvider.IBM)]
    [InlineData(CloudProvider.ROKS)]
    public void GetCrossAZCostMultiplier_IBMFamily_SimilarToAWS(CloudProvider provider)
    {
        var result2AZ = provider.GetCrossAZCostMultiplier(2);
        var result3AZ = provider.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0.02m);
        result3AZ.Should().Be(0.03m);
    }

    [Theory]
    [InlineData(CloudProvider.DigitalOcean)]
    [InlineData(CloudProvider.Linode)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Manual)]
    public void GetCrossAZCostMultiplier_OtherProviders_DefaultToAWSStyle(CloudProvider provider)
    {
        // Other providers default to conservative AWS-like estimates
        var result2AZ = provider.GetCrossAZCostMultiplier(2);
        var result3AZ = provider.GetCrossAZCostMultiplier(3);

        result2AZ.Should().Be(0.02m);
        result3AZ.Should().Be(0.03m);
    }

    #endregion

    #region GetControlPlaneCostPerHour Tests

    [Theory]
    [InlineData(CloudProvider.AWS, false, 0.10)]
    [InlineData(CloudProvider.AWS, true, 0.10)]
    [InlineData(CloudProvider.ROSA, false, 0.10)]
    [InlineData(CloudProvider.ROSA, true, 0.10)]
    public void GetControlPlaneCostPerHour_AWS_Always010(CloudProvider provider, bool isHA, decimal expected)
    {
        // AWS EKS always $0.10/hr regardless of HA
        var result = provider.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(CloudProvider.Azure, false, 0)]
    [InlineData(CloudProvider.Azure, true, 0.10)]
    [InlineData(CloudProvider.ARO, false, 0)]
    [InlineData(CloudProvider.ARO, true, 0.10)]
    public void GetControlPlaneCostPerHour_Azure_FreeUnlessHA(CloudProvider provider, bool isHA, decimal expected)
    {
        // Azure AKS free tier available, paid for premium/HA
        var result = provider.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(CloudProvider.GCP, false, 0.10)]
    [InlineData(CloudProvider.GCP, true, 0.10)]
    [InlineData(CloudProvider.OSD, false, 0.10)]
    [InlineData(CloudProvider.OSD, true, 0.10)]
    public void GetControlPlaneCostPerHour_GCP_Always010(CloudProvider provider, bool isHA, decimal expected)
    {
        // GKE $0.10/hr Autopilot
        var result = provider.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0.10)]
    public void GetControlPlaneCostPerHour_OCI_FreeUnlessEnhanced(bool isHA, decimal expected)
    {
        // Oracle OKE free basic, $0.10/hr enhanced
        var result = CloudProvider.OCI.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(CloudProvider.IBM, false)]
    [InlineData(CloudProvider.IBM, true)]
    [InlineData(CloudProvider.ROKS, false)]
    [InlineData(CloudProvider.ROKS, true)]
    public void GetControlPlaneCostPerHour_IBM_AlwaysFree(CloudProvider provider, bool isHA)
    {
        // IBM includes control plane free
        var result = provider.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(0m);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0.055)]
    public void GetControlPlaneCostPerHour_DigitalOcean_FreeUnlessHA(bool isHA, decimal expected)
    {
        // DigitalOcean ~$40/mo HA = ~$0.055/hr
        var result = CloudProvider.DigitalOcean.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0.083)]
    public void GetControlPlaneCostPerHour_Linode_FreeUnlessHA(bool isHA, decimal expected)
    {
        // Linode ~$60/mo HA = ~$0.083/hr
        var result = CloudProvider.Linode.GetControlPlaneCostPerHour(isHA);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetControlPlaneCostPerHour_OnPrem_AlwaysFree()
    {
        // On-prem self-managed, no cloud control plane cost
        var result = CloudProvider.OnPrem.GetControlPlaneCostPerHour(false);
        var resultHA = CloudProvider.OnPrem.GetControlPlaneCostPerHour(true);

        result.Should().Be(0m);
        resultHA.Should().Be(0m);
    }

    [Theory]
    [InlineData(CloudProvider.Alibaba)]
    [InlineData(CloudProvider.Tencent)]
    [InlineData(CloudProvider.Huawei)]
    [InlineData(CloudProvider.Vultr)]
    [InlineData(CloudProvider.Hetzner)]
    [InlineData(CloudProvider.OVH)]
    [InlineData(CloudProvider.Scaleway)]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    [InlineData(CloudProvider.Manual)]
    public void GetControlPlaneCostPerHour_OtherProviders_DefaultTo010(CloudProvider provider)
    {
        // Default fallback to AWS pricing
        var result = provider.GetControlPlaneCostPerHour(false);

        result.Should().Be(0.10m);
    }

    #endregion

    #region All Distribution Mapping Coverage

    [Fact]
    public void GetCloudProvider_AllDistributions_HaveValidMapping()
    {
        // Ensure every distribution maps to a valid CloudProvider
        var allDistributions = Enum.GetValues<Distribution>();

        foreach (var distribution in allDistributions)
        {
            var provider = distribution.GetCloudProvider();
            provider.Should().BeOneOf(
                CloudProvider.AWS, CloudProvider.Azure, CloudProvider.GCP,
                CloudProvider.OCI, CloudProvider.IBM, CloudProvider.Alibaba,
                CloudProvider.Tencent, CloudProvider.Huawei, CloudProvider.ROSA,
                CloudProvider.ARO, CloudProvider.OSD, CloudProvider.ROKS,
                CloudProvider.DigitalOcean, CloudProvider.Linode, CloudProvider.Vultr,
                CloudProvider.Hetzner, CloudProvider.OVH, CloudProvider.Scaleway,
                CloudProvider.Civo, CloudProvider.Exoscale, CloudProvider.OnPrem,
                CloudProvider.Manual);
        }
    }

    #endregion
}
