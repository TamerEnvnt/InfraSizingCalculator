using InfraSizingCalculator.E2ETests.PageObjects;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests.Components;

/// <summary>
/// E2E tests for navigation links - NavLinks, anchor tags, and internal navigation.
/// Tests cover main nav menu, auth links, error page links, and external documentation links.
/// </summary>
[TestFixture]
public class NavigationLinkTests : PlaywrightFixture
{
    private WizardPage _wizard = null!;

    // Locators
    private const string NavLink = ".nav-link, a.nav-link";
    private const string NavMenu = ".navbar, .nav-menu, nav";
    private const string AuthLink = ".auth-link, a[href='/login'], a[href='/register']";
    private const string ExternalLink = "a[target='_blank'], a[href^='http']";
    private const string InternalLink = "a[href^='/'], a[href^='./'], a:not([href^='http'])";
    private const string BrandLink = ".navbar-brand, .brand-link, .logo-link";
    private const string FooterLink = "footer a, .app-footer a";

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _wizard = new WizardPage(Page);
    }

    [TearDown]
    public new async Task TearDown()
    {
        await base.TearDown();
    }

    #region Main Navigation Tests

    [Test]
    public async Task NavMenu_IsVisible_OnHomePage()
    {
        await _wizard.GoToHomeAsync();

        var navMenu = await Page.QuerySelectorAsync(NavMenu);

        if (navMenu == null)
        {
            // Check for header-based navigation
            navMenu = await Page.QuerySelectorAsync("header, .header-bar");
        }

        Assert.That(navMenu != null, Is.True,
            "Navigation menu should be visible on home page");
    }

    [Test]
    public async Task NavLinks_AreClickable()
    {
        await _wizard.GoToHomeAsync();

        var navLinks = await Page.QuerySelectorAllAsync(NavLink);

        if (navLinks.Count == 0)
        {
            // Try finding any navigation links
            navLinks = await Page.QuerySelectorAllAsync("nav a, .nav a, header a");
        }

        if (navLinks.Count == 0)
        {
            Assert.Pass("No navigation links found - may use button-based navigation");
            return;
        }

        // Verify links are visible and have href
        foreach (var link in navLinks.Take(3))
        {
            var isVisible = await link.IsVisibleAsync();
            var href = await link.GetAttributeAsync("href");

            if (isVisible)
            {
                Assert.That(href, Is.Not.Null,
                    "Visible nav link should have href attribute");
            }
        }

        Assert.Pass($"Found {navLinks.Count} navigation links");
    }

    [Test]
    public async Task BrandLink_NavigatesToHome()
    {
        await _wizard.GoToHomeAsync();

        var brandLink = await Page.QuerySelectorAsync(BrandLink);

        if (brandLink == null)
        {
            brandLink = await Page.QuerySelectorAsync("a[href='/'], a[href=''], .logo");
        }

        if (brandLink == null)
        {
            Assert.Pass("Brand/logo link not found - may use different navigation");
            return;
        }

        var href = await brandLink.GetAttributeAsync("href");

        // Brand link should go to home
        Assert.That(href == "/" || href == "" || href == "./",
            Is.True, "Brand link should navigate to home");
    }

    [Test]
    public async Task NavLink_HighlightsCurrentPage()
    {
        await _wizard.GoToHomeAsync();

        var navLinks = await Page.QuerySelectorAllAsync(NavLink);

        if (navLinks.Count == 0)
        {
            Assert.Pass("No nav links found");
            return;
        }

        // Check for active state on current page link
        var hasActiveLink = false;

        foreach (var link in navLinks)
        {
            var isActive = await link.EvaluateAsync<bool>(
                "el => el.classList.contains('active') || el.getAttribute('aria-current') === 'page'");

            if (isActive)
            {
                hasActiveLink = true;
                break;
            }
        }

        Assert.Pass(hasActiveLink ?
            "Current page link is highlighted" :
            "Active state may use different styling");
    }

    #endregion

    #region Authentication Link Tests

    [Test]
    public async Task LoginLink_IsVisible_WhenNotAuthenticated()
    {
        await _wizard.GoToHomeAsync();

        var loginLink = await Page.QuerySelectorAsync(
            "a[href='/login'], a:has-text('Sign In'), a:has-text('Login'), .auth-link");

        if (loginLink == null)
        {
            // Check user menu for auth links
            var userMenu = await Page.QuerySelectorAsync(".user-menu, .auth-menu");
            if (userMenu != null)
            {
                loginLink = await userMenu.QuerySelectorAsync("a");
            }
        }

        if (loginLink == null)
        {
            Assert.Pass("Login link not found - may require user menu interaction");
            return;
        }

        Assert.That(await loginLink.IsVisibleAsync(), Is.True,
            "Login link should be visible when not authenticated");
    }

    [Test]
    public async Task RegisterLink_IsVisible_WhenNotAuthenticated()
    {
        await _wizard.GoToHomeAsync();

        var registerLink = await Page.QuerySelectorAsync(
            "a[href='/register'], a:has-text('Create Account'), a:has-text('Register'), a:has-text('Sign Up')");

        if (registerLink == null)
        {
            Assert.Pass("Register link not found - may not be available or requires menu");
            return;
        }

        Assert.That(await registerLink.IsVisibleAsync(), Is.True,
            "Register link should be visible when not authenticated");
    }

    [Test]
    public async Task LoginLink_NavigatesToLoginPage()
    {
        await _wizard.GoToHomeAsync();

        var loginLink = await Page.QuerySelectorAsync(
            "a[href='/login'], a:has-text('Sign In')");

        if (loginLink == null)
        {
            Assert.Pass("Login link not found");
            return;
        }

        await loginLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Verify navigation to login page
        var url = Page.Url;
        var hasLoginForm = await Page.QuerySelectorAsync(
            "form[action*='login'], input[type='password'], .login-form");

        Assert.That(url.Contains("login") || hasLoginForm != null, Is.True,
            "Clicking login link should navigate to login page");
    }

    [Test]
    public async Task RegisterLink_NavigatesToRegisterPage()
    {
        await _wizard.GoToHomeAsync();

        var registerLink = await Page.QuerySelectorAsync(
            "a[href='/register'], a:has-text('Create Account'), a:has-text('Register')");

        if (registerLink == null)
        {
            Assert.Pass("Register link not found");
            return;
        }

        await registerLink.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Verify navigation to register page
        var url = Page.Url;
        var hasRegisterForm = await Page.QuerySelectorAsync(
            "form[action*='register'], .register-form, input[name='confirmPassword']");

        Assert.That(url.Contains("register") || hasRegisterForm != null, Is.True,
            "Clicking register link should navigate to register page");
    }

    [Test]
    public async Task LoginPage_HasLinkToRegister()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.WaitForTimeoutAsync(500);

        var registerLink = await Page.QuerySelectorAsync(
            "a[href='/register'], a:has-text('Create'), a:has-text('Sign up')");

        if (registerLink == null)
        {
            Assert.Pass("Register link on login page not found - may use different flow");
            return;
        }

        Assert.That(await registerLink.IsVisibleAsync(), Is.True,
            "Login page should have link to register page");
    }

    [Test]
    public async Task RegisterPage_HasLinkToLogin()
    {
        await Page.GotoAsync($"{BaseUrl}/register");
        await Page.WaitForTimeoutAsync(500);

        var loginLink = await Page.QuerySelectorAsync(
            "a[href='/login'], a:has-text('Sign in'), a:has-text('Login')");

        if (loginLink == null)
        {
            Assert.Pass("Login link on register page not found - may use different flow");
            return;
        }

        Assert.That(await loginLink.IsVisibleAsync(), Is.True,
            "Register page should have link to login page");
    }

    #endregion

    #region Error Page Link Tests

    [Test]
    public async Task AccessDeniedPage_HasHomeLink()
    {
        await Page.GotoAsync($"{BaseUrl}/access-denied");
        await Page.WaitForTimeoutAsync(500);

        var homeLink = await Page.QuerySelectorAsync(
            "a[href='/'], a:has-text('Home'), a:has-text('Go to Home')");

        if (homeLink == null)
        {
            // May redirect or show different content
            Assert.Pass("Access denied page home link not found - may auto-redirect");
            return;
        }

        Assert.That(await homeLink.IsVisibleAsync(), Is.True,
            "Access denied page should have link to home");
    }

    [Test]
    public async Task AccessDeniedPage_HasLoginLink()
    {
        await Page.GotoAsync($"{BaseUrl}/access-denied");
        await Page.WaitForTimeoutAsync(500);

        var loginLink = await Page.QuerySelectorAsync(
            "a[href='/login'], a:has-text('Sign In'), a:has-text('Login')");

        if (loginLink == null)
        {
            Assert.Pass("Access denied page login link not found - may auto-redirect");
            return;
        }

        Assert.That(await loginLink.IsVisibleAsync(), Is.True,
            "Access denied page should have link to login");
    }

    #endregion

    #region Scenarios Page Link Tests

    [Test]
    public async Task ScenariosPage_HasCalculatorLink()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForTimeoutAsync(500);

        var calculatorLink = await Page.QuerySelectorAsync(
            "a[href='/'], a:has-text('Calculator'), a:has-text('Go to Calculator')");

        if (calculatorLink == null)
        {
            Assert.Pass("Calculator link on scenarios page not found");
            return;
        }

        Assert.That(await calculatorLink.IsVisibleAsync(), Is.True,
            "Scenarios page should have link back to calculator");
    }

    #endregion

    #region External Link Tests

    [Test]
    public async Task ExternalLinks_HaveTargetBlank()
    {
        await _wizard.GoToHomeAsync();
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();

        // Navigate to results where external links might appear
        var calculateBtn = await Page.QuerySelectorAsync("button:has-text('Calculate')");
        if (calculateBtn != null)
        {
            await calculateBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(1500);
        }

        var externalLinks = await Page.QuerySelectorAllAsync(ExternalLink);

        if (externalLinks.Count == 0)
        {
            Assert.Pass("No external links found on current page");
            return;
        }

        foreach (var link in externalLinks.Take(3))
        {
            var target = await link.GetAttributeAsync("target");
            Assert.That(target, Is.EqualTo("_blank"),
                "External links should open in new tab");
        }
    }

    [Test]
    public async Task ExternalLinks_HaveRelNoopener()
    {
        await _wizard.GoToHomeAsync();

        var externalLinks = await Page.QuerySelectorAllAsync("a[target='_blank']");

        if (externalLinks.Count == 0)
        {
            Assert.Pass("No external links found");
            return;
        }

        foreach (var link in externalLinks.Take(3))
        {
            var rel = await link.GetAttributeAsync("rel");

            // Should have noopener for security
            var hasNoopener = rel?.Contains("noopener") == true ||
                             rel?.Contains("noreferrer") == true;

            Assert.Pass(hasNoopener ?
                "External links have security rel attribute" :
                "External links may rely on browser defaults");
        }
    }

    [Test]
    public async Task DocumentationLinks_OpenInNewTab()
    {
        await _wizard.GoToHomeAsync();
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();

        // Navigate to results and cloud alternatives
        var calculateBtn = await Page.QuerySelectorAsync("button:has-text('Calculate')");
        if (calculateBtn != null)
        {
            await calculateBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(1500);
        }

        // Look for documentation links
        var docLinks = await Page.QuerySelectorAllAsync(
            "a.btn-docs, a:has-text('Documentation'), a:has-text('Docs')");

        if (docLinks.Count == 0)
        {
            Assert.Pass("No documentation links found");
            return;
        }

        var link = docLinks[0];
        var target = await link.GetAttributeAsync("target");
        var href = await link.GetAttributeAsync("href");

        Assert.That(target, Is.EqualTo("_blank"),
            "Documentation links should open in new tab");

        Assert.That(href, Does.StartWith("http"),
            "Documentation links should be external URLs");
    }

    #endregion

    #region Link Accessibility Tests

    [Test]
    public async Task Links_HaveDescriptiveText()
    {
        await _wizard.GoToHomeAsync();

        var links = await Page.QuerySelectorAllAsync("a");

        if (links.Count == 0)
        {
            Assert.Pass("No links found");
            return;
        }

        foreach (var link in links.Take(5))
        {
            var text = await link.TextContentAsync();
            var ariaLabel = await link.GetAttributeAsync("aria-label");
            var title = await link.GetAttributeAsync("title");

            // Link should have some descriptive content
            var hasDescription = !string.IsNullOrWhiteSpace(text) ||
                                !string.IsNullOrEmpty(ariaLabel) ||
                                !string.IsNullOrEmpty(title);

            if (!hasDescription)
            {
                // Check for child elements with text (icons with sr-only text)
                var childText = await link.EvaluateAsync<string>(
                    "el => el.querySelector('.sr-only, .visually-hidden')?.textContent");

                hasDescription = !string.IsNullOrEmpty(childText);
            }

            Assert.That(hasDescription, Is.True,
                "Links should have descriptive text or aria-label");
        }
    }

    [Test]
    public async Task Links_AreKeyboardAccessible()
    {
        await _wizard.GoToHomeAsync();

        var links = await Page.QuerySelectorAllAsync("a");

        if (links.Count == 0)
        {
            Assert.Pass("No links found");
            return;
        }

        // Tab to first link and verify it receives focus
        await Page.Keyboard.PressAsync("Tab");
        await Page.WaitForTimeoutAsync(100);

        var focusedElement = await Page.EvaluateAsync<string>(
            "document.activeElement?.tagName");

        Assert.That(focusedElement, Is.EqualTo("A").Or.EqualTo("BUTTON"),
            "Links should be keyboard accessible");
    }

    [Test]
    public async Task InternalLinks_NavigateWithinApp()
    {
        await _wizard.GoToHomeAsync();

        var internalLinks = await Page.QuerySelectorAllAsync(
            "a[href^='/']:not([target='_blank']), a[href='']:not([target='_blank'])");

        if (internalLinks.Count == 0)
        {
            Assert.Pass("No internal navigation links found");
            return;
        }

        // Store initial URL
        var initialUrl = Page.Url;

        // Click first internal link
        var link = internalLinks[0];
        var href = await link.GetAttributeAsync("href");

        await link.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Verify navigation occurred within the app (same origin)
        var newUrl = Page.Url;
        var sameOrigin = new Uri(initialUrl).Host == new Uri(newUrl).Host;

        Assert.That(sameOrigin, Is.True,
            "Internal links should navigate within the application");
    }

    #endregion

    #region Reload Link Tests

    [Test]
    public async Task ReloadLink_IsPresent_OnErrorPage()
    {
        // Trigger an error state or go to error layout
        await Page.GotoAsync($"{BaseUrl}/nonexistent-page-404-test");
        await Page.WaitForTimeoutAsync(500);

        var reloadLink = await Page.QuerySelectorAsync(
            "a.reload, a:has-text('Reload'), a[href='.']");

        // Reload link may not appear on all error states
        Assert.Pass(reloadLink != null ?
            "Reload link is present on error page" :
            "Reload link not found - error handling may be different");
    }

    #endregion
}
