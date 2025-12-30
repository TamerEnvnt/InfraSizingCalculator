using Bunit;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Data.Identity;
using InfraSizingCalculator.Services.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

public class UserMenuTests : TestContext
{
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigationManager;

    public UserMenuTests()
    {
        _authService = Substitute.For<IAuthService>();

        Services.AddSingleton(_authService);
    }

    [Fact]
    public void UserMenu_WhenNotAuthenticated_ShowsAuthLinks()
    {
        // Arrange
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(null));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        Assert.NotNull(cut.Find(".auth-links"));
        Assert.NotNull(cut.Find("a[href='/login']"));
        Assert.NotNull(cut.Find("a[href='/register']"));
    }

    [Fact]
    public void UserMenu_WhenNotAuthenticated_ShowsSignInText()
    {
        // Arrange
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(null));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var signInLink = cut.Find("a[href='/login']");
        Assert.Equal("Sign In", signInLink.TextContent);
    }

    [Fact]
    public void UserMenu_WhenNotAuthenticated_ShowsCreateAccountText()
    {
        // Arrange
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(null));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var registerLink = cut.Find("a[href='/register']");
        Assert.Equal("Create Account", registerLink.TextContent);
    }

    [Fact]
    public async Task UserMenu_WhenAuthenticated_ShowsUserDropdown()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        Assert.NotNull(cut.Find(".user-dropdown"));
        Assert.NotNull(cut.Find(".user-button"));
    }

    [Fact]
    public async Task UserMenu_WhenAuthenticated_ShowsUserAvatar()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var avatar = cut.Find(".user-avatar");
        Assert.Equal("TU", avatar.TextContent); // First letters of "Test User"
    }

    [Fact]
    public async Task UserMenu_WhenAuthenticated_ShowsDisplayName()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var userName = cut.Find(".user-name");
        Assert.Equal("Test User", userName.TextContent);
    }

    [Fact]
    public async Task UserMenu_WhenAuthenticatedWithoutDisplayName_ShowsEmailPrefix()
    {
        // Arrange
        var user = new ApplicationUser { Email = "john@example.com", DisplayName = null };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var userName = cut.Find(".user-name");
        Assert.Equal("john", userName.TextContent);
    }

    [Fact]
    public async Task UserMenu_ClickingUserButton_TogglesDropdown()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        var cut = RenderComponent<UserMenu>();

        // Initially dropdown should not be open
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find(".dropdown-menu"));

        // Act - click to open
        await cut.Find(".user-button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert - dropdown should be open
        Assert.NotNull(cut.Find(".dropdown-menu"));
    }

    [Fact]
    public async Task UserMenu_WhenDropdownOpen_ShowsUserEmail()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        var cut = RenderComponent<UserMenu>();
        await cut.Find(".user-button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        var email = cut.Find(".user-email");
        Assert.Equal("test@example.com", email.TextContent);
    }

    [Fact]
    public async Task UserMenu_WhenDropdownOpen_ShowsSignOutButton()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        var cut = RenderComponent<UserMenu>();
        await cut.Find(".user-button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        var signOutButton = cut.Find(".dropdown-item");
        Assert.Contains("Sign out", signOutButton.TextContent);
    }

    [Fact]
    public async Task UserMenu_ClickingSignOut_CallsLogoutAsync()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Test User" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));
        _authService.LogoutAsync().Returns(Task.CompletedTask);

        var cut = RenderComponent<UserMenu>();
        await cut.Find(".user-button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Act
        await cut.Find(".dropdown-item").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        await _authService.Received(1).LogoutAsync();
    }

    [Fact]
    public async Task UserMenu_SingleLetterName_ShowsTwoCharacterInitials()
    {
        // Arrange - single word name should take first 2 chars
        var user = new ApplicationUser { Email = "test@example.com", DisplayName = "Administrator" };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var avatar = cut.Find(".user-avatar");
        Assert.Equal("AD", avatar.TextContent);
    }

    [Fact]
    public async Task UserMenu_EmailWithoutDisplayName_ShowsEmailInitials()
    {
        // Arrange
        var user = new ApplicationUser { Email = "john@example.com", DisplayName = null };
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        var avatar = cut.Find(".user-avatar");
        Assert.Equal("JO", avatar.TextContent); // First 2 chars of "john@example.com"
    }

    [Fact]
    public void UserMenu_NotAuthenticated_DoesNotShowDropdown()
    {
        // Arrange
        _authService.GetCurrentUserAsync().Returns(Task.FromResult<ApplicationUser?>(null));

        // Act
        var cut = RenderComponent<UserMenu>();

        // Assert
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find(".user-dropdown"));
    }
}
