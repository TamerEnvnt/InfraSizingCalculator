using FluentAssertions;
using InfraSizingCalculator.Services.Validation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.Validation;

public class InputValidationServiceTests
{
    private readonly ILogger<InputValidationService> _logger;
    private readonly InputValidationService _service;

    public InputValidationServiceTests()
    {
        _logger = Substitute.For<ILogger<InputValidationService>>();
        _service = new InputValidationService(_logger);
    }

    #region ValidateAppCounts Tests

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(10, 5, 2)]
    [InlineData(1000, 500, 100)] // Maximum valid values
    public void ValidateAppCounts_ValidCounts_ReturnsSuccess(int small, int medium, int large)
    {
        // Act
        var result = _service.ValidateAppCounts(small, medium, large);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAppCounts_AllZero_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(0, 0, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("At least one application"));
    }

    [Fact]
    public void ValidateAppCounts_NegativeSmall_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(-1, 0, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Small app count cannot be negative"));
    }

    [Fact]
    public void ValidateAppCounts_NegativeMedium_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(1, -1, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Medium app count cannot be negative"));
    }

    [Fact]
    public void ValidateAppCounts_NegativeLarge_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(1, 0, -1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Large app count cannot be negative"));
    }

    [Fact]
    public void ValidateAppCounts_SmallExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(1001, 0, 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Small app count cannot exceed 1000"));
    }

    [Fact]
    public void ValidateAppCounts_MediumExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(1, 501, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Medium app count cannot exceed 500"));
    }

    [Fact]
    public void ValidateAppCounts_LargeExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateAppCounts(1, 0, 101);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Large app count cannot exceed 100"));
    }

    [Fact]
    public void ValidateAppCounts_MultipleErrors_ReturnsAllErrors()
    {
        // Act
        var result = _service.ValidateAppCounts(-1, -1, -1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region ValidateNodeSpecs Tests

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(4, 8, 100)]
    [InlineData(256, 1024, 10000)] // Maximum valid values
    public void ValidateNodeSpecs_ValidSpecs_ReturnsSuccess(int cpu, int memory, int storage)
    {
        // Act
        var result = _service.ValidateNodeSpecs(cpu, memory, storage);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateNodeSpecs_ZeroCpu_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(0, 8, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CPU cores must be at least 1"));
    }

    [Fact]
    public void ValidateNodeSpecs_NegativeCpu_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(-1, 8, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CPU cores must be at least 1"));
    }

    [Fact]
    public void ValidateNodeSpecs_CpuExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(257, 8, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CPU cores cannot exceed 256"));
    }

    [Fact]
    public void ValidateNodeSpecs_ZeroMemory_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(4, 0, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Memory must be at least 1 GB"));
    }

    [Fact]
    public void ValidateNodeSpecs_MemoryExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(4, 1025, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Memory cannot exceed 1024 GB"));
    }

    [Fact]
    public void ValidateNodeSpecs_ZeroStorage_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(4, 8, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Storage must be at least 1 GB"));
    }

    [Fact]
    public void ValidateNodeSpecs_StorageExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateNodeSpecs(4, 8, 10001);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Storage cannot exceed 10000 GB"));
    }

    #endregion

    #region ValidatePricing Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.5, 100)]
    [InlineData(10000, 1000000)] // Maximum valid values
    public void ValidatePricing_ValidRates_ReturnsSuccess(decimal hourly, decimal monthly)
    {
        // Act
        var result = _service.ValidatePricing(hourly, monthly);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePricing_NegativeHourly_ReturnsError()
    {
        // Act
        var result = _service.ValidatePricing(-1, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Hourly rate cannot be negative"));
    }

    [Fact]
    public void ValidatePricing_HourlyExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidatePricing(10001, 100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Hourly rate exceeds reasonable maximum"));
    }

    [Fact]
    public void ValidatePricing_NegativeMonthly_ReturnsError()
    {
        // Act
        var result = _service.ValidatePricing(1, -100);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Monthly rate cannot be negative"));
    }

    [Fact]
    public void ValidatePricing_MonthlyExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidatePricing(1, 1000001);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Monthly rate exceeds reasonable maximum"));
    }

    #endregion

    #region ValidateGrowthRate Tests

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)] // Maximum valid
    public void ValidateGrowthRate_ValidRate_ReturnsSuccess(decimal rate)
    {
        // Act
        var result = _service.ValidateGrowthRate(rate);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGrowthRate_NegativeRate_ReturnsError()
    {
        // Act
        var result = _service.ValidateGrowthRate(-1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Growth rate cannot be negative"));
    }

    [Fact]
    public void ValidateGrowthRate_ExceedsMax_ReturnsError()
    {
        // Act
        var result = _service.ValidateGrowthRate(501);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Growth rate cannot exceed 500%"));
    }

    #endregion

    #region SanitizeScenarioName Tests

    [Theory]
    [InlineData("My Scenario", "My Scenario")]
    [InlineData("Test_Scenario-123", "Test_Scenario-123")]
    [InlineData("Simple Name", "Simple Name")]
    public void SanitizeScenarioName_ValidName_ReturnsSame(string input, string expected)
    {
        // Act
        var result = _service.SanitizeScenarioName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeScenarioName_EmptyInput_ReturnsDefault(string? input)
    {
        // Act
        var result = _service.SanitizeScenarioName(input!);

        // Assert
        result.Should().Be("Untitled Scenario");
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>", "scriptalert(xss)/script")]
    [InlineData("Test<div>", "Testdiv")]
    [InlineData("Name&More", "NameMore")]
    public void SanitizeScenarioName_DangerousChars_RemovesThem(string input, string expected)
    {
        // Act
        var result = _service.SanitizeScenarioName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SanitizeScenarioName_MultipleSpaces_NormalizesWhitespace()
    {
        // Arrange
        var input = "Test    Multiple    Spaces";

        // Act
        var result = _service.SanitizeScenarioName(input);

        // Assert
        result.Should().Be("Test Multiple Spaces");
    }

    [Fact]
    public void SanitizeScenarioName_LeadingTrailingSpaces_TrimsSpaces()
    {
        // Arrange
        var input = "   Test Name   ";

        // Act
        var result = _service.SanitizeScenarioName(input);

        // Assert
        result.Should().Be("Test Name");
    }

    [Fact]
    public void SanitizeScenarioName_ExceedsMaxLength_Truncates()
    {
        // Arrange
        var input = new string('A', 150);

        // Act
        var result = _service.SanitizeScenarioName(input);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void SanitizeScenarioName_OnlyDangerousChars_ReturnsDefault()
    {
        // Arrange
        var input = "<>\"'&;\\";

        // Act
        var result = _service.SanitizeScenarioName(input);

        // Assert
        result.Should().Be("Untitled Scenario");
    }

    #endregion

    #region SanitizeText Tests

    [Theory]
    [InlineData("Simple text", "Simple text")]
    [InlineData("With numbers 123", "With numbers 123")]
    public void SanitizeText_ValidText_ReturnsSame(string input, string expected)
    {
        // Act
        var result = _service.SanitizeText(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeText_EmptyInput_ReturnsEmpty(string? input)
    {
        // Act
        var result = _service.SanitizeText(input!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeText_HtmlTags_RemovesThem()
    {
        // Arrange
        var input = "<div>Content</div>";

        // Act
        var result = _service.SanitizeText(input);

        // Assert
        result.Should().NotContain("<");
        result.Should().NotContain(">");
    }

    [Fact]
    public void SanitizeText_ScriptTags_RemovesThem()
    {
        // Arrange
        var input = "<script>alert('xss')</script>";

        // Act
        var result = _service.SanitizeText(input);

        // Assert
        result.Should().NotContain("script");
        result.Should().NotContain("<");
    }

    [Fact]
    public void SanitizeText_ExceedsDefaultMaxLength_Truncates()
    {
        // Arrange
        var input = new string('A', 600);

        // Act
        var result = _service.SanitizeText(input);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(500);
    }

    [Fact]
    public void SanitizeText_CustomMaxLength_Truncates()
    {
        // Arrange
        var input = new string('A', 200);

        // Act
        var result = _service.SanitizeText(input, 50);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(50);
    }

    [Fact]
    public void SanitizeText_MultipleSpaces_Normalizes()
    {
        // Arrange
        var input = "Test   multiple   spaces";

        // Act
        var result = _service.SanitizeText(input);

        // Assert
        result.Should().Be("Test multiple spaces");
    }

    #endregion
}

public class ValidationResultTests
{
    #region Constructor Tests

    [Fact]
    public void ValidationResult_DefaultConstructor_IsValid()
    {
        // Arrange & Act
        var result = new ValidationResult();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_NoErrors_IsValid()
    {
        // Arrange & Act
        var result = new ValidationResult(new List<string>());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_WithErrors_IsNotValid()
    {
        // Arrange & Act
        var result = new ValidationResult(new List<string> { "Error 1", "Error 2" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void ValidationResult_ErrorsAreReadOnly()
    {
        // Arrange
        var errors = new List<string> { "Error" };
        var result = new ValidationResult(errors);

        // Assert
        result.Errors.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public void ValidationResult_EmptyEnumerable_IsValid()
    {
        // Arrange & Act
        var result = new ValidationResult(Enumerable.Empty<string>());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Static Factory Method Tests

    [Fact]
    public void ValidationResult_Success_ReturnsValidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Success();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_FailureWithParamsArray_ReturnsInvalidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Failure("Error 1", "Error 2", "Error 3");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
        result.Errors.Should().Contain("Error 3");
    }

    [Fact]
    public void ValidationResult_FailureWithSingleError_ReturnsInvalidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Failure("Single error");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Be("Single error");
    }

    [Fact]
    public void ValidationResult_FailureWithEnumerable_ReturnsInvalidResult()
    {
        // Arrange
        var errors = new List<string> { "Error A", "Error B" };

        // Act
        var result = ValidationResult.Failure(errors);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error A");
        result.Errors.Should().Contain("Error B");
    }

    [Fact]
    public void ValidationResult_FailureWithEmptyArray_ReturnsValidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Failure();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_FailureWithEmptyEnumerable_ReturnsValidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Failure(Enumerable.Empty<string>());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region IsValid Computed Property Tests

    [Fact]
    public void ValidationResult_IsValid_TrueWhenNoErrors()
    {
        // Arrange
        var result = new ValidationResult();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidationResult_IsValid_FalseWhenHasErrors()
    {
        // Arrange
        var result = new ValidationResult(new[] { "An error" });

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidationResult_IsValid_ComputedFromErrorsCollection()
    {
        // Arrange - Create results with different error counts
        var emptyResult = ValidationResult.Success();
        var singleErrorResult = ValidationResult.Failure("Error");
        var multipleErrorsResult = ValidationResult.Failure("Error1", "Error2", "Error3");

        // Assert
        emptyResult.IsValid.Should().BeTrue();
        emptyResult.Errors.Should().BeEmpty();

        singleErrorResult.IsValid.Should().BeFalse();
        singleErrorResult.Errors.Should().HaveCount(1);

        multipleErrorsResult.IsValid.Should().BeFalse();
        multipleErrorsResult.Errors.Should().HaveCount(3);
    }

    #endregion

    #region Error Immutability Tests

    [Fact]
    public void ValidationResult_Errors_AreImmutableFromOriginalList()
    {
        // Arrange
        var originalErrors = new List<string> { "Error 1" };
        var result = new ValidationResult(originalErrors);

        // Act - Modify original list
        originalErrors.Add("Error 2");

        // Assert - Result should not be affected
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().NotContain("Error 2");
    }

    #endregion
}
