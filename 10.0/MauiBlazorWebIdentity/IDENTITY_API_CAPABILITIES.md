# ASP.NET Core Identity REST capability matrix

This sample keeps the framework's `MapIdentityApi` contract at `/identity`.
It adds contribution-oriented, first-party REST contracts using only public
`UserManager<TUser>` and `SignInManager<TUser>` APIs. **Stock**, **override**,
and **new** below describe the route source; support is assessed for the
complete hosted Identity feature, including its MAUI experience.

`MauiBlazorWeb.IdentityApi.Tests/IdentityApiTests.cs` is the Microsoft-only
integration suite. It uses MSTest, `WebApplicationFactory`, and SQLite.
`MauiBlazorWeb.DevFlow.Tests` is a separately discoverable, opt-in live MSTest
suite: the official DevFlow `AgentClient` verifies the native MAUI host and
BlazorWebView AutomationId, then drives the MAUI Blazor DOM through CDP. It
does not replace API coverage; see [DEVFLOW_TESTING.md](DEVFLOW_TESTING.md) for
infrastructure and experimental limitations.

## Capability inventory

| Hosted Account feature | Classification | Route and implementation | MAUI use | Coverage / limitation |
| --- | --- | --- | --- | --- |
| Register | Fully supported | **Stock:** `POST /identity/register` | `Register.razor` through `AccountClient.RegisterAsync` | Registration is exercised by the typed client; Development supplies an in-memory notification only. |
| Confirm and resend email | Fully supported | **Stock:** `GET /identity/confirmEmail`, `POST /identity/resendConfirmationEmail` | Registration and Account screens | Confirmation remains the stock encoded-token flow; account-existence disclosure remains framework behavior. |
| Forgot and reset password | Fully supported | **Stock:** `POST /identity/forgotPassword`, `POST /identity/resetPassword` | `PasswordHelp.razor` | The development notification page provides links only in Development; production must provide a real `IEmailSender`. |
| Password login and opaque tokens | Fully supported | **Override:** `POST /identity-overrides/login`; **Stock comparison:** `POST /identity/login?useCookies=false`, `POST /identity/refresh` | `Login.razor`, `MauiAuthenticationStateProvider` | Test covers a stable `invalid_credentials` response. The override emits the stock bearer-token response on success. |
| Lockout, unconfirmed-account, authenticator, and recovery-code login outcomes | Partially supported | **Override:** `POST /identity-overrides/login` accepts `twoFactorCode` or `twoFactorRecoveryCode` and returns stable `locked_out`, `not_allowed`, or `requires_two_factor` codes | Login page has authenticator/recovery inputs after `requires_two_factor` | No deterministic integration test: meaningful TOTP/recovery tests require clock/authenticator or recovery-code setup that would obscure this public-API sample. |
| Profile and pending email change | Fully supported | **Override:** `GET/POST /identity-overrides/manage/info` | `Account.razor` displays and changes email | Update requires exactly one operation and sends the stock confirmation link. |
| Phone, change password, and first local password | Fully supported | **Override:** `POST /identity-overrides/manage/info` | Account editor | Test verifies exactly-one mutation plus required/valid current-password behavior. A passwordless account may add its first password. |
| Two-factor status | Fully supported | **Override:** `GET /identity-overrides/manage/2fa` | Account editor | Read-only status never exposes the authenticator key. |
| Configure authenticator, recovery codes, and remembered browser | Fully supported | **Stock:** `POST /identity/manage/2fa` | Account editor | Uses the framework's mutation endpoint. Successful TOTP ceremony coverage is intentionally not synthesized with private internals. |
| Passkey list, rename, and remove | Partially supported | **New:** `GET/PATCH/DELETE /identity/manage/passkeys` | Account screen lists/removes; the typed client also supports rename | Server route is complete, but MAUI's current UI does not offer rename and has no native ceremony yet. |
| Passkey register and sign-in ceremony | Partially supported | **New:** `POST /identity/passkeys/register/begin`, `/register/finish`, `/login/begin`, `/login/finish` | Account screen previews registration-begin JSON only | Begin writes the official temporary Identity cookie; tests cover its presence and finish-without-cookie failure. A real success assertion needs an authenticator. Native `Microsoft.Maui.Authentication.Passkeys` create/assert calls are the .NET 11 client seam. |
| View filtered personal data | Fully supported | **New:** `GET /identity/manage/personal-data` | Account screen | Explicit DTO excludes password hashes, tokens, provider keys, passkey public-key data, and authenticator secrets. |
| Delete account | Partially supported | **New:** `DELETE /identity/manage/account` | Account screen | Test proves wrong-password rejection and confirmed deletion. Passwordless deletion is deliberately blocked pending a real recent interactive reauthentication contract. |
| List and unlink linked providers | Partially supported | **New:** `GET/DELETE /identity/manage/external-logins/{provider}` | Account screen | Test proves a final sign-in method cannot be unlinked. Browser provider challenge, callback, and link handoff are deferred until a real provider is configured. |
| Local logout and logout all devices | Fully supported | Local MAUI token cleanup; **New:** `POST /identity/manage/logout-all` | `Logout.razor` and Account screen | Test proves security-stamp logout invalidates the refresh token but leaves the presented access token usable until expiry. |

## Route coexistence and framework proposal

The generic endpoint library never maps an existing method/path pair below
`/identity`. Framework routes remain directly inspectable and comparable.
Enhanced behavior is isolated at `/identity-overrides`; new routes under
`/identity` exist only where `MapIdentityApi` has no counterpart.

The sample contracts marked **Override** or **New** are proposed ASP.NET Core
framework additions, not stock framework APIs:

* a machine-readable login-outcome contract;
* extended, unambiguous account read/update and read-only 2FA status;
* passkey management and JSON ceremony endpoints;
* safe personal-data projection, password-confirmed deletion, provider
  management, and security-stamp logout-all.

They are intentionally generic and only invoke public Identity APIs, so they
can be lifted into an ASP.NET Core contribution without inheriting MAUI types.

## Token, notification, and deployment constraints

The in-box bearer tokens are Data Protection-protected opaque tickets, **not
JWTs** or an OAuth/OIDC service. Access tokens default to one hour; refresh
tokens default to 14 days and are reusable. There is no device registry,
replay detection, or individual-token revocation. Updating the security stamp
invalidates refresh credentials, while access tickets already issued remain
valid until expiry. Production deployments must persist and share Data
Protection keys across instances.

`/development/notifications` is mapped only in Development, stores a bounded
in-memory list, and is test-covered as absent in Production. It is not an
email implementation and must never be enabled as a production delivery
mechanism.

The server passkey routes use Identity's official temporary ceremony cookie.
The net10 MAUI client preserves a narrow JSON seam and only previews
registration options; its real `CreateAsync`/`AssertAsync` implementation is
deferred to .NET 11 MAUI passkey APIs. External providers similarly require a
browser handoff and callback, so their flow is deferred rather than simulated.
