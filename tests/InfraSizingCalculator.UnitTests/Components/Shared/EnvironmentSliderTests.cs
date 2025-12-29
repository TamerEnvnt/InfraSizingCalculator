using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

public class EnvironmentSliderTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void Render_ShowsSliderHeader()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        cut.Markup.Should().Contain("Multi-Cluster Mode");
        cut.Markup.Should().Contain("Configure apps for each environment cluster");
    }

    [Fact]
    public void Render_ShowsStatsSection()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        cut.Markup.Should().Contain("Apps");
        cut.Markup.Should().Contain("Est. CPU");
        cut.Markup.Should().Contain("Est. RAM (GB)");
    }

    [Fact]
    public void Render_ShowsNavigationDots()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var dots = cut.FindAll(".env-dot");
        dots.Should().HaveCount(2);
    }

    [Fact]
    public void Render_ShowsNavigationButtons()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        cut.FindAll(".slider-nav").Should().HaveCount(2); // Prev and Next
        cut.Markup.Should().Contain("◀");
        cut.Markup.Should().Contain("▶");
    }

    [Fact]
    public void Render_ShowsTierPanels()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var tierPanels = cut.FindAll(".tier-panel");
        tierPanels.Should().HaveCount(4); // S, M, L, XL
    }

    [Fact]
    public void Render_ShowsNavigationHint()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        cut.Markup.Should().Contain("1 of 2 environments");
    }

    #endregion

    #region CurrentEnvironment Tests

    [Fact]
    public void CurrentEnvironment_StartsAtFirstEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - First env (Dev in order) should be shown
        cut.Markup.Should().Contain("Dev");
        var activeDot = cut.Find(".env-dot.active");
        activeDot.InnerHtml.Should().Contain("Dev");
    }

    [Fact]
    public void CurrentEnvironment_ShowsCorrectEnvClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var slide = cut.Find(".env-slide");
        slide.ClassList.Should().Contain("env-prod");
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void PrevButton_DisabledAtFirstEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var prevButton = cut.Find(".slider-nav.prev");
        prevButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void NextButton_EnabledWhenMoreEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var nextButton = cut.Find(".slider-nav.next");
        nextButton.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public async Task GoNext_MovesToNextEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Act
        var nextButton = cut.Find(".slider-nav.next");
        await cut.InvokeAsync(() => nextButton.Click());

        // Assert - Should show Prod now (second env)
        cut.Markup.Should().Contain("2 of 2 environments");
    }

    [Fact]
    public async Task GoPrevious_MovesToPreviousEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Navigate to second env first
        var nextButton = cut.Find(".slider-nav.next");
        await cut.InvokeAsync(() => nextButton.Click());

        // Act - Go back
        var prevButton = cut.Find(".slider-nav.prev");
        await cut.InvokeAsync(() => prevButton.Click());

        // Assert - Should be back at first env
        cut.Markup.Should().Contain("1 of 2 environments");
    }

    [Fact]
    public async Task GoNext_DisabledAtLastEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Navigate to last env
        var nextButton = cut.Find(".slider-nav.next");
        await cut.InvokeAsync(() => nextButton.Click());

        // Assert
        nextButton = cut.Find(".slider-nav.next");
        nextButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task SelectEnvironment_NavigatesToEnv()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Prod
        };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Act - Click on Prod dot directly
        var dots = cut.FindAll(".env-dot");
        var prodDot = dots.First(d => d.InnerHtml.Contains("Prod"));
        await cut.InvokeAsync(() => prodDot.Click());

        // Assert
        cut.Markup.Should().Contain("3 of 3 environments");
        cut.Find(".env-dot.active").InnerHtml.Should().Contain("Prod");
    }

    #endregion

    #region Environment Dot Tests

    [Fact]
    public void ActiveDot_HasActiveClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var activeDot = cut.Find(".env-dot.active");
        activeDot.Should().NotBeNull();
    }

    [Fact]
    public void Dot_ShowsBadgeWhenAppsConfigured()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 5 }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert
        var badge = cut.Find(".dot-badge");
        badge.TextContent.Should().Be("5");
    }

    [Fact]
    public void Dot_HidesBadgeWhenNoApps()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>()));

        // Assert
        var badges = cut.FindAll(".dot-badge");
        badges.Should().BeEmpty();
    }

    [Theory]
    [InlineData(EnvironmentType.Prod, "prod")]
    [InlineData(EnvironmentType.DR, "prod")]
    [InlineData(EnvironmentType.Dev, "nonprod")]
    public void Dot_HasCorrectProdNonProdClass(EnvironmentType env, string expectedClass)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var dot = cut.Find(".env-dot");
        dot.ClassList.Should().Contain(expectedClass);
    }

    #endregion

    #region Stats Calculations Tests

    [Fact]
    public void GetTotalApps_CalculatesCorrectly()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev
        };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 5, Medium = 3 },
            [EnvironmentType.Dev] = new AppConfig { Small = 2, Large = 1 }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - Total is 11 apps
        var statItems = cut.FindAll(".stat-item");
        var appsStatValue = statItems[0].QuerySelector(".stat-value");
        appsStatValue!.TextContent.Should().Be("11");
    }

    [Fact]
    public void GetEstimatedCpu_FormatsWithOneDecimal()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            // 2*0.25 = 0.5
            [EnvironmentType.Prod] = new AppConfig { Small = 2 }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var cpuStatValue = statItems[1].QuerySelector(".stat-value");
        cpuStatValue!.TextContent.Should().Be("0.5");
    }

    [Fact]
    public void GetEstimatedMemory_FormatsAsWholeNumber()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            // 2*4.0 = 8
            [EnvironmentType.Prod] = new AppConfig { XLarge = 2 }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var memoryStatValue = statItems[2].QuerySelector(".stat-value");
        memoryStatValue!.TextContent.Should().Be("8");
    }

    #endregion

    #region Tier Input Tests

    [Fact]
    public async Task TierInput_UpdatesCorrectTier()
    {
        // Arrange
        Dictionary<EnvironmentType, AppConfig>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>();

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, a => result = a)));

        // Act - Update Small tier
        var inputs = cut.FindAll(".tier-input");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "7" }));

        // Assert
        result.Should().NotBeNull();
        result![EnvironmentType.Prod].Small.Should().Be(7);
    }

    [Fact]
    public async Task TierInput_CreatesNewConfig_WhenMissing()
    {
        // Arrange
        Dictionary<EnvironmentType, AppConfig>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>())
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, a => result = a)));

        // Act
        var inputs = cut.FindAll(".tier-input");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "3" }));

        // Assert
        result.Should().ContainKey(EnvironmentType.Prod);
    }

    [Theory]
    [InlineData(0, AppTier.Small)]
    [InlineData(1, AppTier.Medium)]
    [InlineData(2, AppTier.Large)]
    [InlineData(3, AppTier.XLarge)]
    public async Task TierInputs_MapToCorrectTiers(int inputIndex, AppTier expectedTier)
    {
        // Arrange
        Dictionary<EnvironmentType, AppConfig>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig()
        };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, a => result = a)));

        // Act
        var inputs = cut.FindAll(".tier-input");
        await cut.InvokeAsync(() => inputs[inputIndex].Change(new ChangeEventArgs { Value = "5" }));

        // Assert
        result.Should().NotBeNull();
        var config = result![EnvironmentType.Prod];
        var actualValue = expectedTier switch
        {
            AppTier.Small => config.Small,
            AppTier.Medium => config.Medium,
            AppTier.Large => config.Large,
            AppTier.XLarge => config.XLarge,
            _ => 0
        };
        actualValue.Should().Be(5);
    }

    #endregion

    #region ParseInt Tests

    [Fact]
    public async Task ParseInt_NegativeValue_ReturnsZero()
    {
        // Arrange
        Dictionary<EnvironmentType, AppConfig>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>())
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, a => result = a)));

        // Act
        var inputs = cut.FindAll(".tier-input");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "-5" }));

        // Assert
        result![EnvironmentType.Prod].Small.Should().Be(0);
    }

    [Fact]
    public async Task ParseInt_InvalidValue_ReturnsZero()
    {
        // Arrange
        Dictionary<EnvironmentType, AppConfig>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>())
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, a => result = a)));

        // Act
        var inputs = cut.FindAll(".tier-input");
        await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "invalid" }));

        // Assert
        result![EnvironmentType.Prod].Small.Should().Be(0);
    }

    #endregion

    #region OnParametersSet Tests

    [Fact]
    public void OnParametersSet_ClampsIndexToValidRange()
    {
        // Arrange - Start with 3 environments
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Prod
        };

        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Navigate to last env
        for (int i = 0; i < 2; i++)
        {
            cut.Find(".slider-nav.next").Click();
        }
        cut.Markup.Should().Contain("3 of 3 environments");

        // Act - Reduce to 1 environment
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - Index should be clamped to 0
        cut.Markup.Should().Contain("1 of 1 environments");
    }

    #endregion

    #region Spec Display Tests

    [Fact]
    public void TierSpecs_AreDisplayed()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.SmallSpec, "S Spec")
            .Add(p => p.MediumSpec, "M Spec")
            .Add(p => p.LargeSpec, "L Spec")
            .Add(p => p.XLargeSpec, "XL Spec"));

        // Assert
        cut.Markup.Should().Contain("S Spec");
        cut.Markup.Should().Contain("M Spec");
        cut.Markup.Should().Contain("L Spec");
        cut.Markup.Should().Contain("XL Spec");
    }

    [Fact]
    public void DefaultSpecs_AreCorrect()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        cut.Markup.Should().Contain("0.25 CPU, 0.5 GB RAM");
        cut.Markup.Should().Contain("0.5 CPU, 1 GB RAM");
        cut.Markup.Should().Contain("1 CPU, 2 GB RAM");
        cut.Markup.Should().Contain("2 CPU, 4 GB RAM");
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
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var slide = cut.Find(".env-slide");
        slide.ClassList.Should().Contain(expectedClass);
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
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var icon = cut.Find(".env-icon");
        icon.TextContent.Should().Be(expectedIcon);
    }

    #endregion

    #region IsProdEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod, "Production")]
    [InlineData(EnvironmentType.DR, "Production")]
    [InlineData(EnvironmentType.Dev, "Non-Production")]
    [InlineData(EnvironmentType.Test, "Non-Production")]
    [InlineData(EnvironmentType.Stage, "Non-Production")]
    public void IsProdEnv_ShowsCorrectLabel(EnvironmentType env, string expectedLabel)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        cut.Markup.Should().Contain(expectedLabel + " Environment");
    }

    #endregion

    #region Empty State Tests

    [Fact]
    public void EmptyEnvironments_HandlesGracefully()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType>()));

        // Assert - Should render without error
        cut.Find(".env-slider").Should().NotBeNull();
    }

    #endregion
}
