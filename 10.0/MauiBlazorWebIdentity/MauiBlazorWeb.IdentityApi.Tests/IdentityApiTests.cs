using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MauiBlazorWeb.Web.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MauiBlazorWeb.IdentityApi.Tests;

[TestClass]
public sealed class IdentityApiTests
{
    [TestMethod]
    public async Task Stock_override_and_new_routes_are_distinct()
    {
        await using var factory = new IdentityApiFactory();
        using var client = factory.CreateInitializedClient();

        var document = await client.GetStringAsync("/openapi/v1.json");

        StringAssert.Contains(document, "\"/identity/login\"");
        StringAssert.Contains(document, "\"/identity-overrides/login\"");
        StringAssert.Contains(document, "\"/identity/manage/passkeys\"");
    }

    [TestMethod]
    public async Task Override_login_returns_stable_invalid_credentials_code()
    {
        await using var factory = new IdentityApiFactory();
        using var client = factory.CreateInitializedClient();

        var response = await client.PostAsJsonAsync("/identity-overrides/login", new
        {
            email = "missing@example.test",
            password = "Password1!",
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual("invalid_credentials", body.RootElement.GetProperty("code").GetString());
    }

    [TestMethod]
    public async Task Native_routes_do_not_accept_the_application_cookie()
    {
        await using var factory = new IdentityApiFactory();
        using var client = factory.CreateInitializedClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

        var response = await client.GetAsync("/identity/manage/passkeys");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Passkey_login_begin_writes_temporary_identity_cookie()
    {
        await using var factory = new IdentityApiFactory();
        using var client = factory.CreateInitializedClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
        });

        var response = await client.PostAsync("/identity/passkeys/login/begin", null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        StringAssert.Contains(string.Join(Environment.NewLine, cookies), "Identity.TwoFactorUserId");
    }

    [TestMethod]
    public async Task Development_notifications_are_not_mapped_in_production()
    {
        await using var factory = new IdentityApiFactory(Environments.Production);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/development/notifications");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class IdentityApiFactory(string environment = "Testing")
        : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"identity-api-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                }));
        }

        public HttpClient CreateInitializedClient(WebApplicationFactoryClientOptions? options = null)
        {
            var client = options is null ? CreateClient() : CreateClient(options);
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            return client;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
    }
}
