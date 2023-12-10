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
            this.GlobalTab = new TabControl();
            this.tabScripts = new TabPage();
            this.eventScriptEditor = new ScriptEditor();
            this.panel1 = new Panel();
            this.grpEigenschaften = new GroupBox();
            this.chkAendertWerte = new Button();
            this.btnSave = new Button();
            this.chkExternVerf�gbar = new Button();
            this.txbName = new TextBox();
            this.capName = new Caption();
            this.grpAusl�ser = new GroupBox();
            this.chkAusl�ser_valuechangedThread = new Button();
            this.chkAusl�ser_valuechanged = new Button();
            this.chkAusl�ser_prepaireformula = new Button();
            this.grpVerf�gbareSkripte = new GroupBox();
            this.lstEventScripts = new ListBox();
            this.tabVariablen = new TabPage();
            this.variableEditor = new VariableEditor();
            this.chkAusl�ser_formulaloaded = new Button();
            this.chkAusl�ser_export = new Button();
            this.button1 = new Button();
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
            this.capStatusBar.Size = new Size(1297, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 653);
            this.pnlStatusBar.Size = new Size(1297, 24);
            // 
            // GlobalTab
            // 
            this.GlobalTab.Controls.Add(this.tabScripts);
            this.GlobalTab.Controls.Add(this.tabVariablen);
            this.GlobalTab.Dock = DockStyle.Fill;
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new Size(1297, 653);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.SelectedIndexChanged += new EventHandler(this.GlobalTab_SelectedIndexChanged);
            // 
            // tabScripts
            // 
            this.tabScripts.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScripts.Controls.Add(this.eventScriptEditor);
            this.tabScripts.Controls.Add(this.panel1);
            this.tabScripts.Controls.Add(this.grpVerf�gbareSkripte);
            this.tabScripts.Location = new Point(4, 25);
            this.tabScripts.Name = "tabScripts";
            this.tabScripts.Padding = new Padding(3);
            this.tabScripts.Size = new Size(1289, 624);
            this.tabScripts.TabIndex = 7;
            this.tabScripts.Text = "Skripte";
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Dock = DockStyle.Fill;
            this.eventScriptEditor.Location = new Point(240, 152);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.ScriptText = "";
            this.eventScriptEditor.Size = new Size(1046, 469);
            this.eventScriptEditor.TabIndex = 6;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Changed += new EventHandler(this.ScriptEditor_Changed);
            this.eventScriptEditor.ExecuteScript += new EventHandler<ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.grpEigenschaften);
            this.panel1.Controls.Add(this.grpAusl�ser);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(240, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(1046, 149);
            this.panel1.TabIndex = 22;
            // 
            // grpEigenschaften
            // 
            this.grpEigenschaften.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpEigenschaften.Controls.Add(this.chkAendertWerte);
            this.grpEigenschaften.Controls.Add(this.btnSave);
            this.grpEigenschaften.Controls.Add(this.chkExternVerf�gbar);
            this.grpEigenschaften.Controls.Add(this.txbName);
            this.grpEigenschaften.Controls.Add(this.capName);
            this.grpEigenschaften.Dock = DockStyle.Top;
            this.grpEigenschaften.Location = new Point(0, 0);
            this.grpEigenschaften.Name = "grpEigenschaften";
            this.grpEigenschaften.Size = new Size(848, 77);
            this.grpEigenschaften.TabIndex = 4;
            this.grpEigenschaften.TabStop = false;
            this.grpEigenschaften.Text = "Eigenschaften";
            // 
            // chkAendertWerte
            // 
            this.chkAendertWerte.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAendertWerte.Location = new Point(288, 48);
            this.chkAendertWerte.Name = "chkAendertWerte";
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgef�hrt um dessen\r\nBerechnungen abzugreifen.\r\n�nderungen w" +
    "erden nicht in die Datenbank\r\nzur�ckgespielt";
            this.chkAendertWerte.Size = new Size(120, 16);
            this.chkAendertWerte.TabIndex = 16;
            this.chkAendertWerte.Text = "�ndert Werte";
            this.chkAendertWerte.CheckedChanged += new EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new Point(727, 48);
            this.btnSave.Name = "btnSave";
            this.btnSave.QuickInfo = "Datenbank und die �nderungen am Skript\r\nfest auf den Datentr�ger speichern.";
            this.btnSave.Size = new Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // chkExternVerf�gbar
            // 
            this.chkExternVerf�gbar.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkExternVerf�gbar.Location = new Point(160, 48);
            this.chkExternVerf�gbar.Name = "chkExternVerf�gbar";
            this.chkExternVerf�gbar.QuickInfo = "Wenn das Skript �ber eine Men�leiste oder dem Kontextmen�\r\nw�hlbar sein soll, mus" +
    "s dieses H�kchen gesetzt sein.";
            this.chkExternVerf�gbar.Size = new Size(120, 16);
            this.chkExternVerf�gbar.TabIndex = 15;
            this.chkExternVerf�gbar.Text = "Extern verf�gbar";
            this.chkExternVerf�gbar.CheckedChanged += new EventHandler(this.chkExternVerf�gbar_CheckedChanged);
            // 
            // txbName
            // 
            this.txbName.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.txbName.Cursor = Cursors.IBeam;
            this.txbName.Location = new Point(64, 24);
            this.txbName.Name = "txbName";
            this.txbName.Size = new Size(775, 22);
            this.txbName.TabIndex = 13;
            this.txbName.TextChanged += new EventHandler(this.txbName_TextChanged);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new Point(8, 24);
            this.capName.Name = "capName";
            this.capName.Size = new Size(56, 22);
            this.capName.Text = "Name:";
            // 
            // grpAusl�ser
            // 
            this.grpAusl�ser.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAusl�ser.Controls.Add(this.button1);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_export);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_formulaloaded);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_valuechangedThread);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_valuechanged);
            this.grpAusl�ser.Controls.Add(this.chkAusl�ser_prepaireformula);
            this.grpAusl�ser.Dock = DockStyle.Right;
            this.grpAusl�ser.Location = new Point(848, 0);
            this.grpAusl�ser.Name = "grpAusl�ser";
            this.grpAusl�ser.Size = new Size(198, 149);
            this.grpAusl�ser.TabIndex = 21;
            this.grpAusl�ser.TabStop = false;
            this.grpAusl�ser.Text = "Ausl�ser";
            // 
            // chkAusl�ser_valuechangedThread
            // 
            this.chkAusl�ser_valuechangedThread.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAusl�ser_valuechangedThread.Location = new Point(9, 64);
            this.chkAusl�ser_valuechangedThread.Name = "chkAusl�ser_valuechangedThread";
            this.chkAusl�ser_valuechangedThread.QuickInfo = null;
            this.chkAusl�ser_valuechangedThread.Size = new Size(176, 16);
            this.chkAusl�ser_valuechangedThread.TabIndex = 20;
            this.chkAusl�ser_valuechangedThread.Text = "Wert ge�ndert <b><fontsize=8><i>Extra Thread";
            this.chkAusl�ser_valuechangedThread.CheckedChanged += new EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_valuechanged
            // 
            this.chkAusl�ser_valuechanged.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAusl�ser_valuechanged.Location = new Point(9, 48);
            this.chkAusl�ser_valuechanged.Name = "chkAusl�ser_valuechanged";
            this.chkAusl�ser_valuechanged.QuickInfo = "Das Skript wird nach dem �ndern eines\r\nWertes einer Zelle ausgef�hrt";
            this.chkAusl�ser_valuechanged.Size = new Size(176, 16);
            this.chkAusl�ser_valuechanged.TabIndex = 18;
            this.chkAusl�ser_valuechanged.Text = "Wert ge�ndert";
            this.chkAusl�ser_valuechanged.CheckedChanged += new EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_prepaireformula
            // 
            this.chkAusl�ser_prepaireformula.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAusl�ser_prepaireformula.Location = new Point(8, 32);
            this.chkAusl�ser_prepaireformula.Name = "chkAusl�ser_prepaireformula";
            this.chkAusl�ser_prepaireformula.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzpr�fung\r\nund f�r Variablen f�r Formul" +
    "are.\r\n\r\nEs kann keine Daten �ndern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse ansto�en.";
            this.chkAusl�ser_prepaireformula.Size = new Size(175, 16);
            this.chkAusl�ser_prepaireformula.TabIndex = 19;
            this.chkAusl�ser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAusl�ser_prepaireformula.CheckedChanged += new EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // grpVerf�gbareSkripte
            // 
            this.grpVerf�gbareSkripte.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerf�gbareSkripte.CausesValidation = false;
            this.grpVerf�gbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerf�gbareSkripte.Dock = DockStyle.Left;
            this.grpVerf�gbareSkripte.Location = new Point(3, 3);
            this.grpVerf�gbareSkripte.Name = "grpVerf�gbareSkripte";
            this.grpVerf�gbareSkripte.Size = new Size(237, 618);
            this.grpVerf�gbareSkripte.TabIndex = 2;
            this.grpVerf�gbareSkripte.TabStop = false;
            this.grpVerf�gbareSkripte.Text = "Verf�gbare Skripte:";
            // 
            // lstEventScripts
            // 
            this.lstEventScripts.AddAllowed = AddType.UserDef;
            this.lstEventScripts.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                            | AnchorStyles.Left) 
                                                           | AnchorStyles.Right)));
            this.lstEventScripts.FilterAllowed = true;
            this.lstEventScripts.Location = new Point(8, 24);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new Size(222, 586);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.variableEditor);
            this.tabVariablen.Location = new Point(4, 25);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Padding = new Padding(3);
            this.tabVariablen.Size = new Size(1289, 624);
            this.tabVariablen.TabIndex = 3;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new Size(1283, 618);
            this.variableEditor.TabIndex = 0;
            // 
            // chkAusl�ser_formulaloaded
            // 
            this.chkAusl�ser_formulaloaded.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAusl�ser_formulaloaded.Location = new Point(8, 80);
            this.chkAusl�ser_formulaloaded.Name = "chkAusl�ser_formulaloaded";
            this.chkAusl�ser_formulaloaded.QuickInfo = "Das Skript wird direkt nach dem ersten Laden einer Datenbank angesto�en.\r\n\r\nEs ka" +
    "nn verwendet werden, um z.B. Backups zu erstellen.";
            this.chkAusl�ser_formulaloaded.Size = new Size(176, 16);
            this.chkAusl�ser_formulaloaded.TabIndex = 21;
            this.chkAusl�ser_formulaloaded.Text = "Formular geladen";
            this.chkAusl�ser_formulaloaded.CheckedChanged += new EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // chkAusl�ser_export
            // 
            this.chkAusl�ser_export.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAusl�ser_export.Location = new Point(8, 96);
            this.chkAusl�ser_export.Name = "chkAusl�ser_export";
            this.chkAusl�ser_export.QuickInfo = "Das Skript wird vor einem Export ausgef�hrt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte tempor�r zu �ndern,\r\nVariablen hinzuzuf�gen oder Bilder zu laden.";
            this.chkAusl�ser_export.Size = new Size(176, 16);
            this.chkAusl�ser_export.TabIndex = 22;
            this.chkAusl�ser_export.Text = "Export";
            this.chkAusl�ser_export.CheckedChanged += new EventHandler(this.chkAusl�ser_newrow_CheckedChanged);
            // 
            // button1
            // 
            this.button1.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.button1.Location = new Point(8, 16);
            this.button1.Name = "button1";
            this.button1.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzpr�fung\r\nund f�r Variablen f�r Formul" +
    "are.\r\n\r\nEs kann keine Daten �ndern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse ansto�en.";
            this.button1.Size = new Size(175, 16);
            this.button1.TabIndex = 23;
            this.button1.Text = "Clipbard Ver�nderung";
            // 
            // ConnectedFormulaScriptEditor
            // 
            this.ClientSize = new Size(1297, 677);
            this.Controls.Add(this.GlobalTab);
            this.MinimizeBox = false;
            this.Name = "ConnectedFormulaScriptEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.WindowState = FormWindowState.Maximized;
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
