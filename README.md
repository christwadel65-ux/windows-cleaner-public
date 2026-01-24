# Windows Cleaner v2.0.9

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Propriétaire-red.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![Version](https://img.shields.io/badge/version-2.0.9-brightgreen.svg)](https://github.com/christwadel65-ux/Windows-Cleaner/releases)

Outil professionnel en C# (WinForms + CLI) pour nettoyer, analyser et optimiser votre système Windows. Interface moderne avec support multilingue (FR/EN), mode sombre, système de profils, CLI complet, statistiques, et mise à jour automatique.

<center><img width="1192" height="618" alt="image" src="https://github.com/user-attachments/assets/66e0b7c9-9bf4-4621-86b3-b7be33391b8c" />
</center>
# Windows Cleaner v2.0.9 - Guide Utilisateur

Bienvenue ! **Windows Cleaner** est un outil professionnel pour nettoyer, analyser et optimiser votre ordinateur Windows. C'est simple, rapide et sûr.

---

## 🎯 Qu'est-ce que Windows Cleaner ?

**Windows Cleaner** nettoie votre ordinateur en supprimant :
- 📁 Les fichiers temporaires inutilisés
- 🗑️ Le cache des navigateurs (Chrome, Firefox, Edge…)
- 📚 L'historique de navigation
- 🔧 Les fichiers de mise à jour Windows
- 👻 Les entrées de programmes désinstallés depuis longtemps
- Et bien d'autres !

**Le résultat** : plus d'espace disque, un ordinateur plus rapide et plus privé.

---

## 💻 Configuration requise

- **Windows** : Windows 10 ou 11 (64-bit)
- **Droits** : Administrateur (pour nettoyer les fichiers système)
- **.NET** : Runtime .NET 10.0 (installé automatiquement si absent)

---

## 🚀 Installation et Lancement

### Option 1 : Installateur (Recommandé)
1. Téléchargez le fichier `WindowsCleaner-Setup-2.0.9.exe`
2. Double-cliquez pour installer
3. Lancez depuis le menu Démarrer ou Bureau



> 💡 **Conseil** : Lancez toujours en tant qu'administrateur pour accéder à toutes les fonctionnalités

---

## 🎮 Utilisation Basique

### Première Utilisation

1. **Ouvrez Windows Cleaner**
   - L'application affiche automatiquement votre **statut de licence** (essai 7 jours ou activée)

2. **Choisissez ce que nettoyer**
   - Les cases cochées par défaut sont sûres
   - Cochez/Décochez selon vos besoins
   - **En doute ?** Gardez les paramètres par défaut !

3. **Cliquez "Démarrer Nettoyage"**
   - Le programme affiche ce qui va être supprimé
   - **Mode Simuler** : voir sans rien supprimer (idéal pour tester)

4. **C'est fait !**
   - Un résumé montre l'espace libéré
   - Les fichiers supprimés vont à la corbeille (vous pouvez récupérer)

### Les Onglets Principaux

**🧹 Standard** (par défaut)
- ✅ Cache navigateurs (Chrome, Edge, Firefox…)
- ✅ Historique de navigation
- ✅ Fichiers temporaires
- ✅ Corbeille vide

**🔬 Avancé** (à utiliser avec prudence)
- Journaux système
- Cache mises à jour Windows
- Applications fantômes (entrées registre orphelines)
- Dossiers vides

**💻 Développeur** (pour les développeurs)
- Caches node_modules, Python, Docker
- Fichiers temporaires Visual Studio
- Et d'autres outils de programmation

**🔒 Confidentialité**
- Historique (Win+R)
- Documents récents
- Presse-papiers

---

## 🌐 Langue

- **Français** 🇫🇷 ou **Anglais** 🇺🇸 disponibles
- Changez dans **Menu → Aide → 🌍 Langue**
- Redémarrage automatique

---

## 🔒 Licence et Essai

### Essai Gratuit
- **3 jours** au premier lancement
- Accès complet sans limite pendant l'essai
- Un avertissement apparaît les 3 derniers jours

### Activation Permanente
- Une fois l'essai expiré, activez une licence pour continuer
- Allez à **Menu → Licence**
- Copiez votre **Hardware ID** et demandez une clé au via ce lien:

💡lien : admin@easycoding.fr

- Entrez la clé pour activer
- ✅ Accès illimité après activation

> 📖 Voir [README_ACTIVATION_LICENCE.md](docs/README_ACTIVATION_LICENCE.md) pour plus de détails

---

## ⚙️ Profils (Automatiser le Nettoyage)

Windows Cleaner propose 4 profils prédéfinis :

| Profil | Utilité |
|--------|---------|
| **Nettoyage Rapide** | 5 minutes, les essentiels |
| **Nettoyage Standard** | 15 minutes, complet sans risque |
| **Nettoyage Complet** | 30 minutes, tout y compris avancé |
| **Nettoyage Développeur** | Pour les développeurs |

**Comment utiliser** :
1. Sélectionnez un profil en haut de la fenêtre
2. Vérifiez les cases cochées
3. Cliquez "Démarrer Nettoyage"

---

## 🎯 Fonctionnalités Populaires

### 📊 Analyse Disque
- Voir les **fichiers les plus volumineux**
- Découvrir quel dossier utilise le plus d'espace
- Menu **Analyse** en haut

### 🔄 Planificateur
- Nettoyer **automatiquement** à une heure précise
- Intégré à Windows Task Scheduler
- Menu **Planificateur**

### 📈 Statistiques
- Historique complet des nettoyages
- Espace libéré par mois/année
- Menu **Statistiques**

### 🔍 Détecteur de Doublons
- Trouver les **fichiers identiques** (même contenu)
- Les supprimer intelligemment
- Menu **Outils → 🔍 Détecteur de Doublons**

### ⚙️ Configuration Système
- Gérer les **programmes au démarrage**
- Désactiver les services inutiles
- Gérer les tâches planifiées
- Menu **Tools → ⚙️ Configuration Système**

### 🗑️ Désinstallateur
- Supprimer complètement les programmes
- Nettoie aussi le registre et AppData
- Meilleur que "Ajouter/Supprimer des programmes"
- Menu **Tools → 🗑️ Désinstallateur**

---

## 🔒 Sécurité et Confiance

✅ **Mode Simuler (Dry Run)**
- Cochez "Mode Simuler" avant de nettoyer
- Voir exactement ce qui sera supprimé
- Parfait pour tester avant de vraiment nettoyer

✅ **Les fichiers vont à la corbeille**
- Pas de suppression définitive immédiate
- Vous avez 30 jours pour récupérer

✅ **Listes blanches intégrées**
- 80+ dossiers système protégés
- Aucun risque de supprimer des fichiers système critiques

✅ **Logs complets**
- Chaque action est enregistrée
- Voir **Fichier → Exporter les logs** pour un rapport

---

## ⚠️ Conseils de Sécurité

1. **Lancez toujours en administrateur**
   - Clique-droit sur l'icône → "Exécuter en tant qu'administrateur"

2. **Testez d'abord avec "Mode Simuler"**
   - Cochez "Mode Simuler" → Cliquez "Démarrer Nettoyage"
   - Vérifiez la liste avant de vraiment nettoyer

3. **Ne décochez pas les protections**
   - Les listes blanches existent pour une raison
   - Ne supprimez pas les "Fichiers système" à moins de savoir ce que vous faites

4. **Fermez vos navigateurs**
   - Windows Cleaner le fait automatiquement
   - Sauvegardez votre travail avant de nettoyer

5. **Nettoyez régulièrement**
   - Une fois par semaine = idéal
   - Une fois par mois = minimum

---

## 🐛 Dépannage Rapide

### L'application ne démarre pas
- **Lancez en administrateur** (clique-droit)
- Vérifiez que .NET 10.0 est installé (voir le message d'erreur)

### Rien n'a été supprimé
- Avez-vous cliqué "Démarrer Nettoyage" ? (pas juste cochez les cases)
- Vérifiez que les cases sont cochées
- Essayez de décocher "Mode Simuler"

### Le nettoyage est très lent
- C'est normal pour un premier nettoyage (analysez d'abord)
- Fermez les autres applications
- Relancez la machine après le nettoyage

### Erreur "Droits insuffisants"
- Lancez en tant qu'administrateur
- Certains fichiers verrouillés par Windows ne peuvent pas être supprimés (normal)

### Licence invalide
- Vérifiez que le Hardware ID fourni au support est exact
- Si vous avez changé le disque dur ou CPU, le Hardware ID change (demandez une nouvelle clé)

---

## 📞 Support et Aide

📧 **Contact** : contact@easycoding.fr  
💻 **Logs** : Voir dans **Fichier → Exporter les logs**  
📖 **Documentation** : Dossier `docs/` du programme

---

## 🎓 Cas d'Usage Courants

### "Mon disque est plein (100%)"
1. Lancez Analyse Disque
2. Sélectionnez "Nettoyage Complet"
3. Lancez le nettoyage
4. → Gagnez souvent 10-50 GB !

### "Mon ordi est lent"
1. Utilisez "Nettoyage Rapide"
2. Activez le planificateur pour hebdomadaire
3. Redémarrez votre PC après
4. → À faire une fois par semaine

### "Je veux un ordi très privé"
1. Cochez tout dans l'onglet "Confidentialité"
2. Cochez "Nettoyage Complet"
3. Lancez le nettoyage
4. → Aucune trace de navigation

### "Je suis développeur"
1. Sélectionnez "Nettoyage Développeur"
2. Choisissez les outils (VS Code, node_modules, etc.)
3. Lancez
4. → Gagnez plusieurs GB sur les caches

---

## ✨ Bonnes Pratiques

- 🔁 **Nettoyez une fois par semaine** pour maintenir la performance
- 📋 **Gardez un backup** (disque externe) avant gros nettoyage
- 🧪 **Testez avec "Mode Simuler"** en cas de doute
- 📊 **Consultez les statistiques** pour voir l'impact
- 💾 **Gardez l'historique** pour monitoring (Menu **Fichier**)

---

## 📝 Notes Finales

- **Windows Cleaner ne supprime JAMAIS les fichiers importants**
  - Listes blanches de 80+ dossiers système
  - Mode Simuler pour vérifier
  
- **Vos données personnelles ne quittent jamais votre PC**
  - Pas de connexion à internet pour nettoyer
  - Hors ligne = sûr et rapide
  
- **Vous contrôlez tout**
  - Chaque case peut être décochée
  - Vous décidez ce qui part

---

**Merci d'utiliser Windows Cleaner ! 🎉**

Pour toute question, contactez le support ou consultez la documentation complète dans `docs/`.
