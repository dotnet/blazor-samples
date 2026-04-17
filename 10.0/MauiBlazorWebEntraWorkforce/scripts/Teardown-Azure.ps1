<#
.SYNOPSIS
    Deletes the app registrations created for the
    MauiBlazorWebEntraWorkforce sample.
#>

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "=== MauiBlazorWebEntraWorkforce - Azure Teardown ===" -ForegroundColor Blue
Write-Host ""

$sampleRoot = Join-Path $PSScriptRoot ".."
$dataPath = Join-Path $sampleRoot ".azure-setup.json"

if (-not (Test-Path $dataPath)) {
    Write-Error "Setup data file not found: $dataPath"
    exit 1
}

$data = Get-Content $dataPath -Raw | ConvertFrom-Json
$tenantTarget = if ($data.tenantDomain) { $data.tenantDomain } else { $data.tenantId }

Write-Host "Signing in to tenant: $tenantTarget" -ForegroundColor Cyan
az login --tenant $tenantTarget --allow-no-subscriptions 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to sign in to tenant '$tenantTarget'."
    exit 1
}

Write-Host "This will delete these app registrations:" -ForegroundColor Yellow
Write-Host "  Web App:  $($data.webClientId)"
Write-Host "  MAUI App: $($data.mauiClientId)"
Write-Host ""
Write-Host "Continue? (y/N): " -ForegroundColor Magenta -NoNewline
$confirm = Read-Host

if ($confirm -notmatch "^[Yy]$") {
    Write-Host "Cancelled."
    exit 0
}

az ad app delete --id $data.mauiClientId 2>&1 | Out-Null
az ad app delete --id $data.webClientId 2>&1 | Out-Null
Remove-Item $dataPath -Force

Write-Host ""
Write-Host "Teardown complete." -ForegroundColor Green
