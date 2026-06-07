#!/usr/bin/env pwsh
# connect.ps1 — Abre una sesion psql interactiva contra la BD del proyecto AulaIA
# Lee credenciales desde src/AulaIA.Api/appsettings.Development.json
# Uso: ./connect.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Resolver raiz del repo (4 niveles arriba de .agents/skills/mep-db-access/examples/)
# ---------------------------------------------------------------------------
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "../../../../")
$SettingsFile = Join-Path $RepoRoot "src/AulaIA.Api/appsettings.Development.json"

if (-not (Test-Path $SettingsFile)) {
    Write-Error "No se encontro $SettingsFile. Ejecutar desde la raiz del repo o verificar la ruta."
}

# ---------------------------------------------------------------------------
# Parsear ConnectionString desde appsettings.Development.json
# ---------------------------------------------------------------------------
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

if (-not $DbHost -or -not $DbName -or -not $DbUser -or -not $DbPass) {
    Write-Error "No se pudo parsear la ConnectionString. Verificar $SettingsFile"
}

Write-Host "`nConectando a PostgreSQL..." -ForegroundColor Cyan
Write-Host "  Host: $DbHost:$DbPort"
Write-Host "  DB:   $DbName"
Write-Host "  User: $DbUser`n"

# ---------------------------------------------------------------------------
# Verificar que psql esta disponible
# ---------------------------------------------------------------------------
$psql = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psql) {
    # Intentar ruta Homebrew libpq (macOS Apple Silicon)
    $brewPsql = "/opt/homebrew/opt/libpq/bin/psql"
    if (Test-Path $brewPsql) {
        $psql = $brewPsql
    } else {
        Write-Error "psql no encontrado. Instalar con: brew install libpq`nLuego agregar a PATH: export PATH=`"/opt/homebrew/opt/libpq/bin:`$PATH`""
    }
} else {
    $psql = $psql.Source
}

# ---------------------------------------------------------------------------
# Conectar
# ---------------------------------------------------------------------------
$env:PGPASSWORD = $DbPass
& $psql -h $DbHost -p $DbPort -U $DbUser -d $DbName
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
