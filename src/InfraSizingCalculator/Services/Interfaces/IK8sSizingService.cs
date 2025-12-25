using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

public interface IK8sSizingService
{
    K8sSizingResult Calculate(K8sSizingInput input);

    /// <summary>
    /// BR-M001 through BR-M004: Master node calculation
    /// </summary>
    int CalculateMasterNodes(int workerCount, bool isManagedControlPlane);

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
