#!/usr/bin/env pwsh
# deploy.ps1 — Deploy completo de AulaIA a Azure Container Apps
# Uso: ./deploy.ps1 [[-Tag] <string>]
#   -Tag  Tag adicional para la imagen (default: SHA corto del commit actual)

param(
    [string]$Tag = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AcrName    = "acrdemoitqs"
$AcrRegistry = "$AcrName.azurecr.io"
$ImageName  = "aulaia-api"
$ContainerApp = "ca-aulaia-api"
$ResourceGroup = "rg-ezequiel"
$TenantId   = "2f80d4e1-da0e-4b6d-84da-30f67e280e4b"
$SubscriptionId = "d9a8cd11-1beb-4255-a890-72797ac44a61"

# ---------------------------------------------------------------------------
# 0. Verificar que estamos en la raíz del repo
# ---------------------------------------------------------------------------
$Dockerfile = Join-Path $PSScriptRoot "../../../../Dockerfile"
if (-not (Test-Path $Dockerfile)) {
    Write-Error "Ejecutar desde la raiz del repo o asegurarse de que existe Dockerfile en la raiz."
}

# ---------------------------------------------------------------------------
# 1. Verificar login de Azure
# ---------------------------------------------------------------------------
Write-Host "`n[1/4] Verificando sesion de Azure..." -ForegroundColor Cyan

$account = az account show --query "{sub:id, tenant:tenantId}" -o json 2>&1 | ConvertFrom-Json
if ($account.tenant -ne $TenantId -or $account.sub -ne $SubscriptionId) {
    Write-Host "  No hay sesion activa en el tenant correcto. Iniciando az login..." -ForegroundColor Yellow
    az login --tenant $TenantId
    az account set --subscription $SubscriptionId
}
Write-Host "  OK — tenant: $($account.tenant) / sub: $($account.sub)" -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2. Login al ACR
# ---------------------------------------------------------------------------
Write-Host "`n[2/4] Login al ACR ($AcrRegistry)..." -ForegroundColor Cyan
az acr login --name $AcrName
Write-Host "  OK" -ForegroundColor Green

# ---------------------------------------------------------------------------
# 3. Build y push de la imagen (linux/amd64 obligatorio en Mac Apple Silicon)
# ---------------------------------------------------------------------------
Write-Host "`n[3/4] Build y push de la imagen..." -ForegroundColor Cyan

# Determinar tag
if ($Tag -eq "") {
    $Tag = git rev-parse --short HEAD 2>&1
    if ($LASTEXITCODE -ne 0) {
        $Tag = (Get-Date -Format "yyyyMMdd-HHmmss")
        Write-Host "  No es un repo Git. Usando timestamp como tag: $Tag" -ForegroundColor Yellow
    }
}

$ImageLatest = "${AcrRegistry}/${ImageName}:latest"
$ImageTagged = "${AcrRegistry}/${ImageName}:${Tag}"

Write-Host "  Tags: latest + $Tag" -ForegroundColor Gray

# Resolver ruta raiz del repo
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "../../../..")

docker buildx build `
    --platform linux/amd64 `
    --tag $ImageLatest `
    --tag $ImageTagged `
    --push `
    $RepoRoot

if ($LASTEXITCODE -ne 0) {
    Write-Error "docker buildx build fallo. Revisa los logs anteriores."
}
Write-Host "  OK — imagen pusheada como $ImageTagged" -ForegroundColor Green

# ---------------------------------------------------------------------------
# 4. Actualizar el Container App
# ---------------------------------------------------------------------------
Write-Host "`n[4/4] Actualizando Container App ($ContainerApp)..." -ForegroundColor Cyan

az containerapp update `
    --name $ContainerApp `
    --resource-group $ResourceGroup `
    --image $ImageTagged

if ($LASTEXITCODE -ne 0) {
    Write-Error "az containerapp update fallo."
}

Write-Host "`n Deploy completado exitosamente." -ForegroundColor Green
Write-Host "  Imagen: $ImageTagged"
Write-Host "  Container App: $ContainerApp"
Write-Host ""
Write-Host "Ejecuta ./verify.ps1 para verificar que el servicio levanto bien." -ForegroundColor Cyan
