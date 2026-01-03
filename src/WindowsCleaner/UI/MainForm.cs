using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsCleaner
{
    public class MainForm : Form
    {
        private MenuStrip menu = new MenuStrip();
        private ToolStripMenuItem fileMenu = null!;
        private ToolStripMenuItem exportLogsMenuItem = null!;
        private ToolStripMenuItem exitMenuItem = null!;

        private Button btnDryRun = null!;
        private Button btnClean = null!;
        private Button btnCancel = null!;
        private Button btnSelectAll = null!;
        private Button btnDeselectAll = null!;

        private CheckBox chkRecycle = null!;
        private CheckBox chkSystemTemp = null!;
        private CheckBox chkBrowsers = null!;
        private CheckBox chkWindowsUpdate = null!;
        private CheckBox chkBrowserHistory = null!;
        private CheckBox chkThumbnails = null!;
        private CheckBox chkPrefetch = null!;
        private CheckBox chkFlushDns = null!;
        private CheckBox chkVerbose = null!;
        private CheckBox chkAdvanced = null!;
        
        // Advanced cleaning options
        private CheckBox chkOrphanedFiles = null!;
        private CheckBox chkClearMemoryCache = null!;
        private CheckBox chkBrokenShortcuts = null!;
        private CheckBox chkGhostApps = null!;
        private CheckBox chkEmptyFolders = null!;
        
        // Developer cleaning options
        private CheckBox chkVsCodeCache = null!;
        private CheckBox chkNugetCache = null!;
        private CheckBox chkMavenCache = null!;
        private CheckBox chkNpmCache = null!;
        private CheckBox chkDockerCache = null!;
        private CheckBox chkNodeModules = null!;
        private CheckBox chkVisualStudio = null!;
        private CheckBox chkPythonCache = null!;
        private CheckBox chkGitCache = null!;
        private CheckBox chkGameCaches = null!;

        // Profile selection
        private ComboBox cmbProfiles = null!;
        private Label lblProfile = null!;

        private ListView lvLogs = null!;
        private ColoredProgressBar progressBar = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        private CancellationTokenSource? _cts;
        private bool _suppressLogs = false;
        private bool _logSubscribed = false;
        private Color _accentColor = Color.FromArgb(0, 120, 215);
        private bool _isDark = false;
        private List<CleaningProfile> _profiles = new List<CleaningProfile>();
        private bool _isApplyingProfile = false;
        private bool _suppressProfileEvent = false;
        private string CustomProfileLabel => LanguageManager.Get("profile_custom");

#pragma warning disable CS8774
        public MainForm()
        {
            // Charger la langue depuis les paramètres
            LanguageManager.LoadLanguageFromSettings();
            
            Text = LanguageManager.Get("window_title");
            Width = 1220;
            Height = 850;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;

            // Check if running as admin
            var isAdmin = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                MessageBox.Show(
                    LanguageManager.Get("warning_admin_message"),
                    LanguageManager.Get("warning_admin_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            InitializeComponents();
            LoadProfilesIntoCombo();
            Logger.Init();
            Logger.OnLog += Logger_OnLog;
            _logSubscribed = true;
            
            // Charger les paramètres sauvegardés
            LoadSavedOptions();
            
            // Vérifier les mises à jour au démarrage (asynchrone)
            _ = CheckForUpdatesAsync(silent: true);
        }
        
        private void LoadSavedOptions()
        {
            try
            {
                var settings = SettingsManager.Load();
                var preferredProfile = settings.SelectedProfileName;

                // Si un profil enregistré existe encore, l'appliquer
                // Essayer une correspondance exacte d'abord
                var matchingProfile = _profiles.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(preferredProfile) &&
                    p.Name.Equals(preferredProfile, StringComparison.OrdinalIgnoreCase));

                // Si pas de correspondance exacte, essayer de trouver le profil par type (indépendant de la langue)
                if (matchingProfile == null && !string.IsNullOrWhiteSpace(preferredProfile))
                {
                    matchingProfile = FindProfileByTranslatedName(preferredProfile);
                }

                if (matchingProfile != null)
                {
                    SetProfileSelection(matchingProfile.Name);
                    ApplyProfileToUi(matchingProfile);
                }
                else
                {
                    RestoreCheckboxesFromSettings(settings);
                    SetProfileSelection(string.IsNullOrWhiteSpace(preferredProfile) ? CustomProfileLabel : preferredProfile!);
                }
            }
            catch { /* Ignorer les erreurs de chargement */ }
        }
        
        private CleaningProfile? FindProfileByTranslatedName(string savedName)
        {
            // Mapper les noms traduits aux clés de profils
            var profileKeys = new[] { "profile_quick", "profile_complete", "profile_developer", "profile_privacy" };
            
            foreach (var key in profileKeys)
            {
                // Vérifier si le nom sauvegardé correspond à l'une des traductions (fr ou en)
                if (savedName.Equals(LanguageManager.Get(key), StringComparison.OrdinalIgnoreCase) ||
                    savedName.Equals(GetFrenchTranslation(key), StringComparison.OrdinalIgnoreCase) ||
                    savedName.Equals(GetEnglishTranslation(key), StringComparison.OrdinalIgnoreCase))
                {
                    // Retourner le profil avec le nom actuel
                    return _profiles.FirstOrDefault(p => p.Name.Equals(LanguageManager.Get(key), StringComparison.OrdinalIgnoreCase));
                }
            }
            
            return null;
        }
        
        private string GetFrenchTranslation(string key)
        {
            var currentLang = LanguageManager.CurrentLanguage;
            LanguageManager.SetLanguage(LanguageManager.Language.French, false);
            var translation = LanguageManager.Get(key);
            LanguageManager.SetLanguage(currentLang, false);
            return translation;
        }
        
        private string GetEnglishTranslation(string key)
        {
            var currentLang = LanguageManager.CurrentLanguage;
            LanguageManager.SetLanguage(LanguageManager.Language.English, false);
            var translation = LanguageManager.Get(key);
            LanguageManager.SetLanguage(currentLang, false);
            return translation;
        }
        
        private void SaveOptions()
        {
            try
            {
                // Charger les settings existants pour préserver la langue et autres paramètres
                var settings = SettingsManager.Load();
                settings.CleanRecycleBin = chkRecycle.Checked;
                settings.CleanSystemTemp = chkSystemTemp.Checked;
                settings.CleanBrowsers = chkBrowsers.Checked;
                settings.CleanBrowserHistory = chkBrowserHistory.Checked;
                settings.CleanWindowsUpdate = chkWindowsUpdate.Checked;
                settings.CleanThumbnails = chkThumbnails.Checked;
                settings.CleanPrefetch = chkPrefetch.Checked;
                settings.FlushDns = chkFlushDns.Checked;
                settings.Verbose = chkVerbose.Checked;
                settings.Advanced = chkAdvanced.Checked;
                settings.CleanOrphanedFiles = chkOrphanedFiles.Checked;
                settings.ClearMemoryCache = chkClearMemoryCache.Checked;
                settings.CleanBrokenShortcuts = chkBrokenShortcuts.Checked;
                settings.CleanGhostApps = chkGhostApps.Checked;
                settings.SelectedProfileName = cmbProfiles?.SelectedItem?.ToString() ?? CustomProfileLabel;
                SettingsManager.Save(settings);
            }
            catch { /* Ignorer les erreurs de sauvegarde */ }
        }

        private void RestoreCheckboxesFromSettings(AppSettings settings)
        {
            if (settings.CleanRecycleBin.HasValue) chkRecycle.Checked = settings.CleanRecycleBin.Value;
            if (settings.CleanSystemTemp.HasValue) chkSystemTemp.Checked = settings.CleanSystemTemp.Value;
            if (settings.CleanBrowsers.HasValue) chkBrowsers.Checked = settings.CleanBrowsers.Value;
            if (settings.CleanBrowserHistory.HasValue) chkBrowserHistory.Checked = settings.CleanBrowserHistory.Value;
            if (settings.CleanWindowsUpdate.HasValue) chkWindowsUpdate.Checked = settings.CleanWindowsUpdate.Value;
            if (settings.CleanThumbnails.HasValue) chkThumbnails.Checked = settings.CleanThumbnails.Value;
            if (settings.CleanPrefetch.HasValue) chkPrefetch.Checked = settings.CleanPrefetch.Value;
            if (settings.FlushDns.HasValue) chkFlushDns.Checked = settings.FlushDns.Value;
            if (settings.Verbose.HasValue) chkVerbose.Checked = settings.Verbose.Value;
            if (settings.Advanced.HasValue) chkAdvanced.Checked = settings.Advanced.Value;
            if (settings.CleanOrphanedFiles.HasValue) chkOrphanedFiles.Checked = settings.CleanOrphanedFiles.Value;
            if (settings.ClearMemoryCache.HasValue) chkClearMemoryCache.Checked = settings.ClearMemoryCache.Value;
            if (settings.CleanBrokenShortcuts.HasValue) chkBrokenShortcuts.Checked = settings.CleanBrokenShortcuts.Value;
            if (settings.CleanGhostApps.HasValue) chkGhostApps.Checked = settings.CleanGhostApps.Value;
        }

        private void LoadProfilesIntoCombo()
        {
            _profiles = ProfileManager.GetAllProfiles();

            if (cmbProfiles == null)
                return;

            _suppressProfileEvent = true;
            try
            {
                cmbProfiles.Items.Clear();
                cmbProfiles.Items.Add(CustomProfileLabel);
                foreach (var profile in _profiles)
                    cmbProfiles.Items.Add(profile.Name);

                if (cmbProfiles.Items.Count > 0)
                    cmbProfiles.SelectedIndex = 0;
            }
            finally
            {
                _suppressProfileEvent = false;
            }
        }

        private void SetProfileSelection(string profileName)
        {
            if (cmbProfiles == null) return;

            _suppressProfileEvent = true;
            try
            {
                if (cmbProfiles.Items.Contains(profileName))
                    cmbProfiles.SelectedItem = profileName;
                else if (cmbProfiles.Items.Count > 0)
                    cmbProfiles.SelectedIndex = 0;
            }
            finally
            {
                _suppressProfileEvent = false;
            }
        }

        private CleaningProfile? GetSelectedProfile()
        {
            if (cmbProfiles == null || cmbProfiles.SelectedItem == null) return null;
            var selectedName = cmbProfiles.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(selectedName) || selectedName == CustomProfileLabel) return null;

            return _profiles.FirstOrDefault(p => p.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));
        }

        private void ApplyProfileToUi(CleaningProfile profile)
        {
            _isApplyingProfile = true;
            try
            {
                chkRecycle.Checked = profile.EmptyRecycleBin;
                chkSystemTemp.Checked = profile.IncludeSystemTemp;
                chkBrowsers.Checked = profile.CleanBrowsers;
                chkBrowserHistory.Checked = profile.CleanBrowserHistory;
                chkWindowsUpdate.Checked = profile.CleanWindowsUpdate;
                chkThumbnails.Checked = profile.CleanThumbnails;
                chkPrefetch.Checked = profile.CleanPrefetch;
                chkFlushDns.Checked = profile.FlushDns;
                chkVerbose.Checked = profile.Verbose;
                chkOrphanedFiles.Checked = profile.CleanOrphanedFiles;
                chkClearMemoryCache.Checked = profile.ClearMemoryCache;
                chkBrokenShortcuts.Checked = profile.CleanBrokenShortcuts;
                chkGhostApps.Checked = profile.CleanGhostApps;
                
                // Developer options
                chkVsCodeCache.Checked = profile.CleanVsCodeCache;
                chkNugetCache.Checked = profile.CleanNugetCache;
                chkMavenCache.Checked = profile.CleanMavenCache;
                chkNpmCache.Checked = profile.CleanNpmCache;
                chkDockerCache.Checked = profile.CleanDocker;
                chkNodeModules.Checked = profile.CleanNodeModules;
                chkVisualStudio.Checked = profile.CleanVisualStudio;
                chkPythonCache.Checked = profile.CleanPythonCache;
                chkGitCache.Checked = profile.CleanGitCache;
                chkGameCaches.Checked = profile.CleanGameCaches;
            }
            finally
            {
                _isApplyingProfile = false;
            }

            SaveOptions();
        }

        private void CmbProfiles_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressProfileEvent) return;

            SaveOptions();

            var selectedProfile = GetSelectedProfile();
            if (selectedProfile == null)
                return;

            ApplyProfileToUi(selectedProfile);
            statusLabel.Text = LanguageManager.Get("status_profile_applied", selectedProfile.Name);
        }

        private void OnOptionChanged(object? sender, EventArgs e)
        {
            SaveOptions();

            if (_isApplyingProfile)
                return;

            if (cmbProfiles != null && cmbProfiles.SelectedItem != null && cmbProfiles.SelectedItem.ToString() != CustomProfileLabel)
                SetProfileSelection(CustomProfileLabel);
        }

        [MemberNotNull(nameof(menu), nameof(fileMenu), nameof(exportLogsMenuItem), nameof(exitMenuItem),
            nameof(btnDryRun), nameof(btnClean), nameof(btnCancel), nameof(btnSelectAll), nameof(btnDeselectAll),
            nameof(chkRecycle), nameof(chkSystemTemp),
            nameof(chkBrowsers), nameof(chkWindowsUpdate), nameof(chkThumbnails), nameof(chkPrefetch),
            nameof(chkFlushDns), nameof(chkBrowserHistory), nameof(chkVerbose), nameof(chkAdvanced), nameof(chkOrphanedFiles), nameof(chkClearMemoryCache), 
            nameof(chkBrokenShortcuts), nameof(chkVsCodeCache), nameof(chkNugetCache), nameof(chkMavenCache), 
            nameof(chkNpmCache), nameof(chkDockerCache), nameof(chkNodeModules), nameof(chkVisualStudio),
            nameof(chkPythonCache), nameof(chkGitCache), nameof(chkGameCaches),
            nameof(cmbProfiles), nameof(lblProfile), nameof(lvLogs), nameof(progressBar), nameof(statusStrip), nameof(statusLabel))]
        private void InitializeComponents()
        {
            menu = new MenuStrip();
            fileMenu = new ToolStripMenuItem(LanguageManager.Get("menu_file"));
            var clearLogsMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_clear_logs"));
            var readLogsMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_read_logs"));
            exportLogsMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_export_logs"));
            exitMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_exit"));
            var viewMenu = new ToolStripMenuItem(LanguageManager.Get("menu_view"));
            var themeLight = new ToolStripMenuItem(LanguageManager.Get("menu_theme_light"));
            var themeDark = new ToolStripMenuItem(LanguageManager.Get("menu_theme_dark"));
            var accentBlue = new ToolStripMenuItem(LanguageManager.Get("menu_accent_blue"));
            var accentGreen = new ToolStripMenuItem(LanguageManager.Get("menu_accent_green"));
            var accentOrange = new ToolStripMenuItem(LanguageManager.Get("menu_accent_orange"));
            var toolsMenu = new ToolStripMenuItem(LanguageManager.Get("menu_tools"));
            var diskAnalyzerMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_disk_analyzer"));
            var duplicateFinderMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_duplicate_finder"));
            var statisticsMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_statistics"));
            var profilesMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_profiles"));
            var schedulerMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_scheduler"));
            var backupMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_backup"));
            var optimizerMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_optimizer"));
            
            var helpMenu = new ToolStripMenuItem(LanguageManager.Get("menu_help"));
            var checkUpdateMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_check_updates"));
            var languageMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_language"));
            var languageFrenchMenuItem = new ToolStripMenuItem(LanguageManager.Get("lang_french"));
            var languageEnglishMenuItem = new ToolStripMenuItem(LanguageManager.Get("lang_english"));
            var aboutMenuItem = new ToolStripMenuItem(LanguageManager.Get("menu_about"));
            
            checkUpdateMenuItem.Click += CheckUpdateMenuItem_Click;
            languageFrenchMenuItem.Click += (s, e) => ChangeLanguage(LanguageManager.Language.French);
            languageEnglishMenuItem.Click += (s, e) => ChangeLanguage(LanguageManager.Language.English);
            languageMenuItem.DropDownItems.Add(languageFrenchMenuItem);
            languageMenuItem.DropDownItems.Add(languageEnglishMenuItem);
            aboutMenuItem.Click += AboutMenuItem_Click;
            diskAnalyzerMenuItem.Click += DiskAnalyzerMenuItem_Click;
            duplicateFinderMenuItem.Click += DuplicateFinderMenuItem_Click;
            statisticsMenuItem.Click += StatisticsMenuItem_Click;
            profilesMenuItem.Click += ProfilesMenuItem_Click;
            schedulerMenuItem.Click += SchedulerMenuItem_Click;
            backupMenuItem.Click += BackupMenuItem_Click;
            optimizerMenuItem.Click += OptimizerMenuItem_Click;
            clearLogsMenuItem.Click += ClearLogsMenuItem_Click;
            readLogsMenuItem.Click += ReadLogsMenuItem_Click;
            exportLogsMenuItem.Click += ExportLogsMenuItem_Click;
            exitMenuItem.Click += (s, e) => Close();
            
            fileMenu.DropDownItems.Add(clearLogsMenuItem);
            fileMenu.DropDownItems.Add(readLogsMenuItem);
            fileMenu.DropDownItems.Add(exportLogsMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);
            menu.Items.Add(fileMenu);

            // Affichage / thèmes
            themeLight.Click += (s, e) => ApplyTheme(isDark: false, accent: Color.FromArgb(0, 120, 215));
            themeDark.Click += (s, e) => ApplyTheme(isDark: true, accent: Color.FromArgb(0, 120, 215));
            accentBlue.Click += (s, e) => ApplyAccent(Color.FromArgb(0, 120, 215));
            accentGreen.Click += (s, e) => ApplyAccent(Color.FromArgb(0, 153, 51));
            accentOrange.Click += (s, e) => ApplyAccent(Color.FromArgb(255, 140, 0));
            viewMenu.DropDownItems.Add(themeLight);
            viewMenu.DropDownItems.Add(themeDark);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(accentBlue);
            viewMenu.DropDownItems.Add(accentGreen);
            viewMenu.DropDownItems.Add(accentOrange);
            menu.Items.Add(viewMenu);
            
            toolsMenu.DropDownItems.Add(diskAnalyzerMenuItem);
            toolsMenu.DropDownItems.Add(duplicateFinderMenuItem);
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add(statisticsMenuItem);
            toolsMenu.DropDownItems.Add(profilesMenuItem);
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add(schedulerMenuItem);
            toolsMenu.DropDownItems.Add(backupMenuItem);
            toolsMenu.DropDownItems.Add(optimizerMenuItem);
            menu.Items.Add(toolsMenu);

            // Uninstaller Menu
            var uninstallerMenu = new ToolStripMenuItem(LanguageManager.Get("menu_uninstaller"));
            var uninstallProgramsMenuItem = new ToolStripMenuItem("🗑️ " + LanguageManager.Get("uninstaller_title"));
            var uninstallExportMenuItem = new ToolStripMenuItem("📋 " + LanguageManager.Get("btn_export_csv"));
            
            uninstallProgramsMenuItem.Click += UninstallerMenuItem_Click;
            uninstallExportMenuItem.Click += (s, e) =>
            {
                var saveDialog = new SaveFileDialog
                {
                    FileName = $"programmes_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    Filter = "Fichiers CSV (*.csv)|*.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (ProgramUninstaller.ExportProgramsList(saveDialog.FileName))
                    {
                        MessageBox.Show(LanguageManager.Get("uninstaller_export_success", saveDialog.FileName), LanguageManager.Get("msgbox_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(LanguageManager.Get("uninstaller_export_error"), LanguageManager.Get("msgbox_result"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            uninstallerMenu.DropDownItems.Add(uninstallProgramsMenuItem);
            uninstallerMenu.DropDownItems.Add(new ToolStripSeparator());
            uninstallerMenu.DropDownItems.Add(uninstallExportMenuItem);
            menu.Items.Add(uninstallerMenu);
            
            // System Configuration Menu
            var sysConfigMenu = new ToolStripMenuItem(LanguageManager.Get("menu_sysconfig"));
            var sysConfigMenuItem = new ToolStripMenuItem("⚙️ " + LanguageManager.Get("sysconfig_title"));
            sysConfigMenuItem.Click += SysConfigMenuItem_Click;
            sysConfigMenu.DropDownItems.Add(sysConfigMenuItem);
            menu.Items.Add(sysConfigMenu);
            
            helpMenu.DropDownItems.Add(checkUpdateMenuItem);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(languageMenuItem);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(aboutMenuItem);
            menu.Items.Add(helpMenu);
            
            // Add copyright label on the right side of the menu
            var copyrightLabel = new ToolStripLabel("© 2025 easycoding.fr")
            {
                Alignment = ToolStripItemAlignment.Right,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8f)
            };
            menu.Items.Add(copyrightLabel);
            
            Controls.Add(menu);

            // GroupBox for actions and profiles - DESIGN AMÉLIORÉ
            var grpActions = new GroupBox() { Text = LanguageManager.Get("group_actions"), Left = 12, Top = 50, Width = 380, Height = 135, Padding = new Padding(0, 8, 0, 0) };
            lblProfile = new Label() { Text = LanguageManager.Get("label_profile"), Left = 15, Top = 22, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbProfiles = new ComboBox() { Left = 15, Top = 42, Width = 340, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            btnDryRun = new Button() { Text = LanguageManager.Get("btn_simulate"), Left = 15, Top = 80, Width = 105, Height = 40, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnClean = new Button() { Text = LanguageManager.Get("btn_clean"), Left = 125, Top = 80, Width = 105, Height = 40, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnSelectAll = new Button() { Text = LanguageManager.Get("btn_select_all"), Left = 235, Top = 80, Width = 60, Height = 40, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Tag = "selectall" };
            btnDeselectAll = new Button() { Text = LanguageManager.Get("btn_deselect_all"), Left = 300, Top = 80, Width = 60, Height = 40, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Tag = "deselectall" };
            btnCancel = new Button() { Text = LanguageManager.Get("btn_cancel"), Left = 12, Top = 80, Width = 356, Height = 40, Enabled = false, Visible = false, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            
            // Ajouter les infobulles pour les boutons de sélection
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnSelectAll, LanguageManager.Get("tooltip_select_all"));
            toolTip.SetToolTip(btnDeselectAll, LanguageManager.Get("tooltip_deselect_all"));
            toolTip.SetToolTip(btnDryRun, LanguageManager.Get("tooltip_simulate"));
            toolTip.SetToolTip(btnClean, LanguageManager.Get("tooltip_clean"));
            
            // Initialiser les couleurs des boutons de sélection (blanc par défaut)
            btnSelectAll.BackColor = Color.White;
            btnSelectAll.ForeColor = Color.Black;
            btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnDeselectAll.BackColor = Color.White;
            btnDeselectAll.ForeColor = Color.Black;
            btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            
            grpActions.Controls.Add(lblProfile);
            grpActions.Controls.Add(cmbProfiles);
            grpActions.Controls.Add(btnDryRun);
            grpActions.Controls.Add(btnClean);
            grpActions.Controls.Add(btnSelectAll);
            grpActions.Controls.Add(btnDeselectAll);
            grpActions.Controls.Add(btnCancel);
            btnCancel.BringToFront();
            Controls.Add(grpActions);

            // GroupBox for cleanup options - LARGEMENT AGRANDI
            var grpOptions = new GroupBox() { Text = LanguageManager.Get("group_standard"), Left = 400, Top = 50, Width = 780, Height = 95, Padding = new Padding(0, 8, 0, 0) };
            chkRecycle = new CheckBox() { Text = LanguageManager.Get("chk_recycle"), Left = 15, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkSystemTemp = new CheckBox() { Text = LanguageManager.Get("chk_system_temp"), Left = 205, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkBrowsers = new CheckBox() { Text = LanguageManager.Get("chk_browsers"), Left = 395, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkWindowsUpdate = new CheckBox() { Text = LanguageManager.Get("chk_windows_update"), Left = 585, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkThumbnails = new CheckBox() { Text = LanguageManager.Get("chk_thumbnails"), Left = 15, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkPrefetch = new CheckBox() { Text = LanguageManager.Get("chk_prefetch"), Left = 205, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkFlushDns = new CheckBox() { Text = LanguageManager.Get("chk_flush_dns"), Left = 395, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkBrowserHistory = new CheckBox() { Text = LanguageManager.Get("chk_browser_history"), Left = 585, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f), Checked = true };
            grpOptions.Controls.Add(chkRecycle);
            grpOptions.Controls.Add(chkSystemTemp);
            grpOptions.Controls.Add(chkBrowsers);
            grpOptions.Controls.Add(chkWindowsUpdate);
            grpOptions.Controls.Add(chkThumbnails);
            grpOptions.Controls.Add(chkPrefetch);
            grpOptions.Controls.Add(chkFlushDns);
            grpOptions.Controls.Add(chkBrowserHistory);
            Controls.Add(grpOptions);

            // Advanced options - MEILLEURE PRÉSENTATION
            var grpAdvanced = new GroupBox() { Text = LanguageManager.Get("group_advanced"), Left = 12, Top = 155, Width = 1168, Height = 135, Padding = new Padding(0, 8, 0, 0) };
            chkVerbose = new CheckBox() { Text = LanguageManager.Get("chk_verbose"), Left = 15, Top = 30, Width = 360, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkAdvanced = new CheckBox() { Text = LanguageManager.Get("chk_report"), Left = 390, Top = 30, Width = 360, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkOrphanedFiles = new CheckBox() { Text = LanguageManager.Get("chk_orphaned"), Left = 765, Top = 30, Width = 380, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkClearMemoryCache = new CheckBox() { Text = LanguageManager.Get("chk_memory_cache"), Left = 15, Top = 63, Width = 360, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkBrokenShortcuts = new CheckBox() { Text = LanguageManager.Get("chk_broken_shortcuts"), Left = 390, Top = 63, Width = 360, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkGhostApps = new CheckBox() { Text = LanguageManager.Get("chk_ghost_apps"), Left = 765, Top = 63, Width = 380, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkEmptyFolders = new CheckBox() { Text = LanguageManager.Get("chk_empty_folders"), Left = 15, Top = 96, Width = 360, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            grpAdvanced.Controls.Add(chkVerbose);
            grpAdvanced.Controls.Add(chkAdvanced);
            grpAdvanced.Controls.Add(chkOrphanedFiles);
            grpAdvanced.Controls.Add(chkClearMemoryCache);
            grpAdvanced.Controls.Add(chkBrokenShortcuts);
            grpAdvanced.Controls.Add(chkGhostApps);
            grpAdvanced.Controls.Add(chkEmptyFolders);
            Controls.Add(grpAdvanced);
            
            // Developer options - NOUVEAU GROUPE
            var grpDeveloper = new GroupBox() { Text = LanguageManager.Get("group_developer"), Left = 12, Top = 300, Width = 1168, Height = 100, Padding = new Padding(0, 8, 0, 0) };
            chkVsCodeCache = new CheckBox() { Text = LanguageManager.Get("chk_vscode"), Left = 15, Top = 25, Width = 145, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkNugetCache = new CheckBox() { Text = LanguageManager.Get("chk_nuget"), Left = 165, Top = 25, Width = 130, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkMavenCache = new CheckBox() { Text = LanguageManager.Get("chk_maven"), Left = 300, Top = 25, Width = 130, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkNpmCache = new CheckBox() { Text = LanguageManager.Get("chk_npm"), Left = 435, Top = 25, Width = 120, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkDockerCache = new CheckBox() { Text = LanguageManager.Get("chk_docker"), Left = 560, Top = 25, Width = 130, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkNodeModules = new CheckBox() { Text = LanguageManager.Get("chk_node_modules"), Left = 695, Top = 25, Width = 160, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkVisualStudio = new CheckBox() { Text = LanguageManager.Get("chk_visual_studio"), Left = 860, Top = 25, Width = 145, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkPythonCache = new CheckBox() { Text = LanguageManager.Get("chk_python"), Left = 1010, Top = 25, Width = 145, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkGitCache = new CheckBox() { Text = LanguageManager.Get("chk_git"), Left = 15, Top = 58, Width = 130, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            chkGameCaches = new CheckBox() { Text = LanguageManager.Get("chk_games"), Left = 150, Top = 58, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9f) };
            
            grpDeveloper.Controls.Add(chkVsCodeCache);
            grpDeveloper.Controls.Add(chkNugetCache);
            grpDeveloper.Controls.Add(chkMavenCache);
            grpDeveloper.Controls.Add(chkNpmCache);
            grpDeveloper.Controls.Add(chkDockerCache);
            grpDeveloper.Controls.Add(chkNodeModules);
            grpDeveloper.Controls.Add(chkVisualStudio);
            grpDeveloper.Controls.Add(chkPythonCache);
            grpDeveloper.Controls.Add(chkGitCache);
            grpDeveloper.Controls.Add(chkGameCaches);
            Controls.Add(grpDeveloper);

            // Logs GroupBox - AUGMENTÉ - Position ajustée
            var grpLogs = new GroupBox() { Text = LanguageManager.Get("group_logs"), Left = 12, Top = 410, Width = 1168, Height = 380, Padding = new Padding(0, 8, 0, 0) };
            lvLogs = new ListView() { Left = 8, Top = 30, Width = 1152, Height = 342, View = View.Details, FullRowSelect = true, Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.None };
            lvLogs.Columns.Add(LanguageManager.Get("log_time"), 160);
            lvLogs.Columns.Add(LanguageManager.Get("log_level"), 100);
            lvLogs.Columns.Add(LanguageManager.Get("log_message"), 892);
            grpLogs.Controls.Add(lvLogs);
            Controls.Add(grpLogs);

            // Improved ListView appearance: owner-draw, alternating rows, double-buffered
            lvLogs.OwnerDraw = true;
            lvLogs.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvLogs.DrawColumnHeader += LvLogs_DrawColumnHeader;
            lvLogs.DrawItem += LvLogs_DrawItem;
            lvLogs.DrawSubItem += LvLogs_DrawSubItem;
            // enable double buffering to reduce flicker
            try { typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(lvLogs, true, null); } catch { }
            // increase row height via a tiny ImageList hack
            try
            {
                var img = new ImageList();
                img.ImageSize = new Size(1, 22); // height of rows
                lvLogs.SmallImageList = img;
                lvLogs.Font = new Font("Segoe UI", 9);
            }
            catch { }

            progressBar = new ColoredProgressBar() { Left = 12, Top = 735, Width = 1168, Height = 32, BackColor = menu != null ? menu.BackColor : SystemColors.Control, ForeColor = this.ForeColor };
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.BarColor = _accentColor;
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("✓ Prêt - Sélectionnez les options et cliquez sur une action");
            statusLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            statusLabel.AutoSize = false;
            statusLabel.Width = 1350;
            statusLabel.ForeColor = Color.DarkGreen;
            statusStrip.Items.Add(statusLabel);

            Controls.Add(progressBar);
            Controls.Add(statusStrip);

            cmbProfiles.SelectedIndexChanged += CmbProfiles_SelectedIndexChanged;
            btnDryRun.Click += async (s, e) => await StartCleanerAsync(dryRun: true);
            btnClean.Click += async (s, e) => await StartCleanerAsync(dryRun: false);
            btnCancel.Click += (s, e) => Cancel();
            btnSelectAll.Click += (s, e) => SelectAllOptions(true);
            btnDeselectAll.Click += (s, e) => SelectAllOptions(false);
            
            // Double-clic sur les logs pour ouvrir les chemins
            lvLogs.DoubleClick += LvLogs_DoubleClick;
            
            // Event handlers pour sauvegarder les options à chaque changement
            foreach (Control c in Controls)
            {
                if (c is GroupBox gb)
                {
                    foreach (Control subC in gb.Controls)
                    {
                        if (subC is CheckBox chk)
                        {
                            chk.CheckedChanged += OnOptionChanged;
                            chk.CheckedChanged += (s, e) => UpdateSelectionButtons();
                        }
                    }
                }
            }
            
            // Sauvegarder les options à la fermeture
            FormClosing += (s, e) => SaveOptions();

            // button visual polish - STYLE MODERNE
            foreach (var b in new[] { btnDryRun, btnClean, btnCancel, btnSelectAll, btnDeselectAll })
            {
                if (b == null) continue;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 1;
                b.Cursor = Cursors.Hand;
                
                if (b == btnSelectAll || b == btnDeselectAll)
                {
                    b.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
                    b.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
                    b.BackColor = Color.White;
                    b.ForeColor = Color.Black;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
                    b.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 220, 220);
                }
                else
                {
                    b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    b.FlatAppearance.BorderSize = 0;
                    b.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 180);
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 230);
                    b.BackColor = _accentColor;
                    b.ForeColor = Color.White;
                }
            }
            
            // Mise à jour initiale des boutons de sélection
            UpdateSelectionButtons();

            // GroupBox styling - DESIGN ÉPURÉ
            var grpFont = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            foreach (Control c in Controls)
            {
                if (c is GroupBox gb)
                {
                    gb.Font = grpFont;
                    gb.ForeColor = _accentColor;
                    gb.Padding = new Padding(10);
                }
            }

            MainMenuStrip = menu;

            // apply default theme
            ApplyTheme(isDark: false, accent: _accentColor);
        }
#pragma warning restore CS8774

        private void ApplyAccent(Color accent)
        {
            _accentColor = accent;
            ApplyTheme(_isDark, _accentColor);
        }

        private void ApplyTheme(bool isDark, Color accent)
        {
            _isDark = isDark;
            _accentColor = accent;

            Color baseBack = isDark ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            Color controlBack = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color baseFore = isDark ? Color.WhiteSmoke : Color.Black;

            try
            {
                this.BackColor = baseBack;
                this.ForeColor = baseFore;

                if (menu != null)
                {
                    menu.BackColor = controlBack;
                    menu.ForeColor = baseFore;
                }

                if (statusStrip != null)
                {
                    statusStrip.BackColor = controlBack;
                    statusStrip.ForeColor = baseFore;
                }

                if (lvLogs != null)
                {
                    lvLogs.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
                    lvLogs.ForeColor = baseFore;
                }

                if (progressBar != null)
                {
                    try { progressBar.BackColor = controlBack; } catch { }
                    try { progressBar.ForeColor = accent; } catch { }
                }

                foreach (Control c in this.Controls)
                {
                    try
                    {
                        if (c is Button)
                        {
                            c.BackColor = accent;
                            c.ForeColor = Color.White;
                        }
                        else if (c is ListView)
                        {
                            // already set
                        }
                        else if (c is StatusStrip)
                        {
                            // already set
                        }
                        else
                        {
                            c.BackColor = controlBack;
                            c.ForeColor = baseFore;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void LvLogs_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (e == null || e.Header == null) return;
            try
            {
                using var b = new SolidBrush(menu?.BackColor ?? SystemColors.Control);
                e.Graphics.FillRectangle(b, e.Bounds);
                using var f = new SolidBrush(this.ForeColor);
                e.Graphics.DrawString(e.Header.Text, this.Font, f, e.Bounds);
            }
            catch { }
        }

        private void LvLogs_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // handled in DrawSubItem for details view
        }

        private void LvLogs_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e == null || e.Item == null || e.SubItem == null) return;
            try
            {
                var g = e.Graphics;
                int idx = e.Item.Index;
                bool selected = e.Item.Selected;

                Color bg;
                if (selected) bg = _accentColor;
                else if (idx % 2 == 0) bg = _isDark ? Color.FromArgb(37, 37, 38) : Color.White;
                else bg = _isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(250, 250, 250);

                using (var sb = new SolidBrush(bg)) g.FillRectangle(sb, e.Bounds);

                Color textCol;
                if (selected)
                {
                    textCol = Color.White;
                }
                else if (_isDark)
                {
                    // Dark theme: ignore inherited forecolors, use bright text
                    if (e.ColumnIndex == 0 || e.ColumnIndex == 1)
                    {
                        // Time and Level
                        textCol = Color.White;
                    }
                    else
                    {
                        textCol = Color.WhiteSmoke;
                    }
                }
                else
                {
                    // Light theme: respect per-item/subitem forecolor
                    textCol = (e.SubItem.ForeColor != Color.Empty) ? e.SubItem.ForeColor : Color.Black;
                }

                using var brush = new SolidBrush(textCol);
                var r = e.Bounds;
                r.Inflate(-4, 0);
                var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
                g.DrawString(e.SubItem.Text, this.Font, brush, r, sf);
            }
            catch { }
        }

        private void LvLogs_DoubleClick(object? sender, EventArgs e)
        {
            try
            {
                if (lvLogs.SelectedItems.Count == 0) return;
                
                var selectedItem = lvLogs.SelectedItems[0];
                if (selectedItem.SubItems.Count < 3) return;
                
                var message = selectedItem.SubItems[2].Text;
                
                // Extraire les chemins du message (entre guillemets ou après ":")
                var paths = System.Text.RegularExpressions.Regex.Matches(message, @"[A-Z]:\\[^\s""<>|?*]+");
                
                if (paths.Count > 0)
                {
                    var path = paths[0].Value;
                    
                    if (System.IO.Directory.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo("explorer.exe", '"' + path + '"') { UseShellExecute = true });
                        Logger.Log(LogLevel.Info, LanguageManager.Get("log_open_folder", path));
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"") { UseShellExecute = true });
                        Logger.Log(LogLevel.Info, LanguageManager.Get("log_open_file", path));
                    }
                    else
                    {
                        MessageBox.Show(LanguageManager.Get("msg_path_not_found", path), LanguageManager.Get("msgbox_information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(LanguageManager.Get("msg_no_path_detected"), LanguageManager.Get("msgbox_information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_open_error", ex.Message));
            }
        }

        private void Logger_OnLog(DateTime ts, LogLevel level, string message)
        {
            // Ignorer toute arrivée de log pendant/juste après annulation
            if (_suppressLogs) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendLog(ts, level, message)));
            }
            else
            {
                AppendLog(ts, level, message);
            }
        }

        private void AppendLog(DateTime ts, LogLevel level, string message)
        {
            if (_suppressLogs || (_cts?.IsCancellationRequested ?? false)) return;

            var lvi = new ListViewItem(ts.ToString("yyyy-MM-dd HH:mm:ss"));
            lvi.SubItems.Add(level.ToString());
            lvi.SubItems.Add(message);
            // color by level
            switch (level)
            {
                case LogLevel.Debug:
                    lvi.ForeColor = _isDark ? Color.Silver : Color.Gray;
                    break;
                case LogLevel.Info:
                    lvi.ForeColor = _isDark ? Color.Gainsboro : Color.Black;
                    break;
                case LogLevel.Warning:
                    lvi.ForeColor = _isDark ? Color.Khaki : Color.Orange;
                    break;
                case LogLevel.Error:
                    lvi.ForeColor = _isDark ? Color.LightCoral : Color.Red;
                    break;
            }
            lvLogs.Items.Add(lvi);
            // keep bottom visible
            if (lvLogs.Items.Count > 0) lvLogs.EnsureVisible(lvLogs.Items.Count - 1);
            statusLabel.Text = $"Logs: {lvLogs.Items.Count}";
        }

        private void ClearLogsMenuItem_Click(object? sender, EventArgs e)
        {
            var res = MessageBox.Show(LanguageManager.Get("msg_clear_logs_confirm"), LanguageManager.Get("msgbox_confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;
            try
            {
                Logger.Clear();
                lvLogs.Items.Clear();
                statusLabel.Text = LanguageManager.Get("msg_logs_cleared");
                MessageBox.Show(LanguageManager.Get("msg_logs_cleared"), LanguageManager.Get("msgbox_delete"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("msg_cannot_clear_logs", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadLogsMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var logFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "windows-cleaner.log");
                
                if (!System.IO.File.Exists(logFile))
                {
                    MessageBox.Show(LanguageManager.Get("msg_no_log_file"), LanguageManager.Get("msgbox_read_logs"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Ouvrir le fichier avec l'éditeur par défaut
                var psi = new ProcessStartInfo(logFile)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(psi);
                statusLabel.Text = "Fichier de log ouvert";
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("msg_cannot_open_log", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportLogsMenuItem_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog();
            sfd.Filter = "Fichier log|*.log|Texte|*.txt|Tous|*.*";
            sfd.FileName = "windows-cleaner.log";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var dest = Logger.Export(sfd.FileName);
                if (dest != null)
                    MessageBox.Show(LanguageManager.Get("msg_logs_exported", dest), LanguageManager.Get("msgbox_export"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(LanguageManager.Get("msg_export_failed"), LanguageManager.Get("msgbox_export"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeLanguage(LanguageManager.Language newLanguage)
        {
            if (LanguageManager.CurrentLanguage == newLanguage)
                return;

            LanguageManager.SetLanguage(newLanguage);
            
            MessageBox.Show(
                LanguageManager.Get("language_changed_message"),
                LanguageManager.Get("language_changed_title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            
            // Redémarrer l'application
            Application.Restart();
            Environment.Exit(0);
        }

        private void AboutMenuItem_Click(object? sender, EventArgs e)
        {
            // Create custom About dialog with clickable links
            var aboutForm = new Form
            {
                Text = LanguageManager.Get("msgbox_about"),
                Size = new Size(500, 550),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            int yPos = 10;

            // Title
            var lblTitle = new Label
            {
                Text = "Windows Cleaner",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);
            yPos += 40;

            // Version
            var lblVersion = new Label
            {
                Text = "Version: 2.0.4 - Program Uninstaller Edition",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            panel.Controls.Add(lblVersion);
            yPos += 30;

            // Author label
            var lblAuthor = new Label
            {
                Text = "Auteur : ",
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblAuthor);

            // Author link
            var linkAuthor = new LinkLabel
            {
                Text = "easycoding.fr",
                Location = new Point(lblAuthor.Right, yPos),
                AutoSize = true
            };
            linkAuthor.LinkClicked += (s, ev) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://easycoding.fr",
                        UseShellExecute = true
                    });
                }
                catch { }
            };
            panel.Controls.Add(linkAuthor);
            yPos += 40;

            // New Features in v2.0.4
            var lblFeatures = new Label
            {
                Text = "🆕 Nouveau dans v2.0.4 :",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblFeatures);
            yPos += 30;

            var lblFeatures1 = new Label
            {
                Text = "• 🗑️ Désinstallateur de programmes complet",
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblFeatures1);
            yPos += 25;

            var lblFeatures2 = new Label
            {
                Text = "• Interface professionnelle avec recherche temps réel",
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblFeatures2);
            yPos += 25;

            var lblFeatures3 = new Label
            {
                Text = "• Export CSV de la liste des programmes",
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblFeatures3);
            yPos += 25;

            var lblFeatures4 = new Label
            {
                Text = "• Nettoyage complet registre + AppData",
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblFeatures4);
            yPos += 40;

            // License title
            var lblLicenseTitle = new Label
            {
                Text = "Licence : MIT",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblLicenseTitle);
            yPos += 30;

            // License text with clickable link
            var txtLicense = new RichTextBox
            {
                Location = new Point(10, yPos),
                Size = new Size(440, 260),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = panel.BackColor,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = @"MIT License

Copyright (c) 2025 easycoding.fr

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
            };
            
            // Make the link in RichTextBox clickable
            int linkStart = txtLicense.Text.IndexOf("easycoding.fr");
            if (linkStart >= 0)
            {
                txtLicense.Select(linkStart, "easycoding.fr".Length);
                txtLicense.SelectionColor = Color.Blue;
                txtLicense.Select(0, 0);
            }
            
            txtLicense.LinkClicked += (s, ev) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://easycoding.fr",
                        UseShellExecute = true
                    });
                }
                catch { }
            };
            
            panel.Controls.Add(txtLicense);
            yPos += 270;

            // OK button
            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(190, yPos),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            panel.Controls.Add(btnOk);

            aboutForm.Controls.Add(panel);
            aboutForm.AcceptButton = btnOk;
            aboutForm.ShowDialog(this);
        }

        private void Cancel()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _suppressLogs = true; // arrêter le journal visuel
                _cts.Cancel();
                btnCancel.Enabled = false; // éviter double clic
                statusLabel.Text = "Annulation en cours...";

                // Désabonner temporairement le logger pour bloquer toute arrivée
                if (_logSubscribed)
                {
                    Logger.OnLog -= Logger_OnLog;
                    _logSubscribed = false;
                }
            }
        }

        private void SelectAllOptions(bool select)
        {
            _suppressProfileEvent = true; // Éviter de déclencher des événements de profil
            
            // Options standard
            chkRecycle.Checked = select;
            chkSystemTemp.Checked = select;
            chkBrowsers.Checked = select;
            chkBrowserHistory.Checked = select;
            chkWindowsUpdate.Checked = select;
            chkThumbnails.Checked = select;
            chkPrefetch.Checked = select;
            chkFlushDns.Checked = select;
            
            // Options avancées
            chkOrphanedFiles.Checked = select;
            chkClearMemoryCache.Checked = select;
            chkBrokenShortcuts.Checked = select;
            chkGhostApps.Checked = select;
            chkEmptyFolders.Checked = select;
            
            // Options développeur
            chkVsCodeCache.Checked = select;
            chkNugetCache.Checked = select;
            chkMavenCache.Checked = select;
            chkNpmCache.Checked = select;
            chkDockerCache.Checked = select;
            chkNodeModules.Checked = select;
            chkVisualStudio.Checked = select;
            chkPythonCache.Checked = select;
            chkGitCache.Checked = select;
            chkGameCaches.Checked = select;
            
            // Note: on ne touche pas chkVerbose et chkAdvanced car ce sont des options de comportement, pas de nettoyage
            
            _suppressProfileEvent = false;
            
            // Mettre le profil sur "Personnalisé"
            if (cmbProfiles.Items.Contains(CustomProfileLabel))
            {
                cmbProfiles.SelectedItem = CustomProfileLabel;
            }
            
            SaveOptions();
        }

        private void UpdateSelectionButtons()
        {
            // Vérifier si toutes les options de nettoyage sont cochées
            bool allChecked = chkRecycle.Checked && chkSystemTemp.Checked && chkBrowsers.Checked &&
                             chkBrowserHistory.Checked && chkWindowsUpdate.Checked && chkThumbnails.Checked && chkPrefetch.Checked &&
                             chkFlushDns.Checked && chkOrphanedFiles.Checked && chkClearMemoryCache.Checked &&
                             chkBrokenShortcuts.Checked && chkGhostApps.Checked && chkEmptyFolders.Checked &&
                             chkVsCodeCache.Checked && chkNugetCache.Checked && chkMavenCache.Checked &&
                             chkNpmCache.Checked && chkDockerCache.Checked && chkNodeModules.Checked &&
                             chkVisualStudio.Checked && chkPythonCache.Checked && chkGitCache.Checked &&
                             chkGameCaches.Checked;
            
            // Vérifier si aucune option n'est cochée
            bool noneChecked = !chkRecycle.Checked && !chkSystemTemp.Checked && !chkBrowsers.Checked &&
                              !chkBrowserHistory.Checked && !chkWindowsUpdate.Checked && !chkThumbnails.Checked && !chkPrefetch.Checked &&
                              !chkFlushDns.Checked && !chkOrphanedFiles.Checked && !chkClearMemoryCache.Checked &&
                              !chkBrokenShortcuts.Checked && !chkGhostApps.Checked && !chkEmptyFolders.Checked &&
                              !chkVsCodeCache.Checked && !chkNugetCache.Checked && !chkMavenCache.Checked &&
                              !chkNpmCache.Checked && !chkDockerCache.Checked && !chkNodeModules.Checked &&
                              !chkVisualStudio.Checked && !chkPythonCache.Checked && !chkGitCache.Checked &&
                              !chkGameCaches.Checked;
            
            // Mettre à jour l'apparence des boutons avec des couleurs vives
            if (allChecked)
            {
                // Toutes les options cochées → VERT
                btnSelectAll.BackColor = Color.FromArgb(76, 175, 80); // Vert vif
                btnSelectAll.ForeColor = Color.White;
                btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(56, 142, 60);
                btnDeselectAll.BackColor = Color.White;
                btnDeselectAll.ForeColor = Color.Black;
                btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            }
            else if (noneChecked)
            {
                // Aucune option cochée → ROUGE
                btnDeselectAll.BackColor = Color.FromArgb(244, 67, 54); // Rouge vif
                btnDeselectAll.ForeColor = Color.White;
                btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(198, 40, 40);
                btnSelectAll.BackColor = Color.White;
                btnSelectAll.ForeColor = Color.Black;
                btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            }
            else
            {
                // Partiellement sélectionné - ORANGE pour signaler
                btnSelectAll.BackColor = Color.FromArgb(255, 152, 0); // Orange
                btnSelectAll.ForeColor = Color.White;
                btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(230, 126, 0);
                btnDeselectAll.BackColor = Color.FromArgb(255, 152, 0); // Orange
                btnDeselectAll.ForeColor = Color.White;
                btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(230, 126, 0);
            }
        }

        private async Task StartCleanerAsync(bool dryRun)
        {
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;
            btnClean.Visible = false;
            btnDryRun.Visible = false;
            btnCancel.Visible = true;
            btnCancel.Enabled = true;
            // Positionner le bouton Annuler à l'emplacement des boutons d'action
            btnCancel.Top = 80;
            btnCancel.Height = 40;
            _suppressLogs = false; // réactiver l'affichage des logs pour ce run

            // Ré-abonner le logger si besoin
            if (!_logSubscribed)
            {
                Logger.OnLog += Logger_OnLog;
                _logSubscribed = true;
            }
            lvLogs.Items.Clear();
            progressBar.Value = 0;
            statusLabel.Text = dryRun ? "Dry run en cours..." : "Nettoyage en cours...";

            var selectedProfile = GetSelectedProfile();
            var profileUsed = selectedProfile?.Name ?? CustomProfileLabel;

            CleanerOptions options = selectedProfile != null
                ? selectedProfile.ToCleanerOptions(dryRun)
                : new CleanerOptions
                {
                    DryRun = dryRun,
                    EmptyRecycleBin = chkRecycle.Checked,
                    IncludeSystemTemp = chkSystemTemp.Checked,
                    CleanBrowsers = chkBrowsers.Checked,
                    CleanBrowserHistory = chkBrowserHistory.Checked,
                    CleanWindowsUpdate = chkWindowsUpdate.Checked,
                    CleanThumbnails = chkThumbnails.Checked,
                    CleanPrefetch = chkPrefetch.Checked,
                    FlushDns = chkFlushDns.Checked,
                    Verbose = chkVerbose.Checked,
                    // Advanced options
                    CleanSystemLogs = false,
                    CleanInstallerCache = false,
                    CleanOrphanedFiles = chkOrphanedFiles.Checked,
                    CleanApplicationLogs = false,
                    ClearMemoryCache = chkClearMemoryCache.Checked,
                    CleanBrokenShortcuts = chkBrokenShortcuts.Checked,
                    CleanGhostApps = chkGhostApps.Checked,
                    // Developer options
                    CleanVsCodeCache = chkVsCodeCache.Checked,
                    CleanNugetCache = chkNugetCache.Checked,
                    CleanMavenCache = chkMavenCache.Checked,
                    CleanNpmCache = chkNpmCache.Checked,
                    CleanDocker = chkDockerCache.Checked,
                    CleanNodeModules = chkNodeModules.Checked,
                    CleanVisualStudio = chkVisualStudio.Checked,
                    CleanPythonCache = chkPythonCache.Checked,
                    CleanGitCache = chkGitCache.Checked,
                    CleanGameCaches = chkGameCaches.Checked,
                };

            // advanced mode: generate and show report before executing (unless dry-run)
            if (chkAdvanced.Checked && !dryRun)
            {
                statusLabel.Text = "Génération du rapport avancé...";
                var report = Cleaner.GenerateReport(options, s => Logger.Log(LogLevel.Debug, s));
                var summary = $"Éléments candidats: {report.Count}\nEspace estimé: {report.TotalBytes} octets";
                var details = new System.Text.StringBuilder();
                details.AppendLine(summary);
                details.AppendLine();
                foreach (var it in report.Items.Take(200)) // limit preview size
                {
                    details.AppendLine($"{(it.IsDirectory ? "[D]" : "[F]")} {it.Path} ({it.Size} octets)");
                }
                if (report.Count > 200) details.AppendLine($"... ({report.Count - 200} autres éléments non affichés)");

                var dlg = new Form() { Text = "Rapport avancé - aperçu", Width = 1000, Height = 700 };
                var panelTop = new Panel() { Dock = DockStyle.Top, Height = 44 };
                var lblFilter = new Label() { Text = "Filtre:", Left = 6, Top = 12, AutoSize = true };
                var txtFilter = new TextBox() { Left = 56, Top = 8, Width = 480, Height = 26 };
                panelTop.Controls.Add(lblFilter);
                panelTop.Controls.Add(txtFilter);

                var dgv = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
                var colType = new DataGridViewTextBoxColumn() { Name = "Type", HeaderText = "Type", DataPropertyName = "Type", Width = 50 };
                var colPath = new DataGridViewTextBoxColumn() { Name = "Path", HeaderText = "Path", DataPropertyName = "Path", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
                var colSize = new DataGridViewTextBoxColumn() { Name = "Size", HeaderText = "Size", DataPropertyName = "Size", Width = 120 };
                dgv.Columns.Add(colType);
                dgv.Columns.Add(colPath);
                dgv.Columns.Add(colSize);

                // Build DataTable for sortable view
                var dt = new System.Data.DataTable();
                dt.Columns.Add("Type", typeof(string));
                dt.Columns.Add("Path", typeof(string));
                dt.Columns.Add("Size", typeof(long));
                foreach (var it in report.Items)
                {
                    var row = dt.NewRow();
                    row["Type"] = it.IsDirectory ? "D" : "F";
                    row["Path"] = it.Path;
                    row["Size"] = it.Size;
                    dt.Rows.Add(row);
                }

                var dv = dt.DefaultView;

                // apply persisted sort if present
                var sett = SettingsManager.Load();
                if (!string.IsNullOrEmpty(sett.ReportSortColumn) && (sett.ReportSortDirection == "ASC" || sett.ReportSortDirection == "DESC"))
                {
                    dv.Sort = sett.ReportSortColumn + " " + (sett.ReportSortDirection == "ASC" ? "ASC" : "DESC");
                }

                var bs = new BindingSource();
                bs.DataSource = dv;
                dgv.DataSource = bs;

                txtFilter.TextChanged += (s, e) =>
                {
                    var f = txtFilter.Text ?? string.Empty;
                    dv.RowFilter = string.IsNullOrWhiteSpace(f) ? string.Empty : $"Path LIKE '%" + f.Replace("'", "''") + "%'";
                };

                var btnPanel = new Panel() { Dock = DockStyle.Bottom, Height = 50 };
                var btnProceed = new Button() { Text = "Continuer et exécuter", Left = 10, Width = 180, Top = 8, Height = 35, DialogResult = DialogResult.OK }; btnProceed.FlatStyle = FlatStyle.Flat;
                var btnCancelReport = new Button() { Text = "Annuler", Left = 200, Width = 100, Top = 8, Height = 35, DialogResult = DialogResult.Cancel }; btnCancelReport.FlatStyle = FlatStyle.Flat;
                var btnSave = new Button() { Text = "Enregistrer le rapport complet", Left = 310, Width = 220, Top = 8, Height = 35 }; btnSave.FlatStyle = FlatStyle.Flat;
                btnSave.Click += (s, e) =>
                {
                    using var sfd = new SaveFileDialog();
                    sfd.Filter = "CSV|*.csv|Texte|*.txt|Tous|*.*";
                    sfd.FileName = "rapport_avance.csv";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using var sw = new System.IO.StreamWriter(sfd.FileName);
                        sw.WriteLine("Type,Path,Size");
                        foreach (var it in report.Items)
                            sw.WriteLine($"{(it.IsDirectory ? "D" : "F")},\"{it.Path.Replace("\"", "\"\"")}\",{it.Size}");
                    }
                };
                btnPanel.Controls.Add(btnProceed); btnPanel.Controls.Add(btnCancelReport); btnPanel.Controls.Add(btnSave);
                dlg.Controls.Add(panelTop); dlg.Controls.Add(dgv); dlg.Controls.Add(btnPanel);
                var res = dlg.ShowDialog();
                // persist current sort
                try
                {
                    if (dgv.SortedColumn != null)
                    {
                        var col = dgv.SortedColumn.Name;
                        var dir = dgv.SortOrder == SortOrder.Ascending ? "ASC" : "DESC";
                        var s = SettingsManager.Load();
                        s.ReportSortColumn = col;
                        s.ReportSortDirection = dir;
                        SettingsManager.Save(s);
                    }
                }
                catch { }

                // double-click to open
                dgv.CellDoubleClick += (ss, ee) =>
                {
                    try
                    {
                        if (ee.RowIndex < 0) return;
                        var row = ((System.Data.DataRowView?)((BindingSource)dgv.DataSource).Current)?.Row;
                        // safer approach: get value directly from dgv
                        var val = dgv.Rows[ee.RowIndex].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        if (System.IO.Directory.Exists(val))
                        {
                            Process.Start(new ProcessStartInfo("explorer.exe", '"' + val + '"') { UseShellExecute = true });
                            Logger.Log(LogLevel.Info, LanguageManager.Get("log_open_folder", val));
                        }
                        else if (System.IO.File.Exists(val))
                        {
                            // select the file
                            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + val + "\"") { UseShellExecute = true });
                            Logger.Log(LogLevel.Info, LanguageManager.Get("log_open_file", val));
                        }
                        else
                        {
                            MessageBox.Show(LanguageManager.Get("msg_path_not_found_short", val), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("log_cannot_open", ex.Message));
                    }
                };

                // Context menu: Open / Copy path / Ignore
                var cms = new ContextMenuStrip();
                var openItem = new ToolStripMenuItem("Ouvrir dans l'Explorateur");
                var copyItem = new ToolStripMenuItem("Copier le chemin");
                var ignoreItem = new ToolStripMenuItem("Ignorer cet élément");
                cms.Items.Add(openItem);
                cms.Items.Add(copyItem);
                cms.Items.Add(new ToolStripSeparator());
                cms.Items.Add(ignoreItem);
                dgv.ContextMenuStrip = cms;

                // Ensure right-click selects the row
                dgv.CellMouseDown += (ss, ee) =>
                {
                    if (ee.Button == MouseButtons.Right && ee.RowIndex >= 0)
                    {
                        dgv.ClearSelection();
                        dgv.Rows[ee.RowIndex].Selected = true;
                        dgv.CurrentCell = dgv.Rows[ee.RowIndex].Cells[0];
                    }
                };

                openItem.Click += (s3, e3) =>
                {
                    try
                    {
                        if (dgv.SelectedRows.Count == 0) return;
                        var val = dgv.SelectedRows[0].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        if (System.IO.Directory.Exists(val))
                            Process.Start(new ProcessStartInfo("explorer.exe", '"' + val + '"') { UseShellExecute = true });
                        else if (System.IO.File.Exists(val))
                            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + val + "\"") { UseShellExecute = true });
                    }
                    catch (Exception ex) { Logger.Log(LogLevel.Error, LanguageManager.Get("log_open_item_error", ex.Message)); }
                };

                copyItem.Click += (s3, e3) =>
                {
                    try
                    {
                        if (dgv.SelectedRows.Count == 0) return;
                        var val = dgv.SelectedRows[0].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        Clipboard.SetText(val);
                    }
                    catch (Exception ex) { Logger.Log(LogLevel.Error, LanguageManager.Get("log_copy_path_error", ex.Message)); }
                };

                ignoreItem.Click += (s3, e3) =>
                {
                    try
                    {
                        if (dgv.SelectedRows.Count == 0) return;
                        var val = dgv.SelectedRows[0].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        // remove from DataTable
                        var rows = dt.Select("Path = '" + val.Replace("'", "''") + "'");
                        foreach (var r in rows) r.Delete();
                        dt.AcceptChanges();
                        // remove from report items
                        report.Items.RemoveAll(it => string.Equals(it.Path, val, StringComparison.OrdinalIgnoreCase));
                    }
                    catch (Exception ex) { Logger.Log(LogLevel.Error, LanguageManager.Get("log_ignore_item_error", ex.Message)); }
                };
                // persist current sort
                try
                {
                    if (dgv.SortedColumn != null)
                    {
                        var col = dgv.SortedColumn.Name;
                        var dir = dgv.SortOrder == SortOrder.Ascending ? "ASC" : "DESC";
                        var s = SettingsManager.Load();
                        s.ReportSortColumn = col;
                        s.ReportSortDirection = dir;
                        SettingsManager.Save(s);
                    }
                }
                catch { }
                if (res != DialogResult.OK)
                {
                    Logger.Log(LogLevel.Warning, LanguageManager.Get("log_warning_cancelled"));
                    statusLabel.Text = "Annulé (rapport)";
                    btnClean.Enabled = true;
                    btnDryRun.Enabled = true;
                    btnClean.Visible = true;
                    btnDryRun.Visible = true;
                    btnCancel.Enabled = false;
                    btnCancel.Visible = false;
                    return;
                }
                // otherwise continue
                statusLabel.Text = "Exécution après rapport avancé...";
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var sw = Stopwatch.StartNew();

            try
            {
                // If performing real cleaning, confirm dangerous options
                if (!dryRun)
                {
                    var dangers = new System.Text.StringBuilder();
                    if (options.IncludeSystemTemp) dangers.AppendLine(LanguageManager.Get("confirm_include_system_temp"));
                    if (options.CleanWindowsUpdate) dangers.AppendLine(LanguageManager.Get("confirm_clean_windows_update"));
                    if (options.CleanPrefetch) dangers.AppendLine(LanguageManager.Get("confirm_clean_prefetch"));
                    if (options.EmptyRecycleBin) dangers.AppendLine(LanguageManager.Get("confirm_empty_recycle_bin"));

                    if (dangers.Length > 0)
                    {
                        var msg = LanguageManager.Get("confirm_dangerous_operations") + "\n\n" + dangers.ToString() + "\n" + LanguageManager.Get("confirm_continue");
                        var confirm = MessageBox.Show(msg, LanguageManager.Get("confirm_operations_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (confirm != DialogResult.Yes)
                        {
                            Logger.Log(LogLevel.Warning, LanguageManager.Get("log_warning_confirmation_refused"));
                            statusLabel.Text = LanguageManager.Get("status_cancelled_confirmation");
                            btnClean.Enabled = true;
                            btnDryRun.Enabled = true;
                            btnCancel.Enabled = false;
                            btnCancel.Visible = false;
                            return;
                        }
                    }
                }

                var mode = dryRun ? LanguageManager.Get("log_dryrun") : LanguageManager.Get("log_execution");
                Logger.Log(LogLevel.Info, LanguageManager.Get("log_cleaning_start", mode));

                // Run the cleaner in a Task and periodically update progress
                var task = Task.Run(() => Cleaner.RunCleanup(options, s => Logger.Log(LogLevel.Info, s), token), token);

                while (!task.IsCompleted)
                {
                    if (token.IsCancellationRequested) break;
                    progressBar.Value = Math.Min(progressBar.Value + 5, 95);
                    await Task.Delay(200, token).ContinueWith(_ => { });
                }

                var result = await task;
                Logger.Log(LogLevel.Info, LanguageManager.Get("log_finished", result.FilesDeleted, result.BytesFreed));
                statusLabel.Text = "Terminé";
                progressBar.Value = 100;

                sw.Stop();

                // Enregistrer les statistiques uniquement en mode réel
                if (!dryRun)
                {
                    StatisticsManager.RecordCleaningSession(new CleaningStatistics
                    {
                        Timestamp = DateTime.Now,
                        ProfileUsed = profileUsed,
                        FilesDeleted = result.FilesDeleted,
                        BytesFreed = result.BytesFreed,
                        Duration = sw.Elapsed,
                        WasDryRun = false,
                        
                        // App cache stats
                        VsCodeCacheFilesDeleted = result.VsCodeCacheFilesDeleted,
                        NugetCacheFilesDeleted = result.NugetCacheFilesDeleted,
                        MavenCacheFilesDeleted = result.MavenCacheFilesDeleted,
                        NpmCacheFilesDeleted = result.NpmCacheFilesDeleted,
                        GameCachesFilesDeleted = result.GameCachesFilesDeleted,
                        AppCachesBytesFreed = result.AppCachesBytesFreed,
                        
                        // SSD stats
                        SsdOptimized = result.SsdOptimized,
                        DiskHealthChecked = result.DiskHealthChecked,
                        DiskHealthReport = result.DiskHealthReport
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log(LogLevel.Warning, LanguageManager.Get("log_cancelled"));
                statusLabel.Text = "Annulé";
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_error", ex.Message));
                statusLabel.Text = "Erreur";
            }
            finally
            {
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
                btnClean.Visible = true;
                btnDryRun.Visible = true;
                btnCancel.Enabled = false;
                btnCancel.Visible = false;
                _suppressLogs = false; // réinitialiser pour la prochaine exécution
                _cts = null;
            }
        }

        private async void DiskAnalyzerMenuItem_Click(object? sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { Description = "Sélectionnez le dossier à analyser" };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            var path = dialog.SelectedPath;
            Logger.Log(LogLevel.Info, LanguageManager.Get("log_analysis_start", path));
            statusLabel.Text = "Analyse en cours...";
            progressBar.Value = 0;
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;

            try
            {
                var progress = new Action<string>(msg => 
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => 
                        {
                            statusLabel.Text = msg;
                            progressBar.Value = Math.Min(progressBar.Value + 2, 95);
                        }));
                    }
                    else
                    {
                        statusLabel.Text = msg;
                        progressBar.Value = Math.Min(progressBar.Value + 2, 95);
                    }
                });

                var result = await Task.Run(() => DiskAnalyzer.AnalyzeDirectory(path, 100, progress));

                // Générer et ouvrir le rapport HTML
                var htmlPath = DiskAnalyzer.ExportHtmlReport(result, path);
                
                // Ouvrir le rapport dans le navigateur par défaut
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = htmlPath,
                        UseShellExecute = true
                    });
                    
                    MessageBox.Show(LanguageManager.Get("msg_analysis_report_opened", htmlPath),
                        LanguageManager.Get("msgbox_analysis_complete"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.Get("msg_report_cannot_open", htmlPath, ex.Message),
                        LanguageManager.Get("msgbox_analysis_complete"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Logger.Log(LogLevel.Info, LanguageManager.Get("log_analysis_finished", result.TotalScannedFiles, FormatBytes(result.TotalScannedSize)));
                statusLabel.Text = "Analyse terminée";
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_analysis_error", ex.Message));
                MessageBox.Show(LanguageManager.Get("msg_analysis_error", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Value = 0;
            }
            finally
            {
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
            }
        }

        private async void DuplicateFinderMenuItem_Click(object? sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { Description = LanguageManager.Get("msg_select_folder_duplicates") };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            var path = dialog.SelectedPath;
            Logger.Log(LogLevel.Info, LanguageManager.Get("log_duplicate_search", path));
            statusLabel.Text = "Recherche de doublons...";
            progressBar.Value = 0;
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;

            try
            {
                var progress = new Action<string>(msg => 
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => 
                        {
                            statusLabel.Text = msg;
                            progressBar.Value = Math.Min(progressBar.Value + 1, 95);
                        }));
                    }
                    else
                    {
                        statusLabel.Text = msg;
                        progressBar.Value = Math.Min(progressBar.Value + 1, 95);
                    }
                });
                var duplicates = await Task.Run(() => DuplicateFinder.FindDuplicates(path, 1024, null, progress));

                // Générer et ouvrir le rapport HTML
                var htmlPath = DuplicateFinder.ExportHtmlReport(duplicates, path);
                
                if (duplicates.DuplicateGroups.Count == 0)
                {
                    // Ouvrir quand même le rapport qui affichera un message positif
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = htmlPath,
                            UseShellExecute = true
                        });
                        
                        MessageBox.Show(LanguageManager.Get("msg_no_duplicates_report", htmlPath),
                            LanguageManager.Get("msgbox_result"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show(LanguageManager.Get("msg_no_duplicates_found"), LanguageManager.Get("msgbox_result"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    Logger.Log(LogLevel.Info, LanguageManager.Get("log_no_duplicates"));
                }
                else
                {
                    // Ouvrir le rapport dans le navigateur par défaut
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = htmlPath,
                            UseShellExecute = true
                        });
                        
                        MessageBox.Show(LanguageManager.Get("msg_duplicates_found", duplicates.DuplicateGroups.Count, FormatBytes(duplicates.TotalWastedSpace), htmlPath),
                            LanguageManager.Get("msgbox_duplicates_detected"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(LanguageManager.Get("msg_report_cannot_open", htmlPath, ex.Message),
                            LanguageManager.Get("msgbox_duplicates_detected"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    Logger.Log(LogLevel.Info, LanguageManager.Get("log_duplicates_found", duplicates.DuplicateGroups.Count, FormatBytes(duplicates.TotalWastedSpace)));
                }
                
                statusLabel.Text = "Recherche terminée";
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_duplicate_error", ex.Message));
                MessageBox.Show(LanguageManager.Get("msg_error_generic", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Value = 0;
            }
            finally
            {
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
            }
        }

        private void StatisticsMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var allStats = StatisticsManager.LoadAllStatistics();

                if (allStats.Count == 0)
                {
                    MessageBox.Show(LanguageManager.Get("msg_no_statistics"),
                        LanguageManager.Get("msgbox_statistics"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Logger.Log(LogLevel.Info, LanguageManager.Get("log_stats_empty"));
                    return;
                }

                // Générer et ouvrir le rapport HTML
                var htmlPath = StatisticsManager.ExportHtmlReport();

                if (string.IsNullOrEmpty(htmlPath))
                {
                    MessageBox.Show(LanguageManager.Get("msg_cannot_generate_report"), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = htmlPath,
                        UseShellExecute = true
                    });

                    MessageBox.Show(LanguageManager.Get("msg_report_generated", htmlPath),
                        LanguageManager.Get("msgbox_statistics"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.Get("msg_report_cannot_open", htmlPath, ex.Message),
                        LanguageManager.Get("msgbox_statistics"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Logger.Log(LogLevel.Info, LanguageManager.Get("log_stats_displayed"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("msg_stats_error", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_stats_error", ex.Message));
            }
        }

        private void ProfilesMenuItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(LanguageManager.Get("msg_profiles_info"), LanguageManager.Get("msgbox_profiles_available"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SchedulerMenuItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(LanguageManager.Get("msg_scheduler_info"), LanguageManager.Get("msgbox_scheduler"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BackupMenuItem_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show(LanguageManager.Get("msg_restore_point_confirm"), 
                LanguageManager.Get("msgbox_confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            statusLabel.Text = "Création du point de restauration...";
            progressBar.Value = 0;
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;

            try
            {
                // Animation de la progress bar
                var progressTask = Task.Run(async () =>
                {
                    for (int i = 0; i < 90; i += 5)
                    {
                        await Task.Delay(500);
                        if (InvokeRequired)
                            Invoke(new Action(() => progressBar.Value = i));
                        else
                            progressBar.Value = i;
                    }
                });

                var success = await Task.Run(() => BackupManager.CreateSystemRestorePoint("Windows Cleaner Manuel"));
                
                await progressTask;
                progressBar.Value = 100;
                
                if (success)
                {
                    MessageBox.Show(LanguageManager.Get("msg_restore_point_success"), LanguageManager.Get("msgbox_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Logger.Log(LogLevel.Info, LanguageManager.Get("log_restore_point_created"));
                }
                else
                {
                    MessageBox.Show(LanguageManager.Get("msg_restore_point_failed"), 
                        LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("msg_error_generic", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_restore_point_error", ex.Message));
                progressBar.Value = 0;
            }
            finally
            {
                statusLabel.Text = "Prêt";
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
            }
        }

        private async void OptimizerMenuItem_Click(object? sender, EventArgs e)
        {
            var msg = LanguageManager.Get("opt_dialog_available") + "\n\n";
            msg += LanguageManager.Get("opt_trim_ssd") + "\n";
            msg += LanguageManager.Get("opt_compact_registry") + "\n";
            msg += LanguageManager.Get("opt_clean_memory") + "\n\n";
            msg += LanguageManager.Get("opt_requires_admin");
            
            if (MessageBox.Show(msg, LanguageManager.Get("opt_dialog_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            statusLabel.Text = LanguageManager.Get("opt_in_progress");
            progressBar.Value = 0;
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;

            var startTime = DateTime.Now;
            try
            {
                var results = new System.Text.StringBuilder();
                
                Logger.Log(LogLevel.Info, LanguageManager.Get("log_optimization_start"));
                
                // TRIM SSD
                progressBar.Value = 10;
                statusLabel.Text = LanguageManager.Get("opt_trim_progress");
                var trimResult = await Task.Run(() => SystemOptimizer.OptimizeSsd());
                results.AppendLine(LanguageManager.Get("opt_trim_result", trimResult ? LanguageManager.Get("opt_success") : LanguageManager.Get("opt_failure")));
                
                // Vérification SMART
                progressBar.Value = 30;
                statusLabel.Text = LanguageManager.Get("opt_smart_progress");
                var smartReport = await Task.Run(() => SystemOptimizer.CheckDiskHealth());
                var healthOk = !string.IsNullOrEmpty(smartReport);
                results.AppendLine(LanguageManager.Get("opt_smart_result", healthOk ? LanguageManager.Get("opt_success") : LanguageManager.Get("opt_failure")));
                
                // Compaction registre
                progressBar.Value = 50;
                statusLabel.Text = LanguageManager.Get("opt_registry_progress");
                var regResult = await Task.Run(() => SystemOptimizer.CompactRegistry());
                results.AppendLine(LanguageManager.Get("opt_registry_result", regResult ? LanguageManager.Get("opt_success") : LanguageManager.Get("opt_failure")));
                
                // Nettoyage mémoire
                progressBar.Value = 75;
                statusLabel.Text = LanguageManager.Get("opt_memory_progress");
                var memResult = await Task.Run(() => SystemOptimizer.ClearStandbyMemory());
                results.AppendLine(LanguageManager.Get("opt_memory_result", memResult ? LanguageManager.Get("opt_success") : LanguageManager.Get("opt_failure")));
                
                progressBar.Value = 100;
                
                // Sauvegarder les statistiques
                var duration = DateTime.Now - startTime;
                var stats = new CleaningStatistics
                {
                    Timestamp = DateTime.Now,
                    ProfileUsed = LanguageManager.Get("opt_profile_name"),
                    FilesDeleted = 0,
                    BytesFreed = 0,
                    Duration = duration,
                    WasDryRun = false,
                    SsdOptimized = trimResult,
                    DiskHealthChecked = healthOk,
                    DiskHealthReport = smartReport ?? ""
                };
                
                StatisticsManager.RecordCleaningSession(stats);
                Logger.Log(LogLevel.Info, LanguageManager.Get("log_optimization_stats", trimResult, healthOk));
                
                MessageBox.Show($"{LanguageManager.Get("opt_results_completed")}\n\n{results}", LanguageManager.Get("opt_results_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logger.Log(LogLevel.Info, LanguageManager.Get("log_optimization_finished"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("msg_error_generic", ex.Message), LanguageManager.Get("msgbox_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(LogLevel.Error, LanguageManager.Get("log_optimization_error", ex.Message));
                progressBar.Value = 0;
            }
            finally
            {
                statusLabel.Text = "Prêt";
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
            }
        }

        private async void CheckUpdateMenuItem_Click(object? sender, EventArgs e)
        {
            await CheckForUpdatesAsync(silent: false);
        }

        private async Task CheckForUpdatesAsync(bool silent)
        {
            try
            {
                // Configurer avec votre dépôt GitHub
                // Format: "propriétaire", "nom-du-repo"
                var updateManager = new UpdateManager("christwadel65-ux", "Windows-Cleaner", AppVersion.Current);
                
                if (silent)
                {
                    // Vérification silencieuse au démarrage
                    var updateInfo = await updateManager.CheckForUpdateAsync();
                    if (updateInfo != null)
                    {
                        // Afficher une notification discrète dans la barre de statut
                        statusLabel.Text = LanguageManager.Get("update_available", updateInfo.Version);
                        statusLabel.ForeColor = Color.Orange;
                    }
                }
                else
                {
                    // Vérification manuelle avec dialogue
                    await updateManager.CheckAndNotifyUpdateAsync(this);
                }
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    MessageBox.Show(
                        $"Impossible de vérifier les mises à jour :\n{ex.Message}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
                Logger.Log(LogLevel.Warning, LanguageManager.Get("update_check_error", ex.Message));
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

        private void SysConfigMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var sysConfigForm = new SystemConfigForm
                {
                    Icon = this.Icon
                };
                sysConfigForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.Get("msg_error_generic", ex.Message),
                    LanguageManager.Get("msgbox_error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void UninstallerMenuItem_Click(object? sender, EventArgs e)
        {
            var uninstallerForm = new Form
            {
                Text = LanguageManager.Get("uninstaller_title"),
                Size = new Size(1100, 700),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MinimumSize = new Size(900, 500),
                BackColor = SystemColors.Control,
                Icon = this.Icon
            };

            // Main layout - Top to Bottom
            var mainPanel = new Panel { Dock = DockStyle.Fill };

            // TOP PANEL - Title and Search
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12, 10, 12, 10)
            };

            var lblTitle = new Label
            {
                Text = LanguageManager.Get("uninstaller_title"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(12, 8),
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);

            var txtSearch = new TextBox
            {
                Location = new Point(12, 32),
                Size = new Size(550, 28),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "🔍 " + LanguageManager.Get("uninstaller_select_program")
            };
            topPanel.Controls.Add(txtSearch);

            mainPanel.Controls.Add(topPanel);

            // MIDDLE PANEL - ListView
            var middlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = SystemColors.Control
            };

            var lvPrograms = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lvPrograms.Columns.Add(LanguageManager.Get("uninstaller_program_header"), 380);
            lvPrograms.Columns.Add(LanguageManager.Get("uninstaller_version_header"), 110);
            lvPrograms.Columns.Add(LanguageManager.Get("uninstaller_publisher_header"), 320);
            lvPrograms.Columns.Add(LanguageManager.Get("uninstaller_size_header"), 110);

            middlePanel.Controls.Add(lvPrograms);
            mainPanel.Controls.Add(middlePanel);

            // LOGS PANEL - Bottom with logs
            var logsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8)
            };

            var txtLogs = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Courier New", 8),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Lime,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            logsPanel.Controls.Add(txtLogs);
            mainPanel.Controls.Add(logsPanel);

            // BOTTOM PANEL - Options and Buttons
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12)
            };

            var chkCleanRemnants = new CheckBox
            {
                Text = LanguageManager.Get("uninstaller_cleanup_remnants"),
                Location = new Point(12, 12),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 10)
            };
            bottomPanel.Controls.Add(chkCleanRemnants);

            // Buttons Row
            var btnUninstall = new Button
            {
                Text = LanguageManager.Get("btn_uninstall"),
                Location = new Point(12, 40),
                Size = new Size(130, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(btnUninstall);

            var btnExport = new Button
            {
                Text = LanguageManager.Get("btn_export_csv"),
                Location = new Point(155, 40),
                Size = new Size(130, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(btnExport);

            var btnCancel = new Button
            {
                Text = "Fermer",
                Location = new Point(1000, 40),
                Size = new Size(90, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(btnCancel);

            mainPanel.Controls.Add(bottomPanel);
            uninstallerForm.Controls.Add(mainPanel);

            // Charger les programmes
            statusLabel.Text = LanguageManager.Get("uninstaller_loading");
            Application.DoEvents();

            var programs = ProgramUninstaller.GetInstalledPrograms();
            foreach (var program in programs)
            {
                var size = program.EstimatedSize > 0 ? (program.EstimatedSize / 1024.0).ToString("F2") : "N/A";
                var item = new ListViewItem(program.DisplayName);
                item.SubItems.Add(program.DisplayVersion);
                item.SubItems.Add(program.Publisher);
                item.SubItems.Add(size);
                item.Tag = program;
                lvPrograms.Items.Add(item);
            }

            statusLabel.Text = LanguageManager.Get("uninstaller_programs_found", programs.Count);

            // Search filter
            txtSearch.TextChanged += (s, e) =>
            {
                lvPrograms.Items.Clear();
                var filtered = programs.Where(p => p.DisplayName.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var program in filtered)
                {
                    var size = program.EstimatedSize > 0 ? (program.EstimatedSize / 1024.0).ToString("F2") : "N/A";
                    var item = new ListViewItem(program.DisplayName);
                    item.SubItems.Add(program.DisplayVersion);
                    item.SubItems.Add(program.Publisher);
                    item.SubItems.Add(size);
                    item.Tag = program;
                    lvPrograms.Items.Add(item);
                }
            };

            btnUninstall.Click += (s, ev) =>
            {
                if (lvPrograms.SelectedItems.Count == 0)
                {
                    MessageBox.Show(LanguageManager.Get("uninstaller_no_selection"), LanguageManager.Get("uninstaller_attention"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    LanguageManager.Get("uninstaller_confirm", lvPrograms.SelectedItems.Count),
                    LanguageManager.Get("msgbox_confirmation"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm != DialogResult.Yes) return;

                btnUninstall.Enabled = false;
                btnExport.Enabled = false;
                lvPrograms.Enabled = false;
                chkCleanRemnants.Enabled = false;
                txtSearch.Enabled = false;

                var log = new Action<string>(msg =>
                {
                    // Affichage dans les logs
                    txtLogs.AppendText(msg + Environment.NewLine);
                    txtLogs.ScrollToCaret();
                });

                var successCount = 0;
                var failureCount = 0;
                var successList = new List<string>();
                var failureList = new List<string>();

                log("═══════════════════════════════════════════════════════");
                log("🗑️  DÉBUT DE LA DÉSINSTALLATION");
                log("═══════════════════════════════════════════════════════");
                log("");

                foreach (ListViewItem item in lvPrograms.SelectedItems)
                {
                    if (item.Tag is ProgramUninstaller.InstalledProgram program)
                    {
                        log($"→ Désinstallation de : {program.DisplayName} (v{program.DisplayVersion})");
                        log($"  Éditeur: {program.Publisher}");
                        
                        if (ProgramUninstaller.UninstallProgram(program, true, log))
                        {
                            log($"  ✓ Désinstallation réussie");
                            
                            if (chkCleanRemnants.Checked)
                            {
                                log($"  • Nettoyage des fichiers résiduels...");
                                ProgramUninstaller.CleanupProgramRemnants(program, log);
                                log($"  ✓ Nettoyage terminé");
                            }
                            
                            successCount++;
                            successList.Add(program.DisplayName);
                            item.ForeColor = Color.Green;
                            log("");
                        }
                        else
                        {
                            log($"  ✗ ERREUR : Désinstallation échouée");
                            failureCount++;
                            failureList.Add(program.DisplayName);
                            item.ForeColor = Color.Red;
                            log("");
                        }
                    }
                }

                log("═══════════════════════════════════════════════════════");
                log("📊 RÉSUMÉ DE LA DÉSINSTALLATION");
                log("═══════════════════════════════════════════════════════");
                log($"✓ Applications supprimées avec succès : {successCount}");
                if (successCount > 0)
                {
                    foreach (var appName in successList)
                    {
                        log($"  • {appName}");
                    }
                }
                log($"✗ Applications échouées : {failureCount}");
                if (failureCount > 0)
                {
                    foreach (var appName in failureList)
                    {
                        log($"  • {appName}");
                    }
                }
                log("═══════════════════════════════════════════════════════");
                log("");
                
                // Rafraîchir la liste des programmes
                log("🔄 Rafraîchissement de la liste des programmes...");
                Application.DoEvents();
                
                lvPrograms.Items.Clear();
                var updatedPrograms = ProgramUninstaller.GetInstalledPrograms();
                foreach (var program in updatedPrograms)
                {
                    var size = program.EstimatedSize > 0 ? (program.EstimatedSize / 1024.0).ToString("F2") : "N/A";
                    var item = new ListViewItem(program.DisplayName);
                    item.SubItems.Add(program.DisplayVersion);
                    item.SubItems.Add(program.Publisher);
                    item.SubItems.Add(size);
                    item.Tag = program;
                    lvPrograms.Items.Add(item);
                }
                
                log("✓ Liste rafraîchie");
                log("");
                
                btnUninstall.Enabled = true;
                btnExport.Enabled = true;
                lvPrograms.Enabled = true;
                chkCleanRemnants.Enabled = true;
                txtSearch.Enabled = true;
            };

            btnExport.Click += (s, ev) =>
            {
                var saveDialog = new SaveFileDialog
                {
                    FileName = $"programmes_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    Filter = "Fichiers CSV (*.csv)|*.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (ProgramUninstaller.ExportProgramsList(saveDialog.FileName))
                    {
                        MessageBox.Show(LanguageManager.Get("uninstaller_export_success", saveDialog.FileName), LanguageManager.Get("msgbox_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(LanguageManager.Get("uninstaller_export_error"), LanguageManager.Get("msgbox_result"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            uninstallerForm.CancelButton = btnCancel;
            uninstallerForm.ShowDialog(this);
        }
    }
}
