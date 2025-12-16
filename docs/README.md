# Windows Cleaner v2.0.0

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](../LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![Version](https://img.shields.io/badge/version-2.0.0-brightgreen.svg)](https://github.com/christwadel65-ux/Windows-Cleaner/releases)

Outil professionnel en C# (WinForms + CLI) pour nettoyer, analyser et optimiser votre système Windows. Interface moderne avec support du mode sombre, système de profils, CLI complet, statistiques, mise à jour automatique, et options de nettoyage développeur.

## 🆕 Nouveautés v2.0.0 (15 décembre 2025)

### ✨ Version Majeure - Refonte Complète

#### 🔄 Système de Mise à Jour Automatique
- **Vérification automatique au démarrage** : L'application vérifie les nouvelles versions sur GitHub
- **Menu "Aide > 🔄 Vérifier les mises à jour"** : Vérification manuelle à tout moment
- **Notification discrète** : Alerte dans la barre de statut si mise à jour disponible
- **Dialogue informatif** : Affichage de la version, date et notes de version
- **Ouverture directe** : Accès à la page de téléchargement en un clic
- **API GitHub** : Récupération automatique des dernières releases
- **Versionnage sémantique** : Comparaison intelligente des versions (X.Y.Z)
- **Contrôle utilisateur** : Pas de téléchargement automatique - vous décidez

**Exemple d'utilisation :**
1. Lancez l'application → Vérification automatique en arrière-plan
2. Si mise à jour disponible : `✨ Nouvelle version disponible : 2.0.0`
3. Cliquez sur **Aide > 🔄 Vérifier les mises à jour**
4. Dialogue avec détails → Clic sur "Oui" → Page GitHub s'ouvre
5. Téléchargez et installez la nouvelle version

#### 💻 Interface de Nettoyage Développeur
**Nouveau groupe "💻 Nettoyage Développeur"** avec 10 options spécialisées :

1. **📦 VS Code** - Nettoie le cache Visual Studio Code
2. **📦 NuGet** - Nettoie le cache de packages NuGet
3. **📦 Maven** - Nettoie le repository local Maven (~/.m2)
4. **📦 npm** - Nettoie le cache npm global
5. **🐳 Docker** - Nettoie images, conteneurs et volumes inutilisés
6. **📁 node_modules** - Supprime les vieux dossiers node_modules (> 30 jours)
7. **🔨 Visual Studio** - Nettoie les dossiers obj, bin, .vs
8. **🐍 Python** - Supprime les caches __pycache__ et fichiers .pyc
9. **📂 Git** - Optimise les repositories avec garbage collection
10. **🎮 Jeux (Steam/Epic)** - Nettoie les caches de jeux

**Intégration complète :**
- Profil "Nettoyage Développeur" enrichi avec toutes ces options
- Boutons "✅ Tout" et "❌ Rien" incluent les 10 nouvelles options
- Statistiques détaillées par type de cache dans les rapports HTML
- Sauvegarde automatique des sessions de nettoyage

#### 📊 Statistiques SSD Améliorées
- **Optimisations TRIM** : Compteur fonctionnel (affiche X session(s))
- **Vérifications SMART** : Compteur fonctionnel avec rapport détaillé
- **Détection multi-niveaux** : Win32_DiskDrive + Get-Volume pour compatibilité maximale
- **Rapport enrichi** : Modèle, statut, interface, taille, partitions, santé des volumes
- **Sauvegarde automatique** : Chaque optimisation crée une entrée dans les statistiques

**Format du rapport SMART :**
```
=== DISQUES PHYSIQUES ===
Disque: Samsung SSD 970 EVO Plus
Statut: OK
Interface: NVMe
Taille: 500 GB
Partitions: 3

=== VOLUMES ===
Lecteur: C:
Type: NTFS
Santé: Healthy
Taille: 465.75 GB (Libre: 123.45 GB)
```

## 📋 Historique des Versions

### v1.0.8 (12 décembre 2025)

#### 🔗 Suppression des Raccourcis Cassés
- **Détection automatique** des raccourcis (.lnk) dont la cible n'existe plus
- **Scan intelligent** : Bureau, Menu Démarrer, Documents Récents, Dossier Liens
- **Vérification via COM** (WScript.Shell) pour validation précise
- **Suppression sécurisée** avec support du mode Dry-Run
- **Aucun privilège admin requis**
- Améliore la propreté du bureau et des menus

#### ☑ Boutons de Sélection Rapide
- **Bouton "✅ Tout"** : Coche toutes les options de nettoyage en un clic
- **Bouton "❌ Rien"** : Décoche toutes les options rapidement
- **Emplacement** : Groupe Actions, à côté des boutons Simuler/Nettoyer
- **Couleurs intelligentes** :
  - 🟢 VERT VIF si tout est coché
  - 🔴 ROUGE VIF si rien n'est coché
  - 🟠 ORANGE VIF si sélection partielle
- **Infobulles explicites** : Description au survol de la souris
- **Bascule automatique** vers le profil "Personnalisé (manuel)"
- **Sélectif** : Affecte uniquement les options de nettoyage (préserve Mode verbeux et Rapport détaillé)

#### 🎨 Améliorations de l'Interface
- **Infobulles contextuelles** sur tous les boutons d'action
  - 🔍 Simuler : "Simuler le nettoyage sans supprimer (Mode test sûr)"
  - 🧹 Nettoyer : "Exécuter le nettoyage avec suppression réelle (Vérifiez d'abord)"
  - ✅ Tout : "Cocher toutes les options en un clic"
  - ❌ Rien : "Décocher toutes les options en un clic"
- **Design Material moderne** avec palette de couleurs vives
- **Feedback visuel immédiat** sur l'état de sélection

#### 🔧 Améliorations Techniques
- Migration complète vers **.NET 10.0-windows**
- Restructuration des méthodes d'énumération (correction erreurs CS1626)
- Optimisation mémoire et gestion des ressources
- Corrections de typage (AuditManager, BackupManager)
- **Système de tooltips** avec ToolTip .NET standard
- **Système de feedback visuel** avec détection d'état en temps réel
- Documentation enrichie et mise à jour

## 🆕 Nouveautés v1.0.8 (12 décembre 2025)

### ✨ Nouvelles Fonctionnalités

#### 🔗 Suppression des Raccourcis Cassés
- **Détection automatique** des raccourcis (.lnk) dont la cible n'existe plus
- **Scan intelligent** : Bureau, Menu Démarrer, Documents Récents, Dossier Liens
- **Vérification via COM** (WScript.Shell) pour validation précise
- **Suppression sécurisée** avec support du mode Dry-Run
- **Aucun privilège admin requis**
- Améliore la propreté du bureau et des menus

#### ☑ Boutons de Sélection Rapide
- **Bouton "✅ Tout"** : Coche toutes les options de nettoyage en un clic
- **Bouton "❌ Rien"** : Décoche toutes les options rapidement
- **Emplacement** : Groupe Actions, à côté des boutons Simuler/Nettoyer
- **Couleurs intelligentes** :
  - 🟢 VERT VIF si tout est coché
  - 🔴 ROUGE VIF si rien n'est coché
  - 🟠 ORANGE VIF si sélection partielle
- **Infobulles explicites** : Description au survol de la souris
- **Bascule automatique** vers le profil "Personnalisé (manuel)"
- **Sélectif** : Affecte uniquement les options de nettoyage (préserve Mode verbeux et Rapport détaillé)

### 🎨 Améliorations de l'Interface
- **Infobulles contextuelles** sur tous les boutons d'action
  - 🔍 Simuler : "Simuler le nettoyage sans supprimer (Mode test sûr)"
  - 🧹 Nettoyer : "Exécuter le nettoyage avec suppression réelle (Vérifiez d'abord)"
  - ✅ Tout : "Cocher toutes les options en un clic"
  - ❌ Rien : "Décocher toutes les options en un clic"
- **Design Material moderne** avec palette de couleurs vives
- **Feedback visuel immédiat** sur l'état de sélection

### 🔧 Améliorations Techniques
- Migration complète vers **.NET 10.0-windows**
- Restructuration des méthodes d'énumération (correction erreurs CS1626)
- Optimisation mémoire et gestion des ressources
- Corrections de typage (AuditManager, BackupManager)
- **Système de tooltips** avec ToolTip .NET standard
- **Système de feedback visuel** avec détection d'état en temps réel
- Documentation enrichie et mise à jour

## 🚀 Démarrage Rapide

### Compilation et Exécution
```powershell
# À la racine du repo
dotnet build src/WindowsCleaner/WindowsCleaner.csproj --configuration Release
dotnet run --project src/WindowsCleaner/WindowsCleaner.csproj
```

### Exécution depuis le dossier compilé
```powershell
# Après compilation Release
.\bin\Release\net10.0-windows\windows-cleaner.exe
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
- **Cache Navigateurs** : Chrome, Edge, Firefox, Brave, Opera, Vivaldi (fermeture automatique avant nettoyage)
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
- **Raccourcis Cassés** : Détecte et supprime les raccourcis (.lnk) dont la cible n'existe plus (Bureau, Menu Démarrer, Récents)

### 💻 Nettoyage Développeur (v1.0.6)
- **Docker** : `docker system prune` pour libérer l'espace
- **Node.js** : Détection et suppression des `node_modules` anciens (> 30 jours)
- **Visual Studio** : Nettoyage des dossiers `obj`, `bin`, `.vs`
- **Python** : Suppression des caches `__pycache__` et fichiers `.pyc`
- **Git** : Optimisation avec `git gc --aggressive --prune=now`
- **VS Code** : Nettoyage du cache (depuis AppData)
- **NuGet** : Suppression des packages anciens (> 30 jours)
- **Maven** : Nettoyage du repository cache (`~/.m2`)
- **npm** : Vidage du cache npm global
- **Jeux** : Steam et Epic Games caches

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
 - **📈 Statistiques et Rapports** : Rapports HTML pro (analyse disque, doublons, stats) générés et ouverts automatiquement (copie sur Bureau), historique complet, graphiques
- **💾 Backup et Restauration** : Points de restauration système, backup automatique < 24h
- **⌨️ Support CLI Complet** : Arguments --profile, --dry-run, --silent, --stats, --help
- **🔔 Alertes Intelligentes** : Monitoring proactif (disque < 10%, cache > 2GB, maintenance)
- **⚡ Optimisations Système** : TRIM SSD, compaction registre, nettoyage mémoire cache
- **🎯 Fermeture Auto Navigateurs** : Ferme automatiquement Chrome/Edge/Firefox avant nettoyage cache

### 🎨 Interface Moderne
- **Thème Sombre/Clair** : Mode sombre avec accents personnalisables
- **Barre de Progression** : Suivi visuel en temps réel
- **Mode Simulation** : Prévisualisation sans suppression (Dry Run)
- **Rapport Détaillé** : Aperçu des éléments à supprimer avant exécution
- **Annulation** : Arrêtez les opérations en cours à tout moment
- **Logs en Temps Réel** : Affichage des opérations dans l'interface
- **Sélection Rapide** : Boutons "☑ Tout" et "☐ Rien" pour cocher/décocher toutes les options en un clic

### 🛡️ Robustesse et Sécurité
- ✅ **Gestion d'Erreurs Complète** : Toutes les erreurs sont loggées et tracées
- ✅ **Thread-Safe** : Logger sécurisé pour opérations parallèles
- ✅ **Retry Logic Avancée** : 8 tentatives avec backoff exponentiel plafonné (fichiers verrouillés)
- ✅ **Attributs ReadOnly** : Retrait automatique avant suppression
- ✅ **Support Annulation** : CancellationToken pour arrêt gracieux
- ✅ **Architecture Modulaire** : 18 fichiers C#, ~5500 lignes de code
- ✅ **Mode Dry-Run** : Test sans suppression pour sécurité maximale
- ✅ **Backup Automatique** : Restauration possible < 24h après nettoyage
- ✅ **Points de Restauration** : Création automatique avant opérations critiques
- ✅ **Logs Intelligents** : Verbosité réduite pour fichiers verrouillés (niveau Debug)


## 📊 Spécifications Techniques

- **Framework** : .NET 10.0 Windows
- **Version** : 2.0.0
- **UI** : Windows Forms (WinForms) + CLI
- **Configuration** : Release (optimisée)
- **Taille** : ~371 KB DLL + ~199 KB EXE
- **Modules** : 21+ fichiers C# (~6500+ lignes)
- **Prérequis** : Windows 10/11 (x64), .NET 10.0 Runtime
- **Mise à jour** : Système automatique via GitHub API
- **Options de nettoyage** : 20+ options (standard + avancées + développeur)

## 📁 Structure du Projet

```
Windows Cleaner/
├── src/WindowsCleaner/
│   ├── WindowsCleaner.csproj
│   ├── Core/                # Cleaner, SystemOptimizer, BackupManager, Logger
│   ├── Features/            # DiskAnalyzer, DuplicateFinder, Profiles, UpdateManager
│   └── UI/                  # Program, MainForm, ColoredProgressBar, manifest, ico
├── docs/                    # README, guides et notes de version
│   ├── UPDATE_GUIDE.md      # Guide de mise à jour
│   ├── RELEASE_GUIDE.md     # Guide de publication
│   └── ...
├── scripts/                 # Scripts PowerShell
│   ├── prepare_release.ps1  # Automatisation des releases
│   └── ...
├── assets/                  # Ressources (icônes/images auxiliaires)
├── build/                   # Scripts d'installation (ex: Inno Setup)
└── bin/ obj/                # Générés (ignorés du dépôt)
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

## 📂 Structure du Dépôt

Le dépôt Git contient **uniquement le code source** pour maintenir une taille minimale :

```
├── src/WindowsCleaner/       # Code source et projet
├── docs/                     # Documentation (guides)
├── scripts/                  # Scripts PowerShell
├── build/                    # Scripts d'installation (.iss)
├── assets/                   # Icônes/ressources
├── LICENSE                   # Licence MIT
├── .gitignore                # Exclusions Git (bin/ obj/ *.iss)
└── create_icon.ps1           # Utilitaires
```

**Les dossiers EXCLUS du dépôt** (à générer localement) :
- `bin/` - Binaires compilés
- `obj/` - Fichiers intermédiaires
- `Output/` - Builds Release
- `publish-output/` - Sorties de publication

## 🔨 Build et Distribution

### Compiler une build Release
```powershell
dotnet build src/WindowsCleaner/WindowsCleaner.csproj --configuration Release
```

Build générée dans : `bin\Release\net10.0-windows\`

### Automatiser la préparation d'une release

Le script `prepare_release.ps1` automatise la mise à jour des numéros de version :

```powershell
# Mise à jour simple des fichiers
.\scripts\prepare_release.ps1 -Version 2.0.0

# Avec compilation et création du ZIP portable
.\scripts\prepare_release.ps1 -Version 2.0.0 -Build -CreateZip

# Tout automatique (fichiers + build + tag Git)
.\scripts\prepare_release.ps1 -Version 2.0.0 -Build -CreateZip -PushTag
```

Le script met à jour automatiquement :
- `WindowsCleaner.csproj` (Version, FileVersion, InformationalVersion)
- `MainForm.cs` (UpdateManager version)
- `app.manifest` (assemblyIdentity version)

### Créer un installateur

```powershell
# Avec Inno Setup compilé
iscc build/windows-cleaner.iss
```

### Package Portable
Fichiers à distribuer depuis `bin\Release\net10.0-windows\` :
- `windows-cleaner.exe` - Exécutable principal
- `windows-cleaner.dll` - Bibliothèque
- `*.deps.json`, `*.runtimeconfig.json` - Configuration
- `app.ico` - Icône de l'application

### Publication sur GitHub

1. Utilisez le script de préparation : `.\scripts\prepare_release.ps1 -Version X.Y.Z -Build -CreateZip -PushTag`
2. Créez une release sur GitHub : https://github.com/votre-username/Windows-Cleaner/releases/new
3. Attachez les fichiers : setup.exe, portable.zip
4. Publiez - Le système de mise à jour automatique détectera la nouvelle version

Consultez [RELEASE_GUIDE.md](docs/RELEASE_GUIDE.md) pour plus de détails.

## 📚 Documentation

### Version 2.0.0
- **[UPDATE_GUIDE.md](docs/UPDATE_GUIDE.md)** - Guide complet de mise à jour
- **[RELEASE_GUIDE.md](docs/RELEASE_GUIDE.md)** - Guide de publication des releases
- **[CHANGELOG.md](CHANGELOG.md)** - Historique détaillé des versions

### Version 1.0.6
- **[NEW_FEATURES_v1.0.6.md](NEW_FEATURES_v1.0.6.md)** - Guide complet des 12 nouvelles fonctionnalités
- **[USAGE_EXAMPLES.md](USAGE_EXAMPLES.md)** - 9 scénarios pratiques d'utilisation
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Résumé de l'implémentation
- **[README_v1.0.6.md](README_v1.0.6.md)** - Documentation principale détaillée

### Versions Précédentes
- **[RELEASE_v1.0.5.md](RELEASE_v1.0.5.md)** - Notes de version v1.0.5
- **[COMPLETION_REPORT.md](COMPLETION_REPORT.md)** - Rapport des améliorations v1.0.5
- **[ADVANCED_FEATURES.md](ADVANCED_FEATURES.md)** - Guide des fonctionnalités avancées
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Guide d'utilisation

---

## 🆕 Historique v1.0.6 (8 décembre 2025)

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

Copyright (c) 2025 C.L

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

**C.L (Skill_teams)**

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

**Windows Cleaner v1.0.8** | Build: Release | Date: 12 décembre 2025 | .NET 10.0-windows
