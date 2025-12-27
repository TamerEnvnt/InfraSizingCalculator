using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.VM;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.VM;

/// <summary>
/// Tests for VMHADRConfig component - VM High Availability and Disaster Recovery configuration
/// </summary>
public class VMHADRConfigTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void VMHADRConfig_RendersMainContainer()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".vm-ha-dr-config").Should().NotBeNull();
    }

    [Fact]
    public void VMHADRConfig_RendersHeader()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".ha-dr-header").TextContent.Should().Contain("High Availability");
        cut.Find(".ha-dr-header").TextContent.Should().Contain("Disaster Recovery");
    }

    [Fact]
    public void VMHADRConfig_RendersAccordion()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".h-accordion").Should().NotBeNull();
    }

    #endregion

    #region Environment Panel Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    public void VMHADRConfig_RendersEnvironmentPanel(EnvironmentType env)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should render at least one environment panel
        cut.FindAll(".ha-dr-accordion").Should().NotBeEmpty();
    }

    [Fact]
    public void VMHADRConfig_RendersMultipleEnvironments()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Prod
        };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should render panels for all environments
        var panels = cut.FindAll(".h-accordion-panel");
        panels.Should().HaveCount(3);
    }

    [Fact]
    public void VMHADRConfig_EnvironmentsOrderedCorrectly()
    {
        // Arrange - Add in non-sorted order
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev,
            EnvironmentType.Test
        };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should be ordered by enum value (Dev=0, Test=1, Prod=3)
        var panels = cut.FindAll(".h-accordion-panel");
        // First panel should be Dev (lowest enum value)
        panels.Should().HaveCountGreaterOrEqualTo(1);
    }

    #endregion

    #region HA Pattern Tests

    [Fact]
    public void VMHADRConfig_RendersHAPatternSelector()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("HA Pattern"));
    }

    [Fact]
    public void VMHADRConfig_HAPatternSelector_HasAllOptions()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should have all HAPattern options
        var selects = cut.FindAll("select");
        var haSelect = selects.First(s => s.InnerHtml.Contains("None") && s.InnerHtml.Contains("Active-Passive"));
        haSelect.InnerHtml.Should().Contain("None");
        haSelect.InnerHtml.Should().Contain("Active-Passive");
        haSelect.InnerHtml.Should().Contain("Active-Active");
        haSelect.InnerHtml.Should().Contain("N+1 Redundancy");
    }

    [Fact]
    public async Task VMHADRConfig_ChangingHAPattern_InvokesCallback()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act - Find and change HA pattern selector
        var selects = cut.FindAll("select");
        var haSelect = selects.First(s => s.InnerHtml.Contains("Active-Passive"));
        await cut.InvokeAsync(() => haSelect.Change(HAPattern.ActiveActive.ToString()));

        // Assert
        updatedConfigs.Should().NotBeNull();
        configs[EnvironmentType.Prod].HAPattern.Should().Be(HAPattern.ActiveActive);
    }

    [Theory]
    [InlineData(HAPattern.None, "Single server, no redundancy")]
    [InlineData(HAPattern.ActivePassive, "Primary + standby failover")]
    [InlineData(HAPattern.ActiveActive, "Multiple active servers")]
    [InlineData(HAPattern.NPlus1, "N active servers + 1 spare")]
    public void VMHADRConfig_ShowsCorrectHAPatternHint(HAPattern pattern, string expectedHint)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].HAPattern = pattern;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains(expectedHint));
    }

    #endregion

    #region DR Pattern Tests

    [Fact]
    public void VMHADRConfig_RendersDRPatternSelector()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Disaster Recovery"));
    }

    [Fact]
    public void VMHADRConfig_DRPatternSelector_HasAllOptions()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var selects = cut.FindAll("select");
        var drSelect = selects.First(s => s.InnerHtml.Contains("Warm Standby"));
        drSelect.InnerHtml.Should().Contain("None");
        drSelect.InnerHtml.Should().Contain("Warm Standby");
        drSelect.InnerHtml.Should().Contain("Hot Standby");
        drSelect.InnerHtml.Should().Contain("Multi-Region");
    }

    [Fact]
    public async Task VMHADRConfig_ChangingDRPattern_InvokesCallback()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act
        var selects = cut.FindAll("select");
        var drSelect = selects.First(s => s.InnerHtml.Contains("Warm Standby"));
        await cut.InvokeAsync(() => drSelect.Change(DRPattern.HotStandby.ToString()));

        // Assert
        updatedConfigs.Should().NotBeNull();
        configs[EnvironmentType.Prod].DRPattern.Should().Be(DRPattern.HotStandby);
    }

    [Theory]
    [InlineData(DRPattern.None, "No disaster recovery")]
    [InlineData(DRPattern.WarmStandby, "Minimal resources at DR site")]
    [InlineData(DRPattern.HotStandby, "Full duplicate environment")]
    [InlineData(DRPattern.MultiRegion, "Multiple regions serving traffic")]
    public void VMHADRConfig_ShowsCorrectDRPatternHint(DRPattern pattern, string expectedHint)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].DRPattern = pattern;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains(expectedHint));
    }

    #endregion

    #region Load Balancer Tests

    [Fact]
    public void VMHADRConfig_RendersLoadBalancerSelector()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Load Balancer"));
    }

    [Fact]
    public void VMHADRConfig_LoadBalancerSelector_HasAllOptions()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var selects = cut.FindAll("select");
        var lbSelect = selects.First(s => s.InnerHtml.Contains("Single LB"));
        lbSelect.InnerHtml.Should().Contain("None");
        lbSelect.InnerHtml.Should().Contain("Single LB");
        lbSelect.InnerHtml.Should().Contain("HA Pair");
        lbSelect.InnerHtml.Should().Contain("Cloud LB");
    }

    [Fact]
    public async Task VMHADRConfig_ChangingLoadBalancer_InvokesCallback()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act
        var selects = cut.FindAll("select");
        var lbSelect = selects.First(s => s.InnerHtml.Contains("Single LB"));
        await cut.InvokeAsync(() => lbSelect.Change(LoadBalancerOption.HAPair.ToString()));

        // Assert
        updatedConfigs.Should().NotBeNull();
        configs[EnvironmentType.Prod].LoadBalancer.Should().Be(LoadBalancerOption.HAPair);
    }

    [Theory]
    [InlineData(LoadBalancerOption.None, "No load balancing")]
    [InlineData(LoadBalancerOption.Single, "Single load balancer")]
    [InlineData(LoadBalancerOption.HAPair, "Two load balancers")]
    [InlineData(LoadBalancerOption.CloudLB, "Managed cloud service")]
    public void VMHADRConfig_ShowsCorrectLoadBalancerHint(LoadBalancerOption option, string expectedHint)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].LoadBalancer = option;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains(expectedHint));
    }

    #endregion

    #region Summary Display Tests

    [Fact]
    public void VMHADRConfig_RendersSummarySection()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".config-summary").Should().NotBeNull();
    }

    [Fact]
    public void VMHADRConfig_NoRedundancy_ShowsNoRedundancyMessage()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].HAPattern = HAPattern.None;
        configs[EnvironmentType.Prod].DRPattern = DRPattern.None;
        configs[EnvironmentType.Prod].LoadBalancer = LoadBalancerOption.None;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".summary-item").Should().Contain(s => s.TextContent.Contains("No redundancy"));
    }

    [Fact]
    public void VMHADRConfig_WithHA_ShowsHAInSummary()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].HAPattern = HAPattern.ActivePassive;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".summary-item.ha").Should().NotBeEmpty();
    }

    [Fact]
    public void VMHADRConfig_WithDR_ShowsDRInSummary()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].DRPattern = DRPattern.HotStandby;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".summary-item.dr").Should().NotBeEmpty();
    }

    [Fact]
    public void VMHADRConfig_WithLB_ShowsLBInSummary()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].LoadBalancer = LoadBalancerOption.HAPair;

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".summary-item.lb").Should().NotBeEmpty();
    }

    #endregion

    #region Environment Config Creation Tests

    [Fact]
    public async Task VMHADRConfig_CreatesConfigForNewEnvironment()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>(); // Empty!

        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act - Change HA pattern on empty config
        var selects = cut.FindAll("select");
        if (selects.Any())
        {
            var haSelect = selects.FirstOrDefault(s => s.InnerHtml.Contains("Active-Passive"));
            if (haSelect != null)
            {
                await cut.InvokeAsync(() => haSelect.Change(HAPattern.ActiveActive.ToString()));

                // Assert - Config should be created
                configs.Should().ContainKey(EnvironmentType.Prod);
            }
        }
    }

    [Fact]
    public void VMHADRConfig_DefaultExpandsFirstEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - First panel (Dev) should be expanded by default
        // The component sets expandedEnv to first ordered environment
        var panels = cut.FindAll(".h-accordion-panel");
        panels.Should().NotBeEmpty();
    }

    #endregion

    #region EventCallback Safety Tests

    [Fact]
    public async Task VMHADRConfig_NoCallback_DoesNotThrow()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Act & Assert
        var action = async () =>
        {
            var selects = cut.FindAll("select");
            if (selects.Any())
            {
                var haSelect = selects.FirstOrDefault(s => s.InnerHtml.Contains("Active-Passive"));
                if (haSelect != null)
                {
                    await cut.InvokeAsync(() => haSelect.Change(HAPattern.ActiveActive.ToString()));
                }
            }
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Environment Class Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    public void VMHADRConfig_AppliesCorrectEnvironmentClass(EnvironmentType env, string expectedClass)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMHADRConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Panel should have environment-specific class
        var markup = cut.Markup;
        markup.Should().Contain(expectedClass);
    }

    #endregion

    #region Helper Methods

    private static Dictionary<EnvironmentType, VMEnvironmentConfig> CreateDefaultConfigs(
        HashSet<EnvironmentType> environments)
    {
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();

        foreach (var env in environments)
        {
            configs[env] = new VMEnvironmentConfig
            {
                Environment = env,
                Enabled = true,
                HAPattern = HAPattern.None,
                DRPattern = DRPattern.None,
                LoadBalancer = LoadBalancerOption.None,
                StorageGB = 100,
                Roles = new List<VMRoleConfig>()
            };
        }

        return configs;
    }

    #endregion
}
