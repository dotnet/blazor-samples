<#
.SYNOPSIS
    Full guided setup for MauiBlazorWebEntra — from a fresh Azure account to a working sample.

.DESCRIPTION
    This interactive script walks you through the complete setup:
      1. Create an Entra External ID (CIAM) tenant in the Azure Portal  (manual)
      2. Login to the new CIAM tenant via Azure CLI                     (automated)
      3. Create app registrations, API scope, client secret             (automated)
      4. Create a user flow and link applications in the Portal         (manual)
      5. Patch all configuration files with real values                 (automated)

    Steps marked (manual) open the Azure Portal and wait for you to complete them.
    Steps marked (automated) run entirely via Azure CLI.

.NOTES
    Prerequisites:
    - Azure CLI (az) installed: https://learn.microsoft.com/cli/azure/install-azure-cli
    - PowerShell Core 7+: https://learn.microsoft.com/powershell/scripting/install/installing-powershell
    - An Azure subscription (free tier works)
#>

$ErrorActionPreference = 'Stop'

# --- Helper: Open URL cross-platform ---
function Open-Url {
    param([string]$Url)
    if ($IsMacOS) { Start-Process "open" -ArgumentList $Url }
    elseif ($IsWindows) { Start-Process $Url }
    elseif ($IsLinux) { Start-Process "xdg-open" -ArgumentList $Url }
    else { Write-Host "  Please open: $Url" }
}

function Write-Step {
    param([int]$Number, [string]$Title, [string]$Type = "AUTO")
    Write-Host ""
    Write-Host "=== Step $Number — $Title [$Type] ===" -ForegroundColor Blue
}

Write-Host ""
Write-Host "=== MauiBlazorWebEntra — Full Azure Setup ===" -ForegroundColor Blue
Write-Host ""
Write-Host "This script will walk you through the complete setup."
Write-Host "Some steps require the Azure Portal; they will open automatically."
Write-Host ""

# ---------------------------------------------------
# PREREQUISITES
# ---------------------------------------------------

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "Azure CLI (az) is not installed." -ForegroundColor Red
    Write-Host "Install it from: https://learn.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Blue
    exit 1
}
Write-Host "✓ Azure CLI found" -ForegroundColor Green

# ---------------------------------------------------
# STEP 1: Create CIAM Tenant (Portal)
# ---------------------------------------------------

Write-Step 1 "Create an Entra External ID Tenant" "PORTAL"

Write-Host ""
Write-Host "You need a Microsoft Entra External ID tenant (CIAM)."
Write-Host "This cannot be automated and must be created in the Azure Portal."
Write-Host ""

Write-Host "Do you already have a CIAM tenant? (y/N): " -ForegroundColor Magenta -NoNewline
$hasExistingTenant = Read-Host

if ($hasExistingTenant -ne 'y' -and $hasExistingTenant -ne 'Y') {
    Write-Host ""
    Write-Host "We will open the Entra admin center so you can create a tenant."
    Write-Host "Review the instructions below first, then press Enter to open the browser."
    Write-Host ""
    Write-Host "  Basics tab:" -ForegroundColor Blue
    Write-Host "    1. Click '+ Create' at the top of the page"
    Write-Host "    2. Select 'External' tenant type, click 'Continue'"
    Write-Host "    3. Fill in the form:"
    Write-Host "         - Tenant Name:     Maui Blazor Entra" -ForegroundColor Blue
    Write-Host "         - Domain Name:     mauiblazorentra" -ForegroundColor Blue
    Write-Host "           (becomes mauiblazorentra.onmicrosoft.com)"
    Write-Host "         - Country/Region:  <your nearest location>" -ForegroundColor Blue
    Write-Host "           (cannot be changed later!)" -ForegroundColor Red
    Write-Host "    4. Click 'Next: Add a subscription'"
    Write-Host ""
    Write-Host "  Subscription tab:" -ForegroundColor Blue
    Write-Host "         - Subscription:        <select your subscription>" -ForegroundColor Blue
    Write-Host "         - Resource group:      rg-maui-blazor-entra" -ForegroundColor Blue
    Write-Host "           (click 'Create new' if needed)"
    Write-Host "         - Resource group loc:  <match your tenant region>" -ForegroundColor Blue
    Write-Host "         - Billing Unit:        Monthly active users (MAU)" -ForegroundColor Blue
    Write-Host "           (leave default, free in preview)"
    Write-Host "    5. Click 'Review + Create', then 'Create'"
    Write-Host "    6. Wait for provisioning (1-3 minutes)"
    Write-Host ""

    Write-Host "Press Enter to open the Entra admin center..." -ForegroundColor Magenta
    Read-Host

    Open-Url "https://entra.microsoft.com/#view/Microsoft_AAD_IAM/DirectorySwitchBlade/subtitle/"

    Write-Host "Press Enter once the tenant is created..." -ForegroundColor Magenta
    Read-Host
}

Write-Host ""
Write-Host "Now enter the details of your CIAM tenant."
Write-Host "You can find these in the Azure Portal → Microsoft Entra ID → Overview"
Write-Host ""

Write-Host "Tenant domain prefix (e.g., 'mauiblazorentra' from mauiblazorentra.onmicrosoft.com): " -ForegroundColor Magenta -NoNewline
$tenantName = Read-Host
if ([string]::IsNullOrWhiteSpace($tenantName)) {
    Write-Error "Tenant domain prefix is required."
    exit 1
}

# ---------------------------------------------------
# STEP 2: Login to the CIAM Tenant
# ---------------------------------------------------

Write-Step 2 "Login to CIAM Tenant" "AUTO"

Write-Host ""
Write-Host "A browser window will open to log in to the CIAM tenant:"
Write-Host "  $tenantName.onmicrosoft.com" -ForegroundColor Blue
Write-Host ""
Write-Host "Press Enter to continue..." -ForegroundColor Magenta -NoNewline
Read-Host

az login --tenant "$tenantName.onmicrosoft.com" --allow-no-subscriptions 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to login to tenant '$tenantName.onmicrosoft.com'. Please check the domain prefix and try again."
    exit 1
}

# Auto-detect tenant ID from the logged-in session
$account = az account show 2>$null | ConvertFrom-Json
$tenantId = $account.tenantId

Write-Host "  ✓ Logged in successfully" -ForegroundColor Green
Write-Host ""
Write-Host "  Tenant: $tenantName.onmicrosoft.com"
Write-Host "  ID:     $tenantId"
Write-Host "  CIAM:   https://$tenantName.ciamlogin.com"

# ---------------------------------------------------
# STEP 3: Create App Registrations
# ---------------------------------------------------

Write-Step 3 "Create App Registrations" "AUTO"

# --- Web App Registration (Confidential Client) ---
Write-Host ""
Write-Host "Creating web server app registration..."

$webAppName = "MauiBlazorWebEntra-Web"
$webAppJson = az ad app create `
    --display-name $webAppName `
    --sign-in-audience "AzureADMyOrg" `
    --web-redirect-uris "https://localhost:7157/signin-oidc" `
    --enable-id-token-issuance true 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create web app registration."
    exit 1
}

$webApp = $webAppJson | ConvertFrom-Json

$webClientId = $webApp.appId
$webObjectId = $webApp.id
Write-Host "  ✓ Web App Client ID: $webClientId" -ForegroundColor Green

# Create service principal
az ad sp create --id $webClientId 2>&1 | Out-Null
Write-Host "  ✓ Service principal created" -ForegroundColor Green

# Generate client secret
Write-Host "  Generating client secret..."
$secretJson = az ad app credential reset `
    --id $webClientId `
    --display-name "MauiBlazorWebEntra Setup" `
    --years 1 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to generate client secret."
    exit 1
}

$secretResult = $secretJson | ConvertFrom-Json

$clientSecret = $secretResult.password
Write-Host "  ✓ Client secret generated" -ForegroundColor Green

# Expose API scope (access_as_user)
Write-Host "  Exposing API scope: access_as_user..."

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

az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$webObjectId" `
    --headers "Content-Type=application/json" `
    --body $apiBody `
    2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to expose API scope automatically. You may need to do this manually."
} else {
    Write-Host "  ✓ API scope: api://$webClientId/access_as_user" -ForegroundColor Green
}

# --- MAUI App Registration (Public Client) ---
Write-Host ""
Write-Host "Creating MAUI client app registration..."

$mauiAppName = "MauiBlazorWebEntra-MAUI"
$mauiAppJson = az ad app create `
    --display-name $mauiAppName `
    --sign-in-audience "AzureADMyOrg" `
    --is-fallback-public-client true 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create MAUI app registration."
    exit 1
}

$mauiApp = $mauiAppJson | ConvertFrom-Json

$mauiClientId = $mauiApp.appId
$mauiObjectId = $mauiApp.id
Write-Host "  ✓ MAUI App Client ID: $mauiClientId" -ForegroundColor Green

# Create service principal
az ad sp create --id $mauiClientId 2>&1 | Out-Null
Write-Host "  ✓ Service principal created" -ForegroundColor Green

# Add MSAL redirect URI
$actualRedirectUri = "msal$mauiClientId`://auth"
Write-Host "  Adding redirect URI: $actualRedirectUri"

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
Write-Host "  ✓ Redirect URI configured" -ForegroundColor Green

# Grant MAUI app permission to web API scope
Write-Host "  Granting API permission (access_as_user)..."

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
az ad app permission admin-consent --id $mauiClientId 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Admin consent may need to be granted manually in the Azure portal."
} else {
    Write-Host "  ✓ Admin consent granted" -ForegroundColor Green
}

# ---------------------------------------------------
# STEP 4: Create User Flow (Automated via Graph API)
# ---------------------------------------------------

Write-Step 4 "Create User Flow" "AUTO"

Write-Host ""
Write-Host "Creating sign-up/sign-in user flow via Microsoft Graph API..." -ForegroundColor Magenta
Write-Host ""

$userFlowBody = @{
    "@odata.type" = "#microsoft.graph.externalUsersSelfServiceSignUpEventsFlow"
    displayName = "signup_signin"
    conditions = @{
        applications = @{
            includeApplications = @(
                @{ appId = $webClientId },
                @{ appId = $mauiClientId }
            )
        }
    }
    onAuthenticationMethodLoadStart = @{
        "@odata.type" = "#microsoft.graph.onAuthenticationMethodLoadStartExternalUsersSelfServiceSignUp"
        identityProviders = @(
            @{ id = "EmailPassword-OAUTH" }
        )
    }
    onInteractiveAuthFlowStart = @{
        "@odata.type" = "#microsoft.graph.onInteractiveAuthFlowStartExternalUsersSelfServiceSignUp"
        isSignUpAllowed = $true
    }
    onAttributeCollection = @{
        "@odata.type" = "#microsoft.graph.onAttributeCollectionExternalUsersSelfServiceSignUp"
        attributes = @(
            @{
                id = "email"
                displayName = "Email Address"
                description = "Email address of the user"
                userFlowAttributeType = "builtIn"
                dataType = "string"
            },
            @{
                id = "displayName"
                displayName = "Display Name"
                description = "Display Name of the User."
                userFlowAttributeType = "builtIn"
                dataType = "string"
            }
        )
        attributeCollectionPage = @{
            views = @(
                @{
                    inputs = @(
                        @{
                            attribute = "email"
                            label = "Email Address"
                            inputType = "text"
                            hidden = $true
                            editable = $false
                            writeToDirectory = $true
                            required = $true
                        },
                        @{
                            attribute = "displayName"
                            label = "Display Name"
                            inputType = "text"
                            hidden = $false
                            editable = $true
                            writeToDirectory = $true
                            required = $false
                        }
                    )
                }
            )
        }
    }
} | ConvertTo-Json -Depth 10

$flowResult = az rest `
    --method POST `
    --url "https://graph.microsoft.com/v1.0/identity/authenticationEventsFlows" `
    --headers "Content-Type=application/json" `
    --body $userFlowBody 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "  ✗ Automated user flow creation failed." -ForegroundColor Red
    Write-Host ""
    # Show the actual error so the user can diagnose
    $errorText = ($flowResult | Out-String).Trim()
    if ($errorText) {
        Write-Host "  Error details:" -ForegroundColor Red
        foreach ($line in ($errorText -split "`n")) {
            Write-Host "    $line" -ForegroundColor Red
        }
        Write-Host ""
    }
    Write-Host "  Falling back to manual creation in the portal..." -ForegroundColor Magenta
    Write-Host ""
    Write-Host "  Part A — Create the user flow:" -ForegroundColor Blue
    Write-Host ""
    Write-Host "  Make sure you are in your CIAM tenant (check top-right)!" -ForegroundColor Magenta
    Write-Host ""
    Write-Host "    1. Left menu: External Identities > User flows"
    Write-Host "    2. Click '+ New user flow'"
    Write-Host "    3. Select 'Sign up and sign in', click 'Create'"
    Write-Host "    4. Fill in the form:"
    Write-Host "         - Name:                 signup_signin" -ForegroundColor Blue
    Write-Host "         - Identity providers:   Email with password" -ForegroundColor Blue
    Write-Host "    5. Click 'Create'"
    Write-Host ""
    Write-Host "  Part B — Add applications to the flow:" -ForegroundColor Blue
    Write-Host ""
    Write-Host "    1. Click on the new 'signup_signin' flow"
    Write-Host "    2. In the left panel, click 'Applications'"
    Write-Host "    3. Click '+ Add application'"
    Write-Host "    4. Check BOTH of these apps:"
    Write-Host "         - $webAppName" -ForegroundColor Blue
    Write-Host "         - $mauiAppName" -ForegroundColor Blue
    Write-Host "    5. Click 'Select'"
    Write-Host ""

    Write-Host "Press Enter to open the Entra admin center..." -ForegroundColor Magenta -NoNewline
    Read-Host

    Open-Url "https://entra.microsoft.com/"

    Write-Host "Press Enter once the user flow is created and both apps are linked..." -ForegroundColor Magenta -NoNewline
    Read-Host
} else {
    Write-Host "  ✓ User flow 'signup_signin' created" -ForegroundColor Green
    Write-Host "  ✓ Linked apps: $webAppName, $mauiAppName" -ForegroundColor Green
}

# ---------------------------------------------------
# STEP 5: Patch Configuration Files
# ---------------------------------------------------

Write-Step 5 "Patch Configuration Files" "AUTO"

Write-Host ""
$scriptDir = $PSScriptRoot

# --- appsettings.json ---
$appSettingsPath = Join-Path $scriptDir "MauiBlazorWebEntra.Web" "appsettings.json"
if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $appSettings.AzureAd.Instance = "https://$tenantName.ciamlogin.com/"
    $appSettings.AzureAd.TenantId = $tenantId
    $appSettings.AzureAd.ClientId = $webClientId
    $appSettings.AzureAd.ClientSecret = $clientSecret
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "  ✓ appsettings.json" -ForegroundColor Green
} else {
    Write-Warning "  appsettings.json not found at: $appSettingsPath"
}

# --- MsalConfig.cs ---
$msalConfigPath = Join-Path $scriptDir "MauiBlazorWebEntra" "Services" "MsalConfig.cs"
if (Test-Path $msalConfigPath) {
    $content = Get-Content $msalConfigPath -Raw
    $content = $content -replace 'YOUR_TENANT_NAME', $tenantName
    $content = $content -replace 'YOUR_TENANT_ID', $tenantId
    $content = $content -replace 'YOUR_MAUI_CLIENT_ID', $mauiClientId
    $content = $content -replace 'YOUR_WEB_CLIENT_ID', $webClientId
    Set-Content $msalConfigPath $content -Encoding UTF8
    Write-Host "  ✓ MsalConfig.cs" -ForegroundColor Green
} else {
    Write-Warning "  MsalConfig.cs not found at: $msalConfigPath"
}

# --- MsalActivity.cs (Android) ---
$msalActivityPath = Join-Path $scriptDir "MauiBlazorWebEntra" "Platforms" "Android" "MsalActivity.cs"
if (Test-Path $msalActivityPath) {
    $content = Get-Content $msalActivityPath -Raw
    $content = $content -replace 'msalYOUR_MAUI_CLIENT_ID', "msal$mauiClientId"
    Set-Content $msalActivityPath $content -Encoding UTF8
    Write-Host "  ✓ MsalActivity.cs (Android)" -ForegroundColor Green
}

# --- Info.plist (iOS) ---
$plistPath = Join-Path $scriptDir "MauiBlazorWebEntra" "Platforms" "iOS" "Info.plist"
if (Test-Path $plistPath) {
    $content = Get-Content $plistPath -Raw
    $content = $content -replace 'msalYOUR_MAUI_CLIENT_ID', "msal$mauiClientId"
    Set-Content $plistPath $content -Encoding UTF8
    Write-Host "  ✓ Info.plist (iOS)" -ForegroundColor Green
}

# ---------------------------------------------------
# SAVE & SUMMARY
# ---------------------------------------------------

# Save for teardown
$teardownDataPath = Join-Path $scriptDir ".azure-setup.json"
@{
    webClientId  = $webClientId
    mauiClientId = $mauiClientId
    tenantId     = $tenantId
    tenantName   = $tenantName
} | ConvertTo-Json | Set-Content $teardownDataPath -Encoding UTF8

Write-Host ""
Write-Host "=== Setup Complete! ✓ ===" -ForegroundColor Green
Write-Host ""
Write-Host "  Tenant:     $tenantName.ciamlogin.com ($tenantId)"
Write-Host "  Web App:    $webAppName → $webClientId"
Write-Host "  MAUI App:   $mauiAppName → $mauiClientId"
Write-Host "  API Scope:  api://$webClientId/access_as_user"
Write-Host ""
Write-Host "  Config files patched:"
Write-Host "    • MauiBlazorWebEntra.Web/appsettings.json"
Write-Host "    • MauiBlazorWebEntra/Services/MsalConfig.cs"
Write-Host "    • MauiBlazorWebEntra/Platforms/Android/MsalActivity.cs"
Write-Host "    • MauiBlazorWebEntra/Platforms/iOS/Info.plist"
Write-Host ""
Write-Host "  Next:" -ForegroundColor Blue
Write-Host "    cd MauiBlazorWebEntra.Web && dotnet run --launch-profile https"
Write-Host "    # Then build and deploy the MAUI app"
Write-Host ""
Write-Host "  To clean up: pwsh ./Teardown-Azure.ps1"
Write-Host ""
