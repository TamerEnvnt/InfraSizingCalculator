# Development Process & Synchronization

This folder contains the documented process for keeping code, documentation, and tests in sync.

## Why This Exists

Documentation drift is inevitable without a process. We've experienced:
- Docs saying "11 distributions" when code had 46
- "Cost estimation out of scope" when it was fully implemented
- Missing service documentation for 6 implemented services

This process prevents that drift.

---

## Quick Reference

### Before You Commit

```
1. Code changes complete?     → Check CHANGE_IMPACT_MATRIX.md
2. Docs need updating?        → Update them NOW (not "later")
3. Tests cover the change?    → Add/update tests
4. Business rules affected?   → Update business-rules.md
```

### The Golden Rule

> **If you change code behavior, update docs in the same commit.**

Not in a follow-up commit. Not "when you have time." In the SAME commit.

---

## Process Documents

| Document | Purpose | When to Read |
|----------|---------|--------------|
| [TEAM_WORKFLOW.md](./TEAM_WORKFLOW.md) | **Team coordination & sync gates** | When multiple people work on same feature |
| [SYNC_WORKFLOW.md](./SYNC_WORKFLOW.md) | Individual step-by-step workflow | Before starting any feature |
| [CHANGE_IMPACT_MATRIX.md](./CHANGE_IMPACT_MATRIX.md) | What changes require what updates | Before every commit |
| [BRANCHING_STRATEGY.md](./BRANCHING_STRATEGY.md) | Git branching and merge guidelines | When creating branches or PRs |

---

## For AI Assistants (Claude Code)

Claude Code reads `/.claude/CLAUDE.md` at session start, which contains:
- Project-specific instructions
- Sync requirements
- Documentation standards

**Every Claude session must:**
1. Check for uncommitted doc updates from previous sessions
2. Follow the sync workflow for any code changes
3. Verify docs match code before completing a task

---

## Enforcement Mechanisms

This process is **automatically enforced** at multiple levels:

### Local Enforcement (Git Hooks)

```bash
# Setup (run once after cloning)
./scripts/setup-hooks.sh
```

This installs:
- **commit-msg hook**: Blocks commits without `[SPEC]`, `[IMPL]`, `[SYNC]`, or `[TEST]` tags
- **Commit template**: Shows required format when you run `git commit`

### GitHub Enforcement (Actions)

The `.github/workflows/commit-validation.yml` workflow:
- ❌ **Blocks PRs** with commits missing phase tags
- ⚠️ **Warns** if `[IMPL]` commits lack `[SYNC]` commits
- ❌ **Blocks** `[TEST]` commits without prior `[SYNC]`

### PR Template

The `.github/PULL_REQUEST_TEMPLATE.md` includes:
- Phase checklist
- Sync gate verification
- Documentation update checklist

### What Gets Blocked

| Violation | Local Hook | GitHub Action |
|-----------|------------|---------------|
| Missing phase tag | ❌ Blocked | ❌ Blocked |
| [TEST] without [SYNC] | ⚠️ Warning | ❌ Blocked |
| [SYNC] without doc changes | - | ❌ Blocked |
| [IMPL] without [SYNC] | ⚠️ Warning | ⚠️ Warning |

We optimize for:
1. Catching violations early (at commit time, not in reviews)
2. Making the right thing easy to do
3. Clear error messages that explain what to fix

---

## Metrics

Track sync health by periodically running:

```bash
# Count TODOs in docs
grep -r "TODO" docs/ | wc -l

# Find docs older than newest code change
find src/ -name "*.cs" -newer docs/technical/services.md
```

If docs are consistently older than code, the process isn't being followed.
