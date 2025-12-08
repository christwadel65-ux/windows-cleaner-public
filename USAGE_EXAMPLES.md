# 📘 Guide d'Exemples Pratiques - Windows Cleaner v1.0.6

## 🎯 Scénarios d'Utilisation Courants

### Scénario 1 : Nettoyage Rapide Quotidien (CLI)

Créez un fichier `nettoyage-quotidien.bat` :

```batch
@echo off
echo ========================================
echo  Nettoyage Quotidien Windows Cleaner
echo ========================================
echo.

"C:\Program Files\WindowsCleaner\windows-cleaner.exe" --profile "Nettoyage Rapide" --silent

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [OK] Nettoyage termine avec succes
    exit /b 0
) else (
    echo.
    echo [ERREUR] Echec du nettoyage
    exit /b 1
)
```

**Ajoutez au Planificateur de Tâches Windows** pour exécution quotidienne à 2h du matin.

---

### Scénario 2 : Analyse Complète de Disque avec Rapport

```csharp
using System;
using System.Threading.Tasks;
using WindowsCleaner;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("🔍 Analyse du disque en cours...\n");
        
        // Analyser le disque C:
        var result = await DiskAnalyzer.AnalyzeDirectory(
            @"C:\Users\VotreNom",
            topFileCount: 100,
            progress: msg => Console.WriteLine($"  {msg}")
        );
        
        Console.WriteLine("\n📊 RÉSULTATS DE L'ANALYSE");
        Console.WriteLine($"{"=",-60}");
        Console.WriteLine($"Fichiers scannés: {result.TotalScannedFiles:N0}");
        Console.WriteLine($"Taille totale: {FormatBytes(result.TotalScannedSize)}");
        Console.WriteLine($"Durée: {result.ScanDuration.TotalSeconds:F1}s\n");
        
        // Top 10 des plus gros fichiers
        Console.WriteLine("📁 TOP 10 FICHIERS VOLUMINEUX:");
        Console.WriteLine($"{"=",-60}");
        foreach (var file in result.LargestFiles.Take(10))
        {
            Console.WriteLine($"{file.FormattedSize,10} - {file.Path}");
        }
        
        // Catégories
        Console.WriteLine("\n📂 UTILISATION PAR CATÉGORIE:");
        Console.WriteLine($"{"=",-60}");
        foreach (var cat in result.Categories.Take(10))
        {
            var bar = new string('█', (int)(cat.Percentage / 2));
            Console.WriteLine($"{cat.Name,-20} {cat.FormattedSize,10} ({cat.Percentage:F1}%) {bar}");
        }
    }
    
    static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
```

---

### Scénario 3 : Détection et Nettoyage des Doublons Photos

```csharp
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowsCleaner;

class DuplicatePhotosCleaner
{
    static async Task Main()
    {
        var photosPath = @"C:\Users\VotreNom\Photos";
        
        Console.WriteLine("🔍 Recherche de photos en double...\n");
        
        var result = await DuplicateFinder.FindDuplicates(
            photosPath,
            minFileSize: 50 * 1024, // 50 KB minimum
            extensions: new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" },
            progress: Console.WriteLine
        );
        
        Console.WriteLine($"\n✅ Recherche terminée en {result.SearchDuration.TotalSeconds:F1}s");
        Console.WriteLine($"   {result.TotalDuplicates} doublons trouvés");
        Console.WriteLine($"   {FormatBytes(result.TotalWastedSpace)} récupérables\n");
        
        // Afficher les groupes de doublons
        int groupNum = 1;
        foreach (var group in result.DuplicateGroups.Take(10))
        {
            Console.WriteLine($"Groupe #{groupNum} ({group.Files.Count} fichiers):");
            foreach (var file in group.Files)
            {
                Console.WriteLine($"  - {file.Path}");
            }
            Console.WriteLine($"  Espace gaspillé: {group.FormattedWastedSpace}\n");
            groupNum++;
        }
        
        // Demander confirmation
        Console.Write("Supprimer les doublons (garder le plus ancien) ? (O/N): ");
        if (Console.ReadLine()?.ToUpper() == "O")
        {
            int deleted = 0;
            foreach (var group in result.DuplicateGroups)
            {
                // Garder le premier (plus ancien), supprimer les autres
                var toDelete = group.Files
                    .OrderBy(f => f.LastModified)
                    .Skip(1)
                    .ToList();
                
                deleted += DuplicateFinder.DeleteDuplicates(toDelete, moveToRecycleBin: true);
            }
            
            Console.WriteLine($"\n✅ {deleted} fichiers déplacés vers la corbeille");
        }
    }
    
    static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
```

---

### Scénario 4 : Nettoyage Développeur Automatisé

Script PowerShell `clean-dev-environment.ps1` :

```powershell
# Nettoyage automatique pour développeurs
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host " Nettoyage Environnement Développeur" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Exécuter avec le profil Développeur
$cleanerPath = "C:\Program Files\WindowsCleaner\windows-cleaner.exe"

Write-Host "🧹 Nettoyage en cours..." -ForegroundColor Yellow
& $cleanerPath --profile "Nettoyage Développeur" --silent

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Nettoyage réussi" -ForegroundColor Green
    
    # Afficher les statistiques
    Write-Host ""
    Write-Host "📊 Statistiques:" -ForegroundColor Cyan
    & $cleanerPath --stats
    
} else {
    Write-Host "❌ Erreur lors du nettoyage" -ForegroundColor Red
    exit 1
}

# Nettoyages additionnels spécifiques
Write-Host ""
Write-Host "🔧 Nettoyages additionnels..." -ForegroundColor Yellow

# Nettoyer npm cache
Write-Host "  → npm cache clean" -ForegroundColor Gray
npm cache clean --force 2>$null

# Nettoyer yarn cache
Write-Host "  → yarn cache clean" -ForegroundColor Gray
yarn cache clean 2>$null

# Nettoyer pip cache
Write-Host "  → pip cache purge" -ForegroundColor Gray
pip cache purge 2>$null

Write-Host ""
Write-Host "✅ Nettoyage complet terminé!" -ForegroundColor Green
```

---

### Scénario 5 : Création de Profil Personnalisé pour Gamer

```csharp
using WindowsCleaner;

class GamerProfileCreator
{
    static void Main()
    {
        // Créer un profil optimisé pour les gamers
        var gamerProfile = new CleaningProfile
        {
            Name = "Optimisation Gaming",
            Description = "Nettoie et optimise pour les performances gaming",
            
            // Nettoyages standards
            EmptyRecycleBin = true,
            IncludeSystemTemp = true,
            CleanBrowsers = true,
            CleanThumbnails = true,
            CleanPrefetch = false, // Important pour les jeux
            
            // Nettoyages avancés
            CleanOrphanedFiles = true,
            ClearMemoryCache = true,
            
            // Pas de nettoyage dev (pas utile pour gamers)
            CleanDocker = false,
            CleanNodeModules = false,
            CleanVisualStudio = false,
            
            // Options
            Verbose = false,
            CreateBackup = false // Plus rapide sans backup
        };
        
        // Sauvegarder
        ProfileManager.SaveProfile(gamerProfile);
        
        Console.WriteLine("✅ Profil 'Optimisation Gaming' créé!");
        Console.WriteLine("\nUtilisation:");
        Console.WriteLine("  windows-cleaner.exe --profile \"Optimisation Gaming\"");
    }
}
```

---

### Scénario 6 : Planification Intelligente avec Vérifications

```csharp
using System;
using WindowsCleaner;

class SmartScheduler
{
    static void Main()
    {
        Console.WriteLine("🔔 Vérification de l'état du système...\n");
        
        // Vérifier si un nettoyage est nécessaire
        var recommendations = SmartAlerts.GenerateRecommendations();
        Console.WriteLine(recommendations);
        
        // Vérifier spécifiquement l'espace disque
        var (diskAlert, diskMsg) = SmartAlerts.CheckDiskSpace();
        
        if (diskAlert)
        {
            Console.WriteLine("\n⚠️  ALERTE: Espace disque faible!");
            Console.WriteLine(diskMsg);
            
            // Proposer une planification
            Console.Write("\nPlanifier un nettoyage quotidien à 2h du matin ? (O/N): ");
            if (Console.ReadLine()?.ToUpper() == "O")
            {
                var profile = CleaningProfile.CreateCompleteProfile();
                var success = TaskSchedulerManager.CreateDailyTask(
                    "NettoyageAutomatique",
                    profile,
                    new TimeSpan(2, 0, 0)
                );
                
                if (success)
                {
                    Console.WriteLine("✅ Tâche planifiée créée avec succès!");
                }
                else
                {
                    Console.WriteLine("❌ Erreur: droits administrateur requis");
                }
            }
        }
        else
        {
            Console.WriteLine("\n✅ Votre système est en bon état.");
            Console.WriteLine("   Aucune action immédiate requise.");
        }
    }
}
```

---

### Scénario 7 : Rapport HTML Automatique par Email

Script PowerShell `send-cleaning-report.ps1` :

```powershell
param(
    [string]$ToEmail = "admin@example.com",
    [string]$FromEmail = "cleaner@example.com",
    [string]$SmtpServer = "smtp.example.com"
)

Write-Host "📊 Génération du rapport de nettoyage..." -ForegroundColor Cyan

# Exécuter le nettoyage
$cleanerPath = "C:\Program Files\WindowsCleaner\windows-cleaner.exe"
& $cleanerPath --profile "Nettoyage Complet" --silent

# Générer le rapport HTML
$reportPath = [System.IO.Path]::Combine($env:TEMP, "cleaning-report.html")

# Utiliser l'API pour générer le rapport
Add-Type -AssemblyName "System.Windows.Forms"
$code = @"
using System;
using WindowsCleaner;

public class ReportGenerator
{
    public static void Generate(string path)
    {
        var html = StatisticsManager.GenerateHtmlReport();
        System.IO.File.WriteAllText(path, html);
    }
}
"@

# Compiler et exécuter
Add-Type -TypeDefinition $code -ReferencedAssemblies @(
    "System.dll",
    "C:\Program Files\WindowsCleaner\windows-cleaner.dll"
)

[ReportGenerator]::Generate($reportPath)

# Envoyer par email
Write-Host "📧 Envoi du rapport par email..." -ForegroundColor Yellow

$subject = "Rapport de Nettoyage Windows - $(Get-Date -Format 'dd/MM/yyyy')"
$body = Get-Content $reportPath -Raw

Send-MailMessage `
    -To $ToEmail `
    -From $FromEmail `
    -Subject $subject `
    -Body $body `
    -BodyAsHtml `
    -SmtpServer $SmtpServer `
    -Priority High

Write-Host "✅ Rapport envoyé avec succès!" -ForegroundColor Green

# Nettoyer
Remove-Item $reportPath -Force
```

---

### Scénario 8 : Intégration CI/CD (Build Server Cleanup)

Pour nettoyer régulièrement un serveur de build :

`.gitlab-ci.yml` ou Azure Pipeline :

```yaml
cleanup_job:
  stage: cleanup
  schedule:
    - cron: "0 2 * * *"  # Tous les jours à 2h
  script:
    - |
      # Créer un profil pour serveur de build
      windows-cleaner.exe --profile "Nettoyage Développeur" --silent
      
      # Vérifier le résultat
      if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Nettoyage du serveur de build réussi"
      } else {
        Write-Host "❌ Erreur nettoyage"
        exit 1
      }
      
      # Afficher les statistiques
      windows-cleaner.exe --stats
  only:
    - schedules
```

---

### Scénario 9 : Monitoring et Alertes Proactives

Script de monitoring `monitor-system.ps1` :

```powershell
# À exécuter régulièrement (ex: toutes les heures)
$cleanerPath = "C:\Program Files\WindowsCleaner\windows-cleaner.exe"

# Vérifier l'espace disque
$drives = Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Used -gt 0 }

foreach ($drive in $drives) {
    $freePercent = ($drive.Free / ($drive.Used + $drive.Free)) * 100
    
    if ($freePercent -lt 10) {
        Write-Warning "⚠️  Espace faible sur $($drive.Name): $([math]::Round($freePercent, 1))%"
        
        # Envoyer une notification
        $message = "Espace disque faible sur $($drive.Name). Lancer un nettoyage ?"
        
        # Notification Windows
        Add-Type -AssemblyName System.Windows.Forms
        $notify = New-Object System.Windows.Forms.NotifyIcon
        $notify.Icon = [System.Drawing.SystemIcons]::Warning
        $notify.Visible = $true
        $notify.ShowBalloonTip(5000, "Windows Cleaner", $message, [System.Windows.Forms.ToolTipIcon]::Warning)
        
        # Log
        Add-Content -Path "C:\Logs\disk-monitor.log" -Value "$(Get-Date) - ALERTE: $message"
    }
}

# Vérifier le dernier nettoyage
$lastClean = & $cleanerPath --stats | Select-String "Dernières sessions" -Context 0,1

# Si pas de nettoyage depuis 7 jours, envoyer alerte
# ... logique d'alerte
```

---

## 📝 Conseils d'Utilisation

### Pour les Particuliers
- Utilisez **"Nettoyage Rapide"** hebdomadairement
- Planifiez un **"Nettoyage Complet"** mensuel
- Activez les **alertes intelligentes**

### Pour les Développeurs
- Utilisez **"Nettoyage Développeur"** après chaque projet
- Automatisez avec **Git hooks** ou **scripts build**
- Analysez votre disque régulièrement

### Pour les Entreprises
- Déployez via **GPO** ou **Intune**
- Utilisez le **mode CLI silencieux**
- Centralisez les **rapports HTML**
- Planifiez sur tous les postes

---

## 🔗 Ressources

- Documentation complète: `NEW_FEATURES_v1.0.6.md`
- Résumé d'implémentation: `IMPLEMENTATION_SUMMARY.md`
- Code source: Tous les fichiers `.cs`

**Version**: 1.0.6  
**Date**: Décembre 2025
