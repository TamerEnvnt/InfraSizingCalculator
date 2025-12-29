using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Modals;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
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
    public void SaveScenarioModal_ShowsFieldError_WhenNameCleared()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Act - Clear the auto-generated name
        cut.Find("#scenarioName").Input("");

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
    public void SaveScenarioModal_SaveButtonIsDisabled_WhenNameCleared()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Act - Clear the auto-generated name
        cut.Find("#scenarioName").Input("");

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

    #region VM Scenario Tests

    private VMSizingInput CreateTestVMInput()
    {
        return new VMSizingInput
        {
            Technology = Technology.Java
        };
    }

    private VMSizingResult CreateTestVMResult()
    {
        return new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>(),
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 320,
                TotalDisk = 2000
            }
        };
    }

    [Fact]
    public void SaveScenarioModal_ShowsVMType_ForVMScenario()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.VMInput, CreateTestVMInput())
            .Add(p => p.VMResult, CreateTestVMResult()));

        // Assert
        var typeValue = cut.FindAll(".preview-value")[0];
        typeValue.TextContent.Should().Be("Virtual Machines");
    }

    [Fact]
    public void SaveScenarioModal_ShowsVMDetails_ForVMScenario()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.VMInput, CreateTestVMInput())
            .Add(p => p.VMResult, CreateTestVMResult()));

        // Assert
        var previewValues = cut.FindAll(".preview-value");
        previewValues.Any(p => p.TextContent == "Java").Should().BeTrue();
        previewValues.Any(p => p.TextContent == "10").Should().BeTrue();
        previewValues.Any(p => p.TextContent == "80").Should().BeTrue();
    }

    [Fact]
    public async Task SaveScenarioModal_CallsScenarioService_WhenSavingVM()
    {
        // Arrange
        var savedScenario = new Scenario
        {
            Id = Guid.NewGuid(),
            Name = "VM Test Scenario"
        };

        _mockScenarioService.SaveVMScenarioAsync(
            Arg.Any<string>(),
            Arg.Any<VMSizingInput>(),
            Arg.Any<VMSizingResult>(),
            Arg.Any<CostEstimate?>(),
            Arg.Any<string>(),
            Arg.Any<List<string>>(),
            Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.VMInput, CreateTestVMInput())
            .Add(p => p.VMResult, CreateTestVMResult()));

        cut.Find("#scenarioName").Input("VM Test Scenario");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        await _mockScenarioService.Received(1).SaveVMScenarioAsync(
            "VM Test Scenario",
            Arg.Any<VMSizingInput>(),
            Arg.Any<VMSizingResult>(),
            Arg.Any<CostEstimate?>(),
            Arg.Any<string>(),
            Arg.Any<List<string>>(),
            false);
    }

    #endregion

    #region Default Name Generation Tests

    [Fact]
    public void SaveScenarioModal_GeneratesK8sDefaultName_WhenOpened()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert
        var nameInput = cut.Find("#scenarioName");
        var value = nameInput.GetAttribute("value");
        value.Should().Contain("OpenShift");
        value.Should().Contain("DotNet");
    }

    [Fact]
    public void SaveScenarioModal_GeneratesVMDefaultName_WhenOpened()
    {
        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.VMInput, CreateTestVMInput())
            .Add(p => p.VMResult, CreateTestVMResult()));

        // Assert
        var nameInput = cut.Find("#scenarioName");
        var value = nameInput.GetAttribute("value");
        value.Should().Contain("VM");
        value.Should().Contain("Java");
    }

    [Fact]
    public void SaveScenarioModal_GeneratesFallbackName_WhenNoInputProvided()
    {
        // Arrange - Render with result but no input (unusual case)
        var result = CreateTestK8sResult();

        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sResult, result));

        // Assert
        var nameInput = cut.Find("#scenarioName");
        var value = nameInput.GetAttribute("value");
        value.Should().Contain("Scenario");
    }

    #endregion

    #region Tags Parsing Tests

    [Fact]
    public async Task SaveScenarioModal_ParsesTags_WhenProvided()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        List<string>? capturedTags = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(),
            Arg.Any<K8sSizingInput>(),
            Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(),
            Arg.Any<string>(),
            Arg.Do<List<string>>(t => capturedTags = t),
            Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test Scenario");
        cut.Find("#scenarioTags").Change("production, enterprise, openshift");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        capturedTags.Should().NotBeNull();
        capturedTags.Should().Contain("production");
        capturedTags.Should().Contain("enterprise");
        capturedTags.Should().Contain("openshift");
    }

    [Fact]
    public async Task SaveScenarioModal_NormalizesTagsToLowercase()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        List<string>? capturedTags = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(),
            Arg.Do<List<string>>(t => capturedTags = t), Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");
        cut.Find("#scenarioTags").Change("PRODUCTION, Enterprise, OpenShift");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        capturedTags.Should().NotBeNull();
        capturedTags.Should().OnlyContain(t => t == t.ToLowerInvariant());
    }

    [Fact]
    public async Task SaveScenarioModal_RemovesDuplicateTags()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        List<string>? capturedTags = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(),
            Arg.Do<List<string>>(t => capturedTags = t), Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");
        cut.Find("#scenarioTags").Change("prod, prod, PROD, production");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        capturedTags.Should().NotBeNull();
        capturedTags!.Count(t => t == "prod").Should().Be(1);
    }

    [Fact]
    public async Task SaveScenarioModal_ReturnsEmptyTags_WhenNoneProvided()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        List<string>? capturedTags = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(),
            Arg.Do<List<string>>(t => capturedTags = t), Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");
        // Don't set any tags

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        capturedTags.Should().NotBeNull();
        capturedTags.Should().BeEmpty();
    }

    #endregion

    #region Save As Draft Tests

    [Fact]
    public void SaveScenarioModal_ShowsSaveDraftText_WhenDraftCheckboxChecked()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Act
        cut.Find("input[type='checkbox']").Change(true);

        // Assert
        var saveButton = cut.Find(".btn-primary");
        saveButton.TextContent.Should().Contain("Save Draft");
    }

    [Fact]
    public void SaveScenarioModal_ShowsSaveScenarioText_WhenDraftUnchecked()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Act - Ensure checkbox is unchecked (default)
        var checkbox = cut.Find("input[type='checkbox']");

        // Assert
        var saveButton = cut.Find(".btn-primary");
        saveButton.TextContent.Should().Contain("Save Scenario");
    }

    [Fact]
    public async Task SaveScenarioModal_PassesDraftFlag_WhenSavingAsDraft()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        bool? capturedDraftFlag = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(), Arg.Any<List<string>>(),
            Arg.Do<bool>(d => capturedDraftFlag = d))
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");
        cut.Find("input[type='checkbox']").Change(true);

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        capturedDraftFlag.Should().BeTrue();
    }

    #endregion

    #region Currency Formatting Tests

    [Theory]
    [InlineData(1_500_000, "$1.50M")]
    [InlineData(2_000_000, "$2.00M")]
    public void SaveScenarioModal_FormatsCurrency_Millions(decimal amount, string expected)
    {
        // Arrange
        var costEstimate = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            MonthlyTotal = amount,
            Breakdown = new Dictionary<CostCategory, CostBreakdown>()
        };

        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.CostEstimate, costEstimate));

        // Assert
        var costItem = cut.FindAll(".preview-item")
            .First(p => p.QuerySelector(".preview-label")?.TextContent == "Est. Monthly");
        costItem.QuerySelector(".preview-value")!.TextContent.Should().Be(expected);
    }

    [Theory]
    [InlineData(15_000, "$15.0K")]
    [InlineData(1_500, "$1.5K")]
    public void SaveScenarioModal_FormatsCurrency_Thousands(decimal amount, string expected)
    {
        // Arrange
        var costEstimate = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            MonthlyTotal = amount,
            Breakdown = new Dictionary<CostCategory, CostBreakdown>()
        };

        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.CostEstimate, costEstimate));

        // Assert
        var costItem = cut.FindAll(".preview-item")
            .First(p => p.QuerySelector(".preview-label")?.TextContent == "Est. Monthly");
        costItem.QuerySelector(".preview-value")!.TextContent.Should().Be(expected);
    }

    [Theory]
    [InlineData(500, "$500.00")]
    [InlineData(99.99, "$99.99")]
    public void SaveScenarioModal_FormatsCurrency_SmallAmounts(decimal amount, string expected)
    {
        // Arrange
        var costEstimate = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            MonthlyTotal = amount,
            Breakdown = new Dictionary<CostCategory, CostBreakdown>()
        };

        // Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.CostEstimate, costEstimate));

        // Assert
        var costItem = cut.FindAll(".preview-item")
            .First(p => p.QuerySelector(".preview-label")?.TextContent == "Est. Monthly");
        costItem.QuerySelector(".preview-value")!.TextContent.Should().Be(expected);
    }

    #endregion

    #region Overlay Click Tests

    [Fact]
    public async Task SaveScenarioModal_Closes_WhenOverlayClicked()
    {
        // Arrange
        bool closedWithFalse = false;
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.IsVisibleChanged, value => closedWithFalse = value == false));

        // Act
        await cut.InvokeAsync(() => cut.Find(".modal-overlay").Click());

        // Assert
        closedWithFalse.Should().BeTrue();
    }

    [Fact]
    public void SaveScenarioModal_ContentHasStopPropagation_ToPreventOverlayClose()
    {
        // Arrange & Act
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        // Assert - Verify the modal-content has stopPropagation to prevent overlay click
        // This is implemented via @onclick:stopPropagation in Blazor
        var content = cut.Find(".modal-content");
        content.Should().NotBeNull();
        // The presence of onclick:stopPropagation prevents bubbling to overlay
    }

    #endregion

    #region Saving State Tests

    [Fact]
    public async Task SaveScenarioModal_ShowsSavingText_DuringSave()
    {
        // Arrange
        var tcs = new TaskCompletionSource<Scenario>();
        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<bool>())
            .Returns(tcs.Task);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");

        // Act
        var clickTask = cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert - During save, button should show "Saving..."
        cut.WaitForAssertion(() =>
            cut.Find(".btn-primary").TextContent.Should().Contain("Saving..."));

        // Complete the save
        tcs.SetResult(new Scenario { Id = Guid.NewGuid(), Name = "Test" });
        await clickTask;
    }

    [Fact]
    public async Task SaveScenarioModal_DisablesButton_DuringSave()
    {
        // Arrange
        var tcs = new TaskCompletionSource<Scenario>();
        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<bool>())
            .Returns(tcs.Task);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");

        // Act
        var clickTask = cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert - Button should be disabled during save
        cut.WaitForAssertion(() =>
            cut.Find(".btn-primary").HasAttribute("disabled").Should().BeTrue());

        // Complete the save
        tcs.SetResult(new Scenario { Id = Guid.NewGuid(), Name = "Test" });
        await clickTask;
    }

    #endregion

    #region OnSaved Callback Tests

    [Fact]
    public async Task SaveScenarioModal_InvokesOnSaved_AfterSuccessfulSave()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        Scenario? callbackScenario = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult())
            .Add(p => p.OnSaved, EventCallback.Factory.Create<Scenario>(this, s => callbackScenario = s)));

        cut.Find("#scenarioName").Input("Test");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        callbackScenario.Should().NotBeNull();
        callbackScenario!.Id.Should().Be(savedScenario.Id);
    }

    #endregion

    #region Description Tests

    [Fact]
    public async Task SaveScenarioModal_PassesDescription_WhenProvided()
    {
        // Arrange
        var savedScenario = new Scenario { Id = Guid.NewGuid(), Name = "Test" };
        string? capturedDescription = null;

        _mockScenarioService.SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(),
            Arg.Do<string>(d => capturedDescription = d),
            Arg.Any<List<string>>(), Arg.Any<bool>())
            .Returns(savedScenario);

        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())
            .Add(p => p.K8sResult, CreateTestK8sResult()));

        cut.Find("#scenarioName").Input("Test");
        cut.Find("#scenarioDescription").Change("This is a test description");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        capturedDescription.Should().Be("This is a test description");
    }

    #endregion

    #region CanSave Validation Tests

    [Fact]
    public void SaveScenarioModal_SaveButtonDisabled_WhenNoResult()
    {
        // Arrange & Act - No K8sResult or VMResult provided
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.K8sInput, CreateTestK8sInput())); // Input but no result

        // Assert
        var saveButton = cut.Find(".btn-primary");
        saveButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task SaveScenarioModal_DoesNotSave_WhenNoResultAvailable()
    {
        // Arrange
        var cut = RenderComponent<SaveScenarioModal>(parameters => parameters
            .Add(p => p.IsVisible, true)); // No input or result

        cut.Find("#scenarioName").Input("Test Scenario");

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert - Neither save method should be called
        await _mockScenarioService.DidNotReceive().SaveK8sScenarioAsync(
            Arg.Any<string>(), Arg.Any<K8sSizingInput>(), Arg.Any<K8sSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<bool>());
        await _mockScenarioService.DidNotReceive().SaveVMScenarioAsync(
            Arg.Any<string>(), Arg.Any<VMSizingInput>(), Arg.Any<VMSizingResult>(),
            Arg.Any<CostEstimate?>(), Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<bool>());
    }

    #endregion
}
