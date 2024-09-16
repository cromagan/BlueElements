using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls.Forms {
    partial class ConnectedFormulaEditor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectedFormulaEditor));
            this.tabEditorStd = new System.Windows.Forms.TabPage();
            this.grpVorschau = new BlueControls.Controls.GroupBox();
            this.btnPfeileAusblenden = new BlueControls.Controls.Button();
            this.grpFelder = new BlueControls.Controls.GroupBox();
            this.btnRegionAdd = new BlueControls.Controls.Button();
            this.btnButton = new BlueControls.Controls.Button();
            this.btnBild = new BlueControls.Controls.Button();
            this.btnFileExplorer = new BlueControls.Controls.Button();
            this.btnFeldHinzu = new BlueControls.Controls.Button();
            this.grpOptik = new BlueControls.Controls.GroupBox();
            this.btnRegisterKarte = new BlueControls.Controls.Button();
            this.btnTabControlAdd = new BlueControls.Controls.Button();
            this.groupBox1 = new BlueControls.Controls.GroupBox();
            this.btnTable = new BlueControls.Controls.Button();
            this.btnDropdownmenu = new BlueControls.Controls.Button();
            this.btnFilterConverter = new BlueControls.Controls.Button();
            this.btnBenutzerFilterWahl = new BlueControls.Controls.Button();
            this.grpArbeitsbereich = new BlueControls.Controls.GroupBox();
            this.btnArbeitsbereich = new BlueControls.Controls.Button();
            this.tabFile = new System.Windows.Forms.TabPage();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnSpeichern = new BlueControls.Controls.Button();
            this.btnLetzteFormulare = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSaveAs = new BlueControls.Controls.Button();
            this.btnNeuDB = new BlueControls.Controls.Button();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnWeitereCF = new BlueControls.Controls.Button();
            this.grpDesign.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.tabSeiten.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.tabEditorStd.SuspendLayout();
            this.grpVorschau.SuspendLayout();
            this.grpFelder.SuspendLayout();
            this.grpOptik.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpArbeitsbereich.SuspendLayout();
            this.tabFile.SuspendLayout();
            this.grpDatei.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.Visible = false;
            // 
            // grpDesign
            // 
            this.grpDesign.Visible = false;
            // 
            // tabHintergrund
            // 
            this.tabHintergrund.Size = new System.Drawing.Size(1267, 81);
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new System.Drawing.Point(903, 136);
            this.tabRightSide.Size = new System.Drawing.Size(372, 339);
            // 
            // Pad
            // 
            this.Pad.Location = new System.Drawing.Point(0, 136);
            this.Pad.Size = new System.Drawing.Size(903, 339);
            this.Pad.GotNewItemCollection += new System.EventHandler(this.Pad_GotNewItemCollection);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabEditorStd);
            this.Ribbon.Controls.Add(this.tabFile);
            this.Ribbon.Size = new System.Drawing.Size(1275, 110);
            this.Ribbon.TabDefault = this.tabFile;
            this.Ribbon.TabDefaultOrder = new string[] {
        "Datei",
        "Editor-Std.",
        "Start"};
            this.Ribbon.Controls.SetChildIndex(this.tabFile, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabEditorStd, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabHintergrund, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(1267, 81);
            // 
            // grpAssistent
            // 
            this.grpAssistent.Visible = false;
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.CheckedChanged += new System.EventHandler(this.btnVorschauModus_CheckedChanged);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Controls.Add(this.tabPage1);
            this.tabSeiten.Size = new System.Drawing.Size(1275, 26);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1275, 24);
            this.capStatusBar.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Text_Abschneiden;
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 475);
            this.pnlStatusBar.Size = new System.Drawing.Size(1275, 24);
            // 
            // tabEditorStd
            // 
            this.tabEditorStd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabEditorStd.Controls.Add(this.grpVorschau);
            this.tabEditorStd.Controls.Add(this.grpFelder);
            this.tabEditorStd.Controls.Add(this.grpOptik);
            this.tabEditorStd.Controls.Add(this.groupBox1);
            this.tabEditorStd.Controls.Add(this.grpArbeitsbereich);
            this.tabEditorStd.Location = new System.Drawing.Point(4, 25);
            this.tabEditorStd.Margin = new System.Windows.Forms.Padding(0);
            this.tabEditorStd.Name = "tabEditorStd";
            this.tabEditorStd.Size = new System.Drawing.Size(1267, 81);
            this.tabEditorStd.TabIndex = 4;
            this.tabEditorStd.Text = "Editor-Std.";
            // 
            // grpVorschau
            // 
            this.grpVorschau.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpVorschau.Controls.Add(this.btnPfeileAusblenden);
            this.grpVorschau.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVorschau.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpVorschau.Location = new System.Drawing.Point(760, 0);
            this.grpVorschau.Name = "grpVorschau";
            this.grpVorschau.Size = new System.Drawing.Size(88, 81);
            this.grpVorschau.TabIndex = 1;
            this.grpVorschau.TabStop = false;
            this.grpVorschau.Text = "Vorschau";
            // 
            // btnPfeileAusblenden
            // 
            this.btnPfeileAusblenden.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Big_Borderless;
            this.btnPfeileAusblenden.ImageCode = "Pfeil_Rechts|16||1||||0";
            this.btnPfeileAusblenden.Location = new System.Drawing.Point(8, 2);
            this.btnPfeileAusblenden.Name = "btnPfeileAusblenden";
            this.btnPfeileAusblenden.Size = new System.Drawing.Size(72, 66);
            this.btnPfeileAusblenden.TabIndex = 0;
            this.btnPfeileAusblenden.Text = "Pfeile etc. ausblenden";
            this.btnPfeileAusblenden.CheckedChanged += new System.EventHandler(this.btnPfeileAusblenden_CheckedChanged);
            // 
            // grpFelder
            // 
            this.grpFelder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpFelder.Controls.Add(this.btnWeitereCF);
            this.grpFelder.Controls.Add(this.btnRegionAdd);
            this.grpFelder.Controls.Add(this.btnButton);
            this.grpFelder.Controls.Add(this.btnBild);
            this.grpFelder.Controls.Add(this.btnFileExplorer);
            this.grpFelder.Controls.Add(this.btnFeldHinzu);
            this.grpFelder.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFelder.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpFelder.Location = new System.Drawing.Point(504, 0);
            this.grpFelder.Name = "grpFelder";
            this.grpFelder.Size = new System.Drawing.Size(256, 81);
            this.grpFelder.TabIndex = 0;
            this.grpFelder.TabStop = false;
            this.grpFelder.Text = "Felder";
            // 
            // btnRegionAdd
            // 
            this.btnRegionAdd.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnRegionAdd.ImageCode = "Layout|16";
            this.btnRegionAdd.Location = new System.Drawing.Point(8, 46);
            this.btnRegionAdd.Name = "btnRegionAdd";
            this.btnRegionAdd.Size = new System.Drawing.Size(136, 22);
            this.btnRegionAdd.TabIndex = 7;
            this.btnRegionAdd.Text = "Region";
            this.btnRegionAdd.Click += new System.EventHandler(this.btnRegionAdd_Click);
            // 
            // btnButton
            // 
            this.btnButton.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnButton.ImageCode = "Stop";
            this.btnButton.Location = new System.Drawing.Point(8, 24);
            this.btnButton.Name = "btnButton";
            this.btnButton.Size = new System.Drawing.Size(136, 22);
            this.btnButton.TabIndex = 6;
            this.btnButton.Text = "Knopf";
            this.btnButton.Click += new System.EventHandler(this.btnButton_Click);
            // 
            // btnBild
            // 
            this.btnBild.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnBild.ImageCode = "Bild";
            this.btnBild.Location = new System.Drawing.Point(144, 24);
            this.btnBild.Name = "btnBild";
            this.btnBild.Size = new System.Drawing.Size(104, 22);
            this.btnBild.TabIndex = 5;
            this.btnBild.Text = "Bild";
            this.btnBild.Click += new System.EventHandler(this.btnBild_Click);
            // 
            // btnFileExplorer
            // 
            this.btnFileExplorer.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnFileExplorer.ImageCode = "Ordner";
            this.btnFileExplorer.Location = new System.Drawing.Point(144, 2);
            this.btnFileExplorer.Name = "btnFileExplorer";
            this.btnFileExplorer.Size = new System.Drawing.Size(104, 22);
            this.btnFileExplorer.TabIndex = 3;
            this.btnFileExplorer.Text = "Verzeichniss";
            this.btnFileExplorer.Click += new System.EventHandler(this.grpFileExplorer_Click);
            // 
            // btnFeldHinzu
            // 
            this.btnFeldHinzu.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnFeldHinzu.ImageCode = "Textfeld2|24";
            this.btnFeldHinzu.Location = new System.Drawing.Point(8, 2);
            this.btnFeldHinzu.Name = "btnFeldHinzu";
            this.btnFeldHinzu.Size = new System.Drawing.Size(136, 22);
            this.btnFeldHinzu.TabIndex = 1;
            this.btnFeldHinzu.Text = "Zelle";
            this.btnFeldHinzu.Click += new System.EventHandler(this.btnFeldHinzu_Click);
            // 
            // grpOptik
            // 
            this.grpOptik.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpOptik.Controls.Add(this.btnRegisterKarte);
            this.grpOptik.Controls.Add(this.btnTabControlAdd);
            this.grpOptik.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpOptik.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpOptik.Location = new System.Drawing.Point(344, 0);
            this.grpOptik.Name = "grpOptik";
            this.grpOptik.Size = new System.Drawing.Size(160, 81);
            this.grpOptik.TabIndex = 2;
            this.grpOptik.TabStop = false;
            this.grpOptik.Text = "Optik";
            // 
            // btnRegisterKarte
            // 
            this.btnRegisterKarte.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnRegisterKarte.ImageCode = "Register|16|||||||||PlusZeichen";
            this.btnRegisterKarte.Location = new System.Drawing.Point(72, 2);
            this.btnRegisterKarte.Name = "btnRegisterKarte";
            this.btnRegisterKarte.QuickInfo = resources.GetString("btnRegisterKarte.QuickInfo");
            this.btnRegisterKarte.Size = new System.Drawing.Size(80, 66);
            this.btnRegisterKarte.TabIndex = 6;
            this.btnRegisterKarte.Text = "Neue Registerkarte";
            this.btnRegisterKarte.Click += new System.EventHandler(this.btnRegisterKarte_Click);
            // 
            // btnTabControlAdd
            // 
            this.btnTabControlAdd.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTabControlAdd.ImageCode = "Registersammlung";
            this.btnTabControlAdd.Location = new System.Drawing.Point(8, 2);
            this.btnTabControlAdd.Name = "btnTabControlAdd";
            this.btnTabControlAdd.Size = new System.Drawing.Size(64, 66);
            this.btnTabControlAdd.TabIndex = 3;
            this.btnTabControlAdd.Text = "Register-sammlung";
            this.btnTabControlAdd.Click += new System.EventHandler(this.btnTabControlAdd_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.groupBox1.Controls.Add(this.btnTable);
            this.groupBox1.Controls.Add(this.btnDropdownmenu);
            this.groupBox1.Controls.Add(this.btnFilterConverter);
            this.groupBox1.Controls.Add(this.btnBenutzerFilterWahl);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.groupBox1.Location = new System.Drawing.Point(72, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 81);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Zeilen-Berechnung";
            // 
            // btnTable
            // 
            this.btnTable.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTable.ImageCode = "Tabelle|16";
            this.btnTable.Location = new System.Drawing.Point(200, 2);
            this.btnTable.Name = "btnTable";
            this.btnTable.Size = new System.Drawing.Size(64, 66);
            this.btnTable.TabIndex = 5;
            this.btnTable.Text = "Tabelle";
            this.btnTable.Click += new System.EventHandler(this.btnTable_Click);
            // 
            // btnDropdownmenu
            // 
            this.btnDropdownmenu.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnDropdownmenu.ImageCode = "Textfeld2|16|||||||||Pfeil_Unten_Scrollbar";
            this.btnDropdownmenu.Location = new System.Drawing.Point(136, 2);
            this.btnDropdownmenu.Name = "btnDropdownmenu";
            this.btnDropdownmenu.Size = new System.Drawing.Size(64, 66);
            this.btnDropdownmenu.TabIndex = 4;
            this.btnDropdownmenu.Text = "Auswahl-feld";
            this.btnDropdownmenu.Click += new System.EventHandler(this.btnDropdownmenu_Click);
            // 
            // btnFilterConverter
            // 
            this.btnFilterConverter.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnFilterConverter.ImageCode = "Trichter|16|||||||||PlusZeichen";
            this.btnFilterConverter.Location = new System.Drawing.Point(72, 2);
            this.btnFilterConverter.Name = "btnFilterConverter";
            this.btnFilterConverter.Size = new System.Drawing.Size(64, 66);
            this.btnFilterConverter.TabIndex = 3;
            this.btnFilterConverter.Text = "Filter";
            this.btnFilterConverter.Click += new System.EventHandler(this.btnFilterConverter_Click);
            // 
            // btnBenutzerFilterWahl
            // 
            this.btnBenutzerFilterWahl.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnBenutzerFilterWahl.ImageCode = "Trichter|16|||||||||Textfeld2";
            this.btnBenutzerFilterWahl.Location = new System.Drawing.Point(8, 2);
            this.btnBenutzerFilterWahl.Name = "btnBenutzerFilterWahl";
            this.btnBenutzerFilterWahl.Size = new System.Drawing.Size(64, 66);
            this.btnBenutzerFilterWahl.TabIndex = 2;
            this.btnBenutzerFilterWahl.Text = "Benutzer-Filter Wahl";
            this.btnBenutzerFilterWahl.Click += new System.EventHandler(this.btnBenutzerFilterWahl_Click);
            // 
            // grpArbeitsbereich
            // 
            this.grpArbeitsbereich.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpArbeitsbereich.Controls.Add(this.btnArbeitsbereich);
            this.grpArbeitsbereich.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpArbeitsbereich.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpArbeitsbereich.Location = new System.Drawing.Point(0, 0);
            this.grpArbeitsbereich.Name = "grpArbeitsbereich";
            this.grpArbeitsbereich.Size = new System.Drawing.Size(72, 81);
            this.grpArbeitsbereich.TabIndex = 4;
            this.grpArbeitsbereich.TabStop = false;
            this.grpArbeitsbereich.Text = "Vorschau";
            // 
            // btnArbeitsbereich
            // 
            this.btnArbeitsbereich.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Big_Borderless;
            this.btnArbeitsbereich.ImageCode = "SeiteEinrichten";
            this.btnArbeitsbereich.Location = new System.Drawing.Point(8, 2);
            this.btnArbeitsbereich.Name = "btnArbeitsbereich";
            this.btnArbeitsbereich.Size = new System.Drawing.Size(56, 66);
            this.btnArbeitsbereich.TabIndex = 0;
            this.btnArbeitsbereich.Text = "Arbeits-bereich";
            this.btnArbeitsbereich.Click += new System.EventHandler(this.btnArbeitsbereich_Click);
            // 
            // tabFile
            // 
            this.tabFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabFile.Controls.Add(this.grpDatei);
            this.tabFile.Location = new System.Drawing.Point(4, 25);
            this.tabFile.Margin = new System.Windows.Forms.Padding(0);
            this.tabFile.Name = "tabFile";
            this.tabFile.Size = new System.Drawing.Size(876, 81);
            this.tabFile.TabIndex = 5;
            this.tabFile.Text = "Datei";
            // 
            // grpDatei
            // 
            this.grpDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Controls.Add(this.btnSpeichern);
            this.grpDatei.Controls.Add(this.btnLetzteFormulare);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Controls.Add(this.btnSaveAs);
            this.grpDatei.Controls.Add(this.btnNeuDB);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(368, 81);
            this.grpDatei.TabIndex = 5;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
            // 
            // btnSpeichern
            // 
            this.btnSpeichern.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSpeichern.ImageCode = "Diskette";
            this.btnSpeichern.Location = new System.Drawing.Point(232, 2);
            this.btnSpeichern.Name = "btnSpeichern";
            this.btnSpeichern.Size = new System.Drawing.Size(64, 66);
            this.btnSpeichern.TabIndex = 5;
            this.btnSpeichern.Text = "Speichern";
            this.btnSpeichern.Click += new System.EventHandler(this.btnSpeichern_Click);
            // 
            // btnLetzteFormulare
            // 
            this.btnLetzteFormulare.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLetzteFormulare.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnLetzteFormulare.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLetzteFormulare.Enabled = false;
            this.btnLetzteFormulare.ImageCode = "Ordner";
            this.btnLetzteFormulare.Location = new System.Drawing.Point(128, 2);
            this.btnLetzteFormulare.Name = "btnLetzteFormulare";
            this.btnLetzteFormulare.SettingsLoaded = false;
            this.btnLetzteFormulare.Size = new System.Drawing.Size(104, 66);
            this.btnLetzteFormulare.TabIndex = 1;
            this.btnLetzteFormulare.Text = "zuletzt geöffnete Dateien";
            this.btnLetzteFormulare.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnLetzteDateien_ItemClicked);
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
            this.btnSaveAs.Enabled = false;
            this.btnSaveAs.ImageCode = "Diskette";
            this.btnSaveAs.Location = new System.Drawing.Point(296, 2);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(64, 66);
            this.btnSaveAs.TabIndex = 4;
            this.btnSaveAs.Text = "Speichern unter";
            this.btnSaveAs.Click += new System.EventHandler(this.btnNeuDB_SaveAs_Click);
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
            this.btnNeuDB.Click += new System.EventHandler(this.btnNeuDB_SaveAs_Click);
            // 
            // LoadTab
            // 
            this.LoadTab.Filter = "*.CFO Formulare|*.CFO|*.* Alle Dateien|*";
            this.LoadTab.Title = "Bitte Formular wählen:";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // SaveTab
            // 
            this.SaveTab.DefaultExt = "CFO";
            this.SaveTab.Filter = "*.CFO Formulare|*.CFO|*.* Alle Dateien|*";
            this.SaveTab.Title = "Bitte neuen Dateinamen des Formulars wählen.";
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1267, 0);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Visible = false;
            // 
            // btnWeitereCF
            // 
            this.btnWeitereCF.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnWeitereCF.Location = new System.Drawing.Point(144, 46);
            this.btnWeitereCF.Name = "btnWeitereCF";
            this.btnWeitereCF.Size = new System.Drawing.Size(104, 22);
            this.btnWeitereCF.TabIndex = 8;
            this.btnWeitereCF.Text = "weitere...";
            this.btnWeitereCF.Click += new System.EventHandler(this.btnWeitereCF_Click);
            // 
            // ConnectedFormulaEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1275, 499);
            this.Name = "ConnectedFormulaEditor";
            this.Text = "ConnectedFormula";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.grpDesign.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.tabSeiten.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.tabEditorStd.ResumeLayout(false);
            this.grpVorschau.ResumeLayout(false);
            this.grpFelder.ResumeLayout(false);
            this.grpOptik.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.grpArbeitsbereich.ResumeLayout(false);
            this.tabFile.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TabPage tabEditorStd;
        private GroupBox grpFelder;
        private Button btnFeldHinzu;
        private GroupBox grpVorschau;
        private Button btnPfeileAusblenden;
        private Button btnTabControlAdd;
        private TabPage tabFile;
        private GroupBox grpDatei;
        private LastFilesCombo btnLetzteFormulare;
        private Button btnOeffnen;
        private Button btnSaveAs;
        private Button btnNeuDB;
        private OpenFileDialog LoadTab;
        private SaveFileDialog SaveTab;
        private Button btnFileExplorer;
        private GroupBox groupBox1;
        private GroupBox grpOptik;
        private TabPage tabPage1;
        private Button btnRegisterKarte;
        private Button btnBild;
        private Button btnSpeichern;
        private Button btnButton;
        private Button btnTable;
        private Button btnDropdownmenu;
        private Button btnFilterConverter;
        private Button btnBenutzerFilterWahl;
        private GroupBox grpArbeitsbereich;
        private Button btnArbeitsbereich;
        private Button btnRegionAdd;
        private Button btnWeitereCF;
    }
}