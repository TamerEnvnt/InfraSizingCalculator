using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Layout;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Layout;

/// <summary>
/// Tests for HeaderBar component - Top navigation and actions
/// </summary>
public class HeaderBarTests : TestContext
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ISettingsPersistenceService _settingsService;

    public HeaderBarTests()
    {
        _jsRuntime = Substitute.For<IJSRuntime>();
        _settingsService = Substitute.For<ISettingsPersistenceService>();

        // Default theme is dark
        _jsRuntime.InvokeAsync<string>("ThemeManager.get", Arg.Any<object[]>())
            .Returns(new ValueTask<string>("dark"));

        Services.AddSingleton(_jsRuntime);
        Services.AddSingleton(_settingsService);
        Services.AddSingleton<NavigationManager>(new FakeNavigationManager(this));
    }

    #region Rendering Tests

    [Fact]
    public void HeaderBar_RendersHeaderElement()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find("header.header-bar").Should().NotBeNull();
    }

    [Fact]
    public void HeaderBar_RendersTitle()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Infrastructure Sizing Calculator");
    }

    [Fact]
    public void HeaderBar_RendersLogoIcon()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".logo-icon").Should().NotBeNull();
        cut.Find(".logo-icon svg").Should().NotBeNull();
    }

    [Fact]
    public void HeaderBar_RendersSettingsButton()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        var settingsBtn = cut.Find(".settings-btn");
        settingsBtn.Should().NotBeNull();
        settingsBtn.TextContent.Should().Contain("Settings");
    }

    [Fact]
    public void HeaderBar_RendersSavedButton()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        var buttons = cut.FindAll(".header-btn");
        buttons.Should().Contain(b => b.TextContent.Contains("Saved"));
    }

    [Fact]
    public void HeaderBar_RendersResetButton()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        var buttons = cut.FindAll(".header-btn");
        buttons.Should().Contain(b => b.TextContent.Contains("Reset"));
    }

    [Fact]
    public void HeaderBar_RendersThemeToggle()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".theme-toggle").Should().NotBeNull();
    }

    #endregion

    #region Action Tests

    [Fact]
    public void HeaderBar_ClickingSettings_NavigatesToSettings()
    {
        // Arrange
        var cut = RenderComponent<HeaderBar>();
        var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

        // Act
        cut.Find(".settings-btn").Click();

        // Assert
        nav!.NavigatedUri.Should().Be("/settings");
    }

    [Fact]
    public async Task HeaderBar_ClickingSaved_InvokesOnAction()
    {
        // Arrange
        string? actionReceived = null;
        var cut = RenderComponent<HeaderBar>(parameters => parameters
            .Add(p => p.OnAction, EventCallback.Factory.Create<string>(this, a => actionReceived = a)));

        // Act
        var savedBtn = cut.FindAll(".header-btn").First(b => b.TextContent.Contains("Saved"));
        await cut.InvokeAsync(() => savedBtn.Click());

        // Assert
        actionReceived.Should().Be("scenarios");
    }

    [Fact]
    public async Task HeaderBar_ClickingReset_InvokesOnAction()
    {
        // Arrange
        string? actionReceived = null;
        var cut = RenderComponent<HeaderBar>(parameters => parameters
            .Add(p => p.OnAction, EventCallback.Factory.Create<string>(this, a => actionReceived = a)));

        // Act
        var resetBtn = cut.FindAll(".header-btn").First(b => b.TextContent.Contains("Reset"));
        await cut.InvokeAsync(() => resetBtn.Click());

        // Assert
        actionReceived.Should().Be("reset");
    }

    [Fact]
    public async Task HeaderBar_NoCallback_DoesNotThrowOnClick()
    {
        // Arrange
        var cut = RenderComponent<HeaderBar>();

        // Act & Assert
        var action = async () =>
        {
            var btn = cut.FindAll(".header-btn").First(b => b.TextContent.Contains("Saved"));
            await cut.InvokeAsync(() => btn.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Theme Toggle Tests

    [Fact]
    public async Task HeaderBar_ClickingThemeToggle_CallsThemeManager()
    {
        // Arrange
        var cut = RenderComponent<HeaderBar>();

        // Act
        await cut.InvokeAsync(() => cut.Find(".theme-toggle").Click());

        // Assert
        await _jsRuntime.Received(1).InvokeVoidAsync("ThemeManager.apply", Arg.Any<object[]>());
    }

    [Fact]
    public async Task HeaderBar_ClickingThemeToggle_SavesTheme()
    {
        // Arrange
        var cut = RenderComponent<HeaderBar>();

        // Act
        await cut.InvokeAsync(() => cut.Find(".theme-toggle").Click());

        // Assert
        await _settingsService.Received(1).SetThemeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task HeaderBar_ThemeToggle_TogglesFromDarkToLight()
    {
        // Arrange
        var cut = RenderComponent<HeaderBar>();

        // Act - Toggle theme (was dark, should become light)
        await cut.InvokeAsync(() => cut.Find(".theme-toggle").Click());

        // Assert - Should apply "light" theme
        // Use Arg.Any for the call args since InvokeVoidAsync takes params object[]
        await _jsRuntime.Received().InvokeVoidAsync("ThemeManager.apply", Arg.Is<object[]>(args => args.Length == 1 && (string)args[0] == "light"));
    }

    #endregion

    #region Structure Tests

    [Fact]
    public void HeaderBar_HasHeaderLeft()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".header-left").Should().NotBeNull();
    }

    [Fact]
    public void HeaderBar_HasHeaderActions()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".header-actions").Should().NotBeNull();
    }

    [Fact]
    public void HeaderBar_HasHeaderRight()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".header-right").Should().NotBeNull();
    }

    [Fact]
    public void HeaderBar_HasThreeMainSections()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.FindAll(".header-bar > div").Should().HaveCount(3);
    }

    #endregion

    #region Icon Tests

    [Fact]
    public void HeaderBar_SettingsButton_HasIcon()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".settings-btn .btn-icon").Should().NotBeNull();
    }

    [Fact]
    public void HeaderBar_ThemeToggle_HasIcon()
    {
        // Act
        var cut = RenderComponent<HeaderBar>();

        // Assert
        cut.Find(".theme-toggle .theme-icon").Should().NotBeNull();
    }

    #endregion

    /// <summary>
    /// Fake NavigationManager for testing navigation
    /// </summary>
    private class FakeNavigationManager : NavigationManager
    {
        private readonly TestContext _context;

        public FakeNavigationManager(TestContext context)
        {
            _context = context;
            Initialize("http://localhost/", "http://localhost/");
        }

        public string? NavigatedUri { get; private set; }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NavigatedUri = uri;
        }
    }
}
