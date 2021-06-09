using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms
{

    public partial class PadEditor
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				if (disposing )
				{
				
				}
				base.Dispose(disposing);
			}


			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.btnZoomOut = new BlueControls.Controls.Button();
            this.btnZoomIn = new BlueControls.Controls.Button();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.Pad = new BlueControls.Controls.CreativePad();
            this.Ribbon = new BlueControls.Controls.RibbonBar();
            this.tabPageStart = new BlueControls.Controls.TabPage();
            this.Area_Drucken = new BlueControls.Controls.GroupBox();
            this.Vorschau = new BlueControls.Controls.Button();
            this.Button_PageSetup = new BlueControls.Controls.Button();
            this.Bild = new BlueControls.Controls.Button();
            this.Drucken = new BlueControls.Controls.Button();
            this.grpDateiSystem = new BlueControls.Controls.GroupBox();
            this.btnImport = new BlueControls.Controls.Button();
            this.btnLastFiles = new BlueControls.Controls.LastFilesCombo();
            this.btnNeu = new BlueControls.Controls.Button();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSpeichern = new BlueControls.Controls.Button();
            this.tabPageControl = new BlueControls.Controls.TabPage();
            this.grpKomponenteHinzufügen = new BlueControls.Controls.GroupBox();
            this.btnPhsyik = new BlueControls.Controls.Button();
            this.btnAddSymbol = new BlueControls.Controls.Button();
            this.btnAddUnterStufe = new BlueControls.Controls.Button();
            this.btnAddText = new BlueControls.Controls.Button();
            this.btnAddImage = new BlueControls.Controls.Button();
            this.btnAddDimension = new BlueControls.Controls.Button();
            this.btnAddLine = new BlueControls.Controls.Button();
            this.grpWerkzeuge = new BlueControls.Controls.GroupBox();
            this.btnAuswahl = new BlueControls.Controls.Button();
            this.Page_Settings = new BlueControls.Controls.TabPage();
            this.Area_Assistent = new BlueControls.Controls.GroupBox();
            this.capRasterFangen = new BlueControls.Controls.Caption();
            this.capRasterAnzeige = new BlueControls.Controls.Caption();
            this.txbRasterFangen = new BlueControls.Controls.TextBox();
            this.txbRasterAnzeige = new BlueControls.Controls.TextBox();
            this.ckbRaster = new BlueControls.Controls.Button();
            this.Area_Design = new BlueControls.Controls.GroupBox();
            this.btnKeinHintergrund = new BlueControls.Controls.Button();
            this.btnHintergrundFarbe = new BlueControls.Controls.Button();
            this.btnVorschauModus = new BlueControls.Controls.Button();
            this.ArbeitsbreichSetup = new BlueControls.Controls.Button();
            this.SchriftGröße = new BlueControls.Controls.ComboBox();
            this.sscchrifthgöße = new BlueControls.Controls.Caption();
            this.PadDesign = new BlueControls.Controls.ComboBox();
            this.ssss = new BlueControls.Controls.Caption();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.Ribbon.SuspendLayout();
            this.tabPageStart.SuspendLayout();
            this.Area_Drucken.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.tabPageControl.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.grpWerkzeuge.SuspendLayout();
            this.Page_Settings.SuspendLayout();
            this.Area_Assistent.SuspendLayout();
            this.Area_Design.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox;
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new System.Drawing.Point(120, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox;
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new System.Drawing.Point(176, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
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
            this.Pad.Location = new System.Drawing.Point(0, 110);
            this.Pad.Name = "Pad";
            this.Pad.Size = new System.Drawing.Size(1334, 571);
            this.Pad.TabIndex = 0;
            this.Pad.PreviewModeChanged += new System.EventHandler(this.Pad_PreviewModChanged);
            this.Pad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabPageStart);
            this.Ribbon.Controls.Add(this.tabPageControl);
            this.Ribbon.Controls.Add(this.Page_Settings);
            this.Ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.Location = new System.Drawing.Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new System.Drawing.Size(1334, 110);
            this.Ribbon.TabIndex = 2;
            // 
            // tabPageStart
            // 
            this.tabPageStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabPageStart.Controls.Add(this.Area_Drucken);
            this.tabPageStart.Controls.Add(this.grpDateiSystem);
            this.tabPageStart.Location = new System.Drawing.Point(4, 25);
            this.tabPageStart.Name = "tabPageStart";
            this.tabPageStart.Size = new System.Drawing.Size(1326, 81);
            this.tabPageStart.TabIndex = 1;
            this.tabPageStart.Text = "Start";
            // 
            // Area_Drucken
            // 
            this.Area_Drucken.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.Area_Drucken.CausesValidation = false;
            this.Area_Drucken.Controls.Add(this.Vorschau);
            this.Area_Drucken.Controls.Add(this.Button_PageSetup);
            this.Area_Drucken.Controls.Add(this.Bild);
            this.Area_Drucken.Controls.Add(this.Drucken);
            this.Area_Drucken.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Drucken.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.Area_Drucken.Location = new System.Drawing.Point(376, 0);
            this.Area_Drucken.Name = "Area_Drucken";
            this.Area_Drucken.Size = new System.Drawing.Size(288, 81);
            this.Area_Drucken.TabIndex = 0;
            this.Area_Drucken.TabStop = false;
            this.Area_Drucken.Text = "Drucken";
            // 
            // Vorschau
            // 
            this.Vorschau.ImageCode = "Datei||||||||||Lupe";
            this.Vorschau.Location = new System.Drawing.Point(224, 2);
            this.Vorschau.Name = "Vorschau";
            this.Vorschau.Size = new System.Drawing.Size(56, 66);
            this.Vorschau.TabIndex = 13;
            this.Vorschau.Text = "Vorschau";
            this.Vorschau.Click += new System.EventHandler(this.Vorschau_Click);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ImageCode = "SeiteEinrichten";
            this.Button_PageSetup.Location = new System.Drawing.Point(136, 2);
            this.Button_PageSetup.Name = "Button_PageSetup";
            this.Button_PageSetup.Size = new System.Drawing.Size(88, 66);
            this.Button_PageSetup.TabIndex = 12;
            this.Button_PageSetup.Text = "Drucker-Seite einreichten";
            this.Button_PageSetup.Click += new System.EventHandler(this.ButtonPageSetup_Click);
            // 
            // Bild
            // 
            this.Bild.ImageCode = "Bild";
            this.Bild.Location = new System.Drawing.Point(72, 2);
            this.Bild.Name = "Bild";
            this.Bild.Size = new System.Drawing.Size(64, 66);
            this.Bild.TabIndex = 11;
            this.Bild.Text = "Als Bild speichern";
            this.Bild.Click += new System.EventHandler(this.Bild_Click);
            // 
            // Drucken
            // 
            this.Drucken.ImageCode = "Drucker";
            this.Drucken.Location = new System.Drawing.Point(8, 2);
            this.Drucken.Name = "Drucken";
            this.Drucken.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.Drucken.Size = new System.Drawing.Size(64, 66);
            this.Drucken.TabIndex = 10;
            this.Drucken.Text = "Drucken";
            this.Drucken.Click += new System.EventHandler(this.Drucken_Click);
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
            this.grpDateiSystem.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpDateiSystem.Location = new System.Drawing.Point(0, 0);
            this.grpDateiSystem.Name = "grpDateiSystem";
            this.grpDateiSystem.Size = new System.Drawing.Size(376, 81);
            this.grpDateiSystem.TabIndex = 4;
            this.grpDateiSystem.TabStop = false;
            this.grpDateiSystem.Text = "Dateisystem";
            // 
            // btnImport
            // 
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
            this.btnLastFiles.DrawStyle = BlueControls.Enums.enComboboxStyle.RibbonBar;
            this.btnLastFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLastFiles.Enabled = false;
            this.btnLastFiles.ImageCode = "Ordner";
            this.btnLastFiles.Location = new System.Drawing.Point(136, 2);
            this.btnLastFiles.Name = "btnLastFiles";
            this.btnLastFiles.Size = new System.Drawing.Size(104, 66);
            this.btnLastFiles.TabIndex = 11;
            this.btnLastFiles.Text = "zuletzt geöffnete Dateien";
            this.btnLastFiles.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.btnLastFiles_ItemClicked);
            // 
            // btnNeu
            // 
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
            this.btnSpeichern.ImageCode = "Diskette";
            this.btnSpeichern.Location = new System.Drawing.Point(304, 2);
            this.btnSpeichern.Name = "btnSpeichern";
            this.btnSpeichern.Size = new System.Drawing.Size(64, 66);
            this.btnSpeichern.TabIndex = 8;
            this.btnSpeichern.Text = "Speichern unter";
            this.btnSpeichern.Click += new System.EventHandler(this.btnSpeichern_Click);
            // 
            // tabPageControl
            // 
            this.tabPageControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabPageControl.Controls.Add(this.grpKomponenteHinzufügen);
            this.tabPageControl.Controls.Add(this.grpWerkzeuge);
            this.tabPageControl.Location = new System.Drawing.Point(4, 25);
            this.tabPageControl.Name = "tabPageControl";
            this.tabPageControl.Size = new System.Drawing.Size(1326, 81);
            this.tabPageControl.TabIndex = 0;
            this.tabPageControl.Text = "Steuerung";
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpKomponenteHinzufügen.CausesValidation = false;
            this.grpKomponenteHinzufügen.Controls.Add(this.btnPhsyik);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddSymbol);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddUnterStufe);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddText);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddImage);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddDimension);
            this.grpKomponenteHinzufügen.Controls.Add(this.btnAddLine);
            this.grpKomponenteHinzufügen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpKomponenteHinzufügen.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpKomponenteHinzufügen.Location = new System.Drawing.Point(240, 0);
            this.grpKomponenteHinzufügen.Name = "grpKomponenteHinzufügen";
            this.grpKomponenteHinzufügen.Size = new System.Drawing.Size(264, 81);
            this.grpKomponenteHinzufügen.TabIndex = 0;
            this.grpKomponenteHinzufügen.TabStop = false;
            this.grpKomponenteHinzufügen.Text = "Komponente hinzufügen";
            // 
            // btnPhsyik
            // 
            this.btnPhsyik.ImageCode = "Formel|16";
            this.btnPhsyik.Location = new System.Drawing.Point(168, 24);
            this.btnPhsyik.Name = "btnPhsyik";
            this.btnPhsyik.Size = new System.Drawing.Size(88, 22);
            this.btnPhsyik.TabIndex = 10;
            this.btnPhsyik.Text = "Physik";
            this.btnPhsyik.Click += new System.EventHandler(this.btnPhsyik_Click);
            // 
            // btnAddSymbol
            // 
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
            this.btnAddUnterStufe.ImageCode = "Datei|16";
            this.btnAddUnterStufe.Location = new System.Drawing.Point(168, 2);
            this.btnAddUnterStufe.Name = "btnAddUnterStufe";
            this.btnAddUnterStufe.Size = new System.Drawing.Size(88, 22);
            this.btnAddUnterStufe.TabIndex = 8;
            this.btnAddUnterStufe.Text = "Unterstufe";
            this.btnAddUnterStufe.Click += new System.EventHandler(this.btnAddUnterStufe_Click);
            // 
            // btnAddText
            // 
            this.btnAddText.ImageCode = "Textfeld|16";
            this.btnAddText.Location = new System.Drawing.Point(8, 2);
            this.btnAddText.Name = "btnAddText";
            this.btnAddText.Size = new System.Drawing.Size(80, 22);
            this.btnAddText.TabIndex = 4;
            this.btnAddText.Text = "Text";
            this.btnAddText.Click += new System.EventHandler(this.AddText_Click);
            // 
            // btnAddImage
            // 
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
            this.btnAddLine.ImageCode = "Linie|16";
            this.btnAddLine.Location = new System.Drawing.Point(96, 24);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(72, 22);
            this.btnAddLine.TabIndex = 7;
            this.btnAddLine.Text = "Linie";
            this.btnAddLine.Click += new System.EventHandler(this.btnAddLine_Click);
            // 
            // grpWerkzeuge
            // 
            this.grpWerkzeuge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpWerkzeuge.CausesValidation = false;
            this.grpWerkzeuge.Controls.Add(this.btnAuswahl);
            this.grpWerkzeuge.Controls.Add(this.btnZoomFit);
            this.grpWerkzeuge.Controls.Add(this.btnZoomOut);
            this.grpWerkzeuge.Controls.Add(this.btnZoomIn);
            this.grpWerkzeuge.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpWerkzeuge.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpWerkzeuge.Location = new System.Drawing.Point(0, 0);
            this.grpWerkzeuge.Name = "grpWerkzeuge";
            this.grpWerkzeuge.Size = new System.Drawing.Size(240, 81);
            this.grpWerkzeuge.TabIndex = 1;
            this.grpWerkzeuge.TabStop = false;
            this.grpWerkzeuge.Text = "Werkzeuge";
            // 
            // btnAuswahl
            // 
            this.btnAuswahl.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox;
            this.btnAuswahl.Checked = true;
            this.btnAuswahl.ImageCode = "Mauspfeil";
            this.btnAuswahl.Location = new System.Drawing.Point(64, 2);
            this.btnAuswahl.Name = "btnAuswahl";
            this.btnAuswahl.Size = new System.Drawing.Size(56, 66);
            this.btnAuswahl.TabIndex = 3;
            this.btnAuswahl.Text = "wählen";
            // 
            // Page_Settings
            // 
            this.Page_Settings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.Page_Settings.Controls.Add(this.Area_Assistent);
            this.Page_Settings.Controls.Add(this.Area_Design);
            this.Page_Settings.Location = new System.Drawing.Point(4, 25);
            this.Page_Settings.Name = "Page_Settings";
            this.Page_Settings.Size = new System.Drawing.Size(1326, 81);
            this.Page_Settings.TabIndex = 2;
            this.Page_Settings.Text = "Einstellungen";
            // 
            // Area_Assistent
            // 
            this.Area_Assistent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.Area_Assistent.CausesValidation = false;
            this.Area_Assistent.Controls.Add(this.capRasterFangen);
            this.Area_Assistent.Controls.Add(this.capRasterAnzeige);
            this.Area_Assistent.Controls.Add(this.txbRasterFangen);
            this.Area_Assistent.Controls.Add(this.txbRasterAnzeige);
            this.Area_Assistent.Controls.Add(this.ckbRaster);
            this.Area_Assistent.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Assistent.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.Area_Assistent.Location = new System.Drawing.Point(648, 0);
            this.Area_Assistent.Name = "Area_Assistent";
            this.Area_Assistent.Size = new System.Drawing.Size(152, 81);
            this.Area_Assistent.TabIndex = 0;
            this.Area_Assistent.TabStop = false;
            this.Area_Assistent.Text = "Assistenten";
            // 
            // capRasterFangen
            // 
            this.capRasterFangen.CausesValidation = false;
            this.capRasterFangen.Location = new System.Drawing.Point(8, 46);
            this.capRasterFangen.Name = "capRasterFangen";
            this.capRasterFangen.Size = new System.Drawing.Size(56, 22);
            this.capRasterFangen.Text = "Fangen:";
            // 
            // capRasterAnzeige
            // 
            this.capRasterAnzeige.CausesValidation = false;
            this.capRasterAnzeige.Location = new System.Drawing.Point(8, 24);
            this.capRasterAnzeige.Name = "capRasterAnzeige";
            this.capRasterAnzeige.Size = new System.Drawing.Size(56, 22);
            this.capRasterAnzeige.Text = "Anzeige:";
            // 
            // txbRasterFangen
            // 
            this.txbRasterFangen.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRasterFangen.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
            this.txbRasterFangen.Location = new System.Drawing.Point(72, 46);
            this.txbRasterFangen.Name = "txbRasterFangen";
            this.txbRasterFangen.Size = new System.Drawing.Size(64, 22);
            this.txbRasterFangen.Suffix = "mm";
            this.txbRasterFangen.TabIndex = 6;
            this.txbRasterFangen.Text = "10";
            this.txbRasterFangen.TextChanged += new System.EventHandler(this.RasterFangen_TextChanged);
            // 
            // txbRasterAnzeige
            // 
            this.txbRasterAnzeige.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRasterAnzeige.Format = BlueBasics.Enums.enDataFormat.Gleitkommazahl;
            this.txbRasterAnzeige.Location = new System.Drawing.Point(72, 24);
            this.txbRasterAnzeige.Name = "txbRasterAnzeige";
            this.txbRasterAnzeige.Size = new System.Drawing.Size(64, 22);
            this.txbRasterAnzeige.Suffix = "mm";
            this.txbRasterAnzeige.TabIndex = 5;
            this.txbRasterAnzeige.Text = "10";
            this.txbRasterAnzeige.TextChanged += new System.EventHandler(this.txbRasterAnzeige_TextChanged);
            // 
            // ckbRaster
            // 
            this.ckbRaster.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox;
            this.ckbRaster.ImageCode = "Raster|18";
            this.ckbRaster.Location = new System.Drawing.Point(8, 2);
            this.ckbRaster.Name = "ckbRaster";
            this.ckbRaster.Size = new System.Drawing.Size(112, 22);
            this.ckbRaster.TabIndex = 4;
            this.ckbRaster.Text = "Raster";
            this.ckbRaster.CheckedChanged += new System.EventHandler(this.ckbRaster_CheckedChanged);
            // 
            // Area_Design
            // 
            this.Area_Design.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.Area_Design.CausesValidation = false;
            this.Area_Design.Controls.Add(this.btnKeinHintergrund);
            this.Area_Design.Controls.Add(this.btnHintergrundFarbe);
            this.Area_Design.Controls.Add(this.btnVorschauModus);
            this.Area_Design.Controls.Add(this.ArbeitsbreichSetup);
            this.Area_Design.Controls.Add(this.SchriftGröße);
            this.Area_Design.Controls.Add(this.sscchrifthgöße);
            this.Area_Design.Controls.Add(this.PadDesign);
            this.Area_Design.Controls.Add(this.ssss);
            this.Area_Design.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Design.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.Area_Design.Location = new System.Drawing.Point(0, 0);
            this.Area_Design.Name = "Area_Design";
            this.Area_Design.Size = new System.Drawing.Size(648, 81);
            this.Area_Design.TabIndex = 1;
            this.Area_Design.TabStop = false;
            this.Area_Design.Text = "Design";
            // 
            // btnKeinHintergrund
            // 
            this.btnKeinHintergrund.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.btnHintergrundFarbe.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.btnHintergrundFarbe.ImageCode = "Farben";
            this.btnHintergrundFarbe.Location = new System.Drawing.Point(376, 2);
            this.btnHintergrundFarbe.Name = "btnHintergrundFarbe";
            this.btnHintergrundFarbe.Size = new System.Drawing.Size(80, 66);
            this.btnHintergrundFarbe.TabIndex = 15;
            this.btnHintergrundFarbe.Text = "Hintergrund-Farbe";
            this.btnHintergrundFarbe.Click += new System.EventHandler(this.btnHintergrundFarbe_Click);
            // 
            // btnVorschauModus
            // 
            this.btnVorschauModus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVorschauModus.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_RibbonBar;
            this.btnVorschauModus.ImageCode = "Textdatei";
            this.btnVorschauModus.Location = new System.Drawing.Point(584, 2);
            this.btnVorschauModus.Name = "btnVorschauModus";
            this.btnVorschauModus.Size = new System.Drawing.Size(56, 66);
            this.btnVorschauModus.TabIndex = 14;
            this.btnVorschauModus.Text = "Vorschau-Modus";
            this.btnVorschauModus.CheckedChanged += new System.EventHandler(this.btnVorschauModus_CheckedChanged);
            // 
            // ArbeitsbreichSetup
            // 
            this.ArbeitsbreichSetup.ImageCode = "SeiteEinrichten";
            this.ArbeitsbreichSetup.Location = new System.Drawing.Point(8, 2);
            this.ArbeitsbreichSetup.Name = "ArbeitsbreichSetup";
            this.ArbeitsbreichSetup.Size = new System.Drawing.Size(96, 66);
            this.ArbeitsbreichSetup.TabIndex = 13;
            this.ArbeitsbreichSetup.Text = "Arbeitsbereich einreichten";
            this.ArbeitsbreichSetup.Click += new System.EventHandler(this.ArbeitsbreichSetup_Click);
            // 
            // SchriftGröße
            // 
            this.SchriftGröße.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SchriftGröße.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SchriftGröße.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SchriftGröße.Location = new System.Drawing.Point(208, 46);
            this.SchriftGröße.Name = "SchriftGröße";
            this.SchriftGröße.Size = new System.Drawing.Size(168, 22);
            this.SchriftGröße.TabIndex = 3;
            this.SchriftGröße.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.SchriftGröße_ItemClicked);
            // 
            // sscchrifthgöße
            // 
            this.sscchrifthgöße.CausesValidation = false;
            this.sscchrifthgöße.Location = new System.Drawing.Point(112, 46);
            this.sscchrifthgöße.Name = "sscchrifthgöße";
            this.sscchrifthgöße.Size = new System.Drawing.Size(88, 22);
            this.sscchrifthgöße.Text = "Schrift-Größe:";
            // 
            // PadDesign
            // 
            this.PadDesign.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PadDesign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.PadDesign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PadDesign.Location = new System.Drawing.Point(112, 24);
            this.PadDesign.Name = "PadDesign";
            this.PadDesign.Size = new System.Drawing.Size(264, 22);
            this.PadDesign.TabIndex = 1;
            this.PadDesign.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.PadDesign_ItemClicked);
            // 
            // ssss
            // 
            this.ssss.CausesValidation = false;
            this.ssss.Location = new System.Drawing.Point(112, 2);
            this.ssss.Name = "ssss";
            this.ssss.Size = new System.Drawing.Size(77, 22);
            this.ssss.Text = "Design:";
            this.ssss.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
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
            // PadEditor
            // 
            this.ClientSize = new System.Drawing.Size(1334, 681);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.Ribbon);
            this.Name = "PadEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.TopMost = true;
            this.Ribbon.ResumeLayout(false);
            this.tabPageStart.ResumeLayout(false);
            this.Area_Drucken.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.tabPageControl.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
            this.grpWerkzeuge.ResumeLayout(false);
            this.Page_Settings.ResumeLayout(false);
            this.Area_Assistent.ResumeLayout(false);
            this.Area_Design.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button btnZoomOut;
			private Button btnAuswahl;
			private Button btnAddText;
			private Button btnAddImage;
			private Button btnAddDimension;
			private Button btnAddLine;
			public CreativePad Pad;
			protected RibbonBar Ribbon;
			protected TabPage tabPageControl;
			protected TabPage tabPageStart;
			protected GroupBox grpWerkzeuge;
			protected GroupBox grpKomponenteHinzufügen;
			protected GroupBox Area_Drucken;
			protected GroupBox Area_Design;
			private Caption ssss;
			protected GroupBox Area_Assistent;
			private Button btnZoomFit;
			private Button btnZoomIn;
			private Button Drucken;
			private Button Bild;
			private Button Vorschau;
			private ComboBox PadDesign;
            protected Button ckbRaster;
            private Caption capRasterFangen;
			private Caption capRasterAnzeige;
			private TextBox txbRasterFangen;
			private TextBox txbRasterAnzeige;
			internal ComboBox SchriftGröße;
			internal Caption sscchrifthgöße;
			protected internal Button Button_PageSetup;
			protected internal Button ArbeitsbreichSetup;


            private TabPage Page_Settings;
        private Button btnAddUnterStufe;
        private Button btnAddSymbol;
        private Button btnPhsyik;
        private Button btnVorschauModus;
        private System.Windows.Forms.OpenFileDialog LoadTab;
        private System.Windows.Forms.SaveFileDialog SaveTab;
        protected GroupBox grpDateiSystem;
        protected LastFilesCombo btnLastFiles;
        protected Button btnNeu;
        protected Button btnOeffnen;
        protected Button btnSpeichern;
        private Button btnHintergrundFarbe;
        private System.Windows.Forms.ColorDialog ColorDia;
        private Button btnKeinHintergrund;
        protected Button btnImport;
    }
	}