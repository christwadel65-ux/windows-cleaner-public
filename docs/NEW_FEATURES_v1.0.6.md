# Windows Cleaner v1.0.6 - Nouvelles Fonctionnalités

## 🎉 Vue d'Ensemble

Windows Cleaner v1.0.6 introduit des fonctionnalités majeures pour transformer votre expérience de nettoyage Windows :

- 🎯 **Profils de Nettoyage** personnalisés et prédéfinis
- 📊 **Analyse de l'Espace Disque** avec visualisation détaillée
- ⏰ **Planificateur de Tâches** pour nettoyage automatique
- 🔍 **Détecteur de Doublons** avec hash MD5
- 📈 **Statistiques et Rapports** détaillés (HTML)
- 🛡️ **Système de Sauvegarde** avec point de restauration
- 💻 **Support CLI** complet pour automatisation
- 🔔 **Alertes Intelligentes** proactives
- 🧹 **Nettoyage Étendu** : Docker, Node.js, Python, Git, VS
- 🎯 **Caches Applicatifs** : VS Code, NuGet, Maven, npm, Steam, Epic
- 🔒 **Nettoyage Vie Privée** : historique, timeline, presse-papiers
- 🎯 **Fermeture Auto Navigateurs** : Chrome, Edge, Firefox, Brave, Opera, Vivaldi
- ⚡ **Optimisations SSD** : TRIM, SMART, défragmentation légère
- ⚡ **Retry Logic Améliorée** : 8 tentatives avec backoff intelligent

---

## 🎯 1. Profils de Nettoyage

### Profils Prédéfinis

#### **Nettoyage Rapide**
- Corbeille
- Caches navigateurs
- Vignettes
- Fichiers orphelins (> 7 jours)

#### **Nettoyage Complet**
- Toutes les options standard
- Windows Update cache
- Prefetch
- Flush DNS
- Options avancées
- Avec sauvegarde automatique

#### **Nettoyage Développeur**
- node_modules anciens
- Cache Python (__pycache__)
- Dossiers obj/bin Visual Studio
- Cache Git (gc --aggressive)
- Images Docker inutilisées

#### **Protection Vie Privée**
- Caches navigateurs
- Historique Exécuter (Win+R)
- Documents récents
- Timeline Windows
- Historique recherche
- Presse-papiers

### Utilisation

```csharp
// Créer un profil personnalisé
var profile = new CleaningProfile
{
    Name = "Mon Profil",
    Description = "Nettoyage hebdomadaire",
    CleanBrowsers = true,
    CleanNodeModules = true,
    CreateBackup = true
};

// Sauvegarder
ProfileManager.SaveProfile(profile);

// Charger et utiliser
var loaded = ProfileManager.LoadProfile("Mon Profil");
var options = loaded.ToCleanerOptions(dryRun: false);
Cleaner.RunCleanup(options);
```

---

## 📊 2. Analyse de l'Espace Disque

Analyse complète de votre disque avec :

- **Top fichiers volumineux** (configurable : 20, 50, 100)
- **Catégorisation automatique** par type (vidéos, images, documents, etc.)
- **Calcul des pourcentages** d'utilisation
- **Dossiers les plus volumineux**

### Utilisation

```csharp
var result = await DiskAnalyzer.AnalyzeDirectory(
    "C:\\Users\\YourName", 
    topFileCount: 100,
    progress: msg => Console.WriteLine(msg)
);

Console.WriteLine($"Total: {result.TotalScannedFiles} fichiers");
Console.WriteLine($"Taille: {result.TotalScannedSize} octets");

foreach (var category in result.Categories)
{
    Console.WriteLine($"{category.Name}: {category.FormattedSize} ({category.Percentage:F1}%)");
}
```

---

## ⏰ 3. Planificateur de Tâches

Planifiez des nettoyages automatiques avec Windows Task Scheduler.

### Fonctionnalités

- Tâches **quotidiennes**, **hebdomadaires**, **mensuelles**
- Utilise les **profils de nettoyage**
- Exécution en **arrière-plan**
- Gestion complète (création, suppression, activation/désactivation)

### Utilisation

```csharp
// Créer une tâche quotidienne à 3h du matin
var profile = CleaningProfile.CreateQuickProfile();
TaskSchedulerManager.CreateDailyTask(
    "NettoyageQuotidien", 
    profile, 
    new TimeSpan(3, 0, 0)
);

// Tâche hebdomadaire le dimanche à 10h
TaskSchedulerManager.CreateWeeklyTask(
    "NettoyageHebdo",
    profile,
    DayOfWeek.Sunday,
    new TimeSpan(10, 0, 0)
);

// Lister toutes les tâches
var tasks = TaskSchedulerManager.ListTasks();
```

---

## 🔍 4. Détecteur de Doublons

Trouve les fichiers dupliqués par hash MD5 pour libérer de l'espace.

### Fonctionnalités

- Hash **MD5** rapide et fiable
- Filtrage par **taille minimale**
- Filtrage par **extensions**
- Calcul de l'**espace récupérable**
- Déplacement vers **corbeille** (sécurisé)

### Utilisation

```csharp
var result = await DuplicateFinder.FindDuplicates(
    "C:\\Users\\YourName\\Documents",
    minFileSize: 1024 * 1024, // 1 MB
    extensions: new[] { ".jpg", ".png", ".mp4" },
    progress: msg => Console.WriteLine(msg)
);

Console.WriteLine($"Doublons: {result.TotalDuplicates}");
Console.WriteLine($"Espace récupérable: {result.TotalWastedSpace} octets");

// Supprimer les doublons (garder le premier de chaque groupe)
foreach (var group in result.DuplicateGroups)
{
    var toDelete = group.Files.Skip(1).ToList();
    DuplicateFinder.DeleteDuplicates(toDelete, moveToRecycleBin: true);
}
```

---

## 📈 5. Statistiques et Rapports

Suivez l'historique de vos nettoyages avec statistiques détaillées.

### Fonctionnalités

- **Enregistrement automatique** de chaque session
- Statistiques **globales** et sur **30 jours**
- Export **HTML** professionnel
- Graphiques et tableaux

### Utilisation

```csharp
// Enregistrer une session
StatisticsManager.RecordCleaningSession(new CleaningStatistics
{
    ProfileUsed = "Nettoyage Rapide",
    FilesDeleted = 1234,
    BytesFreed = 5368709120, // 5 GB
    Duration = TimeSpan.FromMinutes(2)
});

// Consulter les stats
var totalBytes = StatisticsManager.GetTotalBytesFreed();
var last30Days = StatisticsManager.GetRecentStatistics(30);

// Exporter un rapport HTML
var reportPath = StatisticsManager.ExportHtmlReport();
Process.Start(reportPath); // Ouvre dans le navigateur
```

---

## 🛡️ 6. Système de Sauvegarde

Protection avant nettoyage avec sauvegarde et restauration.

### Fonctionnalités

- **Point de restauration système** Windows
- **Sauvegarde des fichiers** avant suppression
- **Restauration complète** possible
- **Nettoyage automatique** des sauvegardes > 24h

### Utilisation

```csharp
// Créer un point de restauration
BackupManager.CreateSystemRestorePoint("Avant nettoyage Windows Cleaner");

// Sauvegarder des fichiers
var filesToBackup = new List<string> { "C:\\Temp\\important.txt" };
var backupPath = BackupManager.CreateBackup(filesToBackup, "PreClean");

// Restaurer si nécessaire
BackupManager.RestoreBackup(backupPath);

// Lister les sauvegardes
var backups = BackupManager.ListBackups();
```

---

## 💻 7. Support Ligne de Commande (CLI)

Automatisez vos nettoyages avec des scripts PowerShell/Batch.

### Commandes Disponibles

```powershell
# Aide
windows-cleaner.exe --help

# Nettoyage avec profil
windows-cleaner.exe --profile "Nettoyage Rapide"

# Mode simulation (dry-run)
windows-cleaner.exe --profile "Nettoyage Complet" --dry-run

# Mode silencieux (pour scripts)
windows-cleaner.exe --profile "Protection Vie Privée" --silent

# Lister les profils
windows-cleaner.exe --list-profiles

# Afficher les statistiques
windows-cleaner.exe --stats
```

### Exemple Script PowerShell

```powershell
# Nettoyage automatique quotidien
$result = & "C:\Program Files\WindowsCleaner\windows-cleaner.exe" --profile "Nettoyage Rapide" --silent

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Nettoyage réussi"
} else {
    Write-Host "❌ Erreur lors du nettoyage"
}
```

---

## 🔔 8. Alertes Intelligentes

Notifications proactives pour maintenir votre système en bon état.

### Vérifications Automatiques

- **Espace disque < 10%** ou < 10 GB
- **Cache navigateurs > 2 GB**
- **Dernier nettoyage > 7 jours**
- **Fichiers temporaires > 1 GB**

### Utilisation

```csharp
// Vérifier et afficher les alertes
SmartAlerts.PerformAllChecksAndAlert();

// Générer des recommandations
var recommendations = SmartAlerts.GenerateRecommendations();
Console.WriteLine(recommendations);

// Vérifications individuelles
var (alertNeeded, message) = SmartAlerts.CheckDiskSpace();
if (alertNeeded)
{
    MessageBox.Show(message, "Alerte", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

---

## 🧹 9. Nettoyages Étendus

### Docker
```csharp
// Nettoie images, conteneurs, volumes inutilisés
options.CleanDocker = true;
```

### Node.js
```csharp
// Supprime node_modules > 30 jours
options.CleanNodeModules = true;
```

### Visual Studio
```csharp
// Cache VS, dossiers obj/bin
options.CleanVisualStudio = true;
```

### Python
```csharp
// __pycache__, fichiers .pyc
options.CleanPythonCache = true;
```

### Git
```csharp
// git gc --aggressive sur tous les repos
options.CleanGitCache = true;
```

---

## 🔒 10. Nettoyage Vie Privée

### Historique Exécuter (Win+R)
```csharp
options.CleanRunHistory = true;
```

### Documents Récents
```csharp
options.CleanRecentDocuments = true;
```

### Timeline Windows 10/11
```csharp
options.CleanWindowsTimeline = true;
```

### Historique de Recherche
```csharp
options.CleanSearchHistory = true;
```

### Presse-papiers
```csharp
options.CleanClipboard = true;
```

---

## 🎯 11. Fermeture Automatique des Navigateurs

### Détection et Fermeture Intelligente

Avant de nettoyer les caches navigateurs, Windows Cleaner peut **fermer automatiquement** les navigateurs en cours d'exécution pour éviter les fichiers verrouillés.

**Navigateurs supportés** :
- Google Chrome
- Microsoft Edge
- Mozilla Firefox
- Brave
- Opera
- Vivaldi

### Utilisation

```csharp
var options = new CleanerOptions
{
    CleanBrowsers = true,
    CloseBrowsersIfNeeded = true // Activé par défaut
};

Cleaner.RunCleanup(options);
```

**Comportement** :
1. Détection automatique des processus navigateurs
2. Tentative de fermeture propre (`CloseMainWindow()`)
3. Fermeture forcée (`Kill()`) après 3s si nécessaire
4. Attente de 1.5s pour libération des fichiers
5. Nettoyage des caches

**Note** : En mode `DryRun`, les navigateurs ne sont **pas** fermés.

---

## ⚡ 12. Améliorations de Robustesse

### Retry Logic Avancée

**Avant** : 5 tentatives avec backoff exponentiel illimité
**Maintenant** :
- **8 tentatives** pour les fichiers
- **6 tentatives** pour les dossiers
- **Backoff plafonné** : 2s (fichiers), 2.5s (dossiers)
- **Retrait automatique** de l'attribut `ReadOnly`
- **Logs intelligents** : niveau `Debug` pour fichiers verrouillés/protégés

### Gestion des Attributs ReadOnly

```csharp
// Retrait automatique avant suppression
var attributes = File.GetAttributes(filePath);
if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
{
    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
}
```

### Réduction du Bruit dans les Logs

Les fichiers verrouillés ou protégés par le système sont maintenant loggés en niveau `Debug` au lieu de `Warning`, réduisant les faux positifs dans les journaux.

---

## 📦 13. Nettoyage des Caches Applicatifs

### VS Code
```csharp
options.CleanVsCodeCache = true;
```

### NuGet (packages anciens > 30 jours)
```csharp
options.CleanNugetCache = true;
```

### Maven
```csharp
options.CleanMavenCache = true;
```

### npm Global
```csharp
options.CleanNpmCache = true;
```

### Jeux (Steam, Epic Games)
```csharp
options.CleanGameCaches = true;
```

---

## 💾 14. Optimisations SSD Avancées

### Activation de l'Optimisation SSD

```csharp
var options = new CleanerOptions
{
    OptimizeSsd = true,        // TRIM et défragmentation légère
    CheckDiskHealth = true     // Vérification SMART
};

Cleaner.RunCleanup(options);
```

### Fonctionnalités

**Optimisation TRIM** :
- Lance `defrag.exe /L` pour analyser les volumes
- Applique TRIM aux SSD détectés
- Réduit la fragmentation

**Vérification SMART** :
- Récupère les données SMART de chaque disque
- Affiche le statut de santé
- Alerte si des anomalies détectées
- Fournit la capacité totale en GB

### Exemple de Rapport SMART

```
Disque: Samsung SSD 970 EVO
Santé: OK
Taille: 500 GB

Disque: WDC WD10EZEX
Santé: OK
Taille: 1000 GB
```

---

## 🚀 Compilation et Utilisation

### Compiler

```powershell
cd "$env:USERPROFILE\Documents\dev_pyt\Windows Cleaner"
dotnet restore
dotnet build --configuration Release
```

### Exécuter

```powershell
# Mode GUI
.\bin\Release\net10.0-windows\windows-cleaner.exe

# Mode CLI
.\bin\Release\net10.0-windows\windows-cleaner.exe --help
```

---

## 📝 Notes Importantes

1. **Droits Administrateur** : Requis pour la plupart des fonctionnalités avancées
2. **Sauvegarde** : Toujours activer la sauvegarde pour les nettoyages importants
3. **Dry-Run** : Testez avec `--dry-run` avant un nettoyage réel
4. **Planificateur** : Les tâches planifiées s'exécutent même si l'application est fermée
5. **Statistiques** : Conservées indéfiniment, nettoyage manuel possible

---

## 🐛 Dépannage

### "Droits insuffisants"
→ Lancez en tant qu'administrateur

### "Profil introuvable"
→ Utilisez `--list-profiles` pour voir les profils disponibles

### "Erreur accès fichier"
→ Fermez les navigateurs et applications avant nettoyage

### "Tâche planifiée échoue"
→ Vérifiez que le chemin vers l'exécutable est correct dans le planificateur

---

## 📞 Support

Pour toute question ou problème :
- Consultez les logs dans `%AppData%\WindowsCleaner\logs`
- Vérifiez les statistiques avec `--stats`
- Utilisez le mode `--dry-run` pour tester

**Version** : 1.0.6  
**Date** : Décembre 2025  
**Licence** : MIT
