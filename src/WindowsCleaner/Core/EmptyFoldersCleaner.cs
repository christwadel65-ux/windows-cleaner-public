using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace WindowsCleaner
{
    /// <summary>
    /// Détecte et supprime les dossiers vides
    /// </summary>
    public static class EmptyFoldersCleaner
    {
        /// <summary>
        /// Détecte tous les dossiers vides
        /// </summary>
        public static List<string> DetectEmptyFolders(string[] basePaths, Action<string>? log = null, CancellationToken cancellationToken = default)
        {
            var emptyFolders = new List<string>();

            try
            {
                log?.Invoke(LanguageManager.Get("log_detecting_empty_folders"));

                foreach (var basePath in basePaths)
                {
                    if (!Directory.Exists(basePath)) continue;

                    try
                    {
                        var emptyDirs = FindEmptyDirectories(basePath, cancellationToken);
                        emptyFolders.AddRange(emptyDirs);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignorer les dossiers inaccessibles
                    }
                }

                log?.Invoke(LanguageManager.Get("log_empty_folders_found", emptyFolders.Count.ToString()));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("error_detecting_empty_folders", ex.Message));
            }

            return emptyFolders;
        }

        /// <summary>
        /// Trouve récursivement tous les dossiers vides
        /// </summary>
        private static List<string> FindEmptyDirectories(string path, CancellationToken cancellationToken)
        {
            var emptyDirs = new List<string>();

            try
            {
                var di = new DirectoryInfo(path);

                // Vérifier d'abord les sous-dossiers récursivement
                foreach (var subDir in di.GetDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        // Récurser dans les sous-dossiers
                        var subEmptyDirs = FindEmptyDirectories(subDir.FullName, cancellationToken);
                        emptyDirs.AddRange(subEmptyDirs);

                        // Vérifier si ce dossier est maintenant vide
                        if (IsDirectoryEmpty(subDir.FullName))
                        {
                            emptyDirs.Add(subDir.FullName);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignorer les dossiers inaccessibles
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs
            }

            return emptyDirs;
        }

        /// <summary>
        /// Vérifie si un dossier est vide (aucun fichier ni sous-dossier)
        /// </summary>
        private static bool IsDirectoryEmpty(string path)
        {
            try
            {
                return !Directory.EnumerateFileSystemEntries(path).Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Nettoie les dossiers vides détectés
        /// </summary>
        public static (int foldersDeleted, long bytesFreed) CleanEmptyFolders(List<string> emptyFolders, bool dryRun, Action<string>? log = null, CancellationToken cancellationToken = default)
        {
            int foldersDeleted = 0;
            long bytesFreed = 0;

            // Trier par longueur décroissante pour supprimer d'abord les dossiers imbriqués
            var sortedFolders = emptyFolders.OrderByDescending(p => p.Length).ToList();

            foreach (var folderPath in sortedFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Directory.Exists(folderPath)) continue;

                try
                {
                    // Vérifier une dernière fois que le dossier est vide
                    if (IsDirectoryEmpty(folderPath))
                    {
                        log?.Invoke(LanguageManager.Get("log_removing_empty_folder", folderPath));

                        if (!dryRun)
                        {
                            try
                            {
                                Directory.Delete(folderPath, false);
                                foldersDeleted++;
                            }
                            catch (Exception ex)
                            {
                                log?.Invoke(LanguageManager.Get("error_removing_empty_folder", folderPath, ex.Message));
                            }
                        }
                        else
                        {
                            foldersDeleted++;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignorer les dossiers inaccessibles
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warning, $"Erreur lors de la suppression du dossier {folderPath}: {ex.Message}");
                }
            }

            return (foldersDeleted, bytesFreed);
        }
    }
}
