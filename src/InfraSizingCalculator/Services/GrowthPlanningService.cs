using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Growth;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for calculating growth projections and capacity planning
/// </summary>
public class GrowthPlanningService : IGrowthPlanningService
{
    // Cluster limits by distribution
    private static readonly Dictionary<Distribution, Dictionary<string, int>> ClusterLimitsByDistribution = new()
    {
        // Cloud managed (larger limits due to provider management)
        [Distribution.EKS] = new() { ["Nodes"] = 5000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },
        [Distribution.AKS] = new() { ["Nodes"] = 5000, ["PodsPerNode"] = 250, ["TotalPods"] = 150000 },
        [Distribution.GKE] = new() { ["Nodes"] = 15000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },
        [Distribution.OKE] = new() { ["Nodes"] = 2000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },

        // OpenShift variants
        [Distribution.OpenShift] = new() { ["Nodes"] = 2000, ["PodsPerNode"] = 250, ["TotalPods"] = 150000 },
        [Distribution.OpenShiftROSA] = new() { ["Nodes"] = 5000, ["PodsPerNode"] = 250, ["TotalPods"] = 150000 },
        [Distribution.OpenShiftARO] = new() { ["Nodes"] = 5000, ["PodsPerNode"] = 250, ["TotalPods"] = 150000 },

        // Rancher variants
        [Distribution.Rancher] = new() { ["Nodes"] = 2000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },
        [Distribution.RancherHosted] = new() { ["Nodes"] = 2000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },

        // Tanzu variants
        [Distribution.Tanzu] = new() { ["Nodes"] = 2000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },
        [Distribution.TanzuCloud] = new() { ["Nodes"] = 2000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 },

        // Lightweight distributions
        [Distribution.K3s] = new() { ["Nodes"] = 500, ["PodsPerNode"] = 110, ["TotalPods"] = 50000 },
        [Distribution.MicroK8s] = new() { ["Nodes"] = 200, ["PodsPerNode"] = 110, ["TotalPods"] = 20000 },

        // Enterprise on-prem
        [Distribution.Charmed] = new() { ["Nodes"] = 1000, ["PodsPerNode"] = 110, ["TotalPods"] = 100000 },
        [Distribution.Kubernetes] = new() { ["Nodes"] = 5000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 }
    };

    public GrowthProjection CalculateK8sGrowthProjection(
        K8sSizingResult currentResult,
        CostEstimate? currentCost,
        GrowthSettings settings,
        Distribution distribution)
    {
        var projection = new GrowthProjection
        {
            Settings = settings,
            BaselineDate = DateTime.UtcNow
        };

        // Calculate baseline from current result
        var totalApps = currentResult.Environments.Sum(e => e.Apps);
        var totalNodes = currentResult.GrandTotal.TotalNodes;
        var totalWorkers = currentResult.GrandTotal.TotalWorkers;
        var totalCpu = currentResult.GrandTotal.TotalCpu;
        var totalRam = currentResult.GrandTotal.TotalRam;
        var totalStorage = currentResult.GrandTotal.TotalDisk;
        var monthlyCost = currentCost?.MonthlyTotal ?? 0;

        projection.Baseline = new ProjectionPoint
        {
            Year = 0,
            Label = "Current",
            ProjectedApps = totalApps,
            ProjectedNodes = totalNodes,
            ProjectedWorkerNodes = totalWorkers,
            ProjectedCpu = totalCpu,
            ProjectedRamGB = totalRam,
            ProjectedStorageGB = totalStorage,
            ProjectedMonthlyCost = monthlyCost,
            ProjectedYearlyCost = monthlyCost * 12,
            GrowthFromPrevious = 0,
            CumulativeGrowth = 0
        };

        // Add environment breakdown for baseline
        foreach (var env in currentResult.Environments)
        {
            var envCost = currentCost?.EnvironmentCosts.GetValueOrDefault(env.Environment)?.MonthlyCost ?? 0;
            projection.Baseline.EnvironmentBreakdown[env.Environment] = new EnvironmentProjection
            {
                Environment = env.Environment,
                Apps = env.Apps,
                Nodes = env.Masters + env.Workers + env.Infra,
                Cpu = env.TotalCpu,
                RamGB = env.TotalRam,
                MonthlyCost = envCost
            };
        }

        // Calculate projections for each year
        var previousApps = (double)totalApps;
        var previousNodes = (double)totalNodes;
        var previousWorkers = (double)totalWorkers;
        var previousCpu = (double)totalCpu;
        var previousRam = (double)totalRam;
        var previousStorage = (double)totalStorage;
        var previousCost = (double)monthlyCost;

        for (int year = 1; year <= settings.ProjectionYears; year++)
        {
            var growthRate = GetGrowthRateForYear(year, settings);
            var costInflation = 1 + (settings.AnnualCostInflation / 100);

            // Calculate new values based on growth pattern
            var newApps = ApplyGrowth(previousApps, growthRate, settings.Pattern, year);
            var newNodes = ApplyGrowth(previousNodes, growthRate, settings.Pattern, year);
            var newWorkers = ApplyGrowth(previousWorkers, growthRate, settings.Pattern, year);
            var newCpu = ApplyGrowth(previousCpu, growthRate, settings.Pattern, year);
            var newRam = ApplyGrowth(previousRam, growthRate, settings.Pattern, year);
            var newStorage = ApplyGrowth(previousStorage, growthRate, settings.Pattern, year);
            var newCost = settings.IncludeCostProjections
                ? ApplyGrowth(previousCost, growthRate, settings.Pattern, year) * costInflation
                : 0;

            var point = new ProjectionPoint
            {
                Year = year,
                Label = $"Year {year}",
                ProjectedApps = (int)Math.Ceiling(newApps),
                ProjectedNodes = (int)Math.Ceiling(newNodes),
                ProjectedWorkerNodes = (int)Math.Ceiling(newWorkers),
                ProjectedCpu = (int)Math.Ceiling(newCpu),
                ProjectedRamGB = (int)Math.Ceiling(newRam),
                ProjectedStorageGB = (int)Math.Ceiling(newStorage),
                ProjectedMonthlyCost = (decimal)newCost,
                ProjectedYearlyCost = (decimal)(newCost * 12),
                GrowthFromPrevious = previousApps > 0 ? ((newApps - previousApps) / previousApps) * 100 : 0,
                CumulativeGrowth = totalApps > 0 ? ((newApps - totalApps) / totalApps) * 100 : 0
            };

            projection.Points.Add(point);

            previousApps = newApps;
            previousNodes = newNodes;
            previousWorkers = newWorkers;
            previousCpu = newCpu;
            previousRam = newRam;
            previousStorage = newStorage;
            previousCost = newCost;
        }

        // Check for cluster limits and generate warnings
        if (settings.ShowClusterLimitWarnings)
        {
            projection.Warnings = CheckClusterLimits(projection, distribution);
        }

        // Generate scaling recommendations
        projection.Recommendations = GenerateRecommendations(projection, distribution);

        // Calculate summary
        projection.Summary = CalculateSummary(projection);

        return projection;
    }

    public GrowthProjection CalculateVMGrowthProjection(
        VMSizingResult currentResult,
        CostEstimate? currentCost,
        GrowthSettings settings)
    {
        var projection = new GrowthProjection
        {
            Settings = settings,
            BaselineDate = DateTime.UtcNow
        };

        // Calculate baseline from current VM result
        var totalVMs = currentResult.GrandTotal.TotalVMs;
        var totalCpu = currentResult.GrandTotal.TotalCpu;
        var totalRam = currentResult.GrandTotal.TotalRam;
        var totalStorage = currentResult.GrandTotal.TotalDisk;
        var monthlyCost = currentCost?.MonthlyTotal ?? 0;

        projection.Baseline = new ProjectionPoint
        {
            Year = 0,
            Label = "Current",
            ProjectedApps = totalVMs, // Use VMs as "apps" equivalent
            ProjectedNodes = totalVMs,
            ProjectedCpu = totalCpu,
            ProjectedRamGB = totalRam,
            ProjectedStorageGB = totalStorage,
            ProjectedMonthlyCost = monthlyCost,
            ProjectedYearlyCost = monthlyCost * 12,
            GrowthFromPrevious = 0,
            CumulativeGrowth = 0
        };

        // Calculate projections
        var previousVMs = (double)totalVMs;
        var previousCpu = (double)totalCpu;
        var previousRam = (double)totalRam;
        var previousStorage = (double)totalStorage;
        var previousCost = (double)monthlyCost;

        for (int year = 1; year <= settings.ProjectionYears; year++)
        {
            var growthRate = GetGrowthRateForYear(year, settings);
            var costInflation = 1 + (settings.AnnualCostInflation / 100);

            var newVMs = ApplyGrowth(previousVMs, growthRate, settings.Pattern, year);
            var newCpu = ApplyGrowth(previousCpu, growthRate, settings.Pattern, year);
            var newRam = ApplyGrowth(previousRam, growthRate, settings.Pattern, year);
            var newStorage = ApplyGrowth(previousStorage, growthRate, settings.Pattern, year);
            var newCost = settings.IncludeCostProjections
                ? ApplyGrowth(previousCost, growthRate, settings.Pattern, year) * costInflation
                : 0;

            var point = new ProjectionPoint
            {
                Year = year,
                Label = $"Year {year}",
                ProjectedApps = (int)Math.Ceiling(newVMs),
                ProjectedNodes = (int)Math.Ceiling(newVMs),
                ProjectedCpu = (int)Math.Ceiling(newCpu),
                ProjectedRamGB = (int)Math.Ceiling(newRam),
                ProjectedStorageGB = (int)Math.Ceiling(newStorage),
                ProjectedMonthlyCost = (decimal)newCost,
                ProjectedYearlyCost = (decimal)(newCost * 12),
                GrowthFromPrevious = previousVMs > 0 ? ((newVMs - previousVMs) / previousVMs) * 100 : 0,
                CumulativeGrowth = totalVMs > 0 ? ((newVMs - totalVMs) / totalVMs) * 100 : 0
            };

            projection.Points.Add(point);

            previousVMs = newVMs;
            previousCpu = newCpu;
            previousRam = newRam;
            previousStorage = newStorage;
            previousCost = newCost;
        }

        // Generate recommendations (no cluster limits for VMs)
        projection.Recommendations = GenerateRecommendations(projection, null);

        // Calculate summary
        projection.Summary = CalculateSummary(projection);

        return projection;
    }

    public Dictionary<string, int> GetClusterLimits(Distribution distribution)
    {
        return ClusterLimitsByDistribution.TryGetValue(distribution, out var limits)
            ? limits
            : new Dictionary<string, int> { ["Nodes"] = 2000, ["PodsPerNode"] = 110, ["TotalPods"] = 150000 };
    }

    public int? CalculateYearToLimit(double currentValue, double limit, double annualGrowthRate, GrowthPattern pattern)
    {
        if (currentValue >= limit) return 0;
        if (annualGrowthRate <= 0) return null;

        var value = currentValue;
        for (int year = 1; year <= 10; year++)
        {
            value = ApplyGrowth(value, annualGrowthRate, pattern, year);
            if (value >= limit) return year;
        }

        return null; // Not within 10 years
    }

    public List<ScalingRecommendation> GenerateRecommendations(GrowthProjection projection, Distribution? distribution)
    {
        var recommendations = new List<ScalingRecommendation>();
        var finalPoint = projection.Points.LastOrDefault() ?? projection.Baseline;
        var baseline = projection.Baseline;

        // Check for significant growth
        if (finalPoint.CumulativeGrowth > 100)
        {
            recommendations.Add(new ScalingRecommendation
            {
                Type = RecommendationType.EnableAutoscaling,
                RecommendedYear = 1,
                Priority = 1,
                Title = "Enable Cluster Autoscaling",
                Description = $"With {finalPoint.CumulativeGrowth:F0}% projected growth, consider enabling autoscaling to handle dynamic workloads efficiently.",
                Icon = "⚡"
            });
        }

        // Check for node growth > 50%
        var nodeGrowth = baseline.ProjectedNodes > 0
            ? ((double)(finalPoint.ProjectedNodes - baseline.ProjectedNodes) / baseline.ProjectedNodes) * 100
            : 0;

        if (nodeGrowth > 50)
        {
            recommendations.Add(new ScalingRecommendation
            {
                Type = RecommendationType.UpgradeNodeSize,
                RecommendedYear = Math.Max(1, projection.Settings.ProjectionYears / 2),
                Priority = 2,
                Title = "Consider Larger Node Sizes",
                Description = $"Node count will grow by {nodeGrowth:F0}%. Larger nodes may be more cost-effective than adding more smaller nodes.",
                Icon = "📈"
            });
        }

        // Check warnings for cluster split recommendation
        var criticalWarnings = projection.Warnings.Where(w => w.Severity == Models.Growth.WarningSeverity.Critical).ToList();
        if (criticalWarnings.Any())
        {
            var firstCritical = criticalWarnings.OrderBy(w => w.YearTriggered).First();
            recommendations.Add(new ScalingRecommendation
            {
                Type = RecommendationType.SplitCluster,
                RecommendedYear = Math.Max(1, firstCritical.YearTriggered - 1),
                Priority = 1,
                Title = "Plan for Cluster Split",
                Description = $"Cluster limits will be approached by Year {firstCritical.YearTriggered}. Plan to split workloads across multiple clusters.",
                Icon = "🔀"
            });
        }

        // Cost optimization for high growth
        if (projection.Summary.PercentageCostIncrease > 75)
        {
            recommendations.Add(new ScalingRecommendation
            {
                Type = RecommendationType.OptimizeResources,
                RecommendedYear = 1,
                Priority = 2,
                Title = "Review Resource Optimization",
                Description = $"Costs projected to increase by {projection.Summary.PercentageCostIncrease:F0}%. Consider reserved instances, spot instances, or right-sizing.",
                EstimatedCostImpact = -(projection.Summary.TotalCostOverPeriod * 0.15m), // ~15% potential savings
                Icon = "💰"
            });
        }

        // Distribution-specific recommendations
        if (distribution.HasValue)
        {
            switch (distribution.Value)
            {
                case Distribution.K3s when finalPoint.ProjectedNodes > 200:
                    recommendations.Add(new ScalingRecommendation
                    {
                        Type = RecommendationType.ConsiderManagedService,
                        RecommendedYear = 1,
                        Priority = 2,
                        Title = "Consider Migration to Managed K8s",
                        Description = "K3s is optimized for edge/small deployments. For large-scale growth, consider EKS, AKS, or GKE.",
                        Icon = "☁️"
                    });
                    break;

                case Distribution.MicroK8s when finalPoint.ProjectedNodes > 100:
                    recommendations.Add(new ScalingRecommendation
                    {
                        Type = RecommendationType.ConsiderManagedService,
                        RecommendedYear = 1,
                        Priority = 2,
                        Title = "Evaluate Enterprise Distribution",
                        Description = "MicroK8s is designed for development/small production. Consider OpenShift or Rancher for enterprise scale.",
                        Icon = "🏢"
                    });
                    break;
            }
        }

        return recommendations.OrderBy(r => r.Priority).ThenBy(r => r.RecommendedYear).ToList();
    }

    private double GetGrowthRateForYear(int year, GrowthSettings settings)
    {
        return settings.AnnualGrowthRate;
    }

    private double ApplyGrowth(double currentValue, double growthRate, GrowthPattern pattern, int year)
    {
        var rate = growthRate / 100;

        return pattern switch
        {
            GrowthPattern.Linear => currentValue * (1 + rate),
            GrowthPattern.Exponential => currentValue * Math.Pow(1 + rate, 1), // Compound per year
            GrowthPattern.SCurve => ApplySCurveGrowth(currentValue, rate, year),
            _ => currentValue * (1 + rate)
        };
    }

    private double ApplySCurveGrowth(double currentValue, double rate, int year)
    {
        // S-curve: slower at start and end, faster in middle
        // Using logistic function approximation
        var midpoint = 2.5; // Middle year for max growth
        var steepness = 1.5;
        var factor = 1 / (1 + Math.Exp(-steepness * (year - midpoint)));
        var adjustedRate = rate * factor * 2; // Scale to maintain overall growth
        return currentValue * (1 + adjustedRate);
    }

    private List<ClusterLimitWarning> CheckClusterLimits(GrowthProjection projection, Distribution distribution)
    {
        var warnings = new List<ClusterLimitWarning>();
        var limits = GetClusterLimits(distribution);
        var nodeLimit = limits.GetValueOrDefault("Nodes", 2000);

        foreach (var point in projection.Points)
        {
            var percentageOfLimit = (double)point.ProjectedNodes / nodeLimit * 100;

            if (percentageOfLimit >= 90 && !warnings.Any(w => w.Type == WarningType.NodeLimit && w.Severity == Models.Growth.WarningSeverity.Critical))
            {
                warnings.Add(new ClusterLimitWarning
                {
                    Type = WarningType.NodeLimit,
                    Severity = Models.Growth.WarningSeverity.Critical,
                    YearTriggered = point.Year,
                    Message = $"Node count ({point.ProjectedNodes}) will reach {percentageOfLimit:F0}% of cluster limit ({nodeLimit}) by Year {point.Year}",
                    CurrentValue = projection.Baseline.ProjectedNodes,
                    ProjectedValue = point.ProjectedNodes,
                    Limit = nodeLimit,
                    PercentageOfLimit = percentageOfLimit,
                    ResourceType = "Nodes"
                });
            }
            else if (percentageOfLimit >= 70 && !warnings.Any(w => w.Type == WarningType.NodeLimit))
            {
                warnings.Add(new ClusterLimitWarning
                {
                    Type = WarningType.NodeLimit,
                    Severity = Models.Growth.WarningSeverity.Warning,
                    YearTriggered = point.Year,
                    Message = $"Node count ({point.ProjectedNodes}) will reach {percentageOfLimit:F0}% of cluster limit ({nodeLimit}) by Year {point.Year}",
                    CurrentValue = projection.Baseline.ProjectedNodes,
                    ProjectedValue = point.ProjectedNodes,
                    Limit = nodeLimit,
                    PercentageOfLimit = percentageOfLimit,
                    ResourceType = "Nodes"
                });
            }
        }

        return warnings;
    }

    private ProjectionSummary CalculateSummary(GrowthProjection projection)
    {
        var baseline = projection.Baseline;
        var finalPoint = projection.Points.LastOrDefault() ?? baseline;

        var totalCost = projection.Baseline.ProjectedYearlyCost + projection.Points.Sum(p => p.ProjectedYearlyCost);
        var avgYearlyCost = projection.Points.Any()
            ? projection.Points.Average(p => p.ProjectedYearlyCost)
            : baseline.ProjectedYearlyCost;

        return new ProjectionSummary
        {
            TotalAppGrowth = finalPoint.ProjectedApps - baseline.ProjectedApps,
            PercentageAppGrowth = baseline.ProjectedApps > 0
                ? ((double)(finalPoint.ProjectedApps - baseline.ProjectedApps) / baseline.ProjectedApps) * 100
                : 0,
            TotalNodeGrowth = finalPoint.ProjectedNodes - baseline.ProjectedNodes,
            PercentageNodeGrowth = baseline.ProjectedNodes > 0
                ? ((double)(finalPoint.ProjectedNodes - baseline.ProjectedNodes) / baseline.ProjectedNodes) * 100
                : 0,
            TotalCostOverPeriod = totalCost,
            AverageYearlyCost = avgYearlyCost,
            CostIncrease = finalPoint.ProjectedYearlyCost - baseline.ProjectedYearlyCost,
            PercentageCostIncrease = baseline.ProjectedYearlyCost > 0
                ? (double)((finalPoint.ProjectedYearlyCost - baseline.ProjectedYearlyCost) / baseline.ProjectedYearlyCost) * 100
                : 0,
            MajorScalingYear = projection.Warnings.FirstOrDefault(w => w.Severity == Models.Growth.WarningSeverity.Critical)?.YearTriggered,
            WarningCount = projection.Warnings.Count,
            CriticalWarningCount = projection.Warnings.Count(w => w.Severity == Models.Growth.WarningSeverity.Critical)
        };
    }
}
