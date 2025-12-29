using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Growth;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Manages state for the Home page component.
/// Extracted from Home.razor for testability and separation of concerns.
/// </summary>
public interface IHomePageStateService
{
    // Wizard navigation
    int CurrentStep { get; set; }
    void GoToStep(int step);
    void NextStep();
    void PreviousStep();
    bool CanProceed();
    int GetTotalSteps();
    string GetStepLabel(int step);
    string? GetStepSelection(int step);

    // Platform/Deployment/Technology selection
    PlatformType? SelectedPlatform { get; set; }
    DeploymentModel? SelectedDeployment { get; set; }
    Technology? SelectedTechnology { get; set; }
    Distribution? SelectedDistribution { get; set; }

    // Cluster configuration
    ClusterMode? SelectedClusterMode { get; set; }
    string SingleClusterScope { get; set; }
    bool IsSingleClusterMode();
    int GetClusterCount();

    // Environment configuration
    HashSet<EnvironmentType> EnabledEnvironments { get; set; }
    Dictionary<EnvironmentType, AppConfig> EnvironmentApps { get; set; }
    bool IsEnvironmentEnabled(EnvironmentType env);
    void ToggleEnvironment(EnvironmentType env, bool enabled);
    IEnumerable<EnvironmentType> GetVisibleEnvironments();

    // Node specs and settings
    NodeSpecsConfig NodeSpecs { get; set; }
    K8sHADRConfig K8sHADRConfig { get; set; }
    Dictionary<EnvironmentType, double> Headroom { get; set; }
    Dictionary<EnvironmentType, int> Replicas { get; set; }
    double ProdCpuOvercommit { get; set; }
    double ProdMemoryOvercommit { get; set; }
    double NonProdCpuOvercommit { get; set; }
    double NonProdMemoryOvercommit { get; set; }

    // Results
    K8sSizingResult? K8sResults { get; set; }
    VMSizingResult? VMResults { get; set; }
    PricingStepResult? PricingResult { get; set; }
    List<ValidationWarning>? ValidationWarnings { get; set; }
    List<ValidationWarning>? VMValidationWarnings { get; set; }

    // Cost estimation
    CostEstimate? K8sCostEstimate { get; set; }
    CostEstimate? VMCostEstimate { get; set; }
    CostEstimationOptions K8sCostOptions { get; set; }
    CostEstimationOptions VMCostOptions { get; set; }
    bool K8sCostLoading { get; set; }
    bool VMCostLoading { get; set; }

    // Growth planning
    GrowthSettings K8sGrowthSettings { get; set; }
    GrowthSettings VMGrowthSettings { get; set; }
    GrowthProjection? K8sGrowthProjection { get; set; }
    GrowthProjection? VMGrowthProjection { get; set; }

    // VM Configuration
    Dictionary<EnvironmentType, VMEnvironmentConfig> VMEnvironmentConfigs { get; set; }
    VMEnvironmentConfig? VMManagementEnvConfig { get; set; }
    double VMSystemOverhead { get; set; }

    // UI state
    string ConfigTab { get; set; }
    string K8sResultsTab { get; set; }
    string VMResultsTab { get; set; }
    string DistributionFilter { get; set; }
    string CloudCategory { get; set; }
    string DistributionSearch { get; set; }

    // Modal state
    bool ShowSettings { get; set; }
    bool ShowInfoModal { get; set; }
    bool ShowSaveScenarioModal { get; set; }
    string InfoModalTitle { get; set; }
    string InfoModalContent { get; set; }

    // Reset operations
    void Reset();
    void ResetConfiguration();
    void ResetEnvironmentApps();

    // Events
    event Action? StateChanged;
    void NotifyStateChanged();
}

public class HomePageStateService : IHomePageStateService
{
    private readonly IDistributionService _distributionService;
    private readonly ITechnologyService _technologyService;

    public HomePageStateService(
        IDistributionService distributionService,
        ITechnologyService technologyService)
    {
        _distributionService = distributionService;
        _technologyService = technologyService;
        InitializeDefaults();
    }

    // Events
    public event Action? StateChanged;

    public void NotifyStateChanged() => StateChanged?.Invoke();

    // Wizard navigation
    public int CurrentStep { get; set; } = 1;

    // Platform/Deployment/Technology selection
    public PlatformType? SelectedPlatform { get; set; }
    public DeploymentModel? SelectedDeployment { get; set; }
    public Technology? SelectedTechnology { get; set; }
    public Distribution? SelectedDistribution { get; set; }

    // Cluster configuration
    public ClusterMode? SelectedClusterMode { get; set; } = ClusterMode.MultiCluster;
    public string SingleClusterScope { get; set; } = "Shared";

    // Environment configuration
    public HashSet<EnvironmentType> EnabledEnvironments { get; set; } = new()
    {
        EnvironmentType.Dev,
        EnvironmentType.Test,
        EnvironmentType.Stage,
        EnvironmentType.Prod
    };

    public Dictionary<EnvironmentType, AppConfig> EnvironmentApps { get; set; } = new()
    {
        { EnvironmentType.Dev, new AppConfig { Medium = 70 } },
        { EnvironmentType.Test, new AppConfig { Medium = 70 } },
        { EnvironmentType.Stage, new AppConfig { Medium = 70 } },
        { EnvironmentType.Prod, new AppConfig { Medium = 70 } }
    };

    // Node specs and settings
    public NodeSpecsConfig NodeSpecs { get; set; } = new();
    public K8sHADRConfig K8sHADRConfig { get; set; } = new();

    public Dictionary<EnvironmentType, double> Headroom { get; set; } = new()
    {
        { EnvironmentType.Dev, 33 },
        { EnvironmentType.Test, 33 },
        { EnvironmentType.Stage, 0 },
        { EnvironmentType.Prod, 37.5 }
    };

    public Dictionary<EnvironmentType, int> Replicas { get; set; } = new()
    {
        { EnvironmentType.Dev, 1 },
        { EnvironmentType.Test, 1 },
        { EnvironmentType.Stage, 2 },
        { EnvironmentType.Prod, 3 }
    };

    public double ProdCpuOvercommit { get; set; } = 1.0;
    public double ProdMemoryOvercommit { get; set; } = 1.0;
    public double NonProdCpuOvercommit { get; set; } = 1.0;
    public double NonProdMemoryOvercommit { get; set; } = 1.0;

    // Results
    public K8sSizingResult? K8sResults { get; set; }
    public VMSizingResult? VMResults { get; set; }
    public PricingStepResult? PricingResult { get; set; }
    public List<ValidationWarning>? ValidationWarnings { get; set; }
    public List<ValidationWarning>? VMValidationWarnings { get; set; }

    // Cost estimation
    public CostEstimate? K8sCostEstimate { get; set; }
    public CostEstimate? VMCostEstimate { get; set; }
    public CostEstimationOptions K8sCostOptions { get; set; } = new();
    public CostEstimationOptions VMCostOptions { get; set; } = new();
    public bool K8sCostLoading { get; set; }
    public bool VMCostLoading { get; set; }

    // Growth planning
    public GrowthSettings K8sGrowthSettings { get; set; } = new();
    public GrowthSettings VMGrowthSettings { get; set; } = new();
    public GrowthProjection? K8sGrowthProjection { get; set; }
    public GrowthProjection? VMGrowthProjection { get; set; }

    // VM Configuration
    public Dictionary<EnvironmentType, VMEnvironmentConfig> VMEnvironmentConfigs { get; set; } = new();
    public VMEnvironmentConfig? VMManagementEnvConfig { get; set; }
    public double VMSystemOverhead { get; set; } = 15;

    // UI state
    public string ConfigTab { get; set; } = "apps";
    public string K8sResultsTab { get; set; } = "sizing";
    public string VMResultsTab { get; set; } = "sizing";
    public string DistributionFilter { get; set; } = "on-prem";
    public string CloudCategory { get; set; } = "major";
    public string DistributionSearch { get; set; } = "";

    // Modal state
    public bool ShowSettings { get; set; }
    public bool ShowInfoModal { get; set; }
    public bool ShowSaveScenarioModal { get; set; }
    public string InfoModalTitle { get; set; } = "";
    public string InfoModalContent { get; set; } = "";

    // Wizard navigation methods
    public void GoToStep(int step)
    {
        if (step >= 1 && step <= GetTotalSteps())
        {
            CurrentStep = step;
            NotifyStateChanged();
        }
    }

    public void NextStep()
    {
        if (CanProceed() && CurrentStep < GetTotalSteps())
        {
            CurrentStep++;
            NotifyStateChanged();
        }
    }

    public void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            NotifyStateChanged();
        }
    }

    public int GetTotalSteps()
    {
        // K8s: 1-Platform, 2-Deployment, 3-Technology, 4-Distribution, 5-Configure, 6-Pricing, 7-Results
        // VMs (Mendix): 1-Platform, 2-Deployment, 3-Technology, 4-DeploymentType, 5-Configure, 6-Pricing, 7-Results
        // VMs (Other): 1-Platform, 2-Deployment, 3-Technology, 4-Configure, 5-Pricing, 6-Results
        if (SelectedDeployment == DeploymentModel.Kubernetes)
        {
            return 7;
        }
        // VMs with Mendix have deployment type selection step
        if (SelectedDeployment == DeploymentModel.VMs && SelectedTechnology == Technology.Mendix)
        {
            return 7;
        }
        return 6; // Non-Mendix VMs: skip deployment type step but include pricing
    }

    public string GetStepLabel(int step)
    {
        if (SelectedDeployment == DeploymentModel.Kubernetes)
        {
            return step switch
            {
                1 => "Platform",
                2 => "Deployment",
                3 => "Technology",
                4 => "Distribution",
                5 => "Configure",
                6 => "Pricing",
                7 => "Results",
                _ => ""
            };
        }
        // VMs with Mendix
        if (SelectedTechnology == Technology.Mendix)
        {
            return step switch
            {
                1 => "Platform",
                2 => "Deployment",
                3 => "Technology",
                4 => "Deployment Type",
                5 => "Configure",
                6 => "Pricing",
                7 => "Results",
                _ => ""
            };
        }
        // Non-Mendix VMs
        return step switch
        {
            1 => "Platform",
            2 => "Deployment",
            3 => "Technology",
            4 => "Configure",
            5 => "Pricing",
            6 => "Results",
            _ => ""
        };
    }

    public string? GetStepSelection(int step)
    {
        if (SelectedDeployment == DeploymentModel.Kubernetes)
        {
            return step switch
            {
                1 => SelectedPlatform?.ToString(),
                2 => SelectedDeployment?.ToString(),
                3 => SelectedTechnology?.ToString(),
                4 => SelectedDistribution?.ToString(),
                5 => SelectedClusterMode == ClusterMode.MultiCluster
                    ? "Multi-Cluster"
                    : $"Single ({SingleClusterScope})",
                6 => PricingResult?.HasCosts == true
                    ? "Configured"
                    : (PricingResult?.IsOnPrem == true ? "N/A" : null),
                _ => null
            };
        }
        // VM flow (Mendix)
        if (SelectedTechnology == Technology.Mendix)
        {
            return step switch
            {
                1 => SelectedPlatform?.ToString(),
                2 => "Virtual Machines",
                3 => SelectedTechnology?.ToString(),
                4 => null, // Would need Mendix deployment info
                5 => EnabledEnvironments.Count > 0 ? $"{EnabledEnvironments.Count} env(s)" : null,
                6 => "Configured",
                _ => null
            };
        }
        // VM flow (Non-Mendix)
        return step switch
        {
            1 => SelectedPlatform?.ToString(),
            2 => "Virtual Machines",
            3 => SelectedTechnology?.ToString(),
            4 => EnabledEnvironments.Count > 0 ? $"{EnabledEnvironments.Count} env(s)" : null,
            5 => "Configured",
            _ => null
        };
    }

    public bool CanProceed()
    {
        return CurrentStep switch
        {
            1 => SelectedPlatform.HasValue,
            2 => SelectedDeployment.HasValue,
            3 => SelectedTechnology.HasValue,
            4 => SelectedDeployment == DeploymentModel.Kubernetes
                ? SelectedDistribution.HasValue
                : (SelectedTechnology == Technology.Mendix
                    ? true // Mendix VM deployment type selection
                    : HasAppsConfigured()),
            5 => SelectedDeployment == DeploymentModel.Kubernetes
                ? HasAppsConfigured()
                : true,
            6 => true, // Pricing step - always can proceed
            _ => false
        };
    }

    private bool HasAppsConfigured()
    {
        return EnvironmentApps.Values.Any(app => app.TotalApps > 0);
    }

    // Cluster mode methods
    public bool IsSingleClusterMode() =>
        SelectedClusterMode == ClusterMode.SharedCluster ||
        SelectedClusterMode == ClusterMode.PerEnvironment;

    public int GetClusterCount() => SelectedClusterMode switch
    {
        ClusterMode.MultiCluster => K8sResults?.Environments.Count ?? 0,
        ClusterMode.SharedCluster => 1,
        ClusterMode.PerEnvironment => 1,
        _ => 0
    };

    // Environment methods
    public bool IsEnvironmentEnabled(EnvironmentType env) => EnabledEnvironments.Contains(env);

    public void ToggleEnvironment(EnvironmentType env, bool enabled)
    {
        if (enabled)
            EnabledEnvironments.Add(env);
        else
            EnabledEnvironments.Remove(env);
        NotifyStateChanged();
    }

    public IEnumerable<EnvironmentType> GetVisibleEnvironments()
    {
        if (IsSingleClusterMode() && SingleClusterScope != "Shared")
        {
            // Return only the selected environment for single-environment mode
            if (Enum.TryParse<EnvironmentType>(SingleClusterScope, out var singleEnv))
            {
                return new[] { singleEnv };
            }
        }
        return new[] { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod };
    }

    // Reset methods
    public void Reset()
    {
        InitializeDefaults();
        NotifyStateChanged();
    }

    public void ResetConfiguration()
    {
        ResetEnvironmentApps();
        NodeSpecs = new NodeSpecsConfig();
        K8sHADRConfig = new K8sHADRConfig();
        NotifyStateChanged();
    }

    public void ResetEnvironmentApps()
    {
        EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Medium = 70 } },
            { EnvironmentType.Test, new AppConfig { Medium = 70 } },
            { EnvironmentType.Stage, new AppConfig { Medium = 70 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 70 } }
        };
        NotifyStateChanged();
    }

    private void InitializeDefaults()
    {
        CurrentStep = 1;
        SelectedPlatform = null;
        SelectedDeployment = null;
        SelectedTechnology = null;
        SelectedDistribution = null;
        SelectedClusterMode = ClusterMode.MultiCluster;
        SingleClusterScope = "Shared";

        EnabledEnvironments = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Stage,
            EnvironmentType.Prod
        };

        EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Medium = 70 } },
            { EnvironmentType.Test, new AppConfig { Medium = 70 } },
            { EnvironmentType.Stage, new AppConfig { Medium = 70 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 70 } }
        };

        Headroom = new Dictionary<EnvironmentType, double>
        {
            { EnvironmentType.Dev, 33 },
            { EnvironmentType.Test, 33 },
            { EnvironmentType.Stage, 0 },
            { EnvironmentType.Prod, 37.5 }
        };

        Replicas = new Dictionary<EnvironmentType, int>
        {
            { EnvironmentType.Dev, 1 },
            { EnvironmentType.Test, 1 },
            { EnvironmentType.Stage, 2 },
            { EnvironmentType.Prod, 3 }
        };

        ProdCpuOvercommit = 1.0;
        ProdMemoryOvercommit = 1.0;
        NonProdCpuOvercommit = 1.0;
        NonProdMemoryOvercommit = 1.0;

        NodeSpecs = new NodeSpecsConfig();
        K8sHADRConfig = new K8sHADRConfig();

        K8sResults = null;
        VMResults = null;
        PricingResult = null;
        ValidationWarnings = null;
        VMValidationWarnings = null;

        K8sCostEstimate = null;
        VMCostEstimate = null;
        K8sCostOptions = new CostEstimationOptions();
        VMCostOptions = new CostEstimationOptions();
        K8sCostLoading = false;
        VMCostLoading = false;

        K8sGrowthSettings = new GrowthSettings();
        VMGrowthSettings = new GrowthSettings();
        K8sGrowthProjection = null;
        VMGrowthProjection = null;

        VMEnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();
        VMManagementEnvConfig = null;
        VMSystemOverhead = 15;

        ConfigTab = "apps";
        K8sResultsTab = "sizing";
        VMResultsTab = "sizing";
        DistributionFilter = "on-prem";
        CloudCategory = "major";
        DistributionSearch = "";

        ShowSettings = false;
        ShowInfoModal = false;
        ShowSaveScenarioModal = false;
        InfoModalTitle = "";
        InfoModalContent = "";
    }
}
