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
            this.GlobalTab = new TabControl();
            this.tabScripts = new TabPage();
            this.eventScriptEditor = new ScriptEditor();
            this.panel1 = new Panel();
            this.grpEigenschaften = new GroupBox();
            this.chkAendertWerte = new Button();
            this.btnSave = new Button();
            this.chkExternVerfügbar = new Button();
            this.txbName = new TextBox();
            this.capName = new Caption();
            this.grpAuslöser = new GroupBox();
            this.chkAuslöser_valuechangedThread = new Button();
            this.chkAuslöser_valuechanged = new Button();
            this.chkAuslöser_prepaireformula = new Button();
            this.grpVerfügbareSkripte = new GroupBox();
            this.lstEventScripts = new ListBox();
            this.tabVariablen = new TabPage();
            this.variableEditor = new VariableEditor();
            this.chkAuslöser_formulaloaded = new Button();
            this.chkAuslöser_export = new Button();
            this.button1 = new Button();
            this.pnlStatusBar.SuspendLayout();
            this.GlobalTab.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpEigenschaften.SuspendLayout();
            this.grpAuslöser.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
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
            this.tabScripts.Controls.Add(this.grpVerfügbareSkripte);
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
            this.panel1.Controls.Add(this.grpAuslöser);
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
            this.grpEigenschaften.Controls.Add(this.chkExternVerfügbar);
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
            this.chkAendertWerte.QuickInfo = "Das Skript wird nur ausgeführt um dessen\r\nBerechnungen abzugreifen.\r\nÄnderungen w" +
    "erden nicht in die Datenbank\r\nzurückgespielt";
            this.chkAendertWerte.Size = new Size(120, 16);
            this.chkAendertWerte.TabIndex = 16;
            this.chkAendertWerte.Text = "Ändert Werte";
            this.chkAendertWerte.CheckedChanged += new EventHandler(this.chkAendertWerte_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new Point(727, 48);
            this.btnSave.Name = "btnSave";
            this.btnSave.QuickInfo = "Datenbank und die änderungen am Skript\r\nfest auf den Datenträger speichern.";
            this.btnSave.Size = new Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // chkExternVerfügbar
            // 
            this.chkExternVerfügbar.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkExternVerfügbar.Location = new Point(160, 48);
            this.chkExternVerfügbar.Name = "chkExternVerfügbar";
            this.chkExternVerfügbar.QuickInfo = "Wenn das Skript über eine Menüleiste oder dem Kontextmenü\r\nwählbar sein soll, mus" +
    "s dieses Häkchen gesetzt sein.";
            this.chkExternVerfügbar.Size = new Size(120, 16);
            this.chkExternVerfügbar.TabIndex = 15;
            this.chkExternVerfügbar.Text = "Extern verfügbar";
            this.chkExternVerfügbar.CheckedChanged += new EventHandler(this.chkExternVerfügbar_CheckedChanged);
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
            // grpAuslöser
            // 
            this.grpAuslöser.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAuslöser.Controls.Add(this.button1);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_export);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_formulaloaded);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechangedThread);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_valuechanged);
            this.grpAuslöser.Controls.Add(this.chkAuslöser_prepaireformula);
            this.grpAuslöser.Dock = DockStyle.Right;
            this.grpAuslöser.Location = new Point(848, 0);
            this.grpAuslöser.Name = "grpAuslöser";
            this.grpAuslöser.Size = new Size(198, 149);
            this.grpAuslöser.TabIndex = 21;
            this.grpAuslöser.TabStop = false;
            this.grpAuslöser.Text = "Auslöser";
            // 
            // chkAuslöser_valuechangedThread
            // 
            this.chkAuslöser_valuechangedThread.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_valuechangedThread.Location = new Point(9, 64);
            this.chkAuslöser_valuechangedThread.Name = "chkAuslöser_valuechangedThread";
            this.chkAuslöser_valuechangedThread.QuickInfo = null;
            this.chkAuslöser_valuechangedThread.Size = new Size(176, 16);
            this.chkAuslöser_valuechangedThread.TabIndex = 20;
            this.chkAuslöser_valuechangedThread.Text = "Wert geändert <b><fontsize=8><i>Extra Thread";
            this.chkAuslöser_valuechangedThread.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_valuechanged
            // 
            this.chkAuslöser_valuechanged.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_valuechanged.Location = new Point(9, 48);
            this.chkAuslöser_valuechanged.Name = "chkAuslöser_valuechanged";
            this.chkAuslöser_valuechanged.QuickInfo = "Das Skript wird nach dem Ändern eines\r\nWertes einer Zelle ausgeführt";
            this.chkAuslöser_valuechanged.Size = new Size(176, 16);
            this.chkAuslöser_valuechanged.TabIndex = 18;
            this.chkAuslöser_valuechanged.Text = "Wert geändert";
            this.chkAuslöser_valuechanged.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_prepaireformula
            // 
            this.chkAuslöser_prepaireformula.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_prepaireformula.Location = new Point(8, 32);
            this.chkAuslöser_prepaireformula.Name = "chkAuslöser_prepaireformula";
            this.chkAuslöser_prepaireformula.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzprüfung\r\nund für Variablen für Formul" +
    "are.\r\n\r\nEs kann keine Daten ändern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse anstoßen.";
            this.chkAuslöser_prepaireformula.Size = new Size(175, 16);
            this.chkAuslöser_prepaireformula.TabIndex = 19;
            this.chkAuslöser_prepaireformula.Text = "Formular vorbereiten";
            this.chkAuslöser_prepaireformula.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new Point(3, 3);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new Size(237, 618);
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
            // chkAuslöser_formulaloaded
            // 
            this.chkAuslöser_formulaloaded.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_formulaloaded.Location = new Point(8, 80);
            this.chkAuslöser_formulaloaded.Name = "chkAuslöser_formulaloaded";
            this.chkAuslöser_formulaloaded.QuickInfo = "Das Skript wird direkt nach dem ersten Laden einer Datenbank angestoßen.\r\n\r\nEs ka" +
    "nn verwendet werden, um z.B. Backups zu erstellen.";
            this.chkAuslöser_formulaloaded.Size = new Size(176, 16);
            this.chkAuslöser_formulaloaded.TabIndex = 21;
            this.chkAuslöser_formulaloaded.Text = "Formular geladen";
            this.chkAuslöser_formulaloaded.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // chkAuslöser_export
            // 
            this.chkAuslöser_export.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.chkAuslöser_export.Location = new Point(8, 96);
            this.chkAuslöser_export.Name = "chkAuslöser_export";
            this.chkAuslöser_export.QuickInfo = "Das Skript wird vor einem Export ausgeführt.\r\n\r\nEs kann dazu verwendet werden, um" +
    " Werte temporär zu ändern,\r\nVariablen hinzuzufügen oder Bilder zu laden.";
            this.chkAuslöser_export.Size = new Size(176, 16);
            this.chkAuslöser_export.TabIndex = 22;
            this.chkAuslöser_export.Text = "Export";
            this.chkAuslöser_export.CheckedChanged += new EventHandler(this.chkAuslöser_newrow_CheckedChanged);
            // 
            // button1
            // 
            this.button1.ButtonStyle = ((ButtonStyle)((ButtonStyle.Checkbox | ButtonStyle.Text)));
            this.button1.Location = new Point(8, 16);
            this.button1.Name = "button1";
            this.button1.QuickInfo = "Das Skript wird verwendet zur Datenkonsitenzprüfung\r\nund für Variablen für Formul" +
    "are.\r\n\r\nEs kann keine Daten ändern, auf Festplatte zugreifen oder\r\nlange dauernd" +
    "e Prozesse anstoßen.";
            this.button1.Size = new Size(175, 16);
            this.button1.TabIndex = 23;
            this.button1.Text = "Clipbard Veränderung";
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
        private Button chkAendertWerte;
        private Button chkExternVerfügbar;
        private TextBox txbName;
        private Caption capName;
        private Button chkAuslöser_valuechangedThread;
        private Panel panel1;
        private GroupBox grpAuslöser;
        private Button chkAuslöser_export;
        private Button chkAuslöser_formulaloaded;
        private Button button1;
    }
}
