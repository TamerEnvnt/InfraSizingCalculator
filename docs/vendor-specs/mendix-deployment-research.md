# Mendix Deployment Options - Comprehensive Research Report

**Research Date:** December 24, 2025
**Document Version:** 1.0
**Status:** Complete

---

## Executive Summary

Mendix offers a comprehensive range of deployment options spanning from fully-managed cloud platforms to on-premises installations, supporting diverse organizational requirements from startups to large enterprises with strict compliance needs. This report provides a complete analysis of all deployment paths, licensing models, supported platforms, and recent platform updates.

---

## 1. Complete Deployment Options Matrix

### 1.1 Overview of All Deployment Paths

Mendix applications follow cloud-native design principles and conform to "Twelve-Factor App" architecture, enabling deployment across diverse infrastructure environments.

| Deployment Type | Management Level | Best For | Infrastructure Owner |
|----------------|------------------|----------|---------------------|
| **Mendix Cloud** (Multi-tenant) | Fully Managed by Mendix | Organizations lacking infrastructure expertise | Mendix |
| **Mendix Cloud Dedicated** | Fully Managed by Mendix | Single-tenant requirements | Mendix (dedicated AWS VPC) |
| **Mendix on Azure** | Managed by Mendix | Azure-committed organizations | Customer (Azure subscription) |
| **Mendix on SAP BTP** | Partially Managed | SAP ecosystem integration | Customer/SAP |
| **Mendix on STACKIT** | Partially Managed | GDPR compliance, German data sovereignty | Customer (STACKIT) |
| **Mendix on Kubernetes** | Customer/Partner Managed | Private cloud, any K8s provider | Customer |
| **On-Premises (Windows)** | Customer Managed | Traditional infrastructure | Customer |
| **On-Premises (Linux)** | Customer Managed | Traditional infrastructure | Customer |
| **Docker/Buildpack** | Customer Managed | Custom container deployments | Customer |

---

## 2. Cloud Providers - Official Support Status

### 2.1 Mendix Cloud (Native Platform)

**Architecture:**
- Built on Amazon Web Services (AWS)
- Multi-tenant Kubernetes-based platform
- Fully hosted and managed by Mendix

**Regions:**
- **North America:** US regions
- **Europe:** Multiple EU regions
- **Asia Pacific:** Multiple APAC regions
  - Seoul, South Korea (added 2025)
  - Osaka, Japan (added 2025)
  - Jakarta, Indonesia (added 2025)
  - São Paulo, Brazil (added 2025)

**High Availability:**
- Multi-availability zone architecture per region
- 99.5% uptime SLA (Standard)
- 99.95% uptime SLA (Premium)
- RPO: up to 15 minutes
- RTO: up to 15 minutes
- Automatic failover across availability zones
- Regional fallback capability (with Premium Plus)

**Technical Stack:**
- Container Platform: Kubernetes on AWS
- Database: RDS PostgreSQL (Multi-AZ)
- Storage: S3 (Multi-AZ replication)
- Web Server: NGINX
- Monitoring: Built-in Grafana dashboards

**Support Status:** ✅ Fully Supported (Primary deployment option)

---

### 2.2 Mendix Cloud Dedicated

**Architecture:**
- Single-tenant instance of Mendix Cloud
- Dedicated AWS VPC in customer's AWS account
- Runs within two availability zones
- Operated by Mendix

**Key Features:**
- Same features as Mendix Cloud
- Connected to private/corporate network
- Enhanced security isolation
- Compliance for strict regulatory requirements (SOC2, etc.)

**Requirements:**
- Must purchase Mendix Cloud Resource Packs separately
- Premium/Enterprise licensing tier

**Support Status:** ✅ Fully Supported

---

### 2.3 Mendix on Azure

**Architecture:**
- Managed service deploying to customer's private Azure subscription
- Automated setup of AKS, ACR, databases, storage, monitoring
- Deployment time: Under 30 minutes
- Built-in security controls following Azure best practices

**Technical Components:**
- Container Platform: Azure Kubernetes Service (AKS)
- Container Registry: Azure Container Registry (ACR)
- Monitoring: Grafana dashboard
- Database: Configurable Azure database services
- Storage: Azure Storage

**Key Features:**
- DDoS Protection on Virtual Network
- Private VNet deployment (default)
- Apps can be configured without public endpoints
- GDPR-compliant
- Built-in Mendix Pipelines support (2025 update)

**Free Trial:**
- 120-day free trial per app environment via Azure Marketplace (as of November 2025)

**Manual vs Automated:**
- Manual AKS setup: ~140 hours initially + quarterly maintenance
- Mendix on Azure: Automated, under 30 minutes

**Limitations:**
- Third-party monitoring tools (Datadog, Dynatrace) not supported out of the box

**Support Status:** ✅ Fully Supported

---

### 2.4 Mendix on SAP BTP

**Architecture:**
- Deployed on SAP Business Technology Platform
- Uses Cloud Foundry environment
- Mendix Cloud Foundry buildpack

**Technical Stack:**
- Runtime: Cloud Foundry
- Database: SAP HANA or PostgreSQL (AWS/Azure-hosted)
- Storage: Optional storage service
- Authentication: Integrated with SAP authentication/authorization service
- Connectivity: SAP connectivity service integration

**Key Features:**
- Highly available with auto-scaling support
- Vertical, horizontal, and auto-scaling capabilities
- Multi-cloud capability via Cloud Foundry
- Direct deployment from SAP BTP cockpit or Cloud Foundry CLI

**Partnership:**
- Mendix is SAP's main low-code partner since 2017
- Only low-code vendor with SAP Endorsed Apps partnership
- SAP Premium Certification

**Recent Updates (2025):**
- Upgraded to Cloud Foundry API v3 (v2 deprecated)
- Custom environment variables support
- Dynatrace integration available
- Improved SAP HANA database environment creation

**Support Status:** ✅ Fully Supported

---

### 2.5 Mendix on STACKIT

**Architecture:**
- Runs on STACKIT cloud platform (Schwarz Group)
- High-performance data centers in Germany and Austria
- GDPR-compliant, sovereign cloud solution

**Key Features:**
- Data sovereignty with infrastructure exclusively in Germany/Austria
- Highest European security standards
- 3 data centers (2 in Germany, 1 in Austria as of March 2025)

**Target Market:**
- Organizations requiring German data residency
- DACH region (Germany, Austria, Switzerland)
- Strict GDPR compliance requirements

**Partnership:**
- Schwarz Group (Lidl, Kaufland parent) has used Mendix since 2021
- Joint solution announced September 2022
- Starter app with pre-configured standard modules available

**Support Status:** ✅ Fully Supported (Partnership offering)

---

### 2.6 Mendix on Kubernetes (Private Cloud)

**Supported Kubernetes Distributions:**
- Amazon EKS (Elastic Kubernetes Service)
- Microsoft AKS (Azure Kubernetes Service)
- Google GKE (Google Kubernetes Engine)
- Red Hat OpenShift (version 3.11+, 4.4+ recommended)
- Any Kubernetes cluster version 1.19+ (with Operator v2.x)
- Any Kubernetes cluster version 1.12-1.21 (with Operator v1.12.x)

**Architecture:**
- Uses Mendix Operator for deployment
- Kubernetes-based cloud environment
- Complete customer control over operations

**Mendix Operator Compatibility:**
- Operator v2.x: Supports Kubernetes 1.19 and later
- Operator v1.12.x: Supports Kubernetes 1.12 through 1.21
- Operator v2.1.0+: Required for Prometheus metrics
- Monitoring: Compatible with Kubernetes 1.22

**Deployment Options:**
- Connected mode: Integrated with Mendix Developer Portal
- Standalone mode: Fully air-gapped environments
- Command-line deployment via mxpc-cli

**CI/CD Support:**
- Mendix Pipelines support added in 2025
- Automated deployment workflows
- GitOps-compatible

**Runtime Version Compatibility:**
- Native metrics mode: Mendix 9.7 and above
- Compatibility mode: Mendix 9.6 and below

**Support Status:** ✅ Fully Supported

**Official Documentation:**
- [Supported Providers](https://docs.mendix.com/developerportal/deploy/private-cloud-supported-environments/)
- [Mendix on Kubernetes](https://docs.mendix.com/developerportal/deploy/private-cloud/)
- [Upgrading Guide](https://docs.mendix.com/developerportal/deploy/private-cloud-upgrade-guide/)

---

## 3. Kubernetes/Container Options

### 3.1 Container Technology Support

**Supported Container Platforms:**
- ✅ Kubernetes (see distributions above)
- ✅ Docker (via docker-mendix-buildpack)
- ✅ Cloud Foundry (any CF-based platform)
- ✅ Red Hat OpenShift

**Container Architecture:**
- Cloud-native stateless runtime
- Twelve-Factor App principles
- Auto-scaling, auto-provisioning, auto-healing
- Low infrastructure overhead
- CI/CD integration

---

### 3.2 Docker Deployment

**Buildpack Status:**
- Repository: mendix/docker-mendix-buildpack
- Current Version: v6.x
- Minimum Docker Version: 17.05+ (for multistage builds)

**Recent Changes (2024-2025):**
- Migration from Ubuntu Bionic to Red Hat UBI8
  - Ubuntu Bionic EOL: May 31, 2025
- UBI9 required for Mendix 9.20+
  - Addresses .NET dependency issues
- Procedure changes in v6 vs v5
  - May require CI/CD pipeline updates

**Base Images:**
- Previous: Ubuntu Bionic
- Current (Mendix <9.20): Red Hat UBI8
- Current (Mendix 9.20+): Red Hat UBI9

**Support Status:** ✅ Fully Supported (with version considerations)

**Official Documentation:**
- [GitHub Repository](https://github.com/mendix/docker-mendix-buildpack)
- [Docker Deployment Guide](https://docs.mendix.com/developerportal/deploy/docker-deploy/)

---

### 3.3 Cloud Foundry Deployment

**Buildpack:**
- Repository: mendix/cf-mendix-buildpack
- Recommended: Fixed version v4.30.14 or later
- Versions below v4.30.14: Not supported

**Recent Changes:**
- Cloud Foundry API v3 adoption (v2 deprecated)

**Supported CF Platforms:**
- SAP BTP (Cloud Foundry environment)
- AWS Cloud Foundry
- IBM Cloud (formerly Bluemix)
- Microsoft Azure Cloud Foundry
- VMware Tanzu Application Service
- Any CF-based PaaS

**Multi-Cloud Capability:**
- Mendix is the only low-code platform leveraging Cloud Foundry as core runtime technology
- Enables portable deployments across CF-based clouds

**Support Status:** ✅ Fully Supported

**Official Documentation:**
- [GitHub Repository](https://github.com/mendix/cf-mendix-buildpack)
- [Cloud Foundry Deployment](https://docs.mendix.com/developerportal/deploy/cloud-foundry-deploy/)

---

## 4. VM/Traditional Deployment Options

### 4.1 On-Premises Deployment Overview

**Definition:**
Any deployment on customer infrastructure that:
- Does NOT use Kubernetes/OpenShift
- Does NOT use Mendix Operator
- Runs on traditional VMs or physical servers

**Technology Foundation:**
- JVM-based applications
- Mendix Runtime = Application server (similar to Tomcat)
- Standalone Java process execution

---

### 4.2 Microsoft Windows Deployment

**Supported Configurations:**
- Microsoft IIS server (easiest, fewest configuration issues)
- Standalone Java process
- Windows Server (versions per system requirements)

**Technical Stack:**
- Web Server: IIS
- Database: Microsoft SQL Server or PostgreSQL
- Runtime: JVM-based
- Configuration: Via Windows services

**Database Support:**
- SQL Server (with minor behavioral differences vs PostgreSQL)
- PostgreSQL
- Automatic schema management by Mendix Runtime

**Key Characteristics:**
- Easiest on-premises setup
- Fewer connection/configuration problems vs Linux
- No database administrator needed (Mendix manages schema)

**Support Status:** ✅ Fully Supported

**Official Documentation:**
- [Microsoft Windows Deployment](https://docs.mendix.com/developerportal/deploy/deploy-mendix-on-microsoft-windows/)
- [SQL Server Setup](https://docs.mendix.com/developerportal/deploy/setting-up-a-new-sql-server-database/)

---

### 4.3 Linux/Unix Deployment

**Deployment Methods:**
- Java process using m2ee-tools (Python utility)
- Docker containers (recommended approach)
- Application server deployment

**Technical Stack:**
- Web Server: NGINX (mirrors Mendix Cloud configuration)
- Database: PostgreSQL (primary), SQL Server, MySQL, Oracle, HSQLDB
- Runtime: JVM-based
- Orchestration: systemd, init scripts, or containerization

**Database Support:**
- PostgreSQL (recommended, matches Mendix Cloud)
- Microsoft SQL Server
- MySQL
- Oracle Database
- HSQLDB (development only)
- Any database via JDBC protocol (for integrations)

**Configuration:**
- m2ee-tools for process management
- Custom runtime settings for advanced configurations
- Environment variable configuration

**Migration Features:**
- Built-in database migration via custom runtime settings
- Cross-database migration support (e.g., PostgreSQL → SQL Server)
- SourceDatabaseType options: HSQLDB, MYSQL, ORACLE, POSTGRESQL, SQLSERVER

**Support Status:** ✅ Fully Supported

**Official Documentation:**
- [On-Premises Design](https://docs.mendix.com/developerportal/deploy/on-premises-design/)

---

### 4.4 Database Requirements Summary

| Database | Support Level | Notes |
|----------|--------------|-------|
| **PostgreSQL** | ✅ Primary | Default for Mendix Cloud, recommended |
| **Microsoft SQL Server** | ✅ Full Support | Minor behavioral differences documented |
| **MySQL** | ✅ Supported | Migration support available |
| **Oracle Database** | ✅ Supported | Migration support available |
| **SAP HANA** | ✅ Supported | For SAP BTP deployments |
| **HSQLDB** | ⚠️ Development Only | Not for production |
| **Any JDBC** | ✅ Integration Only | For external data integration |

**Database Management:**
- Mendix Runtime creates and manages database schema
- No dedicated DBA required
- Automatic migration handling
- Multi-database migration support

---

## 5. Licensing Model

### 5.1 Pricing Tiers (2025)

| Tier | Monthly Cost | Apps | Users | Environments | Support | Uptime SLA |
|------|--------------|------|-------|--------------|---------|------------|
| **Free** | $0 | Limited | Development | 1 | Community | N/A |
| **Basic** | ~$75 | 1 | Up to 5 | 1 Production | Basic | 99.5% |
| **Standard (One App)** | $998 base + per-user | 1 | Variable | Up to 4 | Business Hours (9×5) | 99.5% |
| **Standard (Unlimited)** | $2,495+ | Unlimited | Variable | Up to 4 per app | Business Hours (9×5) | 99.5% |
| **Premium** | Custom Quote | Unlimited | Variable | Flexible | 24×7 | 99.95% |

**Alternative Regional Pricing:**
- Standard: €900/month (one app) or €2,100/month (unlimited apps)
- Premium: Custom enterprise quotes

---

### 5.2 Licensing Model Details

**User-Based Pricing:**
- Based on number of app users, NOT:
  - Number of developers
  - Complexity of apps
  - Number of apps (except One App tier)
- Anonymous users: Not counted
- Named users with login credentials: Counted

**Deployment-Specific Licensing:**

#### Mendix Cloud
- Platform license + Cloud Resource Packs
- Resource packs scale with compute needs
- High Availability: Premium Resource Packs
- Multi-region failover: Premium Plus Resource Packs

#### Mendix Cloud Dedicated
- Platform license required
- Cloud Resource Packs required (sold separately)
- Premium/Enterprise tier minimum
- Custom pricing model

#### Mendix on Azure
- Platform license
- Customer pays Azure infrastructure costs directly
- No Mendix resource pack fees (infrastructure in customer subscription)

#### Mendix on SAP BTP
- Platform license
- Customer pays SAP BTP infrastructure costs
- Available via SAP procurement

#### Mendix on Kubernetes (Private Cloud)
- Platform license
- Tiered pricing for number of environments
- Reduced fees at higher environment counts
- Customer pays all infrastructure costs

#### On-Premises Deployment
- Platform license
- No cloud resource pack fees
- Customer responsible for all infrastructure
- Typically Premium tier for support

---

### 5.3 Cloud Resource Packs & Tokens

**Traditional Resource Packs:**
- Fixed-size resource allocations
- Standard vs Premium packs
- Scaling within pack boundaries
- Locked to specific environments

**New Cloud Tokens Model (2025):**
- Flexible token-based allocation
- Purchase token bundles
- Allocate dynamically across:
  - Environment provisioning
  - Container resizing
  - Additional services
- Reallocate between resources as needs change
- No rigid plan lock-in

**GenAI Resource Packs (New in 2025):**
- Introduced in Mendix 10.18
- Specifically for GenAI workloads
- Reduced barriers for AI experimentation
- Separate pricing from standard compute

---

### 5.4 Additional Cost Considerations

**Included in Platform License:**
- Development tools (Studio Pro)
- Developer Portal access
- Application lifecycle management
- Governance and portfolio management
- Version control
- Unlimited developers

**Additional Costs (May Apply):**
- Advanced analytics modules
- Additional environments beyond plan limits
- Premium support tiers
- High availability features
- Regional failback
- Private cloud deployments
- Custom integrations
- Professional services
- Training and certification

**Total Cost of Ownership Factors:**
- Mendix Cloud: Lowest TCO (fully managed)
- Third-party clouds: Medium TCO (infrastructure costs + platform license)
- Private cloud/on-premises: Highest TCO (infrastructure + operations + platform license)

---

### 5.5 Procurement Options

**Direct Purchase:**
- Mendix.com website
- Direct sales engagement

**Marketplace Options:**
- AWS Marketplace
  - Counts toward AWS Enterprise Discount Program (EDP)
- Azure Marketplace
  - 120-day free trial for Mendix on Azure (as of Nov 2025)
- SAP Store

---

## 6. Recent Changes & Updates (2024-2025)

### 6.1 Platform Updates

#### Q4 2024 - Q1 2025

**Mendix Cloud:**
- New regions added: Osaka (Japan), Seoul (South Korea), Jakarta (Indonesia)
- São Paulo region added for South America
- Regional failback capabilities enhanced

**Mendix on Azure:**
- Free 120-day trial via Azure Marketplace (November 2025)
- Enhanced Mendix Pipelines support for automated deployment
- Deployment time reduced to under 30 minutes

**Mendix on Kubernetes:**
- Mendix Pipelines CI/CD support added (2025)
- Enhanced operator capabilities
- Improved monitoring with Prometheus integration
- Kubernetes 1.22+ monitoring compatibility

**SAP BTP:**
- Upgraded to Cloud Foundry API v3
- Custom environment variables support
- Dynatrace integration capabilities
- Improved SAP HANA database environment creation flow

---

### 6.2 Runtime & Version Support Updates

**Mendix 10.24 LTS (June 2025):**
- Released as Long-Term Support version
- Coincides with Mendix 11 GA release
- End of support for 10.6, 10.12, 10.18 MTS: September 2025

**Native Mobile Support Policy (2025 Update):**
- Support limited to:
  - Latest Mendix minor version
  - Any MTS/LTS version released within last 12 months
- Mendix 9 LTS extended support until August 2025
- All Mendix 10 MTS versions supported until August 2025

**Currently Supported Major Versions:**
- Mendix 9 (LTS until Mendix 12 GA)
- Mendix 10 (LTS 10.24 released June 2025)
- Mendix 11 (Current GA)

**Support Duration:**
- LTS: Supported until third consecutive major version GA
- MTS: Supported for 6 months + 3 months after next MTS
- Monthly releases: Supported until next release

**End of Support Implications:**
- Cannot deploy to Mendix Cloud on unsupported versions
- Can still deploy to customer-owned infrastructure
- No fixes, updates, security patches, or troubleshooting
- No investigation of issues

---

### 6.3 Container & Buildpack Changes

**Docker Buildpack:**
- Migration from Ubuntu Bionic to Red Hat UBI8
  - Ubuntu Bionic EOL: May 31, 2025
- UBI9 requirement for Mendix 9.20+
- Buildpack v6 released with changes from v5
  - May require CI/CD pipeline updates
- Minimum Docker version: 17.05 (multistage build support)

**Cloud Foundry Buildpack:**
- Minimum supported version: v4.30.14
- Cloud Foundry API v3 migration (v2 deprecated)
- Recommended: Use fixed versions for stable pipelines

---

### 6.4 New Deployment Features

**Cloud Tokens (2025):**
- Flexible resource allocation model
- Alternative to fixed resource packs
- Dynamic reallocation capabilities
- Simpler cloud resource management

**GenAI Resource Packs (Mendix 10.18):**
- Dedicated resources for AI workloads
- Reduced barriers for GenAI experimentation
- Separate from standard compute resources

**Mendix Pipelines Expansion:**
- Extended to Mendix on Kubernetes
- Extended to Mendix on Azure
- Eliminates manual deployment steps
- Integrated CI/CD for private cloud

**Java 21 Support:**
- Modern Java runtime support
- Enhanced performance
- Security improvements

---

### 6.5 Partnership & Ecosystem Updates

**STACKIT Partnership (2022-2025):**
- Joint solution for sovereign cloud
- German/Austrian data residency
- Pre-configured starter apps
- Growing DACH market presence

**AWS Integration:**
- EKS reference deployment
- One-click EKS environment provisioning
- ~30 minute automated setup
- Marketplace availability

**Azure Integration:**
- Fully managed Azure service
- AKS automation
- 120-day free trial
- Private VNet deployment default

**SAP Partnership:**
- Continued SAP Endorsed Apps status
- SAP Premium Certification maintained
- Enhanced BTP integration
- 7+ years of strategic partnership

---

## 7. Deprecated & Legacy Options

### 7.1 Deprecated Technologies

**Cloud Foundry API v2:**
- Status: Deprecated
- Migration Required: Upgrade to API v3
- Impact: SAP BTP and CF-based deployments
- Timeline: Active deprecation as of 2024-2025

**Ubuntu Bionic Base Image:**
- Status: End of Life - May 31, 2025
- Replacement: Red Hat UBI8/UBI9
- Impact: Docker buildpack users
- Migration: Required for continued support

**Mendix Operator v1.12.x:**
- Status: Legacy support
- Limitation: Kubernetes 1.12-1.21 only
- Recommendation: Upgrade to Operator v2.x
- Impact: Cannot use Kubernetes 1.22+

---

### 7.2 Unsupported/Legacy Versions

**Cloud Foundry Buildpack < v4.30.14:**
- Status: Not supported
- Recommendation: Upgrade to v4.30.14+

**Docker < 17.05:**
- Status: Incompatible
- Reason: No multistage build support
- Workaround: Use Mendix Docker Buildpack v2.3.2 (legacy)

**Mendix 8 and Earlier:**
- Status: End of support reached
- Cannot deploy to Mendix Cloud
- Can deploy to customer infrastructure (unsupported)

---

## 8. Prerequisites & Requirements Summary

### 8.1 Mendix Cloud

**Prerequisites:**
- Mendix account
- Platform license (Basic/Standard/Premium)
- Cloud Resource Packs (for Standard/Premium)

**Requirements:**
- None (fully managed)

**Developer Requirements:**
- Mendix Studio Pro
- Internet connectivity
- Modern web browser

---

### 8.2 Mendix Cloud Dedicated

**Prerequisites:**
- Enterprise/Premium platform license
- Cloud Resource Packs
- Network connectivity requirements

**Requirements:**
- Private network connection setup
- AWS account coordination (for VPC peering)

---

### 8.3 Mendix on Azure

**Prerequisites:**
- Mendix platform license
- Azure subscription with appropriate permissions
- Marketplace access

**Requirements:**
- Azure subscription
- Resource group creation permissions
- VNet configuration (for private deployment)

**Automated Provisioning Includes:**
- AKS cluster
- Azure Container Registry
- Database services
- Storage accounts
- Monitoring (Grafana)
- DNS and certificates

---

### 8.4 Mendix on SAP BTP

**Prerequisites:**
- Mendix platform license
- SAP BTP account
- Cloud Foundry environment

**Requirements:**
- Cloud Foundry CLI (optional for direct deployment)
- SAP authentication/authorization service
- Database service (HANA or PostgreSQL)
- Storage service (optional)

---

### 8.5 Mendix on Kubernetes

**Prerequisites:**
- Mendix platform license
- Kubernetes cluster (version 1.19+ for Operator v2.x)
- kubectl access
- Cluster admin permissions

**Requirements:**

**Minimum Kubernetes Version:**
- Operator v2.x: Kubernetes 1.19+
- Operator v1.12.x: Kubernetes 1.12-1.21

**Cluster Resources:**
- Sufficient compute capacity for app instances
- Storage class for persistent volumes
- Load balancer support (for external access)
- DNS configuration
- TLS certificate management

**Additional Components:**
- Mendix Operator installation
- Database service (PostgreSQL recommended)
- Storage solution (S3-compatible or equivalent)
- Monitoring stack (Prometheus/Grafana - optional)

**Supported Distributions:**
- Amazon EKS
- Microsoft AKS
- Google GKE
- Red Hat OpenShift 3.11+ (4.4+ recommended)
- Any conformant Kubernetes cluster

---

### 8.6 On-Premises (Windows)

**Prerequisites:**
- Mendix platform license
- Windows Server
- IIS installation

**Requirements:**
- Windows Server (per system requirements)
- Internet Information Services (IIS)
- Java Runtime Environment (JRE)
- Database server:
  - Microsoft SQL Server, OR
  - PostgreSQL
- Storage for file documents
- Network configuration for app access

**Optional:**
- Load balancer (for HA)
- Backup solution
- Monitoring tools

---

### 8.7 On-Premises (Linux)

**Prerequisites:**
- Mendix platform license
- Linux server (RHEL, Ubuntu, CentOS, etc.)

**Requirements:**
- Linux distribution (per system requirements)
- Java Runtime Environment (JRE)
- Python (for m2ee-tools)
- NGINX or Apache web server
- Database server:
  - PostgreSQL (recommended), OR
  - MySQL, OR
  - Oracle, OR
  - Microsoft SQL Server
- Storage for file documents
- Network configuration for app access

**Optional:**
- systemd service configuration
- Load balancer (for HA)
- Backup solution
- Monitoring tools

---

### 8.8 Docker Deployment

**Prerequisites:**
- Mendix platform license
- Docker 17.05 or higher

**Requirements:**
- Docker runtime
- Appropriate base image:
  - UBI8 for Mendix < 9.20
  - UBI9 for Mendix 9.20+
- Database connectivity
- Storage for file documents
- Network configuration

**Build Requirements:**
- Mendix deployment package (.mda file)
- Mendix docker-mendix-buildpack
- Docker build capabilities

---

## 9. Architecture Comparison

### 9.1 Responsibility Matrix

| Component | Mendix Cloud | Cloud Dedicated | Azure/SAP/STACKIT | Kubernetes | On-Premises |
|-----------|--------------|----------------|-------------------|------------|-------------|
| **Application Code** | Customer | Customer | Customer | Customer | Customer |
| **Mendix Runtime** | Mendix | Mendix | Mendix | Mendix | Mendix (updates) |
| **Deployment Pipeline** | Mendix | Mendix | Mendix/Customer | Customer | Customer |
| **Container Platform** | Mendix | Mendix | Mendix/Customer | Customer | N/A |
| **Infrastructure** | Mendix | Mendix | Customer | Customer | Customer |
| **Database** | Mendix | Mendix | Customer | Customer | Customer |
| **File Storage** | Mendix | Mendix | Customer | Customer | Customer |
| **Network** | Mendix | Mendix | Customer | Customer | Customer |
| **Security** | Mendix | Mendix | Shared | Customer | Customer |
| **Monitoring** | Mendix | Mendix | Mendix/Customer | Customer | Customer |
| **Backup/DR** | Mendix | Mendix | Customer | Customer | Customer |
| **Scaling** | Mendix (auto) | Mendix (auto) | Mendix/Auto | Customer | Customer |
| **OS Updates** | Mendix | Mendix | Mendix/Customer | Customer | Customer |

---

### 9.2 Feature Comparison

| Feature | Mendix Cloud | Cloud Dedicated | Azure | SAP BTP | Kubernetes | On-Prem |
|---------|--------------|----------------|-------|---------|------------|---------|
| **Multi-AZ HA** | ✅ Built-in | ✅ Built-in | ✅ Configurable | ✅ Built-in | ⚠️ Customer | ⚠️ Customer |
| **Auto-scaling** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ⚠️ Configure | ❌ No |
| **Regional Failback** | ✅ Premium+ | ✅ Premium+ | ❌ No | ❌ No | ⚠️ Customer | ❌ No |
| **Mendix Pipelines** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes (2025) | ❌ No |
| **Built-in Monitoring** | ✅ Grafana | ✅ Grafana | ✅ Grafana | ⚠️ Basic | ⚠️ Setup | ⚠️ Setup |
| **Automatic Backups** | ✅ Yes | ✅ Yes | ⚠️ Configure | ⚠️ Configure | ❌ Customer | ❌ Customer |
| **One-click Deploy** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ⚠️ Via Portal | ❌ No |
| **Air-gapped Mode** | ❌ No | ❌ No | ❌ No | ❌ No | ✅ Yes | ✅ Yes |
| **Custom Network** | ❌ No | ✅ Yes | ✅ Yes | ⚠️ Limited | ✅ Yes | ✅ Yes |
| **Data Residency** | ✅ Regional | ✅ Regional | ✅ Full Control | ✅ Regional | ✅ Full Control | ✅ Full Control |

Legend:
- ✅ Fully supported/included
- ⚠️ Requires configuration or partial support
- ❌ Not available or not applicable

---

## 10. Recommendations by Use Case

### 10.1 Startups & Small Teams
**Recommended:** Mendix Cloud (Multi-tenant)
- **Why:** Lowest TCO, zero operations overhead, fast time-to-market
- **Tier:** Free (development) → Basic → Standard
- **Alternative:** Mendix on Azure (if Azure-committed with credits)

---

### 10.2 Enterprise - Standard Compliance
**Recommended:** Mendix Cloud (Standard/Premium)
- **Why:** Balanced operations overhead, 99.5-99.95% SLA, proven platform
- **Tier:** Standard (multi-app) or Premium
- **Consideration:** Premium for HA/failback requirements

---

### 10.3 Enterprise - Strict Compliance (SOC2, HIPAA, etc.)
**Recommended:** Mendix Cloud Dedicated or Mendix on Azure
- **Why:** Single-tenant isolation, custom network integration, audit capabilities
- **Tier:** Premium
- **Alternative:** Kubernetes (if existing K8s investment)

---

### 10.4 Enterprise - Data Residency (GDPR, Germany)
**Recommended:** Mendix on STACKIT or Mendix Cloud (EU regions)
- **Why:** German/Austrian infrastructure, GDPR-native, sovereign cloud
- **Tier:** Premium
- **Alternative:** Mendix Cloud with strict region selection

---

### 10.5 SAP Ecosystem Integration
**Recommended:** Mendix on SAP BTP
- **Why:** Native SAP integration, leverages SAP authentication, connectivity services
- **Tier:** Per SAP procurement
- **Alternative:** Mendix Cloud with SAP OData connectors

---

### 10.6 Multi-Cloud Strategy
**Recommended:** Mendix on Kubernetes
- **Why:** Portable across EKS, AKS, GKE; consistent deployment model
- **Tier:** Standard/Premium
- **Alternative:** Mix of Mendix on Azure + AWS (via Mendix Cloud)

---

### 10.7 Existing Kubernetes Investment
**Recommended:** Mendix on Kubernetes
- **Why:** Leverage existing K8s expertise, integrate with existing platform
- **Tier:** Standard/Premium (based on support needs)
- **Consideration:** Requires K8s operational maturity

---

### 10.8 Air-gapped/High Security Environments
**Recommended:** Mendix on Kubernetes (Standalone) or On-Premises
- **Why:** Fully disconnected from internet, complete control
- **Tier:** Premium (for support)
- **Consideration:** Highest operational overhead

---

### 10.9 Rapid Prototyping/POC
**Recommended:** Mendix Cloud (Free tier)
- **Why:** Zero cost, instant provisioning, full platform access
- **Tier:** Free
- **Alternative:** Azure Marketplace free trial (120 days)

---

### 10.10 Legacy Infrastructure Modernization
**Recommended:** Mendix on Windows/Linux → Migrate to Kubernetes/Cloud
- **Why:** Incremental migration path from existing VMs
- **Strategy:** Start on-prem, migrate to containers, then to cloud
- **Consideration:** Plan for containerization early

---

## 11. Key Decision Factors

### 11.1 Total Cost of Ownership (TCO)

**Lowest to Highest TCO:**
1. Mendix Cloud (Multi-tenant) - Fully managed, no infrastructure costs
2. Mendix Cloud Dedicated - Managed, dedicated resources
3. Mendix on Azure/SAP/STACKIT - Infrastructure costs + reduced operations
4. Mendix on Kubernetes - Infrastructure + K8s operations + platform license
5. On-Premises - Full infrastructure + operations + maintenance

---

### 11.2 Operational Overhead

**Lowest to Highest Overhead:**
1. Mendix Cloud - Zero operations (fully managed)
2. Mendix Cloud Dedicated - Near-zero (network integration only)
3. Mendix on Azure - Low (automated, some config)
4. Mendix on SAP BTP - Low-Medium (CF management)
5. Mendix on Kubernetes - Medium-High (K8s expertise required)
6. On-Premises - Highest (full infrastructure management)

---

### 11.3 Time to Production

**Fastest to Slowest:**
1. Mendix Cloud - Minutes (one-click deploy)
2. Mendix on Azure - Under 30 minutes (automated)
3. Mendix Cloud Dedicated - Days (network setup)
4. Mendix on SAP BTP - Days (CF setup)
5. Mendix on Kubernetes - Weeks (cluster setup, operator install)
6. On-Premises - Weeks to Months (full infrastructure)

---

### 11.4 Control & Customization

**Least to Most Control:**
1. Mendix Cloud - Limited customization, full management
2. Mendix Cloud Dedicated - Network control, managed runtime
3. Mendix on Azure/SAP - Infrastructure control, managed runtime
4. Mendix on Kubernetes - Full infrastructure + runtime config
5. On-Premises - Complete control over all aspects

---

## 12. Official Resources

### 12.1 Primary Documentation

- **Main Deployment Guide:** [https://docs.mendix.com/developerportal/deploy/](https://docs.mendix.com/developerportal/deploy/)
- **Deployment Options Evaluation:** [https://www.mendix.com/evaluation-guide/deployment/flexibility/options/](https://www.mendix.com/evaluation-guide/deployment/flexibility/options/)
- **System Requirements:** [https://docs.mendix.com/refguide/system-requirements/](https://docs.mendix.com/refguide/system-requirements/)

### 12.2 Platform-Specific Documentation

**Mendix Cloud:**
- Documentation: [https://docs.mendix.com/developerportal/deploy/mendix-cloud-deploy/](https://docs.mendix.com/developerportal/deploy/mendix-cloud-deploy/)
- Evaluation Guide: [https://www.mendix.com/evaluation-guide/deployment/mendix-cloud/cloud/](https://www.mendix.com/evaluation-guide/deployment/mendix-cloud/cloud/)

**Mendix on Kubernetes:**
- Documentation: [https://docs.mendix.com/developerportal/deploy/private-cloud/](https://docs.mendix.com/developerportal/deploy/private-cloud/)
- Supported Providers: [https://docs.mendix.com/developerportal/deploy/private-cloud-supported-environments/](https://docs.mendix.com/developerportal/deploy/private-cloud-supported-environments/)
- Upgrade Guide: [https://docs.mendix.com/developerportal/deploy/private-cloud-upgrade-guide/](https://docs.mendix.com/developerportal/deploy/private-cloud-upgrade-guide/)

**Mendix on Azure:**
- Main Page: [https://www.mendix.com/mendix-on-azure/](https://www.mendix.com/mendix-on-azure/)
- Azure Marketplace: Available with 120-day free trial

**Mendix on SAP BTP:**
- Documentation: [https://docs.mendix.com/developerportal/deploy/sap-cloud-platform/](https://docs.mendix.com/developerportal/deploy/sap-cloud-platform/)
- Evaluation Guide: [https://www.mendix.com/evaluation-guide/deployment/partner-cloud/sap-btp/](https://www.mendix.com/evaluation-guide/deployment/partner-cloud/sap-btp/)

**On-Premises:**
- Design Guide: [https://docs.mendix.com/developerportal/deploy/on-premises-design/](https://docs.mendix.com/developerportal/deploy/on-premises-design/)
- Windows Deployment: [https://docs.mendix.com/developerportal/deploy/deploy-mendix-on-microsoft-windows/](https://docs.mendix.com/developerportal/deploy/deploy-mendix-on-microsoft-windows/)

**Docker:**
- GitHub Repository: [https://github.com/mendix/docker-mendix-buildpack](https://github.com/mendix/docker-mendix-buildpack)
- Documentation: [https://docs.mendix.com/developerportal/deploy/docker-deploy/](https://docs.mendix.com/developerportal/deploy/docker-deploy/)

**Cloud Foundry:**
- GitHub Repository: [https://github.com/mendix/cf-mendix-buildpack](https://github.com/mendix/cf-mendix-buildpack)
- Documentation: [https://docs.mendix.com/developerportal/deploy/cloud-foundry-deploy/](https://docs.mendix.com/developerportal/deploy/cloud-foundry-deploy/)

### 12.3 Release Notes

- **Mendix Cloud:** [https://docs.mendix.com/releasenotes/developer-portal/mendix-cloud/](https://docs.mendix.com/releasenotes/developer-portal/mendix-cloud/)
- **Mendix on Kubernetes:** [https://docs.mendix.com/releasenotes/developer-portal/mendix-for-private-cloud/](https://docs.mendix.com/releasenotes/developer-portal/mendix-for-private-cloud/)
- **SAP BTP:** [https://docs.mendix.com/releasenotes/developer-portal/sap-cloud-platform/](https://docs.mendix.com/releasenotes/developer-portal/sap-cloud-platform/)
- **On-Premises:** [https://docs.mendix.com/releasenotes/developer-portal/on-premises/](https://docs.mendix.com/releasenotes/developer-portal/on-premises/)

### 12.4 Version Support

- **LTS/MTS Policy:** [https://docs.mendix.com/releasenotes/studio-pro/lts-mts/](https://docs.mendix.com/releasenotes/studio-pro/lts-mts/)
- **Pricing:** [https://www.mendix.com/pricing/](https://www.mendix.com/pricing/)

---

## 13. Summary & Conclusions

### 13.1 Key Takeaways

1. **Deployment Flexibility:** Mendix offers industry-leading deployment flexibility spanning fully-managed SaaS to air-gapped on-premises installations.

2. **Cloud-Native Architecture:** All deployment options benefit from Mendix's cloud-native, Twelve-Factor App architecture with auto-scaling, auto-healing, and containerization.

3. **Licensing Simplicity:** User-based pricing model (not per-app or per-developer) with predictable costs. Free tier for development and prototyping.

4. **Kubernetes-First Strategy:** Strong emphasis on Kubernetes across cloud providers (EKS, AKS, GKE, OpenShift) with 2025 enhancements including Mendix Pipelines CI/CD support.

5. **Strategic Partnerships:** Deep integrations with AWS, Azure, SAP, and STACKIT providing optimized experiences for each platform.

6. **Recent Innovation (2024-2025):**
   - Cloud Tokens for flexible resource allocation
   - GenAI Resource Packs for AI workloads
   - Mendix Pipelines expansion to K8s and Azure
   - New cloud regions (Osaka, Seoul, Jakarta, São Paulo)
   - 120-day Azure free trial

7. **Version Support:** Clear LTS/MTS strategy with Mendix 9, 10, 11 currently supported. Mendix 10.24 LTS released June 2025.

8. **Container Evolution:** Migration from Ubuntu to Red Hat UBI8/UBI9 base images. Cloud Foundry API v3 adoption.

---

### 13.2 Deployment Selection Framework

**Choose Mendix Cloud if:**
- Minimizing TCO and operations overhead
- No specific cloud vendor requirements
- Need 99.5-99.95% SLA with minimal effort
- Want fastest time-to-production

**Choose Mendix Cloud Dedicated if:**
- Need single-tenant isolation
- Require custom network integration
- SOC2 or similar compliance requirements
- Want Mendix to manage infrastructure

**Choose Mendix on Azure if:**
- Committed to Azure ecosystem
- Want infrastructure in your subscription
- Need private VNet deployment
- Have Azure credits or EDP agreements

**Choose Mendix on SAP BTP if:**
- Deep SAP ecosystem integration
- Leveraging SAP authentication/authorization
- SAP procurement channels required
- Cloud Foundry expertise available

**Choose Mendix on STACKIT if:**
- German/Austrian data residency required
- GDPR compliance priority
- Sovereign cloud requirements
- DACH region focus

**Choose Mendix on Kubernetes if:**
- Existing Kubernetes investment
- Multi-cloud portability required
- Need full infrastructure control
- Air-gapped environments (standalone mode)
- Have Kubernetes operational expertise

**Choose On-Premises if:**
- Legacy infrastructure constraints
- Incremental migration strategy
- Complete isolation required
- No cloud connectivity available
- Regulatory prohibition on cloud

---

### 13.3 Future-Proofing Considerations

1. **Containerization First:** Even for on-premises, adopt Docker/containerization early for future cloud migration paths.

2. **Kubernetes Skills:** Invest in K8s expertise for multi-cloud flexibility and operational maturity.

3. **LTS Tracking:** Plan upgrades around LTS releases (every ~1.5 years) for stability; use MTS for faster feature access.

4. **Cloud Tokens:** Consider new token model for dynamic workloads versus fixed resource packs.

5. **GenAI Ready:** Leverage GenAI Resource Packs for AI/ML experimentation as strategic differentiator.

6. **Mendix Pipelines:** Adopt CI/CD automation across all deployment types for consistency and reduced manual effort.

---

## Appendix A: Glossary

- **AKS:** Azure Kubernetes Service
- **AZ:** Availability Zone (AWS infrastructure isolation)
- **BTP:** Business Technology Platform (SAP)
- **CF:** Cloud Foundry
- **DACH:** Germany (Deutschland), Austria, Switzerland
- **EDP:** Enterprise Discount Program (AWS)
- **EKS:** Elastic Kubernetes Service (AWS)
- **GKE:** Google Kubernetes Engine
- **GDPR:** General Data Protection Regulation
- **HA:** High Availability
- **LTS:** Long-Term Support
- **MTS:** Medium-Term Support
- **PaaS:** Platform as a Service
- **RPO:** Recovery Point Objective
- **RTO:** Recovery Time Objective
- **SLA:** Service Level Agreement
- **TCO:** Total Cost of Ownership
- **UBI:** Universal Base Image (Red Hat)

---

## Appendix B: Quick Reference - Deployment Checklist

### Mendix Cloud
- [ ] Mendix account created
- [ ] Platform license tier selected (Basic/Standard/Premium)
- [ ] Cloud Resource Packs purchased (if Standard/Premium)
- [ ] Region selected
- [ ] App deployed via Developer Portal

### Mendix on Azure
- [ ] Azure subscription active
- [ ] Mendix platform license obtained
- [ ] Azure Marketplace offering accessed
- [ ] VNet requirements defined
- [ ] Deployment initiated (under 30 min)
- [ ] DNS and certificates configured

### Mendix on Kubernetes
- [ ] Kubernetes cluster version verified (1.19+ for Operator v2.x)
- [ ] kubectl access confirmed
- [ ] Cluster admin permissions obtained
- [ ] Mendix Operator installed
- [ ] Database service deployed (PostgreSQL recommended)
- [ ] Storage solution configured
- [ ] Load balancer configured
- [ ] TLS certificates obtained
- [ ] Mendix Developer Portal connected (if not standalone)
- [ ] First app deployed and tested

### On-Premises (Windows)
- [ ] Windows Server installed
- [ ] IIS configured
- [ ] Java Runtime Environment installed
- [ ] Database server deployed (SQL Server or PostgreSQL)
- [ ] Mendix Service Console installed
- [ ] Network/firewall rules configured
- [ ] Backup solution implemented
- [ ] Monitoring configured

### On-Premises (Linux)
- [ ] Linux server provisioned
- [ ] Java Runtime Environment installed
- [ ] Python installed (for m2ee-tools)
- [ ] NGINX or Apache configured
- [ ] Database server deployed (PostgreSQL recommended)
- [ ] m2ee-tools installed and configured
- [ ] systemd service created
- [ ] Network/firewall rules configured
- [ ] Backup solution implemented
- [ ] Monitoring configured

---

**Document End**

*This research report is based on publicly available Mendix documentation, evaluation guides, and official announcements as of December 24, 2025. For the most current information, always consult the official Mendix documentation at https://docs.mendix.com.*
