namespace InfraSizingCalculator.Services.Validation;

/// <summary>
/// Interface for centralized input validation.
/// </summary>
public interface IInputValidationService
{
    /// <summary>
    /// Validates application count inputs.
    /// </summary>
    ValidationResult ValidateAppCounts(int small, int medium, int large);

    /// <summary>
    /// Validates node specification inputs.
    /// </summary>
    ValidationResult ValidateNodeSpecs(int cpuCores, int memoryGb, int storageGb);

    /// <summary>
    /// Validates pricing inputs.
    /// </summary>
    ValidationResult ValidatePricing(decimal hourlyRate, decimal monthlyRate);

    /// <summary>
    /// Sanitizes scenario name to prevent XSS and limit length.
    /// </summary>
    string SanitizeScenarioName(string name);

    /// <summary>
    /// Sanitizes general text input.
    /// </summary>
    string SanitizeText(string input, int maxLength = 500);

    /// <summary>
    /// Validates growth rate percentage.
    /// </summary>
    ValidationResult ValidateGrowthRate(decimal percentage);
}

/// <summary>
/// Result of a validation operation.
/// </summary>
public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public IReadOnlyList<string> Errors { get; }

    public ValidationResult()
    {
        Errors = Array.Empty<string>();
    }

    public ValidationResult(IEnumerable<string> errors)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public static ValidationResult Success() => new();
    public static ValidationResult Failure(params string[] errors) => new(errors);
    public static ValidationResult Failure(IEnumerable<string> errors) => new(errors);
}
