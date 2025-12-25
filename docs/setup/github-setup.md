# GitHub Repository Setup Guide

This guide explains how to set up the GitHub repository for the Infrastructure Sizing Calculator project.

---

## Repository Structure

The repository will contain two solutions under one parent:

```
infrastructure-sizing/                    # Parent repository
├── README.md                            # Main repo documentation
├── .gitignore                           # Git ignore rules
├── docs/                                # Shared documentation
│   ├── README.md
│   ├── architecture/
│   ├── business/
│   ├── technical/
│   ├── vendor-specs/
│   └── setup/
├── portable/                            # HTML/JS version (standalone)
│   └── Infra Sizing/
│       ├── universal-sizing-calculator-v3.html
│       └── related files...
└── dotnet/                              # .NET Blazor version
    └── InfraSizingCalculator/
        ├── src/
        ├── tests/
        └── InfraSizingCalculator.slnx
```

---

## Initial Setup Steps

### 1. Create Repository on GitHub

1. Go to https://github.com/new
2. Repository name: `infrastructure-sizing`
3. Visibility: **Private**
4. Do NOT initialize with README (we'll push existing code)
5. Click "Create repository"

### 2. Initialize Local Repository

```bash
# Navigate to parent folder containing both solutions
cd /path/to/infrastructure-sizing

# Initialize git
git init

# Create .gitignore
cat > .gitignore << 'EOF'
# .NET
bin/
obj/
.vs/
*.user
*.suo
appsettings.Development.json

# IDE
.idea/
*.swp
.vscode/

# OS
.DS_Store
Thumbs.db

# Node (if any)
node_modules/

# Test results
TestResults/
coverage/

# Build artifacts
publish/
out/

# NuGet
*.nupkg
packages/

# Playwright
playwright-report/
test-results/
EOF

# Add all files
git add .

# Initial commit
git commit -m "Initial commit: Infrastructure Sizing Calculator

- .NET Blazor Server application
- HTML/JS portable version
- Documentation and vendor specs

🤖 Generated with Claude Code
"
```

### 3. Push to GitHub

```bash
# Add remote origin
git remote add origin https://github.com/your-org/infrastructure-sizing.git

# Push to main branch
git branch -M main
git push -u origin main
```

---

## Branch Strategy

### Recommended Branches

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code |
| `develop` | Integration branch |
| `feature/*` | New features |
| `bugfix/*` | Bug fixes |
| `release/*` | Release preparation |

### Branch Protection Rules

1. Go to Settings > Branches > Add rule
2. Branch name pattern: `main`
3. Enable:
   - Require pull request before merging
   - Require approvals (1+)
   - Dismiss stale reviews
   - Require status checks to pass

---

## GitHub Actions (CI/CD)

### Build and Test Workflow

Create `.github/workflows/build.yml`:

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Restore dependencies
      run: dotnet restore dotnet/InfraSizingCalculator

    - name: Build
      run: dotnet build dotnet/InfraSizingCalculator --no-restore

    - name: Run Unit Tests
      run: dotnet test dotnet/InfraSizingCalculator/tests/InfraSizingCalculator.UnitTests --no-build --verbosity normal
```

### E2E Test Workflow

Create `.github/workflows/e2e.yml`:

```yaml
name: E2E Tests

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  e2e:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Build
      run: dotnet build dotnet/InfraSizingCalculator

    - name: Install Playwright
      run: |
        cd dotnet/InfraSizingCalculator/tests/InfraSizingCalculator.E2ETests
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install --with-deps

    - name: Run E2E Tests
      run: dotnet test dotnet/InfraSizingCalculator/tests/InfraSizingCalculator.E2ETests
```

---

## Team Collaboration

### Access Control

1. Go to Settings > Collaborators and teams
2. Add team members with appropriate roles:
   - **Admin**: Full access
   - **Write**: Push and manage PRs
   - **Read**: View only

### Pull Request Template

Create `.github/PULL_REQUEST_TEMPLATE.md`:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation
- [ ] Refactoring

## Testing
- [ ] Unit tests pass
- [ ] E2E tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style
- [ ] Self-review completed
- [ ] Documentation updated
```

### Issue Templates

Create `.github/ISSUE_TEMPLATE/bug_report.md`:

```markdown
---
name: Bug Report
about: Report a bug in the calculator
---

## Description
Clear description of the bug

## Steps to Reproduce
1. Go to...
2. Click on...
3. See error

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- Browser:
- OS:
```

---

## Release Process

### Semantic Versioning

Follow semver: `MAJOR.MINOR.PATCH`

- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

### Creating a Release

```bash
# Tag the release
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0

# Or use GitHub Releases UI
```

### Release Workflow

Create `.github/workflows/release.yml`:

```yaml
name: Release

on:
  release:
    types: [published]

jobs:
  publish:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Publish
      run: dotnet publish dotnet/InfraSizingCalculator/src/InfraSizingCalculator -c Release -o ./publish

    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: infrastructure-sizing-${{ github.ref_name }}
        path: ./publish
```

---

## Handover Checklist

When handing over to the development team:

- [ ] Repository created and accessible
- [ ] All code pushed to main branch
- [ ] Branch protection rules configured
- [ ] CI/CD workflows set up
- [ ] Team members added with appropriate access
- [ ] README updated with quick start guide
- [ ] Documentation complete and accessible
