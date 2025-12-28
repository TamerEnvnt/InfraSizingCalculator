# InfraSizing Calculator v0.4 Wireframe Specification

This document provides a complete specification of the v0.4 wireframe design for implementing the UI in Blazor.

---

## Architecture Overview

### Two-State Design Pattern

The application supports two distinct user states with different UI layouts:

| State | Header | Features | Restrictions |
|-------|--------|----------|--------------|
| **Guest** | Logo + New Scenario + Sign In/Sign Up | Full calculations, limited export | No saving, no scenarios list |
| **Logged-in** | Logo + Full navigation | All features | None |

### Navigation Flow

```
Landing Page (Entry Point)
    ├── Guest Flow
    │   └── Create Scenario → Configure → Results (with save prompt)
    │       └── Limited Export (summary only)
    │
    └── Logged-in Flow
        ├── Create Scenario → Configure → Results → Save/Export
        ├── Scenarios List → Load/Compare
        └── Settings
```

---

## Page Inventory

### Guest Pages (7 files)

| File | Purpose | Key Elements |
|------|---------|--------------|
| `00-landing-guest.html` | Entry point for guests | Single "Create New Scenario" card, sign-in prompts |
| `01-results-guest.html` | Full results display | Save banner with sign-in CTA |
| `02-configure-guest.html` | Platform configuration panel | Platform/mode selection |
| `04-apps-panel-guest.html` | App count configuration | Environment sliders |
| `05-node-specs-guest.html` | Node specifications | Master/Infra/Worker config |
| `06-pricing-panel-guest.html` | Pricing settings | Cloud provider, region, currency |
| `07-growth-panel-guest.html` | Growth planning | Timeline, projections |
| `09-export-guest.html` | Limited export modal | PDF summary only, PRO badges on locked features |

### Logged-in Pages (12 files)

| File | Purpose |
|------|---------|
| `00-landing.html` | Entry with Create + Load options |
| `01-dashboard.html` | Scenario details view (no panel) |
| `02-dashboard-with-panel.html` | Platform configuration |
| `03-vm-mode.html` | VM-specific configuration |
| `04-apps-panel.html` | App configuration |
| `05-node-specs-panel.html` | Node specifications |
| `06-pricing-panel.html` | Pricing settings |
| `07-growth-panel.html` | Growth planning |
| `08-scenarios.html` | Saved scenarios list |
| `09-export-modal.html` | Full export options |
| `10-settings.html` | User settings |
| `11-login.html` | Login page |
| `12-register.html` | Registration page |

---

## Component Patterns

### 1. Guest Header

**Usage**: All guest pages

```html
<header>
    <a href="00-landing-guest.html" class="logo">InfraSizing Calculator</a>
    <div class="header-actions">
        <a href="00-landing-guest.html" class="btn-new">New Scenario</a>
        <div class="auth-links">
            <a href="11-login.html" class="btn-signin">Sign In</a>
            <a href="12-register.html" class="btn-signup">Sign Up</a>
        </div>
    </div>
</header>
```

**Blazor Implementation Notes**:
- Create `GuestHeader.razor` component
- No navigation buttons, only authentication CTAs
- "New Scenario" resets and returns to landing

### 2. Logged-in Header

**Usage**: All logged-in pages

```html
<header>
    <a href="00-landing.html" class="logo">InfraSizing Calculator</a>
    <nav class="header-nav">
        <a href="01-dashboard.html" class="nav-btn">Scenario Details</a>
        <a href="02-dashboard-with-panel.html" class="nav-btn active">Configure</a>
        <a href="08-scenarios.html" class="nav-btn">Scenarios</a>
        <a href="10-settings.html" class="nav-btn">[@]</a>
    </nav>
</header>
```

**Blazor Implementation Notes**:
- Create `AuthenticatedHeader.razor` component
- Active state based on current route
- Settings icon (represented as `[@]` in wireframes)

### 3. Panel Tabs

**Usage**: Configuration panels (both guest and logged-in)

```html
<div class="panel-tabs">
    <a href="[platform-page]" class="panel-tab">Platform</a>
    <a href="[apps-page]" class="panel-tab">Apps</a>
    <a href="[nodes-page]" class="panel-tab">Nodes</a>
    <a href="[pricing-page]" class="panel-tab active">Pricing</a>
    <a href="[growth-page]" class="panel-tab">Growth</a>
</div>
```

**Guest vs Logged-in**: Links differ by suffix (`-guest.html` vs regular)

**Blazor Implementation Notes**:
- Create `PanelTabs.razor` component
- Accept parameter for guest/authenticated mode
- Highlight active tab based on current panel

### 4. Save Banner (Guest Only)

**Usage**: Guest results page

```html
<div class="save-banner">
    <div class="save-banner-content">
        <div class="save-banner-icon">[bookmark icon]</div>
        <div class="save-banner-text">
            <h3>Save your scenario</h3>
            <p>Sign in to save, compare scenarios, and access your calculations later.</p>
        </div>
    </div>
    <div class="save-banner-actions">
        <a href="11-login.html" class="btn-save-signin">Sign In to Save</a>
        <a href="12-register.html" class="btn-save-signup">Create Account</a>
    </div>
</div>
```

**Styling**: Purple gradient background (`linear-gradient(135deg, #a371f7 0%, #58a6ff 100%)`)

**Blazor Implementation Notes**:
- Create `SavePromptBanner.razor` component
- Show only for guest users
- Appears at top of results view

### 5. Limited Export Modal (Guest)

**Key Features**:
- Only "PDF / Summary" format enabled
- Excel/JSON marked with "PRO" badge and disabled
- Content options show "Sign In" badge for locked features
- Watermark warning at bottom
- Upgrade prompt with sign-in CTA

```html
<!-- Disabled format option -->
<div class="format-option disabled">
    <div class="format-icon">[XLS]</div>
    <div class="format-name">Excel</div>
    <span class="pro-badge">PRO</span>
</div>

<!-- Disabled content option -->
<div class="option-row disabled">
    <div class="checkbox disabled"></div>
    <div class="option-info">
        <div class="option-label">Node Details</div>
        <div class="option-desc">Specifications for each node type</div>
    </div>
    <span class="option-badge">Sign In</span>
</div>
```

---

## Layout Structure

### Main Layout (All Pages)

```
┌─────────────────────────────────────────────────────────────┐
│ Header (64px fixed)                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Main Content Area                    │  Slide Panel       │
│  (flex: 1, scrollable)                │  (420-480px)       │
│                                       │                     │
│  - Summary Cards                      │  - Panel Header     │
│  - Charts Grid                        │  - Panel Tabs       │
│                                       │  - Panel Content    │
│                                       │  - Panel Footer     │
│                                       │                     │
├─────────────────────────────────────────────────────────────┤
│ Action Bar (64px fixed) - Logged-in only                    │
└─────────────────────────────────────────────────────────────┘
```

**Note**: Guests do NOT have the action bar with Save/Compare/Export buttons.

### Summary Cards

5-column grid showing key metrics:

| Card | Color | Example Value |
|------|-------|---------------|
| Nodes | Blue | 12 |
| vCPUs | Default | 96 |
| GB RAM | Default | 384 |
| TB Storage | Default | 2.4 |
| Monthly Cost | Green | $4,200 |

---

## Design Tokens (CSS Variables)

```css
:root {
    /* Backgrounds */
    --bg-primary: #0d1117;      /* Main background */
    --bg-secondary: #161b22;    /* Cards, panels */
    --bg-tertiary: #21262d;     /* Inputs, buttons */

    /* Borders */
    --border-primary: #30363d;

    /* Text */
    --text-primary: #c9d1d9;    /* Main text */
    --text-secondary: #8b949e;  /* Labels, descriptions */
    --text-tertiary: #6e7681;   /* Hints, placeholders */

    /* Accents */
    --accent-blue: #58a6ff;     /* Primary actions, links */
    --accent-green: #3fb950;    /* Success, positive values */
    --accent-orange: #f0883e;   /* Warnings, alerts */
    --accent-purple: #a371f7;   /* Special CTAs, save prompts */
}
```

---

## Interactive Behaviors

### 1. Panel Navigation

- Panel slides in from right
- Background content dims (opacity: 0.5)
- Close button returns to results view
- Tab switching is instant (no animation needed)

### 2. Save Prompts (Guest)

**Trigger Points**:
1. Banner always visible on results page
2. Modal appears when clicking any "Save" action

### 3. Export Modal (Guest Restrictions)

- PDF Summary: Enabled
- Excel: Disabled with "PRO" badge
- JSON: Disabled with "PRO" badge
- Detailed content options: Disabled with "Sign In" badge

---

## Responsive Considerations

The wireframes are designed for desktop (1440px+ width). For mobile:

| Element | Desktop | Mobile (Future) |
|---------|---------|-----------------|
| Panel | Side panel | Full-screen overlay |
| Summary Cards | 5 columns | 2 columns + scroll |
| Charts | 2 columns | Stacked |

---

## Implementation Checklist

### Phase 1: Core Components
- [ ] `GuestHeader.razor`
- [ ] `AuthenticatedHeader.razor`
- [ ] `PanelTabs.razor`
- [ ] `SummaryCards.razor`
- [ ] `SlidePanel.razor` (container)

### Phase 2: Guest Flow
- [ ] `GuestLanding.razor`
- [ ] `GuestResults.razor` with save banner
- [ ] `GuestExportModal.razor` with restrictions
- [ ] Configuration panels (guest versions)

### Phase 3: Authenticated Flow
- [ ] `AuthenticatedLanding.razor`
- [ ] `ScenarioDetails.razor`
- [ ] `ScenariosListPage.razor`
- [ ] `SettingsPage.razor`
- [ ] Full export functionality

### Phase 4: Shared Panels
- [ ] `PlatformConfigPanel.razor`
- [ ] `AppsConfigPanel.razor`
- [ ] `NodeSpecsPanel.razor`
- [ ] `PricingPanel.razor`
- [ ] `GrowthPlanningPanel.razor`

---

## File Reference

All wireframe HTML files are located at:
```
docs/wireframes/v0.4/html/
```

To view in browser:
```bash
open docs/wireframes/v0.4/html/00-landing-guest.html
```

---

## Change Log

| Version | Date | Changes |
|---------|------|---------|
| v0.4 | 2024 | Initial two-state design with guest/logged-in flows |
