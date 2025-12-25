using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service interface for scenario management and comparison
/// </summary>
public interface IScenarioService
{
    /// <summary>
    /// Save a K8s scenario
    /// </summary>
    Task<Scenario> SaveK8sScenarioAsync(
        string name,
        K8sSizingInput input,
        K8sSizingResult result,
        CostEstimate? costEstimate = null,
        string? description = null,
        List<string>? tags = null,
        bool isDraft = false);

    /// <summary>
    /// Save a VM scenario
    /// </summary>
    Task<Scenario> SaveVMScenarioAsync(
        string name,
        VMSizingInput input,
        VMSizingResult result,
        CostEstimate? costEstimate = null,
        string? description = null,
        List<string>? tags = null,
        bool isDraft = false);

    /// <summary>
    /// Update an existing scenario
    /// </summary>
    Task<Scenario> UpdateScenarioAsync(Scenario scenario);

    /// <summary>
    /// Get all scenarios
    /// </summary>
    Task<List<Scenario>> GetAllScenariosAsync();

    /// <summary>
    /// Get scenario summaries for list display
    /// </summary>
    Task<List<ScenarioSummary>> GetScenarioSummariesAsync();

    /// <summary>
    /// Get a scenario by ID
    /// </summary>
    Task<Scenario?> GetScenarioAsync(Guid id);

    /// <summary>
    /// Delete a scenario
    /// </summary>
    Task<bool> DeleteScenarioAsync(Guid id);

    /// <summary>
    /// Toggle favorite status
    /// </summary>
    Task<bool> ToggleFavoriteAsync(Guid id);

    /// <summary>
    /// Compare multiple scenarios
    /// </summary>
    ScenarioComparison Compare(params Scenario[] scenarios);

    /// <summary>
    /// Compare scenarios by IDs
    /// </summary>
    Task<ScenarioComparison> CompareByIdsAsync(params Guid[] ids);

    /// <summary>
    /// Duplicate a scenario with a new name
    /// </summary>
    Task<Scenario> DuplicateScenarioAsync(Guid id, string newName);

    /// <summary>
    /// Get scenario count
    /// </summary>
    Task<int> GetScenarioCountAsync();

    /// <summary>
    /// Search scenarios
    /// </summary>
    Task<List<ScenarioSummary>> SearchScenariosAsync(string query);

    /// <summary>
    /// Export scenarios to JSON
    /// </summary>
    Task<string> ExportToJsonAsync(params Guid[] ids);

    /// <summary>
    /// Import scenarios from JSON
    /// </summary>
    Task<List<Scenario>> ImportFromJsonAsync(string json);
}
