using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.BlueDatabaseDialogs
{


    internal partial class LayoutDesigner
    { 
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
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
            this.DatenbankVER = new BlueControls.Controls.GroupBox();
            this.NamB = new BlueControls.Controls.Button();
            this.weg = new BlueControls.Controls.Button();
            this.Hinzu = new BlueControls.Controls.Button();
            this.Captionxx1 = new BlueControls.Controls.Caption();
            this.Layout1 = new BlueControls.Controls.ComboBox();
            this.Spaltx = new BlueControls.Controls.ComboBox();
            this.Caption4 = new BlueControls.Controls.Caption();
            this.Code = new BlueControls.Controls.TextBox();
            this.Clip = new BlueControls.Controls.Button();
            this.groupBoxX = new BlueControls.Controls.GroupBox();
            this.Caption11 = new BlueControls.Controls.Caption();
            this.FomAx = new BlueControls.Controls.TabControl();
            this.txt = new BlueControls.Controls.TabPage();
            this.Abkürz = new BlueControls.Controls.Button();
            this.ÜberschriftS = new BlueControls.Controls.Button();
            this.Leer = new BlueControls.Controls.ComboBox();
            this.Nachtext = new BlueControls.Controls.TextBox();
            this.ZeilUmbruch = new BlueControls.Controls.ComboBox();
            this.Vortext = new BlueControls.Controls.TextBox();
            this.HtmlSonderzeichen = new BlueControls.Controls.Button();
            this.LeerLösch = new BlueControls.Controls.Button();
            this.Spezialvormat = new BlueControls.Controls.Button();
            this.ZeilCap = new BlueControls.Controls.Caption();
            this.Caption10 = new BlueControls.Controls.Caption();
            this.Caption9 = new BlueControls.Controls.Caption();
            this.Caption7 = new BlueControls.Controls.Caption();
            this.Pic = new BlueControls.Controls.TabPage();
            this.Base64 = new BlueControls.Controls.Button();
            this.Abma = new BlueControls.Controls.GroupBox();
            this.GroMi = new BlueControls.Controls.Button();
            this.Caption6 = new BlueControls.Controls.Caption();
            this.Caption5 = new BlueControls.Controls.Caption();
            this.ExactMi = new BlueControls.Controls.Button();
            this.Mxx = new BlueControls.Controls.Button();
            this.Wi = new BlueControls.Controls.TextBox();
            this.He = new BlueControls.Controls.TextBox();
            this.ex2 = new BlueControls.Controls.TabPage();
            this.BefE2 = new BlueControls.Controls.ListBox();
            this.BlueFrame1 = new BlueControls.Controls.GroupBox();
            this.LayBearb = new BlueControls.Controls.Button();
            this.LayVZ = new BlueControls.Controls.Button();
            this.LayOpen = new BlueControls.Controls.Button();
            this.grpDateiSystem = new BlueControls.Controls.GroupBox();
            this.btnLastFiles = new BlueControls.Controls.LastFilesCombo();
            this.btnNeu = new BlueControls.Controls.Button();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSpeichern = new BlueControls.Controls.Button();
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.tabRightSide = new BlueControls.Controls.TabControl();
            this.tabElementEigenschaften = new BlueControls.Controls.TabPage();
            this.tabCodeGenerator = new BlueControls.Controls.TabPage();
            this.Ribbon.SuspendLayout();
            this.tabPageControl.SuspendLayout();
            this.tabPageStart.SuspendLayout();
            this.Area_Drucken.SuspendLayout();
            this.Area_Design.SuspendLayout();
            this.DatenbankVER.SuspendLayout();
            this.groupBoxX.SuspendLayout();
            this.FomAx.SuspendLayout();
            this.txt.SuspendLayout();
            this.Pic.SuspendLayout();
            this.Abma.SuspendLayout();
            this.ex2.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.grpDateiSystem.SuspendLayout();
            this.tabRightSide.SuspendLayout();
            this.tabCodeGenerator.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.AutoRelation = ((BlueControls.Enums.enAutoRelationMode)((BlueControls.Enums.enAutoRelationMode.DirektVerbindungen | BlueControls.Enums.enAutoRelationMode.NurBeziehungenErhalten)));
            this.Pad.Size = new System.Drawing.Size(816, 502);
            this.Pad.HotItemChanged += new System.EventHandler(this.Pad_HotItemChanged);
            // 
            // Ribbon
            // 
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new System.Drawing.Size(1340, 110);
            // 
            // tabPageStart
            // 
            this.tabPageStart.Controls.Add(this.BlueFrame1);
            this.tabPageStart.Controls.Add(this.DatenbankVER);
            this.tabPageStart.Controls.Add(this.grpDateiSystem);
            this.tabPageStart.Size = new System.Drawing.Size(1332, 81);
            this.tabPageStart.Controls.SetChildIndex(this.grpDateiSystem, 0);
            this.tabPageStart.Controls.SetChildIndex(this.Area_Drucken, 0);
            this.tabPageStart.Controls.SetChildIndex(this.DatenbankVER, 0);
            this.tabPageStart.Controls.SetChildIndex(this.BlueFrame1, 0);
            // 
            // Area_Drucken
            // 
            this.Area_Drucken.Location = new System.Drawing.Point(312, 0);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ButtonStyle = BlueControls.Enums.enButtonStyle.Button;
            // 
            // ArbeitsbreichSetup
            // 
            this.ArbeitsbreichSetup.ButtonStyle = BlueControls.Enums.enButtonStyle.Button;
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
            this.DatenbankVER.Location = new System.Drawing.Point(608, 0);
            this.DatenbankVER.Name = "DatenbankVER";
            this.DatenbankVER.Size = new System.Drawing.Size(232, 81);
            this.DatenbankVER.Text = "Datenbank";
            // 
            // NamB
            // 
            this.NamB.ImageCode = "Stift|16";
            this.NamB.Location = new System.Drawing.Point(120, 46);
            this.NamB.Name = "NamB";
            this.NamB.Size = new System.Drawing.Size(24, 22);
            this.NamB.TabIndex = 3;
            this.NamB.Click += new System.EventHandler(this.NamB_Click);
            // 
            // weg
            // 
            this.weg.ImageCode = "MinusZeichen|16";
            this.weg.Location = new System.Drawing.Point(72, 46);
            this.weg.Name = "weg";
            this.weg.Size = new System.Drawing.Size(24, 22);
            this.weg.TabIndex = 2;
            this.weg.Click += new System.EventHandler(this.weg_Click);
            // 
            // Hinzu
            // 
            this.Hinzu.ImageCode = "PlusZeichen|16";
            this.Hinzu.Location = new System.Drawing.Point(40, 46);
            this.Hinzu.Name = "Hinzu";
            this.Hinzu.Size = new System.Drawing.Size(24, 22);
            this.Hinzu.TabIndex = 1;
            this.Hinzu.Click += new System.EventHandler(this.Hinzu_Click);
            // 
            // Captionxx1
            // 
            this.Captionxx1.Location = new System.Drawing.Point(8, 2);
            this.Captionxx1.Name = "Captionxx1";
            this.Captionxx1.Size = new System.Drawing.Size(82, 22);
            this.Captionxx1.Text = "Layout";
            this.Captionxx1.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // Layout1
            // 
            this.Layout1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Layout1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Layout1.Location = new System.Drawing.Point(8, 24);
            this.Layout1.Name = "Layout1";
            this.Layout1.Size = new System.Drawing.Size(216, 22);
            this.Layout1.TabIndex = 0;
            this.Layout1.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Layout1_ItemClicked);
            // 
            // Spaltx
            // 
            this.Spaltx.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Spaltx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Spaltx.Location = new System.Drawing.Point(168, 24);
            this.Spaltx.Name = "Spaltx";
            this.Spaltx.Size = new System.Drawing.Size(232, 22);
            this.Spaltx.TabIndex = 3;
            this.Spaltx.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Spalt_ItemClicked);
            // 
            // Caption4
            // 
            this.Caption4.Location = new System.Drawing.Point(16, 24);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new System.Drawing.Size(104, 24);
            this.Caption4.Text = "<b><u>Bezugsspalte:";
            this.Caption4.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // Code
            // 
            this.Code.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Code.Location = new System.Drawing.Point(56, 352);
            this.Code.Name = "Code";
            this.Code.Size = new System.Drawing.Size(280, 66);
            this.Code.TabIndex = 112;
            this.Code.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Clip
            // 
            this.Clip.ButtonStyle = BlueControls.Enums.enButtonStyle.Button_RibbonBar;
            this.Clip.ImageCode = "Clipboard";
            this.Clip.Location = new System.Drawing.Point(392, 352);
            this.Clip.Name = "Clip";
            this.Clip.Size = new System.Drawing.Size(68, 66);
            this.Clip.TabIndex = 111;
            this.Clip.Text = "Clipboard";
            this.Clip.Click += new System.EventHandler(this.Clip_Click);
            // 
            // groupBoxX
            // 
            this.groupBoxX.CausesValidation = false;
            this.groupBoxX.Controls.Add(this.Caption4);
            this.groupBoxX.Controls.Add(this.Spaltx);
            this.groupBoxX.Controls.Add(this.Code);
            this.groupBoxX.Controls.Add(this.Caption11);
            this.groupBoxX.Controls.Add(this.Clip);
            this.groupBoxX.Controls.Add(this.FomAx);
            this.groupBoxX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxX.Location = new System.Drawing.Point(0, 0);
            this.groupBoxX.Name = "groupBoxX";
            this.groupBoxX.Size = new System.Drawing.Size(516, 473);
            this.groupBoxX.Text = "Feld-Codes";
            // 
            // Caption11
            // 
            this.Caption11.Location = new System.Drawing.Point(8, 352);
            this.Caption11.Name = "Caption11";
            this.Caption11.Size = new System.Drawing.Size(48, 18);
            this.Caption11.Text = "Code:";
            // 
            // FomAx
            // 
            this.FomAx.Controls.Add(this.txt);
            this.FomAx.Controls.Add(this.Pic);
            this.FomAx.Controls.Add(this.ex2);
            this.FomAx.HotTrack = true;
            this.FomAx.Location = new System.Drawing.Point(8, 67);
            this.FomAx.Name = "FomAx";
            this.FomAx.Size = new System.Drawing.Size(472, 285);
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
            this.txt.Location = new System.Drawing.Point(4, 25);
            this.txt.Name = "txt";
            this.txt.Padding = new System.Windows.Forms.Padding(3);
            this.txt.Size = new System.Drawing.Size(464, 256);
            this.txt.TabIndex = 0;
            this.txt.Text = "Textfeld";
            this.txt.UseVisualStyleBackColor = true;
            // 
            // Abkürz
            // 
            this.Abkürz.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.Abkürz.Location = new System.Drawing.Point(0, 208);
            this.Abkürz.Name = "Abkürz";
            this.Abkürz.Size = new System.Drawing.Size(448, 16);
            this.Abkürz.TabIndex = 109;
            this.Abkürz.Text = "Benutze Abkürzungen <i>(z.B. Minute -<>> Min.)";
            this.Abkürz.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // ÜberschriftS
            // 
            this.ÜberschriftS.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.ÜberschriftS.Location = new System.Drawing.Point(0, 192);
            this.ÜberschriftS.Name = "ÜberschriftS";
            this.ÜberschriftS.Size = new System.Drawing.Size(448, 16);
            this.ÜberschriftS.TabIndex = 108;
            this.ÜberschriftS.Text = "Überschriften noch stärker darstellen";
            this.ÜberschriftS.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // Leer
            // 
            this.Leer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Leer.Location = new System.Drawing.Point(184, 40);
            this.Leer.Name = "Leer";
            this.Leer.Size = new System.Drawing.Size(273, 24);
            this.Leer.TabIndex = 98;
            this.Leer.TextChanged += new System.EventHandler(this.CodeGeneratorChanges_TextChanged);
            // 
            // Nachtext
            // 
            this.Nachtext.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Nachtext.Location = new System.Drawing.Point(236, 111);
            this.Nachtext.Name = "Nachtext";
            this.Nachtext.Size = new System.Drawing.Size(224, 24);
            this.Nachtext.TabIndex = 101;
            this.Nachtext.TextChanged += new System.EventHandler(this.CodeGeneratorChanges_TextChanged);
            // 
            // ZeilUmbruch
            // 
            this.ZeilUmbruch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ZeilUmbruch.Location = new System.Drawing.Point(184, 63);
            this.ZeilUmbruch.Name = "ZeilUmbruch";
            this.ZeilUmbruch.Size = new System.Drawing.Size(273, 24);
            this.ZeilUmbruch.TabIndex = 99;
            this.ZeilUmbruch.TextChanged += new System.EventHandler(this.CodeGeneratorChanges_TextChanged);
            // 
            // Vortext
            // 
            this.Vortext.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Vortext.Location = new System.Drawing.Point(4, 111);
            this.Vortext.Name = "Vortext";
            this.Vortext.Size = new System.Drawing.Size(224, 24);
            this.Vortext.TabIndex = 100;
            this.Vortext.TextChanged += new System.EventHandler(this.CodeGeneratorChanges_TextChanged);
            // 
            // HtmlSonderzeichen
            // 
            this.HtmlSonderzeichen.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.HtmlSonderzeichen.Location = new System.Drawing.Point(0, 176);
            this.HtmlSonderzeichen.Name = "HtmlSonderzeichen";
            this.HtmlSonderzeichen.Size = new System.Drawing.Size(448, 16);
            this.HtmlSonderzeichen.TabIndex = 107;
            this.HtmlSonderzeichen.Text = "Ersetze \'&amp;\', \'<<>\' und \'<>>\' für XML-Formate";
            this.HtmlSonderzeichen.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // LeerLösch
            // 
            this.LeerLösch.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.LeerLösch.Location = new System.Drawing.Point(0, 15);
            this.LeerLösch.Name = "LeerLösch";
            this.LeerLösch.Size = new System.Drawing.Size(208, 16);
            this.LeerLösch.TabIndex = 92;
            this.LeerLösch.Text = "Wenn Leer, komplett löschen";
            this.LeerLösch.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // Spezialvormat
            // 
            this.Spezialvormat.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.Spezialvormat.Location = new System.Drawing.Point(0, 160);
            this.Spezialvormat.Name = "Spezialvormat";
            this.Spezialvormat.Size = new System.Drawing.Size(448, 16);
            this.Spezialvormat.TabIndex = 106;
            this.Spezialvormat.Text = "Bereinige das Format (z.B.: Spezielle Felder wie Telefonnummer)";
            this.Spezialvormat.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // ZeilCap
            // 
            this.ZeilCap.Location = new System.Drawing.Point(4, 63);
            this.ZeilCap.Name = "ZeilCap";
            this.ZeilCap.Size = new System.Drawing.Size(172, 18);
            this.ZeilCap.Text = "Ersetze Zeilenumbrüche mit:";
            // 
            // Caption10
            // 
            this.Caption10.Location = new System.Drawing.Point(236, 95);
            this.Caption10.Name = "Caption10";
            this.Caption10.Size = new System.Drawing.Size(76, 18);
            this.Caption10.Text = "Nachtext:";
            // 
            // Caption9
            // 
            this.Caption9.Location = new System.Drawing.Point(4, 95);
            this.Caption9.Name = "Caption9";
            this.Caption9.Size = new System.Drawing.Size(68, 18);
            this.Caption9.Text = "Vortext:";
            // 
            // Caption7
            // 
            this.Caption7.Location = new System.Drawing.Point(4, 39);
            this.Caption7.Name = "Caption7";
            this.Caption7.Size = new System.Drawing.Size(134, 18);
            this.Caption7.Text = "Ersetze Leeres Feld mit:";
            // 
            // Pic
            // 
            this.Pic.Controls.Add(this.Base64);
            this.Pic.Controls.Add(this.Abma);
            this.Pic.Location = new System.Drawing.Point(4, 25);
            this.Pic.Name = "Pic";
            this.Pic.Padding = new System.Windows.Forms.Padding(3);
            this.Pic.Size = new System.Drawing.Size(464, 256);
            this.Pic.TabIndex = 1;
            this.Pic.Text = "Bild";
            this.Pic.UseVisualStyleBackColor = true;
            // 
            // Base64
            // 
            this.Base64.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.Base64.Checked = true;
            this.Base64.Location = new System.Drawing.Point(304, 32);
            this.Base64.Name = "Base64";
            this.Base64.Size = new System.Drawing.Size(135, 16);
            this.Base64.TabIndex = 10;
            this.Base64.Text = "Base64-Format";
            this.Base64.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
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
            this.Abma.Location = new System.Drawing.Point(8, 8);
            this.Abma.Name = "Abma";
            this.Abma.Size = new System.Drawing.Size(272, 240);
            this.Abma.Text = "Abmaße:";
            // 
            // GroMi
            // 
            this.GroMi.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_RibbonBar;
            this.GroMi.Checked = true;
            this.GroMi.ImageCode = "BildmodusAbschneiden";
            this.GroMi.ImageCode_Checked = "Das Bild wird in den Rahmen eingepasst, <br> das Seitenverhältniss wird beibehalt" +
    "en,<br>überstehende Bildteile werden abgeschnitten";
            this.GroMi.Location = new System.Drawing.Point(8, 56);
            this.GroMi.Name = "GroMi";
            this.GroMi.Size = new System.Drawing.Size(80, 64);
            this.GroMi.TabIndex = 10;
            this.GroMi.Text = "abschneiden";
            this.GroMi.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // Caption6
            // 
            this.Caption6.Location = new System.Drawing.Point(8, 16);
            this.Caption6.Name = "Caption6";
            this.Caption6.Size = new System.Drawing.Size(39, 18);
            this.Caption6.Text = "Breite:";
            // 
            // Caption5
            // 
            this.Caption5.Location = new System.Drawing.Point(112, 16);
            this.Caption5.Name = "Caption5";
            this.Caption5.Size = new System.Drawing.Size(34, 18);
            this.Caption5.Text = "Höhe:";
            // 
            // ExactMi
            // 
            this.ExactMi.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_RibbonBar;
            this.ExactMi.ImageCode = "BildmodusEinpassen";
            this.ExactMi.Location = new System.Drawing.Point(96, 56);
            this.ExactMi.Name = "ExactMi";
            this.ExactMi.QuickInfo = "Das Bild wird in den Rahmen eingepasst, <br> das Seitenverhältniss wird beibehalt" +
    "en,<br>der Rest wird mit weißer Farbe auffüllt";
            this.ExactMi.Size = new System.Drawing.Size(80, 64);
            this.ExactMi.TabIndex = 8;
            this.ExactMi.Text = "einpassen";
            this.ExactMi.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // Mxx
            // 
            this.Mxx.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_RibbonBar;
            this.Mxx.ImageCode = "BildmodusVerzerren";
            this.Mxx.Location = new System.Drawing.Point(184, 56);
            this.Mxx.Name = "Mxx";
            this.Mxx.QuickInfo = "Das Bild wird verzerrt, dass es<br>in den Rahmen passt.";
            this.Mxx.Size = new System.Drawing.Size(80, 64);
            this.Mxx.TabIndex = 9;
            this.Mxx.Text = "verzerren";
            this.Mxx.Click += new System.EventHandler(this.CodeGeneratorChanges_Click);
            // 
            // Wi
            // 
            this.Wi.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Wi.Format = BlueBasics.Enums.enDataFormat.Ganzzahl;
            this.Wi.Location = new System.Drawing.Point(48, 16);
            this.Wi.Name = "Wi";
            this.Wi.Size = new System.Drawing.Size(40, 24);
            this.Wi.TabIndex = 4;
            this.Wi.Text = "800";
            this.Wi.TextChanged += new System.EventHandler(this.CodeGeneratorChanges_TextChanged);
            // 
            // He
            // 
            this.He.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.He.Format = BlueBasics.Enums.enDataFormat.Ganzzahl;
            this.He.Location = new System.Drawing.Point(152, 16);
            this.He.Name = "He";
            this.He.Size = new System.Drawing.Size(40, 24);
            this.He.TabIndex = 5;
            this.He.Text = "600";
            this.He.TextChanged += new System.EventHandler(this.CodeGeneratorChanges_TextChanged);
            // 
            // ex2
            // 
            this.ex2.Controls.Add(this.BefE2);
            this.ex2.Location = new System.Drawing.Point(4, 25);
            this.ex2.Name = "ex2";
            this.ex2.Padding = new System.Windows.Forms.Padding(3);
            this.ex2.Size = new System.Drawing.Size(464, 256);
            this.ex2.TabIndex = 2;
            this.ex2.Text = "Allgemeine Steuercodes";
            this.ex2.UseVisualStyleBackColor = true;
            // 
            // BefE2
            // 
            this.BefE2.AddAllowed = BlueControls.Enums.enAddType.OnlySuggests;
            this.BefE2.LastFilePath = null;
            this.BefE2.Location = new System.Drawing.Point(0, 16);
            this.BefE2.Name = "BefE2";
            this.BefE2.QuickInfo = "";
            this.BefE2.Size = new System.Drawing.Size(456, 232);
            this.BefE2.TabIndex = 0;
            this.BefE2.ItemCheckedChanged += new System.EventHandler(this.BefE2_Item_CheckedChanged);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.LayBearb);
            this.BlueFrame1.Controls.Add(this.LayVZ);
            this.BlueFrame1.Controls.Add(this.LayOpen);
            this.BlueFrame1.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame1.Location = new System.Drawing.Point(840, 0);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(336, 81);
            this.BlueFrame1.Text = "Externe Layouts aus dem Dateisytem";
            // 
            // LayBearb
            // 
            this.LayBearb.ImageCode = "Textdatei";
            this.LayBearb.Location = new System.Drawing.Point(8, 2);
            this.LayBearb.Name = "LayBearb";
            this.LayBearb.QuickInfo = "Layout mit Texteditor bearbeiten";
            this.LayBearb.Size = new System.Drawing.Size(80, 66);
            this.LayBearb.TabIndex = 81;
            this.LayBearb.Text = "Texteditor";
            this.LayBearb.Click += new System.EventHandler(this.LayBearb_Click);
            // 
            // LayVZ
            // 
            this.LayVZ.ImageCode = "Ordner|16";
            this.LayVZ.Location = new System.Drawing.Point(96, 46);
            this.LayVZ.Name = "LayVZ";
            this.LayVZ.Size = new System.Drawing.Size(232, 22);
            this.LayVZ.TabIndex = 84;
            this.LayVZ.Text = "Layout-Verzeichnis öffnen";
            this.LayVZ.Click += new System.EventHandler(this.LayVZ_Click);
            // 
            // LayOpen
            // 
            this.LayOpen.ImageCode = "Anwendung|16";
            this.LayOpen.Location = new System.Drawing.Point(96, 2);
            this.LayOpen.Name = "LayOpen";
            this.LayOpen.Size = new System.Drawing.Size(232, 22);
            this.LayOpen.TabIndex = 83;
            this.LayOpen.Text = "Layout mit Std.-Anwendung öffnen";
            this.LayOpen.Click += new System.EventHandler(this.LayOpen_Click);
            // 
            // grpDateiSystem
            // 
            this.grpDateiSystem.CausesValidation = false;
            this.grpDateiSystem.Controls.Add(this.btnLastFiles);
            this.grpDateiSystem.Controls.Add(this.btnNeu);
            this.grpDateiSystem.Controls.Add(this.btnOeffnen);
            this.grpDateiSystem.Controls.Add(this.btnSpeichern);
            this.grpDateiSystem.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDateiSystem.Location = new System.Drawing.Point(0, 0);
            this.grpDateiSystem.Name = "grpDateiSystem";
            this.grpDateiSystem.Size = new System.Drawing.Size(312, 81);
            this.grpDateiSystem.Text = "Dateisystem";
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
            this.btnNeu.Size = new System.Drawing.Size(64, 66);
            this.btnNeu.TabIndex = 10;
            this.btnNeu.Text = "Neu";
            this.btnNeu.Click += new System.EventHandler(this.btnNeu_Click);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(72, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.QuickInfo = "Eine Datei von ihrem<br>Computer öffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(64, 66);
            this.btnOeffnen.TabIndex = 9;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // btnSpeichern
            // 
            this.btnSpeichern.ImageCode = "Diskette";
            this.btnSpeichern.Location = new System.Drawing.Point(240, 2);
            this.btnSpeichern.Name = "btnSpeichern";
            this.btnSpeichern.Size = new System.Drawing.Size(64, 66);
            this.btnSpeichern.TabIndex = 8;
            this.btnSpeichern.Text = "Speichern";
            this.btnSpeichern.Click += new System.EventHandler(this.btnSpeichern_Click);
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
            // tabRightSide
            // 
            this.tabRightSide.Controls.Add(this.tabElementEigenschaften);
            this.tabRightSide.Controls.Add(this.tabCodeGenerator);
            this.tabRightSide.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabRightSide.HotTrack = true;
            this.tabRightSide.Location = new System.Drawing.Point(816, 110);
            this.tabRightSide.Name = "tabRightSide";
            this.tabRightSide.Size = new System.Drawing.Size(524, 502);
            this.tabRightSide.TabIndex = 3;
            // 
            // tabElementEigenschaften
            // 
            this.tabElementEigenschaften.Location = new System.Drawing.Point(4, 25);
            this.tabElementEigenschaften.Name = "tabElementEigenschaften";
            this.tabElementEigenschaften.Size = new System.Drawing.Size(516, 473);
            this.tabElementEigenschaften.TabIndex = 0;
            this.tabElementEigenschaften.Text = "Element-Eigenschaften";
            // 
            // tabCodeGenerator
            // 
            this.tabCodeGenerator.Controls.Add(this.groupBoxX);
            this.tabCodeGenerator.Location = new System.Drawing.Point(4, 25);
            this.tabCodeGenerator.Name = "tabCodeGenerator";
            this.tabCodeGenerator.Size = new System.Drawing.Size(516, 473);
            this.tabCodeGenerator.TabIndex = 1;
            this.tabCodeGenerator.Text = "Code-Generator";
            // 
            // LayoutDesigner
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1340, 612);
            this.Controls.Add(this.tabRightSide);
            this.Name = "LayoutDesigner";
            this.Text = "Druck-Layout";
            this.TopMost = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.Ribbon, 0);
            this.Controls.SetChildIndex(this.tabRightSide, 0);
            this.Controls.SetChildIndex(this.Pad, 0);
            this.Ribbon.ResumeLayout(false);
            this.tabPageControl.ResumeLayout(false);
            this.tabPageStart.ResumeLayout(false);
            this.Area_Drucken.ResumeLayout(false);
            this.Area_Design.ResumeLayout(false);
            this.DatenbankVER.ResumeLayout(false);
            this.groupBoxX.ResumeLayout(false);
            this.FomAx.ResumeLayout(false);
            this.txt.ResumeLayout(false);
            this.Pic.ResumeLayout(false);
            this.Abma.ResumeLayout(false);
            this.ex2.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.grpDateiSystem.ResumeLayout(false);
            this.tabRightSide.ResumeLayout(false);
            this.tabCodeGenerator.ResumeLayout(false);
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
        private GroupBox groupBoxX;
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
        protected internal GroupBox grpDateiSystem;
        private LastFilesCombo btnLastFiles;
        private Button btnNeu;
        private Button btnOeffnen;
        private Button btnSpeichern;
        private System.Windows.Forms.OpenFileDialog LoadTab;
        private System.Windows.Forms.SaveFileDialog SaveTab;
        private AbstractTabControl tabRightSide;
        private TabPage tabElementEigenschaften;
        private TabPage tabCodeGenerator;
    }
}
