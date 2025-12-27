# OutSystems On-Premises Architecture Specifications

## Overview

OutSystems is an enterprise low-code platform. This document describes the correct on-premises infrastructure architecture based on official OutSystems documentation.

## Reference Documentation

- [Infrastructure Architecture and Deployment Options](https://success.outsystems.com/documentation/11/setup_outsystems_infrastructure_and_platform/setting_up_outsystems/possible_setups_for_an_outsystems_infrastructure/infrastructure_architecture_and_deployment_options/)
- [Sizing OutSystems Platform](https://success.outsystems.com/documentation/best_practices/infrastructure_management/designing_outsystems_infrastructures/sizing_outsystems_platform/)
- [OutSystems Platform Server Roles](https://success.outsystems.com/Support/Enterprise_Customers/Maintenance_and_Operations/Designing_OutSystems_Infrastructures/01_OutSystems_Platform_server_roles)
- [Scaling and High Availability](https://success.outsystems.com/Support/Enterprise_Customers/Maintenance_and_Operations/Designing_OutSystems_Infrastructures/03_Scaling_and_high_availability_for_OutSystems_Platform_servers)

## Infrastructure Architecture

### Environment Types

A typical OutSystems infrastructure consists of **4 separate environments**:

| Environment | Purpose | Access |
|-------------|---------|--------|
| **Development** | Application development | Developers, Development Managers |
| **Quality/Test** | Testing and UAT | Testers, Business Users |
| **Production** | Live applications | Operations Team, End Users |
| **LifeTime** | Infrastructure Management Console | Platform Administrators |

### Server Roles (Per Application Environment)

Each application environment (Dev, Test, Prod) has these server roles:

| Role | Description | Scaling | Required |
|------|-------------|---------|----------|
| **Deployment Controller** | Orchestrates compilation, staging, deployment | Max 1 per environment | Yes |
| **Front-End Server** | Runs applications, serves end users | Horizontal (multiple) | Yes |
| **Database Server** | Stores platform and application data | Clustered (SQL Always On, Oracle RAC) | Yes |

### LifeTime Environment (CRITICAL - Separate Dedicated Environment)

**LifeTime is NOT a role within application environments. It is a completely separate, dedicated environment.**

From OutSystems 11 onwards:
- LifeTime MUST run in a dedicated environment
- Installing LifeTime in an existing environment is NOT supported
- LifeTime does NOT support farm configuration
- Must be a single server with both Deployment Controller + Server roles combined
- Requires its own database catalogs (SQL Server) or schemas (Oracle)

| Component | Specification |
|-----------|---------------|
| Server | Single server with DC + Server roles |
| Database | Dedicated database catalogs/schemas |
| Scaling | Does not support horizontal scaling (light management console) |
| HA | Can scale horizontally for high availability only |

## Server Sizing

### Application Environment Servers

#### Minimum Requirements (per server)

| Workload Type | CPU Cores | RAM (GB) |
|---------------|-----------|----------|
| Mobile/Reactive Apps (Small) | 4 | 8 |
| Mobile/Reactive Apps (Medium) | 8 | 16 |
| Mobile/Reactive Apps (Large) | 16 | 32 |
| Traditional Web (Small) | 4 | 8 |
| Traditional Web (Medium) | 8 | 16 |
| Traditional Web (Large) | 16 | 32 |

#### Storage Requirements

| Server Role | Minimum Disk (GB) | Notes |
|-------------|-------------------|-------|
| Deployment Controller | 200 | Large teams need more for compilation |
| Front-End Server | 100 | Application binaries |
| Database Server | 500+ | Depends on data volume |

### LifeTime Environment

| Component | Specification |
|-----------|---------------|
| CPU Cores | 4 (minimum) |
| RAM | 8 GB (minimum) |
| Disk | 100 GB + storage for monitoring data |
| Storage Formula | MaxStorage = AvgPageViewsPerDay * 2 days * 1100 bytes |

## High Availability Configurations

### Front-End Servers (Active-Active Only)

OutSystems front-end servers operate in an **Active-Active** configuration:
- ALL front-end servers actively handle traffic simultaneously
- Load balancer distributes requests across all FE servers
- No user data or session stored locally (sessions in database)
- Any server can handle any request
- Scale horizontally - add more FE servers as needed
- Minimum 2 servers recommended for redundancy

**Note**: Active-Passive is NOT applicable to OutSystems FE servers - they always operate Active-Active.

### Database Servers

- SQL Server: Always On Availability Groups (recommended)
- Oracle: Real Application Clusters (RAC)
- Database handles session storage for farm deployments

### Deployment Controller (Single Point - Manual Failover)

**Critical**: The Deployment Controller has specific limitations:
- Only **ONE** DC per environment - cannot be clustered
- If DC fails:
  - Applications continue running normally
  - Publishing becomes unavailable
  - Cache invalidation stops working
- Failover is **MANUAL** - requires moving DC role to another FE server
- For resilience: Use infrastructure-level VM failover (not OutSystems level)

### LifeTime (Management Environment)

- Does NOT support farm configuration
- Single server with DC + Server roles combined
- For HA: Infrastructure-level failover only (VM HA, cluster)

## Disaster Recovery Configurations

### DR is a Separate Environment

In OutSystems, DR is implemented as a **separate environment** in a different location:
- DR environment has its own DC, FE servers, and database
- Data replication via database-level replication
- Application deployment via LifeTime staging

### DR Patterns

| Pattern | Description | RTO | RPO |
|---------|-------------|-----|-----|
| **None** | No DR environment | N/A | N/A |
| **Warm Standby** | DR environment with minimal resources, scaled up on failover | Hours | Minutes-Hours |
| **Hot Standby** | DR environment at full capacity, ready for immediate failover | Minutes | Seconds-Minutes |
| **Multi-Region Active** | Both regions actively serving traffic | Near-zero | Near-zero |

### DR Components

For each production environment, the DR environment needs:
- Deployment Controller (1)
- Front-End Servers (scaled appropriately)
- Database (replicated from production)

### Database Replication for DR

- SQL Server: Database Mirroring, Log Shipping, or Always On (async)
- Oracle: Data Guard
- Replication should be asynchronous for geographic distance

## Typical Infrastructure Sizes

### Small (Startups, Small Teams)

| Environment | DC | FE | DB | Total Servers |
|-------------|----|----|----|--------------:|
| Development | 1 | 1 (combined) | 1 | 1-2 |
| Test | 1 | 1 (combined) | 1 | 1-2 |
| Production | 1 | 2 | 1 | 3-4 |
| LifeTime | 1 (combined) | - | 1 | 1 |
| **Total** | - | - | - | **6-9** |

### Medium (Enterprise Teams)

| Environment | DC | FE | DB | Total Servers |
|-------------|----|----|----|--------------:|
| Development | 1 | 2 | 1 | 4 |
| Test | 1 | 2 | 1 | 4 |
| Staging | 1 | 2 | 1 | 4 |
| Production | 1 | 4 | 2 (HA) | 7 |
| DR | 1 | 2 | 1 | 4 |
| LifeTime | 1 (combined) | - | 1 | 2 |
| **Total** | - | - | - | **25** |

### Large (Enterprise with HA)

| Environment | DC | FE | DB | Total Servers |
|-------------|----|----|----|--------------:|
| Development | 1 | 4 | 2 (HA) | 7 |
| Test | 1 | 4 | 2 (HA) | 7 |
| Staging | 1 | 4 | 2 (HA) | 7 |
| Production | 1 | 8 | 2 (HA) | 11 |
| DR | 1 | 4 | 2 (HA) | 7 |
| LifeTime | 1 (combined) | 1 (HA) | 2 (HA) | 4 |
| **Total** | - | - | - | **43** |

## Implementation Notes for Calculator

1. **LifeTime should be treated as a separate environment type**, not a server role
2. When calculating infrastructure:
   - Add application environments (Dev, Test, Staging, Prod, DR)
   - Add ONE LifeTime environment separately
3. LifeTime environment always has:
   - 1 combined DC+Server (never separate)
   - 1 Database (can be HA)
4. LifeTime is SHARED across all application environments - only ONE per infrastructure

## Version History

| Date | Version | Changes |
|------|---------|---------|
| 2024-12-26 | 1.0 | Initial documentation based on OutSystems 11 |
