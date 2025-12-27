using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models.Growth;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Configuration;

/// <summary>
/// Tests for GrowthSettingsPanel component - Growth planning configuration
/// </summary>
public class GrowthSettingsPanelTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void GrowthSettingsPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        cut.Find(".growth-settings-panel").Should().NotBeNull();
    }

    [Fact]
    public void GrowthSettingsPanel_RendersTitle()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Growth Configuration");
    }

    [Fact]
    public void GrowthSettingsPanel_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".growth-settings-panel").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void GrowthSettingsPanel_RendersProjectionPeriodDropdown()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        var selects = cut.FindAll(".setting-item select");
        selects.Should().HaveCount(2); // Period and Pattern
        var periodSelect = selects[0];
        var periodOptions = periodSelect.QuerySelectorAll("option");
        periodOptions.Should().HaveCount(3);
        periodOptions.Should().Contain(o => o.TextContent.Contains("1 Year"));
        periodOptions.Should().Contain(o => o.TextContent.Contains("3 Years"));
        periodOptions.Should().Contain(o => o.TextContent.Contains("5 Years"));
    }

    [Fact]
    public void GrowthSettingsPanel_RendersGrowthRateSlider()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        cut.Find(".rate-input-group input[type='range']").Should().NotBeNull();
        cut.Find(".rate-value input[type='number']").Should().NotBeNull();
    }

    [Fact]
    public void GrowthSettingsPanel_RendersGrowthPatternDropdown()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        var patternSelect = cut.FindAll("select")[1]; // Second select
        var options = patternSelect.QuerySelectorAll("option");
        options.Should().HaveCount(3);
    }

    [Fact]
    public void GrowthSettingsPanel_RendersCalculateButton()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        cut.Find(".btn-calculate").TextContent.Should().Contain("Calculate Projection");
    }

    #endregion

    #region Settings Display Tests

    [Fact]
    public void GrowthSettingsPanel_DisplaysCurrentSettings()
    {
        // Arrange
        var settings = new GrowthSettings
        {
            ProjectionYears = 5,
            AnnualGrowthRate = 25
        };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        var yearSelect = cut.Find(".setting-item select");
        yearSelect.GetAttribute("value").Should().Be("5");
    }

    [Fact]
    public void GrowthSettingsPanel_DisplaysGrowthRate()
    {
        // Arrange
        var settings = new GrowthSettings { AnnualGrowthRate = 30 };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        var rangeInput = cut.Find(".rate-input-group input[type='range']");
        rangeInput.GetAttribute("value").Should().Be("30");
    }

    #endregion

    #region Pattern Hint Tests

    [Fact]
    public void GrowthSettingsPanel_ShowsLinearPatternHint()
    {
        // Arrange
        var settings = new GrowthSettings { Pattern = GrowthPattern.Linear };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.Find(".hint").TextContent.Should().Contain("Same absolute increase each year");
    }

    [Fact]
    public void GrowthSettingsPanel_ShowsExponentialPatternHint()
    {
        // Arrange
        var settings = new GrowthSettings { Pattern = GrowthPattern.Exponential };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.Find(".hint").TextContent.Should().Contain("Compound percentage growth");
    }

    [Fact]
    public void GrowthSettingsPanel_ShowsSCurvePatternHint()
    {
        // Arrange
        var settings = new GrowthSettings { Pattern = GrowthPattern.SCurve };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.Find(".hint").TextContent.Should().Contain("Accelerate then plateau");
    }

    [Fact]
    public void GrowthSettingsPanel_ShowsCustomPatternHint()
    {
        // Arrange
        var settings = new GrowthSettings { Pattern = GrowthPattern.Custom };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.Find(".hint").TextContent.Should().Contain("Custom rates per year");
    }

    #endregion

    #region Toggle Tests

    [Fact]
    public void GrowthSettingsPanel_RendersIncludeCostToggle()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        var toggleLabels = cut.FindAll(".toggle-option .toggle-label");
        toggleLabels.Should().Contain(l => l.TextContent.Contains("Include Cost Projections"));
    }

    [Fact]
    public void GrowthSettingsPanel_RendersClusterLimitToggle()
    {
        // Act
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Assert
        var toggleLabels = cut.FindAll(".toggle-option .toggle-label");
        toggleLabels.Should().Contain(l => l.TextContent.Contains("Show Capacity Warnings"));
    }

    [Fact]
    public void GrowthSettingsPanel_ShowsCostInflation_WhenCostProjectionsEnabled()
    {
        // Arrange
        var settings = new GrowthSettings { IncludeCostProjections = true };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.Find(".inline-setting").Should().NotBeNull();
        cut.Find(".inline-setting .inline-label").TextContent.Should().Contain("Inflation Rate");
    }

    [Fact]
    public void GrowthSettingsPanel_HidesCostInflation_WhenCostProjectionsDisabled()
    {
        // Arrange
        var settings = new GrowthSettings { IncludeCostProjections = false };

        // Act
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.FindAll(".inline-setting").Should().BeEmpty();
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task GrowthSettingsPanel_ChangingSettings_InvokesSettingsChangedCallback()
    {
        // Arrange
        GrowthSettings? changedSettings = null;
        var settings = new GrowthSettings();
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.SettingsChanged, EventCallback.Factory.Create<GrowthSettings>(this, s => changedSettings = s)));

        // Act
        var yearSelect = cut.Find(".setting-item select");
        await cut.InvokeAsync(() => yearSelect.Change("5"));

        // Assert
        changedSettings.Should().NotBeNull();
    }

    [Fact]
    public async Task GrowthSettingsPanel_ClickingCalculate_InvokesOnCalculateCallback()
    {
        // Arrange
        GrowthSettings? calculatedSettings = null;
        var settings = new GrowthSettings();
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<GrowthSettings>(this, s => calculatedSettings = s)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-calculate").Click());

        // Assert
        calculatedSettings.Should().NotBeNull();
    }

    [Fact]
    public async Task GrowthSettingsPanel_WhileCalculating_ButtonShowsCalculating()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var settings = new GrowthSettings();
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<GrowthSettings>(this, async _ => await tcs.Task)));

        // Act
        var clickTask = cut.InvokeAsync(() => cut.Find(".btn-calculate").Click());

        // Assert - Button should show calculating state
        cut.WaitForAssertion(() =>
        {
            cut.Find(".btn-calculate").TextContent.Should().Contain("Calculating");
        });

        // Cleanup
        tcs.SetResult(true);
        await clickTask;
    }

    [Fact]
    public async Task GrowthSettingsPanel_NoCallback_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<GrowthSettingsPanel>();

        // Act & Assert
        var action = async () =>
        {
            await cut.InvokeAsync(() => cut.Find(".btn-calculate").Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void GrowthSettingsPanel_UpdatesPattern_WhenSettingsChange()
    {
        // Arrange
        var settings = new GrowthSettings { Pattern = GrowthPattern.Linear };
        var cut = RenderComponent<GrowthSettingsPanel>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert initial
        cut.Find(".hint").TextContent.Should().Contain("Same absolute increase");

        // Act
        settings.Pattern = GrowthPattern.Exponential;
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        cut.Find(".hint").TextContent.Should().Contain("Compound percentage growth");
    }

    #endregion
}
