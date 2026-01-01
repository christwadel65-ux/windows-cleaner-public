# Implémentation de la Détection et Nettoyage des Applications Fantômes (#20)

## Vue d'ensemble
J'ai implémenté la fonctionnalité #20 : **Nettoyage des applications fantômes** (dossiers orphelins et entrées registre invalides).

## Fichiers créés/modifiés

### 1. **Nouveau fichier : `GhostAppsCleaner.cs`**
**Chemin :** `src/WindowsCleaner/Core/GhostAppsCleaner.cs`

Une classe complète de nettoyage des applications fantômes avec :

#### Fonctionnalités principales :
- **`DetectGhostApps()`** : Détecte les applications fantômes en :
  - Analysant les dossiers Program Files (32 et 64 bits)
  - Scannant les entrées du registre (HKLM et HKCU)
  - Identifiant les dossiers orphelins (> 1 MB sans entrée registre)
  - Détectant les entrées registre invalides (pointant vers des dossiers inexistants)

- **`CleanGhostApps()`** : Nettoie les applications détectées :
  - Supprime les dossiers orphelins
  - Supprime les entrées registre invalides
  - Retourne les statistiques (fichiers supprimés, octets libérés, entrées registre supprimées)

#### Énumération `GhostAppType` :
- `OrphanedFolder` : Dossier existe mais pas d'entrée registre
- `OrphanedRegistry` : Entrée registre existe mais pas de dossier
- `InvalidRegistry` : Entrée registre invalide (chemin inexistant)

#### Classe `GhostApp` :
Représente une application fantôme détectée avec :
- `Name`, `Version`, `InstallLocation`
- `Type`, `RegistryPath`, `SizeBytes`
- `InstallDate`

### 2. **Modifié : `Core/Cleaner.cs`**

#### Ajouts :
- **Option :** `public bool CleanGhostApps { get; set; }` dans `CleanerOptions`
- **Statistiques :** 
  - `public int GhostAppsRemoved { get; set; }`
  - `public int InvalidRegistryEntriesRemoved { get; set; }`
  - dans `CleanerResult`

- **Logique de nettoyage** dans `RunCleanup()` :
  ```csharp
  if (options.CleanGhostApps)
  {
      tasks.Add(Task.Run(() =>
      {
          var ghostApps = GhostAppsCleaner.DetectGhostApps(...);
          var (deletedFiles, freedBytes, removedRegistry) = 
              GhostAppsCleaner.CleanGhostApps(...);
          // Mise à jour des statistiques
      }));
  }
  ```

### 3. **Modifié : `Features/CleaningProfile.cs`**

#### Ajouts :
- **Propriété :** `public bool CleanGhostApps { get; set; }`
- **Intégration au profil "Complet"** :
  ```csharp
  CleanGhostApps = true,
  ```
- **Conversion** dans `ToCleanerOptions()` :
  ```csharp
  CleanGhostApps = this.CleanGhostApps
  ```

### 4. **Modifié : `Features/LanguageManager.cs`**

#### Traductions ajoutées (FR/EN) :

**Interface :**
- `chk_ghost_apps` : "👻 Applications fantômes" / "👻 Ghost apps"

**Logs de détection :**
- `log_detecting_ghost_apps`
- `log_ghost_apps_found`
- `log_orphaned_folder_found`
- `log_invalid_registry_found`
- `log_removing_ghost_app`
- `log_removing_invalid_registry`

**Erreurs :**
- `error_detecting_ghost_apps`
- `error_removing_ghost_app`
- `error_removing_registry_entry`

### 5. **Modifié : `Features/Settings.cs`**

#### Ajout :
- **Propriété :** `public bool? CleanGhostApps { get; set; }`

Permet la sauvegarde/chargement des paramètres utilisateur.

### 6. **Modifié : `UI/MainForm.cs`**

#### Ajouts :

**Champ privé :**
```csharp
private CheckBox chkGhostApps = null!;
```

**Interface utilisateur :**
- Ajout d'une case à cocher "👻 Applications fantômes" dans le groupe "Advanced options"
- Position : `Left = 15, Top = 58` (2ème ligne du groupe)
- Ajustement de la hauteur du groupe Advanced pour accommoder 2 lignes

**Synchronisation des données :**
- `SaveCheckboxesToSettings()` : Sauvegarde l'état de la checkbox
- `RestoreCheckboxesFromSettings()` : Restaure l'état des paramètres
- `ApplyProfileToCheckboxes()` : Applique les profils
- `GetSelectedProfile()` : Retourne le profil sélectionné

## Fonctionnalités implémentées

### Détection
✅ Détecte les dossiers orphelins > 1 MB sans entrée registre
✅ Détecte les entrées registre HKLM et HKCU valides
✅ Détecte les entrées registre invalides
✅ Évite les faux positifs en utilisant le registre comme source fiable

### Nettoyage
✅ Suppression sécurisée des dossiers orphelins
✅ Suppression des entrées registre invalides
✅ Mode Dry-Run supporté
✅ Annulation supportée via CancellationToken

### Interface
✅ Case à cocher dans le groupe "Advanced options"
✅ Intégration dans les profils prédéfinis
✅ Sauvegarde/chargement des paramètres
✅ Messages de log bilingues (FR/EN)

### Robustesse
✅ Gestion d'erreurs complète
✅ Respect des permissions d'accès
✅ Pas d'exceptions non gérées
✅ Logging détaillé de toutes les opérations

## Statistiques et rapports

Le nettoyage génère des statistiques :
- **Nombre d'applications fantômes supprimées**
- **Nombre d'entrées registre invalides supprimées**
- **Espace libéré** (par défaut)
- **Fichiers supprimés** (par défaut)

## Compilation

✅ **Compilation réussie** avec 0 avertissements et 0 erreurs

## Prochaines étapes possibles

1. **Interface GUI avancée** : Afficher la liste des applications fantômes détectées
2. **Whitelisting** : Permettre à l'utilisateur d'exclure certaines applications
3. **Analyse prédictive** : Estimer les économies de disque avant nettoyage
4. **Historique** : Conserver un log des applications fantômes nettoyées

## Notes

- La détection est **très conservative** (> 1 MB minimum) pour éviter les faux positifs
- Les chemins inaccessibles sont silencieusement ignorés
- Le registre est scanné deux fois (HKLM et HKCU) pour une couverture complète
- Les performances sont optimisées avec les opérations parallèles
