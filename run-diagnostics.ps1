[CmdletBinding()]
param(
    [string]$Executable = ".\artifacts\win-x64\VexoJumper.exe",
    [int]$WaitSeconds = 5
)

$ErrorActionPreference = 'Stop'
$exePath = [System.IO.Path]::GetFullPath($Executable)
$logPath = Join-Path $env:LOCALAPPDATA 'VexoJumper\logs\startup.log'

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "VexoJumper.exe not found: $exePath. Run .\build-and-publish.ps1 first."
}

if (Test-Path -LiteralPath $logPath) {
    Remove-Item -LiteralPath $logPath -Force
}

Write-Host "Launching: $exePath" -ForegroundColor Cyan
$process = Start-Process -FilePath $exePath -PassThru
Start-Sleep -Seconds $WaitSeconds
$process.Refresh()

if (-not $process.HasExited) {
    Write-Host "Vexo Jumper is still running. Startup appears successful." -ForegroundColor Green
    Write-Host "Process ID: $($process.Id)"
    exit 0
}

Write-Host "Vexo Jumper exited during startup. Exit code: $($process.ExitCode)" -ForegroundColor Red
if (Test-Path -LiteralPath $logPath) {
    Write-Host "Startup log: $logPath" -ForegroundColor Yellow
    Get-Content -LiteralPath $logPath -Raw
} else {
    Write-Warning "No startup log was created. Check Windows Event Viewer > Windows Logs > Application, and Windows Security/SmartScreen history."
}
exit 1
