#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Deploys InfraSizing Calculator to an existing IIS website.

.DESCRIPTION
    This script:
    - Stops the application pool
    - Backs up existing deployment (optional)
    - Extracts/copies new files
    - Preserves database files
    - Restarts the application pool

.PARAMETER SourcePath
    Path to the deployment package (ZIP file or folder)

.PARAMETER SitePath
    Physical path of the IIS site (default: C:\inetpub\InfraSizing)

.PARAMETER SiteName
    Name of the IIS site/app pool (default: InfraSizing)

.PARAMETER BackupExisting
    Create backup before deployment (default: true)

.PARAMETER PreserveDatabase
    Preserve existing database files (default: true)

.EXAMPLE
    .\03-deploy.ps1 -SourcePath "C:\deploy\InfraSizing.zip"

.EXAMPLE
    .\03-deploy.ps1 -SourcePath "\\server\share\InfraSizing.zip" -SitePath "C:\inetpub\InfraSizing"

.EXAMPLE
    .\03-deploy.ps1 -SourcePath "C:\publish\" -BackupExisting $false
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,
    [string]$SitePath = "C:\inetpub\InfraSizing",
    [string]$SiteName = "InfraSizing",
    [bool]$BackupExisting = $true,
    [bool]$PreserveDatabase = $true
)

$ErrorActionPreference = "Stop"
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "InfraSizing Calculator - Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validate source
if (-not (Test-Path $SourcePath)) {
    Write-Host "ERROR: Source path not found: $SourcePath" -ForegroundColor Red
    exit 1
}

$isZip = $SourcePath -like "*.zip"
Write-Host "Source: $SourcePath $(if ($isZip) { '(ZIP)' } else { '(Folder)' })" -ForegroundColor Gray
Write-Host "Target: $SitePath" -ForegroundColor Gray
Write-Host ""

# Step 1: Stop application pool
Write-Host "[1/6] Stopping application pool: $SiteName..." -ForegroundColor Yellow
$pool = Get-IISAppPool -Name $SiteName -ErrorAction SilentlyContinue
if ($pool) {
    if ($pool.State -eq "Started") {
        Stop-WebAppPool -Name $SiteName
        Start-Sleep -Seconds 2
        Write-Host "  Application pool stopped" -ForegroundColor Green
    } else {
        Write-Host "  Application pool already stopped" -ForegroundColor Green
    }
} else {
    Write-Host "  WARNING: Application pool not found. Continuing..." -ForegroundColor Yellow
}

# Step 2: Backup existing deployment
Write-Host ""
Write-Host "[2/6] Backup existing deployment..." -ForegroundColor Yellow
if ($BackupExisting -and (Test-Path $SitePath)) {
    $backupPath = "$SitePath-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "  Creating backup: $backupPath"

    # Copy existing files to backup (excluding logs)
    Copy-Item -Path $SitePath -Destination $backupPath -Recurse -Exclude "logs"
    Write-Host "  Backup created" -ForegroundColor Green
} else {
    Write-Host "  Skipping backup" -ForegroundColor Gray
}

# Step 3: Preserve database files
Write-Host ""
Write-Host "[3/6] Preserving database files..." -ForegroundColor Yellow
$preservedFiles = @()
$dbFiles = @("*.db", "*.db-shm", "*.db-wal")

if ($PreserveDatabase -and (Test-Path $SitePath)) {
    $tempDbPath = "$env:TEMP\InfraSizing-db-preserve"
    New-Item -Path $tempDbPath -ItemType Directory -Force | Out-Null

    foreach ($pattern in $dbFiles) {
        $files = Get-ChildItem -Path $SitePath -Filter $pattern -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            Copy-Item -Path $file.FullName -Destination $tempDbPath -Force
            $preservedFiles += $file.Name
            Write-Host "  Preserved: $($file.Name)" -ForegroundColor Green
        }
    }

    if ($preservedFiles.Count -eq 0) {
        Write-Host "  No database files found to preserve" -ForegroundColor Gray
    }
} else {
    Write-Host "  Skipping database preservation" -ForegroundColor Gray
}

# Step 4: Clear existing files (except logs and preserved items)
Write-Host ""
Write-Host "[4/6] Clearing existing deployment..." -ForegroundColor Yellow
if (Test-Path $SitePath) {
    # Remove all files except logs directory
    Get-ChildItem -Path $SitePath -Exclude "logs" | ForEach-Object {
        if ($_.PSIsContainer) {
            Remove-Item -Path $_.FullName -Recurse -Force
        } else {
            Remove-Item -Path $_.FullName -Force
        }
    }
    Write-Host "  Existing files cleared" -ForegroundColor Green
} else {
    New-Item -Path $SitePath -ItemType Directory -Force | Out-Null
    Write-Host "  Created new site directory" -ForegroundColor Green
}

# Step 5: Deploy new files
Write-Host ""
Write-Host "[5/6] Deploying new files..." -ForegroundColor Yellow

if ($isZip) {
    # Extract ZIP file
    Write-Host "  Extracting ZIP archive..."
    Expand-Archive -Path $SourcePath -DestinationPath $SitePath -Force
    Write-Host "  Extraction complete" -ForegroundColor Green
} else {
    # Copy from folder
    Write-Host "  Copying files..."
    Copy-Item -Path "$SourcePath\*" -Destination $SitePath -Recurse -Force
    Write-Host "  Copy complete" -ForegroundColor Green
}

# Restore preserved database files
if ($preservedFiles.Count -gt 0) {
    Write-Host "  Restoring database files..."
    $tempDbPath = "$env:TEMP\InfraSizing-db-preserve"
    Copy-Item -Path "$tempDbPath\*" -Destination $SitePath -Force
    Remove-Item -Path $tempDbPath -Recurse -Force
    Write-Host "  Database files restored" -ForegroundColor Green
}

# Ensure permissions
icacls $SitePath /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls $SitePath /grant "IUSR:(OI)(CI)F" /T /Q
Write-Host "  Permissions set" -ForegroundColor Green

# Step 6: Start application pool
Write-Host ""
Write-Host "[6/6] Starting application pool..." -ForegroundColor Yellow
if ($pool) {
    Start-WebAppPool -Name $SiteName
    Start-Sleep -Seconds 2

    # Verify it started
    $pool = Get-IISAppPool -Name $SiteName
    if ($pool.State -eq "Started") {
        Write-Host "  Application pool started" -ForegroundColor Green
    } else {
        Write-Host "  WARNING: Application pool may not have started correctly" -ForegroundColor Yellow
    }
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Deployment details:" -ForegroundColor White
Write-Host "  Source: $SourcePath" -ForegroundColor Gray
Write-Host "  Target: $SitePath" -ForegroundColor Gray
Write-Host "  Site: $SiteName" -ForegroundColor Gray
if ($preservedFiles.Count -gt 0) {
    Write-Host "  Preserved: $($preservedFiles -join ', ')" -ForegroundColor Gray
}
Write-Host ""

# Get site URL
$site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if ($site) {
    $binding = $site.Bindings.Collection | Select-Object -First 1
    $port = ($binding.bindingInformation -split ":")[1]
    Write-Host "Access the application at: http://localhost:$port" -ForegroundColor Green
}

Write-Host ""
Write-Host "Check logs at: $SitePath\logs" -ForegroundColor Gray
