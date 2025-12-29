using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for NodeSpecsConfig model that manages node specifications
/// for K8s clusters across different environments.
/// </summary>
public class NodeSpecsConfigTests
{
    #region Default Value Tests

    [Fact]
    public void Constructor_SetsProdMasterDefaults()
    {
        var config = new NodeSpecsConfig();

        config.ProdMasterCpu.Should().Be(8);
        config.ProdMasterRam.Should().Be(32);
        config.ProdMasterDisk.Should().Be(200);
    }

    [Fact]
    public void Constructor_SetsNonProdMasterDefaults()
    {
        var config = new NodeSpecsConfig();

        config.NonProdMasterCpu.Should().Be(8);
        config.NonProdMasterRam.Should().Be(32);
        config.NonProdMasterDisk.Should().Be(100);
    }

    [Fact]
    public void Constructor_SetsProdInfraDefaults()
    {
        var config = new NodeSpecsConfig();

        config.ProdInfraCpu.Should().Be(8);
        config.ProdInfraRam.Should().Be(32);
        config.ProdInfraDisk.Should().Be(500);
    }

    [Fact]
    public void Constructor_SetsNonProdInfraDefaults()
    {
        var config = new NodeSpecsConfig();

        config.NonProdInfraCpu.Should().Be(8);
        config.NonProdInfraRam.Should().Be(32);
        config.NonProdInfraDisk.Should().Be(200);
    }

    [Fact]
    public void Constructor_SetsProdWorkerDefaults()
    {
        var config = new NodeSpecsConfig();

        config.ProdWorkerCpu.Should().Be(16);
        config.ProdWorkerRam.Should().Be(64);
        config.ProdWorkerDisk.Should().Be(200);
    }

    [Fact]
    public void Constructor_SetsNonProdWorkerDefaults()
    {
        var config = new NodeSpecsConfig();

        config.NonProdWorkerCpu.Should().Be(8);
        config.NonProdWorkerRam.Should().Be(32);
        config.NonProdWorkerDisk.Should().Be(100);
    }

    [Fact]
    public void Constructor_SetsDevEnvironmentDefaults()
    {
        var config = new NodeSpecsConfig();

        config.DevMasterCpu.Should().Be(8);
        config.DevMasterRam.Should().Be(32);
        config.DevMasterDisk.Should().Be(100);
        config.DevInfraCpu.Should().Be(8);
        config.DevInfraRam.Should().Be(32);
        config.DevInfraDisk.Should().Be(200);
        config.DevWorkerCpu.Should().Be(8);
        config.DevWorkerRam.Should().Be(32);
        config.DevWorkerDisk.Should().Be(100);
    }

    [Fact]
    public void Constructor_SetsTestEnvironmentDefaults()
    {
        var config = new NodeSpecsConfig();

        config.TestMasterCpu.Should().Be(8);
        config.TestMasterRam.Should().Be(32);
        config.TestMasterDisk.Should().Be(100);
        config.TestInfraCpu.Should().Be(8);
        config.TestInfraRam.Should().Be(32);
        config.TestInfraDisk.Should().Be(200);
        config.TestWorkerCpu.Should().Be(8);
        config.TestWorkerRam.Should().Be(32);
        config.TestWorkerDisk.Should().Be(100);
    }

    [Fact]
    public void Constructor_SetsStageEnvironmentDefaults()
    {
        var config = new NodeSpecsConfig();

        config.StageMasterCpu.Should().Be(8);
        config.StageMasterRam.Should().Be(32);
        config.StageMasterDisk.Should().Be(100);
        config.StageInfraCpu.Should().Be(8);
        config.StageInfraRam.Should().Be(32);
        config.StageInfraDisk.Should().Be(200);
        config.StageWorkerCpu.Should().Be(8);
        config.StageWorkerRam.Should().Be(32);
        config.StageWorkerDisk.Should().Be(100);
    }

    [Fact]
    public void Constructor_SetsDREnvironmentDefaults()
    {
        var config = new NodeSpecsConfig();

        // DR uses Prod-like specs
        config.DRMasterCpu.Should().Be(8);
        config.DRMasterRam.Should().Be(32);
        config.DRMasterDisk.Should().Be(200);
        config.DRInfraCpu.Should().Be(8);
        config.DRInfraRam.Should().Be(32);
        config.DRInfraDisk.Should().Be(500);
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
    public void GetControlPlaneSpecs_ReturnsCorrectSpecs_ForEachEnvironment(
        EnvironmentType env, int expectedCpu, int expectedRam, int expectedDisk)
    {
        var config = new NodeSpecsConfig();

        var specs = config.GetControlPlaneSpecs(env);

        specs.Cpu.Should().Be(expectedCpu);
        specs.Ram.Should().Be(expectedRam);
        specs.Disk.Should().Be(expectedDisk);
    }

    [Fact]
    public void GetControlPlaneSpecs_ReturnsCustomValues_WhenModified()
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

    [Fact]
    public void GetControlPlaneSpecs_ReturnsProdDefaults_ForUnknownEnvironment()
    {
        var config = new NodeSpecsConfig();

        // Cast an invalid value to trigger the default case
        var specs = config.GetControlPlaneSpecs((EnvironmentType)999);

        // Should return Prod defaults
        specs.Cpu.Should().Be(config.ProdMasterCpu);
        specs.Ram.Should().Be(config.ProdMasterRam);
        specs.Disk.Should().Be(config.ProdMasterDisk);
    }

    #endregion

    #region GetInfraSpecs Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 8, 32, 200)]
    [InlineData(EnvironmentType.Test, 8, 32, 200)]
    [InlineData(EnvironmentType.Stage, 8, 32, 200)]
    [InlineData(EnvironmentType.Prod, 8, 32, 500)]
    [InlineData(EnvironmentType.DR, 8, 32, 500)]
    public void GetInfraSpecs_ReturnsCorrectSpecs_ForEachEnvironment(
        EnvironmentType env, int expectedCpu, int expectedRam, int expectedDisk)
    {
        var config = new NodeSpecsConfig();

        var specs = config.GetInfraSpecs(env);

        specs.Cpu.Should().Be(expectedCpu);
        specs.Ram.Should().Be(expectedRam);
        specs.Disk.Should().Be(expectedDisk);
    }

    [Fact]
    public void GetInfraSpecs_ReturnsCustomValues_WhenModified()
    {
        var config = new NodeSpecsConfig
        {
            ProdInfraCpu = 16,
            ProdInfraRam = 64,
            ProdInfraDisk = 1000
        };

        var specs = config.GetInfraSpecs(EnvironmentType.Prod);

        specs.Cpu.Should().Be(16);
        specs.Ram.Should().Be(64);
        specs.Disk.Should().Be(1000);
    }

    [Fact]
    public void GetInfraSpecs_ReturnsProdDefaults_ForUnknownEnvironment()
    {
        var config = new NodeSpecsConfig();

        var specs = config.GetInfraSpecs((EnvironmentType)999);

        specs.Cpu.Should().Be(config.ProdInfraCpu);
        specs.Ram.Should().Be(config.ProdInfraRam);
        specs.Disk.Should().Be(config.ProdInfraDisk);
    }

    #endregion

    #region GetWorkerSpecs Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 8, 32, 100)]
    [InlineData(EnvironmentType.Test, 8, 32, 100)]
    [InlineData(EnvironmentType.Stage, 8, 32, 100)]
    [InlineData(EnvironmentType.Prod, 16, 64, 200)]
    [InlineData(EnvironmentType.DR, 16, 64, 200)]
    public void GetWorkerSpecs_ReturnsCorrectSpecs_ForEachEnvironment(
        EnvironmentType env, int expectedCpu, int expectedRam, int expectedDisk)
    {
        var config = new NodeSpecsConfig();

        var specs = config.GetWorkerSpecs(env);

        specs.Cpu.Should().Be(expectedCpu);
        specs.Ram.Should().Be(expectedRam);
        specs.Disk.Should().Be(expectedDisk);
    }

    [Fact]
    public void GetWorkerSpecs_ReturnsCustomValues_WhenModified()
    {
        var config = new NodeSpecsConfig
        {
            TestWorkerCpu = 12,
            TestWorkerRam = 48,
            TestWorkerDisk = 150
        };

        var specs = config.GetWorkerSpecs(EnvironmentType.Test);

        specs.Cpu.Should().Be(12);
        specs.Ram.Should().Be(48);
        specs.Disk.Should().Be(150);
    }

    [Fact]
    public void GetWorkerSpecs_ReturnsProdDefaults_ForUnknownEnvironment()
    {
        var config = new NodeSpecsConfig();

        var specs = config.GetWorkerSpecs((EnvironmentType)999);

        specs.Cpu.Should().Be(config.ProdWorkerCpu);
        specs.Ram.Should().Be(config.ProdWorkerRam);
        specs.Disk.Should().Be(config.ProdWorkerDisk);
    }

    #endregion

    #region InitializeFromDistro Tests

    [Fact]
    public void InitializeFromDistro_SetsAllProdSpecs()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistroConfig();

        config.InitializeFromDistro(distro);

        // Prod control plane
        config.ProdMasterCpu.Should().Be(distro.ProdControlPlane.Cpu);
        config.ProdMasterRam.Should().Be(distro.ProdControlPlane.Ram);
        config.ProdMasterDisk.Should().Be(distro.ProdControlPlane.Disk);

        // Prod infra
        config.ProdInfraCpu.Should().Be(distro.ProdInfra.Cpu);
        config.ProdInfraRam.Should().Be(distro.ProdInfra.Ram);
        config.ProdInfraDisk.Should().Be(distro.ProdInfra.Disk);

        // Prod worker
        config.ProdWorkerCpu.Should().Be(distro.ProdWorker.Cpu);
        config.ProdWorkerRam.Should().Be(distro.ProdWorker.Ram);
        config.ProdWorkerDisk.Should().Be(distro.ProdWorker.Disk);
    }

    [Fact]
    public void InitializeFromDistro_SetsAllNonProdSpecs()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistroConfig();

        config.InitializeFromDistro(distro);

        config.NonProdMasterCpu.Should().Be(distro.NonProdControlPlane.Cpu);
        config.NonProdMasterRam.Should().Be(distro.NonProdControlPlane.Ram);
        config.NonProdMasterDisk.Should().Be(distro.NonProdControlPlane.Disk);

        config.NonProdInfraCpu.Should().Be(distro.NonProdInfra.Cpu);
        config.NonProdInfraRam.Should().Be(distro.NonProdInfra.Ram);
        config.NonProdInfraDisk.Should().Be(distro.NonProdInfra.Disk);

        config.NonProdWorkerCpu.Should().Be(distro.NonProdWorker.Cpu);
        config.NonProdWorkerRam.Should().Be(distro.NonProdWorker.Ram);
        config.NonProdWorkerDisk.Should().Be(distro.NonProdWorker.Disk);
    }

    [Fact]
    public void InitializeFromDistro_SetsDevEnvironmentFromNonProd()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistroConfig();

        config.InitializeFromDistro(distro);

        config.DevMasterCpu.Should().Be(distro.NonProdControlPlane.Cpu);
        config.DevMasterRam.Should().Be(distro.NonProdControlPlane.Ram);
        config.DevMasterDisk.Should().Be(distro.NonProdControlPlane.Disk);
        config.DevInfraCpu.Should().Be(distro.NonProdInfra.Cpu);
        config.DevInfraRam.Should().Be(distro.NonProdInfra.Ram);
        config.DevInfraDisk.Should().Be(distro.NonProdInfra.Disk);
        config.DevWorkerCpu.Should().Be(distro.NonProdWorker.Cpu);
        config.DevWorkerRam.Should().Be(distro.NonProdWorker.Ram);
        config.DevWorkerDisk.Should().Be(distro.NonProdWorker.Disk);
    }

    [Fact]
    public void InitializeFromDistro_SetsTestEnvironmentFromNonProd()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistroConfig();

        config.InitializeFromDistro(distro);

        config.TestMasterCpu.Should().Be(distro.NonProdControlPlane.Cpu);
        config.TestMasterRam.Should().Be(distro.NonProdControlPlane.Ram);
        config.TestMasterDisk.Should().Be(distro.NonProdControlPlane.Disk);
        config.TestInfraCpu.Should().Be(distro.NonProdInfra.Cpu);
        config.TestInfraRam.Should().Be(distro.NonProdInfra.Ram);
        config.TestInfraDisk.Should().Be(distro.NonProdInfra.Disk);
        config.TestWorkerCpu.Should().Be(distro.NonProdWorker.Cpu);
        config.TestWorkerRam.Should().Be(distro.NonProdWorker.Ram);
        config.TestWorkerDisk.Should().Be(distro.NonProdWorker.Disk);
    }

    [Fact]
    public void InitializeFromDistro_SetsStageEnvironmentFromNonProd()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistroConfig();

        config.InitializeFromDistro(distro);

        config.StageMasterCpu.Should().Be(distro.NonProdControlPlane.Cpu);
        config.StageMasterRam.Should().Be(distro.NonProdControlPlane.Ram);
        config.StageMasterDisk.Should().Be(distro.NonProdControlPlane.Disk);
        config.StageInfraCpu.Should().Be(distro.NonProdInfra.Cpu);
        config.StageInfraRam.Should().Be(distro.NonProdInfra.Ram);
        config.StageInfraDisk.Should().Be(distro.NonProdInfra.Disk);
        config.StageWorkerCpu.Should().Be(distro.NonProdWorker.Cpu);
        config.StageWorkerRam.Should().Be(distro.NonProdWorker.Ram);
        config.StageWorkerDisk.Should().Be(distro.NonProdWorker.Disk);
    }

    [Fact]
    public void InitializeFromDistro_SetsDREnvironmentFromProd()
    {
        var config = new NodeSpecsConfig();
        var distro = CreateTestDistroConfig();

        config.InitializeFromDistro(distro);

        // DR uses Prod specs
        config.DRMasterCpu.Should().Be(distro.ProdControlPlane.Cpu);
        config.DRMasterRam.Should().Be(distro.ProdControlPlane.Ram);
        config.DRMasterDisk.Should().Be(distro.ProdControlPlane.Disk);
        config.DRInfraCpu.Should().Be(distro.ProdInfra.Cpu);
        config.DRInfraRam.Should().Be(distro.ProdInfra.Ram);
        config.DRInfraDisk.Should().Be(distro.ProdInfra.Disk);
        config.DRWorkerCpu.Should().Be(distro.ProdWorker.Cpu);
        config.DRWorkerRam.Should().Be(distro.ProdWorker.Ram);
        config.DRWorkerDisk.Should().Be(distro.ProdWorker.Disk);
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
            DevWorkerCpu = 4,
            DevWorkerRam = 16,
            DevWorkerDisk = 50
        };

        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.ProdMasterCpu.Should().Be(12);
        clone.ProdMasterRam.Should().Be(48);
        clone.ProdMasterDisk.Should().Be(300);
        clone.DevWorkerCpu.Should().Be(4);
        clone.DevWorkerRam.Should().Be(16);
        clone.DevWorkerDisk.Should().Be(50);
    }

    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new NodeSpecsConfig
        {
            ProdMasterCpu = 8
        };

        var clone = original.Clone();
        clone.ProdMasterCpu = 16;

        original.ProdMasterCpu.Should().Be(8);
        clone.ProdMasterCpu.Should().Be(16);
    }

    [Fact]
    public void Clone_CopiesAllProperties()
    {
        var original = new NodeSpecsConfig
        {
            // Prod/NonProd
            ProdMasterCpu = 1, ProdMasterRam = 2, ProdMasterDisk = 3,
            NonProdMasterCpu = 4, NonProdMasterRam = 5, NonProdMasterDisk = 6,
            ProdInfraCpu = 7, ProdInfraRam = 8, ProdInfraDisk = 9,
            NonProdInfraCpu = 10, NonProdInfraRam = 11, NonProdInfraDisk = 12,
            ProdWorkerCpu = 13, ProdWorkerRam = 14, ProdWorkerDisk = 15,
            NonProdWorkerCpu = 16, NonProdWorkerRam = 17, NonProdWorkerDisk = 18,
            // Per-environment
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

        // Verify all values were copied
        clone.ProdMasterCpu.Should().Be(1);
        clone.NonProdMasterDisk.Should().Be(6);
        clone.ProdInfraCpu.Should().Be(7);
        clone.NonProdWorkerDisk.Should().Be(18);
        clone.DevMasterCpu.Should().Be(19);
        clone.TestWorkerDisk.Should().Be(36);
        clone.StageMasterCpu.Should().Be(37);
        clone.DRWorkerDisk.Should().Be(54);
    }

    #endregion

    #region Property Modification Tests

    [Fact]
    public void Properties_CanBeModified()
    {
        var config = new NodeSpecsConfig
        {
            ProdMasterCpu = 16,
            ProdMasterRam = 64,
            ProdMasterDisk = 500
        };

        config.ProdMasterCpu.Should().Be(16);
        config.ProdMasterRam.Should().Be(64);
        config.ProdMasterDisk.Should().Be(500);
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        var config1 = new NodeSpecsConfig();
        var config2 = new NodeSpecsConfig();

        config1.ProdMasterCpu = 32;

        config1.ProdMasterCpu.Should().Be(32);
        config2.ProdMasterCpu.Should().Be(8);
    }

    #endregion

    #region Business Rule Tests

    [Fact]
    public void ProdEnvironments_HaveLargerWorkerResources()
    {
        var config = new NodeSpecsConfig();

        // Prod and DR should have larger worker resources than Dev/Test/Stage
        config.ProdWorkerCpu.Should().BeGreaterThan(config.DevWorkerCpu);
        config.ProdWorkerRam.Should().BeGreaterThan(config.DevWorkerRam);
        config.DRWorkerCpu.Should().BeGreaterThan(config.TestWorkerCpu);
        config.DRWorkerRam.Should().BeGreaterThan(config.TestWorkerRam);
    }

    [Fact]
    public void ProdAndDR_HaveSameDefaults()
    {
        var config = new NodeSpecsConfig();

        // DR mirrors Prod
        config.DRMasterCpu.Should().Be(config.ProdMasterCpu);
        config.DRMasterRam.Should().Be(config.ProdMasterRam);
        config.DRMasterDisk.Should().Be(config.ProdMasterDisk);
        config.DRWorkerCpu.Should().Be(config.ProdWorkerCpu);
        config.DRWorkerRam.Should().Be(config.ProdWorkerRam);
        config.DRWorkerDisk.Should().Be(config.ProdWorkerDisk);
    }

    [Fact]
    public void InfraNodes_HaveLargerDisk_ThanControlPlane()
    {
        var config = new NodeSpecsConfig();

        // Infrastructure nodes need more disk for logging, monitoring, etc.
        config.ProdInfraDisk.Should().BeGreaterThan(config.ProdMasterDisk);
        config.NonProdInfraDisk.Should().BeGreaterThan(config.NonProdMasterDisk);
    }

    #endregion

    #region SetSpec Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "Master", "Cpu")]
    [InlineData(EnvironmentType.Dev, "Master", "Ram")]
    [InlineData(EnvironmentType.Dev, "Master", "Disk")]
    [InlineData(EnvironmentType.Dev, "Infra", "Cpu")]
    [InlineData(EnvironmentType.Dev, "Infra", "Ram")]
    [InlineData(EnvironmentType.Dev, "Infra", "Disk")]
    [InlineData(EnvironmentType.Dev, "Worker", "Cpu")]
    [InlineData(EnvironmentType.Dev, "Worker", "Ram")]
    [InlineData(EnvironmentType.Dev, "Worker", "Disk")]
    [InlineData(EnvironmentType.Test, "Master", "Cpu")]
    [InlineData(EnvironmentType.Test, "Infra", "Ram")]
    [InlineData(EnvironmentType.Test, "Worker", "Disk")]
    [InlineData(EnvironmentType.Stage, "Master", "Cpu")]
    [InlineData(EnvironmentType.Stage, "Infra", "Ram")]
    [InlineData(EnvironmentType.Stage, "Worker", "Disk")]
    [InlineData(EnvironmentType.Prod, "Master", "Cpu")]
    [InlineData(EnvironmentType.Prod, "Infra", "Ram")]
    [InlineData(EnvironmentType.Prod, "Worker", "Disk")]
    [InlineData(EnvironmentType.DR, "Master", "Cpu")]
    [InlineData(EnvironmentType.DR, "Infra", "Ram")]
    [InlineData(EnvironmentType.DR, "Worker", "Disk")]
    public void SetSpec_ReturnsTrue_ForValidCombinations(EnvironmentType env, string nodeType, string specType)
    {
        var config = new NodeSpecsConfig();

        var result = config.SetSpec(env, nodeType, specType, 42);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "Master", "Cpu", nameof(NodeSpecsConfig.DevMasterCpu))]
    [InlineData(EnvironmentType.Test, "Infra", "Ram", nameof(NodeSpecsConfig.TestInfraRam))]
    [InlineData(EnvironmentType.Stage, "Worker", "Disk", nameof(NodeSpecsConfig.StageWorkerDisk))]
    [InlineData(EnvironmentType.Prod, "Master", "Ram", nameof(NodeSpecsConfig.ProdMasterRam))]
    [InlineData(EnvironmentType.DR, "Worker", "Cpu", nameof(NodeSpecsConfig.DRWorkerCpu))]
    public void SetSpec_SetsCorrectProperty(EnvironmentType env, string nodeType, string specType, string propertyName)
    {
        var config = new NodeSpecsConfig();
        const int testValue = 99;

        config.SetSpec(env, nodeType, specType, testValue);

        var propertyValue = (int?)config.GetType().GetProperty(propertyName)?.GetValue(config);
        propertyValue.Should().Be(testValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetSpec_ReturnsFalse_ForInvalidValue(int invalidValue)
    {
        var config = new NodeSpecsConfig();
        var originalValue = config.DevMasterCpu;

        var result = config.SetSpec(EnvironmentType.Dev, "Master", "Cpu", invalidValue);

        result.Should().BeFalse();
        config.DevMasterCpu.Should().Be(originalValue, "value should not be modified");
    }

    [Theory]
    [InlineData("Invalid", "Cpu")]
    [InlineData("Master", "Invalid")]
    [InlineData("Unknown", "Unknown")]
    public void SetSpec_ReturnsFalse_ForInvalidNodeTypeOrSpecType(string nodeType, string specType)
    {
        var config = new NodeSpecsConfig();

        var result = config.SetSpec(EnvironmentType.Dev, nodeType, specType, 42);

        result.Should().BeFalse();
    }

    [Fact]
    public void SetSpec_ReturnsFalse_ForInvalidEnvironmentType()
    {
        var config = new NodeSpecsConfig();

        var result = config.SetSpec((EnvironmentType)999, "Master", "Cpu", 42);

        result.Should().BeFalse();
    }

    [Fact]
    public void SetSpec_AllDevCombinations_WorkCorrectly()
    {
        var config = new NodeSpecsConfig();

        config.SetSpec(EnvironmentType.Dev, "Master", "Cpu", 10).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Master", "Ram", 20).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Master", "Disk", 30).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Infra", "Cpu", 11).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Infra", "Ram", 21).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Infra", "Disk", 31).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Worker", "Cpu", 12).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Worker", "Ram", 22).Should().BeTrue();
        config.SetSpec(EnvironmentType.Dev, "Worker", "Disk", 32).Should().BeTrue();

        config.DevMasterCpu.Should().Be(10);
        config.DevMasterRam.Should().Be(20);
        config.DevMasterDisk.Should().Be(30);
        config.DevInfraCpu.Should().Be(11);
        config.DevInfraRam.Should().Be(21);
        config.DevInfraDisk.Should().Be(31);
        config.DevWorkerCpu.Should().Be(12);
        config.DevWorkerRam.Should().Be(22);
        config.DevWorkerDisk.Should().Be(32);
    }

    [Fact]
    public void SetSpec_AllProdCombinations_WorkCorrectly()
    {
        var config = new NodeSpecsConfig();

        config.SetSpec(EnvironmentType.Prod, "Master", "Cpu", 16).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Master", "Ram", 64).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Master", "Disk", 500).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Infra", "Cpu", 24).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Infra", "Ram", 96).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Infra", "Disk", 1000).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Worker", "Cpu", 32).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Worker", "Ram", 128).Should().BeTrue();
        config.SetSpec(EnvironmentType.Prod, "Worker", "Disk", 2000).Should().BeTrue();

        config.ProdMasterCpu.Should().Be(16);
        config.ProdMasterRam.Should().Be(64);
        config.ProdMasterDisk.Should().Be(500);
        config.ProdInfraCpu.Should().Be(24);
        config.ProdInfraRam.Should().Be(96);
        config.ProdInfraDisk.Should().Be(1000);
        config.ProdWorkerCpu.Should().Be(32);
        config.ProdWorkerRam.Should().Be(128);
        config.ProdWorkerDisk.Should().Be(2000);
    }

    #endregion

    #region GetSpec Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "Master", "Cpu")]
    [InlineData(EnvironmentType.Dev, "Master", "Ram")]
    [InlineData(EnvironmentType.Dev, "Master", "Disk")]
    [InlineData(EnvironmentType.Test, "Infra", "Cpu")]
    [InlineData(EnvironmentType.Stage, "Worker", "Ram")]
    [InlineData(EnvironmentType.Prod, "Master", "Disk")]
    [InlineData(EnvironmentType.DR, "Worker", "Cpu")]
    public void GetSpec_ReturnsValue_ForValidCombinations(EnvironmentType env, string nodeType, string specType)
    {
        var config = new NodeSpecsConfig();

        var result = config.GetSpec(env, nodeType, specType);

        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Invalid", "Cpu")]
    [InlineData("Master", "Invalid")]
    [InlineData("Unknown", "Unknown")]
    public void GetSpec_ReturnsNull_ForInvalidNodeTypeOrSpecType(string nodeType, string specType)
    {
        var config = new NodeSpecsConfig();

        var result = config.GetSpec(EnvironmentType.Dev, nodeType, specType);

        result.Should().BeNull();
    }

    [Fact]
    public void GetSpec_ReturnsNull_ForInvalidEnvironmentType()
    {
        var config = new NodeSpecsConfig();

        var result = config.GetSpec((EnvironmentType)999, "Master", "Cpu");

        result.Should().BeNull();
    }

    [Fact]
    public void GetSpec_ReturnsCorrectDefaultValues()
    {
        var config = new NodeSpecsConfig();

        config.GetSpec(EnvironmentType.Dev, "Master", "Cpu").Should().Be(8);
        config.GetSpec(EnvironmentType.Dev, "Master", "Ram").Should().Be(32);
        config.GetSpec(EnvironmentType.Dev, "Master", "Disk").Should().Be(100);
        config.GetSpec(EnvironmentType.Prod, "Worker", "Cpu").Should().Be(16);
        config.GetSpec(EnvironmentType.Prod, "Worker", "Ram").Should().Be(64);
        config.GetSpec(EnvironmentType.Prod, "Worker", "Disk").Should().Be(200);
    }

    [Fact]
    public void GetSpec_ReturnsModifiedValues_AfterSetSpec()
    {
        var config = new NodeSpecsConfig();
        config.SetSpec(EnvironmentType.Test, "Infra", "Ram", 128);

        var result = config.GetSpec(EnvironmentType.Test, "Infra", "Ram");

        result.Should().Be(128);
    }

    [Fact]
    public void GetSpec_AllEnvironments_ReturnCorrectValues()
    {
        var config = new NodeSpecsConfig();

        // Set unique values for each environment
        config.DevMasterCpu = 1;
        config.TestMasterCpu = 2;
        config.StageMasterCpu = 3;
        config.ProdMasterCpu = 4;
        config.DRMasterCpu = 5;

        config.GetSpec(EnvironmentType.Dev, "Master", "Cpu").Should().Be(1);
        config.GetSpec(EnvironmentType.Test, "Master", "Cpu").Should().Be(2);
        config.GetSpec(EnvironmentType.Stage, "Master", "Cpu").Should().Be(3);
        config.GetSpec(EnvironmentType.Prod, "Master", "Cpu").Should().Be(4);
        config.GetSpec(EnvironmentType.DR, "Master", "Cpu").Should().Be(5);
    }

    [Fact]
    public void SetSpec_And_GetSpec_AreConsistent()
    {
        var config = new NodeSpecsConfig();
        var testCases = new[]
        {
            (EnvironmentType.Dev, "Master", "Cpu", 10),
            (EnvironmentType.Test, "Infra", "Ram", 20),
            (EnvironmentType.Stage, "Worker", "Disk", 30),
            (EnvironmentType.Prod, "Master", "Ram", 40),
            (EnvironmentType.DR, "Worker", "Cpu", 50)
        };

        foreach (var (env, nodeType, specType, value) in testCases)
        {
            config.SetSpec(env, nodeType, specType, value);
            config.GetSpec(env, nodeType, specType).Should().Be(value,
                $"GetSpec should return {value} for {env}/{nodeType}/{specType} after SetSpec");
        }
    }

    #endregion

    #region Helper Methods

    private static DistributionConfig CreateTestDistroConfig()
    {
        return new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "Test OpenShift",
            Vendor = "Red Hat",
            ProdControlPlane = new NodeSpecs(12, 48, 250),
            NonProdControlPlane = new NodeSpecs(6, 24, 125),
            ProdInfra = new NodeSpecs(10, 40, 600),
            NonProdInfra = new NodeSpecs(5, 20, 300),
            ProdWorker = new NodeSpecs(20, 80, 250),
            NonProdWorker = new NodeSpecs(10, 40, 125)
        };
    }

    #endregion
}
