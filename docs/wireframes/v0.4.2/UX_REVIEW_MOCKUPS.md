# UX States Review - v0.4 Wireframes

**Status:** REQUIRES REVIEW
**Created:** December 29, 2024
**Visual Mockup:** [13-ux-states-review.html](html/13-ux-states-review.html)

---

## Overview

This document catalogs UX states that need visual review before implementation. Each state should be evaluated for:
- **Clarity**: Is the message/feedback clear to users?
- **Accessibility**: Does it meet WCAG 2.1 AA standards?
- **Consistency**: Does it match the design system?
- **Recovery**: Are there clear paths to resolve/dismiss?

---

## 1. Error States

### 1.1 API Failure Banner
**Priority:** HIGH
**Screen:** Dashboard

| Aspect | Implementation |
|--------|----------------|
| Trigger | Calculation service unavailable or timeout |
| Placement | Below header, above summary cards |
| Style | Red gradient background, non-modal banner |
| Actions | "Retry Now" (primary), "Dismiss" (secondary) |
| Behavior | Auto-retry after 30 seconds, max 3 attempts |

**Review Questions:**
- [ ] Is "Unable to calculate sizing" clear enough?
- [ ] Should we show the last-known-good results?
- [ ] Is the retry delay appropriate (30s)?

---

### 1.2 Config Panel Validation Errors
**Priority:** HIGH
**Screen:** Any config panel

| Aspect | Implementation |
|--------|----------------|
| Trigger | Invalid field values on blur or submit |
| Placement | Summary at panel top, inline per field |
| Style | Red border on invalid fields, error icon |
| Actions | Fix errors to enable "Apply" button |
| Behavior | Real-time validation on change |

**Review Questions:**
- [ ] Is the validation summary at top necessary?
- [ ] Should inline errors appear immediately or on blur?
- [ ] Is red the right color for error states (colorblind)?

---

### 1.3 Session Timeout Warning
**Priority:** MEDIUM
**Screen:** All authenticated pages

| Aspect | Implementation |
|--------|----------------|
| Trigger | 5 minutes before session expiry |
| Placement | Centered modal with overlay |
| Style | Orange accent, countdown timer |
| Actions | "Stay Signed In" (primary), "Sign Out" (secondary) |
| Behavior | Auto-logout when countdown reaches 0 |

**Review Questions:**
- [ ] Is 5 minutes enough warning?
- [ ] Should we save unsaved changes before warning?
- [ ] Is the countdown anxiety-inducing?

---

### 1.4 Empty Scenarios Page
**Priority:** MEDIUM
**Screen:** Scenarios page

| Aspect | Implementation |
|--------|----------------|
| Trigger | User has no saved scenarios |
| Placement | Center of content area |
| Style | Illustration + CTA button |
| Actions | "Create New Scenario" button |
| Behavior | Navigate to dashboard on click |

**Review Questions:**
- [ ] Is the illustration appropriate?
- [ ] Should we show sample scenarios instead?
- [ ] Is one CTA enough?

---

### 1.5 Export Failed Modal
**Priority:** MEDIUM
**Screen:** Export modal

| Aspect | Implementation |
|--------|----------------|
| Trigger | PDF/Excel generation fails |
| Placement | Replace export modal content |
| Style | Red error icon, error code visible |
| Actions | "Try Again" (primary), "Cancel" (secondary) |
| Behavior | Log error for support |

**Review Questions:**
- [ ] Should we show error code to users?
- [ ] Is "Try Again" appropriate for all failures?
- [ ] Should we suggest alternative formats?

---

## 2. Loading States

### 2.1 Skeleton Summary Cards
**Priority:** HIGH
**Screen:** Dashboard initial load

| Aspect | Implementation |
|--------|----------------|
| Trigger | Initial page load, before data arrives |
| Style | Gray skeleton with shimmer animation |
| Duration | Until first calculation completes (< 2s) |
| Behavior | Match exact card dimensions |

**Review Questions:**
- [ ] Is shimmer animation too distracting?
- [ ] Should skeleton show for < 500ms loads?
- [ ] Are dimensions correct?

---

### 2.2 Skeleton Panel Content
**Priority:** MEDIUM
**Screen:** Config panels

| Aspect | Implementation |
|--------|----------------|
| Trigger | Panel opens before content loads |
| Style | Skeleton tabs + form fields |
| Duration | Until panel data loads (< 500ms typical) |
| Behavior | Tabs skeleton first, then content |

**Review Questions:**
- [ ] Do we need skeleton for fast-loading panels?
- [ ] Should existing values persist during reload?

---

### 2.3 Chart Loading Placeholder
**Priority:** MEDIUM
**Screen:** Dashboard visualizations

| Aspect | Implementation |
|--------|----------------|
| Trigger | Lazy-loaded charts not yet rendered |
| Style | Spinner + "Loading chart data..." text |
| Duration | Until chart renders (< 1s typical) |
| Behavior | Fixed height to prevent layout shift |

**Review Questions:**
- [ ] Should we use skeleton instead of spinner?
- [ ] Is the loading text necessary?
- [ ] Should charts be eager-loaded instead?

---

### 2.4 Calculating State
**Priority:** HIGH
**Screen:** Summary cards during recalculation

| Aspect | Implementation |
|--------|----------------|
| Trigger | Config change triggers recalculation |
| Style | Pulse animation on cards, "--" for pending values |
| Duration | Until calculation completes (< 500ms) |
| Behavior | Dots animation shows processing |

**Review Questions:**
- [ ] Should we show old values or "--" during calc?
- [ ] Is pulse animation noticeable enough?
- [ ] Should we debounce to avoid constant recalcs?

---

## 3. Mobile Adaptations (< 768px)

### 3.1 Mobile Summary Cards (2x3 Grid)
**Priority:** HIGH
**Screen:** Dashboard

| Desktop | Mobile |
|---------|--------|
| 5 columns | 2 columns + 1 full-width |
| 20px padding | 12px padding |
| 36px values | 24px values |

**Review Questions:**
- [ ] Is the cost card spanning full width correct?
- [ ] Should we hide any cards on mobile?
- [ ] Are touch targets large enough (48px minimum)?

---

### 3.2 Full-Screen Config Panel
**Priority:** HIGH
**Screen:** Any config panel

| Desktop | Mobile |
|---------|--------|
| 480px slide-out | 100% full-screen |
| Overlay click to close | Back button to close |
| Horizontal tabs | Horizontally scrollable tabs |

**Review Questions:**
- [ ] Is full-screen the right pattern?
- [ ] Should tabs collapse into a dropdown?
- [ ] Is the back button placement correct?

---

### 3.3 FAB Action Menu
**Priority:** MEDIUM
**Screen:** Dashboard

| Desktop | Mobile |
|---------|--------|
| Fixed bottom action bar | FAB + expandable menu |
| 4 visible buttons | 1 FAB, 4 menu items |

**Review Questions:**
- [ ] Is FAB the right mobile pattern?
- [ ] Should we use a bottom sheet instead?
- [ ] Are labels necessary or icons sufficient?

---

### 3.4 Mobile Action Bar (Alternative)
**Priority:** LOW
**Screen:** Dashboard

Alternative to FAB: fixed bottom navigation bar.

| Aspect | Implementation |
|--------|----------------|
| Style | Icon + label for each action |
| Height | 64px fixed |
| Actions | Save, Compare, Export, Settings |

**Review Questions:**
- [ ] FAB vs Bottom Bar - which is better?
- [ ] Should Export expand to sub-options?

---

## 4. Micro-Interactions

### 4.1 Card Hover Effect
**Priority:** LOW
**Trigger:** Mouse hover on summary card

| Property | Value |
|----------|-------|
| Transform | translateY(-4px) |
| Box Shadow | 0 8px 24px rgba(0,0,0,0.3) |
| Border Color | accent-blue |
| Duration | 0.2s ease |

**Review Questions:**
- [ ] Is 4px lift too subtle?
- [ ] Should we add shadow on mobile (no hover)?

---

### 4.2 Tab Underline Animation
**Priority:** LOW
**Trigger:** Tab selection change

| Property | Value |
|----------|-------|
| Underline | 2px accent-blue |
| Animation | scaleX(0) to scaleX(1) |
| Duration | 0.3s ease |

**Review Questions:**
- [ ] Should underline slide or scale?
- [ ] Is 0.3s too slow?

---

### 4.3 Save Success Feedback
**Priority:** MEDIUM
**Trigger:** Scenario saved successfully

| Property | Value |
|----------|-------|
| Icon | Check mark in green circle |
| Animation | Scale 0 -> 1.2 -> 1 (pop) |
| Duration | 0.3s for animation, 3s display |
| Placement | Toast at top-right or inline |

**Review Questions:**
- [ ] Toast vs inline feedback?
- [ ] Is 3s enough to notice?
- [ ] Should we add a sound?

---

### 4.4 Export Progress Button
**Priority:** MEDIUM
**Trigger:** Export in progress

| Property | Value |
|----------|-------|
| Progress Bar | Fills button from left |
| Text | "Exporting... XX%" |
| Duration | Matches actual progress |

**Review Questions:**
- [ ] Should progress be determinate or indeterminate?
- [ ] Keep button disabled during export?

---

## Review Checklist

Before approving for implementation:

### Error States
- [ ] All error messages are clear and actionable
- [ ] Recovery paths exist for all errors
- [ ] Errors don't block unrelated functionality
- [ ] Error states are accessible (not red-only)

### Loading States
- [ ] Skeletons match actual content dimensions
- [ ] Loading times are acceptable (< 2s)
- [ ] No layout shift when content loads
- [ ] Loading indicators are not jarring

### Mobile Adaptations
- [ ] Touch targets are 48px minimum
- [ ] Text is readable without zooming
- [ ] Interactions work with touch
- [ ] Critical actions are accessible

### Micro-Interactions
- [ ] Animations respect `prefers-reduced-motion`
- [ ] Duration is appropriate (< 0.3s for most)
- [ ] Animations enhance, don't distract
- [ ] Consistent timing across similar elements

---

## Implementation Notes

Once approved, implement in this order:

1. **HIGH Priority** - Block launch without these
2. **MEDIUM Priority** - Implement before v1.0
3. **LOW Priority** - Nice-to-have, can defer

Reference the visual mockup at [13-ux-states-review.html](html/13-ux-states-review.html) for exact styling.
