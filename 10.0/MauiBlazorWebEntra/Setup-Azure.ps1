<#
.SYNOPSIS
    Sets up Microsoft Entra External ID (CIAM) resources for the MauiBlazorWebEntra sample.

.DESCRIPTION
    This interactive script creates the required Azure resources:
    - Two app registrations (web server + MAUI client) in your Entra External ID tenant
    - An API scope on the web app (access_as_user)
    - API permission grant for the MAUI app
    - Client secret for the web app
    It then patches appsettings.json and MsalConfig.cs with the real values.

.NOTES
    Prerequisites:
    - Azure CLI (az) installed and on PATH
    - PowerShell Core 7+ (pwsh)
    - An existing Microsoft Entra External ID (CIAM) tenant
    - You must be signed in to the correct tenant: az login --tenant <tenant-id>
#>

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " MauiBlazorWebEntra - Azure Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# --- Verify Azure CLI ---
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI (az) is not installed. Please install it: https://learn.microsoft.com/cli/azure/install-azure-cli"
    exit 1
}

# --- Get tenant info ---
Write-Host "Checking Azure CLI login status..." -ForegroundColor Yellow
$account = az account show 2>&1 | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in. Run: az login --tenant <your-ciam-tenant-id>"
    exit 1
}

$tenantId = $account.tenantId
$tenantName = Read-Host "Enter your CIAM tenant name (e.g., 'contoso' from contoso.ciamlogin.com)"

if ([string]::IsNullOrWhiteSpace($tenantName)) {
    Write-Error "Tenant name is required."
    exit 1
}

Write-Host ""
Write-Host "Tenant ID:   $tenantId" -ForegroundColor Green
Write-Host "Tenant Name: $tenantName" -ForegroundColor Green
Write-Host "Authority:   https://$tenantName.ciamlogin.com/$tenantId" -ForegroundColor Green
Write-Host ""

# --- Create Web App Registration ---
Write-Host "Creating web server app registration..." -ForegroundColor Yellow

$webAppName = "MauiBlazorWebEntra-Web"
$webApp = az ad app create `
    --display-name $webAppName `
    --sign-in-audience "AzureADMyOrg" `
    --web-redirect-uris "https://localhost:7157/signin-oidc" `
    --enable-id-token-issuance true `
    2>&1 | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create web app registration: $webApp"
    exit 1
}

$webClientId = $webApp.appId
$webObjectId = $webApp.id
Write-Host "  Web App Client ID: $webClientId" -ForegroundColor Green

# Create service principal for the web app
Write-Host "  Creating service principal..." -ForegroundColor Yellow
az ad sp create --id $webClientId 2>&1 | Out-Null

# --- Generate Client Secret ---
Write-Host "  Generating client secret..." -ForegroundColor Yellow
$secretResult = az ad app credential reset `
    --id $webClientId `
    --display-name "MauiBlazorWebEntra Setup" `
    --years 1 `
    2>&1 | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to generate client secret: $secretResult"
    exit 1
}

$clientSecret = $secretResult.password
Write-Host "  Client secret generated." -ForegroundColor Green

# --- Expose API Scope (access_as_user) ---
Write-Host "  Exposing API scope: access_as_user..." -ForegroundColor Yellow

$scopeId = [guid]::NewGuid().ToString()
$apiBody = @{
    identifierUris = @("api://$webClientId")
    api = @{
        oauth2PermissionScopes = @(
            @{
                id = $scopeId
                adminConsentDescription = "Allow the MAUI app to access the web API on behalf of the signed-in user."
                adminConsentDisplayName = "Access web API as user"
                isEnabled = $true
                type = "User"
                userConsentDescription = "Allow this app to access the web API on your behalf."
                userConsentDisplayName = "Access web API"
                value = "access_as_user"
            }
        )
    }
} | ConvertTo-Json -Depth 10 -Compress

# Use az rest to PATCH the application via Microsoft Graph
az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$webObjectId" `
    --headers "Content-Type=application/json" `
    --body $apiBody `
    2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to expose API scope. You may need to do this manually in the Azure portal."
} else {
    Write-Host "  API scope exposed: api://$webClientId/access_as_user" -ForegroundColor Green
}

# --- Create MAUI App Registration (Public Client) ---
Write-Host ""
Write-Host "Creating MAUI client app registration..." -ForegroundColor Yellow

$mauiAppName = "MauiBlazorWebEntra-MAUI"
$mauiRedirectUri = "msal{PLACEHOLDER}://auth"

$mauiApp = az ad app create `
    --display-name $mauiAppName `
    --sign-in-audience "AzureADMyOrg" `
    --is-fallback-public-client true `
    2>&1 | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create MAUI app registration: $mauiApp"
    exit 1
}

$mauiClientId = $mauiApp.appId
$mauiObjectId = $mauiApp.id
Write-Host "  MAUI App Client ID: $mauiClientId" -ForegroundColor Green

# Create service principal for the MAUI app
Write-Host "  Creating service principal..." -ForegroundColor Yellow
az ad sp create --id $mauiClientId 2>&1 | Out-Null

# Add MSAL redirect URI for native client
$actualRedirectUri = "msal$mauiClientId`://auth"
Write-Host "  Adding redirect URI: $actualRedirectUri" -ForegroundColor Yellow

$redirectBody = @{
    publicClient = @{
        redirectUris = @($actualRedirectUri)
    }
} | ConvertTo-Json -Depth 5 -Compress

az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$mauiObjectId" `
    --headers "Content-Type=application/json" `
    --body $redirectBody `
    2>&1 | Out-Null

# --- Grant MAUI App Permission to Web API Scope ---
Write-Host "  Granting API permission (access_as_user)..." -ForegroundColor Yellow

# Get the web app's service principal ID for the permission
$webSpId = (az ad sp show --id $webClientId 2>&1 | ConvertFrom-Json).id

$permissionBody = @{
    requiredResourceAccess = @(
        @{
            resourceAppId = $webClientId
            resourceAccess = @(
                @{
                    id = $scopeId
                    type = "Scope"
                }
            )
        }
    )
} | ConvertTo-Json -Depth 10 -Compress

az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$mauiObjectId" `
    --headers "Content-Type=application/json" `
    --body $permissionBody `
    2>&1 | Out-Null

# Admin consent
Write-Host "  Granting admin consent..." -ForegroundColor Yellow
az ad app permission admin-consent --id $mauiClientId 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Admin consent may need to be granted manually in the Azure portal."
}

# --- Patch appsettings.json ---
Write-Host ""
Write-Host "Patching configuration files..." -ForegroundColor Yellow

$scriptDir = $PSScriptRoot
$appSettingsPath = Join-Path $scriptDir "MauiBlazorWebEntra.Web" "appsettings.json"

if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

    $appSettings.AzureAd.Instance = "https://$tenantName.ciamlogin.com/"
    $appSettings.AzureAd.TenantId = $tenantId
    $appSettings.AzureAd.ClientId = $webClientId
    $appSettings.AzureAd.ClientSecret = $clientSecret

    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "  Updated: $appSettingsPath" -ForegroundColor Green
} else {
    Write-Warning "appsettings.json not found at: $appSettingsPath"
}

# --- Patch MsalConfig.cs ---
$msalConfigPath = Join-Path $scriptDir "MauiBlazorWebEntra" "Services" "MsalConfig.cs"

if (Test-Path $msalConfigPath) {
    $content = Get-Content $msalConfigPath -Raw
    $content = $content -replace 'YOUR_TENANT_NAME', $tenantName
    $content = $content -replace 'YOUR_TENANT_ID', $tenantId
    $content = $content -replace 'YOUR_MAUI_CLIENT_ID', $mauiClientId
    $content = $content -replace 'YOUR_WEB_CLIENT_ID', $webClientId
    Set-Content $msalConfigPath $content -Encoding UTF8
    Write-Host "  Updated: $msalConfigPath" -ForegroundColor Green
} else {
    Write-Warning "MsalConfig.cs not found at: $msalConfigPath"
}

# --- Patch MsalActivity.cs (Android) ---
$msalActivityPath = Join-Path $scriptDir "MauiBlazorWebEntra" "Platforms" "Android" "MsalActivity.cs"
if (Test-Path $msalActivityPath) {
    $content = Get-Content $msalActivityPath -Raw
    $content = $content -replace 'msalYOUR_MAUI_CLIENT_ID', "msal$mauiClientId"
    Set-Content $msalActivityPath $content -Encoding UTF8
    Write-Host "  Updated: $msalActivityPath" -ForegroundColor Green
}

# --- Patch Info.plist (iOS) ---
$plistPath = Join-Path $scriptDir "MauiBlazorWebEntra" "Platforms" "iOS" "Info.plist"
if (Test-Path $plistPath) {
    $content = Get-Content $plistPath -Raw
    $content = $content -replace 'msalYOUR_MAUI_CLIENT_ID', "msal$mauiClientId"
    Set-Content $plistPath $content -Encoding UTF8
    Write-Host "  Updated: $plistPath" -ForegroundColor Green
}

# --- Summary ---
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Setup Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resources Created:" -ForegroundColor Green
Write-Host "  Web App:  $webAppName (Client ID: $webClientId)" -ForegroundColor White
Write-Host "  MAUI App: $mauiAppName (Client ID: $mauiClientId)" -ForegroundColor White
Write-Host "  API Scope: api://$webClientId/access_as_user" -ForegroundColor White
Write-Host ""
Write-Host "Configuration files patched:" -ForegroundColor Green
Write-Host "  - MauiBlazorWebEntra.Web/appsettings.json" -ForegroundColor White
Write-Host "  - MauiBlazorWebEntra/Services/MsalConfig.cs" -ForegroundColor White
Write-Host "  - MauiBlazorWebEntra/Platforms/Android/MsalActivity.cs" -ForegroundColor White
Write-Host "  - MauiBlazorWebEntra/Platforms/iOS/Info.plist" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Create a user flow in your CIAM tenant (Azure Portal > External Identities > User flows)" -ForegroundColor White
Write-Host "  2. Build and run the web server: cd MauiBlazorWebEntra.Web && dotnet run" -ForegroundColor White
Write-Host "  3. Build and deploy the MAUI app to your device/emulator" -ForegroundColor White
Write-Host ""
Write-Host "To clean up, run: ./Teardown-Azure.ps1" -ForegroundColor Yellow
Write-Host ""

# Save app IDs for teardown
$teardownDataPath = Join-Path $scriptDir ".azure-setup.json"
@{
    webClientId = $webClientId
    mauiClientId = $mauiClientId
    tenantId = $tenantId
    tenantName = $tenantName
} | ConvertTo-Json | Set-Content $teardownDataPath -Encoding UTF8
Write-Host "Setup data saved to .azure-setup.json (used by Teardown-Azure.ps1)" -ForegroundColor DarkGray
