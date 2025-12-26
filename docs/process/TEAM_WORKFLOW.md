# Team Workflow & Synchronization

How multiple team members (human and AI) stay in sync when working in parallel.

---

## The Golden Rule

> **After implementation begins, CODE is the source of truth.**
>
> Documentation describes what code DOES, not what we WISH it did.

---

## Team Roles

| Role | Responsibility | Primary Artifacts |
|------|----------------|-------------------|
| **BA (Business Analyst)** | Requirements, business rules, specifications | BRD.md, business-rules.md, SRS chapters |
| **Architect** | Technical design, component structure | solution-overview.md, data-flow.md |
| **Developer** | Implementation | Source code (Services/, Components/, Models/) |
| **Tester** | Test coverage, verification | Unit tests, E2E tests |
| **Tech Writer** | Technical documentation | services.md, models.md, ui-components.md |

---

## The Correct Order of Operations

### Phase 1: SPECIFICATION (BA leads)

```
BA writes/updates:
├── docs/business/BRD.md (what we're building)
├── docs/business/business-rules.md (how it should work)
└── docs/srs/chapters/*.html (detailed requirements)

Status: DRAFT - This is INTENT, not REALITY yet
```

**Rules:**
- BA can work independently during this phase
- Specs are marked as "Draft" or "Proposed"
- Dev and Tester can READ but should not depend on specs yet

### Phase 2: IMPLEMENTATION (Developer leads)

```
Developer implements:
├── src/InfraSizingCalculator/Services/*.cs
├── src/InfraSizingCalculator/Components/*.razor
├── src/InfraSizingCalculator/Models/*.cs
└── May deviate from spec if technical constraints require it

Status: CODE becomes source of truth
```

**Rules:**
- Developer follows spec as closely as possible
- If deviation needed, Developer documents WHY in code comments
- Developer notifies BA of any deviations
- BA and Tech Writer should NOT update docs during this phase (they'll be wrong)

### Phase 3: SYNC GATE (BA + Tech Writer update docs)

```
After implementation complete, BEFORE testing:

BA updates (to match code):
├── docs/business/business-rules.md (actual rules implemented)
└── docs/srs/chapters/*.html (actual behavior)

Tech Writer updates (to match code):
├── docs/technical/services.md
├── docs/technical/models.md
├── docs/technical/ui-components.md
└── docs/architecture/solution-overview.md

Status: DOCS now match CODE (source of truth)
```

**Rules:**
- This is a BLOCKING gate - testing cannot start until docs are synced
- BA/Tech Writer read the ACTUAL CODE to update docs
- If code doesn't match original intent, discuss with Dev:
  - Option A: Update docs to match code (code is correct)
  - Option B: Dev fixes code to match spec (spec was correct)
- Mark docs as "Final" or "Verified" after sync

### Phase 4: TESTING (Tester leads)

```
Tester writes/runs tests against UPDATED docs:
├── tests/InfraSizingCalculator.UnitTests/
└── tests/InfraSizingCalculator.E2ETests/

Tests verify: Code matches Updated Documentation
```

**Rules:**
- Tester uses SYNCED docs as test oracle (expected behavior)
- If test fails, it's a bug in CODE (docs are now truth for expected behavior)
- Tester should NOT update docs - report issues to BA/Tech Writer

### Phase 5: RELEASE (All artifacts in sync)

```
Final state:
├── CODE      - Implementation (source of truth for "what it does")
├── DOCS      - Accurately describe code behavior
├── TESTS     - Verify code matches documented behavior
└── All three are IN SYNC
```

---

## Workflow Diagram

```
                    ┌─────────────────────────────────────────────────────┐
                    │                  FEATURE LIFECYCLE                   │
                    └─────────────────────────────────────────────────────┘
                                           │
         ┌─────────────────────────────────┼─────────────────────────────────┐
         │                                 │                                 │
         ▼                                 ▼                                 ▼
    ┌─────────┐                      ┌─────────┐                      ┌─────────┐
    │   BA    │                      │   DEV   │                      │ TESTER  │
    └────┬────┘                      └────┬────┘                      └────┬────┘
         │                                 │                                 │
    ┌────▼────┐                            │                                 │
    │ Write   │                            │                                 │
    │ Spec    │─────── Spec v1 ───────────▶│                                 │
    │ (Draft) │                            │                                 │
    └────┬────┘                            │                                 │
         │                                 │                                 │
         │ WAIT                       ┌────▼────┐                            │
         │ (don't update              │Implement│                            │
         │  during impl)              │ Code    │                            │
         │                            └────┬────┘                            │
         │                                 │                                 │
         │                                 │ "Done implementing"             │
         │                                 │                                 │
         │◀────── Deviation Report ────────┤                                 │
         │       (if any)                  │                                 │
         │                                 │                                 │
    ┌────▼────┐                            │                                 │
    │ SYNC    │◀─────── Read Code ─────────┤                                 │
    │ Docs to │                            │                                 │
    │ Code    │                            │                                 │
    └────┬────┘                            │                                 │
         │                                 │                                 │
         │────── Updated Docs (Final) ─────┼────────────────────────────────▶│
         │                                 │                                 │
         │                                 │                            ┌────▼────┐
         │                                 │                            │ Write   │
         │                                 │                            │ Tests   │
         │                                 │                            └────┬────┘
         │                                 │                                 │
         │                                 │◀─────── Bug Reports ────────────┤
         │                                 │                                 │
         │                            ┌────▼────┐                            │
         │                            │ Fix     │                            │
         │                            │ Bugs    │                            │
         │                            └────┬────┘                            │
         │                                 │                                 │
         │◀─────── If behavior changed ────┤                                 │
         │                                 │                                 │
    ┌────▼────┐                            │                                 │
    │ Update  │                            │                            ┌────▼────┐
    │ Docs    │─────── Final Docs ─────────┼───────────────────────────▶│ Verify  │
    │ (if     │                            │                            │ Tests   │
    │ needed) │                            │                            │ Pass    │
    └─────────┘                            │                            └────┬────┘
                                           │                                 │
                                           │                                 │
                              ═════════════╧═════════════════════════════════╧═══
                                              ALL IN SYNC - READY TO RELEASE
```

---

## Parallel Work Rules

### What CAN happen in parallel:

| Activity | Can parallel with |
|----------|-------------------|
| BA writing NEW feature spec | Dev implementing DIFFERENT feature |
| Tester testing Feature A | Dev implementing Feature B |
| Tech Writer documenting Feature A | BA speccing Feature B |

### What CANNOT happen in parallel:

| Activity | Must wait for |
|----------|---------------|
| BA updating spec for Feature A | Dev to finish implementing Feature A |
| Tester testing Feature A | Docs to be synced for Feature A |
| Tech Writer documenting Feature A | Dev to finish implementing Feature A |

### The Sync Gate

```
         Feature A                              Feature B
         ─────────                              ─────────
              │                                      │
    ┌─────────▼─────────┐              ┌─────────────▼─────────┐
    │ Dev implementing  │              │ BA writing spec       │
    │ Feature A         │              │ Feature B             │
    └─────────┬─────────┘              └───────────────────────┘
              │
              │ DONE
              │
    ══════════╪══════════  SYNC GATE FOR FEATURE A
              │
    ┌─────────▼─────────┐
    │ BA + Tech Writer  │◀── Must complete before testing
    │ sync docs to code │
    └─────────┬─────────┘
              │
    ┌─────────▼─────────┐
    │ Tester tests      │
    │ Feature A         │
    └───────────────────┘
```

---

## Branch Strategy for Team Sync

### Recommended: Feature Branches with Phase Commits

```
main ─────────────────────────────────────────────────────────────────────►
       │                                              ▲
       │                                              │ (merge after all phases)
       └──▶ feature/new-distribution ─────────────────┘
                │           │           │
                │           │           │
             [IMPL]      [SYNC]      [TEST]
             commit      commit      commit
             (Dev)       (BA/TW)     (Tester)
```

### Commit Order on Feature Branch

1. **[SPEC] commits**: Requirements/specs (if needed)
2. **[IMPL] commits**: Implementation code (Docs-Updated: no)
3. **[SYNC] commits**: BA/Tech Writer update docs (Docs-Updated: yes)
4. **[TEST] commits**: Tester adds tests
5. **Merge**: Only after all phases complete

### Branch Protection Rules

```yaml
# Suggested branch protection
require_sync_commit: true     # Must have [SYNC] commit before merge
require_tests: true           # Must have [TEST] commit
require_passing_tests: true   # Tests must pass
```

---

## Phase-Based Commit Format

### MANDATORY: Every Commit Must Have a Phase Tag

```
[PHASE] type: short description

Body explaining what and why.

Phase: SPEC|IMPL|SYNC|TEST
Feature: feature-name
Docs-Updated: yes|no|n/a
Tests-Updated: yes|no|n/a
```

### Git Template Setup

Configure git to use the commit template:

```bash
# Set up commit template for this repo
git config commit.template .gitmessage
```

### Phase-Specific Commit Rules

| Phase | Tag | Docs-Updated | Tests-Updated | Example |
|-------|-----|--------------|---------------|---------|
| Specification | `[SPEC]` | yes | n/a | `[SPEC] feat: define ACK requirements` |
| Implementation | `[IMPL]` | **no** | **no** | `[IMPL] feat: add ACK distribution` |
| Sync | `[SYNC]` | **yes** | n/a | `[SYNC] docs: sync ACK documentation` |
| Test | `[TEST]` | n/a | yes | `[TEST] test: add ACK tests` |

### Why Phase Tags Matter

Phase tags let us:
1. **Track sync status**: Filter commits by `[SYNC]` to see if docs are updated
2. **Enforce order**: PRs must have `[SYNC]` before `[TEST]`
3. **Identify gaps**: Missing `[SYNC]` commit = docs are out of sync
4. **Audit history**: See who did what in which phase

### Example Feature Branch History

```
$ git log --oneline feature/ack-distribution

a1b2c3d [TEST] test: add ACK distribution tests
9f8e7d6 [SYNC] docs: sync documentation for ACK
5c4b3a2 [IMPL] feat: implement ACK distribution
1d2e3f4 [SPEC] feat: define ACK distribution requirements
```

### Checking for Missing Sync

```bash
# List all IMPL commits without a following SYNC
git log --oneline --grep="\[IMPL\]" | head -5

# List all SYNC commits
git log --oneline --grep="\[SYNC\]" | head -5

# If IMPL count > SYNC count, docs may be out of sync
```

---

## Conflict Resolution

### Scenario: Spec says X, Code does Y

**Resolution Process:**

1. **Dev explains** why code does Y instead of X
2. **Decision point:**
   - If Y is better: BA updates spec to match code
   - If X is correct: Dev fixes code to match spec
3. **Document decision** in commit message or ADR (Architecture Decision Record)

### Scenario: Tests fail after doc sync

**Resolution:**

1. Tests are based on UPDATED docs (which match code)
2. If test fails, it's a CODE BUG
3. Dev fixes code, not Tester adjusting tests to match broken code

### Scenario: Multiple people editing same doc

**Prevention:**

1. Use doc ownership (see table below)
2. Communicate before editing shared docs
3. Use pull requests for doc changes too

---

## Document Ownership

| Document | Primary Owner | Can Edit | Reviews |
|----------|---------------|----------|---------|
| BRD.md | BA | BA | Architect, Dev |
| business-rules.md | BA | BA, Tech Writer | Dev |
| SRS chapters | BA | BA | All |
| solution-overview.md | Architect | Architect, Tech Writer | Dev |
| services.md | Tech Writer | Tech Writer, Dev | BA |
| models.md | Tech Writer | Tech Writer, Dev | - |
| ui-components.md | Tech Writer | Tech Writer, Dev | - |
| Test docs | Tester | Tester | Dev |

---

## Checklist: Before Declaring "Feature Complete"

```markdown
## Feature: [Name]

### Implementation (Dev)
- [ ] Code complete
- [ ] Code reviewed
- [ ] Dev notified BA of any spec deviations

### Documentation Sync (BA + Tech Writer)
- [ ] BA updated business-rules.md (if logic changed)
- [ ] BA updated SRS chapters (if requirements changed)
- [ ] Tech Writer updated services.md (if services changed)
- [ ] Tech Writer updated models.md (if models changed)
- [ ] Tech Writer updated ui-components.md (if UI changed)
- [ ] All counts updated (distributions, services, etc.)

### Testing (Tester)
- [ ] Unit tests written against SYNCED docs
- [ ] Tests pass
- [ ] Coverage acceptable

### Final
- [ ] All artifacts in sync
- [ ] Ready to merge
```

---

## AI Assistant Guidelines

When an AI assistant joins a session:

1. **Identify current phase** - Is feature in Spec, Impl, Sync, or Test phase?
2. **Respect the order** - Don't update docs during active implementation
3. **Wait for sync gate** - Don't write tests until docs are synced
4. **Ask if unclear** - "Is implementation complete? Can I sync docs now?"

### AI Role Assignments

| AI Role | Phase | Actions |
|---------|-------|---------|
| AI BA | Phase 1, 3 | Write specs, sync docs after impl |
| AI Dev | Phase 2 | Implement code, report deviations |
| AI Tester | Phase 4 | Write tests against synced docs |
| AI General | Any | Ask which phase, act accordingly |

---

## Summary: The Order

1. **BA** writes initial spec (intent)
2. **Dev** implements (reality)
3. **SYNC GATE**: BA + Tech Writer update docs to match code
4. **Tester** tests against synced docs
5. **Merge** when all in sync

**Never skip the sync gate.** It's the checkpoint that prevents drift.
