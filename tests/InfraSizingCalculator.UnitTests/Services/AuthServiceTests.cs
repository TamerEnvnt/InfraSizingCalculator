using InfraSizingCalculator.Data.Identity;
using InfraSizingCalculator.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

public class AuthServiceTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);

        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager, contextAccessor, claimsFactory, null, null, null, null);

        _logger = Substitute.For<ILogger<AuthService>>();

        _authService = new AuthService(_userManager, _signInManager, _logger);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        _userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns((ApplicationUser?)null);

        // Act
        var result = await _authService.LoginAsync("nonexistent@test.com", "password");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email or password.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Email = "inactive@test.com",
            IsActive = false
        };

        _userManager.FindByEmailAsync("inactive@test.com")
            .Returns(user);

        // Act
        var result = await _authService.LoginAsync("inactive@test.com", "password");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("This account has been deactivated.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "1",
            Email = "valid@test.com",
            UserName = "valid@test.com",
            IsActive = true
        };

        _userManager.FindByEmailAsync("valid@test.com")
            .Returns(user);

        _signInManager.PasswordSignInAsync(
                user, "correctpassword", Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(SignInResult.Success);

        _userManager.UpdateAsync(Arg.Any<ApplicationUser>())
            .Returns(IdentityResult.Success);

        // Act
        var result = await _authService.LoginAsync("valid@test.com", "correctpassword");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);
        Assert.Equal("valid@test.com", result.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithLockedOutUser_ReturnsLockedOutMessage()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Email = "lockedout@test.com",
            IsActive = true
        };

        _userManager.FindByEmailAsync("lockedout@test.com")
            .Returns(user);

        _signInManager.PasswordSignInAsync(
                user, Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(SignInResult.LockedOut);

        // Act
        var result = await _authService.LoginAsync("lockedout@test.com", "password");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Account is locked. Please try again later.", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var existingUser = new ApplicationUser { Email = "existing@test.com" };

        _userManager.FindByEmailAsync("existing@test.com")
            .Returns(existingUser);

        // Act
        var result = await _authService.RegisterAsync("existing@test.com", "Password123");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("An account with this email already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndAssignsRole()
    {
        // Arrange
        _userManager.FindByEmailAsync("new@test.com")
            .Returns((ApplicationUser?)null);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), "Password123!")
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "User")
            .Returns(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync("new@test.com", "Password123!", "New User");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);

        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordValidationError_ReturnsErrors()
    {
        // Arrange
        _userManager.FindByEmailAsync("new@test.com")
            .Returns((ApplicationUser?)null);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), "weak")
            .Returns(IdentityResult.Failed(
                new IdentityError { Description = "Password too short" },
                new IdentityError { Description = "Password must have uppercase" }));

        // Act
        var result = await _authService.RegisterAsync("new@test.com", "weak");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Password too short", result.ErrorMessage);
        Assert.Contains("Password must have uppercase", result.ErrorMessage);
    }

    [Fact]
    public async Task UserExistsAsync_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        _userManager.FindByEmailAsync("exists@test.com")
            .Returns(new ApplicationUser { Email = "exists@test.com" });

        // Act
        var exists = await _authService.UserExistsAsync("exists@test.com");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task UserExistsAsync_WithNonExistingUser_ReturnsFalse()
    {
        // Arrange
        _userManager.FindByEmailAsync("notexists@test.com")
            .Returns((ApplicationUser?)null);

        // Act
        var exists = await _authService.UserExistsAsync("notexists@test.com");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task AdminExistsAsync_WithNoAdmins_ReturnsFalse()
    {
        // Arrange
        _userManager.GetUsersInRoleAsync("Admin")
            .Returns(new List<ApplicationUser>());

        // Act
        var exists = await _authService.AdminExistsAsync();

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task AdminExistsAsync_WithAdmins_ReturnsTrue()
    {
        // Arrange
        _userManager.GetUsersInRoleAsync("Admin")
            .Returns(new List<ApplicationUser> { new() { Email = "admin@test.com" } });

        // Act
        var exists = await _authService.AdminExistsAsync();

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task CreateInitialAdminAsync_WhenAdminExists_ReturnsFailure()
    {
        // Arrange
        _userManager.GetUsersInRoleAsync("Admin")
            .Returns(new List<ApplicationUser> { new() { Email = "admin@test.com" } });

        // Act
        var result = await _authService.CreateInitialAdminAsync("newadmin@test.com", "Password123!", "New Admin");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("An admin account already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateInitialAdminAsync_WhenNoAdmins_CreatesAdminUser()
    {
        // Arrange
        _userManager.GetUsersInRoleAsync("Admin")
            .Returns(new List<ApplicationUser>());

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), "Password123!")
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Admin")
            .Returns(IdentityResult.Success);

        // Act
        var result = await _authService.CreateInitialAdminAsync("admin@test.com", "Password123!", "Admin User");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);

        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "Admin");
    }

    [Fact]
    public async Task LogoutAsync_CallsSignOut()
    {
        // Arrange
        _signInManager.SignOutAsync()
            .Returns(Task.CompletedTask);

        // Act
        await _authService.LogoutAsync();

        // Assert
        await _signInManager.Received(1).SignOutAsync();
    }
}
