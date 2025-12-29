# v0.4 Wireframe Implementation Plan

**Version:** 1.0
**Date:** December 29, 2024
**Status:** Draft

---

## Executive Summary

This plan details the implementation of the v0.4 "Dashboard-First" UI design, transitioning from the current wizard-based approach to a results-oriented interface with slide-out configuration panels.

### Key Changes
1. **Dashboard-First Pattern**: Show results immediately with smart defaults
2. **Slide-Out Panels**: Replace wizard steps with 480px slide-out panels
3. **Two-State UI**: Separate Guest and Authenticated user experiences
4. **Simplified Navigation**: Header-based navigation instead of sidebar wizard

---

## Current State Analysis

### Existing Architecture
```
Current Layout (Wizard-Based):
┌─────────────────────────────────────────────────────────────┐
│ HeaderBar (56px)                              Settings/Theme │
├──────────┬───────────────────────────────────┬──────────────┤
│ Left     │ Main Content                      │ Right Stats  │
│ Sidebar  │ (Wizard Step Content)             │ Sidebar      │
│ (200px)  │                                   │ (260px)      │
│          │                                   │              │
│ Steps:   │ [Platform Selection]              │ Summary      │
│ 1. Plat  │ [Technology Cards]                │ Cards        │
│ 2. Deploy│ [Distribution Grid]               │              │
│ 3. Tech  │ [Pricing Config]                  │              │
│ 4. Dist  │ [Results View]                    │              │
│ 5. Price │                                   │              │
└──────────┴───────────────────────────────────┴──────────────┘
```

### Key Files to Modify
| File | Lines | Purpose | Impact |
|------|-------|---------|--------|
| `Home.razor` | 2,742 | Main SPA interface | Major refactor |
| `MainLayout.razor` | ~100 | Root layout | Major refactor |
| `LeftSidebar.razor` | ~200 | Wizard navigation | Remove/replace |
| `RightStatsSidebar.razor` | ~150 | Stats panel | Relocate content |
| `HeaderBar.razor` | ~100 | Top navigation | Major refactor |
| `app.css` | 11,680 | All styling | Add new patterns |

### Services to Preserve
All existing services remain valid - only the UI layer changes:
- `AppStateService` - Continue using for state management
- `K8sSizingService` / `VMSizingService` - Core calculations
- `PricingService` / `CostEstimationService` - Cost calculations
- `DistributionService` / `TechnologyService` - Data services
- `AuthService` - Authentication (already implemented)

---

## Target Architecture

### v0.4 Layout (Dashboard-First)
```
Target Layout:
┌─────────────────────────────────────────────────────────────┐
│ Header (64px)        [New Scenario] [Scenarios] [@]         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  DASHBOARD CONTENT (Full Width)                             │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ Summary Cards: Nodes | vCPUs | RAM | Storage | Cost     ││
│  └─────────────────────────────────────────────────────────┘│
│                                                             │
│  ┌──────────────────────┐  ┌────────────────────────────────┤
│  │ Node Architecture    │  │ Cost Breakdown                ││
│  │ [Visualization]      │  │ [Pie Chart]                   ││
│  └──────────────────────┘  └────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ Config Bar: Platform | Dist | Apps | Nodes | Growth     ││
│  └─────────────────────────────────────────────────────────┘│
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ Action Bar (64px)  [Save] [Compare] [Export PDF] [Export XL]│
└─────────────────────────────────────────────────────────────┘

With Panel Open:
┌─────────────────────────────────────────────────────────────┐
│ Header (64px)                                               │
├───────────────────────────────────────────┬─────────────────┤
│                                           │ SLIDE PANEL     │
│  DASHBOARD (dimmed)                       │ (480px)         │
│                                           │                 │
│                                           │ [X] Close       │
│                                           │ ─────────────── │
│                                           │ Tab Navigation  │
│                                           │ [Plat][App][etc]│
│                                           │                 │
│                                           │ Panel Content   │
│                                           │ ...             │
│                                           │                 │
│                                           │ [Apply Changes] │
├───────────────────────────────────────────┴─────────────────┤
│ Action Bar (64px)                                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementation Phases

## Phase 0: Preparation (Foundation)
**Goal:** Set up infrastructure without breaking existing functionality

### 0.1 Create New Layout Components (Parallel Development)
Create in new folders to avoid conflicts:

```
Components/
├── Layout/
│   ├── MainLayout.razor          # Keep existing
│   └── DashboardLayout.razor     # NEW - v0.4 layout
├── V4/                           # NEW folder for v0.4 components
│   ├── Headers/
│   │   ├── GuestHeader.razor
│   │   └── AuthHeader.razor
│   ├── Panels/
│   │   ├── SlidePanel.razor
│   │   └── PanelTabs.razor
│   ├── Dashboard/
│   │   ├── SummaryCardsRow.razor
│   │   ├── ConfigBar.razor
│   │   └── ActionBar.razor
│   └── Guest/
│       ├── SaveBanner.razor
│       └── LimitedExportModal.razor
```

### 0.2 Add CSS for New Patterns
Add to `app.css` or create `dashboard.css`:

```css
/* Slide Panel System */
.slide-panel { ... }
.slide-panel-overlay { ... }

/* Dashboard Layout */
.dashboard-layout { ... }
.summary-cards-row { ... }
.config-bar { ... }
.action-bar { ... }

/* Two-State Headers */
.guest-header { ... }
.auth-header { ... }
```

### 0.3 Create Feature Flag
```csharp
// appsettings.json
{
  "Features": {
    "UseDashboardLayout": false  // Toggle between wizard and dashboard
  }
}
```

**Deliverables:**
- [ ] `DashboardLayout.razor` - New layout shell
- [ ] `SlidePanel.razor` - Panel container with animation
- [ ] `PanelTabs.razor` - Tab navigation for panels
- [ ] CSS additions for new patterns
- [ ] Feature flag configuration

---

## Phase 1: Core Components
**Goal:** Build reusable components for the dashboard pattern

### 1.1 Headers (2 components)

#### GuestHeader.razor
```razor
@* Guest header with auth CTAs *@
<header class="guest-header">
    <a href="/" class="logo">InfraSizing Calculator</a>
    <div class="header-actions">
        <a href="/" class="btn-new">New Scenario</a>
        <div class="auth-links">
            <a href="/login" class="btn-signin">Sign In</a>
            <a href="/register" class="btn-signup">Sign Up</a>
        </div>
    </div>
</header>
```

#### AuthHeader.razor
```razor
@* Authenticated header with navigation *@
<header class="auth-header">
    <a href="/" class="logo">InfraSizing Calculator</a>
    <nav class="header-nav">
        <button @onclick="OpenNewScenario" class="nav-btn @(IsNewActive ? "active" : "")">
            New Scenario
        </button>
        <a href="/scenarios" class="nav-btn @(IsScenariosActive ? "active" : "")">
            Scenarios
        </a>
        <a href="/settings" class="nav-btn nav-icon">
            <span class="icon-settings"></span>
        </a>
    </nav>
</header>
```

### 1.2 Slide Panel System (2 components)

#### SlidePanel.razor
```razor
@* Reusable slide-out panel container *@
<div class="slide-panel-overlay @(IsOpen ? "visible" : "")" @onclick="Close">
</div>
<aside class="slide-panel @(IsOpen ? "open" : "")">
    <div class="panel-header">
        <h2>@Title</h2>
        <button class="btn-close" @onclick="Close">×</button>
    </div>
    <div class="panel-tabs">
        @ChildTabs
    </div>
    <div class="panel-content">
        @ChildContent
    </div>
    <div class="panel-footer">
        <button class="btn-apply" @onclick="ApplyChanges">Apply Changes</button>
    </div>
</aside>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public RenderFragment ChildTabs { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnApply { get; set; }
}
```

#### PanelTabs.razor
```razor
@* Tab navigation within panels *@
<div class="panel-tabs">
    @foreach (var tab in Tabs)
    {
        <button class="panel-tab @(ActiveTab == tab.Id ? "active" : "")"
                @onclick="() => SelectTab(tab.Id)">
            @tab.Label
        </button>
    }
</div>

@code {
    [Parameter] public List<TabItem> Tabs { get; set; }
    [Parameter] public string ActiveTab { get; set; }
    [Parameter] public EventCallback<string> OnTabChange { get; set; }
}
```

### 1.3 Dashboard Components (3 components)

#### SummaryCardsRow.razor
```razor
@* Top row of summary metric cards *@
<div class="summary-cards-row">
    <div class="summary-card nodes">
        <span class="card-value">@TotalNodes</span>
        <span class="card-label">Nodes</span>
    </div>
    <div class="summary-card">
        <span class="card-value">@TotalCPU</span>
        <span class="card-label">vCPUs</span>
    </div>
    <div class="summary-card">
        <span class="card-value">@TotalRAM</span>
        <span class="card-label">GB RAM</span>
    </div>
    <div class="summary-card">
        <span class="card-value">@TotalStorage</span>
        <span class="card-label">TB Storage</span>
    </div>
    <div class="summary-card cost">
        <span class="card-value">@MonthlyCost</span>
        <span class="card-label">/month</span>
    </div>
</div>
```

#### ConfigBar.razor
```razor
@* Current configuration summary with edit buttons *@
<div class="config-bar">
    <div class="config-item" @onclick="() => OpenPanel("platform")">
        <span class="config-label">Platform</span>
        <span class="config-value">@CurrentPlatform</span>
        <span class="config-edit">Edit</span>
    </div>
    <!-- Similar for: Distribution, Technology, Apps, Nodes, Pricing, Growth -->
</div>
```

#### ActionBar.razor
```razor
@* Bottom action bar for authenticated users *@
<div class="action-bar">
    <button class="btn-action primary" @onclick="SaveScenario">
        Save Scenario
    </button>
    <button class="btn-action" @onclick="CompareScenarios">
        Compare
    </button>
    <button class="btn-action" @onclick="() => Export("pdf")">
        Export PDF
    </button>
    <button class="btn-action" @onclick="() => Export("excel")">
        Export Excel
    </button>
</div>
```

**Deliverables:**
- [ ] `GuestHeader.razor` - Guest navigation
- [ ] `AuthHeader.razor` - Authenticated navigation
- [ ] `SlidePanel.razor` - Panel container
- [ ] `PanelTabs.razor` - Tab navigation
- [ ] `SummaryCardsRow.razor` - Metric cards
- [ ] `ConfigBar.razor` - Config summary
- [ ] `ActionBar.razor` - Action buttons

---

## Phase 2: Guest Flow
**Goal:** Implement the guest user experience

### 2.1 Guest-Specific Components

#### SaveBanner.razor
```razor
@* Shown on guest results page *@
<div class="save-banner">
    <div class="save-banner-content">
        <div class="save-banner-icon">
            <span class="icon-bookmark"></span>
        </div>
        <div class="save-banner-text">
            <h3>Save your scenario</h3>
            <p>Sign in to save, compare scenarios, and access your calculations later.</p>
        </div>
    </div>
    <div class="save-banner-actions">
        <a href="/login?returnUrl=@CurrentUrl" class="btn-save-signin">Sign In to Save</a>
        <a href="/register" class="btn-save-signup">Create Account</a>
    </div>
</div>
```

#### LimitedExportModal.razor
```razor
@* Export modal with guest restrictions *@
<div class="export-modal guest">
    <div class="format-options">
        <div class="format-option selected">
            <span class="format-icon pdf"></span>
            <span class="format-name">PDF</span>
        </div>
        <div class="format-option disabled">
            <span class="format-icon excel"></span>
            <span class="format-name">Excel</span>
            <span class="pro-badge">PRO</span>
        </div>
        <div class="format-option disabled">
            <span class="format-icon json"></span>
            <span class="format-name">JSON</span>
            <span class="pro-badge">PRO</span>
        </div>
    </div>

    <div class="content-options">
        <div class="option-row">
            <input type="checkbox" checked disabled />
            <span>Summary Overview</span>
        </div>
        <div class="option-row disabled">
            <input type="checkbox" disabled />
            <span>Node Details</span>
            <span class="signin-badge">Sign In</span>
        </div>
        <!-- More options with Sign In badge -->
    </div>

    <div class="watermark-warning">
        Guest exports include watermark
    </div>

    <div class="upgrade-prompt">
        <a href="/register">Create a free account</a> for full export options
    </div>
</div>
```

### 2.2 Guest Landing Page
Modify or create `GuestLanding.razor`:
- Single "Create New Scenario" card
- Sign-in prompts
- Feature highlights

### 2.3 Guest Results Page
Modify or create `GuestResults.razor`:
- Full dashboard with calculations
- `SaveBanner` at top
- Limited export functionality

**Deliverables:**
- [ ] `SaveBanner.razor` - Save prompt for guests
- [ ] `LimitedExportModal.razor` - Restricted export
- [ ] `GuestLanding.razor` - Guest entry point
- [ ] Guest-specific routing logic

---

## Phase 3: Authenticated Flow
**Goal:** Implement the full authenticated experience

### 3.1 Dashboard Page
Create `Dashboard.razor` (or significantly refactor `Home.razor`):

```razor
@page "/"
@page "/dashboard"
@page "/dashboard/{ScenarioId:guid?}"

@inject IAppStateService AppState
@inject AuthenticationStateProvider AuthState

<DashboardLayout>
    @if (IsGuest)
    {
        <GuestHeader />
    }
    else
    {
        <AuthHeader OnNewScenario="OpenConfigPanel" />
    }

    <main class="dashboard-content">
        @if (IsGuest && !HasStartedConfig)
        {
            <GuestLanding OnStart="StartConfiguration" />
        }
        else
        {
            @if (IsGuest)
            {
                <SaveBanner />
            }

            <SummaryCardsRow
                TotalNodes="@Results.TotalNodes"
                TotalCPU="@Results.TotalCPU"
                TotalRAM="@Results.TotalRAM"
                MonthlyCost="@Results.MonthlyCost" />

            <div class="dashboard-grid">
                <NodeArchitectureView Results="@Results" />
                <CostBreakdownView Costs="@Costs" />
            </div>

            <ConfigBar
                Config="@CurrentConfig"
                OnEdit="OpenConfigPanel" />
        }
    </main>

    @if (!IsGuest)
    {
        <ActionBar
            OnSave="SaveScenario"
            OnCompare="OpenCompare"
            OnExport="OpenExport" />
    }

    <SlidePanel
        IsOpen="@IsPanelOpen"
        Title="@PanelTitle"
        OnClose="ClosePanel"
        OnApply="ApplyConfig">
        <ChildTabs>
            <PanelTabs Tabs="@ConfigTabs" ActiveTab="@ActiveTab" OnTabChange="SwitchTab" />
        </ChildTabs>
        <ChildContent>
            @switch (ActiveTab)
            {
                case "platform":
                    <PlatformConfigPanel Config="@Config" />
                    break;
                case "apps":
                    <AppsConfigPanel Config="@Config" />
                    break;
                case "nodes":
                    <NodeSpecsConfigPanel Config="@Config" />
                    break;
                case "pricing":
                    <PricingConfigPanel Config="@Config" />
                    break;
                case "growth":
                    <GrowthConfigPanel Config="@Config" />
                    break;
            }
        </ChildContent>
    </SlidePanel>
</DashboardLayout>
```

### 3.2 Scenarios List Enhancement
Update `Scenarios.razor`:
- Grid layout with scenario cards
- Load/Compare/Delete actions
- Search and filter

### 3.3 Settings Page Enhancement
Update `Settings.razor`:
- Two-column layout
- Settings categories navigation
- Profile management

**Deliverables:**
- [ ] `Dashboard.razor` - Main dashboard page
- [ ] Enhanced `Scenarios.razor` - Scenario management
- [ ] Enhanced `Settings.razor` - User settings
- [ ] Export modal (full version)

---

## Phase 4: Configuration Panels
**Goal:** Convert wizard steps to panel-based configuration

### 4.1 Panel Components (Adapt Existing)

Most configuration logic exists - wrap in panel format:

| Current Component | Panel Version |
|-------------------|---------------|
| `PlatformStep.razor` | `PlatformConfigPanel.razor` |
| `TechnologyStep.razor` | Part of Platform panel |
| `DistributionStep.razor` | Part of Platform panel |
| `AppCountsPanel.razor` | `AppsConfigPanel.razor` |
| `NodeSpecsPanel.razor` | `NodeSpecsConfigPanel.razor` |
| `PricingSelector.razor` | `PricingConfigPanel.razor` |
| `GrowthSettingsPanel.razor` | `GrowthConfigPanel.razor` |

### 4.2 Panel-Specific Adaptations

Each panel needs:
1. **Compact layout** - Fit in 480px width
2. **Apply button integration** - Work with `SlidePanel` footer
3. **Live preview** - Optional instant feedback
4. **Mobile-friendly** - Touch targets, scrollable content

### 4.3 Reuse Existing Components

These can be reused directly:
- `SelectionCard.razor` - For platform/tech/distribution selection
- `EnvironmentSlider.razor` - For app counts
- `EnvironmentAppGrid.razor` - For environment breakdown
- `FilterButtons.razor` - For filtering distributions

**Deliverables:**
- [ ] `PlatformConfigPanel.razor` - Platform/Tech/Distribution
- [ ] `AppsConfigPanel.razor` - App counts
- [ ] `NodeSpecsConfigPanel.razor` - Node specifications
- [ ] `PricingConfigPanel.razor` - Pricing options
- [ ] `GrowthConfigPanel.razor` - Growth planning

---

## Phase 5: Polish & Integration
**Goal:** Final integration, testing, and refinement

### 5.1 Layout Transition
- Update `MainLayout.razor` to use `DashboardLayout` by default
- Remove or deprecate wizard-specific components
- Clean up unused CSS

### 5.2 State Management Updates
Enhance `AppStateService`:
```csharp
// Add panel state
public string ActivePanel { get; set; }
public bool IsPanelOpen => !string.IsNullOrEmpty(ActivePanel);

// Add guest state helpers
public bool IsGuestMode { get; set; }
public bool HasStartedConfiguration { get; set; }
```

### 5.3 Animation & Transitions
```css
/* Slide panel animation */
.slide-panel {
    transform: translateX(100%);
    transition: transform 0.3s ease-out;
}
.slide-panel.open {
    transform: translateX(0);
}

/* Overlay fade */
.slide-panel-overlay {
    opacity: 0;
    transition: opacity 0.3s ease-out;
}
.slide-panel-overlay.visible {
    opacity: 1;
}
```

### 5.4 Responsive Adaptations
```css
/* Mobile: Full-screen panels */
@media (max-width: 768px) {
    .slide-panel {
        width: 100%;
    }
    .summary-cards-row {
        grid-template-columns: repeat(2, 1fr);
    }
}
```

### 5.5 Testing
- Unit tests for new components
- Integration tests for panel flow
- E2E tests for guest/auth paths
- Accessibility audit

**Deliverables:**
- [ ] Layout switch complete
- [ ] State management updates
- [ ] CSS animations
- [ ] Responsive breakpoints
- [ ] Test coverage
- [ ] Accessibility compliance

---

## File Change Summary

### New Files (17)
```
Components/V4/
├── Headers/
│   ├── GuestHeader.razor
│   └── AuthHeader.razor
├── Panels/
│   ├── SlidePanel.razor
│   ├── PanelTabs.razor
│   ├── PlatformConfigPanel.razor
│   ├── AppsConfigPanel.razor
│   ├── NodeSpecsConfigPanel.razor
│   ├── PricingConfigPanel.razor
│   └── GrowthConfigPanel.razor
├── Dashboard/
│   ├── SummaryCardsRow.razor
│   ├── ConfigBar.razor
│   ├── ActionBar.razor
│   ├── NodeArchitectureView.razor
│   └── CostBreakdownView.razor
└── Guest/
    ├── SaveBanner.razor
    ├── LimitedExportModal.razor
    └── GuestLanding.razor
```

### Modified Files (8)
```
Components/
├── Layout/
│   └── MainLayout.razor         # Add DashboardLayout option
├── Pages/
│   ├── Home.razor               # Refactor to Dashboard
│   ├── Scenarios.razor          # Update layout
│   └── Settings.razor           # Update layout
wwwroot/
└── css/
    └── app.css                  # Add dashboard styles
Services/
└── AppStateService.cs           # Add panel state
appsettings.json                 # Add feature flag
```

### Deprecated Files (4)
```
Components/
├── Layout/
│   ├── LeftSidebar.razor        # No longer needed
│   └── RightStatsSidebar.razor  # Content moved to dashboard
└── Wizard/
    ├── WizardContainer.razor    # Replaced by panels
    └── WizardStepper.razor      # Replaced by tabs
```

---

## Risk Mitigation

### High Risk Areas
1. **Home.razor complexity** - 2,742 lines, heavy refactor
   - *Mitigation*: Incremental extraction, feature flag for rollback

2. **State management changes** - Could break existing flows
   - *Mitigation*: Extend `AppStateService`, don't replace

3. **CSS conflicts** - New patterns may clash with existing
   - *Mitigation*: Use BEM naming, scope new styles

### Rollback Strategy
- Keep wizard components until v0.4 is stable
- Feature flag to switch between layouts
- Maintain parallel routes during transition

---

## Testing Strategy

### Unit Tests
- All new components with bUnit
- State service changes
- Panel open/close logic

### Integration Tests
- Guest → Auth transition
- Panel configuration → Results update
- Save/Load scenarios

### E2E Tests (Playwright)
- Complete guest flow
- Complete authenticated flow
- Export functionality
- Mobile responsive behavior

---

## Timeline Estimate

| Phase | Components | Complexity |
|-------|------------|------------|
| Phase 0: Preparation | 5 | Medium |
| Phase 1: Core Components | 7 | Medium |
| Phase 2: Guest Flow | 4 | Medium |
| Phase 3: Auth Flow | 4 | High |
| Phase 4: Config Panels | 5 | Medium |
| Phase 5: Polish | N/A | Medium |

**Total new components:** 25
**Total modified files:** 8
**Total deprecated files:** 4

---

## Success Criteria

1. [ ] Dashboard shows results immediately with smart defaults
2. [ ] All configuration accessible via slide-out panels
3. [ ] Guest flow works with save prompts and limited export
4. [ ] Authenticated flow has full functionality
5. [ ] No regression in calculation accuracy
6. [ ] Responsive design works on tablet/mobile
7. [ ] All existing tests pass
8. [ ] New components have 80%+ test coverage

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 29, 2024 | Initial implementation plan |
