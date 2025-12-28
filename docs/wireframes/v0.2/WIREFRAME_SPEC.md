# Wireframe Specification

**Source:** UX Redesign Plan v3.0
**Version:** 1.0
**Purpose:** Single source of truth for all wireframe formats

---

## Design Tokens

### Colors (Dark Theme - Primary)

| Token | Hex | Usage |
|-------|-----|-------|
| `bg-primary` | `#0d1117` | Main app background |
| `bg-secondary` | `#161b22` | Card/panel background |
| `bg-tertiary` | `#21262d` | Elevated surfaces, inputs |
| `bg-header` | `#010409` | Header background |
| `border-default` | `#30363d` | Default borders |
| `border-focus` | `#58a6ff` | Focus/active borders |
| `text-primary` | `#e6edf3` | Primary text |
| `text-secondary` | `#8b949e` | Secondary/muted text |
| `text-muted` | `#6e7681` | Disabled text |
| `accent-blue` | `#58a6ff` | Primary accent, links |
| `accent-green` | `#3fb950` | Success, completed |
| `accent-yellow` | `#d29922` | Warning |
| `accent-red` | `#f85149` | Error, danger |
| `accent-purple` | `#a371f7` | Staging environment |

### Typography

| Token | Size | Usage |
|-------|------|-------|
| `font-xs` | 12px | Labels, badges |
| `font-sm` | 14px | Body text, descriptions |
| `font-md` | 16px | Base size |
| `font-lg` | 18px | Section headers |
| `font-xl` | 20px | Page subtitles |
| `font-2xl` | 24px | Page titles |
| `font-3xl` | 30px | Large stats |

### Spacing

| Token | Value |
|-------|-------|
| `space-1` | 4px |
| `space-2` | 8px |
| `space-3` | 12px |
| `space-4` | 16px |
| `space-5` | 24px |
| `space-6` | 32px |

### Border Radius

| Token | Value |
|-------|-------|
| `radius-sm` | 4px |
| `radius-md` | 8px |
| `radius-lg` | 12px |
| `radius-full` | 9999px |

---

## Layout Specifications

### Desktop (1440px+)

```
Total Width: 1440px
Total Height: 900px

┌─────────────────────────────────────────────────────────────┐
│ HEADER                                              64px    │
├──────────┬──────────────────────────────────┬───────────────┤
│ SIDEBAR  │ MAIN CONTENT                     │ CONTEXT PANEL │
│  240px   │   flex (920px at 1440)           │     280px     │
│          │                                   │               │
│          │                                   │               │
│  836px   │        836px                      │    836px      │
│  height  │        height                     │    height     │
└──────────┴──────────────────────────────────┴───────────────┘
```

### Tablet (768px - 1024px)

```
Total Width: 1024px
Total Height: 800px

┌─────────────────────────────────────────────────────────────┐
│ HEADER                                              64px    │
├─────────────────────────────────────────────────────────────┤
│ BREADCRUMB PROGRESS                                 48px    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ MAIN CONTENT (full width, 2-column grid for cards)         │
│                                                             │
│                                                   ~600px    │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ COLLAPSIBLE SUMMARY (bottom sheet)                  88px    │
└─────────────────────────────────────────────────────────────┘
```

### Mobile (<768px)

```
Total Width: 375px
Total Height: 812px

┌─────────────────────────────────────────────────────────────┐
│ HEADER                                              56px    │
├─────────────────────────────────────────────────────────────┤
│ STEP INDICATOR                                      48px    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ CONTENT AREA (single column, scrollable)                    │
│                                                             │
│                                                   ~644px    │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ BOTTOM NAV                                          64px    │
└─────────────────────────────────────────────────────────────┘
```

---

## Wizard Steps (5 Total)

| Step | Name | Status in Wireframe | Description |
|------|------|---------------------|-------------|
| 1 | Platform | Completed | Native vs Low-Code |
| 2 | Technology | Active | Mendix, OutSystems, .NET, etc. |
| 3 | Distribution | Pending | K8s distribution selection |
| 4 | Applications | Pending | Environment matrix |
| 5 | Pricing | Pending | Cost configuration |

---

## Context Panel Summary (Standard Values)

These values appear in all wireframes for consistency:

| Metric | Value | Subtext |
|--------|-------|---------|
| Total Nodes | 12 | +2 HA |
| Total vCPUs | 96 | - |
| Total RAM | 384 GB | - |
| Monthly Cost | $4,200 | /month |

---

## Screen Specifications

### 01 - Desktop Layout (1440x900)

**Purpose:** Show 3-panel layout structure

**Header (64px):**
- Logo: "ISC" icon + "Infrastructure Sizing Calculator"
- Right actions: Theme toggle, Settings icon, User avatar

**Sidebar (240px):**
- Progress dots: 5 dots (1 completed, 1 active, 3 pending)
- Wizard Steps section:
  - Step 1: Platform (completed - green checkmark)
  - Step 2: Technology (active - blue highlight)
  - Step 3: Distribution (pending)
  - Step 4: Applications (pending)
  - Step 5: Pricing (pending)
- Results section (at bottom):
  - Sizing
  - Costs
  - Growth

**Main Content:**
- Title: "Select Technology"
- Subtitle: "Choose your application technology stack"
- 2 selection cards:
  - Card 1: "Native Applications" - .NET, Java, Node.js, Python, Go
  - Card 2: "Low-Code Platform" - Mendix, OutSystems (SELECTED)
- Info tip box at bottom

**Context Panel (280px):**
- Header: "Quick Summary"
- Stat cards:
  - Platform: Kubernetes
  - Total Nodes: 12
  - Monthly Cost: $4,200
- Quick Actions:
  - Save Scenario (primary button)
  - Export (secondary)
  - Share (secondary)

**Wizard Navigation (bottom of main content):**
- Back button (secondary)
- Progress dots
- Next Step button (primary)

---

### 02 - Tablet Layout (1024x800)

**Purpose:** Show responsive tablet layout with horizontal progress

**Header (64px):**
- Hamburger menu icon
- "InfraSizing Calculator" title
- Theme, Settings, User icons

**Breadcrumb Progress (48px):**
- Horizontal step indicators with labels
- Format: [1] Platform > [2] Tech > [3] Dist > [4] Apps > [5] Price

**Main Content (full width):**
- Same content as desktop but in 2-column card grid
- Wizard nav at bottom: Back | [Summary toggle] | Next

**Collapsible Summary (88px, bottom):**
- Collapsed state shows: "Summary: 12 Nodes | 96 vCPU | $4,200/mo"
- Expandable to full summary panel

---

### 03 - Mobile Layout (375x812)

**Purpose:** Show mobile-first single column layout

**Header (56px):**
- Hamburger menu
- "InfraSiz" (truncated)
- Overflow menu (3 dots)

**Step Indicator (48px):**
- 5 dots with current step number
- "Step 2 of 5"

**Content Area (single column):**
- Stacked cards
- Touch-friendly spacing (min 44px tap targets)

**Bottom Nav (64px):**
- 4 sections: Back | Summary (sheet trigger) | Next | Menu

---

### 04 - Platform Selection (Desktop content area)

**Purpose:** First wizard step - choose platform type

**Title:** "Select Platform Type"
**Subtitle:** "Choose the type of applications you'll be deploying"

**Two large selection cards (side by side):**

Card 1 - Native Applications:
- Icon: Container/microservices visual
- Title: "Native Applications"
- Description: ".NET, Java, Node.js, Python, Go"
- Subtext: "Standard container workloads"
- [SELECT] button

Card 2 - Low-Code Platform:
- Icon: Low-code visual
- Title: "Low-Code Platform"
- Description: "Mendix, OutSystems"
- Subtext: "Platform-specific runtime requirements"
- [SELECT] button (SELECTED state shown)

**Info Tip:**
"TIP: Choose based on your development team's skills and your organization's existing technology stack."

---

### 05 - Applications Configuration (Desktop content area)

**Purpose:** Environment matrix for app counts

**Title:** "Configure Applications"
**Subtitle:** "Set the number of applications per environment and size"

**Environment Toggles (top):**
[x] DEV | [x] TEST | [x] STAGING | [x] PROD

**Application Size Matrix (table):**

|           | DEV | TEST | STAGING | PROD |
|-----------|-----|------|---------|------|
| Small (1c)| [5] | [5]  | [3]     | [3]  |
| Medium (2c)| [3] | [3]  | [2]     | [2]  |
| Large (4c)| [1] | [1]  | [1]     | [1]  |
|-----------|-----|------|---------|------|
| TOTAL     | 9   | 9    | 6       | 6    | = 30

**Quick Presets:**
[Startup (10 apps)] [Medium (50)] [Enterprise (100+)]

---

### 06 - Sizing Results (Desktop content area)

**Purpose:** Infrastructure sizing overview

**Title:** "Sizing Results"
**Tabs:** [Overview] [Nodes] [Resources] [By Environment]

**Summary Cards (4 across):**
- 12 NODES (+2 HA)
- 96 vCPUs
- 384 GB RAM
- $4,200 /MONTH

**Cluster Architecture Diagram:**
```
CONTROL PLANE (3 nodes)
┌─────┐ ┌─────┐ ┌─────┐
│ M1  │ │ M2  │ │ M3  │  4 vCPU / 16GB each
└─────┘ └─────┘ └─────┘
          │
          ▼
WORKER NODES (9 nodes)
┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┐
│ W1  │ W2  │ W3  │ W4  │ W5  │ W6  │ W7  │ W8  │ W9  │
└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┘
8 vCPU / 32GB each
```

**Recommendations Box:**
- "Consider adding 1 more worker node for 20% headroom"
- "Enable pod disruption budgets for high availability"

---

### 07 - Cost Analysis (Desktop content area)

**Purpose:** Cost breakdown and cloud comparison

**Title:** "Cost Analysis"
**Tabs:** [On-Prem] [AWS] [Azure] [GCP] | [Compare All]

**Monthly Cost Breakdown (bar chart):**
- Compute: $3,200 (76%)
- Storage: $800 (19%)
- Licensing: $200 (5%)
- TOTAL: $4,200/month ($50,400/year)

**Cloud Provider Comparison Table:**

| Provider | Monthly | vs On-Prem | 3-Year TCO |
|----------|---------|------------|------------|
| On-Premises | $4,200 | -- | $151,200 |
| AWS EKS | $5,100 | +21% | $183,600 |
| Azure AKS | $4,800 | +14% | $172,800 |
| Google GKE | $4,600 | +10% | $165,600 * Lowest |

---

### 08 - Growth Planning (Desktop content area)

**Purpose:** Growth projections over time

**Title:** "Growth Planning"
**Subtitle:** "Plan your infrastructure scaling over time"

**Growth Configuration:**
- Annual Growth Rate: [slider] 25%
- Planning Horizon: [1yr] [2yr] [3yr*] [5yr]

**Projection Chart:**
- X-axis: Now, Q1, Q2, Q3, Q4, Y2, Y3
- Y-axis: Nodes (12 to 30)
- Line chart showing growth curve

**Scaling Milestones (3 cards):**

| TODAY | YEAR 1 | YEAR 3 |
|-------|--------|--------|
| 12 nodes | 15 nodes | 24 nodes |
| 30 apps | 38 apps | 59 apps |
| $4,200/mo | $5,300/mo | $8,400/mo |

**Insight Box:**
"Plan to add 4 worker nodes by end of Year 1 to maintain 20% headroom with projected growth."

---

### 09 - Scenarios Management (Full page)

**Purpose:** Save, load, compare scenarios

**Header:**
- Title: "Saved Scenarios"
- Search box
- [+ New Scenario] button

**Scenario List (cards):**

Card 1 (starred):
- Title: "Production K8s Cluster"
- Details: Mendix - OpenShift - 12 nodes - $4,200/mo
- Dates: Created Dec 20, Modified Dec 27
- Actions: [Load] [Duplicate] [Edit] [Delete] [Compare checkbox]

Card 2:
- Title: "Development Environment"
- Details: .NET - K3s - 4 nodes - $800/mo
- Actions: same

Card 3:
- Title: "Cloud Migration Plan"
- Details: OutSystems - EKS - 8 nodes - $3,500/mo
- Actions: same

**Comparison View (when 2+ selected):**

| Metric | Production K8s | Cloud Migration |
|--------|----------------|-----------------|
| Nodes | 12 | 8 |
| vCPUs | 96 | 64 |
| RAM | 384 GB | 256 GB |
| Cost/mo | $4,200 | $3,500 |
| 3yr TCO | $151,200 | $126,000 (17% savings) |

---

### 10 - Settings Page (Full page)

**Purpose:** Application configuration

**Title:** "Settings"
**Subtitle:** "Configure application preferences"

**Section 1: Appearance**
- Theme: ( ) Light (x) Dark ( ) System
- Preview box showing current theme

**Section 2: Pricing Configuration**
- On-Premises Pricing table:
  | Component | Unit | Monthly Cost |
  |-----------|------|--------------|
  | Compute (vCPU) | per core | $35.00 |
  | Memory (RAM) | per GB | $5.00 |
  | Storage (Disk) | per GB | $0.10 |
  | Licensing | flat | $200.00 |
- [Reset to Defaults] button

**Section 3: Data Management**
- [Import Configuration]
- [Export All Scenarios]
- [Clear All Data] (danger button)

**Footer:**
- [Save Changes] primary button

---

## Consistency Checklist

Before finalizing any wireframe, verify:

- [ ] Header height matches spec (64px desktop, 56px mobile)
- [ ] Sidebar width is 240px (desktop only)
- [ ] Context panel width is 280px (desktop only)
- [ ] All 5 wizard steps shown with correct names
- [ ] Step 1 completed, Step 2 active, Steps 3-5 pending
- [ ] Summary shows: 12 nodes, 96 vCPU, 384 GB, $4,200
- [ ] Colors match design tokens
- [ ] Font sizes follow typography scale
- [ ] Border radius follows spec (sm=4, md=8, lg=12)
