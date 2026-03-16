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
