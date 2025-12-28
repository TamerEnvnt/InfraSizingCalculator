# InfraSizing Calculator - Figma Design Specifications

This folder contains comprehensive design specifications for recreating the InfraSizing Calculator wireframes in Figma.

---

## Contents

### Design System Files

| File | Description |
|------|-------------|
| [design-tokens.json](design-tokens.json) | Tokens Studio compatible design tokens (colors, typography, spacing, etc.) |
| [component-library.md](component-library.md) | Complete component specifications (buttons, forms, cards, navigation, etc.) |

### Screen Specifications

| # | Screen | Type | File |
|---|--------|------|------|
| 01 | Desktop Layout | Layout | [screens/01-desktop-layout.md](screens/01-desktop-layout.md) |
| 02 | Tablet Layout | Layout | [screens/02-tablet-layout.md](screens/02-tablet-layout.md) |
| 03 | Mobile Layout | Layout | [screens/03-mobile-layout.md](screens/03-mobile-layout.md) |
| 04 | Platform Selection | Wizard | [screens/04-platform-selection.md](screens/04-platform-selection.md) |
| 05 | Applications Config | Wizard | [screens/05-applications-config.md](screens/05-applications-config.md) |
| 06 | Sizing Results | Results | [screens/06-sizing-results.md](screens/06-sizing-results.md) |
| 07 | Cost Analysis | Results | [screens/07-cost-analysis.md](screens/07-cost-analysis.md) |
| 08 | Growth Planning | Results | [screens/08-growth-planning.md](screens/08-growth-planning.md) |
| 09 | Scenarios Management | Full Page | [screens/09-scenarios.md](screens/09-scenarios.md) |
| 10 | Settings Page | Full Page | [screens/10-settings.md](screens/10-settings.md) |

---

## How to Use These Specs

### 1. Import Design Tokens

1. Install the [Tokens Studio for Figma](https://tokens.studio/) plugin
2. Open the plugin in Figma
3. Import `design-tokens.json`
4. Apply tokens to your designs

### 2. Create Components

1. Read `component-library.md` for detailed component specs
2. Create each component as a Figma component with variants
3. Use auto layout for responsive behavior
4. Apply design tokens for consistent styling

### 3. Build Screens

1. Start with layout templates (01-03)
2. Create reusable frames for header, sidebar, and content areas
3. Build each screen following the detailed specifications
4. Link components to create interactive prototypes

---

## Design Token Structure

```
global/
├── colors/
│   ├── bg/           (primary, secondary, tertiary)
│   ├── border/       (primary, secondary)
│   ├── text/         (primary, secondary, tertiary)
│   ├── accent/       (blue, green, orange, red, purple)
│   └── button/       (primary, secondary, danger)
├── typography/
│   ├── fontFamily/   (primary, mono)
│   ├── fontSize/     (xs through 4xl)
│   ├── fontWeight/   (normal through bold)
│   └── lineHeight/   (tight, normal, relaxed)
├── spacing/          (xs through 5xl)
├── borderRadius/     (sm, md, lg, full)
├── shadows/          (sm, md, lg)
└── layout/
    ├── desktop/      (dimensions for 1440x900)
    ├── tablet/       (dimensions for 1024x800)
    └── mobile/       (dimensions for 375x812)
```

---

## Color Palette Reference

| Token | Value | Usage |
|-------|-------|-------|
| `bg.primary` | `#0d1117` | Main background |
| `bg.secondary` | `#161b22` | Cards, panels |
| `bg.tertiary` | `#21262d` | Inputs, hovers |
| `border.primary` | `#30363d` | Default borders |
| `text.primary` | `#c9d1d9` | Main text |
| `text.secondary` | `#8b949e` | Labels, hints |
| `accent.blue` | `#58a6ff` | Actions, links |
| `accent.green` | `#3fb950` | Success, positive |
| `accent.orange` | `#f0883e` | Warnings, starred |
| `accent.red` | `#f85149` | Errors, danger |

---

## Component Checklist

### Core Components
- [ ] Primary Button
- [ ] Secondary Button
- [ ] Danger Button
- [ ] Text Input
- [ ] Number Input
- [ ] Checkbox
- [ ] Radio Button
- [ ] Toggle Switch
- [ ] Slider
- [ ] Select Dropdown

### Layout Components
- [ ] Header (Desktop, Tablet, Mobile)
- [ ] Sidebar Navigation
- [ ] Wizard Step Item
- [ ] Context Panel
- [ ] Breadcrumb Progress
- [ ] Bottom Navigation (Mobile)

### Card Components
- [ ] Base Card
- [ ] Selection Card
- [ ] Summary Card (Metric)
- [ ] Scenario Card
- [ ] Milestone Card

### Feedback Components
- [ ] Info Tip Box
- [ ] Recommendation Box
- [ ] Warning Box
- [ ] Badge

---

## Prototype Flows

### Main Flow
1. Platform Selection → Technology → Distribution → Applications → Pricing
2. Each step updates the context panel summary
3. Results views accessible after wizard completion

### Secondary Flows
- Settings page (from header icon)
- Scenarios page (from sidebar)
- Comparison modal (from scenarios)

---

## File Organization in Figma

Recommended page structure:

```
📁 InfraSizing Calculator
├── 📄 Cover
├── 📄 Design Tokens
├── 📄 Component Library
├── 📄 Icons
├── 📁 Layouts
│   ├── 📄 Desktop
│   ├── 📄 Tablet
│   └── 📄 Mobile
├── 📁 Wizard Steps
│   ├── 📄 Platform Selection
│   ├── 📄 Technology Selection
│   ├── 📄 Distribution Selection
│   ├── 📄 Applications Config
│   └── 📄 Pricing Config
├── 📁 Results Views
│   ├── 📄 Sizing Results
│   ├── 📄 Cost Analysis
│   └── 📄 Growth Planning
├── 📁 Full Pages
│   ├── 📄 Scenarios
│   └── 📄 Settings
└── 📄 Prototype
```

---

## Related Resources

- [WIREFRAME_SPEC.md](../WIREFRAME_SPEC.md) - Source specification document
- [HTML Wireframes](../html/index.html) - Interactive HTML/CSS prototypes
- [SVG Wireframes](../svg/index.html) - Vector-based wireframes
- [Excalidraw Wireframes](../excalidraw/index.html) - Editable diagram files
