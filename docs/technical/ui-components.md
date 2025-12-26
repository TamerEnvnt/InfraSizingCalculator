# UI Components Reference

This document describes the Blazor UI components in the Infrastructure Sizing Calculator.

---

## Component Hierarchy

```
App.razor
└── MainLayout.razor
    ├── HeaderBar.razor
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
    │   │   └── K8sNodeSpecsConfig.razor
    │   ├── VM Configuration (Components/VM/)
    │   │   ├── VMServerRolesConfig.razor
    │   │   └── VMHADRConfig.razor
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
    │   │   ├── Info Modal
    │   │   └── Save Scenario Modal
    │   └── Configuration (Components/Configuration/)
    │       ├── AppCountsPanel.razor
    │       ├── NodeSpecsPanel.razor
    │       ├── SettingsPanel.razor
    │       ├── ClusterModeSelector.razor
    │       └── PricingSelector.razor
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
