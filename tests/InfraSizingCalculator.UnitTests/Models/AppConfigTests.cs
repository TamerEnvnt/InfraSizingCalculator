using System.ComponentModel.DataAnnotations;
using InfraSizingCalculator.Models;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

public class AppConfigTests
{
    #region Constructor and Properties

    [Fact]
    public void Constructor_DefaultValues_AllZero()
    {
        var config = new AppConfig();

        Assert.Equal(0, config.Small);
        Assert.Equal(0, config.Medium);
        Assert.Equal(0, config.Large);
        Assert.Equal(0, config.XLarge);
    }

    [Fact]
    public void TotalApps_CalculatesCorrectSum()
    {
        var config = new AppConfig
        {
            Small = 5,
            Medium = 10,
            Large = 3,
            XLarge = 2
        };

        Assert.Equal(20, config.TotalApps);
    }

    [Fact]
    public void TotalApps_AllZero_ReturnsZero()
    {
        var config = new AppConfig();
        Assert.Equal(0, config.TotalApps);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new AppConfig
        {
            Small = 5,
            Medium = 10,
            Large = 3,
            XLarge = 2
        };

        var clone = original.Clone();

        Assert.Equal(original.Small, clone.Small);
        Assert.Equal(original.Medium, clone.Medium);
        Assert.Equal(original.Large, clone.Large);
        Assert.Equal(original.XLarge, clone.XLarge);
    }

    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new AppConfig
        {
            Small = 5,
            Medium = 10,
            Large = 3,
            XLarge = 2
        };

        var clone = original.Clone();
        clone.Small = 99;
        clone.Medium = 88;

        Assert.Equal(5, original.Small);
        Assert.Equal(10, original.Medium);
    }

    [Fact]
    public void Clone_EmptyConfig_ClonesCorrectly()
    {
        var original = new AppConfig();
        var clone = original.Clone();

        Assert.Equal(0, clone.Small);
        Assert.Equal(0, clone.Medium);
        Assert.Equal(0, clone.Large);
        Assert.Equal(0, clone.XLarge);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Validate_ValidConfig_ReturnsNoErrors()
    {
        var config = new AppConfig
        {
            Small = 5,
            Medium = 10,
            Large = 3,
            XLarge = 2
        };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_AllZero_ReturnsNoErrors()
    {
        var config = new AppConfig();

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_NegativeSmall_ReturnsError()
    {
        var config = new AppConfig { Small = -1 };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Single(results);
        Assert.Contains("Small", results[0].MemberNames);
        Assert.Contains("BR-V001", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NegativeMedium_ReturnsError()
    {
        var config = new AppConfig { Medium = -1 };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Single(results);
        Assert.Contains("Medium", results[0].MemberNames);
        Assert.Contains("BR-V001", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NegativeLarge_ReturnsError()
    {
        var config = new AppConfig { Large = -1 };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Single(results);
        Assert.Contains("Large", results[0].MemberNames);
        Assert.Contains("BR-V001", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NegativeXLarge_ReturnsError()
    {
        var config = new AppConfig { XLarge = -1 };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Single(results);
        Assert.Contains("XLarge", results[0].MemberNames);
        Assert.Contains("BR-V001", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_MultipleNegativeValues_ReturnsMultipleErrors()
    {
        var config = new AppConfig
        {
            Small = -1,
            Medium = -2,
            Large = -3,
            XLarge = -4
        };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public void Validate_MixedValidAndInvalid_ReturnsOnlyInvalidErrors()
    {
        var config = new AppConfig
        {
            Small = 5,  // valid
            Medium = -1,  // invalid
            Large = 3,   // valid
            XLarge = -2  // invalid
        };

        var results = config.Validate(new ValidationContext(config)).ToList();

        Assert.Equal(2, results.Count);
    }

    #endregion

    #region DataAnnotation Range Attribute Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void RangeAttribute_ValidValues_PassValidation(int value)
    {
        var config = new AppConfig { Small = value };

        var context = new ValidationContext(config) { MemberName = nameof(AppConfig.Small) };
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateProperty(
            config.Small,
            context,
            results);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void RangeAttribute_NegativeValue_FailsValidation()
    {
        var config = new AppConfig { Small = -1 };

        var context = new ValidationContext(config) { MemberName = nameof(AppConfig.Small) };
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateProperty(
            config.Small,
            context,
            results);

        Assert.False(isValid);
        Assert.Single(results);
    }

    #endregion
}
