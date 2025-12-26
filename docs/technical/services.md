# Services Reference

This document describes all services in the Infrastructure Sizing Calculator.

---

## Service Overview

| Service | Interface | Purpose |
|---------|-----------|---------|
| K8sSizingService | IK8sSizingService | Kubernetes cluster sizing calculations |
| VMSizingService | IVMSizingService | Virtual machine sizing calculations |
| TechnologyService | ITechnologyService | Technology configurations and specs |
| DistributionService | IDistributionService | K8s distribution configurations (46 distributions) |
| ExportService | IExportService | Result export to various formats |
| WizardStateService | IWizardStateService | UI wizard state management |
| AppStateService | IAppStateService | Centralized application state management |
| PricingService | IPricingService | Cloud pricing and cost estimation |
| PricingSettingsService | IPricingSettingsService | Pricing configuration and Mendix pricing |
| DatabasePricingSettingsService | IPricingSettingsService | SQLite-backed pricing settings |
| GrowthPlanningService | IGrowthPlanningService | Growth projection calculations |
| CostEstimationService | ICostEstimationService | Cost estimation for K8s and VM deployments |
| ScenarioService | IScenarioService | Scenario management (save/load/compare) |
| SettingsPersistenceService | ISettingsPersistenceService | Settings persistence to localStorage |
| ConfigurationSharingService | - | Configuration sharing via URL encoding |
| ValidationRecommendationService | - | Input validation and recommendations |

---

## K8sSizingService

**File:** `Services/K8sSizingService.cs`
**Interface:** `Services/Interfaces/IK8sSizingService.cs`

### Dependencies
- `IDistributionService` - For distribution-specific node specs
- `ITechnologyService` - For technology-specific tier specs
- `CalculatorSettings` (optional) - Configurable calculation parameters

### Configurable Settings (via CalculatorSettings)
All constants are now configurable via the `CalculatorSettings` model:

| Setting | Default | Description |
|---------|---------|-------------|
| SystemReservePercent | 15 | Percent reserved for system pods (85% available) |
| MinWorkers | 3 | Minimum workers per cluster |
| AppsPerInfra | 25 | 1 infra per N apps |
| MinInfra | 3 | Minimum infra nodes |
| MaxInfra | 10 | Maximum infra nodes |
| LargeDeploymentThreshold | 50 | Large deployment app count |
| MinProdInfraLarge | 5 | Min infra for large prod |
| LargeClusterWorkerThreshold | 100 | Threshold for 5 masters |

### Methods

#### Calculate
```csharp
K8sSizingResult Calculate(K8sSizingInput input)
```
Main entry point for K8s sizing calculation.

**Process:**
1. Get distribution config from DistributionService
2. Get technology config from TechnologyService
3. For each enabled environment:
   - Calculate pods (apps × replicas)
   - Calculate master nodes (BR-M001 to BR-M004)
   - Calculate infra nodes (BR-I001 to BR-I006)
   - Calculate worker nodes (BR-W001 to BR-W006)
   - Calculate total resources
4. Aggregate to grand total

#### CalculateMasters
```csharp
int CalculateMasters(bool hasManagedControlPlane, int workers, bool isProd)
```
Calculate control plane node count.

**Rules:**
- Managed CP (EKS/AKS/GKE/OKE) → 0
- 100+ workers → 5
- Default → 3

#### CalculateInfra
```csharp
int CalculateInfra(bool hasInfraNodes, int apps, bool isProd)
```
Calculate infrastructure node count (OpenShift only).

**Rules:**
- Non-OpenShift → 0
- Base = ceiling(apps / 25)
- Production with 50+ apps → minimum 5
- Clamped to [3, 10]

#### CalculateWorkers
```csharp
int CalculateWorkers(K8sSizingInput input, AppConfig apps, NodeSpecs workerSpecs, int replicas, bool isProd)
```
Calculate worker node count.

**Process:**
1. Sum resource requirements per tier
2. Apply overcommit ratios
3. Apply system reserve (85%)
4. Calculate by CPU and RAM
5. Return max(byCpu, byRam, minimum)

#### ApplyHeadroom
```csharp
int ApplyHeadroom(int workers, double headroomPercent)
```
Apply headroom percentage to worker count.

**Formula:** `workers × (1 + headroomPercent / 100)`

---

## VMSizingService

**File:** `Services/VMSizingService.cs`
**Interface:** `Services/Interfaces/IVMSizingService.cs`

### Dependencies
- `ITechnologyService` - For technology-specific tier specs and VM roles

### Methods

#### Calculate
```csharp
VMSizingResult Calculate(VMSizingInput input)
```
Main entry point for VM sizing calculation.

**Process:**
1. Get technology config
2. For each enabled environment:
   - Get HA multiplier
   - For each role: calculate instances and resources
   - Calculate load balancer requirements
   - Sum environment totals
3. Aggregate to grand total

#### GetRoleSpecs
```csharp
(int cpu, int ram) GetRoleSpecs(ServerRole role, AppTier size, Technology technology)
```
Get CPU and RAM for a role at specified size.

#### GetHAMultiplier
```csharp
double GetHAMultiplier(HAPattern pattern)
```
Get instance multiplier for HA pattern.

| Pattern | Multiplier |
|---------|------------|
| None | 1.0 |
| ActiveActive | 2.0 |
| ActivePassive | 2.0 |
| NPlus1 | 1 + (1/instances) |
| NPlus2 | 1 + (2/instances) |

#### GetLoadBalancerSpecs
```csharp
(int vms, int cpuPerVm, int ramPerVm) GetLoadBalancerSpecs(LoadBalancerOption option)
```
Get load balancer VM specifications.

| Option | VMs | CPU | RAM |
|--------|-----|-----|-----|
| None | 0 | 0 | 0 |
| Single | 1 | 2 | 4 |
| HAPair | 2 | 2 | 4 |
| CloudLB | 0 | 0 | 0 |

---

## TechnologyService

**File:** `Services/TechnologyService.cs`
**Interface:** `Services/Interfaces/ITechnologyService.cs`

### Data
Static dictionary of `TechnologyConfig` for all 7 technologies.

### Methods

#### GetConfig
```csharp
TechnologyConfig GetConfig(Technology technology)
```
Get configuration for a specific technology.

**Returns:** TechnologyConfig with tier specs and VM roles

#### GetAll
```csharp
IEnumerable<TechnologyConfig> GetAll()
```
Get all technology configurations.

#### GetByPlatformType
```csharp
IEnumerable<TechnologyConfig> GetByPlatformType(PlatformType platformType)
```
Filter technologies by platform type (Native or LowCode).

#### GetVMRoles
```csharp
TechnologyVMRoles? GetVMRoles(Technology technology)
```
Get VM server role definitions for a technology.

### Technology Tier Specs

| Technology | Small | Medium | Large | XLarge |
|------------|-------|--------|-------|--------|
| .NET | 0.25/0.5 | 0.5/1 | 1/2 | 2/4 |
| Java | 0.5/1 | 1/2 | 2/4 | 4/8 |
| Node.js | 0.25/1* | 0.5/1 | 1/2 | 2/4 |
| Python | 0.25/1* | 0.5/1 | 1/2 | 2/4 |
| Go | 0.125/0.25 | 0.25/0.5 | 0.5/1 | 1/2 |
| Mendix | 1/2 | 2/4 | 4/8 | 8/16 |
| OutSystems | 1/2 | 2/4 | 4/8 | 8/16 |

*Format: CPU cores / RAM GB*

*Note: Node.js and Python Small tier RAM increased to 1 GB per official docs (V8 heap management and WSGI/Django overhead).

---

## DistributionService

**File:** `Services/DistributionService.cs`
**Interface:** `Services/Interfaces/IDistributionService.cs`

### Data
Static dictionary of `DistributionConfig` for all 11 distributions.

### Methods

#### GetConfig
```csharp
DistributionConfig GetConfig(Distribution distribution)
```
Get configuration for a specific distribution.

#### GetAll
```csharp
IEnumerable<DistributionConfig> GetAll()
```
Get all distribution configurations.

### Distribution Characteristics

| Distribution | Managed CP | Infra Nodes | Category |
|--------------|------------|-------------|----------|
| OpenShift | No | Yes | Enterprise |
| Kubernetes | No | No | Standard |
| Rancher | No | No | Standard |
| K3s | No | No | Lightweight |
| MicroK8s | No | No | Lightweight |
| Charmed | No | No | Standard |
| Tanzu | No | No | Enterprise |
| EKS | Yes | No | Managed |
| AKS | Yes | No | Managed |
| GKE | Yes | No | Managed |
| OKE | Yes | No | Managed |

---

## ExportService

**File:** `Services/ExportService.cs`
**Interface:** `Services/Interfaces/IExportService.cs`

### Dependencies
- ClosedXML (for Excel export)

### Methods

#### ExportToCsv
```csharp
string ExportToCsv(K8sSizingResult result)
string ExportToCsv(VMSizingResult result)
```
Export results to CSV format.

**Output:** String containing CSV content

#### ExportToJson
```csharp
string ExportToJson(K8sSizingResult result)
string ExportToJson(VMSizingResult result)
```
Export results to JSON format.

**Output:** Formatted JSON string

#### ExportToExcel
```csharp
byte[] ExportToExcel(K8sSizingResult result)
byte[] ExportToExcel(VMSizingResult result)
```
Export results to Excel format.

**Output:** Byte array of .xlsx file

**Worksheets:**
1. Summary - Metadata and grand totals
2. Per-environment sheets with detailed breakdown

#### ExportToHtmlDiagram
```csharp
string ExportToHtmlDiagram(K8sSizingResult result)
string ExportToHtmlDiagram(VMSizingResult result)
```
Export results to HTML visualization.

**Output:** Self-contained HTML page with diagram

---

## WizardStateService

**File:** `Services/WizardStateService.cs`
**Interface:** `Services/Interfaces/IWizardStateService.cs`

### Purpose
Manages UI wizard state across steps. Uses Blazor's scoped lifetime.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| CurrentStep | int | Current wizard step (1-based) |
| SelectedPlatform | PlatformType? | Selected platform |
| SelectedDeployment | DeploymentModel? | Selected deployment |
| SelectedTechnology | Technology? | Selected technology |
| SelectedDistribution | Distribution? | Selected distribution |
| SelectedClusterMode | ClusterMode | Selected cluster mode |
| EnabledEnvironments | HashSet\<EnvironmentType\> | Enabled environments |
| EnvApps | Dictionary\<EnvironmentType, AppConfig\> | App configs |
| Headroom | HeadroomSettings | Headroom settings |
| Replicas | ReplicaSettings | Replica settings |
| K8sResult | K8sSizingResult? | Latest K8s result |
| VMResult | VMSizingResult? | Latest VM result |

### Methods

#### Reset
```csharp
void Reset()
```
Clear all state and return to step 1.

#### NotifyStateChanged
```csharp
void NotifyStateChanged()
```
Trigger UI update via StateHasChanged event.

#### CanNavigateToStep
```csharp
bool CanNavigateToStep(int step)
```
Validate if user can navigate to specified step.

**Rules:**
- Step 1: Always allowed
- Step 2: Platform selected
- Step 3: Deployment selected
- Step 4: Technology selected
- Step 5: Distribution or VM config complete
- Step 6: Configuration valid

#### GetStepLabel
```csharp
string GetStepLabel(int step)
```
Get display label for wizard step.

---

## AppStateService

**File:** `Services/AppStateService.cs`
**Interface:** `Services/Interfaces/IAppStateService.cs`

### Purpose
Centralized state management for the entire application. Solves the problem of duplicate state and state resets across components. Uses the Single Source of Truth pattern.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| ActiveSection | string | Current view section ("config", "sizing", "cost", "growth") |
| ExpandedCard | string? | Currently expanded card ID (for progressive disclosure) |
| HasResults | bool | Whether any results exist |
| K8sResults | K8sSizingResult? | Latest Kubernetes sizing results |
| VMResults | VMSizingResult? | Latest VM sizing results |
| K8sCostEstimate | CostEstimate? | K8s cost estimation |
| VMCostEstimate | CostEstimate? | VM cost estimation |
| TotalNodes | int | Computed total nodes across all results |
| TotalCPU | double | Computed total CPU across all results |
| TotalRAM | double | Computed total RAM across all results |
| MonthlyEstimate | decimal | Current monthly cost estimate |
| CostProvider | string? | Current cost provider (AWS, Azure, etc.) |

### Methods

#### NavigateToSection
```csharp
void NavigateToSection(string section)
```
Navigate to a specific section without resetting results.

#### SetK8sResults
```csharp
void SetK8sResults(K8sSizingResult results)
```
Store K8s sizing results and notify listeners.

#### SetVMResults
```csharp
void SetVMResults(VMSizingResult results)
```
Store VM sizing results and notify listeners.

#### ResetResults
```csharp
void ResetResults()
```
Explicitly clear all results. Only called on user request.

#### ToggleExpand
```csharp
void ToggleExpand(string cardId)
```
Toggle expansion state for progressive disclosure cards.

#### IsExpanded
```csharp
bool IsExpanded(string cardId)
```
Check if a card is currently expanded.

### Events

```csharp
event Action? OnStateChanged;
```
Fired when any state changes. Components subscribe to update UI.

### Key Design Decisions

1. **Results persist until explicit reset** - Navigation between sections does NOT clear results
2. **Computed properties for summaries** - TotalNodes, TotalCPU, etc. compute from current results
3. **Single source of truth** - All components read from AppStateService, not local variables
4. **Event-based updates** - Components subscribe to OnStateChanged for reactive updates

---

## Dependency Injection Registration

**File:** `Program.cs`

```csharp
builder.Services.AddSingleton<CalculatorSettings>();
builder.Services.AddSingleton<IDistributionService, DistributionService>();
builder.Services.AddSingleton<ITechnologyService, TechnologyService>();
builder.Services.AddScoped<IK8sSizingService, K8sSizingService>();
builder.Services.AddScoped<IVMSizingService, VMSizingService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IWizardStateService, WizardStateService>();
builder.Services.AddScoped<IAppStateService, AppStateService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IGrowthService, GrowthService>();
```

**Lifetimes:**
- **Singleton:** CalculatorSettings, TechnologyService, DistributionService (static data/global settings)
- **Scoped:** All others (per-user session state)

---

## PricingSettingsService

**File:** `Services/Pricing/PricingSettingsService.cs`
**Interface:** `Services/Interfaces/IPricingSettingsService.cs`

### Purpose
Manages pricing settings and calculations for both on-premises and Mendix deployments.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| IncludePricingInResults | bool | Whether to include pricing in results output |

### Methods

#### GetOnPremDefaults
```csharp
OnPremPricing GetOnPremDefaults()
```
Get default on-premises pricing configuration.

#### GetMendixPricingSettings
```csharp
MendixPricingSettings GetMendixPricingSettings()
```
Get complete Mendix pricing configuration including resource packs and environment tiers.

#### CalculateMendixCost
```csharp
MendixPricingResult CalculateMendixCost(
    MendixDeploymentCategory category,
    MendixCloudType? cloudType,
    MendixPrivateCloudProvider? privateProvider,
    int environments,
    int internalUsers,
    int externalUsers,
    string resourcePackSize = "M",
    int resourcePackCount = 1)
```
Calculate total Mendix licensing cost based on deployment configuration.

**Parameters:**
- `category` - Cloud, PrivateCloud, or Other
- `cloudType` - SaaS or Dedicated (for Cloud category)
- `privateProvider` - Azure, EKS, AKS, GKE, OpenShift, etc.
- `environments` - Total number of Mendix environments
- `internalUsers` - Number of internal application users
- `externalUsers` - Number of external application users
- `resourcePackSize` - Size of resource pack (XS-4XL)
- `resourcePackCount` - Number of resource packs

**Returns:** `MendixPricingResult` with detailed cost breakdown

#### IsMendixSupportedProvider
```csharp
bool IsMendixSupportedProvider(MendixPrivateCloudProvider provider)
```
Check if a K8s provider is officially supported by Mendix.

**Supported Providers:**
- Azure
- EKS
- AKS
- GKE
- OpenShift

### Mendix Pricing Calculations

#### K8s Environment Tiers
The service implements tiered pricing for K8s environments:

```
Tier 1 (4-50 envs):   $552/env/year
Tier 2 (51-100 envs): $408/env/year
Tier 3 (101-150 envs): $240/env/year
Tier 4 (151+ envs):   FREE
```

#### User Licensing
```
Internal: ceil(users / 100) × $40,800/year
External: ceil(users / 250,000) × $60,000/year
```

### Related Models

- `MendixPricingSettings` - Complete pricing configuration
- `MendixPricingResult` - Calculated cost breakdown
- `MendixDeploymentCategory` - Cloud, PrivateCloud, Other
- `MendixPrivateCloudProvider` - Azure, EKS, AKS, GKE, OpenShift, etc.
- `MendixResourcePackSpec` - Resource pack specifications

See [Mendix Pricing Guide](../guides/mendix-pricing.md) for detailed pricing information.

---

## CostEstimationService

**File:** `Services/CostEstimationService.cs`
**Interface:** `Services/Interfaces/ICostEstimationService.cs`

### Purpose
Provides comprehensive cost estimation for both K8s and VM deployments, including cloud provider pricing and on-premises hardware costs.

### Methods

#### EstimateK8sCost
```csharp
CostEstimate EstimateK8sCost(K8sSizingResult result, CostEstimationOptions options)
```
Estimate costs for Kubernetes deployment.

#### EstimateVMCost
```csharp
CostEstimate EstimateVMCost(VMSizingResult result, CostEstimationOptions options)
```
Estimate costs for VM deployment.

---

## ScenarioService

**File:** `Services/ScenarioService.cs`
**Interface:** `Services/Interfaces/IScenarioService.cs`

### Purpose
Manages scenario persistence including save, load, update, delete, and comparison operations.

### Methods

#### SaveScenarioAsync
```csharp
Task<Scenario> SaveScenarioAsync(string name, string? description, ConfigurationState config)
```
Save a new scenario to the database.

#### GetAllScenariosAsync
```csharp
Task<IEnumerable<Scenario>> GetAllScenariosAsync()
```
Retrieve all saved scenarios.

#### DeleteScenarioAsync
```csharp
Task<bool> DeleteScenarioAsync(Guid id)
```
Delete a scenario by ID.

---

## SettingsPersistenceService

**File:** `Services/SettingsPersistenceService.cs`
**Interface:** `Services/Interfaces/ISettingsPersistenceService.cs`

### Purpose
Persists user settings and preferences to browser localStorage using JavaScript interop.

### Methods

#### SaveSettingsAsync
```csharp
Task SaveSettingsAsync(UserSettings settings)
```
Save settings to localStorage.

#### LoadSettingsAsync
```csharp
Task<UserSettings?> LoadSettingsAsync()
```
Load settings from localStorage.

---

## GrowthPlanningService

**File:** `Services/GrowthPlanningService.cs`
**Interface:** `Services/Interfaces/IGrowthPlanningService.cs`

### Purpose
Calculates growth projections over time based on configurable growth patterns.

### Methods

#### ProjectGrowth
```csharp
GrowthProjection ProjectGrowth(GrowthSettings settings, K8sSizingResult baselineResult)
GrowthProjection ProjectGrowth(GrowthSettings settings, VMSizingResult baselineResult)
```
Project resource requirements over specified years.

### Growth Patterns

| Pattern | Formula | Use Case |
|---------|---------|----------|
| Linear | base × (1 + rate × year) | Steady, predictable growth |
| Exponential | base × (1 + rate)^year | Rapid scaling startups |
| S-Curve | Logistic function | Growth with market saturation |

---

## ConfigurationSharingService

**File:** `Services/ConfigurationSharingService.cs`

### Purpose
Enables sharing configurations via URL-encoded links.

### Methods

#### GenerateShareUrl
```csharp
string GenerateShareUrl(ConfigurationState config)
```
Generate a shareable URL containing the encoded configuration.

#### ParseShareUrl
```csharp
ConfigurationState? ParseShareUrl(string url)
```
Parse a shared URL and restore the configuration.

---

## ValidationRecommendationService

**File:** `Services/ValidationRecommendationService.cs`

### Purpose
Provides input validation and sizing recommendations based on best practices.

### Methods

#### ValidateInput
```csharp
ValidationResult ValidateInput(K8sSizingInput input)
ValidationResult ValidateInput(VMSizingInput input)
```
Validate sizing inputs against business rules.

#### GetRecommendations
```csharp
IEnumerable<Recommendation> GetRecommendations(K8sSizingResult result)
```
Generate optimization recommendations based on results.
