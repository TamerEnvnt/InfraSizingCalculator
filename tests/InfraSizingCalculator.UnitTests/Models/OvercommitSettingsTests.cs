using FluentAssertions;
using InfraSizingCalculator.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for OvercommitSettings model validating business rules BR-V006 and BR-V007.
/// </summary>
public class OvercommitSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_CpuIsOne()
    {
        var settings = new OvercommitSettings();
        settings.Cpu.Should().Be(1.0);
    }

    [Fact]
    public void DefaultValues_MemoryIsOne()
    {
        var settings = new OvercommitSettings();
        settings.Memory.Should().Be(1.0);
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new OvercommitSettings { Cpu = 4.0, Memory = 2.0 };
        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Cpu.Should().Be(4.0);
        clone.Memory.Should().Be(2.0);
    }

    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new OvercommitSettings { Cpu = 4.0, Memory = 2.0 };
        var clone = original.Clone();

        clone.Cpu = 8.0;
        clone.Memory = 3.5;

        original.Cpu.Should().Be(4.0);
        original.Memory.Should().Be(2.0);
    }

    [Fact]
    public void Clone_WithDefaultValues_ClonesCorrectly()
    {
        var original = new OvercommitSettings();
        var clone = original.Clone();

        clone.Cpu.Should().Be(1.0);
        clone.Memory.Should().Be(1.0);
    }

    #endregion

    #region Validate Tests (BR-V006, BR-V007)

    [Fact]
    public void Validate_WithValidValues_ReturnsNoErrors()
    {
        var settings = new OvercommitSettings { Cpu = 4.0, Memory = 2.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void Validate_WithValidCpuValues_ReturnsNoErrors(double value)
    {
        // BR-V006: CPU overcommit ratio must be between 1 and 10
        var settings = new OvercommitSettings { Cpu = value, Memory = 1.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.99)]
    [InlineData(-1.0)]
    public void Validate_WithCpuBelowMinimum_ReturnsError(double value)
    {
        var settings = new OvercommitSettings { Cpu = value, Memory = 1.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Cpu");
        results[0].ErrorMessage.Should().Contain("BR-V006");
    }

    [Theory]
    [InlineData(10.01)]
    [InlineData(11.0)]
    [InlineData(100.0)]
    public void Validate_WithCpuAboveMaximum_ReturnsError(double value)
    {
        var settings = new OvercommitSettings { Cpu = value, Memory = 1.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Cpu");
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(4.0)]
    public void Validate_WithValidMemoryValues_ReturnsNoErrors(double value)
    {
        // BR-V007: Memory overcommit ratio must be between 1 and 4
        var settings = new OvercommitSettings { Cpu = 1.0, Memory = value };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.99)]
    [InlineData(-1.0)]
    public void Validate_WithMemoryBelowMinimum_ReturnsError(double value)
    {
        var settings = new OvercommitSettings { Cpu = 1.0, Memory = value };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Memory");
        results[0].ErrorMessage.Should().Contain("BR-V007");
    }

    [Theory]
    [InlineData(4.01)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void Validate_WithMemoryAboveMaximum_ReturnsError(double value)
    {
        var settings = new OvercommitSettings { Cpu = 1.0, Memory = value };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Memory");
    }

    [Fact]
    public void Validate_WithBothInvalid_ReturnsTwoErrors()
    {
        var settings = new OvercommitSettings { Cpu = 0.5, Memory = 5.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().HaveCount(2);
        results.SelectMany(r => r.MemberNames).Should().Contain(new[] { "Cpu", "Memory" });
    }

    [Fact]
    public void Validate_AtBoundaryValues_ReturnsNoErrors()
    {
        var settings = new OvercommitSettings { Cpu = 1.0, Memory = 4.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AtUpperBoundaryValues_ReturnsNoErrors()
    {
        var settings = new OvercommitSettings { Cpu = 10.0, Memory = 4.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region DataAnnotation Attribute Tests

    [Fact]
    public void Cpu_HasRangeAttribute()
    {
        var property = typeof(OvercommitSettings).GetProperty(nameof(OvercommitSettings.Cpu));
        var attribute = property!.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault() as RangeAttribute;

        attribute.Should().NotBeNull();
        attribute!.Minimum.Should().Be(1.0);
        attribute.Maximum.Should().Be(10.0);
    }

    [Fact]
    public void Memory_HasRangeAttribute()
    {
        var property = typeof(OvercommitSettings).GetProperty(nameof(OvercommitSettings.Memory));
        var attribute = property!.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault() as RangeAttribute;

        attribute.Should().NotBeNull();
        attribute!.Minimum.Should().Be(1.0);
        attribute.Maximum.Should().Be(4.0);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Cpu_CanBeSetAndRetrieved()
    {
        var settings = new OvercommitSettings { Cpu = 5.5 };
        settings.Cpu.Should().Be(5.5);
    }

    [Fact]
    public void Memory_CanBeSetAndRetrieved()
    {
        var settings = new OvercommitSettings { Memory = 3.5 };
        settings.Memory.Should().Be(3.5);
    }

    [Fact]
    public void Cpu_SupportsDecimalValues()
    {
        var settings = new OvercommitSettings { Cpu = 4.75 };
        settings.Cpu.Should().Be(4.75);
    }

    [Fact]
    public void Memory_SupportsDecimalValues()
    {
        var settings = new OvercommitSettings { Memory = 2.25 };
        settings.Memory.Should().Be(2.25);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_WithDefaultInstance_ReturnsNoErrors()
    {
        var settings = new OvercommitSettings();
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1.001)]
    [InlineData(9.999)]
    public void Validate_WithValuesNearBoundary_ReturnsNoErrors(double cpuValue)
    {
        var settings = new OvercommitSettings { Cpu = cpuValue, Memory = 1.0 };
        var context = new ValidationContext(settings);
        var results = settings.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    #endregion
}
