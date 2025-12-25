using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class K8sController : ControllerBase
{
    private readonly IK8sSizingService _sizingService;
    private readonly ILogger<K8sController> _logger;

    public K8sController(IK8sSizingService sizingService, ILogger<K8sController> logger)
    {
        _sizingService = sizingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate Kubernetes cluster sizing based on input parameters
    /// </summary>
    /// <param name="input">The sizing input parameters</param>
    /// <returns>Sizing results with environment breakdown and grand totals</returns>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(K8sSizingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<K8sSizingResult> Calculate([FromBody] K8sSizingInput? input)
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
            "Calculating K8s sizing for {Distribution} with {TotalProdApps} prod apps and {TotalNonProdApps} non-prod apps",
            input.Distribution,
            input.ProdApps.TotalApps,
            input.NonProdApps.TotalApps);

        var result = _sizingService.Calculate(input);

        _logger.LogInformation(
            "Calculation complete: {TotalNodes} nodes, {TotalCpu} vCPU, {TotalRam} GB RAM",
            result.GrandTotal.TotalNodes,
            result.GrandTotal.TotalCpu,
            result.GrandTotal.TotalRam);

        return Ok(result);
    }

    /// <summary>
    /// Validate K8s sizing input without performing the calculation
    /// </summary>
    /// <param name="input">The sizing input parameters to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult Validate([FromBody] K8sSizingInput? input)
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
}
