namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Centralized CSS selectors for all UI elements.
/// When UI changes, update selectors HERE ONLY.
/// </summary>
public static class Locators
{
    #region Layout & Navigation

    public static class Layout
    {
        public const string MainContent = ".main-content";
        public const string LeftSidebar = ".left-sidebar";
        public const string RightSidebar = ".right-sidebar, .summary-panel";
        public const string Header = ".app-header, header";
        public const string Footer = ".app-footer, footer";
    }

    public static class Sidebar
    {
        public const string NavSection = ".nav-section";
        public const string NavSectionTitle = ".nav-section-title";
        public const string NavStep = ".nav-step";
        public const string NavStepCurrent = ".nav-step.current";
        public const string NavStepCompleted = ".nav-step.completed";
        public const string NavItem = "button.nav-item";
        public const string NavItemActive = "button.nav-item.active";

        // Result tabs in sidebar
        public const string ResultsSection = ".nav-section:has(.nav-section-title:has-text('Results'))";
        public const string SizingDetailsTab = "button.nav-item:has-text('Sizing Details')";
        public const string CostBreakdownTab = "button.nav-item:has-text('Cost Breakdown')";
        public const string GrowthPlanningTab = "button.nav-item:has-text('Growth Planning')";
        public const string InsightsTab = "button.nav-item:has-text('Insights')";
    }

    #endregion

    #region Wizard Steps

    public static class Wizard
    {
        // Step navigation
        public const string WizardContainer = ".wizard-container";
        public const string StepIndicator = ".step-indicator, .wizard-stepper";
        public const string CurrentStep = ".step.current, .wizard-step.active";

        // Navigation buttons
        public const string NextButton = "button:has-text('Next'):not([disabled])";
        public const string BackButton = "button:has-text('Back')";
        // The main wizard Calculate button is .btn-nav.primary with text "Calculate" when on pricing step
        public const string CalculateButton = "button.btn-nav.primary:has-text('Calculate'), button:has-text('Calculate'):not([disabled])";
        public const string ResetButton = "button:has-text('Reset')";
        public const string NewCalculationButton = "button:has-text('New Calculation')";
    }

    public static class SelectionCards
    {
        // Platform selection (Step 1)
        public const string PlatformCard = ".selection-card";
        public const string NativeCard = ".selection-card:has-text('Native')";
        public const string LowCodeCard = ".selection-card:has-text('Low-Code')";

        // Deployment selection (Step 2)
        public const string KubernetesCard = ".selection-card:has-text('Kubernetes')";
        public const string VirtualMachinesCard = ".selection-card:has-text('Virtual Machines')";

        // Technology selection (Step 3)
        public const string TechCard = ".tech-card";
        public const string DotNetCard = ".tech-card:has-text('.NET')";
        public const string JavaCard = ".tech-card:has-text('Java')";
        public const string NodeJsCard = ".tech-card:has-text('Node.js')";
        public const string PythonCard = ".tech-card:has-text('Python')";
        public const string GoCard = ".tech-card:has-text('Go')";
        public const string MendixCard = ".tech-card:has-text('Mendix')";
        public const string OutSystemsCard = ".tech-card:has-text('OutSystems')";

        // Distribution selection (Step 4 for K8s)
        public const string DistroCard = ".distro-card";

        // Mendix specific
        public const string MendixCategoryCard = ".mendix-category-card";
        public const string MendixPrivateCloud = ".mendix-category-card:has-text('Private Cloud')";
        public const string MendixCloud = ".mendix-category-card:has-text('Mendix Cloud')";
        public const string MendixOtherK8s = ".mendix-category-card:has-text('Other Kubernetes')";
        public const string MendixServer = ".mendix-category-card:has-text('Server')";
        public const string MendixStackIT = ".mendix-category-card:has-text('StackIT')";
        public const string MendixSapBtp = ".mendix-category-card:has-text('SAP BTP')";

        public const string MendixOptionCard = ".mendix-option-card";
        public const string MendixAzure = ".mendix-option-card:has-text('Mendix Azure')";
        public const string AmazonEKS = ".mendix-option-card:has-text('Amazon EKS')";
        public const string AzureAKS = ".mendix-option-card:has-text('Azure AKS')";
        public const string GoogleGKE = ".mendix-option-card:has-text('Google GKE')";
        public const string OpenShift = ".mendix-option-card:has-text('OpenShift')";
        public const string Rancher = ".mendix-option-card:has-text('Rancher')";
        public const string K3s = ".mendix-option-card:has-text('K3s')";
        public const string GenericK8s = ".mendix-option-card:has-text('Generic K8s')";
    }

    #endregion

    #region Configuration - K8s

    public static class K8sConfig
    {
        // Tab navigation
        public const string ConfigTabsContainer = ".config-tabs-container";
        public const string ConfigTab = ".config-tab";
        public const string ApplicationsTab = ".config-tab:has-text('Applications')";
        public const string NodeSpecsTab = ".config-tab:has-text('Node Specs')";
        public const string SettingsTab = ".config-tab:has-text('Settings')";

        // Cluster mode selector (ClusterModeSelector.razor)
        public const string ClusterModeSelector = ".cluster-mode-sidebar, .cluster-mode-options";
        public const string ModeOption = ".mode-option";
        public const string MultiClusterMode = ".mode-option:has-text('Multi-Cluster')";
        public const string SingleClusterMode = ".mode-option:has-text('Single Cluster')";
        public const string PerEnvMode = ".mode-option:has-text('Per-Env')";
        public const string ActiveMode = ".mode-option.selected";

        // Single cluster tier selection
        public const string TierCard = ".tier-card";
        public const string ScopeDropdown = ".single-cluster-selector select";

        // Environment sliders
        public const string EnvironmentSlider = ".environment-slider";
        public const string SliderInput = ".slider-input input[type='range']";
        public const string SliderValue = ".slider-value";

        // K8s Apps Config - Multi-cluster mode uses accordion panels
        public const string K8sAppsConfig = ".k8s-apps-config";
        public const string MultiClusterHeader = ".multi-cluster-header";
        public const string EnvAppsAccordion = ".env-apps-accordion";
        public const string EnvAccordionPanel = ".h-accordion-panel";  // Each environment panel
        public const string TierGrid = ".tier-grid";
        public const string TierPanel = ".tier-panel";
        public const string TierInput = "input.tier-input";  // Input element has class directly

        // App counts (legacy names for compatibility)
        public const string EnvironmentAppCard = ".h-accordion-panel, .environment-app-card";
        public const string AppCountInput = "input.tier-input, .app-count-input input";
        public const string PodsPerAppInput = ".pods-per-app input";

        // Node specs
        public const string NodeSpecsPanel = ".k8s-node-specs-config, .node-specs-panel, .k8s-node-specs";

        // Single-cluster mode: .node-spec-row with .spec-col divs (column position)
        public const string NodeSpecRow = ".node-spec-row";
        public const string WorkerNodeRow = ".node-spec-row:has-text('Worker')";

        // Multi-cluster mode: .node-card with .spec-field divs (label-based)
        public const string NodeCard = ".node-card";
        public const string WorkerNodeCard = ".node-card:has-text('Worker')";

        // Combined selectors for Worker node CPU/RAM/Disk - handle both modes:
        // Single-cluster: .spec-col nth-child position (2=CPU, 3=RAM, 4=Disk)
        // Multi-cluster: .spec-field with label
        public const string CpuInput = ".node-card:has-text('Worker') .spec-field:has(label:has-text('CPU')) input, .node-spec-row:has-text('Worker') .spec-col:nth-child(2) input";
        public const string RamInput = ".node-card:has-text('Worker') .spec-field:has(label:has-text('RAM')) input, .node-spec-row:has-text('Worker') .spec-col:nth-child(3) input";
        public const string DiskInput = ".node-card:has-text('Worker') .spec-field:has(label:has-text('Disk')) input, .node-spec-row:has-text('Worker') .spec-col:nth-child(4) input";

        // HA/DR settings
        public const string HADRPanel = ".ha-dr-panel, .k8s-hadr";
        public const string HAToggle = ".ha-toggle input[type='checkbox']";
        public const string DRToggle = ".dr-toggle input[type='checkbox']";
    }

    #endregion

    #region Configuration - VM

    public static class VMConfig
    {
        // Tab navigation
        public const string ServerRolesTab = ".config-tab:has-text('Server Roles')";
        public const string HADRTab = ".config-tab:has-text('HA & DR'), .config-tab:has-text('HA/DR')";

        // Server roles - actual class is .vm-server-roles-config
        public const string ServerRolesPanel = ".vm-server-roles-config, .vm-server-roles, .server-roles-panel";
        public const string RoleCard = "button.role-chip, .role-card";  // VM uses .role-chip buttons
        public const string RoleCheckbox = ".role-checkbox input[type='checkbox']";
        public const string RoleCountInput = ".role-count input, .instance-count input";
        public const string RoleDetailRow = ".role-detail-row";

        // HA/DR settings
        public const string VMHADRPanel = ".vm-hadr-config";
    }

    #endregion

    #region Pricing

    public static class Pricing
    {
        // Pricing panel
        public const string PricingPanel = ".pricing-panel, .pricing-config";
        public const string PricingToggle = ".pricing-toggle input[type='checkbox']";

        // Pricing tabs
        public const string InfraTab = ".pricing-tab:has-text('Infra'), .tab:has-text('Infrastructure')";
        public const string CloudTab = ".pricing-tab:has-text('Cloud'), .tab:has-text('Alternatives')";

        // Cloud provider selection
        public const string ProviderSelector = ".provider-selector, .cloud-provider-select";
        public const string AWSOption = "[value='AWS'], :has-text('AWS')";
        public const string AzureOption = "[value='Azure'], :has-text('Azure')";
        public const string GCPOption = "[value='GCP'], :has-text('GCP')";
        public const string OnPremOption = "[value='OnPrem'], :has-text('On-Prem')";

        // Region selection
        public const string RegionDropdown = ".region-select select, .region-dropdown";

        // Cost inputs
        public const string CostInput = ".cost-input input, .price-input input";
        public const string MonthlyServerCost = "input[name='monthlyServerCost']";

        // Mendix pricing
        public const string MendixEditionSelector = ".edition-selector select, select:has-text('Edition')";
        public const string MendixUserCountInput = ".user-count input";

        // OutSystems pricing
        public const string OutSystemsAOInput = ".ao-input input";
    }

    #endregion

    #region Results

    public static class Results
    {
        // Results container - Home.razor uses .results-panel
        public const string ResultsContainer = ".results-panel, .sizing-results-view, .results-container";

        // Results table - Home.razor uses .data-grid-table inside .sizing-data-grid
        public const string ResultsTable = ".data-grid-table, .results-table, .sizing-table, .env-cards-container";

        // Summary cards / totals - Home.razor uses .totals-row in tfoot, SizingResultsView uses .grand-total-bar
        public const string SummaryCards = ".totals-row, .summary-cards, .grand-total-bar";
        public const string TotalItem = ".totals-row td, .total-item, .summary-card";
        public const string TotalNodesCard = ".totals-row .total-col, .total-item:has-text('Nodes'), .summary-card:has-text('Nodes')";
        public const string TotalCPUCard = ".totals-row td:has-text('vCPU'), .total-item:has-text('vCPU'), .summary-card:has-text('vCPU')";
        public const string TotalRAMCard = ".totals-row td:has-text('RAM'), .total-item:has-text('RAM'), .summary-card:has-text('RAM')";
        public const string TotalDiskCard = ".totals-row td:has-text('Disk'), .total-item:has-text('Disk'), .summary-card:has-text('Disk')";
        public const string MonthlyCostCard = ".summary-card:has-text('Cost'), .monthly-cost";

        // Environment rows in results table
        public const string EnvironmentRow = ".env-row, tr:has-text('Dev'), tr:has-text('Test'), tr:has-text('Stage'), tr:has-text('Prod')";
        public const string TotalsRow = ".totals-row, tr.totals-row";
    }

    public static class CostBreakdown
    {
        // Cost breakdown panel
        public const string CostPanel = ".cost-breakdown, .cost-analysis";
        public const string CostEstimationPanel = ".cost-estimation-panel";
        public const string PanelHeader = ".cost-panel-header";
        public const string PanelContent = ".cost-panel-content";

        // Cost categories
        public const string CostCategory = ".cost-category";
        public const string CPUCostRow = ".cost-row:has-text('CPU')";
        public const string StorageCostRow = ".cost-row:has-text('Storage'), .cost-row:has-text('HDD')";
        public const string NetworkCostRow = ".cost-row:has-text('Network')";

        // Cost summary
        public const string CostSummary = ".cost-summary";
        public const string MonthlyTotal = ".monthly-total";
        public const string YearlyTotal = ".yearly-total";
        public const string TCO3Year = ".tco-3year";
        public const string TCO5Year = ".tco-5year";

        // Pricing options expand
        public const string PricingOptionsExpand = ".pricing-options, details:has-text('Pricing')";
    }

    public static class GrowthPlanning
    {
        // Settings bar
        public const string SettingsBar = ".settings-bar-compact";
        public const string PeriodDropdown = ".setting-inline:has-text('Period') select";
        public const string GrowthRateInput = ".rate-input-sm input, .setting-inline:has-text('Growth') input";
        public const string PatternDropdown = ".setting-inline:has-text('Pattern') select";
        public const string CostCheckbox = ".toggle-sm input[type='checkbox']";
        public const string CalculateButton = ".btn-calc-sm, button:has-text('Calculate')";

        // Hero strip (summary after calculate)
        public const string HeroStrip = ".hero-strip";
        public const string AppsGrowth = ".hero-item:has-text('Apps')";
        public const string NodesGrowth = ".hero-item:has-text('Nodes')";
        public const string InvestmentTotal = ".hero-item:has-text('Investment')";

        // Sub-tabs (buttons have emoji prefixes: 📦 Resources, 💵 Cost, 📅 Timeline)
        public const string TabBar = ".tab-bar-compact";
        public const string ResourcesTab = "button.tab-sm:has-text('Resources')";
        public const string CostTab = "button.tab-sm:has-text('Cost')";
        public const string TimelineTab = "button.tab-sm:has-text('Timeline')";
        public const string ActiveSubTab = "button.tab-sm.active";

        // Resources sub-tab
        public const string YearCards = ".year-cards-compact";
        public const string YearCard = ".year-card-sm";
        public const string BaselineCard = ".year-card-sm.baseline";
        public const string ResourceTable = ".data-table-sm";

        // Cost sub-tab
        public const string CostChart = ".cost-chart-compact";
        public const string CostBar = ".bar-sm";
        public const string CostCards = ".cost-cards-compact";
        public const string TotalCostCard = ".cost-card-sm.total";

        // Timeline sub-tab
        public const string TimelineVisual = ".timeline-visual-compact";
        public const string TimelineNode = ".tl-node";
        public const string WarningsSection = ".alerts-section.warnings";
        public const string RecommendationsSection = ".alerts-section.recommendations";
        public const string AlertItem = ".alert-item";
    }

    public static class Insights
    {
        public const string InsightsList = ".insights-list";
        public const string InsightItem = ".insight-item, .alert-item";
        public const string InsightBadge = ".badge";
        public const string CriticalBadge = ".badge.critical";
        public const string ExpandedInsight = ".insight-expanded";
    }

    #endregion

    #region Export & Actions

    public static class Export
    {
        public const string ExportButtons = ".export-buttons";
        public const string SaveProfileButton = ".export-btn.profile, button:has-text('Save Profile')";
        public const string ExportCSVButton = ".export-btn.csv, button:has-text('Export CSV')";
        public const string ExportJSONButton = ".export-btn.json, button:has-text('Export JSON')";
        public const string ExportExcelButton = ".export-btn.excel, button:has-text('Export Excel')";
        public const string ExportDiagramButton = ".export-btn.diagram, button:has-text('Export Diagram')";
        public const string ShareButton = "button:has-text('Share')";
        public const string SaveScenarioButton = "button:has-text('Save Scenario')";
    }

    #endregion

    #region Modals & Dialogs

    public static class Modals
    {
        public const string Modal = ".modal, .dialog";
        public const string ModalOverlay = ".modal-overlay, .dialog-backdrop";
        public const string ModalContent = ".modal-content, .dialog-content";
        public const string ModalClose = ".modal-close, .dialog-close, button:has-text('Close')";
        public const string ModalConfirm = ".modal-confirm, button:has-text('Confirm'), button:has-text('OK')";
        public const string ModalCancel = ".modal-cancel, button:has-text('Cancel')";
    }

    #endregion

    #region Shared Components

    public static class Shared
    {
        // Accordion
        public const string Accordion = ".h-accordion, .horizontal-accordion";
        public const string AccordionPanel = ".h-accordion-panel";
        public const string AccordionHeader = ".accordion-header";
        public const string AccordionContent = ".accordion-content";
        public const string AccordionExpanded = ".accordion-expanded, .h-accordion-panel.expanded";

        // Info buttons
        public const string InfoButton = ".info-button, .info-icon";
        public const string Tooltip = ".tooltip, .info-tooltip";

        // Loading states
        public const string LoadingSpinner = ".loading-spinner, .spinner";
        public const string LoadingOverlay = ".loading-overlay";

        // Dropdowns
        public const string Dropdown = "select";
        public const string DropdownOption = "option";

        // Checkboxes & Toggles
        public const string Checkbox = "input[type='checkbox']";
        public const string Toggle = ".toggle, .switch";

        // Inputs
        public const string NumberInput = "input[type='number']";
        public const string TextInput = "input[type='text']";
        public const string RangeInput = "input[type='range']";

        // Filter buttons
        public const string FilterButtons = ".filter-buttons";
        public const string FilterButton = ".filter-btn";
        public const string FilterButtonActive = ".filter-btn.active";
    }

    #endregion

    #region Header & User

    public static class Header
    {
        public const string AppTitle = ".app-title, h1:has-text('Infrastructure Sizing')";
        public const string SettingsButton = "button:has-text('Settings')";
        public const string SavedButton = "button:has-text('Saved')";
        public const string ResetButton = "button:has-text('Reset')";
        public const string UserMenu = ".user-menu";
        public const string ThemeToggle = ".theme-toggle";
    }

    #endregion
}
