using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for managing wizard state across Blazor components
/// </summary>
public interface IWizardStateService
{
    // Current wizard state
    int CurrentStep { get; set; }
    PlatformType? SelectedPlatform { get; set; }
    DeploymentModel? SelectedDeployment { get; set; }
    Technology? SelectedTechnology { get; set; }
    Distribution? SelectedDistribution { get; set; }
    ClusterMode? SelectedClusterMode { get; set; }
    string SingleClusterScope { get; set; }
    string ConfigTab { get; set; }
    string DistributionFilter { get; set; }

    // Environment configuration
    HashSet<EnvironmentType> EnabledEnvironments { get; }
    Dictionary<EnvironmentType, AppConfig> EnvApps { get; }
    Dictionary<EnvironmentType, double> Headroom { get; }
    Dictionary<EnvironmentType, int> Replicas { get; }

    // Overcommit settings
    double ProdCpuOvercommit { get; set; }
    double ProdMemoryOvercommit { get; set; }
    double NonProdCpuOvercommit { get; set; }
    double NonProdMemoryOvercommit { get; set; }

    // Node specs
    NodeSpecsConfig NodeSpecs { get; set; }

    // Platform-specific configuration (V4 panels)
    // K8s Configuration
    K8sPodConfig K8sPodConfig { get; }
    K8sEnvironmentConfig K8sEnvConfig { get; }
    K8sNodeConfig K8sNodeConfig { get; }

    // VM Configuration
    VmAppConfig VmAppConfig { get; }
    VmEnvironmentConfig VmEnvConfig { get; }
    VmHostConfig VmHostConfig { get; }

    // Calculated resource requirements (derived from above configs)
    K8sResourceRequirements K8sRequirements { get; }
    VmResourceRequirements VmRequirements { get; }

    // Results
    K8sSizingResult? Results { get; set; }

    // Events
    event Action? OnStateChanged;

    // Methods
    void Reset();
    void NotifyStateChanged();
    int GetTotalSteps();
    string GetStepLabel(int step);
    bool CanNavigateToStep(int step);
    EnvironmentType GetSingleClusterEnvironment();
    bool IsSingleClusterMode();
    bool IsProdEnvironment(EnvironmentType env);
}
