using System.ComponentModel.DataAnnotations;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Application counts by tier
/// BR-V001: Application count for each tier must be >= 0
/// BR-V002: Application counts must be whole numbers (integers)
/// </summary>
public class AppConfig : IValidatableObject
{
    [Range(0, int.MaxValue, ErrorMessage = "Small app count must be >= 0 (BR-V001)")]
    public int Small { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Medium app count must be >= 0 (BR-V001)")]
    public int Medium { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Large app count must be >= 0 (BR-V001)")]
    public int Large { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "XLarge app count must be >= 0 (BR-V001)")]
    public int XLarge { get; set; }

    public int TotalApps => Small + Medium + Large + XLarge;

    public AppConfig Clone() => new()
    {
        Small = Small,
        Medium = Medium,
        Large = Large,
        XLarge = XLarge
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Small < 0)
        {
            yield return new ValidationResult(
                "Small app count must be >= 0 (BR-V001)",
                new[] { nameof(Small) });
        }

        if (Medium < 0)
        {
            yield return new ValidationResult(
                "Medium app count must be >= 0 (BR-V001)",
                new[] { nameof(Medium) });
        }

        if (Large < 0)
        {
            yield return new ValidationResult(
                "Large app count must be >= 0 (BR-V001)",
                new[] { nameof(Large) });
        }

        if (XLarge < 0)
        {
            yield return new ValidationResult(
                "XLarge app count must be >= 0 (BR-V001)",
                new[] { nameof(XLarge) });
        }
    }
}
