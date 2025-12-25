namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// On-premises pricing defaults stored in database
/// </summary>
public class OnPremPricingEntity
{
    public int Id { get; set; }

    // Hardware Costs
    public decimal ServerCostPerUnit { get; set; } = 5000m;
    public decimal CostPerCore { get; set; } = 100m;
    public decimal CostPerGBRam { get; set; } = 10m;
    public decimal StorageCostPerTB { get; set; } = 200m;
    public int HardwareLifespanYears { get; set; } = 5;

    // Data Center Costs
    public decimal RackUnitCostPerMonth { get; set; } = 50m;
    public decimal PowerCostPerKwhMonth { get; set; } = 0.10m;
    public decimal CoolingPUE { get; set; } = 1.5m;
    public decimal NetworkCostPerMonth { get; set; } = 500m;

    // Labor Costs
    public decimal DevOpsSalaryPerYear { get; set; } = 120000m;
    public decimal SysAdminSalaryPerYear { get; set; } = 90000m;
    public int NodesPerDevOpsEngineer { get; set; } = 50;
    public int NodesPerSysAdmin { get; set; } = 100;

    // Distribution Licensing (per node per year)
    public decimal OpenShiftPerNodeYear { get; set; } = 2500m;
    public decimal TanzuPerCoreYear { get; set; } = 1500m;
    public decimal RancherEnterprisePerNodeYear { get; set; } = 1000m;
    public decimal CharmedK8sPerNodeYear { get; set; } = 500m;
    public decimal RKE2PerNodeYear { get; set; } = 0m;
    public decimal K3sPerNodeYear { get; set; } = 0m;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
