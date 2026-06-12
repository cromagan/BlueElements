// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using ColorDialog = System.Windows.Forms.ColorDialog;
using ComboBox = BlueControls.Controls.ComboBox;
using Form = BlueControls.Forms.Form;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs {
    internal sealed partial class ColumnEditor : Form {
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            var resources = new ComponentResourceManager(typeof(ColumnEditor));
            ColorDia = new ColorDialog();
            tabAnzeige = new TabPage();
            cbxBackground = new ComboBox();
            capHintergrund = new Caption();
            cbxRenderer = new ComboBox();
            txbSpaltenbild = new TextBox();
            txbUeberschift1 = new TextBox();
            txbUeberschift3 = new TextBox();
            capSpaltenbild = new Caption();
            txbFixedColumnWidth = new TextBox();
            RendererEditor = new ScrollPanel();
            capFixedColumnWidth = new Caption();
            txbUeberschift2 = new TextBox();
            btnTextColor = new Button();
            btnBackColor = new Button();
            capUeberschrift3 = new Caption();
            capRenderer = new Caption();
            btnSpellChecking = new Button();
            capUeberschrift1 = new Caption();
            cbxTranslate = new ComboBox();
            capTranslate = new Caption();
            capUeberschrift2 = new Caption();
            capLinkerRand = new Caption();
            cbxAlign = new ComboBox();
            cbxRandLinks = new ComboBox();
            btnStandard = new Button();
            capRechterRand = new Caption();
            cbxRandRechts = new ComboBox();
            capAlign = new Caption();
            cbxScriptType = new ComboBox();
            capScriptType = new Caption();
            chkFormatierungErlaubt = new Button();
            cbxAdditionalCheck = new ComboBox();
            capcbxAdditionalCheck = new Caption();
            cbxChunk = new ComboBox();
            capChunk = new Caption();
            chkMultiline = new Button();
            tabBearbeitung = new TabPage();
            grpAuswahlmenuOptionen = new GroupBox();
            btnOtherValuesToo = new Button();
            txbAuswaehlbareWerte = new TextBox();
            capImmerWerte = new Caption();
            btnRequired = new Button();
            btnIgnoreLock = new Button();
            lbxCellEditor = new ListBox();
            btnEditableStandard = new Button();
            capUserGroupEdit = new Caption();
            btnEditableDropdown = new Button();
            chkSaveContent = new Button();
            tabAutoKorrektur = new TabPage();
            btnCalculateMaxCellLength = new Button();
            txbAutoReplace = new TextBox();
            capAutoReplace = new Caption();
            txbMaxCellLength = new TextBox();
            txbAutoRemove = new TextBox();
            capAutoRemove = new Caption();
            capMaxCellLength = new Caption();
            btnAutoEditKleineFehler = new Button();
            btnAutoEditToUpper = new Button();
            txbRunden = new TextBox();
            capNachkommastellen = new Caption();
            btnAutoEditAutoSort = new Button();
            tabFilter = new TabPage();
            chkFilterOnlyOr = new Button();
            chkFilterOnlyAND = new Button();
            capJokerValue = new Caption();
            txbJoker = new TextBox();
            btnZeilenFilterIgnorieren = new Button();
            btnAutoFilterMoeglich = new Button();
            btnAutoFilterTXTErlaubt = new Button();
            btnAutoFilterErweitertErlaubt = new Button();
            tabQuickInfo = new TabPage();
            txbAdminInfo = new TextBox();
            txbQuickinfo = new TextBox();
            capAdminInfo = new Caption();
            capQuickInfo = new Caption();
            btnQI_Vorschau = new Button();
            tabSonstiges = new TabPage();
            txbTags = new TextBox();
            capTags = new Caption();
            cbxSort = new ComboBox();
            txbRegex = new TextBox();
            capSortiermaske = new Caption();
            capRegex = new Caption();
            txbAllowedChars = new TextBox();
            capAllowedChars = new Caption();
            cbxLinkedTable = new ComboBox();
            capLinkedTable = new Caption();
            BlueFrame1 = new GroupBox();
            capInfo = new Caption();
            capInternalName = new Caption();
            txbName = new TextBox();
            capCaption = new Caption();
            txbCaption = new TextBox();
            btnSystemInfo = new Button();
            btnOk = new Button();
            tabControl = new TabControl();
            tabDatenFormat = new TabPage();
            grpStyles = new GroupBox();
            lstStyles = new ListBox();
            capInfos = new Caption();
            chkRelation = new Button();
            chkIsKeyColumn = new Button();
            chkIsFirst = new Button();
            btnMaxTextLength = new Button();
            txbMaxTextLength = new TextBox();
            capMaxTextLength = new Caption();
            tabSpaltenVerlinkung = new TabPage();
            cbxRelationType = new ComboBox();
            capOtherTable = new Caption();
            tblFilterliste = new TableView();
            cbxTargetColumn = new ComboBox();
            capTargetColumn = new Caption();
            capCurrentView = new Caption();
            butAktuellVor = new Button();
            butAktuellZurueck = new Button();
            capTabellenname = new Caption();
            btnSpaltenkopf = new Button();
            tabAnzeige.SuspendLayout();
            tabBearbeitung.SuspendLayout();
            grpAuswahlmenuOptionen.SuspendLayout();
            tabAutoKorrektur.SuspendLayout();
            tabFilter.SuspendLayout();
            tabQuickInfo.SuspendLayout();
            tabSonstiges.SuspendLayout();
            BlueFrame1.SuspendLayout();
            tabControl.SuspendLayout();
            tabDatenFormat.SuspendLayout();
            grpStyles.SuspendLayout();
            tabSpaltenVerlinkung.SuspendLayout();
            SuspendLayout();
            // 
            // ColorDia
            // 
            ColorDia.AnyColor = true;
            ColorDia.FullOpen = true;
            // 
            // tabAnzeige
            // 
            tabAnzeige.BackColor = Color.FromArgb(255, 255, 255);
            tabAnzeige.Controls.Add(cbxBackground);
            tabAnzeige.Controls.Add(capHintergrund);
            tabAnzeige.Controls.Add(cbxRenderer);
            tabAnzeige.Controls.Add(txbSpaltenbild);
            tabAnzeige.Controls.Add(txbUeberschift1);
            tabAnzeige.Controls.Add(txbUeberschift3);
            tabAnzeige.Controls.Add(capSpaltenbild);
            tabAnzeige.Controls.Add(txbFixedColumnWidth);
            tabAnzeige.Controls.Add(RendererEditor);
            tabAnzeige.Controls.Add(capFixedColumnWidth);
            tabAnzeige.Controls.Add(txbUeberschift2);
            tabAnzeige.Controls.Add(btnTextColor);
            tabAnzeige.Controls.Add(btnBackColor);
            tabAnzeige.Controls.Add(capUeberschrift3);
            tabAnzeige.Controls.Add(capRenderer);
            tabAnzeige.Controls.Add(btnSpellChecking);
            tabAnzeige.Controls.Add(capUeberschrift1);
            tabAnzeige.Controls.Add(cbxTranslate);
            tabAnzeige.Controls.Add(capTranslate);
            tabAnzeige.Controls.Add(capUeberschrift2);
            tabAnzeige.Controls.Add(capLinkerRand);
            tabAnzeige.Controls.Add(cbxAlign);
            tabAnzeige.Controls.Add(cbxRandLinks);
            tabAnzeige.Controls.Add(btnStandard);
            tabAnzeige.Controls.Add(capRechterRand);
            tabAnzeige.Controls.Add(cbxRandRechts);
            tabAnzeige.Controls.Add(capAlign);
            tabAnzeige.Location = new Point(4, 25);
            tabAnzeige.Name = "tabAnzeige";
            tabAnzeige.Padding = new Padding(3);
            tabAnzeige.Size = new Size(1098, 594);
            tabAnzeige.TabIndex = 0;
            tabAnzeige.Text = "Anzeige";
            // 
            // cbxBackground
            // 
            cbxBackground.Cursor = Cursors.IBeam;
            cbxBackground.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxBackground.Location = new Point(280, 96);
            cbxBackground.Name = "cbxBackground";
            cbxBackground.RegexCheck = null;
            cbxBackground.Size = new Size(128, 24);
            cbxBackground.TabIndex = 47;
            // 
            // capHintergrund
            // 
            capHintergrund.CausesValidation = false;
            capHintergrund.Location = new Point(280, 80);
            capHintergrund.Name = "capHintergrund";
            capHintergrund.Size = new Size(88, 16);
            capHintergrund.Text = "Hintergrund:";
            // 
            // cbxRenderer
            // 
            cbxRenderer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbxRenderer.Cursor = Cursors.IBeam;
            cbxRenderer.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxRenderer.Location = new Point(416, 24);
            cbxRenderer.Name = "cbxRenderer";
            cbxRenderer.RegexCheck = null;
            cbxRenderer.Size = new Size(673, 24);
            cbxRenderer.TabIndex = 44;
            cbxRenderer.TextChanged += cbxRenderer_TextChanged;
            // 
            // txbSpaltenbild
            // 
            txbSpaltenbild.Cursor = Cursors.IBeam;
            txbSpaltenbild.Location = new Point(8, 376);
            txbSpaltenbild.Name = "txbSpaltenbild";
            txbSpaltenbild.RegexCheck = null;
            txbSpaltenbild.Size = new Size(400, 24);
            txbSpaltenbild.TabIndex = 40;
            // 
            // txbUeberschift1
            // 
            txbUeberschift1.Cursor = Cursors.IBeam;
            txbUeberschift1.Location = new Point(104, 256);
            txbUeberschift1.Name = "txbUeberschift1";
            txbUeberschift1.RegexCheck = null;
            txbUeberschift1.Size = new Size(304, 24);
            txbUeberschift1.TabIndex = 36;
            // 
            // txbUeberschift3
            // 
            txbUeberschift3.Cursor = Cursors.IBeam;
            txbUeberschift3.Location = new Point(104, 319);
            txbUeberschift3.Name = "txbUeberschift3";
            txbUeberschift3.RegexCheck = null;
            txbUeberschift3.Size = new Size(304, 24);
            txbUeberschift3.TabIndex = 38;
            // 
            // capSpaltenbild
            // 
            capSpaltenbild.CausesValidation = false;
            capSpaltenbild.Location = new Point(8, 360);
            capSpaltenbild.Name = "capSpaltenbild";
            capSpaltenbild.Size = new Size(72, 24);
            capSpaltenbild.Text = "Spaltenbild:";
            // 
            // txbFixedColumnWidth
            // 
            txbFixedColumnWidth.AllowedChars = "0123456789|";
            txbFixedColumnWidth.Cursor = Cursors.IBeam;
            txbFixedColumnWidth.Location = new Point(136, 224);
            txbFixedColumnWidth.Name = "txbFixedColumnWidth";
            txbFixedColumnWidth.QuickInfo = "Wenn ein Wert >0 eingegeben wird, \r\nwird die Spalte immer in dieser Breite angezeigt.";
            txbFixedColumnWidth.Size = new Size(88, 24);
            txbFixedColumnWidth.Suffix = "Pixel";
            txbFixedColumnWidth.TabIndex = 42;
            // 
            // RendererEditor
            // 
            RendererEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RendererEditor.Location = new Point(416, 48);
            RendererEditor.Name = "RendererEditor";
            RendererEditor.Size = new Size(673, 543);
            RendererEditor.TabIndex = 48;
            // 
            // capFixedColumnWidth
            // 
            capFixedColumnWidth.CausesValidation = false;
            capFixedColumnWidth.Location = new Point(8, 224);
            capFixedColumnWidth.Name = "capFixedColumnWidth";
            capFixedColumnWidth.Size = new Size(120, 16);
            capFixedColumnWidth.Text = "Feste Spaltenbreite:";
            // 
            // txbUeberschift2
            // 
            txbUeberschift2.Cursor = Cursors.IBeam;
            txbUeberschift2.Location = new Point(104, 295);
            txbUeberschift2.Name = "txbUeberschift2";
            txbUeberschift2.RegexCheck = null;
            txbUeberschift2.Size = new Size(304, 24);
            txbUeberschift2.TabIndex = 37;
            // 
            // btnTextColor
            // 
            btnTextColor.Location = new Point(280, 8);
            btnTextColor.Name = "btnTextColor";
            btnTextColor.Size = new Size(128, 32);
            btnTextColor.TabIndex = 4;
            btnTextColor.Text = "Textfarbe";
            btnTextColor.Click += btnTextColor_Click;
            // 
            // btnBackColor
            // 
            btnBackColor.Location = new Point(280, 40);
            btnBackColor.Name = "btnBackColor";
            btnBackColor.Size = new Size(128, 32);
            btnBackColor.TabIndex = 3;
            btnBackColor.Text = "Hintergrundfarbe";
            btnBackColor.Click += btnBackColor_Click;
            // 
            // capUeberschrift3
            // 
            capUeberschrift3.CausesValidation = false;
            capUeberschrift3.Location = new Point(8, 320);
            capUeberschrift3.Name = "capUeberschrift3";
            capUeberschrift3.Size = new Size(88, 16);
            capUeberschrift3.Text = "Überschrift 3:";
            // 
            // capRenderer
            // 
            capRenderer.CausesValidation = false;
            capRenderer.Location = new Point(416, 8);
            capRenderer.Name = "capRenderer";
            capRenderer.Size = new Size(160, 16);
            capRenderer.Text = "Standard-Renderer:";
            // 
            // btnSpellChecking
            // 
            btnSpellChecking.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnSpellChecking.Location = new Point(8, 200);
            btnSpellChecking.Name = "btnSpellChecking";
            btnSpellChecking.Size = new Size(200, 16);
            btnSpellChecking.TabIndex = 33;
            btnSpellChecking.Text = "Rechtschreibprüfung aktivieren";
            // 
            // capUeberschrift1
            // 
            capUeberschrift1.CausesValidation = false;
            capUeberschrift1.Location = new Point(8, 272);
            capUeberschrift1.Name = "capUeberschrift1";
            capUeberschrift1.Size = new Size(88, 16);
            capUeberschrift1.Text = "Überschrift 1:";
            // 
            // cbxTranslate
            // 
            cbxTranslate.Cursor = Cursors.IBeam;
            cbxTranslate.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxTranslate.Location = new Point(104, 168);
            cbxTranslate.Name = "cbxTranslate";
            cbxTranslate.RegexCheck = null;
            cbxTranslate.Size = new Size(304, 24);
            cbxTranslate.TabIndex = 37;
            // 
            // capTranslate
            // 
            capTranslate.CausesValidation = false;
            capTranslate.Location = new Point(8, 168);
            capTranslate.Name = "capTranslate";
            capTranslate.Size = new Size(88, 24);
            capTranslate.Text = "Übersetzen:";
            // 
            // capUeberschrift2
            // 
            capUeberschrift2.CausesValidation = false;
            capUeberschrift2.Location = new Point(8, 296);
            capUeberschrift2.Name = "capUeberschrift2";
            capUeberschrift2.Size = new Size(88, 16);
            capUeberschrift2.Text = "Überschrift 2:";
            // 
            // capLinkerRand
            // 
            capLinkerRand.CausesValidation = false;
            capLinkerRand.Location = new Point(8, 80);
            capLinkerRand.Name = "capLinkerRand";
            capLinkerRand.Size = new Size(80, 16);
            capLinkerRand.Text = "Linker Rand:";
            // 
            // cbxAlign
            // 
            cbxAlign.Cursor = Cursors.IBeam;
            cbxAlign.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxAlign.Location = new Point(104, 136);
            cbxAlign.Name = "cbxAlign";
            cbxAlign.RegexCheck = null;
            cbxAlign.Size = new Size(304, 24);
            cbxAlign.TabIndex = 7;
            // 
            // cbxRandLinks
            // 
            cbxRandLinks.Cursor = Cursors.IBeam;
            cbxRandLinks.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxRandLinks.Location = new Point(8, 96);
            cbxRandLinks.Name = "cbxRandLinks";
            cbxRandLinks.RegexCheck = null;
            cbxRandLinks.Size = new Size(128, 24);
            cbxRandLinks.TabIndex = 24;
            // 
            // btnStandard
            // 
            btnStandard.Location = new Point(8, 8);
            btnStandard.Name = "btnStandard";
            btnStandard.Size = new Size(200, 48);
            btnStandard.TabIndex = 39;
            btnStandard.Text = "Standard herstellen";
            btnStandard.Click += btnStandard_Click;
            // 
            // capRechterRand
            // 
            capRechterRand.CausesValidation = false;
            capRechterRand.Location = new Point(144, 80);
            capRechterRand.Name = "capRechterRand";
            capRechterRand.Size = new Size(88, 16);
            capRechterRand.Text = "Rechter Rand:";
            // 
            // cbxRandRechts
            // 
            cbxRandRechts.Cursor = Cursors.IBeam;
            cbxRandRechts.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxRandRechts.Location = new Point(144, 96);
            cbxRandRechts.Name = "cbxRandRechts";
            cbxRandRechts.RegexCheck = null;
            cbxRandRechts.Size = new Size(128, 24);
            cbxRandRechts.TabIndex = 25;
            // 
            // capAlign
            // 
            capAlign.CausesValidation = false;
            capAlign.Location = new Point(8, 136);
            capAlign.Name = "capAlign";
            capAlign.Size = new Size(88, 24);
            capAlign.Text = "Ausrichtung:";
            // 
            // cbxScriptType
            // 
            cbxScriptType.Cursor = Cursors.IBeam;
            cbxScriptType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxScriptType.Location = new Point(232, 288);
            cbxScriptType.Name = "cbxScriptType";
            cbxScriptType.RegexCheck = null;
            cbxScriptType.Size = new Size(304, 24);
            cbxScriptType.TabIndex = 43;
            // 
            // capScriptType
            // 
            capScriptType.CausesValidation = false;
            capScriptType.Location = new Point(8, 288);
            capScriptType.Name = "capScriptType";
            capScriptType.Size = new Size(216, 24);
            capScriptType.Text = "Im Skript ist der Datentyp:";
            // 
            // chkFormatierungErlaubt
            // 
            chkFormatierungErlaubt.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkFormatierungErlaubt.Location = new Point(312, 8);
            chkFormatierungErlaubt.Name = "chkFormatierungErlaubt";
            chkFormatierungErlaubt.Size = new Size(296, 16);
            chkFormatierungErlaubt.TabIndex = 41;
            chkFormatierungErlaubt.Text = "Text-Formatierung erlaubt (Fett, Kursiv, etc.)";
            // 
            // cbxAdditionalCheck
            // 
            cbxAdditionalCheck.Cursor = Cursors.IBeam;
            cbxAdditionalCheck.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxAdditionalCheck.Location = new Point(232, 248);
            cbxAdditionalCheck.Name = "cbxAdditionalCheck";
            cbxAdditionalCheck.RegexCheck = null;
            cbxAdditionalCheck.Size = new Size(304, 24);
            cbxAdditionalCheck.TabIndex = 34;
            // 
            // capcbxAdditionalCheck
            // 
            capcbxAdditionalCheck.CausesValidation = false;
            capcbxAdditionalCheck.Location = new Point(8, 248);
            capcbxAdditionalCheck.Name = "capcbxAdditionalCheck";
            capcbxAdditionalCheck.Size = new Size(216, 40);
            capcbxAdditionalCheck.Text = "Zusätzliche Prüfung, ob der eingegeben Wert konsitent ist zu:";
            // 
            // cbxChunk
            // 
            cbxChunk.Cursor = Cursors.IBeam;
            cbxChunk.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxChunk.Location = new Point(840, 8);
            cbxChunk.Name = "cbxChunk";
            cbxChunk.RegexCheck = null;
            cbxChunk.Size = new Size(232, 24);
            cbxChunk.TabIndex = 27;
            // 
            // capChunk
            // 
            capChunk.CausesValidation = false;
            capChunk.Location = new Point(656, 8);
            capChunk.Name = "capChunk";
            capChunk.Size = new Size(184, 16);
            capChunk.Text = "Werte zum Chunken benutzen:";
            // 
            // chkMultiline
            // 
            chkMultiline.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkMultiline.Location = new Point(312, 24);
            chkMultiline.Name = "chkMultiline";
            chkMultiline.Size = new Size(296, 16);
            chkMultiline.TabIndex = 7;
            chkMultiline.Text = "Mehrere Einträge pro Zelle erlaubt (mehrzeilig)";
            // 
            // tabBearbeitung
            // 
            tabBearbeitung.BackColor = Color.FromArgb(255, 255, 255);
            tabBearbeitung.Controls.Add(grpAuswahlmenuOptionen);
            tabBearbeitung.Controls.Add(btnIgnoreLock);
            tabBearbeitung.Controls.Add(lbxCellEditor);
            tabBearbeitung.Controls.Add(btnEditableStandard);
            tabBearbeitung.Controls.Add(capUserGroupEdit);
            tabBearbeitung.Controls.Add(btnEditableDropdown);
            tabBearbeitung.Location = new Point(4, 25);
            tabBearbeitung.Name = "tabBearbeitung";
            tabBearbeitung.Padding = new Padding(3);
            tabBearbeitung.Size = new Size(1098, 594);
            tabBearbeitung.TabIndex = 1;
            tabBearbeitung.Text = "Bearbeitung";
            // 
            // grpAuswahlmenuOptionen
            // 
            grpAuswahlmenuOptionen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpAuswahlmenuOptionen.BackColor = Color.FromArgb(255, 255, 255);
            grpAuswahlmenuOptionen.CausesValidation = false;
            grpAuswahlmenuOptionen.Controls.Add(btnOtherValuesToo);
            grpAuswahlmenuOptionen.Controls.Add(txbAuswaehlbareWerte);
            grpAuswahlmenuOptionen.Controls.Add(capImmerWerte);
            grpAuswahlmenuOptionen.Location = new Point(32, 80);
            grpAuswahlmenuOptionen.Name = "grpAuswahlmenuOptionen";
            grpAuswahlmenuOptionen.Size = new Size(536, 504);
            grpAuswahlmenuOptionen.TabIndex = 0;
            grpAuswahlmenuOptionen.TabStop = false;
            grpAuswahlmenuOptionen.Text = "Auswahlmenü-Optionen:";
            // 
            // btnOtherValuesToo
            // 
            btnOtherValuesToo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnOtherValuesToo.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnOtherValuesToo.Location = new Point(8, 464);
            btnOtherValuesToo.Name = "btnOtherValuesToo";
            btnOtherValuesToo.Size = new Size(512, 32);
            btnOtherValuesToo.TabIndex = 7;
            btnOtherValuesToo.Text = "Auch Werte, die in anderen Zellen derselben Spalte vorhanden sind, werden zum Auswählen vorgeschlagen";
            // 
            // txbAuswaehlbareWerte
            // 
            txbAuswaehlbareWerte.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbAuswaehlbareWerte.Cursor = Cursors.IBeam;
            txbAuswaehlbareWerte.Location = new Point(8, 64);
            txbAuswaehlbareWerte.MultiLine = true;
            txbAuswaehlbareWerte.Name = "txbAuswaehlbareWerte";
            txbAuswaehlbareWerte.RegexCheck = null;
            txbAuswaehlbareWerte.Size = new Size(520, 392);
            txbAuswaehlbareWerte.SpellCheckingEnabled = true;
            txbAuswaehlbareWerte.TabIndex = 0;
            // 
            // capImmerWerte
            // 
            capImmerWerte.CausesValidation = false;
            capImmerWerte.Location = new Point(8, 40);
            capImmerWerte.Name = "capImmerWerte";
            capImmerWerte.Size = new Size(440, 24);
            capImmerWerte.Text = "<b><u>Immer auswählbare Werte:";
            // 
            // btnRequired
            // 
            btnRequired.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnRequired.Location = new Point(8, 152);
            btnRequired.Name = "btnRequired";
            btnRequired.QuickInfo = resources.GetString("btnRequired.QuickInfo");
            btnRequired.Size = new Size(328, 16);
            btnRequired.TabIndex = 6;
            btnRequired.Text = "Wert erforderlich <i> ('0' zählt als Wert!)";
            // 
            // btnIgnoreLock
            // 
            btnIgnoreLock.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnIgnoreLock.Location = new Point(592, 328);
            btnIgnoreLock.Name = "btnIgnoreLock";
            btnIgnoreLock.Size = new Size(288, 32);
            btnIgnoreLock.TabIndex = 27;
            btnIgnoreLock.Text = "Die Bearbeitung ist auch möglich, wenn die Zeile abgeschlossen ist.";
            // 
            // lbxCellEditor
            // 
            lbxCellEditor.Appearance = ListBoxAppearance.Listbox_Boxes;
            lbxCellEditor.CheckBehavior = CheckBehavior.MultiSelection;
            lbxCellEditor.FilterText = null;
            lbxCellEditor.Location = new Point(576, 48);
            lbxCellEditor.Name = "lbxCellEditor";
            lbxCellEditor.Size = new Size(328, 272);
            lbxCellEditor.TabIndex = 26;
            // 
            // btnEditableStandard
            // 
            btnEditableStandard.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnEditableStandard.Location = new Point(8, 16);
            btnEditableStandard.Name = "btnEditableStandard";
            btnEditableStandard.Size = new Size(544, 32);
            btnEditableStandard.TabIndex = 4;
            btnEditableStandard.Text = "Benutzer-Bearbeitung mit der <b>Standard-Methode</b> erlauben<br><i>Wenn neue Werte erlaubt sein sollen, muss hier ein Häkchen gesetzt werden.";
            // 
            // capUserGroupEdit
            // 
            capUserGroupEdit.CausesValidation = false;
            capUserGroupEdit.Location = new Point(576, 16);
            capUserGroupEdit.Name = "capUserGroupEdit";
            capUserGroupEdit.Size = new Size(328, 32);
            capUserGroupEdit.Text = "<b>Folgende Benutzergruppen dürfen den Inhalt der Zellen bearbeiten:";
            // 
            // btnEditableDropdown
            // 
            btnEditableDropdown.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnEditableDropdown.Location = new Point(8, 56);
            btnEditableDropdown.Name = "btnEditableDropdown";
            btnEditableDropdown.Size = new Size(544, 16);
            btnEditableDropdown.TabIndex = 5;
            btnEditableDropdown.Text = "Benutzer-Bearbeitung mit <b>Auswahl-Menü (Dropdown-Menü)</b> erlauben";
            // 
            // chkSaveContent
            // 
            chkSaveContent.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkSaveContent.Location = new Point(8, 40);
            chkSaveContent.Name = "chkSaveContent";
            chkSaveContent.Size = new Size(296, 16);
            chkSaveContent.TabIndex = 32;
            chkSaveContent.Text = "Inhalt wird auf Festplatte gespeichert";
            // 
            // tabAutoKorrektur
            // 
            tabAutoKorrektur.BackColor = Color.FromArgb(255, 255, 255);
            tabAutoKorrektur.Controls.Add(btnCalculateMaxCellLength);
            tabAutoKorrektur.Controls.Add(txbAutoReplace);
            tabAutoKorrektur.Controls.Add(capAutoReplace);
            tabAutoKorrektur.Controls.Add(txbMaxCellLength);
            tabAutoKorrektur.Controls.Add(txbAutoRemove);
            tabAutoKorrektur.Controls.Add(capAutoRemove);
            tabAutoKorrektur.Controls.Add(capMaxCellLength);
            tabAutoKorrektur.Controls.Add(btnAutoEditKleineFehler);
            tabAutoKorrektur.Controls.Add(btnAutoEditToUpper);
            tabAutoKorrektur.Controls.Add(txbRunden);
            tabAutoKorrektur.Controls.Add(capNachkommastellen);
            tabAutoKorrektur.Controls.Add(btnAutoEditAutoSort);
            tabAutoKorrektur.Location = new Point(4, 25);
            tabAutoKorrektur.Name = "tabAutoKorrektur";
            tabAutoKorrektur.Size = new Size(1098, 594);
            tabAutoKorrektur.TabIndex = 6;
            tabAutoKorrektur.Text = "Auto-Korrektur";
            // 
            // btnCalculateMaxCellLength
            // 
            btnCalculateMaxCellLength.ImageCode = "Taschenrechner|16";
            btnCalculateMaxCellLength.Location = new Point(312, 88);
            btnCalculateMaxCellLength.Name = "btnCalculateMaxCellLength";
            btnCalculateMaxCellLength.QuickInfo = "Prüft alle Zellen und berechnet die ideale\r\nmaximale Text Länge";
            btnCalculateMaxCellLength.Size = new Size(40, 24);
            btnCalculateMaxCellLength.TabIndex = 46;
            btnCalculateMaxCellLength.Click += btnCalculateMaxCellLength_Click;
            // 
            // txbAutoReplace
            // 
            txbAutoReplace.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbAutoReplace.Cursor = Cursors.IBeam;
            txbAutoReplace.Location = new Point(16, 192);
            txbAutoReplace.MultiLine = true;
            txbAutoReplace.Name = "txbAutoReplace";
            txbAutoReplace.RegexCheck = null;
            txbAutoReplace.Size = new Size(1073, 392);
            txbAutoReplace.SpellCheckingEnabled = true;
            txbAutoReplace.TabIndex = 39;
            // 
            // capAutoReplace
            // 
            capAutoReplace.CausesValidation = false;
            capAutoReplace.Location = new Point(16, 168);
            capAutoReplace.Name = "capAutoReplace";
            capAutoReplace.Size = new Size(184, 24);
            capAutoReplace.Text = "Permanente Ersetzungen:";
            // 
            // txbMaxCellLength
            // 
            txbMaxCellLength.AdditionalFormatCheck = AdditionalCheck.Integer;
            txbMaxCellLength.AllowedChars = "0123456789";
            txbMaxCellLength.Cursor = Cursors.IBeam;
            txbMaxCellLength.Location = new Point(216, 88);
            txbMaxCellLength.MaxTextLength = 255;
            txbMaxCellLength.Name = "txbMaxCellLength";
            txbMaxCellLength.RegexCheck = "^((-?[1-9]\\d*)|0)$";
            txbMaxCellLength.Size = new Size(96, 24);
            txbMaxCellLength.TabIndex = 45;
            // 
            // txbAutoRemove
            // 
            txbAutoRemove.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txbAutoRemove.Cursor = Cursors.IBeam;
            txbAutoRemove.Location = new Point(16, 136);
            txbAutoRemove.Name = "txbAutoRemove";
            txbAutoRemove.RegexCheck = null;
            txbAutoRemove.Size = new Size(1073, 24);
            txbAutoRemove.TabIndex = 37;
            // 
            // capAutoRemove
            // 
            capAutoRemove.CausesValidation = false;
            capAutoRemove.Location = new Point(16, 120);
            capAutoRemove.Name = "capAutoRemove";
            capAutoRemove.Size = new Size(568, 16);
            capAutoRemove.Text = "Folgende Zeichen automatisch aus der Eingabe löschen:";
            // 
            // capMaxCellLength
            // 
            capMaxCellLength.CausesValidation = false;
            capMaxCellLength.Location = new Point(16, 88);
            capMaxCellLength.Name = "capMaxCellLength";
            capMaxCellLength.QuickInfo = "Falls mehrere Zeilen erlaubt sind, pro Zeile.\r\nAber es sind niemals mehr als 4000 Zeichen erlaubt.\r\nDa im UTF8-Format gespeichert wird, evtl. auch weniger.";
            capMaxCellLength.Size = new Size(160, 24);
            capMaxCellLength.Text = "Maximale Zellen-Kapazität:";
            // 
            // btnAutoEditKleineFehler
            // 
            btnAutoEditKleineFehler.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnAutoEditKleineFehler.Location = new Point(440, 16);
            btnAutoEditKleineFehler.Name = "btnAutoEditKleineFehler";
            btnAutoEditKleineFehler.Size = new Size(440, 24);
            btnAutoEditKleineFehler.TabIndex = 13;
            btnAutoEditKleineFehler.Text = "Kleinere Fehler, wie z.B. doppelte Leerzeichen automatisch korrigieren";
            // 
            // btnAutoEditToUpper
            // 
            btnAutoEditToUpper.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnAutoEditToUpper.Location = new Point(16, 16);
            btnAutoEditToUpper.Name = "btnAutoEditToUpper";
            btnAutoEditToUpper.Size = new Size(208, 24);
            btnAutoEditToUpper.TabIndex = 12;
            btnAutoEditToUpper.Text = "Texte in Grossbuchstaben ändern";
            // 
            // txbRunden
            // 
            txbRunden.Cursor = Cursors.IBeam;
            txbRunden.Location = new Point(216, 48);
            txbRunden.Name = "txbRunden";
            txbRunden.RegexCheck = null;
            txbRunden.Size = new Size(96, 24);
            txbRunden.TabIndex = 11;
            // 
            // capNachkommastellen
            // 
            capNachkommastellen.CausesValidation = false;
            capNachkommastellen.Location = new Point(16, 48);
            capNachkommastellen.Name = "capNachkommastellen";
            capNachkommastellen.Size = new Size(200, 16);
            capNachkommastellen.Text = "Zahlen runden auf Kommastellen:";
            // 
            // btnAutoEditAutoSort
            // 
            btnAutoEditAutoSort.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnAutoEditAutoSort.Location = new Point(440, 40);
            btnAutoEditAutoSort.Name = "btnAutoEditAutoSort";
            btnAutoEditAutoSort.Size = new Size(416, 24);
            btnAutoEditAutoSort.TabIndex = 10;
            btnAutoEditAutoSort.Text = "Mehrzeilige Einträge sortieren und doppelte entfernen";
            // 
            // tabFilter
            // 
            tabFilter.BackColor = Color.FromArgb(255, 255, 255);
            tabFilter.Controls.Add(chkFilterOnlyOr);
            tabFilter.Controls.Add(chkFilterOnlyAND);
            tabFilter.Controls.Add(capJokerValue);
            tabFilter.Controls.Add(txbJoker);
            tabFilter.Controls.Add(btnZeilenFilterIgnorieren);
            tabFilter.Controls.Add(btnAutoFilterMoeglich);
            tabFilter.Controls.Add(btnAutoFilterTXTErlaubt);
            tabFilter.Controls.Add(btnAutoFilterErweitertErlaubt);
            tabFilter.Location = new Point(4, 25);
            tabFilter.Name = "tabFilter";
            tabFilter.Padding = new Padding(3);
            tabFilter.Size = new Size(1098, 594);
            tabFilter.TabIndex = 2;
            tabFilter.Text = "Filter";
            // 
            // chkFilterOnlyOr
            // 
            chkFilterOnlyOr.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkFilterOnlyOr.Location = new Point(32, 88);
            chkFilterOnlyOr.Name = "chkFilterOnlyOr";
            chkFilterOnlyOr.Size = new Size(192, 16);
            chkFilterOnlyOr.TabIndex = 35;
            chkFilterOnlyOr.Text = "nur <b>ODER</b>-Filterung erlauben";
            // 
            // chkFilterOnlyAND
            // 
            chkFilterOnlyAND.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkFilterOnlyAND.Location = new Point(32, 72);
            chkFilterOnlyAND.Name = "chkFilterOnlyAND";
            chkFilterOnlyAND.Size = new Size(192, 16);
            chkFilterOnlyAND.TabIndex = 34;
            chkFilterOnlyAND.Text = "nur <b>UND</b>-Filterung erlauben";
            // 
            // capJokerValue
            // 
            capJokerValue.CausesValidation = false;
            capJokerValue.Location = new Point(4, 177);
            capJokerValue.Name = "capJokerValue";
            capJokerValue.Size = new Size(312, 56);
            capJokerValue.Text = "Bei Autofilter-Aktionen, Zellen mit folgenden Inhalt <b>immer</b> anzeigen, wenn ein Wert gewählt wurde:<br>(Joker)";
            // 
            // txbJoker
            // 
            txbJoker.Cursor = Cursors.IBeam;
            txbJoker.Location = new Point(4, 233);
            txbJoker.Name = "txbJoker";
            txbJoker.RegexCheck = null;
            txbJoker.Size = new Size(312, 24);
            txbJoker.TabIndex = 7;
            // 
            // btnZeilenFilterIgnorieren
            // 
            btnZeilenFilterIgnorieren.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnZeilenFilterIgnorieren.Location = new Point(8, 144);
            btnZeilenFilterIgnorieren.Name = "btnZeilenFilterIgnorieren";
            btnZeilenFilterIgnorieren.Size = new Size(304, 16);
            btnZeilenFilterIgnorieren.TabIndex = 6;
            btnZeilenFilterIgnorieren.Text = "Bei Zeilenfilter ignorieren (Suchfeld-Eingabe)";
            // 
            // btnAutoFilterMoeglich
            // 
            btnAutoFilterMoeglich.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnAutoFilterMoeglich.Location = new Point(12, 15);
            btnAutoFilterMoeglich.Name = "btnAutoFilterMoeglich";
            btnAutoFilterMoeglich.Size = new Size(120, 16);
            btnAutoFilterMoeglich.TabIndex = 3;
            btnAutoFilterMoeglich.Text = "AutoFilter erlaubt";
            // 
            // btnAutoFilterTXTErlaubt
            // 
            btnAutoFilterTXTErlaubt.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnAutoFilterTXTErlaubt.Location = new Point(32, 32);
            btnAutoFilterTXTErlaubt.Name = "btnAutoFilterTXTErlaubt";
            btnAutoFilterTXTErlaubt.Size = new Size(208, 16);
            btnAutoFilterTXTErlaubt.TabIndex = 4;
            btnAutoFilterTXTErlaubt.Text = "AutoFilter - Texteingabe - erlaubt";
            // 
            // btnAutoFilterErweitertErlaubt
            // 
            btnAutoFilterErweitertErlaubt.ButtonStyle = ButtonStyle.Checkbox_Text;
            btnAutoFilterErweitertErlaubt.Location = new Point(32, 48);
            btnAutoFilterErweitertErlaubt.Name = "btnAutoFilterErweitertErlaubt";
            btnAutoFilterErweitertErlaubt.Size = new Size(192, 16);
            btnAutoFilterErweitertErlaubt.TabIndex = 5;
            btnAutoFilterErweitertErlaubt.Text = "AutoFilter - Erweitert - erlaubt";
            // 
            // tabQuickInfo
            // 
            tabQuickInfo.BackColor = Color.FromArgb(255, 255, 255);
            tabQuickInfo.Controls.Add(txbAdminInfo);
            tabQuickInfo.Controls.Add(txbQuickinfo);
            tabQuickInfo.Controls.Add(capAdminInfo);
            tabQuickInfo.Controls.Add(capQuickInfo);
            tabQuickInfo.Controls.Add(btnQI_Vorschau);
            tabQuickInfo.Location = new Point(4, 25);
            tabQuickInfo.Name = "tabQuickInfo";
            tabQuickInfo.Padding = new Padding(3);
            tabQuickInfo.Size = new Size(1098, 594);
            tabQuickInfo.TabIndex = 3;
            tabQuickInfo.Text = "Quickinfo";
            // 
            // txbAdminInfo
            // 
            txbAdminInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txbAdminInfo.Cursor = Cursors.IBeam;
            txbAdminInfo.Location = new Point(617, 24);
            txbAdminInfo.MultiLine = true;
            txbAdminInfo.Name = "txbAdminInfo";
            txbAdminInfo.RegexCheck = null;
            txbAdminInfo.Size = new Size(473, 528);
            txbAdminInfo.SpellCheckingEnabled = true;
            txbAdminInfo.TabIndex = 3;
            txbAdminInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbQuickinfo
            // 
            txbQuickinfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbQuickinfo.Cursor = Cursors.IBeam;
            txbQuickinfo.Location = new Point(8, 24);
            txbQuickinfo.MultiLine = true;
            txbQuickinfo.Name = "txbQuickinfo";
            txbQuickinfo.RegexCheck = null;
            txbQuickinfo.Size = new Size(601, 528);
            txbQuickinfo.SpellCheckingEnabled = true;
            txbQuickinfo.TabIndex = 0;
            txbQuickinfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAdminInfo
            // 
            capAdminInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            capAdminInfo.CausesValidation = false;
            capAdminInfo.Location = new Point(617, 8);
            capAdminInfo.Name = "capAdminInfo";
            capAdminInfo.Size = new Size(188, 15);
            capAdminInfo.Text = "Administrator-Info:";
            // 
            // capQuickInfo
            // 
            capQuickInfo.CausesValidation = false;
            capQuickInfo.Location = new Point(8, 8);
            capQuickInfo.Name = "capQuickInfo";
            capQuickInfo.Size = new Size(168, 16);
            capQuickInfo.Text = "QuickInfo:";
            // 
            // btnQI_Vorschau
            // 
            btnQI_Vorschau.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnQI_Vorschau.Location = new Point(992, 560);
            btnQI_Vorschau.Name = "btnQI_Vorschau";
            btnQI_Vorschau.Size = new Size(96, 24);
            btnQI_Vorschau.TabIndex = 1;
            btnQI_Vorschau.Text = "Vorschau";
            btnQI_Vorschau.Click += btnQI_Vorschau_Click;
            // 
            // tabSonstiges
            // 
            tabSonstiges.BackColor = Color.FromArgb(255, 255, 255);
            tabSonstiges.Controls.Add(txbTags);
            tabSonstiges.Controls.Add(capTags);
            tabSonstiges.Location = new Point(4, 25);
            tabSonstiges.Name = "tabSonstiges";
            tabSonstiges.Padding = new Padding(3);
            tabSonstiges.Size = new Size(1098, 594);
            tabSonstiges.TabIndex = 4;
            tabSonstiges.Text = "Sonstiges allgemein";
            // 
            // txbTags
            // 
            txbTags.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbTags.Cursor = Cursors.IBeam;
            txbTags.Location = new Point(8, 32);
            txbTags.MultiLine = true;
            txbTags.Name = "txbTags";
            txbTags.RegexCheck = null;
            txbTags.Size = new Size(1080, 552);
            txbTags.TabIndex = 30;
            txbTags.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capTags
            // 
            capTags.CausesValidation = false;
            capTags.Location = new Point(4, 15);
            capTags.Name = "capTags";
            capTags.Size = new Size(144, 16);
            capTags.Text = "Sonstige Daten (Tags):";
            // 
            // cbxSort
            // 
            cbxSort.Cursor = Cursors.IBeam;
            cbxSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxSort.Location = new Point(232, 328);
            cbxSort.Name = "cbxSort";
            cbxSort.RegexCheck = null;
            cbxSort.Size = new Size(304, 24);
            cbxSort.TabIndex = 35;
            // 
            // txbRegex
            // 
            txbRegex.Cursor = Cursors.IBeam;
            txbRegex.Location = new Point(8, 192);
            txbRegex.Name = "txbRegex";
            txbRegex.Size = new Size(744, 48);
            txbRegex.TabIndex = 9;
            // 
            // capSortiermaske
            // 
            capSortiermaske.CausesValidation = false;
            capSortiermaske.Location = new Point(8, 328);
            capSortiermaske.Name = "capSortiermaske";
            capSortiermaske.Size = new Size(216, 40);
            capSortiermaske.Text = "Bei der Tabellen-Zeilen-Sortierung fungiert diese Spalte als:";
            // 
            // capRegex
            // 
            capRegex.CausesValidation = false;
            capRegex.Location = new Point(8, 168);
            capRegex.Name = "capRegex";
            capRegex.Size = new Size(388, 24);
            capRegex.Text = "Die Eingabe muss mit dieser Regex-Maske übereinstimmen:";
            // 
            // txbAllowedChars
            // 
            txbAllowedChars.Cursor = Cursors.IBeam;
            txbAllowedChars.Location = new Point(8, 88);
            txbAllowedChars.Name = "txbAllowedChars";
            txbAllowedChars.RegexCheck = null;
            txbAllowedChars.Size = new Size(744, 56);
            txbAllowedChars.TabIndex = 30;
            txbAllowedChars.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capAllowedChars
            // 
            capAllowedChars.CausesValidation = false;
            capAllowedChars.Location = new Point(8, 72);
            capAllowedChars.Name = "capAllowedChars";
            capAllowedChars.Size = new Size(352, 24);
            capAllowedChars.Text = "Folgende Zeichen können vom Benutzer eingegeben werden:";
            // 
            // cbxLinkedTable
            // 
            cbxLinkedTable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbxLinkedTable.Cursor = Cursors.IBeam;
            cbxLinkedTable.Location = new Point(248, 48);
            cbxLinkedTable.Name = "cbxLinkedTable";
            cbxLinkedTable.RegexCheck = null;
            cbxLinkedTable.Size = new Size(833, 24);
            cbxLinkedTable.TabIndex = 38;
            cbxLinkedTable.TextChanged += cbxLinkedTable_TextChanged;
            // 
            // capLinkedTable
            // 
            capLinkedTable.CausesValidation = false;
            capLinkedTable.Location = new Point(8, 48);
            capLinkedTable.Name = "capLinkedTable";
            capLinkedTable.Size = new Size(152, 16);
            capLinkedTable.Text = "Vernküpfte Tabelle:";
            // 
            // BlueFrame1
            // 
            BlueFrame1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            BlueFrame1.BackColor = Color.FromArgb(240, 240, 240);
            BlueFrame1.CausesValidation = false;
            BlueFrame1.Controls.Add(capInfo);
            BlueFrame1.Controls.Add(capInternalName);
            BlueFrame1.Controls.Add(txbName);
            BlueFrame1.Controls.Add(capCaption);
            BlueFrame1.Controls.Add(txbCaption);
            BlueFrame1.Location = new Point(8, 24);
            BlueFrame1.Name = "BlueFrame1";
            BlueFrame1.Size = new Size(1102, 104);
            BlueFrame1.TabIndex = 16;
            BlueFrame1.TabStop = false;
            BlueFrame1.Text = "Allgemein";
            // 
            // capInfo
            // 
            capInfo.CausesValidation = false;
            capInfo.Location = new Point(8, 16);
            capInfo.Name = "capInfo";
            capInfo.Size = new Size(280, 19);
            capInfo.Text = "NR";
            // 
            // capInternalName
            // 
            capInternalName.CausesValidation = false;
            capInternalName.Location = new Point(8, 40);
            capInternalName.Name = "capInternalName";
            capInternalName.Size = new Size(136, 16);
            capInternalName.Text = "Interner Spaltename:";
            // 
            // txbName
            // 
            txbName.AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.[]()";
            txbName.Cursor = Cursors.IBeam;
            txbName.Location = new Point(8, 56);
            txbName.Name = "txbName";
            txbName.RegexCheck = null;
            txbName.Size = new Size(296, 24);
            txbName.TabIndex = 0;
            // 
            // capCaption
            // 
            capCaption.CausesValidation = false;
            capCaption.Location = new Point(312, 16);
            capCaption.Name = "capCaption";
            capCaption.Size = new Size(144, 16);
            capCaption.Text = "Angezeigte Beschriftung:";
            // 
            // txbCaption
            // 
            txbCaption.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txbCaption.Cursor = Cursors.IBeam;
            txbCaption.Location = new Point(312, 32);
            txbCaption.MultiLine = true;
            txbCaption.Name = "txbCaption";
            txbCaption.RegexCheck = null;
            txbCaption.Size = new Size(785, 64);
            txbCaption.TabIndex = 2;
            // 
            // btnSystemInfo
            // 
            btnSystemInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSystemInfo.Location = new Point(760, 88);
            btnSystemInfo.Name = "btnSystemInfo";
            btnSystemInfo.Size = new Size(328, 24);
            btnSystemInfo.TabIndex = 4;
            btnSystemInfo.Text = "Alle gesammelten Infos zurücksetzen";
            btnSystemInfo.Click += btnSystemInfo_Click;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.ImageCode = "Häkchen|16";
            btnOk.Location = new Point(1026, 765);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(72, 24);
            btnOk.TabIndex = 6;
            btnOk.Text = "OK";
            btnOk.Click += btnOk_Click;
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabDatenFormat);
            tabControl.Controls.Add(tabAnzeige);
            tabControl.Controls.Add(tabBearbeitung);
            tabControl.Controls.Add(tabAutoKorrektur);
            tabControl.Controls.Add(tabFilter);
            tabControl.Controls.Add(tabQuickInfo);
            tabControl.Controls.Add(tabSonstiges);
            tabControl.Controls.Add(tabSpaltenVerlinkung);
            tabControl.HotTrack = true;
            tabControl.Location = new Point(0, 136);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1106, 623);
            tabControl.TabDefault = tabDatenFormat;
            tabControl.TabDefaultOrder = null;
            tabControl.TabIndex = 15;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabDatenFormat
            // 
            tabDatenFormat.BackColor = Color.FromArgb(255, 255, 255);
            tabDatenFormat.Controls.Add(grpStyles);
            tabDatenFormat.Controls.Add(capInfos);
            tabDatenFormat.Controls.Add(btnSystemInfo);
            tabDatenFormat.Controls.Add(btnRequired);
            tabDatenFormat.Controls.Add(chkRelation);
            tabDatenFormat.Controls.Add(chkIsKeyColumn);
            tabDatenFormat.Controls.Add(chkSaveContent);
            tabDatenFormat.Controls.Add(chkIsFirst);
            tabDatenFormat.Controls.Add(btnMaxTextLength);
            tabDatenFormat.Controls.Add(txbMaxTextLength);
            tabDatenFormat.Controls.Add(capMaxTextLength);
            tabDatenFormat.Controls.Add(txbAllowedChars);
            tabDatenFormat.Controls.Add(cbxSort);
            tabDatenFormat.Controls.Add(capSortiermaske);
            tabDatenFormat.Controls.Add(txbRegex);
            tabDatenFormat.Controls.Add(capRegex);
            tabDatenFormat.Controls.Add(capAllowedChars);
            tabDatenFormat.Controls.Add(chkFormatierungErlaubt);
            tabDatenFormat.Controls.Add(cbxAdditionalCheck);
            tabDatenFormat.Controls.Add(capcbxAdditionalCheck);
            tabDatenFormat.Controls.Add(cbxScriptType);
            tabDatenFormat.Controls.Add(capScriptType);
            tabDatenFormat.Controls.Add(cbxChunk);
            tabDatenFormat.Controls.Add(capChunk);
            tabDatenFormat.Controls.Add(chkMultiline);
            tabDatenFormat.Location = new Point(4, 25);
            tabDatenFormat.Name = "tabDatenFormat";
            tabDatenFormat.Size = new Size(1098, 594);
            tabDatenFormat.TabIndex = 12;
            tabDatenFormat.Text = "Daten-Format";
            // 
            // grpStyles
            // 
            grpStyles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpStyles.BackColor = Color.FromArgb(255, 255, 255);
            grpStyles.Controls.Add(lstStyles);
            grpStyles.Location = new Point(8, 360);
            grpStyles.Name = "grpStyles";
            grpStyles.Size = new Size(744, 224);
            grpStyles.TabIndex = 55;
            grpStyles.TabStop = false;
            grpStyles.Text = "Schnellauswahl";
            // 
            // lstStyles
            // 
            lstStyles.AddAllowed = AddType.None;
            lstStyles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstStyles.Appearance = ListBoxAppearance.ButtonList;
            lstStyles.CheckBehavior = CheckBehavior.NoSelection;
            lstStyles.Location = new Point(8, 24);
            lstStyles.Name = "lstStyles";
            lstStyles.Size = new Size(728, 192);
            lstStyles.TabIndex = 54;
            lstStyles.ItemClicked += lstStyles_ItemClicked;
            // 
            // capInfos
            // 
            capInfos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            capInfos.CausesValidation = false;
            capInfos.Location = new Point(760, 112);
            capInfos.Name = "capInfos";
            capInfos.Size = new Size(328, 472);
            // 
            // chkRelation
            // 
            chkRelation.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkRelation.Location = new Point(312, 40);
            chkRelation.Name = "chkRelation";
            chkRelation.QuickInfo = "Ist in den Einträgen der Spalte ein Wert, der in ein der ersten Spalte wiederzufinden ist, werden die Einträge in beiden Feldern dieser Spalte dupliziert.";
            chkRelation.Size = new Size(336, 16);
            chkRelation.TabIndex = 51;
            chkRelation.Text = "Beziehungen automatisch mit erster Spalte abgleichen";
            // 
            // chkIsKeyColumn
            // 
            chkIsKeyColumn.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkIsKeyColumn.Location = new Point(8, 24);
            chkIsKeyColumn.Name = "chkIsKeyColumn";
            chkIsKeyColumn.QuickInfo = "Wenn der Imhalt dieser Spalte geändert wird,\r\nwird eine erweitertes Skript angestoßen.";
            chkIsKeyColumn.Size = new Size(296, 16);
            chkIsKeyColumn.TabIndex = 50;
            chkIsKeyColumn.Text = "Diese Spalte ist eine Schlüsselspalte";
            // 
            // chkIsFirst
            // 
            chkIsFirst.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkIsFirst.Location = new Point(8, 8);
            chkIsFirst.Name = "chkIsFirst";
            chkIsFirst.Size = new Size(296, 16);
            chkIsFirst.TabIndex = 49;
            chkIsFirst.Text = "Diese Spalte ist die funktionelle erste Spalte";
            // 
            // btnMaxTextLength
            // 
            btnMaxTextLength.ImageCode = "Taschenrechner|16";
            btnMaxTextLength.Location = new Point(656, 272);
            btnMaxTextLength.Name = "btnMaxTextLength";
            btnMaxTextLength.QuickInfo = "Prüft alle Zellen und berechnet die ideale\r\nmaximale Text Länge";
            btnMaxTextLength.Size = new Size(40, 24);
            btnMaxTextLength.TabIndex = 48;
            btnMaxTextLength.Click += btnMaxTextLength_Click;
            // 
            // txbMaxTextLength
            // 
            txbMaxTextLength.AdditionalFormatCheck = AdditionalCheck.Integer;
            txbMaxTextLength.AllowedChars = "0123456789";
            txbMaxTextLength.Cursor = Cursors.IBeam;
            txbMaxTextLength.Location = new Point(560, 272);
            txbMaxTextLength.MaxTextLength = 255;
            txbMaxTextLength.Name = "txbMaxTextLength";
            txbMaxTextLength.RegexCheck = "^((-?[1-9]\\d*)|0)$";
            txbMaxTextLength.Size = new Size(96, 24);
            txbMaxTextLength.TabIndex = 47;
            // 
            // capMaxTextLength
            // 
            capMaxTextLength.CausesValidation = false;
            capMaxTextLength.Location = new Point(560, 248);
            capMaxTextLength.Name = "capMaxTextLength";
            capMaxTextLength.QuickInfo = "Pro Zeile!\r\nEs wird wirklich die Anzahl der Zeichen gezählt.\r\nEs bezeht sich nur auf das Format, und es wird evtl. eine Meldung ausgegeben,\r\ndas die Eingabe nicht dem Format entspricht.";
            capMaxTextLength.Size = new Size(136, 24);
            capMaxTextLength.Text = "Maximale Text-Länge:";
            // 
            // tabSpaltenVerlinkung
            // 
            tabSpaltenVerlinkung.BackColor = Color.FromArgb(255, 255, 255);
            tabSpaltenVerlinkung.Controls.Add(cbxRelationType);
            tabSpaltenVerlinkung.Controls.Add(capOtherTable);
            tabSpaltenVerlinkung.Controls.Add(tblFilterliste);
            tabSpaltenVerlinkung.Controls.Add(cbxTargetColumn);
            tabSpaltenVerlinkung.Controls.Add(capLinkedTable);
            tabSpaltenVerlinkung.Controls.Add(capTargetColumn);
            tabSpaltenVerlinkung.Controls.Add(cbxLinkedTable);
            tabSpaltenVerlinkung.Location = new Point(4, 25);
            tabSpaltenVerlinkung.Name = "tabSpaltenVerlinkung";
            tabSpaltenVerlinkung.Size = new Size(1098, 594);
            tabSpaltenVerlinkung.TabIndex = 11;
            tabSpaltenVerlinkung.Text = "Spalten-Verlinkung";
            // 
            // cbxRelationType
            // 
            cbxRelationType.Cursor = Cursors.IBeam;
            cbxRelationType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxRelationType.Location = new Point(248, 8);
            cbxRelationType.Name = "cbxRelationType";
            cbxRelationType.RegexCheck = null;
            cbxRelationType.Size = new Size(232, 24);
            cbxRelationType.TabIndex = 41;
            // 
            // capOtherTable
            // 
            capOtherTable.CausesValidation = false;
            capOtherTable.Location = new Point(8, 8);
            capOtherTable.Name = "capOtherTable";
            capOtherTable.Size = new Size(232, 16);
            capOtherTable.Text = "Werte aus anderer Tabelle benutzen:";
            // 
            // tblFilterliste
            // 
            tblFilterliste.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tblFilterliste.Ansichtbearbeitung = false;
            tblFilterliste.Location = new Point(8, 112);
            tblFilterliste.Name = "tblFilterliste";
            tblFilterliste.PowerEdit = false;
            tblFilterliste.Size = new Size(1073, 472);
            tblFilterliste.TabIndex = 39;
            // 
            // cbxTargetColumn
            // 
            cbxTargetColumn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbxTargetColumn.Cursor = Cursors.IBeam;
            cbxTargetColumn.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxTargetColumn.Location = new Point(248, 80);
            cbxTargetColumn.Name = "cbxTargetColumn";
            cbxTargetColumn.RegexCheck = null;
            cbxTargetColumn.Size = new Size(833, 24);
            cbxTargetColumn.TabIndex = 5;
            cbxTargetColumn.TextChanged += cbxTargetColumn_TextChanged;
            // 
            // capTargetColumn
            // 
            capTargetColumn.CausesValidation = false;
            capTargetColumn.Location = new Point(8, 80);
            capTargetColumn.Name = "capTargetColumn";
            capTargetColumn.Size = new Size(200, 16);
            capTargetColumn.Text = "Aus dieser Spalte die Werte holen:";
            // 
            // capCurrentView
            // 
            capCurrentView.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            capCurrentView.CausesValidation = false;
            capCurrentView.Location = new Point(185, 765);
            capCurrentView.Name = "capCurrentView";
            capCurrentView.Size = new Size(104, 24);
            capCurrentView.Text = "Aktuelle Ansicht:";
            // 
            // butAktuellVor
            // 
            butAktuellVor.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            butAktuellVor.ImageCode = "Pfeil_Rechts|16|||0000FF";
            butAktuellVor.Location = new Point(377, 765);
            butAktuellVor.Name = "butAktuellVor";
            butAktuellVor.Size = new Size(72, 24);
            butAktuellVor.TabIndex = 19;
            butAktuellVor.Click += butAktuellVor_Click;
            // 
            // butAktuellZurueck
            // 
            butAktuellZurueck.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            butAktuellZurueck.ImageCode = "Pfeil_Links|16|||0000FF";
            butAktuellZurueck.Location = new Point(297, 765);
            butAktuellZurueck.Name = "butAktuellZurueck";
            butAktuellZurueck.Size = new Size(72, 24);
            butAktuellZurueck.TabIndex = 18;
            butAktuellZurueck.Click += butAktuellZurueck_Click;
            // 
            // capTabellenname
            // 
            capTabellenname.CausesValidation = false;
            capTabellenname.Location = new Point(40, 0);
            capTabellenname.Name = "capTabellenname";
            capTabellenname.Size = new Size(940, 24);
            capTabellenname.Text = "Tabellenname:";
            capTabellenname.Translate = false;
            // 
            // btnSpaltenkopf
            // 
            btnSpaltenkopf.ImageCode = "Stift|16";
            btnSpaltenkopf.Location = new Point(8, 0);
            btnSpaltenkopf.Name = "btnSpaltenkopf";
            btnSpaltenkopf.QuickInfo = "Spaltenkopf bearbeiten";
            btnSpaltenkopf.Size = new Size(32, 24);
            btnSpaltenkopf.TabIndex = 49;
            btnSpaltenkopf.Click += btnSpaltenkopf_Click;
            // 
            // ColumnEditor
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1112, 796);
            Controls.Add(btnSpaltenkopf);
            Controls.Add(capTabellenname);
            Controls.Add(capCurrentView);
            Controls.Add(butAktuellVor);
            Controls.Add(butAktuellZurueck);
            Controls.Add(tabControl);
            Controls.Add(BlueFrame1);
            Controls.Add(btnOk);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "ColumnEditor";
            Text = "Spalten-Eigenschaften";
            tabAnzeige.ResumeLayout(false);
            tabBearbeitung.ResumeLayout(false);
            grpAuswahlmenuOptionen.ResumeLayout(false);
            tabAutoKorrektur.ResumeLayout(false);
            tabFilter.ResumeLayout(false);
            tabQuickInfo.ResumeLayout(false);
            tabSonstiges.ResumeLayout(false);
            BlueFrame1.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabDatenFormat.ResumeLayout(false);
            grpStyles.ResumeLayout(false);
            tabSpaltenVerlinkung.ResumeLayout(false);
            ResumeLayout(false);

        }
        private Button btnOk;
        private TextBox txbName;
        private Caption capInternalName;
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
        private Caption capCaption;
        private TextBox txbCaption;
        private GroupBox BlueFrame1;
        private Button btnEditableDropdown;
        private Button btnEditableStandard;
        private TextBox txbAuswaehlbareWerte;
        private Button btnRequired;
        private Caption capUserGroupEdit;
        private Caption capTags;
        private ComboBox cbxRandRechts;
        private ComboBox cbxRandLinks;
        private Caption capAllowedChars;
        private TextBox txbAllowedChars;
        private Caption capImmerWerte;
        private Caption capChunk;
        private Caption capAdminInfo;
        private TextBox txbAdminInfo;
        private Caption capQuickInfo;
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
        private ComboBox cbxLinkedTable;
        private Caption capLinkedTable;
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
        private Caption capCurrentView;
        private Button butAktuellVor;
        private Button butAktuellZurueck;
        private TextBox txbAutoReplace;
        private Caption capAutoReplace;
        private Button chkFilterOnlyAND;
        private Button chkFilterOnlyOr;
        private TextBox txbSpaltenbild;
        private ComboBox cbxSort;
        private ComboBox cbxTranslate;
        private Caption capTranslate;
        private Button chkFormatierungErlaubt;
        private ComboBox cbxAdditionalCheck;
        private Caption capcbxAdditionalCheck;
        private ComboBox cbxScriptType;
        private Caption capScriptType;
        private TabPage tabDatenFormat;
        private TabPage tabSpaltenVerlinkung;
        private TableView tblFilterliste;
        private TextBox txbMaxCellLength;
        private Caption capMaxCellLength;
        private Button btnCalculateMaxCellLength;
        private Caption capTabellenname;
        private TextBox txbFixedColumnWidth;
        private Caption capFixedColumnWidth;
        private TextBox txbMaxTextLength;
        private Caption capMaxTextLength;
        private Button btnMaxTextLength;
        private Button btnSpaltenkopf;
        private ComboBox cbxRenderer;
        private Caption capRenderer;
        private ScrollPanel RendererEditor;
        private Button btnSystemInfo;
        private Button chkIsKeyColumn;
        private Button chkIsFirst;
        private Button chkRelation;
        private ComboBox cbxRelationType;
        private Caption capOtherTable;
        private Caption capHintergrund;
        private ComboBox cbxBackground;
        private Caption capInfos;
        private ListBox lstStyles;
        private GroupBox grpStyles;
    }
}
