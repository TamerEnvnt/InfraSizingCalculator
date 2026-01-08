using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for NodeSpecsConfig model covering environment-specific specs and initialization.
/// </summary>
public class NodeSpecsConfigTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_ProdMasterSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.ProdMasterCpu.Should().Be(8);
        config.ProdMasterRam.Should().Be(32);
        config.ProdMasterDisk.Should().Be(200);
    }

    [Fact]
    public void DefaultValues_NonProdMasterSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.NonProdMasterCpu.Should().Be(8);
        config.NonProdMasterRam.Should().Be(32);
        config.NonProdMasterDisk.Should().Be(100);
    }

    [Fact]
    public void DefaultValues_ProdWorkerSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.ProdWorkerCpu.Should().Be(16);
        config.ProdWorkerRam.Should().Be(64);
        config.ProdWorkerDisk.Should().Be(200);
    }

    [Fact]
    public void DefaultValues_NonProdWorkerSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.NonProdWorkerCpu.Should().Be(8);
        config.NonProdWorkerRam.Should().Be(32);
        config.NonProdWorkerDisk.Should().Be(100);
    }

    [Fact]
    public void DefaultValues_ProdInfraSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.ProdInfraCpu.Should().Be(8);
        config.ProdInfraRam.Should().Be(32);
        config.ProdInfraDisk.Should().Be(500);
    }

    [Fact]
    public void DefaultValues_NonProdInfraSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.NonProdInfraCpu.Should().Be(8);
        config.NonProdInfraRam.Should().Be(32);
        config.NonProdInfraDisk.Should().Be(200);
    }

    [Fact]
    public void DefaultValues_DevSpecs_AreCorrect()
    {
        var config = new NodeSpecsConfig();
        config.DevMasterCpu.Should().Be(8);
        config.DevMasterRam.Should().Be(32);
        config.DevMasterDisk.Should().Be(100);
        config.DevWorkerCpu.Should().Be(8);
        config.DevWorkerRam.Should().Be(32);
        config.DevWorkerDisk.Should().Be(100);
    }

    [Fact]
    public void DefaultValues_DRSpecs_SimilarToProd()
    {
        var config = new NodeSpecsConfig();
        config.DRMasterCpu.Should().Be(8);
        config.DRMasterRam.Should().Be(32);
        config.DRMasterDisk.Should().Be(200);
        config.DRWorkerCpu.Should().Be(16);
        config.DRWorkerRam.Should().Be(64);
        config.DRWorkerDisk.Should().Be(200);
    }

    #endregion

    #region GetControlPlaneSpecs Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 8, 32, 100)]
    [InlineData(EnvironmentType.Test, 8, 32, 100)]
    [InlineData(EnvironmentType.Stage, 8, 32, 100)]
    [InlineData(EnvironmentType.Prod, 8, 32, 200)]
    [InlineData(EnvironmentType.DR, 8, 32, 200)]
    public void GetControlPlaneSpecs_ReturnsCorrectSpecs(EnvironmentType env, int expectedCpu, int expectedRam, int expectedDisk)
    {
        var config = new NodeSpecsConfig();
        var specs = config.GetControlPlaneSpecs(env);

        specs.Cpu.Should().Be(expectedCpu);
        specs.Ram.Should().Be(expectedRam);
        specs.Disk.Should().Be(expectedDisk);
    }

    [Fact]
    public void GetControlPlaneSpecs_WithUnknownEnvironment_ReturnsProductionDefaults()
    {
        var config = new NodeSpecsConfig();
        var specs = config.GetControlPlaneSpecs((EnvironmentType)999);

        specs.Cpu.Should().Be(config.ProdMasterCpu);
        specs.Ram.Should().Be(config.ProdMasterRam);
        specs.Disk.Should().Be(config.ProdMasterDisk);
    }

    [Fact]
    public void GetControlPlaneSpecs_WithCustomValues_ReturnsCustomValues()
    {
        var config = new NodeSpecsConfig
        {
            DevMasterCpu = 4,
            DevMasterRam = 16,
            DevMasterDisk = 50
        };

        var specs = config.GetControlPlaneSpecs(EnvironmentType.Dev);
        specs.Cpu.Should().Be(4);
        specs.Ram.Should().Be(16);
        specs.Disk.Should().Be(50);
    }

    #endregion

    #region GetWorkerSpecs Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 8, 32, 100)]
    [InlineData(EnvironmentType.Test, 8, 32, 100)]
    [InlineData(EnvironmentType.Stage, 8, 32, 100)]
    [InlineData(EnvironmentType.Prod, 16, 64, 200)]
    [InlineData(EnvironmentType.DR, 16, 64, 200)]
    public void GetWorkerSpecs_ReturnsCorrectSpecs(EnvironmentType env, int expectedCpu, int expectedRam, int expectedDisk)
    {
        var config = new NodeSpecsConfig();
        var specs = config.GetWorkerSpecs(env);

        specs.Cpu.Should().Be(expectedCpu);
        specs.Ram.Should().Be(expectedRam);
        specs.Disk.Should().Be(expectedDisk);
    }

    [Fact]
    public void GetWorkerSpecs_WithUnknownEnvironment_ReturnsProductionDefaults()
    {
        var config = new NodeSpecsConfig();
        var specs = config.GetWorkerSpecs((EnvironmentType)999);

        specs.Cpu.Should().Be(config.ProdWorkerCpu);
        specs.Ram.Should().Be(config.ProdWorkerRam);
        specs.Disk.Should().Be(config.ProdWorkerDisk);
    }

    #endregion

    #region GetInfraSpecs Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 8, 32, 200)]
    [InlineData(EnvironmentType.Test, 8, 32, 200)]
    [InlineData(EnvironmentType.Stage, 8, 32, 200)]
    [InlineData(EnvironmentType.Prod, 8, 32, 500)]
    [InlineData(EnvironmentType.DR, 8, 32, 500)]
    public void GetInfraSpecs_ReturnsCorrectSpecs(EnvironmentType env, int expectedCpu, int expectedRam, int expectedDisk)
    {
        var config = new NodeSpecsConfig();
        var specs = config.GetInfraSpecs(env);

        specs.Cpu.Should().Be(expectedCpu);
        specs.Ram.Should().Be(expectedRam);
        specs.Disk.Should().Be(expectedDisk);
    }

    [Fact]
    public void GetInfraSpecs_WithUnknownEnvironment_ReturnsProductionDefaults()
    {
        var config = new NodeSpecsConfig();
        var specs = config.GetInfraSpecs((EnvironmentType)999);

        specs.Cpu.Should().Be(config.ProdInfraCpu);
        specs.Ram.Should().Be(config.ProdInfraRam);
        specs.Disk.Should().Be(config.ProdInfraDisk);
    }

    #endregion

    #region InitializeFromDistro Tests

    [Fact]
    public void InitializeFromDistro_SetsAllProdNonProdSpecs()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistributionConfig();

        config.InitializeFromDistro(distro);

        // Verify Prod specs are set from distribution
        config.ProdMasterCpu.Should().Be(4);
        config.ProdMasterRam.Should().Be(16);
        config.ProdMasterDisk.Should().Be(150);
        config.ProdWorkerCpu.Should().Be(8);
        config.ProdWorkerRam.Should().Be(32);
        config.ProdWorkerDisk.Should().Be(100);
        config.ProdInfraCpu.Should().Be(4);
        config.ProdInfraRam.Should().Be(16);
        config.ProdInfraDisk.Should().Be(300);
    }

    [Fact]
    public void InitializeFromDistro_SetsAllNonProdSpecs()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistributionConfig();

        config.InitializeFromDistro(distro);

        config.NonProdMasterCpu.Should().Be(2);
        config.NonProdMasterRam.Should().Be(8);
        config.NonProdMasterDisk.Should().Be(50);
        config.NonProdWorkerCpu.Should().Be(4);
        config.NonProdWorkerRam.Should().Be(16);
        config.NonProdWorkerDisk.Should().Be(50);
    }

    [Fact]
    public void InitializeFromDistro_SetsDevTestStageFromNonProd()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistributionConfig();

        config.InitializeFromDistro(distro);

        // Dev, Test, Stage should use NonProd specs
        config.DevMasterCpu.Should().Be(2);
        config.TestMasterCpu.Should().Be(2);
        config.StageMasterCpu.Should().Be(2);

        config.DevWorkerCpu.Should().Be(4);
        config.TestWorkerCpu.Should().Be(4);
        config.StageWorkerCpu.Should().Be(4);
    }

    [Fact]
    public void InitializeFromDistro_SetsDRFromProd()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistributionConfig();

        config.InitializeFromDistro(distro);

        // DR should use Prod-like specs
        config.DRMasterCpu.Should().Be(4);
        config.DRMasterRam.Should().Be(16);
        config.DRMasterDisk.Should().Be(150);
        config.DRWorkerCpu.Should().Be(8);
        config.DRWorkerRam.Should().Be(32);
        config.DRWorkerDisk.Should().Be(100);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new NodeSpecsConfig
        {
            ProdMasterCpu = 12,
            ProdMasterRam = 48,
            ProdMasterDisk = 300,
            DevWorkerCpu = 6
        };

        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.ProdMasterCpu.Should().Be(12);
        clone.ProdMasterRam.Should().Be(48);
        clone.ProdMasterDisk.Should().Be(300);
        clone.DevWorkerCpu.Should().Be(6);
    }

    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new NodeSpecsConfig { ProdMasterCpu = 12 };
        var clone = original.Clone();

        clone.ProdMasterCpu = 24;

        original.ProdMasterCpu.Should().Be(12);
    }

    [Fact]
    public void Clone_ClonesAllProperties()
    {
        var original = new NodeSpecsConfig
        {
            ProdMasterCpu = 1, ProdMasterRam = 2, ProdMasterDisk = 3,
            NonProdMasterCpu = 4, NonProdMasterRam = 5, NonProdMasterDisk = 6,
            ProdInfraCpu = 7, ProdInfraRam = 8, ProdInfraDisk = 9,
            NonProdInfraCpu = 10, NonProdInfraRam = 11, NonProdInfraDisk = 12,
            ProdWorkerCpu = 13, ProdWorkerRam = 14, ProdWorkerDisk = 15,
            NonProdWorkerCpu = 16, NonProdWorkerRam = 17, NonProdWorkerDisk = 18,
            DevMasterCpu = 19, DevMasterRam = 20, DevMasterDisk = 21,
            DevInfraCpu = 22, DevInfraRam = 23, DevInfraDisk = 24,
            DevWorkerCpu = 25, DevWorkerRam = 26, DevWorkerDisk = 27,
            TestMasterCpu = 28, TestMasterRam = 29, TestMasterDisk = 30,
            TestInfraCpu = 31, TestInfraRam = 32, TestInfraDisk = 33,
            TestWorkerCpu = 34, TestWorkerRam = 35, TestWorkerDisk = 36,
            StageMasterCpu = 37, StageMasterRam = 38, StageMasterDisk = 39,
            StageInfraCpu = 40, StageInfraRam = 41, StageInfraDisk = 42,
            StageWorkerCpu = 43, StageWorkerRam = 44, StageWorkerDisk = 45,
            DRMasterCpu = 46, DRMasterRam = 47, DRMasterDisk = 48,
            DRInfraCpu = 49, DRInfraRam = 50, DRInfraDisk = 51,
            DRWorkerCpu = 52, DRWorkerRam = 53, DRWorkerDisk = 54
        };

        var clone = original.Clone();

        // Verify all 54 properties
        clone.ProdMasterCpu.Should().Be(1);
        clone.NonProdMasterDisk.Should().Be(6);
        clone.ProdInfraRam.Should().Be(8);
        clone.NonProdWorkerDisk.Should().Be(18);
        clone.DevWorkerDisk.Should().Be(27);
        clone.TestInfraDisk.Should().Be(33);
        clone.StageWorkerDisk.Should().Be(45);
        clone.DRWorkerDisk.Should().Be(54);
    }

    #endregion

    #region Helper Methods

    private static DistributionConfig CreateTestDistributionConfig()
    {
        return new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Test Distribution",
            Vendor = "Test",
            ProdControlPlane = new NodeSpecs(4, 16, 150),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            ProdInfra = new NodeSpecs(4, 16, 300),
            NonProdInfra = new NodeSpecs(2, 8, 100),
            HasInfraNodes = true
        };
    }

    #endregion
}
