using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using ColorDialog = System.Windows.Forms.ColorDialog;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using TabControl = BlueControls.Controls.TabControl;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Forms {
    public partial class PadEditor {
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
            this.grpKomponenteHinzufügen = new GroupBox();
            this.grpAssistent = new GroupBox();
            this.capRasterFangen = new Caption();
            this.capRasterAnzeige = new Caption();
            this.txbRasterFangen = new TextBox();
            this.txbRasterAnzeige = new TextBox();
            this.ckbRaster = new Button();
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
            this.tabHintergrund = new TabPage();
            this.grpDesign = new GroupBox();
            this.btnKeinHintergrund = new Button();
            this.btnHintergrundFarbe = new Button();
            this.btnArbeitsbreichSetup = new Button();
            this.PadDesign = new ComboBox();
            this.capDesign = new Caption();
            this.ColorDia = new ColorDialog();
            this.tabRightSide = new TabControl();
            this.tabElementEigenschaften = new TabPage();
            this.btnNoArea = new Button();
            this.pnlStatusBar.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpZoom.SuspendLayout();
            this.tabExport.SuspendLayout();
            this.grpDrucken.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.tabRightSide.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 337);
            this.pnlStatusBar.Size = new Size(512, 24);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new Point(176, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new Point(232, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = ButtonStyle.Button_Big_Borderless;
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
            this.Pad.Location = new Point(0, 110);
            this.Pad.Name = "Pad";
            this.Pad.Size = new Size(512, 227);
            this.Pad.TabIndex = 0;
            this.Pad.ClickedItemChanged += new EventHandler(this.Pad_ClickedItemChanged);
            this.Pad.ClickedItemChanging += new EventHandler(this.Pad_ClickedItemChanging);
            this.Pad.DrawModeChanged += new EventHandler(this.Pad_DrawModChanged);
            this.Pad.GotNewItemCollection += new EventHandler(this.Pad_GotNewItemCollection);
            this.Pad.MouseUp += new MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabStart);
            this.Ribbon.Controls.Add(this.tabExport);
            this.Ribbon.Controls.Add(this.tabHintergrund);
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
            this.tabStart.Controls.Add(this.grpKomponenteHinzufügen);
            this.tabStart.Controls.Add(this.grpAssistent);
            this.tabStart.Controls.Add(this.grpZoom);
            this.tabStart.Location = new Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new Size(876, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpKomponenteHinzufügen.CausesValidation = false;
            this.grpKomponenteHinzufügen.Dock = DockStyle.Left;
            this.grpKomponenteHinzufügen.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpKomponenteHinzufügen.Location = new Point(504, 0);
            this.grpKomponenteHinzufügen.Name = "grpKomponenteHinzufügen";
            this.grpKomponenteHinzufügen.Size = new Size(296, 81);
            this.grpKomponenteHinzufügen.TabIndex = 2;
            this.grpKomponenteHinzufügen.TabStop = false;
            this.grpKomponenteHinzufügen.Text = "Komponente hinzufügen";
            // 
            // grpAssistent
            // 
            this.grpAssistent.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAssistent.CausesValidation = false;
            this.grpAssistent.Controls.Add(this.capRasterFangen);
            this.grpAssistent.Controls.Add(this.capRasterAnzeige);
            this.grpAssistent.Controls.Add(this.txbRasterFangen);
            this.grpAssistent.Controls.Add(this.txbRasterAnzeige);
            this.grpAssistent.Controls.Add(this.ckbRaster);
            this.grpAssistent.Controls.Add(this.btnVorschauModus);
            this.grpAssistent.Dock = DockStyle.Left;
            this.grpAssistent.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpAssistent.Location = new Point(296, 0);
            this.grpAssistent.Name = "grpAssistent";
            this.grpAssistent.Size = new Size(208, 81);
            this.grpAssistent.TabIndex = 0;
            this.grpAssistent.TabStop = false;
            this.grpAssistent.Text = "Assistenten";
            // 
            // capRasterFangen
            // 
            this.capRasterFangen.CausesValidation = false;
            this.capRasterFangen.Location = new Point(72, 46);
            this.capRasterFangen.Name = "capRasterFangen";
            this.capRasterFangen.Size = new Size(56, 22);
            this.capRasterFangen.Text = "Fangen:";
            // 
            // capRasterAnzeige
            // 
            this.capRasterAnzeige.CausesValidation = false;
            this.capRasterAnzeige.Location = new Point(72, 24);
            this.capRasterAnzeige.Name = "capRasterAnzeige";
            this.capRasterAnzeige.Size = new Size(56, 22);
            this.capRasterAnzeige.Text = "Anzeige:";
            // 
            // txbRasterFangen
            // 
            this.txbRasterFangen.AdditionalFormatCheck = AdditionalCheck.Float;
            this.txbRasterFangen.AllowedChars = "0123456789,";
            this.txbRasterFangen.Cursor = Cursors.IBeam;
            this.txbRasterFangen.Location = new Point(128, 46);
            this.txbRasterFangen.Name = "txbRasterFangen";
            this.txbRasterFangen.RegexCheck = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterFangen.Size = new Size(64, 22);
            this.txbRasterFangen.Suffix = "mm";
            this.txbRasterFangen.TabIndex = 19;
            this.txbRasterFangen.Text = "10";
            this.txbRasterFangen.TextChanged += new EventHandler(this.txbRasterFangen_TextChanged);
            // 
            // txbRasterAnzeige
            // 
            this.txbRasterAnzeige.AdditionalFormatCheck = AdditionalCheck.Float;
            this.txbRasterAnzeige.AllowedChars = "0123456789,";
            this.txbRasterAnzeige.Cursor = Cursors.IBeam;
            this.txbRasterAnzeige.Location = new Point(128, 24);
            this.txbRasterAnzeige.Name = "txbRasterAnzeige";
            this.txbRasterAnzeige.RegexCheck = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterAnzeige.Size = new Size(64, 22);
            this.txbRasterAnzeige.Suffix = "mm";
            this.txbRasterAnzeige.TabIndex = 18;
            this.txbRasterAnzeige.Text = "10";
            this.txbRasterAnzeige.TextChanged += new EventHandler(this.txbRasterAnzeige_TextChanged);
            // 
            // ckbRaster
            // 
            this.ckbRaster.ButtonStyle = ButtonStyle.Checkbox_Big_Borderless;
            this.ckbRaster.ImageCode = "Raster|18";
            this.ckbRaster.Location = new Point(72, 2);
            this.ckbRaster.Name = "ckbRaster";
            this.ckbRaster.Size = new Size(120, 22);
            this.ckbRaster.TabIndex = 17;
            this.ckbRaster.Text = "Raster";
            this.ckbRaster.CheckedChanged += new EventHandler(this.ckbRaster_CheckedChanged);
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.ButtonStyle = ButtonStyle.Checkbox_Big_Borderless;
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
            this.btnZoom11.ButtonStyle = ButtonStyle.Button_Big_Borderless;
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
            this.btnAuswahl.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
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
            this.btnVorschau.ButtonStyle = ButtonStyle.Button_Big_Borderless;
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
            this.btnPageSetup.ButtonStyle = ButtonStyle.Button_Big_Borderless;
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
            this.btnAlsBildSpeichern.ButtonStyle = ButtonStyle.Button_Big_Borderless;
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
            this.btnDruckerDialog.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnDruckerDialog.ImageCode = "Drucker";
            this.btnDruckerDialog.Location = new Point(8, 2);
            this.btnDruckerDialog.Name = "btnDruckerDialog";
            this.btnDruckerDialog.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDruckerDialog.Size = new Size(64, 66);
            this.btnDruckerDialog.TabIndex = 10;
            this.btnDruckerDialog.Text = "Drucken";
            this.btnDruckerDialog.Click += new EventHandler(this.btnDruckerDialog_Click);
            // 
            // tabHintergrund
            // 
            this.tabHintergrund.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabHintergrund.Controls.Add(this.grpDesign);
            this.tabHintergrund.Location = new Point(4, 25);
            this.tabHintergrund.Name = "tabHintergrund";
            this.tabHintergrund.Padding = new Padding(3);
            this.tabHintergrund.Size = new Size(876, 81);
            this.tabHintergrund.TabIndex = 2;
            this.tabHintergrund.Text = "Hintergrund";
            // 
            // grpDesign
            // 
            this.grpDesign.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDesign.CausesValidation = false;
            this.grpDesign.Controls.Add(this.btnNoArea);
            this.grpDesign.Controls.Add(this.btnKeinHintergrund);
            this.grpDesign.Controls.Add(this.btnHintergrundFarbe);
            this.grpDesign.Controls.Add(this.btnArbeitsbreichSetup);
            this.grpDesign.Controls.Add(this.PadDesign);
            this.grpDesign.Controls.Add(this.capDesign);
            this.grpDesign.Dock = DockStyle.Left;
            this.grpDesign.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpDesign.Location = new Point(3, 3);
            this.grpDesign.Name = "grpDesign";
            this.grpDesign.Size = new Size(648, 75);
            this.grpDesign.TabIndex = 2;
            this.grpDesign.TabStop = false;
            this.grpDesign.Text = "Design";
            // 
            // btnKeinHintergrund
            // 
            this.btnKeinHintergrund.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnKeinHintergrund.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnKeinHintergrund.Location = new Point(464, 2);
            this.btnKeinHintergrund.Name = "btnKeinHintergrund";
            this.btnKeinHintergrund.Size = new Size(136, 22);
            this.btnKeinHintergrund.TabIndex = 16;
            this.btnKeinHintergrund.Text = "kein Hintergrund";
            this.btnKeinHintergrund.Click += new EventHandler(this.btnKeinHintergrund_Click);
            // 
            // btnHintergrundFarbe
            // 
            this.btnHintergrundFarbe.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnHintergrundFarbe.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnHintergrundFarbe.ImageCode = "Farben";
            this.btnHintergrundFarbe.Location = new Point(376, 2);
            this.btnHintergrundFarbe.Name = "btnHintergrundFarbe";
            this.btnHintergrundFarbe.Size = new Size(80, 66);
            this.btnHintergrundFarbe.TabIndex = 15;
            this.btnHintergrundFarbe.Text = "Hintergrund-Farbe";
            this.btnHintergrundFarbe.Click += new EventHandler(this.btnHintergrundFarbe_Click);
            // 
            // btnArbeitsbreichSetup
            // 
            this.btnArbeitsbreichSetup.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnArbeitsbreichSetup.ImageCode = "SeiteEinrichten";
            this.btnArbeitsbreichSetup.Location = new Point(8, 2);
            this.btnArbeitsbreichSetup.Name = "btnArbeitsbreichSetup";
            this.btnArbeitsbreichSetup.Size = new Size(96, 66);
            this.btnArbeitsbreichSetup.TabIndex = 13;
            this.btnArbeitsbreichSetup.Text = "Arbeitsbereich einrichten";
            this.btnArbeitsbreichSetup.Click += new EventHandler(this.btnArbeitsbreichSetup_Click);
            // 
            // PadDesign
            // 
            this.PadDesign.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.PadDesign.Cursor = Cursors.IBeam;
            this.PadDesign.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PadDesign.Location = new Point(112, 24);
            this.PadDesign.Name = "PadDesign";
            this.PadDesign.RegexCheck = null;
            this.PadDesign.Size = new Size(264, 22);
            this.PadDesign.TabIndex = 1;
            this.PadDesign.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.PadDesign_ItemClicked);
            // 
            // capDesign
            // 
            this.capDesign.CausesValidation = false;
            this.capDesign.Location = new Point(112, 2);
            this.capDesign.Name = "capDesign";
            this.capDesign.Size = new Size(77, 22);
            this.capDesign.Text = "Design:";
            // 
            // tabRightSide
            // 
            this.tabRightSide.Controls.Add(this.tabElementEigenschaften);
            this.tabRightSide.Dock = DockStyle.Right;
            this.tabRightSide.HotTrack = true;
            this.tabRightSide.Location = new Point(512, 110);
            this.tabRightSide.Name = "tabRightSide";
            this.tabRightSide.SelectedIndex = 0;
            this.tabRightSide.Size = new Size(372, 251);
            this.tabRightSide.TabDefault = null;
            this.tabRightSide.TabDefaultOrder = null;
            this.tabRightSide.TabIndex = 4;
            // 
            // tabElementEigenschaften
            // 
            this.tabElementEigenschaften.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabElementEigenschaften.Location = new Point(4, 25);
            this.tabElementEigenschaften.Name = "tabElementEigenschaften";
            this.tabElementEigenschaften.Size = new Size(364, 222);
            this.tabElementEigenschaften.TabIndex = 0;
            this.tabElementEigenschaften.Text = "Element-Eigenschaften";
            // 
            // btnNoArea
            // 
            this.btnNoArea.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnNoArea.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnNoArea.Location = new Point(464, 24);
            this.btnNoArea.Name = "btnNoArea";
            this.btnNoArea.Size = new Size(136, 22);
            this.btnNoArea.TabIndex = 17;
            this.btnNoArea.Text = "kein Arbeitsbereich";
            this.btnNoArea.Click += new EventHandler(this.btnNoArea_Click);
            // 
            // PadEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(884, 361);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.tabRightSide);
            this.Controls.Add(this.Ribbon);
            this.Name = "PadEditor";
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.tabRightSide, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.Pad, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.grpZoom.ResumeLayout(false);
            this.tabExport.ResumeLayout(false);
            this.grpDrucken.ResumeLayout(false);
            this.tabHintergrund.ResumeLayout(false);
            this.grpDesign.ResumeLayout(false);
            this.tabRightSide.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        public Button btnZoomOut;
        private Button btnAuswahl;
        public CreativePad Pad;
        protected RibbonBar Ribbon;
        protected TabPage tabStart;
        protected TabPage tabExport;
        protected GroupBox grpAssistent;
        public Button btnZoomFit;
        public Button btnZoomIn;
        private Button btnDruckerDialog;
        private Button btnAlsBildSpeichern;
        protected Button btnVorschau;
        private Button btnZoom11;
        private GroupBox grpZoom;
        private GroupBox grpDrucken;
        private Button btnPageSetup;
        protected Button btnVorschauModus;
        protected GroupBox grpKomponenteHinzufügen;
        private Caption capRasterFangen;
        private Caption capRasterAnzeige;
        private TextBox txbRasterFangen;
        private TextBox txbRasterAnzeige;
        protected Button ckbRaster;
        private ColorDialog ColorDia;
        protected TabPage tabHintergrund;
        private TabPage tabElementEigenschaften;
        protected GroupBox grpDesign;
        private Button btnKeinHintergrund;
        private Button btnHintergrundFarbe;
        protected internal Button btnArbeitsbreichSetup;
        private ComboBox PadDesign;
        private Caption capDesign;
        protected TabControl tabRightSide;
        private Button btnNoArea;
    }
}