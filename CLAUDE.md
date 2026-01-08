# InfraSizingCalculator - Claude Context

## Project Overview
Infrastructure Sizing Calculator - Blazor Server application for calculating VM and Kubernetes cluster sizing with cost estimation.

## CRITICAL: Current Design Version
**The active design is V4** located in `src/InfraSizingCalculator/Components/V4/`

- Main dashboard: `Components/V4/Dashboard/Dashboard.razor` (routes: `/`, `/dashboard`)
- Old design at `/legacy` - DO NOT USE for new work
- All new UI work should use V4 components

## WIREFRAMES - Reference Design (v0.4.4)

**ACTIVE REDESIGN IN PROGRESS - v0.4.4**

The v0.4.4 redesign separates K8s and VM modes with completely different configurations.

### Master Plan (READ FIRST after compaction)
```
docs/wireframes/v0.4.4/plan/WIREFRAME_REDESIGN_PLAN.md
```
This plan links to vendor research and defines ALL pages, tabs, and panels.

### Vendor Research (Source of Truth for Configuration)
```
docs/vendor-specs/
├── ux-ui/enterprise-dashboard-best-practices.md   - UX/UI patterns (NN/G, Material Design)
├── k8s/K8S_CLOUD_SIZING_RESEARCH.md              - AWS EKS, Azure AKS, GKE sizing
├── k8s/ON_PREMISES_K8S_DISTRIBUTIONS.md          - OpenShift, RKE2, K3s, Tanzu, MKE
├── VM_HYPERVISOR_BEST_PRACTICES.md               - vSphere, Hyper-V, Proxmox
└── lowcode/LOW_CODE_SIZING_RESEARCH.md           - Mendix, OutSystems sizing
```

### Previous Version (v0.4.2)
```
docs/wireframes/v0.4.2/html/index.html
```

**To view wireframes:** Open `docs/wireframes/v0.4.4/html/index.html` in browser (when created)

## V4 Component Structure
```
Components/V4/
├── Dashboard/     - Main dashboard, ActionBar, ConfigBar, SummaryCardsRow
├── Guest/         - Guest landing pages
├── Headers/       - Header components (AuthHeader, GuestHeader)
└── Panels/        - Slide-out config panels (Apps, Growth, NodeSpecs, Platform, Pricing)
```

## Authentication Architecture
- ASP.NET Core Identity with SQLite
- Cookie-based authentication (HTTP POST required, not Blazor events)
- Login endpoint: `POST /api/auth/login`
- Default admin: `admin@localhost` / `Admin123!`
- OAuth providers: Google, Microsoft (configured via Settings)
- LDAP/AD authentication available

## Key Technical Decisions
1. **Forms that set cookies** must use HTTP POST (not Blazor SignalR events)
2. **InteractiveServer render mode** for interactive pages
3. **V4 design** is the production UI - do not revert to legacy components

## Wireframe Structure (v0.4.2)

The V4 Dashboard implements THREE states matching the wireframes:

### State 1: Guest Landing (00-landing-guest.html) - `!_isAuthenticated && !_hasActiveScenario`
- Header: logo + Sign In / Sign Up buttons
- Single "Create New Scenario" action card (centered)
- Login prompt box below
- Features list (46 K8s, 7 platforms, Cost estimation)
- Footer with links

### State 2: Auth Landing (00-landing.html) - `_isAuthenticated && !_hasActiveScenario`
- Header: logo + User menu (avatar + initials + name) + Settings button
- TWO action cards side-by-side: "Load Scenario" + "Create New Scenario"
- Recent Scenarios section (3 items with K8s/VM badges)
- "View All" link to /scenarios

### State 3: Active Dashboard (01-dashboard.html) - `_hasActiveScenario`
- Full dashboard with summary cards, config bar, and results
- Slide-out configuration panels

### All Wireframe Pages (docs/wireframes/v0.4.2/html/):
```
00-landing-guest.html  - Guest landing (single action card)
00-landing.html        - Auth landing (two action cards + recent scenarios)
01-dashboard.html      - Main dashboard
02-dashboard-with-panel.html - Dashboard with slide panel open
04-apps-panel.html     - Apps configuration panel
05-node-specs-panel.html - Node specs panel
06-pricing-panel.html  - Pricing panel
07-growth-panel.html   - Growth planning panel
08-scenarios.html      - Scenarios list page
09-export-modal.html   - Export modal
10-settings.html       - Settings page
11-login.html          - Login page
12-register.html       - Registration page
```

### COMPLETED: V4 now matches wireframes for:
- Guest Landing (single action card + login prompt + features list)
- Auth Landing (two action cards + recent scenarios section)
- Login page (centered form with social login)
- Register page (benefits list + form with password strength)

## After Session Compaction (CRITICAL)

When resuming after context compaction or starting a new session, **READ THESE FILES IN ORDER:**

### 1. MANDATORY: Load Wireframe Redesign Plan
```
docs/wireframes/v0.4.4/plan/WIREFRAME_REDESIGN_PLAN.md
```
This plan contains:
- All page/panel definitions with fields
- K8s vs VM mode differences
- Links to vendor research ([K8S-CLOUD], [VM-REF], etc.)
- Formulas and validation rules

### 2. Load Relevant Vendor Research (as needed)
Based on current task, read the specific research file:
- **For K8s work:** `docs/vendor-specs/k8s/K8S_CLOUD_SIZING_RESEARCH.md` and `ON_PREMISES_K8S_DISTRIBUTIONS.md`
- **For VM work:** `docs/vendor-specs/VM_HYPERVISOR_BEST_PRACTICES.md`
- **For UI/UX work:** `docs/vendor-specs/ux-ui/enterprise-dashboard-best-practices.md`
- **For low-code sizing:** `docs/vendor-specs/lowcode/LOW_CODE_SIZING_RESEARCH.md`

### 3. Check Current State
- This CLAUDE.md file - project context
- `git status` - uncommitted changes
- `Components/V4/Dashboard/Dashboard.razor` - main UI entry point

### 4. DO NOT proceed without reading the plan
The plan contains critical information that cannot be reconstructed from memory.

## Database Files
- `infrasizing.db` - Application data (scenarios, settings, pricing)
- `identity.db` - User authentication data
