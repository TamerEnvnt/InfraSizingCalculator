# Technology Stack

**Analysis Date:** 2026-01-11

## Languages

**Primary:**
- C# 12 - All application code

**Secondary:**
- CSS - Styling (`wwwroot/app.css`)
- JavaScript - Interop (`wwwroot/js/site.js`)

## Runtime

**Environment:**
- .NET 8.0
- Blazor Server with InteractiveServer render mode

**Package Manager:**
- NuGet
- Lockfile: `packages.lock.json` present

## Frameworks

**Core:**
- ASP.NET Core 8.0 - Web application framework
- Blazor Server - Interactive UI framework
- Entity Framework Core - Database access

**Testing:**
- xUnit - Unit test framework
- bUnit - Blazor component testing
- Playwright - E2E browser testing
- NSubstitute - Mocking
- FluentAssertions - Test assertions
- Stryker.NET - Mutation testing

**Build/Dev:**
- MSBuild - Project compilation
- dotnet CLI - Development workflow

## Key Dependencies

**Critical:**
- ClosedXML - Excel export functionality
- iText - PDF document generation
- Serilog - Structured logging
- OpenTelemetry - Observability and metrics

**Infrastructure:**
- Microsoft.EntityFrameworkCore.Sqlite - SQLite database
- Microsoft.AspNetCore.Identity - Authentication

## Configuration

**Environment:**
- `appsettings.json` - Base configuration with CalculatorSettings
- Environment variables for secrets

**Build:**
- `*.csproj` files - Project configuration

## Platform Requirements

**Development:**
- .NET 8 SDK
- Any platform (Windows/macOS/Linux)
- No external dependencies (SQLite embedded)

**Production:**
- Docker container or native .NET hosting

---

*Stack analysis: 2026-01-11*
*Update after major dependency changes*
