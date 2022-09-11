
namespace BlueControls.Forms {
    partial class ConnectedFormulaEditor {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectedFormulaEditor));
            this.tabEditor = new System.Windows.Forms.TabPage();
            this.grpVorschau = new BlueControls.Controls.GroupBox();
            this.btnVorschauÖffnen = new BlueControls.Controls.Button();
            this.btnPfeileAusblenden = new BlueControls.Controls.Button();
            this.grpFelder = new BlueControls.Controls.GroupBox();
            this.btnVariable = new BlueControls.Controls.Button();
            this.btnFileExplorer = new BlueControls.Controls.Button();
            this.btnKonstante = new BlueControls.Controls.Button();
            this.btnFeldHinzu = new BlueControls.Controls.Button();
            this.groupBox1 = new BlueControls.Controls.GroupBox();
            this.btnEingangsZeile = new BlueControls.Controls.Button();
            this.btnZeileHinzu = new BlueControls.Controls.Button();
            this.grpOptik = new BlueControls.Controls.GroupBox();
            this.btnRegisterKarte = new BlueControls.Controls.Button();
            this.btnTabControlAdd = new BlueControls.Controls.Button();
            this.tabFile = new System.Windows.Forms.TabPage();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnLetzteFormulare = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSaveAs = new BlueControls.Controls.Button();
            this.btnNeuDB = new BlueControls.Controls.Button();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.LoadTabDatabase = new System.Windows.Forms.OpenFileDialog();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnBild = new BlueControls.Controls.Button();
            this.grpDesign.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.tabSeiten.SuspendLayout();
            this.tabEditor.SuspendLayout();
            this.grpVorschau.SuspendLayout();
            this.grpFelder.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpOptik.SuspendLayout();
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
            this.tabHintergrund.Size = new System.Drawing.Size(966, 81);
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new System.Drawing.Point(602, 136);
            this.tabRightSide.Size = new System.Drawing.Size(372, 363);
            // 
            // Pad
            // 
            this.Pad.Location = new System.Drawing.Point(0, 136);
            this.Pad.Size = new System.Drawing.Size(602, 363);
            this.Pad.GotNewItemCollection += new System.EventHandler(this.Pad_GotNewItemCollection);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabEditor);
            this.Ribbon.Controls.Add(this.tabFile);
            this.Ribbon.Size = new System.Drawing.Size(974, 110);
            this.Ribbon.TabDefault = this.tabFile;
            this.Ribbon.TabDefaultOrder = new string[] {
        "Datei",
        "Editor",
        "Start"};
            this.Ribbon.Controls.SetChildIndex(this.tabHintergrund, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabFile, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabEditor, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(966, 81);
            // 
            // tabExport
            // 
            this.tabExport.Size = new System.Drawing.Size(966, 81);
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.CheckedChanged += new System.EventHandler(this.btnVorschauModus_CheckedChanged);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Controls.Add(this.tabPage1);
            this.tabSeiten.Size = new System.Drawing.Size(974, 26);
            // 
            // tabEditor
            // 
            this.tabEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabEditor.Controls.Add(this.grpVorschau);
            this.tabEditor.Controls.Add(this.grpFelder);
            this.tabEditor.Controls.Add(this.grpOptik);
            this.tabEditor.Controls.Add(this.groupBox1);
            this.tabEditor.Location = new System.Drawing.Point(4, 25);
            this.tabEditor.Margin = new System.Windows.Forms.Padding(0);
            this.tabEditor.Name = "tabEditor";
            this.tabEditor.Size = new System.Drawing.Size(966, 81);
            this.tabEditor.TabIndex = 4;
            this.tabEditor.Text = "Editor";
            // 
            // grpVorschau
            // 
            this.grpVorschau.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpVorschau.Controls.Add(this.btnVorschauÖffnen);
            this.grpVorschau.Controls.Add(this.btnPfeileAusblenden);
            this.grpVorschau.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVorschau.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpVorschau.Location = new System.Drawing.Point(648, 0);
            this.grpVorschau.Name = "grpVorschau";
            this.grpVorschau.Size = new System.Drawing.Size(160, 81);
            this.grpVorschau.TabIndex = 1;
            this.grpVorschau.TabStop = false;
            this.grpVorschau.Text = "Vorschau";
            // 
            // btnVorschauÖffnen
            // 
            this.btnVorschauÖffnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVorschauÖffnen.ImageCode = "Anwendung|16";
            this.btnVorschauÖffnen.Location = new System.Drawing.Point(80, 2);
            this.btnVorschauÖffnen.Name = "btnVorschauÖffnen";
            this.btnVorschauÖffnen.Size = new System.Drawing.Size(72, 66);
            this.btnVorschauÖffnen.TabIndex = 1;
            this.btnVorschauÖffnen.Text = "Vorschau öffnen";
            this.btnVorschauÖffnen.Click += new System.EventHandler(this.btnVorschauÖffnen_Click);
            // 
            // btnPfeileAusblenden
            // 
            this.btnPfeileAusblenden.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.grpFelder.Controls.Add(this.btnBild);
            this.grpFelder.Controls.Add(this.btnVariable);
            this.grpFelder.Controls.Add(this.btnTabControlAdd);
            this.grpFelder.Controls.Add(this.btnFileExplorer);
            this.grpFelder.Controls.Add(this.btnKonstante);
            this.grpFelder.Controls.Add(this.btnFeldHinzu);
            this.grpFelder.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFelder.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpFelder.Location = new System.Drawing.Point(240, 0);
            this.grpFelder.Name = "grpFelder";
            this.grpFelder.Size = new System.Drawing.Size(408, 81);
            this.grpFelder.TabIndex = 0;
            this.grpFelder.TabStop = false;
            this.grpFelder.Text = "Felder";
            // 
            // btnVariable
            // 
            this.btnVariable.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVariable.ImageCode = "Variable|16|||||||||PlusZeichen";
            this.btnVariable.Location = new System.Drawing.Point(208, 2);
            this.btnVariable.Name = "btnVariable";
            this.btnVariable.QuickInfo = "Ein Feld, das eine Variable \r\neines Zeilen-Skriptes dargestellen kann.\r\nImmer sch" +
    "reibgeschützt.";
            this.btnVariable.Size = new System.Drawing.Size(64, 66);
            this.btnVariable.TabIndex = 4;
            this.btnVariable.Text = "Variablen anzeigen";
            this.btnVariable.Click += new System.EventHandler(this.btnVariable_Click);
            // 
            // btnFileExplorer
            // 
            this.btnFileExplorer.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnFileExplorer.ImageCode = "Ordner|16|||||||||PlusZeichen";
            this.btnFileExplorer.Location = new System.Drawing.Point(272, 2);
            this.btnFileExplorer.Name = "btnFileExplorer";
            this.btnFileExplorer.QuickInfo = "Fügt ein Feld hinzu, das ein Verzeichniss darstellen kann.\r\nEs können Variablen e" +
    "ines Zeilen-Skriptes benutzt werden.";
            this.btnFileExplorer.Size = new System.Drawing.Size(64, 66);
            this.btnFileExplorer.TabIndex = 3;
            this.btnFileExplorer.Text = "Verzeich-niss";
            this.btnFileExplorer.Click += new System.EventHandler(this.grpFileExplorer_Click);
            // 
            // btnKonstante
            // 
            this.btnKonstante.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnKonstante.ImageCode = "Textfeld|16|||||||||PlusZeichen";
            this.btnKonstante.Location = new System.Drawing.Point(80, 2);
            this.btnKonstante.Name = "btnKonstante";
            this.btnKonstante.QuickInfo = "Fügt einen konstanten Wert hinzu,\r\nder für Filterungen benutzt werden\r\nkann.";
            this.btnKonstante.Size = new System.Drawing.Size(64, 66);
            this.btnKonstante.TabIndex = 2;
            this.btnKonstante.Text = "Konstante für Filter";
            this.btnKonstante.Click += new System.EventHandler(this.btnKonstante_Click);
            // 
            // btnFeldHinzu
            // 
            this.btnFeldHinzu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnFeldHinzu.ImageCode = "Stift|16|||||||||PlusZeichen";
            this.btnFeldHinzu.Location = new System.Drawing.Point(8, 2);
            this.btnFeldHinzu.Name = "btnFeldHinzu";
            this.btnFeldHinzu.QuickInfo = "Fügt ein Feld hinzu, mit der z.B. eine Zelle einer \r\nSpalte bearbeitet werden kan" +
    "n.\r\n\r\nDieses Feld kann entweder von einer Zeile\r\nbefüllt werden, oder frei einge" +
    "geben werden";
            this.btnFeldHinzu.Size = new System.Drawing.Size(72, 66);
            this.btnFeldHinzu.TabIndex = 1;
            this.btnFeldHinzu.Text = "Zellen-Bearbeitung";
            this.btnFeldHinzu.Click += new System.EventHandler(this.btnFeldHinzu_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.groupBox1.Controls.Add(this.btnEingangsZeile);
            this.groupBox1.Controls.Add(this.btnZeileHinzu);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(152, 81);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Zeilen-Berechnung";
            // 
            // btnEingangsZeile
            // 
            this.btnEingangsZeile.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnEingangsZeile.ImageCode = "Pfeil_Unten|16|||00FF00||||||Zeile";
            this.btnEingangsZeile.Location = new System.Drawing.Point(8, 2);
            this.btnEingangsZeile.Name = "btnEingangsZeile";
            this.btnEingangsZeile.QuickInfo = resources.GetString("btnEingangsZeile.QuickInfo");
            this.btnEingangsZeile.Size = new System.Drawing.Size(72, 66);
            this.btnEingangsZeile.TabIndex = 5;
            this.btnEingangsZeile.Text = "Zeilen-Eingang";
            this.btnEingangsZeile.Click += new System.EventHandler(this.btnEingangsZeile_Click);
            // 
            // btnZeileHinzu
            // 
            this.btnZeileHinzu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZeileHinzu.ImageCode = "Trichter|16|||||||||Zeile";
            this.btnZeileHinzu.Location = new System.Drawing.Point(80, 2);
            this.btnZeileHinzu.Name = "btnZeileHinzu";
            this.btnZeileHinzu.QuickInfo = resources.GetString("btnZeileHinzu.QuickInfo");
            this.btnZeileHinzu.Size = new System.Drawing.Size(64, 66);
            this.btnZeileHinzu.TabIndex = 0;
            this.btnZeileHinzu.Text = "Zeile mit Filterung";
            this.btnZeileHinzu.Click += new System.EventHandler(this.btnZeileHinzu_Click);
            // 
            // grpOptik
            // 
            this.grpOptik.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpOptik.Controls.Add(this.btnRegisterKarte);
            this.grpOptik.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpOptik.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpOptik.Location = new System.Drawing.Point(152, 0);
            this.grpOptik.Name = "grpOptik";
            this.grpOptik.Size = new System.Drawing.Size(88, 81);
            this.grpOptik.TabIndex = 2;
            this.grpOptik.TabStop = false;
            this.grpOptik.Text = "Optik";
            // 
            // btnRegisterKarte
            // 
            this.btnRegisterKarte.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnRegisterKarte.ImageCode = "PlusZeichen|16|||00FF00||||||Zeile";
            this.btnRegisterKarte.Location = new System.Drawing.Point(0, 2);
            this.btnRegisterKarte.Name = "btnRegisterKarte";
            this.btnRegisterKarte.QuickInfo = "Fügt eine weiter Formularseite hinzu.\r\nDiese kann dann in einem Register dargeste" +
    "llt werden.\r\n\r\nEin solches Formular muss einen Zeilen-Eingang\r\nbesitzen, um Wert" +
    "e empfangen zu können.";
            this.btnRegisterKarte.Size = new System.Drawing.Size(80, 66);
            this.btnRegisterKarte.TabIndex = 6;
            this.btnRegisterKarte.Text = "Formular hinzufügen";
            this.btnRegisterKarte.Click += new System.EventHandler(this.btnRegisterKarte_Click);
            // 
            // btnTabControlAdd
            // 
            this.btnTabControlAdd.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnTabControlAdd.ImageCode = "Bruchlinie|16|||||||||PlusZeichen";
            this.btnTabControlAdd.Location = new System.Drawing.Point(144, 2);
            this.btnTabControlAdd.Name = "btnTabControlAdd";
            this.btnTabControlAdd.QuickInfo = resources.GetString("btnTabControlAdd.QuickInfo");
            this.btnTabControlAdd.Size = new System.Drawing.Size(64, 66);
            this.btnTabControlAdd.TabIndex = 3;
            this.btnTabControlAdd.Text = "Register-Karte";
            this.btnTabControlAdd.Click += new System.EventHandler(this.btnTabControlAdd_Click);
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
            this.grpDatei.Controls.Add(this.btnLetzteFormulare);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Controls.Add(this.btnSaveAs);
            this.grpDatei.Controls.Add(this.btnNeuDB);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(304, 81);
            this.grpDatei.TabIndex = 5;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
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
            this.btnLetzteFormulare.Size = new System.Drawing.Size(104, 66);
            this.btnLetzteFormulare.TabIndex = 1;
            this.btnLetzteFormulare.Text = "zuletzt geöffnete Dateien";
            this.btnLetzteFormulare.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.btnLetzteDateien_ItemClicked);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnSaveAs.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSaveAs.ImageCode = "Diskette";
            this.btnSaveAs.Location = new System.Drawing.Point(232, 2);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(64, 66);
            this.btnSaveAs.TabIndex = 4;
            this.btnSaveAs.Text = "Speichern unter";
            this.btnSaveAs.Click += new System.EventHandler(this.btnNeuDB_SaveAs_Click);
            // 
            // btnNeuDB
            // 
            this.btnNeuDB.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            // LoadTabDatabase
            // 
            this.LoadTabDatabase.Filter = "*.MDB Datenbanken|*.MDB|*.* Alle Dateien|*";
            this.LoadTabDatabase.Title = "Bitte Datenbank wählen:";
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(966, 0);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Visible = false;
            // 
            // btnBild
            // 
            this.btnBild.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnBild.ImageCode = "Bild|16|||||||||PlusZeichen";
            this.btnBild.Location = new System.Drawing.Point(336, 2);
            this.btnBild.Name = "btnBild";
            this.btnBild.QuickInfo = "Fügt ein Feld hinzu, das ein Bild darstellen kann.\r\nEs können Variablen eines Zei" +
    "len-Skriptes benutzt werden.";
            this.btnBild.Size = new System.Drawing.Size(64, 66);
            this.btnBild.TabIndex = 5;
            this.btnBild.Text = "Bild";
            this.btnBild.Click += new System.EventHandler(this.btnBild_Click);
            // 
            // ConnectedFormulaEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 499);
            this.Name = "ConnectedFormulaEditor";
            this.Text = "ConnectedFormula";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.grpDesign.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.tabSeiten.ResumeLayout(false);
            this.tabEditor.ResumeLayout(false);
            this.grpVorschau.ResumeLayout(false);
            this.grpFelder.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.grpOptik.ResumeLayout(false);
            this.tabFile.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabEditor;
        private Controls.GroupBox grpFelder;
        private Controls.Button btnZeileHinzu;
        private Controls.Button btnFeldHinzu;
        private Controls.Button btnKonstante;
        private Controls.GroupBox grpVorschau;
        private Controls.Button btnPfeileAusblenden;
        private Controls.Button btnVorschauÖffnen;
        private Controls.Button btnTabControlAdd;
        private System.Windows.Forms.TabPage tabFile;
        private Controls.GroupBox grpDatei;
        private Controls.LastFilesCombo btnLetzteFormulare;
        private Controls.Button btnOeffnen;
        private Controls.Button btnSaveAs;
        private Controls.Button btnNeuDB;
        private System.Windows.Forms.OpenFileDialog LoadTab;
        private System.Windows.Forms.SaveFileDialog SaveTab;
        private System.Windows.Forms.OpenFileDialog LoadTabDatabase;
        private Controls.Button btnFileExplorer;
        private Controls.GroupBox groupBox1;
        private Controls.GroupBox grpOptik;
        private Controls.Button btnVariable;
        private Controls.Button btnEingangsZeile;
        private System.Windows.Forms.TabPage tabPage1;
        private Controls.Button btnRegisterKarte;
        private Controls.Button btnBild;
    }
}