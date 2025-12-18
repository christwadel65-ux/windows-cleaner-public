param(
    [Parameter(Mandatory=$true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,
    
    [switch]$Build,
    [switch]$CreateInstaller
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Mise a jour de version - Windows Cleaner" -ForegroundColor Cyan
Write-Host "  Nouvelle version: $Version" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Chemins des fichiers
$projectRoot = Split-Path -Parent $PSScriptRoot
$csprojPath = Join-Path $projectRoot "src\WindowsCleaner\WindowsCleaner.csproj"
$issPath = Join-Path $projectRoot "build\windows-cleaner.iss"
$readmePath = Join-Path $projectRoot "README.md"

# Validation des fichiers
$files = @($csprojPath, $issPath, $readmePath)
foreach ($file in $files) {
    if (-not (Test-Path $file)) {
        Write-Error "Fichier non trouvé: $file"
        exit 1
    }
}

Write-Host "[1/4] Mise à jour de WindowsCleaner.csproj..." -ForegroundColor Yellow

# Mise à jour du .csproj
$csproj = Get-Content $csprojPath -Raw
$csproj = $csproj -replace '<Version>[\d\.]+</Version>', "<Version>$Version</Version>"
$csproj = $csproj -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$Version.0</FileVersion>"
$csproj = $csproj -replace '<InformationalVersion>[\d\.]+</InformationalVersion>', "<InformationalVersion>$Version</InformationalVersion>"
Set-Content $csprojPath $csproj -NoNewline
Write-Host "  OK Version mise a jour: $Version" -ForegroundColor Green

Write-Host ""
Write-Host "[2/4] Mise à jour de windows-cleaner.iss..." -ForegroundColor Yellow

# Mise à jour du fichier Inno Setup
$iss = Get-Content $issPath -Raw
$iss = $iss -replace 'AppVersion=[\d\.]+', "AppVersion=$Version"
$iss = $iss -replace 'OutputBaseFilename=WindowsCleaner-Setup-[\d\.]+', "OutputBaseFilename=WindowsCleaner-Setup-$Version"
Set-Content $issPath $iss -NoNewline
Write-Host "  OK AppVersion et OutputBaseFilename mis a jour: $Version" -ForegroundColor Green

Write-Host ""
Write-Host "[3/4] Mise à jour de README.md..." -ForegroundColor Yellow

# Mise à jour du README
$readme = Get-Content $readmePath -Raw
$readme = $readme -replace '# Windows Cleaner v[\d\.]+', "# Windows Cleaner v$Version"
$readme = $readme -replace 'version-[\d\.]+-brightgreen', "version-$Version-brightgreen"
$readme = $readme -replace 'Nouveautés v[\d\.]+', "Nouveautés v$Version"
$readme = $readme -replace 'WindowsCleaner-Setup-[\d\.]+', "WindowsCleaner-Setup-$Version"
Set-Content $readmePath $readme -NoNewline
Write-Host "  OK README mis a jour avec la version $Version" -ForegroundColor Green

Write-Host ""
Write-Host "[4/4] Résumé des modifications..." -ForegroundColor Yellow
Write-Host "  OK WindowsCleaner.csproj" -ForegroundColor Green
Write-Host "  OK windows-cleaner.iss" -ForegroundColor Green
Write-Host "  OK README.md" -ForegroundColor Green

# Compilation optionnelle
if ($Build) {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Compilation du projet" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    
    Push-Location $projectRoot
    try {
        dotnet build src\WindowsCleaner\WindowsCleaner.csproj -c Release
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Erreur lors de la compilation"
            exit 1
        }
        Write-Host ""
        Write-Host "OK Compilation réussie" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Création de l'installeur optionnelle
if ($CreateInstaller) {
    if (-not $Build) {
        Write-Warning "Option CreateInstaller necessit Build. Compilation en cours..."
        Push-Location $projectRoot
        try {
            dotnet build src\WindowsCleaner\WindowsCleaner.csproj -c Release
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Erreur lors de la compilation"
                exit 1
            }
        }
        finally {
            Pop-Location
        }
    }
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Creation de l'installeur Inno Setup" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    
    $isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (-not (Test-Path $isccPath)) {
        Write-Error "ISCC.exe non trouvé. Assurez-vous qu'Inno Setup 6 est installe."
        exit 1
    }
    
    & $isccPath $issPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Erreur lors de la creation de l'installeur"
        exit 1
    }
    
    Write-Host ""
    Write-Host "OK Installeur cree: Output\WindowsCleaner-Setup-$Version.exe" -ForegroundColor Green
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  OK Mise a jour terminée avec succès!" -ForegroundColor Green
Write-Host "  Version: $Version" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Prochaines étapes suggérées:" -ForegroundColor Yellow
Write-Host "  1. Verifier les changements: git diff" -ForegroundColor White
Write-Host "  2. Commiter les changements" -ForegroundColor White
Write-Host "  3. Tag: git tag v$Version" -ForegroundColor White
Write-Host "  4. Push: git push origin main --tags" -ForegroundColor White
Write-Host ""
