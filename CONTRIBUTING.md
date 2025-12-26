# Contributing to Infrastructure Sizing Calculator

## First Time Setup (REQUIRED)

Run this command after cloning the repository:

```bash
./scripts/setup-hooks.sh
```

This configures:
- **Git hooks**: Block commits without phase tags
- **Commit template**: Show format when you run `git commit`

## Before You Start

**READ THIS FIRST**: This project uses a strict synchronization process to keep code, docs, and tests aligned.

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ⚠️  MANDATORY: Follow the Team Workflow                                │
│                                                                          │
│  1. Know which PHASE you're in (Spec → Impl → Sync → Test)              │
│  2. Don't skip the SYNC GATE (docs must match code before testing)      │
│  3. Use correct COMMIT FORMAT (includes phase marker)                   │
│                                                                          │
│  📖 Full process: docs/process/TEAM_WORKFLOW.md                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Git Branching Strategy

This project uses **Simplified Git Flow** with the following branches:

### Branch Structure

```
main ─────────────────────────────────────────────► (stable/production)
  │                                      ▲
  │                                      │ (release merges)
  ▼                                      │
develop ──┬──────────────────────────────┴───────► (integration)
          │         ▲         ▲         ▲
          ▼         │         │         │
     feature/*   feature/*  bugfix/*  hotfix/*
```

### Branch Types

| Branch | Purpose | Created From | Merges To |
|--------|---------|--------------|-----------|
| `main` | Production-ready, stable code | - | - |
| `develop` | Integration and testing | `main` | `main` |
| `feature/*` | New features | `develop` | `develop` |
| `bugfix/*` | Bug fixes | `develop` | `develop` |
| `hotfix/*` | Urgent production fixes | `main` | `main` + `develop` |
| `release/*` | Release preparation | `develop` | `main` + `develop` |

### Branch Naming

| Type | Format | Example |
|------|--------|---------|
| Feature | `feature/short-description` | `feature/growth-planning` |
| Bugfix | `bugfix/issue-description` | `bugfix/calculation-error` |
| Hotfix | `hotfix/critical-fix` | `hotfix/security-patch` |
| Release | `release/version` | `release/1.2.0` |

### Starting New Work

```bash
# Always start from develop
git checkout develop
git pull origin develop

# Create your feature branch
git checkout -b feature/your-feature-name

# Work through phases on your branch
# [SPEC] → [IMPL] → [SYNC] → [TEST]

# Push your branch
git push -u origin feature/your-feature-name

# Create PR to develop when ready
```

### Branch Rules

| Branch | Direct Push | Merge Method | Requirements |
|--------|-------------|--------------|--------------|
| `main` | No | Squash merge | PR + Review + Tests pass |
| `develop` | No | Merge commit | PR + Review |
| `feature/*` | Yes | - | Assigned dev only |
| `bugfix/*` | Yes | - | Assigned dev only |

### Feature Branch Lifecycle

```
1. Create branch from develop
   └── git checkout -b feature/new-feature

2. Complete all phases on branch
   ├── [SPEC] commits
   ├── [IMPL] commits
   ├── [SYNC] commits
   └── [TEST] commits

3. Create PR to develop
   └── Request review
   └── Address feedback
   └── Merge when approved

4. Delete branch after merge
   └── git branch -d feature/new-feature
```

---

## Quick Start

### 1. Understand the Phase System

Every feature goes through 4 phases IN ORDER:

| Phase | Who | What | Commits Tagged |
|-------|-----|------|----------------|
| **1. SPEC** | BA | Write requirements | `[SPEC]` |
| **2. IMPL** | Developer | Write code | `[IMPL]` |
| **3. SYNC** | BA/Tech Writer | Update docs to match code | `[SYNC]` |
| **4. TEST** | Tester | Write/run tests | `[TEST]` |

### 2. Identify Your Role

| If you're... | You work in phases... | You wait during... |
|--------------|----------------------|-------------------|
| BA/Analyst | 1 (Spec), 3 (Sync) | 2 (Impl), 4 (Test) |
| Developer | 2 (Impl) | 1 (Spec), 4 (Test) |
| Tech Writer | 3 (Sync) | 1, 2, 4 |
| Tester | 4 (Test) | 1, 2, 3 |

### 3. Use Correct Commit Format

```
[PHASE] type: description

Body explaining what and why

Phase: SPEC|IMPL|SYNC|TEST
Docs-Updated: yes|no|n/a
Tests-Updated: yes|no|n/a
```

---

## The Golden Rules

### Rule 1: Code is Source of Truth

After implementation begins, **CODE determines reality**. Documentation must sync TO the code, not the other way around.

### Rule 2: Never Skip the Sync Gate

```
[IMPL complete] ═══ SYNC GATE ═══ [TEST can start]
                        │
                        └── ALL docs must be updated here
```

Testing against unsynced docs = testing against wrong requirements.

### Rule 3: Phase-Appropriate Actions Only

| In Phase | You CAN | You CANNOT |
|----------|---------|------------|
| SPEC | Write specs, requirements | Write code, tests |
| IMPL | Write code | Update docs (they'll change), write tests |
| SYNC | Update docs to match code | Change code behavior |
| TEST | Write/run tests | Change code behavior, change docs |

### Rule 4: Atomic Phase Commits

Each commit should be:
- Tagged with its phase `[SPEC]`, `[IMPL]`, `[SYNC]`, or `[TEST]`
- Complete for that phase (don't split a sync across commits)
- Include all related files for that phase

---

## Commit Message Format

### Template

```
[PHASE] type: short description

Longer description of what changed and why.

Phase: SPEC|IMPL|SYNC|TEST
Feature: feature-name (if applicable)
Docs-Updated: yes|no|n/a
Tests-Updated: yes|no|n/a

🤖 Generated with [Claude Code](https://claude.com/claude-code)
Co-Authored-By: Your Name <your@email.com>
```

### Examples

**Spec Phase Commit:**
```
[SPEC] feat: define ACK distribution requirements

Added requirements for Alibaba Cloud ACK distribution support.
Includes managed control plane and pricing tiers.

Phase: SPEC
Feature: ack-distribution
Docs-Updated: yes (BRD.md, business-rules.md)
Tests-Updated: n/a
```

**Implementation Phase Commit:**
```
[IMPL] feat: add ACK distribution support

Implemented ACK distribution in DistributionService.
- Added Distribution.ACK enum value
- Added ACK configuration with managed control plane
- Pricing integration pending

Phase: IMPL
Feature: ack-distribution
Docs-Updated: no (SYNC phase pending)
Tests-Updated: no (TEST phase pending)
```

**Sync Phase Commit:**
```
[SYNC] docs: sync documentation for ACK distribution

Updated all documentation to match ACK implementation.
- models.md: Added ACK to Distribution enum
- services.md: Updated DistributionService docs
- solution-overview.md: Updated distribution count (46→47)

Phase: SYNC
Feature: ack-distribution
Docs-Updated: yes
Tests-Updated: n/a
```

**Test Phase Commit:**
```
[TEST] test: add ACK distribution tests

Added unit tests for ACK distribution configuration.
- Config validation tests
- Managed control plane verification
- Pricing calculation tests

Phase: TEST
Feature: ack-distribution
Docs-Updated: n/a
Tests-Updated: yes
```

---

## Workflow Checklist

### Before Starting Any Work

- [ ] Read `docs/process/TEAM_WORKFLOW.md`
- [ ] Identify which phase the feature is in
- [ ] Confirm you should be working in this phase
- [ ] Check `docs/process/CHANGE_IMPACT_MATRIX.md` for impact

### Before Each Commit

- [ ] Commit message includes `[PHASE]` tag
- [ ] Commit message includes phase metadata
- [ ] Only phase-appropriate files are included
- [ ] If SYNC phase: ALL required docs are updated

### Before Declaring Feature Complete

- [ ] All 4 phases completed
- [ ] All commits properly tagged
- [ ] Docs match code (SYNC gate passed)
- [ ] Tests pass and cover the feature

---

## Process Documentation

| Document | Purpose |
|----------|---------|
| [docs/process/README.md](./docs/process/README.md) | Process overview |
| [docs/process/TEAM_WORKFLOW.md](./docs/process/TEAM_WORKFLOW.md) | **Full team coordination guide** |
| [docs/process/SYNC_WORKFLOW.md](./docs/process/SYNC_WORKFLOW.md) | Individual sync workflow |
| [docs/process/CHANGE_IMPACT_MATRIX.md](./docs/process/CHANGE_IMPACT_MATRIX.md) | Code change → doc update mapping |

---

## For AI Assistants

AI assistants (Claude Code, etc.) read `.claude/CLAUDE.md` at session start which contains:
- Project-specific instructions
- Phase-based workflow rules
- Commit format requirements

AI assistants must:
1. Identify current phase before taking action
2. Ask if phase is unclear
3. Use proper commit format with phase tags
4. Never skip the sync gate

---

## Getting Help

If you're unsure about:
- **Which phase to work in**: Ask the feature owner or check recent commits
- **What docs to update**: See `docs/process/CHANGE_IMPACT_MATRIX.md`
- **Commit format**: See examples above or ask for review

---

## Enforcement

This process is **semi-enforced**:
- Required: Follow the phase order and commit format
- Verified: Code reviews check for proper phase tags
- Flexible: Exceptions documented in commit message

Violations (commits without phase tags, skipped sync gate) will be flagged in code review.
