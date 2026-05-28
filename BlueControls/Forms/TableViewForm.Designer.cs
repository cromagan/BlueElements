// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.EventArgs;
using System.ComponentModel;
using System.Windows.Forms;
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
            var resources = new ComponentResourceManager(typeof(TableViewForm));
            ribMain = new RibbonBar();
            tabFile = new TabPage();
            grpOrdner = new GroupBox();
            btnTabellenSpeicherort = new Button();
            btnTemporärenSpeicherortÖffnen = new Button();
            grpDatei = new GroupBox();
            btnLetzteDateien = new LastFilesCombo();
            btnOeffnen = new Button();
            btnSaveAs = new Button();
            btnNeuDB = new Button();
            tabAllgemein = new TabPage();
            grpAufgaben = new GroupBox();
            lstAufgaben = new ListBox();
            grpAnsicht = new GroupBox();
            btnZoomFit = new Button();
            btnZoomOut = new Button();
            btnZoomIn = new Button();
            btnAlleSchließen = new Button();
            btnAlleErweitern = new Button();
            capSpaltenanordnung = new Caption();
            capZeilen1 = new Caption();
            cbxColumnArr = new ComboBox();
            grpHilfen = new GroupBox();
            ckbZeilenclickInsClipboard = new Button();
            btnNummerierung = new Button();
            btnSuchFenster = new Button();
            tabAdmin = new TabPage();
            grpAdminZeilen = new GroupBox();
            btnAufräumen = new Button();
            btnSuchenUndErsetzen = new Button();
            btnZeileLöschen = new Button();
            grpAdminAllgemein = new GroupBox();
            btnMonitoring = new Button();
            btnUserInfo = new Button();
            btnSaveLoad = new Button();
            btnPowerBearbeitung = new Button();
            btnSpaltenUebersicht = new Button();
            grpAdminBearbeiten = new GroupBox();
            btnSuchInScript = new Button();
            btnSkripteBearbeiten = new Button();
            btnFormular = new Button();
            btnSpaltenanordnung = new Button();
            btnTabelleKopf = new Button();
            btnLayouts = new Button();
            tabExport = new TabPage();
            grpExport = new GroupBox();
            btnDrucken = new ComboBox();
            btnHTMLExport = new Button();
            btnCSVClipboard = new Button();
            grpImport = new GroupBox();
            btnMDBImport = new Button();
            btnClipboardImport = new Button();
            pnlTableSelect = new Panel();
            tbcTableSelector = new TabControl();
            TableView = new TableViewWithFilters();
            SplitContainer1 = new SplitContainer();
            tbcSidebar = new TabControl();
            tabFormula = new TabPage();
            CFO = new ConnectedFormulaView();
            capZeilen2 = new Caption();
            LoadTab = new OpenFileDialog();
            SaveTab = new SaveFileDialog();
            grpAufräumen = new Button();
            pnlStatusBar.SuspendLayout();
            ribMain.SuspendLayout();
            tabFile.SuspendLayout();
            grpOrdner.SuspendLayout();
            grpDatei.SuspendLayout();
            tabAllgemein.SuspendLayout();
            grpAufgaben.SuspendLayout();
            grpAnsicht.SuspendLayout();
            grpHilfen.SuspendLayout();
            tabAdmin.SuspendLayout();
            grpAdminZeilen.SuspendLayout();
            grpAdminAllgemein.SuspendLayout();
            grpAdminBearbeiten.SuspendLayout();
            tabExport.SuspendLayout();
            grpExport.SuspendLayout();
            grpImport.SuspendLayout();
            pnlTableSelect.SuspendLayout();
            ((ISupportInitialize)SplitContainer1).BeginInit();
            SplitContainer1.Panel1.SuspendLayout();
            SplitContainer1.Panel2.SuspendLayout();
            SplitContainer1.SuspendLayout();
            tbcSidebar.SuspendLayout();
            tabFormula.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Location = new Point(304, 0);
            capStatusBar.Size = new Size(1025, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Controls.Add(capZeilen2);
            pnlStatusBar.Location = new Point(0, 705);
            pnlStatusBar.Size = new Size(1329, 24);
            pnlStatusBar.Controls.SetChildIndex(capZeilen2, 0);
            pnlStatusBar.Controls.SetChildIndex(capStatusBar, 0);
            // 
            // ribMain
            // 
            ribMain.Controls.Add(tabFile);
            ribMain.Controls.Add(tabAllgemein);
            ribMain.Controls.Add(tabAdmin);
            ribMain.Controls.Add(tabExport);
            ribMain.Dock = DockStyle.Top;
            ribMain.HotTrack = true;
            ribMain.Location = new Point(0, 0);
            ribMain.Name = "ribMain";
            ribMain.SelectedIndex = 1;
            ribMain.Size = new Size(1329, 110);
            ribMain.TabDefault = tabFile;
            ribMain.TabDefaultOrder = null;
            ribMain.TabIndex = 93;
            // 
            // tabFile
            // 
            tabFile.BackColor = Color.FromArgb(244, 245, 246);
            tabFile.Controls.Add(grpOrdner);
            tabFile.Controls.Add(grpDatei);
            tabFile.Location = new Point(4, 25);
            tabFile.Margin = new Padding(0);
            tabFile.Name = "tabFile";
            tabFile.Size = new Size(1321, 81);
            tabFile.TabIndex = 3;
            tabFile.Text = "Datei";
            // 
            // grpOrdner
            // 
            grpOrdner.BackColor = Color.FromArgb(244, 245, 246);
            grpOrdner.CausesValidation = false;
            grpOrdner.Controls.Add(btnTabellenSpeicherort);
            grpOrdner.Controls.Add(btnTemporärenSpeicherortÖffnen);
            grpOrdner.Dock = DockStyle.Left;
            grpOrdner.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpOrdner.Location = new Point(304, 0);
            grpOrdner.Name = "grpOrdner";
            grpOrdner.Size = new Size(184, 81);
            grpOrdner.TabIndex = 3;
            grpOrdner.TabStop = false;
            grpOrdner.Text = "Ordner";
            // 
            // btnTabellenSpeicherort
            // 
            btnTabellenSpeicherort.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnTabellenSpeicherort.ImageCode = "Ordner";
            btnTabellenSpeicherort.Location = new Point(8, 2);
            btnTabellenSpeicherort.Name = "btnTabellenSpeicherort";
            btnTabellenSpeicherort.QuickInfo = "Speicherort der Tabellen öffnen";
            btnTabellenSpeicherort.Size = new Size(88, 66);
            btnTabellenSpeicherort.TabIndex = 27;
            btnTabellenSpeicherort.Text = "Tabellen-Pfad";
            btnTabellenSpeicherort.Click += btnTabellenSpeicherort_Click;
            // 
            // btnTemporärenSpeicherortÖffnen
            // 
            btnTemporärenSpeicherortÖffnen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnTemporärenSpeicherortÖffnen.ImageCode = "Ordner||||0000ff||126";
            btnTemporärenSpeicherortÖffnen.Location = new Point(96, 2);
            btnTemporärenSpeicherortÖffnen.Name = "btnTemporärenSpeicherortÖffnen";
            btnTemporärenSpeicherortÖffnen.QuickInfo = "Temporären Speicherort öffnen";
            btnTemporärenSpeicherortÖffnen.Size = new Size(80, 66);
            btnTemporärenSpeicherortÖffnen.TabIndex = 26;
            btnTemporärenSpeicherortÖffnen.Text = "Temporärer Speicherort";
            btnTemporärenSpeicherortÖffnen.Click += btnTemporärenSpeicherortÖffnen_Click;
            // 
            // grpDatei
            // 
            grpDatei.BackColor = Color.FromArgb(244, 245, 246);
            grpDatei.CausesValidation = false;
            grpDatei.Controls.Add(btnLetzteDateien);
            grpDatei.Controls.Add(btnOeffnen);
            grpDatei.Controls.Add(btnSaveAs);
            grpDatei.Controls.Add(btnNeuDB);
            grpDatei.Dock = DockStyle.Left;
            grpDatei.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpDatei.Location = new Point(0, 0);
            grpDatei.Name = "grpDatei";
            grpDatei.Size = new Size(304, 81);
            grpDatei.TabIndex = 4;
            grpDatei.TabStop = false;
            grpDatei.Text = "Datei";
            // 
            // btnLetzteDateien
            // 
            btnLetzteDateien.DrawStyle = ComboboxStyle.RibbonBar;
            btnLetzteDateien.DropDownStyle = ComboBoxStyle.DropDownList;
            btnLetzteDateien.Enabled = false;
            btnLetzteDateien.ImageCode = "Ordner";
            btnLetzteDateien.Location = new Point(128, 2);
            btnLetzteDateien.Name = "btnLetzteDateien";
            btnLetzteDateien.RemoveAllowed = true;
            btnLetzteDateien.SettingsLoaded = false;
            btnLetzteDateien.Size = new Size(104, 66);
            btnLetzteDateien.TabIndex = 1;
            btnLetzteDateien.Text = "zuletzt geöffnete Dateien";
            btnLetzteDateien.ItemClicked += btnLetzteDateien_ItemClicked;
            // 
            // btnOeffnen
            // 
            btnOeffnen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnOeffnen.ImageCode = "Ordner";
            btnOeffnen.Location = new Point(72, 2);
            btnOeffnen.Name = "btnOeffnen";
            btnOeffnen.Size = new Size(56, 66);
            btnOeffnen.TabIndex = 1;
            btnOeffnen.Text = "Öffnen";
            btnOeffnen.Click += btnOeffnen_Click;
            // 
            // btnSaveAs
            // 
            btnSaveAs.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSaveAs.ImageCode = "Diskette";
            btnSaveAs.Location = new Point(232, 2);
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new Size(64, 66);
            btnSaveAs.TabIndex = 4;
            btnSaveAs.Text = "Speichern unter";
            btnSaveAs.Click += btnSaveAs_Click;
            // 
            // btnNeuDB
            // 
            btnNeuDB.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnNeuDB.ImageCode = "Datei";
            btnNeuDB.Location = new Point(8, 2);
            btnNeuDB.Name = "btnNeuDB";
            btnNeuDB.Size = new Size(56, 66);
            btnNeuDB.TabIndex = 0;
            btnNeuDB.Text = "Neu";
            btnNeuDB.Click += btnNeuDB_Click;
            // 
            // tabAllgemein
            // 
            tabAllgemein.BackColor = Color.FromArgb(244, 245, 246);
            tabAllgemein.Controls.Add(grpAufgaben);
            tabAllgemein.Controls.Add(grpAnsicht);
            tabAllgemein.Controls.Add(grpHilfen);
            tabAllgemein.Location = new Point(4, 25);
            tabAllgemein.Name = "tabAllgemein";
            tabAllgemein.Size = new Size(1321, 81);
            tabAllgemein.TabIndex = 1;
            tabAllgemein.Text = "Allgemein";
            // 
            // grpAufgaben
            // 
            grpAufgaben.BackColor = Color.FromArgb(244, 245, 246);
            grpAufgaben.CausesValidation = false;
            grpAufgaben.Controls.Add(lstAufgaben);
            grpAufgaben.Dock = DockStyle.Left;
            grpAufgaben.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpAufgaben.Location = new Point(640, 0);
            grpAufgaben.Name = "grpAufgaben";
            grpAufgaben.Size = new Size(222, 81);
            grpAufgaben.TabIndex = 7;
            grpAufgaben.TabStop = false;
            grpAufgaben.Text = "Aufgaben";
            // 
            // lstAufgaben
            // 
            lstAufgaben.AddAllowed = AddType.None;
            lstAufgaben.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstAufgaben.AutoSort = false;
            lstAufgaben.CheckBehavior = CheckBehavior.NoSelection;
            lstAufgaben.Location = new Point(8, 2);
            lstAufgaben.Name = "lstAufgaben";
            lstAufgaben.Size = new Size(208, 66);
            lstAufgaben.TabIndex = 0;
            // 
            // grpAnsicht
            // 
            grpAnsicht.BackColor = Color.FromArgb(244, 245, 246);
            grpAnsicht.CausesValidation = false;
            grpAnsicht.Controls.Add(btnZoomFit);
            grpAnsicht.Controls.Add(btnZoomOut);
            grpAnsicht.Controls.Add(btnZoomIn);
            grpAnsicht.Controls.Add(btnAlleSchließen);
            grpAnsicht.Controls.Add(btnAlleErweitern);
            grpAnsicht.Controls.Add(capSpaltenanordnung);
            grpAnsicht.Controls.Add(capZeilen1);
            grpAnsicht.Controls.Add(cbxColumnArr);
            grpAnsicht.Dock = DockStyle.Left;
            grpAnsicht.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpAnsicht.Location = new Point(256, 0);
            grpAnsicht.Margin = new Padding(0);
            grpAnsicht.Name = "grpAnsicht";
            grpAnsicht.Padding = new Padding(0);
            grpAnsicht.Size = new Size(384, 81);
            grpAnsicht.TabIndex = 3;
            grpAnsicht.TabStop = false;
            grpAnsicht.Text = "Ansicht";
            // 
            // btnZoomFit
            // 
            btnZoomFit.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnZoomFit.ImageCode = "ZoomFit|20";
            btnZoomFit.Location = new Point(328, 24);
            btnZoomFit.Name = "btnZoomFit";
            btnZoomFit.Size = new Size(48, 22);
            btnZoomFit.TabIndex = 17;
            btnZoomFit.Text = "1:1";
            btnZoomFit.Click += btnZoomFit_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnZoomOut.ImageCode = "LupeMinus|20";
            btnZoomOut.Location = new Point(352, 2);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(24, 22);
            btnZoomOut.TabIndex = 16;
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnZoomIn.ImageCode = "LupePlus|20";
            btnZoomIn.Location = new Point(328, 2);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(24, 22);
            btnZoomIn.TabIndex = 15;
            btnZoomIn.Click += btnZoomIn_Click;
            // 
            // btnAlleSchließen
            // 
            btnAlleSchließen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnAlleSchließen.ImageCode = "Pfeil_Oben_Scrollbar|14|||||0";
            btnAlleSchließen.Location = new Point(216, 24);
            btnAlleSchließen.Name = "btnAlleSchließen";
            btnAlleSchließen.QuickInfo = "Neuen Eintrag ergänzen";
            btnAlleSchließen.Size = new Size(104, 22);
            btnAlleSchließen.TabIndex = 4;
            btnAlleSchließen.Text = "alle schließen";
            btnAlleSchließen.Click += btnAlleSchließen_Click;
            // 
            // btnAlleErweitern
            // 
            btnAlleErweitern.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnAlleErweitern.ImageCode = "Pfeil_Unten_Scrollbar|14|||ff0000||200|200";
            btnAlleErweitern.Location = new Point(216, 2);
            btnAlleErweitern.Name = "btnAlleErweitern";
            btnAlleErweitern.QuickInfo = "Neuen Eintrag ergänzen";
            btnAlleErweitern.Size = new Size(104, 22);
            btnAlleErweitern.TabIndex = 3;
            btnAlleErweitern.Text = "alle erweitern";
            btnAlleErweitern.Click += btnAlleErweitern_Click;
            // 
            // capSpaltenanordnung
            // 
            capSpaltenanordnung.CausesValidation = false;
            capSpaltenanordnung.Location = new Point(8, 2);
            capSpaltenanordnung.Margin = new Padding(4);
            capSpaltenanordnung.Name = "capSpaltenanordnung";
            capSpaltenanordnung.Size = new Size(200, 22);
            capSpaltenanordnung.Text = "<u>Spaltenanordnung:";
            // 
            // capZeilen1
            // 
            capZeilen1.CausesValidation = false;
            capZeilen1.Location = new Point(8, 46);
            capZeilen1.Margin = new Padding(4);
            capZeilen1.Name = "capZeilen1";
            capZeilen1.Size = new Size(200, 22);
            capZeilen1.Text = "<imagecode=Information|16>";
            capZeilen1.Translate = false;
            // 
            // cbxColumnArr
            // 
            cbxColumnArr.Cursor = Cursors.IBeam;
            cbxColumnArr.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxColumnArr.Location = new Point(8, 24);
            cbxColumnArr.Margin = new Padding(4);
            cbxColumnArr.Name = "cbxColumnArr";
            cbxColumnArr.Size = new Size(200, 22);
            cbxColumnArr.TabIndex = 2;
            cbxColumnArr.ItemClicked += cbxColumnArr_ItemClicked;
            // 
            // grpHilfen
            // 
            grpHilfen.BackColor = Color.FromArgb(244, 245, 246);
            grpHilfen.CausesValidation = false;
            grpHilfen.Controls.Add(ckbZeilenclickInsClipboard);
            grpHilfen.Controls.Add(btnNummerierung);
            grpHilfen.Controls.Add(btnSuchFenster);
            grpHilfen.Dock = DockStyle.Left;
            grpHilfen.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpHilfen.Location = new Point(0, 0);
            grpHilfen.Name = "grpHilfen";
            grpHilfen.Size = new Size(256, 81);
            grpHilfen.TabIndex = 6;
            grpHilfen.TabStop = false;
            grpHilfen.Text = "Hilfen";
            // 
            // ckbZeilenclickInsClipboard
            // 
            ckbZeilenclickInsClipboard.ButtonStyle = ButtonStyle.Checkbox_Text;
            ckbZeilenclickInsClipboard.Location = new Point(96, 24);
            ckbZeilenclickInsClipboard.Name = "ckbZeilenclickInsClipboard";
            ckbZeilenclickInsClipboard.Size = new Size(152, 22);
            ckbZeilenclickInsClipboard.TabIndex = 13;
            ckbZeilenclickInsClipboard.Text = "Zeilenclick = Clipboard";
            // 
            // btnNummerierung
            // 
            btnNummerierung.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnNummerierung.Location = new Point(96, 2);
            btnNummerierung.Margin = new Padding(4);
            btnNummerierung.Name = "btnNummerierung";
            btnNummerierung.QuickInfo = "Ist diese Option aktiviert, werden<br>temporäre Nummern von Spalten eingeblendet";
            btnNummerierung.Size = new Size(112, 22);
            btnNummerierung.TabIndex = 12;
            btnNummerierung.Text = "Nummerierung";
            btnNummerierung.CheckedChanged += btnNummerierung_CheckedChanged;
            // 
            // btnSuchFenster
            // 
            btnSuchFenster.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSuchFenster.ImageCode = "Lupe";
            btnSuchFenster.Location = new Point(8, 2);
            btnSuchFenster.Name = "btnSuchFenster";
            btnSuchFenster.Size = new Size(80, 66);
            btnSuchFenster.TabIndex = 11;
            btnSuchFenster.Text = "Suchfenster öffnen";
            btnSuchFenster.Click += btnSuchFenster_Click;
            // 
            // tabAdmin
            // 
            tabAdmin.BackColor = Color.FromArgb(244, 245, 246);
            tabAdmin.Controls.Add(grpAdminZeilen);
            tabAdmin.Controls.Add(grpAdminAllgemein);
            tabAdmin.Controls.Add(grpAdminBearbeiten);
            tabAdmin.Location = new Point(4, 25);
            tabAdmin.Name = "tabAdmin";
            tabAdmin.Size = new Size(1321, 81);
            tabAdmin.TabIndex = 0;
            tabAdmin.Text = "Administration";
            // 
            // grpAdminZeilen
            // 
            grpAdminZeilen.BackColor = Color.FromArgb(244, 245, 246);
            grpAdminZeilen.CausesValidation = false;
            grpAdminZeilen.Controls.Add(btnAufräumen);
            grpAdminZeilen.Controls.Add(btnSuchenUndErsetzen);
            grpAdminZeilen.Controls.Add(btnZeileLöschen);
            grpAdminZeilen.Dock = DockStyle.Left;
            grpAdminZeilen.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpAdminZeilen.Location = new Point(720, 0);
            grpAdminZeilen.Name = "grpAdminZeilen";
            grpAdminZeilen.Size = new Size(232, 81);
            grpAdminZeilen.TabIndex = 8;
            grpAdminZeilen.TabStop = false;
            grpAdminZeilen.Text = "Zeilen";
            // 
            // btnAufräumen
            // 
            btnAufräumen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnAufräumen.ImageCode = "Zeile||||||||||Pinsel";
            btnAufräumen.Location = new Point(160, 2);
            btnAufräumen.Name = "btnAufräumen";
            btnAufräumen.QuickInfo = "Es wird ein extra Dialog geöffnet.\r\nZeilen löschen / zusammenfügen";
            btnAufräumen.Size = new Size(64, 66);
            btnAufräumen.TabIndex = 45;
            btnAufräumen.Text = "Zeilen aufräumen";
            btnAufräumen.Click += btnAufräumen_Click;
            // 
            // btnSuchenUndErsetzen
            // 
            btnSuchenUndErsetzen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSuchenUndErsetzen.ImageCode = "Fernglas";
            btnSuchenUndErsetzen.Location = new Point(8, 2);
            btnSuchenUndErsetzen.Name = "btnSuchenUndErsetzen";
            btnSuchenUndErsetzen.Size = new Size(80, 66);
            btnSuchenUndErsetzen.TabIndex = 44;
            btnSuchenUndErsetzen.Text = "Suchen und ersetzen";
            btnSuchenUndErsetzen.Click += btnSuchenUndErsetzen_Click;
            // 
            // btnZeileLöschen
            // 
            btnZeileLöschen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnZeileLöschen.ImageCode = "Zeile||||||||||Kreuz";
            btnZeileLöschen.Location = new Point(88, 2);
            btnZeileLöschen.Name = "btnZeileLöschen";
            btnZeileLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            btnZeileLöschen.Size = new Size(72, 66);
            btnZeileLöschen.TabIndex = 42;
            btnZeileLöschen.Text = "Zeilen löschen";
            btnZeileLöschen.Click += btnZeileLöschen_Click;
            // 
            // grpAdminAllgemein
            // 
            grpAdminAllgemein.BackColor = Color.FromArgb(244, 245, 246);
            grpAdminAllgemein.CausesValidation = false;
            grpAdminAllgemein.Controls.Add(btnMonitoring);
            grpAdminAllgemein.Controls.Add(btnUserInfo);
            grpAdminAllgemein.Controls.Add(btnSaveLoad);
            grpAdminAllgemein.Controls.Add(btnPowerBearbeitung);
            grpAdminAllgemein.Controls.Add(btnSpaltenUebersicht);
            grpAdminAllgemein.Dock = DockStyle.Left;
            grpAdminAllgemein.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpAdminAllgemein.Location = new Point(376, 0);
            grpAdminAllgemein.Name = "grpAdminAllgemein";
            grpAdminAllgemein.Size = new Size(344, 81);
            grpAdminAllgemein.TabIndex = 7;
            grpAdminAllgemein.TabStop = false;
            grpAdminAllgemein.Text = "Allgemein";
            // 
            // btnMonitoring
            // 
            btnMonitoring.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnMonitoring.ImageCode = "Monitor|16";
            btnMonitoring.Location = new Point(264, 2);
            btnMonitoring.Name = "btnMonitoring";
            btnMonitoring.QuickInfo = "A";
            btnMonitoring.Size = new Size(72, 66);
            btnMonitoring.TabIndex = 45;
            btnMonitoring.Text = "Monitoring starten";
            btnMonitoring.Click += btnMonitoring_Click;
            // 
            // btnUserInfo
            // 
            btnUserInfo.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnUserInfo.ImageCode = "Person|16|||||200";
            btnUserInfo.Location = new Point(208, 2);
            btnUserInfo.Name = "btnUserInfo";
            btnUserInfo.QuickInfo = "A";
            btnUserInfo.Size = new Size(56, 66);
            btnUserInfo.TabIndex = 44;
            btnUserInfo.Text = "Benutzer Info";
            btnUserInfo.Click += btnUserInfo_Click;
            // 
            // btnSaveLoad
            // 
            btnSaveLoad.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSaveLoad.ImageCode = "Diskette|16";
            btnSaveLoad.Location = new Point(80, 2);
            btnSaveLoad.Name = "btnSaveLoad";
            btnSaveLoad.QuickInfo = "Aktualisiert die Tabellen-Daten. (Speichern, neu Laden)";
            btnSaveLoad.Size = new Size(48, 66);
            btnSaveLoad.TabIndex = 43;
            btnSaveLoad.Text = "Daten aktual.";
            btnSaveLoad.Click += btnSaveLoad_Click;
            // 
            // btnPowerBearbeitung
            // 
            btnPowerBearbeitung.ButtonStyle = ButtonStyle.Checkbox_Big_Borderless;
            btnPowerBearbeitung.ImageCode = "Stift||||FF0000||||||Uhr";
            btnPowerBearbeitung.Location = new Point(128, 2);
            btnPowerBearbeitung.Name = "btnPowerBearbeitung";
            btnPowerBearbeitung.QuickInfo = "5 Minuten (fast) rechtefreies Bearbeiten aktivieren/deaktivieren.";
            btnPowerBearbeitung.Size = new Size(72, 66);
            btnPowerBearbeitung.TabIndex = 43;
            btnPowerBearbeitung.Text = "Power-Bearbeitung";
            btnPowerBearbeitung.CheckedChanged += btnPowerBearbeitung_CheckedChanged;
            // 
            // btnSpaltenUebersicht
            // 
            btnSpaltenUebersicht.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSpaltenUebersicht.ImageCode = "Spalte||||||||||Information";
            btnSpaltenUebersicht.Location = new Point(8, 2);
            btnSpaltenUebersicht.Name = "btnSpaltenUebersicht";
            btnSpaltenUebersicht.Size = new Size(64, 66);
            btnSpaltenUebersicht.TabIndex = 36;
            btnSpaltenUebersicht.Text = "Spalten-Übersicht";
            btnSpaltenUebersicht.Click += btnSpaltenUebersicht_Click;
            // 
            // grpAdminBearbeiten
            // 
            grpAdminBearbeiten.BackColor = Color.FromArgb(244, 245, 246);
            grpAdminBearbeiten.CausesValidation = false;
            grpAdminBearbeiten.Controls.Add(btnSuchInScript);
            grpAdminBearbeiten.Controls.Add(btnSkripteBearbeiten);
            grpAdminBearbeiten.Controls.Add(btnFormular);
            grpAdminBearbeiten.Controls.Add(btnSpaltenanordnung);
            grpAdminBearbeiten.Controls.Add(btnTabelleKopf);
            grpAdminBearbeiten.Controls.Add(btnLayouts);
            grpAdminBearbeiten.Dock = DockStyle.Left;
            grpAdminBearbeiten.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpAdminBearbeiten.Location = new Point(0, 0);
            grpAdminBearbeiten.Name = "grpAdminBearbeiten";
            grpAdminBearbeiten.Size = new Size(376, 81);
            grpAdminBearbeiten.TabIndex = 9;
            grpAdminBearbeiten.TabStop = false;
            grpAdminBearbeiten.Text = "Bearbeiten";
            // 
            // btnSuchInScript
            // 
            btnSuchInScript.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSuchInScript.ImageCode = "Skript||||||||||Stift";
            btnSuchInScript.Location = new Point(304, 2);
            btnSuchInScript.Name = "btnSuchInScript";
            btnSuchInScript.Size = new Size(64, 66);
            btnSuchInScript.TabIndex = 46;
            btnSuchInScript.Text = "in Skripten suchen";
            btnSuchInScript.Click += btnSuchInScript_Click;
            // 
            // btnSkripteBearbeiten
            // 
            btnSkripteBearbeiten.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSkripteBearbeiten.ImageCode = "Skript||||||||||Stift";
            btnSkripteBearbeiten.Location = new Point(240, 2);
            btnSkripteBearbeiten.Name = "btnSkripteBearbeiten";
            btnSkripteBearbeiten.Size = new Size(56, 66);
            btnSkripteBearbeiten.TabIndex = 45;
            btnSkripteBearbeiten.Text = "Skripte";
            btnSkripteBearbeiten.Click += ContextMenu_OpenScriptEditor;
            // 
            // btnFormular
            // 
            btnFormular.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnFormular.ImageCode = "Anwendung||||||||||Stift";
            btnFormular.Location = new Point(64, 2);
            btnFormular.Name = "btnFormular";
            btnFormular.Size = new Size(56, 66);
            btnFormular.TabIndex = 44;
            btnFormular.Text = "Formular-Editor";
            btnFormular.Click += btnFormular_Click;
            // 
            // btnSpaltenanordnung
            // 
            btnSpaltenanordnung.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSpaltenanordnung.ImageCode = "Spalte||||||||||Stift";
            btnSpaltenanordnung.Location = new Point(176, 2);
            btnSpaltenanordnung.Name = "btnSpaltenanordnung";
            btnSpaltenanordnung.Size = new Size(64, 66);
            btnSpaltenanordnung.TabIndex = 43;
            btnSpaltenanordnung.Text = "Spalten-anordung";
            btnSpaltenanordnung.Click += btnSpaltenanordnung_Click;
            // 
            // btnTabelleKopf
            // 
            btnTabelleKopf.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnTabelleKopf.ImageCode = "Tabelle||||||||||Stift";
            btnTabelleKopf.Location = new Point(0, 2);
            btnTabelleKopf.Name = "btnTabelleKopf";
            btnTabelleKopf.Size = new Size(64, 66);
            btnTabelleKopf.TabIndex = 37;
            btnTabelleKopf.Text = "Tabellen-Kopf";
            btnTabelleKopf.Click += btnTabelleKopf_Click;
            // 
            // btnLayouts
            // 
            btnLayouts.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnLayouts.ImageCode = "Layout||||||||||Stift";
            btnLayouts.Location = new Point(120, 2);
            btnLayouts.Name = "btnLayouts";
            btnLayouts.Size = new Size(56, 66);
            btnLayouts.TabIndex = 41;
            btnLayouts.Text = "Layout-Editor";
            btnLayouts.Click += btnLayouts_Click;
            // 
            // tabExport
            // 
            tabExport.BackColor = Color.FromArgb(244, 245, 246);
            tabExport.Controls.Add(grpExport);
            tabExport.Controls.Add(grpImport);
            tabExport.Location = new Point(4, 25);
            tabExport.Name = "tabExport";
            tabExport.Size = new Size(1321, 81);
            tabExport.TabIndex = 2;
            tabExport.Text = "Import/Export";
            // 
            // grpExport
            // 
            grpExport.BackColor = Color.FromArgb(244, 245, 246);
            grpExport.CausesValidation = false;
            grpExport.Controls.Add(btnDrucken);
            grpExport.Controls.Add(btnHTMLExport);
            grpExport.Controls.Add(btnCSVClipboard);
            grpExport.Dock = DockStyle.Left;
            grpExport.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpExport.Location = new Point(144, 0);
            grpExport.Name = "grpExport";
            grpExport.Size = new Size(224, 81);
            grpExport.TabIndex = 3;
            grpExport.TabStop = false;
            grpExport.Text = "Export";
            // 
            // btnDrucken
            // 
            btnDrucken.AutoSort = false;
            btnDrucken.DrawStyle = ComboboxStyle.RibbonBar;
            btnDrucken.DropDownStyle = ComboBoxStyle.DropDownList;
            btnDrucken.ImageCode = "Drucker";
            btnDrucken.Location = new Point(136, 2);
            btnDrucken.Name = "btnDrucken";
            btnDrucken.Size = new Size(80, 66);
            btnDrucken.TabIndex = 13;
            btnDrucken.Text = "Drucken bzw. Export";
            btnDrucken.ItemClicked += btnDrucken_ItemClicked;
            // 
            // btnHTMLExport
            // 
            btnHTMLExport.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnHTMLExport.ImageCode = "Globus";
            btnHTMLExport.Location = new Point(72, 2);
            btnHTMLExport.Name = "btnHTMLExport";
            btnHTMLExport.QuickInfo = "HTML-Format (für einen Internet-Browser) als Datei";
            btnHTMLExport.Size = new Size(64, 66);
            btnHTMLExport.TabIndex = 3;
            btnHTMLExport.Text = "HTML";
            btnHTMLExport.Click += btnHTMLExport_Click;
            // 
            // btnCSVClipboard
            // 
            btnCSVClipboard.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnCSVClipboard.ImageCode = "Clipboard||||||||||Pfeil_Rechts";
            btnCSVClipboard.Location = new Point(8, 2);
            btnCSVClipboard.Name = "btnCSVClipboard";
            btnCSVClipboard.QuickInfo = "CSV-Format (z.B.: für Excel) in die Zwischenablage";
            btnCSVClipboard.Size = new Size(64, 66);
            btnCSVClipboard.TabIndex = 0;
            btnCSVClipboard.Text = "CSV";
            btnCSVClipboard.Click += btnCSVClipboard_Click;
            // 
            // grpImport
            // 
            grpImport.BackColor = Color.FromArgb(244, 245, 246);
            grpImport.CausesValidation = false;
            grpImport.Controls.Add(btnMDBImport);
            grpImport.Controls.Add(btnClipboardImport);
            grpImport.Dock = DockStyle.Left;
            grpImport.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpImport.Location = new Point(0, 0);
            grpImport.Name = "grpImport";
            grpImport.Size = new Size(144, 81);
            grpImport.TabIndex = 4;
            grpImport.TabStop = false;
            grpImport.Text = "Import";
            // 
            // btnMDBImport
            // 
            btnMDBImport.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnMDBImport.ImageCode = "Tabelle||||||||||Tabelle";
            btnMDBImport.Location = new Point(72, 2);
            btnMDBImport.Name = "btnMDBImport";
            btnMDBImport.QuickInfo = "Importiert in die aktuell angezeigte Tabelle\r\nDaten einer andere Tabelle.";
            btnMDBImport.Size = new Size(64, 66);
            btnMDBImport.TabIndex = 40;
            btnMDBImport.Text = "Datei-Import";
            btnMDBImport.Click += btnMDBImport_Click;
            // 
            // btnClipboardImport
            // 
            btnClipboardImport.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnClipboardImport.ImageCode = "Tabelle||||||||||Clipboard";
            btnClipboardImport.Location = new Point(8, 2);
            btnClipboardImport.Name = "btnClipboardImport";
            btnClipboardImport.QuickInfo = "Importiert in die aktuell angezeigte Tabelle\r\nDaten aus dem Clipboard.";
            btnClipboardImport.Size = new Size(64, 66);
            btnClipboardImport.TabIndex = 39;
            btnClipboardImport.Text = "Clipboard-Import";
            btnClipboardImport.Click += btnClipboardImport_Click;
            // 
            // pnlTableSelect
            // 
            pnlTableSelect.Controls.Add(tbcTableSelector);
            pnlTableSelect.Dock = DockStyle.Top;
            pnlTableSelect.Location = new Point(0, 0);
            pnlTableSelect.Name = "pnlTableSelect";
            pnlTableSelect.Size = new Size(972, 24);
            pnlTableSelect.TabIndex = 21;
            // 
            // tbcTableSelector
            // 
            tbcTableSelector.Dock = DockStyle.Fill;
            tbcTableSelector.HotTrack = true;
            tbcTableSelector.Location = new Point(0, 0);
            tbcTableSelector.Name = "tbcTableSelector";
            tbcTableSelector.SelectedIndex = 0;
            tbcTableSelector.Size = new Size(972, 24);
            tbcTableSelector.TabDefault = null;
            tbcTableSelector.TabDefaultOrder = null;
            tbcTableSelector.TabIndex = 20;
            tbcTableSelector.Selected += tbcTableSelector_Selected;
            tbcTableSelector.Deselecting += tbcTableSelector_Deselecting;
            // 
            // TableView
            // 
            TableView.Dock = DockStyle.Fill;
            TableView.Location = new Point(0, 24);
            TableView.Name = "TableView";
            TableView.Size = new Size(972, 571);
            TableView.TabIndex = 0;
            TableView.SelectedCellChanged += Table_SelectedCellChanged;
            TableView.CellClicked += Table_CellClicked;
            TableView.SelectedRowChanged += Table_SelectedRowChanged;
            TableView.TableChanged += Table_TableChanged;
            TableView.ViewChanged += Table_ViewChanged;
            TableView.VisibleRowsChanged += Table_VisibleRowsChanged;
            // 
            // SplitContainer1
            // 
            SplitContainer1.Dock = DockStyle.Fill;
            SplitContainer1.Location = new Point(0, 110);
            SplitContainer1.Margin = new Padding(27, 10, 27, 25);
            SplitContainer1.Name = "SplitContainer1";
            // 
            // SplitContainer1.Panel1
            // 
            SplitContainer1.Panel1.Controls.Add(TableView);
            SplitContainer1.Panel1.Controls.Add(pnlTableSelect);
            // 
            // SplitContainer1.Panel2
            // 
            SplitContainer1.Panel2.Controls.Add(tbcSidebar);
            SplitContainer1.Panel2.Margin = new Padding(27, 25, 27, 25);
            SplitContainer1.Size = new Size(1329, 595);
            SplitContainer1.SplitterDistance = 972;
            SplitContainer1.SplitterWidth = 11;
            SplitContainer1.TabIndex = 94;
            // 
            // tbcSidebar
            // 
            tbcSidebar.Controls.Add(tabFormula);
            tbcSidebar.Dock = DockStyle.Fill;
            tbcSidebar.HotTrack = true;
            tbcSidebar.Location = new Point(0, 0);
            tbcSidebar.Name = "tbcSidebar";
            tbcSidebar.SelectedIndex = 0;
            tbcSidebar.Size = new Size(346, 595);
            tbcSidebar.TabDefault = null;
            tbcSidebar.TabDefaultOrder = null;
            tbcSidebar.TabIndex = 21;
            // 
            // tabFormula
            // 
            tabFormula.BackColor = Color.FromArgb(255, 255, 255);
            tabFormula.Controls.Add(CFO);
            tabFormula.Location = new Point(4, 25);
            tabFormula.Name = "tabFormula";
            tabFormula.Size = new Size(338, 566);
            tabFormula.TabIndex = 1;
            tabFormula.Text = "Formular";
            // 
            // CFO
            // 
            CFO.CausesValidation = false;
            CFO.Dock = DockStyle.Fill;
            CFO.FilenameForEditor = "";
            CFO.GroupBoxStyle = GroupBoxStyle.Nothing;
            CFO.Location = new Point(0, 0);
            CFO.Name = "CFO";
            CFO.Page = null;
            CFO.Size = new Size(338, 566);
            CFO.TabIndex = 0;
            CFO.TabStop = false;
            // 
            // capZeilen2
            // 
            capZeilen2.CausesValidation = false;
            capZeilen2.Dock = DockStyle.Left;
            capZeilen2.Location = new Point(0, 0);
            capZeilen2.Name = "capZeilen2";
            capZeilen2.Size = new Size(304, 24);
            capZeilen2.Translate = false;
            // 
            // LoadTab
            // 
            LoadTab.Filter = "Tabellen (*.BDB;*.MBDB;*.CBDB;*.CSV)|*.BDB;*.MBDB;*.CBDB;*.CSV|Alle Dateien (*.*)|*.*";
            LoadTab.Title = "Bitte Tabelle laden!";
            LoadTab.FileOk += LoadTab_FileOk;
            // 
            // SaveTab
            // 
            SaveTab.Filter = "*.BDB Tabellen|*.BDB|*.MBDB Tabellen|*.MBDB|*.CBDB Tabellen|*.CBDB|*.CSV Tabellen|*.CSV|*.* Alle Dateien|*";
            SaveTab.Title = "Bitte neuen Dateinamen der Tabelle wählen.";
            // 
            // grpAufräumen
            // 
            grpAufräumen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            grpAufräumen.ImageCode = "Zeile||||||||||Pinsel";
            grpAufräumen.Location = new Point(160, 2);
            grpAufräumen.Name = "grpAufräumen";
            grpAufräumen.QuickInfo = "Angezeigte Zeilen löschen";
            grpAufräumen.Size = new Size(72, 66);
            grpAufräumen.TabIndex = 45;
            grpAufräumen.Text = "Zeilen löschen";
            // 
            // TableViewForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1329, 729);
            Controls.Add(SplitContainer1);
            Controls.Add(ribMain);
            MinimumSize = new Size(800, 600);
            Name = "TableViewForm";
            Text = "TableView";
            WindowState = FormWindowState.Maximized;
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(ribMain, 0);
            Controls.SetChildIndex(SplitContainer1, 0);
            pnlStatusBar.ResumeLayout(false);
            ribMain.ResumeLayout(false);
            tabFile.ResumeLayout(false);
            grpOrdner.ResumeLayout(false);
            grpDatei.ResumeLayout(false);
            tabAllgemein.ResumeLayout(false);
            grpAufgaben.ResumeLayout(false);
            grpAnsicht.ResumeLayout(false);
            grpHilfen.ResumeLayout(false);
            tabAdmin.ResumeLayout(false);
            grpAdminZeilen.ResumeLayout(false);
            grpAdminAllgemein.ResumeLayout(false);
            grpAdminBearbeiten.ResumeLayout(false);
            tabExport.ResumeLayout(false);
            grpExport.ResumeLayout(false);
            grpImport.ResumeLayout(false);
            pnlTableSelect.ResumeLayout(false);
            SplitContainer1.Panel1.ResumeLayout(false);
            SplitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)SplitContainer1).EndInit();
            SplitContainer1.ResumeLayout(false);
            tbcSidebar.ResumeLayout(false);
            tabFormula.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        protected RibbonBar ribMain;
        protected Panel pnlTableSelect;
        protected TabControl tbcTableSelector;
        protected Controls.TableViewWithFilters TableView;
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