#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

param(
    [string]$ConnectionString,
    [string]$SettingsFile,
    [string]$SettingsKeyPath,
    [string]$Filter,
    [string]$Component,
    [string]$Since,
    [int]$Limit = 50
)

# Cargar config del proyecto si existe en el directorio actual (raíz del repo)
$configFile = Join-Path (Get-Location) 'audit.config.ps1'
if (Test-Path $configFile) { . $configFile }

# Resolver connection string
if (-not $ConnectionString) {
    if (-not $SettingsFile)   { $SettingsFile    = if ($null -ne $AuditConfig) { $AuditConfig.SettingsFile }   else { 'appsettings.Development.json' } }
    if (-not $SettingsKeyPath) { $SettingsKeyPath = if ($null -ne $AuditConfig) { $AuditConfig.SettingsKeyPath } else { 'ConnectionStrings.DefaultConnection' } }

    if (-not (Test-Path $SettingsFile)) {
        Write-Error "No se encontró $SettingsFile. Pasa -ConnectionString directamente o configura audit.config.ps1"
    }

    $settings = Get-Content $SettingsFile -Raw | ConvertFrom-Json
    $keys = $SettingsKeyPath -split '\.'
    $val = $settings
    foreach ($k in $keys) { $val = $val.$k }
    $ConnectionString = $val
}

if (-not $ConnectionString) {
    Write-Error "No se pudo obtener ConnectionString. Verifica SettingsKeyPath='$SettingsKeyPath' en $SettingsFile"
}

# Extraer credenciales del connection string
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

# Construir SQL
$conditions = @('1=1')
if ($Filter)    { $conditions += "CAST(intent AS TEXT) ILIKE '%$Filter%' OR CAST(result AS TEXT) ILIKE '%$Filter%'" }
if ($Component) { $conditions += "component ILIKE '%$Component%'" }
if ($Since) {
    $sinceTime = switch -Regex ($Since) {
        '(\d+)h$' { (Get-Date).AddHours(-[int]$Matches[1]).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
        '(\d+)m$' { (Get-Date).AddMinutes(-[int]$Matches[1]).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
        '(\d+)d$' { (Get-Date).AddDays(-[int]$Matches[1]).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
        default   { $Since }
    }
    $conditions += "created_at >= '$sinceTime'"
}

$where = $conditions -join ' AND '
$sql = "SELECT id, created_at, category, component, intent, LEFT(result,120) AS result FROM llm_audit_entries WHERE $where ORDER BY id DESC LIMIT $Limit;"

Write-Host "Conectando a $pgHost/$pgDb como $pgUser ..." -ForegroundColor Cyan
Write-Host "Query: $sql`n" -ForegroundColor DarkGray

$env:PGPASSWORD = $pgPass
$result = psql -h $pgHost -p $pgPort -U $pgUser -d $pgDb -c $sql 2>&1
$env:PGPASSWORD = $null

$result | ForEach-Object {
    $line = $_
    if ($line -match 'ERROR') { Write-Host $line -ForegroundColor Red }
    elseif ($line -match '✅') { Write-Host $line -ForegroundColor Green }
    else { Write-Host $line }
}
