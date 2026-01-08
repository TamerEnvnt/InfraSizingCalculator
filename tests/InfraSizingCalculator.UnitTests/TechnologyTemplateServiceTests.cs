using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for TechnologyTemplateService - provides technology-specific VM role templates
/// </summary>
public class TechnologyTemplateServiceTests
{
    private readonly TechnologyTemplateService _service;

    public TechnologyTemplateServiceTests()
    {
        _service = new TechnologyTemplateService();
    }

    #region GetAllTemplates Tests

    [Fact]
    public void GetAllTemplates_Returns7Templates()
    {
        // Act
        var templates = _service.GetAllTemplates();

        // Assert
        Assert.Equal(7, templates.Count());
    }

    [Fact]
    public void GetAllTemplates_AllHaveValidNames()
    {
        // Act
        var templates = _service.GetAllTemplates();

        // Assert
        Assert.All(templates, t =>
        {
            Assert.False(string.IsNullOrWhiteSpace(t.TemplateName));
            Assert.False(string.IsNullOrWhiteSpace(t.Description));
        });
    }

    [Fact]
    public void GetAllTemplates_AllHaveAtLeastOneRole()
    {
        // Act
        var templates = _service.GetAllTemplates();

        // Assert
        Assert.All(templates, t => Assert.NotEmpty(t.Roles));
    }

    #endregion

    #region GetTemplate Tests

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void GetTemplate_ReturnsTemplateForEachTechnology(Technology technology)
    {
        // Act
        var template = _service.GetTemplate(technology);

        // Assert
        Assert.NotNull(template);
        Assert.Equal(technology, template.Technology);
        Assert.NotEmpty(template.Roles);
    }

    [Theory]
    [InlineData(Technology.DotNet, ".NET Web Application Stack")]
    [InlineData(Technology.Java, "Java Enterprise Application Stack")]
    [InlineData(Technology.NodeJs, "Node.js Application Stack")]
    [InlineData(Technology.Python, "Python Web Application Stack")]
    [InlineData(Technology.Go, "Go Application Stack")]
    [InlineData(Technology.Mendix, "Mendix Low-Code Platform")]
    [InlineData(Technology.OutSystems, "OutSystems Enterprise Platform")]
    public void GetTemplate_HasCorrectTemplateName(Technology technology, string expectedName)
    {
        // Act
        var template = _service.GetTemplate(technology);

        // Assert
        Assert.Equal(expectedName, template.TemplateName);
    }

    #endregion

    #region IsLowCodePlatform Tests

    [Theory]
    [InlineData(Technology.Mendix, true)]
    [InlineData(Technology.OutSystems, true)]
    [InlineData(Technology.DotNet, false)]
    [InlineData(Technology.Java, false)]
    [InlineData(Technology.NodeJs, false)]
    [InlineData(Technology.Python, false)]
    [InlineData(Technology.Go, false)]
    public void IsLowCodePlatform_ReturnsCorrectValue(Technology technology, bool expected)
    {
        // Act
        var result = _service.IsLowCodePlatform(technology);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetTemplate_LowCodePlatforms_HaveIsLowCodeFlag()
    {
        // Arrange & Act
        var mendix = _service.GetTemplate(Technology.Mendix);
        var outsystems = _service.GetTemplate(Technology.OutSystems);

        // Assert
        Assert.True(mendix.IsLowCode);
        Assert.True(outsystems.IsLowCode);
    }

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    public void GetTemplate_NativePlatforms_HaveIsLowCodeFalse(Technology technology)
    {
        // Act
        var template = _service.GetTemplate(technology);

        // Assert
        Assert.False(template.IsLowCode);
    }

    #endregion

    #region GetTemplatesByPlatformType Tests

    [Fact]
    public void GetTemplatesByPlatformType_LowCode_Returns2Templates()
    {
        // Act
        var templates = _service.GetTemplatesByPlatformType(PlatformType.LowCode);

        // Assert
        Assert.Equal(2, templates.Count());
        Assert.All(templates, t => Assert.True(t.IsLowCode));
    }

    [Fact]
    public void GetTemplatesByPlatformType_Native_Returns5Templates()
    {
        // Act
        var templates = _service.GetTemplatesByPlatformType(PlatformType.Native);

        // Assert
        Assert.Equal(5, templates.Count());
        Assert.All(templates, t => Assert.False(t.IsLowCode));
    }

    #endregion

    #region Required Roles Tests

    [Theory]
    [InlineData(Technology.DotNet, 2)]    // Web, Database
    [InlineData(Technology.Java, 3)]      // Web, App, Database
    [InlineData(Technology.NodeJs, 3)]    // API, Database, Cache
    [InlineData(Technology.Python, 3)]    // Web, App, Database
    [InlineData(Technology.Go, 2)]        // API, Database
    [InlineData(Technology.Mendix, 3)]    // Runtime, Database, Storage
    [InlineData(Technology.OutSystems, 5)] // Controller, Frontend, Database, Cache, Scheduler
    public void GetTemplate_HasExpectedRequiredRoleCount(Technology technology, int expectedCount)
    {
        // Act
        var template = _service.GetTemplate(technology);
        var requiredRoles = template.RequiredRoles.ToList();

        // Assert
        Assert.Equal(expectedCount, requiredRoles.Count);
    }

    [Fact]
    public void GetTemplate_AllRoles_HaveValidProperties()
    {
        // Act
        var templates = _service.GetAllTemplates();

        // Assert
        foreach (var template in templates)
        {
            Assert.All(template.Roles, role =>
            {
                Assert.False(string.IsNullOrWhiteSpace(role.RoleId));
                Assert.False(string.IsNullOrWhiteSpace(role.RoleName));
                Assert.True(role.DefaultDiskGB > 0);
                Assert.True(role.MemoryMultiplier > 0);
            });
        }
    }

    #endregion

    #region GetDefaultRoles Tests

    [Theory]
    [InlineData(Technology.DotNet, false, 2)]  // Non-prod, required only
    [InlineData(Technology.DotNet, true, 2)]   // Prod, required only
    [InlineData(Technology.Java, false, 3)]    // Non-prod, required only
    [InlineData(Technology.OutSystems, true, 5)] // Prod, all required for OutSystems
    public void GetDefaultRoles_WithoutOptional_ReturnsRequiredRolesOnly(
        Technology technology, bool isProd, int expectedCount)
    {
        // Act
        var roles = _service.GetDefaultRoles(technology, isProd, includeOptional: false);

        // Assert
        Assert.Equal(expectedCount, roles.Count);
    }

    [Fact]
    public void GetDefaultRoles_WithOptional_ReturnsAllRoles()
    {
        // Arrange
        var template = _service.GetTemplate(Technology.DotNet);
        var expectedCount = template.Roles.Count;

        // Act
        var roles = _service.GetDefaultRoles(Technology.DotNet, isProd: true, includeOptional: true);

        // Assert
        Assert.Equal(expectedCount, roles.Count);
    }

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    public void GetDefaultRoles_Prod_HasMoreInstances(Technology technology)
    {
        // Act
        var nonProdRoles = _service.GetDefaultRoles(technology, isProd: false);
        var prodRoles = _service.GetDefaultRoles(technology, isProd: true);

        // Assert - Prod should have more or equal instances for scalable roles
        var nonProdInstances = nonProdRoles.Where(r => r.Role == ServerRole.Web || r.Role == ServerRole.App)
            .Sum(r => r.InstanceCount);
        var prodInstances = prodRoles.Where(r => r.Role == ServerRole.Web || r.Role == ServerRole.App)
            .Sum(r => r.InstanceCount);

        Assert.True(prodInstances >= nonProdInstances);
    }

    #endregion

    #region ApplyTemplate Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, HAPattern.None)]
    [InlineData(EnvironmentType.Test, HAPattern.None)]
    [InlineData(EnvironmentType.Stage, HAPattern.None)]
    [InlineData(EnvironmentType.Prod, HAPattern.ActivePassive)]
    [InlineData(EnvironmentType.DR, HAPattern.ActivePassive)]
    public void ApplyTemplate_SetsCorrectHAPattern(EnvironmentType env, HAPattern expectedPattern)
    {
        // Act
        var config = _service.ApplyTemplate(Technology.DotNet, env);

        // Assert
        Assert.Equal(expectedPattern, config.HAPattern);
    }

    [Fact]
    public void ApplyTemplate_Prod_SetsDRPattern()
    {
        // Act
        var config = _service.ApplyTemplate(Technology.DotNet, EnvironmentType.Prod);

        // Assert
        Assert.Equal(DRPattern.WarmStandby, config.DRPattern);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.DR)]
    public void ApplyTemplate_NonProd_HasNoDRPattern(EnvironmentType env)
    {
        // Act
        var config = _service.ApplyTemplate(Technology.DotNet, env);

        // Assert
        Assert.Equal(DRPattern.None, config.DRPattern);
    }

    [Fact]
    public void ApplyTemplate_SetsEnvironmentType()
    {
        // Act
        var config = _service.ApplyTemplate(Technology.Java, EnvironmentType.Stage);

        // Assert
        Assert.Equal(EnvironmentType.Stage, config.Environment);
        Assert.True(config.Enabled);
    }

    [Fact]
    public void ApplyTemplate_CalculatesStorageGB()
    {
        // Act
        var config = _service.ApplyTemplate(Technology.DotNet, EnvironmentType.Prod);

        // Assert
        Assert.True(config.StorageGB > 0);
    }

    [Fact]
    public void ApplyTemplate_WithMultipleWebServers_SetsLoadBalancer()
    {
        // Act - OutSystems prod has 2 frontend servers
        var config = _service.ApplyTemplate(Technology.OutSystems, EnvironmentType.Prod);

        // Assert
        Assert.NotEqual(LoadBalancerOption.None, config.LoadBalancer);
    }

    #endregion

    #region Technology-Specific Template Tests

    [Fact]
    public void DotNetTemplate_HasDatabaseRole()
    {
        // Act
        var template = _service.GetTemplate(Technology.DotNet);

        // Assert
        Assert.Contains(template.Roles, r => r.Role == ServerRole.Database);
    }

    [Fact]
    public void JavaTemplate_AppServer_HasHighMemoryMultiplier()
    {
        // Act
        var template = _service.GetTemplate(Technology.Java);
        var appServer = template.Roles.FirstOrDefault(r => r.RoleId == "java-app");

        // Assert
        Assert.NotNull(appServer);
        Assert.Equal(1.5, appServer.MemoryMultiplier);
    }

    [Fact]
    public void GoTemplate_ApiServer_HasLowMemoryMultiplier()
    {
        // Act
        var template = _service.GetTemplate(Technology.Go);
        var apiServer = template.Roles.FirstOrDefault(r => r.RoleId == "go-api");

        // Assert
        Assert.NotNull(apiServer);
        Assert.Equal(0.8, apiServer.MemoryMultiplier);
    }

    [Fact]
    public void MendixTemplate_HasFileStorageRole()
    {
        // Act
        var template = _service.GetTemplate(Technology.Mendix);

        // Assert
        Assert.Contains(template.Roles, r => r.Role == ServerRole.Storage);
    }

    [Fact]
    public void OutSystemsTemplate_HasDeploymentController()
    {
        // Act
        var template = _service.GetTemplate(Technology.OutSystems);
        var controller = template.Roles.FirstOrDefault(r => r.RoleId == "os-controller");

        // Assert
        Assert.NotNull(controller);
        Assert.True(controller.IsRequired);
        Assert.False(controller.IsScalable);
        Assert.Equal(1, controller.MaxInstances);
    }

    [Fact]
    public void OutSystemsTemplate_DatabaseRole_IsXLarge()
    {
        // Act
        var template = _service.GetTemplate(Technology.OutSystems);
        var database = template.Roles.FirstOrDefault(r => r.Role == ServerRole.Database);

        // Assert
        Assert.NotNull(database);
        Assert.Equal(AppTier.XLarge, database.DefaultSize);
    }

    #endregion

    #region VMRoleTemplateItem ToRoleConfig Tests

    [Fact]
    public void ToRoleConfig_Prod_UsesProductionInstanceCount()
    {
        // Arrange
        var template = _service.GetTemplate(Technology.DotNet);
        var webRole = template.Roles.First(r => r.Role == ServerRole.Web);

        // Act
        var config = webRole.ToRoleConfig(isProd: true);

        // Assert
        Assert.Equal(webRole.DefaultInstancesProd, config.InstanceCount);
    }

    [Fact]
    public void ToRoleConfig_NonProd_UsesNonProdInstanceCount()
    {
        // Arrange
        var template = _service.GetTemplate(Technology.DotNet);
        var webRole = template.Roles.First(r => r.Role == ServerRole.Web);

        // Act
        var config = webRole.ToRoleConfig(isProd: false);

        // Assert
        Assert.Equal(webRole.DefaultInstancesNonProd, config.InstanceCount);
    }

    [Fact]
    public void ToRoleConfig_CopiesAllProperties()
    {
        // Arrange
        var template = _service.GetTemplate(Technology.Java);
        var appRole = template.Roles.First(r => r.RoleId == "java-app");

        // Act
        var config = appRole.ToRoleConfig(isProd: true);

        // Assert
        Assert.Equal(appRole.Role, config.Role);
        Assert.Equal(appRole.RoleId, config.RoleId);
        Assert.Equal(appRole.RoleName, config.RoleName);
        Assert.Equal(appRole.Icon, config.RoleIcon);
        Assert.Equal(appRole.Description, config.RoleDescription);
        Assert.Equal(appRole.DefaultSize, config.Size);
        Assert.Equal(appRole.DefaultDiskGB, config.DiskGB);
        Assert.Equal(appRole.MemoryMultiplier, config.MemoryMultiplier);
    }

    #endregion
}
