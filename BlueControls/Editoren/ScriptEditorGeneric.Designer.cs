// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Editoren;
using System.Diagnostics;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls.BlueTableDialogs {
    public partial class ScriptEditorGeneric {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            components = new Container();
            var resources = new ComponentResourceManager(typeof(ScriptEditorGeneric));
            btnSaveLoad = new Button();
            btnBefehlsUebersicht = new Button();
            btnAusführen = new Button();
            tbcScriptEigenschaften = new BlueControls.Controls.TabControl();
            tabScriptEditor = new TabPage();
            splitContainer1 = new SplitContainer();
            grpCode = new GroupBox();
            txtSkript = new FastColoredTextBoxNS.FastColoredTextBox();
            grpVariablen = new VariableEditor();
            tabError = new GroupBox();
            btnAnzeigen = new Button();
            btnLeeren = new Button();
            txbErrorInfo = new BlueControls.Controls.TextBox();
            tabAssistent = new TabPage();
            lstAssistant = new BlueControls.Controls.ListBox();
            tabStart = new GroupBox();
            grpInjectVariables = new GroupBox();
            btnVariables = new Button();
            pnlStatusBar.SuspendLayout();
            tbcScriptEigenschaften.SuspendLayout();
            tabScriptEditor.SuspendLayout();
            ((ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            grpCode.SuspendLayout();
            ((ISupportInitialize)txtSkript).BeginInit();
            tabError.SuspendLayout();
            tabAssistent.SuspendLayout();
            tabStart.SuspendLayout();
            grpInjectVariables.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(784, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(0, 537);
            pnlStatusBar.Size = new Size(784, 24);
            // 
            // btnSaveLoad
            // 
            btnSaveLoad.ImageCode = "Diskette|16";
            btnSaveLoad.Location = new Point(248, 8);
            btnSaveLoad.Name = "btnSaveLoad";
            btnSaveLoad.QuickInfo = "Aktualisiert die Tabellen-Daten. (Speichern, neu Laden)";
            btnSaveLoad.Size = new Size(112, 24);
            btnSaveLoad.TabIndex = 45;
            btnSaveLoad.Text = "Daten aktual.";
            btnSaveLoad.Click += btnSave_Click;
            // 
            // btnBefehlsUebersicht
            // 
            btnBefehlsUebersicht.ImageCode = "Bruchlinie";
            btnBefehlsUebersicht.Location = new Point(112, 8);
            btnBefehlsUebersicht.Name = "btnBefehlsUebersicht";
            btnBefehlsUebersicht.Size = new Size(128, 24);
            btnBefehlsUebersicht.TabIndex = 0;
            btnBefehlsUebersicht.Text = "Befehlsübersicht";
            btnBefehlsUebersicht.Click += btnBefehlsUebersicht_Click;
            // 
            // btnAusführen
            // 
            btnAusführen.ImageCode = "Abspielen";
            btnAusführen.Location = new Point(8, 8);
            btnAusführen.Name = "btnAusführen";
            btnAusführen.Size = new Size(96, 24);
            btnAusführen.TabIndex = 3;
            btnAusführen.Text = "Ausführen";
            btnAusführen.Click += btnAusführen_Click;
            // 
            // tbcScriptEigenschaften
            // 
            tbcScriptEigenschaften.Controls.Add(tabScriptEditor);
            tbcScriptEigenschaften.Controls.Add(tabAssistent);
            tbcScriptEigenschaften.Dock = DockStyle.Fill;
            tbcScriptEigenschaften.HotTrack = true;
            tbcScriptEigenschaften.Location = new Point(0, 80);
            tbcScriptEigenschaften.Name = "tbcScriptEigenschaften";
            tbcScriptEigenschaften.SelectedIndex = 0;
            tbcScriptEigenschaften.Size = new Size(784, 457);
            tbcScriptEigenschaften.TabDefault = null;
            tbcScriptEigenschaften.TabIndex = 98;
            tbcScriptEigenschaften.Selecting += tbcScriptEigenschaften_Selecting;
            // 
            // tabScriptEditor
            // 
            tabScriptEditor.BackColor = Color.FromArgb(255, 255, 255);
            tabScriptEditor.Controls.Add(splitContainer1);
            tabScriptEditor.Location = new Point(4, 25);
            tabScriptEditor.Name = "tabScriptEditor";
            tabScriptEditor.Padding = new Padding(3);
            tabScriptEditor.Size = new Size(776, 428);
            tabScriptEditor.TabIndex = 1;
            tabScriptEditor.Text = "Skript-Editor";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(grpCode);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(grpVariablen);
            splitContainer1.Panel2.Controls.Add(tabError);
            splitContainer1.Size = new Size(770, 422);
            splitContainer1.SplitterDistance = 214;
            splitContainer1.SplitterWidth = 8;
            splitContainer1.TabIndex = 5;
            // 
            // grpCode
            // 
            grpCode.Controls.Add(txtSkript);
            grpCode.Dock = DockStyle.Fill;
            grpCode.GroupBoxStyle = GroupBoxStyle.NormalBold;
            grpCode.Location = new Point(0, 0);
            grpCode.Name = "grpCode";
            grpCode.Size = new Size(770, 214);
            grpCode.TabIndex = 3;
            grpCode.TabStop = false;
            grpCode.Text = "Code";
            // 
            // txtSkript
            // 
            txtSkript.AllowMacroRecording = false;
            txtSkript.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSkript.AutoCompleteBracketsList = new char[]
    {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
    };
            txtSkript.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);\r\n";
            txtSkript.AutoScrollMinSize = new Size(27, 14);
            txtSkript.BackBrush = null;
            txtSkript.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            txtSkript.CharHeight = 14;
            txtSkript.CharWidth = 8;
            txtSkript.Cursor = Cursors.IBeam;
            txtSkript.DisabledColor = Color.FromArgb(100, 180, 180, 180);
            txtSkript.Hotkeys = resources.GetString("txtSkript.Hotkeys");
            txtSkript.IsReplaceMode = false;
            txtSkript.Language = FastColoredTextBoxNS.Language.CSharp;
            txtSkript.LeftBracket = '(';
            txtSkript.LeftBracket2 = '{';
            txtSkript.Location = new Point(8, 32);
            txtSkript.Name = "txtSkript";
            txtSkript.Paddings = new Padding(0);
            txtSkript.RightBracket = ')';
            txtSkript.RightBracket2 = '}';
            txtSkript.SelectionColor = Color.FromArgb(60, 0, 0, 255);
            txtSkript.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("txtSkript.ServiceColors");
            txtSkript.Size = new Size(754, 171);
            txtSkript.TabIndex = 2;
            txtSkript.Zoom = 100;
            txtSkript.ToolTipNeeded += txtSkript_ToolTipNeeded;
            txtSkript.TextChanged += TxtSkript_TextChanged;
            txtSkript.MouseUp += TxtSkript_MouseUp;
            // 
            // grpVariablen
            // 
            grpVariablen.Dock = DockStyle.Fill;
            grpVariablen.GroupBoxStyle = GroupBoxStyle.NormalBold;
            grpVariablen.Location = new Point(0, 0);
            grpVariablen.Name = "grpVariablen";
            grpVariablen.Size = new Size(360, 200);
            grpVariablen.TabIndex = 3;
            grpVariablen.TabStop = false;
            // 
            // tabError
            // 
            tabError.Controls.Add(btnAnzeigen);
            tabError.Controls.Add(btnLeeren);
            tabError.Controls.Add(txbErrorInfo);
            tabError.Dock = DockStyle.Right;
            tabError.GroupBoxStyle = GroupBoxStyle.NormalBold;
            tabError.Location = new Point(360, 0);
            tabError.Name = "tabError";
            tabError.Size = new Size(410, 200);
            tabError.TabIndex = 4;
            tabError.TabStop = false;
            tabError.Text = "Fehler";
            // 
            // btnAnzeigen
            // 
            btnAnzeigen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAnzeigen.ImageCode = "Blitz|16";
            btnAnzeigen.Location = new Point(8, 167);
            btnAnzeigen.Name = "btnAnzeigen";
            btnAnzeigen.Size = new Size(80, 24);
            btnAnzeigen.TabIndex = 4;
            btnAnzeigen.Text = "anzeigen";
            btnAnzeigen.Click += btnAnzeigen_Click;
            // 
            // btnLeeren
            // 
            btnLeeren.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLeeren.ImageCode = "Papierkorb|16";
            btnLeeren.Location = new Point(320, 167);
            btnLeeren.Name = "btnLeeren";
            btnLeeren.Size = new Size(80, 24);
            btnLeeren.TabIndex = 3;
            btnLeeren.Text = "leeren";
            btnLeeren.Click += btnLeeren_Click;
            // 
            // txbErrorInfo
            // 
            txbErrorInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbErrorInfo.Cursor = Cursors.IBeam;
            txbErrorInfo.Location = new Point(8, 24);
            txbErrorInfo.Name = "txbErrorInfo";
            txbErrorInfo.Size = new Size(392, 135);
            txbErrorInfo.TabIndex = 2;
            txbErrorInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tabAssistent
            // 
            tabAssistent.BackColor = Color.FromArgb(255, 255, 255);
            tabAssistent.Controls.Add(lstAssistant);
            tabAssistent.Location = new Point(4, 25);
            tabAssistent.Name = "tabAssistent";
            tabAssistent.Padding = new Padding(3);
            tabAssistent.Size = new Size(776, 428);
            tabAssistent.TabIndex = 2;
            tabAssistent.Text = "Befehls-Assistent";
            // 
            // lstAssistant
            // 
            lstAssistant.AddAllowed = AddType.None;
            lstAssistant.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstAssistant.CheckBehavior = CheckBehavior.NoSelection;
            lstAssistant.Location = new Point(8, 8);
            lstAssistant.Name = "lstAssistant";
            lstAssistant.Size = new Size(416, 526);
            lstAssistant.TabIndex = 0;
            lstAssistant.ItemClicked += lstAssistant_ItemClicked;
            // 
            // tabStart
            // 
            tabStart.BackColor = Color.FromArgb(255, 255, 255);
            tabStart.Controls.Add(btnAusführen);
            tabStart.Controls.Add(btnBefehlsUebersicht);
            tabStart.Controls.Add(btnSaveLoad);
            tabStart.Dock = DockStyle.Top;
            tabStart.GroupBoxStyle = GroupBoxStyle.RoundRect;
            tabStart.Location = new Point(0, 0);
            tabStart.Name = "tabStart";
            tabStart.Size = new Size(784, 40);
            tabStart.TabIndex = 99;
            tabStart.TabStop = false;
            // 
            // grpInjectVariables
            // 
            grpInjectVariables.BackColor = Color.FromArgb(255, 255, 255);
            grpInjectVariables.Controls.Add(btnVariables);
            grpInjectVariables.Dock = DockStyle.Top;
            grpInjectVariables.GroupBoxStyle = GroupBoxStyle.RoundRect;
            grpInjectVariables.Location = new Point(0, 40);
            grpInjectVariables.Name = "grpInjectVariables";
            grpInjectVariables.Size = new Size(784, 40);
            grpInjectVariables.TabIndex = 100;
            grpInjectVariables.TabStop = false;
            // 
            // btnVariables
            // 
            btnVariables.ImageCode = "Stern|18";
            btnVariables.Location = new Point(8, 8);
            btnVariables.Name = "btnVariables";
            btnVariables.QuickInfo = "Gespeicherte Variablen";
            btnVariables.Size = new Size(24, 24);
            btnVariables.TabIndex = 56;
            btnVariables.Click += btnVariables_Click;
            // 
            // ScriptEditorGeneric
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(784, 561);
            Controls.Add(tbcScriptEigenschaften);
            Controls.Add(grpInjectVariables);
            Controls.Add(tabStart);
            GlobalMenuHeight = 0;
            MinimizeBox = false;
            Name = "ScriptEditorGeneric";
            Text = "Skript Editor";
            WindowState = FormWindowState.Maximized;
            Controls.SetChildIndex(tabStart, 0);
            Controls.SetChildIndex(grpInjectVariables, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(tbcScriptEigenschaften, 0);
            pnlStatusBar.ResumeLayout(false);
            tbcScriptEigenschaften.ResumeLayout(false);
            tabScriptEditor.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            grpCode.ResumeLayout(false);
            ((ISupportInitialize)txtSkript).EndInit();
            tabError.ResumeLayout(false);
            tabAssistent.ResumeLayout(false);
            tabStart.ResumeLayout(false);
            grpInjectVariables.ResumeLayout(false);
            ResumeLayout(false);

        }
        protected Button btnAusführen;
        protected Controls.TabControl tbcScriptEigenschaften;
        private TabPage tabScriptEditor;
        private SplitContainer splitContainer1;
        private GroupBox grpCode;
        private FastColoredTextBoxNS.FastColoredTextBox txtSkript;
        private VariableEditor grpVariablen;
        private System.ComponentModel.IContainer components;
        private TabPage tabAssistent;
        private Controls.ListBox lstAssistant;
        private GroupBox tabError;
        private Button btnLeeren;
        private Controls.TextBox txbErrorInfo;
        private Button btnAnzeigen;
        protected GroupBox tabStart;
        protected GroupBox grpInjectVariables;
        protected Button btnBefehlsUebersicht;
        protected Button btnSaveLoad;
        private Button btnVariables;
    }
}
