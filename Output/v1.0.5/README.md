# Windows Cleaner (minimal) - WinForms

Outil minimal en C# (WinForms) pour nettoyer les fichiers temporaires utilisateur et optionnellement la corbeille.

Usage rapide
- Ouvrir un terminal PowerShell
- Se placer dans le dossier du projet

```
dotnet build
dotnet run
```

Notes importantes
- Exécuter l'application en tant qu'administrateur si vous voulez supprimer les fichiers du dossier Temp système.
- Le mode "Dry Run" permet de voir les actions sans supprimer.
- Le vidage de la Corbeille utilise l'API Windows (P/Invoke). Aucune confirmation n'est demandée si l'option est activée.

Options de nettoyage standard
- Nettoyage du cache Chrome, Edge et Firefox (fermez les navigateurs avant d'exécuter pour éviter les fichiers verrouillés).
- Nettoyage du dossier `C:\Windows\SoftwareDistribution\Download` (nécessite les droits administrateur).
- Suppression des fichiers de vignettes (`thumbcache_*.db`) pour récupérer de l'espace.
- Nettoyage du dossier `C:\Windows\Prefetch` (nécessite les droits administrateur).
- Flush DNS (`ipconfig /flushdns`) pour vider le cache DNS local.

**Nouveau : Options de nettoyage avancé**
- **Journaux système (.evtx)** : Supprime les journaux d'événements Windows pour libérer de l'espace.
- **Cache des installeurs** : Nettoie le dossier `C:\Windows\Installer` contenant les fichiers d'installation en cache.
- **Journaux d'applications** : Supprime les fichiers journaux des applications Microsoft Store dans `LocalAppData\Packages`.
- **Fichiers orphelins** : Détecte et supprime les fichiers temporaires non modifiés depuis plus de 7 jours.
- **Nettoyage du cache mémoire** : Vide les caches de la RAM et du disque système (nécessite les droits administrateur).

Interface et logs
- L'application propose une interface professionnelle avec menu, barre de progression et bouton d'annulation.
- Les logs sont enregistrés dans le dossier `logs\windows-cleaner.log` à côté de l'exécutable.
- Vous pouvez exporter les logs via `Fichier -> Exporter les logs`.
- Nouveau : Rapport avancé avec aperçu des éléments à supprimer avant exécution.

Robustesse et retries
- L'application effectue des tentatives (retries) avec backoff pour supprimer les fichiers et dossiers qui peuvent être verrouillés.
- En mode `Dry Run` les suppressions ne sont pas effectuées mais sont listées dans les logs.

Licence (MIT)
----------------
Ce projet est distribué sous la licence MIT. Le texte complet de la licence est inclus ci-dessous et dans le fichier `LICENSE` à la racine du projet.

MIT License

Copyright (c) 2025 c.lecomte

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Auteur
------
`c.lecomte`

Limites
- Cet outil est minimal et ne gère pas tous les cas (verrou de fichiers, profils multiples, nettoyage approfondi). À utiliser avec précaution.

Licence
- Aucun licence particulière fournie ici; utilisez à vos risques.

Developer helper
----------------

Un script PowerShell pratique est inclus pour appliquer le format, vérifier la build et préparer un commit groupé :

- `scripts/prepare_commit.ps1`

Usage :

```powershell
.\scripts\prepare_commit.ps1
```

Le script :
- Exécute `dotnet format` (si installé) ; sinon propose l'installation.
- Exécute `dotnet build` pour vérifier que tout compile.
- Propose d'exécuter `git add -A` + `git commit -m "..."` pour regrouper les modifications en un seul commit.

Conseil : utilisez ce script avant de pousser vos changements pour garder un historique propre.
