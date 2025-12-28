# Screen 02: Tablet Layout

**Type:** Layout Template
**Dimensions:** 1024 x 800px
**Purpose:** Responsive tablet layout with horizontal progress

---

## Frame Structure

```
┌────────────────────────────────────────────────────────┐
│                    HEADER (64px)                       │
├────────────────────────────────────────────────────────┤
│              BREADCRUMB PROGRESS (48px)                │
│    [1]─────[2]─────[3]─────[4]─────[5]                │
├────────────────────────────────────────────────────────┤
│                                                        │
│                   MAIN CONTENT                         │
│                    (flex)                              │
│                                                        │
├────────────────────────────────────────────────────────┤
│              COLLAPSIBLE SUMMARY (88px)                │
│     Summary: 12 Nodes | 96 vCPU | $4,200/mo           │
└────────────────────────────────────────────────────────┘
```

---

## Header Component

| Property | Value |
|----------|-------|
| Height | 64px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (bottom) |

### Header Content

**Left Section:**
- Hamburger menu icon: 24px
- Padding-left: 16px

**Center Section:**
- Logo text: "InfraSizing Calculator"
- Font: 18px / font-weight: 600
- Color: `#58a6ff`

**Right Section:**
- Icons: Theme, Settings, User
- Icon size: 20px
- Icon spacing: 12px
- Padding-right: 16px

---

## Breadcrumb Progress

| Property | Value |
|----------|-------|
| Height | 48px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (bottom) |
| Padding | 0 24px |

### Step Indicators

**Layout:**
- Horizontal centered
- 5 circles connected by lines
- Gap between circles: ~150px (flexible)

**Circle Specs:**
- Size: 28px diameter
- Font: 12px / font-weight: 600

**States:**
| State | Circle BG | Circle Border | Number Color |
|-------|-----------|---------------|--------------|
| Completed | `#3fb950` | none | `#ffffff` |
| Active | `#58a6ff` | none | `#ffffff` |
| Pending | `#21262d` | `#30363d` | `#6e7681` |

**Connecting Lines:**
- Height: 2px
- Completed: `#3fb950`
- Pending: `#30363d`

**Labels Below (optional):**
- Font: 11px / `#8b949e`
- Abbreviated: "Platform", "Tech", "Dist", "Apps", "Price"

---

## Main Content Area

| Property | Value |
|----------|-------|
| Height | calc(100% - 64px - 48px - 88px) |
| Background | `#0d1117` |
| Padding | 24px |

### Content Grid
- 2-column grid for cards
- Gap: 16px
- Cards fill available width

---

## Collapsible Summary Bar

| Property | Value |
|----------|-------|
| Height | 88px (expanded) / 48px (collapsed) |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (top) |
| Padding | 16px 24px |

### Collapsed State
- Single line: "Summary: 12 Nodes | 96 vCPU | $4,200/mo"
- Font: 14px / `#c9d1d9`
- Expand chevron icon on right

### Expanded State (88px)
- Horizontal metric cards
- 4 metrics in a row
- Collapse chevron icon

### Navigation Controls
- Back button: Left aligned
- Summary toggle: Center
- Next button: Right aligned
- Button height: 40px

---

## Figma Implementation Notes

1. **Responsive Behavior:**
   - Content uses 2-column grid
   - Cards span full column width
   - Summary bar collapses on scroll

2. **Components:**
   - Breadcrumb Progress component
   - Collapsible Summary Bar component (2 states)
   - Tablet Card Grid component

3. **Auto Layout:**
   - Main frame: vertical auto layout
   - Breadcrumb: horizontal centered
   - Summary bar: horizontal space-between
