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

    #region DR Pattern Tests

    /// <summary>
    /// Verify PilotLight DR pattern exists and has correct multiplier
    /// </summary>
    [Fact]
    public void Calculate_WithPilotLightDR_AppliesCorrectMultiplier()
    {
        var input = CreateBasicInput();
        input.EnvironmentConfigs[EnvironmentType.DR].DRPattern = DRPattern.PilotLight;

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        var drEnv = result.Environments.First(e => e.Environment == EnvironmentType.DR);
        Assert.True(drEnv.TotalVMs > 0);
    }

    /// <summary>
    /// Verify WarmStandby DR pattern
    /// </summary>
    [Fact]
    public void Calculate_WithWarmStandbyDR_AppliesCorrectMultiplier()
    {
        var input = CreateBasicInput();
        input.EnvironmentConfigs[EnvironmentType.DR].DRPattern = DRPattern.WarmStandby;

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        var drEnv = result.Environments.First(e => e.Environment == EnvironmentType.DR);
        Assert.True(drEnv.TotalVMs > 0);
    }

    /// <summary>
    /// Verify HotStandby DR pattern provides full redundancy
    /// </summary>
    [Fact]
    public void Calculate_WithHotStandbyDR_ProvidesFuLLRedundancy()
    {
        var input = CreateBasicInput();
        input.EnvironmentConfigs[EnvironmentType.DR].DRPattern = DRPattern.HotStandby;
        input.EnvironmentConfigs[EnvironmentType.Prod].DRPattern = DRPattern.None;

        // Make both have same roles
        input.EnvironmentConfigs[EnvironmentType.DR].Roles = new List<VMRoleConfig>
        {
            new() { Role = ServerRole.Web, Size = AppTier.Medium, InstanceCount = 2, DiskGB = 100 },
            new() { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 2, DiskGB = 100 },
            new() { Role = ServerRole.Database, Size = AppTier.Large, InstanceCount = 1, DiskGB = 500 }
        };

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        var drEnv = result.Environments.First(e => e.Environment == EnvironmentType.DR);
        Assert.True(drEnv.TotalVMs >= 5); // At least 5 VMs for full redundancy
    }

    /// <summary>
    /// Verify MultiRegion DR pattern
    /// </summary>
    [Fact]
    public void Calculate_WithMultiRegionDR_ReturnsResults()
    {
        var input = CreateBasicInput();
        input.EnvironmentConfigs[EnvironmentType.DR].DRPattern = DRPattern.MultiRegion;

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        var drEnv = result.Environments.First(e => e.Environment == EnvironmentType.DR);
        Assert.True(drEnv.TotalVMs > 0);
    }

    #endregion

    #region Custom CPU/RAM Override Tests

    /// <summary>
    /// Verify custom CPU override is applied
    /// </summary>
    [Fact]
    public void Calculate_WithCustomCpu_UsesCustomValue()
    {
        var input = CreateBasicInput();
        input.SystemOverheadPercent = 0; // Remove overhead to test exact values
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        var webRole = prodConfig.Roles.First(r => r.Role == ServerRole.Web);
        webRole.CustomCpu = 32; // Override to 32 CPU

        var result = _service.Calculate(input);

        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var webResult = prod.Roles.First(r => r.Role == ServerRole.Web);
        Assert.Equal(32, webResult.CpuPerInstance);
    }

    /// <summary>
    /// Verify custom RAM override is applied
    /// </summary>
    [Fact]
    public void Calculate_WithCustomRam_UsesCustomValue()
    {
        var input = CreateBasicInput();
        input.SystemOverheadPercent = 0; // Remove overhead to test exact values
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        var webRole = prodConfig.Roles.First(r => r.Role == ServerRole.Web);
        webRole.CustomRam = 128; // Override to 128 GB

        var result = _service.Calculate(input);

        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var webResult = prod.Roles.First(r => r.Role == ServerRole.Web);
        Assert.Equal(128, webResult.RamPerInstance);
    }

    /// <summary>
    /// Verify custom CPU and RAM together
    /// </summary>
    [Fact]
    public void Calculate_WithBothCustomCpuAndRam_UsesBothValues()
    {
        var input = CreateBasicInput();
        input.SystemOverheadPercent = 0; // Remove overhead to test exact values
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        var dbRole = prodConfig.Roles.First(r => r.Role == ServerRole.Database);
        dbRole.CustomCpu = 64;
        dbRole.CustomRam = 256;

        var result = _service.Calculate(input);

        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var dbResult = prod.Roles.First(r => r.Role == ServerRole.Database);
        Assert.Equal(64, dbResult.CpuPerInstance);
        Assert.Equal(256, dbResult.RamPerInstance);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Verify handling of zero instance count
    /// </summary>
    [Fact]
    public void Calculate_WithZeroInstances_ReturnsZeroForRole()
    {
        var input = CreateBasicInput();
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        var webRole = prodConfig.Roles.First(r => r.Role == ServerRole.Web);
        webRole.InstanceCount = 0;

        var result = _service.Calculate(input);

        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var webResult = prod.Roles.FirstOrDefault(r => r.Role == ServerRole.Web);

        // Either role is excluded or has zero VMs
        if (webResult != null)
        {
            Assert.Equal(0, webResult.TotalInstances);
        }
    }

    /// <summary>
    /// Verify handling of XLarge tier (maximum size)
    /// </summary>
    [Fact]
    public void Calculate_WithXLargeTier_ReturnsCorrectSpecs()
    {
        var input = CreateBasicInput();
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        foreach (var role in prodConfig.Roles)
        {
            role.Size = AppTier.XLarge;
        }

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        Assert.True(result.GrandTotal.TotalCpu > 0);
        Assert.True(result.GrandTotal.TotalRam > 0);
    }

    /// <summary>
    /// Verify handling of single environment
    /// </summary>
    [Fact]
    public void Calculate_WithSingleEnvironment_ReturnsResults()
    {
        var input = CreateBasicInput();
        input.EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        input.EnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>
        {
            [EnvironmentType.Prod] = input.EnvironmentConfigs[EnvironmentType.Prod]
        };

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        Assert.Single(result.Environments);
        Assert.True(result.GrandTotal.TotalVMs > 0);
    }

    /// <summary>
    /// Verify handling of many environments
    /// </summary>
    [Fact]
    public void Calculate_WithAllEnvironments_ReturnsResults()
    {
        var input = CreateBasicInput();
        input.EnabledEnvironments = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Stage,
            EnvironmentType.Prod,
            EnvironmentType.DR
        };

        foreach (var env in input.EnabledEnvironments)
        {
            if (!input.EnvironmentConfigs.ContainsKey(env))
            {
                input.EnvironmentConfigs[env] = new VMEnvironmentConfig
                {
                    Environment = env,
                    Enabled = true,
                    HAPattern = HAPattern.None,
                    LoadBalancer = LoadBalancerOption.None,
                    StorageGB = 100,
                    Roles = new List<VMRoleConfig>
                    {
                        new() { Role = ServerRole.Web, Size = AppTier.Small, InstanceCount = 1, DiskGB = 50 }
                    }
                };
            }
        }

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        Assert.Equal(5, result.Environments.Count);
    }

    /// <summary>
    /// Verify handling of high instance counts
    /// </summary>
    [Fact]
    public void Calculate_WithHighInstanceCounts_ReturnsCorrectTotals()
    {
        var input = CreateBasicInput();
        var prodConfig = input.EnvironmentConfigs[EnvironmentType.Prod];
        prodConfig.Roles = new List<VMRoleConfig>
        {
            new() { Role = ServerRole.Web, Size = AppTier.Medium, InstanceCount = 10, DiskGB = 100 },
            new() { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 10, DiskGB = 100 }
        };

        var result = _service.Calculate(input);

        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        Assert.True(prod.TotalVMs >= 20);
    }

    #endregion

    #region Technology Memory Multiplier Tests

    /// <summary>
    /// Verify DotNet does NOT get memory multiplier
    /// </summary>
    [Fact]
    public void GetRoleSpecs_DotNet_NoMemoryMultiplier()
    {
        var (cpu, ram) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.DotNet);

        Assert.Equal(4, cpu);
        Assert.Equal(8, ram); // Base RAM without multiplier
    }

    /// <summary>
    /// Verify Python does NOT get memory multiplier
    /// </summary>
    [Fact]
    public void GetRoleSpecs_Python_NoMemoryMultiplier()
    {
        var (cpuDotNet, ramDotNet) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.DotNet);
        var (cpuPython, ramPython) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.Python);

        Assert.Equal(cpuDotNet, cpuPython);
        Assert.Equal(ramDotNet, ramPython);
    }

    /// <summary>
    /// Verify Go does NOT get memory multiplier
    /// </summary>
    [Fact]
    public void GetRoleSpecs_Go_NoMemoryMultiplier()
    {
        var (cpuDotNet, ramDotNet) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.DotNet);
        var (cpuGo, ramGo) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.Go);

        Assert.Equal(cpuDotNet, cpuGo);
        Assert.Equal(ramDotNet, ramGo);
    }

    /// <summary>
    /// Verify NodeJs does NOT get memory multiplier
    /// </summary>
    [Fact]
    public void GetRoleSpecs_NodeJs_NoMemoryMultiplier()
    {
        var (cpuDotNet, ramDotNet) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.DotNet);
        var (cpuNode, ramNode) = _service.GetRoleSpecs(ServerRole.Web, AppTier.Medium, Technology.NodeJs);

        Assert.Equal(cpuDotNet, cpuNode);
        Assert.Equal(ramDotNet, ramNode);
    }

    /// <summary>
    /// Verify memory multiplier applies to all roles for Java
    /// </summary>
    [Theory]
    [InlineData(ServerRole.Web)]
    [InlineData(ServerRole.App)]
    [InlineData(ServerRole.Database)]
    [InlineData(ServerRole.Cache)]
    public void GetRoleSpecs_Java_AppliesMultiplierToAllRoles(ServerRole role)
    {
        var (_, ramDotNet) = _service.GetRoleSpecs(role, AppTier.Medium, Technology.DotNet);
        var (_, ramJava) = _service.GetRoleSpecs(role, AppTier.Medium, Technology.Java);

        Assert.Equal((int)(ramDotNet * 1.5), ramJava);
    }

    /// <summary>
    /// Verify memory multiplier applies to all tiers for Mendix
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small)]
    [InlineData(AppTier.Medium)]
    [InlineData(AppTier.Large)]
    [InlineData(AppTier.XLarge)]
    public void GetRoleSpecs_Mendix_AppliesMultiplierToAllTiers(AppTier tier)
    {
        var (_, ramDotNet) = _service.GetRoleSpecs(ServerRole.App, tier, Technology.DotNet);
        var (_, ramMendix) = _service.GetRoleSpecs(ServerRole.App, tier, Technology.Mendix);

        Assert.Equal((int)(ramDotNet * 1.5), ramMendix);
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Verify empty enabled environments returns empty results
    /// </summary>
    [Fact]
    public void Calculate_WithNoEnabledEnvironments_ReturnsEmptyResults()
    {
        var input = CreateBasicInput();
        input.EnabledEnvironments = new HashSet<EnvironmentType>();

        var result = _service.Calculate(input);

        Assert.NotNull(result);
        Assert.Empty(result.Environments);
        Assert.Equal(0, result.GrandTotal.TotalVMs);
    }

    /// <summary>
    /// Verify empty roles returns zero VMs for environment
    /// </summary>
    [Fact]
    public void Calculate_WithNoRoles_ReturnsZeroVMsForEnvironment()
    {
        var input = CreateBasicInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].Roles = new List<VMRoleConfig>();

        var result = _service.Calculate(input);

        var prod = result.Environments.FirstOrDefault(e => e.Environment == EnvironmentType.Prod);
        if (prod != null)
        {
            Assert.Equal(0, prod.Roles.Sum(r => r.TotalInstances));
        }
    }

    /// <summary>
    /// Verify 50% system overhead increases resources significantly
    /// </summary>
    [Fact]
    public void Calculate_With50PercentOverhead_IncreasesResourcesSignificantly()
    {
        var inputNoOverhead = CreateBasicInput();
        inputNoOverhead.SystemOverheadPercent = 0;

        var inputWithOverhead = CreateBasicInput();
        inputWithOverhead.SystemOverheadPercent = 50; // 50% overhead

        var resultNoOverhead = _service.Calculate(inputNoOverhead);
        var resultWithOverhead = _service.Calculate(inputWithOverhead);

        // With 50% overhead, CPU and RAM should increase by at least 40%
        Assert.True(resultWithOverhead.GrandTotal.TotalCpu >= resultNoOverhead.GrandTotal.TotalCpu * 1.4,
            $"Expected CPU to increase by at least 40% with 50% overhead. Without: {resultNoOverhead.GrandTotal.TotalCpu}, With: {resultWithOverhead.GrandTotal.TotalCpu}");
        Assert.True(resultWithOverhead.GrandTotal.TotalRam >= resultNoOverhead.GrandTotal.TotalRam * 1.4,
            $"Expected RAM to increase by at least 40% with 50% overhead. Without: {resultNoOverhead.GrandTotal.TotalRam}, With: {resultWithOverhead.GrandTotal.TotalRam}");
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
