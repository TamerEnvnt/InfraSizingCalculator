# Infrastructure Sizing Calculator - Claude Code Instructions

This file is automatically read at the start of every Claude Code session.

---

## Project Overview

- **Type**: Blazor Server application (.NET 10.0)
- **Purpose**: Calculate infrastructure sizing for Kubernetes (46 distributions) and VM deployments
- **Tech Stack**: Blazor Server, Entity Framework Core, SQLite, ClosedXML

---

## Critical: Documentation Sync Requirements

**The Golden Rule**: If you change code behavior, update docs in the SAME commit.

### Before Any Code Change

1. Check `docs/process/CHANGE_IMPACT_MATRIX.md` to identify required doc updates
2. Note which docs need updating BEFORE you start coding
3. Update docs as part of the same task, not as a follow-up

### After Any Code Change

Before committing, verify:
- [ ] Enum changes → `docs/technical/models.md` updated
- [ ] Service changes → `docs/technical/services.md` updated
- [ ] Component changes → `docs/technical/ui-components.md` updated
- [ ] Logic changes → `docs/business/business-rules.md` updated
- [ ] Counts updated (distributions, services, etc.) in ALL locations

### Count Locations to Update

When adding/removing items, search and update:
```bash
grep -rn "[0-9]* distribution" docs/    # Distribution counts
grep -rn "[0-9]* service" docs/          # Service counts
```

---

## Session Start Checklist

At the start of each session:

1. **Check git status** for uncommitted changes from previous sessions
2. **Review recent commits** to understand current state
3. **Identify current phase** of any active feature work
4. **Check for doc drift** if resuming work on a feature

```bash
git status
git log --oneline -5
```

---

## Team Workflow (Multiple Contributors)

When working in a team with other developers, AI assistants, BAs, or testers:

### The Correct Order

```
1. BA writes spec (INTENT)     → Docs are DRAFT
2. Dev implements (REALITY)    → CODE becomes source of truth
3. SYNC GATE: Docs updated     → Docs match CODE
4. Tester tests                → Tests verify CODE matches DOCS
```

### Critical Rules

1. **CODE is source of truth** after implementation begins
2. **Don't update docs during active implementation** - wait for dev to finish
3. **Don't write tests until docs are synced** - tests should verify documented behavior
4. **Never skip the sync gate** - it prevents drift

### Ask Before Acting

If joining mid-feature, ask:
- "Is implementation complete?"
- "Have docs been synced to code?"
- "Can I proceed with [docs/tests]?"

### Phase Detection

| If you see... | You're in phase... | Your action... |
|---------------|-------------------|----------------|
| Draft spec, no code | 1. Specification | Wait or help spec |
| Active code changes | 2. Implementation | Wait for completion |
| Code done, old docs | 3. Sync needed | Update docs NOW |
| Synced docs, no tests | 4. Testing | Write tests |

See `docs/process/TEAM_WORKFLOW.md` for full team coordination details.

---

## Code Organization

### Key Directories

```
src/InfraSizingCalculator/
├── Components/           # Blazor UI components
│   ├── Pages/           # Routable pages (Home.razor, Scenarios.razor)
│   ├── Configuration/   # Config panels (AppCountsPanel, NodeSpecsPanel)
│   ├── Results/         # Result views (SizingResultsView, CostAnalysisView)
│   ├── K8s/             # Kubernetes-specific components
│   ├── VM/              # VM-specific components
│   ├── Pricing/         # Pricing components
│   └── Wizard/          # Wizard framework
├── Services/            # Business logic (16 services)
├── Models/              # Data models and enums
│   └── Enums/           # 11 enumeration types
└── Controllers/Api/     # REST API endpoints
```

### Key Files

| Purpose | File |
|---------|------|
| Main entry | `Program.cs` |
| Main UI | `Components/Pages/Home.razor` |
| K8s sizing | `Services/K8sSizingService.cs` |
| VM sizing | `Services/VMSizingService.cs` |
| Distributions | `Services/DistributionService.cs` (46 distributions) |
| Technologies | `Services/TechnologyService.cs` (7 technologies) |

---

## Business Rules

Business rules are documented in `docs/business/business-rules.md` with IDs:

| Prefix | Category |
|--------|----------|
| BR-M | Master nodes |
| BR-I | Infrastructure nodes |
| BR-W | Worker nodes |
| BR-H | Headroom |
| BR-R | Replicas |
| BR-T | Technologies |
| BR-D | Distributions |
| BR-C | Cost estimation |
| BR-S | Scenarios |
| BR-G | Growth planning |

When changing calculation logic, update the relevant BR-* rules.

---

## Testing Requirements

- **Unit tests**: `tests/InfraSizingCalculator.UnitTests/`
- **E2E tests**: `tests/InfraSizingCalculator.E2ETests/`
- **Framework**: xUnit + bUnit for Blazor components

Run tests before committing:
```bash
dotnet test
```

---

## Commit Guidelines (Phase-Based)

### MANDATORY: Use Phase Tags

Every commit MUST include a phase tag. This is how we track sync status.

### Format
```
[PHASE] type: short description

Body explaining what and why.

Phase: SPEC|IMPL|SYNC|TEST
Feature: feature-name
Docs-Updated: yes|no|n/a
Tests-Updated: yes|no|n/a
```

### Phase Tags

| Tag | When to Use | Docs-Updated | Tests-Updated |
|-----|-------------|--------------|---------------|
| `[SPEC]` | Writing requirements | yes | n/a |
| `[IMPL]` | Writing code | **no** (SYNC later) | **no** (TEST later) |
| `[SYNC]` | Updating docs to match code | **yes** (required) | n/a |
| `[TEST]` | Writing tests | n/a | yes |

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `refactor`: Code restructuring
- `test`: Test changes
- `chore`: Maintenance

### Examples

**Implementation commit (don't update docs yet):**
```
[IMPL] feat: add GrowthPlanningService

Added service for calculating growth projections.
Docs will be updated in SYNC phase.

Phase: IMPL
Feature: growth-planning
Docs-Updated: no
Tests-Updated: no
```

**Sync commit (update all docs):**
```
[SYNC] docs: sync docs for GrowthPlanningService

Updated documentation to match implementation.
- services.md: Added GrowthPlanningService section
- business-rules.md: Added BR-G* rules
- solution-overview.md: Updated service count

Phase: SYNC
Feature: growth-planning
Docs-Updated: yes
Tests-Updated: n/a
```

### Critical Rules

1. **[IMPL] commits**: Set `Docs-Updated: no` - docs sync happens later
2. **[SYNC] commits**: Must update ALL affected docs before testing
3. **[TEST] commits**: Only after SYNC phase is complete
4. **Never mix phases**: Don't put IMPL and SYNC changes in same commit

---

## Common Tasks Reference

### Adding a New Distribution

1. Add to `Models/Enums/Distribution.cs`
2. Add config to `Services/DistributionService.cs`
3. Update `docs/technical/models.md` (Distribution enum)
4. Update distribution counts in:
   - `docs/architecture/solution-overview.md`
   - `docs/business/BRD.md`
5. Add tests to `DistributionServiceTests.cs`

### Adding a New Service

1. Create `Services/NewService.cs`
2. Register in `Program.cs`
3. Update `docs/technical/services.md` (full documentation)
4. Update service count in `docs/architecture/solution-overview.md`
5. Add business rules to `docs/business/business-rules.md` if applicable
6. Create `tests/.../NewServiceTests.cs`

### Adding a New Component

1. Create component in appropriate folder
2. Update `docs/technical/ui-components.md`
3. Add bUnit tests if component has logic

---

## Documentation Structure

```
docs/
├── process/              # THIS IS IMPORTANT - sync workflow
│   ├── SYNC_WORKFLOW.md
│   └── CHANGE_IMPACT_MATRIX.md
├── architecture/         # High-level design
├── business/             # BRD, business rules
├── technical/            # Models, services, API, components
├── srs/                  # Software Requirements Specification
└── testing/              # Test documentation
```

---

## Reminders

1. **Don't defer documentation** - Update it now, not "later"
2. **Check counts** - When adding items, search for hardcoded counts
3. **Verify before commit** - Run build and tests
4. **Atomic commits** - Code + docs + tests together
5. **Use the matrix** - `docs/process/CHANGE_IMPACT_MATRIX.md` is your friend
