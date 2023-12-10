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
        protected override void Dispose(bool disposing) {
            //if (disposing && components != null)
            //{
            //	components?.Dispose();
            //}
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ColumnEditor));
            this.ColorDia = new ColorDialog();
            this.tabAnzeige = new TabPage();
            this.txbFixedColumnWidth = new TextBox();
            this.capFixedColumnWidth = new Caption();
            this.btnSpellChecking = new Button();
            this.cbxTranslate = new ComboBox();
            this.txbBildCodeConstHeight = new TextBox();
            this.capBildCodeConstHeight = new Caption();
            this.capTranslate = new Caption();
            this.txbSpaltenbild = new TextBox();
            this.cbxBildTextVerhalten = new ComboBox();
            this.cbxAlign = new ComboBox();
            this.txbReplacer = new TextBox();
            this.capBildCodeImageNotfound = new Caption();
            this.btnStandard = new Button();
            this.capAlign = new Caption();
            this.txbPrefix = new TextBox();
            this.capPraefix = new Caption();
            this.capUeberschrift3 = new Caption();
            this.capUeberschrift2 = new Caption();
            this.capUeberschrift1 = new Caption();
            this.txbUeberschift3 = new TextBox();
            this.txbUeberschift2 = new TextBox();
            this.txbUeberschift1 = new TextBox();
            this.capReplacer = new Caption();
            this.btnEinzeiligDarstellen = new Button();
            this.capEinheit = new Caption();
            this.cbxEinheit = new ComboBox();
            this.capSpaltenbild = new Caption();
            this.btnBackColor = new Button();
            this.cbxRandRechts = new ComboBox();
            this.btnTextColor = new Button();
            this.cbxRandLinks = new ComboBox();
            this.capLinkerRand = new Caption();
            this.capRechterRand = new Caption();
            this.cbxScriptType = new ComboBox();
            this.capScriptType = new Caption();
            this.btnFormatierungErlaubt = new Button();
            this.cbxAdditionalCheck = new ComboBox();
            this.capcbxAdditionalCheck = new Caption();
            this.cbxFormat = new ComboBox();
            this.capFormat = new Caption();
            this.btnMultiline = new Button();
            this.tabBearbeitung = new TabPage();
            this.grpAuswahlmenuOptionen = new GroupBox();
            this.btnOtherValuesToo = new Button();
            this.txbAuswaehlbareWerte = new TextBox();
            this.capImmerWerte = new Caption();
            this.btnCanBeEmpty = new Button();
            this.btnLogUndo = new Button();
            this.btnIgnoreLock = new Button();
            this.lbxCellEditor = new ListBox();
            this.btnEditableStandard = new Button();
            this.capUserGroupEdit = new Caption();
            this.btnEditableDropdown = new Button();
            this.tabAutoBearbeitung = new TabPage();
            this.btnCalculateMaxCellLenght = new Button();
            this.txbAutoReplace = new TextBox();
            this.capAutoReplace = new Caption();
            this.txbMaxCellLenght = new TextBox();
            this.txbAutoRemove = new TextBox();
            this.capAutoRemove = new Caption();
            this.capMaxCellLenght = new Caption();
            this.txbInitValue = new TextBox();
            this.capInitValue = new Caption();
            this.btnAutoEditKleineFehler = new Button();
            this.btnAutoEditToUpper = new Button();
            this.txbRunden = new TextBox();
            this.capNachkommastellen = new Caption();
            this.btnAutoEditAutoSort = new Button();
            this.tabFilter = new TabPage();
            this.chkFilterOnlyOr = new Button();
            this.chkFilterOnlyAND = new Button();
            this.capJokerValue = new Caption();
            this.txbJoker = new TextBox();
            this.btnZeilenFilterIgnorieren = new Button();
            this.btnAutoFilterMoeglich = new Button();
            this.btnAutoFilterTXTErlaubt = new Button();
            this.btnAutoFilterErweitertErlaubt = new Button();
            this.tabQuickInfo = new TabPage();
            this.txbAdminInfo = new TextBox();
            this.txbQuickinfo = new TextBox();
            this.Caption18 = new Caption();
            this.Caption17 = new Caption();
            this.btnQI_Vorschau = new Button();
            this.tabSonstiges = new TabPage();
            this.txbTags = new TextBox();
            this.Caption8 = new Caption();
            this.cbxSort = new ComboBox();
            this.txbRegex = new TextBox();
            this.capSortiermaske = new Caption();
            this.capRegex = new Caption();
            this.txbAllowedChars = new TextBox();
            this.Caption13 = new Caption();
            this.cbxLinkedDatabase = new ComboBox();
            this.capLinkedDatabase = new Caption();
            this.BlueFrame1 = new GroupBox();
            this.btnVerwendung = new Button();
            this.capInfo = new Caption();
            this.Caption3 = new Caption();
            this.txbName = new TextBox();
            this.Caption2 = new Caption();
            this.txbCaption = new TextBox();
            this.btnOk = new Button();
            this.tabControl = new TabControl();
            this.tabDatenFormat = new TabPage();
            this.btnMaxTextLenght = new Button();
            this.txbMaxTextLenght = new TextBox();
            this.capMaxTextLenght = new Caption();
            this.grpSchnellformat = new GroupBox();
            this.btnSchnellText = new Button();
            this.btnSchnellBit = new Button();
            this.btnSchnellDatum = new Button();
            this.btnSchnellBildCode = new Button();
            this.btnSchnellDatumUhrzeit = new Button();
            this.btnSchnellIInternetAdresse = new Button();
            this.btnSchnellEmail = new Button();
            this.btnSchnellAuswahloptionen = new Button();
            this.btnSchnellTelefonNummer = new Button();
            this.btnSchnellGleitkommazahl = new Button();
            this.btnSchnellGanzzahl = new Button();
            this.tabSpaltenVerlinkung = new TabPage();
            this.tblFilterliste = new Table();
            this.cbxTargetColumn = new ComboBox();
            this.capTargetColumn = new Caption();
            this.caption5 = new Caption();
            this.butAktuellVor = new Button();
            this.butAktuellZurueck = new Button();
            this.capTabellenname = new Caption();
            this.tabAnzeige.SuspendLayout();
            this.tabBearbeitung.SuspendLayout();
            this.grpAuswahlmenuOptionen.SuspendLayout();
            this.tabAutoBearbeitung.SuspendLayout();
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
            this.tabAnzeige.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAnzeige.Controls.Add(this.txbFixedColumnWidth);
            this.tabAnzeige.Controls.Add(this.capFixedColumnWidth);
            this.tabAnzeige.Controls.Add(this.btnSpellChecking);
            this.tabAnzeige.Controls.Add(this.cbxTranslate);
            this.tabAnzeige.Controls.Add(this.txbBildCodeConstHeight);
            this.tabAnzeige.Controls.Add(this.capBildCodeConstHeight);
            this.tabAnzeige.Controls.Add(this.capTranslate);
            this.tabAnzeige.Controls.Add(this.txbSpaltenbild);
            this.tabAnzeige.Controls.Add(this.cbxBildTextVerhalten);
            this.tabAnzeige.Controls.Add(this.cbxAlign);
            this.tabAnzeige.Controls.Add(this.txbReplacer);
            this.tabAnzeige.Controls.Add(this.capBildCodeImageNotfound);
            this.tabAnzeige.Controls.Add(this.btnStandard);
            this.tabAnzeige.Controls.Add(this.capAlign);
            this.tabAnzeige.Controls.Add(this.txbPrefix);
            this.tabAnzeige.Controls.Add(this.capPraefix);
            this.tabAnzeige.Controls.Add(this.capUeberschrift3);
            this.tabAnzeige.Controls.Add(this.capUeberschrift2);
            this.tabAnzeige.Controls.Add(this.capUeberschrift1);
            this.tabAnzeige.Controls.Add(this.txbUeberschift3);
            this.tabAnzeige.Controls.Add(this.txbUeberschift2);
            this.tabAnzeige.Controls.Add(this.txbUeberschift1);
            this.tabAnzeige.Controls.Add(this.capReplacer);
            this.tabAnzeige.Controls.Add(this.btnEinzeiligDarstellen);
            this.tabAnzeige.Controls.Add(this.capEinheit);
            this.tabAnzeige.Controls.Add(this.cbxEinheit);
            this.tabAnzeige.Controls.Add(this.capSpaltenbild);
            this.tabAnzeige.Controls.Add(this.btnBackColor);
            this.tabAnzeige.Controls.Add(this.cbxRandRechts);
            this.tabAnzeige.Controls.Add(this.btnTextColor);
            this.tabAnzeige.Controls.Add(this.cbxRandLinks);
            this.tabAnzeige.Controls.Add(this.capLinkerRand);
            this.tabAnzeige.Controls.Add(this.capRechterRand);
            this.tabAnzeige.Location = new Point(4, 25);
            this.tabAnzeige.Name = "tabAnzeige";
            this.tabAnzeige.Padding = new Padding(3);
            this.tabAnzeige.Size = new Size(993, 483);
            this.tabAnzeige.TabIndex = 0;
            this.tabAnzeige.Text = "Anzeige";
            // 
            // txbFixedColumnWidth
            // 
            this.txbFixedColumnWidth.AllowedChars = "0123456789|";
            this.txbFixedColumnWidth.Cursor = Cursors.IBeam;
            this.txbFixedColumnWidth.Location = new Point(176, 136);
            this.txbFixedColumnWidth.Name = "txbFixedColumnWidth";
            this.txbFixedColumnWidth.QuickInfo = "Wenn ein Wert >0 eingegeben wird, \r\nwird die Spalte immer in dieser Breite angeze" +
    "igt.";
            this.txbFixedColumnWidth.Size = new Size(96, 24);
            this.txbFixedColumnWidth.Suffix = "Pixel";
            this.txbFixedColumnWidth.TabIndex = 42;
            // 
            // capFixedColumnWidth
            // 
            this.capFixedColumnWidth.CausesValidation = false;
            this.capFixedColumnWidth.Location = new Point(8, 136);
            this.capFixedColumnWidth.Name = "capFixedColumnWidth";
            this.capFixedColumnWidth.Size = new Size(160, 16);
            this.capFixedColumnWidth.Text = "Feste Spaltenbreite:";
            // 
            // btnSpellChecking
            // 
            this.btnSpellChecking.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnSpellChecking.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnSpellChecking.Location = new Point(528, 208);
            this.btnSpellChecking.Name = "btnSpellChecking";
            this.btnSpellChecking.Size = new Size(352, 16);
            this.btnSpellChecking.TabIndex = 33;
            this.btnSpellChecking.Text = "Rechtschreibprüfung aktivieren";
            // 
            // cbxTranslate
            // 
            this.cbxTranslate.Cursor = Cursors.IBeam;
            this.cbxTranslate.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxTranslate.Location = new Point(528, 176);
            this.cbxTranslate.Name = "cbxTranslate";
            this.cbxTranslate.Regex = null;
            this.cbxTranslate.Size = new Size(368, 24);
            this.cbxTranslate.TabIndex = 37;
            // 
            // txbBildCodeConstHeight
            // 
            this.txbBildCodeConstHeight.AllowedChars = "0123456789|";
            this.txbBildCodeConstHeight.Cursor = Cursors.IBeam;
            this.txbBildCodeConstHeight.Location = new Point(176, 200);
            this.txbBildCodeConstHeight.Name = "txbBildCodeConstHeight";
            this.txbBildCodeConstHeight.QuickInfo = "Beispieleingabe: 24|16";
            this.txbBildCodeConstHeight.Size = new Size(96, 24);
            this.txbBildCodeConstHeight.Suffix = "Pixel";
            this.txbBildCodeConstHeight.TabIndex = 32;
            // 
            // capBildCodeConstHeight
            // 
            this.capBildCodeConstHeight.CausesValidation = false;
            this.capBildCodeConstHeight.Location = new Point(8, 200);
            this.capBildCodeConstHeight.Name = "capBildCodeConstHeight";
            this.capBildCodeConstHeight.Size = new Size(160, 16);
            this.capBildCodeConstHeight.Text = "Breite/Höhe von Bildern:";
            // 
            // capTranslate
            // 
            this.capTranslate.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.capTranslate.CausesValidation = false;
            this.capTranslate.Location = new Point(528, 160);
            this.capTranslate.Name = "capTranslate";
            this.capTranslate.Size = new Size(152, 24);
            this.capTranslate.Text = "Übersetzen:";
            // 
            // txbSpaltenbild
            // 
            this.txbSpaltenbild.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.txbSpaltenbild.Cursor = Cursors.IBeam;
            this.txbSpaltenbild.Location = new Point(528, 104);
            this.txbSpaltenbild.Name = "txbSpaltenbild";
            this.txbSpaltenbild.Regex = null;
            this.txbSpaltenbild.Size = new Size(465, 24);
            this.txbSpaltenbild.TabIndex = 40;
            // 
            // cbxBildTextVerhalten
            // 
            this.cbxBildTextVerhalten.Cursor = Cursors.IBeam;
            this.cbxBildTextVerhalten.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxBildTextVerhalten.Location = new Point(176, 176);
            this.cbxBildTextVerhalten.Name = "cbxBildTextVerhalten";
            this.cbxBildTextVerhalten.Regex = null;
            this.cbxBildTextVerhalten.Size = new Size(336, 24);
            this.cbxBildTextVerhalten.TabIndex = 34;
            // 
            // cbxAlign
            // 
            this.cbxAlign.Cursor = Cursors.IBeam;
            this.cbxAlign.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxAlign.Location = new Point(384, 136);
            this.cbxAlign.Name = "cbxAlign";
            this.cbxAlign.Regex = null;
            this.cbxAlign.Size = new Size(128, 24);
            this.cbxAlign.TabIndex = 7;
            // 
            // txbReplacer
            // 
            this.txbReplacer.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                        | AnchorStyles.Left) 
                                                       | AnchorStyles.Right)));
            this.txbReplacer.Cursor = Cursors.IBeam;
            this.txbReplacer.Location = new Point(8, 264);
            this.txbReplacer.MultiLine = true;
            this.txbReplacer.Name = "txbReplacer";
            this.txbReplacer.QuickInfo = "Texte in der Spalte werden mit diesen Angaben <b>optisch</b> ersetzt.<br><i><u>Be" +
    "ispiel:</i></u>Beispiel-Text|Bsp.-Txt";
            this.txbReplacer.Regex = null;
            this.txbReplacer.Size = new Size(985, 217);
            this.txbReplacer.SpellCheckingEnabled = true;
            this.txbReplacer.TabIndex = 35;
            // 
            // capBildCodeImageNotfound
            // 
            this.capBildCodeImageNotfound.CausesValidation = false;
            this.capBildCodeImageNotfound.Location = new Point(8, 176);
            this.capBildCodeImageNotfound.Name = "capBildCodeImageNotfound";
            this.capBildCodeImageNotfound.Size = new Size(136, 16);
            this.capBildCodeImageNotfound.Text = "Bild/Text-Verhalten:";
            // 
            // btnStandard
            // 
            this.btnStandard.Location = new Point(384, 16);
            this.btnStandard.Name = "btnStandard";
            this.btnStandard.Size = new Size(128, 48);
            this.btnStandard.TabIndex = 39;
            this.btnStandard.Text = "Standard herstellen";
            this.btnStandard.Click += new EventHandler(this.btnStandard_Click);
            // 
            // capAlign
            // 
            this.capAlign.CausesValidation = false;
            this.capAlign.Location = new Point(384, 120);
            this.capAlign.Name = "capAlign";
            this.capAlign.Size = new Size(104, 16);
            this.capAlign.Text = "Ausrichtung:";
            // 
            // txbPrefix
            // 
            this.txbPrefix.Cursor = Cursors.IBeam;
            this.txbPrefix.Location = new Point(56, 48);
            this.txbPrefix.Name = "txbPrefix";
            this.txbPrefix.Regex = null;
            this.txbPrefix.Size = new Size(168, 24);
            this.txbPrefix.TabIndex = 36;
            // 
            // capPraefix
            // 
            this.capPraefix.CausesValidation = false;
            this.capPraefix.Location = new Point(8, 48);
            this.capPraefix.Name = "capPraefix";
            this.capPraefix.Size = new Size(48, 16);
            this.capPraefix.Text = "Präfix:";
            // 
            // capUeberschrift3
            // 
            this.capUeberschrift3.CausesValidation = false;
            this.capUeberschrift3.Location = new Point(528, 56);
            this.capUeberschrift3.Name = "capUeberschrift3";
            this.capUeberschrift3.Size = new Size(88, 16);
            this.capUeberschrift3.Text = "Überschrift 3:";
            this.capUeberschrift3.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift2
            // 
            this.capUeberschrift2.CausesValidation = false;
            this.capUeberschrift2.Location = new Point(528, 32);
            this.capUeberschrift2.Name = "capUeberschrift2";
            this.capUeberschrift2.Size = new Size(88, 16);
            this.capUeberschrift2.Text = "Überschrift 2:";
            this.capUeberschrift2.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capUeberschrift1
            // 
            this.capUeberschrift1.CausesValidation = false;
            this.capUeberschrift1.Location = new Point(528, 8);
            this.capUeberschrift1.Name = "capUeberschrift1";
            this.capUeberschrift1.Size = new Size(88, 16);
            this.capUeberschrift1.Text = "Überschrift 1:";
            this.capUeberschrift1.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbUeberschift3
            // 
            this.txbUeberschift3.Cursor = Cursors.IBeam;
            this.txbUeberschift3.Location = new Point(624, 56);
            this.txbUeberschift3.Name = "txbUeberschift3";
            this.txbUeberschift3.Regex = null;
            this.txbUeberschift3.Size = new Size(288, 24);
            this.txbUeberschift3.TabIndex = 38;
            // 
            // txbUeberschift2
            // 
            this.txbUeberschift2.Cursor = Cursors.IBeam;
            this.txbUeberschift2.Location = new Point(624, 32);
            this.txbUeberschift2.Name = "txbUeberschift2";
            this.txbUeberschift2.Regex = null;
            this.txbUeberschift2.Size = new Size(288, 24);
            this.txbUeberschift2.TabIndex = 37;
            // 
            // txbUeberschift1
            // 
            this.txbUeberschift1.Cursor = Cursors.IBeam;
            this.txbUeberschift1.Location = new Point(624, 8);
            this.txbUeberschift1.Name = "txbUeberschift1";
            this.txbUeberschift1.Regex = null;
            this.txbUeberschift1.Size = new Size(288, 24);
            this.txbUeberschift1.TabIndex = 36;
            // 
            // capReplacer
            // 
            this.capReplacer.CausesValidation = false;
            this.capReplacer.Location = new Point(8, 240);
            this.capReplacer.Name = "capReplacer";
            this.capReplacer.Size = new Size(144, 24);
            this.capReplacer.Text = "Optische Ersetzungen:";
            // 
            // btnEinzeiligDarstellen
            // 
            this.btnEinzeiligDarstellen.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnEinzeiligDarstellen.Location = new Point(8, 88);
            this.btnEinzeiligDarstellen.Name = "btnEinzeiligDarstellen";
            this.btnEinzeiligDarstellen.Size = new Size(216, 16);
            this.btnEinzeiligDarstellen.TabIndex = 29;
            this.btnEinzeiligDarstellen.Text = "Mehrzeilig einzeilig darstellen";
            // 
            // capEinheit
            // 
            this.capEinheit.CausesValidation = false;
            this.capEinheit.Location = new Point(8, 8);
            this.capEinheit.Name = "capEinheit";
            this.capEinheit.Size = new Size(48, 32);
            this.capEinheit.Text = "Einheit:<br>(Suffix)";
            // 
            // cbxEinheit
            // 
            this.cbxEinheit.Cursor = Cursors.IBeam;
            this.cbxEinheit.Location = new Point(56, 8);
            this.cbxEinheit.Name = "cbxEinheit";
            this.cbxEinheit.Regex = null;
            this.cbxEinheit.Size = new Size(168, 24);
            this.cbxEinheit.TabIndex = 31;
            // 
            // capSpaltenbild
            // 
            this.capSpaltenbild.CausesValidation = false;
            this.capSpaltenbild.Location = new Point(528, 88);
            this.capSpaltenbild.Name = "capSpaltenbild";
            this.capSpaltenbild.Size = new Size(152, 24);
            this.capSpaltenbild.Text = "Spaltenbild:";
            this.capSpaltenbild.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnBackColor
            // 
            this.btnBackColor.Location = new Point(248, 40);
            this.btnBackColor.Name = "btnBackColor";
            this.btnBackColor.Size = new Size(128, 24);
            this.btnBackColor.TabIndex = 3;
            this.btnBackColor.Text = "Hintergrundfarbe";
            this.btnBackColor.Click += new EventHandler(this.btnBackColor_Click);
            // 
            // cbxRandRechts
            // 
            this.cbxRandRechts.Cursor = Cursors.IBeam;
            this.cbxRandRechts.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxRandRechts.Location = new Point(384, 88);
            this.cbxRandRechts.Name = "cbxRandRechts";
            this.cbxRandRechts.Regex = null;
            this.cbxRandRechts.Size = new Size(128, 24);
            this.cbxRandRechts.TabIndex = 25;
            // 
            // btnTextColor
            // 
            this.btnTextColor.Location = new Point(248, 16);
            this.btnTextColor.Name = "btnTextColor";
            this.btnTextColor.Size = new Size(128, 24);
            this.btnTextColor.TabIndex = 4;
            this.btnTextColor.Text = "Textfarbe";
            this.btnTextColor.Click += new EventHandler(this.btnTextColor_Click);
            // 
            // cbxRandLinks
            // 
            this.cbxRandLinks.Cursor = Cursors.IBeam;
            this.cbxRandLinks.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxRandLinks.Location = new Point(248, 88);
            this.cbxRandLinks.Name = "cbxRandLinks";
            this.cbxRandLinks.Regex = null;
            this.cbxRandLinks.Size = new Size(128, 24);
            this.cbxRandLinks.TabIndex = 24;
            // 
            // capLinkerRand
            // 
            this.capLinkerRand.CausesValidation = false;
            this.capLinkerRand.Location = new Point(248, 72);
            this.capLinkerRand.Name = "capLinkerRand";
            this.capLinkerRand.Size = new Size(80, 16);
            this.capLinkerRand.Text = "Linker Rand:";
            this.capLinkerRand.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRechterRand
            // 
            this.capRechterRand.CausesValidation = false;
            this.capRechterRand.Location = new Point(384, 72);
            this.capRechterRand.Name = "capRechterRand";
            this.capRechterRand.Size = new Size(88, 16);
            this.capRechterRand.Text = "Rechter Rand:";
            this.capRechterRand.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxScriptType
            // 
            this.cbxScriptType.Cursor = Cursors.IBeam;
            this.cbxScriptType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxScriptType.Location = new Point(232, 272);
            this.cbxScriptType.Name = "cbxScriptType";
            this.cbxScriptType.Regex = null;
            this.cbxScriptType.Size = new Size(304, 24);
            this.cbxScriptType.TabIndex = 43;
            // 
            // capScriptType
            // 
            this.capScriptType.CausesValidation = false;
            this.capScriptType.Location = new Point(8, 272);
            this.capScriptType.Name = "capScriptType";
            this.capScriptType.Size = new Size(216, 24);
            this.capScriptType.Text = "Im Skript ist der Datentyp:";
            // 
            // btnFormatierungErlaubt
            // 
            this.btnFormatierungErlaubt.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnFormatierungErlaubt.Location = new Point(360, 8);
            this.btnFormatierungErlaubt.Name = "btnFormatierungErlaubt";
            this.btnFormatierungErlaubt.Size = new Size(296, 16);
            this.btnFormatierungErlaubt.TabIndex = 41;
            this.btnFormatierungErlaubt.Text = "Text-Formatierung erlaubt (Fett, Kursiv, etc.)";
            // 
            // cbxAdditionalCheck
            // 
            this.cbxAdditionalCheck.Cursor = Cursors.IBeam;
            this.cbxAdditionalCheck.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxAdditionalCheck.Location = new Point(232, 232);
            this.cbxAdditionalCheck.Name = "cbxAdditionalCheck";
            this.cbxAdditionalCheck.Regex = null;
            this.cbxAdditionalCheck.Size = new Size(304, 24);
            this.cbxAdditionalCheck.TabIndex = 34;
            // 
            // capcbxAdditionalCheck
            // 
            this.capcbxAdditionalCheck.CausesValidation = false;
            this.capcbxAdditionalCheck.Location = new Point(8, 232);
            this.capcbxAdditionalCheck.Name = "capcbxAdditionalCheck";
            this.capcbxAdditionalCheck.Size = new Size(216, 40);
            this.capcbxAdditionalCheck.Text = "Zusätzliche Prüfung, ob der eingegeben Wert konsitent ist zu:";
            this.capcbxAdditionalCheck.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxFormat
            // 
            this.cbxFormat.Cursor = Cursors.IBeam;
            this.cbxFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxFormat.Location = new Point(8, 32);
            this.cbxFormat.Name = "cbxFormat";
            this.cbxFormat.Regex = null;
            this.cbxFormat.Size = new Size(312, 24);
            this.cbxFormat.TabIndex = 27;
            this.cbxFormat.TextChanged += new EventHandler(this.cbxFormat_TextChanged);
            // 
            // capFormat
            // 
            this.capFormat.CausesValidation = false;
            this.capFormat.Location = new Point(8, 16);
            this.capFormat.Name = "capFormat";
            this.capFormat.Size = new Size(136, 16);
            this.capFormat.Text = "Funktion:";
            this.capFormat.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnMultiline
            // 
            this.btnMultiline.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnMultiline.Location = new Point(360, 32);
            this.btnMultiline.Name = "btnMultiline";
            this.btnMultiline.Size = new Size(296, 16);
            this.btnMultiline.TabIndex = 7;
            this.btnMultiline.Text = "Mehrere Einträge pro Zelle erlaubt (mehrzeilig)";
            // 
            // tabBearbeitung
            // 
            this.tabBearbeitung.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBearbeitung.Controls.Add(this.grpAuswahlmenuOptionen);
            this.tabBearbeitung.Controls.Add(this.btnLogUndo);
            this.tabBearbeitung.Controls.Add(this.btnIgnoreLock);
            this.tabBearbeitung.Controls.Add(this.lbxCellEditor);
            this.tabBearbeitung.Controls.Add(this.btnEditableStandard);
            this.tabBearbeitung.Controls.Add(this.capUserGroupEdit);
            this.tabBearbeitung.Controls.Add(this.btnEditableDropdown);
            this.tabBearbeitung.Location = new Point(4, 25);
            this.tabBearbeitung.Name = "tabBearbeitung";
            this.tabBearbeitung.Padding = new Padding(3);
            this.tabBearbeitung.Size = new Size(993, 483);
            this.tabBearbeitung.TabIndex = 1;
            this.tabBearbeitung.Text = "Bearbeitung";
            // 
            // grpAuswahlmenuOptionen
            // 
            this.grpAuswahlmenuOptionen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuswahlmenuOptionen.CausesValidation = false;
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnOtherValuesToo);
            this.grpAuswahlmenuOptionen.Controls.Add(this.txbAuswaehlbareWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.capImmerWerte);
            this.grpAuswahlmenuOptionen.Controls.Add(this.btnCanBeEmpty);
            this.grpAuswahlmenuOptionen.Location = new Point(32, 80);
            this.grpAuswahlmenuOptionen.Name = "grpAuswahlmenuOptionen";
            this.grpAuswahlmenuOptionen.Size = new Size(536, 392);
            this.grpAuswahlmenuOptionen.TabIndex = 0;
            this.grpAuswahlmenuOptionen.TabStop = false;
            this.grpAuswahlmenuOptionen.Text = "Auswahlmenü-Optionen:";
            // 
            // btnOtherValuesToo
            // 
            this.btnOtherValuesToo.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
                                                             | AnchorStyles.Right)));
            this.btnOtherValuesToo.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnOtherValuesToo.Location = new Point(8, 352);
            this.btnOtherValuesToo.Name = "btnOtherValuesToo";
            this.btnOtherValuesToo.Size = new Size(512, 32);
            this.btnOtherValuesToo.TabIndex = 7;
            this.btnOtherValuesToo.Text = "Auch Werte, die in anderen Zellen derselben Spalte vorhanden sind, werden zum Aus" +
    "wählen vorgeschlagen";
            // 
            // txbAuswaehlbareWerte
            // 
            this.txbAuswaehlbareWerte.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                                 | AnchorStyles.Left) 
                                                                | AnchorStyles.Right)));
            this.txbAuswaehlbareWerte.Cursor = Cursors.IBeam;
            this.txbAuswaehlbareWerte.Location = new Point(8, 64);
            this.txbAuswaehlbareWerte.MultiLine = true;
            this.txbAuswaehlbareWerte.Name = "txbAuswaehlbareWerte";
            this.txbAuswaehlbareWerte.Regex = null;
            this.txbAuswaehlbareWerte.Size = new Size(520, 280);
            this.txbAuswaehlbareWerte.SpellCheckingEnabled = true;
            this.txbAuswaehlbareWerte.TabIndex = 0;
            // 
            // capImmerWerte
            // 
            this.capImmerWerte.CausesValidation = false;
            this.capImmerWerte.Location = new Point(8, 40);
            this.capImmerWerte.Name = "capImmerWerte";
            this.capImmerWerte.Size = new Size(440, 24);
            this.capImmerWerte.Text = "<b><u>Immer auswählbare Werte:";
            this.capImmerWerte.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnCanBeEmpty
            // 
            this.btnCanBeEmpty.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnCanBeEmpty.Location = new Point(8, 24);
            this.btnCanBeEmpty.Name = "btnCanBeEmpty";
            this.btnCanBeEmpty.Size = new Size(328, 16);
            this.btnCanBeEmpty.TabIndex = 6;
            this.btnCanBeEmpty.Text = "Alles abwählen erlaubt (leere Zelle möglich)";
            // 
            // btnLogUndo
            // 
            this.btnLogUndo.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnLogUndo.Location = new Point(592, 376);
            this.btnLogUndo.Name = "btnLogUndo";
            this.btnLogUndo.Size = new Size(288, 16);
            this.btnLogUndo.TabIndex = 32;
            this.btnLogUndo.Text = "Undo der Spalte wird geloggt";
            // 
            // btnIgnoreLock
            // 
            this.btnIgnoreLock.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnIgnoreLock.Location = new Point(592, 328);
            this.btnIgnoreLock.Name = "btnIgnoreLock";
            this.btnIgnoreLock.Size = new Size(288, 32);
            this.btnIgnoreLock.TabIndex = 27;
            this.btnIgnoreLock.Text = "Die Bearbeitung ist auch möglich, wenn die Zeile abgeschlossen ist.";
            // 
            // lbxCellEditor
            // 
            this.lbxCellEditor.AddAllowed = AddType.Text;
            this.lbxCellEditor.CheckBehavior = CheckBehavior.MultiSelection;
            this.lbxCellEditor.FilterAllowed = true;
            this.lbxCellEditor.Location = new Point(576, 48);
            this.lbxCellEditor.Name = "lbxCellEditor";
            this.lbxCellEditor.RemoveAllowed = true;
            this.lbxCellEditor.Size = new Size(328, 272);
            this.lbxCellEditor.TabIndex = 26;
            // 
            // btnEditableStandard
            // 
            this.btnEditableStandard.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnEditableStandard.Location = new Point(8, 16);
            this.btnEditableStandard.Name = "btnEditableStandard";
            this.btnEditableStandard.Size = new Size(544, 32);
            this.btnEditableStandard.TabIndex = 4;
            this.btnEditableStandard.Text = "Benutzer-Bearbeitung mit der <b>Standard-Methode</b> erlauben<br><i>Wenn neue Wer" +
    "te erlaubt sein sollen, muss hier ein Häkchen gesetzt werden.";
            // 
            // capUserGroupEdit
            // 
            this.capUserGroupEdit.CausesValidation = false;
            this.capUserGroupEdit.Location = new Point(576, 16);
            this.capUserGroupEdit.Name = "capUserGroupEdit";
            this.capUserGroupEdit.Size = new Size(328, 32);
            this.capUserGroupEdit.Text = "<b>Folgende Benutzergruppen dürfen den Inhalt der Zellen bearbeiten:";
            this.capUserGroupEdit.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnEditableDropdown
            // 
            this.btnEditableDropdown.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnEditableDropdown.Location = new Point(8, 56);
            this.btnEditableDropdown.Name = "btnEditableDropdown";
            this.btnEditableDropdown.Size = new Size(544, 16);
            this.btnEditableDropdown.TabIndex = 5;
            this.btnEditableDropdown.Text = "Benutzer-Bearbeitung mit <b>Auswahl-Menü (Dropdown-Menü)</b> erlauben";
            // 
            // tabAutoBearbeitung
            // 
            this.tabAutoBearbeitung.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAutoBearbeitung.Controls.Add(this.btnCalculateMaxCellLenght);
            this.tabAutoBearbeitung.Controls.Add(this.txbAutoReplace);
            this.tabAutoBearbeitung.Controls.Add(this.capAutoReplace);
            this.tabAutoBearbeitung.Controls.Add(this.txbMaxCellLenght);
            this.tabAutoBearbeitung.Controls.Add(this.txbAutoRemove);
            this.tabAutoBearbeitung.Controls.Add(this.capAutoRemove);
            this.tabAutoBearbeitung.Controls.Add(this.capMaxCellLenght);
            this.tabAutoBearbeitung.Controls.Add(this.txbInitValue);
            this.tabAutoBearbeitung.Controls.Add(this.capInitValue);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditKleineFehler);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditToUpper);
            this.tabAutoBearbeitung.Controls.Add(this.txbRunden);
            this.tabAutoBearbeitung.Controls.Add(this.capNachkommastellen);
            this.tabAutoBearbeitung.Controls.Add(this.btnAutoEditAutoSort);
            this.tabAutoBearbeitung.Location = new Point(4, 25);
            this.tabAutoBearbeitung.Name = "tabAutoBearbeitung";
            this.tabAutoBearbeitung.Size = new Size(993, 483);
            this.tabAutoBearbeitung.TabIndex = 6;
            this.tabAutoBearbeitung.Text = "Auto-Bearbeitung";
            // 
            // btnCalculateMaxCellLenght
            // 
            this.btnCalculateMaxCellLenght.ImageCode = "Taschenrechner|16";
            this.btnCalculateMaxCellLenght.Location = new Point(264, 136);
            this.btnCalculateMaxCellLenght.Name = "btnCalculateMaxCellLenght";
            this.btnCalculateMaxCellLenght.QuickInfo = "Prüft alle Zellen und berechnet die ideale\r\nmaximale Text Länge";
            this.btnCalculateMaxCellLenght.Size = new Size(40, 24);
            this.btnCalculateMaxCellLenght.TabIndex = 46;
            this.btnCalculateMaxCellLenght.Click += new EventHandler(this.btnCalculateMaxCellLenght_Click);
            // 
            // txbAutoReplace
            // 
            this.txbAutoReplace.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                           | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.txbAutoReplace.Cursor = Cursors.IBeam;
            this.txbAutoReplace.Location = new Point(8, 288);
            this.txbAutoReplace.MultiLine = true;
            this.txbAutoReplace.Name = "txbAutoReplace";
            this.txbAutoReplace.QuickInfo = resources.GetString("txbAutoReplace.QuickInfo");
            this.txbAutoReplace.Regex = null;
            this.txbAutoReplace.Size = new Size(985, 188);
            this.txbAutoReplace.SpellCheckingEnabled = true;
            this.txbAutoReplace.TabIndex = 39;
            // 
            // capAutoReplace
            // 
            this.capAutoReplace.CausesValidation = false;
            this.capAutoReplace.Location = new Point(8, 272);
            this.capAutoReplace.Name = "capAutoReplace";
            this.capAutoReplace.Size = new Size(184, 24);
            this.capAutoReplace.Text = "Permanente Ersetzungen:";
            // 
            // txbMaxCellLenght
            // 
            this.txbMaxCellLenght.AdditionalFormatCheck = AdditionalCheck.Integer;
            this.txbMaxCellLenght.AllowedChars = "0123456789";
            this.txbMaxCellLenght.Cursor = Cursors.IBeam;
            this.txbMaxCellLenght.Location = new Point(168, 136);
            this.txbMaxCellLenght.MaxTextLenght = 255;
            this.txbMaxCellLenght.Name = "txbMaxCellLenght";
            this.txbMaxCellLenght.QuickInfo = resources.GetString("txbMaxCellLenght.QuickInfo");
            this.txbMaxCellLenght.Regex = "^((-?[1-9]\\d*)|0)$";
            this.txbMaxCellLenght.Size = new Size(96, 24);
            this.txbMaxCellLenght.TabIndex = 45;
            // 
            // txbAutoRemove
            // 
            this.txbAutoRemove.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                         | AnchorStyles.Right)));
            this.txbAutoRemove.Cursor = Cursors.IBeam;
            this.txbAutoRemove.Location = new Point(8, 240);
            this.txbAutoRemove.Name = "txbAutoRemove";
            this.txbAutoRemove.Regex = null;
            this.txbAutoRemove.Size = new Size(977, 24);
            this.txbAutoRemove.TabIndex = 37;
            // 
            // capAutoRemove
            // 
            this.capAutoRemove.CausesValidation = false;
            this.capAutoRemove.Location = new Point(8, 224);
            this.capAutoRemove.Name = "capAutoRemove";
            this.capAutoRemove.Size = new Size(568, 16);
            this.capAutoRemove.Text = "Folgende Zeichen automatisch aus der Eingabe löschen:";
            // 
            // capMaxCellLenght
            // 
            this.capMaxCellLenght.CausesValidation = false;
            this.capMaxCellLenght.Location = new Point(8, 136);
            this.capMaxCellLenght.Name = "capMaxCellLenght";
            this.capMaxCellLenght.QuickInfo = "Falls mehrere Zeilen erlaubt sind, pro Zeile.\r\nAber es sind niemals mehr als 4000" +
    " Zeichen erlaubt.\r\nDa im UTF8-Format gespeichert wird, evtl. auch weniger.";
            this.capMaxCellLenght.Size = new Size(160, 24);
            this.capMaxCellLenght.Text = "Maximale Zellen-Kapazität:";
            this.capMaxCellLenght.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbInitValue
            // 
            this.txbInitValue.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                        | AnchorStyles.Right)));
            this.txbInitValue.Cursor = Cursors.IBeam;
            this.txbInitValue.Location = new Point(8, 32);
            this.txbInitValue.Name = "txbInitValue";
            this.txbInitValue.Regex = null;
            this.txbInitValue.Size = new Size(977, 24);
            this.txbInitValue.TabIndex = 15;
            // 
            // capInitValue
            // 
            this.capInitValue.CausesValidation = false;
            this.capInitValue.Location = new Point(8, 16);
            this.capInitValue.Name = "capInitValue";
            this.capInitValue.Size = new Size(568, 16);
            this.capInitValue.Text = "Wenn eine neue Zeile erstellt wird, folgenden Wert in die Zelle schreiben:";
            // 
            // btnAutoEditKleineFehler
            // 
            this.btnAutoEditKleineFehler.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnAutoEditKleineFehler.Location = new Point(432, 64);
            this.btnAutoEditKleineFehler.Name = "btnAutoEditKleineFehler";
            this.btnAutoEditKleineFehler.Size = new Size(440, 24);
            this.btnAutoEditKleineFehler.TabIndex = 13;
            this.btnAutoEditKleineFehler.Text = "Kleinere Fehler, wie z.B. doppelte Leerzeichen automatisch korrigieren";
            // 
            // btnAutoEditToUpper
            // 
            this.btnAutoEditToUpper.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnAutoEditToUpper.Location = new Point(8, 64);
            this.btnAutoEditToUpper.Name = "btnAutoEditToUpper";
            this.btnAutoEditToUpper.Size = new Size(416, 24);
            this.btnAutoEditToUpper.TabIndex = 12;
            this.btnAutoEditToUpper.Text = "Texte in Grossbuchstaben ändern";
            // 
            // txbRunden
            // 
            this.txbRunden.Cursor = Cursors.IBeam;
            this.txbRunden.Location = new Point(224, 96);
            this.txbRunden.Name = "txbRunden";
            this.txbRunden.Regex = null;
            this.txbRunden.Size = new Size(88, 24);
            this.txbRunden.TabIndex = 11;
            // 
            // capNachkommastellen
            // 
            this.capNachkommastellen.CausesValidation = false;
            this.capNachkommastellen.Location = new Point(8, 96);
            this.capNachkommastellen.Name = "capNachkommastellen";
            this.capNachkommastellen.Size = new Size(328, 16);
            this.capNachkommastellen.Text = "Zahlen runden auf Kommastellen:";
            // 
            // btnAutoEditAutoSort
            // 
            this.btnAutoEditAutoSort.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnAutoEditAutoSort.Location = new Point(432, 88);
            this.btnAutoEditAutoSort.Name = "btnAutoEditAutoSort";
            this.btnAutoEditAutoSort.Size = new Size(416, 24);
            this.btnAutoEditAutoSort.TabIndex = 10;
            this.btnAutoEditAutoSort.Text = "Mehrzeilige Einträge sortieren und doppelte entfernen";
            // 
            // tabFilter
            // 
            this.tabFilter.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabFilter.Controls.Add(this.chkFilterOnlyOr);
            this.tabFilter.Controls.Add(this.chkFilterOnlyAND);
            this.tabFilter.Controls.Add(this.capJokerValue);
            this.tabFilter.Controls.Add(this.txbJoker);
            this.tabFilter.Controls.Add(this.btnZeilenFilterIgnorieren);
            this.tabFilter.Controls.Add(this.btnAutoFilterMoeglich);
            this.tabFilter.Controls.Add(this.btnAutoFilterTXTErlaubt);
            this.tabFilter.Controls.Add(this.btnAutoFilterErweitertErlaubt);
            this.tabFilter.Location = new Point(4, 25);
            this.tabFilter.Name = "tabFilter";
            this.tabFilter.Padding = new Padding(3);
            this.tabFilter.Size = new Size(993, 483);
            this.tabFilter.TabIndex = 2;
            this.tabFilter.Text = "Filter";
            // 
            // chkFilterOnlyOr
            // 
            this.chkFilterOnlyOr.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkFilterOnlyOr.Location = new Point(32, 88);
            this.chkFilterOnlyOr.Name = "chkFilterOnlyOr";
            this.chkFilterOnlyOr.Size = new Size(192, 16);
            this.chkFilterOnlyOr.TabIndex = 35;
            this.chkFilterOnlyOr.Text = "nur <b>ODER</b>-Filterung erlauben";
            // 
            // chkFilterOnlyAND
            // 
            this.chkFilterOnlyAND.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkFilterOnlyAND.Location = new Point(32, 72);
            this.chkFilterOnlyAND.Name = "chkFilterOnlyAND";
            this.chkFilterOnlyAND.Size = new Size(192, 16);
            this.chkFilterOnlyAND.TabIndex = 34;
            this.chkFilterOnlyAND.Text = "nur <b>UND</b>-Filterung erlauben";
            // 
            // capJokerValue
            // 
            this.capJokerValue.CausesValidation = false;
            this.capJokerValue.Location = new Point(4, 177);
            this.capJokerValue.Name = "capJokerValue";
            this.capJokerValue.Size = new Size(312, 56);
            this.capJokerValue.Text = "Bei Autofilter-Aktionen, Zellen mit folgenden Inhalt <b>immer</b> anzeigen, wenn " +
    "ein Wert gewählt wurde:<br>(Joker)";
            this.capJokerValue.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbJoker
            // 
            this.txbJoker.Cursor = Cursors.IBeam;
            this.txbJoker.Location = new Point(4, 233);
            this.txbJoker.Name = "txbJoker";
            this.txbJoker.Regex = null;
            this.txbJoker.Size = new Size(312, 24);
            this.txbJoker.TabIndex = 7;
            // 
            // btnZeilenFilterIgnorieren
            // 
            this.btnZeilenFilterIgnorieren.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnZeilenFilterIgnorieren.Location = new Point(8, 144);
            this.btnZeilenFilterIgnorieren.Name = "btnZeilenFilterIgnorieren";
            this.btnZeilenFilterIgnorieren.Size = new Size(304, 16);
            this.btnZeilenFilterIgnorieren.TabIndex = 6;
            this.btnZeilenFilterIgnorieren.Text = "Bei Zeilenfilter ignorieren (Suchfeld-Eingabe)";
            // 
            // btnAutoFilterMoeglich
            // 
            this.btnAutoFilterMoeglich.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnAutoFilterMoeglich.Location = new Point(12, 15);
            this.btnAutoFilterMoeglich.Name = "btnAutoFilterMoeglich";
            this.btnAutoFilterMoeglich.Size = new Size(120, 16);
            this.btnAutoFilterMoeglich.TabIndex = 3;
            this.btnAutoFilterMoeglich.Text = "AutoFilter erlaubt";
            // 
            // btnAutoFilterTXTErlaubt
            // 
            this.btnAutoFilterTXTErlaubt.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnAutoFilterTXTErlaubt.Location = new Point(32, 32);
            this.btnAutoFilterTXTErlaubt.Name = "btnAutoFilterTXTErlaubt";
            this.btnAutoFilterTXTErlaubt.Size = new Size(208, 16);
            this.btnAutoFilterTXTErlaubt.TabIndex = 4;
            this.btnAutoFilterTXTErlaubt.Text = "AutoFilter - Texteingabe - erlaubt";
            // 
            // btnAutoFilterErweitertErlaubt
            // 
            this.btnAutoFilterErweitertErlaubt.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.btnAutoFilterErweitertErlaubt.Location = new Point(32, 48);
            this.btnAutoFilterErweitertErlaubt.Name = "btnAutoFilterErweitertErlaubt";
            this.btnAutoFilterErweitertErlaubt.Size = new Size(192, 16);
            this.btnAutoFilterErweitertErlaubt.TabIndex = 5;
            this.btnAutoFilterErweitertErlaubt.Text = "AutoFilter - Erweitert - erlaubt";
            // 
            // tabQuickInfo
            // 
            this.tabQuickInfo.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabQuickInfo.Controls.Add(this.txbAdminInfo);
            this.tabQuickInfo.Controls.Add(this.txbQuickinfo);
            this.tabQuickInfo.Controls.Add(this.Caption18);
            this.tabQuickInfo.Controls.Add(this.Caption17);
            this.tabQuickInfo.Controls.Add(this.btnQI_Vorschau);
            this.tabQuickInfo.Location = new Point(4, 25);
            this.tabQuickInfo.Name = "tabQuickInfo";
            this.tabQuickInfo.Padding = new Padding(3);
            this.tabQuickInfo.Size = new Size(993, 483);
            this.tabQuickInfo.TabIndex = 3;
            this.tabQuickInfo.Text = "Quickinfo";
            // 
            // txbAdminInfo
            // 
            this.txbAdminInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                        | AnchorStyles.Right)));
            this.txbAdminInfo.Cursor = Cursors.IBeam;
            this.txbAdminInfo.Location = new Point(512, 24);
            this.txbAdminInfo.MultiLine = true;
            this.txbAdminInfo.Name = "txbAdminInfo";
            this.txbAdminInfo.Regex = null;
            this.txbAdminInfo.Size = new Size(473, 424);
            this.txbAdminInfo.SpellCheckingEnabled = true;
            this.txbAdminInfo.TabIndex = 3;
            this.txbAdminInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbQuickinfo
            // 
            this.txbQuickinfo.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                         | AnchorStyles.Left) 
                                                        | AnchorStyles.Right)));
            this.txbQuickinfo.Cursor = Cursors.IBeam;
            this.txbQuickinfo.Location = new Point(8, 24);
            this.txbQuickinfo.MultiLine = true;
            this.txbQuickinfo.Name = "txbQuickinfo";
            this.txbQuickinfo.Regex = null;
            this.txbQuickinfo.Size = new Size(496, 424);
            this.txbQuickinfo.SpellCheckingEnabled = true;
            this.txbQuickinfo.TabIndex = 0;
            this.txbQuickinfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption18
            // 
            this.Caption18.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.Caption18.CausesValidation = false;
            this.Caption18.Location = new Point(512, 8);
            this.Caption18.Name = "Caption18";
            this.Caption18.Size = new Size(188, 15);
            this.Caption18.Text = "Administrator-Info:";
            this.Caption18.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption17
            // 
            this.Caption17.CausesValidation = false;
            this.Caption17.Location = new Point(8, 8);
            this.Caption17.Name = "Caption17";
            this.Caption17.Size = new Size(168, 16);
            this.Caption17.Text = "QuickInfo:";
            this.Caption17.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnQI_Vorschau
            // 
            this.btnQI_Vorschau.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnQI_Vorschau.Location = new Point(889, 456);
            this.btnQI_Vorschau.Name = "btnQI_Vorschau";
            this.btnQI_Vorschau.Size = new Size(96, 24);
            this.btnQI_Vorschau.TabIndex = 1;
            this.btnQI_Vorschau.Text = "Vorschau";
            this.btnQI_Vorschau.Click += new EventHandler(this.btnQI_Vorschau_Click);
            // 
            // tabSonstiges
            // 
            this.tabSonstiges.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSonstiges.Controls.Add(this.txbTags);
            this.tabSonstiges.Controls.Add(this.Caption8);
            this.tabSonstiges.Location = new Point(4, 25);
            this.tabSonstiges.Name = "tabSonstiges";
            this.tabSonstiges.Padding = new Padding(3);
            this.tabSonstiges.Size = new Size(993, 483);
            this.tabSonstiges.TabIndex = 4;
            this.tabSonstiges.Text = "Sonstiges allgemein";
            // 
            // txbTags
            // 
            this.txbTags.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                    | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txbTags.Cursor = Cursors.IBeam;
            this.txbTags.Location = new Point(4, 31);
            this.txbTags.MultiLine = true;
            this.txbTags.Name = "txbTags";
            this.txbTags.Regex = null;
            this.txbTags.Size = new Size(980, 357);
            this.txbTags.TabIndex = 30;
            this.txbTags.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption8
            // 
            this.Caption8.CausesValidation = false;
            this.Caption8.Location = new Point(4, 15);
            this.Caption8.Name = "Caption8";
            this.Caption8.Size = new Size(144, 16);
            this.Caption8.Text = "Sonstige Daten (Tags):";
            this.Caption8.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxSort
            // 
            this.cbxSort.Cursor = Cursors.IBeam;
            this.cbxSort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxSort.Location = new Point(232, 312);
            this.cbxSort.Name = "cbxSort";
            this.cbxSort.Regex = null;
            this.cbxSort.Size = new Size(304, 24);
            this.cbxSort.TabIndex = 35;
            // 
            // txbRegex
            // 
            this.txbRegex.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                    | AnchorStyles.Right)));
            this.txbRegex.Cursor = Cursors.IBeam;
            this.txbRegex.Location = new Point(8, 168);
            this.txbRegex.Name = "txbRegex";
            this.txbRegex.Size = new Size(976, 48);
            this.txbRegex.TabIndex = 9;
            // 
            // capSortiermaske
            // 
            this.capSortiermaske.CausesValidation = false;
            this.capSortiermaske.Location = new Point(8, 312);
            this.capSortiermaske.Name = "capSortiermaske";
            this.capSortiermaske.Size = new Size(216, 40);
            this.capSortiermaske.Text = "Bei der Datenbank-Zeilen-Sortierung fungiert diese Spalte als:";
            this.capSortiermaske.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capRegex
            // 
            this.capRegex.CausesValidation = false;
            this.capRegex.Location = new Point(8, 152);
            this.capRegex.Name = "capRegex";
            this.capRegex.Size = new Size(388, 24);
            this.capRegex.Text = "Die Eingabe muss mit dieser Regex-Maske übereinstimmen:";
            // 
            // txbAllowedChars
            // 
            this.txbAllowedChars.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.txbAllowedChars.Cursor = Cursors.IBeam;
            this.txbAllowedChars.Location = new Point(8, 88);
            this.txbAllowedChars.Name = "txbAllowedChars";
            this.txbAllowedChars.Regex = null;
            this.txbAllowedChars.Size = new Size(976, 56);
            this.txbAllowedChars.TabIndex = 30;
            this.txbAllowedChars.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption13
            // 
            this.Caption13.CausesValidation = false;
            this.Caption13.Location = new Point(8, 72);
            this.Caption13.Name = "Caption13";
            this.Caption13.Size = new Size(352, 24);
            this.Caption13.Text = "Folgende Zeichen können vom Benutzer eingegeben werden:";
            this.Caption13.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // cbxLinkedDatabase
            // 
            this.cbxLinkedDatabase.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                             | AnchorStyles.Right)));
            this.cbxLinkedDatabase.Cursor = Cursors.IBeam;
            this.cbxLinkedDatabase.Location = new Point(224, 16);
            this.cbxLinkedDatabase.Name = "cbxLinkedDatabase";
            this.cbxLinkedDatabase.Regex = null;
            this.cbxLinkedDatabase.Size = new Size(752, 24);
            this.cbxLinkedDatabase.TabIndex = 38;
            this.cbxLinkedDatabase.TextChanged += new EventHandler(this.cbxLinkedDatabase_TextChanged);
            // 
            // capLinkedDatabase
            // 
            this.capLinkedDatabase.CausesValidation = false;
            this.capLinkedDatabase.Location = new Point(8, 16);
            this.capLinkedDatabase.Name = "capLinkedDatabase";
            this.capLinkedDatabase.Size = new Size(152, 16);
            this.capLinkedDatabase.Text = "Vernküpfte Datenbank:";
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.BlueFrame1.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.btnVerwendung);
            this.BlueFrame1.Controls.Add(this.capInfo);
            this.BlueFrame1.Controls.Add(this.Caption3);
            this.BlueFrame1.Controls.Add(this.txbName);
            this.BlueFrame1.Controls.Add(this.Caption2);
            this.BlueFrame1.Controls.Add(this.txbCaption);
            this.BlueFrame1.Location = new Point(8, 24);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new Size(997, 104);
            this.BlueFrame1.TabIndex = 16;
            this.BlueFrame1.TabStop = false;
            this.BlueFrame1.Text = "Allgemein";
            // 
            // btnVerwendung
            // 
            this.btnVerwendung.Location = new Point(8, 80);
            this.btnVerwendung.Name = "btnVerwendung";
            this.btnVerwendung.Size = new Size(144, 24);
            this.btnVerwendung.TabIndex = 3;
            this.btnVerwendung.Text = "Verwendungs-Info";
            this.btnVerwendung.Click += new EventHandler(this.btnVerwendung_Click);
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new Point(8, 16);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new Size(280, 19);
            this.capInfo.Text = "NR";
            this.capInfo.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new Point(8, 40);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new Size(136, 16);
            this.Caption3.Text = "Interner Spaltename:";
            this.Caption3.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbName
            // 
            this.txbName.AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.[]()";
            this.txbName.Cursor = Cursors.IBeam;
            this.txbName.Location = new Point(8, 56);
            this.txbName.Name = "txbName";
            this.txbName.Regex = null;
            this.txbName.Size = new Size(296, 24);
            this.txbName.TabIndex = 0;
            // 
            // Caption2
            // 
            this.Caption2.CausesValidation = false;
            this.Caption2.Location = new Point(312, 16);
            this.Caption2.Name = "Caption2";
            this.Caption2.Size = new Size(144, 16);
            this.Caption2.Text = "Angezeigte Beschriftung:";
            this.Caption2.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbCaption
            // 
            this.txbCaption.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.txbCaption.Cursor = Cursors.IBeam;
            this.txbCaption.Location = new Point(312, 32);
            this.txbCaption.MultiLine = true;
            this.txbCaption.Name = "txbCaption";
            this.txbCaption.Regex = null;
            this.txbCaption.Size = new Size(681, 64);
            this.txbCaption.TabIndex = 2;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnOk.ImageCode = "Häkchen|16";
            this.btnOk.Location = new Point(921, 654);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new Size(72, 24);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new EventHandler(this.btnOk_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                       | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabDatenFormat);
            this.tabControl.Controls.Add(this.tabAnzeige);
            this.tabControl.Controls.Add(this.tabBearbeitung);
            this.tabControl.Controls.Add(this.tabAutoBearbeitung);
            this.tabControl.Controls.Add(this.tabFilter);
            this.tabControl.Controls.Add(this.tabQuickInfo);
            this.tabControl.Controls.Add(this.tabSonstiges);
            this.tabControl.Controls.Add(this.tabSpaltenVerlinkung);
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new Point(0, 136);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new Size(1001, 512);
            this.tabControl.TabDefault = this.tabDatenFormat;
            this.tabControl.TabDefaultOrder = null;
            this.tabControl.TabIndex = 15;
            this.tabControl.SelectedIndexChanged += new EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabDatenFormat
            // 
            this.tabDatenFormat.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
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
            this.tabDatenFormat.Controls.Add(this.cbxFormat);
            this.tabDatenFormat.Controls.Add(this.capFormat);
            this.tabDatenFormat.Controls.Add(this.btnMultiline);
            this.tabDatenFormat.Location = new Point(4, 25);
            this.tabDatenFormat.Name = "tabDatenFormat";
            this.tabDatenFormat.Size = new Size(993, 483);
            this.tabDatenFormat.TabIndex = 12;
            this.tabDatenFormat.Text = "Daten-Format";
            // 
            // btnMaxTextLenght
            // 
            this.btnMaxTextLenght.ImageCode = "Taschenrechner|16";
            this.btnMaxTextLenght.Location = new Point(816, 232);
            this.btnMaxTextLenght.Name = "btnMaxTextLenght";
            this.btnMaxTextLenght.QuickInfo = "Prüft alle Zellen und berechnet die ideale\r\nmaximale Text Länge";
            this.btnMaxTextLenght.Size = new Size(40, 24);
            this.btnMaxTextLenght.TabIndex = 48;
            this.btnMaxTextLenght.Click += new EventHandler(this.btnMaxTextLenght_Click);
            // 
            // txbMaxTextLenght
            // 
            this.txbMaxTextLenght.AdditionalFormatCheck = AdditionalCheck.Integer;
            this.txbMaxTextLenght.AllowedChars = "0123456789";
            this.txbMaxTextLenght.Cursor = Cursors.IBeam;
            this.txbMaxTextLenght.Location = new Point(720, 232);
            this.txbMaxTextLenght.MaxTextLenght = 255;
            this.txbMaxTextLenght.Name = "txbMaxTextLenght";
            this.txbMaxTextLenght.QuickInfo = resources.GetString("txbMaxTextLenght.QuickInfo");
            this.txbMaxTextLenght.Regex = "^((-?[1-9]\\d*)|0)$";
            this.txbMaxTextLenght.Size = new Size(96, 24);
            this.txbMaxTextLenght.TabIndex = 47;
            // 
            // capMaxTextLenght
            // 
            this.capMaxTextLenght.CausesValidation = false;
            this.capMaxTextLenght.Location = new Point(560, 232);
            this.capMaxTextLenght.Name = "capMaxTextLenght";
            this.capMaxTextLenght.QuickInfo = "Pro Zeile!\r\nEs wird wirklich die Anzahl der Zeichen gezählt.\r\nEs bezeht sich nur " +
    "auf das Format, und es wird evtl. eine Meldung ausgegeben,\r\ndas die Eingabe nich" +
    "t dem Format entspricht.";
            this.capMaxTextLenght.Size = new Size(160, 24);
            this.capMaxTextLenght.Text = "Maximale Text-Länge:";
            this.capMaxTextLenght.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpSchnellformat
            // 
            this.grpSchnellformat.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
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
            this.grpSchnellformat.Location = new Point(8, 384);
            this.grpSchnellformat.Name = "grpSchnellformat";
            this.grpSchnellformat.Size = new Size(976, 80);
            this.grpSchnellformat.TabIndex = 11;
            this.grpSchnellformat.TabStop = false;
            this.grpSchnellformat.Text = "Schnell-Format";
            // 
            // btnSchnellText
            // 
            this.btnSchnellText.Location = new Point(8, 16);
            this.btnSchnellText.Name = "btnSchnellText";
            this.btnSchnellText.Size = new Size(128, 24);
            this.btnSchnellText.TabIndex = 0;
            this.btnSchnellText.Text = "Text";
            this.btnSchnellText.Click += new EventHandler(this.btnSchnellText_Click);
            // 
            // btnSchnellBit
            // 
            this.btnSchnellBit.Location = new Point(280, 16);
            this.btnSchnellBit.Name = "btnSchnellBit";
            this.btnSchnellBit.Size = new Size(128, 24);
            this.btnSchnellBit.TabIndex = 10;
            this.btnSchnellBit.Text = "Bit (Ja/Nein)";
            this.btnSchnellBit.Click += new EventHandler(this.btnSchnellBit_Click);
            // 
            // btnSchnellDatum
            // 
            this.btnSchnellDatum.Location = new Point(8, 48);
            this.btnSchnellDatum.Name = "btnSchnellDatum";
            this.btnSchnellDatum.Size = new Size(128, 24);
            this.btnSchnellDatum.TabIndex = 1;
            this.btnSchnellDatum.Text = "Datum";
            this.btnSchnellDatum.Click += new EventHandler(this.btnSchnellDatum_Click);
            // 
            // btnSchnellBildCode
            // 
            this.btnSchnellBildCode.Location = new Point(688, 48);
            this.btnSchnellBildCode.Name = "btnSchnellBildCode";
            this.btnSchnellBildCode.Size = new Size(128, 24);
            this.btnSchnellBildCode.TabIndex = 9;
            this.btnSchnellBildCode.Text = "Bild-Code";
            this.btnSchnellBildCode.Click += new EventHandler(this.btnSchnellBildCode_Click);
            // 
            // btnSchnellDatumUhrzeit
            // 
            this.btnSchnellDatumUhrzeit.Location = new Point(144, 48);
            this.btnSchnellDatumUhrzeit.Name = "btnSchnellDatumUhrzeit";
            this.btnSchnellDatumUhrzeit.Size = new Size(128, 24);
            this.btnSchnellDatumUhrzeit.TabIndex = 2;
            this.btnSchnellDatumUhrzeit.Text = "Datum/Uhrzeit";
            this.btnSchnellDatumUhrzeit.Click += new EventHandler(this.btnSchnellDatumUhrzeit_Click);
            // 
            // btnSchnellIInternetAdresse
            // 
            this.btnSchnellIInternetAdresse.Location = new Point(552, 48);
            this.btnSchnellIInternetAdresse.Name = "btnSchnellIInternetAdresse";
            this.btnSchnellIInternetAdresse.Size = new Size(128, 24);
            this.btnSchnellIInternetAdresse.TabIndex = 8;
            this.btnSchnellIInternetAdresse.Text = "Internet-Adresse";
            this.btnSchnellIInternetAdresse.Click += new EventHandler(this.btnSchnellIInternetAdresse_Click);
            // 
            // btnSchnellEmail
            // 
            this.btnSchnellEmail.Location = new Point(280, 48);
            this.btnSchnellEmail.Name = "btnSchnellEmail";
            this.btnSchnellEmail.Size = new Size(128, 24);
            this.btnSchnellEmail.TabIndex = 3;
            this.btnSchnellEmail.Text = "Email";
            this.btnSchnellEmail.Click += new EventHandler(this.btnSchnellEmail_Click);
            // 
            // btnSchnellAuswahloptionen
            // 
            this.btnSchnellAuswahloptionen.Location = new Point(144, 16);
            this.btnSchnellAuswahloptionen.Name = "btnSchnellAuswahloptionen";
            this.btnSchnellAuswahloptionen.Size = new Size(128, 24);
            this.btnSchnellAuswahloptionen.TabIndex = 7;
            this.btnSchnellAuswahloptionen.Text = "Auswahl-Optionen";
            this.btnSchnellAuswahloptionen.Click += new EventHandler(this.btnSchnellAuswahloptionen_Click);
            // 
            // btnSchnellTelefonNummer
            // 
            this.btnSchnellTelefonNummer.Location = new Point(416, 48);
            this.btnSchnellTelefonNummer.Name = "btnSchnellTelefonNummer";
            this.btnSchnellTelefonNummer.Size = new Size(128, 24);
            this.btnSchnellTelefonNummer.TabIndex = 4;
            this.btnSchnellTelefonNummer.Text = "Telefonnummer";
            this.btnSchnellTelefonNummer.Click += new EventHandler(this.btnSchnellTelefonNummer_Click);
            // 
            // btnSchnellGleitkommazahl
            // 
            this.btnSchnellGleitkommazahl.Location = new Point(688, 16);
            this.btnSchnellGleitkommazahl.Name = "btnSchnellGleitkommazahl";
            this.btnSchnellGleitkommazahl.Size = new Size(128, 24);
            this.btnSchnellGleitkommazahl.TabIndex = 6;
            this.btnSchnellGleitkommazahl.Text = "Gleitkommazahl";
            this.btnSchnellGleitkommazahl.Click += new EventHandler(this.btnSchnellGleitkommazahl_Click);
            // 
            // btnSchnellGanzzahl
            // 
            this.btnSchnellGanzzahl.Location = new Point(552, 16);
            this.btnSchnellGanzzahl.Name = "btnSchnellGanzzahl";
            this.btnSchnellGanzzahl.Size = new Size(128, 24);
            this.btnSchnellGanzzahl.TabIndex = 5;
            this.btnSchnellGanzzahl.Text = "Ganzzahl";
            this.btnSchnellGanzzahl.Click += new EventHandler(this.btnSchnellGanzzahl_Click);
            // 
            // tabSpaltenVerlinkung
            // 
            this.tabSpaltenVerlinkung.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSpaltenVerlinkung.Controls.Add(this.tblFilterliste);
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxTargetColumn);
            this.tabSpaltenVerlinkung.Controls.Add(this.capLinkedDatabase);
            this.tabSpaltenVerlinkung.Controls.Add(this.capTargetColumn);
            this.tabSpaltenVerlinkung.Controls.Add(this.cbxLinkedDatabase);
            this.tabSpaltenVerlinkung.Location = new Point(4, 25);
            this.tabSpaltenVerlinkung.Name = "tabSpaltenVerlinkung";
            this.tabSpaltenVerlinkung.Size = new Size(993, 483);
            this.tabSpaltenVerlinkung.TabIndex = 11;
            this.tabSpaltenVerlinkung.Text = "Spalten-Verlinkung";
            // 
            // tblFilterliste
            // 
            this.tblFilterliste.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                           | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.tblFilterliste.Location = new Point(8, 80);
            this.tblFilterliste.Name = "tblFilterliste";
            this.tblFilterliste.ShowWaitScreen = true;
            this.tblFilterliste.Size = new Size(968, 396);
            this.tblFilterliste.TabIndex = 39;
            // 
            // cbxTargetColumn
            // 
            this.cbxTargetColumn.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.cbxTargetColumn.Cursor = Cursors.IBeam;
            this.cbxTargetColumn.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxTargetColumn.Location = new Point(224, 48);
            this.cbxTargetColumn.Name = "cbxTargetColumn";
            this.cbxTargetColumn.Regex = null;
            this.cbxTargetColumn.Size = new Size(752, 24);
            this.cbxTargetColumn.TabIndex = 5;
            this.cbxTargetColumn.TextChanged += new EventHandler(this.cbxTargetColumn_TextChanged);
            // 
            // capTargetColumn
            // 
            this.capTargetColumn.CausesValidation = false;
            this.capTargetColumn.Location = new Point(8, 48);
            this.capTargetColumn.Name = "capTargetColumn";
            this.capTargetColumn.Size = new Size(200, 16);
            this.capTargetColumn.Text = "Aus dieser Spalte die Werte holen:";
            this.capTargetColumn.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // caption5
            // 
            this.caption5.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.caption5.CausesValidation = false;
            this.caption5.Location = new Point(80, 654);
            this.caption5.Name = "caption5";
            this.caption5.Size = new Size(104, 24);
            this.caption5.Text = "Aktuelle Ansicht:";
            // 
            // butAktuellVor
            // 
            this.butAktuellVor.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.butAktuellVor.ImageCode = "Pfeil_Rechts|16|||0000FF";
            this.butAktuellVor.Location = new Point(272, 654);
            this.butAktuellVor.Name = "butAktuellVor";
            this.butAktuellVor.Size = new Size(72, 24);
            this.butAktuellVor.TabIndex = 19;
            this.butAktuellVor.Click += new EventHandler(this.butAktuellVor_Click);
            // 
            // butAktuellZurueck
            // 
            this.butAktuellZurueck.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.butAktuellZurueck.ImageCode = "Pfeil_Links|16|||0000FF";
            this.butAktuellZurueck.Location = new Point(192, 654);
            this.butAktuellZurueck.Name = "butAktuellZurueck";
            this.butAktuellZurueck.Size = new Size(72, 24);
            this.butAktuellZurueck.TabIndex = 18;
            this.butAktuellZurueck.Click += new EventHandler(this.butAktuellZurueck_Click);
            // 
            // capTabellenname
            // 
            this.capTabellenname.CausesValidation = false;
            this.capTabellenname.Location = new Point(12, 1);
            this.capTabellenname.Name = "capTabellenname";
            this.capTabellenname.Size = new Size(988, 24);
            this.capTabellenname.Text = "Tabellenname:";
            this.capTabellenname.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            this.capTabellenname.Translate = false;
            // 
            // ColumnEditor
            // 
            this.ClientSize = new Size(1007, 685);
            this.Controls.Add(this.capTabellenname);
            this.Controls.Add(this.caption5);
            this.Controls.Add(this.butAktuellVor);
            this.Controls.Add(this.butAktuellZurueck);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Name = "ColumnEditor";
            this.Text = "Spalten-Eigenschaften";
            this.tabAnzeige.ResumeLayout(false);
            this.tabBearbeitung.ResumeLayout(false);
            this.grpAuswahlmenuOptionen.ResumeLayout(false);
            this.tabAutoBearbeitung.ResumeLayout(false);
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
        private Caption capFormat;
        private Caption Caption18;
        private TextBox txbAdminInfo;
        private Caption Caption17;
        private Button btnOtherValuesToo;
        private ListBox lbxCellEditor;
        private ComboBox cbxFormat;
        private TextBox txbTags;
        private TabControl tabControl;
        private TabPage tabAnzeige;
        private TabPage tabBearbeitung;
        private TabPage tabAutoBearbeitung;
        private TabPage tabFilter;
        private TabPage tabQuickInfo;
        private TabPage tabSonstiges;
        private Button btnEinzeiligDarstellen;
        private Button btnIgnoreLock;
        private Button btnLogUndo;
        private Button btnSpellChecking;
        private Caption capSpaltenbild;
        private Caption capEinheit;
        private ComboBox cbxEinheit;
        private TextBox txbJoker;
        private Caption capJokerValue;
        private TextBox txbInitValue;
        private Caption capInitValue;
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
        private TextBox txbPrefix;
        private Caption capPraefix;
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
    }
}
