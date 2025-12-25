using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

public interface ITechnologyService
{
    TechnologyConfig GetConfig(Technology technology);
    IEnumerable<TechnologyConfig> GetAll();
    IEnumerable<TechnologyConfig> GetByPlatformType(PlatformType platformType);
    TechnologyVMRoles? GetVMRoles(Technology technology);
}
