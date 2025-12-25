using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for VMSizingService
/// </summary>
public class VMSizingServiceTests
{
    private readonly VMSizingService _service;
    private readonly TechnologyService _technologyService;

    public VMSizingServiceTests()
    {
        _technologyService = new TechnologyService();
        _service = new VMSizingService(_technologyService);
    }

    #region GetRoleSpecs Tests

    /// <summary>
    /// Verify Web role specs for different sizes
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 2, 4)]
    [InlineData(AppTier.Medium, 4, 8)]
    [InlineData(AppTier.Large, 8, 16)]
    [InlineData(AppTier.XLarge, 16, 32)]
    public void GetRoleSpecs_WebRole_ReturnsCorrectSpecs(AppTier size, int expectedCpu, int expectedRam)
    {
        var (cpu, ram) = _service.GetRoleSpecs(ServerRole.Web, size, Technology.DotNet);

        Assert.Equal(expectedCpu, cpu);
        Assert.Equal(expectedRam, ram);
    }

    /// <summary>
    /// Verify Database role specs (higher memory)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 4, 16)]
    [InlineData(AppTier.Medium, 8, 32)]
    [InlineData(AppTier.Large, 16, 64)]
    [InlineData(AppTier.XLarge, 32, 128)]
    public void GetRoleSpecs_DatabaseRole_ReturnsHigherMemory(AppTier size, int expectedCpu, int expectedRam)
    {
        var (cpu, ram) = _service.GetRoleSpecs(ServerRole.Database, size, Technology.DotNet);

        Assert.Equal(expectedCpu, cpu);
        Assert.Equal(expectedRam, ram);
    }

    /// <summary>
    /// Verify Cache role specs
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 2, 8)]
    [InlineData(AppTier.Medium, 4, 16)]
    [InlineData(AppTier.Large, 8, 32)]
    [InlineData(AppTier.XLarge, 16, 64)]
    public void GetRoleSpecs_CacheRole_ReturnsCorrectSpecs(AppTier size, int expectedCpu, int expectedRam)
    {
        var (cpu, ram) = _service.GetRoleSpecs(ServerRole.Cache, size, Technology.DotNet);

        Assert.Equal(expectedCpu, cpu);
        Assert.Equal(expectedRam, ram);
    }

    /// <summary>
    /// Verify Bastion role is always small
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small)]
    [InlineData(AppTier.Medium)]
    [InlineData(AppTier.Large)]
    [InlineData(AppTier.XLarge)]
    public void GetRoleSpecs_BastionRole_AlwaysSmall(AppTier size)
    {
        var (cpu, ram) = _service.GetRoleSpecs(ServerRole.Bastion, size, Technology.DotNet);

        Assert.Equal(2, cpu);
        Assert.Equal(4, ram);
    }

    /// <summary>
    /// Verify Java/Mendix/OutSystems get 1.5x memory
    /// </summary>
    [Theory]
    [InlineData(Technology.Java)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void GetRoleSpecs_JavaMendixOutSystems_Get50PercentMoreMemory(Technology tech)
    {
        var (cpuDotNet, ramDotNet) = _service.GetRoleSpecs(ServerRole.App, AppTier.Medium, Technology.DotNet);
        var (cpuJava, ramJava) = _service.GetRoleSpecs(ServerRole.App, AppTier.Medium, tech);

        Assert.Equal(cpuDotNet, cpuJava);
        Assert.Equal((int)(ramDotNet * 1.5), ramJava);
    }

    #endregion

    #region GetHAMultiplier Tests

    /// <summary>
    /// Verify HA pattern multipliers
    /// </summary>
    [Theory]
    [InlineData(HAPattern.None, 1.0)]
    [InlineData(HAPattern.ActiveActive, 2.0)]
    [InlineData(HAPattern.ActivePassive, 2.0)]
    [InlineData(HAPattern.NPlus1, 1.5)]
    [InlineData(HAPattern.NPlus2, 1.67)]
    public void GetHAMultiplier_ReturnsCorrectMultiplier(HAPattern pattern, double expectedMultiplier)
    {
        var result = _service.GetHAMultiplier(pattern);
        Assert.Equal(expectedMultiplier, result, precision: 2);
    }

    #endregion

    #region GetLoadBalancerSpecs Tests

    /// <summary>
    /// Verify load balancer specs
    /// </summary>
    [Theory]
    [InlineData(LoadBalancerOption.None, 0, 0, 0)]
    [InlineData(LoadBalancerOption.Single, 1, 2, 4)]
    [InlineData(LoadBalancerOption.HAPair, 2, 2, 4)]
    [InlineData(LoadBalancerOption.CloudLB, 0, 0, 0)]
    public void GetLoadBalancerSpecs_ReturnsCorrectSpecs(LoadBalancerOption option, int vms, int cpuPerVm, int ramPerVm)
    {
        var (resultVms, resultCpu, resultRam) = _service.GetLoadBalancerSpecs(option);

        Assert.Equal(vms, resultVms);
        Assert.Equal(cpuPerVm, resultCpu);
        Assert.Equal(ramPerVm, resultRam);
    }

    #endregion

    #region Calculate Tests

    /// <summary>
    /// Verify basic VM calculation
    /// </summary>
    [Fact]
    public void Calculate_SimpleConfig_ReturnsResults()
    {
        var input = CreateBasicInput();

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Environments);
        Assert.True(result.GrandTotal.TotalVMs > 0);
    }

    /// <summary>
    /// Verify all enabled environments are in results
    /// </summary>
    [Fact]
    public void Calculate_AllEnabledEnvironments_AreInResults()
    {
        var input = CreateBasicInput();

        var result = _service.Calculate(input);

        foreach (var env in input.EnabledEnvironments)
        {
            Assert.Contains(result.Environments, e => e.Environment == env);
        }
    }

    /// <summary>
    /// Verify HA multiplier is applied
    /// </summary>
    [Fact]
    public void Calculate_WithActiveActive_DoublesInstances()
    {
        var inputNoHA = CreateBasicInput();
        inputNoHA.EnvironmentConfigs[EnvironmentType.Prod].HAPattern = HAPattern.None;

        var inputHA = CreateBasicInput();
        inputHA.EnvironmentConfigs[EnvironmentType.Prod].HAPattern = HAPattern.ActiveActive;

        var resultNoHA = _service.Calculate(inputNoHA);
        var resultHA = _service.Calculate(inputHA);

        var prodNoHA = resultNoHA.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var prodHA = resultHA.Environments.First(e => e.Environment == EnvironmentType.Prod);

        // HA should have more VMs
        Assert.True(prodHA.TotalVMs > prodNoHA.TotalVMs);
    }

    /// <summary>
    /// Verify load balancer VMs are added
    /// </summary>
    [Fact]
    public void Calculate_WithHAPairLB_Adds2VMs()
    {
        var input = CreateBasicInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].LoadBalancer = LoadBalancerOption.HAPair;

        var result = _service.Calculate(input);
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);

        Assert.Equal(2, prod.LoadBalancerVMs);
    }

    /// <summary>
    /// Verify system overhead is applied
    /// </summary>
    [Fact]
    public void Calculate_WithSystemOverhead_IncreasesResources()
    {
        var inputNoOverhead = CreateBasicInput();
        inputNoOverhead.SystemOverheadPercent = 0;

        var inputWithOverhead = CreateBasicInput();
        inputWithOverhead.SystemOverheadPercent = 20;

        var resultNoOverhead = _service.Calculate(inputNoOverhead);
        var resultWithOverhead = _service.Calculate(inputWithOverhead);

        Assert.True(resultWithOverhead.GrandTotal.TotalCpu >= resultNoOverhead.GrandTotal.TotalCpu);
        Assert.True(resultWithOverhead.GrandTotal.TotalRam >= resultNoOverhead.GrandTotal.TotalRam);
    }

    /// <summary>
    /// Verify production environments are marked correctly
    /// </summary>
    [Fact]
    public void Calculate_ProdAndDR_AreMarkedAsProd()
    {
        var input = CreateBasicInput();

        var result = _service.Calculate(input);

        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var dr = result.Environments.First(e => e.Environment == EnvironmentType.DR);
        var dev = result.Environments.First(e => e.Environment == EnvironmentType.Dev);

        Assert.True(prod.IsProd);
        Assert.True(dr.IsProd);
        Assert.False(dev.IsProd);
    }

    /// <summary>
    /// Verify grand total is sum of all environments
    /// </summary>
    [Fact]
    public void Calculate_GrandTotal_IsSumOfEnvironments()
    {
        var input = CreateBasicInput();

        var result = _service.Calculate(input);

        var totalVMs = result.Environments.Sum(e => e.TotalVMs);
        var totalCpu = result.Environments.Sum(e => e.TotalCpu);
        var totalRam = result.Environments.Sum(e => e.TotalRam);

        Assert.Equal(totalVMs, result.GrandTotal.TotalVMs);
        Assert.Equal(totalCpu, result.GrandTotal.TotalCpu);
        Assert.Equal(totalRam, result.GrandTotal.TotalRam);
    }

    /// <summary>
    /// Verify all roles are included in results
    /// </summary>
    [Fact]
    public void Calculate_AllRoles_AreInResults()
    {
        var input = CreateBasicInput();
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        prodConfig.Roles.Add(new VMRoleConfig { Role = ServerRole.Cache, Size = AppTier.Medium, InstanceCount = 1 });

        var result = _service.Calculate(input);
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);

        Assert.Contains(prod.Roles, r => r.Role == ServerRole.Web);
        Assert.Contains(prod.Roles, r => r.Role == ServerRole.App);
        Assert.Contains(prod.Roles, r => r.Role == ServerRole.Database);
        Assert.Contains(prod.Roles, r => r.Role == ServerRole.Cache);
    }

    #endregion

    #region Helper Methods

    private VMSizingInput CreateBasicInput()
    {
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Prod,
            EnvironmentType.DR
        };

        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>();
        foreach (var env in enabledEnvs)
        {
            configs[env] = new VMEnvironmentConfig
            {
                Environment = env,
                Enabled = true,
                HAPattern = env == EnvironmentType.Prod ? HAPattern.ActivePassive : HAPattern.None,
                DRPattern = env == EnvironmentType.DR ? DRPattern.WarmStandby : DRPattern.None,
                LoadBalancer = LoadBalancerOption.Single,
                StorageGB = 100,
                Roles = new List<VMRoleConfig>
                {
                    new() { Role = ServerRole.Web, Size = AppTier.Medium, InstanceCount = 1, DiskGB = 100 },
                    new() { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 1, DiskGB = 100 },
                    new() { Role = ServerRole.Database, Size = AppTier.Large, InstanceCount = 1, DiskGB = 500 }
                }
            };
        }

        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = enabledEnvs,
            EnvironmentConfigs = configs,
            SystemOverheadPercent = 15
        };
    }

    #endregion
}
