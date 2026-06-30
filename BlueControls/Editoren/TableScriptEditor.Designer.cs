// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.Diagnostics;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class TableScriptEditor  {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            tabKopfdaten = new TabPage();
            capLaufzeit = new Caption();
            capFehler = new Caption();
            txbQuickInfo = new TextBox();
            cbxPic = new ComboBox();
            capQuickInfo = new Caption();
            capImage = new Caption();
            grpRechte = new GroupBox();
            lstPermissionExecute = new ListBox();
            grpEigenschaften = new GroupBox();
            chkReadOnly = new Button();
            chkZeile = new Button();
            grpAuslöser = new GroupBox();
            chkAuslöser_deletingRow = new Button();
            chkAuslöser_export = new Button();
            chkAuslöser_newrow = new Button();
            chkAuslöser_valuechangedThread = new Button();
            chkAuslöser_valuechanged = new Button();
            chkAuslöser_prepaireformula = new Button();
            btnVerlauf = new Button();
            capName = new Caption();
            txbName = new TextBox();
            grpRow = new GroupBox();
            txbChunk = new TextBox();
            capChunk = new Caption();
            cpZeile = new Caption();
            txbTestZeile = new TextBox();
            chkExtendend = new Button();
            btnTest = new Button();
            grpVerfügbareSkripte = new GroupBox();
            lstEventScripts = new ListBox();
            btnVersionErhöhen = new Button();
            btnTabelleKopf = new Button();
            btnSpaltenuebersicht = new Button();
            btnZusatzDateien = new Button();
            tbcScriptEigenschaften.SuspendLayout();
            tabStart.SuspendLayout();
            grpInjectVariables.SuspendLayout();
            pnlStatusBar.SuspendLayout();
            tabKopfdaten.SuspendLayout();
            grpRechte.SuspendLayout();
            grpEigenschaften.SuspendLayout();
            grpAuslöser.SuspendLayout();
            grpRow.SuspendLayout();
            grpVerfügbareSkripte.SuspendLayout();
            SuspendLayout();
            // 
            // btnAusführen
            // 
            btnAusführen.ImageCode = "Abspielen|16|||ffff00||||||Warnung";
            btnAusführen.Location = new Point(112, 8);
            // 
            // tbcScriptEigenschaften
            // 
            tbcScriptEigenschaften.Controls.Add(tabKopfdaten);
            tbcScriptEigenschaften.Location = new Point(237, 108);
            tbcScriptEigenschaften.Size = new Size(1015, 478);
            tbcScriptEigenschaften.TabDefaultOrder = new string[]
    {
    "Skript-Editor",
    "Kopfdaten",
    "Befehls-Assistent"
    };
            tbcScriptEigenschaften.TabIndex = 0;
            tbcScriptEigenschaften.SelectedIndexChanged += GlobalTab_SelectedIndexChanged;
            tbcScriptEigenschaften.Controls.SetChildIndex(tabKopfdaten, 0);
            // 
            // tabStart
            // 
            tabStart.Controls.Add(btnZusatzDateien);
            tabStart.Controls.Add(btnSpaltenuebersicht);
            tabStart.Controls.Add(btnTabelleKopf);
            tabStart.Controls.Add(btnTest);
            tabStart.Controls.Add(btnVersionErhöhen);
            tabStart.Size = new Size(1252, 36);
            tabStart.Controls.SetChildIndex(btnSaveLoad, 0);
            tabStart.Controls.SetChildIndex(btnBefehlsUebersicht, 0);
            tabStart.Controls.SetChildIndex(btnAusführen, 0);
            tabStart.Controls.SetChildIndex(btnVersionErhöhen, 0);
            tabStart.Controls.SetChildIndex(btnTest, 0);
            tabStart.Controls.SetChildIndex(btnTabelleKopf, 0);
            tabStart.Controls.SetChildIndex(btnSpaltenuebersicht, 0);
            tabStart.Controls.SetChildIndex(btnZusatzDateien, 0);
            // 
            // grpInjectVariables
            // 
            grpInjectVariables.Controls.Add(chkExtendend);
            grpInjectVariables.Location = new Point(0, 72);
            grpInjectVariables.Size = new Size(1252, 36);
            grpInjectVariables.Controls.SetChildIndex(chkExtendend, 0);
            // 
            // btnBefehlsUebersicht
            // 
            btnBefehlsUebersicht.Location = new Point(224, 8);
            // 
            // btnSaveLoad
            // 
            btnSaveLoad.Location = new Point(504, 8);
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(1252, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(0, 586);
            pnlStatusBar.Size = new Size(1252, 24);
            // 
            // tabKopfdaten
            // 
            tabKopfdaten.BackColor = Color.FromArgb(255, 255, 255);
            tabKopfdaten.Controls.Add(capLaufzeit);
            tabKopfdaten.Controls.Add(capFehler);
            tabKopfdaten.Controls.Add(txbQuickInfo);
            tabKopfdaten.Controls.Add(cbxPic);
            tabKopfdaten.Controls.Add(capQuickInfo);
            tabKopfdaten.Controls.Add(capImage);
            tabKopfdaten.Controls.Add(grpRechte);
            tabKopfdaten.Controls.Add(grpEigenschaften);
            tabKopfdaten.Controls.Add(grpAuslöser);
            tabKopfdaten.Controls.Add(btnVerlauf);
            tabKopfdaten.Controls.Add(capName);
            tabKopfdaten.Controls.Add(txbName);
            tabKopfdaten.Location = new Point(4, 25);
            tabKopfdaten.Name = "tabKopfdaten";
            tabKopfdaten.Padding = new Padding(3);
            tabKopfdaten.Size = new Size(776, 428);
            tabKopfdaten.TabIndex = 0;
            tabKopfdaten.Text = "Kopfdaten";
            // 
            // capLaufzeit
            // 
            capLaufzeit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capLaufzeit.CausesValidation = false;
            capLaufzeit.Location = new Point(704, 184);
            capLaufzeit.Name = "capLaufzeit";
            capLaufzeit.Size = new Size(66, 88);
            // 
            // capFehler
            // 
            capFehler.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capFehler.CausesValidation = false;
            capFehler.Location = new Point(704, 80);
            capFehler.Name = "capFehler";
            capFehler.Size = new Size(66, 88);
            // 
            // txbQuickInfo
            // 
            txbQuickInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbQuickInfo.Cursor = Cursors.IBeam;
            txbQuickInfo.Location = new Point(8, 304);
            txbQuickInfo.MultiLine = true;
            txbQuickInfo.Name = "txbQuickInfo";
            txbQuickInfo.RaiseChangeDelay = 5;
            txbQuickInfo.Size = new Size(668, 96);
            txbQuickInfo.TabIndex = 28;
            txbQuickInfo.TextChanged += txbQuickInfo_TextChanged;
            // 
            // cbxPic
            // 
            cbxPic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbxPic.Cursor = Cursors.IBeam;
            cbxPic.Location = new Point(436, 32);
            cbxPic.Name = "cbxPic";
            cbxPic.RaiseChangeDelay = 5;
            cbxPic.Size = new Size(256, 24);
            cbxPic.TabIndex = 27;
            cbxPic.TextChanged += cbxPic_TextChanged;
            // 
            // capQuickInfo
            // 
            capQuickInfo.CausesValidation = false;
            capQuickInfo.Location = new Point(8, 280);
            capQuickInfo.Name = "capQuickInfo";
            capQuickInfo.Size = new Size(152, 16);
            capQuickInfo.Text = "QuickInfo:";
            // 
            // capImage
            // 
            capImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            capImage.CausesValidation = false;
            capImage.Location = new Point(436, 8);
            capImage.Name = "capImage";
            capImage.Size = new Size(152, 24);
            capImage.Text = "Bild:";
            // 
            // grpRechte
            // 
            grpRechte.BackColor = Color.FromArgb(255, 255, 255);
            grpRechte.Controls.Add(lstPermissionExecute);
            grpRechte.Location = new Point(472, 80);
            grpRechte.Name = "grpRechte";
            grpRechte.Size = new Size(224, 192);
            grpRechte.TabIndex = 25;
            grpRechte.TabStop = false;
            grpRechte.Text = "Rechte";
            // 
            // lstPermissionExecute
            // 
            lstPermissionExecute.AddAllowed = AddType.Text;
            lstPermissionExecute.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstPermissionExecute.Appearance = ListBoxAppearance.Listbox_Boxes;
            lstPermissionExecute.CheckBehavior = CheckBehavior.MultiSelection;
            lstPermissionExecute.FilterText = null;
            lstPermissionExecute.Location = new Point(8, 16);
            lstPermissionExecute.Name = "lstPermissionExecute";
            lstPermissionExecute.RemoveAllowed = true;
            lstPermissionExecute.Size = new Size(208, 168);
            lstPermissionExecute.TabIndex = 18;
            lstPermissionExecute.Translate = false;
            lstPermissionExecute.ItemClicked += lstPermissionExecute_ItemClicked;
            // 
            // grpEigenschaften
            // 
            grpEigenschaften.BackColor = Color.FromArgb(255, 255, 255);
            grpEigenschaften.Controls.Add(chkReadOnly);
            grpEigenschaften.Controls.Add(chkZeile);
            grpEigenschaften.Location = new Point(8, 64);
            grpEigenschaften.Name = "grpEigenschaften";
            grpEigenschaften.Size = new Size(208, 208);
            grpEigenschaften.TabIndex = 24;
            grpEigenschaften.TabStop = false;
            grpEigenschaften.Text = "Eigenschaften";
            // 
            // chkReadOnly
            // 
            chkReadOnly.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkReadOnly.Location = new Point(8, 64);
            chkReadOnly.Name = "chkReadOnly";
            chkReadOnly.QuickInfo = "Schreibgeschützte Skripte können auch die eigene Zeile ändern";
            chkReadOnly.Size = new Size(120, 16);
            chkReadOnly.TabIndex = 15;
            chkReadOnly.Text = "Schreibgeschützt";
            chkReadOnly.CheckedChanged += chkReadOnly_CheckedChanged;
            // 
            // chkZeile
            // 
            chkZeile.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkZeile.Location = new Point(8, 40);
            chkZeile.Name = "chkZeile";
            chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile anzeigen/ändern können soll,\r\nmuss dieses Häkchen gesetzt sein.";
            chkZeile.Size = new Size(88, 16);
            chkZeile.TabIndex = 14;
            chkZeile.Text = "Zeilen-Skript";
            chkZeile.CheckedChanged += chkZeile_CheckedChanged;
            // 
            // grpAuslöser
            // 
            grpAuslöser.BackColor = Color.FromArgb(255, 255, 255);
            grpAuslöser.Controls.Add(chkAuslöser_deletingRow);
            grpAuslöser.Controls.Add(chkAuslöser_export);
            grpAuslöser.Controls.Add(chkAuslöser_newrow);
            grpAuslöser.Controls.Add(chkAuslöser_valuechangedThread);
            grpAuslöser.Controls.Add(chkAuslöser_valuechanged);
            grpAuslöser.Controls.Add(chkAuslöser_prepaireformula);
            grpAuslöser.Location = new Point(224, 64);
            grpAuslöser.Name = "grpAuslöser";
            grpAuslöser.Size = new Size(240, 208);
            grpAuslöser.TabIndex = 23;
            grpAuslöser.TabStop = false;
            grpAuslöser.Text = "Auslöser";
            // 
            // chkAuslöser_deletingRow
            // 
            chkAuslöser_deletingRow.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAuslöser_deletingRow.Location = new Point(8, 120);
            chkAuslöser_deletingRow.Name = "chkAuslöser_deletingRow";
            chkAuslöser_deletingRow.QuickInfo = "Das Skript wird ausgeführt, bevor eine Zeile gelöscht wird.";
            chkAuslöser_deletingRow.Size = new Size(176, 16);
            chkAuslöser_deletingRow.TabIndex = 24;
            chkAuslöser_deletingRow.Text = "Zeile wird gelöscht";
            chkAuslöser_deletingRow.CheckedChanged += chkAuslöser_newrow_CheckedChanged;
            // 
            // chkAuslöser_export
            // 
            chkAuslöser_export.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAuslöser_export.Location = new Point(8, 176);
            chkAuslöser_export.Name = "chkAuslöser_export";
            chkAuslöser_export.QuickInfo = "Das Skript wird vor einem Export ausgeführt.\r\n\r\nEs kann dazu verwendet werden, um Werte temporär zu ändern,\r\nVariablen hinzuzufügen oder Bilder zu laden.";
            chkAuslöser_export.Size = new Size(176, 16);
            chkAuslöser_export.TabIndex = 22;
            chkAuslöser_export.Text = "Export";
            chkAuslöser_export.CheckedChanged += chkAuslöser_newrow_CheckedChanged;
            // 
            // chkAuslöser_newrow
            // 
            chkAuslöser_newrow.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAuslöser_newrow.Location = new Point(8, 48);
            chkAuslöser_newrow.Name = "chkAuslöser_newrow";
            chkAuslöser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgeführt.\r\nMit diesem Skript können Initialwerte\r\neiner Zeile ergänzt werden.";
            chkAuslöser_newrow.Size = new Size(176, 16);
            chkAuslöser_newrow.TabIndex = 17;
            chkAuslöser_newrow.Text = "Zeile initialisieren";
            chkAuslöser_newrow.CheckedChanged += chkAuslöser_newrow_CheckedChanged;
            // 
            // chkAuslöser_valuechangedThread
            // 
            chkAuslöser_valuechangedThread.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAuslöser_valuechangedThread.Location = new Point(8, 96);
            chkAuslöser_valuechangedThread.Name = "chkAuslöser_valuechangedThread";
            chkAuslöser_valuechangedThread.QuickInfo = "Das Skript wird irgendwann im Hintergrund nach einer Zelländerung ausgeführt und ist nicht sehr zuverlässig.\r\nKann dazu benutzt werden, um Exporte auszuführen.";
            chkAuslöser_valuechangedThread.Size = new Size(176, 16);
            chkAuslöser_valuechangedThread.TabIndex = 20;
            chkAuslöser_valuechangedThread.Text = "Wert geändert <b><fontsize=8><i>Extra Thread";
            chkAuslöser_valuechangedThread.CheckedChanged += chkAuslöser_newrow_CheckedChanged;
            // 
            // chkAuslöser_valuechanged
            // 
            chkAuslöser_valuechanged.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAuslöser_valuechanged.Location = new Point(8, 80);
            chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            chkAuslöser_valuechanged.Size = new Size(176, 16);
            chkAuslöser_valuechanged.TabIndex = 18;
            chkAuslöser_valuechanged.Text = "Wert geändert";
            chkAuslöser_valuechanged.CheckedChanged += chkAuslöser_newrow_CheckedChanged;
            // 
            // chkAuslöser_prepaireformula
            // 
            chkAuslöser_prepaireformula.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkAuslöser_prepaireformula.Location = new Point(8, 160);
            chkAuslöser_prepaireformula.Name = "chkAuslöser_prepaireformula";
            chkAuslöser_prepaireformula.Size = new Size(175, 16);
            chkAuslöser_prepaireformula.TabIndex = 19;
            chkAuslöser_prepaireformula.Text = "Formular vorbereiten";
            chkAuslöser_prepaireformula.CheckedChanged += chkAuslöser_newrow_CheckedChanged;
            // 
            // btnVerlauf
            // 
            btnVerlauf.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnVerlauf.Enabled = false;
            btnVerlauf.ImageCode = "Undo|16";
            btnVerlauf.Location = new Point(665, 8);
            btnVerlauf.Name = "btnVerlauf";
            btnVerlauf.QuickInfo = "Zeigt den Verlauf in einem\r\nseparatem Fenster an";
            btnVerlauf.Size = new Size(100, 24);
            btnVerlauf.TabIndex = 1;
            btnVerlauf.Text = "Verlauf";
            btnVerlauf.Click += btnVerlauf_Click;
            // 
            // capName
            // 
            capName.CausesValidation = false;
            capName.Location = new Point(8, 8);
            capName.Name = "capName";
            capName.Size = new Size(56, 22);
            capName.Text = "Name:";
            // 
            // txbName
            // 
            txbName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txbName.Cursor = Cursors.IBeam;
            txbName.Location = new Point(8, 32);
            txbName.Name = "txbName";
            txbName.RaiseChangeDelay = 5;
            txbName.Size = new Size(412, 24);
            txbName.TabIndex = 13;
            txbName.TextChanged += txbName_TextChanged;
            // 
            // grpRow
            // 
            grpRow.BackColor = Color.FromArgb(255, 255, 255);
            grpRow.Controls.Add(txbChunk);
            grpRow.Controls.Add(capChunk);
            grpRow.Controls.Add(cpZeile);
            grpRow.Controls.Add(txbTestZeile);
            grpRow.Dock = DockStyle.Top;
            grpRow.GroupBoxStyle = GroupBoxStyle.RoundRect;
            grpRow.Location = new Point(0, 36);
            grpRow.Name = "grpRow";
            grpRow.Size = new Size(1252, 36);
            grpRow.TabIndex = 3;
            grpRow.TabStop = false;
            grpRow.Text = "Zeile und Chunk";
            // 
            // txbChunk
            // 
            txbChunk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txbChunk.Cursor = Cursors.IBeam;
            txbChunk.Enabled = false;
            txbChunk.Location = new Point(828, 8);
            txbChunk.Name = "txbChunk";
            txbChunk.RaiseChangeDelay = 10;
            txbChunk.Size = new Size(416, 22);
            txbChunk.TabIndex = 9;
            txbChunk.TextChanged += txbChunk_TextChanged;
            // 
            // capChunk
            // 
            capChunk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            capChunk.CausesValidation = false;
            capChunk.Enabled = false;
            capChunk.Location = new Point(740, 8);
            capChunk.Name = "capChunk";
            capChunk.Size = new Size(80, 22);
            capChunk.Text = "Chunk-Wert:";
            // 
            // cpZeile
            // 
            cpZeile.CausesValidation = false;
            cpZeile.Location = new Point(8, 8);
            cpZeile.Name = "cpZeile";
            cpZeile.Size = new Size(112, 22);
            cpZeile.Text = "Betreffende Zeile:";
            // 
            // txbTestZeile
            // 
            txbTestZeile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txbTestZeile.Cursor = Cursors.IBeam;
            txbTestZeile.Location = new Point(128, 8);
            txbTestZeile.Name = "txbTestZeile";
            txbTestZeile.RaiseChangeDelay = 5;
            txbTestZeile.Size = new Size(604, 22);
            txbTestZeile.TabIndex = 7;
            // 
            // chkExtendend
            // 
            chkExtendend.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkExtendend.Location = new Point(40, 8);
            chkExtendend.Name = "chkExtendend";
            chkExtendend.Size = new Size(152, 24);
            chkExtendend.TabIndex = 8;
            chkExtendend.Tag = "";
            chkExtendend.Text = "Erweiterte Ausführung";
            // 
            // btnTest
            // 
            btnTest.ImageCode = "Abspielen|16";
            btnTest.Location = new Point(8, 8);
            btnTest.Name = "btnTest";
            btnTest.QuickInfo = "Keine Änderung der Daten\r\nin den Tabellen.";
            btnTest.Size = new Size(96, 24);
            btnTest.TabIndex = 9;
            btnTest.Text = "Testen";
            btnTest.Click += btnTest_Click;
            // 
            // grpVerfügbareSkripte
            // 
            grpVerfügbareSkripte.BackColor = Color.FromArgb(240, 240, 240);
            grpVerfügbareSkripte.CausesValidation = false;
            grpVerfügbareSkripte.Controls.Add(lstEventScripts);
            grpVerfügbareSkripte.Dock = DockStyle.Left;
            grpVerfügbareSkripte.Location = new Point(0, 108);
            grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            grpVerfügbareSkripte.Size = new Size(237, 478);
            grpVerfügbareSkripte.TabIndex = 2;
            grpVerfügbareSkripte.TabStop = false;
            grpVerfügbareSkripte.Text = "Verfügbare Skripte:";
            // 
            // lstEventScripts
            // 
            lstEventScripts.AddAllowed = AddType.Suggestions;
            lstEventScripts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstEventScripts.Location = new Point(8, 16);
            lstEventScripts.Name = "lstEventScripts";
            lstEventScripts.RemoveAllowed = true;
            lstEventScripts.Size = new Size(222, 455);
            lstEventScripts.TabIndex = 0;
            lstEventScripts.AddClicked += lstEventScripts_AddClicked;
            lstEventScripts.ItemCheckedChanged += lstEventScripts_ItemCheckedChanged;
            lstEventScripts.RemoveClicked += lstEventScripts_RemoveClicked;
            // 
            // btnVersionErhöhen
            // 
            btnVersionErhöhen.ImageCode = "Pfeil_Oben|16|||||85|0";
            btnVersionErhöhen.Location = new Point(360, 8);
            btnVersionErhöhen.Name = "btnVersionErhöhen";
            btnVersionErhöhen.QuickInfo = "Skript-Version erhöhen bewirkt,\r\ndass alle Zeilen neu durchgerechnet\r\nwerden.";
            btnVersionErhöhen.Size = new Size(120, 24);
            btnVersionErhöhen.TabIndex = 37;
            btnVersionErhöhen.Text = "Version erhöhen";
            btnVersionErhöhen.Click += btnVersionErhöhen_Click;
            // 
            // btnTabelleKopf
            // 
            btnTabelleKopf.ImageCode = "Tabelle|16||||||||Stift";
            btnTabelleKopf.Location = new Point(624, 8);
            btnTabelleKopf.Name = "btnTabelleKopf";
            btnTabelleKopf.Size = new Size(120, 24);
            btnTabelleKopf.TabIndex = 45;
            btnTabelleKopf.Text = "Tabellen-Kopf";
            btnTabelleKopf.Click += btnTabelleKopf_Click;
            // 
            // btnSpaltenuebersicht
            // 
            btnSpaltenuebersicht.ImageCode = "Spalte|16||||||||Information";
            btnSpaltenuebersicht.Location = new Point(760, 8);
            btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            btnSpaltenuebersicht.Size = new Size(140, 24);
            btnSpaltenuebersicht.TabIndex = 44;
            btnSpaltenuebersicht.Text = "Spaltenübersicht";
            btnSpaltenuebersicht.Click += btnSpaltenuebersicht_Click;
            // 
            // btnZusatzDateien
            // 
            btnZusatzDateien.ImageCode = "Ordner|16";
            btnZusatzDateien.Location = new Point(904, 8);
            btnZusatzDateien.Name = "btnZusatzDateien";
            btnZusatzDateien.QuickInfo = "Den Ordner der Zusatzdateien öffnen.\r\nIn diesen können z.B. Skript-Routinen enthalten sein\r\ndie mit CallByFilename aufgerufen werden können.";
            btnZusatzDateien.Size = new Size(120, 24);
            btnZusatzDateien.TabIndex = 5;
            btnZusatzDateien.Text = "Zusatzdateien";
            btnZusatzDateien.Click += btnZusatzDateien_Click;
            // 
            // TableScriptEditor
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1252, 610);
            Controls.Add(grpVerfügbareSkripte);
            Controls.Add(grpRow);
            Name = "TableScriptEditor";
            Text = "Tabellen-Eigenschaften";
            VariableDefinitions = "Attribut0, Attribut1, Attribut2, Attribut3, Attribut4, Attribut5";
            Controls.SetChildIndex(tabStart, 0);
            Controls.SetChildIndex(grpRow, 0);
            Controls.SetChildIndex(grpInjectVariables, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(grpVerfügbareSkripte, 0);
            Controls.SetChildIndex(tbcScriptEigenschaften, 0);
            tbcScriptEigenschaften.ResumeLayout(false);
            tabStart.ResumeLayout(false);
            grpInjectVariables.ResumeLayout(false);
            pnlStatusBar.ResumeLayout(false);
            tabKopfdaten.ResumeLayout(false);
            grpRechte.ResumeLayout(false);
            grpEigenschaften.ResumeLayout(false);
            grpAuslöser.ResumeLayout(false);
            grpRow.ResumeLayout(false);
            grpVerfügbareSkripte.ResumeLayout(false);
            ResumeLayout(false);

        }


        private GroupBox grpRow;
        private GroupBox grpVerfügbareSkripte;
        private ListBox lstEventScripts;
        private Button chkAuslöser_prepaireformula;
        private Button chkAuslöser_valuechanged;
        private Button chkAuslöser_newrow;
        private Button chkZeile;
        private TextBox txbName;
        private Caption capName;
        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button chkAuslöser_valuechangedThread;
        private Button chkAuslöser_export;
        private Button btnVersionErhöhen;
        private Button btnVerlauf;
        private Button btnZusatzDateien;
        private Button btnTest;
        private TabPage tabKopfdaten;
        private Button btnSpaltenuebersicht;
        private Button btnTabelleKopf;
        private GroupBox grpAuslöser;
        private GroupBox grpRechte;
        private GroupBox grpEigenschaften;
        private ListBox lstPermissionExecute;
        private ComboBox cbxPic;
        private Caption capQuickInfo;
        private Caption capImage;
        private TextBox txbQuickInfo;
        private Caption capFehler;
        private Button chkAuslöser_deletingRow;
        private Button chkExtendend;
        private Button chkReadOnly;
        private Caption capLaufzeit;
        private Caption capChunk;
        private TextBox txbChunk;
    }
}
