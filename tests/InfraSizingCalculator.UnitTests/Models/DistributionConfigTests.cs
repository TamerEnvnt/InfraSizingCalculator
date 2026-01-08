using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for DistributionConfig model covering environment-specific node specifications.
/// </summary>
public class DistributionConfigTests
{
    #region GetControlPlaneForEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetControlPlaneForEnv_ProdLikeEnvironment_ReturnsProdSpecs(EnvironmentType env)
    {
        var config = CreateTestConfig();
        var specs = config.GetControlPlaneForEnv(env);

        specs.Should().Be(config.ProdControlPlane);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void GetControlPlaneForEnv_NonProdEnvironment_ReturnsNonProdSpecs(EnvironmentType env)
    {
        var config = CreateTestConfig();
        var specs = config.GetControlPlaneForEnv(env);

        specs.Should().Be(config.NonProdControlPlane);
    }

    [Fact]
    public void GetControlPlaneForEnv_WithPerEnvOverride_ReturnsOverride()
    {
        var overrideSpecs = new NodeSpecs(16, 64, 500);
        var config = CreateTestConfigWithPerEnvControlPlane(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Stage, overrideSpecs }
        });

        var specs = config.GetControlPlaneForEnv(EnvironmentType.Stage);

        specs.Should().Be(overrideSpecs);
    }

    [Fact]
    public void GetControlPlaneForEnv_WithPerEnvOverrideForOtherEnv_ReturnsFallback()
    {
        var overrideSpecs = new NodeSpecs(16, 64, 500);
        var config = CreateTestConfigWithPerEnvControlPlane(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Stage, overrideSpecs }
        });

        // Dev is not in the override dictionary, should fall back to NonProd
        var specs = config.GetControlPlaneForEnv(EnvironmentType.Dev);

        specs.Should().Be(config.NonProdControlPlane);
    }

    #endregion

    #region GetWorkerForEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetWorkerForEnv_ProdLikeEnvironment_ReturnsProdSpecs(EnvironmentType env)
    {
        var config = CreateTestConfig();
        var specs = config.GetWorkerForEnv(env);

        specs.Should().Be(config.ProdWorker);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void GetWorkerForEnv_NonProdEnvironment_ReturnsNonProdSpecs(EnvironmentType env)
    {
        var config = CreateTestConfig();
        var specs = config.GetWorkerForEnv(env);

        specs.Should().Be(config.NonProdWorker);
    }

    [Fact]
    public void GetWorkerForEnv_WithPerEnvOverride_ReturnsOverride()
    {
        var overrideSpecs = new NodeSpecs(32, 128, 1000);
        var config = CreateTestConfigWithPerEnvWorker(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Prod, overrideSpecs }
        });

        var specs = config.GetWorkerForEnv(EnvironmentType.Prod);

        specs.Should().Be(overrideSpecs);
    }

    [Fact]
    public void GetWorkerForEnv_WithPerEnvOverrideForOtherEnv_ReturnsFallback()
    {
        var overrideSpecs = new NodeSpecs(32, 128, 1000);
        var config = CreateTestConfigWithPerEnvWorker(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Prod, overrideSpecs }
        });

        // DR should fall back to ProdWorker since override is only for Prod
        var specs = config.GetWorkerForEnv(EnvironmentType.DR);

        specs.Should().Be(config.ProdWorker);
    }

    #endregion

    #region GetInfraForEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetInfraForEnv_ProdLikeEnvironment_ReturnsProdSpecs(EnvironmentType env)
    {
        var config = CreateTestConfig();
        var specs = config.GetInfraForEnv(env);

        specs.Should().Be(config.ProdInfra);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    public void GetInfraForEnv_NonProdEnvironment_ReturnsNonProdSpecs(EnvironmentType env)
    {
        var config = CreateTestConfig();
        var specs = config.GetInfraForEnv(env);

        specs.Should().Be(config.NonProdInfra);
    }

    [Fact]
    public void GetInfraForEnv_WithPerEnvOverride_ReturnsOverride()
    {
        var overrideSpecs = new NodeSpecs(8, 32, 500);
        var config = CreateTestConfigWithPerEnvInfra(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Test, overrideSpecs }
        });

        var specs = config.GetInfraForEnv(EnvironmentType.Test);

        specs.Should().Be(overrideSpecs);
    }

    [Fact]
    public void GetInfraForEnv_WithPerEnvOverrideForOtherEnv_ReturnsFallback()
    {
        var overrideSpecs = new NodeSpecs(8, 32, 500);
        var config = CreateTestConfigWithPerEnvInfra(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Test, overrideSpecs }
        });

        var specs = config.GetInfraForEnv(EnvironmentType.Dev);

        specs.Should().Be(config.NonProdInfra);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Distribution_IsRequired()
    {
        var config = CreateTestConfig();
        config.Distribution.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public void Name_IsRequired()
    {
        var config = CreateTestConfig();
        config.Name.Should().Be("OpenShift Container Platform");
    }

    [Fact]
    public void Vendor_IsRequired()
    {
        var config = CreateTestConfig();
        config.Vendor.Should().Be("Red Hat");
    }

    [Fact]
    public void HasInfraNodes_CanBeSetAndRetrieved()
    {
        var config = CreateTestConfig();
        config.HasInfraNodes.Should().BeTrue();
    }

    [Fact]
    public void HasManagedControlPlane_CanBeSetAndRetrieved()
    {
        var config = CreateTestConfigWithManagedControlPlane(true);
        config.HasManagedControlPlane.Should().BeTrue();
    }

    [Fact]
    public void Tags_DefaultsToEmptyArray()
    {
        var config = CreateTestConfig();
        config.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Tags_CanBeSetAndRetrieved()
    {
        var config = CreateTestConfigWithTags(new[] { "enterprise", "on-prem" });
        config.Tags.Should().BeEquivalentTo(new[] { "enterprise", "on-prem" });
    }

    [Fact]
    public void Icon_DefaultsToEmptyString()
    {
        var config = CreateTestConfig();
        config.Icon.Should().BeEmpty();
    }

    [Fact]
    public void BrandColor_DefaultsToKubernetesBlue()
    {
        var config = CreateTestConfig();
        config.BrandColor.Should().Be("#326CE5");
    }

    [Fact]
    public void BrandColor_CanBeOverridden()
    {
        var config = CreateTestConfigWithBrandColor("#EE0000");
        config.BrandColor.Should().Be("#EE0000");
    }

    #endregion

    #region Default NodeSpecs Tests

    [Fact]
    public void ProdInfra_DefaultsToZero()
    {
        var config = new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Test",
            Vendor = "Test",
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50)
            // ProdInfra not set - should default to Zero
        };

        config.ProdInfra.Should().Be(NodeSpecs.Zero);
    }

    [Fact]
    public void NonProdInfra_DefaultsToZero()
    {
        var config = new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Test",
            Vendor = "Test",
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50)
            // NonProdInfra not set - should default to Zero
        };

        config.NonProdInfra.Should().Be(NodeSpecs.Zero);
    }

    #endregion

    #region Per-Environment Override Edge Cases

    [Fact]
    public void GetControlPlaneForEnv_WithEmptyOverrideDictionary_ReturnsFallback()
    {
        var config = CreateTestConfigWithPerEnvControlPlane(new Dictionary<EnvironmentType, NodeSpecs>());

        var specs = config.GetControlPlaneForEnv(EnvironmentType.Prod);

        specs.Should().Be(config.ProdControlPlane);
    }

    [Fact]
    public void GetWorkerForEnv_WithNullOverrideDictionary_ReturnsFallback()
    {
        var config = CreateTestConfigWithPerEnvWorker(null);

        var specs = config.GetWorkerForEnv(EnvironmentType.Dev);

        specs.Should().Be(config.NonProdWorker);
    }

    [Fact]
    public void GetInfraForEnv_AllEnvironmentsOverridden_ReturnsCorrectOverride()
    {
        var devSpecs = new NodeSpecs(2, 8, 100);
        var testSpecs = new NodeSpecs(4, 16, 200);
        var stageSpecs = new NodeSpecs(6, 24, 300);
        var prodSpecs = new NodeSpecs(8, 32, 400);
        var drSpecs = new NodeSpecs(8, 32, 400);

        var config = CreateTestConfigWithPerEnvInfra(new Dictionary<EnvironmentType, NodeSpecs>
        {
            { EnvironmentType.Dev, devSpecs },
            { EnvironmentType.Test, testSpecs },
            { EnvironmentType.Stage, stageSpecs },
            { EnvironmentType.Prod, prodSpecs },
            { EnvironmentType.DR, drSpecs }
        });

        config.GetInfraForEnv(EnvironmentType.Dev).Should().Be(devSpecs);
        config.GetInfraForEnv(EnvironmentType.Test).Should().Be(testSpecs);
        config.GetInfraForEnv(EnvironmentType.Stage).Should().Be(stageSpecs);
        config.GetInfraForEnv(EnvironmentType.Prod).Should().Be(prodSpecs);
        config.GetInfraForEnv(EnvironmentType.DR).Should().Be(drSpecs);
    }

    #endregion

    #region Managed Control Plane Configurations

    [Fact]
    public void ManagedControlPlaneDistribution_HasCorrectSettings()
    {
        var eksConfig = new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "Amazon EKS",
            Vendor = "Amazon Web Services",
            HasManagedControlPlane = true,
            HasInfraNodes = false,
            ProdControlPlane = NodeSpecs.Zero, // Managed by AWS
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50)
        };

        eksConfig.HasManagedControlPlane.Should().BeTrue();
        eksConfig.ProdControlPlane.Should().Be(NodeSpecs.Zero);
    }

    #endregion

    #region Helper Methods

    private static DistributionConfig CreateTestConfig()
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false
        };
    }

    private static DistributionConfig CreateTestConfigWithPerEnvControlPlane(Dictionary<EnvironmentType, NodeSpecs>? perEnv)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            PerEnvControlPlane = perEnv
        };
    }

    private static DistributionConfig CreateTestConfigWithPerEnvWorker(Dictionary<EnvironmentType, NodeSpecs>? perEnv)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            PerEnvWorker = perEnv
        };
    }

    private static DistributionConfig CreateTestConfigWithPerEnvInfra(Dictionary<EnvironmentType, NodeSpecs>? perEnv)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            PerEnvInfra = perEnv
        };
    }

    private static DistributionConfig CreateTestConfigWithManagedControlPlane(bool hasManaged)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = hasManaged
        };
    }

    private static DistributionConfig CreateTestConfigWithTags(string[] tags)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            Tags = tags
        };
    }

    private static DistributionConfig CreateTestConfigWithBrandColor(string color)
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift Container Platform",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(4, 16, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(4, 16, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            BrandColor = color
        };
    }

    #endregion
}
