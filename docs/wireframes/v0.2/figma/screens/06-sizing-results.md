# Screen 06: Sizing Results

**Type:** Results View
**Context:** Desktop main content area
**Purpose:** Display infrastructure sizing overview

---

## Content Structure

```
┌────────────────────────────────────────────────────────────────┐
│  Sizing Results                                                 │
│  [Overview*] [Nodes] [Resources] [By Environment]              │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │ 12 NODES │  │ 96 vCPUs │  │ 384 GB   │  │ $4,200   │       │
│  │   +2 HA  │  │  Total   │  │   RAM    │  │  /MONTH  │       │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘       │
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│  Cluster Architecture                                           │
│                                                                 │
│     CONTROL PLANE (3 nodes)                                    │
│     ┌─────┐  ┌─────┐  ┌─────┐                                 │
│     │ M1  │  │ M2  │  │ M3  │   4 vCPU / 16 GB each           │
│     └─────┘  └─────┘  └─────┘                                 │
│                 │                                               │
│                 ▼                                               │
│     WORKER NODES (9 nodes)                                     │
│     ┌───┬───┬───┬───┬───┬───┬───┬───┬───┐                    │
│     │W1 │W2 │W3 │W4 │W5 │W6 │W7 │W8 │W9 │  8 vCPU / 32 GB   │
│     └───┴───┴───┴───┴───┴───┴───┴───┴───┘                    │
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│  💡 Recommendations                                             │
│  • Consider adding 1 more worker node for 20% headroom         │
│  • Enable pod disruption budgets for high availability         │
└────────────────────────────────────────────────────────────────┘
```

---

## Page Header with Tabs

### Title
- Text: "Sizing Results"
- Font: 24px / font-weight: 600 / `#c9d1d9`

### Tab Bar

| Property | Value |
|----------|-------|
| Margin Top | 12px |
| Gap | 4px |

### Tab Item

| Property | Value |
|----------|-------|
| Height | 36px |
| Padding | 0 16px |
| Border Radius | 8px 8px 0 0 |
| Font | 14px / font-weight: 500 |

**States:**
| State | Background | Text | Border Bottom |
|-------|------------|------|---------------|
| Inactive | transparent | `#8b949e` | none |
| Active | `#161b22` | `#c9d1d9` | 2px `#58a6ff` |
| Hover | `#21262d` | `#c9d1d9` | none |

**Tabs:**
- Overview (active)
- Nodes
- Resources
- By Environment

---

## Summary Cards Row

### Container
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 16px |
| Margin | 24px 0 |

### Summary Card

| Property | Value |
|----------|-------|
| Width | 160px |
| Height | 100px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 16px |
| Text Align | center |

### Card Content

**Value:**
- Font: 32px / font-weight: 700 / `#c9d1d9`

**Subtext:**
- Font: 11px / `#6e7681`
- Margin top: 4px

**Label:**
- Font: 12px / font-weight: 500 / `#8b949e`
- Margin top: 8px

### Card Data

| Card | Value | Subtext | Label |
|------|-------|---------|-------|
| 1 | 12 | +2 HA | NODES |
| 2 | 96 | Total | vCPUs |
| 3 | 384 GB | Total | RAM |
| 4 | $4,200 | /month | EST. COST |

---

## Cluster Architecture Diagram

### Container
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 24px |

### Section Title
- Text: "Cluster Architecture"
- Font: 16px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 24px

### Control Plane Section

**Label:**
- Text: "CONTROL PLANE (3 nodes)"
- Font: 12px / font-weight: 500 / `#8b949e` / uppercase

**Node Boxes (3):**
| Property | Value |
|----------|-------|
| Width | 64px |
| Height | 48px |
| Background | `#21262d` |
| Border | 1px solid `#58a6ff` |
| Border Radius | 8px |
| Gap | 12px |

**Node Label:**
- Text: "M1", "M2", "M3"
- Font: 14px / font-weight: 600 / `#c9d1d9`

**Specs Text:**
- Text: "4 vCPU / 16 GB each"
- Font: 12px / `#8b949e`
- Position: Right of nodes

### Connection Arrow
- Vertical line from center master
- Arrow pointing down
- Color: `#30363d`
- Width: 2px

### Worker Nodes Section

**Label:**
- Text: "WORKER NODES (9 nodes)"
- Font: 12px / font-weight: 500 / `#8b949e` / uppercase

**Node Boxes (9):**
| Property | Value |
|----------|-------|
| Width | 48px |
| Height | 40px |
| Background | `#21262d` |
| Border | 1px solid `#3fb950` |
| Border Radius | 6px |
| Gap | 8px |

**Node Labels:**
- Text: "W1" through "W9"
- Font: 12px / font-weight: 500 / `#c9d1d9`

**Specs Text:**
- Text: "8 vCPU / 32 GB each"
- Font: 12px / `#8b949e`

---

## Recommendations Box

### Container
| Property | Value |
|----------|-------|
| Background | `rgba(63, 185, 80, 0.1)` |
| Border Left | 3px solid `#3fb950` |
| Border Radius | 0 8px 8px 0 |
| Padding | 16px |
| Margin Top | 24px |

### Title
- Icon: Lightbulb
- Text: "Recommendations"
- Font: 14px / font-weight: 500 / `#c9d1d9`

### Items
- Bullet points
- Font: 14px / `#c9d1d9`
- Line height: 1.6

---

## Figma Implementation Notes

1. **Components:**
   - Tab Bar component (with active state)
   - Summary Card component
   - Node Box component (master/worker variants)
   - Recommendation Box component

2. **Diagram:**
   - Use Figma's line tool for connections
   - Group control plane and worker sections
   - Use auto layout for node rows
