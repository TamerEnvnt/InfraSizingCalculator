using System.ComponentModel.DataAnnotations;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Per-environment headroom percentages
/// BR-H003: Default headroom for Development = 33%
/// BR-H004: Default headroom for Test = 33%
/// BR-H005: Default headroom for Staging = 0%
/// BR-H006: Default headroom for Production = 37.5%
/// BR-H007: Default headroom for DR = 37.5%
/// BR-H008: Headroom percentage must be between 0 and 100
/// </summary>
public class HeadroomSettings : IValidatableObject
{
    [Range(0.0, 100.0, ErrorMessage = "Dev headroom must be between 0 and 100% (BR-H008)")]
    public double Dev { get; set; } = 33.0;

    [Range(0.0, 100.0, ErrorMessage = "Test headroom must be between 0 and 100% (BR-H008)")]
    public double Test { get; set; } = 33.0;

    [Range(0.0, 100.0, ErrorMessage = "Stage headroom must be between 0 and 100% (BR-H008)")]
    public double Stage { get; set; } = 0.0;

    [Range(0.0, 100.0, ErrorMessage = "Prod headroom must be between 0 and 100% (BR-H008)")]
    public double Prod { get; set; } = 37.5;

    [Range(0.0, 100.0, ErrorMessage = "DR headroom must be between 0 and 100% (BR-H008)")]
    public double DR { get; set; } = 37.5;

    public HeadroomSettings Clone() => new()
    {
        Dev = Dev,
        Test = Test,
        Stage = Stage,
        Prod = Prod,
        DR = DR
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var environments = new (string Name, double Value)[]
        {
            (nameof(Dev), Dev),
            (nameof(Test), Test),
            (nameof(Stage), Stage),
            (nameof(Prod), Prod),
            (nameof(DR), DR)
        };

        foreach (var (name, value) in environments)
        {
            if (value < 0.0 || value > 100.0)
            {
                yield return new ValidationResult(
                    $"{name} headroom must be between 0 and 100% (BR-H008)",
                    new[] { name });
            }
        }
    }
}
