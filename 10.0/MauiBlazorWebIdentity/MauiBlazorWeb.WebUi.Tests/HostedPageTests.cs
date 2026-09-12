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
        if (!string.Equals(Environment.GetEnvironmentVariable("WEB_UI_TESTS"), "1", StringComparison.Ordinal))
        {
            Assert.Inconclusive("Set WEB_UI_TESTS=1 to opt into hosted browser tests.");
        }

        if (!Uri.TryCreate(Environment.GetEnvironmentVariable("DEVFLOW_SERVER_URL"), UriKind.Absolute, out var serverUrl))
        {
            Assert.Inconclusive("Set DEVFLOW_SERVER_URL to the reachable Development server URL.");
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions { IgnoreHTTPSErrors = serverUrl.IsLoopback });

        await page.GotoAsync(new Uri(serverUrl, "/Account/Register").AbsoluteUri);
        await Expect(page.Locator("h1")).ToHaveTextAsync("Register");
        await Expect(page.Locator("form")).ToBeVisibleAsync();
        await Expect(page.Locator("input[autocomplete='username']")).ToBeVisibleAsync();

        await page.GotoAsync(new Uri(serverUrl, "/Account/Login").AbsoluteUri);
        await Expect(page.Locator("h1")).ToHaveTextAsync("Log in");
        await Expect(page.Locator("form")).ToBeVisibleAsync();
        await Expect(page.Locator("input[autocomplete='current-password']")).ToBeVisibleAsync();
    }
}
