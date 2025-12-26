# Change Impact Matrix

Quick reference: What code changes require what documentation updates.

---

## How to Use This Matrix

1. Find your change type in the left column
2. Check all required updates in the right columns
3. Update ALL listed docs before committing

---

## Enums (Models/Enums/)

| If you change... | Update these docs | Update these tests |
|------------------|-------------------|-------------------|
| `Distribution.cs` - add value | models.md (Distribution enum), solution-overview.md (count), business-rules.md (BR-D) | DistributionServiceTests |
| `Distribution.cs` - remove value | models.md, solution-overview.md (count), search all docs for references | DistributionServiceTests |
| `Technology.cs` - add value | models.md (Technology enum), solution-overview.md | TechnologyServiceTests |
| `ClusterMode.cs` - add value | models.md (ClusterMode enum), BRD.md (FR-003) | K8sSizingServiceTests |
| `EnvironmentType.cs` - add value | models.md, business-rules.md (BR-E) | All sizing tests |
| `AppTier.cs` - add value | models.md, all tier references | TechnologyServiceTests |
| `HAPattern.cs` - add value | models.md, business-rules.md (BR-VM) | VMSizingServiceTests |
| `DRPattern.cs` - add value | models.md, business-rules.md (BR-VM) | VMSizingServiceTests |

---

## Services (Services/)

| If you change... | Update these docs | Update these tests |
|------------------|-------------------|-------------------|
| Add new service | services.md (full section), solution-overview.md (service count) | New test file |
| Add public method | services.md (method list) | Method-specific tests |
| Change calculation logic | business-rules.md (relevant BR-* rules) | Calculation tests |
| Change default values | models.md (defaults), business-rules.md | Default value tests |

### Service-Specific Updates

| Service | Primary Docs | Business Rules |
|---------|--------------|----------------|
| K8sSizingService | services.md, solution-overview.md | BR-M*, BR-I*, BR-W*, BR-RC* |
| VMSizingService | services.md | BR-VM* |
| TechnologyService | services.md | BR-T* |
| DistributionService | services.md | BR-D* |
| CostEstimationService | services.md | BR-C* |
| ScenarioService | services.md | BR-S* |
| GrowthPlanningService | services.md | BR-G* |
| ExportService | services.md | - |
| PricingService | services.md, PRICING_VALIDATION.md | BR-C* |

---

## Components (Components/)

| If you change... | Update these docs |
|------------------|-------------------|
| Add new page | ui-components.md (Pages section), WIZARD_FLOW.md if wizard-related |
| Add new component | ui-components.md (appropriate section) |
| Add component folder | ui-components.md (folder structure) |
| Change wizard flow | WIZARD_FLOW.md, ui-components.md |
| Add modal | ui-components.md (Modals section) |

### Component Folder Mapping

| Folder | Section in ui-components.md |
|--------|----------------------------|
| Pages/ | Main Pages |
| Layout/ | Layout Components |
| Wizard/ | Wizard Framework |
| Configuration/ | Configuration Components |
| Results/ | Results Display |
| Pricing/ | Pricing Components |
| K8s/ | Kubernetes Configuration |
| VM/ | Virtual Machine Configuration |
| Shared/ | Shared/Reusable Components |
| Modals/ | Modal Dialogs |

---

## Models (Models/)

| If you change... | Update these docs |
|------------------|-------------------|
| Add input model | models.md (Input Models section) |
| Add output model | models.md (Output Models section) |
| Add config model | models.md (Configuration Models section) |
| Change property | models.md (property table) |
| Change validation | models.md, business-rules.md (BR-V*) |
| Change defaults | models.md (Default column) |

---

## API (Controllers/Api/)

| If you change... | Update these docs |
|------------------|-------------------|
| Add endpoint | api-reference.md |
| Change request/response | api-reference.md |
| Add controller | api-reference.md (new section) |

---

## Business Logic Changes

| If you change... | Update these docs |
|------------------|-------------------|
| Calculation formula | business-rules.md (relevant rule), services.md |
| Default values | models.md, business-rules.md |
| Validation rules | business-rules.md (BR-V*) |
| Feature scope | BRD.md (scope section) |
| New business rule | business-rules.md (add new BR-* rule) |

---

## Count Updates

When adding/removing items, update counts in ALL these locations:

### Distribution Count
- `docs/technical/models.md` - "Distribution (X total)"
- `docs/architecture/solution-overview.md` - "X distributions"
- `docs/business/BRD.md` - "X distribution options"
- `docs/srs/chapters/11-appendices.html` - distribution tables

### Service Count
- `docs/architecture/solution-overview.md` - "Services (X total)"
- `docs/technical/services.md` - service table

### Technology Count
- `docs/architecture/solution-overview.md` - "Supported Technologies (X)"

**Quick search for counts:**
```bash
# Find all distribution count mentions
grep -rn "distribution" docs/ | grep -E "[0-9]+ distribution"

# Find service count mentions
grep -rn "service" docs/ | grep -E "[0-9]+ service"
```

---

## Test Requirements

| Code Change | Test Requirement |
|-------------|------------------|
| New enum value | Config/behavior tests for new value |
| New service | Full service test class |
| New public method | Method unit tests |
| Calculation change | Before/after calculation tests |
| New component | bUnit component tests |
| API endpoint | Integration tests |
| Bug fix | Regression test for the bug |

---

## SRS Chapter Mapping

| Feature Area | SRS Chapter |
|--------------|-------------|
| Platform selection | 03-use-cases-platform.html |
| K8s sizing | 04-use-cases-k8s.html |
| VM sizing | 05-use-cases-vm.html |
| Results display | 06-use-cases-results.html |
| Export | 07-use-cases-export.html |
| Scenarios | 08-use-cases-scenarios.html |
| Settings | 09-use-cases-settings.html |

---

## Checklist Generator

For any code change, generate your checklist:

```markdown
## Change: [describe your change]

### Code Files Modified
- [ ] file1.cs
- [ ] file2.cs

### Documentation Updates Required
- [ ] docs/technical/models.md (if enums/models changed)
- [ ] docs/technical/services.md (if services changed)
- [ ] docs/technical/ui-components.md (if components changed)
- [ ] docs/business/business-rules.md (if logic changed)
- [ ] docs/architecture/solution-overview.md (if counts changed)

### Tests Required
- [ ] Unit tests for new/changed methods
- [ ] Integration tests if API changed

### Verification
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] No hardcoded counts are now wrong
```
