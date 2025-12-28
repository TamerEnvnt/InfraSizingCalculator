# Screen 09: Scenarios Management

**Type:** Full Page
**Context:** Desktop with sidebar
**Purpose:** Save, load, and compare scenarios

---

## Content Structure

```
┌────────────────────────────────────────────────────────────────┐
│  Saved Scenarios              🔍 Search...    [+ New Scenario] │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌────────────────────────────┐  ┌────────────────────────────┐│
│  │ ★ Production K8s Cluster   │  │   Development Environment  ││
│  │   Mendix • OpenShift       │  │   .NET • K3s               ││
│  │   12 nodes • $4,200/mo     │  │   4 nodes • $800/mo        ││
│  │   Created Dec 20           │  │   Created Dec 15           ││
│  │   [Load][Duplicate][✓]     │  │   [Load][Duplicate][ ]     ││
│  └────────────────────────────┘  └────────────────────────────┘│
│                                                                 │
│  ┌────────────────────────────┐                                │
│  │   Cloud Migration Plan     │                                │
│  │   OutSystems • EKS         │                                │
│  │   8 nodes • $3,500/mo      │                                │
│  │   Created Dec 18           │                                │
│  │   [Load][Duplicate][✓]     │                                │
│  └────────────────────────────┘                                │
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│  Comparison (2 selected)                                        │
│  ┌────────────────────────────────────────────────────────────┐│
│  │ Metric        │ Production K8s  │ Cloud Migration         ││
│  │───────────────┼─────────────────┼─────────────────────────││
│  │ Nodes         │       12        │          8              ││
│  │ vCPUs         │       96        │         64              ││
│  │ RAM           │     384 GB      │       256 GB            ││
│  │ Cost/mo       │    $4,200       │      $3,500             ││
│  │ 3yr TCO       │   $151,200      │ $126,000 (17% savings)  ││
│  └────────────────────────────────────────────────────────────┘│
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Page Header

### Container
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border Radius | 12px |
| Padding | 20px 24px |
| Margin Bottom | 24px |
| Layout | Horizontal space-between |

### Title
- Text: "Saved Scenarios"
- Font: 24px / font-weight: 600 / `#c9d1d9`

### Search Box
| Property | Value |
|----------|-------|
| Width | 280px |
| Height | 40px |
| Background | `#21262d` |
| Border | 1px solid `#30363d` |
| Border Radius | 8px |
| Padding | 0 16px |

**Placeholder:**
- Icon: 🔍 (16px)
- Text: "Search scenarios..."
- Font: 14px / `#8b949e`

### New Scenario Button
| Property | Value |
|----------|-------|
| Width | 144px |
| Height | 40px |
| Background | `#238636` |
| Border Radius | 8px |
| Font | 14px / font-weight: 500 / `#ffffff` |

---

## Scenario Cards Grid

### Container
| Property | Value |
|----------|-------|
| Layout | 2-column grid |
| Gap | 16px |
| Margin Bottom | 32px |

### Scenario Card

| Property | Value |
|----------|-------|
| Width | 100% (of column) |
| Min Height | 140px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 20px |

### Card States

**Default:**
- Border: 1px solid `#30363d`

**Starred:**
- Border: 2px solid `#f0883e`

**Selected (for comparison):**
- Border: 2px solid `#58a6ff`

### Card Content

**Row 1: Title**
- Star icon (if starred): `#f0883e` / 18px
- Title: 18px / font-weight: 500 / `#c9d1d9`
- Compare checkbox: Right aligned

**Row 2: Details**
- Format: "Technology • Distribution • X nodes • $X,XXX/mo"
- Font: 14px / `#8b949e`

**Row 3: Dates**
- Format: "Created Dec 20 • Modified Dec 27"
- Font: 12px / `#6e7681`

**Row 4: Actions**
- Buttons: Load, Duplicate, Edit, Delete
- Button height: 28px
- Gap: 8px

### Action Buttons

| Button | Style | Border | Text |
|--------|-------|--------|------|
| Load | Ghost | `#58a6ff` | `#58a6ff` |
| Duplicate | Ghost | `#30363d` | `#c9d1d9` |
| Edit | Ghost | `#30363d` | `#c9d1d9` |
| Delete | Ghost | `#f85149` | `#f85149` |

### Compare Checkbox
| Property | Value |
|----------|-------|
| Size | 24px |
| Border Radius | 4px |
| Unchecked | `#21262d` border `#30363d` |
| Checked | `#58a6ff` with ✓ icon |

---

## Comparison Section

### Section Title
- Text: "Comparison (2 selected)"
- Font: 18px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 16px

### Comparison Table

| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |

### Table Structure

**Header Row:**
| Property | Value |
|----------|-------|
| Background | `#21262d` |
| Height | 44px |
| Font | 12px / font-weight: 500 / `#8b949e` |

**Column Headers:**
- Metric (left aligned)
- Scenario 1 name (center, color: `#f0883e`)
- Scenario 2 name (center, color: `#58a6ff`)

**Data Rows:**
| Property | Value |
|----------|-------|
| Height | 44px |
| Border Top | 1px solid `#30363d` |
| Font | 14px / `#c9d1d9` |

### Comparison Data

| Metric | Production K8s | Cloud Migration |
|--------|----------------|-----------------|
| Nodes | 12 | 8 |
| vCPUs | 96 | 64 |
| RAM | 384 GB | 256 GB |
| Cost/mo | $4,200 | $3,500 |
| 3yr TCO | $151,200 | $126,000 |

### Savings Highlight
- Text: "(17% savings)"
- Color: `#3fb950`
- Font: 14px / font-weight: 500

---

## Scenario Card Details

### Card 1: Production K8s Cluster
- **Starred:** Yes
- **Title:** Production K8s Cluster
- **Technology:** Mendix
- **Distribution:** OpenShift
- **Nodes:** 12
- **Cost:** $4,200/mo
- **Created:** Dec 20
- **Modified:** Dec 27
- **Selected:** Yes (for comparison)

### Card 2: Development Environment
- **Starred:** No
- **Title:** Development Environment
- **Technology:** .NET
- **Distribution:** K3s
- **Nodes:** 4
- **Cost:** $800/mo
- **Created:** Dec 15
- **Modified:** Dec 22
- **Selected:** No

### Card 3: Cloud Migration Plan
- **Starred:** No
- **Title:** Cloud Migration Plan
- **Technology:** OutSystems
- **Distribution:** EKS
- **Nodes:** 8
- **Cost:** $3,500/mo
- **Created:** Dec 18
- **Modified:** Dec 26
- **Selected:** Yes (for comparison)

---

## Figma Implementation Notes

1. **Components:**
   - Scenario Card component (variants: default, starred, selected)
   - Comparison Table component
   - Action Button component

2. **Interactions:**
   - Checkbox toggles comparison selection
   - Comparison table shows when 2+ selected
   - Load opens scenario in wizard
   - Delete shows confirmation modal

3. **Empty State:**
   - Show when no scenarios exist
   - CTA: "Create your first scenario"
