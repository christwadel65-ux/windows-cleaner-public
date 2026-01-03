using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WindowsCleaner
{
    /// <summary>
    /// Formulaire de configuration système (similaire à msconfig)
    /// </summary>
    public class SystemConfigForm : Form
    {
        private TabControl tabControl = null!;
        private TabPage tabStartup = null!;
        private TabPage tabServices = null!;
        private TabPage tabScheduledTasks = null!;
        private TabPage tabTools = null!;

        // Startup tab
        private ListView lvStartup = null!;
        private Button btnAddStartup = null!;
        private Button btnDisableStartup = null!;
        private Button btnRefreshStartup = null!;
        private TextBox txtSearchStartup = null!;
        private Label lblStartupCount = null!;

        // Services tab
        private ListView lvServices = null!;
        private Button btnStartService = null!;
        private Button btnStopService = null!;
        private Button btnRefreshServices = null!;
        private ComboBox cmbStartType = null!;
        private Button btnChangeStartType = null!;
        private TextBox txtSearchServices = null!;
        private Label lblServicesCount = null!;
        private ComboBox cmbFilterStatus = null!;

        // Scheduled tasks tab
        private ListView lvTasks = null!;
        private Button btnEnableTask = null!;
        private Button btnDisableTask = null!;
        private Button btnRefreshTasks = null!;
        private TextBox txtSearchTasks = null!;
        private Label lblTasksCount = null!;

        // Tools tab
        private Button btnOpenMsconfig = null!;
        private Button btnOpenTaskManager = null!;
        private Button btnOpenServices = null!;

        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        // Data storage for filtering
        private List<SystemConfigManager.StartupProgram> _allStartupPrograms = new List<SystemConfigManager.StartupProgram>();
        private List<SystemConfigManager.WindowsServiceInfo> _allServices = new List<SystemConfigManager.WindowsServiceInfo>();
        private List<SystemConfigManager.ScheduledTaskInfo> _allTasks = new List<SystemConfigManager.ScheduledTaskInfo>();

        // Sorting
        private int _lastSortColumn = -1;
        private SortOrder _lastSortOrder = SortOrder.None;

        // Debounce timers for search
        private System.Windows.Forms.Timer? _searchStartupTimer;
        private System.Windows.Forms.Timer? _searchServicesTimer;
        private System.Windows.Forms.Timer? _searchTasksTimer;

        public SystemConfigForm()
        {
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            Text = LanguageManager.Get("sysconfig_title");
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(800, 600);
            BackColor = Color.White;

            // Tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Point(20, 6),
                ItemSize = new Size(0, 35),
                SizeMode = TabSizeMode.Normal,
                DrawMode = TabDrawMode.OwnerDrawFixed
            };
            
            tabControl.DrawItem += TabControl_DrawItem;

            // Startup tab
            tabStartup = new TabPage(LanguageManager.Get("sysconfig_tab_startup"));
            tabStartup.BackColor = Color.FromArgb(250, 250, 250);
            InitializeStartupTab();

            // Services tab
            tabServices = new TabPage(LanguageManager.Get("sysconfig_tab_services"));
            tabServices.BackColor = Color.FromArgb(250, 250, 250);
            InitializeServicesTab();

            // Scheduled tasks tab
            tabScheduledTasks = new TabPage(LanguageManager.Get("sysconfig_tab_tasks"));
            tabScheduledTasks.BackColor = Color.FromArgb(250, 250, 250);
            InitializeScheduledTasksTab();

            // Tools tab
            tabTools = new TabPage(LanguageManager.Get("sysconfig_tab_tools"));
            tabTools.BackColor = Color.FromArgb(250, 250, 250);
            InitializeToolsTab();

            tabControl.TabPages.Add(tabStartup);
            tabControl.TabPages.Add(tabServices);
            tabControl.TabPages.Add(tabScheduledTasks);
            tabControl.TabPages.Add(tabTools);

            // Status bar
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel
            {
                Text = LanguageManager.Get("sysconfig_ready"),
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusStrip.Items.Add(statusLabel);

            Controls.Add(tabControl);
            Controls.Add(statusStrip);

            // Warning message if not admin
            if (!SystemConfigManager.IsAdministrator())
            {
                var lblWarning = new Label
                {
                    Text = "⚠️ " + LanguageManager.Get("sysconfig_warning_admin"),
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(255, 244, 204),
                    ForeColor = Color.FromArgb(133, 100, 4),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Height = 40,
                    Padding = new Padding(10)
                };
                Controls.Add(lblWarning);
                lblWarning.BringToFront();
            }
        }

        private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null) return;

            var tabPage = tabControl.TabPages[e.Index];
            var tabRect = tabControl.GetTabRect(e.Index);

            // Définir les couleurs par onglet
            Color accentColor;
            switch (e.Index)
            {
                case 0: // Startup Programs
                    accentColor = Color.FromArgb(0, 120, 215); // Bleu
                    break;
                case 1: // Services
                    accentColor = Color.FromArgb(16, 124, 16); // Vert
                    break;
                case 2: // Scheduled Tasks
                    accentColor = Color.FromArgb(202, 80, 16); // Orange
                    break;
                case 3: // System Tools
                    accentColor = Color.FromArgb(142, 68, 173); // Violet
                    break;
                default:
                    accentColor = Color.FromArgb(0, 120, 215);
                    break;
            }

            bool isSelected = e.Index == tabControl.SelectedIndex;

            // Couleur de fond
            Color backColor = isSelected ? Color.White : Color.FromArgb(240, 240, 240);
            Color textColor = isSelected ? accentColor : Color.FromArgb(80, 80, 80);

            // Dessiner le fond
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            // Dessiner une barre de couleur à gauche de l'onglet
            if (isSelected)
            {
                using (var pen = new Pen(accentColor, 3))
                {
                    e.Graphics.DrawLine(pen, 
                        tabRect.Left, tabRect.Top + 5, 
                        tabRect.Left, tabRect.Bottom - 5);
                }
            }

            // Dessiner le texte
            var textFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            var textRect = new Rectangle(
                tabRect.Left + (isSelected ? 8 : 5), 
                tabRect.Top, 
                tabRect.Width - (isSelected ? 8 : 5), 
                tabRect.Height
            );

            using (var textBrush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(tabPage.Text, tabControl.Font, textBrush, textRect, textFormat);
            }
        }

        #region Startup Tab

        private void InitializeStartupTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // Top search panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var lblSearch = new Label
            {
                Text = "🔍",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f),
                Location = new Point(5, 15)
            };

            txtSearchStartup = new TextBox
            {
                Location = new Point(35, 12),
                Width = 300,
                Font = new Font("Segoe UI", 10f),
                PlaceholderText = "Rechercher un programme..."
            };
            
            _searchStartupTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchStartupTimer.Tick += (s, e) =>
            {
                _searchStartupTimer.Stop();
                FilterStartupPrograms();
            };
            
            txtSearchStartup.TextChanged += (s, e) =>
            {
                _searchStartupTimer.Stop();
                _searchStartupTimer.Start();
            };

            lblStartupCount = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Location = new Point(350, 15)
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearchStartup);
            searchPanel.Controls.Add(lblStartupCount);

            lvStartup = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HeaderStyle = ColumnHeaderStyle.Clickable,
                OwnerDraw = true
            };
            
            lvStartup.DrawColumnHeader += (s, e) => DrawColoredColumnHeader(e, 0);
            lvStartup.DrawItem += (s, e) => e.DrawDefault = true;

            lvStartup.Columns.Add(LanguageManager.Get("sysconfig_col_name"), 280);
            lvStartup.Columns.Add(LanguageManager.Get("sysconfig_col_location"), 150);
            lvStartup.Columns.Add(LanguageManager.Get("sysconfig_col_command"), 420);
            lvStartup.Columns.Add(LanguageManager.Get("sysconfig_col_status"), 100);

            lvStartup.ColumnClick += (s, e) => SortListView(lvStartup, e.Column);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10, 10, 10, 10)
            };

            btnAddStartup = new Button
            {
                Text = "➕ Ajouter",
                Location = new Point(10, 10),
                Width = 150,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddStartup.FlatAppearance.BorderSize = 0;
            btnAddStartup.Click += BtnAddStartup_Click;

            btnDisableStartup = new Button
            {
                Text = "🚫 " + LanguageManager.Get("sysconfig_btn_disable"),
                Location = new Point(170, 10),
                Width = 150,
                Height = 40,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDisableStartup.FlatAppearance.BorderSize = 0;
            btnDisableStartup.Click += BtnDisableStartup_Click;

            var btnEnableStartup = new Button
            {
                Text = "✅ " + LanguageManager.Get("sysconfig_btn_enable"),
                Location = new Point(170, 10),
                Width = 150,
                Height = 40,
                Enabled = false,
                Visible = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEnableStartup.FlatAppearance.BorderSize = 0;
            btnEnableStartup.Click += BtnEnableStartup_Click;

            btnRefreshStartup = new Button
            {
                Text = "🔄 " + LanguageManager.Get("sysconfig_btn_refresh"),
                Location = new Point(330, 10),
                Width = 150,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshStartup.FlatAppearance.BorderSize = 0;
            btnRefreshStartup.Click += (s, e) => LoadStartupPrograms();

            var tooltip = new ToolTip();
            tooltip.SetToolTip(btnAddStartup, "Ajouter un programme au démarrage");
            tooltip.SetToolTip(btnDisableStartup, "Désactiver le programme sélectionné du démarrage");
            tooltip.SetToolTip(btnEnableStartup, "Activer le programme sélectionné du démarrage");
            tooltip.SetToolTip(btnRefreshStartup, "Actualiser la liste des programmes de démarrage");

            buttonPanel.Controls.Add(btnAddStartup);
            buttonPanel.Controls.Add(btnDisableStartup);
            buttonPanel.Controls.Add(btnEnableStartup);
            buttonPanel.Controls.Add(btnRefreshStartup);

            lvStartup.SelectedIndexChanged += (s, e) =>
            {
                if (lvStartup.SelectedItems.Count > 0)
                {
                    var program = lvStartup.SelectedItems[0].Tag as SystemConfigManager.StartupProgram;
                    if (program != null)
                    {
                        // Afficher le bon bouton selon le statut
                        btnDisableStartup.Visible = program.IsEnabled;
                        btnDisableStartup.Enabled = program.IsEnabled;
                        btnEnableStartup.Visible = !program.IsEnabled;
                        btnEnableStartup.Enabled = !program.IsEnabled;
                    }
                }
                else
                {
                    btnDisableStartup.Enabled = false;
                    btnEnableStartup.Enabled = false;
                }
            };

            panel.Controls.Add(buttonPanel);
            panel.Controls.Add(lvStartup);
            panel.Controls.Add(searchPanel);
            tabStartup.Controls.Add(panel);
        }

        private void LoadStartupPrograms()
        {
            lvStartup.Items.Clear();
            statusLabel.Text = LanguageManager.Get("sysconfig_loading");

            try
            {
                _allStartupPrograms = SystemConfigManager.GetStartupPrograms();
                FilterStartupPrograms();
                statusLabel.Text = LanguageManager.Get("sysconfig_loaded_programs", _allStartupPrograms.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_error_loading", ex.Message),
                    LanguageManager.Get("msgbox_error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                statusLabel.Text = LanguageManager.Get("sysconfig_error");
            }
        }

        private void FilterStartupPrograms()
        {
            var searchText = txtSearchStartup?.Text?.ToLower() ?? "";
            
            lvStartup.BeginUpdate();
            lvStartup.Items.Clear();
            
            var filtered = _allStartupPrograms.Where(p =>
                string.IsNullOrEmpty(searchText) ||
                p.Name.ToLower().Contains(searchText) ||
                p.Command.ToLower().Contains(searchText) ||
                p.Location.ToLower().Contains(searchText)
            ).ToList();

            foreach (var program in filtered)
            {
                var item = new ListViewItem(program.Name);
                item.SubItems.Add(program.Location);
                item.SubItems.Add(program.Command);
                item.SubItems.Add(program.IsEnabled ? LanguageManager.Get("sysconfig_enabled") : LanguageManager.Get("sysconfig_disabled"));
                item.Tag = program;
                lvStartup.Items.Add(item);
            }

            lvStartup.EndUpdate();

            if (lblStartupCount != null)
            {
                lblStartupCount.Text = $"{filtered.Count}/{_allStartupPrograms.Count} programmes";
            }
        }

        private void BtnAddStartup_Click(object? sender, EventArgs e)
        {
            // Créer un dialogue pour ajouter un programme
            var addForm = new Form
            {
                Text = "Ajouter un programme au démarrage",
                Width = 650,
                Height = 280,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblName = new Label
            {
                Text = "Nom du programme:",
                Location = new Point(20, 20),
                Width = 590,
                Font = new Font("Segoe UI", 10f)
            };

            var txtName = new TextBox
            {
                Location = new Point(20, 50),
                Width = 590,
                Font = new Font("Segoe UI", 10f)
            };

            var lblPath = new Label
            {
                Text = "Chemin du programme:",
                Location = new Point(20, 90),
                Width = 590,
                Font = new Font("Segoe UI", 10f)
            };

            var txtPath = new TextBox
            {
                Location = new Point(20, 120),
                Width = 490,
                Font = new Font("Segoe UI", 10f)
            };

            var btnBrowse = new Button
            {
                Text = "📁 Parcourir",
                Location = new Point(520, 118),
                Width = 90,
                Height = 27
            };

            btnBrowse.Click += (s, ev) =>
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Programmes exécutables (*.exe)|*.exe|Tous les fichiers (*.*)|*.*",
                    Title = "Sélectionner un programme"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = openDialog.FileName;
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        txtName.Text = System.IO.Path.GetFileNameWithoutExtension(openDialog.FileName);
                    }
                }
            };

            var chkAllUsers = new CheckBox
            {
                Text = "Ajouter pour tous les utilisateurs (nécessite admin)",
                Location = new Point(20, 165),
                Width = 590,
                Font = new Font("Segoe UI", 9f),
                Enabled = SystemConfigManager.IsAdministrator()
            };

            var btnOk = new Button
            {
                Text = "Ajouter",
                Location = new Point(420, 205),
                Width = 90,
                Height = 35,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(520, 205),
                Width = 90,
                Height = 35,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            addForm.Controls.Add(lblName);
            addForm.Controls.Add(txtName);
            addForm.Controls.Add(lblPath);
            addForm.Controls.Add(txtPath);
            addForm.Controls.Add(btnBrowse);
            addForm.Controls.Add(chkAllUsers);
            addForm.Controls.Add(btnOk);
            addForm.Controls.Add(btnCancel);

            addForm.AcceptButton = btnOk;
            addForm.CancelButton = btnCancel;

            if (addForm.ShowDialog(this) == DialogResult.OK)
            {
                var name = txtName.Text.Trim();
                var path = txtPath.Text.Trim();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                {
                    MessageBox.Show(
                        "Veuillez remplir tous les champs.",
                        "Champs requis",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (!System.IO.File.Exists(path))
                {
                    MessageBox.Show(
                        "Le fichier spécifié n'existe pas.",
                        "Fichier introuvable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                bool currentUser = !chkAllUsers.Checked;
                if (SystemConfigManager.AddStartupProgram(name, path, currentUser))
                {
                    MessageBox.Show(
                        $"Le programme '{name}' a été ajouté au démarrage avec succès.",
                        "Succès",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    LoadStartupPrograms();
                }
                else
                {
                    MessageBox.Show(
                        "Erreur lors de l'ajout du programme au démarrage.",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void BtnDisableStartup_Click(object? sender, EventArgs e)
        {
            if (lvStartup.SelectedItems.Count == 0)
                return;

            var item = lvStartup.SelectedItems[0];
            var program = item.Tag as SystemConfigManager.StartupProgram;

            if (program == null)
                return;

            var result = MessageBox.Show(
                LanguageManager.Get("sysconfig_confirm_disable_startup", program.Name),
                LanguageManager.Get("msgbox_confirmation"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                if (SystemConfigManager.DisableStartupProgram(program))
                {
                    MessageBox.Show(
                        LanguageManager.Get("sysconfig_success_disable"),
                        LanguageManager.Get("msgbox_success"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    LoadStartupPrograms();
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.Get("sysconfig_error_disable"),
                        LanguageManager.Get("msgbox_error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void BtnEnableStartup_Click(object? sender, EventArgs e)
        {
            if (lvStartup.SelectedItems.Count == 0)
                return;

            var item = lvStartup.SelectedItems[0];
            var program = item.Tag as SystemConfigManager.StartupProgram;

            if (program == null)
                return;

            if (SystemConfigManager.EnableStartupProgram(program))
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_success_enable"),
                    LanguageManager.Get("msgbox_success"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                LoadStartupPrograms();
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_error_enable"),
                    LanguageManager.Get("msgbox_error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        #endregion

        #region Services Tab

        private void InitializeServicesTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // Top search and filter panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var lblSearch = new Label
            {
                Text = "🔍",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f),
                Location = new Point(5, 15)
            };

            txtSearchServices = new TextBox
            {
                Location = new Point(35, 12),
                Width = 250,
                Font = new Font("Segoe UI", 10f),
                PlaceholderText = "Rechercher un service..."
            };
            
            _searchServicesTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchServicesTimer.Tick += (s, e) =>
            {
                _searchServicesTimer.Stop();
                FilterServices();
            };
            
            txtSearchServices.TextChanged += (s, e) =>
            {
                _searchServicesTimer.Stop();
                _searchServicesTimer.Start();
            };

            var lblFilter = new Label
            {
                Text = "Statut:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                Location = new Point(300, 15)
            };

            cmbFilterStatus = new ComboBox
            {
                Location = new Point(350, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cmbFilterStatus.Items.AddRange(new[] { "Tous", "Running", "Stopped" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => FilterServices();

            lblServicesCount = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Location = new Point(490, 15)
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearchServices);
            searchPanel.Controls.Add(lblFilter);
            searchPanel.Controls.Add(cmbFilterStatus);
            searchPanel.Controls.Add(lblServicesCount);

            lvServices = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HeaderStyle = ColumnHeaderStyle.Clickable,
                OwnerDraw = true
            };
            
            lvServices.DrawColumnHeader += (s, e) => DrawColoredColumnHeader(e, 1);
            lvServices.DrawItem += (s, e) => e.DrawDefault = true;

            lvServices.Columns.Add(LanguageManager.Get("sysconfig_col_service_name"), 200);
            lvServices.Columns.Add(LanguageManager.Get("sysconfig_col_display_name"), 250);
            lvServices.Columns.Add(LanguageManager.Get("sysconfig_col_status"), 100);
            lvServices.Columns.Add(LanguageManager.Get("sysconfig_col_start_type"), 120);
            lvServices.Columns.Add(LanguageManager.Get("sysconfig_col_description"), 300);

            lvServices.ColumnClick += (s, e) => SortListView(lvServices, e.Column);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10)
            };

            btnStartService = new Button
            {
                Text = "▶️ " + LanguageManager.Get("sysconfig_btn_start"),
                Location = new Point(10, 10),
                Width = 130,
                Height = 40,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStartService.FlatAppearance.BorderSize = 0;
            btnStartService.Click += BtnStartService_Click;

            btnStopService = new Button
            {
                Text = "⏹️ " + LanguageManager.Get("sysconfig_btn_stop"),
                Location = new Point(150, 10),
                Width = 130,
                Height = 40,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStopService.FlatAppearance.BorderSize = 0;
            btnStopService.Click += BtnStopService_Click;

            btnRefreshServices = new Button
            {
                Text = "🔄 " + LanguageManager.Get("sysconfig_btn_refresh"),
                Location = new Point(290, 10),
                Width = 130,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshServices.FlatAppearance.BorderSize = 0;
            btnRefreshServices.Click += (s, e) => LoadServices();

            var lblStartType = new Label
            {
                Text = LanguageManager.Get("sysconfig_lbl_start_type"),
                AutoSize = true,
                Location = new Point(10, 62),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            cmbStartType = new ComboBox
            {
                Location = new Point(120, 58),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false,
                Font = new Font("Segoe UI", 9f)
            };
            cmbStartType.Items.AddRange(new[] { "auto", "demand", "disabled" });
            cmbStartType.SelectedIndex = 0;

            btnChangeStartType = new Button
            {
                Text = "💾 " + LanguageManager.Get("sysconfig_btn_change_type"),
                Location = new Point(230, 55),
                Width = 190,
                Height = 35,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChangeStartType.FlatAppearance.BorderSize = 0;
            btnChangeStartType.Click += BtnChangeStartType_Click;

            var tooltip = new ToolTip();
            tooltip.SetToolTip(btnStartService, "Démarrer le service sélectionné");
            tooltip.SetToolTip(btnStopService, "Arrêter le service sélectionné");
            tooltip.SetToolTip(btnRefreshServices, "Actualiser la liste des services");

            buttonPanel.Controls.Add(btnStartService);
            buttonPanel.Controls.Add(btnStopService);
            buttonPanel.Controls.Add(btnRefreshServices);
            buttonPanel.Controls.Add(lblStartType);
            buttonPanel.Controls.Add(cmbStartType);
            buttonPanel.Controls.Add(btnChangeStartType);

            lvServices.SelectedIndexChanged += (s, e) =>
            {
                var hasSelection = lvServices.SelectedItems.Count > 0;
                btnStartService.Enabled = hasSelection;
                btnStopService.Enabled = hasSelection;
                cmbStartType.Enabled = hasSelection;
                btnChangeStartType.Enabled = hasSelection;
            };

            panel.Controls.Add(buttonPanel);
            panel.Controls.Add(lvServices);
            panel.Controls.Add(searchPanel);
            tabServices.Controls.Add(panel);
        }

        private void LoadServices()
        {
            lvServices.Items.Clear();
            statusLabel.Text = LanguageManager.Get("sysconfig_loading");

            Task.Run(() =>
            {
                try
                {
                    var services = SystemConfigManager.GetWindowsServices();
                    
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => UpdateServicesUI(services)));
                    }
                    else
                    {
                        UpdateServicesUI(services);
                    }
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show(
                                LanguageManager.Get("sysconfig_error_loading", ex.Message),
                                LanguageManager.Get("msgbox_error"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            statusLabel.Text = LanguageManager.Get("sysconfig_error");
                        }));
                    }
                }
            });
        }

        private void UpdateServicesUI(List<SystemConfigManager.WindowsServiceInfo> services)
        {
            _allServices = services;
            FilterServices();
        }

        private void FilterServices()
        {
            var searchText = txtSearchServices?.Text?.ToLower() ?? "";
            var statusFilter = cmbFilterStatus?.SelectedItem?.ToString() ?? "Tous";
            
            lvServices.BeginUpdate();
            lvServices.Items.Clear();
            
            var filtered = _allServices.Where(s =>
                (string.IsNullOrEmpty(searchText) ||
                 s.Name.ToLower().Contains(searchText) ||
                 s.DisplayName.ToLower().Contains(searchText)) &&
                (statusFilter == "Tous" || s.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            foreach (var service in filtered)
            {
                var item = new ListViewItem(service.Name);
                item.SubItems.Add(service.DisplayName);
                item.SubItems.Add(service.Status);
                item.SubItems.Add(service.StartType);
                item.SubItems.Add(service.Description);
                item.Tag = service;
                
                // Color coding for status
                if (service.Status == "Running")
                    item.ForeColor = Color.FromArgb(40, 167, 69);
                else if (service.Status == "Stopped")
                    item.ForeColor = Color.FromArgb(220, 53, 69);
                
                lvServices.Items.Add(item);
            }
            
            lvServices.EndUpdate();
            
            if (lblServicesCount != null)
            {
                lblServicesCount.Text = $"{filtered.Count}/{_allServices.Count} services";
            }
            statusLabel.Text = LanguageManager.Get("sysconfig_loaded_services", filtered.Count);
        }

        private void BtnStartService_Click(object? sender, EventArgs e)
        {
            if (lvServices.SelectedItems.Count == 0)
                return;

            var item = lvServices.SelectedItems[0];
            var service = item.Tag as SystemConfigManager.WindowsServiceInfo;

            if (service == null)
                return;

            statusLabel.Text = LanguageManager.Get("sysconfig_starting_service", service.DisplayName);

            if (SystemConfigManager.StartService(service.Name))
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_service_started"),
                    LanguageManager.Get("msgbox_success"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                LoadServices();
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_error_start_service"),
                    LanguageManager.Get("msgbox_error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnStopService_Click(object? sender, EventArgs e)
        {
            if (lvServices.SelectedItems.Count == 0)
                return;

            var item = lvServices.SelectedItems[0];
            var service = item.Tag as SystemConfigManager.WindowsServiceInfo;

            if (service == null)
                return;

            var result = MessageBox.Show(
                LanguageManager.Get("sysconfig_confirm_stop_service", service.DisplayName),
                LanguageManager.Get("msgbox_confirmation"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                statusLabel.Text = LanguageManager.Get("sysconfig_stopping_service", service.DisplayName);

                if (SystemConfigManager.StopService(service.Name))
                {
                    MessageBox.Show(
                        LanguageManager.Get("sysconfig_service_stopped"),
                        LanguageManager.Get("msgbox_success"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    LoadServices();
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.Get("sysconfig_error_stop_service"),
                        LanguageManager.Get("msgbox_error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void BtnChangeStartType_Click(object? sender, EventArgs e)
        {
            if (lvServices.SelectedItems.Count == 0 || cmbStartType.SelectedItem == null)
                return;

            var item = lvServices.SelectedItems[0];
            var service = item.Tag as SystemConfigManager.WindowsServiceInfo;

            if (service == null)
                return;

            var startType = cmbStartType.SelectedItem.ToString();

            if (SystemConfigManager.ChangeServiceStartType(service.Name, startType!))
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_start_type_changed"),
                    LanguageManager.Get("msgbox_success"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                LoadServices();
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_error_change_type"),
                    LanguageManager.Get("msgbox_error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        #endregion

        #region Scheduled Tasks Tab

        private void InitializeScheduledTasksTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // Top search panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var lblSearch = new Label
            {
                Text = "🔍",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f),
                Location = new Point(5, 15)
            };

            txtSearchTasks = new TextBox
            {
                Location = new Point(35, 12),
                Width = 300,
                Font = new Font("Segoe UI", 10f),
                PlaceholderText = "Rechercher une tâche..."
            };
            
            _searchTasksTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchTasksTimer.Tick += (s, e) =>
            {
                _searchTasksTimer.Stop();
                FilterTasks();
            };
            
            txtSearchTasks.TextChanged += (s, e) =>
            {
                _searchTasksTimer.Stop();
                _searchTasksTimer.Start();
            };

            lblTasksCount = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Location = new Point(350, 15)
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearchTasks);
            searchPanel.Controls.Add(lblTasksCount);

            lvTasks = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HeaderStyle = ColumnHeaderStyle.Clickable,
                OwnerDraw = true
            };
            
            lvTasks.DrawColumnHeader += (s, e) => DrawColoredColumnHeader(e, 2);
            lvTasks.DrawItem += (s, e) => e.DrawDefault = true;

            lvTasks.Columns.Add(LanguageManager.Get("sysconfig_col_task_name"), 350);
            lvTasks.Columns.Add(LanguageManager.Get("sysconfig_col_status"), 100);
            lvTasks.Columns.Add(LanguageManager.Get("sysconfig_col_next_run"), 170);
            lvTasks.Columns.Add(LanguageManager.Get("sysconfig_col_last_run"), 170);

            lvTasks.ColumnClick += (s, e) => SortListView(lvTasks, e.Column);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10, 10, 10, 10)
            };

            btnEnableTask = new Button
            {
                Text = "✅ " + LanguageManager.Get("sysconfig_btn_enable"),
                Location = new Point(10, 10),
                Width = 150,
                Height = 40,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEnableTask.FlatAppearance.BorderSize = 0;
            btnEnableTask.Click += BtnEnableTask_Click;

            btnDisableTask = new Button
            {
                Text = "🚫 " + LanguageManager.Get("sysconfig_btn_disable"),
                Location = new Point(170, 10),
                Width = 150,
                Height = 40,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDisableTask.FlatAppearance.BorderSize = 0;
            btnDisableTask.Click += BtnDisableTask_Click;

            btnRefreshTasks = new Button
            {
                Text = "🔄 " + LanguageManager.Get("sysconfig_btn_refresh"),
                Location = new Point(330, 10),
                Width = 150,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshTasks.FlatAppearance.BorderSize = 0;
            btnRefreshTasks.Click += (s, e) => LoadScheduledTasks();

            var tooltip = new ToolTip();
            tooltip.SetToolTip(btnEnableTask, "Activer la tâche sélectionnée");
            tooltip.SetToolTip(btnDisableTask, "Désactiver la tâche sélectionnée");
            tooltip.SetToolTip(btnRefreshTasks, "Actualiser la liste des tâches");

            buttonPanel.Controls.Add(btnEnableTask);
            buttonPanel.Controls.Add(btnDisableTask);
            buttonPanel.Controls.Add(btnRefreshTasks);

            lvTasks.SelectedIndexChanged += (s, e) =>
            {
                var hasSelection = lvTasks.SelectedItems.Count > 0;
                btnEnableTask.Enabled = hasSelection;
                btnDisableTask.Enabled = hasSelection;
            };

            panel.Controls.Add(buttonPanel);
            panel.Controls.Add(lvTasks);
            panel.Controls.Add(searchPanel);
            tabScheduledTasks.Controls.Add(panel);
        }

        private void LoadScheduledTasks()
        {
            lvTasks.Items.Clear();
            statusLabel.Text = LanguageManager.Get("sysconfig_loading");

            Task.Run(() =>
            {
                try
                {
                    var tasks = SystemConfigManager.GetScheduledTasks();
                    
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => UpdateTasksUI(tasks)));
                    }
                    else
                    {
                        UpdateTasksUI(tasks);
                    }
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show(
                                LanguageManager.Get("sysconfig_error_loading", ex.Message),
                                LanguageManager.Get("msgbox_error"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            statusLabel.Text = LanguageManager.Get("sysconfig_error");
                        }));
                    }
                }
            });
        }

        private void UpdateTasksUI(List<SystemConfigManager.ScheduledTaskInfo> tasks)
        {
            _allTasks = tasks;
            FilterTasks();
        }

        private void FilterTasks()
        {
            var searchText = txtSearchTasks?.Text?.ToLower() ?? "";
            
            lvTasks.BeginUpdate();
            lvTasks.Items.Clear();
            
            var filtered = _allTasks.Where(t =>
                string.IsNullOrEmpty(searchText) ||
                t.Name.ToLower().Contains(searchText) ||
                t.Path.ToLower().Contains(searchText)
            ).ToList();

            foreach (var task in filtered)
            {
                var item = new ListViewItem(task.Name);
                item.SubItems.Add(task.Status);  // Afficher directement sans traduction
                item.SubItems.Add(task.NextRunTime);
                item.SubItems.Add(task.LastRunTime);
                item.Tag = task;
                
                // Color coding for status
                if (task.Status.Equals("Ready", StringComparison.OrdinalIgnoreCase))
                    item.ForeColor = Color.FromArgb(40, 167, 69);
                else if (task.Status.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                    item.ForeColor = Color.Gray;
                
                lvTasks.Items.Add(item);
            }
            
            lvTasks.EndUpdate();
            
            if (lblTasksCount != null)
            {
                lblTasksCount.Text = $"{filtered.Count}/{_allTasks.Count} tâches";
            }
            statusLabel.Text = LanguageManager.Get("sysconfig_loaded_tasks", filtered.Count);
        }

        private void BtnEnableTask_Click(object? sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0)
                return;

            var item = lvTasks.SelectedItems[0];
            var task = item.Tag as SystemConfigManager.ScheduledTaskInfo;

            if (task == null)
                return;

            if (SystemConfigManager.EnableScheduledTask(task.Path))
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_task_enabled"),
                    LanguageManager.Get("msgbox_success"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                LoadScheduledTasks();
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.Get("sysconfig_error_enable_task"),
                    LanguageManager.Get("msgbox_error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnDisableTask_Click(object? sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0)
                return;

            var item = lvTasks.SelectedItems[0];
            var task = item.Tag as SystemConfigManager.ScheduledTaskInfo;

            if (task == null)
                return;

            var result = MessageBox.Show(
                LanguageManager.Get("sysconfig_confirm_disable_task", task.Name),
                LanguageManager.Get("msgbox_confirm"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                if (SystemConfigManager.DisableScheduledTask(task.Path))
                {
                    MessageBox.Show(
                        LanguageManager.Get("sysconfig_task_disabled"),
                        LanguageManager.Get("msgbox_success"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    LoadScheduledTasks();
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.Get("sysconfig_error_disable_task"),
                        LanguageManager.Get("msgbox_error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        #endregion

        #region Tools Tab

        private void InitializeToolsTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = LanguageManager.Get("sysconfig_tools_title"),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var lblDescription = new Label
            {
                Text = LanguageManager.Get("sysconfig_tools_description"),
                Font = new Font("Segoe UI", 10f),
                AutoSize = true,
                Location = new Point(20, 50)
            };

            btnOpenMsconfig = new Button
            {
                Text = "⚙️ " + LanguageManager.Get("sysconfig_btn_open_msconfig"),
                Location = new Point(20, 100),
                Width = 300,
                Height = 50,
                Font = new Font("Segoe UI", 11f)
            };
            btnOpenMsconfig.Click += (s, e) => SystemConfigManager.OpenMsconfig();

            btnOpenTaskManager = new Button
            {
                Text = "📊 " + LanguageManager.Get("sysconfig_btn_open_taskmgr"),
                Location = new Point(20, 160),
                Width = 300,
                Height = 50,
                Font = new Font("Segoe UI", 11f)
            };
            btnOpenTaskManager.Click += (s, e) => SystemConfigManager.OpenTaskManager();

            btnOpenServices = new Button
            {
                Text = "🔧 " + LanguageManager.Get("sysconfig_btn_open_services"),
                Location = new Point(20, 220),
                Width = 300,
                Height = 50,
                Font = new Font("Segoe UI", 11f)
            };
            btnOpenServices.Click += (s, e) => SystemConfigManager.OpenServicesManager();

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblDescription);
            panel.Controls.Add(btnOpenMsconfig);
            panel.Controls.Add(btnOpenTaskManager);
            panel.Controls.Add(btnOpenServices);

            tabTools.Controls.Add(panel);
        }

        #endregion

        private void LoadData()
        {
            LoadStartupPrograms();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Load services in background to avoid blocking UI
            Task.Run(() =>
            {
                try
                {
                    var services = SystemConfigManager.GetWindowsServices();
                    
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            UpdateServicesUI(services);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            statusLabel.Text = LanguageManager.Get("sysconfig_error");
                        }));
                    }
                }
            });

            // Load scheduled tasks in background
            Task.Run(() =>
            {
                try
                {
                    var tasks = SystemConfigManager.GetScheduledTasks();
                    
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            UpdateTasksUI(tasks);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            statusLabel.Text = LanguageManager.Get("sysconfig_error");
                        }));
                    }
                }
            });
        }

        #region Column Header Styling

        private void DrawColoredColumnHeader(DrawListViewColumnHeaderEventArgs e, int tabIndex)
        {
            // Couleurs d'accent par onglet
            Color accentColor;
            switch (tabIndex)
            {
                case 0: // Startup Programs
                    accentColor = Color.FromArgb(0, 120, 215); // Bleu
                    break;
                case 1: // Services
                    accentColor = Color.FromArgb(16, 124, 16); // Vert
                    break;
                case 2: // Scheduled Tasks
                    accentColor = Color.FromArgb(202, 80, 16); // Orange
                    break;
                default:
                    accentColor = Color.FromArgb(0, 120, 215);
                    break;
            }

            // Fond dégradé
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                e.Bounds,
                Color.FromArgb(250, 250, 250),
                Color.FromArgb(240, 240, 240),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Texte en couleur d'accent
            var textFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var textBrush = new SolidBrush(accentColor))
            {
                var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                e.Graphics.DrawString(e.Header.Text, font, textBrush, e.Bounds, textFormat);
            }

            // Ligne de séparation
            using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                e.Graphics.DrawLine(pen, 
                    e.Bounds.Right - 1, e.Bounds.Top, 
                    e.Bounds.Right - 1, e.Bounds.Bottom);
            }
            
            // Ligne colorée en bas
            using (var pen = new Pen(accentColor, 2))
            {
                e.Graphics.DrawLine(pen, 
                    e.Bounds.Left, e.Bounds.Bottom - 2, 
                    e.Bounds.Right, e.Bounds.Bottom - 2);
            }
        }

        #endregion

        #region Sorting

        private void SortListView(ListView listView, int column)
        {
            // Determine sort order
            if (column == _lastSortColumn)
            {
                // Toggle sort order if clicking same column
                _lastSortOrder = _lastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // New column, default to ascending
                _lastSortColumn = column;
                _lastSortOrder = SortOrder.Ascending;
            }

            // Create a comparer and sort
            listView.ListViewItemSorter = new ListViewItemComparer(column, _lastSortOrder);
            listView.Sort();
        }

        private class ListViewItemComparer : System.Collections.IComparer
        {
            private int _column;
            private SortOrder _order;

            public ListViewItemComparer(int column, SortOrder order)
            {
                _column = column;
                _order = order;
            }

            public int Compare(object? x, object? y)
            {
                if (x == null || y == null)
                    return 0;

                var itemX = x as ListViewItem;
                var itemY = y as ListViewItem;

                if (itemX == null || itemY == null)
                    return 0;

                string textX = _column < itemX.SubItems.Count ? itemX.SubItems[_column].Text : "";
                string textY = _column < itemY.SubItems.Count ? itemY.SubItems[_column].Text : "";

                int result = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);

                return _order == SortOrder.Ascending ? result : -result;
            }
        }

        #endregion
    }
}
