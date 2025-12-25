using Microsoft.AspNetCore.Mvc;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class TechnologiesController : ControllerBase
{
    private readonly ITechnologyService _technologyService;

    public TechnologiesController(ITechnologyService technologyService)
    {
        _technologyService = technologyService;
    }

    /// <summary>
    /// Get all available technologies
    /// </summary>
    /// <returns>List of all technology configurations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TechnologyInfo>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<TechnologyInfo>> GetAll()
    {
        var technologies = Enum.GetValues<Technology>()
            .Select(t =>
            {
                var config = _technologyService.GetConfig(t);
                return new TechnologyInfo
                {
                    Technology = t,
                    Name = GetTechnologyName(t),
                    Icon = GetTechnologyIcon(t),
                    PlatformType = GetPlatformType(t),
                    TierSpecs = config.Tiers
                };
            })
            .ToList();

        return Ok(technologies);
    }

    /// <summary>
    /// Get a specific technology configuration
    /// </summary>
    /// <param name="technology">The technology to get</param>
    /// <returns>Technology configuration</returns>
    [HttpGet("{technology}")]
    [ProducesResponseType(typeof(TechnologyInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TechnologyInfo> Get(Technology technology)
    {
        try
        {
            var config = _technologyService.GetConfig(technology);
            return Ok(new TechnologyInfo
            {
                Technology = technology,
                Name = GetTechnologyName(technology),
                Icon = GetTechnologyIcon(technology),
                PlatformType = GetPlatformType(technology),
                TierSpecs = config.Tiers
            });
        }
        catch (ArgumentException)
        {
            return NotFound($"Technology '{technology}' not found");
        }
    }

    private static string GetTechnologyName(Technology technology) => technology switch
    {
        Technology.DotNet => ".NET",
        Technology.Java => "Java",
        Technology.NodeJs => "Node.js",
        Technology.Go => "Go",
        Technology.Mendix => "Mendix",
        _ => technology.ToString()
    };

    private static string GetTechnologyIcon(Technology technology) => technology switch
    {
        Technology.DotNet => "🔷",
        Technology.Java => "☕",
        Technology.NodeJs => "🟢",
        Technology.Go => "🔵",
        Technology.Mendix => "🟠",
        _ => "⚙️"
    };

    private static PlatformType GetPlatformType(Technology technology) => technology switch
    {
        Technology.Mendix => PlatformType.LowCode,
        _ => PlatformType.Native
    };
}

/// <summary>
/// Technology information DTO
/// </summary>
public class TechnologyInfo
{
    public Technology Technology { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public PlatformType PlatformType { get; set; }
    public Dictionary<AppTier, TierSpecs> TierSpecs { get; set; } = new();
}
