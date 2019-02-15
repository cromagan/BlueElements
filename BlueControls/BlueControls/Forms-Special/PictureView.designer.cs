using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Forms
    {

        public partial class PictureView : Form
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
            this.Area_Seiten = new GroupBox();
            this.Rechts = new Button();
            this.Links = new Button();
            this.InfoText = new Caption();
            this.ZoomOut = new Button();
            this.ZoomIn = new Button();
            this.ZoomFitBut = new Button();
            this.Pad = new CreativePad();
            this.Ribbon = new TabControl();
            this.Page_Start = new TabPage();
            this.Area_Drucken = new GroupBox();
            this.Vorschau = new Button();
            this.Button_PageSetup = new Button();
            this.Bild = new Button();
            this.Drucken = new Button();
            this.Area_Dateisystem = new GroupBox();
            this.LastBCRFiles = new LastFilesCombo();
            this.DelAll = new Button();
            this.LoadDisk = new Button();
            this.SaveDisk = new Button();
            this.Page_Control = new TabPage();
            this.Area_KomponenteHinzufügen = new GroupBox();
            this.AddText = new Button();
            this.AddImage = new Button();
            this.AddDistance = new Button();
            this.AddDimension = new Button();
            this.AddLine = new Button();
            this.Area_Werkzeuge = new GroupBox();
            this.Auswahl = new Button();
            this.Page_Settings = new TabPage();
            this.Area_Assistent = new GroupBox();
            this.Bez_None = new Button();
            this.Bez_Direkt = new Button();
            this.RasterFangenCap = new Caption();
            this.cappi1 = new Caption();
            this.RasterFangen = new TextBox();
            this.RasterAnzeige = new TextBox();
            this.Raster = new Button();
            this.Bez_All = new Button();
            this.Area_Design = new GroupBox();
            this.ArbeitsbreichSetup = new Button();
            this.SchriftGröße = new ComboBox();
            this.sscchrifthgöße = new Caption();
            this.PadDesign = new ComboBox();
            this.ssss = new Caption();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.Area_Seiten.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.Page_Start.SuspendLayout();
            this.Area_Drucken.SuspendLayout();
            this.Area_Dateisystem.SuspendLayout();
            this.Page_Control.SuspendLayout();
            this.Area_KomponenteHinzufügen.SuspendLayout();
            this.Area_Werkzeuge.SuspendLayout();
            this.Page_Settings.SuspendLayout();
            this.Area_Assistent.SuspendLayout();
            this.Area_Design.SuspendLayout();
            this.SuspendLayout();
            // 
            // Area_Seiten
            // 
            this.Area_Seiten.BackColor = Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.Area_Seiten.CausesValidation = false;
            this.Area_Seiten.Controls.Add(this.Rechts);
            this.Area_Seiten.Controls.Add(this.Links);
            this.Area_Seiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Seiten.Location = new Point(0, 0);
            this.Area_Seiten.Name = "Area_Seiten";
            this.Area_Seiten.Size = new Size(112, 81);
            this.Area_Seiten.Text = "Seiten";
            // 
            // Rechts
            // 
            this.Rechts.ImageCode = "Pfeil_Rechts";
            this.Rechts.Location = new Point(56, 2);
            this.Rechts.Name = "Rechts";
            this.Rechts.Size = new Size(48, 66);
            this.Rechts.TabIndex = 6;
            this.Rechts.Text = "vor";
            this.Rechts.Click += new EventHandler(this.Rechts_Click);
            // 
            // Links
            // 
            this.Links.ImageCode = "Pfeil_Links";
            this.Links.Location = new Point(7, 2);
            this.Links.Name = "Links";
            this.Links.Size = new Size(49, 66);
            this.Links.TabIndex = 5;
            this.Links.Text = "zurück";
            this.Links.Click += new EventHandler(this.Links_Click);
            // 
            // InfoText
            // 
            this.InfoText.CausesValidation = false;
            this.InfoText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoText.Location = new Point(528, 0);
            this.InfoText.Margin = new System.Windows.Forms.Padding(10);
            this.InfoText.Name = "InfoText";
            this.InfoText.Padding = new System.Windows.Forms.Padding(10);
            this.InfoText.Size = new Size(798, 81);
            this.InfoText.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // ZoomOut
            // 
            this.ZoomOut.ButtonStyle = enButtonStyle.Optionbox;
            this.ZoomOut.ImageCode = "LupeMinus";
            this.ZoomOut.Location = new Point(120, 2);
            this.ZoomOut.Name = "ZoomOut";
            this.ZoomOut.Size = new Size(56, 66);
            this.ZoomOut.TabIndex = 2;
            this.ZoomOut.Text = "kleiner";
            // 
            // ZoomIn
            // 
            this.ZoomIn.ButtonStyle = enButtonStyle.Optionbox;
            this.ZoomIn.ImageCode = "LupePlus";
            this.ZoomIn.Location = new Point(176, 2);
            this.ZoomIn.Name = "ZoomIn";
            this.ZoomIn.Size = new Size(56, 66);
            this.ZoomIn.TabIndex = 1;
            this.ZoomIn.Text = "größer";
            // 
            // ZoomFitBut
            // 
            this.ZoomFitBut.ImageCode = "ZoomFit";
            this.ZoomFitBut.Location = new Point(8, 2);
            this.ZoomFitBut.Name = "ZoomFitBut";
            this.ZoomFitBut.Size = new Size(48, 66);
            this.ZoomFitBut.TabIndex = 0;
            this.ZoomFitBut.Text = "ein-passen";
            this.ZoomFitBut.Click += new EventHandler(this.ZoomFitBut_Click);
            // 
            // Pad
            // 
            this.Pad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pad.GridShow = 10F;
            this.Pad.GridSnap = 1F;
            this.Pad.Location = new Point(0, 110);
            this.Pad.Name = "Pad";
            this.Pad.RandinMM = new System.Windows.Forms.Padding(0);
            this.Pad.SheetSizeInMM = new SizeF(0F, 0F);
            this.Pad.SheetStyle = "Lysithea";
            this.Pad.SheetStyleScale = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.Pad.Size = new Size(1334, 571);
            this.Pad.TabIndex = 0;
            this.Pad.Parsed += new  EventHandler(this.Pad_Parsed);
            this.Pad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.Page_Start);
            this.Ribbon.Controls.Add(this.Page_Control);
            this.Ribbon.Controls.Add(this.Page_Settings);
            this.Ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.IsRibbonBar = true;
            this.Ribbon.Location = new Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.SelectedIndex = 1;
            this.Ribbon.Size = new Size(1334, 110);
            this.Ribbon.TabIndex = 2;
            // 
            // Page_Start
            // 
            this.Page_Start.Controls.Add(this.Area_Drucken);
            this.Page_Start.Controls.Add(this.Area_Dateisystem);
            this.Page_Start.Location = new Point(4, 25);
            this.Page_Start.Name = "Page_Start";
            this.Page_Start.Size = new Size(1326, 81);
            this.Page_Start.TabIndex = 1;
            this.Page_Start.Text = "Start";
            // 
            // Area_Drucken
            // 
            this.Area_Drucken.CausesValidation = false;
            this.Area_Drucken.Controls.Add(this.Vorschau);
            this.Area_Drucken.Controls.Add(this.Button_PageSetup);
            this.Area_Drucken.Controls.Add(this.Bild);
            this.Area_Drucken.Controls.Add(this.Drucken);
            this.Area_Drucken.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Drucken.Location = new Point(320, 0);
            this.Area_Drucken.Name = "Area_Drucken";
            this.Area_Drucken.Size = new Size(280, 81);
            this.Area_Drucken.Text = "Drucken";
            // 
            // Vorschau
            // 
            this.Vorschau.ImageCode = "Datei||||||||||Lupe";
            this.Vorschau.Location = new Point(216, 2);
            this.Vorschau.Name = "Vorschau";
            this.Vorschau.Size = new Size(56, 66);
            this.Vorschau.TabIndex = 13;
            this.Vorschau.Text = "Vorschau";
            this.Vorschau.Click += new EventHandler(this.Vorschau_Click);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ImageCode = "SeiteEinrichten";
            this.Button_PageSetup.Location = new Point(144, 2);
            this.Button_PageSetup.Name = "Button_PageSetup";
            this.Button_PageSetup.Size = new Size(72, 66);
            this.Button_PageSetup.TabIndex = 12;
            this.Button_PageSetup.Text = "Drucker einreichten";
            this.Button_PageSetup.Click += new EventHandler(this.ButtonPageSetup_Click);
            // 
            // Bild
            // 
            this.Bild.ImageCode = "Bild";
            this.Bild.Location = new Point(76, 2);
            this.Bild.Name = "Bild";
            this.Bild.Size = new Size(64, 66);
            this.Bild.TabIndex = 11;
            this.Bild.Text = "Als Bild speichern";
            this.Bild.Click += new EventHandler(this.Bild_Click);
            // 
            // Drucken
            // 
            this.Drucken.ImageCode = "Drucker";
            this.Drucken.Location = new Point(8, 2);
            this.Drucken.Name = "Drucken";
            this.Drucken.QuickInfo = "Öffnet den Drucker-Dialog.";
            this.Drucken.Size = new Size(64, 66);
            this.Drucken.TabIndex = 10;
            this.Drucken.Text = "Drucken";
            this.Drucken.Click += new EventHandler(this.Drucken_Click);
            // 
            // Area_Dateisystem
            // 
            this.Area_Dateisystem.CausesValidation = false;
            this.Area_Dateisystem.Controls.Add(this.LastBCRFiles);
            this.Area_Dateisystem.Controls.Add(this.DelAll);
            this.Area_Dateisystem.Controls.Add(this.LoadDisk);
            this.Area_Dateisystem.Controls.Add(this.SaveDisk);
            this.Area_Dateisystem.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Dateisystem.Location = new Point(0, 0);
            this.Area_Dateisystem.Name = "Area_Dateisystem";
            this.Area_Dateisystem.Size = new Size(320, 81);
            this.Area_Dateisystem.Text = "Dateisystem";
            // 
            // LastBCRFiles
            // 
            this.LastBCRFiles.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.LastBCRFiles.DrawStyle = enComboboxStyle.RibbonBar;
            this.LastBCRFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LastBCRFiles.Enabled = false;
            this.LastBCRFiles.ImageCode = "Ordner";
            this.LastBCRFiles.Location = new Point(144, 2);
            this.LastBCRFiles.Name = "LastBCRFiles";
            this.LastBCRFiles.Size = new Size(104, 66);
            this.LastBCRFiles.TabIndex = 11;
            this.LastBCRFiles.Text = "zuletzt geöffnete Dateien";
            this.LastBCRFiles.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.LastBCRFiles_Item_Click);
            // 
            // DelAll
            // 
            this.DelAll.ImageCode = "Datei";
            this.DelAll.Location = new Point(8, 2);
            this.DelAll.Name = "DelAll";
            this.DelAll.Size = new Size(64, 66);
            this.DelAll.TabIndex = 10;
            this.DelAll.Text = "Neu";
            this.DelAll.Click += new EventHandler(this.DelAll_Click);
            // 
            // LoadDisk
            // 
            this.LoadDisk.ImageCode = "Ordner";
            this.LoadDisk.Location = new Point(80, 2);
            this.LoadDisk.Name = "LoadDisk";
            this.LoadDisk.QuickInfo = "Eine Datei von ihrem<br>Computer öffnen";
            this.LoadDisk.Size = new Size(64, 66);
            this.LoadDisk.TabIndex = 9;
            this.LoadDisk.Text = "Öffnen";
            this.LoadDisk.Click += new EventHandler(this.LoadDisk_Click);
            // 
            // SaveDisk
            // 
            this.SaveDisk.ImageCode = "Diskette";
            this.SaveDisk.Location = new Point(248, 2);
            this.SaveDisk.Name = "SaveDisk";
            this.SaveDisk.Size = new Size(64, 66);
            this.SaveDisk.TabIndex = 8;
            this.SaveDisk.Text = "Speichern";
            this.SaveDisk.Click += new EventHandler(this.SaveDisk_Click);
            // 
            // Page_Control
            // 
            this.Page_Control.Controls.Add(this.InfoText);
            this.Page_Control.Controls.Add(this.Area_KomponenteHinzufügen);
            this.Page_Control.Controls.Add(this.Area_Werkzeuge);
            this.Page_Control.Controls.Add(this.Area_Seiten);
            this.Page_Control.Location = new Point(4, 25);
            this.Page_Control.Name = "Page_Control";
            this.Page_Control.Size = new Size(1326, 81);
            this.Page_Control.TabIndex = 0;
            this.Page_Control.Text = "Steuerung";
            // 
            // Area_KomponenteHinzufügen
            // 
            this.Area_KomponenteHinzufügen.CausesValidation = false;
            this.Area_KomponenteHinzufügen.Controls.Add(this.AddText);
            this.Area_KomponenteHinzufügen.Controls.Add(this.AddImage);
            this.Area_KomponenteHinzufügen.Controls.Add(this.AddDistance);
            this.Area_KomponenteHinzufügen.Controls.Add(this.AddDimension);
            this.Area_KomponenteHinzufügen.Controls.Add(this.AddLine);
            this.Area_KomponenteHinzufügen.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_KomponenteHinzufügen.Location = new Point(352, 0);
            this.Area_KomponenteHinzufügen.Name = "Area_KomponenteHinzufügen";
            this.Area_KomponenteHinzufügen.Size = new Size(176, 81);
            this.Area_KomponenteHinzufügen.Text = "Komponente hinzufügen";
            // 
            // AddText
            // 
            this.AddText.ImageCode = "Textfeld|16";
            this.AddText.Location = new Point(8, 2);
            this.AddText.Name = "AddText";
            this.AddText.Size = new Size(80, 22);
            this.AddText.TabIndex = 4;
            this.AddText.Text = "Text";
            this.AddText.Click += new EventHandler(this.AddText_Click);
            // 
            // AddImage
            // 
            this.AddImage.ImageCode = "Bild|16";
            this.AddImage.Location = new Point(8, 24);
            this.AddImage.Name = "AddImage";
            this.AddImage.Size = new Size(80, 22);
            this.AddImage.TabIndex = 2;
            this.AddImage.Text = "Bild";
            this.AddImage.Click += new EventHandler(this.AddImage_Click);
            // 
            // AddDistance
            // 
            this.AddDistance.ImageCode = "Kreis2|16|||||124|0";
            this.AddDistance.Location = new Point(96, 46);
            this.AddDistance.Name = "AddDistance";
            this.AddDistance.Size = new Size(72, 22);
            this.AddDistance.TabIndex = 5;
            this.AddDistance.Text = "Distanz";
            this.AddDistance.Click += new EventHandler(this.AddDistance_Click);
            // 
            // AddDimension
            // 
            this.AddDimension.ImageCode = "Bemaßung|16";
            this.AddDimension.Location = new Point(96, 2);
            this.AddDimension.Name = "AddDimension";
            this.AddDimension.Size = new Size(72, 22);
            this.AddDimension.TabIndex = 6;
            this.AddDimension.Text = "Maß";
            this.AddDimension.Click += new EventHandler(this.AddDimension_Click);
            // 
            // AddLine
            // 
            this.AddLine.ImageCode = "Linie|16";
            this.AddLine.Location = new Point(96, 24);
            this.AddLine.Name = "AddLine";
            this.AddLine.Size = new Size(72, 22);
            this.AddLine.TabIndex = 7;
            this.AddLine.Text = "Linie";
            this.AddLine.Click += new EventHandler(this.AddLine_Click);
            // 
            // Area_Werkzeuge
            // 
            this.Area_Werkzeuge.CausesValidation = false;
            this.Area_Werkzeuge.Controls.Add(this.Auswahl);
            this.Area_Werkzeuge.Controls.Add(this.ZoomFitBut);
            this.Area_Werkzeuge.Controls.Add(this.ZoomOut);
            this.Area_Werkzeuge.Controls.Add(this.ZoomIn);
            this.Area_Werkzeuge.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Werkzeuge.Location = new Point(112, 0);
            this.Area_Werkzeuge.Name = "Area_Werkzeuge";
            this.Area_Werkzeuge.Size = new Size(240, 81);
            this.Area_Werkzeuge.Text = "Werkzeuge";
            // 
            // Auswahl
            // 
            this.Auswahl.ButtonStyle = enButtonStyle.Optionbox;
            this.Auswahl.Checked = true;
            this.Auswahl.ImageCode = "Mauspfeil";
            this.Auswahl.Location = new Point(64, 2);
            this.Auswahl.Name = "Auswahl";
            this.Auswahl.Size = new Size(56, 66);
            this.Auswahl.TabIndex = 3;
            this.Auswahl.Text = "wählen";
            // 
            // Page_Settings
            // 
            this.Page_Settings.Controls.Add(this.Area_Assistent);
            this.Page_Settings.Controls.Add(this.Area_Design);
            this.Page_Settings.Location = new Point(4, 25);
            this.Page_Settings.Name = "Page_Settings";
            this.Page_Settings.Size = new Size(1326, 81);
            this.Page_Settings.TabIndex = 2;
            this.Page_Settings.Text = "Einstellungen";
            // 
            // Area_Assistent
            // 
            this.Area_Assistent.CausesValidation = false;
            this.Area_Assistent.Controls.Add(this.Bez_None);
            this.Area_Assistent.Controls.Add(this.Bez_Direkt);
            this.Area_Assistent.Controls.Add(this.RasterFangenCap);
            this.Area_Assistent.Controls.Add(this.cappi1);
            this.Area_Assistent.Controls.Add(this.RasterFangen);
            this.Area_Assistent.Controls.Add(this.RasterAnzeige);
            this.Area_Assistent.Controls.Add(this.Raster);
            this.Area_Assistent.Controls.Add(this.Bez_All);
            this.Area_Assistent.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Assistent.Location = new Point(288, 0);
            this.Area_Assistent.Name = "Area_Assistent";
            this.Area_Assistent.Size = new Size(426, 81);
            this.Area_Assistent.Text = "Assistenten";
            // 
            // Bez_None
            // 
            this.Bez_None.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.Bez_None.Location = new Point(152, 46);
            this.Bez_None.Name = "Bez_None";
            this.Bez_None.Size = new Size(256, 22);
            this.Bez_None.TabIndex = 10;
            this.Bez_None.Text = "Automatische Beziehungen deaktivieren";
            this.Bez_None.CheckedChanged += new  EventHandler(this.BezMode_CheckedChanged);
            // 
            // Bez_Direkt
            // 
            this.Bez_Direkt.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.Bez_Direkt.Checked = true;
            this.Bez_Direkt.Location = new Point(152, 24);
            this.Bez_Direkt.Name = "Bez_Direkt";
            this.Bez_Direkt.Size = new Size(256, 22);
            this.Bez_Direkt.TabIndex = 9;
            this.Bez_Direkt.Text = "Nur Direktverbindungen erstellen";
            this.Bez_Direkt.CheckedChanged += new  EventHandler(this.BezMode_CheckedChanged);
            // 
            // RasterFangenCap
            // 
            this.RasterFangenCap.CausesValidation = false;
            this.RasterFangenCap.Location = new Point(8, 46);
            this.RasterFangenCap.Name = "RasterFangenCap";
            this.RasterFangenCap.Size = new Size(56, 22);
            this.RasterFangenCap.Text = "Fangen:";
            // 
            // cappi1
            // 
            this.cappi1.CausesValidation = false;
            this.cappi1.Location = new Point(8, 24);
            this.cappi1.Name = "cappi1";
            this.cappi1.Size = new Size(56, 22);
            this.cappi1.Text = "Anzeige:";
            // 
            // RasterFangen
            // 
            this.RasterFangen.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RasterFangen.Format = enDataFormat.Gleitkommazahl;
            this.RasterFangen.Location = new Point(72, 46);
            this.RasterFangen.Name = "RasterFangen";
            this.RasterFangen.Size = new Size(64, 22);
            this.RasterFangen.Suffix = "mm";
            this.RasterFangen.TabIndex = 6;
            this.RasterFangen.Text = "10";
            this.RasterFangen.TextChanged += new EventHandler(this.RasterFangen_TextChanged);
            // 
            // RasterAnzeige
            // 
            this.RasterAnzeige.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RasterAnzeige.Format = enDataFormat.Gleitkommazahl;
            this.RasterAnzeige.Location = new Point(72, 24);
            this.RasterAnzeige.Name = "RasterAnzeige";
            this.RasterAnzeige.Size = new Size(64, 22);
            this.RasterAnzeige.Suffix = "mm";
            this.RasterAnzeige.TabIndex = 5;
            this.RasterAnzeige.Text = "10";
            this.RasterAnzeige.TextChanged += new EventHandler(this.RasterAnzeige_TextChanged);
            // 
            // Raster
            // 
            this.Raster.ButtonStyle = enButtonStyle.Checkbox;
            this.Raster.ImageCode = "Raster|18";
            this.Raster.Location = new Point(8, 2);
            this.Raster.Name = "Raster";
            this.Raster.Size = new Size(112, 22);
            this.Raster.TabIndex = 4;
            this.Raster.Text = "Raster";
            this.Raster.CheckedChanged += new  EventHandler(this.Raster_CheckedChanged);
            // 
            // Bez_All
            // 
            this.Bez_All.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.Bez_All.Location = new Point(152, 2);
            this.Bez_All.Name = "Bez_All";
            this.Bez_All.Size = new Size(256, 22);
            this.Bez_All.TabIndex = 3;
            this.Bez_All.Text = "Alle Beziehungen erstellen";
            this.Bez_All.CheckedChanged += new  EventHandler(this.BezMode_CheckedChanged);
            // 
            // Area_Design
            // 
            this.Area_Design.CausesValidation = false;
            this.Area_Design.Controls.Add(this.ArbeitsbreichSetup);
            this.Area_Design.Controls.Add(this.SchriftGröße);
            this.Area_Design.Controls.Add(this.sscchrifthgöße);
            this.Area_Design.Controls.Add(this.PadDesign);
            this.Area_Design.Controls.Add(this.ssss);
            this.Area_Design.Dock = System.Windows.Forms.DockStyle.Left;
            this.Area_Design.Location = new Point(0, 0);
            this.Area_Design.Name = "Area_Design";
            this.Area_Design.Size = new Size(288, 81);
            this.Area_Design.Text = "Design";
            // 
            // ArbeitsbreichSetup
            // 
            this.ArbeitsbreichSetup.ImageCode = "SeiteEinrichten";
            this.ArbeitsbreichSetup.Location = new Point(8, 2);
            this.ArbeitsbreichSetup.Name = "ArbeitsbreichSetup";
            this.ArbeitsbreichSetup.Size = new Size(96, 66);
            this.ArbeitsbreichSetup.TabIndex = 13;
            this.ArbeitsbreichSetup.Text = "Arbeitsbereich einreichten";
            this.ArbeitsbreichSetup.Click += new EventHandler(this.ArbeitsbreichSetup_Click);
            // 
            // SchriftGröße
            // 
            this.SchriftGröße.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SchriftGröße.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SchriftGröße.Location = new Point(208, 46);
            this.SchriftGröße.Name = "SchriftGröße";
            this.SchriftGröße.Size = new Size(72, 22);
            this.SchriftGröße.TabIndex = 3;
            SchriftGröße.TextChanged += SchriftGröße_TextChanged;
             // 
             // sscchrifthgöße
             // 
            this.sscchrifthgöße.CausesValidation = false;
            this.sscchrifthgöße.Location = new Point(112, 46);
            this.sscchrifthgöße.Name = "sscchrifthgöße";
            this.sscchrifthgöße.Size = new Size(88, 22);
            this.sscchrifthgöße.Text = "Schrift-Größe:";
            // 
            // PadDesign
            // 
            this.PadDesign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.PadDesign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PadDesign.Location = new Point(112, 24);
            this.PadDesign.Name = "Design";
            this.PadDesign.Size = new Size(168, 22);
            this.PadDesign.TabIndex = 1;
            this.PadDesign.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.PadDesign_Item_Click);
            // 
            // ssss
            // 
            this.ssss.CausesValidation = false;
            this.ssss.Location = new Point(112, 2);
            this.ssss.Name = "ssss";
            this.ssss.Size = new Size(77, 22);
            this.ssss.Text = "Design:";
            this.ssss.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // LoadTab
            // 
            this.LoadTab.DefaultExt = "BCR";
            this.LoadTab.Filter = "*.BCR BCR-Datei|*.BCR|*.* Alle Dateien|*";
            this.LoadTab.Title = "Bitte Datei zum Laden wählen:";
            this.LoadTab.FileOk += new CancelEventHandler(this.LoadTab_FileOk);
            // 
            // SaveTab
            // 
            this.SaveTab.DefaultExt = "BCR";
            this.SaveTab.Filter = "*.BCR BCR-Datei|*.BCR|*.* Alle Dateien|*";
            this.SaveTab.Title = "Bitte neuen Dateinamen der Datei wählen.";
            this.SaveTab.FileOk += new CancelEventHandler(this.SaveTab_FileOk);
            // 
            // PictureView
            // 
            this.ClientSize = new Size(1334, 681);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.Ribbon);
            this.Name = "PictureView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.TopMost = true;
            this.Area_Seiten.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.Page_Start.ResumeLayout(false);
            this.Area_Drucken.ResumeLayout(false);
            this.Area_Dateisystem.ResumeLayout(false);
            this.Page_Control.ResumeLayout(false);
            this.Area_KomponenteHinzufügen.ResumeLayout(false);
            this.Area_Werkzeuge.ResumeLayout(false);
            this.Page_Settings.ResumeLayout(false);
            this.Area_Assistent.ResumeLayout(false);
            this.Area_Design.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button Links;
			private Button Rechts;
			private Button ZoomOut;
			private Button Auswahl;
			private Button AddText;
			private Button AddImage;
			private Button AddDistance;
			private Button AddDimension;
			private Button AddLine;
			public CreativePad Pad;
			protected TabControl Ribbon;
			private System.Windows.Forms.OpenFileDialog LoadTab;
			private System.Windows.Forms.SaveFileDialog SaveTab;
			protected TabPage Page_Control;
			protected TabPage Page_Start;
			protected Caption InfoText;
			protected GroupBox Area_Seiten;
			protected GroupBox Area_Werkzeuge;
			protected GroupBox Area_KomponenteHinzufügen;
			protected GroupBox Area_Drucken;
			protected GroupBox Area_Design;
			private Caption ssss;
			protected GroupBox Area_Assistent;
			private Button ZoomFitBut;
			private Button ZoomIn;
			private Button LoadDisk;
			private Button SaveDisk;
			private Button DelAll;
			private LastFilesCombo LastBCRFiles;
			private Button Drucken;
			private Button Bild;
			private Button Vorschau;
			private ComboBox PadDesign;
			private Button Bez_All;
			private Button Raster;
			private Caption RasterFangenCap;
			private Caption cappi1;
			private TextBox RasterFangen;
			private TextBox RasterAnzeige;
			private Button Bez_None;
			private Button Bez_Direkt;
			protected internal GroupBox Area_Dateisystem;
			internal ComboBox SchriftGröße;
			internal Caption sscchrifthgöße;
			protected internal Button Button_PageSetup;
			protected internal Button ArbeitsbreichSetup;


            private TabPage Page_Settings;
        }
	}