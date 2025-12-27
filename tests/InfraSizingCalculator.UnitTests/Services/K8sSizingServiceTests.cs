using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for K8sSizingService - K8s sizing calculation service
/// Focus on HA/DR-related methods: CalculateMasterNodes, CalculateEtcdNodes, ApplyAZMinimum, CalculateDRResources
/// </summary>
public class K8sSizingServiceTests
{
    private readonly IDistributionService _mockDistributionService;
    private readonly ITechnologyService _mockTechnologyService;
    private readonly K8sSizingService _service;

    public K8sSizingServiceTests()
    {
        _mockDistributionService = Substitute.For<IDistributionService>();
        _mockTechnologyService = Substitute.For<ITechnologyService>();

        // Set up default distribution config
        _mockDistributionService.GetConfig(Arg.Any<Distribution>())
            .Returns(CreateDefaultDistributionConfig());

        // Set up default technology config
        _mockTechnologyService.GetConfig(Arg.Any<Technology>())
            .Returns(CreateDefaultTechnologyConfig());

        _service = new K8sSizingService(
            _mockDistributionService,
            _mockTechnologyService);
    }

    #region CalculateMasterNodes Tests

    [Fact]
    public void CalculateMasterNodes_ManagedControlPlane_ReturnsZero()
    {
        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: true);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateMasterNodes_SelfManaged_SmallCluster_ReturnsThree()
    {
        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: false);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateMasterNodes_SelfManaged_LargeCluster_ReturnsFive()
    {
        // Large cluster = 100+ workers
        // Act
        var result = _service.CalculateMasterNodes(150, isManagedControlPlane: false);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void CalculateMasterNodes_WithHADRConfig_ManagedMode_NonManagedDist_ReturnsThree()
    {
        // Arrange - HADRConfig says Managed, but distribution doesn't support managed control planes
        // In this case, the distribution capability takes precedence (you can't have managed CP on OpenShift on-prem)
        var hadrConfig = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.Managed };

        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: false, hadrConfig);

        // Assert - Returns 3 (standard HA) because distribution doesn't support managed control planes
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateMasterNodes_WithHADRConfig_SingleMode_ReturnsOne()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.Single };

        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: false, hadrConfig);

        // Assert
        result.Should().Be(1);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void CalculateMasterNodes_WithHADRConfig_StackedHA_ReturnsConfiguredNodes(int nodes)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = nodes
        };

        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: false, hadrConfig);

        // Assert
        result.Should().Be(nodes);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void CalculateMasterNodes_WithHADRConfig_ExternalEtcd_ReturnsConfiguredNodes(int nodes)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = nodes
        };

        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: false, hadrConfig);

        // Assert
        result.Should().Be(nodes);
    }

    [Fact]
    public void CalculateMasterNodes_ManagedOverridesHADRConfig()
    {
        // Even if HADR config says StackedHA, managed control plane returns 0
        var hadrConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = 5
        };

        // Act
        var result = _service.CalculateMasterNodes(10, isManagedControlPlane: true, hadrConfig);

        // Assert - Managed takes precedence
        result.Should().Be(0);
    }

    #endregion

    #region CalculateEtcdNodes Tests

    [Fact]
    public void CalculateEtcdNodes_NullConfig_ReturnsZero()
    {
        // Act
        var result = _service.CalculateEtcdNodes(null);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(K8sControlPlaneHA.Managed)]
    [InlineData(K8sControlPlaneHA.Single)]
    [InlineData(K8sControlPlaneHA.StackedHA)]
    public void CalculateEtcdNodes_NonExternalEtcd_ReturnsZero(K8sControlPlaneHA mode)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { ControlPlaneHA = mode };

        // Act
        var result = _service.CalculateEtcdNodes(hadrConfig);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 3)]  // 1-3 control plane nodes = 3 etcd
    [InlineData(2, 3)]
    [InlineData(3, 3)]
    [InlineData(4, 5)]  // 4-5 control plane nodes = 5 etcd
    [InlineData(5, 5)]
    [InlineData(6, 7)]  // 6+ control plane nodes = 7 etcd
    [InlineData(7, 7)]
    public void CalculateEtcdNodes_ExternalEtcd_ReturnsQuorumNodes(int cpNodes, int expectedEtcd)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = cpNodes
        };

        // Act
        var result = _service.CalculateEtcdNodes(hadrConfig);

        // Assert
        result.Should().Be(expectedEtcd);
    }

    #endregion

    #region ApplyAZMinimum Tests

    [Fact]
    public void ApplyAZMinimum_NullConfig_ReturnsOriginalWorkers()
    {
        // Act
        var result = _service.ApplyAZMinimum(5, null);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void ApplyAZMinimum_SingleAZ_ReturnsOriginalWorkers()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            AvailabilityZones = 1
        };

        // Act
        var result = _service.ApplyAZMinimum(2, hadrConfig);

        // Assert
        result.Should().Be(2);
    }

    [Theory]
    [InlineData(K8sNodeDistribution.DualAZ, 2)]
    [InlineData(K8sNodeDistribution.MultiAZ, 3)]
    [InlineData(K8sNodeDistribution.MultiRegion, 3)]
    public void ApplyAZMinimum_MultiAZ_EnforcesMinimum(K8sNodeDistribution dist, int azCount)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            NodeDistribution = dist,
            AvailabilityZones = azCount
        };

        // Act - Only 1 worker requested, but should be minimum of AZ count
        var result = _service.ApplyAZMinimum(1, hadrConfig);

        // Assert
        result.Should().Be(azCount);
    }

    [Theory]
    [InlineData(5, 3, 5)]  // 5 workers > 3 AZs = keep 5
    [InlineData(10, 5, 10)]  // 10 workers > 5 AZs = keep 10
    public void ApplyAZMinimum_WorkersExceedAZ_ReturnsOriginal(int workers, int azCount, int expected)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = azCount
        };

        // Act
        var result = _service.ApplyAZMinimum(workers, hadrConfig);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(2, 5, 5)]  // 2 workers < 5 AZs = bump to 5
    [InlineData(1, 3, 3)]  // 1 worker < 3 AZs = bump to 3
    public void ApplyAZMinimum_WorkersLessThanAZ_BumpsToAZCount(int workers, int azCount, int expected)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = azCount
        };

        // Act
        var result = _service.ApplyAZMinimum(workers, hadrConfig);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region CalculateDRResources Tests

    [Fact]
    public void CalculateDRResources_NullConfig_ReturnsNoDR()
    {
        // Act
        var (nodes, multiplier) = _service.CalculateDRResources(10, null);

        // Assert
        nodes.Should().Be(0);
        multiplier.Should().Be(1.0);
    }

    [Fact]
    public void CalculateDRResources_DRPatternNone_ReturnsNoDR()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.None };

        // Act
        var (nodes, multiplier) = _service.CalculateDRResources(10, hadrConfig);

        // Assert
        nodes.Should().Be(0);
        multiplier.Should().Be(1.0);
    }

    [Fact]
    public void CalculateDRResources_BackupRestore_ReturnsStorageOnlyCost()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.BackupRestore };

        // Act
        var (nodes, multiplier) = _service.CalculateDRResources(10, hadrConfig);

        // Assert
        nodes.Should().Be(0);  // No standby infrastructure
        multiplier.Should().Be(1.08);  // ~8% for backup storage
    }

    [Fact]
    public void CalculateDRResources_WarmStandby_Returns30PercentCapacity()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.WarmStandby };

        // Act
        var (nodes, multiplier) = _service.CalculateDRResources(10, hadrConfig);

        // Assert
        nodes.Should().Be(3);  // 30% of 10, minimum 3 for HA quorum
        multiplier.Should().Be(1.40);
    }

    [Fact]
    public void CalculateDRResources_WarmStandby_MinimumThreeNodes()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.WarmStandby };

        // Act - Even 3 primary nodes should give 3 DR nodes (30% would be 0.9)
        var (nodes, _) = _service.CalculateDRResources(3, hadrConfig);

        // Assert - Minimum 3 nodes for HA quorum
        nodes.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void CalculateDRResources_HotStandby_Returns85PercentCapacity()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.HotStandby };

        // Act
        var (nodes, multiplier) = _service.CalculateDRResources(10, hadrConfig);

        // Assert
        nodes.Should().Be(8);  // 85% of 10 = 8.5 → 8
        multiplier.Should().Be(1.90);
    }

    [Fact]
    public void CalculateDRResources_HotStandby_LargeCluster()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.HotStandby };

        // Act
        var (nodes, _) = _service.CalculateDRResources(100, hadrConfig);

        // Assert
        nodes.Should().Be(85);  // 85% of 100
    }

    [Fact]
    public void CalculateDRResources_ActiveActive_ReturnsFullDuplication()
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = K8sDRPattern.ActiveActive };

        // Act
        var (nodes, multiplier) = _service.CalculateDRResources(10, hadrConfig);

        // Assert
        nodes.Should().Be(10);  // 100% duplication
        multiplier.Should().Be(2.10);  // Full infra + global LB overhead
    }

    [Theory]
    [InlineData(K8sDRPattern.BackupRestore, 1.08)]
    [InlineData(K8sDRPattern.WarmStandby, 1.40)]
    [InlineData(K8sDRPattern.HotStandby, 1.90)]
    [InlineData(K8sDRPattern.ActiveActive, 2.10)]
    public void CalculateDRResources_CostMultipliers_MatchBusinessRules(K8sDRPattern pattern, double expectedMultiplier)
    {
        // Arrange
        var hadrConfig = new K8sHADRConfig { DRPattern = pattern };

        // Act
        var (_, multiplier) = _service.CalculateDRResources(10, hadrConfig);

        // Assert
        multiplier.Should().Be(expectedMultiplier);
    }

    #endregion

    #region CalculateInfraNodes Tests

    [Fact]
    public void CalculateInfraNodes_NoInfraNodes_ReturnsZero()
    {
        // Act
        var result = _service.CalculateInfraNodes(100, isProd: true, hasInfraNodes: false);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateInfraNodes_SmallDeployment_ReturnsMinimum()
    {
        // Act
        var result = _service.CalculateInfraNodes(10, isProd: true, hasInfraNodes: true);

        // Assert
        result.Should().BeGreaterOrEqualTo(3); // Minimum infra nodes
    }

    [Fact]
    public void CalculateInfraNodes_LargeProduction_ReturnsMinimumFive()
    {
        // BR-I005: Large production (>=50 apps) needs minimum 5 infra
        // Act
        var result = _service.CalculateInfraNodes(50, isProd: true, hasInfraNodes: true);

        // Assert
        result.Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public void CalculateInfraNodes_ScalesWithApps()
    {
        // BR-I004: Scale at 1 per 25 apps
        // Act
        var result = _service.CalculateInfraNodes(75, isProd: true, hasInfraNodes: true);

        // Assert - 75 apps / 25 = 3, but large prod needs 5+
        result.Should().BeGreaterOrEqualTo(5);
    }

    #endregion

    #region Integration Tests - Full Calculate with HA/DR

    [Fact]
    public void Calculate_MultiCluster_WithHADRConfig_AppliesSettingsPerEnvironment()
    {
        // Arrange
        var input = CreateBasicInput();
        input.HADRConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = 5,
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 3,
            DRPattern = K8sDRPattern.WarmStandby
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        result.Environments.Should().NotBeEmpty();

        // Each prod environment should reflect HA/DR settings
        var prodEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Prod);
        if (prodEnv != null)
        {
            prodEnv.Masters.Should().Be(5); // StackedHA with 5 nodes
            prodEnv.Workers.Should().BeGreaterOrEqualTo(3); // Multi-AZ minimum
            prodEnv.DRNodes.Should().BeGreaterThan(0); // WarmStandby has DR nodes
        }
    }

    [Fact]
    public void Calculate_WithExternalEtcd_IncludesEtcdNodes()
    {
        // Arrange
        var input = CreateBasicInput();
        input.Distribution = Distribution.Kubernetes; // Self-managed
        input.HADRConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = 5
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();

        // Check that etcd nodes are calculated
        var prodEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Prod);
        if (prodEnv != null)
        {
            prodEnv.EtcdNodes.Should().Be(5); // External etcd with 5 nodes
        }
    }

    [Fact]
    public void Calculate_ManagedDistribution_IgnoresControlPlaneHASettings()
    {
        // Arrange
        var input = CreateBasicInput();
        input.Distribution = Distribution.EKS; // Managed control plane
        input.HADRConfig = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA, // This should be ignored
            ControlPlaneNodes = 5
        };

        _mockDistributionService.GetConfig(Distribution.EKS)
            .Returns(CreateManagedDistributionConfig());

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();

        // Masters should be 0 for managed distribution regardless of HADR config
        foreach (var env in result.Environments)
        {
            env.Masters.Should().Be(0);
        }
    }

    [Fact]
    public void Calculate_ActiveActiveDR_CalculatesFullDuplication()
    {
        // Arrange
        var input = CreateBasicInput();
        input.HADRConfig = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.ActiveActive
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();

        var prodEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Prod);
        if (prodEnv != null)
        {
            // DR nodes should equal primary nodes (full duplication)
            var primaryNodes = prodEnv.Masters + prodEnv.Infra + prodEnv.Workers;
            prodEnv.DRNodes.Should().Be(primaryNodes);
            prodEnv.DRCostMultiplier.Should().Be(2.10);
        }
    }

    #endregion

    #region Per-Environment HA/DR Config Tests

    [Fact]
    public void Calculate_WithEnvironmentSpecificHADR_UsesCorrectConfig()
    {
        // Arrange
        var input = CreateBasicInput();

        // Default config (for non-prod) - explicitly set SingleAZ with 1 zone
        input.HADRConfig = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            AvailabilityZones = 1,  // Explicitly set for SingleAZ
            DRPattern = K8sDRPattern.None
        };

        // Prod-specific config
        input.EnvironmentHADRConfigs = new Dictionary<EnvironmentType, K8sHADRConfig>
        {
            [EnvironmentType.Prod] = new K8sHADRConfig
            {
                NodeDistribution = K8sNodeDistribution.MultiAZ,
                AvailabilityZones = 3,
                DRPattern = K8sDRPattern.HotStandby
            }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();

        // Dev should use default config (no DR)
        var devEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Dev);
        if (devEnv != null)
        {
            devEnv.DRNodes.Should().Be(0);
            devEnv.AvailabilityZones.Should().Be(1);
        }

        // Prod should use prod-specific config
        var prodEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Prod);
        if (prodEnv != null)
        {
            prodEnv.DRNodes.Should().BeGreaterThan(0);
            prodEnv.AvailabilityZones.Should().Be(3);
        }
    }

    #endregion

    #region Helper Methods

    private static DistributionConfig CreateDefaultDistributionConfig()
    {
        return new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Test Distribution",
            Vendor = "Test Vendor",
            HasManagedControlPlane = false,
            HasInfraNodes = false,
            ProdControlPlane = new NodeSpecs(8, 32, 100),
            NonProdControlPlane = new NodeSpecs(4, 16, 50),
            ProdInfra = new NodeSpecs(8, 32, 200),
            NonProdInfra = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100)
        };
    }

    private static DistributionConfig CreateManagedDistributionConfig()
    {
        return new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "EKS",
            Vendor = "AWS",
            HasManagedControlPlane = true, // Key difference
            HasInfraNodes = false,
            ProdControlPlane = NodeSpecs.Zero, // Managed = 0
            NonProdControlPlane = NodeSpecs.Zero,
            ProdInfra = NodeSpecs.Zero,
            NonProdInfra = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50)
        };
    }

    private static TechnologyConfig CreateDefaultTechnologyConfig()
    {
        return new TechnologyConfig
        {
            Technology = Technology.Mendix,
            Name = "Test Technology",
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.25, 0.5),
                [AppTier.Medium] = new TierSpecs(0.5, 1),
                [AppTier.Large] = new TierSpecs(1, 2),
                [AppTier.XLarge] = new TierSpecs(2, 4)
            }
        };
    }

    private static K8sSizingInput CreateBasicInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            Technology = Technology.Mendix,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Small = 5, Medium = 3, Large = 2 },
            NonProdApps = new AppConfig { Small = 3, Medium = 2, Large = 1 },
            Replicas = new ReplicaSettings { Prod = 2, NonProd = 1, Stage = 1 },
            Headroom = new HeadroomSettings
            {
                Dev = 10,
                Test = 10,
                Stage = 15,
                Prod = 20,
                DR = 20
            },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 2.0, Memory = 1.5 }
        };
    }

    #endregion
}
