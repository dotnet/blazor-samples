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
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " MauiBlazorWebEntra - Azure Teardown" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptDir = $PSScriptRoot
$dataPath = Join-Path $scriptDir ".azure-setup.json"

if (-not (Test-Path $dataPath)) {
    Write-Error "Setup data file not found: $dataPath`nRun Setup-Azure.ps1 first, or delete resources manually in the Azure portal."
    exit 1
}

$data = Get-Content $dataPath -Raw | ConvertFrom-Json

Write-Host "This will delete the following app registrations:" -ForegroundColor Yellow
Write-Host "  Web App:  $($data.webClientId)" -ForegroundColor White
Write-Host "  MAUI App: $($data.mauiClientId)" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Are you sure you want to proceed? (y/N)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

# Delete MAUI app registration
Write-Host "Deleting MAUI app registration ($($data.mauiClientId))..." -ForegroundColor Yellow
az ad app delete --id $data.mauiClientId 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Deleted." -ForegroundColor Green
} else {
    Write-Warning "Failed to delete MAUI app. It may have already been deleted."
}

# Delete Web app registration
Write-Host "Deleting web app registration ($($data.webClientId))..." -ForegroundColor Yellow
az ad app delete --id $data.webClientId 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Deleted." -ForegroundColor Green
} else {
    Write-Warning "Failed to delete web app. It may have already been deleted."
}

# Clean up local files
Remove-Item $dataPath -Force
Write-Host ""
Write-Host "Teardown complete. Local .azure-setup.json removed." -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: The configuration placeholders in appsettings.json and MsalConfig.cs" -ForegroundColor Yellow
Write-Host "were NOT reverted. You can re-run Setup-Azure.ps1 to configure new resources," -ForegroundColor Yellow
Write-Host "or manually restore the placeholder values." -ForegroundColor Yellow
