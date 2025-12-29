using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for DistributionConfig model that manages distribution-specific
/// node specifications for K8s clusters.
/// </summary>
public class DistributionConfigTests
{
    #region GetControlPlaneForEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetControlPlaneForEnv_ReturnsProdSpecs_ForProdLikeEnvironments(EnvironmentType env)
    {
        var config = CreateTestConfig();

        var specs = config.GetControlPlaneForEnv(env);

        specs.Should().Be(config.ProdControlPlane);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void GetControlPlaneForEnv_ReturnsNonProdSpecs_ForNonProdEnvironments(EnvironmentType env)
    {
        var config = CreateTestConfig();

        var specs = config.GetControlPlaneForEnv(env);

        specs.Should().Be(config.NonProdControlPlane);
    }

    [Fact]
    public void GetControlPlaneForEnv_ReturnsPerEnvSpecs_WhenAvailable()
    {
        var customSpecs = new NodeSpecs(99, 99, 99);
        var config = CreateConfigWithPerEnvControlPlane(new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Dev] = customSpecs
        });

        var specs = config.GetControlPlaneForEnv(EnvironmentType.Dev);

        specs.Should().Be(customSpecs);
    }

    [Fact]
    public void GetControlPlaneForEnv_FallsBackToProdNonProd_WhenEnvNotInPerEnvDict()
    {
        var config = CreateConfigWithPerEnvControlPlane(new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Dev] = new NodeSpecs(99, 99, 99)
        });

        // Test should not be in the dictionary, so it falls back
        var specs = config.GetControlPlaneForEnv(EnvironmentType.Test);

        specs.Should().Be(config.NonProdControlPlane);
    }

    [Fact]
    public void GetControlPlaneForEnv_UsesNullCoalescence_WhenPerEnvIsNull()
    {
        var config = CreateTestConfig(); // PerEnvControlPlane is null by default

        // Should fall back to Prod/NonProd logic
        config.GetControlPlaneForEnv(EnvironmentType.Prod).Should().Be(config.ProdControlPlane);
        config.GetControlPlaneForEnv(EnvironmentType.Dev).Should().Be(config.NonProdControlPlane);
    }

    #endregion

    #region GetWorkerForEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetWorkerForEnv_ReturnsProdSpecs_ForProdLikeEnvironments(EnvironmentType env)
    {
        var config = CreateTestConfig();

        var specs = config.GetWorkerForEnv(env);

        specs.Should().Be(config.ProdWorker);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void GetWorkerForEnv_ReturnsNonProdSpecs_ForNonProdEnvironments(EnvironmentType env)
    {
        var config = CreateTestConfig();

        var specs = config.GetWorkerForEnv(env);

        specs.Should().Be(config.NonProdWorker);
    }

    [Fact]
    public void GetWorkerForEnv_ReturnsPerEnvSpecs_WhenAvailable()
    {
        var customSpecs = new NodeSpecs(88, 88, 88);
        var config = CreateConfigWithPerEnvWorker(new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Stage] = customSpecs
        });

        var specs = config.GetWorkerForEnv(EnvironmentType.Stage);

        specs.Should().Be(customSpecs);
    }

    [Fact]
    public void GetWorkerForEnv_FallsBackToProdNonProd_WhenEnvNotInPerEnvDict()
    {
        var config = CreateConfigWithPerEnvWorker(new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Stage] = new NodeSpecs(88, 88, 88)
        });

        // Prod should not be in the dictionary, so it falls back
        var specs = config.GetWorkerForEnv(EnvironmentType.Prod);

        specs.Should().Be(config.ProdWorker);
    }

    [Fact]
    public void GetWorkerForEnv_UsesNullCoalescence_WhenPerEnvIsNull()
    {
        var config = CreateTestConfig();

        config.GetWorkerForEnv(EnvironmentType.DR).Should().Be(config.ProdWorker);
        config.GetWorkerForEnv(EnvironmentType.Test).Should().Be(config.NonProdWorker);
    }

    #endregion

    #region GetInfraForEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetInfraForEnv_ReturnsProdSpecs_ForProdLikeEnvironments(EnvironmentType env)
    {
        var config = CreateTestConfig();

        var specs = config.GetInfraForEnv(env);

        specs.Should().Be(config.ProdInfra);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void GetInfraForEnv_ReturnsNonProdSpecs_ForNonProdEnvironments(EnvironmentType env)
    {
        var config = CreateTestConfig();

        var specs = config.GetInfraForEnv(env);

        specs.Should().Be(config.NonProdInfra);
    }

    [Fact]
    public void GetInfraForEnv_ReturnsPerEnvSpecs_WhenAvailable()
    {
        var customSpecs = new NodeSpecs(77, 77, 77);
        var config = CreateConfigWithPerEnvInfra(new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Test] = customSpecs
        });

        var specs = config.GetInfraForEnv(EnvironmentType.Test);

        specs.Should().Be(customSpecs);
    }

    [Fact]
    public void GetInfraForEnv_FallsBackToProdNonProd_WhenEnvNotInPerEnvDict()
    {
        var config = CreateConfigWithPerEnvInfra(new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Test] = new NodeSpecs(77, 77, 77)
        });

        var specs = config.GetInfraForEnv(EnvironmentType.Dev);

        specs.Should().Be(config.NonProdInfra);
    }

    [Fact]
    public void GetInfraForEnv_UsesNullCoalescence_WhenPerEnvIsNull()
    {
        var config = CreateTestConfig();

        config.GetInfraForEnv(EnvironmentType.Prod).Should().Be(config.ProdInfra);
        config.GetInfraForEnv(EnvironmentType.Stage).Should().Be(config.NonProdInfra);
    }

    [Fact]
    public void GetInfraForEnv_ReturnsZeroSpecs_WhenNoInfraNodes()
    {
        var config = new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "Amazon EKS",
            Vendor = "Amazon",
            ProdControlPlane = new NodeSpecs(0, 0, 0), // Managed
            NonProdControlPlane = new NodeSpecs(0, 0, 0),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            HasInfraNodes = false,
            HasManagedControlPlane = true
            // ProdInfra and NonProdInfra default to NodeSpecs.Zero
        };

        config.GetInfraForEnv(EnvironmentType.Prod).Should().Be(NodeSpecs.Zero);
        config.GetInfraForEnv(EnvironmentType.Dev).Should().Be(NodeSpecs.Zero);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void DefaultValues_AreApplied()
    {
        var config = new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Kubernetes",
            Vendor = "CNCF",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100)
        };

        // Check defaults
        config.ProdInfra.Should().Be(NodeSpecs.Zero);
        config.NonProdInfra.Should().Be(NodeSpecs.Zero);
        config.HasInfraNodes.Should().BeFalse();
        config.HasManagedControlPlane.Should().BeFalse();
        config.Tags.Should().BeEmpty();
        config.Icon.Should().BeEmpty();
        config.BrandColor.Should().Be("#326CE5");
    }

    [Fact]
    public void RequiredProperties_MustBeSet()
    {
        var config = CreateTestConfig();

        config.Distribution.Should().Be(Distribution.OpenShift);
        config.Name.Should().NotBeNullOrEmpty();
        config.Vendor.Should().NotBeNullOrEmpty();
        config.ProdControlPlane.Should().NotBeNull();
        config.NonProdControlPlane.Should().NotBeNull();
        config.ProdWorker.Should().NotBeNull();
        config.NonProdWorker.Should().NotBeNull();
    }

    [Fact]
    public void Tags_CanBeSet()
    {
        var config = CreateConfigWithTags(new[] { "enterprise", "on-prem", "openshift" });

        config.Tags.Should().HaveCount(3);
        config.Tags.Should().Contain("enterprise");
    }

    [Fact]
    public void Icon_CanBeSet()
    {
        var config = CreateConfigWithIcon("fa-openshift");

        config.Icon.Should().Be("fa-openshift");
    }

    [Fact]
    public void BrandColor_CanBeSet()
    {
        var config = CreateConfigWithBrandColor("#EE0000");

        config.BrandColor.Should().Be("#EE0000");
    }

    #endregion

    #region OpenShift Specific Tests (Has Infra Nodes)

    [Fact]
    public void OpenShiftConfig_HasInfraNodes()
    {
        var config = CreateTestConfig();

        config.HasInfraNodes.Should().BeTrue();
        config.ProdInfra.Should().NotBe(NodeSpecs.Zero);
        config.NonProdInfra.Should().NotBe(NodeSpecs.Zero);
    }

    #endregion

    #region Managed K8s Tests

    [Fact]
    public void ManagedK8s_HasManagedControlPlane()
    {
        var config = new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "Amazon EKS",
            Vendor = "Amazon",
            ProdControlPlane = NodeSpecs.Zero, // Managed by cloud provider
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            HasManagedControlPlane = true
        };

        config.HasManagedControlPlane.Should().BeTrue();
        config.ProdControlPlane.Should().Be(NodeSpecs.Zero);
    }

    #endregion

    #region All Environments Coverage Tests

    [Fact]
    public void AllEnvironments_CanRetrieveSpecs()
    {
        var config = CreateTestConfig();
        var allEnvs = new[]
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Stage,
            EnvironmentType.Prod,
            EnvironmentType.DR
        };

        foreach (var env in allEnvs)
        {
            var control = config.GetControlPlaneForEnv(env);
            var worker = config.GetWorkerForEnv(env);
            var infra = config.GetInfraForEnv(env);

            control.Should().NotBeNull();
            worker.Should().NotBeNull();
            infra.Should().NotBeNull();
        }
    }

    [Fact]
    public void PerEnvDictionaries_CanOverrideAllEnvironments()
    {
        var customControlPlane = new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Dev] = new NodeSpecs(1, 1, 1),
            [EnvironmentType.Test] = new NodeSpecs(2, 2, 2),
            [EnvironmentType.Stage] = new NodeSpecs(3, 3, 3),
            [EnvironmentType.Prod] = new NodeSpecs(4, 4, 4),
            [EnvironmentType.DR] = new NodeSpecs(5, 5, 5)
        };

        var config = CreateConfigWithPerEnvControlPlane(customControlPlane);

        config.GetControlPlaneForEnv(EnvironmentType.Dev).Cpu.Should().Be(1);
        config.GetControlPlaneForEnv(EnvironmentType.Test).Cpu.Should().Be(2);
        config.GetControlPlaneForEnv(EnvironmentType.Stage).Cpu.Should().Be(3);
        config.GetControlPlaneForEnv(EnvironmentType.Prod).Cpu.Should().Be(4);
        config.GetControlPlaneForEnv(EnvironmentType.DR).Cpu.Should().Be(5);
    }

    #endregion

    #region Helper Methods

    private static DistributionConfig CreateTestConfig()
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            Tags = new[] { "enterprise", "on-prem" },
            Icon = "fab fa-redhat",
            BrandColor = "#EE0000"
        };
    }

    private static DistributionConfig CreateConfigWithPerEnvControlPlane(Dictionary<EnvironmentType, NodeSpecs> perEnv)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            PerEnvControlPlane = perEnv
        };
    }

    private static DistributionConfig CreateConfigWithPerEnvWorker(Dictionary<EnvironmentType, NodeSpecs> perEnv)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            PerEnvWorker = perEnv
        };
    }

    private static DistributionConfig CreateConfigWithPerEnvInfra(Dictionary<EnvironmentType, NodeSpecs> perEnv)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            PerEnvInfra = perEnv
        };
    }

    private static DistributionConfig CreateConfigWithTags(string[] tags)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            Tags = tags
        };
    }

    private static DistributionConfig CreateConfigWithIcon(string icon)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            Icon = icon
        };
    }

    private static DistributionConfig CreateConfigWithBrandColor(string color)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Red Hat OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            BrandColor = color
        };
    }

    #endregion
}
