using System.Text.Json.Serialization;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Standardized API error response format
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// Error code for programmatic handling (e.g., "VALIDATION_ERROR", "NOT_FOUND")
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed validation errors or additional context
    /// </summary>
    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ValidationErrorDetail>? Details { get; set; }

    /// <summary>
    /// Request path that caused the error
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    /// <summary>
    /// Timestamp of when the error occurred
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique trace ID for debugging (optional)
    /// </summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    public static ApiErrorResponse BadRequest(string message, List<ValidationErrorDetail>? details = null)
    {
        return new ApiErrorResponse
        {
            Status = 400,
            Error = "BAD_REQUEST",
            Message = message,
            Details = details
        };
    }

    public static ApiErrorResponse ValidationError(string message, List<ValidationErrorDetail> details)
    {
        return new ApiErrorResponse
        {
            Status = 400,
            Error = "VALIDATION_ERROR",
            Message = message,
            Details = details
        };
    }

    public static ApiErrorResponse NotFound(string message)
    {
        return new ApiErrorResponse
        {
            Status = 404,
            Error = "NOT_FOUND",
            Message = message
        };
    }

    public static ApiErrorResponse InternalServerError(string? traceId = null)
    {
        return new ApiErrorResponse
        {
            Status = 500,
            Error = "INTERNAL_SERVER_ERROR",
            Message = "An unexpected error occurred. Please try again later.",
            TraceId = traceId
        };
    }

    public static ApiErrorResponse Conflict(string message)
    {
        return new ApiErrorResponse
        {
            Status = 409,
            Error = "CONFLICT",
            Message = message
        };
    }

    public static ApiErrorResponse Forbidden(string message = "Access denied")
    {
        return new ApiErrorResponse
        {
            Status = 403,
            Error = "FORBIDDEN",
            Message = message
        };
    }

    public static ApiErrorResponse Unauthorized(string message = "Authentication required")
    {
        return new ApiErrorResponse
        {
            Status = 401,
            Error = "UNAUTHORIZED",
            Message = message
        };
    }
}

/// <summary>
/// Individual validation error detail
/// </summary>
public class ValidationErrorDetail
{
    /// <summary>
    /// Field or property name that failed validation
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Validation error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The invalid value that was provided (optional, for debugging)
    /// </summary>
    [JsonPropertyName("rejectedValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? RejectedValue { get; set; }

    public ValidationErrorDetail() { }

    public ValidationErrorDetail(string field, string message, object? rejectedValue = null)
    {
        Field = field;
        Message = message;
        RejectedValue = rejectedValue;
    }
}
