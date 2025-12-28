# Screen 04: Platform Selection

**Type:** Wizard Step (Step 1)
**Context:** Desktop main content area
**Purpose:** Choose between Native Applications and Low-Code Platform

---

## Content Structure

```
┌──────────────────────────────────────────────────────┐
│  Select Platform Type                                │
│  Choose the type of applications you'll be deploying │
├──────────────────────────────────────────────────────┤
│                                                      │
│  ┌─────────────────┐    ┌─────────────────┐         │
│  │                 │    │                 │         │
│  │     [ICON]      │    │     [ICON]      │         │
│  │                 │    │                 │         │
│  │    Native       │    │   Low-Code      │         │
│  │  Applications   │    │   Platform      │ ←ACTIVE │
│  │                 │    │                 │         │
│  │ .NET, Java...   │    │ Mendix, OutSys  │         │
│  │                 │    │                 │         │
│  │    [SELECT]     │    │   [SELECTED]    │         │
│  │                 │    │                 │         │
│  └─────────────────┘    └─────────────────┘         │
│                                                      │
│  ┌─────────────────────────────────────────────────┐│
│  │ 💡 TIP: Choose based on your development...     ││
│  └─────────────────────────────────────────────────┘│
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

## Page Header

| Element | Specification |
|---------|---------------|
| Title | "Select Platform Type" |
| Title Font | 24px / font-weight: 600 / `#c9d1d9` |
| Subtitle | "Choose the type of applications you'll be deploying" |
| Subtitle Font | 14px / `#8b949e` |
| Margin Bottom | 32px |

---

## Selection Cards

### Card Container
- Layout: Horizontal, centered
- Gap: 24px
- Max width: 800px

### Individual Card

| Property | Value |
|----------|-------|
| Width | 320px |
| Height | 280px |
| Background | `#161b22` |
| Border | 1px solid `#30363d` |
| Border Radius | 12px |
| Padding | 32px |
| Text Align | center |

### Card Content

**Icon Area:**
- Size: 64px x 64px
- Color: `#8b949e`
- Margin bottom: 24px

**Title:**
- Font: 18px / font-weight: 600
- Color: `#c9d1d9`
- Margin bottom: 8px

**Description:**
- Font: 14px / `#8b949e`
- Margin bottom: 4px

**Subtext:**
- Font: 12px / `#6e7681`
- Margin bottom: 24px

**Button:**
- Width: 100%
- Height: 40px
- Style: Secondary button

### Card States

**Default:**
- Border: 1px solid `#30363d`
- Button: "Select"

**Hover:**
- Border: 1px solid `#8b949e`
- Background: slight lift effect

**Selected:**
- Border: 2px solid `#58a6ff`
- Button: "Selected" with checkmark
- Button style: Primary

---

## Card Details

### Card 1: Native Applications
- **Icon:** Container/microservices visual
- **Title:** "Native Applications"
- **Description:** ".NET, Java, Node.js, Python, Go"
- **Subtext:** "Standard container workloads"

### Card 2: Low-Code Platform
- **Icon:** Low-code/drag-drop visual
- **Title:** "Low-Code Platform"
- **Description:** "Mendix, OutSystems"
- **Subtext:** "Platform-specific runtime requirements"

---

## Info Tip

| Property | Value |
|----------|-------|
| Background | `rgba(88, 166, 255, 0.1)` |
| Border Left | 3px solid `#58a6ff` |
| Border Radius | 0 8px 8px 0 |
| Padding | 16px |
| Margin Top | 32px |
| Max Width | 800px |

**Content:**
- Icon: Lightbulb (💡 or icon)
- Text: "TIP: Choose based on your development team's skills and your organization's existing technology stack."
- Font: 14px / `#c9d1d9`

---

## Figma Implementation Notes

1. **Components:**
   - Platform Selection Card (with variants: default, hover, selected)
   - Info Tip component

2. **Interactions:**
   - Card hover: border color change
   - Card click: toggle selected state
   - Only one card can be selected at a time

3. **Auto Layout:**
   - Cards: horizontal, centered, wrap
   - Card content: vertical, centered
