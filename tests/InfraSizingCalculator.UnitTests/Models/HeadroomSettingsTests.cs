using FluentAssertions;
using InfraSizingCalculator.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for HeadroomSettings model covering properties, validation, and Clone() method.
/// Tests business rules BR-H003 through BR-H008.
/// </summary>
public class HeadroomSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_MatchBusinessRules()
    {
        var settings = new HeadroomSettings();

        // BR-H003: Default headroom for Development = 33%
        settings.Dev.Should().Be(33.0);
        // BR-H004: Default headroom for Test = 33%
        settings.Test.Should().Be(33.0);
        // BR-H005: Default headroom for Staging = 0%
        settings.Stage.Should().Be(0.0);
        // BR-H006: Default headroom for Production = 37.5%
        settings.Prod.Should().Be(37.5);
        // BR-H007: Default headroom for DR = 37.5%
        settings.DR.Should().Be(37.5);
    }

    #endregion

    #region Property Setting Tests

    [Fact]
    public void Dev_CanBeSet()
    {
        var settings = new HeadroomSettings { Dev = 50.0 };
        settings.Dev.Should().Be(50.0);
    }

    [Fact]
    public void Test_CanBeSet()
    {
        var settings = new HeadroomSettings { Test = 45.0 };
        settings.Test.Should().Be(45.0);
    }

    [Fact]
    public void Stage_CanBeSet()
    {
        var settings = new HeadroomSettings { Stage = 25.0 };
        settings.Stage.Should().Be(25.0);
    }

    [Fact]
    public void Prod_CanBeSet()
    {
        var settings = new HeadroomSettings { Prod = 50.0 };
        settings.Prod.Should().Be(50.0);
    }

    [Fact]
    public void DR_CanBeSet()
    {
        var settings = new HeadroomSettings { DR = 50.0 };
        settings.DR.Should().Be(50.0);
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        var settings = new HeadroomSettings
        {
            Dev = 10.0,
            Test = 20.0,
            Stage = 30.0,
            Prod = 40.0,
            DR = 50.0
        };

        settings.Dev.Should().Be(10.0);
        settings.Test.Should().Be(20.0);
        settings.Stage.Should().Be(30.0);
        settings.Prod.Should().Be(40.0);
        settings.DR.Should().Be(50.0);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_CreatesNewInstance()
    {
        var original = new HeadroomSettings();
        var cloned = original.Clone();

        cloned.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Clone_CopiesAllDefaultValues()
    {
        var original = new HeadroomSettings();
        var cloned = original.Clone();

        cloned.Dev.Should().Be(original.Dev);
        cloned.Test.Should().Be(original.Test);
        cloned.Stage.Should().Be(original.Stage);
        cloned.Prod.Should().Be(original.Prod);
        cloned.DR.Should().Be(original.DR);
    }

    [Fact]
    public void Clone_CopiesCustomValues()
    {
        var original = new HeadroomSettings
        {
            Dev = 10.0,
            Test = 20.0,
            Stage = 30.0,
            Prod = 40.0,
            DR = 50.0
        };

        var cloned = original.Clone();

        cloned.Dev.Should().Be(10.0);
        cloned.Test.Should().Be(20.0);
        cloned.Stage.Should().Be(30.0);
        cloned.Prod.Should().Be(40.0);
        cloned.DR.Should().Be(50.0);
    }

    [Fact]
    public void Clone_IsIndependent()
    {
        var original = new HeadroomSettings { Dev = 20.0 };
        var cloned = original.Clone();

        cloned.Dev = 50.0;

        original.Dev.Should().Be(20.0);
        cloned.Dev.Should().Be(50.0);
    }

    #endregion

    #region Validation Tests - BR-H008

    [Fact]
    public void Validate_AllValuesValid_ReturnsNoErrors()
    {
        var settings = new HeadroomSettings();
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AllZeroValues_ReturnsNoErrors()
    {
        var settings = new HeadroomSettings
        {
            Dev = 0.0,
            Test = 0.0,
            Stage = 0.0,
            Prod = 0.0,
            DR = 0.0
        };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AllMaxValues_ReturnsNoErrors()
    {
        var settings = new HeadroomSettings
        {
            Dev = 100.0,
            Test = 100.0,
            Stage = 100.0,
            Prod = 100.0,
            DR = 100.0
        };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_DevNegative_ReturnsError()
    {
        var settings = new HeadroomSettings { Dev = -1.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Dev");
        results[0].ErrorMessage.Should().Contain("BR-H008");
    }

    [Fact]
    public void Validate_DevOver100_ReturnsError()
    {
        var settings = new HeadroomSettings { Dev = 101.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Dev");
    }

    [Fact]
    public void Validate_TestNegative_ReturnsError()
    {
        var settings = new HeadroomSettings { Test = -0.5 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Test");
    }

    [Fact]
    public void Validate_StageNegative_ReturnsError()
    {
        var settings = new HeadroomSettings { Stage = -10.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Stage");
    }

    [Fact]
    public void Validate_ProdNegative_ReturnsError()
    {
        var settings = new HeadroomSettings { Prod = -5.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Prod");
    }

    [Fact]
    public void Validate_DROver100_ReturnsError()
    {
        var settings = new HeadroomSettings { DR = 150.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("DR");
    }

    [Fact]
    public void Validate_MultipleInvalidValues_ReturnsMultipleErrors()
    {
        var settings = new HeadroomSettings
        {
            Dev = -1.0,
            Test = 200.0,
            Stage = -50.0
        };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(3);
    }

    [Fact]
    public void Validate_AllInvalidValues_ReturnsFiveErrors()
    {
        var settings = new HeadroomSettings
        {
            Dev = -1.0,
            Test = -2.0,
            Stage = -3.0,
            Prod = 101.0,
            DR = 102.0
        };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.1)]
    [InlineData(50.0)]
    [InlineData(99.9)]
    [InlineData(100.0)]
    public void Validate_ValidBoundaryValues_ReturnsNoErrors(double value)
    {
        var settings = new HeadroomSettings { Dev = value };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-0.001)]
    [InlineData(-100.0)]
    [InlineData(100.001)]
    [InlineData(1000.0)]
    public void Validate_InvalidBoundaryValues_ReturnsError(double value)
    {
        var settings = new HeadroomSettings { Dev = value };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
    }

    #endregion

    #region DataAnnotation Validation Tests

    [Fact]
    public void DataAnnotations_Dev_HasRangeAttribute()
    {
        var property = typeof(HeadroomSettings).GetProperty(nameof(HeadroomSettings.Dev));
        var rangeAttr = property!.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() as RangeAttribute;

        rangeAttr.Should().NotBeNull();
        rangeAttr!.Minimum.Should().Be(0.0);
        rangeAttr.Maximum.Should().Be(100.0);
    }

    [Fact]
    public void DataAnnotations_AllProperties_HaveRangeAttributes()
    {
        var properties = new[] { "Dev", "Test", "Stage", "Prod", "DR" };

        foreach (var propName in properties)
        {
            var property = typeof(HeadroomSettings).GetProperty(propName);
            var rangeAttr = property!.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() as RangeAttribute;

            rangeAttr.Should().NotBeNull($"{propName} should have RangeAttribute");
            rangeAttr!.Minimum.Should().Be(0.0);
            rangeAttr.Maximum.Should().Be(100.0);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_ExactZeroBoundary_IsValid()
    {
        var settings = new HeadroomSettings { Dev = 0.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ExactHundredBoundary_IsValid()
    {
        var settings = new HeadroomSettings { Prod = 100.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_IncludesMemberName()
    {
        var settings = new HeadroomSettings { Dev = -1.0 };
        var context = new ValidationContext(settings);

        var results = settings.Validate(context).ToList();

        results[0].MemberNames.Should().Contain("Dev");
    }

    #endregion
}
