# OutSystems Pricing Implementation Plan

**Date**: January 2026
**Based On**: Gap Analysis from Partner Calculator (verified with tooltips)
**Branch**: `feature/outsystems-pricing-v2`

---

## Key Pricing Model Insights

### Critical Understanding

1. **Unlimited Users = $60,500 × Number of AO Packs** (not a flat fee)
2. **Most add-ons scale with AO packs** (price shown is per AO pack)
3. **ODC uses flat pack pricing** for users ($6,050 per pack)
4. **O11 uses tiered volume pricing** for users (decreasing per-user cost at higher volumes)
5. **AppShield uses flat tier pricing** based on user count (19 tiers)

---

## Phase 1: Model Updates

### 1.1 Add New Enums

```csharp
/// <summary>
/// OutSystems bundle tier options (primary selection in Partner Calculator)
/// </summary>
public enum OutSystemsBundleTier
{
    BusinessBasics,      // Entry tier - different inclusions per platform
    BusinessCritical,    // Mission-critical with HA/Sentry/DR
    StrategicCoE,        // Center of Excellence with extra environments
    ALaCarte             // Custom configuration (no bundle)
}

/// <summary>
/// Support level options
/// </summary>
public enum OutSystemsSupportLevel
{
    Standard8x5,         // 8x5 business hours (ODC Business Basics only)
    Extended24x7,        // 24x7 extended support (ODC add-on, O11 included)
    Premium24x7          // 24x7 premium support with SLA
}
```

### 1.2 Correct All Pricing Values

```csharp
// ==================== ODC PRICING (CORRECTED) ====================

public decimal ODCBasePrice { get; set; } = 30250m;              // Was: 36300
public decimal ODCAdditionalAOPackPrice { get; set; } = 18150m;  // Was: 36300
public decimal ODCInternalUserPackPrice { get; set; } = 6050m;   // Was: 6000
public decimal ODCExternalUserPackPrice { get; set; } = 6050m;   // Was: 4840
public decimal ODCUnlimitedUsersPerAOPack { get; set; } = 60500m; // NEW: per AO pack, not flat!

// ==================== ODC ADD-ONS (CORRECTED - all per AO pack) ====================

public decimal ODCSupport24x7ExtendedPerAOPack { get; set; } = 6050m;   // NEW
public decimal ODCSupport24x7PremiumPerAOPack { get; set; } = 9680m;    // Was: 3630
public decimal ODCHighAvailabilityPerAOPack { get; set; } = 18150m;    // Was: 12100
public decimal ODCNonProductionRuntimePerAOPack { get; set; } = 6050m; // Was: 3630
public decimal ODCPrivateGatewayPerAOPack { get; set; } = 1210m;       // NEW
public decimal ODCSentryPerAOPack { get; set; } = 6050m;               // Was: 24200

// ==================== O11 PRICING ====================

public decimal EnterpriseEditionBasePrice { get; set; } = 36300m;      // Correct
public decimal AdditionalAOPackPrice { get; set; } = 36300m;           // Correct
public decimal O11UnlimitedUsersPerAOPack { get; set; } = 60500m;      // NEW: per AO pack!

// O11 uses TIERED pricing for users (not flat packs like ODC)
// See InternalUserTieredPricing and ExternalUserTieredPricing classes

// ==================== O11 ADD-ONS ====================

// Support 24x7 Extended is INCLUDED for O11 (no charge)
public decimal Support24x7PremiumPerAOPack { get; set; } = 3630m;      // Correct
public decimal HighAvailabilityPerAOPack { get; set; } = 12100m;       // Correct (Cloud only)
public decimal SentryPerAOPack { get; set; } = 24200m;                 // Correct (Cloud only, inc. HA)
public decimal LogStreamingFlat { get; set; } = 7260m;                 // Correct (Cloud only, flat)
public decimal NonProductionEnvFlat { get; set; } = 7260m;             // CORRECTED: flat, not per-pack
public decimal LoadTestEnvPerAOPack { get; set; } = 6050m;             // Correct (Cloud only)
public decimal EnvironmentPackPerAOPack { get; set; } = 9680m;         // Correct
public decimal DisasterRecoveryPerAOPack { get; set; } = 12100m;       // Correct
public decimal DatabaseReplicaFlat { get; set; } = 96800m;             // Correct (Cloud only, flat)

// ==================== SERVICES (CORRECTED) ====================

public decimal EssentialSuccessPlanPrice { get; set; } = 30250m;       // Correct
public decimal PremierSuccessPlanPrice { get; set; } = 60500m;         // Correct
public decimal DedicatedGroupSessionPrice { get; set; } = 2670m;       // Was: 3820
public decimal PublicSessionPrice { get; set; } = 480m;                // Was: 720
public decimal ExpertDayPrice { get; set; } = 1400m;                   // Was: 2640
```

### 1.3 Add Tiered Pricing Classes (O11 Only)

```csharp
/// <summary>
/// O11 Internal user tiered pricing (Enterprise Edition)
/// First 100 users included, then tiered pricing applies
/// </summary>
public class O11InternalUserTieredPricing
{
    public int IncludedUsers { get; set; } = 100;

    public List<UserPricingTier> Tiers { get; set; } = new()
    {
        new() { MinUsers = 200, MaxUsers = 1000, PricePer100 = 12100m },      // $121/user
        new() { MinUsers = 1100, MaxUsers = 10000, PricePer100 = 2420m },     // $24.20/user
        new() { MinUsers = 10100, MaxUsers = 100000000, PricePer100 = 242m }  // $2.42/user
    };
}

/// <summary>
/// O11 External user tiered pricing (Enterprise Edition)
/// </summary>
public class O11ExternalUserTieredPricing
{
    public List<UserPricingTier> Tiers { get; set; } = new()
    {
        new() { MinUsers = 1000, MaxUsers = 10000, PricePer1000 = 4840m },       // $4.84/user
        new() { MinUsers = 11000, MaxUsers = 250000, PricePer1000 = 1452m },     // $1.45/user
        new() { MinUsers = 251000, MaxUsers = 100000000, PricePer1000 = 30.25m } // $0.03/user
    };
}

/// <summary>
/// AppShield 19-tier pricing (both ODC and O11)
/// Flat price based on total user count - NOT per-user!
/// </summary>
public class AppShieldTieredPricing
{
    public List<AppShieldTier> Tiers { get; set; } = new()
    {
        new() { MinUsers = 0, MaxUsers = 10000, FlatPrice = 18150m },
        new() { MinUsers = 10001, MaxUsers = 50000, FlatPrice = 32670m },
        new() { MinUsers = 50001, MaxUsers = 100000, FlatPrice = 54450m },
        new() { MinUsers = 100001, MaxUsers = 500000, FlatPrice = 96800m },
        new() { MinUsers = 500001, MaxUsers = 1000000, FlatPrice = 234740m },
        new() { MinUsers = 1000001, MaxUsers = 2000000, FlatPrice = 275880m },
        new() { MinUsers = 2000001, MaxUsers = 3000000, FlatPrice = 358160m },
        new() { MinUsers = 3000001, MaxUsers = 4000000, FlatPrice = 411400m },
        new() { MinUsers = 4000001, MaxUsers = 5000000, FlatPrice = 508200m },
        new() { MinUsers = 5000001, MaxUsers = 6000000, FlatPrice = 605000m },
        new() { MinUsers = 6000001, MaxUsers = 7000000, FlatPrice = 701800m },
        new() { MinUsers = 7000001, MaxUsers = 8000000, FlatPrice = 798600m },
        new() { MinUsers = 8000001, MaxUsers = 9000000, FlatPrice = 895400m },
        new() { MinUsers = 9000001, MaxUsers = 10000000, FlatPrice = 992200m },
        new() { MinUsers = 10000001, MaxUsers = 11000000, FlatPrice = 1089000m },
        new() { MinUsers = 11000001, MaxUsers = 12000000, FlatPrice = 1185800m },
        new() { MinUsers = 12000001, MaxUsers = 13000000, FlatPrice = 1282600m },
        new() { MinUsers = 13000001, MaxUsers = 14000000, FlatPrice = 1379400m },
        new() { MinUsers = 14000001, MaxUsers = 15000000, FlatPrice = 1476200m }
    };
}
```

### 1.4 Add Bundle Configuration Classes

```csharp
/// <summary>
/// Bundle configuration with all inclusions
/// </summary>
public class BundleConfig
{
    public decimal Price { get; set; }
    public int AOs { get; set; }
    public int InternalUsers { get; set; }
    public int DevEnvironments { get; set; }
    public int NonProdEnvironments { get; set; }
    public int ProdEnvironments { get; set; }
    public OutSystemsSupportLevel Support { get; set; }
    public bool IncludesPrivateGateway { get; set; }  // ODC only
    public bool IncludesHA { get; set; }
    public bool IncludesSentry { get; set; }
    public bool IncludesDR { get; set; }              // O11 Self-Managed only

    public int TotalEnvironments => DevEnvironments + NonProdEnvironments + ProdEnvironments;
    public int AOPacks => (int)Math.Ceiling(AOs / 150.0);
}

// ODC Bundles
public static readonly Dictionary<OutSystemsBundleTier, BundleConfig> ODCBundles = new()
{
    { OutSystemsBundleTier.BusinessBasics, new BundleConfig
    {
        Price = 88000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Standard8x5,
        IncludesPrivateGateway = true
    }},
    { OutSystemsBundleTier.BusinessCritical, new BundleConfig
    {
        Price = 179000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Extended24x7,
        IncludesPrivateGateway = true, IncludesHA = true, IncludesSentry = true
    }},
    { OutSystemsBundleTier.StrategicCoE, new BundleConfig
    {
        Price = 179000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Extended24x7,
        IncludesPrivateGateway = true, IncludesHA = true, IncludesSentry = true
    }}
};

// O11 Cloud Bundles
public static readonly Dictionary<OutSystemsBundleTier, BundleConfig> O11CloudBundles = new()
{
    { OutSystemsBundleTier.BusinessBasics, new BundleConfig
    {
        Price = 109000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Extended24x7
    }},
    { OutSystemsBundleTier.BusinessCritical, new BundleConfig
    {
        Price = 182000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Extended24x7,
        IncludesSentry = true  // Sentry includes HA
    }},
    { OutSystemsBundleTier.StrategicCoE, new BundleConfig
    {
        Price = 211000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 2, ProdEnvironments = 2,
        Support = OutSystemsSupportLevel.Extended24x7,
        IncludesSentry = true
    }}
};

// O11 Self-Managed Bundles
public static readonly Dictionary<OutSystemsBundleTier, BundleConfig> O11SelfManagedBundles = new()
{
    { OutSystemsBundleTier.BusinessBasics, new BundleConfig
    {
        Price = 109000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Extended24x7
    }},
    { OutSystemsBundleTier.BusinessCritical, new BundleConfig
    {
        Price = 145000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 1, ProdEnvironments = 1,
        Support = OutSystemsSupportLevel.Extended24x7,
        IncludesDR = true
    }},
    { OutSystemsBundleTier.StrategicCoE, new BundleConfig
    {
        Price = 174000m, AOs = 450, InternalUsers = 100,
        DevEnvironments = 1, NonProdEnvironments = 2, ProdEnvironments = 2,
        Support = OutSystemsSupportLevel.Extended24x7,
        IncludesDR = true
    }}
};
```

---

## Phase 2: Service Layer Updates

### 2.1 Key Calculation Methods

```csharp
/// <summary>
/// Calculate number of AO packs (critical for pricing)
/// </summary>
public int CalculateAOPacks(int totalAOs, int includedAOs = 150)
{
    if (totalAOs <= includedAOs) return 1;
    return (int)Math.Ceiling(totalAOs / 150.0);
}

/// <summary>
/// Calculate Unlimited Users cost (per AO pack!)
/// </summary>
public decimal CalculateUnlimitedUsersCost(int aoPacks)
{
    // $60,500 per AO pack for both ODC and O11
    return 60500m * aoPacks;
}

/// <summary>
/// Calculate ODC user costs (flat pack pricing)
/// </summary>
public decimal CalculateODCUserCost(int internalUsers, int externalUsers, int includedInternal = 100)
{
    decimal cost = 0;

    // Internal users: $6,050 per pack of 100
    if (internalUsers > includedInternal)
    {
        int additionalUsers = internalUsers - includedInternal;
        int packs = (int)Math.Ceiling(additionalUsers / 100.0);
        cost += packs * 6050m;
    }

    // External users: $6,050 per pack of 1000
    if (externalUsers > 0)
    {
        int packs = (int)Math.Ceiling(externalUsers / 1000.0);
        cost += packs * 6050m;
    }

    return cost;
}

/// <summary>
/// Calculate O11 internal user cost (tiered pricing)
/// </summary>
public decimal CalculateO11InternalUserCost(int totalUsers, int includedUsers = 100)
{
    if (totalUsers <= includedUsers) return 0;

    int billableUsers = totalUsers - includedUsers;
    decimal cost = 0;
    int remaining = billableUsers;

    // Tier 1: 200-1,000 users = $12,100 per 100 ($121/user)
    if (remaining > 0 && totalUsers <= 1000)
    {
        int usersInTier = Math.Min(remaining, 1000 - includedUsers);
        int packs = (int)Math.Ceiling(usersInTier / 100.0);
        cost += packs * 12100m;
        remaining -= usersInTier;
    }

    // Tier 2: 1,100-10,000 users = $2,420 per 100 ($24.20/user)
    if (remaining > 0 && totalUsers > 1000)
    {
        int tierStart = Math.Max(1100, includedUsers + 1);
        int usersInTier = Math.Min(remaining, 10000 - tierStart + 1);
        int packs = (int)Math.Ceiling(usersInTier / 100.0);
        cost += packs * 2420m;
        remaining -= usersInTier;
    }

    // Tier 3: 10,100+ users = $242 per 100 ($2.42/user)
    if (remaining > 0)
    {
        int packs = (int)Math.Ceiling(remaining / 100.0);
        cost += packs * 242m;
    }

    return cost;
}

/// <summary>
/// Calculate O11 external user cost (tiered pricing)
/// </summary>
public decimal CalculateO11ExternalUserCost(int totalUsers)
{
    if (totalUsers <= 0) return 0;

    decimal cost = 0;
    int remaining = totalUsers;

    // Tier 1: 1,000-10,000 = $4,840 per 1000
    int tier1Users = Math.Min(remaining, 10000);
    if (tier1Users > 0)
    {
        int packs = (int)Math.Ceiling(tier1Users / 1000.0);
        cost += packs * 4840m;
        remaining -= tier1Users;
    }

    // Tier 2: 11,000-250,000 = $1,452 per 1000
    if (remaining > 0)
    {
        int tier2Users = Math.Min(remaining, 250000 - 10000);
        int packs = (int)Math.Ceiling(tier2Users / 1000.0);
        cost += packs * 1452m;
        remaining -= tier2Users;
    }

    // Tier 3: 251,000+ = $30.25 per 1000
    if (remaining > 0)
    {
        int packs = (int)Math.Ceiling(remaining / 1000.0);
        cost += packs * 30.25m;
    }

    return cost;
}

/// <summary>
/// Calculate AppShield cost (flat tier based on user count)
/// </summary>
public decimal CalculateAppShieldCost(int totalUsers)
{
    if (totalUsers <= 0) return 0;

    var tier = _pricing.AppShieldTiers.Tiers
        .FirstOrDefault(t => totalUsers >= t.MinUsers && totalUsers <= t.MaxUsers);

    // Return highest tier if over max
    return tier?.FlatPrice ?? _pricing.AppShieldTiers.Tiers.Last().FlatPrice;
}

/// <summary>
/// Calculate add-on cost (most scale with AO packs)
/// </summary>
public decimal CalculateAddOnCost(decimal perPackPrice, int aoPacks)
{
    return perPackPrice * aoPacks;
}
```

### 2.2 Main Calculation Flow

```csharp
public OutSystemsPricingResult CalculateOutSystemsCost(OutSystemsDeploymentConfig config)
{
    var result = new OutSystemsPricingResult();
    var pricing = GetPricingSettings();

    // Step 1: Determine AO pack count (critical for all pricing)
    int aoPacks = CalculateAOPacks(config.TotalApplicationObjects);
    result.AOPacks = aoPacks;

    // Step 2: Bundle or A La Carte
    if (config.BundleTier != OutSystemsBundleTier.ALaCarte)
    {
        var bundle = GetBundleConfig(config.Platform, config.DeploymentType, config.BundleTier);
        result.BundleBaseCost = bundle.Price;
        result.IncludedAOs = bundle.AOs;
        result.IncludedInternalUsers = bundle.InternalUsers;

        // Additional AOs beyond bundle (bundles include 450 = 3 packs)
        int bundleAOPacks = bundle.AOPacks;
        if (aoPacks > bundleAOPacks)
        {
            int additionalPacks = aoPacks - bundleAOPacks;
            decimal packPrice = config.Platform == OutSystemsPlatform.ODC
                ? pricing.ODCAdditionalAOPackPrice
                : pricing.AdditionalAOPackPrice;
            result.AdditionalAOsCost = additionalPacks * packPrice;
        }
    }
    else
    {
        // A La Carte pricing
        if (config.Platform == OutSystemsPlatform.ODC)
        {
            result.EditionBaseCost = pricing.ODCBasePrice;
            // First pack included, additional packs charged
            if (aoPacks > 1)
            {
                result.AdditionalAOsCost = (aoPacks - 1) * pricing.ODCAdditionalAOPackPrice;
            }
        }
        else // O11
        {
            result.EditionBaseCost = pricing.EnterpriseEditionBasePrice;
            if (aoPacks > 1)
            {
                result.AdditionalAOsCost = (aoPacks - 1) * pricing.AdditionalAOPackPrice;
            }
        }
    }

    // Step 3: User licensing
    if (config.UseUnlimitedUsers)
    {
        // Unlimited Users = $60,500 × AO packs
        result.UserLicenseCost = CalculateUnlimitedUsersCost(aoPacks);
        result.UnlimitedUsersEnabled = true;
    }
    else
    {
        int includedUsers = config.BundleTier != OutSystemsBundleTier.ALaCarte
            ? GetBundleConfig(...).InternalUsers
            : (config.Platform == OutSystemsPlatform.ODC ? 100 : 100);

        if (config.Platform == OutSystemsPlatform.ODC)
        {
            // ODC: flat pack pricing
            result.UserLicenseCost = CalculateODCUserCost(
                config.InternalUsers, config.ExternalUsers, includedUsers);
        }
        else
        {
            // O11: tiered pricing
            result.UserLicenseCost =
                CalculateO11InternalUserCost(config.InternalUsers, includedUsers) +
                CalculateO11ExternalUserCost(config.ExternalUsers);
        }
    }

    // Step 4: Add-ons (most scale with AO packs)
    CalculateAddOns(config, pricing, aoPacks, result);

    // Step 5: Services
    CalculateServices(config, pricing, result);

    // Step 6: Total
    result.TotalAnnualCost = result.BundleBaseCost + result.EditionBaseCost +
        result.AdditionalAOsCost + result.UserLicenseCost +
        result.AddOnsCost + result.ServicesCost;

    return result;
}
```

---

## Phase 3: UI Updates

### 3.1 Primary Flow (matches Partner Calculator)

```
1. Platform Selection
   ├── OutSystems Developer Cloud (ODC)
   ├── OutSystems 11 Cloud
   └── OutSystems 11 Self-Managed

2. Bundle Tier Selection (Card Layout)
   ├── Business Basics ($88k-$109k)
   ├── Business Critical ($145k-$182k)
   ├── Strategic CoE ($174k-$211k)
   └── Build Your Own (A La Carte)

3. Customize (if needed)
   ├── Application Objects slider
   ├── Internal Users input
   ├── External Users input
   └── Unlimited Users toggle

4. Add-Ons (with "Included" badges for bundle items)
   ├── Support upgrades
   ├── HA / Sentry / DR
   ├── Additional environments
   └── AppShield (tiered selector)

5. Services
   ├── Success Plan selection
   └── Training & consulting

6. Summary
   └── Line-item breakdown with totals
```

### 3.2 Key UI Elements

- **Bundle cards** showing included features with checkmarks
- **"Included in Bundle"** badges on add-ons
- **AO Pack counter** showing how many packs selected
- **Tiered pricing indicator** for AppShield
- **Cloud-only badges** on restricted add-ons

---

## Phase 4: Testing

### 4.1 Critical Test Cases

```csharp
[Fact]
public void UnlimitedUsers_ShouldScaleWithAOPacks()
{
    // 300 AOs = 2 packs
    // Unlimited Users = $60,500 × 2 = $121,000
    var config = new OutSystemsDeploymentConfig
    {
        Platform = OutSystemsPlatform.ODC,
        TotalApplicationObjects = 300,
        UseUnlimitedUsers = true
    };

    var result = _service.CalculateOutSystemsCost(config);

    result.UserLicenseCost.Should().Be(121000m);
}

[Fact]
public void ODC_UserPricing_ShouldUseFlatPacks()
{
    // ODC: 250 internal users = 100 included + 150 additional
    // 150 users = 2 packs × $6,050 = $12,100
    var config = new OutSystemsDeploymentConfig
    {
        Platform = OutSystemsPlatform.ODC,
        InternalUsers = 250
    };

    var result = _service.CalculateOutSystemsCost(config);

    result.UserLicenseCost.Should().Be(12100m);
}

[Fact]
public void O11_InternalUsers_ShouldUseTieredPricing()
{
    // O11: 500 internal users
    // 100 included, 400 billable
    // Tier 1 (200-1000): 4 packs × $12,100 = $48,400
    var config = new OutSystemsDeploymentConfig
    {
        Platform = OutSystemsPlatform.O11,
        InternalUsers = 500
    };

    var result = _service.CalculateOutSystemsCost(config);

    result.UserLicenseCost.Should().Be(48400m);
}

[Fact]
public void AppShield_ShouldUseFlatTierPricing()
{
    // 25,000 users = Tier 2 = $32,670 flat
    var cost = _service.CalculateAppShieldCost(25000);

    cost.Should().Be(32670m);
}
```

---

## Implementation Checklist

### Phase 1: Models
- [ ] Add `OutSystemsBundleTier` enum
- [ ] Add `OutSystemsSupportLevel` enum
- [ ] Fix all ODC pricing values
- [ ] Fix all O11 pricing values
- [ ] Add `O11InternalUserTieredPricing` class
- [ ] Add `O11ExternalUserTieredPricing` class
- [ ] Add `AppShieldTieredPricing` class with 19 tiers
- [ ] Add `BundleConfig` class
- [ ] Add bundle definitions for all 9 bundles

### Phase 2: Services
- [ ] Add `CalculateAOPacks` method
- [ ] Add `CalculateUnlimitedUsersCost` (per AO pack!)
- [ ] Add `CalculateODCUserCost` (flat packs)
- [ ] Add `CalculateO11InternalUserCost` (tiered)
- [ ] Add `CalculateO11ExternalUserCost` (tiered)
- [ ] Add `CalculateAppShieldCost` (tiered flat)
- [ ] Update main calculation method

### Phase 3: UI
- [ ] Add bundle selection cards
- [ ] Add "Included" badges
- [ ] Add AO pack counter display
- [ ] Update pricing summary

### Phase 4: Testing
- [ ] Unlimited users × AO packs test
- [ ] ODC flat pack user tests
- [ ] O11 tiered user tests
- [ ] AppShield tier tests
- [ ] Bundle calculation tests
- [ ] Add-on × AO packs tests

---

## Validation Checklist

After implementation, verify these scenarios match Partner Calculator:

- [ ] ODC Business Basics with 300 AOs = $88k + additional AO cost
- [ ] ODC Unlimited Users with 300 AOs (2 packs) = $121,000
- [ ] O11 Cloud with 5,000 internal users = tiered pricing applied
- [ ] O11 Self-Managed Business Critical = $145k with DR included
- [ ] AppShield for 100,000 users = $54,450 (Tier 3)
- [ ] Services pricing matches new values
