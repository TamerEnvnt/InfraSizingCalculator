using Bunit;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

/// <summary>
/// Tests for EnvironmentAppCard Blazor component.
/// Tests expand/collapse, environment styling, tier inputs, and callbacks.
/// </summary>
public class EnvironmentAppCardTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void RendersCollapsedByDefault()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, false));

        // Assert
        Assert.Contains("collapsed", cut.Markup);
        Assert.DoesNotContain("tier-inputs-horizontal", cut.Markup);
    }

    [Fact]
    public void RendersExpanded_WhenIsExpandedTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true));

        // Assert
        Assert.Contains("expanded", cut.Markup);
        Assert.Contains("tier-inputs-horizontal", cut.Markup);
    }

    [Fact]
    public void ShowsTierInputs_WhenExpanded()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true));

        // Assert - All 4 tier badges rendered
        Assert.Contains("tier-badge small", cut.Markup);
        Assert.Contains("tier-badge medium", cut.Markup);
        Assert.Contains("tier-badge large", cut.Markup);
        Assert.Contains("tier-badge xlarge", cut.Markup);
    }

    #endregion

    #region Environment Display Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "D")]
    [InlineData(EnvironmentType.Test, "T")]
    [InlineData(EnvironmentType.Stage, "S")]
    [InlineData(EnvironmentType.Prod, "P")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void DisplaysCorrectEnvironmentIcon(EnvironmentType env, string expectedIcon)
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, env));

        // Assert
        Assert.Contains(expectedIcon, cut.Find(".env-card-icon").TextContent);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void AppliesCorrectEnvironmentCss(EnvironmentType env, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, env));

        // Assert
        Assert.Contains(expectedClass, cut.Markup);
    }

    [Fact]
    public void DisplaysEnvironmentName()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod));

        // Assert
        Assert.Contains("Prod", cut.Find(".env-card-name").TextContent);
    }

    #endregion

    #region Total Apps Calculation Tests

    [Fact]
    public void CalculatesTotalApps()
    {
        // Arrange - 3 + 2 + 1 + 0 = 6 total
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.SmallCount, 3)
                     .Add(p => p.MediumCount, 2)
                     .Add(p => p.LargeCount, 1)
                     .Add(p => p.XLargeCount, 0));

        // Assert
        Assert.Contains("6", cut.Find(".total-value").TextContent);
    }

    [Fact]
    public void ShowsAppsSummary_WhenCollapsed()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, false)
                     .Add(p => p.SmallCount, 5)
                     .Add(p => p.MediumCount, 3));

        // Assert - Should show "8 apps" in summary
        Assert.Contains("8 apps", cut.Markup);
    }

    #endregion

    #region Expand/Collapse Toggle Tests

    [Fact]
    public async Task ClickingHeader_TogglesExpanded()
    {
        // Arrange
        bool? expandedState = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, false)
                     .Add(p => p.IsExpandedChanged, EventCallback.Factory.Create<bool>(new object(), v => expandedState = v)));

        // Act
        await cut.Find(".env-card-header").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.True(expandedState);
    }

    [Fact]
    public void ShowsCorrectExpandIcon_WhenCollapsed()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, false));

        // Assert
        Assert.Contains("▶", cut.Find(".expand-icon").TextContent);
    }

    [Fact]
    public void ShowsCorrectExpandIcon_WhenExpanded()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true));

        // Assert
        Assert.Contains("▼", cut.Find(".expand-icon").TextContent);
    }

    #endregion

    #region Tier Input Callback Tests

    [Fact]
    public async Task SmallInput_TriggersOnSmallChanged()
    {
        // Arrange
        int? received = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(new object(), v => received = v)));

        // Act
        var input = cut.FindAll("input[type='number']")[0];
        await input.ChangeAsync(new ChangeEventArgs { Value = "7" });

        // Assert
        Assert.Equal(7, received);
    }

    [Fact]
    public async Task MediumInput_TriggersOnMediumChanged()
    {
        // Arrange
        int? received = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.OnMediumChanged, EventCallback.Factory.Create<int>(new object(), v => received = v)));

        // Act
        var input = cut.FindAll("input[type='number']")[1];
        await input.ChangeAsync(new ChangeEventArgs { Value = "3" });

        // Assert
        Assert.Equal(3, received);
    }

    [Fact]
    public async Task LargeInput_TriggersOnLargeChanged()
    {
        // Arrange
        int? received = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.OnLargeChanged, EventCallback.Factory.Create<int>(new object(), v => received = v)));

        // Act
        var input = cut.FindAll("input[type='number']")[2];
        await input.ChangeAsync(new ChangeEventArgs { Value = "2" });

        // Assert
        Assert.Equal(2, received);
    }

    [Fact]
    public async Task XLargeInput_TriggersOnXLargeChanged()
    {
        // Arrange
        int? received = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.OnXLargeChanged, EventCallback.Factory.Create<int>(new object(), v => received = v)));

        // Act
        var input = cut.FindAll("input[type='number']")[3];
        await input.ChangeAsync(new ChangeEventArgs { Value = "1" });

        // Assert
        Assert.Equal(1, received);
    }

    #endregion

    #region Custom Tier Spec Tests

    [Fact]
    public void DisplaysCustomTierSpecs()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.SmallSpec, "Custom S")
                     .Add(p => p.MediumSpec, "Custom M")
                     .Add(p => p.LargeSpec, "Custom L")
                     .Add(p => p.XLargeSpec, "Custom XL"));

        // Assert
        Assert.Contains("Custom S", cut.Markup);
        Assert.Contains("Custom M", cut.Markup);
        Assert.Contains("Custom L", cut.Markup);
        Assert.Contains("Custom XL", cut.Markup);
    }

    #endregion

    #region Input Parsing Tests

    [Fact]
    public async Task ParseInt_HandlesNegativeValues()
    {
        // Arrange
        int? received = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(new object(), v => received = v)));

        // Act - Try negative value
        var input = cut.FindAll("input[type='number']")[0];
        await input.ChangeAsync(new ChangeEventArgs { Value = "-5" });

        // Assert - Should be clamped to 0
        Assert.Equal(0, received);
    }

    [Fact]
    public async Task ParseInt_HandlesNullValue()
    {
        // Arrange
        int? received = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters =>
            parameters.Add(p => p.Environment, EnvironmentType.Prod)
                     .Add(p => p.IsExpanded, true)
                     .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(new object(), v => received = v)));

        // Act - null value
        var input = cut.FindAll("input[type='number']")[0];
        await input.ChangeAsync(new ChangeEventArgs { Value = null });

        // Assert - Should default to 0
        Assert.Equal(0, received);
    }

    #endregion
}
