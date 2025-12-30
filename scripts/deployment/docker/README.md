# InfraSizing Calculator - Docker Deployment Guide

Complete guide for deploying InfraSizing Calculator using Docker and Docker Compose.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Deployment Scenarios](#deployment-scenarios)
- [Configuration](#configuration)
- [Data Management](#data-management)
- [Monitoring](#monitoring)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| Docker | 20.10+ | Container runtime |
| Docker Compose | 2.0+ | Multi-container orchestration |

### Installation

**macOS:**
```bash
# Install Docker Desktop (includes Compose)
brew install --cask docker

# Verify installation
docker --version
docker compose version
```

**Linux (Ubuntu/Debian):**
```bash
# Install Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Install Docker Compose plugin
sudo apt-get install docker-compose-plugin

# Verify (log out and back in first)
docker --version
docker compose version
```

**Windows:**
1. Download Docker Desktop from https://www.docker.com/products/docker-desktop
2. Run installer and follow prompts
3. Restart computer if prompted
4. Verify in PowerShell:
   ```powershell
   docker --version
   docker compose version
   ```

### System Requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| CPU | 1 core | 2+ cores |
| RAM | 512 MB | 1 GB |
| Disk | 1 GB | 5 GB |

---

## Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# Clone repository (if not already done)
git clone https://github.com/your-org/InfraSizingCalculator.git
cd InfraSizingCalculator

# Start the application
docker compose up -d

# View logs
docker compose logs -f

# Access application
open http://localhost:8080
```

### Option 2: Docker Run

```bash
# Build the image
docker build -t infrasizing-calculator:latest .

# Create a volume for data persistence
docker volume create infrasizing-data

# Run the container
docker run -d \
  --name infrasizing \
  -p 8080:8080 \
  -v infrasizing-data:/app/data \
  -e ASPNETCORE_ENVIRONMENT=Production \
  infrasizing-calculator:latest

# Access application
open http://localhost:8080
```

---

## Deployment Scenarios

### Scenario 1: Local Development

Development mode with debug logging and local file mounts:

```bash
# Uses docker-compose.override.yml automatically
docker compose up -d

# View detailed logs
docker compose logs -f infrasizing
```

The override file enables:
- Development environment
- Debug logging
- Local directory mounts for easy debugging

### Scenario 2: Production on Single Server

```bash
# Use production settings only (ignore override)
docker compose -f docker-compose.yml up -d

# Or set environment variable
export ASPNETCORE_ENVIRONMENT=Production
docker compose up -d
```

### Scenario 3: With Prometheus Monitoring

```bash
# Start with monitoring profile
docker compose --profile monitoring up -d

# Access:
# - Application: http://localhost:8080
# - Prometheus: http://localhost:9090
# - Metrics: http://localhost:8080/metrics
```

### Scenario 4: Custom Port

```bash
# Change host port
APP_PORT=3000 docker compose up -d

# Access at http://localhost:3000
```

### Scenario 5: Multiple Instances

```bash
# Create separate compose files or use project names
docker compose -p infrasizing-dev up -d
docker compose -p infrasizing-staging up -d
```

---

## Configuration

### Environment Variables

Configure the application using environment variables in `docker-compose.yml` or via `-e` flags:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runtime environment (`Development`, `Staging`, `Production`) |
| `ASPNETCORE_URLS` | `http://+:8080` | URLs to listen on |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/infrasizing.db` | App database path |
| `ConnectionStrings__IdentityConnection` | `Data Source=/app/data/identity.db` | Identity database path |
| `Database__AutoMigrate` | `true` | Auto-create database on startup |
| `Security__EnableHttpsRedirection` | `false` | HTTPS redirect (behind proxy) |
| `Security__EnableHsts` | `false` | HTTP Strict Transport Security |
| `Security__EnableUpgradeInsecureRequests` | `false` | CSP upgrade directive |
| `Logging__LogLevel__Default` | `Information` | Default log level |

### Custom Configuration

Create a `.env` file in the project root:

```env
# .env file
ASPNETCORE_ENVIRONMENT=Staging
APP_PORT=3000
PROMETHEUS_PORT=9091
```

Then run:
```bash
docker compose up -d
```

### Using appsettings Override

Mount a custom appsettings file:

```yaml
# In docker-compose.yml
services:
  infrasizing:
    volumes:
      - ./my-appsettings.json:/app/appsettings.Production.json:ro
```

---

## Data Management

### Volume Locations

| Volume | Container Path | Purpose |
|--------|----------------|---------|
| `infrasizing-data` | `/app/data` | SQLite databases |
| `infrasizing-logs` | `/app/logs` | Application logs |

### Backup Database

```bash
# Method 1: Copy from running container
docker cp infrasizing:/app/data/infrasizing.db ./backup/infrasizing-$(date +%Y%m%d).db

# Method 2: Copy from volume (container stopped)
docker run --rm \
  -v infrasizing-data:/data \
  -v $(pwd)/backup:/backup \
  alpine cp /data/infrasizing.db /backup/

# Method 3: Using docker compose
docker compose exec infrasizing cat /app/data/infrasizing.db > ./backup/infrasizing.db
```

### Restore Database

```bash
# Method 1: Copy to running container
docker cp ./backup/infrasizing.db infrasizing:/app/data/infrasizing.db
docker restart infrasizing

# Method 2: Copy to volume
docker compose down
docker run --rm \
  -v infrasizing-data:/data \
  -v $(pwd)/backup:/backup \
  alpine cp /backup/infrasizing.db /data/
docker compose up -d
```

### Reset Database

```bash
# Remove data volume (WARNING: deletes all data)
docker compose down -v

# Restart with fresh database
docker compose up -d
```

### View Logs

```bash
# Live logs
docker compose logs -f infrasizing

# Last 100 lines
docker compose logs --tail 100 infrasizing

# Logs from volume
docker compose exec infrasizing ls -la /app/logs
docker compose exec infrasizing cat /app/logs/infrasizing-*.log
```

---

## Monitoring

### Health Checks

The container includes health checks. View status:

```bash
# Container health
docker inspect infrasizing --format='{{.State.Health.Status}}'

# Detailed health info
docker inspect infrasizing --format='{{json .State.Health}}' | jq

# Manual health check
curl http://localhost:8080/health
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
```

### Prometheus Metrics

Enable Prometheus monitoring:

```bash
docker compose --profile monitoring up -d
```

**Access Points:**
- Application metrics: http://localhost:8080/metrics
- Prometheus UI: http://localhost:9090

**Available Metrics:**
- `http_request_duration_seconds` - Request latency
- `http_requests_received_total` - Request count
- `infrasizing_calculations_total` - Sizing calculations performed
- Standard .NET metrics (GC, thread pool, etc.)

### Resource Usage

```bash
# Live resource stats
docker stats infrasizing

# One-time snapshot
docker stats --no-stream infrasizing
```

---

## Production Deployment

### Pre-Deployment Checklist

- [ ] Docker and Docker Compose installed
- [ ] Sufficient disk space (5GB+ recommended)
- [ ] Firewall allows port 8080 (or custom port)
- [ ] Backup strategy defined
- [ ] Monitoring configured (optional)

### Deployment Steps

1. **Transfer files to server:**
   ```bash
   # Option A: Clone repository
   git clone https://github.com/your-org/InfraSizingCalculator.git

   # Option B: Copy files (SCP)
   scp -r ./InfraSizingCalculator user@server:/opt/
   ```

2. **Build the image:**
   ```bash
   cd /opt/InfraSizingCalculator
   docker compose build
   ```

3. **Start the application:**
   ```bash
   docker compose -f docker-compose.yml up -d
   ```

4. **Verify deployment:**
   ```bash
   docker compose ps
   curl http://localhost:8080/health
   ```

### HTTPS with Reverse Proxy

For production, use a reverse proxy (nginx, Traefik, Caddy) for HTTPS:

**nginx example (`/etc/nginx/sites-available/infrasizing`):**

```nginx
server {
    listen 443 ssl http2;
    server_name infrasizing.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/infrasizing.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/infrasizing.yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # WebSocket support for Blazor Server
        proxy_read_timeout 86400;
    }
}

server {
    listen 80;
    server_name infrasizing.yourdomain.com;
    return 301 https://$server_name$request_uri;
}
```

### Auto-Start on Boot

Docker Desktop handles this automatically. For Linux servers:

```bash
# Enable Docker service
sudo systemctl enable docker

# Containers with restart: unless-stopped will auto-start
# This is already set in docker-compose.yml
```

### Updates and Redeployment

```bash
# Pull latest code
cd /opt/InfraSizingCalculator
git pull

# Rebuild and restart
docker compose build
docker compose up -d

# Or one command
docker compose up -d --build
```

---

## Troubleshooting

### Container Won't Start

```bash
# Check container status
docker compose ps -a

# View startup logs
docker compose logs infrasizing

# Check for port conflicts
lsof -i :8080
# or
netstat -tlnp | grep 8080
```

### Application Errors

```bash
# View application logs
docker compose logs -f infrasizing

# Access container shell
docker compose exec infrasizing sh

# Check environment variables
docker compose exec infrasizing env | grep -E "ASPNET|Connection|Database"
```

### Database Issues

```bash
# Check database file exists
docker compose exec infrasizing ls -la /app/data/

# Check permissions
docker compose exec infrasizing stat /app/data/infrasizing.db

# SQLite integrity check
docker compose exec infrasizing sqlite3 /app/data/infrasizing.db "PRAGMA integrity_check;"
```

### Performance Issues

```bash
# Check resource usage
docker stats infrasizing

# Check memory limits
docker inspect infrasizing --format='{{.HostConfig.Memory}}'

# Increase memory if needed (in docker-compose.yml)
services:
  infrasizing:
    deploy:
      resources:
        limits:
          memory: 1G
```

### Network Issues

```bash
# Check container IP
docker inspect infrasizing --format='{{.NetworkSettings.IPAddress}}'

# Test from inside container
docker compose exec infrasizing curl -v http://localhost:8080/health

# Check port mapping
docker port infrasizing
```

### Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `port is already allocated` | Port 8080 in use | Change `APP_PORT` or stop conflicting service |
| `no space left on device` | Disk full | `docker system prune` or add disk space |
| `Cannot connect to Docker daemon` | Docker not running | Start Docker Desktop or `sudo systemctl start docker` |
| `database is locked` | Concurrent access | Ensure single container instance |
| `permission denied` | Volume permissions | Check file ownership, run as correct user |

### Reset Everything

```bash
# Stop and remove containers, networks, volumes
docker compose down -v

# Remove image
docker rmi infrasizing-calculator:latest

# Rebuild from scratch
docker compose build --no-cache
docker compose up -d
```

---

## Commands Reference

| Task | Command |
|------|---------|
| Start | `docker compose up -d` |
| Stop | `docker compose down` |
| Restart | `docker compose restart` |
| View logs | `docker compose logs -f` |
| Shell access | `docker compose exec infrasizing sh` |
| Build image | `docker compose build` |
| Rebuild | `docker compose up -d --build` |
| Status | `docker compose ps` |
| Resource usage | `docker stats infrasizing` |
| Health check | `curl http://localhost:8080/health` |
| Remove all | `docker compose down -v --rmi all` |
