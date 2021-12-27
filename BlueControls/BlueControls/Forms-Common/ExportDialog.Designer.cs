using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms
    {
        public sealed partial class ExportDialog : Form
		{
			//Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing )
					{
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}
			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.capAnzahlInfo = new BlueControls.Controls.Caption();
            this.cbxLayoutWahl = new BlueControls.Controls.ComboBox();
            this.c_Layoutx = new BlueControls.Controls.Caption();
            this.Caption3 = new BlueControls.Controls.Caption();
            this.MachZu = new BlueControls.Controls.Button();
            this.btnLayoutEditorÖffnen = new BlueControls.Controls.Button();
            this.FrmDrucken_ExportVerzeichniss = new BlueControls.Controls.Button();
            this.capLayout = new BlueControls.Controls.Caption();
            this.padVorschau = new BlueControls.Controls.CreativePad();
            this.Tabs = new BlueControls.Controls.TabControl();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.grpArt = new BlueControls.Controls.GroupBox();
            this.optSpezialFormat = new BlueControls.Controls.Button();
            this.optBildSchateln = new BlueControls.Controls.Button();
            this.optSpeichern = new BlueControls.Controls.Button();
            this.optDrucken = new BlueControls.Controls.Button();
            this.btnWeiter = new BlueControls.Controls.Button();
            this.grpEinträge = new BlueControls.Controls.GroupBox();
            this.tabDrucken = new System.Windows.Forms.TabPage();
            this.Vorschau = new BlueControls.Controls.Button();
            this.btnDrucken = new BlueControls.Controls.Button();
            this.Button_PageSetup = new BlueControls.Controls.Button();
            this.padPrint = new BlueControls.Controls.CreativePad();
            this.tabBildSchachteln = new System.Windows.Forms.TabPage();
            this.btnEinstellung = new BlueControls.Controls.Button();
            this.capDPI = new BlueControls.Controls.Caption();
            this.flxAbstand = new BlueControls.Controls.FlexiControl();
            this.flxHöhe = new BlueControls.Controls.FlexiControl();
            this.flxBreite = new BlueControls.Controls.FlexiControl();
            this.btnSchachtelnSpeichern = new BlueControls.Controls.Button();
            this.padSchachteln = new BlueControls.Controls.CreativePad();
            this.tabDateiExport = new System.Windows.Forms.TabPage();
            this.Caption4 = new BlueControls.Controls.Caption();
            this.lstExported = new BlueControls.Controls.ListBox();
            this.Tabs.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpArt.SuspendLayout();
            this.grpEinträge.SuspendLayout();
            this.tabDrucken.SuspendLayout();
            this.tabBildSchachteln.SuspendLayout();
            this.tabDateiExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // capAnzahlInfo
            // 
            this.capAnzahlInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capAnzahlInfo.CausesValidation = false;
            this.capAnzahlInfo.Location = new System.Drawing.Point(8, 16);
            this.capAnzahlInfo.Name = "capAnzahlInfo";
            this.capAnzahlInfo.Size = new System.Drawing.Size(847, 40);
            this.capAnzahlInfo.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxLayoutWahl
            // 
            this.cbxLayoutWahl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxLayoutWahl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxLayoutWahl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLayoutWahl.Location = new System.Drawing.Point(232, 48);
            this.cbxLayoutWahl.Name = "cbxLayoutWahl";
            this.cbxLayoutWahl.Size = new System.Drawing.Size(623, 24);
            this.cbxLayoutWahl.TabIndex = 80;
            this.cbxLayoutWahl.TextChanged += new System.EventHandler(this.cbxLayoutWahl_TextChanged);
            // 
            // c_Layoutx
            // 
            this.c_Layoutx.CausesValidation = false;
            this.c_Layoutx.Location = new System.Drawing.Point(-86, 51);
            this.c_Layoutx.Name = "c_Layoutx";
            this.c_Layoutx.Size = new System.Drawing.Size(80, 16);
            this.c_Layoutx.Text = "c_Layoutx";
            this.c_Layoutx.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new System.Drawing.Point(512, -208);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new System.Drawing.Size(46, 20);
            this.Caption3.Text = "Layout:";
            // 
            // MachZu
            // 
            this.MachZu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MachZu.ImageCode = "Häkchen|16";
            this.MachZu.Location = new System.Drawing.Point(743, 641);
            this.MachZu.Name = "MachZu";
            this.MachZu.Size = new System.Drawing.Size(112, 40);
            this.MachZu.TabIndex = 85;
            this.MachZu.Text = "Beenden";
            this.MachZu.Click += new System.EventHandler(this.FrmDrucken_Drucken_Click);
            // 
            // btnLayoutEditorÖffnen
            // 
            this.btnLayoutEditorÖffnen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLayoutEditorÖffnen.ImageCode = "Layout|16";
            this.btnLayoutEditorÖffnen.Location = new System.Drawing.Point(704, 16);
            this.btnLayoutEditorÖffnen.Name = "btnLayoutEditorÖffnen";
            this.btnLayoutEditorÖffnen.Size = new System.Drawing.Size(152, 32);
            this.btnLayoutEditorÖffnen.TabIndex = 86;
            this.btnLayoutEditorÖffnen.Text = "Layout bearbeiten";
            this.btnLayoutEditorÖffnen.Click += new System.EventHandler(this.LayoutEditor_Click);
            // 
            // FrmDrucken_ExportVerzeichniss
            // 
            this.FrmDrucken_ExportVerzeichniss.ImageCode = "Ordner|16";
            this.FrmDrucken_ExportVerzeichniss.Location = new System.Drawing.Point(8, 448);
            this.FrmDrucken_ExportVerzeichniss.Name = "FrmDrucken_ExportVerzeichniss";
            this.FrmDrucken_ExportVerzeichniss.Size = new System.Drawing.Size(208, 40);
            this.FrmDrucken_ExportVerzeichniss.TabIndex = 87;
            this.FrmDrucken_ExportVerzeichniss.Text = "Export Verzeichnis öffnen";
            this.FrmDrucken_ExportVerzeichniss.Click += new System.EventHandler(this.Button1_Click);
            // 
            // capLayout
            // 
            this.capLayout.CausesValidation = false;
            this.capLayout.Location = new System.Drawing.Point(232, 24);
            this.capLayout.Name = "capLayout";
            this.capLayout.Size = new System.Drawing.Size(82, 22);
            this.capLayout.Text = "Layout:";
            this.capLayout.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // padVorschau
            // 
            this.padVorschau.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.padVorschau.EditAllowed = false;
            this.padVorschau.Location = new System.Drawing.Point(232, 80);
            this.padVorschau.Name = "padVorschau";
            this.padVorschau.ShowInPrintMode = true;
            this.padVorschau.Size = new System.Drawing.Size(623, 480);
            this.padVorschau.TabIndex = 1;
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.tabStart);
            this.Tabs.Controls.Add(this.tabDrucken);
            this.Tabs.Controls.Add(this.tabBildSchachteln);
            this.Tabs.Controls.Add(this.tabDateiExport);
            this.Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tabs.HotTrack = true;
            this.Tabs.Location = new System.Drawing.Point(0, 0);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(868, 716);
            this.Tabs.TabIndex = 81;
            this.Tabs.SelectedIndexChanged += new System.EventHandler(this.Tabs_SelectedIndexChanged);
            // 
            // tabStart
            // 
            this.tabStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabStart.Controls.Add(this.grpArt);
            this.tabStart.Controls.Add(this.btnWeiter);
            this.tabStart.Controls.Add(this.grpEinträge);
            this.tabStart.Location = new System.Drawing.Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new System.Drawing.Size(860, 687);
            this.tabStart.TabIndex = 3;
            this.tabStart.Text = "Start";
            // 
            // grpArt
            // 
            this.grpArt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpArt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpArt.Controls.Add(this.padVorschau);
            this.grpArt.Controls.Add(this.cbxLayoutWahl);
            this.grpArt.Controls.Add(this.btnLayoutEditorÖffnen);
            this.grpArt.Controls.Add(this.optSpezialFormat);
            this.grpArt.Controls.Add(this.optBildSchateln);
            this.grpArt.Controls.Add(this.capLayout);
            this.grpArt.Controls.Add(this.optSpeichern);
            this.grpArt.Controls.Add(this.optDrucken);
            this.grpArt.Location = new System.Drawing.Point(0, 64);
            this.grpArt.Name = "grpArt";
            this.grpArt.Size = new System.Drawing.Size(860, 568);
            this.grpArt.TabIndex = 89;
            this.grpArt.TabStop = false;
            this.grpArt.Text = "Art des Exportes";
            // 
            // optSpezialFormat
            // 
            this.optSpezialFormat.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Optionbox | BlueControls.Enums.enButtonStyle.Text)));
            this.optSpezialFormat.ImageCode = "Diskette";
            this.optSpezialFormat.Location = new System.Drawing.Point(8, 168);
            this.optSpezialFormat.Name = "optSpezialFormat";
            this.optSpezialFormat.Size = new System.Drawing.Size(216, 96);
            this.optSpezialFormat.TabIndex = 88;
            this.optSpezialFormat.Text = "<b>Spezial-Dateiformat</b><br>Das Vorlagen Layout enthält einen speziellen Code, " +
    "so dass alle Einträge in eine Datei geschrieben werden";
            // 
            // optBildSchateln
            // 
            this.optBildSchateln.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Optionbox | BlueControls.Enums.enButtonStyle.Text)));
            this.optBildSchateln.ImageCode = "Diskette";
            this.optBildSchateln.Location = new System.Drawing.Point(8, 120);
            this.optBildSchateln.Name = "optBildSchateln";
            this.optBildSchateln.Size = new System.Drawing.Size(216, 40);
            this.optBildSchateln.TabIndex = 87;
            this.optBildSchateln.Text = "<b>Als Bild speichern</b><br>Einträge auf einem Bild schachteln";
            // 
            // optSpeichern
            // 
            this.optSpeichern.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Optionbox | BlueControls.Enums.enButtonStyle.Text)));
            this.optSpeichern.Checked = true;
            this.optSpeichern.ImageCode = "Diskette";
            this.optSpeichern.Location = new System.Drawing.Point(8, 24);
            this.optSpeichern.Name = "optSpeichern";
            this.optSpeichern.Size = new System.Drawing.Size(216, 40);
            this.optSpeichern.TabIndex = 86;
            this.optSpeichern.Text = "<b>Einzeln Speichern</b><br>Auf einem Datenträger schreiben";
            // 
            // optDrucken
            // 
            this.optDrucken.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Optionbox | BlueControls.Enums.enButtonStyle.Text)));
            this.optDrucken.ImageCode = "Drucker";
            this.optDrucken.Location = new System.Drawing.Point(8, 72);
            this.optDrucken.Name = "optDrucken";
            this.optDrucken.Size = new System.Drawing.Size(216, 40);
            this.optDrucken.TabIndex = 85;
            this.optDrucken.Text = "<b>Drucken</b><br>Auf einem Drucker ausgeben";
            // 
            // btnWeiter
            // 
            this.btnWeiter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWeiter.ImageCode = "Pfeil_Rechts|24";
            this.btnWeiter.Location = new System.Drawing.Point(711, 640);
            this.btnWeiter.Name = "btnWeiter";
            this.btnWeiter.Size = new System.Drawing.Size(144, 41);
            this.btnWeiter.TabIndex = 88;
            this.btnWeiter.Text = "Weiter";
            this.btnWeiter.Click += new System.EventHandler(this.WeiterAktion_Click);
            // 
            // grpEinträge
            // 
            this.grpEinträge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpEinträge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEinträge.Controls.Add(this.capAnzahlInfo);
            this.grpEinträge.Location = new System.Drawing.Point(0, 0);
            this.grpEinträge.Name = "grpEinträge";
            this.grpEinträge.Size = new System.Drawing.Size(860, 64);
            this.grpEinträge.TabIndex = 91;
            this.grpEinträge.TabStop = false;
            this.grpEinträge.Text = "Einträge";
            // 
            // tabDrucken
            // 
            this.tabDrucken.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabDrucken.Controls.Add(this.Vorschau);
            this.tabDrucken.Controls.Add(this.btnDrucken);
            this.tabDrucken.Controls.Add(this.Button_PageSetup);
            this.tabDrucken.Controls.Add(this.padPrint);
            this.tabDrucken.Location = new System.Drawing.Point(4, 25);
            this.tabDrucken.Name = "tabDrucken";
            this.tabDrucken.Size = new System.Drawing.Size(860, 687);
            this.tabDrucken.TabIndex = 4;
            this.tabDrucken.Text = "Drucken";
            // 
            // Vorschau
            // 
            this.Vorschau.ImageCode = "Datei|36|||||||||Lupe";
            this.Vorschau.Location = new System.Drawing.Point(184, 8);
            this.Vorschau.Name = "Vorschau";
            this.Vorschau.Size = new System.Drawing.Size(168, 48);
            this.Vorschau.TabIndex = 15;
            this.Vorschau.Text = "btnVorschau";
            this.Vorschau.Click += new System.EventHandler(this.Vorschau_Click);
            // 
            // btnDrucken
            // 
            this.btnDrucken.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDrucken.ImageCode = "Drucker|24";
            this.btnDrucken.Location = new System.Drawing.Point(743, 633);
            this.btnDrucken.Name = "btnDrucken";
            this.btnDrucken.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.btnDrucken.Size = new System.Drawing.Size(112, 48);
            this.btnDrucken.TabIndex = 14;
            this.btnDrucken.Text = "Drucken";
            this.btnDrucken.Click += new System.EventHandler(this.btnDrucken_Click);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ImageCode = "SeiteEinrichten|36";
            this.Button_PageSetup.Location = new System.Drawing.Point(8, 8);
            this.Button_PageSetup.Name = "Button_PageSetup";
            this.Button_PageSetup.Size = new System.Drawing.Size(168, 48);
            this.Button_PageSetup.TabIndex = 13;
            this.Button_PageSetup.Text = "Seite einrichten";
            this.Button_PageSetup.Click += new System.EventHandler(this.Button_PageSetup_Click);
            // 
            // padPrint
            // 
            this.padPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.padPrint.EditAllowed = false;
            this.padPrint.Location = new System.Drawing.Point(5, 60);
            this.padPrint.Name = "padPrint";
            this.padPrint.ShowInPrintMode = true;
            this.padPrint.Size = new System.Drawing.Size(849, 568);
            this.padPrint.TabIndex = 2;
            this.padPrint.BeginnPrint += new System.Drawing.Printing.PrintEventHandler(this.PrintPad_BeginnPrint);
            this.padPrint.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPad_PrintPage);
            // 
            // tabBildSchachteln
            // 
            this.tabBildSchachteln.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBildSchachteln.Controls.Add(this.btnEinstellung);
            this.tabBildSchachteln.Controls.Add(this.capDPI);
            this.tabBildSchachteln.Controls.Add(this.flxAbstand);
            this.tabBildSchachteln.Controls.Add(this.flxHöhe);
            this.tabBildSchachteln.Controls.Add(this.flxBreite);
            this.tabBildSchachteln.Controls.Add(this.btnSchachtelnSpeichern);
            this.tabBildSchachteln.Controls.Add(this.padSchachteln);
            this.tabBildSchachteln.Location = new System.Drawing.Point(4, 25);
            this.tabBildSchachteln.Name = "tabBildSchachteln";
            this.tabBildSchachteln.Size = new System.Drawing.Size(860, 687);
            this.tabBildSchachteln.TabIndex = 5;
            this.tabBildSchachteln.Text = "Bild Schachteln";
            // 
            // btnEinstellung
            // 
            this.btnEinstellung.Location = new System.Drawing.Point(592, 8);
            this.btnEinstellung.Name = "btnEinstellung";
            this.btnEinstellung.Size = new System.Drawing.Size(128, 24);
            this.btnEinstellung.TabIndex = 19;
            this.btnEinstellung.Text = "Einstellung laden";
            this.btnEinstellung.Click += new System.EventHandler(this.btnEinstellung_Click);
            // 
            // capDPI
            // 
            this.capDPI.CausesValidation = false;
            this.capDPI.Location = new System.Drawing.Point(440, 8);
            this.capDPI.Name = "capDPI";
            this.capDPI.Size = new System.Drawing.Size(112, 24);
            this.capDPI.Text = "DPI: 300";
            // 
            // flxAbstand
            // 
            this.flxAbstand.Caption = "Abstand:";
            this.flxAbstand.CaptionPosition = BlueBasics.Enums.enÜberschriftAnordnung.Links_neben_Dem_Feld;
            this.flxAbstand.EditType = BlueBasics.Enums.enEditTypeFormula.Textfeld;
            this.flxAbstand.FileEncryptionKey = null;
            this.flxAbstand.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
            this.flxAbstand.Location = new System.Drawing.Point(296, 8);
            this.flxAbstand.Name = "flxAbstand";
            this.flxAbstand.QuickInfo = "Abstand zwischen zwei Einträgen";
            this.flxAbstand.Size = new System.Drawing.Size(136, 24);
            this.flxAbstand.Suffix = "mm";
            this.flxAbstand.TabIndex = 18;
            this.flxAbstand.ValueChanged += new System.EventHandler(this.Attribute_Changed);
            // 
            // flxHöhe
            // 
            this.flxHöhe.Caption = "Höhe";
            this.flxHöhe.CaptionPosition = BlueBasics.Enums.enÜberschriftAnordnung.Links_neben_Dem_Feld;
            this.flxHöhe.EditType = BlueBasics.Enums.enEditTypeFormula.Textfeld;
            this.flxHöhe.FileEncryptionKey = null;
            this.flxHöhe.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
            this.flxHöhe.Location = new System.Drawing.Point(152, 8);
            this.flxHöhe.Name = "flxHöhe";
            this.flxHöhe.QuickInfo = "Höhe des endgültigen Bildes";
            this.flxHöhe.Size = new System.Drawing.Size(136, 24);
            this.flxHöhe.Suffix = "mm";
            this.flxHöhe.TabIndex = 17;
            this.flxHöhe.ValueChanged += new System.EventHandler(this.Attribute_Changed);
            // 
            // flxBreite
            // 
            this.flxBreite.Caption = "Breite:";
            this.flxBreite.CaptionPosition = BlueBasics.Enums.enÜberschriftAnordnung.Links_neben_Dem_Feld;
            this.flxBreite.EditType = BlueBasics.Enums.enEditTypeFormula.Textfeld;
            this.flxBreite.FileEncryptionKey = null;
            this.flxBreite.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
            this.flxBreite.Location = new System.Drawing.Point(8, 8);
            this.flxBreite.Name = "flxBreite";
            this.flxBreite.QuickInfo = "Breite des endgültigen Bildes";
            this.flxBreite.Size = new System.Drawing.Size(136, 24);
            this.flxBreite.Suffix = "mm";
            this.flxBreite.TabIndex = 16;
            this.flxBreite.ValueChanged += new System.EventHandler(this.Attribute_Changed);
            // 
            // btnSchachtelnSpeichern
            // 
            this.btnSchachtelnSpeichern.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSchachtelnSpeichern.ImageCode = "Diskette|24";
            this.btnSchachtelnSpeichern.Location = new System.Drawing.Point(743, 632);
            this.btnSchachtelnSpeichern.Name = "btnSchachtelnSpeichern";
            this.btnSchachtelnSpeichern.Size = new System.Drawing.Size(112, 48);
            this.btnSchachtelnSpeichern.TabIndex = 15;
            this.btnSchachtelnSpeichern.Text = "Speichern";
            this.btnSchachtelnSpeichern.Click += new System.EventHandler(this.btnSchachtelnSpeichern_Click);
            // 
            // padSchachteln
            // 
            this.padSchachteln.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.padSchachteln.EditAllowed = false;
            this.padSchachteln.Location = new System.Drawing.Point(8, 40);
            this.padSchachteln.Name = "padSchachteln";
            this.padSchachteln.ShowInPrintMode = true;
            this.padSchachteln.Size = new System.Drawing.Size(849, 584);
            this.padSchachteln.TabIndex = 3;
            // 
            // tabDateiExport
            // 
            this.tabDateiExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabDateiExport.Controls.Add(this.Caption4);
            this.tabDateiExport.Controls.Add(this.lstExported);
            this.tabDateiExport.Controls.Add(this.FrmDrucken_ExportVerzeichniss);
            this.tabDateiExport.Controls.Add(this.MachZu);
            this.tabDateiExport.Location = new System.Drawing.Point(4, 25);
            this.tabDateiExport.Name = "tabDateiExport";
            this.tabDateiExport.Size = new System.Drawing.Size(860, 687);
            this.tabDateiExport.TabIndex = 2;
            this.tabDateiExport.Text = "Datei-Export";
            // 
            // Caption4
            // 
            this.Caption4.CausesValidation = false;
            this.Caption4.Location = new System.Drawing.Point(8, 8);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new System.Drawing.Size(328, 24);
            this.Caption4.Text = "Erstellte Dateien:";
            // 
            // lstExported
            // 
            this.lstExported.AddAllowed = BlueControls.Enums.enAddType.None;
            this.lstExported.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstExported.CheckBehavior = BlueControls.Enums.enCheckBehavior.NoSelection;
            this.lstExported.FilterAllowed = true;
            this.lstExported.Location = new System.Drawing.Point(8, 40);
            this.lstExported.Name = "lstExported";
            this.lstExported.Size = new System.Drawing.Size(847, 593);
            this.lstExported.TabIndex = 88;
            this.lstExported.Text = "Exported";
            this.lstExported.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.lstExported_ContextMenuInit);
            this.lstExported.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.lstExported_ContextMenuItemClicked);
            this.lstExported.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Exported_ItemClicked);
            // 
            // ExportDialog
            // 
            this.ClientSize = new System.Drawing.Size(868, 716);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.c_Layoutx);
            this.Controls.Add(this.Caption3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "ExportDialog";
            this.ShowIcon = false;
            this.Text = "Drucken / Exportieren";
            this.Tabs.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpArt.ResumeLayout(false);
            this.grpEinträge.ResumeLayout(false);
            this.tabDrucken.ResumeLayout(false);
            this.tabBildSchachteln.ResumeLayout(false);
            this.tabDateiExport.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private ComboBox cbxLayoutWahl;
			private Caption c_Layoutx;
			private Caption capAnzahlInfo;
			private Caption Caption3;
			internal Button MachZu;
			internal Button btnLayoutEditorÖffnen;
			internal Button FrmDrucken_ExportVerzeichniss;
			internal Caption capLayout;
			internal CreativePad padVorschau;
			internal  System.Windows.Forms.TabPage tabDateiExport;
			internal  System.Windows.Forms.TabPage tabStart;
			private Button optSpeichern;
			private Button optDrucken;
			internal  System.Windows.Forms.TabPage tabDrucken;
			internal Caption Caption4;
			internal ListBox lstExported;
			internal Button btnWeiter;
			internal CreativePad padPrint;
			internal Button Button_PageSetup;
			private Button btnDrucken;
			private Button Vorschau;
        internal TabControl Tabs;
        private GroupBox grpArt;
        private Button optBildSchateln;
        private GroupBox grpEinträge;
        private Button optSpezialFormat;
        private  System.Windows.Forms.TabPage tabBildSchachteln;
        internal CreativePad padSchachteln;
        private Button btnSchachtelnSpeichern;
        private FlexiControl flxAbstand;
        private FlexiControl flxHöhe;
        private FlexiControl flxBreite;
        private Caption capDPI;
        private Button btnEinstellung;
    }
	}
