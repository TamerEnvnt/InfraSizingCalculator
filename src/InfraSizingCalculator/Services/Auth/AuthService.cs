using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Data.Identity;

namespace InfraSizingCalculator.Services.Auth;

/// <summary>
/// Authentication service implementation using ASP.NET Core Identity
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
            return AuthResult.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", email);
            return AuthResult.Failure("This account has been deactivated.");
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: rememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User logged in: {Email}", email);
            return AuthResult.Success(user);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", email);
            return AuthResult.Failure("Account is locked. Please try again later.");
        }

        _logger.LogWarning("Invalid login attempt for user: {Email}", email);
        return AuthResult.Failure("Invalid email or password.");
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string? displayName = null)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return AuthResult.Failure("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName ?? email.Split('@')[0],
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("New user registered: {Email}", email);
            return AuthResult.Success(user);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("Failed to register user: {Email}. Errors: {Errors}", email, errors);
        return AuthResult.Failure(errors);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var principal = _signInManager.Context.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return await _userManager.GetUserAsync(principal);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<bool> AdminExistsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins.Any();
    }

    public async Task<AuthResult> CreateInitialAdminAsync(string email, string password, string displayName)
    {
        // Check if admin already exists
        if (await AdminExistsAsync())
        {
            return AuthResult.Failure("An admin account already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
            _logger.LogInformation("Initial admin account created: {Email}", email);
            return AuthResult.Success(user);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return AuthResult.Failure(errors);
    }
}
