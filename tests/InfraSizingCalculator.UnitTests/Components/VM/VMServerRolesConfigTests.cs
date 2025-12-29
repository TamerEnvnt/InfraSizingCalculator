using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.VM;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.VM;

/// <summary>
/// Tests for VMServerRolesConfig component - VM server roles configuration per environment
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
        cut.Find(".server-roles-header").TextContent.Should().Contain("Server Roles Configuration");
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
        cut.Find(".server-roles-accordion").Should().NotBeNull();
    }

    #endregion

    #region Header Stats Tests

    [Fact]
    public void VMServerRolesConfig_DisplaysTotalRolesInHeader()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        // Add roles to each environment
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.App));
        configs[EnvironmentType.Prod].Roles.Add(CreateRoleConfig(ServerRole.Database));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should show 3 total roles
        var stats = cut.FindAll(".stat-item");
        stats.Should().Contain(s => s.TextContent.Contains("3") && s.TextContent.Contains("Total Roles"));
    }

    [Fact]
    public void VMServerRolesConfig_DisplaysTotalInstancesInHeader()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web, instanceCount: 2));
        configs[EnvironmentType.Prod].Roles.Add(CreateRoleConfig(ServerRole.App, instanceCount: 3));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should show 5 total VMs (2 + 3)
        var stats = cut.FindAll(".stat-item");
        stats.Should().Contain(s => s.TextContent.Contains("5") && s.TextContent.Contains("Total VMs"));
    }

    [Fact]
    public void VMServerRolesConfig_ZeroRoles_DisplaysZeroInStats()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var stats = cut.FindAll(".stat-item");
        stats.Should().Contain(s => s.TextContent.Contains("0") && s.TextContent.Contains("Total Roles"));
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
            EnvironmentType.Test
        };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should be ordered by enum value (Dev=0, Test=1, Prod=3)
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

        // Assert - First environment (Dev) should be expanded
        cut.FindAll(".h-accordion-panel").Should().NotBeEmpty();
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
    public void VMServerRolesConfig_ProdEnvironment_ShowsProductionLabel()
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
    public void VMServerRolesConfig_NonProdEnvironment_ShowsNonProdLabel()
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

    #region Role Toggle Tests with Technology Roles

    [Fact]
    public async Task VMServerRolesConfig_ToggleTechRole_InvokesCallback()
    {
        // Arrange
        (EnvironmentType, TechnologyServerRole)? receivedToggle = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new TechnologyServerRole
            {
                Id = "web-server",
                Name = "Web Server",
                Description = "HTTP frontend"
            }
        };

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles)
            .Add(p => p.OnToggleTechRoleRequested,
                EventCallback.Factory.Create<(EnvironmentType, TechnologyServerRole)>(
                    this, t => receivedToggle = t)));

        // Act - Click on a technology role chip
        var roleChips = cut.FindAll(".role-chip");
        if (roleChips.Any())
        {
            await cut.InvokeAsync(() => roleChips.First().Click());

            // Assert
            receivedToggle.Should().NotBeNull();
            receivedToggle!.Value.Item1.Should().Be(EnvironmentType.Dev);
            receivedToggle.Value.Item2.Id.Should().Be("web-server");
        }
    }

    [Fact]
    public void VMServerRolesConfig_WithTechnologyRoles_RendersTechRoleChips()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new TechnologyServerRole { Id = "web", Name = "Web Server", Description = "HTTP" },
            new TechnologyServerRole { Id = "app", Name = "App Server", Description = "Logic" }
        };

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        cut.Markup.Should().Contain("Web Server");
        cut.Markup.Should().Contain("App Server");
    }

    [Fact]
    public void VMServerRolesConfig_ActiveTechRole_ShowsActiveClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new TechnologyServerRole { Id = "web-role", Name = "Web Server", Description = "HTTP" }
        };
        // Add a matching role to config
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig
        {
            RoleId = "web-role",
            Role = ServerRole.Web,
            InstanceCount = 1
        });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        var activeChips = cut.FindAll(".role-chip.active");
        activeChips.Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_OptionalTechRole_ShowsOptionalClass()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new TechnologyServerRole
            {
                Id = "cache",
                Name = "Cache Server",
                Description = "Caching",
                Required = false
            }
        };

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        var optionalChips = cut.FindAll(".role-chip.optional");
        optionalChips.Should().NotBeEmpty();
    }

    #endregion

    #region Role Toggle Tests with Generic Roles

    [Fact]
    public async Task VMServerRolesConfig_NoTechRoles_RendersGenericServerRoles()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        // No technology roles provided

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should render ServerRole enum values
        var roleChips = cut.FindAll(".role-chip");
        roleChips.Should().NotBeEmpty();
    }

    [Fact]
    public async Task VMServerRolesConfig_ToggleGenericRole_InvokesCallback()
    {
        // Arrange
        (EnvironmentType, ServerRole)? receivedToggle = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.OnToggleRoleRequested,
                EventCallback.Factory.Create<(EnvironmentType, ServerRole)>(
                    this, t => receivedToggle = t)));

        // Act - Click on a role chip
        var roleChips = cut.FindAll(".role-chip");
        if (roleChips.Any())
        {
            await cut.InvokeAsync(() => roleChips.First().Click());

            // Assert
            receivedToggle.Should().NotBeNull();
            receivedToggle!.Value.Item1.Should().Be(EnvironmentType.Dev);
        }
    }

    #endregion

    #region Active Roles Configuration Tests

    [Fact]
    public void VMServerRolesConfig_WithActiveRoles_RendersRoleDetailsSection()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".active-roles-section").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_NoActiveRoles_ShowsEmptyMessage()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Markup.Should().Contain("Click roles above to add");
    }

    [Fact]
    public void VMServerRolesConfig_RoleDetailRow_RendersSizeSelector()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var selects = cut.FindAll(".role-detail-row select");
        selects.Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_RoleDetailRow_RendersRemoveButton()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".role-remove").Should().NotBeEmpty();
    }

    [Fact]
    public async Task VMServerRolesConfig_RemoveRole_InvokesCallback()
    {
        // Arrange
        (EnvironmentType, VMRoleConfig)? receivedRemove = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var roleConfig = CreateRoleConfig(ServerRole.Web);
        configs[EnvironmentType.Dev].Roles.Add(roleConfig);

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.OnRemoveRoleRequested,
                EventCallback.Factory.Create<(EnvironmentType, VMRoleConfig)>(
                    this, r => receivedRemove = r)));

        // Act
        var removeBtn = cut.Find(".role-remove");
        await cut.InvokeAsync(() => removeBtn.Click());

        // Assert
        receivedRemove.Should().NotBeNull();
        receivedRemove!.Value.Item1.Should().Be(EnvironmentType.Dev);
    }

    #endregion

    #region Max Instances Tests

    [Fact]
    public void VMServerRolesConfig_SingleMaxInstance_ShowsFixedCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var roleConfig = CreateRoleConfig(ServerRole.Web);
        configs[EnvironmentType.Dev].Roles.Add(roleConfig);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.GetMaxInstances, (VMRoleConfig _) => 1)); // Max 1 instance

        // Assert
        cut.FindAll(".fixed-count").Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_MultipleMaxInstances_ShowsNumberInput()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var roleConfig = CreateRoleConfig(ServerRole.Web);
        configs[EnvironmentType.Dev].Roles.Add(roleConfig);

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.GetMaxInstances, (VMRoleConfig _) => 10)); // Max 10 instances

        // Assert
        var numberInputs = cut.FindAll("input[type='number']");
        numberInputs.Should().NotBeEmpty();
    }

    [Fact]
    public void VMServerRolesConfig_NoGetMaxInstances_DefaultsTo10()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));
        // No GetMaxInstances provided, defaults to 10

        // Assert - Should show number input (not fixed count)
        var numberInputs = cut.FindAll("input[type='number']");
        numberInputs.Should().NotBeEmpty();
    }

    #endregion

    #region Config Changed Tests

    [Fact]
    public async Task VMServerRolesConfig_ChangingSize_InvokesConfigChangedCallback()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web, size: AppTier.Small));

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act
        var select = cut.Find(".role-detail-row select");
        await cut.InvokeAsync(() => select.Change(AppTier.Large.ToString()));

        // Assert
        updatedConfigs.Should().NotBeNull();
    }

    [Fact]
    public async Task VMServerRolesConfig_ChangingInstanceCount_InvokesConfigChangedCallback()
    {
        // Arrange
        Dictionary<EnvironmentType, VMEnvironmentConfig>? updatedConfigs = null;
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web, instanceCount: 1));

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.GetMaxInstances, (VMRoleConfig _) => 10)
            .Add(p => p.EnvironmentConfigsChanged,
                EventCallback.Factory.Create<Dictionary<EnvironmentType, VMEnvironmentConfig>>(
                    this, c => updatedConfigs = c)));

        // Act
        var numberInput = cut.Find("input[type='number']");
        await cut.InvokeAsync(() => numberInput.Change("3"));

        // Assert
        updatedConfigs.Should().NotBeNull();
    }

    #endregion

    #region Role Count Display Tests

    [Fact]
    public void VMServerRolesConfig_ActiveRole_DisplaysInstanceCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        var techRoles = new List<TechnologyServerRole>
        {
            new TechnologyServerRole { Id = "web-role", Name = "Web Server", Description = "HTTP" }
        };
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig
        {
            RoleId = "web-role",
            Role = ServerRole.Web,
            InstanceCount = 3
        });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs)
            .Add(p => p.AvailableTechnologyRoles, techRoles));

        // Assert
        cut.Markup.Should().Contain("x3");
    }

    [Fact]
    public void VMServerRolesConfig_EnvironmentBadge_DisplaysRoleCount()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.App));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".env-badge").Should().Contain(b => b.TextContent.Contains("2"));
    }

    #endregion

    #region Missing Config Handling Tests

    [Fact]
    public void VMServerRolesConfig_MissingEnvConfig_HandlesGracefully()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>(); // Empty configs

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert - Should render without errors and show 0 roles
        cut.Find(".vm-server-roles-config").Should().NotBeNull();
    }

    [Fact]
    public void VMServerRolesConfig_EmptyEnvironments_RendersEmptyAccordion()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType>();
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.FindAll(".h-accordion-panel").Should().BeEmpty();
    }

    #endregion

    #region EventCallback Safety Tests

    [Fact]
    public async Task VMServerRolesConfig_NoCallbacks_DoesNotThrow()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));

        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Act & Assert - No callbacks provided, should not throw
        var action = async () =>
        {
            var removeBtn = cut.Find(".role-remove");
            await cut.InvokeAsync(() => removeBtn.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Role Name Display Tests

    [Fact]
    public void VMServerRolesConfig_RoleWithCustomName_DisplaysCustomName()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(new VMRoleConfig
        {
            Role = ServerRole.Web,
            RoleName = "Custom Frontend Server",
            InstanceCount = 1
        });

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        cut.Markup.Should().Contain("Custom Frontend Server");
    }

    #endregion

    #region Size Selector Options Tests

    [Fact]
    public void VMServerRolesConfig_SizeSelector_HasAllAppTierOptions()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = CreateDefaultConfigs(enabledEnvs);
        configs[EnvironmentType.Dev].Roles.Add(CreateRoleConfig(ServerRole.Web));

        // Act
        var cut = RenderComponent<VMServerRolesConfig>(parameters => parameters
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.EnvironmentConfigs, configs));

        // Assert
        var select = cut.Find(".role-detail-row select");
        foreach (var tier in Enum.GetValues<AppTier>())
        {
            select.InnerHtml.Should().Contain(tier.ToString());
        }
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

    private static VMRoleConfig CreateRoleConfig(
        ServerRole role,
        int instanceCount = 1,
        AppTier size = AppTier.Medium,
        string? roleId = null,
        string? roleName = null)
    {
        return new VMRoleConfig
        {
            Role = role,
            RoleId = roleId,
            RoleName = roleName,
            InstanceCount = instanceCount,
            Size = size
        };
    }

    #endregion
}
