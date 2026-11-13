<#
.SYNOPSIS
    Tears down Azure resources created by Setup-Azure.ps1 for the MauiBlazorWebEntra sample.

.DESCRIPTION
    Deletes the app registrations created during setup. Reads the saved
    .azure-setup.json file to find the app IDs to delete.

.NOTES
    Prerequisites:
    - Azure CLI (az) installed and logged in to the correct tenant
    - .azure-setup.json file (created by Setup-Azure.ps1)
#>

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "=== MauiBlazorWebEntra — Azure Teardown ===" -ForegroundColor Blue
Write-Host ""

$scriptDir = Join-Path $PSScriptRoot ".."
$dataPath = Join-Path $scriptDir ".azure-setup.json"

if (-not (Test-Path $dataPath)) {
    Write-Error "Setup data file not found: $dataPath`nRun Setup-Azure.ps1 first, or delete resources manually in the Azure portal."
    exit 1
}

$data = Get-Content $dataPath -Raw | ConvertFrom-Json

# Ensure we're logged into the correct CIAM tenant
Write-Host "A browser window will open to log in to the CIAM tenant:"
Write-Host "  $($data.tenantName).onmicrosoft.com" -ForegroundColor Blue
Write-Host ""
Write-Host "Press Enter to continue..." -ForegroundColor Magenta -NoNewline
Read-Host

az login --tenant "$($data.tenantName).onmicrosoft.com" --allow-no-subscriptions 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to login to tenant '$($data.tenantName).onmicrosoft.com'."
    exit 1
}

$account = az account show 2>$null | ConvertFrom-Json
Write-Host "  ✓ Logged in to tenant: $($account.tenantId)" -ForegroundColor Green
Write-Host ""

Write-Host "This will delete the following app registrations:"
Write-Host "  Web App:  $($data.webClientId)" -ForegroundColor Blue
Write-Host "  MAUI App: $($data.mauiClientId)" -ForegroundColor Blue
Write-Host ""

Write-Host "Are you sure you want to proceed? (y/N): " -ForegroundColor Magenta -NoNewline
$confirm = Read-Host
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Cancelled."
    exit 0
}

# Delete MAUI app registration
Write-Host ""
Write-Host "Deleting MAUI app registration ($($data.mauiClientId))..."
az ad app delete --id $data.mauiClientId 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Deleted." -ForegroundColor Green
} else {
    Write-Warning "Failed to delete MAUI app. It may have already been deleted."
}

# Delete Web app registration
Write-Host "Deleting web app registration ($($data.webClientId))..."
az ad app delete --id $data.webClientId 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Deleted." -ForegroundColor Green
} else {
    Write-Warning "Failed to delete web app. It may have already been deleted."
}

# Clean up local files
Remove-Item $dataPath -Force
Write-Host ""
Write-Host "=== Teardown Complete ✓ ===" -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: The configuration placeholders in appsettings.json and MsalConfig.cs" -ForegroundColor Magenta
Write-Host "were NOT reverted. You can re-run Setup-Azure.ps1 to configure new resources," -ForegroundColor Magenta
Write-Host "or manually restore the placeholder values." -ForegroundColor Magenta
Write-Host ""
