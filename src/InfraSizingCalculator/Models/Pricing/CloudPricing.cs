namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Cloud provider pricing settings
/// </summary>
public class CloudPricingSettings
{
    public AwsPricing Aws { get; set; } = new();
    public AzurePricing Azure { get; set; } = new();
    public GcpPricing Gcp { get; set; } = new();
}

/// <summary>
/// AWS pricing settings
/// </summary>
public class AwsPricing
{
    // EKS Control Plane
    public decimal EksControlPlanePerHour { get; set; } = 0.10m;

    // EC2 Instance Types (us-east-1 on-demand)
    public decimal M5XlargePerHour { get; set; } = 0.192m;      // 4 vCPU, 16GB
    public decimal M52XlargePerHour { get; set; } = 0.384m;     // 8 vCPU, 32GB
    public decimal M54XlargePerHour { get; set; } = 0.768m;     // 16 vCPU, 64GB

    // EBS Storage
    public decimal EbsGp3PerGBMonth { get; set; } = 0.08m;

    // ROSA (OpenShift on AWS)
    public decimal RosaPerWorkerHour { get; set; } = 0.171m;

    // Helper methods
    public decimal GetMonthlyControlPlaneCost() => EksControlPlanePerHour * 730;
    public decimal GetMonthlyInstanceCost(string type, int count) => type switch
    {
        "m5.xlarge" => M5XlargePerHour * 730 * count,
        "m5.2xlarge" => M52XlargePerHour * 730 * count,
        "m5.4xlarge" => M54XlargePerHour * 730 * count,
        _ => M5XlargePerHour * 730 * count
    };
}

/// <summary>
/// Azure pricing settings
/// </summary>
public class AzurePricing
{
    // AKS Control Plane (free tier available)
    public decimal AksControlPlanePerHour { get; set; } = 0m;   // Free tier

    // Azure VMs (East US on-demand)
    public decimal D4sV3PerHour { get; set; } = 0.192m;         // 4 vCPU, 16GB
    public decimal D8sV3PerHour { get; set; } = 0.384m;         // 8 vCPU, 32GB
    public decimal D16sV3PerHour { get; set; } = 0.768m;        // 16 vCPU, 64GB

    // Managed Disks
    public decimal ManagedDiskPremiumPerGBMonth { get; set; } = 0.12m;

    // ARO (Azure Red Hat OpenShift)
    public decimal AroPerWorkerHour { get; set; } = 0.35m;

    // Helper methods
    public decimal GetMonthlyControlPlaneCost() => AksControlPlanePerHour * 730;
    public decimal GetMonthlyInstanceCost(string type, int count) => type switch
    {
        "D4s_v3" => D4sV3PerHour * 730 * count,
        "D8s_v3" => D8sV3PerHour * 730 * count,
        "D16s_v3" => D16sV3PerHour * 730 * count,
        _ => D4sV3PerHour * 730 * count
    };
}

/// <summary>
/// GCP pricing settings
/// </summary>
public class GcpPricing
{
    // GKE Control Plane
    public decimal GkeControlPlanePerHour { get; set; } = 0.10m;

    // GCE Instance Types (us-central1 on-demand)
    public decimal E2Standard4PerHour { get; set; } = 0.134m;   // 4 vCPU, 16GB
    public decimal E2Standard8PerHour { get; set; } = 0.268m;   // 8 vCPU, 32GB
    public decimal E2Standard16PerHour { get; set; } = 0.536m;  // 16 vCPU, 64GB

    // Persistent Disk
    public decimal PdSsdPerGBMonth { get; set; } = 0.17m;

    // OSD (OpenShift Dedicated on GCP)
    public decimal OsdPerWorkerHour { get; set; } = 0.171m;

    // Helper methods
    public decimal GetMonthlyControlPlaneCost() => GkeControlPlanePerHour * 730;
    public decimal GetMonthlyInstanceCost(string type, int count) => type switch
    {
        "e2-standard-4" => E2Standard4PerHour * 730 * count,
        "e2-standard-8" => E2Standard8PerHour * 730 * count,
        "e2-standard-16" => E2Standard16PerHour * 730 * count,
        _ => E2Standard4PerHour * 730 * count
    };
}
