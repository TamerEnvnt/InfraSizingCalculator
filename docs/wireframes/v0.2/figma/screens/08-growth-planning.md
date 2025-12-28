# Screen 08: Growth Planning

**Type:** Results View
**Context:** Desktop main content area
**Purpose:** Growth projections over time

---

## Content Structure

```
┌────────────────────────────────────────────────────────────────┐
│  Growth Planning                                                │
│  Plan your infrastructure scaling over time                     │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Annual Growth Rate          Planning Horizon                   │
│  ────────●────── 25%        [1yr] [2yr] [3yr*] [5yr]           │
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│  Projection Chart                                               │
│  ┌────────────────────────────────────────────────────────────┐│
│  │ 30 ┤                                              ●        ││
│  │    │                                        ●              ││
│  │ 24 ┤                                  ●                    ││
│  │    │                            ●                          ││
│  │ 18 ┤                      ●                                ││
│  │    │                ●                                      ││
│  │ 12 ┼──────────●                                            ││
│  │    └────┬────┬────┬────┬────┬────┬────                     ││
│  │        Now  Q1   Q2   Q3   Q4   Y2   Y3                    ││
│  └────────────────────────────────────────────────────────────┘│
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│  Scaling Milestones                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │   TODAY     │  │   YEAR 1    │  │   YEAR 3    │            │
│  │  12 nodes   │  │  15 nodes   │  │  24 nodes   │            │
│  │  30 apps    │  │  38 apps    │  │  59 apps    │            │
│  │ $4,200/mo   │  │ $5,300/mo   │  │ $8,400/mo   │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
│                                                                 │
│  💡 Plan to add 4 worker nodes by end of Year 1...             │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Page Header

| Element | Specification |
|---------|---------------|
| Title | "Growth Planning" |
| Title Font | 24px / font-weight: 600 / `#c9d1d9` |
| Subtitle | "Plan your infrastructure scaling over time" |
| Subtitle Font | 14px / `#8b949e` |
| Margin Bottom | 24px |

---

## Growth Configuration

### Container
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 48px |
| Margin Bottom | 32px |

### Annual Growth Rate

**Label:**
- Text: "Annual Growth Rate"
- Font: 14px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 12px

**Slider:**
| Property | Value |
|----------|-------|
| Width | 240px |
| Track Height | 4px |
| Track Background | `#30363d` |
| Track Fill | `#58a6ff` |
| Knob Size | 16px |
| Knob Color | `#58a6ff` |

**Value Display:**
- Text: "25%"
- Font: 14px / font-weight: 600 / `#58a6ff`
- Position: Right of slider

### Planning Horizon

**Label:**
- Text: "Planning Horizon"
- Font: 14px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 12px

**Button Group:**
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 8px |

**Horizon Button:**
| Property | Value |
|----------|-------|
| Width | 56px |
| Height | 36px |
| Border Radius | 8px |
| Font | 14px / font-weight: 500 |

**States:**
| State | Background | Text | Border |
|-------|------------|------|--------|
| Inactive | `#21262d` | `#8b949e` | `#30363d` |
| Active | `#58a6ff` | `#ffffff` | none |

**Options:** 1yr, 2yr, 3yr (active), 5yr

---

## Projection Chart

### Container
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 24px |
| Height | 280px |

### Chart Area

**Y-Axis:**
- Labels: 12, 18, 24, 30
- Font: 12px / `#8b949e`
- Grid lines: 1px dashed `#30363d`

**X-Axis:**
- Labels: Now, Q1, Q2, Q3, Q4, Y2, Y3
- Font: 12px / `#8b949e`

**Line:**
- Color: `#58a6ff`
- Width: 2px
- Style: Smooth curve

**Data Points:**
- Size: 8px circles
- Fill: `#58a6ff`
- Border: 2px solid `#0d1117`
- Last point: `#3fb950` (target)

**Area Fill:**
- Gradient from `rgba(88, 166, 255, 0.2)` to transparent
- Below the line

---

## Scaling Milestones

### Section Title
- Text: "Scaling Milestones"
- Font: 16px / font-weight: 500 / `#c9d1d9`
- Margin: 24px 0 16px

### Cards Container
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 16px |

### Milestone Card

| Property | Value |
|----------|-------|
| Width | 180px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 20px |
| Text Align | center |

### Card Content

**Period Label:**
- Font: 11px / font-weight: 600 / `#8b949e` / uppercase
- Margin bottom: 12px

**Nodes:**
- Font: 24px / font-weight: 700 / `#c9d1d9`
- Subtext: "nodes" / 12px / `#8b949e`

**Apps:**
- Font: 16px / font-weight: 500 / `#c9d1d9`
- Subtext: "apps" / 12px / `#8b949e`

**Cost:**
- Font: 14px / font-weight: 500 / `#3fb950`

### Card States

| Card | Period | Nodes | Apps | Cost | Border |
|------|--------|-------|------|------|--------|
| 1 | TODAY | 12 | 30 | $4,200/mo | `#58a6ff` |
| 2 | YEAR 1 | 15 | 38 | $5,300/mo | `#30363d` |
| 3 | YEAR 3 | 24 | 59 | $8,400/mo | `#3fb950` |

---

## Insight Box

### Container
| Property | Value |
|----------|-------|
| Background | `rgba(88, 166, 255, 0.1)` |
| Border Left | 3px solid `#58a6ff` |
| Border Radius | 0 8px 8px 0 |
| Padding | 16px |
| Margin Top | 24px |

### Content
- Icon: Lightbulb / info
- Text: "Plan to add 4 worker nodes by end of Year 1 to maintain 20% headroom with projected growth."
- Font: 14px / `#c9d1d9`

---

## Figma Implementation Notes

1. **Components:**
   - Slider component
   - Horizon Button Group component
   - Line Chart component (use Figma plugin or manual)
   - Milestone Card component

2. **Chart:**
   - Can use Figma charts plugin
   - Or manually draw with line tool + ellipses
   - Ensure consistent styling

3. **Interactions:**
   - Slider updates growth rate
   - Horizon buttons update chart timeframe
   - Values recalculate based on inputs
