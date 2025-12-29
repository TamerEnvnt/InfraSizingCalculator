using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

public class HomePageUIHelperServiceTests
{
    private readonly HomePageUIHelperService _sut;

    public HomePageUIHelperServiceTests()
    {
        _sut = new HomePageUIHelperService();
    }

    #region Cluster Mode Display Helper Tests

    [Fact]
    public void GetClusterModeIcon_MultiCluster_ReturnsMC()
    {
        var result = _sut.GetClusterModeIcon(ClusterMode.MultiCluster, "Shared");

        result.Should().Be("MC");
    }

    [Theory]
    [InlineData("Shared", "SC")]
    [InlineData("Dev", "SE")]
    [InlineData("Test", "SE")]
    [InlineData("Prod", "SE")]
    public void GetClusterModeIcon_SingleClusterMode_ReturnsCorrectIcon(string scope, string expected)
    {
        var result = _sut.GetClusterModeIcon(ClusterMode.SharedCluster, scope);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetClusterModeDescription_MultiCluster_ReturnsDescriptionWithEnvCount()
    {
        var result = _sut.GetClusterModeDescription(ClusterMode.MultiCluster, "Shared", 4);

        result.Should().Be("Multi-Cluster: 4 separate clusters (one per environment)");
    }

    [Fact]
    public void GetClusterModeDescription_SharedCluster_ReturnsSharedDescription()
    {
        var result = _sut.GetClusterModeDescription(ClusterMode.SharedCluster, "Shared", 4);

        result.Should().Be("Shared Cluster: All environments in a single cluster with namespace isolation");
    }

    [Fact]
    public void GetClusterModeDescription_SingleEnvironment_ReturnsScopeDescription()
    {
        var result = _sut.GetClusterModeDescription(ClusterMode.PerEnvironment, "Prod", 1);

        result.Should().Be("Single Environment: Sizing for Prod only");
    }

    [Theory]
    [InlineData(ClusterMode.MultiCluster, "Shared", "multi")]
    [InlineData(ClusterMode.SharedCluster, "Shared", "shared")]
    [InlineData(ClusterMode.PerEnvironment, "Prod", "single")]
    [InlineData(ClusterMode.SharedCluster, "Dev", "single")]
    public void GetClusterModeBannerClass_ReturnsCorrectClass(ClusterMode mode, string scope, string expected)
    {
        var result = _sut.GetClusterModeBannerClass(mode, scope);

        result.Should().Be(expected);
    }

    #endregion

    #region Step Calculation Helper Tests

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.DotNet, 5)]
    [InlineData(DeploymentModel.Kubernetes, Technology.Java, 5)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 5)]
    [InlineData(DeploymentModel.VMs, Technology.DotNet, 4)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 4)]
    public void GetConfigStep_ReturnsCorrectStep(DeploymentModel deployment, Technology tech, int expected)
    {
        var result = _sut.GetConfigStep(deployment, tech);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.DotNet, 6)]
    [InlineData(DeploymentModel.Kubernetes, Technology.Java, 6)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 6)]
    [InlineData(DeploymentModel.VMs, Technology.DotNet, 5)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 5)]
    public void GetPricingStep_ReturnsCorrectStep(DeploymentModel deployment, Technology tech, int expected)
    {
        var result = _sut.GetPricingStep(deployment, tech);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.DotNet, 7)]
    [InlineData(DeploymentModel.Kubernetes, Technology.Java, 7)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 7)]
    [InlineData(DeploymentModel.VMs, Technology.DotNet, 6)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 6)]
    public void GetResultsStep_ReturnsCorrectStep(DeploymentModel deployment, Technology tech, int expected)
    {
        var result = _sut.GetResultsStep(deployment, tech);

        result.Should().Be(expected);
    }

    #endregion

    #region Pricing Estimation Helper Tests

    [Fact]
    public void GetTotalNodesForPricing_WithApps_ReturnsCorrectEstimate()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Medium = 10 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 20 } }
        };

        var result = _sut.GetTotalNodesForPricing(enabledEnvs, envApps);

        // 30 apps / 3 = 10 nodes
        result.Should().Be(10);
    }

    [Fact]
    public void GetTotalNodesForPricing_WithNoApps_ReturnsMinimum()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 0, Medium = 0, Large = 0, XLarge = 0 } }
        };

        var result = _sut.GetTotalNodesForPricing(enabledEnvs, envApps);

        result.Should().Be(3); // Minimum
    }

    [Fact]
    public void GetTotalCoresForPricing_WithMixedApps_ReturnsCorrectTotal()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 5, Medium = 5, Large = 2, XLarge = 1 } }
        };

        var result = _sut.GetTotalCoresForPricing(enabledEnvs, envApps);

        // 5*2 + 5*4 + 2*8 + 1*16 = 10 + 20 + 16 + 16 = 62 cores
        result.Should().Be(62);
    }

    [Fact]
    public void GetTotalCoresForPricing_WithNoApps_ReturnsMinimum()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig() }
        };

        var result = _sut.GetTotalCoresForPricing(enabledEnvs, envApps);

        result.Should().Be(8); // Minimum
    }

    [Fact]
    public void GetTotalRamForPricing_WithMixedApps_ReturnsCorrectTotal()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 5, Medium = 5, Large = 2, XLarge = 1 } }
        };

        var result = _sut.GetTotalRamForPricing(enabledEnvs, envApps);

        // 5*4 + 5*8 + 2*16 + 1*32 = 20 + 40 + 32 + 32 = 124 GB
        result.Should().Be(124);
    }

    [Fact]
    public void GetTotalRamForPricing_WithNoApps_ReturnsMinimum()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig() }
        };

        var result = _sut.GetTotalRamForPricing(enabledEnvs, envApps);

        result.Should().Be(16); // Minimum
    }

    [Fact]
    public void GetTotalNodesForPricing_IgnoresDisabledEnvironments()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Medium = 100 } }, // Should be ignored
            { EnvironmentType.Prod, new AppConfig { Medium = 9 } }
        };

        var result = _sut.GetTotalNodesForPricing(enabledEnvs, envApps);

        // Only Prod apps: 9 apps / 3 = 3 nodes (minimum)
        result.Should().Be(3);
    }

    #endregion

    #region Display Name Helper Tests

    [Fact]
    public void GetDistributionDisplayName_NonMendix_ReturnsDistributionName()
    {
        var result = _sut.GetDistributionDisplayName(
            Technology.DotNet, Distribution.EKS, null, MendixCloudType.SaaS, null);

        result.Should().Be("EKS");
    }

    [Theory]
    [InlineData(MendixCloudType.SaaS, "Mendix Cloud SaaS")]
    [InlineData(MendixCloudType.Dedicated, "Mendix Cloud Dedicated")]
    public void GetDistributionDisplayName_MendixCloud_ReturnsCloudTypeName(MendixCloudType cloudType, string expected)
    {
        var result = _sut.GetDistributionDisplayName(
            Technology.Mendix, Distribution.EKS, MendixDeploymentCategory.Cloud, cloudType, null);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Azure, "Mendix on Azure")]
    [InlineData(MendixPrivateCloudProvider.EKS, "Mendix on EKS")]
    [InlineData(MendixPrivateCloudProvider.AKS, "Mendix on AKS")]
    [InlineData(MendixPrivateCloudProvider.GKE, "Mendix on GKE")]
    [InlineData(MendixPrivateCloudProvider.OpenShift, "Mendix on OpenShift")]
    public void GetDistributionDisplayName_MendixPrivateCloud_ReturnsProviderName(MendixPrivateCloudProvider provider, string expected)
    {
        var result = _sut.GetDistributionDisplayName(
            Technology.Mendix, Distribution.EKS, MendixDeploymentCategory.PrivateCloud, MendixCloudType.SaaS, provider);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Rancher, "Rancher (Manual)")]
    [InlineData(MendixPrivateCloudProvider.K3s, "K3s (Manual)")]
    [InlineData(MendixPrivateCloudProvider.GenericK8s, "Generic K8s (Manual)")]
    public void GetDistributionDisplayName_MendixOther_ReturnsManualName(MendixPrivateCloudProvider provider, string expected)
    {
        var result = _sut.GetDistributionDisplayName(
            Technology.Mendix, Distribution.Kubernetes, MendixDeploymentCategory.Other, MendixCloudType.SaaS, provider);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixOtherDeployment.Server, "Server (VMs)")]
    [InlineData(MendixOtherDeployment.StackIT, "StackIT")]
    [InlineData(MendixOtherDeployment.SapBtp, "SAP BTP")]
    [InlineData(null, "Not selected")]
    public void GetMendixVMDeploymentDisplayName_ReturnsCorrectName(MendixOtherDeployment? deployment, string expected)
    {
        var result = _sut.GetMendixVMDeploymentDisplayName(deployment);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixOtherDeployment.Server, "Mendix on Server (VMs/Docker)")]
    [InlineData(MendixOtherDeployment.StackIT, "Mendix on StackIT")]
    [InlineData(MendixOtherDeployment.SapBtp, "Mendix on SAP BTP")]
    [InlineData(null, "Mendix Deployment")]
    public void GetMendixVMDeploymentName_ReturnsCorrectName(MendixOtherDeployment? deployment, string expected)
    {
        var result = _sut.GetMendixVMDeploymentName(deployment);

        result.Should().Be(expected);
    }

    #endregion

    #region Environment Helper Tests

    [Theory]
    [InlineData("Dev", EnvironmentType.Dev)]
    [InlineData("Test", EnvironmentType.Test)]
    [InlineData("Stage", EnvironmentType.Stage)]
    [InlineData("Prod", EnvironmentType.Prod)]
    [InlineData("DR", EnvironmentType.DR)]
    [InlineData("Shared", EnvironmentType.Prod)] // Default
    [InlineData("Unknown", EnvironmentType.Prod)] // Default
    public void GetSingleClusterEnvironment_ReturnsCorrectEnvironment(string scope, EnvironmentType expected)
    {
        var result = _sut.GetSingleClusterEnvironment(scope);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetSingleClusterEnvironments_Shared_ReturnsAllEnvironments()
    {
        var result = _sut.GetSingleClusterEnvironments("Shared").ToList();

        result.Should().HaveCount(4);
        result.Should().Contain(EnvironmentType.Dev);
        result.Should().Contain(EnvironmentType.Test);
        result.Should().Contain(EnvironmentType.Stage);
        result.Should().Contain(EnvironmentType.Prod);
    }

    [Fact]
    public void GetSingleClusterEnvironments_SpecificEnv_ReturnsSingleEnvironment()
    {
        var result = _sut.GetSingleClusterEnvironments("Prod").ToList();

        result.Should().HaveCount(1);
        result.Should().Contain(EnvironmentType.Prod);
    }

    #endregion

    #region Icon and CSS Class Helper Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvCssClass_ReturnsCorrectClass(EnvironmentType env, string expected)
    {
        var result = _sut.GetEnvCssClass(env);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "fas fa-code")]
    [InlineData(EnvironmentType.Test, "fas fa-flask")]
    [InlineData(EnvironmentType.Stage, "fas fa-clipboard-check")]
    [InlineData(EnvironmentType.Prod, "fas fa-rocket")]
    [InlineData(EnvironmentType.DR, "fas fa-shield-alt")]
    public void GetEnvIcon_ReturnsCorrectIcon(EnvironmentType env, string expected)
    {
        var result = _sut.GetEnvIcon(env);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "Dev")]
    [InlineData(EnvironmentType.Test, "Test")]
    [InlineData(EnvironmentType.Stage, "Stage")]
    [InlineData(EnvironmentType.Prod, "Prod")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void GetShortEnvName_ReturnsCorrectName(EnvironmentType env, string expected)
    {
        var result = _sut.GetShortEnvName(env);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("compute", "fas fa-microchip")]
    [InlineData("storage", "fas fa-hdd")]
    [InlineData("network", "fas fa-network-wired")]
    [InlineData("license", "fas fa-key")]
    [InlineData("support", "fas fa-headset")]
    [InlineData("labor", "fas fa-users")]
    [InlineData("datacenter", "fas fa-building")]
    [InlineData("control-plane", "fas fa-server")]
    [InlineData("unknown", "fas fa-dollar-sign")]
    [InlineData("COMPUTE", "fas fa-microchip")] // Case insensitive
    public void GetCostCategoryIcon_ReturnsCorrectIcon(string category, string expected)
    {
        var result = _sut.GetCostCategoryIcon(category);

        result.Should().Be(expected);
    }

    #endregion

    #region Tag Formatting Helper Tests

    [Theory]
    [InlineData("on-prem", "On-Prem")]
    [InlineData("cloud", "Cloud")]
    [InlineData("managed", "Managed")]
    [InlineData("enterprise", "Enterprise")]
    [InlineData("developer", "Developer")]
    [InlineData("lightweight", "Lightweight")]
    [InlineData("security", "Security")]
    [InlineData("ubuntu", "Ubuntu")]
    [InlineData("open-source", "Open Source")]
    [InlineData("edge", "Edge")]
    [InlineData("cost-effective", "Cost-Effective")]
    [InlineData("free", "Free")]
    [InlineData("infra", "Infra Nodes")]
    public void FormatTagLabel_KnownTags_ReturnsCorrectLabel(string tag, string expected)
    {
        var result = _sut.FormatTagLabel(tag);

        result.Should().Be(expected);
    }

    [Fact]
    public void FormatTagLabel_NullInput_ReturnsEmptyString()
    {
        var result = _sut.FormatTagLabel(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatTagLabel_EmptyInput_ReturnsEmptyString()
    {
        var result = _sut.FormatTagLabel(string.Empty);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("custom-tag", "Custom Tag")]
    [InlineData("my-custom-value", "My Custom Value")]
    [InlineData("simple", "Simple")]
    [InlineData("multi-word-tag-value", "Multi Word Tag Value")]
    public void FormatTagLabel_UnknownTags_ReturnsTitleCase(string tag, string expected)
    {
        var result = _sut.FormatTagLabel(tag);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("single")]
    [InlineData("word")]
    public void FormatTagLabel_SingleWord_ReturnsTitleCase(string tag)
    {
        var result = _sut.FormatTagLabel(tag);

        // First letter should be uppercase, rest lowercase
        result.Should().Be(char.ToUpper(tag[0]) + tag.Substring(1));
    }

    [Fact]
    public void FormatTagLabel_HyphenatedUnknownTag_ReplacesHyphensWithSpaces()
    {
        var result = _sut.FormatTagLabel("test-hyphen-tag");

        result.Should().Contain(" ");
        result.Should().NotContain("-");
    }

    [Fact]
    public void FormatTagLabel_AllKnownTagsCovered()
    {
        // Verify all known distribution tags are covered
        var knownTags = new[]
        {
            "on-prem", "cloud", "managed", "enterprise", "developer",
            "lightweight", "security", "ubuntu", "open-source", "edge",
            "cost-effective", "free", "infra"
        };

        foreach (var tag in knownTags)
        {
            var result = _sut.FormatTagLabel(tag);
            result.Should().NotBeNullOrEmpty($"Tag '{tag}' should have a label");
        }
    }

    #endregion
}
