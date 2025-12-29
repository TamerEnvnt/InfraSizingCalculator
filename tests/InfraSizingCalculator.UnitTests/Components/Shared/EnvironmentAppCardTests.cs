using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

public class EnvironmentAppCardTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void Render_ShowsEnvironmentName()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod));

        // Assert
        cut.Markup.Should().Contain("Prod");
    }

    [Fact]
    public void Render_CollapsedByDefault_ShowsTotalApps()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.SmallCount, 5)
            .Add(p => p.MediumCount, 3)
            .Add(p => p.LargeCount, 2)
            .Add(p => p.XLargeCount, 1)
            .Add(p => p.IsExpanded, false));

        // Assert - Total is 11 apps
        cut.Markup.Should().Contain("11 apps");
    }

    [Fact]
    public void Render_Expanded_ShowsTierInputs()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Test)
            .Add(p => p.IsExpanded, true));

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        inputs.Should().HaveCount(4); // S, M, L, XL
    }

    [Fact]
    public void Render_Expanded_ShowsTierBadges()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Stage)
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Markup.Should().Contain("S</span>");
        cut.Markup.Should().Contain("M</span>");
        cut.Markup.Should().Contain("L</span>");
        cut.Markup.Should().Contain("XL</span>");
    }

    [Fact]
    public void Render_Expanded_ShowsTierSpecs()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.SmallSpec, "0.25 CPU, 0.5 GB")
            .Add(p => p.MediumSpec, "0.5 CPU, 1 GB")
            .Add(p => p.LargeSpec, "1 CPU, 2 GB")
            .Add(p => p.XLargeSpec, "2 CPU, 4 GB"));

        // Assert
        cut.Markup.Should().Contain("0.25 CPU, 0.5 GB");
        cut.Markup.Should().Contain("0.5 CPU, 1 GB");
        cut.Markup.Should().Contain("1 CPU, 2 GB");
        cut.Markup.Should().Contain("2 CPU, 4 GB");
    }

    [Fact]
    public void Render_Collapsed_HidesTierInputs()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, false));

        // Assert
        var inputs = cut.FindAll(".tier-inputs-horizontal");
        inputs.Should().BeEmpty();
    }

    [Fact]
    public void Render_HasExpandIcon()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, false));

        // Assert - collapsed shows right arrow
        cut.Markup.Should().Contain("▶");
    }

    [Fact]
    public void Render_Expanded_ShowsDownArrow()
    {
        // Arrange
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true));

        // Assert - expanded shows down arrow
        cut.Markup.Should().Contain("▼");
    }

    #endregion

    #region GetEnvClass Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvClass_ReturnsCorrectClass(EnvironmentType env, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, env));

        // Assert
        cut.Find(".env-app-card").ClassList.Should().Contain(expectedClass);
    }

    #endregion

    #region GetEnvIcon Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "D")]
    [InlineData(EnvironmentType.Test, "T")]
    [InlineData(EnvironmentType.Stage, "S")]
    [InlineData(EnvironmentType.Prod, "P")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void GetEnvIcon_ReturnsCorrectIcon(EnvironmentType env, string expectedIcon)
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, env));

        // Assert
        var iconDiv = cut.Find(".env-card-icon");
        iconDiv.TextContent.Should().Be(expectedIcon);
    }

    #endregion

    #region GetTotalApps Tests

    [Fact]
    public void GetTotalApps_CalculatesCorrectly()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.SmallCount, 10)
            .Add(p => p.MediumCount, 5)
            .Add(p => p.LargeCount, 2)
            .Add(p => p.XLargeCount, 1)
            .Add(p => p.IsExpanded, true));

        // Assert - Shows total in the total section
        var totalValue = cut.Find(".total-value");
        totalValue.TextContent.Should().Be("18");
    }

    [Fact]
    public void GetTotalApps_AllZeros_ReturnsZero()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.SmallCount, 0)
            .Add(p => p.MediumCount, 0)
            .Add(p => p.LargeCount, 0)
            .Add(p => p.XLargeCount, 0)
            .Add(p => p.IsExpanded, false));

        // Assert
        cut.Markup.Should().Contain("0 apps");
    }

    #endregion

    #region Toggle Expand Tests

    [Fact]
    public async Task ClickHeader_TogglesExpanded()
    {
        // Arrange
        bool? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, false)
            .Add(p => p.IsExpandedChanged, EventCallback.Factory.Create<bool>(
                this, expanded => result = expanded)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".env-card-header").Click());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ClickHeader_CollapsesWhenExpanded()
    {
        // Arrange
        bool? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.IsExpandedChanged, EventCallback.Factory.Create<bool>(
                this, expanded => result = expanded)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".env-card-header").Click());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Tier Input Callbacks Tests

    [Fact]
    public async Task SmallInput_InvokesCallback()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "15" }));

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public async Task MediumInput_InvokesCallback()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnMediumChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[1].Change(new ChangeEventArgs { Value = "8" }));

        // Assert
        result.Should().Be(8);
    }

    [Fact]
    public async Task LargeInput_InvokesCallback()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnLargeChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[2].Change(new ChangeEventArgs { Value = "3" }));

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task XLargeInput_InvokesCallback()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnXLargeChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[3].Change(new ChangeEventArgs { Value = "2" }));

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region ParseInt Tests

    [Theory]
    [InlineData("5", 5)]
    [InlineData("0", 0)]
    [InlineData("100", 100)]
    public async Task ParseInt_ValidPositiveValues_ReturnsParsedValue(string value, int expected)
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = value }));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ParseInt_NegativeValue_ReturnsZero()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "-5" }));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ParseInt_InvalidValue_ReturnsZero()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "invalid" }));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ParseInt_NullValue_ReturnsZero()
    {
        // Arrange
        int? result = null;
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.OnSmallChanged, EventCallback.Factory.Create<int>(
                this, count => result = count)));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = null }));

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Default Spec Values Tests

    [Fact]
    public void DefaultSpecs_AreCorrect()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true));

        // Assert - verify default spec values are rendered
        cut.Markup.Should().Contain("0.25 CPU, 0.5 GB");
        cut.Markup.Should().Contain("0.5 CPU, 1 GB");
        cut.Markup.Should().Contain("1 CPU, 2 GB");
        cut.Markup.Should().Contain("2 CPU, 4 GB");
    }

    [Fact]
    public void CustomSpecs_AreDisplayed()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.SmallSpec, "Custom Small")
            .Add(p => p.MediumSpec, "Custom Medium")
            .Add(p => p.LargeSpec, "Custom Large")
            .Add(p => p.XLargeSpec, "Custom XLarge"));

        // Assert
        cut.Markup.Should().Contain("Custom Small");
        cut.Markup.Should().Contain("Custom Medium");
        cut.Markup.Should().Contain("Custom Large");
        cut.Markup.Should().Contain("Custom XLarge");
    }

    #endregion

    #region Input Values Tests

    [Fact]
    public void TierInputs_DisplayConfiguredValues()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Prod)
            .Add(p => p.IsExpanded, true)
            .Add(p => p.SmallCount, 10)
            .Add(p => p.MediumCount, 5)
            .Add(p => p.LargeCount, 2)
            .Add(p => p.XLargeCount, 1));

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        inputs[0].GetAttribute("value").Should().Be("10");
        inputs[1].GetAttribute("value").Should().Be("5");
        inputs[2].GetAttribute("value").Should().Be("2");
        inputs[3].GetAttribute("value").Should().Be("1");
    }

    #endregion

    #region Expanded/Collapsed State Tests

    [Fact]
    public void ExpandedState_HasExpandedClass()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".env-app-card").ClassList.Should().Contain("expanded");
    }

    [Fact]
    public void CollapsedState_HasCollapsedClass()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.Dev)
            .Add(p => p.IsExpanded, false));

        // Assert
        cut.Find(".env-app-card").ClassList.Should().Contain("collapsed");
    }

    #endregion

    #region LifeTime Environment Tests

    [Fact]
    public void LifeTimeEnvironment_UsesDefaultClass()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.LifeTime));

        // Assert - should not have any env-specific class for unknown env
        var card = cut.Find(".env-app-card");
        card.ClassList.Should().NotContain("env-dev");
        card.ClassList.Should().NotContain("env-test");
        card.ClassList.Should().NotContain("env-stage");
        card.ClassList.Should().NotContain("env-prod");
        card.ClassList.Should().NotContain("env-dr");
    }

    [Fact]
    public void LifeTimeEnvironment_ShowsDefaultIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppCard>(parameters => parameters
            .Add(p => p.Environment, EnvironmentType.LifeTime));

        // Assert - unknown environments show "E" as default icon
        var iconDiv = cut.Find(".env-card-icon");
        iconDiv.TextContent.Should().Be("E");
    }

    #endregion
}
