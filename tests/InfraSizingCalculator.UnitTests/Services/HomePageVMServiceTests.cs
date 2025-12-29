using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;

namespace InfraSizingCalculator.UnitTests.Services;

public class HomePageVMServiceTests
{
    private readonly IHomePageVMService _sut;
    private readonly ITechnologyService _mockTechService;

    public HomePageVMServiceTests()
    {
        _sut = new HomePageVMService();
        _mockTechService = Substitute.For<ITechnologyService>();
    }

    #region Test Helpers

    private static TechnologyConfig CreateTechnologyConfig(
        Technology technology = Technology.Mendix,
        bool hasSeparateManagementEnv = false,
        string managementEnvName = "",
        TechnologyVMRoles? managementRoles = null)
    {
        return new TechnologyConfig
        {
            Technology = technology,
            Name = technology.ToString(),
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new(1, 2),
                [AppTier.Medium] = new(2, 4),
                [AppTier.Large] = new(4, 8)
            },
            HasSeparateManagementEnvironment = hasSeparateManagementEnv,
            ManagementEnvironmentName = managementEnvName,
            ManagementEnvironmentRoles = managementRoles
        };
    }

    private static TechnologyVMRoles CreateVMRoles(
        Technology technology = Technology.Mendix,
        params TechnologyServerRole[] roles)
    {
        return new TechnologyVMRoles
        {
            Technology = technology,
            DeploymentName = $"{technology} VM Deployment",
            Description = $"VM deployment for {technology}",
            Roles = roles.ToList()
        };
    }

    private static TechnologyServerRole CreateServerRole(
        string id,
        string name,
        string description = "Test role",
        bool required = true,
        bool scaleHorizontally = false,
        int minInstances = 1,
        int? maxInstances = null,
        AppTier defaultSize = AppTier.Medium,
        int defaultDiskGB = 100)
    {
        return new TechnologyServerRole
        {
            Id = id,
            Name = name,
            Description = description,
            Required = required,
            ScaleHorizontally = scaleHorizontally,
            MinInstances = minInstances,
            MaxInstances = maxInstances,
            DefaultSize = defaultSize,
            DefaultDiskGB = defaultDiskGB
        };
    }

    #endregion

    #region IsProdEnvironment Tests

    [Theory]
    [InlineData(EnvironmentType.Prod, true)]
    [InlineData(EnvironmentType.DR, true)]
    [InlineData(EnvironmentType.Dev, false)]
    [InlineData(EnvironmentType.Test, false)]
    [InlineData(EnvironmentType.Stage, false)]
    public void IsProdEnvironment_ReturnsCorrectly(EnvironmentType env, bool expected)
    {
        _sut.IsProdEnvironment(env).Should().Be(expected);
    }

    #endregion

    #region GetShortEnvName Tests

    [Theory]
    [InlineData("Development", "DEV")]
    [InlineData("Test", "TEST")]
    [InlineData("Testing", "TEST")]
    [InlineData("Staging", "STG")]
    [InlineData("Production", "PROD")]
    [InlineData("Disaster Recovery", "DR")]
    [InlineData("QA", "QA")]
    [InlineData("Integration", "INTE")]
    public void GetShortEnvName_ReturnsCorrectAbbreviation(string input, string expected)
    {
        _sut.GetShortEnvName(input).Should().Be(expected);
    }

    #endregion

    #region GetEnvCssClass Tests

    [Theory]
    [InlineData("development", "env-dev")]
    [InlineData("Development", "env-dev")]
    [InlineData("test", "env-test")]
    [InlineData("testing", "env-test")]
    [InlineData("staging", "env-stage")]
    [InlineData("production", "env-prod")]
    [InlineData("disaster recovery", "env-dr")]
    [InlineData("unknown", "env-default")]
    public void GetEnvCssClass_ReturnsCorrectClass(string input, string expected)
    {
        _sut.GetEnvCssClass(input).Should().Be(expected);
    }

    #endregion

    #region GetEnvIcon Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "D")]
    [InlineData(EnvironmentType.Test, "T")]
    [InlineData(EnvironmentType.Stage, "S")]
    [InlineData(EnvironmentType.Prod, "P")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void GetEnvIcon_ReturnsCorrectIcon(EnvironmentType env, string expected)
    {
        _sut.GetEnvIcon(env).Should().Be(expected);
    }

    #endregion

    #region GetRoleIcon Tests

    [Theory]
    [InlineData(ServerRole.Web, "web")]
    [InlineData(ServerRole.App, "app")]
    [InlineData(ServerRole.Database, "db")]
    [InlineData(ServerRole.Cache, "cache")]
    [InlineData(ServerRole.MessageQueue, "mq")]
    [InlineData(ServerRole.Search, "search")]
    [InlineData(ServerRole.Storage, "storage")]
    [InlineData(ServerRole.Monitoring, "mon")]
    [InlineData(ServerRole.Bastion, "bastion")]
    public void GetRoleIcon_ReturnsCorrectIcon(ServerRole role, string expected)
    {
        _sut.GetRoleIcon(role).Should().Be(expected);
    }

    #endregion

    #region FormatHAPattern Tests

    [Theory]
    [InlineData(HAPattern.None, "None")]
    [InlineData(HAPattern.ActiveActive, "Active-Active")]
    [InlineData(HAPattern.ActivePassive, "Active-Passive")]
    [InlineData(HAPattern.NPlus1, "N+1 Redundancy")]
    [InlineData(HAPattern.NPlus2, "N+2 Redundancy")]
    public void FormatHAPattern_ReturnsCorrectDisplay(HAPattern pattern, string expected)
    {
        _sut.FormatHAPattern(pattern).Should().Be(expected);
    }

    #endregion

    #region GetHAPatternDisplay Tests

    [Theory]
    [InlineData(HAPattern.None, "None")]
    [InlineData(HAPattern.ActiveActive, "Active-Active")]
    [InlineData(HAPattern.ActivePassive, "Active-Passive")]
    [InlineData(HAPattern.NPlus1, "N+1")]
    [InlineData(HAPattern.NPlus2, "N+2")]
    public void GetHAPatternDisplay_ReturnsCorrectDisplay(HAPattern pattern, string expected)
    {
        _sut.GetHAPatternDisplay(pattern).Should().Be(expected);
    }

    #endregion

    #region GetDRPatternDisplay Tests

    [Theory]
    [InlineData(DRPattern.None, "None")]
    [InlineData(DRPattern.WarmStandby, "Warm Standby")]
    [InlineData(DRPattern.HotStandby, "Hot Standby")]
    [InlineData(DRPattern.MultiRegion, "Multi-Region")]
    public void GetDRPatternDisplay_ReturnsCorrectDisplay(DRPattern pattern, string expected)
    {
        _sut.GetDRPatternDisplay(pattern).Should().Be(expected);
    }

    #endregion

    #region GetLBOptionDisplay Tests

    [Theory]
    [InlineData(LoadBalancerOption.None, "None")]
    [InlineData(LoadBalancerOption.Single, "Single LB")]
    [InlineData(LoadBalancerOption.HAPair, "HA Pair")]
    [InlineData(LoadBalancerOption.CloudLB, "Cloud LB")]
    public void GetLBOptionDisplay_ReturnsCorrectDisplay(LoadBalancerOption option, string expected)
    {
        _sut.GetLBOptionDisplay(option).Should().Be(expected);
    }

    #endregion

    #region CreateFallbackRoles Tests

    [Fact]
    public void CreateFallbackRoles_ForNonProdEnv_ReturnsCorrectRoles()
    {
        var roles = _sut.CreateFallbackRoles(EnvironmentType.Dev);

        roles.Should().HaveCount(3);
        roles.Should().Contain(r => r.Role == ServerRole.Web && r.InstanceCount == 1);
        roles.Should().Contain(r => r.Role == ServerRole.App && r.InstanceCount == 1);
        roles.Should().Contain(r => r.Role == ServerRole.Database && r.InstanceCount == 1);
    }

    [Fact]
    public void CreateFallbackRoles_ForProdEnv_ReturnsCorrectRolesWithHA()
    {
        var roles = _sut.CreateFallbackRoles(EnvironmentType.Prod);

        roles.Should().HaveCount(3);
        roles.Should().Contain(r => r.Role == ServerRole.Web && r.InstanceCount == 2);
        roles.Should().Contain(r => r.Role == ServerRole.App && r.InstanceCount == 2);
        roles.Should().Contain(r => r.Role == ServerRole.Database && r.InstanceCount == 1);
    }

    #endregion

    #region CreateVMConfig Tests

    [Fact]
    public void CreateVMConfig_ForNonProdEnv_ReturnsCorrectConfig()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Test };

        var config = _sut.CreateVMConfig(EnvironmentType.Dev, enabledEnvs, null, _mockTechService);

        config.Environment.Should().Be(EnvironmentType.Dev);
        config.Enabled.Should().BeTrue();
        config.HAPattern.Should().Be(HAPattern.None);
        config.DRPattern.Should().Be(DRPattern.None);
        config.LoadBalancer.Should().Be(LoadBalancerOption.None);
        config.StorageGB.Should().Be(100);
    }

    [Fact]
    public void CreateVMConfig_ForProdEnv_ReturnsCorrectConfigWithHA()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var config = _sut.CreateVMConfig(EnvironmentType.Prod, enabledEnvs, null, _mockTechService);

        config.Environment.Should().Be(EnvironmentType.Prod);
        config.Enabled.Should().BeTrue();
        config.HAPattern.Should().Be(HAPattern.ActivePassive);
        config.DRPattern.Should().Be(DRPattern.None);
        config.LoadBalancer.Should().Be(LoadBalancerOption.HAPair);
        config.StorageGB.Should().Be(500);
    }

    [Fact]
    public void CreateVMConfig_ForDisabledEnv_HasEnabledFalse()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var config = _sut.CreateVMConfig(EnvironmentType.Dev, enabledEnvs, null, _mockTechService);

        config.Enabled.Should().BeFalse();
    }

    #endregion

    #region GetVMEnvSummary Tests

    [Fact]
    public void GetVMEnvSummary_WhenEnvDisabled_ReturnsDisabled()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();

        var result = _sut.GetVMEnvSummary(EnvironmentType.Dev, enabledEnvs, configs);

        result.Should().Be("Disabled");
    }

    [Fact]
    public void GetVMEnvSummary_WhenEnvNotConfigured_ReturnsNotConfigured()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();

        var result = _sut.GetVMEnvSummary(EnvironmentType.Dev, enabledEnvs, configs);

        result.Should().Be("Not configured");
    }

    [Fact]
    public void GetVMEnvSummary_WhenConfigured_ReturnsCorrectSummary()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev };
        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>
        {
            [EnvironmentType.Dev] = new VMEnvironmentConfig
            {
                Roles = new List<VMRoleConfig>
                {
                    new() { Role = ServerRole.Web, InstanceCount = 1 },
                    new() { Role = ServerRole.App, InstanceCount = 2 },
                    new() { Role = ServerRole.Database, InstanceCount = 1 }
                }
            }
        };

        var result = _sut.GetVMEnvSummary(EnvironmentType.Dev, enabledEnvs, configs);

        result.Should().Be("3 roles, 4 VMs");
    }

    #endregion

    #region GetRoleMaxInstances Tests

    [Fact]
    public void GetRoleMaxInstances_WhenNoRoleId_ReturnsDefault()
    {
        var roleConfig = new VMRoleConfig { Role = ServerRole.Web };

        var result = _sut.GetRoleMaxInstances(roleConfig, null, _mockTechService);

        result.Should().Be(100);
    }

    [Fact]
    public void GetRoleMaxInstances_WhenNoTechnology_ReturnsDefault()
    {
        var roleConfig = new VMRoleConfig { Role = ServerRole.Web, RoleId = "web-server" };

        var result = _sut.GetRoleMaxInstances(roleConfig, null, _mockTechService);

        result.Should().Be(100);
    }

    [Fact]
    public void GetRoleMaxInstances_WhenTechnologyRole_ReturnsConfiguredMax()
    {
        var roleConfig = new VMRoleConfig { Role = ServerRole.App, RoleId = "frontend-server" };
        var vmRoles = CreateVMRoles(Technology.Mendix,
            CreateServerRole("frontend-server", "Frontend Server", maxInstances: 10));
        _mockTechService.GetVMRoles(Technology.Mendix).Returns(vmRoles);

        var result = _sut.GetRoleMaxInstances(roleConfig, Technology.Mendix, _mockTechService);

        result.Should().Be(10);
    }

    #endregion

    #region IsRoleSingleInstance Tests

    [Fact]
    public void IsRoleSingleInstance_WhenMaxIsOne_ReturnsTrue()
    {
        var roleConfig = new VMRoleConfig { Role = ServerRole.Database, RoleId = "db-server" };
        var vmRoles = CreateVMRoles(Technology.Mendix,
            CreateServerRole("db-server", "Database Server", maxInstances: 1));
        _mockTechService.GetVMRoles(Technology.Mendix).Returns(vmRoles);

        var result = _sut.IsRoleSingleInstance(roleConfig, Technology.Mendix, _mockTechService);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoleSingleInstance_WhenMaxIsMoreThanOne_ReturnsFalse()
    {
        var roleConfig = new VMRoleConfig { Role = ServerRole.App, RoleId = "app-server" };
        var vmRoles = CreateVMRoles(Technology.Mendix,
            CreateServerRole("app-server", "App Server", maxInstances: 10));
        _mockTechService.GetVMRoles(Technology.Mendix).Returns(vmRoles);

        var result = _sut.IsRoleSingleInstance(roleConfig, Technology.Mendix, _mockTechService);

        result.Should().BeFalse();
    }

    #endregion

    #region GetAvailableTechnologyRoles Tests

    [Fact]
    public void GetAvailableTechnologyRoles_WhenNoTechnology_ReturnsEmpty()
    {
        var result = _sut.GetAvailableTechnologyRoles(null, _mockTechService);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableTechnologyRoles_WhenTechnologyHasRoles_ReturnsRoles()
    {
        var vmRoles = CreateVMRoles(Technology.OutSystems,
            CreateServerRole("web", "Web Server"),
            CreateServerRole("app", "App Server"));
        _mockTechService.GetVMRoles(Technology.OutSystems).Returns(vmRoles);

        var result = _sut.GetAvailableTechnologyRoles(Technology.OutSystems, _mockTechService);

        result.Should().HaveCount(2);
    }

    #endregion

    #region Management Environment Tests

    [Fact]
    public void HasManagementEnvironment_WhenNoTechnology_ReturnsFalse()
    {
        var result = _sut.HasManagementEnvironment(null, _mockTechService);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasManagementEnvironment_WhenTechnologyHasManagementEnv_ReturnsTrue()
    {
        var config = CreateTechnologyConfig(Technology.OutSystems, hasSeparateManagementEnv: true);
        _mockTechService.GetConfig(Technology.OutSystems).Returns(config);

        var result = _sut.HasManagementEnvironment(Technology.OutSystems, _mockTechService);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetManagementEnvironmentName_WhenNoTechnology_ReturnsDefault()
    {
        var result = _sut.GetManagementEnvironmentName(null, _mockTechService);

        result.Should().Be("Management");
    }

    [Fact]
    public void GetManagementEnvironmentName_WhenTechnologyHasName_ReturnsName()
    {
        var config = CreateTechnologyConfig(Technology.OutSystems, managementEnvName: "LifeTime");
        _mockTechService.GetConfig(Technology.OutSystems).Returns(config);

        var result = _sut.GetManagementEnvironmentName(Technology.OutSystems, _mockTechService);

        result.Should().Be("LifeTime");
    }

    [Fact]
    public void GetManagementEnvironmentRoles_WhenNoTechnology_ReturnsEmpty()
    {
        var result = _sut.GetManagementEnvironmentRoles(null, _mockTechService);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetManagementEnvironmentRoles_WhenTechnologyHasRoles_ReturnsRoles()
    {
        var managementRoles = CreateVMRoles(Technology.OutSystems,
            CreateServerRole("lifetime", "LifeTime Server"));
        var config = CreateTechnologyConfig(Technology.OutSystems, managementRoles: managementRoles);
        _mockTechService.GetConfig(Technology.OutSystems).Returns(config);

        var result = _sut.GetManagementEnvironmentRoles(Technology.OutSystems, _mockTechService);

        result.Should().HaveCount(1);
    }

    #endregion

    #region CreateTechnologySpecificRoles Tests

    [Fact]
    public void CreateTechnologySpecificRoles_WhenNoTechnology_ReturnsFallbackRoles()
    {
        var roles = _sut.CreateTechnologySpecificRoles(EnvironmentType.Dev, null, _mockTechService);

        roles.Should().HaveCount(3);
        roles.Should().Contain(r => r.Role == ServerRole.Web);
        roles.Should().Contain(r => r.Role == ServerRole.App);
        roles.Should().Contain(r => r.Role == ServerRole.Database);
    }

    [Fact]
    public void CreateTechnologySpecificRoles_ForLifeTimeEnv_UsesManagementRoles()
    {
        var managementRoles = CreateVMRoles(Technology.OutSystems,
            CreateServerRole("lifetime", "LifeTime", required: true, defaultSize: AppTier.Medium, defaultDiskGB: 200));
        var config = CreateTechnologyConfig(Technology.OutSystems, managementRoles: managementRoles);
        _mockTechService.GetConfig(Technology.OutSystems).Returns(config);

        var roles = _sut.CreateTechnologySpecificRoles(EnvironmentType.LifeTime, Technology.OutSystems, _mockTechService);

        roles.Should().HaveCount(1);
        roles.First().RoleName.Should().Be("LifeTime");
    }

    [Fact]
    public void CreateTechnologySpecificRoles_ForRegularEnv_UsesTechnologyVMRoles()
    {
        var vmRoles = CreateVMRoles(Technology.Mendix,
            CreateServerRole("frontend", "Frontend Server", required: true),
            CreateServerRole("db", "Database", required: true),
            CreateServerRole("optional", "Optional Service", required: false));
        _mockTechService.GetVMRoles(Technology.Mendix).Returns(vmRoles);
        _mockTechService.GetConfig(Technology.Mendix).Returns(CreateTechnologyConfig(Technology.Mendix));

        var roles = _sut.CreateTechnologySpecificRoles(EnvironmentType.Dev, Technology.Mendix, _mockTechService);

        roles.Should().HaveCount(2); // Only required roles
        roles.Should().Contain(r => r.RoleName == "Frontend Server");
        roles.Should().Contain(r => r.RoleName == "Database");
        roles.Should().NotContain(r => r.RoleName == "Optional Service");
    }

    [Fact]
    public void CreateTechnologySpecificRoles_ForProdEnv_HasHigherInstanceCounts()
    {
        var vmRoles = CreateVMRoles(Technology.Mendix,
            CreateServerRole("app", "App Server", required: true, scaleHorizontally: true, minInstances: 1));
        _mockTechService.GetVMRoles(Technology.Mendix).Returns(vmRoles);
        _mockTechService.GetConfig(Technology.Mendix).Returns(CreateTechnologyConfig(Technology.Mendix));

        var roles = _sut.CreateTechnologySpecificRoles(EnvironmentType.Prod, Technology.Mendix, _mockTechService);

        roles.First().InstanceCount.Should().BeGreaterOrEqualTo(2);
    }

    #endregion
}
