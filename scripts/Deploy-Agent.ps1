param(
    [string]$ProjectPath = "..\src\Agent",
    [string]$PublishPath = "..\publish\Agent",
    [string]$ServiceName = "POSMonitorAgent"
)

Write-Host "Publishing agent..." -ForegroundColor Cyan
& dotnet publish $ProjectPath -c Release -o $PublishPath
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed"
}

$exePath = Join-Path (Resolve-Path $PublishPath) "POSMonitor.Agent.exe"

if (-not (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue)) {
    sc.exe create $ServiceName binPath= $exePath start= auto DisplayName= "POS Monitor Agent"
    Write-Host "Windows Service $ServiceName created" -ForegroundColor Green
}
else {
    sc.exe stop $ServiceName | Out-Null
    sc.exe config $ServiceName binPath= $exePath | Out-Null
    Write-Host "Service $ServiceName updated" -ForegroundColor Yellow
}

sc.exe start $ServiceName | Out-Null
Write-Host "Agent service started" -ForegroundColor Cyan
