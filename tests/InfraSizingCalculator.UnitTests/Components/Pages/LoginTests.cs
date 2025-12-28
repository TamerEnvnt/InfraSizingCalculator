using Bunit;
using InfraSizingCalculator.Components.Pages;
using InfraSizingCalculator.Data.Identity;
using InfraSizingCalculator.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pages;

public class LoginTests : TestContext
{
    private readonly IAuthService _authService;
    private readonly ILogger<Login> _logger;

    public LoginTests()
    {
        _authService = Substitute.For<IAuthService>();
        _logger = Substitute.For<ILogger<Login>>();

        Services.AddSingleton(_authService);
        Services.AddSingleton(_logger);
    }

    [Fact]
    public void Login_RendersFormWithEmailAndPasswordFields()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        Assert.NotNull(cut.Find("input#email"));
        Assert.NotNull(cut.Find("input#password"));
        Assert.NotNull(cut.Find("button[type='submit']"));
    }

    [Fact]
    public void Login_RendersRememberMeCheckbox()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        Assert.NotNull(cut.Find("input#rememberMe"));
    }

    [Fact]
    public void Login_RendersLinkToRegister()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var registerLink = cut.Find("a[href='/register']");
        Assert.NotNull(registerLink);
    }

    [Fact]
    public void Login_SubmitButton_IsEnabledByDefault()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        Assert.False(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task Login_WithValidCredentials_CallsAuthService()
    {
        // Arrange
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(AuthResult.Success(new ApplicationUser { Email = "test@test.com" }));

        var cut = RenderComponent<Login>();

        // Fill in the form
        cut.Find("input#email").Change("test@test.com");
        cut.Find("input#password").Change("Password123!");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        await _authService.Received(1).LoginAsync("test@test.com", "Password123!", Arg.Any<bool>());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_DisplaysErrorMessage()
    {
        // Arrange
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(AuthResult.Failure("Invalid email or password."));

        var cut = RenderComponent<Login>();

        // Fill in the form
        cut.Find("input#email").Change("test@test.com");
        cut.Find("input#password").Change("wrongpassword");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        var errorAlert = cut.Find(".alert-error");
        Assert.Contains("Invalid email or password", errorAlert.TextContent);
    }

    [Fact]
    public async Task Login_WithLockedAccount_DisplaysLockedMessage()
    {
        // Arrange
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(AuthResult.Failure("Account is locked. Please try again later."));

        var cut = RenderComponent<Login>();

        // Fill in the form
        cut.Find("input#email").Change("locked@test.com");
        cut.Find("input#password").Change("password");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        var errorAlert = cut.Find(".alert-error");
        Assert.Contains("Account is locked", errorAlert.TextContent);
    }

    [Fact]
    public void Login_HasCorrectPageTitle()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var pageTitle = cut.Find("h1");
        Assert.Equal("Sign In", pageTitle.TextContent);
    }

    [Fact]
    public void Login_EmailField_HasCorrectAutocomplete()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var emailInput = cut.Find("input#email");
        Assert.Equal("email", emailInput.GetAttribute("autocomplete"));
    }

    [Fact]
    public void Login_PasswordField_HasCorrectType()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var passwordInput = cut.Find("input#password");
        Assert.Equal("password", passwordInput.GetAttribute("type"));
    }

    [Fact]
    public async Task Login_WithRememberMeChecked_PassesTrueToAuthService()
    {
        // Arrange
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(AuthResult.Success(new ApplicationUser { Email = "test@test.com" }));

        var cut = RenderComponent<Login>();

        // Fill in the form and check remember me
        cut.Find("input#email").Change("test@test.com");
        cut.Find("input#password").Change("Password123!");
        cut.Find("input#rememberMe").Change(true);

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        await _authService.Received(1).LoginAsync("test@test.com", "Password123!", true);
    }

    [Fact]
    public void Login_NoErrorMessage_WhenFirstLoaded()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find(".alert-error"));
    }
}
