using BlueControls.Controls;
using BlueControls.Forms;
using System.Diagnostics;


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
            this.RendererEditor = new GroupBox();
            this.cbxRenderer = new BlueControls.Controls.ComboBox();
            this.capRenderer = new BlueControls.Controls.Caption();
            this.btnSpellChecking = new BlueControls.Controls.Button();
            this.cbxTranslate = new BlueControls.Controls.ComboBox();
            this.txbBildCodeConstHeight = new BlueControls.Controls.TextBox();
            this.capBildCodeConstHeight = new BlueControls.Controls.Caption();
            this.capTranslate = new BlueControls.Controls.Caption();
            this.cbxBildTextVerhalten = new BlueControls.Controls.ComboBox();
            this.cbxAlign = new BlueControls.Controls.ComboBox();
            this.txbReplacer = new BlueControls.Controls.TextBox();
            this.capBildCodeImageNotfound = new BlueControls.Controls.Caption();
            this.btnStandard = new BlueControls.Controls.Button();
            this.capAlign = new BlueControls.Controls.Caption();
            this.capReplacer = new BlueControls.Controls.Caption();
            this.txbFixedColumnWidth = new BlueControls.Controls.TextBox();
            this.capFixedColumnWidth = new BlueControls.Controls.Caption();
            this.txbSpaltenbild = new BlueControls.Controls.TextBox();
            this.capUeberschrift3 = new BlueControls.Controls.Caption();
            this.capUeberschrift2 = new BlueControls.Controls.Caption();
            this.capUeberschrift1 = new BlueControls.Controls.Caption();
            this.txbUeberschift3 = new BlueControls.Controls.TextBox();
            this.txbUeberschift2 = new BlueControls.Controls.TextBox();
            this.txbUeberschift1 = new BlueControls.Controls.TextBox();
            this.capSpaltenbild = new BlueControls.Controls.Caption();
            this.btnBackColor = new BlueControls.Controls.Button();
            this.cbxRandRechts = new BlueControls.Controls.ComboBox();
            this.btnTextColor = new BlueControls.Controls.Button();
            this.cbxRandLinks = new BlueControls.Controls.ComboBox();
            this.capLinkerRand = new BlueControls.Controls.Caption();
            this.capRechterRand = new BlueControls.Controls.Caption();
            this.cbxScriptType = new BlueControls.Controls.ComboBox();
            this.capScriptType = new BlueControls.Controls.Caption();
            this.btnFormatierungErlaubt = new BlueControls.Controls.Button();
            this.cbxAdditionalCheck = new BlueControls.Controls.ComboBox();
            this.capcbxAdditionalCheck = new BlueControls.Controls.Caption();
            this.cbxFunction = new BlueControls.Controls.ComboBox();
            this.capFunction = new BlueControls.Controls.Caption();
            this.btnMultiline = new BlueControls.Controls.Button();
            this.tabBearbeitung = new System.Windows.Forms.TabPage();
            this.grpAuswahlmenuOptionen = new BlueControls.Controls.GroupBox();
            this.btnOtherValuesToo = new BlueControls.Controls.Button();
            this.txbAuswaehlbareWerte = new BlueControls.Controls.TextBox();
            this.capImmerWerte = new BlueControls.Controls.Caption();
            this.btnCanBeEmpty = new BlueControls.Controls.Button();
            this.btnLogUndo = new BlueControls.Controls.Button();
            this.btnIgnoreLock = new BlueControls.Controls.Button();
            this.lbxCellEditor = new BlueControls.Controls.ListBox();
            this.btnEditableStandard = new BlueControls.Controls.Button();
            this.capUserGroupEdit = new BlueControls.Controls.Caption();
            this.btnEditableDropdown = new BlueControls.Controls.Button();
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
            this.btnVerwendung = new BlueControls.Controls.Button();
            this.capInfo = new BlueControls.Controls.Caption();
            this.Caption3 = new BlueControls.Controls.Caption();
            this.txbName = new BlueControls.Controls.TextBox();
            this.Caption2 = new BlueControls.Controls.Caption();
            this.txbCaption = new BlueControls.Controls.TextBox();
            this.btnOk = new BlueControls.Controls.Button();
            this.tabControl = new BlueControls.Controls.TabControl();
            this.tabDatenFormat = new System.Windows.Forms.TabPage();
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
            this.tabSpaltenKopf = new System.Windows.Forms.TabPage();
            this.tabSpaltenVerlinkung = new System.Windows.Forms.TabPage();
            this.tblFilterliste = new BlueControls.Controls.Table();
            this.cbxTargetColumn = new BlueControls.Controls.ComboBox();
            this.capTargetColumn = new BlueControls.Controls.Caption();
            this.caption5 = new BlueControls.Controls.Caption();
            this.butAktuellVor = new BlueControls.Controls.Button();
            this.butAktuellZurueck = new BlueControls.Controls.Button();
            this.capTabellenname = new BlueControls.Controls.Caption();
            this.btnSpaltenkopf = new BlueControls.Controls.Button();
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
            this.tabSpaltenKopf.SuspendLayout();
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
            this.tabAnzeige.Controls.Add(this.RendererEditor);
            this.tabAnzeige.Controls.Add(this.cbxRenderer);
            this.tabAnzeige.Controls.Add(this.capRenderer);
            this.tabAnzeige.Controls.Add(this.btnSpellChecking);
            this.tabAnzeige.Controls.Add(this.cbxTranslate);
            this.tabAnzeige.Controls.Add(this.txbBildCodeConstHeight);
            this.tabAnzeige.Controls.Add(this.capBildCodeConstHeight);
            this.tabAnzeige.Controls.Add(this.capTranslate);
            this.tabAnzeige.Controls.Add(this.cbxBildTextVerhalten);
            this.tabAnzeige.Controls.Add(this.cbxAlign);
            this.tabAnzeige.Controls.Add(this.txbReplacer);
            this.tabAnzeige.Controls.Add(this.capBildCodeImageNotfound);
            this.tabAnzeige.Controls.Add(this.btnStandard);
            this.tabAnzeige.Controls.Add(this.capAlign);
            this.tabAnzeige.Controls.Add(this.capReplacer);
            this.tabAnzeige.Location = new System.Drawing.Point(4, 25);
            this.tabAnzeige.Name = "tabAnzeige";
            this.tabAnzeige.Padding = new System.Windows.Forms.Padding(3);
            this.tabAnzeige.Size = new System.Drawing.Size(993, 483);
            this.tabAnzeige.TabIndex = 0;
            this.tabAnzeige.Text = "Anzeige";
            // 
            // RendererEditor
            // 
            this.RendererEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RendererEditor.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Normal;
            this.RendererEditor.Location = new System.Drawing.Point(528, 152);
            this.RendererEditor.Name = "RendererEditor";
            this.RendererEditor.Size = new System.Drawing.Size(456, 328);
            this.RendererEditor.TabIndex = 45;
            // 
            // cbxRenderer
            // 
            this.cbxRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRenderer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRenderer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRenderer.Location = new System.Drawing.Point(528, 128);
            this.cbxRenderer.Name = "cbxRenderer";
            this.cbxRenderer.Regex = null;
            this.cbxRenderer.Size = new System.Drawing.Size(456, 24);
            this.cbxRenderer.TabIndex = 44;
            this.cbxRenderer.TextChanged += new System.EventHandler(this.cbxRenderer_TextChanged);
            // 
            // capRenderer
            // 
            this.capRenderer.CausesValidation = false;
            this.capRenderer.Location = new System.Drawing.Point(528, 112);
            this.capRenderer.Name = "capRenderer";
            this.capRenderer.Size = new System.Drawing.Size(88, 16);
            this.capRenderer.Text = "Renderer:";
            // 
            // btnSpellChecking
            // 
            this.btnSpellChecking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSpellChecking.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnSpellChecking.Location = new System.Drawing.Point(384, 56);
            this.btnSpellChecking.Name = "btnSpellChecking";
            this.btnSpellChecking.Size = new System.Drawing.Size(352, 16);
            this.btnSpellChecking.TabIndex = 33;
            this.btnSpellChecking.Text = "Rechtschreibprüfung aktivieren";
            // 
            // cbxTranslate
            // 
            this.cbxTranslate.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTranslate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTranslate.Location = new System.Drawing.Point(608, 24);
            this.cbxTranslate.Name = "cbxTranslate";
            this.cbxTranslate.Regex = null;
            this.cbxTranslate.Size = new System.Drawing.Size(360, 24);
            this.cbxTranslate.TabIndex = 37;
            // 
            // txbBildCodeConstHeight
            // 
            this.txbBildCodeConstHeight.AllowedChars = "0123456789|";
            this.txbBildCodeConstHeight.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbBildCodeConstHeight.Location = new System.Drawing.Point(552, 80);
            this.txbBildCodeConstHeight.Name = "txbBildCodeConstHeight";
            this.txbBildCodeConstHeight.QuickInfo = "Beispieleingabe: 24|16";
            this.txbBildCodeConstHeight.Size = new System.Drawing.Size(96, 24);
            this.txbBildCodeConstHeight.Suffix = "Pixel";
            this.txbBildCodeConstHeight.TabIndex = 32;
            // 
            // capBildCodeConstHeight
            // 
            this.capBildCodeConstHeight.CausesValidation = false;
            this.capBildCodeConstHeight.Location = new System.Drawing.Point(384, 80);
            this.capBildCodeConstHeight.Name = "capBildCodeConstHeight";
            this.capBildCodeConstHeight.Size = new System.Drawing.Size(160, 16);
            this.capBildCodeConstHeight.Text = "Breite/Höhe von Bildern:";
            // 
            // capTranslate
            // 
            this.capTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capTranslate.CausesValidation = false;
            this.capTranslate.Location = new System.Drawing.Point(512, 24);
            this.capTranslate.Name = "capTranslate";
            this.capTranslate.Size = new System.Drawing.Size(96, 24);
            this.capTranslate.Text = "Übersetzen:";
            // 
            // cbxBildTextVerhalten
            // 
            this.cbxBildTextVerhalten.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxBildTextVerhalten.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBildTextVerhalten.Location = new System.Drawing.Point(136, 80);
            this.cbxBildTextVerhalten.Name = "cbxBildTextVerhalten";
            this.cbxBildTextVerhalten.Regex = null;
            this.cbxBildTextVerhalten.Size = new System.Drawing.Size(240, 24);
            this.cbxBildTextVerhalten.TabIndex = 34;
            // 
            // cbxAlign
            // 
            this.cbxAlign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAlign.Location = new System.Drawing.Point(368, 24);
            this.cbxAlign.Name = "cbxAlign";
            this.cbxAlign.Regex = null;
            this.cbxAlign.Size = new System.Drawing.Size(128, 24);
            this.cbxAlign.TabIndex = 7;
            // 
            // txbReplacer
            // 
            this.txbReplacer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txbReplacer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbReplacer.Location = new System.Drawing.Point(8, 136);
            this.txbReplacer.MultiLine = true;
            this.txbReplacer.Name = "txbReplacer";
            this.txbReplacer.QuickInfo = "Texte in der Spalte werden mit diesen Angaben <b>optisch</b> ersetzt.<br><i><u>Be" +
    "ispiel:</i></u>Beispiel-Text|Bsp.-Txt";
            this.txbReplacer.Regex = null;
            this.txbReplacer.Size = new System.Drawing.Size(504, 345);
            this.txbReplacer.SpellCheckingEnabled = true;
            this.txbReplacer.TabIndex = 35;
            // 
            // capBildCodeImageNotfound
            // 
            this.capBildCodeImageNotfound.CausesValidation = false;
            this.capBildCodeImageNotfound.Location = new System.Drawing.Point(8, 80);
            this.capBildCodeImageNotfound.Name = "capBildCodeImageNotfound";
            this.capBildCodeImageNotfound.Size = new System.Drawing.Size(128, 16);
            this.capBildCodeImageNotfound.Text = "Bild/Text-Verhalten:";
            // 
            // btnStandard
            // 
            this.btnStandard.Location = new System.Drawing.Point(232, 8);
            this.btnStandard.Name = "btnStandard";
            this.btnStandard.Size = new System.Drawing.Size(128, 48);
            this.btnStandard.TabIndex = 39;
            this.btnStandard.Text = "Standard herstellen";
            this.btnStandard.Click += new System.EventHandler(this.btnStandard_Click);
            // 
            // capAlign
            // 
            this.capAlign.CausesValidation = false;
            this.capAlign.Location = new System.Drawing.Point(368, 8);
            this.capAlign.Name = "capAlign";
            this.capAlign.Size = new System.Drawing.Size(104, 16);
            this.capAlign.Text = "Ausrichtung:";
            // 
            // capReplacer
            // 
            this.capReplacer.CausesValidation = false;
            this.capReplacer.Location = new System.Drawing.Point(8, 112);
            this.capReplacer.Name = "capReplacer";
            this.capReplacer.Size = new System.Drawing.Size(144, 24);
            this.capReplacer.Text = "Optische Ersetzungen:";
            // 
            // txbFixedColumnWidth
            // 
            this.txbFixedColumnWidth.AllowedChars = "0123456789|";
            this.txbFixedColumnWidth.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFixedColumnWidth.Location = new System.Drawing.Point(160, 184);
            this.txbFixedColumnWidth.Name = "txbFixedColumnWidth";
            this.txbFixedColumnWidth.QuickInfo = "Wenn ein Wert >0 eingegeben wird, \r\nwird die Spalte immer in dieser Breite angeze" +
    "igt.";
            this.txbFixedColumnWidth.Size = new System.Drawing.Size(88, 24);
            this.txbFixedColumnWidth.Suffix = "Pixel";
            this.txbFixedColumnWidth.TabIndex = 42;
            // 
            // capFixedColumnWidth
            // 
            this.capFixedColumnWidth.CausesValidation = false;
            this.capFixedColumnWidth.Location = new System.Drawing.Point(32, 184);
            this.capFixedColumnWidth.Name = "capFixedColumnWidth";
            this.capFixedColumnWidth.Size = new System.Drawing.Size(128, 16);
            this.capFixedColumnWidth.Text = "Feste Spaltenbreite:";
            // 
            // txbSpaltenbild
            // 
            this.txbSpaltenbild.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbSpaltenbild.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSpaltenbild.Location = new System.Drawing.Point(336, 168);
            this.txbSpaltenbild.Name = "txbSpaltenbild";
            this.txbSpaltenbild.Regex = null;
            this.txbSpaltenbild.Size = new System.Drawing.Size(456, 24);
            this.txbSpaltenbild.TabIndex = 40;
            // 
            // capUeberschrift3
            // 
            this.capUeberschrift3.CausesValidation = false;
            this.capUeberschrift3.Location = new System.Drawing.Point(288, 70);
            this.capUeberschrift3.Name = "capUeberschrift3";
            this.capUeberschrift3.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift3.Text = "Überschrift 3:";
            this.capUeberschrift3.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift2
            // 
            this.capUeberschrift2.CausesValidation = false;
            this.capUeberschrift2.Location = new System.Drawing.Point(288, 46);
            this.capUeberschrift2.Name = "capUeberschrift2";
            this.capUeberschrift2.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift2.Text = "Überschrift 2:";
            this.capUeberschrift2.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift1
            // 
            this.capUeberschrift1.CausesValidation = false;
            this.capUeberschrift1.Location = new System.Drawing.Point(288, 22);
            this.capUeberschrift1.Name = "capUeberschrift1";
            this.capUeberschrift1.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift1.Text = "Überschrift 1:";
            this.capUeberschrift1.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbUeberschift3
            // 
            this.txbUeberschift3.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift3.Location = new System.Drawing.Point(380, 63);
            this.txbUeberschift3.Name = "txbUeberschift3";
            this.txbUeberschift3.Regex = null;
            this.txbUeberschift3.Size = new System.Drawing.Size(288, 24);
            this.txbUeberschift3.TabIndex = 38;
            // 
            // txbUeberschift2
            // 
            this.txbUeberschift2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift2.Location = new System.Drawing.Point(380, 39);
            this.txbUeberschift2.Name = "txbUeberschift2";
            this.txbUeberschift2.Regex = null;
            this.txbUeberschift2.Size = new System.Drawing.Size(288, 24);
            this.txbUeberschift2.TabIndex = 37;
            // 
            // txbUeberschift1
            // 
            this.txbUeberschift1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift1.Location = new System.Drawing.Point(380, 15);
            this.txbUeberschift1.Name = "txbUeberschift1";
            this.txbUeberschift1.Regex = null;
            this.txbUeberschift1.Size = new System.Drawing.Size(288, 24);
            this.txbUeberschift1.TabIndex = 36;
            // 
            // capSpaltenbild
            // 
            this.capSpaltenbild.CausesValidation = false;
            this.capSpaltenbild.Location = new System.Drawing.Point(336, 152);
            this.capSpaltenbild.Name = "capSpaltenbild";
            this.capSpaltenbild.Size = new System.Drawing.Size(152, 24);
            this.capSpaltenbild.Text = "Spaltenbild:";
            this.capSpaltenbild.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnBackColor
            // 
            this.btnBackColor.Location = new System.Drawing.Point(16, 40);
            this.btnBackColor.Name = "btnBackColor";
            this.btnBackColor.Size = new System.Drawing.Size(128, 24);
            this.btnBackColor.TabIndex = 3;
            this.btnBackColor.Text = "Hintergrundfarbe";
            this.btnBackColor.Click += new System.EventHandler(this.btnBackColor_Click);
            // 
            // cbxRandRechts
            // 
            this.cbxRandRechts.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRandRechts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRandRechts.Location = new System.Drawing.Point(188, 135);
            this.cbxRandRechts.Name = "cbxRandRechts";
            this.cbxRandRechts.Regex = null;
            this.cbxRandRechts.Size = new System.Drawing.Size(128, 24);
            this.cbxRandRechts.TabIndex = 25;
            // 
            // btnTextColor
            // 
            this.btnTextColor.Location = new System.Drawing.Point(12, 15);
            this.btnTextColor.Name = "btnTextColor";
            this.btnTextColor.Size = new System.Drawing.Size(128, 24);
            this.btnTextColor.TabIndex = 4;
            this.btnTextColor.Text = "Textfarbe";
            this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
            // 
            // cbxRandLinks
            // 
            this.cbxRandLinks.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRandLinks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRandLinks.Location = new System.Drawing.Point(52, 135);
            this.cbxRandLinks.Name = "cbxRandLinks";
            this.cbxRandLinks.Regex = null;
            this.cbxRandLinks.Size = new System.Drawing.Size(128, 24);
            this.cbxRandLinks.TabIndex = 24;
            // 
            // capLinkerRand
            // 
            this.capLinkerRand.CausesValidation = false;
            this.capLinkerRand.Location = new System.Drawing.Point(52, 119);
            this.capLinkerRand.Name = "capLinkerRand";
            this.capLinkerRand.Size = new System.Drawing.Size(80, 16);
            this.capLinkerRand.Text = "Linker Rand:";
            this.capLinkerRand.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRechterRand
            // 
            this.capRechterRand.CausesValidation = false;
            this.capRechterRand.Location = new System.Drawing.Point(188, 119);
            this.capRechterRand.Name = "capRechterRand";
            this.capRechterRand.Size = new System.Drawing.Size(88, 16);
            this.capRechterRand.Text = "Rechter Rand:";
            this.capRechterRand.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxScriptType
            // 
            this.cbxScriptType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxScriptType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxScriptType.Location = new System.Drawing.Point(232, 272);
            this.cbxScriptType.Name = "cbxScriptType";
            this.cbxScriptType.Regex = null;
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
            // btnFormatierungErlaubt
            // 
            this.btnFormatierungErlaubt.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnFormatierungErlaubt.Location = new System.Drawing.Point(360, 8);
            this.btnFormatierungErlaubt.Name = "btnFormatierungErlaubt";
            this.btnFormatierungErlaubt.Size = new System.Drawing.Size(296, 16);
            this.btnFormatierungErlaubt.TabIndex = 41;
            this.btnFormatierungErlaubt.Text = "Text-Formatierung erlaubt (Fett, Kursiv, etc.)";
            // 
            // cbxAdditionalCheck
            // 
            this.cbxAdditionalCheck.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAdditionalCheck.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAdditionalCheck.Location = new System.Drawing.Point(232, 232);
            this.cbxAdditionalCheck.Name = "cbxAdditionalCheck";
            this.cbxAdditionalCheck.Regex = null;
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
            // cbxFunction
            // 
            this.cbxFunction.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFunction.Location = new System.Drawing.Point(8, 32);
            this.cbxFunction.Name = "cbxFunction";
            this.cbxFunction.Regex = null;
            this.cbxFunction.Size = new System.Drawing.Size(312, 24);
            this.cbxFunction.TabIndex = 27;
            this.cbxFunction.TextChanged += new System.EventHandler(this.cbxFunction_TextChanged);
            // 
            // capFunction
            // 
            this.capFunction.CausesValidation = false;
            this.capFunction.Location = new System.Drawing.Point(8, 16);
            this.capFunction.Name = "capFunction";
            this.capFunction.Size = new System.Drawing.Size(136, 16);
            this.capFunction.Text = "Funktion:";
            this.capFunction.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnMultiline
            // 
            this.btnMultiline.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnMultiline.Location = new System.Drawing.Point(360, 32);
            this.btnMultiline.Name = "btnMultiline";
            this.btnMultiline.Size = new System.Drawing.Size(296, 16);
            this.btnMultiline.TabIndex = 7;
            this.btnMultiline.Text = "Mehrere Einträge pro Zelle erlaubt (mehrzeilig)";
            // 
            // tabBearbeitung
            // 
            this.tabBearbeitung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBearbeitung.Controls.Add(this.grpAuswahlmenuOptionen);
            this.tabBearbeitung.Controls.Add(this.btnLogUndo);
            this.tabBearbeitung.Controls.Add(this.btnIgnoreLock);
            this.tabBearbeitung.Controls.Add(this.lbxCellEditor);
            this.tabBearbeitung.Controls.Add(this.btnEditableStandard);
            this.tabBearbeitung.Controls.Add(this.capUserGroupEdit);
            this.tabBearbeitung.Controls.Add(this.btnEditableDropdown);
            this.tabBearbeitung.Location = new System.Drawing.Point(4, 25);
            this.tabBearbeitung.Name = "tabBearbeitung";
            this.tabBearbeitung.Padding = new System.Windows.Forms.Padding(3);
            this.tabBearbeitung.Size = new System.Drawing.Size(993, 483);
            this.tabBearbeitung.TabIndex = 1;
            this.tabBearbeitung.Text = "Bearbeitung";
            // 
            // grpAuswahlmenuOptionen
            // 
            this.grpAuswahlmenuOptionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuswahlmenuOptionen.CausesValidation = false;
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnOtherValuesToo);
            this.grpAuswahlmenuOptionen.Controls.Add(this.txbAuswaehlbareWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.capImmerWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnCanBeEmpty);
            this.grpAuswahlmenuOptionen.Location = new System.Drawing.Point(32, 80);
            this.grpAuswahlmenuOptionen.Name = "grpAuswahlmenuOptionen";
            this.grpAuswahlmenuOptionen.Size = new System.Drawing.Size(536, 392);
            this.grpAuswahlmenuOptionen.TabIndex = 0;
            this.grpAuswahlmenuOptionen.TabStop = false;
            this.grpAuswahlmenuOptionen.Text = "Auswahlmenü-Optionen:";
            // 
            // btnOtherValuesToo
            // 
            this.btnOtherValuesToo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOtherValuesToo.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnOtherValuesToo.Location = new System.Drawing.Point(8, 352);
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
            this.txbAuswaehlbareWerte.Regex = null;
            this.txbAuswaehlbareWerte.Size = new System.Drawing.Size(520, 280);
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
            // btnLogUndo
            // 
            this.btnLogUndo.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.btnLogUndo.Location = new System.Drawing.Point(592, 376);
            this.btnLogUndo.Name = "btnLogUndo";
            this.btnLogUndo.Size = new System.Drawing.Size(288, 16);
            this.btnLogUndo.TabIndex = 32;
            this.btnLogUndo.Text = "Undo der Spalte wird geloggt";
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
            this.tabAutoKorrektur.Size = new System.Drawing.Size(993, 483);
            this.tabAutoKorrektur.TabIndex = 6;
            this.tabAutoKorrektur.Text = "Auto-Korrektur";
            // 
            // btnCalculateMaxCellLenght
            // 
            this.btnCalculateMaxCellLenght.ImageCode = "Taschenrechner|16";
            this.btnCalculateMaxCellLenght.Location = new System.Drawing.Point(272, 88);
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
            this.txbAutoReplace.Location = new System.Drawing.Point(16, 184);
            this.txbAutoReplace.MultiLine = true;
            this.txbAutoReplace.Name = "txbAutoReplace";
            this.txbAutoReplace.QuickInfo = resources.GetString("txbAutoReplace.QuickInfo");
            this.txbAutoReplace.Regex = null;
            this.txbAutoReplace.Size = new System.Drawing.Size(968, 288);
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
            this.txbMaxCellLenght.Location = new System.Drawing.Point(176, 88);
            this.txbMaxCellLenght.MaxTextLenght = 255;
            this.txbMaxCellLenght.Name = "txbMaxCellLenght";
            this.txbMaxCellLenght.QuickInfo = resources.GetString("txbMaxCellLenght.QuickInfo");
            this.txbMaxCellLenght.Regex = "^((-?[1-9]\\d*)|0)$";
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
            this.txbAutoRemove.Regex = null;
            this.txbAutoRemove.Size = new System.Drawing.Size(968, 24);
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
            this.btnAutoEditToUpper.Size = new System.Drawing.Size(416, 24);
            this.btnAutoEditToUpper.TabIndex = 12;
            this.btnAutoEditToUpper.Text = "Texte in Grossbuchstaben ändern";
            // 
            // txbRunden
            // 
            this.txbRunden.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRunden.Location = new System.Drawing.Point(232, 48);
            this.txbRunden.Name = "txbRunden";
            this.txbRunden.Regex = null;
            this.txbRunden.Size = new System.Drawing.Size(88, 24);
            this.txbRunden.TabIndex = 11;
            // 
            // capNachkommastellen
            // 
            this.capNachkommastellen.CausesValidation = false;
            this.capNachkommastellen.Location = new System.Drawing.Point(16, 48);
            this.capNachkommastellen.Name = "capNachkommastellen";
            this.capNachkommastellen.Size = new System.Drawing.Size(328, 16);
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
            this.tabFilter.Size = new System.Drawing.Size(993, 483);
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
            this.txbJoker.Regex = null;
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
            this.tabQuickInfo.Size = new System.Drawing.Size(993, 483);
            this.tabQuickInfo.TabIndex = 3;
            this.tabQuickInfo.Text = "Quickinfo";
            // 
            // txbAdminInfo
            // 
            this.txbAdminInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAdminInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAdminInfo.Location = new System.Drawing.Point(512, 24);
            this.txbAdminInfo.MultiLine = true;
            this.txbAdminInfo.Name = "txbAdminInfo";
            this.txbAdminInfo.Regex = null;
            this.txbAdminInfo.Size = new System.Drawing.Size(473, 424);
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
            this.txbQuickinfo.Regex = null;
            this.txbQuickinfo.Size = new System.Drawing.Size(496, 424);
            this.txbQuickinfo.SpellCheckingEnabled = true;
            this.txbQuickinfo.TabIndex = 0;
            this.txbQuickinfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption18
            // 
            this.Caption18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Caption18.CausesValidation = false;
            this.Caption18.Location = new System.Drawing.Point(512, 8);
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
            this.btnQI_Vorschau.Location = new System.Drawing.Point(889, 456);
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
            this.tabSonstiges.Size = new System.Drawing.Size(993, 483);
            this.tabSonstiges.TabIndex = 4;
            this.tabSonstiges.Text = "Sonstiges allgemein";
            // 
            // txbTags
            // 
            this.txbTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbTags.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTags.Location = new System.Drawing.Point(4, 31);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.Regex = null;
            this.txbTags.Size = new System.Drawing.Size(980, 441);
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
            this.cbxSort.Regex = null;
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
            this.txbRegex.Size = new System.Drawing.Size(976, 48);
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
            this.txbAllowedChars.Regex = null;
            this.txbAllowedChars.Size = new System.Drawing.Size(976, 56);
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
            this.cbxLinkedDatabase.Location = new System.Drawing.Point(224, 16);
            this.cbxLinkedDatabase.Name = "cbxLinkedDatabase";
            this.cbxLinkedDatabase.Regex = null;
            this.cbxLinkedDatabase.Size = new System.Drawing.Size(752, 24);
            this.cbxLinkedDatabase.TabIndex = 38;
            this.cbxLinkedDatabase.TextChanged += new System.EventHandler(this.cbxLinkedDatabase_TextChanged);
            // 
            // capLinkedDatabase
            // 
            this.capLinkedDatabase.CausesValidation = false;
            this.capLinkedDatabase.Location = new System.Drawing.Point(8, 16);
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
            this.BlueFrame1.Controls.Add(this.btnVerwendung);
            this.BlueFrame1.Controls.Add(this.capInfo);
            this.BlueFrame1.Controls.Add(this.Caption3);
            this.BlueFrame1.Controls.Add(this.txbName);
            this.BlueFrame1.Controls.Add(this.Caption2);
            this.BlueFrame1.Controls.Add(this.txbCaption);
            this.BlueFrame1.Location = new System.Drawing.Point(8, 24);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(997, 104);
            this.BlueFrame1.TabIndex = 16;
            this.BlueFrame1.TabStop = false;
            this.BlueFrame1.Text = "Allgemein";
            // 
            // btnVerwendung
            // 
            this.btnVerwendung.Location = new System.Drawing.Point(8, 80);
            this.btnVerwendung.Name = "btnVerwendung";
            this.btnVerwendung.Size = new System.Drawing.Size(144, 24);
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
            this.txbName.Regex = null;
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
            this.txbCaption.Regex = null;
            this.txbCaption.Size = new System.Drawing.Size(681, 64);
            this.txbCaption.TabIndex = 2;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.ImageCode = "Häkchen|16";
            this.btnOk.Location = new System.Drawing.Point(921, 654);
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
            this.tabControl.Controls.Add(this.tabSpaltenKopf);
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
            this.tabControl.Size = new System.Drawing.Size(1001, 512);
            this.tabControl.TabDefault = this.tabDatenFormat;
            this.tabControl.TabDefaultOrder = null;
            this.tabControl.TabIndex = 15;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabDatenFormat
            // 
            this.tabDatenFormat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
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
            this.tabDatenFormat.Controls.Add(this.btnFormatierungErlaubt);
            this.tabDatenFormat.Controls.Add(this.cbxAdditionalCheck);
            this.tabDatenFormat.Controls.Add(this.capcbxAdditionalCheck);
            this.tabDatenFormat.Controls.Add(this.cbxScriptType);
            this.tabDatenFormat.Controls.Add(this.capScriptType);
            this.tabDatenFormat.Controls.Add(this.cbxFunction);
            this.tabDatenFormat.Controls.Add(this.capFunction);
            this.tabDatenFormat.Controls.Add(this.btnMultiline);
            this.tabDatenFormat.Location = new System.Drawing.Point(4, 25);
            this.tabDatenFormat.Name = "tabDatenFormat";
            this.tabDatenFormat.Size = new System.Drawing.Size(993, 483);
            this.tabDatenFormat.TabIndex = 12;
            this.tabDatenFormat.Text = "Daten-Format";
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
            this.txbMaxTextLenght.Regex = "^((-?[1-9]\\d*)|0)$";
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
            this.grpSchnellformat.Size = new System.Drawing.Size(976, 80);
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
            // tabSpaltenKopf
            // 
            this.tabSpaltenKopf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSpaltenKopf.Controls.Add(this.btnTextColor);
            this.tabSpaltenKopf.Controls.Add(this.btnBackColor);
            this.tabSpaltenKopf.Controls.Add(this.txbFixedColumnWidth);
            this.tabSpaltenKopf.Controls.Add(this.capFixedColumnWidth);
            this.tabSpaltenKopf.Controls.Add(this.txbUeberschift1);
            this.tabSpaltenKopf.Controls.Add(this.txbUeberschift3);
            this.tabSpaltenKopf.Controls.Add(this.txbSpaltenbild);
            this.tabSpaltenKopf.Controls.Add(this.capSpaltenbild);
            this.tabSpaltenKopf.Controls.Add(this.txbUeberschift2);
            this.tabSpaltenKopf.Controls.Add(this.capUeberschrift3);
            this.tabSpaltenKopf.Controls.Add(this.cbxRandRechts);
            this.tabSpaltenKopf.Controls.Add(this.capUeberschrift1);
            this.tabSpaltenKopf.Controls.Add(this.capRechterRand);
            this.tabSpaltenKopf.Controls.Add(this.capLinkerRand);
            this.tabSpaltenKopf.Controls.Add(this.capUeberschrift2);
            this.tabSpaltenKopf.Controls.Add(this.cbxRandLinks);
            this.tabSpaltenKopf.Location = new System.Drawing.Point(4, 25);
            this.tabSpaltenKopf.Name = "tabSpaltenKopf";
            this.tabSpaltenKopf.Padding = new System.Windows.Forms.Padding(3);
            this.tabSpaltenKopf.Size = new System.Drawing.Size(993, 483);
            this.tabSpaltenKopf.TabIndex = 13;
            this.tabSpaltenKopf.Text = "Spaltenkopf";
            // 
            // tabSpaltenVerlinkung
            // 
            this.tabSpaltenVerlinkung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSpaltenVerlinkung.Controls.Add(this.tblFilterliste);
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxTargetColumn);
            this.tabSpaltenVerlinkung.Controls.Add(this.capLinkedDatabase);
            this.tabSpaltenVerlinkung.Controls.Add(this.capTargetColumn);
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxLinkedDatabase);
            this.tabSpaltenVerlinkung.Location = new System.Drawing.Point(4, 25);
            this.tabSpaltenVerlinkung.Name = "tabSpaltenVerlinkung";
            this.tabSpaltenVerlinkung.Size = new System.Drawing.Size(993, 483);
            this.tabSpaltenVerlinkung.TabIndex = 11;
            this.tabSpaltenVerlinkung.Text = "Spalten-Verlinkung";
            // 
            // tblFilterliste
            // 
            this.tblFilterliste.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblFilterliste.Arrangement = "";
            this.tblFilterliste.Item = null;
            this.tblFilterliste.Location = new System.Drawing.Point(8, 80);
            this.tblFilterliste.Mode = "";
            this.tblFilterliste.Name = "tblFilterliste";
            this.tblFilterliste.Size = new System.Drawing.Size(968, 396);
            this.tblFilterliste.TabIndex = 39;
            // 
            // cbxTargetColumn
            // 
            this.cbxTargetColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxTargetColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTargetColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTargetColumn.Location = new System.Drawing.Point(224, 48);
            this.cbxTargetColumn.Name = "cbxTargetColumn";
            this.cbxTargetColumn.Regex = null;
            this.cbxTargetColumn.Size = new System.Drawing.Size(752, 24);
            this.cbxTargetColumn.TabIndex = 5;
            this.cbxTargetColumn.TextChanged += new System.EventHandler(this.cbxTargetColumn_TextChanged);
            // 
            // capTargetColumn
            // 
            this.capTargetColumn.CausesValidation = false;
            this.capTargetColumn.Location = new System.Drawing.Point(8, 48);
            this.capTargetColumn.Name = "capTargetColumn";
            this.capTargetColumn.Size = new System.Drawing.Size(200, 16);
            this.capTargetColumn.Text = "Aus dieser Spalte die Werte holen:";
            this.capTargetColumn.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // caption5
            // 
            this.caption5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.caption5.CausesValidation = false;
            this.caption5.Location = new System.Drawing.Point(80, 654);
            this.caption5.Name = "caption5";
            this.caption5.Size = new System.Drawing.Size(104, 24);
            this.caption5.Text = "Aktuelle Ansicht:";
            // 
            // butAktuellVor
            // 
            this.butAktuellVor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAktuellVor.ImageCode = "Pfeil_Rechts|16|||0000FF";
            this.butAktuellVor.Location = new System.Drawing.Point(272, 654);
            this.butAktuellVor.Name = "butAktuellVor";
            this.butAktuellVor.Size = new System.Drawing.Size(72, 24);
            this.butAktuellVor.TabIndex = 19;
            this.butAktuellVor.Click += new System.EventHandler(this.butAktuellVor_Click);
            // 
            // butAktuellZurueck
            // 
            this.butAktuellZurueck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAktuellZurueck.ImageCode = "Pfeil_Links|16|||0000FF";
            this.butAktuellZurueck.Location = new System.Drawing.Point(192, 654);
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
            // ColumnEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1007, 685);
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
            this.tabSpaltenKopf.ResumeLayout(false);
            this.tabSpaltenVerlinkung.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnOk;
        private TextBox txbName;
        private Caption Caption3;
        private System.Windows.Forms.ColorDialog ColorDia;
        private Button btnQI_Vorschau;
        private TextBox txbQuickinfo;
        private Button btnZeilenFilterIgnorieren;
        private Button btnAutoFilterErweitertErlaubt;
        private Button btnAutoFilterTXTErlaubt;
        private Button btnAutoFilterMoeglich;
        private Caption capInfo;
        private Button btnMultiline;
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
        private Caption capFunction;
        private Caption Caption18;
        private TextBox txbAdminInfo;
        private Caption Caption17;
        private Button btnOtherValuesToo;
        private ListBox lbxCellEditor;
        private ComboBox cbxFunction;
        private TextBox txbTags;
        private TabControl tabControl;
        private System.Windows.Forms.TabPage tabAnzeige;
        private System.Windows.Forms.TabPage tabBearbeitung;
        private System.Windows.Forms.TabPage tabAutoKorrektur;
        private System.Windows.Forms.TabPage tabFilter;
        private System.Windows.Forms.TabPage tabQuickInfo;
        private System.Windows.Forms.TabPage tabSonstiges;
        private Button btnIgnoreLock;
        private Button btnLogUndo;
        private Button btnSpellChecking;
        private Caption capSpaltenbild;
        private TextBox txbJoker;
        private Caption capJokerValue;
        private Button btnAutoEditKleineFehler;
        private Button btnAutoEditToUpper;
        private TextBox txbRunden;
        private Caption capNachkommastellen;
        private Button btnAutoEditAutoSort;
        private ComboBox cbxBildTextVerhalten;
        private Caption capBildCodeImageNotfound;
        private Caption capBildCodeConstHeight;
        private TextBox txbBildCodeConstHeight;
        private ComboBox cbxLinkedDatabase;
        private Caption capLinkedDatabase;
        private ComboBox cbxTargetColumn;
        private Caption capTargetColumn;
        private TextBox txbReplacer;
        private Caption capReplacer;
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
        private Button btnFormatierungErlaubt;
        private ComboBox cbxAdditionalCheck;
        private Caption capcbxAdditionalCheck;
        private Button btnSchnellBildCode;
        private ComboBox cbxScriptType;
        private Caption capScriptType;
        private Button btnSchnellBit;
        private System.Windows.Forms.TabPage tabDatenFormat;
        private GroupBox grpSchnellformat;
        private System.Windows.Forms.TabPage tabSpaltenVerlinkung;
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
        private System.Windows.Forms.TabPage tabSpaltenKopf;
        private GroupBox RendererEditor;
    }
}
