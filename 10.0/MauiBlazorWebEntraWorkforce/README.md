# MauiBlazorWebEntraWorkforce

A .NET MAUI Blazor Hybrid + ASP.NET Core Web App sample that uses **Microsoft Entra workforce identity** for employee and organizational sign-in instead of ASP.NET Core Identity.

## Architecture

This solution contains three projects:

| Project | Description |
|---|---|
| **MauiBlazorWebEntraWorkforce** | .NET MAUI Blazor Hybrid app (Android, iOS, Mac Catalyst, Windows) |
| **MauiBlazorWebEntraWorkforce.Shared** | Razor Class Library with shared UI components (Home, Counter, Weather) |
| **MauiBlazorWebEntraWorkforce.Web** | ASP.NET Core Blazor Server web app + API |

### Authentication flow

**Web browser users:** OpenID Connect -> Microsoft Entra sign-in -> Cookie-based session

**MAUI native app:** MSAL.NET -> Microsoft Entra sign-in -> Access token -> Bearer header to API

Both flows authenticate against the same workforce tenant. There is no local user database, and users must already exist in the tenant or be invited as guests.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`)
- [PowerShell Core 7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) (`pwsh`)
- A Microsoft Entra workforce tenant where you can create app registrations

## Quick start

### 1. Set up Azure resources

Run the interactive setup script from the sample root:

```bash
pwsh ./scripts/Setup-Azure.ps1
```

The script will:

1. Sign you into your workforce tenant
2. Create or reuse the web app registration
3. Create or reuse the MAUI public client registration
4. Grant the MAUI app permission to call the web API
5. Patch the local config files with the generated values

### 2. Run the web server

```bash
cd MauiBlazorWebEntraWorkforce.Web
dotnet run --launch-profile https
```

### 3. Run the MAUI app

```bash
# Android
cd MauiBlazorWebEntraWorkforce
dotnet build -f net10.0-android

# iOS Simulator
dotnet build -f net10.0-ios -r iossimulator-arm64
```

## Cleanup

To remove the Azure app registrations created by the script:

```bash
pwsh ./scripts/Teardown-Azure.ps1
```

## Known issues

### Windows: MSAL token cache is not persisted by default

On iOS and Android, MSAL persists tokens automatically using native platform features (Keychain and SharedPreferences respectively). On Windows, the generic .NET assembly does not include built-in token persistence, so tokens would be lost when the app restarts.

This sample calls `EnableSecureStorageTokenCachePersistence()` to persist the MSAL token cache using .NET MAUI [`SecureStorage`](https://github.com/dotnet/maui/blob/main/src/Essentials/src/SecureStorage/SecureStorage.windows.cs), which encrypts data using [`DataProtectionProvider("LOCAL=user")`](https://learn.microsoft.com/uwp/api/windows.security.cryptography.dataprotection.dataprotectionprovider) scoped to the current Windows user.

### Mac Catalyst: MSAL does not ship a `maccatalyst` TFM

MSAL.NET (as of v4.x) does not include a `net*-maccatalyst` target framework. When running on Mac Catalyst, the app loads the generic .NET assembly instead of a platform-specific one. This causes two problems:

1. **System browser auth throws `PlatformNotSupportedException`** - MSAL's browser-based interactive login is not implemented in the generic assembly.
2. **Token cache is not persisted** - the generic assembly has no native token persistence, so users must sign in again every time the app restarts.

#### Workarounds in this sample

**Authentication:** A custom [`ICustomWebUi`](MauiBlazorWebEntraWorkforce/Platforms/MacCatalyst/MacCatalystWebUi.cs) implementation uses Apple's `ASWebAuthenticationSession` to handle the interactive login flow. It is wired up via a `WithMacCatalystWebView()` extension method in [`MsalServiceExtensions.cs`](MauiBlazorWebEntraWorkforce/Services/MsalServiceExtensions.cs).

**Token persistence:** The `SecureStorage`-based token cache (normally used only on Windows) is also enabled for Mac Catalyst so that tokens survive app restarts.

## Key differences from MauiBlazorWebIdentity

| Aspect | MauiBlazorWebIdentity | MauiBlazorWebEntraWorkforce |
|---|---|---|
| **Auth provider** | ASP.NET Core Identity (local DB) | Microsoft Entra workforce tenant |
| **Web auth** | Cookie + Identity pages | OIDC redirect to Entra |
| **API auth** | JWT from `/identity/login` | JWT from MSAL.NET |
| **User store** | SQLite (EF Core) | Entra directory (no local DB) |
| **User onboarding** | Self-service registration pages | Admin-managed or invited tenant users |
| **NuGet (Web)** | EF Core + Identity | Microsoft.Identity.Web |
| **NuGet (MAUI)** | - | Microsoft.Identity.Client (MSAL) |
