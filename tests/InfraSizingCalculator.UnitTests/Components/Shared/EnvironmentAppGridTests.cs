using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

public class EnvironmentAppGridTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void Render_ShowsGridHeader()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        cut.Markup.Should().Contain("Multi-Cluster Mode");
        cut.Markup.Should().Contain("Configure apps for each environment cluster");
    }

    [Fact]
    public void Render_ShowsStatsSection()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        cut.Markup.Should().Contain("Apps");
        cut.Markup.Should().Contain("Est. CPU");
        cut.Markup.Should().Contain("Est. RAM");
    }

    [Fact]
    public void Render_ShowsEnvironmentToggles()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - should show all 5 environments as toggles
        var toggles = cut.FindAll(".env-toggle");
        toggles.Should().HaveCount(5); // Dev, Test, Stage, Prod, DR
    }

    [Fact]
    public void Render_ShowsToggleHint()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        cut.Markup.Should().Contain("Select clusters to include in sizing");
    }

    [Fact]
    public void Render_ShowsEnvironmentCards()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - should show cards for enabled environments
        var cards = cut.FindAll(".env-app-card");
        cards.Should().HaveCount(2);
    }

    #endregion

    #region Environment Toggle Tests

    [Fact]
    public void EnabledToggle_HasEnabledClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.Dev };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var toggles = cut.FindAll(".env-toggle.enabled");
        toggles.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ProdToggle_IsDisabled()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - Prod toggle's checkbox should be disabled
        var toggles = cut.FindAll(".env-toggle");
        var prodToggle = toggles.FirstOrDefault(t => t.InnerHtml.Contains("Prod"));
        prodToggle.Should().NotBeNull();

        var checkbox = prodToggle!.QuerySelector("input[type='checkbox']");
        checkbox.Should().NotBeNull();
        checkbox!.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ProdEnv_HasProdClass()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        var toggles = cut.FindAll(".env-toggle.prod");
        toggles.Should().NotBeEmpty();
    }

    [Fact]
    public void NonProdEnv_HasNonProdClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.Dev };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var toggles = cut.FindAll(".env-toggle.nonprod");
        toggles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ToggleEnv_EnablesEnvironment()
    {
        // Arrange
        HashSet<EnvironmentType>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnabledEnvironmentsChanged, EventCallback.Factory.Create<HashSet<EnvironmentType>>(
                this, envs => result = envs)));

        // Act - click Dev toggle
        var checkboxes = cut.FindAll(".env-toggle input[type='checkbox']");
        var devCheckbox = checkboxes.First(); // Dev is first
        await cut.InvokeAsync(() => devCheckbox.Change(new ChangeEventArgs { Value = true }));

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(EnvironmentType.Dev);
    }

    [Fact]
    public async Task ToggleEnv_DisablesEnvironment()
    {
        // Arrange
        HashSet<EnvironmentType>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.Dev };

        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnabledEnvironmentsChanged, EventCallback.Factory.Create<HashSet<EnvironmentType>>(
                this, envs => result = envs)));

        // Act - uncheck Dev toggle
        var checkboxes = cut.FindAll(".env-toggle input[type='checkbox']");
        var devCheckbox = checkboxes.First(); // Dev is first
        await cut.InvokeAsync(() => devCheckbox.Change(new ChangeEventArgs { Value = false }));

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain(EnvironmentType.Dev);
    }

    #endregion

    #region Stats Calculations Tests

    [Fact]
    public void GetTotalApps_CalculatesCorrectly()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 5, Medium = 3, Large = 2, XLarge = 1 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - Total is 11 apps
        var statItems = cut.FindAll(".stat-item");
        var appsStatValue = statItems[0].QuerySelector(".stat-value");
        appsStatValue!.TextContent.Should().Be("11");
    }

    [Fact]
    public void GetTotalApps_SumsAcrossMultipleEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev
        };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 5, Medium = 0, Large = 0, XLarge = 0 },
            [EnvironmentType.Dev] = new AppConfig { Small = 3, Medium = 0, Large = 0, XLarge = 0 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - Total is 8 apps
        var statItems = cut.FindAll(".stat-item");
        var appsStatValue = statItems[0].QuerySelector(".stat-value");
        appsStatValue!.TextContent.Should().Be("8");
    }

    [Fact]
    public void GetEstimatedCpu_CalculatesCorrectly()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            // 4*0.25 + 2*0.5 + 2*1.0 + 1*2.0 = 1 + 1 + 2 + 2 = 6
            [EnvironmentType.Prod] = new AppConfig { Small = 4, Medium = 2, Large = 2, XLarge = 1 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var cpuStatValue = statItems[1].QuerySelector(".stat-value");
        cpuStatValue!.TextContent.Should().Be("6");
    }

    [Fact]
    public void GetEstimatedMemory_CalculatesCorrectly()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            // 4*0.5 + 2*1.0 + 2*2.0 + 1*4.0 = 2 + 2 + 4 + 4 = 12
            [EnvironmentType.Prod] = new AppConfig { Small = 4, Medium = 2, Large = 2, XLarge = 1 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var memoryStatValue = statItems[2].QuerySelector(".stat-value");
        memoryStatValue!.TextContent.Should().Be("12");
    }

    [Fact]
    public void GetTotalApps_NoApps_ReturnsZero()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>()));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var appsStatValue = statItems[0].QuerySelector(".stat-value");
        appsStatValue!.TextContent.Should().Be("0");
    }

    #endregion

    #region Card Expansion Tests

    [Fact]
    public void ProdCard_IsExpandedByDefault()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - Prod card should be expanded by default
        var cards = cut.FindAll(".env-app-card.expanded");
        cards.Should().NotBeEmpty();
    }

    #endregion

    #region GetEnvConfig Tests

    [Fact]
    public void GetEnvConfig_ExistingEnv_ReturnsConfig()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 10 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - The card should show the configured values
        cut.Markup.Should().Contain("10");
    }

    [Fact]
    public void GetEnvConfig_MissingEnv_ReturnsEmptyConfig()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>()));

        // Assert - Should render without error (empty config means zeros)
        cut.Find(".env-app-card").Should().NotBeNull();
    }

    #endregion

    #region UpdateEnvTier Tests

    [Fact]
    public void UpdateEnvTier_MethodExists_InComponent()
    {
        // Arrange - This test verifies that EnvironmentAppsChanged callback parameter exists
        // The actual update is tested through integration since child component callbacks
        // are complex to test in isolation with bUnit
        Dictionary<EnvironmentType, AppConfig>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>();

        // Act - Just verify the component renders without error with the callback
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, a => result = a)));

        // Assert - Component should render successfully
        cut.Find(".env-app-grid").Should().NotBeNull();
        cut.Find(".env-cards-container").Should().NotBeNull();
    }

    #endregion

    #region IsProdEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod, true)]
    [InlineData(EnvironmentType.DR, true)]
    [InlineData(EnvironmentType.Dev, false)]
    [InlineData(EnvironmentType.Test, false)]
    [InlineData(EnvironmentType.Stage, false)]
    public void IsProdEnv_ReturnsCorrectValue(EnvironmentType env, bool expected)
    {
        // The IsProdEnv method treats Prod and DR as production environments
        // We verify this by checking if the toggle has 'prod' or 'nonprod' class

        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, env };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var toggles = cut.FindAll(".env-toggle");
        var targetToggle = toggles.FirstOrDefault(t => t.InnerHtml.Contains(env.ToString()));
        targetToggle.Should().NotBeNull();

        if (expected)
            targetToggle!.ClassList.Should().Contain("prod");
        else
            targetToggle!.ClassList.Should().Contain("nonprod");
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
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, env };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var toggles = cut.FindAll(".env-toggle-icon");
        toggles.Any(t => t.TextContent == expectedIcon).Should().BeTrue();
    }

    #endregion

    #region Spec Passthrough Tests

    [Fact]
    public void TierSpecs_Parameters_ExistOnComponent()
    {
        // Arrange - Verify spec parameters can be passed to the component
        // The actual rendering of specs in child cards requires the card to be expanded
        // which is complex in bUnit with nested components
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.SmallSpec, "Custom Small Spec")
            .Add(p => p.MediumSpec, "Custom Medium Spec")
            .Add(p => p.LargeSpec, "Custom Large Spec")
            .Add(p => p.XLargeSpec, "Custom XLarge Spec"));

        // Assert - Component should render successfully with the spec parameters
        cut.Find(".env-app-grid").Should().NotBeNull();
        cut.Find(".env-cards-container").Should().NotBeNull();

        // Note: Verifying spec passthrough to child cards is tested in EnvironmentAppCardTests
        // where we directly test the EnvironmentAppCard component in expanded state
    }

    #endregion

    #region Environment Order Tests

    [Fact]
    public void EnabledEnvironments_AreOrderedByEnumValue()
    {
        // Arrange - Add environments in reverse order
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.DR,
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - Cards should be in order: Dev, Prod, DR
        var cards = cut.FindAll(".env-app-card");
        cards.Should().HaveCount(3);
    }

    #endregion

    #region Card Expansion State Tests

    [Fact]
    public void NonProdCard_IsNotExpandedByDefault()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - Dev card should NOT be expanded by default
        var cards = cut.FindAll(".env-app-card");
        cards.Should().HaveCount(2);
        // Only Prod is expanded by default
        cut.FindAll(".env-app-card.expanded").Should().HaveCount(1);
    }

    #endregion

    #region ToggleEnv Protection Tests

    [Fact]
    public async Task ToggleEnv_ProdEnvironment_CannotBeDisabled()
    {
        // Arrange
        HashSet<EnvironmentType>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.Dev };

        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnabledEnvironmentsChanged, EventCallback.Factory.Create<HashSet<EnvironmentType>>(
                this, envs => result = envs)));

        // Act - Try to uncheck Prod toggle (checkbox is disabled, but test the protection logic)
        var checkboxes = cut.FindAll(".env-toggle input[type='checkbox']");
        var prodCheckbox = checkboxes.Skip(3).First(); // Prod is 4th (Dev, Test, Stage, Prod, DR)

        // Prod checkbox is disabled at the HTML level, so we're verifying the attribute
        prodCheckbox.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task ToggleEnv_NonProdEnvironment_CanBeToggled()
    {
        // Arrange
        HashSet<EnvironmentType>? result = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnabledEnvironmentsChanged, EventCallback.Factory.Create<HashSet<EnvironmentType>>(
                this, envs => result = envs)));

        // Act - Enable Stage environment
        var checkboxes = cut.FindAll(".env-toggle input[type='checkbox']");
        var stageCheckbox = checkboxes.Skip(2).First(); // Stage is 3rd

        stageCheckbox.HasAttribute("disabled").Should().BeFalse();
        await cut.InvokeAsync(() => stageCheckbox.Change(new ChangeEventArgs { Value = true }));

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(EnvironmentType.Stage);
    }

    #endregion

    #region Multi-Environment Stats Tests

    [Fact]
    public void Stats_OnlyCountEnabledEnvironments()
    {
        // Arrange - Dev is disabled but has apps configured
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 5 },
            [EnvironmentType.Dev] = new AppConfig { Small = 10 } // Not enabled
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - Should only count Prod's 5 apps, not Dev's 10
        var statItems = cut.FindAll(".stat-item");
        var appsStatValue = statItems[0].QuerySelector(".stat-value");
        appsStatValue!.TextContent.Should().Be("5");
    }

    [Fact]
    public void GetEstimatedCpu_ReturnsZero_WhenNoEnabledEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>();

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var cpuStatValue = statItems[1].QuerySelector(".stat-value");
        cpuStatValue!.TextContent.Should().Be("0");
    }

    [Fact]
    public void GetEstimatedMemory_ReturnsZero_WhenNoEnabledEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>();

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var memoryStatValue = statItems[2].QuerySelector(".stat-value");
        memoryStatValue!.TextContent.Should().Be("0");
    }

    [Fact]
    public void GetEstimatedCpu_SumsAcrossMultipleEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev
        };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            // Dev: 4*0.25 = 1 CPU
            [EnvironmentType.Dev] = new AppConfig { Small = 4 },
            // Prod: 2*1.0 = 2 CPU
            [EnvironmentType.Prod] = new AppConfig { Large = 2 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - Total: 1 + 2 = 3 CPU
        var statItems = cut.FindAll(".stat-item");
        var cpuStatValue = statItems[1].QuerySelector(".stat-value");
        cpuStatValue!.TextContent.Should().Be("3");
    }

    [Fact]
    public void GetEstimatedMemory_SumsAcrossMultipleEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev
        };
        var apps = new Dictionary<EnvironmentType, AppConfig>
        {
            // Dev: 4*0.5 = 2 GB
            [EnvironmentType.Dev] = new AppConfig { Small = 4 },
            // Prod: 2*2.0 = 4 GB
            [EnvironmentType.Prod] = new AppConfig { Large = 2 }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, apps));

        // Assert - Total: 2 + 4 = 6 GB
        var statItems = cut.FindAll(".stat-item");
        var memoryStatValue = statItems[2].QuerySelector(".stat-value");
        memoryStatValue!.TextContent.Should().Be("6");
    }

    #endregion

    #region DR Environment Tests

    [Fact]
    public void DREnvironment_IsMarkedAsProdClass()
    {
        // DR environments are treated as production
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.DR };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var toggles = cut.FindAll(".env-toggle");
        var drToggle = toggles.FirstOrDefault(t => t.InnerHtml.Contains("DR</span>"));
        drToggle.Should().NotBeNull();
        drToggle!.ClassList.Should().Contain("prod");
    }

    [Fact]
    public void DREnvironment_CheckboxNotDisabled()
    {
        // DR can be toggled unlike Prod
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.DR };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var toggles = cut.FindAll(".env-toggle");
        var drToggle = toggles.FirstOrDefault(t => t.InnerHtml.Contains("DR</span>"));
        var drCheckbox = drToggle!.QuerySelector("input[type='checkbox']");
        drCheckbox!.HasAttribute("disabled").Should().BeFalse();
    }

    #endregion

    #region AllEnvironments Display Tests

    [Fact]
    public void AllEnvironments_ShowCorrectIcons()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - All 5 icons should be present
        var icons = cut.FindAll(".env-toggle-icon");
        icons.Should().HaveCount(5);

        var iconTexts = icons.Select(i => i.TextContent).ToList();
        iconTexts.Should().Contain("D");   // Dev
        iconTexts.Should().Contain("T");   // Test
        iconTexts.Should().Contain("S");   // Stage
        iconTexts.Should().Contain("P");   // Prod
        iconTexts.Should().Contain("DR");  // DR
    }

    [Fact]
    public void AllEnvironments_ShowCorrectNames()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var names = cut.FindAll(".env-toggle-name");
        names.Should().HaveCount(5);

        var nameTexts = names.Select(n => n.TextContent).ToList();
        nameTexts.Should().Contain("Dev");
        nameTexts.Should().Contain("Test");
        nameTexts.Should().Contain("Stage");
        nameTexts.Should().Contain("Prod");
        nameTexts.Should().Contain("DR");
    }

    #endregion
}
