#!/usr/bin/env pwsh
# cleanup.ps1 — Limpia imagenes locales de Docker generadas durante el deploy
# Uso: ./cleanup.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AcrRegistry = "acrdemoitqs.azurecr.io"
$ImageName   = "aulaia-api"

Write-Host "`n[1/2] Eliminando imagenes locales de ${AcrRegistry}/${ImageName}..." -ForegroundColor Cyan

$imageIds = docker images "${AcrRegistry}/${ImageName}" --format "{{.ID}}" 2>&1
if ($imageIds) {
    $imageIds | ForEach-Object {
        Write-Host "  Eliminando ID: $_" -ForegroundColor Gray
        docker rmi -f $_ 2>&1 | Out-Null
    }
    Write-Host "  OK" -ForegroundColor Green
} else {
    Write-Host "  No hay imagenes locales de $ImageName." -ForegroundColor Gray
}

Write-Host "`n[2/2] Limpiando capas intermedias huerfanas (dangling)..." -ForegroundColor Cyan
docker image prune -f
Write-Host "  OK" -ForegroundColor Green

Write-Host "`nLimpieza completada." -ForegroundColor Green
