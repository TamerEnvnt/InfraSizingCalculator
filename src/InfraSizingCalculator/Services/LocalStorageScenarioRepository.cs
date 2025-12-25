using System.Text.Json;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.JSInterop;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Scenario repository implementation using browser localStorage.
/// Can be swapped for database implementation later.
/// </summary>
public class LocalStorageScenarioRepository : IScenarioRepository
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "infra-sizing-scenarios";
    private const int MaxScenarios = 50; // Limit for localStorage

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public LocalStorageScenarioRepository(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<Scenario> SaveAsync(Scenario scenario)
    {
        var scenarios = await LoadScenariosAsync();

        var existingIndex = scenarios.FindIndex(s => s.Id == scenario.Id);
        if (existingIndex >= 0)
        {
            scenario.ModifiedAt = DateTime.UtcNow;
            scenarios[existingIndex] = scenario;
        }
        else
        {
            scenario.CreatedAt = DateTime.UtcNow;
            scenarios.Insert(0, scenario);

            // Enforce max limit - remove oldest non-favorite scenarios
            while (scenarios.Count > MaxScenarios)
            {
                var toRemove = scenarios
                    .OrderBy(s => s.IsFavorite ? 1 : 0)
                    .ThenBy(s => s.CreatedAt)
                    .First();
                scenarios.Remove(toRemove);
            }
        }

        await SaveScenariosAsync(scenarios);
        return scenario;
    }

    public async Task<List<Scenario>> GetAllAsync()
    {
        return await LoadScenariosAsync();
    }

    public async Task<List<ScenarioSummary>> GetSummariesAsync()
    {
        var scenarios = await LoadScenariosAsync();
        return scenarios.Select(ToSummary).ToList();
    }

    public async Task<Scenario?> GetByIdAsync(Guid id)
    {
        var scenarios = await LoadScenariosAsync();
        return scenarios.FirstOrDefault(s => s.Id == id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var scenarios = await LoadScenariosAsync();
        var removed = scenarios.RemoveAll(s => s.Id == id);
        if (removed > 0)
        {
            await SaveScenariosAsync(scenarios);
            return true;
        }
        return false;
    }

    public async Task<int> DeleteManyAsync(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        var scenarios = await LoadScenariosAsync();
        var removed = scenarios.RemoveAll(s => idSet.Contains(s.Id));
        if (removed > 0)
        {
            await SaveScenariosAsync(scenarios);
        }
        return removed;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var scenarios = await LoadScenariosAsync();
        return scenarios.Any(s => s.Id == id);
    }

    public async Task<int> CountAsync()
    {
        var scenarios = await LoadScenariosAsync();
        return scenarios.Count;
    }

    public async Task<List<ScenarioSummary>> SearchAsync(string query)
    {
        var scenarios = await LoadScenariosAsync();
        var lowerQuery = query.ToLowerInvariant();

        return scenarios
            .Where(s =>
                s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                s.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Select(ToSummary)
            .ToList();
    }

    public async Task<List<ScenarioSummary>> GetByTypeAsync(string type)
    {
        var scenarios = await LoadScenariosAsync();
        return scenarios
            .Where(s => s.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .Select(ToSummary)
            .ToList();
    }

    public async Task ClearAllAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }

    private async Task<List<Scenario>> LoadScenariosAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(json))
                return new List<Scenario>();

            return JsonSerializer.Deserialize<List<Scenario>>(json, JsonOptions) ?? new List<Scenario>();
        }
        catch
        {
            return new List<Scenario>();
        }
    }

    private async Task SaveScenariosAsync(List<Scenario> scenarios)
    {
        var json = JsonSerializer.Serialize(scenarios, JsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    private static ScenarioSummary ToSummary(Scenario scenario)
    {
        return new ScenarioSummary
        {
            Id = scenario.Id,
            Name = scenario.Name,
            Type = scenario.Type,
            DistributionOrTechnology = scenario.DistributionOrTechnology,
            TotalNodes = scenario.TotalNodes,
            TotalCpu = scenario.TotalCpu,
            MonthlyEstimate = scenario.MonthlyEstimate,
            CreatedAt = scenario.CreatedAt,
            IsFavorite = scenario.IsFavorite,
            IsDraft = scenario.IsDraft,
            Tags = scenario.Tags
        };
    }
}
