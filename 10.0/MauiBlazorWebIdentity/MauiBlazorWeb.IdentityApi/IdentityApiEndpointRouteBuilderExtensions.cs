using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
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
                    var confirmationUrl = BuildConfirmationUrl(context, userId, code, request.NewEmail);
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
        return endpoints.MapGroup("").WithTags("Identity extensions");
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

    private static string BuildConfirmationUrl(HttpContext context, string userId, string code, string changedEmail)
    {
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        return $"{context.Request.Scheme}://{context.Request.Host}/identity/confirmEmail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(encodedCode)}&changedEmail={Uri.EscapeDataString(changedEmail)}";
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
}

/// <summary>Stable machine-readable values returned for unsuccessful override login attempts.</summary>
public static class LoginFailureCodes
{
    public const string InvalidCredentials = "invalid_credentials";
    public const string RequiresTwoFactor = "requires_two_factor";
    public const string LockedOut = "locked_out";
    public const string NotAllowed = "not_allowed";
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
