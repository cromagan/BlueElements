using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueScript.EventArgs;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class DatabaseScriptEditor : FormWithStatusBar {
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
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.tabScripts = new System.Windows.Forms.TabPage();
            this.eventScriptEditor = new BlueControls.ScriptEditor();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbcScriptEigenschaften = new BlueControls.Controls.TabControl();
            this.tabHaupt = new System.Windows.Forms.TabPage();
            this.chkExternVerfügbar = new BlueControls.Controls.Button();
            this.chkAendertWerte = new BlueControls.Controls.Button();
            this.chkZeile = new BlueControls.Controls.Button();
            this.capName = new BlueControls.Controls.Caption();
            this.txbName = new BlueControls.Controls.TextBox();
            this.tabAuslöser = new System.Windows.Forms.TabPage();
            this.chkAuslöser_export = new BlueControls.Controls.Button();
            this.chkAuslöser_prepaireformula = new BlueControls.Controls.Button();
            this.chkAuslöser_databaseloaded = new BlueControls.Controls.Button();
            this.chkAuslöser_newrow = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechangedThread = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechanged = new BlueControls.Controls.Button();
            this.tabRechte = new System.Windows.Forms.TabPage();
            this.Aussehen = new System.Windows.Forms.TabPage();
            this.grpVerfügbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.chkChangeValuesInTest = new BlueControls.Controls.Button();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.btnVersionErhöhen = new BlueControls.Controls.Button();
            this.btnVerlauf = new BlueControls.Controls.Button();
            this.ribMain = new BlueControls.Controls.RibbonBar();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.grpAktionen = new BlueControls.Controls.GroupBox();
            this.btnSaveLoad = new BlueControls.Controls.Button();
            this.grpInfos = new BlueControls.Controls.GroupBox();
            this.btnDatenbankKopf = new BlueControls.Controls.Button();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.btnBefehlsUebersicht = new BlueControls.Controls.Button();
            this.btnZusatzDateien = new BlueControls.Controls.Button();
            this.grpAusführen = new BlueControls.Controls.GroupBox();
            this.btnTest = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tbcScriptEigenschaften.SuspendLayout();
            this.tabHaupt.SuspendLayout();
            this.tabAuslöser.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(1297, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 653);
            this.pnlStatusBar.Size = new System.Drawing.Size(1297, 24);
            // 
            // GlobalTab
            // 
            this.GlobalTab.Controls.Add(this.tabScripts);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(280, 232);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new System.Drawing.Size(1297, 543);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.SelectedIndexChanged += new System.EventHandler(this.GlobalTab_SelectedIndexChanged);
            // 
            // tabScripts
            // 
            this.tabScripts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScripts.Controls.Add(this.eventScriptEditor);
            this.tabScripts.Controls.Add(this.panel1);
            this.tabScripts.Controls.Add(this.grpVerfügbareSkripte);
            this.tabScripts.Location = new System.Drawing.Point(4, 25);
            this.tabScripts.Name = "tabScripts";
            this.tabScripts.Padding = new System.Windows.Forms.Padding(3);
            this.tabScripts.Size = new System.Drawing.Size(1289, 514);
            this.tabScripts.TabIndex = 7;
            this.tabScripts.Text = "Skripte";
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventScriptEditor.Location = new System.Drawing.Point(240, 152);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.ScriptText = "";
            this.eventScriptEditor.Size = new System.Drawing.Size(1046, 359);
            this.eventScriptEditor.TabIndex = 6;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Changed += new System.EventHandler(this.ScriptEditor_Changed);
            this.eventScriptEditor.ExecuteScript += new System.EventHandler<BlueScript.EventArgs.ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbcScriptEigenschaften);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(240, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1046, 149);
            this.panel1.TabIndex = 22;
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Controls.Add(this.tabHaupt);
            this.tbcScriptEigenschaften.Controls.Add(this.tabAuslöser);
            this.tbcScriptEigenschaften.Controls.Add(this.tabRechte);
            this.tbcScriptEigenschaften.Controls.Add(this.Aussehen);
            this.tbcScriptEigenschaften.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcScriptEigenschaften.HotTrack = true;
            this.tbcScriptEigenschaften.Location = new System.Drawing.Point(0, 0);
            this.tbcScriptEigenschaften.Name = "tbcScriptEigenschaften";
            this.tbcScriptEigenschaften.SelectedIndex = 0;
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(1046, 149);
            this.tbcScriptEigenschaften.TabDefault = null;
            this.tbcScriptEigenschaften.TabDefaultOrder = new string[0];
            this.tbcScriptEigenschaften.TabIndex = 0;
            // 
            // tabHaupt
            // 
            this.tabHaupt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabHaupt.Controls.Add(this.chkExternVerfügbar);
            this.tabHaupt.Controls.Add(this.chkAendertWerte);
            this.tabHaupt.Controls.Add(this.chkZeile);
            this.tabHaupt.Controls.Add(this.capName);
            this.tabHaupt.Controls.Add(this.txbName);
            this.tabHaupt.Location = new System.Drawing.Point(4, 25);
            this.tabHaupt.Name = "tabHaupt";
            this.tabHaupt.Padding = new System.Windows.Forms.Padding(3);
            this.tabHaupt.Size = new System.Drawing.Size(1038, 120);
            this.tabHaupt.TabIndex = 0;
            this.tabHaupt.Text = "Haupt";
            // 
            // chkExternVerfügbar
            // 
            this.chkExternVerfügbar.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkExternVerfügbar.Location = new System.Drawing.Point(120, 88);
            this.chkExternVerfügbar.Name = "chkExternVerfügbar";
            this.chkExternVerfügbar.QuickInfo = "Wenn das Skript über eine Menüleiste oder dem Kontextmenü\r\nwählbar sein soll, mus" +
    "s dieses Häkchen gesetzt sein.";
            this.chkExternVerfügbar.Size = new System.Drawing.Size(120, 16);
            this.chkExternVerfügbar.TabIndex = 15;
            this.chkExternVerfügbar.Text = "Extern verfügbar";
            this.chkExternVerfügbar.CheckedChanged += new System.EventHandler(this.chkExternVerfügbar_CheckedChanged);
            // 
            // chkAendertWerte
            // 
            this.chkAendertWerte.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAendertWerte.Location = new System.Drawing.Point(48, 64);
            this.chkAendertWerte.Name = "chkAendertWerte";
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgeführt um dessen\r\nBerechnungen abzugreifen.\r\nÄnderungen w" +
    "erden nicht in die Datenbank\r\nzurückgespielt";
            this.chkAendertWerte.Size = new System.Drawing.Size(120, 16);
            this.chkAendertWerte.TabIndex = 16;
            this.chkAendertWerte.Text = "Ändert Werte";
            this.chkAendertWerte.CheckedChanged += new System.EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkZeile.Location = new System.Drawing.Point(200, 72);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile ändern können soll,\r\nmuss dieses Hä" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new System.Drawing.Size(88, 16);
            this.chkZeile.TabIndex = 14;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new System.EventHandler(this.chkZeile_CheckedChanged);
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
            this.txbName.Size = new System.Drawing.Size(648, 22);
            this.txbName.TabIndex = 13;
            this.txbName.TextChanged += new System.EventHandler(this.txbName_TextChanged);
            // 
            // tabAuslöser
            // 
            this.tabAuslöser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAuslöser.Controls.Add(this.chkAuslöser_export);
            this.tabAuslöser.Controls.Add(this.chkAuslöser_prepaireformula);
            this.tabAuslöser.Controls.Add(this.chkAuslöser_databaseloaded);
            this.tabAuslöser.Controls.Add(this.chkAuslöser_newrow);
            this.tabAuslöser.Controls.Add(this.chkAuslöser_valuechangedThread);
            this.tabAuslöser.Controls.Add(this.chkAuslöser_valuechanged);
            this.tabAuslöser.Location = new System.Drawing.Point(4, 25);
            this.tabAuslöser.Name = "tabAuslöser";
            this.tabAuslöser.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuslöser.Size = new System.Drawing.Size(1038, 120);
            this.tabAuslöser.TabIndex = 1;
            this.tabAuslöser.Text = "Auslöser";
            // 
            // chkAuslöser_export
            // 
            this.chkAuslöser_export.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_export.Location = new System.Drawing.Point(216, 24);
            this.chkAuslöser_export.Name = "chkAuslöser_export";
            this.chkAuslöser_export.QuickInfo = "Das Skript wird vor einem Export ausgeführt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte temporär zu ändern,\r\nVariablen hinzuzufügen oder Bilder zu laden.";
            this.chkAuslöser_export.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_export.TabIndex = 22;
            this.chkAuslöser_export.Text = "Export";
            this.chkAuslöser_export.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_prepaireformula
            // 
            this.chkAuslöser_prepaireformula.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_prepaireformula.Location = new System.Drawing.Point(216, 40);
            this.chkAuslöser_prepaireformula.Name = "chkAuslöser_prepaireformula";
            this.chkAuslöser_prepaireformula.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzprüfung\r\nund für Variablen für Formul" +
    "are.\r\n\r\nEs kann keine Daten ändern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse anstoßen.";
            this.chkAuslöser_prepaireformula.Size = new System.Drawing.Size(175, 16);
            this.chkAuslöser_prepaireformula.TabIndex = 19;
            this.chkAuslöser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAuslöser_prepaireformula.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_databaseloaded
            // 
            this.chkAuslöser_databaseloaded.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_databaseloaded.Location = new System.Drawing.Point(216, 8);
            this.chkAuslöser_databaseloaded.Name = "chkAuslöser_databaseloaded";
            this.chkAuslöser_databaseloaded.QuickInfo = "Das Skript wird direkt nach dem ersten Laden einer Datenbank angestoßen.\r\n\r\nEs ka" +
    "nn verwendet werden, um z.B. Backups zu erstellen.";
            this.chkAuslöser_databaseloaded.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_databaseloaded.TabIndex = 21;
            this.chkAuslöser_databaseloaded.Text = "Datenbank geladen";
            this.chkAuslöser_databaseloaded.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_newrow
            // 
            this.chkAuslöser_newrow.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_newrow.Location = new System.Drawing.Point(8, 8);
            this.chkAuslöser_newrow.Name = "chkAuslöser_newrow";
            this.chkAuslöser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgeführt.";
            this.chkAuslöser_newrow.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_newrow.TabIndex = 17;
            this.chkAuslöser_newrow.Text = "Neue Zeile";
            this.chkAuslöser_newrow.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechangedThread
            // 
            this.chkAuslöser_valuechangedThread.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_valuechangedThread.Location = new System.Drawing.Point(8, 40);
            this.chkAuslöser_valuechangedThread.Name = "chkAuslöser_valuechangedThread";
            this.chkAuslöser_valuechangedThread.QuickInfo = null;
            this.chkAuslöser_valuechangedThread.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_valuechangedThread.TabIndex = 20;
            this.chkAuslöser_valuechangedThread.Text = "Wert geändert <b><fontsize=8><i>Extra Thread";
            this.chkAuslöser_valuechangedThread.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechanged
            // 
            this.chkAuslöser_valuechanged.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_valuechanged.Location = new System.Drawing.Point(8, 24);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.QuickInfo = "Das Skript wird nach dem Ändern eines\r\nWertes einer Zelle ausgeführt";
            this.chkAuslöser_valuechanged.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_valuechanged.TabIndex = 18;
            this.chkAuslöser_valuechanged.Text = "Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // tabRechte
            // 
            this.tabRechte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabRechte.Location = new System.Drawing.Point(4, 25);
            this.tabRechte.Name = "tabRechte";
            this.tabRechte.Size = new System.Drawing.Size(1038, 120);
            this.tabRechte.TabIndex = 2;
            this.tabRechte.Text = "Rechte";
            // 
            // Aussehen
            // 
            this.Aussehen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Aussehen.Location = new System.Drawing.Point(4, 25);
            this.Aussehen.Name = "Aussehen";
            this.Aussehen.Size = new System.Drawing.Size(1038, 120);
            this.Aussehen.TabIndex = 3;
            this.Aussehen.Text = "Aussehen";
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new System.Drawing.Point(3, 3);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new System.Drawing.Size(237, 508);
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
            this.lstEventScripts.CheckBehavior = BlueControls.Enums.CheckBehavior.AlwaysSingleSelection;
            this.lstEventScripts.Location = new System.Drawing.Point(8, 16);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new System.Drawing.Size(222, 236);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new System.EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new System.EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // chkChangeValuesInTest
            // 
            this.chkChangeValuesInTest.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkChangeValuesInTest.Location = new System.Drawing.Point(157, 46);
            this.chkChangeValuesInTest.Name = "chkChangeValuesInTest";
            this.chkChangeValuesInTest.QuickInfo = "<i>Tests im Skript Editor ändern keine Werte,\r\naußer dieses Häkchen ist gesetzt.";
            this.chkChangeValuesInTest.Size = new System.Drawing.Size(376, 22);
            this.chkChangeValuesInTest.TabIndex = 15;
            this.chkChangeValuesInTest.Text = "<Imagecode=Warnung|16> Das Skript darf auch im Test Werte der Datenbank verändern" +
    "";
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new System.Drawing.Point(168, 2);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new System.Drawing.Size(96, 22);
            this.cpZeile.Text = "Zeile für Test:";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Enabled = false;
            this.txbTestZeile.Location = new System.Drawing.Point(157, 24);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new System.Drawing.Size(379, 22);
            this.txbTestZeile.TabIndex = 7;
            // 
            // btnVersionErhöhen
            // 
            this.btnVersionErhöhen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVersionErhöhen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVersionErhöhen.ImageCode = "Pfeil_Oben|16|||||85|0";
            this.btnVersionErhöhen.Location = new System.Drawing.Point(5, 2);
            this.btnVersionErhöhen.Name = "btnVersionErhöhen";
            this.btnVersionErhöhen.QuickInfo = "Skript-Version erhöhen bewirkt,\r\ndass alle Zeilen neu durchgerechnet\r\nwerden.";
            this.btnVersionErhöhen.Size = new System.Drawing.Size(136, 22);
            this.btnVersionErhöhen.TabIndex = 37;
            this.btnVersionErhöhen.Text = "Version erhöhen";
            this.btnVersionErhöhen.Click += new System.EventHandler(this.btnVersionErhöhen_Click);
            // 
            // btnVerlauf
            // 
            this.btnVerlauf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnVerlauf.Enabled = false;
            this.btnVerlauf.ImageCode = "Undo|32";
            this.btnVerlauf.Location = new System.Drawing.Point(200, 2);
            this.btnVerlauf.Name = "btnVerlauf";
            this.btnVerlauf.QuickInfo = "Zeigt den Verlauf in einem\r\nseparatem Fenster an";
            this.btnVerlauf.Size = new System.Drawing.Size(64, 66);
            this.btnVerlauf.TabIndex = 1;
            this.btnVerlauf.Text = "Verlauf";
            this.btnVerlauf.Click += new System.EventHandler(this.btnVerlauf_Click);
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabStart);
            this.ribMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribMain.HotTrack = true;
            this.ribMain.Location = new System.Drawing.Point(0, 0);
            this.ribMain.Name = "ribMain";
            this.ribMain.SelectedIndex = 0;
            this.ribMain.Size = new System.Drawing.Size(1297, 110);
            this.ribMain.TabDefault = null;
            this.ribMain.TabDefaultOrder = new string[0];
            this.ribMain.TabIndex = 97;
            // 
            // tabStart
            // 
            this.tabStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabStart.Controls.Add(this.grpAktionen);
            this.tabStart.Controls.Add(this.grpInfos);
            this.tabStart.Controls.Add(this.grpAusführen);
            this.tabStart.Location = new System.Drawing.Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Padding = new System.Windows.Forms.Padding(3);
            this.tabStart.Size = new System.Drawing.Size(1289, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpAktionen
            // 
            this.grpAktionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAktionen.Controls.Add(this.btnSaveLoad);
            this.grpAktionen.Controls.Add(this.btnVersionErhöhen);
            this.grpAktionen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAktionen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAktionen.Location = new System.Drawing.Point(920, 3);
            this.grpAktionen.Name = "grpAktionen";
            this.grpAktionen.Size = new System.Drawing.Size(301, 75);
            this.grpAktionen.TabIndex = 2;
            this.grpAktionen.TabStop = false;
            this.grpAktionen.Text = "Aktionen";
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSaveLoad.ImageCode = "Refresh|16";
            this.btnSaveLoad.Location = new System.Drawing.Point(160, 2);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.QuickInfo = "Aktualisiert die Datenbank-Daten. (Speichern, neu Laden)";
            this.btnSaveLoad.Size = new System.Drawing.Size(48, 66);
            this.btnSaveLoad.TabIndex = 45;
            this.btnSaveLoad.Text = "Daten aktual.";
            this.btnSaveLoad.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // grpInfos
            // 
            this.grpInfos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpInfos.Controls.Add(this.btnDatenbankKopf);
            this.grpInfos.Controls.Add(this.btnSpaltenuebersicht);
            this.grpInfos.Controls.Add(this.btnVerlauf);
            this.grpInfos.Controls.Add(this.btnBefehlsUebersicht);
            this.grpInfos.Controls.Add(this.btnZusatzDateien);
            this.grpInfos.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpInfos.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpInfos.Location = new System.Drawing.Point(547, 3);
            this.grpInfos.Name = "grpInfos";
            this.grpInfos.Size = new System.Drawing.Size(373, 75);
            this.grpInfos.TabIndex = 1;
            this.grpInfos.TabStop = false;
            this.grpInfos.Text = "Infos";
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new System.Drawing.Point(264, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new System.Drawing.Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 45;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new System.EventHandler(this.btnDatenbankKopf_Click);
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
            // btnBefehlsUebersicht
            // 
            this.btnBefehlsUebersicht.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnBefehlsUebersicht.ImageCode = "Tabelle|16";
            this.btnBefehlsUebersicht.Location = new System.Drawing.Point(8, 2);
            this.btnBefehlsUebersicht.Name = "btnBefehlsUebersicht";
            this.btnBefehlsUebersicht.Size = new System.Drawing.Size(64, 66);
            this.btnBefehlsUebersicht.TabIndex = 4;
            this.btnBefehlsUebersicht.Text = "Befehls-Übersicht";
            // 
            // btnZusatzDateien
            // 
            this.btnZusatzDateien.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnZusatzDateien.ImageCode = "Ordner|16";
            this.btnZusatzDateien.Location = new System.Drawing.Point(72, 2);
            this.btnZusatzDateien.Name = "btnZusatzDateien";
            this.btnZusatzDateien.QuickInfo = "Den Ordner der Zusatzdatein öffnen.\r\nIn diesen können z.B. Skript-Routinen enthal" +
    "ten sein\r\ndie mit CallByFilename aufgerufen werden können.";
            this.btnZusatzDateien.Size = new System.Drawing.Size(64, 66);
            this.btnZusatzDateien.TabIndex = 5;
            this.btnZusatzDateien.Text = "Zusatz-dateien";
            // 
            // grpAusführen
            // 
            this.grpAusführen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAusführen.Controls.Add(this.chkChangeValuesInTest);
            this.grpAusführen.Controls.Add(this.btnTest);
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            this.grpAusführen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAusführen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAusführen.Location = new System.Drawing.Point(3, 3);
            this.grpAusführen.Name = "grpAusführen";
            this.grpAusführen.Size = new System.Drawing.Size(544, 75);
            this.grpAusführen.TabIndex = 0;
            this.grpAusführen.TabStop = false;
            this.grpAusführen.Text = "Ausführen";
            // 
            // btnTest
            // 
            this.btnTest.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnTest.ImageCode = "Abspielen|16";
            this.btnTest.Location = new System.Drawing.Point(8, 2);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(60, 66);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "Testen";
            // 
            // DatabaseScriptEditor
            // 
            this.ClientSize = new System.Drawing.Size(1297, 677);
            this.Controls.Add(this.GlobalTab);
            this.Controls.Add(this.ribMain);
            this.MinimizeBox = false;
            this.Name = "DatabaseScriptEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.GlobalTab, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabScripts.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tbcScriptEigenschaften.ResumeLayout(false);
            this.tabHaupt.ResumeLayout(false);
            this.tabAuslöser.ResumeLayout(false);
            this.grpVerfügbareSkripte.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TabControl GlobalTab;
        private TabPage tabScripts;
        private GroupBox grpVerfügbareSkripte;
        private ListBox lstEventScripts;
        private ScriptEditor eventScriptEditor;
        private Button chkAuslöser_prepaireformula;
        private Button chkAuslöser_valuechanged;
        private Button chkAuslöser_newrow;
        private Button chkAendertWerte;
        private Button chkExternVerfügbar;
        private Button chkZeile;
        private TextBox txbName;
        private Caption capName;
        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button chkAuslöser_valuechangedThread;
        private Panel panel1;
        private Button chkAuslöser_databaseloaded;
        private Button chkAuslöser_export;
        private Button chkChangeValuesInTest;
        private Button btnVersionErhöhen;
        private Button btnVerlauf;
        private RibbonBar ribMain;
        private TabPage tabStart;
        private GroupBox grpInfos;
        private Button btnBefehlsUebersicht;
        private Button btnZusatzDateien;
        private GroupBox grpAusführen;
        private Button btnTest;
        private GroupBox grpAktionen;
        private TabControl tbcScriptEigenschaften;
        private TabPage tabHaupt;
        private TabPage tabAuslöser;
        private TabPage tabRechte;
        private TabPage Aussehen;
        private Button btnSaveLoad;
        private Button btnSpaltenuebersicht;
        private Button btnDatenbankKopf;
    }
}
