// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;

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
            var resources = new ComponentResourceManager(typeof(ConnectedFormulaEditor));
            tabEditorStd = new TabPage();
            grpVorschau = new GroupBox();
            btnPfeileAusblenden = new Button();
            grpFelder = new GroupBox();
            btnSymbolLaden = new Button();
            btnWeitereCF = new Button();
            btnRegionAdd = new Button();
            btnButton = new Button();
            btnBild = new Button();
            btnFileExplorer = new Button();
            btnFeldHinzu = new Button();
            grpOptik = new GroupBox();
            btnRegisterKarte = new Button();
            btnTabControlAdd = new Button();
            groupBox1 = new GroupBox();
            btnTable = new Button();
            btnDropdownmenu = new Button();
            btnFilterConverter = new Button();
            btnBenutzerFilterWahl = new Button();
            grpAllgemein = new GroupBox();
            btnArbeitsbereich = new Button();
            tabFile = new TabPage();
            grpDatei = new GroupBox();
            btnSpeichern = new Button();
            btnLetzteFormulare = new LastFilesCombo();
            btnOeffnen = new Button();
            btnSaveAs = new Button();
            btnNeuDB = new Button();
            lstPages = new ListBox();
            LoadTab = new OpenFileDialog();
            SaveTab = new SaveFileDialog();
            LoadSymbol = new OpenFileDialog();
            grpBETA = new GroupBox();
            btnSpeichernBeta = new Button();
            btnOeffnenBeta = new Button();
            Ribbon.SuspendLayout();
            tabStart.SuspendLayout();
            grpAssistent.SuspendLayout();
            tabHintergrund.SuspendLayout();
            grpDesign.SuspendLayout();
            pnlStatusBar.SuspendLayout();
            tabEditorStd.SuspendLayout();
            grpVorschau.SuspendLayout();
            grpFelder.SuspendLayout();
            grpOptik.SuspendLayout();
            groupBox1.SuspendLayout();
            grpAllgemein.SuspendLayout();
            tabFile.SuspendLayout();
            grpDatei.SuspendLayout();
            grpBETA.SuspendLayout();
            SuspendLayout();
            // 
            // Pad
            // 
            Pad.Location = new Point(170, 110);
            Pad.Size = new Size(242, 427);
            Pad.GotNewItemCollection += Pad_GotNewItemCollection;
            // 
            // Ribbon
            // 
            Ribbon.Controls.Add(tabEditorStd);
            Ribbon.Controls.Add(tabFile);
            Ribbon.Size = new Size(784, 110);
            Ribbon.TabDefault = tabFile;
            Ribbon.TabDefaultOrder = new string[]
    {
    "Datei",
    "Editor-Std.",
    "Start"
    };
            Ribbon.Controls.SetChildIndex(tabFile, 0);
            Ribbon.Controls.SetChildIndex(tabEditorStd, 0);
            Ribbon.Controls.SetChildIndex(tabHintergrund, 0);
            Ribbon.Controls.SetChildIndex(tabExport, 0);
            Ribbon.Controls.SetChildIndex(tabStart, 0);
            // 
            // tabStart
            // 
            tabStart.Size = new Size(776, 81);
            // 
            // grpAssistent
            // 
            grpAssistent.Visible = false;
            // 
            // btnVorschauModus
            // 
            btnVorschauModus.CheckedChanged += btnVorschauModus_CheckedChanged;
            // 
            // grpKomponenteHinzufügen
            // 
            grpKomponenteHinzufügen.Visible = false;
            // 
            // grpDesign
            // 
            grpDesign.Visible = false;
            // 
            // tabRightSide
            // 
            tabRightSide.Location = new Point(412, 110);
            tabRightSide.Size = new Size(372, 451);
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(412, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(0, 537);
            pnlStatusBar.Size = new Size(412, 24);
            // 
            // tabEditorStd
            // 
            tabEditorStd.BackColor = Color.FromArgb(244, 245, 246);
            tabEditorStd.Controls.Add(grpVorschau);
            tabEditorStd.Controls.Add(grpFelder);
            tabEditorStd.Controls.Add(grpOptik);
            tabEditorStd.Controls.Add(groupBox1);
            tabEditorStd.Controls.Add(grpAllgemein);
            tabEditorStd.Location = new Point(4, 25);
            tabEditorStd.Margin = new Padding(0);
            tabEditorStd.Name = "tabEditorStd";
            tabEditorStd.Size = new Size(876, 81);
            tabEditorStd.TabIndex = 4;
            tabEditorStd.Text = "Editor-Std.";
            // 
            // grpVorschau
            // 
            grpVorschau.BackColor = Color.FromArgb(244, 245, 246);
            grpVorschau.Controls.Add(btnPfeileAusblenden);
            grpVorschau.Dock = DockStyle.Left;
            grpVorschau.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpVorschau.Location = new Point(848, 0);
            grpVorschau.Name = "grpVorschau";
            grpVorschau.Size = new Size(88, 81);
            grpVorschau.TabIndex = 1;
            grpVorschau.TabStop = false;
            grpVorschau.Text = "Vorschau";
            // 
            // btnPfeileAusblenden
            // 
            btnPfeileAusblenden.ButtonStyle = ButtonStyle.Checkbox_Big_Borderless;
            btnPfeileAusblenden.ImageCode = "Pfeil_Rechts|16||1||||0";
            btnPfeileAusblenden.Location = new Point(8, 2);
            btnPfeileAusblenden.Name = "btnPfeileAusblenden";
            btnPfeileAusblenden.Size = new Size(72, 66);
            btnPfeileAusblenden.TabIndex = 0;
            btnPfeileAusblenden.Text = "Pfeile etc. ausblenden";
            btnPfeileAusblenden.CheckedChanged += btnPfeileAusblenden_CheckedChanged;
            // 
            // grpFelder
            // 
            grpFelder.BackColor = Color.FromArgb(244, 245, 246);
            grpFelder.Controls.Add(btnSymbolLaden);
            grpFelder.Controls.Add(btnWeitereCF);
            grpFelder.Controls.Add(btnRegionAdd);
            grpFelder.Controls.Add(btnButton);
            grpFelder.Controls.Add(btnBild);
            grpFelder.Controls.Add(btnFileExplorer);
            grpFelder.Controls.Add(btnFeldHinzu);
            grpFelder.Dock = DockStyle.Left;
            grpFelder.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpFelder.Location = new Point(504, 0);
            grpFelder.Name = "grpFelder";
            grpFelder.Size = new Size(344, 81);
            grpFelder.TabIndex = 0;
            grpFelder.TabStop = false;
            grpFelder.Text = "Felder";
            // 
            // btnSymbolLaden
            // 
            btnSymbolLaden.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSymbolLaden.ImageCode = "Ordner|16";
            btnSymbolLaden.Location = new Point(256, 2);
            btnSymbolLaden.Name = "btnSymbolLaden";
            btnSymbolLaden.Size = new Size(72, 66);
            btnSymbolLaden.TabIndex = 14;
            btnSymbolLaden.Text = "Symbol laden";
            btnSymbolLaden.Click += btnSymbolLaden_Click;
            // 
            // btnWeitereCF
            // 
            btnWeitereCF.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnWeitereCF.Location = new Point(144, 46);
            btnWeitereCF.Name = "btnWeitereCF";
            btnWeitereCF.Size = new Size(104, 22);
            btnWeitereCF.TabIndex = 8;
            btnWeitereCF.Text = "weitere...";
            btnWeitereCF.Click += btnWeitereCF_Click;
            // 
            // btnRegionAdd
            // 
            btnRegionAdd.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnRegionAdd.ImageCode = "Layout|16";
            btnRegionAdd.Location = new Point(8, 46);
            btnRegionAdd.Name = "btnRegionAdd";
            btnRegionAdd.Size = new Size(136, 22);
            btnRegionAdd.TabIndex = 7;
            btnRegionAdd.Text = "Region";
            btnRegionAdd.Click += btnRegionAdd_Click;
            // 
            // btnButton
            // 
            btnButton.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnButton.ImageCode = "Stop";
            btnButton.Location = new Point(8, 24);
            btnButton.Name = "btnButton";
            btnButton.Size = new Size(136, 22);
            btnButton.TabIndex = 6;
            btnButton.Text = "Schaltfläche";
            btnButton.Click += btnButton_Click;
            // 
            // btnBild
            // 
            btnBild.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnBild.ImageCode = "Bild";
            btnBild.Location = new Point(144, 24);
            btnBild.Name = "btnBild";
            btnBild.Size = new Size(104, 22);
            btnBild.TabIndex = 5;
            btnBild.Text = "Bild";
            btnBild.Click += btnBild_Click;
            // 
            // btnFileExplorer
            // 
            btnFileExplorer.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnFileExplorer.ImageCode = "Ordner";
            btnFileExplorer.Location = new Point(144, 2);
            btnFileExplorer.Name = "btnFileExplorer";
            btnFileExplorer.Size = new Size(104, 22);
            btnFileExplorer.TabIndex = 3;
            btnFileExplorer.Text = "Verzeichnis";
            btnFileExplorer.Click += grpFileExplorer_Click;
            // 
            // btnFeldHinzu
            // 
            btnFeldHinzu.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnFeldHinzu.ImageCode = "Textfeld2|24";
            btnFeldHinzu.Location = new Point(8, 2);
            btnFeldHinzu.Name = "btnFeldHinzu";
            btnFeldHinzu.Size = new Size(136, 22);
            btnFeldHinzu.TabIndex = 1;
            btnFeldHinzu.Text = "Zelle";
            btnFeldHinzu.Click += btnFeldHinzu_Click;
            // 
            // grpOptik
            // 
            grpOptik.BackColor = Color.FromArgb(244, 245, 246);
            grpOptik.Controls.Add(btnRegisterKarte);
            grpOptik.Controls.Add(btnTabControlAdd);
            grpOptik.Dock = DockStyle.Left;
            grpOptik.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpOptik.Location = new Point(344, 0);
            grpOptik.Name = "grpOptik";
            grpOptik.Size = new Size(160, 81);
            grpOptik.TabIndex = 2;
            grpOptik.TabStop = false;
            grpOptik.Text = "Optik";
            // 
            // btnRegisterKarte
            // 
            btnRegisterKarte.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnRegisterKarte.ImageCode = "Register|16|||||||||PlusZeichen";
            btnRegisterKarte.Location = new Point(72, 2);
            btnRegisterKarte.Name = "btnRegisterKarte";
            btnRegisterKarte.Size = new Size(80, 66);
            btnRegisterKarte.TabIndex = 6;
            btnRegisterKarte.Text = "Neue Registerkarte";
            btnRegisterKarte.Click += btnRegisterKarte_Click;
            // 
            // btnTabControlAdd
            // 
            btnTabControlAdd.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnTabControlAdd.ImageCode = "Registersammlung";
            btnTabControlAdd.Location = new Point(8, 2);
            btnTabControlAdd.Name = "btnTabControlAdd";
            btnTabControlAdd.Size = new Size(64, 66);
            btnTabControlAdd.TabIndex = 3;
            btnTabControlAdd.Text = "Register-sammlung";
            btnTabControlAdd.Click += btnTabControlAdd_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.FromArgb(244, 245, 246);
            groupBox1.Controls.Add(btnTable);
            groupBox1.Controls.Add(btnDropdownmenu);
            groupBox1.Controls.Add(btnFilterConverter);
            groupBox1.Controls.Add(btnBenutzerFilterWahl);
            groupBox1.Dock = DockStyle.Left;
            groupBox1.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            groupBox1.Location = new Point(72, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(272, 81);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Zeilen-Berechnung";
            // 
            // btnTable
            // 
            btnTable.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnTable.ImageCode = "Tabelle|16";
            btnTable.Location = new Point(200, 2);
            btnTable.Name = "btnTable";
            btnTable.Size = new Size(64, 66);
            btnTable.TabIndex = 5;
            btnTable.Text = "Tabelle";
            btnTable.Click += btnTable_Click;
            // 
            // btnDropdownmenu
            // 
            btnDropdownmenu.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnDropdownmenu.ImageCode = "Textfeld2|16|||||||||Pfeil_Unten_Scrollbar";
            btnDropdownmenu.Location = new Point(136, 2);
            btnDropdownmenu.Name = "btnDropdownmenu";
            btnDropdownmenu.Size = new Size(64, 66);
            btnDropdownmenu.TabIndex = 4;
            btnDropdownmenu.Text = "Auswahl-feld";
            btnDropdownmenu.Click += btnDropdownmenu_Click;
            // 
            // btnFilterConverter
            // 
            btnFilterConverter.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnFilterConverter.ImageCode = "Trichter|16|||||||||PlusZeichen";
            btnFilterConverter.Location = new Point(72, 2);
            btnFilterConverter.Name = "btnFilterConverter";
            btnFilterConverter.Size = new Size(64, 66);
            btnFilterConverter.TabIndex = 3;
            btnFilterConverter.Text = "Filter";
            btnFilterConverter.Click += btnFilterConverter_Click;
            // 
            // btnBenutzerFilterWahl
            // 
            btnBenutzerFilterWahl.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnBenutzerFilterWahl.ImageCode = "Trichter|16|||||||||Textfeld2";
            btnBenutzerFilterWahl.Location = new Point(8, 2);
            btnBenutzerFilterWahl.Name = "btnBenutzerFilterWahl";
            btnBenutzerFilterWahl.Size = new Size(64, 66);
            btnBenutzerFilterWahl.TabIndex = 2;
            btnBenutzerFilterWahl.Text = "Benutzer-Filter Wahl";
            btnBenutzerFilterWahl.Click += btnBenutzerFilterWahl_Click;
            // 
            // grpAllgemein
            // 
            grpAllgemein.BackColor = Color.FromArgb(244, 245, 246);
            grpAllgemein.Controls.Add(btnArbeitsbereich);
            grpAllgemein.Dock = DockStyle.Left;
            grpAllgemein.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpAllgemein.Location = new Point(0, 0);
            grpAllgemein.Name = "grpAllgemein";
            grpAllgemein.Size = new Size(72, 81);
            grpAllgemein.TabIndex = 4;
            grpAllgemein.TabStop = false;
            grpAllgemein.Text = "Allgemein";
            // 
            // btnArbeitsbereich
            // 
            btnArbeitsbereich.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnArbeitsbereich.ImageCode = "SeiteEinrichten";
            btnArbeitsbereich.Location = new Point(8, 2);
            btnArbeitsbereich.Name = "btnArbeitsbereich";
            btnArbeitsbereich.Size = new Size(56, 66);
            btnArbeitsbereich.TabIndex = 0;
            btnArbeitsbereich.Text = "Arbeits-bereich";
            btnArbeitsbereich.Click += btnArbeitsbereich_Click;
            // 
            // tabFile
            // 
            tabFile.BackColor = Color.FromArgb(244, 245, 246);
            tabFile.Controls.Add(grpBETA);
            tabFile.Controls.Add(grpDatei);
            tabFile.Location = new Point(4, 25);
            tabFile.Margin = new Padding(0);
            tabFile.Name = "tabFile";
            tabFile.Size = new Size(776, 81);
            tabFile.TabIndex = 5;
            tabFile.Text = "Datei";
            // 
            // grpDatei
            // 
            grpDatei.BackColor = Color.FromArgb(244, 245, 246);
            grpDatei.CausesValidation = false;
            grpDatei.Controls.Add(btnSpeichern);
            grpDatei.Controls.Add(btnLetzteFormulare);
            grpDatei.Controls.Add(btnOeffnen);
            grpDatei.Controls.Add(btnSaveAs);
            grpDatei.Controls.Add(btnNeuDB);
            grpDatei.Dock = DockStyle.Left;
            grpDatei.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpDatei.Location = new Point(0, 0);
            grpDatei.Name = "grpDatei";
            grpDatei.Size = new Size(368, 81);
            grpDatei.TabIndex = 5;
            grpDatei.TabStop = false;
            grpDatei.Text = "Datei";
            // 
            // btnSpeichern
            // 
            btnSpeichern.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSpeichern.ImageCode = "Diskette";
            btnSpeichern.Location = new Point(232, 2);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(64, 66);
            btnSpeichern.TabIndex = 5;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.Click += btnSpeichern_Click;
            // 
            // btnLetzteFormulare
            // 
            btnLetzteFormulare.DrawStyle = ComboboxStyle.RibbonBar;
            btnLetzteFormulare.DropDownStyle = ComboBoxStyle.DropDownList;
            btnLetzteFormulare.Enabled = false;
            btnLetzteFormulare.ImageCode = "Ordner";
            btnLetzteFormulare.Location = new Point(128, 2);
            btnLetzteFormulare.Name = "btnLetzteFormulare";
            btnLetzteFormulare.RemoveAllowed = true;
            btnLetzteFormulare.SettingsLoaded = false;
            btnLetzteFormulare.Size = new Size(104, 66);
            btnLetzteFormulare.TabIndex = 1;
            btnLetzteFormulare.Text = "zuletzt geöffnete Dateien";
            btnLetzteFormulare.ItemClicked += btnLetzteDateien_ItemClicked;
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
            btnSaveAs.Enabled = false;
            btnSaveAs.ImageCode = "Diskette";
            btnSaveAs.Location = new Point(296, 2);
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new Size(64, 66);
            btnSaveAs.TabIndex = 4;
            btnSaveAs.Text = "Speichern unter";
            btnSaveAs.Click += btnNeuDB_SaveAs_Click;
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
            btnNeuDB.Click += btnNeuDB_SaveAs_Click;
            // 
            // lstPages
            // 
            lstPages.Appearance = ListBoxAppearance.Gallery;
            lstPages.AutoSort = false;
            lstPages.Dock = DockStyle.Left;
            lstPages.Location = new Point(0, 110);
            lstPages.Name = "lstPages";
            lstPages.Size = new Size(170, 427);
            lstPages.TabIndex = 3;
            lstPages.AddClicked += lstPages_AddClicked;
            lstPages.ItemClicked += lstPages_ItemClicked;
            // 
            // LoadTab
            // 
            LoadTab.Filter = "*.CFO Formulare|*.CFO|*.* Alle Dateien|*";
            LoadTab.Title = "Bitte Formular wählen:";
            LoadTab.FileOk += LoadTab_FileOk;
            // 
            // SaveTab
            // 
            SaveTab.DefaultExt = "CFO";
            SaveTab.Filter = "*.CFO Formulare|*.CFO|*.* Alle Dateien|*";
            SaveTab.Title = "Bitte neuen Dateinamen des Formulars wählen.";
            // 
            // LoadSymbol
            // 
            LoadSymbol.DefaultExt = "BCS";
            LoadSymbol.Filter = "*.BCS Symbol-Datei|*.BCS|*.* Alle Dateien|*";
            LoadSymbol.Title = "Bitte Datei zum Importieren wählen:";
            LoadSymbol.FileOk += LoadSymbol_FileOk;
            // 
            // grpBETA
            // 
            grpBETA.BackColor = Color.FromArgb(244, 245, 246);
            grpBETA.CausesValidation = false;
            grpBETA.Controls.Add(btnSpeichernBeta);
            grpBETA.Controls.Add(btnOeffnenBeta);
            grpBETA.Dock = DockStyle.Left;
            grpBETA.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            grpBETA.Location = new Point(368, 0);
            grpBETA.Name = "grpBETA";
            grpBETA.Size = new Size(136, 81);
            grpBETA.TabIndex = 6;
            grpBETA.TabStop = false;
            grpBETA.Text = "BETA";
            // 
            // btnSpeichernBeta
            // 
            btnSpeichernBeta.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnSpeichernBeta.ImageCode = "Diskette";
            btnSpeichernBeta.Location = new Point(64, 2);
            btnSpeichernBeta.Name = "btnSpeichernBeta";
            btnSpeichernBeta.Size = new Size(64, 66);
            btnSpeichernBeta.TabIndex = 5;
            btnSpeichernBeta.Text = "Speichern (BETA)";
            btnSpeichernBeta.Click += btnSpeichernBeta_Click;
            // 
            // btnOeffnenBeta
            // 
            btnOeffnenBeta.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            btnOeffnenBeta.ImageCode = "Ordner";
            btnOeffnenBeta.Location = new Point(8, 2);
            btnOeffnenBeta.Name = "btnOeffnenBeta";
            btnOeffnenBeta.Size = new Size(56, 66);
            btnOeffnenBeta.TabIndex = 1;
            btnOeffnenBeta.Text = "Öffnen (BETA)";
            btnOeffnenBeta.Click += btnOeffnenBeta_Click;
            // 
            // ConnectedFormulaEditor
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(784, 561);
            Controls.Add(lstPages);
            Name = "ConnectedFormulaEditor";
            Text = "ConnectedFormula";
            WindowState = FormWindowState.Maximized;
            Controls.SetChildIndex(Ribbon, 0);
            Controls.SetChildIndex(tabRightSide, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(lstPages, 0);
            Controls.SetChildIndex(Pad, 0);
            Ribbon.ResumeLayout(false);
            tabStart.ResumeLayout(false);
            grpAssistent.ResumeLayout(false);
            tabHintergrund.ResumeLayout(false);
            grpDesign.ResumeLayout(false);
            pnlStatusBar.ResumeLayout(false);
            tabEditorStd.ResumeLayout(false);
            grpVorschau.ResumeLayout(false);
            grpFelder.ResumeLayout(false);
            grpOptik.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            grpAllgemein.ResumeLayout(false);
            tabFile.ResumeLayout(false);
            grpDatei.ResumeLayout(false);
            grpBETA.ResumeLayout(false);
            ResumeLayout(false);

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
        protected ListBox lstPages;
        private Button btnOeffnen;
        private Button btnSaveAs;
        private Button btnNeuDB;
        private OpenFileDialog LoadTab;
        private SaveFileDialog SaveTab;
        private Button btnFileExplorer;
        private GroupBox groupBox1;
        private GroupBox grpOptik;
        private Button btnRegisterKarte;
        private Button btnBild;
        private Button btnSpeichern;
        private Button btnButton;
        private Button btnTable;
        private Button btnDropdownmenu;
        private Button btnFilterConverter;
        private Button btnBenutzerFilterWahl;
        private GroupBox grpAllgemein;
        private Button btnArbeitsbereich;
        private Button btnRegionAdd;
        private Button btnWeitereCF;
        private Button btnSymbolLaden;
        private OpenFileDialog LoadSymbol;
        private GroupBox grpBETA;
        private Button btnSpeichernBeta;
        private Button btnOeffnenBeta;
    }
}