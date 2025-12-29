namespace InfraSizingCalculator.Models;

/// <summary>
/// User-configurable calculator settings including infrastructure thresholds,
/// Kubernetes parameters, and per-technology tier specifications.
/// </summary>
public class UICalculatorSettings
{
    // Infrastructure thresholds
    public int LargeDeploymentThreshold { get; set; } = 50;
    public int LargeClusterThreshold { get; set; } = 100;
    public int AppsPerInfraNode { get; set; } = 25;
    public int MinProdInfraLarge { get; set; } = 5;
    public int MinProdInfraSmall { get; set; } = 3;
    public int MaxInfraNodes { get; set; } = 10;

    // Kubernetes parameters - Production
    public double ProdCpuOvercommit { get; set; } = 1.0;
    public double ProdMemoryOvercommit { get; set; } = 1.0;
    public int ProdResourceBuffer { get; set; } = 30;

    // Kubernetes parameters - Non-Production
    public double NonProdCpuOvercommit { get; set; } = 1.0;
    public double NonProdMemoryOvercommit { get; set; } = 1.0;
    public int NonProdResourceBuffer { get; set; } = 15;

    // System settings
    public int NodeSystemReserve { get; set; } = 15;
    public int MinWorkerNodes { get; set; } = 3;

    // Headroom
    public int HeadroomDev { get; set; } = 33;
    public int HeadroomTest { get; set; } = 33;
    public int HeadroomStage { get; set; } = 0;
    public double HeadroomProd { get; set; } = 37.5;
    public double HeadroomDR { get; set; } = 37.5;

    // Replicas
    public int ReplicasDev { get; set; } = 1;
    public int ReplicasTest { get; set; } = 1;
    public int ReplicasStage { get; set; } = 2;
    public int ReplicasProd { get; set; } = 3;
    public int ReplicasDR { get; set; } = 3;

    // Default node specs - Control Plane
    public int ProdMasterCpu { get; set; } = 8;
    public int ProdMasterRam { get; set; } = 32;
    public int ProdMasterDisk { get; set; } = 200;
    public int NonProdMasterCpu { get; set; } = 8;
    public int NonProdMasterRam { get; set; } = 32;
    public int NonProdMasterDisk { get; set; } = 100;

    // Default node specs - Infrastructure
    public int ProdInfraCpu { get; set; } = 8;
    public int ProdInfraRam { get; set; } = 32;
    public int ProdInfraDisk { get; set; } = 500;
    public int NonProdInfraCpu { get; set; } = 8;
    public int NonProdInfraRam { get; set; } = 32;
    public int NonProdInfraDisk { get; set; } = 200;

    // Default node specs - Worker
    public int ProdWorkerCpu { get; set; } = 16;
    public int ProdWorkerRam { get; set; } = 64;
    public int ProdWorkerDisk { get; set; } = 200;
    public int NonProdWorkerCpu { get; set; } = 8;
    public int NonProdWorkerRam { get; set; } = 32;
    public int NonProdWorkerDisk { get; set; } = 100;

    // .NET tier specs
    public double DotNetSmallCpu { get; set; } = 0.25;
    public double DotNetSmallRam { get; set; } = 0.5;
    public double DotNetMediumCpu { get; set; } = 0.5;
    public double DotNetMediumRam { get; set; } = 1;
    public double DotNetLargeCpu { get; set; } = 1;
    public double DotNetLargeRam { get; set; } = 2;
    public double DotNetXLargeCpu { get; set; } = 2;
    public double DotNetXLargeRam { get; set; } = 4;

    // Java tier specs
    public double JavaSmallCpu { get; set; } = 0.5;
    public double JavaSmallRam { get; set; } = 1;
    public double JavaMediumCpu { get; set; } = 1;
    public double JavaMediumRam { get; set; } = 2;
    public double JavaLargeCpu { get; set; } = 2;
    public double JavaLargeRam { get; set; } = 4;
    public double JavaXLargeCpu { get; set; } = 4;
    public double JavaXLargeRam { get; set; } = 8;

    // Node.js tier specs (Small RAM increased to 1 GB per official docs - V8 heap management)
    public double NodeJsSmallCpu { get; set; } = 0.25;
    public double NodeJsSmallRam { get; set; } = 1;
    public double NodeJsMediumCpu { get; set; } = 0.5;
    public double NodeJsMediumRam { get; set; } = 1;
    public double NodeJsLargeCpu { get; set; } = 1;
    public double NodeJsLargeRam { get; set; } = 2;
    public double NodeJsXLargeCpu { get; set; } = 2;
    public double NodeJsXLargeRam { get; set; } = 4;

    // Python tier specs (Small RAM increased to 1 GB per official docs - WSGI/Django overhead)
    public double PythonSmallCpu { get; set; } = 0.25;
    public double PythonSmallRam { get; set; } = 1;
    public double PythonMediumCpu { get; set; } = 0.5;
    public double PythonMediumRam { get; set; } = 1;
    public double PythonLargeCpu { get; set; } = 1;
    public double PythonLargeRam { get; set; } = 2;
    public double PythonXLargeCpu { get; set; } = 2;
    public double PythonXLargeRam { get; set; } = 4;

    // Go tier specs
    public double GoSmallCpu { get; set; } = 0.125;
    public double GoSmallRam { get; set; } = 0.25;
    public double GoMediumCpu { get; set; } = 0.25;
    public double GoMediumRam { get; set; } = 0.5;
    public double GoLargeCpu { get; set; } = 0.5;
    public double GoLargeRam { get; set; } = 1;
    public double GoXLargeCpu { get; set; } = 1;
    public double GoXLargeRam { get; set; } = 2;

    // Mendix tier specs
    public double MendixSmallCpu { get; set; } = 0.5;
    public double MendixSmallRam { get; set; } = 1;
    public double MendixMediumCpu { get; set; } = 1;
    public double MendixMediumRam { get; set; } = 2;
    public double MendixLargeCpu { get; set; } = 2;
    public double MendixLargeRam { get; set; } = 4;
    public double MendixXLargeCpu { get; set; } = 4;
    public double MendixXLargeRam { get; set; } = 8;

    // Mendix Operator settings
    public int MendixOperatorReplicas { get; set; } = 2;
}
