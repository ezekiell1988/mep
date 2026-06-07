#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

param(
    [string]$AuditFile,
    [int]$IntervalMs = 1000
)

# Cargar config del proyecto si existe en el directorio actual (raíz del repo)
$configFile = Join-Path (Get-Location) 'audit.config.ps1'
if (Test-Path $configFile) { . $configFile }

if (-not $AuditFile) {
    $AuditFile = if ($null -ne $AuditConfig) { $AuditConfig.AuditFile } else { 'logs/llm-audit.md' }
}

Write-Host "Watching: $AuditFile  (Ctrl+C para detener)" -ForegroundColor Cyan

$lastSize = 0
while ($true) {
    if (Test-Path $AuditFile) {
        $info = Get-Item $AuditFile
        if ($info.Length -gt $lastSize) {
            $bytes = $info.Length - $lastSize
            $content = [System.IO.File]::ReadAllText($AuditFile, [System.Text.Encoding]::UTF8)
            $newContent = $content.Substring([int][Math]::Max(0, $content.Length - $bytes))
            $newContent -split "`n" | ForEach-Object {
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
            $lastSize = $info.Length
        }
    } else {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') Esperando archivo..." -ForegroundColor DarkGray
    }
    Start-Sleep -Milliseconds $IntervalMs
}
