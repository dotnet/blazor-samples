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
| Starting commit for this increment | `94b3e0da570f36addc395be53c709edb96b98283` |
| Current implementation commit | `fbb447d08fce0e35c655aa8fc5ba51ddacab7c1c` (authenticated fixtures) |
| Current documentation baseline | `c04d9fde79c3d7683fb47616d60f8f0d8ff6d894` (OpenAPI URL correction) |
| Current completion status | Endpoint implementation, authenticated fixtures, documentation, and requested builds are complete; this status update records fixture artifact cleanup |
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
| 4 | Complete | Development notification UI, complete Microsoft-only integration suite, documentation reconciliation, and platform validation |
| 5 | Complete | Debug-only DevFlow Mac Catalyst live-test increment with isolated Development state |

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

Final validation was run after `fbb447d08fce0e35c655aa8fc5ba51ddacab7c1c`
and before this completion-documentation commit.

| Command | Result |
| --- | --- |
| `dotnet --info` | SDK 11 preview and .NET 10 SDK/runtime installed |
| `dotnet build MauiBlazorWeb.Web/MauiBlazorWeb.Web.csproj --no-restore` | Passed; known upstream package vulnerability warnings remain |
| HTTPS development smoke test | Passed; stock, override, and new routes present; bearer-only passkey route returns 401 without bearer credential |
| Production development-notification smoke test | Passed; `/development/notifications` returns 404 outside Development |
| Passkey login-begin smoke test | Passed; emits an `Identity.TwoFactorUserId` temporary ceremony cookie |
| `dotnet build MauiBlazorWeb/MauiBlazorWeb.csproj -f net10.0-maccatalyst --no-restore` | Passed; existing unsigned local development entitlement warning |
| `dotnet build MauiBlazorWeb/MauiBlazorWeb.csproj -f net10.0-ios --no-restore` | Passed |
| `dotnet build MauiBlazorWeb/MauiBlazorWeb.csproj -f net10.0-android --no-restore` | Passed |
| `dotnet build MauiBlazorWeb.sln --no-restore` | Passed for web, Mac Catalyst, iOS, and Android; existing upstream package and unsigned local development entitlement warnings remain |
| SecureStorage boundary validation | Passed by build review: reads/removes fall back to logged-out/best-effort cleanup with diagnostics; failed writes retain the valid in-memory pair and disable restart persistence |
| `dotnet test MauiBlazorWeb.IdentityApi.Tests/MauiBlazorWeb.IdentityApi.Tests.csproj --no-restore` | Passed: 10 tests for route separation, stable invalid login, bearer isolation, Development-only notification gating, account-mutation ambiguity/current-password checks, logout-all refresh invalidation, safe deletion, external-login final-method protection, and passkey temporary-cookie failure behavior; per-factory SQLite files and WAL/SHM sidecars are cleaned |

The separately discoverable `MauiBlazorWeb.DevFlow.Tests` project is opt-in
live coverage. It uses Microsoft test infrastructure and the official
`Microsoft.Maui.DevFlow.Client` `AgentClient` API for coordinated CDP
automation; the runner also exercises the current `maui devflow webview`,
`agent`, and `broker` CLI commands for readiness diagnostics. It validates the
real Debug Mac Catalyst host/`IdentityBlazorWebView`, then drives registration,
Development-only confirmation, login, phone update, and logout through CDP.
See [DEVFLOW_TESTING.md](DEVFLOW_TESTING.md); no token, confirmation-code, or
full-response diagnostics are emitted.

## Known blockers and deferred work

* The MAUI client uses typed REST account methods and an operation/auth epoch.
  The Microsoft-only integration suite covers stable public account invariants.
  A full passkey attestation/assertion cannot be manufactured without a platform
  authenticator; tests instead cover the official begin-cookie contract and
  finish-without-cookie failure. This avoids test-only protocol internals.
* Native passkey ceremony execution is deferred behind a small client seam until
  the .NET 11 MAUI passkey APIs are used. The net10 UI only previews the server
  begin JSON.
* Browser external-provider login/link completion remains deferred until a real
  provider is configured.

## Resume instructions

## Completion and resume status

**Completion target:** all implemented routes and the hosted-feature ledger are
complete. The final validation run is recorded below before this documentation
increment is committed.

**Deliberate limits:** lockout, TOTP, recovery-code, and successful
passkey-finish integration tests are not synthesized: realistic inputs require
clock control, an authenticator, or private protocol fixtures. The suite covers
the closest stable public behavior instead. Passwordless deletion requires a
future recent-interactive-reauthentication contract, and external-provider
browser handoff is deferred until a provider is configured.
