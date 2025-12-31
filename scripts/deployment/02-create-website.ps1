#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Creates an IIS website for InfraSizing Calculator.

.DESCRIPTION
    This script creates and configures:
    - Application pool with correct settings
    - IIS website bound to specified port
    - Basic web.config for ASP.NET Core

.PARAMETER SiteName
    Name of the IIS site (default: InfraSizing)

.PARAMETER SitePort
    Port for the IIS site (default: 8080)

.PARAMETER SitePath
    Physical path for the site (default: C:\inetpub\InfraSizing)

.PARAMETER Environment
    ASP.NET Core environment (default: Production)

.EXAMPLE
    .\02-create-website.ps1 -SitePort 8080

.EXAMPLE
    .\02-create-website.ps1 -SiteName "InfraSizing-Dev" -SitePort 8081 -Environment "Development"
#>

param(
    [string]$SiteName = "InfraSizing",
    [int]$SitePort = 8080,
    [string]$SitePath = "C:\inetpub\InfraSizing",
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment = "Production"
)

$ErrorActionPreference = "Stop"

# Import WebAdministration module (compatible with Windows PowerShell and PowerShell Core)
try {
    Import-Module WebAdministration -SkipEditionCheck -ErrorAction Stop
} catch {
    try {
        Import-Module WebAdministration -ErrorAction Stop
    } catch {
        Write-Host "ERROR: WebAdministration module not available." -ForegroundColor Red
        Write-Host "Ensure IIS Management Tools are installed." -ForegroundColor Red
        exit 1
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "InfraSizing Calculator - Create Website" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Create site directory if it doesn't exist
Write-Host "[1/5] Preparing site directory: $SitePath..." -ForegroundColor Yellow
if (-not (Test-Path $SitePath)) {
    New-Item -Path $SitePath -ItemType Directory -Force | Out-Null
    Write-Host "  Created directory" -ForegroundColor Green
} else {
    Write-Host "  Directory already exists" -ForegroundColor Green
}

# Create logs subdirectory
New-Item -Path "$SitePath\logs" -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null

# Set permissions
icacls $SitePath /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls $SitePath /grant "IUSR:(OI)(CI)F" /T /Q
Write-Host "  Permissions configured" -ForegroundColor Green

# Step 2: Create Application Pool
Write-Host ""
Write-Host "[2/5] Creating application pool: $SiteName..." -ForegroundColor Yellow
$poolName = $SiteName
$appcmd = "$env:SystemRoot\System32\inetsrv\appcmd.exe"

# Check if app pool exists
$poolCheck = & $appcmd list apppool /name:$poolName 2>$null
if ($poolCheck) {
    Write-Host "  Application pool already exists, updating settings..." -ForegroundColor Yellow
} else {
    & $appcmd add apppool /name:$poolName | Out-Null
    Write-Host "  Created application pool" -ForegroundColor Green
}

# Configure app pool for ASP.NET Core (No Managed Code)
& $appcmd set apppool /apppool.name:$poolName /managedRuntimeVersion:"" | Out-Null
& $appcmd set apppool /apppool.name:$poolName /startMode:AlwaysRunning | Out-Null
& $appcmd set apppool /apppool.name:$poolName /processModel.idleTimeout:00:00:00 | Out-Null
Write-Host "  Application pool configured for ASP.NET Core" -ForegroundColor Green

# Step 3: Remove existing site if it exists (with same name)
Write-Host ""
Write-Host "[3/5] Configuring IIS site: $SiteName..." -ForegroundColor Yellow
$existingSite = & $appcmd list site /name:$SiteName 2>$null

if ($existingSite) {
    Write-Host "  Removing existing site configuration..." -ForegroundColor Yellow
    & $appcmd delete site /site.name:$SiteName | Out-Null
}

# Check if port is in use by another site
$allSites = & $appcmd list site 2>$null
$portInUse = $allSites | Where-Object { $_ -match ":${SitePort}:" -and $_ -notmatch "^SITE `"$SiteName`"" }
if ($portInUse) {
    Write-Host "  WARNING: Port $SitePort is already used by another site" -ForegroundColor Red
    Write-Host "  Please choose a different port or remove the existing site" -ForegroundColor Red
    exit 1
}

# Step 4: Create the website
Write-Host ""
Write-Host "[4/5] Creating website on port $SitePort..." -ForegroundColor Yellow
& $appcmd add site /name:$SiteName /physicalPath:$SitePath /bindings:"http/*:${SitePort}:" | Out-Null
& $appcmd set site /site.name:$SiteName /[path='/'].applicationPool:$poolName | Out-Null

Write-Host "  Website created successfully" -ForegroundColor Green

# Step 5: Create web.config
Write-Host ""
Write-Host "[5/5] Creating web.config..." -ForegroundColor Yellow

$webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\InfraSizingCalculator.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="$Environment" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

$webConfig | Out-File -FilePath "$SitePath\web.config" -Encoding UTF8 -Force
Write-Host "  web.config created with ASPNETCORE_ENVIRONMENT=$Environment" -ForegroundColor Green

# Start the site
& $appcmd start site /site.name:$SiteName 2>$null | Out-Null

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Website Creation Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor White
Write-Host "  Site Name: $SiteName" -ForegroundColor Gray
Write-Host "  Site Path: $SitePath" -ForegroundColor Gray
Write-Host "  Port: $SitePort" -ForegroundColor Gray
Write-Host "  App Pool: $poolName" -ForegroundColor Gray
Write-Host "  Environment: $Environment" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "1. Copy your published application files to: $SitePath" -ForegroundColor Gray
Write-Host "2. Ensure .NET Hosting Bundle is installed" -ForegroundColor Gray
Write-Host "3. Access the site at: http://localhost:$SitePort" -ForegroundColor Gray
Write-Host ""
Write-Host "To deploy, run: .\03-deploy.ps1 -SitePath '$SitePath'" -ForegroundColor Yellow
