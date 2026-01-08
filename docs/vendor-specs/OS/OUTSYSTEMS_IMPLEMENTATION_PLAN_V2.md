# OutSystems Pricing Implementation Plan V2

**Created**: January 2026
**Status**: Approved for Implementation
**Verified Against**: OutSystems Partner Calculator (Examples 1, 2, 3)

---

## Executive Summary

Implement complete OutSystems pricing calculator supporting:
- **3 Platforms**: ODC, O11 Cloud, O11 Self-Managed
- **Region-based pricing**: Services vary by region
- **Manual discounts**: Percentage or fixed value
- **All pricing factors**: Per-pack, flat, tiered, cloud-only restrictions

---

## Part 1: Verified Pricing Data

### 1.1 Platform Base Pricing

| Platform | Base Price | Includes |
|----------|------------|----------|
| **ODC** | $30,250 | 150 AOs, 100 Internal Users, 2 runtimes (Dev, Prod), 8x5 Support |
| **O11 Enterprise** | $36,300 | 150 AOs, 100 Internal Users, 3 environments (Dev, Non-Prod, Prod), 24x7 Support |

### 1.2 Application Objects Pricing

| Platform | Per Pack (150 AOs) | Included |
|----------|-------------------|----------|
| **ODC** | $18,150 | 1 pack (150 AOs) |
| **O11** | $36,300 | 1 pack (150 AOs) |

**Formula**: `AO_Cost = (AO_Packs - 1) × Per_Pack_Rate`

### 1.3 User Pricing

#### ODC Users (Flat Pack Pricing)
| Type | Pack Size | Price/Pack | Included |
|------|-----------|------------|----------|
| Internal | 100 users | $6,050 | 1 pack (100) |
| External | 1000 users | $6,050 | 0 packs |
| Unlimited | N/A | $60,500 × AO_Packs | N/A |

**ODC User Formula**:
```
Internal_Cost = Max(0, Internal_Packs - 1) × $6,050
External_Cost = External_Packs × $6,050
Unlimited_Cost = $60,500 × AO_Packs
```

#### O11 Users (Tiered Pricing)

**Internal Users (per 100):**
| Tier | User Range | Price/100 | Per User |
|------|------------|-----------|----------|
| Included | 1-100 | $0 | $0 |
| Tier 1 | 200-1,000 | $12,100 | $121.00 |
| Tier 2 | 1,100-10,000 | $2,420 | $24.20 |
| Tier 3 | 10,100-100,000,000 | $242 | $2.42 |

**External Users (per 1000):**
| Tier | User Range | Price/1000 | Per User |
|------|------------|------------|----------|
| Tier 1 | 1-10,000 | $4,840 | $4.84 |
| Tier 2 | 11,000-250,000 | $1,452 | $1.45 |
| Tier 3 | 251,000-100,000,000 | $30.25 | $0.03 |

**O11 Unlimited Users**: $60,500 × AO_Packs

**O11 Tiered User Calculation Algorithm**:
```csharp
// Internal Users (100 included)
decimal CalculateO11InternalUsers(int users) {
    if (users <= 100) return 0;

    decimal cost = 0;
    int remaining = users - 100; // First 100 included

    // Tier 1: 200-1000 (covers users 101-1000 = 900 max)
    int tier1Users = Math.Min(remaining, 900);
    cost += Math.Ceiling(tier1Users / 100.0m) * 12100;
    remaining -= tier1Users;

    // Tier 2: 1100-10000 (covers users 1001-10000 = 9000 max)
    int tier2Users = Math.Min(remaining, 9000);
    cost += Math.Ceiling(tier2Users / 100.0m) * 2420;
    remaining -= tier2Users;

    // Tier 3: 10100+ (remaining users)
    if (remaining > 0) {
        cost += Math.Ceiling(remaining / 100.0m) * 242;
    }

    return cost;
}

// External Users (0 included)
decimal CalculateO11ExternalUsers(int users) {
    if (users <= 0) return 0;

    decimal cost = 0;
    int remaining = users;

    // Tier 1: 1-10000
    int tier1Users = Math.Min(remaining, 10000);
    cost += Math.Ceiling(tier1Users / 1000.0m) * 4840;
    remaining -= tier1Users;

    // Tier 2: 11000-250000 (covers 240000 max)
    int tier2Users = Math.Min(remaining, 240000);
    cost += Math.Ceiling(tier2Users / 1000.0m) * 1452;
    remaining -= tier2Users;

    // Tier 3: 251000+ (remaining users)
    if (remaining > 0) {
        cost += Math.Ceiling(remaining / 1000.0m) * 30.25m;
    }

    return cost;
}
```

### 1.4 Add-Ons Pricing

#### ODC Add-Ons (All per AO pack)
| Add-On | Per Pack Rate | Formula |
|--------|---------------|---------|
| Support 24x7 Extended | $6,050 | Rate × AO_Packs |
| Support 24x7 Premium | $9,680 | Rate × AO_Packs |
| AppShield | Tiered | See AppShield table |
| High Availability | $18,150 | Rate × AO_Packs |
| Non-Production Runtime | $6,050 | Rate × AO_Packs × Quantity |
| Private Gateway | $1,210 | Rate × AO_Packs |
| ODC Sentry | $6,050 | Rate × AO_Packs |

#### O11 Add-Ons (Cloud vs Self-Managed availability)
| Add-On | Per Pack Rate | Cloud | Self-Managed | Pricing Type |
|--------|---------------|-------|--------------|--------------|
| Support 24x7 Extended | - | **Included** | **Included** | - |
| Support 24x7 Premium | $3,630 | Yes | Yes | Per AO Pack |
| AppShield | Tiered | Yes | Yes | Tiered by users |
| High Availability | $12,100 | Yes | **No** | Per AO Pack |
| Sentry (inc. HA) | $24,200 | Yes | **No** | Per AO Pack |
| Log Streaming | $7,260 | Yes | **No** | **Flat** |
| Non-Production Env | $3,630 | Yes | Yes | Per AO Pack × Qty |
| Load Test Environment | $6,050 | Yes | **No** | Per AO Pack |
| Environment Pack | $9,680 | Yes | Yes | Per AO Pack × Qty |
| Disaster Recovery | $12,100 | **No** | Yes | Per AO Pack |
| Database Replica | $96,800 | Yes | **No** | **Flat** |

**Special Rules**:
- When Sentry is enabled, HA becomes "Included" (no additional cost)
- O11 Cloud: DR not available ("Self Managed only")
- O11 Self-Managed: HA, Sentry, Log Streaming, Load Test, DB Replica not available ("Cloud only")

### 1.5 AppShield Tiered Pricing (All platforms)

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

**AppShield User Count**: For unlimited users, user must enter expected user volume manually.

### 1.6 Services Pricing (Region-Dependent)

| Service | Africa | Middle East | (Other regions TBD) |
|---------|--------|-------------|---------------------|
| Essential Success Plan | $30,250 | $30,250 | $30,250 |
| Premier Success Plan | $60,500 | $60,500 | $60,500 |
| Dedicated Group Session | $2,670 | $3,820 | TBD |
| Public Session | $480 | $720 | TBD |
| Expert Day | $1,400 | $2,130 | TBD |

**Regions to Support**: Africa, Middle East, Americas, Europe, Asia Pacific (verify prices from calculator)

---

## Part 2: Discount Feature

### 2.1 Discount Types

| Type | Description | Application |
|------|-------------|-------------|
| **Percentage** | % off total | `Final = Total × (1 - Discount%)` |
| **Fixed Amount** | Fixed $ off | `Final = Total - DiscountAmount` |

### 2.2 Discount Scope Options

| Scope | Applies To |
|-------|------------|
| **Total** | Entire quote (License + Add-Ons + Services) |
| **License Only** | Edition + AOs + Users |
| **Add-Ons Only** | All add-ons |
| **Services Only** | Success plans + Training + Expert days |

### 2.3 Discount Data Model

```csharp
public class OutSystemsDiscount
{
    public DiscountType Type { get; set; } // Percentage, FixedAmount
    public DiscountScope Scope { get; set; } // Total, LicenseOnly, AddOnsOnly, ServicesOnly
    public decimal Value { get; set; } // Percentage (0-100) or Dollar amount
    public string? Notes { get; set; } // Optional description (e.g., "Partner discount")
}

public enum DiscountType { Percentage, FixedAmount }
public enum DiscountScope { Total, LicenseOnly, AddOnsOnly, ServicesOnly }
```

---

## Part 3: Data Model Design

### 3.1 Enums

```csharp
// Platform selection
public enum OutSystemsPlatform { ODC, O11 }

// O11 Deployment type
public enum OutSystemsDeployment { Cloud, SelfManaged }

// Service region
public enum OutSystemsRegion { Africa, MiddleEast, Americas, Europe, AsiaPacific }

// Discount enums
public enum DiscountType { Percentage, FixedAmount }
public enum DiscountScope { Total, LicenseOnly, AddOnsOnly, ServicesOnly }
```

### 3.2 Configuration Model

```csharp
public class OutSystemsDeploymentConfig
{
    // Platform Selection
    public OutSystemsPlatform Platform { get; set; }
    public OutSystemsDeployment? Deployment { get; set; } // Only for O11
    public OutSystemsRegion Region { get; set; }

    // Application Scale
    public int TotalApplicationObjects { get; set; }
    public int AOPacks => (int)Math.Ceiling(TotalApplicationObjects / 150.0);

    // User Capacity
    public bool UseUnlimitedUsers { get; set; }
    public int InternalUsers { get; set; }
    public int ExternalUsers { get; set; }
    public int? AppShieldUserVolume { get; set; } // Required if Unlimited + AppShield

    // ODC Add-Ons
    public bool OdcSupport24x7Extended { get; set; }
    public bool OdcSupport24x7Premium { get; set; }
    public bool OdcAppShield { get; set; }
    public bool OdcHighAvailability { get; set; }
    public int OdcNonProdRuntimeQuantity { get; set; }
    public bool OdcPrivateGateway { get; set; }
    public bool OdcSentry { get; set; }

    // O11 Add-Ons
    public bool O11Support24x7Premium { get; set; }
    public bool O11AppShield { get; set; }
    public bool O11HighAvailability { get; set; } // Cloud only
    public bool O11Sentry { get; set; } // Cloud only
    public int O11LogStreamingQuantity { get; set; } // Cloud only
    public int O11NonProdEnvQuantity { get; set; }
    public int O11LoadTestEnvQuantity { get; set; } // Cloud only
    public int O11EnvPackQuantity { get; set; }
    public bool O11DisasterRecovery { get; set; } // Self-Managed only
    public int O11DatabaseReplicaQuantity { get; set; } // Cloud only

    // Services
    public int EssentialSuccessPlanQuantity { get; set; }
    public int PremierSuccessPlanQuantity { get; set; }
    public int DedicatedGroupSessionQuantity { get; set; }
    public int PublicSessionQuantity { get; set; }
    public int ExpertDayQuantity { get; set; }

    // Discount
    public OutSystemsDiscount? Discount { get; set; }
}
```

### 3.3 Result Model

```csharp
public class OutSystemsPricingResult
{
    // License Breakdown
    public decimal EditionCost { get; set; }
    public decimal AOPacksCost { get; set; }
    public decimal InternalUsersCost { get; set; }
    public decimal ExternalUsersCost { get; set; }
    public decimal UnlimitedUsersCost { get; set; }
    public decimal LicenseSubtotal { get; set; }

    // Add-Ons Breakdown
    public Dictionary<string, decimal> AddOnCosts { get; set; }
    public decimal AddOnsSubtotal { get; set; }

    // Services Breakdown
    public Dictionary<string, decimal> ServiceCosts { get; set; }
    public decimal ServicesSubtotal { get; set; }

    // Totals
    public decimal GrossTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountDescription { get; set; }
    public decimal NetTotal { get; set; }

    // Metadata
    public List<string> Warnings { get; set; } // e.g., "Cloud-only feature selected for Self-Managed"
    public List<string> Recommendations { get; set; } // e.g., "Unlimited users would save $X"

    // Calculation Details (for transparency)
    public int AOPackCount { get; set; }
    public int InternalUserPackCount { get; set; }
    public int ExternalUserPackCount { get; set; }
}
```

---

## Part 4: Implementation Tasks

### Phase 1: Core Model Updates
**Files**: `src/InfraSizingCalculator/Models/Pricing/OutSystemsPricing.cs`

- [ ] **Task 1.1**: Add new enums
  - `OutSystemsPlatform` (ODC, O11)
  - `OutSystemsDeployment` (Cloud, SelfManaged)
  - `OutSystemsRegion` (Africa, MiddleEast, Americas, Europe, AsiaPacific)
  - `DiscountType` (Percentage, FixedAmount)
  - `DiscountScope` (Total, LicenseOnly, AddOnsOnly, ServicesOnly)

- [ ] **Task 1.2**: Create `OutSystemsDiscount` class
  - Type, Scope, Value, Notes properties

- [ ] **Task 1.3**: Create `OutSystemsDeploymentConfig` class
  - Platform, Deployment, Region
  - AO configuration
  - User capacity (internal, external, unlimited, AppShield volume)
  - ODC add-ons
  - O11 add-ons
  - Services
  - Discount

- [ ] **Task 1.4**: Create `OutSystemsPricingResult` class
  - License breakdown
  - Add-ons breakdown
  - Services breakdown
  - Discount details
  - Warnings and recommendations

- [ ] **Task 1.5**: Create `OutSystemsPricingSettings` class
  - All base prices
  - ODC prices (base, AO pack, user packs)
  - O11 prices (enterprise, AO pack)
  - O11 tiered user pricing arrays
  - Add-on prices
  - AppShield tier pricing array
  - Services pricing by region
  - Cloud-only feature flags

### Phase 2: Database Entity Updates
**Files**: `src/InfraSizingCalculator/Data/Entities/OutSystemsPricingEntity.cs`

- [ ] **Task 2.1**: Add platform-specific pricing fields
  - ODC base, AO pack, user pack prices
  - O11 enterprise, AO pack prices

- [ ] **Task 2.2**: Add tiered pricing JSON fields
  - O11InternalUserTiersJson
  - O11ExternalUserTiersJson
  - AppShieldTiersJson

- [ ] **Task 2.3**: Add add-on pricing fields
  - ODC add-on rates
  - O11 add-on rates
  - Cloud-only flags

- [ ] **Task 2.4**: Add services pricing by region
  - ServicesRegionPricingJson (Dictionary<Region, ServicePrices>)

- [ ] **Task 2.5**: Add migration for new fields

### Phase 3: Service Layer Implementation
**Files**: `src/InfraSizingCalculator/Services/Pricing/DatabasePricingSettingsService.cs`

- [ ] **Task 3.1**: Implement `CalculateODCLicenseCost(config)`
  - Base edition: $30,250
  - AO packs: (packs - 1) × $18,150
  - Internal users: (packs - 1) × $6,050
  - External users: packs × $6,050
  - Unlimited: $60,500 × AO_packs

- [ ] **Task 3.2**: Implement `CalculateO11LicenseCost(config)`
  - Base edition: $36,300
  - AO packs: (packs - 1) × $36,300
  - Internal users: tiered calculation
  - External users: tiered calculation
  - Unlimited: $60,500 × AO_packs

- [ ] **Task 3.3**: Implement `CalculateO11TieredInternalUsers(users)`
  - 100 included
  - Tier 1 (101-1000): $12,100/100
  - Tier 2 (1001-10000): $2,420/100
  - Tier 3 (10001+): $242/100

- [ ] **Task 3.4**: Implement `CalculateO11TieredExternalUsers(users)`
  - Tier 1 (1-10000): $4,840/1000
  - Tier 2 (10001-250000): $1,452/1000
  - Tier 3 (250001+): $30.25/1000

- [ ] **Task 3.5**: Implement `CalculateODCAddOns(config)`
  - All use formula: Rate × AO_Packs × Quantity
  - Support Extended/Premium (mutually exclusive)
  - AppShield (tiered)
  - HA, Non-Prod Runtime, Private Gateway, Sentry

- [ ] **Task 3.6**: Implement `CalculateO11AddOns(config, deployment)`
  - Check cloud-only restrictions
  - Support Premium: $3,630 × AO_packs
  - AppShield (tiered)
  - HA: $12,100 × AO_packs (Cloud only)
  - Sentry: $24,200 × AO_packs (Cloud only, makes HA included)
  - Log Streaming: $7,260 flat (Cloud only)
  - Non-Prod Env: $3,630 × AO_packs × qty
  - Load Test: $6,050 × AO_packs (Cloud only)
  - Env Pack: $9,680 × AO_packs × qty
  - DR: $12,100 × AO_packs (Self-Managed only)
  - DB Replica: $96,800 flat (Cloud only)

- [ ] **Task 3.7**: Implement `CalculateAppShieldCost(userVolume)`
  - Lookup tier from AppShield pricing table
  - Return flat tier price

- [ ] **Task 3.8**: Implement `CalculateServices(config, region)`
  - Get region-specific prices
  - Essential: qty × price
  - Premier: qty × price
  - Dedicated Group: qty × price
  - Public Session: qty × price
  - Expert Day: qty × price

- [ ] **Task 3.9**: Implement `ApplyDiscount(subtotals, discount)`
  - Handle Percentage vs FixedAmount
  - Handle scope (Total, LicenseOnly, AddOnsOnly, ServicesOnly)
  - Return discount amount

- [ ] **Task 3.10**: Implement `GenerateRecommendations(config, result)`
  - Compare unlimited vs tiered user costs
  - Suggest if unlimited would save money
  - Warn about cloud-only selections on self-managed

- [ ] **Task 3.11**: Implement main `CalculateOutSystemsCost(config)` method
  - Route to ODC or O11 calculations
  - Aggregate all costs
  - Apply discount
  - Generate warnings/recommendations
  - Return complete result

### Phase 4: Interface Updates
**Files**: `src/InfraSizingCalculator/Services/Interfaces/IPricingSettingsService.cs`

- [ ] **Task 4.1**: Add method signatures
  ```csharp
  OutSystemsPricingSettings GetOutSystemsPricingSettings();
  Task SaveOutSystemsPricingSettingsAsync(OutSystemsPricingSettings settings);
  OutSystemsPricingResult CalculateOutSystemsCost(OutSystemsDeploymentConfig config);
  bool IsCloudOnlyFeature(string featureName, OutSystemsDeployment deployment);
  decimal GetAppShieldPrice(int userVolume);
  decimal GetRegionServicePrice(OutSystemsRegion region, string serviceName);
  ```

### Phase 5: UI Component
**Files**: `src/InfraSizingCalculator/Components/Pricing/OutSystemsPricingPanel.razor`

- [ ] **Task 5.1**: Create platform selection section
  - ODC vs O11 toggle/cards
  - O11: Cloud vs Self-Managed toggle

- [ ] **Task 5.2**: Create region selection dropdown
  - Africa, Middle East, Americas, Europe, Asia Pacific

- [ ] **Task 5.3**: Create AO configuration section
  - Slider/input for total AOs
  - Display calculated pack count

- [ ] **Task 5.4**: Create user capacity section
  - Unlimited users toggle
  - Internal users input (disabled if unlimited)
  - External users input (disabled if unlimited)
  - AppShield user volume input (shown if unlimited + AppShield enabled)

- [ ] **Task 5.5**: Create add-ons section
  - Dynamic based on platform (ODC vs O11)
  - Cloud-only badges for O11
  - Disable cloud-only items when Self-Managed selected
  - Quantity inputs for applicable items
  - Show "Included" badge for HA when Sentry selected

- [ ] **Task 5.6**: Create services section
  - Success Plans
  - Bootcamps
  - Expert Days
  - Quantity inputs

- [ ] **Task 5.7**: Create discount section
  - Type dropdown (Percentage / Fixed Amount)
  - Scope dropdown (Total / License Only / Add-Ons Only / Services Only)
  - Value input
  - Notes text field

- [ ] **Task 5.8**: Create cost summary section
  - License subtotal with breakdown
  - Add-ons subtotal with breakdown
  - Services subtotal with breakdown
  - Gross total
  - Discount line (if applicable)
  - Net total
  - Warnings panel
  - Recommendations panel

- [ ] **Task 5.9**: Add real-time calculation
  - Recalculate on any input change
  - Debounce for performance

### Phase 6: Unit Tests
**Files**: `tests/InfraSizingCalculator.UnitTests/Services/OutSystemsPricingServiceTests.cs`

- [ ] **Task 6.1**: Test ODC license calculations
  - Base edition cost
  - AO pack scaling
  - User pack calculations
  - Unlimited users calculation

- [ ] **Task 6.2**: Test O11 license calculations
  - Base edition cost
  - AO pack scaling
  - Tiered internal user calculation (verify Example 3: 2000 users = $133,100)
  - Tiered external user calculation (verify Example 3: 1M users = $419,567.50)
  - Unlimited users calculation

- [ ] **Task 6.3**: Test ODC add-on calculations
  - Per-pack scaling
  - Quantity multipliers
  - AppShield tiers

- [ ] **Task 6.4**: Test O11 add-on calculations
  - Cloud deployment with all features
  - Self-Managed with cloud-only restrictions
  - Sentry making HA included
  - Flat vs per-pack pricing

- [ ] **Task 6.5**: Test services by region
  - Africa pricing
  - Middle East pricing

- [ ] **Task 6.6**: Test discount calculations
  - Percentage discount on total
  - Fixed amount discount
  - Scope-limited discounts

- [ ] **Task 6.7**: Test complete examples
  - Example 1: ODC = $367,250
  - Example 2: O11 Cloud = $2,079,400
  - Example 3: O11 Self-Managed = $1,091,737.50

- [ ] **Task 6.8**: Test recommendations
  - Unlimited vs tiered comparison
  - Cloud-only warnings

### Phase 7: Documentation Sync
**Files**: Various docs

- [ ] **Task 7.1**: Update `docs/technical/models.md`
  - Add new enums
  - Add new classes

- [ ] **Task 7.2**: Update `docs/technical/services.md`
  - Document pricing service methods

- [ ] **Task 7.3**: Update `docs/business/business-rules.md`
  - Add BR-OS-* rules for OutSystems pricing

- [ ] **Task 7.4**: Archive old gap analysis
  - Move to `docs/vendor-specs/OS/archive/`

---

## Part 5: Verification Test Cases

### Test Case 1: ODC (Example 1)
**Input**:
- Platform: ODC
- Region: Middle East
- AOs: 450 (3 packs)
- Internal: 1000 (10 packs)
- External: 5000 (5 packs)
- Add-ons: Support Extended, AppShield (6000), HA, Non-Prod Runtime ×2, Private Gateway, Sentry
- Services: Premier ×1, Dedicated ×1, Public ×1, Expert ×1

**Expected Output**:
- License: $151,250.00
- Add-ons: $148,830.00
- Services: $67,170.00
- **Total: $367,250.00**

### Test Case 2: O11 Cloud (Example 2)
**Input**:
- Platform: O11, Deployment: Cloud
- Region: Middle East
- AOs: 450 (3 packs)
- Users: Unlimited, AppShield Volume: 15,000,000
- Add-ons: Support Premium, AppShield, Sentry, Log Streaming ×1, Non-Prod ×1, Load Test ×1, Env Pack ×1, DB Replica ×1
- Services: Premier ×1, Dedicated ×1, Public ×1, Expert ×1

**Expected Output**:
- License: $290,400.00
- Add-ons: $1,721,830.00
- Services: $67,170.00
- **Total: $2,079,400.00**

### Test Case 3: O11 Self-Managed (Example 3)
**Input**:
- Platform: O11, Deployment: Self-Managed
- Region: Middle East
- AOs: 450 (3 packs)
- Internal: 2000, External: 1,000,000
- Add-ons: Support Premium, AppShield (1,002,000), Non-Prod ×1, Env Pack ×1, DR
- Services: Premier ×1, Dedicated ×1, Public ×1, Expert ×1

**Expected Output**:
- License: $661,567.50
- Add-ons: $363,000.00
- Services: $67,170.00
- **Total: $1,091,737.50**

### Test Case 4: Discount - 10% off total
**Input**: Same as Test Case 1 + 10% discount on Total
**Expected**:
- Gross: $367,250.00
- Discount: $36,725.00
- **Net: $330,525.00**

### Test Case 5: Unlimited vs Tiered Recommendation
**Input**: O11 Self-Managed with 2000 internal + 1M external
**Expected Recommendation**:
- Tiered cost: $552,667.50
- Unlimited cost: $181,500 (3 packs × $60,500)
- Savings: $371,167.50
- Message: "Switching to Unlimited Users would save $371,167.50"

---

## Part 6: Implementation Order

```
Week 1: Core Model (Phase 1)
├── Day 1-2: Enums and basic classes
├── Day 3-4: Config and Result models
└── Day 5: Settings class with all pricing data

Week 2: Database & Service (Phase 2-3)
├── Day 1: Entity updates
├── Day 2: Migration
├── Day 3-4: License calculations (ODC, O11)
└── Day 5: Add-on calculations

Week 3: Service & Tests (Phase 3-6)
├── Day 1: Services and discount calculations
├── Day 2: Recommendations engine
├── Day 3-4: Unit tests
└── Day 5: Verification against examples

Week 4: UI & Docs (Phase 5, 7)
├── Day 1-3: UI component
├── Day 4: Integration testing
└── Day 5: Documentation sync
```

---

## Part 7: Quick Reference Card

### Pricing Formulas

```
ODC:
  Edition = $30,250
  AOs = (packs - 1) × $18,150
  Internal = (packs - 1) × $6,050
  External = packs × $6,050
  Unlimited = $60,500 × AO_packs
  Add-ons = Rate × AO_packs × Qty

O11:
  Edition = $36,300
  AOs = (packs - 1) × $36,300
  Internal = Tiered($12,100 → $2,420 → $242 per 100)
  External = Tiered($4,840 → $1,452 → $30.25 per 1000)
  Unlimited = $60,500 × AO_packs
  Add-ons = Rate × AO_packs × Qty (except flat items)

Flat Items (O11 Cloud only):
  Log Streaming = $7,260
  Database Replica = $96,800

Cloud-Only (O11):
  HA, Sentry, Log Streaming, Load Test, DB Replica

Self-Managed Only (O11):
  Disaster Recovery

Sentry Rule:
  When Sentry ON → HA = Included (no cost)
```

---

## Appendix: Files to Create/Modify

| File | Action | Description |
|------|--------|-------------|
| `Models/Pricing/OutSystemsPricing.cs` | **Modify** | Add enums, config, result, settings classes |
| `Data/Entities/OutSystemsPricingEntity.cs` | **Modify** | Add pricing fields |
| `Data/InfraSizingDbContext.cs` | **Modify** | Configure entity |
| `Data/Migrations/YYYYMMDD_OutSystemsPricingV2.cs` | **Create** | Database migration |
| `Services/Interfaces/IPricingSettingsService.cs` | **Modify** | Add method signatures |
| `Services/Pricing/DatabasePricingSettingsService.cs` | **Modify** | Implement calculations |
| `Components/Pricing/OutSystemsPricingPanel.razor` | **Create** | UI component |
| `UnitTests/Services/OutSystemsPricingServiceTests.cs` | **Create** | Unit tests |
| `docs/technical/models.md` | **Modify** | Document new types |
| `docs/technical/services.md` | **Modify** | Document service methods |
| `docs/business/business-rules.md` | **Modify** | Add OS pricing rules |
