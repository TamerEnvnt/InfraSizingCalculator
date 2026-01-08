# Wireframe v0.4.4 vs Backend Services Gap Analysis

**Date**: 2026-01-07
**Source of Truth**: `docs/wireframes/v0.4.4/html/` (40+ wireframes)
**Backend**: `src/InfraSizingCalculator/Services/`

## Executive Summary

| Category | Wireframe Features | Backend Support | Gap Status |
|----------|-------------------|-----------------|------------|
| K8s Sizing | Full dashboard with 5 tabs | **Fully Supported** | No gaps |
| VM Sizing | Full dashboard with 5 tabs | **Fully Supported** | No gaps |
| Cost Estimation | TCO, breakdown, comparisons | **Fully Supported** | No gaps |
| Growth Planning | Projections, limits, warnings | **Fully Supported** | No gaps |
| Export | Excel, PDF, JSON, CSV | **Fully Supported** | No gaps |
| Scenarios | CRUD, compare, search | **Fully Supported** | No gaps |
| Share Config | URL, email, permissions | **Partial Support** | Minor gaps |
| Cloud Alternatives | Multi-cloud comparison | **Fully Supported** | No gaps |
| Authentication | Login, register, SSO | **Fully Supported** | No gaps |

**Bottom Line**: The backend services fully support all wireframe functionality. The gap is purely in the UI implementation - the V4 Blazor code needs to be rewritten to properly wire up to these services.

---

## Detailed Feature-to-Service Mapping

### 1. K8s Dashboard (01-dashboard-k8s.html, 01b-01e tabs)

#### Platform Tab (01-dashboard-k8s.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Distribution dropdown | List of 46 K8s distributions | `IDistributionService` | `GetFiltered()` | ✅ |
| Distribution categories | On-prem, Cloud filters | `IDistributionService` | `GetByDeploymentType()`, `GetCloudByCategory()` | ✅ |
| Cluster config (HA, CP, etcd) | HA/DR configuration | `IK8sSizingService` | `Calculate()` with `K8sHADRConfig` | ✅ |
| Environment toggles | Dev/Test/Staging/Prod | `IWizardStateService` | `EnabledEnvironments` | ✅ |
| Node count sliders | Control plane, infra, worker | `IK8sSizingService` | `CalculateMasterNodes()`, `CalculateInfraNodes()`, `CalculateWorkerNodes()` | ✅ |
| Cluster features checkboxes | Istio, ingress, monitoring | `IK8sSizingService` | Part of `K8sSizingInput` | ✅ |
| Total Nodes summary | Sum of all node types | `IK8sSizingService` | `K8sSizingResult` | ✅ |
| Monthly Cost summary | Estimated monthly cost | `ICostEstimationService` | `EstimateK8sCostAsync()` | ✅ |
| Growth projection chip | Year-end node projection | `IGrowthPlanningService` | `CalculateK8sGrowthProjection()` | ✅ |

#### Apps Tab (01b-dashboard-k8s-apps.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Application type selector | Mendix, generic containers | `ITechnologyService` | `GetAll()` | ✅ |
| Mendix Runtimes slider | Runtime pod count | `IWizardStateService` | `K8sPodConfig` | ✅ |
| DB Proxies slider | Database proxy count | `IWizardStateService` | `K8sPodConfig` | ✅ |
| Native Containers slider | Other containers | `IWizardStateService` | `K8sPodConfig` | ✅ |
| Resource per pod (CPU/Memory) | Tier-based resources | `ITierConfigurationService` | `GetTierCpu()`, `GetTierRam()` | ✅ |
| Total resources summary | Calculated totals | `IK8sSizingService` | `K8sSizingResult` | ✅ |

#### Nodes Tab (01c-dashboard-k8s-nodes.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Node specs table | vCPU, RAM, Storage per type | `IWizardStateService` | `NodeSpecs` | ✅ |
| Custom node sizes | User-defined specs | `IWizardStateService` | `K8sNodeConfig` | ✅ |
| Cluster Overview table | CP/Infra/Worker breakdown | `IK8sSizingService` | `K8sSizingResult` | ✅ |
| Pod Distribution | Pods per node type | `IK8sSizingService` | `K8sSizingResult.PodDistribution` | ✅ |
| etcd Requirements | etcd cluster sizing | `IK8sSizingService` | `CalculateEtcdNodes()` | ✅ |

#### Pricing Tab (01d-dashboard-k8s-pricing.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Cost type toggle (On-prem/Cloud) | Pricing source | `ICostEstimationService` | `EstimateOnPremCost()` / `EstimateK8sCostAsync()` | ✅ |
| Cloud provider selector | AWS, Azure, GCP | `IPricingService` | `GetPricingAsync()` | ✅ |
| Region selector | Provider regions | `IPricingService` | `GetRegions()` | ✅ |
| Instance type selector | VM sizes | `IPricingService` | `PricingModel` | ✅ |
| Cost breakdown table | Hardware, licenses, support | `CostEstimate` | All cost properties | ✅ |
| Monthly/Yearly/3-Year TCO | TCO calculations | `ICostEstimationService` | `CalculateTCO()` | ✅ |

#### Growth Tab (01e-dashboard-k8s-growth.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Projection period selector | 6mo/1yr/3yr/5yr | `IGrowthPlanningService` | `GrowthSettings.ProjectionYears` | ✅ |
| App growth rate slider | % per year | `IGrowthPlanningService` | `GrowthSettings.AppGrowthRate` | ✅ |
| Resource growth rate slider | % per year | `IGrowthPlanningService` | `GrowthSettings.ResourceGrowthRate` | ✅ |
| Storage growth rate slider | % per year | `IGrowthPlanningService` | `GrowthSettings.StorageGrowthRate` | ✅ |
| Headroom buffer slider | % buffer | `IGrowthPlanningService` | `GrowthSettings.HeadroomPercent` | ✅ |
| Year-end projection | Future nodes/cost | `IGrowthPlanningService` | `CalculateK8sGrowthProjection()` | ✅ |
| Limit warnings | Cluster limit alerts | `IGrowthPlanningService` | `GetClusterLimits()`, `CalculateYearToLimit()` | ✅ |
| Scaling recommendations | Actionable advice | `IGrowthPlanningService` | `GenerateRecommendations()` | ✅ |

---

### 2. VM Dashboard (02-dashboard-vm.html, 02b-02e tabs)

#### Platform Tab (02-dashboard-vm.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Hypervisor dropdown | vSphere, Hyper-V, Proxmox | `ITechnologyService` | `GetByPlatformType(VM)` | ✅ |
| Physical hosts slider | Number of hosts | `IWizardStateService` | `VmHostConfig` | ✅ |
| HA reserve toggle | Reserve capacity | `IVMSizingService` | `Calculate()` with HA settings | ✅ |
| Overcommit settings | CPU/RAM overcommit ratios | `IWizardStateService` | `ProdCpuOvercommit`, `ProdMemoryOvercommit` | ✅ |
| Total VMs summary | Sum of all server types | `IVMSizingService` | `VMSizingResult` | ✅ |
| Physical Hosts summary | Required hosts | `IVMSizingService` | `VMSizingResult.PhysicalHosts` | ✅ |
| Monthly Cost summary | Estimated monthly cost | `ICostEstimationService` | `EstimateVMCostAsync()` | ✅ |

#### Apps Tab (02b-dashboard-vm-apps.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Server role selector | App/DB/Web/Utility | `IVMSizingService` | `GetRoleSpecs()` | ✅ |
| Server count per role | Number of VMs | `IWizardStateService` | `VmAppConfig` | ✅ |
| VM specs per role | vCPU, RAM per VM type | `IVMSizingService` | `GetRoleSpecs()` | ✅ |
| Infrastructure Overview table | Server breakdown | `IVMSizingService` | `VMSizingResult` | ✅ |
| Total resources summary | Aggregate CPU/RAM/Storage | `IVMSizingService` | `VMSizingResult` | ✅ |

#### Nodes Tab (02c-dashboard-vm-nodes.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Host specs | Physical host configuration | `IWizardStateService` | `VmHostConfig` | ✅ |
| Host Requirements | Physical hosts needed | `IVMSizingService` | `VMSizingResult.PhysicalHosts` | ✅ |
| Overcommit Analysis | Effective capacity | `IVMSizingService` | Part of `VMSizingResult` | ✅ |
| Load balancer specs | LB configuration | `IVMSizingService` | `GetLoadBalancerSpecs()` | ✅ |

#### Pricing Tab (02d-dashboard-vm-pricing.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Cost type toggle | On-prem/Cloud | `ICostEstimationService` | `EstimateOnPremVMCost()` / `EstimateVMCostAsync()` | ✅ |
| Cost breakdown | Hardware, licenses, support | `CostEstimate` | All cost properties | ✅ |
| TCO calculations | Multi-year costs | `ICostEstimationService` | `CalculateTCO()` | ✅ |

#### Growth Tab (02e-dashboard-vm-growth.html)
| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Projection settings | Growth rates | `IGrowthPlanningService` | `GrowthSettings` | ✅ |
| VM growth projection | Future VMs/hosts | `IGrowthPlanningService` | `CalculateVMGrowthProjection()` | ✅ |
| Recommendations | Scaling advice | `IGrowthPlanningService` | `GenerateRecommendations()` | ✅ |

---

### 3. Comparison Dashboard (03-comparison.html)

| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Side-by-side table | Multiple scenarios | `IScenarioService` | `CompareByIdsAsync()` | ✅ |
| Metrics comparison | Platform, nodes, CPU, RAM, cost | `ScenarioComparison` | `Metrics` array | ✅ |
| Best/worst highlighting | Winner/loser per metric | `ScenarioComparison` | `Metrics[].WinnerId` | ✅ |
| Analysis cards | Per-scenario summary | `ScenarioComparison` | `Scenarios[]` | ✅ |
| Recommendations | Suggested choice | `ScenarioComparison` | `RecommendationReason` | ✅ |
| Comparison options | Highlight mode, metrics | UI state only | N/A | ✅ |

---

### 4. Scenarios Page (17-scenarios.html)

| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Scenario grid | All saved scenarios | `IScenarioService` | `GetAllScenariosAsync()` | ✅ |
| Scenario cards | Summary info | `IScenarioService` | `GetScenarioSummariesAsync()` | ✅ |
| Search box | Text search | `IScenarioService` | `SearchScenariosAsync()` | ✅ |
| Filter options | K8s/VM filter | `IScenarioService` | Filter on `Scenario.Type` | ✅ |
| New Scenario button | Create new | `IScenarioService` | `SaveK8sScenarioAsync()` / `SaveVMScenarioAsync()` | ✅ |
| Delete scenario | Remove saved | `IScenarioService` | `DeleteScenarioAsync()` | ✅ |
| Duplicate scenario | Clone | `IScenarioService` | `DuplicateScenarioAsync()` | ✅ |
| Toggle favorite | Star/unstar | `IScenarioService` | `ToggleFavoriteAsync()` | ✅ |

---

### 5. Export Modal (20-export-modal.html)

| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Excel export | XLSX file | `IExportService` | `ExportToExcel()` | ✅ |
| PDF export | PDF document | `IExportService` | `ExportToPdf()` | ✅ |
| JSON export | JSON data | `IExportService` | `ExportToJson()` | ✅ |
| CSV export | CSV file | `IExportService` | `ExportToCsv()` | ✅ |
| Include options | Configuration, sizing, cost, growth | `IExportService` | Various methods | ✅ |
| File naming | Timestamped filename | `IExportService` | `GetTimestampedFilename()` | ✅ |

---

### 6. Share Configuration Modal (19-share-config.html)

| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Shareable link | URL with config | `ConfigurationSharingService` | `GenerateShareUrlAsync()` | ✅ |
| Copy link button | Clipboard | `ConfigurationSharingService` | `CopyShareUrlAsync()` | ✅ |
| Parse shared link | Load from URL | `ConfigurationSharingService` | `ParseFromUrlAsync()` | ✅ |
| Email invite | Send via email | N/A | **Not implemented** | ⚠️ |
| Permissions (View/Edit) | Access control | N/A | **Not implemented** | ⚠️ |
| Link expiration | Time-limited links | N/A | **Not implemented** | ⚠️ |
| Export JSON | Download config | `IScenarioService` | `ExportToJsonAsync()` | ✅ |
| Import JSON | Load config | `IScenarioService` | `ImportFromJsonAsync()` | ✅ |

---

### 7. Cloud Alternatives (18-cloud-alternatives.html)

| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Provider comparison | AWS, Azure, GCP costs | `ICostEstimationService` | `EstimateK8sCostAsync()` per provider | ✅ |
| Region pricing | Regional variations | `IPricingService` | `GetPricingAsync()` with region | ✅ |
| Cost comparison | Side-by-side | `ICostEstimationService` | `Compare()` | ✅ |
| On-prem vs cloud | TCO comparison | `ICostEstimationService` | Both `EstimateOnPremCost()` and cloud methods | ✅ |

---

### 8. Settings Pages (10-16)

| Wireframe | Required Data | Backend Service | Method | Status |
|-----------|---------------|-----------------|--------|--------|
| Pricing defaults | On-prem pricing | `IPricingSettingsService` | Get/Set pricing defaults | ✅ |
| Node defaults | Default node specs | `IPricingSettingsService` | Settings | ✅ |
| Tier configuration | App tier defaults | `ITierConfigurationService` | All tier methods | ✅ |
| Auth providers | OAuth config | Auth services | OAuth configuration | ✅ |
| LDAP config | AD/LDAP settings | `ILdapAuthenticationService` | LDAP configuration | ✅ |

---

### 9. Authentication Pages (15-login, 16-register)

| Wireframe Element | Required Data | Backend Service | Method | Status |
|-------------------|---------------|-----------------|--------|--------|
| Login form | Username/password | `IAuthService` | Login method | ✅ |
| Register form | New user creation | `IAuthService` | Register method | ✅ |
| Social login | Google, Microsoft | `IExternalAuthenticationService` | OAuth providers | ✅ |
| Password strength | Validation rules | `IAuthService` | Password validation | ✅ |
| Remember me | Session persistence | `IAuthService` | Cookie settings | ✅ |

---

### 10. Error Pages (21-404, 22-500, 23-access-denied)

| Wireframe Element | Required Data | Backend Service | Status |
|-------------------|---------------|-----------------|--------|
| 404 Not Found | Navigation | No service needed | ✅ |
| 500 Server Error | Error display | No service needed | ✅ |
| 403 Access Denied | Permission display | `IAuthService` for user info | ✅ |

---

## Identified Gaps

### Gap 1: Email Sharing (Minor)
- **Wireframe**: 19-share-config.html shows "Email Invite" feature
- **Backend**: `ConfigurationSharingService` only supports URL sharing, not email
- **Impact**: Low - URL sharing works, email is convenience feature
- **Recommendation**: Add email service integration if needed

### Gap 2: Permission-Based Sharing (Minor)
- **Wireframe**: 19-share-config.html shows "Can view" / "Can edit" permissions
- **Backend**: No access control on shared configs
- **Impact**: Low - all shared links are read-only
- **Recommendation**: Implement role-based sharing if multi-user collaboration needed

### Gap 3: Link Expiration (Minor)
- **Wireframe**: 19-share-config.html shows expiration options (1/7/30/90 days)
- **Backend**: Links are permanent (no expiration)
- **Impact**: Low - permanent links work fine
- **Recommendation**: Add expiration if security concern

---

## Summary: No Critical Gaps

**All core functionality shown in wireframes v0.4.4 has corresponding backend service support:**

| Category | Services Available |
|----------|-------------------|
| K8s Sizing | `IK8sSizingService` - full calculation support |
| VM Sizing | `IVMSizingService` - full calculation support |
| Distributions | `IDistributionService` - 46 distributions |
| Technologies | `ITechnologyService` - 7 platforms |
| Cost Estimation | `ICostEstimationService` - full TCO support |
| Pricing | `IPricingService` - multi-cloud pricing |
| Growth Planning | `IGrowthPlanningService` - projections + recommendations |
| Export | `IExportService` - Excel, PDF, JSON, CSV |
| Scenarios | `IScenarioService` - full CRUD + compare |
| Share | `ConfigurationSharingService` - URL sharing |
| State | `IAppStateService`, `IWizardStateService` - full state management |
| Tier Config | `ITierConfigurationService` - resource configuration |
| Auth | `IAuthService` + OAuth + LDAP services |

**The only gaps are minor convenience features in sharing (email, permissions, expiration) that don't affect core functionality.**

---

## Recommendation

The V4 Blazor UI code should be rewritten to:

1. **Inject all required services** into Dashboard and panel components
2. **Wire up UI events** to service method calls
3. **Display service results** in the wireframe-specified layouts
4. **Follow wireframe state patterns** (5 tabs, slide panels, summary cards)

The backend is **fully capable** of supporting all wireframe features - the gap is purely in the UI wiring, not backend functionality.
