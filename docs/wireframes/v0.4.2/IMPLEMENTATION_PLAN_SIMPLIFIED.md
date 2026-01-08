# v0.4.2 Implementation Plan - DETAILED TASK LIST

**IMPORTANT FOR FUTURE SESSIONS**: This is the source of truth. Read this file completely before doing any work.

---

## FLOW DEFINITION (DO NOT CHANGE)

### Guest Flow
```
00-landing-guest.html → 01-results-guest.html (with save banner + config panels)
```
- Guest sees landing with ONE card: "Create New Scenario"
- Clicking it goes to results page with save prompts
- Limited export (PDF only, watermarked)

### Authenticated Flow
```
11-login.html → 00-landing.html → 01-dashboard.html (with config panels)
```
- User logs in
- Sees landing with TWO cards: "Load Scenario" + "Create New Scenario"
- Plus "Recent Scenarios" section
- Selecting/creating goes to dashboard with full features

### KEY DISTINCTION
- `00-landing.html` = Entry point AFTER login (shows scenario choices)
- `01-dashboard.html` = Shown AFTER user selects or creates a scenario (shows sizing results)

---

## WIREFRAME FILE MAPPING

| Wireframe File | Purpose | Blazor Component |
|----------------|---------|------------------|
| `00-landing-guest.html` | Guest entry point | `Pages/LandingGuest.razor` (NEW) |
| `00-landing.html` | Auth entry point (after login) | `Pages/LandingAuth.razor` (NEW) |
| `01-results-guest.html` | Guest results with save banner | `Pages/Home.razor` (modify for guest) |
| `01-dashboard.html` | Auth results (full features) | `Pages/Home.razor` (modify for auth) |
| `02-dashboard-with-panel.html` | Config panel open | Part of Home.razor |
| `08-scenarios.html` | Saved scenarios list | `Pages/Scenarios.razor` (modify) |
| `10-settings.html` | User settings | `Pages/Settings.razor` (NEW) |
| `11-login.html` | Login form | `Pages/Login.razor` (exists, fix) |
| `12-register.html` | Register form | `Pages/Register.razor` (exists, fix) |

---

## DETAILED TASK LIST

### PHASE 1: AUTHENTICATION (Must Work First)

#### Task 1.1: Verify AuthService is functional
- **File**: `src/InfraSizingCalculator/Services/Auth/AuthService.cs`
- **Check**: `RegisterAsync()` creates user in database
- **Check**: `LoginAsync()` validates credentials and creates session
- **Check**: `GetCurrentUserAsync()` returns logged-in user
- **Check**: `LogoutAsync()` ends session
- **Test**: Run app, try to register, check if user appears in identity.db

#### Task 1.2: Fix Login.razor to use AuthService
- **File**: `src/InfraSizingCalculator/Components/Pages/Login.razor`
- **Wireframe**: `docs/wireframes/v0.4.2/html/11-login.html`
- **Steps**:
  1. Ensure `HandleLogin()` calls `AuthService.LoginAsync()`
  2. On success, redirect to `/` (which will show LandingAuth)
  3. On failure, show error message from wireframe design
  4. Match CSS styling from `11-login.html`

#### Task 1.3: Fix Register.razor to use AuthService
- **File**: `src/InfraSizingCalculator/Components/Pages/Register.razor`
- **Wireframe**: `docs/wireframes/v0.4.2/html/12-register.html`
- **Steps**:
  1. Ensure `HandleRegister()` calls `AuthService.RegisterAsync()`
  2. On success, redirect to `/login`
  3. On failure, show error message
  4. Match CSS styling from `12-register.html`

#### Task 1.4: Test authentication end-to-end
- **Test Steps**:
  1. Go to `/register`, create account
  2. Go to `/login`, login with new account
  3. Verify user is authenticated
  4. Logout and verify session ends

---

### PHASE 2: LANDING PAGES

#### Task 2.1: Create Guest Landing Page
- **File to Create**: `src/InfraSizingCalculator/Components/Pages/LandingGuest.razor`
- **File to Create**: `src/InfraSizingCalculator/Components/Pages/LandingGuest.razor.css`
- **Wireframe**: `docs/wireframes/v0.4.2/html/00-landing-guest.html`
- **Elements to include**:
  1. Header with logo + "Sign In" / "Sign Up" buttons
  2. Hero section with title: "Infrastructure Sizing Calculator"
  3. Single action card: "Create New Scenario"
  4. Login prompt box: "Want to save your scenarios? Sign in..."
  5. Features list at bottom (46 K8s Distributions, 7 Low-Code Platforms, Cost Estimation)
  6. Footer with links
- **Route**: `@page "/landing-guest"` (will be shown from Home.razor)
- **Event**: `OnStartScenario` callback when user clicks "Create New Scenario"

#### Task 2.2: Create Auth Landing Page
- **File to Create**: `src/InfraSizingCalculator/Components/Pages/LandingAuth.razor`
- **File to Create**: `src/InfraSizingCalculator/Components/Pages/LandingAuth.razor.css`
- **Wireframe**: `docs/wireframes/v0.4.2/html/00-landing.html`
- **Elements to include**:
  1. Header with logo + user avatar + settings icon
  2. Hero section with title
  3. TWO action cards: "Load Scenario" + "Create New Scenario"
  4. "Recent Scenarios" section with 3 scenario cards
  5. Footer with links
- **Route**: `@page "/landing"` (will be shown from Home.razor)
- **Events**:
  - `OnLoadScenario` → navigate to Scenarios page
  - `OnCreateNew` → navigate to Dashboard with empty scenario

---

### PHASE 3: ROUTING LOGIC

#### Task 3.1: Create App Entry Point Logic
- **File to Modify**: `src/InfraSizingCalculator/Components/Pages/Home.razor`
- **Logic to Implement**:
```
IF user is NOT authenticated:
    IF user has NOT started a scenario:
        SHOW LandingGuest component
    ELSE:
        SHOW GuestResults (current dashboard with save banner)
ELSE (user IS authenticated):
    IF user is on landing (no scenario selected):
        SHOW LandingAuth component
    ELSE:
        SHOW Dashboard (current dashboard without save banner)
```

#### Task 3.2: Add state tracking
- **File**: `src/InfraSizingCalculator/Services/AppStateService.cs`
- **Add properties**:
  - `bool HasStartedScenario` - true when guest clicks "Create New"
  - `Guid? CurrentScenarioId` - set when loading existing scenario
  - `bool IsViewingLanding` - true when on landing page

#### Task 3.3: Update navigation flow
- **Login success** → redirect to `/` → shows LandingAuth
- **Register success** → redirect to `/login`
- **Guest "Create New"** → sets HasStartedScenario=true, shows results
- **Auth "Create New"** → navigates to dashboard
- **Auth "Load Scenario"** → navigates to `/scenarios`

---

### PHASE 4: GUEST EXPERIENCE

#### Task 4.1: Create SaveBanner component
- **File to Create**: `src/InfraSizingCalculator/Components/Guest/SaveBanner.razor`
- **File to Create**: `src/InfraSizingCalculator/Components/Guest/SaveBanner.razor.css`
- **Wireframe**: See top of `01-results-guest.html`
- **Content**:
  - Icon + "Save your scenario"
  - Text: "Sign in to save, compare scenarios, and access your calculations later."
  - Two buttons: "Sign In to Save" + "Create Account"
- **Placement**: Top of results page for guest users

#### Task 4.2: Create LimitedExportModal component
- **File to Create**: `src/InfraSizingCalculator/Components/Guest/LimitedExportModal.razor`
- **Wireframe**: `docs/wireframes/v0.4.2/html/09-export-guest.html`
- **Features**:
  - PDF option enabled
  - Excel/JSON options disabled with "PRO" badge
  - Watermark warning text
  - "Create a free account for full export" link

#### Task 4.3: Integrate guest components into Home.razor
- When guest user is viewing results:
  - Show SaveBanner at top
  - Export button opens LimitedExportModal
  - Hide "Save Scenario" and "Compare" buttons

---

### PHASE 5: UPDATE EXISTING PAGES

#### Task 5.1: Update Scenarios page
- **File**: `src/InfraSizingCalculator/Components/Pages/Scenarios.razor`
- **Wireframe**: `docs/wireframes/v0.4.2/html/08-scenarios.html`
- **Changes**:
  - Grid layout with scenario cards
  - Each card shows: name, type badge (K8s/VM), last modified date
  - Actions: Load, Compare checkbox, Delete
  - Search/filter bar at top
  - "New Scenario" button

#### Task 5.2: Create Settings page
- **File to Create**: `src/InfraSizingCalculator/Components/Pages/Settings.razor`
- **File to Create**: `src/InfraSizingCalculator/Components/Pages/Settings.razor.css`
- **Wireframe**: `docs/wireframes/v0.4.2/html/10-settings.html`
- **Route**: `@page "/settings"`
- **Sections**:
  - Profile settings
  - Notification preferences
  - Theme/appearance
  - Account management

---

### PHASE 6: POLISH AND TESTING

#### Task 6.1: Test complete guest flow
1. Open app (not logged in) → see Guest Landing
2. Click "Create New Scenario" → see Results with SaveBanner
3. Configure sizing → results update
4. Click Export → see LimitedExportModal
5. Click "Sign In" → go to Login

#### Task 6.2: Test complete auth flow
1. Login → see Auth Landing
2. Click "Create New Scenario" → see Dashboard
3. Configure and save scenario
4. Go to Scenarios page → see saved scenario
5. Load scenario → see Dashboard with that scenario

#### Task 6.3: Visual comparison
- Open each wireframe HTML in browser
- Compare with running Blazor app
- Fix any styling differences

---

## CURRENT STATUS

**Last Updated**: December 30, 2024

| Task | Status | Notes |
|------|--------|-------|
| 1.1 Verify AuthService | NOT STARTED | |
| 1.2 Fix Login.razor | NOT STARTED | |
| 1.3 Fix Register.razor | NOT STARTED | |
| 1.4 Test auth E2E | NOT STARTED | |
| 2.1 Guest Landing | NOT STARTED | |
| 2.2 Auth Landing | NOT STARTED | |
| 3.1 Entry point logic | NOT STARTED | |
| 3.2 State tracking | NOT STARTED | |
| 3.3 Navigation flow | NOT STARTED | |
| 4.1 SaveBanner | NOT STARTED | |
| 4.2 LimitedExportModal | NOT STARTED | |
| 4.3 Integrate guest | NOT STARTED | |
| 5.1 Update Scenarios | NOT STARTED | |
| 5.2 Create Settings | NOT STARTED | |
| 6.1 Test guest flow | NOT STARTED | |
| 6.2 Test auth flow | NOT STARTED | |
| 6.3 Visual comparison | NOT STARTED | |

---

## NOTES FOR FUTURE SESSIONS

1. **DO NOT** skip to later tasks - complete in order
2. **DO NOT** modify the flow definition - it matches the wireframes
3. **ALWAYS** open the wireframe HTML to see exact styling
4. **TEST** each phase before moving to next
5. **UPDATE** the status table as tasks complete

---
