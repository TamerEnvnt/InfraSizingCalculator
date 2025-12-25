using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns consistent, secure error responses
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId,
            context.Request.Path);

        var response = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx),
            ArgumentException argEx => HandleArgumentException(argEx),
            KeyNotFoundException => ApiErrorResponse.NotFound("The requested resource was not found"),
            UnauthorizedAccessException => ApiErrorResponse.Unauthorized(),
            NotSupportedException notSupportedEx => ApiErrorResponse.BadRequest(notSupportedEx.Message),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
            _ => HandleGenericException(traceId)
        };

        response.Path = context.Request.Path;
        response.TraceId = traceId;

        context.Response.StatusCode = response.Status;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private ApiErrorResponse HandleValidationException(ValidationException ex)
    {
        var details = new List<ValidationErrorDetail>();

        if (ex.ValidationResult != null)
        {
            foreach (var memberName in ex.ValidationResult.MemberNames)
            {
                details.Add(new ValidationErrorDetail(
                    memberName,
                    ex.ValidationResult.ErrorMessage ?? "Validation failed"));
            }
        }

        if (details.Count == 0)
        {
            details.Add(new ValidationErrorDetail("", ex.Message));
        }

        return ApiErrorResponse.ValidationError("One or more validation errors occurred", details);
    }

    private ApiErrorResponse HandleArgumentException(ArgumentException ex)
    {
        var details = new List<ValidationErrorDetail>
        {
            new ValidationErrorDetail(
                ex.ParamName ?? "",
                SanitizeErrorMessage(ex.Message))
        };

        return ApiErrorResponse.ValidationError("Invalid argument provided", details);
    }

    private ApiErrorResponse HandleInvalidOperationException(InvalidOperationException ex)
    {
        // For known business rule violations, return the message
        // Otherwise, treat as internal error
        if (ex.Message.Contains("BR-") || ex.Message.Contains("must be") || ex.Message.Contains("cannot be"))
        {
            return ApiErrorResponse.BadRequest(SanitizeErrorMessage(ex.Message));
        }

        return ApiErrorResponse.InternalServerError();
    }

    private ApiErrorResponse HandleGenericException(string traceId)
    {
        // Never expose internal exception details to clients
        return ApiErrorResponse.InternalServerError(traceId);
    }

    /// <summary>
    /// Sanitize error messages to prevent information disclosure
    /// </summary>
    private string SanitizeErrorMessage(string message)
    {
        // Remove potentially sensitive information
        var sanitized = message;

        // Remove file paths
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"[A-Za-z]:\\[^\s]+|/[^\s]+\.cs|/[^\s]+\.dll",
            "[path]");

        // Remove stack trace references
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"at\s+\S+\.\S+\(",
            "");

        // Remove connection strings
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"(Server|Data Source|Initial Catalog|User Id|Password|Integrated Security)=[^;]+",
            "$1=[hidden]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return sanitized.Trim();
    }
}

/// <summary>
/// Extension methods for registering the global exception handler
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}
