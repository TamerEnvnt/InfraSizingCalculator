using FluentAssertions;
using InfraSizingCalculator.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for ReplicaSettings model validating business rules BR-R001 through BR-R004.
/// </summary>
public class ReplicaSettingsTests
{
    #region Default Values Tests (BR-R001, BR-R002, BR-R003)

    [Fact]
    public void DefaultValues_Prod_IsThree()
    {
        // BR-R001: Default replica count for Production = 3
        var settings = new ReplicaSettings();
        settings.Prod.Should().Be(3);
    }

    [Fact]
    public void DefaultValues_NonProd_IsOne()
    {
        // BR-R002: Default replica count for Development and Test = 1
        var settings = new ReplicaSettings();
        settings.NonProd.Should().Be(1);
    }

    [Fact]
    public void DefaultValues_Stage_IsTwo()
    {
        // BR-R003: Default replica count for Staging = 2
        var settings = new ReplicaSettings();
        settings.Stage.Should().Be(2);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new ReplicaSettings { Prod = 5, NonProd = 2, Stage = 3 };
        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Prod.Should().Be(5);
        clone.NonProd.Should().Be(2);
        clone.Stage.Should().Be(3);
    }

    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new ReplicaSettings { Prod = 5, NonProd = 2, Stage = 3 };
        var clone = original.Clone();

        clone.Prod = 10;
        clone.NonProd = 10;
        clone.Stage = 10;

        original.Prod.Should().Be(5);
        original.NonProd.Should().Be(2);
        original.Stage.Should().Be(3);
    }

    [Fact]
    public void Clone_WithDefaultValues_ClonesCorrectly()
    {
        var original = new ReplicaSettings();
        var clone = original.Clone();

        clone.Prod.Should().Be(3);
        clone.NonProd.Should().Be(1);
        clone.Stage.Should().Be(2);
    }

    #endregion

    #region Validate Tests (BR-R004)

    [Fact]
    public void Validate_WithValidValues_ReturnsNoErrors()
    {
        var settings = new ReplicaSettings { Prod = 3, NonProd = 1, Stage = 2 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_WithValidProdValues_ReturnsNoErrors(int value)
    {
        var settings = new ReplicaSettings { Prod = value, NonProd = 1, Stage = 2 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithProdBelowMinimum_ReturnsError(int value)
    {
        var settings = new ReplicaSettings { Prod = value, NonProd = 1, Stage = 2 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Prod");
        results[0].ErrorMessage.Should().Contain("BR-R004");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Validate_WithProdAboveMaximum_ReturnsError(int value)
    {
        var settings = new ReplicaSettings { Prod = value, NonProd = 1, Stage = 2 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Prod");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonProdBelowMinimum_ReturnsError(int value)
    {
        var settings = new ReplicaSettings { Prod = 3, NonProd = value, Stage = 2 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("NonProd");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(100)]
    public void Validate_WithNonProdAboveMaximum_ReturnsError(int value)
    {
        var settings = new ReplicaSettings { Prod = 3, NonProd = value, Stage = 2 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("NonProd");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithStageBelowMinimum_ReturnsError(int value)
    {
        var settings = new ReplicaSettings { Prod = 3, NonProd = 1, Stage = value };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Stage");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(100)]
    public void Validate_WithStageAboveMaximum_ReturnsError(int value)
    {
        var settings = new ReplicaSettings { Prod = 3, NonProd = 1, Stage = value };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Stage");
    }

    [Fact]
    public void Validate_WithMultipleInvalidValues_ReturnsMultipleErrors()
    {
        var settings = new ReplicaSettings { Prod = 0, NonProd = 0, Stage = 0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(3);
        results.SelectMany(r => r.MemberNames).Should().Contain(new[] { "Prod", "NonProd", "Stage" });
    }

    [Fact]
    public void Validate_AtBoundaryValues_ReturnsNoErrors()
    {
        var settings = new ReplicaSettings { Prod = 1, NonProd = 10, Stage = 5 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region DataAnnotation Attribute Tests

    [Fact]
    public void Prod_HasRangeAttribute()
    {
        var property = typeof(ReplicaSettings).GetProperty(nameof(ReplicaSettings.Prod));
        var attribute = property!.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault() as RangeAttribute;

        attribute.Should().NotBeNull();
        attribute!.Minimum.Should().Be(1);
        attribute.Maximum.Should().Be(10);
    }

    [Fact]
    public void NonProd_HasRangeAttribute()
    {
        var property = typeof(ReplicaSettings).GetProperty(nameof(ReplicaSettings.NonProd));
        var attribute = property!.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault() as RangeAttribute;

        attribute.Should().NotBeNull();
        attribute!.Minimum.Should().Be(1);
        attribute.Maximum.Should().Be(10);
    }

    [Fact]
    public void Stage_HasRangeAttribute()
    {
        var property = typeof(ReplicaSettings).GetProperty(nameof(ReplicaSettings.Stage));
        var attribute = property!.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault() as RangeAttribute;

        attribute.Should().NotBeNull();
        attribute!.Minimum.Should().Be(1);
        attribute.Maximum.Should().Be(10);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Prod_CanBeSetAndRetrieved()
    {
        var settings = new ReplicaSettings { Prod = 7 };
        settings.Prod.Should().Be(7);
    }

    [Fact]
    public void NonProd_CanBeSetAndRetrieved()
    {
        var settings = new ReplicaSettings { NonProd = 4 };
        settings.NonProd.Should().Be(4);
    }

    [Fact]
    public void Stage_CanBeSetAndRetrieved()
    {
        var settings = new ReplicaSettings { Stage = 5 };
        settings.Stage.Should().Be(5);
    }

    #endregion
}
