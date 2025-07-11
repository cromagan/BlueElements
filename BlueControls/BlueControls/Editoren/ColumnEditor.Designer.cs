using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using ColorDialog = System.Windows.Forms.ColorDialog;
using ComboBox = BlueControls.Controls.ComboBox;
using Form = BlueControls.Forms.Form;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    internal sealed partial class ColumnEditor : Form {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) =>
            //if (disposing && components != null)
            //{
            //	components?.Dispose();
            //}
            base.Dispose(disposing);
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColumnEditor));
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.tabAnzeige = new System.Windows.Forms.TabPage();
            this.cbxRenderer = new BlueControls.Controls.ComboBox();
            this.txbSpaltenbild = new BlueControls.Controls.TextBox();
            this.txbUeberschift1 = new BlueControls.Controls.TextBox();
            this.txbUeberschift3 = new BlueControls.Controls.TextBox();
            this.capSpaltenbild = new BlueControls.Controls.Caption();
            this.txbFixedColumnWidth = new BlueControls.Controls.TextBox();
            this.RendererEditor = new BlueControls.Controls.GroupBox();
            this.capFixedColumnWidth = new BlueControls.Controls.Caption();
            this.txbUeberschift2 = new BlueControls.Controls.TextBox();
            this.btnTextColor = new BlueControls.Controls.Button();
            this.btnBackColor = new BlueControls.Controls.Button();
            this.capUeberschrift3 = new BlueControls.Controls.Caption();
            this.capRenderer = new BlueControls.Controls.Caption();
            this.btnSpellChecking = new BlueControls.Controls.Button();
            this.capUeberschrift1 = new BlueControls.Controls.Caption();
            this.cbxTranslate = new BlueControls.Controls.ComboBox();
            this.capTranslate = new BlueControls.Controls.Caption();
            this.capUeberschrift2 = new BlueControls.Controls.Caption();
            this.capLinkerRand = new BlueControls.Controls.Caption();
            this.cbxAlign = new BlueControls.Controls.ComboBox();
            this.cbxRandLinks = new BlueControls.Controls.ComboBox();
            this.btnStandard = new BlueControls.Controls.Button();
            this.capRechterRand = new BlueControls.Controls.Caption();
            this.cbxRandRechts = new BlueControls.Controls.ComboBox();
            this.capAlign = new BlueControls.Controls.Caption();
            this.cbxScriptType = new BlueControls.Controls.ComboBox();
            this.capScriptType = new BlueControls.Controls.Caption();
            this.chkFormatierungErlaubt = new BlueControls.Controls.Button();
            this.cbxAdditionalCheck = new BlueControls.Controls.ComboBox();
            this.capcbxAdditionalCheck = new BlueControls.Controls.Caption();
            this.cbxChunk = new BlueControls.Controls.ComboBox();
            this.capChunk = new BlueControls.Controls.Caption();
            this.chkMultiline = new BlueControls.Controls.Button();
            this.tabBearbeitung = new System.Windows.Forms.TabPage();
            this.grpAuswahlmenuOptionen = new BlueControls.Controls.GroupBox();
            this.btnOtherValuesToo = new BlueControls.Controls.Button();
            this.txbAuswaehlbareWerte = new BlueControls.Controls.TextBox();
            this.capImmerWerte = new BlueControls.Controls.Caption();
            this.btnCanBeEmpty = new BlueControls.Controls.Button();
            this.btnIgnoreLock = new BlueControls.Controls.Button();
            this.lbxCellEditor = new BlueControls.Controls.ListBox();
            this.btnEditableStandard = new BlueControls.Controls.Button();
            this.capUserGroupEdit = new BlueControls.Controls.Caption();
            this.btnEditableDropdown = new BlueControls.Controls.Button();
            this.chkSaveContent = new BlueControls.Controls.Button();
            this.tabAutoKorrektur = new System.Windows.Forms.TabPage();
            this.btnCalculateMaxCellLenght = new BlueControls.Controls.Button();
            this.txbAutoReplace = new BlueControls.Controls.TextBox();
            this.capAutoReplace = new BlueControls.Controls.Caption();
            this.txbMaxCellLenght = new BlueControls.Controls.TextBox();
            this.txbAutoRemove = new BlueControls.Controls.TextBox();
            this.capAutoRemove = new BlueControls.Controls.Caption();
            this.capMaxCellLenght = new BlueControls.Controls.Caption();
            this.btnAutoEditKleineFehler = new BlueControls.Controls.Button();
            this.btnAutoEditToUpper = new BlueControls.Controls.Button();
            this.txbRunden = new BlueControls.Controls.TextBox();
            this.capNachkommastellen = new BlueControls.Controls.Caption();
            this.btnAutoEditAutoSort = new BlueControls.Controls.Button();
            this.tabFilter = new System.Windows.Forms.TabPage();
            this.chkFilterOnlyOr = new BlueControls.Controls.Button();
            this.chkFilterOnlyAND = new BlueControls.Controls.Button();
            this.capJokerValue = new BlueControls.Controls.Caption();
            this.txbJoker = new BlueControls.Controls.TextBox();
            this.btnZeilenFilterIgnorieren = new BlueControls.Controls.Button();
            this.btnAutoFilterMoeglich = new BlueControls.Controls.Button();
            this.btnAutoFilterTXTErlaubt = new BlueControls.Controls.Button();
            this.btnAutoFilterErweitertErlaubt = new BlueControls.Controls.Button();
            this.tabQuickInfo = new System.Windows.Forms.TabPage();
            this.txbAdminInfo = new BlueControls.Controls.TextBox();
            this.txbQuickinfo = new BlueControls.Controls.TextBox();
            this.Caption18 = new BlueControls.Controls.Caption();
            this.Caption17 = new BlueControls.Controls.Caption();
            this.btnQI_Vorschau = new BlueControls.Controls.Button();
            this.tabSonstiges = new System.Windows.Forms.TabPage();
            this.txbTags = new BlueControls.Controls.TextBox();
            this.Caption8 = new BlueControls.Controls.Caption();
            this.cbxSort = new BlueControls.Controls.ComboBox();
            this.txbRegex = new BlueControls.Controls.TextBox();
            this.capSortiermaske = new BlueControls.Controls.Caption();
            this.capRegex = new BlueControls.Controls.Caption();
            this.txbAllowedChars = new BlueControls.Controls.TextBox();
            this.Caption13 = new BlueControls.Controls.Caption();
            this.cbxLinkedDatabase = new BlueControls.Controls.ComboBox();
            this.capLinkedDatabase = new BlueControls.Controls.Caption();
            this.BlueFrame1 = new BlueControls.Controls.GroupBox();
            this.btnSystemInfo = new BlueControls.Controls.Button();
            this.btnVerwendung = new BlueControls.Controls.Button();
            this.capInfo = new BlueControls.Controls.Caption();
            this.Caption3 = new BlueControls.Controls.Caption();
            this.txbName = new BlueControls.Controls.TextBox();
            this.Caption2 = new BlueControls.Controls.Caption();
            this.txbCaption = new BlueControls.Controls.TextBox();
            this.btnOk = new BlueControls.Controls.Button();
            this.tabControl = new BlueControls.Controls.TabControl();
            this.tabDatenFormat = new System.Windows.Forms.TabPage();
            this.chkIsKeyColumn = new BlueControls.Controls.Button();
            this.chkIsFirst = new BlueControls.Controls.Button();
            this.btnMaxTextLenght = new BlueControls.Controls.Button();
            this.txbMaxTextLenght = new BlueControls.Controls.TextBox();
            this.capMaxTextLenght = new BlueControls.Controls.Caption();
            this.grpSchnellformat = new BlueControls.Controls.GroupBox();
            this.btnSchnellText = new BlueControls.Controls.Button();
            this.btnSchnellBit = new BlueControls.Controls.Button();
            this.btnSchnellDatum = new BlueControls.Controls.Button();
            this.btnSchnellBildCode = new BlueControls.Controls.Button();
            this.btnSchnellDatumUhrzeit = new BlueControls.Controls.Button();
            this.btnSchnellIInternetAdresse = new BlueControls.Controls.Button();
            this.btnSchnellEmail = new BlueControls.Controls.Button();
            this.btnSchnellAuswahloptionen = new BlueControls.Controls.Button();
            this.btnSchnellTelefonNummer = new BlueControls.Controls.Button();
            this.btnSchnellGleitkommazahl = new BlueControls.Controls.Button();
            this.btnSchnellGanzzahl = new BlueControls.Controls.Button();
            this.tabSpaltenVerlinkung = new System.Windows.Forms.TabPage();
            this.tblFilterliste = new BlueControls.Controls.Table();
            this.cbxTargetColumn = new BlueControls.Controls.ComboBox();
            this.capTargetColumn = new BlueControls.Controls.Caption();
            this.caption5 = new BlueControls.Controls.Caption();
            this.butAktuellVor = new BlueControls.Controls.Button();
            this.butAktuellZurueck = new BlueControls.Controls.Button();
            this.capTabellenname = new BlueControls.Controls.Caption();
            this.btnSpaltenkopf = new BlueControls.Controls.Button();
            this.chkRelation = new BlueControls.Controls.Button();
            this.cbxRelationType = new BlueControls.Controls.ComboBox();
            this.caption1 = new BlueControls.Controls.Caption();
            this.tabAnzeige.SuspendLayout();
            this.tabBearbeitung.SuspendLayout();
            this.grpAuswahlmenuOptionen.SuspendLayout();
            this.tabAutoKorrektur.SuspendLayout();
            this.tabFilter.SuspendLayout();
            this.tabQuickInfo.SuspendLayout();
            this.tabSonstiges.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabDatenFormat.SuspendLayout();
            this.grpSchnellformat.SuspendLayout();
            this.tabSpaltenVerlinkung.SuspendLayout();
            this.SuspendLayout();
            // 
            // ColorDia
            // 
            this.ColorDia.AnyColor = true;
            this.ColorDia.FullOpen = true;
            // 
            // tabAnzeige
            // 
            this.tabAnzeige.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAnzeige.Controls.Add(this.cbxRenderer);
            this.tabAnzeige.Controls.Add(this.txbSpaltenbild);
            this.tabAnzeige.Controls.Add(this.txbUeberschift1);
            this.tabAnzeige.Controls.Add(this.txbUeberschift3);
            this.tabAnzeige.Controls.Add(this.capSpaltenbild);
            this.tabAnzeige.Controls.Add(this.txbFixedColumnWidth);
            this.tabAnzeige.Controls.Add(this.RendererEditor);
            this.tabAnzeige.Controls.Add(this.capFixedColumnWidth);
            this.tabAnzeige.Controls.Add(this.txbUeberschift2);
            this.tabAnzeige.Controls.Add(this.btnTextColor);
            this.tabAnzeige.Controls.Add(this.btnBackColor);
            this.tabAnzeige.Controls.Add(this.capUeberschrift3);
            this.tabAnzeige.Controls.Add(this.capRenderer);
            this.tabAnzeige.Controls.Add(this.btnSpellChecking);
            this.tabAnzeige.Controls.Add(this.capUeberschrift1);
            this.tabAnzeige.Controls.Add(this.cbxTranslate);
            this.tabAnzeige.Controls.Add(this.capTranslate);
            this.tabAnzeige.Controls.Add(this.capUeberschrift2);
            this.tabAnzeige.Controls.Add(this.capLinkerRand);
            this.tabAnzeige.Controls.Add(this.cbxAlign);
            this.tabAnzeige.Controls.Add(this.cbxRandLinks);
            this.tabAnzeige.Controls.Add(this.btnStandard);
            this.tabAnzeige.Controls.Add(this.capRechterRand);
            this.tabAnzeige.Controls.Add(this.cbxRandRechts);
            this.tabAnzeige.Controls.Add(this.capAlign);
            this.tabAnzeige.Location = new System.Drawing.Point(4, 25);
            this.tabAnzeige.Name = "tabAnzeige";
            this.tabAnzeige.Padding = new System.Windows.Forms.Padding(3);
            this.tabAnzeige.Size = new System.Drawing.Size(1098, 594);
            this.tabAnzeige.TabIndex = 0;
            this.tabAnzeige.Text = "Anzeige";
            // 
            // cbxRenderer
            // 
            this.cbxRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRenderer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRenderer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRenderer.Location = new System.Drawing.Point(416, 24);
            this.cbxRenderer.Name = "cbxRenderer";
            this.cbxRenderer.RegexCheck = null;
            this.cbxRenderer.Size = new System.Drawing.Size(673, 24);
            this.cbxRenderer.TabIndex = 44;
            this.cbxRenderer.TextChanged += new System.EventHandler(this.cbxRenderer_TextChanged);
            // 
            // txbSpaltenbild
            // 
            this.txbSpaltenbild.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSpaltenbild.Location = new System.Drawing.Point(8, 360);
            this.txbSpaltenbild.Name = "txbSpaltenbild";
            this.txbSpaltenbild.RegexCheck = null;
            this.txbSpaltenbild.Size = new System.Drawing.Size(400, 24);
            this.txbSpaltenbild.TabIndex = 40;
            // 
            // txbUeberschift1
            // 
            this.txbUeberschift1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift1.Location = new System.Drawing.Point(104, 256);
            this.txbUeberschift1.Name = "txbUeberschift1";
            this.txbUeberschift1.RegexCheck = null;
            this.txbUeberschift1.Size = new System.Drawing.Size(304, 24);
            this.txbUeberschift1.TabIndex = 36;
            // 
            // txbUeberschift3
            // 
            this.txbUeberschift3.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift3.Location = new System.Drawing.Point(104, 303);
            this.txbUeberschift3.Name = "txbUeberschift3";
            this.txbUeberschift3.RegexCheck = null;
            this.txbUeberschift3.Size = new System.Drawing.Size(304, 24);
            this.txbUeberschift3.TabIndex = 38;
            // 
            // capSpaltenbild
            // 
            this.capSpaltenbild.CausesValidation = false;
            this.capSpaltenbild.Location = new System.Drawing.Point(8, 344);
            this.capSpaltenbild.Name = "capSpaltenbild";
            this.capSpaltenbild.Size = new System.Drawing.Size(72, 24);
            this.capSpaltenbild.Text = "Spaltenbild:";
            this.capSpaltenbild.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbFixedColumnWidth
            // 
            this.txbFixedColumnWidth.AllowedChars = "0123456789|";
            this.txbFixedColumnWidth.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFixedColumnWidth.Location = new System.Drawing.Point(136, 208);
            this.txbFixedColumnWidth.Name = "txbFixedColumnWidth";
            this.txbFixedColumnWidth.QuickInfo = "Wenn ein Wert >0 eingegeben wird, \r\nwird die Spalte immer in dieser Breite angeze" +
    "igt.";
            this.txbFixedColumnWidth.Size = new System.Drawing.Size(88, 24);
            this.txbFixedColumnWidth.Suffix = "Pixel";
            this.txbFixedColumnWidth.TabIndex = 42;
            // 
            // RendererEditor
            // 
            this.RendererEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RendererEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.RendererEditor.Location = new System.Drawing.Point(416, 48);
            this.RendererEditor.Name = "RendererEditor";
            this.RendererEditor.Size = new System.Drawing.Size(673, 543);
            this.RendererEditor.TabIndex = 45;
            this.RendererEditor.TabStop = false;
            // 
            // capFixedColumnWidth
            // 
            this.capFixedColumnWidth.CausesValidation = false;
            this.capFixedColumnWidth.Location = new System.Drawing.Point(8, 208);
            this.capFixedColumnWidth.Name = "capFixedColumnWidth";
            this.capFixedColumnWidth.Size = new System.Drawing.Size(120, 16);
            this.capFixedColumnWidth.Text = "Feste Spaltenbreite:";
            // 
            // txbUeberschift2
            // 
            this.txbUeberschift2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift2.Location = new System.Drawing.Point(104, 279);
            this.txbUeberschift2.Name = "txbUeberschift2";
            this.txbUeberschift2.RegexCheck = null;
            this.txbUeberschift2.Size = new System.Drawing.Size(304, 24);
            this.txbUeberschift2.TabIndex = 37;
            // 
            // btnTextColor
            // 
            this.btnTextColor.Location = new System.Drawing.Point(272, 8);
            this.btnTextColor.Name = "btnTextColor";
            this.btnTextColor.Size = new System.Drawing.Size(128, 32);
            this.btnTextColor.TabIndex = 4;
            this.btnTextColor.Text = "Textfarbe";
            this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
            // 
            // btnBackColor
            // 
            this.btnBackColor.Location = new System.Drawing.Point(272, 40);
            this.btnBackColor.Name = "btnBackColor";
            this.btnBackColor.Size = new System.Drawing.Size(128, 32);
            this.btnBackColor.TabIndex = 3;
            this.btnBackColor.Text = "Hintergrundfarbe";
            this.btnBackColor.Click += new System.EventHandler(this.btnBackColor_Click);
            // 
            // capUeberschrift3
            // 
            this.capUeberschrift3.CausesValidation = false;
            this.capUeberschrift3.Location = new System.Drawing.Point(8, 304);
            this.capUeberschrift3.Name = "capUeberschrift3";
            this.capUeberschrift3.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift3.Text = "Überschrift 3:";
            this.capUeberschrift3.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRenderer
            // 
            this.capRenderer.CausesValidation = false;
            this.capRenderer.Location = new System.Drawing.Point(416, 8);
            this.capRenderer.Name = "capRenderer";
            this.capRenderer.Size = new System.Drawing.Size(160, 16);
            this.capRenderer.Text = "Standard-Renderer:";
            // 
            // btnSpellChecking
            // 
            this.btnSpellChecking.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnSpellChecking.Location = new System.Drawing.Point(8, 184);
            this.btnSpellChecking.Name = "btnSpellChecking";
            this.btnSpellChecking.Size = new System.Drawing.Size(200, 16);
            this.btnSpellChecking.TabIndex = 33;
            this.btnSpellChecking.Text = "Rechtschreibprüfung aktivieren";
            // 
            // capUeberschrift1
            // 
            this.capUeberschrift1.CausesValidation = false;
            this.capUeberschrift1.Location = new System.Drawing.Point(8, 256);
            this.capUeberschrift1.Name = "capUeberschrift1";
            this.capUeberschrift1.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift1.Text = "Überschrift 1:";
            this.capUeberschrift1.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxTranslate
            // 
            this.cbxTranslate.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTranslate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTranslate.Location = new System.Drawing.Point(104, 152);
            this.cbxTranslate.Name = "cbxTranslate";
            this.cbxTranslate.RegexCheck = null;
            this.cbxTranslate.Size = new System.Drawing.Size(304, 24);
            this.cbxTranslate.TabIndex = 37;
            // 
            // capTranslate
            // 
            this.capTranslate.CausesValidation = false;
            this.capTranslate.Location = new System.Drawing.Point(8, 152);
            this.capTranslate.Name = "capTranslate";
            this.capTranslate.Size = new System.Drawing.Size(88, 24);
            this.capTranslate.Text = "Übersetzen:";
            // 
            // capUeberschrift2
            // 
            this.capUeberschrift2.CausesValidation = false;
            this.capUeberschrift2.Location = new System.Drawing.Point(8, 280);
            this.capUeberschrift2.Name = "capUeberschrift2";
            this.capUeberschrift2.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift2.Text = "Überschrift 2:";
            this.capUeberschrift2.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capLinkerRand
            // 
            this.capLinkerRand.CausesValidation = false;
            this.capLinkerRand.Location = new System.Drawing.Point(8, 64);
            this.capLinkerRand.Name = "capLinkerRand";
            this.capLinkerRand.Size = new System.Drawing.Size(80, 16);
            this.capLinkerRand.Text = "Linker Rand:";
            this.capLinkerRand.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxAlign
            // 
            this.cbxAlign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAlign.Location = new System.Drawing.Point(104, 120);
            this.cbxAlign.Name = "cbxAlign";
            this.cbxAlign.RegexCheck = null;
            this.cbxAlign.Size = new System.Drawing.Size(304, 24);
            this.cbxAlign.TabIndex = 7;
            // 
            // cbxRandLinks
            // 
            this.cbxRandLinks.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRandLinks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRandLinks.Location = new System.Drawing.Point(8, 80);
            this.cbxRandLinks.Name = "cbxRandLinks";
            this.cbxRandLinks.RegexCheck = null;
            this.cbxRandLinks.Size = new System.Drawing.Size(128, 24);
            this.cbxRandLinks.TabIndex = 24;
            // 
            // btnStandard
            // 
            this.btnStandard.Location = new System.Drawing.Point(8, 8);
            this.btnStandard.Name = "btnStandard";
            this.btnStandard.Size = new System.Drawing.Size(200, 48);
            this.btnStandard.TabIndex = 39;
            this.btnStandard.Text = "Standard herstellen";
            this.btnStandard.Click += new System.EventHandler(this.btnStandard_Click);
            // 
            // capRechterRand
            // 
            this.capRechterRand.CausesValidation = false;
            this.capRechterRand.Location = new System.Drawing.Point(144, 64);
            this.capRechterRand.Name = "capRechterRand";
            this.capRechterRand.Size = new System.Drawing.Size(88, 16);
            this.capRechterRand.Text = "Rechter Rand:";
            this.capRechterRand.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxRandRechts
            // 
            this.cbxRandRechts.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRandRechts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRandRechts.Location = new System.Drawing.Point(144, 80);
            this.cbxRandRechts.Name = "cbxRandRechts";
            this.cbxRandRechts.RegexCheck = null;
            this.cbxRandRechts.Size = new System.Drawing.Size(128, 24);
            this.cbxRandRechts.TabIndex = 25;
            // 
            // capAlign
            // 
            this.capAlign.CausesValidation = false;
            this.capAlign.Location = new System.Drawing.Point(8, 120);
            this.capAlign.Name = "capAlign";
            this.capAlign.Size = new System.Drawing.Size(88, 24);
            this.capAlign.Text = "Ausrichtung:";
            // 
            // cbxScriptType
            // 
            this.cbxScriptType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxScriptType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxScriptType.Location = new System.Drawing.Point(232, 272);
            this.cbxScriptType.Name = "cbxScriptType";
            this.cbxScriptType.RegexCheck = null;
            this.cbxScriptType.Size = new System.Drawing.Size(304, 24);
            this.cbxScriptType.TabIndex = 43;
            // 
            // capScriptType
            // 
            this.capScriptType.CausesValidation = false;
            this.capScriptType.Location = new System.Drawing.Point(8, 272);
            this.capScriptType.Name = "capScriptType";
            this.capScriptType.Size = new System.Drawing.Size(216, 24);
            this.capScriptType.Text = "Im Skript ist der Datentyp:";
            // 
            // chkFormatierungErlaubt
            // 
            this.chkFormatierungErlaubt.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkFormatierungErlaubt.Location = new System.Drawing.Point(312, 8);
            this.chkFormatierungErlaubt.Name = "chkFormatierungErlaubt";
            this.chkFormatierungErlaubt.Size = new System.Drawing.Size(296, 16);
            this.chkFormatierungErlaubt.TabIndex = 41;
            this.chkFormatierungErlaubt.Text = "Text-Formatierung erlaubt (Fett, Kursiv, etc.)";
            // 
            // cbxAdditionalCheck
            // 
            this.cbxAdditionalCheck.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAdditionalCheck.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAdditionalCheck.Location = new System.Drawing.Point(232, 232);
            this.cbxAdditionalCheck.Name = "cbxAdditionalCheck";
            this.cbxAdditionalCheck.RegexCheck = null;
            this.cbxAdditionalCheck.Size = new System.Drawing.Size(304, 24);
            this.cbxAdditionalCheck.TabIndex = 34;
            // 
            // capcbxAdditionalCheck
            // 
            this.capcbxAdditionalCheck.CausesValidation = false;
            this.capcbxAdditionalCheck.Location = new System.Drawing.Point(8, 232);
            this.capcbxAdditionalCheck.Name = "capcbxAdditionalCheck";
            this.capcbxAdditionalCheck.Size = new System.Drawing.Size(216, 40);
            this.capcbxAdditionalCheck.Text = "Zusätzliche Prüfung, ob der eingegeben Wert konsitent ist zu:";
            this.capcbxAdditionalCheck.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxChunk
            // 
            this.cbxChunk.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxChunk.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxChunk.Location = new System.Drawing.Point(840, 8);
            this.cbxChunk.Name = "cbxChunk";
            this.cbxChunk.RegexCheck = null;
            this.cbxChunk.Size = new System.Drawing.Size(232, 24);
            this.cbxChunk.TabIndex = 27;
            // 
            // capChunk
            // 
            this.capChunk.CausesValidation = false;
            this.capChunk.Location = new System.Drawing.Point(656, 8);
            this.capChunk.Name = "capChunk";
            this.capChunk.Size = new System.Drawing.Size(184, 16);
            this.capChunk.Text = "Werte zum Chunken benutzen:";
            this.capChunk.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // chkMultiline
            // 
            this.chkMultiline.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkMultiline.Location = new System.Drawing.Point(312, 24);
            this.chkMultiline.Name = "chkMultiline";
            this.chkMultiline.Size = new System.Drawing.Size(296, 16);
            this.chkMultiline.TabIndex = 7;
            this.chkMultiline.Text = "Mehrere Einträge pro Zelle erlaubt (mehrzeilig)";
            // 
            // tabBearbeitung
            // 
            this.tabBearbeitung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBearbeitung.Controls.Add(this.grpAuswahlmenuOptionen);
            this.tabBearbeitung.Controls.Add(this.btnIgnoreLock);
            this.tabBearbeitung.Controls.Add(this.lbxCellEditor);
            this.tabBearbeitung.Controls.Add(this.btnEditableStandard);
            this.tabBearbeitung.Controls.Add(this.capUserGroupEdit);
            this.tabBearbeitung.Controls.Add(this.btnEditableDropdown);
            this.tabBearbeitung.Location = new System.Drawing.Point(4, 25);
            this.tabBearbeitung.Name = "tabBearbeitung";
            this.tabBearbeitung.Padding = new System.Windows.Forms.Padding(3);
            this.tabBearbeitung.Size = new System.Drawing.Size(1098, 594);
            this.tabBearbeitung.TabIndex = 1;
            this.tabBearbeitung.Text = "Bearbeitung";
            // 
            // grpAuswahlmenuOptionen
            // 
            this.grpAuswahlmenuOptionen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpAuswahlmenuOptionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuswahlmenuOptionen.CausesValidation = false;
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnOtherValuesToo);
            this.grpAuswahlmenuOptionen.Controls.Add(this.txbAuswaehlbareWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.capImmerWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnCanBeEmpty);
            this.grpAuswahlmenuOptionen.Location = new System.Drawing.Point(32, 80);
            this.grpAuswahlmenuOptionen.Name = "grpAuswahlmenuOptionen";
            this.grpAuswahlmenuOptionen.Size = new System.Drawing.Size(536, 504);
            this.grpAuswahlmenuOptionen.TabIndex = 0;
            this.grpAuswahlmenuOptionen.TabStop = false;
            this.grpAuswahlmenuOptionen.Text = "Auswahlmenü-Optionen:";
            // 
            // btnOtherValuesToo
            // 
            this.btnOtherValuesToo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOtherValuesToo.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnOtherValuesToo.Location = new System.Drawing.Point(8, 464);
            this.btnOtherValuesToo.Name = "btnOtherValuesToo";
            this.btnOtherValuesToo.Size = new System.Drawing.Size(512, 32);
            this.btnOtherValuesToo.TabIndex = 7;
            this.btnOtherValuesToo.Text = "Auch Werte, die in anderen Zellen derselben Spalte vorhanden sind, werden zum Aus" +
    "wählen vorgeschlagen";
            // 
            // txbAuswaehlbareWerte
            // 
            this.txbAuswaehlbareWerte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAuswaehlbareWerte.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAuswaehlbareWerte.Location = new System.Drawing.Point(8, 64);
            this.txbAuswaehlbareWerte.MultiLine = true;
            this.txbAuswaehlbareWerte.Name = "txbAuswaehlbareWerte";
            this.txbAuswaehlbareWerte.RegexCheck = null;
            this.txbAuswaehlbareWerte.Size = new System.Drawing.Size(520, 392);
            this.txbAuswaehlbareWerte.SpellCheckingEnabled = true;
            this.txbAuswaehlbareWerte.TabIndex = 0;
            // 
            // capImmerWerte
            // 
            this.capImmerWerte.CausesValidation = false;
            this.capImmerWerte.Location = new System.Drawing.Point(8, 40);
            this.capImmerWerte.Name = "capImmerWerte";
            this.capImmerWerte.Size = new System.Drawing.Size(440, 24);
            this.capImmerWerte.Text = "<b><u>Immer auswählbare Werte:";
            this.capImmerWerte.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnCanBeEmpty
            // 
            this.btnCanBeEmpty.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnCanBeEmpty.Location = new System.Drawing.Point(8, 24);
            this.btnCanBeEmpty.Name = "btnCanBeEmpty";
            this.btnCanBeEmpty.Size = new System.Drawing.Size(328, 16);
            this.btnCanBeEmpty.TabIndex = 6;
            this.btnCanBeEmpty.Text = "Alles abwählen erlaubt (leere Zelle möglich)";
            // 
            // btnIgnoreLock
            // 
            this.btnIgnoreLock.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnIgnoreLock.Location = new System.Drawing.Point(592, 328);
            this.btnIgnoreLock.Name = "btnIgnoreLock";
            this.btnIgnoreLock.Size = new System.Drawing.Size(288, 32);
            this.btnIgnoreLock.TabIndex = 27;
            this.btnIgnoreLock.Text = "Die Bearbeitung ist auch möglich, wenn die Zeile abgeschlossen ist.";
            // 
            // lbxCellEditor
            // 
            this.lbxCellEditor.AddAllowed = BlueControls.Enums.AddType.Text;
            this.lbxCellEditor.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            this.lbxCellEditor.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.lbxCellEditor.FilterText = null;
            this.lbxCellEditor.Location = new System.Drawing.Point(576, 48);
            this.lbxCellEditor.Name = "lbxCellEditor";
            this.lbxCellEditor.Size = new System.Drawing.Size(328, 272);
            this.lbxCellEditor.TabIndex = 26;
            // 
            // btnEditableStandard
            // 
            this.btnEditableStandard.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnEditableStandard.Location = new System.Drawing.Point(8, 16);
            this.btnEditableStandard.Name = "btnEditableStandard";
            this.btnEditableStandard.Size = new System.Drawing.Size(544, 32);
            this.btnEditableStandard.TabIndex = 4;
            this.btnEditableStandard.Text = "Benutzer-Bearbeitung mit der <b>Standard-Methode</b> erlauben<br><i>Wenn neue Wer" +
    "te erlaubt sein sollen, muss hier ein Häkchen gesetzt werden.";
            // 
            // capUserGroupEdit
            // 
            this.capUserGroupEdit.CausesValidation = false;
            this.capUserGroupEdit.Location = new System.Drawing.Point(576, 16);
            this.capUserGroupEdit.Name = "capUserGroupEdit";
            this.capUserGroupEdit.Size = new System.Drawing.Size(328, 32);
            this.capUserGroupEdit.Text = "<b>Folgende Benutzergruppen dürfen den Inhalt der Zellen bearbeiten:";
            this.capUserGroupEdit.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnEditableDropdown
            // 
            this.btnEditableDropdown.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnEditableDropdown.Location = new System.Drawing.Point(8, 56);
            this.btnEditableDropdown.Name = "btnEditableDropdown";
            this.btnEditableDropdown.Size = new System.Drawing.Size(544, 16);
            this.btnEditableDropdown.TabIndex = 5;
            this.btnEditableDropdown.Text = "Benutzer-Bearbeitung mit <b>Auswahl-Menü (Dropdown-Menü)</b> erlauben";
            // 
            // chkSaveContent
            // 
            this.chkSaveContent.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkSaveContent.Location = new System.Drawing.Point(8, 40);
            this.chkSaveContent.Name = "chkSaveContent";
            this.chkSaveContent.Size = new System.Drawing.Size(296, 16);
            this.chkSaveContent.TabIndex = 32;
            this.chkSaveContent.Text = "Inhalt wird auf Festplatte gespeichert";
            // 
            // tabAutoKorrektur
            // 
            this.tabAutoKorrektur.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAutoKorrektur.Controls.Add(this.btnCalculateMaxCellLenght);
            this.tabAutoKorrektur.Controls.Add(this.txbAutoReplace);
            this.tabAutoKorrektur.Controls.Add(this.capAutoReplace);
            this.tabAutoKorrektur.Controls.Add(this.txbMaxCellLenght);
            this.tabAutoKorrektur.Controls.Add(this.txbAutoRemove);
            this.tabAutoKorrektur.Controls.Add(this.capAutoRemove);
            this.tabAutoKorrektur.Controls.Add(this.capMaxCellLenght);
            this.tabAutoKorrektur.Controls.Add(this.btnAutoEditKleineFehler);
            this.tabAutoKorrektur.Controls.Add(this.btnAutoEditToUpper);
            this.tabAutoKorrektur.Controls.Add(this.txbRunden);
            this.tabAutoKorrektur.Controls.Add(this.capNachkommastellen);
            this.tabAutoKorrektur.Controls.Add(this.btnAutoEditAutoSort);
            this.tabAutoKorrektur.Location = new System.Drawing.Point(4, 25);
            this.tabAutoKorrektur.Name = "tabAutoKorrektur";
            this.tabAutoKorrektur.Size = new System.Drawing.Size(1098, 594);
            this.tabAutoKorrektur.TabIndex = 6;
            this.tabAutoKorrektur.Text = "Auto-Korrektur";
            // 
            // btnCalculateMaxCellLenght
            // 
            this.btnCalculateMaxCellLenght.ImageCode = "Taschenrechner|16";
            this.btnCalculateMaxCellLenght.Location = new System.Drawing.Point(312, 88);
            this.btnCalculateMaxCellLenght.Name = "btnCalculateMaxCellLenght";
            this.btnCalculateMaxCellLenght.QuickInfo = "Prüft alle Zellen und berechnet die ideale\r\nmaximale Text Länge";
            this.btnCalculateMaxCellLenght.Size = new System.Drawing.Size(40, 24);
            this.btnCalculateMaxCellLenght.TabIndex = 46;
            this.btnCalculateMaxCellLenght.Click += new System.EventHandler(this.btnCalculateMaxCellLenght_Click);
            // 
            // txbAutoReplace
            // 
            this.txbAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAutoReplace.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAutoReplace.Location = new System.Drawing.Point(16, 192);
            this.txbAutoReplace.MultiLine = true;
            this.txbAutoReplace.Name = "txbAutoReplace";
            this.txbAutoReplace.QuickInfo = resources.GetString("txbAutoReplace.QuickInfo");
            this.txbAutoReplace.RegexCheck = null;
            this.txbAutoReplace.Size = new System.Drawing.Size(1073, 392);
            this.txbAutoReplace.SpellCheckingEnabled = true;
            this.txbAutoReplace.TabIndex = 39;
            // 
            // capAutoReplace
            // 
            this.capAutoReplace.CausesValidation = false;
            this.capAutoReplace.Location = new System.Drawing.Point(16, 168);
            this.capAutoReplace.Name = "capAutoReplace";
            this.capAutoReplace.Size = new System.Drawing.Size(184, 24);
            this.capAutoReplace.Text = "Permanente Ersetzungen:";
            // 
            // txbMaxCellLenght
            // 
            this.txbMaxCellLenght.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.Integer;
            this.txbMaxCellLenght.AllowedChars = "0123456789";
            this.txbMaxCellLenght.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbMaxCellLenght.Location = new System.Drawing.Point(216, 88);
            this.txbMaxCellLenght.MaxTextLenght = 255;
            this.txbMaxCellLenght.Name = "txbMaxCellLenght";
            this.txbMaxCellLenght.QuickInfo = resources.GetString("txbMaxCellLenght.QuickInfo");
            this.txbMaxCellLenght.RegexCheck = "^((-?[1-9]\\d*)|0)$";
            this.txbMaxCellLenght.Size = new System.Drawing.Size(96, 24);
            this.txbMaxCellLenght.TabIndex = 45;
            // 
            // txbAutoRemove
            // 
            this.txbAutoRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAutoRemove.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAutoRemove.Location = new System.Drawing.Point(16, 136);
            this.txbAutoRemove.Name = "txbAutoRemove";
            this.txbAutoRemove.RegexCheck = null;
            this.txbAutoRemove.Size = new System.Drawing.Size(1073, 24);
            this.txbAutoRemove.TabIndex = 37;
            // 
            // capAutoRemove
            // 
            this.capAutoRemove.CausesValidation = false;
            this.capAutoRemove.Location = new System.Drawing.Point(16, 120);
            this.capAutoRemove.Name = "capAutoRemove";
            this.capAutoRemove.Size = new System.Drawing.Size(568, 16);
            this.capAutoRemove.Text = "Folgende Zeichen automatisch aus der Eingabe löschen:";
            // 
            // capMaxCellLenght
            // 
            this.capMaxCellLenght.CausesValidation = false;
            this.capMaxCellLenght.Location = new System.Drawing.Point(16, 88);
            this.capMaxCellLenght.Name = "capMaxCellLenght";
            this.capMaxCellLenght.QuickInfo = "Falls mehrere Zeilen erlaubt sind, pro Zeile.\r\nAber es sind niemals mehr als 4000" +
    " Zeichen erlaubt.\r\nDa im UTF8-Format gespeichert wird, evtl. auch weniger.";
            this.capMaxCellLenght.Size = new System.Drawing.Size(160, 24);
            this.capMaxCellLenght.Text = "Maximale Zellen-Kapazität:";
            this.capMaxCellLenght.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnAutoEditKleineFehler
            // 
            this.btnAutoEditKleineFehler.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnAutoEditKleineFehler.Location = new System.Drawing.Point(440, 16);
            this.btnAutoEditKleineFehler.Name = "btnAutoEditKleineFehler";
            this.btnAutoEditKleineFehler.Size = new System.Drawing.Size(440, 24);
            this.btnAutoEditKleineFehler.TabIndex = 13;
            this.btnAutoEditKleineFehler.Text = "Kleinere Fehler, wie z.B. doppelte Leerzeichen automatisch korrigieren";
            // 
            // btnAutoEditToUpper
            // 
            this.btnAutoEditToUpper.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnAutoEditToUpper.Location = new System.Drawing.Point(16, 16);
            this.btnAutoEditToUpper.Name = "btnAutoEditToUpper";
            this.btnAutoEditToUpper.Size = new System.Drawing.Size(208, 24);
            this.btnAutoEditToUpper.TabIndex = 12;
            this.btnAutoEditToUpper.Text = "Texte in Grossbuchstaben ändern";
            // 
            // txbRunden
            // 
            this.txbRunden.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRunden.Location = new System.Drawing.Point(216, 48);
            this.txbRunden.Name = "txbRunden";
            this.txbRunden.RegexCheck = null;
            this.txbRunden.Size = new System.Drawing.Size(96, 24);
            this.txbRunden.TabIndex = 11;
            // 
            // capNachkommastellen
            // 
            this.capNachkommastellen.CausesValidation = false;
            this.capNachkommastellen.Location = new System.Drawing.Point(16, 48);
            this.capNachkommastellen.Name = "capNachkommastellen";
            this.capNachkommastellen.Size = new System.Drawing.Size(200, 16);
            this.capNachkommastellen.Text = "Zahlen runden auf Kommastellen:";
            // 
            // btnAutoEditAutoSort
            // 
            this.btnAutoEditAutoSort.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnAutoEditAutoSort.Location = new System.Drawing.Point(440, 40);
            this.btnAutoEditAutoSort.Name = "btnAutoEditAutoSort";
            this.btnAutoEditAutoSort.Size = new System.Drawing.Size(416, 24);
            this.btnAutoEditAutoSort.TabIndex = 10;
            this.btnAutoEditAutoSort.Text = "Mehrzeilige Einträge sortieren und doppelte entfernen";
            // 
            // tabFilter
            // 
            this.tabFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabFilter.Controls.Add(this.chkFilterOnlyOr);
            this.tabFilter.Controls.Add(this.chkFilterOnlyAND);
            this.tabFilter.Controls.Add(this.capJokerValue);
            this.tabFilter.Controls.Add(this.txbJoker);
            this.tabFilter.Controls.Add(this.btnZeilenFilterIgnorieren);
            this.tabFilter.Controls.Add(this.btnAutoFilterMoeglich);
            this.tabFilter.Controls.Add(this.btnAutoFilterTXTErlaubt);
            this.tabFilter.Controls.Add(this.btnAutoFilterErweitertErlaubt);
            this.tabFilter.Location = new System.Drawing.Point(4, 25);
            this.tabFilter.Name = "tabFilter";
            this.tabFilter.Padding = new System.Windows.Forms.Padding(3);
            this.tabFilter.Size = new System.Drawing.Size(1098, 594);
            this.tabFilter.TabIndex = 2;
            this.tabFilter.Text = "Filter";
            // 
            // chkFilterOnlyOr
            // 
            this.chkFilterOnlyOr.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkFilterOnlyOr.Location = new System.Drawing.Point(32, 88);
            this.chkFilterOnlyOr.Name = "chkFilterOnlyOr";
            this.chkFilterOnlyOr.Size = new System.Drawing.Size(192, 16);
            this.chkFilterOnlyOr.TabIndex = 35;
            this.chkFilterOnlyOr.Text = "nur <b>ODER</b>-Filterung erlauben";
            // 
            // chkFilterOnlyAND
            // 
            this.chkFilterOnlyAND.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkFilterOnlyAND.Location = new System.Drawing.Point(32, 72);
            this.chkFilterOnlyAND.Name = "chkFilterOnlyAND";
            this.chkFilterOnlyAND.Size = new System.Drawing.Size(192, 16);
            this.chkFilterOnlyAND.TabIndex = 34;
            this.chkFilterOnlyAND.Text = "nur <b>UND</b>-Filterung erlauben";
            // 
            // capJokerValue
            // 
            this.capJokerValue.CausesValidation = false;
            this.capJokerValue.Location = new System.Drawing.Point(4, 177);
            this.capJokerValue.Name = "capJokerValue";
            this.capJokerValue.Size = new System.Drawing.Size(312, 56);
            this.capJokerValue.Text = "Bei Autofilter-Aktionen, Zellen mit folgenden Inhalt <b>immer</b> anzeigen, wenn " +
    "ein Wert gewählt wurde:<br>(Joker)";
            this.capJokerValue.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbJoker
            // 
            this.txbJoker.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbJoker.Location = new System.Drawing.Point(4, 233);
            this.txbJoker.Name = "txbJoker";
            this.txbJoker.RegexCheck = null;
            this.txbJoker.Size = new System.Drawing.Size(312, 24);
            this.txbJoker.TabIndex = 7;
            // 
            // btnZeilenFilterIgnorieren
            // 
            this.btnZeilenFilterIgnorieren.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnZeilenFilterIgnorieren.Location = new System.Drawing.Point(8, 144);
            this.btnZeilenFilterIgnorieren.Name = "btnZeilenFilterIgnorieren";
            this.btnZeilenFilterIgnorieren.Size = new System.Drawing.Size(304, 16);
            this.btnZeilenFilterIgnorieren.TabIndex = 6;
            this.btnZeilenFilterIgnorieren.Text = "Bei Zeilenfilter ignorieren (Suchfeld-Eingabe)";
            // 
            // btnAutoFilterMoeglich
            // 
            this.btnAutoFilterMoeglich.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnAutoFilterMoeglich.Location = new System.Drawing.Point(12, 15);
            this.btnAutoFilterMoeglich.Name = "btnAutoFilterMoeglich";
            this.btnAutoFilterMoeglich.Size = new System.Drawing.Size(120, 16);
            this.btnAutoFilterMoeglich.TabIndex = 3;
            this.btnAutoFilterMoeglich.Text = "AutoFilter erlaubt";
            // 
            // btnAutoFilterTXTErlaubt
            // 
            this.btnAutoFilterTXTErlaubt.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnAutoFilterTXTErlaubt.Location = new System.Drawing.Point(32, 32);
            this.btnAutoFilterTXTErlaubt.Name = "btnAutoFilterTXTErlaubt";
            this.btnAutoFilterTXTErlaubt.Size = new System.Drawing.Size(208, 16);
            this.btnAutoFilterTXTErlaubt.TabIndex = 4;
            this.btnAutoFilterTXTErlaubt.Text = "AutoFilter - Texteingabe - erlaubt";
            // 
            // btnAutoFilterErweitertErlaubt
            // 
            this.btnAutoFilterErweitertErlaubt.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnAutoFilterErweitertErlaubt.Location = new System.Drawing.Point(32, 48);
            this.btnAutoFilterErweitertErlaubt.Name = "btnAutoFilterErweitertErlaubt";
            this.btnAutoFilterErweitertErlaubt.Size = new System.Drawing.Size(192, 16);
            this.btnAutoFilterErweitertErlaubt.TabIndex = 5;
            this.btnAutoFilterErweitertErlaubt.Text = "AutoFilter - Erweitert - erlaubt";
            // 
            // tabQuickInfo
            // 
            this.tabQuickInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabQuickInfo.Controls.Add(this.txbAdminInfo);
            this.tabQuickInfo.Controls.Add(this.txbQuickinfo);
            this.tabQuickInfo.Controls.Add(this.Caption18);
            this.tabQuickInfo.Controls.Add(this.Caption17);
            this.tabQuickInfo.Controls.Add(this.btnQI_Vorschau);
            this.tabQuickInfo.Location = new System.Drawing.Point(4, 25);
            this.tabQuickInfo.Name = "tabQuickInfo";
            this.tabQuickInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabQuickInfo.Size = new System.Drawing.Size(1098, 594);
            this.tabQuickInfo.TabIndex = 3;
            this.tabQuickInfo.Text = "Quickinfo";
            // 
            // txbAdminInfo
            // 
            this.txbAdminInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAdminInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAdminInfo.Location = new System.Drawing.Point(617, 24);
            this.txbAdminInfo.MultiLine = true;
            this.txbAdminInfo.Name = "txbAdminInfo";
            this.txbAdminInfo.RegexCheck = null;
            this.txbAdminInfo.Size = new System.Drawing.Size(473, 528);
            this.txbAdminInfo.SpellCheckingEnabled = true;
            this.txbAdminInfo.TabIndex = 3;
            this.txbAdminInfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbQuickinfo
            // 
            this.txbQuickinfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbQuickinfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbQuickinfo.Location = new System.Drawing.Point(8, 24);
            this.txbQuickinfo.MultiLine = true;
            this.txbQuickinfo.Name = "txbQuickinfo";
            this.txbQuickinfo.RegexCheck = null;
            this.txbQuickinfo.Size = new System.Drawing.Size(601, 528);
            this.txbQuickinfo.SpellCheckingEnabled = true;
            this.txbQuickinfo.TabIndex = 0;
            this.txbQuickinfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption18
            // 
            this.Caption18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Caption18.CausesValidation = false;
            this.Caption18.Location = new System.Drawing.Point(617, 8);
            this.Caption18.Name = "Caption18";
            this.Caption18.Size = new System.Drawing.Size(188, 15);
            this.Caption18.Text = "Administrator-Info:";
            this.Caption18.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption17
            // 
            this.Caption17.CausesValidation = false;
            this.Caption17.Location = new System.Drawing.Point(8, 8);
            this.Caption17.Name = "Caption17";
            this.Caption17.Size = new System.Drawing.Size(168, 16);
            this.Caption17.Text = "QuickInfo:";
            this.Caption17.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnQI_Vorschau
            // 
            this.btnQI_Vorschau.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQI_Vorschau.Location = new System.Drawing.Point(992, 560);
            this.btnQI_Vorschau.Name = "btnQI_Vorschau";
            this.btnQI_Vorschau.Size = new System.Drawing.Size(96, 24);
            this.btnQI_Vorschau.TabIndex = 1;
            this.btnQI_Vorschau.Text = "Vorschau";
            this.btnQI_Vorschau.Click += new System.EventHandler(this.btnQI_Vorschau_Click);
            // 
            // tabSonstiges
            // 
            this.tabSonstiges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSonstiges.Controls.Add(this.txbTags);
            this.tabSonstiges.Controls.Add(this.Caption8);
            this.tabSonstiges.Location = new System.Drawing.Point(4, 25);
            this.tabSonstiges.Name = "tabSonstiges";
            this.tabSonstiges.Padding = new System.Windows.Forms.Padding(3);
            this.tabSonstiges.Size = new System.Drawing.Size(1098, 594);
            this.tabSonstiges.TabIndex = 4;
            this.tabSonstiges.Text = "Sonstiges allgemein";
            // 
            // txbTags
            // 
            this.txbTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbTags.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTags.Location = new System.Drawing.Point(8, 32);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.RegexCheck = null;
            this.txbTags.Size = new System.Drawing.Size(1080, 552);
            this.txbTags.TabIndex = 30;
            this.txbTags.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption8
            // 
            this.Caption8.CausesValidation = false;
            this.Caption8.Location = new System.Drawing.Point(4, 15);
            this.Caption8.Name = "Caption8";
            this.Caption8.Size = new System.Drawing.Size(144, 16);
            this.Caption8.Text = "Sonstige Daten (Tags):";
            this.Caption8.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxSort
            // 
            this.cbxSort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSort.Location = new System.Drawing.Point(232, 312);
            this.cbxSort.Name = "cbxSort";
            this.cbxSort.RegexCheck = null;
            this.cbxSort.Size = new System.Drawing.Size(304, 24);
            this.cbxSort.TabIndex = 35;
            // 
            // txbRegex
            // 
            this.txbRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbRegex.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRegex.Location = new System.Drawing.Point(8, 168);
            this.txbRegex.Name = "txbRegex";
            this.txbRegex.Size = new System.Drawing.Size(1081, 48);
            this.txbRegex.TabIndex = 9;
            // 
            // capSortiermaske
            // 
            this.capSortiermaske.CausesValidation = false;
            this.capSortiermaske.Location = new System.Drawing.Point(8, 312);
            this.capSortiermaske.Name = "capSortiermaske";
            this.capSortiermaske.Size = new System.Drawing.Size(216, 40);
            this.capSortiermaske.Text = "Bei der Datenbank-Zeilen-Sortierung fungiert diese Spalte als:";
            this.capSortiermaske.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRegex
            // 
            this.capRegex.CausesValidation = false;
            this.capRegex.Location = new System.Drawing.Point(8, 152);
            this.capRegex.Name = "capRegex";
            this.capRegex.Size = new System.Drawing.Size(388, 24);
            this.capRegex.Text = "Die Eingabe muss mit dieser Regex-Maske übereinstimmen:";
            // 
            // txbAllowedChars
            // 
            this.txbAllowedChars.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAllowedChars.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAllowedChars.Location = new System.Drawing.Point(8, 88);
            this.txbAllowedChars.Name = "txbAllowedChars";
            this.txbAllowedChars.RegexCheck = null;
            this.txbAllowedChars.Size = new System.Drawing.Size(1081, 56);
            this.txbAllowedChars.TabIndex = 30;
            this.txbAllowedChars.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption13
            // 
            this.Caption13.CausesValidation = false;
            this.Caption13.Location = new System.Drawing.Point(8, 72);
            this.Caption13.Name = "Caption13";
            this.Caption13.Size = new System.Drawing.Size(352, 24);
            this.Caption13.Text = "Folgende Zeichen können vom Benutzer eingegeben werden:";
            this.Caption13.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxLinkedDatabase
            // 
            this.cbxLinkedDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxLinkedDatabase.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxLinkedDatabase.Location = new System.Drawing.Point(248, 48);
            this.cbxLinkedDatabase.Name = "cbxLinkedDatabase";
            this.cbxLinkedDatabase.RegexCheck = null;
            this.cbxLinkedDatabase.Size = new System.Drawing.Size(833, 24);
            this.cbxLinkedDatabase.TabIndex = 38;
            this.cbxLinkedDatabase.TextChanged += new System.EventHandler(this.cbxLinkedDatabase_TextChanged);
            // 
            // capLinkedDatabase
            // 
            this.capLinkedDatabase.CausesValidation = false;
            this.capLinkedDatabase.Location = new System.Drawing.Point(8, 48);
            this.capLinkedDatabase.Name = "capLinkedDatabase";
            this.capLinkedDatabase.Size = new System.Drawing.Size(152, 16);
            this.capLinkedDatabase.Text = "Vernküpfte Datenbank:";
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BlueFrame1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.btnSystemInfo);
            this.BlueFrame1.Controls.Add(this.btnVerwendung);
            this.BlueFrame1.Controls.Add(this.capInfo);
            this.BlueFrame1.Controls.Add(this.Caption3);
            this.BlueFrame1.Controls.Add(this.txbName);
            this.BlueFrame1.Controls.Add(this.Caption2);
            this.BlueFrame1.Controls.Add(this.txbCaption);
            this.BlueFrame1.Location = new System.Drawing.Point(8, 24);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(1102, 104);
            this.BlueFrame1.TabIndex = 16;
            this.BlueFrame1.TabStop = false;
            this.BlueFrame1.Text = "Allgemein";
            // 
            // btnSystemInfo
            // 
            this.btnSystemInfo.Location = new System.Drawing.Point(152, 80);
            this.btnSystemInfo.Name = "btnSystemInfo";
            this.btnSystemInfo.Size = new System.Drawing.Size(152, 24);
            this.btnSystemInfo.TabIndex = 4;
            this.btnSystemInfo.Text = "Systeminfos zurücksetzen";
            this.btnSystemInfo.Click += new System.EventHandler(this.btnSystemInfo_Click);
            // 
            // btnVerwendung
            // 
            this.btnVerwendung.Location = new System.Drawing.Point(8, 80);
            this.btnVerwendung.Name = "btnVerwendung";
            this.btnVerwendung.Size = new System.Drawing.Size(128, 24);
            this.btnVerwendung.TabIndex = 3;
            this.btnVerwendung.Text = "Verwendungs-Info";
            this.btnVerwendung.Click += new System.EventHandler(this.btnVerwendung_Click);
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 16);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(280, 19);
            this.capInfo.Text = "NR";
            this.capInfo.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new System.Drawing.Point(8, 40);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new System.Drawing.Size(136, 16);
            this.Caption3.Text = "Interner Spaltename:";
            this.Caption3.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbName
            // 
            this.txbName.AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.[]()";
            this.txbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbName.Location = new System.Drawing.Point(8, 56);
            this.txbName.Name = "txbName";
            this.txbName.RegexCheck = null;
            this.txbName.Size = new System.Drawing.Size(296, 24);
            this.txbName.TabIndex = 0;
            // 
            // Caption2
            // 
            this.Caption2.CausesValidation = false;
            this.Caption2.Location = new System.Drawing.Point(312, 16);
            this.Caption2.Name = "Caption2";
            this.Caption2.Size = new System.Drawing.Size(144, 16);
            this.Caption2.Text = "Angezeigte Beschriftung:";
            this.Caption2.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbCaption.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbCaption.Location = new System.Drawing.Point(312, 32);
            this.txbCaption.MultiLine = true;
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.RegexCheck = null;
            this.txbCaption.Size = new System.Drawing.Size(785, 64);
            this.txbCaption.TabIndex = 2;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.ImageCode = "Häkchen|16";
            this.btnOk.Location = new System.Drawing.Point(1026, 765);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(72, 24);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabDatenFormat);
            this.tabControl.Controls.Add(this.tabAnzeige);
            this.tabControl.Controls.Add(this.tabBearbeitung);
            this.tabControl.Controls.Add(this.tabAutoKorrektur);
            this.tabControl.Controls.Add(this.tabFilter);
            this.tabControl.Controls.Add(this.tabQuickInfo);
            this.tabControl.Controls.Add(this.tabSonstiges);
            this.tabControl.Controls.Add(this.tabSpaltenVerlinkung);
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new System.Drawing.Point(0, 136);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1106, 623);
            this.tabControl.TabDefault = this.tabDatenFormat;
            this.tabControl.TabDefaultOrder = null;
            this.tabControl.TabIndex = 15;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabDatenFormat
            // 
            this.tabDatenFormat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabDatenFormat.Controls.Add(this.chkRelation);
            this.tabDatenFormat.Controls.Add(this.chkIsKeyColumn);
            this.tabDatenFormat.Controls.Add(this.chkSaveContent);
            this.tabDatenFormat.Controls.Add(this.chkIsFirst);
            this.tabDatenFormat.Controls.Add(this.btnMaxTextLenght);
            this.tabDatenFormat.Controls.Add(this.txbMaxTextLenght);
            this.tabDatenFormat.Controls.Add(this.capMaxTextLenght);
            this.tabDatenFormat.Controls.Add(this.txbAllowedChars);
            this.tabDatenFormat.Controls.Add(this.grpSchnellformat);
            this.tabDatenFormat.Controls.Add(this.cbxSort);
            this.tabDatenFormat.Controls.Add(this.capSortiermaske);
            this.tabDatenFormat.Controls.Add(this.txbRegex);
            this.tabDatenFormat.Controls.Add(this.capRegex);
            this.tabDatenFormat.Controls.Add(this.Caption13);
            this.tabDatenFormat.Controls.Add(this.chkFormatierungErlaubt);
            this.tabDatenFormat.Controls.Add(this.cbxAdditionalCheck);
            this.tabDatenFormat.Controls.Add(this.capcbxAdditionalCheck);
            this.tabDatenFormat.Controls.Add(this.cbxScriptType);
            this.tabDatenFormat.Controls.Add(this.capScriptType);
            this.tabDatenFormat.Controls.Add(this.cbxChunk);
            this.tabDatenFormat.Controls.Add(this.capChunk);
            this.tabDatenFormat.Controls.Add(this.chkMultiline);
            this.tabDatenFormat.Location = new System.Drawing.Point(4, 25);
            this.tabDatenFormat.Name = "tabDatenFormat";
            this.tabDatenFormat.Size = new System.Drawing.Size(1098, 594);
            this.tabDatenFormat.TabIndex = 12;
            this.tabDatenFormat.Text = "Daten-Format";
            // 
            // chkIsKeyColumn
            // 
            this.chkIsKeyColumn.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkIsKeyColumn.Location = new System.Drawing.Point(8, 24);
            this.chkIsKeyColumn.Name = "chkIsKeyColumn";
            this.chkIsKeyColumn.QuickInfo = "Wenn der Imhalt dieser Spalte geändert wird,\r\nwird eine erweitertes Skript angest" +
    "oßen.";
            this.chkIsKeyColumn.Size = new System.Drawing.Size(296, 16);
            this.chkIsKeyColumn.TabIndex = 50;
            this.chkIsKeyColumn.Text = "Diese Spalte ist eine Schlüsselspalte";
            // 
            // chkIsFirst
            // 
            this.chkIsFirst.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkIsFirst.Location = new System.Drawing.Point(8, 8);
            this.chkIsFirst.Name = "chkIsFirst";
            this.chkIsFirst.Size = new System.Drawing.Size(296, 16);
            this.chkIsFirst.TabIndex = 49;
            this.chkIsFirst.Text = "Diese Spalte ist die funktionelle erste Spalte";
            // 
            // btnMaxTextLenght
            // 
            this.btnMaxTextLenght.ImageCode = "Taschenrechner|16";
            this.btnMaxTextLenght.Location = new System.Drawing.Point(816, 232);
            this.btnMaxTextLenght.Name = "btnMaxTextLenght";
            this.btnMaxTextLenght.QuickInfo = "Prüft alle Zellen und berechnet die ideale\r\nmaximale Text Länge";
            this.btnMaxTextLenght.Size = new System.Drawing.Size(40, 24);
            this.btnMaxTextLenght.TabIndex = 48;
            this.btnMaxTextLenght.Click += new System.EventHandler(this.btnMaxTextLenght_Click);
            // 
            // txbMaxTextLenght
            // 
            this.txbMaxTextLenght.AdditionalFormatCheck = BlueBasics.Enums.AdditionalCheck.Integer;
            this.txbMaxTextLenght.AllowedChars = "0123456789";
            this.txbMaxTextLenght.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbMaxTextLenght.Location = new System.Drawing.Point(720, 232);
            this.txbMaxTextLenght.MaxTextLenght = 255;
            this.txbMaxTextLenght.Name = "txbMaxTextLenght";
            this.txbMaxTextLenght.QuickInfo = resources.GetString("txbMaxTextLenght.QuickInfo");
            this.txbMaxTextLenght.RegexCheck = "^((-?[1-9]\\d*)|0)$";
            this.txbMaxTextLenght.Size = new System.Drawing.Size(96, 24);
            this.txbMaxTextLenght.TabIndex = 47;
            // 
            // capMaxTextLenght
            // 
            this.capMaxTextLenght.CausesValidation = false;
            this.capMaxTextLenght.Location = new System.Drawing.Point(560, 232);
            this.capMaxTextLenght.Name = "capMaxTextLenght";
            this.capMaxTextLenght.QuickInfo = "Pro Zeile!\r\nEs wird wirklich die Anzahl der Zeichen gezählt.\r\nEs bezeht sich nur " +
    "auf das Format, und es wird evtl. eine Meldung ausgegeben,\r\ndas die Eingabe nich" +
    "t dem Format entspricht.";
            this.capMaxTextLenght.Size = new System.Drawing.Size(160, 24);
            this.capMaxTextLenght.Text = "Maximale Text-Länge:";
            this.capMaxTextLenght.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpSchnellformat
            // 
            this.grpSchnellformat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSchnellformat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpSchnellformat.Controls.Add(this.btnSchnellText);
            this.grpSchnellformat.Controls.Add(this.btnSchnellBit);
            this.grpSchnellformat.Controls.Add(this.btnSchnellDatum);
            this.grpSchnellformat.Controls.Add(this.btnSchnellBildCode);
            this.grpSchnellformat.Controls.Add(this.btnSchnellDatumUhrzeit);
            this.grpSchnellformat.Controls.Add(this.btnSchnellIInternetAdresse);
            this.grpSchnellformat.Controls.Add(this.btnSchnellEmail);
            this.grpSchnellformat.Controls.Add(this.btnSchnellAuswahloptionen);
            this.grpSchnellformat.Controls.Add(this.btnSchnellTelefonNummer);
            this.grpSchnellformat.Controls.Add(this.btnSchnellGleitkommazahl);
            this.grpSchnellformat.Controls.Add(this.btnSchnellGanzzahl);
            this.grpSchnellformat.Location = new System.Drawing.Point(8, 384);
            this.grpSchnellformat.Name = "grpSchnellformat";
            this.grpSchnellformat.Size = new System.Drawing.Size(1081, 80);
            this.grpSchnellformat.TabIndex = 11;
            this.grpSchnellformat.TabStop = false;
            this.grpSchnellformat.Text = "Schnell-Format";
            // 
            // btnSchnellText
            // 
            this.btnSchnellText.Location = new System.Drawing.Point(8, 16);
            this.btnSchnellText.Name = "btnSchnellText";
            this.btnSchnellText.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellText.TabIndex = 0;
            this.btnSchnellText.Text = "Text";
            this.btnSchnellText.Click += new System.EventHandler(this.btnSchnellText_Click);
            // 
            // btnSchnellBit
            // 
            this.btnSchnellBit.Location = new System.Drawing.Point(280, 16);
            this.btnSchnellBit.Name = "btnSchnellBit";
            this.btnSchnellBit.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellBit.TabIndex = 10;
            this.btnSchnellBit.Text = "Bit (Ja/Nein)";
            this.btnSchnellBit.Click += new System.EventHandler(this.btnSchnellBit_Click);
            // 
            // btnSchnellDatum
            // 
            this.btnSchnellDatum.Location = new System.Drawing.Point(8, 48);
            this.btnSchnellDatum.Name = "btnSchnellDatum";
            this.btnSchnellDatum.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellDatum.TabIndex = 1;
            this.btnSchnellDatum.Text = "Datum";
            this.btnSchnellDatum.Click += new System.EventHandler(this.btnSchnellDatum_Click);
            // 
            // btnSchnellBildCode
            // 
            this.btnSchnellBildCode.Location = new System.Drawing.Point(688, 48);
            this.btnSchnellBildCode.Name = "btnSchnellBildCode";
            this.btnSchnellBildCode.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellBildCode.TabIndex = 9;
            this.btnSchnellBildCode.Text = "Bild-Code";
            this.btnSchnellBildCode.Click += new System.EventHandler(this.btnSchnellBildCode_Click);
            // 
            // btnSchnellDatumUhrzeit
            // 
            this.btnSchnellDatumUhrzeit.Location = new System.Drawing.Point(144, 48);
            this.btnSchnellDatumUhrzeit.Name = "btnSchnellDatumUhrzeit";
            this.btnSchnellDatumUhrzeit.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellDatumUhrzeit.TabIndex = 2;
            this.btnSchnellDatumUhrzeit.Text = "Datum/Uhrzeit";
            this.btnSchnellDatumUhrzeit.Click += new System.EventHandler(this.btnSchnellDatumUhrzeit_Click);
            // 
            // btnSchnellIInternetAdresse
            // 
            this.btnSchnellIInternetAdresse.Location = new System.Drawing.Point(552, 48);
            this.btnSchnellIInternetAdresse.Name = "btnSchnellIInternetAdresse";
            this.btnSchnellIInternetAdresse.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellIInternetAdresse.TabIndex = 8;
            this.btnSchnellIInternetAdresse.Text = "Internet-Adresse";
            this.btnSchnellIInternetAdresse.Click += new System.EventHandler(this.btnSchnellIInternetAdresse_Click);
            // 
            // btnSchnellEmail
            // 
            this.btnSchnellEmail.Location = new System.Drawing.Point(280, 48);
            this.btnSchnellEmail.Name = "btnSchnellEmail";
            this.btnSchnellEmail.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellEmail.TabIndex = 3;
            this.btnSchnellEmail.Text = "Email";
            this.btnSchnellEmail.Click += new System.EventHandler(this.btnSchnellEmail_Click);
            // 
            // btnSchnellAuswahloptionen
            // 
            this.btnSchnellAuswahloptionen.Location = new System.Drawing.Point(144, 16);
            this.btnSchnellAuswahloptionen.Name = "btnSchnellAuswahloptionen";
            this.btnSchnellAuswahloptionen.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellAuswahloptionen.TabIndex = 7;
            this.btnSchnellAuswahloptionen.Text = "Auswahl-Optionen";
            this.btnSchnellAuswahloptionen.Click += new System.EventHandler(this.btnSchnellAuswahloptionen_Click);
            // 
            // btnSchnellTelefonNummer
            // 
            this.btnSchnellTelefonNummer.Location = new System.Drawing.Point(416, 48);
            this.btnSchnellTelefonNummer.Name = "btnSchnellTelefonNummer";
            this.btnSchnellTelefonNummer.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellTelefonNummer.TabIndex = 4;
            this.btnSchnellTelefonNummer.Text = "Telefonnummer";
            this.btnSchnellTelefonNummer.Click += new System.EventHandler(this.btnSchnellTelefonNummer_Click);
            // 
            // btnSchnellGleitkommazahl
            // 
            this.btnSchnellGleitkommazahl.Location = new System.Drawing.Point(688, 16);
            this.btnSchnellGleitkommazahl.Name = "btnSchnellGleitkommazahl";
            this.btnSchnellGleitkommazahl.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellGleitkommazahl.TabIndex = 6;
            this.btnSchnellGleitkommazahl.Text = "Gleitkommazahl";
            this.btnSchnellGleitkommazahl.Click += new System.EventHandler(this.btnSchnellGleitkommazahl_Click);
            // 
            // btnSchnellGanzzahl
            // 
            this.btnSchnellGanzzahl.Location = new System.Drawing.Point(552, 16);
            this.btnSchnellGanzzahl.Name = "btnSchnellGanzzahl";
            this.btnSchnellGanzzahl.Size = new System.Drawing.Size(128, 24);
            this.btnSchnellGanzzahl.TabIndex = 5;
            this.btnSchnellGanzzahl.Text = "Ganzzahl";
            this.btnSchnellGanzzahl.Click += new System.EventHandler(this.btnSchnellGanzzahl_Click);
            // 
            // tabSpaltenVerlinkung
            // 
            this.tabSpaltenVerlinkung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxRelationType);
            this.tabSpaltenVerlinkung.Controls.Add(this.caption1);
            this.tabSpaltenVerlinkung.Controls.Add(this.tblFilterliste);
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxTargetColumn);
            this.tabSpaltenVerlinkung.Controls.Add(this.capLinkedDatabase);
            this.tabSpaltenVerlinkung.Controls.Add(this.capTargetColumn);
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxLinkedDatabase);
            this.tabSpaltenVerlinkung.Location = new System.Drawing.Point(4, 25);
            this.tabSpaltenVerlinkung.Name = "tabSpaltenVerlinkung";
            this.tabSpaltenVerlinkung.Size = new System.Drawing.Size(1098, 594);
            this.tabSpaltenVerlinkung.TabIndex = 11;
            this.tabSpaltenVerlinkung.Text = "Spalten-Verlinkung";
            // 
            // tblFilterliste
            // 
            this.tblFilterliste.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblFilterliste.Location = new System.Drawing.Point(8, 112);
            this.tblFilterliste.Name = "tblFilterliste";
            this.tblFilterliste.SheetStyle = "Windows 11";
            this.tblFilterliste.Size = new System.Drawing.Size(1073, 472);
            this.tblFilterliste.TabIndex = 39;
            // 
            // cbxTargetColumn
            // 
            this.cbxTargetColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxTargetColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTargetColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTargetColumn.Location = new System.Drawing.Point(248, 80);
            this.cbxTargetColumn.Name = "cbxTargetColumn";
            this.cbxTargetColumn.RegexCheck = null;
            this.cbxTargetColumn.Size = new System.Drawing.Size(833, 24);
            this.cbxTargetColumn.TabIndex = 5;
            this.cbxTargetColumn.TextChanged += new System.EventHandler(this.cbxTargetColumn_TextChanged);
            // 
            // capTargetColumn
            // 
            this.capTargetColumn.CausesValidation = false;
            this.capTargetColumn.Location = new System.Drawing.Point(8, 80);
            this.capTargetColumn.Name = "capTargetColumn";
            this.capTargetColumn.Size = new System.Drawing.Size(200, 16);
            this.capTargetColumn.Text = "Aus dieser Spalte die Werte holen:";
            this.capTargetColumn.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // caption5
            // 
            this.caption5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.caption5.CausesValidation = false;
            this.caption5.Location = new System.Drawing.Point(185, 765);
            this.caption5.Name = "caption5";
            this.caption5.Size = new System.Drawing.Size(104, 24);
            this.caption5.Text = "Aktuelle Ansicht:";
            // 
            // butAktuellVor
            // 
            this.butAktuellVor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAktuellVor.ImageCode = "Pfeil_Rechts|16|||0000FF";
            this.butAktuellVor.Location = new System.Drawing.Point(377, 765);
            this.butAktuellVor.Name = "butAktuellVor";
            this.butAktuellVor.Size = new System.Drawing.Size(72, 24);
            this.butAktuellVor.TabIndex = 19;
            this.butAktuellVor.Click += new System.EventHandler(this.butAktuellVor_Click);
            // 
            // butAktuellZurueck
            // 
            this.butAktuellZurueck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAktuellZurueck.ImageCode = "Pfeil_Links|16|||0000FF";
            this.butAktuellZurueck.Location = new System.Drawing.Point(297, 765);
            this.butAktuellZurueck.Name = "butAktuellZurueck";
            this.butAktuellZurueck.Size = new System.Drawing.Size(72, 24);
            this.butAktuellZurueck.TabIndex = 18;
            this.butAktuellZurueck.Click += new System.EventHandler(this.butAktuellZurueck_Click);
            // 
            // capTabellenname
            // 
            this.capTabellenname.CausesValidation = false;
            this.capTabellenname.Location = new System.Drawing.Point(40, 0);
            this.capTabellenname.Name = "capTabellenname";
            this.capTabellenname.Size = new System.Drawing.Size(940, 24);
            this.capTabellenname.Text = "Tabellenname:";
            this.capTabellenname.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            this.capTabellenname.Translate = false;
            // 
            // btnSpaltenkopf
            // 
            this.btnSpaltenkopf.ImageCode = "Stift|16";
            this.btnSpaltenkopf.Location = new System.Drawing.Point(8, 0);
            this.btnSpaltenkopf.Name = "btnSpaltenkopf";
            this.btnSpaltenkopf.QuickInfo = "Spaltenkopf bearbeiten";
            this.btnSpaltenkopf.Size = new System.Drawing.Size(32, 24);
            this.btnSpaltenkopf.TabIndex = 49;
            this.btnSpaltenkopf.Click += new System.EventHandler(this.btnSpaltenkopf_Click);
            // 
            // chkRelation
            // 
            this.chkRelation.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkRelation.Location = new System.Drawing.Point(312, 40);
            this.chkRelation.Name = "chkRelation";
            this.chkRelation.Size = new System.Drawing.Size(336, 16);
            this.chkRelation.TabIndex = 51;
            this.chkRelation.Text = "Beziehungen automatisch mit erster Spalte abgleichen";
            // 
            // cbxRelationType
            // 
            this.cbxRelationType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRelationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRelationType.Location = new System.Drawing.Point(248, 8);
            this.cbxRelationType.Name = "cbxRelationType";
            this.cbxRelationType.RegexCheck = null;
            this.cbxRelationType.Size = new System.Drawing.Size(232, 24);
            this.cbxRelationType.TabIndex = 41;
            // 
            // caption1
            // 
            this.caption1.CausesValidation = false;
            this.caption1.Location = new System.Drawing.Point(8, 8);
            this.caption1.Name = "caption1";
            this.caption1.Size = new System.Drawing.Size(232, 16);
            this.caption1.Text = "Werte aus anderer Datenbank benutzen:";
            this.caption1.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // ColumnEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1112, 796);
            this.Controls.Add(this.btnSpaltenkopf);
            this.Controls.Add(this.capTabellenname);
            this.Controls.Add(this.caption5);
            this.Controls.Add(this.butAktuellVor);
            this.Controls.Add(this.butAktuellZurueck);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ColumnEditor";
            this.Text = "Spalten-Eigenschaften";
            this.tabAnzeige.ResumeLayout(false);
            this.tabBearbeitung.ResumeLayout(false);
            this.grpAuswahlmenuOptionen.ResumeLayout(false);
            this.tabAutoKorrektur.ResumeLayout(false);
            this.tabFilter.ResumeLayout(false);
            this.tabQuickInfo.ResumeLayout(false);
            this.tabSonstiges.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabDatenFormat.ResumeLayout(false);
            this.grpSchnellformat.ResumeLayout(false);
            this.tabSpaltenVerlinkung.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnOk;
        private TextBox txbName;
        private Caption Caption3;
        private ColorDialog ColorDia;
        private Button btnQI_Vorschau;
        private TextBox txbQuickinfo;
        private Button btnZeilenFilterIgnorieren;
        private Button btnAutoFilterErweitertErlaubt;
        private Button btnAutoFilterTXTErlaubt;
        private Button btnAutoFilterMoeglich;
        private Caption capInfo;
        private Button chkMultiline;
        private Caption capRechterRand;
        private Caption capLinkerRand;
        private Button btnTextColor;
        private Button btnBackColor;
        private Caption Caption2;
        private TextBox txbCaption;
        private GroupBox BlueFrame1;
        private Button btnEditableDropdown;
        private Button btnEditableStandard;
        private TextBox txbAuswaehlbareWerte;
        private Button btnCanBeEmpty;
        private Caption capUserGroupEdit;
        private Caption Caption8;
        private ComboBox cbxRandRechts;
        private ComboBox cbxRandLinks;
        private Caption Caption13;
        private TextBox txbAllowedChars;
        private Caption capImmerWerte;
        private Caption capChunk;
        private Caption Caption18;
        private TextBox txbAdminInfo;
        private Caption Caption17;
        private Button btnOtherValuesToo;
        private ListBox lbxCellEditor;
        private ComboBox cbxChunk;
        private TextBox txbTags;
        private TabControl tabControl;
        private TabPage tabAnzeige;
        private TabPage tabBearbeitung;
        private TabPage tabAutoKorrektur;
        private TabPage tabFilter;
        private TabPage tabQuickInfo;
        private TabPage tabSonstiges;
        private Button btnIgnoreLock;
        private Button chkSaveContent;
        private Button btnSpellChecking;
        private Caption capSpaltenbild;
        private TextBox txbJoker;
        private Caption capJokerValue;
        private Button btnAutoEditKleineFehler;
        private Button btnAutoEditToUpper;
        private TextBox txbRunden;
        private Caption capNachkommastellen;
        private Button btnAutoEditAutoSort;
        private ComboBox cbxLinkedDatabase;
        private Caption capLinkedDatabase;
        private ComboBox cbxTargetColumn;
        private Caption capTargetColumn;
        private Caption capUeberschrift3;
        private Caption capUeberschrift2;
        private Caption capUeberschrift1;
        private TextBox txbUeberschift3;
        private TextBox txbUeberschift2;
        private TextBox txbUeberschift1;
        private Button btnStandard;
        private Caption capSortiermaske;
        private TextBox txbRegex;
        private Caption capRegex;
        private ComboBox cbxAlign;
        private Caption capAlign;
        private GroupBox grpAuswahlmenuOptionen;
        private TextBox txbAutoRemove;
        private Caption capAutoRemove;
        private Caption caption5;
        private Button butAktuellVor;
        private Button butAktuellZurueck;
        private TextBox txbAutoReplace;
        private Caption capAutoReplace;
        private Button chkFilterOnlyAND;
        private Button chkFilterOnlyOr;
        private Button btnVerwendung;
        private TextBox txbSpaltenbild;
        private Button btnSchnellAuswahloptionen;
        private Button btnSchnellGleitkommazahl;
        private Button btnSchnellGanzzahl;
        private Button btnSchnellTelefonNummer;
        private Button btnSchnellEmail;
        private Button btnSchnellDatumUhrzeit;
        private Button btnSchnellDatum;
        private Button btnSchnellText;
        private Button btnSchnellIInternetAdresse;
        private ComboBox cbxSort;
        private ComboBox cbxTranslate;
        private Caption capTranslate;
        private Button chkFormatierungErlaubt;
        private ComboBox cbxAdditionalCheck;
        private Caption capcbxAdditionalCheck;
        private Button btnSchnellBildCode;
        private ComboBox cbxScriptType;
        private Caption capScriptType;
        private Button btnSchnellBit;
        private TabPage tabDatenFormat;
        private GroupBox grpSchnellformat;
        private TabPage tabSpaltenVerlinkung;
        private Table tblFilterliste;
        private TextBox txbMaxCellLenght;
        private Caption capMaxCellLenght;
        private Button btnCalculateMaxCellLenght;
        private Caption capTabellenname;
        private TextBox txbFixedColumnWidth;
        private Caption capFixedColumnWidth;
        private TextBox txbMaxTextLenght;
        private Caption capMaxTextLenght;
        private Button btnMaxTextLenght;
        private Button btnSpaltenkopf;
        private ComboBox cbxRenderer;
        private Caption capRenderer;
        private GroupBox RendererEditor;
        private Button btnSystemInfo;
        private Button chkIsKeyColumn;
        private Button chkIsFirst;
        private Button chkRelation;
        private ComboBox cbxRelationType;
        private Caption caption1;
    }
}
