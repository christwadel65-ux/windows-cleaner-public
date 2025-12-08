# 🧹 Windows Cleaner v1.0.6

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![Version](https://img.shields.io/badge/version-1.0.6-brightgreen.svg)](CHANGELOG.md)

> **Outil professionnel tout-en-un pour nettoyer, optimiser et maintenir Windows**

Windows Cleaner est une application C# complète qui combine nettoyage système, analyse d'espace disque, détection de doublons, statistiques détaillées et automatisation via CLI. Conçu pour les particuliers, développeurs et entreprises.

---

## ✨ Fonctionnalités Principales

### 🎯 Système de Profils
- **4 profils prédéfinis** : Rapide, Complet, Développeur, Vie Privée
- **Profils personnalisés** avec import/export JSON
- **Sauvegarde automatique** des préférences

### 📊 Analyse et Rapports
- **Analyse complète du disque** avec catégorisation
- **Top fichiers volumineux** (configurable)
- **Statistiques détaillées** avec export HTML
- **Graphiques et tableaux** professionnels

### 🔍 Détection Intelligente
- **Doublons par hash MD5** avec calcul espace récupérable
- **Alertes proactives** (espace disque, cache, maintenance)
- **Recommandations personnalisées**

### ⏰ Automatisation
- **Planificateur de tâches** Windows intégré
- **Support CLI complet** pour scripts
- **Mode silencieux** pour automatisation

### 🛡️ Sécurité
- **Point de restauration** système avant nettoyage
- **Sauvegarde fichiers** avec restauration
- **Mode dry-run** pour tests

---

## 🚀 Installation et Utilisation

### Compilation
```powershell
cd "Windows Cleaner"
dotnet restore windows-cleaner.csproj
dotnet build windows-cleaner.csproj --configuration Release
```

### Exécution

#### Mode Interface Graphique (GUI)
```powershell
.\bin\Release\net10.0-windows\windows-cleaner.exe
```

#### Mode Ligne de Commande (CLI)
```powershell
# Aide
.\windows-cleaner.exe --help

# Nettoyage avec profil
.\windows-cleaner.exe --profile "Nettoyage Rapide"

# Mode simulation
.\windows-cleaner.exe --profile "Nettoyage Complet" --dry-run

# Mode silencieux (pour scripts)
.\windows-cleaner.exe --profile "Protection Vie Privée" --silent

# Lister les profils disponibles
.\windows-cleaner.exe --list-profiles

# Afficher les statistiques
.\windows-cleaner.exe --stats
```

---

## 🧹 Options de Nettoyage

### Standard
- ✅ **Corbeille** - Vidage complet sans confirmation
- ✅ **Fichiers Temporaires** - User et System Temp
- ✅ **Caches Navigateurs** - Chrome, Edge, Firefox
- ✅ **Windows Update** - Cache de téléchargement
- ✅ **Vignettes** - Fichiers thumbcache_*.db
- ✅ **Prefetch** - Dossier C:\Windows\Prefetch
- ✅ **Flush DNS** - Vidage cache DNS local

### Avancé
- ✅ **Journaux Système** (.evtx) ⚠️
- ✅ **Cache Installateurs** (C:\Windows\Installer) ⚠️
- ✅ **Fichiers Orphelins** (> 7 jours)
- ✅ **Journaux Applications** (LocalState) ⚠️
- ✅ **Cache Mémoire** - GC forcé

### Développeurs
- ✅ **Docker** - Images, conteneurs, volumes inutilisés
- ✅ **Node.js** - node_modules > 30 jours
- ✅ **Visual Studio** - Cache, obj/bin
- ✅ **Python** - __pycache__, .pyc
- ✅ **Git** - Optimisation repositories (gc)

### Vie Privée
- ✅ **Historique Exécuter** (Win+R)
- ✅ **Documents Récents**
- ✅ **Timeline Windows** 10/11
- ✅ **Historique Recherche**
- ✅ **Presse-papiers**

---

## 📋 Profils Prédéfinis

### 🚀 Nettoyage Rapide
Usage quotidien - **~2 minutes**
- Corbeille + Caches navigateurs
- Vignettes + Fichiers orphelins
- Idéal pour maintenance régulière

### 🔧 Nettoyage Complet
Maintenance approfondie - **~10 minutes**
- Toutes options standard + avancées
- Windows Update + Prefetch + DNS
- Avec sauvegarde automatique
- Recommandé mensuel

### 💻 Nettoyage Développeur
Spécialisé développeurs - **~5 minutes**
- node_modules + Python cache
- Visual Studio cache + obj/bin
- Git repos optimization
- Docker cleanup
- Parfait après projets

### 🔒 Protection Vie Privée
Effacement traces - **~3 minutes**
- Caches navigateurs complets
- Historiques et Timeline
- Documents récents + Recherche
- Presse-papiers
- Maximum confidentialité

---

## 📊 Exemples d'Utilisation

### Script PowerShell - Nettoyage Automatique
```powershell
# Nettoyage quotidien à 2h du matin
$profile = "Nettoyage Rapide"
$result = & "C:\Windows Cleaner\windows-cleaner.exe" --profile $profile --silent

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Nettoyage réussi"
} else {
    Write-Host "❌ Erreur nettoyage"
}
```

### Script Batch - Avec Rapport
```batch
@echo off
windows-cleaner.exe --profile "Nettoyage Complet"
windows-cleaner.exe --stats > rapport-%date%.txt
echo Rapport sauvegarde dans rapport-%date%.txt
```

### Code C# - Analyse Disque
```csharp
var result = await DiskAnalyzer.AnalyzeDirectory(
    @"C:\Users\VotreNom",
    topFileCount: 100,
    progress: Console.WriteLine
);

Console.WriteLine($"Total: {result.TotalScannedFiles} fichiers");
Console.WriteLine($"Top fichier: {result.LargestFiles[0].Path}");
```

### Code C# - Détection Doublons
```csharp
var result = await DuplicateFinder.FindDuplicates(
    @"C:\Photos",
    minFileSize: 1024 * 1024, // 1 MB
    extensions: new[] { ".jpg", ".png" }
);

Console.WriteLine($"Doublons: {result.TotalDuplicates}");
Console.WriteLine($"Espace récupérable: {result.TotalWastedSpace} octets");
```

---

## ⚙️ Configuration

### Droits Administrateur
Certaines fonctionnalités nécessitent des droits admin :
- ✅ Temp système (C:\Windows\Temp)
- ✅ Windows Update cache
- ✅ Prefetch
- ✅ Point de restauration
- ✅ Optimisations système

**Sans admin**, les fonctionnalités utilisateur fonctionnent normalement.

### Fichiers de Configuration
```
%AppData%\WindowsCleaner\
├── Profiles\          # Profils personnalisés (.json)
├── Statistics\        # Historique nettoyages
├── Backups\          # Sauvegardes temporaires (< 24h)
└── logs\             # Logs détaillés
```

---

## 📈 Statistiques et Rapports

### Suivi Automatique
Chaque nettoyage enregistre :
- Profil utilisé
- Fichiers supprimés
- Espace libéré
- Durée d'exécution
- Date/heure

### Export HTML
Générez un rapport professionnel :
```csharp
var reportPath = StatisticsManager.ExportHtmlReport();
Process.Start(reportPath); // Ouvre dans navigateur
```

Le rapport inclut :
- Statistiques globales
- Évolution 30 jours
- Top 50 dernières sessions
- Graphiques CSS modernes

---

## 🔔 Alertes Intelligentes

### Vérifications Automatiques
- ⚠️ Espace disque < 10% ou < 10 GB
- ⚠️ Cache navigateurs > 2 GB
- ⚠️ Dernier nettoyage > 7 jours
- ⚠️ Fichiers temporaires > 1 GB

### Utilisation
```csharp
// Vérifier et afficher alertes
SmartAlerts.PerformAllChecksAndAlert();

// Générer recommandations
var recommendations = SmartAlerts.GenerateRecommendations();
Console.WriteLine(recommendations);
```

---

## 📅 Planification Automatique

### Créer une Tâche
```csharp
var profile = CleaningProfile.CreateQuickProfile();

// Quotidien à 2h
TaskSchedulerManager.CreateDailyTask(
    "NettoyageQuotidien",
    profile,
    new TimeSpan(2, 0, 0)
);

// Hebdomadaire dimanche 10h
TaskSchedulerManager.CreateWeeklyTask(
    "NettoyageHebdo",
    profile,
    DayOfWeek.Sunday,
    new TimeSpan(10, 0, 0)
);
```

### Gérer les Tâches
```csharp
// Lister
var tasks = TaskSchedulerManager.ListTasks();

// Désactiver
TaskSchedulerManager.SetTaskEnabled("NettoyageQuotidien", false);

// Supprimer
TaskSchedulerManager.DeleteTask("NettoyageQuotidien");
```

---

## 🛡️ Sauvegarde et Restauration

### Avant Nettoyage Important
```csharp
// Point de restauration système
BackupManager.CreateSystemRestorePoint("Avant Windows Cleaner");

// Sauvegarde fichiers spécifiques
var files = new List<string> { "C:\\Important\\file.txt" };
var backupPath = BackupManager.CreateBackup(files, "PreClean");
```

### Restauration si Problème
```csharp
// Lister sauvegardes disponibles
var backups = BackupManager.ListBackups();

// Restaurer
BackupManager.RestoreBackup(backups[0].Path);
```

---

## 🏗️ Architecture

### Modules Principaux
```
WindowsCleaner/
├── Cleaner.cs              # Moteur de nettoyage
├── CleaningProfile.cs      # Système de profils
├── DiskAnalyzer.cs         # Analyse espace disque
├── DuplicateFinder.cs      # Détection doublons
├── TaskSchedulerManager.cs # Planification tâches
├── BackupManager.cs        # Sauvegarde/Restauration
├── StatisticsManager.cs    # Statistiques/Rapports
├── SmartAlerts.cs          # Alertes intelligentes
├── SystemOptimizer.cs      # Optimisations avancées
├── Logger.cs               # Système de logs
├── Settings.cs             # Configuration
├── MainForm.cs             # Interface GUI
└── Program.cs              # Point d'entrée + CLI
```

### Technologies
- **.NET 10.0** - Framework moderne
- **Windows Forms** - Interface graphique
- **P/Invoke** - Appels API Windows natifs
- **System.Text.Json** - Sérialisation JSON
- **Task Parallel Library** - Parallélisme

---

## 📚 Documentation

- **[NEW_FEATURES_v1.0.6.md](NEW_FEATURES_v1.0.6.md)** - Guide complet nouvelles fonctionnalités
- **[USAGE_EXAMPLES.md](USAGE_EXAMPLES.md)** - Exemples pratiques détaillés
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Résumé d'implémentation
- **[CHANGELOG.md](CHANGELOG.md)** - Historique des versions
- **[ADVANCED_FEATURES.md](ADVANCED_FEATURES.md)** - Fonctionnalités avancées

---

## ⚠️ Avertissements

### Options Risquées (⚠️)
Ces options sont **désactivées par défaut** :
- Journaux système (.evtx)
- Cache installateurs (C:\Windows\Installer)
- Journaux applications (LocalState)

**Ne les activez que si vous savez ce que vous faites !**

### Recommandations
1. ✅ **Testez avec --dry-run** avant nettoyage réel
2. ✅ **Fermez les navigateurs** avant nettoyage
3. ✅ **Créez un point de restauration** pour nettoyage complet
4. ✅ **Sauvegardez vos données** importantes
5. ✅ **Vérifiez les logs** en cas de problème

---

## 🐛 Dépannage

### "Droits insuffisants"
→ **Solution** : Lancez en tant qu'administrateur

### "Profil introuvable"
→ **Solution** : `windows-cleaner.exe --list-profiles`

### "Fichiers verrouillés"
→ **Solution** : Fermez les applications et navigateurs

### "Erreur compilation"
→ **Solution** : Vérifiez .NET 10.0 SDK installé

### Consulter les Logs
```powershell
notepad "%AppData%\WindowsCleaner\logs\cleaner.log"
```

---

## 📊 Statistiques du Projet

| Métrique | Valeur |
|----------|--------|
| **Version** | 1.0.6 |
| **Fichiers source** | 18 fichiers .cs |
| **Lignes de code** | ~5,500 |
| **Fonctionnalités** | 27 |
| **Options nettoyage** | 27 |
| **Profils prédéfinis** | 4 |
| **Documentation** | 5 fichiers .md |

---

## 🔄 Mises à Jour

### Version Actuelle
- **v1.0.6** (8 décembre 2025)
- Toutes fonctionnalités implémentées
- Production ready

### Prochaine Version (v1.1.0)
- Interface avec onglets
- Graphiques intégrés
- Mode portable
- Multi-langues

---

## 📞 Support

### Problème ?
1. Consultez **[USAGE_EXAMPLES.md](USAGE_EXAMPLES.md)**
2. Vérifiez **logs** : `%AppData%\WindowsCleaner\logs`
3. Testez **--dry-run**
4. Reportez sur **GitHub Issues**

### Contact
- **GitHub** : [Votre Repo]
- **Email** : [Votre Email]

---

## 📜 Licence

MIT License - Voir [LICENSE](LICENSE) pour détails.

---

## 🙏 Remerciements

Merci à tous les utilisateurs et contributeurs !

---

<div align="center">

**Windows Cleaner v1.0.6**  
*Nettoyez, Optimisez, Automatisez*

Made with ❤️ in C#

[⬆ Retour en haut](#-windows-cleaner-v106)

</div>
