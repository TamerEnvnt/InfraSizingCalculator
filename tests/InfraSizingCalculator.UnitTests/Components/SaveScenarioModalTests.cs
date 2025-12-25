using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Modals;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for SaveScenarioModal component
/// </summary>
public class SaveScenarioModalTests : TestContext
{
    private readonly IScenarioService _mockScenarioService;

    public SaveScenarioModalTests()
    {
        _mockScenarioService = Substitute.For<IScenarioService>();
        Services.AddSingleton(_mockScenarioService);
    }

    private K8sSizingInput CreateTestK8sInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.Dev }
        };
    }

    private K8sSizingResult CreateTestK8sResult()
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 15,
                TotalCpu = 120,
                TotalRam = 480,
                TotalDisk = 1000
            },
            Configuration = CreateTestK8sInput()
        };
    }

    private CostEstimate CreateTestCostEstimate()
    {
        return new CostEstimate
        {
            Provider = CloudProvider.AWS,
            MonthlyTotal = 15000m,
            Breakdown = new Dictionary<CostCategory, CostBreakdown>()
        };
    }

    #region Visibility Tests

    [Fact]
    public void SaveScenarioModal_IsHidden_WhenIsVisibleIsFalse()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        var overlay = cut.Find(".modal-overlay");
        overlay.ClassList.Should().NotContain("visible");
    }

    [Fact]
    public void SaveScenarioModal_IsVisible_WhenIsVisibleIsTrue()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var overlay = cut.Find(".modal-overlay");
        overlay.ClassList.Should().Contain("visible");
    }

    [Fact]
    public void SaveScenarioModal_DisplaysTitle()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        cut.Find(".modal-header h2").TextContent.Should().Be("Save Scenario");
    }

    #endregion

    #region Form Fields Tests

    [Fact]
    public void SaveScenarioModal_DisplaysNameInput()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var nameInput = cut.Find("#scenarioName");
        nameInput.Should().NotBeNull();
    }

    [Fact]
    public void SaveScenarioModal_ShowsFieldError_WhenNameIsEmpty()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var error = cut.Find(".field-error");
        error.TextContent.Should().Be("Name is required");
    }

    [Fact]
    public void SaveScenarioModal_HidesFieldError_WhenNameProvided()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Act
        cut.Find("#scenarioName").Input("Test Scenario");

        // Assert
        cut.FindAll(".field-error").Should().BeEmpty();
    }

    #endregion

    #region Scenario Preview Tests

    [Fact]
    public void SaveScenarioModal_DisplaysScenarioPreview()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var preview = cut.Find(".scenario-preview");
        preview.Should().NotBeNull();
        preview.QuerySelector("h4")?.TextContent.Should().Be("Scenario Summary");
    }

    [Fact]
    public void SaveScenarioModal_ShowsK8sType_ForK8sScenario()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var typeValue = cut.FindAll(".preview-value")[0];
        typeValue.TextContent.Should().Be("Kubernetes");
    }

    [Fact]
    public void SaveScenarioModal_ShowsCostEstimate_WhenProvided()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var previewItems = cut.FindAll(".preview-item");
        var costItem = previewItems.FirstOrDefault(p => p.QuerySelector(".preview-label")?.TextContent == "Est. Monthly");
        costItem.Should().NotBeNull();
        costItem!.ClassList.Should().Contain("highlight");
    }

    #endregion

    #region Save Functionality Tests

    [Fact]
    public void SaveScenarioModal_SaveButtonIsDisabled_WhenNameIsEmpty()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var saveButton = cut.Find(".btn-primary");
        saveButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void SaveScenarioModal_SaveButtonIsEnabled_WhenNameProvided()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Act
        cut.Find("#scenarioName").Input("Test Scenario");

        // Assert
        var saveButton = cut.Find(".btn-primary");
        saveButton.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public async Task SaveScenarioModal_CallsScenarioService_WhenSavingK8s()
    {
        // Arrange
        var savedScenario = new Scenario
        {
            Id = Guid.NewGuid(),
            Name = "Test Scenario"
        };

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(),
            Arg.Any<K8sSizingInput>(),
            Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(),
            Arg.Any<string>(),
            Arg.Any<List<string>>(),
            Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test Scenario");

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.Find(".btn-primary").Click();
        });

        // Assert
        await _mockScenarioService.Received(1).SaveK8sScenarioAsync(
            "Test Scenario",
            Arg.Any<K8sSizingInput>(),
            Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(),
            Arg.Any<string>(),
            Arg.Any<List<string>>(),
            false);
    }

    #endregion

    #region Close Functionality Tests

    [Fact]
    public async Task SaveScenarioModal_Closes_WhenCloseButtonClicked()
    {
        // Arrange
        bool visibilityChanged = false;
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.IsVisibleChanged, value => visibilityChanged = !value));

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.Find(".modal-close").Click();
        });

        // Assert
        visibilityChanged.Should().BeTrue();
    }

    [Fact]
    public async Task SaveScenarioModal_Closes_WhenCancelButtonClicked()
    {
        // Arrange
        bool visibilityChanged = false;
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.IsVisibleChanged, value => visibilityChanged = !value));

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.Find(".btn-secondary").Click();
        });

        // Assert
        visibilityChanged.Should().BeTrue();
    }

    #endregion
}
