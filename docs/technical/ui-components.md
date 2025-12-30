# UI Components Reference

This document describes the Blazor UI components in the Infrastructure Sizing Calculator.

---

## Component Hierarchy

```
App.razor
└── MainLayout.razor
    ├── HeaderBar.razor
    │   └── UserMenu.razor (Authentication dropdown)
    ├── LeftSidebar.razor (Navigation Controller)
    ├── Home.razor (Main Content Area)
    │   ├── Wizard Steps
    │   │   ├── Step 1: Platform Selection
    │   │   ├── Step 2: Deployment Model
    │   │   ├── Step 3: Technology
    │   │   ├── Step 4a: Distribution (K8s)
    │   │   ├── Step 4b: VM Configuration (VMs)
    │   │   ├── Step 5: Configuration
    │   │   ├── Step 6: Pricing
    │   │   └── Step 7: Results
    │   ├── K8s Configuration (Components/K8s/)
    │   │   ├── K8sAppsConfig.razor
    │   │   ├── K8sNodeSpecsConfig.razor
    │   │   ├── K8sHADRPanel.razor
    │   │   └── K8sSettingsConfig.razor
    │   ├── VM Configuration (Components/VM/)
    │   │   ├── VMServerRolesConfig.razor
    │   │   └── VMHADRConfig.razor
    │   ├── Shared Components (Components/Shared/)
    │   │   ├── EnvironmentSlider.razor
    │   │   ├── HorizontalSlider.razor
    │   │   ├── EnvironmentAppCard.razor
    │   │   ├── EnvironmentAppGrid.razor
    │   │   ├── InfoButton.razor
    │   │   ├── LoadingSpinner.razor
    │   │   ├── SelectionCard.razor
    │   │   └── HorizontalAccordionPanel.razor
    │   ├── Pricing (Components/Pricing/)
    │   │   ├── CloudAlternativesPanel.razor
    │   │   └── OnPremPricingPanel.razor
    │   ├── Results Views (Components/Results/)
    │   │   ├── SizingResultsView.razor
    │   │   ├── CostAnalysisView.razor
    │   │   ├── CostEstimationPanel.razor
    │   │   ├── GrowthPlanningView.razor
    │   │   ├── GrowthProjectionChart.razor
    │   │   └── GrowthTimeline.razor
    │   ├── Modals (Components/Modals/)
    │   │   ├── ModalBase.razor (with accessibility)
    │   │   ├── Info Modal
    │   │   └── Save Scenario Modal
    │   └── Configuration (Components/Configuration/)
    │       ├── AppCountsPanel.razor
    │       ├── NodeSpecsPanel.razor
    │       ├── SettingsPanel.razor
    │       ├── ClusterModeSelector.razor
    │       └── PricingSelector.razor
    ├── Wizard Framework (Components/Wizard/)
    │   └── WizardStepper.razor (progress tracking)
    ├── Authentication Pages (Components/Pages/)
    │   ├── Login.razor
    │   ├── Register.razor
    │   └── AccessDenied.razor
    ├── Scenarios.razor (Scenario Management Page)
    ├── Settings.razor (User Settings Page)
    └── RightStatsSidebar.razor (Summary Stats Only)
```

### Layout Pattern: Three-Panel Dashboard

The application uses a three-panel layout:
- **Left Sidebar (200px):** Navigation only - the SOLE navigation controller
- **Main Content (flexible):** Displays one view at a time based on sidebar selection
- **Right Stats Panel (260px):** Read-only summary metrics (never duplicates main content)

---

## Main Components

### App.razor

Application root component with routing configuration.

**File:** `Components/App.razor`

**Purpose:** Configures Blazor routing and renders the main layout.

---

### MainLayout.razor

Page layout wrapper with header and navigation.

**File:** `Components/Layout/MainLayout.razor`

**Features:**
- Dark theme styling
- Header with branding
- Main content area
- Footer

---

### Home.razor

Main wizard component containing all calculator functionality.

**File:** `Components/Pages/Home.razor`
**Lines:** ~3700+

**State Variables:**
```csharp
// Wizard Navigation
private int currentStep = 1;
private bool isCalculating = false;

// Selections
private PlatformType? selectedPlatform;
private DeploymentModel? selectedDeployment;
private Technology? selectedTechnology;
private Distribution? selectedDistribution;
private ClusterMode clusterMode = ClusterMode.MultiCluster;

// Configuration
private HashSet<EnvironmentType> enabledEnvironments;
private Dictionary<EnvironmentType, AppConfig> envApps;
private Dictionary<EnvironmentType, VMEnvironmentConfig> vmEnvConfigs;

// Results
private K8sSizingResult? k8sResult;
private VMSizingResult? vmResult;
```

---

## Wizard Steps

### Step 1: Platform Selection

**Purpose:** Choose between Native and Low-Code platforms.

**UI Elements:**
- Selection cards for Native and Low-Code
- Platform descriptions and icons
- Auto-advance on selection

**Selection Cards:**
| Platform | Icon | Description |
|----------|------|-------------|
| Native | Code icon | Traditional code-first development |
| Low-Code | Visual icon | Visual development platforms |

---

### Step 2: Deployment Model

**Purpose:** Choose between Kubernetes and VMs.

**UI Elements:**
- Selection cards for K8s and VMs
- Icons and descriptions
- Auto-advance on selection

**Selection Cards:**
| Model | Icon | Description |
|-------|------|-------------|
| Kubernetes | Container icon | Container orchestration |
| VMs | Server icon | Traditional virtual machines |

---

### Step 3: Technology Selection

**Purpose:** Select the application technology.

**UI Elements:**
- Grid of technology cards
- Filtered by platform type
- Brand colors and icons
- Info buttons with detailed specs
- Vendor attribution

**Technologies by Platform:**

*Native:*
- .NET (Microsoft)
- Java (Oracle)
- Node.js (OpenJS)
- Python (PSF)
- Go (Google)

*Low-Code:*
- Mendix (Siemens)
- OutSystems (OutSystems) - VM only

---

### Step 4a: Distribution Selection (K8s Only)

**Purpose:** Select Kubernetes distribution.

**UI Elements:**
- Grid of distribution cards
- Categorized by type (Enterprise, Standard, Managed, Lightweight)
- Managed CP indicator badges
- Infra nodes indicator (OpenShift)
- Info buttons with detailed specs

**Distribution Categories:**

| Category | Distributions |
|----------|---------------|
| Enterprise | OpenShift, Tanzu |
| Standard | Kubernetes, Rancher, Charmed |
| Managed | EKS, AKS, GKE, OKE |
| Lightweight | K3s, MicroK8s |

---

### Step 4b: VM Configuration (VMs Only)

**Purpose:** Configure VM server roles.

**UI Elements:**
- Technology-specific server roles
- Per-role configuration:
  - Size tier dropdown
  - Instance count (with MaxInstances constraint)
  - Disk size input
- HA pattern selection
- DR pattern selection
- Load balancer option

---

### Step 5: Configuration

**Purpose:** Configure applications, nodes, and settings.

**Sub-tabs:**
1. **Applications** - App counts per tier per environment
2. **Node Specs** - Customize node specifications
3. **Settings** - Replicas, headroom, overcommit

---

### Step 6: Results

**Purpose:** Display calculation results and export options.

**UI Elements:**
- Summary cards with grand totals
- Per-environment breakdown table
- Export buttons (CSV, JSON, Excel, HTML)
- Recalculate button

---

## Modal Components

### Info Modal

**Purpose:** Display detailed information about technologies or distributions.

**Trigger:** Info button (ℹ️) on cards

**Content:**
- Icon and name
- Vendor information
- Description
- Key specifications
- Links to documentation

---

### Profiles Modal

**Purpose:** Load saved configuration profiles.

**Features:**
- List of saved profiles
- Profile metadata (name, date, type)
- Load profile button
- Delete profile button

---

### Save Profile Modal

**Purpose:** Save current configuration as a profile.

**Features:**
- Profile name input
- Save button
- Cancel button

---

## Configuration Components

### Applications Tab

**Purpose:** Configure application counts per tier.

**UI Elements:**
- Per-environment sections
- Tier inputs (Small, Medium, Large, XLarge)
- Tier specs display (CPU/RAM)
- Total apps calculation

---

### Node Specs Tab

**Purpose:** Customize node specifications.

**UI Elements:**
- Distribution defaults display
- Custom override toggles
- Per-node-type inputs:
  - Control Plane (CPU, RAM, Disk)
  - Infra (OpenShift only)
  - Worker (CPU, RAM, Disk)

---

### Settings Tab

**Purpose:** Configure replicas, headroom, and overcommit.

**Sections:**

**Replicas:**
- Production replicas (1-10)
- Non-production replicas (1-10)
- Staging replicas (1-10)

**Headroom:**
- Enable/disable toggle
- Per-environment percentages (0-100%)
- Default values button

**Overcommit:**
- Production CPU ratio (1-10)
- Production Memory ratio (1-4)
- Non-production CPU ratio (1-10)
- Non-production Memory ratio (1-4)

---

## Cluster Mode UI

### Multi-Cluster Mode

**UI Pattern:** Checkboxes per environment

```
☐ Dev Cluster
   └── [Node configuration panel]

☑ Test Cluster
   ├── Control Plane: CPU [8 ] RAM [32] Disk [100]
   └── Worker: CPU [16] RAM [64] Disk [200]

☑ Prod Cluster
   ├── Control Plane: CPU [8 ] RAM [32] Disk [100]
   ├── Infra (OpenShift): CPU [8 ] RAM [32] Disk [200]
   └── Worker: CPU [16] RAM [64] Disk [200]
```

**Behavior:**
- Each checked cluster shows its own configuration
- Unchecked clusters are hidden
- Independent node specs per cluster

---

### Single Cluster Mode

**UI Pattern:** Dropdown selection

```
Scope: [▼ Shared Cluster]
       ├── Shared Cluster
       ├── Dev
       ├── Test
       ├── Stage
       ├── Prod
       └── DR

Configuration for: Shared Cluster
├── Control Plane: CPU [8 ] RAM [32] Disk [100]
├── Infra (OpenShift): CPU [8 ] RAM [32] Disk [200]
└── Worker: CPU [16] RAM [64] Disk [200]
```

**Behavior:**
- Dropdown to select scope
- Each selection shows its specific configuration
- Single cluster calculated for selected scope

---

## Results View Components

These components use progressive disclosure patterns instead of nested tabs, following UX best practices.

---

### SizingResultsView.razor

**File:** `Components/Results/SizingResultsView.razor`

**Purpose:** Display sizing results with expandable environment cards.

**Design Pattern:** Expandable cards with grand total bar

**Key Features:**
- Grand total bar always visible at top
- Environment cards that expand/collapse on click
- No environment tabs - uses expand/collapse pattern instead
- Role breakdown table shown when expanded
- Supports both K8s and VM results

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| K8sResult | K8sSizingResult? | Kubernetes sizing results |
| VMResult | VMSizingResult? | VM sizing results |
| ExpandedEnvironment | string? | Currently expanded environment |
| OnExpandToggle | EventCallback<string> | Callback when expand toggled |

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ Grand Total: 24 nodes | 192 vCPU | 768 GB RAM       │
├─────────────────────────────────────────────────────┤
│ ▶ Dev (3 nodes | 24 vCPU | 96 GB)                   │
├─────────────────────────────────────────────────────┤
│ ▼ Prod (12 nodes | 96 vCPU | 384 GB)               │
│   ┌─────────────────────────────────────────────┐   │
│   │ Role      | Nodes | CPU | RAM               │   │
│   │ Master    |   3   |  24 |  96               │   │
│   │ Infra     |   3   |  24 |  96               │   │
│   │ Worker    |   6   |  48 | 192               │   │
│   └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

### CostAnalysisView.razor

**File:** `Components/Results/CostAnalysisView.razor`

**Purpose:** Display cost analysis with progressive disclosure sections.

**Design Pattern:** Stacked `<details>` elements for progressive disclosure

**Key Features:**
- Cost summary cards always visible (Monthly, Yearly, 3-Year TCO, 5-Year TCO)
- Collapsible sections using HTML `<details>` elements
- No sub-tabs - each section expands independently
- Pricing selector embedded in collapsible section

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| CostEstimate | CostEstimate? | Current cost estimate |
| Options | CostEstimationOptions | Pricing configuration options |
| Distribution | string? | Selected distribution |
| ShowPricingSelector | bool | Whether to show pricing options |
| IsLoading | bool | Loading state |
| OnRecalculate | EventCallback<...> | Callback to recalculate costs |

**Sections (all collapsible):**
1. **Pricing Options** - Provider, region, pricing type, support tier
2. **Cost Breakdown** - By category (Compute, Storage, Network, License, Support)
3. **Cost by Environment** - Per-environment cost breakdown
4. **Notes & Assumptions** - Pricing assumptions and caveats

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ [$2,450/mo] [$29.4K/yr] [$88.2K 3yr] [$147K 5yr]   │
├─────────────────────────────────────────────────────┤
│ ▶ Pricing Options                                   │
├─────────────────────────────────────────────────────┤
│ ▼ Cost Breakdown                                    │
│   Compute: $1,800 (73%) ████████████░░░            │
│   Storage: $400 (16%)   ████░░░░░░░░░░░            │
│   Network: $150 (6%)    ██░░░░░░░░░░░░░            │
│   License: $100 (4%)    █░░░░░░░░░░░░░░            │
├─────────────────────────────────────────────────────┤
│ ▶ Cost by Environment                               │
├─────────────────────────────────────────────────────┤
│ ▶ Notes & Assumptions                               │
└─────────────────────────────────────────────────────┘
```

---

### GrowthPlanningView.razor

**File:** `Components/Results/GrowthPlanningView.razor`

**Purpose:** Display growth projections with inline settings.

**Design Pattern:** Inline settings row + projection cards

**Key Features:**
- Settings always visible in compact row (no modal)
- Year-by-year projection cards
- Warning indicators on cards
- Cost projections optional toggle

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| Settings | GrowthSettings | Growth configuration |
| Projection | GrowthProjection? | Calculated projection |
| OnCalculate | EventCallback<GrowthSettings> | Calculate callback |
| SettingsChanged | EventCallback<GrowthSettings> | Settings changed callback |

**Settings (inline):**
- Growth Rate: 0-100% slider
- Projection Years: 1/3/5 years dropdown
- Growth Pattern: Linear/Exponential/S-Curve
- Include Costs: Toggle

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ Rate: [====●====] 25% | Years: [3▼] | Pattern: [Linear▼] │
│ ☑ Include Costs                        [Calculate] │
├─────────────────────────────────────────────────────┤
│ [+15 Apps] [+8 Nodes] [$5.2K Cost Increase]        │
├─────────────────────────────────────────────────────┤
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │
│ │ Current │ │ Year 1  │ │ Year 2  │ │ Year 3⚠│   │
│ │ 50 apps │ │ 63 apps │ │ 79 apps │ │ 99 apps │   │
│ │ 24 nodes│ │ 28 nodes│ │ 32 nodes│ │ 38 nodes│   │
│ │ $2.4K/mo│ │ $2.8K/mo│ │ $3.2K/mo│ │ $3.8K/mo│   │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘   │
├─────────────────────────────────────────────────────┤
│ ⚠ Warnings (1)                                      │
│   Year 3: Approaching cluster node limit (100)      │
└─────────────────────────────────────────────────────┘
```

---

## Legacy Results Components

### Summary Cards

**Purpose:** Display grand totals at a glance.

**Cards:**
| Card | Icon | Value |
|------|------|-------|
| Total Nodes | Server | Sum of all nodes |
| Total vCPU | CPU | Sum of all CPU |
| Total RAM | Memory | Sum of all RAM (GB) |
| Total Disk | Storage | Sum of all Disk (GB) |

---

### Results Table

**Purpose:** Detailed per-environment breakdown.

**Columns (K8s):**
| Column | Description |
|--------|-------------|
| Environment | Dev, Test, Stage, Prod, DR |
| Type | Prod/Non-Prod |
| Apps | Total applications |
| Pods | Apps × Replicas |
| Masters | Control plane nodes |
| Infra | Infrastructure nodes |
| Workers | Worker nodes |
| Total | Sum of all nodes |
| CPU | Total vCPU |
| RAM | Total RAM (GB) |
| Disk | Total Disk (GB) |

**Columns (VM):**
| Column | Description |
|--------|-------------|
| Environment | Dev, Test, Stage, Prod, DR |
| HA Pattern | HA configuration |
| DR Pattern | DR configuration |
| VMs | Total VM count |
| CPU | Total vCPU |
| RAM | Total RAM (GB) |
| Disk | Total Disk (GB) |
| LB VMs | Load balancer VMs |

---

### Export Buttons

**Purpose:** Export results in various formats.

**Buttons:**
| Button | Format | Action |
|--------|--------|--------|
| CSV | .csv | Download CSV file |
| JSON | .json | Download JSON file |
| Excel | .xlsx | Download Excel workbook |
| Diagram | .html | Download HTML visualization |

---

## Styling

### CSS Variables

**File:** `wwwroot/app.css`

```css
:root {
  --primary-color: #667eea;
  --secondary-color: #764ba2;
  --background-dark: #1a1a2e;
  --background-card: #16213e;
  --text-primary: #ffffff;
  --text-secondary: #a0aec0;
  --border-color: #2d3748;
  --success-color: #48bb78;
  --warning-color: #ed8936;
  --error-color: #f56565;
}
```

### Key CSS Classes

| Class | Purpose |
|-------|---------|
| `.wizard-container` | Main wizard wrapper |
| `.wizard-step` | Individual step container |
| `.selection-card` | Clickable selection card |
| `.selection-card.selected` | Selected card state |
| `.config-section` | Configuration section |
| `.summary-card` | Results summary card |
| `.results-table` | Results breakdown table |
| `.modal-overlay` | Modal background |
| `.modal-content` | Modal dialog box |
| `.btn-primary` | Primary action button |
| `.btn-secondary` | Secondary action button |

---

## JavaScript Interop

**File:** `wwwroot/js/site.js`

### Functions

```javascript
// Download file to browser
window.downloadFile = function(filename, content, contentType) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// Download binary file (Excel)
window.downloadBinaryFile = function(filename, base64Content, contentType) {
    const binary = atob(base64Content);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    const blob = new Blob([bytes], { type: contentType });
    // ... same download logic
};

// Scroll to element
window.scrollToElement = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};
```

### Blazor Invocation

```csharp
// In Home.razor
@inject IJSRuntime JS

private async Task DownloadCsv()
{
    var content = ExportService.ExportToCsv(k8sResult!);
    await JS.InvokeVoidAsync("downloadFile",
        "sizing-results.csv",
        content,
        "text/csv");
}

private async Task DownloadExcel()
{
    var bytes = ExportService.ExportToExcel(k8sResult!);
    var base64 = Convert.ToBase64String(bytes);
    await JS.InvokeVoidAsync("downloadBinaryFile",
        "sizing-results.xlsx",
        base64,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}
```

---

## K8s Configuration Components

### K8sHADRPanel.razor

**File:** `Components/K8s/K8sHADRPanel.razor`

**Purpose:** Configure High Availability and Disaster Recovery settings for Kubernetes clusters.

**Design Pattern:** Two-section panel with summary bar

**Key Features:**
- Control plane HA configuration (for self-managed distributions only)
- Node distribution across availability zones
- DR strategy selection with RTO guidance
- Backup strategy configuration (Velero, Kasten, Portworx, CloudNative)
- Live cost multiplier calculation display

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| Config | K8sHADRConfig | Current HA/DR configuration |
| ConfigChanged | EventCallback<K8sHADRConfig> | Callback when config changes |
| Distribution | Distribution | Selected K8s distribution |

**HA Options:**
| Option | Description | Nodes |
|--------|-------------|-------|
| Managed | Cloud provider manages CP | 0 (managed) |
| Single | No HA, single node | 1 |
| StackedHA | etcd on control plane | 3, 5, or 7 |
| ExternalEtcd | Separate etcd cluster | 3, 5, or 7 |

**DR Patterns:**
| Pattern | RTO | Description |
|---------|-----|-------------|
| None | N/A | Multi-AZ only |
| BackupRestore | ~24h | Regular backups, manual restore |
| WarmStandby | ~1h | Minimal DR cluster, scales on failover |
| HotStandby | ~15min | Full DR cluster ready |
| ActiveActive | ~5min | Multi-region traffic serving |

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ 🛡️ High Availability                                │
│   Control Plane: [Stacked HA ▼]  Nodes: [3 ▼]      │
│   Node Distribution: [Multi-AZ ▼]  AZs: [3 ▼]      │
│   ☑ Pod Disruption Budgets  ☑ Topology Spread      │
├─────────────────────────────────────────────────────┤
│ 🔄 Disaster Recovery                                │
│   DR Strategy: [Warm Standby ▼]                     │
│   DR Region: [us-west-2]  RTO: [1 hour ▼]          │
│   Backup Strategy: [Velero ▼]                       │
│   Frequency: [Daily ▼]  Retention: [30 days ▼]      │
├─────────────────────────────────────────────────────┤
│ 📊 Multi-AZ + Warm Standby | Cost multiplier: 1.40x │
└─────────────────────────────────────────────────────┘
```

---

### K8sSettingsConfig.razor

**File:** `Components/K8s/K8sSettingsConfig.razor`

**Purpose:** Configure headroom, replicas, and resource overcommit settings.

**Design Pattern:** Compact grid with sections

**Key Features:**
- Per-environment headroom percentages (buffer capacity)
- Per-environment replica counts (pod instances)
- Production vs non-production overcommit ratios
- Disabled inputs for unchecked environments

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| EnabledEnvironments | HashSet<EnvironmentType> | Currently enabled environments |
| Headroom | Dictionary<EnvironmentType, double> | Headroom % per environment |
| HeadroomChanged | EventCallback | Headroom update callback |
| Replicas | Dictionary<EnvironmentType, int> | Replicas per environment |
| ReplicasChanged | EventCallback | Replicas update callback |
| ProdCpuOvercommit | double | Production CPU overcommit ratio |
| ProdMemoryOvercommit | double | Production memory overcommit ratio |
| NonProdCpuOvercommit | double | Non-prod CPU overcommit ratio |
| NonProdMemoryOvercommit | double | Non-prod memory overcommit ratio |

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ Headroom (Buffer %)                                 │
│ [Dev: 20%] [Test: 20%] [Stage: 25%] [Prod: 30%]    │
├─────────────────────────────────────────────────────┤
│ Replicas (Pod Instances)                            │
│ [Dev: 1] [Test: 1] [Stage: 2] [Prod: 3]            │
├─────────────────────────────────────────────────────┤
│ Resource Overcommit Ratios                          │
│ Production:     CPU [1.0]  Memory [1.0]            │
│ Non-Production: CPU [2.0]  Memory [1.5]            │
└─────────────────────────────────────────────────────┘
```

---

## VM Configuration Components

### VMHADRConfig.razor

**File:** `Components/VM/VMHADRConfig.razor`

**Purpose:** Configure HA and DR patterns for VM deployments per environment.

**Design Pattern:** Horizontal accordion with inline configuration

**Key Features:**
- Uses HorizontalAccordion for environment navigation
- HA pattern selection (None, Active-Passive, Active-Active, N+1)
- DR pattern selection (None, Warm Standby, Hot Standby, Multi-Region)
- Load balancer configuration (None, Single, HA Pair, Cloud LB)
- Inline summary display

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| EnabledEnvironments | HashSet<EnvironmentType> | Enabled environments |
| EnvironmentConfigs | Dictionary<EnvironmentType, VMEnvironmentConfig> | Per-env configs |
| EnvironmentConfigsChanged | EventCallback | Config update callback |

**HA Patterns:**
| Pattern | Description | Multiplier |
|---------|-------------|------------|
| None | Single server, no redundancy | 1.0x |
| ActivePassive | Primary + standby failover | 2.0x |
| ActiveActive | Multiple active with LB | 2.0x+ |
| NPlus1 | N servers + 1 spare | 1.x× |

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ ⚙️ High Availability & Disaster Recovery            │
├─────────────────────────────────────────────────────┤
│ [Dev][Test][Stage][▼ Prod]                         │
│ ┌─────────────────────────────────────────────────┐ │
│ │ HA: [Active-Active ▼]  DR: [Hot Standby ▼]     │ │
│ │ LB: [HA Pair ▼]                                 │ │
│ │ Summary: Active-Active + Hot Standby + HA LB   │ │
│ └─────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

---

## Shared Components

### EnvironmentSlider.razor

**File:** `Components/Shared/EnvironmentSlider.razor`

**Purpose:** Navigate environments horizontally with app tier configuration.

**Design Pattern:** Left-to-right slider with navigation dots

**Key Features:**
- One environment visible at a time
- Previous/Next arrow navigation
- Navigation dots for quick jump
- Real-time stats (total apps, estimated CPU/RAM)
- Four-tier application configuration (S/M/L/XL)

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| EnabledEnvironments | HashSet<EnvironmentType> | Enabled environments |
| EnvironmentApps | Dictionary<EnvironmentType, AppConfig> | App counts per env |
| EnvironmentAppsChanged | EventCallback | App config callback |
| SmallSpec | string | Small tier specification |
| MediumSpec | string | Medium tier specification |
| LargeSpec | string | Large tier specification |
| XLargeSpec | string | XLarge tier specification |

**UI Structure:**
```
┌─────────────────────────────────────────────────────┐
│ 📦 Multi-Cluster Mode | 25 Apps | 12.5 CPU | 50 GB │
├─────────────────────────────────────────────────────┤
│         [D] [T] [S] [●P] [DR]                       │
├─────────────────────────────────────────────────────┤
│ ◀ │ ┌─────────────────────────────────┐          │ ▶│
│   │ │ P  Production Environment  8    │          │  │
│   │ │    ┌───┐ ┌───┐ ┌───┐ ┌───┐    │          │  │
│   │ │    │ S │ │ M │ │ L │ │XL │    │          │  │
│   │ │    │ 2 │ │ 3 │ │ 2 │ │ 1 │    │          │  │
│   │ │    └───┘ └───┘ └───┘ └───┘    │          │  │
│   │ └─────────────────────────────────┘          │  │
├─────────────────────────────────────────────────────┤
│                  4 of 5 environments                │
└─────────────────────────────────────────────────────┘
```

---

### HorizontalSlider.razor

**File:** `Components/Shared/HorizontalSlider.razor`

**Purpose:** Generic horizontal slider component for any item type.

**Design Pattern:** Generic templated slider with navigation

**Key Features:**
- Generic `TItem` type parameter for any item collection
- RenderFragment templates for content customization
- Optional header, navigation dots, and progress bar
- Previous/Next navigation with optional labels
- CSS class customization via callback functions

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| Items | IEnumerable<TItem> | Items to navigate |
| ItemTemplate | RenderFragment<TItem> | Template for item content |
| HeaderContent | RenderFragment? | Optional header content |
| NavDotTemplate | RenderFragment<TItem>? | Optional nav dot template |
| CurrentIndex | int | Current item index |
| CurrentIndexChanged | EventCallback<int> | Index change callback |
| OnItemChanged | EventCallback<TItem> | Item change callback |
| ItemTitleFunc | Func<TItem, string>? | Get item title |
| ItemCssClassFunc | Func<TItem, string>? | Get item CSS class |
| ShowHeader | bool | Show header (default: true) |
| ShowNavDots | bool | Show nav dots (default: true) |
| ShowProgress | bool | Show progress (default: true) |

**Usage Example:**
```razor
<HorizontalSlider TItem="EnvironmentType"
                  Items="@environments"
                  CurrentIndex="@currentIndex"
                  ItemTitleFunc="@(e => e.ToString())"
                  ItemCssClassFunc="@(e => $"env-{e.ToString().ToLower()}")">
    <ItemTemplate Context="env">
        <div class="env-config">@env configuration...</div>
    </ItemTemplate>
</HorizontalSlider>
```

---

### EnvironmentAppCard.razor

**File:** `Components/Shared/EnvironmentAppCard.razor`

**Purpose:** Expandable card for environment app tier configuration.

**Design Pattern:** Expand/collapse card with horizontal inputs

**Key Features:**
- Collapsed state shows environment name and app count
- Expanded state shows all four tier inputs horizontally
- Color-coded by environment type
- Displays tier specifications and total count

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| Environment | EnvironmentType | The environment type |
| IsExpanded | bool | Current expand state |
| IsExpandedChanged | EventCallback<bool> | Expand state callback |
| SmallCount | int | Small tier count |
| MediumCount | int | Medium tier count |
| LargeCount | int | Large tier count |
| XLargeCount | int | XLarge tier count |
| SmallSpec | string | Small tier spec display |
| MediumSpec | string | Medium tier spec display |
| LargeSpec | string | Large tier spec display |
| XLargeSpec | string | XLarge tier spec display |
| OnSmallChanged | EventCallback<int> | Small count callback |
| OnMediumChanged | EventCallback<int> | Medium count callback |
| OnLargeChanged | EventCallback<int> | Large count callback |
| OnXLargeChanged | EventCallback<int> | XLarge count callback |

**UI Structure:**
```
Collapsed:
┌───────────────────────────────────────┐
│ [P] Prod                    8 apps ▶ │
└───────────────────────────────────────┘

Expanded:
┌───────────────────────────────────────────────────────┐
│ [P] Prod                                          ▼ │
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐      │
│ │  S   │ │  M   │ │  L   │ │  XL  │ │Total │      │
│ │ [2]  │ │ [3]  │ │ [2]  │ │ [1]  │ │  8   │      │
│ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘      │
└───────────────────────────────────────────────────────┘
```

---

### EnvironmentAppGrid.razor

**File:** `Components/Shared/EnvironmentAppGrid.razor`

**Purpose:** Grid layout for multi-cluster app configuration.

**Design Pattern:** Toggle chips + expandable cards grid

**Key Features:**
- Environment toggle chips to enable/disable clusters
- Multiple EnvironmentAppCard components in horizontal layout
- Real-time stats header (total apps, CPU, RAM)
- Production environment always enabled

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| EnabledEnvironments | HashSet<EnvironmentType> | Enabled environments |
| EnvironmentApps | Dictionary<EnvironmentType, AppConfig> | App configs |
| EnabledEnvironmentsChanged | EventCallback | Enabled envs callback |
| EnvironmentAppsChanged | EventCallback | App config callback |
| SmallSpec | string | Small tier spec display |
| MediumSpec | string | Medium tier spec display |
| LargeSpec | string | Large tier spec display |
| XLargeSpec | string | XLarge tier spec display |

**UI Structure:**
```
┌─────────────────────────────────────────────────────────────┐
│ 📦 Multi-Cluster Mode            25 Apps | 12.5 CPU | 50 GB │
├─────────────────────────────────────────────────────────────┤
│ [☑ Dev] [☑ Test] [☑ Stage] [☑ Prod*] [☐ DR]                │
│ * Prod always enabled                                        │
├─────────────────────────────────────────────────────────────┤
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐            │
│ │ Dev     │ │ Test    │ │ Stage   │ │▼ Prod   │            │
│ │ 5 apps ▶│ │ 4 apps ▶│ │ 4 apps ▶│ │ S M L XL│            │
│ └─────────┘ └─────────┘ └─────────┘ │ 2 3 2 1 │            │
│                                      └─────────┘            │
└─────────────────────────────────────────────────────────────┘
```

---

## Component Relationships

### Shared Component Usage

```
Home.razor
├── EnvironmentSlider.razor ──────── Alternative app config UI (slider mode)
│
├── EnvironmentAppGrid.razor ─────── Multi-cluster app configuration
│   └── EnvironmentAppCard.razor ─── Individual environment card
│
├── HorizontalSlider.razor ───────── Generic slider (reusable)
│
├── K8sHADRPanel.razor ───────────── K8s HA/DR configuration
│
├── K8sSettingsConfig.razor ──────── K8s headroom/replicas/overcommit
│
└── VMHADRConfig.razor ───────────── VM HA/DR configuration
    └── HorizontalAccordion.razor ── Environment navigation
        └── HorizontalAccordionPanel.razor ── Panel content
```

---

## Design Patterns Summary

| Pattern | Components | Use Case |
|---------|------------|----------|
| **Horizontal Slider** | EnvironmentSlider, HorizontalSlider | Step-through navigation |
| **Expand/Collapse Card** | EnvironmentAppCard, SizingResultsView | Progressive disclosure |
| **Horizontal Accordion** | VMHADRConfig, HorizontalAccordion | Multi-panel selection |
| **Grid with Toggle Chips** | EnvironmentAppGrid | Multi-select configuration |
| **Two-Section Panel** | K8sHADRPanel | Grouped configuration |
| **Compact Grid** | K8sSettingsConfig | Dense settings layout |

---

## Authentication Components

### UserMenu.razor

**File:** `Components/Shared/UserMenu.razor`

**Purpose:** Dropdown menu for authenticated users in the header.

**Features:**
- Shows user email when authenticated
- Login/Register links for anonymous users
- Logout functionality

**Parameters:** None (uses cascading authentication state)

### Login.razor

**File:** `Components/Pages/Login.razor`
**Route:** `/login`

**Purpose:** User login page with form validation.

**Features:**
- Email/password form
- Client-side validation
- Error message display
- Redirect to home on success
- Link to registration

### Register.razor

**File:** `Components/Pages/Register.razor`
**Route:** `/register`

**Purpose:** User registration page.

**Features:**
- Email/password/confirm password form
- Password strength validation (min 8 characters)
- Confirmation matching validation
- Error message display
- Link to login

### AccessDenied.razor

**File:** `Components/Pages/AccessDenied.razor`
**Route:** `/access-denied`

**Purpose:** Display access denied message for unauthorized access.

---

## UI Helper Components

### InfoButton.razor

**File:** `Components/Shared/InfoButton.razor`

**Purpose:** Contextual help tooltip button.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `Title` | string | Tooltip title |
| `Content` | RenderFragment | Tooltip content |

### LoadingSpinner.razor

**File:** `Components/Shared/LoadingSpinner.razor`

**Purpose:** Loading indicator with customizable text.

**Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | string | "Loading..." | Display text |
| `Size` | string | "md" | Size variant (sm, md, lg) |

### SelectionCard.razor

**File:** `Components/Shared/SelectionCard.razor`

**Purpose:** Clickable selection card for wizard steps.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `Title` | string | Card title |
| `Description` | string | Card description |
| `IsSelected` | bool | Selection state |
| `OnClick` | EventCallback | Click handler |

### WizardStepper.razor

**File:** `Components/Wizard/WizardStepper.razor`

**Purpose:** Progress stepper for wizard navigation.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `Steps` | List<WizardStep> | Step definitions |
| `CurrentStep` | int | Active step index |
| `OnStepClick` | EventCallback<int> | Step click handler |

### ModalBase.razor

**File:** `Components/Modals/ModalBase.razor`

**Purpose:** Base modal component with accessibility support.

**Features:**
- Focus trapping
- Escape key handling
- Backdrop click to close
- Transition animations
- ARIA attributes

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `IsOpen` | bool | Modal visibility |
| `Title` | string | Modal title |
| `ChildContent` | RenderFragment | Modal content |
| `OnClose` | EventCallback | Close handler |
