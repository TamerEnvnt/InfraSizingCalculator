using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class WizardStateServiceTests
{
    private readonly WizardStateService _service;

    public WizardStateServiceTests()
    {
        _service = new WizardStateService();
    }

    #region Initial State Tests

    [Fact]
    public void InitialState_CurrentStepIsOne()
    {
        // Assert
        _service.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void InitialState_SelectedPlatformIsNull()
    {
        // Assert
        _service.SelectedPlatform.Should().BeNull();
    }

    [Fact]
    public void InitialState_SelectedDeploymentIsNull()
    {
        // Assert
        _service.SelectedDeployment.Should().BeNull();
    }

    [Fact]
    public void InitialState_SelectedTechnologyIsNull()
    {
        // Assert
        _service.SelectedTechnology.Should().BeNull();
    }

    [Fact]
    public void InitialState_SelectedDistributionIsNull()
    {
        // Assert
        _service.SelectedDistribution.Should().BeNull();
    }

    [Fact]
    public void InitialState_SelectedClusterModeIsMultiCluster()
    {
        // Assert
        _service.SelectedClusterMode.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public void InitialState_SingleClusterScopeIsShared()
    {
        // Assert
        _service.SingleClusterScope.Should().Be("Shared");
    }

    [Fact]
    public void InitialState_ConfigTabIsApps()
    {
        // Assert
        _service.ConfigTab.Should().Be("apps");
    }

    [Fact]
    public void InitialState_DistributionFilterIsAll()
    {
        // Assert
        _service.DistributionFilter.Should().Be("all");
    }

    [Fact]
    public void InitialState_ResultsIsNull()
    {
        // Assert
        _service.Results.Should().BeNull();
    }

    [Fact]
    public void InitialState_HasAllDefaultEnvironments()
    {
        // Assert
        _service.EnabledEnvironments.Should().Contain(EnvironmentType.Dev);
        _service.EnabledEnvironments.Should().Contain(EnvironmentType.Test);
        _service.EnabledEnvironments.Should().Contain(EnvironmentType.Stage);
        _service.EnabledEnvironments.Should().Contain(EnvironmentType.Prod);
        _service.EnabledEnvironments.Should().Contain(EnvironmentType.DR);
    }

    [Fact]
    public void InitialState_HasDefaultHeadroomValues()
    {
        // Assert
        _service.Headroom[EnvironmentType.Dev].Should().Be(20);
        _service.Headroom[EnvironmentType.Test].Should().Be(20);
        _service.Headroom[EnvironmentType.Stage].Should().Be(25);
        _service.Headroom[EnvironmentType.Prod].Should().Be(30);
        _service.Headroom[EnvironmentType.DR].Should().Be(30);
    }

    [Fact]
    public void InitialState_HasDefaultReplicaValues()
    {
        // Assert
        _service.Replicas[EnvironmentType.Dev].Should().Be(1);
        _service.Replicas[EnvironmentType.Test].Should().Be(1);
        _service.Replicas[EnvironmentType.Stage].Should().Be(2);
        _service.Replicas[EnvironmentType.Prod].Should().Be(3);
        _service.Replicas[EnvironmentType.DR].Should().Be(3);
    }

    [Fact]
    public void InitialState_OvercommitValuesAreOne()
    {
        // Assert
        _service.ProdCpuOvercommit.Should().Be(1.0);
        _service.ProdMemoryOvercommit.Should().Be(1.0);
        _service.NonProdCpuOvercommit.Should().Be(1.0);
        _service.NonProdMemoryOvercommit.Should().Be(1.0);
    }

    [Fact]
    public void InitialState_NodeSpecsIsNotNull()
    {
        // Assert
        _service.NodeSpecs.Should().NotBeNull();
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_RestoresCurrentStepToOne()
    {
        // Arrange
        _service.CurrentStep = 5;

        // Act
        _service.Reset();

        // Assert
        _service.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void Reset_ClearsSelections()
    {
        // Arrange
        _service.SelectedPlatform = PlatformType.Native;
        _service.SelectedDeployment = DeploymentModel.Kubernetes;
        _service.SelectedTechnology = Technology.DotNet;
        _service.SelectedDistribution = Distribution.Kubernetes;

        // Act
        _service.Reset();

        // Assert
        _service.SelectedPlatform.Should().BeNull();
        _service.SelectedDeployment.Should().BeNull();
        _service.SelectedTechnology.Should().BeNull();
        _service.SelectedDistribution.Should().BeNull();
    }

    [Fact]
    public void Reset_RestoresClusterMode()
    {
        // Arrange
        _service.SelectedClusterMode = ClusterMode.SharedCluster;
        _service.SingleClusterScope = "Prod";

        // Act
        _service.Reset();

        // Assert
        _service.SelectedClusterMode.Should().Be(ClusterMode.MultiCluster);
        _service.SingleClusterScope.Should().Be("Shared");
    }

    [Fact]
    public void Reset_ClearsResults()
    {
        // Arrange
        _service.Results = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal(),
            Configuration = new K8sSizingInput()
        };

        // Act
        _service.Reset();

        // Assert
        _service.Results.Should().BeNull();
    }

    [Fact]
    public void Reset_RestoresEnabledEnvironments()
    {
        // Arrange
        _service.EnabledEnvironments.Clear();
        _service.EnabledEnvironments.Add(EnvironmentType.Prod);

        // Act
        _service.Reset();

        // Assert
        _service.EnabledEnvironments.Should().HaveCount(5);
    }

    [Fact]
    public void Reset_RestoresHeadroomDefaults()
    {
        // Arrange
        _service.Headroom[EnvironmentType.Prod] = 50;

        // Act
        _service.Reset();

        // Assert
        _service.Headroom[EnvironmentType.Prod].Should().Be(30);
    }

    [Fact]
    public void Reset_RestoresReplicaDefaults()
    {
        // Arrange
        _service.Replicas[EnvironmentType.Dev] = 5;

        // Act
        _service.Reset();

        // Assert
        _service.Replicas[EnvironmentType.Dev].Should().Be(1);
    }

    [Fact]
    public void Reset_RestoresOvercommitDefaults()
    {
        // Arrange
        _service.ProdCpuOvercommit = 2.0;
        _service.NonProdMemoryOvercommit = 1.5;

        // Act
        _service.Reset();

        // Assert
        _service.ProdCpuOvercommit.Should().Be(1.0);
        _service.NonProdMemoryOvercommit.Should().Be(1.0);
    }

    [Fact]
    public void Reset_FiresOnStateChangedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;

        // Act
        _service.Reset();

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void Reset_CreatesNewEnvAppsEntries()
    {
        // Arrange
        _service.EnvApps.Clear();

        // Act
        _service.Reset();

        // Assert
        _service.EnvApps.Should().ContainKey(EnvironmentType.Dev);
        _service.EnvApps.Should().ContainKey(EnvironmentType.Test);
        _service.EnvApps.Should().ContainKey(EnvironmentType.Stage);
        _service.EnvApps.Should().ContainKey(EnvironmentType.Prod);
        _service.EnvApps.Should().ContainKey(EnvironmentType.DR);
    }

    #endregion

    #region GetTotalSteps Tests

    [Fact]
    public void GetTotalSteps_VMDeployment_ReturnsFive()
    {
        // Arrange
        _service.SelectedDeployment = DeploymentModel.VMs;

        // Act & Assert
        _service.GetTotalSteps().Should().Be(5);
    }

    [Fact]
    public void GetTotalSteps_K8sDeployment_ReturnsSix()
    {
        // Arrange
        _service.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act & Assert
        _service.GetTotalSteps().Should().Be(6);
    }

    [Fact]
    public void GetTotalSteps_NoDeployment_ReturnsSix()
    {
        // Arrange
        _service.SelectedDeployment = null;

        // Act & Assert
        _service.GetTotalSteps().Should().Be(6);
    }

    #endregion

    #region GetStepLabel Tests

    [Theory]
    [InlineData(1, "Platform")]
    [InlineData(2, "Deployment")]
    [InlineData(3, "Technology")]
    [InlineData(4, "Distribution")]
    [InlineData(5, "Configuration")]
    [InlineData(6, "Results")]
    public void GetStepLabel_K8sDeployment_ReturnsCorrectLabel(int step, string expected)
    {
        // Arrange
        _service.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act & Assert
        _service.GetStepLabel(step).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "Platform")]
    [InlineData(2, "Deployment")]
    [InlineData(3, "Technology")]
    [InlineData(4, "VM Config")]
    [InlineData(5, "Results")]
    public void GetStepLabel_VMDeployment_ReturnsCorrectLabel(int step, string expected)
    {
        // Arrange
        _service.SelectedDeployment = DeploymentModel.VMs;

        // Act & Assert
        _service.GetStepLabel(step).Should().Be(expected);
    }

    [Fact]
    public void GetStepLabel_InvalidStep_ReturnsEmptyString()
    {
        // Act & Assert
        _service.GetStepLabel(99).Should().BeEmpty();
    }

    #endregion

    #region CanNavigateToStep Tests

    [Fact]
    public void CanNavigateToStep_PreviousStep_ReturnsTrue()
    {
        // Arrange
        _service.CurrentStep = 3;

        // Act & Assert
        _service.CanNavigateToStep(2).Should().BeTrue();
        _service.CanNavigateToStep(1).Should().BeTrue();
    }

    [Fact]
    public void CanNavigateToStep_CurrentStep_ReturnsFalse()
    {
        // Arrange
        _service.CurrentStep = 3;

        // Act & Assert
        _service.CanNavigateToStep(3).Should().BeFalse();
    }

    [Fact]
    public void CanNavigateToStep_FutureStep_ReturnsFalse()
    {
        // Arrange
        _service.CurrentStep = 3;

        // Act & Assert
        _service.CanNavigateToStep(4).Should().BeFalse();
        _service.CanNavigateToStep(5).Should().BeFalse();
    }

    #endregion

    #region GetSingleClusterEnvironment Tests

    [Theory]
    [InlineData("Dev", EnvironmentType.Dev)]
    [InlineData("Test", EnvironmentType.Test)]
    [InlineData("Stage", EnvironmentType.Stage)]
    [InlineData("Prod", EnvironmentType.Prod)]
    [InlineData("DR", EnvironmentType.DR)]
    public void GetSingleClusterEnvironment_ReturnsCorrectEnvironment(
        string scope, EnvironmentType expected)
    {
        // Arrange
        _service.SingleClusterScope = scope;

        // Act & Assert
        _service.GetSingleClusterEnvironment().Should().Be(expected);
    }

    [Fact]
    public void GetSingleClusterEnvironment_UnknownScope_DefaultsToProd()
    {
        // Arrange
        _service.SingleClusterScope = "Unknown";

        // Act & Assert
        _service.GetSingleClusterEnvironment().Should().Be(EnvironmentType.Prod);
    }

    #endregion

    #region IsSingleClusterMode Tests

    [Fact]
    public void IsSingleClusterMode_SharedCluster_ReturnsTrue()
    {
        // Arrange
        _service.SelectedClusterMode = ClusterMode.SharedCluster;

        // Act & Assert
        _service.IsSingleClusterMode().Should().BeTrue();
    }

    [Fact]
    public void IsSingleClusterMode_PerEnvironment_ReturnsTrue()
    {
        // Arrange
        _service.SelectedClusterMode = ClusterMode.PerEnvironment;

        // Act & Assert
        _service.IsSingleClusterMode().Should().BeTrue();
    }

    [Fact]
    public void IsSingleClusterMode_MultiCluster_ReturnsFalse()
    {
        // Arrange
        _service.SelectedClusterMode = ClusterMode.MultiCluster;

        // Act & Assert
        _service.IsSingleClusterMode().Should().BeFalse();
    }

    #endregion

    #region IsProdEnvironment Tests

    [Fact]
    public void IsProdEnvironment_Prod_ReturnsTrue()
    {
        // Act & Assert
        _service.IsProdEnvironment(EnvironmentType.Prod).Should().BeTrue();
    }

    [Fact]
    public void IsProdEnvironment_DR_ReturnsTrue()
    {
        // Act & Assert
        _service.IsProdEnvironment(EnvironmentType.DR).Should().BeTrue();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void IsProdEnvironment_NonProd_ReturnsFalse(EnvironmentType env)
    {
        // Act & Assert
        _service.IsProdEnvironment(env).Should().BeFalse();
    }

    #endregion

    #region NotifyStateChanged Tests

    [Fact]
    public void NotifyStateChanged_NoSubscribers_DoesNotThrow()
    {
        // Act & Assert
        var action = () => _service.NotifyStateChanged();
        action.Should().NotThrow();
    }

    [Fact]
    public void NotifyStateChanged_WithSubscriber_FiresEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;

        // Act
        _service.NotifyStateChanged();

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region EnvApps Tests

    [Fact]
    public void EnvApps_HasDefaultMediumApps()
    {
        // Assert
        foreach (var env in _service.EnabledEnvironments)
        {
            _service.EnvApps[env].Medium.Should().Be(70);
        }
    }

    [Fact]
    public void EnvApps_CanBeModified()
    {
        // Arrange & Act
        _service.EnvApps[EnvironmentType.Prod].Medium = 100;
        _service.EnvApps[EnvironmentType.Prod].Large = 20;

        // Assert
        _service.EnvApps[EnvironmentType.Prod].Medium.Should().Be(100);
        _service.EnvApps[EnvironmentType.Prod].Large.Should().Be(20);
    }

    #endregion

    #region Headroom Tests

    [Fact]
    public void Headroom_CanBeModified()
    {
        // Arrange & Act
        _service.Headroom[EnvironmentType.Prod] = 50;

        // Assert
        _service.Headroom[EnvironmentType.Prod].Should().Be(50);
    }

    #endregion

    #region Replicas Tests

    [Fact]
    public void Replicas_CanBeModified()
    {
        // Arrange & Act
        _service.Replicas[EnvironmentType.Prod] = 5;

        // Assert
        _service.Replicas[EnvironmentType.Prod].Should().Be(5);
    }

    #endregion

    #region EnabledEnvironments Tests

    [Fact]
    public void EnabledEnvironments_CanRemoveEnvironment()
    {
        // Act
        _service.EnabledEnvironments.Remove(EnvironmentType.DR);

        // Assert
        _service.EnabledEnvironments.Should().NotContain(EnvironmentType.DR);
        _service.EnabledEnvironments.Should().HaveCount(4);
    }

    [Fact]
    public void EnabledEnvironments_CanAddEnvironment()
    {
        // Arrange
        _service.EnabledEnvironments.Clear();

        // Act
        _service.EnabledEnvironments.Add(EnvironmentType.Prod);

        // Assert
        _service.EnabledEnvironments.Should().Contain(EnvironmentType.Prod);
    }

    #endregion
}
