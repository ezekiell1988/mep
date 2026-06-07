#!/usr/bin/env pwsh
# cleanup-dev.ps1 — Limpia datos de prueba en la BD de desarrollo de AulaIA
# Lee credenciales desde src/AulaIA.Api/appsettings.Development.json
#
# ATENCION: Este script modifica datos. Solo usar en entornos de desarrollo.
#
# Uso: ./cleanup-dev.ps1 [-Target <curriculum-unvalidated|all>] [-Confirm]
#   -Target  Qué limpiar (default: curriculum-unvalidated)
#   -Confirm Omite el prompt interactivo de confirmacion

param(
    [ValidateSet("curriculum-unvalidated", "all")]
    [string]$Target = "curriculum-unvalidated",

    [switch]$Confirm
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Resolver raiz del repo
# ---------------------------------------------------------------------------
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "../../../../")
$SettingsFile = Join-Path $RepoRoot "src/AulaIA.Api/appsettings.Development.json"

if (-not (Test-Path $SettingsFile)) {
    Write-Error "No se encontro $SettingsFile."
}

$settings = Get-Content $SettingsFile -Raw | ConvertFrom-Json
$cs = $settings.Database.ConnectionString

function Get-CsPart([string]$cs, [string]$key) {
    foreach ($part in $cs -split ";") {
        if ($part -match "^\s*${key}\s*=\s*(.+)$") { return $Matches[1].Trim() }
    }
    return $null
}

$DbHost = Get-CsPart $cs "Host"
$DbPort = Get-CsPart $cs "Port"
$DbName = Get-CsPart $cs "Database"
$DbUser = Get-CsPart $cs "Username"
$DbPass = Get-CsPart $cs "Password"

$psql = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psql) {
    $brewPsql = "/opt/homebrew/opt/libpq/bin/psql"
    if (Test-Path $brewPsql) { $psql = $brewPsql }
    else { Write-Error "psql no encontrado. Instalar con: brew install libpq" }
} else {
    $psql = $psql.Source
}

function Run-Sql([string]$sql) {
    $env:PGPASSWORD = $DbPass
    & $psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -c $sql
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

# ---------------------------------------------------------------------------
# Definir operaciones por target
# ---------------------------------------------------------------------------
$operations = @{
    "curriculum-unvalidated" = @{
        Description = "Eliminar curriculum_units donde ValidatedAt IS NULL"
        Sql         = 'DELETE FROM curriculum_units WHERE "ValidatedAt" IS NULL;'
    }
    "all" = @{
        Description = "TRUNCATE: curriculum_units, planeamientos, asistencia (CASCADE)"
        Sql         = "TRUNCATE curriculum_units, planeamientos, asistencia RESTART IDENTITY CASCADE;"
    }
}

$op = $operations[$Target]

# ---------------------------------------------------------------------------
# Confirmacion
# ---------------------------------------------------------------------------
Write-Host "`n[ADVERTENCIA] Operacion destructiva en la BD de desarrollo" -ForegroundColor Yellow
Write-Host "  BD:     $DbName @ $DbHost"
Write-Host "  Accion: $($op.Description)" -ForegroundColor Red

if (-not $Confirm) {
    $answer = Read-Host "`nEscribir 'si' para confirmar"
    if ($answer -ne "si") {
        Write-Host "Operacion cancelada." -ForegroundColor Gray
        exit 0
    }
}

# ---------------------------------------------------------------------------
# Ejecutar
# ---------------------------------------------------------------------------
Write-Host "`nEjecutando limpieza..." -ForegroundColor Cyan
Run-Sql $op.Sql

Write-Host "`nLimpieza completada: $($op.Description)" -ForegroundColor Green
