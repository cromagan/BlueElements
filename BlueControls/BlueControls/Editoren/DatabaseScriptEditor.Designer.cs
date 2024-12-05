using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueScript.EventArgs;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class DatabaseScriptEditor  {
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(DatabaseScriptEditor));
            this.tbcScriptEigenschaften = new TabControl();
            this.tabSkriptEditor = new TabPage();
            this.tabKopfdaten = new TabPage();
            this.capFehler = new Caption();
            this.txbQuickInfo = new TextBox();
            this.cbxPic = new ComboBox();
            this.capQuickInfo = new Caption();
            this.capImage = new Caption();
            this.grpRechte = new GroupBox();
            this.lstPermissionExecute = new ListBox();
            this.grpEigenschaften = new GroupBox();
            this.chkZeile = new Button();
            this.grpAuslöser = new GroupBox();
            this.chkAuslöser_deletingRow = new Button();
            this.chkAuslöser_databaseloaded = new Button();
            this.chkAuslöser_export = new Button();
            this.chkAuslöser_newrow = new Button();
            this.chkAuslöser_valuechangedThread = new Button();
            this.chkAuslöser_valuechanged = new Button();
            this.chkAuslöser_prepaireformula = new Button();
            this.btnVerlauf = new Button();
            this.capName = new Caption();
            this.txbName = new TextBox();
            this.grpVerfügbareSkripte = new GroupBox();
            this.lstEventScripts = new ListBox();
            this.cpZeile = new Caption();
            this.txbTestZeile = new TextBox();
            this.btnVersionErhöhen = new Button();
            this.btnDatenbankKopf = new Button();
            this.btnSpaltenuebersicht = new Button();
            this.btnZusatzDateien = new Button();
            this.chkExtendend = new Button();
            this.btnTest = new Button();
            this.chkAuslöser_Fehlerfrei = new Button();
            this.pnlStatusBar.SuspendLayout();
            this.tbcScriptEigenschaften.SuspendLayout();
            this.tabSkriptEditor.SuspendLayout();
            this.tabKopfdaten.SuspendLayout();
            this.grpRechte.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpAuslöser.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(237, 653);
            this.pnlStatusBar.Size = new Size(1060, 24);
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Dock = DockStyle.Fill;
            this.eventScriptEditor.Location = new Point(3, 3);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.Script = "";
            this.eventScriptEditor.Size = new Size(1046, 508);
            this.eventScriptEditor.TabIndex = 6;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.ExecuteScript += new EventHandler<ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            this.eventScriptEditor.PropertyChanged += new EventHandler(this.ScriptEditor_PropertyChanged);
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Controls.Add(this.tabSkriptEditor);
            this.tbcScriptEigenschaften.Controls.Add(this.tabKopfdaten);
            this.tbcScriptEigenschaften.Dock = DockStyle.Fill;
            this.tbcScriptEigenschaften.HotTrack = true;
            this.tbcScriptEigenschaften.Location = new Point(237, 110);
            this.tbcScriptEigenschaften.Name = "tbcScriptEigenschaften";
            this.tbcScriptEigenschaften.SelectedIndex = 0;
            this.tbcScriptEigenschaften.Size = new Size(1060, 543);
            this.tbcScriptEigenschaften.TabDefault = null;
            this.tbcScriptEigenschaften.TabDefaultOrder = new string[0];
            this.tbcScriptEigenschaften.TabIndex = 0;
            this.tbcScriptEigenschaften.SelectedIndexChanged += new EventHandler(this.GlobalTab_SelectedIndexChanged);
            // 
            // tabSkriptEditor
            // 
            this.tabSkriptEditor.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabSkriptEditor.Controls.Add(this.eventScriptEditor);
            this.tabSkriptEditor.Location = new Point(4, 25);
            this.tabSkriptEditor.Name = "tabSkriptEditor";
            this.tabSkriptEditor.Padding = new Padding(3);
            this.tabSkriptEditor.Size = new Size(1052, 514);
            this.tabSkriptEditor.TabIndex = 1;
            this.tabSkriptEditor.Text = "Skript-Editor";
            // 
            // tabKopfdaten
            // 
            this.tabKopfdaten.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
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
            this.tabKopfdaten.Location = new Point(4, 25);
            this.tabKopfdaten.Name = "tabKopfdaten";
            this.tabKopfdaten.Padding = new Padding(3);
            this.tabKopfdaten.Size = new Size(1052, 514);
            this.tabKopfdaten.TabIndex = 0;
            this.tabKopfdaten.Text = "Kopfdaten";
            // 
            // capFehler
            // 
            this.capFehler.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.capFehler.CausesValidation = false;
            this.capFehler.Location = new Point(712, 80);
            this.capFehler.Name = "capFehler";
            this.capFehler.Size = new Size(336, 192);
            this.capFehler.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // txbQuickInfo
            // 
            this.txbQuickInfo.Cursor = Cursors.IBeam;
            this.txbQuickInfo.Location = new Point(8, 296);
            this.txbQuickInfo.MultiLine = true;
            this.txbQuickInfo.Name = "txbQuickInfo";
            this.txbQuickInfo.RaiseChangeDelay = 5;
            this.txbQuickInfo.Size = new Size(688, 152);
            this.txbQuickInfo.TabIndex = 28;
            this.txbQuickInfo.TextChanged += new EventHandler(this.txbQuickInfo_TextChanged);
            // 
            // cbxPic
            // 
            this.cbxPic.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.cbxPic.Cursor = Cursors.IBeam;
            this.cbxPic.Location = new Point(712, 32);
            this.cbxPic.Name = "cbxPic";
            this.cbxPic.RaiseChangeDelay = 5;
            this.cbxPic.Size = new Size(256, 24);
            this.cbxPic.TabIndex = 27;
            this.cbxPic.TextChanged += new EventHandler(this.cbxPic_TextChanged);
            // 
            // capQuickInfo
            // 
            this.capQuickInfo.CausesValidation = false;
            this.capQuickInfo.Location = new Point(8, 280);
            this.capQuickInfo.Name = "capQuickInfo";
            this.capQuickInfo.Size = new Size(152, 16);
            this.capQuickInfo.Text = "QuickInfo:";
            // 
            // capImage
            // 
            this.capImage.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.capImage.CausesValidation = false;
            this.capImage.Location = new Point(712, 8);
            this.capImage.Name = "capImage";
            this.capImage.Size = new Size(152, 24);
            this.capImage.Text = "Bild:";
            // 
            // grpRechte
            // 
            this.grpRechte.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpRechte.Controls.Add(this.lstPermissionExecute);
            this.grpRechte.Location = new Point(472, 64);
            this.grpRechte.Name = "grpRechte";
            this.grpRechte.Size = new Size(224, 208);
            this.grpRechte.TabIndex = 25;
            this.grpRechte.TabStop = false;
            this.grpRechte.Text = "Rechte";
            // 
            // lstPermissionExecute
            // 
            this.lstPermissionExecute.AddAllowed = AddType.Text;
            this.lstPermissionExecute.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                                | AnchorStyles.Left)));
            this.lstPermissionExecute.Appearance = ListBoxAppearance.Listbox_Boxes;
            this.lstPermissionExecute.CheckBehavior = CheckBehavior.MultiSelection;
            this.lstPermissionExecute.FilterText = null;
            this.lstPermissionExecute.Location = new Point(8, 16);
            this.lstPermissionExecute.Name = "lstPermissionExecute";
            this.lstPermissionExecute.RemoveAllowed = true;
            this.lstPermissionExecute.Size = new Size(208, 184);
            this.lstPermissionExecute.TabIndex = 18;
            this.lstPermissionExecute.Translate = false;
            this.lstPermissionExecute.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.lstPermissionExecute_ItemClicked);
            // 
            // grpEigenschaften
            // 
            this.grpEigenschaften.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEigenschaften.Controls.Add(this.chkZeile);
            this.grpEigenschaften.Location = new Point(8, 64);
            this.grpEigenschaften.Name = "grpEigenschaften";
            this.grpEigenschaften.Size = new Size(208, 208);
            this.grpEigenschaften.TabIndex = 24;
            this.grpEigenschaften.TabStop = false;
            this.grpEigenschaften.Text = "Eigenschaften";
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkZeile.Location = new Point(8, 40);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile ändern können soll,\r\nmuss dieses Hä" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new Size(88, 16);
            this.chkZeile.TabIndex = 14;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new EventHandler(this.chkZeile_CheckedChanged);
            // 
            // grpAuslöser
            // 
            this.grpAuslöser.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuslöser.Controls.Add(this.chkAuslöser_Fehlerfrei);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_deletingRow);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_databaseloaded);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_export);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_newrow);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechangedThread);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechanged);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_prepaireformula);
            this.grpAuslöser.Location = new Point(224, 64);
            this.grpAuslöser.Name = "grpAuslöser";
            this.grpAuslöser.Size = new Size(240, 208);
            this.grpAuslöser.TabIndex = 23;
            this.grpAuslöser.TabStop = false;
            this.grpAuslöser.Text = "Auslöser";
            // 
            // chkAuslöser_deletingRow
            // 
            this.chkAuslöser_deletingRow.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_deletingRow.Location = new Point(8, 120);
            this.chkAuslöser_deletingRow.Name = "chkAuslöser_deletingRow";
            this.chkAuslöser_deletingRow.QuickInfo = "Das Skript wird ausgeführt, bevor eine Zeile gelöscht wird.";
            this.chkAuslöser_deletingRow.Size = new Size(176, 16);
            this.chkAuslöser_deletingRow.TabIndex = 24;
            this.chkAuslöser_deletingRow.Text = "Zeile wird gelöscht";
            this.chkAuslöser_deletingRow.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_databaseloaded
            // 
            this.chkAuslöser_databaseloaded.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_databaseloaded.Location = new Point(8, 24);
            this.chkAuslöser_databaseloaded.Name = "chkAuslöser_databaseloaded";
            this.chkAuslöser_databaseloaded.QuickInfo = "Das Skript wird direkt nach dem ersten Laden einer Datenbank angestoßen.\r\n\r\nEs ka" +
    "nn verwendet werden, um z.B. Backups zu erstellen.";
            this.chkAuslöser_databaseloaded.Size = new Size(176, 16);
            this.chkAuslöser_databaseloaded.TabIndex = 21;
            this.chkAuslöser_databaseloaded.Text = "Datenbank geladen";
            this.chkAuslöser_databaseloaded.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_export
            // 
            this.chkAuslöser_export.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_export.Location = new Point(8, 176);
            this.chkAuslöser_export.Name = "chkAuslöser_export";
            this.chkAuslöser_export.QuickInfo = "Das Skript wird vor einem Export ausgeführt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte temporär zu ändern,\r\nVariablen hinzuzufügen oder Bilder zu laden.";
            this.chkAuslöser_export.Size = new Size(176, 16);
            this.chkAuslöser_export.TabIndex = 22;
            this.chkAuslöser_export.Text = "Export";
            this.chkAuslöser_export.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_newrow
            // 
            this.chkAuslöser_newrow.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_newrow.Location = new Point(8, 48);
            this.chkAuslöser_newrow.Name = "chkAuslöser_newrow";
            this.chkAuslöser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgeführt.\r\nMit diesem Skri" +
    "pt können Initialwerte einer Zeile ergänzt werden.";
            this.chkAuslöser_newrow.Size = new Size(176, 16);
            this.chkAuslöser_newrow.TabIndex = 17;
            this.chkAuslöser_newrow.Text = "Zeile initialisieren";
            this.chkAuslöser_newrow.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechangedThread
            // 
            this.chkAuslöser_valuechangedThread.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_valuechangedThread.Location = new Point(8, 96);
            this.chkAuslöser_valuechangedThread.Name = "chkAuslöser_valuechangedThread";
            this.chkAuslöser_valuechangedThread.QuickInfo = "Das Skript wird irgendwann im Hintergrund nach einer Zelländerung ausgeführt und " +
    "ist nicht sehr zuverlässig.\r\nKann dazu benutzt werden, um Exporte auszuführen.";
            this.chkAuslöser_valuechangedThread.Size = new Size(176, 16);
            this.chkAuslöser_valuechangedThread.TabIndex = 20;
            this.chkAuslöser_valuechangedThread.Text = "Wert geändert <b><fontsize=8><i>Extra Thread";
            this.chkAuslöser_valuechangedThread.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechanged
            // 
            this.chkAuslöser_valuechanged.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_valuechanged.Location = new Point(8, 80);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.QuickInfo = resources.GetString("chkAuslöser_valuechanged.QuickInfo");
            this.chkAuslöser_valuechanged.Size = new Size(176, 16);
            this.chkAuslöser_valuechanged.TabIndex = 18;
            this.chkAuslöser_valuechanged.Text = "Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_prepaireformula
            // 
            this.chkAuslöser_prepaireformula.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_prepaireformula.Location = new Point(8, 160);
            this.chkAuslöser_prepaireformula.Name = "chkAuslöser_prepaireformula";
            this.chkAuslöser_prepaireformula.QuickInfo = "Das Skript wird zur Datenkonsitenzprüfung,\r\nfür Variablen für Formulare und virtu" +
    "elle Spalten verwendet.\r\n\r\nEs kann keine Daten ändern, auf Festplatte zugreifen " +
    "oder\r\nlange dauernde Prozesse anstoßen.";
            this.chkAuslöser_prepaireformula.Size = new Size(175, 16);
            this.chkAuslöser_prepaireformula.TabIndex = 19;
            this.chkAuslöser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAuslöser_prepaireformula.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // btnVerlauf
            // 
            this.btnVerlauf.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnVerlauf.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnVerlauf.Enabled = false;
            this.btnVerlauf.ImageCode = "Undo|32";
            this.btnVerlauf.Location = new Point(984, 8);
            this.btnVerlauf.Name = "btnVerlauf";
            this.btnVerlauf.QuickInfo = "Zeigt den Verlauf in einem\r\nseparatem Fenster an";
            this.btnVerlauf.Size = new Size(64, 66);
            this.btnVerlauf.TabIndex = 1;
            this.btnVerlauf.Text = "Verlauf";
            this.btnVerlauf.Click += new EventHandler(this.btnVerlauf_Click);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new Point(8, 8);
            this.capName.Name = "capName";
            this.capName.Size = new Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // txbName
            // 
            this.txbName.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txbName.Cursor = Cursors.IBeam;
            this.txbName.Location = new Point(8, 32);
            this.txbName.Name = "txbName";
            this.txbName.RaiseChangeDelay = 5;
            this.txbName.Size = new Size(688, 24);
            this.txbName.TabIndex = 13;
            this.txbName.TextChanged += new EventHandler(this.txbName_TextChanged);
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new Point(0, 110);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new Size(237, 567);
            this.grpVerfügbareSkripte.TabIndex = 2;
            this.grpVerfügbareSkripte.TabStop = false;
            this.grpVerfügbareSkripte.Text = "Verfügbare Skripte:";
            // 
            // lstEventScripts
            // 
            this.lstEventScripts.AddAllowed = AddType.UserDef;
            this.lstEventScripts.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                            | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.lstEventScripts.Location = new Point(8, 16);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new Size(222, 544);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new Point(128, 2);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new Size(112, 22);
            this.cpZeile.Text = "Betreffende Zeile:";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = Cursors.IBeam;
            this.txbTestZeile.Enabled = false;
            this.txbTestZeile.Location = new Point(128, 24);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.RaiseChangeDelay = 5;
            this.txbTestZeile.Size = new Size(379, 22);
            this.txbTestZeile.TabIndex = 7;
            // 
            // btnVersionErhöhen
            // 
            this.btnVersionErhöhen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnVersionErhöhen.ImageCode = "Pfeil_Oben|16|||||85|0";
            this.btnVersionErhöhen.Location = new Point(64, 2);
            this.btnVersionErhöhen.Name = "btnVersionErhöhen";
            this.btnVersionErhöhen.QuickInfo = "Skript-Version erhöhen bewirkt,\r\ndass alle Zeilen neu durchgerechnet\r\nwerden.";
            this.btnVersionErhöhen.Size = new Size(136, 22);
            this.btnVersionErhöhen.TabIndex = 37;
            this.btnVersionErhöhen.Text = "Version erhöhen";
            this.btnVersionErhöhen.Click += new EventHandler(this.btnVersionErhöhen_Click);
            // 
            // grpInfos
            // 
            this.grpInfos.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpInfos.Controls.Add(this.btnDatenbankKopf);
            this.grpInfos.Controls.Add(this.btnSpaltenuebersicht);
            this.grpInfos.Controls.Add(this.btnZusatzDateien);
            this.grpInfos.Dock = DockStyle.Left;
            this.grpInfos.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpInfos.Location = new Point(520, 3);
            this.grpInfos.Name = "grpInfos";
            this.grpInfos.Size = new Size(280, 75);
            this.grpInfos.TabIndex = 1;
            this.grpInfos.TabStop = false;
            this.grpInfos.Text = "Infos";
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new Point(8, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 45;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new EventHandler(this.btnDatenbankKopf_Click);
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnSpaltenuebersicht.ImageCode = "Spalte||||||||||Information";
            this.btnSpaltenuebersicht.Location = new Point(72, 2);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new Size(64, 66);
            this.btnSpaltenuebersicht.TabIndex = 44;
            this.btnSpaltenuebersicht.Text = "Spalten-Übersicht";
            this.btnSpaltenuebersicht.Click += new EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // btnZusatzDateien
            // 
            this.btnZusatzDateien.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnZusatzDateien.ImageCode = "Ordner|16";
            this.btnZusatzDateien.Location = new Point(144, 2);
            this.btnZusatzDateien.Name = "btnZusatzDateien";
            this.btnZusatzDateien.QuickInfo = "Den Ordner der Zusatzdateien öffnen.\r\nIn diesen können z.B. Skript-Routinen enthal" +
    "ten sein\r\ndie mit CallByFilename aufgerufen werden können.";
            this.btnZusatzDateien.Size = new Size(64, 66);
            this.btnZusatzDateien.TabIndex = 5;
            this.btnZusatzDateien.Text = "Zusatz-dateien";
            this.btnZusatzDateien.Click += new EventHandler(this.btnZusatzDateien_Click);
            // 
            // grpAusführen
            // 
            this.grpAusführen.Controls.Add(this.chkExtendend);
            this.grpAusführen.Controls.Add(this.btnTest);
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            // 
            // chkExtendend
            // 
            this.chkExtendend.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkExtendend.Location = new Point(128, 46);
            this.chkExtendend.Name = "chkExtendend";
            this.chkExtendend.QuickInfo = resources.GetString("chkExtendend.QuickInfo");
            this.chkExtendend.Size = new Size(184, 22);
            this.chkExtendend.TabIndex = 8;
            this.chkExtendend.Tag = "";
            this.chkExtendend.Text = "Erweiterte Ausführung";
            // 
            // btnAusführen
            // 
            this.btnAusführen.Click += new EventHandler(this.btnAusführen_Click);
            // 
            // btnTest
            // 
            this.btnTest.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnTest.ImageCode = "Abspielen|16";
            this.btnTest.Location = new Point(8, 2);
            this.btnTest.Name = "btnTest";
            this.btnTest.QuickInfo = "Keine Änderung der Daten\r\nin den Datenbanken.";
            this.btnTest.Size = new Size(60, 66);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "Testen";
            this.btnTest.Click += new EventHandler(this.btnTest_Click);
            // 
            // chkAuslöser_Fehlerfrei
            // 
            this.chkAuslöser_Fehlerfrei.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkAuslöser_Fehlerfrei.Location = new Point(8, 136);
            this.chkAuslöser_Fehlerfrei.Name = "chkAuslöser_Fehlerfrei";
            this.chkAuslöser_Fehlerfrei.QuickInfo = "Das Skript wird ausgeführt, wenn sich der Fehlerfrei-Status\r\nverändert hat.";
            this.chkAuslöser_Fehlerfrei.Size = new Size(208, 16);
            this.chkAuslöser_Fehlerfrei.TabIndex = 25;
            this.chkAuslöser_Fehlerfrei.Text = "\'Fehlerfrei\' hat sich verändert";
            this.chkAuslöser_Fehlerfrei.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // DatabaseScriptEditor
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.ClientSize = new Size(1297, 677);
            this.Controls.Add(this.tbcScriptEigenschaften);
            this.Controls.Add(this.grpVerfügbareSkripte);
            this.MinimizeBox = false;
            this.Name = "DatabaseScriptEditor";
            this.Text = "Datenbank-Eigenschaften";
            this.WindowState = FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.grpVerfügbareSkripte, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.tbcScriptEigenschaften, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.tbcScriptEigenschaften.ResumeLayout(false);
            this.tabSkriptEditor.ResumeLayout(false);
            this.tabKopfdaten.ResumeLayout(false);
            this.grpRechte.ResumeLayout(false);
            this.grpEigenschaften.ResumeLayout(false);
            this.grpAuslöser.ResumeLayout(false);
            this.grpVerfügbareSkripte.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
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
        private Button chkAuslöser_databaseloaded;
        private Button chkAuslöser_export;
        private Button btnVersionErhöhen;
        private Button btnVerlauf;
        private Button btnZusatzDateien;
        private Button btnTest;
        private TabControl tbcScriptEigenschaften;
        private TabPage tabKopfdaten;
        private TabPage tabSkriptEditor;
        private Button btnSpaltenuebersicht;
        private Button btnDatenbankKopf;
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
        private Button chkAuslöser_Fehlerfrei;
    }
}
