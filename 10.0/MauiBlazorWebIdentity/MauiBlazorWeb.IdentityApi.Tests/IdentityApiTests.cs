using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MauiBlazorWeb.Web.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public async Task Passkey_login_finish_without_the_begin_cookie_returns_a_stable_ceremony_failure()
    {
        await using var factory = new IdentityApiFactory();
        using var client = factory.CreateInitializedClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
        });

        var response = await client.PostAsJsonAsync("/identity/passkeys/login/finish", new { });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual("passkey_ceremony_not_found", body.RootElement.GetProperty("code").GetString());
    }

    [TestMethod]
    public async Task Development_notifications_are_not_mapped_in_production()
    {
        await using var factory = new IdentityApiFactory(Environments.Production);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/development/notifications");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Override_account_update_requires_exactly_one_operation_and_confirms_the_current_password()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        var ambiguous = await authenticated.Client.PostAsJsonAsync("/identity-overrides/manage/info", new
        {
            phoneNumber = "+15551234567",
            newPassword = "ChangedPassword1!",
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, ambiguous.StatusCode);
        await AssertValidationErrorAsync(ambiguous, "AmbiguousOperation");

        var missingCurrentPassword = await authenticated.Client.PostAsJsonAsync("/identity-overrides/manage/info", new
        {
            newPassword = "ChangedPassword1!",
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, missingCurrentPassword.StatusCode);
        await AssertValidationErrorAsync(missingCurrentPassword, "OldPasswordRequired");

        var wrongCurrentPassword = await authenticated.Client.PostAsJsonAsync("/identity-overrides/manage/info", new
        {
            oldPassword = "WrongPassword1!",
            newPassword = "ChangedPassword1!",
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, wrongCurrentPassword.StatusCode);

        var changed = await authenticated.Client.PostAsJsonAsync("/identity-overrides/manage/info", new
        {
            oldPassword = authenticated.Password,
            newPassword = "ChangedPassword1!",
        });
        Assert.AreEqual(HttpStatusCode.OK, changed.StatusCode);

        var login = await authenticated.Client.PostAsJsonAsync("/identity-overrides/login", new
        {
            email = authenticated.Email,
            password = "ChangedPassword1!",
        });
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
    }

    [TestMethod]
    public async Task Logout_all_invalidates_refresh_tokens_but_not_the_current_access_token()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        var beforeLogoutAll = await authenticated.Client.GetAsync("/identity/manage/personal-data");
        Assert.AreEqual(HttpStatusCode.OK, beforeLogoutAll.StatusCode);

        var logoutAll = await authenticated.Client.PostAsync("/identity/manage/logout-all", null);
        Assert.AreEqual(HttpStatusCode.NoContent, logoutAll.StatusCode);

        var currentAccessToken = await authenticated.Client.GetAsync("/identity/manage/personal-data");
        Assert.AreEqual(HttpStatusCode.OK, currentAccessToken.StatusCode);

        var refresh = await authenticated.Client.PostAsJsonAsync("/identity/refresh", new
        {
            refreshToken = authenticated.RefreshToken,
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, refresh.StatusCode);
    }

    [TestMethod]
    public async Task Account_deletion_requires_the_current_password_and_deletes_only_after_confirmation()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        var rejected = await authenticated.Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/identity/manage/account")
        {
            Content = JsonContent.Create(new { currentPassword = "WrongPassword1!" }),
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode);
        await AssertValidationErrorAsync(rejected, "InvalidCurrentPassword");
        Assert.IsNotNull(await factory.FindByEmailAsync(authenticated.Email));

        var deleted = await authenticated.Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/identity/manage/account")
        {
            Content = JsonContent.Create(new { currentPassword = authenticated.Password }),
        });
        Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.IsNull(await factory.FindByEmailAsync(authenticated.Email));
    }

    [TestMethod]
    public async Task External_login_unlink_cannot_remove_the_last_sign_in_method()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        await factory.MakeExternalLoginTheOnlySignInMethodAsync(authenticated.Email);

        var rejected = await authenticated.Client.DeleteAsync("/identity/manage/external-logins/GitHub");
        Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode);
        await AssertValidationErrorAsync(rejected, "LastSignInMethod");

        var logins = await authenticated.Client.GetFromJsonAsync<JsonElement[]>("/identity/manage/external-logins");
        Assert.IsNotNull(logins);
        Assert.AreEqual(1, logins.Length);
        Assert.AreEqual("GitHub", logins[0].GetProperty("provider").GetString());
    }

    private static async Task AssertValidationErrorAsync(HttpResponseMessage response, string errorCode)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(body.RootElement.GetProperty("errors").TryGetProperty(errorCode, out _),
            $"Expected validation error '{errorCode}'.");
    }

    private sealed class IdentityApiFactory(string environment = "Testing")
        : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Directory.GetCurrentDirectory(), $".identity-api-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={_databasePath}"));
            });
        }

        public HttpClient CreateInitializedClient(WebApplicationFactoryClientOptions? options = null)
        {
            var client = options is null ? CreateClient() : CreateClient(options);
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            return client;
        }

        public async Task<AuthenticatedIdentityClient> CreateAuthenticatedClientAsync()
        {
            var email = $"user-{Guid.NewGuid():N}@example.test";
            const string password = "Password1!";
            var client = CreateInitializedClient();
            using (var scope = Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var result = await userManager.CreateAsync(new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                }, password);
                Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Code)));
            }

            var response = await client.PostAsJsonAsync("/identity-overrides/login", new
            {
                email,
                password,
            });
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
            var accessToken = body.RootElement.GetProperty("accessToken").GetString();
            var refreshToken = body.RootElement.GetProperty("refreshToken").GetString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(accessToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(refreshToken));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return new AuthenticatedIdentityClient(client, email, password, refreshToken!);
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            using var scope = Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        }

        public async Task MakeExternalLoginTheOnlySignInMethodAsync(string email)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.IsNotNull(user);
            Assert.IsTrue((await userManager.AddLoginAsync(user, new UserLoginInfo("GitHub", "github-user", "GitHub"))).Succeeded);
            Assert.IsTrue((await userManager.RemovePasswordAsync(user)).Succeeded);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }

        public sealed class AuthenticatedIdentityClient(HttpClient client, string email, string password, string refreshToken)
            : IDisposable
        {
            public HttpClient Client { get; } = client;

            public string Email { get; } = email;

            public string Password { get; } = password;

            public string RefreshToken { get; } = refreshToken;

            public void Dispose() => Client.Dispose();
        }
    }
}
