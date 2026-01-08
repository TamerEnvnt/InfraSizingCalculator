using FluentAssertions;
using InfraSizingCalculator.Services.Validation;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.Validation;

/// <summary>
/// Tests for ValidationResult model covering all factory methods and properties.
/// </summary>
public class ValidationResultTests
{
    #region Constructor Tests

    [Fact]
    public void DefaultConstructor_CreatesValidResult()
    {
        var result = new ValidationResult();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyErrors_CreatesValidResult()
    {
        var result = new ValidationResult(Array.Empty<string>());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithSingleError_CreatesInvalidResult()
    {
        var result = new ValidationResult(new[] { "Error message" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be("Error message");
    }

    [Fact]
    public void Constructor_WithMultipleErrors_CreatesInvalidResult()
    {
        var errors = new[] { "Error 1", "Error 2", "Error 3" };
        var result = new ValidationResult(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().ContainInOrder("Error 1", "Error 2", "Error 3");
    }

    [Fact]
    public void Constructor_WithListErrors_CreatesReadOnlyList()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var result = new ValidationResult(errors);

        result.Errors.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    #endregion

    #region Static Factory Method Tests

    [Fact]
    public void Success_ReturnsValidResult()
    {
        var result = ValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_ReturnsInvalidResult()
    {
        var result = ValidationResult.Failure("Single error");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be("Single error");
    }

    [Fact]
    public void Failure_WithMultipleErrors_ReturnsInvalidResult()
    {
        var result = ValidationResult.Failure("Error 1", "Error 2", "Error 3");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void Failure_WithParamsArray_ReturnsInvalidResult()
    {
        var result = ValidationResult.Failure("A", "B", "C", "D");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
    }

    [Fact]
    public void Failure_WithEnumerable_ReturnsInvalidResult()
    {
        IEnumerable<string> errors = new[] { "Err1", "Err2" };
        var result = ValidationResult.Failure(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Failure_WithEmptyParams_ReturnsValidResult()
    {
        var result = ValidationResult.Failure();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region IsValid Property Tests

    [Fact]
    public void IsValid_WhenNoErrors_ReturnsTrue()
    {
        var result = new ValidationResult(Enumerable.Empty<string>());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenHasErrors_ReturnsFalse()
    {
        var result = new ValidationResult(new[] { "error" });
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Errors Property Tests

    [Fact]
    public void Errors_IsReadOnly()
    {
        var result = ValidationResult.Failure("error");

        // The Errors property should be read-only
        result.Errors.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public void Errors_PreservesOrder()
    {
        var result = ValidationResult.Failure("First", "Second", "Third");

        result.Errors[0].Should().Be("First");
        result.Errors[1].Should().Be("Second");
        result.Errors[2].Should().Be("Third");
    }

    [Fact]
    public void Errors_CanBeEnumerated()
    {
        var result = ValidationResult.Failure("A", "B");
        var collected = result.Errors.ToList();

        collected.Should().HaveCount(2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MultipleSuccessCalls_ReturnIndependentInstances()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        result1.Should().NotBeSameAs(result2);
    }

    [Fact]
    public void Failure_WithNullInArray_HandlesGracefully()
    {
        var result = ValidationResult.Failure("Valid", null!, "Also Valid");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void Failure_WithEmptyStringError_StillInvalid()
    {
        var result = ValidationResult.Failure("");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Failure_WithWhitespaceError_StillInvalid()
    {
        var result = ValidationResult.Failure("   ");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    #endregion
}
