#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

param(
    [string]$ConnectionString,
    [string]$SettingsFile,
    [string]$SettingsKeyPath,
    [int]$KeepLast     = 0,
    [string]$OlderThan = '',
    [switch]$All
)

# Cargar config del proyecto si existe en el directorio actual (raíz del repo)
$configFile = Join-Path (Get-Location) 'audit.config.ps1'
if (Test-Path $configFile) { . $configFile }

if (-not $ConnectionString) {
    if (-not $SettingsFile)    { $SettingsFile    = if ($null -ne $AuditConfig) { $AuditConfig.SettingsFile }   else { 'appsettings.Development.json' } }
    if (-not $SettingsKeyPath) { $SettingsKeyPath = if ($null -ne $AuditConfig) { $AuditConfig.SettingsKeyPath } else { 'ConnectionStrings.DefaultConnection' } }

    if (-not (Test-Path $SettingsFile)) {
        Write-Error "No se encontró $SettingsFile"
    }
    $settings = Get-Content $SettingsFile -Raw | ConvertFrom-Json
    $keys = $SettingsKeyPath -split '\.'
    $val = $settings
    foreach ($k in $keys) { $val = $val.$k }
    $ConnectionString = $val
}

function Get-PgParam([string]$cs, [string]$key) {
    if ($cs -match "(?i)(^|;)${key}=([^;]+)") { return $Matches[2].Trim() }
    return $null
}

$pgHost = Get-PgParam $ConnectionString 'Host'
$pgPort = Get-PgParam $ConnectionString 'Port'
$pgDb   = Get-PgParam $ConnectionString 'Database'
$pgUser = Get-PgParam $ConnectionString 'Username'
$pgPass = Get-PgParam $ConnectionString 'Password'
if (-not $pgPort) { $pgPort = '5432' }

function Invoke-Sql([string]$sql) {
    Write-Host "SQL: $sql" -ForegroundColor DarkGray
    $env:PGPASSWORD = $pgPass
    $r = psql -h $pgHost -p $pgPort -U $pgUser -d $pgDb -c $sql 2>&1
    $env:PGPASSWORD = $null
    $r | ForEach-Object { Write-Host $_ }
}

if ($All) {
    Write-Host "TRUNCATE total de llm_audit_entries..." -ForegroundColor Red
    Invoke-Sql "TRUNCATE TABLE llm_audit_entries RESTART IDENTITY;"
    Write-Host "Listo — tabla vacía." -ForegroundColor Green
    exit 0
}

if ($KeepLast -gt 0) {
    Write-Host "Conservando últimos $KeepLast registros..." -ForegroundColor Yellow
    Invoke-Sql "DELETE FROM llm_audit_entries WHERE id NOT IN (SELECT id FROM llm_audit_entries ORDER BY id DESC LIMIT $KeepLast);"
    Write-Host "Listo." -ForegroundColor Green
    exit 0
}

if ($OlderThan) {
    $cutoff = switch -Regex ($OlderThan) {
        '(\d+)h$' { (Get-Date).AddHours(-[int]$Matches[1]).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
        '(\d+)d$' { (Get-Date).AddDays(-[int]$Matches[1]).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
        '(\d+)m$' { (Get-Date).AddMinutes(-[int]$Matches[1]).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
        default   { $OlderThan }
    }
    Write-Host "Eliminando entradas anteriores a $cutoff ..." -ForegroundColor Yellow
    Invoke-Sql "DELETE FROM llm_audit_entries WHERE created_at < '$cutoff';"
    Write-Host "Listo." -ForegroundColor Green
    exit 0
}

Write-Host "Uso:" -ForegroundColor Yellow
Write-Host "  -All                   Vacía la tabla completa"
Write-Host "  -KeepLast 100          Conserva solo los últimos 100 registros"
Write-Host "  -OlderThan 7d          Elimina entradas más viejas que 7 días (1h, 30m también válidos)"
