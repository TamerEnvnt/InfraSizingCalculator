# Low-Code Platform Infrastructure Sizing Research

**Research Date:** January 7, 2026
**Document Version:** 1.0
**Status:** Complete

---

## Executive Summary

This document provides comprehensive infrastructure sizing guidelines for low-code platforms (Mendix and OutSystems), including cloud deployment options, self-managed requirements, sizing formulas, and comparison with native application deployments.

---

## Table of Contents

1. [Mendix Infrastructure Sizing](#1-mendix-infrastructure-sizing)
2. [OutSystems Infrastructure Sizing](#2-outsystems-infrastructure-sizing)
3. [Sizing Formulas](#3-sizing-formulas)
4. [Deployment Patterns](#4-deployment-patterns)
5. [Native App Considerations](#5-native-app-considerations)
6. [Environment Multipliers](#6-environment-multipliers)
7. [Quick Reference Tables](#7-quick-reference-tables)

---

## 1. Mendix Infrastructure Sizing

### 1.1 Cloud Deployment Options

| Deployment Type | Management | Best For | SLA |
|-----------------|------------|----------|-----|
| **Mendix Cloud** (Multi-tenant) | Fully Managed | Standard workloads | 99.5% |
| **Mendix Cloud Dedicated** | Fully Managed | Single-tenant needs | 99.95% |
| **Mendix on Azure** | Managed/Customer | Azure ecosystem | Configurable |
| **Mendix on SAP BTP** | Partially Managed | SAP integration | Configurable |
| **Mendix on Kubernetes** | Customer Managed | Full control | Customer-defined |
| **On-Premises (Windows/Linux)** | Customer Managed | Air-gapped/legacy | Customer-defined |

### 1.2 Cloud Resource Packs - Standard

| Resource Pack | XS | S | M | L | XL |
|---------------|-----|-----|-----|-----|-----|
| **App Memory** | 1 GB | 2 GB | 4 GB | 8 GB | 16 GB |
| **App vCPU** | 0.25 | 0.5 | 1 | 2 | 4 |
| **DB Memory** | 1 GB | 2 GB | 4 GB | 8 GB | 16 GB |
| **DB vCPU** | 2 | 2 | 2 | 2 | 4 |
| **DB Storage** | 5 GB | 10 GB | 20 GB | 40 GB | 80 GB |
| **File Storage** | 10 GB | 20 GB | 40 GB | 80 GB | 160 GB |
| **Uptime SLA** | 99.5% | 99.5% | 99.5% | 99.5% | 99.5% |

### 1.3 Cloud Resource Packs - Premium

| Resource Pack | S | M | L | XL | XXL |
|---------------|-----|-----|-----|-----|-----|
| **App Memory** | 2 GB | 4 GB | 8 GB | 16 GB | 32 GB |
| **App vCPU** | 0.5 | 1 | 2 | 4 | 8 |
| **DB Memory** | 2 GB | 4 GB | 8 GB | 16 GB | 32 GB |
| **DB vCPU** | 2 | 2 | 2 | 4 | 4 |
| **DB Storage** | 10 GB | 20 GB | 40 GB | 80 GB | 160 GB |
| **File Storage** | 20 GB | 40 GB | 80 GB | 160 GB | 320 GB |
| **Uptime SLA** | 99.95% | 99.95% | 99.95% | 99.95% | 99.95% |

### 1.4 Kubernetes Deployment Requirements

#### Minimum Pod Resources (per Mendix App)

| Component | CPU Request | CPU Limit | Memory Request | Memory Limit |
|-----------|-------------|-----------|----------------|--------------|
| **Small App** | 250m | 1 | 1 GB | 2 GB |
| **Medium App** | 500m | 2 | 2 GB | 4 GB |
| **Large App** | 1 | 4 | 4 GB | 8 GB |
| **Build Pod** | 250m | 1 | 64 Mi | 256 Mi |

**Note:** Mendix apps require relatively more memory vs. CPU. Instance sizes with higher memory-to-CPU ratio are more cost-efficient.

#### Minimum Cluster Requirements

| Cluster Size | Nodes | Node Specs | Apps Supported |
|--------------|-------|------------|----------------|
| **Minimum** | 3 | 4 vCPU, 16 GB RAM | 5-10 |
| **Small** | 3-5 | 8 vCPU, 32 GB RAM | 10-25 |
| **Medium** | 5-10 | 8 vCPU, 32 GB RAM | 25-50 |
| **Large** | 10+ | 16 vCPU, 64 GB RAM | 50+ |

### 1.5 VM Deployment Requirements (On-Premises)

#### Windows Server (with IIS)

| Server Role | CPU Cores | RAM | Disk |
|-------------|-----------|-----|------|
| **Small (1-5 apps)** | 4 | 8 GB | 100 GB |
| **Medium (5-15 apps)** | 8 | 16 GB | 200 GB |
| **Large (15-30 apps)** | 16 | 32 GB | 500 GB |

#### Linux Server (with NGINX)

| Server Role | CPU Cores | RAM | Disk |
|-------------|-----------|-----|------|
| **Small (1-5 apps)** | 4 | 8 GB | 100 GB |
| **Medium (5-15 apps)** | 8 | 16 GB | 200 GB |
| **Large (15-30 apps)** | 16 | 32 GB | 500 GB |

### 1.6 Database Requirements (per Environment)

| App Size | Database Size | PostgreSQL RAM | SQL Server RAM |
|----------|---------------|----------------|----------------|
| **Small** | 5-20 GB | 2 GB | 4 GB |
| **Medium** | 20-100 GB | 4 GB | 8 GB |
| **Large** | 100-500 GB | 8 GB | 16 GB |
| **Enterprise** | 500+ GB | 16+ GB | 32+ GB |

**Supported Databases:**
- PostgreSQL (recommended)
- Microsoft SQL Server
- MySQL
- Oracle
- SAP HANA (SAP BTP only)

---

## 2. OutSystems Infrastructure Sizing

### 2.1 Deployment Options

| Deployment | Management | Best For |
|------------|------------|----------|
| **OutSystems Cloud** | Fully Managed | Standard enterprise |
| **OutSystems ODC** | Fully Managed (SaaS) | Cloud-native apps |
| **O11 Self-Managed** | Customer | Full control |
| **Hybrid** | Mixed | Transition scenarios |

### 2.2 Server Roles (O11 Self-Managed)

OutSystems O11 uses a **role-based architecture**:

| Role | Purpose | Scaling | Count per Env |
|------|---------|---------|---------------|
| **Deployment Controller** | Compilation, staging | Vertical only | 1 (max) |
| **Front-End Server** | App runtime | Horizontal | 1+ |
| **Database Server** | Data storage | Clustered | 1+ |

**Critical Notes:**
- Only ONE Deployment Controller per environment
- Front-end servers scale horizontally
- Database can use SQL Always On or Oracle RAC

### 2.3 Environment Types

Standard OutSystems infrastructure:

| Environment | Purpose | Required |
|-------------|---------|----------|
| **Development** | App development | Yes |
| **Quality/Test** | Testing & UAT | Yes |
| **Production** | Live applications | Yes |
| **LifeTime** | Infrastructure management | Yes (separate) |

**LifeTime Requirements:**
- MUST be a separate, dedicated environment
- Cannot be installed within application environments
- Does NOT support farm configuration
- Single server with DC + Server roles combined

### 2.4 Server Sizing by Workload

#### Front-End Servers

| Workload | Concurrent Users | CPU Cores | RAM | Requests/sec |
|----------|------------------|-----------|-----|--------------|
| **Small** | Up to 100 | 4 | 8 GB | ~50 |
| **Medium** | 100-500 | 8 | 16 GB | ~200 |
| **Large** | 500-2000 | 16 | 32 GB | ~500 |
| **Enterprise** | 2000+ | 16+ | 64 GB | ~1000+ |

**Note:** Values based on Mobile/Reactive Web Apps. Traditional Web Apps may vary.

#### Deployment Controller

| Team Size | CPU Cores | RAM | Disk |
|-----------|-----------|-----|------|
| **Small (1-16 devs)** | 4 | 8 GB | 200 GB |
| **Medium (16-50 devs)** | 8 | 16 GB | 400 GB |
| **Large (50+ devs)** | 16 | 32 GB | 500+ GB |

**Recommendation:** For 16+ developers or slow compilation, use a dedicated Deployment Controller server.

#### LifeTime Environment

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **CPU Cores** | 4 | 8 |
| **RAM** | 8 GB | 16 GB |
| **Disk** | 100 GB | 200 GB |
| **Storage Formula** | `AvgPageViews/Day * 2 * 1100 bytes` | - |

### 2.5 Database Sizing

| Environment Size | SQL Server RAM | Disk IOPS | Storage |
|------------------|----------------|-----------|---------|
| **Small** | 8 GB | 500 | 100 GB |
| **Medium** | 16 GB | 1000 | 250 GB |
| **Large** | 32 GB | 3000 | 500 GB |
| **Enterprise** | 64+ GB | 5000+ | 1+ TB |

**Supported Databases:**
- Microsoft SQL Server (Standard or Enterprise)
- Oracle Database
- Amazon RDS (1 vCPU minimum)

### 2.6 Redis Requirements (Session Storage)

#### Non-Production

| Component | Specification |
|-----------|---------------|
| **Servers** | 1 (single) |
| **CPU** | 2 vCPUs (>2.6 GHz) |
| **RAM** | 4 GB |
| **Network** | 100 Mbps |
| **Disk** | 10 GB |

#### Production (HA)

| Component | Specification |
|-----------|---------------|
| **Servers** | 3 (Redis Cluster) |
| **CPU** | 3-4 vCPUs each (>2.6 GHz) |
| **RAM** | 8 GB each |
| **Network** | 1 Gbps |
| **Disk** | 10 GB each |

### 2.7 OutSystems ODC Architecture

OutSystems Developer Cloud (ODC) is fully cloud-native:

| Component | Technology | Management |
|-----------|------------|------------|
| **Runtime** | Kubernetes + AWS | Auto-managed |
| **Database** | Aurora Serverless V2 | Auto-scaling |
| **CDN** | AWS CloudFront | Included |
| **WAF** | AWS WAF | Included |
| **Containers** | Linux containers | Auto-scaled |

**Key Benefits:**
- No infrastructure sizing required
- Auto-scaling for compute and database
- Multi-AZ high availability by default
- No Kubernetes expertise needed

### 2.8 Application Objects (Licensing)

OutSystems uses **Application Objects (AOs)** for licensing:

| Element | AO Count |
|---------|----------|
| Screen (Web, Mobile, Email) | 1 |
| Entity/Table | 1 |
| API Method (created) | 1 |
| API Method (consumed) | 1 per app |
| Block/Component | 0 |
| Logic flow | 0 |

**Notes:**
- AO limits apply to Production only
- Dev/Test environments: unlimited
- Typical starting allocation: 750-900 AOs (5-6 packs of 150)

---

## 3. Sizing Formulas

### 3.1 Mendix Sizing Formulas

#### Memory per Environment

```
Total_Memory_GB = (App_Count * Avg_App_Memory_GB) + DB_Memory_GB + Overhead_GB

Where:
- Small App: 1-2 GB
- Medium App: 2-4 GB
- Large App: 4-8 GB
- Overhead: 2 GB (system processes)
```

#### CPU per Environment

```
Total_vCPU = (App_Count * Avg_App_vCPU) + DB_vCPU + Overhead_vCPU

Where:
- Small App: 0.25-0.5 vCPU
- Medium App: 0.5-1 vCPU
- Large App: 1-2 vCPU
- Overhead: 0.5 vCPU
```

#### Storage per App

```
Total_Storage_GB = DB_Storage + File_Storage + Logs_Storage

Typical Ratios:
- DB Storage: 50% of total
- File Storage: 40% of total
- Logs/Temp: 10% of total
```

### 3.2 OutSystems Sizing Formulas

#### Front-End Server Capacity

```
FE_Servers_Required = CEILING(Concurrent_Users / Users_Per_Server)

Users_Per_Server (approx):
- 4 cores, 8 GB RAM: 100 users
- 8 cores, 16 GB RAM: 300 users
- 16 cores, 32 GB RAM: 800 users
```

#### Database Sizing

```
DB_Size_GB = (AO_Count * 0.1) + (Concurrent_Users * 0.05) + Base_Size_GB

Where:
- Base_Size_GB: 50 GB minimum
- Growth factor: 20% per year
```

#### Deployment Controller Memory

```
DC_RAM_GB = 8 + (Developer_Count * 0.25)

Minimum: 8 GB
Maximum recommended: 32 GB
```

### 3.3 User-to-Infrastructure Mapping

#### Small Deployment (<500 users)

| Users | Mendix Resources | OutSystems Resources |
|-------|------------------|---------------------|
| 50 | S pack per app | 1 FE (4c/8GB) |
| 100 | S-M pack per app | 1 FE (4c/8GB) |
| 250 | M pack per app | 1 FE (8c/16GB) |
| 500 | M-L pack per app | 2 FE (4c/8GB each) |

#### Medium Deployment (500-5000 users)

| Users | Mendix Resources | OutSystems Resources |
|-------|------------------|---------------------|
| 1000 | L pack per app | 2 FE (8c/16GB each) |
| 2500 | L-XL pack per app | 3 FE (8c/16GB each) |
| 5000 | XL pack per app | 4 FE (16c/32GB each) |

#### Large Deployment (5000+ users)

| Users | Mendix Resources | OutSystems Resources |
|-------|------------------|---------------------|
| 10000 | XXL pack + replicas | 6 FE (16c/32GB each) |
| 25000 | Multi-instance | 10+ FE (16c/32GB each) |
| 50000+ | Multi-region | Farm architecture |

### 3.4 Application Count to Resources

| App Count | Mendix K8s | OutSystems VMs |
|-----------|------------|----------------|
| 1-5 | 3 nodes (4c/16GB) | 2-3 servers |
| 5-20 | 5 nodes (8c/32GB) | 4-6 servers |
| 20-50 | 10 nodes (8c/32GB) | 8-12 servers |
| 50-100 | 15+ nodes (16c/64GB) | 15-20 servers |

---

## 4. Deployment Patterns

### 4.1 Small Deployment (1-5 Apps)

**Use Case:** Departmental apps, pilot projects, small teams

#### Mendix

| Component | Specification | Cost Profile |
|-----------|---------------|--------------|
| **Cloud Option** | S-M resource packs | ~$1,000-2,000/month |
| **K8s Option** | 3 nodes (4c/16GB) | Infrastructure + license |
| **Environments** | 2 (Acceptance, Production) | Per node |

#### OutSystems

| Component | Specification | Notes |
|-----------|---------------|-------|
| **Development** | 1 combined server | 4c/8GB |
| **Test** | 1 combined server | 4c/8GB |
| **Production** | 1-2 FE + 1 DB | 8c/16GB each |
| **LifeTime** | 1 dedicated | 4c/8GB |
| **Total Servers** | 5-7 | |

### 4.2 Medium Deployment (5-20 Apps)

**Use Case:** Multi-department, cross-functional applications

#### Mendix

| Component | Specification | Cost Profile |
|-----------|---------------|--------------|
| **Cloud Option** | M-L resource packs | ~$5,000-15,000/month |
| **K8s Option** | 5-7 nodes (8c/32GB) | Infrastructure + license |
| **Environments** | 3 (Test, Acceptance, Production) | Per node |

#### OutSystems

| Component | Specification | Notes |
|-----------|---------------|-------|
| **Development** | 1 DC + 2 FE + 1 DB | Separate roles |
| **Test** | 1 DC + 2 FE + 1 DB | |
| **Production** | 1 DC + 3-4 FE + 1 DB (HA) | Clustered DB |
| **LifeTime** | 1 dedicated + DB | |
| **Total Servers** | 15-20 | |

### 4.3 Large Deployment (20-50+ Apps)

**Use Case:** Enterprise-wide platform, center of excellence

#### Mendix

| Component | Specification | Cost Profile |
|-----------|---------------|--------------|
| **Cloud Option** | L-XXL resource packs | ~$25,000-75,000/month |
| **K8s Option** | 10-20 nodes (16c/64GB) | Infrastructure + license |
| **Environments** | 4+ (Dev, Test, Acceptance, Production) | Per node |
| **HA/DR** | Multi-AZ, regional failover | Premium required |

#### OutSystems

| Component | Specification | Notes |
|-----------|---------------|-------|
| **Development** | 1 DC + 4 FE + 2 DB (HA) | Large teams |
| **Test** | 1 DC + 4 FE + 2 DB (HA) | |
| **Staging** | 1 DC + 4 FE + 2 DB (HA) | Pre-production |
| **Production** | 1 DC + 8+ FE + 2 DB (HA) | Farm architecture |
| **DR** | 1 DC + 4 FE + 2 DB | Separate region |
| **LifeTime** | 1 + 1 DB (HA) | |
| **Total Servers** | 40-50+ | |

### 4.4 Enterprise Patterns

#### Multi-Region Active-Active

```
Region A (Primary)                    Region B (DR/Active)
+-------------------+                 +-------------------+
| Production        |                 | Production        |
| - 8 FE servers    |  <-- Sync -->  | - 8 FE servers    |
| - 2 DB (HA)       |                 | - 2 DB (HA)       |
+-------------------+                 +-------------------+
         |                                     |
         +------------- Global LB ------------+
```

#### Hub-and-Spoke (Mendix Private Cloud)

```
                    Central Management
                    +---------------+
                    | Mendix Portal |
                    +-------+-------+
                            |
        +-------------------+-------------------+
        |                   |                   |
    +---+---+           +---+---+           +---+---+
    | AWS   |           | Azure |           | GCP   |
    | EKS   |           | AKS   |           | GKE   |
    +-------+           +-------+           +-------+
```

---

## 5. Native App Considerations

### 5.1 Resource Overhead Comparison

Low-code platforms introduce runtime overhead compared to native applications:

| Factor | Native Apps | Low-Code (Mendix/OS) | Overhead |
|--------|-------------|----------------------|----------|
| **Memory per instance** | 256-512 MB | 1-4 GB | 2-8x |
| **CPU per instance** | 0.1-0.25 vCPU | 0.25-1 vCPU | 2-4x |
| **Startup time** | 1-5 seconds | 30-120 seconds | 10-30x |
| **Cold start impact** | Minimal | Significant | Higher |

### 5.2 When to Adjust for Low-Code

**Add 30-50% overhead** to native app estimates for low-code when:
- Converting sizing from traditional apps
- Migrating native apps to low-code
- Estimating capacity for mixed workloads

### 5.3 Efficiency Trade-offs

| Aspect | Native | Low-Code | Trade-off |
|--------|--------|----------|-----------|
| **Development Speed** | Slower | 2-10x faster | Time vs resources |
| **Resource Efficiency** | High | Medium | Cost vs speed |
| **Scalability Control** | Full | Limited | Control vs simplicity |
| **Performance Tuning** | Full | Limited | Optimization options |
| **Maintenance** | Higher effort | Lower effort | Ongoing cost |

### 5.4 Right-Sizing Guidelines

**Use Low-Code When:**
- Time-to-market is critical
- Business logic is primary concern
- Standard patterns suffice
- Team skills favor low-code

**Use Native When:**
- Performance is critical (<10ms response)
- High transaction volumes (>10,000 TPS)
- Complex algorithms required
- Extreme resource efficiency needed

### 5.5 Hybrid Architecture Sizing

For mixed native + low-code deployments:

```
Total_Resources = Native_Resources + (LowCode_Resources * 1.3)

Where:
- Native_Resources: Standard K8s/VM sizing
- LowCode_Resources: Platform-specific sizing
- 1.3: Overhead factor for abstraction layer
```

---

## 6. Environment Multipliers

### 6.1 Environment Sizing Ratios

| Environment | vs. Production | Rationale |
|-------------|----------------|-----------|
| **Development** | 25-50% | Developer workstations, minimal load |
| **Test/QA** | 25-50% | Functional testing, limited users |
| **Staging/UAT** | 50-75% | Performance testing, near-prod config |
| **Production** | 100% | Full capacity baseline |
| **DR** | 50-100% | Depends on RTO requirements |

### 6.2 Mendix Environment Configuration

| Environment Type | Resource Pack Recommendation |
|------------------|------------------------------|
| **Free App** | Limited (shared resources) |
| **Acceptance** | Same as Production |
| **Production** | Full specification |
| **Test** | 50% of Production |

**Note:** Mendix Cloud nodes include Acceptance + Production by default.

### 6.3 OutSystems Environment Configuration

| Environment | Front-Ends | Database | Scaling |
|-------------|------------|----------|---------|
| **Development** | 1-2 | Shared | None |
| **Test** | 1-2 | Shared | None |
| **Staging** | 2-4 | Dedicated | Similar to Prod |
| **Production** | 2+ (HA) | HA Cluster | Full |

### 6.4 Cost Optimization Strategies

#### Non-Production Environments

1. **Right-size resources** - Use smaller instances
2. **Auto-shutdown** - Stop dev/test outside hours
3. **Shared databases** - Combine where possible
4. **Spot instances** - Use for non-critical workloads

#### Production Environments

1. **Auto-scaling** - Scale based on demand
2. **Reserved capacity** - Commit for discounts
3. **Right-size continuously** - Monitor and adjust
4. **Multi-AZ only where needed** - HA adds cost

---

## 7. Quick Reference Tables

### 7.1 Mendix Quick Sizing

| Scenario | Apps | Users | K8s Nodes | Resource Pack |
|----------|------|-------|-----------|---------------|
| Pilot | 1-2 | <100 | 3 (4c/16GB) | S |
| Department | 5-10 | 100-500 | 5 (8c/32GB) | M |
| Enterprise | 20-50 | 500-5000 | 10 (8c/32GB) | L-XL |
| Large Enterprise | 50+ | 5000+ | 20+ (16c/64GB) | XXL |

### 7.2 OutSystems Quick Sizing

| Scenario | Apps | Users | Total Servers | FE per Env |
|----------|------|-------|---------------|------------|
| Pilot | 1-3 | <100 | 6-8 | 1 |
| Department | 5-15 | 100-500 | 12-18 | 2 |
| Enterprise | 20-50 | 500-5000 | 25-35 | 4 |
| Large Enterprise | 50+ | 5000+ | 45+ | 8+ |

### 7.3 Database Quick Sizing

| Data Volume | PostgreSQL | SQL Server | Storage |
|-------------|------------|------------|---------|
| <10 GB | 2c/4GB | 4c/8GB | 50 GB |
| 10-50 GB | 4c/8GB | 8c/16GB | 100 GB |
| 50-200 GB | 8c/16GB | 16c/32GB | 250 GB |
| 200+ GB | 16c/32GB | 32c/64GB | 500+ GB |

### 7.4 Growth Planning Factors

| Growth Type | Annual Factor | 3-Year Factor |
|-------------|---------------|---------------|
| **Users** | 1.2x | 1.7x |
| **Apps** | 1.3x | 2.2x |
| **Data** | 1.5x | 3.4x |
| **Traffic** | 1.3x | 2.2x |

**Formula:**
```
Future_Capacity = Current_Capacity * (Growth_Factor ^ Years)
```

---

## Sources

### Mendix Official Documentation
- [Mendix Cloud Deployments](https://www.mendix.com/evaluation-guide/deployment/mendix-cloud/)
- [Mendix on Kubernetes](https://docs.mendix.com/developerportal/deploy/private-cloud/)
- [System Requirements](https://docs.mendix.com/refguide/system-requirements/)
- [Scaling Your Environment](https://docs.mendix.com/developerportal/deploy/scale-environment/)
- [Licensing Mendix Cloud Apps](https://docs.mendix.com/developerportal/deploy/licensing-apps/)

### OutSystems Official Documentation
- [Sizing OutSystems Platform](https://success.outsystems.com/documentation/best_practices/infrastructure_management/designing_outsystems_infrastructures/sizing_outsystems_platform/)
- [OutSystems System Requirements](https://success.outsystems.com/documentation/11/setup_outsystems_infrastructure_and_platform/setting_up_outsystems/outsystems_system_requirements/)
- [Platform Server Roles](https://success.outsystems.com/documentation/11/setup_outsystems_infrastructure_and_platform/designing_outsystems_infrastructures/outsystems_platform_server_roles/)
- [Application Objects](https://success.outsystems.com/support/licensing/application_objects/)
- [ODC Architecture](https://success.outsystems.com/documentation/outsystems_developer_cloud/managing_outsystems_platform_and_apps/cloud_native_architecture_of_outsystems_developer_cloud/)

### General References
- [Northflank: Dev, QA, Staging Environments](https://northflank.com/blog/what-are-dev-qa-preview-test-staging-and-production-environments)
- [Plutora: Scale Down Test Environments](https://www.plutora.com/blog/scale-down-test-environments)
- [Low-Code vs Traditional Development Comparisons](https://kissflow.com/low-code/low-code-vs-traditional-app-development/)

---

**Document End**

*This research report is based on publicly available documentation and official vendor sources as of January 2026. For the most current information, always consult the official Mendix and OutSystems documentation.*
