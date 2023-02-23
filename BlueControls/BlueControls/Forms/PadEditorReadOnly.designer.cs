using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TabControl = BlueControls.Controls.TabControl;

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
            this.btnZoomOut = new Button();
            this.btnZoomIn = new Button();
            this.btnZoomFit = new Button();
            this.Pad = new CreativePad();
            this.Ribbon = new RibbonBar();
            this.tabStart = new TabPage();
            this.grpAssistent = new GroupBox();
            this.btnVorschauModus = new Button();
            this.grpZoom = new GroupBox();
            this.btnZoom11 = new Button();
            this.btnAuswahl = new Button();
            this.tabExport = new TabPage();
            this.grpDrucken = new GroupBox();
            this.btnVorschau = new Button();
            this.btnPageSetup = new Button();
            this.btnAlsBildSpeichern = new Button();
            this.btnDruckerDialog = new Button();
            this.tabSeiten = new TabControl();
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
            this.btnZoomOut.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Optionbox | ButtonStyle.Button_Big) 
                                                          | ButtonStyle.Borderless)));
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new Point(176, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Optionbox | ButtonStyle.Button_Big) 
                                                         | ButtonStyle.Borderless)));
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new Point(232, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnZoomFit.ImageCode = "ZoomFit";
            this.btnZoomFit.Location = new Point(8, 2);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new Size(48, 66);
            this.btnZoomFit.TabIndex = 0;
            this.btnZoomFit.Text = "ein-passen";
            this.btnZoomFit.Click += new EventHandler(this.btnZoomFit_Click);
            // 
            // Pad
            // 
            this.Pad.Dock = DockStyle.Fill;
            this.Pad.Location = new Point(0, 136);
            this.Pad.Name = "Pad";
            this.Pad.ShiftX = -1F;
            this.Pad.ShiftY = -1F;
            this.Pad.Size = new Size(884, 200);
            this.Pad.TabIndex = 0;
            this.Pad.Zoom = 1F;
            this.Pad.Changed += new EventHandler(this.Pad_Changed);
            this.Pad.DrawModeChanged += new EventHandler(this.Pad_DrawModChanged);
            this.Pad.GotNewItemCollection += new EventHandler(this.Pad_GotNewItemCollection);
            this.Pad.MouseUp += new MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabStart);
            this.Ribbon.Controls.Add(this.tabExport);
            this.Ribbon.Dock = DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.Location = new Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new Size(884, 110);
            this.Ribbon.TabDefault = this.tabStart;
            this.Ribbon.TabDefaultOrder = new string[0];
            this.Ribbon.TabIndex = 2;
            // 
            // tabStart
            // 
            this.tabStart.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabStart.Controls.Add(this.grpAssistent);
            this.tabStart.Controls.Add(this.grpZoom);
            this.tabStart.Location = new Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new Size(876, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpAssistent
            // 
            this.grpAssistent.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAssistent.CausesValidation = false;
            this.grpAssistent.Controls.Add(this.btnVorschauModus);
            this.grpAssistent.Dock = DockStyle.Left;
            this.grpAssistent.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpAssistent.Location = new Point(296, 0);
            this.grpAssistent.Name = "grpAssistent";
            this.grpAssistent.Size = new Size(72, 81);
            this.grpAssistent.TabIndex = 0;
            this.grpAssistent.TabStop = false;
            this.grpAssistent.Text = "Assistenten";
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Checkbox | ButtonStyle.Button_Big) 
                                                                | ButtonStyle.Borderless)));
            this.btnVorschauModus.ImageCode = "Textdatei";
            this.btnVorschauModus.Location = new Point(8, 2);
            this.btnVorschauModus.Name = "btnVorschauModus";
            this.btnVorschauModus.Size = new Size(56, 66);
            this.btnVorschauModus.TabIndex = 14;
            this.btnVorschauModus.Text = "Vorschau-Modus";
            this.btnVorschauModus.CheckedChanged += new EventHandler(this.btnVorschauModus_CheckedChanged);
            // 
            // grpZoom
            // 
            this.grpZoom.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpZoom.CausesValidation = false;
            this.grpZoom.Controls.Add(this.btnZoom11);
            this.grpZoom.Controls.Add(this.btnAuswahl);
            this.grpZoom.Controls.Add(this.btnZoomFit);
            this.grpZoom.Controls.Add(this.btnZoomOut);
            this.grpZoom.Controls.Add(this.btnZoomIn);
            this.grpZoom.Dock = DockStyle.Left;
            this.grpZoom.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpZoom.Location = new Point(0, 0);
            this.grpZoom.Name = "grpZoom";
            this.grpZoom.Size = new Size(296, 81);
            this.grpZoom.TabIndex = 1;
            this.grpZoom.TabStop = false;
            this.grpZoom.Text = "Zoom";
            // 
            // btnZoom11
            // 
            this.btnZoom11.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnZoom11.ImageCode = "Bild||||||149|10";
            this.btnZoom11.Location = new Point(64, 2);
            this.btnZoom11.Name = "btnZoom11";
            this.btnZoom11.Size = new Size(48, 66);
            this.btnZoom11.TabIndex = 4;
            this.btnZoom11.Text = "1:1";
            this.btnZoom11.Click += new EventHandler(this.btnZoom11_Click);
            // 
            // btnAuswahl
            // 
            this.btnAuswahl.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Optionbox | ButtonStyle.Button_Big) 
                                                          | ButtonStyle.Borderless)));
            this.btnAuswahl.Checked = true;
            this.btnAuswahl.ImageCode = "Mauspfeil";
            this.btnAuswahl.Location = new Point(120, 2);
            this.btnAuswahl.Name = "btnAuswahl";
            this.btnAuswahl.Size = new Size(56, 66);
            this.btnAuswahl.TabIndex = 3;
            this.btnAuswahl.Text = "wählen";
            // 
            // tabExport
            // 
            this.tabExport.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabExport.Controls.Add(this.grpDrucken);
            this.tabExport.Location = new Point(4, 25);
            this.tabExport.Name = "tabExport";
            this.tabExport.Size = new Size(876, 81);
            this.tabExport.TabIndex = 1;
            this.tabExport.Text = "Export";
            // 
            // grpDrucken
            // 
            this.grpDrucken.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDrucken.CausesValidation = false;
            this.grpDrucken.Controls.Add(this.btnVorschau);
            this.grpDrucken.Controls.Add(this.btnPageSetup);
            this.grpDrucken.Controls.Add(this.btnAlsBildSpeichern);
            this.grpDrucken.Controls.Add(this.btnDruckerDialog);
            this.grpDrucken.Dock = DockStyle.Left;
            this.grpDrucken.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpDrucken.Location = new Point(0, 0);
            this.grpDrucken.Name = "grpDrucken";
            this.grpDrucken.Size = new Size(288, 81);
            this.grpDrucken.TabIndex = 0;
            this.grpDrucken.TabStop = false;
            this.grpDrucken.Text = "Drucken";
            // 
            // btnVorschau
            // 
            this.btnVorschau.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnVorschau.ImageCode = "Datei||||||||||Lupe";
            this.btnVorschau.Location = new Point(224, 2);
            this.btnVorschau.Name = "btnVorschau";
            this.btnVorschau.Size = new Size(56, 66);
            this.btnVorschau.TabIndex = 13;
            this.btnVorschau.Text = "Vorschau";
            this.btnVorschau.Click += new EventHandler(this.btnVorschau_Click);
            // 
            // btnPageSetup
            // 
            this.btnPageSetup.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnPageSetup.ImageCode = "SeiteEinrichten";
            this.btnPageSetup.Location = new Point(136, 2);
            this.btnPageSetup.Name = "btnPageSetup";
            this.btnPageSetup.Size = new Size(88, 66);
            this.btnPageSetup.TabIndex = 12;
            this.btnPageSetup.Text = "Drucker-Seite einrichten";
            this.btnPageSetup.Click += new EventHandler(this.btnPageSetup_Click);
            // 
            // btnAlsBildSpeichern
            // 
            this.btnAlsBildSpeichern.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnAlsBildSpeichern.ImageCode = "Bild";
            this.btnAlsBildSpeichern.Location = new Point(72, 2);
            this.btnAlsBildSpeichern.Name = "btnAlsBildSpeichern";
            this.btnAlsBildSpeichern.Size = new Size(64, 66);
            this.btnAlsBildSpeichern.TabIndex = 11;
            this.btnAlsBildSpeichern.Text = "Als Bild speichern";
            this.btnAlsBildSpeichern.Click += new EventHandler(this.btnAlsBildSpeichern_Click);
            // 
            // btnDruckerDialog
            // 
            this.btnDruckerDialog.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnDruckerDialog.ImageCode = "Drucker";
            this.btnDruckerDialog.Location = new Point(8, 2);
            this.btnDruckerDialog.Name = "btnDruckerDialog";
            this.btnDruckerDialog.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDruckerDialog.Size = new Size(64, 66);
            this.btnDruckerDialog.TabIndex = 10;
            this.btnDruckerDialog.Text = "Drucken";
            this.btnDruckerDialog.Click += new EventHandler(this.btnDruckerDialog_Click);
            // 
            // tabSeiten
            // 
            this.tabSeiten.Dock = DockStyle.Top;
            this.tabSeiten.HotTrack = true;
            this.tabSeiten.Location = new Point(0, 110);
            this.tabSeiten.Name = "tabSeiten";
            this.tabSeiten.SelectedIndex = 0;
            this.tabSeiten.Size = new Size(884, 26);
            this.tabSeiten.TabDefault = null;
            this.tabSeiten.TabDefaultOrder = null;
            this.tabSeiten.TabIndex = 3;
            this.tabSeiten.Selected += new TabControlEventHandler(this.tabSeiten_Selected);
            // 
            // PadEditorReadOnly
            // 
            this.ClientSize = new Size(884, 361);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.tabSeiten);
            this.Controls.Add(this.Ribbon);
            this.Name = "PadEditorReadOnly";
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
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
        protected TabPage tabStart;
        protected TabPage tabExport;
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