#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

param(
    [string]$AuditFile,
    [string]$BaseUrl,
    [string]$DiagPath,
    [string]$TestsPath,
    [string]$TestFilter,
    [switch]$SkipClear
)

# Cargar config del proyecto si existe en el directorio actual (raíz del repo)
$configFile = Join-Path (Get-Location) 'audit.config.ps1'
if (Test-Path $configFile) { . $configFile }

if (-not $AuditFile)  { $AuditFile  = if ($null -ne $AuditConfig) { $AuditConfig.AuditFile }  else { 'logs/llm-audit.md' } }
if (-not $BaseUrl)    { $BaseUrl    = if ($null -ne $AuditConfig) { $AuditConfig.BaseUrl }    else { 'http://localhost:8000' } }
if (-not $DiagPath)   { $DiagPath   = if ($null -ne $AuditConfig) { $AuditConfig.DiagPath }   else { '/api/diag' } }
if (-not $TestsPath)  { $TestsPath  = if ($null -ne $AuditConfig) { $AuditConfig.TestsPath }  else { 'tests' } }

Write-Host "`n=== DEBUG JOB PROTOCOL ===" -ForegroundColor Cyan
Write-Host "AuditFile : $AuditFile"
Write-Host "BaseUrl   : $BaseUrl"
Write-Host "TestsPath : $TestsPath"
if ($TestFilter) { Write-Host "TestFilter: $TestFilter" }

# PASO 1 — Limpiar audit
if (-not $SkipClear) {
    Write-Host "`n[1] Limpiando audit..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Method DELETE -Uri "$BaseUrl$DiagPath/audit" -ErrorAction Stop | Out-Null
        Write-Host "    OK — audit limpio" -ForegroundColor Green
    } catch {
        Write-Host "    WARN: No se pudo limpiar via HTTP — limpiando archivo directamente" -ForegroundColor Yellow
        if (Test-Path $AuditFile) {
            $header = "# LLM Audit Log`nGenerated: $(Get-Date -Format 'o')`n`n---`n"
            Set-Content -Path $AuditFile -Value $header -Encoding UTF8
            Write-Host "    OK — archivo truncado" -ForegroundColor Green
        }
    }
}

# PASO 2 — Ejecutar tests
Write-Host "`n[2] Ejecutando tests en $TestsPath ..." -ForegroundColor Yellow
$dotnetArgs = @('test', $TestsPath, '--no-build', '--verbosity', 'minimal')
if ($TestFilter) { $dotnetArgs += @('--filter', $TestFilter) }

$exitCode = 0
try {
    & dotnet @dotnetArgs
    $exitCode = $LASTEXITCODE
} catch {
    $exitCode = 1
    Write-Host "    ERROR al ejecutar dotnet test: $_" -ForegroundColor Red
}

# PASO 3 — Mostrar evidencia del audit
Write-Host "`n[3] Evidencia del audit:" -ForegroundColor Yellow
if (Test-Path $AuditFile) {
    $content = Get-Content $AuditFile -Raw
    $content -split "`n" | ForEach-Object {
        $line = $_
        if ($line -match '^\#\# \[ERROR\]' -or $line -match '❌') {
            Write-Host $line -ForegroundColor Red
        } elseif ($line -match '^\#\# \[STARTUP\]') {
            Write-Host $line -ForegroundColor Cyan
        } elseif ($line -match '^\#\# \[DECISION\]') {
            Write-Host $line -ForegroundColor Yellow
        } elseif ($line -match '✅') {
            Write-Host $line -ForegroundColor Green
        } else {
            Write-Host $line
        }
    }
} else {
    Write-Host "    No hay archivo de audit en: $AuditFile" -ForegroundColor DarkGray
}

Write-Host "`n=== FIN (exitCode=$exitCode) ===" -ForegroundColor Cyan
exit $exitCode
