using Bunit;
using InfraSizingCalculator.Components.Pages;
using InfraSizingCalculator.Data.Identity;
using InfraSizingCalculator.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pages;

public class RegisterTests : TestContext
{
    private readonly IAuthService _authService;
    private readonly ILogger<Register> _logger;

    public RegisterTests()
    {
        _authService = Substitute.For<IAuthService>();
        _logger = Substitute.For<ILogger<Register>>();

        Services.AddSingleton(_authService);
        Services.AddSingleton(_logger);
    }

    [Fact]
    public void Register_RendersFormWithAllFields()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        Assert.NotNull(cut.Find("input#displayName"));
        Assert.NotNull(cut.Find("input#email"));
        Assert.NotNull(cut.Find("input#password"));
        Assert.NotNull(cut.Find("input#confirmPassword"));
        Assert.NotNull(cut.Find("button[type='submit']"));
    }

    [Fact]
    public void Register_RendersLinkToLogin()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var loginLink = cut.Find("a[href='/login']");
        Assert.NotNull(loginLink);
    }

    [Fact]
    public void Register_HasCorrectPageTitle()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var pageTitle = cut.Find("h1");
        Assert.Equal("Create Account", pageTitle.TextContent);
    }

    [Fact]
    public void Register_PasswordField_HasCorrectType()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var passwordInput = cut.Find("input#password");
        var confirmPasswordInput = cut.Find("input#confirmPassword");
        Assert.Equal("password", passwordInput.GetAttribute("type"));
        Assert.Equal("password", confirmPasswordInput.GetAttribute("type"));
    }

    [Fact]
    public void Register_EmailField_HasCorrectAutocomplete()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var emailInput = cut.Find("input#email");
        Assert.Equal("email", emailInput.GetAttribute("autocomplete"));
    }

    [Fact]
    public void Register_PasswordField_HasNewPasswordAutocomplete()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var passwordInput = cut.Find("input#password");
        Assert.Equal("new-password", passwordInput.GetAttribute("autocomplete"));
    }

    [Fact]
    public async Task Register_WithValidData_CallsAuthService()
    {
        // Arrange
        _authService.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(AuthResult.Success(new ApplicationUser { Email = "new@test.com" }));

        var cut = RenderComponent<Register>();

        // Fill in the form
        cut.Find("input#displayName").Change("New User");
        cut.Find("input#email").Change("new@test.com");
        cut.Find("input#password").Change("Password123!");
        cut.Find("input#confirmPassword").Change("Password123!");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        await _authService.Received(1).RegisterAsync("new@test.com", "Password123!", "New User");
    }

    [Fact]
    public async Task Register_WithExistingEmail_DisplaysErrorMessage()
    {
        // Arrange
        _authService.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(AuthResult.Failure("An account with this email already exists."));

        var cut = RenderComponent<Register>();

        // Fill in the form
        cut.Find("input#displayName").Change("New User");
        cut.Find("input#email").Change("existing@test.com");
        cut.Find("input#password").Change("Password123!");
        cut.Find("input#confirmPassword").Change("Password123!");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        var errorAlert = cut.Find(".alert-error");
        Assert.Contains("already exists", errorAlert.TextContent);
    }

    [Fact]
    public async Task Register_WithWeakPassword_DisplaysErrorMessage()
    {
        // Arrange
        // Use a password that passes form validation (8+ chars) but is rejected by the auth service
        _authService.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(AuthResult.Failure("Password does not meet complexity requirements."));

        var cut = RenderComponent<Register>();

        // Fill in the form - password is 8+ chars to pass form validation
        cut.Find("input#displayName").Change("New User");
        cut.Find("input#email").Change("new@test.com");
        cut.Find("input#password").Change("weakpassword");
        cut.Find("input#confirmPassword").Change("weakpassword");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        var errorAlert = cut.Find(".alert-error");
        Assert.Contains("complexity", errorAlert.TextContent);
    }

    [Fact]
    public async Task Register_WithSuccess_DisplaysSuccessMessage()
    {
        // Arrange
        _authService.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(AuthResult.Success(new ApplicationUser { Email = "new@test.com" }));

        var cut = RenderComponent<Register>();

        // Fill in the form
        cut.Find("input#displayName").Change("New User");
        cut.Find("input#email").Change("new@test.com");
        cut.Find("input#password").Change("Password123!");
        cut.Find("input#confirmPassword").Change("Password123!");

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        var successAlert = cut.Find(".alert-success");
        Assert.Contains("successfully", successAlert.TextContent);
    }

    [Fact]
    public void Register_NoErrorMessage_WhenFirstLoaded()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find(".alert-error"));
    }

    [Fact]
    public void Register_SubmitButton_IsEnabledByDefault()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        Assert.False(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public void Register_HasPasswordRequirementsHint()
    {
        // Act
        var cut = RenderComponent<Register>();

        // Assert
        var hint = cut.Find(".form-hint");
        Assert.Contains("uppercase", hint.TextContent.ToLower());
    }
}
