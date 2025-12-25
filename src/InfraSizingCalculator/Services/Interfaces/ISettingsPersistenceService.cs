using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for persisting user settings and configurations via browser localStorage.
/// </summary>
public interface ISettingsPersistenceService
{
    /// <summary>
    /// Save user default settings.
    /// </summary>
    Task SaveUserDefaultsAsync(UserDefaults defaults);

    /// <summary>
    /// Load user default settings.
    /// </summary>
    Task<UserDefaults?> LoadUserDefaultsAsync();

    /// <summary>
    /// Clear user default settings.
    /// </summary>
    Task ClearUserDefaultsAsync();

    /// <summary>
    /// Save a configuration to recent configurations list.
    /// </summary>
    Task SaveRecentConfigurationAsync(SavedConfiguration config);

    /// <summary>
    /// Get list of recent configurations (max 5).
    /// </summary>
    Task<List<SavedConfiguration>> GetRecentConfigurationsAsync();

    /// <summary>
    /// Delete a specific saved configuration.
    /// </summary>
    Task DeleteConfigurationAsync(Guid id);

    /// <summary>
    /// Clear all saved configurations.
    /// </summary>
    Task ClearAllConfigurationsAsync();

    /// <summary>
    /// Clear all persisted data (defaults + configurations).
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// Get the current theme preference.
    /// </summary>
    Task<string> GetThemeAsync();

    /// <summary>
    /// Set the theme preference.
    /// </summary>
    Task SetThemeAsync(string theme);

    /// <summary>
    /// Check if localStorage is available.
    /// </summary>
    Task<bool> IsAvailableAsync();
}
