using System.Text.Json;
using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Microsoft.JSInterop;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class SettingsPersistenceServiceTests
{
    private readonly IJSRuntime _mockJsRuntime;
    private readonly SettingsPersistenceService _service;
    private readonly Dictionary<string, string> _storage;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SettingsPersistenceServiceTests()
    {
        _mockJsRuntime = Substitute.For<IJSRuntime>();
        _service = new SettingsPersistenceService(_mockJsRuntime);
        _storage = new Dictionary<string, string>();

        SetupMockStorage();
    }

    private void SetupMockStorage()
    {
        // Mock localStorage.getItem
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(callInfo =>
            {
                var args = callInfo.ArgAt<object[]>(1);
                var key = args[0].ToString();
                var value = _storage.ContainsKey(key!) ? _storage[key!] : null;
                return new ValueTask<string?>(value);
            });

        // Mock localStorage.setItem to capture saved data
        _mockJsRuntime.When(x => x.InvokeVoidAsync("localStorage.setItem", Arg.Any<object[]>()))
            .Do(callInfo =>
            {
                var args = callInfo.ArgAt<object[]>(1);
                var key = args[0].ToString();
                var value = args[1].ToString();
                _storage[key!] = value!;
            });

        // Mock localStorage.removeItem
        _mockJsRuntime.When(x => x.InvokeVoidAsync("localStorage.removeItem", Arg.Any<object[]>()))
            .Do(callInfo =>
            {
                var args = callInfo.ArgAt<object[]>(1);
                var key = args[0].ToString();
                _storage.Remove(key!);
            });
    }

    #region SaveUserDefaultsAsync Tests

    [Fact]
    public async Task SaveUserDefaultsAsync_SavesDefaultsToStorage()
    {
        // Arrange
        var defaults = new UserDefaults
        {
            DefaultPlatform = PlatformType.Native,
            DefaultDeployment = DeploymentModel.Kubernetes,
            DefaultTechnology = Technology.DotNet,
            DefaultDistribution = Distribution.OpenShift,
            Theme = "light"
        };

        // Act
        await _service.SaveUserDefaultsAsync(defaults);

        // Assert
        _storage.Should().ContainKey("infra-sizing-user-defaults");
        var json = _storage["infra-sizing-user-defaults"];
        var saved = JsonSerializer.Deserialize<UserDefaults>(json, JsonOptions);
        saved.Should().NotBeNull();
        saved!.DefaultPlatform.Should().Be(PlatformType.Native);
        saved.DefaultTechnology.Should().Be(Technology.DotNet);
        saved.Theme.Should().Be("light");
    }

    [Fact]
    public async Task SaveUserDefaultsAsync_SetsLastSavedTimestamp()
    {
        // Arrange
        var defaults = new UserDefaults();
        var beforeSave = DateTime.UtcNow;

        // Act
        await _service.SaveUserDefaultsAsync(defaults);

        // Assert
        var json = _storage["infra-sizing-user-defaults"];
        var saved = JsonSerializer.Deserialize<UserDefaults>(json, JsonOptions);
        saved!.LastSaved.Should().BeOnOrAfter(beforeSave.AddSeconds(-1));
        saved.LastSaved.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task SaveUserDefaultsAsync_OverwritesExistingDefaults()
    {
        // Arrange
        var firstDefaults = new UserDefaults { Theme = "dark" };
        await _service.SaveUserDefaultsAsync(firstDefaults);

        var secondDefaults = new UserDefaults { Theme = "light" };

        // Act
        await _service.SaveUserDefaultsAsync(secondDefaults);

        // Assert
        var json = _storage["infra-sizing-user-defaults"];
        var saved = JsonSerializer.Deserialize<UserDefaults>(json, JsonOptions);
        saved!.Theme.Should().Be("light");
    }

    [Fact]
    public async Task SaveUserDefaultsAsync_CallsJSRuntimeSetItem()
    {
        // Arrange
        var defaults = new UserDefaults();

        // Act
        await _service.SaveUserDefaultsAsync(defaults);

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.setItem", Arg.Is<object[]>(args =>
                args.Length == 2 && args[0].ToString() == "infra-sizing-user-defaults"));
    }

    #endregion

    #region LoadUserDefaultsAsync Tests

    [Fact]
    public async Task LoadUserDefaultsAsync_ReturnsStoredDefaults()
    {
        // Arrange
        var defaults = new UserDefaults
        {
            DefaultPlatform = PlatformType.LowCode,
            DefaultTechnology = Technology.Java,
            Theme = "light"
        };
        var json = JsonSerializer.Serialize(defaults, JsonOptions);
        _storage["infra-sizing-user-defaults"] = json;

        // Act
        var result = await _service.LoadUserDefaultsAsync();

        // Assert
        result.Should().NotBeNull();
        result!.DefaultPlatform.Should().Be(PlatformType.LowCode);
        result.DefaultTechnology.Should().Be(Technology.Java);
        result.Theme.Should().Be("light");
    }

    [Fact]
    public async Task LoadUserDefaultsAsync_NoStoredData_ReturnsNull()
    {
        // Act
        var result = await _service.LoadUserDefaultsAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadUserDefaultsAsync_EmptyString_ReturnsNull()
    {
        // Arrange
        _storage["infra-sizing-user-defaults"] = string.Empty;

        // Act
        var result = await _service.LoadUserDefaultsAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadUserDefaultsAsync_InvalidJson_ReturnsNull()
    {
        // Arrange
        _storage["infra-sizing-user-defaults"] = "{ invalid json }";

        // Act
        var result = await _service.LoadUserDefaultsAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadUserDefaultsAsync_JSRuntimeThrows_ReturnsNull()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Throws(new JSException("localStorage not available"));

        // Act
        var result = await _service.LoadUserDefaultsAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ClearUserDefaultsAsync Tests

    [Fact]
    public async Task ClearUserDefaultsAsync_RemovesDefaultsFromStorage()
    {
        // Arrange
        _storage["infra-sizing-user-defaults"] = "{}";

        // Act
        await _service.ClearUserDefaultsAsync();

        // Assert
        _storage.Should().NotContainKey("infra-sizing-user-defaults");
    }

    [Fact]
    public async Task ClearUserDefaultsAsync_CallsJSRuntimeRemoveItem()
    {
        // Act
        await _service.ClearUserDefaultsAsync();

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.removeItem", Arg.Is<object[]>(args =>
                args.Length == 1 && args[0].ToString() == "infra-sizing-user-defaults"));
    }

    #endregion

    #region SaveRecentConfigurationAsync Tests

    [Fact]
    public async Task SaveRecentConfigurationAsync_AddsNewConfigToBeginning()
    {
        // Arrange
        var config = CreateSavedConfiguration("Config 1");

        // Act
        await _service.SaveRecentConfigurationAsync(config);

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().ContainSingle();
        configs[0].Name.Should().Be("Config 1");
    }

    [Fact]
    public async Task SaveRecentConfigurationAsync_AddsToBeginningOfExistingList()
    {
        // Arrange
        var config1 = CreateSavedConfiguration("Config 1");
        var config2 = CreateSavedConfiguration("Config 2");
        await _service.SaveRecentConfigurationAsync(config1);

        // Act
        await _service.SaveRecentConfigurationAsync(config2);

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().HaveCount(2);
        configs[0].Name.Should().Be("Config 2");
        configs[1].Name.Should().Be("Config 1");
    }

    [Fact]
    public async Task SaveRecentConfigurationAsync_RemovesDuplicateById()
    {
        // Arrange
        var config = CreateSavedConfiguration("Original");
        await _service.SaveRecentConfigurationAsync(config);

        config.Name = "Updated";

        // Act
        await _service.SaveRecentConfigurationAsync(config);

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().ContainSingle();
        configs[0].Name.Should().Be("Updated");
    }

    [Fact]
    public async Task SaveRecentConfigurationAsync_MaintainsMaximumOf5Configs()
    {
        // Arrange - Add 5 configs
        for (int i = 1; i <= 5; i++)
        {
            await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration($"Config {i}"));
        }

        // Act - Add 6th config
        var sixthConfig = CreateSavedConfiguration("Config 6");
        await _service.SaveRecentConfigurationAsync(sixthConfig);

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().HaveCount(5);
        configs[0].Name.Should().Be("Config 6");
        configs.Should().NotContain(c => c.Name == "Config 1");
    }

    [Fact]
    public async Task SaveRecentConfigurationAsync_RemovesOldestWhenExceedingLimit()
    {
        // Arrange - Add 5 configs
        var configIds = new List<Guid>();
        for (int i = 1; i <= 5; i++)
        {
            var config = CreateSavedConfiguration($"Config {i}");
            configIds.Add(config.Id);
            await _service.SaveRecentConfigurationAsync(config);
        }

        // Act - Add 6th config
        await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration("Config 6"));

        // Assert - First config should be removed
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().NotContain(c => c.Id == configIds[0]);
    }

    [Fact]
    public async Task SaveRecentConfigurationAsync_MovesUpdatedConfigToBeginning()
    {
        // Arrange
        var config1 = CreateSavedConfiguration("Config 1");
        var config2 = CreateSavedConfiguration("Config 2");
        var config3 = CreateSavedConfiguration("Config 3");
        await _service.SaveRecentConfigurationAsync(config1);
        await _service.SaveRecentConfigurationAsync(config2);
        await _service.SaveRecentConfigurationAsync(config3);

        // Act - Re-save config1 (oldest)
        await _service.SaveRecentConfigurationAsync(config1);

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs[0].Id.Should().Be(config1.Id);
        configs[0].Name.Should().Be("Config 1");
    }

    #endregion

    #region GetRecentConfigurationsAsync Tests

    [Fact]
    public async Task GetRecentConfigurationsAsync_ReturnsEmptyListWhenNoConfigs()
    {
        // Act
        var result = await _service.GetRecentConfigurationsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentConfigurationsAsync_ReturnsStoredConfigurations()
    {
        // Arrange
        await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration("Config 1"));
        await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration("Config 2"));

        // Act
        var result = await _service.GetRecentConfigurationsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentConfigurationsAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        _storage["infra-sizing-recent-configs"] = "{ invalid json }";

        // Act
        var result = await _service.GetRecentConfigurationsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentConfigurationsAsync_NullJson_ReturnsEmptyList()
    {
        // Arrange
        _storage["infra-sizing-recent-configs"] = "null";

        // Act
        var result = await _service.GetRecentConfigurationsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentConfigurationsAsync_JSRuntimeThrows_ReturnsEmptyList()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Throws(new JSException("localStorage not available"));

        // Act
        var result = await _service.GetRecentConfigurationsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region DeleteConfigurationAsync Tests

    [Fact]
    public async Task DeleteConfigurationAsync_RemovesConfigById()
    {
        // Arrange
        var config1 = CreateSavedConfiguration("Config 1");
        var config2 = CreateSavedConfiguration("Config 2");
        await _service.SaveRecentConfigurationAsync(config1);
        await _service.SaveRecentConfigurationAsync(config2);

        // Act
        await _service.DeleteConfigurationAsync(config1.Id);

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().ContainSingle();
        configs[0].Id.Should().Be(config2.Id);
    }

    [Fact]
    public async Task DeleteConfigurationAsync_NonExistentId_DoesNothing()
    {
        // Arrange
        var config = CreateSavedConfiguration("Config 1");
        await _service.SaveRecentConfigurationAsync(config);

        // Act
        await _service.DeleteConfigurationAsync(Guid.NewGuid());

        // Assert
        var configs = await _service.GetRecentConfigurationsAsync();
        configs.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteConfigurationAsync_EmptyList_DoesNotThrow()
    {
        // Act
        var act = async () => await _service.DeleteConfigurationAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteConfigurationAsync_UpdatesStorage()
    {
        // Arrange
        var config = CreateSavedConfiguration("Config 1");
        await _service.SaveRecentConfigurationAsync(config);

        // Act
        await _service.DeleteConfigurationAsync(config.Id);

        // Assert
        await _mockJsRuntime.Received()
            .InvokeVoidAsync("localStorage.setItem", Arg.Is<object[]>(args =>
                args[0].ToString() == "infra-sizing-recent-configs"));
    }

    #endregion

    #region ClearAllConfigurationsAsync Tests

    [Fact]
    public async Task ClearAllConfigurationsAsync_RemovesAllConfigurations()
    {
        // Arrange
        await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration("Config 1"));
        await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration("Config 2"));

        // Act
        await _service.ClearAllConfigurationsAsync();

        // Assert
        _storage.Should().NotContainKey("infra-sizing-recent-configs");
    }

    [Fact]
    public async Task ClearAllConfigurationsAsync_CallsJSRuntimeRemoveItem()
    {
        // Act
        await _service.ClearAllConfigurationsAsync();

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.removeItem", Arg.Is<object[]>(args =>
                args.Length == 1 && args[0].ToString() == "infra-sizing-recent-configs"));
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task ClearAllAsync_RemovesAllData()
    {
        // Arrange
        await _service.SaveUserDefaultsAsync(new UserDefaults());
        await _service.SaveRecentConfigurationAsync(CreateSavedConfiguration("Config"));
        await _service.SetThemeAsync("light");

        // Act
        await _service.ClearAllAsync();

        // Assert
        _storage.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearAllAsync_RemovesUserDefaults()
    {
        // Arrange
        _storage["infra-sizing-user-defaults"] = "{}";

        // Act
        await _service.ClearAllAsync();

        // Assert
        _storage.Should().NotContainKey("infra-sizing-user-defaults");
    }

    [Fact]
    public async Task ClearAllAsync_RemovesConfigurations()
    {
        // Arrange
        _storage["infra-sizing-recent-configs"] = "[]";

        // Act
        await _service.ClearAllAsync();

        // Assert
        _storage.Should().NotContainKey("infra-sizing-recent-configs");
    }

    [Fact]
    public async Task ClearAllAsync_RemovesTheme()
    {
        // Arrange
        _storage["infra-sizing-theme"] = "light";

        // Act
        await _service.ClearAllAsync();

        // Assert
        _storage.Should().NotContainKey("infra-sizing-theme");
    }

    [Fact]
    public async Task ClearAllAsync_CallsAllRemoveMethods()
    {
        // Act
        await _service.ClearAllAsync();

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.removeItem", Arg.Is<object[]>(args =>
                args[0].ToString() == "infra-sizing-user-defaults"));
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.removeItem", Arg.Is<object[]>(args =>
                args[0].ToString() == "infra-sizing-recent-configs"));
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.removeItem", Arg.Is<object[]>(args =>
                args[0].ToString() == "infra-sizing-theme"));
    }

    #endregion

    #region GetThemeAsync Tests

    [Fact]
    public async Task GetThemeAsync_ReturnsStoredTheme()
    {
        // Arrange
        _storage["infra-sizing-theme"] = "light";

        // Act
        var result = await _service.GetThemeAsync();

        // Assert
        result.Should().Be("light");
    }

    [Fact]
    public async Task GetThemeAsync_NoStoredTheme_ReturnsDark()
    {
        // Act
        var result = await _service.GetThemeAsync();

        // Assert
        result.Should().Be("dark");
    }

    [Fact]
    public async Task GetThemeAsync_EmptyString_ReturnsDark()
    {
        // Arrange
        _storage["infra-sizing-theme"] = string.Empty;

        // Act
        var result = await _service.GetThemeAsync();

        // Assert
        result.Should().Be("dark");
    }

    [Fact]
    public async Task GetThemeAsync_JSRuntimeThrows_ReturnsDark()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Throws(new JSException("localStorage not available"));

        // Act
        var result = await _service.GetThemeAsync();

        // Assert
        result.Should().Be("dark");
    }

    #endregion

    #region SetThemeAsync Tests

    [Fact]
    public async Task SetThemeAsync_SavesThemeToStorage()
    {
        // Act
        await _service.SetThemeAsync("light");

        // Assert
        _storage.Should().ContainKey("infra-sizing-theme");
        _storage["infra-sizing-theme"].Should().Be("light");
    }

    [Fact]
    public async Task SetThemeAsync_OverwritesExistingTheme()
    {
        // Arrange
        _storage["infra-sizing-theme"] = "dark";

        // Act
        await _service.SetThemeAsync("light");

        // Assert
        _storage["infra-sizing-theme"].Should().Be("light");
    }

    [Fact]
    public async Task SetThemeAsync_CallsJSRuntimeSetItem()
    {
        // Act
        await _service.SetThemeAsync("custom");

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("localStorage.setItem", Arg.Is<object[]>(args =>
                args.Length == 2 &&
                args[0].ToString() == "infra-sizing-theme" &&
                args[1].ToString() == "custom"));
    }

    #endregion

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_LocalStorageAvailable_ReturnsTrue()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<bool>("localStorageAvailable", Arg.Any<object[]>())
            .Returns(new ValueTask<bool>(true));

        // Act
        var result = await _service.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_LocalStorageNotAvailable_ReturnsFalse()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<bool>("localStorageAvailable", Arg.Any<object[]>())
            .Returns(new ValueTask<bool>(false));

        // Act
        var result = await _service.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_JSRuntimeThrows_ReturnsFalse()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<bool>("localStorageAvailable", Arg.Any<object[]>())
            .Throws(new JSException("Function not found"));

        // Act
        var result = await _service.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_CallsCorrectJSFunction()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<bool>("localStorageAvailable", Arg.Any<object[]>())
            .Returns(new ValueTask<bool>(true));

        // Act
        await _service.IsAvailableAsync();

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeAsync<bool>("localStorageAvailable", Arg.Any<object[]>());
    }

    #endregion

    #region Helper Methods

    private SavedConfiguration CreateSavedConfiguration(string name)
    {
        return new SavedConfiguration
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            DeploymentModel = DeploymentModel.Kubernetes,
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            SavedAt = DateTime.UtcNow,
            Summary = new ConfigurationSummary
            {
                TotalApps = 5,
                TotalNodes = 10,
                TotalCpu = 80,
                TotalRam = 256,
                EnvironmentCount = 3
            }
        };
    }

    #endregion
}
