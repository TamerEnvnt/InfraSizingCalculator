using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for managing wizard state across Blazor components
/// </summary>
public class WizardStateService : IWizardStateService
{
    // Current wizard state
    public int CurrentStep { get; set; } = 1;
    public PlatformType? SelectedPlatform { get; set; }
    public DeploymentModel? SelectedDeployment { get; set; }
    public Technology? SelectedTechnology { get; set; }
    public Distribution? SelectedDistribution { get; set; }
    public ClusterMode? SelectedClusterMode { get; set; } = ClusterMode.MultiCluster;
    public string SingleClusterScope { get; set; } = "Shared";
    public string ConfigTab { get; set; } = "apps";
    public string DistributionFilter { get; set; } = "all";

    // Environment configuration
    public HashSet<EnvironmentType> EnabledEnvironments { get; } = new()
    {
        EnvironmentType.Dev,
        EnvironmentType.Test,
        EnvironmentType.Stage,
        EnvironmentType.Prod,
        EnvironmentType.DR
    };

    public Dictionary<EnvironmentType, AppConfig> EnvApps { get; } = new()
    {
        [EnvironmentType.Dev] = new AppConfig { Medium = 70 },
        [EnvironmentType.Test] = new AppConfig { Medium = 70 },
        [EnvironmentType.Stage] = new AppConfig { Medium = 70 },
        [EnvironmentType.Prod] = new AppConfig { Medium = 70 },
        [EnvironmentType.DR] = new AppConfig { Medium = 70 }
    };

    public Dictionary<EnvironmentType, double> Headroom { get; } = new()
    {
        [EnvironmentType.Dev] = 20,
        [EnvironmentType.Test] = 20,
        [EnvironmentType.Stage] = 25,
        [EnvironmentType.Prod] = 30,
        [EnvironmentType.DR] = 30
    };

    public Dictionary<EnvironmentType, int> Replicas { get; } = new()
    {
        [EnvironmentType.Dev] = 1,
        [EnvironmentType.Test] = 1,
        [EnvironmentType.Stage] = 2,
        [EnvironmentType.Prod] = 3,
        [EnvironmentType.DR] = 3
    };

    // Overcommit settings
    public double ProdCpuOvercommit { get; set; } = 1.0;
    public double ProdMemoryOvercommit { get; set; } = 1.0;
    public double NonProdCpuOvercommit { get; set; } = 1.0;
    public double NonProdMemoryOvercommit { get; set; } = 1.0;

    // Node specs
    public NodeSpecsConfig NodeSpecs { get; set; } = new();

    // Results
    public K8sSizingResult? Results { get; set; }

    // Events
    public event Action? OnStateChanged;

    public void Reset()
    {
        CurrentStep = 1;
        SelectedPlatform = null;
        SelectedDeployment = null;
        SelectedTechnology = null;
        SelectedDistribution = null;
        SelectedClusterMode = ClusterMode.MultiCluster;
        SingleClusterScope = "Shared";
        ConfigTab = "apps";
        DistributionFilter = "all";
        Results = null;

        EnabledEnvironments.Clear();
        EnabledEnvironments.Add(EnvironmentType.Dev);
        EnabledEnvironments.Add(EnvironmentType.Test);
        EnabledEnvironments.Add(EnvironmentType.Stage);
        EnabledEnvironments.Add(EnvironmentType.Prod);
        EnabledEnvironments.Add(EnvironmentType.DR);

        EnvApps.Clear();
        foreach (var env in EnabledEnvironments)
        {
            EnvApps[env] = new AppConfig { Medium = 70 };
        }

        Headroom[EnvironmentType.Dev] = 20;
        Headroom[EnvironmentType.Test] = 20;
        Headroom[EnvironmentType.Stage] = 25;
        Headroom[EnvironmentType.Prod] = 30;
        Headroom[EnvironmentType.DR] = 30;

        Replicas[EnvironmentType.Dev] = 1;
        Replicas[EnvironmentType.Test] = 1;
        Replicas[EnvironmentType.Stage] = 2;
        Replicas[EnvironmentType.Prod] = 3;
        Replicas[EnvironmentType.DR] = 3;

        ProdCpuOvercommit = 1.0;
        ProdMemoryOvercommit = 1.0;
        NonProdCpuOvercommit = 1.0;
        NonProdMemoryOvercommit = 1.0;

        NodeSpecs = new NodeSpecsConfig();

        NotifyStateChanged();
    }

    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    public int GetTotalSteps()
    {
        if (SelectedDeployment == DeploymentModel.VMs)
            return 5; // Platform -> Deployment -> Tech -> VM Config -> Results
        return 6; // Platform -> Deployment -> Tech -> Distribution -> Config -> Results
    }

    public string GetStepLabel(int step)
    {
        if (SelectedDeployment == DeploymentModel.VMs)
        {
            return step switch
            {
                1 => "Platform",
                2 => "Deployment",
                3 => "Technology",
                4 => "VM Config",
                5 => "Results",
                _ => ""
            };
        }
        return step switch
        {
            1 => "Platform",
            2 => "Deployment",
            3 => "Technology",
            4 => "Distribution",
            5 => "Configuration",
            6 => "Results",
            _ => ""
        };
    }

    public bool CanNavigateToStep(int step)
    {
        return step < CurrentStep;
    }

    public EnvironmentType GetSingleClusterEnvironment()
    {
        return SingleClusterScope switch
        {
            "Dev" => EnvironmentType.Dev,
            "Test" => EnvironmentType.Test,
            "Stage" => EnvironmentType.Stage,
            "Prod" => EnvironmentType.Prod,
            "DR" => EnvironmentType.DR,
            _ => EnvironmentType.Prod
        };
    }

    public bool IsSingleClusterMode()
    {
        return SelectedClusterMode == ClusterMode.SharedCluster ||
               SelectedClusterMode == ClusterMode.PerEnvironment;
    }

    public bool IsProdEnvironment(EnvironmentType env)
    {
        return env == EnvironmentType.Prod || env == EnvironmentType.DR;
    }
}
