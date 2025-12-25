using System.ComponentModel.DataAnnotations;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Per-environment replica counts
/// BR-R001: Default replica count for Production and DR = 3
/// BR-R002: Default replica count for Development and Test = 1
/// BR-R003: Default replica count for Staging = 2
/// BR-R004: Replica count must be between 1 and 10
/// </summary>
public class ReplicaSettings : IValidatableObject
{
    [Range(1, 10, ErrorMessage = "Prod replica count must be between 1 and 10 (BR-R004)")]
    public int Prod { get; set; } = 3;

    [Range(1, 10, ErrorMessage = "NonProd replica count must be between 1 and 10 (BR-R004)")]
    public int NonProd { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "Stage replica count must be between 1 and 10 (BR-R004)")]
    public int Stage { get; set; } = 2;

    public ReplicaSettings Clone() => new()
    {
        Prod = Prod,
        NonProd = NonProd,
        Stage = Stage
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Prod < 1 || Prod > 10)
        {
            yield return new ValidationResult(
                "Prod replica count must be between 1 and 10 (BR-R004)",
                new[] { nameof(Prod) });
        }

        if (NonProd < 1 || NonProd > 10)
        {
            yield return new ValidationResult(
                "NonProd replica count must be between 1 and 10 (BR-R004)",
                new[] { nameof(NonProd) });
        }

        if (Stage < 1 || Stage > 10)
        {
            yield return new ValidationResult(
                "Stage replica count must be between 1 and 10 (BR-R004)",
                new[] { nameof(Stage) });
        }
    }
}
