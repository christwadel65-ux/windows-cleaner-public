# 📦 Package Information - Windows Cleaner v1.0.6

## 📋 Informations Générales

| Propriété | Valeur |
|-----------|--------|
| **Nom** | Windows Cleaner |
| **Version** | 1.0.6 |
| **Date de Build** | 2025-01-xx |
| **Plateforme** | Windows 10/11 (x64) |
| **Framework** | .NET 10.0 |
| **Type** | Application WinForms + CLI |
| **Licence** | MIT License |

## 📊 Contenu du Package

### Fichiers Exécutables
```
windows-cleaner.exe          (Exécutable principal)
windows-cleaner.dll          (Bibliothèque de l'application)
```

### Fichiers de Configuration
```
windows-cleaner.deps.json    (Dépendances .NET)
windows-cleaner.runtimeconfig.json (Configuration runtime)
```

### Scripts et Documentation
```
run.bat                      (Script de lancement rapide)
README.md                    (Guide utilisateur)
PACKAGE_INFO.md             (Ce fichier)
```

## 🎯 Nouveautés v1.0.6

### 🔥 Fonctionnalités Majeures Ajoutées

#### 1. Système de Profils
- **4 profils prédéfinis** : Rapide, Complet, Développeur, Vie Privée
- **Profils personnalisés** en JSON
- **Import/Export** de profils

#### 2. Analyse d'Espace Disque
- **Catégorisation automatique** par type de fichier
- **Top 100 plus gros fichiers**
- **Graphiques et rapports** détaillés

#### 3. Détecteur de Doublons
- **Hash MD5** pour identification précise
- **Filtres par extension** et taille
- **Suppression avec Corbeille**

#### 4. Planificateur de Tâches
- **Intégration Windows Task Scheduler**
- **Fréquences** : Quotidien, Hebdomadaire, Mensuel
- **Profils automatiques**

#### 5. Statistiques et Rapports
- **Historique complet** des nettoyages
- **Rapports HTML** avec graphiques
- **Export CSV** pour analyse

#### 6. Sauvegarde et Restauration
- **Points de restauration** système
- **Backup automatique** avant nettoyage
- **Restauration < 24h**

#### 7. Support CLI Complet
- **Arguments multiples** : --profile, --dry-run, --silent
- **Intégration scripts** PowerShell/Batch
- **Codes de sortie** pour automation

#### 8. Alertes Intelligentes
- **Monitoring espace disque** (< 10%)
- **Alertes cache** navigateurs (> 2GB)
- **Rappels maintenance** (7 jours)

#### 9. Nettoyage Étendu
- **Docker** : `docker system prune`
- **Node.js** : `node_modules` anciens
- **Visual Studio** : `obj/bin/.vs`
- **Python** : `__pycache__` et `.pyc`
- **Git** : `git gc --aggressive`

#### 10. Nettoyage Vie Privée
- **Historique Exécuter** (Win+R)
- **Documents récents**
- **Timeline Windows**
- **Historique recherche**
- **Presse-papiers**

#### 11. Optimisations Système
- **TRIM SSD** automatique
- **Compaction registre**
- **Nettoyage mémoire cache**
- **Optimisation Prefetch**

### 🛠️ Améliorations Techniques

#### Performance
- **Exécution parallèle** des tâches de nettoyage
- **Async/Await** pour opérations I/O
- **Cancellation** pour opérations longues

#### Sécurité
- **Mode dry-run** : test sans suppression
- **Backup automatique** : restauration 24h
- **Vérifications admin** : sécurité renforcée
- **Logs détaillés** : traçabilité complète

#### Extensibilité
- **Architecture modulaire** : 18 fichiers C#
- **Profils JSON** : personnalisation facile
- **API publique** : intégration possible

## 📈 Statistiques du Projet

### Lignes de Code
```
Module                    Lignes    Statut
----------------------------------------
CleaningProfile.cs        250       ✅ Nouveau
DiskAnalyzer.cs          320       ✅ Nouveau
TaskSchedulerManager.cs  400       ✅ Nouveau
DuplicateFinder.cs       280       ✅ Nouveau
BackupManager.cs         270       ✅ Nouveau
StatisticsManager.cs     380       ✅ Nouveau
SmartAlerts.cs           260       ✅ Nouveau
SystemOptimizer.cs       320       ✅ Nouveau
Cleaner.cs               1620      ✅ Étendu (+651)
Program.cs               235       ✅ Étendu (+220)
MainForm.cs              950       ⚠️ Non modifié
----------------------------------------
TOTAL (nouveaux)         2480      ✅
TOTAL (modifications)    871       ✅
TOTAL (projet)           ~5500     ✅
```

### Fichiers de Documentation
```
NEW_FEATURES_v1.0.6.md        (Guide complet)
IMPLEMENTATION_SUMMARY.md     (Résumé implémentation)
USAGE_EXAMPLES.md            (Exemples pratiques)
CHANGELOG.md                 (Historique versions)
README_v1.0.6.md            (Documentation principale)
```

## 🔧 Configuration Requise

### Minimale
- **OS** : Windows 10 version 1809 (October 2018 Update)
- **CPU** : Processeur x64 1 GHz+
- **RAM** : 512 MB minimum
- **.NET** : Runtime .NET 10.0 (inclus avec Windows Update récent)
- **Disque** : 50 MB pour l'application + logs

### Recommandée
- **OS** : Windows 11 (dernière version)
- **CPU** : Processeur x64 2 GHz+ (multi-core)
- **RAM** : 2 GB ou plus
- **.NET** : Runtime .NET 10.0 (dernière version)
- **Droits** : Administrateur pour fonctionnalités complètes

### Droits d'Administration Nécessaires Pour
- ✅ Nettoyage Windows Update
- ✅ Vidage Prefetch
- ✅ Points de restauration système
- ✅ Optimisation SSD (TRIM)
- ✅ Compaction registre
- ✅ Planification tâches système

### Optionnel (Selon Fonctionnalités)
- **Docker Desktop** (pour nettoyage Docker)
- **Git** (pour optimisation Git)
- **Node.js** (pour nettoyage node_modules)
- **Visual Studio** (pour nettoyage VS cache)
- **Python** (pour nettoyage Python cache)

## 📂 Structure d'Installation

### Après Installation
```
C:\Program Files\WindowsCleaner\  (ou dossier choisi)
├── windows-cleaner.exe
├── windows-cleaner.dll
├── windows-cleaner.deps.json
├── windows-cleaner.runtimeconfig.json
├── run.bat
├── README.md
└── PACKAGE_INFO.md
```

### Données Utilisateur (Auto-créé)
```
%AppData%\WindowsCleaner\
├── Profiles\
│   ├── Nettoyage Rapide.json
│   ├── Nettoyage Complet.json
│   ├── Nettoyage Développeur.json
│   ├── Protection Vie Privée.json
│   └── [profils personnalisés].json
│
├── Statistics\
│   ├── history.json
│   └── reports\
│       └── [rapports HTML]
│
├── Backups\
│   └── [sauvegardes temporaires < 24h]
│
└── logs\
    └── cleaner.log
```

## 🚀 Démarrage Rapide

### 1. Première Exécution (GUI)
```batch
# Double-clic sur
run.bat

# Ou directement
windows-cleaner.exe
```

### 2. Test Sans Risque
```batch
# Mode dry-run (simulation)
windows-cleaner.exe --profile "Nettoyage Complet" --dry-run
```

### 3. Nettoyage Automatisé
```batch
# Nettoyage quotidien silencieux
windows-cleaner.exe --profile "Nettoyage Rapide" --silent
```

### 4. Analyse d'Espace
```batch
# Via GUI : Menu "Outils" > "Analyser l'espace disque"
# Via CLI : Fonctionnalité accessible via profiles
```

## 🔄 Migration depuis v1.0.5

### Étapes
1. ✅ **Arrêtez** l'ancienne version si en cours
2. ✅ **Sauvegardez** vos paramètres (optionnel, auto-préservés)
3. ✅ **Remplacez** les fichiers dans le dossier d'installation
4. ✅ **Lancez** la nouvelle version

### Compatibilité
- ✅ **Paramètres préservés** : Vos options sauvegardées sont compatibles
- ✅ **Logs conservés** : Historique accessible dans `%AppData%\WindowsCleaner\logs`
- ⚠️ **Statistiques réinitialisées** : Nouvelle fonctionnalité, historique commence à v1.0.6

### Nouveautés Visibles
- ✅ Nouvelle commande `--help` avec toutes les options
- ✅ Nouveaux profils disponibles via `--list-profiles`
- ✅ Statistiques accessibles via `--stats`

## 📊 Performances

### Temps d'Exécution Moyen (SSD)
```
Nettoyage Rapide          : 5-15 secondes
Nettoyage Complet        : 30-120 secondes
Nettoyage Développeur    : 60-300 secondes (dépend de node_modules)
Analyse Espace Disque    : 10-60 secondes (dépend de la taille)
Détection Doublons       : 30-600 secondes (dépend du nombre de fichiers)
```

### Espace Libéré Typique
```
Nettoyage Rapide          : 500 MB - 2 GB
Nettoyage Complet        : 2 GB - 10 GB
Nettoyage Développeur    : 5 GB - 50 GB (si beaucoup de projets)
Protection Vie Privée    : 100 MB - 500 MB
```

## ⚠️ Avertissements

### ⚠️ Nettoyage Développeur
- **node_modules** : Peut supprimer des dépendances actives (filtre 30 jours)
- **Git gc** : Peut prendre du temps sur gros repos
- **Solution** : Utilisez `--dry-run` d'abord

### ⚠️ Détecteur de Doublons
- **Hash MD5** : Prend du temps sur gros disques
- **Suppression** : Vérifiez les résultats avant suppression
- **Solution** : Commencez par un dossier spécifique

### ⚠️ Points de Restauration
- **Espace requis** : Peut nécessiter 1-5 GB d'espace
- **Activation** : Doit être activé dans Windows
- **Solution** : Vérifiez paramètres Protection Système

## 📞 Support et Documentation

### Documentation Complète
Consultez les fichiers markdown dans le dossier source du projet :
- `NEW_FEATURES_v1.0.6.md` - Guide détaillé de toutes les fonctionnalités
- `USAGE_EXAMPLES.md` - 9 scénarios pratiques avec exemples
- `CHANGELOG.md` - Historique complet des versions

### Aide CLI
```batch
windows-cleaner.exe --help
```

### Logs
Tous les logs sont dans : `%AppData%\WindowsCleaner\logs\cleaner.log`

### Dépannage Rapide
| Problème | Solution |
|----------|----------|
| "Accès refusé" | Lancez en tant qu'administrateur |
| "Runtime introuvable" | Installez .NET 10.0 Runtime |
| "Profil non trouvé" | Utilisez `--list-profiles` pour voir disponibles |
| Nettoyage lent | Normal pour première exécution avec beaucoup de fichiers |

## 📜 Licence et Crédits

### Licence
MIT License - Voir fichier LICENSE dans le dossier source

### Utilisation
- ✅ Usage personnel gratuit
- ✅ Usage professionnel autorisé
- ✅ Modification et redistribution permises
- ⚠️ Sans garantie explicite

### Crédits
- **Développement** : c.lecomte
- **Version** : 1.0.6
- **Framework** : .NET 10.0, Windows Forms
- **Build** : MSBuild / Visual Studio

---

**Windows Cleaner v1.0.6**  
*Libérez, Optimisez, Automatisez*  
© 2025 - Tous droits réservés
