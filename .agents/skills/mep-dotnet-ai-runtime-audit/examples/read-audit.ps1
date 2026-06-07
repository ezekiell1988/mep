#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

param(
    [string]$AuditFile,
    [string]$Filter,
    [int]$Tail = 0
)

# Cargar config del proyecto si existe en el directorio actual (raíz del repo)
$configFile = Join-Path (Get-Location) 'audit.config.ps1'
if (Test-Path $configFile) { . $configFile }

if (-not $AuditFile) {
    $AuditFile = if ($null -ne $AuditConfig) { $AuditConfig.AuditFile } else { 'logs/llm-audit.md' }
}

if (-not (Test-Path $AuditFile)) {
    Write-Host "No existe el archivo: $AuditFile" -ForegroundColor Yellow
    exit 0
}

$content = Get-Content $AuditFile -Raw

if ($Filter) {
    $blocks = $content -split '(?=\n## \[)'
    $content = ($blocks | Where-Object { $_ -match [regex]::Escape($Filter) }) -join ''
}

if ($Tail -gt 0) {
    $lines = $content -split "`n"
    $content = ($lines | Select-Object -Last $Tail) -join "`n"
}

$content -split "`n" | ForEach-Object {
    $line = $_
    if ($line -match '^\#\# \[ERROR\]') {
        Write-Host $line -ForegroundColor Red
    } elseif ($line -match '^\#\# \[STARTUP\]') {
        Write-Host $line -ForegroundColor Cyan
    } elseif ($line -match '^\#\# \[DECISION\]') {
        Write-Host $line -ForegroundColor Yellow
    } elseif ($line -match '^\#\# \[EVENT\]') {
        Write-Host $line -ForegroundColor Green
    } elseif ($line -match '❌') {
        Write-Host $line -ForegroundColor Red
    } elseif ($line -match '✅') {
        Write-Host $line -ForegroundColor Green
    } else {
        Write-Host $line
    }
}
