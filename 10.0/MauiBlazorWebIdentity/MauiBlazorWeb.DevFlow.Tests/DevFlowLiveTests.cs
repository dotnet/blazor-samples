using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Maui.DevFlow.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MauiBlazorWeb.DevFlow.Tests;

[TestClass]
public sealed class DevFlowLiveTests
{
    [TestMethod]
    [TestCategory("Live")]
    public async Task Server_agent_and_cdp_are_ready()
    {
        using var environment = await LiveEnvironment.RequireServerAndDevFlowAsync();

        await environment.DevFlow.RequireSuccessAsync("agent status", "agent", "status");
        await environment.DevFlow.RequireSuccessAsync("CDP WebView list", "webview", "webviews");
        await environment.DevFlow.RequireSuccessAsync("CDP status", "webview", "status");
        await environment.DevFlow.RequireSuccessAsync("CDP source", "webview", "source");
    }

    [TestMethod]
    [TestCategory("Live")]
    public async Task Incorrect_login_displays_a_failure_through_the_Maui_DOM()
    {
        using var environment = await LiveEnvironment.RequireServerAndDevFlowAsync();
        try
        {
            await environment.DevFlow.ClickAsync("[data-test='nav-login']");
            await environment.DevFlow.WaitForSelectorAsync("[data-test='login-email']");
            await environment.DevFlow.FillAsync("[data-test='login-email']", $"missing-{Guid.NewGuid():N}@example.test");
            await environment.DevFlow.FillAsync("[data-test='login-password']", "incorrect-password");
            await environment.DevFlow.ClickAsync("[data-test='login-submit']");
            await environment.DevFlow.WaitForConditionAsync(
                "document.querySelector(\"[data-test='login-failure']\")?.hidden === false",
                "invalid-login alert");
        }
        finally
        {
            await environment.DevFlow.ReleaseMutationLeaseAsync();
        }
    }

    [TestMethod]
    [TestCategory("Live")]
    public async Task Register_confirm_login_update_phone_and_logout_through_the_Maui_DOM()
    {
        using var environment = await LiveEnvironment.RequireServerAndDevFlowAsync();
        try
        {
            var email = $"devflow-{Guid.NewGuid():N}@example.test";
            const string password = "DevFlow!Test-Password42";
            var phone = $"555{Random.Shared.Next(1000000, 9999999)}";

            await environment.DevFlow.ClickAsync("[data-test='nav-register']");
            await environment.DevFlow.WaitForSelectorAsync("[data-test='register-email']");
            await environment.DevFlow.FillAsync("[data-test='register-email']", email);
            await environment.DevFlow.FillAsync("[data-test='register-password']", password);
            await environment.DevFlow.ClickAsync("[data-test='register-submit']");
            await environment.DevFlow.WaitForConditionAsync(
                "document.querySelector(\"[data-test='registration-result']\")?.classList.contains('alert-success') === true",
                "registration success");

            var confirmationUrl = await environment.GetConfirmationUrlAsync(email);
            using var confirmation = await environment.HttpClient.GetAsync(confirmationUrl);
            Assert.IsTrue(
                confirmation.IsSuccessStatusCode,
                $"The development confirmation action returned {(int)confirmation.StatusCode} ({confirmation.StatusCode}).");
            Assert.IsTrue(
                await environment.CanLogInAsync(email, password),
                "The confirmed account was not accepted by the server login endpoint.");

            await environment.DevFlow.ClickAsync("[data-test='nav-login']");
            await environment.DevFlow.WaitForSelectorAsync("[data-test='login-email']");
            await environment.DevFlow.FillAsync("[data-test='login-email']", email);
            await environment.DevFlow.FillAsync("[data-test='login-password']", password);
            await environment.DevFlow.WaitForConditionAsync(
                $"document.querySelector(\"[data-test='login-password']\")?.value === '{password}'",
                "replacement login password");
            await environment.DevFlow.ClickAsync("[data-test='login-submit']");
            await environment.DevFlow.WaitForSelectorAsync("[data-test='nav-account']");
            await environment.DevFlow.ClickAsync("[data-test='nav-account']");
            await environment.DevFlow.WaitForConditionAsync(
                $"document.querySelector(\"[data-test='account-email']\")?.textContent.includes('{email}') === true",
                "confirmed account email");

            await environment.DevFlow.FillAsync("[data-test='phone-input']", phone);
            await environment.DevFlow.ClickAsync("[data-test='phone-save']");
            await environment.DevFlow.WaitForConditionAsync(
                "document.querySelector(\"[data-test='account-result']\")?.classList.contains('alert-success') === true",
                "phone-save success");
            await environment.DevFlow.WaitForConditionAsync(
                $"document.querySelector(\"[data-test='phone-input']\")?.value === '{phone}'",
                "phone round trip");

            await environment.DevFlow.ClickAsync("[data-test='nav-logout']");
            await environment.DevFlow.WaitForSelectorAsync("[data-test='nav-login']");
            await environment.DevFlow.WaitForConditionAsync(
                "document.querySelector(\"[data-test='nav-account']\") === null",
                "anonymous navigation state");
        }
        finally
        {
            await environment.DevFlow.ReleaseMutationLeaseAsync();
        }
    }

}

internal sealed class LiveEnvironment : IDisposable
{
    private const int DefaultAgentPort = 10223;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    private LiveEnvironment(Uri serverUrl, int agentPort)
    {
        ServerUrl = serverUrl;
        HttpClient = CreateHttpClient();
        DevFlow = new DevFlowClient(agentPort);
    }

    internal Uri ServerUrl { get; }
    internal HttpClient HttpClient { get; }
    internal DevFlowClient DevFlow { get; }

    internal static async Task<LiveEnvironment> RequireServerAndDevFlowAsync()
    {
        var environment = Create();
        try
        {
            using var response = await environment.HttpClient.GetAsync(new Uri(environment.ServerUrl, "/health"));
            Assert.IsTrue(response.IsSuccessStatusCode, "DEVFLOW_SERVER_URL is reachable but did not return a successful response.");
            await environment.DevFlow.WaitForAgentAsync();
            await environment.DevFlow.WaitForCdpAsync();
            return environment;
        }
        catch (HttpRequestException)
        {
            environment.HttpClient.Dispose();
            Assert.Inconclusive("DEVFLOW_SERVER_URL is not reachable. Start the Development server before running live tests.");
            throw;
        }
    }

    internal async Task<Uri> GetConfirmationUrlAsync(string email)
    {
        using var response = await HttpClient.GetAsync(new Uri(ServerUrl, "/development/notifications"));
        Assert.IsTrue(response.IsSuccessStatusCode, "The Development notification endpoint is unavailable.");
        var notifications = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            notifications,
            $"Confirm email.*?for\\s+{Regex.Escape(email)}.*?href=\\\"(?<href>[^\\\"]+)\\\"",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);
        Assert.IsTrue(match.Success, "No confirmation notification was found for the generated test account.");
        return new Uri(ServerUrl, WebUtility.HtmlDecode(match.Groups["href"].Value));
    }

    internal async Task<bool> CanLogInAsync(string email, string password)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            new Uri(ServerUrl, "/identity-overrides/login"),
            new { email, password });
        return response.IsSuccessStatusCode;
    }

    private static LiveEnvironment Create()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DEVFLOW_LIVE_TESTS"), "1", StringComparison.Ordinal))
        {
            Assert.Inconclusive("Set DEVFLOW_LIVE_TESTS=1 to opt into tests that control a live Development app.");
        }

        var serverValue = Environment.GetEnvironmentVariable("DEVFLOW_SERVER_URL");
        if (!Uri.TryCreate(serverValue, UriKind.Absolute, out var serverUrl) ||
            (serverUrl.Scheme != Uri.UriSchemeHttp && serverUrl.Scheme != Uri.UriSchemeHttps))
        {
            Assert.Inconclusive("Set DEVFLOW_SERVER_URL to the reachable Development server URL.");
        }

        var agentPortValue = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
        if (!int.TryParse(agentPortValue, out var agentPort) || agentPort is < 1 or > 65535)
        {
            Assert.Inconclusive($"Set DEVFLOW_AGENT_PORT to the reachable DevFlow agent port (for example, {DefaultAgentPort}).");
        }

        return new LiveEnvironment(serverUrl!, agentPort);
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = static (request, _, _, _) =>
                request.RequestUri?.IsLoopback == true
        };
        return new HttpClient(handler) { Timeout = RequestTimeout };
    }

    public void Dispose()
    {
        DevFlow.Dispose();
        HttpClient.Dispose();
    }
}

internal sealed class DevFlowClient : IDisposable
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(30);
    private readonly AgentClient client;

    internal DevFlowClient(int agentPort)
    {
        client = new AgentClient("127.0.0.1", agentPort)
        {
            MutationLeaseHolderKind = "mstest",
            MutationLeaseLabel = "MauiBlazorWeb.DevFlow.Tests",
        };
    }

    internal async Task FillAsync(string selector, string text)
    {
        Assert.IsTrue(await client.FillWebViewAsync(selector, text), "DevFlow could not fill a DOM form field.");
        Assert.IsTrue(
            await EvaluateBooleanAsync(
                $"(() => {{ const element = document.querySelector({JsonSerializer.Serialize(selector)}); if (!element) return false; element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true; }})()"),
            "DevFlow could not commit a DOM form field change.");
    }

    internal async Task ClickAsync(string selector)
    {
        if (await client.ClickWebViewAsync(selector))
        {
            return;
        }

        var selectorLiteral = JsonSerializer.Serialize(selector);
        await EvaluateBooleanAsync(
            $"(() => {{ const element = document.querySelector({selectorLiteral}); if (!element) return false; element.click(); return true; }})()");
    }

    internal async Task WaitForSelectorAsync(string selector)
    {
        await WaitForConditionAsync($"document.querySelector(\"{selector}\") !== null", $"selector {selector}");
    }

    internal async Task WaitForConditionAsync(string expression, string description)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            while (!timeout.IsCancellationRequested)
            {
                if (await EvaluateBooleanAsync(expression))
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), timeout.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        Assert.Fail($"Timed out waiting for {description} in the Blazor WebView.");
    }

    internal async Task RequireSuccessAsync(string operation, params string[] command)
    {
        if (command is ["agent", "status"])
        {
            Assert.IsNotNull(await client.GetStatusAsync(), $"{operation} did not return an agent status.");
            return;
        }

        if (command is ["webview", "webviews"])
        {
            Assert.IsTrue((await client.GetCdpWebViewsAsync()).ToString().Contains("IdentityBlazorWebView", StringComparison.Ordinal),
                $"{operation} did not return the registered WebView.");
            return;
        }

        if (command is ["webview", "status"])
        {
            await WaitForCdpAsync();
            return;
        }

        if (command is ["webview", "source"])
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(await client.GetCdpSourceAsync()), $"{operation} returned no document.");
            return;
        }

        Assert.Fail($"Unsupported DevFlow readiness command: {string.Join(' ', command)}.");
    }

    internal async Task WaitForAgentAsync()
    {
        try
        {
            Assert.IsNotNull(await client.GetStatusAsync(), "The DevFlow agent did not return a status.");
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Assert.Inconclusive("The DevFlow agent is unavailable. Launch the Debug Mac Catalyst app and wait for its agent.");
        }
    }

    internal async Task WaitForCdpAsync()
    {
        using var timeout = new CancellationTokenSource(CommandTimeout);
        try
        {
            while (!timeout.IsCancellationRequested)
            {
                var webViews = await client.GetCdpWebViewsAsync();
                if (webViews.TryGetProperty("webviews", out var items) &&
                    items.ValueKind == JsonValueKind.Array &&
                    items.EnumerateArray().Any(item =>
                        item.TryGetProperty("ready", out var ready) &&
                        ready.ValueKind == JsonValueKind.True))
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), timeout.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        Assert.Fail("The DevFlow agent connected, but its Blazor CDP bridge did not become ready.");
    }

    internal async Task ReleaseMutationLeaseAsync()
    {
        var result = await client.ControlMutationLeaseAsync("release");
        Assert.IsTrue(result.Ok || string.Equals(result.Authority, "unsupported", StringComparison.Ordinal),
            "The live test could not release its DevFlow mutation lease.");
    }

    private async Task<bool> EvaluateBooleanAsync(string expression)
    {
        var result = await client.SendCdpCommandAsync(
            "Runtime.evaluate",
            new JsonObject
            {
                ["expression"] = expression,
                ["returnByValue"] = true,
            });

        return result.TryGetProperty("result", out var outer) &&
            outer.TryGetProperty("result", out var inner) &&
            inner.TryGetProperty("value", out var value) &&
            value.ValueKind is JsonValueKind.True;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
