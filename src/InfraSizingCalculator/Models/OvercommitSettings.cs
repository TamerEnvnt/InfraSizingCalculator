using System.ComponentModel.DataAnnotations;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Overcommit ratios for CPU and Memory
/// BR-V006: CPU overcommit ratio must be between 1 and 10
/// BR-V007: Memory overcommit ratio must be between 1 and 4
/// </summary>
public class OvercommitSettings : IValidatableObject
{
    [Range(1.0, 10.0, ErrorMessage = "CPU overcommit ratio must be between 1 and 10 (BR-V006)")]
    public double Cpu { get; set; } = 1.0;

    [Range(1.0, 4.0, ErrorMessage = "Memory overcommit ratio must be between 1 and 4 (BR-V007)")]
    public double Memory { get; set; } = 1.0;

    public OvercommitSettings Clone() => new()
    {
        Cpu = Cpu,
        Memory = Memory
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Cpu < 1.0 || Cpu > 10.0)
        {
            yield return new ValidationResult(
                "CPU overcommit ratio must be between 1 and 10 (BR-V006)",
                new[] { nameof(Cpu) });
        }

        if (Memory < 1.0 || Memory > 4.0)
        {
            yield return new ValidationResult(
                "Memory overcommit ratio must be between 1 and 4 (BR-V007)",
                new[] { nameof(Memory) });
        }
    }
}
