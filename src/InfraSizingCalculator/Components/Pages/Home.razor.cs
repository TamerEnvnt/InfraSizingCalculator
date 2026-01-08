using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using InfraSizingCalculator.Components.Layout;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Growth;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Auth;
using InfraSizingCalculator.Services.Interfaces;
using InfraSizingCalculator.Components.Guest;

namespace InfraSizingCalculator.Components.Pages;

public partial class Home : ComponentBase
{
    #region Injected Services

    [Inject] private IK8sSizingService K8sSizingService { get; set; } = default!;
    [Inject] private IVMSizingService VMSizingService { get; set; } = default!;
    [Inject] private IDistributionService DistributionService { get; set; } = default!;
    [Inject] private ITechnologyService TechnologyService { get; set; } = default!;
    [Inject] private IExportService ExportService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ConfigurationSharingService SharingService { get; set; } = default!;
    [Inject] private ValidationRecommendationService ValidationService { get; set; } = default!;
    [Inject] private ICostEstimationService CostEstimationService { get; set; } = default!;
    [Inject] private IPricingService PricingService { get; set; } = default!;
    [Inject] private IScenarioService ScenarioService { get; set; } = default!;
    [Inject] private IGrowthPlanningService GrowthPlanningService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IPricingSettingsService PricingSettingsService { get; set; } = default!;
    [Inject] private ITierConfigurationService TierConfigService { get; set; } = default!;
    [Inject] private IHomePageStateService StateService { get; set; } = default!;
    [Inject] private IHomePageCalculationService CalculationService { get; set; } = default!;
    [Inject] private IHomePageCostService CostService { get; set; } = default!;
    [Inject] private IHomePageDistributionService DistributionDisplayService { get; set; } = default!;
    [Inject] private IInfoContentService InfoContentService { get; set; } = default!;
    [Inject] private IHomePageUIHelperService UIHelperService { get; set; } = default!;
    [Inject] private IHomePageCloudAlternativeService CloudAlternativeService { get; set; } = default!;
    [Inject] private IHomePageVMService VMService { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private IAppStateService AppState { get; set; } = default!;

    #endregion

    #region Landing Page State

    // Track which view to show: landing or dashboard
    private bool _isAuthenticated;
    private bool _isInitialized;

    #endregion

    // Wizard state
    private int currentStep = 1;
    private PlatformType? selectedPlatform;
    private DeploymentModel? selectedDeployment;
    private Technology? selectedTechnology;
    private Distribution? selectedDistribution;
    private ClusterMode? selectedClusterMode = ClusterMode.MultiCluster;
    private string distributionFilter = "on-prem"; // Default to Self-Managed tab
    private string cloudCategory = "major"; // Cloud sub-category filter (no "all" option)
    private string distributionSearch = "";
    private string configTab = "apps"; // apps, nodes, settings, mendix
    private EnvironmentType selectedSingleEnvironment = EnvironmentType.Prod;
    private string singleClusterScope = "Shared"; // Options: Shared, Dev, Test, Stage, Prod, DR
    private EnvironmentType selectedNodeSpecsEnv = EnvironmentType.Prod; // For multi-cluster node specs tabs

    // Modal state
    private bool showSettings = false;
    private bool showInfoModal = false;
    private bool showSaveScenarioModal = false;
    private bool showLimitedExportModal = false;
    private string settingsTab = "infra";
    private string infoModalTitle = "";
    private string infoModalContent = "";

    // Environment app counts (per environment)
    private Dictionary<EnvironmentType, AppConfig> envApps = new()
    {
        { EnvironmentType.Dev, new AppConfig { Medium = 70 } },
        { EnvironmentType.Test, new AppConfig { Medium = 70 } },
        { EnvironmentType.Stage, new AppConfig { Medium = 70 } },
        { EnvironmentType.Prod, new AppConfig { Medium = 70 } }
    };

    private HashSet<EnvironmentType> enabledEnvironments = new()
    {
        EnvironmentType.Dev,
        EnvironmentType.Test,
        EnvironmentType.Stage,
        EnvironmentType.Prod
    };

    // Node specs (editable on main page, loaded from settings defaults)
    private NodeSpecsConfig nodeSpecs = new();
    private Models.K8sHADRConfig k8sHADRConfig = new();

    // Resource configuration (editable on main page, loaded from settings defaults)
    private double prodCpuOvercommit = 1.0;
    private double prodMemoryOvercommit = 1.0;
    private double nonProdCpuOvercommit = 1.0;
    private double nonProdMemoryOvercommit = 1.0;

    // Headroom per environment (editable on main page)
    // Matches JS defaults: Dev/Test=33%, Stage=0%, Prod=37.5%
    private Dictionary<EnvironmentType, double> headroom = new()
    {
        { EnvironmentType.Dev, 33 },
        { EnvironmentType.Test, 33 },
        { EnvironmentType.Stage, 0 },
        { EnvironmentType.Prod, 37.5 }
    };

    // Replicas per environment (editable on main page)
    private Dictionary<EnvironmentType, int> replicas = new()
    {
        { EnvironmentType.Dev, 1 },
        { EnvironmentType.Test, 1 },
        { EnvironmentType.Stage, 2 },
        { EnvironmentType.Prod, 3 }
    };

    // Mendix-specific settings (editable on main page when Mendix selected)
    private int mendixOperatorReplicas = 2;

    // Mendix deployment configuration (for LowCode flow)
    private MendixDeploymentCategory? mendixDeploymentCategory = null;
    private MendixCloudType mendixCloudType = MendixCloudType.SaaS;
    private MendixPrivateCloudProvider? mendixPrivateCloudProvider = null;
    private MendixOtherDeployment? mendixOtherDeployment = null;
    private bool mendixIsUnlimitedApps = true;
    private int mendixNumberOfApps = 1;
    private int mendixNumberOfEnvironments = 3;
    private int mendixInternalUsers = 100;
    private int mendixExternalUsers = 0;
    private string mendixResourcePackSize = "M";

    // Settings (defaults only)
    private UICalculatorSettings settings = new();

    // On-prem pricing defaults
    private OnPremPricing onPremPricing = new();

    // Mendix pricing settings (from official pricebook)
    private MendixPricingSettings mendixPricingSettings = new();

    // OutSystems pricing configuration
    private OutSystemsEdition outsystemsEdition = OutSystemsEdition.Standard;
    private int outsystemsApplicationObjects = 150;  // Default to 1 AO pack (typical medium app)
    private int outsystemsInternalUsers = 100;       // Included in Standard
    private int outsystemsExternalSessions = 0;      // Monthly sessions for external users
    private OutSystemsPricingSettings outsystemsPricingSettings = new();
    private string outsystemsActiveAccordion = "edition"; // "edition" or "licensing" - only one open at a time

    // Results
    private K8sSizingResult? results;
    private VMSizingResult? vmResults;

    // Pricing step result
    private PricingStepResult? pricingResult;

    // Validation warnings
    private List<ValidationWarning>? validationWarnings;
    private List<ValidationWarning>? vmValidationWarnings;

    // Share functionality
    private bool showShareCopied = false;

    // Cost estimation state
    private string k8sResultsTab = "sizing"; // "sizing", "cost", or "insights"
    private string vmResultsTab = "sizing"; // "sizing", "cost", or "insights"
    private string? expandedRoleBreakdownEnv = null; // Accordion state for role breakdown - only one env open at a time
    private string costSubTab = "onprem"; // "onprem" or "cloud" - for on-prem distributions
    private string selectedCloudAlt = ""; // Selected cloud alternative key for comparison
    private CostEstimate? k8sCostEstimate;
    private CostEstimate? vmCostEstimate;
    private CostEstimationOptions k8sCostOptions = new();
    private CostEstimationOptions vmCostOptions = new();
    private bool k8sCostLoading = false;
    private bool vmCostLoading = false;
    private bool k8sPricingOptionsOpen = false;
    private bool k8sCostNotesOpen = false;
    private bool vmPricingOptionsOpen = false;
    private bool vmCostNotesOpen = false;
    private string vmCostAccordion = "current"; // "current", "alternative", or "" (both collapsed)

    // VM Configuration state
    private Dictionary<EnvironmentType, VMEnvironmentConfig> vmEnvironmentConfigs = new();
    private VMEnvironmentConfig? vmManagementEnvConfig; // e.g., OutSystems LifeTime
    private double vmSystemOverhead = 15;

    // Growth planning state
    private string k8sGrowthSubTab = "settings"; // "settings", "projection", or "timeline"
    private string vmGrowthSubTab = "settings"; // "settings", "projection", or "timeline"
    private GrowthSettings k8sGrowthSettings = new();
    private GrowthSettings vmGrowthSettings = new();
    private GrowthProjection? k8sGrowthProjection;
    private GrowthProjection? vmGrowthProjection;

    #region Lifecycle Methods

    /// <summary>
    /// Initialize auth state to determine which view to show:
    /// - Guest landing (not auth, no scenario)
    /// - Guest results (not auth, started scenario)
    /// - Auth landing (auth, viewing landing)
    /// - Auth dashboard (auth, not viewing landing)
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var currentUser = await AuthService.GetCurrentUserAsync();
        _isAuthenticated = currentUser != null;

        // For new sessions, start on landing page
        if (!_isInitialized)
        {
            AppState.IsViewingLanding = true;
            _isInitialized = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadSettingsFromStorage();
            await LoadFromSharedUrlAsync();
            StateHasChanged();
        }
    }

    #endregion

    #region Landing Page Handlers

    /// <summary>
    /// Called when guest user clicks "Create New Scenario" on LandingGuest.
    /// Sets HasStartedScenario=true to show the dashboard/results view.
    /// </summary>
    private void HandleGuestStartScenario()
    {
        AppState.HasStartedScenario = true;
        AppState.IsViewingLanding = false;
        AppState.NotifyStateChanged();
        StateHasChanged();
    }

    /// <summary>
    /// Called when authenticated user clicks "Create New Scenario" on LandingAuth.
    /// Sets IsViewingLanding=false to show the dashboard with empty scenario.
    /// </summary>
    private void HandleAuthCreateScenario()
    {
        AppState.CurrentScenarioId = null; // New scenario
        AppState.IsViewingLanding = false;
        AppState.NotifyStateChanged();
        StateHasChanged();
    }

    /// <summary>
    /// Called when authenticated user clicks on a recent scenario from LandingAuth.
    /// Loads the scenario and shows the dashboard.
    /// </summary>
    private async Task HandleAuthLoadScenario(Guid scenarioId)
    {
        AppState.CurrentScenarioId = scenarioId;
        AppState.IsViewingLanding = false;
        AppState.NotifyStateChanged();

        // TODO: Actually load the scenario data from database
        await Task.CompletedTask;
        StateHasChanged();
    }

    /// <summary>
    /// Helper to determine if we should show the landing page.
    /// Returns true when user hasn't started working on a scenario yet.
    /// </summary>
    private bool ShouldShowLanding()
    {
        if (!_isAuthenticated)
        {
            // Guest: Show landing if they haven't started a scenario
            return !AppState.HasStartedScenario;
        }
        else
        {
            // Auth: Show landing if explicitly viewing landing (no scenario selected)
            return AppState.IsViewingLanding;
        }
    }

    #endregion

    private async Task LoadFromSharedUrlAsync()
    {
        try
        {
            var sharedConfig = await SharingService.ParseFromUrlAsync();
            if (sharedConfig == null) return;

            if (sharedConfig.Type == "k8s" && sharedConfig.K8sInput != null)
            {
                await ApplyK8sSharedConfig(sharedConfig.K8sInput);
            }
            else if (sharedConfig.Type == "vm" && sharedConfig.VMInput != null)
            {
                await ApplyVMSharedConfig(sharedConfig.VMInput);
            }

            // Clear the URL parameter after loading
            await SharingService.ClearUrlParameterAsync();
        }
        catch
        {
            // Silently fail - shared config is optional
        }
    }

    private async Task ApplyK8sSharedConfig(K8sSizingInput input)
    {
        // Set platform based on technology
        var techConfig = TechnologyService.GetConfig(input.Technology);
        selectedPlatform = techConfig.IsLowCode ? PlatformType.LowCode : PlatformType.Native;

        // Set selections
        selectedDeployment = DeploymentModel.Kubernetes;
        selectedTechnology = input.Technology;
        selectedDistribution = input.Distribution;
        selectedClusterMode = input.ClusterMode;

        // Set enabled environments (DR is no longer a separate environment - it's a config option)
        enabledEnvironments = input.EnabledEnvironments ?? new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod
        };
        // Filter out any legacy DR entries from shared configs
        enabledEnvironments.Remove(EnvironmentType.DR);

        // Set app counts per environment
        if (input.EnvironmentApps != null)
        {
            foreach (var kvp in input.EnvironmentApps.Where(x => x.Key != EnvironmentType.DR))
            {
                envApps[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            // Use ProdApps/NonProdApps as fallback
            envApps[EnvironmentType.Prod] = input.ProdApps;
            envApps[EnvironmentType.Dev] = input.NonProdApps;
            envApps[EnvironmentType.Test] = input.NonProdApps;
            envApps[EnvironmentType.Stage] = input.NonProdApps;
        }

        // Set headroom
        if (input.Headroom != null)
        {
            headroom[EnvironmentType.Dev] = input.Headroom.Dev;
            headroom[EnvironmentType.Test] = input.Headroom.Test;
            headroom[EnvironmentType.Stage] = input.Headroom.Stage;
            headroom[EnvironmentType.Prod] = input.Headroom.Prod;
        }

        // Set replicas
        if (input.Replicas != null)
        {
            replicas[EnvironmentType.Dev] = input.Replicas.NonProd;
            replicas[EnvironmentType.Test] = input.Replicas.NonProd;
            replicas[EnvironmentType.Stage] = input.Replicas.Stage;
            replicas[EnvironmentType.Prod] = input.Replicas.Prod;
        }

        // Set overcommit
        if (input.ProdOvercommit != null)
        {
            prodCpuOvercommit = input.ProdOvercommit.Cpu;
            prodMemoryOvercommit = input.ProdOvercommit.Memory;
        }
        if (input.NonProdOvercommit != null)
        {
            nonProdCpuOvercommit = input.NonProdOvercommit.Cpu;
            nonProdMemoryOvercommit = input.NonProdOvercommit.Memory;
        }

        // Load node specs from distribution defaults
        LoadNodeSpecsFromSettings();

        // Calculate results automatically (skip pricing step for shared configs)
        currentStep = 7;
        await CalculateK8s();
    }

    private async Task ApplyVMSharedConfig(VMSizingInput input)
    {
        // Set platform based on technology
        var techConfig = TechnologyService.GetConfig(input.Technology);
        selectedPlatform = techConfig.IsLowCode ? PlatformType.LowCode : PlatformType.Native;

        // Set selections
        selectedDeployment = DeploymentModel.VMs;
        selectedTechnology = input.Technology;

        // Set enabled environments (DR is no longer a separate environment - it's a config option)
        enabledEnvironments = input.EnabledEnvironments ?? new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod
        };
        // Filter out any legacy DR entries from shared configs
        enabledEnvironments.Remove(EnvironmentType.DR);

        // Set VM configs (filter out any legacy DR entries)
        if (input.EnvironmentConfigs != null)
        {
            vmEnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>(
                input.EnvironmentConfigs.Where(x => x.Key != EnvironmentType.DR));
        }

        // Set system overhead
        vmSystemOverhead = input.SystemOverheadPercent;

        // Calculate results automatically
        currentStep = 5;
        await CalculateVM();
    }

    // Step management
    private int GetTotalSteps()
    {
        // K8s: 1-Platform, 2-Deployment, 3-Technology, 4-Distribution, 5-Configure, 6-Pricing, 7-Results
        // VMs (Mendix): 1-Platform, 2-Deployment, 3-Technology, 4-DeploymentType, 5-Configure, 6-Pricing, 7-Results
        // VMs (Other): 1-Platform, 2-Deployment, 3-Technology, 4-Configure, 5-Pricing, 6-Results
        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            return 7;
        }
        // VMs with Mendix have deployment type selection step
        if (selectedDeployment == DeploymentModel.VMs && selectedTechnology == Technology.Mendix)
        {
            return 7;
        }
        return 6; // Non-Mendix VMs: skip deployment type step but include pricing
    }

    private int GetConfigStep()
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 5;
        // Mendix VMs: config at step 5 (after deployment type at step 4)
        if (selectedTechnology == Technology.Mendix)
            return 5;
        // Non-Mendix VMs: config at step 4 (no deployment type step)
        return 4;
    }

    private int GetPricingStep()
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 6;
        // Mendix VMs: pricing at step 6 (after config at step 5)
        if (selectedTechnology == Technology.Mendix)
            return 6;
        // Non-Mendix VMs: pricing at step 5 (after config at step 4)
        return 5;
    }

    private int GetResultsStep()
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 7;
        // Mendix VMs: results at step 7 (after pricing at step 6)
        if (selectedTechnology == Technology.Mendix)
            return 7;
        // Non-Mendix VMs: results at step 6 (after pricing at step 5)
        return 6;
    }

    private string GetStepLabel(int step)
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            return step switch { 1 => "Platform", 2 => "Deployment", 3 => "Technology", 4 => "Distribution", 5 => "Configure", 6 => "Pricing", 7 => "Results", _ => "" };
        }
        // VMs with Mendix
        if (selectedTechnology == Technology.Mendix)
        {
            return step switch { 1 => "Platform", 2 => "Deployment", 3 => "Technology", 4 => "Deployment Type", 5 => "Configure", 6 => "Pricing", 7 => "Results", _ => "" };
        }
        // Non-Mendix VMs
        return step switch { 1 => "Platform", 2 => "Deployment", 3 => "Technology", 4 => "Configure", 5 => "Pricing", 6 => "Results", _ => "" };
    }

    private string? GetStepSelection(int step)
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            return step switch
            {
                1 => selectedPlatform?.ToString(),
                2 => selectedDeployment?.ToString(),
                3 => selectedTechnology?.ToString(),
                4 => GetDistributionDisplayName(),
                5 => selectedClusterMode == ClusterMode.MultiCluster ? "Multi-Cluster" : $"Single ({singleClusterScope})",
                6 => pricingResult?.HasCosts == true ? "Configured" : (pricingResult?.IsOnPrem == true ? "N/A" : null),
                _ => null
            };
        }
        // VM flow (Mendix)
        if (selectedTechnology == Technology.Mendix)
        {
            return step switch
            {
                1 => selectedPlatform?.ToString(),
                2 => "Virtual Machines",
                3 => selectedTechnology?.ToString(),
                4 => GetMendixVMDeploymentDisplayName(),
                5 => enabledEnvironments.Count > 0 ? $"{enabledEnvironments.Count} env(s)" : null,
                6 => "Configured",
                _ => null
            };
        }
        // VM flow (Non-Mendix)
        return step switch
        {
            1 => selectedPlatform?.ToString(),
            2 => "Virtual Machines",
            3 => selectedTechnology?.ToString(),
            4 => enabledEnvironments.Count > 0 ? $"{enabledEnvironments.Count} env(s)" : null,
            5 => "Configured",
            _ => null
        };
    }

    private string? GetDistributionDisplayName() =>
        UIHelperService.GetDistributionDisplayName(
            selectedTechnology,
            selectedDistribution,
            mendixDeploymentCategory,
            mendixCloudType,
            mendixPrivateCloudProvider);

    private string? GetMendixVMDeploymentDisplayName() =>
        UIHelperService.GetMendixVMDeploymentDisplayName(mendixOtherDeployment);

    // Cluster mode helpers - delegate to UIHelperService
    private string GetClusterModeIcon() =>
        UIHelperService.GetClusterModeIcon(selectedClusterMode, singleClusterScope);

    private string GetClusterModeDescription() =>
        UIHelperService.GetClusterModeDescription(selectedClusterMode, singleClusterScope, results?.Environments.Count ?? 0);

    private string GetClusterModeBannerClass() =>
        UIHelperService.GetClusterModeBannerClass(selectedClusterMode, singleClusterScope);

    private int GetClusterCount() => selectedClusterMode switch
    {
        ClusterMode.MultiCluster => results?.Environments.Count ?? 0,
        ClusterMode.SharedCluster => 1,
        ClusterMode.PerEnvironment => 1,
        _ => 0
    };

    // Single Cluster mode helpers
    private bool IsSingleClusterMode() => selectedClusterMode == ClusterMode.SharedCluster || selectedClusterMode == ClusterMode.PerEnvironment;

    private bool IsManagedControlPlane() =>
        DistributionDisplayService.IsManagedControlPlane(selectedDistribution);

    private bool IsOnPremDistribution()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = selectedTechnology,
            SelectedDeployment = selectedDeployment,
            SelectedDistribution = selectedDistribution,
            MendixDeploymentCategory = mendixDeploymentCategory,
            MendixPrivateCloudProvider = mendixPrivateCloudProvider
        };
        return DistributionDisplayService.IsOnPremDistribution(context);
    }

    private bool IsOpenShiftDistribution() =>
        DistributionDisplayService.IsOpenShiftDistribution(selectedDistribution);

    private bool IsMendixCloudDeployment()
    {
        // True only for Mendix Cloud SaaS or Dedicated (not Private Cloud or Other K8s)
        return selectedTechnology == Technology.Mendix
            && selectedDeployment == DeploymentModel.Kubernetes
            && mendixDeploymentCategory == MendixDeploymentCategory.Cloud;
    }

    private void SwitchToOnPremTab()
    {
        costSubTab = "onprem";
    }

    private List<CloudAlternativeInfo> GetCloudAlternativesForBreakdown() =>
        CloudAlternativeService.GetCloudAlternativesForBreakdown(results, selectedDistribution, CostService, DistributionDisplayService);

    private void SelectCloudAlternative(string key)
    {
        selectedCloudAlt = selectedCloudAlt == key ? "" : key;
    }

    private void ApplyCloudAlternative(CloudAlternativeInfo alt)
    {
        // Change the distribution to the cloud alternative
        selectedDistribution = alt.TargetDistribution;
        costSubTab = "onprem"; // Switch back to main tab
        selectedCloudAlt = "";

        // Re-calculate sizing with new distribution
        if (results != null)
        {
            // Trigger recalculation
            StateHasChanged();
        }
    }

    private bool IsSharedClusterMode() => IsSingleClusterMode() && singleClusterScope == "Shared";

    private EnvironmentType GetSingleClusterEnvironment() => singleClusterScope switch
    {
        "Dev" => EnvironmentType.Dev,
        "Test" => EnvironmentType.Test,
        "Stage" => EnvironmentType.Stage,
        "Prod" => EnvironmentType.Prod,
        "DR" => EnvironmentType.DR,
        _ => EnvironmentType.Prod // Default for "Shared" - will use all envs
    };

    private IEnumerable<EnvironmentType> GetSingleClusterEnvironments()
    {
        if (singleClusterScope == "Shared")
        {
            return new[] { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod };
        }
        return new[] { GetSingleClusterEnvironment() };
    }

    private void BackToConfiguration()
    {
        currentStep = 5;
    }

    private void BackToPricing()
    {
        // Go back to pricing step (varies by deployment/technology)
        // K8s: step 6, Mendix VMs: step 6, OutSystems VMs: step 5
        currentStep = GetPricingStep();
    }

    // Pricing step helpers
    private int GetTotalNodesForPricing()
    {
        // Estimate total nodes from app configuration
        var totalApps = 0;
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var apps))
            {
                totalApps += apps.Small + apps.Medium + apps.Large + apps.XLarge;
            }
        }
        // Rough estimate: 1 node per 3 apps minimum
        return Math.Max(3, (int)Math.Ceiling(totalApps / 3.0));
    }

    private int GetTotalCoresForPricing()
    {
        // Estimate total cores based on app sizes
        var totalCores = 0;
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var apps))
            {
                totalCores += apps.Small * 2 + apps.Medium * 4 + apps.Large * 8 + apps.XLarge * 16;
            }
        }
        return Math.Max(8, totalCores); // Minimum 8 cores
    }

    private int GetTotalRamForPricing()
    {
        // Estimate total RAM based on app sizes
        var totalRam = 0;
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var apps))
            {
                totalRam += apps.Small * 4 + apps.Medium * 8 + apps.Large * 16 + apps.XLarge * 32;
            }
        }
        return Math.Max(16, totalRam); // Minimum 16 GB
    }

    private void HandlePricingConfigured(PricingStepResult result)
    {
        pricingResult = result;
        StateHasChanged();
    }

    private async Task HandleApplyAlternative(Distribution newDistribution)
    {
        // Change distribution only - keep technology, apps, and environment config
        selectedDistribution = newDistribution;

        // Recalculate results with the new distribution
        await CalculateK8s();

        // Navigate to results step to see the updated sizing
        currentStep = 7;
        StateHasChanged();
    }

    private void OpenSaveScenarioModal()
    {
        showSaveScenarioModal = true;
        StateHasChanged();
    }

    private void StartNewCalculation()
    {
        results = null;
        currentStep = 1;
        ResetConfiguration();
    }

    // Selection handlers with auto-advance and scroll - CASCADE RESET on step change
    private async Task SelectPlatformAsync(PlatformType platform)
    {
        selectedPlatform = platform;
        // CASCADE RESET: Reset all subsequent selections
        selectedDeployment = null;
        selectedTechnology = null;
        selectedDistribution = null;
        selectedClusterMode = ClusterMode.MultiCluster;
        nodeSpecs = new NodeSpecsConfig();
        results = null;
        currentStep = 2;
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step2");
    }

    private async Task SelectDeploymentAsync(DeploymentModel deployment)
    {
        selectedDeployment = deployment;
        // CASCADE RESET: Reset all subsequent selections
        selectedTechnology = null;
        selectedDistribution = null;
        selectedClusterMode = ClusterMode.MultiCluster;
        nodeSpecs = new NodeSpecsConfig();
        results = null;
        currentStep = 3;
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step3");
    }

    private async Task SelectTechnologyAsync(Technology technology)
    {
        selectedTechnology = technology;
        // CASCADE RESET: Reset distribution and node specs
        selectedDistribution = null;
        selectedClusterMode = ClusterMode.MultiCluster;
        nodeSpecs = new NodeSpecsConfig();
        results = null;
        vmResults = null;

        // Adjust default app counts based on platform type
        var techConfig = TechnologyService.GetConfig(technology);
        if (techConfig.PlatformType == PlatformType.LowCode)
        {
            // Low-code platforms typically have fewer, larger apps
            // Default to 10 medium apps instead of 70
            foreach (var env in enabledEnvironments)
            {
                envApps[env] = new AppConfig { Medium = 10 };
            }
        }
        else
        {
            // Native platforms: use standard defaults
            foreach (var env in enabledEnvironments)
            {
                envApps[env] = new AppConfig { Medium = 70 };
            }
        }

        // Reset VM configs to use new technology-specific roles
        OnTechnologyChanged();

        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            currentStep = 4;
            StateHasChanged();
            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("scrollToElement", "step4");
        }
        else
        {
            // VM mode
            if (technology == Technology.Mendix)
            {
                // Mendix VMs need deployment type selection first (step 4)
                currentStep = 4;
                StateHasChanged();
                await Task.Delay(100);
                await JSRuntime.InvokeVoidAsync("scrollToElement", "step4");
            }
            else
            {
                // Non-Mendix VMs go directly to configuration
                configTab = "roles";
                currentStep = GetConfigStep();
                StateHasChanged();
                await Task.Delay(100);
                await JSRuntime.InvokeVoidAsync("scrollToElement", "step5");
            }
        }
    }

    private async Task SelectDistributionAsync(Distribution distribution)
    {
        selectedDistribution = distribution;
        // Reset cluster mode and results when distribution changes
        selectedClusterMode = ClusterMode.MultiCluster;
        results = null;

        // Load node specs from settings defaults for this distribution
        LoadNodeSpecsFromSettings();

        currentStep = GetConfigStep();
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step5");
    }

    // ==================== Mendix Deployment Selection ====================

    private void SelectMendixCategory(MendixDeploymentCategory category)
    {
        mendixDeploymentCategory = category;
        // Reset sub-selections
        mendixPrivateCloudProvider = null;
        mendixOtherDeployment = null;
        mendixCloudType = MendixCloudType.SaaS;
    }

    private async Task SelectMendixCloudType(MendixCloudType cloudType)
    {
        mendixCloudType = cloudType;
        // Map Mendix Cloud to a suitable distribution for sizing
        // Use a cloud-managed K8s for the sizing calculation
        selectedDistribution = Distribution.EKS; // Default to EKS for cloud
        LoadNodeSpecsFromSettings();
        currentStep = GetConfigStep();
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step5");
    }

    private async Task SelectMendixPrivateCloudProvider(MendixPrivateCloudProvider provider)
    {
        mendixPrivateCloudProvider = provider;
        mendixOtherDeployment = null;

        // Map Mendix provider to distribution for sizing
        selectedDistribution = provider switch
        {
            MendixPrivateCloudProvider.Azure => Distribution.AKS,
            MendixPrivateCloudProvider.EKS => Distribution.EKS,
            MendixPrivateCloudProvider.AKS => Distribution.AKS,
            MendixPrivateCloudProvider.GKE => Distribution.GKE,
            MendixPrivateCloudProvider.OpenShift => Distribution.OpenShift,
            MendixPrivateCloudProvider.Rancher => Distribution.Rancher,
            MendixPrivateCloudProvider.K3s => Distribution.K3s,
            MendixPrivateCloudProvider.GenericK8s => Distribution.Kubernetes,
            MendixPrivateCloudProvider.Docker => Distribution.Kubernetes, // Use generic K8s for sizing
            _ => Distribution.Kubernetes
        };

        LoadNodeSpecsFromSettings();
        currentStep = GetConfigStep();
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step5");
    }

    private async Task SelectMendixOtherDeployment(MendixOtherDeployment deployment)
    {
        mendixOtherDeployment = deployment;
        mendixPrivateCloudProvider = null;

        // For VM-based deployments, we'll still use K8s sizing as approximation
        selectedDistribution = Distribution.Kubernetes;
        LoadNodeSpecsFromSettings();
        currentStep = GetConfigStep();
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step5");
    }

    // Mendix VM deployment selection (for VMs path)
    private async Task SelectMendixVMDeployment(MendixOtherDeployment deployment)
    {
        mendixOtherDeployment = deployment;
        mendixDeploymentCategory = MendixDeploymentCategory.Other;
        mendixPrivateCloudProvider = null;

        // Go to VM configuration step
        configTab = "roles";
        currentStep = 5;
        StateHasChanged();
        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "step5");
    }

    private string GetMendixVMDeploymentName() =>
        UIHelperService.GetMendixVMDeploymentName(mendixOtherDeployment);

    private (decimal perApp, decimal unlimited) GetMendixVMDeploymentPricing()
    {
        var mendixPricing = PricingSettingsService.GetMendixPricingSettings();
        return mendixOtherDeployment switch
        {
            MendixOtherDeployment.Server => (mendixPricing.ServerPerAppPricePerYear, mendixPricing.ServerUnlimitedAppsPricePerYear),
            MendixOtherDeployment.StackIT => (mendixPricing.StackITPerAppPricePerYear, mendixPricing.StackITUnlimitedAppsPricePerYear),
            MendixOtherDeployment.SapBtp => (mendixPricing.SapBtpPerAppPricePerYear, mendixPricing.SapBtpUnlimitedAppsPricePerYear),
            _ => (mendixPricing.ServerPerAppPricePerYear, mendixPricing.ServerUnlimitedAppsPricePerYear)
        };
    }

    private void ShowMendixDeploymentInfo()
    {
        infoModalTitle = "Mendix Deployment Options";
        infoModalContent = @"
            <h3>Mendix Cloud</h3>
            <p><strong>SaaS:</strong> Multi-tenant managed service. Pay per resource pack based on app requirements. Best for most use cases.</p>
            <p><strong>Dedicated:</strong> Single-tenant infrastructure for enterprise customers with strict compliance requirements.</p>

            <h3>Private Cloud (Officially Supported)</h3>
            <p>Deploy Mendix on your own Kubernetes infrastructure with official support:</p>
            <ul>
                <li><strong>Mendix on Azure:</strong> Managed Mendix deployment on Microsoft Azure</li>
                <li><strong>EKS, AKS, GKE:</strong> Self-managed Mendix for Private Cloud on major cloud Kubernetes services</li>
                <li><strong>OpenShift:</strong> Deploy on Red Hat OpenShift with official support</li>
            </ul>

            <h3>Other Kubernetes (Manual Setup)</h3>
            <p>Unsupported Kubernetes distributions requiring manual Mendix operator setup:</p>
            <ul>
                <li><strong>Rancher:</strong> SUSE Rancher managed Kubernetes</li>
                <li><strong>K3s:</strong> Lightweight Kubernetes</li>
                <li><strong>Generic K8s:</strong> Any other Kubernetes distribution</li>
            </ul>

            <h3>VM Deployments</h3>
            <p>For non-Kubernetes deployments, select <strong>VMs</strong> in Step 2:</p>
            <ul>
                <li><strong>Server (VMs/Docker):</strong> Traditional deployment on virtual machines or containers</li>
                <li><strong>StackIT:</strong> Schwarz IT cloud platform</li>
                <li><strong>SAP BTP:</strong> SAP Business Technology Platform</li>
            </ul>
        ";
        showInfoModal = true;
    }

    private void SetDistroFilter(string filter)
    {
        distributionFilter = filter;
        if (filter == "cloud" && string.IsNullOrEmpty(cloudCategory))
        {
            cloudCategory = "major"; // Default to Major category
        }
    }

    private void SelectClusterMode(ClusterMode mode)
    {
        selectedClusterMode = mode;
        results = null;
    }

    private void GoBack()
    {
        if (currentStep > 1)
        {
            currentStep--;
            results = null;
        }
    }

    // Wizard navigation
    private void GoToStep(int step)
    {
        // Only allow going to completed steps (going back)
        if (step < currentStep)
        {
            currentStep = step;
            results = null;
        }
    }

    private void PreviousStep()
    {
        if (currentStep > 1)
        {
            currentStep--;
            results = null;
        }
    }

    private async Task NextStep()
    {
        if (!CanProceed()) return;

        if (IsLastStep())
        {
            // Go to results step (varies by deployment/technology)
            // K8s: step 7, Mendix VMs: step 7, OutSystems VMs: step 6
            currentStep = GetResultsStep();
        }
        else
        {
            // Calculate at appropriate steps
            if (selectedDeployment == DeploymentModel.Kubernetes && currentStep == 5)
            {
                // K8s: Calculate before going to pricing step
                await CalculateK8s();
            }
            else if (selectedDeployment == DeploymentModel.VMs && currentStep == GetConfigStep())
            {
                // VMs: Calculate when leaving config step (step 4 for non-Mendix, step 5 for Mendix)
                await CalculateVM();
            }
            currentStep++;
        }
    }

    private bool CanProceed()
    {
        // Steps 1-3 are the same for all flows
        if (currentStep == 1) return selectedPlatform != null;
        if (currentStep == 2) return selectedDeployment != null;
        if (currentStep == 3) return selectedTechnology != null;

        // Pricing step always allows proceed
        if (currentStep == GetPricingStep()) return true;

        // Config step requires roles/apps configured
        if (currentStep == GetConfigStep())
        {
            if (selectedDeployment == DeploymentModel.VMs)
            {
                return HasVMRolesConfigured();
            }
            // K8s: need cluster mode and apps configured
            return selectedClusterMode != null && HasAppsConfigured();
        }

        // Step 4 for specific flows
        if (currentStep == 4)
        {
            // K8s step 4: need distribution selected
            if (selectedDeployment == DeploymentModel.Kubernetes)
            {
                return selectedDistribution != null;
            }
            // Mendix VM step 4: need deployment type selected
            if (selectedDeployment == DeploymentModel.VMs && selectedTechnology == Technology.Mendix)
            {
                return mendixOtherDeployment != null;
            }
        }

        return false;
    }

    private bool HasVMRolesConfigured()
    {
        return enabledEnvironments.Any(env =>
            vmEnvironmentConfigs.TryGetValue(env, out var config) &&
            config.Roles.Count > 0);
    }

    private bool IsLastStep()
    {
        // Last configurable step (before results) is the pricing step
        // K8s: step 6, Mendix VMs: step 6, OutSystems VMs: step 5
        return currentStep == GetPricingStep();
    }

    private bool HasAppsConfigured()
    {
        return enabledEnvironments.Any(env =>
            envApps.TryGetValue(env, out var config) &&
            (config.Small > 0 || config.Medium > 0 || config.Large > 0 || config.XLarge > 0));
    }

    private void ResetWizard()
    {
        currentStep = 1;
        selectedPlatform = null;
        selectedDeployment = null;
        selectedTechnology = null;
        selectedDistribution = null;
        selectedClusterMode = ClusterMode.MultiCluster;
        results = null;
        vmResults = null;
        vmEnvironmentConfigs.Clear();
        ResetEnvApps();
    }

    private void ResetConfiguration()
    {
        ResetEnvApps();
        results = null;
        vmResults = null;
    }

    private void ResetEnvApps()
    {
        foreach (var env in envApps.Keys.ToList())
        {
            envApps[env] = new AppConfig { Medium = 70 };
        }
        enabledEnvironments = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod
        };
    }

    // Load all configuration from settings defaults when distribution is selected
    private void LoadConfigFromSettings()
    {
        // Node specs
        nodeSpecs = new NodeSpecsConfig
        {
            ProdMasterCpu = settings.ProdMasterCpu,
            ProdMasterRam = settings.ProdMasterRam,
            ProdMasterDisk = settings.ProdMasterDisk,
            NonProdMasterCpu = settings.NonProdMasterCpu,
            NonProdMasterRam = settings.NonProdMasterRam,
            NonProdMasterDisk = settings.NonProdMasterDisk,
            ProdInfraCpu = settings.ProdInfraCpu,
            ProdInfraRam = settings.ProdInfraRam,
            ProdInfraDisk = settings.ProdInfraDisk,
            NonProdInfraCpu = settings.NonProdInfraCpu,
            NonProdInfraRam = settings.NonProdInfraRam,
            NonProdInfraDisk = settings.NonProdInfraDisk,
            ProdWorkerCpu = settings.ProdWorkerCpu,
            ProdWorkerRam = settings.ProdWorkerRam,
            ProdWorkerDisk = settings.ProdWorkerDisk,
            NonProdWorkerCpu = settings.NonProdWorkerCpu,
            NonProdWorkerRam = settings.NonProdWorkerRam,
            NonProdWorkerDisk = settings.NonProdWorkerDisk
        };

        // Overcommit ratios
        prodCpuOvercommit = settings.ProdCpuOvercommit;
        prodMemoryOvercommit = settings.ProdMemoryOvercommit;
        nonProdCpuOvercommit = settings.NonProdCpuOvercommit;
        nonProdMemoryOvercommit = settings.NonProdMemoryOvercommit;

        // Headroom per environment
        headroom[EnvironmentType.Dev] = settings.HeadroomDev;
        headroom[EnvironmentType.Test] = settings.HeadroomTest;
        headroom[EnvironmentType.Stage] = settings.HeadroomStage;
        headroom[EnvironmentType.Prod] = settings.HeadroomProd;
        headroom[EnvironmentType.DR] = settings.HeadroomDR;

        // Replicas per environment
        replicas[EnvironmentType.Dev] = settings.ReplicasDev;
        replicas[EnvironmentType.Test] = settings.ReplicasTest;
        replicas[EnvironmentType.Stage] = settings.ReplicasStage;
        replicas[EnvironmentType.Prod] = settings.ReplicasProd;
        replicas[EnvironmentType.DR] = settings.ReplicasDR;

        // Mendix operator replicas
        mendixOperatorReplicas = settings.MendixOperatorReplicas;
    }

    // Legacy alias for compatibility
    private void LoadNodeSpecsFromSettings() => LoadConfigFromSettings();

    // Environment helpers
    private bool IsEnvironmentEnabled(EnvironmentType env) => enabledEnvironments.Contains(env);

    private bool IsProdEnvironment(EnvironmentType env) => VMService.IsProdEnvironment(env);

    // Returns the list of environments for VM deployment sidebar
    // Includes LifeTime only when OutSystems is selected
    private IEnumerable<EnvironmentType> GetVMEnvironmentList()
    {
        var baseEnvs = new[] { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod };

        // Add LifeTime for OutSystems
        if (selectedTechnology == Technology.OutSystems)
        {
            return baseEnvs.Append(EnvironmentType.LifeTime);
        }

        return baseEnvs;
    }

    private IEnumerable<EnvironmentType> GetVisibleEnvironments()
    {
        if (selectedClusterMode == ClusterMode.PerEnvironment)
        {
            return new[] { selectedSingleEnvironment };
        }
        return new[] { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod };
    }

    private string GetTierSpec(AppTier tier)
    {
        if (selectedTechnology == null) return "";
        var techConfig = TechnologyService.GetConfig(selectedTechnology.Value);
        if (techConfig.Tiers.TryGetValue(tier, out var specs))
        {
            return $"{specs.Cpu} CPU, {specs.Ram} GB RAM";
        }
        return "";
    }

    // Unified workload methods - use Prod as the reference environment for single cluster
    private int GetTotalApps(AppTier tier)
    {
        // For unified input, we use Prod environment as the base
        return GetEnvApps(EnvironmentType.Prod, tier);
    }

    private void SetTotalApps(AppTier tier, int count)
    {
        // Set the same count for all enabled environments
        foreach (var env in enabledEnvironments)
        {
            SetEnvApps(env, tier, count);
        }
    }

    private int GetTotalAppCount()
    {
        return GetTotalApps(AppTier.Small) + GetTotalApps(AppTier.Medium) +
               GetTotalApps(AppTier.Large) + GetTotalApps(AppTier.XLarge);
    }

    private string GetEstimatedCpu()
    {
        if (selectedTechnology == null) return "0";
        var techConfig = TechnologyService.GetConfig(selectedTechnology.Value);
        double total = 0;
        foreach (var tier in new[] { AppTier.Small, AppTier.Medium, AppTier.Large, AppTier.XLarge })
        {
            if (techConfig.Tiers.TryGetValue(tier, out var specs))
            {
                total += GetTotalApps(tier) * specs.Cpu;
            }
        }
        return total.ToString("0.#");
    }

    private string GetEstimatedMemory()
    {
        if (selectedTechnology == null) return "0";
        var techConfig = TechnologyService.GetConfig(selectedTechnology.Value);
        double total = 0;
        foreach (var tier in new[] { AppTier.Small, AppTier.Medium, AppTier.Large, AppTier.XLarge })
        {
            if (techConfig.Tiers.TryGetValue(tier, out var specs))
            {
                total += GetTotalApps(tier) * specs.Ram;
            }
        }
        return total.ToString("0.#");
    }

    private void ToggleEnvironment(EnvironmentType env, bool enabled)
    {
        if (env == EnvironmentType.Prod) return;
        if (enabled) enabledEnvironments.Add(env);
        else enabledEnvironments.Remove(env);
    }

    private int GetEnvApps(EnvironmentType env, AppTier tier)
    {
        if (!envApps.TryGetValue(env, out var config)) return 0;
        return tier switch
        {
            AppTier.Small => config.Small,
            AppTier.Medium => config.Medium,
            AppTier.Large => config.Large,
            AppTier.XLarge => config.XLarge,
            _ => 0
        };
    }

    private void SetEnvApps(EnvironmentType env, AppTier tier, int count)
    {
        if (!envApps.ContainsKey(env)) envApps[env] = new AppConfig();
        switch (tier)
        {
            case AppTier.Small: envApps[env].Small = count; break;
            case AppTier.Medium: envApps[env].Medium = count; break;
            case AppTier.Large: envApps[env].Large = count; break;
            case AppTier.XLarge: envApps[env].XLarge = count; break;
        }
    }

    private static int ParseInt(object? value) => int.TryParse(value?.ToString(), out var result) ? result : 0;

    // Wrapper methods for per-environment app UI
    private int GetEnvAppCount(EnvironmentType env, AppTier tier) => GetEnvApps(env, tier);

    private void SetEnvAppCount(EnvironmentType env, AppTier tier, int count) => SetEnvApps(env, tier, count);

    private int GetEnvTotalApps(EnvironmentType env)
    {
        if (!envApps.TryGetValue(env, out var config)) return 0;
        return config.Small + config.Medium + config.Large + config.XLarge;
    }
    private static double ParseDouble(object? value) => double.TryParse(value?.ToString(), out var result) ? result : 0;

    // Settings
    private void OpenSettings() => showSettings = true;
    private void CloseSettings() => showSettings = false;

    private async Task SaveAndCloseSettings()
    {
        await SaveSettingsToStorage();
        showSettings = false;
    }

    private void ResetSettings()
    {
        settings = new UICalculatorSettings();
        mendixPricingSettings = new MendixPricingSettings();
    }

    private async Task SaveSettingsToStorage()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "calculatorSettings", json);

        // Save Mendix pricing settings through the pricing service
        await PricingSettingsService.UpdateMendixPricingSettingsAsync(mendixPricingSettings);
    }

    private async Task LoadSettingsFromStorage()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "calculatorSettings");
            if (!string.IsNullOrEmpty(json))
            {
                settings = System.Text.Json.JsonSerializer.Deserialize<UICalculatorSettings>(json) ?? new();
            }

            // Load Mendix pricing settings from the pricing service
            mendixPricingSettings = PricingSettingsService.GetMendixPricingSettings();
        }
        catch { }
    }

    // Tier settings helpers - delegated to TierConfigurationService
    private double GetTierCpu(string tech, string tier) => TierConfigService.GetTierCpu(settings, tech, tier);
    private double GetTierRam(string tech, string tier) => TierConfigService.GetTierRam(settings, tech, tier);
    private void SetTierCpu(string tech, string tier, double value) => TierConfigService.SetTierCpu(settings, tech, tier, value);
    private void SetTierRam(string tech, string tier, double value) => TierConfigService.SetTierRam(settings, tech, tier, value);

    // Calculate
    private async Task CalculateK8s()
    {
        if (selectedDistribution == null || selectedTechnology == null || selectedClusterMode == null) return;

        // Determine the effective cluster mode based on singleClusterScope
        var effectiveClusterMode = selectedClusterMode.Value;
        var effectiveEnabledEnvs = enabledEnvironments;

        // For Single Cluster mode, adjust based on scope
        if (IsSingleClusterMode())
        {
            if (singleClusterScope == "Shared")
            {
                // Shared cluster: use SharedCluster mode with all enabled environments
                effectiveClusterMode = ClusterMode.SharedCluster;
            }
            else
            {
                // Single environment: use PerEnvironment mode with only that environment enabled
                effectiveClusterMode = ClusterMode.PerEnvironment;
                var singleEnv = GetSingleClusterEnvironment();
                effectiveEnabledEnvs = new HashSet<EnvironmentType> { singleEnv };
            }
        }

        // Use values from main page (NOT from settings - settings are only for defaults)
        var input = new K8sSizingInput
        {
            Distribution = selectedDistribution.Value,
            Technology = selectedTechnology.Value,
            ClusterMode = effectiveClusterMode,
            // For PerEnvironment mode, set the selected environment
            SelectedEnvironment = singleClusterScope != "Shared" ? GetSingleClusterEnvironment() : EnvironmentType.Prod,
            // Pass per-environment app counts from main page
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>(envApps),
            // Also set Prod/NonProd as fallback
            ProdApps = envApps.GetValueOrDefault(EnvironmentType.Prod, new AppConfig { Medium = 70 }),
            NonProdApps = envApps.GetValueOrDefault(EnvironmentType.Dev, new AppConfig { Medium = 70 }),
            EnabledEnvironments = effectiveEnabledEnvs,
            EnableHeadroom = true,
            // Use headroom values from main page (defaults match JS: 33/33/0/37.5/37.5)
            Headroom = new HeadroomSettings
            {
                Dev = headroom.GetValueOrDefault(EnvironmentType.Dev, 33),
                Test = headroom.GetValueOrDefault(EnvironmentType.Test, 33),
                Stage = headroom.GetValueOrDefault(EnvironmentType.Stage, 0),
                Prod = headroom.GetValueOrDefault(EnvironmentType.Prod, 37.5),
                DR = headroom.GetValueOrDefault(EnvironmentType.DR, 37.5)
            },
            // Use replica values from main page
            Replicas = new ReplicaSettings
            {
                NonProd = replicas.GetValueOrDefault(EnvironmentType.Dev, 1),
                Stage = replicas.GetValueOrDefault(EnvironmentType.Stage, 2),
                Prod = replicas.GetValueOrDefault(EnvironmentType.Prod, 3)
            },
            // Use overcommit values from main page
            ProdOvercommit = new OvercommitSettings
            {
                Cpu = prodCpuOvercommit,
                Memory = prodMemoryOvercommit
            },
            NonProdOvercommit = new OvercommitSettings
            {
                Cpu = nonProdCpuOvercommit,
                Memory = nonProdMemoryOvercommit
            },
            // HA/DR configuration from the HA & DR tab
            HADRConfig = k8sHADRConfig
        };

        // If custom node specs are provided via main page, use them
        if (selectedDistribution != null)
        {
            var distroConfig = DistributionService.GetConfig(selectedDistribution.Value);

            // For multi-cluster mode, pass per-environment specs
            Dictionary<EnvironmentType, NodeSpecs>? perEnvControlPlane = null;
            Dictionary<EnvironmentType, NodeSpecs>? perEnvWorker = null;
            Dictionary<EnvironmentType, NodeSpecs>? perEnvInfra = null;

            if (selectedClusterMode == ClusterMode.MultiCluster)
            {
                perEnvControlPlane = new Dictionary<EnvironmentType, NodeSpecs>
                {
                    [EnvironmentType.Dev] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Dev),
                    [EnvironmentType.Test] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Test),
                    [EnvironmentType.Stage] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Stage),
                    [EnvironmentType.Prod] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.Prod),
                    [EnvironmentType.DR] = nodeSpecs.GetControlPlaneSpecs(EnvironmentType.DR)
                };
                perEnvWorker = new Dictionary<EnvironmentType, NodeSpecs>
                {
                    [EnvironmentType.Dev] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Dev),
                    [EnvironmentType.Test] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Test),
                    [EnvironmentType.Stage] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Stage),
                    [EnvironmentType.Prod] = nodeSpecs.GetWorkerSpecs(EnvironmentType.Prod),
                    [EnvironmentType.DR] = nodeSpecs.GetWorkerSpecs(EnvironmentType.DR)
                };
                perEnvInfra = new Dictionary<EnvironmentType, NodeSpecs>
                {
                    [EnvironmentType.Dev] = nodeSpecs.GetInfraSpecs(EnvironmentType.Dev),
                    [EnvironmentType.Test] = nodeSpecs.GetInfraSpecs(EnvironmentType.Test),
                    [EnvironmentType.Stage] = nodeSpecs.GetInfraSpecs(EnvironmentType.Stage),
                    [EnvironmentType.Prod] = nodeSpecs.GetInfraSpecs(EnvironmentType.Prod),
                    [EnvironmentType.DR] = nodeSpecs.GetInfraSpecs(EnvironmentType.DR)
                };
            }

            input.CustomNodeSpecs = new DistributionConfig
            {
                Distribution = selectedDistribution.Value,
                Name = distroConfig.Name,
                Vendor = distroConfig.Vendor,
                HasManagedControlPlane = distroConfig.HasManagedControlPlane,
                HasInfraNodes = distroConfig.HasInfraNodes,
                ProdControlPlane = new NodeSpecs(nodeSpecs.ProdMasterCpu, nodeSpecs.ProdMasterRam, nodeSpecs.ProdMasterDisk),
                NonProdControlPlane = new NodeSpecs(nodeSpecs.NonProdMasterCpu, nodeSpecs.NonProdMasterRam, nodeSpecs.NonProdMasterDisk),
                ProdWorker = new NodeSpecs(nodeSpecs.ProdWorkerCpu, nodeSpecs.ProdWorkerRam, nodeSpecs.ProdWorkerDisk),
                NonProdWorker = new NodeSpecs(nodeSpecs.NonProdWorkerCpu, nodeSpecs.NonProdWorkerRam, nodeSpecs.NonProdWorkerDisk),
                ProdInfra = new NodeSpecs(nodeSpecs.ProdInfraCpu, nodeSpecs.ProdInfraRam, nodeSpecs.ProdInfraDisk),
                NonProdInfra = new NodeSpecs(nodeSpecs.NonProdInfraCpu, nodeSpecs.NonProdInfraRam, nodeSpecs.NonProdInfraDisk),
                // Per-environment specs for multi-cluster mode
                PerEnvControlPlane = perEnvControlPlane,
                PerEnvWorker = perEnvWorker,
                PerEnvInfra = perEnvInfra
            };
        }

        results = K8sSizingService.Calculate(input);

        // Generate validation warnings and recommendations
        validationWarnings = ValidationService.Analyze(results, input);

        // Reset cost estimate and auto-calculate default costs
        k8sCostEstimate = null;
        k8sResultsTab = "sizing";
        StateHasChanged();

        // Auto-calculate costs in the background
        _ = AutoCalculateK8sCost();

        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "results");
    }

    // Export functions
    private async Task ExportCsv()
    {
        if (results == null) return;
        var csv = ExportService.ExportToCsv(results);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"sizing-{DateTime.Now:yyyyMMdd-HHmmss}.csv", csv, "text/csv");
    }

    private async Task ExportJson()
    {
        if (results == null) return;
        var json = ExportService.ExportToJson(results);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"sizing-{DateTime.Now:yyyyMMdd-HHmmss}.json", json, "application/json");
    }

    private async Task ExportExcel()
    {
        if (results == null) return;
        var excel = ExportService.ExportToExcel(results);
        var base64 = Convert.ToBase64String(excel);
        await JSRuntime.InvokeVoidAsync("downloadFileBase64", $"sizing-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx", base64, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private async Task ExportHtml()
    {
        if (results == null) return;
        var html = ExportService.ExportToHtmlDiagram(results);
        await JSRuntime.InvokeVoidAsync("openInNewWindow", html, "Infrastructure Diagram");
    }

    private async Task ExportVMJson()
    {
        if (vmResults == null) return;
        var json = System.Text.Json.JsonSerializer.Serialize(vmResults, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        await JSRuntime.InvokeVoidAsync("downloadFile", $"vm-sizing-{DateTime.Now:yyyyMMdd-HHmmss}.json", json, "application/json");
    }

    private async Task ExportPdf()
    {
        if (results == null) return;
        var pdf = ExportService.ExportToPdf(results);
        var base64 = Convert.ToBase64String(pdf);
        await JSRuntime.InvokeVoidAsync("downloadFileBase64", $"sizing-{DateTime.Now:yyyyMMdd-HHmmss}.pdf", base64, "application/pdf");
    }

    private async Task ExportVMPdf()
    {
        if (vmResults == null) return;
        var pdf = ExportService.ExportToPdf(vmResults);
        var base64 = Convert.ToBase64String(pdf);
        await JSRuntime.InvokeVoidAsync("downloadFileBase64", $"vm-sizing-{DateTime.Now:yyyyMMdd-HHmmss}.pdf", base64, "application/pdf");
    }

    // Share functionality
    private async Task ShareConfiguration()
    {
        if (results == null) return;
        try
        {
            var input = BuildK8sInput();
            var url = await SharingService.GenerateShareUrlAsync(input);
            await SharingService.CopyShareUrlAsync(url);
            showShareCopied = true;
            StateHasChanged();
            await Task.Delay(2000);
            showShareCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Silently fail - share is non-critical
        }
    }

    private async Task ShareVMConfiguration()
    {
        if (vmResults == null) return;
        try
        {
            var input = BuildVMInput();
            var url = await SharingService.GenerateShareUrlAsync(input);
            await SharingService.CopyShareUrlAsync(url);
            showShareCopied = true;
            StateHasChanged();
            await Task.Delay(2000);
            showShareCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Silently fail - share is non-critical
        }
    }

    // Cost estimation methods
    private async Task HandleK8sCostCalculate((CloudProvider provider, string region, CostEstimationOptions options) args)
    {
        if (results == null) return;

        k8sCostLoading = true;
        StateHasChanged();

        try
        {
            var (provider, region, options) = args;
            options.Distribution = selectedDistribution?.ToString();
            k8sCostOptions = options;

            if (provider == CloudProvider.OnPrem)
            {
                var onPremPricing = PricingService.GetOnPremPricing();
                k8sCostEstimate = CostEstimationService.EstimateOnPremCost(results, onPremPricing, options);
            }
            else
            {
                k8sCostEstimate = await CostEstimationService.EstimateK8sCostAsync(results, provider, region, options);
            }
        }
        finally
        {
            k8sCostLoading = false;
            StateHasChanged();
        }
    }

    private async Task HandleVMCostCalculate((CloudProvider provider, string region, CostEstimationOptions options) args)
    {
        if (vmResults == null) return;

        vmCostLoading = true;
        StateHasChanged();

        try
        {
            var (provider, region, options) = args;
            options.Distribution = selectedTechnology?.ToString();
            vmCostOptions = options;

            if (provider == CloudProvider.OnPrem)
            {
                var onPremPricing = PricingService.GetOnPremPricing();
                vmCostEstimate = CostEstimationService.EstimateOnPremVMCost(vmResults, onPremPricing, options);
            }
            else
            {
                vmCostEstimate = await CostEstimationService.EstimateVMCostAsync(vmResults, provider, region, options);
            }
        }
        finally
        {
            vmCostLoading = false;
            StateHasChanged();
        }
    }

    // Growth planning methods
    private bool k8sGrowthCalculating = false;

    private void HandleK8sGrowthSettingsChanged()
    {
        // Settings changed - projection needs recalculation
        // Optionally auto-recalculate or just update UI
        StateHasChanged();
    }

    private void HandleK8sGrowthSettingsFromComponent(GrowthSettings settings)
    {
        // Settings changed from component - update local settings
        k8sGrowthSettings = settings;
        StateHasChanged();
    }

    private async Task CalculateK8sGrowth()
    {
        if (results == null || selectedDistribution == null) return;

        k8sGrowthCalculating = true;
        StateHasChanged();

        try
        {
            await Task.Delay(100); // Small delay for UI feedback
            k8sGrowthProjection = GrowthPlanningService.CalculateK8sGrowthProjection(
                results,
                k8sCostEstimate,
                k8sGrowthSettings,
                selectedDistribution.Value);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Growth calculation error: {ex.Message}");
        }
        finally
        {
            k8sGrowthCalculating = false;
            StateHasChanged();
        }
    }

    private async Task HandleK8sGrowthCalculate(GrowthSettings settings)
    {
        if (results == null || selectedDistribution == null) return;

        k8sGrowthSettings = settings;

        try
        {
            k8sGrowthProjection = GrowthPlanningService.CalculateK8sGrowthProjection(
                results,
                k8sCostEstimate,
                settings,
                selectedDistribution.Value);

            // Auto-switch to projection tab after calculation
            k8sGrowthSubTab = "projection";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Growth calculation error: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async Task HandleVMGrowthCalculate(GrowthSettings settings)
    {
        if (vmResults == null) return;

        vmGrowthSettings = settings;

        try
        {
            vmGrowthProjection = GrowthPlanningService.CalculateVMGrowthProjection(
                vmResults,
                vmCostEstimate,
                settings);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VM Growth calculation error: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private void HandleVMGrowthSettingsFromComponent(GrowthSettings settings)
    {
        // Settings changed from component - update local settings
        vmGrowthSettings = settings;
        StateHasChanged();
    }

    private async Task AutoCalculateK8sCost()
    {
        if (results == null) return;

        // Auto-calculate with defaults: AWS us-east-1
        k8sCostOptions = new CostEstimationOptions
        {
            Distribution = selectedDistribution?.ToString(),
            IncludeLicenses = true,
            IncludeStorage = true,
            IncludeNetwork = true,
            SupportTier = SupportTier.Basic
        };

        k8sCostLoading = true;
        StateHasChanged();

        try
        {
            k8sCostEstimate = await CostEstimationService.EstimateK8sCostAsync(
                results,
                CloudProvider.AWS,
                "us-east-1",
                k8sCostOptions);
        }
        catch
        {
            // Silently fail - cost estimation is supplementary
        }
        finally
        {
            k8sCostLoading = false;
            StateHasChanged();
        }
    }

    private async Task AutoCalculateVMCost()
    {
        if (vmResults == null) return;

        // Auto-calculate with defaults: AWS us-east-1
        vmCostOptions = new CostEstimationOptions
        {
            Distribution = selectedTechnology?.ToString(),
            IncludeLicenses = true,
            IncludeStorage = true,
            IncludeNetwork = true,
            SupportTier = SupportTier.Basic
        };

        vmCostLoading = true;
        StateHasChanged();

        try
        {
            vmCostEstimate = await CostEstimationService.EstimateVMCostAsync(
                vmResults,
                CloudProvider.AWS,
                "us-east-1",
                vmCostOptions);
        }
        catch
        {
            // Silently fail - cost estimation is supplementary
        }
        finally
        {
            vmCostLoading = false;
            StateHasChanged();
        }
    }

    private string FormatCostPreview(decimal? amount)
    {
        if (!amount.HasValue) return "--";
        var value = amount.Value;
        if (value >= 1_000_000)
            return $"${value / 1_000_000:F2}M";
        if (value >= 1_000)
            return $"${value / 1_000:F1}K";
        return $"${value:F0}";
    }

    private string FormatCurrency(decimal amount)
    {
        if (amount >= 1_000_000)
            return $"${amount / 1_000_000:F2}M";
        if (amount >= 1_000)
            return $"${amount / 1_000:F1}K";
        return $"${amount:F2}";
    }

    private string GetCostCategoryIcon(CostCategory category)
    {
        return category switch
        {
            CostCategory.Compute => "CPU",
            CostCategory.Storage => "HDD",
            CostCategory.Network => "NET",
            CostCategory.License => "LIC",
            CostCategory.Support => "SUP",
            CostCategory.DataCenter => "DC",
            CostCategory.Labor => "OPS",
            _ => ""
        };
    }

    private K8sSizingInput BuildK8sInput()
    {
        // Build per-environment apps config
        var environmentApps = new Dictionary<EnvironmentType, AppConfig>();
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var config))
            {
                environmentApps[env] = config;
            }
        }

        // Calculate ProdApps and NonProdApps from envApps for fallback
        var prodConfig = envApps.GetValueOrDefault(EnvironmentType.Prod, new AppConfig());
        var nonProdConfig = new AppConfig
        {
            Small = envApps.GetValueOrDefault(EnvironmentType.Dev)?.Small ?? 0,
            Medium = envApps.GetValueOrDefault(EnvironmentType.Dev)?.Medium ?? 0,
            Large = envApps.GetValueOrDefault(EnvironmentType.Dev)?.Large ?? 0,
            XLarge = envApps.GetValueOrDefault(EnvironmentType.Dev)?.XLarge ?? 0
        };

        return new K8sSizingInput
        {
            Distribution = selectedDistribution ?? Distribution.OpenShift,
            Technology = selectedTechnology ?? Technology.DotNet,
            ClusterMode = selectedClusterMode ?? ClusterMode.MultiCluster,
            EnabledEnvironments = enabledEnvironments,
            EnvironmentApps = environmentApps,
            ProdApps = prodConfig,
            NonProdApps = nonProdConfig,
            EnableHeadroom = headroom.Values.Any(h => h > 0),
            Headroom = new HeadroomSettings
            {
                Dev = (int)headroom.GetValueOrDefault(EnvironmentType.Dev, 33),
                Test = (int)headroom.GetValueOrDefault(EnvironmentType.Test, 33),
                Stage = (int)headroom.GetValueOrDefault(EnvironmentType.Stage, 0),
                Prod = (int)headroom.GetValueOrDefault(EnvironmentType.Prod, 40),
                DR = (int)headroom.GetValueOrDefault(EnvironmentType.DR, 40)
            },
            ProdOvercommit = new OvercommitSettings { Cpu = prodCpuOvercommit, Memory = prodMemoryOvercommit },
            NonProdOvercommit = new OvercommitSettings { Cpu = nonProdCpuOvercommit, Memory = nonProdMemoryOvercommit },
            HADRConfig = k8sHADRConfig
        };
    }

    private VMSizingInput BuildVMInput()
    {
        return new VMSizingInput
        {
            Technology = selectedTechnology ?? Technology.DotNet,
            EnvironmentConfigs = vmEnvironmentConfigs
        };
    }

    // Scenario helper methods
    private K8sSizingInput? GetK8sSizingInput()
    {
        if (results == null) return null;
        return BuildK8sInput();
    }

    private VMSizingInput? GetVMSizingInput()
    {
        if (vmResults == null) return null;
        return BuildVMInput();
    }

    private async Task HandleScenarioSaved(Scenario scenario)
    {
        // Show a brief notification that scenario was saved
        await JSRuntime.InvokeVoidAsync("alert", $"Scenario '{scenario.Name}' saved successfully!");
    }

    // ========================================
    // Navigation and Statistics Helper Methods
    // ========================================

    private bool ShouldShowResultsSection()
    {
        // Only show Results section when on or past the results step
        // K8s: step 7, Mendix VMs: step 7, OutSystems VMs: step 6
        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            return currentStep >= GetResultsStep() && results != null;
        }
        else if (selectedDeployment == DeploymentModel.VMs)
        {
            return currentStep >= GetResultsStep() && vmResults != null;
        }
        return false;
    }

    private string GetActiveResultsTab()
    {
        return results != null ? k8sResultsTab : vmResultsTab;
    }

    private void HandleResultTabSelected(string tab)
    {
        if (results != null)
        {
            k8sResultsTab = tab;
        }
        else if (vmResults != null)
        {
            vmResultsTab = tab;
        }
        StateHasChanged();
    }

    private string GetCurrentContext()
    {
        var parts = new List<string>();

        if (selectedPlatform != null)
        {
            parts.Add(selectedPlatform == PlatformType.Native ? "Native" : "Low-Code");
        }

        if (selectedDeployment != null)
        {
            parts.Add(selectedDeployment == DeploymentModel.Kubernetes ? "K8s" : "VM");
        }

        if (selectedTechnology != null)
        {
            var tech = GetAvailableTechnologies().FirstOrDefault(t => t.Technology == selectedTechnology);
            if (tech != null)
            {
                parts.Add(tech.Name);
            }
        }

        if (selectedDistribution != null)
        {
            var distro = DistributionService.GetAll().FirstOrDefault(d => d.Distribution == selectedDistribution);
            if (distro != null)
            {
                parts.Add(distro.Name);
            }
        }

        return parts.Count > 0 ? string.Join(" → ", parts) : "";
    }

    private int GetInsightsCount()
    {
        var warnings = results != null ? validationWarnings : vmValidationWarnings;
        return warnings?.Count ?? 0;
    }

    private int GetWarningsCount()
    {
        var warnings = results != null ? validationWarnings : vmValidationWarnings;
        return warnings?.Count(w => w.Severity == Models.WarningSeverity.Warning) ?? 0;
    }

    private int GetCriticalCount()
    {
        var warnings = results != null ? validationWarnings : vmValidationWarnings;
        return warnings?.Count(w => w.Severity == Models.WarningSeverity.Critical) ?? 0;
    }

    private int GetRecommendationsCount()
    {
        var warnings = results != null ? validationWarnings : vmValidationWarnings;
        return warnings?.Count(w => w.Severity == Models.WarningSeverity.Info) ?? 0;
    }

    private void HandleHeaderAction(string action)
    {
        switch (action)
        {
            case "settings":
                OpenSettings();
                break;
            case "scenarios":
                NavigationManager.NavigateTo("/scenarios");
                break;
            case "reset":
                ResetWizard();
                break;
        }
    }

    private int GetTotalNodes()
    {
        if (results != null)
        {
            return results.Environments.Sum(e => e.TotalNodes);
        }
        if (vmResults != null)
        {
            return vmResults.Environments.Sum(e => e.TotalVMs);
        }
        return 0;
    }

    private double GetTotalCPU()
    {
        if (results != null)
        {
            return results.Environments.Sum(e => e.TotalCpu);
        }
        if (vmResults != null)
        {
            return vmResults.Environments.Sum(e => e.TotalCpu);
        }
        return 0;
    }

    private double GetTotalRAM()
    {
        if (results != null)
        {
            return results.Environments.Sum(e => e.TotalRam);
        }
        if (vmResults != null)
        {
            return vmResults.Environments.Sum(e => e.TotalRam);
        }
        return 0;
    }

    private double GetTotalDisk()
    {
        if (results != null)
        {
            return results.Environments.Sum(e => e.TotalDisk);
        }
        if (vmResults != null)
        {
            return vmResults.Environments.Sum(e => e.TotalDisk);
        }
        return 0;
    }

    /// <summary>
    /// Builds the cost context from the current page state for use with CostService.
    /// </summary>
    private HomePageCostContext BuildCostContext() => new()
    {
        SelectedTechnology = selectedTechnology,
        SelectedDeployment = selectedDeployment,
        SelectedDistribution = selectedDistribution,
        PricingResult = pricingResult,
        K8sCostEstimate = k8sCostEstimate,
        VMCostEstimate = vmCostEstimate,
        VMResults = vmResults,
        MendixDeploymentCategory = mendixDeploymentCategory,
        MendixCloudType = mendixCloudType,
        MendixPrivateCloudProvider = mendixPrivateCloudProvider,
        MendixOtherDeployment = mendixOtherDeployment,
        MendixIsUnlimitedApps = mendixIsUnlimitedApps,
        MendixNumberOfApps = mendixNumberOfApps
    };

    private decimal GetMonthlyEstimate()
    {
        // Delegate to the cost service for testable cost calculation
        return CostService.GetMonthlyEstimate(BuildCostContext());
    }

    private decimal GetMendixVMLicenseMonthly()
    {
        // Delegate to the cost service for testable license calculation
        return CostService.GetMendixVMLicenseMonthly(
            mendixOtherDeployment ?? MendixOtherDeployment.Server,
            mendixIsUnlimitedApps,
            mendixNumberOfApps);
    }

    private string? GetCostProvider()
    {
        // Delegate to the cost service for testable provider determination
        return CostService.GetCostProvider(BuildCostContext());
    }

    private bool ShouldShowPricingNA()
    {
        // Delegate to the cost service for testable pricing availability check
        return CostService.ShouldShowPricingNA(BuildCostContext());
    }

    private async Task HandleExport(RightStatsSidebar.ExportFormat format)
    {
        // For guest users, show the limited export modal instead
        if (!_isAuthenticated)
        {
            showLimitedExportModal = true;
            StateHasChanged();
            return;
        }

        await PerformExport(format);
    }

    private async Task PerformExport(RightStatsSidebar.ExportFormat format)
    {
        try
        {
            if (results != null)
            {
                switch (format)
                {
                    case RightStatsSidebar.ExportFormat.CSV:
                        var csv = ExportService.ExportToCsv(results);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "k8s-sizing-export.csv", csv, "text/csv");
                        break;
                    case RightStatsSidebar.ExportFormat.JSON:
                        var json = ExportService.ExportToJson(results);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "k8s-sizing-export.json", json, "application/json");
                        break;
                    case RightStatsSidebar.ExportFormat.Excel:
                        var excel = ExportService.ExportToExcel(results);
                        await JSRuntime.InvokeVoidAsync("downloadFileBytes", "k8s-sizing-export.xlsx", excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        break;
                    case RightStatsSidebar.ExportFormat.PDF:
                        var pdf = ExportService.ExportToPdf(results);
                        await JSRuntime.InvokeVoidAsync("downloadFileBytes", "k8s-sizing-export.pdf", pdf, "application/pdf");
                        break;
                    case RightStatsSidebar.ExportFormat.Diagram:
                        var html = ExportService.ExportToHtmlDiagram(results);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "k8s-architecture.html", html, "text/html");
                        break;
                }
            }
            else if (vmResults != null)
            {
                switch (format)
                {
                    case RightStatsSidebar.ExportFormat.JSON:
                        var jsonContent = System.Text.Json.JsonSerializer.Serialize(vmResults, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        await JSRuntime.InvokeVoidAsync("downloadFile", "vm-sizing-export.json", jsonContent, "application/json");
                        break;
                    case RightStatsSidebar.ExportFormat.PDF:
                        var pdf = ExportService.ExportToPdf(vmResults);
                        await JSRuntime.InvokeVoidAsync("downloadFileBytes", "vm-sizing-export.pdf", pdf, "application/pdf");
                        break;
                    default:
                        // For unsupported formats, default to JSON
                        var defaultJson = System.Text.Json.JsonSerializer.Serialize(vmResults, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        await JSRuntime.InvokeVoidAsync("downloadFile", "vm-sizing-export.json", defaultJson, "application/json");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Export error: {ex.Message}");
        }
    }

    private async Task HandleGuestSummaryExport()
    {
        // Export a limited summary PDF for guest users
        try
        {
            if (results != null)
            {
                var pdf = ExportService.ExportToPdf(results);
                await JSRuntime.InvokeVoidAsync("downloadFileBytes", "k8s-sizing-summary.pdf", pdf, "application/pdf");
            }
            else if (vmResults != null)
            {
                var pdf = ExportService.ExportToPdf(vmResults);
                await JSRuntime.InvokeVoidAsync("downloadFileBytes", "vm-sizing-summary.pdf", pdf, "application/pdf");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Guest export error: {ex.Message}");
        }
    }

    #region LimitedExportModal Helpers

    private int GetTotalNodeCount() => GetTotalNodes();

    private double GetTotalCpuCount() => GetTotalCPU();

    private double GetTotalRamCount() => GetTotalRAM();

    private string GetMonthlyEstimateDisplay()
    {
        var estimate = GetMonthlyEstimate();
        return estimate > 0 ? $"${estimate:N0}" : "N/A";
    }

    private string GetPlatformDisplay()
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            var dist = selectedDistribution;
            if (dist != null)
            {
                var distInfo = DistributionService.GetAll()
                    .FirstOrDefault(d => d.Distribution == dist);
                if (distInfo != null)
                {
                    return $"Kubernetes ({distInfo.Name})";
                }
            }
            return "Kubernetes";
        }
        return "Virtual Machines";
    }

    private string GetTechnologyDisplay()
    {
        if (selectedTechnology != null)
        {
            var techInfo = TechnologyService.GetAll()
                .FirstOrDefault(t => t.Technology == selectedTechnology);
            return techInfo?.Name ?? selectedTechnology.ToString() ?? "Native";
        }
        return "Native";
    }

    #endregion

    private async Task HandleShare()
    {
        try
        {
            string? shareUrl = null;

            if (results != null)
            {
                var input = GetK8sSizingInput();
                if (input != null)
                {
                    shareUrl = await SharingService.GenerateShareUrlAsync(input);
                }
            }
            else if (vmResults != null)
            {
                var input = GetVMSizingInput();
                if (input != null)
                {
                    shareUrl = await SharingService.GenerateShareUrlAsync(input);
                }
            }

            if (string.IsNullOrEmpty(shareUrl))
            {
                return;
            }

            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", shareUrl);
            showShareCopied = true;
            StateHasChanged();
            await Task.Delay(2000);
            showShareCopied = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Share error: {ex.Message}");
        }
    }

    // VM Configuration helpers - delegated to VMService
    private VMEnvironmentConfig GetOrCreateVMConfig(EnvironmentType env)
    {
        if (!vmEnvironmentConfigs.TryGetValue(env, out var config))
        {
            config = VMService.CreateVMConfig(env, enabledEnvironments, selectedTechnology, TechnologyService);
            vmEnvironmentConfigs[env] = config;
        }
        return config;
    }

    private List<VMRoleConfig> CreateTechnologySpecificRoles(EnvironmentType env) =>
        VMService.CreateTechnologySpecificRoles(env, selectedTechnology, TechnologyService);

    private List<VMRoleConfig> CreateFallbackRoles(EnvironmentType env) =>
        VMService.CreateFallbackRoles(env);

    private IEnumerable<TechnologyServerRole> GetAvailableTechnologyRoles() =>
        VMService.GetAvailableTechnologyRoles(selectedTechnology, TechnologyService);

    // Management Environment helpers - delegated to VMService
    private bool HasManagementEnvironment() =>
        VMService.HasManagementEnvironment(selectedTechnology, TechnologyService);

    private string GetManagementEnvironmentName() =>
        VMService.GetManagementEnvironmentName(selectedTechnology, TechnologyService);

    private IEnumerable<TechnologyServerRole> GetManagementEnvironmentRoles() =>
        VMService.GetManagementEnvironmentRoles(selectedTechnology, TechnologyService);

    private void ToggleManagementVMRole(TechnologyServerRole techRole)
    {
        vmManagementEnvConfig ??= new VMEnvironmentConfig { Enabled = true };

        var existingRole = vmManagementEnvConfig.Roles.FirstOrDefault(r => r.RoleId == techRole.Id);
        if (existingRole != null)
        {
            vmManagementEnvConfig.Roles.Remove(existingRole);
        }
        else
        {
            var newRole = new VMRoleConfig
            {
                RoleId = techRole.Id,
                RoleName = techRole.Name,
                RoleIcon = techRole.Icon,
                Role = ServerRole.App, // Default role type
                Size = techRole.DefaultSize,
                InstanceCount = 1, // Management env roles typically single instance
                DiskGB = techRole.DefaultDiskGB
            };
            vmManagementEnvConfig.Roles.Add(newRole);
        }
    }

    private void RemoveManagementVMRole(VMRoleConfig roleConfig)
    {
        vmManagementEnvConfig?.Roles.Remove(roleConfig);
    }

    // Reset and initialize VM configs when technology changes
    private void OnTechnologyChanged()
    {
        vmEnvironmentConfigs.Clear();
        vmManagementEnvConfig = null; // Legacy - kept for compatibility

        // Add LifeTime to enabled environments for OutSystems
        if (selectedTechnology == Technology.OutSystems)
        {
            enabledEnvironments.Add(EnvironmentType.LifeTime);
        }
        else
        {
            enabledEnvironments.Remove(EnvironmentType.LifeTime);
        }

        // Pre-initialize all enabled environments with default roles
        // This ensures Front-End Server and Database Server are selected by default
        foreach (var env in enabledEnvironments)
        {
            GetOrCreateVMConfig(env);
        }
    }

    private void ToggleVMEnvironment(EnvironmentType env)
    {
        if (env == EnvironmentType.Prod) return; // Prod cannot be disabled
        if (enabledEnvironments.Contains(env))
        {
            enabledEnvironments.Remove(env);
            if (vmEnvironmentConfigs.TryGetValue(env, out var config))
                config.Enabled = false;
        }
        else
        {
            enabledEnvironments.Add(env);
            GetOrCreateVMConfig(env).Enabled = true;
        }
    }

    private void ToggleRoleBreakdownEnv(string envName)
    {
        // Accordion behavior: clicking same env collapses it, clicking different env switches to it
        expandedRoleBreakdownEnv = expandedRoleBreakdownEnv == envName ? null : envName;
    }

    private string GetShortEnvName(string envName) => VMService.GetShortEnvName(envName);

    private string GetEnvCssClass(string envName) => VMService.GetEnvCssClass(envName);

    private string FormatHAPattern(HAPattern pattern) => VMService.FormatHAPattern(pattern);

    private void ToggleVMRole(EnvironmentType env, ServerRole role)
    {
        var config = GetOrCreateVMConfig(env);
        var existing = config.Roles.FirstOrDefault(r => r.Role == role && string.IsNullOrEmpty(r.RoleId));
        if (existing != null)
        {
            config.Roles.Remove(existing);
        }
        else
        {
            config.Roles.Add(new VMRoleConfig
            {
                Role = role,
                RoleName = role.ToString(),
                RoleIcon = GetRoleIcon(role),
                Size = AppTier.Medium,
                InstanceCount = IsProdEnvironment(env) ? 2 : 1,
                DiskGB = role == ServerRole.Database ? 500 : 100
            });
        }
    }

    // Toggle technology-specific VM role
    private void ToggleTechVMRole(EnvironmentType env, TechnologyServerRole techRole)
    {
        var config = GetOrCreateVMConfig(env);
        var existing = config.Roles.FirstOrDefault(r => r.RoleId == techRole.Id);
        if (existing != null)
        {
            config.Roles.Remove(existing);
        }
        else
        {
            var isProd = IsProdEnvironment(env);
            config.Roles.Add(new VMRoleConfig
            {
                Role = ServerRole.App, // Fallback generic role
                RoleId = techRole.Id,
                RoleName = techRole.Name,
                RoleIcon = techRole.Icon,
                RoleDescription = techRole.Description,
                Size = techRole.DefaultSize,
                InstanceCount = techRole.ScaleHorizontally && isProd ? Math.Max(2, techRole.MinInstances) : techRole.MinInstances,
                DiskGB = techRole.DefaultDiskGB,
                MemoryMultiplier = techRole.MemoryMultiplier
            });
        }
    }

    private void RemoveVMRole(EnvironmentType env, ServerRole role)
    {
        var config = GetOrCreateVMConfig(env);
        var existing = config.Roles.FirstOrDefault(r => r.Role == role && string.IsNullOrEmpty(r.RoleId));
        if (existing != null)
            config.Roles.Remove(existing);
    }

    // Remove technology-specific VM role
    private void RemoveTechVMRole(EnvironmentType env, VMRoleConfig roleConfig)
    {
        var config = GetOrCreateVMConfig(env);
        config.Roles.Remove(roleConfig);
    }

    private string GetEnvIcon(EnvironmentType env) => VMService.GetEnvIcon(env);

    // Simpler helper methods for per-environment node specs (used in Razor template)
    private int GetMasterCpu() => nodeSpecs.GetControlPlaneSpecs(selectedNodeSpecsEnv).Cpu;
    private int GetMasterRam() => nodeSpecs.GetControlPlaneSpecs(selectedNodeSpecsEnv).Ram;
    private int GetMasterDisk() => nodeSpecs.GetControlPlaneSpecs(selectedNodeSpecsEnv).Disk;
    private int GetInfraCpu() => nodeSpecs.GetInfraSpecs(selectedNodeSpecsEnv).Cpu;
    private int GetInfraRam() => nodeSpecs.GetInfraSpecs(selectedNodeSpecsEnv).Ram;
    private int GetInfraDisk() => nodeSpecs.GetInfraSpecs(selectedNodeSpecsEnv).Disk;
    private int GetWorkerCpu() => nodeSpecs.GetWorkerSpecs(selectedNodeSpecsEnv).Cpu;
    private int GetWorkerRam() => nodeSpecs.GetWorkerSpecs(selectedNodeSpecsEnv).Ram;
    private int GetWorkerDisk() => nodeSpecs.GetWorkerSpecs(selectedNodeSpecsEnv).Disk;

    private void SetMasterCpu(ChangeEventArgs e) => SetEnvNodeSpec("Master", "Cpu", e);
    private void SetMasterRam(ChangeEventArgs e) => SetEnvNodeSpec("Master", "Ram", e);
    private void SetMasterDisk(ChangeEventArgs e) => SetEnvNodeSpec("Master", "Disk", e);
    private void SetInfraCpu(ChangeEventArgs e) => SetEnvNodeSpec("Infra", "Cpu", e);
    private void SetInfraRam(ChangeEventArgs e) => SetEnvNodeSpec("Infra", "Ram", e);
    private void SetInfraDisk(ChangeEventArgs e) => SetEnvNodeSpec("Infra", "Disk", e);
    private void SetWorkerCpu(ChangeEventArgs e) => SetEnvNodeSpec("Worker", "Cpu", e);
    private void SetWorkerRam(ChangeEventArgs e) => SetEnvNodeSpec("Worker", "Ram", e);
    private void SetWorkerDisk(ChangeEventArgs e) => SetEnvNodeSpec("Worker", "Disk", e);

    private void SetEnvNodeSpec(string nodeType, string spec, ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var value) || value < 1) return;
        nodeSpecs.SetSpec(selectedNodeSpecsEnv, nodeType, spec, value);
    }

    private string GetRoleIcon(ServerRole role) => VMService.GetRoleIcon(role);

    private int GetRoleMaxInstances(VMRoleConfig roleConfig) =>
        VMService.GetRoleMaxInstances(roleConfig, selectedTechnology, TechnologyService);

    private bool IsRoleSingleInstance(VMRoleConfig roleConfig) =>
        VMService.IsRoleSingleInstance(roleConfig, selectedTechnology, TechnologyService);

    private string GetVMEnvSummary(EnvironmentType env) =>
        VMService.GetVMEnvSummary(env, enabledEnvironments, vmEnvironmentConfigs);

    private string GetHAPatternDisplay(HAPattern pattern) => VMService.GetHAPatternDisplay(pattern);

    private string GetDRPatternDisplay(DRPattern pattern) => VMService.GetDRPatternDisplay(pattern);

    private string GetLBOptionDisplay(LoadBalancerOption option) => VMService.GetLBOptionDisplay(option);

    private async Task CalculateVM()
    {
        if (selectedTechnology == null) return;

        // Build the input from VM configs
        var input = new VMSizingInput
        {
            Technology = selectedTechnology.Value,
            EnabledEnvironments = enabledEnvironments,
            EnvironmentConfigs = vmEnvironmentConfigs,
            SystemOverheadPercent = vmSystemOverhead
        };

        vmResults = VMSizingService.Calculate(input);

        // Generate validation warnings and recommendations
        vmValidationWarnings = ValidationService.Analyze(vmResults, input);

        // Reset cost estimate and auto-calculate default costs
        vmCostEstimate = null;
        vmResultsTab = "sizing";
        StateHasChanged();

        // Auto-calculate costs in the background
        _ = AutoCalculateVMCost();

        await Task.Delay(100);
        await JSRuntime.InvokeVoidAsync("scrollToElement", "results");
    }

    // Info modals - now using database-backed content via InfoContentService
    private enum InfoType { Platform, Deployment, Technology, Distribution, ClusterMode, NodeSpecs, AppConfig }

    private async Task ShowInfoAsync(InfoType type)
    {
        var content = await InfoContentService.GetInfoTypeContentAsync(type.ToString());
        infoModalTitle = content.Title;
        infoModalContent = content.ContentHtml;
        showInfoModal = true;
        StateHasChanged();
    }

    // Sync wrapper for backward compatibility with event handlers
    private void ShowInfo(InfoType type) => _ = ShowInfoAsync(type);

    private async Task ShowDistroInfoAsync(Distribution distro)
    {
        var content = await InfoContentService.GetDistributionInfoAsync(distro);
        infoModalTitle = content.Title;
        infoModalContent = content.ContentHtml;
        showInfoModal = true;
        StateHasChanged();
    }

    // Sync wrapper for backward compatibility with event handlers
    private void ShowDistroInfo(Distribution distro) => _ = ShowDistroInfoAsync(distro);


    private async Task ShowTechInfoAsync(Technology tech)
    {
        var content = await InfoContentService.GetTechnologyInfoAsync(tech);
        infoModalTitle = content.Title;
        infoModalContent = content.ContentHtml;
        showInfoModal = true;
        StateHasChanged();
    }

    // Sync wrapper for backward compatibility with event handlers
    private void ShowTechInfo(Technology tech) => _ = ShowTechInfoAsync(tech);


    // Data helpers
    private IEnumerable<TechInfo> GetAvailableTechnologies()
    {
        var isLowCode = selectedPlatform == PlatformType.LowCode;
        var isK8s = selectedDeployment == DeploymentModel.Kubernetes;
        var techs = new List<TechInfo>();

        if (isLowCode)
        {
            techs.Add(new TechInfo(Technology.Mendix, "Mendix", "Siemens", "mendix", "Enterprise low-code platform", true, "#0CABF9"));
            // OutSystems only supports VM deployment (K8s only available on OutSystems Cloud/ODC)
            if (!isK8s)
            {
                techs.Add(new TechInfo(Technology.OutSystems, "OutSystems", "OutSystems", "outsystems", "Enterprise low-code platform (VM only)", true, "#FF6B35"));
            }
        }
        else
        {
            techs.Add(new TechInfo(Technology.DotNet, ".NET", "Microsoft", "dotnet", "Cross-platform framework", false, "#512BD4"));
            techs.Add(new TechInfo(Technology.Java, "Java", "Oracle/OpenJDK", "java", "Enterprise runtime", false, "#007396"));
            techs.Add(new TechInfo(Technology.NodeJs, "Node.js", "OpenJS Foundation", "nodejs", "JavaScript runtime", false, "#339933"));
            techs.Add(new TechInfo(Technology.Python, "Python", "Python Software Foundation", "python", "Web, data science & automation", false, "#3776AB"));
            techs.Add(new TechInfo(Technology.Go, "Go", "Google", "go", "Lightweight compiled language", false, "#00ADD8"));
        }

        return techs;
    }

    /// <summary>
    /// Gets filtered distributions using the distribution service.
    /// Applies deployment type, cloud category, and search filters.
    /// </summary>
    private IEnumerable<DistroInfo> GetFilteredDistributions()
    {
        // Use service for filtering logic
        var cloudCategoryFilter = distributionFilter == "cloud" ? cloudCategory : null;
        var filtered = DistributionService.GetFiltered(distributionFilter, cloudCategoryFilter, distributionSearch);

        // Project to DistroInfo for template compatibility
        return filtered.Select(ToDistroInfo);
    }

    /// <summary>
    /// Gets count of distributions by deployment type using the service.
    /// </summary>
    private int GetDistributionCount(string filter) =>
        DistributionService.GetCountByDeploymentType(filter);

    /// <summary>
    /// Gets count of cloud distributions by category using the service.
    /// </summary>
    private int GetCloudCategoryCount(string category) =>
        DistributionService.GetCountByCloudCategory(category);

    /// <summary>
    /// Converts a DistributionConfig to a DistroInfo for template rendering.
    /// </summary>
    private DistroInfo ToDistroInfo(DistributionConfig config)
    {
        var tags = config.Tags.Select(t => (t, UIHelperService.FormatTagLabel(t))).ToArray();
        return new DistroInfo(config.Distribution, config.Name, config.Vendor, config.Icon, config.BrandColor, tags);
    }

    private string GetTechnologyName(Technology? tech) =>
        DistributionDisplayService.GetTechnologyName(tech);

    private string GetDistributionName(Distribution? distro) =>
        DistributionDisplayService.GetDistributionName(distro);

    // Records and classes
    private record TechInfo(Technology Technology, string Name, string Vendor, string Icon, string Description, bool IsLowCode, string BrandColor);
    private record DistroTag(string CssClass, string Label);
    private record DistroInfo(Distribution Distribution, string Name, string Vendor, string Icon, string BrandColor, (string CssClass, string Label)[] TagData)
    {
        public IEnumerable<DistroTag> Tags => TagData.Select(t => new DistroTag(t.CssClass, t.Label));
    }

    // UICalculatorSettings and NodeSpecsConfig are now in InfraSizingCalculator.Models namespace
}
