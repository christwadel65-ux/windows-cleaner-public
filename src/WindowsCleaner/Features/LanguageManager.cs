using System;
using System.Collections.Generic;

namespace WindowsCleaner
{
    public static class LanguageManager
    {
        public enum Language
        {
            French,
            English
        }

        public static Language CurrentLanguage { get; private set; } = Language.English;

        private static Dictionary<string, Dictionary<Language, string>> _translations = new Dictionary<string, Dictionary<Language, string>>
        {
            // Window & Menus
            { "window_title", new Dictionary<Language, string> { { Language.French, "Windows Cleaner - Nettoyage Professionnel" }, { Language.English, "Windows Cleaner - Professional Cleaning" } } },
            { "menu_file", new Dictionary<Language, string> { { Language.French, "Fichier" }, { Language.English, "File" } } },
            { "menu_view", new Dictionary<Language, string> { { Language.French, "Affichage" }, { Language.English, "View" } } },
            { "menu_tools", new Dictionary<Language, string> { { Language.French, "Outils" }, { Language.English, "Tools" } } },
            { "menu_help", new Dictionary<Language, string> { { Language.French, "Aide" }, { Language.English, "Help" } } },
            
            // File Menu
            { "menu_clear_logs", new Dictionary<Language, string> { { Language.French, "Effacer les logs" }, { Language.English, "Clear logs" } } },
            { "menu_read_logs", new Dictionary<Language, string> { { Language.French, "📖 Lire les logs" }, { Language.English, "📖 Read logs" } } },
            { "menu_export_logs", new Dictionary<Language, string> { { Language.French, "Exporter les logs" }, { Language.English, "Export logs" } } },
            { "menu_exit", new Dictionary<Language, string> { { Language.French, "Quitter" }, { Language.English, "Exit" } } },
            
            // View Menu
            { "menu_theme_light", new Dictionary<Language, string> { { Language.French, "Thème Clair" }, { Language.English, "Light Theme" } } },
            { "menu_theme_dark", new Dictionary<Language, string> { { Language.French, "Thème Sombre" }, { Language.English, "Dark Theme" } } },
            { "menu_accent_blue", new Dictionary<Language, string> { { Language.French, "Accent Bleu" }, { Language.English, "Blue Accent" } } },
            { "menu_accent_green", new Dictionary<Language, string> { { Language.French, "Accent Vert" }, { Language.English, "Green Accent" } } },
            { "menu_accent_orange", new Dictionary<Language, string> { { Language.French, "Accent Orange" }, { Language.English, "Orange Accent" } } },
            
            // Tools Menu
            { "menu_disk_analyzer", new Dictionary<Language, string> { { Language.French, "📊 Analyser l'espace disque" }, { Language.English, "📊 Analyze disk space" } } },
            { "menu_duplicate_finder", new Dictionary<Language, string> { { Language.French, "🔍 Détecter les doublons" }, { Language.English, "🔍 Find duplicates" } } },
            { "menu_statistics", new Dictionary<Language, string> { { Language.French, "📈 Voir les statistiques" }, { Language.English, "📈 View statistics" } } },
            { "menu_profiles", new Dictionary<Language, string> { { Language.French, "📋 Gérer les profils" }, { Language.English, "📋 Manage profiles" } } },
            { "menu_scheduler", new Dictionary<Language, string> { { Language.French, "⏰ Planifier un nettoyage" }, { Language.English, "⏰ Schedule cleaning" } } },
            { "menu_backup", new Dictionary<Language, string> { { Language.French, "💾 Créer un point de restauration" }, { Language.English, "💾 Create restore point" } } },
            { "menu_optimizer", new Dictionary<Language, string> { { Language.French, "⚡ Optimiser le système" }, { Language.English, "⚡ Optimize system" } } },
            
            // Help Menu
            { "menu_check_updates", new Dictionary<Language, string> { { Language.French, "🔄 Vérifier les mises à jour" }, { Language.English, "🔄 Check for updates" } } },
            { "menu_language", new Dictionary<Language, string> { { Language.French, "🌍 Langue" }, { Language.English, "🌍 Language" } } },
            { "menu_about", new Dictionary<Language, string> { { Language.French, "À propos" }, { Language.English, "About" } } },
            
            // Language submenu
            { "lang_french", new Dictionary<Language, string> { { Language.French, "🇫🇷 Français" }, { Language.English, "🇫🇷 French" } } },
            { "lang_english", new Dictionary<Language, string> { { Language.French, "🇺🇸 Anglais" }, { Language.English, "🇺🇸 English" } } },
            
            // Actions Group
            { "group_actions", new Dictionary<Language, string> { { Language.French, "Actions" }, { Language.English, "Actions" } } },
            { "label_profile", new Dictionary<Language, string> { { Language.French, "Profil de nettoyage" }, { Language.English, "Cleaning profile" } } },
            { "btn_simulate", new Dictionary<Language, string> { { Language.French, "🔍 Simuler" }, { Language.English, "🔍 Simulate" } } },
            { "btn_clean", new Dictionary<Language, string> { { Language.French, "🧹 Nettoyer" }, { Language.English, "🧹 Clean" } } },
            { "btn_select_all", new Dictionary<Language, string> { { Language.French, "✅\nTout" }, { Language.English, "✅\nAll" } } },
            { "btn_deselect_all", new Dictionary<Language, string> { { Language.French, "❌\nRien" }, { Language.English, "❌\nNone" } } },
            { "btn_cancel", new Dictionary<Language, string> { { Language.French, "✖ Annuler" }, { Language.English, "✖ Cancel" } } },
            
            // Tooltips
            { "tooltip_select_all", new Dictionary<Language, string> { { Language.French, "Cocher toutes les options de nettoyage en un clic" }, { Language.English, "Check all cleaning options at once" } } },
            { "tooltip_deselect_all", new Dictionary<Language, string> { { Language.French, "Décocher toutes les options de nettoyage en un clic" }, { Language.English, "Uncheck all cleaning options at once" } } },
            { "tooltip_simulate", new Dictionary<Language, string> { { Language.French, "Simuler le nettoyage sans supprimer de fichiers\n(Mode test sûr)" }, { Language.English, "Simulate cleaning without deleting files\n(Safe test mode)" } } },
            { "tooltip_clean", new Dictionary<Language, string> { { Language.French, "Exécuter le nettoyage avec suppression réelle\n(Vérifiez votre sélection avant de cliquer)" }, { Language.English, "Execute cleaning with actual deletion\n(Check your selection before clicking)" } } },
            
            // Options Group
            { "group_standard", new Dictionary<Language, string> { { Language.French, "Nettoyage Standard" }, { Language.English, "Standard Cleaning" } } },
            { "chk_recycle", new Dictionary<Language, string> { { Language.French, "🗑️ Corbeille" }, { Language.English, "🗑️ Recycle Bin" } } },
            { "chk_system_temp", new Dictionary<Language, string> { { Language.French, "📁 Temp Système" }, { Language.English, "📁 System Temp" } } },
            { "chk_browsers", new Dictionary<Language, string> { { Language.French, "🌐 Navigateurs" }, { Language.English, "🌐 Browsers" } } },
            { "chk_windows_update", new Dictionary<Language, string> { { Language.French, "🔄 Windows Update" }, { Language.English, "🔄 Windows Update" } } },
            { "chk_thumbnails", new Dictionary<Language, string> { { Language.French, "🖼️ Vignettes" }, { Language.English, "🖼️ Thumbnails" } } },
            { "chk_prefetch", new Dictionary<Language, string> { { Language.French, "⚡ Prefetch" }, { Language.English, "⚡ Prefetch" } } },
            { "chk_flush_dns", new Dictionary<Language, string> { { Language.French, "🔗 Flush DNS" }, { Language.English, "🔗 Flush DNS" } } },
            { "chk_browser_history", new Dictionary<Language, string> { { Language.French, "🕘 Historique navigateurs" }, { Language.English, "🕘 Browser history" } } },
            
            // Advanced Group
            { "group_advanced", new Dictionary<Language, string> { { Language.French, "Options Avancées" }, { Language.English, "Advanced Options" } } },
            { "chk_verbose", new Dictionary<Language, string> { { Language.French, "📝 Mode verbeux" }, { Language.English, "📝 Verbose mode" } } },
            { "chk_report", new Dictionary<Language, string> { { Language.French, "📊 Rapport détaillé" }, { Language.English, "📊 Detailed report" } } },
            { "chk_orphaned", new Dictionary<Language, string> { { Language.French, "🧩 Fichiers orphelins" }, { Language.English, "🧩 Orphaned files" } } },
            { "chk_memory_cache", new Dictionary<Language, string> { { Language.French, "💾 Cache mémoire" }, { Language.English, "💾 Memory cache" } } },
            { "chk_broken_shortcuts", new Dictionary<Language, string> { { Language.French, "🔗 Raccourcis cassés" }, { Language.English, "🔗 Broken shortcuts" } } },
            { "chk_ghost_apps", new Dictionary<Language, string> { { Language.French, "👻 Applications fantômes" }, { Language.English, "👻 Ghost apps" } } },
            { "chk_empty_folders", new Dictionary<Language, string> { { Language.French, "📁 Dossiers vides" }, { Language.English, "📁 Empty folders" } } },
            
            // Developer Group
            { "group_developer", new Dictionary<Language, string> { { Language.French, "💻 Nettoyage Développeur" }, { Language.English, "💻 Developer Cleaning" } } },
            { "chk_vscode", new Dictionary<Language, string> { { Language.French, "📦 VS Code" }, { Language.English, "📦 VS Code" } } },
            { "chk_nuget", new Dictionary<Language, string> { { Language.French, "📦 NuGet" }, { Language.English, "📦 NuGet" } } },
            { "chk_maven", new Dictionary<Language, string> { { Language.French, "📦 Maven" }, { Language.English, "📦 Maven" } } },
            { "chk_npm", new Dictionary<Language, string> { { Language.French, "📦 npm" }, { Language.English, "📦 npm" } } },
            { "chk_docker", new Dictionary<Language, string> { { Language.French, "🐳 Docker" }, { Language.English, "🐳 Docker" } } },
            { "chk_node_modules", new Dictionary<Language, string> { { Language.French, "📁 node_modules" }, { Language.English, "📁 node_modules" } } },
            { "chk_visual_studio", new Dictionary<Language, string> { { Language.French, "🔨 Visual Studio" }, { Language.English, "🔨 Visual Studio" } } },
            { "chk_python", new Dictionary<Language, string> { { Language.French, "🐍 Python" }, { Language.English, "🐍 Python" } } },
            { "chk_git", new Dictionary<Language, string> { { Language.French, "📂 Git" }, { Language.English, "📂 Git" } } },
            { "chk_games", new Dictionary<Language, string> { { Language.French, "🎮 Jeux (Steam/Epic)" }, { Language.English, "🎮 Games (Steam/Epic)" } } },
            
            // Logs Group
            { "group_logs", new Dictionary<Language, string> { { Language.French, "📋 Journal des Opérations" }, { Language.English, "📋 Operations Log" } } },
            { "log_time", new Dictionary<Language, string> { { Language.French, "Heure" }, { Language.English, "Time" } } },
            { "log_level", new Dictionary<Language, string> { { Language.French, "Niveau" }, { Language.English, "Level" } } },
            { "log_message", new Dictionary<Language, string> { { Language.French, "Message" }, { Language.English, "Message" } } },
            
            // Status messages
            { "status_profile_applied", new Dictionary<Language, string> { { Language.French, "Profil appliqué: {0}" }, { Language.English, "Profile applied: {0}" } } },
            { "profile_custom", new Dictionary<Language, string> { { Language.French, "Personnalisé (manuel)" }, { Language.English, "Custom (manual)" } } },
            
            // Admin warning
            { "warning_admin_title", new Dictionary<Language, string> { { Language.French, "Avertissement : Droits insuffisants" }, { Language.English, "Warning: Insufficient Rights" } } },
            { "warning_admin_message", new Dictionary<Language, string> { 
                { Language.French, "⚠️ Cette application doit s'exécuter en mode Administrateur pour fonctionner correctement.\n\n" +
                    "Certaines opérations de nettoyage (fichiers système, Temp système, Windows Update, Prefetch, Flush DNS) nécessitent les droits administrateur.\n\n" +
                    "Le nettoyage des fichiers utilisateur fonctionnera partiellement sans admin." }, 
                { Language.English, "⚠️ This application must run as Administrator to work properly.\n\n" +
                    "Some cleaning operations (system files, System Temp, Windows Update, Prefetch, Flush DNS) require administrator rights.\n\n" +
                    "User file cleaning will work partially without admin." } 
            } },
            
            // Language change message
            { "language_changed_title", new Dictionary<Language, string> { { Language.French, "Langue modifiée" }, { Language.English, "Language Changed" } } },
            { "language_changed_message", new Dictionary<Language, string> { 
                { Language.French, "La langue a été changée. L'application va redémarrer pour appliquer les modifications." }, 
                { Language.English, "The language has been changed. The application will restart to apply the changes." } 
            } },
            
            // Settings messages
            { "settings_saved", new Dictionary<Language, string> { { Language.French, "Paramètres sauvegardés avec succès" }, { Language.English, "Settings saved successfully" } } },
            { "settings_save_error", new Dictionary<Language, string> { { Language.French, "Erreur sauvegarde settings: {0}" }, { Language.English, "Error saving settings: {0}" } } },
            { "settings_load_error", new Dictionary<Language, string> { { Language.French, "Erreur chargement settings: {0}" }, { Language.English, "Error loading settings: {0}" } } },
            
            // Update messages
            { "update_checking", new Dictionary<Language, string> { { Language.French, "🔍 Vérification des mises à jour..." }, { Language.English, "🔍 Checking for updates..." } } },
            { "update_available", new Dictionary<Language, string> { { Language.French, "✨ Nouvelle version disponible : {0} (Cliquez sur Aide > Vérifier les mises à jour)" }, { Language.English, "✨ New version available: {0} (Click Help > Check for updates)" } } },
            { "update_current_version", new Dictionary<Language, string> { { Language.French, "✅ Vous utilisez la dernière version ({0})" }, { Language.English, "✅ You are using the latest version ({0})" } } },
            { "update_check_error", new Dictionary<Language, string> { { Language.French, "Erreur vérification mise à jour : {0}" }, { Language.English, "Update check error: {0}" } } },
            
            // Cleaning profiles
            { "profile_quick", new Dictionary<Language, string> { { Language.French, "Nettoyage Rapide" }, { Language.English, "Quick Cleaning" } } },
            { "profile_complete", new Dictionary<Language, string> { { Language.French, "Nettoyage Complet" }, { Language.English, "Complete Cleaning" } } },
            { "profile_developer", new Dictionary<Language, string> { { Language.French, "Nettoyage Développeur" }, { Language.English, "Developer Cleaning" } } },
            { "profile_privacy", new Dictionary<Language, string> { { Language.French, "Protection Vie Privée" }, { Language.English, "Privacy Protection" } } },
            
            { "profile_quick_desc", new Dictionary<Language, string> { { Language.French, "Nettoyage rapide des fichiers temporaires et caches navigateurs" }, { Language.English, "Quick cleaning of temporary files and browser caches" } } },
            { "profile_complete_desc", new Dictionary<Language, string> { { Language.French, "Nettoyage complet incluant système et options avancées" }, { Language.English, "Complete cleaning including system and advanced options" } } },
            { "profile_developer_desc", new Dictionary<Language, string> { { Language.French, "Nettoyage spécialisé pour développeurs (Node, Python, VS, Git, Docker)" }, { Language.English, "Specialized cleaning for developers (Node, Python, VS, Git, Docker)" } } },
            { "profile_privacy_desc", new Dictionary<Language, string> { { Language.French, "Efface l'historique et les traces d'activité Windows" }, { Language.English, "Clears history and Windows activity traces" } } },
            
            // Log messages - Common
            { "log_cleaning_start", new Dictionary<Language, string> { { Language.French, "Début du nettoyage ({0})..." }, { Language.English, "Starting cleanup ({0})..." } } },
            { "log_dryrun", new Dictionary<Language, string> { { Language.French, "dry-run" }, { Language.English, "dry-run" } } },
            { "log_execution", new Dictionary<Language, string> { { Language.French, "exécution" }, { Language.English, "execution" } } },
            { "log_finished", new Dictionary<Language, string> { { Language.French, "Terminé. Fichiers supprimés: {0}, Octets libérés: {1}" }, { Language.English, "Finished. Files deleted: {0}, Bytes freed: {1}" } } },
            { "log_cancelled", new Dictionary<Language, string> { { Language.French, "Opération annulée par l'utilisateur." }, { Language.English, "Operation cancelled by user." } } },
            { "log_error", new Dictionary<Language, string> { { Language.French, "Erreur: {0}" }, { Language.English, "Error: {0}" } } },
            { "log_statistics_saved", new Dictionary<Language, string> { { Language.French, "Statistiques enregistrées: {0} fichiers, {1}" }, { Language.English, "Statistics saved: {0} files, {1}" } } },
            
            // Log messages - File operations
            { "log_open_folder", new Dictionary<Language, string> { { Language.French, "Ouverture dossier: {0}" }, { Language.English, "Opening folder: {0}" } } },
            { "log_open_file", new Dictionary<Language, string> { { Language.French, "Ouverture fichier: {0}" }, { Language.English, "Opening file: {0}" } } },
            { "log_open_error", new Dictionary<Language, string> { { Language.French, "Erreur lors de l'ouverture: {0}" }, { Language.English, "Error opening: {0}" } } },
            { "log_cannot_open", new Dictionary<Language, string> { { Language.French, "Impossible d'ouvrir l'élément: {0}" }, { Language.English, "Cannot open item: {0}" } } },
            { "log_open_item_error", new Dictionary<Language, string> { { Language.French, "Ouvrir élément: {0}" }, { Language.English, "Open item: {0}" } } },
            { "log_copy_path_error", new Dictionary<Language, string> { { Language.French, "Copier chemin: {0}" }, { Language.English, "Copy path: {0}" } } },
            { "log_ignore_item_error", new Dictionary<Language, string> { { Language.French, "Ignorer élément: {0}" }, { Language.English, "Ignore item: {0}" } } },
            
            // Log messages - Analysis
            { "log_analysis_start", new Dictionary<Language, string> { { Language.French, "Démarrage de l'analyse du dossier : {0}" }, { Language.English, "Starting folder analysis: {0}" } } },
            { "log_analysis_finished", new Dictionary<Language, string> { { Language.French, "Analyse terminée : {0} fichiers, {1}" }, { Language.English, "Analysis finished: {0} files, {1}" } } },
            { "log_analysis_error", new Dictionary<Language, string> { { Language.French, "Erreur lors de l'analyse : {0}" }, { Language.English, "Analysis error: {0}" } } },
            
            // Log messages - Duplicates
            { "log_duplicate_search", new Dictionary<Language, string> { { Language.French, "Recherche de doublons dans : {0}" }, { Language.English, "Searching for duplicates in: {0}" } } },
            { "log_no_duplicates", new Dictionary<Language, string> { { Language.French, "Aucun doublon trouvé" }, { Language.English, "No duplicates found" } } },
            { "log_duplicates_found", new Dictionary<Language, string> { { Language.French, "{0} groupes de doublons trouvés, {1} récupérables" }, { Language.English, "{0} duplicate groups found, {1} recoverable" } } },
            { "log_duplicate_error", new Dictionary<Language, string> { { Language.French, "Erreur lors de la recherche : {0}" }, { Language.English, "Search error: {0}" } } },
            
            // Log messages - Statistics
            { "log_stats_empty", new Dictionary<Language, string> { { Language.French, "Statistiques vides" }, { Language.English, "Statistics empty" } } },
            { "log_stats_displayed", new Dictionary<Language, string> { { Language.French, "Statistiques affichées (rapport HTML)" }, { Language.English, "Statistics displayed (HTML report)" } } },
            { "log_stats_error", new Dictionary<Language, string> { { Language.French, "Erreur statistiques : {0}" }, { Language.English, "Statistics error: {0}" } } },
            
            // Log messages - System optimization
            { "log_restore_point_created", new Dictionary<Language, string> { { Language.French, "Point de restauration créé manuellement" }, { Language.English, "Restore point created manually" } } },
            { "log_restore_point_error", new Dictionary<Language, string> { { Language.French, "Erreur point de restauration : {0}" }, { Language.English, "Restore point error: {0}" } } },
            { "log_optimization_start", new Dictionary<Language, string> { { Language.French, "Démarrage des optimisations système" }, { Language.English, "Starting system optimizations" } } },
            { "log_optimization_stats", new Dictionary<Language, string> { { Language.French, "Statistiques d'optimisation sauvegardées (TRIM: {0}, SMART: {1})" }, { Language.English, "Optimization statistics saved (TRIM: {0}, SMART: {1})" } } },
            { "log_optimization_finished", new Dictionary<Language, string> { { Language.French, "Optimisations système terminées" }, { Language.English, "System optimizations finished" } } },
            { "log_optimization_error", new Dictionary<Language, string> { { Language.French, "Erreur optimisation : {0}" }, { Language.English, "Optimization error: {0}" } } },
            
            // Log messages - Warnings
            { "log_warning_cancelled", new Dictionary<Language, string> { { Language.French, "Opération avancée annulée par l'utilisateur (rapport)." }, { Language.English, "Advanced operation cancelled by user (report)." } } },
            { "log_warning_confirmation_refused", new Dictionary<Language, string> { { Language.French, "Opération annulée par l'utilisateur (confirmation refusée)" }, { Language.English, "Operation cancelled by user (confirmation refused)" } } },
            
            // Log messages - Cleaner operations
            { "log_clean_user_temp", new Dictionary<Language, string> { { Language.French, "Nettoyage du dossier temporaire utilisateur: {0}" }, { Language.English, "Cleaning user temp folder: {0}" } } },
            { "log_clean_local_temp", new Dictionary<Language, string> { { Language.French, "Nettoyage du dossier LocalAppData Temp: {0}" }, { Language.English, "Cleaning LocalAppData Temp folder: {0}" } } },
            { "log_clean_system_temp", new Dictionary<Language, string> { { Language.French, "Nettoyage du dossier Temp système: {0}" }, { Language.English, "Cleaning system Temp folder: {0}" } } },
            { "log_closing_browsers_history", new Dictionary<Language, string> { { Language.French, "Fermeture des navigateurs avant nettoyage de l'historique..." }, { Language.English, "Closing browsers before history cleanup..." } } },
            { "log_deleted_history", new Dictionary<Language, string> { { Language.French, "Supprimé historique {0}: {1}" }, { Language.English, "Deleted {0} history: {1}" } } },
            { "log_cleaned_sessions", new Dictionary<Language, string> { { Language.French, "Nettoyé sessions {0}: {1}" }, { Language.English, "Cleaned {0} sessions: {1}" } } },
            { "log_cleaned_firefox_sessions", new Dictionary<Language, string> { { Language.French, "Nettoyé sessions Firefox: {0}" }, { Language.English, "Cleaned Firefox sessions: {0}" } } },
            { "log_deleted_firefox_session", new Dictionary<Language, string> { { Language.French, "Supprimé session Firefox: {0}" }, { Language.English, "Deleted Firefox session: {0}" } } },
            { "log_deleted_firefox_history", new Dictionary<Language, string> { { Language.French, "Supprimé historique Firefox: {0}" }, { Language.English, "Deleted Firefox history: {0}" } } },
            { "log_clean_chrome_cache", new Dictionary<Language, string> { { Language.French, "Nettoyage cache Chrome: {0}" }, { Language.English, "Cleaning Chrome cache: {0}" } } },
            { "log_clean_edge_cache", new Dictionary<Language, string> { { Language.French, "Nettoyage cache Edge: {0}" }, { Language.English, "Cleaning Edge cache: {0}" } } },
            { "log_clean_firefox_cache", new Dictionary<Language, string> { { Language.French, "Nettoyage cache Firefox: {0}" }, { Language.English, "Cleaning Firefox cache: {0}" } } },
            { "log_clean_windows_update", new Dictionary<Language, string> { { Language.French, "Nettoyage SoftwareDistribution\\Download: {0}" }, { Language.English, "Cleaning SoftwareDistribution\\Download: {0}" } } },
            { "log_clean_thumbnails", new Dictionary<Language, string> { { Language.French, "Nettoyage vignettes: {0}" }, { Language.English, "Cleaning thumbnails: {0}" } } },
            { "log_deleted_thumbnail", new Dictionary<Language, string> { { Language.French, "Supprimé vignette: {0}" }, { Language.English, "Deleted thumbnail: {0}" } } },
            { "log_cannot_delete_thumbnail", new Dictionary<Language, string> { { Language.French, "Impossible de supprimer vignette {0}: {1}" }, { Language.English, "Cannot delete thumbnail {0}: {1}" } } },
            { "log_clean_prefetch", new Dictionary<Language, string> { { Language.French, "Nettoyage Prefetch: {0}" }, { Language.English, "Cleaning Prefetch: {0}" } } },
            { "log_system_logs_disabled", new Dictionary<Language, string> { { Language.French, "⚠️ Nettoyage des journaux système désactivé (risque de crash système)" }, { Language.English, "⚠️ System logs cleanup disabled (system crash risk)" } } },
            { "log_installer_cache_disabled", new Dictionary<Language, string> { { Language.French, "⚠️ Nettoyage du cache installeur désactivé (risque de crash/dysfonctionnement)" }, { Language.English, "⚠️ Installer cache cleanup disabled (crash/malfunction risk)" } } },
            { "log_app_logs_disabled", new Dictionary<Language, string> { { Language.French, "⚠️ Nettoyage des journaux applications désactivé (risque de dysfonctionnement)" }, { Language.English, "⚠️ Application logs cleanup disabled (malfunction risk)" } } },
            { "log_clean_orphaned_files", new Dictionary<Language, string> { { Language.French, "Nettoyage fichiers orphelins: {0}" }, { Language.English, "Cleaning orphaned files: {0}" } } },
            { "log_deleted_orphaned_file", new Dictionary<Language, string> { { Language.French, "Supprimé fichier orphelin: {0}" }, { Language.English, "Deleted orphaned file: {0}" } } },
            { "log_cannot_delete_file", new Dictionary<Language, string> { { Language.French, "Impossible de supprimer {0}: {1}" }, { Language.English, "Cannot delete {0}: {1}" } } },
            { "log_cleaning_cancelled_user", new Dictionary<Language, string> { { Language.French, "Nettoyage annulé par l'utilisateur" }, { Language.English, "Cleanup cancelled by user" } } },
            { "log_clean_memory_cache", new Dictionary<Language, string> { { Language.French, "Nettoyage du cache mémoire système..." }, { Language.English, "Cleaning system memory cache..." } } },
            { "log_memory_cache_cleaned", new Dictionary<Language, string> { { Language.French, "Cache mémoire nettoyé avec succès" }, { Language.English, "Memory cache cleaned successfully" } } },
            { "log_memory_cache_dryrun", new Dictionary<Language, string> { { Language.French, "(dry-run) Cache mémoire non nettoyé" }, { Language.English, "(dry-run) Memory cache not cleaned" } } },
            { "log_ssd_optimization", new Dictionary<Language, string> { { Language.French, "Optimisation SSD en cours..." }, { Language.English, "SSD optimization in progress..." } } },
            { "log_disk_health_check", new Dictionary<Language, string> { { Language.French, "Vérification santé disque (SMART)..." }, { Language.English, "Checking disk health (SMART)..." } } },
            { "log_smart_check_success", new Dictionary<Language, string> { { Language.French, "Vérification SMART effectuée avec succès" }, { Language.English, "SMART check completed successfully" } } },
            { "log_flush_dns", new Dictionary<Language, string> { { Language.French, "Exécution flush DNS..." }, { Language.English, "Executing DNS flush..." } } },
            { "log_flush_dns_dryrun", new Dictionary<Language, string> { { Language.French, "(dry-run) ipconfig /flushdns non exécuté" }, { Language.English, "(dry-run) ipconfig /flushdns not executed" } } },
            { "log_empty_recycle_bin", new Dictionary<Language, string> { { Language.French, "Vidage de la Corbeille..." }, { Language.English, "Emptying Recycle Bin..." } } },
            { "log_recycle_bin_dryrun", new Dictionary<Language, string> { { Language.French, "(dry-run) Corbeille non vidée" }, { Language.English, "(dry-run) Recycle Bin not emptied" } } },
            
            // Error messages - Generic pattern "Erreur X: {0}"
            { "error_cleaning", new Dictionary<Language, string> { { Language.French, "Erreur nettoyage {0}: {1}" }, { Language.English, "Cleaning error {0}: {1}" } } },
            { "error_scan", new Dictionary<Language, string> { { Language.French, "Erreur scan {0}: {1}" }, { Language.English, "Scan error {0}: {1}" } } },
            { "error_unhandled", new Dictionary<Language, string> { { Language.French, "Exception non gérée: {0}" }, { Language.English, "Unhandled exception: {0}" } } },
            { "error_stack_trace", new Dictionary<Language, string> { { Language.French, "Stack trace: {0}" }, { Language.English, "Stack trace: {0}" } } },
            { "error_cannot_delete", new Dictionary<Language, string> { { Language.French, "Impossible de supprimer {0}: {1}" }, { Language.English, "Cannot delete {0}: {1}" } } },
            { "error_reading_file", new Dictionary<Language, string> { { Language.French, "Erreur lecture {0}: {1}" }, { Language.English, "Error reading {0}: {1}" } } },
            
            // Update Manager messages
            { "update_cannot_check", new Dictionary<Language, string> { { Language.French, "⚠️ Impossible de vérifier les mises à jour (HTTP {0})" }, { Language.English, "⚠️ Cannot check for updates (HTTP {0})" } } },
            { "update_no_version", new Dictionary<Language, string> { { Language.French, "⚠️ Aucune version disponible sur GitHub" }, { Language.English, "⚠️ No version available on GitHub" } } },
            { "update_network_error", new Dictionary<Language, string> { { Language.French, "❌ Erreur réseau lors de la vérification : {0}" }, { Language.English, "❌ Network error during check: {0}" } } },
            { "update_error", new Dictionary<Language, string> { { Language.French, "❌ Erreur lors de la vérification : {0}" }, { Language.English, "❌ Error during check: {0}" } } },
            { "update_no_download", new Dictionary<Language, string> { { Language.French, "❌ Aucun fichier de téléchargement disponible" }, { Language.English, "❌ No download file available" } } },
            { "update_downloading", new Dictionary<Language, string> { { Language.French, "📥 Téléchargement de la mise à jour depuis : {0}" }, { Language.English, "📥 Downloading update from: {0}" } } },
            { "update_downloaded", new Dictionary<Language, string> { { Language.French, "✅ Téléchargement terminé : {0}" }, { Language.English, "✅ Download completed: {0}" } } },
            { "update_download_error", new Dictionary<Language, string> { { Language.French, "❌ Erreur lors du téléchargement : {0}" }, { Language.English, "❌ Download error: {0}" } } },
            { "update_page_opened", new Dictionary<Language, string> { { Language.French, "🌐 Page de release ouverte : {0}" }, { Language.English, "🌐 Release page opened: {0}" } } },
            { "update_cannot_open_browser", new Dictionary<Language, string> { { Language.French, "❌ Impossible d'ouvrir le navigateur : {0}" }, { Language.English, "❌ Cannot open browser: {0}" } } },
            
            // System optimization dialog and messages
            { "opt_dialog_title", new Dictionary<Language, string> { { Language.French, "Optimisations système" }, { Language.English, "System optimizations" } } },
            { "opt_dialog_available", new Dictionary<Language, string> { { Language.French, "OPTIMISATIONS SYSTÈME DISPONIBLES :" }, { Language.English, "AVAILABLE SYSTEM OPTIMIZATIONS:" } } },
            { "opt_trim_ssd", new Dictionary<Language, string> { { Language.French, "• TRIM SSD - Optimise les disques SSD" }, { Language.English, "• TRIM SSD - Optimize SSD drives" } } },
            { "opt_compact_registry", new Dictionary<Language, string> { { Language.French, "• Compaction Registre - Réduit la fragmentation" }, { Language.English, "• Registry Compaction - Reduce fragmentation" } } },
            { "opt_clean_memory", new Dictionary<Language, string> { { Language.French, "• Nettoyage Mémoire Cache - Libère la RAM" }, { Language.English, "• Memory Cache Cleanup - Free RAM" } } },
            { "opt_requires_admin", new Dictionary<Language, string> { { Language.French, "Ces opérations nécessitent des droits administrateur.\nLancer les optimisations maintenant ?" }, { Language.English, "These operations require administrator rights.\nStart optimizations now?" } } },
            { "opt_in_progress", new Dictionary<Language, string> { { Language.French, "Optimisations en cours..." }, { Language.English, "Optimizations in progress..." } } },
            { "opt_trim_progress", new Dictionary<Language, string> { { Language.French, "Optimisation SSD (TRIM)..." }, { Language.English, "SSD optimization (TRIM)..." } } },
            { "opt_smart_progress", new Dictionary<Language, string> { { Language.French, "Vérification santé disque (SMART)..." }, { Language.English, "Checking disk health (SMART)..." } } },
            { "opt_registry_progress", new Dictionary<Language, string> { { Language.French, "Compaction du registre..." }, { Language.English, "Registry compaction..." } } },
            { "opt_memory_progress", new Dictionary<Language, string> { { Language.French, "Nettoyage mémoire cache..." }, { Language.English, "Cleaning memory cache..." } } },
            { "opt_results_title", new Dictionary<Language, string> { { Language.French, "Résultats" }, { Language.English, "Results" } } },
            { "opt_results_completed", new Dictionary<Language, string> { { Language.French, "Optimisations terminées :" }, { Language.English, "Optimizations completed:" } } },
            { "opt_trim_result", new Dictionary<Language, string> { { Language.French, "TRIM SSD : {0}" }, { Language.English, "TRIM SSD: {0}" } } },
            { "opt_smart_result", new Dictionary<Language, string> { { Language.French, "Vérification SMART : {0}" }, { Language.English, "SMART Check: {0}" } } },
            { "opt_registry_result", new Dictionary<Language, string> { { Language.French, "Compaction Registre : {0}" }, { Language.English, "Registry Compaction: {0}" } } },
            { "opt_memory_result", new Dictionary<Language, string> { { Language.French, "Nettoyage Mémoire : {0}" }, { Language.English, "Memory Cleanup: {0}" } } },
            { "opt_success", new Dictionary<Language, string> { { Language.French, "✓ Succès" }, { Language.English, "✓ Success" } } },
            { "opt_failure", new Dictionary<Language, string> { { Language.French, "✗ Échec" }, { Language.English, "✗ Failed" } } },
            { "opt_profile_name", new Dictionary<Language, string> { { Language.French, "Optimisation Système" }, { Language.English, "System Optimization" } } },
            
            // SystemOptimizer messages
            { "sysopt_trim_running", new Dictionary<Language, string> { { Language.French, "Optimisation SSD (TRIM) en cours..." }, { Language.English, "SSD optimization (TRIM) running..." } } },
            { "sysopt_trim_executed", new Dictionary<Language, string> { { Language.French, "TRIM exécuté sur {0}" }, { Language.English, "TRIM executed on {0}" } } },
            { "sysopt_trim_error", new Dictionary<Language, string> { { Language.French, "Erreur TRIM {0}: {1}" }, { Language.English, "TRIM error {0}: {1}" } } },
            { "sysopt_trim_finished", new Dictionary<Language, string> { { Language.French, "Optimisation SSD terminée" }, { Language.English, "SSD optimization finished" } } },
            { "sysopt_trim_completed", new Dictionary<Language, string> { { Language.French, "Optimisation SSD (TRIM) terminée" }, { Language.English, "SSD optimization (TRIM) completed" } } },
            { "sysopt_cannot_start_defrag", new Dictionary<Language, string> { { Language.French, "Impossible de lancer defrag.exe" }, { Language.English, "Cannot start defrag.exe" } } },
            { "sysopt_registry_compacting", new Dictionary<Language, string> { { Language.French, "Compactage du registre Windows..." }, { Language.English, "Compacting Windows registry..." } } },
            { "sysopt_registry_scheduled", new Dictionary<Language, string> { { Language.French, "Compactage programmé pour le prochain redémarrage" }, { Language.English, "Compaction scheduled for next reboot" } } },
            { "sysopt_registry_completed", new Dictionary<Language, string> { { Language.French, "Compactage registre programmé" }, { Language.English, "Registry compaction scheduled" } } },
            { "sysopt_memory_clearing", new Dictionary<Language, string> { { Language.French, "Vidage du cache mémoire standby..." }, { Language.English, "Clearing standby memory cache..." } } },
            { "sysopt_memory_cleared", new Dictionary<Language, string> { { Language.French, "Cache mémoire vidé" }, { Language.English, "Memory cache cleared" } } },
            
            // HTML Report
            { "report_title", new Dictionary<Language, string> { { Language.French, "Rapport Statistiques - Windows Cleaner" }, { Language.English, "Statistics Report - Windows Cleaner" } } },
            { "report_generated", new Dictionary<Language, string> { { Language.French, "Généré le:" }, { Language.English, "Generated on:" } } },
            { "report_global_stats", new Dictionary<Language, string> { { Language.French, "Statistiques Globales" }, { Language.English, "Global Statistics" } } },
            { "report_space_freed", new Dictionary<Language, string> { { Language.French, "ESPACE TOTAL LIBÉRÉ" }, { Language.English, "TOTAL SPACE FREED" } } },
            { "report_files_deleted", new Dictionary<Language, string> { { Language.French, "FICHIERS SUPPRIMÉS" }, { Language.English, "FILES DELETED" } } },
            { "report_cleaning_sessions", new Dictionary<Language, string> { { Language.French, "SESSIONS DE NETTOYAGE" }, { Language.English, "CLEANING SESSIONS" } } },
            { "report_last_30_days", new Dictionary<Language, string> { { Language.French, "Derniers 30 Jours" }, { Language.English, "Last 30 Days" } } },
            { "report_cleanings", new Dictionary<Language, string> { { Language.French, "NETTOYAGES" }, { Language.English, "CLEANINGS" } } },
            { "report_app_cache_cleaning", new Dictionary<Language, string> { { Language.French, "Nettoyage des Caches Applicatifs" }, { Language.English, "Application Cache Cleaning" } } },
            { "report_global_stats_cache", new Dictionary<Language, string> { { Language.French, "Statistiques Globales" }, { Language.English, "Global Statistics" } } },
            { "report_total_cache_files", new Dictionary<Language, string> { { Language.French, "Total Fichiers Cache:" }, { Language.English, "Total Cache Files:" } } },
            { "report_details_by_source", new Dictionary<Language, string> { { Language.French, "Détails par Source" }, { Language.English, "Details by Source" } } },
            { "report_files", new Dictionary<Language, string> { { Language.French, "fichiers" }, { Language.English, "files" } } },
            { "report_games", new Dictionary<Language, string> { { Language.French, "Jeux (Steam/Epic):" }, { Language.English, "Games (Steam/Epic):" } } },
            { "report_ssd_optimization", new Dictionary<Language, string> { { Language.French, "Optimisation SSD" }, { Language.English, "SSD Optimization" } } },
            { "report_trim_optimizations", new Dictionary<Language, string> { { Language.French, "Optimisations TRIM:" }, { Language.English, "TRIM Optimizations:" } } },
            { "report_smart_checks", new Dictionary<Language, string> { { Language.French, "Vérifications SMART:" }, { Language.English, "SMART Checks:" } } },
            { "report_sessions", new Dictionary<Language, string> { { Language.French, "session(s)" }, { Language.English, "session(s)" } } },
            { "report_last_smart_report", new Dictionary<Language, string> { { Language.French, "Dernier Rapport SMART" }, { Language.English, "Last SMART Report" } } },
            { "report_session_history", new Dictionary<Language, string> { { Language.French, "Historique des Sessions" }, { Language.English, "Session History" } } },
            { "report_date", new Dictionary<Language, string> { { Language.French, "Date" }, { Language.English, "Date" } } },
            { "report_profile", new Dictionary<Language, string> { { Language.French, "Profil" }, { Language.English, "Profile" } } },
            { "report_duration", new Dictionary<Language, string> { { Language.French, "Durée" }, { Language.English, "Duration" } } },
            
            // Confirmation dialogs
            { "confirm_operations_title", new Dictionary<Language, string> { { Language.French, "Confirmer les opérations" }, { Language.English, "Confirm operations" } } },
            { "confirm_dangerous_operations", new Dictionary<Language, string> { { Language.French, "Vous êtes sur le point d'exécuter des opérations potentiellement dangereuses:" }, { Language.English, "You are about to perform potentially dangerous operations:" } } },
            { "confirm_continue", new Dictionary<Language, string> { { Language.French, "Continuer ?" }, { Language.English, "Continue?" } } },
            { "confirm_include_system_temp", new Dictionary<Language, string> { { Language.French, "- Inclure Temp système" }, { Language.English, "- Include System Temp" } } },
            { "confirm_clean_windows_update", new Dictionary<Language, string> { { Language.French, "- Nettoyer Windows Update (SoftwareDistribution\\Download)" }, { Language.English, "- Clean Windows Update (SoftwareDistribution\\Download)" } } },
            { "confirm_clean_prefetch", new Dictionary<Language, string> { { Language.French, "- Nettoyer Prefetch" }, { Language.English, "- Clean Prefetch" } } },
            { "confirm_empty_recycle_bin", new Dictionary<Language, string> { { Language.French, "- Vider la Corbeille" }, { Language.English, "- Empty Recycle Bin" } } },
            { "status_cancelled_confirmation", new Dictionary<Language, string> { { Language.French, "Annulé (confirmation)" }, { Language.English, "Cancelled (confirmation)" } } },
            
            // Cleaner log messages
            { "log_deleted", new Dictionary<Language, string> { { Language.French, "Supprimé: {0}" }, { Language.English, "Deleted: {0}" } } },
            { "log_deleted_folder", new Dictionary<Language, string> { { Language.French, "Supprimé dossier: {0}" }, { Language.English, "Deleted folder: {0}" } } },
            { "log_cannot_delete_entry", new Dictionary<Language, string> { { Language.French, "Impossible de supprimer {0}: {1}" }, { Language.English, "Cannot delete {0}: {1}" } } },
            { "log_error_listing", new Dictionary<Language, string> { { Language.French, "Erreur en listant {0}: {1}" }, { Language.English, "Error listing {0}: {1}" } } },
            { "log_dryrun_deletion_planned", new Dictionary<Language, string> { { Language.French, "(dry-run) Suppression planifiée: {0}" }, { Language.English, "(dry-run) Deletion planned: {0}" } } },
            { "log_file_locked", new Dictionary<Language, string> { { Language.French, "Fichier verrouillé (suppression au redémarrage): {0}" }, { Language.English, "File locked (deletion on reboot): {0}" } } },
            { "log_dryrun_folder_deletion", new Dictionary<Language, string> { { Language.French, "(dry-run) Suppression planifiée du dossier: {0}" }, { Language.English, "(dry-run) Folder deletion planned: {0}" } } },
            { "log_docker_cleaned", new Dictionary<Language, string> { { Language.French, "Docker nettoyé: {0}" }, { Language.English, "Docker cleaned: {0}" } } },
            { "log_node_modules_finished", new Dictionary<Language, string> { { Language.French, "Nettoyage node_modules terminé: {0} dossiers, {1} libérés" }, { Language.English, "node_modules cleanup finished: {0} folders, {1} freed" } } },
            { "log_node_modules_deleted", new Dictionary<Language, string> { { Language.French, "node_modules supprimé ({0}): {1}" }, { Language.English, "node_modules deleted ({0}): {1}" } } },
            { "log_visual_studio_finished", new Dictionary<Language, string> { { Language.French, "Nettoyage Visual Studio terminé: {0} libérés" }, { Language.English, "Visual Studio cleanup finished: {0} freed" } } },
            { "log_build_folder_deleted", new Dictionary<Language, string> { { Language.French, "Dossier build supprimé ({0}): {1}" }, { Language.English, "Build folder deleted ({0}): {1}" } } },
            { "log_python_finished", new Dictionary<Language, string> { { Language.French, "Nettoyage Python terminé: {0} éléments, {1} libérés" }, { Language.English, "Python cleanup finished: {0} items, {1} freed" } } },
            { "log_pyc_deleted", new Dictionary<Language, string> { { Language.French, "Fichier .pyc supprimé: {0}" }, { Language.English, ".pyc file deleted: {0}" } } },
            { "log_pycache_deleted", new Dictionary<Language, string> { { Language.French, "__pycache__ supprimé ({0}): {1}" }, { Language.English, "__pycache__ deleted ({0}): {1}" } } },
            { "log_git_finished", new Dictionary<Language, string> { { Language.French, "Nettoyage Git terminé: {0} repositories optimisés" }, { Language.English, "Git cleanup finished: {0} repositories optimized" } } },
            { "log_game_caches_cleaned", new Dictionary<Language, string> { { Language.French, "Caches jeux nettoyés: {0} fichiers supprimés" }, { Language.English, "Game caches cleaned: {0} files deleted" } } },
            { "log_git_repo_optimized", new Dictionary<Language, string> { { Language.French, "Git repository optimisé: {0}" }, { Language.English, "Git repository optimized: {0}" } } },
            { "log_recent_docs_cleaned", new Dictionary<Language, string> { { Language.French, "Documents récents nettoyés: {0} éléments" }, { Language.English, "Recent documents cleaned: {0} items" } } },
            { "log_timeline_cleaned", new Dictionary<Language, string> { { Language.French, "Timeline Windows nettoyée: {0} éléments" }, { Language.English, "Windows Timeline cleaned: {0} items" } } },
            { "log_shortcuts_verified", new Dictionary<Language, string> { { Language.French, "Vérification de {0} raccourcis dans {1}..." }, { Language.English, "Checking {0} shortcuts in {1}..." } } },
            { "log_shortcut_deleted", new Dictionary<Language, string> { { Language.French, "✓ Supprimé: {0}" }, { Language.English, "✓ Deleted: {0}" } } },
            { "log_dryrun_would_delete", new Dictionary<Language, string> { { Language.French, "(dry-run) Serait supprimé: {0}" }, { Language.English, "(dry-run) Would delete: {0}" } } },
            { "log_broken_shortcuts_found", new Dictionary<Language, string> { { Language.French, "Raccourcis cassés trouvés: {0} ({1})" }, { Language.English, "Broken shortcuts found: {0} ({1})" } } },
            { "log_cannot_verify_shortcut", new Dictionary<Language, string> { { Language.French, "Impossible de vérifier {0}: {1}" }, { Language.English, "Cannot verify {0}: {1}" } } },
            { "log_closing_browsers", new Dictionary<Language, string> { { Language.French, "Fermeture de {0} instance(s) de {1}..." }, { Language.English, "Closing {0} instance(s) of {1}..." } } },
            { "log_browsers_closed", new Dictionary<Language, string> { { Language.French, "✓ {0} navigateur(s) fermé(s) avec succès" }, { Language.English, "✓ {0} browser(s) closed successfully" } } },
            { "log_no_browsers_running", new Dictionary<Language, string> { { Language.French, "Aucun navigateur en cours d'exécution" }, { Language.English, "No browsers running" } } },
            
            // Ghost apps messages
            { "log_detecting_ghost_apps", new Dictionary<Language, string> { { Language.French, "Détection des applications fantômes en cours..." }, { Language.English, "Detecting ghost apps..." } } },
            { "log_ghost_apps_found", new Dictionary<Language, string> { { Language.French, "Applications fantômes trouvées: {0}" }, { Language.English, "Ghost apps found: {0}" } } },
            { "log_orphaned_folder_found", new Dictionary<Language, string> { { Language.French, "Dossier orphelin trouvé: {0}" }, { Language.English, "Orphaned folder found: {0}" } } },
            { "log_invalid_registry_found", new Dictionary<Language, string> { { Language.French, "Entrée registre invalide trouvée: {0}" }, { Language.English, "Invalid registry entry found: {0}" } } },
            { "log_removing_ghost_app", new Dictionary<Language, string> { { Language.French, "Suppression de l'application fantôme: {0}" }, { Language.English, "Removing ghost app: {0}" } } },
            { "log_removing_invalid_registry", new Dictionary<Language, string> { { Language.French, "Suppression de l'entrée registre invalide: {0}" }, { Language.English, "Removing invalid registry entry: {0}" } } },
            { "error_detecting_ghost_apps", new Dictionary<Language, string> { { Language.French, "Erreur lors de la détection des applications fantômes: {0}" }, { Language.English, "Error detecting ghost apps: {0}" } } },
            { "error_removing_ghost_app", new Dictionary<Language, string> { { Language.French, "Erreur suppression {0}: {1}" }, { Language.English, "Error removing {0}: {1}" } } },
            { "error_removing_registry_entry", new Dictionary<Language, string> { { Language.French, "Erreur suppression entrée registre {0}: {1}" }, { Language.English, "Error removing registry entry {0}: {1}" } } },
            
            // Empty folders messages
            { "log_detecting_empty_folders", new Dictionary<Language, string> { { Language.French, "Détection des dossiers vides en cours..." }, { Language.English, "Detecting empty folders..." } } },
            { "log_empty_folders_found", new Dictionary<Language, string> { { Language.French, "Dossiers vides trouvés: {0}" }, { Language.English, "Empty folders found: {0}" } } },
            { "log_removing_empty_folder", new Dictionary<Language, string> { { Language.French, "Suppression du dossier vide: {0}" }, { Language.English, "Removing empty folder: {0}" } } },
            { "error_detecting_empty_folders", new Dictionary<Language, string> { { Language.French, "Erreur lors de la détection des dossiers vides: {0}" }, { Language.English, "Error detecting empty folders: {0}" } } },
            { "error_removing_empty_folder", new Dictionary<Language, string> { { Language.French, "Erreur suppression {0}: {1}" }, { Language.English, "Error removing {0}: {1}" } } },
            
            // MessageBox titles and messages
            { "msgbox_information", new Dictionary<Language, string> { { Language.French, "Information" }, { Language.English, "Information" } } },
            { "msgbox_error", new Dictionary<Language, string> { { Language.French, "Erreur" }, { Language.English, "Error" } } },
            { "msgbox_warning", new Dictionary<Language, string> { { Language.French, "Avertissement" }, { Language.English, "Warning" } } },
            { "msgbox_confirm", new Dictionary<Language, string> { { Language.French, "Confirmer" }, { Language.English, "Confirm" } } },
            { "msgbox_confirmation", new Dictionary<Language, string> { { Language.French, "Confirmation" }, { Language.English, "Confirmation" } } },
            { "msgbox_success", new Dictionary<Language, string> { { Language.French, "Succès" }, { Language.English, "Success" } } },
            { "msgbox_result", new Dictionary<Language, string> { { Language.French, "Résultat" }, { Language.English, "Result" } } },
            { "msgbox_about", new Dictionary<Language, string> { { Language.French, "À propos" }, { Language.English, "About" } } },
            { "msgbox_export", new Dictionary<Language, string> { { Language.French, "Export" }, { Language.English, "Export" } } },
            { "msgbox_delete", new Dictionary<Language, string> { { Language.French, "Effacer" }, { Language.English, "Delete" } } },
            { "msgbox_statistics", new Dictionary<Language, string> { { Language.French, "Statistiques" }, { Language.English, "Statistics" } } },
            { "msgbox_profiles_available", new Dictionary<Language, string> { { Language.French, "Profils disponibles" }, { Language.English, "Available Profiles" } } },
            { "msgbox_scheduler", new Dictionary<Language, string> { { Language.French, "Planification" }, { Language.English, "Scheduler" } } },
            { "msgbox_analysis_complete", new Dictionary<Language, string> { { Language.French, "Analyse terminée" }, { Language.English, "Analysis Complete" } } },
            { "msgbox_duplicates_detected", new Dictionary<Language, string> { { Language.French, "Doublons détectés" }, { Language.English, "Duplicates Detected" } } },
            { "msgbox_read_logs", new Dictionary<Language, string> { { Language.French, "Lire les logs" }, { Language.English, "Read Logs" } } },
            
            // Log file messages
            { "msg_path_not_found", new Dictionary<Language, string> { { Language.French, "Chemin introuvable dans le message: {0}" }, { Language.English, "Path not found in message: {0}" } } },
            { "msg_no_path_detected", new Dictionary<Language, string> { { Language.French, "Aucun chemin de fichier ou dossier détecté dans ce message." }, { Language.English, "No file or folder path detected in this message." } } },
            { "msg_path_not_found_short", new Dictionary<Language, string> { { Language.French, "Chemin introuvable: {0}" }, { Language.English, "Path not found: {0}" } } },
            
            // Clear logs messages
            { "msg_clear_logs_confirm", new Dictionary<Language, string> { { Language.French, "Effacer les logs sur disque et dans l'interface ?" }, { Language.English, "Clear logs on disk and in interface?" } } },
            { "msg_logs_cleared", new Dictionary<Language, string> { { Language.French, "Logs effacés." }, { Language.English, "Logs cleared." } } },
            { "msg_cannot_clear_logs", new Dictionary<Language, string> { { Language.French, "Impossible d'effacer les logs: {0}" }, { Language.English, "Cannot clear logs: {0}" } } },
            { "msg_no_log_file", new Dictionary<Language, string> { { Language.French, "Aucun fichier de log trouvé." }, { Language.English, "No log file found." } } },
            { "msg_cannot_open_log", new Dictionary<Language, string> { { Language.French, "Impossible d'ouvrir le fichier de log: {0}" }, { Language.English, "Cannot open log file: {0}" } } },
            { "msg_logs_exported", new Dictionary<Language, string> { { Language.French, "Logs exportés vers {0}" }, { Language.English, "Logs exported to {0}" } } },
            { "msg_export_failed", new Dictionary<Language, string> { { Language.French, "Échec de l'export des logs" }, { Language.English, "Export failed" } } },
            
            // Statistics messages
            { "msg_no_statistics", new Dictionary<Language, string> { { Language.French, "Aucune statistique disponible. Effectuez un nettoyage pour commencer." }, { Language.English, "No statistics available. Perform a cleanup to start." } } },
            { "msg_cannot_generate_report", new Dictionary<Language, string> { { Language.French, "Impossible de générer le rapport HTML." }, { Language.English, "Cannot generate HTML report." } } },
            { "msg_report_generated", new Dictionary<Language, string> { { Language.French, "Rapport HTML généré et ouvert !\n\nEmplacement : {0}" }, { Language.English, "HTML report generated and opened!\n\nLocation: {0}" } } },
            { "msg_report_cannot_open", new Dictionary<Language, string> { { Language.French, "Rapport généré mais impossible de l'ouvrir automatiquement.\n\nEmplacement : {0}\n\nErreur : {1}" }, { Language.English, "Report generated but cannot open automatically.\n\nLocation: {0}\n\nError: {1}" } } },
            { "msg_stats_error", new Dictionary<Language, string> { { Language.French, "Erreur lors du chargement des statistiques :\n{0}" }, { Language.English, "Error loading statistics:\n{0}" } } },
            
            // Profile messages
            { "msg_profiles_info", new Dictionary<Language, string> { { Language.French, "PROFILS PRÉDÉFINIS :\n\n• Nettoyage Rapide - Usage quotidien rapide et sûr\n• Nettoyage Complet - Maintenance approfondie mensuelle\n• Nettoyage Développeur - Spécial projets de développement\n• Protection Vie Privée - Effacement des traces\n\nPour utiliser un profil, lancez l'application en ligne de commande :\nwindows-cleaner.exe --profile \"Nettoyage Rapide\"" }, { Language.English, "PREDEFINED PROFILES:\n\n• Quick Cleaning - Fast and safe daily use\n• Complete Cleaning - Thorough monthly maintenance\n• Developer Cleaning - Special development projects\n• Privacy Protection - Clear traces\n\nTo use a profile, launch the application from command line:\nwindows-cleaner.exe --profile \"Quick Cleaning\"" } } },
            
            // Scheduler messages
            { "msg_scheduler_info", new Dictionary<Language, string> { { Language.French, "PLANIFICATION DE TÂCHES\n\nPour planifier un nettoyage automatique, utilisez la ligne de commande :\n\nExemple - Nettoyage quotidien à 2h du matin :\nCréez une tâche Windows avec l'action :\n  windows-cleaner.exe --profile \"Nettoyage Rapide\" --silent\n\nCette fonctionnalité nécessite des droits administrateur." }, { Language.English, "TASK SCHEDULER\n\nTo schedule automatic cleanup, use command line:\n\nExample - Daily cleanup at 2 AM:\nCreate a Windows task with the action:\n  windows-cleaner.exe --profile \"Quick Cleaning\" --silent\n\nThis feature requires administrator rights." } } },
            
            // Backup/Restore point messages
            { "msg_restore_point_confirm", new Dictionary<Language, string> { { Language.French, "Créer un point de restauration système maintenant ?\n\nCela peut prendre quelques minutes." }, { Language.English, "Create a system restore point now?\n\nThis may take a few minutes." } } },
            { "msg_restore_point_success", new Dictionary<Language, string> { { Language.French, "Point de restauration créé avec succès !" }, { Language.English, "Restore point created successfully!" } } },
            { "msg_restore_point_failed", new Dictionary<Language, string> { { Language.French, "Échec de la création du point de restauration.\nVérifiez que la Protection Système est activée." }, { Language.English, "Failed to create restore point.\nCheck that System Protection is enabled." } } },
            { "msg_error_generic", new Dictionary<Language, string> { { Language.French, "Erreur :\n{0}" }, { Language.English, "Error:\n{0}" } } },
            
            // Disk analyzer messages
            { "msg_analysis_started", new Dictionary<Language, string> { { Language.French, "Analyse terminée" }, { Language.English, "Analysis complete" } } },
            { "msg_analysis_report_opened", new Dictionary<Language, string> { { Language.French, "Rapport HTML généré et ouvert !\n\nEmplacement : {0}" }, { Language.English, "HTML report generated and opened!\n\nLocation: {0}" } } },
            { "msg_analysis_error", new Dictionary<Language, string> { { Language.French, "Erreur lors de l'analyse :\n{0}" }, { Language.English, "Error during analysis:\n{0}" } } },
            
            // Duplicate finder messages
            { "msg_select_folder_duplicates", new Dictionary<Language, string> { { Language.French, "Sélectionnez le dossier où chercher les doublons" }, { Language.English, "Select folder to search for duplicates" } } },
            { "msg_no_duplicates_found", new Dictionary<Language, string> { { Language.French, "Aucun doublon trouvé !" }, { Language.English, "No duplicates found!" } } },
            { "msg_no_duplicates_report", new Dictionary<Language, string> { { Language.French, "Aucun doublon trouvé !\n\nRapport HTML généré : {0}" }, { Language.English, "No duplicates found!\n\nHTML report generated: {0}" } } },
            { "msg_duplicates_found", new Dictionary<Language, string> { { Language.French, "Rapport HTML généré et ouvert !\n\n{0} groupes de doublons détectés\nEspace récupérable : {1}\n\nEmplacement : {2}" }, { Language.English, "HTML report generated and opened!\n\n{0} duplicate groups detected\nRecoverable space: {1}\n\nLocation: {2}" } } },
        };

        public static string Get(string key)
        {
            if (_translations.TryGetValue(key, out var translations) && translations.TryGetValue(CurrentLanguage, out var text))
            {
                return text;
            }
            return key;
        }

        public static string Get(string key, params object[] args)
        {
            var text = Get(key);
            try
            {
                return string.Format(text, args);
            }
            catch
            {
                return text;
            }
        }

        public static void SetLanguage(Language language)
        {
            SetLanguage(language, true);
        }

        internal static void SetLanguage(Language language, bool saveToSettings)
        {
            CurrentLanguage = language;
            // Save to settings
            if (saveToSettings)
            {
                try
                {
                    var settings = SettingsManager.Load();
                    settings.Language = language.ToString();
                    SettingsManager.Save(settings);
                }
                catch { }
            }
        }

        public static void LoadLanguageFromSettings()
        {
            try
            {
                var settings = SettingsManager.Load();
                if (!string.IsNullOrEmpty(settings.Language) && Enum.TryParse<Language>(settings.Language, out var lang))
                {
                    CurrentLanguage = lang;
                }
            }
            catch { }
        }
    }
}
