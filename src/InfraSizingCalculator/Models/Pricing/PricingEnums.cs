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
