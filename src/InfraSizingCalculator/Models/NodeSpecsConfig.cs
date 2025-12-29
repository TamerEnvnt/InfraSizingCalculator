using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Node specifications configuration for K8s clusters.
/// Supports both single-cluster (Prod/NonProd) and multi-cluster (per-environment) modes.
/// </summary>
public class NodeSpecsConfig
{
    // Legacy Prod/NonProd properties (for single cluster modes)
    public int ProdMasterCpu { get; set; } = 8;
    public int ProdMasterRam { get; set; } = 32;
    public int ProdMasterDisk { get; set; } = 200;
    public int NonProdMasterCpu { get; set; } = 8;
    public int NonProdMasterRam { get; set; } = 32;
    public int NonProdMasterDisk { get; set; } = 100;

    // Infrastructure node specs (OpenShift)
    public int ProdInfraCpu { get; set; } = 8;
    public int ProdInfraRam { get; set; } = 32;
    public int ProdInfraDisk { get; set; } = 500;
    public int NonProdInfraCpu { get; set; } = 8;
    public int NonProdInfraRam { get; set; } = 32;
    public int NonProdInfraDisk { get; set; } = 200;

    // Worker node specs
    public int ProdWorkerCpu { get; set; } = 16;
    public int ProdWorkerRam { get; set; } = 64;
    public int ProdWorkerDisk { get; set; } = 200;
    public int NonProdWorkerCpu { get; set; } = 8;
    public int NonProdWorkerRam { get; set; } = 32;
    public int NonProdWorkerDisk { get; set; } = 100;

    // Per-Environment Node Specs (for multi-cluster mode)
    // Dev environment
    public int DevMasterCpu { get; set; } = 8;
    public int DevMasterRam { get; set; } = 32;
    public int DevMasterDisk { get; set; } = 100;
    public int DevInfraCpu { get; set; } = 8;
    public int DevInfraRam { get; set; } = 32;
    public int DevInfraDisk { get; set; } = 200;
    public int DevWorkerCpu { get; set; } = 8;
    public int DevWorkerRam { get; set; } = 32;
    public int DevWorkerDisk { get; set; } = 100;

    // Test environment
    public int TestMasterCpu { get; set; } = 8;
    public int TestMasterRam { get; set; } = 32;
    public int TestMasterDisk { get; set; } = 100;
    public int TestInfraCpu { get; set; } = 8;
    public int TestInfraRam { get; set; } = 32;
    public int TestInfraDisk { get; set; } = 200;
    public int TestWorkerCpu { get; set; } = 8;
    public int TestWorkerRam { get; set; } = 32;
    public int TestWorkerDisk { get; set; } = 100;

    // Stage environment
    public int StageMasterCpu { get; set; } = 8;
    public int StageMasterRam { get; set; } = 32;
    public int StageMasterDisk { get; set; } = 100;
    public int StageInfraCpu { get; set; } = 8;
    public int StageInfraRam { get; set; } = 32;
    public int StageInfraDisk { get; set; } = 200;
    public int StageWorkerCpu { get; set; } = 8;
    public int StageWorkerRam { get; set; } = 32;
    public int StageWorkerDisk { get; set; } = 100;

    // DR environment (similar to Prod defaults)
    public int DRMasterCpu { get; set; } = 8;
    public int DRMasterRam { get; set; } = 32;
    public int DRMasterDisk { get; set; } = 200;
    public int DRInfraCpu { get; set; } = 8;
    public int DRInfraRam { get; set; } = 32;
    public int DRInfraDisk { get; set; } = 500;
    public int DRWorkerCpu { get; set; } = 16;
    public int DRWorkerRam { get; set; } = 64;
    public int DRWorkerDisk { get; set; } = 200;

    /// <summary>
    /// Gets control plane specs for a specific environment
    /// </summary>
    public NodeSpecs GetControlPlaneSpecs(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => new NodeSpecs(DevMasterCpu, DevMasterRam, DevMasterDisk),
        EnvironmentType.Test => new NodeSpecs(TestMasterCpu, TestMasterRam, TestMasterDisk),
        EnvironmentType.Stage => new NodeSpecs(StageMasterCpu, StageMasterRam, StageMasterDisk),
        EnvironmentType.Prod => new NodeSpecs(ProdMasterCpu, ProdMasterRam, ProdMasterDisk),
        EnvironmentType.DR => new NodeSpecs(DRMasterCpu, DRMasterRam, DRMasterDisk),
        _ => new NodeSpecs(ProdMasterCpu, ProdMasterRam, ProdMasterDisk)
    };

    /// <summary>
    /// Gets infrastructure node specs for a specific environment
    /// </summary>
    public NodeSpecs GetInfraSpecs(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => new NodeSpecs(DevInfraCpu, DevInfraRam, DevInfraDisk),
        EnvironmentType.Test => new NodeSpecs(TestInfraCpu, TestInfraRam, TestInfraDisk),
        EnvironmentType.Stage => new NodeSpecs(StageInfraCpu, StageInfraRam, StageInfraDisk),
        EnvironmentType.Prod => new NodeSpecs(ProdInfraCpu, ProdInfraRam, ProdInfraDisk),
        EnvironmentType.DR => new NodeSpecs(DRInfraCpu, DRInfraRam, DRInfraDisk),
        _ => new NodeSpecs(ProdInfraCpu, ProdInfraRam, ProdInfraDisk)
    };

    /// <summary>
    /// Gets worker node specs for a specific environment
    /// </summary>
    public NodeSpecs GetWorkerSpecs(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => new NodeSpecs(DevWorkerCpu, DevWorkerRam, DevWorkerDisk),
        EnvironmentType.Test => new NodeSpecs(TestWorkerCpu, TestWorkerRam, TestWorkerDisk),
        EnvironmentType.Stage => new NodeSpecs(StageWorkerCpu, StageWorkerRam, StageWorkerDisk),
        EnvironmentType.Prod => new NodeSpecs(ProdWorkerCpu, ProdWorkerRam, ProdWorkerDisk),
        EnvironmentType.DR => new NodeSpecs(DRWorkerCpu, DRWorkerRam, DRWorkerDisk),
        _ => new NodeSpecs(ProdWorkerCpu, ProdWorkerRam, ProdWorkerDisk)
    };

    /// <summary>
    /// Initialize per-environment specs from a distribution config
    /// </summary>
    public void InitializeFromDistro(DistributionConfig distro)
    {
        // Single cluster modes use Prod specs
        ProdMasterCpu = distro.ProdControlPlane.Cpu;
        ProdMasterRam = distro.ProdControlPlane.Ram;
        ProdMasterDisk = distro.ProdControlPlane.Disk;
        NonProdMasterCpu = distro.NonProdControlPlane.Cpu;
        NonProdMasterRam = distro.NonProdControlPlane.Ram;
        NonProdMasterDisk = distro.NonProdControlPlane.Disk;

        ProdInfraCpu = distro.ProdInfra.Cpu;
        ProdInfraRam = distro.ProdInfra.Ram;
        ProdInfraDisk = distro.ProdInfra.Disk;
        NonProdInfraCpu = distro.NonProdInfra.Cpu;
        NonProdInfraRam = distro.NonProdInfra.Ram;
        NonProdInfraDisk = distro.NonProdInfra.Disk;

        ProdWorkerCpu = distro.ProdWorker.Cpu;
        ProdWorkerRam = distro.ProdWorker.Ram;
        ProdWorkerDisk = distro.ProdWorker.Disk;
        NonProdWorkerCpu = distro.NonProdWorker.Cpu;
        NonProdWorkerRam = distro.NonProdWorker.Ram;
        NonProdWorkerDisk = distro.NonProdWorker.Disk;

        // Per-environment specs - Dev, Test, Stage use NonProd defaults
        DevMasterCpu = distro.NonProdControlPlane.Cpu;
        DevMasterRam = distro.NonProdControlPlane.Ram;
        DevMasterDisk = distro.NonProdControlPlane.Disk;
        DevInfraCpu = distro.NonProdInfra.Cpu;
        DevInfraRam = distro.NonProdInfra.Ram;
        DevInfraDisk = distro.NonProdInfra.Disk;
        DevWorkerCpu = distro.NonProdWorker.Cpu;
        DevWorkerRam = distro.NonProdWorker.Ram;
        DevWorkerDisk = distro.NonProdWorker.Disk;

        TestMasterCpu = distro.NonProdControlPlane.Cpu;
        TestMasterRam = distro.NonProdControlPlane.Ram;
        TestMasterDisk = distro.NonProdControlPlane.Disk;
        TestInfraCpu = distro.NonProdInfra.Cpu;
        TestInfraRam = distro.NonProdInfra.Ram;
        TestInfraDisk = distro.NonProdInfra.Disk;
        TestWorkerCpu = distro.NonProdWorker.Cpu;
        TestWorkerRam = distro.NonProdWorker.Ram;
        TestWorkerDisk = distro.NonProdWorker.Disk;

        StageMasterCpu = distro.NonProdControlPlane.Cpu;
        StageMasterRam = distro.NonProdControlPlane.Ram;
        StageMasterDisk = distro.NonProdControlPlane.Disk;
        StageInfraCpu = distro.NonProdInfra.Cpu;
        StageInfraRam = distro.NonProdInfra.Ram;
        StageInfraDisk = distro.NonProdInfra.Disk;
        StageWorkerCpu = distro.NonProdWorker.Cpu;
        StageWorkerRam = distro.NonProdWorker.Ram;
        StageWorkerDisk = distro.NonProdWorker.Disk;

        // DR uses Prod-like specs
        DRMasterCpu = distro.ProdControlPlane.Cpu;
        DRMasterRam = distro.ProdControlPlane.Ram;
        DRMasterDisk = distro.ProdControlPlane.Disk;
        DRInfraCpu = distro.ProdInfra.Cpu;
        DRInfraRam = distro.ProdInfra.Ram;
        DRInfraDisk = distro.ProdInfra.Disk;
        DRWorkerCpu = distro.ProdWorker.Cpu;
        DRWorkerRam = distro.ProdWorker.Ram;
        DRWorkerDisk = distro.ProdWorker.Disk;
    }

    /// <summary>
    /// Sets a node specification value for a specific environment, node type, and spec type.
    /// Returns true if the value was set successfully, false if the combination was invalid.
    /// </summary>
    /// <param name="env">The environment type (Dev, Test, Stage, Prod, DR)</param>
    /// <param name="nodeType">The node type ("Master", "Infra", "Worker")</param>
    /// <param name="specType">The spec type ("Cpu", "Ram", "Disk")</param>
    /// <param name="value">The value to set (must be >= 1)</param>
    /// <returns>True if the value was set, false if invalid</returns>
    public bool SetSpec(EnvironmentType env, string nodeType, string specType, int value)
    {
        if (value < 1) return false;

        switch ((env, nodeType, specType))
        {
            case (EnvironmentType.Dev, "Master", "Cpu"): DevMasterCpu = value; return true;
            case (EnvironmentType.Dev, "Master", "Ram"): DevMasterRam = value; return true;
            case (EnvironmentType.Dev, "Master", "Disk"): DevMasterDisk = value; return true;
            case (EnvironmentType.Dev, "Infra", "Cpu"): DevInfraCpu = value; return true;
            case (EnvironmentType.Dev, "Infra", "Ram"): DevInfraRam = value; return true;
            case (EnvironmentType.Dev, "Infra", "Disk"): DevInfraDisk = value; return true;
            case (EnvironmentType.Dev, "Worker", "Cpu"): DevWorkerCpu = value; return true;
            case (EnvironmentType.Dev, "Worker", "Ram"): DevWorkerRam = value; return true;
            case (EnvironmentType.Dev, "Worker", "Disk"): DevWorkerDisk = value; return true;
            case (EnvironmentType.Test, "Master", "Cpu"): TestMasterCpu = value; return true;
            case (EnvironmentType.Test, "Master", "Ram"): TestMasterRam = value; return true;
            case (EnvironmentType.Test, "Master", "Disk"): TestMasterDisk = value; return true;
            case (EnvironmentType.Test, "Infra", "Cpu"): TestInfraCpu = value; return true;
            case (EnvironmentType.Test, "Infra", "Ram"): TestInfraRam = value; return true;
            case (EnvironmentType.Test, "Infra", "Disk"): TestInfraDisk = value; return true;
            case (EnvironmentType.Test, "Worker", "Cpu"): TestWorkerCpu = value; return true;
            case (EnvironmentType.Test, "Worker", "Ram"): TestWorkerRam = value; return true;
            case (EnvironmentType.Test, "Worker", "Disk"): TestWorkerDisk = value; return true;
            case (EnvironmentType.Stage, "Master", "Cpu"): StageMasterCpu = value; return true;
            case (EnvironmentType.Stage, "Master", "Ram"): StageMasterRam = value; return true;
            case (EnvironmentType.Stage, "Master", "Disk"): StageMasterDisk = value; return true;
            case (EnvironmentType.Stage, "Infra", "Cpu"): StageInfraCpu = value; return true;
            case (EnvironmentType.Stage, "Infra", "Ram"): StageInfraRam = value; return true;
            case (EnvironmentType.Stage, "Infra", "Disk"): StageInfraDisk = value; return true;
            case (EnvironmentType.Stage, "Worker", "Cpu"): StageWorkerCpu = value; return true;
            case (EnvironmentType.Stage, "Worker", "Ram"): StageWorkerRam = value; return true;
            case (EnvironmentType.Stage, "Worker", "Disk"): StageWorkerDisk = value; return true;
            case (EnvironmentType.Prod, "Master", "Cpu"): ProdMasterCpu = value; return true;
            case (EnvironmentType.Prod, "Master", "Ram"): ProdMasterRam = value; return true;
            case (EnvironmentType.Prod, "Master", "Disk"): ProdMasterDisk = value; return true;
            case (EnvironmentType.Prod, "Infra", "Cpu"): ProdInfraCpu = value; return true;
            case (EnvironmentType.Prod, "Infra", "Ram"): ProdInfraRam = value; return true;
            case (EnvironmentType.Prod, "Infra", "Disk"): ProdInfraDisk = value; return true;
            case (EnvironmentType.Prod, "Worker", "Cpu"): ProdWorkerCpu = value; return true;
            case (EnvironmentType.Prod, "Worker", "Ram"): ProdWorkerRam = value; return true;
            case (EnvironmentType.Prod, "Worker", "Disk"): ProdWorkerDisk = value; return true;
            case (EnvironmentType.DR, "Master", "Cpu"): DRMasterCpu = value; return true;
            case (EnvironmentType.DR, "Master", "Ram"): DRMasterRam = value; return true;
            case (EnvironmentType.DR, "Master", "Disk"): DRMasterDisk = value; return true;
            case (EnvironmentType.DR, "Infra", "Cpu"): DRInfraCpu = value; return true;
            case (EnvironmentType.DR, "Infra", "Ram"): DRInfraRam = value; return true;
            case (EnvironmentType.DR, "Infra", "Disk"): DRInfraDisk = value; return true;
            case (EnvironmentType.DR, "Worker", "Cpu"): DRWorkerCpu = value; return true;
            case (EnvironmentType.DR, "Worker", "Ram"): DRWorkerRam = value; return true;
            case (EnvironmentType.DR, "Worker", "Disk"): DRWorkerDisk = value; return true;
            default: return false;
        }
    }

    /// <summary>
    /// Gets a node specification value for a specific environment, node type, and spec type.
    /// Returns the value if found, or null if the combination was invalid.
    /// </summary>
    public int? GetSpec(EnvironmentType env, string nodeType, string specType)
    {
        return (env, nodeType, specType) switch
        {
            (EnvironmentType.Dev, "Master", "Cpu") => DevMasterCpu,
            (EnvironmentType.Dev, "Master", "Ram") => DevMasterRam,
            (EnvironmentType.Dev, "Master", "Disk") => DevMasterDisk,
            (EnvironmentType.Dev, "Infra", "Cpu") => DevInfraCpu,
            (EnvironmentType.Dev, "Infra", "Ram") => DevInfraRam,
            (EnvironmentType.Dev, "Infra", "Disk") => DevInfraDisk,
            (EnvironmentType.Dev, "Worker", "Cpu") => DevWorkerCpu,
            (EnvironmentType.Dev, "Worker", "Ram") => DevWorkerRam,
            (EnvironmentType.Dev, "Worker", "Disk") => DevWorkerDisk,
            (EnvironmentType.Test, "Master", "Cpu") => TestMasterCpu,
            (EnvironmentType.Test, "Master", "Ram") => TestMasterRam,
            (EnvironmentType.Test, "Master", "Disk") => TestMasterDisk,
            (EnvironmentType.Test, "Infra", "Cpu") => TestInfraCpu,
            (EnvironmentType.Test, "Infra", "Ram") => TestInfraRam,
            (EnvironmentType.Test, "Infra", "Disk") => TestInfraDisk,
            (EnvironmentType.Test, "Worker", "Cpu") => TestWorkerCpu,
            (EnvironmentType.Test, "Worker", "Ram") => TestWorkerRam,
            (EnvironmentType.Test, "Worker", "Disk") => TestWorkerDisk,
            (EnvironmentType.Stage, "Master", "Cpu") => StageMasterCpu,
            (EnvironmentType.Stage, "Master", "Ram") => StageMasterRam,
            (EnvironmentType.Stage, "Master", "Disk") => StageMasterDisk,
            (EnvironmentType.Stage, "Infra", "Cpu") => StageInfraCpu,
            (EnvironmentType.Stage, "Infra", "Ram") => StageInfraRam,
            (EnvironmentType.Stage, "Infra", "Disk") => StageInfraDisk,
            (EnvironmentType.Stage, "Worker", "Cpu") => StageWorkerCpu,
            (EnvironmentType.Stage, "Worker", "Ram") => StageWorkerRam,
            (EnvironmentType.Stage, "Worker", "Disk") => StageWorkerDisk,
            (EnvironmentType.Prod, "Master", "Cpu") => ProdMasterCpu,
            (EnvironmentType.Prod, "Master", "Ram") => ProdMasterRam,
            (EnvironmentType.Prod, "Master", "Disk") => ProdMasterDisk,
            (EnvironmentType.Prod, "Infra", "Cpu") => ProdInfraCpu,
            (EnvironmentType.Prod, "Infra", "Ram") => ProdInfraRam,
            (EnvironmentType.Prod, "Infra", "Disk") => ProdInfraDisk,
            (EnvironmentType.Prod, "Worker", "Cpu") => ProdWorkerCpu,
            (EnvironmentType.Prod, "Worker", "Ram") => ProdWorkerRam,
            (EnvironmentType.Prod, "Worker", "Disk") => ProdWorkerDisk,
            (EnvironmentType.DR, "Master", "Cpu") => DRMasterCpu,
            (EnvironmentType.DR, "Master", "Ram") => DRMasterRam,
            (EnvironmentType.DR, "Master", "Disk") => DRMasterDisk,
            (EnvironmentType.DR, "Infra", "Cpu") => DRInfraCpu,
            (EnvironmentType.DR, "Infra", "Ram") => DRInfraRam,
            (EnvironmentType.DR, "Infra", "Disk") => DRInfraDisk,
            (EnvironmentType.DR, "Worker", "Cpu") => DRWorkerCpu,
            (EnvironmentType.DR, "Worker", "Ram") => DRWorkerRam,
            (EnvironmentType.DR, "Worker", "Disk") => DRWorkerDisk,
            _ => null
        };
    }

    /// <summary>
    /// Creates a deep copy of this configuration
    /// </summary>
    public NodeSpecsConfig Clone() => new()
    {
        ProdMasterCpu = ProdMasterCpu,
        ProdMasterRam = ProdMasterRam,
        ProdMasterDisk = ProdMasterDisk,
        NonProdMasterCpu = NonProdMasterCpu,
        NonProdMasterRam = NonProdMasterRam,
        NonProdMasterDisk = NonProdMasterDisk,
        ProdInfraCpu = ProdInfraCpu,
        ProdInfraRam = ProdInfraRam,
        ProdInfraDisk = ProdInfraDisk,
        NonProdInfraCpu = NonProdInfraCpu,
        NonProdInfraRam = NonProdInfraRam,
        NonProdInfraDisk = NonProdInfraDisk,
        ProdWorkerCpu = ProdWorkerCpu,
        ProdWorkerRam = ProdWorkerRam,
        ProdWorkerDisk = ProdWorkerDisk,
        NonProdWorkerCpu = NonProdWorkerCpu,
        NonProdWorkerRam = NonProdWorkerRam,
        NonProdWorkerDisk = NonProdWorkerDisk,
        // Per-environment
        DevMasterCpu = DevMasterCpu,
        DevMasterRam = DevMasterRam,
        DevMasterDisk = DevMasterDisk,
        DevInfraCpu = DevInfraCpu,
        DevInfraRam = DevInfraRam,
        DevInfraDisk = DevInfraDisk,
        DevWorkerCpu = DevWorkerCpu,
        DevWorkerRam = DevWorkerRam,
        DevWorkerDisk = DevWorkerDisk,
        TestMasterCpu = TestMasterCpu,
        TestMasterRam = TestMasterRam,
        TestMasterDisk = TestMasterDisk,
        TestInfraCpu = TestInfraCpu,
        TestInfraRam = TestInfraRam,
        TestInfraDisk = TestInfraDisk,
        TestWorkerCpu = TestWorkerCpu,
        TestWorkerRam = TestWorkerRam,
        TestWorkerDisk = TestWorkerDisk,
        StageMasterCpu = StageMasterCpu,
        StageMasterRam = StageMasterRam,
        StageMasterDisk = StageMasterDisk,
        StageInfraCpu = StageInfraCpu,
        StageInfraRam = StageInfraRam,
        StageInfraDisk = StageInfraDisk,
        StageWorkerCpu = StageWorkerCpu,
        StageWorkerRam = StageWorkerRam,
        StageWorkerDisk = StageWorkerDisk,
        DRMasterCpu = DRMasterCpu,
        DRMasterRam = DRMasterRam,
        DRMasterDisk = DRMasterDisk,
        DRInfraCpu = DRInfraCpu,
        DRInfraRam = DRInfraRam,
        DRInfraDisk = DRInfraDisk,
        DRWorkerCpu = DRWorkerCpu,
        DRWorkerRam = DRWorkerRam,
        DRWorkerDisk = DRWorkerDisk
    };
}
