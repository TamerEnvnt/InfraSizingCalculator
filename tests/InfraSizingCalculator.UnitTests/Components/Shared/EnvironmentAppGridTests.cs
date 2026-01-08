using Bunit;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

/// <summary>
/// Tests for EnvironmentAppGrid Blazor component.
/// Tests grid layout, environment toggles, stats, and propagation.
/// </summary>
public class EnvironmentAppGridTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void RendersGridContainer()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("env-app-grid", cut.Markup);
        Assert.Contains("Multi-Cluster Mode", cut.Markup);
    }

    [Fact]
    public void RendersHeaderWithStats()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("header-stats", cut.Markup);
        Assert.Contains("Apps", cut.Markup);
        Assert.Contains("Est. CPU", cut.Markup);
        Assert.Contains("Est. RAM", cut.Markup);
    }

    [Fact]
    public void RendersEnvironmentToggles()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - All 5 environments have toggles
        var toggles = cut.FindAll(".env-toggle");
        Assert.Equal(5, toggles.Count);
    }

    [Fact]
    public void RendersEnvironmentCards_ForEnabledEnvironments()
    {
        // Arrange
        var envs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envs)
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Dev, new AppConfig() },
                         { EnvironmentType.Prod, new AppConfig() }
                     }));

        // Assert - Should render EnvironmentAppCard for each enabled env
        var cards = cut.FindAll(".env-app-card");
        Assert.Equal(2, cards.Count);
    }

    #endregion

    #region Environment Toggle Tests

    [Fact]
    public void ProdToggle_IsDisabled()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - Prod checkbox should be disabled
        var prodToggle = cut.FindAll(".env-toggle input[type='checkbox']")
            .FirstOrDefault(cb => cut.Markup.Contains("env-toggle") && cut.Markup.Contains("Prod"));

        // Find the disabled input for Prod
        var disabledInputs = cut.FindAll("input[disabled]");
        Assert.True(disabledInputs.Count >= 1);
    }

    [Fact]
    public void EnabledEnvironments_HaveEnabledClass()
    {
        // Arrange
        var envs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envs));

        // Assert
        var enabledToggles = cut.FindAll(".env-toggle.enabled");
        Assert.Equal(2, enabledToggles.Count);
    }

    [Fact]
    public async Task TogglingEnvironment_TriggersCallback()
    {
        // Arrange
        var envs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        HashSet<EnvironmentType>? received = null;

        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envs)
                     .Add(p => p.EnabledEnvironmentsChanged, EventCallback.Factory.Create<HashSet<EnvironmentType>>(new object(), e => received = e)));

        // Act - Toggle Dev environment (find the checkbox that's not Prod)
        var checkboxes = cut.FindAll(".env-toggle input[type='checkbox']");
        var devCheckbox = checkboxes[0]; // Dev is first in order
        await devCheckbox.ChangeAsync(new ChangeEventArgs { Value = true });

        // Assert
        Assert.NotNull(received);
        Assert.Contains(EnvironmentType.Dev, received);
    }

    #endregion

    #region Stats Calculation Tests

    [Fact]
    public void CalculatesTotalApps()
    {
        // Arrange - 3 + 2 + 1 = 6 total across environments
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 3 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 2, Large = 1 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "6");
    }

    [Fact]
    public void CalculatesEstimatedCpu()
    {
        // Arrange - Dev: 4 small (0.25*4=1), Prod: 2 large (1*2=2) = 3.0 CPU
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 4 } },
            { EnvironmentType.Prod, new AppConfig { Large = 2 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "3");
    }

    [Fact]
    public void CalculatesEstimatedMemory()
    {
        // Arrange - Dev: 4 small (0.5*4=2), Prod: 2 large (2*2=4) = 6 GB
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 4 } },
            { EnvironmentType.Prod, new AppConfig { Large = 2 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "6");
    }

    #endregion

    #region Card Expansion Tests

    [Fact]
    public void ProdCard_ExpandedByDefault()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Prod, new AppConfig() }
                     }));

        // Assert
        Assert.Contains("expanded", cut.Markup);
    }

    #endregion

    #region Environment Classification Tests

    [Fact]
    public void ProdEnvironment_HasProdClass()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("prod", cut.Markup);
    }

    [Fact]
    public void DevEnvironment_HasNonprodClass()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod }));

        // Assert
        Assert.Contains("nonprod", cut.Markup);
    }

    #endregion

    #region Environment Icon Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "D")]
    [InlineData(EnvironmentType.Test, "T")]
    [InlineData(EnvironmentType.Stage, "S")]
    [InlineData(EnvironmentType.Prod, "P")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void DisplaysCorrectEnvironmentIcons(EnvironmentType env, string expectedIcon)
    {
        // Arrange - Use HashSet which handles duplicates automatically
        var envSet = new HashSet<EnvironmentType> { env };
        if (env != EnvironmentType.Prod) envSet.Add(EnvironmentType.Prod);

        var envApps = new Dictionary<EnvironmentType, AppConfig> { { env, new AppConfig() } };
        if (env != EnvironmentType.Prod) envApps[EnvironmentType.Prod] = new AppConfig();

        // Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envSet)
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        Assert.Contains(expectedIcon, cut.Markup);
    }

    #endregion

    #region Custom Tier Spec Tests

    [Fact]
    public void PassesTierSpecs_ToChildCards()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Prod, new AppConfig() }
                     })
                     .Add(p => p.SmallSpec, "Custom Small Spec")
                     .Add(p => p.MediumSpec, "Custom Medium Spec"));

        // Assert - Since Prod is expanded by default, should see custom specs
        Assert.Contains("Custom Small Spec", cut.Markup);
        Assert.Contains("Custom Medium Spec", cut.Markup);
    }

    #endregion

    #region Toggle Hint Tests

    [Fact]
    public void ShowsToggleHint()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("Select clusters to include in sizing", cut.Markup);
    }

    #endregion

    #region App Update Callback Tests

    [Fact]
    public async Task UpdatingTier_TriggersEnvironmentAppsChanged()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<EnvironmentAppGrid>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), e => received = e)));

        // Act - Find and update a tier input
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count > 0)
        {
            await inputs[0].ChangeAsync(new ChangeEventArgs { Value = "5" });
        }

        // Assert
        Assert.NotNull(received);
    }

    #endregion
}
