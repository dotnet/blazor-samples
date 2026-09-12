using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MauiBlazorWeb.Web.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.WebUtilities;
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
    public async Task Development_notification_confirms_a_stock_registration()
    {
        await using var factory = new IdentityApiFactory(Environments.Development);
        using var client = factory.CreateInitializedClient();
        var email = $"development-{Guid.NewGuid():N}@example.test";
        const string password = "Password1!";

        var registration = await client.PostAsJsonAsync("/identity/register", new { email, password });
        Assert.AreEqual(HttpStatusCode.OK, registration.StatusCode);

        var confirmationUrl = await factory.GetDevelopmentNotificationActionAsync(client, "Confirm email", email);
        using var confirmation = await client.GetAsync(confirmationUrl);
        Assert.AreEqual(HttpStatusCode.OK, confirmation.StatusCode);

        var login = await client.PostAsJsonAsync("/identity/login?useCookies=false", new { email, password });
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
    }

    [TestMethod]
    public async Task Stock_resend_forgot_reset_login_refresh_and_manage_routes_have_happy_paths()
    {
        await using var factory = new IdentityApiFactory(Environments.Development);
        using var client = factory.CreateInitializedClient();
        var unconfirmedEmail = $"unconfirmed-{Guid.NewGuid():N}@example.test";
        const string originalPassword = "Password1!";
        const string resetPassword = "ResetPassword1!";
        await factory.CreateUserAsync(unconfirmedEmail, originalPassword, emailConfirmed: false);

        var resend = await client.PostAsJsonAsync("/identity/resendConfirmationEmail", new { email = unconfirmedEmail });
        Assert.AreEqual(HttpStatusCode.OK, resend.StatusCode);
        _ = await factory.GetDevelopmentNotificationActionAsync(client, "Confirm email", unconfirmedEmail);

        var authenticated = await factory.CreateAuthenticatedClientAsync();
        var forgot = await client.PostAsJsonAsync("/identity/forgotPassword", new { email = authenticated.Email });
        Assert.AreEqual(HttpStatusCode.OK, forgot.StatusCode);
        _ = await factory.GetDevelopmentNotificationActionAsync(client, "Reset password", authenticated.Email);

        var resetCode = await factory.GeneratePasswordResetCodeAsync(authenticated.Email);
        var reset = await client.PostAsJsonAsync("/identity/resetPassword", new
        {
            email = authenticated.Email,
            resetCode,
            newPassword = resetPassword,
        });
        Assert.AreEqual(HttpStatusCode.OK, reset.StatusCode);

        var login = await client.PostAsJsonAsync("/identity/login?useCookies=false", new
        {
            email = authenticated.Email,
            password = resetPassword,
        });
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
        using var loginBody = JsonDocument.Parse(await login.Content.ReadAsStreamAsync());
        var refreshToken = loginBody.RootElement.GetProperty("refreshToken").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(refreshToken));

        var refresh = await client.PostAsJsonAsync("/identity/refresh", new { refreshToken });
        Assert.AreEqual(HttpStatusCode.OK, refresh.StatusCode);

        using var stockManage = await authenticated.Client.GetAsync("/identity/manage/info");
        Assert.AreEqual(HttpStatusCode.OK, stockManage.StatusCode);
        using var stockManageBody = JsonDocument.Parse(await stockManage.Content.ReadAsStreamAsync());
        Assert.AreEqual(authenticated.Email, stockManageBody.RootElement.GetProperty("email").GetString());

        var stockManageUpdate = await authenticated.Client.PostAsJsonAsync("/identity/manage/info", new
        {
            oldPassword = resetPassword,
            newPassword = "ManagedPassword1!",
        });
        Assert.AreEqual(HttpStatusCode.OK, stockManageUpdate.StatusCode);

        var stockTwoFactor = await authenticated.Client.PostAsJsonAsync("/identity/manage/2fa", new { resetSharedKey = true });
        Assert.AreEqual(HttpStatusCode.OK, stockTwoFactor.StatusCode);
    }

    [TestMethod]
    public async Task Override_profile_and_two_factor_status_return_safe_extended_data()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        using var info = await authenticated.Client.GetAsync("/identity-overrides/manage/info");
        Assert.AreEqual(HttpStatusCode.OK, info.StatusCode);
        using var infoBody = JsonDocument.Parse(await info.Content.ReadAsStreamAsync());
        Assert.AreEqual(authenticated.Email, infoBody.RootElement.GetProperty("email").GetString());
        Assert.IsTrue(infoBody.RootElement.GetProperty("isEmailConfirmed").GetBoolean());
        Assert.IsFalse(infoBody.RootElement.TryGetProperty("accessToken", out _));

        var updated = await authenticated.Client.PostAsJsonAsync("/identity-overrides/manage/info", new { phoneNumber = "+15551234567" });
        Assert.AreEqual(HttpStatusCode.OK, updated.StatusCode);
        using var updatedBody = JsonDocument.Parse(await updated.Content.ReadAsStreamAsync());
        Assert.AreEqual("+15551234567", updatedBody.RootElement.GetProperty("phoneNumber").GetString());

        using var twoFactor = await authenticated.Client.GetAsync("/identity-overrides/manage/2fa");
        Assert.AreEqual(HttpStatusCode.OK, twoFactor.StatusCode);
        using var twoFactorBody = JsonDocument.Parse(await twoFactor.Content.ReadAsStreamAsync());
        Assert.IsFalse(twoFactorBody.RootElement.GetProperty("isTwoFactorEnabled").GetBoolean());
        Assert.IsFalse(twoFactorBody.RootElement.TryGetProperty("sharedKey", out _));
    }

    [TestMethod]
    public async Task Passkey_management_and_registration_validate_public_failure_paths()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        var list = await authenticated.Client.GetFromJsonAsync<JsonElement[]>("/identity/manage/passkeys");
        Assert.IsNotNull(list);
        Assert.AreEqual(0, list.Length);

        var malformedRename = await authenticated.Client.PatchAsJsonAsync("/identity/manage/passkeys/not-base64!", new { name = "Laptop" });
        Assert.AreEqual(HttpStatusCode.BadRequest, malformedRename.StatusCode);
        await AssertValidationErrorAsync(malformedRename, "credentialId");

        var malformedDelete = await authenticated.Client.DeleteAsync("/identity/manage/passkeys/not-base64!");
        Assert.AreEqual(HttpStatusCode.BadRequest, malformedDelete.StatusCode);
        await AssertValidationErrorAsync(malformedDelete, "credentialId");

        var begin = await authenticated.Client.PostAsync("/identity/passkeys/register/begin", null);
        Assert.AreEqual(HttpStatusCode.OK, begin.StatusCode);
        Assert.AreEqual("application/json", begin.Content.Headers.ContentType?.MediaType);

        var finish = await authenticated.Client.PostAsJsonAsync("/identity/passkeys/register/finish", new { });
        Assert.AreEqual(HttpStatusCode.BadRequest, finish.StatusCode);
        await AssertFailureCodeAsync(finish, "passkey_attestation_failed");
    }

    [TestMethod]
    public async Task Personal_data_and_external_login_management_exclude_sensitive_data()
    {
        await using var factory = new IdentityApiFactory();
        using var authenticated = await factory.CreateAuthenticatedClientAsync();

        using var personalData = await authenticated.Client.GetAsync("/identity/manage/personal-data");
        Assert.AreEqual(HttpStatusCode.OK, personalData.StatusCode);
        using var personalDataBody = JsonDocument.Parse(await personalData.Content.ReadAsStreamAsync());
        Assert.AreEqual(authenticated.Email, personalDataBody.RootElement.GetProperty("email").GetString());
        Assert.IsFalse(personalDataBody.RootElement.TryGetProperty("passwordHash", out _));
        Assert.IsFalse(personalDataBody.RootElement.TryGetProperty("authenticatorKey", out _));

        var emptyLogins = await authenticated.Client.GetFromJsonAsync<JsonElement[]>("/identity/manage/external-logins");
        Assert.IsNotNull(emptyLogins);
        Assert.AreEqual(0, emptyLogins.Length);

        await factory.AddExternalLoginAsync(authenticated.Email);
        var deleted = await authenticated.Client.DeleteAsync("/identity/manage/external-logins/GitHub");
        Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
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

    private static async Task AssertFailureCodeAsync(HttpResponseMessage response, string expectedCode)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(expectedCode, body.RootElement.GetProperty("code").GetString());
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
            await CreateUserAsync(email, password, emailConfirmed: true);

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

        public async Task CreateUserAsync(string email, string password, bool emailConfirmed)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var result = await userManager.CreateAsync(new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = emailConfirmed,
            }, password);
            Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Code)));
        }

        public async Task<string> GeneratePasswordResetCodeAsync(string email)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.IsNotNull(user);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        public async Task<string> GetDevelopmentNotificationActionAsync(HttpClient client, string kind, string email)
        {
            var page = await client.GetStringAsync("/development/notifications");
            var match = Regex.Match(
                page,
                $"{Regex.Escape(kind)}.*?for\\s+{Regex.Escape(email)}.*?href=\\\"(?<href>[^\\\"]+)\\\"",
                RegexOptions.Singleline | RegexOptions.CultureInvariant);
            Assert.IsTrue(match.Success, $"No {kind} notification was found for the test account.");
            return WebUtility.HtmlDecode(match.Groups["href"].Value);
        }

        public async Task AddExternalLoginAsync(string email)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.IsNotNull(user);
            Assert.IsTrue((await userManager.AddLoginAsync(user, new UserLoginInfo("GitHub", "github-user", "GitHub"))).Succeeded);
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
            if (disposing)
            {
                SqliteConnection.ClearAllPools();
                var directory = Path.GetDirectoryName(_databasePath)!;
                var fileName = Path.GetFileName(_databasePath);
                foreach (var path in Directory.GetFiles(directory, $"{fileName}*"))
                {
                    File.Delete(path);
                }
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
