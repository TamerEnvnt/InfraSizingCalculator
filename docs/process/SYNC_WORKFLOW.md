# Synchronization Workflow

Step-by-step process for keeping code, docs, and tests in sync.

---

## Workflow Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  1. PLAN        │────▶│  2. IMPLEMENT   │────▶│  3. SYNC        │
│  Check impact   │     │  Write code     │     │  Update docs    │
│  matrix         │     │  Write tests    │     │  Update tests   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                                         │
                                                         ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  6. DONE        │◀────│  5. COMMIT      │◀────│  4. VERIFY      │
│  Task complete  │     │  Single commit  │     │  Run checklist  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

---

## Phase 1: PLAN

Before writing any code, check what documentation will need updating.

### 1.1 Identify Change Type

| Change Type | Example |
|-------------|---------|
| New feature | Adding growth projection charts |
| Bug fix | Fixing calculation error |
| Refactor | Renaming service methods |
| New enum value | Adding new Distribution |
| New service | Adding ValidationService |
| UI change | New component or page |

### 1.2 Consult Impact Matrix

Open [CHANGE_IMPACT_MATRIX.md](./CHANGE_IMPACT_MATRIX.md) and identify:
- Which docs need updating
- Which tests need adding/updating
- Which business rules are affected

### 1.3 Note Required Updates

Before coding, list what you'll need to update:

```markdown
## Task: Add ACK distribution support

Code changes:
- [ ] Add ACK to Distribution enum
- [ ] Add ACK config to DistributionService

Doc updates required:
- [ ] docs/technical/models.md (Distribution enum)
- [ ] docs/architecture/solution-overview.md (distribution count)
- [ ] docs/business/business-rules.md (BR-D section)

Tests required:
- [ ] DistributionServiceTests.ACK_HasCorrectConfig()
```

---

## Phase 2: IMPLEMENT

Write the code with documentation in mind.

### 2.1 Code Standards

- Use XML doc comments for public APIs
- Use meaningful names that match documentation terminology
- Follow existing patterns in the codebase

### 2.2 Test-Driven When Possible

Write tests alongside code, not after:

```csharp
[Fact]
public void ACK_Distribution_HasManagedControlPlane()
{
    var config = _service.GetDistributionConfig(Distribution.ACK);
    Assert.True(config.HasManagedControlPlane);
}
```

### 2.3 Track What Changed

Keep a mental or written note of:
- New public methods/properties
- Changed behavior
- New enum values
- Removed functionality

---

## Phase 3: SYNC

Update documentation BEFORE committing.

### 3.1 Technical Documentation

| If you added/changed... | Update this file |
|------------------------|------------------|
| Enum values | `docs/technical/models.md` |
| Services | `docs/technical/services.md` |
| Components | `docs/technical/ui-components.md` |
| API endpoints | `docs/technical/api-reference.md` |

### 3.2 Architecture Documentation

| If you added/changed... | Update this file |
|------------------------|------------------|
| New service layer | `docs/architecture/solution-overview.md` |
| Data flow changes | `docs/architecture/data-flow.md` |
| Component structure | `docs/architecture/component-diagram.md` |

### 3.3 Business Documentation

| If you added/changed... | Update this file |
|------------------------|------------------|
| Calculation logic | `docs/business/business-rules.md` |
| Feature scope | `docs/business/BRD.md` |
| Requirements | `docs/business/SRS.md` or `docs/srs/chapters/` |

### 3.4 Update Counts and Lists

When adding to enumerations, update ALL places that mention counts:
- "46 distributions" → "47 distributions"
- Distribution lists in multiple files

**Search for outdated counts:**
```bash
grep -r "46 distribution" docs/
grep -r "11 distribution" docs/   # Old count, should find nothing
```

---

## Phase 4: VERIFY

Run the pre-commit checklist before committing.

### 4.1 Code Verification

```bash
# Build succeeds
dotnet build

# Tests pass
dotnet test

# No new warnings (ideally)
dotnet build 2>&1 | grep -i warning
```

### 4.2 Documentation Verification

Manual checklist:

- [ ] All new enum values documented in models.md
- [ ] All new services documented in services.md
- [ ] All new components documented in ui-components.md
- [ ] Counts updated (distributions, services, etc.)
- [ ] Business rules updated if logic changed
- [ ] No "TODO: document this" left behind

### 4.3 Test Verification

- [ ] New functionality has tests
- [ ] Changed functionality has updated tests
- [ ] No tests reference old/removed code

---

## Phase 5: COMMIT

Create a single, atomic commit with code + docs + tests.

### 5.1 Commit Message Format

```
<type>: <short description>

<body explaining what and why>

Docs updated:
- docs/technical/models.md (added ACK distribution)
- docs/architecture/solution-overview.md (updated count)

Tests added:
- DistributionServiceTests.ACK_*
```

### 5.2 Commit Types

| Type | Use for |
|------|---------|
| feat | New feature |
| fix | Bug fix |
| docs | Documentation only |
| refactor | Code restructuring |
| test | Test additions/changes |
| chore | Maintenance tasks |

### 5.3 Atomic Commits

**Good:** One commit with feature + docs + tests
**Bad:** Separate commits for code, then docs, then tests

Why? If commits are separate, docs can be "forgotten" or deferred indefinitely.

---

## Phase 6: DONE

### 6.1 Verify Commit

```bash
git log -1 --stat  # Check what was included
git diff HEAD~1 --name-only  # List changed files
```

### 6.2 For AI Assistants

Before ending a session, verify:
- [ ] All code changes have corresponding doc updates
- [ ] No pending documentation tasks deferred
- [ ] Git status shows no uncommitted doc changes

---

## Special Cases

### Adding a New Enum Value

Minimum docs to update:
1. `docs/technical/models.md` - Add to enum list
2. Any file mentioning the count ("X distributions")
3. `docs/business/business-rules.md` if it affects rules

### Adding a New Service

Minimum docs to update:
1. `docs/technical/services.md` - Full service documentation
2. `docs/architecture/solution-overview.md` - Add to services list
3. `docs/business/business-rules.md` - Add rule category if new rules

### Adding a New Component

Minimum docs to update:
1. `docs/technical/ui-components.md` - Add to hierarchy
2. Parent page documentation if applicable

### Changing Scope (In/Out of Scope)

Update:
1. `docs/business/BRD.md` - Scope section
2. Related SRS chapters
3. Any docs that mention the feature

---

## Troubleshooting

### "I forgot to update docs in my last commit"

Option 1: Amend if not pushed
```bash
# Update docs now
git add docs/
git commit --amend --no-edit
```

Option 2: Separate commit if pushed
```bash
git add docs/
git commit -m "docs: sync documentation with <previous commit>"
```

### "I don't know what docs to update"

1. Check [CHANGE_IMPACT_MATRIX.md](./CHANGE_IMPACT_MATRIX.md)
2. Search for related terms: `grep -r "YourFeature" docs/`
3. When in doubt, update more rather than less

### "The docs are already out of sync"

1. Fix it now, don't propagate the problem
2. Create a separate "docs: sync" commit
3. Consider a documentation audit task
