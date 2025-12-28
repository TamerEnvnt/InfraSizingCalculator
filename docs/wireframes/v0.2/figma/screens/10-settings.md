# Screen 10: Settings Page

**Type:** Full Page
**Context:** Desktop with sidebar
**Purpose:** Application configuration

---

## Content Structure

```
┌────────────────────────────────────────────────────────────────┐
│  Settings                                                       │
│  Configure application preferences                              │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Appearance                                               │  │
│  │                                                           │  │
│  │  Theme:  (○) Light  (●) Dark  (○) System    [Preview]    │  │
│  │                                                           │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Pricing Configuration                                    │  │
│  │                                                           │  │
│  │  On-Premises Pricing                                      │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │ Component      │    Unit    │  Monthly Cost       │  │  │
│  │  │────────────────┼────────────┼─────────────────────│  │  │
│  │  │ Compute (vCPU) │  per core  │    [$35.00]         │  │  │
│  │  │ Memory (RAM)   │  per GB    │    [$5.00]          │  │  │
│  │  │ Storage (Disk) │  per GB    │    [$0.10]          │  │  │
│  │  │ Licensing      │  flat      │    [$200.00]        │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                                                           │  │
│  │  [Reset to Defaults]                                      │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Data Management                                          │  │
│  │                                                           │  │
│  │  [Import Configuration] [Export All Scenarios]            │  │
│  │                                                           │  │
│  │  [Clear All Data]  ← danger                               │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
│                                           [Save Changes]        │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Page Header

| Element | Specification |
|---------|---------------|
| Title | "Settings" |
| Title Font | 28px / font-weight: 600 / `#c9d1d9` |
| Subtitle | "Configure application preferences" |
| Subtitle Font | 14px / `#8b949e` |
| Margin Bottom | 32px |

---

## Section 1: Appearance

### Section Card
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 24px |
| Margin Bottom | 24px |

### Section Title
- Text: "Appearance"
- Font: 18px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 20px

### Theme Selector

**Label:**
- Text: "Theme"
- Font: 14px / `#8b949e`
- Margin bottom: 12px

**Radio Group:**
| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 24px |

**Radio Option:**
| Property | Value |
|----------|-------|
| Radio Size | 20px |
| Radio-Label Gap | 8px |
| Label Font | 14px / `#c9d1d9` |

**Options:**
- Light (unchecked)
- Dark (checked)
- System (unchecked)

### Theme Preview

| Property | Value |
|----------|-------|
| Width | 320px |
| Height | 112px |
| Background | `#0d1117` |
| Border | 1px solid `#30363d` |
| Border Radius | 8px |
| Position | Right side |
| Padding | 16px |

**Preview Content:**
- Label: "Preview" / 12px / `#8b949e`
- Mini card showing current theme colors
- Text: "Dark theme active" / 12px / `#c9d1d9`

---

## Section 2: Pricing Configuration

### Section Card
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 24px |
| Margin Bottom | 24px |

### Section Title
- Text: "Pricing Configuration"
- Font: 18px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 8px

### Subsection Title
- Text: "On-Premises Pricing"
- Font: 14px / `#8b949e`
- Margin bottom: 16px

### Pricing Table

| Property | Value |
|----------|-------|
| Background | `#21262d` |
| Border | 1px solid `#30363d` |
| Border Radius | 8px |

**Header Row:**
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Height | 32px |
| Font | 12px / `#8b949e` |

**Columns:**
| Column | Width | Align |
|--------|-------|-------|
| Component | 160px | left |
| Unit | 100px | center |
| Monthly Cost | 120px | right |

**Data Rows:**

| Component | Unit | Default Value |
|-----------|------|---------------|
| Compute (vCPU) | per core | $35.00 |
| Memory (RAM) | per GB | $5.00 |
| Storage (Disk) | per GB | $0.10 |
| Licensing | flat | $200.00 |

### Price Input

| Property | Value |
|----------|-------|
| Width | 100px |
| Height | 28px |
| Background | `#0d1117` |
| Border | 1px solid `#30363d` |
| Border Radius | 4px |
| Text Align | right |
| Padding Right | 8px |
| Font | 13px / `#c9d1d9` |

### Reset Button
- Text: "Reset to Defaults"
- Style: Secondary button
- Position: Below table, right aligned
- Margin top: 16px

---

## Section 3: Data Management

### Section Card
| Property | Value |
|----------|-------|
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 24px |
| Margin Bottom | 32px |

### Section Title
- Text: "Data Management"
- Font: 18px / font-weight: 500 / `#c9d1d9`
- Margin bottom: 20px

### Action Buttons Row

| Property | Value |
|----------|-------|
| Layout | Horizontal |
| Gap | 12px |
| Margin Bottom | 16px |

**Import Button:**
| Property | Value |
|----------|-------|
| Width | 200px |
| Height | 40px |
| Style | Secondary |
| Text | "Import Configuration" |

**Export Button:**
| Property | Value |
|----------|-------|
| Width | 200px |
| Height | 40px |
| Style | Secondary |
| Text | "Export All Scenarios" |

### Danger Zone

**Clear Data Button:**
| Property | Value |
|----------|-------|
| Width | 160px |
| Height | 40px |
| Background | transparent |
| Border | 1px solid `#f85149` |
| Text | "Clear All Data" / `#f85149` |

**Warning on hover:**
- Tooltip or inline warning
- "This action cannot be undone"

---

## Page Footer

### Container
| Property | Value |
|----------|-------|
| Position | Bottom right |
| Margin Top | 32px |

### Save Button
| Property | Value |
|----------|-------|
| Width | 128px |
| Height | 44px |
| Background | `#238636` |
| Border Radius | 8px |
| Font | 14px / font-weight: 500 / `#ffffff` |
| Text | "Save Changes" |

---

## Figma Implementation Notes

1. **Components:**
   - Settings Section Card component
   - Radio Group component
   - Pricing Table component
   - Price Input component
   - Action Button variants

2. **Form Handling:**
   - Inputs should show focus states
   - Validation for price inputs (numbers only)
   - Dirty state detection for save button

3. **Confirmations:**
   - Clear All Data: Confirmation modal required
   - Export: File download dialog
   - Import: File picker dialog

4. **Responsive:**
   - Max content width: 800px
   - Center align on larger screens
   - Stack buttons on smaller screens
