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

        private CheckBox chkRecycle = null!;
        private CheckBox chkSystemTemp = null!;
        private CheckBox chkBrowsers = null!;
        private CheckBox chkWindowsUpdate = null!;
        private CheckBox chkThumbnails = null!;
        private CheckBox chkPrefetch = null!;
        private CheckBox chkFlushDns = null!;
        private CheckBox chkVerbose = null!;
        private CheckBox chkAdvanced = null!;
        
        // Advanced cleaning options
        private CheckBox chkOrphanedFiles = null!;
        private CheckBox chkClearMemoryCache = null!;

        // Profile selection
        private ComboBox cmbProfiles = null!;
        private Label lblProfile = null!;

        private ListView lvLogs = null!;
        private ColoredProgressBar progressBar = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        private CancellationTokenSource? _cts;
        private Color _accentColor = Color.FromArgb(0, 120, 215);
        private bool _isDark = false;
        private List<CleaningProfile> _profiles = new List<CleaningProfile>();
        private bool _isApplyingProfile = false;
        private bool _suppressProfileEvent = false;
        private const string CustomProfileLabel = "Personnalisé (manuel)";

#pragma warning disable CS8774
        public MainForm()
        {
            Text = "Windows Cleaner - Nettoyage Professionnel";
            Width = 1220;
            Height = 820;
            MinimumSize = new Size(1220, 700);
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = true;
            AutoScaleMode = AutoScaleMode.Dpi;

            // Check if running as admin
            var isAdmin = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                MessageBox.Show(
                    "⚠️ Cette application doit s'exécuter en mode Administrateur pour fonctionner correctement.\n\n" +
                    "Certaines opérations de nettoyage (fichiers système, Temp système, Windows Update, Prefetch, Flush DNS) nécessitent les droits administrateur.\n\n" +
                    "Le nettoyage des fichiers utilisateur fonctionnera partiellement sans admin.",
                    "Avertissement : Droits insuffisants",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            InitializeComponents();
            LoadProfilesIntoCombo();
            Logger.Init();
            Logger.OnLog += Logger_OnLog;
            
            // Charger les paramètres sauvegardés
            LoadSavedOptions();
        }
        
        private void LoadSavedOptions()
        {
            try
            {
                var settings = SettingsManager.Load();
                var preferredProfile = settings.SelectedProfileName;

                // Si un profil enregistré existe encore, l'appliquer
                var matchingProfile = _profiles.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(preferredProfile) &&
                    p.Name.Equals(preferredProfile, StringComparison.OrdinalIgnoreCase));

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
        
        private void SaveOptions()
        {
            try
            {
                var settings = new AppSettings
                {
                    CleanRecycleBin = chkRecycle.Checked,
                    CleanSystemTemp = chkSystemTemp.Checked,
                    CleanBrowsers = chkBrowsers.Checked,
                    CleanWindowsUpdate = chkWindowsUpdate.Checked,
                    CleanThumbnails = chkThumbnails.Checked,
                    CleanPrefetch = chkPrefetch.Checked,
                    FlushDns = chkFlushDns.Checked,
                    Verbose = chkVerbose.Checked,
                    Advanced = chkAdvanced.Checked,
                    CleanOrphanedFiles = chkOrphanedFiles.Checked,
                    ClearMemoryCache = chkClearMemoryCache.Checked,
                    SelectedProfileName = cmbProfiles?.SelectedItem?.ToString() ?? CustomProfileLabel,
                };
                SettingsManager.Save(settings);
            }
            catch { /* Ignorer les erreurs de sauvegarde */ }
        }

        private void RestoreCheckboxesFromSettings(AppSettings settings)
        {
            if (settings.CleanRecycleBin.HasValue) chkRecycle.Checked = settings.CleanRecycleBin.Value;
            if (settings.CleanSystemTemp.HasValue) chkSystemTemp.Checked = settings.CleanSystemTemp.Value;
            if (settings.CleanBrowsers.HasValue) chkBrowsers.Checked = settings.CleanBrowsers.Value;
            if (settings.CleanWindowsUpdate.HasValue) chkWindowsUpdate.Checked = settings.CleanWindowsUpdate.Value;
            if (settings.CleanThumbnails.HasValue) chkThumbnails.Checked = settings.CleanThumbnails.Value;
            if (settings.CleanPrefetch.HasValue) chkPrefetch.Checked = settings.CleanPrefetch.Value;
            if (settings.FlushDns.HasValue) chkFlushDns.Checked = settings.FlushDns.Value;
            if (settings.Verbose.HasValue) chkVerbose.Checked = settings.Verbose.Value;
            if (settings.Advanced.HasValue) chkAdvanced.Checked = settings.Advanced.Value;
            if (settings.CleanOrphanedFiles.HasValue) chkOrphanedFiles.Checked = settings.CleanOrphanedFiles.Value;
            if (settings.ClearMemoryCache.HasValue) chkClearMemoryCache.Checked = settings.ClearMemoryCache.Value;
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
                chkWindowsUpdate.Checked = profile.CleanWindowsUpdate;
                chkThumbnails.Checked = profile.CleanThumbnails;
                chkPrefetch.Checked = profile.CleanPrefetch;
                chkFlushDns.Checked = profile.FlushDns;
                chkVerbose.Checked = profile.Verbose;
                chkOrphanedFiles.Checked = profile.CleanOrphanedFiles;
                chkClearMemoryCache.Checked = profile.ClearMemoryCache;
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
            statusLabel.Text = $"Profil appliqué: {selectedProfile.Name}";
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
            nameof(btnDryRun), nameof(btnClean), nameof(btnCancel), nameof(chkRecycle), nameof(chkSystemTemp),
            nameof(chkBrowsers), nameof(chkWindowsUpdate), nameof(chkThumbnails), nameof(chkPrefetch),
            nameof(chkFlushDns), nameof(chkVerbose), nameof(chkAdvanced), nameof(chkOrphanedFiles), nameof(chkClearMemoryCache), 
            nameof(cmbProfiles), nameof(lblProfile), nameof(lvLogs), nameof(progressBar), nameof(statusStrip), nameof(statusLabel))]
        private void InitializeComponents()
        {
            menu = new MenuStrip();
            fileMenu = new ToolStripMenuItem("Fichier");
            var clearLogsMenuItem = new ToolStripMenuItem("Effacer les logs");
            var readLogsMenuItem = new ToolStripMenuItem("📖 Lire les logs");
            exportLogsMenuItem = new ToolStripMenuItem("Exporter les logs");
            exitMenuItem = new ToolStripMenuItem("Quitter");
            var viewMenu = new ToolStripMenuItem("Affichage");
            var themeLight = new ToolStripMenuItem("Thème Clair");
            var themeDark = new ToolStripMenuItem("Thème Sombre");
            var accentBlue = new ToolStripMenuItem("Accent Bleu");
            var accentGreen = new ToolStripMenuItem("Accent Vert");
            var accentOrange = new ToolStripMenuItem("Accent Orange");
            var toolsMenu = new ToolStripMenuItem("Outils");
            var diskAnalyzerMenuItem = new ToolStripMenuItem("📊 Analyser l'espace disque");
            var duplicateFinderMenuItem = new ToolStripMenuItem("🔍 Détecter les doublons");
            var statisticsMenuItem = new ToolStripMenuItem("📈 Voir les statistiques");
            var profilesMenuItem = new ToolStripMenuItem("📋 Gérer les profils");
            var schedulerMenuItem = new ToolStripMenuItem("⏰ Planifier un nettoyage");
            var backupMenuItem = new ToolStripMenuItem("💾 Créer un point de restauration");
            var optimizerMenuItem = new ToolStripMenuItem("⚡ Optimiser le système");
            
            var helpMenu = new ToolStripMenuItem("Aide");
            var aboutMenuItem = new ToolStripMenuItem("À propos");
            
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
            
            helpMenu.DropDownItems.Add(aboutMenuItem);
            menu.Items.Add(helpMenu);
            Controls.Add(menu);

            // GroupBox for actions and profiles - DESIGN AMÉLIORÉ
            var grpActions = new GroupBox() { Text = "Actions", Left = 12, Top = 50, Width = 380, Height = 135, Padding = new Padding(0, 8, 0, 0) };
            lblProfile = new Label() { Text = "Profil de nettoyage", Left = 15, Top = 22, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbProfiles = new ComboBox() { Left = 15, Top = 42, Width = 340, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            btnDryRun = new Button() { Text = "🔍 Simuler", Left = 15, Top = 80, Width = 170, Height = 40, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnClean = new Button() { Text = "🧹 Nettoyer", Left = 195, Top = 80, Width = 170, Height = 40, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnCancel = new Button() { Text = "✖ Annuler", Left = 12, Top = 80, Width = 356, Height = 40, Enabled = false, Visible = false, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            grpActions.Controls.Add(lblProfile);
            grpActions.Controls.Add(cmbProfiles);
            grpActions.Controls.Add(btnDryRun);
            grpActions.Controls.Add(btnClean);
            grpActions.Controls.Add(btnCancel);
            btnCancel.BringToFront();
            Controls.Add(grpActions);

            // GroupBox for cleanup options - LARGEMENT AGRANDI
            var grpOptions = new GroupBox() { Text = "Nettoyage Standard", Left = 400, Top = 50, Width = 780, Height = 95, Padding = new Padding(0, 8, 0, 0) };
            chkRecycle = new CheckBox() { Text = "🗑️ Corbeille", Left = 15, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkSystemTemp = new CheckBox() { Text = "📁 Temp Système", Left = 205, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkBrowsers = new CheckBox() { Text = "🌐 Navigateurs", Left = 395, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkWindowsUpdate = new CheckBox() { Text = "🔄 Windows Update", Left = 585, Top = 30, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkThumbnails = new CheckBox() { Text = "🖼️ Vignettes", Left = 15, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkPrefetch = new CheckBox() { Text = "⚡ Prefetch", Left = 205, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkFlushDns = new CheckBox() { Text = "🔗 Flush DNS", Left = 395, Top = 60, Width = 180, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            grpOptions.Controls.Add(chkRecycle);
            grpOptions.Controls.Add(chkSystemTemp);
            grpOptions.Controls.Add(chkBrowsers);
            grpOptions.Controls.Add(chkWindowsUpdate);
            grpOptions.Controls.Add(chkThumbnails);
            grpOptions.Controls.Add(chkPrefetch);
            grpOptions.Controls.Add(chkFlushDns);
            Controls.Add(grpOptions);

            // Advanced options - MEILLEURE PRÉSENTATION
            var grpAdvanced = new GroupBox() { Text = "Options Avancées", Left = 12, Top = 200, Width = 1168, Height = 75, Padding = new Padding(0, 8, 0, 0) };
            chkVerbose = new CheckBox() { Text = "📝 Mode verbeux", Left = 15, Top = 30, Width = 250, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkAdvanced = new CheckBox() { Text = "📊 Rapport détaillé", Left = 280, Top = 30, Width = 250, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkOrphanedFiles = new CheckBox() { Text = "🧩 Fichiers orphelins", Left = 545, Top = 30, Width = 250, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            chkClearMemoryCache = new CheckBox() { Text = "💾 Cache mémoire", Left = 810, Top = 30, Width = 250, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 9.5f) };
            grpAdvanced.Controls.Add(chkVerbose);
            grpAdvanced.Controls.Add(chkAdvanced);
            grpAdvanced.Controls.Add(chkOrphanedFiles);
            grpAdvanced.Controls.Add(chkClearMemoryCache);
            Controls.Add(grpAdvanced);

            // Logs GroupBox - AUGMENTÉ
            var grpLogs = new GroupBox() { Text = "📋 Journal des Opérations", Left = 12, Top = 290, Width = 1168, Height = 440, Padding = new Padding(0, 8, 0, 0) };
            lvLogs = new ListView() { Left = 8, Top = 30, Width = 1152, Height = 402, View = View.Details, FullRowSelect = true, Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.None };
            lvLogs.Columns.Add("Heure", 160);
            lvLogs.Columns.Add("Niveau", 100);
            lvLogs.Columns.Add("Message", 892);
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
                            chk.CheckedChanged += OnOptionChanged;
                    }
                }
            }
            
            // Sauvegarder les options à la fermeture
            FormClosing += (s, e) => SaveOptions();

            // button visual polish - STYLE MODERNE
            foreach (var b in new[] { btnDryRun, btnClean, btnCancel })
            {
                if (b == null) continue;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 180);
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 230);
                b.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                b.Cursor = Cursors.Hand;
                b.BackColor = _accentColor;
                b.ForeColor = Color.White;
            }

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
                if (selected) textCol = Color.White;
                else if (e.SubItem.ForeColor != Color.Empty) textCol = e.SubItem.ForeColor;
                else textCol = _isDark ? Color.WhiteSmoke : Color.Black;

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
                        Logger.Log(LogLevel.Info, $"Ouverture dossier: {path}");
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"") { UseShellExecute = true });
                        Logger.Log(LogLevel.Info, $"Ouverture fichier: {path}");
                    }
                    else
                    {
                        MessageBox.Show($"Chemin introuvable dans le message: {path}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Aucun chemin de fichier ou dossier détecté dans ce message.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Erreur lors de l'ouverture: " + ex.Message);
            }
        }

        private void Logger_OnLog(DateTime ts, LogLevel level, string message)
        {
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
            var lvi = new ListViewItem(ts.ToString("yyyy-MM-dd HH:mm:ss"));
            lvi.SubItems.Add(level.ToString());
            lvi.SubItems.Add(message);
            // color by level
            switch (level)
            {
                case LogLevel.Debug:
                    lvi.ForeColor = Color.Gray;
                    break;
                case LogLevel.Info:
                    lvi.ForeColor = Color.Black;
                    break;
                case LogLevel.Warning:
                    lvi.ForeColor = Color.Orange;
                    break;
                case LogLevel.Error:
                    lvi.ForeColor = Color.Red;
                    break;
            }
            lvLogs.Items.Add(lvi);
            // keep bottom visible
            if (lvLogs.Items.Count > 0) lvLogs.EnsureVisible(lvLogs.Items.Count - 1);
            statusLabel.Text = $"Logs: {lvLogs.Items.Count}";
        }

        private void ClearLogsMenuItem_Click(object? sender, EventArgs e)
        {
            var res = MessageBox.Show("Effacer les logs sur disque et dans l'interface ?", "Confirmer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;
            try
            {
                Logger.Clear();
                lvLogs.Items.Clear();
                statusLabel.Text = "Logs effacés";
                MessageBox.Show("Logs effacés.", "Effacer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible d'effacer les logs: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadLogsMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var logFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "windows-cleaner.log");
                
                if (!System.IO.File.Exists(logFile))
                {
                    MessageBox.Show("Aucun fichier de log trouvé.", "Lire les logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Impossible d'ouvrir le fichier de log: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show($"Logs exportés vers {dest}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Échec de l'export des logs", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AboutMenuItem_Click(object? sender, EventArgs e)
        {
            // Author and MIT license (full text)
            var author = "Auteur : C.L";
            var licenseTitle = "Licence : MIT";
            var licenseText = @"MIT License

Copyright (c) 2025 C.L

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";

            var msg = $"Windows Cleaner\n\n{author}\n\n{licenseTitle}\n\n{licenseText}\n\nVersion: 1.0.6";
            MessageBox.Show(msg, "À propos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Cancel()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                statusLabel.Text = "Annulation en cours...";
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
            btnCancel.Top = 25;
            btnCancel.Height = 40;
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
                            Logger.Log(LogLevel.Info, $"Ouverture dossier: {val}");
                        }
                        else if (System.IO.File.Exists(val))
                        {
                            // select the file
                            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + val + "\"") { UseShellExecute = true });
                            Logger.Log(LogLevel.Info, $"Ouverture fichier: {val}");
                        }
                        else
                        {
                            MessageBox.Show($"Chemin introuvable: {val}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Impossible d'ouvrir l'élément: " + ex.Message);
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
                    catch (Exception ex) { Logger.Log(LogLevel.Error, "Ouvrir élément: " + ex.Message); }
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
                    catch (Exception ex) { Logger.Log(LogLevel.Error, "Copier chemin: " + ex.Message); }
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
                    catch (Exception ex) { Logger.Log(LogLevel.Error, "Ignorer élément: " + ex.Message); }
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
                    Logger.Log(LogLevel.Warning, "Opération avancée annulée par l'utilisateur (rapport). ");
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
                    if (options.IncludeSystemTemp) dangers.AppendLine("- Inclure Temp système");
                    if (options.CleanWindowsUpdate) dangers.AppendLine("- Nettoyer Windows Update (SoftwareDistribution\\Download)");
                    if (options.CleanPrefetch) dangers.AppendLine("- Nettoyer Prefetch");
                    if (options.EmptyRecycleBin) dangers.AppendLine("- Vider la Corbeille");

                    if (dangers.Length > 0)
                    {
                        var msg = "Vous êtes sur le point d'exécuter des opérations potentiellement dangereuses:\n\n" + dangers.ToString() + "\nContinuer ?";
                        var confirm = MessageBox.Show(msg, "Confirmer les opérations", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (confirm != DialogResult.Yes)
                        {
                            Logger.Log(LogLevel.Warning, "Opération annulée par l'utilisateur (confirmation refusée)");
                            statusLabel.Text = "Annulé (confirmation)";
                            btnClean.Enabled = true;
                            btnDryRun.Enabled = true;
                            btnCancel.Enabled = false;
                            btnCancel.Visible = false;
                            return;
                        }
                    }
                }

                Logger.Log(LogLevel.Info, $"Début du nettoyage ({(dryRun ? "dry-run" : "exécution")})...");

                // Run the cleaner in a Task and periodically update progress
                var task = Task.Run(() => Cleaner.RunCleanup(options, s => Logger.Log(LogLevel.Info, s)), token);

                while (!task.IsCompleted)
                {
                    if (token.IsCancellationRequested) break;
                    progressBar.Value = Math.Min(progressBar.Value + 5, 95);
                    await Task.Delay(200, token).ContinueWith(_ => { });
                }

                var result = await task;
                Logger.Log(LogLevel.Info, $"Terminé. Fichiers supprimés: {result.FilesDeleted}, Octets libérés: {result.BytesFreed}");
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
                        WasDryRun = false
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log(LogLevel.Warning, "Opération annulée par l'utilisateur.");
                statusLabel.Text = "Annulé";
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Erreur: " + ex.Message);
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
                _cts = null;
            }
        }

        private async void DiskAnalyzerMenuItem_Click(object? sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { Description = "Sélectionnez le dossier à analyser" };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            var path = dialog.SelectedPath;
            Logger.Log(LogLevel.Info, $"Démarrage de l'analyse du dossier : {path}");
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

                // Afficher les résultats
                var report = new System.Text.StringBuilder();
                report.AppendLine($"=== ANALYSE D'ESPACE DISQUE ===\n");
                report.AppendLine($"Dossier : {path}");
                report.AppendLine($"Fichiers totaux : {result.TotalScannedFiles:N0}");
                report.AppendLine($"Taille totale : {FormatBytes(result.TotalScannedSize)}\n");
                report.AppendLine("--- PAR CATÉGORIE ---");
                foreach (var cat in result.Categories.OrderByDescending(c => c.TotalSize))
                {
                    report.AppendLine($"{cat.Name,-20} : {cat.FormattedSize,15} ({cat.Percentage:F1}%)");
                }
                
                report.AppendLine("\n--- TOP 20 PLUS GROS FICHIERS ---");
                foreach (var file in result.LargestFiles.Take(20))
                {
                    report.AppendLine($"{FormatBytes(file.Size),15}  {file.Path}");
                }

                // Afficher dans une MessageBox avec possibilité de copier
                var resultForm = new Form
                {
                    Text = "Résultats de l'analyse",
                    Width = 900,
                    Height = 700,
                    StartPosition = FormStartPosition.CenterParent
                };
                var textBox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9),
                    Text = report.ToString(),
                    ReadOnly = true
                };
                resultForm.Controls.Add(textBox);
                resultForm.ShowDialog(this);

                Logger.Log(LogLevel.Info, $"Analyse terminée : {result.TotalScannedFiles} fichiers, {FormatBytes(result.TotalScannedSize)}");
                statusLabel.Text = "Analyse terminée";
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de l'analyse : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'analyse :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var dialog = new FolderBrowserDialog { Description = "Sélectionnez le dossier où chercher les doublons" };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            var path = dialog.SelectedPath;
            Logger.Log(LogLevel.Info, $"Recherche de doublons dans : {path}");
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

                if (duplicates.DuplicateGroups.Count == 0)
                {
                    MessageBox.Show("Aucun doublon trouvé !", "Résultat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Logger.Log(LogLevel.Info, "Aucun doublon trouvé");
                }
                else
                {
                    var report = new System.Text.StringBuilder();
                    report.AppendLine($"=== DOUBLONS DÉTECTÉS : {duplicates.DuplicateGroups.Count} groupes ===\n");
                    
                    long totalWasted = duplicates.TotalWastedSpace;
                    int groupNum = 1;
                    foreach (var group in duplicates.DuplicateGroups.Take(50)) // Limiter à 50 groupes pour l'affichage
                    {
                        report.AppendLine($"Groupe {groupNum++} - {FormatBytes(group.Files[0].Size)} - {group.Files.Count} copies :");
                        foreach (var file in group.Files)
                            report.AppendLine($"  • {file}");
                        report.AppendLine();
                    }
                    
                    if (duplicates.DuplicateGroups.Count > 50)
                        report.AppendLine($"... et {duplicates.DuplicateGroups.Count - 50} autres groupes");
                    
                    report.AppendLine($"\nEspace récupérable : {FormatBytes(totalWasted)}");

                    var resultForm = new Form
                    {
                        Text = $"Doublons détectés - {duplicates.DuplicateGroups.Count} groupes",
                        Width = 900,
                        Height = 700,
                        StartPosition = FormStartPosition.CenterParent
                    };
                    var textBox = new TextBox
                    {
                        Multiline = true,
                        ScrollBars = ScrollBars.Both,
                        Dock = DockStyle.Fill,
                        Font = new Font("Consolas", 9),
                        Text = report.ToString(),
                        ReadOnly = true
                    };
                    resultForm.Controls.Add(textBox);
                    resultForm.ShowDialog(this);

                    Logger.Log(LogLevel.Info, $"{duplicates.DuplicateGroups.Count} groupes de doublons trouvés, {FormatBytes(totalWasted)} récupérables");
                }
                
                statusLabel.Text = "Recherche terminée";
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Erreur lors de la recherche : {ex.Message}");
                MessageBox.Show($"Erreur :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                var report = new System.Text.StringBuilder();
                report.AppendLine($"=== STATISTIQUES DE NETTOYAGE ===\n");
                report.AppendLine($"Nombre de sessions : {allStats.Count}");
                report.AppendLine($"Espace total libéré : {FormatBytes(allStats.Sum(s => s.BytesFreed))}");
                report.AppendLine($"Fichiers totaux supprimés : {allStats.Sum(s => s.FilesDeleted):N0}\n");
                
                if (allStats.Count > 0)
                {
                    report.AppendLine("--- DERNIÈRES SESSIONS ---");
                    foreach (var session in allStats.OrderByDescending(s => s.Timestamp).Take(10))
                    {
                        report.AppendLine($"\n{session.Timestamp:yyyy-MM-dd HH:mm:ss}");
                        report.AppendLine($"  Profil : {session.ProfileUsed}");
                        report.AppendLine($"  Espace libéré : {FormatBytes(session.BytesFreed)}");
                        report.AppendLine($"  Fichiers supprimés : {session.FilesDeleted}");
                        report.AppendLine($"  Durée : {session.Duration:hh\\:mm\\:ss}");
                    }
                    
                    // Proposer d'exporter en HTML
                    report.AppendLine("\n\n--- EXPORT HTML DISPONIBLE ---");
                    report.AppendLine("Utilisez 'Générer rapport HTML' pour un rapport visuel complet avec graphiques.");
                }
                else
                {
                    report.AppendLine("Aucune statistique disponible. Effectuez un nettoyage pour commencer.");
                }

                var resultForm = new Form
                {
                    Text = "Statistiques",
                    Width = 800,
                    Height = 600,
                    StartPosition = FormStartPosition.CenterParent
                };
                
                var panel = new Panel { Dock = DockStyle.Fill };
                var textBox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9),
                    Text = report.ToString(),
                    ReadOnly = true
                };
                
                var btnExportHtml = new Button
                {
                    Text = "📊 Générer rapport HTML",
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                
                btnExportHtml.Click += (s, ev) =>
                {
                    try
                    {
                        var saveDialog = new SaveFileDialog
                        {
                            Filter = "Fichiers HTML|*.html",
                            FileName = $"statistics_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                        };
                        
                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            var html = StatisticsManager.GenerateHtmlReport();
                            System.IO.File.WriteAllText(saveDialog.FileName, html);
                            MessageBox.Show($"Rapport exporté :\n{saveDialog.FileName}", "Export réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // Ouvrir le fichier
                            if (MessageBox.Show("Ouvrir le rapport maintenant ?", "Ouvrir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de l'export :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                
                panel.Controls.Add(textBox);
                panel.Controls.Add(btnExportHtml);
                resultForm.Controls.Add(panel);
                resultForm.ShowDialog(this);
                
                Logger.Log(LogLevel.Info, "Statistiques affichées");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des statistiques :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(LogLevel.Error, $"Erreur statistiques : {ex.Message}");
            }
        }

        private void ProfilesMenuItem_Click(object? sender, EventArgs e)
        {
            var profiles = new[] { "Nettoyage Rapide", "Nettoyage Complet", "Nettoyage Développeur", "Protection Vie Privée" };
            
            var msg = "PROFILS PRÉDÉFINIS :\n\n";
            msg += "• Nettoyage Rapide - Usage quotidien rapide et sûr\n";
            msg += "• Nettoyage Complet - Maintenance approfondie mensuelle\n";
            msg += "• Nettoyage Développeur - Spécial projets de développement\n";
            msg += "• Protection Vie Privée - Effacement des traces\n\n";
            msg += "Pour utiliser un profil, lancez l'application en ligne de commande :\n";
            msg += "windows-cleaner.exe --profile \"Nettoyage Rapide\"";
            
            MessageBox.Show(msg, "Profils disponibles", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SchedulerMenuItem_Click(object? sender, EventArgs e)
        {
            var msg = "PLANIFICATION DE TÂCHES\n\n";
            msg += "Pour planifier un nettoyage automatique, utilisez la ligne de commande :\n\n";
            msg += "Exemple - Nettoyage quotidien à 2h du matin :\n";
            msg += "Créez une tâche Windows avec l'action :\n";
            msg += "  windows-cleaner.exe --profile \"Nettoyage Rapide\" --silent\n\n";
            msg += "Cette fonctionnalité nécessite des droits administrateur.";
            
            MessageBox.Show(msg, "Planification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BackupMenuItem_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Créer un point de restauration système maintenant ?\n\nCela peut prendre quelques minutes.", 
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
                    MessageBox.Show("Point de restauration créé avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Logger.Log(LogLevel.Info, "Point de restauration créé manuellement");
                }
                else
                {
                    MessageBox.Show("Échec de la création du point de restauration.\nVérifiez que la Protection Système est activée.", 
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(LogLevel.Error, $"Erreur point de restauration : {ex.Message}");
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
            var msg = "OPTIMISATIONS SYSTÈME DISPONIBLES :\n\n";
            msg += "• TRIM SSD - Optimise les disques SSD\n";
            msg += "• Compaction Registre - Réduit la fragmentation\n";
            msg += "• Nettoyage Mémoire Cache - Libère la RAM\n\n";
            msg += "Ces opérations nécessitent des droits administrateur.\n";
            msg += "Lancer les optimisations maintenant ?";
            
            if (MessageBox.Show(msg, "Optimisations système", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            statusLabel.Text = "Optimisations en cours...";
            progressBar.Value = 0;
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;

            try
            {
                var results = new System.Text.StringBuilder();
                
                Logger.Log(LogLevel.Info, "Démarrage des optimisations système");
                
                // TRIM SSD
                progressBar.Value = 10;
                statusLabel.Text = "Optimisation SSD (TRIM)...";
                var trimResult = await Task.Run(() => SystemOptimizer.OptimizeSsd());
                results.AppendLine($"TRIM SSD : {(trimResult ? "✓ Succès" : "✗ Échec")}");
                
                // Compaction registre
                progressBar.Value = 40;
                statusLabel.Text = "Compaction du registre...";
                var regResult = await Task.Run(() => SystemOptimizer.CompactRegistry());
                results.AppendLine($"Compaction Registre : {(regResult ? "✓ Succès" : "✗ Échec")}");
                
                // Nettoyage mémoire
                progressBar.Value = 70;
                statusLabel.Text = "Nettoyage mémoire cache...";
                var memResult = await Task.Run(() => SystemOptimizer.ClearStandbyMemory());
                results.AppendLine($"Nettoyage Mémoire : {(memResult ? "✓ Succès" : "✗ Échec")}");
                
                progressBar.Value = 100;
                
                MessageBox.Show($"Optimisations terminées :\n\n{results}", "Résultats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logger.Log(LogLevel.Info, "Optimisations système terminées");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(LogLevel.Error, $"Erreur optimisation : {ex.Message}");
                progressBar.Value = 0;
            }
            finally
            {
                statusLabel.Text = "Prêt";
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
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
