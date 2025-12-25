namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// On-premises pricing configuration
/// </summary>
public class OnPremPricing
{
    /// <summary>
    /// Hardware costs
    /// </summary>
    public HardwareCosts Hardware { get; set; } = new();

    /// <summary>
    /// Data center costs
    /// </summary>
    public DataCenterCosts DataCenter { get; set; } = new();

    /// <summary>
    /// Labor costs
    /// </summary>
    public LaborCosts Labor { get; set; } = new();

    /// <summary>
    /// License costs per Kubernetes distribution
    /// </summary>
    public DistributionLicensing Licensing { get; set; } = new();

    /// <summary>
    /// Hardware refresh cycle in years (typically 3-5)
    /// </summary>
    public int HardwareRefreshYears { get; set; } = 4;

    /// <summary>
    /// Annual hardware maintenance percentage (of hardware cost)
    /// </summary>
    public decimal HardwareMaintenancePercent { get; set; } = 10;

    /// <summary>
    /// Calculate monthly hardware cost including amortization
    /// </summary>
    public decimal CalculateMonthlyHardwareCost(int servers)
    {
        var totalHardwareCost = servers * Hardware.ServerCost;
        var amortizedMonthly = totalHardwareCost / (HardwareRefreshYears * 12);
        var maintenanceMonthly = (totalHardwareCost * (HardwareMaintenancePercent / 100)) / 12;
        return amortizedMonthly + maintenanceMonthly;
    }
}

/// <summary>
/// Hardware costs for on-premises deployment
/// </summary>
public class HardwareCosts
{
    /// <summary>
    /// Average server cost (per physical server)
    /// </summary>
    public decimal ServerCost { get; set; } = 15000;

    /// <summary>
    /// Additional cost per CPU core
    /// </summary>
    public decimal PerCpuCore { get; set; } = 200;

    /// <summary>
    /// Additional cost per GB RAM
    /// </summary>
    public decimal PerGBRam { get; set; } = 15;

    /// <summary>
    /// Cost per TB storage (SSD)
    /// </summary>
    public decimal PerTBSsd { get; set; } = 200;

    /// <summary>
    /// Cost per TB storage (HDD)
    /// </summary>
    public decimal PerTBHdd { get; set; } = 50;

    /// <summary>
    /// Network switch cost
    /// </summary>
    public decimal NetworkSwitchCost { get; set; } = 5000;

    /// <summary>
    /// Load balancer hardware cost
    /// </summary>
    public decimal LoadBalancerCost { get; set; } = 10000;

    /// <summary>
    /// Number of VMs per physical server (consolidation ratio)
    /// </summary>
    public int VMsPerServer { get; set; } = 10;

    /// <summary>
    /// Calculate total hardware cost for a deployment
    /// </summary>
    public decimal CalculateTotalCost(int cpuCores, int ramGB, int storageTB, int loadBalancers = 0)
    {
        var servers = Math.Ceiling((decimal)cpuCores / 64); // Assuming 64 cores per server
        var serverCost = servers * ServerCost;
        var cpuCost = cpuCores * PerCpuCore;
        var ramCost = ramGB * PerGBRam;
        var storageCost = storageTB * PerTBSsd;
        var networkCost = NetworkSwitchCost * Math.Max(1, (int)Math.Ceiling(servers / 40));
        var lbCost = loadBalancers * LoadBalancerCost;

        return serverCost + cpuCost + ramCost + storageCost + networkCost + lbCost;
    }
}

/// <summary>
/// Data center costs
/// </summary>
public class DataCenterCosts
{
    /// <summary>
    /// Cost per rack unit per month (colocation)
    /// </summary>
    public decimal RackUnitPerMonth { get; set; } = 100;

    /// <summary>
    /// Power cost per kWh
    /// </summary>
    public decimal PowerPerKWh { get; set; } = 0.12m;

    /// <summary>
    /// Average watts per server
    /// </summary>
    public int WattsPerServer { get; set; } = 500;

    /// <summary>
    /// PUE (Power Usage Effectiveness) - typically 1.5-2.0
    /// </summary>
    public decimal PUE { get; set; } = 1.6m;

    /// <summary>
    /// Cooling cost as percentage of power cost
    /// </summary>
    public decimal CoolingPercent { get; set; } = 40;

    /// <summary>
    /// Calculate monthly data center cost
    /// </summary>
    public decimal CalculateMonthlyCost(int servers)
    {
        var rackUnits = servers * 2; // 2U per server average
        var rackCost = rackUnits * RackUnitPerMonth;

        var monthlyKWh = servers * WattsPerServer * 730 / 1000; // 730 hours/month
        var powerCost = monthlyKWh * PowerPerKWh * PUE;
        var coolingCost = powerCost * (CoolingPercent / 100);

        return rackCost + powerCost + coolingCost;
    }
}

/// <summary>
/// Labor/administration costs
/// </summary>
public class LaborCosts
{
    /// <summary>
    /// Monthly cost for a DevOps/SRE engineer
    /// </summary>
    public decimal DevOpsEngineerMonthly { get; set; } = 12000;

    /// <summary>
    /// Number of nodes one engineer can manage
    /// </summary>
    public int NodesPerEngineer { get; set; } = 50;

    /// <summary>
    /// Monthly cost for a system administrator
    /// </summary>
    public decimal SysAdminMonthly { get; set; } = 8000;

    /// <summary>
    /// Monthly cost for a DBA
    /// </summary>
    public decimal DBAMonthly { get; set; } = 10000;

    /// <summary>
    /// Include DBA costs for production environments
    /// </summary>
    public bool IncludeDBA { get; set; } = true;

    /// <summary>
    /// Calculate monthly labor cost based on node count
    /// </summary>
    public decimal CalculateMonthlyCost(int nodes, bool hasProd = true)
    {
        var engineersNeeded = Math.Max(1, (decimal)nodes / NodesPerEngineer);
        var devOpsCost = engineersNeeded * DevOpsEngineerMonthly;
        var sysAdminCost = Math.Max(1, engineersNeeded * 0.5m) * SysAdminMonthly;
        var dbaCost = IncludeDBA && hasProd ? DBAMonthly : 0;

        return devOpsCost + sysAdminCost + dbaCost;
    }
}

/// <summary>
/// License costs per Kubernetes distribution (per node per year)
/// </summary>
public class DistributionLicensing
{
    /// <summary>
    /// OpenShift subscription cost per node per year
    /// Red Hat OpenShift Container Platform subscription
    /// </summary>
    public decimal OpenShiftPerNodeYear { get; set; } = 2500m;

    /// <summary>
    /// VMware Tanzu license cost per core per year
    /// </summary>
    public decimal TanzuPerCoreYear { get; set; } = 1500m;

    /// <summary>
    /// Rancher Enterprise support cost per node per year
    /// (Rancher itself is open source, this is for SUSE support)
    /// </summary>
    public decimal RancherEnterprisePerNodeYear { get; set; } = 1000m;

    /// <summary>
    /// Canonical Charmed Kubernetes support per node per year
    /// </summary>
    public decimal CharmedK8sPerNodeYear { get; set; } = 500m;

    /// <summary>
    /// RKE2 - open source, no license cost
    /// </summary>
    public decimal RKE2PerNodeYear { get; set; } = 0m;

    /// <summary>
    /// K3s - open source, no license cost
    /// </summary>
    public decimal K3sPerNodeYear { get; set; } = 0m;

    /// <summary>
    /// MicroK8s - open source, no license cost (support available separately)
    /// </summary>
    public decimal MicroK8sPerNodeYear { get; set; } = 0m;

    /// <summary>
    /// Vanilla Kubernetes - open source, no license cost
    /// </summary>
    public decimal KubernetesPerNodeYear { get; set; } = 0m;

    /// <summary>
    /// Get license cost for a specific distribution
    /// </summary>
    /// <param name="distribution">The distribution name (case-insensitive)</param>
    /// <param name="nodeCount">Number of nodes</param>
    /// <param name="coreCount">Number of cores (for per-core licensing like Tanzu)</param>
    /// <returns>Annual license cost</returns>
    public decimal GetAnnualLicenseCost(string distribution, int nodeCount, int coreCount = 0)
    {
        var distUpper = distribution?.ToUpperInvariant() ?? "";

        return distUpper switch
        {
            "OPENSHIFT" => OpenShiftPerNodeYear * nodeCount,
            "TANZU" => TanzuPerCoreYear * (coreCount > 0 ? coreCount : nodeCount * 8), // Assume 8 cores/node if not specified
            "RANCHER" => RancherEnterprisePerNodeYear * nodeCount,
            "CHARMED" => CharmedK8sPerNodeYear * nodeCount,
            "RKE2" => RKE2PerNodeYear * nodeCount,
            "K3S" => K3sPerNodeYear * nodeCount,
            "MICROK8S" => MicroK8sPerNodeYear * nodeCount,
            "KUBERNETES" => KubernetesPerNodeYear * nodeCount,
            _ => 0m // Unknown distributions assumed free
        };
    }

    /// <summary>
    /// Get monthly license cost for a specific distribution
    /// </summary>
    public decimal GetMonthlyLicenseCost(string distribution, int nodeCount, int coreCount = 0)
    {
        return GetAnnualLicenseCost(distribution, nodeCount, coreCount) / 12;
    }

    /// <summary>
    /// Check if a distribution has licensing costs
    /// </summary>
    public bool HasLicenseCost(string distribution)
    {
        var distUpper = distribution?.ToUpperInvariant() ?? "";
        return distUpper switch
        {
            "OPENSHIFT" => OpenShiftPerNodeYear > 0,
            "TANZU" => TanzuPerCoreYear > 0,
            "RANCHER" => RancherEnterprisePerNodeYear > 0,
            "CHARMED" => CharmedK8sPerNodeYear > 0,
            _ => false
        };
    }
}
