# Screen 03: Mobile Layout

**Type:** Layout Template
**Dimensions:** 375 x 812px
**Purpose:** Mobile-first single column layout

---

## Frame Structure

```
┌───────────────────────────────┐
│        HEADER (56px)          │
│  ☰  InfraSiz...     •••       │
├───────────────────────────────┤
│    STEP INDICATOR (48px)      │
│     ● ○ ○ ○ ○  Step 1 of 5    │
├───────────────────────────────┤
│                               │
│        CONTENT AREA           │
│       (single column)         │
│                               │
│                               │
│                               │
├───────────────────────────────┤
│      BOTTOM NAV (64px)        │
│  Back | Summary | Next | Menu │
└───────────────────────────────┘
```

---

## Header Component

| Property | Value |
|----------|-------|
| Height | 56px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (bottom) |
| Padding | 0 16px |

### Header Content

**Left:** Hamburger icon (24px)

**Center:**
- Logo: "InfraSiz" (truncated)
- Font: 16px / font-weight: 600
- Color: `#58a6ff`

**Right:** Overflow menu (3 dots, 24px)

---

## Step Indicator

| Property | Value |
|----------|-------|
| Height | 48px |
| Background | `#0d1117` |
| Padding | 12px 16px |

### Indicator Layout

**Dot Row (centered):**
- 5 dots, 8px each
- Gap: 8px
- Active: `#58a6ff`
- Inactive: `#30363d`

**Step Text (right of dots):**
- "Step 1 of 5"
- Font: 13px / `#8b949e`

---

## Content Area

| Property | Value |
|----------|-------|
| Height | calc(100% - 56px - 48px - 64px) |
| Background | `#0d1117` |
| Padding | 16px |
| Overflow | scroll |

### Card Layout
- Single column
- Full width cards
- Gap: 12px
- Min tap target: 44px

---

## Bottom Navigation

| Property | Value |
|----------|-------|
| Height | 64px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (top) |
| Padding | 8px 16px |

### Navigation Items (4 sections)

| Section | Width | Content |
|---------|-------|---------|
| Back | 25% | Icon + "Back" |
| Summary | 25% | Icon (sheet trigger) |
| Next | 25% | Icon + "Next" |
| Menu | 25% | Icon (more options) |

**Item Styling:**
- Height: 48px
- Icon: 20px, centered above
- Label: 11px / `#8b949e`
- Active: `#58a6ff`

---

## Bottom Sheet (Summary)

**Triggered by:** Summary icon tap

| Property | Value |
|----------|-------|
| Height | 60% of viewport |
| Background | `#161b22` |
| Border Radius | 16px 16px 0 0 |
| Padding | 24px |

### Sheet Content
- Drag handle: 32px x 4px centered, `#30363d`
- Title: "Summary"
- 4 metric cards (stacked)
- Close button or swipe down to dismiss

---

## Figma Implementation Notes

1. **Touch Targets:**
   - All interactive elements: min 44px
   - Buttons: full-width, 48px height
   - Card actions: spaced for thumb reach

2. **Components:**
   - Mobile Header component
   - Step Indicator Dots component
   - Bottom Navigation component
   - Bottom Sheet component (overlay)

3. **Prototyping:**
   - Summary icon opens bottom sheet
   - Hamburger opens side drawer
   - Swipe gestures for navigation

4. **Safe Areas:**
   - Account for notch (top)
   - Account for home indicator (bottom)
