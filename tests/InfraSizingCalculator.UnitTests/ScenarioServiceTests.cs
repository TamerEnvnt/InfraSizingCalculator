using System.Text.Json;
using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class ScenarioServiceTests
{
    private readonly IScenarioRepository _mockRepository;
    private readonly ScenarioService _service;

    public ScenarioServiceTests()
    {
        _mockRepository = Substitute.For<IScenarioRepository>();
        _service = new ScenarioService(_mockRepository);
    }

    #region SaveK8sScenarioAsync Tests

    [Fact]
    public async Task SaveK8sScenarioAsync_WithRequiredFields_SavesScenario()
    {
        // Arrange
        var name = "Test K8s Scenario";
        var input = CreateK8sSizingInput();
        var result = CreateK8sSizingResult();

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var scenario = await _service.SaveK8sScenarioAsync(name, input, result);

        // Assert
        scenario.Name.Should().Be(name);
        scenario.Type.Should().Be("k8s");
        scenario.K8sInput.Should().Be(input);
        scenario.K8sResult.Should().Be(result);
        scenario.IsDraft.Should().BeFalse();
        await _mockRepository.Received(1).SaveAsync(Arg.Any<Scenario>());
    }

    [Fact]
    public async Task SaveK8sScenarioAsync_WithAllOptionalFields_SavesScenario()
    {
        // Arrange
        var name = "Full K8s Scenario";
        var input = CreateK8sSizingInput();
        var result = CreateK8sSizingResult();
        var costEstimate = CreateCostEstimate();
        var description = "Test description";
        var tags = new List<string> { "tag1", "tag2" };
        var isDraft = true;

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var scenario = await _service.SaveK8sScenarioAsync(
            name, input, result, costEstimate, description, tags, isDraft);

        // Assert
        scenario.Description.Should().Be(description);
        scenario.CostEstimate.Should().Be(costEstimate);
        scenario.Tags.Should().BeEquivalentTo(tags);
        scenario.IsDraft.Should().BeTrue();
    }

    [Fact]
    public async Task SaveK8sScenarioAsync_WithNullTags_UsesEmptyList()
    {
        // Arrange
        var input = CreateK8sSizingInput();
        var result = CreateK8sSizingResult();

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var scenario = await _service.SaveK8sScenarioAsync("Name", input, result, tags: null);

        // Assert
        scenario.Tags.Should().NotBeNull();
        scenario.Tags.Should().BeEmpty();
    }

    #endregion

    #region SaveVMScenarioAsync Tests

    [Fact]
    public async Task SaveVMScenarioAsync_WithRequiredFields_SavesScenario()
    {
        // Arrange
        var name = "Test VM Scenario";
        var input = CreateVMSizingInput();
        var result = CreateVMSizingResult();

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var scenario = await _service.SaveVMScenarioAsync(name, input, result);

        // Assert
        scenario.Name.Should().Be(name);
        scenario.Type.Should().Be("vm");
        scenario.VMInput.Should().Be(input);
        scenario.VMResult.Should().Be(result);
    }

    [Fact]
    public async Task SaveVMScenarioAsync_WithAllOptionalFields_SavesScenario()
    {
        // Arrange
        var name = "Full VM Scenario";
        var input = CreateVMSizingInput();
        var result = CreateVMSizingResult();
        var costEstimate = CreateCostEstimate();
        var description = "VM description";
        var tags = new List<string> { "vm", "production" };

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var scenario = await _service.SaveVMScenarioAsync(
            name, input, result, costEstimate, description, tags, isDraft: true);

        // Assert
        scenario.Description.Should().Be(description);
        scenario.CostEstimate.Should().Be(costEstimate);
        scenario.Tags.Should().BeEquivalentTo(tags);
        scenario.IsDraft.Should().BeTrue();
    }

    #endregion

    #region UpdateScenarioAsync Tests

    [Fact]
    public async Task UpdateScenarioAsync_UpdatesModifiedAtTimestamp()
    {
        // Arrange
        var scenario = CreateScenario();
        var originalModifiedAt = scenario.ModifiedAt;

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var updated = await _service.UpdateScenarioAsync(scenario);

        // Assert
        updated.ModifiedAt.Should().NotBeNull();
        updated.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateScenarioAsync_CallsRepositorySave()
    {
        // Arrange
        var scenario = CreateScenario();

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        await _service.UpdateScenarioAsync(scenario);

        // Assert
        await _mockRepository.Received(1).SaveAsync(Arg.Is<Scenario>(s => s.Id == scenario.Id));
    }

    #endregion

    #region GetAllScenariosAsync Tests

    [Fact]
    public async Task GetAllScenariosAsync_ReturnsAllScenarios()
    {
        // Arrange
        var scenarios = new List<Scenario>
        {
            CreateScenario("Scenario 1"),
            CreateScenario("Scenario 2"),
            CreateScenario("Scenario 3")
        };
        _mockRepository.GetAllAsync().Returns(Task.FromResult(scenarios));

        // Act
        var result = await _service.GetAllScenariosAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllScenariosAsync_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.GetAllAsync().Returns(Task.FromResult(new List<Scenario>()));

        // Act
        var result = await _service.GetAllScenariosAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetScenarioSummariesAsync Tests

    [Fact]
    public async Task GetScenarioSummariesAsync_ReturnsSummaries()
    {
        // Arrange
        var summaries = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Summary 1" },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Summary 2" }
        };
        _mockRepository.GetSummariesAsync().Returns(Task.FromResult(summaries));

        // Act
        var result = await _service.GetScenarioSummariesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetScenarioAsync Tests

    [Fact]
    public async Task GetScenarioAsync_ExistingId_ReturnsScenario()
    {
        // Arrange
        var scenario = CreateScenario();
        _mockRepository.GetByIdAsync(scenario.Id).Returns(Task.FromResult<Scenario?>(scenario));

        // Act
        var result = await _service.GetScenarioAsync(scenario.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(scenario.Id);
    }

    [Fact]
    public async Task GetScenarioAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Scenario?>(null));

        // Act
        var result = await _service.GetScenarioAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteScenarioAsync Tests

    [Fact]
    public async Task DeleteScenarioAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.DeleteAsync(id).Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeleteScenarioAsync(id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteScenarioAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.DeleteAsync(id).Returns(Task.FromResult(false));

        // Act
        var result = await _service.DeleteScenarioAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ToggleFavoriteAsync Tests

    [Fact]
    public async Task ToggleFavoriteAsync_NotFavorite_BecomeFavorite()
    {
        // Arrange
        var scenario = CreateScenario();
        scenario.IsFavorite = false;
        _mockRepository.GetByIdAsync(scenario.Id).Returns(Task.FromResult<Scenario?>(scenario));
        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var result = await _service.ToggleFavoriteAsync(scenario.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleFavoriteAsync_IsFavorite_BecomeNotFavorite()
    {
        // Arrange
        var scenario = CreateScenario();
        scenario.IsFavorite = true;
        _mockRepository.GetByIdAsync(scenario.Id).Returns(Task.FromResult<Scenario?>(scenario));
        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var result = await _service.ToggleFavoriteAsync(scenario.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleFavoriteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.GetByIdAsync(id).Returns(Task.FromResult<Scenario?>(null));

        // Act
        var result = await _service.ToggleFavoriteAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleFavoriteAsync_UpdatesModifiedAt()
    {
        // Arrange
        var scenario = CreateScenario();
        scenario.ModifiedAt = null;
        _mockRepository.GetByIdAsync(scenario.Id).Returns(Task.FromResult<Scenario?>(scenario));
        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        await _service.ToggleFavoriteAsync(scenario.Id);

        // Assert
        await _mockRepository.Received(1).SaveAsync(Arg.Is<Scenario>(s =>
            s.ModifiedAt != null &&
            s.ModifiedAt.Value >= DateTime.UtcNow.AddSeconds(-5)));
    }

    #endregion

    #region Compare Tests

    [Fact]
    public void Compare_SingleScenario_ReturnsComparisonWithMessage()
    {
        // Arrange
        var scenario = CreateScenario();

        // Act
        var comparison = _service.Compare(scenario);

        // Assert
        comparison.Scenarios.Should().HaveCount(1);
        comparison.Insights.Should().Contain("At least 2 scenarios are needed for comparison");
    }

    [Fact]
    public void Compare_TwoScenarios_ReturnsValidComparison()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 10000m);

        // Act
        var comparison = _service.Compare(scenario1, scenario2);

        // Assert
        comparison.Scenarios.Should().HaveCount(2);
        comparison.Metrics.Should().NotBeEmpty();
    }

    [Fact]
    public void Compare_CheaperScenario_IsRecommended()
    {
        // Arrange
        var cheapScenario = CreateScenarioWithCost("Cheap", 3000m);
        var expensiveScenario = CreateScenarioWithCost("Expensive", 10000m);

        // Act
        var comparison = _service.Compare(cheapScenario, expensiveScenario);

        // Assert
        comparison.RecommendedScenarioId.Should().Be(cheapScenario.Id);
        comparison.RecommendationReason.Should().Contain("Lowest monthly cost");
    }

    [Fact]
    public void Compare_SimilarCosts_NoRecommendation()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 5000m);

        // Act
        var comparison = _service.Compare(scenario1, scenario2);

        // Assert
        comparison.RecommendedScenarioId.Should().BeNull();
    }

    [Fact]
    public void Compare_GeneratesResourceMetrics()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 7500m);

        // Act
        var comparison = _service.Compare(scenario1, scenario2);

        // Assert
        comparison.Metrics.Should().Contain(m => m.Name == "Total Nodes/VMs");
        comparison.Metrics.Should().Contain(m => m.Name == "Total vCPU");
        comparison.Metrics.Should().Contain(m => m.Name == "Total RAM");
    }

    [Fact]
    public void Compare_GeneratesCostMetrics()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 7500m);

        // Act
        var comparison = _service.Compare(scenario1, scenario2);

        // Assert
        comparison.Metrics.Should().Contain(m => m.Name == "Monthly Cost");
        comparison.Metrics.Should().Contain(m => m.Name == "Yearly Cost");
        comparison.Metrics.Should().Contain(m => m.Name == "3-Year TCO");
    }

    [Fact]
    public void Compare_GeneratesInsights()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 10000m);

        // Act
        var comparison = _service.Compare(scenario1, scenario2);

        // Assert
        comparison.Insights.Should().NotBeEmpty();
        comparison.Insights.Should().Contain(i => i.Contains("cheaper"));
    }

    [Fact]
    public void Compare_K8sAndVM_GeneratesTypeInsight()
    {
        // Arrange
        var k8sScenario = CreateScenarioWithCost("K8s", 5000m);
        k8sScenario.Type = "k8s";
        k8sScenario.K8sInput = CreateK8sSizingInput();

        var vmScenario = CreateScenarioWithCost("VM", 7500m);
        vmScenario.Type = "vm";
        vmScenario.VMInput = CreateVMSizingInput();
        vmScenario.K8sInput = null;

        // Act
        var comparison = _service.Compare(k8sScenario, vmScenario);

        // Assert
        comparison.Insights.Should().Contain(i =>
            i.Contains("Kubernetes") || i.Contains("VM"));
    }

    [Fact]
    public void Compare_MetricsHaveWinner_WhenSignificantDifference()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 10000m);

        // Act
        var comparison = _service.Compare(scenario1, scenario2);

        // Assert
        var costMetric = comparison.Metrics.First(m => m.Name == "Monthly Cost");
        costMetric.WinnerId.Should().Be(scenario1.Id);
        costMetric.LowerIsBetter.Should().BeTrue();
    }

    #endregion

    #region CompareByIdsAsync Tests

    [Fact]
    public async Task CompareByIdsAsync_ExistingIds_ReturnsComparison()
    {
        // Arrange
        var scenario1 = CreateScenarioWithCost("Scenario 1", 5000m);
        var scenario2 = CreateScenarioWithCost("Scenario 2", 10000m);

        _mockRepository.GetByIdAsync(scenario1.Id).Returns(Task.FromResult<Scenario?>(scenario1));
        _mockRepository.GetByIdAsync(scenario2.Id).Returns(Task.FromResult<Scenario?>(scenario2));

        // Act
        var comparison = await _service.CompareByIdsAsync(scenario1.Id, scenario2.Id);

        // Assert
        comparison.Scenarios.Should().HaveCount(2);
    }

    [Fact]
    public async Task CompareByIdsAsync_SomeNonExisting_OnlyComparesExisting()
    {
        // Arrange
        var existingScenario = CreateScenario();
        var nonExistingId = Guid.NewGuid();

        _mockRepository.GetByIdAsync(existingScenario.Id).Returns(Task.FromResult<Scenario?>(existingScenario));
        _mockRepository.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Scenario?>(null));

        // Act
        var comparison = await _service.CompareByIdsAsync(existingScenario.Id, nonExistingId);

        // Assert
        comparison.Scenarios.Should().HaveCount(1);
    }

    #endregion

    #region DuplicateScenarioAsync Tests

    [Fact]
    public async Task DuplicateScenarioAsync_ExistingScenario_CreatesDuplicate()
    {
        // Arrange
        var original = CreateScenario("Original");
        original.Description = "Original description";
        original.Tags = new List<string> { "tag1" };

        _mockRepository.GetByIdAsync(original.Id).Returns(Task.FromResult<Scenario?>(original));
        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var duplicate = await _service.DuplicateScenarioAsync(original.Id, "Duplicate");

        // Assert
        duplicate.Name.Should().Be("Duplicate");
        duplicate.Description.Should().Be(original.Description);
        duplicate.Type.Should().Be(original.Type);
        duplicate.Tags.Should().BeEquivalentTo(original.Tags);
        duplicate.Id.Should().NotBe(original.Id);
    }

    [Fact]
    public async Task DuplicateScenarioAsync_NonExisting_ThrowsException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Scenario?>(null));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.DuplicateScenarioAsync(nonExistingId, "New Name"));
    }

    [Fact]
    public async Task DuplicateScenarioAsync_CopiesK8sInputAndResult()
    {
        // Arrange
        var original = CreateScenario("Original");
        original.K8sInput = CreateK8sSizingInput();
        original.K8sResult = CreateK8sSizingResult();

        _mockRepository.GetByIdAsync(original.Id).Returns(Task.FromResult<Scenario?>(original));
        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var duplicate = await _service.DuplicateScenarioAsync(original.Id, "Duplicate");

        // Assert
        duplicate.K8sInput.Should().Be(original.K8sInput);
        duplicate.K8sResult.Should().Be(original.K8sResult);
    }

    #endregion

    #region GetScenarioCountAsync Tests

    [Fact]
    public async Task GetScenarioCountAsync_ReturnsCount()
    {
        // Arrange
        _mockRepository.CountAsync().Returns(Task.FromResult(5));

        // Act
        var count = await _service.GetScenarioCountAsync();

        // Assert
        count.Should().Be(5);
    }

    #endregion

    #region SearchScenariosAsync Tests

    [Fact]
    public async Task SearchScenariosAsync_ReturnsMatchingResults()
    {
        // Arrange
        var summaries = new List<ScenarioSummary>
        {
            new ScenarioSummary { Name = "Production K8s" },
            new ScenarioSummary { Name = "Dev K8s" }
        };
        _mockRepository.SearchAsync("K8s").Returns(Task.FromResult(summaries));

        // Act
        var results = await _service.SearchScenariosAsync("K8s");

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(s => s.Name.Should().Contain("K8s"));
    }

    #endregion

    #region ExportToJsonAsync Tests

    [Fact]
    public async Task ExportToJsonAsync_ExportsScenariosAsJson()
    {
        // Arrange
        var scenario1 = CreateScenario("Scenario 1");
        var scenario2 = CreateScenario("Scenario 2");

        _mockRepository.GetByIdAsync(scenario1.Id).Returns(Task.FromResult<Scenario?>(scenario1));
        _mockRepository.GetByIdAsync(scenario2.Id).Returns(Task.FromResult<Scenario?>(scenario2));

        // Act
        var json = await _service.ExportToJsonAsync(scenario1.Id, scenario2.Id);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Scenario 1");
        json.Should().Contain("Scenario 2");
    }

    [Fact]
    public async Task ExportToJsonAsync_ProducesValidJson()
    {
        // Arrange
        var scenario = CreateScenario("Test");
        _mockRepository.GetByIdAsync(scenario.Id).Returns(Task.FromResult<Scenario?>(scenario));

        // Act
        var json = await _service.ExportToJsonAsync(scenario.Id);

        // Assert
        var action = () => JsonDocument.Parse(json);
        action.Should().NotThrow();
    }

    [Fact]
    public async Task ExportToJsonAsync_NonExistingIds_ExportsEmptyArray()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Scenario?>(null));

        // Act
        var json = await _service.ExportToJsonAsync(nonExistingId);

        // Assert
        json.Should().Be("[]");
    }

    #endregion

    #region ImportFromJsonAsync Tests

    [Fact]
    public async Task ImportFromJsonAsync_ValidJson_ImportsScenarios()
    {
        // Arrange
        var scenarios = new List<Scenario>
        {
            CreateScenario("Imported 1"),
            CreateScenario("Imported 2")
        };
        var json = JsonSerializer.Serialize(scenarios, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var imported = await _service.ImportFromJsonAsync(json);

        // Assert
        imported.Should().HaveCount(2);
        await _mockRepository.Received(2).SaveAsync(Arg.Any<Scenario>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_GeneratesNewIds()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var scenario = new Scenario { Id = originalId, Name = "Test" };
        var json = JsonSerializer.Serialize(new[] { scenario }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var imported = await _service.ImportFromJsonAsync(json);

        // Assert
        imported.Should().HaveCount(1);
        imported[0].Id.Should().NotBe(originalId);
    }

    [Fact]
    public async Task ImportFromJsonAsync_SetsCreatedAtToNow()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddYears(-1);
        var scenario = new Scenario { Name = "Test", CreatedAt = oldDate };
        var json = JsonSerializer.Serialize(new[] { scenario }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var imported = await _service.ImportFromJsonAsync(json);

        // Assert
        imported[0].CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ImportFromJsonAsync_ClearsModifiedAt()
    {
        // Arrange
        var scenario = new Scenario { Name = "Test", ModifiedAt = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(new[] { scenario }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockRepository.SaveAsync(Arg.Any<Scenario>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Scenario>()));

        // Act
        var imported = await _service.ImportFromJsonAsync(json);

        // Assert
        imported[0].ModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task ImportFromJsonAsync_EmptyJson_ReturnsEmptyList()
    {
        // Arrange
        var json = "[]";

        // Act
        var imported = await _service.ImportFromJsonAsync(json);

        // Assert
        imported.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private K8sSizingInput CreateK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = 5, Medium = 5 },
            NonProdApps = new AppConfig { Small = 5, Medium = 5 }
        };
    }

    private K8sSizingResult CreateK8sSizingResult()
    {
        var input = CreateK8sSizingInput();
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 20,
                TotalCpu = 160,
                TotalRam = 320
            },
            Configuration = input
        };
    }

    private VMSizingInput CreateVMSizingInput()
    {
        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod }
        };
    }

    private VMSizingResult CreateVMSizingResult()
    {
        return new VMSizingResult
        {
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 160
            }
        };
    }

    private CostEstimate CreateCostEstimate()
    {
        return new CostEstimate
        {
            MonthlyTotal = 5000m
            // YearlyTotal and ThreeYearTCO are computed properties
        };
    }

    private Scenario CreateScenario(string name = "Test Scenario")
    {
        return new Scenario
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = "k8s",
            K8sInput = CreateK8sSizingInput(),
            K8sResult = CreateK8sSizingResult(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private Scenario CreateScenarioWithCost(string name, decimal monthlyCost)
    {
        var scenario = CreateScenario(name);
        scenario.CostEstimate = new CostEstimate
        {
            MonthlyTotal = monthlyCost
            // YearlyTotal and ThreeYearTCO are computed properties
        };
        return scenario;
    }

    #endregion
}
