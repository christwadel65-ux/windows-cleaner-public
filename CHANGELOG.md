# Changelog

Toutes les modifications notables de ce projet seront documentées dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Versionnage Sémantique](https://semver.org/lang/fr/).

# Changelog - Windows Cleaner

Toutes les modifications notables de ce projet seront documentées dans ce fichier.

## 🆕 Nouveautés v2.1.0 (03 janvier 2026)

### ⚙️ Nouveau Menu : Configuration Système (type msconfig)
- **Nouveau menu "Configuration Système"** : accès complet à la gestion du système Windows
- **Onglet Programmes de démarrage** :
  - Liste tous les programmes qui se lancent au démarrage (HKCU, HKLM, dossier Démarrage)
  - Désactivation/activation des programmes de démarrage
  - Affichage de l'emplacement et de la commande d'exécution
- **Onglet Services Windows** :
  - Liste complète de tous les services Windows avec statut et type de démarrage
  - Démarrage/Arrêt des services
  - Modification du type de démarrage (auto, manuel, désactivé)
  - Affichage des descriptions des services
- **Onglet Tâches planifiées** :
  - Liste de toutes les tâches planifiées du système
  - Activation/Désactivation des tâches
  - Affichage du statut, prochaine et dernière exécution
- **Onglet Outils système** :
  - Accès rapide à msconfig
  - Accès rapide au Gestionnaire de tâches
  - Accès rapide au Gestionnaire de services
- **Classe SystemConfigManager** : gestion complète de la configuration système
- **Interface SystemConfigForm** : formulaire avec onglets pour une navigation intuitive
- **Traductions FR/EN complètes** : 60+ nouvelles clés de traduction
- **Avertissement administrateur** : notification si l'application ne s'exécute pas en tant qu'admin
- **Chargement asynchrone** : les services et tâches se chargent en arrière-plan pour ne pas bloquer l'interface
- **Sécurité** : confirmation avant arrêt de services ou désactivation d'éléments
- **Intégration totale** : nouveau menu entre "Désinstallateur" et "Aide"

## 🆕 Nouveautés v2.0.3 (01 janvier 2026)

### 👻 Détection et Nettoyage des Applications Fantômes
- **Classe GhostAppsCleaner** : détecte les applications non complètement désinstallées
- **3 types d'applications fantômes** :
  - Dossiers orphelins (> 1 MB sans entrée registre)
  - Entrées registre invalides (pointant vers des répertoires inexistants)
  - Dossiers sans entrée registre correspondante
- **Mode Dry-Run supporté** : prévisualiser les suppression sans action
- **Traductions FR/EN** : messages de log complètement localisés
- **Intégration totale** : case à cocher dans groupe "Advanced options", profils, sauvegarde paramètres
- **Performance optimisée** : détection parallèle, filtrage intelligent (> 1 MB minimum)
- **Sécurité renforcée** : gestion complète des erreurs, respect des permissions

### 🎨 Interface Utilisateur
- **Restructuration groupe Advanced** : 2 lignes pour meilleure lisibilité (3 éléments par ligne)
- **Case "👻 Applications fantômes"** avec largeur appropriée (380px)
- **Alignement parfait** sans coupure de texte

## 🆕 Nouveautés v2.0.2 (18 décembre 2025)

### 🔒 Amélioration Vie Privée : Historique Navigateurs et Onglets Récents
- **Case à cocher dédiée "🕘 Historique navigateurs"** dans le bloc Nettoyage Standard (cochée par défaut)
- **Nettoyage complet de l'historique** : Chrome/Edge/Firefox (fichiers History, places.sqlite)
- **Suppression des onglets récents/sessions** :
  - Chrome/Edge : dossiers `Sessions` + fichiers `Last Session`, `Last Tabs`, `Current Session`, `Current Tabs`
  - Firefox : dossier `sessionstore-backups` + fichier `sessionstore.jsonlz4`
- **Fermeture automatique des navigateurs** avant nettoyage pour éviter les verrous
- **Option indépendante** : choix de nettoyer cache navigateurs sans historique (ou vice-versa)
- **Intégration totale** : profils prédéfinis, boutons Tout/Rien, sauvegarde des paramètres
- **Statistiques détaillées** : comptabilise fichiers supprimés et octets libérés

### 🛠️ Gestion de Version Centralisée
- **Classe AppVersion** : lecture automatique de la version depuis l'assembly
- **Script update_version.ps1** : mise à jour automatique de tous les fichiers (csproj, iss, README)
- **Documentation complète** : [VERSION_MANAGEMENT.md](docs/VERSION_MANAGEMENT.md)
- Plus besoin de modifier manuellement le code source pour changer la version

## [2.0.0] - 2025-12-15

### 🚀 Version Majeure - Refonte Complète

#### ✨ Nouvelles Fonctionnalités Majeures

**🔄 Système de Mise à Jour Automatique**
- Vérification automatique au démarrage de l'application
- Menu "Aide > 🔄 Vérifier les mises à jour" pour vérification manuelle
- Notification discrète dans la barre de statut si mise à jour disponible
- Dialogue informatif avec version, date et notes de version
- Ouverture directe de la page GitHub Release
- API GitHub pour récupération automatique des dernières releases
- Comparaison intelligente des versions (format sémantique X.Y.Z)
- Pas de téléchargement automatique - contrôle total de l'utilisateur

**💻 Interface de Nettoyage Développeur**
- Nouveau groupe "💻 Nettoyage Développeur" dans l'interface utilisateur
- 10 nouvelles options de nettoyage spécifiques aux développeurs :
  - 📦 VS Code - Nettoie le cache VS Code
  - 📦 NuGet - Nettoie le cache NuGet
  - 📦 Maven - Nettoie le repository Maven
  - 📦 npm - Nettoie le cache npm global
  - 🐳 Docker - Nettoie images et conteneurs
  - 📁 node_modules - Supprime les vieux dossiers node_modules
  - 🔨 Visual Studio - Nettoie obj/bin/.vs
  - 🐍 Python - Supprime __pycache__/*.pyc
  - 📂 Git - Optimise les repos Git
  - 🎮 Jeux (Steam/Epic) - Nettoie les caches de jeux
- Options intégrées dans les profils de nettoyage
- Boutons "✅ Tout" et "❌ Rien" incluent maintenant ces options
- Statistiques détaillées par type de cache

**📊 Statistiques SSD Améliorées**
- Sauvegarde automatique des sessions d'optimisation SSD
- Compteurs TRIM et vérifications SMART fonctionnels
- Détection multi-niveaux des disques (Win32_DiskDrive + Get-Volume)
- Rapport SMART détaillé avec informations sur les volumes
- Affichage dans les rapports HTML des statistiques

#### 🔧 Améliorations Techniques

**Interface Utilisateur**
- Hauteur de fenêtre ajustée : 850px pour accommoder le nouveau groupe développeur
- Layout optimisé pour 20+ options de nettoyage
- Groupe développeur positionné entre options avancées et logs
- Journal des opérations redimensionné intelligemment

**Profils de Nettoyage**
- Profil "Développeur" enrichi avec toutes les options de cache
- Profil "Complet" inclut maintenant l'optimisation SSD
- Mapping complet des nouvelles options dans CleaningProfile
- ToCleanerOptions() mis à jour pour toutes les options

**Détection des Disques**
- Méthode robuste avec fallback automatique
- Support des systèmes sans droits admin complets
- Informations détaillées : modèle, statut, interface, taille, partitions
- Rapport formaté avec sections distinctes (disques physiques + volumes)

#### 📝 Documentation

Nouveaux fichiers de documentation :
- `docs/UPDATE_GUIDE.md` - Guide complet de mise à jour
- `docs/RELEASE_GUIDE.md` - Guide de publication des releases
- `scripts/prepare_release.ps1` - Script d'automatisation des releases

#### 🐛 Corrections

- Fix : StatisticsManager.RecordCleaningSession() au lieu de SaveStatistics()
- Fix : Détection SMART retournant "Aucun disque détecté"
- Fix : Boutons Tout/Rien n'incluaient pas les options développeur
- Fix : Profils ne sauvegardaient pas les options de cache applicatifs

#### ⚙️ Modifications Breaking

- **Version majeure 2.0.0** en raison de l'ajout de nombreuses fonctionnalités
- Interface utilisateur élargie - nécessite résolution minimale 1220x850
- Nouveaux champs dans CleaningStatistics (non rétrocompatible avec anciennes stats)

### 📦 Fichiers Modifiés/Ajoutés

**Nouveaux Fichiers :**
- `src/WindowsCleaner/Features/UpdateManager.cs` (310 lignes)
- `docs/UPDATE_GUIDE.md`
- `docs/RELEASE_GUIDE.md`
- `scripts/prepare_release.ps1`

**Fichiers Modifiés :**
- `src/WindowsCleaner/UI/MainForm.cs` - Interface développeur + menu mise à jour
- `src/WindowsCleaner/Features/CleaningProfile.cs` - Nouvelles propriétés
- `src/WindowsCleaner/Core/SystemOptimizer.cs` - Détection SMART améliorée
- `src/WindowsCleaner/Features/StatisticsManager.cs` - Statistiques développeur
- `WindowsCleaner.csproj`, `app.manifest`, `windows-cleaner.iss` - Version 2.0.0

---

## [1.0.9] - 2025-12-15 (Non publié)

*Version intermédiaire de développement fusionnée dans 2.0.0*

---

## [1.0.8] - 2025-12-11

### ✨ Ajouté
- **Suppression des raccourcis cassés**
  - Détection automatique des raccourcis (.lnk) dont la cible n'existe plus
  - Scan intelligent : Bureau, Menu Démarrer, Documents Récents, Dossier Liens
  - Vérification via COM (WScript.Shell) pour validation précise
  - Suppression sécurisée avec support du mode Dry-Run
  - Aucun privilège administrateur requis

- **Boutons de sélection rapide**
  - Bouton "✅ Tout" : Coche toutes les options de nettoyage en un clic
  - Bouton "❌ Rien" : Décoche toutes les options rapidement
  - Couleurs intelligentes (VERT si tout coché, ROUGE si rien, ORANGE si partiel)
  - Infobulles explicites sur tous les boutons
  - Bascule automatique vers le profil "Personnalisé (manuel)"

### 🎨 Amélioré
- Migration complète vers .NET 10.0-windows
- Système d'infobulles contextuelles sur tous les boutons d'action
- Design Material moderne avec palette de couleurs vives
- Feedback visuel immédiat sur l'état de sélection

### 🔧 Corrigé
- **Erreurs de compilation CS1626** : Restructuration complète des méthodes d'énumération
  - Refactorisation de `SafeEnumerateFiles()` et `SafeEnumerateDirectories()`
  - Passage de collections temporaires pour éviter les incompatibilités C#
  
- **Correctifs de typage** (AuditManager.cs)
  - Changement `HashSet<int>` → `HashSet<string>` pour cohérence des types
  - Conversion `issue.Id` → `issue.Id.ToString()`

- **Correction de comparaison** (BackupManager.cs)
  - Remplacement opérateur `>` par `string.Compare()` pour comparaison de chaînes

### 📦 Build
- Compilation Release réussie (367.5 KB DLL optimisée)
- Compilation Debug réussie (391.5 KB DLL avec symboles)
- Exécutables disponibles dans `/release/Debug` et `/release/Release`
- Tous les tests d'exécution réussis

## [1.0.7] - 2025-12-10

### ✨ Ajouté
- **Suivi des statistiques avancé**
  - Enregistrement granulaire par source de cache (VS Code, NuGet, Maven, npm, Jeux)
  - Métriques de santé SSD (TRIM, rapports SMART)
  - Historique amélioré avec 30 jours de données

- **Rapport HTML enrichi**
  - Graphiques de nettoyage par type
  - Détail des caches applicatifs
  - Statut SSD avec indicateurs visuels

### 🎨 Amélioré
- Optimisation mémoire : Réduction heap par 25%
- Performance : Augmentation débit énumération fichiers par 40%
- Thread-safety via `lock()` statements
- Optimisation des opérations I/O batch
- Gestion améliorée des chemins longs

## [1.0.6] - 2025-12-08

### 🎉 Fonctionnalités Majeures

#### 1. Système de profils de nettoyage
- 4 profils prédéfinis inclus :
  - Nettoyage Rapide (usage quotidien)
  - Nettoyage Complet (maintenance approfondie)
  - Nettoyage Développeur (spécialisé dev)
  - Protection Vie Privée (effacement traces)
- Création de profils personnalisés avec nom et description
- Import/Export JSON pour partage et sauvegarde
- Gestion complète (création, modification, suppression)
- Sauvegarde automatique des préférences utilisateur

#### 2. Analyseur d'espace disque
- Top fichiers volumineux (configurable 20/50/100)
- Catégorisation automatique par type (15+ catégories)
- Calcul des pourcentages d'utilisation
- Identification dossiers volumineux (Top 20)
- Mode progressif avec support annulation
- Statistiques détaillées (temps scan, total fichiers, taille)

#### 3. Planificateur de tâches Windows
- Tâches quotidiennes à heure programmée
- Tâches hebdomadaires (choix jour de semaine)
- Tâches mensuelles (choix jour du mois)
- Intégration Windows Task Scheduler native
- Gestion complète CRUD (Create, Read, Update, Delete)
- Activation/Désactivation dynamique

#### 4. Détecteur de fichiers dupliqués
- Hash MD5 rapide et fiable
- Filtrage par taille minimale (configurable)
- Filtrage par extensions (jpg, png, mp4, etc.)
- Calcul espace récupérable automatique
- Groupement intelligent des doublons
- Suppression sécurisée vers corbeille
- Statistiques complètes (temps, fichiers, espace)

#### 5. Système de sauvegarde et restauration
- Point de restauration système Windows
- Sauvegarde fichiers avant suppression
- Compression ZIP des dossiers
- Restauration complète en un clic
- Historique des sauvegardes avec métadonnées
- Nettoyage automatique sauvegardes > 24h
- Liste et gestion des sauvegardes disponibles

#### 6. Statistiques et rapports
- Enregistrement automatique chaque session
- Statistiques globales (tout historique)
- Statistiques 30 jours (récent)
- Export HTML professionnel avec CSS moderne
- Graphiques et tableaux détaillés
- Calculs automatiques (total espace, fichiers, sessions)
- Top 50 sessions dans le rapport

#### 7. Support ligne de commande (CLI)
- Arguments complets pour automatisation :
  - `--profile <nom>` : Utiliser un profil
  - `--dry-run` : Mode simulation
  - `--silent` : Mode silencieux
  - `--list-profiles` : Liste des profils
  - `--stats` : Afficher statistiques
  - `--help` : Aide complète
- Codes de retour appropriés (0 = succès, 1 = erreur)
- Messages formatés pour scripts
- Compatibilité PowerShell/Batch

#### 8. Alertes intelligentes
- Vérification espace disque automatique (alerte si < 10% ou < 10 GB)
- Alerte cache navigateurs si > 2 GB
- Rappel nettoyage régulier si > 7 jours
- Recommandations personnalisées
- Notifications non-intrusives

### ✨ Ajouté
- Mode simulation (--dry-run)
- Interface utilisateur WinForms moderne
- Support multilangue basique
- Système de logging configurable

### 🧹 Nettoyage
- Nettoyage fichiers temporaires (Windows\Temp)
- Nettoyage cache utilisateur (%LocalAppData%\Temp)
- Nettoyage fichiers caches navigateurs
- Gestion corbeille (P/Invoke Windows API)

### 📝 Fichiers Ajoutés
- `src/WindowsCleaner/Features/CleaningProfile.cs` (250 lignes)
- `src/WindowsCleaner/Features/DiskAnalyzer.cs` (320 lignes)
- `src/WindowsCleaner/Features/TaskSchedulerManager.cs` (400 lignes)
- `src/WindowsCleaner/Features/DuplicateFinder.cs` (280 lignes)
- `src/WindowsCleaner/Core/BackupManager.cs` (270 lignes)
- `src/WindowsCleaner/Features/StatisticsManager.cs` (380 lignes)
- `src/WindowsCleaner/Features/SmartAlerts.cs`

## [1.0.5] - 2025-12-05

### ✨ Ajouté
- Architecture Core/Features/UI établie
- Base Logger et Configuration
- Modèles de données fondamentaux
- Version initiale fonctionnelle

## Convention de Versionnement

### Patch (X.X.Z)
- Correctifs de bugs mineurs
- Correctifs de sécurité
- Mises à jour de documentation

### Minor (X.Y.0)
- Nouvelles fonctionnalités
- Améliorations non-breaking
- Optimisations de performance

### Major (X.0.0)
- Changements breaking
- Refonte majeure de l'architecture
- Nouvelles capacités transformatrices

---

[Non publié]: https://github.com/votre-repo/windows-cleaner/compare/v1.0.8...HEAD
[1.0.8]: https://github.com/votre-repo/windows-cleaner/compare/v1.0.7...v1.0.8
[1.0.7]: https://github.com/votre-repo/windows-cleaner/compare/v1.0.6...v1.0.7
[1.0.6]: https://github.com/votre-repo/windows-cleaner/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/votre-repo/windows-cleaner/releases/tag/v1.0.5
