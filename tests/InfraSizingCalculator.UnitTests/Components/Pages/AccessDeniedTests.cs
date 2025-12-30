using Bunit;
using InfraSizingCalculator.Components.Pages;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pages;

public class AccessDeniedTests : TestContext
{
    [Fact]
    public void AccessDenied_RendersMainContainer()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        Assert.NotNull(cut.Find(".access-denied-container"));
    }

    [Fact]
    public void AccessDenied_RendersCard()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        Assert.NotNull(cut.Find(".access-denied-card"));
    }

    [Fact]
    public void AccessDenied_ShowsLockIcon()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var icon = cut.Find(".icon");
        Assert.Contains("🔒", icon.TextContent);
    }

    [Fact]
    public void AccessDenied_HasCorrectPageTitle()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var title = cut.Find("h1");
        Assert.Equal("Access Denied", title.TextContent);
    }

    [Fact]
    public void AccessDenied_ShowsPermissionMessage()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var message = cut.Find(".message");
        Assert.Contains("don't have permission", message.TextContent);
    }

    [Fact]
    public void AccessDenied_ShowsSuggestionText()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var suggestion = cut.Find(".suggestion");
        Assert.Contains("contact your administrator", suggestion.TextContent);
    }

    [Fact]
    public void AccessDenied_HasActionsSection()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        Assert.NotNull(cut.Find(".actions"));
    }

    [Fact]
    public void AccessDenied_HasHomeLink()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var homeLink = cut.Find("a[href='/']");
        Assert.NotNull(homeLink);
        Assert.Equal("Go to Home", homeLink.TextContent);
    }

    [Fact]
    public void AccessDenied_HasSignInLink()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var signInLink = cut.Find("a[href='/login']");
        Assert.NotNull(signInLink);
        Assert.Equal("Sign In", signInLink.TextContent);
    }

    [Fact]
    public void AccessDenied_HomeButton_HasPrimaryClass()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var homeLink = cut.Find("a[href='/']");
        Assert.Contains("btn-primary", homeLink.ClassName);
    }

    [Fact]
    public void AccessDenied_SignInButton_HasSecondaryClass()
    {
        // Act
        var cut = RenderComponent<AccessDenied>();

        // Assert
        var signInLink = cut.Find("a[href='/login']");
        Assert.Contains("btn-secondary", signInLink.ClassName);
    }
}
