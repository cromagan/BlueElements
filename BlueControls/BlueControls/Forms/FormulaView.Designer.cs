using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Forms {
    partial class FormulaView {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.tabFile = new System.Windows.Forms.TabPage();
            this.grpOrdner = new BlueControls.Controls.GroupBox();
            this.btnDatenbankenSpeicherort = new BlueControls.Controls.Button();
            this.btnTemporärenSpeicherortÖffnen = new BlueControls.Controls.Button();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnLastFormulas = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.tabAllgemein = new System.Windows.Forms.TabPage();
            this.grpAnsichtWahl = new BlueControls.Controls.GroupBox();
            this.chkAnsichtTableFormular = new BlueControls.Controls.Button();
            this.chkAnsichtFormular = new BlueControls.Controls.Button();
            this.chkAnsichtNurTabelle = new BlueControls.Controls.Button();
            this.grpAnsicht = new BlueControls.Controls.GroupBox();
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
            this.grpFormularSteuerung = new BlueControls.Controls.GroupBox();
            this.btnNeu = new BlueControls.Controls.Button();
            this.btnTextSuche = new BlueControls.Controls.Button();
            this.btnLoeschen = new BlueControls.Controls.Button();
            this.txbTextSuche = new BlueControls.Controls.TextBox();
            this.btnVorwärts = new BlueControls.Controls.Button();
            this.btnZurück = new BlueControls.Controls.Button();
            this.tabAdmin = new System.Windows.Forms.TabPage();
            this.grpAdminZeilen = new BlueControls.Controls.GroupBox();
            this.btnSuchenUndErsetzen = new BlueControls.Controls.Button();
            this.btnZeileLöschen = new BlueControls.Controls.Button();
            this.grpAdminAllgemein = new BlueControls.Controls.GroupBox();
            this.btnSaveLoad = new BlueControls.Controls.Button();
            this.btnPowerBearbeitung = new BlueControls.Controls.Button();
            this.btnSpaltenUebersicht = new BlueControls.Controls.Button();
            this.grpAdminBearbeiten = new BlueControls.Controls.GroupBox();
            this.btnSkripteBearbeiten = new BlueControls.Controls.Button();
            this.btnFormular = new BlueControls.Controls.Button();
            this.btnSpaltenanordnung = new BlueControls.Controls.Button();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.btnLayouts = new BlueControls.Controls.Button();
            this.tabExport = new System.Windows.Forms.TabPage();
            this.grpExport = new BlueControls.Controls.GroupBox();
            this.btnDrucken = new BlueControls.Controls.ComboBox();
            this.btnHTMLExport = new BlueControls.Controls.Button();
            this.btnCSVClipboard = new BlueControls.Controls.Button();
            this.grpImport = new BlueControls.Controls.GroupBox();
            this.btnClipboardImport = new BlueControls.Controls.Button();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.CFormula = new BlueControls.Controls.ConnectedFormulaView();
            this.grpSkripte = new BlueControls.Controls.GroupBox();
            this.pnlStatusBar.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabFile.SuspendLayout();
            this.grpOrdner.SuspendLayout();
            this.grpDatei.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.grpAnsichtWahl.SuspendLayout();
            this.grpAnsicht.SuspendLayout();
            this.grpHilfen.SuspendLayout();
            this.grpFormularSteuerung.SuspendLayout();
            this.tabAdmin.SuspendLayout();
            this.grpAdminZeilen.SuspendLayout();
            this.grpAdminAllgemein.SuspendLayout();
            this.grpAdminBearbeiten.SuspendLayout();
            this.tabExport.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.grpImport.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1202, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 426);
            this.pnlStatusBar.Size = new System.Drawing.Size(1202, 24);
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
            this.ribMain.Size = new System.Drawing.Size(1202, 110);
            this.ribMain.TabDefault = this.tabFile;
            this.ribMain.TabDefaultOrder = null;
            this.ribMain.TabIndex = 97;
            // 
            // tabFile
            // 
            this.tabFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabFile.Controls.Add(this.grpOrdner);
            this.tabFile.Controls.Add(this.grpDatei);
            this.tabFile.Location = new System.Drawing.Point(4, 25);
            this.tabFile.Margin = new System.Windows.Forms.Padding(0);
            this.tabFile.Name = "tabFile";
            this.tabFile.Size = new System.Drawing.Size(923, 81);
            this.tabFile.TabIndex = 3;
            this.tabFile.Text = "Datei";
            // 
            // grpOrdner
            // 
            this.grpOrdner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpOrdner.CausesValidation = false;
            this.grpOrdner.Controls.Add(this.btnDatenbankenSpeicherort);
            this.grpOrdner.Controls.Add(this.btnTemporärenSpeicherortÖffnen);
            this.grpOrdner.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpOrdner.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpOrdner.Location = new System.Drawing.Point(176, 0);
            this.grpOrdner.Name = "grpOrdner";
            this.grpOrdner.Size = new System.Drawing.Size(184, 81);
            this.grpOrdner.TabIndex = 3;
            this.grpOrdner.TabStop = false;
            this.grpOrdner.Text = "Ordner";
            // 
            // btnDatenbankenSpeicherort
            // 
            this.btnDatenbankenSpeicherort.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnDatenbankenSpeicherort.ImageCode = "Ordner";
            this.btnDatenbankenSpeicherort.Location = new System.Drawing.Point(8, 2);
            this.btnDatenbankenSpeicherort.Name = "btnDatenbankenSpeicherort";
            this.btnDatenbankenSpeicherort.QuickInfo = "Speicherort der Datenbanken öffnen";
            this.btnDatenbankenSpeicherort.Size = new System.Drawing.Size(88, 66);
            this.btnDatenbankenSpeicherort.TabIndex = 27;
            this.btnDatenbankenSpeicherort.Text = "Datenbanken-Pfad";
            // 
            // btnTemporärenSpeicherortÖffnen
            // 
            this.btnTemporärenSpeicherortÖffnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnTemporärenSpeicherortÖffnen.ImageCode = "Ordner||||0000ff||126";
            this.btnTemporärenSpeicherortÖffnen.Location = new System.Drawing.Point(96, 2);
            this.btnTemporärenSpeicherortÖffnen.Name = "btnTemporärenSpeicherortÖffnen";
            this.btnTemporärenSpeicherortÖffnen.QuickInfo = "Temporären Speicherort öffnen";
            this.btnTemporärenSpeicherortÖffnen.Size = new System.Drawing.Size(80, 66);
            this.btnTemporärenSpeicherortÖffnen.TabIndex = 26;
            this.btnTemporärenSpeicherortÖffnen.Text = "Temporärer Speicherort";
            // 
            // grpDatei
            // 
            this.grpDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Controls.Add(this.btnLastFormulas);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(176, 81);
            this.grpDatei.TabIndex = 4;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
            // 
            // btnLastFormulas
            // 
            this.btnLastFormulas.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLastFormulas.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnLastFormulas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLastFormulas.Enabled = false;
            this.btnLastFormulas.ImageCode = "Ordner";
            this.btnLastFormulas.Location = new System.Drawing.Point(64, 2);
            this.btnLastFormulas.Name = "btnLastFormulas";
            this.btnLastFormulas.Size = new System.Drawing.Size(104, 66);
            this.btnLastFormulas.TabIndex = 1;
            this.btnLastFormulas.Text = "zuletzt geöffnete Dateien";
            this.btnLastFormulas.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnLetzteDateien_ItemClicked);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(8, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(56, 66);
            this.btnOeffnen.TabIndex = 1;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAllgemein.Controls.Add(this.grpAnsichtWahl);
            this.tabAllgemein.Controls.Add(this.grpAnsicht);
            this.tabAllgemein.Controls.Add(this.grpHilfen);
            this.tabAllgemein.Controls.Add(this.grpFormularSteuerung);
            this.tabAllgemein.Controls.Add(this.grpSkripte);
            this.tabAllgemein.Location = new System.Drawing.Point(4, 25);
            this.tabAllgemein.Name = "tabAllgemein";
            this.tabAllgemein.Size = new System.Drawing.Size(1194, 81);
            this.tabAllgemein.TabIndex = 1;
            this.tabAllgemein.Text = "Allgemein";
            // 
            // grpAnsichtWahl
            // 
            this.grpAnsichtWahl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAnsichtWahl.CausesValidation = false;
            this.grpAnsichtWahl.Controls.Add(this.chkAnsichtTableFormular);
            this.grpAnsichtWahl.Controls.Add(this.chkAnsichtFormular);
            this.grpAnsichtWahl.Controls.Add(this.chkAnsichtNurTabelle);
            this.grpAnsichtWahl.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsichtWahl.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAnsichtWahl.Location = new System.Drawing.Point(1210, 0);
            this.grpAnsichtWahl.Name = "grpAnsichtWahl";
            this.grpAnsichtWahl.Size = new System.Drawing.Size(260, 81);
            this.grpAnsichtWahl.TabIndex = 5;
            this.grpAnsichtWahl.TabStop = false;
            this.grpAnsichtWahl.Text = "Ansichten-Auswahl";
            // 
            // chkAnsichtTableFormular
            // 
            this.chkAnsichtTableFormular.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAnsichtTableFormular.Location = new System.Drawing.Point(8, 46);
            this.chkAnsichtTableFormular.Name = "chkAnsichtTableFormular";
            this.chkAnsichtTableFormular.Size = new System.Drawing.Size(240, 22);
            this.chkAnsichtTableFormular.TabIndex = 14;
            this.chkAnsichtTableFormular.Text = "Tabelle und Formular nebeneinander";
            // 
            // chkAnsichtFormular
            // 
            this.chkAnsichtFormular.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAnsichtFormular.Location = new System.Drawing.Point(8, 24);
            this.chkAnsichtFormular.Name = "chkAnsichtFormular";
            this.chkAnsichtFormular.Size = new System.Drawing.Size(192, 22);
            this.chkAnsichtFormular.TabIndex = 13;
            this.chkAnsichtFormular.Text = "Überschriften und Formular";
            // 
            // chkAnsichtNurTabelle
            // 
            this.chkAnsichtNurTabelle.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAnsichtNurTabelle.Location = new System.Drawing.Point(8, 2);
            this.chkAnsichtNurTabelle.Name = "chkAnsichtNurTabelle";
            this.chkAnsichtNurTabelle.Size = new System.Drawing.Size(104, 22);
            this.chkAnsichtNurTabelle.TabIndex = 12;
            this.chkAnsichtNurTabelle.Text = "Nur Tabelle";
            // 
            // grpAnsicht
            // 
            this.grpAnsicht.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAnsicht.CausesValidation = false;
            this.grpAnsicht.Controls.Add(this.btnUnterschiede);
            this.grpAnsicht.Controls.Add(this.btnAlleSchließen);
            this.grpAnsicht.Controls.Add(this.btnAlleErweitern);
            this.grpAnsicht.Controls.Add(this.capSpaltenanordnung);
            this.grpAnsicht.Controls.Add(this.capZeilen1);
            this.grpAnsicht.Controls.Add(this.cbxColumnArr);
            this.grpAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsicht.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAnsicht.Location = new System.Drawing.Point(882, 0);
            this.grpAnsicht.Margin = new System.Windows.Forms.Padding(0);
            this.grpAnsicht.Name = "grpAnsicht";
            this.grpAnsicht.Padding = new System.Windows.Forms.Padding(0);
            this.grpAnsicht.Size = new System.Drawing.Size(328, 81);
            this.grpAnsicht.TabIndex = 3;
            this.grpAnsicht.TabStop = false;
            this.grpAnsicht.Text = "Ansicht";
            // 
            // btnUnterschiede
            // 
            this.btnUnterschiede.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnUnterschiede.Location = new System.Drawing.Point(216, 46);
            this.btnUnterschiede.Name = "btnUnterschiede";
            this.btnUnterschiede.QuickInfo = "Zeigt Unterschiede zur gewählten Zeile an";
            this.btnUnterschiede.Size = new System.Drawing.Size(104, 22);
            this.btnUnterschiede.TabIndex = 14;
            this.btnUnterschiede.Text = "Unterschiede";
            // 
            // btnAlleSchließen
            // 
            this.btnAlleSchließen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAlleSchließen.ImageCode = "Pfeil_Oben_Scrollbar|14|||||0";
            this.btnAlleSchließen.Location = new System.Drawing.Point(216, 24);
            this.btnAlleSchließen.Name = "btnAlleSchließen";
            this.btnAlleSchließen.QuickInfo = "Neuen Eintrag ergänzen";
            this.btnAlleSchließen.Size = new System.Drawing.Size(104, 22);
            this.btnAlleSchließen.TabIndex = 4;
            this.btnAlleSchließen.Text = "alle schließen";
            // 
            // btnAlleErweitern
            // 
            this.btnAlleErweitern.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAlleErweitern.ImageCode = "Pfeil_Unten_Scrollbar|14|||ff0000||200|200";
            this.btnAlleErweitern.Location = new System.Drawing.Point(216, 2);
            this.btnAlleErweitern.Name = "btnAlleErweitern";
            this.btnAlleErweitern.QuickInfo = "Neuen Eintrag ergänzen";
            this.btnAlleErweitern.Size = new System.Drawing.Size(104, 22);
            this.btnAlleErweitern.TabIndex = 3;
            this.btnAlleErweitern.Text = "alle erweitern";
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
            this.capZeilen1.Text = "<ImageCode=Information|16>";
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
            this.grpHilfen.Location = new System.Drawing.Point(626, 0);
            this.grpHilfen.Name = "grpHilfen";
            this.grpHilfen.Size = new System.Drawing.Size(256, 81);
            this.grpHilfen.TabIndex = 6;
            this.grpHilfen.TabStop = false;
            this.grpHilfen.Text = "Hilfen";
            // 
            // ckbZeilenclickInsClipboard
            // 
            this.ckbZeilenclickInsClipboard.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.ckbZeilenclickInsClipboard.Location = new System.Drawing.Point(96, 24);
            this.ckbZeilenclickInsClipboard.Name = "ckbZeilenclickInsClipboard";
            this.ckbZeilenclickInsClipboard.Size = new System.Drawing.Size(152, 22);
            this.ckbZeilenclickInsClipboard.TabIndex = 13;
            this.ckbZeilenclickInsClipboard.Text = "Zeilenclick = Clipboard";
            // 
            // btnNummerierung
            // 
            this.btnNummerierung.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.btnNummerierung.Location = new System.Drawing.Point(96, 2);
            this.btnNummerierung.Margin = new System.Windows.Forms.Padding(4);
            this.btnNummerierung.Name = "btnNummerierung";
            this.btnNummerierung.QuickInfo = "Ist diese Option aktiviert, werden<br>temporäre Nummern von Spalten eingeblendet";
            this.btnNummerierung.Size = new System.Drawing.Size(112, 22);
            this.btnNummerierung.TabIndex = 12;
            this.btnNummerierung.Text = "Nummerierung";
            // 
            // btnSuchFenster
            // 
            this.btnSuchFenster.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSuchFenster.ImageCode = "Lupe";
            this.btnSuchFenster.Location = new System.Drawing.Point(8, 2);
            this.btnSuchFenster.Name = "btnSuchFenster";
            this.btnSuchFenster.Size = new System.Drawing.Size(80, 66);
            this.btnSuchFenster.TabIndex = 11;
            this.btnSuchFenster.Text = "Suchfenster öffnen";
            // 
            // grpFormularSteuerung
            // 
            this.grpFormularSteuerung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpFormularSteuerung.CausesValidation = false;
            this.grpFormularSteuerung.Controls.Add(this.btnNeu);
            this.grpFormularSteuerung.Controls.Add(this.btnTextSuche);
            this.grpFormularSteuerung.Controls.Add(this.btnLoeschen);
            this.grpFormularSteuerung.Controls.Add(this.txbTextSuche);
            this.grpFormularSteuerung.Controls.Add(this.btnVorwärts);
            this.grpFormularSteuerung.Controls.Add(this.btnZurück);
            this.grpFormularSteuerung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFormularSteuerung.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpFormularSteuerung.Location = new System.Drawing.Point(256, 0);
            this.grpFormularSteuerung.Name = "grpFormularSteuerung";
            this.grpFormularSteuerung.Size = new System.Drawing.Size(370, 81);
            this.grpFormularSteuerung.TabIndex = 4;
            this.grpFormularSteuerung.TabStop = false;
            this.grpFormularSteuerung.Text = "Formular-Ansicht-Steuerung";
            // 
            // btnNeu
            // 
            this.btnNeu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnNeu.ImageCode = "PlusZeichen";
            this.btnNeu.Location = new System.Drawing.Point(8, 2);
            this.btnNeu.Name = "btnNeu";
            this.btnNeu.Size = new System.Drawing.Size(56, 66);
            this.btnNeu.TabIndex = 2;
            this.btnNeu.Text = "Neu";
            // 
            // btnTextSuche
            // 
            this.btnTextSuche.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnTextSuche.Enabled = false;
            this.btnTextSuche.ImageCode = "Lupe|16";
            this.btnTextSuche.Location = new System.Drawing.Point(240, 24);
            this.btnTextSuche.Name = "btnTextSuche";
            this.btnTextSuche.QuickInfo = "Nächsten Eintrag anzeigen,<br>der obigen Text enthält";
            this.btnTextSuche.Size = new System.Drawing.Size(120, 22);
            this.btnTextSuche.TabIndex = 7;
            this.btnTextSuche.Text = "Textsuche";
            // 
            // btnLoeschen
            // 
            this.btnLoeschen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnLoeschen.ImageCode = "Papierkorb";
            this.btnLoeschen.Location = new System.Drawing.Point(64, 2);
            this.btnLoeschen.Name = "btnLoeschen";
            this.btnLoeschen.Size = new System.Drawing.Size(56, 66);
            this.btnLoeschen.TabIndex = 3;
            this.btnLoeschen.Text = "löschen";
            // 
            // txbTextSuche
            // 
            this.txbTextSuche.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTextSuche.Location = new System.Drawing.Point(240, 2);
            this.txbTextSuche.Name = "txbTextSuche";
            this.txbTextSuche.Size = new System.Drawing.Size(120, 22);
            this.txbTextSuche.TabIndex = 6;
            // 
            // btnVorwärts
            // 
            this.btnVorwärts.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVorwärts.ImageCode = "Pfeil_Rechts";
            this.btnVorwärts.Location = new System.Drawing.Point(184, 2);
            this.btnVorwärts.Name = "btnVorwärts";
            this.btnVorwärts.QuickInfo = "Nächsten Eintrag anzeigen";
            this.btnVorwärts.Size = new System.Drawing.Size(48, 66);
            this.btnVorwärts.TabIndex = 5;
            this.btnVorwärts.Text = "vor";
            // 
            // btnZurück
            // 
            this.btnZurück.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZurück.ImageCode = "Pfeil_Links";
            this.btnZurück.Location = new System.Drawing.Point(136, 2);
            this.btnZurück.Name = "btnZurück";
            this.btnZurück.QuickInfo = "Vorherigen Eintrag anzeigen";
            this.btnZurück.Size = new System.Drawing.Size(48, 66);
            this.btnZurück.TabIndex = 4;
            this.btnZurück.Text = "zurück";
            // 
            // tabAdmin
            // 
            this.tabAdmin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAdmin.Controls.Add(this.grpAdminZeilen);
            this.tabAdmin.Controls.Add(this.grpAdminAllgemein);
            this.tabAdmin.Controls.Add(this.grpAdminBearbeiten);
            this.tabAdmin.Location = new System.Drawing.Point(4, 25);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(923, 81);
            this.tabAdmin.TabIndex = 0;
            this.tabAdmin.Text = "Administration";
            // 
            // grpAdminZeilen
            // 
            this.grpAdminZeilen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminZeilen.CausesValidation = false;
            this.grpAdminZeilen.Controls.Add(this.btnSuchenUndErsetzen);
            this.grpAdminZeilen.Controls.Add(this.btnZeileLöschen);
            this.grpAdminZeilen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminZeilen.Enabled = false;
            this.grpAdminZeilen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminZeilen.Location = new System.Drawing.Point(520, 0);
            this.grpAdminZeilen.Name = "grpAdminZeilen";
            this.grpAdminZeilen.Size = new System.Drawing.Size(232, 81);
            this.grpAdminZeilen.TabIndex = 8;
            this.grpAdminZeilen.TabStop = false;
            this.grpAdminZeilen.Text = "Zeilen";
            // 
            // btnSuchenUndErsetzen
            // 
            this.btnSuchenUndErsetzen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSuchenUndErsetzen.ImageCode = "Fernglas";
            this.btnSuchenUndErsetzen.Location = new System.Drawing.Point(8, 2);
            this.btnSuchenUndErsetzen.Name = "btnSuchenUndErsetzen";
            this.btnSuchenUndErsetzen.Size = new System.Drawing.Size(80, 66);
            this.btnSuchenUndErsetzen.TabIndex = 44;
            this.btnSuchenUndErsetzen.Text = "Suchen und ersetzen";
            // 
            // btnZeileLöschen
            // 
            this.btnZeileLöschen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZeileLöschen.ImageCode = "Zeile||||||||||Kreuz";
            this.btnZeileLöschen.Location = new System.Drawing.Point(152, 2);
            this.btnZeileLöschen.Name = "btnZeileLöschen";
            this.btnZeileLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            this.btnZeileLöschen.Size = new System.Drawing.Size(72, 66);
            this.btnZeileLöschen.TabIndex = 42;
            this.btnZeileLöschen.Text = "Zeilen löschen";
            // 
            // grpAdminAllgemein
            // 
            this.grpAdminAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminAllgemein.CausesValidation = false;
            this.grpAdminAllgemein.Controls.Add(this.btnSaveLoad);
            this.grpAdminAllgemein.Controls.Add(this.btnPowerBearbeitung);
            this.grpAdminAllgemein.Controls.Add(this.btnSpaltenUebersicht);
            this.grpAdminAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminAllgemein.Enabled = false;
            this.grpAdminAllgemein.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminAllgemein.Location = new System.Drawing.Point(312, 0);
            this.grpAdminAllgemein.Name = "grpAdminAllgemein";
            this.grpAdminAllgemein.Size = new System.Drawing.Size(208, 81);
            this.grpAdminAllgemein.TabIndex = 7;
            this.grpAdminAllgemein.TabStop = false;
            this.grpAdminAllgemein.Text = "Allgemein";
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSaveLoad.ImageCode = "Refresh|16";
            this.btnSaveLoad.Location = new System.Drawing.Point(80, 2);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.QuickInfo = "Aktualisiert die Datenbank-Daten. (Speichern, neu Laden)";
            this.btnSaveLoad.Size = new System.Drawing.Size(48, 66);
            this.btnSaveLoad.TabIndex = 43;
            this.btnSaveLoad.Text = "Daten aktual.";
            // 
            // btnPowerBearbeitung
            // 
            this.btnPowerBearbeitung.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnPowerBearbeitung.ImageCode = "Stift||||FF0000||||||Uhr";
            this.btnPowerBearbeitung.Location = new System.Drawing.Point(128, 2);
            this.btnPowerBearbeitung.Name = "btnPowerBearbeitung";
            this.btnPowerBearbeitung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.btnPowerBearbeitung.Size = new System.Drawing.Size(72, 66);
            this.btnPowerBearbeitung.TabIndex = 43;
            this.btnPowerBearbeitung.Text = "Power-Bearbeitung";
            // 
            // btnSpaltenUebersicht
            // 
            this.btnSpaltenUebersicht.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSpaltenUebersicht.ImageCode = "Spalte||||||||||Information";
            this.btnSpaltenUebersicht.Location = new System.Drawing.Point(8, 2);
            this.btnSpaltenUebersicht.Name = "btnSpaltenUebersicht";
            this.btnSpaltenUebersicht.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenUebersicht.TabIndex = 36;
            this.btnSpaltenUebersicht.Text = "Spalten-Übersicht";
            // 
            // grpAdminBearbeiten
            // 
            this.grpAdminBearbeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAdminBearbeiten.CausesValidation = false;
            this.grpAdminBearbeiten.Controls.Add(this.btnSkripteBearbeiten);
            this.grpAdminBearbeiten.Controls.Add(this.btnFormular);
            this.grpAdminBearbeiten.Controls.Add(this.btnSpaltenanordnung);
            this.grpAdminBearbeiten.Controls.Add(this.btnDatenbankKopf);
            this.grpAdminBearbeiten.Controls.Add(this.btnLayouts);
            this.grpAdminBearbeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAdminBearbeiten.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAdminBearbeiten.Location = new System.Drawing.Point(0, 0);
            this.grpAdminBearbeiten.Name = "grpAdminBearbeiten";
            this.grpAdminBearbeiten.Size = new System.Drawing.Size(312, 81);
            this.grpAdminBearbeiten.TabIndex = 9;
            this.grpAdminBearbeiten.TabStop = false;
            this.grpAdminBearbeiten.Text = "Bearbeiten";
            // 
            // btnSkripteBearbeiten
            // 
            this.btnSkripteBearbeiten.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSkripteBearbeiten.ImageCode = "Skript||||||||||Stift";
            this.btnSkripteBearbeiten.Location = new System.Drawing.Point(240, 2);
            this.btnSkripteBearbeiten.Name = "btnSkripteBearbeiten";
            this.btnSkripteBearbeiten.Size = new System.Drawing.Size(56, 66);
            this.btnSkripteBearbeiten.TabIndex = 45;
            this.btnSkripteBearbeiten.Text = "Skripte";
            this.btnSkripteBearbeiten.Click += new System.EventHandler(this.btnSkripteBearbeiten_Click);
            // 
            // btnFormular
            // 
            this.btnFormular.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnFormular.ImageCode = "Textfeld|16|||||||||Stift";
            this.btnFormular.Location = new System.Drawing.Point(64, 2);
            this.btnFormular.Name = "btnFormular";
            this.btnFormular.Size = new System.Drawing.Size(56, 66);
            this.btnFormular.TabIndex = 44;
            this.btnFormular.Text = "Formular-Editor";
            this.btnFormular.Click += new System.EventHandler(this.btnFormular_Click);
            // 
            // btnSpaltenanordnung
            // 
            this.btnSpaltenanordnung.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnSpaltenanordnung.Enabled = false;
            this.btnSpaltenanordnung.ImageCode = "Spalte||||||||||Stift";
            this.btnSpaltenanordnung.Location = new System.Drawing.Point(176, 2);
            this.btnSpaltenanordnung.Name = "btnSpaltenanordnung";
            this.btnSpaltenanordnung.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenanordnung.TabIndex = 43;
            this.btnSpaltenanordnung.Text = "Spalten-anordung";
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnDatenbankKopf.Enabled = false;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new System.Drawing.Point(0, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new System.Drawing.Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 37;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            // 
            // btnLayouts
            // 
            this.btnLayouts.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnLayouts.Enabled = false;
            this.btnLayouts.ImageCode = "Layout||||||||||Stift";
            this.btnLayouts.Location = new System.Drawing.Point(120, 2);
            this.btnLayouts.Name = "btnLayouts";
            this.btnLayouts.Size = new System.Drawing.Size(56, 66);
            this.btnLayouts.TabIndex = 41;
            this.btnLayouts.Text = "Layout-Editor";
            // 
            // tabExport
            // 
            this.tabExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabExport.Controls.Add(this.grpExport);
            this.tabExport.Controls.Add(this.grpImport);
            this.tabExport.Location = new System.Drawing.Point(4, 25);
            this.tabExport.Name = "tabExport";
            this.tabExport.Size = new System.Drawing.Size(923, 81);
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
            this.grpExport.Location = new System.Drawing.Point(88, 0);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(224, 81);
            this.grpExport.TabIndex = 3;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Export";
            // 
            // btnDrucken
            // 
            this.btnDrucken.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnDrucken.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnDrucken.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnDrucken.ImageCode = "Drucker";
            this.btnDrucken.Location = new System.Drawing.Point(136, 2);
            this.btnDrucken.Name = "btnDrucken";
            this.btnDrucken.Size = new System.Drawing.Size(80, 66);
            this.btnDrucken.TabIndex = 13;
            this.btnDrucken.Text = "Drucken bzw. Export";
            // 
            // btnHTMLExport
            // 
            this.btnHTMLExport.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnHTMLExport.ImageCode = "Globus";
            this.btnHTMLExport.Location = new System.Drawing.Point(72, 2);
            this.btnHTMLExport.Name = "btnHTMLExport";
            this.btnHTMLExport.QuickInfo = "HTML-Format (für einen Internet-Browser) als Datei";
            this.btnHTMLExport.Size = new System.Drawing.Size(64, 66);
            this.btnHTMLExport.TabIndex = 3;
            this.btnHTMLExport.Text = "HTML";
            // 
            // btnCSVClipboard
            // 
            this.btnCSVClipboard.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnCSVClipboard.ImageCode = "Clipboard||||||||||Pfeil_Rechts";
            this.btnCSVClipboard.Location = new System.Drawing.Point(8, 2);
            this.btnCSVClipboard.Name = "btnCSVClipboard";
            this.btnCSVClipboard.QuickInfo = "CSV-Format (z.B.: für Excel) in die Zwischenablage";
            this.btnCSVClipboard.Size = new System.Drawing.Size(64, 66);
            this.btnCSVClipboard.TabIndex = 0;
            this.btnCSVClipboard.Text = "CSV";
            // 
            // grpImport
            // 
            this.grpImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpImport.CausesValidation = false;
            this.grpImport.Controls.Add(this.btnClipboardImport);
            this.grpImport.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpImport.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpImport.Location = new System.Drawing.Point(0, 0);
            this.grpImport.Name = "grpImport";
            this.grpImport.Size = new System.Drawing.Size(88, 81);
            this.grpImport.TabIndex = 4;
            this.grpImport.TabStop = false;
            this.grpImport.Text = "Import";
            // 
            // btnClipboardImport
            // 
            this.btnClipboardImport.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnClipboardImport.ImageCode = "Tabelle||||||||||Pfeil_Links";
            this.btnClipboardImport.Location = new System.Drawing.Point(8, 2);
            this.btnClipboardImport.Name = "btnClipboardImport";
            this.btnClipboardImport.Size = new System.Drawing.Size(64, 66);
            this.btnClipboardImport.TabIndex = 39;
            this.btnClipboardImport.Text = "Clipboard-Import";
            // 
            // LoadTab
            // 
            this.LoadTab.Filter = "*.CFO Formulare|*.CFO|*.* Alle Dateien|*";
            this.LoadTab.Title = "Bitte Datenbank laden!";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // CFormula
            // 
            this.CFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CFormula.Location = new System.Drawing.Point(0, 110);
            this.CFormula.Name = "CFormula";
            this.CFormula.Size = new System.Drawing.Size(1202, 316);
            this.CFormula.TabIndex = 98;
            this.CFormula.Text = "CFO";
            // 
            // grpSkripte
            // 
            this.grpSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpSkripte.CausesValidation = false;
            this.grpSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSkripte.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpSkripte.Location = new System.Drawing.Point(0, 0);
            this.grpSkripte.Name = "grpSkripte";
            this.grpSkripte.Size = new System.Drawing.Size(256, 81);
            this.grpSkripte.TabIndex = 7;
            this.grpSkripte.TabStop = false;
            this.grpSkripte.Text = "Skripte";
            // 
            // FormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1202, 450);
            this.Controls.Add(this.CFormula);
            this.Controls.Add(this.ribMain);
            this.Name = "FormulaView";
            this.Text = "FormulaView";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.CFormula, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabFile.ResumeLayout(false);
            this.grpOrdner.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.grpAnsichtWahl.ResumeLayout(false);
            this.grpAnsicht.ResumeLayout(false);
            this.grpHilfen.ResumeLayout(false);
            this.grpFormularSteuerung.ResumeLayout(false);
            this.tabAdmin.ResumeLayout(false);
            this.grpAdminZeilen.ResumeLayout(false);
            this.grpAdminAllgemein.ResumeLayout(false);
            this.grpAdminBearbeiten.ResumeLayout(false);
            this.tabExport.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
            this.grpImport.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected RibbonBar ribMain;
        private TabPage tabFile;
        private GroupBox grpOrdner;
        private Button btnDatenbankenSpeicherort;
        private Button btnTemporärenSpeicherortÖffnen;
        private GroupBox grpDatei;
        private LastFilesCombo btnLastFormulas;
        private Button btnOeffnen;
        protected TabPage tabAllgemein;
        protected GroupBox grpAnsichtWahl;
        private Button chkAnsichtTableFormular;
        private Button chkAnsichtFormular;
        private Button chkAnsichtNurTabelle;
        protected GroupBox grpAnsicht;
        protected Button btnUnterschiede;
        protected Button btnAlleSchließen;
        protected Button btnAlleErweitern;
        private Caption capSpaltenanordnung;
        private Caption capZeilen1;
        private ComboBox cbxColumnArr;
        private GroupBox grpHilfen;
        private Button ckbZeilenclickInsClipboard;
        private Button btnNummerierung;
        private Button btnSuchFenster;
        private GroupBox grpFormularSteuerung;
        private Button btnNeu;
        private Button btnTextSuche;
        private Button btnLoeschen;
        private TextBox txbTextSuche;
        private Button btnVorwärts;
        private Button btnZurück;
        protected TabPage tabAdmin;
        private GroupBox grpAdminZeilen;
        private Button btnSuchenUndErsetzen;
        private Button btnZeileLöschen;
        private GroupBox grpAdminAllgemein;
        private Button btnSaveLoad;
        private Button btnPowerBearbeitung;
        private Button btnSpaltenUebersicht;
        private GroupBox grpAdminBearbeiten;
        private Button btnSkripteBearbeiten;
        private Button btnFormular;
        private Button btnSpaltenanordnung;
        private Button btnDatenbankKopf;
        private Button btnLayouts;
        protected TabPage tabExport;
        private GroupBox grpExport;
        private ComboBox btnDrucken;
        private Button btnHTMLExport;
        private Button btnCSVClipboard;
        private GroupBox grpImport;
        private Button btnClipboardImport;
        private OpenFileDialog LoadTab;
        private ConnectedFormulaView CFormula;
        private GroupBox grpSkripte;
    }
}