using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

public interface IDistributionService
{
    DistributionConfig GetConfig(Distribution distribution);
    IEnumerable<DistributionConfig> GetAll();
    IEnumerable<DistributionConfig> GetByTag(string tag);
}
