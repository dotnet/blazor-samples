using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MauiBlazorWeb.IdentityApi;

/// <summary>
/// Maps proposed Identity API routes that are intentionally separate from the
/// framework's stock <c>MapIdentityApi</c> routes.
/// </summary>
public static class IdentityApiEndpointRouteBuilderExtensions
{
    private static readonly EmailAddressAttribute EmailAddress = new();

    /// <summary>
    /// Maps enhanced equivalents of selected stock Identity API routes.
    /// Map these endpoints on a separately prefixed route group.
    /// </summary>
    public static IEndpointRouteBuilder MapOverrideIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("")
            .WithTags("Identity overrides");
        var bearerOnly = new AuthorizeAttribute
        {
            AuthenticationSchemes = IdentityConstants.BearerScheme,
        };

        group.MapPost("/login", async Task<Results<EmptyHttpResult, JsonHttpResult<LoginFailureResponse>>>
            ([FromBody] LoginRequest login, [FromServices] IServiceProvider services) =>
        {
            var signInManager = services.GetRequiredService<SignInManager<TUser>>();
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            var result = await signInManager.PasswordSignInAsync(
                login.Email,
                login.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrWhiteSpace(login.TwoFactorCode))
                {
                    result = await signInManager.TwoFactorAuthenticatorSignInAsync(
                        login.TwoFactorCode,
                        isPersistent: false,
                        rememberClient: false);
                }
                else if (!string.IsNullOrWhiteSpace(login.TwoFactorRecoveryCode))
                {
                    result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }

            if (result.Succeeded)
            {
                // The in-box bearer handler writes AccessTokenResponse when this
                // request signs in using IdentityConstants.BearerScheme.
                return TypedResults.Empty;
            }

            return TypedResults.Json(new LoginFailureResponse(GetLoginFailureCode(result)), statusCode: StatusCodes.Status401Unauthorized);
        })
        .WithName("IdentityOverridesLogin")
        .WithSummary("Signs in with an opaque bearer token and stable failures.")
        .Produces<AccessTokenResponse>()
        .Produces<LoginFailureResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/manage/info", async Task<Results<Ok<ExtendedInfoResponse>, NotFound>>
            (ClaimsPrincipal principal, [FromServices] IServiceProvider services) =>
        {
            var userManager = services.GetRequiredService<UserManager<TUser>>();
            var user = await userManager.GetUserAsync(principal);
            return user is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(await CreateExtendedInfoResponseAsync(user, userManager));
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityOverridesManageInfo")
        .WithSummary("Gets an extended, non-secret account profile.")
        .Produces<ExtendedInfoResponse>();

        group.MapPost("/manage/info", async Task<Results<Ok<ExtendedInfoResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal principal, [FromBody] ExtendedInfoRequest request, HttpContext context, [FromServices] IServiceProvider services) =>
        {
            var userManager = services.GetRequiredService<UserManager<TUser>>();
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var changes = new[]
            {
                !string.IsNullOrWhiteSpace(request.NewEmail),
                !string.IsNullOrWhiteSpace(request.NewPassword),
                request.PhoneNumber is not null,
            };

            if (changes.Count(change => change) != 1)
            {
                return CreateValidationProblem(
                    "AmbiguousOperation",
                    "Specify exactly one of newEmail, newPassword, or phoneNumber.");
            }

            if (!string.IsNullOrWhiteSpace(request.NewEmail))
            {
                if (!EmailAddress.IsValid(request.NewEmail))
                {
                    return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(request.NewEmail)));
                }

                var currentEmail = await userManager.GetEmailAsync(user);
                if (!string.Equals(currentEmail, request.NewEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var code = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
                    var userId = await userManager.GetUserIdAsync(user);
                    var routeOptions = services.GetRequiredService<IOptions<IdentityApiRouteOptions>>().Value;
                    var confirmationUrl = BuildConfirmationUrl(context, routeOptions.StockIdentityPrefix, userId, code, request.NewEmail);
                    var emailSender = services.GetRequiredService<IEmailSender<TUser>>();
                    await emailSender.SendConfirmationLinkAsync(user, request.NewEmail, confirmationUrl);
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                IdentityResult passwordResult;
                if (await userManager.HasPasswordAsync(user))
                {
                    if (string.IsNullOrWhiteSpace(request.OldPassword))
                    {
                        return CreateValidationProblem(
                            "OldPasswordRequired",
                            "The old password is required to set a new password. Use password reset if it is unavailable.");
                    }

                    passwordResult = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                }
                else
                {
                    passwordResult = await userManager.AddPasswordAsync(user, request.NewPassword);
                }

                if (!passwordResult.Succeeded)
                {
                    return CreateValidationProblem(passwordResult);
                }
            }
            else
            {
                var phoneResult = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber!);
                if (!phoneResult.Succeeded)
                {
                    return CreateValidationProblem(phoneResult);
                }
            }

            return TypedResults.Ok(await CreateExtendedInfoResponseAsync(user, userManager));
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityOverridesUpdateManageInfo")
        .WithSummary("Changes exactly one extended account property.")
        .Produces<ExtendedInfoResponse>()
        .ProducesValidationProblem();

        group.MapGet("/manage/2fa", async Task<Results<Ok<TwoFactorStatusResponse>, NotFound>>
            (ClaimsPrincipal principal, [FromServices] IServiceProvider services) =>
        {
            var signInManager = services.GetRequiredService<SignInManager<TUser>>();
            var user = await signInManager.UserManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var userManager = signInManager.UserManager;
            var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
            return TypedResults.Ok(new TwoFactorStatusResponse(
                await userManager.GetTwoFactorEnabledAsync(user),
                !string.IsNullOrEmpty(authenticatorKey),
                await userManager.CountRecoveryCodesAsync(user),
                await signInManager.IsTwoFactorClientRememberedAsync(user)));
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityOverridesTwoFactorStatus")
        .WithSummary("Gets two-factor status without generating or revealing a shared key.")
        .Produces<TwoFactorStatusResponse>();

        return endpoints;
    }

    /// <summary>
    /// Maps portable account routes that do not duplicate a stock Identity API route.
    /// </summary>
    public static IEndpointRouteBuilder MapNewIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("")
            .WithTags("Identity extensions");
        var bearerOnly = new AuthorizeAttribute
        {
            AuthenticationSchemes = IdentityConstants.BearerScheme,
        };

        group.MapGet("/manage/passkeys", async Task<IResult> (ClaimsPrincipal principal, [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var passkeys = await userManager.GetPasskeysAsync(user);
            return TypedResults.Ok(passkeys.Select(passkey => new PasskeyResponse(
                WebEncoders.Base64UrlEncode(passkey.CredentialId),
                passkey.Name,
                passkey.CreatedAt,
                passkey.IsUserVerified,
                passkey.IsBackedUp)).ToArray());
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityManagePasskeys")
        .WithSummary("Lists the authenticated user's passkeys without exposing key material.")
        .Produces<PasskeyResponse[]>();

        group.MapPatch("/manage/passkeys/{credentialId}", async Task<IResult> (
            string credentialId,
            [FromBody] RenamePasskeyRequest request,
            ClaimsPrincipal principal,
            [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (!TryDecodeCredentialId(credentialId, out var credentialIdBytes))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["credentialId"] = ["The credential ID must be base64url encoded."],
                });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["A passkey name is required."],
                });
            }

            var passkey = await userManager.GetPasskeyAsync(user, credentialIdBytes);
            if (passkey is null)
            {
                return TypedResults.NotFound();
            }

            passkey.Name = request.Name.Trim();
            var result = await userManager.AddOrUpdatePasskeyAsync(user, passkey);
            return result.Succeeded
                ? TypedResults.NoContent()
                : CreateValidationProblem(result);
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityRenamePasskey")
        .WithSummary("Renames one passkey.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();

        group.MapDelete("/manage/passkeys/{credentialId}", async Task<IResult> (
            string credentialId,
            ClaimsPrincipal principal,
            [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (!TryDecodeCredentialId(credentialId, out var credentialIdBytes))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["credentialId"] = ["The credential ID must be base64url encoded."],
                });
            }

            var result = await userManager.RemovePasskeyAsync(user, credentialIdBytes);
            return result.Succeeded
                ? TypedResults.NoContent()
                : CreateValidationProblem(result);
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityDeletePasskey")
        .WithSummary("Removes one passkey.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();

        var passkeyGroup = group.MapGroup("/passkeys").DisableAntiforgery();

        passkeyGroup.MapPost("/register/begin", async Task<IResult> (
            ClaimsPrincipal principal,
            [FromServices] UserManager<TUser> userManager,
            [FromServices] SignInManager<TUser> signInManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var userId = await userManager.GetUserIdAsync(user);
            var userName = await userManager.GetUserNameAsync(user) ?? userId;
            var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
            {
                Id = userId,
                Name = userName,
                DisplayName = userName,
            });

            return TypedResults.Content(optionsJson, "application/json");
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityBeginPasskeyRegistration")
        .WithSummary("Begins passkey registration and writes the official temporary Identity ceremony cookie.");

        passkeyGroup.MapPost("/register/finish", async Task<IResult> (
            [FromBody] JsonElement credential,
            [FromQuery] string? name,
            ClaimsPrincipal principal,
            [FromServices] UserManager<TUser> userManager,
            [FromServices] SignInManager<TUser> signInManager) =>
        {
            PasskeyAttestationResult attestation;
            try
            {
                attestation = await signInManager.PerformPasskeyAttestationAsync(credential.GetRawText());
            }
            catch (InvalidOperationException)
            {
                return TypedResults.BadRequest(new PasskeyCeremonyFailureResponse(
                    "passkey_ceremony_not_found",
                    "No passkey registration is in progress. Begin a new registration and return its temporary Identity cookie."));
            }

            if (!attestation.Succeeded)
            {
                return TypedResults.BadRequest(new PasskeyCeremonyFailureResponse(
                    "passkey_attestation_failed",
                    "The passkey attestation could not be verified."));
            }

            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var userId = await userManager.GetUserIdAsync(user);
            if (!string.Equals(userId, attestation.UserEntity.Id, StringComparison.Ordinal))
            {
                return TypedResults.BadRequest(new PasskeyCeremonyFailureResponse(
                    "passkey_user_mismatch",
                    "The passkey ceremony belongs to a different account."));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                attestation.Passkey.Name = name.Trim();
            }

            var result = await userManager.AddOrUpdatePasskeyAsync(user, attestation.Passkey);
            return result.Succeeded
                ? TypedResults.Ok(new PasskeyRegistrationResponse(true, attestation.Passkey.Name))
                : CreateValidationProblem(result);
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityFinishPasskeyRegistration")
        .WithSummary("Completes passkey registration using the temporary Identity ceremony cookie.");

        passkeyGroup.MapPost("/login/begin", async Task<IResult> ([FromServices] SignInManager<TUser> signInManager) =>
        {
            var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user: null);
            return TypedResults.Content(optionsJson, "application/json");
        })
        .WithName("IdentityBeginPasskeyLogin")
        .WithSummary("Begins discoverable passkey login and writes the official temporary Identity ceremony cookie.");

        passkeyGroup.MapPost("/login/finish", async Task<IResult> (
            [FromBody] JsonElement credential,
            [FromServices] SignInManager<TUser> signInManager) =>
        {
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            Microsoft.AspNetCore.Identity.SignInResult result;
            try
            {
                result = await signInManager.PasskeySignInAsync(credential.GetRawText());
            }
            catch (InvalidOperationException)
            {
                return TypedResults.BadRequest(new PasskeyCeremonyFailureResponse(
                    "passkey_ceremony_not_found",
                    "No passkey login is in progress. Begin a new login and return its temporary Identity cookie."));
            }

            if (!result.Succeeded)
            {
                return TypedResults.Json(
                    new LoginFailureResponse(GetLoginFailureCode(result)),
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            return TypedResults.Empty;
        })
        .WithName("IdentityFinishPasskeyLogin")
        .WithSummary("Completes passkey login and emits an in-box opaque bearer token response.");

        group.MapGet("/manage/personal-data", async Task<IResult> (ClaimsPrincipal principal, [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new PersonalDataResponse(
                await userManager.GetUserIdAsync(user),
                await userManager.GetUserNameAsync(user),
                await userManager.GetEmailAsync(user),
                await userManager.GetPhoneNumberAsync(user),
                await userManager.IsEmailConfirmedAsync(user),
                await userManager.GetTwoFactorEnabledAsync(user)));
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityPersonalData")
        .WithSummary("Returns a fixed safe subset of personal data.")
        .Produces<PersonalDataResponse>();

        group.MapDelete("/manage/account", async Task<IResult> (
            [FromBody] DeleteAccountRequest request,
            ClaimsPrincipal principal,
            [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (!await userManager.HasPasswordAsync(user))
            {
                return CreateValidationProblem(
                    "PasswordlessRecentAuthenticationRequired",
                    "Passwordless deletion requires a recent interactive reauthentication flow, which this sample does not implement.");
            }

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return CreateValidationProblem("CurrentPasswordRequired", "The current password is required to delete this account.");
            }

            if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
            {
                return CreateValidationProblem("InvalidCurrentPassword", "The current password is incorrect.");
            }

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded
                ? TypedResults.NoContent()
                : CreateValidationProblem(result);
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityDeleteAccount")
        .WithSummary("Deletes a password account after validating its current password.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();

        group.MapPost("/manage/logout-all", async Task<IResult> (ClaimsPrincipal principal, [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var result = await userManager.UpdateSecurityStampAsync(user);
            return result.Succeeded
                ? TypedResults.NoContent()
                : CreateValidationProblem(result);
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityLogoutAll")
        .WithSummary("Invalidates refresh tokens by updating the security stamp; access tokens remain valid until expiry.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();

        group.MapGet("/manage/external-logins", async Task<IResult> (ClaimsPrincipal principal, [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var logins = await userManager.GetLoginsAsync(user);
            return TypedResults.Ok(logins.Select(login => new ExternalLoginResponse(
                login.LoginProvider,
                login.ProviderDisplayName)).ToArray());
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityExternalLogins")
        .WithSummary("Lists linked external login providers without provider keys.")
        .Produces<ExternalLoginResponse[]>();

        group.MapDelete("/manage/external-logins/{provider}", async Task<IResult> (
            string provider,
            ClaimsPrincipal principal,
            [FromServices] UserManager<TUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var logins = await userManager.GetLoginsAsync(user);
            var linkedLogins = logins
                .Where(login => string.Equals(login.LoginProvider, provider, StringComparison.Ordinal))
                .ToArray();
            if (linkedLogins.Length == 0)
            {
                return TypedResults.NotFound();
            }

            var hasOtherLogin = logins.Any(login => !string.Equals(login.LoginProvider, provider, StringComparison.Ordinal));
            var hasPassword = await userManager.HasPasswordAsync(user);
            var hasPasskey = (await userManager.GetPasskeysAsync(user)).Count > 0;
            if (!hasOtherLogin && !hasPassword && !hasPasskey)
            {
                return CreateValidationProblem(
                    "LastSignInMethod",
                    "Removing this provider would leave the account without a usable sign-in method.");
            }

            foreach (var login in linkedLogins)
            {
                var result = await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                if (!result.Succeeded)
                {
                    return CreateValidationProblem(result);
                }
            }

            return TypedResults.NoContent();
        })
        .RequireAuthorization(bearerOnly)
        .WithName("IdentityDeleteExternalLogin")
        .WithSummary("Unlinks an external provider without exposing provider keys or removing the final sign-in method.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();

        return endpoints;
    }

    private static string GetLoginFailureCode(Microsoft.AspNetCore.Identity.SignInResult result) =>
        result.RequiresTwoFactor ? LoginFailureCodes.RequiresTwoFactor :
        result.IsLockedOut ? LoginFailureCodes.LockedOut :
        result.IsNotAllowed ? LoginFailureCodes.NotAllowed :
        LoginFailureCodes.InvalidCredentials;

    private static async Task<ExtendedInfoResponse> CreateExtendedInfoResponseAsync<TUser>(
        TUser user,
        UserManager<TUser> userManager)
        where TUser : class
    {
        var passkeys = await userManager.GetPasskeysAsync(user);
        var logins = await userManager.GetLoginsAsync(user);
        var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

        return new ExtendedInfoResponse(
            await userManager.GetEmailAsync(user),
            await userManager.IsEmailConfirmedAsync(user),
            await userManager.GetPhoneNumberAsync(user),
            await userManager.HasPasswordAsync(user),
            await userManager.GetTwoFactorEnabledAsync(user),
            !string.IsNullOrEmpty(authenticatorKey),
            await userManager.CountRecoveryCodesAsync(user),
            passkeys.Count,
            logins.Select(login => login.LoginProvider).Distinct(StringComparer.Ordinal).ToArray());
    }

    private static string BuildConfirmationUrl(HttpContext context, string stockIdentityPrefix, string userId, string code, string changedEmail)
    {
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var prefix = stockIdentityPrefix.Trim('/');
        return $"{context.Request.Scheme}://{context.Request.Host}/{prefix}/confirmEmail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(encodedCode)}&changedEmail={Uri.EscapeDataString(changedEmail)}";
    }

    private static ValidationProblem CreateValidationProblem(IdentityResult result) =>
        TypedResults.ValidationProblem(result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray()));

    private static ValidationProblem CreateValidationProblem(string code, string description) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [code] = [description],
        });

    private static bool TryDecodeCredentialId(string credentialId, out byte[] credentialIdBytes)
    {
        try
        {
            credentialIdBytes = WebEncoders.Base64UrlDecode(credentialId);
            return credentialIdBytes.Length > 0;
        }
        catch (FormatException)
        {
            credentialIdBytes = [];
            return false;
        }
    }
}

/// <summary>Stable machine-readable values returned for unsuccessful override login attempts.</summary>
public static class LoginFailureCodes
{
    public const string InvalidCredentials = "invalid_credentials";
    public const string RequiresTwoFactor = "requires_two_factor";
    public const string LockedOut = "locked_out";
    public const string NotAllowed = "not_allowed";
}

/// <summary>Configures stock Identity route links used by the override endpoints.</summary>
public sealed class IdentityApiRouteOptions
{
    /// <summary>The mount path of the application's stock MapIdentityApi endpoints.</summary>
    public string StockIdentityPrefix { get; set; } = "/identity";
}

/// <summary>Failure response for <c>/identity-overrides/login</c>.</summary>
public sealed record LoginFailureResponse(string Code);

/// <summary>Supported extended account mutations.</summary>
public sealed class ExtendedInfoRequest
{
    public string? NewEmail { get; init; }

    public string? OldPassword { get; init; }

    public string? NewPassword { get; init; }

    public string? PhoneNumber { get; init; }
}

/// <summary>Non-secret extended account information.</summary>
public sealed record ExtendedInfoResponse(
    string? Email,
    bool IsEmailConfirmed,
    string? PhoneNumber,
    bool HasPassword,
    bool IsTwoFactorEnabled,
    bool HasAuthenticator,
    int RecoveryCodesLeft,
    int PasskeyCount,
    string[] ExternalLoginProviders);

/// <summary>Non-mutating two-factor status.</summary>
public sealed record TwoFactorStatusResponse(
    bool IsTwoFactorEnabled,
    bool HasAuthenticator,
    int RecoveryCodesLeft,
    bool IsMachineRemembered);

/// <summary>Safe metadata about one passkey.</summary>
public sealed record PasskeyResponse(
    string CredentialId,
    string? Name,
    DateTimeOffset CreatedAt,
    bool IsUserVerified,
    bool IsBackedUp);

/// <summary>Request to rename a passkey.</summary>
public sealed record RenamePasskeyRequest(string? Name);

/// <summary>Non-secret passkey registration completion response.</summary>
public sealed record PasskeyRegistrationResponse(bool Registered, string? Name);

/// <summary>Machine-readable passkey ceremony failure response.</summary>
public sealed record PasskeyCeremonyFailureResponse(string Code, string Message);

/// <summary>A safe explicit personal-data projection.</summary>
public sealed record PersonalDataResponse(
    string UserId,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    bool IsEmailConfirmed,
    bool IsTwoFactorEnabled);

/// <summary>Current-password confirmation for deleting an account.</summary>
public sealed record DeleteAccountRequest(string? CurrentPassword);

/// <summary>External login metadata without a provider key.</summary>
public sealed record ExternalLoginResponse(string Provider, string? DisplayName);
