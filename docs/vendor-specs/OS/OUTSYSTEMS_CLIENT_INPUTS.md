# OutSystems Pricing - Client Input Factors

**Purpose**: Define all inputs needed to accurately size and price an OutSystems deployment
**Source**: OutSystems Partner Price Calculator (Official)

---

## Client Needs Assessment Questionnaire

### 1. Platform Selection

**Question**: Which OutSystems platform best fits your needs?

| Platform | Description | When to Choose |
|----------|-------------|----------------|
| **ODC** (OutSystems Developer Cloud) | Cloud-native, Kubernetes-based, fully managed | New projects, cloud-first strategy, rapid scaling needs |
| **O11** (OutSystems 11) | Traditional .NET platform, mature ecosystem | Existing O11 investments, on-premises requirements, specific compliance needs |

**Input Required**:
- [ ] Platform: `ODC` or `O11`

---

### 2. Deployment Type (O11 Only)

**Question**: How do you want to host your O11 environment?

| Type | Description | When to Choose |
|------|-------------|----------------|
| **Cloud** | OutSystems-managed cloud infrastructure | Prefer managed service, minimize ops overhead |
| **Self-Managed** | Customer-managed infrastructure | Data sovereignty, existing DC, specific security requirements |

**Input Required**:
- [ ] Deployment Type: `Cloud` or `Self-Managed`
- [ ] If Self-Managed: Cloud Provider (`Azure`, `AWS`, `On-Premises`)

---

### 3. Bundle Tier Selection

**Question**: Which package tier matches your organization's needs?

#### ODC Bundles

| Tier | Price | Best For |
|------|-------|----------|
| **Business Basics** | $88,000/yr | Teams getting started, standard business apps |
| **Business Critical** | $179,000/yr | Mission-critical apps requiring HA and Sentry |
| **Strategic CoE** | $179,000/yr | Organizations building a Center of Excellence |
| **A La Carte** | Varies | Custom configurations not matching bundles |

#### O11 Cloud Bundles

| Tier | Price | Best For |
|------|-------|----------|
| **Business Basics** | $109,000/yr | Standard enterprise deployments |
| **Business Critical** | $182,000/yr | Mission-critical with Sentry (inc. HA) |
| **Strategic CoE** | $211,000/yr | Large deployments with multiple environments |

#### O11 Self-Managed Bundles

| Tier | Price | Best For |
|------|-------|----------|
| **Business Basics** | $109,000/yr | Standard self-hosted deployments |
| **Business Critical** | $145,000/yr | Mission-critical with Disaster Recovery |
| **Strategic CoE** | $174,000/yr | Large deployments with DR and extra environments |

**Input Required**:
- [ ] Bundle Tier: `Business Basics` | `Business Critical` | `Strategic CoE` | `A La Carte`

---

### 4. Application Scale (Application Objects)

**Question**: How many Application Objects (AOs) do you anticipate?

**What are AOs?**
- AOs = Screens + Database Entities/Tables + API Methods + Timers + Processes
- Each screen, table, and API endpoint counts as 1 AO
- Bundles include 450 AOs; A La Carte includes 150 AOs

**Sizing Guidelines**:
| App Complexity | Typical AOs per App | Example |
|---------------|--------------------|---------|
| Simple | 10-30 | Single-purpose utility app |
| Medium | 30-100 | Departmental workflow app |
| Complex | 100-300 | Enterprise portal with integrations |
| Large | 300+ | Core business system |

**Input Required**:
- [ ] Expected number of applications: ___
- [ ] Average complexity per app: `Simple` | `Medium` | `Complex` | `Large`
- [ ] **Total estimated AOs**: ___ (calculate: apps × avg AOs per complexity)

**Or direct input**:
- [ ] Total Application Objects needed: ___

---

### 5. User Capacity

#### 5.1 Internal Users (Named Users)

**Question**: How many internal employees/contractors will use the applications?

**Definition**: Users who log in with organizational credentials (SSO, AD, etc.)

| User Range | Annual Cost per 100 (O11) |
|------------|---------------------------|
| 100-1,000 | $12,100 per 100 users |
| 1,001-10,000 | $2,420 per 100 users |
| 10,001+ | $242 per 100 users |

**Input Required**:
- [ ] Number of internal users: ___
- [ ] Expected growth rate per year: ___% (for multi-year planning)

#### 5.2 External Users (Anonymous/Public)

**Question**: How many external users (customers, partners, public) will access your applications?

**Definition**: Users accessing public-facing applications without internal authentication

| User Range | Annual Cost per 1,000 (O11) |
|------------|----------------------------|
| 1,000-10,000 | $4,840 per 1,000 users |
| 10,001-250,000 | $1,452 per 1,000 users |
| 250,001+ | $30.25 per 1,000 users |

**Input Required**:
- [ ] Number of external users: ___
- [ ] Peak concurrent external users: ___

#### 5.3 Unlimited Users Option

**Question**: Do you need unlimited user capacity?

**When to consider**:
- Large public-facing applications with unpredictable traffic
- Consumer apps with viral growth potential
- Pricing simplicity for budgeting

**Price**: **$60,500 per AO pack/year** (scales with application size!)

Example:
- 150 AOs (1 pack): $60,500/year
- 300 AOs (2 packs): $121,000/year
- 450 AOs (3 packs): $181,500/year

**Input Required**:
- [ ] Use unlimited users: `Yes` | `No`
- [ ] If yes, note that cost scales with AO pack count

---

### 6. Environments

**Question**: How many environments do you need?

**Standard Pattern**:
| Environment Type | Purpose | Typically Needed |
|-----------------|---------|------------------|
| Development | Building and testing code | 1 per team |
| Non-Production | QA, staging, UAT | 1-3 |
| Production | Live applications | 1-2 |

**Bundle Inclusions**:
- Business Basics: 1 Dev, 1 NonProd, 1 Prod
- Business Critical: 1 Dev, 1 NonProd, 1 Prod
- Strategic CoE: 1 Dev, 2 NonProd, 2 Prod

**Input Required**:
- [ ] Development environments needed: ___
- [ ] Non-production environments needed: ___
- [ ] Production environments needed: ___

---

### 7. High Availability & Business Continuity

**Question**: What are your uptime and recovery requirements?

#### 7.1 High Availability (Cloud Only)

| Requirement | Solution | Cost |
|-------------|----------|------|
| 99.9% uptime | Standard | Included |
| 99.95%+ uptime | High Availability | ODC: $18,150/AO pack, O11: $12,100/AO pack |
| Mission-critical | Sentry (includes HA) | ODC: $6,050/AO pack, O11: $24,200/AO pack |

**Input Required**:
- [ ] Required SLA: ___% uptime
- [ ] Include HA: `Yes` | `No`
- [ ] Include Sentry: `Yes` | `No`

#### 7.2 Disaster Recovery

**When needed**: Business continuity requirements, regulatory compliance, geographic redundancy

**Cost**: $12,100 per AO pack/year

**Input Required**:
- [ ] Include Disaster Recovery: `Yes` | `No`
- [ ] RPO requirement: ___ hours
- [ ] RTO requirement: ___ hours

---

### 8. Add-On Features

**Question**: Which additional features do you need?

#### Support Options

| Option | Description | Cost |
|--------|-------------|------|
| Standard (8x5) | Business hours support | Included (ODC Business Basics) |
| Extended (24x7) | Round-the-clock support | ODC: $6,050/AO pack |
| Premium (24x7) | Priority support with SLA | ODC: $9,680/AO pack, O11: $3,630/AO pack |

**Input Required**:
- [ ] Support Level: `Standard` | `Extended` | `Premium`

#### Security Features

| Feature | Description | Cost |
|---------|-------------|------|
| AppShield | Runtime application protection | Tiered by user count ($18,150 - $1,476,200) |
| Private Gateway (ODC) | Private network connectivity | $1,210 per AO pack |

**Input Required**:
- [ ] Include AppShield: `Yes` | `No`
- [ ] If yes, number of users to protect: ___
- [ ] Include Private Gateway (ODC): `Yes` | `No`

#### Infrastructure Features (Cloud Only)

| Feature | Description | Cost |
|---------|-------------|------|
| Load Test Environment | Dedicated performance testing | O11: $6,050/AO pack |
| Log Streaming | Export logs to external systems | O11: $7,260/year |
| Database Replica | Read replica for analytics | O11: $96,800/year |

**Input Required**:
- [ ] Include Load Test Environment: `Yes` | `No`
- [ ] Include Log Streaming: `Yes` | `No`
- [ ] Include Database Replica: `Yes` | `No`

---

### 9. Professional Services

**Question**: Do you need OutSystems professional services?

#### Success Plans

| Plan | Description | Cost |
|------|-------------|------|
| Essential | Standard support package | $30,250/year |
| Premier | Enhanced support with dedicated resources | $60,500/year |

#### Training (Bootcamps)

| Type | Description | Cost |
|------|-------------|------|
| Dedicated Group Session | Private training for your team | $2,670/session |
| Public Session | Join scheduled public training | $480/session |

#### Consulting

| Service | Description | Cost |
|---------|-------------|------|
| Expert Day | OutSystems expert consulting | $1,400/day |

**Input Required**:
- [ ] Success Plan: `None` | `Essential` | `Premier`
- [ ] Dedicated training sessions needed: ___
- [ ] Public training seats needed: ___
- [ ] Expert consulting days needed: ___

---

### 10. Self-Managed Infrastructure (O11 Self-Managed Only)

**Question**: What cloud infrastructure will you use?

#### Azure Options

| Instance Type | Specs | Hourly Rate | Monthly (est.) |
|--------------|-------|-------------|----------------|
| F4s_v2 | 4 vCPU, 8 GB RAM | $0.169 | ~$123 |
| D4s_v3 | 4 vCPU, 16 GB RAM | $0.192 | ~$140 |
| D8s_v3 | 8 vCPU, 32 GB RAM | $0.384 | ~$280 |
| D16s_v3 | 16 vCPU, 64 GB RAM | $0.768 | ~$561 |

#### AWS Options

| Instance Type | Specs | Hourly Rate | Monthly (est.) |
|--------------|-------|-------------|----------------|
| m5.large | 2 vCPU, 8 GB RAM | $0.096 | ~$70 |
| m5.xlarge | 4 vCPU, 16 GB RAM | $0.192 | ~$140 |
| m5.2xlarge | 8 vCPU, 32 GB RAM | $0.384 | ~$280 |

**Input Required**:
- [ ] Cloud Provider: `Azure` | `AWS` | `On-Premises`
- [ ] Instance type: ___
- [ ] Front-end servers per environment: ___
- [ ] Deployment controller servers: ___

---

## Summary: Complete Input Checklist

### Required Inputs

1. [ ] Platform (ODC/O11)
2. [ ] Deployment Type (Cloud/Self-Managed) - O11 only
3. [ ] Bundle Tier or A La Carte
4. [ ] Total Application Objects
5. [ ] Internal Users count
6. [ ] External Users count (if applicable)

### Optional Inputs (for customization)

7. [ ] Additional environments needed
8. [ ] High Availability requirement
9. [ ] Sentry requirement
10. [ ] Disaster Recovery requirement
11. [ ] Support level upgrade
12. [ ] AppShield users
13. [ ] Private Gateway (ODC)
14. [ ] Load Test Environment (O11 Cloud)
15. [ ] Log Streaming (O11 Cloud)
16. [ ] Database Replica (O11 Cloud)
17. [ ] Success Plan
18. [ ] Training sessions
19. [ ] Expert consulting days
20. [ ] Cloud infrastructure specs (Self-Managed)

---

## Sample Client Profile Templates

### Startup / Small Business
```
Platform: ODC
Bundle: Business Basics ($88k)
AOs: 450 (3 packs, included in bundle)
Users: 50-100 internal (included in bundle)
Environments: 3 (included: 1 Dev, 1 NonProd, 1 Prod)
Support: 8x5 (included)
Add-ons: Private Gateway (included)
Total: $88,000/year
```

### Mid-Market Enterprise
```
Platform: O11 Cloud
Bundle: Business Critical ($182k)
AOs: 450-900 (3-6 packs)
Users: 500-2,000 internal (tiered pricing applies)
  - First 100 included
  - 200-1000: $12,100/100 users
  - 1001+: $2,420/100 users
Environments: 3 (included) + additional as needed
Add-ons: Sentry with HA (included), AppShield (Tier 1-2)
Total: $200,000 - $280,000/year
```

### Large Enterprise
```
Platform: O11 Self-Managed
Bundle: Strategic CoE ($174k)
AOs: 1,500+ (10+ packs)
Users: 5,000+ internal (tiered: mostly at $24.20/user)
       50,000+ external (tiered: $4.84-$1.45/user)
Environments: 5 (included: 1 Dev, 2 NonProd, 2 Prod) + extras
Add-ons: DR (included), AppShield (Tier 2-3)
Services: Premier Success Plan ($60,500)
Note: Add-ons scale with AO packs!
Total: $400,000 - $600,000/year
```

### Key Pricing Notes
- **Unlimited Users**: $60,500 × AO packs (not flat!)
- **ODC Users**: Flat $6,050 per pack (100 internal, 1000 external)
- **O11 Users**: Tiered pricing with volume discounts
- **AppShield**: Flat tier pricing based on user count (19 tiers)
- **Most Add-ons**: Price shown is per AO pack

---

## Data Collection Form

For implementation in the UI, collect these inputs in this order:

```
Step 1: Platform & Deployment
├── Platform selection (ODC/O11)
└── Deployment type (Cloud/Self-Managed) [O11 only]

Step 2: Package Selection
└── Bundle tier cards with "Build Your Own" option

Step 3: Application Scale
├── Total AOs slider/input
└── AO estimation helper (apps × complexity)

Step 4: User Capacity
├── Internal users input
├── External users input
└── Unlimited users toggle

Step 5: Add-Ons & Features
├── HA/Sentry/DR toggles
├── Support level selection
├── Security features
└── Infrastructure features

Step 6: Services
├── Success Plan selection
└── Training & consulting

Step 7: Review & Calculate
└── Full cost breakdown
```
