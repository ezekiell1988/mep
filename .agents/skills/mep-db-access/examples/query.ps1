#!/usr/bin/env pwsh
# query.ps1 — Ejecuta una consulta SQL contra la BD de AulaIA y muestra el resultado
# Lee credenciales desde src/AulaIA.Api/appsettings.Development.json
# Uso: ./query.ps1 -Sql "SELECT ..."
#      ./query.ps1 -Sql "SELECT ..." -Format csv
#
# Formatos disponibles: table (default), csv, json, unaligned

param(
    [Parameter(Mandatory = $true)]
    [string]$Sql,

    [ValidateSet("table", "csv", "json", "unaligned")]
    [string]$Format = "table"
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

# ---------------------------------------------------------------------------
# Verificar psql
# ---------------------------------------------------------------------------
$psql = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psql) {
    $brewPsql = "/opt/homebrew/opt/libpq/bin/psql"
    if (Test-Path $brewPsql) { $psql = $brewPsql }
    else { Write-Error "psql no encontrado. Instalar con: brew install libpq" }
} else {
    $psql = $psql.Source
}

# ---------------------------------------------------------------------------
# Mapear formato a flags de psql
# ---------------------------------------------------------------------------
$formatFlags = switch ($Format) {
    "table"     { @("--pset=format=aligned") }
    "csv"       { @("--csv") }
    "json"      { @("--pset=format=unaligned", "--tuples-only", "--no-align") }
    "unaligned" { @("--pset=format=unaligned") }
}

# ---------------------------------------------------------------------------
# Ejecutar
# ---------------------------------------------------------------------------
$env:PGPASSWORD = $DbPass
& $psql -h $DbHost -p $DbPort -U $DbUser -d $DbName @formatFlags -c $Sql
$exit = $LASTEXITCODE
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue

exit $exit
