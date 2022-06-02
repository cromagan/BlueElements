using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms {
    public partial class PadEditorReadOnly {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.btnZoomOut = new BlueControls.Controls.Button();
            this.btnZoomIn = new BlueControls.Controls.Button();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.Pad = new BlueControls.Controls.CreativePad();
            this.Ribbon = new BlueControls.Controls.RibbonBar();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.grpAssistent = new BlueControls.Controls.GroupBox();
            this.btnVorschauModus = new BlueControls.Controls.Button();
            this.grpZoom = new BlueControls.Controls.GroupBox();
            this.btnZoom11 = new BlueControls.Controls.Button();
            this.btnAuswahl = new BlueControls.Controls.Button();
            this.tabExport = new System.Windows.Forms.TabPage();
            this.grpDrucken = new BlueControls.Controls.GroupBox();
            this.btnVorschau = new BlueControls.Controls.Button();
            this.btnPageSetup = new BlueControls.Controls.Button();
            this.btnAlsBildSpeichern = new BlueControls.Controls.Button();
            this.btnDruckerDialog = new BlueControls.Controls.Button();
            this.tabSeiten = new BlueControls.Controls.TabControl();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpZoom.SuspendLayout();
            this.tabExport.SuspendLayout();
            this.grpDrucken.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new System.Drawing.Point(176, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new System.Drawing.Point(232, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoomFit.ImageCode = "ZoomFit";
            this.btnZoomFit.Location = new System.Drawing.Point(8, 2);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new System.Drawing.Size(48, 66);
            this.btnZoomFit.TabIndex = 0;
            this.btnZoomFit.Text = "ein-passen";
            this.btnZoomFit.Click += new System.EventHandler(this.btnZoomFit_Click);
            // 
            // Pad
            // 
            this.Pad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pad.Location = new System.Drawing.Point(0, 136);
            this.Pad.Name = "Pad";
            this.Pad.ShiftX = -1F;
            this.Pad.ShiftY = -1F;
            this.Pad.Size = new System.Drawing.Size(884, 225);
            this.Pad.TabIndex = 0;
            this.Pad.Zoom = 1F;
            this.Pad.Changed += new System.EventHandler(this.Pad_Changed);
            this.Pad.DrawModeChanged += new System.EventHandler(this.Pad_DrawModChanged);
            this.Pad.GotNewItemCollection += new System.EventHandler(this.Pad_GotNewItemCollection);
            this.Pad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabStart);
            this.Ribbon.Controls.Add(this.tabExport);
            this.Ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.Location = new System.Drawing.Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new System.Drawing.Size(884, 110);
            this.Ribbon.TabDefault = this.tabStart;
            this.Ribbon.TabDefaultOrder = new string[0];
            this.Ribbon.TabIndex = 2;
            // 
            // tabStart
            // 
            this.tabStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabStart.Controls.Add(this.grpAssistent);
            this.tabStart.Controls.Add(this.grpZoom);
            this.tabStart.Location = new System.Drawing.Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new System.Drawing.Size(876, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpAssistent
            // 
            this.grpAssistent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAssistent.CausesValidation = false;
            this.grpAssistent.Controls.Add(this.btnVorschauModus);
            this.grpAssistent.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAssistent.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAssistent.Location = new System.Drawing.Point(296, 0);
            this.grpAssistent.Name = "grpAssistent";
            this.grpAssistent.Size = new System.Drawing.Size(72, 81);
            this.grpAssistent.TabIndex = 0;
            this.grpAssistent.TabStop = false;
            this.grpAssistent.Text = "Assistenten";
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVorschauModus.ImageCode = "Textdatei";
            this.btnVorschauModus.Location = new System.Drawing.Point(8, 2);
            this.btnVorschauModus.Name = "btnVorschauModus";
            this.btnVorschauModus.Size = new System.Drawing.Size(56, 66);
            this.btnVorschauModus.TabIndex = 14;
            this.btnVorschauModus.Text = "Vorschau-Modus";
            this.btnVorschauModus.CheckedChanged += new System.EventHandler(this.btnVorschauModus_CheckedChanged);
            // 
            // grpZoom
            // 
            this.grpZoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpZoom.CausesValidation = false;
            this.grpZoom.Controls.Add(this.btnZoom11);
            this.grpZoom.Controls.Add(this.btnAuswahl);
            this.grpZoom.Controls.Add(this.btnZoomFit);
            this.grpZoom.Controls.Add(this.btnZoomOut);
            this.grpZoom.Controls.Add(this.btnZoomIn);
            this.grpZoom.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpZoom.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpZoom.Location = new System.Drawing.Point(0, 0);
            this.grpZoom.Name = "grpZoom";
            this.grpZoom.Size = new System.Drawing.Size(296, 81);
            this.grpZoom.TabIndex = 1;
            this.grpZoom.TabStop = false;
            this.grpZoom.Text = "Zoom";
            // 
            // btnZoom11
            // 
            this.btnZoom11.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoom11.ImageCode = "Bild||||||149|10";
            this.btnZoom11.Location = new System.Drawing.Point(64, 2);
            this.btnZoom11.Name = "btnZoom11";
            this.btnZoom11.Size = new System.Drawing.Size(48, 66);
            this.btnZoom11.TabIndex = 4;
            this.btnZoom11.Text = "1:1";
            this.btnZoom11.Click += new System.EventHandler(this.btnZoom11_Click);
            // 
            // btnAuswahl
            // 
            this.btnAuswahl.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAuswahl.Checked = true;
            this.btnAuswahl.ImageCode = "Mauspfeil";
            this.btnAuswahl.Location = new System.Drawing.Point(120, 2);
            this.btnAuswahl.Name = "btnAuswahl";
            this.btnAuswahl.Size = new System.Drawing.Size(56, 66);
            this.btnAuswahl.TabIndex = 3;
            this.btnAuswahl.Text = "wählen";
            // 
            // tabExport
            // 
            this.tabExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabExport.Controls.Add(this.grpDrucken);
            this.tabExport.Location = new System.Drawing.Point(4, 25);
            this.tabExport.Name = "tabExport";
            this.tabExport.Size = new System.Drawing.Size(876, 81);
            this.tabExport.TabIndex = 1;
            this.tabExport.Text = "Export";
            // 
            // grpDrucken
            // 
            this.grpDrucken.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDrucken.CausesValidation = false;
            this.grpDrucken.Controls.Add(this.btnVorschau);
            this.grpDrucken.Controls.Add(this.btnPageSetup);
            this.grpDrucken.Controls.Add(this.btnAlsBildSpeichern);
            this.grpDrucken.Controls.Add(this.btnDruckerDialog);
            this.grpDrucken.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDrucken.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDrucken.Location = new System.Drawing.Point(0, 0);
            this.grpDrucken.Name = "grpDrucken";
            this.grpDrucken.Size = new System.Drawing.Size(288, 81);
            this.grpDrucken.TabIndex = 0;
            this.grpDrucken.TabStop = false;
            this.grpDrucken.Text = "Drucken";
            // 
            // btnVorschau
            // 
            this.btnVorschau.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVorschau.ImageCode = "Datei||||||||||Lupe";
            this.btnVorschau.Location = new System.Drawing.Point(224, 2);
            this.btnVorschau.Name = "btnVorschau";
            this.btnVorschau.Size = new System.Drawing.Size(56, 66);
            this.btnVorschau.TabIndex = 13;
            this.btnVorschau.Text = "Vorschau";
            this.btnVorschau.Click += new System.EventHandler(this.btnVorschau_Click);
            // 
            // btnPageSetup
            // 
            this.btnPageSetup.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnPageSetup.ImageCode = "SeiteEinrichten";
            this.btnPageSetup.Location = new System.Drawing.Point(136, 2);
            this.btnPageSetup.Name = "btnPageSetup";
            this.btnPageSetup.Size = new System.Drawing.Size(88, 66);
            this.btnPageSetup.TabIndex = 12;
            this.btnPageSetup.Text = "Drucker-Seite einrichten";
            this.btnPageSetup.Click += new System.EventHandler(this.btnPageSetup_Click);
            // 
            // btnAlsBildSpeichern
            // 
            this.btnAlsBildSpeichern.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnAlsBildSpeichern.ImageCode = "Bild";
            this.btnAlsBildSpeichern.Location = new System.Drawing.Point(72, 2);
            this.btnAlsBildSpeichern.Name = "btnAlsBildSpeichern";
            this.btnAlsBildSpeichern.Size = new System.Drawing.Size(64, 66);
            this.btnAlsBildSpeichern.TabIndex = 11;
            this.btnAlsBildSpeichern.Text = "Als Bild speichern";
            this.btnAlsBildSpeichern.Click += new System.EventHandler(this.btnAlsBildSpeichern_Click);
            // 
            // btnDruckerDialog
            // 
            this.btnDruckerDialog.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnDruckerDialog.ImageCode = "Drucker";
            this.btnDruckerDialog.Location = new System.Drawing.Point(8, 2);
            this.btnDruckerDialog.Name = "btnDruckerDialog";
            this.btnDruckerDialog.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDruckerDialog.Size = new System.Drawing.Size(64, 66);
            this.btnDruckerDialog.TabIndex = 10;
            this.btnDruckerDialog.Text = "Drucken";
            this.btnDruckerDialog.Click += new System.EventHandler(this.btnDruckerDialog_Click);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Database = null;
            this.tabSeiten.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabSeiten.HotTrack = true;
            this.tabSeiten.Location = new System.Drawing.Point(0, 110);
            this.tabSeiten.Name = "tabSeiten";
            this.tabSeiten.RowKey = ((long)(-1));
            this.tabSeiten.SelectedIndex = 0;
            this.tabSeiten.Size = new System.Drawing.Size(884, 26);
            this.tabSeiten.TabDefault = null;
            this.tabSeiten.TabDefaultOrder = null;
            this.tabSeiten.TabIndex = 3;
            this.tabSeiten.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabSeiten_Selected);
            // 
            // PadEditorReadOnly
            // 
            this.ClientSize = new System.Drawing.Size(884, 361);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.tabSeiten);
            this.Controls.Add(this.Ribbon);
            this.Name = "PadEditorReadOnly";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.TopMost = true;
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpZoom.ResumeLayout(false);
            this.tabExport.ResumeLayout(false);
            this.grpDrucken.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnZoomOut;
        private Button btnAuswahl;
        protected CreativePad Pad;
        protected RibbonBar Ribbon;
        protected System.Windows.Forms.TabPage tabStart;
        protected System.Windows.Forms.TabPage tabExport;
        protected GroupBox grpAssistent;
        private Button btnZoomFit;
        private Button btnZoomIn;
        private Button btnDruckerDialog;
        private Button btnAlsBildSpeichern;
        protected Button btnVorschau;
        private Button btnZoom11;
        private GroupBox grpZoom;
        private GroupBox grpDrucken;
        private Button btnPageSetup;
        protected Button btnVorschauModus;
        protected TabControl tabSeiten;
    }
}