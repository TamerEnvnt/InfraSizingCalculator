# OutSystems Pricing Gap Analysis

**Date**: January 2026
**Source**: OutSystems Partner Price Calculator (Official Portal)
**Purpose**: Compare current implementation against actual OutSystems pricing structure

---

## Executive Summary

The current implementation has **significant gaps** compared to the actual OutSystems Partner Calculator. Key issues include:

1. **Missing Bundle Tiers**: ODC and O11 both offer 3 bundle tiers (Business Basics, Business Critical, Strategic CoE) - not implemented
2. **Per-AO-Pack Pricing Model**: Many prices scale with number of AO packs, not flat fees
3. **Unlimited Users**: Priced at **$60,500 per AO pack** (not flat fee)
4. **Tiered User Pricing for O11**: Volume-based discounts for internal/external users
5. **Platform-Specific User Pricing**: ODC uses flat pack pricing, O11 uses tiered pricing

---

## Verified Pricing Data (From Partner Calculator)

### 1. Platform Bundle Tiers

#### ODC (OutSystems Developer Cloud) Bundles

| Tier | Price | AOs | Internal Users | Environments | Support | Extras |
|------|-------|-----|----------------|--------------|---------|--------|
| **Business Basics** | $88,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 8x5 | Private Gateway |
| **Business Critical** | $179,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 24x7 | Private Gateway, HA, Sentry |
| **Strategic CoE** | $179,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 24x7 | Private Gateway, HA, Sentry |

#### O11 Cloud Bundles

| Tier | Price | AOs | Internal Users | Environments | Support | Extras |
|------|-------|-----|----------------|--------------|---------|--------|
| **Business Basics** | $109,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 24x7 | - |
| **Business Critical** | $182,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 24x7 | Sentry (inc. HA) |
| **Strategic CoE** | $211,000 | 450 | 100 | 1 Dev, 2 NonProd, 2 Prod | 24x7 | Sentry (inc. HA) |

#### O11 Self-Managed Bundles

| Tier | Price | AOs | Internal Users | Environments | Support | Extras |
|------|-------|-----|----------------|--------------|---------|--------|
| **Business Basics** | $109,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 24x7 | - |
| **Business Critical** | $145,000 | 450 | 100 | 1 Dev, 1 NonProd, 1 Prod | 24x7 | Disaster Recovery |
| **Strategic CoE** | $174,000 | 450 | 100 | 1 Dev, 2 NonProd, 2 Prod | 24x7 | Disaster Recovery |

**Current Implementation**: No bundle concept - only a la carte pricing
**Gap**: Major - bundles are the primary entry point in the actual calculator

---

### 2. ODC A La Carte Pricing

| Item | Per-Pack Price | Pack Size | Included | Current Implementation | Gap |
|------|---------------|-----------|----------|----------------------|-----|
| ODC Platform Base | $30,250 | - | - | $36,300 | **Wrong** |
| AO Pack | **$18,150** | 150 AOs | 1 pack (150) | $36,300 | **Wrong** |
| Internal User Pack | **$6,050** | 100 users | 1 pack (100) | $6,000 | Minor |
| External User Pack | **$6,050** | 1000 users | 0 | $4,840 | **Wrong** |
| Unlimited Users | **$60,500 per AO pack** | - | - | $181,500 flat | **Wrong model** |

**Key Insight**: ODC uses simple flat pack pricing (no volume tiers for users)

---

### 3. O11 A La Carte Pricing

| Item | Per-Pack Price | Pack Size | Included | Current Implementation | Gap |
|------|---------------|-----------|----------|----------------------|-----|
| Enterprise Edition | $36,300 | - | - | $36,300 | Correct |
| AO Pack | **$36,300** | 150 AOs | 1 pack (150) | $36,300 | Correct |
| Internal Users | **Tiered** | 100 users | 1 pack (100) | Flat rate | **Wrong model** |
| External Users | **Tiered** | 1000 users | 0 | Flat rate | **Wrong model** |
| Unlimited Users | **$60,500 per AO pack** | - | - | $181,500 flat | **Wrong model** |

---

### 4. O11 Internal User Tiered Pricing (Enterprise Edition)

| User Range | Price per 100 | Per User | Current Implementation |
|------------|--------------|----------|----------------------|
| 200 - 1,000 | $12,100 | $121.00 | Flat rate - **Wrong** |
| 1,100 - 10,000 | $2,420 | $24.20 | Not implemented |
| 10,100 - 100,000,000 | $242 | $2.42 | Not implemented |

**Note**: First 100 users included in Enterprise Edition

---

### 5. O11 External User Tiered Pricing (Enterprise Edition)

| User Range | Price per 1000 | Per User | Current Implementation |
|------------|---------------|----------|----------------------|
| 1,000 - 10,000 | $4,840 | $4.84 | Partial |
| 11,000 - 250,000 | $1,452 | $1.45 | Not implemented |
| 251,000 - 100,000,000 | $30.25 | $0.03 | Not implemented |

---

### 6. AppShield Tiered Pricing (Both ODC and O11)

AppShield uses **flat tier pricing** based on total user count (not per-user):

| Tier | Min Users | Max Users | Price |
|------|-----------|-----------|-------|
| 1 | 0 | 10,000 | $18,150 |
| 2 | 10,001 | 50,000 | $32,670 |
| 3 | 50,001 | 100,000 | $54,450 |
| 4 | 100,001 | 500,000 | $96,800 |
| 5 | 500,001 | 1,000,000 | $234,740 |
| 6 | 1,000,001 | 2,000,000 | $275,880 |
| 7 | 2,000,001 | 3,000,000 | $358,160 |
| 8 | 3,000,001 | 4,000,000 | $411,400 |
| 9 | 4,000,001 | 5,000,000 | $508,200 |
| 10 | 5,000,001 | 6,000,000 | $605,000 |
| 11 | 6,000,001 | 7,000,000 | $701,800 |
| 12 | 7,000,001 | 8,000,000 | $798,600 |
| 13 | 8,000,001 | 9,000,000 | $895,400 |
| 14 | 9,000,001 | 10,000,000 | $992,200 |
| 15 | 10,000,001 | 11,000,000 | $1,089,000 |
| 16 | 11,000,001 | 12,000,000 | $1,185,800 |
| 17 | 12,000,001 | 13,000,000 | $1,282,600 |
| 18 | 13,000,001 | 14,000,000 | $1,379,400 |
| 19 | 14,000,001 | 15,000,000 | $1,476,200 |

**Current Implementation**: ~$16.50 per user calculation - **Wrong model**

---

### 7. ODC Add-Ons

All ODC add-ons are priced **per AO pack**:

| Add-On | Per AO Pack | Display Total (2 packs) | Current | Gap |
|--------|-------------|------------------------|---------|-----|
| Support 24x7 Extended | $6,050 | $12,100 | Not implemented | **Missing** |
| Support 24x7 Premium | $9,680 | $19,360 | $3,630/pack | **Wrong** |
| AppShield | Tiered | $18,150+ | Per-user | **Wrong model** |
| High Availability | $18,150 | $36,300 | $12,100/pack | **Wrong** |
| Non-Production Runtime | $6,050 | $12,100 | $3,630/pack | **Wrong** |
| Private Gateway | $1,210 | $2,420 | Not implemented | **Missing** |
| ODC Sentry | $6,050 | $12,100 | $24,200/pack | **Wrong** |

---

### 8. O11 Add-Ons

| Add-On | Per AO Pack | Cloud Only | Current | Gap |
|--------|-------------|------------|---------|-----|
| Support 24x7 Extended | **Included** | No | Not tracked | Note |
| Support 24x7 Premium | $3,630 | No | $3,630 | Correct |
| AppShield | Tiered | No | Per-user | **Wrong model** |
| High Availability | $12,100 | **Yes** | $12,100 | Correct |
| Sentry (inc. HA) | $24,200 | **Yes** | $24,200 | Correct |
| Log Streaming | $7,260 flat | **Yes** | $7,260 | Correct |
| Non-Production Env | $7,260 flat | No | Per-pack | **Wrong** |
| Load Test Env | $6,050 | **Yes** | $6,050 | Correct |
| Environment Pack | $9,680 | No | $9,680 | Correct |
| Disaster Recovery | $12,100 | No | $12,100 | Correct |
| Database Replica | $96,800 flat | **Yes** | $96,800 | Correct |

---

### 9. Services (Same for ODC and O11)

| Service | Actual Price | Current | Gap |
|---------|-------------|---------|-----|
| Essential Success Plan | $30,250 | $30,250 | Correct |
| Premier Success Plan | $60,500 | $60,500 | Correct |
| Dedicated Group Session | **$2,670** | $3,820 | **Wrong** |
| Public Session | **$480** | $720 | **Wrong** |
| Expert Day | **$1,400** | $2,640 | **Wrong** |

---

## Summary of Required Changes

### Critical Fixes (Breaking Changes)

| Priority | Change | Impact |
|----------|--------|--------|
| 1 | **Unlimited Users = $60,500 × AO packs** (not flat fee) | Major pricing difference |
| 2 | **Add Bundle Tier Selection** | Primary UI flow change |
| 3 | **ODC Base = $30,250** (not $36,300) | $6,050 difference |
| 4 | **ODC AO Pack = $18,150** (not $36,300) | $18,150 difference per pack |
| 5 | **ODC External Users = $6,050/1000** (not $4,840) | $1,210 difference per pack |
| 6 | **O11 Internal Users = Tiered** | Volume discount model |
| 7 | **O11 External Users = Tiered** | Volume discount model |
| 8 | **AppShield = Tiered flat pricing** | Completely different model |

### Important Fixes

| Priority | Change | Impact |
|----------|--------|--------|
| 9 | ODC Support 24x7 Premium = $9,680/pack (not $3,630) | ~3x difference |
| 10 | ODC HA = $18,150/pack (not $12,100) | ~1.5x difference |
| 11 | ODC Non-Prod Runtime = $6,050/pack (not $3,630) | ~1.7x difference |
| 12 | ODC Sentry = $6,050/pack (not $24,200) | ~0.25x difference |
| 13 | O11 Non-Prod Env = $7,260 flat (not per-pack) | Model change |
| 14 | Add ODC Support 24x7 Extended ($6,050/pack) | Missing add-on |
| 15 | Add ODC Private Gateway ($1,210/pack) | Missing add-on |

### Service Price Fixes

| Service | Current | Correct | Difference |
|---------|---------|---------|------------|
| Dedicated Group Session | $3,820 | $2,670 | -$1,150 |
| Public Session | $720 | $480 | -$240 |
| Expert Day | $2,640 | $1,400 | -$1,240 |

---

## Pricing Model Comparison

### Current Implementation (Simplified)

```
Total = Edition + (AO Packs × Pack Price) + (Users × Per-User Rate) + Add-ons
```

### Actual Partner Calculator Model

```
If Bundle Selected:
  Total = Bundle Price + Additional AOs + Additional Users + Extra Add-ons

If A La Carte:
  ODC: Total = Platform + (AO Packs × $18,150) + (User Packs × $6,050) + Add-ons
  O11: Total = Edition + (AO Packs × $36,300) + Tiered Users + Add-ons

Unlimited Users = $60,500 × Number of AO Packs

Add-ons = Sum(Add-on per-pack price × Number of AO Packs)
```

---

## Files to Modify

| File | Changes Required |
|------|-----------------|
| `Models/Pricing/OutSystemsPricing.cs` | Add bundle enums, fix all pricing values, add tiered pricing classes |
| `Data/Entities/OutSystemsPricingEntity.cs` | Add bundle fields, tiered pricing storage |
| `Services/Pricing/DatabasePricingSettingsService.cs` | Implement tiered calculations, bundle logic, per-AO-pack pricing |
| `Components/Pricing/OutSystemsPricingPanel.razor` | Add bundle selection UI |

---

## Pricing Examples (EXACT from Partner Calculator Screenshots)

> **Note**: These examples are taken directly from OutSystems Partner Calculator screenshots.
> All figures are verified from the actual calculator interface.

### Example 1: ODC Deployment (from ODC-Exmp-1.x screenshots)

**Configuration:**
- Platform: OutSystems Developer Cloud (ODC)
- AOs: 450 (3 packs)
- Internal Users: 1,000 (10 packs)
- External Users: 5,000 (5 packs)
- Region: Middle East

**Platform & Users (from screenshot):**
| Item | Screenshot Value |
|------|-----------------|
| ODC Platform Base | **$30,250.00** |
| Application Objects (3 packs) | **$36,300.00** |
| Internal Users (10 packs) | **$54,450.00** |
| External Users (5 packs) | **$30,250.00** |

**Add-Ons Total: $148,830.00 (from screenshot)**
| Add-On | Enabled | Screenshot Value |
|--------|---------|-----------------|
| Support 24x7 Extended | Yes | $18,150.00 |
| Support 24x7 Premium | No | - |
| AppShield (6,000 users) | Yes | $18,150.00 |
| High Availability | Yes | $54,450.00 |
| Non-Production Runtime (×2) | Yes | $36,300.00 |
| Private Gateway | Yes | $3,630.00 |
| ODC Sentry | Yes | $18,150.00 |

**Services Total: $67,170.00 (Middle East)**
| Service | Qty | Screenshot Value |
|---------|-----|-----------------|
| Premier Success Plan | 1 | $60,500.00 |
| Dedicated Group Session | 1 | $3,820.00 |
| Public Session | 1 | $720.00 |
| Expert Days | 1 | $2,130.00 |

**Screenshot Totals:**
- Platform + Users: $151,250.00
- Add-Ons: $148,830.00
- Services: $67,170.00
- **Grand Total: $367,250.00**

---

### Example 2: O11 Cloud Large Enterprise (from O11-Cloud-Exmp-2.x screenshots)

**Configuration:**
- Platform: OutSystems 11 Cloud
- AOs: 450 (3 packs)
- Unlimited Users: Yes
- AppShield: 15,000,000 users
- Region: Middle East

**Platform & Users (from screenshot):**
| Item | Screenshot Value |
|------|-----------------|
| Enterprise Edition (Cloud) | **$36,300.00** |
| Application Objects (3 packs) | **$72,600.00** |
| Unlimited Users (enabled) | **$181,500.00** |

**Add-Ons Total: $1,721,830.00 (from screenshot)**
| Add-On | Status | Screenshot Value |
|--------|--------|-----------------|
| Support 24x7 Extended | Included | $0 |
| Support 24x7 Premium | Enabled | $10,890.00 |
| AppShield (15M users = Tier 19) | Enabled | $1,476,200.00 |
| High Availability | Included | $0 |
| Sentry (inc. HA) | Enabled | $72,600.00 |
| Log Streaming | 1 | $7,260.00 |
| Non-Production Environment | 1 | $10,890.00 |
| Load Test Environment | 1 | $18,150.00 |
| Environment Pack | 1 | $29,040.00 |
| Disaster Recovery | Self-Managed only | N/A |
| Database Replica | 1 | $96,800.00 |

**Services Total: $67,170.00 (Middle East)**

**Screenshot Totals:**
- Platform + Users: $290,400.00
- Add-Ons: $1,721,830.00
- Services: $67,170.00
- **Grand Total: $2,079,400.00**

---

### Example 3: O11 Self-Managed Enterprise (from O11-Self-Managed-Exmp-3.x screenshots)

**Configuration:**
- Platform: OutSystems 11 Self-Managed
- AOs: 450 (3 packs)
- Internal Users: 2,000 (tiered pricing)
- External Users: 1,000,000 (tiered pricing)
- AppShield: ~1M users
- Region: Middle East

**Platform & Users (from screenshot):**
| Item | Screenshot Value |
|------|-----------------|
| Enterprise Edition (Self-Managed) | **$36,300.00** |
| Application Objects (3 packs) | **$72,600.00** |
| Internal Users (2,000 - 20 packs, tiered) | **$133,100.00** |
| External Users (1,000,000 - 1000 packs, tiered) | **$419,567.50** |

**Add-Ons Total: $363,000.00 (from screenshot)**
| Add-On | Status | Screenshot Value |
|--------|--------|-----------------|
| Support 24x7 Extended | Included | $0 |
| Support 24x7 Premium | Enabled | $10,890.00 |
| AppShield (1,002,000 users = Tier 6) | Enabled | $275,880.00 |
| High Availability | Cloud only | N/A |
| Sentry (inc. HA) | Cloud only | N/A |
| Log Streaming | Cloud only | N/A |
| Non-Production Environment | 1 | $10,890.00 |
| Load Test Environment | Cloud only | N/A |
| Environment Pack | 1 | $29,040.00 |
| Disaster Recovery | Enabled | $36,300.00 |
| Database Replica | Cloud only | N/A |

**Services Total: $67,170.00 (Middle East)**

**Screenshot Totals:**
- Platform + Users: $661,567.50
- Add-Ons: $363,000.00
- Services: $67,170.00
- **Grand Total: $1,091,737.50**

---

## Appendix: Source Images Analyzed

| Image | Key Data Extracted |
|-------|-------------------|
| `ODC.png` | ODC bundle tiers and inclusions |
| `ODC - Unlimited Users.png` | **$60,500 per pack of AOs** (tooltip) |
| `ODC AOs and Users.png` | ODC base $30,250, AO pack $18,150 |
| `ODC - AOs.png` | AO pack tooltip: $18,150 per pack |
| `ODC - Intenral Users.png` | Internal users: $6,050 per pack of 100 |
| `ODC - External Users.png` | External users: $6,050 per pack of 1000 |
| `ODC - Add-Ons.png` | All ODC add-on per-pack prices |
| `ODC - Services.png` | Services pricing |
| `O11 Cloud.png` | O11 Cloud bundle tiers |
| `O11 Self-Managed.png` | O11 Self-Managed bundle tiers |
| `O11 Cloud - AOs and USers.png` | O11 pricing, **$60,500 per AO pack** for unlimited |
| `O11 Self-Managed - AOs and Users.png` | Same unlimited users tooltip |
| `O11 Add-Ons.png` | O11 add-on prices, Cloud-only badges |
| `O11 Services.png` | Services pricing (same as ODC) |
| `Application Objects.png` | O11 AO pack: $36,300 per pack |
| `Internal User Pricing - Enterprise Editon.png` | 3-tier internal user pricing |
| `External User Pricing - Enterprise Editon.png` | 3-tier external user pricing |
| `AppShield Pricing.png` | 19-tier AppShield pricing table |
