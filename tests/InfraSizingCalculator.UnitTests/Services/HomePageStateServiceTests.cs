using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Growth;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for HomePageStateService - manages Home page state and wizard navigation.
/// This service was extracted from Home.razor to improve testability.
/// </summary>
public class HomePageStateServiceTests
{
    private readonly IDistributionService _distributionService;
    private readonly ITechnologyService _technologyService;
    private readonly HomePageStateService _sut;

    public HomePageStateServiceTests()
    {
        _distributionService = Substitute.For<IDistributionService>();
        _technologyService = Substitute.For<ITechnologyService>();
        _sut = new HomePageStateService(_distributionService, _technologyService);
    }

    #region Initial State Tests

    [Fact]
    public void Constructor_SetsDefaultCurrentStep_ToOne()
    {
        _sut.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedPlatform_ToNull()
    {
        _sut.SelectedPlatform.Should().BeNull();
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedDeployment_ToNull()
    {
        _sut.SelectedDeployment.Should().BeNull();
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedTechnology_ToNull()
    {
        _sut.SelectedTechnology.Should().BeNull();
    }

    [Fact]
    public void Constructor_SetsDefaultSelectedDistribution_ToNull()
    {
        _sut.SelectedDistribution.Should().BeNull();
    }

    [Fact]
    public void Constructor_SetsDefaultClusterMode_ToMultiCluster()
    {
        _sut.SelectedClusterMode.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public void Constructor_SetsDefaultSingleClusterScope_ToShared()
    {
        _sut.SingleClusterScope.Should().Be("Shared");
    }

    [Fact]
    public void Constructor_SetsDefaultEnabledEnvironments_ToAllFour()
    {
        _sut.EnabledEnvironments.Should().HaveCount(4);
        _sut.EnabledEnvironments.Should().Contain(EnvironmentType.Dev);
        _sut.EnabledEnvironments.Should().Contain(EnvironmentType.Test);
        _sut.EnabledEnvironments.Should().Contain(EnvironmentType.Stage);
        _sut.EnabledEnvironments.Should().Contain(EnvironmentType.Prod);
    }

    [Fact]
    public void Constructor_SetsDefaultEnvironmentApps_WithMedium70()
    {
        _sut.EnvironmentApps.Should().HaveCount(4);
        foreach (var env in new[] { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod })
        {
            _sut.EnvironmentApps[env].Medium.Should().Be(70);
        }
    }

    [Fact]
    public void Constructor_SetsDefaultHeadroom_WithCorrectValues()
    {
        _sut.Headroom[EnvironmentType.Dev].Should().Be(33);
        _sut.Headroom[EnvironmentType.Test].Should().Be(33);
        _sut.Headroom[EnvironmentType.Stage].Should().Be(0);
        _sut.Headroom[EnvironmentType.Prod].Should().Be(37.5);
    }

    [Fact]
    public void Constructor_SetsDefaultReplicas_WithCorrectValues()
    {
        _sut.Replicas[EnvironmentType.Dev].Should().Be(1);
        _sut.Replicas[EnvironmentType.Test].Should().Be(1);
        _sut.Replicas[EnvironmentType.Stage].Should().Be(2);
        _sut.Replicas[EnvironmentType.Prod].Should().Be(3);
    }

    [Fact]
    public void Constructor_SetsDefaultOvercommit_ToOne()
    {
        _sut.ProdCpuOvercommit.Should().Be(1.0);
        _sut.ProdMemoryOvercommit.Should().Be(1.0);
        _sut.NonProdCpuOvercommit.Should().Be(1.0);
        _sut.NonProdMemoryOvercommit.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_SetsDefaultVMSystemOverhead_To15()
    {
        _sut.VMSystemOverhead.Should().Be(15);
    }

    [Fact]
    public void Constructor_SetsDefaultConfigTab_ToApps()
    {
        _sut.ConfigTab.Should().Be("apps");
    }

    [Fact]
    public void Constructor_SetsDefaultDistributionFilter_ToOnPrem()
    {
        _sut.DistributionFilter.Should().Be("on-prem");
    }

    [Fact]
    public void Constructor_SetsDefaultCloudCategory_ToMajor()
    {
        _sut.CloudCategory.Should().Be("major");
    }

    #endregion

    #region Wizard Navigation Tests

    [Fact]
    public void GoToStep_WithValidStep_UpdatesCurrentStep()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        _sut.GoToStep(3);

        // Assert
        _sut.CurrentStep.Should().Be(3);
    }

    [Fact]
    public void GoToStep_WithStepLessThanOne_DoesNotChange()
    {
        // Act
        _sut.GoToStep(0);

        // Assert
        _sut.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void GoToStep_WithStepGreaterThanTotal_DoesNotChange()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes; // 7 steps

        // Act
        _sut.GoToStep(10);

        // Assert
        _sut.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void GoToStep_FiresStateChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.StateChanged += () => eventFired = true;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        _sut.GoToStep(2);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void NextStep_WhenCanProceed_IncrementsStep()
    {
        // Arrange
        _sut.SelectedPlatform = PlatformType.Native;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        _sut.NextStep();

        // Assert
        _sut.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void NextStep_WhenCannotProceed_DoesNotChange()
    {
        // Arrange - step 1 with no platform selected
        _sut.SelectedPlatform = null;

        // Act
        _sut.NextStep();

        // Assert
        _sut.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void NextStep_WhenAtLastStep_DoesNotChange()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.CurrentStep = 7; // Last step for K8s
        _sut.SelectedPlatform = PlatformType.Native;
        _sut.SelectedTechnology = Technology.DotNet;
        _sut.SelectedDistribution = Distribution.OpenShift;
        _sut.EnvironmentApps[EnvironmentType.Prod] = new AppConfig { Medium = 10 };

        // Act
        _sut.NextStep();

        // Assert
        _sut.CurrentStep.Should().Be(7);
    }

    [Fact]
    public void PreviousStep_WhenNotAtFirstStep_DecrementsStep()
    {
        // Arrange
        _sut.CurrentStep = 3;

        // Act
        _sut.PreviousStep();

        // Assert
        _sut.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void PreviousStep_WhenAtFirstStep_DoesNotChange()
    {
        // Act
        _sut.PreviousStep();

        // Assert
        _sut.CurrentStep.Should().Be(1);
    }

    #endregion

    #region GetTotalSteps Tests

    [Fact]
    public void GetTotalSteps_ForKubernetes_ReturnsSeven()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        var result = _sut.GetTotalSteps();

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    public void GetTotalSteps_ForVMsWithMendix_ReturnsSeven()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.VMs;
        _sut.SelectedTechnology = Technology.Mendix;

        // Act
        var result = _sut.GetTotalSteps();

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    public void GetTotalSteps_ForVMsWithOtherTechnology_ReturnsSix()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.VMs;
        _sut.SelectedTechnology = Technology.DotNet;

        // Act
        var result = _sut.GetTotalSteps();

        // Assert
        result.Should().Be(6);
    }

    [Fact]
    public void GetTotalSteps_WithNoDeployment_ReturnsSix()
    {
        // Act
        var result = _sut.GetTotalSteps();

        // Assert
        result.Should().Be(6);
    }

    #endregion

    #region GetStepLabel Tests

    [Theory]
    [InlineData(1, "Platform")]
    [InlineData(2, "Deployment")]
    [InlineData(3, "Technology")]
    [InlineData(4, "Distribution")]
    [InlineData(5, "Configure")]
    [InlineData(6, "Pricing")]
    [InlineData(7, "Results")]
    public void GetStepLabel_ForKubernetes_ReturnsCorrectLabels(int step, string expected)
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        var result = _sut.GetStepLabel(step);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "Platform")]
    [InlineData(2, "Deployment")]
    [InlineData(3, "Technology")]
    [InlineData(4, "Deployment Type")]
    [InlineData(5, "Configure")]
    [InlineData(6, "Pricing")]
    [InlineData(7, "Results")]
    public void GetStepLabel_ForMendixVMs_ReturnsCorrectLabels(int step, string expected)
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.VMs;
        _sut.SelectedTechnology = Technology.Mendix;

        // Act
        var result = _sut.GetStepLabel(step);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "Platform")]
    [InlineData(2, "Deployment")]
    [InlineData(3, "Technology")]
    [InlineData(4, "Configure")]
    [InlineData(5, "Pricing")]
    [InlineData(6, "Results")]
    public void GetStepLabel_ForNonMendixVMs_ReturnsCorrectLabels(int step, string expected)
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.VMs;
        _sut.SelectedTechnology = Technology.DotNet;

        // Act
        var result = _sut.GetStepLabel(step);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetStepLabel_WithInvalidStep_ReturnsEmpty()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        var result = _sut.GetStepLabel(99);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetStepSelection Tests

    [Fact]
    public void GetStepSelection_Step1_ReturnsPlatform()
    {
        // Arrange
        _sut.SelectedPlatform = PlatformType.Native;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        var result = _sut.GetStepSelection(1);

        // Assert
        result.Should().Be("Native");
    }

    [Fact]
    public void GetStepSelection_Step2_ReturnsDeployment()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        var result = _sut.GetStepSelection(2);

        // Assert
        result.Should().Be("Kubernetes");
    }

    [Fact]
    public void GetStepSelection_Step3_ReturnsTechnology()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedTechnology = Technology.DotNet;

        // Act
        var result = _sut.GetStepSelection(3);

        // Assert
        result.Should().Be("DotNet");
    }

    [Fact]
    public void GetStepSelection_Step4_ReturnsDistribution()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedDistribution = Distribution.OpenShift;

        // Act
        var result = _sut.GetStepSelection(4);

        // Assert
        result.Should().Be("OpenShift");
    }

    [Fact]
    public void GetStepSelection_Step5_ForMultiCluster_ReturnsMultiCluster()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedClusterMode = ClusterMode.MultiCluster;

        // Act
        var result = _sut.GetStepSelection(5);

        // Assert
        result.Should().Be("Multi-Cluster");
    }

    [Fact]
    public void GetStepSelection_Step5_ForSingleCluster_ReturnsSingleWithScope()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedClusterMode = ClusterMode.SharedCluster;
        _sut.SingleClusterScope = "Prod";

        // Act
        var result = _sut.GetStepSelection(5);

        // Assert
        result.Should().Be("Single (Prod)");
    }

    [Fact]
    public void GetStepSelection_Step6_WithCosts_ReturnsConfigured()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        // HasCosts is computed: true when !IsOnPrem OR IncludePricing OR MendixCost != null
        _sut.PricingResult = new PricingStepResult { IsOnPrem = false };

        // Act
        var result = _sut.GetStepSelection(6);

        // Assert
        result.Should().Be("Configured");
    }

    [Fact]
    public void GetStepSelection_Step6_OnPremNoCosts_ReturnsNA()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        // HasCosts is false when IsOnPrem = true AND IncludePricing = false AND MendixCost = null
        _sut.PricingResult = new PricingStepResult { IsOnPrem = true, IncludePricing = false };

        // Act
        var result = _sut.GetStepSelection(6);

        // Assert
        result.Should().Be("N/A");
    }

    [Fact]
    public void GetStepSelection_ForVMs_Step2_ReturnsVirtualMachines()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.VMs;
        _sut.SelectedTechnology = Technology.DotNet;

        // Act
        var result = _sut.GetStepSelection(2);

        // Assert
        result.Should().Be("Virtual Machines");
    }

    [Fact]
    public void GetStepSelection_ForVMs_Step4_ReturnsEnvCount()
    {
        // Arrange
        _sut.SelectedDeployment = DeploymentModel.VMs;
        _sut.SelectedTechnology = Technology.DotNet;
        _sut.EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };

        // Act
        var result = _sut.GetStepSelection(4);

        // Assert
        result.Should().Be("2 env(s)");
    }

    #endregion

    #region CanProceed Tests

    [Fact]
    public void CanProceed_Step1_WithPlatform_ReturnsTrue()
    {
        // Arrange
        _sut.CurrentStep = 1;
        _sut.SelectedPlatform = PlatformType.Native;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanProceed_Step1_WithoutPlatform_ReturnsFalse()
    {
        // Arrange
        _sut.CurrentStep = 1;
        _sut.SelectedPlatform = null;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanProceed_Step2_WithDeployment_ReturnsTrue()
    {
        // Arrange
        _sut.CurrentStep = 2;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanProceed_Step3_WithTechnology_ReturnsTrue()
    {
        // Arrange
        _sut.CurrentStep = 3;
        _sut.SelectedTechnology = Technology.DotNet;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanProceed_Step4_K8sWithDistribution_ReturnsTrue()
    {
        // Arrange
        _sut.CurrentStep = 4;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedDistribution = Distribution.EKS;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanProceed_Step4_K8sWithoutDistribution_ReturnsFalse()
    {
        // Arrange
        _sut.CurrentStep = 4;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedDistribution = null;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanProceed_Step5_K8sWithApps_ReturnsTrue()
    {
        // Arrange
        _sut.CurrentStep = 5;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.EnvironmentApps[EnvironmentType.Prod] = new AppConfig { Medium = 10 };

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanProceed_Step6_AlwaysReturnsTrue()
    {
        // Arrange
        _sut.CurrentStep = 6;

        // Act
        var result = _sut.CanProceed();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Cluster Mode Tests

    [Fact]
    public void IsSingleClusterMode_WithMultiCluster_ReturnsFalse()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.MultiCluster;

        // Act
        var result = _sut.IsSingleClusterMode();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSingleClusterMode_WithSharedCluster_ReturnsTrue()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.SharedCluster;

        // Act
        var result = _sut.IsSingleClusterMode();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSingleClusterMode_WithPerEnvironment_ReturnsTrue()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.PerEnvironment;

        // Act
        var result = _sut.IsSingleClusterMode();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(ClusterMode.SharedCluster, 1)]
    [InlineData(ClusterMode.PerEnvironment, 1)]
    public void GetClusterCount_ForSingleModes_ReturnsOne(ClusterMode mode, int expected)
    {
        // Arrange
        _sut.SelectedClusterMode = mode;

        // Act
        var result = _sut.GetClusterCount();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetClusterCount_ForMultiCluster_ReturnsEnvironmentCount()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.MultiCluster;
        _sut.K8sResults = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new(), new(), new()
            },
            GrandTotal = new GrandTotal(),
            Configuration = new K8sSizingInput()
        };

        // Act
        var result = _sut.GetClusterCount();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void GetClusterCount_ForMultiCluster_WithNoResults_ReturnsZero()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.MultiCluster;
        _sut.K8sResults = null;

        // Act
        var result = _sut.GetClusterCount();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Environment Methods Tests

    [Fact]
    public void IsEnvironmentEnabled_ForEnabledEnv_ReturnsTrue()
    {
        // Arrange - Dev is enabled by default
        // Act
        var result = _sut.IsEnvironmentEnabled(EnvironmentType.Dev);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEnvironmentEnabled_ForDisabledEnv_ReturnsFalse()
    {
        // Arrange
        _sut.EnabledEnvironments.Remove(EnvironmentType.Dev);

        // Act
        var result = _sut.IsEnvironmentEnabled(EnvironmentType.Dev);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToggleEnvironment_EnablesEnvironment()
    {
        // Arrange
        _sut.EnabledEnvironments.Remove(EnvironmentType.Dev);

        // Act
        _sut.ToggleEnvironment(EnvironmentType.Dev, true);

        // Assert
        _sut.EnabledEnvironments.Should().Contain(EnvironmentType.Dev);
    }

    [Fact]
    public void ToggleEnvironment_DisablesEnvironment()
    {
        // Act
        _sut.ToggleEnvironment(EnvironmentType.Dev, false);

        // Assert
        _sut.EnabledEnvironments.Should().NotContain(EnvironmentType.Dev);
    }

    [Fact]
    public void ToggleEnvironment_FiresStateChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.StateChanged += () => eventFired = true;

        // Act
        _sut.ToggleEnvironment(EnvironmentType.Dev, false);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void GetVisibleEnvironments_ForMultiCluster_ReturnsAllFour()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.MultiCluster;

        // Act
        var result = _sut.GetVisibleEnvironments().ToList();

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(EnvironmentType.Dev);
        result.Should().Contain(EnvironmentType.Test);
        result.Should().Contain(EnvironmentType.Stage);
        result.Should().Contain(EnvironmentType.Prod);
    }

    [Fact]
    public void GetVisibleEnvironments_ForSingleEnvProd_ReturnsOnlyProd()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.PerEnvironment;
        _sut.SingleClusterScope = "Prod";

        // Act
        var result = _sut.GetVisibleEnvironments().ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(EnvironmentType.Prod);
    }

    [Fact]
    public void GetVisibleEnvironments_ForSharedCluster_ReturnsAllFour()
    {
        // Arrange
        _sut.SelectedClusterMode = ClusterMode.SharedCluster;
        _sut.SingleClusterScope = "Shared";

        // Act
        var result = _sut.GetVisibleEnvironments().ToList();

        // Assert
        result.Should().HaveCount(4);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsAllState()
    {
        // Arrange
        _sut.CurrentStep = 5;
        _sut.SelectedPlatform = PlatformType.LowCode;
        _sut.SelectedDeployment = DeploymentModel.Kubernetes;
        _sut.SelectedTechnology = Technology.Mendix;
        _sut.SelectedDistribution = Distribution.AKS;
        _sut.K8sResults = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal(),
            Configuration = new K8sSizingInput()
        };

        // Act
        _sut.Reset();

        // Assert
        _sut.CurrentStep.Should().Be(1);
        _sut.SelectedPlatform.Should().BeNull();
        _sut.SelectedDeployment.Should().BeNull();
        _sut.SelectedTechnology.Should().BeNull();
        _sut.SelectedDistribution.Should().BeNull();
        _sut.K8sResults.Should().BeNull();
    }

    [Fact]
    public void Reset_FiresStateChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.StateChanged += () => eventFired = true;

        // Act
        _sut.Reset();

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void ResetConfiguration_ResetsAppsAndSpecs()
    {
        // Arrange
        _sut.EnvironmentApps[EnvironmentType.Prod] = new AppConfig { Large = 100 };
        _sut.NodeSpecs = new NodeSpecsConfig { ProdWorkerCpu = 32 };

        // Act
        _sut.ResetConfiguration();

        // Assert
        _sut.EnvironmentApps[EnvironmentType.Prod].Medium.Should().Be(70);
        _sut.NodeSpecs.ProdWorkerCpu.Should().Be(16); // Default is 16
    }

    [Fact]
    public void ResetEnvironmentApps_ResetsToDefaults()
    {
        // Arrange
        _sut.EnvironmentApps[EnvironmentType.Dev] = new AppConfig { Small = 50, Medium = 30, Large = 20 };

        // Act
        _sut.ResetEnvironmentApps();

        // Assert
        _sut.EnvironmentApps[EnvironmentType.Dev].Small.Should().Be(0);
        _sut.EnvironmentApps[EnvironmentType.Dev].Medium.Should().Be(70);
        _sut.EnvironmentApps[EnvironmentType.Dev].Large.Should().Be(0);
    }

    #endregion

    #region StateChanged Event Tests

    [Fact]
    public void NotifyStateChanged_FiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.StateChanged += () => eventFired = true;

        // Act
        _sut.NotifyStateChanged();

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void StateChanged_CanHaveMultipleSubscribers()
    {
        // Arrange
        int callCount = 0;
        _sut.StateChanged += () => callCount++;
        _sut.StateChanged += () => callCount++;
        _sut.StateChanged += () => callCount++;

        // Act
        _sut.NotifyStateChanged();

        // Assert
        callCount.Should().Be(3);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void K8sResults_CanBeAssigned()
    {
        // Arrange
        var results = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal { TotalNodes = 10 },
            Configuration = new K8sSizingInput()
        };

        // Act
        _sut.K8sResults = results;

        // Assert
        _sut.K8sResults.Should().BeSameAs(results);
        _sut.K8sResults.GrandTotal.TotalNodes.Should().Be(10);
    }

    [Fact]
    public void VMResults_CanBeAssigned()
    {
        // Arrange
        var results = new VMSizingResult
        {
            GrandTotal = new VMGrandTotal { TotalVMs = 8 }
        };

        // Act
        _sut.VMResults = results;

        // Assert
        _sut.VMResults.Should().BeSameAs(results);
        _sut.VMResults.GrandTotal.TotalVMs.Should().Be(8);
    }

    [Fact]
    public void CostEstimate_CanBeAssigned()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 5000m };

        // Act
        _sut.K8sCostEstimate = estimate;

        // Assert
        _sut.K8sCostEstimate.Should().BeSameAs(estimate);
        _sut.K8sCostEstimate.MonthlyTotal.Should().Be(5000m);
    }

    [Fact]
    public void GrowthProjection_CanBeAssigned()
    {
        // Arrange
        var projection = new GrowthProjection
        {
            Summary = new ProjectionSummary { TotalAppGrowth = 50 }
        };

        // Act
        _sut.K8sGrowthProjection = projection;

        // Assert
        _sut.K8sGrowthProjection.Should().BeSameAs(projection);
        _sut.K8sGrowthProjection.Summary.TotalAppGrowth.Should().Be(50);
    }

    [Fact]
    public void VMEnvironmentConfigs_CanBeModified()
    {
        // Arrange
        var config = new VMEnvironmentConfig();

        // Act
        _sut.VMEnvironmentConfigs[EnvironmentType.Prod] = config;

        // Assert
        _sut.VMEnvironmentConfigs.Should().ContainKey(EnvironmentType.Prod);
        _sut.VMEnvironmentConfigs[EnvironmentType.Prod].Should().BeSameAs(config);
    }

    [Fact]
    public void ValidationWarnings_CanBeAssigned()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            new() { Message = "Test warning" }
        };

        // Act
        _sut.ValidationWarnings = warnings;

        // Assert
        _sut.ValidationWarnings.Should().HaveCount(1);
        _sut.ValidationWarnings![0].Message.Should().Be("Test warning");
    }

    #endregion
}
