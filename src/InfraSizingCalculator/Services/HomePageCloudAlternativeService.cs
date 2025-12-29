using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Provides cloud alternative comparison functionality for the Home page.
/// Extracted from Home.razor.cs for testability and separation of concerns.
/// </summary>
public interface IHomePageCloudAlternativeService
{
    /// <summary>
    /// Gets the cloud instance type recommendation based on CPU and RAM requirements.
    /// </summary>
    string GetInstanceType(string provider, decimal cpuPerWorker, decimal ramPerWorker);

    /// <summary>
    /// Gets a list of cloud alternatives for comparison against on-prem deployment.
    /// </summary>
    List<CloudAlternativeInfo> GetCloudAlternativesForBreakdown(
        K8sSizingResult? results,
        Distribution? selectedDistribution,
        IHomePageCostService costService,
        IHomePageDistributionService distributionService);
}

/// <summary>
/// Represents cloud alternative information for comparison display.
/// </summary>
public class CloudAlternativeInfo
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ProviderClass { get; set; } = "";
    public string InstanceType { get; set; } = "";
    public decimal MonthlyCost { get; set; }
    public decimal ComputeCost { get; set; }
    public decimal ControlPlaneCost { get; set; }
    public decimal StorageCost { get; set; }
    public decimal LicenseCost { get; set; }
    public bool IsOpenShiftManaged { get; set; }
    public Distribution TargetDistribution { get; set; }
}

public class HomePageCloudAlternativeService : IHomePageCloudAlternativeService
{
    public string GetInstanceType(string provider, decimal cpuPerWorker, decimal ramPerWorker)
    {
        // Simple instance type mapping based on CPU/RAM
        return provider.ToLower() switch
        {
            "aws" => cpuPerWorker >= 16 ? "m6i.4xlarge" :
                     cpuPerWorker >= 8 ? "m6i.2xlarge" : "m6i.xlarge",
            "azure" => cpuPerWorker >= 16 ? "Standard_D16s_v5" :
                       cpuPerWorker >= 8 ? "Standard_D8s_v5" : "Standard_D4s_v5",
            "gcp" => cpuPerWorker >= 16 ? "n2-standard-16" :
                     cpuPerWorker >= 8 ? "n2-standard-8" : "n2-standard-4",
            _ => "standard"
        };
    }

    public List<CloudAlternativeInfo> GetCloudAlternativesForBreakdown(
        K8sSizingResult? results,
        Distribution? selectedDistribution,
        IHomePageCostService costService,
        IHomePageDistributionService distributionService)
    {
        var alternatives = new List<CloudAlternativeInfo>();
        var workerCount = results?.GrandTotal.TotalWorkers ?? 3;
        var totalCpu = results?.GrandTotal.TotalCpu ?? 48;
        var totalRam = results?.GrandTotal.TotalRam ?? 192;

        // Calculate instance sizing - use worker specs from results
        var cpuPerWorker = totalCpu / Math.Max(workerCount, 1);
        var ramPerWorker = totalRam / Math.Max(workerCount, 1);

        // AWS EKS
        var eksInstanceType = GetInstanceType("aws", cpuPerWorker, ramPerWorker);
        var eksComputeCost = costService.GetComputeCost("aws", eksInstanceType, workerCount);
        alternatives.Add(new CloudAlternativeInfo
        {
            Key = "eks",
            Name = "AWS EKS",
            Description = "Managed Kubernetes on AWS",
            ProviderClass = "aws",
            InstanceType = eksInstanceType,
            ComputeCost = eksComputeCost,
            ControlPlaneCost = 73, // $0.10/hr
            StorageCost = workerCount * 50 * 0.10m, // 50GB gp3 per worker
            LicenseCost = 0,
            MonthlyCost = eksComputeCost + 73 + (workerCount * 50 * 0.10m),
            IsOpenShiftManaged = false,
            TargetDistribution = Distribution.EKS
        });

        // Azure AKS
        var aksInstanceType = GetInstanceType("azure", cpuPerWorker, ramPerWorker);
        var aksComputeCost = costService.GetComputeCost("azure", aksInstanceType, workerCount);
        alternatives.Add(new CloudAlternativeInfo
        {
            Key = "aks",
            Name = "Azure AKS",
            Description = "Managed Kubernetes on Azure (Free control plane)",
            ProviderClass = "azure",
            InstanceType = aksInstanceType,
            ComputeCost = aksComputeCost,
            ControlPlaneCost = 0, // Free!
            StorageCost = workerCount * 50 * 0.08m, // 50GB managed disk per worker
            LicenseCost = 0,
            MonthlyCost = aksComputeCost + (workerCount * 50 * 0.08m),
            IsOpenShiftManaged = false,
            TargetDistribution = Distribution.AKS
        });

        // Google GKE
        var gkeInstanceType = GetInstanceType("gcp", cpuPerWorker, ramPerWorker);
        var gkeComputeCost = costService.GetComputeCost("gcp", gkeInstanceType, workerCount);
        alternatives.Add(new CloudAlternativeInfo
        {
            Key = "gke",
            Name = "Google GKE",
            Description = "Managed Kubernetes on GCP",
            ProviderClass = "gcp",
            InstanceType = gkeInstanceType,
            ComputeCost = gkeComputeCost,
            ControlPlaneCost = 73, // $0.10/hr
            StorageCost = workerCount * 50 * 0.08m, // 50GB pd-ssd per worker
            LicenseCost = 0,
            MonthlyCost = gkeComputeCost + 73 + (workerCount * 50 * 0.08m),
            IsOpenShiftManaged = false,
            TargetDistribution = Distribution.GKE
        });

        // Add OpenShift managed options if current selection is OpenShift
        if (selectedDistribution.HasValue && distributionService.IsOpenShiftDistribution(selectedDistribution))
        {
            // ROSA (AWS)
            alternatives.Add(new CloudAlternativeInfo
            {
                Key = "rosa",
                Name = "ROSA (AWS)",
                Description = "Red Hat OpenShift Service on AWS",
                ProviderClass = "aws",
                InstanceType = eksInstanceType,
                ComputeCost = eksComputeCost,
                ControlPlaneCost = 125, // ~$0.171/hr
                StorageCost = workerCount * 100 * 0.10m, // 100GB gp3 per worker
                LicenseCost = workerCount * 125, // ~$0.171/hr per 4 vCPU
                MonthlyCost = eksComputeCost + 125 + (workerCount * 100 * 0.10m) + (workerCount * 125),
                IsOpenShiftManaged = true,
                TargetDistribution = Distribution.OpenShiftROSA
            });

            // ARO (Azure)
            alternatives.Add(new CloudAlternativeInfo
            {
                Key = "aro",
                Name = "ARO (Azure)",
                Description = "Azure Red Hat OpenShift",
                ProviderClass = "azure",
                InstanceType = aksInstanceType,
                ComputeCost = aksComputeCost * 1.1m, // Slightly higher for D8s_v4 typical
                ControlPlaneCost = 0, // Included in VM pricing
                StorageCost = workerCount * 100 * 0.08m, // 100GB managed disk per worker
                LicenseCost = workerCount * 110, // OpenShift license included
                MonthlyCost = (aksComputeCost * 1.1m) + (workerCount * 100 * 0.08m) + (workerCount * 110),
                IsOpenShiftManaged = true,
                TargetDistribution = Distribution.OpenShiftARO
            });

            // OpenShift Dedicated (GCP)
            alternatives.Add(new CloudAlternativeInfo
            {
                Key = "osd",
                Name = "OSD (GCP)",
                Description = "OpenShift Dedicated on Google Cloud",
                ProviderClass = "gcp",
                InstanceType = gkeInstanceType,
                ComputeCost = gkeComputeCost,
                ControlPlaneCost = 150, // Managed fee
                StorageCost = workerCount * 100 * 0.08m, // 100GB pd-ssd per worker
                LicenseCost = workerCount * 120, // OpenShift license
                MonthlyCost = gkeComputeCost + 150 + (workerCount * 100 * 0.08m) + (workerCount * 120),
                IsOpenShiftManaged = true,
                TargetDistribution = Distribution.OpenShiftDedicated
            });
        }

        return alternatives;
    }
}
