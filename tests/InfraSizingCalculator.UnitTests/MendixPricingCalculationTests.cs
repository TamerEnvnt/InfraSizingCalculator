using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for Mendix pricing calculations
/// </summary>
public class MendixPricingCalculationTests
{
    private readonly MendixPricingSettings _mendixPricing;

    public MendixPricingCalculationTests()
    {
        _mendixPricing = new MendixPricingSettings();
    }

    #region K8s Environment Tier Pricing

    /// <summary>
    /// K8s environment cost should be 0 for environments within base included count
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateK8sEnvironmentCost_WithinBaseIncluded_ReturnsZero(int environments)
    {
        var cost = _mendixPricing.CalculateK8sEnvironmentCost(environments);
        Assert.Equal(0, cost);
    }

    /// <summary>
    /// K8s environment cost for tier 1 (4-50 environments) at $552/env
    /// </summary>
    [Theory]
    [InlineData(4, 552)]       // 1 additional env
    [InlineData(5, 1104)]      // 2 additional envs
    [InlineData(10, 3864)]     // 7 additional envs
    [InlineData(53, 27600)]    // 50 additional envs
    public void CalculateK8sEnvironmentCost_Tier1_CorrectCost(int environments, decimal expectedCost)
    {
        var cost = _mendixPricing.CalculateK8sEnvironmentCost(environments);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// K8s environment cost for tier 2 (51-100 environments) at $408/env
    /// </summary>
    [Theory]
    [InlineData(54, 27600 + 408)]      // 50 tier1 + 1 tier2
    [InlineData(60, 27600 + 2856)]     // 50 tier1 + 7 tier2
    [InlineData(103, 27600 + 20400)]   // 50 tier1 + 50 tier2
    public void CalculateK8sEnvironmentCost_Tier2_CorrectCost(int environments, decimal expectedCost)
    {
        var cost = _mendixPricing.CalculateK8sEnvironmentCost(environments);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// K8s environment cost for tier 3 (101-150 environments) at $240/env
    /// </summary>
    [Theory]
    [InlineData(104, 27600 + 20400 + 240)]      // 50 tier1 + 50 tier2 + 1 tier3
    [InlineData(110, 27600 + 20400 + 1680)]     // 50 tier1 + 50 tier2 + 7 tier3
    [InlineData(153, 27600 + 20400 + 12000)]    // 50 tier1 + 50 tier2 + 50 tier3
    public void CalculateK8sEnvironmentCost_Tier3_CorrectCost(int environments, decimal expectedCost)
    {
        var cost = _mendixPricing.CalculateK8sEnvironmentCost(environments);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// K8s environment cost for tier 4 (151+) is FREE
    /// </summary>
    [Theory]
    [InlineData(154)]   // 50 tier1 + 50 tier2 + 50 tier3 + 1 free
    [InlineData(200)]
    [InlineData(500)]
    public void CalculateK8sEnvironmentCost_Tier4_NoAdditionalCost(int environments)
    {
        // Total from tiers 1-3: 27600 + 20400 + 12000 = 60000
        var expectedCost = 27600m + 20400m + 12000m;
        var cost = _mendixPricing.CalculateK8sEnvironmentCost(environments);
        Assert.Equal(expectedCost, cost);
    }

    #endregion

    #region User Licensing Cost

    /// <summary>
    /// Internal user licensing should charge per 100 users (rounded up)
    /// </summary>
    [Theory]
    [InlineData(100, 40800)]     // 1 block
    [InlineData(101, 81600)]     // 2 blocks
    [InlineData(200, 81600)]     // 2 blocks
    [InlineData(250, 122400)]    // 3 blocks
    public void CalculateInternalUserCost_PerHundredUsers(int users, decimal expectedCost)
    {
        int userBlocks = (int)Math.Ceiling(users / 100.0);
        decimal cost = userBlocks * _mendixPricing.InternalUsersPer100PerYear;
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// External user licensing should charge per 250K users (rounded up)
    /// </summary>
    [Theory]
    [InlineData(250000, 60000)]     // 1 block
    [InlineData(250001, 120000)]    // 2 blocks
    [InlineData(500000, 120000)]    // 2 blocks
    [InlineData(750000, 180000)]    // 3 blocks
    public void CalculateExternalUserCost_Per250KUsers(int users, decimal expectedCost)
    {
        int userBlocks = (int)Math.Ceiling(users / 250000.0);
        decimal cost = userBlocks * _mendixPricing.ExternalUsersPer250KPerYear;
        Assert.Equal(expectedCost, cost);
    }

    #endregion

    #region Resource Pack Pricing

    /// <summary>
    /// Standard resource packs should have correct prices
    /// </summary>
    [Theory]
    [InlineData(MendixResourcePackSize.XS, 516)]
    [InlineData(MendixResourcePackSize.S, 1032)]
    [InlineData(MendixResourcePackSize.M, 2064)]
    [InlineData(MendixResourcePackSize.L, 4128)]
    [InlineData(MendixResourcePackSize.XL, 8256)]
    public void StandardResourcePacks_CorrectPricing(MendixResourcePackSize size, decimal expectedPrice)
    {
        var pack = _mendixPricing.GetResourcePack(MendixResourcePackTier.Standard, size);
        Assert.NotNull(pack);
        Assert.Equal(expectedPrice, pack.PricePerYear);
    }

    /// <summary>
    /// Premium resource packs (99.95% SLA) should be 1.5x standard price
    /// </summary>
    [Theory]
    [InlineData(MendixResourcePackSize.S, 1548)]
    [InlineData(MendixResourcePackSize.M, 3096)]
    [InlineData(MendixResourcePackSize.L, 6192)]
    public void PremiumResourcePacks_CorrectPricing(MendixResourcePackSize size, decimal expectedPrice)
    {
        var pack = _mendixPricing.GetResourcePack(MendixResourcePackTier.Premium, size);
        Assert.NotNull(pack);
        Assert.Equal(expectedPrice, pack.PricePerYear);
    }

    #endregion

    #region Azure Pricing

    /// <summary>
    /// Azure base package should include 3 environments
    /// </summary>
    [Fact]
    public void AzurePricing_BaseIncludesThreeEnvironments()
    {
        Assert.Equal(3, _mendixPricing.AzureBaseEnvironmentsIncluded);
        Assert.Equal(6612m, _mendixPricing.AzureBasePricePerYear);
    }

    /// <summary>
    /// Azure additional environments should cost $722.40/year each
    /// </summary>
    [Fact]
    public void AzurePricing_AdditionalEnvironmentCost()
    {
        Assert.Equal(722.40m, _mendixPricing.AzureAdditionalEnvironmentPrice);
    }

    /// <summary>
    /// Calculate Azure total cost with environments
    /// </summary>
    [Theory]
    [InlineData(3, 6612)]              // Base only
    [InlineData(4, 6612 + 722.40)]     // Base + 1 additional
    [InlineData(10, 6612 + 5056.80)]   // Base + 7 additional
    public void AzureTotalCost_WithEnvironments(int environments, decimal expectedCost)
    {
        var envCost = environments > _mendixPricing.AzureBaseEnvironmentsIncluded
            ? (environments - _mendixPricing.AzureBaseEnvironmentsIncluded) * _mendixPricing.AzureAdditionalEnvironmentPrice
            : 0;
        var totalCost = _mendixPricing.AzureBasePricePerYear + envCost;
        Assert.Equal(expectedCost, totalCost);
    }

    #endregion

    #region Mendix Pricing Result Totals

    /// <summary>
    /// MendixPricingResult should calculate totals correctly
    /// </summary>
    [Fact]
    public void MendixPricingResult_CalculatesTotals()
    {
        var result = new MendixPricingResult
        {
            DeploymentFeeCost = 6612m,
            EnvironmentCost = 3864m,
            UserLicenseCost = 40800m
        };

        Assert.Equal(51276m, result.TotalPerYear);
        Assert.Equal(51276m / 12, result.TotalPerMonth);
        Assert.Equal(51276m * 3, result.TotalThreeYear);
    }

    #endregion
}
