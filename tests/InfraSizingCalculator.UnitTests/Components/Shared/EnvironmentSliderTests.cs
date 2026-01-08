using Bunit;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

/// <summary>
/// Tests for EnvironmentSlider Blazor component.
/// Tests slider navigation, tier inputs, stats calculations, and environment icons.
/// </summary>
public class EnvironmentSliderTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void RendersSliderContainer()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("env-slider", cut.Markup);
        Assert.Contains("slider-container", cut.Markup);
    }

    [Fact]
    public void RendersModeBanner()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("Multi-Cluster Mode", cut.Markup);
        Assert.Contains("Configure apps for each environment cluster", cut.Markup);
    }

    [Fact]
    public void RendersTierInputs()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Prod, new AppConfig() }
                     }));

        // Assert - All 4 tier panels rendered
        Assert.Contains("tier-panel small", cut.Markup);
        Assert.Contains("tier-panel medium", cut.Markup);
        Assert.Contains("tier-panel large", cut.Markup);
        Assert.Contains("tier-panel xlarge", cut.Markup);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void RendersNavigationDots_ForEachEnvironment()
    {
        // Arrange
        var envs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envs));

        // Assert
        var dots = cut.FindAll(".env-dot");
        Assert.Equal(3, dots.Count);
    }

    [Fact]
    public void NavigationButtons_DisabledAtBoundaries()
    {
        // Arrange - Single environment
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - Both buttons should be disabled at boundaries
        var prevButton = cut.Find(".slider-nav.prev");
        var nextButton = cut.Find(".slider-nav.next");

        Assert.Contains("disabled", prevButton.ClassName);
        Assert.Contains("disabled", nextButton.ClassName);
    }

    [Fact]
    public void ClickingDot_ChangesCurrentEnvironment()
    {
        // Arrange
        var envs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envs)
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Dev, new AppConfig() },
                         { EnvironmentType.Prod, new AppConfig() }
                     }));

        // Act - Click on Prod dot
        var prodDot = cut.FindAll(".env-dot").FirstOrDefault(d => d.TextContent.Contains("Prod"));
        prodDot?.Click();

        // Assert - Should show Prod environment
        Assert.Contains("Prod", cut.Find(".env-name").TextContent);
    }

    [Fact]
    public void ShowsNavigationHint()
    {
        // Arrange
        var envs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, envs)
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Dev, new AppConfig() },
                         { EnvironmentType.Prod, new AppConfig() }
                     }));

        // Assert
        Assert.Contains("1 of 2 environments", cut.Markup);
    }

    #endregion

    #region Stats Calculation Tests

    [Fact]
    public void CalculatesTotalApps()
    {
        // Arrange - 3 small + 2 medium = 5 total
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 3, Medium = 2 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "5");
    }

    [Fact]
    public void CalculatesEstimatedCpu()
    {
        // Arrange - 4 small (0.25*4=1) + 2 large (1*2=2) = 3.0 CPU
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 4, Large = 2 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "3.0");
    }

    [Fact]
    public void CalculatesEstimatedMemory()
    {
        // Arrange - 4 small (0.5*4=2) + 2 large (2*2=4) = 6 GB
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig { Small = 4, Large = 2 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "6");
    }

    [Fact]
    public void AggregatesStats_AcrossMultipleEnvironments()
    {
        // Arrange - Dev: 2 small (0.5 CPU), Prod: 2 medium (1 CPU) = 1.5 total CPU
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 2 } },
            { EnvironmentType.Prod, new AppConfig { Medium = 2 } }
        };

        // Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps));

        // Assert - Total apps = 4
        var stats = cut.FindAll(".stat-value");
        Assert.Contains(stats, s => s.TextContent == "4");
    }

    #endregion

    #region Environment Icon Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "D")]
    [InlineData(EnvironmentType.Test, "T")]
    [InlineData(EnvironmentType.Stage, "S")]
    [InlineData(EnvironmentType.Prod, "P")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void GetEnvIcon_ReturnsCorrectIcon(EnvironmentType env, string expectedIcon)
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { env, new AppConfig() }
                     }));

        // Assert
        Assert.Contains(expectedIcon, cut.Find(".env-icon").TextContent);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvClass_ReturnsCorrectCss(EnvironmentType env, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { env, new AppConfig() }
                     }));

        // Assert
        Assert.Contains(expectedClass, cut.Markup);
    }

    #endregion

    #region Tier Update Tests

    [Fact]
    public async Task TierUpdate_TriggersCallback()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Prod, new AppConfig() }
        };
        Dictionary<EnvironmentType, AppConfig>? received = null;

        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, envApps)
                     .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(new object(), c => received = c)));

        // Act
        var smallInput = cut.Find(".tier-panel.small input");
        await smallInput.ChangeAsync(new ChangeEventArgs { Value = "5" });

        // Assert
        Assert.NotNull(received);
        Assert.Equal(5, received[EnvironmentType.Prod].Small);
    }

    #endregion

    #region Prod/NonProd Classification Tests

    [Fact]
    public void ProdEnvironment_ShowsProductionLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Prod, new AppConfig() }
                     }));

        // Assert
        Assert.Contains("Production Environment", cut.Markup);
    }

    [Fact]
    public void DevEnvironment_ShowsNonProductionLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Dev, new AppConfig() }
                     }));

        // Assert
        Assert.Contains("Non-Production Environment", cut.Markup);
    }

    [Fact]
    public void DrEnvironment_ShowsProductionLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.DR })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.DR, new AppConfig() }
                     }));

        // Assert
        Assert.Contains("Production Environment", cut.Markup);
    }

    #endregion

    #region Custom Tier Spec Tests

    [Fact]
    public void DisplaysCustomTierSpecs()
    {
        // Arrange & Act
        var cut = RenderComponent<EnvironmentSlider>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>
                     {
                         { EnvironmentType.Prod, new AppConfig() }
                     })
                     .Add(p => p.SmallSpec, "Custom Small")
                     .Add(p => p.MediumSpec, "Custom Medium")
                     .Add(p => p.LargeSpec, "Custom Large")
                     .Add(p => p.XLargeSpec, "Custom XL"));

        // Assert
        Assert.Contains("Custom Small", cut.Markup);
        Assert.Contains("Custom Medium", cut.Markup);
        Assert.Contains("Custom Large", cut.Markup);
        Assert.Contains("Custom XL", cut.Markup);
    }

    #endregion
}
