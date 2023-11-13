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

namespace BlueControls.ConnectedFormula {
    public sealed partial class ConnectedFormulaScriptEditor : FormWithStatusBar {
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
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.tabScripts = new System.Windows.Forms.TabPage();
            this.eventScriptEditor = new BlueControls.ScriptEditor();
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpEigenschaften = new BlueControls.Controls.GroupBox();
            this.chkAendertWerte = new BlueControls.Controls.Button();
            this.btnSave = new BlueControls.Controls.Button();
            this.chkExternVerf�gbar = new BlueControls.Controls.Button();
            this.txbName = new BlueControls.Controls.TextBox();
            this.capName = new BlueControls.Controls.Caption();
            this.grpAusl�ser = new BlueControls.Controls.GroupBox();
            this.chkAusl�ser_valuechangedThread = new BlueControls.Controls.Button();
            this.chkAusl�ser_valuechanged = new BlueControls.Controls.Button();
            this.chkAusl�ser_prepaireformula = new BlueControls.Controls.Button();
            this.grpVerf�gbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.tabVariablen = new System.Windows.Forms.TabPage();
            this.variableEditor = new BlueControls.VariableEditor();
            this.chkAusl�ser_formulaloaded = new BlueControls.Controls.Button();
            this.chkAusl�ser_export = new BlueControls.Controls.Button();
            this.button1 = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpAusl�ser.SuspendLayout();
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
            this.panel1.Controls.Add(this.grpEigenschaften);
            this.panel1.Controls.Add(this.grpAusl�ser);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(240, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1046, 149);
            this.panel1.TabIndex = 22;
            // 
            // grpEigenschaften
            // 
            this.grpEigenschaften.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEigenschaften.Controls.Add(this.chkAendertWerte);
            this.grpEigenschaften.Controls.Add(this.btnSave);
            this.grpEigenschaften.Controls.Add(this.chkExternVerf�gbar);
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
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new System.Drawing.Point(727, 48);
            this.btnSave.Name = "btnSave";
            this.btnSave.QuickInfo = "Datenbank und die �nderungen am Skript\r\nfest auf den Datentr�ger speichern.";
            this.btnSave.Size = new System.Drawing.Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            // grpAusl�ser
            // 
            this.grpAusl�ser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAusl�ser.Controls.Add(this.button1);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_export);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_formulaloaded);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_valuechangedThread);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_valuechanged);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_prepaireformula);
            this.grpAusl�ser.Dock = System.Windows.Forms.DockStyle.Right;
            this.grpAusl�ser.Location = new System.Drawing.Point(848, 0);
            this.grpAusl�ser.Name = "grpAusl�ser";
            this.grpAusl�ser.Size = new System.Drawing.Size(198, 149);
            this.grpAusl�ser.TabIndex = 21;
            this.grpAusl�ser.TabStop = false;
            this.grpAusl�ser.Text = "Ausl�ser";
            // 
            // chkAusl�ser_valuechangedThread
            // 
            this.chkAusl�ser_valuechangedThread.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_valuechangedThread.Location = new System.Drawing.Point(9, 64);
            this.chkAusl�ser_valuechangedThread.Name = "chkAusl�ser_valuechangedThread";
            this.chkAusl�ser_valuechangedThread.QuickInfo = null;
            this.chkAusl�ser_valuechangedThread.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_valuechangedThread.TabIndex = 20;
            this.chkAusl�ser_valuechangedThread.Text = "Wert ge�ndert <b><fontsize=8><i>Extra Thread";
            this.chkAusl�ser_valuechangedThread.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_valuechanged
            // 
            this.chkAusl�ser_valuechanged.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_valuechanged.Location = new System.Drawing.Point(9, 48);
            this.chkAusl�ser_valuechanged.Name = "chkAusl�ser_valuechanged";
            this.chkAusl�ser_valuechanged.QuickInfo = "Das Skript wird nach dem �ndern eines\r\nWertes einer Zelle ausgef�hrt";
            this.chkAusl�ser_valuechanged.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_valuechanged.TabIndex = 18;
            this.chkAusl�ser_valuechanged.Text = "Wert ge�ndert";
            this.chkAusl�ser_valuechanged.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_prepaireformula
            // 
            this.chkAusl�ser_prepaireformula.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_prepaireformula.Location = new System.Drawing.Point(8, 32);
            this.chkAusl�ser_prepaireformula.Name = "chkAusl�ser_prepaireformula";
            this.chkAusl�ser_prepaireformula.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzpr�fung\r\nund f�r Variablen f�r Formul" +
    "are.\r\n\r\nEs kann keine Daten �ndern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse ansto�en.";
            this.chkAusl�ser_prepaireformula.Size = new System.Drawing.Size(175, 16);
            this.chkAusl�ser_prepaireformula.TabIndex = 19;
            this.chkAusl�ser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAusl�ser_prepaireformula.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
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
            // chkAusl�ser_formulaloaded
            // 
            this.chkAusl�ser_formulaloaded.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_formulaloaded.Location = new System.Drawing.Point(8, 80);
            this.chkAusl�ser_formulaloaded.Name = "chkAusl�ser_formulaloaded";
            this.chkAusl�ser_formulaloaded.QuickInfo = "Das Skript wird direkt nach dem ersten Laden einer Datenbank angesto�en.\r\n\r\nEs ka" +
    "nn verwendet werden, um z.B. Backups zu erstellen.";
            this.chkAusl�ser_formulaloaded.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_formulaloaded.TabIndex = 21;
            this.chkAusl�ser_formulaloaded.Text = "Formular geladen";
            this.chkAusl�ser_formulaloaded.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_export
            // 
            this.chkAusl�ser_export.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.chkAusl�ser_export.Location = new System.Drawing.Point(8, 96);
            this.chkAusl�ser_export.Name = "chkAusl�ser_export";
            this.chkAusl�ser_export.QuickInfo = "Das Skript wird vor einem Export ausgef�hrt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte tempor�r zu �ndern,\r\nVariablen hinzuzuf�gen oder Bilder zu laden.";
            this.chkAusl�ser_export.Size = new System.Drawing.Size(176, 16);
            this.chkAusl�ser_export.TabIndex = 22;
            this.chkAusl�ser_export.Text = "Export";
            this.chkAusl�ser_export.CheckedChanged += new System.EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // button1
            // 
            this.button1.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Checkbox | BlueControls.Enums.ButtonStyle.Text)));
            this.button1.Location = new System.Drawing.Point(8, 16);
            this.button1.Name = "button1";
            this.button1.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzpr�fung\r\nund f�r Variablen f�r Formul" +
    "are.\r\n\r\nEs kann keine Daten �ndern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse ansto�en.";
            this.button1.Size = new System.Drawing.Size(175, 16);
            this.button1.TabIndex = 23;
            this.button1.Text = "Clipbard Ver�nderung";
            // 
            // ConnectedFormulaScriptEditor
            // 
            this.ClientSize = new System.Drawing.Size(1297, 677);
            this.Controls.Add(this.GlobalTab);
            this.MinimizeBox = false;
            this.Name = "ConnectedFormulaScriptEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.GlobalTab, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.GlobalTab.ResumeLayout(false);
            this.tabScripts.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.grpEigenschaften.ResumeLayout(false);
            this.grpAusl�ser.ResumeLayout(false);
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
        private Button chkAusl�ser_prepaireformula;
        private Button chkAusl�ser_valuechanged;
        private Button chkAendertWerte;
        private Button chkExternVerf�gbar;
        private TextBox txbName;
        private Caption capName;
        private Button chkAusl�ser_valuechangedThread;
        private Panel panel1;
        private GroupBox grpAusl�ser;
        private Button chkAusl�ser_export;
        private Button chkAusl�ser_formulaloaded;
        private Button button1;
    }
}
