using System.Diagnostics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
	{

	
		internal sealed partial class ColumnEditor : Form
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				//if (disposing && components != null)
				//{
				//	components.Dispose();
				//}
				base.Dispose(disposing);
			}


			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColumnEditor));
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.tabDesign = new BlueControls.Controls.TabPage();
            this.txbPrefix = new BlueControls.Controls.TextBox();
            this.grbBildCode = new BlueControls.Controls.GroupBox();
            this.cbxBildCodeImageNotfound = new BlueControls.Controls.ComboBox();
            this.capBildCodeImageNotfound = new BlueControls.Controls.Caption();
            this.capBildCodeConstHeight = new BlueControls.Controls.Caption();
            this.txbBildCodeConstHeight = new BlueControls.Controls.TextBox();
            this.caption7 = new BlueControls.Controls.Caption();
            this.capUeberschrift3 = new BlueControls.Controls.Caption();
            this.capUeberschrift2 = new BlueControls.Controls.Caption();
            this.capUeberschrift1 = new BlueControls.Controls.Caption();
            this.txbUeberschift3 = new BlueControls.Controls.TextBox();
            this.txbUeberschift2 = new BlueControls.Controls.TextBox();
            this.txbUeberschift1 = new BlueControls.Controls.TextBox();
            this.txbReplacer = new BlueControls.Controls.TextBox();
            this.capReplacer = new BlueControls.Controls.Caption();
            this.btnEinzeiligDarstellen = new BlueControls.Controls.Button();
            this.capEinheit = new BlueControls.Controls.Caption();
            this.cbxEinheit = new BlueControls.Controls.ComboBox();
            this.picCaptionImage = new BlueControls.Controls.EasyPic();
            this.Caption6 = new BlueControls.Controls.Caption();
            this.btnKompakteAnzeige = new BlueControls.Controls.Button();
            this.cbxFormat = new BlueControls.Controls.ComboBox();
            this.Caption16 = new BlueControls.Controls.Caption();
            this.H_Colorx = new BlueControls.Controls.Button();
            this.cbxRandRechts = new BlueControls.Controls.ComboBox();
            this.T_Colorx = new BlueControls.Controls.Button();
            this.cbxRandLinks = new BlueControls.Controls.ComboBox();
            this.Caption1 = new BlueControls.Controls.Caption();
            this.Caption4 = new BlueControls.Controls.Caption();
            this.btnMultiline = new BlueControls.Controls.Button();
            this.tabRechte = new BlueControls.Controls.TabPage();
            this.btnIgnoreLock = new BlueControls.Controls.Button();
            this.btnOtherValuesToo = new BlueControls.Controls.Button();
            this.lbxCellEditor = new BlueControls.Controls.ListBox();
            this.btnEditableStandard = new BlueControls.Controls.Button();
            this.tbxAuswaehlbareWerte = new BlueControls.Controls.TextBox();
            this.Caption9 = new BlueControls.Controls.Caption();
            this.Caption15 = new BlueControls.Controls.Caption();
            this.btnCanBeEmpty = new BlueControls.Controls.Button();
            this.btnEditableDropdown = new BlueControls.Controls.Button();
            this.tabAutoBearbeitung = new BlueControls.Controls.TabPage();
            this.tbxInitValue = new BlueControls.Controls.TextBox();
            this.Caption12 = new BlueControls.Controls.Caption();
            this.btnAutoEditKleineFehler = new BlueControls.Controls.Button();
            this.btnAutoEditToUpper = new BlueControls.Controls.Button();
            this.tbxRunden = new BlueControls.Controls.TextBox();
            this.Caption11 = new BlueControls.Controls.Caption();
            this.btnAutoEditAutoSort = new BlueControls.Controls.Button();
            this.tabFilter = new BlueControls.Controls.TabPage();
            this.Caption10 = new BlueControls.Controls.Caption();
            this.tbxJoker = new BlueControls.Controls.TextBox();
            this.ZeilenFilter = new BlueControls.Controls.Button();
            this.AutoFilterMöglich = new BlueControls.Controls.Button();
            this.AutoFilterTXT = new BlueControls.Controls.Button();
            this.AutoFilterErw = new BlueControls.Controls.Button();
            this.tabQuickInfo = new BlueControls.Controls.TabPage();
            this.tbxAdminInfo = new BlueControls.Controls.TextBox();
            this.tbxQuickinfo = new BlueControls.Controls.TextBox();
            this.Caption18 = new BlueControls.Controls.Caption();
            this.Caption17 = new BlueControls.Controls.Caption();
            this.QI_Vorschau = new BlueControls.Controls.Button();
            this.tabSonstiges = new BlueControls.Controls.TabPage();
            this.txbLinkedKeyKennung = new BlueControls.Controls.TextBox();
            this.capLinkedKeyKennung = new BlueControls.Controls.Caption();
            this.cbxLinkedDatabase = new BlueControls.Controls.ComboBox();
            this.capLinkedDatabase = new BlueControls.Controls.Caption();
            this.capBestFileStandardSuffix = new BlueControls.Controls.Caption();
            this.txbBestFileStandardSuffix = new BlueControls.Controls.TextBox();
            this.capBestFileStandardFolder = new BlueControls.Controls.Caption();
            this.txbBestFileStandardFolder = new BlueControls.Controls.TextBox();
            this.btnSpellChecking = new BlueControls.Controls.Button();
            this.btnLogUndo = new BlueControls.Controls.Button();
            this.tbxAllowedChars = new BlueControls.Controls.TextBox();
            this.Caption13 = new BlueControls.Controls.Caption();
            this.tbxTags = new BlueControls.Controls.TextBox();
            this.Caption8 = new BlueControls.Controls.Caption();
            this.BlueFrame1 = new BlueControls.Controls.GroupBox();
            this.capInfo = new BlueControls.Controls.Caption();
            this.Caption3 = new BlueControls.Controls.Caption();
            this.tbxName = new BlueControls.Controls.TextBox();
            this.Caption2 = new BlueControls.Controls.Caption();
            this.tbxCaption = new BlueControls.Controls.TextBox();
            this.btnVor = new BlueControls.Controls.Button();
            this.btnZurueck = new BlueControls.Controls.Button();
            this.btnOk = new BlueControls.Controls.Button();
            this.tabControl = new BlueControls.Controls.TabControl();
            this.tabRegeln = new BlueControls.Controls.TabPage();
            this.gpxVerlinkteZellen = new BlueControls.Controls.GroupBox();
            this.caption5 = new BlueControls.Controls.Caption();
            this.txbZeichenkette = new BlueControls.Controls.TextBox();
            this.capZeichenkette = new BlueControls.Controls.TextBox();
            this.line1 = new BlueControls.Controls.Line();
            this.cbxTargetColumn = new BlueControls.Controls.ComboBox();
            this.cbxColumnKeyInColumn = new BlueControls.Controls.ComboBox();
            this.cbxRowKeyInColumn = new BlueControls.Controls.ComboBox();
            this.btnTargetColumn = new BlueControls.Controls.Button();
            this.btnColumnKeyInColumn = new BlueControls.Controls.Button();
            this.capRowKeyInColumn = new BlueControls.Controls.Caption();
            this.btnFehlerWennUnsichtbare = new BlueControls.Controls.Button();
            this.btnFormatFehler = new BlueControls.Controls.Button();
            this.btnFehlerWennLeer = new BlueControls.Controls.Button();
            this.cbxFehlendesZiel = new BlueControls.Controls.ComboBox();
            this.tabDesign.SuspendLayout();
            this.grbBildCode.SuspendLayout();
            this.tabRechte.SuspendLayout();
            this.tabAutoBearbeitung.SuspendLayout();
            this.tabFilter.SuspendLayout();
            this.tabQuickInfo.SuspendLayout();
            this.tabSonstiges.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabRegeln.SuspendLayout();
            this.gpxVerlinkteZellen.SuspendLayout();
            this.SuspendLayout();
            // 
            // ColorDia
            // 
            this.ColorDia.AnyColor = true;
            this.ColorDia.FullOpen = true;
            // 
            // tabDesign
            // 
            this.tabDesign.Controls.Add(this.txbPrefix);
            this.tabDesign.Controls.Add(this.grbBildCode);
            this.tabDesign.Controls.Add(this.caption7);
            this.tabDesign.Controls.Add(this.capUeberschrift3);
            this.tabDesign.Controls.Add(this.capUeberschrift2);
            this.tabDesign.Controls.Add(this.capUeberschrift1);
            this.tabDesign.Controls.Add(this.txbUeberschift3);
            this.tabDesign.Controls.Add(this.txbUeberschift2);
            this.tabDesign.Controls.Add(this.txbUeberschift1);
            this.tabDesign.Controls.Add(this.txbReplacer);
            this.tabDesign.Controls.Add(this.capReplacer);
            this.tabDesign.Controls.Add(this.btnEinzeiligDarstellen);
            this.tabDesign.Controls.Add(this.capEinheit);
            this.tabDesign.Controls.Add(this.cbxEinheit);
            this.tabDesign.Controls.Add(this.picCaptionImage);
            this.tabDesign.Controls.Add(this.Caption6);
            this.tabDesign.Controls.Add(this.btnKompakteAnzeige);
            this.tabDesign.Controls.Add(this.cbxFormat);
            this.tabDesign.Controls.Add(this.Caption16);
            this.tabDesign.Controls.Add(this.H_Colorx);
            this.tabDesign.Controls.Add(this.cbxRandRechts);
            this.tabDesign.Controls.Add(this.T_Colorx);
            this.tabDesign.Controls.Add(this.cbxRandLinks);
            this.tabDesign.Controls.Add(this.Caption1);
            this.tabDesign.Controls.Add(this.Caption4);
            this.tabDesign.Controls.Add(this.btnMultiline);
            this.tabDesign.Location = new System.Drawing.Point(4, 25);
            this.tabDesign.Name = "tabDesign";
            this.tabDesign.Padding = new System.Windows.Forms.Padding(3);
            this.tabDesign.Size = new System.Drawing.Size(912, 487);
            this.tabDesign.TabIndex = 0;
            this.tabDesign.Text = "Design";
            this.tabDesign.UseVisualStyleBackColor = true;
            // 
            // txbPrefix
            // 
            this.txbPrefix.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbPrefix.Location = new System.Drawing.Point(56, 96);
            this.txbPrefix.Name = "txbPrefix";
            this.txbPrefix.Size = new System.Drawing.Size(168, 24);
            this.txbPrefix.TabIndex = 36;
            // 
            // grbBildCode
            // 
            this.grbBildCode.CausesValidation = false;
            this.grbBildCode.Controls.Add(this.cbxBildCodeImageNotfound);
            this.grbBildCode.Controls.Add(this.capBildCodeImageNotfound);
            this.grbBildCode.Controls.Add(this.capBildCodeConstHeight);
            this.grbBildCode.Controls.Add(this.txbBildCodeConstHeight);
            this.grbBildCode.Location = new System.Drawing.Point(8, 384);
            this.grbBildCode.Name = "grbBildCode";
            this.grbBildCode.Size = new System.Drawing.Size(416, 88);
            this.grbBildCode.Text = "Fomat: BildCode";
            // 
            // cbxBildCodeImageNotfound
            // 
            this.cbxBildCodeImageNotfound.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxBildCodeImageNotfound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBildCodeImageNotfound.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxBildCodeImageNotfound.Location = new System.Drawing.Point(200, 40);
            this.cbxBildCodeImageNotfound.Name = "cbxBildCodeImageNotfound";
            this.cbxBildCodeImageNotfound.Size = new System.Drawing.Size(160, 24);
            this.cbxBildCodeImageNotfound.TabIndex = 34;
            // 
            // capBildCodeImageNotfound
            // 
            this.capBildCodeImageNotfound.CausesValidation = false;
            this.capBildCodeImageNotfound.Location = new System.Drawing.Point(8, 40);
            this.capBildCodeImageNotfound.Name = "capBildCodeImageNotfound";
            this.capBildCodeImageNotfound.Size = new System.Drawing.Size(184, 16);
            this.capBildCodeImageNotfound.Text = "Verhalten bei Fehler-Bildern:";
            // 
            // capBildCodeConstHeight
            // 
            this.capBildCodeConstHeight.CausesValidation = false;
            this.capBildCodeConstHeight.Location = new System.Drawing.Point(8, 16);
            this.capBildCodeConstHeight.Name = "capBildCodeConstHeight";
            this.capBildCodeConstHeight.Size = new System.Drawing.Size(184, 16);
            this.capBildCodeConstHeight.Text = "Konstante Höhe bei Bildern:";
            // 
            // txbBildCodeConstHeight
            // 
            this.txbBildCodeConstHeight.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbBildCodeConstHeight.Format = BlueBasics.Enums.enDataFormat.Ganzzahl;
            this.txbBildCodeConstHeight.Location = new System.Drawing.Point(200, 16);
            this.txbBildCodeConstHeight.Name = "txbBildCodeConstHeight";
            this.txbBildCodeConstHeight.Size = new System.Drawing.Size(64, 24);
            this.txbBildCodeConstHeight.Suffix = "Pixel";
            this.txbBildCodeConstHeight.TabIndex = 32;
            this.txbBildCodeConstHeight.Text = "0";
            // 
            // caption7
            // 
            this.caption7.CausesValidation = false;
            this.caption7.Location = new System.Drawing.Point(8, 96);
            this.caption7.Name = "caption7";
            this.caption7.Size = new System.Drawing.Size(48, 16);
            this.caption7.Text = "Präfix:";
            // 
            // capUeberschrift3
            // 
            this.capUeberschrift3.CausesValidation = false;
            this.capUeberschrift3.Location = new System.Drawing.Point(8, 216);
            this.capUeberschrift3.Name = "capUeberschrift3";
            this.capUeberschrift3.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift3.Text = "Überschrift 3:";
            this.capUeberschrift3.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift2
            // 
            this.capUeberschrift2.CausesValidation = false;
            this.capUeberschrift2.Location = new System.Drawing.Point(8, 192);
            this.capUeberschrift2.Name = "capUeberschrift2";
            this.capUeberschrift2.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift2.Text = "Überschrift 2:";
            this.capUeberschrift2.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift1
            // 
            this.capUeberschrift1.CausesValidation = false;
            this.capUeberschrift1.Location = new System.Drawing.Point(8, 168);
            this.capUeberschrift1.Name = "capUeberschrift1";
            this.capUeberschrift1.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift1.Text = "Überschrift 1:";
            this.capUeberschrift1.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbUeberschift3
            // 
            this.txbUeberschift3.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift3.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.txbUeberschift3.Location = new System.Drawing.Point(104, 216);
            this.txbUeberschift3.Name = "txbUeberschift3";
            this.txbUeberschift3.Size = new System.Drawing.Size(328, 24);
            this.txbUeberschift3.TabIndex = 38;
            // 
            // txbUeberschift2
            // 
            this.txbUeberschift2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift2.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.txbUeberschift2.Location = new System.Drawing.Point(104, 192);
            this.txbUeberschift2.Name = "txbUeberschift2";
            this.txbUeberschift2.Size = new System.Drawing.Size(328, 24);
            this.txbUeberschift2.TabIndex = 37;
            // 
            // txbUeberschift1
            // 
            this.txbUeberschift1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift1.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.txbUeberschift1.Location = new System.Drawing.Point(104, 168);
            this.txbUeberschift1.Name = "txbUeberschift1";
            this.txbUeberschift1.Size = new System.Drawing.Size(328, 24);
            this.txbUeberschift1.TabIndex = 36;
            // 
            // txbReplacer
            // 
            this.txbReplacer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbReplacer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbReplacer.Location = new System.Drawing.Point(440, 240);
            this.txbReplacer.MultiLine = true;
            this.txbReplacer.Name = "txbReplacer";
            this.txbReplacer.QuickInfo = "Texte in der Spalte werden mit diesen Angaben <b>optisch</b> ersetzt.<br><i><u>Be" +
    "ispiel:</i></u>Beispiel-Text|Bsp.-Txt";
            this.txbReplacer.Size = new System.Drawing.Size(464, 245);
            this.txbReplacer.SpellChecking = true;
            this.txbReplacer.TabIndex = 35;
            // 
            // capReplacer
            // 
            this.capReplacer.CausesValidation = false;
            this.capReplacer.Location = new System.Drawing.Point(440, 224);
            this.capReplacer.Name = "capReplacer";
            this.capReplacer.Size = new System.Drawing.Size(144, 24);
            this.capReplacer.Text = "Ersetzungen:";
            // 
            // btnEinzeiligDarstellen
            // 
            this.btnEinzeiligDarstellen.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnEinzeiligDarstellen.Location = new System.Drawing.Point(8, 272);
            this.btnEinzeiligDarstellen.Name = "btnEinzeiligDarstellen";
            this.btnEinzeiligDarstellen.Size = new System.Drawing.Size(296, 24);
            this.btnEinzeiligDarstellen.TabIndex = 29;
            this.btnEinzeiligDarstellen.Text = "Mehrzeilig einzeilig darstellen";
            // 
            // capEinheit
            // 
            this.capEinheit.CausesValidation = false;
            this.capEinheit.Location = new System.Drawing.Point(8, 56);
            this.capEinheit.Name = "capEinheit";
            this.capEinheit.Size = new System.Drawing.Size(48, 32);
            this.capEinheit.Text = "Einheit:<br>(Suffix)";
            // 
            // cbxEinheit
            // 
            this.cbxEinheit.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxEinheit.Location = new System.Drawing.Point(56, 56);
            this.cbxEinheit.Name = "cbxEinheit";
            this.cbxEinheit.Size = new System.Drawing.Size(168, 24);
            this.cbxEinheit.TabIndex = 31;
            // 
            // picCaptionImage
            // 
            this.picCaptionImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picCaptionImage.CausesValidation = false;
            this.picCaptionImage.Location = new System.Drawing.Point(536, 24);
            this.picCaptionImage.MaxSize = 300;
            this.picCaptionImage.Name = "picCaptionImage";
            this.picCaptionImage.Size = new System.Drawing.Size(368, 184);
            // 
            // Caption6
            // 
            this.Caption6.CausesValidation = false;
            this.Caption6.Location = new System.Drawing.Point(536, 8);
            this.Caption6.Name = "Caption6";
            this.Caption6.Size = new System.Drawing.Size(216, 24);
            this.Caption6.Text = "Spaltenbild:";
            this.Caption6.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnKompakteAnzeige
            // 
            this.btnKompakteAnzeige.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnKompakteAnzeige.Location = new System.Drawing.Point(8, 248);
            this.btnKompakteAnzeige.Name = "btnKompakteAnzeige";
            this.btnKompakteAnzeige.Size = new System.Drawing.Size(296, 24);
            this.btnKompakteAnzeige.TabIndex = 28;
            this.btnKompakteAnzeige.Text = "Kompakte Anzeige verwenden";
            // 
            // cbxFormat
            // 
            this.cbxFormat.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFormat.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxFormat.Location = new System.Drawing.Point(8, 24);
            this.cbxFormat.Name = "cbxFormat";
            this.cbxFormat.Size = new System.Drawing.Size(216, 24);
            this.cbxFormat.TabIndex = 27;
            this.cbxFormat.TextChanged += new System.EventHandler(this.cbxFormat_TextChanged);
            // 
            // Caption16
            // 
            this.Caption16.CausesValidation = false;
            this.Caption16.Location = new System.Drawing.Point(8, 8);
            this.Caption16.Name = "Caption16";
            this.Caption16.Size = new System.Drawing.Size(136, 16);
            this.Caption16.Text = "<b><u>Format:";
            this.Caption16.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // H_Colorx
            // 
            this.H_Colorx.Location = new System.Drawing.Point(248, 40);
            this.H_Colorx.Name = "H_Colorx";
            this.H_Colorx.Size = new System.Drawing.Size(144, 24);
            this.H_Colorx.TabIndex = 3;
            this.H_Colorx.Text = "Hintergrundfarbe";
            this.H_Colorx.Click += new System.EventHandler(this.H_Color_Click);
            // 
            // cbxRandRechts
            // 
            this.cbxRandRechts.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRandRechts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRandRechts.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxRandRechts.Location = new System.Drawing.Point(336, 96);
            this.cbxRandRechts.Name = "cbxRandRechts";
            this.cbxRandRechts.Size = new System.Drawing.Size(184, 24);
            this.cbxRandRechts.TabIndex = 25;
            // 
            // T_Colorx
            // 
            this.T_Colorx.Location = new System.Drawing.Point(248, 16);
            this.T_Colorx.Name = "T_Colorx";
            this.T_Colorx.Size = new System.Drawing.Size(144, 24);
            this.T_Colorx.TabIndex = 4;
            this.T_Colorx.Text = "Textfarbe";
            this.T_Colorx.Click += new System.EventHandler(this.T_Color_Click);
            // 
            // cbxRandLinks
            // 
            this.cbxRandLinks.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRandLinks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRandLinks.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.cbxRandLinks.Location = new System.Drawing.Point(336, 72);
            this.cbxRandLinks.Name = "cbxRandLinks";
            this.cbxRandLinks.Size = new System.Drawing.Size(184, 24);
            this.cbxRandLinks.TabIndex = 24;
            // 
            // Caption1
            // 
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new System.Drawing.Point(248, 72);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new System.Drawing.Size(80, 16);
            this.Caption1.Text = "Linker Rand:";
            this.Caption1.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption4
            // 
            this.Caption4.CausesValidation = false;
            this.Caption4.Location = new System.Drawing.Point(248, 96);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new System.Drawing.Size(88, 16);
            this.Caption4.Text = "Rechter Rand:";
            this.Caption4.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnMultiline
            // 
            this.btnMultiline.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnMultiline.Location = new System.Drawing.Point(8, 128);
            this.btnMultiline.Name = "btnMultiline";
            this.btnMultiline.Size = new System.Drawing.Size(216, 32);
            this.btnMultiline.TabIndex = 7;
            this.btnMultiline.Text = "Mehrere Einträge pro Zelle erlaubt (mehrzeilig)";
            // 
            // tabRechte
            // 
            this.tabRechte.Controls.Add(this.btnIgnoreLock);
            this.tabRechte.Controls.Add(this.btnOtherValuesToo);
            this.tabRechte.Controls.Add(this.lbxCellEditor);
            this.tabRechte.Controls.Add(this.btnEditableStandard);
            this.tabRechte.Controls.Add(this.tbxAuswaehlbareWerte);
            this.tabRechte.Controls.Add(this.Caption9);
            this.tabRechte.Controls.Add(this.Caption15);
            this.tabRechte.Controls.Add(this.btnCanBeEmpty);
            this.tabRechte.Controls.Add(this.btnEditableDropdown);
            this.tabRechte.Location = new System.Drawing.Point(4, 25);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Padding = new System.Windows.Forms.Padding(3);
            this.tabRechte.Size = new System.Drawing.Size(912, 487);
            this.tabRechte.TabIndex = 1;
            this.tabRechte.Text = "Rechte";
            this.tabRechte.UseVisualStyleBackColor = true;
            // 
            // btnIgnoreLock
            // 
            this.btnIgnoreLock.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnIgnoreLock.Location = new System.Drawing.Point(400, 200);
            this.btnIgnoreLock.Name = "btnIgnoreLock";
            this.btnIgnoreLock.Size = new System.Drawing.Size(288, 40);
            this.btnIgnoreLock.TabIndex = 27;
            this.btnIgnoreLock.Text = "Die Bearbeitung ist auch möglich, wenn die Zeile abgeschlossen ist.";
            // 
            // btnOtherValuesToo
            // 
            this.btnOtherValuesToo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOtherValuesToo.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnOtherValuesToo.Location = new System.Drawing.Point(32, 447);
            this.btnOtherValuesToo.Name = "btnOtherValuesToo";
            this.btnOtherValuesToo.Size = new System.Drawing.Size(368, 32);
            this.btnOtherValuesToo.TabIndex = 7;
            this.btnOtherValuesToo.Text = "Auch Werte, die in anderen Zellen derselben Spalte vorhanden sind, werden zum Aus" +
    "wählen vorgschlagen.";
            // 
            // lbxCellEditor
            // 
            this.lbxCellEditor.AddAllowed = BlueControls.Enums.enAddType.Text;
            this.lbxCellEditor.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.lbxCellEditor.FilterAllowed = true;
            this.lbxCellEditor.LastFilePath = null;
            this.lbxCellEditor.Location = new System.Drawing.Point(400, 48);
            this.lbxCellEditor.Name = "lbxCellEditor";
            this.lbxCellEditor.QuickInfo = "";
            this.lbxCellEditor.RemoveAllowed = true;
            this.lbxCellEditor.Size = new System.Drawing.Size(264, 144);
            this.lbxCellEditor.TabIndex = 26;
            // 
            // btnEditableStandard
            // 
            this.btnEditableStandard.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnEditableStandard.Location = new System.Drawing.Point(8, 16);
            this.btnEditableStandard.Name = "btnEditableStandard";
            this.btnEditableStandard.Size = new System.Drawing.Size(384, 48);
            this.btnEditableStandard.TabIndex = 4;
            this.btnEditableStandard.Text = "Benutzer-Bearbeitung mit der <b>Standard-Methode</b> erlaubt<br><i>Wenn neue Wert" +
    "e erlaubt sein sollen, muss hier ein Häkchen gesetzt werden.";
            // 
            // tbxAuswaehlbareWerte
            // 
            this.tbxAuswaehlbareWerte.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tbxAuswaehlbareWerte.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxAuswaehlbareWerte.Location = new System.Drawing.Point(32, 120);
            this.tbxAuswaehlbareWerte.MultiLine = true;
            this.tbxAuswaehlbareWerte.Name = "tbxAuswaehlbareWerte";
            this.tbxAuswaehlbareWerte.Size = new System.Drawing.Size(360, 311);
            this.tbxAuswaehlbareWerte.SpellChecking = true;
            this.tbxAuswaehlbareWerte.TabIndex = 0;
            // 
            // Caption9
            // 
            this.Caption9.CausesValidation = false;
            this.Caption9.Location = new System.Drawing.Point(400, 16);
            this.Caption9.Name = "Caption9";
            this.Caption9.Size = new System.Drawing.Size(264, 32);
            this.Caption9.Text = "<b>Folgende Benutzergruppen dürfen den Inhalt der Zellen bearbeiten:";
            this.Caption9.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption15
            // 
            this.Caption15.CausesValidation = false;
            this.Caption15.Location = new System.Drawing.Point(32, 104);
            this.Caption15.Name = "Caption15";
            this.Caption15.Size = new System.Drawing.Size(216, 16);
            this.Caption15.Text = "<b><u>Immer auswählbare Werte:";
            this.Caption15.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnCanBeEmpty
            // 
            this.btnCanBeEmpty.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnCanBeEmpty.Location = new System.Drawing.Point(32, 80);
            this.btnCanBeEmpty.Name = "btnCanBeEmpty";
            this.btnCanBeEmpty.Size = new System.Drawing.Size(184, 16);
            this.btnCanBeEmpty.TabIndex = 6;
            this.btnCanBeEmpty.Text = "Alles abwählen erlaubt";
            // 
            // btnEditableDropdown
            // 
            this.btnEditableDropdown.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnEditableDropdown.Location = new System.Drawing.Point(8, 64);
            this.btnEditableDropdown.Name = "btnEditableDropdown";
            this.btnEditableDropdown.Size = new System.Drawing.Size(328, 16);
            this.btnEditableDropdown.TabIndex = 5;
            this.btnEditableDropdown.Text = "Benutzer-Bearbeitung mit <b>Auswahl-Menü</b> erlaubt";
            // 
            // tabAutoBearbeitung
            // 
            this.tabAutoBearbeitung.Controls.Add(this.tbxInitValue);
            this.tabAutoBearbeitung.Controls.Add(this.Caption12);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditKleineFehler);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditToUpper);
            this.tabAutoBearbeitung.Controls.Add(this.tbxRunden);
            this.tabAutoBearbeitung.Controls.Add(this.Caption11);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditAutoSort);
            this.tabAutoBearbeitung.Location = new System.Drawing.Point(4, 25);
            this.tabAutoBearbeitung.Name = "tabAutoBearbeitung";
            this.tabAutoBearbeitung.Size = new System.Drawing.Size(912, 487);
            this.tabAutoBearbeitung.TabIndex = 6;
            this.tabAutoBearbeitung.Text = "Auto-Bearbeitung";
            // 
            // tbxInitValue
            // 
            this.tbxInitValue.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxInitValue.Location = new System.Drawing.Point(8, 32);
            this.tbxInitValue.Name = "tbxInitValue";
            this.tbxInitValue.Size = new System.Drawing.Size(568, 24);
            this.tbxInitValue.TabIndex = 15;
            // 
            // Caption12
            // 
            this.Caption12.CausesValidation = false;
            this.Caption12.Location = new System.Drawing.Point(8, 16);
            this.Caption12.Name = "Caption12";
            this.Caption12.Size = new System.Drawing.Size(568, 16);
            this.Caption12.Text = "Wenn eine neue Zeile erstellt wird, folgenden Wert in die Zelle schreiben:";
            // 
            // btnAutoEditKleineFehler
            // 
            this.btnAutoEditKleineFehler.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnAutoEditKleineFehler.Location = new System.Drawing.Point(8, 160);
            this.btnAutoEditKleineFehler.Name = "btnAutoEditKleineFehler";
            this.btnAutoEditKleineFehler.Size = new System.Drawing.Size(480, 24);
            this.btnAutoEditKleineFehler.TabIndex = 13;
            this.btnAutoEditKleineFehler.Text = "Kleinere Fehler, wie z.B. doppelte Leerzeichen automatisch korrigieren";
            // 
            // btnAutoEditToUpper
            // 
            this.btnAutoEditToUpper.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnAutoEditToUpper.Location = new System.Drawing.Point(8, 96);
            this.btnAutoEditToUpper.Name = "btnAutoEditToUpper";
            this.btnAutoEditToUpper.Size = new System.Drawing.Size(416, 24);
            this.btnAutoEditToUpper.TabIndex = 12;
            this.btnAutoEditToUpper.Text = "Texte in Grossbuchstaben ändern";
            // 
            // tbxRunden
            // 
            this.tbxRunden.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxRunden.Location = new System.Drawing.Point(224, 128);
            this.tbxRunden.Name = "tbxRunden";
            this.tbxRunden.Size = new System.Drawing.Size(88, 24);
            this.tbxRunden.TabIndex = 11;
            // 
            // Caption11
            // 
            this.Caption11.CausesValidation = false;
            this.Caption11.Location = new System.Drawing.Point(8, 128);
            this.Caption11.Name = "Caption11";
            this.Caption11.Size = new System.Drawing.Size(328, 16);
            this.Caption11.Text = "Zahlen runden auf Kommastellen:";
            // 
            // btnAutoEditAutoSort
            // 
            this.btnAutoEditAutoSort.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnAutoEditAutoSort.Location = new System.Drawing.Point(8, 200);
            this.btnAutoEditAutoSort.Name = "btnAutoEditAutoSort";
            this.btnAutoEditAutoSort.Size = new System.Drawing.Size(416, 24);
            this.btnAutoEditAutoSort.TabIndex = 10;
            this.btnAutoEditAutoSort.Text = "Mehrzeilige Einträge sortieren und doppelte entfernen";
            // 
            // tabFilter
            // 
            this.tabFilter.Controls.Add(this.Caption10);
            this.tabFilter.Controls.Add(this.tbxJoker);
            this.tabFilter.Controls.Add(this.ZeilenFilter);
            this.tabFilter.Controls.Add(this.AutoFilterMöglich);
            this.tabFilter.Controls.Add(this.AutoFilterTXT);
            this.tabFilter.Controls.Add(this.AutoFilterErw);
            this.tabFilter.Location = new System.Drawing.Point(4, 25);
            this.tabFilter.Name = "tabFilter";
            this.tabFilter.Padding = new System.Windows.Forms.Padding(3);
            this.tabFilter.Size = new System.Drawing.Size(912, 487);
            this.tabFilter.TabIndex = 2;
            this.tabFilter.Text = "Filter";
            this.tabFilter.UseVisualStyleBackColor = true;
            // 
            // Caption10
            // 
            this.Caption10.CausesValidation = false;
            this.Caption10.Location = new System.Drawing.Point(8, 104);
            this.Caption10.Name = "Caption10";
            this.Caption10.Size = new System.Drawing.Size(312, 56);
            this.Caption10.Text = "Bei Autofilter-Aktionen, Zellen mit folgenden Inhalt <b>immer</b> anzeigen, wenn " +
    "ein Wert gewählt wurde:<br>(Joker)";
            this.Caption10.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tbxJoker
            // 
            this.tbxJoker.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxJoker.Location = new System.Drawing.Point(8, 160);
            this.tbxJoker.Name = "tbxJoker";
            this.tbxJoker.Size = new System.Drawing.Size(312, 24);
            this.tbxJoker.TabIndex = 7;
            // 
            // ZeilenFilter
            // 
            this.ZeilenFilter.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.ZeilenFilter.Location = new System.Drawing.Point(12, 71);
            this.ZeilenFilter.Name = "ZeilenFilter";
            this.ZeilenFilter.Size = new System.Drawing.Size(304, 16);
            this.ZeilenFilter.TabIndex = 6;
            this.ZeilenFilter.Text = "Bei Zeilenfilter ignorieren (Suchfeld-Eingabe)";
            // 
            // AutoFilterMöglich
            // 
            this.AutoFilterMöglich.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.AutoFilterMöglich.Location = new System.Drawing.Point(12, 15);
            this.AutoFilterMöglich.Name = "AutoFilterMöglich";
            this.AutoFilterMöglich.Size = new System.Drawing.Size(120, 16);
            this.AutoFilterMöglich.TabIndex = 3;
            this.AutoFilterMöglich.Text = "AutoFilter erlaubt";
            // 
            // AutoFilterTXT
            // 
            this.AutoFilterTXT.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.AutoFilterTXT.Location = new System.Drawing.Point(28, 31);
            this.AutoFilterTXT.Name = "AutoFilterTXT";
            this.AutoFilterTXT.Size = new System.Drawing.Size(208, 16);
            this.AutoFilterTXT.TabIndex = 4;
            this.AutoFilterTXT.Text = "AutoFilter - Texteingabe - erlaubt";
            // 
            // AutoFilterErw
            // 
            this.AutoFilterErw.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.AutoFilterErw.Location = new System.Drawing.Point(28, 47);
            this.AutoFilterErw.Name = "AutoFilterErw";
            this.AutoFilterErw.Size = new System.Drawing.Size(192, 16);
            this.AutoFilterErw.TabIndex = 5;
            this.AutoFilterErw.Text = "AutoFilter - Erweitert - erlaubt";
            // 
            // tabQuickInfo
            // 
            this.tabQuickInfo.Controls.Add(this.tbxAdminInfo);
            this.tabQuickInfo.Controls.Add(this.tbxQuickinfo);
            this.tabQuickInfo.Controls.Add(this.Caption18);
            this.tabQuickInfo.Controls.Add(this.Caption17);
            this.tabQuickInfo.Controls.Add(this.QI_Vorschau);
            this.tabQuickInfo.Location = new System.Drawing.Point(4, 25);
            this.tabQuickInfo.Name = "tabQuickInfo";
            this.tabQuickInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabQuickInfo.Size = new System.Drawing.Size(912, 487);
            this.tabQuickInfo.TabIndex = 3;
            this.tabQuickInfo.Text = "Quickinfo";
            this.tabQuickInfo.UseVisualStyleBackColor = true;
            // 
            // tbxAdminInfo
            // 
            this.tbxAdminInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAdminInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxAdminInfo.Location = new System.Drawing.Point(560, 24);
            this.tbxAdminInfo.MultiLine = true;
            this.tbxAdminInfo.Name = "tbxAdminInfo";
            this.tbxAdminInfo.Size = new System.Drawing.Size(344, 428);
            this.tbxAdminInfo.SpellChecking = true;
            this.tbxAdminInfo.TabIndex = 3;
            this.tbxAdminInfo.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tbxQuickinfo
            // 
            this.tbxQuickinfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxQuickinfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxQuickinfo.Location = new System.Drawing.Point(8, 24);
            this.tbxQuickinfo.MultiLine = true;
            this.tbxQuickinfo.Name = "tbxQuickinfo";
            this.tbxQuickinfo.Size = new System.Drawing.Size(544, 428);
            this.tbxQuickinfo.SpellChecking = true;
            this.tbxQuickinfo.TabIndex = 0;
            this.tbxQuickinfo.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption18
            // 
            this.Caption18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Caption18.CausesValidation = false;
            this.Caption18.Location = new System.Drawing.Point(560, 8);
            this.Caption18.Name = "Caption18";
            this.Caption18.Size = new System.Drawing.Size(188, 15);
            this.Caption18.Text = "Administrator-Info:";
            this.Caption18.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption17
            // 
            this.Caption17.CausesValidation = false;
            this.Caption17.Location = new System.Drawing.Point(8, 8);
            this.Caption17.Name = "Caption17";
            this.Caption17.Size = new System.Drawing.Size(168, 16);
            this.Caption17.Text = "QuickInfo:";
            this.Caption17.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // QI_Vorschau
            // 
            this.QI_Vorschau.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.QI_Vorschau.Location = new System.Drawing.Point(808, 460);
            this.QI_Vorschau.Name = "QI_Vorschau";
            this.QI_Vorschau.Size = new System.Drawing.Size(96, 24);
            this.QI_Vorschau.TabIndex = 1;
            this.QI_Vorschau.Text = "Vorschau";
            this.QI_Vorschau.Click += new System.EventHandler(this.QI_Vorschau_Click);
            // 
            // tabSonstiges
            // 
            this.tabSonstiges.Controls.Add(this.txbLinkedKeyKennung);
            this.tabSonstiges.Controls.Add(this.capLinkedKeyKennung);
            this.tabSonstiges.Controls.Add(this.cbxLinkedDatabase);
            this.tabSonstiges.Controls.Add(this.capLinkedDatabase);
            this.tabSonstiges.Controls.Add(this.capBestFileStandardSuffix);
            this.tabSonstiges.Controls.Add(this.txbBestFileStandardSuffix);
            this.tabSonstiges.Controls.Add(this.capBestFileStandardFolder);
            this.tabSonstiges.Controls.Add(this.txbBestFileStandardFolder);
            this.tabSonstiges.Controls.Add(this.btnSpellChecking);
            this.tabSonstiges.Controls.Add(this.btnLogUndo);
            this.tabSonstiges.Controls.Add(this.tbxAllowedChars);
            this.tabSonstiges.Controls.Add(this.Caption13);
            this.tabSonstiges.Controls.Add(this.tbxTags);
            this.tabSonstiges.Controls.Add(this.Caption8);
            this.tabSonstiges.Location = new System.Drawing.Point(4, 25);
            this.tabSonstiges.Name = "tabSonstiges";
            this.tabSonstiges.Padding = new System.Windows.Forms.Padding(3);
            this.tabSonstiges.Size = new System.Drawing.Size(912, 487);
            this.tabSonstiges.TabIndex = 4;
            this.tabSonstiges.Text = "Sonstiges";
            this.tabSonstiges.UseVisualStyleBackColor = true;
            // 
            // txbLinkedKeyKennung
            // 
            this.txbLinkedKeyKennung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txbLinkedKeyKennung.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbLinkedKeyKennung.Location = new System.Drawing.Point(785, 288);
            this.txbLinkedKeyKennung.Name = "txbLinkedKeyKennung";
            this.txbLinkedKeyKennung.Size = new System.Drawing.Size(120, 24);
            this.txbLinkedKeyKennung.TabIndex = 40;
            // 
            // capLinkedKeyKennung
            // 
            this.capLinkedKeyKennung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capLinkedKeyKennung.CausesValidation = false;
            this.capLinkedKeyKennung.Location = new System.Drawing.Point(513, 288);
            this.capLinkedKeyKennung.Name = "capLinkedKeyKennung";
            this.capLinkedKeyKennung.Size = new System.Drawing.Size(224, 16);
            this.capLinkedKeyKennung.Text = "Verknüpfte Spalten beginnen mit:";
            // 
            // cbxLinkedDatabase
            // 
            this.cbxLinkedDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxLinkedDatabase.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxLinkedDatabase.Location = new System.Drawing.Point(673, 256);
            this.cbxLinkedDatabase.Name = "cbxLinkedDatabase";
            this.cbxLinkedDatabase.Size = new System.Drawing.Size(232, 24);
            this.cbxLinkedDatabase.TabIndex = 38;
            // 
            // capLinkedDatabase
            // 
            this.capLinkedDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capLinkedDatabase.CausesValidation = false;
            this.capLinkedDatabase.Location = new System.Drawing.Point(513, 256);
            this.capLinkedDatabase.Name = "capLinkedDatabase";
            this.capLinkedDatabase.Size = new System.Drawing.Size(152, 16);
            this.capLinkedDatabase.Text = "Vernküpfte Datenbank:";
            // 
            // capBestFileStandardSuffix
            // 
            this.capBestFileStandardSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capBestFileStandardSuffix.CausesValidation = false;
            this.capBestFileStandardSuffix.Location = new System.Drawing.Point(513, 208);
            this.capBestFileStandardSuffix.Name = "capBestFileStandardSuffix";
            this.capBestFileStandardSuffix.Size = new System.Drawing.Size(264, 16);
            this.capBestFileStandardSuffix.Text = "Standard Dateiendung bei neuen Dateien:";
            // 
            // txbBestFileStandardSuffix
            // 
            this.txbBestFileStandardSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txbBestFileStandardSuffix.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbBestFileStandardSuffix.Location = new System.Drawing.Point(785, 208);
            this.txbBestFileStandardSuffix.Name = "txbBestFileStandardSuffix";
            this.txbBestFileStandardSuffix.Size = new System.Drawing.Size(120, 24);
            this.txbBestFileStandardSuffix.TabIndex = 36;
            // 
            // capBestFileStandardFolder
            // 
            this.capBestFileStandardFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capBestFileStandardFolder.CausesValidation = false;
            this.capBestFileStandardFolder.Location = new System.Drawing.Point(513, 160);
            this.capBestFileStandardFolder.Name = "capBestFileStandardFolder";
            this.capBestFileStandardFolder.Size = new System.Drawing.Size(320, 16);
            this.capBestFileStandardFolder.Text = "Standard Speicher Ordner bei Dateien:";
            // 
            // txbBestFileStandardFolder
            // 
            this.txbBestFileStandardFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txbBestFileStandardFolder.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbBestFileStandardFolder.Location = new System.Drawing.Point(513, 176);
            this.txbBestFileStandardFolder.Name = "txbBestFileStandardFolder";
            this.txbBestFileStandardFolder.Size = new System.Drawing.Size(392, 24);
            this.txbBestFileStandardFolder.TabIndex = 34;
            // 
            // btnSpellChecking
            // 
            this.btnSpellChecking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSpellChecking.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnSpellChecking.Location = new System.Drawing.Point(513, 128);
            this.btnSpellChecking.Name = "btnSpellChecking";
            this.btnSpellChecking.Size = new System.Drawing.Size(352, 16);
            this.btnSpellChecking.TabIndex = 33;
            this.btnSpellChecking.Text = "Rechtschreibprüfung aktivieren";
            // 
            // btnLogUndo
            // 
            this.btnLogUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogUndo.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnLogUndo.Location = new System.Drawing.Point(513, 104);
            this.btnLogUndo.Name = "btnLogUndo";
            this.btnLogUndo.Size = new System.Drawing.Size(352, 16);
            this.btnLogUndo.TabIndex = 32;
            this.btnLogUndo.Text = "Undo wird geloggt";
            // 
            // tbxAllowedChars
            // 
            this.tbxAllowedChars.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAllowedChars.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxAllowedChars.Location = new System.Drawing.Point(513, 48);
            this.tbxAllowedChars.Name = "tbxAllowedChars";
            this.tbxAllowedChars.Size = new System.Drawing.Size(391, 48);
            this.tbxAllowedChars.TabIndex = 30;
            this.tbxAllowedChars.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption13
            // 
            this.Caption13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Caption13.CausesValidation = false;
            this.Caption13.Location = new System.Drawing.Point(513, 24);
            this.Caption13.Name = "Caption13";
            this.Caption13.Size = new System.Drawing.Size(392, 24);
            this.Caption13.Text = "Abweichend von der Formatvorgabe, folgende Zeichen sind erlaubt:";
            this.Caption13.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tbxTags
            // 
            this.tbxTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTags.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxTags.Location = new System.Drawing.Point(4, 31);
            this.tbxTags.MultiLine = true;
            this.tbxTags.Name = "tbxTags";
            this.tbxTags.Size = new System.Drawing.Size(500, 448);
            this.tbxTags.TabIndex = 30;
            this.tbxTags.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption8
            // 
            this.Caption8.CausesValidation = false;
            this.Caption8.Location = new System.Drawing.Point(4, 15);
            this.Caption8.Name = "Caption8";
            this.Caption8.Size = new System.Drawing.Size(144, 16);
            this.Caption8.Text = "Sonstige Daten (Tags):";
            this.Caption8.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BlueFrame1.BackColor = System.Drawing.SystemColors.Control;
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.capInfo);
            this.BlueFrame1.Controls.Add(this.Caption3);
            this.BlueFrame1.Controls.Add(this.tbxName);
            this.BlueFrame1.Controls.Add(this.Caption2);
            this.BlueFrame1.Controls.Add(this.tbxCaption);
            this.BlueFrame1.Location = new System.Drawing.Point(8, 8);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(916, 104);
            this.BlueFrame1.Text = "Allgemein";
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 16);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(280, 19);
            this.capInfo.Text = "NR";
            this.capInfo.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new System.Drawing.Point(8, 40);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new System.Drawing.Size(136, 16);
            this.Caption3.Text = "Interner Spaltename:";
            this.Caption3.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tbxName
            // 
            this.tbxName.AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.[]()";
            this.tbxName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxName.Location = new System.Drawing.Point(8, 56);
            this.tbxName.Name = "tbxName";
            this.tbxName.Size = new System.Drawing.Size(296, 24);
            this.tbxName.TabIndex = 0;
            // 
            // Caption2
            // 
            this.Caption2.CausesValidation = false;
            this.Caption2.Location = new System.Drawing.Point(312, 16);
            this.Caption2.Name = "Caption2";
            this.Caption2.Size = new System.Drawing.Size(144, 16);
            this.Caption2.Text = "Angezeigte Beschriftung:";
            this.Caption2.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tbxCaption
            // 
            this.tbxCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxCaption.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxCaption.Location = new System.Drawing.Point(312, 32);
            this.tbxCaption.MultiLine = true;
            this.tbxCaption.Name = "tbxCaption";
            this.tbxCaption.Size = new System.Drawing.Size(600, 64);
            this.tbxCaption.TabIndex = 2;
            // 
            // btnVor
            // 
            this.btnVor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVor.Location = new System.Drawing.Point(696, 643);
            this.btnVor.Name = "btnVor";
            this.btnVor.Size = new System.Drawing.Size(72, 24);
            this.btnVor.TabIndex = 5;
            this.btnVor.Text = ">>>";
            this.btnVor.Click += new System.EventHandler(this.Plus_Click);
            // 
            // btnZurueck
            // 
            this.btnZurueck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZurueck.Location = new System.Drawing.Point(592, 643);
            this.btnZurueck.Name = "btnZurueck";
            this.btnZurueck.Size = new System.Drawing.Size(72, 24);
            this.btnZurueck.TabIndex = 4;
            this.btnZurueck.Text = "<<<";
            this.btnZurueck.Click += new System.EventHandler(this.Minus_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(848, 643);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(72, 24);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.OkBut_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabDesign);
            this.tabControl.Controls.Add(this.tabRechte);
            this.tabControl.Controls.Add(this.tabAutoBearbeitung);
            this.tabControl.Controls.Add(this.tabFilter);
            this.tabControl.Controls.Add(this.tabQuickInfo);
            this.tabControl.Controls.Add(this.tabSonstiges);
            this.tabControl.Controls.Add(this.tabRegeln);
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new System.Drawing.Point(0, 120);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = new System.Drawing.Size(920, 516);
            this.tabControl.TabIndex = 15;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabRegeln
            // 
            this.tabRegeln.Controls.Add(this.gpxVerlinkteZellen);
            this.tabRegeln.Controls.Add(this.btnFehlerWennUnsichtbare);
            this.tabRegeln.Controls.Add(this.btnFormatFehler);
            this.tabRegeln.Controls.Add(this.btnFehlerWennLeer);
            this.tabRegeln.Location = new System.Drawing.Point(4, 25);
            this.tabRegeln.Name = "tabRegeln";
            this.tabRegeln.Size = new System.Drawing.Size(912, 487);
            this.tabRegeln.TabIndex = 8;
            this.tabRegeln.Text = "Regeln";
            // 
            // gpxVerlinkteZellen
            // 
            this.gpxVerlinkteZellen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpxVerlinkteZellen.CausesValidation = false;
            this.gpxVerlinkteZellen.Controls.Add(this.cbxFehlendesZiel);
            this.gpxVerlinkteZellen.Controls.Add(this.caption5);
            this.gpxVerlinkteZellen.Controls.Add(this.txbZeichenkette);
            this.gpxVerlinkteZellen.Controls.Add(this.capZeichenkette);
            this.gpxVerlinkteZellen.Controls.Add(this.line1);
            this.gpxVerlinkteZellen.Controls.Add(this.cbxTargetColumn);
            this.gpxVerlinkteZellen.Controls.Add(this.cbxColumnKeyInColumn);
            this.gpxVerlinkteZellen.Controls.Add(this.cbxRowKeyInColumn);
            this.gpxVerlinkteZellen.Controls.Add(this.btnTargetColumn);
            this.gpxVerlinkteZellen.Controls.Add(this.btnColumnKeyInColumn);
            this.gpxVerlinkteZellen.Controls.Add(this.capRowKeyInColumn);
            this.gpxVerlinkteZellen.Location = new System.Drawing.Point(8, 96);
            this.gpxVerlinkteZellen.Name = "gpxVerlinkteZellen";
            this.gpxVerlinkteZellen.Size = new System.Drawing.Size(900, 256);
            this.gpxVerlinkteZellen.Text = "Verlinkte Zellen:";
            // 
            // caption5
            // 
            this.caption5.CausesValidation = false;
            this.caption5.Location = new System.Drawing.Point(8, 224);
            this.caption5.Name = "caption5";
            this.caption5.Size = new System.Drawing.Size(320, 23);
            this.caption5.Text = "Zeile nicht gefunden in Zieldatenbank:";
            // 
            // txbZeichenkette
            // 
            this.txbZeichenkette.AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
            this.txbZeichenkette.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbZeichenkette.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeichenkette.Location = new System.Drawing.Point(336, 136);
            this.txbZeichenkette.Name = "txbZeichenkette";
            this.txbZeichenkette.QuickInfo = resources.GetString("txbZeichenkette.QuickInfo");
            this.txbZeichenkette.Size = new System.Drawing.Size(548, 24);
            this.txbZeichenkette.TabIndex = 7;
            // 
            // capZeichenkette
            // 
            this.capZeichenkette.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.capZeichenkette.Location = new System.Drawing.Point(40, 136);
            this.capZeichenkette.Name = "capZeichenkette";
            this.capZeichenkette.Size = new System.Drawing.Size(288, 24);
            this.capZeichenkette.TabIndex = 6;
            this.capZeichenkette.Text = "...zusätzlich folgende Zeichenkette voranstellen:";
            // 
            // line1
            // 
            this.line1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.line1.CausesValidation = false;
            this.line1.Location = new System.Drawing.Point(8, 56);
            this.line1.Name = "line1";
            this.line1.Size = new System.Drawing.Size(884, 2);
            this.line1.Text = "line1";
            // 
            // cbxTargetColumn
            // 
            this.cbxTargetColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxTargetColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTargetColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTargetColumn.Location = new System.Drawing.Point(336, 72);
            this.cbxTargetColumn.Name = "cbxTargetColumn";
            this.cbxTargetColumn.Size = new System.Drawing.Size(548, 24);
            this.cbxTargetColumn.TabIndex = 5;
            // 
            // cbxColumnKeyInColumn
            // 
            this.cbxColumnKeyInColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxColumnKeyInColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumnKeyInColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColumnKeyInColumn.Location = new System.Drawing.Point(336, 112);
            this.cbxColumnKeyInColumn.Name = "cbxColumnKeyInColumn";
            this.cbxColumnKeyInColumn.Size = new System.Drawing.Size(548, 24);
            this.cbxColumnKeyInColumn.TabIndex = 4;
            // 
            // cbxRowKeyInColumn
            // 
            this.cbxRowKeyInColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRowKeyInColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRowKeyInColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRowKeyInColumn.Location = new System.Drawing.Point(336, 24);
            this.cbxRowKeyInColumn.Name = "cbxRowKeyInColumn";
            this.cbxRowKeyInColumn.Size = new System.Drawing.Size(548, 24);
            this.cbxRowKeyInColumn.TabIndex = 3;
            // 
            // btnTargetColumn
            // 
            this.btnTargetColumn.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.btnTargetColumn.Checked = true;
            this.btnTargetColumn.Location = new System.Drawing.Point(8, 72);
            this.btnTargetColumn.Name = "btnTargetColumn";
            this.btnTargetColumn.Size = new System.Drawing.Size(320, 32);
            this.btnTargetColumn.TabIndex = 2;
            this.btnTargetColumn.Text = "Die Quell-Spalte (aus der verlinkten Datenbank) ist immer:";
            // 
            // btnColumnKeyInColumn
            // 
            this.btnColumnKeyInColumn.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.btnColumnKeyInColumn.Location = new System.Drawing.Point(8, 112);
            this.btnColumnKeyInColumn.Name = "btnColumnKeyInColumn";
            this.btnColumnKeyInColumn.Size = new System.Drawing.Size(320, 16);
            this.btnColumnKeyInColumn.TabIndex = 1;
            this.btnColumnKeyInColumn.Text = "Die zu suchende Spalte ist in dieser Spalte zu finden:";
            // 
            // capRowKeyInColumn
            // 
            this.capRowKeyInColumn.CausesValidation = false;
            this.capRowKeyInColumn.Location = new System.Drawing.Point(8, 24);
            this.capRowKeyInColumn.Name = "capRowKeyInColumn";
            this.capRowKeyInColumn.Size = new System.Drawing.Size(320, 16);
            this.capRowKeyInColumn.Text = "Die zu suchende Zeile ist in dieser Spalte zu finden:";
            // 
            // btnFehlerWennUnsichtbare
            // 
            this.btnFehlerWennUnsichtbare.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFehlerWennUnsichtbare.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnFehlerWennUnsichtbare.Location = new System.Drawing.Point(8, 64);
            this.btnFehlerWennUnsichtbare.Name = "btnFehlerWennUnsichtbare";
            this.btnFehlerWennUnsichtbare.Size = new System.Drawing.Size(900, 16);
            this.btnFehlerWennUnsichtbare.TabIndex = 2;
            this.btnFehlerWennUnsichtbare.Text = "Wenn der Zelleninhalt dieser Spalte unsichtbare Leerzeichen oder Enter-Codes enth" +
    "ält, gib einen Fehler aus";
            // 
            // btnFormatFehler
            // 
            this.btnFormatFehler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFormatFehler.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnFormatFehler.Location = new System.Drawing.Point(8, 40);
            this.btnFormatFehler.Name = "btnFormatFehler";
            this.btnFormatFehler.Size = new System.Drawing.Size(900, 16);
            this.btnFormatFehler.TabIndex = 1;
            this.btnFormatFehler.Text = "Wenn der Zelleninhalt dieser Spalte vom eingestellen Format abweicht, gib einen F" +
    "ehler aus";
            // 
            // btnFehlerWennLeer
            // 
            this.btnFehlerWennLeer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFehlerWennLeer.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.btnFehlerWennLeer.Location = new System.Drawing.Point(8, 16);
            this.btnFehlerWennLeer.Name = "btnFehlerWennLeer";
            this.btnFehlerWennLeer.Size = new System.Drawing.Size(900, 16);
            this.btnFehlerWennLeer.TabIndex = 0;
            this.btnFehlerWennLeer.Text = "Wenn die Zelle dieser Spalte leer ist, gib einen Fehler aus";
            // 
            // cbxFehlendesZiel
            // 
            this.cbxFehlendesZiel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxFehlendesZiel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxFehlendesZiel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFehlendesZiel.Location = new System.Drawing.Point(336, 224);
            this.cbxFehlendesZiel.Name = "cbxFehlendesZiel";
            this.cbxFehlendesZiel.Size = new System.Drawing.Size(548, 24);
            this.cbxFehlendesZiel.TabIndex = 9;
            // 
            // ColumnEditor
            // 
            this.ClientSize = new System.Drawing.Size(926, 671);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.btnVor);
            this.Controls.Add(this.btnZurueck);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ColumnEditor";
            this.Text = "Spalten-Eigenschaften";
            this.TopMost = true;
            this.tabDesign.ResumeLayout(false);
            this.grbBildCode.ResumeLayout(false);
            this.tabRechte.ResumeLayout(false);
            this.tabAutoBearbeitung.ResumeLayout(false);
            this.tabFilter.ResumeLayout(false);
            this.tabQuickInfo.ResumeLayout(false);
            this.tabSonstiges.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabRegeln.ResumeLayout(false);
            this.gpxVerlinkteZellen.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button btnOk;
			private TextBox tbxName;
			private Caption Caption3;
			private System.Windows.Forms.ColorDialog ColorDia;
			private Button QI_Vorschau;
			private TextBox tbxQuickinfo;
			private Button ZeilenFilter;
			private Button AutoFilterErw;
			private Button AutoFilterTXT;
			private Button AutoFilterMöglich;
			private Caption capInfo;
			private Button btnZurueck;
			private Button btnVor;
			private Button btnMultiline;
			private Caption Caption4;
			private Caption Caption1;
			private Button T_Colorx;
			private Button H_Colorx;
			private Caption Caption2;
			private TextBox tbxCaption;
			private GroupBox BlueFrame1;
			private Button btnEditableDropdown;
			private Button btnEditableStandard;
			private TextBox tbxAuswaehlbareWerte;
			private Button btnCanBeEmpty;
			private Caption Caption9;
			private Caption Caption8;
			private ComboBox cbxRandRechts;
			private ComboBox cbxRandLinks;
			private Caption Caption13;
			private TextBox tbxAllowedChars;
			private Caption Caption15;
			private Caption Caption16;
			private Caption Caption18;
			private TextBox tbxAdminInfo;
			private Caption Caption17;
			private Button btnOtherValuesToo;
			private ListBox lbxCellEditor;
			private ComboBox cbxFormat;
			private TextBox tbxTags;
			private TabControl tabControl;
			private TabPage tabDesign;
			private TabPage tabRechte;
			private TabPage tabAutoBearbeitung;
			private TabPage tabFilter;
			private TabPage tabQuickInfo;
			private TabPage tabSonstiges;
			private Button btnKompakteAnzeige;
			private Button btnEinzeiligDarstellen;
			private Button btnIgnoreLock;
			private Button btnLogUndo;
			private Button btnSpellChecking;
			internal EasyPic picCaptionImage;
			private Caption Caption6;
			internal Caption capEinheit;
			internal ComboBox cbxEinheit;
			internal TextBox tbxJoker;
			internal Caption Caption10;
			internal TextBox tbxInitValue;
			internal Caption Caption12;
			private Button btnAutoEditKleineFehler;
			private Button btnAutoEditToUpper;
			internal TextBox tbxRunden;
			internal Caption Caption11;
			private Button btnAutoEditAutoSort;
            private ComboBox cbxBildCodeImageNotfound;
            private Caption capBildCodeImageNotfound;
            private Caption capBildCodeConstHeight;
            private TextBox txbBildCodeConstHeight;
            private TextBox txbLinkedKeyKennung;
            private Caption capLinkedKeyKennung;
            internal ComboBox cbxLinkedDatabase;
            private Caption capLinkedDatabase;
            private Caption capBestFileStandardSuffix;
            private TextBox txbBestFileStandardSuffix;
            private Caption capBestFileStandardFolder;
            private TextBox txbBestFileStandardFolder;
            private TabPage tabRegeln;
            private GroupBox gpxVerlinkteZellen;
            private ComboBox cbxTargetColumn;
            private ComboBox cbxColumnKeyInColumn;
            private ComboBox cbxRowKeyInColumn;
            private Button btnTargetColumn;
            private Button btnColumnKeyInColumn;
            private Caption capRowKeyInColumn;
            private Button btnFehlerWennUnsichtbare;
            private Button btnFormatFehler;
            private Button btnFehlerWennLeer;
            private TextBox capZeichenkette;
            private Line line1;
            private TextBox txbZeichenkette;
            private TextBox txbReplacer;
            private Caption capReplacer;
        private Caption capUeberschrift3;
        private Caption capUeberschrift2;
        private Caption capUeberschrift1;
        private TextBox txbUeberschift3;
        private TextBox txbUeberschift2;
        private TextBox txbUeberschift1;
        private GroupBox grbBildCode;
        private TextBox txbPrefix;
        private Caption caption7;
        private Caption caption5;
        private ComboBox cbxFehlendesZiel;
    }
	}
