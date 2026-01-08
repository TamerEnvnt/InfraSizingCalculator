using FluentAssertions;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

/// <summary>
/// Tests for PricingStepResult model covering all computed properties and FormatCost method.
/// </summary>
public class PricingStepResultTests
{
    #region HasCosts Tests

    [Fact]
    public void HasCosts_WhenCloudDeployment_ReturnsTrue()
    {
        var result = new PricingStepResult { IsOnPrem = false };
        result.HasCosts.Should().BeTrue();
    }

    [Fact]
    public void HasCosts_WhenOnPremWithPricingEnabled_ReturnsTrue()
    {
        var result = new PricingStepResult { IsOnPrem = true, IncludePricing = true };
        result.HasCosts.Should().BeTrue();
    }

    [Fact]
    public void HasCosts_WhenOnPremWithPricingDisabled_ReturnsFalse()
    {
        var result = new PricingStepResult { IsOnPrem = true, IncludePricing = false };
        result.HasCosts.Should().BeFalse();
    }

    [Fact]
    public void HasCosts_WhenOnPremWithMendixCost_ReturnsTrue()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = false,
            MendixCost = new MendixPricingResult()
        };
        result.HasCosts.Should().BeTrue();
    }

    #endregion

    #region MonthlyCost Tests

    [Fact]
    public void MonthlyCost_WhenNoCosts_ReturnsNull()
    {
        var result = new PricingStepResult { IsOnPrem = true, IncludePricing = false };
        result.MonthlyCost.Should().BeNull();
    }

    [Fact]
    public void MonthlyCost_WhenOnPremWithCost_ReturnsOnPremMonthlyTotal()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = new OnPremCostBreakdown { MonthlyHardware = 5000m }
        };
        result.MonthlyCost.Should().Be(5000m);
    }

    [Fact]
    public void MonthlyCost_WhenCloudWithCost_ReturnsCloudMonthlyTotal()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = new CostEstimate { MonthlyTotal = 7500m }
        };
        result.MonthlyCost.Should().Be(7500m);
    }

    [Fact]
    public void MonthlyCost_WhenOnPremWithoutOnPremCost_ReturnsNull()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = null
        };
        result.MonthlyCost.Should().BeNull();
    }

    [Fact]
    public void MonthlyCost_WhenCloudWithoutCloudCost_ReturnsNull()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = null
        };
        result.MonthlyCost.Should().BeNull();
    }

    #endregion

    #region YearlyCost Tests

    [Fact]
    public void YearlyCost_WhenNoCosts_ReturnsNull()
    {
        var result = new PricingStepResult { IsOnPrem = true, IncludePricing = false };
        result.YearlyCost.Should().BeNull();
    }

    [Fact]
    public void YearlyCost_WhenOnPremWithCost_ReturnsOnPremYearlyTotal()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = new OnPremCostBreakdown { MonthlyHardware = 5000m } // YearlyTotal = 5000 * 12 = 60000
        };
        result.YearlyCost.Should().Be(60000m);
    }

    [Fact]
    public void YearlyCost_WhenCloudWithCost_ReturnsCloudYearlyTotal()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = new CostEstimate { MonthlyTotal = 5000m } // YearlyTotal = MonthlyTotal * 12
        };
        result.YearlyCost.Should().Be(60000m);
    }

    [Fact]
    public void YearlyCost_WhenOnPremWithoutOnPremCost_ReturnsNull()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = null
        };
        result.YearlyCost.Should().BeNull();
    }

    #endregion

    #region ThreeYearTCO Tests

    [Fact]
    public void ThreeYearTCO_WhenNoCosts_ReturnsNull()
    {
        var result = new PricingStepResult { IsOnPrem = true, IncludePricing = false };
        result.ThreeYearTCO.Should().BeNull();
    }

    [Fact]
    public void ThreeYearTCO_WhenOnPremWithCost_ReturnsOnPremTCO()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = new OnPremCostBreakdown { MonthlyHardware = 5000m } // ThreeYearTCO = 5000 * 36 = 180000
        };
        result.ThreeYearTCO.Should().Be(180000m);
    }

    [Fact]
    public void ThreeYearTCO_WhenCloudWithCost_ReturnsCloudThreeYearTCO()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = new CostEstimate { MonthlyTotal = 5000m } // ThreeYearTCO = MonthlyTotal * 12 * 3
        };
        result.ThreeYearTCO.Should().Be(180000m);
    }

    [Fact]
    public void ThreeYearTCO_WhenCloudWithoutCloudCost_ReturnsNull()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = null
        };
        result.ThreeYearTCO.Should().BeNull();
    }

    #endregion

    #region FormatCost Tests

    [Fact]
    public void FormatCost_WithNullCost_ReturnsNA()
    {
        PricingStepResult.FormatCost(null).Should().Be("N/A");
    }

    [Theory]
    [InlineData(0, "$0")]
    [InlineData(100, "$100")]
    [InlineData(1000, "$1,000")]
    [InlineData(10000, "$10,000")]
    [InlineData(100000, "$100,000")]
    [InlineData(1000000, "$1,000,000")]
    public void FormatCost_WithValidCost_ReturnsFormattedCurrency(decimal cost, string expected)
    {
        PricingStepResult.FormatCost(cost).Should().Be(expected);
    }

    [Fact]
    public void FormatCost_WithNegativeCost_ReturnsFormattedNegative()
    {
        var result = PricingStepResult.FormatCost(-1000m);
        result.Should().Be("-$1,000");
    }

    [Fact]
    public void FormatCost_WithDecimalCost_RoundsToNearestDollar()
    {
        // "C0" format rounds to nearest whole number
        PricingStepResult.FormatCost(1234.56m).Should().Be("$1,235");
    }

    [Fact]
    public void FormatCost_WithDecimalCostRoundDown_RoundsCorrectly()
    {
        PricingStepResult.FormatCost(1234.49m).Should().Be("$1,234");
    }

    [Fact]
    public void FormatCost_UsesUSCultureFormatting()
    {
        // Verify US formatting (comma as thousands separator, dollar sign)
        var result = PricingStepResult.FormatCost(1234567m);
        result.Should().StartWith("$");
        result.Should().Contain(",");
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void IncludePricing_CanBeSetAndRetrieved()
    {
        var result = new PricingStepResult { IncludePricing = true };
        result.IncludePricing.Should().BeTrue();

        result.IncludePricing = false;
        result.IncludePricing.Should().BeFalse();
    }

    [Fact]
    public void IsOnPrem_CanBeSetAndRetrieved()
    {
        var result = new PricingStepResult { IsOnPrem = true };
        result.IsOnPrem.Should().BeTrue();

        result.IsOnPrem = false;
        result.IsOnPrem.Should().BeFalse();
    }

    [Fact]
    public void OnPremCost_CanBeSetAndRetrieved()
    {
        var breakdown = new OnPremCostBreakdown { MonthlyHardware = 1000m };
        var result = new PricingStepResult { OnPremCost = breakdown };
        result.OnPremCost.Should().BeSameAs(breakdown);
    }

    [Fact]
    public void CloudCost_CanBeSetAndRetrieved()
    {
        var estimate = new CostEstimate { MonthlyTotal = 2000m };
        var result = new PricingStepResult { CloudCost = estimate };
        result.CloudCost.Should().BeSameAs(estimate);
    }

    [Fact]
    public void SelectedAlternative_CanBeSetAndRetrieved()
    {
        var alternative = new CloudAlternative { Name = "AWS Alternative" };
        var result = new PricingStepResult { SelectedAlternative = alternative };
        result.SelectedAlternative.Should().BeSameAs(alternative);
    }

    [Fact]
    public void MendixCost_CanBeSetAndRetrieved()
    {
        var mendixCost = new MendixPricingResult();
        var result = new PricingStepResult { MendixCost = mendixCost };
        result.MendixCost.Should().BeSameAs(mendixCost);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var result = new PricingStepResult();

        result.IncludePricing.Should().BeFalse();
        result.IsOnPrem.Should().BeFalse();
        result.OnPremCost.Should().BeNull();
        result.CloudCost.Should().BeNull();
        result.SelectedAlternative.Should().BeNull();
        result.MendixCost.Should().BeNull();
    }

    [Fact]
    public void HasCosts_IsOnPremFalse_WithNullCosts_StillReturnsTrue()
    {
        // Cloud deployment always has costs (conceptually), even if cost objects are null
        var result = new PricingStepResult { IsOnPrem = false };
        result.HasCosts.Should().BeTrue();
    }

    [Fact]
    public void ComplexScenario_OnPremWithAllCosts()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = new OnPremCostBreakdown { MonthlyHardware = 10000m }, // Yearly=120000, TCO=360000
            CloudCost = new CostEstimate { MonthlyTotal = 8000m }, // Should be ignored for on-prem
            MendixCost = new MendixPricingResult()
        };

        result.HasCosts.Should().BeTrue();
        result.MonthlyCost.Should().Be(10000m);
        result.YearlyCost.Should().Be(120000m);
        result.ThreeYearTCO.Should().Be(360000m);
    }

    [Fact]
    public void ComplexScenario_CloudWithAllCosts()
    {
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            IncludePricing = true, // Should be ignored for cloud
            OnPremCost = new OnPremCostBreakdown { MonthlyHardware = 10000m }, // Should be ignored for cloud
            CloudCost = new CostEstimate { MonthlyTotal = 8000m }
        };

        result.HasCosts.Should().BeTrue();
        result.MonthlyCost.Should().Be(8000m);
        result.YearlyCost.Should().Be(96000m); // 8000 * 12
        result.ThreeYearTCO.Should().Be(288000m); // 8000 * 12 * 3
    }

    #endregion
}
