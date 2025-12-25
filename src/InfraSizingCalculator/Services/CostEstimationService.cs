using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for estimating infrastructure costs
/// </summary>
public class CostEstimationService : ICostEstimationService
{
    private readonly IPricingService _pricingService;
    private const int HoursPerMonth = 730;
    private const int MonthsPerYear = 12;

    public CostEstimationService(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    /// <inheritdoc />
    public async Task<CostEstimate> EstimateK8sCostAsync(
        K8sSizingResult sizing,
        CloudProvider provider,
        string region,
        CostEstimationOptions? options = null)
    {
        options ??= new CostEstimationOptions();
        var pricing = await _pricingService.GetPricingAsync(provider, region, options.PricingType);

        return CalculateK8sCost(sizing, pricing, options);
    }

    /// <inheritdoc />
    public async Task<CostEstimate> EstimateVMCostAsync(
        VMSizingResult sizing,
        CloudProvider provider,
        string region,
        CostEstimationOptions? options = null)
    {
        options ??= new CostEstimationOptions();
        var pricing = await _pricingService.GetPricingAsync(provider, region, options.PricingType);

        return CalculateVMCost(sizing, pricing, options);
    }

    /// <inheritdoc />
    public CostEstimate EstimateOnPremCost(
        K8sSizingResult sizing,
        OnPremPricing onPremPricing,
        CostEstimationOptions? options = null)
    {
        options ??= new CostEstimationOptions();
        var estimate = new CostEstimate
        {
            Provider = CloudProvider.OnPrem,
            Region = "On-Premises",
            Currency = options.Currency,
            PricingType = PricingType.OnDemand,
            CalculatedAt = DateTime.UtcNow,
            PricingSource = "On-Premises Configuration"
        };

        var totalCpu = sizing.GrandTotal.TotalCpu;
        var totalRam = sizing.GrandTotal.TotalRam;
        var totalDisk = sizing.GrandTotal.TotalDisk;
        var totalNodes = sizing.GrandTotal.TotalNodes;

        var serverCount = (int)Math.Ceiling((double)totalCpu / 64);

        // Hardware costs (amortized)
        var hardwareCost = onPremPricing.CalculateMonthlyHardwareCost(serverCount);
        var cpuCost = totalCpu * onPremPricing.Hardware.PerCpuCore / (onPremPricing.HardwareRefreshYears * MonthsPerYear);
        var ramCost = totalRam * onPremPricing.Hardware.PerGBRam / (onPremPricing.HardwareRefreshYears * MonthsPerYear);
        var computeMonthly = hardwareCost + cpuCost + ramCost;

        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown
        {
            Category = CostCategory.Compute,
            Monthly = computeMonthly,
            Description = "Compute hardware (amortized)",
            LineItems = new List<CostLineItem>
            {
                new() { Description = "Server Hardware", Quantity = serverCount, UnitPrice = hardwareCost / serverCount, Unit = "servers" },
                new() { Description = "CPU Cores", Quantity = totalCpu, UnitPrice = cpuCost / totalCpu, Unit = "cores" },
                new() { Description = "RAM", Quantity = totalRam, UnitPrice = ramCost / totalRam, Unit = "GB" }
            }
        };

        // Storage
        if (options.IncludeStorage && totalDisk > 0)
        {
            var storageCost = (totalDisk / 1000m) * onPremPricing.Hardware.PerTBSsd / (onPremPricing.HardwareRefreshYears * MonthsPerYear);
            estimate.Breakdown[CostCategory.Storage] = new CostBreakdown
            {
                Category = CostCategory.Storage,
                Monthly = storageCost,
                Description = "Storage hardware (amortized)",
                LineItems = new List<CostLineItem>
                {
                    new() { Description = "SSD Storage", Quantity = totalDisk, UnitPrice = storageCost / totalDisk, Unit = "GB" }
                }
            };
        }

        // Data center costs
        var dataCenterCost = onPremPricing.DataCenter.CalculateMonthlyCost(serverCount);
        estimate.Breakdown[CostCategory.DataCenter] = new CostBreakdown
        {
            Category = CostCategory.DataCenter,
            Monthly = dataCenterCost,
            Description = "Data center operations",
            LineItems = new List<CostLineItem>
            {
                new() { Description = "Power & Cooling", Quantity = serverCount, UnitPrice = dataCenterCost / serverCount, Unit = "servers" }
            }
        };

        // Labor costs
        var laborCost = onPremPricing.Labor.CalculateMonthlyCost(totalNodes);
        estimate.Breakdown[CostCategory.Labor] = new CostBreakdown
        {
            Category = CostCategory.Labor,
            Monthly = laborCost,
            Description = "Operations staff",
            LineItems = new List<CostLineItem>
            {
                new() { Description = "DevOps/SRE", Quantity = (decimal)Math.Ceiling((double)totalNodes / onPremPricing.Labor.NodesPerEngineer), UnitPrice = onPremPricing.Labor.DevOpsEngineerMonthly, Unit = "FTE" }
            }
        };

        // License costs
        if (options.IncludeLicenses && !string.IsNullOrEmpty(options.Distribution))
        {
            var licenseCost = CalculateLicenseCost(options.Distribution, totalNodes);
            if (licenseCost > 0)
            {
                estimate.Breakdown[CostCategory.License] = new CostBreakdown
                {
                    Category = CostCategory.License,
                    Monthly = licenseCost / MonthsPerYear,
                    Description = $"{options.Distribution} License",
                    LineItems = new List<CostLineItem>
                    {
                        new() { Description = $"{options.Distribution} License", Quantity = totalNodes, UnitPrice = licenseCost / MonthsPerYear / totalNodes, Unit = "nodes" }
                    }
                };
            }
        }

        // Calculate per-environment costs
        foreach (var env in sizing.Environments)
        {
            var envRatio = totalNodes > 0 ? (decimal)env.TotalNodes / totalNodes : 0;
            var envMonthly = estimate.Breakdown.Values.Sum(b => b.Monthly) * envRatio;
            estimate.EnvironmentCosts[env.Environment] = new EnvironmentCost
            {
                Environment = env.Environment,
                EnvironmentName = env.EnvironmentName,
                MonthlyCost = envMonthly,
                Nodes = env.TotalNodes,
                TotalCpu = env.TotalCpu,
                TotalRamGB = env.TotalRam,
                TotalDiskGB = env.TotalDisk
            };
        }

        CalculateTotals(estimate);
        return estimate;
    }

    /// <inheritdoc />
    public CostEstimate EstimateOnPremVMCost(
        VMSizingResult sizing,
        OnPremPricing onPremPricing,
        CostEstimationOptions? options = null)
    {
        options ??= new CostEstimationOptions();
        var estimate = new CostEstimate
        {
            Provider = CloudProvider.OnPrem,
            Region = "On-Premises",
            Currency = options.Currency,
            PricingType = PricingType.OnDemand,
            CalculatedAt = DateTime.UtcNow,
            PricingSource = "On-Premises Configuration"
        };

        var totalCpu = sizing.GrandTotal.TotalCpu;
        var totalRam = sizing.GrandTotal.TotalRam;
        var totalDisk = sizing.GrandTotal.TotalDisk;
        var totalVMs = sizing.GrandTotal.TotalVMs;

        var serverCount = (int)Math.Ceiling((double)totalCpu / 64);

        // Hardware costs (amortized)
        var hardwareCost = onPremPricing.CalculateMonthlyHardwareCost(serverCount);
        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown
        {
            Category = CostCategory.Compute,
            Monthly = hardwareCost,
            Description = "Compute hardware (amortized)",
            LineItems = new List<CostLineItem>
            {
                new() { Description = "Server Hardware", Quantity = serverCount, UnitPrice = hardwareCost / serverCount, Unit = "servers" }
            }
        };

        // Storage
        if (options.IncludeStorage && totalDisk > 0)
        {
            var storageCost = (totalDisk / 1000m) * onPremPricing.Hardware.PerTBSsd / (onPremPricing.HardwareRefreshYears * MonthsPerYear);
            estimate.Breakdown[CostCategory.Storage] = new CostBreakdown
            {
                Category = CostCategory.Storage,
                Monthly = storageCost,
                Description = "Storage hardware (amortized)",
                LineItems = new List<CostLineItem>
                {
                    new() { Description = "SSD Storage", Quantity = totalDisk, UnitPrice = storageCost / totalDisk, Unit = "GB" }
                }
            };
        }

        // Data center costs
        var dataCenterCost = onPremPricing.DataCenter.CalculateMonthlyCost(serverCount);
        estimate.Breakdown[CostCategory.DataCenter] = new CostBreakdown
        {
            Category = CostCategory.DataCenter,
            Monthly = dataCenterCost,
            Description = "Data center operations"
        };

        // Labor costs
        var laborCost = onPremPricing.Labor.CalculateMonthlyCost(totalVMs);
        estimate.Breakdown[CostCategory.Labor] = new CostBreakdown
        {
            Category = CostCategory.Labor,
            Monthly = laborCost,
            Description = "Operations staff"
        };

        // Calculate per-environment costs
        var totalResources = totalCpu + totalRam;
        foreach (var env in sizing.Environments)
        {
            var envResources = env.TotalCpu + env.TotalRam;
            var envRatio = totalResources > 0 ? (decimal)envResources / totalResources : 0;
            var envMonthly = estimate.Breakdown.Values.Sum(b => b.Monthly) * envRatio;
            estimate.EnvironmentCosts[env.Environment] = new EnvironmentCost
            {
                Environment = env.Environment,
                EnvironmentName = env.EnvironmentName,
                MonthlyCost = envMonthly,
                Nodes = env.TotalVMs,
                TotalCpu = env.TotalCpu,
                TotalRamGB = env.TotalRam,
                TotalDiskGB = env.TotalDisk
            };
        }

        CalculateTotals(estimate);
        return estimate;
    }

    /// <inheritdoc />
    public CostComparison Compare(params CostEstimate[] estimates)
    {
        var comparison = new CostComparison { ComparedAt = DateTime.UtcNow };

        if (estimates.Length == 0)
            return comparison;

        comparison.Estimates.AddRange(estimates);

        // Find cheapest and most expensive
        var ordered = estimates.OrderBy(e => e.MonthlyTotal).ToList();
        comparison.CheapestOption = ordered.First();
        comparison.MostExpensiveOption = ordered.Last();

        // Calculate potential savings
        foreach (var estimate in estimates)
        {
            if (estimate != comparison.CheapestOption)
            {
                var savings = estimate.MonthlyTotal - comparison.CheapestOption.MonthlyTotal;
                comparison.PotentialSavings[$"{estimate.Provider}-{estimate.Region}"] = savings;
            }
        }

        // Generate insights
        if (comparison.MostExpensiveOption.MonthlyTotal > 0)
        {
            var savingsPercent = ((comparison.MostExpensiveOption.MonthlyTotal - comparison.CheapestOption.MonthlyTotal) /
                                  comparison.MostExpensiveOption.MonthlyTotal) * 100;

            if (savingsPercent > 30)
            {
                comparison.Insights.Add($"Significant cost difference ({savingsPercent:F0}%) between providers");
            }
        }

        var awsEstimate = estimates.FirstOrDefault(e => e.Provider == CloudProvider.AWS);
        var ociEstimate = estimates.FirstOrDefault(e => e.Provider == CloudProvider.OCI);
        if (awsEstimate != null && ociEstimate != null && ociEstimate.MonthlyTotal < awsEstimate.MonthlyTotal)
        {
            var savings = ((awsEstimate.MonthlyTotal - ociEstimate.MonthlyTotal) / awsEstimate.MonthlyTotal) * 100;
            comparison.Insights.Add($"OCI is {savings:F0}% cheaper than AWS (typically lower egress costs)");
        }

        var onPremEstimate = estimates.FirstOrDefault(e => e.Provider == CloudProvider.OnPrem);
        if (onPremEstimate != null && estimates.Length > 1)
        {
            var cloudEstimates = estimates.Where(e => e.Provider != CloudProvider.OnPrem).ToList();
            if (cloudEstimates.Any())
            {
                var cloudAvgTCO = cloudEstimates.Average(e => e.ThreeYearTCO);
                if (onPremEstimate.ThreeYearTCO < cloudAvgTCO)
                {
                    comparison.Insights.Add("On-premises may be more cost-effective for long-term deployments");
                }
                else
                {
                    comparison.Insights.Add("Cloud may be more cost-effective due to elasticity and reduced operational overhead");
                }
            }
        }

        return comparison;
    }

    /// <inheritdoc />
    public decimal CalculateTCO(CostEstimate estimate, int years)
    {
        return estimate.YearlyTotal * years;
    }

    private CostEstimate CalculateK8sCost(K8sSizingResult sizing, PricingModel pricing, CostEstimationOptions options)
    {
        var estimate = new CostEstimate
        {
            Provider = pricing.Provider,
            Region = pricing.Region,
            Currency = pricing.Currency,
            PricingType = pricing.PricingType,
            CalculatedAt = DateTime.UtcNow,
            PricingSource = pricing.Source
        };

        var totalNodes = sizing.GrandTotal.TotalNodes;
        var totalCpu = sizing.GrandTotal.TotalCpu;
        var totalRam = sizing.GrandTotal.TotalRam;
        var totalDisk = sizing.GrandTotal.TotalDisk;

        // Apply headroom if specified
        if (options.HeadroomPercent > 0)
        {
            var multiplier = 1 + (options.HeadroomPercent / 100.0m);
            totalCpu = (int)(totalCpu * multiplier);
            totalRam = (int)(totalRam * multiplier);
        }

        // Compute costs
        var cpuHourlyCost = totalCpu * pricing.Compute.CpuPerHour;
        var ramHourlyCost = totalRam * pricing.Compute.RamGBPerHour;
        var computeHourly = cpuHourlyCost + ramHourlyCost;
        var computeMonthly = computeHourly * HoursPerMonth;

        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown
        {
            Category = CostCategory.Compute,
            Monthly = computeMonthly,
            Description = "Compute resources",
            LineItems = new List<CostLineItem>
            {
                new() { Description = "vCPU", Quantity = totalCpu, UnitPrice = cpuHourlyCost * HoursPerMonth / totalCpu, Unit = "cores" },
                new() { Description = "RAM", Quantity = totalRam, UnitPrice = ramHourlyCost * HoursPerMonth / totalRam, Unit = "GB" }
            }
        };

        // Managed control plane (EKS, AKS, GKE)
        if (options.IncludeManagedControlPlane && pricing.Compute.ManagedControlPlanePerHour > 0)
        {
            var clusterCount = sizing.Environments.Count;
            var controlPlaneMonthly = pricing.Compute.ManagedControlPlanePerHour * HoursPerMonth * clusterCount;

            // Add to compute breakdown
            var existingCompute = estimate.Breakdown[CostCategory.Compute];
            existingCompute.Monthly += controlPlaneMonthly;
            existingCompute.LineItems.Add(new CostLineItem
            {
                Description = "Managed Control Plane",
                Quantity = clusterCount,
                UnitPrice = controlPlaneMonthly / clusterCount,
                Unit = "clusters"
            });
        }

        // Storage costs
        if (options.IncludeStorage)
        {
            var ssdStorageGB = totalDisk > 0 ? totalDisk : totalNodes * options.StorageGBPerNode;
            var storageMonthly = ssdStorageGB * pricing.Storage.SsdPerGBMonth;
            var registryStorageMonthly = 50 * pricing.Storage.RegistryPerGBMonth;

            estimate.Breakdown[CostCategory.Storage] = new CostBreakdown
            {
                Category = CostCategory.Storage,
                Monthly = storageMonthly + registryStorageMonthly,
                Description = "Storage",
                LineItems = new List<CostLineItem>
                {
                    new() { Description = "Block Storage (SSD)", Quantity = ssdStorageGB, UnitPrice = pricing.Storage.SsdPerGBMonth, Unit = "GB" },
                    new() { Description = "Container Registry", Quantity = 50, UnitPrice = pricing.Storage.RegistryPerGBMonth, Unit = "GB" }
                }
            };
        }

        // Network costs
        if (options.IncludeNetwork)
        {
            var egressMonthly = options.MonthlyEgressGB * pricing.Network.EgressPerGB;
            var lbMonthly = options.LoadBalancers * pricing.Network.LoadBalancerPerHour * HoursPerMonth;
            var natMonthly = pricing.Network.NatGatewayPerHour * HoursPerMonth;

            estimate.Breakdown[CostCategory.Network] = new CostBreakdown
            {
                Category = CostCategory.Network,
                Monthly = egressMonthly + lbMonthly + natMonthly,
                Description = "Network",
                LineItems = new List<CostLineItem>
                {
                    new() { Description = "Data Egress", Quantity = options.MonthlyEgressGB, UnitPrice = pricing.Network.EgressPerGB, Unit = "GB" },
                    new() { Description = "Load Balancers", Quantity = options.LoadBalancers, UnitPrice = lbMonthly / options.LoadBalancers, Unit = "LBs" },
                    new() { Description = "NAT Gateway", Quantity = 1, UnitPrice = natMonthly, Unit = "gateway" }
                }
            };
        }

        // License costs
        if (options.IncludeLicenses && !string.IsNullOrEmpty(options.Distribution))
        {
            var licensePerNode = pricing.Licenses.GetLicensePerNodeYear(options.Distribution);
            if (licensePerNode > 0)
            {
                var licenseCostYearly = licensePerNode * totalNodes;
                var licenseCostMonthly = licenseCostYearly / MonthsPerYear;

                estimate.Breakdown[CostCategory.License] = new CostBreakdown
                {
                    Category = CostCategory.License,
                    Monthly = licenseCostMonthly,
                    Description = $"{options.Distribution} License",
                    LineItems = new List<CostLineItem>
                    {
                        new() { Description = $"{options.Distribution} License", Quantity = totalNodes, UnitPrice = licenseCostMonthly / totalNodes, Unit = "nodes" }
                    }
                };
            }
        }

        // Support costs
        if (options.IncludeSupport && options.SupportTier != SupportTier.None)
        {
            var baseMonthly = estimate.Breakdown.Values.Sum(b => b.Monthly);
            var supportPercent = GetSupportPercent(pricing.Support, options.SupportTier);
            var supportMonthly = baseMonthly * (supportPercent / 100);

            if (supportMonthly > 0)
            {
                estimate.Breakdown[CostCategory.Support] = new CostBreakdown
                {
                    Category = CostCategory.Support,
                    Monthly = supportMonthly,
                    Description = $"{options.SupportTier} Support ({supportPercent}%)",
                    LineItems = new List<CostLineItem>
                    {
                        new() { Description = $"{options.SupportTier} Support", Quantity = 1, UnitPrice = supportMonthly, Unit = $"{supportPercent}% of base" }
                    }
                };
            }
        }

        // Calculate per-environment costs
        foreach (var env in sizing.Environments)
        {
            var envRatio = totalNodes > 0 ? (decimal)env.TotalNodes / totalNodes : 0;
            var envMonthly = estimate.Breakdown.Values.Sum(b => b.Monthly) * envRatio;
            estimate.EnvironmentCosts[env.Environment] = new EnvironmentCost
            {
                Environment = env.Environment,
                EnvironmentName = env.EnvironmentName,
                MonthlyCost = envMonthly,
                Nodes = env.TotalNodes,
                TotalCpu = env.TotalCpu,
                TotalRamGB = env.TotalRam,
                TotalDiskGB = env.TotalDisk
            };
        }

        CalculateTotals(estimate);
        return estimate;
    }

    private CostEstimate CalculateVMCost(VMSizingResult sizing, PricingModel pricing, CostEstimationOptions options)
    {
        var estimate = new CostEstimate
        {
            Provider = pricing.Provider,
            Region = pricing.Region,
            Currency = pricing.Currency,
            PricingType = pricing.PricingType,
            CalculatedAt = DateTime.UtcNow,
            PricingSource = pricing.Source
        };

        var totalVMs = sizing.GrandTotal.TotalVMs;
        var totalCpu = sizing.GrandTotal.TotalCpu;
        var totalRam = sizing.GrandTotal.TotalRam;
        var totalDisk = sizing.GrandTotal.TotalDisk;

        // Compute costs
        var cpuHourlyCost = totalCpu * pricing.Compute.CpuPerHour;
        var ramHourlyCost = totalRam * pricing.Compute.RamGBPerHour;
        var computeHourly = cpuHourlyCost + ramHourlyCost;
        var computeMonthly = computeHourly * HoursPerMonth;

        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown
        {
            Category = CostCategory.Compute,
            Monthly = computeMonthly,
            Description = "Compute resources",
            LineItems = new List<CostLineItem>
            {
                new() { Description = "vCPU", Quantity = totalCpu, UnitPrice = totalCpu > 0 ? cpuHourlyCost * HoursPerMonth / totalCpu : 0, Unit = "cores" },
                new() { Description = "RAM", Quantity = totalRam, UnitPrice = totalRam > 0 ? ramHourlyCost * HoursPerMonth / totalRam : 0, Unit = "GB" }
            }
        };

        // Storage costs
        if (options.IncludeStorage && totalDisk > 0)
        {
            var storageMonthly = totalDisk * pricing.Storage.SsdPerGBMonth;
            estimate.Breakdown[CostCategory.Storage] = new CostBreakdown
            {
                Category = CostCategory.Storage,
                Monthly = storageMonthly,
                Description = "Storage",
                LineItems = new List<CostLineItem>
                {
                    new() { Description = "Block Storage (SSD)", Quantity = totalDisk, UnitPrice = pricing.Storage.SsdPerGBMonth, Unit = "GB" }
                }
            };
        }

        // Network costs
        if (options.IncludeNetwork)
        {
            var lbCount = sizing.Environments.Count(e => e.LoadBalancerVMs > 0);
            var egressMonthly = options.MonthlyEgressGB * pricing.Network.EgressPerGB;
            var effectiveLbCount = Math.Max(options.LoadBalancers, lbCount);
            var lbMonthly = effectiveLbCount * pricing.Network.LoadBalancerPerHour * HoursPerMonth;

            estimate.Breakdown[CostCategory.Network] = new CostBreakdown
            {
                Category = CostCategory.Network,
                Monthly = egressMonthly + lbMonthly,
                Description = "Network",
                LineItems = new List<CostLineItem>
                {
                    new() { Description = "Data Egress", Quantity = options.MonthlyEgressGB, UnitPrice = pricing.Network.EgressPerGB, Unit = "GB" },
                    new() { Description = "Load Balancers", Quantity = effectiveLbCount, UnitPrice = effectiveLbCount > 0 ? lbMonthly / effectiveLbCount : 0, Unit = "LBs" }
                }
            };
        }

        // Support costs
        if (options.IncludeSupport && options.SupportTier != SupportTier.None)
        {
            var baseMonthly = estimate.Breakdown.Values.Sum(b => b.Monthly);
            var supportPercent = GetSupportPercent(pricing.Support, options.SupportTier);
            var supportMonthly = baseMonthly * (supportPercent / 100);

            if (supportMonthly > 0)
            {
                estimate.Breakdown[CostCategory.Support] = new CostBreakdown
                {
                    Category = CostCategory.Support,
                    Monthly = supportMonthly,
                    Description = $"{options.SupportTier} Support ({supportPercent}%)"
                };
            }
        }

        // Calculate per-environment costs
        var totalResources = totalCpu + totalRam;
        foreach (var env in sizing.Environments)
        {
            var envResources = env.TotalCpu + env.TotalRam;
            var envRatio = totalResources > 0 ? (decimal)envResources / totalResources : 0;
            var envMonthly = estimate.Breakdown.Values.Sum(b => b.Monthly) * envRatio;
            estimate.EnvironmentCosts[env.Environment] = new EnvironmentCost
            {
                Environment = env.Environment,
                EnvironmentName = env.EnvironmentName,
                MonthlyCost = envMonthly,
                Nodes = env.TotalVMs,
                TotalCpu = env.TotalCpu,
                TotalRamGB = env.TotalRam,
                TotalDiskGB = env.TotalDisk
            };
        }

        CalculateTotals(estimate);
        return estimate;
    }

    private static void CalculateTotals(CostEstimate estimate)
    {
        estimate.MonthlyTotal = estimate.Breakdown.Values.Sum(b => b.Monthly);

        // Calculate percentages
        foreach (var breakdown in estimate.Breakdown.Values)
        {
            breakdown.Percentage = estimate.MonthlyTotal > 0
                ? (breakdown.Monthly / estimate.MonthlyTotal) * 100
                : 0;
        }

        foreach (var envCost in estimate.EnvironmentCosts.Values)
        {
            envCost.Percentage = estimate.MonthlyTotal > 0
                ? (envCost.MonthlyCost / estimate.MonthlyTotal) * 100
                : 0;
        }
    }

    private static decimal GetSupportPercent(SupportPricing support, SupportTier tier)
    {
        return tier switch
        {
            SupportTier.Basic => support.BasicSupportPercent,
            SupportTier.Developer => support.DeveloperSupportPercent,
            SupportTier.Business => support.BusinessSupportPercent,
            SupportTier.Enterprise => support.EnterpriseSupportPercent,
            _ => 0
        };
    }

    private static decimal CalculateLicenseCost(string distribution, int nodes)
    {
        var distributionLower = distribution.ToLowerInvariant();

        return distributionLower switch
        {
            "openshift" or "red hat openshift" => 2500m * nodes,
            "tanzu" or "vmware tanzu" => 1500m * nodes,
            "rancher" or "rancher enterprise" or "rke2" => 1000m * nodes,
            "charmed" or "charmed kubernetes" or "charmed k8s" => 500m * nodes,
            _ => 0m
        };
    }
}
