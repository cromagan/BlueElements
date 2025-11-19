using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;

namespace BlueControls.Forms {
    partial class TableViewForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                components?.Dispose();
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
            this.tabFile = new System.Windows.Forms.TabPage();
            this.grpOrdner = new BlueControls.Controls.GroupBox();
            this.btnTabellenSpeicherort = new BlueControls.Controls.Button();
            this.btnTemporärenSpeicherortÖffnen = new BlueControls.Controls.Button();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnLetzteDateien = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSaveAs = new BlueControls.Controls.Button();
            this.btnNeuDB = new BlueControls.Controls.Button();
            this.tabAllgemein = new System.Windows.Forms.TabPage();
            this.grpAufgaben = new BlueControls.Controls.GroupBox();
            this.lstAufgaben = new BlueControls.Controls.ListBox();
            this.grpAnsicht = new BlueControls.Controls.GroupBox();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.btnZoomOut = new BlueControls.Controls.Button();
            this.btnZoomIn = new BlueControls.Controls.Button();
            this.btnUnterschiede = new BlueControls.Controls.Button();
            this.btnAlleSchließen = new BlueControls.Controls.Button();
            this.btnAlleErweitern = new BlueControls.Controls.Button();
            this.capSpaltenanordnung = new BlueControls.Controls.Caption();
            this.capZeilen1 = new BlueControls.Controls.Caption();
            this.cbxColumnArr = new BlueControls.Controls.ComboBox();
            this.grpHilfen = new BlueControls.Controls.GroupBox();
            this.ckbZeilenclickInsClipboard = new BlueControls.Controls.Button();
            this.btnNummerierung = new BlueControls.Controls.Button();
            this.btnSuchFenster = new BlueControls.Controls.Button();
            this.tabAdmin = new System.Windows.Forms.TabPage();
            this.grpAdminZeilen = new BlueControls.Controls.GroupBox();
            this.btnAufräumen = new BlueControls.Controls.Button();
            this.btnSuchenUndErsetzen = new BlueControls.Controls.Button();
            this.btnZeileLöschen = new BlueControls.Controls.Button();
            this.grpAdminAllgemein = new BlueControls.Controls.GroupBox();
            this.btnMonitoring = new BlueControls.Controls.Button();
            this.btnUserInfo = new BlueControls.Controls.Button();
            this.btnSaveLoad = new BlueControls.Controls.Button();
            this.btnPowerBearbeitung = new BlueControls.Controls.Button();
            this.btnSpaltenUebersicht = new BlueControls.Controls.Button();
            this.grpAdminBearbeiten = new BlueControls.Controls.GroupBox();
            this.btnSuchInScript = new BlueControls.Controls.Button();
            this.btnSkripteBearbeiten = new BlueControls.Controls.Button();
            this.btnFormular = new BlueControls.Controls.Button();
            this.btnSpaltenanordnung = new BlueControls.Controls.Button();
            this.btnTabelleKopf = new BlueControls.Controls.Button();
            this.btnLayouts = new BlueControls.Controls.Button();
            this.tabExport = new System.Windows.Forms.TabPage();
            this.grpExport = new BlueControls.Controls.GroupBox();
            this.btnDrucken = new BlueControls.Controls.ComboBox();
            this.btnHTMLExport = new BlueControls.Controls.Button();
            this.btnCSVClipboard = new BlueControls.Controls.Button();
            this.grpImport = new BlueControls.Controls.GroupBox();
            this.btnMDBImport = new BlueControls.Controls.Button();
            this.btnClipboardImport = new BlueControls.Controls.Button();
            this.pnlTableSelect = new System.Windows.Forms.Panel();
            this.tbcTableSelector = new BlueControls.Controls.TabControl();
            this.Table = new BlueControls.Controls.TableView();
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbcSidebar = new BlueControls.Controls.TabControl();
            this.tabFormula = new System.Windows.Forms.TabPage();
            this.CFO = new BlueControls.Controls.ConnectedFormulaView();
            this.capZeilen2 = new BlueControls.Controls.Caption();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.grpAufräumen = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabFile.SuspendLayout();
            this.grpOrdner.SuspendLayout();
            this.grpDatei.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.grpAufgaben.SuspendLayout();
            this.grpAnsicht.SuspendLayout();
            this.grpHilfen.SuspendLayout();
            this.tabAdmin.SuspendLayout();
            this.grpAdminZeilen.SuspendLayout();
            this.grpAdminAllgemein.SuspendLayout();
            this.grpAdminBearbeiten.SuspendLayout();
            this.tabExport.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.grpImport.SuspendLayout();
            this.pnlTableSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.tbcSidebar.SuspendLayout();
            this.tabFormula.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Location = new System.Drawing.Point(304, 0);
            this.capStatusBar.Size = new System.Drawing.Size(1025, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Controls.Add(this.capZeilen2);
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 705);
            this.pnlStatusBar.Size = new System.Drawing.Size(1329, 24);
            this.pnlStatusBar.Controls.SetChildIndex(this.capZeilen2, 0);
            this.pnlStatusBar.Controls.SetChildIndex(this.capStatusBar, 0);
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabFile);
            this.ribMain.Controls.Add(this.tabAllgemein);
            this.ribMain.Controls.Add(this.tabAdmin);
            this.ribMain.Controls.Add(this.tabExport);
            this.ribMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribMain.HotTrack = true;
            this.ribMain.Location = new System.Drawing.Point(0, 0);
            this.ribMain.Name = "ribMain";
            this.ribMain.SelectedIndex = 1;
            this.ribMain.Size = new System.Drawing.Size(1329, 110);
            this.ribMain.TabDefault = this.tabFile;
            this.ribMain.TabDefaultOrder = null;
            this.ribMain.TabIndex = 93;
            // 
            // tabFile
            // 
            this.tabFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabFile.Controls.Add(this.grpOrdner);
            this.tabFile.Controls.Add(this.grpDatei);
            this.tabFile.Location = new System.Drawing.Point(4, 25);
            this.tabFile.Margin = new System.Windows.Forms.Padding(0);
            this.tabFile.Name = "tabFile";
            this.tabFile.Size = new System.Drawing.Size(1321, 81);
            this.tabFile.TabIndex = 3;
            this.tabFile.Text = "Datei";
            // 
            // grpOrdner
            // 
            this.grpOrdner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpOrdner.CausesValidation = false;
            this.grpOrdner.Controls.Add(this.btnTabellenSpeicherort);
            this.grpOrdner.Controls.Add(this.btnTemporärenSpeicherortÖffnen);
            this.grpOrdner.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpOrdner.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpOrdner.Location = new System.Drawing.Point(304, 0);
            this.grpOrdner.Name = "grpOrdner";
            this.grpOrdner.Size = new System.Drawing.Size(184, 81);
            this.grpOrdner.TabIndex = 3;
            this.grpOrdner.TabStop = false;
            this.grpOrdner.Text = "Ordner";
            // 
            // btnTabellenSpeicherort
            // 
            this.btnTabellenSpeicherort.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTabellenSpeicherort.ImageCode = "Ordner";
            this.btnTabellenSpeicherort.Location = new System.Drawing.Point(8, 2);
            this.btnTabellenSpeicherort.Name = "btnTabellenSpeicherort";
            this.btnTabellenSpeicherort.QuickInfo = "Speicherort der Tabellen öffnen";
            this.btnTabellenSpeicherort.Size = new System.Drawing.Size(88, 66);
            this.btnTabellenSpeicherort.TabIndex = 27;
            this.btnTabellenSpeicherort.Text = "Tabellen-Pfad";
            this.btnTabellenSpeicherort.Click += new System.EventHandler(this.btnTabellenSpeicherort_Click);
            // 
            // btnTemporärenSpeicherortÖffnen
            // 
            this.btnTemporärenSpeicherortÖffnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTemporärenSpeicherortÖffnen.ImageCode = "Ordner||||0000ff||126";
            this.btnTemporärenSpeicherortÖffnen.Location = new System.Drawing.Point(96, 2);
            this.btnTemporärenSpeicherortÖffnen.Name = "btnTemporärenSpeicherortÖffnen";
            this.btnTemporärenSpeicherortÖffnen.QuickInfo = "Temporären Speicherort öffnen";
            this.btnTemporärenSpeicherortÖffnen.Size = new System.Drawing.Size(80, 66);
            this.btnTemporärenSpeicherortÖffnen.TabIndex = 26;
            this.btnTemporärenSpeicherortÖffnen.Text = "Temporärer Speicherort";
            this.btnTemporärenSpeicherortÖffnen.Click += new System.EventHandler(this.btnTemporärenSpeicherortÖffnen_Click);
            // 
            // grpDatei
            // 
            this.grpDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Controls.Add(this.btnLetzteDateien);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Controls.Add(this.btnSaveAs);
            this.grpDatei.Controls.Add(this.btnNeuDB);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(304, 81);
            this.grpDatei.TabIndex = 4;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
            // 
            // btnLetzteDateien
            // 
            this.btnLetzteDateien.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLetzteDateien.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnLetzteDateien.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLetzteDateien.Enabled = false;
            this.btnLetzteDateien.ImageCode = "Ordner";
            this.btnLetzteDateien.Location = new System.Drawing.Point(128, 2);
            this.btnLetzteDateien.Name = "btnLetzteDateien";
            this.btnLetzteDateien.SettingsLoaded = false;
            this.btnLetzteDateien.Size = new System.Drawing.Size(104, 66);
            this.btnLetzteDateien.TabIndex = 1;
            this.btnLetzteDateien.Text = "zuletzt geöffnete Dateien";
            this.btnLetzteDateien.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnLetzteDateien_ItemClicked);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(72, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(56, 66);
            this.btnOeffnen.TabIndex = 1;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSaveAs.ImageCode = "Diskette";
            this.btnSaveAs.Location = new System.Drawing.Point(232, 2);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(64, 66);
            this.btnSaveAs.TabIndex = 4;
            this.btnSaveAs.Text = "Speichern unter";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // btnNeuDB
            // 
            this.btnNeuDB.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnNeuDB.ImageCode = "Datei";
            this.btnNeuDB.Location = new System.Drawing.Point(8, 2);
            this.btnNeuDB.Name = "btnNeuDB";
            this.btnNeuDB.Size = new System.Drawing.Size(56, 66);
            this.btnNeuDB.TabIndex = 0;
            this.btnNeuDB.Text = "Neu";
            this.btnNeuDB.Click += new System.EventHandler(this.btnNeuDB_Click);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAllgemein.Controls.Add(this.grpAufgaben);
            this.tabAllgemein.Controls.Add(this.grpAnsicht);
            this.tabAllgemein.Controls.Add(this.grpHilfen);
            this.tabAllgemein.Location = new System.Drawing.Point(4, 25);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Size = new System.Drawing.Size(1321, 81);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // grpAufgaben
            // 
            this.grpAufgaben.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAufgaben.CausesValidation = false;
            this.grpAufgaben.Controls.Add(this.lstAufgaben);
            this.grpAufgaben.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAufgaben.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAufgaben.Location = new System.Drawing.Point(640, 0);
            this.grpAufgaben.Name = "grpAufgaben";
            this.grpAufgaben.Size = new System.Drawing.Size(222, 81);
            this.grpAufgaben.TabIndex = 7;
            this.grpAufgaben.TabStop = false;
            this.grpAufgaben.Text = "Aufgaben";
            // 
            // lstAufgaben
            // 
            this.lstAufgaben.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstAufgaben.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAufgaben.AutoSort = false;
            this.lstAufgaben.CheckBehavior = BlueControls.Enums.CheckBehavior.NoSelection;
            this.lstAufgaben.Location = new System.Drawing.Point(8, 2);
            this.lstAufgaben.Name = "lstAufgaben";
            this.lstAufgaben.Size = new System.Drawing.Size(208, 66);
            this.lstAufgaben.TabIndex = 0;
            // 
            // grpAnsicht
            // 
            this.grpAnsicht.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAnsicht.CausesValidation = false;
            this.grpAnsicht.Controls.Add(this.btnZoomFit);
            this.grpAnsicht.Controls.Add(this.btnZoomOut);
            this.grpAnsicht.Controls.Add(this.btnZoomIn);
            this.grpAnsicht.Controls.Add(this.btnUnterschiede);
            this.grpAnsicht.Controls.Add(this.btnAlleSchließen);
            this.grpAnsicht.Controls.Add(this.btnAlleErweitern);
            this.grpAnsicht.Controls.Add(this.capSpaltenanordnung);
            this.grpAnsicht.Controls.Add(this.capZeilen1);
            this.grpAnsicht.Controls.Add(this.cbxColumnArr);
            this.grpAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsicht.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAnsicht.Location = new System.Drawing.Point(256, 0);
            this.grpAnsicht.Margin = new System.Windows.Forms.Padding(0);
            this.grpAnsicht.Name = "grpAnsicht";
            this.grpAnsicht.Padding = new System.Windows.Forms.Padding(0);
            this.grpAnsicht.Size = new System.Drawing.Size(384, 81);
            this.grpAnsicht.TabIndex = 3;
            this.grpAnsicht.TabStop = false;
            this.grpAnsicht.Text = "Ansicht";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZoomFit.ImageCode = "ZoomFit|20";
            this.btnZoomFit.Location = new System.Drawing.Point(328, 24);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new System.Drawing.Size(48, 22);
            this.btnZoomFit.TabIndex = 17;
            this.btnZoomFit.Text = "1:1";
            this.btnZoomFit.Click += new System.EventHandler(this.btnZoomFit_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZoomOut.ImageCode = "LupeMinus|20";
            this.btnZoomOut.Location = new System.Drawing.Point(352, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(24, 22);
            this.btnZoomOut.TabIndex = 16;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZoomIn.ImageCode = "LupePlus|20";
            this.btnZoomIn.Location = new System.Drawing.Point(328, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(24, 22);
            this.btnZoomIn.TabIndex = 15;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnUnterschiede
            // 
            this.btnUnterschiede.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Big_Borderless;
            this.btnUnterschiede.Location = new System.Drawing.Point(216, 46);
            this.btnUnterschiede.Name = "btnUnterschiede";
            this.btnUnterschiede.QuickInfo = "Zeigt Unterschiede zur gewählten Zeile an";
            this.btnUnterschiede.Size = new System.Drawing.Size(104, 22);
            this.btnUnterschiede.TabIndex = 14;
            this.btnUnterschiede.Text = "Unterschiede";
            this.btnUnterschiede.CheckedChanged += new System.EventHandler(this.btnUnterschiede_CheckedChanged);
            // 
            // btnAlleSchließen
            // 
            this.btnAlleSchließen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAlleSchließen.ImageCode = "Pfeil_Oben_Scrollbar|14|||||0";
            this.btnAlleSchließen.Location = new System.Drawing.Point(216, 24);
            this.btnAlleSchließen.Name = "btnAlleSchließen";
            this.btnAlleSchließen.QuickInfo = "Neuen Eintrag ergänzen";
            this.btnAlleSchließen.Size = new System.Drawing.Size(104, 22);
            this.btnAlleSchließen.TabIndex = 4;
            this.btnAlleSchließen.Text = "alle schließen";
            this.btnAlleSchließen.Click += new System.EventHandler(this.btnAlleSchließen_Click);
            // 
            // btnAlleErweitern
            // 
            this.btnAlleErweitern.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAlleErweitern.ImageCode = "Pfeil_Unten_Scrollbar|14|||ff0000||200|200";
            this.btnAlleErweitern.Location = new System.Drawing.Point(216, 2);
            this.btnAlleErweitern.Name = "btnAlleErweitern";
            this.btnAlleErweitern.QuickInfo = "Neuen Eintrag ergänzen";
            this.btnAlleErweitern.Size = new System.Drawing.Size(104, 22);
            this.btnAlleErweitern.TabIndex = 3;
            this.btnAlleErweitern.Text = "alle erweitern";
            this.btnAlleErweitern.Click += new System.EventHandler(this.btnAlleErweitern_Click);
            // 
            // capSpaltenanordnung
            // 
            this.capSpaltenanordnung.CausesValidation = false;
            this.capSpaltenanordnung.Location = new System.Drawing.Point(8, 2);
            this.capSpaltenanordnung.Margin = new System.Windows.Forms.Padding(4);
            this.capSpaltenanordnung.Name = "capSpaltenanordnung";
            this.capSpaltenanordnung.Size = new System.Drawing.Size(200, 22);
            this.capSpaltenanordnung.Text = "<u>Spaltenanordnung:";
            // 
            // capZeilen1
            // 
            this.capZeilen1.CausesValidation = false;
            this.capZeilen1.Location = new System.Drawing.Point(8, 46);
            this.capZeilen1.Margin = new System.Windows.Forms.Padding(4);
            this.capZeilen1.Name = "capZeilen1";
            this.capZeilen1.Size = new System.Drawing.Size(200, 22);
            this.capZeilen1.Text = "<imagecode=Information|16>";
            this.capZeilen1.Translate = false;
            // 
            // cbxColumnArr
            // 
            this.cbxColumnArr.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumnArr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColumnArr.Location = new System.Drawing.Point(8, 24);
            this.cbxColumnArr.Margin = new System.Windows.Forms.Padding(4);
            this.cbxColumnArr.Name = "cbxColumnArr";
            this.cbxColumnArr.Size = new System.Drawing.Size(200, 22);
            this.cbxColumnArr.TabIndex = 2;
            this.cbxColumnArr.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.cbxColumnArr_ItemClicked);
            // 
            // grpHilfen
            // 
            this.grpHilfen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpHilfen.CausesValidation = false;
            this.grpHilfen.Controls.Add(this.ckbZeilenclickInsClipboard);
            this.grpHilfen.Controls.Add(this.btnNummerierung);
            this.grpHilfen.Controls.Add(this.btnSuchFenster);
            this.grpHilfen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpHilfen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpHilfen.Location = new System.Drawing.Point(0, 0);
            this.grpHilfen.Name = "grpHilfen";
            this.grpHilfen.Size = new System.Drawing.Size(256, 81);
            this.grpHilfen.TabIndex = 6;
            this.grpHilfen.TabStop = false;
            this.grpHilfen.Text = "Hilfen";
            // 
            // ckbZeilenclickInsClipboard
            // 
            this.ckbZeilenclickInsClipboard.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.ckbZeilenclickInsClipboard.Location = new System.Drawing.Point(96, 24);
            this.ckbZeilenclickInsClipboard.Name = "ckbZeilenclickInsClipboard";
            this.ckbZeilenclickInsClipboard.Size = new System.Drawing.Size(152, 22);
            this.ckbZeilenclickInsClipboard.TabIndex = 13;
            this.ckbZeilenclickInsClipboard.Text = "Zeilenclick = Clipboard";
            // 
            // btnNummerierung
            // 
            this.btnNummerierung.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnNummerierung.Location = new System.Drawing.Point(96, 2);
            this.btnNummerierung.Margin = new System.Windows.Forms.Padding(4);
            this.btnNummerierung.Name = "btnNummerierung";
            this.btnNummerierung.QuickInfo = "Ist diese Option aktiviert, werden<br>temporäre Nummern von Spalten eingeblendet";
            this.btnNummerierung.Size = new System.Drawing.Size(112, 22);
            this.btnNummerierung.TabIndex = 12;
            this.btnNummerierung.Text = "Nummerierung";
            this.btnNummerierung.CheckedChanged += new System.EventHandler(this.btnNummerierung_CheckedChanged);
            // 
            // btnSuchFenster
            // 
            this.btnSuchFenster.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSuchFenster.ImageCode = "Lupe";
            this.btnSuchFenster.Location = new System.Drawing.Point(8, 2);
            this.btnSuchFenster.Name = "btnSuchFenster";
            this.btnSuchFenster.Size = new System.Drawing.Size(80, 66);
            this.btnSuchFenster.TabIndex = 11;
            this.btnSuchFenster.Text = "Suchfenster öffnen";
            this.btnSuchFenster.Click += new System.EventHandler(this.btnSuchFenster_Click);
            // 
            // tabAdmin
            // 
            this.tabAdmin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAdmin.Controls.Add(this.grpAdminZeilen);
            this.tabAdmin.Controls.Add(this.grpAdminAllgemein);
            this.tabAdmin.Controls.Add(this.grpAdminBearbeiten);
            this.tabAdmin.Location = new System.Drawing.Point(4, 25);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(1321, 81);
            this.tabAdmin.TabIndex = 0;
            this.tabAdmin.Text = "Administration";
            // 
            // grpAdminZeilen
            // 
            this.grpAdminZeilen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminZeilen.CausesValidation = false;
            this.grpAdminZeilen.Controls.Add(this.btnAufräumen);
            this.grpAdminZeilen.Controls.Add(this.btnSuchenUndErsetzen);
            this.grpAdminZeilen.Controls.Add(this.btnZeileLöschen);
            this.grpAdminZeilen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminZeilen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminZeilen.Location = new System.Drawing.Point(720, 0);
            this.grpAdminZeilen.Name = "grpAdminZeilen";
            this.grpAdminZeilen.Size = new System.Drawing.Size(232, 81);
            this.grpAdminZeilen.TabIndex = 8;
            this.grpAdminZeilen.TabStop = false;
            this.grpAdminZeilen.Text = "Zeilen";
            // 
            // btnAufräumen
            // 
            this.btnAufräumen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAufräumen.ImageCode = "Zeile||||||||||Pinsel";
            this.btnAufräumen.Location = new System.Drawing.Point(160, 2);
            this.btnAufräumen.Name = "btnAufräumen";
            this.btnAufräumen.QuickInfo = "Es wird ein extra Dialog geöffnet.\r\nZeilen löschen / zusammenfügen";
            this.btnAufräumen.Size = new System.Drawing.Size(64, 66);
            this.btnAufräumen.TabIndex = 45;
            this.btnAufräumen.Text = "Zeilen aufräumen";
            this.btnAufräumen.Click += new System.EventHandler(this.btnAufräumen_Click);
            // 
            // btnSuchenUndErsetzen
            // 
            this.btnSuchenUndErsetzen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSuchenUndErsetzen.ImageCode = "Fernglas";
            this.btnSuchenUndErsetzen.Location = new System.Drawing.Point(8, 2);
            this.btnSuchenUndErsetzen.Name = "btnSuchenUndErsetzen";
            this.btnSuchenUndErsetzen.Size = new System.Drawing.Size(80, 66);
            this.btnSuchenUndErsetzen.TabIndex = 44;
            this.btnSuchenUndErsetzen.Text = "Suchen und ersetzen";
            this.btnSuchenUndErsetzen.Click += new System.EventHandler(this.btnSuchenUndErsetzen_Click);
            // 
            // btnZeileLöschen
            // 
            this.btnZeileLöschen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZeileLöschen.ImageCode = "Zeile||||||||||Kreuz";
            this.btnZeileLöschen.Location = new System.Drawing.Point(88, 2);
            this.btnZeileLöschen.Name = "btnZeileLöschen";
            this.btnZeileLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            this.btnZeileLöschen.Size = new System.Drawing.Size(72, 66);
            this.btnZeileLöschen.TabIndex = 42;
            this.btnZeileLöschen.Text = "Zeilen löschen";
            this.btnZeileLöschen.Click += new System.EventHandler(this.btnZeileLöschen_Click);
            // 
            // grpAdminAllgemein
            // 
            this.grpAdminAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminAllgemein.CausesValidation = false;
            this.grpAdminAllgemein.Controls.Add(this.btnMonitoring);
            this.grpAdminAllgemein.Controls.Add(this.btnUserInfo);
            this.grpAdminAllgemein.Controls.Add(this.btnSaveLoad);
            this.grpAdminAllgemein.Controls.Add(this.btnPowerBearbeitung);
            this.grpAdminAllgemein.Controls.Add(this.btnSpaltenUebersicht);
            this.grpAdminAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminAllgemein.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminAllgemein.Location = new System.Drawing.Point(376, 0);
            this.grpAdminAllgemein.Name = "grpAdminAllgemein";
            this.grpAdminAllgemein.Size = new System.Drawing.Size(344, 81);
            this.grpAdminAllgemein.TabIndex = 7;
            this.grpAdminAllgemein.TabStop = false;
            this.grpAdminAllgemein.Text = "Allgemein";
            // 
            // btnMonitoring
            // 
            this.btnMonitoring.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnMonitoring.ImageCode = "Monitor|16";
            this.btnMonitoring.Location = new System.Drawing.Point(264, 2);
            this.btnMonitoring.Name = "btnMonitoring";
            this.btnMonitoring.QuickInfo = "A";
            this.btnMonitoring.Size = new System.Drawing.Size(72, 66);
            this.btnMonitoring.TabIndex = 45;
            this.btnMonitoring.Text = "Monitoring starten";
            this.btnMonitoring.Click += new System.EventHandler(this.btnMonitoring_Click);
            // 
            // btnUserInfo
            // 
            this.btnUserInfo.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnUserInfo.ImageCode = "Person|16|||||200";
            this.btnUserInfo.Location = new System.Drawing.Point(208, 2);
            this.btnUserInfo.Name = "btnUserInfo";
            this.btnUserInfo.QuickInfo = "A";
            this.btnUserInfo.Size = new System.Drawing.Size(56, 66);
            this.btnUserInfo.TabIndex = 44;
            this.btnUserInfo.Text = "Benutzer Info";
            this.btnUserInfo.Click += new System.EventHandler(this.btnUserInfo_Click);
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSaveLoad.ImageCode = "Diskette|16";
            this.btnSaveLoad.Location = new System.Drawing.Point(80, 2);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.QuickInfo = "Aktualisiert die Tabellen-Daten. (Speichern, neu Laden)";
            this.btnSaveLoad.Size = new System.Drawing.Size(48, 66);
            this.btnSaveLoad.TabIndex = 43;
            this.btnSaveLoad.Text = "Daten aktual.";
            this.btnSaveLoad.Click += new System.EventHandler(this.btnSaveLoad_Click);
            // 
            // btnPowerBearbeitung
            // 
            this.btnPowerBearbeitung.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnPowerBearbeitung.ImageCode = "Stift||||FF0000||||||Uhr";
            this.btnPowerBearbeitung.Location = new System.Drawing.Point(128, 2);
            this.btnPowerBearbeitung.Name = "btnPowerBearbeitung";
            this.btnPowerBearbeitung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnPowerBearbeitung.Size = new System.Drawing.Size(72, 66);
            this.btnPowerBearbeitung.TabIndex = 43;
            this.btnPowerBearbeitung.Text = "Power-Bearbeitung";
            this.btnPowerBearbeitung.Click += new System.EventHandler(this.btnPowerBearbeitung_Click);
            // 
            // btnSpaltenUebersicht
            // 
            this.btnSpaltenUebersicht.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSpaltenUebersicht.ImageCode = "Spalte||||||||||Information";
            this.btnSpaltenUebersicht.Location = new System.Drawing.Point(8, 2);
            this.btnSpaltenUebersicht.Name = "btnSpaltenUebersicht";
            this.btnSpaltenUebersicht.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenUebersicht.TabIndex = 36;
            this.btnSpaltenUebersicht.Text = "Spalten-Übersicht";
            this.btnSpaltenUebersicht.Click += new System.EventHandler(this.btnSpaltenUebersicht_Click);
            // 
            // grpAdminBearbeiten
            // 
            this.grpAdminBearbeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminBearbeiten.CausesValidation = false;
            this.grpAdminBearbeiten.Controls.Add(this.btnSuchInScript);
            this.grpAdminBearbeiten.Controls.Add(this.btnSkripteBearbeiten);
            this.grpAdminBearbeiten.Controls.Add(this.btnFormular);
            this.grpAdminBearbeiten.Controls.Add(this.btnSpaltenanordnung);
            this.grpAdminBearbeiten.Controls.Add(this.btnTabelleKopf);
            this.grpAdminBearbeiten.Controls.Add(this.btnLayouts);
            this.grpAdminBearbeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminBearbeiten.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminBearbeiten.Location = new System.Drawing.Point(0, 0);
            this.grpAdminBearbeiten.Name = "grpAdminBearbeiten";
            this.grpAdminBearbeiten.Size = new System.Drawing.Size(376, 81);
            this.grpAdminBearbeiten.TabIndex = 9;
            this.grpAdminBearbeiten.TabStop = false;
            this.grpAdminBearbeiten.Text = "Bearbeiten";
            // 
            // btnSuchInScript
            // 
            this.btnSuchInScript.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSuchInScript.ImageCode = "Skript||||||||||Stift";
            this.btnSuchInScript.Location = new System.Drawing.Point(304, 2);
            this.btnSuchInScript.Name = "btnSuchInScript";
            this.btnSuchInScript.Size = new System.Drawing.Size(64, 66);
            this.btnSuchInScript.TabIndex = 46;
            this.btnSuchInScript.Text = "in Skripten suchen";
            this.btnSuchInScript.Click += new System.EventHandler(this.btnSuchInScript_Click);
            // 
            // btnSkripteBearbeiten
            // 
            this.btnSkripteBearbeiten.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSkripteBearbeiten.ImageCode = "Skript||||||||||Stift";
            this.btnSkripteBearbeiten.Location = new System.Drawing.Point(240, 2);
            this.btnSkripteBearbeiten.Name = "btnSkripteBearbeiten";
            this.btnSkripteBearbeiten.Size = new System.Drawing.Size(56, 66);
            this.btnSkripteBearbeiten.TabIndex = 45;
            this.btnSkripteBearbeiten.Text = "Skripte";
            this.btnSkripteBearbeiten.Click += new System.EventHandler(this.ContextMenu_OpenScriptEditor);
            // 
            // btnFormular
            // 
            this.btnFormular.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnFormular.ImageCode = "Anwendung||||||||||Stift";
            this.btnFormular.Location = new System.Drawing.Point(64, 2);
            this.btnFormular.Name = "btnFormular";
            this.btnFormular.Size = new System.Drawing.Size(56, 66);
            this.btnFormular.TabIndex = 44;
            this.btnFormular.Text = "Formular-Editor";
            this.btnFormular.Click += new System.EventHandler(this.btnFormular_Click);
            // 
            // btnSpaltenanordnung
            // 
            this.btnSpaltenanordnung.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSpaltenanordnung.ImageCode = "Spalte||||||||||Stift";
            this.btnSpaltenanordnung.Location = new System.Drawing.Point(176, 2);
            this.btnSpaltenanordnung.Name = "btnSpaltenanordnung";
            this.btnSpaltenanordnung.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenanordnung.TabIndex = 43;
            this.btnSpaltenanordnung.Text = "Spalten-anordung";
            this.btnSpaltenanordnung.Click += new System.EventHandler(this.btnSpaltenanordnung_Click);
            // 
            // btnTabelleKopf
            // 
            this.btnTabelleKopf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTabelleKopf.ImageCode = "Tabelle||||||||||Stift";
            this.btnTabelleKopf.Location = new System.Drawing.Point(0, 2);
            this.btnTabelleKopf.Name = "btnTabelleKopf";
            this.btnTabelleKopf.Size = new System.Drawing.Size(64, 66);
            this.btnTabelleKopf.TabIndex = 37;
            this.btnTabelleKopf.Text = "Tabellen-Kopf";
            this.btnTabelleKopf.Click += new System.EventHandler(this.btnTabelleKopf_Click);
            // 
            // btnLayouts
            // 
            this.btnLayouts.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnLayouts.ImageCode = "Layout||||||||||Stift";
            this.btnLayouts.Location = new System.Drawing.Point(120, 2);
            this.btnLayouts.Name = "btnLayouts";
            this.btnLayouts.Size = new System.Drawing.Size(56, 66);
            this.btnLayouts.TabIndex = 41;
            this.btnLayouts.Text = "Layout-Editor";
            this.btnLayouts.Click += new System.EventHandler(this.btnLayouts_Click);
            // 
            // tabExport
            // 
            this.tabExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabExport.Controls.Add(this.grpExport);
            this.tabExport.Controls.Add(this.grpImport);
            this.tabExport.Location = new System.Drawing.Point(4, 25);
            this.tabExport.Name = "tabExport";
            this.tabExport.Size = new System.Drawing.Size(1321, 81);
            this.tabExport.TabIndex = 2;
            this.tabExport.Text = "Import/Export";
            // 
            // grpExport
            // 
            this.grpExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpExport.CausesValidation = false;
            this.grpExport.Controls.Add(this.btnDrucken);
            this.grpExport.Controls.Add(this.btnHTMLExport);
            this.grpExport.Controls.Add(this.btnCSVClipboard);
            this.grpExport.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpExport.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpExport.Location = new System.Drawing.Point(144, 0);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(224, 81);
            this.grpExport.TabIndex = 3;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Export";
            // 
            // btnDrucken
            // 
            this.btnDrucken.AutoSort = false;
            this.btnDrucken.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnDrucken.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnDrucken.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnDrucken.ImageCode = "Drucker";
            this.btnDrucken.Location = new System.Drawing.Point(136, 2);
            this.btnDrucken.Name = "btnDrucken";
            this.btnDrucken.Size = new System.Drawing.Size(80, 66);
            this.btnDrucken.TabIndex = 13;
            this.btnDrucken.Text = "Drucken bzw. Export";
            this.btnDrucken.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnDrucken_ItemClicked);
            // 
            // btnHTMLExport
            // 
            this.btnHTMLExport.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnHTMLExport.ImageCode = "Globus";
            this.btnHTMLExport.Location = new System.Drawing.Point(72, 2);
            this.btnHTMLExport.Name = "btnHTMLExport";
            this.btnHTMLExport.QuickInfo = "HTML-Format (für einen Internet-Browser) als Datei";
            this.btnHTMLExport.Size = new System.Drawing.Size(64, 66);
            this.btnHTMLExport.TabIndex = 3;
            this.btnHTMLExport.Text = "HTML";
            this.btnHTMLExport.Click += new System.EventHandler(this.btnHTMLExport_Click);
            // 
            // btnCSVClipboard
            // 
            this.btnCSVClipboard.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnCSVClipboard.ImageCode = "Clipboard||||||||||Pfeil_Rechts";
            this.btnCSVClipboard.Location = new System.Drawing.Point(8, 2);
            this.btnCSVClipboard.Name = "btnCSVClipboard";
            this.btnCSVClipboard.QuickInfo = "CSV-Format (z.B.: für Excel) in die Zwischenablage";
            this.btnCSVClipboard.Size = new System.Drawing.Size(64, 66);
            this.btnCSVClipboard.TabIndex = 0;
            this.btnCSVClipboard.Text = "CSV";
            this.btnCSVClipboard.Click += new System.EventHandler(this.btnCSVClipboard_Click);
            // 
            // grpImport
            // 
            this.grpImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpImport.CausesValidation = false;
            this.grpImport.Controls.Add(this.btnMDBImport);
            this.grpImport.Controls.Add(this.btnClipboardImport);
            this.grpImport.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpImport.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpImport.Location = new System.Drawing.Point(0, 0);
            this.grpImport.Name = "grpImport";
            this.grpImport.Size = new System.Drawing.Size(144, 81);
            this.grpImport.TabIndex = 4;
            this.grpImport.TabStop = false;
            this.grpImport.Text = "Import";
            // 
            // btnMDBImport
            // 
            this.btnMDBImport.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnMDBImport.ImageCode = "Tabelle||||||||||Tabelle";
            this.btnMDBImport.Location = new System.Drawing.Point(72, 2);
            this.btnMDBImport.Name = "btnMDBImport";
            this.btnMDBImport.QuickInfo = "Importiert in die aktuell angezeigte Tabelle\r\nDaten einer andere Tabelle.";
            this.btnMDBImport.Size = new System.Drawing.Size(64, 66);
            this.btnMDBImport.TabIndex = 40;
            this.btnMDBImport.Text = "Datei-Import";
            this.btnMDBImport.Click += new System.EventHandler(this.btnMDBImport_Click);
            // 
            // btnClipboardImport
            // 
            this.btnClipboardImport.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnClipboardImport.ImageCode = "Tabelle||||||||||Clipboard";
            this.btnClipboardImport.Location = new System.Drawing.Point(8, 2);
            this.btnClipboardImport.Name = "btnClipboardImport";
            this.btnClipboardImport.QuickInfo = "Importiert in die aktuell angezeigte Tabelle\r\nDaten aus dem Clipboard.";
            this.btnClipboardImport.Size = new System.Drawing.Size(64, 66);
            this.btnClipboardImport.TabIndex = 39;
            this.btnClipboardImport.Text = "Clipboard-Import";
            this.btnClipboardImport.Click += new System.EventHandler(this.btnClipboardImport_Click);
            // 
            // pnlTableSelect
            // 
            this.pnlTableSelect.Controls.Add(this.tbcTableSelector);
            this.pnlTableSelect.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTableSelect.Location = new System.Drawing.Point(0, 0);
            this.pnlTableSelect.Name = "pnlTableSelect";
            this.pnlTableSelect.Size = new System.Drawing.Size(972, 24);
            this.pnlTableSelect.TabIndex = 21;
            // 
            // tbcTableSelector
            // 
            this.tbcTableSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcTableSelector.HotTrack = true;
            this.tbcTableSelector.Location = new System.Drawing.Point(0, 0);
            this.tbcTableSelector.Name = "tbcTableSelector";
            this.tbcTableSelector.SelectedIndex = 0;
            this.tbcTableSelector.Size = new System.Drawing.Size(972, 24);
            this.tbcTableSelector.TabDefault = null;
            this.tbcTableSelector.TabDefaultOrder = null;
            this.tbcTableSelector.TabIndex = 20;
            this.tbcTableSelector.Selected += new System.Windows.Forms.TabControlEventHandler(this.tbcTableSelector_Selected);
            this.tbcTableSelector.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tbcTableSelector_Deselecting);
            // 
            // Table
            // 
            this.Table.ÄhnlicheAnsichtName = "Filterleiste Ähnlich";
            this.Table.FilterAnsichtName = "Filterleiste Waagerecht";
            this.Table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Table.Location = new System.Drawing.Point(0, 24);
            this.Table.Name = "Table";
            this.Table.SheetStyle = "Windows 11";
            this.Table.Size = new System.Drawing.Size(972, 571);
            this.Table.TabIndex = 0;
            this.Table.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.Table_ContextMenuInit);
            this.Table.TableChanged += new System.EventHandler(this.Table_TableChanged);
            this.Table.SelectedCellChanged += new System.EventHandler<BlueControls.EventArgs.CellExtEventArgs>(this.Table_SelectedCellChanged);
            this.Table.SelectedRowChanged += new System.EventHandler<BlueTable.EventArgs.RowNullableEventArgs>(this.Table_SelectedRowChanged);
            this.Table.ViewChanged += new System.EventHandler(this.Table_ViewChanged);
            this.Table.VisibleRowsChanged += new System.EventHandler(this.Table_VisibleRowsChanged);
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
            this.SplitContainer1.Panel1.Controls.Add(this.pnlTableSelect);
            // 
            // SplitContainer1.Panel2
            // 
            this.SplitContainer1.Panel2.Controls.Add(this.tbcSidebar);
            this.SplitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(27, 25, 27, 25);
            this.SplitContainer1.Size = new System.Drawing.Size(1329, 595);
            this.SplitContainer1.SplitterDistance = 972;
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
            this.tbcSidebar.Size = new System.Drawing.Size(346, 595);
            this.tbcSidebar.TabDefault = null;
            this.tbcSidebar.TabDefaultOrder = null;
            this.tbcSidebar.TabIndex = 21;
            // 
            // tabFormula
            // 
            this.tabFormula.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabFormula.Controls.Add(this.CFO);
            this.tabFormula.Location = new System.Drawing.Point(4, 25);
            this.tabFormula.Name = "tabFormula";
            this.tabFormula.Size = new System.Drawing.Size(338, 566);
            this.tabFormula.TabIndex = 1;
            this.tabFormula.Text = "Formular";
            // 
            // CFO
            // 
            this.CFO.CausesValidation = false;
            this.CFO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CFO.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.CFO.Location = new System.Drawing.Point(0, 0);
            this.CFO.Name = "CFO";
            this.CFO.Page = null;
            this.CFO.Size = new System.Drawing.Size(338, 566);
            this.CFO.TabIndex = 0;
            this.CFO.TabStop = false;
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
            // LoadTab
            // 
            this.LoadTab.Filter = "Tabellen (*.BDB;*.MBDB;*.CBDB)|*.BDB;*.MBDB;*.CBDB|Alle Dateien (*.*)|*.*";
            this.LoadTab.Title = "Bitte Tabelle laden!";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // SaveTab
            // 
            this.SaveTab.Filter = "*.BDB Tabellen|*.BDB|*.MBDB Tabellen|*.MBDB|*.CBDB Tabellen|*.CBDB|*.* A" +
    "lle Dateien|*";
            this.SaveTab.Title = "Bitte neuen Dateinamen der Tabelle wählen.";
            // 
            // grpAufräumen
            // 
            this.grpAufräumen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.grpAufräumen.ImageCode = "Zeile||||||||||Pinsel";
            this.grpAufräumen.Location = new System.Drawing.Point(160, 2);
            this.grpAufräumen.Name = "grpAufräumen";
            this.grpAufräumen.QuickInfo = "Angezeigte Zeilen löschen";
            this.grpAufräumen.Size = new System.Drawing.Size(72, 66);
            this.grpAufräumen.TabIndex = 45;
            this.grpAufräumen.Text = "Zeilen löschen";
            // 
            // TableView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1329, 729);
            this.Controls.Add(this.SplitContainer1);
            this.Controls.Add(this.ribMain);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "TableView";
            this.Text = "TableView";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.SplitContainer1, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabFile.ResumeLayout(false);
            this.grpOrdner.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.grpAufgaben.ResumeLayout(false);
            this.grpAnsicht.ResumeLayout(false);
            this.grpHilfen.ResumeLayout(false);
            this.tabAdmin.ResumeLayout(false);
            this.grpAdminZeilen.ResumeLayout(false);
            this.grpAdminAllgemein.ResumeLayout(false);
            this.grpAdminBearbeiten.ResumeLayout(false);
            this.tabExport.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
            this.grpImport.ResumeLayout(false);
            this.pnlTableSelect.ResumeLayout(false);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            this.tbcSidebar.ResumeLayout(false);
            this.tabFormula.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected RibbonBar ribMain;
        protected Panel pnlTableSelect;
        protected TabControl tbcTableSelector;
        protected Controls.TableView Table;
        protected SplitContainer SplitContainer1;
        protected TabPage tabAdmin;
        private Button btnZeileLöschen;
        private Button btnPowerBearbeitung;
        private Button btnSpaltenanordnung;
        private Button btnLayouts;
        private Button btnTabelleKopf;
        private Button btnSpaltenUebersicht;
        private Button btnClipboardImport;
        private GroupBox grpAdminZeilen;
        private GroupBox grpAdminBearbeiten;
        private GroupBox grpAdminAllgemein;
        protected TabControl tbcSidebar;
        private Caption capZeilen2;
        protected TabPage tabAllgemein;
        protected Button btnUnterschiede;
        private Caption capSpaltenanordnung;
        private Caption capZeilen1;
        private ComboBox cbxColumnArr;
        protected GroupBox grpAnsicht;
        protected Button btnAlleSchließen;
        protected Button btnAlleErweitern;
        private GroupBox grpImport;
        private GroupBox grpExport;
        private Button btnHTMLExport;
        private Button btnCSVClipboard;
        protected TabPage tabExport;
        private ComboBox btnDrucken;
        private TabPage tabFile;
        private GroupBox grpOrdner;
        private Button btnTabellenSpeicherort;
        private Button btnTemporärenSpeicherortÖffnen;
        private GroupBox grpDatei;
        private LastFilesCombo btnLetzteDateien;
        private Button btnOeffnen;
        private Button btnSaveAs;
        private Button btnNeuDB;
        private OpenFileDialog LoadTab;
        private SaveFileDialog SaveTab;
        private Button btnSuchenUndErsetzen;
        protected TabPage tabFormula;
        private ConnectedFormulaView CFO;
        private Button btnFormular;
        private GroupBox grpHilfen;
        private Button ckbZeilenclickInsClipboard;
        private Button btnNummerierung;
        private Button btnSuchFenster;
        private Button btnSaveLoad;
        private Button btnSkripteBearbeiten;
        protected GroupBox grpAufgaben;
        private ListBox lstAufgaben;
        private Button btnMDBImport;
        private Button btnAufräumen;
        private Button grpAufräumen;
        private Button btnUserInfo;
        private Button btnSuchInScript;
        protected Button btnZoomOut;
        protected Button btnZoomIn;
        protected Button btnZoomFit;
        private Button btnMonitoring;
    }
}