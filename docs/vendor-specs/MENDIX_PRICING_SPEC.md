# Mendix Pricing Specification

**Date**: January 2026
**Source**: Mendix Deployment Options PriceBook (June 2025)
**Purpose**: Complete pricing reference for Mendix implementation

---

## Executive Summary

Mendix pricing is structured around deployment options, resource packs, and user licensing. Key components:

1. **Deployment Options**: Mendix Cloud (SaaS), Cloud Dedicated, Azure, Kubernetes, On-Premises
2. **Resource Packs**: Standard, Premium (99.95% SLA), Premium Plus (Multi-region)
3. **Cloud Tokens**: Flexible resource allocation unit ($51.60 per token)
4. **User Licensing**: Internal users (per 100) and External users (per 250K)

---

## 1. Cloud Tokens

**Mendix Cloud Token**: $51.60

Cloud tokens provide flexible resource allocation. Resource packs are priced in token equivalents.

---

## 2. Mendix Cloud Resource Packs

### 2.1 Standard Resource Packs (99.5% SLA)

| Size | Annual Price | Monthly Equiv. | Tokens |
|------|-------------|----------------|--------|
| XS | $516 | $43 | 10 |
| S | $1,032 | $86 | 20 |
| M | $2,580 | $215 | 50 |
| L | $5,160 | $430 | 100 |
| XL | $10,320 | $860 | 200 |
| XXL | $20,640 | $1,720 | 400 |
| XXXL | $41,280 | $3,440 | 800 |
| 4XL | $82,560 | $6,880 | 1,600 |
| XS-SDB | $1,032 | $86 | 20 |
| S-MDB | $2,580 | $215 | 50 |
| M-LDB | $5,160 | $430 | 100 |
| L-XLDB | $10,320 | $860 | 200 |
| XL-XXLDB | $20,640 | $1,720 | 400 |
| XXL-XXXLDB | $41,280 | $3,440 | 800 |
| XXXL-4XLDB | $82,560 | $6,880 | 1,600 |
| 4XL-5XLDB | $115,584 | $9,632 | 2,240 |

### 2.2 Premium Resource Packs (99.95% SLA)

Premium packs provide enhanced SLA with 99.95% uptime guarantee.

| Size | Annual Price | Monthly Equiv. | Tokens |
|------|-------------|----------------|--------|
| S | $1,548 | $129 | 30 |
| M | $3,870 | $322.50 | 75 |
| L | $7,740 | $645 | 150 |
| XL | $15,480 | $1,290 | 300 |
| XXL | $30,960 | $2,580 | 600 |
| XXXL | $61,920 | $5,160 | 1,200 |
| 4XL | $123,840 | $10,320 | 2,400 |
| S-MDB | $3,870 | $322.50 | 75 |
| M-LDB | $7,740 | $645 | 150 |
| L-XLDB | $15,480 | $1,290 | 300 |
| XL-XXLDB | $30,960 | $2,580 | 600 |
| XXL-XXXLDB | $61,920 | $5,160 | 1,200 |
| XXXL-4XLDB | $123,840 | $10,320 | 2,400 |
| 4XL-5XLDB | $173,376 | $14,448 | 3,360 |

### 2.3 Premium Plus Resource Packs (Multi-Region Failover)

Premium Plus provides multi-region disaster recovery with automatic failover.

| Size | Annual Price | Monthly Equiv. | Tokens |
|------|-------------|----------------|--------|
| XL | $20,640 | $1,720 | 400 |
| XXL | $41,280 | $3,440 | 800 |
| XXXL | $82,560 | $6,880 | 1,600 |
| 4XL | $165,120 | $13,760 | 3,200 |
| XL-XXLDB | $41,280 | $3,440 | 800 |
| XXL-XXXLDB | $82,560 | $6,880 | 1,600 |
| XXXL-4XLDB | $165,120 | $13,760 | 3,200 |
| 4XL-5XLDB | $288,960 | $24,080 | 5,600 |

---

## 3. Mendix Cloud Dedicated

**Annual Price**: $368,100

Single-tenant Mendix Cloud instance with:
- Dedicated AWS VPC
- Full infrastructure isolation
- Enhanced security and compliance
- Requires separate Resource Pack purchase

---

## 4. Private Cloud Deployment Options

### 4.1 Mendix on Azure

| Component | Annual Price |
|-----------|-------------|
| Base Package (3 environments) | $6,612 |
| Additional Environment | $722.40 each |

**Includes:**
- Automated AKS setup
- Azure Container Registry
- Built-in monitoring (Grafana)
- DDoS protection
- Private VNet deployment

### 4.2 Mendix on Kubernetes

| Component | Annual Price |
|-----------|-------------|
| Base Package (3 environments) | $6,360 |

**Additional Environment Tiered Pricing:**

| Environment Range | Price per Environment |
|-------------------|----------------------|
| 4-50 environments | $552/year |
| 51-100 environments | $408/year |
| 101-150 environments | $240/year |
| 151+ environments | FREE |

**Supported Platforms:**
- Amazon EKS
- Microsoft AKS
- Google GKE
- Red Hat OpenShift
- Any Kubernetes 1.19+

---

## 5. On-Premises Deployment

### 5.1 Mendix on Server (Windows/Linux/Docker)

| License Type | Annual Price |
|--------------|-------------|
| Per-App License | $6,612/app |
| Unlimited Apps | $33,060 |

### 5.2 Mendix on SAP BTP / STACKIT

Same pricing as Mendix on Server:
- Per-App: $6,612/year
- Unlimited: $33,060/year

---

## 6. User Licensing

User licensing is required for ALL deployment options.

| User Type | Pack Size | Annual Price | Per-User |
|-----------|-----------|-------------|----------|
| Internal Users | 100 users | $40,800 | $408/user |
| External Users | 250,000 users | $60,000 | $0.24/user |

**Calculation:**
- Internal: `ceil(users / 100) × $40,800`
- External: `ceil(users / 250,000) × $60,000`

---

## 7. Pricing Examples

### Example 1: Small Mendix Cloud Deployment

**Configuration:**
- Deployment: Mendix Cloud (SaaS)
- Resource Pack: Standard M
- Internal Users: 50
- External Users: 0

| Component | Calculation | Annual Price |
|-----------|-------------|--------------|
| Standard M Resource Pack | - | $2,580 |
| Internal Users | 1 pack × $40,800 | $40,800 |
| **Total** | | **$43,380** |

---

### Example 2: Medium Kubernetes Deployment

**Configuration:**
- Deployment: Mendix on Kubernetes
- Environments: 25
- Internal Users: 500
- External Users: 100,000

| Component | Calculation | Annual Price |
|-----------|-------------|--------------|
| K8s Base (3 envs) | - | $6,360 |
| Additional Envs (22) | 22 × $552 | $12,144 |
| Internal Users | 5 packs × $40,800 | $204,000 |
| External Users | 1 pack × $60,000 | $60,000 |
| **Total** | | **$282,504** |

---

### Example 3: Large Azure Deployment with Premium SLA

**Configuration:**
- Deployment: Mendix on Azure
- Environments: 50
- Resource Pack: Premium XL (for each prod env)
- Internal Users: 2,000
- External Users: 500,000

| Component | Calculation | Annual Price |
|-----------|-------------|--------------|
| Azure Base (3 envs) | - | $6,612 |
| Additional Envs (47) | 47 × $722.40 | $33,952.80 |
| Premium XL Resource Packs | 10 × $15,480 | $154,800 |
| Internal Users | 20 packs × $40,800 | $816,000 |
| External Users | 2 packs × $60,000 | $120,000 |
| **Total** | | **$1,131,364.80** |

---

### Example 4: Enterprise Kubernetes at Scale

**Configuration:**
- Deployment: Mendix on Kubernetes
- Environments: 200
- Internal Users: 5,000
- External Users: 2,000,000

| Component | Calculation | Annual Price |
|-----------|-------------|--------------|
| K8s Base (3 envs) | - | $6,360 |
| Envs 4-50 (47 envs) | 47 × $552 | $25,944 |
| Envs 51-100 (50 envs) | 50 × $408 | $20,400 |
| Envs 101-150 (50 envs) | 50 × $240 | $12,000 |
| Envs 151-200 (50 envs) | 50 × $0 | $0 |
| Internal Users | 50 packs × $40,800 | $2,040,000 |
| External Users | 8 packs × $60,000 | $480,000 |
| **Total** | | **$2,584,704** |

---

### Example 5: Mendix Cloud Dedicated Enterprise

**Configuration:**
- Deployment: Mendix Cloud Dedicated
- Resource Packs: Premium Plus 4XL-5XLDB (primary) + XXXL-4XLDB (DR)
- Internal Users: 10,000
- External Users: 5,000,000

| Component | Calculation | Annual Price |
|-----------|-------------|--------------|
| Cloud Dedicated Base | - | $368,100 |
| Premium Plus 4XL-5XLDB | Primary region | $288,960 |
| Premium Plus XXXL-4XLDB | DR region | $165,120 |
| Internal Users | 100 packs × $40,800 | $4,080,000 |
| External Users | 20 packs × $60,000 | $1,200,000 |
| **Total** | | **$6,102,180** |

---

## 8. Comparison: Current Implementation vs Spec

| Component | Current Implementation | Spec | Gap |
|-----------|----------------------|------|-----|
| Cloud Token | Not implemented | $51.60 | Missing |
| Standard XS | $516 | $516 | Correct |
| Standard 4XL-5XLDB | Not implemented | $115,584 | Missing sizes |
| Premium packs | Basic mention | Full tiers | Incomplete |
| Premium Plus | Not implemented | Full tiers | Missing |
| Cloud Dedicated | $368,100 | $368,100 | Correct |
| K8s Base | $6,360 | $6,360 | Correct |
| K8s Tiered Envs | Implemented | Verified | Correct |
| Azure Base | $6,612 | $6,612 | Correct |
| Azure Additional Env | $722.40 | $722.40 | Correct |
| Server Per-App | $6,612 | $6,612 | Correct |
| Server Unlimited | $33,060 | $33,060 | Correct |
| Internal Users | $40,800/100 | $40,800/100 | Correct |
| External Users | $60,000/250K | $60,000/250K | Correct |

---

## 9. Required Implementation Changes

### Priority 1: Add Missing Resource Pack Sizes

- Add all Standard sizes (XS through 4XL-5XLDB)
- Add all Premium sizes (S through 4XL-5XLDB)
- Add all Premium Plus sizes (XL through 4XL-5XLDB)

### Priority 2: Implement Cloud Token Model

- Add Cloud Token as base unit ($51.60)
- Allow flexible token-based resource allocation

### Priority 3: Add Premium Plus Multi-Region

- Implement Premium Plus tier selection
- Add multi-region failover cost calculations

---

## Appendix: Source Document

- **Document**: Mendix Deployment Options PriceBook
- **Version**: June 2025
- **Format**: PDF
- **Location**: `docs/vendor-specs/Mendix Deployment Options PriceBook.pdf`

Additional source:
- **Document**: Mendix Commercial Proposal
- **Format**: Excel
- **Location**: `docs/vendor-specs/Mendix Commercial Proposal.xlsx`
