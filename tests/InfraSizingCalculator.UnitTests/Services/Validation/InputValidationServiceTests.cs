using InfraSizingCalculator.Services.Validation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.Validation;

/// <summary>
/// Tests for InputValidationService.
/// Tests validation of app counts, node specs, pricing, growth rates, and text sanitization.
/// </summary>
public class InputValidationServiceTests
{
    private readonly InputValidationService _service;

    public InputValidationServiceTests()
    {
        var logger = Substitute.For<ILogger<InputValidationService>>();
        _service = new InputValidationService(logger);
    }

    #region ValidateAppCounts Tests

    [Fact]
    public void ValidateAppCounts_ValidValues_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(10, 5, 2);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAppCounts_NegativeSmall_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(-1, 5, 2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("negative") && e.Contains("BR-V001"));
    }

    [Fact]
    public void ValidateAppCounts_ExceedsMaxSmall_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(1001, 5, 2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("1000") && e.Contains("BR-V001"));
    }

    [Fact]
    public void ValidateAppCounts_NegativeMedium_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(10, -1, 2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Medium") && e.Contains("negative"));
    }

    [Fact]
    public void ValidateAppCounts_ExceedsMaxMedium_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(10, 501, 2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("500"));
    }

    [Fact]
    public void ValidateAppCounts_ExceedsMaxLarge_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(10, 5, 101);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("100"));
    }

    [Fact]
    public void ValidateAppCounts_AllZero_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(0, 0, 0);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("At least one") && e.Contains("BR-V002"));
    }

    [Fact]
    public void ValidateAppCounts_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange & Act
        var result = _service.ValidateAppCounts(-1, -1, -1);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
    }

    #endregion

    #region ValidateNodeSpecs Tests

    [Fact]
    public void ValidateNodeSpecs_ValidValues_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(4, 16, 100);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateNodeSpecs_ZeroCpu_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(0, 16, 100);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("CPU") && e.Contains("at least 1"));
    }

    [Fact]
    public void ValidateNodeSpecs_ExceedsMaxCpu_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(257, 16, 100);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("256"));
    }

    [Fact]
    public void ValidateNodeSpecs_ZeroMemory_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(4, 0, 100);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Memory") && e.Contains("at least 1"));
    }

    [Fact]
    public void ValidateNodeSpecs_ExceedsMaxMemory_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(4, 1025, 100);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("1024"));
    }

    [Fact]
    public void ValidateNodeSpecs_ZeroStorage_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(4, 16, 0);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Storage") && e.Contains("at least 1"));
    }

    [Fact]
    public void ValidateNodeSpecs_ExceedsMaxStorage_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateNodeSpecs(4, 16, 10001);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("10000"));
    }

    [Fact]
    public void ValidateNodeSpecs_BoundaryValues_ReturnsNoErrors()
    {
        // Arrange - Test maximum allowed values
        var result = _service.ValidateNodeSpecs(256, 1024, 10000);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidatePricing Tests

    [Fact]
    public void ValidatePricing_ValidValues_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = _service.ValidatePricing(0.10m, 100m);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidatePricing_NegativeHourly_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidatePricing(-1m, 100m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Hourly") && e.Contains("negative"));
    }

    [Fact]
    public void ValidatePricing_ExceedsMaxHourly_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidatePricing(10001m, 100m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Hourly") && e.Contains("maximum"));
    }

    [Fact]
    public void ValidatePricing_NegativeMonthly_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidatePricing(1m, -100m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Monthly") && e.Contains("negative"));
    }

    [Fact]
    public void ValidatePricing_ExceedsMaxMonthly_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidatePricing(1m, 1000001m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Monthly") && e.Contains("maximum"));
    }

    [Fact]
    public void ValidatePricing_ZeroValues_ReturnsNoErrors()
    {
        // Arrange - Zero pricing is valid (free tier scenarios)
        var result = _service.ValidatePricing(0m, 0m);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion

    #region SanitizeScenarioName Tests

    [Fact]
    public void SanitizeScenarioName_NullInput_ReturnsDefault()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName(null!);

        // Assert
        Assert.Equal("Untitled Scenario", result);
    }

    [Fact]
    public void SanitizeScenarioName_EmptyInput_ReturnsDefault()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName("");

        // Assert
        Assert.Equal("Untitled Scenario", result);
    }

    [Fact]
    public void SanitizeScenarioName_WhitespaceOnly_ReturnsDefault()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName("   ");

        // Assert
        Assert.Equal("Untitled Scenario", result);
    }

    [Fact]
    public void SanitizeScenarioName_ValidName_ReturnsTrimmed()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName("  My Scenario  ");

        // Assert
        Assert.Equal("My Scenario", result);
    }

    [Fact]
    public void SanitizeScenarioName_DangerousChars_RemovesThem()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName("<script>alert('xss')</script>");

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.DoesNotContain("'", result);
    }

    [Fact]
    public void SanitizeScenarioName_ExceedsMaxLength_Truncates()
    {
        // Arrange - Create a 150-char string (max is 100)
        var longName = new string('a', 150);

        // Act
        var result = _service.SanitizeScenarioName(longName);

        // Assert
        Assert.Equal(100, result.Length);
    }

    [Fact]
    public void SanitizeScenarioName_NormalizesWhitespace()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName("My    Scenario   Name");

        // Assert
        Assert.Equal("My Scenario Name", result);
    }

    [Fact]
    public void SanitizeScenarioName_OnlyDangerousChars_ReturnsDefault()
    {
        // Arrange & Act
        var result = _service.SanitizeScenarioName("<>\"'&;\\");

        // Assert
        Assert.Equal("Untitled Scenario", result);
    }

    #endregion

    #region SanitizeText Tests

    [Fact]
    public void SanitizeText_NullInput_ReturnsEmpty()
    {
        // Arrange & Act
        var result = _service.SanitizeText(null!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeText_EmptyInput_ReturnsEmpty()
    {
        // Arrange & Act
        var result = _service.SanitizeText("");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeText_ValidText_ReturnsSanitized()
    {
        // Arrange & Act
        var result = _service.SanitizeText("Hello World");

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void SanitizeText_HtmlTags_RemovesThem()
    {
        // Arrange & Act
        var result = _service.SanitizeText("<p>Hello</p><script>bad</script>");

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.Contains("Hello", result);
    }

    [Fact]
    public void SanitizeText_ExceedsMaxLength_Truncates()
    {
        // Arrange - Create a 600-char string (default max is 500)
        var longText = new string('a', 600);

        // Act
        var result = _service.SanitizeText(longText);

        // Assert
        Assert.Equal(500, result.Length);
    }

    [Fact]
    public void SanitizeText_CustomMaxLength_Truncates()
    {
        // Arrange
        var longText = new string('a', 200);

        // Act
        var result = _service.SanitizeText(longText, maxLength: 50);

        // Assert
        Assert.Equal(50, result.Length);
    }

    [Fact]
    public void SanitizeText_NormalizesWhitespace()
    {
        // Arrange & Act
        var result = _service.SanitizeText("Hello    World\t\nTest");

        // Assert
        Assert.Equal("Hello World Test", result);
    }

    #endregion

    #region ValidateGrowthRate Tests

    [Fact]
    public void ValidateGrowthRate_ValidRate_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = _service.ValidateGrowthRate(25m);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateGrowthRate_ZeroRate_ReturnsNoErrors()
    {
        // Arrange & Act
        var result = _service.ValidateGrowthRate(0m);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateGrowthRate_NegativeRate_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateGrowthRate(-10m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("negative") && e.Contains("BR-V005"));
    }

    [Fact]
    public void ValidateGrowthRate_ExceedsMax_ReturnsError()
    {
        // Arrange & Act
        var result = _service.ValidateGrowthRate(501m);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("500") && e.Contains("BR-V005"));
    }

    [Fact]
    public void ValidateGrowthRate_BoundaryValue_ReturnsNoErrors()
    {
        // Arrange - Test max allowed value (500%)
        var result = _service.ValidateGrowthRate(500m);

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion
}
