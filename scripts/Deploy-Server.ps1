param(
    [string]$ProjectPath = "..\src\Server",
    [string]$PublishPath = "..\publish\Server",
    [string]$SiteName = "POSMonitor",
    [string]$AppPoolName = "POSMonitorPool",
    [string]$BindingInformation = "*:8080:"
)

Write-Host "Building and publishing server project..." -ForegroundColor Cyan
& dotnet publish $ProjectPath -c Release -o $PublishPath
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed"
}

Import-Module WebAdministration

if (-not (Test-Path IIS:\AppPools\$AppPoolName)) {
    New-WebAppPool -Name $AppPoolName | Out-Null
    Set-ItemProperty IIS:\AppPools\$AppPoolName -Name managedRuntimeVersion -Value ""
    Write-Host "Created App Pool $AppPoolName" -ForegroundColor Green
}

if (-not (Test-Path IIS:\Sites\$SiteName)) {
    New-Website -Name $SiteName -PhysicalPath (Resolve-Path $PublishPath) -ApplicationPool $AppPoolName -BindingInformation $BindingInformation -Port 8080 -IPAddress * -HostHeader ""
    Write-Host "Created IIS site $SiteName" -ForegroundColor Green
}
else {
    Set-ItemProperty IIS:\Sites\$SiteName -Name physicalPath -Value (Resolve-Path $PublishPath)
    Write-Host "Updated IIS site $SiteName path" -ForegroundColor Yellow
}

Write-Host "Server deployment completed" -ForegroundColor Cyan
