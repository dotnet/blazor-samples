using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MauiBlazorWeb.WebUi.Tests;

[TestClass]
public sealed class HostedPageTests
{
    [TestMethod]
    [TestCategory("HostedUi")]
    public async Task Account_pages_render_their_expected_forms()
    {
        await using var environment = await HostedPageEnvironment.CreateAsync();

        await environment.Page.GotoAsync(environment.Url("/Account/Register"));
        await Expect(environment.Page.Locator("h1")).ToHaveTextAsync("Register");
        await Expect(environment.Page.Locator("form")).ToBeVisibleAsync();
        await Expect(environment.Page.Locator("input[autocomplete='username']")).ToBeVisibleAsync();

        await environment.Page.GotoAsync(environment.Url("/Account/Login"));
        await Expect(environment.Page.Locator("h1")).ToHaveTextAsync("Log in");
        await Expect(environment.Page.Locator("form")).ToBeVisibleAsync();
        await Expect(environment.Page.Locator("input[autocomplete='current-password']")).ToBeVisibleAsync();
    }

    [TestMethod]
    [TestCategory("HostedUi")]
    public async Task Register_confirm_and_login_complete_through_hosted_pages()
    {
        await using var environment = await HostedPageEnvironment.CreateAsync();
        var email = $"browser-{Guid.NewGuid():N}@example.test";
        const string password = "Browser!Password42";

        await environment.Page.GotoAsync(environment.Url("/Account/Register"));
        await environment.Page.Locator("#Input\\.Email").FillAsync(email);
        await environment.Page.Locator("#Input\\.Password").FillAsync(password);
        await environment.Page.Locator("#Input\\.ConfirmPassword").FillAsync(password);
        await environment.Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(environment.Page.Locator("h1")).ToHaveTextAsync("Register confirmation");

        await environment.ConfirmNotificationAsync("Confirm email", email);
        await environment.Page.GotoAsync(environment.Url("/Account/Login"));
        await environment.Page.Locator("#Input\\.Email").FillAsync(email);
        await environment.Page.Locator("#Input\\.Password").FillAsync(password);
        await environment.Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        await environment.Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
    }

    [TestMethod]
    [TestCategory("HostedUi")]
    public async Task Forgot_and_reset_password_complete_through_hosted_pages()
    {
        await using var environment = await HostedPageEnvironment.CreateAsync();
        var email = $"browser-{Guid.NewGuid():N}@example.test";
        const string originalPassword = "Browser!Password42";
        const string resetPassword = "Browser!ResetPassword42";

        await environment.RegisterAndConfirmAsync(email, originalPassword);
        await environment.Page.GotoAsync(environment.Url("/Account/ForgotPassword"));
        await environment.Page.Locator("#Input\\.Email").FillAsync(email);
        await environment.Page.GetByRole(AriaRole.Button, new() { Name = "Reset password", Exact = true }).ClickAsync();
        await Expect(environment.Page.Locator("h1")).ToHaveTextAsync("Forgot password confirmation");

        await environment.OpenNotificationAsync("Reset password", email);
        await environment.Page.Locator("#Input\\.Email").FillAsync(email);
        await environment.Page.Locator("#Input\\.Password").FillAsync(resetPassword);
        await environment.Page.Locator("#Input\\.ConfirmPassword").FillAsync(resetPassword);
        await environment.Page.GetByRole(AriaRole.Button, new() { Name = "Reset", Exact = true }).ClickAsync();
        await Expect(environment.Page.Locator("h1")).ToHaveTextAsync("Reset password confirmation");

        await environment.Page.GotoAsync(environment.Url("/Account/Login"));
        await environment.Page.Locator("#Input\\.Email").FillAsync(email);
        await environment.Page.Locator("#Input\\.Password").FillAsync(resetPassword);
        await environment.Page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        await environment.Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
    }
}

internal sealed class HostedPageEnvironment : IAsyncDisposable
{
    private HostedPageEnvironment(Uri serverUrl, IPlaywright playwright, IBrowser browser, IPage page)
    {
        ServerUrl = serverUrl;
        Playwright = playwright;
        Browser = browser;
        Page = page;
    }

    private Uri ServerUrl { get; }
    private IPlaywright Playwright { get; }
    private IBrowser Browser { get; }
    internal IPage Page { get; }

    internal static async Task<HostedPageEnvironment> CreateAsync()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("WEB_UI_TESTS"), "1", StringComparison.Ordinal))
        {
            Assert.Inconclusive("Set WEB_UI_TESTS=1 to opt into hosted browser tests.");
        }

        if (!Uri.TryCreate(Environment.GetEnvironmentVariable("DEVFLOW_SERVER_URL"), UriKind.Absolute, out var serverUrl))
        {
            Assert.Inconclusive("Set DEVFLOW_SERVER_URL to the reachable Development server URL.");
        }

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions { IgnoreHTTPSErrors = serverUrl.IsLoopback });
        return new HostedPageEnvironment(serverUrl, playwright, browser, page);
    }

    internal string Url(string path) => new Uri(ServerUrl, path).AbsoluteUri;

    internal async Task RegisterAndConfirmAsync(string email, string password)
    {
        await Page.GotoAsync(Url("/Account/Register"));
        await Page.Locator("#Input\\.Email").FillAsync(email);
        await Page.Locator("#Input\\.Password").FillAsync(password);
        await Page.Locator("#Input\\.ConfirmPassword").FillAsync(password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Register confirmation");
        await ConfirmNotificationAsync("Confirm email", email);
    }

    internal Task ConfirmNotificationAsync(string kind, string email) => OpenNotificationAsync(kind, email);

    internal async Task OpenNotificationAsync(string kind, string email)
    {
        await Page.GotoAsync(Url("/development/notifications"));
        var action = Page.Locator($"li:has-text('{kind}'):has-text('{email}') a");
        await Expect(action).ToBeVisibleAsync();
        var href = await action.GetAttributeAsync("href");
        Assert.IsFalse(string.IsNullOrWhiteSpace(href), "The Development notification did not contain an action link.");
        await Page.GotoAsync(new Uri(ServerUrl, href).AbsoluteUri);
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}
