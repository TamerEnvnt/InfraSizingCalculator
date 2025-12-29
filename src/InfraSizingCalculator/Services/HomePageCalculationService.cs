using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Handles sizing calculations for the Home page.
/// Extracted from Home.razor for testability and separation of concerns.
/// </summary>
public interface IHomePageCalculationService
{
    /// <summary>
    /// Builds K8sSizingInput from current state.
    /// </summary>
    K8sSizingInput BuildK8sInput(IHomePageStateService state, IDistributionService distributionService);

    /// <summary>
    /// Builds VMSizingInput from current state.
    /// </summary>
    VMSizingInput BuildVMInput(IHomePageStateService state);

    /// <summary>
    /// Calculates K8s sizing based on current state.
    /// </summary>
    K8sSizingResult CalculateK8s(IHomePageStateService state, IDistributionService distributionService, IK8sSizingService k8sSizingService);

    /// <summary>
    /// Calculates VM sizing based on current state.
    /// </summary>
    VMSizingResult CalculateVM(IHomePageStateService state, IVMSizingService vmSizingService);

    /// <summary>
    /// Gets the effective cluster mode based on single cluster scope.
    /// </summary>
    ClusterMode GetEffectiveClusterMode(ClusterMode? selectedMode, string singleClusterScope);

    /// <summary>
    /// Gets the single cluster environment from the scope string.
    /// </summary>
    EnvironmentType GetSingleClusterEnvironment(string singleClusterScope);

    /// <summary>
    /// Builds per-environment node specs for multi-cluster mode.
    /// </summary>
    (Dictionary<EnvironmentType, NodeSpecs>? ControlPlane,
     Dictionary<EnvironmentType, NodeSpecs>? Worker,
     Dictionary<EnvironmentType, NodeSpecs>? Infra)
    BuildPerEnvironmentSpecs(NodeSpecsConfig nodeSpecs, ClusterMode? clusterMode);
}

public class HomePageCalculationService : IHomePageCalculationService
{
    /// <inheritdoc/>
    public K8sSizingInput BuildK8sInput(IHomePageStateService state, IDistributionService distributionService)
    {
        if (state.SelectedDistribution == null || state.SelectedTechnology == null || state.SelectedClusterMode == null)
        {
            throw new InvalidOperationException("Required selections (Distribution, Technology, ClusterMode) must be set before building input");
        }

        var effectiveClusterMode = GetEffectiveClusterMode(state.SelectedClusterMode, state.SingleClusterScope);
        var effectiveEnabledEnvs = GetEffectiveEnabledEnvironments(state);

        var input = new K8sSizingInput
        {
            Distribution = state.SelectedDistribution.Value,
            Technology = state.SelectedTechnology.Value,
            ClusterMode = effectiveClusterMode,
            SelectedEnvironment = state.SingleClusterScope != "Shared"
                ? GetSingleClusterEnvironment(state.SingleClusterScope)
                : EnvironmentType.Prod,
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>(state.EnvironmentApps),
            ProdApps = state.EnvironmentApps.GetValueOrDefault(EnvironmentType.Prod, new AppConfig { Medium = 70 }),
            NonProdApps = state.EnvironmentApps.GetValueOrDefault(EnvironmentType.Dev, new AppConfig { Medium = 70 }),
            EnabledEnvironments = effectiveEnabledEnvs,
            EnableHeadroom = true,
            Headroom = BuildHeadroomSettings(state.Headroom),
            Replicas = BuildReplicaSettings(state.Replicas),
            ProdOvercommit = new OvercommitSettings
            {
                Cpu = state.ProdCpuOvercommit,
                Memory = state.ProdMemoryOvercommit
            },
            NonProdOvercommit = new OvercommitSettings
            {
                Cpu = state.NonProdCpuOvercommit,
                Memory = state.NonProdMemoryOvercommit
            },
            HADRConfig = state.K8sHADRConfig
        };

        // Build custom node specs
        var distroConfig = distributionService.GetConfig(state.SelectedDistribution.Value);
        var (perEnvControlPlane, perEnvWorker, perEnvInfra) = BuildPerEnvironmentSpecs(state.NodeSpecs, state.SelectedClusterMode);

        input.CustomNodeSpecs = new DistributionConfig
        {
            Distribution = state.SelectedDistribution.Value,
            Name = distroConfig.Name,
            Vendor = distroConfig.Vendor,
            HasManagedControlPlane = distroConfig.HasManagedControlPlane,
            HasInfraNodes = distroConfig.HasInfraNodes,
            ProdControlPlane = new NodeSpecs(state.NodeSpecs.ProdMasterCpu, state.NodeSpecs.ProdMasterRam, state.NodeSpecs.ProdMasterDisk),
            NonProdControlPlane = new NodeSpecs(state.NodeSpecs.NonProdMasterCpu, state.NodeSpecs.NonProdMasterRam, state.NodeSpecs.NonProdMasterDisk),
            ProdWorker = new NodeSpecs(state.NodeSpecs.ProdWorkerCpu, state.NodeSpecs.ProdWorkerRam, state.NodeSpecs.ProdWorkerDisk),
            NonProdWorker = new NodeSpecs(state.NodeSpecs.NonProdWorkerCpu, state.NodeSpecs.NonProdWorkerRam, state.NodeSpecs.NonProdWorkerDisk),
            ProdInfra = new NodeSpecs(state.NodeSpecs.ProdInfraCpu, state.NodeSpecs.ProdInfraRam, state.NodeSpecs.ProdInfraDisk),
            NonProdInfra = new NodeSpecs(state.NodeSpecs.NonProdInfraCpu, state.NodeSpecs.NonProdInfraRam, state.NodeSpecs.NonProdInfraDisk),
            PerEnvControlPlane = perEnvControlPlane,
            PerEnvWorker = perEnvWorker,
            PerEnvInfra = perEnvInfra
        };

        return input;
    }

    /// <inheritdoc/>
    public VMSizingInput BuildVMInput(IHomePageStateService state)
    {
        if (state.SelectedTechnology == null)
        {
            throw new InvalidOperationException("Technology must be selected before building VM input");
        }

        return new VMSizingInput
        {
            Technology = state.SelectedTechnology.Value,
            EnabledEnvironments = state.EnabledEnvironments,
            EnvironmentConfigs = state.VMEnvironmentConfigs,
            SystemOverheadPercent = state.VMSystemOverhead
        };
    }

    /// <inheritdoc/>
    public K8sSizingResult CalculateK8s(
        IHomePageStateService state,
        IDistributionService distributionService,
        IK8sSizingService k8sSizingService)
    {
        var input = BuildK8sInput(state, distributionService);
        return k8sSizingService.Calculate(input);
    }

    /// <inheritdoc/>
    public VMSizingResult CalculateVM(IHomePageStateService state, IVMSizingService vmSizingService)
    {
        var input = BuildVMInput(state);
        return vmSizingService.Calculate(input);
    }

    /// <inheritdoc/>
    public ClusterMode GetEffectiveClusterMode(ClusterMode? selectedMode, string singleClusterScope)
    {
        if (selectedMode == null)
        {
            return ClusterMode.MultiCluster;
        }

        var isSingleClusterMode = selectedMode.Value == ClusterMode.SharedCluster ||
                                  selectedMode.Value == ClusterMode.PerEnvironment;

        if (isSingleClusterMode)
        {
            return singleClusterScope == "Shared"
                ? ClusterMode.SharedCluster
                : ClusterMode.PerEnvironment;
        }

        return selectedMode.Value;
    }

    /// <inheritdoc/>
    public EnvironmentType GetSingleClusterEnvironment(string singleClusterScope)
    {
        return singleClusterScope switch
        {
            "Dev" => EnvironmentType.Dev,
            "Test" => EnvironmentType.Test,
            "Stage" => EnvironmentType.Stage,
            "Prod" => EnvironmentType.Prod,
            "DR" => EnvironmentType.DR,
            _ => EnvironmentType.Prod
        };
    }

    /// <inheritdoc/>
    public (Dictionary<EnvironmentType, NodeSpecs>? ControlPlane,
            Dictionary<EnvironmentType, NodeSpecs>? Worker,
            Dictionary<EnvironmentType, NodeSpecs>? Infra)
    BuildPerEnvironmentSpecs(NodeSpecsConfig nodeSpecs, ClusterMode? clusterMode)
    {
        if (clusterMode != ClusterMode.MultiCluster)
        {
            return (null, null, null);
        }

        var perEnvControlPlane = new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Dev] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Dev),
            [EnvironmentType.Test] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Test),
            [EnvironmentType.Stage] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Stage),
            [EnvironmentType.Prod] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Prod),
            [EnvironmentType.DR] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.DR)
        };

        var perEnvWorker = new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Dev] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Dev),
            [EnvironmentType.Test] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Test),
            [EnvironmentType.Stage] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Stage),
            [EnvironmentType.Prod] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Prod),
            [EnvironmentType.DR] = nodeSpecs.GetWorkerSpecs(EnvironmentType.DR)
        };

        var perEnvInfra = new Dictionary<EnvironmentType, NodeSpecs>
        {
            [EnvironmentType.Dev] = nodeSpecs.GetInfraSpecs(EnvironmentType.Dev),
            [EnvironmentType.Test] = nodeSpecs.GetInfraSpecs(EnvironmentType.Test),
            [EnvironmentType.Stage] = nodeSpecs.GetInfraSpecs(EnvironmentType.Stage),
            [EnvironmentType.Prod] = nodeSpecs.GetInfraSpecs(EnvironmentType.Prod),
            [EnvironmentType.DR] = nodeSpecs.GetInfraSpecs(EnvironmentType.DR)
        };

        return (perEnvControlPlane, perEnvWorker, perEnvInfra);
    }

    private HashSet<EnvironmentType> GetEffectiveEnabledEnvironments(IHomePageStateService state)
    {
        var isSingleClusterMode = state.SelectedClusterMode == ClusterMode.SharedCluster ||
                                  state.SelectedClusterMode == ClusterMode.PerEnvironment;

        if (isSingleClusterMode && state.SingleClusterScope != "Shared")
        {
            // Single environment mode - only that environment enabled
            var singleEnv = GetSingleClusterEnvironment(state.SingleClusterScope);
            return new HashSet<EnvironmentType> { singleEnv };
        }

        return state.EnabledEnvironments;
    }

    private static HeadroomSettings BuildHeadroomSettings(Dictionary<EnvironmentType, double> headroom)
    {
        return new HeadroomSettings
        {
            Dev = headroom.GetValueOrDefault(EnvironmentType.Dev, 33),
            Test = headroom.GetValueOrDefault(EnvironmentType.Test, 33),
            Stage = headroom.GetValueOrDefault(EnvironmentType.Stage, 0),
            Prod = headroom.GetValueOrDefault(EnvironmentType.Prod, 37.5),
            DR = headroom.GetValueOrDefault(EnvironmentType.DR, 37.5)
        };
    }

    private static ReplicaSettings BuildReplicaSettings(Dictionary<EnvironmentType, int> replicas)
    {
        return new ReplicaSettings
        {
            NonProd = replicas.GetValueOrDefault(EnvironmentType.Dev, 1),
            Stage = replicas.GetValueOrDefault(EnvironmentType.Stage, 2),
            Prod = replicas.GetValueOrDefault(EnvironmentType.Prod, 3)
        };
    }
}
