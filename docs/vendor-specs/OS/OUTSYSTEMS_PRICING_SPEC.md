# OutSystems Pricing Specification

**Date**: January 2026
**Source**: OutSystems Partner Pricing Calculator (27 screenshots)
**Purpose**: Complete pricing reference for OutSystems implementation

---

## Executive Summary

OutSystems pricing is structured around 6 main sections:

1. **Edition**: Base platform license
2. **Deployment**: Cloud vs Self-Managed (O11 only)
3. **Application Scale**: Application Objects (AOs) in packs of 150
4. **End User Capacity**: Internal, External, or Unlimited users
5. **Add-Ons**: Support, HA, Sentry, AppShield, Environments, DR
6. **Services**: Success Plans, Bootcamps, Expert Days (regional pricing)

---

## 1. Edition (Base Platform)

### 1.1 OutSystems Developer Cloud (ODC)

| Bundle | Price | Includes |
|--------|-------|----------|
| Business Basics | $88,000 | ODC Platform, 450 AOs, 100 Internal Users, Dev/Prod runtime, 8x5 Support, Private Gateway |
| Business Critical | $179,000 | + 24x7 Support, High Availability, ODC Sentry |
| Strategic CoE | $179,000 | + 24x7 Support, High Availability, ODC Sentry |

**ODC Platform Base**: $30,250.00
- Includes: 150 AOs, 100 Internal Users, 2 runtime environments (Dev, Prod), 8x5 Support

### 1.2 OutSystems 11 Cloud

| Bundle | Price | Includes |
|--------|-------|----------|
| Business Basics | $109,000 | O11 Enterprise, 450 AOs, 100 Internal Users, 3 environments, 24x7 Support |
| Business Critical | $182,000 | + Sentry (includes HA) |
| Strategic CoE | $211,000 | + 2 Non-prod, 2 Prod environments, Sentry (includes HA) |

**O11 Enterprise Base**: $36,300.00
- Includes: 150 AOs, 100 Internal Users, 3 runtime environments (Dev, Non-Prod, Prod), 24x7 Support

### 1.3 OutSystems 11 Self-Managed

| Bundle | Price | Includes |
|--------|-------|----------|
| Business Basics | $109,000 | O11 Enterprise, 450 AOs, 100 Internal Users, 3 environments, 24x7 Support |
| Business Critical | $145,000 | + Disaster Recovery |
| Strategic CoE | $174,000 | + 2 Non-prod, 2 Prod environments, Disaster Recovery |

**O11 Enterprise Base**: $36,300.00 (same as Cloud)

---

## 2. Application Scale (AO Packs)

Application Objects (AOs) are sold in packs of 150.

| Platform | Per Pack Price | Included in Base |
|----------|---------------|------------------|
| ODC | $18,150.00 | 1 pack (150 AOs) |
| O11 (Cloud & Self-Managed) | $36,300.00 | 1 pack (150 AOs) |

**Calculation**: `(Total AOs / 150) - 1` additional packs × per-pack price

**Example**: 450 AOs on ODC
- Packs needed: 450 / 150 = 3 packs
- Additional packs: 3 - 1 = 2 packs
- Cost: 2 × $18,150 = $36,300.00

---

## 3. End User Capacity

### 3.1 ODC User Pricing (Flat Rate)

| User Type | Pack Size | Per Pack | Included |
|-----------|-----------|----------|----------|
| Internal Users | 100 | $6,050.00 | 1 pack (100 users) |
| External Users | 1,000 | $6,050.00 | 0 packs |
| Unlimited Users | Per AO Pack | $60,500.00 | N/A |

**ODC Calculation**:
- Internal: `ceil((users - 100) / 100)` × $6,050
- External: `ceil(users / 1000)` × $6,050
- Unlimited: `AO_packs` × $60,500

### 3.2 O11 User Pricing (Tiered - Enterprise Edition)

**Internal Users** (per 100 users):

| Tier | User Range | Price per 100 | Per User |
|------|------------|---------------|----------|
| 1 | 200 - 1,000 | $12,100.00 | $121.00 |
| 2 | 1,100 - 10,000 | $2,420.00 | $24.20 |
| 3 | 10,100 - 100,000,000 | $242.00 | $2.42 |

*Note: First 100 users included in base. Tiered pricing starts at user 101.*

**External Users** (per 1,000 users):

| Tier | User Range | Price per 1,000 | Per User |
|------|------------|-----------------|----------|
| 1 | 1,000 - 10,000 | $4,840.00 | $4.84 |
| 2 | 11,000 - 250,000 | $1,452.00 | $1.45 |
| 3 | 251,000 - 100,000,000 | $30.25 | $0.03 |

**Unlimited Users** (O11): $60,500.00 per AO pack

### 3.3 O11 Tiered Pricing Calculation Example

**2,000 Internal Users**:
| Tier | Users | Packs | Rate | Subtotal |
|------|-------|-------|------|----------|
| Base | 100 | - | Included | $0.00 |
| Tier 1 | 900 | 9 | $12,100 | $108,900.00 |
| Tier 2 | 1,000 | 10 | $2,420 | $24,200.00 |
| **Total** | 2,000 | 19 | | **$133,100.00** |

**1,000,000 External Users**:
| Tier | Users | Packs | Rate | Subtotal |
|------|-------|-------|------|----------|
| Tier 1 | 10,000 | 10 | $4,840 | $48,400.00 |
| Tier 2 | 240,000 | 240 | $1,452 | $348,480.00 |
| Tier 3 | 750,000 | 750 | $30.25 | $22,687.50 |
| **Total** | 1,000,000 | 1,000 | | **$419,567.50** |

---

## 4. Add-Ons

### 4.1 ODC Add-Ons

| Add-On | Per Pack of AOs | For 3 Packs |
|--------|-----------------|-------------|
| Support 24x7 Extended | $6,050.00 | $18,150.00 |
| Support 24x7 Premium | $9,680.00 | $29,040.00 |
| AppShield | Pricing Scale | See Section 5 |
| High Availability | $18,150.00 | $54,450.00 |
| Non-Production Runtime | $6,050.00 | $18,150.00 |
| Private Gateway | $1,210.00 | $3,630.00 |
| ODC Sentry | $6,050.00 | $18,150.00 |

### 4.2 O11 Add-Ons

| Add-On | Per Pack of AOs | For 3 Packs | Availability |
|--------|-----------------|-------------|--------------|
| Support 24x7 Extended | Included | Included | Both |
| Support 24x7 Premium | $3,630.00 | $10,890.00 | Both |
| AppShield | Pricing Scale | See Section 5 | Both |
| High Availability | $12,100.00 | $36,300.00 | Cloud only |
| Sentry (includes HA) | $24,200.00 | $72,600.00 | Cloud only |
| Log Streaming | 0.5 TB/month | $7,260.00 | Cloud only |
| Non-Production Environment | $3,630.00 | $10,890.00 | Both |
| Load Test Environment | $6,050.00 | $18,150.00 | Cloud only |
| Environment Pack | $9,680.00 | $29,040.00 | Both |
| Disaster Recovery | $12,100.00 | $36,300.00 | Self-Managed only |
| Database Replica | Flat | $96,800.00 | Cloud only |

---

## 5. AppShield Pricing (19 Tiers)

AppShield pricing is based on total user volume (Internal + External).

| Tier | Min Users | Max Users | Price |
|------|-----------|-----------|-------|
| 1 | 0 | 10,000 | $18,150.00 |
| 2 | 10,001 | 50,000 | $32,670.00 |
| 3 | 50,001 | 100,000 | $54,450.00 |
| 4 | 100,001 | 500,000 | $96,800.00 |
| 5 | 500,001 | 1,000,000 | $234,740.00 |
| 6 | 1,000,001 | 2,000,000 | $275,880.00 |
| 7 | 2,000,001 | 3,000,000 | $358,160.00 |
| 8 | 3,000,001 | 4,000,000 | $411,400.00 |
| 9 | 4,000,001 | 5,000,000 | $508,200.00 |
| 10 | 5,000,001 | 6,000,000 | $605,000.00 |
| 11 | 6,000,001 | 7,000,000 | $701,800.00 |
| 12 | 7,000,001 | 8,000,000 | $798,600.00 |
| 13 | 8,000,001 | 9,000,000 | $895,400.00 |
| 14 | 9,000,001 | 10,000,000 | $992,200.00 |
| 15 | 10,000,001 | 11,000,000 | $1,089,000.00 |
| 16 | 11,000,001 | 12,000,000 | $1,185,800.00 |
| 17 | 12,000,001 | 13,000,000 | $1,282,600.00 |
| 18 | 13,000,001 | 14,000,000 | $1,379,400.00 |
| 19 | 14,000,001 | 15,000,000 | $1,476,200.00 |

---

## 6. Services (Regional Pricing)

### 6.1 Middle East Region

| Service | Unit Price |
|---------|------------|
| Essential Success Plan | $30,250.00 |
| Premier Success Plan | $60,500.00 |
| Dedicated Group Session | $3,820.00 |
| Public Session | $720.00 |
| Expert Days | $2,130.00 |

### 6.2 Africa Region

| Service | Unit Price |
|---------|------------|
| Essential Success Plan | $30,250.00 |
| Premier Success Plan | $60,500.00 |
| Dedicated Group Session | $2,670.00 |
| Public Session | $480.00 |
| Expert Days | $1,400.00 |

---

## 7. Verified Pricing Examples

### Example 1: ODC (Middle East)

**Configuration**:
- Platform: ODC
- AOs: 450 (3 packs)
- Internal Users: 1,000 (10 packs)
- External Users: 5,000 (5 packs)
- Region: Middle East

**Section 1-4: License**

| Component | Calculation | Price |
|-----------|-------------|-------|
| ODC Platform | Base | $30,250.00 |
| Application Objects | 2 additional packs × $18,150 | $36,300.00 |
| Internal Users | 9 additional packs × $6,050 | $54,450.00 |
| External Users | 5 packs × $6,050 | $30,250.00 |
| **License Subtotal** | | **$151,250.00** |

**Section 5: Add-Ons**

| Add-On | Calculation | Price |
|--------|-------------|-------|
| Support 24x7 Extended | 3 packs × $6,050 | $18,150.00 |
| AppShield | 6,000 users (Tier 1) | $18,150.00 |
| High Availability | 3 packs × $18,150 | $54,450.00 |
| Non-Production Runtime (×2) | 2 × 3 packs × $6,050 | $36,300.00 |
| Private Gateway | 3 packs × $1,210 | $3,630.00 |
| ODC Sentry | 3 packs × $6,050 | $18,150.00 |
| **Add-Ons Subtotal** | | **$148,830.00** |

**Section 6: Services**

| Service | Qty | Price |
|---------|-----|-------|
| Premier Success Plan | 1 | $60,500.00 |
| Dedicated Group Session | 1 | $3,820.00 |
| Public Session | 1 | $720.00 |
| Expert Days | 1 | $2,130.00 |
| **Services Subtotal** | | **$67,170.00** |

| **TOTAL** | **$367,250.00** |
|-----------|-----------------|

---

### Example 2: O11 Cloud (Middle East)

**Configuration**:
- Platform: O11 Enterprise Cloud
- AOs: 450 (3 packs)
- Users: Unlimited
- AppShield: 15,000,000 users (Tier 19)
- Region: Middle East

**Section 1-4: License**

| Component | Calculation | Price |
|-----------|-------------|-------|
| O11 Enterprise | Base | $36,300.00 |
| Application Objects | 2 additional packs × $36,300 | $72,600.00 |
| Unlimited Users | 3 packs × $60,500 | $181,500.00 |
| **License Subtotal** | | **$290,400.00** |

**Section 5: Add-Ons**

| Add-On | Calculation | Price |
|--------|-------------|-------|
| Support 24x7 Extended | Included | $0.00 |
| Support 24x7 Premium | 3 packs × $3,630 | $10,890.00 |
| AppShield | 15M users (Tier 19) | $1,476,200.00 |
| Sentry (includes HA) | 3 packs × $24,200 | $72,600.00 |
| Log Streaming | 1 × $7,260 | $7,260.00 |
| Non-Production Environment | 1 × 3 packs × $3,630 | $10,890.00 |
| Load Test Environment | 1 × 3 packs × $6,050 | $18,150.00 |
| Environment Pack | 1 × 3 packs × $9,680 | $29,040.00 |
| Database Replica | 1 × $96,800 | $96,800.00 |
| **Add-Ons Subtotal** | | **$1,721,830.00** |

**Section 6: Services**

| Service | Qty | Price |
|---------|-----|-------|
| Premier Success Plan | 1 | $60,500.00 |
| Dedicated Group Session | 1 | $3,820.00 |
| Public Session | 1 | $720.00 |
| Expert Days | 1 | $2,130.00 |
| **Services Subtotal** | | **$67,170.00** |

| **TOTAL** | **$2,079,400.00** |
|-----------|-------------------|

---

### Example 3: O11 Self-Managed (Middle East)

**Configuration**:
- Platform: O11 Enterprise Self-Managed
- AOs: 450 (3 packs)
- Internal Users: 2,000 (tiered pricing)
- External Users: 1,000,000 (tiered pricing)
- AppShield: 1,002,000 users (Tier 6)
- Region: Middle East

**Section 1-4: License**

| Component | Calculation | Price |
|-----------|-------------|-------|
| O11 Enterprise | Base | $36,300.00 |
| Application Objects | 2 additional packs × $36,300 | $72,600.00 |
| Internal Users (2,000) | Tiered (see Section 3.3) | $133,100.00 |
| External Users (1M) | Tiered (see Section 3.3) | $419,567.50 |
| **License Subtotal** | | **$661,567.50** |

**Section 5: Add-Ons**

| Add-On | Calculation | Price |
|--------|-------------|-------|
| Support 24x7 Extended | Included | $0.00 |
| Support 24x7 Premium | 3 packs × $3,630 | $10,890.00 |
| AppShield | 1,002,000 users (Tier 6) | $275,880.00 |
| Non-Production Environment | 1 × 3 packs × $3,630 | $10,890.00 |
| Environment Pack | 1 × 3 packs × $9,680 | $29,040.00 |
| Disaster Recovery | 3 packs × $12,100 | $36,300.00 |
| **Add-Ons Subtotal** | | **$363,000.00** |

**Section 6: Services**

| Service | Qty | Price |
|---------|-----|-------|
| Premier Success Plan | 1 | $60,500.00 |
| Dedicated Group Session | 1 | $3,820.00 |
| Public Session | 1 | $720.00 |
| Expert Days | 1 | $2,130.00 |
| **Services Subtotal** | | **$67,170.00** |

| **TOTAL** | **$1,091,737.50** |
|-----------|-------------------|

---

## 8. Implementation Notes

### Key Pricing Rules

1. **Base includes 1 AO pack (150 AOs) and 1 Internal User pack (100 users)**
2. **AO pack count affects add-on pricing** - most add-ons are priced per pack of AOs
3. **O11 user pricing is tiered** - calculate each tier separately
4. **ODC user pricing is flat** - simple pack × rate calculation
5. **AppShield is based on total users** - Internal + External combined
6. **Services pricing varies by region** - Middle East rates differ from Africa
7. **Some add-ons are deployment-specific** - Cloud only vs Self-Managed only

### Formula Summary

```
Total = License + Add-Ons + Services

License = Platform_Base
        + (AO_Packs - 1) × AO_Pack_Rate
        + User_Cost (flat or tiered)

Add-Ons = Σ (Add-On_Rate × AO_Packs) + AppShield_Tier

Services = Σ (Service × Quantity)
```

---

## Appendix: Source Screenshots

All pricing data verified from OutSystems Partner Pricing Calculator:

| Screenshot | Content |
|------------|---------|
| ODC.png | ODC bundle options |
| ODC - AOs.png | AO pack pricing with tooltip |
| ODC AOs and Users.png | Full ODC pricing breakdown |
| ODC - Internal Users.png | Internal user pack tooltip |
| ODC - External Users.png | External user pack tooltip |
| ODC - Unlimited Users.png | Unlimited users per-pack rate |
| ODC - Add-Ons.png | ODC add-ons with per-pack rates |
| ODC - Services.png | ODC services pricing (Africa) |
| O11 Cloud.png | O11 Cloud bundle options |
| O11 Self-Managed.png | O11 Self-Managed bundle options |
| O11 Cloud - AOs and Users.png | O11 Cloud pricing breakdown |
| O11 Self-Managed - AOs and Users.png | O11 Self-Managed pricing breakdown |
| Internal User Pricing - Enterprise Edition.png | O11 tiered internal pricing |
| External User Pricing - Enterprise Edition.png | O11 tiered external pricing |
| O11 Add-Ons.png | O11 add-ons with per-pack rates |
| O11 Services.png | O11 services pricing (Africa) |
| AppShield Pricing.png | 19-tier AppShield pricing table |
| ODC - Exmp 1.1.png | Example 1 license section |
| ODC - Exmp 1.2.png | Example 1 add-ons section |
| ODC - Exmp 1.3.png | Example 1 services section |
| O11 Cloud - Exmp 2.1.png | Example 2 license section |
| O11 Cloud - Exmp 2.2.png | Example 2 add-ons section |
| O11 Cloud - Exmp 2.3.png | Example 2 services section |
| O11 Self-Managed - Exmp 3.1.png | Example 3 license section |
| O11 Self-Managed - Exmp 3.2.png | Example 3 add-ons section |
| O11 Self-Managed - Exmp 3.3.png | Example 3 services section |
| Application Objects.png | AO pack info |

---

*Last updated: 2026-01-11*
*Source: OutSystems Partner Pricing Calculator (27 screenshots)*
