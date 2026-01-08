using System.ComponentModel.DataAnnotations;

namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Stores authentication provider configuration
/// </summary>
public class AuthenticationSettingsEntity
{
    [Key]
    public int Id { get; set; }

    // Email/Password Authentication
    public bool EmailPasswordEnabled { get; set; } = true;
    public bool RequireEmailConfirmation { get; set; } = false;
    public bool AllowSelfRegistration { get; set; } = true;

    // Google OAuth
    public bool GoogleEnabled { get; set; } = false;
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }

    // Microsoft OAuth
    public bool MicrosoftEnabled { get; set; } = false;
    public string? MicrosoftClientId { get; set; }
    public string? MicrosoftClientSecret { get; set; }
    public string? MicrosoftTenantId { get; set; }

    // LDAP/Active Directory
    public bool LdapEnabled { get; set; } = false;
    public string? LdapServer { get; set; }
    public int LdapPort { get; set; } = 389;
    public bool LdapUseSsl { get; set; } = false;
    public string? LdapBaseDn { get; set; }
    public string? LdapBindDn { get; set; }
    public string? LdapBindPassword { get; set; }
    public string? LdapUserSearchFilter { get; set; } = "(sAMAccountName={0})";
    public string? LdapEmailAttribute { get; set; } = "mail";
    public string? LdapDisplayNameAttribute { get; set; } = "displayName";

    // Password Policy
    public int PasswordMinLength { get; set; } = 8;
    public bool PasswordRequireUppercase { get; set; } = true;
    public bool PasswordRequireLowercase { get; set; } = true;
    public bool PasswordRequireDigit { get; set; } = true;
    public bool PasswordRequireSpecialChar { get; set; } = true;

    // Session Settings
    public int SessionTimeoutMinutes { get; set; } = 60;
    public bool AllowRememberMe { get; set; } = true;
    public int RememberMeDays { get; set; } = 30;

    // Lockout Settings
    public bool LockoutEnabled { get; set; } = true;
    public int LockoutMaxAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
