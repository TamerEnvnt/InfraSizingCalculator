using Bunit;
using InfraSizingCalculator.Components.K8s;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.K8s;

/// <summary>
/// Tests for K8sAppsConfig Blazor component.
/// Tests single/multi-cluster modes, tier specs, and app count callbacks.
/// </summary>
public class K8sAppsConfigTests : TestContext
{
    #region Single Cluster Mode Tests

    [Fact]
    public void SingleCluster_RendersTierCards()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var tierCards = cut.FindAll(".tier-card");
        Assert.Equal(4, tierCards.Count); // S, M, L, XL
        Assert.Contains(tierCards, c => c.TextContent.Contains("Small"));
        Assert.Contains(tierCards, c => c.TextContent.Contains("Medium"));
        Assert.Contains(tierCards, c => c.TextContent.Contains("Large"));
        Assert.Contains(tierCards, c => c.TextContent.Contains("XLarge"));
    }

    [Fact]
    public void SingleCluster_DisplaysTierSpecs()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert - Check default tier specs are displayed
        var markup = cut.Markup;
        Assert.Contains("0.25 CPU", markup);
        Assert.Contains("0.5 CPU", markup);
        Assert.Contains("1 CPU", markup);
        Assert.Contains("2 CPU", markup);
    }

    [Fact]
    public async Task SingleCluster_SetTotalApps_TriggersCallback()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), c => received = c)));

        // Act
        var input = cut.Find(".tier-card.small input");
        await input.ChangeAsync(new ChangeEventArgs { Value = "5" });

        // Assert
        Assert.NotNull(received);
        Assert.Equal(5, received[EnvironmentType.Prod].Small);
    }

    #endregion

    #region Multi-Cluster Mode Tests

    [Fact]
    public void MultiCluster_RendersAccordion()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 2 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 3 } }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        Assert.Contains("Multi-Cluster Mode", cut.Markup);
        Assert.Contains("env-apps-accordion", cut.Markup);
    }

    [Fact]
    public void MultiCluster_DisplaysTotalStats()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 4, Medium = 2 } },
            { EnvironmentType.Prod, new AppConfig { Large = 2, XLarge = 1 } }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert - Total apps = 4 + 2 + 2 + 1 = 9
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "9");
    }

    [Fact]
    public void MultiCluster_CalculatesEstimatedCpu()
    {
        // Arrange - Dev: 4 small (0.25*4=1), Prod: 2 large (1*2=2) = 3.0 total
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 4 } },
            { EnvironmentType.Prod, new AppConfig { Large = 2 } }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "3.0");
    }

    [Fact]
    public void MultiCluster_CalculatesEstimatedMemory()
    {
        // Arrange - Dev: 4 small (0.5*4=2), Prod: 2 large (2*2=4) = 6 GB total
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 4 } },
            { EnvironmentType.Prod, new AppConfig { Large = 2 } }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "6");
    }

    #endregion

    #region Environment Class Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvClass_ReturnsCorrectCss(EnvironmentType env, string expectedClass)
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { env, new AppConfig() }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        Assert.Contains(expectedClass, cut.Markup);
    }

    #endregion

    #region Custom Tier Spec Tests

    [Fact]
    public void CustomTierSpecFunc_UsedWhenProvided()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Func<AppTier, string> customSpec = tier => $"Custom-{tier}";

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.GetTierSpecFunc, customSpec));

        // Assert
        Assert.Contains("Custom-Small", cut.Markup);
        Assert.Contains("Custom-Medium", cut.Markup);
        Assert.Contains("Custom-Large", cut.Markup);
        Assert.Contains("Custom-XLarge", cut.Markup);
    }

    #endregion

    #region Prod Environment Detection Tests

    [Fact]
    public void IsProdEnv_ShowsProductionLabel()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        Assert.Contains("Production", cut.Markup);
    }

    [Fact]
    public void IsNonProdEnv_ShowsNonProductionLabel()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig() }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        Assert.Contains("Non-Production", cut.Markup);
    }

    [Fact]
    public void DREnv_ShowsProductionLabel()
    {
        // Arrange - DR is treated as production
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.DR, new AppConfig() }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.DR })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        Assert.Contains("Production", cut.Markup);
        Assert.Contains("env-dr", cut.Markup);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void SingleCluster_GetTotalApps_WithMissingEnv_ReturnsZero()
    {
        // Arrange - empty env apps dictionary
        var envApps = new Dictionary<EnvironmentType, AppConfig>();

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert - All inputs should show 0
        var inputs = cut.FindAll(".tier-card input");
        foreach (var input in inputs)
        {
            Assert.Equal("0", input.GetAttribute("value"));
        }
    }

    [Fact]
    public void MultiCluster_GetEnvTotalApps_WithMissingEnv_ReturnsZero()
    {
        // Arrange - enabled env not in dictionary
        var envApps = new Dictionary<EnvironmentType, AppConfig>();

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert - Stats should show 0
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "0");
    }

    [Fact]
    public void MultiCluster_WithEmptyEnvironments_DoesNotExpand()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>();

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType>())
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert - No accordion panels should exist
        Assert.DoesNotContain("accordion-panel", cut.Markup.ToLower());
    }

    [Fact]
    public async Task SingleCluster_SetTotalApps_ForAllTiers()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), c => received = c)));

        // Act - Set each tier
        var inputs = cut.FindAll(".tier-card input");
        await inputs[0].ChangeAsync(new ChangeEventArgs { Value = "5" });  // Small
        await inputs[1].ChangeAsync(new ChangeEventArgs { Value = "3" });  // Medium
        await inputs[2].ChangeAsync(new ChangeEventArgs { Value = "2" });  // Large
        await inputs[3].ChangeAsync(new ChangeEventArgs { Value = "1" });  // XLarge

        // Assert
        Assert.NotNull(received);
        Assert.Equal(5, received[EnvironmentType.Prod].Small);
        Assert.Equal(3, received[EnvironmentType.Prod].Medium);
        Assert.Equal(2, received[EnvironmentType.Prod].Large);
        Assert.Equal(1, received[EnvironmentType.Prod].XLarge);
    }

    [Fact]
    public async Task SingleCluster_SetTotalApps_PropagatesAllEnabledEnvs()
    {
        // Arrange - Multiple environments should all get same value
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig() },
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), c => received = c)));

        // Act
        var input = cut.Find(".tier-card.medium input");
        await input.ChangeAsync(new ChangeEventArgs { Value = "7" });

        // Assert - Both environments should have same value
        Assert.NotNull(received);
        Assert.Equal(7, received[EnvironmentType.Dev].Medium);
        Assert.Equal(7, received[EnvironmentType.Prod].Medium);
    }

    [Fact]
    public async Task ParseInt_WithNullValue_ReturnsZero()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), c => received = c)));

        // Act
        var input = cut.Find(".tier-card.small input");
        await input.ChangeAsync(new ChangeEventArgs { Value = null });

        // Assert
        Assert.NotNull(received);
        Assert.Equal(0, received[EnvironmentType.Prod].Small);
    }

    [Fact]
    public async Task ParseInt_WithNegativeValue_ReturnsZero()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), c => received = c)));

        // Act
        var input = cut.Find(".tier-card.small input");
        await input.ChangeAsync(new ChangeEventArgs { Value = "-5" });

        // Assert - Negative values clamp to 0
        Assert.NotNull(received);
        Assert.Equal(0, received[EnvironmentType.Prod].Small);
    }

    [Fact]
    public void MultiCluster_EstimatedCpu_IncludesAllTiers()
    {
        // Arrange - Test all tiers: S=0.25, M=0.5, L=1.0, XL=2.0
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 4, Medium = 2, Large = 1, XLarge = 1 } }
        };
        // Expected: 4*0.25 + 2*0.5 + 1*1.0 + 1*2.0 = 1 + 1 + 1 + 2 = 5.0

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "5.0");
    }

    [Fact]
    public void MultiCluster_EstimatedMemory_IncludesAllTiers()
    {
        // Arrange - Test all tiers: S=0.5, M=1.0, L=2.0, XL=4.0
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 2, Medium = 2, Large = 1, XLarge = 1 } }
        };
        // Expected: 2*0.5 + 2*1.0 + 1*2.0 + 1*4.0 = 1 + 2 + 2 + 4 = 9 GB

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "9");
    }

    #endregion
}
