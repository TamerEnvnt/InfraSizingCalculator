# Screen 07: Cost Analysis

**Type:** Results View
**Context:** Desktop main content area
**Purpose:** Cost breakdown and cloud provider comparison

---

## Content Structure

```
┌────────────────────────────────────────────────────────────────┐
│  Cost Analysis                                                  │
│  [On-Prem*] [AWS] [Azure] [GCP]  |  [Compare All]              │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Monthly Cost Breakdown                                         │
│  ┌────────────────────────────────────────────────────────────┐│
│  │ Compute   ████████████████████████████████████ $3,200 (76%)││
│  │ Storage   █████████████                        $800   (19%)││
│  │ Licensing ████                                 $200   (5%) ││
│  └────────────────────────────────────────────────────────────┘│
│                                                                 │
│  TOTAL: $4,200/month ($50,400/year)                            │
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│  Cloud Provider Comparison                                      │
│  ┌────────────────────────────────────────────────────────────┐│
│  │ Provider     │ Monthly  │ vs On-Prem │ 3-Year TCO         ││
│  │──────────────┼──────────┼────────────┼────────────────────││
│  │ On-Premises  │ $4,200   │    --      │ $151,200           ││
│  │ AWS EKS      │ $5,100   │   +21%     │ $183,600           ││
│  │ Azure AKS    │ $4,800   │   +14%     │ $172,800           ││
│  │ Google GKE   │ $4,600   │   +10%     │ $165,600 ★ Lowest  ││
│  └────────────────────────────────────────────────────────────┘│
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Page Header with Tabs

### Title
- Text: "Cost Analysis"
- Font: 24px / font-weight: 600 / `#c9d1d9`

### Tab Bar (Primary)

| Tab | State |
|-----|-------|
| On-Prem | Active |
| AWS | Inactive |
| Azure | Inactive |
| GCP | Inactive |

### Compare All Button
- Position: Right side of tab bar
- Style: Secondary button
- Text: "Compare All"

---

## Monthly Cost Breakdown

### Section Title
- Text: "Monthly Cost Breakdown"
- Font: 16px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 16px

### Bar Chart Container
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 24px |

### Bar Chart Item

| Property | Value |
|----------|-------|
| Height | 32px per bar |
| Gap | 12px |
| Label Width | 100px |

### Bar Styling

| Category | Color | Value | Percentage |
|----------|-------|-------|------------|
| Compute | `#58a6ff` | $3,200 | 76% |
| Storage | `#3fb950` | $800 | 19% |
| Licensing | `#f0883e` | $200 | 5% |

**Bar Structure:**
- Label (left): 14px / `#c9d1d9`
- Bar: Height 24px, border-radius 4px
- Value (right of bar): 14px / `#c9d1d9`
- Percentage (right): 12px / `#8b949e`

### Total Line
| Property | Value |
|----------|-------|
| Margin Top | 16px |
| Padding Top | 16px |
| Border Top | 1px solid `#30363d` |

**Content:**
- Text: "TOTAL: $4,200/month ($50,400/year)"
- Font: 16px / font-weight: 600 / `#c9d1d9`

---

## Cloud Provider Comparison

### Section Title
- Text: "Cloud Provider Comparison"
- Font: 16px / font-weight: 500 / `#c9d1d9`
- Margin: 32px 0 16px

### Comparison Table

| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Overflow | hidden |

### Table Header

| Property | Value |
|----------|-------|
| Background | `#21262d` |
| Height | 48px |
| Font | 12px / font-weight: 500 / `#8b949e` / uppercase |

### Column Headers
| Column | Width | Align |
|--------|-------|-------|
| Provider | 180px | left |
| Monthly | 120px | right |
| vs On-Prem | 120px | center |
| 3-Year TCO | 180px | right |

### Table Rows

| Property | Value |
|----------|-------|
| Height | 52px |
| Border Top | 1px solid `#30363d` |
| Font | 14px / `#c9d1d9` |

### Row Data

| Provider | Monthly | vs On-Prem | 3-Year TCO | Note |
|----------|---------|------------|------------|------|
| On-Premises | $4,200 | -- | $151,200 | Baseline |
| AWS EKS | $5,100 | +21% | $183,600 | |
| Azure AKS | $4,800 | +14% | $172,800 | |
| Google GKE | $4,600 | +10% | $165,600 | Lowest |

### Special Styling

**vs On-Prem Column:**
- Positive (higher): `#f85149`
- Neutral: `#8b949e`
- Negative (lower): `#3fb950`

**Lowest Badge:**
- Text: "Lowest"
- Background: `rgba(63, 185, 80, 0.2)`
- Color: `#3fb950`
- Padding: 4px 8px
- Border Radius: 4px
- Font: 11px / font-weight: 500

### Active Row (On-Prem)
| Property | Value |
|----------|-------|
| Background | `#21262d` |
| Border Left | 3px solid `#58a6ff` |

---

## Figma Implementation Notes

1. **Components:**
   - Horizontal Bar Chart component
   - Comparison Table component
   - Badge component (Lowest indicator)
   - Tab Bar with Compare button

2. **Data Visualization:**
   - Bar widths calculated as percentage of max
   - Use Figma's auto layout for bar items
   - Percentage labels update with values

3. **Interactions:**
   - Tab switching shows different provider details
   - Compare All opens comparison modal/view
