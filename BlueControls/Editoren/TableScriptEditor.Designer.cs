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
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.tabKopfdaten = new System.Windows.Forms.TabPage();
            this.capLaufzeit = new BlueControls.Controls.Caption();
            this.capFehler = new BlueControls.Controls.Caption();
            this.txbQuickInfo = new BlueControls.Controls.TextBox();
            this.cbxPic = new BlueControls.Controls.ComboBox();
            this.capQuickInfo = new BlueControls.Controls.Caption();
            this.capImage = new BlueControls.Controls.Caption();
            this.grpRechte = new BlueControls.Controls.GroupBox();
            this.lstPermissionExecute = new BlueControls.Controls.ListBox();
            this.grpEigenschaften = new BlueControls.Controls.GroupBox();
            this.chkReadOnly = new BlueControls.Controls.Button();
            this.chkZeile = new BlueControls.Controls.Button();
            this.grpAuslöser = new BlueControls.Controls.GroupBox();
            this.chkAuslöser_deletingRow = new BlueControls.Controls.Button();
            this.chkAuslöser_export = new BlueControls.Controls.Button();
            this.chkAuslöser_newrow = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechangedThread = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechanged = new BlueControls.Controls.Button();
            this.chkAuslöser_prepaireformula = new BlueControls.Controls.Button();
            this.btnVerlauf = new BlueControls.Controls.Button();
            this.capName = new BlueControls.Controls.Caption();
            this.txbName = new BlueControls.Controls.TextBox();
            this.grpVerfügbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.btnVersionErhöhen = new BlueControls.Controls.Button();
            this.btnTabelleKopf = new BlueControls.Controls.Button();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.btnZusatzDateien = new BlueControls.Controls.Button();
            this.chkExtendend = new BlueControls.Controls.Button();
            this.btnTest = new BlueControls.Controls.Button();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.tbcScriptEigenschaften.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.tabKopfdaten.SuspendLayout();
            this.grpRechte.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpAuslöser.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpInfos
            // 
            this.grpInfos.Controls.Add(this.btnTabelleKopf);
            this.grpInfos.Controls.Add(this.btnSpaltenuebersicht);
            this.grpInfos.Controls.Add(this.btnZusatzDateien);
            this.grpInfos.Location = new System.Drawing.Point(533, 3);
            this.grpInfos.Size = new System.Drawing.Size(270, 75);
            this.grpInfos.Controls.SetChildIndex(this.btnZusatzDateien, 0);
            this.grpInfos.Controls.SetChildIndex(this.btnSpaltenuebersicht, 0);
            this.grpInfos.Controls.SetChildIndex(this.btnTabelleKopf, 0);
            // 
            // grpAusführen
            // 
            this.grpAusführen.Controls.Add(this.chkExtendend);
            this.grpAusführen.Controls.Add(this.btnTest);
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            this.grpAusführen.Size = new System.Drawing.Size(530, 75);
            this.grpAusführen.Controls.SetChildIndex(this.btnAusführen, 0);
            this.grpAusführen.Controls.SetChildIndex(this.cpZeile, 0);
            this.grpAusführen.Controls.SetChildIndex(this.txbTestZeile, 0);
            this.grpAusführen.Controls.SetChildIndex(this.btnTest, 0);
            this.grpAusführen.Controls.SetChildIndex(this.chkExtendend, 0);
            // 
            // grpAktionen
            // 
            this.grpAktionen.Controls.Add(this.btnVersionErhöhen);
            this.grpAktionen.Location = new System.Drawing.Point(803, 3);
            this.grpAktionen.Size = new System.Drawing.Size(200, 75);
            this.grpAktionen.Controls.SetChildIndex(this.btnVersionErhöhen, 0);
            // 
            // btnAusführen
            // 
            this.btnAusführen.ImageCode = "Abspielen|16|||ffff00||||||Warnung";
            this.btnAusführen.Location = new System.Drawing.Point(64, 2);
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Controls.Add(this.tabKopfdaten);
            this.tbcScriptEigenschaften.Location = new System.Drawing.Point(237, 110);
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(804, 427);
            this.tbcScriptEigenschaften.TabDefaultOrder = new string[] {
        "Skript-Editor",
        "Kopfdaten",
        "Befehls-Assistent"};
            this.tbcScriptEigenschaften.TabIndex = 0;
            this.tbcScriptEigenschaften.SelectedIndexChanged += new System.EventHandler(this.GlobalTab_SelectedIndexChanged);
            this.tbcScriptEigenschaften.Controls.SetChildIndex(this.tabKopfdaten, 0);
            // 
            // ribMain
            // 
            this.ribMain.Size = new System.Drawing.Size(1041, 110);
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1041, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Size = new System.Drawing.Size(1041, 24);
            // 
            // tabKopfdaten
            // 
            this.tabKopfdaten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabKopfdaten.Controls.Add(this.capLaufzeit);
            this.tabKopfdaten.Controls.Add(this.capFehler);
            this.tabKopfdaten.Controls.Add(this.txbQuickInfo);
            this.tabKopfdaten.Controls.Add(this.cbxPic);
            this.tabKopfdaten.Controls.Add(this.capQuickInfo);
            this.tabKopfdaten.Controls.Add(this.capImage);
            this.tabKopfdaten.Controls.Add(this.grpRechte);
            this.tabKopfdaten.Controls.Add(this.grpEigenschaften);
            this.tabKopfdaten.Controls.Add(this.grpAuslöser);
            this.tabKopfdaten.Controls.Add(this.btnVerlauf);
            this.tabKopfdaten.Controls.Add(this.capName);
            this.tabKopfdaten.Controls.Add(this.txbName);
            this.tabKopfdaten.Location = new System.Drawing.Point(4, 25);
            this.tabKopfdaten.Name = "tabKopfdaten";
            this.tabKopfdaten.Padding = new System.Windows.Forms.Padding(3);
            this.tabKopfdaten.Size = new System.Drawing.Size(796, 398);
            this.tabKopfdaten.TabIndex = 0;
            this.tabKopfdaten.Text = "Kopfdaten";
            // 
            // capLaufzeit
            // 
            this.capLaufzeit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capLaufzeit.CausesValidation = false;
            this.capLaufzeit.Location = new System.Drawing.Point(704, 184);
            this.capLaufzeit.Name = "capLaufzeit";
            this.capLaufzeit.Size = new System.Drawing.Size(83, 100);
            // 
            // capFehler
            // 
            this.capFehler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capFehler.CausesValidation = false;
            this.capFehler.Location = new System.Drawing.Point(704, 80);
            this.capFehler.Name = "capFehler";
            this.capFehler.Size = new System.Drawing.Size(83, 100);
            // 
            // txbQuickInfo
            // 
            this.txbQuickInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbQuickInfo.Location = new System.Drawing.Point(8, 296);
            this.txbQuickInfo.MultiLine = true;
            this.txbQuickInfo.Name = "txbQuickInfo";
            this.txbQuickInfo.RaiseChangeDelay = 5;
            this.txbQuickInfo.Size = new System.Drawing.Size(688, 152);
            this.txbQuickInfo.TabIndex = 28;
            this.txbQuickInfo.TextChanged += new System.EventHandler(this.txbQuickInfo_TextChanged);
            // 
            // cbxPic
            // 
            this.cbxPic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxPic.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxPic.Location = new System.Drawing.Point(456, 32);
            this.cbxPic.Name = "cbxPic";
            this.cbxPic.RaiseChangeDelay = 5;
            this.cbxPic.Size = new System.Drawing.Size(256, 24);
            this.cbxPic.TabIndex = 27;
            this.cbxPic.TextChanged += new System.EventHandler(this.cbxPic_TextChanged);
            // 
            // capQuickInfo
            // 
            this.capQuickInfo.CausesValidation = false;
            this.capQuickInfo.Location = new System.Drawing.Point(8, 280);
            this.capQuickInfo.Name = "capQuickInfo";
            this.capQuickInfo.Size = new System.Drawing.Size(152, 16);
            this.capQuickInfo.Text = "QuickInfo:";
            // 
            // capImage
            // 
            this.capImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capImage.CausesValidation = false;
            this.capImage.Location = new System.Drawing.Point(456, 8);
            this.capImage.Name = "capImage";
            this.capImage.Size = new System.Drawing.Size(152, 24);
            this.capImage.Text = "Bild:";
            // 
            // grpRechte
            // 
            this.grpRechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpRechte.Controls.Add(this.lstPermissionExecute);
            this.grpRechte.Location = new System.Drawing.Point(472, 80);
            this.grpRechte.Name = "grpRechte";
            this.grpRechte.Size = new System.Drawing.Size(224, 192);
            this.grpRechte.TabIndex = 25;
            this.grpRechte.TabStop = false;
            this.grpRechte.Text = "Rechte";
            // 
            // lstPermissionExecute
            // 
            this.lstPermissionExecute.AddAllowed = BlueControls.Enums.AddType.Text;
            this.lstPermissionExecute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstPermissionExecute.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            this.lstPermissionExecute.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.lstPermissionExecute.FilterText = null;
            this.lstPermissionExecute.Location = new System.Drawing.Point(8, 16);
            this.lstPermissionExecute.Name = "lstPermissionExecute";
            this.lstPermissionExecute.RemoveAllowed = true;
            this.lstPermissionExecute.Size = new System.Drawing.Size(208, 168);
            this.lstPermissionExecute.TabIndex = 18;
            this.lstPermissionExecute.Translate = false;
            this.lstPermissionExecute.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstPermissionExecute_ItemClicked);
            // 
            // grpEigenschaften
            // 
            this.grpEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEigenschaften.Controls.Add(this.chkReadOnly);
            this.grpEigenschaften.Controls.Add(this.chkZeile);
            this.grpEigenschaften.Location = new System.Drawing.Point(8, 64);
            this.grpEigenschaften.Name = "grpEigenschaften";
            this.grpEigenschaften.Size = new System.Drawing.Size(208, 208);
            this.grpEigenschaften.TabIndex = 24;
            this.grpEigenschaften.TabStop = false;
            this.grpEigenschaften.Text = "Eigenschaften";
            // 
            // chkReadOnly
            // 
            this.chkReadOnly.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkReadOnly.Location = new System.Drawing.Point(8, 64);
            this.chkReadOnly.Name = "chkReadOnly";
            this.chkReadOnly.QuickInfo = "Schreibgeschützte Skripte können auch die eigene Zeile ändern";
            this.chkReadOnly.Size = new System.Drawing.Size(120, 16);
            this.chkReadOnly.TabIndex = 15;
            this.chkReadOnly.Text = "Schreibgeschützt";
            this.chkReadOnly.CheckedChanged += new System.EventHandler(this.chkReadOnly_CheckedChanged);
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkZeile.Location = new System.Drawing.Point(8, 40);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile anzeigen/ändern können soll,\r\nmuss " +
    "dieses Häkchen gesetzt sein.";
            this.chkZeile.Size = new System.Drawing.Size(88, 16);
            this.chkZeile.TabIndex = 14;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new System.EventHandler(this.chkZeile_CheckedChanged);
            // 
            // grpAuslöser
            // 
            this.grpAuslöser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuslöser.Controls.Add(this.chkAuslöser_deletingRow);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_export);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_newrow);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechangedThread);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechanged);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_prepaireformula);
            this.grpAuslöser.Location = new System.Drawing.Point(224, 64);
            this.grpAuslöser.Name = "grpAuslöser";
            this.grpAuslöser.Size = new System.Drawing.Size(240, 208);
            this.grpAuslöser.TabIndex = 23;
            this.grpAuslöser.TabStop = false;
            this.grpAuslöser.Text = "Auslöser";
            // 
            // chkAuslöser_deletingRow
            // 
            this.chkAuslöser_deletingRow.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_deletingRow.Location = new System.Drawing.Point(8, 120);
            this.chkAuslöser_deletingRow.Name = "chkAuslöser_deletingRow";
            this.chkAuslöser_deletingRow.QuickInfo = "Das Skript wird ausgeführt, bevor eine Zeile gelöscht wird.";
            this.chkAuslöser_deletingRow.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_deletingRow.TabIndex = 24;
            this.chkAuslöser_deletingRow.Text = "Zeile wird gelöscht";
            this.chkAuslöser_deletingRow.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_export
            // 
            this.chkAuslöser_export.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_export.Location = new System.Drawing.Point(8, 176);
            this.chkAuslöser_export.Name = "chkAuslöser_export";
            this.chkAuslöser_export.QuickInfo = "Das Skript wird vor einem Export ausgeführt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte temporär zu ändern,\r\nVariablen hinzuzufügen oder Bilder zu laden.";
            this.chkAuslöser_export.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_export.TabIndex = 22;
            this.chkAuslöser_export.Text = "Export";
            this.chkAuslöser_export.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_newrow
            // 
            this.chkAuslöser_newrow.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_newrow.Location = new System.Drawing.Point(8, 48);
            this.chkAuslöser_newrow.Name = "chkAuslöser_newrow";
            this.chkAuslöser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgeführt.\r\nMit diesem Skri" +
    "pt können Initialwerte\r\neiner Zeile ergänzt werden.";
            this.chkAuslöser_newrow.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_newrow.TabIndex = 17;
            this.chkAuslöser_newrow.Text = "Zeile initialisieren";
            this.chkAuslöser_newrow.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechangedThread
            // 
            this.chkAuslöser_valuechangedThread.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_valuechangedThread.Location = new System.Drawing.Point(8, 96);
            this.chkAuslöser_valuechangedThread.Name = "chkAuslöser_valuechangedThread";
            this.chkAuslöser_valuechangedThread.QuickInfo = "Das Skript wird irgendwann im Hintergrund nach einer Zelländerung ausgeführt und " +
    "ist nicht sehr zuverlässig.\r\nKann dazu benutzt werden, um Exporte auszuführen.";
            this.chkAuslöser_valuechangedThread.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_valuechangedThread.TabIndex = 20;
            this.chkAuslöser_valuechangedThread.Text = "Wert geändert <b><fontsize=8><i>Extra Thread";
            this.chkAuslöser_valuechangedThread.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechanged
            // 
            this.chkAuslöser_valuechanged.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_valuechanged.Location = new System.Drawing.Point(8, 80);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_valuechanged.TabIndex = 18;
            this.chkAuslöser_valuechanged.Text = "Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_prepaireformula
            // 
            this.chkAuslöser_prepaireformula.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_prepaireformula.Location = new System.Drawing.Point(8, 160);
            this.chkAuslöser_prepaireformula.Name = "chkAuslöser_prepaireformula";
            this.chkAuslöser_prepaireformula.Size = new System.Drawing.Size(175, 16);
            this.chkAuslöser_prepaireformula.TabIndex = 19;
            this.chkAuslöser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAuslöser_prepaireformula.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // btnVerlauf
            // 
            this.btnVerlauf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVerlauf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVerlauf.Enabled = false;
            this.btnVerlauf.ImageCode = "Undo|32";
            this.btnVerlauf.Location = new System.Drawing.Point(728, 8);
            this.btnVerlauf.Name = "btnVerlauf";
            this.btnVerlauf.QuickInfo = "Zeigt den Verlauf in einem\r\nseparatem Fenster an";
            this.btnVerlauf.Size = new System.Drawing.Size(64, 66);
            this.btnVerlauf.TabIndex = 1;
            this.btnVerlauf.Text = "Verlauf";
            this.btnVerlauf.Click += new System.EventHandler(this.btnVerlauf_Click);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new System.Drawing.Point(8, 8);
            this.capName.Name = "capName";
            this.capName.Size = new System.Drawing.Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // txbName
            // 
            this.txbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbName.Location = new System.Drawing.Point(8, 32);
            this.txbName.Name = "txbName";
            this.txbName.RaiseChangeDelay = 5;
            this.txbName.Size = new System.Drawing.Size(432, 24);
            this.txbName.TabIndex = 13;
            this.txbName.TextChanged += new System.EventHandler(this.txbName_TextChanged);
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new System.Drawing.Point(0, 110);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new System.Drawing.Size(237, 427);
            this.grpVerfügbareSkripte.TabIndex = 2;
            this.grpVerfügbareSkripte.TabStop = false;
            this.grpVerfügbareSkripte.Text = "Verfügbare Skripte:";
            // 
            // lstEventScripts
            // 
            this.lstEventScripts.AddAllowed = BlueControls.Enums.AddType.UserDef;
            this.lstEventScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEventScripts.Location = new System.Drawing.Point(8, 16);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new System.Drawing.Size(222, 404);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new System.EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new System.EventHandler(this.lstEventScripts_ItemCheckedChanged);
            this.lstEventScripts.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstEventScripts_RemoveClicked);
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new System.Drawing.Point(128, 2);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new System.Drawing.Size(112, 22);
            this.cpZeile.Text = "Betreffende Zeile:";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Enabled = false;
            this.txbTestZeile.Location = new System.Drawing.Point(128, 24);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.RaiseChangeDelay = 5;
            this.txbTestZeile.Size = new System.Drawing.Size(379, 22);
            this.txbTestZeile.TabIndex = 7;
            // 
            // btnVersionErhöhen
            // 
            this.btnVersionErhöhen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVersionErhöhen.ImageCode = "Pfeil_Oben|16|||||85|0";
            this.btnVersionErhöhen.Location = new System.Drawing.Point(64, 2);
            this.btnVersionErhöhen.Name = "btnVersionErhöhen";
            this.btnVersionErhöhen.QuickInfo = "Skript-Version erhöhen bewirkt,\r\ndass alle Zeilen neu durchgerechnet\r\nwerden.";
            this.btnVersionErhöhen.Size = new System.Drawing.Size(120, 22);
            this.btnVersionErhöhen.TabIndex = 37;
            this.btnVersionErhöhen.Text = "Version erhöhen";
            this.btnVersionErhöhen.Click += new System.EventHandler(this.btnVersionErhöhen_Click);
            // 
            // btnTabelleKopf
            // 
            this.btnTabelleKopf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTabelleKopf.ImageCode = "Tabelle||||||||||Stift";
            this.btnTabelleKopf.Location = new System.Drawing.Point(72, 2);
            this.btnTabelleKopf.Name = "btnTabelleKopf";
            this.btnTabelleKopf.Size = new System.Drawing.Size(64, 66);
            this.btnTabelleKopf.TabIndex = 45;
            this.btnTabelleKopf.Text = "Tabellen-Kopf";
            this.btnTabelleKopf.Click += new System.EventHandler(this.btnTabelleKopf_Click);
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSpaltenuebersicht.ImageCode = "Spalte||||||||||Information";
            this.btnSpaltenuebersicht.Location = new System.Drawing.Point(136, 2);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new System.Drawing.Size(64, 66);
            this.btnSpaltenuebersicht.TabIndex = 44;
            this.btnSpaltenuebersicht.Text = "Spalten-Übersicht";
            this.btnSpaltenuebersicht.Click += new System.EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // btnZusatzDateien
            // 
            this.btnZusatzDateien.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZusatzDateien.ImageCode = "Ordner|16";
            this.btnZusatzDateien.Location = new System.Drawing.Point(200, 2);
            this.btnZusatzDateien.Name = "btnZusatzDateien";
            this.btnZusatzDateien.QuickInfo = "Den Ordner der Zusatzdateien öffnen.\r\nIn diesen können z.B. Skript-Routinen entha" +
    "lten sein\r\ndie mit CallByFilename aufgerufen werden können.";
            this.btnZusatzDateien.Size = new System.Drawing.Size(64, 66);
            this.btnZusatzDateien.TabIndex = 5;
            this.btnZusatzDateien.Text = "Zusatz-dateien";
            this.btnZusatzDateien.Click += new System.EventHandler(this.btnZusatzDateien_Click);
            // 
            // chkExtendend
            // 
            this.chkExtendend.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkExtendend.Location = new System.Drawing.Point(128, 46);
            this.chkExtendend.Name = "chkExtendend";
            this.chkExtendend.Size = new System.Drawing.Size(184, 22);
            this.chkExtendend.TabIndex = 8;
            this.chkExtendend.Tag = "";
            this.chkExtendend.Text = "Erweiterte Ausführung";
            // 
            // btnTest
            // 
            this.btnTest.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTest.ImageCode = "Abspielen|16";
            this.btnTest.Location = new System.Drawing.Point(0, 2);
            this.btnTest.Name = "btnTest";
            this.btnTest.QuickInfo = "Keine Änderung der Daten\r\nin den Tabellen.";
            this.btnTest.Size = new System.Drawing.Size(60, 66);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "Testen";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // TableScriptEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1041, 561);
            this.Controls.Add(this.grpVerfügbareSkripte);
            this.Name = "TableScriptEditor";
            this.Text = "Tabellen-Eigenschaften";
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.grpVerfügbareSkripte, 0);
            this.Controls.SetChildIndex(this.tbcScriptEigenschaften, 0);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.tbcScriptEigenschaften.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.tabKopfdaten.ResumeLayout(false);
            this.grpRechte.ResumeLayout(false);
            this.grpEigenschaften.ResumeLayout(false);
            this.grpAuslöser.ResumeLayout(false);
            this.grpVerfügbareSkripte.ResumeLayout(false);
            this.ResumeLayout(false);

        }
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
    }
}
