# Git Branching Strategy

This document describes the branching strategy for the Infrastructure Sizing Calculator project.

---

## Overview

We use **Simplified Git Flow** - a streamlined version of Git Flow that balances structure with simplicity.

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

---

## Branch Types

### Protected Branches

| Branch | Purpose | Protection Level |
|--------|---------|------------------|
| `main` | Production-ready code | Highest - no direct push |
| `develop` | Integration branch | High - no direct push |

### Working Branches

| Branch | Purpose | Lifetime |
|--------|---------|----------|
| `feature/*` | New features | Until merged to develop |
| `bugfix/*` | Bug fixes | Until merged to develop |
| `hotfix/*` | Urgent production fixes | Until merged to main + develop |
| `release/*` | Release preparation | Until merged to main + develop |

---

## Branch Naming Conventions

### Format

```
<type>/<short-description>
```

### Examples

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feature/<description>` | `feature/growth-planning` |
| Bugfix | `bugfix/<description>` | `bugfix/cost-calculation-error` |
| Hotfix | `hotfix/<description>` | `hotfix/security-vulnerability` |
| Release | `release/<version>` | `release/1.2.0` |

### Guidelines

- Use lowercase letters
- Use hyphens to separate words
- Keep descriptions short but meaningful
- Include ticket/issue number if applicable: `feature/ISC-123-new-calculator`

---

## Workflow

### Starting a New Feature

```bash
# 1. Ensure you're on develop and up-to-date
git checkout develop
git pull origin develop

# 2. Create feature branch
git checkout -b feature/your-feature-name

# 3. Work on your feature (following phase workflow)
# Make commits with proper phase tags: [SPEC], [IMPL], [SYNC], [TEST]

# 4. Push your branch
git push -u origin feature/your-feature-name

# 5. Create Pull Request to develop
# Use GitHub/GitLab UI or CLI
```

### Completing a Feature

```bash
# 1. Ensure feature branch is up-to-date with develop
git checkout feature/your-feature-name
git fetch origin
git merge origin/develop

# 2. Resolve any conflicts
# 3. Push updates
git push

# 4. Create/Update PR
# 5. Get review approval
# 6. Merge PR (via GitHub UI)

# 7. Delete feature branch locally
git checkout develop
git pull origin develop
git branch -d feature/your-feature-name
```

### Creating a Hotfix

```bash
# 1. Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-fix

# 2. Make the fix
# Commit with [IMPL] tag

# 3. Create PR to main
# After merge, also merge to develop
```

### Creating a Release

```bash
# 1. Create release branch from develop
git checkout develop
git pull origin develop
git checkout -b release/1.2.0

# 2. Update version numbers, final testing
# 3. Create PR to main
# 4. After main merge, tag the release
git tag -a v1.2.0 -m "Release 1.2.0"
git push origin v1.2.0

# 5. Merge back to develop
```

---

## Branch Protection Rules

### For GitHub

Configure these rules in Repository Settings > Branches:

#### `main` Branch

- [x] Require pull request before merging
- [x] Require approvals: 1
- [x] Dismiss stale PR approvals when new commits are pushed
- [x] Require status checks to pass before merging
- [x] Require branches to be up to date before merging
- [x] Do not allow bypassing the above settings

#### `develop` Branch

- [x] Require pull request before merging
- [x] Require approvals: 1
- [x] Require status checks to pass before merging

### GitHub CLI Commands

```bash
# Protect main branch
gh api repos/{owner}/{repo}/branches/main/protection -X PUT -f required_status_checks='{"strict":true,"contexts":["build","test"]}' -f enforce_admins=true -f required_pull_request_reviews='{"required_approving_review_count":1}'

# Protect develop branch
gh api repos/{owner}/{repo}/branches/develop/protection -X PUT -f required_pull_request_reviews='{"required_approving_review_count":1}'
```

---

## Integration with Phase Workflow

Each feature branch should contain the complete phase lifecycle:

```
feature/new-feature
    │
    ├── [SPEC] Define requirements
    │   └── Update BRD, business-rules.md (draft)
    │
    ├── [IMPL] Implement feature
    │   └── Write code in src/
    │
    ├── [SYNC] Sync documentation
    │   └── Update all docs to match code
    │
    └── [TEST] Add tests
        └── Write unit/integration tests
```

### PR Requirements by Phase

| Phase Complete | PR Ready? | Notes |
|----------------|-----------|-------|
| SPEC only | No | Need implementation |
| IMPL only | No | Need docs sync |
| SYNC only | No | Need tests |
| All phases | Yes | Ready for review |

---

## Merge Strategies

| Merge Type | When to Use | Branch |
|------------|-------------|--------|
| **Squash Merge** | Feature → develop, develop → main | Clean history |
| **Merge Commit** | Hotfix → main, Release → main | Preserve history |
| **Rebase** | Updating feature branch from develop | Linear history |

### Recommended Settings

```
feature/* → develop: Squash and merge
develop → main: Squash and merge (for releases)
hotfix/* → main: Merge commit (preserve fix history)
```

---

## Quick Reference

### Daily Commands

```bash
# Start new feature
git checkout develop && git pull && git checkout -b feature/name

# Update feature branch with latest develop
git fetch origin && git merge origin/develop

# Push changes
git push -u origin feature/name
```

### Branch Cleanup

```bash
# Delete merged local branches
git branch --merged | grep -v "main\|develop" | xargs git branch -d

# Delete remote branch
git push origin --delete feature/old-feature
```

---

## Troubleshooting

### "Branch is behind develop"

```bash
git checkout feature/your-feature
git fetch origin
git merge origin/develop
# Resolve conflicts if any
git push
```

### "Cannot push to protected branch"

You cannot push directly to `main` or `develop`. Create a PR instead.

### "Merge conflicts"

```bash
# On your feature branch
git fetch origin
git merge origin/develop
# Resolve conflicts in your editor
git add .
git commit -m "[IMPL] fix: resolve merge conflicts"
git push
```
