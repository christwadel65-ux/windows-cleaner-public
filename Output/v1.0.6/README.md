# Windows Cleaner v1.0.6 - Package de Distribution

## 📦 Contenu du Package

- `windows-cleaner.exe` - Exécutable principal (v1.0.6)
- `windows-cleaner.dll` - Bibliothèque de l'application
- `windows-cleaner.deps.json` - Dépendances .NET
- `windows-cleaner.runtimeconfig.json` - Configuration runtime
- `run.bat` - Script de lancement rapide
- `README.md` - Ce fichier
- `PACKAGE_INFO.md` - Informations détaillées du package

## 🚀 Installation

### Méthode 1 : Exécution Directe
```batch
windows-cleaner.exe
```

### Méthode 2 : Script de Lancement
```batch
run.bat
```

## ⚙️ Configuration Requise

- **OS** : Windows 10 (1809+) ou Windows 11
- **.NET** : Runtime .NET 10.0 (inclus avec Windows Update)
- **Droits** : Administrateur recommandé pour fonctionnalités complètes

## 📋 Modes d'Utilisation

### Interface Graphique (GUI)
Double-cliquez sur `windows-cleaner.exe`

### Ligne de Commande (CLI)

#### Aide
```batch
windows-cleaner.exe --help
```

#### Nettoyage avec Profil
```batch
windows-cleaner.exe --profile "Nettoyage Rapide"
```

#### Mode Simulation (Test sans suppression)
```batch
windows-cleaner.exe --profile "Nettoyage Complet" --dry-run
```

#### Mode Silencieux (Pour scripts)
```batch
windows-cleaner.exe --profile "Protection Vie Privée" --silent
```

#### Lister les Profils Disponibles
```batch
windows-cleaner.exe --list-profiles
```

#### Afficher les Statistiques
```batch
windows-cleaner.exe --stats
```

## 🎯 Profils Prédéfinis

### 🚀 Nettoyage Rapide
Usage quotidien - Rapide et sûr
- Corbeille
- Caches navigateurs
- Vignettes
- Fichiers orphelins

### 🔧 Nettoyage Complet
Maintenance approfondie - Mensuel recommandé
- Toutes options standard
- Windows Update cache
- Prefetch
- Options avancées
- Avec sauvegarde

### 💻 Nettoyage Développeur
Spécialisé développeurs
- node_modules
- Python cache
- Visual Studio cache
- Git optimization
- Docker cleanup

### 🔒 Protection Vie Privée
Effacement des traces
- Historique Exécuter
- Documents récents
- Timeline Windows
- Historique recherche
- Presse-papiers

## 📊 Nouvelles Fonctionnalités v1.0.6

✅ **Système de Profils** avec 4 profils prédéfinis  
✅ **Analyse d'Espace Disque** complète  
✅ **Détecteur de Doublons** par hash MD5  
✅ **Planificateur de Tâches** Windows intégré  
✅ **Statistiques et Rapports** HTML  
✅ **Sauvegarde et Restauration** système  
✅ **Support CLI** complet  
✅ **Alertes Intelligentes** proactives  
✅ **Nettoyage Étendu** : Docker, Node, Python, Git, VS  
✅ **Nettoyage Vie Privée** : historique, timeline  
✅ **Optimisations Système** : TRIM SSD, registre, mémoire  

## 🛡️ Sécurité

- ✅ Mode **dry-run** pour tests
- ✅ **Point de restauration** système avant nettoyage
- ✅ **Sauvegarde fichiers** avec restauration < 24h
- ✅ **Logs détaillés** dans `%AppData%\WindowsCleaner\logs`

## ⚠️ Important

### Droits Administrateur
Pour fonctionnalités complètes, lancez en tant qu'administrateur :
- Clic droit → "Exécuter en tant qu'administrateur"

### Avant Premier Nettoyage
1. ✅ Fermez tous les navigateurs
2. ✅ Sauvegardez vos données importantes
3. ✅ Testez avec `--dry-run` d'abord
4. ✅ Consultez les logs si problème

## 📁 Fichiers de Configuration

Après première exécution, les fichiers sont créés dans :
```
%AppData%\WindowsCleaner\
├── Profiles\          # Profils personnalisés
├── Statistics\        # Historique nettoyages
├── Backups\          # Sauvegardes temporaires (< 24h)
└── logs\             # Logs détaillés
```

## 🔄 Mise à Jour

Pour mettre à jour depuis v1.0.5 :
1. Remplacez les fichiers dans le dossier d'installation
2. Vos paramètres et profils sont préservés
3. Les statistiques sont conservées

## 📞 Support

### Problème ?
1. Consultez les logs : `%AppData%\WindowsCleaner\logs\cleaner.log`
2. Testez en mode dry-run : `--dry-run`
3. Vérifiez les statistiques : `--stats`

### Documentation Complète
Consultez les fichiers dans le dossier source :
- `NEW_FEATURES_v1.0.6.md` - Guide complet
- `USAGE_EXAMPLES.md` - Exemples pratiques
- `CHANGELOG.md` - Historique versions

## 📜 Licence

MIT License - Utilisation libre pour usage personnel !

---

**Windows Cleaner v1.0.6**  
*Nettoyez, Optimisez, Automatisez*  
© 2025 - Tous droits réservés
Auteur : C.lecomte (Skill_Team)
