using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls.Forms {
    partial class PadEditorWithFileAccess {
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
            this.tabDatei = new System.Windows.Forms.TabPage();
            this.grpDateiSystem = new BlueControls.Controls.GroupBox();
            this.btnImport = new BlueControls.Controls.Button();
            this.btnLastFiles = new BlueControls.Controls.LastFilesCombo();
            this.btnNeu = new BlueControls.Controls.Button();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSpeichern = new BlueControls.Controls.Button();
            this.btnDruckerDialog = new BlueControls.Controls.Button();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.btnWeitereAllItem = new BlueControls.Controls.Button();
            this.btnAddSymbol = new BlueControls.Controls.Button();
            this.btnAddUnterStufe = new BlueControls.Controls.Button();
            this.btnAddText = new BlueControls.Controls.Button();
            this.btnAddImage = new BlueControls.Controls.Button();
            this.btnAddDimension = new BlueControls.Controls.Button();
            this.btnAddLine = new BlueControls.Controls.Button();
            this.LoadSymbol = new System.Windows.Forms.OpenFileDialog();
            this.btnSymbolLaden = new BlueControls.Controls.Button();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.tabDatei.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.Location = new System.Drawing.Point(0, 110);
            this.Pad.Size = new System.Drawing.Size(412, 227);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabDatei);
            this.Ribbon.Size = new System.Drawing.Size(784, 110);
            this.Ribbon.TabDefault = this.tabDatei;
            this.Ribbon.TabDefaultOrder = new string[] {
        "Datei",
        "Start",
        "Hintergrund",
        "Export"};
            this.Ribbon.TabIndex = 3;
            this.Ribbon.Controls.SetChildIndex(this.tabHintergrund, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabDatei, 0);
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(776, 81);
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.Controls.Add(this.btnSymbolLaden);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnWeitereAllItem);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddSymbol);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddUnterStufe);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddText);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddImage);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddDimension);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddLine);
            this.grpKomponenteHinzufügen.Size = new System.Drawing.Size(368, 81);
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new System.Drawing.Point(412, 110);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(412, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Size = new System.Drawing.Size(412, 24);
            // 
            // tabDatei
            // 
            this.tabDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabDatei.Controls.Add(this.grpDateiSystem);
            this.tabDatei.Location = new System.Drawing.Point(4, 25);
            this.tabDatei.Name = "tabDatei";
            this.tabDatei.Padding = new System.Windows.Forms.Padding(3);
            this.tabDatei.Size = new System.Drawing.Size(876, 81);
            this.tabDatei.TabIndex = 3;
            this.tabDatei.Text = "Datei";
            // 
            // grpDateiSystem
            // 
            this.grpDateiSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDateiSystem.CausesValidation = false;
            this.grpDateiSystem.Controls.Add(this.btnImport);
            this.grpDateiSystem.Controls.Add(this.btnLastFiles);
            this.grpDateiSystem.Controls.Add(this.btnNeu);
            this.grpDateiSystem.Controls.Add(this.btnOeffnen);
            this.grpDateiSystem.Controls.Add(this.btnSpeichern);
            this.grpDateiSystem.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDateiSystem.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDateiSystem.Location = new System.Drawing.Point(3, 3);
            this.grpDateiSystem.Name = "grpDateiSystem";
            this.grpDateiSystem.Size = new System.Drawing.Size(376, 75);
            this.grpDateiSystem.TabIndex = 4;
            this.grpDateiSystem.TabStop = false;
            this.grpDateiSystem.Text = "Dateisystem";
            // 
            // btnImport
            // 
            this.btnImport.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnImport.ImageCode = "Textdatei||||||||||Pfeil_Links";
            this.btnImport.Location = new System.Drawing.Point(240, 2);
            this.btnImport.Name = "btnImport";
            this.btnImport.QuickInfo = "Das aktuelle Layout durch eines\r\nvon ihrem Computer ersetzen.";
            this.btnImport.Size = new System.Drawing.Size(64, 66);
            this.btnImport.TabIndex = 12;
            this.btnImport.Text = "Import";
            this.btnImport.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // btnLastFiles
            // 
            this.btnLastFiles.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLastFiles.DrawStyle = BlueControls.Enums.ComboboxStyle.RibbonBar;
            this.btnLastFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLastFiles.Enabled = false;
            this.btnLastFiles.ImageCode = "Ordner";
            this.btnLastFiles.Location = new System.Drawing.Point(136, 2);
            this.btnLastFiles.Name = "btnLastFiles";
            this.btnLastFiles.RegexCheck = null;
            this.btnLastFiles.SettingsLoaded = false;
            this.btnLastFiles.Size = new System.Drawing.Size(104, 66);
            this.btnLastFiles.TabIndex = 11;
            this.btnLastFiles.Text = "zuletzt geöffnete Dateien";
            this.btnLastFiles.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnLastFiles_ItemClicked);
            // 
            // btnNeu
            // 
            this.btnNeu.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnNeu.ImageCode = "Datei";
            this.btnNeu.Location = new System.Drawing.Point(8, 2);
            this.btnNeu.Name = "btnNeu";
            this.btnNeu.QuickInfo = "Löscht alle Objekte";
            this.btnNeu.Size = new System.Drawing.Size(64, 66);
            this.btnNeu.TabIndex = 10;
            this.btnNeu.Text = "Alles leeren";
            this.btnNeu.Click += new System.EventHandler(this.btnNeu_Click);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(72, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.QuickInfo = "Eine Datei von ihrem\r\nComputer öffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(64, 66);
            this.btnOeffnen.TabIndex = 9;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // btnSpeichern
            // 
            this.btnSpeichern.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSpeichern.ImageCode = "Diskette";
            this.btnSpeichern.Location = new System.Drawing.Point(304, 2);
            this.btnSpeichern.Name = "btnSpeichern";
            this.btnSpeichern.Size = new System.Drawing.Size(64, 66);
            this.btnSpeichern.TabIndex = 8;
            this.btnSpeichern.Text = "Speichern unter";
            this.btnSpeichern.Click += new System.EventHandler(this.btnSpeichern_Click);
            // 
            // btnDruckerDialog
            // 
            this.btnDruckerDialog.Location = new System.Drawing.Point(0, 0);
            this.btnDruckerDialog.Name = "btnDruckerDialog";
            this.btnDruckerDialog.Size = new System.Drawing.Size(0, 0);
            this.btnDruckerDialog.TabIndex = 0;
            // 
            // LoadTab
            // 
            this.LoadTab.DefaultExt = "BCR";
            this.LoadTab.Filter = "*.BCR BCR-Datei|*.BCR|*.* Alle Dateien|*";
            this.LoadTab.Title = "Bitte Datei zum Laden wählen:";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // SaveTab
            // 
            this.SaveTab.DefaultExt = "BCR";
            this.SaveTab.Filter = "*.BCR BCR-Datei|*.BCR|*.* Alle Dateien|*";
            this.SaveTab.Title = "Bitte neuen Dateinamen der Datei wählen.";
            this.SaveTab.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveTab_FileOk);
            // 
            // btnWeitereAllItem
            // 
            this.btnWeitereAllItem.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnWeitereAllItem.Location = new System.Drawing.Point(192, 46);
            this.btnWeitereAllItem.Name = "btnWeitereAllItem";
            this.btnWeitereAllItem.Size = new System.Drawing.Size(88, 22);
            this.btnWeitereAllItem.TabIndex = 12;
            this.btnWeitereAllItem.Text = "weitere...";
            this.btnWeitereAllItem.Click += new System.EventHandler(this.btnWeitereAllItem_Click);
            // 
            // btnAddSymbol
            // 
            this.btnAddSymbol.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddSymbol.ImageCode = "Stern|16|||||137|0";
            this.btnAddSymbol.Location = new System.Drawing.Point(8, 46);
            this.btnAddSymbol.Name = "btnAddSymbol";
            this.btnAddSymbol.Size = new System.Drawing.Size(80, 22);
            this.btnAddSymbol.TabIndex = 9;
            this.btnAddSymbol.Text = "Symbol";
            this.btnAddSymbol.Click += new System.EventHandler(this.btnAddSymbol_Click);
            // 
            // btnAddUnterStufe
            // 
            this.btnAddUnterStufe.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddUnterStufe.ImageCode = "Datei|16";
            this.btnAddUnterStufe.Location = new System.Drawing.Point(192, 2);
            this.btnAddUnterStufe.Name = "btnAddUnterStufe";
            this.btnAddUnterStufe.Size = new System.Drawing.Size(88, 22);
            this.btnAddUnterStufe.TabIndex = 8;
            this.btnAddUnterStufe.Text = "Unterstufe";
            this.btnAddUnterStufe.Click += new System.EventHandler(this.btnAddUnterStufe_Click);
            // 
            // btnAddText
            // 
            this.btnAddText.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddText.ImageCode = "Textfeld|16";
            this.btnAddText.Location = new System.Drawing.Point(8, 2);
            this.btnAddText.Name = "btnAddText";
            this.btnAddText.Size = new System.Drawing.Size(80, 22);
            this.btnAddText.TabIndex = 4;
            this.btnAddText.Text = "Text";
            this.btnAddText.Click += new System.EventHandler(this.btnAddText_Click);
            // 
            // btnAddImage
            // 
            this.btnAddImage.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddImage.ImageCode = "Bild|16";
            this.btnAddImage.Location = new System.Drawing.Point(8, 24);
            this.btnAddImage.Name = "btnAddImage";
            this.btnAddImage.Size = new System.Drawing.Size(80, 22);
            this.btnAddImage.TabIndex = 2;
            this.btnAddImage.Text = "Bild";
            this.btnAddImage.Click += new System.EventHandler(this.btnAddImage_Click);
            // 
            // btnAddDimension
            // 
            this.btnAddDimension.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddDimension.ImageCode = "Bemaßung|16";
            this.btnAddDimension.Location = new System.Drawing.Point(96, 2);
            this.btnAddDimension.Name = "btnAddDimension";
            this.btnAddDimension.Size = new System.Drawing.Size(72, 22);
            this.btnAddDimension.TabIndex = 6;
            this.btnAddDimension.Text = "Maß";
            this.btnAddDimension.Click += new System.EventHandler(this.btnAddDimension_Click);
            // 
            // btnAddLine
            // 
            this.btnAddLine.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddLine.ImageCode = "Linie|16";
            this.btnAddLine.Location = new System.Drawing.Point(96, 24);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(72, 22);
            this.btnAddLine.TabIndex = 7;
            this.btnAddLine.Text = "Linie";
            this.btnAddLine.Click += new System.EventHandler(this.btnAddLine_Click);
            // 
            // LoadSymbol
            // 
            this.LoadSymbol.DefaultExt = "BCS";
            this.LoadSymbol.Filter = "*.BCS Symbol-Datei|*.BCS|*.* Alle Dateien|*";
            this.LoadSymbol.Title = "Bitte Datei zum Importieren wählen:";
            this.LoadSymbol.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadSymbol_FileOk);
            // 
            // btnSymbolLaden
            // 
            this.btnSymbolLaden.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSymbolLaden.ImageCode = "Ordner|16";
            this.btnSymbolLaden.Location = new System.Drawing.Point(288, 2);
            this.btnSymbolLaden.Name = "btnSymbolLaden";
            this.btnSymbolLaden.Size = new System.Drawing.Size(72, 66);
            this.btnSymbolLaden.TabIndex = 13;
            this.btnSymbolLaden.Text = "Symbol laden";
            this.btnSymbolLaden.Click += new System.EventHandler(this.btnSymbolLaden_Click);
            // 
            // PadEditorWithFileAccess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(784, 361);
            this.Name = "PadEditorWithFileAccess";
            this.Text = "PadEditorWithFileAccess";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.tabDatei.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected TabPage tabDatei;
        protected GroupBox grpDateiSystem;
        private Button btnDruckerDialog;
        protected LastFilesCombo btnLastFiles;
        private Button btnNeu;
        protected Button btnOeffnen;
        private OpenFileDialog LoadTab;
        private SaveFileDialog SaveTab;
        protected Button btnImport;
        protected Button btnSpeichern;
        private Button btnWeitereAllItem;
        private Button btnAddSymbol;
        private Button btnAddUnterStufe;
        private Button btnAddText;
        private Button btnAddImage;
        private Button btnAddDimension;
        private Button btnAddLine;
        private Button btnSymbolLaden;
        private OpenFileDialog LoadSymbol;
    }
}