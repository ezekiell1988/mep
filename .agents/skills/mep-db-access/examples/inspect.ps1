#!/usr/bin/env pwsh
# inspect.ps1 — Inspecciona tablas clave de AulaIA: curriculum_units, planeamientos, asistencia
# Lee credenciales desde src/AulaIA.Api/appsettings.Development.json
# Uso: ./inspect.ps1 [-Table <curriculum|planeamientos|asistencia|all>]

param(
    [ValidateSet("curriculum", "planeamientos", "asistencia", "all")]
    [string]$Table = "all"
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

function Run-Query([string]$label, [string]$sql) {
    Write-Host "`n=== $label ===" -ForegroundColor Cyan
    $env:PGPASSWORD = $DbPass
    & $psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -c $sql
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

# ---------------------------------------------------------------------------
# Queries por tabla
# ---------------------------------------------------------------------------
$queries = @{
    curriculum = @(
        @{
            Label = "curriculum_units — resumen por ciclo"
            Sql   = @"
SELECT "Ciclo", COUNT(*) AS unidades, SUM("TokensUsed") AS tokens_total
FROM curriculum_units
GROUP BY "Ciclo"
ORDER BY "Ciclo";
"@
        },
        @{
            Label = "curriculum_units — detalle con conteo JSONB"
            Sql   = @"
SELECT
  "Ciclo", "Nivel", "Trimestre", "UnidadNumero", "UnidadNombre",
  "TokensUsed",
  jsonb_array_length("AprendizajesEsperados") AS aprendizajes,
  jsonb_array_length("IndicadoresEvaluacion") AS indicadores,
  "ValidatedAt" IS NOT NULL AS validada
FROM curriculum_units
ORDER BY "Ciclo", "Nivel", "Trimestre", "UnidadNumero";
"@
        }
    )
    planeamientos = @(
        @{
            Label = "planeamientos — ultimos 20"
            Sql   = @"
SELECT id, "DocenteId", "Asignatura", "Nivel", "Trimestre", "Estado", "CreatedAt"
FROM planeamientos
ORDER BY "CreatedAt" DESC
LIMIT 20;
"@
        }
    )
    asistencia = @(
        @{
            Label = "asistencia — registros por grupo"
            Sql   = @"
SELECT g."Nombre" AS grupo, COUNT(a.id) AS registros
FROM asistencia a
JOIN grupos g ON g."Id" = a."GrupoId"
GROUP BY g."Nombre";
"@
        }
    )
}

# ---------------------------------------------------------------------------
# Ejecutar segun -Table
# ---------------------------------------------------------------------------
$tables = if ($Table -eq "all") { @("curriculum", "planeamientos", "asistencia") } else { @($Table) }

foreach ($t in $tables) {
    foreach ($q in $queries[$t]) {
        Run-Query $q.Label $q.Sql
    }
}

Write-Host "`nInspeccion completada." -ForegroundColor Green
