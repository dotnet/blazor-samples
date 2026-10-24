<#
.SYNOPSIS
    Configures Microsoft Entra workforce authentication for the
    MauiBlazorWebEntraWorkforce sample.

.DESCRIPTION
    This interactive script:
      1. Signs in to an existing workforce tenant
      2. Creates or reuses the web app registration
      3. Creates or reuses the MAUI public client registration
      4. Grants the MAUI app permission to call the web API
      5. Patches local config files with the generated values

.NOTES
    Prerequisites:
    - Azure CLI (az) installed
    - PowerShell Core 7+
    - Permission to create app registrations in the target tenant
#>

$ErrorActionPreference = 'Stop'

function Write-Step {
    param([int]$Number, [string]$Title, [string]$Type = "AUTO")
    Write-Host ""
    Write-Host "=== Step $Number - $Title [$Type] ===" -ForegroundColor Blue
}

function Read-RequiredInput {
    param([string]$Prompt)

    while ($true) {
        Write-Host $Prompt -ForegroundColor Magenta -NoNewline
        $value = Read-Host
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value.Trim()
        }

        Write-Host "This value is required." -ForegroundColor Red
    }
}

function Ensure-ServicePrincipal {
    param([string]$AppId)

    az ad sp create --id $AppId 2>&1 | Out-Null
}

function New-AppSecret {
    param(
        [string]$ApplicationObjectId,
        [string]$DisplayName
    )

    $body = @{
        passwordCredential = @{
            displayName = $DisplayName
        }
    } | ConvertTo-Json -Depth 5 -Compress

    $json = az rest --method POST `
        --uri "https://graph.microsoft.com/v1.0/applications/$ApplicationObjectId/addPassword" `
        --headers "Content-Type=application/json" `
        --body $body 2>$null

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create client secret."
    }

    return ($json | ConvertFrom-Json).secretText
}

Write-Host ""
Write-Host "=== MauiBlazorWebEntraWorkforce - Azure Setup ===" -ForegroundColor Blue
Write-Host ""

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "Azure CLI (az) is not installed." -ForegroundColor Red
    Write-Host "Install it from: https://learn.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Blue
    exit 1
}

Write-Host "Azure CLI found." -ForegroundColor Green

$sampleRoot = Join-Path $PSScriptRoot ".."
$webAppName = "MauiBlazorWebEntraWorkforce-Web"
$mauiAppName = "MauiBlazorWebEntraWorkforce-MAUI"

Write-Step 1 "Sign in to workforce tenant" "AUTO"

Write-Host ""
Write-Host "Use an existing Microsoft Entra workforce tenant." -ForegroundColor Cyan
$tenantInput = Read-RequiredInput "Tenant domain or tenant ID (for example, contoso.onmicrosoft.com): "

Write-Host ""
Write-Host "A browser window may open for sign-in..." -ForegroundColor Cyan
az login --tenant $tenantInput --allow-no-subscriptions 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to sign in to tenant '$tenantInput'."
    exit 1
}

$account = az account show 2>$null | ConvertFrom-Json
$tenantId = $account.tenantId

$org = az rest `
    --method GET `
    --uri "https://graph.microsoft.com/v1.0/organization?`$select=displayName,verifiedDomains" 2>$null | ConvertFrom-Json

$tenantDomain = $null
if ($org.value -and $org.value.Count -gt 0) {
    $tenantDomain = $org.value[0].verifiedDomains |
        Where-Object { $_.isDefault -eq $true } |
        Select-Object -ExpandProperty name -First 1
}
if (-not $tenantDomain) {
    $tenantDomain = $tenantInput
}

Write-Host "  Tenant ID:     $tenantId" -ForegroundColor Green
Write-Host "  Tenant domain: $tenantDomain" -ForegroundColor Green

Write-Step 2 "Create web app registration" "AUTO"

$existingWebApp = az ad app list --display-name $webAppName --query "[?displayName=='$webAppName'] | [0]" 2>$null | ConvertFrom-Json
if ($existingWebApp) {
    $webClientId = $existingWebApp.appId
    $webObjectId = $existingWebApp.id
    Write-Host "  Reusing web app: $webClientId" -ForegroundColor Green
} else {
    $webAppJson = az ad app create `
        --display-name $webAppName `
        --sign-in-audience "AzureADMyOrg" `
        --web-redirect-uris "https://localhost:7157/signin-oidc" "https://localhost:7157/signout-callback-oidc" `
        --enable-id-token-issuance true 2>$null

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create web app registration."
        exit 1
    }

    $webApp = $webAppJson | ConvertFrom-Json
    $webClientId = $webApp.appId
    $webObjectId = $webApp.id
    Write-Host "  Created web app: $webClientId" -ForegroundColor Green
}

Ensure-ServicePrincipal -AppId $webClientId

# Ensure redirect URIs are correct (idempotent — handles reuse with stale URIs)
$webRedirectBody = @{
    web = @{
        redirectUris = @(
            "https://localhost:7157/signin-oidc",
            "https://localhost:7157/signout-callback-oidc"
        )
        implicitGrantSettings = @{
            enableIdTokenIssuance = $true
        }
    }
} | ConvertTo-Json -Depth 5 -Compress

az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$webObjectId" `
    --headers "Content-Type=application/json" `
    --body $webRedirectBody 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to update web redirect URIs. Verify manually in the Azure portal."
}

$clientSecret = $null
$appSettingsPath = Join-Path $sampleRoot "MauiBlazorWebEntraWorkforce.Web" "appsettings.json"
if (Test-Path $appSettingsPath) {
    $existingSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $existingConfigClientId = $existingSettings.AzureAd.ClientId
    $existingConfigSecret = $existingSettings.AzureAd.ClientSecret

    if ($existingConfigClientId -eq $webClientId -and
        $existingConfigSecret -and
        -not $existingConfigSecret.StartsWith("YOUR_")) {
        Write-Host "Keep the existing client secret from appsettings.json? (Y/n): " -ForegroundColor Blue -NoNewline
        $keepSecret = Read-Host
        if ($keepSecret -eq "" -or $keepSecret -match "^[Yy]") {
            $clientSecret = $existingConfigSecret
            Write-Host "  Reusing saved client secret." -ForegroundColor Green
        }
    }
}

if (-not $clientSecret) {
    Write-Host "Create a new client secret for the web app? (Y/n): " -ForegroundColor Blue -NoNewline
    $createSecret = Read-Host
    if ($createSecret -eq "" -or $createSecret -match "^[Yy]") {
        $clientSecret = New-AppSecret -ApplicationObjectId $webObjectId -DisplayName "MauiBlazorWebEntraWorkforce Setup"
        Write-Host "  New client secret created." -ForegroundColor Green
    } else {
        $clientSecret = Read-RequiredInput "Paste the existing client secret: "
    }
}

$webAppDetails = az ad app show --id $webClientId 2>$null | ConvertFrom-Json
$existingScope = $webAppDetails.api.oauth2PermissionScopes |
    Where-Object { $_.value -eq "access_as_user" } |
    Select-Object -First 1

if ($existingScope) {
    $scopeId = $existingScope.id
    Write-Host "  Reusing API scope: api://$webClientId/access_as_user" -ForegroundColor Green
} else {
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
        --body $apiBody 2>&1 | Out-Null

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to expose the access_as_user scope on the web app."
        exit 1
    }

    Write-Host "  API scope created: api://$webClientId/access_as_user" -ForegroundColor Green
}

Write-Step 3 "Create MAUI app registration" "AUTO"

$existingMauiApp = az ad app list --display-name $mauiAppName --query "[?displayName=='$mauiAppName'] | [0]" 2>$null | ConvertFrom-Json
if ($existingMauiApp) {
    $mauiClientId = $existingMauiApp.appId
    $mauiObjectId = $existingMauiApp.id
    Write-Host "  Reusing MAUI app: $mauiClientId" -ForegroundColor Green
} else {
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
    Write-Host "  Created MAUI app: $mauiClientId" -ForegroundColor Green
}

Ensure-ServicePrincipal -AppId $mauiClientId

$redirectBody = @{
    publicClient = @{
        redirectUris = @(
            "msal$mauiClientId`://auth",
            "http://localhost"
        )
    }
} | ConvertTo-Json -Depth 5 -Compress

az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$mauiObjectId" `
    --headers "Content-Type=application/json" `
    --body $redirectBody 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to configure redirect URIs for the MAUI app."
    exit 1
}

Write-Host "  Redirect URIs configured." -ForegroundColor Green

Write-Step 4 "Grant API permission" "AUTO"

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
    --body $permissionBody 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to grant the MAUI app access to the web API scope."
    exit 1
}

az ad app permission admin-consent --id $mauiClientId 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Admin consent granted." -ForegroundColor Green
} else {
    Write-Warning "Admin consent may require a tenant administrator."
}

Write-Step 5 "Patch local configuration" "AUTO"

if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $appSettings.AzureAd.Instance = "https://login.microsoftonline.com/"
    $appSettings.AzureAd.Domain = $tenantDomain
    $appSettings.AzureAd.TenantId = $tenantId
    $appSettings.AzureAd.ClientId = $webClientId
    $appSettings.AzureAd.ClientSecret = $clientSecret
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "  Updated appsettings.json" -ForegroundColor Green
}

$msalConfigPath = Join-Path $sampleRoot "MauiBlazorWebEntraWorkforce" "MsalConfig.cs"
if (Test-Path $msalConfigPath) {
    $content = Get-Content $msalConfigPath -Raw
    $content = $content -replace 'YOUR_TENANT_DOMAIN', $tenantDomain
    $content = $content -replace 'YOUR_TENANT_ID', $tenantId
    $content = $content -replace 'YOUR_MAUI_CLIENT_ID', $mauiClientId
    $content = $content -replace 'YOUR_WEB_CLIENT_ID', $webClientId
    Set-Content $msalConfigPath $content -Encoding UTF8
    Write-Host "  Updated MsalConfig.cs" -ForegroundColor Green
}

$msalActivityPath = Join-Path $sampleRoot "MauiBlazorWebEntraWorkforce" "Platforms" "Android" "MsalActivity.cs"
if (Test-Path $msalActivityPath) {
    $content = Get-Content $msalActivityPath -Raw
    $content = $content -replace 'msalYOUR_MAUI_CLIENT_ID', "msal$mauiClientId"
    Set-Content $msalActivityPath $content -Encoding UTF8
}

foreach ($plistPath in @(
    (Join-Path $sampleRoot "MauiBlazorWebEntraWorkforce" "Platforms" "iOS" "Info.plist"),
    (Join-Path $sampleRoot "MauiBlazorWebEntraWorkforce" "Platforms" "MacCatalyst" "Info.plist")
)) {
    if (Test-Path $plistPath) {
        $content = Get-Content $plistPath -Raw
        $content = $content -replace 'msalYOUR_MAUI_CLIENT_ID', "msal$mauiClientId"
        Set-Content $plistPath $content -Encoding UTF8
    }
}

$teardownDataPath = Join-Path $sampleRoot ".azure-setup.json"
@{
    webClientId  = $webClientId
    mauiClientId = $mauiClientId
    tenantId     = $tenantId
    tenantDomain = $tenantDomain
} | ConvertTo-Json | Set-Content $teardownDataPath -Encoding UTF8

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host "  Tenant:    $tenantDomain ($tenantId)"
Write-Host "  Web App:   $webAppName -> $webClientId"
Write-Host "  MAUI App:  $mauiAppName -> $mauiClientId"
Write-Host "  API Scope: api://$webClientId/access_as_user"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Blue
Write-Host "  cd MauiBlazorWebEntraWorkforce.Web && dotnet run --launch-profile https"
Write-Host "  # Then build and deploy the MAUI app"
Write-Host ""
