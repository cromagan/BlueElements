using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase.Enums;
using BlueDatabase;
using BlueScript.Structures;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class DatabaseScriptEditor : FormWithStatusBar {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseScriptEditor));
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.tabScripts = new System.Windows.Forms.TabPage();
            this.eventScriptEditor = new BlueControls.ScriptEditor();
            this.grpZeile = new BlueControls.Controls.GroupBox();
            this.capAnmerkung = new BlueControls.Controls.Caption();
            this.btnSave = new BlueControls.Controls.Button();
            this.cpZeile = new BlueControls.Controls.Caption();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.grpEigenschaften = new BlueControls.Controls.GroupBox();
            this.chkAusl�ser_errorcheck = new BlueControls.Controls.Button();
            this.chkAusl�ser_valuechanged = new BlueControls.Controls.Button();
            this.chkAusl�ser_newrow = new BlueControls.Controls.Button();
            this.chkAendertWerte = new BlueControls.Controls.Button();
            this.chkExternVerf�gbar = new BlueControls.Controls.Button();
            this.chkZeile = new BlueControls.Controls.Button();
            this.txbName = new BlueControls.Controls.TextBox();
            this.capName = new BlueControls.Controls.Caption();
            this.grpVerf�gbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.tabVariablen = new System.Windows.Forms.TabPage();
            this.variableEditor = new BlueControls.VariableEditor();
            this.pnlStatusBar.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.grpZeile.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpVerf�gbareSkripte.SuspendLayout();
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
            this.GlobalTab.RowKey = ((long)(-1));
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
            this.tabScripts.Controls.Add(this.grpZeile);
            this.tabScripts.Controls.Add(this.grpEigenschaften);
            this.tabScripts.Controls.Add(this.grpVerf�gbareSkripte);
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
            this.eventScriptEditor.Location = new System.Drawing.Point(240, 128);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.ScriptText = "";
            this.eventScriptEditor.Size = new System.Drawing.Size(1046, 493);
            this.eventScriptEditor.TabIndex = 6;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Changed += new System.EventHandler(this.ScriptEditor_Changed);
            this.eventScriptEditor.ExecuteScript += new System.EventHandler<BlueScript.EventArgs.ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            // 
            // grpZeile
            // 
            this.grpZeile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpZeile.Controls.Add(this.capAnmerkung);
            this.grpZeile.Controls.Add(this.btnSave);
            this.grpZeile.Controls.Add(this.cpZeile);
            this.grpZeile.Controls.Add(this.txbTestZeile);
            this.grpZeile.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpZeile.Location = new System.Drawing.Point(240, 80);
            this.grpZeile.Name = "grpZeile";
            this.grpZeile.Size = new System.Drawing.Size(1046, 48);
            this.grpZeile.TabIndex = 8;
            this.grpZeile.TabStop = false;
            this.grpZeile.Text = "Zeile";
            // 
            // capAnmerkung
            // 
            this.capAnmerkung.CausesValidation = false;
            this.capAnmerkung.Location = new System.Drawing.Point(376, 16);
            this.capAnmerkung.Name = "capAnmerkung";
            this.capAnmerkung.Size = new System.Drawing.Size(288, 22);
            this.capAnmerkung.Text = "<i>Tests im Skript Editor �ndern keine Werte!";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new System.Drawing.Point(928, 16);
            this.btnSave.Name = "btnSave";
            this.btnSave.QuickInfo = "Datenbank und die �nderungen am Skript\r\nfest auf den Datentr�ger speichern.";
            this.btnSave.Size = new System.Drawing.Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new System.Drawing.Point(8, 16);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new System.Drawing.Size(96, 22);
            this.cpZeile.Text = "Zeile f�r Test:";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Enabled = false;
            this.txbTestZeile.Location = new System.Drawing.Point(112, 16);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new System.Drawing.Size(256, 24);
            this.txbTestZeile.TabIndex = 7;
            // 
            // grpEigenschaften
            // 
            this.grpEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEigenschaften.Controls.Add(this.chkAusl�ser_errorcheck);
            this.grpEigenschaften.Controls.Add(this.chkAusl�ser_valuechanged);
            this.grpEigenschaften.Controls.Add(this.chkAusl�ser_newrow);
            this.grpEigenschaften.Controls.Add(this.chkAendertWerte);
            this.grpEigenschaften.Controls.Add(this.chkExternVerf�gbar);
            this.grpEigenschaften.Controls.Add(this.chkZeile);
            this.grpEigenschaften.Controls.Add(this.txbName);
            this.grpEigenschaften.Controls.Add(this.capName);
            this.grpEigenschaften.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpEigenschaften.Location = new System.Drawing.Point(240, 3);
            this.grpEigenschaften.Name = "grpEigenschaften";
            this.grpEigenschaften.Size = new System.Drawing.Size(1046, 77);
            this.grpEigenschaften.TabIndex = 4;
            this.grpEigenschaften.TabStop = false;
            this.grpEigenschaften.Text = "Eigenschaften";
            // 
            // chkAusl�ser_errorcheck
            // 
            this.chkAusl�ser_errorcheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAusl�ser_errorcheck.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_errorcheck.Location = new System.Drawing.Point(852, 48);
            this.chkAusl�ser_errorcheck.Name = "chkAusl�ser_errorcheck";
            this.chkAusl�ser_errorcheck.QuickInfo = "Das Skript wird nur zur Datenkonsitenzpr�fung\r\nverendet und �ndert keine Daten.";
            this.chkAusl�ser_errorcheck.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_errorcheck.TabIndex = 19;
            this.chkAusl�ser_errorcheck.Text = "Ausl�ser: Fehlerpr�fung";
            this.chkAusl�ser_errorcheck.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_valuechanged
            // 
            this.chkAusl�ser_valuechanged.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAusl�ser_valuechanged.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_valuechanged.Location = new System.Drawing.Point(852, 32);
            this.chkAusl�ser_valuechanged.Name = "chkAusl�ser_valuechanged";
            this.chkAusl�ser_valuechanged.QuickInfo = "Das Skript wir nach dem �ndern eines\r\nWertes einer Zelle ausgef�hrt";
            this.chkAusl�ser_valuechanged.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_valuechanged.TabIndex = 18;
            this.chkAusl�ser_valuechanged.Text = "Ausl�ser: Wert ge�ndert";
            this.chkAusl�ser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_newrow
            // 
            this.chkAusl�ser_newrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAusl�ser_newrow.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_newrow.Location = new System.Drawing.Point(852, 16);
            this.chkAusl�ser_newrow.Name = "chkAusl�ser_newrow";
            this.chkAusl�ser_newrow.QuickInfo = "Das Skript wir nach dem Erstellen einer\r\nneuen Zeile ausgef�hrt.";
            this.chkAusl�ser_newrow.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_newrow.TabIndex = 17;
            this.chkAusl�ser_newrow.Text = "Ausl�ser: Neue Zeile";
            this.chkAusl�ser_newrow.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAendertWerte
            // 
            this.chkAendertWerte.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAendertWerte.Location = new System.Drawing.Point(288, 48);
            this.chkAendertWerte.Name = "chkAendertWerte";
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgef�hrt um dessen\r\nBerechnungen abzugreifen.\r\n�nderungen w" +
    "erden nicht in die Datenbank\r\nzur�ckgespielt";
            this.chkAendertWerte.Size = new System.Drawing.Size(120, 16);
            this.chkAendertWerte.TabIndex = 16;
            this.chkAendertWerte.Text = "�ndert Werte";
            this.chkAendertWerte.CheckedChanged += new System.EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // chkExternVerf�gbar
            // 
            this.chkExternVerf�gbar.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkExternVerf�gbar.Location = new System.Drawing.Point(160, 48);
            this.chkExternVerf�gbar.Name = "chkExternVerf�gbar";
            this.chkExternVerf�gbar.QuickInfo = "Wenn das Skript �ber eine Men�leiste oder dem Kontextmen�\r\nw�hlbar sein soll, mus" +
    "s dieses H�kchen gesetzt sein.";
            this.chkExternVerf�gbar.Size = new System.Drawing.Size(120, 16);
            this.chkExternVerf�gbar.TabIndex = 15;
            this.chkExternVerf�gbar.Text = "Extern verf�gbar";
            this.chkExternVerf�gbar.CheckedChanged += new System.EventHandler(this.chkExternVerf�gbar_CheckedChanged);
            // 
            // chkZeile
            // 
            this.chkZeile.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkZeile.Location = new System.Drawing.Point(64, 48);
            this.chkZeile.Name = "chkZeile";
            this.chkZeile.QuickInfo = "Wenn das Skript Zellwerte der aktuellen Zeile �ndern k�nnen soll,\r\nmuss dieses H�" +
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
            this.txbName.Size = new System.Drawing.Size(772, 22);
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
            // grpVerf�gbareSkripte
            // 
            this.grpVerf�gbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerf�gbareSkripte.CausesValidation = false;
            this.grpVerf�gbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerf�gbareSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVerf�gbareSkripte.Location = new System.Drawing.Point(3, 3);
            this.grpVerf�gbareSkripte.Name = "grpVerf�gbareSkripte";
            this.grpVerf�gbareSkripte.Size = new System.Drawing.Size(237, 618);
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
            this.lstEventScripts.FilterAllowed = true;
            this.lstEventScripts.Location = new System.Drawing.Point(8, 24);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new System.Drawing.Size(222, 586);
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
            this.tabVariablen.Size = new System.Drawing.Size(1289, 648);
            this.tabVariablen.TabIndex = 3;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new System.Drawing.Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new System.Drawing.Size(1283, 642);
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
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.GlobalTab, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabScripts.ResumeLayout(false);
            this.grpZeile.ResumeLayout(false);
            this.grpEigenschaften.ResumeLayout(false);
            this.grpVerf�gbareSkripte.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TabPage tabVariablen;
        private TabControl GlobalTab;
        private Button btnSave;
        private TabPage tabScripts;
        private GroupBox grpVerf�gbareSkripte;
        private ListBox lstEventScripts;
        private VariableEditor variableEditor;
        private GroupBox grpEigenschaften;
        private ScriptEditor eventScriptEditor;
        private Button chkAusl�ser_errorcheck;
        private Button chkAusl�ser_valuechanged;
        private Button chkAusl�ser_newrow;
        private Button chkAendertWerte;
        private Button chkExternVerf�gbar;
        private Button chkZeile;
        private TextBox txbName;
        private Caption capName;
        private TextBox txbTestZeile;
        private GroupBox grpZeile;
        private Caption capAnmerkung;
        private Caption cpZeile;
    }
}
