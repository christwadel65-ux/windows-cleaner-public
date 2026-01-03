# Script de Préparation de Release - Windows Cleaner
# Ce script automatise la préparation d'une nouvelle release

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [switch]$Build,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateZip,
    
    [Parameter(Mandatory=$false)]
    [switch]$PushTag
)

# Couleurs pour le terminal
$SuccessColor = "Green"
$ErrorColor = "Red"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor $SuccessColor }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor $ErrorColor }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor $InfoColor }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor $WarningColor }

# Validation du format de version
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Format de version invalide. Utilisez le format X.Y.Z (ex: 1.0.9)"
    exit 1
}

Write-Info "=== Préparation de la Release v$Version ==="
Write-Host ""

# Chemins
$RootPath = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $RootPath "src\WindowsCleaner"
$CsprojPath = Join-Path $ProjectPath "WindowsCleaner.csproj"
$MainFormPath = Join-Path $ProjectPath "UI\MainForm.cs"
$ManifestPath = Join-Path $ProjectPath "UI\app.manifest"
$ChangelogPath = Join-Path $RootPath "CHANGELOG.md"
$BinPath = Join-Path $RootPath "bin\Release\net10.0-windows"
$OutputPath = Join-Path $RootPath "release"

# Vérifier que les fichiers existent
$requiredFiles = @($CsprojPath, $MainFormPath, $ManifestPath)
foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        Write-Error "Fichier introuvable: $file"
        exit 1
    }
}

# Étape 1: Mettre à jour WindowsCleaner.csproj
Write-Info "Mise à jour de WindowsCleaner.csproj..."
try {
    $csproj = [xml](Get-Content $CsprojPath)
    $propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.Version }
    
    if ($propertyGroup) {
        $propertyGroup.Version = $Version
        $propertyGroup.FileVersion = "$Version.0"
        $propertyGroup.InformationalVersion = $Version
        $csproj.Save($CsprojPath)
        Write-Success "WindowsCleaner.csproj mis à jour"
    } else {
        Write-Error "Impossible de trouver PropertyGroup dans le fichier .csproj"
        exit 1
    }
} catch {
    Write-Error "Erreur lors de la mise à jour du .csproj: $_"
    exit 1
}

# Étape 2: Mettre à jour MainForm.cs
Write-Info "Mise à jour de MainForm.cs..."
try {
    $mainFormContent = Get-Content $MainFormPath -Raw
    $pattern = 'new UpdateManager\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)"\)'
    
    if ($mainFormContent -match $pattern) {
        $owner = $matches[1]
        $repo = $matches[2]
        $newContent = $mainFormContent -replace $pattern, "new UpdateManager(`"$owner`", `"$repo`", `"$Version`")"
        $newContent | Set-Content $MainFormPath -NoNewline
        Write-Success "MainForm.cs mis à jour"
    } else {
        Write-Warning "UpdateManager non trouvé dans MainForm.cs - passage manuel requis"
    }
} catch {
    Write-Error "Erreur lors de la mise à jour de MainForm.cs: $_"
    exit 1
}

# Étape 3: Mettre à jour app.manifest
Write-Info "Mise à jour de app.manifest..."
try {
    $manifest = [xml](Get-Content $ManifestPath)
    $assemblyIdentity = $manifest.assembly.assemblyIdentity
    
    if ($assemblyIdentity) {
        $assemblyIdentity.version = "$Version.0"
        $manifest.Save($ManifestPath)
        Write-Success "app.manifest mis à jour"
    } else {
        Write-Warning "assemblyIdentity non trouvé dans app.manifest"
    }
} catch {
    Write-Error "Erreur lors de la mise à jour du manifest: $_"
    exit 1
}

# Étape 4: Compiler le projet (optionnel)
if ($Build) {
    Write-Info "Compilation en mode Release..."
    try {
        Push-Location $ProjectPath
        dotnet build -c Release --nologo
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Compilation réussie"
        } else {
            Write-Error "Échec de la compilation"
            Pop-Location
            exit 1
        }
        Pop-Location
    } catch {
        Write-Error "Erreur lors de la compilation: $_"
        Pop-Location
        exit 1
    }
}

# Étape 5: Créer l'archive ZIP (optionnel)
if ($CreateZip -and $Build) {
    Write-Info "Création de l'archive portable..."
    try {
        if (-not (Test-Path $OutputPath)) {
            New-Item -ItemType Directory -Path $OutputPath | Out-Null
        }
        
        $zipName = "windows-cleaner-v$Version-portable.zip"
        $zipPath = Join-Path $OutputPath $zipName
        
        # Supprimer l'ancien ZIP s'il existe
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }
        
        # Créer le ZIP
        if (Test-Path $BinPath) {
            Compress-Archive -Path "$BinPath\*" -DestinationPath $zipPath -CompressionLevel Optimal
            
            $zipSize = (Get-Item $zipPath).Length / 1MB
            Write-Success "Archive créée: $zipName ($([math]::Round($zipSize, 2)) MB)"
        } else {
            Write-Error "Dossier bin/Release introuvable. Compilez d'abord le projet."
        }
    } catch {
        Write-Error "Erreur lors de la création du ZIP: $_"
    }
}

# Étape 6: Créer et pousser le tag Git (optionnel)
if ($PushTag) {
    Write-Info "Création du tag Git v$Version..."
    try {
        # Vérifier qu'on est dans un repo Git
        $gitStatus = git status 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Pas un dépôt Git valide"
            exit 1
        }
        
        # Vérifier s'il y a des modifications non commitées
        $status = git status --porcelain
        if ($status) {
            Write-Warning "Il y a des modifications non commitées:"
            Write-Host $status
            $commit = Read-Host "Voulez-vous commiter ces changements maintenant? (O/N)"
            
            if ($commit -eq 'O' -or $commit -eq 'o') {
                git add .
                git commit -m "Release version $Version"
                Write-Success "Modifications commitées"
            } else {
                Write-Warning "Tag créé sans commiter les modifications"
            }
        }
        
        # Créer le tag
        $tagMessage = "Release version $Version"
        git tag -a "v$Version" -m $tagMessage
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Tag v$Version créé localement"
            
            # Demander confirmation pour pousser
            $push = Read-Host "Pousser le tag sur GitHub? (O/N)"
            if ($push -eq 'O' -or $push -eq 'o') {
                git push origin "v$Version"
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "Tag poussé sur GitHub"
                    Write-Info "Créez maintenant la release sur GitHub: https://github.com/votre-username/Windows-Cleaner/releases/new?tag=v$Version"
                } else {
                    Write-Error "Échec du push du tag"
                }
            }
        } else {
            Write-Error "Échec de la création du tag"
        }
    } catch {
        Write-Error "Erreur Git: $_"
    }
}

# Résumé
Write-Host ""
Write-Info "=== Résumé de la préparation ==="
Write-Host "Version: $Version" -ForegroundColor White
Write-Host "Fichiers mis à jour:" -ForegroundColor White
Write-Host "  - WindowsCleaner.csproj" -ForegroundColor Gray
Write-Host "  - MainForm.cs (UpdateManager)" -ForegroundColor Gray
Write-Host "  - app.manifest" -ForegroundColor Gray

if ($Build) {
    Write-Host "Compilation: Effectuée" -ForegroundColor $SuccessColor
}

if ($CreateZip -and $Build) {
    Write-Host "Archive ZIP: Créée dans /release" -ForegroundColor $SuccessColor
}

if ($PushTag) {
    Write-Host "Tag Git: Créé et poussé" -ForegroundColor $SuccessColor
}

Write-Host ""
Write-Info "=== Prochaines étapes ==="
Write-Host "1. Vérifiez les modifications avec: git diff" -ForegroundColor Yellow
Write-Host "2. Testez la compilation et l'application" -ForegroundColor Yellow
Write-Host "3. Mettez à jour le CHANGELOG.md" -ForegroundColor Yellow
Write-Host "4. Créez la release sur GitHub" -ForegroundColor Yellow
Write-Host "5. Attachez les binaires à la release" -ForegroundColor Yellow

# Exemple d'utilisation
Write-Host ""
Write-Info "Exemples d'utilisation:"
Write-Host "  # Mise à jour simple des fichiers" -ForegroundColor Gray
Write-Host "  .\prepare_release.ps1 -Version 1.0.9" -ForegroundColor Gray
Write-Host ""
Write-Host "  # Avec compilation et ZIP" -ForegroundColor Gray
Write-Host "  .\prepare_release.ps1 -Version 1.0.9 -Build -CreateZip" -ForegroundColor Gray
Write-Host ""
Write-Host "  # Tout automatique (fichiers + build + tag + push)" -ForegroundColor Gray
Write-Host "  .\prepare_release.ps1 -Version 1.0.9 -Build -CreateZip -PushTag" -ForegroundColor Gray

Write-Success "Script terminé avec succès !"
