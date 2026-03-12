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

Log in to your CIAM tenant and run the setup script:

```bash
az login --tenant <your-ciam-tenant-id>
pwsh ./Setup-Azure.ps1
```

The script will:
- Create two app registrations (web server + MAUI client)
- Expose an API scope (`access_as_user`)
- Generate a client secret
- Automatically patch `appsettings.json`, `MsalConfig.cs`, and platform config files with real values

### 2. Create a User Flow

In the [Azure Portal](https://portal.azure.com), navigate to your CIAM tenant:
1. Go to **External Identities** → **User flows**
2. Create a **Sign up and sign in** flow
3. Link it to both app registrations

### 3. Run the web server

```bash
cd MauiBlazorWebEntra.Web
dotnet run --launch-profile https
```

### 4. Run the MAUI app

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
pwsh ./Teardown-Azure.ps1
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
