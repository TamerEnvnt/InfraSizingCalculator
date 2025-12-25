using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VMController : ControllerBase
{
    private readonly IVMSizingService _sizingService;
    private readonly ILogger<VMController> _logger;

    public VMController(IVMSizingService sizingService, ILogger<VMController> logger)
    {
        _sizingService = sizingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate VM sizing based on input parameters
    /// </summary>
    /// <param name="input">The VM sizing input parameters</param>
    /// <returns>VM sizing results with environment breakdown and grand totals</returns>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(VMSizingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<VMSizingResult> Calculate([FromBody] VMSizingInput? input)
    {
        // Null check
        if (input == null)
        {
            var errorResponse = ApiErrorResponse.BadRequest(
                "Request body is required",
                new List<ValidationErrorDetail>
                {
                    new("input", "Input is required and cannot be null")
                });
            return BadRequest(errorResponse);
        }

        // Manual validation of the input model
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(input);

        if (!Validator.TryValidateObject(input, validationContext, validationResults, validateAllProperties: true))
        {
            var details = validationResults
                .SelectMany(r => r.MemberNames.Select(m => new ValidationErrorDetail(m, r.ErrorMessage ?? "Validation failed")))
                .ToList();

            var errorResponse = ApiErrorResponse.ValidationError(
                "One or more validation errors occurred",
                details);

            return BadRequest(errorResponse);
        }

        // Additional IValidatableObject validation
        if (input is IValidatableObject validatable)
        {
            var customValidationResults = validatable.Validate(validationContext).ToList();
            if (customValidationResults.Any())
            {
                var details = customValidationResults
                    .SelectMany(r => r.MemberNames.Select(m => new ValidationErrorDetail(m, r.ErrorMessage ?? "Validation failed")))
                    .ToList();

                var errorResponse = ApiErrorResponse.ValidationError(
                    "One or more business rule validation errors occurred",
                    details);

                return BadRequest(errorResponse);
            }
        }

        _logger.LogInformation(
            "Calculating VM sizing for {Technology} with {EnvironmentCount} environments",
            input.Technology,
            input.EnabledEnvironments.Count);

        var result = _sizingService.Calculate(input);

        _logger.LogInformation(
            "VM calculation complete: {TotalVMs} VMs, {TotalCpu} vCPU, {TotalRam} GB RAM",
            result.GrandTotal.TotalVMs,
            result.GrandTotal.TotalCpu,
            result.GrandTotal.TotalRam);

        return Ok(result);
    }

    /// <summary>
    /// Validate VM sizing input without performing the calculation
    /// </summary>
    /// <param name="input">The VM sizing input parameters to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult Validate([FromBody] VMSizingInput? input)
    {
        if (input == null)
        {
            var errorResponse = ApiErrorResponse.BadRequest(
                "Request body is required",
                new List<ValidationErrorDetail>
                {
                    new("input", "Input is required and cannot be null")
                });
            return BadRequest(errorResponse);
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(input);

        // Standard DataAnnotations validation
        Validator.TryValidateObject(input, validationContext, validationResults, validateAllProperties: true);

        // IValidatableObject validation
        if (input is IValidatableObject validatable)
        {
            validationResults.AddRange(validatable.Validate(validationContext));
        }

        if (validationResults.Any())
        {
            var details = validationResults
                .SelectMany(r => r.MemberNames.Select(m => new ValidationErrorDetail(m, r.ErrorMessage ?? "Validation failed")))
                .ToList();

            var errorResponse = ApiErrorResponse.ValidationError(
                "One or more validation errors occurred",
                details);

            return BadRequest(errorResponse);
        }

        return Ok(new { valid = true, message = "Input is valid" });
    }

    /// <summary>
    /// Get default specs for a given role and size tier
    /// </summary>
    [HttpGet("specs/{role}/{size}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult GetRoleSpecs(
        [FromRoute] Models.Enums.ServerRole role,
        [FromRoute] Models.Enums.AppTier size,
        [FromQuery] Models.Enums.Technology technology = Models.Enums.Technology.DotNet)
    {
        var (cpu, ram) = _sizingService.GetRoleSpecs(role, size, technology);
        return Ok(new { role, size, technology, cpu, ram });
    }

    /// <summary>
    /// Get HA multiplier for a given HA pattern
    /// </summary>
    [HttpGet("ha-multiplier/{pattern}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHAMultiplier([FromRoute] Models.Enums.HAPattern pattern)
    {
        var multiplier = _sizingService.GetHAMultiplier(pattern);
        return Ok(new { pattern, multiplier });
    }

    /// <summary>
    /// Get load balancer specs for a given option
    /// </summary>
    [HttpGet("lb-specs/{option}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetLoadBalancerSpecs([FromRoute] Models.Enums.LoadBalancerOption option)
    {
        var (vms, cpuPerVm, ramPerVm) = _sizingService.GetLoadBalancerSpecs(option);
        return Ok(new { option, vms, cpuPerVm, ramPerVm });
    }
}
