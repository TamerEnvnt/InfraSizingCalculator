using System.Text.RegularExpressions;

namespace InfraSizingCalculator.Services.Validation;

/// <summary>
/// Centralized input validation service.
/// Validates and sanitizes all user inputs before processing.
/// </summary>
public partial class InputValidationService : IInputValidationService
{
    private readonly ILogger<InputValidationService> _logger;

    // Validation limits based on business rules
    private const int MaxSmallApps = 1000;
    private const int MaxMediumApps = 500;
    private const int MaxLargeApps = 100;
    private const int MaxCpuCores = 256;
    private const int MaxMemoryGb = 1024;
    private const int MaxStorageGb = 10000;
    private const int MaxScenarioNameLength = 100;
    private const decimal MaxGrowthRatePercent = 500;

    public InputValidationService(ILogger<InputValidationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ValidationResult ValidateAppCounts(int small, int medium, int large)
    {
        var errors = new List<string>();

        if (small < 0)
            errors.Add("Small app count cannot be negative (BR-V001)");
        else if (small > MaxSmallApps)
            errors.Add($"Small app count cannot exceed {MaxSmallApps} (BR-V001)");

        if (medium < 0)
            errors.Add("Medium app count cannot be negative (BR-V001)");
        else if (medium > MaxMediumApps)
            errors.Add($"Medium app count cannot exceed {MaxMediumApps} (BR-V001)");

        if (large < 0)
            errors.Add("Large app count cannot be negative (BR-V001)");
        else if (large > MaxLargeApps)
            errors.Add($"Large app count cannot exceed {MaxLargeApps} (BR-V001)");

        if (small + medium + large == 0)
            errors.Add("At least one application must be specified (BR-V002)");

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "App count validation failed: Small={Small}, Medium={Medium}, Large={Large}, Errors={Errors}",
                small, medium, large, string.Join("; ", errors));
        }

        return new ValidationResult(errors);
    }

    /// <inheritdoc />
    public ValidationResult ValidateNodeSpecs(int cpuCores, int memoryGb, int storageGb)
    {
        var errors = new List<string>();

        if (cpuCores < 1)
            errors.Add("CPU cores must be at least 1 (BR-V003)");
        else if (cpuCores > MaxCpuCores)
            errors.Add($"CPU cores cannot exceed {MaxCpuCores} (BR-V003)");

        if (memoryGb < 1)
            errors.Add("Memory must be at least 1 GB (BR-V003)");
        else if (memoryGb > MaxMemoryGb)
            errors.Add($"Memory cannot exceed {MaxMemoryGb} GB (BR-V003)");

        if (storageGb < 1)
            errors.Add("Storage must be at least 1 GB (BR-V003)");
        else if (storageGb > MaxStorageGb)
            errors.Add($"Storage cannot exceed {MaxStorageGb} GB (BR-V003)");

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Node specs validation failed: CPU={Cpu}, Memory={Mem}GB, Storage={Storage}GB, Errors={Errors}",
                cpuCores, memoryGb, storageGb, string.Join("; ", errors));
        }

        return new ValidationResult(errors);
    }

    /// <inheritdoc />
    public ValidationResult ValidatePricing(decimal hourlyRate, decimal monthlyRate)
    {
        var errors = new List<string>();

        if (hourlyRate < 0)
            errors.Add("Hourly rate cannot be negative (BR-V004)");
        else if (hourlyRate > 10000)
            errors.Add("Hourly rate exceeds reasonable maximum (BR-V004)");

        if (monthlyRate < 0)
            errors.Add("Monthly rate cannot be negative (BR-V004)");
        else if (monthlyRate > 1000000)
            errors.Add("Monthly rate exceeds reasonable maximum (BR-V004)");

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Pricing validation failed: Hourly={Hourly}, Monthly={Monthly}, Errors={Errors}",
                hourlyRate, monthlyRate, string.Join("; ", errors));
        }

        return new ValidationResult(errors);
    }

    /// <inheritdoc />
    public string SanitizeScenarioName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Untitled Scenario";

        // Remove potentially dangerous characters (XSS prevention)
        var sanitized = DangerousCharsRegex().Replace(name, string.Empty);

        // Normalize whitespace
        sanitized = WhitespaceRegex().Replace(sanitized, " ").Trim();

        // Limit length per BR-V008
        if (sanitized.Length > MaxScenarioNameLength)
            sanitized = sanitized[..MaxScenarioNameLength].TrimEnd();

        // Ensure we have something after sanitization
        return string.IsNullOrWhiteSpace(sanitized) ? "Untitled Scenario" : sanitized;
    }

    /// <inheritdoc />
    public string SanitizeText(string input, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML/script tags
        var sanitized = HtmlTagsRegex().Replace(input, string.Empty);

        // Remove potentially dangerous characters
        sanitized = DangerousCharsRegex().Replace(sanitized, string.Empty);

        // Normalize whitespace
        sanitized = WhitespaceRegex().Replace(sanitized, " ").Trim();

        // Limit length
        if (sanitized.Length > maxLength)
            sanitized = sanitized[..maxLength].TrimEnd();

        return sanitized;
    }

    /// <inheritdoc />
    public ValidationResult ValidateGrowthRate(decimal percentage)
    {
        var errors = new List<string>();

        if (percentage < 0)
            errors.Add("Growth rate cannot be negative (BR-V005)");
        else if (percentage > MaxGrowthRatePercent)
            errors.Add($"Growth rate cannot exceed {MaxGrowthRatePercent}% (BR-V005)");

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Growth rate validation failed: Rate={Rate}%, Errors={Errors}",
                percentage, string.Join("; ", errors));
        }

        return new ValidationResult(errors);
    }

    // Compiled regex patterns for performance
    [GeneratedRegex(@"[<>""'&;\\]")]
    private static partial Regex DangerousCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"<[^>]*>")]
    private static partial Regex HtmlTagsRegex();
}
