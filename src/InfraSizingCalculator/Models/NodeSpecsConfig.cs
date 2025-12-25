namespace InfraSizingCalculator.Models;

/// <summary>
/// Node specifications configuration for K8s clusters
/// </summary>
public class NodeSpecsConfig
{
    // Control Plane (Master) specs
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
        NonProdWorkerDisk = NonProdWorkerDisk
    };
}
