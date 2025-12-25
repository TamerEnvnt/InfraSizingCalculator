using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for TechnologyService
/// </summary>
public class TechnologyServiceTests
{
    private readonly TechnologyService _service;

    public TechnologyServiceTests()
    {
        _service = new TechnologyService();
    }

    /// <summary>
    /// Verify all technologies are available
    /// </summary>
    [Fact]
    public void GetAll_Returns7Technologies()
    {
        var technologies = _service.GetAll();
        Assert.Equal(7, technologies.Count());
    }

    /// <summary>
    /// Verify each technology can be retrieved
    /// </summary>
    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void GetConfig_ReturnsTechnologyConfig(Technology tech)
    {
        var config = _service.GetConfig(tech);

        Assert.NotNull(config);
        Assert.Equal(tech, config.Technology);
        Assert.NotNull(config.Name);
        Assert.NotEmpty(config.Tiers);
    }

    /// <summary>
    /// Verify .NET tier specs
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 0.25, 0.5)]
    [InlineData(AppTier.Medium, 0.5, 1)]
    [InlineData(AppTier.Large, 1, 2)]
    [InlineData(AppTier.XLarge, 2, 4)]
    public void DotNet_HasCorrectTierSpecs(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.DotNet);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify Java tier specs (higher memory than .NET)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 0.5, 1)]
    [InlineData(AppTier.Medium, 1, 2)]
    [InlineData(AppTier.Large, 2, 4)]
    [InlineData(AppTier.XLarge, 4, 8)]
    public void Java_HasCorrectTierSpecs(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.Java);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify Go tier specs (lowest footprint)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 0.125, 0.25)]
    [InlineData(AppTier.Medium, 0.25, 0.5)]
    [InlineData(AppTier.Large, 0.5, 1)]
    [InlineData(AppTier.XLarge, 1, 2)]
    public void Go_HasLowestFootprint(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.Go);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify Python tier specs
    /// Note: Small tier RAM increased to 1 GB per official docs (WSGI/Django overhead)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 0.25, 1)]
    [InlineData(AppTier.Medium, 0.5, 1)]
    [InlineData(AppTier.Large, 1, 2)]
    [InlineData(AppTier.XLarge, 2, 4)]
    public void Python_HasCorrectTierSpecs(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.Python);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify Node.js tier specs
    /// Note: Small tier RAM increased to 1 GB per official docs (V8 heap management)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 0.25, 1)]
    [InlineData(AppTier.Medium, 0.5, 1)]
    [InlineData(AppTier.Large, 1, 2)]
    [InlineData(AppTier.XLarge, 2, 4)]
    public void NodeJs_HasCorrectTierSpecs(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.NodeJs);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify Mendix tier specs (low-code platform with higher requirements)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 1, 2)]
    [InlineData(AppTier.Medium, 2, 4)]
    [InlineData(AppTier.Large, 4, 8)]
    [InlineData(AppTier.XLarge, 8, 16)]
    public void Mendix_HasCorrectTierSpecs(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.Mendix);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify OutSystems tier specs (low-code platform with higher requirements)
    /// </summary>
    [Theory]
    [InlineData(AppTier.Small, 1, 2)]
    [InlineData(AppTier.Medium, 2, 4)]
    [InlineData(AppTier.Large, 4, 8)]
    [InlineData(AppTier.XLarge, 8, 16)]
    public void OutSystems_HasCorrectTierSpecs(AppTier tier, double expectedCpu, double expectedRam)
    {
        var config = _service.GetConfig(Technology.OutSystems);
        var tierSpec = config.Tiers[tier];

        Assert.Equal(expectedCpu, tierSpec.Cpu);
        Assert.Equal(expectedRam, tierSpec.Ram);
    }

    /// <summary>
    /// Verify Mendix is marked as low-code
    /// </summary>
    [Fact]
    public void Mendix_IsLowCode()
    {
        var config = _service.GetConfig(Technology.Mendix);
        Assert.True(config.IsLowCode);
    }

    /// <summary>
    /// Verify OutSystems is marked as low-code
    /// </summary>
    [Fact]
    public void OutSystems_IsLowCode()
    {
        var config = _service.GetConfig(Technology.OutSystems);
        Assert.True(config.IsLowCode);
    }

    /// <summary>
    /// Verify native technologies are not low-code
    /// </summary>
    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    public void NativeTechnologies_AreNotLowCode(Technology tech)
    {
        var config = _service.GetConfig(tech);
        Assert.False(config.IsLowCode);
    }

    /// <summary>
    /// Verify all technologies can be filtered by IsLowCode
    /// </summary>
    [Fact]
    public void GetAll_FilterNative_Returns5()
    {
        var native = _service.GetAll().Where(t => !t.IsLowCode);
        Assert.Equal(5, native.Count());
    }

    [Fact]
    public void GetAll_FilterLowCode_Returns2()
    {
        var lowCode = _service.GetAll().Where(t => t.IsLowCode);
        Assert.Equal(2, lowCode.Count());
        Assert.Contains(lowCode, t => t.Technology == Technology.Mendix);
        Assert.Contains(lowCode, t => t.Technology == Technology.OutSystems);
    }
}
