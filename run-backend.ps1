# Run Backend (Terminal 1)
Write-Host "Starting Backend API..." -ForegroundColor Green
Set-Location "$PSScriptRoot\backend\TelemetrySlice.Api"
dotnet run
