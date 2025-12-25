# Mendix Pricing Guide

This guide explains how Mendix pricing works in the Infrastructure Sizing Calculator, including deployment paths, cost calculations, and configuration options.

## Overview

The calculator supports three main Mendix deployment categories:

1. **Mendix Cloud** - Fully managed SaaS or Dedicated options
2. **Private Cloud** - Self-hosted on Azure or Kubernetes providers
3. **Other Deployments** - Server-based, StackIT, or SAP BTP

## Deployment Categories

### 1. Mendix Cloud (SaaS/Dedicated)

**SaaS (Multi-tenant)**
- Resource packs from XS to 4XL
- 99.5% SLA (Standard) or 99.95% SLA (Premium/Premium Plus)
- Starting at $516/year for XS pack
- Cloud tokens for resource allocation

**Dedicated (Single-tenant)**
- Private AWS VPC
- $368,100/year fixed cost
- Full infrastructure isolation

### 2. Private Cloud (Azure/Kubernetes)

**Mendix on Azure**
- Base package: $6,612/year (includes 3 environments)
- Additional environments: $722.40/year each

**Mendix on Kubernetes (EKS, AKS, GKE, OpenShift)**
- Base package: $6,360/year (includes 3 environments)
- Tiered pricing for additional environments:
  - 4-50 environments: $552/env/year
  - 51-100 environments: $408/env/year
  - 101-150 environments: $240/env/year
  - 151+ environments: FREE

### 3. Other Deployments

**Server-based (VMs/Docker)**
- Per-app license: $6,612/year
- Unlimited apps: $33,060/year

**StackIT/SAP BTP**
- Same pricing as Server-based

## User Licensing

All Mendix deployments require user licensing:

| Type | Price | Notes |
|------|-------|-------|
| Internal Users | $40,800 per 100 users/year | Rounded up |
| External Users | $60,000 per 250K users/year | Rounded up |

## Configuration in the Calculator

### Step 5: Mendix Configuration

In Step 5 (Configuration), the Mendix tab allows you to configure:

1. **Operator Replicas** - Number of Mendix operator instances (1-5)
2. **Number of Environments** - Total environments to license (for Private Cloud)
3. **Internal Users** - Number of internal application users
4. **External Users** - Number of external application users (in thousands)

### Step 6: Pricing Step

The Pricing step shows:

1. **Deployment-specific pricing** based on your selection
2. **User licensing costs** calculated from your user counts
3. **Total cost summary** (monthly, annual, 3-year TCO)
4. **Cost breakdown** showing each component

## Pricing Calculation Formula

### Private Cloud (K8s) Total Cost

```
Total Annual Cost = Base Platform Fee + Environment Tier Costs + User Licensing

Environment Tier Cost =
  (envs 4-50) × $552 +
  (envs 51-100) × $408 +
  (envs 101-150) × $240 +
  (envs 151+) × $0

User Licensing Cost =
  ceil(internal_users / 100) × $40,800 +
  ceil(external_users / 250,000) × $60,000
```

### Azure Total Cost

```
Total Annual Cost = Base Fee + Additional Environments + User Licensing

Additional Environments =
  max(0, total_envs - 3) × $722.40
```

## Example Calculations

### Example 1: Small K8s Deployment
- 10 environments, 100 internal users, 0 external users
- Platform: $6,360
- Environments: 7 × $552 = $3,864
- Users: 1 × $40,800 = $40,800
- **Total: $51,024/year**

### Example 2: Medium Azure Deployment
- 15 environments, 500 internal users, 100,000 external users
- Platform: $6,612
- Environments: 12 × $722.40 = $8,668.80
- Internal: 5 × $40,800 = $204,000
- External: 1 × $60,000 = $60,000
- **Total: $279,280.80/year**

## Settings Configuration

Default Mendix pricing values can be configured in the Settings page:

1. Navigate to Settings (gear icon)
2. Scroll to "Mendix Pricing" section
3. Adjust values as needed for your region/negotiated rates
4. Click Save

## Related Files

- `/Models/Pricing/MendixPricing.cs` - Pricing models and enums
- `/Services/Pricing/PricingSettingsService.cs` - Pricing calculation service
- `/Components/Wizard/Steps/PricingStep.razor` - Pricing UI component
- `/Components/Pages/Settings.razor` - Settings configuration UI

## Test Coverage

Unit tests for Mendix pricing calculations are in:
- `/tests/InfraSizingCalculator.UnitTests/MendixPricingCalculationTests.cs`

E2E tests for Mendix pricing flows are in:
- `/tests/InfraSizingCalculator.E2ETests/Pricing/MendixPricingFlowTests.cs`
