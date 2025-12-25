using System.Text.Json;
using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class LocalStorageScenarioRepositoryTests
{
    private readonly IJSRuntime _mockJsRuntime;
    private readonly LocalStorageScenarioRepository _repository;
    private List<Scenario> _storedScenarios;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public LocalStorageScenarioRepositoryTests()
    {
        _mockJsRuntime = Substitute.For<IJSRuntime>();
        _repository = new LocalStorageScenarioRepository(_mockJsRuntime);
        _storedScenarios = new List<Scenario>();

        SetupMockStorage();
    }

    private void SetupMockStorage()
    {
        // Mock localStorage.getItem
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(callInfo =>
            {
                var json = _storedScenarios.Any()
                    ? JsonSerializer.Serialize(_storedScenarios, JsonOptions)
                    : null;
                return new ValueTask<string?>(json);
            });

        // Mock localStorage.setItem to capture saved scenarios
        _mockJsRuntime.When(x => x.InvokeVoidAsync("localStorage.setItem", Arg.Any<object[]>()))
            .Do(callInfo =>
            {
                var args = callInfo.ArgAt<object[]>(1);
                if (args.Length >= 2 && args[1] is string json)
                {
                    _storedScenarios = JsonSerializer.Deserialize<List<Scenario>>(json, JsonOptions)
                        ?? new List<Scenario>();
                }
            });
    }

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_NewScenario_AddsToStorage()
    {
        // Arrange
        var scenario = CreateScenario("Test Scenario");

        // Act
        var saved = await _repository.SaveAsync(scenario);

        // Assert
        saved.Should().NotBeNull();
        saved.Id.Should().Be(scenario.Id);
        _storedScenarios.Should().ContainSingle();
    }

    [Fact]
    public async Task SaveAsync_NewScenario_SetsCreatedAt()
    {
        // Arrange
        var scenario = CreateScenario("Test Scenario");
        var beforeSave = DateTime.UtcNow;

        // Act
        var saved = await _repository.SaveAsync(scenario);

        // Assert
        saved.CreatedAt.Should().BeOnOrAfter(beforeSave.AddSeconds(-1));
    }

    [Fact]
    public async Task SaveAsync_ExistingScenario_Updates()
    {
        // Arrange
        var scenario = CreateScenario("Original Name");
        _storedScenarios.Add(scenario);

        scenario.Name = "Updated Name";

        // Act
        var saved = await _repository.SaveAsync(scenario);

        // Assert
        _storedScenarios.Should().ContainSingle();
        _storedScenarios[0].Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task SaveAsync_ExistingScenario_SetsModifiedAt()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        _storedScenarios.Add(scenario);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        scenario.Name = "Updated";
        var saved = await _repository.SaveAsync(scenario);

        // Assert
        saved.ModifiedAt.Should().NotBeNull();
        saved.ModifiedAt.Should().BeOnOrAfter(beforeUpdate.AddSeconds(-1));
    }

    [Fact]
    public async Task SaveAsync_NewScenario_AddsToFront()
    {
        // Arrange
        var oldScenario = CreateScenario("Old");
        _storedScenarios.Add(oldScenario);

        var newScenario = CreateScenario("New");

        // Act
        await _repository.SaveAsync(newScenario);

        // Assert
        _storedScenarios.Should().HaveCount(2);
        _storedScenarios[0].Name.Should().Be("New");
    }

    [Fact]
    public async Task SaveAsync_ExceedsMaxLimit_RemovesOldest()
    {
        // Arrange - Fill with 50 scenarios
        for (int i = 0; i < 50; i++)
        {
            var s = CreateScenario($"Scenario {i}");
            s.CreatedAt = DateTime.UtcNow.AddDays(-i);
            _storedScenarios.Add(s);
        }

        // Act - Add one more
        var newScenario = CreateScenario("New Scenario");
        await _repository.SaveAsync(newScenario);

        // Assert - Still 50, oldest removed
        _storedScenarios.Should().HaveCount(50);
        _storedScenarios.Should().Contain(s => s.Name == "New Scenario");
    }

    [Fact]
    public async Task SaveAsync_ExceedsMaxLimit_KeepsFavorites()
    {
        // Arrange - Fill with 50 scenarios, mark first as favorite
        var favorite = CreateScenario("Favorite");
        favorite.IsFavorite = true;
        favorite.CreatedAt = DateTime.UtcNow.AddDays(-100); // Oldest
        _storedScenarios.Add(favorite);

        for (int i = 1; i < 50; i++)
        {
            var s = CreateScenario($"Scenario {i}");
            s.CreatedAt = DateTime.UtcNow.AddDays(-i);
            _storedScenarios.Add(s);
        }

        // Act - Add one more
        var newScenario = CreateScenario("New Scenario");
        await _repository.SaveAsync(newScenario);

        // Assert - Favorite still present
        _storedScenarios.Should().Contain(s => s.Name == "Favorite");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllScenarios()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("Scenario 1"));
        _storedScenarios.Add(CreateScenario("Scenario 2"));
        _storedScenarios.Add(CreateScenario("Scenario 3"));

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_EmptyStorage_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_StorageException_ReturnsEmptyList()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns<string?>(_ => throw new InvalidOperationException("JS not available"));

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetSummariesAsync Tests

    [Fact]
    public async Task GetSummariesAsync_ReturnsSummaries()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("Scenario 1"));
        _storedScenarios.Add(CreateScenario("Scenario 2"));

        // Act
        var result = await _repository.GetSummariesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<ScenarioSummary>();
    }

    [Fact]
    public async Task GetSummariesAsync_SummaryContainsCorrectData()
    {
        // Arrange
        var scenario = CreateScenario("Test Scenario");
        scenario.IsFavorite = true;
        scenario.Tags = new List<string> { "tag1", "tag2" };
        _storedScenarios.Add(scenario);

        // Act
        var result = await _repository.GetSummariesAsync();

        // Assert
        var summary = result.First();
        summary.Name.Should().Be("Test Scenario");
        summary.IsFavorite.Should().BeTrue();
        summary.Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2" });
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsScenario()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        _storedScenarios.Add(scenario);

        // Act
        var result = await _repository.GetByIdAsync(scenario.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(scenario.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("Test"));

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesAndReturnsTrue()
    {
        // Arrange
        var scenario = CreateScenario("To Delete");
        _storedScenarios.Add(scenario);

        // Act
        var result = await _repository.DeleteAsync(scenario.Id);

        // Assert
        result.Should().BeTrue();
        _storedScenarios.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("Keep Me"));

        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _storedScenarios.Should().HaveCount(1);
    }

    #endregion

    #region DeleteManyAsync Tests

    [Fact]
    public async Task DeleteManyAsync_ExistingIds_RemovesAllAndReturnsCount()
    {
        // Arrange
        var scenario1 = CreateScenario("Scenario 1");
        var scenario2 = CreateScenario("Scenario 2");
        var scenario3 = CreateScenario("Scenario 3");
        _storedScenarios.AddRange(new[] { scenario1, scenario2, scenario3 });

        // Act
        var result = await _repository.DeleteManyAsync(new[] { scenario1.Id, scenario3.Id });

        // Assert
        result.Should().Be(2);
        _storedScenarios.Should().ContainSingle();
        _storedScenarios[0].Name.Should().Be("Scenario 2");
    }

    [Fact]
    public async Task DeleteManyAsync_SomeNonExisting_RemovesOnlyExisting()
    {
        // Arrange
        var scenario = CreateScenario("Scenario");
        _storedScenarios.Add(scenario);

        // Act
        var result = await _repository.DeleteManyAsync(new[] { scenario.Id, Guid.NewGuid() });

        // Assert
        result.Should().Be(1);
        _storedScenarios.Should().BeEmpty();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        _storedScenarios.Add(scenario);

        // Act
        var result = await _repository.ExistsAsync(scenario.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("1"));
        _storedScenarios.Add(CreateScenario("2"));
        _storedScenarios.Add(CreateScenario("3"));

        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_EmptyStorage_ReturnsZero()
    {
        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_MatchingName_ReturnsResults()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("Production K8s"));
        _storedScenarios.Add(CreateScenario("Development K8s"));
        _storedScenarios.Add(CreateScenario("VM Setup"));

        // Act
        var result = await _repository.SearchAsync("K8s");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_MatchingDescription_ReturnsResults()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        scenario.Description = "This is a Kubernetes configuration";
        _storedScenarios.Add(scenario);
        _storedScenarios.Add(CreateScenario("Other"));

        // Act
        var result = await _repository.SearchAsync("Kubernetes");

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_MatchingTag_ReturnsResults()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        scenario.Tags = new List<string> { "production", "enterprise" };
        _storedScenarios.Add(scenario);
        _storedScenarios.Add(CreateScenario("Other"));

        // Act
        var result = await _repository.SearchAsync("enterprise");

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_CaseInsensitive_ReturnsResults()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("PRODUCTION"));

        // Act
        var result = await _repository.SearchAsync("production");

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        // Arrange
        _storedScenarios.Add(CreateScenario("Test"));

        // Act
        var result = await _repository.SearchAsync("NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByTypeAsync Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsMatchingType()
    {
        // Arrange
        var k8s = CreateScenario("K8s Scenario");
        k8s.Type = "k8s";

        var vm = CreateScenario("VM Scenario");
        vm.Type = "vm";

        _storedScenarios.AddRange(new[] { k8s, vm });

        // Act
        var result = await _repository.GetByTypeAsync("k8s");

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("K8s Scenario");
    }

    [Fact]
    public async Task GetByTypeAsync_CaseInsensitive()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        scenario.Type = "K8S";
        _storedScenarios.Add(scenario);

        // Act
        var result = await _repository.GetByTypeAsync("k8s");

        // Assert
        result.Should().ContainSingle();
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task ClearAllAsync_RemovesFromStorage()
    {
        // Act
        await _repository.ClearAllAsync();

        // Assert
        await _mockJsRuntime.Received()
            .InvokeVoidAsync("localStorage.removeItem", Arg.Is<object[]>(args =>
                args[0].ToString() == "infra-sizing-scenarios"));
    }

    #endregion

    #region Helper Methods

    private Scenario CreateScenario(string name)
    {
        return new Scenario
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = "k8s",
            CreatedAt = DateTime.UtcNow,
            K8sResult = new K8sSizingResult
            {
                Environments = new List<EnvironmentResult>(),
                Configuration = new K8sSizingInput
                {
                    Distribution = Distribution.OpenShift,
                    Technology = Technology.DotNet,
                    ClusterMode = ClusterMode.MultiCluster
                },
                GrandTotal = new GrandTotal { TotalNodes = 10, TotalCpu = 80 }
            }
        };
    }

    #endregion
}
