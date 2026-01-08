using InfraSizingCalculator.Data.Identity;

namespace InfraSizingCalculator.Services.Auth;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login with username and password
    /// </summary>
    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResult> RegisterAsync(string email, string password, string? displayName = null);

    /// <summary>
    /// Logout the current user
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    Task<ApplicationUser?> GetCurrentUserAsync();

    /// <summary>
    /// Check if a user with the given email exists
    /// </summary>
    Task<bool> UserExistsAsync(string email);

    /// <summary>
    /// Check if any admin user exists (for initial setup)
    /// </summary>
    Task<bool> AdminExistsAsync();

    /// <summary>
    /// Create the initial admin account
    /// </summary>
    Task<AuthResult> CreateInitialAdminAsync(string email, string password, string displayName);

    /// <summary>
    /// Login or create a user from an external authentication provider (OAuth, LDAP).
    /// Creates a local user account if one doesn't exist.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="displayName">The user's display name</param>
    /// <param name="providerName">The authentication provider name (e.g., "Google", "Microsoft", "LDAP")</param>
    /// <param name="providerKey">The unique key from the provider</param>
    Task<AuthResult> LoginOrCreateFromExternalAsync(string email, string displayName, string providerName, string providerKey);
}

/// <summary>
/// Result of an authentication operation
/// </summary>
public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public ApplicationUser? User { get; set; }

    public static AuthResult Success(ApplicationUser user) => new() { Succeeded = true, User = user };
    public static AuthResult Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
}
