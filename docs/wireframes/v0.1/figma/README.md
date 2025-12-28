# Figma Design Specifications

This folder contains Figma-compatible design specifications for the Infrastructure Sizing Calculator UX wireframes.

## Important Note

Figma's native `.fig` file format is **proprietary and binary** - it cannot be created programmatically outside of Figma. Instead, these JSON specifications provide:

1. **Design Tokens** - W3C Design Token standard compatible colors, typography, spacing, etc.
2. **Component Specifications** - Detailed JSON descriptions of each wireframe

## Files

| File | Description |
|------|-------------|
| `design-tokens.json` | W3C Design Tokens format - import via [Tokens Studio for Figma](https://tokens.studio/) plugin |
| `01-layout-desktop.figma.json` | Desktop 3-panel layout specification |
| `02-layout-tablet.figma.json` | Tablet layout with horizontal progress bar |
| `03-layout-mobile.figma.json` | Mobile layout with bottom navigation |
| `04-wizard-platform.figma.json` | Platform selection step |
| `05-wizard-applications.figma.json` | Applications configuration step |
| `06-results-sizing.figma.json` | Sizing results overview |
| `07-results-cost.figma.json` | Cost analysis view |
| `08-results-growth.figma.json` | Growth projections |
| `09-scenarios.figma.json` | Scenarios management |
| `10-settings.figma.json` | Settings page |

## How to Use These Specifications

### Option 1: Import Design Tokens

1. Install [Tokens Studio for Figma](https://www.figma.com/community/plugin/843461159747178978)
2. Open the plugin in Figma
3. Import `design-tokens.json`
4. Use the tokens to build components matching the specifications

### Option 2: Manual Recreation

Use the `.figma.json` files as detailed specifications to manually create the wireframes in Figma:

- Each file contains frame hierarchy, positions, sizes, colors, and text
- Element types map directly to Figma primitives (FRAME, TEXT, ELLIPSE, LINE)
- Colors use the exact hex values from the design tokens

### Option 3: Figma API

For programmatic creation, use the [Figma API](https://www.figma.com/developers/api) with these specifications:

```javascript
// Example: Create a frame using Figma API
const frame = figma.createFrame();
frame.name = "Header";
frame.resize(1440, 64);
frame.fills = [{ type: 'SOLID', color: { r: 0.004, g: 0.016, b: 0.035 } }];
```

## Design System Overview

### Color Palette

| Category | Token | Value | Usage |
|----------|-------|-------|-------|
| Background | `colors.background.primary` | `#0d1117` | Main app background |
| Background | `colors.background.secondary` | `#161b22` | Card/panel background |
| Accent | `colors.accent.primary` | `#58a6ff` | Primary interactive elements |
| Accent | `colors.accent.success` | `#3fb950` | Success states, positive changes |
| Text | `colors.text.primary` | `#f0f6fc` | Primary text |
| Text | `colors.text.secondary` | `#8b949e` | Secondary/muted text |

### Typography

- **Font Family**: System fonts (`-apple-system, BlinkMacSystemFont, 'Segoe UI'...`)
- **Sizes**: XS (12px), SM (14px), Base (16px), LG (18px), XL (20px), 2XL (24px)
- **Weights**: Normal (400), Medium (500), Semibold (600), Bold (700)

### Layout Breakpoints

| Breakpoint | Width | Layout |
|------------|-------|--------|
| Mobile | < 768px | Single column, bottom nav |
| Tablet | 768-1024px | Two panel, horizontal progress |
| Desktop | 1024px+ | Three panel, sidebar navigation |

## Comparison with Other Wireframe Formats

| Format | Location | Pros | Cons |
|--------|----------|------|------|
| HTML/CSS | `../html/` | Interactive, runs in browser | Requires development skills |
| Excalidraw | `../excalidraw/` | Editable, portable JSON | Less precise than Figma |
| **Figma Specs** | `../figma/` | Industry standard, detailed | Requires Figma to use |
| SVG | `../svg/` | Vector, scalable, universal | Static, no interactivity |

---

*Generated for the InfraSizing Calculator UX Wireframes project*
