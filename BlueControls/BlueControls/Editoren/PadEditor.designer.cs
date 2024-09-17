using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using ColorDialog = System.Windows.Forms.ColorDialog;
using GroupBox = BlueControls.Controls.GroupBox;
using TabControl = BlueControls.Controls.TabControl;

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
            this.btnZoomOut = new BlueControls.Controls.Button();
            this.btnZoomIn = new BlueControls.Controls.Button();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.Pad = new BlueControls.Controls.CreativePad();
            this.Ribbon = new BlueControls.Controls.RibbonBar();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.grpKomponenteHinzufügen = new BlueControls.Controls.GroupBox();
            this.btnWeitereAllItem = new BlueControls.Controls.Button();
            this.btnAddDynamicSymbol = new BlueControls.Controls.Button();
            this.btnAddPhsyik = new BlueControls.Controls.Button();
            this.btnAddSymbol = new BlueControls.Controls.Button();
            this.btnAddUnterStufe = new BlueControls.Controls.Button();
            this.btnAddText = new BlueControls.Controls.Button();
            this.btnAddImage = new BlueControls.Controls.Button();
            this.btnAddDimension = new BlueControls.Controls.Button();
            this.btnAddLine = new BlueControls.Controls.Button();
            this.grpAssistent = new BlueControls.Controls.GroupBox();
            this.capRasterFangen = new BlueControls.Controls.Caption();
            this.capRasterAnzeige = new BlueControls.Controls.Caption();
            this.txbRasterFangen = new BlueControls.Controls.TextBox();
            this.txbRasterAnzeige = new BlueControls.Controls.TextBox();
            this.ckbRaster = new BlueControls.Controls.Button();
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
            this.tabHintergrund = new System.Windows.Forms.TabPage();
            this.grpDesign = new BlueControls.Controls.GroupBox();
            this.btnKeinHintergrund = new BlueControls.Controls.Button();
            this.btnHintergrundFarbe = new BlueControls.Controls.Button();
            this.btnArbeitsbreichSetup = new BlueControls.Controls.Button();
            this.cbxSchriftGröße = new BlueControls.Controls.ComboBox();
            this.capSchriftgröße = new BlueControls.Controls.Caption();
            this.PadDesign = new BlueControls.Controls.ComboBox();
            this.capDesign = new BlueControls.Controls.Caption();
            this.tabSeiten = new BlueControls.Controls.TabControl();
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.tabRightSide = new BlueControls.Controls.TabControl();
            this.tabElementEigenschaften = new System.Windows.Forms.TabPage();
            this.pnlStatusBar.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.grpZoom.SuspendLayout();
            this.tabExport.SuspendLayout();
            this.grpDrucken.SuspendLayout();
            this.tabHintergrund.SuspendLayout();
            this.grpDesign.SuspendLayout();
            this.tabRightSide.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(512, 24);
            this.capStatusBar.Text = "<imagecode=Häkchen|16> Nix besonderes zu berichten...";
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 337);
            this.pnlStatusBar.Size = new System.Drawing.Size(512, 24);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Big_Borderless;
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new System.Drawing.Point(176, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Big_Borderless;
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new System.Drawing.Point(232, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.Pad.Item = null;
            this.Pad.Location = new System.Drawing.Point(0, 136);
            this.Pad.Mode = "";
            this.Pad.Name = "Pad";
            this.Pad.ShiftX = 0F;
            this.Pad.ShiftY = 0F;
            this.Pad.Size = new System.Drawing.Size(512, 225);
            this.Pad.TabIndex = 0;
            this.Pad.Zoom = 1F;
            this.Pad.DrawModeChanged += new System.EventHandler(this.Pad_DrawModChanged);
            this.Pad.GotNewItemCollection += new System.EventHandler(this.Pad_GotNewItemCollection);
            this.Pad.PropertyChanged += new System.EventHandler(this.Pad_PropertyChanged);
            this.Pad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabStart);
            this.Ribbon.Controls.Add(this.tabExport);
            this.Ribbon.Controls.Add(this.tabHintergrund);
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
            this.tabStart.Controls.Add(this.grpKomponenteHinzufügen);
            this.tabStart.Controls.Add(this.grpAssistent);
            this.tabStart.Controls.Add(this.grpZoom);
            this.tabStart.Location = new System.Drawing.Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new System.Drawing.Size(876, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpKomponenteHinzufügen.CausesValidation = false;
            this.grpKomponenteHinzufügen.Controls.Add(this.btnWeitereAllItem);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddDynamicSymbol);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddPhsyik);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddSymbol);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddUnterStufe);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddText);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddImage);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddDimension);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddLine);
            this.grpKomponenteHinzufügen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpKomponenteHinzufügen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpKomponenteHinzufügen.Location = new System.Drawing.Point(504, 0);
            this.grpKomponenteHinzufügen.Name = "grpKomponenteHinzufügen";
            this.grpKomponenteHinzufügen.Size = new System.Drawing.Size(296, 81);
            this.grpKomponenteHinzufügen.TabIndex = 2;
            this.grpKomponenteHinzufügen.TabStop = false;
            this.grpKomponenteHinzufügen.Text = "Komponente hinzufügen";
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
            // btnAddDynamicSymbol
            // 
            this.btnAddDynamicSymbol.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddDynamicSymbol.ImageCode = "Variable|16";
            this.btnAddDynamicSymbol.Location = new System.Drawing.Point(96, 46);
            this.btnAddDynamicSymbol.Name = "btnAddDynamicSymbol";
            this.btnAddDynamicSymbol.Size = new System.Drawing.Size(96, 22);
            this.btnAddDynamicSymbol.TabIndex = 11;
            this.btnAddDynamicSymbol.Text = "Dyn. Symbol";
            this.btnAddDynamicSymbol.Click += new System.EventHandler(this.btnAddDynamicSymbol_Click);
            // 
            // btnAddPhsyik
            // 
            this.btnAddPhsyik.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAddPhsyik.ImageCode = "Formel|16";
            this.btnAddPhsyik.Location = new System.Drawing.Point(192, 24);
            this.btnAddPhsyik.Name = "btnAddPhsyik";
            this.btnAddPhsyik.Size = new System.Drawing.Size(88, 22);
            this.btnAddPhsyik.TabIndex = 10;
            this.btnAddPhsyik.Text = "Physik";
            this.btnAddPhsyik.Click += new System.EventHandler(this.btnAddUnterStufe_Click);
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
            // grpAssistent
            // 
            this.grpAssistent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAssistent.CausesValidation = false;
            this.grpAssistent.Controls.Add(this.capRasterFangen);
            this.grpAssistent.Controls.Add(this.capRasterAnzeige);
            this.grpAssistent.Controls.Add(this.txbRasterFangen);
            this.grpAssistent.Controls.Add(this.txbRasterAnzeige);
            this.grpAssistent.Controls.Add(this.ckbRaster);
            this.grpAssistent.Controls.Add(this.btnVorschauModus);
            this.grpAssistent.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAssistent.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAssistent.Location = new System.Drawing.Point(296, 0);
            this.grpAssistent.Name = "grpAssistent";
            this.grpAssistent.Size = new System.Drawing.Size(208, 81);
            this.grpAssistent.TabIndex = 0;
            this.grpAssistent.TabStop = false;
            this.grpAssistent.Text = "Assistenten";
            // 
            // capRasterFangen
            // 
            this.capRasterFangen.CausesValidation = false;
            this.capRasterFangen.Location = new System.Drawing.Point(72, 46);
            this.capRasterFangen.Name = "capRasterFangen";
            this.capRasterFangen.Size = new System.Drawing.Size(56, 22);
            this.capRasterFangen.Text = "Fangen:";
            // 
            // capRasterAnzeige
            // 
            this.capRasterAnzeige.CausesValidation = false;
            this.capRasterAnzeige.Location = new System.Drawing.Point(72, 24);
            this.capRasterAnzeige.Name = "capRasterAnzeige";
            this.capRasterAnzeige.Size = new System.Drawing.Size(56, 22);
            this.capRasterAnzeige.Text = "Anzeige:";
            // 
            // txbRasterFangen
            // 
            this.txbRasterFangen.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.Float;
            this.txbRasterFangen.AllowedChars = "0123456789,";
            this.txbRasterFangen.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRasterFangen.Location = new System.Drawing.Point(128, 46);
            this.txbRasterFangen.Name = "txbRasterFangen";
            this.txbRasterFangen.Regex = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterFangen.Size = new System.Drawing.Size(64, 22);
            this.txbRasterFangen.Suffix = "mm";
            this.txbRasterFangen.TabIndex = 19;
            this.txbRasterFangen.Text = "10";
            this.txbRasterFangen.TextChanged += new System.EventHandler(this.txbRasterFangen_TextChanged);
            // 
            // txbRasterAnzeige
            // 
            this.txbRasterAnzeige.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.Float;
            this.txbRasterAnzeige.AllowedChars = "0123456789,";
            this.txbRasterAnzeige.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRasterAnzeige.Location = new System.Drawing.Point(128, 24);
            this.txbRasterAnzeige.Name = "txbRasterAnzeige";
            this.txbRasterAnzeige.Regex = "(^-?([1-9]\\d*)|^0)([.]\\d*[1-9])?$";
            this.txbRasterAnzeige.Size = new System.Drawing.Size(64, 22);
            this.txbRasterAnzeige.Suffix = "mm";
            this.txbRasterAnzeige.TabIndex = 18;
            this.txbRasterAnzeige.Text = "10";
            this.txbRasterAnzeige.TextChanged += new System.EventHandler(this.txbRasterAnzeige_TextChanged);
            // 
            // ckbRaster
            // 
            this.ckbRaster.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Big_Borderless;
            this.ckbRaster.ImageCode = "Raster|18";
            this.ckbRaster.Location = new System.Drawing.Point(72, 2);
            this.ckbRaster.Name = "ckbRaster";
            this.ckbRaster.Size = new System.Drawing.Size(120, 22);
            this.ckbRaster.TabIndex = 17;
            this.ckbRaster.Text = "Raster";
            this.ckbRaster.CheckedChanged += new System.EventHandler(this.ckbRaster_CheckedChanged);
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Big_Borderless;
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
            this.btnZoom11.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.btnAuswahl.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Big_Borderless;
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
            this.btnVorschau.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.btnPageSetup.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.btnAlsBildSpeichern.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
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
            this.btnDruckerDialog.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnDruckerDialog.ImageCode = "Drucker";
            this.btnDruckerDialog.Location = new System.Drawing.Point(8, 2);
            this.btnDruckerDialog.Name = "btnDruckerDialog";
            this.btnDruckerDialog.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDruckerDialog.Size = new System.Drawing.Size(64, 66);
            this.btnDruckerDialog.TabIndex = 10;
            this.btnDruckerDialog.Text = "Drucken";
            this.btnDruckerDialog.Click += new System.EventHandler(this.btnDruckerDialog_Click);
            // 
            // tabHintergrund
            // 
            this.tabHintergrund.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabHintergrund.Controls.Add(this.grpDesign);
            this.tabHintergrund.Location = new System.Drawing.Point(4, 25);
            this.tabHintergrund.Name = "tabHintergrund";
            this.tabHintergrund.Padding = new System.Windows.Forms.Padding(3);
            this.tabHintergrund.Size = new System.Drawing.Size(876, 81);
            this.tabHintergrund.TabIndex = 2;
            this.tabHintergrund.Text = "Hintergrund";
            // 
            // grpDesign
            // 
            this.grpDesign.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDesign.CausesValidation = false;
            this.grpDesign.Controls.Add(this.btnKeinHintergrund);
            this.grpDesign.Controls.Add(this.btnHintergrundFarbe);
            this.grpDesign.Controls.Add(this.btnArbeitsbreichSetup);
            this.grpDesign.Controls.Add(this.cbxSchriftGröße);
            this.grpDesign.Controls.Add(this.capSchriftgröße);
            this.grpDesign.Controls.Add(this.PadDesign);
            this.grpDesign.Controls.Add(this.capDesign);
            this.grpDesign.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDesign.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpDesign.Location = new System.Drawing.Point(3, 3);
            this.grpDesign.Name = "grpDesign";
            this.grpDesign.Size = new System.Drawing.Size(648, 75);
            this.grpDesign.TabIndex = 2;
            this.grpDesign.TabStop = false;
            this.grpDesign.Text = "Design";
            // 
            // btnKeinHintergrund
            // 
            this.btnKeinHintergrund.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKeinHintergrund.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnKeinHintergrund.Location = new System.Drawing.Point(464, 2);
            this.btnKeinHintergrund.Name = "btnKeinHintergrund";
            this.btnKeinHintergrund.Size = new System.Drawing.Size(112, 22);
            this.btnKeinHintergrund.TabIndex = 16;
            this.btnKeinHintergrund.Text = "kein Hintergrund";
            this.btnKeinHintergrund.Click += new System.EventHandler(this.btnKeinHintergrund_Click);
            // 
            // btnHintergrundFarbe
            // 
            this.btnHintergrundFarbe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHintergrundFarbe.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnHintergrundFarbe.ImageCode = "Farben";
            this.btnHintergrundFarbe.Location = new System.Drawing.Point(376, 2);
            this.btnHintergrundFarbe.Name = "btnHintergrundFarbe";
            this.btnHintergrundFarbe.Size = new System.Drawing.Size(80, 66);
            this.btnHintergrundFarbe.TabIndex = 15;
            this.btnHintergrundFarbe.Text = "Hintergrund-Farbe";
            this.btnHintergrundFarbe.Click += new System.EventHandler(this.btnHintergrundFarbe_Click);
            // 
            // btnArbeitsbreichSetup
            // 
            this.btnArbeitsbreichSetup.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnArbeitsbreichSetup.ImageCode = "SeiteEinrichten";
            this.btnArbeitsbreichSetup.Location = new System.Drawing.Point(8, 2);
            this.btnArbeitsbreichSetup.Name = "btnArbeitsbreichSetup";
            this.btnArbeitsbreichSetup.Size = new System.Drawing.Size(96, 66);
            this.btnArbeitsbreichSetup.TabIndex = 13;
            this.btnArbeitsbreichSetup.Text = "Arbeitsbereich einreichten";
            this.btnArbeitsbreichSetup.Click += new System.EventHandler(this.btnArbeitsbreichSetup_Click);
            // 
            // cbxSchriftGröße
            // 
            this.cbxSchriftGröße.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxSchriftGröße.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxSchriftGröße.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSchriftGröße.Location = new System.Drawing.Point(208, 46);
            this.cbxSchriftGröße.Name = "cbxSchriftGröße";
            this.cbxSchriftGröße.Regex = null;
            this.cbxSchriftGröße.Size = new System.Drawing.Size(168, 22);
            this.cbxSchriftGröße.TabIndex = 3;
            this.cbxSchriftGröße.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.cbxSchriftGröße_ItemClicked);
            // 
            // capSchriftgröße
            // 
            this.capSchriftgröße.CausesValidation = false;
            this.capSchriftgröße.Location = new System.Drawing.Point(112, 46);
            this.capSchriftgröße.Name = "capSchriftgröße";
            this.capSchriftgröße.Size = new System.Drawing.Size(88, 22);
            this.capSchriftgröße.Text = "Schrift-Größe:";
            // 
            // PadDesign
            // 
            this.PadDesign.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PadDesign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.PadDesign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PadDesign.Location = new System.Drawing.Point(112, 24);
            this.PadDesign.Name = "PadDesign";
            this.PadDesign.Regex = null;
            this.PadDesign.Size = new System.Drawing.Size(264, 22);
            this.PadDesign.TabIndex = 1;
            this.PadDesign.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.PadDesign_ItemClicked);
            // 
            // capDesign
            // 
            this.capDesign.CausesValidation = false;
            this.capDesign.Location = new System.Drawing.Point(112, 2);
            this.capDesign.Name = "capDesign";
            this.capDesign.Size = new System.Drawing.Size(77, 22);
            this.capDesign.Text = "Design:";
            this.capDesign.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // tabSeiten
            // 
            this.tabSeiten.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabSeiten.HotTrack = true;
            this.tabSeiten.Location = new System.Drawing.Point(0, 110);
            this.tabSeiten.Name = "tabSeiten";
            this.tabSeiten.SelectedIndex = 0;
            this.tabSeiten.Size = new System.Drawing.Size(884, 26);
            this.tabSeiten.TabDefault = null;
            this.tabSeiten.TabDefaultOrder = null;
            this.tabSeiten.TabIndex = 3;
            this.tabSeiten.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabSeiten_Selected);
            // 
            // tabRightSide
            // 
            this.tabRightSide.Controls.Add(this.tabElementEigenschaften);
            this.tabRightSide.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabRightSide.HotTrack = true;
            this.tabRightSide.Location = new System.Drawing.Point(512, 136);
            this.tabRightSide.Name = "tabRightSide";
            this.tabRightSide.SelectedIndex = 0;
            this.tabRightSide.Size = new System.Drawing.Size(372, 225);
            this.tabRightSide.TabDefault = null;
            this.tabRightSide.TabDefaultOrder = null;
            this.tabRightSide.TabIndex = 4;
            // 
            // tabElementEigenschaften
            // 
            this.tabElementEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabElementEigenschaften.Location = new System.Drawing.Point(4, 25);
            this.tabElementEigenschaften.Name = "tabElementEigenschaften";
            this.tabElementEigenschaften.Size = new System.Drawing.Size(364, 196);
            this.tabElementEigenschaften.TabIndex = 0;
            this.tabElementEigenschaften.Text = "Element-Eigenschaften";
            // 
            // PadEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(884, 361);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.tabRightSide);
            this.Controls.Add(this.tabSeiten);
            this.Controls.Add(this.Ribbon);
            this.Name = "PadEditor";
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.tabSeiten, 0);
            this.Controls.SetChildIndex(this.tabRightSide, 0);
            this.Controls.SetChildIndex(this.Pad, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
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
        protected CreativePad Pad;
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
        protected TabControl tabSeiten;
        protected GroupBox grpKomponenteHinzufügen;
        private Button btnWeitereAllItem;
        private Button btnAddDynamicSymbol;
        private Button btnAddPhsyik;
        private Button btnAddSymbol;
        private Button btnAddUnterStufe;
        private Button btnAddText;
        private Button btnAddImage;
        private Button btnAddDimension;
        private Button btnAddLine;
        private Caption capRasterFangen;
        private Caption capRasterAnzeige;
        private Controls.TextBox txbRasterFangen;
        private Controls.TextBox txbRasterAnzeige;
        protected Button ckbRaster;
        private ColorDialog ColorDia;
        protected TabPage tabHintergrund;
        private TabPage tabElementEigenschaften;
        protected GroupBox grpDesign;
        private Button btnKeinHintergrund;
        private Button btnHintergrundFarbe;
        protected internal Button btnArbeitsbreichSetup;
        internal Controls.ComboBox cbxSchriftGröße;
        internal Caption capSchriftgröße;
        private Controls.ComboBox PadDesign;
        private Caption capDesign;
        protected TabControl tabRightSide;
    }
}