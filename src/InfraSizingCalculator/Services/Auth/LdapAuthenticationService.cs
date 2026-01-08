using System.Security.Claims;
using Novell.Directory.Ldap;
using InfraSizingCalculator.Data.Entities;

namespace InfraSizingCalculator.Services.Auth;

/// <summary>
/// LDAP/Active Directory authentication service.
/// Implements secure LDAP authentication following best practices:
/// - Validates credentials without storing them
/// - Supports LDAPS (SSL/TLS) for secure connections
/// - Uses parameterized search filters to prevent LDAP injection
/// - Properly disposes of connections
/// </summary>
public interface ILdapAuthenticationService
{
    /// <summary>
    /// Authenticates a user against LDAP/AD and returns their profile information
    /// </summary>
    /// <param name="username">The username (sAMAccountName for AD)</param>
    /// <param name="password">The user's password</param>
    /// <returns>Authentication result with user info if successful</returns>
    Task<LdapAuthenticationResult> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Tests LDAP connection settings without authenticating a user
    /// </summary>
    /// <param name="settings">The LDAP settings to test</param>
    /// <returns>True if connection successful, false otherwise</returns>
    Task<LdapConnectionTestResult> TestConnectionAsync(AuthenticationSettingsEntity settings);
}

/// <summary>
/// Result of LDAP authentication attempt
/// </summary>
public class LdapAuthenticationResult
{
    public bool Success { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? DistinguishedName { get; init; }
    public string? ErrorMessage { get; init; }

    public static LdapAuthenticationResult Failed(string message) =>
        new() { Success = false, ErrorMessage = message };

    public static LdapAuthenticationResult Succeeded(string email, string displayName, string dn) =>
        new() { Success = true, Email = email, DisplayName = displayName, DistinguishedName = dn };
}

/// <summary>
/// Result of LDAP connection test
/// </summary>
public class LdapConnectionTestResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public int? UserCount { get; init; }

    public static LdapConnectionTestResult Failed(string message) =>
        new() { Success = false, Message = message };

    public static LdapConnectionTestResult Succeeded(string message, int userCount = 0) =>
        new() { Success = true, Message = message, UserCount = userCount };
}

/// <summary>
/// Implementation of LDAP authentication service
/// </summary>
public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly IAuthenticationSettingsService _settingsService;
    private readonly ILogger<LdapAuthenticationService> _logger;
    private const int ConnectionTimeoutMs = 10000; // 10 seconds
    private const int SearchTimeoutMs = 30000; // 30 seconds

    public LdapAuthenticationService(
        IAuthenticationSettingsService settingsService,
        ILogger<LdapAuthenticationService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<LdapAuthenticationResult> AuthenticateAsync(string username, string password)
    {
        // Validate inputs - never process empty credentials
        if (string.IsNullOrWhiteSpace(username))
        {
            return LdapAuthenticationResult.Failed("Username is required");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return LdapAuthenticationResult.Failed("Password is required");
        }

        // Sanitize username to prevent LDAP injection
        var sanitizedUsername = SanitizeLdapInput(username);

        var settings = await _settingsService.GetSettingsAsync();

        if (!settings.LdapEnabled)
        {
            return LdapAuthenticationResult.Failed("LDAP authentication is not enabled");
        }

        if (string.IsNullOrEmpty(settings.LdapServer) || string.IsNullOrEmpty(settings.LdapBaseDn))
        {
            _logger.LogError("LDAP is enabled but server or base DN is not configured");
            return LdapAuthenticationResult.Failed("LDAP is not properly configured");
        }

        LdapConnection? connection = null;
        try
        {
            connection = CreateConnection(settings);

            // Step 1: Bind with service account to search for user
            await BindServiceAccountAsync(connection, settings);

            // Step 2: Search for the user
            var userEntry = await SearchUserAsync(connection, settings, sanitizedUsername);

            if (userEntry == null)
            {
                _logger.LogWarning("LDAP user not found: {Username}", sanitizedUsername);
                return LdapAuthenticationResult.Failed("Invalid username or password");
            }

            var userDn = userEntry.Dn;

            // Step 3: Verify user's password by attempting to bind as the user
            // Create a new connection for user bind (security best practice)
            using var userConnection = CreateConnection(settings);

            try
            {
                // Attempt to bind as the user - this verifies their password
                userConnection.Bind(userDn, password);
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.InvalidCredentials)
            {
                _logger.LogWarning("LDAP authentication failed for user: {Username}", sanitizedUsername);
                return LdapAuthenticationResult.Failed("Invalid username or password");
            }

            // Step 4: Extract user attributes
            var email = GetAttributeValue(userEntry, settings.LdapEmailAttribute ?? "mail");
            var displayName = GetAttributeValue(userEntry, settings.LdapDisplayNameAttribute ?? "displayName");

            // Use username as email fallback if email attribute is empty
            if (string.IsNullOrEmpty(email))
            {
                email = $"{sanitizedUsername}@ldap.local";
                _logger.LogWarning("LDAP user {Username} has no email attribute, using fallback", sanitizedUsername);
            }

            _logger.LogInformation("LDAP authentication successful for user: {Username}", sanitizedUsername);

            return LdapAuthenticationResult.Succeeded(email, displayName ?? sanitizedUsername, userDn);
        }
        catch (LdapException ex)
        {
            _logger.LogError(ex, "LDAP error during authentication for user: {Username}", sanitizedUsername);
            return LdapAuthenticationResult.Failed("Authentication service error. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during LDAP authentication for user: {Username}", sanitizedUsername);
            return LdapAuthenticationResult.Failed("An unexpected error occurred. Please try again later.");
        }
        finally
        {
            connection?.Dispose();
        }
    }

    public async Task<LdapConnectionTestResult> TestConnectionAsync(AuthenticationSettingsEntity settings)
    {
        if (string.IsNullOrEmpty(settings.LdapServer))
        {
            return LdapConnectionTestResult.Failed("LDAP server is required");
        }

        if (string.IsNullOrEmpty(settings.LdapBaseDn))
        {
            return LdapConnectionTestResult.Failed("Base DN is required");
        }

        LdapConnection? connection = null;
        try
        {
            connection = CreateConnection(settings);

            // Test bind with service account
            await BindServiceAccountAsync(connection, settings);

            // Test search capability
            var searchFilter = "(objectClass=user)";
            var searchConstraints = new LdapSearchConstraints
            {
                MaxResults = 5, // Just get a few to verify search works
                ServerTimeLimit = 10
            };

            var results = connection.Search(
                settings.LdapBaseDn,
                LdapConnection.ScopeSub,
                searchFilter,
                new[] { "cn" },
                false,
                searchConstraints);

            var count = 0;
            while (results.HasMore() && count < 5)
            {
                try
                {
                    results.Next();
                    count++;
                }
                catch (LdapException)
                {
                    break;
                }
            }

            _logger.LogInformation("LDAP connection test successful. Found {Count} users", count);
            return LdapConnectionTestResult.Succeeded($"Connection successful. Found {count} user(s) in directory.", count);
        }
        catch (LdapException ex)
        {
            _logger.LogError(ex, "LDAP connection test failed");
            return LdapConnectionTestResult.Failed($"Connection failed: {GetFriendlyLdapError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP connection test error");
            return LdapConnectionTestResult.Failed($"Connection error: {ex.Message}");
        }
        finally
        {
            connection?.Dispose();
        }
    }

    private LdapConnection CreateConnection(AuthenticationSettingsEntity settings)
    {
        var connection = new LdapConnection
        {
            SecureSocketLayer = settings.LdapUseSsl,
            ConnectionTimeout = ConnectionTimeoutMs
        };

        var port = settings.LdapPort > 0 ? settings.LdapPort : (settings.LdapUseSsl ? 636 : 389);
        connection.Connect(settings.LdapServer, port);

        return connection;
    }

    private Task BindServiceAccountAsync(LdapConnection connection, AuthenticationSettingsEntity settings)
    {
        // If bind DN is specified, use service account credentials
        if (!string.IsNullOrEmpty(settings.LdapBindDn))
        {
            if (string.IsNullOrEmpty(settings.LdapBindPassword))
            {
                throw new InvalidOperationException("Bind password is required when bind DN is specified");
            }

            connection.Bind(settings.LdapBindDn, settings.LdapBindPassword);
        }
        else
        {
            // Anonymous bind (some LDAP servers allow this for searches)
            connection.Bind(null, null);
        }

        return Task.CompletedTask;
    }

    private Task<LdapEntry?> SearchUserAsync(
        LdapConnection connection,
        AuthenticationSettingsEntity settings,
        string username)
    {
        // Build search filter using the configured pattern
        // Default: (sAMAccountName={0}) for Active Directory
        var filterTemplate = settings.LdapUserSearchFilter ?? "(sAMAccountName={0})";

        // Replace {0} with the sanitized username
        // The username is already sanitized, but we escape it again for the filter
        var searchFilter = filterTemplate.Replace("{0}", EscapeLdapFilterValue(username));

        var searchConstraints = new LdapSearchConstraints
        {
            MaxResults = 1,
            ServerTimeLimit = SearchTimeoutMs / 1000
        };

        // Request attributes we need
        var attributes = new[]
        {
            "dn",
            settings.LdapEmailAttribute ?? "mail",
            settings.LdapDisplayNameAttribute ?? "displayName",
            "sAMAccountName",
            "userPrincipalName"
        };

        var results = connection.Search(
            settings.LdapBaseDn,
            LdapConnection.ScopeSub,
            searchFilter,
            attributes,
            false,
            searchConstraints);

        if (results.HasMore())
        {
            try
            {
                return Task.FromResult<LdapEntry?>(results.Next());
            }
            catch (LdapException)
            {
                return Task.FromResult<LdapEntry?>(null);
            }
        }

        return Task.FromResult<LdapEntry?>(null);
    }

    private static string? GetAttributeValue(LdapEntry entry, string attributeName)
    {
        try
        {
            var attribute = entry.GetAttribute(attributeName);
            return attribute?.StringValue;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sanitizes input to prevent LDAP injection attacks.
    /// Removes or escapes special LDAP characters.
    /// </summary>
    private static string SanitizeLdapInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove null bytes and other control characters
        var sanitized = new string(input.Where(c => !char.IsControl(c)).ToArray());

        // Limit length to prevent DoS
        if (sanitized.Length > 256)
            sanitized = sanitized[..256];

        return sanitized;
    }

    /// <summary>
    /// Escapes special characters for use in LDAP filter values.
    /// Per RFC 4515, these characters must be escaped: * ( ) \ NUL
    /// </summary>
    private static string EscapeLdapFilterValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }

    private static string GetFriendlyLdapError(LdapException ex)
    {
        return ex.ResultCode switch
        {
            LdapException.ConnectError => "Cannot connect to LDAP server. Check server address and port.",
            LdapException.InvalidCredentials => "Invalid bind credentials. Check bind DN and password.",
            LdapException.NoSuchObject => "Base DN not found. Check your base DN configuration.",
            LdapException.InsufficientAccessRights => "Insufficient permissions. Check service account rights.",
            LdapException.Unavailable => "LDAP server is unavailable.",
            LdapException.ServerDown => "LDAP server is down.",
            LdapException.TimeLimitExceeded => "Search timed out. Try narrowing your search scope.",
            LdapException.SizeLimitExceeded => "Too many results. Try narrowing your search scope.",
            _ => $"LDAP error ({ex.ResultCode}): {ex.Message}"
        };
    }
}
