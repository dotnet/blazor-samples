# MauiBlazorWebEntra

A .NET MAUI Blazor Hybrid + ASP.NET Core Web App sample that uses **Microsoft Entra External ID** (CIAM) for authentication instead of ASP.NET Core Identity.

## Architecture

This solution contains three projects:

| Project | Description |
|---|---|
| **MauiBlazorWebEntra** | .NET MAUI Blazor Hybrid app (Android, iOS, Mac Catalyst, Windows) |
| **MauiBlazorWebEntra.Shared** | Razor Class Library with shared UI components (Home, Counter, Weather) |
| **MauiBlazorWebEntra.Web** | ASP.NET Core Blazor Server web app + API |

### Authentication Flow

**Web browser users:** OpenID Connect → Entra External ID login page → Cookie-based session

**MAUI native app:** MSAL.NET → System browser → Entra login → Access token → Bearer header to API

Both flows authenticate against the same Entra External ID (CIAM) tenant. There is no local user database.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`)
- [PowerShell Core 7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) (`pwsh`)
- A [Microsoft Entra External ID](https://learn.microsoft.com/entra/external-id/customers/overview-customers-ciam) (CIAM) tenant

## Quick Start

### 1. Set up Azure resources

Run the interactive setup script — it will guide you through everything:

```bash
pwsh ./scripts/Setup-Azure.ps1
```

The script walks you through 5 steps:
1. **Create a CIAM tenant** in the Azure Portal (opens automatically)
2. **Login** to the new tenant via Azure CLI
3. **Create app registrations**, API scope, and client secret (automated)
4. **Create a user flow** in the Azure Portal (opens automatically)
5. **Patch config files** with real values (automated)

### 2. Run the web server

```bash
cd MauiBlazorWebEntra.Web
dotnet run --launch-profile https
```

### 3. Run the MAUI app

```bash
# Android
cd MauiBlazorWebEntra
dotnet build -f net10.0-android
# Deploy the APK to your emulator/device

# iOS Simulator
dotnet build -f net10.0-ios -r iossimulator-arm64
```

## Cleanup

To remove the Azure app registrations:

```bash
pwsh ./scripts/Teardown-Azure.ps1
```

## Known Issues

### Windows: MSAL token cache is not persisted by default

On iOS and Android, MSAL persists tokens automatically using native platform features (Keychain and SharedPreferences respectively). On Windows, the generic .NET assembly does not include built-in token persistence, so tokens would be lost when the app restarts.

This sample calls `EnableSecureStorageTokenCachePersistence()` to persist the MSAL token cache using .NET MAUI [`SecureStorage`](https://github.com/dotnet/maui/blob/main/src/Essentials/src/SecureStorage/SecureStorage.windows.cs), which encrypts data using [`DataProtectionProvider("LOCAL=user")`](https://learn.microsoft.com/uwp/api/windows.security.cryptography.dataprotection.dataprotectionprovider) scoped to the current Windows user.

### Mac Catalyst: MSAL does not ship a `maccatalyst` TFM

MSAL.NET (as of v4.x) does not include a `net*-maccatalyst` target framework. When running on Mac Catalyst, the app loads the generic .NET assembly instead of a platform-specific one. This causes two problems:

1. **System browser auth throws `PlatformNotSupportedException`** — MSAL's browser-based interactive login is not implemented in the generic assembly.
2. **Token cache is not persisted** — the generic assembly has no native token persistence, so users must sign in again every time the app restarts.

#### Workarounds in this sample

**Authentication:** A custom [`ICustomWebUi`](MauiBlazorWebEntra/Platforms/MacCatalyst/MacCatalystWebUi.cs) implementation uses Apple's `ASWebAuthenticationSession` to handle the interactive login flow. It is wired up via a `WithMacCatalystWebView()` extension method in [`MsalServiceExtensions.cs`](MauiBlazorWebEntra/Services/MsalServiceExtensions.cs).

**Token persistence:** The `SecureStorage`-based token cache (normally used only on Windows) is also enabled for Mac Catalyst so that tokens survive app restarts.

Both workarounds include comments referencing [MSAL issue #3527](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3527) and can be removed once MSAL ships native Mac Catalyst support.

## Key Differences from MauiBlazorWebIdentity

| Aspect | MauiBlazorWebIdentity | MauiBlazorWebEntra |
|---|---|---|
| **Auth provider** | ASP.NET Core Identity (local DB) | Microsoft Entra External ID (cloud) |
| **Web auth** | Cookie + Identity pages | OIDC redirect to Entra |
| **API auth** | JWT from `/identity/login` | JWT from MSAL.NET |
| **User store** | SQLite (EF Core) | Entra (no local DB) |
| **Registration** | Self-service Identity pages | Entra user flow |
| **NuGet (Web)** | EF Core + Identity | Microsoft.Identity.Web |
| **NuGet (MAUI)** | — | Microsoft.Identity.Client (MSAL) |
