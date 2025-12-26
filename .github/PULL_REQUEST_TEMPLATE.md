## Summary

<!-- Brief description of changes -->

## Phase Checklist

<!-- Check all phases included in this PR -->

- [ ] **[SPEC]** - Requirements/specifications updated
- [ ] **[IMPL]** - Implementation code added/changed
- [ ] **[SYNC]** - Documentation synced to match code ⚠️ **REQUIRED before merge if IMPL changed**
- [ ] **[TEST]** - Tests added/updated

## Sync Gate Verification

<!-- If you have [IMPL] commits, you MUST have [SYNC] commits before [TEST] -->

| Check | Status |
|-------|--------|
| All commits have phase tags? | ✅ / ❌ |
| [SYNC] commit exists if [IMPL] changed code? | ✅ / ❌ / N/A |
| [TEST] commits are after [SYNC]? | ✅ / ❌ / N/A |

## Documentation Updated

<!-- List docs updated in [SYNC] commits. See docs/process/CHANGE_IMPACT_MATRIX.md -->

- [ ] `docs/technical/models.md` (if enums/models changed)
- [ ] `docs/technical/services.md` (if services changed)
- [ ] `docs/technical/ui-components.md` (if components changed)
- [ ] `docs/business/business-rules.md` (if logic changed)
- [ ] `docs/architecture/solution-overview.md` (if counts changed)
- [ ] Other: <!-- specify -->

## Commit Format Verification

All commits should follow this format:
```
[PHASE] type: description

Phase: SPEC|IMPL|SYNC|TEST
Feature: feature-name
Docs-Updated: yes|no|n/a
Tests-Updated: yes|no|n/a
```

## Additional Notes

<!-- Any additional context or notes -->

---

⚠️ **Reminder**: This PR will be validated by GitHub Actions. Commits without phase tags or violations of the sync gate will cause the build to fail.

📖 See [CONTRIBUTING.md](../CONTRIBUTING.md) and [docs/process/TEAM_WORKFLOW.md](../docs/process/TEAM_WORKFLOW.md) for full process documentation.
