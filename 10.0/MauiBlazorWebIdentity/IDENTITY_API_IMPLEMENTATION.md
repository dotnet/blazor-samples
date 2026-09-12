# Identity REST API implementation tracker

## Goal and non-goals

This sample demonstrates a portable, first-party REST client for ASP.NET Core
Identity. MAUI and other native clients use the same JSON contracts while the
server owns identity business rules. Authentication uses the in-box opaque
bearer access and refresh tokens.

It deliberately does **not** add OpenIddict, OAuth/OIDC flows, third-party
authentication packages, custom JWTs, custom token issuance, or a fake external
provider.

## Source and architecture decisions

| Item | Value |
| --- | --- |
| Base commit | `567732e0a78d2c1b2a22c3677f86c673d7527bed` (`origin/main`) |
| Current commit | `HEAD` (MAUI REST account UI and resilient SecureStorage increment) |
| Stock API | `app.MapGroup("/identity").MapIdentityApi<ApplicationUser>()` |
| New endpoints | `MauiBlazorWeb.IdentityApi.MapNewIdentityApi<TUser>()`, mapped on `/identity` only for stock-absent routes |
| Overrides | `MapOverrideIdentityApi<TUser>()`, mapped only under `/identity-overrides` |
| Native authorization | Explicit `IdentityConstants.BearerScheme`; the application cookie remains for `/Account/*` |
| Tokens | ASP.NET Core Data Protection opaque tickets; access defaults to one hour, refresh to 14 days |

The stock bearer refresh token is reusable. There is no replay detection, device
registry, individual token revocation, or immediate access-token revocation.
`logout-all` updates the security stamp, which invalidates refresh but leaves
issued access tokens valid until their expiry. Production deployments require
shared, persistent Data Protection keys.

## Phase status

| Phase | Status | Scope |
| --- | --- | --- |
| 0 | Complete | Tracker, capability ledger, project inventory |
| 1 | Complete | Stock endpoint typed MAUI client, durable token lifecycle, account UI |
| 2 | Complete | Generic override endpoint library and `/identity-overrides` client use |
| 3 | Complete | Generic new endpoint library: passkeys, personal data, deletion, external logins, logout-all |
| 4 | In progress | Development notification UI, integration tests, platform validation |

## Endpoint ledger

| Endpoint group | Endpoint | Status | Notes |
| --- | --- | --- | --- |
| Stock | `/identity/*` | Existing | MapIdentityApi remains the direct comparison surface |
| Override | `/identity-overrides/login` | Complete | Stable failure codes |
| Override | `/identity-overrides/manage/info` | Complete | Extended profile and unambiguous mutations |
| Override | `/identity-overrides/manage/2fa` | Complete | Read-only 2FA status |
| New | `/identity/manage/passkeys` | Complete | List, rename, remove |
| New | `/identity/passkeys/*` | Complete | Official Identity ceremony APIs and temporary cookie continuity |
| New | `/identity/manage/personal-data` | Complete | Explicit filtered DTO |
| New | `/identity/manage/account` | Partial | Password accounts supported; passwordless deletion explicitly requires an unimplemented recent interactive reauthentication flow |
| New | `/identity/manage/logout-all` | Complete | Security-stamp refresh invalidation |
| New | `/identity/manage/external-logins` | Complete | List/unlink with last-method safeguard |

## Client feature ledger

| Feature | Status | Intended client surface |
| --- | --- | --- |
| Password register/login/refresh | Complete | Typed client with serialized refresh and auth epochs |
| Email confirmation and password reset | Complete | Registration and password-help pages |
| Profile, email, phone, passwords | Complete | Account page |
| Authenticator and recovery codes | Complete | Account page and login continuation |
| Passkey management | Partial | List/remove and JSON preview with `.NET 11 Passkeys API coming soon` |
| Personal data/deletion | Complete | Account page |
| External logins and logout-all | Complete | Account page |

## Validation ledger

| Command | Result |
| --- | --- |
| `dotnet --info` | SDK 11 preview and .NET 10 SDK/runtime installed |
| `dotnet build MauiBlazorWeb.Web/MauiBlazorWeb.Web.csproj --no-restore` | Passed; known upstream package vulnerability warnings remain |
| HTTPS development smoke test | Passed; stock, override, and new routes present; bearer-only passkey route returns 401 without bearer credential |
| Production development-notification smoke test | Passed; `/development/notifications` returns 404 outside Development |
| Passkey login-begin smoke test | Passed; emits an `Identity.TwoFactorUserId` temporary ceremony cookie |
| `dotnet build MauiBlazorWeb/MauiBlazorWeb.csproj -f net10.0-maccatalyst --no-restore` | Passed; existing unsigned local development entitlement warning |
| SecureStorage boundary validation | Passed by build review: reads/removes fall back to logged-out/best-effort cleanup with diagnostics; failed writes retain the valid in-memory pair and disable restart persistence |
| Web, Mac Catalyst, iOS, Android builds | Pending |
| Microsoft-only integration tests | Pending project creation |

## Known blockers and deferred work

* The MAUI client uses typed REST account methods and an operation/auth epoch.
  A Microsoft-only integration-test project has not been added yet.
* Native passkey ceremony execution is deferred behind a small client seam until
  the .NET 11 MAUI passkey APIs are used. The net10 UI will inspect the server's
  sanitized options JSON only.
* Browser external-provider login/link completion is designed but deferred until
  a real provider is configured.

## Resume instructions

**Next task:** add Microsoft-only integration tests for route layout, bearer
isolation, login outcomes, passkey ceremony continuity, and development-route
environment gating.
