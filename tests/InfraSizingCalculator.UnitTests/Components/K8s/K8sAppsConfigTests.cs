using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.K8s;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.K8s;

public class K8sAppsConfigTests : TestContext
{

    #region Single Cluster Mode Tests

    [Fact]
    public void SingleCluster_RendersTierCards()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 5, Medium = 3, Large = 2, XLarge = 1 }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps));

        // Assert
        cut.Find(".tier-cards").Should().NotBeNull();
        cut.FindAll(".tier-card").Count.Should().Be(4); // Small, Medium, Large, XLarge
    }

    [Theory]
    [InlineData(AppTier.Small, "0.25 CPU, 0.5 GB")]
    [InlineData(AppTier.Medium, "0.5 CPU, 1 GB")]
    [InlineData(AppTier.Large, "1 CPU, 2 GB")]
    [InlineData(AppTier.XLarge, "2 CPU, 4 GB")]
    public void GetTierSpec_ReturnsCorrectSpec_WhenNoCustomFunc(AppTier tier, string expected)
    {
        // Arrange & Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>()));

        // Assert - check via rendered content
        var tierSpecs = cut.FindAll(".tier-spec");
        tierSpecs.Should().ContainSingle(e => e.TextContent.Contains(expected));
    }

    [Fact]
    public void GetTierSpec_UsesCustomFunc_WhenProvided()
    {
        // Arrange
        Func<AppTier, string> customFunc = tier => $"Custom-{tier}";

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>())
            .Add(p => p.GetTierSpecFunc, customFunc));

        // Assert
        var tierSpecs = cut.FindAll(".tier-spec");
        tierSpecs.Should().Contain(e => e.TextContent == "Custom-Small");
    }

    [Fact]
    public void SingleCluster_DisplaysAppCounts()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 10, Medium = 5, Large = 2, XLarge = 1 }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        inputs.Should().HaveCount(4);
        inputs[0].GetAttribute("value").Should().Be("10"); // Small
        inputs[1].GetAttribute("value").Should().Be("5");  // Medium
        inputs[2].GetAttribute("value").Should().Be("2");  // Large
        inputs[3].GetAttribute("value").Should().Be("1");  // XLarge
    }

    [Fact]
    public async Task SingleCluster_SetTotalApps_UpdatesAllEnvironments()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Dev] = new AppConfig(),
            [EnvironmentType.Prod] = new AppConfig()
        };
        Dictionary<EnvironmentType, AppConfig>? updatedApps = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, apps => updatedApps = apps)));

        // Act - change small apps count
        var smallInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => smallInput.Change(new ChangeEventArgs { Value = "15" }));

        // Assert - both environments should have been updated
        updatedApps.Should().NotBeNull();
        updatedApps![EnvironmentType.Dev].Small.Should().Be(15);
        updatedApps[EnvironmentType.Prod].Small.Should().Be(15);
    }

    #endregion

    #region Multi-Cluster Mode Tests

    [Fact]
    public void MultiCluster_RendersHorizontalAccordion()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>()));

        // Assert
        cut.Find(".multi-cluster-header").Should().NotBeNull();
    }

    [Fact]
    public void MultiCluster_ShowsHeaderStats()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Dev] = new AppConfig { Small = 4, Medium = 2 },
            [EnvironmentType.Prod] = new AppConfig { Small = 6, Medium = 3 }
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps));

        // Assert - total apps: 4+2+6+3 = 15
        var statItems = cut.FindAll(".stat-item");
        statItems.Should().HaveCount(3); // Total Apps, Est. CPU, Est. RAM

        var totalAppsValue = statItems[0].QuerySelector(".stat-value")?.TextContent;
        totalAppsValue.Should().Be("15");
    }

    [Fact]
    public void MultiCluster_CalculatesEstimatedCpu()
    {
        // Arrange - CPU: Small=0.25, Medium=0.5, Large=1.0, XLarge=2.0
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 4, Medium = 2, Large = 1, XLarge = 1 }
        };
        // Expected: 4*0.25 + 2*0.5 + 1*1.0 + 1*2.0 = 1 + 1 + 1 + 2 = 5.0

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var cpuValue = statItems[1].QuerySelector(".stat-value")?.TextContent;
        cpuValue.Should().Be("5.0");
    }

    [Fact]
    public void MultiCluster_CalculatesEstimatedMemory()
    {
        // Arrange - Memory: Small=0.5, Medium=1.0, Large=2.0, XLarge=4.0
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig { Small = 2, Medium = 2, Large = 1, XLarge = 1 }
        };
        // Expected: 2*0.5 + 2*1.0 + 1*2.0 + 1*4.0 = 1 + 2 + 2 + 4 = 9.0

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps));

        // Assert
        var statItems = cut.FindAll(".stat-item");
        var ramValue = statItems[2].QuerySelector(".stat-value")?.TextContent;
        ramValue.Should().Be("9");
    }

    #endregion

    #region Environment Class Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvClass_ReturnsCorrectClass(EnvironmentType env, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig> { [env] = new AppConfig() }));

        // Assert - look for the env class in panel elements
        var markup = cut.Markup;
        markup.Should().Contain(expectedClass);
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
        // Arrange - we can verify through the rendered "Production"/"Non-Production" label
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig> { [env] = new AppConfig() }));

        // Assert
        var markup = cut.Markup;
        if (expected)
        {
            markup.Should().Contain("Production");
        }
        else
        {
            markup.Should().Contain("Non-Production");
        }
    }

    #endregion

    #region ParseInt Edge Cases

    [Fact]
    public async Task ParseInt_HandlesNullValue()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig()
        };
        Dictionary<EnvironmentType, AppConfig>? result = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, apps => result = apps)));

        // Act - pass null via change event
        var input = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => input.Change(new ChangeEventArgs { Value = null }));

        // Assert - should default to 0
        result.Should().NotBeNull();
        result![EnvironmentType.Prod].Small.Should().Be(0);
    }

    [Fact]
    public async Task ParseInt_HandlesNegativeValue()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig()
        };
        Dictionary<EnvironmentType, AppConfig>? result = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, apps => result = apps)));

        // Act - pass negative value
        var input = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => input.Change(new ChangeEventArgs { Value = "-5" }));

        // Assert - should clamp to 0
        result.Should().NotBeNull();
        result![EnvironmentType.Prod].Small.Should().Be(0);
    }

    [Fact]
    public async Task ParseInt_HandlesInvalidString()
    {
        // Arrange
        var envApps = new Dictionary<EnvironmentType, AppConfig>
        {
            [EnvironmentType.Prod] = new AppConfig()
        };
        Dictionary<EnvironmentType, AppConfig>? result = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, apps => result = apps)));

        // Act - pass invalid string
        var input = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => input.Change(new ChangeEventArgs { Value = "not a number" }));

        // Assert - should default to 0
        result.Should().NotBeNull();
        result![EnvironmentType.Prod].Small.Should().Be(0);
    }

    #endregion

    #region Environment Ordering Tests

    [Fact]
    public void OrderedEnvironments_ReturnsInOrder()
    {
        // Arrange - add environments in random order
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev,
            EnvironmentType.DR,
            EnvironmentType.Test
        };

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentApps, new Dictionary<EnvironmentType, AppConfig>()));

        // Assert - should be ordered: Dev < Test < Prod < DR (enum order)
        var markup = cut.Markup;
        var devIndex = markup.IndexOf("env-dev");
        var testIndex = markup.IndexOf("env-test");
        var prodIndex = markup.IndexOf("env-prod");
        var drIndex = markup.IndexOf("env-dr");

        devIndex.Should().BeLessThan(testIndex);
        testIndex.Should().BeLessThan(prodIndex);
        prodIndex.Should().BeLessThan(drIndex);
    }

    #endregion

    #region GetEnvConfig Edge Cases

    [Fact]
    public void GetEnvConfig_ReturnsNewAppConfig_WhenEnvNotInDictionary()
    {
        // Arrange - empty dictionary
        var envApps = new Dictionary<EnvironmentType, AppConfig>();

        // Act
        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps));

        // Assert - should render with zero values (default AppConfig)
        var statItems = cut.FindAll(".stat-item");
        var totalAppsValue = statItems[0].QuerySelector(".stat-value")?.TextContent;
        totalAppsValue.Should().Be("0");
    }

    #endregion

    #region UpdateEnvTier Tests

    [Fact]
    public async Task UpdateEnvTier_CreatesAppConfig_WhenNotExists()
    {
        // Arrange - empty apps dictionary
        var envApps = new Dictionary<EnvironmentType, AppConfig>();
        Dictionary<EnvironmentType, AppConfig>? result = null;

        var cut = RenderComponent<K8sAppsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.EnvironmentApps, envApps)
            .Add(p => p.EnvironmentAppsChanged, EventCallback.Factory.Create<Dictionary<EnvironmentType, AppConfig>>(
                this, apps => result = apps)));

        // Act - expand accordion and update a tier (this tests UpdateEnvTier creating new entry)
        // Find the tier input and change it
        var tierInputs = cut.FindAll("input[type='number']");
        if (tierInputs.Any())
        {
            await cut.InvokeAsync(() => tierInputs[0].Change(new ChangeEventArgs { Value = "10" }));
        }

        // Assert - should create the AppConfig entry
        result.Should().NotBeNull();
        result!.ContainsKey(EnvironmentType.Prod).Should().BeTrue();
    }

    #endregion
}
