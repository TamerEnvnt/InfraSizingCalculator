using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InfraSizingCalculator.Services.Auth;

namespace InfraSizingCalculator.Controllers;

/// <summary>
/// API controller for external authentication (OAuth) flows.
/// OAuth requires HTTP-level handling for the challenge/callback dance,
/// which cannot be done purely in Blazor Server.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IExternalAuthenticationService _externalAuthService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IExternalAuthenticationService externalAuthService,
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _externalAuthService = externalAuthService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Handles local (email/password) login via HTTP POST
    /// This is required because cookie authentication cannot be done over SignalR
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return Redirect("/login?error=" + Uri.EscapeDataString("Email and password are required."));
        }

        var result = await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            var returnUrl = request.ReturnUrl;
            if (string.IsNullOrEmpty(returnUrl) || !IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            return Redirect(returnUrl);
        }

        var errorMessage = result.ErrorMessage ?? "Invalid email or password.";
        return Redirect("/login?error=" + Uri.EscapeDataString(errorMessage));
    }

    /// <summary>
    /// Initiates external login by redirecting to the OAuth provider
    /// </summary>
    [HttpGet("external-login")]
    [AllowAnonymous]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl)
    {
        if (string.IsNullOrEmpty(provider))
        {
            return BadRequest("Provider is required");
        }

        // Validate the return URL to prevent open redirect attacks
        if (string.IsNullOrEmpty(returnUrl) || !IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        // Configure the challenge with the callback URL
        var callbackUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = _externalAuthService.ConfigureExternalLogin(provider, callbackUrl!);

        _logger.LogInformation("Initiating external login with provider: {Provider}", provider);

        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from the OAuth provider after successful authentication
    /// </summary>
    [HttpGet("external-login-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback([FromQuery] string? returnUrl, [FromQuery] string? remoteError)
    {
        returnUrl ??= "/";

        if (!string.IsNullOrEmpty(remoteError))
        {
            _logger.LogWarning("External login failed with error: {Error}", remoteError);
            return Redirect($"/login?error={Uri.EscapeDataString(remoteError)}");
        }

        var result = await _externalAuthService.ProcessExternalLoginAsync();

        if (result.Succeeded)
        {
            _logger.LogInformation("External login successful for {Email}", result.Email);

            // Validate return URL before redirecting
            if (!IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            return Redirect(returnUrl);
        }

        if (result.RequiresEmailConfirmation)
        {
            return Redirect($"/login?message={Uri.EscapeDataString("Please confirm your email before logging in.")}");
        }

        if (result.RequiresAdditionalInfo)
        {
            // Store the external login info in session and redirect to complete registration
            // This handles the case where the OAuth provider doesn't provide an email
            return Redirect($"/register?externalProvider={Uri.EscapeDataString(result.ProviderDisplayName ?? "External")}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var errorMessage = result.ErrorMessage ?? "Unable to sign in with external provider.";
        _logger.LogWarning("External login failed: {Error}", errorMessage);

        return Redirect($"/login?error={Uri.EscapeDataString(errorMessage)}");
    }

    /// <summary>
    /// Signs out the current user from both local and external authentication
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl)
    {
        await _authService.LogoutAsync();

        // Also sign out of external cookie
        await HttpContext.SignOutAsync();

        _logger.LogInformation("User logged out");

        return Redirect(returnUrl ?? "/");
    }

    /// <summary>
    /// Validates that a URL is local to prevent open redirect attacks
    /// </summary>
    private bool IsLocalUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Reject absolute URLs to external sites
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // Only allow same-origin URLs
            var request = HttpContext.Request;
            return uri.Host.Equals(request.Host.Host, StringComparison.OrdinalIgnoreCase);
        }

        // Relative URLs starting with / are OK (but not //)
        return url.StartsWith('/') && !url.StartsWith("//");
    }
}

/// <summary>
/// Request model for local login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
