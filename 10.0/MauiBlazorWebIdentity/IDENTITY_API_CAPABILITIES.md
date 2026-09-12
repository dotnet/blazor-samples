# ASP.NET Core Identity REST capability matrix

This ledger compares the hosted Razor `/Account/*` UI with the stock
`MapIdentityApi` contract and the contribution-oriented REST additions planned
for this sample. The REST routes are intentionally first-party and use only
public `UserManager<TUser>` and `SignInManager<TUser>` APIs.

## Support summary

### Stock APIs fully consumable by portable clients

| Capability | Stock route | MAUI usage | Support |
| --- | --- | --- | --- |
| Register | `POST /identity/register` | Registration page | Implemented |
| Password login and tokens | `POST /identity/login?useCookies=false` | Typed token client | Implemented |
| Refresh | `POST /identity/refresh` | Serialized token refresh | Implemented |
| Confirm/resend email | `/identity/confirmEmail`, `POST /identity/resendConfirmationEmail` | Registration/account pages | Implemented |
| Forgot/reset password | `POST /identity/forgotPassword`, `POST /identity/resetPassword` | Password-help page | Implemented |
| Profile/email/password | `GET/POST /identity/manage/info` | Account editor | Implemented |
| Authenticator and recovery codes | `POST /identity/manage/2fa` | Account editor and login continuation | Implemented |

### Partial stock APIs proposed for override

| Capability | Stock limitation | Override route | Support |
| --- | --- | --- | --- |
| Login outcomes | Generic unsuccessful response does not expose a stable outcome | `POST /identity-overrides/login` | Implemented |
| Manage info | Does not return phone, password, authenticators, passkeys, recovery code count, or external providers | `GET /identity-overrides/manage/info` | Implemented |
| Manage info mutations | Cannot set phone or add a first local password; combination semantics are opaque | `POST /identity-overrides/manage/info` | Implemented |
| 2FA inspection | Stock handler is mutating/configuration-oriented | `GET /identity-overrides/manage/2fa` | Implemented |

### Missing stock APIs proposed as new routes

| Capability | New route(s) | Support |
| --- | --- | --- |
| Passkey list/manage | `GET/PATCH/DELETE /identity/manage/passkeys` | Implemented; MAUI previews native begin JSON pending .NET 11 |
| Passkey registration and login | `/identity/passkeys/register/*`, `/identity/passkeys/login/*` | Server implemented; native MAUI ceremony deferred to .NET 11 |
| Filtered personal data | `GET /identity/manage/personal-data` | Implemented |
| Delete account | `DELETE /identity/manage/account` | Implemented for password accounts; passwordless recent reauth deferred |
| Invalidate refresh sessions | `POST /identity/manage/logout-all` | Implemented |
| List/unlink external logins | `GET/DELETE /identity/manage/external-logins` | Implemented |

## Hosted Account feature inventory

| Hosted Razor source | UserManager/SignInManager API | Stock endpoint | REST route/action | MAUI usage | Support/security/tests |
| --- | --- | --- | --- | --- | --- |
| `Pages/Register.razor` | `CreateAsync`, `GenerateEmailConfirmationTokenAsync` | `POST /identity/register` | Stock | Registration | Planned; development notifications only, test registration/confirmation |
| `Pages/ConfirmEmail.razor` | `ConfirmEmailAsync` | `GET /identity/confirmEmail` | Stock | Confirmation action | Planned; test valid/invalid token |
| `Pages/ResendEmailConfirmation.razor` | `GenerateEmailConfirmationTokenAsync` | `POST /identity/resendConfirmationEmail` | Stock | Resend action | Planned; do not reveal account existence |
| `Pages/Login.razor` | `PasswordSignInAsync` | `POST /identity/login` | Override login for stable outcomes | Login and continuation | Pending; bearer only, test invalid/lockout/not-allowed |
| `Pages/LoginWith2fa.razor` | `TwoFactorAuthenticatorSignInAsync` | `POST /identity/login` | Override login | Authenticator continuation | Pending; test TOTP |
| `Pages/LoginWithRecoveryCode.razor` | `TwoFactorRecoveryCodeSignInAsync` | `POST /identity/login` | Override login | Recovery continuation | Pending; never log recovery codes |
| `Pages/ForgotPassword.razor` | `GeneratePasswordResetTokenAsync` | `POST /identity/forgotPassword` | Stock | Reset request | Planned; bounded dev notification |
| `Pages/ResetPassword.razor` | `ResetPasswordAsync` | `POST /identity/resetPassword` | Stock | Reset completion | Planned; test reset/login |
| `Pages/Manage/Index.razor` | `GetUserNameAsync` | `GET /identity/manage/info` | Override extended info | Profile summary | Pending; bearer isolation |
| `Pages/Manage/Email.razor` | `SetEmailAsync`, `GenerateChangeEmailTokenAsync` | `POST /identity/manage/info` | Override extended info | Email/phone editing | Pending; test confirmation semantics |
| `Pages/Manage/ChangePassword.razor` | `ChangePasswordAsync` | `POST /identity/manage/info` | Override extended info | Change password | Pending; current password required |
| `Pages/Manage/SetPassword.razor` | `AddPasswordAsync` | None | Override info | Set first password | Pending; reject ambiguous mutations |
| `Pages/Manage/TwoFactorAuthentication.razor` | `GetTwoFactorEnabledAsync`, `CountRecoveryCodesAsync` | `POST /identity/manage/2fa` | `GET /identity-overrides/manage/2fa` | 2FA status | Pending; non-mutating read |
| `Pages/Manage/EnableAuthenticator.razor` | `ResetAuthenticatorKeyAsync`, `VerifyTwoFactorTokenAsync`, `SetTwoFactorEnabledAsync` | `POST /identity/manage/2fa` | Stock | Setup/verify | Planned; do not expose shared key in status API |
| `Pages/Manage/Disable2fa.razor` | `SetTwoFactorEnabledAsync` | `POST /identity/manage/2fa` | Stock | Disable | Planned; test bearer auth |
| `Pages/Manage/ResetAuthenticator.razor` | `SetTwoFactorEnabledAsync`, `ResetAuthenticatorKeyAsync` | `POST /identity/manage/2fa` | Stock | Reset | Planned; test regeneration |
| `Pages/Manage/GenerateRecoveryCodes.razor` | `GenerateNewTwoFactorRecoveryCodesAsync` | `POST /identity/manage/2fa` | Stock | Recovery display | Planned; never persist or log |
| `Pages/Manage/Passkeys.razor`, `RenamePasskey.razor` | Passkey list/update/remove APIs | None | New passkey routes | List/rename/remove | Pending; link `dotnet/maui#36837`, JSON-in/JSON-out prior to .NET 11 native API |
| `Pages/Manage/PersonalData.razor` | `GetUserIdAsync` | None | New personal-data route | Data summary | Pending; explicit DTO excludes secrets and provider keys |
| `Pages/Manage/DeletePersonalData.razor` | `HasPasswordAsync`, `CheckPasswordAsync`, `DeleteAsync` | None | New delete route | Account deletion | Pending; password or recent-auth requirement |
| `Pages/Manage/ExternalLogins.razor` | `GetLoginsAsync`, `RemoveLoginAsync` | None | New external-login routes | List/unlink | Pending; cannot remove final sign-in method |
| `Pages/ExternalLogin.razor` | `ConfigureExternalAuthenticationProperties`, `ExternalLoginSignInAsync` | None | Deferred | Display only | Deferred until a real provider is configured |
| `Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs` | `SignOutAsync` | None | New logout-all route | Local logout/logout all | Pending; access ticket remains valid until expiry |

## Contribution notes

The passkey server endpoints follow the official Identity API pattern used by
[dotnet/maui#36837](https://github.com/dotnet/maui/pull/36837). The net10
client exchanges ceremony options and responses as JSON and clearly reports that
the native `.NET 11 Passkeys API` is forthcoming. It keeps the server contract
stable while a client seam later calls `Microsoft.Maui.Authentication.Passkeys`
for `CreateAsync` and `AssertAsync`.

The generic endpoint library never maps an existing method/path pair on
`/identity`; enhanced handlers live under `/identity-overrides` so callers can
compare stock and proposed behavior without route collisions.

## Implementation map

| Surface | Server implementation | MAUI usage |
| --- | --- | --- |
| Stock and proposed endpoint mappings | `MauiBlazorWeb.IdentityApi/IdentityApiEndpointRouteBuilderExtensions.cs` | `MauiBlazorWeb/Services/AccountClient.cs` |
| Bearer-only host configuration | `MauiBlazorWeb.Web/Program.cs` | `MauiBlazorWeb/Services/MauiAuthenticationStateProvider.cs` |
| Development notifications | `MauiBlazorWeb.Web/Components/Account/DevelopmentEmailSender.cs` | `MauiBlazorWeb/Components/Pages/Register.razor` |
| Account management UI | `MauiBlazorWeb.IdentityApi/IdentityApiEndpointRouteBuilderExtensions.cs` | `MauiBlazorWeb/Components/Pages/Account.razor` |
| Registration and recovery | `MauiBlazorWeb.Web/Program.cs` | `MauiBlazorWeb/Components/Pages/Register.razor`, `PasswordHelp.razor` |
