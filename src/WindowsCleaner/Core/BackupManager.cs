using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WindowsCleaner
{
    /// <summary>
    /// Gestionnaire de sauvegarde et restauration avant nettoyage
    /// </summary>
    public static class BackupManager
    {
        private static readonly string BackupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WindowsCleaner",
            "Backups"
        );
        
        private static readonly string BackupHistoryFile = Path.Combine(BackupDirectory, "backup_history.txt");
        
        static BackupManager()
        {
            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);
            }
        }
        
        /// <summary>
        /// Crée un point de restauration système Windows
        /// </summary>
        public static bool CreateSystemRestorePoint(string description)
        {
            try
            {
                Logger.Log(LogLevel.Info, "Création d'un point de restauration système...");
                
                // Utiliser PowerShell pour créer un point de restauration
                var script = $@"
                    try {{
                        Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS'
                        exit 0
                    }} catch {{
                        exit 1
                    }}
                ";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas" // Nécessite élévation
                };
                
                using var process = Process.Start(psi);
                if (process == null)
                    return false;
                
                process.WaitForExit(60000); // Timeout 60s
                
                if (process.ExitCode == 0)
                {
                    Logger.Log(LogLevel.Info, "Point de restauration créé avec succès");
                    return true;
                }
                else
                {
                    var error = process.StandardError.ReadToEnd();
                    Logger.Log(LogLevel.Warning, $"Échec création point de restauration: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur création point de restauration: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Crée une sauvegarde des fichiers avant suppression
        /// </summary>
        public static string? CreateBackup(List<string> filesToBackup, string backupName)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFolder = Path.Combine(BackupDirectory, $"{backupName}_{timestamp}");
                Directory.CreateDirectory(backupFolder);
                
                int backedUpCount = 0;
                long totalSize = 0;
                
                foreach (var filePath in filesToBackup)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            var fileInfo = new FileInfo(filePath);
                            var relativePath = filePath.Replace(":", "_").Replace("\\", "_");
                            var backupPath = Path.Combine(backupFolder, relativePath);
                            
                            File.Copy(filePath, backupPath, true);
                            backedUpCount++;
                            totalSize += fileInfo.Length;
                        }
                        else if (Directory.Exists(filePath))
                        {
                            // Pour les dossiers, créer une archive
                            var dirName = Path.GetFileName(filePath);
                            var zipPath = Path.Combine(backupFolder, $"{dirName}.zip");
                            System.IO.Compression.ZipFile.CreateFromDirectory(filePath, zipPath);
                            backedUpCount++;
                            totalSize += new FileInfo(zipPath).Length;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Impossible de sauvegarder {filePath}: {ex.Message}");
                    }
                }
                
                // Enregistrer l'historique
                var historyEntry = $"{timestamp}|{backupName}|{backedUpCount}|{totalSize}|{backupFolder}";
                File.AppendAllText(BackupHistoryFile, historyEntry + Environment.NewLine);
                
                Logger.Log(LogLevel.Info, $"Sauvegarde créée: {backedUpCount} éléments, {FormatBytes(totalSize)}");
                
                // Nettoyer les anciennes sauvegardes (> 24h)
                CleanOldBackups();
                
                return backupFolder;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur création sauvegarde: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Restaure les fichiers depuis une sauvegarde
        /// </summary>
        public static bool RestoreBackup(string backupFolder)
        {
            try
            {
                if (!Directory.Exists(backupFolder))
                {
                    Logger.Log(LogLevel.Error, $"Dossier de sauvegarde introuvable: {backupFolder}");
                    return false;
                }
                
                int restoredCount = 0;
                
                foreach (var backupFile in Directory.GetFiles(backupFolder))
                {
                    try
                    {
                        var fileName = Path.GetFileName(backupFile);
                        var originalPath = fileName.Replace("_", "\\");
                        
                        // Reconstruire le chemin original
                        if (originalPath.Length > 1 && char.IsLetter(originalPath[0]))
                        {
                            originalPath = originalPath[0] + ":" + originalPath.Substring(1);
                        }
                        
                        var originalDir = Path.GetDirectoryName(originalPath);
                        if (originalDir != null && !Directory.Exists(originalDir))
                        {
                            Directory.CreateDirectory(originalDir);
                        }
                        
                        File.Copy(backupFile, originalPath, true);
                        restoredCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warning, $"Impossible de restaurer {backupFile}: {ex.Message}");
                    }
                }
                
                Logger.Log(LogLevel.Info, $"Restauration terminée: {restoredCount} éléments restaurés");
                return restoredCount > 0;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur restauration sauvegarde: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Liste toutes les sauvegardes disponibles
        /// </summary>
        public static List<(string Timestamp, string Name, string Path)> ListBackups()
        {
            var backups = new List<(string, string, string)>();
            
            try
            {
                if (!File.Exists(BackupHistoryFile))
                    return backups;
                
                var lines = File.ReadAllLines(BackupHistoryFile);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        var timestamp = parts[0];
                        var name = parts[1];
                        var path = parts[4];
                        
                        if (Directory.Exists(path))
                        {
                            backups.Add((timestamp, name, path));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lecture historique sauvegardes: {ex.Message}");
            }
            
            // Tri optimisé sans LINQ pour minimiser les allocations
            for (int i = 0; i < backups.Count - 1; i++)
            {
                for (int j = i + 1; j < backups.Count; j++)
                {
                    if (string.Compare(backups[j].Item1, backups[i].Item1) > 0)
                    {
                        var temp = backups[i];
                        backups[i] = backups[j];
                        backups[j] = temp;
                    }
                }
            }
            
            return backups;
        }
        
        /// <summary>
        /// Supprime les sauvegardes de plus de 24 heures
        /// </summary>
        private static void CleanOldBackups()
        {
            try
            {
                var cutoffTime = DateTime.Now.AddHours(-24);
                var directories = Directory.GetDirectories(BackupDirectory);
                
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.CreationTime < cutoffTime)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            Logger.Log(LogLevel.Info, $"Ancienne sauvegarde supprimée: {dir}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Warning, $"Impossible de supprimer ancienne sauvegarde {dir}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur nettoyage anciennes sauvegardes: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Supprime toutes les sauvegardes
        /// </summary>
        public static void DeleteAllBackups()
        {
            try
            {
                if (Directory.Exists(BackupDirectory))
                {
                    Directory.Delete(BackupDirectory, true);
                    Directory.CreateDirectory(BackupDirectory);
                    Logger.Log(LogLevel.Info, "Toutes les sauvegardes ont été supprimées");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur suppression sauvegardes: {ex.Message}");
            }
        }
        
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
