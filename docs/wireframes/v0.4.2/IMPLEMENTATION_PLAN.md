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

### 1.4 Test Selector Requirements

**CRITICAL:** All interactive components MUST include `data-testid` attributes for E2E testing.

| Component | Required Selectors |
|-----------|-------------------|
| SummaryCardsRow | `data-testid="total-nodes"`, `data-testid="total-cpu"`, `data-testid="total-ram"`, `data-testid="total-storage"`, `data-testid="monthly-cost"` |
| ConfigBar | `data-testid="config-platform"`, `data-testid="config-apps"`, `data-testid="config-nodes"`, etc. |
| ActionBar | `data-testid="save-scenario"`, `data-testid="compare-scenarios"`, `data-testid="export-pdf"`, `data-testid="export-excel"` |
| SlidePanel | `data-testid="slide-panel"`, `data-testid="panel-close"`, `data-testid="panel-apply"` |
| Headers | `data-testid="create-scenario"`, `data-testid="nav-scenarios"`, `data-testid="nav-settings"` |

**Pattern:**
```razor
<div class="summary-card nodes" data-testid="total-nodes">
    <span class="card-value">@TotalNodes</span>
    <span class="card-label">Nodes</span>
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
- [ ] All components include `data-testid` attributes

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
| 1.1 | Dec 29, 2024 | Added Performance, Security, Architecture, and Testing sections |
| 1.2 | Dec 29, 2024 | Added UX review feedback: data-testid requirements, colorblind accessibility, Bottom Bar recommendation, session timeout unsaved changes warning |
| 1.3 | Dec 29, 2024 | Updated wireframe references to v0.4.2, added Phase Dependency Matrix |

---

## Phase Dependency Matrix

This matrix shows dependencies between implementation phases to assist with planning and parallel work.

| Phase | Depends On | Blocks | Can Parallelize With |
|-------|------------|--------|---------------------|
| **Phase 0: Preparation** | None | Phase 1, 2, 3, 4, 5 | None |
| **Phase 1: Core Components** | Phase 0 complete | Phase 2, 3, 4 | None |
| **Phase 2: Guest Flow** | Phase 1 (headers, panels) | Phase 3 (partial) | Phase 4 (config panels) |
| **Phase 3: Authenticated Flow** | Phase 1, Phase 2 (UX patterns) | Phase 5 | Phase 4 (config panels) |
| **Phase 4: Configuration Panels** | Phase 1 (SlidePanel) | Phase 5 | Phase 2, 3 (after Phase 1) |
| **Phase 5: Polish & Integration** | Phase 1, 2, 3, 4 complete | None | None |

### Critical Path

```
Phase 0 → Phase 1 → Phase 2 → Phase 3 → Phase 5
              ↓                            ↑
           Phase 4 ─────────────────────────┘
```

### Parallelization Opportunities

1. **After Phase 1 completes:**
   - Team A: Phase 2 (Guest Flow)
   - Team B: Phase 4 (Configuration Panels - can share SlidePanel component)

2. **After Phase 2 completes:**
   - Team A: Phase 3 (Authenticated Flow)
   - Team B: Continue Phase 4 (if not complete)

### Blockers & Dependencies Detail

| Task | Hard Dependency | Reason |
|------|-----------------|--------|
| GuestHeader | DashboardLayout | Header is rendered within layout |
| AuthHeader | GuestHeader | Extends guest header pattern |
| SlidePanel | CSS variables | Needs panel animation styles |
| ConfigBar | SlidePanel | Opens panels on click |
| SaveBanner | AuthService | Checks authentication state |
| ActionBar | All services | Triggers save/export operations |
| ScenariosPage | ScenarioRepository | Requires DB access |

---

## Appendix A: Performance Considerations

### A.1 Dashboard Recalculation Debouncing

**Problem:** Every configuration change triggers a full recalculation, which can cause UI lag.

**Solution:** Implement debounced recalculation service.

```csharp
// Services/DebounceService.cs
public class DebounceService : IDisposable
{
    private CancellationTokenSource? _cts;
    private readonly ILogger<DebounceService> _logger;

    public DebounceService(ILogger<DebounceService> logger)
    {
        _logger = logger;
    }

    public async Task DebounceAsync(Func<Task> action, int delayMs = 300)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await Task.Delay(delayMs, _cts.Token);
            await action();
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("Debounced action cancelled - newer request pending");
        }
    }

    public void Dispose() => _cts?.Dispose();
}
```

**Integration Point:** Phase 3 - Dashboard.razor should use this for config changes.

### A.2 Slide Panel Animation Performance

**Problem:** CSS transforms can cause layout thrashing on lower-end devices.

**Solution:** Use GPU-accelerated properties and `will-change`.

```css
/* In dashboard.css - add to Phase 0 */
.slide-panel {
    transform: translateX(100%);
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    will-change: transform;
    contain: layout style paint;
}

.slide-panel-overlay {
    opacity: 0;
    transition: opacity 0.25s ease-out;
    will-change: opacity;
    pointer-events: none;
}

.slide-panel-overlay.visible {
    opacity: 1;
    pointer-events: auto;
}
```

### A.3 Chart Lazy Loading

**Problem:** Multiple charts on dashboard increase initial load time.

**Solution:** Lazy-load charts that are not in viewport.

```razor
<!-- In Dashboard.razor -->
@if (IsInViewport("cost-breakdown"))
{
    <CostBreakdownView Costs="@Costs" />
}
else
{
    <ChartPlaceholder Height="300" Label="Cost Breakdown" />
}
```

**Integration Point:** Phase 5 - Add viewport intersection observer.

### A.4 Progressive Loading Strategy

**Load Order:**
1. Header + Summary Cards (immediate)
2. Config Bar (immediate)
3. Node Architecture diagram (deferred 100ms)
4. Cost Charts (deferred 200ms)
5. Growth Projections (on-demand when tab selected)

---

## Appendix B: Security Considerations

### B.1 URL Configuration Validation (CRITICAL)

**Problem:** Shareable URLs with configuration state can be exploited for XSS or injection attacks.

**Solution:** Server-side validation of all URL parameters.

```csharp
// Services/Validation/UrlConfigurationValidator.cs
public class UrlConfigurationValidator : IUrlConfigurationValidator
{
    private readonly ILogger<UrlConfigurationValidator> _logger;
    private readonly IInputValidationService _inputValidator;

    public UrlConfigurationValidator(
        ILogger<UrlConfigurationValidator> logger,
        IInputValidationService inputValidator)
    {
        _logger = logger;
        _inputValidator = inputValidator;
    }

    public ValidationResult ValidateConfigurationFromUrl(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return ValidationResult.Success();

        var errors = new List<string>();

        // 1. Size limit (prevent DoS)
        if (configJson.Length > 10000)
        {
            errors.Add("Configuration too large (SEC-001)");
            return ValidationResult.Failure(errors);
        }

        // 2. JSON structure validation
        try
        {
            var config = JsonSerializer.Deserialize<SizingConfiguration>(
                configJson,
                new JsonSerializerOptions { MaxDepth = 5 });

            if (config == null)
            {
                errors.Add("Invalid configuration format (SEC-002)");
                return ValidationResult.Failure(errors);
            }

            // 3. Business rule validation
            var appValidation = _inputValidator.ValidateAppCounts(
                config.SmallApps, config.MediumApps, config.LargeApps);
            if (!appValidation.IsValid)
                errors.AddRange(appValidation.Errors);

            // 4. Enum range validation
            if (!Enum.IsDefined(typeof(Distribution), config.Distribution))
                errors.Add("Invalid distribution value (SEC-003)");

            if (!Enum.IsDefined(typeof(Technology), config.Technology))
                errors.Add("Invalid technology value (SEC-003)");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Malformed JSON in URL configuration");
            errors.Add("Malformed configuration data (SEC-002)");
        }

        return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }
}
```

**Integration Point:** Phase 4 - Add validation before applying URL config.

### B.2 Server-Side Export Watermarking

**Problem:** Client-side watermarking can be bypassed by guests.

**Solution:** Generate watermarked PDFs server-side.

```csharp
// Services/Export/WatermarkService.cs
public class WatermarkService : IWatermarkService
{
    public byte[] ApplyGuestWatermark(byte[] pdfContent, string ipAddress)
    {
        // Use iTextSharp or similar to add watermark
        // Include: "GUEST EXPORT", timestamp, hashed IP
        // Watermark should be diagonal across each page
    }
}
```

**Integration Point:** Phase 2 - Guest export flow.

### B.3 Session Timeout Handling

**Problem:** Authenticated sessions can remain active indefinitely.

**Solution:** Implement idle timeout with warning that includes unsaved changes detection.

```razor
<!-- SessionTimeoutMonitor.razor -->
@inject ISessionService SessionService
@inject IAppStateService AppState
@implements IDisposable

@if (ShowTimeoutWarning)
{
    <div class="session-timeout-modal" role="alertdialog" aria-modal="true">
        <div class="modal-overlay"></div>
        <div class="modal-content">
            <div class="timeout-icon warning"></div>
            <h3>Session Expiring</h3>
            <p>Your session will expire in <strong>@RemainingMinutes:@RemainingSeconds</strong></p>

            @if (HasUnsavedChanges)
            {
                <div class="unsaved-warning" role="alert">
                    <span class="warning-icon"></span>
                    <span>You have unsaved changes that will be lost.</span>
                </div>
            }

            <div class="modal-actions">
                <button class="btn-primary" @onclick="ExtendSession">Stay Signed In</button>
                <button class="btn-secondary" @onclick="SignOut">Sign Out</button>
            </div>
        </div>
    </div>
}

@code {
    private Timer? _idleTimer;
    private Timer? _countdownTimer;
    private bool ShowTimeoutWarning;
    private int RemainingMinutes = 5;
    private int RemainingSeconds = 0;
    private bool HasUnsavedChanges => AppState.HasModifications;

    protected override void OnInitialized()
    {
        // Show warning 5 minutes before expiry
        _idleTimer = new Timer(CheckIdleTime, null, TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(1));
    }

    private void StartCountdown()
    {
        ShowTimeoutWarning = true;
        _countdownTimer = new Timer(UpdateCountdown, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
}
```

**Key UX Considerations:**
- Show warning 5 minutes before expiry (per wireframe spec)
- Display countdown timer to create urgency without excessive anxiety
- **CRITICAL:** Warn users about unsaved changes before session expires
- Auto-logout when countdown reaches 0
- Orange accent color for warning state (not red - saves red for errors)

**Integration Point:** Phase 3 - Add to AuthHeader component.

### B.4 Rate Limiting for Calculations

**Problem:** Malicious users could spam calculations to overload server.

**Solution:** Add rate limiting middleware.

```csharp
// Middleware/CalculationRateLimitMiddleware.cs
// Limit: 60 calculations per minute per session
```

**Integration Point:** Phase 5 - Add to Program.cs.

---

## Appendix C: Architecture Improvements

### C.1 Home.razor Decomposition Plan

**Current:** 2,742 lines in single file.

**Target:** Maximum 300 lines per component.

```
Home.razor (2,742 lines)
    │
    ▼ DECOMPOSE TO:
    │
    ├── Pages/Dashboard.razor (~200 lines)
    │   └── Orchestration only, no business logic
    │
    ├── Contexts/
    │   ├── ConfigurationContext.razor (~50 lines)
    │   │   └── Cascading configuration state
    │   └── ResultsContext.razor (~50 lines)
    │       └── Cascading calculation results
    │
    ├── Dashboard/
    │   ├── SummaryCardsRow.razor (~80 lines)
    │   ├── NodeArchitectureView.razor (~150 lines)
    │   ├── CostBreakdownView.razor (~120 lines)
    │   ├── ConfigBar.razor (~100 lines)
    │   └── ActionBar.razor (~60 lines)
    │
    ├── Panels/
    │   ├── SlidePanel.razor (~80 lines)
    │   ├── PlatformConfigPanel.razor (~200 lines)
    │   ├── AppsConfigPanel.razor (~180 lines)
    │   ├── NodeSpecsConfigPanel.razor (~200 lines)
    │   ├── PricingConfigPanel.razor (~180 lines)
    │   └── GrowthConfigPanel.razor (~150 lines)
    │
    └── Guest/
        ├── GuestLanding.razor (~100 lines)
        ├── SaveBanner.razor (~50 lines)
        └── LimitedExportModal.razor (~100 lines)

TOTAL: ~1,848 lines across 16 files (avg 115 lines/file)
REDUCTION: 894 lines of duplicated/dead code removed
```

### C.2 Extraction Order (Minimize Risk)

Execute in this order to minimize breaking changes:

| Step | Component | Dependencies | Risk |
|------|-----------|--------------|------|
| 1 | SummaryCardsRow | None | Low |
| 2 | ActionBar | None | Low |
| 3 | ConfigBar | None | Low |
| 4 | SlidePanel | None | Low |
| 5 | NodeArchitectureView | Results | Medium |
| 6 | CostBreakdownView | Results, Costs | Medium |
| 7 | Configuration Panels (5) | Config state | Medium |
| 8 | ConfigurationContext | Services | Medium |
| 9 | ResultsContext | Services | Medium |
| 10 | Dashboard.razor | All above | High |

### C.3 State Management Pattern

**Current:** Mixed state across `AppStateService` and component local state.

**Target:** Centralized immutable state with actions.

```csharp
// Services/State/DashboardState.cs
public record DashboardState
{
    public SizingConfiguration Configuration { get; init; } = new();
    public SizingResults? Results { get; init; }
    public CostEstimate? Costs { get; init; }
    public string? ActivePanel { get; init; }
    public bool IsCalculating { get; init; }
    public string? ErrorMessage { get; init; }
}

// Services/State/DashboardStateService.cs
public class DashboardStateService
{
    private DashboardState _state = new();
    public event Action<DashboardState>? OnStateChanged;

    public DashboardState Current => _state;

    // Undo/Redo support
    private readonly Stack<DashboardState> _undoStack = new();
    private readonly Stack<DashboardState> _redoStack = new();

    public void UpdateConfiguration(Func<SizingConfiguration, SizingConfiguration> updater)
    {
        _undoStack.Push(_state);
        _redoStack.Clear();

        _state = _state with
        {
            Configuration = updater(_state.Configuration)
        };

        OnStateChanged?.Invoke(_state);
    }

    public void Undo()
    {
        if (_undoStack.Count == 0) return;
        _redoStack.Push(_state);
        _state = _undoStack.Pop();
        OnStateChanged?.Invoke(_state);
    }

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
}
```

**Integration Point:** Phase 3 - Replace ad-hoc state management.

---

## Appendix D: Testing Strategy

### D.1 Test Coverage Targets

| Component Type | Target Coverage | Tool |
|----------------|-----------------|------|
| Services | 90%+ | xUnit |
| Blazor Components | 80%+ | bUnit |
| Integration | Key flows | xUnit |
| E2E | Critical paths | Playwright |
| Visual Regression | All screens | Playwright |
| Accessibility | All pages | axe-core |
| Performance | API endpoints | K6 |

### D.2 Unit Test Structure

```
tests/InfraSizingCalculator.UnitTests/
├── Components/
│   └── V4/
│       ├── Dashboard/
│       │   ├── SummaryCardsRowTests.cs
│       │   ├── ConfigBarTests.cs
│       │   └── ActionBarTests.cs
│       ├── Panels/
│       │   ├── SlidePanelTests.cs
│       │   └── PanelTabsTests.cs
│       ├── Headers/
│       │   ├── GuestHeaderTests.cs
│       │   └── AuthHeaderTests.cs
│       └── Guest/
│           ├── SaveBannerTests.cs
│           └── LimitedExportModalTests.cs
├── Services/
│   ├── DebounceServiceTests.cs
│   ├── UrlConfigurationValidatorTests.cs
│   └── DashboardStateServiceTests.cs
```

### D.3 E2E Test Scenarios

```csharp
// tests/InfraSizingCalculator.E2ETests/V4/
public class GuestFlowTests : PlaywrightTest
{
    [Test]
    public async Task Guest_CanViewDashboard_WithDefaultCalculations()
    {
        await Page.GotoAsync("/");
        await Expect(Page.Locator(".summary-cards-row")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='total-nodes']")).ToContainTextAsync("12");
    }

    [Test]
    public async Task Guest_SeeSaveBanner_WhenViewingResults()
    {
        await Page.GotoAsync("/");
        await Page.ClickAsync("[data-testid='create-scenario']");
        await Expect(Page.Locator(".save-banner")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Guest_CanExportLimitedPdf()
    {
        // ...
    }
}

public class AuthFlowTests : PlaywrightTest
{
    [Test]
    public async Task Auth_CanOpenConfigPanel_AndApplyChanges()
    {
        await LoginAsync();
        await Page.ClickAsync("[data-testid='config-platform']");
        await Expect(Page.Locator(".slide-panel")).ToHaveClassAsync("open");
        // ...
    }
}
```

### D.4 Visual Regression Testing

Add to CI pipeline:

```yaml
# .github/workflows/ci.yml - add to existing
visual-regression:
  name: Visual Regression Tests
  runs-on: ubuntu-latest
  needs: build-and-test
  steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Install Playwright
      run: pwsh bin/Debug/net10.0/playwright.ps1 install

    - name: Run Visual Tests
      run: dotnet test tests/InfraSizingCalculator.E2ETests --filter "Category=Visual"

    - name: Upload Screenshots
      uses: actions/upload-artifact@v4
      if: failure()
      with:
        name: visual-regression-screenshots
        path: tests/InfraSizingCalculator.E2ETests/screenshots/
```

### D.5 Accessibility Testing

Add to CI:

```yaml
accessibility:
  name: Accessibility Audit
  runs-on: ubuntu-latest
  needs: build-and-test
  steps:
    - uses: actions/checkout@v4

    - name: Start Application
      run: |
        dotnet run --project src/InfraSizingCalculator &
        sleep 10

    - name: Run axe-core
      run: |
        npx @axe-core/cli http://localhost:5000 --exit --tags wcag2a,wcag2aa

    - name: Run Pa11y
      run: |
        npx pa11y http://localhost:5000 --standard WCAG2AA
```

### D.6 Performance Benchmarks

```yaml
performance:
  name: Performance Benchmarks
  runs-on: ubuntu-latest
  needs: build-and-test
  steps:
    - name: Run K6 Load Tests
      run: |
        k6 run tests/performance/sizing-api.js --out json=results.json

    - name: Check Thresholds
      run: |
        # API response time < 200ms p95
        # Calculation time < 500ms p95
        # Memory usage < 512MB
```

---

## Appendix E: UX Items Requiring Visual Review

> **IMPORTANT:** The following UX changes require visual mockups before implementation.
> See `docs/wireframes/v0.4.2/html/` for complete wireframes including error states, loading states, mobile adaptations, and micro-interactions.

### E.1 Error States (Wireframed in 13-ux-states-review.html)

| Screen | Description | Priority |
|--------|-------------|----------|
| API Failure | Dashboard with API error message | HIGH |
| Invalid Config | Config panel with validation errors | HIGH |
| Session Timeout | Warning modal before session expires | MEDIUM |
| Empty Scenarios | Scenarios page with no saved data | MEDIUM |
| Export Failed | Export modal with failure message | MEDIUM |

**Accessibility Requirement (WCAG 2.1 AA):**

> **IMPORTANT:** Error states MUST NOT rely on color alone to convey meaning.

Current wireframes use red for errors, which is insufficient for colorblind users (~8% of males).

**Required Pattern:**
```razor
<!-- Use BOTH color AND icon/shape -->
<div class="error-state" role="alert">
    <span class="error-icon" aria-hidden="true">⚠</span>  <!-- Shape indicator -->
    <span class="error-text">Unable to calculate sizing</span>
</div>
```

**Icon Requirements:**
| State | Color | Icon Shape | Description |
|-------|-------|------------|-------------|
| Error | Red (#ef4444) | ⚠ Triangle | Critical failures, validation errors |
| Warning | Orange (#f97316) | ⚡ Lightning | Session timeout, rate limits |
| Success | Green (#22c55e) | ✓ Checkmark | Save success, export complete |
| Info | Blue (#3b82f6) | ℹ Circle | Tips, guidance |

**CSS Example:**
```css
.error-state {
    border-left: 4px solid var(--color-error);  /* Color */
    background: rgba(239, 68, 68, 0.1);
}

.error-icon::before {
    content: "⚠";  /* Shape - visible even without color */
    font-size: 1.25rem;
    margin-right: 0.5rem;
}
```

### E.2 Loading States

| Component | Current | Proposed |
|-----------|---------|----------|
| Dashboard Initial | None | Skeleton cards + shimmer |
| Panel Content | None | Skeleton form fields |
| Chart Rendering | None | Placeholder with spinner |
| Calculation | None | Summary cards pulse |

### E.3 Mobile Adaptations (< 768px)

| Pattern | Current | Proposed |
|---------|---------|----------|
| Panel | Slide from right (480px) | Full-screen modal |
| Summary Cards | 5 columns | 2x3 grid |
| Config Bar | Horizontal scroll | Collapsible sections |
| Action Bar | 4 buttons | **Bottom Bar (Recommended)** |

**Mobile Action Pattern Decision:**

The wireframes show two options for mobile actions. **Recommendation: Bottom Bar**

| Option | Pros | Cons |
|--------|------|------|
| **FAB Menu** | Minimal footprint, Material Design pattern | Hidden actions, extra tap required, discovery issues |
| **Bottom Bar** | All actions visible, thumb-friendly, familiar iOS/Android pattern | Takes 64px of screen height |

**Why Bottom Bar is Recommended:**
1. **Discoverability**: Users see all 4 actions immediately (Save, Compare, Export PDF, Export Excel)
2. **Thumb Zone**: Actions are in the natural thumb reach area for one-handed use
3. **Consistency**: Matches the desktop action bar, reducing cognitive load
4. **Efficiency**: Single tap to any action vs. tap FAB then tap action (2 taps)
5. **Platform Conventions**: iOS Tab Bar and Android Bottom Navigation are established patterns

**Implementation:**
```razor
<!-- Mobile Action Bar (bottom fixed) -->
<div class="action-bar mobile" data-testid="mobile-action-bar">
    <button class="action-item" data-testid="save-scenario">
        <span class="icon-save"></span>
        <span class="label">Save</span>
    </button>
    <button class="action-item" data-testid="compare-scenarios">
        <span class="icon-compare"></span>
        <span class="label">Compare</span>
    </button>
    <button class="action-item" data-testid="export-pdf">
        <span class="icon-pdf"></span>
        <span class="label">PDF</span>
    </button>
    <button class="action-item" data-testid="export-excel">
        <span class="icon-excel"></span>
        <span class="label">Excel</span>
    </button>
</div>
```

```css
@media (max-width: 768px) {
    .action-bar.mobile {
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        height: 64px;
        display: flex;
        justify-content: space-around;
        align-items: center;
        background: var(--surface-dark);
        border-top: 1px solid var(--border-color);
        z-index: 100;
    }

    .action-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 8px 16px;
        min-width: 48px;  /* Touch target */
        min-height: 48px; /* Touch target */
    }
}
```

### E.4 Micro-interactions

| Interaction | Description | Priority |
|-------------|-------------|----------|
| Card Hover | Subtle scale + shadow | LOW |
| Panel Tabs | Underline slide animation | LOW |
| Save Success | Check mark animation | MEDIUM |
| Export Progress | Progress bar in button | MEDIUM |

---

## Appendix F: Implementation Checklist Updates

### Phase 0 Additions
- [ ] Add `DebounceService.cs`
- [ ] Add performance CSS properties (`will-change`, `contain`)
- [ ] Set up visual regression test infrastructure

### Phase 1 Additions
- [ ] Add `data-testid` attributes to all interactive components (see Section 1.4)
- [ ] Ensure all error states include icon shapes (not color-only)

### Phase 2 Additions
- [ ] Implement server-side watermarking
- [ ] Add rate limiting for guest exports

### Phase 3 Additions
- [ ] Add `UrlConfigurationValidator.cs`
- [ ] Add `SessionTimeoutMonitor.razor` with unsaved changes detection
- [ ] Implement `DashboardStateService.cs`
- [ ] Add `HasModifications` property to `AppStateService`

### Phase 5 Additions
- [ ] Add accessibility testing to CI
- [ ] Add visual regression tests
- [ ] Add performance benchmarks
- [ ] Home.razor decomposition complete
- [ ] Mobile Bottom Bar implementation (see Section E.3)
- [ ] Verify all error/warning states have icon shapes for colorblind accessibility
