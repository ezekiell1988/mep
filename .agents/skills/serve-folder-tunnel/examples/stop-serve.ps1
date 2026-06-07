<#
.SYNOPSIS
    Detiene el servidor npx serve iniciado por start-serve.ps1.

.EXAMPLE
    .\stop-serve.ps1
#>

$StateFile = "$env:TEMP\serve-tunnel.state.json"

if (-not (Test-Path $StateFile)) {
    Write-Host "No hay servidor activo registrado (no se encontro $StateFile)" -ForegroundColor Yellow
    exit 0
}

$state = Get-Content $StateFile | ConvertFrom-Json

# --- Detener el Job de PowerShell ---
$job = Get-Job -Id $state.JobId -ErrorAction SilentlyContinue
if ($job) {
    Stop-Job  -Job $job -ErrorAction SilentlyContinue
    Remove-Job -Job $job -ErrorAction SilentlyContinue
    Write-Host "Servidor detenido (job $($state.JobId))" -ForegroundColor Green
} else {
    Write-Host "Job $($state.JobId) ya no existe (puede que se detuvo antes)" -ForegroundColor DarkGray
}

# --- Matar procesos node que sirvan el mismo puerto (limpieza extra) ---
$tcpRows = netstat -ano 2>$null | Select-String ":$($state.Port)\s"
foreach ($row in $tcpRows) {
    $procId = ($row -split '\s+')[-1]
    if ($procId -match '^\d+$' -and [int]$procId -gt 0) {
        $proc = Get-Process -Id ([int]$procId) -ErrorAction SilentlyContinue
        if ($proc -and $proc.Name -eq "node") {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            Write-Host "Proceso node (PID $($proc.Id)) en puerto $($state.Port) eliminado" -ForegroundColor DarkGray
        }
    }
}

# --- Limpiar archivo de estado ---
Remove-Item $StateFile -Force
Write-Host ""
Write-Host "Recuerda cerrar el puerto en VS Code (pestana PUERTOS) si ya no lo necesitas." -ForegroundColor Yellow
