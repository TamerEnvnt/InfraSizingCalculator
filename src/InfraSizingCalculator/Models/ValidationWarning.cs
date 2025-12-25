namespace InfraSizingCalculator.Models;

/// <summary>
/// Represents a validation warning or recommendation.
/// </summary>
public class ValidationWarning
{
    /// <summary>
    /// Unique identifier for this warning type.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Warning severity level.
    /// </summary>
    public WarningSeverity Severity { get; set; } = WarningSeverity.Info;

    /// <summary>
    /// Warning category for grouping.
    /// </summary>
    public WarningCategory Category { get; set; } = WarningCategory.General;

    /// <summary>
    /// Short title for the warning.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed message explaining the warning.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Suggested action to resolve the warning.
    /// </summary>
    public string? Suggestion { get; set; }

    /// <summary>
    /// Icon to display (emoji or CSS class).
    /// </summary>
    public string Icon { get; set; } = "⚠️";

    /// <summary>
    /// Whether this warning can be dismissed by the user.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// Create an info-level recommendation.
    /// </summary>
    public static ValidationWarning Info(string id, string title, string message, string? suggestion = null) => new()
    {
        Id = id,
        Severity = WarningSeverity.Info,
        Title = title,
        Message = message,
        Suggestion = suggestion,
        Icon = "💡"
    };

    /// <summary>
    /// Create a warning-level alert.
    /// </summary>
    public static ValidationWarning Warning(string id, string title, string message, string? suggestion = null) => new()
    {
        Id = id,
        Severity = WarningSeverity.Warning,
        Title = title,
        Message = message,
        Suggestion = suggestion,
        Icon = "⚠️"
    };

    /// <summary>
    /// Create a success/optimization message.
    /// </summary>
    public static ValidationWarning Success(string id, string title, string message) => new()
    {
        Id = id,
        Severity = WarningSeverity.Success,
        Title = title,
        Message = message,
        Icon = "✅",
        Dismissible = true
    };
}

/// <summary>
/// Warning severity levels.
/// </summary>
public enum WarningSeverity
{
    /// <summary>Informational recommendation.</summary>
    Info,

    /// <summary>Potential issue or anti-pattern.</summary>
    Warning,

    /// <summary>Critical issue that should be addressed.</summary>
    Critical,

    /// <summary>Positive feedback / optimization success.</summary>
    Success
}

/// <summary>
/// Warning categories for grouping.
/// </summary>
public enum WarningCategory
{
    /// <summary>General recommendations.</summary>
    General,

    /// <summary>Resource sizing concerns.</summary>
    Sizing,

    /// <summary>Cost optimization suggestions.</summary>
    Cost,

    /// <summary>High availability concerns.</summary>
    HighAvailability,

    /// <summary>Distribution-specific recommendations.</summary>
    Distribution,

    /// <summary>Environment configuration.</summary>
    Environment,

    /// <summary>Best practices.</summary>
    BestPractice
}
