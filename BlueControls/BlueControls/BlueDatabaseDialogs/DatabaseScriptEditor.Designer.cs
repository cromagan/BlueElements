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
            this.grpZeile = new BlueControls.Controls.GroupBox();
            this.chkChangeValuesInTest = new BlueControls.Controls.Button();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.grpEigenschaften = new BlueControls.Controls.GroupBox();
            this.btnVersionErhöhen = new BlueControls.Controls.Button();
            this.btnSpaltenuebersicht = new BlueControls.Controls.Button();
            this.chkAendertWerte = new BlueControls.Controls.Button();
            this.btnSave = new BlueControls.Controls.Button();
            this.chkExternVerfügbar = new BlueControls.Controls.Button();
            this.chkZeile = new BlueControls.Controls.Button();
            this.txbName = new BlueControls.Controls.TextBox();
            this.capName = new BlueControls.Controls.Caption();
            this.grpAuslöser = new BlueControls.Controls.GroupBox();
            this.chkAuslöser_export = new BlueControls.Controls.Button();
            this.chkAuslöser_databaseloaded = new BlueControls.Controls.Button();
            this.chkAuslöser_newrow = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechangedThread = new BlueControls.Controls.Button();
            this.chkAuslöser_valuechanged = new BlueControls.Controls.Button();
            this.chkAuslöser_prepaireformula = new BlueControls.Controls.Button();
            this.grpVerfügbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.tabVariablen = new System.Windows.Forms.TabPage();
            this.variableEditor = new BlueControls.VariableEditor();
            this.pnlStatusBar.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpZeile.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpAuslöser.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
            this.tabVariablen.SuspendLayout();
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
            this.GlobalTab.Controls.Add(this.tabVariablen);
            this.GlobalTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new System.Drawing.Size(1297, 653);
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
            this.tabScripts.Size = new System.Drawing.Size(1289, 624);
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
            this.eventScriptEditor.Size = new System.Drawing.Size(1046, 469);
            this.eventScriptEditor.TabIndex = 6;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Changed += new System.EventHandler(this.ScriptEditor_Changed);
            this.eventScriptEditor.ExecuteScript += new System.EventHandler<BlueScript.EventArgs.ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.grpZeile);
            this.panel1.Controls.Add(this.grpEigenschaften);
            this.panel1.Controls.Add(this.grpAuslöser);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(240, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1046, 149);
            this.panel1.TabIndex = 22;
            // 
            // grpZeile
            // 
            this.grpZeile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpZeile.Controls.Add(this.chkChangeValuesInTest);
            this.grpZeile.Controls.Add(this.cpZeile);
            this.grpZeile.Controls.Add(this.txbTestZeile);
            this.grpZeile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpZeile.Location = new System.Drawing.Point(0, 77);
            this.grpZeile.Name = "grpZeile";
            this.grpZeile.Size = new System.Drawing.Size(848, 72);
            this.grpZeile.TabIndex = 8;
            this.grpZeile.TabStop = false;
            this.grpZeile.Text = "Zeile";
            // 
            // chkChangeValuesInTest
            // 
            this.chkChangeValuesInTest.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkChangeValuesInTest.Location = new System.Drawing.Point(112, 40);
            this.chkChangeValuesInTest.Name = "chkChangeValuesInTest";
            this.chkChangeValuesInTest.QuickInfo = "<i>Tests im Skript Editor ändern keine Werte,\r\naußer dieses Häkchen ist gesetzt.";
            this.chkChangeValuesInTest.Size = new System.Drawing.Size(376, 24);
            this.chkChangeValuesInTest.TabIndex = 15;
            this.chkChangeValuesInTest.Text = "<Imagecode=Warnung|16> Das Skript darf auch im Test Werte der Datenbank verändern" +
    "";
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new System.Drawing.Point(8, 16);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new System.Drawing.Size(96, 22);
            this.cpZeile.Text = "Zeile für Test:";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Enabled = false;
            this.txbTestZeile.Location = new System.Drawing.Point(112, 16);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new System.Drawing.Size(536, 24);
            this.txbTestZeile.TabIndex = 7;
            // 
            // grpEigenschaften
            // 
            this.grpEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEigenschaften.Controls.Add(this.btnVersionErhöhen);
            this.grpEigenschaften.Controls.Add(this.btnSpaltenuebersicht);
            this.grpEigenschaften.Controls.Add(this.chkAendertWerte);
            this.grpEigenschaften.Controls.Add(this.btnSave);
            this.grpEigenschaften.Controls.Add(this.chkExternVerfügbar);
            this.grpEigenschaften.Controls.Add(this.chkZeile);
            this.grpEigenschaften.Controls.Add(this.txbName);
            this.grpEigenschaften.Controls.Add(this.capName);
            this.grpEigenschaften.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpEigenschaften.Location = new System.Drawing.Point(0, 0);
            this.grpEigenschaften.Name = "grpEigenschaften";
            this.grpEigenschaften.Size = new System.Drawing.Size(848, 77);
            this.grpEigenschaften.TabIndex = 4;
            this.grpEigenschaften.TabStop = false;
            this.grpEigenschaften.Text = "Eigenschaften";
            // 
            // btnVersionErhöhen
            // 
            this.btnVersionErhöhen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVersionErhöhen.ImageCode = "Pfeil_Oben|16|||||85|0";
            this.btnVersionErhöhen.Location = new System.Drawing.Point(584, 48);
            this.btnVersionErhöhen.Name = "btnVersionErhöhen";
            this.btnVersionErhöhen.QuickInfo = "Skript-Version erhöhen bewirkt,\r\ndass alle Zeilen neu durchgerechnet\r\nwerden.";
            this.btnVersionErhöhen.Size = new System.Drawing.Size(136, 24);
            this.btnVersionErhöhen.TabIndex = 37;
            this.btnVersionErhöhen.Text = "Version erhöhen";
            this.btnVersionErhöhen.Click += new System.EventHandler(this.btnVersionErhöhen_Click);
            // 
            // btnSpaltenuebersicht
            // 
            this.btnSpaltenuebersicht.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSpaltenuebersicht.ImageCode = "Spalte|16";
            this.btnSpaltenuebersicht.Location = new System.Drawing.Point(440, 48);
            this.btnSpaltenuebersicht.Name = "btnSpaltenuebersicht";
            this.btnSpaltenuebersicht.Size = new System.Drawing.Size(136, 24);
            this.btnSpaltenuebersicht.TabIndex = 36;
            this.btnSpaltenuebersicht.Text = "Spaltenübersicht";
            this.btnSpaltenuebersicht.Click += new System.EventHandler(this.btnSpaltenuebersicht_Click);
            // 
            // chkAendertWerte
            // 
            this.chkAendertWerte.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAendertWerte.Location = new System.Drawing.Point(288, 48);
            this.chkAendertWerte.Name = "chkAendertWerte";
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgeführt um dessen\r\nBerechnungen abzugreifen.\r\nÄnderungen w" +
    "erden nicht in die Datenbank\r\nzurückgespielt";
            this.chkAendertWerte.Size = new System.Drawing.Size(120, 16);
            this.chkAendertWerte.TabIndex = 16;
            this.chkAendertWerte.Text = "Ändert Werte";
            this.chkAendertWerte.CheckedChanged += new System.EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new System.Drawing.Point(728, 48);
            this.btnSave.Name = "btnSave";
            this.btnSave.QuickInfo = "Datenbank und die änderungen am Skript\r\nfest auf den Datenträger speichern.";
            this.btnSave.Size = new System.Drawing.Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkExternVerfügbar
            // 
            this.chkExternVerfügbar.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkExternVerfügbar.Location = new System.Drawing.Point(160, 48);
            this.chkExternVerfügbar.Name = "chkExternVerfügbar";
            this.chkExternVerfügbar.QuickInfo = "Wenn das Skript über eine Menüleiste oder dem Kontextmenü\r\nwählbar sein soll, mus" +
    "s dieses Häkchen gesetzt sein.";
            this.chkExternVerfügbar.Size = new System.Drawing.Size(120, 16);
            this.chkExternVerfügbar.TabIndex = 15;
            this.chkExternVerfügbar.Text = "Extern verfügbar";
            this.chkExternVerfügbar.CheckedChanged += new System.EventHandler(this.chkExternVerfügbar_CheckedChanged);
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkZeile.Location = new System.Drawing.Point(64, 48);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile ändern können soll,\r\nmuss dieses Hä" +
    "kchen gesetzt sein.";
            this.chkZeile.Size = new System.Drawing.Size(88, 16);
            this.chkZeile.TabIndex = 14;
            this.chkZeile.Text = "Zeilen-Skript";
            this.chkZeile.CheckedChanged += new System.EventHandler(this.chkZeile_CheckedChanged);
            // 
            // txbName
            // 
            this.txbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbName.Location = new System.Drawing.Point(64, 24);
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(775, 22);
            this.txbName.TabIndex = 13;
            this.txbName.TextChanged += new System.EventHandler(this.txbName_TextChanged);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new System.Drawing.Point(8, 24);
            this.capName.Name = "capName";
            this.capName.Size = new System.Drawing.Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // grpAuslöser
            // 
            this.grpAuslöser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuslöser.Controls.Add(this.chkAuslöser_export);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_databaseloaded);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_newrow);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechangedThread);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechanged);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_prepaireformula);
            this.grpAuslöser.Dock = System.Windows.Forms.DockStyle.Right;
            this.grpAuslöser.Location = new System.Drawing.Point(848, 0);
            this.grpAuslöser.Name = "grpAuslöser";
            this.grpAuslöser.Size = new System.Drawing.Size(198, 149);
            this.grpAuslöser.TabIndex = 21;
            this.grpAuslöser.TabStop = false;
            this.grpAuslöser.Text = "Auslöser";
            // 
            // chkAuslöser_export
            // 
            this.chkAuslöser_export.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_export.Location = new System.Drawing.Point(8, 96);
            this.chkAuslöser_export.Name = "chkAuslöser_export";
            this.chkAuslöser_export.QuickInfo = "Das Skript wird vor einem Export ausgeführt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte temporär zu ändern,\r\nVariablen hinzuzufügen oder Bilder zu laden.";
            this.chkAuslöser_export.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_export.TabIndex = 22;
            this.chkAuslöser_export.Text = "Export";
            this.chkAuslöser_export.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_databaseloaded
            // 
            this.chkAuslöser_databaseloaded.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_databaseloaded.Location = new System.Drawing.Point(8, 80);
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
            this.chkAuslöser_newrow.Location = new System.Drawing.Point(9, 32);
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
            this.chkAuslöser_valuechangedThread.Location = new System.Drawing.Point(9, 64);
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
            this.chkAuslöser_valuechanged.Location = new System.Drawing.Point(9, 48);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.QuickInfo = "Das Skript wird nach dem Ändern eines\r\nWertes einer Zelle ausgeführt";
            this.chkAuslöser_valuechanged.Size = new System.Drawing.Size(176, 16);
            this.chkAuslöser_valuechanged.TabIndex = 18;
            this.chkAuslöser_valuechanged.Text = "Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_prepaireformula
            // 
            this.chkAuslöser_prepaireformula.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkAuslöser_prepaireformula.Location = new System.Drawing.Point(9, 16);
            this.chkAuslöser_prepaireformula.Name = "chkAuslöser_prepaireformula";
            this.chkAuslöser_prepaireformula.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzprüfung\r\nund für Variablen für Formul" +
    "are.\r\n\r\nEs kann keine Daten ändern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse anstoßen.";
            this.chkAuslöser_prepaireformula.Size = new System.Drawing.Size(175, 16);
            this.chkAuslöser_prepaireformula.TabIndex = 19;
            this.chkAuslöser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAuslöser_prepaireformula.CheckedChanged += new System.EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new System.Drawing.Point(3, 3);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new System.Drawing.Size(237, 618);
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
            this.lstEventScripts.Size = new System.Drawing.Size(222, 594);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new System.EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new System.EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.variableEditor);
            this.tabVariablen.Location = new System.Drawing.Point(4, 25);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Padding = new System.Windows.Forms.Padding(3);
            this.tabVariablen.Size = new System.Drawing.Size(1289, 624);
            this.tabVariablen.TabIndex = 3;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new System.Drawing.Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new System.Drawing.Size(1283, 618);
            this.variableEditor.TabIndex = 0;
            // 
            // DatabaseScriptEditor
            // 
            this.ClientSize = new System.Drawing.Size(1297, 677);
            this.Controls.Add(this.GlobalTab);
            this.MinimizeBox = false;
            this.Name = "DatabaseScriptEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.GlobalTab, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabScripts.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.grpZeile.ResumeLayout(false);
            this.grpEigenschaften.ResumeLayout(false);
            this.grpAuslöser.ResumeLayout(false);
            this.grpVerfügbareSkripte.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TabPage tabVariablen;
        private TabControl GlobalTab;
        private Button btnSave;
        private TabPage tabScripts;
        private GroupBox grpVerfügbareSkripte;
        private ListBox lstEventScripts;
        private VariableEditor variableEditor;
        private GroupBox grpEigenschaften;
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
        private GroupBox grpZeile;
        private Caption cpZeile;
        private Button chkAuslöser_valuechangedThread;
        private Panel panel1;
        private GroupBox grpAuslöser;
        private Button chkAuslöser_databaseloaded;
        private Button chkAuslöser_export;
        private Button btnSpaltenuebersicht;
        private Button chkChangeValuesInTest;
        private Button btnVersionErhöhen;
    }
}
