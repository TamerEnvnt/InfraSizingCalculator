using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Cloud provider types for pricing
/// </summary>
public enum CloudProvider
{
    // Major Cloud Providers
    /// <summary>Amazon Web Services (EKS)</summary>
    AWS,

    /// <summary>Microsoft Azure (AKS)</summary>
    Azure,

    /// <summary>Google Cloud Platform (GKE)</summary>
    GCP,

    /// <summary>Oracle Cloud Infrastructure (OKE)</summary>
    OCI,

    /// <summary>IBM Cloud (IKS)</summary>
    IBM,

    /// <summary>Alibaba Cloud (ACK)</summary>
    Alibaba,

    /// <summary>Tencent Cloud (TKE)</summary>
    Tencent,

    /// <summary>Huawei Cloud (CCE)</summary>
    Huawei,

    // Managed OpenShift Services
    /// <summary>Red Hat OpenShift Service on AWS (ROSA)</summary>
    ROSA,

    /// <summary>Azure Red Hat OpenShift (ARO)</summary>
    ARO,

    /// <summary>OpenShift Dedicated on Google Cloud (OSD)</summary>
    OSD,

    /// <summary>Red Hat OpenShift on IBM Cloud (ROKS)</summary>
    ROKS,

    // Smaller/Developer Cloud Providers
    /// <summary>DigitalOcean (DOKS)</summary>
    DigitalOcean,

    /// <summary>Linode (Akamai) (LKE)</summary>
    Linode,

    /// <summary>Vultr (VKE)</summary>
    Vultr,

    /// <summary>Hetzner Cloud</summary>
    Hetzner,

    /// <summary>OVHcloud</summary>
    OVH,

    /// <summary>Scaleway (Kapsule)</summary>
    Scaleway,

    /// <summary>Civo Cloud</summary>
    Civo,

    /// <summary>Exoscale (SKS)</summary>
    Exoscale,

    // On-premises/Manual
    /// <summary>On-premises deployment</summary>
    OnPrem,

    /// <summary>Manual/custom pricing</summary>
    Manual
}

/// <summary>
/// Pricing model type
/// </summary>
public enum PricingType
{
    /// <summary>Pay-as-you-go hourly pricing</summary>
    OnDemand,

    /// <summary>1-year reserved/committed use</summary>
    Reserved1Year,

    /// <summary>3-year reserved/committed use</summary>
    Reserved3Year,

    /// <summary>Spot/preemptible instances</summary>
    Spot
}

/// <summary>
/// Currency for pricing
/// </summary>
public enum Currency
{
    USD,
    EUR,
    GBP,
    AUD,
    CAD,
    JPY
}

/// <summary>
/// Cost category for breakdown
/// </summary>
public enum CostCategory
{
    /// <summary>Compute resources (CPU, RAM)</summary>
    Compute,

    /// <summary>Storage (disk, object storage)</summary>
    Storage,

    /// <summary>Network (egress, load balancer)</summary>
    Network,

    /// <summary>Software licenses (OpenShift, etc.)</summary>
    License,

    /// <summary>Support contracts</summary>
    Support,

    /// <summary>Data center costs (power, cooling, space)</summary>
    DataCenter,

    /// <summary>Labor/administration costs</summary>
    Labor
}

/// <summary>
/// Extension methods for CloudProvider with pricing calculations
/// </summary>
public static class CloudProviderExtensions
{
    /// <summary>
    /// Maps a K8s distribution to its underlying cloud provider.
    /// </summary>
    public static CloudProvider GetCloudProvider(this Distribution distribution)
    {
        return distribution switch
        {
            // On-Premises distributions
            Distribution.OpenShift => CloudProvider.OnPrem,
            Distribution.Kubernetes => CloudProvider.OnPrem,
            Distribution.Rancher => CloudProvider.OnPrem,
            Distribution.RKE2 => CloudProvider.OnPrem,
            Distribution.K3s => CloudProvider.OnPrem,
            Distribution.MicroK8s => CloudProvider.OnPrem,
            Distribution.Charmed => CloudProvider.OnPrem,
            Distribution.Tanzu => CloudProvider.OnPrem,

            // AWS distributions
            Distribution.EKS => CloudProvider.AWS,
            Distribution.OpenShiftROSA => CloudProvider.ROSA,
            Distribution.RancherEKS => CloudProvider.AWS,
            Distribution.TanzuAWS => CloudProvider.AWS,
            Distribution.CharmedAWS => CloudProvider.AWS,
            Distribution.MicroK8sAWS => CloudProvider.AWS,
            Distribution.K3sAWS => CloudProvider.AWS,
            Distribution.RKE2AWS => CloudProvider.AWS,

            // Azure distributions
            Distribution.AKS => CloudProvider.Azure,
            Distribution.OpenShiftARO => CloudProvider.ARO,
            Distribution.RancherAKS => CloudProvider.Azure,
            Distribution.TanzuAzure => CloudProvider.Azure,
            Distribution.CharmedAzure => CloudProvider.Azure,
            Distribution.MicroK8sAzure => CloudProvider.Azure,
            Distribution.K3sAzure => CloudProvider.Azure,
            Distribution.RKE2Azure => CloudProvider.Azure,

            // GCP distributions
            Distribution.GKE => CloudProvider.GCP,
            Distribution.OpenShiftDedicated => CloudProvider.OSD,
            Distribution.RancherGKE => CloudProvider.GCP,
            Distribution.TanzuGCP => CloudProvider.GCP,
            Distribution.CharmedGCP => CloudProvider.GCP,
            Distribution.MicroK8sGCP => CloudProvider.GCP,
            Distribution.K3sGCP => CloudProvider.GCP,
            Distribution.RKE2GCP => CloudProvider.GCP,

            // Other major clouds
            Distribution.OKE => CloudProvider.OCI,
            Distribution.IKS => CloudProvider.IBM,
            Distribution.OpenShiftIBM => CloudProvider.ROKS,
            Distribution.ACK => CloudProvider.Alibaba,
            Distribution.TKE => CloudProvider.Tencent,
            Distribution.CCE => CloudProvider.Huawei,
            Distribution.DOKS => CloudProvider.DigitalOcean,
            Distribution.LKE => CloudProvider.Linode,

            // Generic/multi-cloud
            Distribution.RancherHosted => CloudProvider.Manual,
            Distribution.TanzuCloud => CloudProvider.Manual,
            Distribution.VKE => CloudProvider.Vultr,
            Distribution.HetznerK8s => CloudProvider.Hetzner,
            Distribution.OVHKubernetes => CloudProvider.OVH,
            Distribution.ScalewayKapsule => CloudProvider.Scaleway,

            _ => CloudProvider.Manual
        };
    }

    /// <summary>
    /// Gets the cross-AZ data transfer cost adjustment for a cloud provider.
    ///
    /// PRICING SOURCES (Dec 2024):
    /// - AWS: $0.01/GB per direction, typical workload adds ~2-3% cost
    /// - Azure: FREE within region (huge cost advantage)
    /// - GCP: $0.01/GB egress, similar to AWS
    /// - On-Prem: No cloud charges, but may have network costs
    /// - Others: Generally similar to AWS/GCP
    /// </summary>
    public static decimal GetCrossAZCostMultiplier(this CloudProvider provider, int azCount)
    {
        if (azCount <= 1) return 0m;

        return provider switch
        {
            // Azure: FREE cross-AZ within region - major cost advantage
            CloudProvider.Azure or CloudProvider.ARO => 0m,

            // On-Prem: No cloud data transfer costs
            CloudProvider.OnPrem => 0m,

            // AWS: $0.01/GB cross-AZ each direction
            // Typical multi-AZ workload: 2-3% additional cost
            CloudProvider.AWS or CloudProvider.ROSA => azCount == 2 ? 0.02m : 0.03m,

            // GCP: $0.01/GB egress between zones
            CloudProvider.GCP or CloudProvider.OSD => azCount == 2 ? 0.02m : 0.03m,

            // Oracle: Generally competitive with AWS
            CloudProvider.OCI => azCount == 2 ? 0.015m : 0.025m,

            // IBM Cloud ROKS
            CloudProvider.IBM or CloudProvider.ROKS => azCount == 2 ? 0.02m : 0.03m,

            // Other clouds: Conservative estimate similar to AWS
            _ => azCount == 2 ? 0.02m : 0.03m
        };
    }

    /// <summary>
    /// Gets the control plane (managed K8s) cost per hour.
    ///
    /// PRICING SOURCES (Dec 2024):
    /// - AWS EKS: $0.10/hr standard, $0.60/hr extended support
    /// - Azure AKS: Free tier, $0.10/hr Standard, $0.60/hr Premium
    /// - GKE: $0.10/hr Autopilot, free first cluster
    /// - Oracle OKE: FREE basic, $0.10/hr enhanced (capped at $74.40/mo)
    /// - DigitalOcean: FREE standard, $40/mo HA
    /// - Linode: FREE standard, $60/mo HA
    /// </summary>
    public static decimal GetControlPlaneCostPerHour(this CloudProvider provider, bool isHA = false)
    {
        return provider switch
        {
            CloudProvider.AWS or CloudProvider.ROSA => 0.10m,     // Always $0.10/hr
            CloudProvider.Azure or CloudProvider.ARO => isHA ? 0.10m : 0m,  // Free tier available
            CloudProvider.GCP or CloudProvider.OSD => 0.10m,      // $0.10/hr
            CloudProvider.OCI => isHA ? 0.10m : 0m,               // Free basic
            CloudProvider.IBM or CloudProvider.ROKS => 0m,        // Free control plane
            CloudProvider.DigitalOcean => isHA ? 0.055m : 0m,     // ~$40/mo HA
            CloudProvider.Linode => isHA ? 0.083m : 0m,           // ~$60/mo HA
            CloudProvider.OnPrem => 0m,                            // Self-managed
            _ => 0.10m                                             // Default to AWS pricing
        };
    }
}
