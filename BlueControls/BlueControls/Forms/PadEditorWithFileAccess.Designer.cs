
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
            if (disposing && (components != null)) {
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
            this.grpDesign.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.tabDatei.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabDatei);
            this.Ribbon.Location = new System.Drawing.Point(0, 0);
            this.Ribbon.TabDefault = this.tabDatei;
            this.Ribbon.TabDefaultOrder = new string[] {
        "Start",
        "Bearbeiten",
        "Hintergrund",
        "Export"};
            this.Ribbon.TabIndex = 3;
            this.Ribbon.Controls.SetChildIndex(this.tabHintergrund, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabDatei, 0);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Location = new System.Drawing.Point(0, 110);
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
            this.btnImport.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnLastFiles.Regex = null;
            this.btnLastFiles.Size = new System.Drawing.Size(104, 66);
            this.btnLastFiles.TabIndex = 11;
            this.btnLastFiles.Text = "zuletzt geöffnete Dateien";
            this.btnLastFiles.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.btnLastFiles_ItemClicked);
            // 
            // btnNeu
            // 
            this.btnNeu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnOeffnen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            this.btnSpeichern.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
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
            // PadEditorWithFileAccess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 361);
            this.Name = "PadEditorWithFileAccess";
            this.Text = "PadEditorWithFileAccess";
            this.grpDesign.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.tabDatei.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected TabPage tabDatei;
        protected GroupBox grpDateiSystem;
        private Button btnDruckerDialog;
        private Button btnImport;
        protected LastFilesCombo btnLastFiles;
        private Button btnNeu;
        protected Button btnOeffnen;
        private Button btnSpeichern;
        private OpenFileDialog LoadTab;
        private SaveFileDialog SaveTab;
    }
}