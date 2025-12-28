# Screen 05: Applications Configuration

**Type:** Wizard Step (Step 4)
**Context:** Desktop main content area
**Purpose:** Configure application counts per environment and size

---

## Content Structure

```
┌──────────────────────────────────────────────────────────────┐
│  Configure Applications                                       │
│  Set the number of applications per environment and size      │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Environment Toggles:                                         │
│  [✓ DEV] [✓ TEST] [✓ STAGING] [✓ PROD]                       │
│                                                               │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │           │   DEV   │  TEST   │ STAGING │   PROD   │    │ │
│  │───────────┼─────────┼─────────┼─────────┼──────────│    │ │
│  │ Small(1c) │   [5]   │   [5]   │   [3]   │   [3]    │ 16 │ │
│  │ Medium(2c)│   [3]   │   [3]   │   [2]   │   [2]    │ 10 │ │
│  │ Large(4c) │   [1]   │   [1]   │   [1]   │   [1]    │  4 │ │
│  │───────────┼─────────┼─────────┼─────────┼──────────│    │ │
│  │ TOTAL     │    9    │    9    │    6    │    6     │ 30 │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
│  Quick Presets:                                               │
│  [Startup (10 apps)] [Medium (50 apps)] [Enterprise (100+)]   │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## Page Header

| Element | Specification |
|---------|---------------|
| Title | "Configure Applications" |
| Title Font | 24px / font-weight: 600 / `#c9d1d9` |
| Subtitle | "Set the number of applications per environment and size" |
| Subtitle Font | 14px / `#8b949e` |
| Margin Bottom | 24px |

---

## Environment Toggles

### Container
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 12px |
| Margin Bottom | 24px |

### Toggle Pill

| Property | Value |
|----------|-------|
| Height | 36px |
| Padding | 0 16px |
| Border Radius | 18px (full) |
| Font | 14px / font-weight: 500 |

**States:**

| State | Background | Text | Border |
|-------|------------|------|--------|
| Off | `#21262d` | `#8b949e` | `#30363d` |
| On | `#238636` | `#ffffff` | none |

### Environments
- DEV (on by default)
- TEST (on by default)
- STAGING (on by default)
- PROD (on by default)

---

## Application Matrix Table

### Table Container
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Overflow | hidden |

### Table Header Row
| Property | Value |
|----------|-------|
| Background | `#21262d` |
| Height | 48px |
| Font | 12px / font-weight: 500 / `#8b949e` / uppercase |

### Column Widths
| Column | Width |
|--------|-------|
| Size Label | 140px |
| DEV | 100px |
| TEST | 100px |
| STAGING | 100px |
| PROD | 100px |
| Row Total | 80px |

### Size Rows

| Size | Label | vCPU | Memory |
|------|-------|------|--------|
| Small | "Small (1 vCPU)" | 1 | 2 GB |
| Medium | "Medium (2 vCPU)" | 2 | 4 GB |
| Large | "Large (4 vCPU)" | 4 | 8 GB |

### Number Input Cell

| Property | Value |
|----------|-------|
| Width | 60px |
| Height | 36px |
| Background | `#0d1117` |
| Border | 1px solid `#30363d` |
| Border Radius | 6px |
| Text Align | center |
| Font | 14px / font-weight: 500 / `#c9d1d9` |

### Total Row
| Property | Value |
|----------|-------|
| Background | `#21262d` |
| Font | 14px / font-weight: 600 / `#c9d1d9` |
| Border Top | 1px solid `#30363d` |

### Grand Total Cell
| Property | Value |
|----------|-------|
| Background | `#58a6ff` |
| Color | `#ffffff` |
| Font | 14px / font-weight: 700 |

---

## Quick Presets

### Container
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 12px |
| Margin Top | 24px |

### Preset Label
- Text: "Quick Presets:"
- Font: 14px / `#8b949e`
- Margin Right: 8px

### Preset Button

| Property | Value |
|----------|-------|
| Height | 36px |
| Padding | 0 16px |
| Background | `#21262d` |
| Border | 1px solid `#30363d` |
| Border Radius | 8px |
| Font | 13px / `#c9d1d9` |

**Hover:** Border `#8b949e`

**Presets:**
- "Startup (10 apps)"
- "Medium (50 apps)"
- "Enterprise (100+)"

---

## Figma Implementation Notes

1. **Components:**
   - Environment Toggle Pill (on/off states)
   - Number Input component
   - Preset Button component
   - Matrix Table component

2. **Data Binding:**
   - Inputs update row totals
   - Row totals update column totals
   - All totals update grand total

3. **Validation:**
   - Minimum: 0
   - Maximum: 999
   - Only integers allowed
