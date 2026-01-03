#!/usr/bin/env pwsh
# Script de compilation simple qui utilise le chemin complet de dotnet

$dotnetPath = "C:\Program Files\dotnet\dotnet.exe"
$projectPath = "d:\GitHub\Windows Cleaner\src\WindowsCleaner\WindowsCleaner.csproj"

Write-Host "================================" -ForegroundColor Cyan
Write-Host " Windows Cleaner v2.0.4 - Build" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $dotnetPath) {
    Write-Host "Compilation en cours..." -ForegroundColor Yellow
    & $dotnetPath build $projectPath --configuration Release
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host ">>> COMPILATION REUSSIE <<<" -ForegroundColor Green
        Write-Host ""
        Write-Host "Executable: bin\Release\net10.0-windows\windows-cleaner.exe" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host ">>> ERREUR DE COMPILATION <<<" -ForegroundColor Red
    }
} else {
    Write-Host "ERREUR: dotnet.exe introuvable" -ForegroundColor Red
}
