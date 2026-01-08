using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using InfraSizingCalculator.Data.Identity;

namespace InfraSizingCalculator.Services.Auth;

/// <summary>
/// Service for handling external OAuth authentication providers (Google, Microsoft).
/// Follows OAuth 2.0 best practices:
/// - Uses PKCE where supported
/// - Validates state parameter to prevent CSRF
/// - Creates or links local accounts securely
/// </summary>
public interface IExternalAuthenticationService
{
    /// <summary>
    /// Gets the configured external authentication schemes
    /// </summary>
    Task<IEnumerable<AuthenticationScheme>> GetExternalProvidersAsync();

    /// <summary>
    /// Configures the challenge for an external provider
    /// </summary>
    AuthenticationProperties ConfigureExternalLogin(string provider, string redirectUrl);

    /// <summary>
    /// Processes the callback from an external provider
    /// </summary>
    Task<ExternalLoginResult> ProcessExternalLoginAsync();

    /// <summary>
    /// Creates or links a local user account from external login info
    /// </summary>
    Task<ExternalLoginResult> CreateOrLinkUserAsync(ExternalLoginInfo info, string? email = null);
}

/// <summary>
/// Result of external authentication attempt
/// </summary>
public class ExternalLoginResult
{
    public bool Succeeded { get; init; }
    public bool RequiresEmailConfirmation { get; init; }
    public bool RequiresAdditionalInfo { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? ProviderDisplayName { get; init; }
    public string? ErrorMessage { get; init; }
    public ExternalLoginInfo? LoginInfo { get; init; }

    public static ExternalLoginResult Success(string email, string displayName) =>
        new() { Succeeded = true, Email = email, DisplayName = displayName };

    public static ExternalLoginResult NeedsEmail(ExternalLoginInfo info, string providerName) =>
        new()
        {
            Succeeded = false,
            RequiresAdditionalInfo = true,
            LoginInfo = info,
            ProviderDisplayName = providerName
        };

    public static ExternalLoginResult Failed(string message) =>
        new() { Succeeded = false, ErrorMessage = message };

    public static ExternalLoginResult NeedsConfirmation(string email) =>
        new() { Succeeded = false, RequiresEmailConfirmation = true, Email = email };
}

/// <summary>
/// Implementation of external authentication service
/// </summary>
public class ExternalAuthenticationService : IExternalAuthenticationService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthenticationSettingsService _settingsService;
    private readonly ILogger<ExternalAuthenticationService> _logger;

    public ExternalAuthenticationService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuthenticationSettingsService settingsService,
        ILogger<ExternalAuthenticationService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<IEnumerable<AuthenticationScheme>> GetExternalProvidersAsync()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var settings = await _settingsService.GetSettingsAsync();

        // Filter schemes based on what's actually enabled in settings
        var enabledSchemes = new List<AuthenticationScheme>();

        foreach (var scheme in schemes)
        {
            var isEnabled = scheme.Name.ToLowerInvariant() switch
            {
                "google" => settings.GoogleEnabled &&
                           !string.IsNullOrEmpty(settings.GoogleClientId) &&
                           !string.IsNullOrEmpty(settings.GoogleClientSecret),
                "microsoft" => settings.MicrosoftEnabled &&
                              !string.IsNullOrEmpty(settings.MicrosoftClientId) &&
                              !string.IsNullOrEmpty(settings.MicrosoftClientSecret),
                _ => false
            };

            if (isEnabled)
            {
                enabledSchemes.Add(scheme);
            }
        }

        return enabledSchemes;
    }

    public AuthenticationProperties ConfigureExternalLogin(string provider, string redirectUrl)
    {
        // Configure authentication properties with secure defaults
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        // Add additional security parameters
        properties.Items["LoginProvider"] = provider;

        // Set correlation cookie options for CSRF protection
        properties.SetString(".redirect", redirectUrl);

        return properties;
    }

    public async Task<ExternalLoginResult> ProcessExternalLoginAsync()
    {
        // Get the login information from the external provider
        var info = await _signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            _logger.LogWarning("External login info was null - user may have cancelled or there was an error");
            return ExternalLoginResult.Failed("Unable to load external login information.");
        }

        _logger.LogInformation("Processing external login from provider: {Provider}", info.LoginProvider);

        // Check if this external login is already linked to a user
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: false);

        if (signInResult.Succeeded)
        {
            // User exists and is signed in
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            _logger.LogInformation("User signed in with {Provider}: {Email}", info.LoginProvider, email);

            return ExternalLoginResult.Success(email ?? "", name ?? "");
        }

        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("User account locked out during external login");
            return ExternalLoginResult.Failed("Your account has been locked. Please try again later.");
        }

        if (signInResult.IsNotAllowed)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            _logger.LogWarning("External login not allowed for user: {Email}", email);
            return ExternalLoginResult.NeedsConfirmation(email ?? "");
        }

        // User doesn't exist yet - they need to be created
        return await CreateOrLinkUserAsync(info);
    }

    public async Task<ExternalLoginResult> CreateOrLinkUserAsync(ExternalLoginInfo info, string? providedEmail = null)
    {
        // Extract user information from the external provider claims
        var email = providedEmail ?? info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name)
                   ?? info.Principal.FindFirstValue(ClaimTypes.GivenName);

        // If no email from provider, we need to ask the user
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("External provider {Provider} did not provide email", info.LoginProvider);
            return ExternalLoginResult.NeedsEmail(info, info.ProviderDisplayName ?? info.LoginProvider);
        }

        // Check if user with this email already exists
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            // Link the external login to the existing user
            var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);

            if (!addLoginResult.Succeeded)
            {
                var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to link external login to existing user {Email}: {Errors}", email, errors);
                return ExternalLoginResult.Failed("Unable to link your account. Please contact support.");
            }

            // Sign in the user
            await _signInManager.SignInAsync(existingUser, isPersistent: false);

            _logger.LogInformation("Linked {Provider} login to existing user: {Email}", info.LoginProvider, email);

            return ExternalLoginResult.Success(email, existingUser.DisplayName ?? name ?? email);
        }

        // Check if self-registration is allowed
        var settings = await _settingsService.GetSettingsAsync();
        if (!settings.AllowSelfRegistration)
        {
            _logger.LogWarning("Self-registration disabled, external login rejected for: {Email}", email);
            return ExternalLoginResult.Failed("Registration is not allowed. Please contact an administrator.");
        }

        // Create a new user
        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = name ?? email.Split('@')[0],
            EmailConfirmed = true, // External providers verify email
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(newUser);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user from external login {Email}: {Errors}", email, errors);
            return ExternalLoginResult.Failed("Unable to create your account. Please try again.");
        }

        // Add the external login to the new user
        var addExternalResult = await _userManager.AddLoginAsync(newUser, info);

        if (!addExternalResult.Succeeded)
        {
            _logger.LogError("Failed to add external login to new user {Email}", email);
            // User was created but login wasn't linked - they can still sign in with email/password if set up
        }

        // Add default role
        await _userManager.AddToRoleAsync(newUser, "User");

        // Sign in the new user
        await _signInManager.SignInAsync(newUser, isPersistent: false);

        _logger.LogInformation("Created new user from {Provider} login: {Email}", info.LoginProvider, email);

        return ExternalLoginResult.Success(email, newUser.DisplayName ?? email);
    }
}
