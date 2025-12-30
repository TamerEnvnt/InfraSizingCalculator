# InfraSizing Calculator - Deployment Guide

This guide covers deploying the InfraSizing Calculator to various platforms.

## Table of Contents

- [Deployment Options](#deployment-options)
- [IIS Deployment](#iis-deployment)
  - [Prerequisites](#prerequisites)
  - [Quick Start](#quick-start)
  - [Detailed Instructions](#detailed-instructions)
- [Docker Deployment](#docker-deployment)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)

---

## Deployment Options

| Platform | Best For | Complexity | Full Guide |
|----------|----------|------------|------------|
| **IIS** | Windows Server, traditional hosting | Medium | [This document](#iis-deployment) |
| **Docker** | Local dev, single-server deployment | Low | [Docker Guide](docker/README.md) |
| **Kubernetes** | Production, scalability, cloud-native | High | [Kubernetes Guide](../../k8s/README.md) |

### Quick Decision Guide

```
Need to deploy quickly for testing?
  → Docker (docker compose up -d)

Deploying to existing Windows Server infrastructure?
  → IIS (use scripts in this directory)

Need scalability, HA, or cloud-native deployment?
  → Kubernetes (use manifests in k8s/)

Running on-premises with limited resources?
  → Docker or IIS (both work well)
```

---

## IIS Deployment

## Prerequisites

### On Development Machine (Mac/Linux/Windows)

- .NET SDK 10.0 or later
- Git (for source control)
- zip command (Mac/Linux) or 7-Zip (Windows)

### On Windows Server

- Windows Server 2016 or later
- Administrator access
- Network access from development machine

### Server Software (installed by scripts)

- IIS with WebSocket support
- .NET 10.0 Hosting Bundle
- Web Management Service (for remote management)

## Deployment Scenarios

| Scenario | Scripts to Run |
|----------|----------------|
| New server setup | `01-prepare-server.ps1` then `02-create-website.ps1` |
| New website on existing server | `02-create-website.ps1` only |
| Redeploy to existing site | `03-deploy.ps1` only |

## Quick Start

### First-Time Deployment to New Server

**On your Mac (build machine):**

```bash
cd /path/to/InfraSizingCalculator
./scripts/deployment/build-and-package.sh
```

**Transfer the ZIP file to the server via SMB:**

```bash
# Connect to server share (Finder: Cmd+K)
smb://192.168.10.155/c$

# Copy publish/InfraSizing-latest.zip to C:\deploy\ on server
```

**On Windows Server (as Administrator):**

```powershell
# Step 1: Prepare server (first time only)
.\01-prepare-server.ps1 -DeployPassword "YourSecurePassword123!"

# Step 2: Install .NET Hosting Bundle manually
# Download from: https://dotnet.microsoft.com/download/dotnet/10.0
# Run the Hosting Bundle installer, then restart IIS:
iisreset /restart

# Step 3: Create the IIS website
.\02-create-website.ps1 -SitePort 8080

# Step 4: Deploy the application
.\03-deploy.ps1 -SourcePath "C:\deploy\InfraSizing-latest.zip"
```

**Access the application:**
- http://server-ip:8080

### Redeployment (After Initial Setup)

**On Mac:**
```bash
./scripts/deployment/build-and-package.sh
# Copy ZIP to server
```

**On Server:**
```powershell
.\03-deploy.ps1 -SourcePath "C:\deploy\InfraSizing-latest.zip"
```

## Detailed Instructions

### 1. Server Preparation (`01-prepare-server.ps1`)

Run once per server. This script:

- Installs IIS with required features
- Enables Web Management Service
- Creates a deployment user account
- Sets up firewall rules
- Creates the site directory with permissions

**Parameters:**

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-DeployUsername` | `deploy-infrasizing` | Username for deployment account |
| `-DeployPassword` | (required) | Password for deployment account |
| `-SitePort` | `8080` | Port for firewall rule |

**Example:**
```powershell
.\01-prepare-server.ps1 -DeployPassword "Str0ng!Pass#2024"
```

**After running**, manually install the .NET Hosting Bundle:
1. Download from https://dotnet.microsoft.com/download/dotnet/10.0
2. Choose "Hosting Bundle" under ASP.NET Core Runtime
3. Run the installer
4. Restart IIS: `iisreset /restart`

### 2. Website Creation (`02-create-website.ps1`)

Creates an IIS website and application pool. Run once per website.

**Parameters:**

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-SiteName` | `InfraSizing` | IIS site and app pool name |
| `-SitePort` | `8080` | HTTP port to bind |
| `-SitePath` | `C:\inetpub\InfraSizing` | Physical path for files |
| `-Environment` | `Production` | ASP.NET Core environment |

**Examples:**

```powershell
# Default production site
.\02-create-website.ps1

# Development instance on different port
.\02-create-website.ps1 -SiteName "InfraSizing-Dev" -SitePort 8081 -Environment "Development"

# Staging environment
.\02-create-website.ps1 -SiteName "InfraSizing-Staging" -SitePort 8082 -Environment "Staging"
```

### 3. Deployment (`03-deploy.ps1`)

Deploys application files to an existing IIS site.

**Parameters:**

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-SourcePath` | (required) | ZIP file or folder with app files |
| `-SitePath` | `C:\inetpub\InfraSizing` | Target directory |
| `-SiteName` | `InfraSizing` | IIS site/app pool name |
| `-BackupExisting` | `$true` | Create backup before deploy |
| `-PreserveDatabase` | `$true` | Keep SQLite database files |

**Examples:**

```powershell
# Deploy from ZIP (most common)
.\03-deploy.ps1 -SourcePath "C:\deploy\InfraSizing-latest.zip"

# Deploy from network share
.\03-deploy.ps1 -SourcePath "\\fileserver\deploy\InfraSizing.zip"

# Deploy without backup (faster)
.\03-deploy.ps1 -SourcePath "C:\deploy\InfraSizing.zip" -BackupExisting $false

# Fresh deploy (don't preserve database)
.\03-deploy.ps1 -SourcePath "C:\deploy\InfraSizing.zip" -PreserveDatabase $false
```

### 4. Build and Package (`build-and-package.sh`)

Builds the application on Mac/Linux and creates a deployment package.

**Options:**

| Option | Default | Description |
|--------|---------|-------------|
| `-o` | `./publish` | Output directory |
| `-c` | `Release` | Build configuration |
| `-n` | `InfraSizing` | Package name prefix |

**Examples:**

```bash
# Default build
./scripts/deployment/build-and-package.sh

# Build to Desktop
./scripts/deployment/build-and-package.sh -o ~/Desktop/deploy

# Development build
./scripts/deployment/build-and-package.sh -c Development
```

**Output:**
- `publish/InfraSizing-YYYYMMDD-HHMMSS.zip` - Timestamped package
- `publish/InfraSizing-latest.zip` - Latest build (for automation)
- `publish/app/` - Unpacked files (if needed for debugging)

## Configuration

### Application Settings

The application reads settings from `appsettings.json` and environment-specific files like `appsettings.Production.json`.

**Key settings for deployment:**

```json
{
  "AllowedHosts": "*",
  "Security": {
    "EnableHttpsRedirection": false,
    "EnableHsts": false,
    "EnableUpgradeInsecureRequests": false
  },
  "Database": {
    "ConnectionString": "Data Source=infrasizing.db"
  },
  "Database:AutoMigrate": true
}
```

### HTTPS Configuration

For HTTPS deployment, update `appsettings.Production.json`:

```json
{
  "Security": {
    "EnableHttpsRedirection": true,
    "EnableHsts": true,
    "EnableUpgradeInsecureRequests": true
  }
}
```

And configure SSL in IIS:
1. Add HTTPS binding (port 443) with SSL certificate
2. Optionally keep HTTP binding for redirect

### Environment Variables

Set via IIS Application Pool or `web.config`:

| Variable | Values | Description |
|----------|--------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development`, `Staging`, `Production` | Determines which appsettings file to use |

## Troubleshooting

### Common Issues

#### HTTP 500.30 - ASP.NET Core app failed to start

**Cause:** Missing .NET runtime or hosting bundle

**Solution:**
1. Verify .NET Hosting Bundle is installed:
   ```powershell
   dotnet --list-runtimes
   ```
   Should show `Microsoft.AspNetCore.App 10.x.x`

2. If missing, install from https://dotnet.microsoft.com/download/dotnet/10.0

3. Restart IIS: `iisreset /restart`

#### SQLite Error 14 - Unable to open database

**Cause:** IIS_IUSRS lacks write permission

**Solution:**
```powershell
icacls "C:\inetpub\InfraSizing" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

#### Bad Request - Invalid Hostname

**Cause:** `AllowedHosts` in appsettings doesn't include server IP/hostname

**Solution:** Set `AllowedHosts` to `"*"` or add specific hosts:
```json
"AllowedHosts": "localhost;192.168.10.155;server.domain.com"
```

#### CSS/JS Not Loading (ERR_SSL_PROTOCOL_ERROR)

**Cause:** `upgrade-insecure-requests` CSP directive forcing HTTPS

**Solution:** For HTTP deployment, ensure:
```json
"Security": {
  "EnableUpgradeInsecureRequests": false
}
```

#### Application Pool Stops Immediately

**Cause:** Usually a startup error

**Solution:**
1. Check stdout logs in `C:\inetpub\InfraSizing\logs\`
2. Enable detailed errors in web.config:
   ```xml
   <aspNetCore stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />
   ```

### Log Locations

| Log Type | Location |
|----------|----------|
| Application logs | `C:\inetpub\InfraSizing\logs\` |
| IIS logs | `C:\inetpub\logs\LogFiles\` |
| Event Viewer | Windows Logs > Application |

### Useful Commands

```powershell
# Check app pool status
Get-IISAppPool -Name InfraSizing

# Restart app pool
Restart-WebAppPool -Name InfraSizing

# Check site bindings
Get-Website -Name InfraSizing | Select-Object -ExpandProperty Bindings

# View recent IIS logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC*\*.log" -Tail 50

# Test if port is listening
Test-NetConnection -ComputerName localhost -Port 8080
```

## Security Considerations

### Production Checklist

- [ ] Change default deployment password
- [ ] Remove or restrict the deployment user after setup
- [ ] Configure HTTPS with valid SSL certificate
- [ ] Enable HSTS after HTTPS is working
- [ ] Review and restrict CORS origins
- [ ] Set up regular backups of database
- [ ] Configure appropriate firewall rules
- [ ] Review IIS application pool identity

### HTTP vs HTTPS

The default configuration supports HTTP deployment for internal/development use. For production:

1. Obtain SSL certificate (Let's Encrypt, commercial CA, or internal PKI)
2. Configure IIS HTTPS binding
3. Enable security settings in appsettings
4. Test thoroughly before enabling HSTS

---

## Docker Deployment

Docker provides a simpler deployment option. **Full guide: [docker/README.md](docker/README.md)**

### Quick Start

```bash
# Start with Docker Compose
docker compose up -d

# Access at http://localhost:8080

# With monitoring (Prometheus)
docker compose --profile monitoring up -d
```

### Key Commands

| Task | Command |
|------|---------|
| Start | `docker compose up -d` |
| Stop | `docker compose down` |
| View logs | `docker compose logs -f` |
| Rebuild | `docker compose up -d --build` |
| Reset data | `docker compose down -v` |

For complete Docker documentation including configuration, data management, HTTPS setup, and troubleshooting, see **[docker/README.md](docker/README.md)**.

---

## Kubernetes Deployment

For production workloads requiring scalability. **Full guide: [k8s/README.md](../../k8s/README.md)**

### Quick Start

```bash
# Local development (minikube)
docker build -t infrasizing-calculator:dev .
minikube image load infrasizing-calculator:dev
kubectl apply -k k8s/overlays/development
kubectl -n infrasizing-dev port-forward svc/infrasizing-dev 8080:80

# Production
kubectl apply -k k8s/overlays/production
```

### Key Commands

| Task | Command |
|------|---------|
| Deploy (dev) | `kubectl apply -k k8s/overlays/development` |
| Deploy (prod) | `kubectl apply -k k8s/overlays/production` |
| Get pods | `kubectl -n infrasizing get pods` |
| View logs | `kubectl -n infrasizing logs -l app=infrasizing` |
| Remove | `kubectl delete -k k8s/overlays/<env>` |

### Important Notes

- **SQLite limits to single replica** - Use PostgreSQL for HA
- **Sticky sessions required** - Configured in ingress for Blazor Server
- **Kustomize overlays** - Dev, staging, production environments

For complete Kubernetes documentation including cluster setup, configuration, scaling, and troubleshooting, see **[k8s/README.md](../../k8s/README.md)**.

---

## Support

For issues with deployment scripts, check:
1. This README troubleshooting section
2. Application logs in the `logs` directory
3. Windows Event Viewer for system-level errors (IIS)
4. `docker compose logs` for Docker deployments
5. `kubectl logs` for Kubernetes deployments
