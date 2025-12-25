# Development Setup Guide

This guide explains how to set up the Infrastructure Sizing Calculator for local development.

---

## Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| .NET SDK | 10.0+ | Build and run the application |
| Visual Studio 2022 / VS Code | Latest | IDE with C# support |
| Git | Latest | Version control |

### Optional Software

| Software | Purpose |
|----------|---------|
| Node.js | For Playwright E2E tests |
| Docker | For containerized deployment |

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/infrastructure-sizing.git
cd infrastructure-sizing/dotnet/InfraSizingCalculator
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Application

```bash
cd src/InfraSizingCalculator
dotnet run
```

The application will be available at:
- **HTTP:** http://localhost:5062
- **HTTPS:** https://localhost:7062

---

## Project Structure

```
InfraSizingCalculator/
├── InfraSizingCalculator.slnx          # Solution file
├── src/
│   └── InfraSizingCalculator/          # Main application
│       ├── Program.cs                  # Entry point
│       ├── Components/                 # Blazor UI
│       ├── Controllers/Api/            # REST API
│       ├── Services/                   # Business logic
│       ├── Models/                     # Data models
│       └── wwwroot/                    # Static assets
└── tests/
    ├── InfraSizingCalculator.UnitTests/
    └── InfraSizingCalculator.E2ETests/
```

---

## Running Tests

### Unit Tests

```bash
dotnet test tests/InfraSizingCalculator.UnitTests
```

### E2E Tests (Playwright)

```bash
# Install Playwright browsers (first time only)
cd tests/InfraSizingCalculator.E2ETests
pwsh bin/Debug/net10.0/playwright.ps1 install

# Run E2E tests
dotnet test tests/InfraSizingCalculator.E2ETests
```

---

## Configuration

### appsettings.json

The application uses standard ASP.NET Core configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Development Settings

Create `appsettings.Development.json` for local overrides:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

---

## IDE Setup

### Visual Studio 2022

1. Open `InfraSizingCalculator.slnx`
2. Set `src/InfraSizingCalculator` as startup project
3. Press F5 to run with debugging

### Visual Studio Code

1. Install C# Dev Kit extension
2. Open the `dotnet/InfraSizingCalculator` folder
3. Use the provided launch configuration or create `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Launch",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/InfraSizingCalculator/bin/Debug/net10.0/InfraSizingCalculator.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/InfraSizingCalculator",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      }
    }
  ]
}
```

---

## Hot Reload

The application supports .NET Hot Reload:

```bash
dotnet watch run --project src/InfraSizingCalculator
```

Changes to `.razor`, `.cs`, and `.css` files will automatically reload.

---

## Common Tasks

### Add New Technology

1. Add enum value to `Models/Enums/Technology.cs`
2. Add configuration in `Services/TechnologyService.cs`
3. Update UI in `Components/Pages/Home.razor`

### Add New Distribution

1. Add enum value to `Models/Enums/Distribution.cs`
2. Add configuration in `Services/DistributionService.cs`
3. Update UI in `Components/Pages/Home.razor`

### Modify Calculation Logic

Business logic is in `Services/`:
- `K8sSizingService.cs` - Kubernetes calculations
- `VMSizingService.cs` - VM calculations

---

## Troubleshooting

### Port Already in Use

```bash
# Find and kill process on port 5062
lsof -ti:5062 | xargs kill -9
```

### Clear Build Cache

```bash
dotnet clean
rm -rf bin obj
dotnet build
```

### Restore NuGet Packages

```bash
dotnet restore --force
```
