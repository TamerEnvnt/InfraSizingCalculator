# Wireframe Specification v0.4 - Dashboard-First Pattern

**Version:** 0.4
**Pattern:** Dashboard-First / Results-Oriented Design
**Date:** December 28, 2024

---

## Design Philosophy

This version uses a **Dashboard-First** pattern where users see results immediately and can adjust configuration through side panels. Based on the principle that users want to see outcomes first, then refine their inputs.

### Key Differences from v0.2 and v0.3

| Aspect | v0.2 Wizard | v0.3 Accordion | v0.4 Dashboard |
|--------|-------------|----------------|----------------|
| Focus | Configuration | Configuration | Results |
| First View | Setup form | All config steps | Live dashboard |
| Config Access | Sequential steps | Expandable sections | Slide-out panels |
| Layout | 3-panel wizard | Full-width accordion | Dashboard + panels |
| Summary | Side context | Bottom floating bar | Main content area |

### Benefits of Dashboard-First

1. **Immediate value** - Users see results instantly with defaults
2. **Exploratory** - Easy to experiment with different configurations
3. **Data-focused** - Charts and metrics are the primary content
4. **Action-oriented** - Export and share actions prominently visible

---

## Design Tokens

Same as previous versions for consistency:

### Colors
| Token | Value | Usage |
|-------|-------|-------|
| bg-primary | `#0d1117` | Main background |
| bg-secondary | `#161b22` | Cards, sections |
| bg-tertiary | `#21262d` | Hover, inputs |
| border-primary | `#30363d` | Borders |
| text-primary | `#c9d1d9` | Main text |
| text-secondary | `#8b949e` | Labels |
| accent-blue | `#58a6ff` | Active, links |
| accent-green | `#3fb950` | Complete, success |

### Typography
- Font: `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`
- Same scale as previous versions

---

## Layout Structure

### Desktop (1440x900)

```
┌────────────────────────────────────────────────────────────────┐
│                        HEADER (64px)                           │
│  Logo                              [Config] [Scenarios] [User] │
├────────────────────────────────────────────────────────────────┤
│  DASHBOARD CONTENT                                             │
│  ┌────────────────────────────────────────────────────────────┐│
│  │ SUMMARY CARDS ROW                                          ││
│  │ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐           ││
│  │ │ 12      │ │ 96      │ │ 384     │ │ $4,200  │           ││
│  │ │ Nodes   │ │ vCPUs   │ │ GB RAM  │ │ /month  │           ││
│  │ └─────────┘ └─────────┘ └─────────┘ └─────────┘           ││
│  └────────────────────────────────────────────────────────────┘│
│                                                                │
│  ┌─────────────────────────────┐ ┌────────────────────────────┐│
│  │ NODE ARCHITECTURE           │ │ COST BREAKDOWN             ││
│  │                             │ │                            ││
│  │ [Master] [Infra] [Worker]   │ │    [Pie Chart]             ││
│  │                             │ │                            ││
│  │ Detailed node specs...      │ │ Compute   $2,800           ││
│  │                             │ │ Storage   $480             ││
│  └─────────────────────────────┘ └────────────────────────────┘│
│                                                                │
│  ┌─────────────────────────────────────────────────────────────┐
│  │ CONFIGURATION BAR (Current Settings + Edit Buttons)        ││
│  │ Platform: K8s | Dist: AKS | Tech: Mendix | Apps: 24        ││
│  └─────────────────────────────────────────────────────────────┘
│                                                                │
├────────────────────────────────────────────────────────────────┤
│  ACTION BAR                                                    │
│  [Save Scenario]  [Compare]  [Export PDF]  [Export Excel]      │
└────────────────────────────────────────────────────────────────┘
```

### Fixed Height SPA Layout (Critical Pattern)

The application uses a **fixed viewport layout** where:
- Header (64px) and Footer/Action Bar (64px) are **always visible**
- Only content areas scroll internally
- Page never scrolls as a whole

```
┌────────────────────────────────────────────────────────────────┐
│  HEADER (64px) - FIXED                                         │
│  Logo                              [Config] [Scenarios] [User] │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  CONTENT AREA (calc: 100vh - 128px)                           │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  This area scrolls internally if content overflows       │ │
│  │  Can use vertical scroll, horizontal slides, or          │ │
│  │  accordion patterns for long content                     │ │
│  │                                                          │ │
│  │  [Scrollable content...]                                 │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
├────────────────────────────────────────────────────────────────┤
│  ACTION BAR (64px) - FIXED                                     │
│  [Save Scenario]  [Compare]  [Export PDF]  [Export Excel]      │
└────────────────────────────────────────────────────────────────┘
```

### Configuration Slide-Out Panel with Tags Filter

```
┌────────────────────────────────────────────────────────────────┐
│  MAIN DASHBOARD                          │ SLIDE-OUT PANEL     │
│                                          │ (420px width)       │
│  [Results content with                   │ ┌─────────────────┐ │
│   slight overlay/dim]                    │ │ [X] Close       │ │
│                                          │ │                 │ │
│                                          │ │ Platform Config │ │
│                                          │ │ ─────────────── │ │
│                                          │ │ ( ) VM          │ │
│                                          │ │ (●) Kubernetes  │ │
│                                          │ │                 │ │
│                                          │ │ Distribution    │ │
│                                          │ │ ─────────────── │ │
│                                          │ │ [🔍 Search...]  │ │
│                                          │ │ ┌─────────────┐ │ │
│                                          │ │ │TAGS FILTER  │ │ │
│                                          │ │ │[Cloud ×]    │ │ │
│                                          │ │ │[Enterprise] │ │ │
│                                          │ │ │[Managed ×]  │ │ │
│                                          │ │ └─────────────┘ │ │
│                                          │ │ ┌─────────────┐ │ │
│                                          │ │ │ SCROLLABLE  │ │ │
│                                          │ │ │ CARD GRID   │ │ │
│                                          │ │ │ (filtered)  │ │ │
│                                          │ │ └─────────────┘ │ │
│                                          │ │ [Apply Changes] │ │
│                                          │ └─────────────────┘ │
└────────────────────────────────────────────────────────────────┘
```

### Tags Filter Pattern

For large selection lists (46 distributions, scenarios, etc.):

```
┌─────────────────────────────────────────────────────────────┐
│ [🔍 Search or type to add tag...]                           │
├─────────────────────────────────────────────────────────────┤
│ ACTIVE FILTERS:                                             │
│ [Cloud ×] [Managed ×] [Enterprise ×]                        │
├─────────────────────────────────────────────────────────────┤
│ SUGGESTED TAGS:                                             │
│ [On-Prem] [Lightweight] [Production] [Development]          │
│ [AWS] [Azure] [GCP] [Self-Hosted]                          │
├─────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────┐ │
│ │           FILTERED RESULTS (scrollable)                 │ │
│ │  ┌─────────┐ ┌─────────┐ ┌─────────┐                   │ │
│ │  │  AKS    │ │  EKS    │ │  GKE    │                   │ │
│ │  │ Azure   │ │  AWS    │ │ Google  │                   │ │
│ │  └─────────┘ └─────────┘ └─────────┘                   │ │
│ │  ... more results scroll here ...                       │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**Tag Categories for Distributions:**
| Category | Tags |
|----------|------|
| Provider | AWS, Azure, GCP, On-Prem |
| Type | Managed, Self-Hosted, Lightweight |
| Scale | Enterprise, Development, Edge |
| Features | HA, Air-Gapped, GPU, ARM |

---

## Screen Specifications

### Header (64px)

**Guest Header:**
- Logo left: "InfraSizing Calculator"
- Right: [New Scenario] [Sign In] [Sign Up]
- No main navigation (guest flow is linear)

**Logged-in Header:**
- Logo left: "InfraSizing Calculator"
- Navigation: [New Scenario] [Scenarios] [@]
- New Scenario opens configuration panel
- Scenarios shows saved scenarios list
- [@] opens settings page

### Summary Cards Row
- 4-5 key metrics in card format
- Large numbers, small labels
- Highlight card for cost (green accent)

### Main Content Grid
- 2-column layout for charts and details
- Node architecture visualization
- Cost breakdown with pie chart
- Growth projections (optional)

### Configuration Bar
- Shows current config summary inline
- Edit buttons open respective config panels
- Always visible at bottom of dashboard

### Action Bar
- Fixed at bottom
- Primary actions: Save, Compare, Export
- Multiple export formats

### Slide-Out Configuration Panels
- 480px wide slide-out from right
- Sections for each config type:
  - Platform & Distribution
  - Technology & Apps
  - Pricing Options
- Apply button updates dashboard live

---

## Complete Screen Set (12 Screens)

### Section 1: Core Dashboard (3 screens)

#### Screen 01: Main Dashboard
- **File:** `01-dashboard.html` / `01-dashboard.excalidraw`
- **Purpose:** Full dashboard view with default calculations
- **Components:**
  - Summary cards row (Nodes, vCPU, RAM, Cost)
  - Node architecture grid (Master, Infra, Worker visualization)
  - Cost breakdown with pie chart
  - Configuration bar showing current settings
  - Action bar (Save, Compare, Export)
- **State:** Loaded with sensible defaults

#### Screen 02: Dashboard with Config Panel
- **File:** `02-dashboard-with-panel.html` / `02-dashboard-with-panel.excalidraw`
- **Purpose:** Dashboard with slide-out configuration panel open
- **Components:**
  - Dashboard content (slightly dimmed)
  - 420px slide-out panel from right
  - Platform selection (VM/Kubernetes radio)
  - Distribution card grid with categories
  - Close button and Apply Changes button
- **Interaction:** Panel slides in, dashboard remains visible

#### Screen 03: Comparison Dashboard
- **File:** `03-comparison-dashboard.html` / `03-comparison-dashboard.excalidraw`
- **Purpose:** Side-by-side scenario comparison
- **Components:**
  - Two-column layout with scenario headers
  - Summary cards for each scenario
  - Difference analysis section
  - Visual indicators (green/red for better/worse)
- **State:** Two scenarios loaded for comparison

---

### Section 2: Configuration Panels (4 screens)

#### Screen 04: Apps Configuration Panel
- **File:** `04-apps-panel.html` / `04-apps-panel.excalidraw`
- **Purpose:** Configure applications per environment
- **Components:**
  - Environment tabs (Production, Staging, Test, Development)
  - Slider for total app count
  - App distribution grid with cards per environment
  - Each card shows: App count, High Availability toggle, Resources badge
  - Summary bar showing total apps and HA-enabled count
- **Interaction:** Sliders update totals in real-time

#### Screen 05: Node Specifications Panel
- **File:** `05-node-specs-panel.html` / `05-node-specs-panel.excalidraw`
- **Purpose:** Configure node specs (vCPU, RAM, Storage)
- **Components:**
  - Node type sections (Master, Infrastructure, Worker)
  - Preset buttons (Small, Medium, Large, Custom)
  - Specifications grid for each type:
    - Node count with +/- controls
    - vCPU dropdown
    - RAM dropdown
    - Storage dropdown
  - Color-coded badges (blue=Master, purple=Infra, green=Worker)
  - Summary totals (Total Nodes, Total vCPU, Total RAM)
- **Interaction:** Presets auto-fill specs, custom allows manual entry

#### Screen 06: Pricing Configuration Panel
- **File:** `06-pricing-panel.html` / `06-pricing-panel.excalidraw`
- **Purpose:** Configure pricing source and options
- **Components:**
  - Pricing source toggle (Cloud Pricing / Custom Rates)
  - Cloud provider card grid (Azure, AWS, GCP) with selection state
  - Region dropdown
  - Currency dropdown
  - Toggle options (Include support, Reserved instances, Include egress)
  - Cost preview breakdown showing monthly totals
- **Interaction:** Provider selection updates region options

#### Screen 07: Growth Planning Panel
- **File:** `07-growth-panel.html` / `07-growth-panel.excalidraw`
- **Purpose:** Configure growth projections
- **Components:**
  - Annual growth rate slider (0-100%)
  - Planning horizon buttons (1, 2, 3, 5 years)
  - Bar chart visualization showing resource growth
  - Resource projections table:
    - Year 1/2/3/5 columns
    - Apps, Nodes, vCPU, RAM, Cost rows
    - Percentage change indicators
  - Capacity alert message
- **Interaction:** Slider updates chart and table projections

---

### Section 3: Scenarios & Export (2 screens)

#### Screen 08: Scenarios Management
- **File:** `08-scenarios.html` / `08-scenarios.excalidraw`
- **Purpose:** List, search, and manage saved scenarios
- **Components:**
  - Page header with title and "New Scenario" button
  - Search bar with filter controls
  - Scenario card grid (3 columns):
    - Scenario name
    - Platform/Distribution badge
    - Quick stats (Nodes, vCPU, Cost)
    - Last modified date
    - Action buttons (Load, Compare, Delete)
  - Empty state design (not shown but implied)
- **Interaction:** Cards clickable to load, multi-select for compare

#### Screen 09: Export Modal
- **File:** `09-export-modal.html` / `09-export-modal.excalidraw`
- **Purpose:** Export configuration with format selection
- **Components:**
  - Modal overlay (dims background)
  - Modal card (560px width) with:
    - Header with close button
    - Format selection cards (PDF, Excel, JSON)
    - Content checkboxes:
      - Summary overview
      - Node configuration details
      - Cost breakdown
      - Growth projections
      - Configuration history
    - Filename input with format badge
    - Export preview (pages/sheets, file size)
    - Cancel and Export buttons
- **Interaction:** Format selection updates extension and preview

---

### Section 4: Settings & Authentication (3 screens)

#### Screen 10: Settings
- **File:** `10-settings.html` / `10-settings.excalidraw`
- **Purpose:** User account and application settings
- **Components:**
  - Two-column layout:
    - Left sidebar (280px) with navigation:
      - Account, Preferences, Pricing Defaults, Notifications, API Keys
    - Right content area with sections:
      - Profile section (avatar, display name, email)
      - Preferences section (theme toggle, default currency, auto-save)
      - Danger Zone (delete scenarios, delete account)
  - Section cards with proper spacing
  - Toggle switches and dropdowns
- **Interaction:** Sidebar navigation switches content sections

#### Screen 11: Login
- **File:** `11-login.html` / `11-login.excalidraw`
- **Purpose:** User authentication - sign in
- **Components:**
  - Centered card (400px width) with:
    - Logo header
    - "Welcome back" title and subtitle
    - Email input field
    - Password input field
    - Remember me checkbox
    - Forgot password link
    - Sign In button (primary, blue)
    - Divider with "or continue with"
    - Social login buttons (Microsoft, Google)
    - Sign up link
    - Footer links (Privacy, Terms, Support)
- **State:** Clean login form, no errors

#### Screen 12: Register
- **File:** `12-register.html` / `12-register.excalidraw`
- **Purpose:** User account creation
- **Components:**
  - Centered card (440px width) with:
    - Logo header
    - "Create an account" title
    - Benefits box (save scenarios, export reports, compare)
    - Two-column name inputs (First Name, Last Name)
    - Email input
    - Organization input (optional)
    - Password input with strength indicator bar
    - Confirm password input
    - Terms agreement checkbox
    - Create Account button (primary, blue)
    - Sign in link
    - Footer links
- **Interaction:** Password strength updates as user types

---

## Responsive Behavior

### Tablet (1024px)
- Same layout, narrower cards
- Slide-out panel takes 50% width
- Charts stack on smaller tablets

### Mobile (375px)
- Cards stack vertically (2x2 grid)
- Config panel becomes full-screen modal
- Action bar simplified to icons

---

## Interactions

1. **Config Button Click:** Opens slide-out panel
2. **Panel Section Click:** Expands config section
3. **Apply Changes:** Updates dashboard instantly
4. **Card Click:** Opens detailed view for that metric
5. **Export Click:** Downloads in selected format
6. **Compare Click:** Adds scenario selector

---

## Component Differences

| Component | v0.2 | v0.3 | v0.4 |
|-----------|------|------|------|
| Primary View | Config form | Config form | Results dashboard |
| Navigation | Sidebar wizard | Progress bar | Header icons |
| Config Access | Sequential | Accordion | Slide-out panels |
| Summary | Side panel | Floating bar | Main content cards |
| Actions | Step buttons | Calculate button | Action bar |

---

## Key Design Decisions

1. **Results-First**: Users see value immediately
2. **Non-blocking Config**: Configuration doesn't hide results
3. **Live Updates**: Changes reflect instantly
4. **Export-Ready**: Dashboard is the final product view

---

## UX/UI Handoff Notes

### File Formats
- **HTML wireframes** (`html/` folder): Interactive, viewable in browser
- **Excalidraw wireframes** (`excalidraw/` folder): Editable source files for [excalidraw.com](https://excalidraw.com)
- **Index files**: Quick navigation to all screens

### Design System Reference
All wireframes use consistent tokens defined in the Design Tokens section above. When implementing:
- Use CSS custom properties for colors
- Maintain 8px spacing grid
- Follow typography scale consistently

### Interaction Patterns
| Pattern | Implementation |
|---------|---------------|
| Slide-out panels | 420-480px width, animate from right, overlay with dim |
| Modal dialogs | Centered, 560px max-width, backdrop blur |
| Form controls | 40-48px height, rounded corners (6-8px radius) |
| Buttons (primary) | Blue (#58a6ff), black text, 48px height |
| Buttons (secondary) | Tertiary bg (#21262d), border, white text |
| Cards | Secondary bg (#161b22), 1px border, 12px radius |

### State Considerations
Each screen represents a specific state. Consider implementing:
- **Loading states**: Skeleton screens for dashboard cards
- **Empty states**: For scenarios list, first-time users
- **Error states**: Form validation, API failures
- **Success states**: Save confirmation, export complete

### Accessibility Requirements
- Minimum contrast ratio: 4.5:1 for text
- Focus indicators on all interactive elements
- Keyboard navigation support
- Screen reader labels for icon buttons
- Reduced motion preference support

### Mobile Adaptations (not wireframed)
These wireframes are desktop-focused (1440px). Mobile adaptations needed:
- Bottom navigation bar
- Full-screen panels instead of slide-outs
- Stacked card layouts
- Touch-friendly control sizes (44px minimum)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.4.0 | Dec 28, 2024 | Initial 3 screens (Dashboard pattern) |
| 0.4.1 | Dec 28, 2024 | Complete 12-screen set for handoff |
