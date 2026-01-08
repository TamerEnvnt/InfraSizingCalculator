using FluentAssertions;
using InfraSizingCalculator.Models.Growth;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Growth;

/// <summary>
/// Tests for CustomGrowthRates model covering default initialization and dictionary behavior.
/// </summary>
public class CustomGrowthRatesTests
{
    #region Default Initialization Tests

    [Fact]
    public void Constructor_InitializesYearlyRatesDictionary()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.Should().NotBeNull();
    }

    [Fact]
    public void Default_HasFiveYears()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.Should().HaveCount(5);
    }

    [Fact]
    public void Default_Year1Is30Percent()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates[1].Should().Be(30);
    }

    [Fact]
    public void Default_Year2Is25Percent()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates[2].Should().Be(25);
    }

    [Fact]
    public void Default_Year3Is20Percent()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates[3].Should().Be(20);
    }

    [Fact]
    public void Default_Year4Is15Percent()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates[4].Should().Be(15);
    }

    [Fact]
    public void Default_Year5Is10Percent()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates[5].Should().Be(10);
    }

    [Fact]
    public void Default_AllYearsHaveDecreasingPattern()
    {
        var rates = new CustomGrowthRates();

        // Default pattern: 30 > 25 > 20 > 15 > 10 (decreasing growth rate)
        rates.YearlyRates[1].Should().BeGreaterThan(rates.YearlyRates[2]);
        rates.YearlyRates[2].Should().BeGreaterThan(rates.YearlyRates[3]);
        rates.YearlyRates[3].Should().BeGreaterThan(rates.YearlyRates[4]);
        rates.YearlyRates[4].Should().BeGreaterThan(rates.YearlyRates[5]);
    }

    [Fact]
    public void Default_ContainsAllYears1Through5()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.Should().ContainKeys(1, 2, 3, 4, 5);
    }

    #endregion

    #region Dictionary Modification Tests

    [Fact]
    public void YearlyRates_CanBeModified()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[1] = 50;

        rates.YearlyRates[1].Should().Be(50);
    }

    [Fact]
    public void YearlyRates_CanAddNewYear()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[6] = 5;

        rates.YearlyRates[6].Should().Be(5);
        rates.YearlyRates.Should().HaveCount(6);
    }

    [Fact]
    public void YearlyRates_CanRemoveYear()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates.Remove(5);

        rates.YearlyRates.Should().HaveCount(4);
        rates.YearlyRates.Should().NotContainKey(5);
    }

    [Fact]
    public void YearlyRates_CanBeClearedAndRepopulated()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates.Clear();
        rates.YearlyRates[1] = 100;

        rates.YearlyRates.Should().HaveCount(1);
        rates.YearlyRates[1].Should().Be(100);
    }

    [Fact]
    public void YearlyRates_CanBeReplacedEntirely()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates = new Dictionary<int, double>
        {
            { 1, 5 },
            { 2, 10 }
        };

        rates.YearlyRates.Should().HaveCount(2);
        rates.YearlyRates[1].Should().Be(5);
        rates.YearlyRates[2].Should().Be(10);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void YearlyRates_SupportsNegativeGrowth()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[1] = -10;

        rates.YearlyRates[1].Should().Be(-10);
    }

    [Fact]
    public void YearlyRates_SupportsZeroGrowth()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[1] = 0;

        rates.YearlyRates[1].Should().Be(0);
    }

    [Fact]
    public void YearlyRates_SupportsLargeValues()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[1] = 500;

        rates.YearlyRates[1].Should().Be(500);
    }

    [Fact]
    public void YearlyRates_SupportsFractionalValues()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[1] = 15.5;

        rates.YearlyRates[1].Should().Be(15.5);
    }

    [Fact]
    public void YearlyRates_SupportsNonSequentialYears()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[10] = 5;
        rates.YearlyRates[20] = 2;

        rates.YearlyRates[10].Should().Be(5);
        rates.YearlyRates[20].Should().Be(2);
    }

    [Fact]
    public void YearlyRates_SupportsYear0()
    {
        var rates = new CustomGrowthRates();
        rates.YearlyRates[0] = 100;

        rates.YearlyRates[0].Should().Be(100);
    }

    [Fact]
    public void YearlyRates_KeyNotFound_Throws()
    {
        var rates = new CustomGrowthRates();

        Action act = () => { var _ = rates.YearlyRates[99]; };

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void YearlyRates_TryGetValue_ReturnsFalseForMissingKey()
    {
        var rates = new CustomGrowthRates();

        var exists = rates.YearlyRates.TryGetValue(99, out var value);

        exists.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void YearlyRates_TryGetValue_ReturnsTrueForExistingKey()
    {
        var rates = new CustomGrowthRates();

        var exists = rates.YearlyRates.TryGetValue(1, out var value);

        exists.Should().BeTrue();
        value.Should().Be(30);
    }

    [Fact]
    public void YearlyRates_ContainsKey_ReturnsTrueForDefaultYears()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.ContainsKey(1).Should().BeTrue();
        rates.YearlyRates.ContainsKey(3).Should().BeTrue();
        rates.YearlyRates.ContainsKey(5).Should().BeTrue();
    }

    [Fact]
    public void YearlyRates_ContainsKey_ReturnsFalseForNonDefaultYears()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.ContainsKey(0).Should().BeFalse();
        rates.YearlyRates.ContainsKey(6).Should().BeFalse();
        rates.YearlyRates.ContainsKey(100).Should().BeFalse();
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void YearlyRates_CanBeEnumerated()
    {
        var rates = new CustomGrowthRates();
        var sum = 0.0;

        foreach (var kvp in rates.YearlyRates)
        {
            sum += kvp.Value;
        }

        // 30 + 25 + 20 + 15 + 10 = 100
        sum.Should().Be(100);
    }

    [Fact]
    public void YearlyRates_Keys_ReturnsAllYears()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void YearlyRates_Values_ReturnsAllRates()
    {
        var rates = new CustomGrowthRates();

        rates.YearlyRates.Values.Should().BeEquivalentTo(new[] { 30.0, 25.0, 20.0, 15.0, 10.0 });
    }

    #endregion

    #region Multiple Instance Independence

    [Fact]
    public void TwoInstances_HaveIndependentDictionaries()
    {
        var rates1 = new CustomGrowthRates();
        var rates2 = new CustomGrowthRates();

        rates1.YearlyRates[1] = 99;

        rates2.YearlyRates[1].Should().Be(30);
    }

    [Fact]
    public void ModifyingOneInstance_DoesNotAffectAnother()
    {
        var rates1 = new CustomGrowthRates();
        var rates2 = new CustomGrowthRates();

        rates1.YearlyRates.Clear();
        rates1.YearlyRates[1] = 50;

        rates2.YearlyRates.Should().HaveCount(5);
        rates2.YearlyRates[1].Should().Be(30);
    }

    #endregion
}
