using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.VM;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.VM;

/// <summary>
/// Tests for VMServerRolesConfig component - VM Server Roles Configuration panel
/// </summary>
public class VMServerRolesConfigTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void VMServerRolesConfig_RendersMainContainer()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".vm-server-roles-config").Should().NotBeNull();
    }

    [Fact]
    public void VMServerRolesConfig_RendersHeader()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".server-roles-header").TextContent.Should().Contain("Server Roles");
    }

    [Fact]
    public void VMServerRolesConfig_RendersHeaderStats()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".header-stats").Should().NotBeNull();
        cut.FindAll(".stat-item").Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public void VMServerRolesConfig_RendersAccordion()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Find(".h-accordion").Should().NotBeNull();
    }

    #endregion

    #region Stats Display Tests

    [Fact]
    public void VMServerRolesConfig_ShowsTotalRolesCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        // Add 2 roles to Dev, 3 to Prod
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App });
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.Database });
        configs[EnvironmentType.Prod].Roles.Add(new VMRoleConfig { Role = ServerRole.App });
        configs[EnvironmentType.Prod].Roles.Add(new VMRoleConfig { Role = ServerRole.Database });
        configs[EnvironmentType.Prod].Roles.Add(new VMRoleConfig { Role = ServerRole.Web });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Total roles should be 5
        var statValues = cut.FindAll(".stat-value");
        statValues[0].TextContent.Should().Contain("5");
    }

    [Fact]
    public void VMServerRolesConfig_ShowsTotalInstancesCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Prod].Roles.Add(new VMRoleConfig { Role = ServerRole.App, InstanceCount = 3 });
        configs[EnvironmentType.Prod].Roles.Add(new VMRoleConfig { Role = ServerRole.Database, InstanceCount = 2 });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Total instances should be 5
        var statValues = cut.FindAll(".stat-value");
        statValues[1].TextContent.Should().Contain("5");
    }

    [Fact]
    public void VMServerRolesConfig_ZeroRoles_ShowsZeroStats()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        // No roles added

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var statValues = cut.FindAll(".stat-value");
        statValues[0].TextContent.Should().Contain("0");
        statValues[1].TextContent.Should().Contain("0");
    }

    #endregion

    #region Environment Panel Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    public void VMServerRolesConfig_RendersEnvironmentPanel(EnvironmentType env)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".h-accordion-panel").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_RendersMultipleEnvironments()
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
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var panels = cut.FindAll(".h-accordion-panel");
        panels.Should().HaveCount(3);
    }

    [Fact]
    public void VMServerRolesConfig_EnvironmentsOrderedByEnumValue()
    {
        // Arrange - Add in non-sorted order
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Prod,
            EnvironmentType.Dev,
            EnvironmentType.Stage
        };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should be ordered: Dev (0), Stage (2), Prod (3)
        var panels = cut.FindAll(".h-accordion-panel");
        panels.Should().HaveCount(3);
    }

    [Fact]
    public void VMServerRolesConfig_DefaultExpandsFirstEnvironment()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod
        };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var panels = cut.FindAll(".h-accordion-panel");
        panels.Should().NotBeEmpty();
    }

    #endregion

    #region Environment Class Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.LifeTime, "env-lifetime")]
    public void VMServerRolesConfig_AppliesCorrectEnvironmentClass(EnvironmentType env, string expectedClass)
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { env };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Markup.Should().Contain(expectedClass);
    }

    [Fact]
    public void VMServerRolesConfig_ShowsProdAsProductionType()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Markup.Should().Contain("Production");
    }

    [Fact]
    public void VMServerRolesConfig_ShowsDevAsNonProdType()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Markup.Should().Contain("Non-Prod");
    }

    #endregion

    #region Role Grid Tests (Generic Roles)

    [Fact]
    public void VMServerRolesConfig_NoTechRoles_ShowsGenericRoles()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act - No AvailableTechnologyRoles provided
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert - Should render generic ServerRole enum values
        cut.FindAll(".role-chip").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_ActiveRole_HasActiveClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, InstanceCount = 2 });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.FindAll(".role-chip.active").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_ActiveRole_ShowsInstanceCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, InstanceCount = 3 });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.FindAll(".role-count").Should().Contain(c => c.TextContent.Contains("x3"));
    }

    #endregion

    #region Technology-Specific Role Tests

    [Fact]
    public void VMServerRolesConfig_WithTechRoles_ShowsTechRoles()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = CreateSampleTechnologyRoles();

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        var chips = cut.FindAll(".role-chip");
        chips.Should().HaveCount(techRoles.Count);
    }

    [Fact]
    public void VMServerRolesConfig_OptionalRole_HasOptionalClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new() { Id = "optional-role", Name = "Optional", Description = "Optional role", Required = false }
        };

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        cut.FindAll(".role-chip.optional").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_ActiveTechRole_MatchesByRoleId()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new() { Id = "app-server", Name = "App Server", Description = "Application server" }
        };
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { RoleId = "app-server", InstanceCount = 2 });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        cut.FindAll(".role-chip.active").Should().HaveCount(1);
    }

    #endregion

    #region Active Roles Configuration Tests

    [Fact]
    public void VMServerRolesConfig_HasActiveRoles_ShowsActiveRolesSection()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.FindAll(".active-roles-section").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_NoActiveRoles_ShowsEmptyMessage()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        // No roles added

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.FindAll(".no-roles-message").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_ActiveRole_ShowsSizeSelector()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Medium });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        var selects = cut.FindAll("select");
        selects.Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_ActiveRole_ShowsAllTierOptions()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert - Should have all AppTier values
        var tierSelect = cut.Find("select");
        foreach (var tier in Enum.GetValues<AppTier>())
        {
            tierSelect.InnerHtml.Should().Contain(tier.ToString());
        }
    }

    [Fact]
    public void VMServerRolesConfig_ActiveRole_ShowsRemoveButton()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.FindAll(".role-remove").Should().NotBeEmpty();
    }

    #endregion

    #region EventCallback Tests

    [Fact]
    public async Task VMServerRolesConfig_ToggleTechRole_InvokesCallback()
    {
        // Arrange
        (EnvironmentType env, TechnologyServerRole role)? toggledRole = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new() { Id = "app-server", Name = "App Server", Description = "Application server" }
        };

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles)
            .Add(p => p.OnToggleTechRoleRequested,
                EventCallback.Factory.Create<(EnvironmentType, TechnologyServerRole)>(
                    this, r => toggledRole = r)));

        // Act
        var roleChip = cut.Find(".role-chip");
        await cut.InvokeAsync(() => roleChip.Click());

        // Assert
        toggledRole.Should().NotBeNull();
        toggledRole!.Value.env.Should().Be(EnvironmentType.Dev);
        toggledRole.Value.role.Id.Should().Be("app-server");
    }

    [Fact]
    public async Task VMServerRolesConfig_ToggleGenericRole_InvokesCallback()
    {
        // Arrange
        (EnvironmentType env, ServerRole role)? toggledRole = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>())
            .Add(p => p.OnToggleRoleRequested,
                EventCallback.Factory.Create<(EnvironmentType, ServerRole)>(
                    this, r => toggledRole = r)));

        // Act
        var roleChip = cut.Find(".role-chip");
        await cut.InvokeAsync(() => roleChip.Click());

        // Assert
        toggledRole.Should().NotBeNull();
        toggledRole!.Value.env.Should().Be(EnvironmentType.Dev);
    }

    [Fact]
    public async Task VMServerRolesConfig_RemoveRole_InvokesCallback()
    {
        // Arrange
        (EnvironmentType env, VMRoleConfig role)? removedRole = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App });

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>())
            .Add(p => p.OnRemoveRoleRequested,
                EventCallback.Factory.Create<(EnvironmentType, VMRoleConfig)>(
                    this, r => removedRole = r)));

        // Act
        var removeBtn = cut.Find(".role-remove");
        await cut.InvokeAsync(() => removeBtn.Click());

        // Assert
        removedRole.Should().NotBeNull();
        removedRole!.Value.env.Should().Be(EnvironmentType.Dev);
        removedRole.Value.role.Role.Should().Be(ServerRole.App);
    }

    [Fact]
    public async Task VMServerRolesConfig_ChangingSize_InvokesConfigsChanged()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Small });

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>())
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act
        var tierSelect = cut.Find("select");
        await cut.InvokeAsync(() => tierSelect.Change(AppTier.Large.ToString()));

        // Assert
        updatedConfigs.Should().NotBeNull();
    }

    #endregion

    #region Max Instances Tests

    [Fact]
    public void VMServerRolesConfig_SingleInstanceRole_ShowsFixedCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, InstanceCount = 1 });

        // Act - GetMaxInstances returns 1 (single instance role)
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>())
            .Add(p => p.GetMaxInstances, _ => 1));

        // Assert
        cut.FindAll(".fixed-count").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_MultiInstanceRole_ShowsInput()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, InstanceCount = 2 });

        // Act - GetMaxInstances returns > 1
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>())
            .Add(p => p.GetMaxInstances, _ => 10));

        // Assert
        cut.FindAll("input[type='number']").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_NoGetMaxInstances_DefaultsTo10()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App, InstanceCount = 2 });

        // Act - No GetMaxInstances provided
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert - Should show input (not fixed) since default max is 10
        var inputs = cut.FindAll("input[type='number']");
        inputs.Should().NotBeEmpty();
        inputs[0].GetAttribute("max").Should().Be("10");
    }

    #endregion

    #region Role Name Display Tests

    [Fact]
    public void VMServerRolesConfig_RoleWithCustomName_ShowsCustomName()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig
        {
            Role = ServerRole.App,
            RoleName = "Custom App Server"
        });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.Markup.Should().Contain("Custom App Server");
    }

    #endregion

    #region Environment Badge Tests

    [Fact]
    public void VMServerRolesConfig_ShowsRoleCountBadge()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.App });
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig { Role = ServerRole.Database });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, Enumerable.Empty<TechnologyServerRole>()));

        // Assert
        cut.FindAll(".env-badge").Should().NotBeEmpty();
        cut.FindAll(".badge-count").Should().Contain(b => b.TextContent.Contains("2"));
    }

    #endregion

    #region Empty/Null State Tests

    [Fact]
    public void VMServerRolesConfig_EmptyEnabledEnvironments_RendersWithoutError()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>();
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();

        // Act & Assert - Should not throw
        var action = () => RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        action.Should().NotThrow();
    }

    [Fact]
    public void VMServerRolesConfig_MissingConfig_HandlesGracefully()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>(); // No config for Dev

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should render without error, showing 0 roles
        cut.Find(".vm-server-roles-config").Should().NotBeNull();
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

    private static List<TechnologyServerRole> CreateSampleTechnologyRoles()
    {
        return new List<TechnologyServerRole>
        {
            new() { Id = "controller", Name = "Controller", Description = "Deployment controller", Required = true },
            new() { Id = "frontend", Name = "Front-End", Description = "Front-end server", Required = true, ScaleHorizontally = true },
            new() { Id = "database", Name = "Database", Description = "Database server", Required = true },
            new() { Id = "cache", Name = "Cache", Description = "Caching server", Required = false }
        };
    }

    #endregion
}
