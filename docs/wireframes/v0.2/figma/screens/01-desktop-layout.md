# Screen 01: Desktop Layout

**Type:** Layout Template
**Dimensions:** 1440 x 900px
**Purpose:** Define the 3-panel desktop structure

---

## Frame Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                         HEADER (64px)                           │
├────────────┬───────────────────────────────────┬────────────────┤
│            │                                   │                │
│  SIDEBAR   │          MAIN CONTENT             │ CONTEXT PANEL  │
│   240px    │            flex                   │    280px       │
│            │                                   │                │
│            │                                   │                │
└────────────┴───────────────────────────────────┴────────────────┘
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
- Logo text: "InfraSizing Calculator"
- Font: 20px / font-weight: 600
- Color: `#58a6ff`
- Padding-left: 24px

**Right Section:**
- Icons: Theme toggle, Settings, User
- Icon size: 20px
- Icon color: `#8b949e`
- Icon spacing: 16px
- Padding-right: 24px

---

## Sidebar Component

| Property | Value |
|----------|-------|
| Width | 240px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (right) |

### Sidebar Content

**Section Label:**
- Text: "WIZARD STEPS"
- Font: 11px / font-weight: 500 / uppercase
- Color: `#6e7681`
- Padding: 24px 16px 12px

**Wizard Steps (5 items):**

| Step | Label | State |
|------|-------|-------|
| 1 | Platform | completed (green circle + check) |
| 2 | Technology | active (blue circle + number) |
| 3 | Distribution | pending (gray circle + number) |
| 4 | Applications | pending |
| 5 | Pricing | pending |

**Step Item Layout:**
- Height: 44px
- Padding: 0 16px
- Circle: 24px diameter
- Circle-text gap: 12px
- Connecting line: 2px wide, centered under circles

---

## Main Content Area

| Property | Value |
|----------|-------|
| Width | calc(100% - 240px - 280px) |
| Background | `#0d1117` |
| Padding | 32px |

### Content Placeholder
- Centered text: "Wizard Step Content"
- Font: 16px / `#8b949e`

---

## Context Panel

| Property | Value |
|----------|-------|
| Width | 280px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` (left) |
| Padding | 24px |

### Context Panel Content

**Section: Summary**
- Title: "CURRENT SUMMARY"
- Font: 11px / font-weight: 500 / uppercase
- Color: `#6e7681`
- Margin-bottom: 16px

**Metric Cards (4 stacked):**

| Metric | Value | Subtext |
|--------|-------|---------|
| Nodes | 12 | +2 HA |
| vCPUs | 96 | Total |
| Memory | 384 GB | Total |
| Est. Cost | $4,200 | /month |

**Card Styling:**
- Background: `#21262d`
- Border-radius: 8px
- Padding: 12px
- Margin-bottom: 8px
- Value font: 24px / font-weight: 700 / `#c9d1d9`
- Label font: 12px / `#8b949e`

---

## Figma Implementation Notes

1. **Auto Layout:**
   - Use horizontal auto layout for main frame
   - Sidebar and context panel: fixed width, fill height
   - Main content: fill width, fill height

2. **Components:**
   - Create Header component with variants
   - Create Sidebar component
   - Create Context Panel component
   - Create Wizard Step item component with states

3. **Constraints:**
   - Header: fixed top, stretch horizontal
   - Sidebar: fixed left, stretch vertical
   - Context panel: fixed right, stretch vertical
