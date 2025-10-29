using System.Diagnostics;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class TableScriptEditor  {
        //Das Formular �berschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist f�r den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer m�glich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht m�glich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableScriptEditor));
            this.tabKopfdaten = new System.Windows.Forms.TabPage();
            this.capFehler = new BlueControls.Controls.Caption();
            this.txbQuickInfo = new BlueControls.Controls.TextBox();
            this.cbxPic = new BlueControls.Controls.ComboBox();
            this.capQuickInfo = new BlueControls.Controls.Caption();
            this.capImage = new BlueControls.Controls.Caption();
            this.grpRechte = new BlueControls.Controls.GroupBox();
            this.lstPermissionExecute = new BlueControls.Controls.ListBox();
            this.grpEigenschaften = new BlueControls.Controls.GroupBox();
            this.chkZeile = new BlueControls.Controls.Button();
            this.grpAusl�ser = new BlueControls.Controls.GroupBox();
            this.chkAusl�ser_deletingRow = new BlueControls.Controls.Button();
            this.chkAusl�ser_export = new BlueControls.Controls.Button();
            this.chkAusl�ser_newrow = new BlueControls.Controls.Button();
            this.chkAusl�ser_valuechangedThread = new BlueControls.Controls.Button();
            this.chkAusl�ser_valuechanged = new BlueControls.Controls.Button();
            this.chkAusl�ser_prepaireformula = new BlueControls.Controls.Button();
            this.btnVerlauf = new BlueControls.Controls.Button();
            this.capName = new BlueControls.Controls.Caption();
            this.txbName = new BlueControls.Controls.TextBox();
            this.grpVerf�gbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.btnVersionErh�hen = new BlueControls.Controls.Button();
            this.btnTabelleKopf = new BlueControls.Controls.Button();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.btnZusatzDateien = new BlueControls.Controls.Button();
            this.chkExtendend = new BlueControls.Controls.Button();
            this.btnTest = new BlueControls.Controls.Button();
            this.grpInfos.SuspendLayout();
            this.grpAusf�hren.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.tbcScriptEigenschaften.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.tabKopfdaten.SuspendLayout();
            this.grpRechte.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpAusl�ser.SuspendLayout();
            this.grpVerf�gbareSkripte.SuspendLayout();
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
            // grpAusf�hren
            // 
            this.grpAusf�hren.Controls.Add(this.chkExtendend);
            this.grpAusf�hren.Controls.Add(this.btnTest);
            this.grpAusf�hren.Controls.Add(this.txbTestZeile);
            this.grpAusf�hren.Controls.Add(this.cpZeile);
            this.grpAusf�hren.Size = new System.Drawing.Size(530, 75);
            this.grpAusf�hren.Controls.SetChildIndex(this.btnAusf�hren, 0);
            this.grpAusf�hren.Controls.SetChildIndex(this.cpZeile, 0);
            this.grpAusf�hren.Controls.SetChildIndex(this.txbTestZeile, 0);
            this.grpAusf�hren.Controls.SetChildIndex(this.btnTest, 0);
            this.grpAusf�hren.Controls.SetChildIndex(this.chkExtendend, 0);
            // 
            // grpAktionen
            // 
            this.grpAktionen.Controls.Add(this.btnVersionErh�hen);
            this.grpAktionen.Location = new System.Drawing.Point(803, 3);
            this.grpAktionen.Size = new System.Drawing.Size(200, 75);
            this.grpAktionen.Controls.SetChildIndex(this.btnVersionErh�hen, 0);
            // 
            // btnAusf�hren
            // 
            this.btnAusf�hren.ImageCode = "Abspielen|16|||ffff00||||||Warnung";
            this.btnAusf�hren.Location = new System.Drawing.Point(64, 2);
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Controls.Add(this.tabKopfdaten);
            this.tbcScriptEigenschaften.Location = new System.Drawing.Point(237, 110);
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(547, 427);
            this.tbcScriptEigenschaften.TabDefaultOrder = new string[] {
        "Skript-Editor",
        "Kopfdaten",
        "Befehls-Assistent"};
            this.tbcScriptEigenschaften.TabIndex = 0;
            this.tbcScriptEigenschaften.SelectedIndexChanged += new System.EventHandler(this.GlobalTab_SelectedIndexChanged);
            this.tbcScriptEigenschaften.Controls.SetChildIndex(this.tabKopfdaten, 0);
            // 
            // tabKopfdaten
            // 
            this.tabKopfdaten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabKopfdaten.Controls.Add(this.capFehler);
            this.tabKopfdaten.Controls.Add(this.txbQuickInfo);
            this.tabKopfdaten.Controls.Add(this.cbxPic);
            this.tabKopfdaten.Controls.Add(this.capQuickInfo);
            this.tabKopfdaten.Controls.Add(this.capImage);
            this.tabKopfdaten.Controls.Add(this.grpRechte);
            this.tabKopfdaten.Controls.Add(this.grpEigenschaften);
            this.tabKopfdaten.Controls.Add(this.grpAusl�ser);
            this.tabKopfdaten.Controls.Add(this.btnVerlauf);
            this.tabKopfdaten.Controls.Add(this.capName);
            this.tabKopfdaten.Controls.Add(this.txbName);
            this.tabKopfdaten.Location = new System.Drawing.Point(4, 25);
            this.tabKopfdaten.Name = "tabKopfdaten";
            this.tabKopfdaten.Padding = new System.Windows.Forms.Padding(3);
            this.tabKopfdaten.Size = new System.Drawing.Size(776, 398);
            this.tabKopfdaten.TabIndex = 0;
            this.tabKopfdaten.Text = "Kopfdaten";
            // 
            // capFehler
            // 
            this.capFehler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capFehler.CausesValidation = false;
            this.capFehler.Location = new System.Drawing.Point(712, 80);
            this.capFehler.Name = "capFehler";
            this.capFehler.Size = new System.Drawing.Size(237, 192);
            this.capFehler.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
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
            this.cbxPic.Location = new System.Drawing.Point(436, 32);
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
            this.capImage.Location = new System.Drawing.Point(436, 8);
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
            this.grpEigenschaften.Controls.Add(this.chkZeile);
            this.grpEigenschaften.Location = new System.Drawing.Point(8, 64);
            this.grpEigenschaften.Name = "grpEigenschaften";
            this.grpEigenschaften.Size = new System.Drawing.Size(208, 208);
            this.grpEigenschaften.TabIndex = 24;
            this.grpEigenschaften.TabStop = false;
            this.grpEigenschaften.Text = "Eigenschaften";
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkZeile.Location = new System.Drawing.Point(8, 40);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile �ndern k�nnen soll,\r\nmuss dieses H�" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new System.Drawing.Size(88, 16);
            this.chkZeile.TabIndex = 14;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new System.EventHandler(this.chkZeile_CheckedChanged);
            // 
            // grpAusl�ser
            // 
            this.grpAusl�ser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_deletingRow);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_export);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_newrow);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_valuechangedThread);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_valuechanged);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_prepaireformula);
            this.grpAusl�ser.Location = new System.Drawing.Point(224, 64);
            this.grpAusl�ser.Name = "grpAusl�ser";
            this.grpAusl�ser.Size = new System.Drawing.Size(240, 208);
            this.grpAusl�ser.TabIndex = 23;
            this.grpAusl�ser.TabStop = false;
            this.grpAusl�ser.Text = "Ausl�ser";
            // 
            // chkAusl�ser_deletingRow
            // 
            this.chkAusl�ser_deletingRow.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAusl�ser_deletingRow.Location = new System.Drawing.Point(8, 120);
            this.chkAusl�ser_deletingRow.Name = "chkAusl�ser_deletingRow";
            this.chkAusl�ser_deletingRow.QuickInfo = "Das Skript wird ausgef�hrt, bevor eine Zeile gel�scht wird.";
            this.chkAusl�ser_deletingRow.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_deletingRow.TabIndex = 24;
            this.chkAusl�ser_deletingRow.Text = "Zeile wird gel�scht";
            this.chkAusl�ser_deletingRow.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_export
            // 
            this.chkAusl�ser_export.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAusl�ser_export.Location = new System.Drawing.Point(8, 176);
            this.chkAusl�ser_export.Name = "chkAusl�ser_export";
            this.chkAusl�ser_export.QuickInfo = "Das Skript wird vor einem Export ausgef�hrt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte tempor�r zu �ndern,\r\nVariablen hinzuzuf�gen oder Bilder zu laden.";
            this.chkAusl�ser_export.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_export.TabIndex = 22;
            this.chkAusl�ser_export.Text = "Export";
            this.chkAusl�ser_export.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_newrow
            // 
            this.chkAusl�ser_newrow.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAusl�ser_newrow.Location = new System.Drawing.Point(8, 48);
            this.chkAusl�ser_newrow.Name = "chkAusl�ser_newrow";
            this.chkAusl�ser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgef�hrt.\r\nMit diesem Skri" +
    "pt k�nnen Initialwerte\r\neiner Zeile erg�nzt werden.";
            this.chkAusl�ser_newrow.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_newrow.TabIndex = 17;
            this.chkAusl�ser_newrow.Text = "Zeile initialisieren";
            this.chkAusl�ser_newrow.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_valuechangedThread
            // 
            this.chkAusl�ser_valuechangedThread.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAusl�ser_valuechangedThread.Location = new System.Drawing.Point(8, 96);
            this.chkAusl�ser_valuechangedThread.Name = "chkAusl�ser_valuechangedThread";
            this.chkAusl�ser_valuechangedThread.QuickInfo = "Das Skript wird irgendwann im Hintergrund nach einer Zell�nderung ausgef�hrt und " +
    "ist nicht sehr zuverl�ssig.\r\nKann dazu benutzt werden, um Exporte auszuf�hren.";
            this.chkAusl�ser_valuechangedThread.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_valuechangedThread.TabIndex = 20;
            this.chkAusl�ser_valuechangedThread.Text = "Wert ge�ndert <b><fontsize=8><i>Extra Thread";
            this.chkAusl�ser_valuechangedThread.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_valuechanged
            // 
            this.chkAusl�ser_valuechanged.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAusl�ser_valuechanged.Location = new System.Drawing.Point(8, 80);
            this.chkAusl�ser_valuechanged.Name = "chkAusl�ser_valuechanged";
            this.chkAusl�ser_valuechanged.QuickInfo = resources.GetString("chkAusl�ser_valuechanged.QuickInfo");
            this.chkAusl�ser_valuechanged.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_valuechanged.TabIndex = 18;
            this.chkAusl�ser_valuechanged.Text = "Wert ge�ndert";
            this.chkAusl�ser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_prepaireformula
            // 
            this.chkAusl�ser_prepaireformula.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAusl�ser_prepaireformula.Location = new System.Drawing.Point(8, 160);
            this.chkAusl�ser_prepaireformula.Name = "chkAusl�ser_prepaireformula";
            this.chkAusl�ser_prepaireformula.QuickInfo = resources.GetString("chkAusl�ser_prepaireformula.QuickInfo");
            this.chkAusl�ser_prepaireformula.Size = new System.Drawing.Size(175, 16);
            this.chkAusl�ser_prepaireformula.TabIndex = 19;
            this.chkAusl�ser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAusl�ser_prepaireformula.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // btnVerlauf
            // 
            this.btnVerlauf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVerlauf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVerlauf.Enabled = false;
            this.btnVerlauf.ImageCode = "Undo|32";
            this.btnVerlauf.Location = new System.Drawing.Point(708, 8);
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
            this.txbName.Size = new System.Drawing.Size(412, 24);
            this.txbName.TabIndex = 13;
            this.txbName.TextChanged += new System.EventHandler(this.txbName_TextChanged);
            // 
            // grpVerf�gbareSkripte
            // 
            this.grpVerf�gbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpVerf�gbareSkripte.CausesValidation = false;
            this.grpVerf�gbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerf�gbareSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVerf�gbareSkripte.Location = new System.Drawing.Point(0, 110);
            this.grpVerf�gbareSkripte.Name = "grpVerf�gbareSkripte";
            this.grpVerf�gbareSkripte.Size = new System.Drawing.Size(237, 427);
            this.grpVerf�gbareSkripte.TabIndex = 2;
            this.grpVerf�gbareSkripte.TabStop = false;
            this.grpVerf�gbareSkripte.Text = "Verf�gbare Skripte:";
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
            // btnVersionErh�hen
            // 
            this.btnVersionErh�hen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVersionErh�hen.ImageCode = "Pfeil_Oben|16|||||85|0";
            this.btnVersionErh�hen.Location = new System.Drawing.Point(64, 2);
            this.btnVersionErh�hen.Name = "btnVersionErh�hen";
            this.btnVersionErh�hen.QuickInfo = "Skript-Version erh�hen bewirkt,\r\ndass alle Zeilen neu durchgerechnet\r\nwerden.";
            this.btnVersionErh�hen.Size = new System.Drawing.Size(120, 22);
            this.btnVersionErh�hen.TabIndex = 37;
            this.btnVersionErh�hen.Text = "Version erh�hen";
            this.btnVersionErh�hen.Click += new System.EventHandler(this.btnVersionErh�hen_Click);
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
            this.btnSpaltenuebersicht.Text = "Spalten-�bersicht";
            this.btnSpaltenuebersicht.Click += new System.EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // btnZusatzDateien
            // 
            this.btnZusatzDateien.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZusatzDateien.ImageCode = "Ordner|16";
            this.btnZusatzDateien.Location = new System.Drawing.Point(200, 2);
            this.btnZusatzDateien.Name = "btnZusatzDateien";
            this.btnZusatzDateien.QuickInfo = "Den Ordner der Zusatzdateien �ffnen.\r\nIn diesen k�nnen z.B. Skript-Routinen entha" +
    "lten sein\r\ndie mit CallByFilename aufgerufen werden k�nnen.";
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
            this.chkExtendend.QuickInfo = resources.GetString("chkExtendend.QuickInfo");
            this.chkExtendend.Size = new System.Drawing.Size(184, 22);
            this.chkExtendend.TabIndex = 8;
            this.chkExtendend.Tag = "";
            this.chkExtendend.Text = "Erweiterte Ausf�hrung";
            // 
            // btnTest
            // 
            this.btnTest.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTest.ImageCode = "Abspielen|16";
            this.btnTest.Location = new System.Drawing.Point(0, 2);
            this.btnTest.Name = "btnTest";
            this.btnTest.QuickInfo = "Keine �nderung der Daten\r\nin den Tabellen.";
            this.btnTest.Size = new System.Drawing.Size(60, 66);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "Testen";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // TableScriptEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.grpVerf�gbareSkripte);
            this.Name = "TableScriptEditor";
            this.Text = "Tabellen-Eigenschaften";
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.grpVerf�gbareSkripte, 0);
            this.Controls.SetChildIndex(this.tbcScriptEigenschaften, 0);
            this.grpInfos.ResumeLayout(false);
            this.grpAusf�hren.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.tbcScriptEigenschaften.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.tabKopfdaten.ResumeLayout(false);
            this.grpRechte.ResumeLayout(false);
            this.grpEigenschaften.ResumeLayout(false);
            this.grpAusl�ser.ResumeLayout(false);
            this.grpVerf�gbareSkripte.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private GroupBox grpVerf�gbareSkripte;
        private ListBox lstEventScripts;
        private Button chkAusl�ser_prepaireformula;
        private Button chkAusl�ser_valuechanged;
        private Button chkAusl�ser_newrow;
        private Button chkZeile;
        private TextBox txbName;
        private Caption capName;
        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button chkAusl�ser_valuechangedThread;
        private Button chkAusl�ser_export;
        private Button btnVersionErh�hen;
        private Button btnVerlauf;
        private Button btnZusatzDateien;
        private Button btnTest;
        private TabPage tabKopfdaten;
        private Button btnSpaltenuebersicht;
        private Button btnTabelleKopf;
        private GroupBox grpAusl�ser;
        private GroupBox grpRechte;
        private GroupBox grpEigenschaften;
        private ListBox lstPermissionExecute;
        private ComboBox cbxPic;
        private Caption capQuickInfo;
        private Caption capImage;
        private TextBox txbQuickInfo;
        private Caption capFehler;
        private Button chkAusl�ser_deletingRow;
        private Button chkExtendend;
    }
}
