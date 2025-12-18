# Guide de Gestion des Versions

## 📋 Vue d'ensemble

La version de Windows Cleaner est gérée de manière centralisée pour éviter les incohérences. La version est définie **une seule fois** dans le fichier `.csproj` et propagée automatiquement partout.

## 🎯 Architecture de Versioning

### Source Unique de Vérité
Le fichier `src/WindowsCleaner/WindowsCleaner.csproj` contient les propriétés de version :
```xml
<Version>2.0.1</Version>
<FileVersion>2.0.1.0</FileVersion>
<InformationalVersion>2.0.1</InformationalVersion>
```

### Classe AppVersion
La classe `src/WindowsCleaner/Core/AppVersion.cs` lit automatiquement la version depuis l'assembly au runtime :
```csharp
// Utilisation dans le code
string version = AppVersion.Current;        // "2.0.1"
string fullVersion = AppVersion.Full;       // "2.0.1.0"
string display = AppVersion.GetDisplayVersion(); // "Version: 2.0.1"
```

### Fichiers Synchronisés
- `src/WindowsCleaner/WindowsCleaner.csproj` - **Source primaire**
- `src/WindowsCleaner/Core/AppVersion.cs` - Lit depuis l'assembly
- `src/WindowsCleaner/UI/MainForm.cs` - Utilise `AppVersion.Current`
- `build/windows-cleaner.iss` - Installeur (AppVersion, OutputBaseFilename)
- `README.md` - Badge et documentation

## 🚀 Mettre à Jour la Version

### Méthode 1 : Script Automatique (RECOMMANDÉ)

Utilisez le script PowerShell qui met à jour tous les fichiers automatiquement :

```powershell
# Mise à jour simple
.\scripts\update_version.ps1 -Version 2.0.2

# Mise à jour + compilation
.\scripts\update_version.ps1 -Version 2.0.2 -Build

# Mise à jour + compilation + installeur
.\scripts\update_version.ps1 -Version 2.0.2 -Build -CreateInstaller
```

Le script met à jour automatiquement :
- ✅ WindowsCleaner.csproj (Version, FileVersion, InformationalVersion)
- ✅ windows-cleaner.iss (AppVersion, OutputBaseFilename)
- ✅ README.md (badge, titre, mentions)

### Méthode 2 : Modification Manuelle

Si vous devez modifier manuellement :

1. **Modifier le .csproj** (source primaire)
   ```xml
   <Version>X.Y.Z</Version>
   <FileVersion>X.Y.Z.0</FileVersion>
   <InformationalVersion>X.Y.Z</InformationalVersion>
   ```

2. **Modifier windows-cleaner.iss**
   ```ini
   AppVersion=X.Y.Z
   OutputBaseFilename=WindowsCleaner-Setup-X.Y.Z
   ```

3. **Modifier README.md**
   ```markdown
   # Windows Cleaner vX.Y.Z
   [![Version](https://img.shields.io/badge/version-X.Y.Z-brightgreen.svg)]
   ## 🆕 Nouveautés vX.Y.Z
   ```

4. **Recompiler le projet**
   ```powershell
   dotnet build src\WindowsCleaner\WindowsCleaner.csproj -c Release
   ```

5. **Créer l'installeur**
   ```powershell
   iscc "build\windows-cleaner.iss"
   ```

## ⚠️ Points d'Attention

### ❌ NE JAMAIS
- Coder une version en dur dans le code source (utiliser `AppVersion.Current`)
- Modifier la version dans un seul fichier sans mettre à jour les autres
- Oublier de recompiler après un changement de version

### ✅ TOUJOURS
- Utiliser le script `update_version.ps1` pour les mises à jour
- Utiliser `AppVersion.Current` dans le code au lieu de chaînes en dur
- Vérifier que la version s'affiche correctement après compilation :
  - Menu "Aide > À propos"
  - Vérification des mises à jour
  - Propriétés du fichier .exe (Détails)

## 🔍 Vérification

Après mise à jour, vérifiez :

```powershell
# 1. Version dans le .csproj
Select-String -Path "src\WindowsCleaner\WindowsCleaner.csproj" -Pattern "<Version>"

# 2. Version dans l'installeur
Select-String -Path "build\windows-cleaner.iss" -Pattern "AppVersion"

# 3. Version dans le README
Select-String -Path "README.md" -Pattern "version-"

# 4. Compiler et exécuter
dotnet build src\WindowsCleaner\WindowsCleaner.csproj -c Release
Start-Process "bin\Release\net10.0-windows\windows-cleaner.exe"
# Vérifier "Aide > À propos"
```

## 📝 Workflow Complet de Release

1. **Mettre à jour la version**
   ```powershell
   .\scripts\update_version.ps1 -Version 2.1.0 -Build -CreateInstaller
   ```

2. **Vérifier les changements**
   ```powershell
   git diff
   ```

3. **Commiter**
   ```powershell
   git add -A
   git commit -m "chore: bump version to 2.1.0"
   ```

4. **Créer un tag**
   ```powershell
   git tag v2.1.0
   ```

5. **Pousser vers GitHub**
   ```powershell
   git push origin main
   git push origin v2.1.0
   ```

6. **Créer une release GitHub**
   - Aller sur GitHub > Releases > New Release
   - Choisir le tag `v2.1.0`
   - Joindre `Output\WindowsCleaner-Setup-2.1.0.exe`
   - Publier

## 🔧 Dépannage

### Problème : "Nouvelle mise à jour disponible" alors que j'ai la dernière version
**Cause** : La version dans le code ne correspond pas à celle du .csproj

**Solution** : 
```powershell
# Vérifier que MainForm.cs utilise AppVersion.Current
Select-String -Path "src\WindowsCleaner\UI\MainForm.cs" -Pattern "AppVersion.Current"
# Doit retourner au moins 2 matches
```

### Problème : La version affichée ne correspond pas
**Cause** : L'application n'a pas été recompilée après le changement

**Solution** :
```powershell
# Nettoyer et recompiler
dotnet clean src\WindowsCleaner\WindowsCleaner.csproj
dotnet build src\WindowsCleaner\WindowsCleaner.csproj -c Release
```

### Problème : L'installeur ne détecte pas l'ancienne version
**Cause** : L'AppId dans le .iss a changé ou est manquant

**Solution** : Vérifier que `AppId` est défini dans `windows-cleaner.iss` :
```ini
[Setup]
AppId={{8B5E5F6D-9C3A-4E2B-A1D7-3F8C9E4A6B5D}
```

## 📚 Ressources

- [Semantic Versioning](https://semver.org/)
- [.NET Assembly Versioning](https://learn.microsoft.com/en-us/dotnet/standard/assembly/versioning)
- [Inno Setup Documentation](https://jrsoftware.org/ishelp/)
