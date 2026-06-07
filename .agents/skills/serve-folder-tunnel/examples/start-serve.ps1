<#
.SYNOPSIS
    Inicia npx serve en background y muestra instrucciones para el Dev Tunnel.

.PARAMETER Path
    Carpeta a servir. Default: directorio actual.

.PARAMETER Port
    Puerto local. Default: 3000.

.EXAMPLE
    .\start-serve.ps1
    .\start-serve.ps1 -Path "C:\mis-reportes" -Port 4500
#>
param(
    [string]$Path = ".",
    [int]$Port = 3000
)

$StateFile = "$env:TEMP\serve-tunnel.state.json"

# --- Detener instancia previa si existe ---
if (Test-Path $StateFile) {
    $prev = Get-Content $StateFile | ConvertFrom-Json
    $prevJob = Get-Job -Id $prev.JobId -ErrorAction SilentlyContinue
    if ($prevJob) {
        Stop-Job  -Job $prevJob -ErrorAction SilentlyContinue
        Remove-Job -Job $prevJob -ErrorAction SilentlyContinue
        Write-Host "Instancia previa detenida (job $($prev.JobId))" -ForegroundColor DarkGray
    }
    Remove-Item $StateFile -Force
}

# --- Resolver ruta absoluta ---
$AbsPath = (Resolve-Path $Path -ErrorAction Stop).Path

# --- Verificar que npm está disponible ---
if (-not (Get-Command "npm" -ErrorAction SilentlyContinue)) {
    Write-Error "npm no encontrado. Instala Node.js desde https://nodejs.org"
    exit 1
}

# --- Iniciar en background ---
$job = Start-Job -ScriptBlock {
    param($folder, $port)
    & npx serve $folder --listen $port --no-clipboard 2>&1
} -ArgumentList $AbsPath, $Port

# --- Guardar estado ---
[PSCustomObject]@{
    JobId = $job.Id
    Path  = $AbsPath
    Port  = $Port
} | ConvertTo-Json | Out-File $StateFile -Force -Encoding UTF8

# --- Esperar un momento y verificar que inició ---
Start-Sleep -Seconds 2
$jobState = (Get-Job -Id $job.Id).State
if ($jobState -eq "Failed") {
    Write-Error "El servidor no pudo iniciar. Revisa que el puerto $Port no esté en uso."
    Receive-Job -Id $job.Id
    exit 1
}

# --- Output ---
Write-Host ""
Write-Host "Servidor iniciado" -ForegroundColor Green
Write-Host "  Carpeta : $AbsPath"
Write-Host "  Puerto  : $Port"
Write-Host "  URL local: http://localhost:$Port" -ForegroundColor Cyan
Write-Host "  Job ID  : $($job.Id)  (guardado en $StateFile)"
Write-Host ""
Write-Host "Pasos para exponer a Internet (VS Code):" -ForegroundColor Yellow
Write-Host "  1. Pestaña PUERTOS  →  Agregar puerto  →  $Port"
Write-Host "  2. Clic derecho en la fila  →  Visibilidad del puerto  →  Publico"
Write-Host "  3. Copiar la URL de la columna 'Direccion reenviada'"
Write-Host ""
Write-Host "Para detener el servidor:  .\stop-serve.ps1" -ForegroundColor DarkGray
