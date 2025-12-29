using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for HomePageCalculationService - Core calculation logic extracted from Home.razor.
/// </summary>
public class HomePageCalculationServiceTests
{
    private readonly HomePageCalculationService _sut;
    private readonly IHomePageStateService _mockState;
    private readonly IDistributionService _mockDistributionService;
    private readonly IK8sSizingService _mockK8sSizingService;
    private readonly IVMSizingService _mockVmSizingService;

    public HomePageCalculationServiceTests()
    {
        _sut = new HomePageCalculationService();
        _mockState = Substitute.For<IHomePageStateService>();
        _mockDistributionService = Substitute.For<IDistributionService>();
        _mockK8sSizingService = Substitute.For<IK8sSizingService>();
        _mockVmSizingService = Substitute.For<IVMSizingService>();

        // Set up default state values
        SetupDefaultState();
        SetupDefaultDistributionService();
    }

    private static DistributionConfig CreateDistributionConfig(
        Distribution distribution = Distribution.OpenShift,
        string name = "OpenShift",
        string vendor = "Red Hat",
        bool hasManagedControlPlane = false,
        bool hasInfraNodes = true)
    {
        return new DistributionConfig
        {
            Distribution = distribution,
            Name = name,
            Vendor = vendor,
            HasManagedControlPlane = hasManagedControlPlane,
            HasInfraNodes = hasInfraNodes,
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100)
        };
    }

    private void SetupDefaultState()
    {
        _mockState.SelectedDistribution.Returns(Distribution.OpenShift);
        _mockState.SelectedTechnology.Returns(Technology.DotNet);
        _mockState.SelectedClusterMode.Returns(ClusterMode.MultiCluster);
        _mockState.SingleClusterScope.Returns("Shared");
        _mockState.EnabledEnvironments.Returns(new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod
        });
        _mockState.EnvironmentApps.Returns(new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Medium = 10 } },
            { EnvironmentType.Test, new AppConfig { Medium = 10 } },
            { EnvironmentType.Stage, new AppConfig { Medium = 10 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 20 } }
        });
        _mockState.Headroom.Returns(new Dictionary<EnvironmentType, double>
        {
            { EnvironmentType.Dev, 33 },
            { EnvironmentType.Test, 33 },
            { EnvironmentType.Stage, 0 },
            { EnvironmentType.Prod, 37.5 }
        });
        _mockState.Replicas.Returns(new Dictionary<EnvironmentType, int>
        {
            { EnvironmentType.Dev, 1 },
            { EnvironmentType.Test, 1 },
            { EnvironmentType.Stage, 2 },
            { EnvironmentType.Prod, 3 }
        });
        _mockState.ProdCpuOvercommit.Returns(1.0);
        _mockState.ProdMemoryOvercommit.Returns(1.0);
        _mockState.NonProdCpuOvercommit.Returns(2.0);
        _mockState.NonProdMemoryOvercommit.Returns(1.5);
        _mockState.NodeSpecs.Returns(new NodeSpecsConfig());
        _mockState.K8sHADRConfig.Returns(new K8sHADRConfig());
        _mockState.VMEnvironmentConfigs.Returns(new Dictionary<EnvironmentType, VMEnvironmentConfig>());
        _mockState.VMSystemOverhead.Returns(15.0);
    }

    private void SetupDefaultDistributionService()
    {
        _mockDistributionService.GetConfig(Arg.Any<Distribution>()).Returns(CreateDistributionConfig());
    }

    #region GetEffectiveClusterMode Tests

    [Fact]
    public void GetEffectiveClusterMode_NullMode_ReturnsMultiCluster()
    {
        var result = _sut.GetEffectiveClusterMode(null, "Shared");

        result.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public void GetEffectiveClusterMode_MultiCluster_ReturnsMultiCluster()
    {
        var result = _sut.GetEffectiveClusterMode(ClusterMode.MultiCluster, "Shared");

        result.Should().Be(ClusterMode.MultiCluster);
    }

    [Theory]
    [InlineData(ClusterMode.SharedCluster)]
    [InlineData(ClusterMode.PerEnvironment)]
    public void GetEffectiveClusterMode_SingleClusterWithShared_ReturnsSharedCluster(ClusterMode mode)
    {
        var result = _sut.GetEffectiveClusterMode(mode, "Shared");

        result.Should().Be(ClusterMode.SharedCluster);
    }

    [Theory]
    [InlineData(ClusterMode.SharedCluster, "Dev")]
    [InlineData(ClusterMode.SharedCluster, "Prod")]
    [InlineData(ClusterMode.PerEnvironment, "Test")]
    [InlineData(ClusterMode.PerEnvironment, "Stage")]
    public void GetEffectiveClusterMode_SingleClusterWithSpecificEnv_ReturnsPerEnvironment(ClusterMode mode, string scope)
    {
        var result = _sut.GetEffectiveClusterMode(mode, scope);

        result.Should().Be(ClusterMode.PerEnvironment);
    }

    #endregion

    #region GetSingleClusterEnvironment Tests

    [Theory]
    [InlineData("Dev", EnvironmentType.Dev)]
    [InlineData("Test", EnvironmentType.Test)]
    [InlineData("Stage", EnvironmentType.Stage)]
    [InlineData("Prod", EnvironmentType.Prod)]
    [InlineData("DR", EnvironmentType.DR)]
    public void GetSingleClusterEnvironment_ValidScope_ReturnsCorrectEnvironment(string scope, EnvironmentType expected)
    {
        var result = _sut.GetSingleClusterEnvironment(scope);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Shared")]
    [InlineData("Invalid")]
    [InlineData("")]
    public void GetSingleClusterEnvironment_InvalidScope_ReturnsProd(string scope)
    {
        var result = _sut.GetSingleClusterEnvironment(scope);

        result.Should().Be(EnvironmentType.Prod);
    }

    #endregion

    #region BuildPerEnvironmentSpecs Tests

    [Fact]
    public void BuildPerEnvironmentSpecs_MultiClusterMode_ReturnsAllSpecs()
    {
        var nodeSpecs = new NodeSpecsConfig();

        var (controlPlane, worker, infra) = _sut.BuildPerEnvironmentSpecs(nodeSpecs, ClusterMode.MultiCluster);

        controlPlane.Should().NotBeNull();
        controlPlane.Should().ContainKeys(EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod, EnvironmentType.DR);

        worker.Should().NotBeNull();
        worker.Should().ContainKeys(EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod, EnvironmentType.DR);

        infra.Should().NotBeNull();
        infra.Should().ContainKeys(EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod, EnvironmentType.DR);
    }

    [Theory]
    [InlineData(ClusterMode.SharedCluster)]
    [InlineData(ClusterMode.PerEnvironment)]
    public void BuildPerEnvironmentSpecs_NonMultiClusterMode_ReturnsNull(ClusterMode mode)
    {
        var nodeSpecs = new NodeSpecsConfig();

        var (controlPlane, worker, infra) = _sut.BuildPerEnvironmentSpecs(nodeSpecs, mode);

        controlPlane.Should().BeNull();
        worker.Should().BeNull();
        infra.Should().BeNull();
    }

    [Fact]
    public void BuildPerEnvironmentSpecs_NullMode_ReturnsNull()
    {
        var nodeSpecs = new NodeSpecsConfig();

        var (controlPlane, worker, infra) = _sut.BuildPerEnvironmentSpecs(nodeSpecs, null);

        controlPlane.Should().BeNull();
        worker.Should().BeNull();
        infra.Should().BeNull();
    }

    [Fact]
    public void BuildPerEnvironmentSpecs_MultiCluster_UsesCorrectSpecsPerEnvironment()
    {
        var nodeSpecs = new NodeSpecsConfig
        {
            // Prod specs
            ProdMasterCpu = 8,
            ProdMasterRam = 32,
            ProdWorkerCpu = 16,
            ProdWorkerRam = 64,
            // Dev specs (per-environment)
            DevMasterCpu = 4,
            DevMasterRam = 16,
            DevWorkerCpu = 8,
            DevWorkerRam = 32
        };

        var (controlPlane, worker, _) = _sut.BuildPerEnvironmentSpecs(nodeSpecs, ClusterMode.MultiCluster);

        // Prod should use prod specs
        controlPlane![EnvironmentType.Prod].Cpu.Should().Be(8);
        controlPlane[EnvironmentType.Prod].Ram.Should().Be(32);
        worker![EnvironmentType.Prod].Cpu.Should().Be(16);
        worker[EnvironmentType.Prod].Ram.Should().Be(64);

        // Dev should use dev specs (per-environment)
        controlPlane[EnvironmentType.Dev].Cpu.Should().Be(4);
        controlPlane[EnvironmentType.Dev].Ram.Should().Be(16);
        worker[EnvironmentType.Dev].Cpu.Should().Be(8);
        worker[EnvironmentType.Dev].Ram.Should().Be(32);
    }

    #endregion

    #region BuildK8sInput Tests

    [Fact]
    public void BuildK8sInput_ValidState_ReturnsInput()
    {
        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Should().NotBeNull();
        result.Distribution.Should().Be(Distribution.OpenShift);
        result.Technology.Should().Be(Technology.DotNet);
        result.ClusterMode.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public void BuildK8sInput_NullDistribution_ThrowsInvalidOperationException()
    {
        _mockState.SelectedDistribution.Returns((Distribution?)null);

        var act = () => _sut.BuildK8sInput(_mockState, _mockDistributionService);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Distribution*");
    }

    [Fact]
    public void BuildK8sInput_NullTechnology_ThrowsInvalidOperationException()
    {
        _mockState.SelectedTechnology.Returns((Technology?)null);

        var act = () => _sut.BuildK8sInput(_mockState, _mockDistributionService);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Technology*");
    }

    [Fact]
    public void BuildK8sInput_NullClusterMode_ThrowsInvalidOperationException()
    {
        _mockState.SelectedClusterMode.Returns((ClusterMode?)null);

        var act = () => _sut.BuildK8sInput(_mockState, _mockDistributionService);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ClusterMode*");
    }

    [Fact]
    public void BuildK8sInput_SharedCluster_UsesSharedClusterMode()
    {
        _mockState.SelectedClusterMode.Returns(ClusterMode.SharedCluster);
        _mockState.SingleClusterScope.Returns("Shared");

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.ClusterMode.Should().Be(ClusterMode.SharedCluster);
    }

    [Fact]
    public void BuildK8sInput_SingleEnvironmentScope_UsesPerEnvironmentMode()
    {
        _mockState.SelectedClusterMode.Returns(ClusterMode.SharedCluster);
        _mockState.SingleClusterScope.Returns("Prod");

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.ClusterMode.Should().Be(ClusterMode.PerEnvironment);
        result.SelectedEnvironment.Should().Be(EnvironmentType.Prod);
    }

    [Fact]
    public void BuildK8sInput_SingleEnvironmentScope_SetsOnlySelectedEnvEnabled()
    {
        _mockState.SelectedClusterMode.Returns(ClusterMode.SharedCluster);
        _mockState.SingleClusterScope.Returns("Dev");

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.EnabledEnvironments.Should().ContainSingle()
            .Which.Should().Be(EnvironmentType.Dev);
    }

    [Fact]
    public void BuildK8sInput_CopiesOvercommitSettings()
    {
        _mockState.ProdCpuOvercommit.Returns(1.5);
        _mockState.ProdMemoryOvercommit.Returns(1.2);
        _mockState.NonProdCpuOvercommit.Returns(3.0);
        _mockState.NonProdMemoryOvercommit.Returns(2.0);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.ProdOvercommit.Cpu.Should().Be(1.5);
        result.ProdOvercommit.Memory.Should().Be(1.2);
        result.NonProdOvercommit.Cpu.Should().Be(3.0);
        result.NonProdOvercommit.Memory.Should().Be(2.0);
    }

    [Fact]
    public void BuildK8sInput_CopiesHeadroomSettings()
    {
        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Headroom.Dev.Should().Be(33);
        result.Headroom.Test.Should().Be(33);
        result.Headroom.Stage.Should().Be(0);
        result.Headroom.Prod.Should().Be(37.5);
    }

    [Fact]
    public void BuildK8sInput_CopiesReplicaSettings()
    {
        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Replicas.NonProd.Should().Be(1);
        result.Replicas.Stage.Should().Be(2);
        result.Replicas.Prod.Should().Be(3);
    }

    [Fact]
    public void BuildK8sInput_CopiesEnvironmentApps()
    {
        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.EnvironmentApps.Should().ContainKey(EnvironmentType.Dev);
        result.EnvironmentApps[EnvironmentType.Dev].Medium.Should().Be(10);
        result.EnvironmentApps.Should().ContainKey(EnvironmentType.Prod);
        result.EnvironmentApps[EnvironmentType.Prod].Medium.Should().Be(20);
    }

    [Fact]
    public void BuildK8sInput_SetsCustomNodeSpecs()
    {
        var nodeSpecs = new NodeSpecsConfig
        {
            ProdMasterCpu = 8,
            ProdMasterRam = 32,
            ProdMasterDisk = 200
        };
        _mockState.NodeSpecs.Returns(nodeSpecs);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.CustomNodeSpecs.Should().NotBeNull();
        result.CustomNodeSpecs!.ProdControlPlane.Cpu.Should().Be(8);
        result.CustomNodeSpecs.ProdControlPlane.Ram.Should().Be(32);
        result.CustomNodeSpecs.ProdControlPlane.Disk.Should().Be(200);
    }

    [Fact]
    public void BuildK8sInput_CopiesHADRConfig()
    {
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.WarmStandby };
        _mockState.K8sHADRConfig.Returns(hadrConfig);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.HADRConfig.Should().Be(hadrConfig);
    }

    [Fact]
    public void BuildK8sInput_MultiCluster_SetsPerEnvironmentSpecs()
    {
        _mockState.SelectedClusterMode.Returns(ClusterMode.MultiCluster);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.CustomNodeSpecs!.PerEnvControlPlane.Should().NotBeNull();
        result.CustomNodeSpecs.PerEnvWorker.Should().NotBeNull();
        result.CustomNodeSpecs.PerEnvInfra.Should().NotBeNull();
    }

    [Fact]
    public void BuildK8sInput_SharedCluster_NoPerEnvironmentSpecs()
    {
        _mockState.SelectedClusterMode.Returns(ClusterMode.SharedCluster);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.CustomNodeSpecs!.PerEnvControlPlane.Should().BeNull();
        result.CustomNodeSpecs.PerEnvWorker.Should().BeNull();
        result.CustomNodeSpecs.PerEnvInfra.Should().BeNull();
    }

    #endregion

    #region BuildVMInput Tests

    [Fact]
    public void BuildVMInput_ValidState_ReturnsInput()
    {
        var result = _sut.BuildVMInput(_mockState);

        result.Should().NotBeNull();
        result.Technology.Should().Be(Technology.DotNet);
    }

    [Fact]
    public void BuildVMInput_NullTechnology_ThrowsInvalidOperationException()
    {
        _mockState.SelectedTechnology.Returns((Technology?)null);

        var act = () => _sut.BuildVMInput(_mockState);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Technology*");
    }

    [Fact]
    public void BuildVMInput_CopiesEnabledEnvironments()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };
        _mockState.EnabledEnvironments.Returns(enabledEnvs);

        var result = _sut.BuildVMInput(_mockState);

        result.EnabledEnvironments.Should().BeEquivalentTo(enabledEnvs);
    }

    [Fact]
    public void BuildVMInput_CopiesEnvironmentConfigs()
    {
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>
        {
            { EnvironmentType.Prod, new VMEnvironmentConfig { Environment = EnvironmentType.Prod, StorageGB = 500 } }
        };
        _mockState.VMEnvironmentConfigs.Returns(configs);

        var result = _sut.BuildVMInput(_mockState);

        result.EnvironmentConfigs.Should().ContainKey(EnvironmentType.Prod);
        result.EnvironmentConfigs[EnvironmentType.Prod].StorageGB.Should().Be(500);
    }

    [Fact]
    public void BuildVMInput_CopiesSystemOverhead()
    {
        _mockState.VMSystemOverhead.Returns(20.0);

        var result = _sut.BuildVMInput(_mockState);

        result.SystemOverheadPercent.Should().Be(20.0);
    }

    #endregion

    #region CalculateK8s Tests

    [Fact]
    public void CalculateK8s_ValidState_CallsSizingService()
    {
        var expectedResult = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal(),
            Configuration = new K8sSizingInput()
        };
        _mockK8sSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        var result = _sut.CalculateK8s(_mockState, _mockDistributionService, _mockK8sSizingService);

        result.Should().Be(expectedResult);
        _mockK8sSizingService.Received(1).Calculate(Arg.Any<K8sSizingInput>());
    }

    [Fact]
    public void CalculateK8s_PassesCorrectInput()
    {
        K8sSizingInput? capturedInput = null;
        _mockK8sSizingService.Calculate(Arg.Do<K8sSizingInput>(x => capturedInput = x))
            .Returns(new K8sSizingResult
            {
                Environments = new List<EnvironmentResult>(),
                GrandTotal = new GrandTotal(),
                Configuration = new K8sSizingInput()
            });

        _sut.CalculateK8s(_mockState, _mockDistributionService, _mockK8sSizingService);

        capturedInput.Should().NotBeNull();
        capturedInput!.Distribution.Should().Be(Distribution.OpenShift);
        capturedInput.Technology.Should().Be(Technology.DotNet);
    }

    #endregion

    #region CalculateVM Tests

    [Fact]
    public void CalculateVM_ValidState_CallsSizingService()
    {
        var expectedResult = new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>(),
            GrandTotal = new VMGrandTotal()
        };
        _mockVmSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        var result = _sut.CalculateVM(_mockState, _mockVmSizingService);

        result.Should().Be(expectedResult);
        _mockVmSizingService.Received(1).Calculate(Arg.Any<VMSizingInput>());
    }

    [Fact]
    public void CalculateVM_PassesCorrectInput()
    {
        VMSizingInput? capturedInput = null;
        _mockVmSizingService.Calculate(Arg.Do<VMSizingInput>(x => capturedInput = x))
            .Returns(new VMSizingResult
            {
                Environments = new List<VMEnvironmentResult>(),
                GrandTotal = new VMGrandTotal()
            });

        _sut.CalculateVM(_mockState, _mockVmSizingService);

        capturedInput.Should().NotBeNull();
        capturedInput!.Technology.Should().Be(Technology.DotNet);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void BuildK8sInput_EmptyEnvironmentApps_UsesDefaults()
    {
        _mockState.EnvironmentApps.Returns(new Dictionary<EnvironmentType, AppConfig>());

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        // Should use default values from GetValueOrDefault
        result.ProdApps.Medium.Should().Be(70);
        result.NonProdApps.Medium.Should().Be(70);
    }

    [Fact]
    public void BuildK8sInput_EmptyHeadroom_UsesDefaults()
    {
        _mockState.Headroom.Returns(new Dictionary<EnvironmentType, double>());

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Headroom.Dev.Should().Be(33);
        result.Headroom.Test.Should().Be(33);
        result.Headroom.Stage.Should().Be(0);
        result.Headroom.Prod.Should().Be(37.5);
        result.Headroom.DR.Should().Be(37.5);
    }

    [Fact]
    public void BuildK8sInput_EmptyReplicas_UsesDefaults()
    {
        _mockState.Replicas.Returns(new Dictionary<EnvironmentType, int>());

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Replicas.NonProd.Should().Be(1);
        result.Replicas.Stage.Should().Be(2);
        result.Replicas.Prod.Should().Be(3);
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void BuildK8sInput_DifferentDistributions_SetsCorrectly(Distribution distribution)
    {
        _mockState.SelectedDistribution.Returns(distribution);
        _mockDistributionService.GetConfig(distribution).Returns(CreateDistributionConfig(
            distribution: distribution,
            name: distribution.ToString()));

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Distribution.Should().Be(distribution);
        result.CustomNodeSpecs!.Distribution.Should().Be(distribution);
    }

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    public void BuildK8sInput_DifferentTechnologies_SetsCorrectly(Technology technology)
    {
        _mockState.SelectedTechnology.Returns(technology);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.Technology.Should().Be(technology);
    }

    [Fact]
    public void BuildK8sInput_AllEnvironmentsEnabled_IncludesAll()
    {
        var allEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod, EnvironmentType.DR
        };
        _mockState.EnabledEnvironments.Returns(allEnvs);
        _mockState.SelectedClusterMode.Returns(ClusterMode.MultiCluster);

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.EnabledEnvironments.Should().HaveCount(5);
    }

    [Fact]
    public void BuildK8sInput_DrOnlyEnabled_WorksCorrectly()
    {
        _mockState.SelectedClusterMode.Returns(ClusterMode.SharedCluster);
        _mockState.SingleClusterScope.Returns("DR");

        var result = _sut.BuildK8sInput(_mockState, _mockDistributionService);

        result.EnabledEnvironments.Should().ContainSingle()
            .Which.Should().Be(EnvironmentType.DR);
        result.SelectedEnvironment.Should().Be(EnvironmentType.DR);
    }

    #endregion
}
