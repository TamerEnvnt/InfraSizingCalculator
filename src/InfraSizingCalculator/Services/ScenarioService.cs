using System.Text.Json;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for scenario management and comparison
/// </summary>
public class ScenarioService : IScenarioService
{
    private readonly IScenarioRepository _repository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ScenarioService(IScenarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Scenario> SaveK8sScenarioAsync(
        string name,
        K8sSizingInput input,
        K8sSizingResult result,
        CostEstimate? costEstimate = null,
        string? description = null,
        List<string>? tags = null,
        bool isDraft = false)
    {
        var scenario = new Scenario
        {
            Name = name,
            Description = description,
            Type = "k8s",
            K8sInput = input,
            K8sResult = result,
            CostEstimate = costEstimate,
            Tags = tags ?? new List<string>(),
            IsDraft = isDraft
        };

        return await _repository.SaveAsync(scenario);
    }

    public async Task<Scenario> SaveVMScenarioAsync(
        string name,
        VMSizingInput input,
        VMSizingResult result,
        CostEstimate? costEstimate = null,
        string? description = null,
        List<string>? tags = null,
        bool isDraft = false)
    {
        var scenario = new Scenario
        {
            Name = name,
            Description = description,
            Type = "vm",
            VMInput = input,
            VMResult = result,
            CostEstimate = costEstimate,
            Tags = tags ?? new List<string>(),
            IsDraft = isDraft
        };

        return await _repository.SaveAsync(scenario);
    }

    public async Task<Scenario> UpdateScenarioAsync(Scenario scenario)
    {
        scenario.ModifiedAt = DateTime.UtcNow;
        return await _repository.SaveAsync(scenario);
    }

    public Task<List<Scenario>> GetAllScenariosAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<List<ScenarioSummary>> GetScenarioSummariesAsync()
    {
        return _repository.GetSummariesAsync();
    }

    public Task<Scenario?> GetScenarioAsync(Guid id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> DeleteScenarioAsync(Guid id)
    {
        return _repository.DeleteAsync(id);
    }

    public async Task<bool> ToggleFavoriteAsync(Guid id)
    {
        var scenario = await _repository.GetByIdAsync(id);
        if (scenario == null) return false;

        scenario.IsFavorite = !scenario.IsFavorite;
        scenario.ModifiedAt = DateTime.UtcNow;
        await _repository.SaveAsync(scenario);
        return scenario.IsFavorite;
    }

    public ScenarioComparison Compare(params Scenario[] scenarios)
    {
        if (scenarios.Length < 2)
        {
            return new ScenarioComparison
            {
                Scenarios = scenarios,
                Insights = new List<string> { "At least 2 scenarios are needed for comparison" }
            };
        }

        var comparison = new ScenarioComparison
        {
            Scenarios = scenarios,
            Metrics = GenerateMetrics(scenarios),
            Insights = GenerateInsights(scenarios)
        };

        // Find recommended scenario (lowest cost) - only if there's a meaningful difference
        var withCost = scenarios.Where(s => s.CostEstimate != null).ToArray();
        if (withCost.Length >= 2)
        {
            var sorted = withCost.OrderBy(s => s.MonthlyEstimate).ToArray();
            var cheapest = sorted.First();
            var mostExpensive = sorted.Last();

            // Only recommend if there's more than 1% difference in cost
            var costDifference = mostExpensive.MonthlyEstimate > 0
                ? (mostExpensive.MonthlyEstimate - cheapest.MonthlyEstimate) / mostExpensive.MonthlyEstimate * 100
                : 0;

            if (costDifference > 1m)
            {
                comparison.RecommendedScenarioId = cheapest.Id;
                comparison.RecommendationReason = $"Lowest monthly cost at {cheapest.MonthlyEstimate:C0} ({costDifference:F0}% savings)";
            }
            else if (costDifference < 0.1m)
            {
                // Scenarios are essentially identical
                comparison.RecommendationReason = "Scenarios have equivalent costs - choose based on other factors";
            }
        }

        return comparison;
    }

    public async Task<ScenarioComparison> CompareByIdsAsync(params Guid[] ids)
    {
        var scenarios = new List<Scenario>();
        foreach (var id in ids)
        {
            var scenario = await _repository.GetByIdAsync(id);
            if (scenario != null)
                scenarios.Add(scenario);
        }

        return Compare(scenarios.ToArray());
    }

    public async Task<Scenario> DuplicateScenarioAsync(Guid id, string newName)
    {
        var original = await _repository.GetByIdAsync(id);
        if (original == null)
            throw new ArgumentException($"Scenario with ID {id} not found");

        var duplicate = new Scenario
        {
            Name = newName,
            Description = original.Description,
            Type = original.Type,
            K8sInput = original.K8sInput,
            VMInput = original.VMInput,
            K8sResult = original.K8sResult,
            VMResult = original.VMResult,
            CostEstimate = original.CostEstimate,
            Tags = new List<string>(original.Tags)
        };

        return await _repository.SaveAsync(duplicate);
    }

    public Task<int> GetScenarioCountAsync()
    {
        return _repository.CountAsync();
    }

    public Task<List<ScenarioSummary>> SearchScenariosAsync(string query)
    {
        return _repository.SearchAsync(query);
    }

    public async Task<string> ExportToJsonAsync(params Guid[] ids)
    {
        var scenarios = new List<Scenario>();
        foreach (var id in ids)
        {
            var scenario = await _repository.GetByIdAsync(id);
            if (scenario != null)
                scenarios.Add(scenario);
        }

        return JsonSerializer.Serialize(scenarios, JsonOptions);
    }

    public async Task<List<Scenario>> ImportFromJsonAsync(string json)
    {
        var imported = JsonSerializer.Deserialize<List<Scenario>>(json, JsonOptions) ?? new List<Scenario>();
        var saved = new List<Scenario>();

        foreach (var scenario in imported)
        {
            // Generate new IDs to avoid conflicts
            scenario.Id = Guid.NewGuid();
            scenario.CreatedAt = DateTime.UtcNow;
            scenario.ModifiedAt = null;

            var savedScenario = await _repository.SaveAsync(scenario);
            saved.Add(savedScenario);
        }

        return saved;
    }

    private List<ComparisonMetric> GenerateMetrics(Scenario[] scenarios)
    {
        var metrics = new List<ComparisonMetric>();

        // Resource metrics
        metrics.Add(CreateMetric("Total Nodes/VMs", "nodes", "Resources", scenarios, s => s.TotalNodes, true));
        metrics.Add(CreateMetric("Total vCPU", "cores", "Resources", scenarios, s => s.TotalCpu, true));
        metrics.Add(CreateMetric("Total RAM", "GB", "Resources", scenarios, s => (decimal)s.TotalRam, true));

        // Cost metrics
        metrics.Add(CreateMetric("Monthly Cost", "$", "Cost", scenarios, s => s.MonthlyEstimate, true));
        metrics.Add(CreateMetric("Yearly Cost", "$", "Cost", scenarios, s => s.CostEstimate?.YearlyTotal ?? 0, true));
        metrics.Add(CreateMetric("3-Year TCO", "$", "Cost", scenarios, s => s.CostEstimate?.ThreeYearTCO ?? 0, true));

        // Efficiency metrics (cost per resource - lower is better)
        var withCost = scenarios.Where(s => s.CostEstimate != null && s.TotalNodes > 0).ToArray();
        if (withCost.Any())
        {
            metrics.Add(CreateMetric("Cost/Node", "$/node", "Efficiency", scenarios,
                s => s.TotalNodes > 0 && s.CostEstimate != null ? s.MonthlyEstimate / s.TotalNodes : 0, true));
        }

        return metrics;
    }

    private ComparisonMetric CreateMetric(
        string name,
        string unit,
        string category,
        Scenario[] scenarios,
        Func<Scenario, decimal> valueSelector,
        bool lowerIsBetter)
    {
        var values = scenarios.ToDictionary(s => s.Id, valueSelector);
        var nonZeroValues = values.Where(v => v.Value > 0).ToList();

        Guid? winnerId = null;
        decimal delta = 0;

        if (nonZeroValues.Count > 0)
        {
            var best = lowerIsBetter
                ? nonZeroValues.MinBy(v => v.Value)
                : nonZeroValues.MaxBy(v => v.Value);

            var worst = lowerIsBetter
                ? nonZeroValues.MaxBy(v => v.Value)
                : nonZeroValues.MinBy(v => v.Value);

            // Only set a winner if there's an actual difference (more than 0.1% difference)
            if (best.Value > 0)
            {
                delta = Math.Abs((worst.Value - best.Value) / best.Value) * 100;

                // Only mark a winner if there's a meaningful difference
                if (delta > 0.1m) // More than 0.1% difference
                {
                    winnerId = best.Key;
                }
            }
        }

        return new ComparisonMetric
        {
            Name = name,
            Unit = unit,
            Category = category,
            Values = values,
            WinnerId = winnerId,
            LowerIsBetter = lowerIsBetter,
            DeltaPercentage = delta
        };
    }

    private List<string> GenerateInsights(Scenario[] scenarios)
    {
        var insights = new List<string>();

        // Cost comparison
        var withCost = scenarios.Where(s => s.CostEstimate != null).ToArray();
        if (withCost.Length >= 2)
        {
            var cheapest = withCost.OrderBy(s => s.MonthlyEstimate).First();
            var mostExpensive = withCost.OrderByDescending(s => s.MonthlyEstimate).First();
            var savings = mostExpensive.MonthlyEstimate - cheapest.MonthlyEstimate;
            var savingsPercent = mostExpensive.MonthlyEstimate > 0
                ? (savings / mostExpensive.MonthlyEstimate) * 100
                : 0;

            if (savings > 0)
            {
                insights.Add($"\"{cheapest.Name}\" is {savingsPercent:F0}% cheaper than \"{mostExpensive.Name}\" (saves {savings:C0}/month)");
            }
        }

        // Resource comparison
        if (scenarios.Length >= 2)
        {
            var minNodes = scenarios.Min(s => s.TotalNodes);
            var maxNodes = scenarios.Max(s => s.TotalNodes);

            if (maxNodes > minNodes * 1.5m)
            {
                insights.Add($"Significant resource variation: scenarios range from {minNodes} to {maxNodes} nodes");
            }
        }

        // K8s vs VM comparison
        var k8sScenarios = scenarios.Where(s => s.Type == "k8s").ToArray();
        var vmScenarios = scenarios.Where(s => s.Type == "vm").ToArray();

        if (k8sScenarios.Any() && vmScenarios.Any())
        {
            var avgK8sCost = k8sScenarios.Average(s => (double)s.MonthlyEstimate);
            var avgVmCost = vmScenarios.Average(s => (double)s.MonthlyEstimate);

            if (avgK8sCost > 0 && avgVmCost > 0)
            {
                var cheaper = avgK8sCost < avgVmCost ? "Kubernetes" : "VM";
                insights.Add($"On average, {cheaper} deployment is more cost-effective for your workload");
            }
        }

        // Distribution comparison
        var distributions = scenarios
            .Where(s => s.Type == "k8s")
            .GroupBy(s => s.K8sInput?.Distribution)
            .Where(g => g.Key != null)
            .ToList();

        if (distributions.Count > 1)
        {
            insights.Add($"Comparing {distributions.Count} different Kubernetes distributions");
        }

        return insights;
    }
}
