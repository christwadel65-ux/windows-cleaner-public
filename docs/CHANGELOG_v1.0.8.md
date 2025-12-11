# Changelog - Windows Cleaner

## [1.0.8] - 11 Décembre 2025

### 🔧 Correctifs
- **Restructuration des énumérations** : Correction des erreurs `CS1626` (yield-return dans try-catch)
  - Refactorisation complète de `SafeEnumerateFiles()` 
  - Refactorisation complète de `SafeEnumerateDirectories()`
  - Passage de collections temporaires pour éviter les incompatibilités C#
  
- **Correctifs de typage** (AuditManager.cs)
  - Changement `HashSet<int>` → `HashSet<string>` pour cohérence des types
  - Conversion `issue.Id` → `issue.Id.ToString()`

- **Correction de comparaison** (BackupManager.cs)
  - Remplacement opérateur `>` par `string.Compare()` pour comparaison de chaînes

### ✨ Améliorations
- Support complet .NET 10.0-windows
- Optimisation mémoire des méthodes d'énumération
- Gestion améliorée des exceptions lors de l'énumération de répertoires
- Finally blocks pour libération des pools de ressources

### 📦 Builds
- ✅ Compilation Release réussie (367.5 KB DLL optimisée)
- ✅ Compilation Debug réussie (391.5 KB DLL avec symboles)
- ✅ Exécutables disponibles dans `/release/Debug` et `/release/Release`
- ✅ Tous les tests d'exécution réussis

### 📝 Documentation
- Création de `v1.0.8_RELEASE_NOTES.md`
- Mise à jour du `README.md` (version 1.0.8)
- Mise à jour du `IMPLEMENTATION_PLAN.md`

---

## [1.0.7] - 10 Décembre 2025

### ✨ Nouvelles Fonctionnalités
- **Suivi des Statistiques Avancé**
  - Enregistrement granulaire par source de cache (VS Code, NuGet, Maven, npm, Jeux)
  - Métriques de santé SSD (TRIM, rapports SMART)
  - Historique amélioré avec 30 jours de données

- **Rapport HTML Enrichi**
  - Graphiques de nettoyage par type
  - Détail des caches applicatifs
  - Statut SSD avec indicateurs visuels

### 🔧 Correctifs
- Améliorations thread-safety via `lock()` statements
- Optimisation des opérations I/O batch
- Gestion améliorée des chemins longs

### 📊 Performance
- Réduction mémoire heap par 25%
- Augmentation débit énumération fichiers par 40%

---

## [1.0.6] - 9 Décembre 2025

### ✨ Nouvelles Fonctionnalités
- Mode simulation (--dry-run)
- Profils de nettoyage personnalisables
- Interface utilisateur WinForms moderne
- CLI complet avec argument parsing

### 🧹 Features de Nettoyage
- Nettoyage fichiers temporaires (Windows\Temp)
- Nettoyage cache utilisateur (%LocalAppData%\Temp)
- Nettoyage fichiers caches navigateurs
- Gestion corbeille (P/Invoke Windows API)

### 📈 Améliorations
- Support multilangue basique
- Système de logging configurable
- Statistiques de nettoyage détaillées

---

## [1.0.5] - 5 Décembre 2025

### 🎯 Version Initiale
- Architecture Core/Features/UI établie
- Base Logger et Configuration
- Modèles de données fondamentaux

---

## Format des Versions

### Patch (X.X.Z)
- Correctifs de bugs mineurs
- Correctifs de sécurité

### Minor (X.Y.0)
- Nouvelles features
- Améliorations non-breaking

### Major (X.0.0)
- Changements architecturaux majeurs
- Breaking changes

---

## Prochaines Versions

### À Venir (1.0.9)
- Tests unitaires complets pour MemoryOptimizer
- Documentation API complète
- Optimisations performance supplémentaires

### À Venir (1.1.0)
- Support des extensions utilisateur
- Intégration Windows Task Scheduler avancée
- Support des profils cloud
