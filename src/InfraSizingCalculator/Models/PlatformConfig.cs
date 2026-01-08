namespace InfraSizingCalculator.Models;

/// <summary>
/// K8s-specific pod resource configuration using Kubernetes native units.
/// Requests are guaranteed resources; Limits are burst maximums.
/// </summary>
public class K8sPodConfig
{
    /// <summary>CPU request in millicores (e.g., 500 = 500m = 0.5 cores)</summary>
    public int CpuRequestMillicores { get; set; } = 500;

    /// <summary>CPU limit in millicores (e.g., 1000 = 1000m = 1 core)</summary>
    public int CpuLimitMillicores { get; set; } = 1000;

    /// <summary>Memory request in MiB (e.g., 1024 = 1Gi)</summary>
    public int MemoryRequestMi { get; set; } = 1024;

    /// <summary>Memory limit in MiB (e.g., 2048 = 2Gi)</summary>
    public int MemoryLimitMi { get; set; } = 2048;

    /// <summary>Selected preset name (micro, small, medium, large, xlarge) or empty for custom</summary>
    public string SelectedPreset { get; set; } = "medium";
}

/// <summary>
/// K8s environment configuration with replica counts per environment.
/// </summary>
public class K8sEnvironmentConfig
{
    public int DevReplicas { get; set; } = 1;
    public int TestReplicas { get; set; } = 2;
    public int StagingReplicas { get; set; } = 2;
    public int ProductionReplicas { get; set; } = 3;

    public int TotalReplicas => DevReplicas + TestReplicas + StagingReplicas + ProductionReplicas;

    public int GetReplicas(string envId) => envId switch
    {
        "dev" => DevReplicas,
        "test" => TestReplicas,
        "staging" => StagingReplicas,
        "production" => ProductionReplicas,
        _ => 1
    };

    public void SetReplicas(string envId, int value)
    {
        switch (envId)
        {
            case "dev": DevReplicas = Math.Max(1, value); break;
            case "test": TestReplicas = Math.Max(1, value); break;
            case "staging": StagingReplicas = Math.Max(1, value); break;
            case "production": ProductionReplicas = Math.Max(1, value); break;
        }
    }
}

/// <summary>
/// K8s worker node configuration with allocatable resource calculations.
/// </summary>
public class K8sNodeConfig
{
    public int WorkerVCpu { get; set; } = 8;
    public int WorkerMemoryGB { get; set; } = 16;
    public int WorkerCount { get; set; } = 4;
    public bool N1Redundancy { get; set; } = true;
    public int ControlPlaneCount { get; set; } = 3;
    public string SelectedPreset { get; set; } = "medium";

    // Calculated properties
    public int TotalWorkerNodes => WorkerCount + (N1Redundancy ? 1 : 0);
    public int TotalNodes => TotalWorkerNodes + ControlPlaneCount;

    /// <summary>
    /// Allocatable percentage based on node size.
    /// Larger nodes have less relative overhead.
    /// </summary>
    public int AllocatablePercent => WorkerMemoryGB switch
    {
        <= 8 => 75,
        <= 16 => 85,
        <= 32 => 90,
        _ => 93
    };

    /// <summary>Allocatable memory per node in GB</summary>
    public double AllocatableMemoryGB => WorkerMemoryGB * AllocatablePercent / 100.0;

    /// <summary>Allocatable CPU per node in millicores (~6% system overhead)</summary>
    public int AllocatableCpuMillicores => (int)(WorkerVCpu * 1000 * 0.94);

    // Cluster totals (assuming 8 vCPU, 32GB per control plane node)
    public int TotalClusterVCpu => (TotalWorkerNodes * WorkerVCpu) + (ControlPlaneCount * 8);
    public int TotalClusterMemoryGB => (TotalWorkerNodes * WorkerMemoryGB) + (ControlPlaneCount * 32);
    public double TotalAllocatableCpu => TotalWorkerNodes * (WorkerVCpu * 0.94);
    public double TotalAllocatableMemoryGB => TotalWorkerNodes * AllocatableMemoryGB;
}

/// <summary>
/// VM-specific application configuration using traditional units.
/// </summary>
public class VmAppConfig
{
    /// <summary>Operating system type (linux or windows)</summary>
    public string OperatingSystem { get; set; } = "linux";

    /// <summary>vCPU per VM (whole cores)</summary>
    public int VCpu { get; set; } = 2;

    /// <summary>RAM per VM in GB</summary>
    public int MemoryGB { get; set; } = 4;

    /// <summary>Storage per VM in GB</summary>
    public int StorageGB { get; set; } = 100;

    /// <summary>Selected preset name or empty for custom</summary>
    public string SelectedPreset { get; set; } = "medium";

    /// <summary>OS overhead in GB (Linux ~1GB, Windows ~4GB)</summary>
    public int OsOverheadGB => OperatingSystem == "linux" ? 1 : 4;
}

/// <summary>
/// VM environment configuration with instance counts per environment.
/// </summary>
public class VmEnvironmentConfig
{
    public int DevInstances { get; set; } = 1;
    public int TestInstances { get; set; } = 1;
    public int StagingInstances { get; set; } = 1;
    public int ProductionInstances { get; set; } = 2;

    public int TotalInstances => DevInstances + TestInstances + StagingInstances + ProductionInstances;

    public int GetInstances(string envId) => envId switch
    {
        "dev" => DevInstances,
        "test" => TestInstances,
        "staging" => StagingInstances,
        "production" => ProductionInstances,
        _ => 1
    };

    public void SetInstances(string envId, int value)
    {
        switch (envId)
        {
            case "dev": DevInstances = Math.Max(1, value); break;
            case "test": TestInstances = Math.Max(1, value); break;
            case "staging": StagingInstances = Math.Max(1, value); break;
            case "production": ProductionInstances = Math.Max(1, value); break;
        }
    }
}

/// <summary>
/// VM host (hypervisor) configuration with overcommit ratios.
/// </summary>
public class VmHostConfig
{
    public int HostCores { get; set; } = 16;
    public int HostMemoryGB { get; set; } = 64;
    public int HostStorageGB { get; set; } = 1000;
    public int HostCount { get; set; } = 2;
    public bool N1Redundancy { get; set; } = true;
    public bool OvercommitEnabled { get; set; } = true;
    public string SelectedPreset { get; set; } = "medium";

    // Overcommit ratios per environment
    public double DevCpuRatio { get; set; } = 8.0;
    public double DevMemRatio { get; set; } = 2.0;
    public double TestCpuRatio { get; set; } = 4.0;
    public double TestMemRatio { get; set; } = 1.5;
    public double StagingCpuRatio { get; set; } = 2.0;
    public double StagingMemRatio { get; set; } = 1.25;
    public double ProdCpuRatio { get; set; } = 1.0;
    public double ProdMemRatio { get; set; } = 1.0;

    public int TotalHosts => HostCount + (N1Redundancy ? 1 : 0);
}

/// <summary>
/// Aggregated VM resource requirements calculated from app config and environment config.
/// </summary>
public class VmResourceRequirements
{
    /// <summary>Total vCPU required without overcommit</summary>
    public int TotalVCpu { get; set; }

    /// <summary>Total RAM required without overcommit (includes OS overhead)</summary>
    public int TotalMemoryGB { get; set; }

    /// <summary>Total storage required</summary>
    public int TotalStorageGB { get; set; }

    /// <summary>Total VM instances</summary>
    public int TotalInstances { get; set; }

    /// <summary>OS overhead total in GB</summary>
    public int OsOverheadGB { get; set; }

    /// <summary>App-only RAM (excludes OS overhead)</summary>
    public int AppMemoryGB { get; set; }

    /// <summary>Calculate requirements from app and environment config</summary>
    public static VmResourceRequirements Calculate(VmAppConfig app, VmEnvironmentConfig env)
    {
        var totalInstances = env.TotalInstances;
        var appMemory = app.MemoryGB * totalInstances;
        var osOverhead = app.OsOverheadGB * totalInstances;

        return new VmResourceRequirements
        {
            TotalVCpu = app.VCpu * totalInstances,
            TotalMemoryGB = appMemory + osOverhead,
            TotalStorageGB = app.StorageGB * totalInstances,
            TotalInstances = totalInstances,
            OsOverheadGB = osOverhead,
            AppMemoryGB = appMemory
        };
    }
}

/// <summary>
/// Aggregated K8s resource requirements calculated from pod config and environment config.
/// </summary>
public class K8sResourceRequirements
{
    /// <summary>Total CPU requests in millicores</summary>
    public int TotalCpuRequestMillicores { get; set; }

    /// <summary>Total memory requests in MiB</summary>
    public int TotalMemoryRequestMi { get; set; }

    /// <summary>Total CPU limits in millicores</summary>
    public int TotalCpuLimitMillicores { get; set; }

    /// <summary>Total memory limits in MiB</summary>
    public int TotalMemoryLimitMi { get; set; }

    /// <summary>Total replica count</summary>
    public int TotalReplicas { get; set; }

    /// <summary>Calculate requirements from pod and environment config</summary>
    public static K8sResourceRequirements Calculate(K8sPodConfig pod, K8sEnvironmentConfig env)
    {
        var totalReplicas = env.TotalReplicas;

        return new K8sResourceRequirements
        {
            TotalCpuRequestMillicores = pod.CpuRequestMillicores * totalReplicas,
            TotalMemoryRequestMi = pod.MemoryRequestMi * totalReplicas,
            TotalCpuLimitMillicores = pod.CpuLimitMillicores * totalReplicas,
            TotalMemoryLimitMi = pod.MemoryLimitMi * totalReplicas,
            TotalReplicas = totalReplicas
        };
    }
}
