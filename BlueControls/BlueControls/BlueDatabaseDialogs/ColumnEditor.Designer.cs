using System.Diagnostics;
using BlueControls.Controls;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs {
    internal sealed partial class ColumnEditor : Form {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
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
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColumnEditor));
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.tabDesign = new System.Windows.Forms.TabPage();
            this.btnFormatierungErlaubt = new BlueControls.Controls.Button();
            this.txbSpaltenbild = new BlueControls.Controls.TextBox();
            this.cbxAdditionalCheck = new BlueControls.Controls.ComboBox();
            this.cbxBildTextVerhalten = new BlueControls.Controls.ComboBox();
            this.cbxAlign = new BlueControls.Controls.ComboBox();
            this.capcbxAdditionalCheck = new BlueControls.Controls.Caption();
            this.txbReplacer = new BlueControls.Controls.TextBox();
            this.capBildCodeImageNotfound = new BlueControls.Controls.Caption();
            this.btnStandard = new BlueControls.Controls.Button();
            this.capAlign = new BlueControls.Controls.Caption();
            this.txbPrefix = new BlueControls.Controls.TextBox();
            this.capPraefix = new BlueControls.Controls.Caption();
            this.capUeberschrift3 = new BlueControls.Controls.Caption();
            this.capUeberschrift2 = new BlueControls.Controls.Caption();
            this.capUeberschrift1 = new BlueControls.Controls.Caption();
            this.txbUeberschift3 = new BlueControls.Controls.TextBox();
            this.txbUeberschift2 = new BlueControls.Controls.TextBox();
            this.txbUeberschift1 = new BlueControls.Controls.TextBox();
            this.capReplacer = new BlueControls.Controls.Caption();
            this.btnEinzeiligDarstellen = new BlueControls.Controls.Button();
            this.capEinheit = new BlueControls.Controls.Caption();
            this.cbxEinheit = new BlueControls.Controls.ComboBox();
            this.capSpaltenbild = new BlueControls.Controls.Caption();
            this.cbxFormat = new BlueControls.Controls.ComboBox();
            this.capFormat = new BlueControls.Controls.Caption();
            this.btnBackColor = new BlueControls.Controls.Button();
            this.cbxRandRechts = new BlueControls.Controls.ComboBox();
            this.btnTextColor = new BlueControls.Controls.Button();
            this.cbxRandLinks = new BlueControls.Controls.ComboBox();
            this.capLinkerRand = new BlueControls.Controls.Caption();
            this.capRechterRand = new BlueControls.Controls.Caption();
            this.btnMultiline = new BlueControls.Controls.Button();
            this.grpBildCode = new BlueControls.Controls.GroupBox();
            this.capBildCodeConstHeight = new BlueControls.Controls.Caption();
            this.txbBildCodeConstHeight = new BlueControls.Controls.TextBox();
            this.tabRechte = new System.Windows.Forms.TabPage();
            this.grpAuswahlmenuOptionen = new BlueControls.Controls.GroupBox();
            this.btnOtherValuesToo = new BlueControls.Controls.Button();
            this.tbxAuswaehlbareWerte = new BlueControls.Controls.TextBox();
            this.capImmerWerte = new BlueControls.Controls.Caption();
            this.btnCanBeEmpty = new BlueControls.Controls.Button();
            this.cbxDropDownKey = new BlueControls.Controls.ComboBox();
            this.capDropDownKey = new BlueControls.Controls.Caption();
            this.btnIgnoreLock = new BlueControls.Controls.Button();
            this.lbxCellEditor = new BlueControls.Controls.ListBox();
            this.btnEditableStandard = new BlueControls.Controls.Button();
            this.capUserGroupEdit = new BlueControls.Controls.Caption();
            this.btnEditableDropdown = new BlueControls.Controls.Button();
            this.tabAutoBearbeitung = new System.Windows.Forms.TabPage();
            this.txbAutoReplace = new BlueControls.Controls.TextBox();
            this.capAutoReplace = new BlueControls.Controls.Caption();
            this.txbAutoRemove = new BlueControls.Controls.TextBox();
            this.capAutoRemove = new BlueControls.Controls.Caption();
            this.cbxSchlüsselspalte = new BlueControls.Controls.ComboBox();
            this.tbxInitValue = new BlueControls.Controls.TextBox();
            this.Caption12 = new BlueControls.Controls.Caption();
            this.capSchlüsselspalte = new BlueControls.Controls.Caption();
            this.btnAutoEditKleineFehler = new BlueControls.Controls.Button();
            this.btnAutoEditToUpper = new BlueControls.Controls.Button();
            this.tbxRunden = new BlueControls.Controls.TextBox();
            this.cbxVorschlagSpalte = new BlueControls.Controls.ComboBox();
            this.capVorschlag = new BlueControls.Controls.Caption();
            this.capNachkommastellen = new BlueControls.Controls.Caption();
            this.btnAutoEditAutoSort = new BlueControls.Controls.Button();
            this.tabFilter = new System.Windows.Forms.TabPage();
            this.chkFilterOnlyOr = new BlueControls.Controls.Button();
            this.chkFilterOnlyAND = new BlueControls.Controls.Button();
            this.capDauerfilterPos = new BlueControls.Controls.Caption();
            this.txbDauerFilterPos = new BlueControls.Controls.TextBox();
            this.capJokerValue = new BlueControls.Controls.Caption();
            this.tbxJoker = new BlueControls.Controls.TextBox();
            this.btnZeilenFilterIgnorieren = new BlueControls.Controls.Button();
            this.btnAutoFilterMoeglich = new BlueControls.Controls.Button();
            this.btnAutoFilterTXTErlaubt = new BlueControls.Controls.Button();
            this.btnAutoFilterErweitertErlaubt = new BlueControls.Controls.Button();
            this.tabQuickInfo = new System.Windows.Forms.TabPage();
            this.tbxAdminInfo = new BlueControls.Controls.TextBox();
            this.tbxQuickinfo = new BlueControls.Controls.TextBox();
            this.Caption18 = new BlueControls.Controls.Caption();
            this.Caption17 = new BlueControls.Controls.Caption();
            this.btnQI_Vorschau = new BlueControls.Controls.Button();
            this.tabSonstiges = new System.Windows.Forms.TabPage();
            this.cbxTranslate = new BlueControls.Controls.ComboBox();
            this.capTranslate = new BlueControls.Controls.Caption();
            this.cbxSort = new BlueControls.Controls.ComboBox();
            this.butSaveContent = new BlueControls.Controls.Button();
            this.btnSpellChecking = new BlueControls.Controls.Button();
            this.txbRegex = new BlueControls.Controls.TextBox();
            this.capSortiermaske = new BlueControls.Controls.Caption();
            this.btnLogUndo = new BlueControls.Controls.Button();
            this.capRegex = new BlueControls.Controls.Caption();
            this.tbxAllowedChars = new BlueControls.Controls.TextBox();
            this.Caption13 = new BlueControls.Controls.Caption();
            this.tbxTags = new BlueControls.Controls.TextBox();
            this.Caption8 = new BlueControls.Controls.Caption();
            this.txbLinkedKeyKennung = new BlueControls.Controls.TextBox();
            this.capLinkedKeyKennung = new BlueControls.Controls.Caption();
            this.cbxLinkedDatabase = new BlueControls.Controls.ComboBox();
            this.capLinkedDatabase = new BlueControls.Controls.Caption();
            this.capBestFileStandardSuffix = new BlueControls.Controls.Caption();
            this.txbBestFileStandardSuffix = new BlueControls.Controls.TextBox();
            this.capBestFileStandardFolder = new BlueControls.Controls.Caption();
            this.txbBestFileStandardFolder = new BlueControls.Controls.TextBox();
            this.BlueFrame1 = new BlueControls.Controls.GroupBox();
            this.btnVerwendung = new BlueControls.Controls.Button();
            this.capInfo = new BlueControls.Controls.Caption();
            this.Caption3 = new BlueControls.Controls.Caption();
            this.tbxName = new BlueControls.Controls.TextBox();
            this.Caption2 = new BlueControls.Controls.Caption();
            this.tbxCaption = new BlueControls.Controls.TextBox();
            this.btnVor = new BlueControls.Controls.Button();
            this.btnZurueck = new BlueControls.Controls.Button();
            this.btnOk = new BlueControls.Controls.Button();
            this.tabControl = new BlueControls.Controls.TabControl();
            this.tabSchnellformat = new System.Windows.Forms.TabPage();
            this.btnSchnellBildCode = new BlueControls.Controls.Button();
            this.btnSchnellIInternetAdresse = new BlueControls.Controls.Button();
            this.btnSchnellAuswahloptionen = new BlueControls.Controls.Button();
            this.btnSchnellGleitkommazahl = new BlueControls.Controls.Button();
            this.btnSchnellGanzzahl = new BlueControls.Controls.Button();
            this.btnSchnellTelefonNummer = new BlueControls.Controls.Button();
            this.btnSchnellEmail = new BlueControls.Controls.Button();
            this.btnSchnellDatumUhrzeit = new BlueControls.Controls.Button();
            this.btnSchnellDatum = new BlueControls.Controls.Button();
            this.btnSchnellText = new BlueControls.Controls.Button();
            this.tabSpezial = new System.Windows.Forms.TabPage();
            this.grpLinkToFileSystem = new BlueControls.Controls.GroupBox();
            this.grpLinkedDatabase = new BlueControls.Controls.GroupBox();
            this.grpColumnsForLinkedDatabase = new BlueControls.Controls.GroupBox();
            this.grpVerlinkteZellen = new BlueControls.Controls.GroupBox();
            this.cbxTargetColumn = new BlueControls.Controls.ComboBox();
            this.cbxRowKeyInColumn = new BlueControls.Controls.ComboBox();
            this.capTargetColumn = new BlueControls.Controls.Caption();
            this.capRowKeyInColumn = new BlueControls.Controls.Caption();
            this.capIntern = new BlueControls.Controls.Caption();
            this.caption5 = new BlueControls.Controls.Caption();
            this.butAktuellVor = new BlueControls.Controls.Button();
            this.butAktuellZurueck = new BlueControls.Controls.Button();
            this.cbxScriptType = new BlueControls.Controls.ComboBox();
            this.capScriptType = new BlueControls.Controls.Caption();
            this.tabDesign.SuspendLayout();
            this.grpBildCode.SuspendLayout();
            this.tabRechte.SuspendLayout();
            this.grpAuswahlmenuOptionen.SuspendLayout();
            this.tabAutoBearbeitung.SuspendLayout();
            this.tabFilter.SuspendLayout();
            this.tabQuickInfo.SuspendLayout();
            this.tabSonstiges.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabSchnellformat.SuspendLayout();
            this.tabSpezial.SuspendLayout();
            this.grpLinkToFileSystem.SuspendLayout();
            this.grpLinkedDatabase.SuspendLayout();
            this.grpColumnsForLinkedDatabase.SuspendLayout();
            this.grpVerlinkteZellen.SuspendLayout();
            this.SuspendLayout();
            // 
            // ColorDia
            // 
            this.ColorDia.AnyColor = true;
            this.ColorDia.FullOpen = true;
            // 
            // tabDesign
            // 
            this.tabDesign.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabDesign.Controls.Add(this.cbxScriptType);
            this.tabDesign.Controls.Add(this.capScriptType);
            this.tabDesign.Controls.Add(this.btnFormatierungErlaubt);
            this.tabDesign.Controls.Add(this.txbSpaltenbild);
            this.tabDesign.Controls.Add(this.cbxAdditionalCheck);
            this.tabDesign.Controls.Add(this.cbxBildTextVerhalten);
            this.tabDesign.Controls.Add(this.cbxAlign);
            this.tabDesign.Controls.Add(this.capcbxAdditionalCheck);
            this.tabDesign.Controls.Add(this.txbReplacer);
            this.tabDesign.Controls.Add(this.capBildCodeImageNotfound);
            this.tabDesign.Controls.Add(this.btnStandard);
            this.tabDesign.Controls.Add(this.capAlign);
            this.tabDesign.Controls.Add(this.txbPrefix);
            this.tabDesign.Controls.Add(this.capPraefix);
            this.tabDesign.Controls.Add(this.capUeberschrift3);
            this.tabDesign.Controls.Add(this.capUeberschrift2);
            this.tabDesign.Controls.Add(this.capUeberschrift1);
            this.tabDesign.Controls.Add(this.txbUeberschift3);
            this.tabDesign.Controls.Add(this.txbUeberschift2);
            this.tabDesign.Controls.Add(this.txbUeberschift1);
            this.tabDesign.Controls.Add(this.capReplacer);
            this.tabDesign.Controls.Add(this.btnEinzeiligDarstellen);
            this.tabDesign.Controls.Add(this.capEinheit);
            this.tabDesign.Controls.Add(this.cbxEinheit);
            this.tabDesign.Controls.Add(this.capSpaltenbild);
            this.tabDesign.Controls.Add(this.cbxFormat);
            this.tabDesign.Controls.Add(this.capFormat);
            this.tabDesign.Controls.Add(this.btnBackColor);
            this.tabDesign.Controls.Add(this.cbxRandRechts);
            this.tabDesign.Controls.Add(this.btnTextColor);
            this.tabDesign.Controls.Add(this.cbxRandLinks);
            this.tabDesign.Controls.Add(this.capLinkerRand);
            this.tabDesign.Controls.Add(this.capRechterRand);
            this.tabDesign.Controls.Add(this.btnMultiline);
            this.tabDesign.Location = new System.Drawing.Point(4, 25);
            this.tabDesign.Name = "tabDesign";
            this.tabDesign.Padding = new System.Windows.Forms.Padding(3);
            this.tabDesign.Size = new System.Drawing.Size(912, 487);
            this.tabDesign.TabIndex = 0;
            this.tabDesign.Text = "Design";
            // 
            // btnFormatierungErlaubt
            // 
            this.btnFormatierungErlaubt.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnFormatierungErlaubt.Location = new System.Drawing.Point(8, 208);
            this.btnFormatierungErlaubt.Name = "btnFormatierungErlaubt";
            this.btnFormatierungErlaubt.Size = new System.Drawing.Size(296, 16);
            this.btnFormatierungErlaubt.TabIndex = 41;
            this.btnFormatierungErlaubt.Text = "Formatierung erlaubt";
            // 
            // txbSpaltenbild
            // 
            this.txbSpaltenbild.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbSpaltenbild.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSpaltenbild.Location = new System.Drawing.Point(528, 104);
            this.txbSpaltenbild.Name = "txbSpaltenbild";
            this.txbSpaltenbild.Regex = null;
            this.txbSpaltenbild.Size = new System.Drawing.Size(384, 24);
            this.txbSpaltenbild.TabIndex = 40;
            // 
            // cbxAdditionalCheck
            // 
            this.cbxAdditionalCheck.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAdditionalCheck.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAdditionalCheck.Location = new System.Drawing.Point(144, 232);
            this.cbxAdditionalCheck.Name = "cbxAdditionalCheck";
            this.cbxAdditionalCheck.Regex = null;
            this.cbxAdditionalCheck.Size = new System.Drawing.Size(368, 24);
            this.cbxAdditionalCheck.TabIndex = 34;
            // 
            // cbxBildTextVerhalten
            // 
            this.cbxBildTextVerhalten.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxBildTextVerhalten.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBildTextVerhalten.Location = new System.Drawing.Point(144, 176);
            this.cbxBildTextVerhalten.Name = "cbxBildTextVerhalten";
            this.cbxBildTextVerhalten.Regex = null;
            this.cbxBildTextVerhalten.Size = new System.Drawing.Size(368, 24);
            this.cbxBildTextVerhalten.TabIndex = 34;
            // 
            // cbxAlign
            // 
            this.cbxAlign.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAlign.Location = new System.Drawing.Point(384, 136);
            this.cbxAlign.Name = "cbxAlign";
            this.cbxAlign.Regex = null;
            this.cbxAlign.Size = new System.Drawing.Size(128, 24);
            this.cbxAlign.TabIndex = 7;
            // 
            // capcbxAdditionalCheck
            // 
            this.capcbxAdditionalCheck.CausesValidation = false;
            this.capcbxAdditionalCheck.Location = new System.Drawing.Point(8, 232);
            this.capcbxAdditionalCheck.Name = "capcbxAdditionalCheck";
            this.capcbxAdditionalCheck.Size = new System.Drawing.Size(136, 16);
            this.capcbxAdditionalCheck.Text = "Zusätzlicher Check:";
            // 
            // txbReplacer
            // 
            this.txbReplacer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbReplacer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbReplacer.Location = new System.Drawing.Point(8, 288);
            this.txbReplacer.MultiLine = true;
            this.txbReplacer.Name = "txbReplacer";
            this.txbReplacer.QuickInfo = "Texte in der Spalte werden mit diesen Angaben <b>optisch</b> ersetzt.<br><i><u>Be" +
    "ispiel:</i></u>Beispiel-Text|Bsp.-Txt";
            this.txbReplacer.Regex = null;
            this.txbReplacer.Size = new System.Drawing.Size(904, 197);
            this.txbReplacer.SpellChecking = true;
            this.txbReplacer.TabIndex = 35;
            // 
            // capBildCodeImageNotfound
            // 
            this.capBildCodeImageNotfound.CausesValidation = false;
            this.capBildCodeImageNotfound.Location = new System.Drawing.Point(8, 176);
            this.capBildCodeImageNotfound.Name = "capBildCodeImageNotfound";
            this.capBildCodeImageNotfound.Size = new System.Drawing.Size(136, 16);
            this.capBildCodeImageNotfound.Text = "Bild/Text-Verhalten:";
            // 
            // btnStandard
            // 
            this.btnStandard.Location = new System.Drawing.Point(384, 16);
            this.btnStandard.Name = "btnStandard";
            this.btnStandard.Size = new System.Drawing.Size(128, 48);
            this.btnStandard.TabIndex = 39;
            this.btnStandard.Text = "Standard herstellen";
            this.btnStandard.Click += new System.EventHandler(this.btnStandard_Click);
            // 
            // capAlign
            // 
            this.capAlign.CausesValidation = false;
            this.capAlign.Location = new System.Drawing.Point(384, 120);
            this.capAlign.Name = "capAlign";
            this.capAlign.Size = new System.Drawing.Size(104, 16);
            this.capAlign.Text = "Ausrichtung:";
            // 
            // txbPrefix
            // 
            this.txbPrefix.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbPrefix.Location = new System.Drawing.Point(56, 96);
            this.txbPrefix.Name = "txbPrefix";
            this.txbPrefix.Regex = null;
            this.txbPrefix.Size = new System.Drawing.Size(168, 24);
            this.txbPrefix.TabIndex = 36;
            // 
            // capPraefix
            // 
            this.capPraefix.CausesValidation = false;
            this.capPraefix.Location = new System.Drawing.Point(8, 96);
            this.capPraefix.Name = "capPraefix";
            this.capPraefix.Size = new System.Drawing.Size(48, 16);
            this.capPraefix.Text = "Präfix:";
            // 
            // capUeberschrift3
            // 
            this.capUeberschrift3.CausesValidation = false;
            this.capUeberschrift3.Location = new System.Drawing.Point(528, 56);
            this.capUeberschrift3.Name = "capUeberschrift3";
            this.capUeberschrift3.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift3.Text = "Überschrift 3:";
            this.capUeberschrift3.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift2
            // 
            this.capUeberschrift2.CausesValidation = false;
            this.capUeberschrift2.Location = new System.Drawing.Point(528, 32);
            this.capUeberschrift2.Name = "capUeberschrift2";
            this.capUeberschrift2.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift2.Text = "Überschrift 2:";
            this.capUeberschrift2.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift1
            // 
            this.capUeberschrift1.CausesValidation = false;
            this.capUeberschrift1.Location = new System.Drawing.Point(528, 8);
            this.capUeberschrift1.Name = "capUeberschrift1";
            this.capUeberschrift1.Size = new System.Drawing.Size(88, 16);
            this.capUeberschrift1.Text = "Überschrift 1:";
            this.capUeberschrift1.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbUeberschift3
            // 
            this.txbUeberschift3.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift3.Location = new System.Drawing.Point(624, 56);
            this.txbUeberschift3.Name = "txbUeberschift3";
            this.txbUeberschift3.Regex = null;
            this.txbUeberschift3.Size = new System.Drawing.Size(288, 24);
            this.txbUeberschift3.TabIndex = 38;
            // 
            // txbUeberschift2
            // 
            this.txbUeberschift2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift2.Location = new System.Drawing.Point(624, 32);
            this.txbUeberschift2.Name = "txbUeberschift2";
            this.txbUeberschift2.Regex = null;
            this.txbUeberschift2.Size = new System.Drawing.Size(288, 24);
            this.txbUeberschift2.TabIndex = 37;
            // 
            // txbUeberschift1
            // 
            this.txbUeberschift1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbUeberschift1.Location = new System.Drawing.Point(624, 8);
            this.txbUeberschift1.Name = "txbUeberschift1";
            this.txbUeberschift1.Regex = null;
            this.txbUeberschift1.Size = new System.Drawing.Size(288, 24);
            this.txbUeberschift1.TabIndex = 36;
            // 
            // capReplacer
            // 
            this.capReplacer.CausesValidation = false;
            this.capReplacer.Location = new System.Drawing.Point(8, 264);
            this.capReplacer.Name = "capReplacer";
            this.capReplacer.Size = new System.Drawing.Size(144, 24);
            this.capReplacer.Text = "Optische Ersetzungen:";
            // 
            // btnEinzeiligDarstellen
            // 
            this.btnEinzeiligDarstellen.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnEinzeiligDarstellen.Location = new System.Drawing.Point(8, 152);
            this.btnEinzeiligDarstellen.Name = "btnEinzeiligDarstellen";
            this.btnEinzeiligDarstellen.Size = new System.Drawing.Size(296, 16);
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
            this.cbxEinheit.Regex = null;
            this.cbxEinheit.Size = new System.Drawing.Size(168, 24);
            this.cbxEinheit.TabIndex = 31;
            // 
            // capSpaltenbild
            // 
            this.capSpaltenbild.CausesValidation = false;
            this.capSpaltenbild.Location = new System.Drawing.Point(528, 80);
            this.capSpaltenbild.Name = "capSpaltenbild";
            this.capSpaltenbild.Size = new System.Drawing.Size(152, 24);
            this.capSpaltenbild.Text = "Spaltenbild:";
            this.capSpaltenbild.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxFormat
            // 
            this.cbxFormat.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFormat.Location = new System.Drawing.Point(8, 24);
            this.cbxFormat.Name = "cbxFormat";
            this.cbxFormat.Regex = null;
            this.cbxFormat.Size = new System.Drawing.Size(216, 24);
            this.cbxFormat.TabIndex = 27;
            this.cbxFormat.TextChanged += new System.EventHandler(this.cbxFormat_TextChanged);
            // 
            // capFormat
            // 
            this.capFormat.CausesValidation = false;
            this.capFormat.Location = new System.Drawing.Point(8, 8);
            this.capFormat.Name = "capFormat";
            this.capFormat.Size = new System.Drawing.Size(136, 16);
            this.capFormat.Text = "<b><u>Format:";
            this.capFormat.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnBackColor
            // 
            this.btnBackColor.Location = new System.Drawing.Point(248, 40);
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
            this.cbxRandRechts.Location = new System.Drawing.Point(384, 88);
            this.cbxRandRechts.Name = "cbxRandRechts";
            this.cbxRandRechts.Regex = null;
            this.cbxRandRechts.Size = new System.Drawing.Size(128, 24);
            this.cbxRandRechts.TabIndex = 25;
            // 
            // btnTextColor
            // 
            this.btnTextColor.Location = new System.Drawing.Point(248, 16);
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
            this.cbxRandLinks.Location = new System.Drawing.Point(248, 88);
            this.cbxRandLinks.Name = "cbxRandLinks";
            this.cbxRandLinks.Regex = null;
            this.cbxRandLinks.Size = new System.Drawing.Size(128, 24);
            this.cbxRandLinks.TabIndex = 24;
            // 
            // capLinkerRand
            // 
            this.capLinkerRand.CausesValidation = false;
            this.capLinkerRand.Location = new System.Drawing.Point(248, 72);
            this.capLinkerRand.Name = "capLinkerRand";
            this.capLinkerRand.Size = new System.Drawing.Size(80, 16);
            this.capLinkerRand.Text = "Linker Rand:";
            this.capLinkerRand.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRechterRand
            // 
            this.capRechterRand.CausesValidation = false;
            this.capRechterRand.Location = new System.Drawing.Point(384, 72);
            this.capRechterRand.Name = "capRechterRand";
            this.capRechterRand.Size = new System.Drawing.Size(88, 16);
            this.capRechterRand.Text = "Rechter Rand:";
            this.capRechterRand.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnMultiline
            // 
            this.btnMultiline.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnMultiline.Location = new System.Drawing.Point(8, 128);
            this.btnMultiline.Name = "btnMultiline";
            this.btnMultiline.Size = new System.Drawing.Size(296, 16);
            this.btnMultiline.TabIndex = 7;
            this.btnMultiline.Text = "Mehrere Einträge pro Zelle erlaubt (mehrzeilig)";
            // 
            // grpBildCode
            // 
            this.grpBildCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpBildCode.CausesValidation = false;
            this.grpBildCode.Controls.Add(this.capBildCodeConstHeight);
            this.grpBildCode.Controls.Add(this.txbBildCodeConstHeight);
            this.grpBildCode.Location = new System.Drawing.Point(456, 8);
            this.grpBildCode.Name = "grpBildCode";
            this.grpBildCode.Size = new System.Drawing.Size(440, 104);
            this.grpBildCode.TabIndex = 2;
            this.grpBildCode.TabStop = false;
            this.grpBildCode.Text = "Fomat: BildCode";
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
            this.txbBildCodeConstHeight.Location = new System.Drawing.Point(200, 16);
            this.txbBildCodeConstHeight.Name = "txbBildCodeConstHeight";
            this.txbBildCodeConstHeight.Regex = null;
            this.txbBildCodeConstHeight.Size = new System.Drawing.Size(64, 24);
            this.txbBildCodeConstHeight.Suffix = "Pixel";
            this.txbBildCodeConstHeight.TabIndex = 32;
            this.txbBildCodeConstHeight.Text = "0";
            // 
            // tabRechte
            // 
            this.tabRechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabRechte.Controls.Add(this.grpAuswahlmenuOptionen);
            this.tabRechte.Controls.Add(this.btnIgnoreLock);
            this.tabRechte.Controls.Add(this.lbxCellEditor);
            this.tabRechte.Controls.Add(this.btnEditableStandard);
            this.tabRechte.Controls.Add(this.capUserGroupEdit);
            this.tabRechte.Controls.Add(this.btnEditableDropdown);
            this.tabRechte.Location = new System.Drawing.Point(4, 25);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Padding = new System.Windows.Forms.Padding(3);
            this.tabRechte.Size = new System.Drawing.Size(912, 487);
            this.tabRechte.TabIndex = 1;
            this.tabRechte.Text = "Rechte";
            // 
            // grpAuswahlmenuOptionen
            // 
            this.grpAuswahlmenuOptionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuswahlmenuOptionen.CausesValidation = false;
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnOtherValuesToo);
            this.grpAuswahlmenuOptionen.Controls.Add(this.tbxAuswaehlbareWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.capImmerWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnCanBeEmpty);
            this.grpAuswahlmenuOptionen.Controls.Add(this.cbxDropDownKey);
            this.grpAuswahlmenuOptionen.Controls.Add(this.capDropDownKey);
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
            this.btnOtherValuesToo.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnOtherValuesToo.Location = new System.Drawing.Point(8, 304);
            this.btnOtherValuesToo.Name = "btnOtherValuesToo";
            this.btnOtherValuesToo.Size = new System.Drawing.Size(512, 32);
            this.btnOtherValuesToo.TabIndex = 7;
            this.btnOtherValuesToo.Text = "Auch Werte, die in anderen Zellen derselben Spalte vorhanden sind, werden zum Aus" +
    "wählen vorgschlagen";
            // 
            // tbxAuswaehlbareWerte
            // 
            this.tbxAuswaehlbareWerte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAuswaehlbareWerte.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxAuswaehlbareWerte.Location = new System.Drawing.Point(8, 64);
            this.tbxAuswaehlbareWerte.MultiLine = true;
            this.tbxAuswaehlbareWerte.Name = "tbxAuswaehlbareWerte";
            this.tbxAuswaehlbareWerte.Regex = null;
            this.tbxAuswaehlbareWerte.Size = new System.Drawing.Size(520, 231);
            this.tbxAuswaehlbareWerte.SpellChecking = true;
            this.tbxAuswaehlbareWerte.TabIndex = 0;
            // 
            // capImmerWerte
            // 
            this.capImmerWerte.CausesValidation = false;
            this.capImmerWerte.Location = new System.Drawing.Point(8, 48);
            this.capImmerWerte.Name = "capImmerWerte";
            this.capImmerWerte.Size = new System.Drawing.Size(216, 16);
            this.capImmerWerte.Text = "<b><u>Immer auswählbare Werte:";
            this.capImmerWerte.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnCanBeEmpty
            // 
            this.btnCanBeEmpty.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnCanBeEmpty.Location = new System.Drawing.Point(8, 24);
            this.btnCanBeEmpty.Name = "btnCanBeEmpty";
            this.btnCanBeEmpty.Size = new System.Drawing.Size(328, 16);
            this.btnCanBeEmpty.TabIndex = 6;
            this.btnCanBeEmpty.Text = "Alles abwählen erlaubt (leere Zelle möglich)";
            // 
            // cbxDropDownKey
            // 
            this.cbxDropDownKey.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxDropDownKey.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxDropDownKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDropDownKey.Location = new System.Drawing.Point(232, 344);
            this.cbxDropDownKey.Name = "cbxDropDownKey";
            this.cbxDropDownKey.Regex = null;
            this.cbxDropDownKey.Size = new System.Drawing.Size(296, 24);
            this.cbxDropDownKey.TabIndex = 3;
            // 
            // capDropDownKey
            // 
            this.capDropDownKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.capDropDownKey.CausesValidation = false;
            this.capDropDownKey.Location = new System.Drawing.Point(24, 336);
            this.capDropDownKey.Name = "capDropDownKey";
            this.capDropDownKey.Size = new System.Drawing.Size(208, 40);
            this.capDropDownKey.Text = "...aber nur, wenn der Inhalt dieser Spalte gleich ist:";
            this.capDropDownKey.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnIgnoreLock
            // 
            this.btnIgnoreLock.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnIgnoreLock.Location = new System.Drawing.Point(592, 328);
            this.btnIgnoreLock.Name = "btnIgnoreLock";
            this.btnIgnoreLock.Size = new System.Drawing.Size(288, 32);
            this.btnIgnoreLock.TabIndex = 27;
            this.btnIgnoreLock.Text = "Die Bearbeitung ist auch möglich, wenn die Zeile abgeschlossen ist.";
            // 
            // lbxCellEditor
            // 
            this.lbxCellEditor.AddAllowed = BlueControls.Enums.enAddType.Text;
            this.lbxCellEditor.CheckBehavior = BlueControls.Enums.enCheckBehavior.MultiSelection;
            this.lbxCellEditor.FilterAllowed = true;
            this.lbxCellEditor.Location = new System.Drawing.Point(576, 48);
            this.lbxCellEditor.Name = "lbxCellEditor";
            this.lbxCellEditor.RemoveAllowed = true;
            this.lbxCellEditor.Size = new System.Drawing.Size(328, 272);
            this.lbxCellEditor.TabIndex = 26;
            // 
            // btnEditableStandard
            // 
            this.btnEditableStandard.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
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
            this.capUserGroupEdit.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnEditableDropdown
            // 
            this.btnEditableDropdown.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnEditableDropdown.Location = new System.Drawing.Point(8, 56);
            this.btnEditableDropdown.Name = "btnEditableDropdown";
            this.btnEditableDropdown.Size = new System.Drawing.Size(544, 16);
            this.btnEditableDropdown.TabIndex = 5;
            this.btnEditableDropdown.Text = "Benutzer-Bearbeitung mit <b>Auswahl-Menü (Dropdown-Menü)</b> erlauben";
            // 
            // tabAutoBearbeitung
            // 
            this.tabAutoBearbeitung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAutoBearbeitung.Controls.Add(this.txbAutoReplace);
            this.tabAutoBearbeitung.Controls.Add(this.capAutoReplace);
            this.tabAutoBearbeitung.Controls.Add(this.txbAutoRemove);
            this.tabAutoBearbeitung.Controls.Add(this.capAutoRemove);
            this.tabAutoBearbeitung.Controls.Add(this.cbxSchlüsselspalte);
            this.tabAutoBearbeitung.Controls.Add(this.tbxInitValue);
            this.tabAutoBearbeitung.Controls.Add(this.Caption12);
            this.tabAutoBearbeitung.Controls.Add(this.capSchlüsselspalte);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditKleineFehler);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditToUpper);
            this.tabAutoBearbeitung.Controls.Add(this.tbxRunden);
            this.tabAutoBearbeitung.Controls.Add(this.cbxVorschlagSpalte);
            this.tabAutoBearbeitung.Controls.Add(this.capVorschlag);
            this.tabAutoBearbeitung.Controls.Add(this.capNachkommastellen);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditAutoSort);
            this.tabAutoBearbeitung.Location = new System.Drawing.Point(4, 25);
            this.tabAutoBearbeitung.Name = "tabAutoBearbeitung";
            this.tabAutoBearbeitung.Size = new System.Drawing.Size(912, 487);
            this.tabAutoBearbeitung.TabIndex = 6;
            this.tabAutoBearbeitung.Text = "Auto-Bearbeitung";
            // 
            // txbAutoReplace
            // 
            this.txbAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAutoReplace.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAutoReplace.Location = new System.Drawing.Point(8, 288);
            this.txbAutoReplace.MultiLine = true;
            this.txbAutoReplace.Name = "txbAutoReplace";
            this.txbAutoReplace.QuickInfo = resources.GetString("txbAutoReplace.QuickInfo");
            this.txbAutoReplace.Regex = null;
            this.txbAutoReplace.Size = new System.Drawing.Size(904, 192);
            this.txbAutoReplace.SpellChecking = true;
            this.txbAutoReplace.TabIndex = 39;
            // 
            // capAutoReplace
            // 
            this.capAutoReplace.CausesValidation = false;
            this.capAutoReplace.Location = new System.Drawing.Point(8, 272);
            this.capAutoReplace.Name = "capAutoReplace";
            this.capAutoReplace.Size = new System.Drawing.Size(184, 24);
            this.capAutoReplace.Text = "Permanente Ersetzungen:";
            // 
            // txbAutoRemove
            // 
            this.txbAutoRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbAutoRemove.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbAutoRemove.Location = new System.Drawing.Point(8, 240);
            this.txbAutoRemove.Name = "txbAutoRemove";
            this.txbAutoRemove.Regex = null;
            this.txbAutoRemove.Size = new System.Drawing.Size(896, 24);
            this.txbAutoRemove.TabIndex = 37;
            // 
            // capAutoRemove
            // 
            this.capAutoRemove.CausesValidation = false;
            this.capAutoRemove.Location = new System.Drawing.Point(8, 224);
            this.capAutoRemove.Name = "capAutoRemove";
            this.capAutoRemove.Size = new System.Drawing.Size(568, 16);
            this.capAutoRemove.Text = "Folgende Zeichen automatisch aus der Eingabe löschen:";
            // 
            // cbxSchlüsselspalte
            // 
            this.cbxSchlüsselspalte.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxSchlüsselspalte.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxSchlüsselspalte.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSchlüsselspalte.Location = new System.Drawing.Point(408, 176);
            this.cbxSchlüsselspalte.Name = "cbxSchlüsselspalte";
            this.cbxSchlüsselspalte.Regex = null;
            this.cbxSchlüsselspalte.Size = new System.Drawing.Size(496, 24);
            this.cbxSchlüsselspalte.TabIndex = 35;
            // 
            // tbxInitValue
            // 
            this.tbxInitValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxInitValue.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxInitValue.Location = new System.Drawing.Point(8, 32);
            this.tbxInitValue.Name = "tbxInitValue";
            this.tbxInitValue.Regex = null;
            this.tbxInitValue.Size = new System.Drawing.Size(896, 24);
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
            // capSchlüsselspalte
            // 
            this.capSchlüsselspalte.CausesValidation = false;
            this.capSchlüsselspalte.Location = new System.Drawing.Point(8, 176);
            this.capSchlüsselspalte.Name = "capSchlüsselspalte";
            this.capSchlüsselspalte.Size = new System.Drawing.Size(392, 40);
            this.capSchlüsselspalte.Text = "Die Werte der Zelle gleichhalten, wenn die Schlüsselspalte den gleichen Wert enth" +
    "ält:";
            this.capSchlüsselspalte.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnAutoEditKleineFehler
            // 
            this.btnAutoEditKleineFehler.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnAutoEditKleineFehler.Location = new System.Drawing.Point(432, 64);
            this.btnAutoEditKleineFehler.Name = "btnAutoEditKleineFehler";
            this.btnAutoEditKleineFehler.Size = new System.Drawing.Size(440, 24);
            this.btnAutoEditKleineFehler.TabIndex = 13;
            this.btnAutoEditKleineFehler.Text = "Kleinere Fehler, wie z.B. doppelte Leerzeichen automatisch korrigieren";
            // 
            // btnAutoEditToUpper
            // 
            this.btnAutoEditToUpper.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnAutoEditToUpper.Location = new System.Drawing.Point(8, 64);
            this.btnAutoEditToUpper.Name = "btnAutoEditToUpper";
            this.btnAutoEditToUpper.Size = new System.Drawing.Size(416, 24);
            this.btnAutoEditToUpper.TabIndex = 12;
            this.btnAutoEditToUpper.Text = "Texte in Grossbuchstaben ändern";
            // 
            // tbxRunden
            // 
            this.tbxRunden.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxRunden.Location = new System.Drawing.Point(224, 96);
            this.tbxRunden.Name = "tbxRunden";
            this.tbxRunden.Regex = null;
            this.tbxRunden.Size = new System.Drawing.Size(88, 24);
            this.tbxRunden.TabIndex = 11;
            // 
            // cbxVorschlagSpalte
            // 
            this.cbxVorschlagSpalte.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxVorschlagSpalte.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxVorschlagSpalte.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxVorschlagSpalte.Location = new System.Drawing.Point(416, 128);
            this.cbxVorschlagSpalte.Name = "cbxVorschlagSpalte";
            this.cbxVorschlagSpalte.Regex = null;
            this.cbxVorschlagSpalte.Size = new System.Drawing.Size(496, 24);
            this.cbxVorschlagSpalte.TabIndex = 5;
            // 
            // capVorschlag
            // 
            this.capVorschlag.CausesValidation = false;
            this.capVorschlag.Location = new System.Drawing.Point(8, 128);
            this.capVorschlag.Name = "capVorschlag";
            this.capVorschlag.Size = new System.Drawing.Size(392, 40);
            this.capVorschlag.Text = "Zelle automatisch befüllen, wenn diese leer ist und sie bearbeitet wird. Basieren" +
    "d auf gleichen Werten in dieser Spalte:";
            this.capVorschlag.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capNachkommastellen
            // 
            this.capNachkommastellen.CausesValidation = false;
            this.capNachkommastellen.Location = new System.Drawing.Point(8, 96);
            this.capNachkommastellen.Name = "capNachkommastellen";
            this.capNachkommastellen.Size = new System.Drawing.Size(328, 16);
            this.capNachkommastellen.Text = "Zahlen runden auf Kommastellen:";
            // 
            // btnAutoEditAutoSort
            // 
            this.btnAutoEditAutoSort.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnAutoEditAutoSort.Location = new System.Drawing.Point(432, 88);
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
            this.tabFilter.Controls.Add(this.capDauerfilterPos);
            this.tabFilter.Controls.Add(this.txbDauerFilterPos);
            this.tabFilter.Controls.Add(this.capJokerValue);
            this.tabFilter.Controls.Add(this.tbxJoker);
            this.tabFilter.Controls.Add(this.btnZeilenFilterIgnorieren);
            this.tabFilter.Controls.Add(this.btnAutoFilterMoeglich);
            this.tabFilter.Controls.Add(this.btnAutoFilterTXTErlaubt);
            this.tabFilter.Controls.Add(this.btnAutoFilterErweitertErlaubt);
            this.tabFilter.Location = new System.Drawing.Point(4, 25);
            this.tabFilter.Name = "tabFilter";
            this.tabFilter.Padding = new System.Windows.Forms.Padding(3);
            this.tabFilter.Size = new System.Drawing.Size(912, 487);
            this.tabFilter.TabIndex = 2;
            this.tabFilter.Text = "Filter";
            // 
            // chkFilterOnlyOr
            // 
            this.chkFilterOnlyOr.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.chkFilterOnlyOr.Location = new System.Drawing.Point(32, 88);
            this.chkFilterOnlyOr.Name = "chkFilterOnlyOr";
            this.chkFilterOnlyOr.Size = new System.Drawing.Size(192, 16);
            this.chkFilterOnlyOr.TabIndex = 35;
            this.chkFilterOnlyOr.Text = "nur <b>ODER</b>-Filterung erlauben";
            // 
            // chkFilterOnlyAND
            // 
            this.chkFilterOnlyAND.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.chkFilterOnlyAND.Location = new System.Drawing.Point(32, 72);
            this.chkFilterOnlyAND.Name = "chkFilterOnlyAND";
            this.chkFilterOnlyAND.Size = new System.Drawing.Size(192, 16);
            this.chkFilterOnlyAND.TabIndex = 34;
            this.chkFilterOnlyAND.Text = "nur <b>UND</b>-Filterung erlauben";
            // 
            // capDauerfilterPos
            // 
            this.capDauerfilterPos.CausesValidation = false;
            this.capDauerfilterPos.Location = new System.Drawing.Point(4, 369);
            this.capDauerfilterPos.Name = "capDauerfilterPos";
            this.capDauerfilterPos.Size = new System.Drawing.Size(144, 24);
            this.capDauerfilterPos.Text = "Dauerfilter-Pos.:";
            // 
            // txbDauerFilterPos
            // 
            this.txbDauerFilterPos.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbDauerFilterPos.Location = new System.Drawing.Point(156, 369);
            this.txbDauerFilterPos.Name = "txbDauerFilterPos";
            this.txbDauerFilterPos.Regex = null;
            this.txbDauerFilterPos.Size = new System.Drawing.Size(384, 24);
            this.txbDauerFilterPos.TabIndex = 33;
            // 
            // capJokerValue
            // 
            this.capJokerValue.CausesValidation = false;
            this.capJokerValue.Location = new System.Drawing.Point(4, 177);
            this.capJokerValue.Name = "capJokerValue";
            this.capJokerValue.Size = new System.Drawing.Size(312, 56);
            this.capJokerValue.Text = "Bei Autofilter-Aktionen, Zellen mit folgenden Inhalt <b>immer</b> anzeigen, wenn " +
    "ein Wert gewählt wurde:<br>(Joker)";
            this.capJokerValue.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tbxJoker
            // 
            this.tbxJoker.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxJoker.Location = new System.Drawing.Point(4, 233);
            this.tbxJoker.Name = "tbxJoker";
            this.tbxJoker.Regex = null;
            this.tbxJoker.Size = new System.Drawing.Size(312, 24);
            this.tbxJoker.TabIndex = 7;
            // 
            // btnZeilenFilterIgnorieren
            // 
            this.btnZeilenFilterIgnorieren.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnZeilenFilterIgnorieren.Location = new System.Drawing.Point(8, 144);
            this.btnZeilenFilterIgnorieren.Name = "btnZeilenFilterIgnorieren";
            this.btnZeilenFilterIgnorieren.Size = new System.Drawing.Size(304, 16);
            this.btnZeilenFilterIgnorieren.TabIndex = 6;
            this.btnZeilenFilterIgnorieren.Text = "Bei Zeilenfilter ignorieren (Suchfeld-Eingabe)";
            // 
            // btnAutoFilterMoeglich
            // 
            this.btnAutoFilterMoeglich.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnAutoFilterMoeglich.Location = new System.Drawing.Point(12, 15);
            this.btnAutoFilterMoeglich.Name = "btnAutoFilterMoeglich";
            this.btnAutoFilterMoeglich.Size = new System.Drawing.Size(120, 16);
            this.btnAutoFilterMoeglich.TabIndex = 3;
            this.btnAutoFilterMoeglich.Text = "AutoFilter erlaubt";
            // 
            // btnAutoFilterTXTErlaubt
            // 
            this.btnAutoFilterTXTErlaubt.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnAutoFilterTXTErlaubt.Location = new System.Drawing.Point(32, 32);
            this.btnAutoFilterTXTErlaubt.Name = "btnAutoFilterTXTErlaubt";
            this.btnAutoFilterTXTErlaubt.Size = new System.Drawing.Size(208, 16);
            this.btnAutoFilterTXTErlaubt.TabIndex = 4;
            this.btnAutoFilterTXTErlaubt.Text = "AutoFilter - Texteingabe - erlaubt";
            // 
            // btnAutoFilterErweitertErlaubt
            // 
            this.btnAutoFilterErweitertErlaubt.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnAutoFilterErweitertErlaubt.Location = new System.Drawing.Point(32, 48);
            this.btnAutoFilterErweitertErlaubt.Name = "btnAutoFilterErweitertErlaubt";
            this.btnAutoFilterErweitertErlaubt.Size = new System.Drawing.Size(192, 16);
            this.btnAutoFilterErweitertErlaubt.TabIndex = 5;
            this.btnAutoFilterErweitertErlaubt.Text = "AutoFilter - Erweitert - erlaubt";
            // 
            // tabQuickInfo
            // 
            this.tabQuickInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabQuickInfo.Controls.Add(this.tbxAdminInfo);
            this.tabQuickInfo.Controls.Add(this.tbxQuickinfo);
            this.tabQuickInfo.Controls.Add(this.Caption18);
            this.tabQuickInfo.Controls.Add(this.Caption17);
            this.tabQuickInfo.Controls.Add(this.btnQI_Vorschau);
            this.tabQuickInfo.Location = new System.Drawing.Point(4, 25);
            this.tabQuickInfo.Name = "tabQuickInfo";
            this.tabQuickInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabQuickInfo.Size = new System.Drawing.Size(912, 487);
            this.tabQuickInfo.TabIndex = 3;
            this.tabQuickInfo.Text = "Quickinfo";
            // 
            // tbxAdminInfo
            // 
            this.tbxAdminInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAdminInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxAdminInfo.Location = new System.Drawing.Point(560, 24);
            this.tbxAdminInfo.MultiLine = true;
            this.tbxAdminInfo.Name = "tbxAdminInfo";
            this.tbxAdminInfo.Regex = null;
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
            this.tbxQuickinfo.Regex = null;
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
            // btnQI_Vorschau
            // 
            this.btnQI_Vorschau.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQI_Vorschau.Location = new System.Drawing.Point(808, 460);
            this.btnQI_Vorschau.Name = "btnQI_Vorschau";
            this.btnQI_Vorschau.Size = new System.Drawing.Size(96, 24);
            this.btnQI_Vorschau.TabIndex = 1;
            this.btnQI_Vorschau.Text = "Vorschau";
            this.btnQI_Vorschau.Click += new System.EventHandler(this.btnQI_Vorschau_Click);
            // 
            // tabSonstiges
            // 
            this.tabSonstiges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSonstiges.Controls.Add(this.cbxTranslate);
            this.tabSonstiges.Controls.Add(this.capTranslate);
            this.tabSonstiges.Controls.Add(this.cbxSort);
            this.tabSonstiges.Controls.Add(this.butSaveContent);
            this.tabSonstiges.Controls.Add(this.btnSpellChecking);
            this.tabSonstiges.Controls.Add(this.txbRegex);
            this.tabSonstiges.Controls.Add(this.capSortiermaske);
            this.tabSonstiges.Controls.Add(this.btnLogUndo);
            this.tabSonstiges.Controls.Add(this.capRegex);
            this.tabSonstiges.Controls.Add(this.tbxAllowedChars);
            this.tabSonstiges.Controls.Add(this.Caption13);
            this.tabSonstiges.Controls.Add(this.tbxTags);
            this.tabSonstiges.Controls.Add(this.Caption8);
            this.tabSonstiges.Location = new System.Drawing.Point(4, 25);
            this.tabSonstiges.Name = "tabSonstiges";
            this.tabSonstiges.Padding = new System.Windows.Forms.Padding(3);
            this.tabSonstiges.Size = new System.Drawing.Size(912, 487);
            this.tabSonstiges.TabIndex = 4;
            this.tabSonstiges.Text = "Sonstiges allgemein";
            // 
            // cbxTranslate
            // 
            this.cbxTranslate.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTranslate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTranslate.Location = new System.Drawing.Point(512, 368);
            this.cbxTranslate.Name = "cbxTranslate";
            this.cbxTranslate.Regex = null;
            this.cbxTranslate.Size = new System.Drawing.Size(368, 24);
            this.cbxTranslate.TabIndex = 37;
            // 
            // capTranslate
            // 
            this.capTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capTranslate.CausesValidation = false;
            this.capTranslate.Location = new System.Drawing.Point(512, 344);
            this.capTranslate.Name = "capTranslate";
            this.capTranslate.Size = new System.Drawing.Size(152, 24);
            this.capTranslate.Text = "Übersetzen:";
            // 
            // cbxSort
            // 
            this.cbxSort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSort.Location = new System.Drawing.Point(512, 312);
            this.cbxSort.Name = "cbxSort";
            this.cbxSort.Regex = null;
            this.cbxSort.Size = new System.Drawing.Size(368, 24);
            this.cbxSort.TabIndex = 35;
            // 
            // butSaveContent
            // 
            this.butSaveContent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveContent.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.butSaveContent.Location = new System.Drawing.Point(512, 432);
            this.butSaveContent.Name = "butSaveContent";
            this.butSaveContent.Size = new System.Drawing.Size(352, 24);
            this.butSaveContent.TabIndex = 34;
            this.butSaveContent.Text = "Inhalte der Zellen auf Festplatte speichern und laden";
            // 
            // btnSpellChecking
            // 
            this.btnSpellChecking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSpellChecking.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnSpellChecking.Location = new System.Drawing.Point(512, 248);
            this.btnSpellChecking.Name = "btnSpellChecking";
            this.btnSpellChecking.Size = new System.Drawing.Size(352, 16);
            this.btnSpellChecking.TabIndex = 33;
            this.btnSpellChecking.Text = "Rechtschreibprüfung aktivieren";
            // 
            // txbRegex
            // 
            this.txbRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txbRegex.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRegex.Location = new System.Drawing.Point(512, 128);
            this.txbRegex.Name = "txbRegex";
            this.txbRegex.Size = new System.Drawing.Size(320, 96);
            this.txbRegex.TabIndex = 9;
            // 
            // capSortiermaske
            // 
            this.capSortiermaske.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capSortiermaske.CausesValidation = false;
            this.capSortiermaske.Location = new System.Drawing.Point(512, 288);
            this.capSortiermaske.Name = "capSortiermaske";
            this.capSortiermaske.Size = new System.Drawing.Size(152, 24);
            this.capSortiermaske.Text = "Sortierung nach:";
            // 
            // btnLogUndo
            // 
            this.btnLogUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogUndo.ButtonStyle = ((BlueControls.Enums.enButtonStyle)((BlueControls.Enums.enButtonStyle.Checkbox | BlueControls.Enums.enButtonStyle.Text)));
            this.btnLogUndo.Location = new System.Drawing.Point(512, 224);
            this.btnLogUndo.Name = "btnLogUndo";
            this.btnLogUndo.Size = new System.Drawing.Size(352, 16);
            this.btnLogUndo.TabIndex = 32;
            this.btnLogUndo.Text = "Undo wird geloggt";
            // 
            // capRegex
            // 
            this.capRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capRegex.CausesValidation = false;
            this.capRegex.Location = new System.Drawing.Point(512, 112);
            this.capRegex.Name = "capRegex";
            this.capRegex.Size = new System.Drawing.Size(388, 17);
            this.capRegex.Text = "Das Format muss mit dieser Regex-Maske übereinstimmen:";
            // 
            // tbxAllowedChars
            // 
            this.tbxAllowedChars.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxAllowedChars.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbxAllowedChars.Location = new System.Drawing.Point(512, 48);
            this.tbxAllowedChars.Name = "tbxAllowedChars";
            this.tbxAllowedChars.Regex = null;
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
            this.tbxTags.Regex = null;
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
            // txbLinkedKeyKennung
            // 
            this.txbLinkedKeyKennung.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbLinkedKeyKennung.Location = new System.Drawing.Point(240, 24);
            this.txbLinkedKeyKennung.Name = "txbLinkedKeyKennung";
            this.txbLinkedKeyKennung.Regex = null;
            this.txbLinkedKeyKennung.Size = new System.Drawing.Size(232, 24);
            this.txbLinkedKeyKennung.TabIndex = 40;
            // 
            // capLinkedKeyKennung
            // 
            this.capLinkedKeyKennung.CausesValidation = false;
            this.capLinkedKeyKennung.Location = new System.Drawing.Point(8, 24);
            this.capLinkedKeyKennung.Name = "capLinkedKeyKennung";
            this.capLinkedKeyKennung.Size = new System.Drawing.Size(224, 16);
            this.capLinkedKeyKennung.Text = "Verknüpfte Spalten beginnen mit:";
            // 
            // cbxLinkedDatabase
            // 
            this.cbxLinkedDatabase.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxLinkedDatabase.Location = new System.Drawing.Point(172, 24);
            this.cbxLinkedDatabase.Name = "cbxLinkedDatabase";
            this.cbxLinkedDatabase.Regex = null;
            this.cbxLinkedDatabase.Size = new System.Drawing.Size(708, 24);
            this.cbxLinkedDatabase.TabIndex = 38;
            this.cbxLinkedDatabase.TextChanged += new System.EventHandler(this.cbxLinkedDatabase_TextChanged);
            // 
            // capLinkedDatabase
            // 
            this.capLinkedDatabase.CausesValidation = false;
            this.capLinkedDatabase.Location = new System.Drawing.Point(12, 25);
            this.capLinkedDatabase.Name = "capLinkedDatabase";
            this.capLinkedDatabase.Size = new System.Drawing.Size(152, 16);
            this.capLinkedDatabase.Text = "Vernküpfte Datenbank:";
            // 
            // capBestFileStandardSuffix
            // 
            this.capBestFileStandardSuffix.CausesValidation = false;
            this.capBestFileStandardSuffix.Location = new System.Drawing.Point(8, 72);
            this.capBestFileStandardSuffix.Name = "capBestFileStandardSuffix";
            this.capBestFileStandardSuffix.Size = new System.Drawing.Size(264, 16);
            this.capBestFileStandardSuffix.Text = "Standard Dateiendung bei neuen Dateien:";
            // 
            // txbBestFileStandardSuffix
            // 
            this.txbBestFileStandardSuffix.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbBestFileStandardSuffix.Location = new System.Drawing.Point(280, 72);
            this.txbBestFileStandardSuffix.Name = "txbBestFileStandardSuffix";
            this.txbBestFileStandardSuffix.Regex = null;
            this.txbBestFileStandardSuffix.Size = new System.Drawing.Size(152, 24);
            this.txbBestFileStandardSuffix.TabIndex = 36;
            // 
            // capBestFileStandardFolder
            // 
            this.capBestFileStandardFolder.CausesValidation = false;
            this.capBestFileStandardFolder.Location = new System.Drawing.Point(8, 24);
            this.capBestFileStandardFolder.Name = "capBestFileStandardFolder";
            this.capBestFileStandardFolder.Size = new System.Drawing.Size(320, 16);
            this.capBestFileStandardFolder.Text = "Standard Speicher Ordner bei Dateien:";
            // 
            // txbBestFileStandardFolder
            // 
            this.txbBestFileStandardFolder.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbBestFileStandardFolder.Location = new System.Drawing.Point(8, 40);
            this.txbBestFileStandardFolder.Name = "txbBestFileStandardFolder";
            this.txbBestFileStandardFolder.Regex = null;
            this.txbBestFileStandardFolder.Size = new System.Drawing.Size(424, 24);
            this.txbBestFileStandardFolder.TabIndex = 34;
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
            this.BlueFrame1.Controls.Add(this.tbxName);
            this.BlueFrame1.Controls.Add(this.Caption2);
            this.BlueFrame1.Controls.Add(this.tbxCaption);
            this.BlueFrame1.Location = new System.Drawing.Point(8, 8);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(916, 104);
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
            this.tbxName.Regex = null;
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
            this.tbxCaption.Regex = null;
            this.tbxCaption.Size = new System.Drawing.Size(600, 64);
            this.tbxCaption.TabIndex = 2;
            // 
            // btnVor
            // 
            this.btnVor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVor.ImageCode = "Pfeil_Rechts|16|||FF0000";
            this.btnVor.Location = new System.Drawing.Point(304, 640);
            this.btnVor.Name = "btnVor";
            this.btnVor.Size = new System.Drawing.Size(72, 24);
            this.btnVor.TabIndex = 5;
            this.btnVor.Click += new System.EventHandler(this.btnVor_Click);
            // 
            // btnZurueck
            // 
            this.btnZurueck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZurueck.ImageCode = "Pfeil_Links|16|||FF0000";
            this.btnZurueck.Location = new System.Drawing.Point(224, 640);
            this.btnZurueck.Name = "btnZurueck";
            this.btnZurueck.Size = new System.Drawing.Size(72, 24);
            this.btnZurueck.TabIndex = 4;
            this.btnZurueck.Click += new System.EventHandler(this.btnZurueck_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.ImageCode = "Häkchen|16";
            this.btnOk.Location = new System.Drawing.Point(840, 640);
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
            this.tabControl.Controls.Add(this.tabSchnellformat);
            this.tabControl.Controls.Add(this.tabDesign);
            this.tabControl.Controls.Add(this.tabRechte);
            this.tabControl.Controls.Add(this.tabAutoBearbeitung);
            this.tabControl.Controls.Add(this.tabFilter);
            this.tabControl.Controls.Add(this.tabQuickInfo);
            this.tabControl.Controls.Add(this.tabSonstiges);
            this.tabControl.Controls.Add(this.tabSpezial);
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new System.Drawing.Point(0, 120);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(920, 516);
            this.tabControl.TabIndex = 15;
            // 
            // tabSchnellformat
            // 
            this.tabSchnellformat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSchnellformat.Controls.Add(this.btnSchnellBildCode);
            this.tabSchnellformat.Controls.Add(this.btnSchnellIInternetAdresse);
            this.tabSchnellformat.Controls.Add(this.btnSchnellAuswahloptionen);
            this.tabSchnellformat.Controls.Add(this.btnSchnellGleitkommazahl);
            this.tabSchnellformat.Controls.Add(this.btnSchnellGanzzahl);
            this.tabSchnellformat.Controls.Add(this.btnSchnellTelefonNummer);
            this.tabSchnellformat.Controls.Add(this.btnSchnellEmail);
            this.tabSchnellformat.Controls.Add(this.btnSchnellDatumUhrzeit);
            this.tabSchnellformat.Controls.Add(this.btnSchnellDatum);
            this.tabSchnellformat.Controls.Add(this.btnSchnellText);
            this.tabSchnellformat.Location = new System.Drawing.Point(4, 25);
            this.tabSchnellformat.Name = "tabSchnellformat";
            this.tabSchnellformat.Padding = new System.Windows.Forms.Padding(3);
            this.tabSchnellformat.Size = new System.Drawing.Size(912, 487);
            this.tabSchnellformat.TabIndex = 10;
            this.tabSchnellformat.Text = "Schnellformat";
            // 
            // btnSchnellBildCode
            // 
            this.btnSchnellBildCode.Location = new System.Drawing.Point(432, 80);
            this.btnSchnellBildCode.Name = "btnSchnellBildCode";
            this.btnSchnellBildCode.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellBildCode.TabIndex = 9;
            this.btnSchnellBildCode.Text = "Bild-Code";
            this.btnSchnellBildCode.Click += new System.EventHandler(this.btnSchnellBildCode_Click);
            // 
            // btnSchnellIInternetAdresse
            // 
            this.btnSchnellIInternetAdresse.Location = new System.Drawing.Point(296, 80);
            this.btnSchnellIInternetAdresse.Name = "btnSchnellIInternetAdresse";
            this.btnSchnellIInternetAdresse.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellIInternetAdresse.TabIndex = 8;
            this.btnSchnellIInternetAdresse.Text = "Internet-Adresse";
            this.btnSchnellIInternetAdresse.Click += new System.EventHandler(this.btnSchnellIInternetAdresse_Click);
            // 
            // btnSchnellAuswahloptionen
            // 
            this.btnSchnellAuswahloptionen.Location = new System.Drawing.Point(160, 24);
            this.btnSchnellAuswahloptionen.Name = "btnSchnellAuswahloptionen";
            this.btnSchnellAuswahloptionen.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellAuswahloptionen.TabIndex = 7;
            this.btnSchnellAuswahloptionen.Text = "Auswahl-Optionen";
            this.btnSchnellAuswahloptionen.Click += new System.EventHandler(this.btnSchnellAuswahloptionen_Click);
            // 
            // btnSchnellGleitkommazahl
            // 
            this.btnSchnellGleitkommazahl.Location = new System.Drawing.Point(704, 24);
            this.btnSchnellGleitkommazahl.Name = "btnSchnellGleitkommazahl";
            this.btnSchnellGleitkommazahl.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellGleitkommazahl.TabIndex = 6;
            this.btnSchnellGleitkommazahl.Text = "Gleitkommazahl";
            this.btnSchnellGleitkommazahl.Click += new System.EventHandler(this.btnSchnellGleitkommazahl_Click);
            // 
            // btnSchnellGanzzahl
            // 
            this.btnSchnellGanzzahl.Location = new System.Drawing.Point(568, 24);
            this.btnSchnellGanzzahl.Name = "btnSchnellGanzzahl";
            this.btnSchnellGanzzahl.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellGanzzahl.TabIndex = 5;
            this.btnSchnellGanzzahl.Text = "Ganzzahl";
            this.btnSchnellGanzzahl.Click += new System.EventHandler(this.btnSchnellGanzzahl_Click);
            // 
            // btnSchnellTelefonNummer
            // 
            this.btnSchnellTelefonNummer.Location = new System.Drawing.Point(160, 80);
            this.btnSchnellTelefonNummer.Name = "btnSchnellTelefonNummer";
            this.btnSchnellTelefonNummer.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellTelefonNummer.TabIndex = 4;
            this.btnSchnellTelefonNummer.Text = "Telefonnummer";
            this.btnSchnellTelefonNummer.Click += new System.EventHandler(this.btnSchnellTelefonNummer_Click);
            // 
            // btnSchnellEmail
            // 
            this.btnSchnellEmail.Location = new System.Drawing.Point(24, 80);
            this.btnSchnellEmail.Name = "btnSchnellEmail";
            this.btnSchnellEmail.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellEmail.TabIndex = 3;
            this.btnSchnellEmail.Text = "Email";
            this.btnSchnellEmail.Click += new System.EventHandler(this.btnSchnellEmail_Click);
            // 
            // btnSchnellDatumUhrzeit
            // 
            this.btnSchnellDatumUhrzeit.Location = new System.Drawing.Point(432, 24);
            this.btnSchnellDatumUhrzeit.Name = "btnSchnellDatumUhrzeit";
            this.btnSchnellDatumUhrzeit.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellDatumUhrzeit.TabIndex = 2;
            this.btnSchnellDatumUhrzeit.Text = "Datum/Uhrzeit";
            this.btnSchnellDatumUhrzeit.Click += new System.EventHandler(this.btnSchnellDatumUhrzeit_Click);
            // 
            // btnSchnellDatum
            // 
            this.btnSchnellDatum.Location = new System.Drawing.Point(296, 24);
            this.btnSchnellDatum.Name = "btnSchnellDatum";
            this.btnSchnellDatum.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellDatum.TabIndex = 1;
            this.btnSchnellDatum.Text = "Datum";
            this.btnSchnellDatum.Click += new System.EventHandler(this.btnSchnellDatum_Click);
            // 
            // btnSchnellText
            // 
            this.btnSchnellText.Location = new System.Drawing.Point(24, 24);
            this.btnSchnellText.Name = "btnSchnellText";
            this.btnSchnellText.Size = new System.Drawing.Size(128, 48);
            this.btnSchnellText.TabIndex = 0;
            this.btnSchnellText.Text = "Text";
            this.btnSchnellText.Click += new System.EventHandler(this.btnSchnellText_Click);
            // 
            // tabSpezial
            // 
            this.tabSpezial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSpezial.Controls.Add(this.grpLinkToFileSystem);
            this.tabSpezial.Controls.Add(this.grpLinkedDatabase);
            this.tabSpezial.Controls.Add(this.grpBildCode);
            this.tabSpezial.Location = new System.Drawing.Point(4, 25);
            this.tabSpezial.Name = "tabSpezial";
            this.tabSpezial.Size = new System.Drawing.Size(912, 487);
            this.tabSpezial.TabIndex = 9;
            this.tabSpezial.Text = "Spezial-Formate";
            // 
            // grpLinkToFileSystem
            // 
            this.grpLinkToFileSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpLinkToFileSystem.CausesValidation = false;
            this.grpLinkToFileSystem.Controls.Add(this.capBestFileStandardFolder);
            this.grpLinkToFileSystem.Controls.Add(this.txbBestFileStandardFolder);
            this.grpLinkToFileSystem.Controls.Add(this.txbBestFileStandardSuffix);
            this.grpLinkToFileSystem.Controls.Add(this.capBestFileStandardSuffix);
            this.grpLinkToFileSystem.Location = new System.Drawing.Point(8, 8);
            this.grpLinkToFileSystem.Name = "grpLinkToFileSystem";
            this.grpLinkToFileSystem.Size = new System.Drawing.Size(440, 104);
            this.grpLinkToFileSystem.TabIndex = 0;
            this.grpLinkToFileSystem.TabStop = false;
            this.grpLinkToFileSystem.Text = "Format: Link To Filesystem";
            // 
            // grpLinkedDatabase
            // 
            this.grpLinkedDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpLinkedDatabase.CausesValidation = false;
            this.grpLinkedDatabase.Controls.Add(this.grpColumnsForLinkedDatabase);
            this.grpLinkedDatabase.Controls.Add(this.grpVerlinkteZellen);
            this.grpLinkedDatabase.Controls.Add(this.cbxLinkedDatabase);
            this.grpLinkedDatabase.Controls.Add(this.capLinkedDatabase);
            this.grpLinkedDatabase.Location = new System.Drawing.Point(8, 120);
            this.grpLinkedDatabase.Name = "grpLinkedDatabase";
            this.grpLinkedDatabase.Size = new System.Drawing.Size(888, 224);
            this.grpLinkedDatabase.TabIndex = 1;
            this.grpLinkedDatabase.TabStop = false;
            this.grpLinkedDatabase.Text = "Format: LinkedCell (und dessen Columns / Values)";
            // 
            // grpColumnsForLinkedDatabase
            // 
            this.grpColumnsForLinkedDatabase.CausesValidation = false;
            this.grpColumnsForLinkedDatabase.Controls.Add(this.txbLinkedKeyKennung);
            this.grpColumnsForLinkedDatabase.Controls.Add(this.capLinkedKeyKennung);
            this.grpColumnsForLinkedDatabase.Location = new System.Drawing.Point(8, 64);
            this.grpColumnsForLinkedDatabase.Name = "grpColumnsForLinkedDatabase";
            this.grpColumnsForLinkedDatabase.Size = new System.Drawing.Size(872, 56);
            this.grpColumnsForLinkedDatabase.TabIndex = 0;
            this.grpColumnsForLinkedDatabase.TabStop = false;
            this.grpColumnsForLinkedDatabase.Text = "Format: Columns_für_LinkedCellDropdown";
            // 
            // grpVerlinkteZellen
            // 
            this.grpVerlinkteZellen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpVerlinkteZellen.CausesValidation = false;
            this.grpVerlinkteZellen.Controls.Add(this.cbxTargetColumn);
            this.grpVerlinkteZellen.Controls.Add(this.cbxRowKeyInColumn);
            this.grpVerlinkteZellen.Controls.Add(this.capTargetColumn);
            this.grpVerlinkteZellen.Controls.Add(this.capRowKeyInColumn);
            this.grpVerlinkteZellen.Location = new System.Drawing.Point(8, 128);
            this.grpVerlinkteZellen.Name = "grpVerlinkteZellen";
            this.grpVerlinkteZellen.Size = new System.Drawing.Size(872, 88);
            this.grpVerlinkteZellen.TabIndex = 1;
            this.grpVerlinkteZellen.TabStop = false;
            this.grpVerlinkteZellen.Text = "Format: Verlinkte Zellen";
            // 
            // cbxTargetColumn
            // 
            this.cbxTargetColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxTargetColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxTargetColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTargetColumn.Location = new System.Drawing.Point(336, 48);
            this.cbxTargetColumn.Name = "cbxTargetColumn";
            this.cbxTargetColumn.Regex = null;
            this.cbxTargetColumn.Size = new System.Drawing.Size(520, 24);
            this.cbxTargetColumn.TabIndex = 5;
            // 
            // cbxRowKeyInColumn
            // 
            this.cbxRowKeyInColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRowKeyInColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRowKeyInColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRowKeyInColumn.Location = new System.Drawing.Point(336, 24);
            this.cbxRowKeyInColumn.Name = "cbxRowKeyInColumn";
            this.cbxRowKeyInColumn.Regex = null;
            this.cbxRowKeyInColumn.Size = new System.Drawing.Size(520, 24);
            this.cbxRowKeyInColumn.TabIndex = 3;
            // 
            // capTargetColumn
            // 
            this.capTargetColumn.CausesValidation = false;
            this.capTargetColumn.Location = new System.Drawing.Point(8, 48);
            this.capTargetColumn.Name = "capTargetColumn";
            this.capTargetColumn.Size = new System.Drawing.Size(320, 32);
            this.capTargetColumn.Text = "Die Quell-Spalte (aus der verlinkten Datenbank) ist immer:";
            this.capTargetColumn.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRowKeyInColumn
            // 
            this.capRowKeyInColumn.CausesValidation = false;
            this.capRowKeyInColumn.Location = new System.Drawing.Point(8, 24);
            this.capRowKeyInColumn.Name = "capRowKeyInColumn";
            this.capRowKeyInColumn.Size = new System.Drawing.Size(320, 16);
            this.capRowKeyInColumn.Text = "Die zu suchende Zeile ist in dieser Spalte zu finden:";
            // 
            // capIntern
            // 
            this.capIntern.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.capIntern.CausesValidation = false;
            this.capIntern.Location = new System.Drawing.Point(112, 640);
            this.capIntern.Name = "capIntern";
            this.capIntern.Size = new System.Drawing.Size(104, 24);
            this.capIntern.Text = "Interne Ansicht:";
            // 
            // caption5
            // 
            this.caption5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.caption5.CausesValidation = false;
            this.caption5.Location = new System.Drawing.Point(460, 641);
            this.caption5.Name = "caption5";
            this.caption5.Size = new System.Drawing.Size(104, 24);
            this.caption5.Text = "Aktuelle Ansicht:";
            // 
            // butAktuellVor
            // 
            this.butAktuellVor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAktuellVor.ImageCode = "Pfeil_Rechts|16|||0000FF";
            this.butAktuellVor.Location = new System.Drawing.Point(652, 641);
            this.butAktuellVor.Name = "butAktuellVor";
            this.butAktuellVor.Size = new System.Drawing.Size(72, 24);
            this.butAktuellVor.TabIndex = 19;
            this.butAktuellVor.Click += new System.EventHandler(this.butAktuellVor_Click);
            // 
            // butAktuellZurueck
            // 
            this.butAktuellZurueck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAktuellZurueck.ImageCode = "Pfeil_Links|16|||0000FF";
            this.butAktuellZurueck.Location = new System.Drawing.Point(572, 641);
            this.butAktuellZurueck.Name = "butAktuellZurueck";
            this.butAktuellZurueck.Size = new System.Drawing.Size(72, 24);
            this.butAktuellZurueck.TabIndex = 18;
            this.butAktuellZurueck.Click += new System.EventHandler(this.butAktuellZurueck_Click);
            // 
            // cbxScriptType
            // 
            this.cbxScriptType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxScriptType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxScriptType.Location = new System.Drawing.Point(600, 144);
            this.cbxScriptType.Name = "cbxScriptType";
            this.cbxScriptType.Regex = null;
            this.cbxScriptType.Size = new System.Drawing.Size(304, 32);
            this.cbxScriptType.TabIndex = 43;
            // 
            // capScriptType
            // 
            this.capScriptType.CausesValidation = false;
            this.capScriptType.Location = new System.Drawing.Point(528, 144);
            this.capScriptType.Name = "capScriptType";
            this.capScriptType.Size = new System.Drawing.Size(72, 24);
            this.capScriptType.Text = "Skript-Typ:";
            // 
            // ColumnEditor
            // 
            this.ClientSize = new System.Drawing.Size(926, 671);
            this.Controls.Add(this.caption5);
            this.Controls.Add(this.butAktuellVor);
            this.Controls.Add(this.butAktuellZurueck);
            this.Controls.Add(this.capIntern);
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
            this.grpBildCode.ResumeLayout(false);
            this.tabRechte.ResumeLayout(false);
            this.grpAuswahlmenuOptionen.ResumeLayout(false);
            this.tabAutoBearbeitung.ResumeLayout(false);
            this.tabFilter.ResumeLayout(false);
            this.tabQuickInfo.ResumeLayout(false);
            this.tabSonstiges.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabSchnellformat.ResumeLayout(false);
            this.tabSpezial.ResumeLayout(false);
            this.grpLinkToFileSystem.ResumeLayout(false);
            this.grpLinkedDatabase.ResumeLayout(false);
            this.grpColumnsForLinkedDatabase.ResumeLayout(false);
            this.grpVerlinkteZellen.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnOk;
        private TextBox tbxName;
        private Caption Caption3;
        private System.Windows.Forms.ColorDialog ColorDia;
        private Button btnQI_Vorschau;
        private TextBox tbxQuickinfo;
        private Button btnZeilenFilterIgnorieren;
        private Button btnAutoFilterErweitertErlaubt;
        private Button btnAutoFilterTXTErlaubt;
        private Button btnAutoFilterMoeglich;
        private Caption capInfo;
        private Button btnZurueck;
        private Button btnVor;
        private Button btnMultiline;
        private Caption capRechterRand;
        private Caption capLinkerRand;
        private Button btnTextColor;
        private Button btnBackColor;
        private Caption Caption2;
        private TextBox tbxCaption;
        private GroupBox BlueFrame1;
        private Button btnEditableDropdown;
        private Button btnEditableStandard;
        private TextBox tbxAuswaehlbareWerte;
        private Button btnCanBeEmpty;
        private Caption capUserGroupEdit;
        private Caption Caption8;
        private ComboBox cbxRandRechts;
        private ComboBox cbxRandLinks;
        private Caption Caption13;
        private TextBox tbxAllowedChars;
        private Caption capImmerWerte;
        private Caption capFormat;
        private Caption Caption18;
        private TextBox tbxAdminInfo;
        private Caption Caption17;
        private Button btnOtherValuesToo;
        private ListBox lbxCellEditor;
        private ComboBox cbxFormat;
        private TextBox tbxTags;
        private TabControl tabControl;
        private System.Windows.Forms.TabPage tabDesign;
        private System.Windows.Forms.TabPage tabRechte;
        private System.Windows.Forms.TabPage tabAutoBearbeitung;
        private System.Windows.Forms.TabPage tabFilter;
        private System.Windows.Forms.TabPage tabQuickInfo;
        private System.Windows.Forms.TabPage tabSonstiges;
        private Button btnEinzeiligDarstellen;
        private Button btnIgnoreLock;
        private Button btnLogUndo;
        private Button btnSpellChecking;
        private Caption capSpaltenbild;
        private Caption capEinheit;
        private ComboBox cbxEinheit;
        private TextBox tbxJoker;
        private Caption capJokerValue;
        private TextBox tbxInitValue;
        private Caption Caption12;
        private Button btnAutoEditKleineFehler;
        private Button btnAutoEditToUpper;
        private TextBox tbxRunden;
        private Caption capNachkommastellen;
        private Button btnAutoEditAutoSort;
        private ComboBox cbxBildTextVerhalten;
        private Caption capBildCodeImageNotfound;
        private Caption capBildCodeConstHeight;
        private TextBox txbBildCodeConstHeight;
        private TextBox txbLinkedKeyKennung;
        private Caption capLinkedKeyKennung;
        private ComboBox cbxLinkedDatabase;
        private Caption capLinkedDatabase;
        private Caption capBestFileStandardSuffix;
        private TextBox txbBestFileStandardSuffix;
        private Caption capBestFileStandardFolder;
        private TextBox txbBestFileStandardFolder;
        private GroupBox grpVerlinkteZellen;
        private ComboBox cbxTargetColumn;
        private ComboBox cbxRowKeyInColumn;
        private Caption capTargetColumn;
        private Caption capRowKeyInColumn;
        private TextBox txbReplacer;
        private Caption capReplacer;
        private Caption capUeberschrift3;
        private Caption capUeberschrift2;
        private Caption capUeberschrift1;
        private TextBox txbUeberschift3;
        private TextBox txbUeberschift2;
        private TextBox txbUeberschift1;
        private GroupBox grpBildCode;
        private TextBox txbPrefix;
        private Caption capPraefix;
        private Button btnStandard;
        private System.Windows.Forms.TabPage tabSpezial;
        private GroupBox grpLinkToFileSystem;
        private GroupBox grpLinkedDatabase;
        private ComboBox cbxSchlüsselspalte;
        private Caption capSchlüsselspalte;
        private GroupBox grpColumnsForLinkedDatabase;
        private Caption capSortiermaske;
        private TextBox txbRegex;
        private Caption capRegex;
        private ComboBox cbxAlign;
        private Caption capAlign;
        private ComboBox cbxVorschlagSpalte;
        private Caption capVorschlag;
        private ComboBox cbxDropDownKey;
        private Caption capDropDownKey;
        private GroupBox grpAuswahlmenuOptionen;
        private TextBox txbAutoRemove;
        private Caption capAutoRemove;
        private Button butSaveContent;
        private Caption capDauerfilterPos;
        private TextBox txbDauerFilterPos;
        private Caption capIntern;
        private Caption caption5;
        private Button butAktuellVor;
        private Button butAktuellZurueck;
        private TextBox txbAutoReplace;
        private Caption capAutoReplace;
        private Button chkFilterOnlyAND;
        private Button chkFilterOnlyOr;
        private Button btnVerwendung;
        private TextBox txbSpaltenbild;
        private System.Windows.Forms.TabPage tabSchnellformat;
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
    }
}
