#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Prepares a Windows Server for hosting InfraSizing Calculator.

.DESCRIPTION
    This script installs and configures:
    - IIS with required features
    - Web Management Service for remote management
    - Firewall rules
    - Creates deployment user

.PARAMETER DeployUsername
    Username for the deployment account (default: deploy-infrasizing)

.PARAMETER DeployPassword
    Password for the deployment account

.PARAMETER SitePort
    Port for the IIS site (default: 8080)

.EXAMPLE
    .\01-prepare-server.ps1 -DeployPassword "YourSecurePassword123!"
#>

param(
    [string]$DeployUsername = "deploy-infrasizing",
    [Parameter(Mandatory=$true)]
    [string]$DeployPassword,
    [int]$SitePort = 8080
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "InfraSizing Calculator - Server Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Install IIS
Write-Host "[1/6] Installing IIS and required features..." -ForegroundColor Yellow
$features = @(
    "Web-Server",
    "Web-WebSockets",
    "Web-Asp-Net45",
    "Web-Mgmt-Service",
    "Web-Mgmt-Tools"
)

foreach ($feature in $features) {
    $installed = Get-WindowsFeature -Name $feature
    if (-not $installed.Installed) {
        Write-Host "  Installing $feature..."
        Install-WindowsFeature -Name $feature -IncludeManagementTools | Out-Null
    } else {
        Write-Host "  $feature already installed" -ForegroundColor Green
    }
}

# Step 2: Configure Web Management Service
Write-Host ""
Write-Host "[2/6] Configuring Web Management Service..." -ForegroundColor Yellow
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\WebManagement\Server" -Name "EnableRemoteManagement" -Value 1 -ErrorAction SilentlyContinue
Set-Service -Name WMSVC -StartupType Automatic -ErrorAction SilentlyContinue
Start-Service WMSVC -ErrorAction SilentlyContinue
Write-Host "  Web Management Service configured" -ForegroundColor Green

# Step 3: Create deployment user
Write-Host ""
Write-Host "[3/6] Creating deployment user: $DeployUsername..." -ForegroundColor Yellow
$existingUser = Get-LocalUser -Name $DeployUsername -ErrorAction SilentlyContinue
if ($existingUser) {
    Write-Host "  User already exists, updating password..." -ForegroundColor Yellow
    $securePassword = ConvertTo-SecureString $DeployPassword -AsPlainText -Force
    Set-LocalUser -Name $DeployUsername -Password $securePassword -PasswordNeverExpires $true
} else {
    $securePassword = ConvertTo-SecureString $DeployPassword -AsPlainText -Force
    New-LocalUser -Name $DeployUsername -Password $securePassword -Description "Deployment account for InfraSizing" -PasswordNeverExpires | Out-Null
}

# Add to required groups
Add-LocalGroupMember -Group "IIS_IUSRS" -Member $DeployUsername -ErrorAction SilentlyContinue
Add-LocalGroupMember -Group "Users" -Member $DeployUsername -ErrorAction SilentlyContinue
Write-Host "  Deployment user configured" -ForegroundColor Green

# Step 4: Create site directory
Write-Host ""
Write-Host "[4/6] Creating site directory..." -ForegroundColor Yellow
$sitePath = "C:\inetpub\InfraSizing"
if (-not (Test-Path $sitePath)) {
    New-Item -Path $sitePath -ItemType Directory -Force | Out-Null
}
New-Item -Path "$sitePath\logs" -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null

# Set permissions
icacls $sitePath /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls $sitePath /grant "IUSR:(OI)(CI)F" /T /Q
icacls $sitePath /grant "${DeployUsername}:(OI)(CI)F" /T /Q
Write-Host "  Site directory created with permissions" -ForegroundColor Green

# Step 5: Configure firewall
Write-Host ""
Write-Host "[5/6] Configuring firewall rules..." -ForegroundColor Yellow

# Site port
$ruleName = "InfraSizing Site (TCP $SitePort)"
$existingRule = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if (-not $existingRule) {
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP -LocalPort $SitePort -Action Allow | Out-Null
    Write-Host "  Created firewall rule for port $SitePort" -ForegroundColor Green
} else {
    Write-Host "  Firewall rule for port $SitePort already exists" -ForegroundColor Green
}

# Web Deploy port (8172)
$wmsvcRule = Get-NetFirewallRule -DisplayName "Web Management Service (HTTP)" -ErrorAction SilentlyContinue
if (-not $wmsvcRule) {
    New-NetFirewallRule -DisplayName "Web Management Service (HTTP)" -Direction Inbound -Protocol TCP -LocalPort 8172 -Action Allow | Out-Null
    Write-Host "  Created firewall rule for Web Deploy (8172)" -ForegroundColor Green
}

# Enable File and Printer Sharing for SMB transfers
Enable-NetFirewallRule -DisplayGroup "File and Printer Sharing" -ErrorAction SilentlyContinue
Write-Host "  Enabled File and Printer Sharing" -ForegroundColor Green

# Step 6: Summary
Write-Host ""
Write-Host "[6/6] Setup complete!" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Server Preparation Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "1. Install .NET 10.0 Hosting Bundle manually:" -ForegroundColor White
Write-Host "   https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Gray
Write-Host "   (Download 'Hosting Bundle' under ASP.NET Core Runtime)" -ForegroundColor Gray
Write-Host ""
Write-Host "2. After installing, restart IIS:" -ForegroundColor White
Write-Host "   iisreset /restart" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Run 02-create-website.ps1 to create the IIS site" -ForegroundColor White
Write-Host ""
Write-Host "Deployment credentials:" -ForegroundColor White
Write-Host "  Username: $DeployUsername" -ForegroundColor Gray
Write-Host "  Password: (as provided)" -ForegroundColor Gray
Write-Host "  Site Path: $sitePath" -ForegroundColor Gray
