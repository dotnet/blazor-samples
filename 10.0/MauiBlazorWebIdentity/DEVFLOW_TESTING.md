# DevFlow live testing

The live suite complements—not replaces—the server API suite. Run both with:

```bash
./scripts/run-devflow-live-tests.sh
```

The runner executes `MauiBlazorWeb.IdentityApi.Tests`, builds the web,
Playwright, and `net10.0-maccatalyst` projects, then starts an isolated Development SQLite
database, the real web server, the DevFlow broker when needed, and the
production-bundle-id Mac Catalyst app. It waits for the agent and CDP before
running the hosted Playwright and `MauiBlazorWeb.DevFlow.Tests` suites. It only stops PIDs it started; it stops
the broker only when it started a previously stopped broker. Existing occupied
ports fail explicitly rather than being terminated.
The Development/Testing-only `/health` endpoint gates server readiness.

## Prerequisites

Install the .NET 10 MAUI workload and the matching experimental `maui` CLI
release `0.1.0-preview.12.26421.1`, and Xcode with Mac Catalyst support. The MAUI project references the matching
`Microsoft.Maui.DevFlow.Agent` and `Microsoft.Maui.DevFlow.Blazor`
`0.1.0-preview.12.26421.1` packages from the configured `dotnet10` NuGet
source. An unsigned local Mac Catalyst signing warning is expected.
Install the cached browser once for the Microsoft Playwright hosted-page tests:

```bash
pwsh MauiBlazorWeb.WebUi.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```

Optional runner overrides are `DEVFLOW_SERVER_PORT`, `DEVFLOW_SERVER_URL`,
`DEVFLOW_AGENT_PORT` (or `DEVFLOW_TEST_PORT`). The runner never sources signing identities or
profiles. The temporary state lives under `scripts/.devflow-state-*` and is
removed on exit.

## What is covered

* `MauiBlazorWeb.IdentityApi.Tests` owns server API behavior.
* `MauiBlazorWeb.WebUi.Tests` uses Microsoft Playwright against the hosted
  Razor account pages.
* The official `Microsoft.Maui.DevFlow.Client` `AgentClient` validates the
  native app host through the `IdentityBlazorWebView` AutomationId and uses CDP
  for the Blazor DOM: registration, Development-only
  confirmation, failed and valid login, profile phone/password update, personal
  data visibility, logout-all, deletion, and anonymous navigation.

Live tests are intentionally opt-in and separately discoverable:

```bash
DEVFLOW_LIVE_TESTS=1 DEVFLOW_SERVER_URL=https://localhost:7157 \
DEVFLOW_AGENT_PORT=10223 \
dotnet test MauiBlazorWeb.DevFlow.Tests
```

Hosted page tests are likewise opt-in:

```bash
WEB_UI_TESTS=1 DEVFLOW_SERVER_URL=https://localhost:7157 \
dotnet test MauiBlazorWeb.WebUi.Tests
```

Missing or unreachable required infrastructure produces an explicit MSTest
inconclusive result. The runner uses `maui devflow agent wait`, `list`, `agent
status`, and the `webview` readiness commands; the tests use the matching
`AgentClient` API to retain a single DevFlow mutation lease throughout the
stateful flow. Test diagnostics never print access/refresh/bearer tokens,
confirmation codes, or full response bodies. The test-only `HttpClient`
accepts a certificate only for a loopback Development server.

The agent, Blazor tools, and Apple web inspector mapper are compiled and
registered only in `DEBUG`; Release has no DevFlow registration. The current
CLI surface used here is `webview` (including `webviews`, `status`, and
`source`), `ui`, `agent`, `broker`, and `batch`.
Do not use obsolete `agent interact` commands.

Debug Mac Catalyst uses a Debug-only entitlement file with app sandbox disabled
so the in-app DevFlow agent can bind locally. Release continues to use the
normal sandbox entitlement file.

Experimental limitations: Mac Catalyst launch/signing remains host-dependent,
and WebKit/CDP availability requires supported iOS or Mac Catalyst versions.
The hosted Microsoft Playwright suite covers registration/confirmation/login
and forgot/reset-password/login browser ceremonies. Next coverage should add
deterministic 2FA, real passkey ceremonies, and an external-provider browser
callback once real provider/authenticator fixtures are available.
