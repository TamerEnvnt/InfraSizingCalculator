using FluentAssertions;
using InfraSizingCalculator.Helpers;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Helpers;

/// <summary>
/// Comprehensive tests for HomePageHelpers static helper methods.
/// These helpers were extracted from Home.razor for testability.
/// </summary>
public class HomePageHelpersTests
{
    #region Wizard Step Navigation Tests

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.Mendix, 7)]
    [InlineData(DeploymentModel.Kubernetes, Technology.OutSystems, 7)]
    [InlineData(DeploymentModel.Kubernetes, Technology.DotNet, 7)]
    [InlineData(DeploymentModel.Kubernetes, null, 7)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 7)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 6)]
    [InlineData(DeploymentModel.VMs, Technology.DotNet, 6)]
    [InlineData(DeploymentModel.VMs, null, 6)]
    [InlineData(null, null, 6)]
    public void GetTotalSteps_ReturnsCorrectCount(DeploymentModel? deployment, Technology? tech, int expected)
    {
        // Act
        var result = HomePageHelpers.GetTotalSteps(deployment, tech);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.Mendix, 5)]
    [InlineData(DeploymentModel.Kubernetes, Technology.OutSystems, 5)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 5)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 4)]
    [InlineData(DeploymentModel.VMs, Technology.DotNet, 4)]
    [InlineData(DeploymentModel.VMs, null, 4)]
    public void GetConfigStep_ReturnsCorrectStep(DeploymentModel? deployment, Technology? tech, int expected)
    {
        // Act
        var result = HomePageHelpers.GetConfigStep(deployment, tech);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.Mendix, 6)]
    [InlineData(DeploymentModel.Kubernetes, null, 6)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 6)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 5)]
    [InlineData(DeploymentModel.VMs, null, 5)]
    public void GetPricingStep_ReturnsCorrectStep(DeploymentModel? deployment, Technology? tech, int expected)
    {
        // Act
        var result = HomePageHelpers.GetPricingStep(deployment, tech);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, Technology.Mendix, 7)]
    [InlineData(DeploymentModel.Kubernetes, null, 7)]
    [InlineData(DeploymentModel.VMs, Technology.Mendix, 7)]
    [InlineData(DeploymentModel.VMs, Technology.OutSystems, 6)]
    [InlineData(DeploymentModel.VMs, null, 6)]
    public void GetResultsStep_ReturnsCorrectStep(DeploymentModel? deployment, Technology? tech, int expected)
    {
        // Act
        var result = HomePageHelpers.GetResultsStep(deployment, tech);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, DeploymentModel.Kubernetes, null, "Platform")]
    [InlineData(2, DeploymentModel.Kubernetes, null, "Deployment")]
    [InlineData(3, DeploymentModel.Kubernetes, null, "Technology")]
    [InlineData(4, DeploymentModel.Kubernetes, null, "Distribution")]
    [InlineData(5, DeploymentModel.Kubernetes, null, "Configure")]
    [InlineData(6, DeploymentModel.Kubernetes, null, "Pricing")]
    [InlineData(7, DeploymentModel.Kubernetes, null, "Results")]
    [InlineData(8, DeploymentModel.Kubernetes, null, "")]
    public void GetStepLabel_Kubernetes_ReturnsCorrectLabel(int step, DeploymentModel? deployment, Technology? tech, string expected)
    {
        // Act
        var result = HomePageHelpers.GetStepLabel(step, deployment, tech);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, Technology.Mendix, "Platform")]
    [InlineData(2, Technology.Mendix, "Deployment")]
    [InlineData(3, Technology.Mendix, "Technology")]
    [InlineData(4, Technology.Mendix, "Deployment Type")]
    [InlineData(5, Technology.Mendix, "Configure")]
    [InlineData(6, Technology.Mendix, "Pricing")]
    [InlineData(7, Technology.Mendix, "Results")]
    [InlineData(8, Technology.Mendix, "")]
    public void GetStepLabel_MendixVMs_ReturnsCorrectLabel(int step, Technology tech, string expected)
    {
        // Act
        var result = HomePageHelpers.GetStepLabel(step, DeploymentModel.VMs, tech);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, Technology.OutSystems, "Platform")]
    [InlineData(2, Technology.OutSystems, "Deployment")]
    [InlineData(3, Technology.OutSystems, "Technology")]
    [InlineData(4, Technology.OutSystems, "Configure")]
    [InlineData(5, Technology.OutSystems, "Pricing")]
    [InlineData(6, Technology.OutSystems, "Results")]
    [InlineData(7, Technology.OutSystems, "")]
    public void GetStepLabel_NonMendixVMs_ReturnsCorrectLabel(int step, Technology tech, string expected)
    {
        // Act
        var result = HomePageHelpers.GetStepLabel(step, DeploymentModel.VMs, tech);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Cluster Mode Helper Tests

    [Theory]
    [InlineData(ClusterMode.MultiCluster, "Shared", "MC")]
    [InlineData(ClusterMode.MultiCluster, "Dev", "MC")]
    [InlineData(ClusterMode.SharedCluster, "Shared", "SC")]
    [InlineData(ClusterMode.PerEnvironment, "Shared", "SC")]
    [InlineData(ClusterMode.SharedCluster, "Dev", "SE")]
    [InlineData(ClusterMode.PerEnvironment, "Prod", "SE")]
    public void GetClusterModeIcon_ReturnsCorrectIcon(ClusterMode mode, string scope, string expected)
    {
        // Act
        var result = HomePageHelpers.GetClusterModeIcon(mode, scope);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetClusterModeDescription_MultiCluster_ReturnsCorrectDescription()
    {
        // Act
        var result = HomePageHelpers.GetClusterModeDescription(ClusterMode.MultiCluster, "Shared", 4);

        // Assert
        result.Should().Be("Multi-Cluster: 4 separate clusters (one per environment)");
    }

    [Fact]
    public void GetClusterModeDescription_SharedCluster_ReturnsCorrectDescription()
    {
        // Act
        var result = HomePageHelpers.GetClusterModeDescription(ClusterMode.SharedCluster, "Shared", 4);

        // Assert
        result.Should().Be("Shared Cluster: All environments in a single cluster with namespace isolation");
    }

    [Fact]
    public void GetClusterModeDescription_SingleEnvironment_ReturnsCorrectDescription()
    {
        // Act
        var result = HomePageHelpers.GetClusterModeDescription(ClusterMode.PerEnvironment, "Prod", 4);

        // Assert
        result.Should().Be("Single Environment: Sizing for Prod only");
    }

    [Theory]
    [InlineData(ClusterMode.MultiCluster, "Shared", "multi")]
    [InlineData(ClusterMode.SharedCluster, "Shared", "shared")]
    [InlineData(ClusterMode.PerEnvironment, "Shared", "shared")]
    [InlineData(ClusterMode.SharedCluster, "Dev", "single")]
    [InlineData(ClusterMode.PerEnvironment, "Prod", "single")]
    public void GetClusterModeBannerClass_ReturnsCorrectClass(ClusterMode mode, string scope, string expected)
    {
        // Act
        var result = HomePageHelpers.GetClusterModeBannerClass(mode, scope);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ClusterMode.SharedCluster, true)]
    [InlineData(ClusterMode.PerEnvironment, true)]
    [InlineData(ClusterMode.MultiCluster, false)]
    public void IsSingleClusterMode_ReturnsCorrectResult(ClusterMode mode, bool expected)
    {
        // Act
        var result = HomePageHelpers.IsSingleClusterMode(mode);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ClusterMode.SharedCluster, "Shared", true)]
    [InlineData(ClusterMode.PerEnvironment, "Shared", true)]
    [InlineData(ClusterMode.SharedCluster, "Dev", false)]
    [InlineData(ClusterMode.MultiCluster, "Shared", false)]
    public void IsSharedClusterMode_ReturnsCorrectResult(ClusterMode mode, string scope, bool expected)
    {
        // Act
        var result = HomePageHelpers.IsSharedClusterMode(mode, scope);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Dev", EnvironmentType.Dev)]
    [InlineData("Test", EnvironmentType.Test)]
    [InlineData("Stage", EnvironmentType.Stage)]
    [InlineData("Prod", EnvironmentType.Prod)]
    [InlineData("DR", EnvironmentType.DR)]
    [InlineData("Shared", EnvironmentType.Prod)]
    [InlineData("Unknown", EnvironmentType.Prod)]
    public void GetSingleClusterEnvironment_ReturnsCorrectEnvironment(string scope, EnvironmentType expected)
    {
        // Act
        var result = HomePageHelpers.GetSingleClusterEnvironment(scope);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Cloud Pricing Calculation Tests

    [Theory]
    [InlineData("aws", 4, 16, "m6i.xlarge")]
    [InlineData("aws", 8, 32, "m6i.2xlarge")]
    [InlineData("aws", 16, 64, "m6i.4xlarge")]
    [InlineData("aws", 32, 128, "m6i.4xlarge")]
    [InlineData("azure", 4, 16, "Standard_D4s_v5")]
    [InlineData("azure", 8, 32, "Standard_D8s_v5")]
    [InlineData("azure", 16, 64, "Standard_D16s_v5")]
    [InlineData("gcp", 4, 16, "n2-standard-4")]
    [InlineData("gcp", 8, 32, "n2-standard-8")]
    [InlineData("gcp", 16, 64, "n2-standard-16")]
    [InlineData("unknown", 8, 32, "standard")]
    public void GetInstanceType_ReturnsCorrectType(string provider, decimal cpu, decimal ram, string expected)
    {
        // Act
        var result = HomePageHelpers.GetInstanceType(provider, cpu, ram);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("aws", "m6i.xlarge", 1, 140.16)]
    [InlineData("aws", "m6i.2xlarge", 1, 280.32)]
    [InlineData("aws", "m6i.4xlarge", 1, 560.64)]
    [InlineData("azure", "Standard_D4s_v5", 1, 140.16)]
    [InlineData("azure", "Standard_D8s_v5", 1, 280.32)]
    [InlineData("azure", "Standard_D16s_v5", 1, 560.64)]
    [InlineData("gcp", "n2-standard-4", 1, 142.35)]
    [InlineData("gcp", "n2-standard-8", 1, 283.97)]
    [InlineData("gcp", "n2-standard-16", 1, 567.94)]
    [InlineData("unknown", "unknown", 1, 292.00)]
    public void GetComputeCost_ReturnsCorrectMonthlyCost(string provider, string instanceType, int nodeCount, decimal expected)
    {
        // Act
        var result = HomePageHelpers.GetComputeCost(provider, instanceType, nodeCount);

        // Assert
        result.Should().BeApproximately(expected, 0.01m);
    }

    [Fact]
    public void GetComputeCost_MultipleNodes_ScalesLinearly()
    {
        // Arrange
        var singleNodeCost = HomePageHelpers.GetComputeCost("aws", "m6i.xlarge", 1);

        // Act
        var threeNodeCost = HomePageHelpers.GetComputeCost("aws", "m6i.xlarge", 3);

        // Assert
        threeNodeCost.Should().BeApproximately(singleNodeCost * 3, 0.01m);
    }

    #endregion

    #region Cost Formatting Tests

    public static IEnumerable<object?[]> FormatCostPreviewTestData()
    {
        yield return new object?[] { null, "--" };
        yield return new object?[] { 500m, "$500" };
        yield return new object?[] { 1000m, "$1.0K" };
        yield return new object?[] { 1500m, "$1.5K" };
        yield return new object?[] { 10000m, "$10.0K" };
        yield return new object?[] { 100000m, "$100.0K" };
        yield return new object?[] { 1000000m, "$1.00M" };
        yield return new object?[] { 1500000m, "$1.50M" };
        yield return new object?[] { 10000000m, "$10.00M" };
    }

    [Theory]
    [MemberData(nameof(FormatCostPreviewTestData))]
    public void FormatCostPreview_ReturnsCorrectFormat(decimal? amount, string expected)
    {
        // Act
        var result = HomePageHelpers.FormatCostPreview(amount);

        // Assert
        result.Should().Be(expected);
    }

    public static IEnumerable<object[]> FormatCurrencyTestData()
    {
        yield return new object[] { 0m, "$0.00" };
        yield return new object[] { 99.99m, "$99.99" };
        yield return new object[] { 500m, "$500.00" };
        yield return new object[] { 1000m, "$1.0K" };
        yield return new object[] { 1234.56m, "$1.2K" };
        yield return new object[] { 1000000m, "$1.00M" };
        yield return new object[] { 2500000m, "$2.50M" };
    }

    [Theory]
    [MemberData(nameof(FormatCurrencyTestData))]
    public void FormatCurrency_ReturnsCorrectFormat(decimal amount, string expected)
    {
        // Act
        var result = HomePageHelpers.FormatCurrency(amount);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(CostCategory.Compute, "CPU")]
    [InlineData(CostCategory.Storage, "HDD")]
    [InlineData(CostCategory.Network, "NET")]
    [InlineData(CostCategory.License, "LIC")]
    [InlineData(CostCategory.Support, "SUP")]
    [InlineData(CostCategory.DataCenter, "DC")]
    [InlineData(CostCategory.Labor, "OPS")]
    public void GetCostCategoryIcon_ReturnsCorrectIcon(CostCategory category, string expected)
    {
        // Act
        var result = HomePageHelpers.GetCostCategoryIcon(category);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Environment Display Helper Tests

    [Theory]
    [InlineData("Development", "DEV")]
    [InlineData("Test", "TEST")]
    [InlineData("Testing", "TEST")]
    [InlineData("Staging", "STG")]
    [InlineData("Production", "PROD")]
    [InlineData("Disaster Recovery", "DR")]
    [InlineData("QA", "QA")]
    [InlineData("UAT", "UAT")]
    [InlineData("SomeVeryLongEnvName", "SOME")]
    public void GetShortEnvName_ReturnsCorrectAbbreviation(string envName, string expected)
    {
        // Act
        var result = HomePageHelpers.GetShortEnvName(envName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("development", "env-dev")]
    [InlineData("Development", "env-dev")]
    [InlineData("test", "env-test")]
    [InlineData("testing", "env-test")]
    [InlineData("staging", "env-stage")]
    [InlineData("production", "env-prod")]
    [InlineData("disaster recovery", "env-dr")]
    [InlineData("unknown", "env-default")]
    public void GetEnvCssClass_ReturnsCorrectClass(string envName, string expected)
    {
        // Act
        var result = HomePageHelpers.GetEnvCssClass(envName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "D")]
    [InlineData(EnvironmentType.Test, "T")]
    [InlineData(EnvironmentType.Stage, "S")]
    [InlineData(EnvironmentType.Prod, "P")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void GetEnvIcon_ReturnsCorrectIcon(EnvironmentType env, string expected)
    {
        // Act
        var result = HomePageHelpers.GetEnvIcon(env);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(EnvironmentType.Prod, true)]
    [InlineData(EnvironmentType.Dev, false)]
    [InlineData(EnvironmentType.Test, false)]
    [InlineData(EnvironmentType.Stage, false)]
    [InlineData(EnvironmentType.DR, false)]
    public void IsProdEnvironment_ReturnsCorrectResult(EnvironmentType env, bool expected)
    {
        // Act
        var result = HomePageHelpers.IsProdEnvironment(env);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Pattern Display Helper Tests

    [Theory]
    [InlineData(HAPattern.None, "None")]
    [InlineData(HAPattern.ActiveActive, "Active-Active")]
    [InlineData(HAPattern.ActivePassive, "Active-Passive")]
    [InlineData(HAPattern.NPlus1, "N+1 Redundancy")]
    [InlineData(HAPattern.NPlus2, "N+2 Redundancy")]
    public void FormatHAPattern_ReturnsCorrectFormat(HAPattern pattern, string expected)
    {
        // Act
        var result = HomePageHelpers.FormatHAPattern(pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(HAPattern.None, "None")]
    [InlineData(HAPattern.ActiveActive, "Active-Active")]
    [InlineData(HAPattern.ActivePassive, "Active-Passive")]
    [InlineData(HAPattern.NPlus1, "N+1")]
    [InlineData(HAPattern.NPlus2, "N+2")]
    public void GetHAPatternDisplay_ReturnsShortFormat(HAPattern pattern, string expected)
    {
        // Act
        var result = HomePageHelpers.GetHAPatternDisplay(pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DRPattern.None, "None")]
    [InlineData(DRPattern.WarmStandby, "Warm Standby")]
    [InlineData(DRPattern.HotStandby, "Hot Standby")]
    [InlineData(DRPattern.MultiRegion, "Multi-Region")]
    public void GetDRPatternDisplay_ReturnsCorrectFormat(DRPattern pattern, string expected)
    {
        // Act
        var result = HomePageHelpers.GetDRPatternDisplay(pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(LoadBalancerOption.None, "None")]
    [InlineData(LoadBalancerOption.Single, "Single LB")]
    [InlineData(LoadBalancerOption.HAPair, "HA Pair")]
    [InlineData(LoadBalancerOption.CloudLB, "Cloud LB")]
    public void GetLBOptionDisplay_ReturnsCorrectFormat(LoadBalancerOption option, string expected)
    {
        // Act
        var result = HomePageHelpers.GetLBOptionDisplay(option);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Server Role Helper Tests

    [Theory]
    [InlineData(ServerRole.Web, "web")]
    [InlineData(ServerRole.App, "app")]
    [InlineData(ServerRole.Database, "db")]
    [InlineData(ServerRole.Cache, "cache")]
    [InlineData(ServerRole.MessageQueue, "mq")]
    [InlineData(ServerRole.Search, "search")]
    [InlineData(ServerRole.Storage, "storage")]
    [InlineData(ServerRole.Monitoring, "mon")]
    [InlineData(ServerRole.Bastion, "bastion")]
    public void GetRoleIcon_ReturnsCorrectIcon(ServerRole role, string expected)
    {
        // Act
        var result = HomePageHelpers.GetRoleIcon(role);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Parsing Helper Tests

    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-5", -5)]
    [InlineData("999999", 999999)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("abc", 0)]
    [InlineData("12.5", 0)]
    public void ParseInt_ReturnsCorrectValue(object? input, int expected)
    {
        // Act
        var result = HomePageHelpers.ParseInt(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ParseInt_IntegerInput_ReturnsValue()
    {
        // Act
        var result = HomePageHelpers.ParseInt(42);

        // Assert
        result.Should().Be(42);
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("0", 0.0)]
    [InlineData("-5.5", -5.5)]
    [InlineData("0.001", 0.001)]
    [InlineData(null, 0.0)]
    [InlineData("", 0.0)]
    [InlineData("abc", 0.0)]
    public void ParseDouble_ReturnsCorrectValue(object? input, double expected)
    {
        // Act
        var result = HomePageHelpers.ParseDouble(input);

        // Assert
        result.Should().BeApproximately(expected, 0.0001);
    }

    [Fact]
    public void ParseDouble_DoubleInput_ReturnsValue()
    {
        // Act
        var result = HomePageHelpers.ParseDouble(3.14159);

        // Assert
        result.Should().BeApproximately(3.14159, 0.00001);
    }

    #endregion

    #region Mendix Deployment Helper Tests

    [Theory]
    [InlineData(MendixOtherDeployment.Server, "Server (VMs)")]
    [InlineData(MendixOtherDeployment.StackIT, "StackIT")]
    [InlineData(MendixOtherDeployment.SapBtp, "SAP BTP")]
    [InlineData(null, "Not selected")]
    public void GetMendixVMDeploymentDisplayName_ReturnsCorrectName(MendixOtherDeployment? deployment, string expected)
    {
        // Act
        var result = HomePageHelpers.GetMendixVMDeploymentDisplayName(deployment);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixOtherDeployment.Server, "Mendix on Server (VMs/Docker)")]
    [InlineData(MendixOtherDeployment.StackIT, "Mendix on StackIT")]
    [InlineData(MendixOtherDeployment.SapBtp, "Mendix on SAP BTP")]
    [InlineData(null, "Mendix Deployment")]
    public void GetMendixVMDeploymentName_ReturnsCorrectFullName(MendixOtherDeployment? deployment, string expected)
    {
        // Act
        var result = HomePageHelpers.GetMendixVMDeploymentName(deployment);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Distribution Helper Tests

    [Theory]
    [InlineData(Distribution.OpenShift, true)]
    [InlineData(Distribution.Kubernetes, true)]
    [InlineData(Distribution.Rancher, true)]
    [InlineData(Distribution.RKE2, true)]
    [InlineData(Distribution.K3s, true)]
    [InlineData(Distribution.MicroK8s, true)]
    [InlineData(Distribution.Charmed, true)]
    [InlineData(Distribution.Tanzu, true)]
    [InlineData(Distribution.EKS, false)]
    [InlineData(Distribution.AKS, false)]
    [InlineData(Distribution.GKE, false)]
    public void IsOnPremDistribution_ReturnsCorrectResult(Distribution distribution, bool expected)
    {
        // Act
        var result = HomePageHelpers.IsOnPremDistribution(distribution);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Distribution.OpenShift, true)]
    [InlineData(Distribution.OpenShiftROSA, true)]
    [InlineData(Distribution.OpenShiftARO, true)]
    [InlineData(Distribution.OpenShiftDedicated, true)]
    [InlineData(Distribution.OpenShiftIBM, true)]
    [InlineData(Distribution.Kubernetes, false)]
    [InlineData(Distribution.EKS, false)]
    [InlineData(Distribution.AKS, false)]
    [InlineData(null, false)]
    public void IsOpenShiftDistribution_ReturnsCorrectResult(Distribution? distribution, bool expected)
    {
        // Act
        var result = HomePageHelpers.IsOpenShiftDistribution(distribution);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Azure, Distribution.AKS)]
    [InlineData(MendixPrivateCloudProvider.EKS, Distribution.EKS)]
    [InlineData(MendixPrivateCloudProvider.AKS, Distribution.AKS)]
    [InlineData(MendixPrivateCloudProvider.GKE, Distribution.GKE)]
    [InlineData(MendixPrivateCloudProvider.OpenShift, Distribution.OpenShift)]
    [InlineData(MendixPrivateCloudProvider.Rancher, Distribution.Rancher)]
    [InlineData(MendixPrivateCloudProvider.K3s, Distribution.K3s)]
    [InlineData(MendixPrivateCloudProvider.GenericK8s, Distribution.Kubernetes)]
    [InlineData(MendixPrivateCloudProvider.Docker, Distribution.Kubernetes)]
    [InlineData(null, Distribution.Kubernetes)]
    public void MapMendixProviderToDistribution_ReturnsCorrectDistribution(MendixPrivateCloudProvider? provider, Distribution expected)
    {
        // Act
        var result = HomePageHelpers.MapMendixProviderToDistribution(provider);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Pricing Estimation Helper Tests

    [Fact]
    public void GetTotalNodesForPricing_EmptyApps_ReturnsMinimum()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>();
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalNodesForPricing(envApps, enabledEnvs);

        // Assert
        result.Should().Be(3); // Minimum nodes
    }

    [Fact]
    public void GetTotalNodesForPricing_SmallApps_CalculatesCorrectly()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 5, Medium = 0, Large = 0, XLarge = 0 } },
            { EnvironmentType.Prod, new AppConfig { Small = 5, Medium = 0, Large = 0, XLarge = 0 } }
        };
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalNodesForPricing(envApps, enabledEnvs);

        // Assert - 10 total apps / 3 = 3.33, ceiling = 4
        result.Should().Be(4);
    }

    [Fact]
    public void GetTotalNodesForPricing_MixedApps_CalculatesCorrectly()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 2, Medium = 3, Large = 1, XLarge = 0 } },
            { EnvironmentType.Test, new AppConfig { Small = 2, Medium = 2, Large = 0, XLarge = 1 } },
            { EnvironmentType.Prod, new AppConfig { Small = 3, Medium = 5, Large = 2, XLarge = 1 } }
        };
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalNodesForPricing(envApps, enabledEnvs);

        // Assert - (6 + 5 + 11) = 22 apps / 3 = 7.33, ceiling = 8
        result.Should().Be(8);
    }

    [Fact]
    public void GetTotalNodesForPricing_OnlyEnabledEnvironments_IgnoresDisabled()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 10, Medium = 0, Large = 0, XLarge = 0 } },
            { EnvironmentType.Prod, new AppConfig { Small = 10, Medium = 0, Large = 0, XLarge = 0 } }
        };
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev }; // Only Dev enabled

        // Act
        var result = HomePageHelpers.GetTotalNodesForPricing(envApps, enabledEnvs);

        // Assert - Only Dev's 10 apps / 3 = 3.33, ceiling = 4
        result.Should().Be(4);
    }

    [Fact]
    public void GetTotalCoresForPricing_EmptyApps_ReturnsMinimum()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>();
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalCoresForPricing(envApps, enabledEnvs);

        // Assert
        result.Should().Be(8); // Minimum cores
    }

    [Fact]
    public void GetTotalCoresForPricing_MixedApps_CalculatesCorrectly()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 2, Medium = 2, Large = 1, XLarge = 1 } }
        };
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalCoresForPricing(envApps, enabledEnvs);

        // Assert - Small=2*2=4, Medium=2*4=8, Large=1*8=8, XLarge=1*16=16 = 36 cores
        result.Should().Be(36);
    }

    [Fact]
    public void GetTotalRamForPricing_EmptyApps_ReturnsMinimum()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>();
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalRamForPricing(envApps, enabledEnvs);

        // Assert
        result.Should().Be(16); // Minimum RAM in GB
    }

    [Fact]
    public void GetTotalRamForPricing_MixedApps_CalculatesCorrectly()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 2, Medium = 2, Large = 1, XLarge = 1 } }
        };
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalRamForPricing(envApps, enabledEnvs);

        // Assert - Small=2*4=8, Medium=2*8=16, Large=1*16=16, XLarge=1*32=32 = 72 GB
        result.Should().Be(72);
    }

    [Fact]
    public void GetTotalRamForPricing_MultipleEnvironments_SumsAll()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 1, Medium = 0, Large = 0, XLarge = 0 } },
            { EnvironmentType.Test, new AppConfig { Small = 1, Medium = 0, Large = 0, XLarge = 0 } },
            { EnvironmentType.Prod, new AppConfig { Small = 1, Medium = 0, Large = 0, XLarge = 0 } }
        };
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Prod };

        // Act
        var result = HomePageHelpers.GetTotalRamForPricing(envApps, enabledEnvs);

        // Assert - 3 small apps * 4 GB = 12, but minimum is 16
        result.Should().Be(16);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void GetStepLabel_NullDeploymentNullTech_UsesNonMendixVMsPath()
    {
        // Act
        var result = HomePageHelpers.GetStepLabel(4, null, null);

        // Assert - Uses non-Mendix VMs path: step 4 = Configure
        result.Should().Be("Configure");
    }

    [Fact]
    public void GetClusterModeIcon_NullClusterMode_ReturnsSingleEnvironmentIcon()
    {
        // Act
        var result = HomePageHelpers.GetClusterModeIcon(null, "Dev");

        // Assert - null treated as not-MultiCluster, scope not "Shared" = SE
        result.Should().Be("SE");
    }

    [Fact]
    public void FormatCostPreview_VeryLargeAmount_FormatsCorrectly()
    {
        // Arrange
        decimal largeAmount = 100_000_000;

        // Act
        var result = HomePageHelpers.FormatCostPreview(largeAmount);

        // Assert
        result.Should().Be("$100.00M");
    }

    [Fact]
    public void FormatCostPreview_SmallDecimal_FormatsAsInteger()
    {
        // Arrange
        decimal smallAmount = 99.99m;

        // Act
        var result = HomePageHelpers.FormatCostPreview(smallAmount);

        // Assert
        result.Should().Be("$100"); // Rounded to integer display
    }

    [Fact]
    public void GetShortEnvName_ShortInput_ReturnsUppercase()
    {
        // Act
        var result = HomePageHelpers.GetShortEnvName("QA");

        // Assert
        result.Should().Be("QA");
    }

    [Fact]
    public void GetEnvCssClass_MixedCase_HandlesCorrectly()
    {
        // Act
        var result = HomePageHelpers.GetEnvCssClass("PRODUCTION");

        // Assert
        result.Should().Be("env-prod");
    }

    #endregion
}
