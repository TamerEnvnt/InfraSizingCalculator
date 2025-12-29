using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InfraSizingCalculator.Models;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for settings model classes - ReplicaSettings, OvercommitSettings, HeadroomSettings
/// </summary>
public class SettingsModelsTests
{
    #region ReplicaSettings Tests

    [Fact]
    public void ReplicaSettings_DefaultValues_AreCorrect()
    {
        // Act
        var settings = new ReplicaSettings();

        // Assert
        settings.Prod.Should().Be(3);    // BR-R001
        settings.NonProd.Should().Be(1); // BR-R002
        settings.Stage.Should().Be(2);   // BR-R003
    }

    [Fact]
    public void ReplicaSettings_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new ReplicaSettings { Prod = 5, NonProd = 2, Stage = 3 };

        // Act
        var clone = original.Clone();
        clone.Prod = 10; // Modify clone

        // Assert
        original.Prod.Should().Be(5); // Original unchanged
        clone.Prod.Should().Be(10);
    }

    [Fact]
    public void ReplicaSettings_Clone_CopiesAllValues()
    {
        // Arrange
        var original = new ReplicaSettings { Prod = 4, NonProd = 2, Stage = 3 };

        // Act
        var clone = original.Clone();

        // Assert
        clone.Prod.Should().Be(4);
        clone.NonProd.Should().Be(2);
        clone.Stage.Should().Be(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public void ReplicaSettings_Validate_ProdOutOfRange_ReturnsError(int invalidValue)
    {
        // Arrange
        var settings = new ReplicaSettings { Prod = invalidValue };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Prod"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("BR-R004"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void ReplicaSettings_Validate_NonProdOutOfRange_ReturnsError(int invalidValue)
    {
        // Arrange
        var settings = new ReplicaSettings { NonProd = invalidValue };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("NonProd"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void ReplicaSettings_Validate_StageOutOfRange_ReturnsError(int invalidValue)
    {
        // Arrange
        var settings = new ReplicaSettings { Stage = invalidValue };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Stage"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void ReplicaSettings_Validate_ValidValues_NoErrors(int validValue)
    {
        // Arrange
        var settings = new ReplicaSettings
        {
            Prod = validValue,
            NonProd = validValue,
            Stage = validValue
        };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region OvercommitSettings Tests

    [Fact]
    public void OvercommitSettings_DefaultValues_AreCorrect()
    {
        // Act
        var settings = new OvercommitSettings();

        // Assert
        settings.Cpu.Should().Be(1.0);
        settings.Memory.Should().Be(1.0);
    }

    [Fact]
    public void OvercommitSettings_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new OvercommitSettings { Cpu = 2.5, Memory = 1.5 };

        // Act
        var clone = original.Clone();
        clone.Cpu = 5.0;

        // Assert
        original.Cpu.Should().Be(2.5);
        clone.Cpu.Should().Be(5.0);
    }

    [Fact]
    public void OvercommitSettings_Clone_CopiesAllValues()
    {
        // Arrange
        var original = new OvercommitSettings { Cpu = 3.0, Memory = 2.0 };

        // Act
        var clone = original.Clone();

        // Assert
        clone.Cpu.Should().Be(3.0);
        clone.Memory.Should().Be(2.0);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(10.5)]
    [InlineData(-1.0)]
    public void OvercommitSettings_Validate_CpuOutOfRange_ReturnsError(double invalidValue)
    {
        // Arrange
        var settings = new OvercommitSettings { Cpu = invalidValue };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Cpu"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("BR-V006"));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(4.5)]
    [InlineData(-1.0)]
    public void OvercommitSettings_Validate_MemoryOutOfRange_ReturnsError(double invalidValue)
    {
        // Arrange
        var settings = new OvercommitSettings { Memory = invalidValue };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Memory"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("BR-V007"));
    }

    [Fact]
    public void OvercommitSettings_Validate_ValidValues_NoErrors()
    {
        // Arrange
        var settings = new OvercommitSettings { Cpu = 5.0, Memory = 2.0 };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void OvercommitSettings_Validate_BoundaryValues_Valid()
    {
        // Arrange
        var settings = new OvercommitSettings { Cpu = 1.0, Memory = 4.0 };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region HeadroomSettings Tests

    [Fact]
    public void HeadroomSettings_DefaultValues_AreCorrect()
    {
        // Act
        var settings = new HeadroomSettings();

        // Assert
        settings.Dev.Should().Be(33.0);   // BR-H003
        settings.Test.Should().Be(33.0);  // BR-H004
        settings.Stage.Should().Be(0.0);  // BR-H005
        settings.Prod.Should().Be(37.5);  // BR-H006
        settings.DR.Should().Be(37.5);    // BR-H007
    }

    [Fact]
    public void HeadroomSettings_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new HeadroomSettings { Dev = 25.0, Test = 25.0, Stage = 10.0, Prod = 40.0, DR = 40.0 };

        // Act
        var clone = original.Clone();
        clone.Dev = 50.0;

        // Assert
        original.Dev.Should().Be(25.0);
        clone.Dev.Should().Be(50.0);
    }

    [Fact]
    public void HeadroomSettings_Clone_CopiesAllValues()
    {
        // Arrange
        var original = new HeadroomSettings { Dev = 10, Test = 20, Stage = 30, Prod = 40, DR = 50 };

        // Act
        var clone = original.Clone();

        // Assert
        clone.Dev.Should().Be(10);
        clone.Test.Should().Be(20);
        clone.Stage.Should().Be(30);
        clone.Prod.Should().Be(40);
        clone.DR.Should().Be(50);
    }

    [Theory]
    [InlineData("Dev", -1.0)]
    [InlineData("Dev", 101.0)]
    [InlineData("Test", -5.0)]
    [InlineData("Test", 150.0)]
    [InlineData("Stage", -0.1)]
    [InlineData("Prod", 100.1)]
    [InlineData("DR", 200.0)]
    public void HeadroomSettings_Validate_OutOfRange_ReturnsError(string field, double value)
    {
        // Arrange
        var settings = new HeadroomSettings();
        typeof(HeadroomSettings).GetProperty(field)!.SetValue(settings, value);
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains(field));
        results.Should().Contain(r => r.ErrorMessage!.Contains("BR-H008"));
    }

    [Fact]
    public void HeadroomSettings_Validate_ValidValues_NoErrors()
    {
        // Arrange
        var settings = new HeadroomSettings { Dev = 0, Test = 50, Stage = 100, Prod = 25, DR = 75 };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void HeadroomSettings_Validate_MultipleInvalid_ReturnsMultipleErrors()
    {
        // Arrange
        var settings = new HeadroomSettings { Dev = -10, Test = 150, Stage = 0, Prod = 50, DR = 50 };
        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.MemberNames.Contains("Dev"));
        results.Should().Contain(r => r.MemberNames.Contains("Test"));
    }

    #endregion
}
