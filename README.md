# Windows Cleaner v1.0.5

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

Outil professionnel en C# (WinForms) pour nettoyer et optimiser votre système Windows. Interface moderne avec support du mode sombre, gestion avancée des erreurs et logging complet.

## 🚀 Démarrage Rapide

### Compilation et Exécution
```powershell
cd "c:\Users\c.lecomte\Documents\dev_pyt\Windows Cleaner"
dotnet build
dotnet run
```

### Exécution depuis Release
```powershell
.\Output\v1.0.5\windows-cleaner.exe
```

## ⚠️ Notes Importantes
- **Droits Administrateur** : Requis pour nettoyer les fichiers système (Temp système, Prefetch, Windows Update)
- **Mode Simuler** : Testez les opérations sans supprimer de fichiers
- **Corbeille** : Vidage sans confirmation via l'API Windows (P/Invoke)

## ✨ Fonctionnalités

### 🧹 Nettoyage Standard
- **Cache Navigateurs** : Chrome, Edge, Firefox (fermez les navigateurs avant d'exécuter)
- **Fichiers Temporaires** : Dossiers Temp utilisateur et système
- **Windows Update** : Cache de téléchargement (`C:\Windows\SoftwareDistribution\Download`)
- **Vignettes** : Fichiers `thumbcache_*.db` pour récupérer de l'espace
- **Prefetch** : Dossier `C:\Windows\Prefetch` (nécessite admin)
- **Flush DNS** : Vide le cache DNS local (`ipconfig /flushdns`)
- **Corbeille** : Vidage complet de la corbeille

### 🔬 Nettoyage Avancé
- **Journaux Système (.evtx)** : Supprime les événements Windows pour libérer de l'espace
- **Cache des Installeurs** : Nettoie `C:\Windows\Installer` (fichiers d'installation en cache)
- **Journaux d'Applications** : Logs des apps Microsoft Store (`LocalAppData\Packages`)
- **Fichiers Orphelins** : Détecte et supprime les fichiers temporaires > 7 jours
- **Cache Mémoire** : Vide les caches RAM et disque système (nécessite admin)

### 🎨 Interface Moderne
- **Thème Sombre/Clair** : Mode sombre avec accents personnalisables
- **Barre de Progression** : Suivi visuel en temps réel
- **Mode Simulation** : Prévisualisation sans suppression (Dry Run)
- **Rapport Détaillé** : Aperçu des éléments à supprimer avant exécution
- **Annulation** : Arrêtez les opérations en cours à tout moment
- **Logs en Temps Réel** : Affichage des opérations dans l'interface

### 🛡️ Robustesse (v1.0.5)
- ✅ **Gestion d'Erreurs Complète** : Toutes les erreurs sont loggées et tracées
- ✅ **Thread-Safe** : Logger sécurisé pour opérations parallèles
- ✅ **Retry Logic** : Tentatives avec backoff pour fichiers verrouillés
- ✅ **Support Annulation** : CancellationToken pour arrêt gracieux
- ✅ **Architecture Refactorisée** : Classe `BrowserPaths` centralisée
- ✅ **Documentation XML** : 150+ lignes de documentation IntelliSense


## 📊 Spécifications Techniques

- **Framework** : .NET 10.0 Windows
- **Version** : 1.0.5.0
- **UI** : Windows Forms (WinForms)
- **Configuration** : Release (optimisée)
- **Taille** : ~310 KB (sans runtime)
- **Prérequis** : Windows 10/11 (x64), .NET 10.0 Runtime

## 📁 Structure du Projet

```
Windows Cleaner/
├── Program.cs              # Point d'entrée
├── MainForm.cs             # Interface utilisateur principale
├── Cleaner.cs              # Logique de nettoyage
├── BrowserPaths.cs         # Chemins centralisés (v1.0.5)
├── Logger.cs               # Système de logging
├── Settings.cs             # Gestion des paramètres
├── ColoredProgressBar.cs   # Composant UI personnalisé
├── Output/v1.0.5/          # Build de release
└── scripts/                # Scripts utilitaires
```

## 📝 Logs et Paramètres

### Fichiers de Logs
```
%APPDATA%\WindowsCleaner\logs\windows-cleaner.log
```
Exportez les logs via **Fichier → Exporter les logs**

### Fichiers de Paramètres
```
%APPDATA%\WindowsCleaner\settings.json
```
Sauvegarde automatique de vos préférences

## 🔧 Scripts de Développement

Un script PowerShell pratique est inclus pour faciliter le développement :

```powershell
.\scripts\prepare_commit.ps1
```

**Le script effectue :**
1. Exécute `dotnet format` (propose l'installation si absent)
2. Compile avec `dotnet build` pour vérifier les erreurs
3. Propose `git add -A` + `git commit` pour un commit groupé

**Astuce** : Utilisez ce script avant de pousser vos changements pour maintenir un historique propre.

## 📦 Distribution

### Build Release
```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

### Installateur Inno Setup
Le fichier `windows-cleaner.iss` permet de créer un installateur Windows professionnel.

```powershell
# Avec Inno Setup installé
iscc windows-cleaner.iss
```

### Package Portable
Fichiers dans `Output/v1.0.5/` :
- `windows-cleaner.exe` - Exécutable principal
- `windows-cleaner.dll` - Assembly .NET
- `*.deps.json`, `*.runtimeconfig.json` - Configuration
- `run.bat` - Lanceur optionnel

## 📚 Documentation

- **[RELEASE_v1.0.5.md](RELEASE_v1.0.5.md)** - Notes de version détaillées
- **[COMPLETION_REPORT.md](COMPLETION_REPORT.md)** - Rapport des améliorations
- **[ADVANCED_FEATURES.md](ADVANCED_FEATURES.md)** - Guide des fonctionnalités avancées
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Guide d'utilisation des améliorations

## 🆕 Nouveautés v1.0.5 (6 décembre 2025)

### Refactorisation Majeure
- ✅ Classe `BrowserPaths` pour centraliser les chemins système
- ✅ Suppression de 20+ blocs `catch` vides silencieux
- ✅ Logging robuste dans toutes les méthodes
- ✅ Support `CancellationToken` pour annulation gracieuse
- ✅ Logger thread-safe avec `lock` pour opérations parallèles
- ✅ Documentation XML complète (150+ commentaires)

### Améliorations de Qualité
- ✅ 0 Erreurs de compilation
- ✅ 0 Avertissements critiques
- ✅ Gestion d'erreurs systématique
- ✅ Architecture maintainable et extensible

## 📜 Licence (MIT)

Ce projet est distribué sous licence MIT. Le texte complet de la licence est inclus ci-dessous et dans le fichier `LICENSE` à la racine du projet.

```
MIT License

Copyright (c) 2025 c.lecomte

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 👤 Auteur

**c.lecomte**

## ⚠️ Limitations

Cet outil est conçu pour être robuste mais ne gère pas tous les cas d'usage avancés :
- Fichiers verrouillés par d'autres processus (retry avec backoff)
- Profils multiples de navigateurs (support partiel)
- Nettoyage en profondeur du registre (non inclus)

**Utilisez avec précaution** et testez d'abord en mode Simuler.

## 🔗 Liens Utiles

- **Repository GitHub** : [christwadel65-ux/Nouveau-dossier](https://github.com/christwadel65-ux/Nouveau-dossier)
- **Issues** : [Signaler un bug](https://github.com/christwadel65-ux/Nouveau-dossier/issues)
- **Releases** : [Télécharger la dernière version](https://github.com/christwadel65-ux/Nouveau-dossier/releases)

## 🙏 Contribution

Les contributions sont les bienvenues ! N'hésitez pas à :
1. Fork le projet
2. Créer une branche (`git checkout -b feature/AmazingFeature`)
3. Commit vos changements (`git commit -m 'Add AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

---

**Windows Cleaner v1.0.5** | Build: Release | Date: 6 décembre 2025