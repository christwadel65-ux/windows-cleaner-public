#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-Location "D:\GitHub\Windows Cleaner"

Write-Host "Building Windows Cleaner v2.0.3 (Release)..." -ForegroundColor Cyan
dotnet build src/WindowsCleaner/WindowsCleaner.csproj --configuration Release
Write-Host "Build completed successfully!" -ForegroundColor Green
