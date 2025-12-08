# Windows Cleaner v1.0.6

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![Version](https://img.shields.io/badge/version-1.0.6-brightgreen.svg)](https://github.com/christwadel65-ux/Windows-Cleaner/releases)

Outil professionnel en C# (WinForms + CLI) pour nettoyer, analyser et optimiser votre système Windows. Interface moderne avec support du mode sombre, système de profils, CLI complet, statistiques, et 12 nouvelles fonctionnalités majeures.

## 🚀 Démarrage Rapide

### Compilation et Exécution
```powershell

dotnet build
dotnet run
```

### Exécution depuis Release
```powershell
.\Output\v1.0.6\windows-cleaner.exe
```

### Ligne de Commande (CLI)
```powershell
# Afficher l'aide
windows-cleaner.exe --help

# Nettoyage avec profil
windows-cleaner.exe --profile "Nettoyage Rapide"

# Mode simulation (test sans suppression)
windows-cleaner.exe --profile "Nettoyage Complet" --dry-run

# Lister les profils disponibles
windows-cleaner.exe --list-profiles

# Afficher les statistiques
windows-cleaner.exe --stats
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

### 💻 Nettoyage Développeur (v1.0.6)
- **Docker** : `docker system prune` pour libérer l'espace
- **Node.js** : Détection et suppression des `node_modules` anciens (> 30 jours)
- **Visual Studio** : Nettoyage des dossiers `obj`, `bin`, `.vs`
- **Python** : Suppression des caches `__pycache__` et fichiers `.pyc`
- **Git** : Optimisation avec `git gc --aggressive --prune=now`

### 🔒 Protection Vie Privée (v1.0.6)
- **Historique Exécuter** : Efface l'historique Win+R
- **Documents Récents** : Supprime la liste des documents récents
- **Timeline Windows** : Efface l'historique de la chronologie
- **Historique Recherche** : Nettoie l'historique de recherche Windows
- **Presse-papiers** : Vide le presse-papiers système

### 🎯 Nouvelles Fonctionnalités v1.0.6
- **🗂️ Système de Profils** : 4 profils prédéfinis + profils personnalisés en JSON
- **📊 Analyse d'Espace Disque** : Catégorisation par type, détection des plus gros fichiers
- **🔍 Détecteur de Doublons** : Hash MD5, filtres par extension, suppression intelligente
- **⏰ Planificateur de Tâches** : Intégration Windows Task Scheduler (quotidien/hebdo/mensuel)
- **📈 Statistiques et Rapports** : Historique complet, rapports HTML avec graphiques
- **💾 Backup et Restauration** : Points de restauration système, backup automatique < 24h
- **⌨️ Support CLI Complet** : Arguments --profile, --dry-run, --silent, --stats, --help
- **🔔 Alertes Intelligentes** : Monitoring proactif (disque < 10%, cache > 2GB, maintenance)
- **⚡ Optimisations Système** : TRIM SSD, compaction registre, nettoyage mémoire cache

### 🎨 Interface Moderne
- **Thème Sombre/Clair** : Mode sombre avec accents personnalisables
- **Barre de Progression** : Suivi visuel en temps réel
- **Mode Simulation** : Prévisualisation sans suppression (Dry Run)
- **Rapport Détaillé** : Aperçu des éléments à supprimer avant exécution
- **Annulation** : Arrêtez les opérations en cours à tout moment
- **Logs en Temps Réel** : Affichage des opérations dans l'interface

### 🛡️ Robustesse et Sécurité
- ✅ **Gestion d'Erreurs Complète** : Toutes les erreurs sont loggées et tracées
- ✅ **Thread-Safe** : Logger sécurisé pour opérations parallèles
- ✅ **Retry Logic** : Tentatives avec backoff pour fichiers verrouillés
- ✅ **Support Annulation** : CancellationToken pour arrêt gracieux
- ✅ **Architecture Modulaire** : 18 fichiers C#, ~5500 lignes de code
- ✅ **Mode Dry-Run** : Test sans suppression pour sécurité maximale
- ✅ **Backup Automatique** : Restauration possible < 24h après nettoyage
- ✅ **Points de Restauration** : Création automatique avant opérations critiques


## 📊 Spécifications Techniques

- **Framework** : .NET 10.0 Windows
- **Version** : 1.0.6.0
- **UI** : Windows Forms (WinForms) + CLI
- **Configuration** : Release (optimisée)
- **Taille** : ~400 KB (sans runtime)
- **Modules** : 18 fichiers C# (~5500 lignes)
- **Prérequis** : Windows 10/11 (x64), .NET 10.0 Runtime

## 📁 Structure du Projet

```
Windows Cleaner/
├── Program.cs              # Point d'entrée + CLI
├── MainForm.cs             # Interface utilisateur principale
├── Cleaner.cs              # Logique de nettoyage étendue
├── BrowserPaths.cs         # Chemins centralisés
├── Logger.cs               # Système de logging
├── Settings.cs             # Gestion des paramètres
├── ColoredProgressBar.cs   # Composant UI personnalisé
├── CleaningProfile.cs      # Système de profils (v1.0.6)
├── DiskAnalyzer.cs         # Analyse d'espace disque (v1.0.6)
├── TaskSchedulerManager.cs # Planification tâches (v1.0.6)
├── DuplicateFinder.cs      # Détection doublons (v1.0.6)
├── BackupManager.cs        # Backup et restauration (v1.0.6)
├── StatisticsManager.cs    # Statistiques et rapports (v1.0.6)
├── SmartAlerts.cs          # Alertes intelligentes (v1.0.6)
├── SystemOptimizer.cs      # Optimisations système (v1.0.6)
├── Output/v1.0.6/          # Build de release
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
Fichiers dans `Output/v1.0.6/` :
- `windows-cleaner.exe` - Exécutable principal
- `windows-cleaner.dll` - Assembly .NET
- `*.deps.json`, `*.runtimeconfig.json` - Configuration
- `app.ico` - Icône de l'application
- `run.bat` - Lanceur optionnel
- `README.md`, `PACKAGE_INFO.md` - Documentation

## 📚 Documentation

### Version 1.0.6
- **[NEW_FEATURES_v1.0.6.md](NEW_FEATURES_v1.0.6.md)** - Guide complet des 12 nouvelles fonctionnalités
- **[CHANGELOG.md](CHANGELOG.md)** - Historique détaillé des versions
- **[USAGE_EXAMPLES.md](USAGE_EXAMPLES.md)** - 9 scénarios pratiques d'utilisation
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Résumé de l'implémentation
- **[README_v1.0.6.md](README_v1.0.6.md)** - Documentation principale détaillée

### Versions Précédentes
- **[RELEASE_v1.0.5.md](RELEASE_v1.0.5.md)** - Notes de version v1.0.5
- **[COMPLETION_REPORT.md](COMPLETION_REPORT.md)** - Rapport des améliorations v1.0.5
- **[ADVANCED_FEATURES.md](ADVANCED_FEATURES.md)** - Guide des fonctionnalités avancées
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Guide d'utilisation

## 🆕 Nouveautés v1.0.6 (8 décembre 2025)

### 🎯 12 Fonctionnalités Majeures Ajoutées

#### 1. 🗂️ Système de Profils
- 4 profils prédéfinis : Rapide, Complet, Développeur, Vie Privée
- Profils personnalisés en JSON
- Import/Export de profils

#### 2. 📊 Analyse d'Espace Disque
- Catégorisation automatique par type de fichier (15+ catégories)
- Top 100 plus gros fichiers
- Graphiques et rapports détaillés

#### 3. 🔍 Détecteur de Doublons
- Hash MD5 pour identification précise
- Filtres par extension et taille
- Suppression intelligente avec Corbeille

#### 4. ⏰ Planificateur de Tâches
- Intégration Windows Task Scheduler
- Fréquences : Quotidien, Hebdomadaire, Mensuel
- Profils automatiques

#### 5. 📈 Statistiques et Rapports
- Historique complet des nettoyages
- Rapports HTML avec CSS intégré
- Export CSV pour analyse

#### 6. 💾 Backup et Restauration
- Points de restauration système
- Backup automatique avant nettoyage
- Restauration < 24h

#### 7. ⌨️ Support CLI Complet
- Arguments : `--profile`, `--dry-run`, `--silent`, `--help`, `--stats`, `--list-profiles`
- Codes de sortie pour automation
- Intégration scripts PowerShell/Batch

#### 8. 🔔 Alertes Intelligentes
- Monitoring espace disque (alerte < 10%)
- Alertes cache navigateurs (> 2GB)
- Rappels maintenance (tous les 7 jours)

#### 9. 💻 Nettoyage Développeur
- Docker, Node.js, Visual Studio, Python, Git
- Détection intelligente avec filtres d'âge
- 5-50 GB récupérés selon projets

#### 10. 🔒 Nettoyage Vie Privée
- Historique Exécuter, Documents récents, Timeline
- Historique recherche, Presse-papiers
- Effacement sécurisé des traces

#### 11. ⚡ Optimisations Système
- TRIM SSD automatique
- Compaction registre
- Nettoyage mémoire cache

#### 12. 🏗️ Architecture Étendue
- 8 nouveaux modules C# (2,480+ lignes)
- 15 nouvelles méthodes de nettoyage
- Exécution parallèle avec Task.Run
- 5 fichiers de documentation complète

### 📊 Statistiques de Développement
- **Lignes ajoutées** : 6,445+ insertions
- **Nouveaux fichiers** : 20 fichiers (code + docs)
- **Code total** : ~5,500 lignes
- **Compilation** : 0 erreurs, 0 avertissements

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

**Windows Cleaner v1.0.6** | Build: Release | Date: 8 décembre 2025 | [Télécharger](https://github.com/christwadel65-ux/Windows-Cleaner/releases/tag/v1.0.6)
