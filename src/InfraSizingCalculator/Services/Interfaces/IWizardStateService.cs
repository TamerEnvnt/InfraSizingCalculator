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
