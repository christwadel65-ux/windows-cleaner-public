# Windows Cleaner v2.0.6

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Propriétaire-red.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![Version](https://img.shields.io/badge/version-2.0.5-brightgreen.svg)](https://github.com/christwadel65-ux/Windows-Cleaner/releases)

Outil professionnel en C# (WinForms + CLI) pour nettoyer, analyser et optimiser votre système Windows. Interface moderne avec support multilingue (FR/EN), mode sombre, système de profils, CLI complet, statistiques, et mise à jour automatique.

<center><img width="1192" height="618" alt="image" src="https://github.com/user-attachments/assets/66e0b7c9-9bf4-4621-86b3-b7be33391b8c" />
</center>

🔒 Windows Cleaner dispose maintenant d'un système de licence complet avec:

✅ Essai gratuit 7 jours automatique
✅ Verrouillage par Hardware ID (unique par ordinateur)
✅ Activation de licence permanente
✅ Interfaces conviviales pour la gestion de licence

Pour toute demande merci de passer par notre site : 
easycoding.fr 
contact : admin@easycoding.fr


## ✨ Fonctionnalités Principales

## 🔄 Historique des Versions

### 🆕 Dernières Modifications (17/01/2026) V2.0.7

#### 🛠️ Corrections et Améliorations
- **Affichage statut licence au démarrage** : État de la licence visible dans le Operations Log à chaque lancement
- **Logs de démarrage détaillés** : Version, heure et statut licence (Essai/Activée/Permanente) enregistrés
- **Réaffichage après Clear Logs** : Le statut licence réapparaît après effacement des logs
- **Fix Settings saved au startup** : Suppression des logs "Settings saved successfully" au démarrage

### V2.0.6 (15/01/2026)

#### 🛠️ Corrections et Améliorations
- **Nettoyage Edge amélioré** : Ajout support fichiers SQLite WAL (History-wal, History-shm, WebData)
- **Multi-profils Edge** : Nettoyage de tous les profils Edge (Default + profils utilisateurs)
- **Stabilisation Hardware ID** : Tri par nom d'adaptateur réseau (stable avec/sans internet)
- **Protection anti-fraude licence** : Date première installation en registre Windows (empêche régénération essai)
- **Préservation licence** : Setup Inno ne supprime plus les données de licence lors réinstallation
- **Fermeture navigateurs optimisée** : Support msedgewebview2, délais ajustés (1500ms), 2 tentatives de suppression
- **Messages d'erreur améliorés** : Message réseau plus clair sans connexion internet
- **Build optimisé** : Script build.bat avec publish + nettoyage langues inutiles (garde FR/EN uniquement)
- **Logs améliorés** : Affichage date d'expiration licence au lieu de debug Hardware ID

### ⚙️ Configuration Système Avancée (v2.0.5)
- **Gestionnaire de démarrage complet** : Programmes au démarrage (HKCU/HKLM/Dossiers)
- **Gestion des services Windows** : Démarrage/Arrêt/Modification du type de démarrage
- **Tâches planifiées** : Activation/Désactivation des tâches Windows
- **Désactivation non-destructive** : Sauvegarde des programmes désactivés pour réactivation
- **Support RunOnce** : Détection des programmes à exécution unique
- **Wow6432Node** : Support des applications 32-bit sur Windows 64-bit
- **En-têtes colorés** : Interface moderne avec couleurs d'accent par onglet
- **Recherche en temps réel** : Filtrage instantané dans tous les onglets
- **Statuts normalisés** : Affichage cohérent (Ready, Disabled, Running, etc.)
- **Menu Tools** : Accès via "⚙️ Configuration Système"

### �️ Désinstallateur Complet de Programmes (v2.0.4)
- **Interface professionnelle** : Design moderne avec en-tête coloré
- **Recherche en temps réel** : Filtrage instantané des programmes
- **Désinstallation complète** : Programme + fichiers + registre + AppData
- **Nettoyage profond du registre** : HKEY_LOCAL_MACHINE + HKEY_CURRENT_USER
- **Export CSV** : Liste complète des programmes installés
- **Sélection multiple** : Désinstalle plusieurs programmes à la fois
- **Logs en direct** : Suivi complet des opérations en bas de fenêtre
- **Traduction multilingue** : Interface complète FR/EN
- **Menu Tools** : Accès facile via "🗑️ Désinstallateur"
- **Alternating row colors** : Meilleure lisibilité des listes

### �👻 Nettoyage Applications Fantômes (v2.0.3)
- **Détection des applications fantômes** : entrées registre invalides (HKLM/HKCU)
- **Sécurité maximale** : liste blanche de 80+ dossiers système pour zéro faux positif
- **Mode Dry-Run** : prévisualise avant suppression
- **Case "👻 Applications fantômes"** dans groupe Advanced
- **Rapport détaillé** : statistiques sur applications détectées/supprimées

### 📁 Nettoyage des Dossiers Vides (v2.0.3)
- **Détection récursive** : trouve tous les dossiers vides
- **Suppression intelligente** : traite d'abord les dossiers imbriqués
- **Vérification double** : vérifie que vide avant suppression
- **Case "📁 Dossiers vides"** dans groupe Advanced
- **Mode Dry-Run** : aperçu avant suppression
- **100% sûr** : ne touche que les dossiers vides

### 🌍 Interface Multilingue (v2.0.2)
- **Support complet** : Français 🇫🇷 et Anglais 🇺🇸
- **Changement en direct** : Menu Aide → 🌍 Langue (redémarrage automatique)
- **Traduction complète** : Interface, profils, logs, messages
- **Persistance** : Préférence sauvegardée automatiquement
- **Langue par défaut** : Anglais (modifiable)

### 🔒 Amélioration Vie Privée (v2.0.2)
- **Case dédiée "🕘 Historique navigateurs"** cochée par défaut
- **Nettoyage complet** : Chrome/Edge/Firefox (History, places.sqlite, sessions, onglets récents)
- **Fermeture automatique** des navigateurs avant nettoyage
- **Option indépendante** : choix séparé du cache navigateurs
- **Statistiques intégrées** : fichiers supprimés et espace libéré

### 🔄 Système de Mise à Jour Automatique (v2.0.0)
- Vérification automatique au démarrage via GitHub API
- Menu "Aide > 🔄 Vérifier les mises à jour"
- Notification discrète dans la barre de statut
- Dialogue avec version, date et notes de version
- Accès direct à la page de téléchargement

### 💻 Nettoyage Développeur (v2.0.0)
**10 options spécialisées** : VS Code, NuGet, Maven, npm, Docker, node_modules, Visual Studio, Python, Git, Jeux (Steam/Epic)

### 📊 Optimisations SSD (v2.0.0)
- **TRIM automatique** et **vérifications SMART** avec compteurs et rapports détaillés
- Détection multi-niveaux (Win32_DiskDrive + Get-Volume)
- Sauvegarde automatique de l'historique

## ⚠️ Notes Importantes
- **Droits Administrateur** : Requis pour nettoyer les fichiers système (Temp système, Prefetch, Windows Update)
- **Mode Simuler** : Testez les opérations sans supprimer de fichiers
- **Corbeille** : Vidage sans confirmation via l'API Windows (P/Invoke)

## ✨ Fonctionnalités Complètes

### 🧹 Nettoyage Standard
- Cache navigateurs (Chrome, Edge, Firefox, Brave, Opera, Vivaldi) + fermeture automatique
- Historique navigateurs + onglets récents/sessions
- Fichiers temporaires (utilisateur et système)
- Windows Update, Vignettes, Prefetch
- Flush DNS, Corbeille

### 🔬 Nettoyage Avancé
- Journaux système (.evtx), Cache installeurs
- Journaux d'applications (Microsoft Store)
- Fichiers orphelins (> 7 jours)
- Cache mémoire, Raccourcis cassés
- **👻 Applications fantômes** : détecte et supprime entrées registre invalides
- **📁 Dossiers vides** : détecte et supprime récursivement dossiers vides

### 💻 Nettoyage Développeur
Docker, Node.js (node_modules), Visual Studio (obj/bin/.vs), Python (__pycache__), Git, VS Code, NuGet, Maven, npm, Jeux (Steam/Epic)

### 🔒 Protection Vie Privée
Historique Exécuter (Win+R), Documents récents, Timeline Windows, Historique recherche, Presse-papiers

### 🎯 Autres Fonctionnalités
- **Profils** : 4 profils prédéfinis + personnalisables (JSON)
- **Analyse disque** : Catégorisation par type + top fichiers volumineux
- **Détecteur de doublons** : Hash MD5 + filtres + suppression intelligente
- **Planificateur** : Intégration Windows Task Scheduler
- **Statistiques & Rapports** : Rapports HTML + historique complet
- **Backup/Restauration** : Points de restauration système
- **CLI complet** : --profile, --dry-run, --silent, --stats, --help
- **Alertes intelligentes** : Disque < 10%, cache > 2GB, rappels maintenance
- **Optimisations** : TRIM SSD, compaction registre

### 🎨 Interface
- Thème sombre/clair
- Barre de progression temps réel
- Mode simulation (Dry Run)
- Boutons ✅ Tout / ❌ Rien
- Logs en temps réel
- Infobulles contextuelles

## 🛡️ Sécurité & Robustesse
- Gestion d'erreurs complète + logs
- Thread-safe + retry logic (8 tentatives)
- Mode Dry-Run (test sans suppression)
- Backup automatique < 24h
- Points de restauration système
- CancellationToken pour annulation

## � Documentation

- [CHANGELOG.md](CHANGELOG.md) - Historique des versions
- [docs/UPDATE_GUIDE.md](docs/UPDATE_GUIDE.md) - Guide de mise à jour
- [docs/USAGE_GUIDE.md](docs/USAGE_GUIDE.md) - Guide d'utilisation
- [docs/ADVANCED_FEATURES.md](docs/ADVANCED_FEATURES.md) - Fonctionnalités avancées

## 📜 Licence

Licence Propriétaire - Copyright (c) 2025 [easycoding.fr](https://easycoding.fr)

## 👤 Auteur

**[easycoding.fr](https://easycoding.fr)**

## 🔗 Liens

- **Repository** : [christwadel65-ux/Windows-Cleaner](https://github.com/christwadel65-ux/Windows-Cleaner)
- **Issues** : [Signaler un bug](https://github.com/christwadel65-ux/Windows-Cleaner/issues)
- **Releases** : [Télécharger](https://github.com/christwadel65-ux/Windows-Cleaner/releases)

## 🙏 Contribution

1. Fork le projet
2. Créer une branche (`git checkout -b feature/NewFeature`)
3. Commit (`git commit -m 'Add NewFeature'`)
4. Push (`git push origin feature/NewFeature`)
5. Ouvrir une Pull Request

