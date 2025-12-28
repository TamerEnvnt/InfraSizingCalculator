# Wireframe Specification v0.3 - Progressive Accordion Pattern

**Version:** 0.3
**Pattern:** Progressive Accordion / Single-Page Form
**Date:** December 28, 2024

---

## Design Philosophy

Based on UX research from [Nielsen Norman Group](https://www.nngroup.com/articles/progressive-disclosure/) and [PatternFly](https://medium.com/patternfly/comparing-web-forms-a-progressive-form-vs-a-wizard-110eefc584e7), this version uses a **progressive form** pattern where all configuration steps are visible on a single scrollable page with accordion sections.

### Key Differences from v0.2 (Wizard)

| Aspect | v0.2 Wizard | v0.3 Accordion |
|--------|-------------|----------------|
| Navigation | Step-by-step pages | Single scrollable page |
| Visibility | One step at a time | All steps visible |
| Editing | Navigate to step | Click to expand |
| Layout | 3-panel with sidebar | Full-width centered |
| Progress | Sidebar steps | Top progress bar |
| Summary | Side context panel | Floating bottom bar |

### Benefits of Progressive Accordion

1. **Better for editing** - Users can jump to any section
2. **Full context** - See all sections at once
3. **Less navigation** - No back/next clicking
4. **Faster completion** - Research shows users prefer this

---

## Design Tokens

Same as v0.2 for consistency:

### Colors
| Token | Value | Usage |
|-------|-------|-------|
| bg-primary | `#0d1117` | Main background |
| bg-secondary | `#161b22` | Cards, sections |
| bg-tertiary | `#21262d` | Hover, inputs |
| border-primary | `#30363d` | Borders |
| text-primary | `#c9d1d9` | Main text |
| text-secondary | `#8b949e` | Labels |
| accent-blue | `#58a6ff` | Active, links |
| accent-green | `#3fb950` | Complete, success |

### Typography
- Font: `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`
- Same scale as v0.2

---

## Layout Structure

### Desktop (1440x900)

```
┌────────────────────────────────────────────────────────────────┐
│                        HEADER (64px)                           │
├────────────────────────────────────────────────────────────────┤
│              PROGRESS BAR (48px) - 5 steps                     │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│    ┌──────────────────────────────────────────────────────┐   │
│    │  [▼] 1. Platform Selection              ✓ Complete   │   │
│    │      (collapsed content)                              │   │
│    └──────────────────────────────────────────────────────┘   │
│                                                                │
│    ┌──────────────────────────────────────────────────────┐   │
│    │  [▼] 2. Technology Selection            ✓ Complete   │   │
│    │      (collapsed content)                              │   │
│    └──────────────────────────────────────────────────────┘   │
│                                                                │
│    ┌──────────────────────────────────────────────────────┐   │
│    │  [▲] 3. Distribution Selection          ● Current    │   │
│    │  ┌────────────────────────────────────────────────┐  │   │
│    │  │                                                │  │   │
│    │  │  (expanded content - grid of distribution     │  │   │
│    │  │   cards to select from)                       │  │   │
│    │  │                                                │  │   │
│    │  └────────────────────────────────────────────────┘  │   │
│    └──────────────────────────────────────────────────────┘   │
│                                                                │
│    ┌──────────────────────────────────────────────────────┐   │
│    │  [▶] 4. Applications Config             ○ Pending    │   │
│    └──────────────────────────────────────────────────────┘   │
│                                                                │
│    ┌──────────────────────────────────────────────────────┐   │
│    │  [▶] 5. Pricing Options                 ○ Pending    │   │
│    └──────────────────────────────────────────────────────┘   │
│                                                                │
├────────────────────────────────────────────────────────────────┤
│  FLOATING SUMMARY BAR (64px)                                   │
│  12 Nodes | 96 vCPU | 384 GB | $4,200/mo    [Calculate →]     │
└────────────────────────────────────────────────────────────────┘
```

---

## Screen Specifications

### Header (64px)
- Logo left: "InfraSizing Calculator"
- Right icons: Theme, Settings, User
- Same styling as v0.2

### Progress Bar (48px)
- Horizontal step indicators
- 5 connected circles with labels
- Shows completion status
- Clicking step scrolls to section

### Accordion Sections

Each section has:
- **Header Row:** Chevron + Number + Title + Status badge
- **Content Area:** Expands/collapses on click
- **Transition:** Smooth 300ms animation

**Section States:**
| State | Chevron | Badge | Header BG |
|-------|---------|-------|-----------|
| Completed | ▼ | ✓ Green | `#161b22` |
| Current | ▲ | ● Blue | `#21262d` |
| Pending | ▶ | ○ Gray | `#161b22` |

### Floating Summary Bar (64px)
- Fixed at bottom
- Semi-transparent backdrop blur
- Shows live calculations
- Primary CTA: "Calculate Results →"

---

## Screens to Create

### 1. Main Configuration Page
- All 5 accordion sections
- One section expanded (current)
- Others collapsed showing summary
- Floating bar at bottom

### 2. Results View
- Full-width results display
- Summary cards at top
- Tabbed content (Overview, Nodes, Cost)
- Back to config button

### 3. Comparison View
- Side-by-side scenario comparison
- Diff highlighting
- Export options

---

## Responsive Behavior

### Tablet (1024px)
- Same layout, narrower content
- Accordion works the same
- Floating bar adapts

### Mobile (375px)
- Full-width accordions
- Sections stack vertically
- Floating bar shows minimal info
- Tap to expand summary sheet

---

## Interactions

1. **Section Click:** Expands/collapses with animation
2. **Progress Click:** Scrolls to section, expands it
3. **Auto-collapse:** Optional - collapse previous when opening new
4. **Validation:** Section shows error state if incomplete
5. **Live Update:** Summary bar updates as user configures

---

## Component Differences from v0.2

| Component | v0.2 | v0.3 |
|-----------|------|------|
| Sidebar | Full navigation | Removed |
| Context Panel | Right panel | Floating bar |
| Wizard Steps | Vertical list | Horizontal progress |
| Content Area | Fixed height | Scrollable |
| Navigation | Back/Next buttons | Accordion headers |
