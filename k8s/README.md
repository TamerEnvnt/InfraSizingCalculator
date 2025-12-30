# InfraSizing Calculator - Kubernetes Deployment Guide

Complete guide for deploying InfraSizing Calculator to Kubernetes using Kustomize.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Deployment Scenarios](#deployment-scenarios)
- [Configuration](#configuration)
- [Data Management](#data-management)
- [Monitoring](#monitoring)
- [Scaling and High Availability](#scaling-and-high-availability)
- [Troubleshooting](#troubleshooting)
- [Commands Reference](#commands-reference)

---

## Prerequisites

### Required Tools

| Tool | Version | Purpose |
|------|---------|---------|
| kubectl | 1.25+ | Kubernetes CLI |
| Docker | 20.10+ | Build container images |
| Kustomize | 4.0+ | Manifest customization (included in kubectl) |

### Installation

**kubectl:**
```bash
# macOS
brew install kubectl

# Linux
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
chmod +x kubectl && sudo mv kubectl /usr/local/bin/

# Windows (PowerShell)
choco install kubernetes-cli

# Verify
kubectl version --client
```

**Docker:**
```bash
# macOS
brew install --cask docker

# Linux
curl -fsSL https://get.docker.com | sh

# Verify
docker --version
```

### Kubernetes Cluster Options

| Platform | Best For | Setup |
|----------|----------|-------|
| **minikube** | Local development | `brew install minikube && minikube start` |
| **kind** | CI/CD, testing | `brew install kind && kind create cluster` |
| **k3s** | Lightweight, edge | `curl -sfL https://get.k3s.io \| sh -` |
| **Docker Desktop** | macOS/Windows dev | Enable Kubernetes in settings |
| **AKS/EKS/GKE** | Production | Cloud provider console |

### Cluster Requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| Nodes | 1 | 3+ |
| CPU per node | 2 cores | 4+ cores |
| RAM per node | 4 GB | 8+ GB |
| Storage | 10 GB | 50+ GB |

### Required Cluster Components

1. **Ingress Controller** (nginx recommended):
   ```bash
   # Install nginx ingress
   kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

   # Verify
   kubectl get pods -n ingress-nginx
   ```

2. **Storage Provisioner** (for PVC):
   ```bash
   # Most cloud providers have this by default
   # For minikube:
   minikube addons enable storage-provisioner

   # Verify
   kubectl get storageclass
   ```

---

## Quick Start

### Local Development (5 minutes)

```bash
# 1. Start local cluster (if needed)
minikube start

# 2. Build image locally
docker build -t infrasizing-calculator:dev .

# 3. Load image into cluster
minikube image load infrasizing-calculator:dev
# or for kind: kind load docker-image infrasizing-calculator:dev

# 4. Deploy
kubectl apply -k k8s/overlays/development

# 5. Wait for ready
kubectl -n infrasizing-dev wait --for=condition=ready pod -l app=infrasizing --timeout=120s

# 6. Access application
kubectl -n infrasizing-dev port-forward svc/infrasizing-dev 8080:80 &
open http://localhost:8080
```

### Production Deployment

```bash
# 1. Build and push to registry
docker build -t your-registry.com/infrasizing-calculator:1.0.0 .
docker push your-registry.com/infrasizing-calculator:1.0.0

# 2. Update production overlay
# Edit k8s/overlays/production/kustomization.yaml:
#   - Set your registry URL
#   - Set your domain name
#   - Set version tag

# 3. Deploy
kubectl apply -k k8s/overlays/production

# 4. Verify
kubectl -n infrasizing get pods,svc,ingress

# 5. Access via ingress
curl http://infrasizing.yourdomain.com/health
```

---

## Deployment Scenarios

### Scenario 1: Local Development with Minikube

```bash
# Start minikube with ingress
minikube start
minikube addons enable ingress

# Build and load image
docker build -t infrasizing-calculator:dev .
minikube image load infrasizing-calculator:dev

# Deploy development overlay
kubectl apply -k k8s/overlays/development

# Get minikube IP and access
minikube service infrasizing-dev -n infrasizing-dev --url
```

### Scenario 2: Staging Environment

Create a staging overlay:

```bash
mkdir -p k8s/overlays/staging
```

```yaml
# k8s/overlays/staging/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

namespace: infrasizing-staging

resources:
  - ../../base

nameSuffix: -staging

commonLabels:
  environment: staging

configMapGenerator:
  - name: infrasizing-config
    behavior: merge
    literals:
      - ASPNETCORE_ENVIRONMENT=Staging
      - Logging__LogLevel__Default=Information

images:
  - name: infrasizing-calculator
    newName: your-registry.com/infrasizing-calculator
    newTag: staging
```

Deploy:
```bash
kubectl apply -k k8s/overlays/staging
```

### Scenario 3: Production with TLS

1. Create TLS secret:
   ```bash
   kubectl -n infrasizing create secret tls infrasizing-tls \
     --cert=path/to/tls.crt \
     --key=path/to/tls.key
   ```

2. Uncomment TLS in production overlay or add patch:
   ```yaml
   # k8s/overlays/production/ingress-tls-patch.yaml
   apiVersion: networking.k8s.io/v1
   kind: Ingress
   metadata:
     name: infrasizing
   spec:
     tls:
       - hosts:
           - infrasizing.yourdomain.com
         secretName: infrasizing-tls
   ```

3. Update kustomization:
   ```yaml
   patches:
     - path: ingress-tls-patch.yaml
   ```

### Scenario 4: Multiple Environments in Same Cluster

```bash
# Deploy all environments
kubectl apply -k k8s/overlays/development   # -> infrasizing-dev namespace
kubectl apply -k k8s/overlays/staging       # -> infrasizing-staging namespace
kubectl apply -k k8s/overlays/production    # -> infrasizing namespace

# List all deployments
kubectl get deployments --all-namespaces -l app.kubernetes.io/name=infrasizing-calculator
```

---

## Configuration

### Manifest Structure

```
k8s/
├── base/                         # Shared base manifests
│   ├── namespace.yaml            # Namespace definition
│   ├── configmap.yaml            # Environment configuration
│   ├── pvc.yaml                  # Persistent storage claim
│   ├── deployment.yaml           # Pod specification
│   ├── service.yaml              # Internal service
│   ├── ingress.yaml              # External access
│   ├── servicemonitor.yaml       # Prometheus Operator (optional)
│   └── kustomization.yaml        # Base kustomization
└── overlays/
    ├── development/              # Dev environment settings
    │   └── kustomization.yaml
    └── production/               # Prod environment settings
        └── kustomization.yaml
```

### Environment Variables (ConfigMap)

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runtime environment |
| `ASPNETCORE_URLS` | `http://+:8080` | Listen URLs |
| `Database__AutoMigrate` | `true` | Auto-create database |
| `ConnectionStrings__DefaultConnection` | SQLite path | App database |
| `ConnectionStrings__IdentityConnection` | SQLite path | Identity database |
| `Security__EnableHttpsRedirection` | `false` | HTTPS redirect |
| `Security__EnableHsts` | `false` | HSTS header |
| `Security__EnableUpgradeInsecureRequests` | `false` | CSP directive |
| `Logging__LogLevel__Default` | `Information` | Log level |

### Customizing Configuration

**Method 1: Overlay ConfigMap merge**
```yaml
# In overlay kustomization.yaml
configMapGenerator:
  - name: infrasizing-config
    behavior: merge
    literals:
      - ASPNETCORE_ENVIRONMENT=Staging
      - Logging__LogLevel__Default=Debug
```

**Method 2: Environment-specific ConfigMap**
```yaml
# k8s/overlays/production/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: infrasizing-config
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Logging__LogLevel__Default: "Warning"
```

**Method 3: Secrets for sensitive data**
```bash
kubectl -n infrasizing create secret generic infrasizing-secrets \
  --from-literal=DatabasePassword=supersecret
```

### Resource Limits

Default limits in `deployment.yaml`:

| Environment | CPU Request | CPU Limit | Memory Request | Memory Limit |
|-------------|-------------|-----------|----------------|--------------|
| Development | 50m | 250m | 128Mi | 256Mi |
| Production | 200m | 1000m | 512Mi | 1Gi |

Customize via patches in overlays.

---

## Data Management

### Persistent Volume Claim

The application uses a PVC for SQLite database storage:

```yaml
spec:
  accessModes:
    - ReadWriteOnce    # Single pod access only
  resources:
    requests:
      storage: 1Gi     # Adjust as needed
```

### Backup Database

```bash
# Method 1: kubectl cp
POD=$(kubectl -n infrasizing get pod -l app=infrasizing -o jsonpath='{.items[0].metadata.name}')
kubectl -n infrasizing cp $POD:/app/data/infrasizing.db ./backup/infrasizing-$(date +%Y%m%d).db

# Method 2: Using a job
kubectl -n infrasizing apply -f - <<EOF
apiVersion: batch/v1
kind: Job
metadata:
  name: backup-db
spec:
  template:
    spec:
      containers:
      - name: backup
        image: alpine
        command: ["/bin/sh", "-c"]
        args:
          - cp /data/infrasizing.db /backup/infrasizing-$(date +%Y%m%d).db
        volumeMounts:
        - name: data
          mountPath: /data
        - name: backup
          mountPath: /backup
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: infrasizing-data
      - name: backup
        hostPath:
          path: /tmp/backup
      restartPolicy: Never
EOF
```

### Restore Database

```bash
# Scale down first
kubectl -n infrasizing scale deployment infrasizing --replicas=0

# Copy backup to PVC
POD_NAME="restore-pod"
kubectl -n infrasizing run $POD_NAME --image=alpine --restart=Never \
  --overrides='{"spec":{"volumes":[{"name":"data","persistentVolumeClaim":{"claimName":"infrasizing-data"}}],"containers":[{"name":"restore","image":"alpine","command":["sleep","3600"],"volumeMounts":[{"name":"data","mountPath":"/data"}]}]}}'

kubectl wait --for=condition=ready pod/$POD_NAME -n infrasizing
kubectl cp ./backup/infrasizing.db infrasizing/$POD_NAME:/data/infrasizing.db
kubectl -n infrasizing delete pod $POD_NAME

# Scale back up
kubectl -n infrasizing scale deployment infrasizing --replicas=1
```

### View Logs

```bash
# Current logs
kubectl -n infrasizing logs -l app=infrasizing

# Follow logs
kubectl -n infrasizing logs -f -l app=infrasizing

# Previous container logs (after crash)
kubectl -n infrasizing logs -l app=infrasizing --previous
```

---

## Monitoring

### Health Endpoints

| Endpoint | Probe Type | Purpose |
|----------|------------|---------|
| `/health/live` | Liveness | Is the app running? |
| `/health/ready` | Readiness | Can it handle traffic? |
| `/health` | N/A | Detailed health status |
| `/metrics` | N/A | Prometheus metrics |

### Prometheus Integration

**Option A: Prometheus Operator (ServiceMonitor)**
```bash
# If using Prometheus Operator
kubectl apply -f k8s/base/servicemonitor.yaml
```

**Option B: Pod Annotations (Standalone Prometheus)**

Already configured in deployment.yaml:
```yaml
annotations:
  prometheus.io/scrape: "true"
  prometheus.io/port: "8080"
  prometheus.io/path: "/metrics"
```

**Option C: Manual Prometheus Config**
```yaml
scrape_configs:
  - job_name: 'infrasizing'
    kubernetes_sd_configs:
      - role: pod
        namespaces:
          names: ['infrasizing']
    relabel_configs:
      - source_labels: [__meta_kubernetes_pod_label_app]
        regex: infrasizing
        action: keep
```

### Grafana Dashboard

Import these metrics for a dashboard:
- `http_request_duration_seconds` - Latency histogram
- `http_requests_received_total` - Request count
- `process_cpu_seconds_total` - CPU usage
- `process_working_set_bytes` - Memory usage

---

## Scaling and High Availability

### Current Limitations (SQLite)

The default deployment uses SQLite, which limits scaling:

| Aspect | Limitation | Impact |
|--------|------------|--------|
| Replicas | 1 only | No horizontal scaling |
| Strategy | Recreate | Brief downtime during updates |
| Storage | ReadWriteOnce | Single node access |

### Migration Path to PostgreSQL

For high availability, migrate to PostgreSQL:

1. **Deploy PostgreSQL:**
   ```bash
   # Using Helm
   helm install postgres bitnami/postgresql \
     --namespace infrasizing \
     --set auth.database=infrasizing \
     --set auth.username=infrasizing \
     --set auth.password=secure-password
   ```

2. **Update ConfigMap:**
   ```yaml
   ConnectionStrings__DefaultConnection: "Host=postgres-postgresql;Database=infrasizing;Username=infrasizing;Password=secure-password"
   ```

3. **Update Deployment:**
   ```yaml
   spec:
     replicas: 3
     strategy:
       type: RollingUpdate
   ```

4. **Configure HPA (Horizontal Pod Autoscaler):**
   ```yaml
   apiVersion: autoscaling/v2
   kind: HorizontalPodAutoscaler
   metadata:
     name: infrasizing-hpa
   spec:
     scaleTargetRef:
       apiVersion: apps/v1
       kind: Deployment
       name: infrasizing
     minReplicas: 2
     maxReplicas: 10
     metrics:
       - type: Resource
         resource:
           name: cpu
           target:
             type: Utilization
             averageUtilization: 70
   ```

### Blazor Server Considerations

Blazor Server requires sticky sessions for SignalR:

- **Ingress Affinity:** Already configured in `ingress.yaml`
- **Session Cookie:** `infrasizing-affinity`
- **Timeout:** 48 hours

If using a service mesh (Istio), configure destination rules for sticky sessions.

---

## Troubleshooting

### Pod Won't Start

```bash
# Check pod status
kubectl -n infrasizing get pods

# Describe pod for events
kubectl -n infrasizing describe pod <pod-name>

# Check logs
kubectl -n infrasizing logs <pod-name>

# Common causes:
# - ImagePullBackOff: Check registry credentials
# - CrashLoopBackOff: Check logs for startup errors
# - Pending: Check PVC or node resources
```

### Image Pull Errors

```bash
# Check image exists
docker pull your-registry.com/infrasizing-calculator:1.0.0

# Create image pull secret
kubectl -n infrasizing create secret docker-registry regcred \
  --docker-server=your-registry.com \
  --docker-username=user \
  --docker-password=password

# Add to deployment
spec:
  template:
    spec:
      imagePullSecrets:
        - name: regcred
```

### PVC Issues

```bash
# Check PVC status
kubectl -n infrasizing get pvc

# Describe PVC
kubectl -n infrasizing describe pvc infrasizing-data

# Check storage class
kubectl get storageclass

# Common issues:
# - Pending: No storage provisioner
# - Bound but pod pending: Wrong access mode
```

### Ingress Not Working

```bash
# Check ingress status
kubectl -n infrasizing get ingress
kubectl -n infrasizing describe ingress infrasizing

# Check ingress controller
kubectl get pods -n ingress-nginx
kubectl logs -n ingress-nginx -l app.kubernetes.io/name=ingress-nginx

# Test service directly
kubectl -n infrasizing port-forward svc/infrasizing 8080:80
curl http://localhost:8080/health

# Check DNS resolution
nslookup infrasizing.yourdomain.com
```

### Database Errors

```bash
# Check if database file exists
kubectl -n infrasizing exec <pod-name> -- ls -la /app/data/

# Check permissions
kubectl -n infrasizing exec <pod-name> -- stat /app/data/

# Check SQLite integrity
kubectl -n infrasizing exec <pod-name> -- sqlite3 /app/data/infrasizing.db "PRAGMA integrity_check;"
```

### Resource Issues

```bash
# Check node resources
kubectl top nodes

# Check pod resources
kubectl top pods -n infrasizing

# Describe node for resource pressure
kubectl describe node <node-name>
```

---

## Commands Reference

### Deployment

| Task | Command |
|------|---------|
| Deploy (dev) | `kubectl apply -k k8s/overlays/development` |
| Deploy (prod) | `kubectl apply -k k8s/overlays/production` |
| Remove (dev) | `kubectl delete -k k8s/overlays/development` |
| Remove (prod) | `kubectl delete -k k8s/overlays/production` |
| Preview manifests | `kubectl kustomize k8s/overlays/production` |

### Monitoring

| Task | Command |
|------|---------|
| Get pods | `kubectl -n infrasizing get pods` |
| Get all resources | `kubectl -n infrasizing get all` |
| Watch pods | `kubectl -n infrasizing get pods -w` |
| Pod details | `kubectl -n infrasizing describe pod <name>` |
| View logs | `kubectl -n infrasizing logs -l app=infrasizing` |
| Follow logs | `kubectl -n infrasizing logs -f -l app=infrasizing` |
| Resource usage | `kubectl top pods -n infrasizing` |

### Debugging

| Task | Command |
|------|---------|
| Shell access | `kubectl -n infrasizing exec -it <pod> -- sh` |
| Port forward | `kubectl -n infrasizing port-forward svc/infrasizing 8080:80` |
| Copy files out | `kubectl -n infrasizing cp <pod>:/app/data/file.db ./local.db` |
| Copy files in | `kubectl -n infrasizing cp ./local.db <pod>:/app/data/file.db` |

### Scaling

| Task | Command |
|------|---------|
| Scale replicas | `kubectl -n infrasizing scale deployment infrasizing --replicas=3` |
| Restart pods | `kubectl -n infrasizing rollout restart deployment infrasizing` |
| Rollback | `kubectl -n infrasizing rollout undo deployment infrasizing` |
| Rollout status | `kubectl -n infrasizing rollout status deployment infrasizing` |

### Cleanup

| Task | Command |
|------|---------|
| Delete deployment | `kubectl delete -k k8s/overlays/<env>` |
| Delete PVC | `kubectl -n infrasizing delete pvc infrasizing-data` |
| Delete namespace | `kubectl delete namespace infrasizing` |
