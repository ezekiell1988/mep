#!/usr/bin/env pwsh
# verify.ps1 — Verifica que el Container App levanto correctamente tras un deploy
# Uso: ./verify.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Fqdn = "https://ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io"

$Checks = @(
    @{ Label = "API /health";   Url = "$Fqdn/health";    ExpectedCode = 200; ExpectedBody = "healthy" }
    @{ Label = "SPA /";         Url = "$Fqdn/";           ExpectedCode = 200; ExpectedBody = $null }
    @{ Label = "SPA /grupos";   Url = "$Fqdn/grupos";     ExpectedCode = 200; ExpectedBody = $null }
    @{ Label = "Scalar /scalar";Url = "$Fqdn/scalar";     ExpectedCode = 200; ExpectedBody = $null }
)

Write-Host "`nVerificando endpoints de $Fqdn`n" -ForegroundColor Cyan

$allPassed = $true

foreach ($check in $Checks) {
    try {
        $response = Invoke-WebRequest -Uri $check.Url -UseBasicParsing -TimeoutSec 15 -ErrorAction Stop
        $code = $response.StatusCode

        if ($code -ne $check.ExpectedCode) {
            Write-Host "  [FAIL] $($check.Label) — HTTP $code (esperado $($check.ExpectedCode))" -ForegroundColor Red
            $allPassed = $false
            continue
        }

        if ($check.ExpectedBody -and $response.Content -notmatch $check.ExpectedBody) {
            Write-Host "  [FAIL] $($check.Label) — HTTP $code OK pero body no contiene '$($check.ExpectedBody)'" -ForegroundColor Red
            Write-Host "         Body: $($response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)))" -ForegroundColor Gray
            $allPassed = $false
            continue
        }

        Write-Host "  [ OK ] $($check.Label) — HTTP $code" -ForegroundColor Green
    }
    catch {
        Write-Host "  [FAIL] $($check.Label) — Error: $($_.Exception.Message)" -ForegroundColor Red
        $allPassed = $false
    }
}

if (-not $allPassed) {
    Write-Host "`nAlgunos checks fallaron. Mostrando ultimas 50 lineas de logs del Container App..." -ForegroundColor Yellow
    az containerapp logs show `
        --name ca-aulaia-api `
        --resource-group rg-ezequiel `
        --tail 50
    exit 1
}

Write-Host "`nTodos los checks pasaron. El deploy es exitoso." -ForegroundColor Green
