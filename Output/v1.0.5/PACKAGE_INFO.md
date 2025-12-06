# Windows Cleaner v1.0.5

## 📦 Package de Distribution

Cet archive contient l'application Windows Cleaner compilée en version 1.0.5.

### 📋 Fichiers Inclus

- **windows-cleaner.exe** - Application principale (199.5 KB)
- **windows-cleaner.dll** - Assembly .NET (110 KB)
- **windows-cleaner.deps.json** - Dépendances
- **windows-cleaner.runtimeconfig.json** - Configuration runtime
- **app.ico** - Icône de l'application
- **run.bat** - Script lanceur (optionnel)
- **README.md** - Documentation

### 🚀 Lancement

#### Option 1: Exécution Directe
```bash
windows-cleaner.exe
```

#### Option 2: Utiliser le script batch
```bash
run.bat
```

### ✨ Améliorations v1.0.5

- ✅ Classe `BrowserPaths` centralisée pour les chemins
- ✅ Logging robuste avec gestion d'erreurs complète
- ✅ Support `CancellationToken` pour annulation gracieuse
- ✅ Logger thread-safe pour opérations parallèles
- ✅ Documentation XML complète
- ✅ 0 avertissements de compilation

### 📊 Spécifications

- **Framework**: .NET 10.0 Windows
- **Version**: 1.0.5.0
- **Configuration**: Release (optimisée)
- **Taille totale**: ~355 KB
- **Prérequis**: Windows 10/11, .NET 10.0 runtime

### 🔧 Configuration Requise

- Windows 10 ou Windows 11 (x64)
- .NET 10.0 Runtime (inclus)
- Admin privs pour certaines opérations de nettoyage

### 📝 Notes

- Les droits administrateur sont recommandés pour accéder à tous les chemins système
- Utiliser le mode "Simuler" avant de lancer un nettoyage réel
- Les logs sont sauvegardés dans `%APPDATA%\WindowsCleaner\logs\`

### 🐛 Signaler les Problèmes

Consultez: https://github.com/christwadel65-ux/windows-cleaner

---

**Windows Cleaner v1.0.5** | Build: Release | Date: 6 décembre 2025
