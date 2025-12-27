using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

public interface IK8sSizingService
{
    K8sSizingResult Calculate(K8sSizingInput input);

    /// <summary>
    /// BR-M001 through BR-M004: Master node calculation
    /// Now integrated with K8sHADRConfig for control plane HA settings
    /// </summary>
    int CalculateMasterNodes(int workerCount, bool isManagedControlPlane, K8sHADRConfig? hadrConfig = null);

    /// <summary>
    /// Calculate etcd nodes for external etcd configuration
    /// </summary>
    int CalculateEtcdNodes(K8sHADRConfig? hadrConfig);

    /// <summary>
    /// Apply multi-AZ minimum worker constraint
    /// </summary>
    int ApplyAZMinimum(int workers, K8sHADRConfig? hadrConfig);

    /// <summary>
    /// Calculate DR site resources based on DR pattern
    /// </summary>
    (int nodes, double costMultiplier) CalculateDRResources(int primaryNodes, K8sHADRConfig? hadrConfig);

    /// <summary>
    /// BR-I001 through BR-I006: Infrastructure node calculation
    /// </summary>
    int CalculateInfraNodes(int totalApps, bool isProd, bool hasInfraNodes);

    /// <summary>
    /// BR-W001 through BR-W006: Worker node calculation
    /// </summary>
    int CalculateWorkerNodes(
        AppConfig apps,
        Dictionary<AppTier, TierSpecs> tierSpecs,
        int replicas,
        NodeSpecs workerSpecs,
        double headroomPercent,
        OvercommitSettings overcommit);
}
