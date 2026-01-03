#!/usr/bin/env pwsh
# Script pour préparer un repo de distribution binaire uniquement
# Ce script supprime le code source mais garde les releases et la documentation

param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Red
Write-Host "  ATTENTION: Préparation Repo Binaire Uniquement" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Red
Write-Host ""
Write-Host "Ce script va SUPPRIMER:" -ForegroundColor Yellow
Write-Host "  - Tout le code source (dossier src/)" -ForegroundColor Yellow
Write-Host "  - Les workflows GitHub Actions" -ForegroundColor Yellow
Write-Host "  - Les fichiers de sécurité" -ForegroundColor Yellow
Write-Host "  - Les tests et configuration de dev" -ForegroundColor Yellow
Write-Host ""
Write-Host "Ce script va GARDER:" -ForegroundColor Green
Write-Host "  - README.md" -ForegroundColor Green
Write-Host "  - LICENSE" -ForegroundColor Green
Write-Host "  - CHANGELOG.md" -ForegroundColor Green
Write-Host "  - Les releases (binaires)" -ForegroundColor Green
Write-Host "  - Documentation utilisateur" -ForegroundColor Green
Write-Host ""

if ($DryRun) {
    Write-Host "MODE DRY-RUN: Aucun fichier ne sera supprimé" -ForegroundColor Cyan
    Write-Host ""
}

# Liste des dossiers et fichiers à supprimer
$toDelete = @(
    # Code source
    "src",
    "obj",
    "bin",
    
    # Build et compilation
    "build",
    "Output",
    "scripts",
    "compile.ps1",
    "build.bat",
    "build-release.ps1",
    
    # GitHub Actions et CI/CD
    ".github",
    
    # Sécurité
    "SECURITY.md",
    "CODEOWNERS",
    
    # Développement
    ".vs",
    ".vscode",
    "*.sln",
    "*.csproj",
    "global.json",
    
    # Documentation développeur
    "CONTRIBUTING.md",
    "docs/VERSION_MANAGEMENT.md",
    "docs/RELEASE_GUIDE.md",
    "plans",
    
    # Tests
    "tests",
    
    # Assets de développement
    "assets/icons",
    "create_icon.ps1"
)

$projectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Dossiers et fichiers qui seront supprimés:" -ForegroundColor Yellow
Write-Host ""

$itemsToDelete = @()

foreach ($item in $toDelete) {
    $fullPath = Join-Path $projectRoot $item
    
    if ($item -like "*.*") {
        # Fichier avec pattern
        $files = Get-ChildItem -Path (Split-Path $fullPath) -Filter (Split-Path $fullPath -Leaf) -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            $itemsToDelete += $file.FullName
            Write-Host "  - $($file.FullName)" -ForegroundColor DarkYellow
        }
    }
    elseif (Test-Path $fullPath) {
        $itemsToDelete += $fullPath
        Write-Host "  - $fullPath" -ForegroundColor DarkYellow
    }
}

Write-Host ""
Write-Host "Total: $($itemsToDelete.Count) éléments" -ForegroundColor Yellow
Write-Host ""

if ($DryRun) {
    Write-Host "Mode DRY-RUN activé - Aucune suppression effectuée" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Pour effectuer la suppression réelle, relancez sans -DryRun" -ForegroundColor Cyan
    exit 0
}

$confirm = Read-Host "Voulez-vous vraiment continuer? (tapez 'OUI' en majuscules)"
if ($confirm -ne "OUI") {
    Write-Host "Opération annulée" -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Suppression en cours..." -ForegroundColor Red

foreach ($item in $itemsToDelete) {
    try {
        if (Test-Path $item) {
            Remove-Item $item -Recurse -Force
            Write-Host "  ✓ Supprimé: $item" -ForegroundColor DarkGray
        }
    }
    catch {
        Write-Host "  ✗ Erreur: $item - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Nettoyage terminé!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Prochaines étapes:" -ForegroundColor Yellow
Write-Host "  1. Créer un README_BINARY.md pour expliquer que c'est binaire only" -ForegroundColor White
Write-Host "  2. Vérifier les changements: git status" -ForegroundColor White
Write-Host "  3. Commiter: git add -A && git commit -m 'Distribution binaire uniquement'" -ForegroundColor White
Write-Host "  4. Push: git push origin main" -ForegroundColor White
Write-Host ""
Write-Host "Note: Les releases (binaires) seront toujours disponibles sur GitHub" -ForegroundColor Cyan
Write-Host ""
