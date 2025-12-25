# REST API Reference

This document describes the REST API endpoints for the Infrastructure Sizing Calculator.

---

## Base URL

```
http://localhost:5062/api
```

---

## Kubernetes Sizing API

### Calculate K8s Sizing

Calculate infrastructure requirements for Kubernetes cluster deployment.

**Endpoint:** `POST /api/k8s/calculate`

**Request Body:**
```json
{
  "distribution": "OpenShift",
  "technology": "DotNet",
  "clusterMode": "MultiCluster",
  "enabledEnvironments": ["Dev", "Test", "Prod"],
  "environmentApps": {
    "Dev": { "small": 5, "medium": 10, "large": 2, "xLarge": 0 },
    "Test": { "small": 5, "medium": 10, "large": 2, "xLarge": 0 },
    "Prod": { "small": 10, "medium": 20, "large": 5, "xLarge": 1 }
  },
  "replicas": {
    "prod": 3,
    "nonProd": 1,
    "stage": 2
  },
  "headroom": {
    "dev": 33.0,
    "test": 33.0,
    "stage": 0.0,
    "prod": 37.5,
    "dr": 37.5
  },
  "headroomEnabled": true,
  "prodOvercommit": { "cpu": 1.0, "memory": 1.0 },
  "nonProdOvercommit": { "cpu": 1.0, "memory": 1.0 }
}
```

**Response (200 OK):**
```json
{
  "environments": [
    {
      "environment": "Dev",
      "isProd": false,
      "apps": 17,
      "replicas": 1,
      "pods": 17,
      "masters": 3,
      "infra": 3,
      "workers": 3,
      "totalNodes": 9,
      "totalCpu": 72,
      "totalRam": 288,
      "totalDisk": 900
    },
    {
      "environment": "Prod",
      "isProd": true,
      "apps": 36,
      "replicas": 3,
      "pods": 108,
      "masters": 3,
      "infra": 3,
      "workers": 5,
      "totalNodes": 11,
      "totalCpu": 88,
      "totalRam": 352,
      "totalDisk": 1100
    }
  ],
  "grandTotal": {
    "totalNodes": 29,
    "totalMasters": 9,
    "totalInfra": 9,
    "totalWorkers": 11,
    "totalCpu": 232,
    "totalRam": 928,
    "totalDisk": 2900
  },
  "configuration": { /* original input */ },
  "nodeSpecs": { /* distribution config used */ },
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "ValidationError",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": [
    {
      "field": "EnabledEnvironments",
      "message": "Production environment must always be enabled (BR-E002)"
    }
  ]
}
```

---

### Validate K8s Input

Validate input without performing calculation.

**Endpoint:** `POST /api/k8s/validate`

**Request Body:** Same as calculate

**Response (200 OK):**
```json
{
  "valid": true,
  "message": "Input is valid"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "ValidationError",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": [
    {
      "field": "Replicas.Prod",
      "message": "Prod replica count must be between 1 and 10 (BR-R004)"
    }
  ]
}
```

---

## VM Sizing API

### Calculate VM Sizing

Calculate infrastructure requirements for VM deployment.

**Endpoint:** `POST /api/vm/calculate`

**Request Body:**
```json
{
  "technology": "OutSystems",
  "enabledEnvironments": ["Dev", "Prod"],
  "environmentConfigs": {
    "Dev": {
      "environment": "Dev",
      "enabled": true,
      "roles": [
        {
          "roleId": "os-controller",
          "roleName": "Deployment Controller",
          "size": "Large",
          "instanceCount": 1,
          "diskGB": 200
        },
        {
          "roleId": "os-frontend",
          "roleName": "Front-End Server",
          "size": "Medium",
          "instanceCount": 2,
          "diskGB": 100
        },
        {
          "roleId": "os-db",
          "roleName": "Database Server",
          "size": "Large",
          "instanceCount": 1,
          "diskGB": 500
        }
      ],
      "haPattern": "None",
      "drPattern": "None",
      "loadBalancer": "Single"
    },
    "Prod": {
      "environment": "Prod",
      "enabled": true,
      "roles": [
        {
          "roleId": "os-controller",
          "roleName": "Deployment Controller",
          "size": "Large",
          "instanceCount": 1,
          "diskGB": 200
        },
        {
          "roleId": "os-frontend",
          "roleName": "Front-End Server",
          "size": "Large",
          "instanceCount": 4,
          "diskGB": 100
        },
        {
          "roleId": "os-db",
          "roleName": "Database Server",
          "size": "XLarge",
          "instanceCount": 2,
          "diskGB": 1000
        }
      ],
      "haPattern": "ActivePassive",
      "drPattern": "WarmStandby",
      "loadBalancer": "HAPair"
    }
  },
  "systemOverheadPercent": 10.0
}
```

**Response (200 OK):**
```json
{
  "environments": [
    {
      "environment": "Dev",
      "isProd": false,
      "haPattern": "None",
      "drPattern": "None",
      "loadBalancer": "Single",
      "roles": [
        {
          "role": "Controller",
          "roleName": "Deployment Controller",
          "size": "Large",
          "baseInstances": 1,
          "totalInstances": 1,
          "cpuPerInstance": 4,
          "ramPerInstance": 12,
          "diskPerInstance": 200,
          "totalCpu": 4,
          "totalRam": 12,
          "totalDisk": 200
        }
      ],
      "totalVMs": 5,
      "totalCpu": 16,
      "totalRam": 40,
      "totalDisk": 1000,
      "loadBalancerVMs": 1,
      "loadBalancerCpu": 2,
      "loadBalancerRam": 4
    }
  ],
  "grandTotal": {
    "totalVMs": 20,
    "totalCpu": 80,
    "totalRam": 200,
    "totalDisk": 5000,
    "totalLoadBalancerVMs": 3
  },
  "configuration": { /* original input */ },
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

---

### Validate VM Input

Validate input without performing calculation.

**Endpoint:** `POST /api/vm/validate`

**Request/Response:** Same pattern as K8s validate

---

### Get Role Specs

Get CPU and RAM specs for a role at specified size.

**Endpoint:** `GET /api/vm/specs/{role}/{size}?technology={technology}`

**Parameters:**
- `role` (path): ServerRole enum value
- `size` (path): AppTier enum value
- `technology` (query, optional): Technology enum value (default: DotNet)

**Response (200 OK):**
```json
{
  "role": "AppServer",
  "size": "Large",
  "technology": "Java",
  "cpu": 4,
  "ram": 12
}
```

---

### Get HA Multiplier

Get instance multiplier for an HA pattern.

**Endpoint:** `GET /api/vm/ha-multiplier/{pattern}`

**Parameters:**
- `pattern` (path): HAPattern enum value

**Response (200 OK):**
```json
{
  "pattern": "ActiveActive",
  "multiplier": 2.0
}
```

---

### Get Load Balancer Specs

Get specifications for a load balancer option.

**Endpoint:** `GET /api/vm/lb-specs/{option}`

**Parameters:**
- `option` (path): LoadBalancerOption enum value

**Response (200 OK):**
```json
{
  "option": "HAPair",
  "vms": 2,
  "cpuPerVm": 2,
  "ramPerVm": 4
}
```

---

## Technologies API

### List All Technologies

Get all available technology configurations.

**Endpoint:** `GET /api/technologies`

**Response (200 OK):**
```json
[
  {
    "technology": "DotNet",
    "name": ".NET",
    "icon": "🔷",
    "platformType": "Native",
    "tierSpecs": {
      "Small": { "cpu": 0.25, "ram": 0.5 },
      "Medium": { "cpu": 0.5, "ram": 1.0 },
      "Large": { "cpu": 1.0, "ram": 2.0 },
      "XLarge": { "cpu": 2.0, "ram": 4.0 }
    }
  },
  {
    "technology": "Java",
    "name": "Java",
    "icon": "☕",
    "platformType": "Native",
    "tierSpecs": { /* ... */ }
  }
]
```

---

### Get Technology

Get a specific technology configuration.

**Endpoint:** `GET /api/technologies/{technology}`

**Parameters:**
- `technology` (path): Technology enum value

**Response (200 OK):**
```json
{
  "technology": "Java",
  "name": "Java",
  "icon": "☕",
  "platformType": "Native",
  "tierSpecs": {
    "Small": { "cpu": 0.5, "ram": 1.0 },
    "Medium": { "cpu": 1.0, "ram": 2.0 },
    "Large": { "cpu": 2.0, "ram": 4.0 },
    "XLarge": { "cpu": 4.0, "ram": 8.0 }
  }
}
```

**Error Response (404 Not Found):**
```json
"Technology 'Unknown' not found"
```

---

## Distributions API

### List All Distributions

Get all available Kubernetes distributions.

**Endpoint:** `GET /api/distributions`

**Response (200 OK):**
```json
[
  {
    "distribution": "OpenShift",
    "name": "Red Hat OpenShift",
    "category": "enterprise",
    "hasManagedControlPlane": false,
    "hasInfraNodes": true,
    "prodWorkerSpecs": { "cpu": 16, "ram": 64, "disk": 200 },
    "nonProdWorkerSpecs": { "cpu": 8, "ram": 32, "disk": 100 }
  },
  {
    "distribution": "EKS",
    "name": "Amazon EKS",
    "category": "managed",
    "hasManagedControlPlane": true,
    "hasInfraNodes": false,
    "prodWorkerSpecs": { "cpu": 8, "ram": 32, "disk": 100 },
    "nonProdWorkerSpecs": { "cpu": 4, "ram": 16, "disk": 50 }
  }
]
```

---

### Get Distribution

Get a specific distribution configuration.

**Endpoint:** `GET /api/distributions/{distribution}`

**Parameters:**
- `distribution` (path): Distribution enum value

**Response (200 OK):**
```json
{
  "distribution": "OpenShift",
  "name": "Red Hat OpenShift",
  "category": "enterprise",
  "hasManagedControlPlane": false,
  "hasInfraNodes": true,
  "prodWorkerSpecs": { "cpu": 16, "ram": 64, "disk": 200 },
  "nonProdWorkerSpecs": { "cpu": 8, "ram": 32, "disk": 100 }
}
```

---

## Enum Values Reference

### Technology
```
DotNet, Java, NodeJs, Python, Go, Mendix, OutSystems
```

### Distribution
```
OpenShift, Kubernetes, Rancher, K3s, MicroK8s, Charmed, Tanzu, EKS, AKS, GKE, OKE
```

### EnvironmentType
```
Dev, Test, Stage, Prod, DR
```

### ClusterMode
```
MultiCluster, SingleCluster
```

### AppTier
```
Small, Medium, Large, XLarge
```

### HAPattern
```
None, ActiveActive, ActivePassive, NPlus1, NPlus2
```

### DRPattern
```
None, WarmStandby, HotStandby, MultiRegion
```

### LoadBalancerOption
```
None, Single, HAPair, CloudLB
```

### ServerRole
```
WebServer, AppServer, Database, Cache, Controller, LifeTime, FileStorage
```

---

## Error Responses

All error responses follow this format:

```json
{
  "type": "ErrorType",
  "title": "Human-readable message",
  "status": 400,
  "errors": [
    {
      "field": "FieldName",
      "message": "Validation message"
    }
  ]
}
```

### Error Types

| Type | Status | Description |
|------|--------|-------------|
| BadRequest | 400 | Invalid request format |
| ValidationError | 400 | Business rule validation failed |
| NotFound | 404 | Resource not found |
| InternalServerError | 500 | Unexpected server error |
