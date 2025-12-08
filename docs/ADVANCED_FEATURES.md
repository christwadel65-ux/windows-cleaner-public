# Fonctionnalités de Nettoyage Avancé - Windows Cleaner

## Vue d'ensemble
Windows Cleaner inclut maintenant un ensemble complet d'options de nettoyage avancé pour les utilisateurs expérimentés qui souhaitent optimiser davantage leur système Windows.

## Options Avancées Disponibles

### 1. **Journaux Système (.evtx)**
**Localisation** : `C:\Windows\System32\winevt\Logs\`
**Description** : Supprime les fichiers journaux d'événements Windows qui accumulent des données sur les applications, les services système et les événements de sécurité.
**Impact** : 
- Peut libérer 100 MB à plusieurs GB selon votre configuration
- Les nouveaux journaux seront recréés automatiquement
- Nécessite les droits administrateur
- Utile pour effacer l'historique ou libérer de l'espace

### 2. **Cache des Installeurs Windows**
**Localisation** : `C:\Windows\Installer\`
**Description** : Nettoie les fichiers d'installation en cache utilisés par le système pour les réparations et les désinstallations de programmes.
**Impact** :
- Peut libérer 500 MB à 2 GB
- Les fichiers critiques ne seront pas supprimés
- Nécessite les droits administrateur
- Utile après avoir désinstallé de nombreux programmes

### 3. **Journaux d'Applications**
**Localisation** : `C:\Users\[User]\AppData\Local\Packages\*/LocalState\`
**Description** : Supprime les fichiers journaux des applications Microsoft Store et des applications modernes Windows.
**Impact** :
- Libère généralement 50 MB à 500 MB
- Les journaux seront recréés au besoin par les applications
- Peut nécessiter les droits administrateur selon les permissions
- Utile pour résoudre les problèmes d'applications ou libérer de l'espace

### 4. **Fichiers Orphelins (> 7 jours)**
**Localisation** : `%TEMP%\`
**Description** : Détecte et supprime les fichiers temporaires qui n'ont pas été modifiés depuis plus de 7 jours. Ces fichiers sont souvent laissés par des installations, des téléchargements ou des applications plantées.
**Impact** :
- Peut libérer 100 MB à 1 GB
- Cible uniquement les fichiers "anciens" pour minimiser les risques
- Sûr pour exécuter régulièrement
- Améliore les performances en supprimant les données orphelines

### 5. **Nettoyage du Cache Mémoire**
**Description** : Force le système à vider les caches de la RAM et du disque, créant de l'espace physique immédiatement disponible.
**Utilisation** :
- Peut améliorer les performances système court terme
- Libère typiquement 100 MB à 1 GB temporairement
- Le cache se reconstitue au fil de l'utilisation
- Requiert les droits administrateur

## Comment Utiliser les Options Avancées

### Activation
1. Ouvrez Windows Cleaner en tant qu'administrateur
2. Sélectionnez les options standard que vous désirez (Corbeille, Temp système, etc.)
3. Scroll vers le bas pour voir la section **"Nettoyage Avancé"**
4. Cochez les options avancées que vous souhaitez activerOptions Avancées (Mode Verbeux & Rapport)
- **Mode Verbeux** : Active les logs détaillés pour chaque action
- **Rapport Avancé** : Affiche une prévisualisation des éléments à supprimer avant l'exécution (non disponible en mode Dry Run)

### Test avec Dry Run
1. Activez les options souhaitées
2. Cliquez sur "🔍 Simuler (Dry Run)"
3. Vérifiez les logs pour voir ce qui serait supprimé
4. Cliquez sur "🧹 Nettoyer" pour exécuter réellement

## Recommandations de Sécurité

⚠️ **Avant de commencer :**
- ✅ Créez une sauvegarde système (point de restauration Windows)
- ✅ Fermez toutes les applications
- ✅ Exécutez en tant qu'administrateur pour accéder à tous les fichiers
- ✅ Utilisez "Dry Run" d'abord pour vérifier l'impact

⚠️ **Précautions :**
- Les journaux système supprimés ne peuvent pas être récupérés
- Certaines applications peuvent ne pas fonctionner correctement si des journaux essentiels sont supprimés
- Le nettoyage du cache mémoire peut ralentir légèrement les performances juste après
- Ne supprimez pas d'options avancées si vous n'êtes pas sûr de leur impact

## Exemples d'Utilisation

### Scenario 1 : Libération d'Espace (Sûr)
```
✓ Corbeille
✓ Temp système
✓ Navigateurs
✓ Vignettes
✓ Fichiers orphelins (> 7 jours)
```
Impact : 500 MB à 3 GB libérés

### Scenario 2 : Nettoyage Complet (Expérimenté)
```
✓ Toutes les options standard
✓ Journaux système
✓ Cache des installeurs
✓ Journaux d'applications
✓ Fichiers orphelins
✓ Nettoyage cache mémoire
```
Impact : 2 GB à 5 GB libérés

### Scenario 3 : Maintenance Régulière
```
✓ Corbeille
✓ Temp système
✓ Navigateurs
✓ Fichiers orphelins
```
Impact : Léger, tous les deux jours

## Dépannage

**Q: L'application ne supprime pas les fichiers**
R: Vérifiez que vous avez exécuté en tant qu'administrateur. Regardez les logs pour les messages d'erreur.

**Q: Les performances se sont dégradées après le nettoyage**
R: C'est temporaire. Le système reconstruit ses caches. Attendez 10-15 minutes d'utilisation normale.

**Q: Une application ne fonctionne plus correctement**
R: Restaurez à partir d'un point de restauration Windows. Réessayez sans l'option problématique.

**Q: Combien d'espace puis-je libérer?**
R: Selon votre configuration : 500 MB à 5 GB. Utilisez "Dry Run" pour estimer.

## Notes Techniques

- Les options avancées fonctionnent en parallèle pour une exécution rapide
- Chaque option effectue des tentatives (retries) automatiques sur les fichiers verrouillés
- Les fichiers en cours d'utilisation ne sont pas supprimés
- Un journal détaillé est créé pour chaque exécution

## Mise à Jour Futur

Les futures versions pourraient inclure:
- Nettoyage des fichiers de cache des applications (AppData)
- Suppression des raccourcis cassés
- Archivage des anciens fichiers journaux
- Optimisation de la partition système

---

**Version** : 1.0  
**Dernière mise à jour** : Décembre 2025
