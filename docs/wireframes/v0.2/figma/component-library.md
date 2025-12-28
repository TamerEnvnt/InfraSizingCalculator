# InfraSizing Calculator - Figma Component Library

This document defines all reusable components for the InfraSizing Calculator UI.
Use this specification to build the Figma component library.

---

## 1. Buttons

### Primary Button
- **Background:** `#238636` (button.primary.bg)
- **Text:** `#ffffff` / 14px / font-weight: 500
- **Padding:** 12px 24px
- **Border Radius:** 8px (borderRadius.md)
- **Height:** 40px
- **Hover:** Background `#2ea043`
- **Active:** Background `#238636` with inner shadow

### Secondary Button
- **Background:** `#21262d` (bg.tertiary)
- **Border:** 1px solid `#30363d`
- **Text:** `#c9d1d9` / 14px / font-weight: 500
- **Padding:** 12px 24px
- **Border Radius:** 8px
- **Height:** 40px
- **Hover:** Border `#8b949e`

### Danger Button
- **Background:** transparent
- **Border:** 1px solid `#f85149`
- **Text:** `#f85149` / 14px / font-weight: 500
- **Padding:** 12px 24px
- **Border Radius:** 8px
- **Height:** 40px
- **Hover:** Background `rgba(248, 81, 73, 0.1)`

### Link Button
- **Background:** transparent
- **Text:** `#58a6ff` / 14px / font-weight: 500
- **Padding:** 0
- **Hover:** Text underline

---

## 2. Form Elements

### Text Input
- **Background:** `#0d1117` (bg.primary)
- **Border:** 1px solid `#30363d`
- **Border Radius:** 8px
- **Padding:** 12px 16px
- **Height:** 44px
- **Text:** `#c9d1d9` / 14px
- **Placeholder:** `#8b949e` / 14px
- **Focus:** Border `#58a6ff`

### Number Input
- Same as Text Input
- **Width:** 80px (compact) or 120px (normal)
- Includes increment/decrement controls

### Checkbox
- **Size:** 20px x 20px
- **Border:** 1px solid `#30363d`
- **Border Radius:** 4px
- **Background (unchecked):** `#21262d`
- **Background (checked):** `#58a6ff`
- **Checkmark:** `#ffffff`

### Radio Button
- **Size:** 20px x 20px
- **Border:** 1px solid `#30363d`
- **Background:** `#21262d`
- **Selected:** Border `#58a6ff`, inner dot `#ffffff` (8px)

### Toggle Switch
- **Track Size:** 44px x 24px
- **Track Off:** `#30363d`
- **Track On:** `#3fb950`
- **Knob Size:** 20px
- **Knob Color:** `#ffffff`
- **Border Radius:** 12px (full)

### Slider
- **Track Height:** 4px
- **Track Background:** `#30363d`
- **Track Fill:** `#58a6ff`
- **Knob Size:** 16px
- **Knob Color:** `#58a6ff`
- **Knob Border:** 2px solid `#ffffff`

### Select Dropdown
- Same styling as Text Input
- **Arrow:** Chevron icon `#8b949e`
- **Dropdown:** Background `#161b22`, border `#30363d`
- **Option Hover:** Background `#21262d`

---

## 3. Cards

### Base Card
- **Background:** `#161b22` (bg.secondary)
- **Border:** 1px solid `#30363d`
- **Border Radius:** 12px (borderRadius.lg)
- **Padding:** 24px
- **Shadow:** none (flat design)

### Selection Card
- Same as Base Card
- **Selected State:** Border `#58a6ff`, 2px
- **Hover:** Border `#8b949e`

### Summary Card (Metric)
- **Size:** 160px x 100px (flexible)
- **Background:** `#161b22`
- **Border:** 1px solid `#30363d`
- **Border Radius:** 12px
- **Content:**
  - Value: 32px / font-weight: 700 / `#c9d1d9`
  - Label: 12px / font-weight: 400 / `#8b949e`
  - Subtext: 11px / `#6e7681`

### Scenario Card
- **Size:** Full width or 560px min
- **Height:** 140px
- **Background:** `#161b22`
- **Border:** 1px solid `#30363d` (or `#f0883e` for starred)
- **Border Radius:** 12px
- **Content Layout:**
  - Title row with star indicator
  - Details row (technology, distribution, nodes, cost)
  - Date row (created, modified)
  - Action buttons row

---

## 4. Navigation

### Sidebar
- **Width:** 240px
- **Background:** `#161b22`
- **Border Right:** 1px solid `#30363d`

### Nav Item
- **Height:** 36px
- **Padding:** 0 16px
- **Text:** 14px / `#c9d1d9`
- **Icon Size:** 16px
- **Icon Gap:** 12px

### Nav Item Active
- **Background:** `#21262d`
- **Left Border:** 3px solid `#58a6ff`
- **Text Color:** `#58a6ff`

### Wizard Step (Sidebar)
- **Number Circle:** 24px diameter
- **Number Font:** 12px / font-weight: 600
- **States:**
  - Completed: Circle `#3fb950`, checkmark icon
  - Active: Circle `#58a6ff`, number
  - Pending: Circle `#30363d`, number `#6e7681`

### Breadcrumb (Tablet)
- **Height:** 48px
- **Step Indicator:** 24px circles connected by lines
- **Gap Between Steps:** 60px
- **Labels Below:** 11px / `#8b949e`

### Step Dots (Mobile)
- **Dot Size:** 8px
- **Gap:** 8px
- **Active Dot:** `#58a6ff`
- **Inactive Dot:** `#30363d`

---

## 5. Header

### Desktop Header
- **Height:** 64px
- **Background:** `#161b22`
- **Border Bottom:** 1px solid `#30363d`
- **Logo:** 20px / font-weight: 600 / `#58a6ff`
- **Icon Size:** 20px
- **Icon Color:** `#8b949e`

### Mobile Header
- **Height:** 56px
- **Hamburger Icon:** 24px
- **Title:** Truncated if needed

---

## 6. Tables

### Table Container
- **Background:** `#21262d`
- **Border:** 1px solid `#30363d`
- **Border Radius:** 8px

### Table Header Row
- **Background:** `#161b22`
- **Height:** 44px
- **Text:** 12px / font-weight: 500 / `#8b949e` / uppercase

### Table Body Row
- **Height:** 44px
- **Border Top:** 1px solid `#30363d`
- **Text:** 14px / `#c9d1d9`

### Table Row Hover
- **Background:** `#161b22`

---

## 7. Charts

### Bar Chart (Horizontal)
- **Bar Height:** 24px
- **Bar Radius:** 4px
- **Bar Gap:** 8px
- **Colors:** Use accent colors (blue, green, orange, purple)
- **Label:** Right-aligned, 14px

### Line Chart
- **Line Weight:** 2px
- **Point Size:** 8px
- **Grid Lines:** 1px / `#30363d` / dashed
- **Axis Labels:** 12px / `#8b949e`

### Comparison Table
- **Header Row:** `#21262d`
- **Data Rows:** Alternating `#161b22` and `#0d1117`
- **Highlight Cell:** Text `#3fb950` for savings

---

## 8. Dialogs & Modals

### Modal Overlay
- **Background:** `rgba(0, 0, 0, 0.8)`

### Modal Container
- **Background:** `#161b22`
- **Border:** 1px solid `#30363d`
- **Border Radius:** 12px
- **Shadow:** 0 10px 20px rgba(0, 0, 0, 0.4)
- **Max Width:** 600px (small), 900px (medium), 1200px (large)

### Modal Header
- **Height:** 56px
- **Padding:** 16px 24px
- **Border Bottom:** 1px solid `#30363d`
- **Title:** 18px / font-weight: 500

### Modal Body
- **Padding:** 24px

### Modal Footer
- **Height:** 68px
- **Padding:** 16px 24px
- **Border Top:** 1px solid `#30363d`
- **Buttons:** Right-aligned, 12px gap

---

## 9. Status & Feedback

### Badge
- **Padding:** 4px 10px
- **Border Radius:** 12px (full)
- **Font:** 11px / font-weight: 500 / uppercase
- **Variants:**
  - Default: `#21262d` bg, `#8b949e` text
  - Success: `#238636` bg, `#3fb950` text
  - Warning: `#9e6a03` bg, `#f0883e` text
  - Error: `#8b0000` bg, `#f85149` text
  - Info: `#1f6feb` bg, `#58a6ff` text

### Info Tip Box
- **Background:** `rgba(88, 166, 255, 0.1)`
- **Border Left:** 3px solid `#58a6ff`
- **Padding:** 16px
- **Border Radius:** 0 8px 8px 0
- **Text:** 14px / `#c9d1d9`

### Recommendation Box
- **Background:** `rgba(63, 185, 80, 0.1)`
- **Border Left:** 3px solid `#3fb950`
- **Icon:** Lightbulb or checkmark
- **Text:** 14px / `#c9d1d9`

### Warning Box
- **Background:** `rgba(240, 136, 62, 0.1)`
- **Border Left:** 3px solid `#f0883e`
- **Text:** 14px / `#c9d1d9`

---

## 10. Icons

Use a consistent icon set. Recommended: **Heroicons** or **Lucide Icons**

### Required Icons
- Platform: `server` / `code`
- Technology: Various tech logos
- Navigation: `menu`, `chevron-right`, `x`
- Actions: `plus`, `edit`, `trash`, `copy`, `download`
- Status: `check`, `x`, `warning`, `info`
- UI: `sun`, `moon`, `settings`, `user`
- Chart: `bar-chart`, `line-chart`, `trending-up`

### Icon Sizes
- Small: 16px
- Medium: 20px
- Large: 24px

### Icon Colors
- Default: `#8b949e`
- Active: `#58a6ff`
- Success: `#3fb950`
- Error: `#f85149`
