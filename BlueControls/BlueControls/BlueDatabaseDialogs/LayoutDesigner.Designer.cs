using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
    {


        internal partial class LayoutDesigner : PictureView
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
            this.DatenbankVER = new  GroupBox();
            this.NamB = new  Button();
            this.weg = new  Button();
            this.Hinzu = new  Button();
            this.Captionxx1 = new  Caption();
            this.Layout1 = new  ComboBox();
            this.Spaltx = new  ComboBox();
            this.Caption4 = new  Caption();
            this.Code = new  TextBox();
            this.Clip = new  Button();
            this.BlueFrame2 = new  GroupBox();
            this.Caption11 = new  Caption();
            this.FomAx = new  TabControl();
            this.txt = new  TabPage();
            this.Abkürz = new  Button();
            this.ÜberschriftS = new  Button();
            this.Leer = new  ComboBox();
            this.Nachtext = new  TextBox();
            this.ZeilUmbruch = new  ComboBox();
            this.Vortext = new  TextBox();
            this.HtmlSonderzeichen = new  Button();
            this.LeerLösch = new  Button();
            this.Spezialvormat = new  Button();
            this.ZeilCap = new  Caption();
            this.Caption10 = new  Caption();
            this.Caption9 = new  Caption();
            this.Caption7 = new  Caption();
            this.Pic = new  TabPage();
            this.Base64 = new  Button();
            this.Abma = new  GroupBox();
            this.GroMi = new  Button();
            this.Caption6 = new  Caption();
            this.Caption5 = new  Caption();
            this.ExactMi = new  Button();
            this.Mxx = new  Button();
            this.Wi = new  TextBox();
            this.He = new  TextBox();
            this.ex2 = new  TabPage();
            this.BefE2 = new  ListBox();
            this.BlueFrame1 = new  GroupBox();
            this.LayBearb = new  Button();
            this.LayVZ = new  Button();
            this.LayOpen = new  Button();
            this.Ribbon.SuspendLayout();
            this.Page_Control.SuspendLayout();
            this.Page_Start.SuspendLayout();
            this.Area_Drucken.SuspendLayout();
            this.Area_Design.SuspendLayout();
            this.DatenbankVER.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            this.FomAx.SuspendLayout();
            this.txt.SuspendLayout();
            this.Pic.SuspendLayout();
            this.Abma.SuspendLayout();
            this.ex2.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.AutoRelation = enAutoRelationMode.DirektVerbindungen_Erhalten;
            this.Pad.Size = new Size(837, 433);
            // 
            // Ribbon
            // 
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new Size(1340, 110);
            // 
            // Page_Control
            // 
            this.Page_Control.Size = new Size(1332, 81);
            // 
            // Page_Start
            // 
            this.Page_Start.Controls.Add(this.BlueFrame1);
            this.Page_Start.Controls.Add(this.DatenbankVER);
            this.Page_Start.Size = new Size(1332, 81);
            this.Page_Start.Controls.SetChildIndex(this.Area_Dateisystem, 0);
            this.Page_Start.Controls.SetChildIndex(this.Area_Drucken, 0);
            this.Page_Start.Controls.SetChildIndex(this.DatenbankVER, 0);
            this.Page_Start.Controls.SetChildIndex(this.BlueFrame1, 0);
            // 
            // InfoText
            // 
            this.InfoText.Size = new Size(90, 81);
            // 
            // DatenbankVER
            // 
            this.DatenbankVER.CausesValidation = false;
            this.DatenbankVER.Controls.Add(this.NamB);
            this.DatenbankVER.Controls.Add(this.weg);
            this.DatenbankVER.Controls.Add(this.Hinzu);
            this.DatenbankVER.Controls.Add(this.Captionxx1);
            this.DatenbankVER.Controls.Add(this.Layout1);
            this.DatenbankVER.Dock = System.Windows.Forms.DockStyle.Left;
            this.DatenbankVER.Location = new Point(600, 0);
            this.DatenbankVER.Name = "DatenbankVER";
            this.DatenbankVER.Size = new Size(232, 81);
            this.DatenbankVER.Text = "Datenbank";
            // 
            // NamB
            // 
            this.NamB.ImageCode = "Stift|16";
            this.NamB.Location = new Point(120, 46);
            this.NamB.Name = "NamB";
            this.NamB.Size = new Size(24, 22);
            this.NamB.TabIndex = 3;
            this.NamB.Click += new EventHandler(this.NamB_Click);
            // 
            // weg
            // 
            this.weg.ImageCode = "MinusZeichen|16";
            this.weg.Location = new Point(72, 46);
            this.weg.Name = "weg";
            this.weg.Size = new Size(24, 22);
            this.weg.TabIndex = 2;
            this.weg.Click += new EventHandler(this.weg_Click);
            // 
            // Hinzu
            // 
            this.Hinzu.ImageCode = "PlusZeichen|16";
            this.Hinzu.Location = new Point(40, 46);
            this.Hinzu.Name = "Hinzu";
            this.Hinzu.Size = new Size(24, 22);
            this.Hinzu.TabIndex = 1;
            this.Hinzu.Click += new EventHandler(this.Hinzu_Click);
            // 
            // Captionxx1
            // 
            this.Captionxx1.CausesValidation = false;
            this.Captionxx1.Location = new Point(8, 2);
            this.Captionxx1.Name = "Captionxx1";
            this.Captionxx1.Size = new Size(82, 22);
            this.Captionxx1.Text = "Layout";
            this.Captionxx1.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // Layout1
            // 
            this.Layout1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Layout1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Layout1.Format = enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.Layout1.Location = new Point(8, 24);
            this.Layout1.Name = "Layout1";
            this.Layout1.Size = new Size(216, 22);
            this.Layout1.TabIndex = 0;
            this.Layout1.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.Layout1_Item_Click);
            // 
            // Spaltx
            // 
            this.Spaltx.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Spaltx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Spaltx.Format = enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.Spaltx.Location = new Point(168, 24);
            this.Spaltx.Name = "Spaltx";
            this.Spaltx.Size = new Size(232, 22);
            this.Spaltx.TabIndex = 3;
            this.Spaltx.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.Spalt_Item_Click);
            // 
            // Caption4
            // 
            this.Caption4.CausesValidation = false;
            this.Caption4.Location = new Point(16, 24);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new Size(104, 24);
            this.Caption4.Text = "<b><u>Bezugsspalte:";
            this.Caption4.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // Code
            // 
            this.Code.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Code.Location = new Point(56, 352);
            this.Code.Name = "Code";
            this.Code.Size = new Size(280, 66);
            this.Code.TabIndex = 112;
            this.Code.Verhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Clip
            // 
            this.Clip.ButtonStyle = enButtonStyle.Button_RibbonBar;
            this.Clip.ImageCode = "Clipboard";
            this.Clip.Location = new Point(392, 352);
            this.Clip.Name = "Clip";
            this.Clip.Size = new Size(68, 66);
            this.Clip.TabIndex = 111;
            this.Clip.Text = "Clipboard";
            this.Clip.Click += new EventHandler(this.Clip_Click);
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.Caption4);
            this.BlueFrame2.Controls.Add(this.Spaltx);
            this.BlueFrame2.Controls.Add(this.Code);
            this.BlueFrame2.Controls.Add(this.Caption11);
            this.BlueFrame2.Controls.Add(this.Clip);
            this.BlueFrame2.Controls.Add(this.FomAx);
            this.BlueFrame2.Dock = System.Windows.Forms.DockStyle.Right;
            this.BlueFrame2.Location = new Point(837, 110);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(503, 433);
            this.BlueFrame2.Text = "Feld-Codes";
            // 
            // Caption11
            // 
            this.Caption11.CausesValidation = false;
            this.Caption11.Location = new Point(8, 352);
            this.Caption11.Name = "Caption11";
            this.Caption11.Size = new Size(48, 18);
            this.Caption11.Text = "Code:";
            // 
            // FomAx
            // 
            this.FomAx.Controls.Add(this.txt);
            this.FomAx.Controls.Add(this.Pic);
            this.FomAx.Controls.Add(this.ex2);
            this.FomAx.HotTrack = true;
            this.FomAx.Location = new Point(8, 67);
            this.FomAx.Name = "FomAx";
            this.FomAx.Size = new Size(472, 285);
            this.FomAx.TabIndex = 111;
            // 
            // txt
            // 
            this.txt.Controls.Add(this.Abkürz);
            this.txt.Controls.Add(this.ÜberschriftS);
            this.txt.Controls.Add(this.Leer);
            this.txt.Controls.Add(this.Nachtext);
            this.txt.Controls.Add(this.ZeilUmbruch);
            this.txt.Controls.Add(this.Vortext);
            this.txt.Controls.Add(this.HtmlSonderzeichen);
            this.txt.Controls.Add(this.LeerLösch);
            this.txt.Controls.Add(this.Spezialvormat);
            this.txt.Controls.Add(this.ZeilCap);
            this.txt.Controls.Add(this.Caption10);
            this.txt.Controls.Add(this.Caption9);
            this.txt.Controls.Add(this.Caption7);
            this.txt.Location = new Point(4, 25);
            this.txt.Name = "txt";
            this.txt.Padding = new System.Windows.Forms.Padding(3);
            this.txt.Size = new Size(464, 256);
            this.txt.TabIndex = 0;
            this.txt.Text = "Textfeld";
            this.txt.UseVisualStyleBackColor = true;
            // 
            // Abkürz
            // 
            this.Abkürz.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.Abkürz.Location = new Point(0, 208);
            this.Abkürz.Name = "Abkürz";
            this.Abkürz.Size = new Size(448, 16);
            this.Abkürz.TabIndex = 109;
            this.Abkürz.Text = "Benutze Abkürzungen <i>(z.B. Minute -<>> Min.)";
            this.Abkürz.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // ÜberschriftS
            // 
            this.ÜberschriftS.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.ÜberschriftS.Location = new Point(0, 192);
            this.ÜberschriftS.Name = "ÜberschriftS";
            this.ÜberschriftS.Size = new Size(448, 16);
            this.ÜberschriftS.TabIndex = 108;
            this.ÜberschriftS.Text = "Überschriften noch stärker darstellen";
            this.ÜberschriftS.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // Leer
            // 
            this.Leer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Leer.Location = new Point(184, 40);
            this.Leer.Name = "Leer";
            this.Leer.Size = new Size(273, 24);
            this.Leer.TabIndex = 98;
            this.Leer.TextChanged += new EventHandler(this.Leer_TextChanged);
            // 
            // Nachtext
            // 
            this.Nachtext.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Nachtext.Location = new Point(236, 111);
            this.Nachtext.Name = "Nachtext";
            this.Nachtext.Size = new Size(224, 24);
            this.Nachtext.TabIndex = 101;
            this.Nachtext.TextChanged += new EventHandler(this.Leer_TextChanged);
            // 
            // ZeilUmbruch
            // 
            this.ZeilUmbruch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ZeilUmbruch.Location = new Point(184, 63);
            this.ZeilUmbruch.Name = "ZeilUmbruch";
            this.ZeilUmbruch.Size = new Size(273, 24);
            this.ZeilUmbruch.TabIndex = 99;
            this.ZeilUmbruch.TextChanged += new EventHandler(this.Leer_TextChanged);
            // 
            // Vortext
            // 
            this.Vortext.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Vortext.Location = new Point(4, 111);
            this.Vortext.Name = "Vortext";
            this.Vortext.Size = new Size(224, 24);
            this.Vortext.TabIndex = 100;
            this.Vortext.TextChanged += new EventHandler(this.Leer_TextChanged);
            // 
            // HtmlSonderzeichen
            // 
            this.HtmlSonderzeichen.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.HtmlSonderzeichen.Location = new Point(0, 176);
            this.HtmlSonderzeichen.Name = "HtmlSonderzeichen";
            this.HtmlSonderzeichen.Size = new Size(448, 16);
            this.HtmlSonderzeichen.TabIndex = 107;
            this.HtmlSonderzeichen.Text = "Ersetze \'&amp;\', \'<<>\' und \'<>>\' für XML-Formate";
            this.HtmlSonderzeichen.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // LeerLösch
            // 
            this.LeerLösch.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.LeerLösch.Location = new Point(0, 15);
            this.LeerLösch.Name = "LeerLösch";
            this.LeerLösch.Size = new Size(208, 16);
            this.LeerLösch.TabIndex = 92;
            this.LeerLösch.Text = "Wenn Leer, komplett löschen";
            this.LeerLösch.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // Spezialvormat
            // 
            this.Spezialvormat.ButtonStyle = enButtonStyle.Checkbox_Text;
            this.Spezialvormat.Location = new Point(0, 160);
            this.Spezialvormat.Name = "Spezialvormat";
            this.Spezialvormat.Size = new Size(448, 16);
            this.Spezialvormat.TabIndex = 106;
            this.Spezialvormat.Text = "Bereinige das Format (z.B.: Spezielle Felder wie Telefonnummer)";
            this.Spezialvormat.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // ZeilCap
            // 
            this.ZeilCap.CausesValidation = false;
            this.ZeilCap.Location = new Point(4, 63);
            this.ZeilCap.Name = "ZeilCap";
            this.ZeilCap.Size = new Size(172, 18);
            this.ZeilCap.Text = "Ersetze Zeilenumbrüche mit:";
            // 
            // Caption10
            // 
            this.Caption10.CausesValidation = false;
            this.Caption10.Location = new Point(236, 95);
            this.Caption10.Name = "Caption10";
            this.Caption10.Size = new Size(76, 18);
            this.Caption10.Text = "Nachtext:";
            // 
            // Caption9
            // 
            this.Caption9.CausesValidation = false;
            this.Caption9.Location = new Point(4, 95);
            this.Caption9.Name = "Caption9";
            this.Caption9.Size = new Size(68, 18);
            this.Caption9.Text = "Vortext:";
            // 
            // Caption7
            // 
            this.Caption7.CausesValidation = false;
            this.Caption7.Location = new Point(4, 39);
            this.Caption7.Name = "Caption7";
            this.Caption7.Size = new Size(134, 18);
            this.Caption7.Text = "Ersetze Leeres Feld mit:";
            // 
            // Pic
            // 
            this.Pic.Controls.Add(this.Base64);
            this.Pic.Controls.Add(this.Abma);
            this.Pic.Location = new Point(4, 25);
            this.Pic.Name = "Pic";
            this.Pic.Padding = new System.Windows.Forms.Padding(3);
            this.Pic.Size = new Size(464, 256);
            this.Pic.TabIndex = 1;
            this.Pic.Text = "Bild";
            this.Pic.UseVisualStyleBackColor = true;
            // 
            // Base64
            // 
            this.Base64.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.Base64.Checked = true;
            this.Base64.Location = new Point(304, 32);
            this.Base64.Name = "Base64";
            this.Base64.Size = new Size(135, 16);
            this.Base64.TabIndex = 10;
            this.Base64.Text = "Base64-Format";
            this.Base64.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // Abma
            // 
            this.Abma.CausesValidation = false;
            this.Abma.Controls.Add(this.GroMi);
            this.Abma.Controls.Add(this.Caption6);
            this.Abma.Controls.Add(this.Caption5);
            this.Abma.Controls.Add(this.ExactMi);
            this.Abma.Controls.Add(this.Mxx);
            this.Abma.Controls.Add(this.Wi);
            this.Abma.Controls.Add(this.He);
            this.Abma.Location = new Point(8, 8);
            this.Abma.Name = "Abma";
            this.Abma.Size = new Size(272, 240);
            this.Abma.Text = "Abmaße:";
            // 
            // GroMi
            // 
            this.GroMi.ButtonStyle = enButtonStyle.Optionbox_RibbonBar;
            this.GroMi.Checked = true;
            this.GroMi.ImageCode = "BildmodusAbschneiden";
            this.GroMi.ImageCode_Checked = "Das Bild wird in den Rahmen eingepasst, <br> das Seitenverhältniss wird beibehalt" +
    "en,<br>überstehende Bildteile werden abgeschnitten";
            this.GroMi.Location = new Point(8, 56);
            this.GroMi.Name = "GroMi";
            this.GroMi.Size = new Size(80, 64);
            this.GroMi.TabIndex = 10;
            this.GroMi.Text = "abschneiden";
            this.GroMi.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // Caption6
            // 
            this.Caption6.CausesValidation = false;
            this.Caption6.Location = new Point(8, 16);
            this.Caption6.Name = "Caption6";
            this.Caption6.Size = new Size(39, 18);
            this.Caption6.Text = "Breite:";
            // 
            // Caption5
            // 
            this.Caption5.CausesValidation = false;
            this.Caption5.Location = new Point(112, 16);
            this.Caption5.Name = "Caption5";
            this.Caption5.Size = new Size(34, 18);
            this.Caption5.Text = "Höhe:";
            // 
            // ExactMi
            // 
            this.ExactMi.ButtonStyle = enButtonStyle.Optionbox_RibbonBar;
            this.ExactMi.ImageCode = "BildmodusEinpassen";
            this.ExactMi.Location = new Point(96, 56);
            this.ExactMi.Name = "ExactMi";
            this.ExactMi.QuickInfo = "Das Bild wird in den Rahmen eingepasst, <br> das Seitenverhältniss wird beibehalt" +
    "en,<br>der Rest wird mit weißer Farbe auffüllt";
            this.ExactMi.Size = new Size(80, 64);
            this.ExactMi.TabIndex = 8;
            this.ExactMi.Text = "einpassen";
            this.ExactMi.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // Mxx
            // 
            this.Mxx.ButtonStyle = enButtonStyle.Optionbox_RibbonBar;
            this.Mxx.ImageCode = "BildmodusVerzerren";
            this.Mxx.Location = new Point(184, 56);
            this.Mxx.Name = "Mxx";
            this.Mxx.QuickInfo = "Das Bild wird verzerrt, dass es<br>in den Rahmen passt.";
            this.Mxx.Size = new Size(80, 64);
            this.Mxx.TabIndex = 9;
            this.Mxx.Text = "verzerren";
            this.Mxx.Click += new EventHandler(this.LeerLösch_Click);
            // 
            // Wi
            // 
            this.Wi.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Wi.Format = enDataFormat.Ganzzahl;
            this.Wi.Location = new Point(48, 16);
            this.Wi.Name = "Wi";
            this.Wi.Size = new Size(40, 24);
            this.Wi.TabIndex = 4;
            this.Wi.Text = "800";
            this.Wi.TextChanged += new EventHandler(this.Leer_TextChanged);
            // 
            // He
            // 
            this.He.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.He.Format = enDataFormat.Ganzzahl;
            this.He.Location = new Point(152, 16);
            this.He.Name = "He";
            this.He.Size = new Size(40, 24);
            this.He.TabIndex = 5;
            this.He.Text = "600";
            this.He.TextChanged += new EventHandler(this.Leer_TextChanged);
            // 
            // ex2
            // 
            this.ex2.Controls.Add(this.BefE2);
            this.ex2.Location = new Point(4, 25);
            this.ex2.Name = "ex2";
            this.ex2.Padding = new System.Windows.Forms.Padding(3);
            this.ex2.Size = new Size(464, 256);
            this.ex2.TabIndex = 2;
            this.ex2.Text = "Allgemeine Steuercodes";
            this.ex2.UseVisualStyleBackColor = true;
            // 
            // BefE2
            // 
            this.BefE2.AddAllowed = enAddType.OnlySuggests;
            this.BefE2.Location = new Point(0, 16);
            this.BefE2.Name = "BefE2";
            this.BefE2.QuickInfo = "";
            this.BefE2.Size = new Size(456, 232);
            this.BefE2.TabIndex = 0;
            this.BefE2.ItemCheckedChanged += new  EventHandler(this.BefE2_Item_CheckedChanged);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.LayBearb);
            this.BlueFrame1.Controls.Add(this.LayVZ);
            this.BlueFrame1.Controls.Add(this.LayOpen);
            this.BlueFrame1.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame1.Location = new Point(832, 0);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new Size(336, 81);
            this.BlueFrame1.Text = "Externe Layouts aus dem Dateisytem";
            // 
            // LayBearb
            // 
            this.LayBearb.ImageCode = "Textdatei";
            this.LayBearb.Location = new Point(8, 2);
            this.LayBearb.Name = "LayBearb";
            this.LayBearb.QuickInfo = "Layout mit Texteditor bearbeiten";
            this.LayBearb.Size = new Size(80, 66);
            this.LayBearb.TabIndex = 81;
            this.LayBearb.Text = "Texteditor";
            this.LayBearb.Click += new EventHandler(this.LayBearb_Click);
            // 
            // LayVZ
            // 
            this.LayVZ.ImageCode = "Ordner|16";
            this.LayVZ.Location = new Point(96, 46);
            this.LayVZ.Name = "LayVZ";
            this.LayVZ.Size = new Size(232, 22);
            this.LayVZ.TabIndex = 84;
            this.LayVZ.Text = "Layout-Verzeichnis öffnen";
            this.LayVZ.Click += new EventHandler(this.LayVZ_Click);
            // 
            // LayOpen
            // 
            this.LayOpen.ImageCode = "Anwendung|16";
            this.LayOpen.Location = new Point(96, 2);
            this.LayOpen.Name = "LayOpen";
            this.LayOpen.Size = new Size(232, 22);
            this.LayOpen.TabIndex = 83;
            this.LayOpen.Text = "Layout mit Std.-Anwendung öffnen";
            this.LayOpen.Click += new EventHandler(this.LayOpen_Click);
            // 
            // LayoutDesigner
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(1340, 543);
            this.Controls.Add(this.BlueFrame2);
            this.Name = "LayoutDesigner";
            this.Text = "Druck-Layout";
            this.TopMost = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.BlueFrame2, 0);
            this.Controls.SetChildIndex(this.Pad, 0);
            this.Ribbon.ResumeLayout(false);
            this.Page_Control.ResumeLayout(false);
            this.Page_Start.ResumeLayout(false);
            this.Area_Drucken.ResumeLayout(false);
            this.Area_Design.ResumeLayout(false);
            this.DatenbankVER.ResumeLayout(false);
            this.BlueFrame2.ResumeLayout(false);
            this.FomAx.ResumeLayout(false);
            this.txt.ResumeLayout(false);
            this.Pic.ResumeLayout(false);
            this.Abma.ResumeLayout(false);
            this.ex2.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			internal GroupBox DatenbankVER;
			internal Button NamB;
			internal Button weg;
			internal Button Hinzu;
			internal Caption Captionxx1;
			internal ComboBox Layout1;
			private Button Clip;
			private TextBox Code;
			private Caption Caption4;
			private ComboBox Spaltx;
			private GroupBox BlueFrame2;
			private Caption Caption11;
			private TabControl FomAx;
			private TabPage txt;
			private ComboBox Leer;
			private TextBox Nachtext;
			private ComboBox ZeilUmbruch;
			private TextBox Vortext;
			private Button HtmlSonderzeichen;
			private Button LeerLösch;
			private Button Spezialvormat;
			private Caption ZeilCap;
			private Caption Caption10;
			private Caption Caption9;
			private Caption Caption7;
			private TabPage Pic;
			private Button Base64;
			private GroupBox Abma;
			private Button GroMi;
			private Caption Caption6;
			private Caption Caption5;
			private Button ExactMi;
			private Button Mxx;
			private TextBox Wi;
			private TextBox He;
			private TabPage ex2;
			private ListBox BefE2;
			internal GroupBox BlueFrame1;
			private Button LayBearb;
			private Button LayVZ;
			private Button LayOpen;
			private Button ÜberschriftS;
			private Button Abkürz;
		}
	}
