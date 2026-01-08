using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;

namespace InfraSizingCalculator.Services.Auth;

/// <summary>
/// Service for managing authentication settings stored in the database
/// </summary>
public interface IAuthenticationSettingsService
{
    Task<AuthenticationSettingsEntity> GetSettingsAsync();
    Task<AuthenticationSettingsEntity> UpdateSettingsAsync(AuthenticationSettingsEntity settings, string updatedBy);
    Task<bool> IsGoogleEnabledAsync();
    Task<bool> IsMicrosoftEnabledAsync();
    Task<bool> IsLdapEnabledAsync();
    Task<bool> IsEmailPasswordEnabledAsync();
    Task EnsureDefaultSettingsAsync();
}

/// <summary>
/// Implementation of authentication settings service
/// </summary>
public class AuthenticationSettingsService : IAuthenticationSettingsService
{
    private readonly InfraSizingDbContext _dbContext;
    private readonly ILogger<AuthenticationSettingsService> _logger;
    private AuthenticationSettingsEntity? _cachedSettings;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public AuthenticationSettingsService(
        InfraSizingDbContext dbContext,
        ILogger<AuthenticationSettingsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AuthenticationSettingsEntity> GetSettingsAsync()
    {
        // Return cached settings if valid
        if (_cachedSettings != null && DateTime.UtcNow < _cacheExpiry)
        {
            return _cachedSettings;
        }

        var settings = await _dbContext.AuthenticationSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            // Create default settings if none exist
            settings = new AuthenticationSettingsEntity();
            _dbContext.AuthenticationSettings.Add(settings);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Created default authentication settings");
        }

        _cachedSettings = settings;
        _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);

        return settings;
    }

    public async Task<AuthenticationSettingsEntity> UpdateSettingsAsync(
        AuthenticationSettingsEntity settings,
        string updatedBy)
    {
        var existing = await _dbContext.AuthenticationSettings.FirstOrDefaultAsync();

        if (existing == null)
        {
            settings.CreatedAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = updatedBy;
            _dbContext.AuthenticationSettings.Add(settings);
        }
        else
        {
            // Update existing settings
            existing.EmailPasswordEnabled = settings.EmailPasswordEnabled;
            existing.RequireEmailConfirmation = settings.RequireEmailConfirmation;
            existing.AllowSelfRegistration = settings.AllowSelfRegistration;

            existing.GoogleEnabled = settings.GoogleEnabled;
            existing.GoogleClientId = settings.GoogleClientId;
            existing.GoogleClientSecret = settings.GoogleClientSecret;

            existing.MicrosoftEnabled = settings.MicrosoftEnabled;
            existing.MicrosoftClientId = settings.MicrosoftClientId;
            existing.MicrosoftClientSecret = settings.MicrosoftClientSecret;
            existing.MicrosoftTenantId = settings.MicrosoftTenantId;

            existing.LdapEnabled = settings.LdapEnabled;
            existing.LdapServer = settings.LdapServer;
            existing.LdapPort = settings.LdapPort;
            existing.LdapUseSsl = settings.LdapUseSsl;
            existing.LdapBaseDn = settings.LdapBaseDn;
            existing.LdapBindDn = settings.LdapBindDn;
            existing.LdapBindPassword = settings.LdapBindPassword;
            existing.LdapUserSearchFilter = settings.LdapUserSearchFilter;
            existing.LdapEmailAttribute = settings.LdapEmailAttribute;
            existing.LdapDisplayNameAttribute = settings.LdapDisplayNameAttribute;

            existing.PasswordMinLength = settings.PasswordMinLength;
            existing.PasswordRequireUppercase = settings.PasswordRequireUppercase;
            existing.PasswordRequireLowercase = settings.PasswordRequireLowercase;
            existing.PasswordRequireDigit = settings.PasswordRequireDigit;
            existing.PasswordRequireSpecialChar = settings.PasswordRequireSpecialChar;

            existing.SessionTimeoutMinutes = settings.SessionTimeoutMinutes;
            existing.AllowRememberMe = settings.AllowRememberMe;
            existing.RememberMeDays = settings.RememberMeDays;

            existing.LockoutEnabled = settings.LockoutEnabled;
            existing.LockoutMaxAttempts = settings.LockoutMaxAttempts;
            existing.LockoutDurationMinutes = settings.LockoutDurationMinutes;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = updatedBy;

            settings = existing;
        }

        await _dbContext.SaveChangesAsync();

        // Invalidate cache
        _cachedSettings = null;
        _cacheExpiry = DateTime.MinValue;

        _logger.LogInformation("Authentication settings updated by {UpdatedBy}", updatedBy);

        return settings;
    }

    public async Task<bool> IsGoogleEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.GoogleEnabled &&
               !string.IsNullOrEmpty(settings.GoogleClientId) &&
               !string.IsNullOrEmpty(settings.GoogleClientSecret);
    }

    public async Task<bool> IsMicrosoftEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.MicrosoftEnabled &&
               !string.IsNullOrEmpty(settings.MicrosoftClientId) &&
               !string.IsNullOrEmpty(settings.MicrosoftClientSecret);
    }

    public async Task<bool> IsLdapEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.LdapEnabled &&
               !string.IsNullOrEmpty(settings.LdapServer) &&
               !string.IsNullOrEmpty(settings.LdapBaseDn);
    }

    public async Task<bool> IsEmailPasswordEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.EmailPasswordEnabled;
    }

    public async Task EnsureDefaultSettingsAsync()
    {
        var exists = await _dbContext.AuthenticationSettings.AnyAsync();
        if (!exists)
        {
            var defaultSettings = new AuthenticationSettingsEntity
            {
                EmailPasswordEnabled = true,
                AllowSelfRegistration = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.AuthenticationSettings.Add(defaultSettings);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Default authentication settings created");
        }
    }
}
