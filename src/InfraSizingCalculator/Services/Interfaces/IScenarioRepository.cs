using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Repository interface for scenario storage.
/// Abstraction allows swapping localStorage for database backend later.
/// </summary>
public interface IScenarioRepository
{
    /// <summary>
    /// Save a scenario (create or update)
    /// </summary>
    Task<Scenario> SaveAsync(Scenario scenario);

    /// <summary>
    /// Get all saved scenarios
    /// </summary>
    Task<List<Scenario>> GetAllAsync();

    /// <summary>
    /// Get scenario summaries for list display (lighter than full scenarios)
    /// </summary>
    Task<List<ScenarioSummary>> GetSummariesAsync();

    /// <summary>
    /// Get a scenario by ID
    /// </summary>
    Task<Scenario?> GetByIdAsync(Guid id);

    /// <summary>
    /// Delete a scenario
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Delete multiple scenarios
    /// </summary>
    Task<int> DeleteManyAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// Check if a scenario exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Get scenario count
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// Search scenarios by name or tags
    /// </summary>
    Task<List<ScenarioSummary>> SearchAsync(string query);

    /// <summary>
    /// Get scenarios by type (k8s or vm)
    /// </summary>
    Task<List<ScenarioSummary>> GetByTypeAsync(string type);

    /// <summary>
    /// Clear all scenarios
    /// </summary>
    Task ClearAllAsync();
}
