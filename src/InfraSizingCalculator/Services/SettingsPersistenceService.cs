using System.Text.Json;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.JSInterop;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for persisting user settings via browser localStorage.
/// </summary>
public class SettingsPersistenceService : ISettingsPersistenceService
{
    private readonly IJSRuntime _jsRuntime;
    private const string UserDefaultsKey = "infra-sizing-user-defaults";
    private const string RecentConfigsKey = "infra-sizing-recent-configs";
    private const string ThemeKey = "infra-sizing-theme";
    private const int MaxRecentConfigs = 5;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SettingsPersistenceService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveUserDefaultsAsync(UserDefaults defaults)
    {
        defaults.LastSaved = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(defaults, JsonOptions);
        await SetItemAsync(UserDefaultsKey, json);
    }

    public async Task<UserDefaults?> LoadUserDefaultsAsync()
    {
        try
        {
            var json = await GetItemAsync(UserDefaultsKey);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<UserDefaults>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearUserDefaultsAsync()
    {
        await RemoveItemAsync(UserDefaultsKey);
    }

    public async Task SaveRecentConfigurationAsync(SavedConfiguration config)
    {
        var configs = await GetRecentConfigurationsAsync();

        // Remove existing config with same ID if present
        configs.RemoveAll(c => c.Id == config.Id);

        // Add new config at the beginning
        configs.Insert(0, config);

        // Keep only the most recent configs
        if (configs.Count > MaxRecentConfigs)
        {
            configs = configs.Take(MaxRecentConfigs).ToList();
        }

        var json = JsonSerializer.Serialize(configs, JsonOptions);
        await SetItemAsync(RecentConfigsKey, json);
    }

    public async Task<List<SavedConfiguration>> GetRecentConfigurationsAsync()
    {
        try
        {
            var json = await GetItemAsync(RecentConfigsKey);
            if (string.IsNullOrEmpty(json))
                return new List<SavedConfiguration>();

            return JsonSerializer.Deserialize<List<SavedConfiguration>>(json, JsonOptions)
                ?? new List<SavedConfiguration>();
        }
        catch
        {
            return new List<SavedConfiguration>();
        }
    }

    public async Task DeleteConfigurationAsync(Guid id)
    {
        var configs = await GetRecentConfigurationsAsync();
        configs.RemoveAll(c => c.Id == id);
        var json = JsonSerializer.Serialize(configs, JsonOptions);
        await SetItemAsync(RecentConfigsKey, json);
    }

    public async Task ClearAllConfigurationsAsync()
    {
        await RemoveItemAsync(RecentConfigsKey);
    }

    public async Task ClearAllAsync()
    {
        await ClearUserDefaultsAsync();
        await ClearAllConfigurationsAsync();
        await RemoveItemAsync(ThemeKey);
    }

    public async Task<string> GetThemeAsync()
    {
        try
        {
            var theme = await GetItemAsync(ThemeKey);
            return string.IsNullOrEmpty(theme) ? "dark" : theme;
        }
        catch
        {
            return "dark";
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        await SetItemAsync(ThemeKey, theme);
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("localStorageAvailable");
        }
        catch
        {
            return false;
        }
    }

    private async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    private async Task<string?> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    private async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
