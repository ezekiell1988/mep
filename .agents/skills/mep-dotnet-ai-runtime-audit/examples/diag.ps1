#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

param(
    [Parameter(Mandatory)]
    [ValidateSet('clear', 'event')]
    [string]$Action,

    [string]$BaseUrl,
    [string]$DiagPath,

    # Para action=event
    [string]$Category = 'Manual',
    [string]$Intent   = 'Test manual',
    [string]$Result   = '✅ ok'
)

# Cargar config del proyecto si existe en el directorio actual (raíz del repo)
$configFile = Join-Path (Get-Location) 'audit.config.ps1'
if (Test-Path $configFile) { . $configFile }

if (-not $BaseUrl)  { $BaseUrl  = if ($null -ne $AuditConfig) { $AuditConfig.BaseUrl }  else { 'http://localhost:8000' } }
if (-not $DiagPath) { $DiagPath = if ($null -ne $AuditConfig) { $AuditConfig.DiagPath } else { '/api/diag' } }

$auditUrl = "$BaseUrl$DiagPath/audit"
$eventUrl = "$BaseUrl$DiagPath/audit-event"

switch ($Action) {
    'clear' {
        Write-Host "Limpiando audit en $auditUrl ..." -ForegroundColor Yellow
        $r = Invoke-WebRequest -Method DELETE -Uri $auditUrl -ErrorAction Stop
        Write-Host "OK ($($r.StatusCode))" -ForegroundColor Green
    }
    'event' {
        $body = @{ type = 'event'; category = $Category; intent = $Intent; result = $Result } | ConvertTo-Json
        Write-Host "Enviando evento: $Category / $Intent" -ForegroundColor Cyan
        $r = Invoke-WebRequest -Method POST -Uri $eventUrl -Body $body -ContentType 'application/json' -ErrorAction Stop
        Write-Host "OK ($($r.StatusCode))" -ForegroundColor Green
    }
}
