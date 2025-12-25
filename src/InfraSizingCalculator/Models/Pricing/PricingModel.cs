namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Complete pricing model for cost estimation
/// </summary>
public class PricingModel
{
    /// <summary>
    /// Cloud provider
    /// </summary>
    public CloudProvider Provider { get; set; }

    /// <summary>
    /// Region/location for pricing
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the region
    /// </summary>
    public string RegionDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Currency for all prices
    /// </summary>
    public Currency Currency { get; set; } = Currency.USD;

    /// <summary>
    /// Pricing type (on-demand, reserved, etc.)
    /// </summary>
    public PricingType PricingType { get; set; } = PricingType.OnDemand;

    /// <summary>
    /// Compute pricing (CPU, RAM, instances)
    /// </summary>
    public ComputePricing Compute { get; set; } = new();

    /// <summary>
    /// Storage pricing
    /// </summary>
    public StoragePricing Storage { get; set; } = new();

    /// <summary>
    /// Network pricing
    /// </summary>
    public NetworkPricing Network { get; set; } = new();

    /// <summary>
    /// License pricing
    /// </summary>
    public LicensePricing Licenses { get; set; } = new();

    /// <summary>
    /// Support pricing
    /// </summary>
    public SupportPricing Support { get; set; } = new();

    /// <summary>
    /// When prices were last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this pricing is from live API or cached/default
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Source of pricing data
    /// </summary>
    public string Source { get; set; } = "Default";
}

/// <summary>
/// Compute resource pricing
/// </summary>
public class ComputePricing
{
    /// <summary>
    /// Price per vCPU per hour
    /// </summary>
    public decimal CpuPerHour { get; set; }

    /// <summary>
    /// Price per GB RAM per hour
    /// </summary>
    public decimal RamGBPerHour { get; set; }

    /// <summary>
    /// Instance type prices (key: instance type, value: hourly price)
    /// </summary>
    public Dictionary<string, decimal> InstanceTypePrices { get; set; } = new();

    /// <summary>
    /// Managed K8s control plane cost per hour (EKS, AKS, GKE)
    /// </summary>
    public decimal ManagedControlPlanePerHour { get; set; }

    /// <summary>
    /// OpenShift service fee per worker node per hour (ROSA, ARO, OSD)
    /// This is charged on top of the underlying cloud instance costs
    /// </summary>
    public decimal OpenShiftServiceFeePerWorkerHour { get; set; }

    /// <summary>
    /// Hours per month for calculation (730 = average month)
    /// </summary>
    public const int HoursPerMonth = 730;

    /// <summary>
    /// Calculate hourly cost for given resources
    /// </summary>
    public decimal CalculateHourlyCost(int cpuCores, int ramGB)
    {
        return (cpuCores * CpuPerHour) + (ramGB * RamGBPerHour);
    }

    /// <summary>
    /// Calculate monthly cost for given resources
    /// </summary>
    public decimal CalculateMonthlyCost(int cpuCores, int ramGB)
    {
        return CalculateHourlyCost(cpuCores, ramGB) * HoursPerMonth;
    }
}

/// <summary>
/// Storage pricing
/// </summary>
public class StoragePricing
{
    /// <summary>
    /// Price per GB of SSD storage per month
    /// </summary>
    public decimal SsdPerGBMonth { get; set; }

    /// <summary>
    /// Price per GB of HDD storage per month
    /// </summary>
    public decimal HddPerGBMonth { get; set; }

    /// <summary>
    /// Price per GB of object storage per month
    /// </summary>
    public decimal ObjectStoragePerGBMonth { get; set; }

    /// <summary>
    /// Price per GB for backup storage per month
    /// </summary>
    public decimal BackupPerGBMonth { get; set; }

    /// <summary>
    /// Container registry storage per GB per month
    /// </summary>
    public decimal RegistryPerGBMonth { get; set; }

    /// <summary>
    /// Calculate monthly storage cost
    /// </summary>
    public decimal CalculateMonthlyCost(int ssdGB, int hddGB = 0, int objectGB = 0, int backupGB = 0)
    {
        return (ssdGB * SsdPerGBMonth) +
               (hddGB * HddPerGBMonth) +
               (objectGB * ObjectStoragePerGBMonth) +
               (backupGB * BackupPerGBMonth);
    }
}

/// <summary>
/// Network pricing
/// </summary>
public class NetworkPricing
{
    /// <summary>
    /// Price per GB of egress data transfer
    /// </summary>
    public decimal EgressPerGB { get; set; }

    /// <summary>
    /// Load balancer per hour
    /// </summary>
    public decimal LoadBalancerPerHour { get; set; }

    /// <summary>
    /// NAT Gateway per hour
    /// </summary>
    public decimal NatGatewayPerHour { get; set; }

    /// <summary>
    /// VPN connection per hour
    /// </summary>
    public decimal VpnPerHour { get; set; }

    /// <summary>
    /// Public IP per hour
    /// </summary>
    public decimal PublicIpPerHour { get; set; }

    /// <summary>
    /// Ingress data transfer is usually free
    /// </summary>
    public decimal IngressPerGB { get; set; } = 0;

    /// <summary>
    /// Calculate monthly network cost
    /// </summary>
    public decimal CalculateMonthlyCost(int loadBalancers, decimal egressGB, int publicIps = 0)
    {
        return (loadBalancers * LoadBalancerPerHour * ComputePricing.HoursPerMonth) +
               (egressGB * EgressPerGB) +
               (publicIps * PublicIpPerHour * ComputePricing.HoursPerMonth);
    }
}

/// <summary>
/// License pricing for K8s distributions and software
/// </summary>
public class LicensePricing
{
    /// <summary>
    /// OpenShift subscription per node per year
    /// </summary>
    public decimal OpenShiftPerNodeYear { get; set; }

    /// <summary>
    /// Rancher Enterprise per node per year
    /// </summary>
    public decimal RancherEnterprisePerNodeYear { get; set; }

    /// <summary>
    /// VMware Tanzu per core per year
    /// </summary>
    public decimal TanzuPerCoreYear { get; set; }

    /// <summary>
    /// Canonical Charmed K8s per node per year
    /// </summary>
    public decimal CharmedK8sPerNodeYear { get; set; }

    /// <summary>
    /// Custom license costs (key: license name, value: annual cost)
    /// </summary>
    public Dictionary<string, decimal> CustomLicenses { get; set; } = new();

    /// <summary>
    /// Get license cost per node per year for a distribution
    /// </summary>
    public decimal GetLicensePerNodeYear(Enums.Distribution distribution)
    {
        return distribution switch
        {
            Enums.Distribution.OpenShift => OpenShiftPerNodeYear,
            Enums.Distribution.Rancher => RancherEnterprisePerNodeYear,
            Enums.Distribution.Charmed => CharmedK8sPerNodeYear,
            Enums.Distribution.Tanzu => TanzuPerCoreYear, // Note: Tanzu is per core, approximating per node
            // Managed services don't have per-node licenses
            Enums.Distribution.EKS or Enums.Distribution.AKS or
            Enums.Distribution.GKE or Enums.Distribution.OKE => 0,
            // Open source distributions
            _ => 0
        };
    }

    /// <summary>
    /// Get license cost per node per year for a distribution by name
    /// </summary>
    public decimal GetLicensePerNodeYear(string distributionName)
    {
        var name = distributionName.ToLowerInvariant();
        return name switch
        {
            "openshift" or "red hat openshift" => OpenShiftPerNodeYear,
            "rancher" or "rancher enterprise" or "rke2" => RancherEnterprisePerNodeYear,
            "charmed" or "charmed kubernetes" or "charmed k8s" => CharmedK8sPerNodeYear,
            "tanzu" or "vmware tanzu" => TanzuPerCoreYear,
            _ => 0
        };
    }
}

/// <summary>
/// Support pricing
/// </summary>
public class SupportPricing
{
    /// <summary>
    /// Basic support percentage of total (typically 0%)
    /// </summary>
    public decimal BasicSupportPercent { get; set; } = 0;

    /// <summary>
    /// Developer support percentage of total
    /// </summary>
    public decimal DeveloperSupportPercent { get; set; } = 3;

    /// <summary>
    /// Business support percentage of total
    /// </summary>
    public decimal BusinessSupportPercent { get; set; } = 10;

    /// <summary>
    /// Enterprise support percentage of total
    /// </summary>
    public decimal EnterpriseSupportPercent { get; set; } = 15;

    /// <summary>
    /// Get support cost based on base cost and support level
    /// </summary>
    public decimal GetSupportCost(decimal baseCost, SupportLevel level)
    {
        var percent = level switch
        {
            SupportLevel.Basic => BasicSupportPercent,
            SupportLevel.Developer => DeveloperSupportPercent,
            SupportLevel.Business => BusinessSupportPercent,
            SupportLevel.Enterprise => EnterpriseSupportPercent,
            _ => 0
        };
        return baseCost * (percent / 100);
    }
}

/// <summary>
/// Support level options
/// </summary>
public enum SupportLevel
{
    None,
    Basic,
    Developer,
    Business,
    Enterprise
}
