using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for analyzing sizing results and providing recommendations.
/// </summary>
public class ValidationRecommendationService
{
    /// <summary>
    /// Analyze K8s sizing results and return recommendations.
    /// </summary>
    public List<ValidationWarning> Analyze(K8sSizingResult result, K8sSizingInput input)
    {
        var warnings = new List<ValidationWarning>();

        // Check for over-provisioned non-prod environments
        AnalyzeEnvironmentBalance(result, warnings);

        // Check replica settings
        AnalyzeReplicas(result, input, warnings);

        // Check distribution choice
        AnalyzeDistributionChoice(result, input, warnings);

        // Check headroom settings
        AnalyzeHeadroom(input, warnings);

        // Check overcommit settings
        AnalyzeOvercommit(input, warnings);

        // Check cluster size
        AnalyzeClusterSize(result, warnings);

        // Add positive feedback if configuration looks good
        AddPositiveFeedback(result, input, warnings);

        return warnings.OrderByDescending(w => w.Severity).ToList();
    }

    /// <summary>
    /// Analyze VM sizing results and return recommendations.
    /// </summary>
    public List<ValidationWarning> Analyze(VMSizingResult result, VMSizingInput input)
    {
        var warnings = new List<ValidationWarning>();

        // Check HA patterns
        AnalyzeHAPatterns(result, warnings);

        // Check load balancer configuration
        AnalyzeLoadBalancers(result, warnings);

        // Check role configurations
        AnalyzeRoles(result, warnings);

        return warnings.OrderByDescending(w => w.Severity).ToList();
    }

    private void AnalyzeEnvironmentBalance(K8sSizingResult result, List<ValidationWarning> warnings)
    {
        var prodEnv = result.Environments.FirstOrDefault(e => e.IsProd && e.Environment == EnvironmentType.Prod);
        var devEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Dev);
        var testEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Test);

        if (prodEnv != null && devEnv != null)
        {
            // Check if dev is more than 50% of prod
            if (devEnv.TotalNodes > prodEnv.TotalNodes * 0.6)
            {
                var warning = ValidationWarning.Warning(
                    "env-dev-oversized",
                    "Dev Environment May Be Over-Provisioned",
                    $"Dev environment ({devEnv.TotalNodes} nodes) is {(devEnv.TotalNodes * 100 / prodEnv.TotalNodes)}% of production ({prodEnv.TotalNodes} nodes).",
                    "Consider reducing dev replicas or using smaller node specs for cost savings."
                );
                warning.Category = WarningCategory.Cost;
                warnings.Add(warning);
            }
        }

        if (prodEnv != null && testEnv != null)
        {
            if (testEnv.TotalNodes > prodEnv.TotalNodes * 0.6)
            {
                var warning = ValidationWarning.Warning(
                    "env-test-oversized",
                    "Test Environment May Be Over-Provisioned",
                    $"Test environment ({testEnv.TotalNodes} nodes) is {(testEnv.TotalNodes * 100 / prodEnv.TotalNodes)}% of production ({prodEnv.TotalNodes} nodes).",
                    "Consider reducing test replicas for cost savings."
                );
                warning.Category = WarningCategory.Cost;
                warnings.Add(warning);
            }
        }
    }

    private void AnalyzeReplicas(K8sSizingResult result, K8sSizingInput input, List<ValidationWarning> warnings)
    {
        // Check if non-prod has high replicas
        foreach (var env in result.Environments.Where(e => !e.IsProd))
        {
            if (env.Replicas > 2)
            {
                var warning = ValidationWarning.Info(
                    $"replicas-high-{env.Environment}",
                    $"High Replica Count in {env.Environment}",
                    $"{env.Environment} environment has {env.Replicas} replicas per app.",
                    "Non-production environments typically use 1-2 replicas. Consider reducing for cost savings."
                );
                warning.Category = WarningCategory.Cost;
                warnings.Add(warning);
            }
        }

        // Check if prod has low replicas
        var prodEnv = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Prod);
        if (prodEnv != null && prodEnv.Replicas < 2)
        {
            var warning = ValidationWarning.Warning(
                "replicas-low-prod",
                "Low Production Replicas",
                "Production has only 1 replica per application.",
                "Consider at least 2-3 replicas for high availability in production."
            );
            warning.Category = WarningCategory.HighAvailability;
            warnings.Add(warning);
        }
    }

    private void AnalyzeDistributionChoice(K8sSizingResult result, K8sSizingInput input, List<ValidationWarning> warnings)
    {
        var totalNodes = result.GrandTotal.TotalNodes;
        var distribution = input.Distribution;

        // Suggest lighter distributions for small deployments
        if (totalNodes < 15 && distribution == Distribution.OpenShift)
        {
            var warning = ValidationWarning.Info(
                "dist-openshift-small",
                "OpenShift May Be Overkill",
                $"Your deployment has only {totalNodes} nodes. OpenShift is designed for larger enterprise deployments.",
                "Consider K3s, MicroK8s, or a managed service (EKS/AKS/GKE) for smaller deployments to reduce complexity and cost."
            );
            warning.Category = WarningCategory.Distribution;
            warnings.Add(warning);
        }

        // Suggest managed services for cloud deployments
        if (totalNodes > 50 && distribution == Distribution.Kubernetes)
        {
            var warning = ValidationWarning.Info(
                "dist-managed-suggestion",
                "Consider Managed Kubernetes",
                $"With {totalNodes} nodes, managing vanilla Kubernetes can be complex.",
                "Consider EKS, AKS, GKE, or OKE to reduce operational overhead on control plane management."
            );
            warning.Category = WarningCategory.Distribution;
            warnings.Add(warning);
        }

        // Large deployments without infra nodes
        if (totalNodes > 30 && distribution != Distribution.OpenShift && result.GrandTotal.TotalInfra == 0)
        {
            var warning = ValidationWarning.Info(
                "dist-infra-suggestion",
                "Consider Dedicated Infrastructure Nodes",
                $"Your deployment has {totalNodes} nodes without dedicated infrastructure nodes.",
                "For larger deployments, consider OpenShift or dedicated nodes for monitoring, logging, and ingress."
            );
            warning.Category = WarningCategory.BestPractice;
            warnings.Add(warning);
        }
    }

    private void AnalyzeHeadroom(K8sSizingInput input, List<ValidationWarning> warnings)
    {
        if (!input.EnableHeadroom)
        {
            var warning = ValidationWarning.Warning(
                "headroom-disabled",
                "Headroom Is Disabled",
                "Resource headroom is disabled. Clusters will be sized exactly to current needs.",
                "Enable headroom (20-40%) to accommodate growth and burst traffic without immediate scaling."
            );
            warning.Category = WarningCategory.Sizing;
            warnings.Add(warning);
        }

        // Check for very high headroom in non-prod
        if (input.Headroom.Dev > 50 || input.Headroom.Test > 50)
        {
            var warning = ValidationWarning.Info(
                "headroom-high-nonprod",
                "High Headroom in Non-Production",
                $"Dev ({input.Headroom.Dev}%) or Test ({input.Headroom.Test}%) have high headroom percentages.",
                "Consider lower headroom (20-33%) for non-production to reduce costs."
            );
            warning.Category = WarningCategory.Cost;
            warnings.Add(warning);
        }
    }

    private void AnalyzeOvercommit(K8sSizingInput input, List<ValidationWarning> warnings)
    {
        // Warn about no overcommit
        if (input.ProdOvercommit.Cpu <= 1.0 && input.ProdOvercommit.Memory <= 1.0)
        {
            var warning = ValidationWarning.Info(
                "overcommit-none-prod",
                "No Resource Overcommit in Production",
                "Production has no CPU or memory overcommit (1:1 ratio).",
                "Some overcommit (1.5-2x CPU) is common in production to improve utilization. Adjust based on workload patterns."
            );
            warning.Category = WarningCategory.Cost;
            warnings.Add(warning);
        }

        // Warn about very high overcommit
        if (input.ProdOvercommit.Cpu > 4.0)
        {
            var warning = ValidationWarning.Warning(
                "overcommit-high-cpu",
                "High CPU Overcommit in Production",
                $"Production CPU overcommit is {input.ProdOvercommit.Cpu}x.",
                "High overcommit (>4x) may cause performance issues during peak usage. Consider 2-3x for production."
            );
            warning.Category = WarningCategory.Sizing;
            warnings.Add(warning);
        }

        if (input.ProdOvercommit.Memory > 2.0)
        {
            var warning = ValidationWarning.Warning(
                "overcommit-high-memory",
                "High Memory Overcommit in Production",
                $"Production memory overcommit is {input.ProdOvercommit.Memory}x.",
                "Memory overcommit >2x can cause OOM kills. Consider 1.0-1.5x for production stability."
            );
            warning.Category = WarningCategory.Sizing;
            warnings.Add(warning);
        }
    }

    private void AnalyzeClusterSize(K8sSizingResult result, List<ValidationWarning> warnings)
    {
        // Check for very large clusters
        foreach (var env in result.Environments)
        {
            if (env.Workers > 200)
            {
                var warning = ValidationWarning.Warning(
                    $"cluster-large-{env.Environment}",
                    $"Large Cluster in {env.Environment}",
                    $"{env.Environment} cluster has {env.Workers} worker nodes.",
                    "Consider splitting into multiple clusters for better isolation and easier management."
                );
                warning.Category = WarningCategory.BestPractice;
                warnings.Add(warning);
            }

            // Check for minimum workers
            if (env.Workers < 3 && env.IsProd)
            {
                var warning = ValidationWarning.Warning(
                    $"cluster-small-{env.Environment}",
                    $"Small Cluster in {env.Environment}",
                    $"{env.Environment} has only {env.Workers} workers.",
                    "Minimum 3 workers recommended for high availability."
                );
                warning.Category = WarningCategory.HighAvailability;
                warnings.Add(warning);
            }
        }
    }

    private void AddPositiveFeedback(K8sSizingResult result, K8sSizingInput input, List<ValidationWarning> warnings)
    {
        var hasIssues = warnings.Any(w => w.Severity >= WarningSeverity.Warning);

        if (!hasIssues)
        {
            var warning = ValidationWarning.Success(
                "config-good",
                "Configuration Looks Good",
                "No significant issues detected. Your sizing configuration follows best practices."
            );
            warning.Category = WarningCategory.General;
            warnings.Add(warning);
        }

        // Check for DR environment
        if (result.Environments.Any(e => e.Environment == EnvironmentType.DR))
        {
            var warning = ValidationWarning.Success(
                "dr-configured",
                "DR Environment Configured",
                "Disaster Recovery environment is included in your sizing."
            );
            warning.Category = WarningCategory.HighAvailability;
            warnings.Add(warning);
        }
    }

    private void AnalyzeHAPatterns(VMSizingResult result, List<ValidationWarning> warnings)
    {
        foreach (var env in result.Environments.Where(e => e.IsProd))
        {
            if (env.HAPattern == HAPattern.None)
            {
                var warning = ValidationWarning.Warning(
                    $"ha-none-{env.Environment}",
                    $"No HA in {env.EnvironmentName}",
                    $"{env.EnvironmentName} has no high availability pattern configured.",
                    "Consider Active-Active or Active-Passive HA for production environments."
                );
                warning.Category = WarningCategory.HighAvailability;
                warnings.Add(warning);
            }
        }
    }

    private void AnalyzeLoadBalancers(VMSizingResult result, List<ValidationWarning> warnings)
    {
        foreach (var env in result.Environments.Where(e => e.IsProd))
        {
            if (env.LoadBalancer == LoadBalancerOption.None)
            {
                var warning = ValidationWarning.Info(
                    $"lb-none-{env.Environment}",
                    $"No Load Balancer in {env.EnvironmentName}",
                    $"{env.EnvironmentName} has no load balancer configured.",
                    "Consider adding a load balancer for traffic distribution."
                );
                warning.Category = WarningCategory.HighAvailability;
                warnings.Add(warning);
            }
            else if (env.LoadBalancer == LoadBalancerOption.Single)
            {
                var warning = ValidationWarning.Warning(
                    $"lb-single-{env.Environment}",
                    $"Single Load Balancer in {env.EnvironmentName}",
                    $"{env.EnvironmentName} has a single load balancer (single point of failure).",
                    "Consider HA Pair for production load balancing."
                );
                warning.Category = WarningCategory.HighAvailability;
                warnings.Add(warning);
            }
        }
    }

    private void AnalyzeRoles(VMSizingResult result, List<ValidationWarning> warnings)
    {
        foreach (var env in result.Environments)
        {
            // Check for single instance of critical roles in production
            if (env.IsProd)
            {
                var singleInstanceRoles = env.Roles
                    .Where(r => r.TotalInstances == 1 &&
                               (r.Role == ServerRole.Database || r.Role == ServerRole.App))
                    .ToList();

                foreach (var role in singleInstanceRoles)
                {
                    var warning = ValidationWarning.Warning(
                        $"role-single-{env.Environment}-{role.RoleName}",
                        $"Single {role.RoleName} in {env.EnvironmentName}",
                        $"{role.RoleName} has only 1 instance in production.",
                        "Consider adding redundancy for critical infrastructure roles."
                    );
                    warning.Category = WarningCategory.HighAvailability;
                    warnings.Add(warning);
                }
            }
        }
    }
}
