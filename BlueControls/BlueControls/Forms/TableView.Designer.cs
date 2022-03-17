
namespace BlueControls.Forms {
    partial class TableView {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ribMain = new BlueControls.Controls.RibbonBar();
            this.tabAdmin = new System.Windows.Forms.TabPage();
            this.grpAdminZeilen = new BlueControls.Controls.GroupBox();
            this.btnZeileLöschen = new BlueControls.Controls.Button();
            this.btnDatenüberprüfung = new BlueControls.Controls.Button();
            this.grpAdminImport = new BlueControls.Controls.GroupBox();
            this.btnClipboardImport = new BlueControls.Controls.Button();
            this.grpAdminBearbeiten = new BlueControls.Controls.GroupBox();
            this.btnPowerBearbeitung = new BlueControls.Controls.Button();
            this.btnSpaltenanordnung = new BlueControls.Controls.Button();
            this.btnLayouts = new BlueControls.Controls.Button();
            this.grpAdminAllgemein = new BlueControls.Controls.GroupBox();
            this.btnVorherigeVersion = new BlueControls.Controls.Button();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.btnSpaltenUebersicht = new BlueControls.Controls.Button();
            this.pnlDatabaseSelect = new System.Windows.Forms.Panel();
            this.tbcDatabaseSelector = new BlueControls.Controls.TabControl();
            this.pnlSerachBar = new System.Windows.Forms.Panel();
            this.FilterLeiste = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.Table = new BlueControls.Controls.Table();
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbcSidebar = new BlueControls.Controls.TabControl();
            this.tabFormula = new System.Windows.Forms.TabPage();
            this.Formula = new BlueControls.Controls.Formula();
            this.pnlStatusBar = new System.Windows.Forms.Panel();
            this.capStatusbar = new BlueControls.Controls.Caption();
            this.capZeilen2 = new BlueControls.Controls.Caption();
            this.ribMain.SuspendLayout();
            this.tabAdmin.SuspendLayout();
            this.grpAdminZeilen.SuspendLayout();
            this.grpAdminImport.SuspendLayout();
            this.grpAdminBearbeiten.SuspendLayout();
            this.grpAdminAllgemein.SuspendLayout();
            this.pnlDatabaseSelect.SuspendLayout();
            this.pnlSerachBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.tbcSidebar.SuspendLayout();
            this.tabFormula.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabAdmin);
            this.ribMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribMain.HotTrack = true;
            this.ribMain.Location = new System.Drawing.Point(0, 0);
            this.ribMain.Name = "ribMain";
            this.ribMain.SelectedIndex = 1;
            this.ribMain.Size = new System.Drawing.Size(1008, 110);
            this.ribMain.TabDefault = null;
            this.ribMain.TabDefaultOrder = null;
            this.ribMain.TabIndex = 93;
            // 
            // tabAdmin
            // 
            this.tabAdmin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAdmin.Controls.Add(this.grpAdminZeilen);
            this.tabAdmin.Controls.Add(this.grpAdminImport);
            this.tabAdmin.Controls.Add(this.grpAdminBearbeiten);
            this.tabAdmin.Controls.Add(this.grpAdminAllgemein);
            this.tabAdmin.Location = new System.Drawing.Point(4, 25);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdmin.Size = new System.Drawing.Size(1000, 81);
            this.tabAdmin.TabIndex = 0;
            this.tabAdmin.Text = "Administration";
            // 
            // grpAdminZeilen
            // 
            this.grpAdminZeilen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminZeilen.CausesValidation = false;
            this.grpAdminZeilen.Controls.Add(this.btnZeileLöschen);
            this.grpAdminZeilen.Controls.Add(this.btnDatenüberprüfung);
            this.grpAdminZeilen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminZeilen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminZeilen.Location = new System.Drawing.Point(504, 3);
            this.grpAdminZeilen.Name = "grpAdminZeilen";
            this.grpAdminZeilen.Size = new System.Drawing.Size(152, 75);
            this.grpAdminZeilen.TabIndex = 8;
            this.grpAdminZeilen.TabStop = false;
            this.grpAdminZeilen.Text = "Zeilen";
            // 
            // btnZeileLöschen
            // 
            this.btnZeileLöschen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZeileLöschen.ImageCode = "Zeile||||||||||Kreuz";
            this.btnZeileLöschen.Location = new System.Drawing.Point(72, 2);
            this.btnZeileLöschen.Name = "btnZeileLöschen";
            this.btnZeileLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            this.btnZeileLöschen.Size = new System.Drawing.Size(72, 66);
            this.btnZeileLöschen.TabIndex = 42;
            this.btnZeileLöschen.Text = "Zeilen löschen";
            this.btnZeileLöschen.Click += new System.EventHandler(this.btnZeileLöschen_Click);
            // 
            // btnDatenüberprüfung
            // 
            this.btnDatenüberprüfung.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnDatenüberprüfung.ImageCode = "Zeile||||||||||Häkchen";
            this.btnDatenüberprüfung.Location = new System.Drawing.Point(8, 2);
            this.btnDatenüberprüfung.Name = "btnDatenüberprüfung";
            this.btnDatenüberprüfung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnDatenüberprüfung.Size = new System.Drawing.Size(64, 66);
            this.btnDatenüberprüfung.TabIndex = 41;
            this.btnDatenüberprüfung.Text = "Datenüber-prüfung";
            this.btnDatenüberprüfung.Click += new System.EventHandler(this.btnDatenüberprüfung_Click);
            // 
            // grpAdminImport
            // 
            this.grpAdminImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminImport.CausesValidation = false;
            this.grpAdminImport.Controls.Add(this.btnClipboardImport);
            this.grpAdminImport.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminImport.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminImport.Location = new System.Drawing.Point(432, 3);
            this.grpAdminImport.Name = "grpAdminImport";
            this.grpAdminImport.Size = new System.Drawing.Size(72, 75);
            this.grpAdminImport.TabIndex = 10;
            this.grpAdminImport.TabStop = false;
            this.grpAdminImport.Text = "Import";
            // 
            // btnClipboardImport
            // 
            this.btnClipboardImport.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnClipboardImport.ImageCode = "Tabelle||||||||||Pfeil_Links";
            this.btnClipboardImport.Location = new System.Drawing.Point(8, 2);
            this.btnClipboardImport.Name = "btnClipboardImport";
            this.btnClipboardImport.Size = new System.Drawing.Size(56, 66);
            this.btnClipboardImport.TabIndex = 39;
            this.btnClipboardImport.Text = "Clipboard-Import";
            this.btnClipboardImport.Click += new System.EventHandler(this.btnClipboardImport_Click);
            // 
            // grpAdminBearbeiten
            // 
            this.grpAdminBearbeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminBearbeiten.CausesValidation = false;
            this.grpAdminBearbeiten.Controls.Add(this.btnPowerBearbeitung);
            this.grpAdminBearbeiten.Controls.Add(this.btnSpaltenanordnung);
            this.grpAdminBearbeiten.Controls.Add(this.btnLayouts);
            this.grpAdminBearbeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminBearbeiten.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminBearbeiten.Location = new System.Drawing.Point(211, 3);
            this.grpAdminBearbeiten.Name = "grpAdminBearbeiten";
            this.grpAdminBearbeiten.Size = new System.Drawing.Size(221, 75);
            this.grpAdminBearbeiten.TabIndex = 9;
            this.grpAdminBearbeiten.TabStop = false;
            this.grpAdminBearbeiten.Text = "Bearbeiten";
            // 
            // btnPowerBearbeitung
            // 
            this.btnPowerBearbeitung.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnPowerBearbeitung.ImageCode = "Stift||||FF0000||||||Uhr";
            this.btnPowerBearbeitung.Location = new System.Drawing.Point(136, 2);
            this.btnPowerBearbeitung.Name = "btnPowerBearbeitung";
            this.btnPowerBearbeitung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnPowerBearbeitung.Size = new System.Drawing.Size(72, 66);
            this.btnPowerBearbeitung.TabIndex = 43;
            this.btnPowerBearbeitung.Text = "Power-Bearbeitung";
            this.btnPowerBearbeitung.Click += new System.EventHandler(this.btnPowerBearbeitung_Click);
            // 
            // btnSpaltenanordnung
            // 
            this.btnSpaltenanordnung.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSpaltenanordnung.ImageCode = "Spalte||||||||||Stift";
            this.btnSpaltenanordnung.Location = new System.Drawing.Point(72, 2);
            this.btnSpaltenanordnung.Name = "btnSpaltenanordnung";
            this.btnSpaltenanordnung.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenanordnung.TabIndex = 43;
            this.btnSpaltenanordnung.Text = "Spalten-anordung";
            this.btnSpaltenanordnung.Click += new System.EventHandler(this.btnSpaltenanordnung_Click);
            // 
            // btnLayouts
            // 
            this.btnLayouts.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnLayouts.ImageCode = "Layout||||||||||Stift";
            this.btnLayouts.Location = new System.Drawing.Point(8, 2);
            this.btnLayouts.Name = "btnLayouts";
            this.btnLayouts.Size = new System.Drawing.Size(64, 66);
            this.btnLayouts.TabIndex = 41;
            this.btnLayouts.Text = "Layouts bearbeiten";
            this.btnLayouts.Click += new System.EventHandler(this.btnLayouts_Click);
            // 
            // grpAdminAllgemein
            // 
            this.grpAdminAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminAllgemein.CausesValidation = false;
            this.grpAdminAllgemein.Controls.Add(this.btnVorherigeVersion);
            this.grpAdminAllgemein.Controls.Add(this.btnDatenbankKopf);
            this.grpAdminAllgemein.Controls.Add(this.btnSpaltenUebersicht);
            this.grpAdminAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminAllgemein.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminAllgemein.Location = new System.Drawing.Point(3, 3);
            this.grpAdminAllgemein.Name = "grpAdminAllgemein";
            this.grpAdminAllgemein.Size = new System.Drawing.Size(208, 75);
            this.grpAdminAllgemein.TabIndex = 7;
            this.grpAdminAllgemein.TabStop = false;
            this.grpAdminAllgemein.Text = "Allgemein";
            // 
            // btnVorherigeVersion
            // 
            this.btnVorherigeVersion.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVorherigeVersion.ImageCode = "Uhr";
            this.btnVorherigeVersion.Location = new System.Drawing.Point(136, 2);
            this.btnVorherigeVersion.Name = "btnVorherigeVersion";
            this.btnVorherigeVersion.Size = new System.Drawing.Size(64, 66);
            this.btnVorherigeVersion.TabIndex = 42;
            this.btnVorherigeVersion.Text = "Vorherige Version";
            this.btnVorherigeVersion.Click += new System.EventHandler(this.btnVorherigeVersion_Click);
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnDatenbankKopf.ImageCode = "Datenbank";
            this.btnDatenbankKopf.Location = new System.Drawing.Point(8, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new System.Drawing.Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 37;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new System.EventHandler(this.btnDatenbankKopf_Click);
            // 
            // btnSpaltenUebersicht
            // 
            this.btnSpaltenUebersicht.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSpaltenUebersicht.ImageCode = "Spalte||||||||||Information";
            this.btnSpaltenUebersicht.Location = new System.Drawing.Point(72, 2);
            this.btnSpaltenUebersicht.Name = "btnSpaltenUebersicht";
            this.btnSpaltenUebersicht.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenUebersicht.TabIndex = 36;
            this.btnSpaltenUebersicht.Text = "Spalten-Übersicht";
            this.btnSpaltenUebersicht.Click += new System.EventHandler(this.btnSpaltenUebersicht_Click);
            // 
            // pnlDatabaseSelect
            // 
            this.pnlDatabaseSelect.Controls.Add(this.tbcDatabaseSelector);
            this.pnlDatabaseSelect.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDatabaseSelect.Location = new System.Drawing.Point(0, 0);
            this.pnlDatabaseSelect.Name = "pnlDatabaseSelect";
            this.pnlDatabaseSelect.Size = new System.Drawing.Size(739, 24);
            this.pnlDatabaseSelect.TabIndex = 21;
            // 
            // tbcDatabaseSelector
            // 
            this.tbcDatabaseSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcDatabaseSelector.HotTrack = true;
            this.tbcDatabaseSelector.Location = new System.Drawing.Point(0, 0);
            this.tbcDatabaseSelector.Name = "tbcDatabaseSelector";
            this.tbcDatabaseSelector.SelectedIndex = 0;
            this.tbcDatabaseSelector.Size = new System.Drawing.Size(739, 24);
            this.tbcDatabaseSelector.TabDefault = null;
            this.tbcDatabaseSelector.TabDefaultOrder = null;
            this.tbcDatabaseSelector.TabIndex = 20;
            // 
            // pnlSerachBar
            // 
            this.pnlSerachBar.Controls.Add(this.FilterLeiste);
            this.pnlSerachBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSerachBar.Location = new System.Drawing.Point(0, 24);
            this.pnlSerachBar.Name = "pnlSerachBar";
            this.pnlSerachBar.Size = new System.Drawing.Size(739, 40);
            this.pnlSerachBar.TabIndex = 24;
            // 
            // FilterLeiste
            // 
            this.FilterLeiste.ÄhnlicheAnsichtName = "Filterleiste Ähnlich";
            this.FilterLeiste.AnsichtName = "Filterleiste Waagerecht";
            this.FilterLeiste.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FilterLeiste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.FilterLeiste.CausesValidation = false;
            this.FilterLeiste.Dock = System.Windows.Forms.DockStyle.Top;
            this.FilterLeiste.Filtertypes = BlueControls.Enums.FilterTypesToShow.AktuelleAnsicht_AktiveFilter;
            this.FilterLeiste.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.FilterLeiste.Location = new System.Drawing.Point(0, 0);
            this.FilterLeiste.Name = "FilterLeiste";
            this.FilterLeiste.Size = new System.Drawing.Size(739, 40);
            this.FilterLeiste.TabIndex = 22;
            this.FilterLeiste.TabStop = false;
            // 
            // Table
            // 
            this.Table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Table.Location = new System.Drawing.Point(0, 64);
            this.Table.Name = "Table";
            this.Table.ShowWaitScreen = true;
            this.Table.Size = new System.Drawing.Size(739, 531);
            this.Table.TabIndex = 0;
            this.Table.CursorPosChanged += new System.EventHandler<BlueDatabase.EventArgs.CellExtEventArgs>(this.Table_CursorPosChanged);
            this.Table.DatabaseChanged += new System.EventHandler(this.TableView_DatabaseChanged);
            this.Table.VisibleRowsChanged += new System.EventHandler(this.Table_VisibleRowsChanged);
            this.Table.EnabledChanged += new System.EventHandler(this.TableView_EnabledChanged);
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer1.Location = new System.Drawing.Point(0, 110);
            this.SplitContainer1.Margin = new System.Windows.Forms.Padding(27, 10, 27, 25);
            this.SplitContainer1.Name = "SplitContainer1";
            // 
            // SplitContainer1.Panel1
            // 
            this.SplitContainer1.Panel1.Controls.Add(this.Table);
            this.SplitContainer1.Panel1.Controls.Add(this.pnlSerachBar);
            this.SplitContainer1.Panel1.Controls.Add(this.pnlDatabaseSelect);
            // 
            // SplitContainer1.Panel2
            // 
            this.SplitContainer1.Panel2.Controls.Add(this.tbcSidebar);
            this.SplitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(27, 25, 27, 25);
            this.SplitContainer1.Size = new System.Drawing.Size(1008, 595);
            this.SplitContainer1.SplitterDistance = 739;
            this.SplitContainer1.SplitterWidth = 11;
            this.SplitContainer1.TabIndex = 94;
            // 
            // tbcSidebar
            // 
            this.tbcSidebar.Controls.Add(this.tabFormula);
            this.tbcSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcSidebar.HotTrack = true;
            this.tbcSidebar.Location = new System.Drawing.Point(0, 0);
            this.tbcSidebar.Name = "tbcSidebar";
            this.tbcSidebar.SelectedIndex = 0;
            this.tbcSidebar.Size = new System.Drawing.Size(258, 595);
            this.tbcSidebar.TabDefault = null;
            this.tbcSidebar.TabDefaultOrder = null;
            this.tbcSidebar.TabIndex = 21;
            // 
            // tabFormula
            // 
            this.tabFormula.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabFormula.Controls.Add(this.Formula);
            this.tabFormula.Location = new System.Drawing.Point(4, 25);
            this.tabFormula.Name = "tabFormula";
            this.tabFormula.Padding = new System.Windows.Forms.Padding(3);
            this.tabFormula.Size = new System.Drawing.Size(250, 566);
            this.tabFormula.TabIndex = 0;
            this.tabFormula.Text = "Formular";
            // 
            // Formula
            // 
            this.Formula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Formula.Location = new System.Drawing.Point(3, 3);
            this.Formula.MinimumSize = new System.Drawing.Size(200, 200);
            this.Formula.Name = "Formula";
            this.Formula.Size = new System.Drawing.Size(244, 560);
            this.Formula.TabIndex = 0;
            this.Formula.Text = "Formula";
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Controls.Add(this.capStatusbar);
            this.pnlStatusBar.Controls.Add(this.capZeilen2);
            this.pnlStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 705);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Size = new System.Drawing.Size(1008, 24);
            this.pnlStatusBar.TabIndex = 95;
            // 
            // capStatusbar
            // 
            this.capStatusbar.CausesValidation = false;
            this.capStatusbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capStatusbar.Location = new System.Drawing.Point(304, 0);
            this.capStatusbar.Name = "capStatusbar";
            this.capStatusbar.Size = new System.Drawing.Size(704, 24);
            this.capStatusbar.Translate = false;
            // 
            // capZeilen2
            // 
            this.capZeilen2.CausesValidation = false;
            this.capZeilen2.Dock = System.Windows.Forms.DockStyle.Left;
            this.capZeilen2.Location = new System.Drawing.Point(0, 0);
            this.capZeilen2.Name = "capZeilen2";
            this.capZeilen2.Size = new System.Drawing.Size(304, 24);
            this.capZeilen2.Translate = false;
            // 
            // TableView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.SplitContainer1);
            this.Controls.Add(this.ribMain);
            this.Controls.Add(this.pnlStatusBar);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "TableView";
            this.Text = "TableViewx";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ribMain.ResumeLayout(false);
            this.tabAdmin.ResumeLayout(false);
            this.grpAdminZeilen.ResumeLayout(false);
            this.grpAdminImport.ResumeLayout(false);
            this.grpAdminBearbeiten.ResumeLayout(false);
            this.grpAdminAllgemein.ResumeLayout(false);
            this.pnlDatabaseSelect.ResumeLayout(false);
            this.pnlSerachBar.ResumeLayout(false);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            this.tbcSidebar.ResumeLayout(false);
            this.tabFormula.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected Controls.RibbonBar ribMain;
        protected System.Windows.Forms.Panel pnlDatabaseSelect;
        protected Controls.TabControl tbcDatabaseSelector;
        protected System.Windows.Forms.Panel pnlSerachBar;
        protected BlueDatabaseDialogs.Filterleiste FilterLeiste;
        protected Controls.Table Table;
        protected System.Windows.Forms.SplitContainer SplitContainer1;
        protected System.Windows.Forms.TabPage tabAdmin;
        private Controls.Button btnZeileLöschen;
        private Controls.Button btnDatenüberprüfung;
        private Controls.Button btnPowerBearbeitung;
        internal Controls.Button btnSpaltenanordnung;
        private Controls.Button btnLayouts;
        private Controls.Button btnVorherigeVersion;
        private Controls.Button btnDatenbankKopf;
        internal Controls.Button btnSpaltenUebersicht;
        private Controls.Button btnClipboardImport;
        protected Controls.GroupBox grpAdminZeilen;
        protected Controls.GroupBox grpAdminBearbeiten;
        protected Controls.GroupBox grpAdminAllgemein;
        protected Controls.GroupBox grpAdminImport;
        protected Controls.TabControl tbcSidebar;
        protected System.Windows.Forms.TabPage tabFormula;
        private System.Windows.Forms.Panel pnlStatusBar;
        public Controls.Caption capStatusbar;
        private Controls.Caption capZeilen2;
        protected Controls.Formula Formula;
    }
}