using System.Diagnostics;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BlueControls.BlueDatabaseDialogs {
    public partial class ScriptEditorGeneric {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditorGeneric));
            this.ribMain = new BlueControls.Controls.RibbonBar();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.grpAktionen = new BlueControls.Controls.GroupBox();
            this.btnSaveLoad = new BlueControls.Controls.Button();
            this.grpInfos = new BlueControls.Controls.GroupBox();
            this.btnBefehlsUebersicht = new BlueControls.Controls.Button();
            this.grpAusführen = new BlueControls.Controls.GroupBox();
            this.btnAusführen = new BlueControls.Controls.Button();
            this.tbcScriptEigenschaften = new BlueControls.Controls.TabControl();
            this.tabScriptEditor = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpCode = new BlueControls.Controls.GroupBox();
            this.txtSkript = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tabError = new BlueControls.Controls.TabControl();
            this.tabCurrent = new System.Windows.Forms.TabPage();
            this.txbSkriptInfo = new BlueControls.Controls.TextBox();
            this.tabLastError = new System.Windows.Forms.TabPage();
            this.btnLeeren = new BlueControls.Controls.Button();
            this.txbLastError = new BlueControls.Controls.TextBox();
            this.grpVariablen = new BlueControls.VariableEditor();
            this.tabAssistent = new System.Windows.Forms.TabPage();
            this.lstAssistant = new BlueControls.Controls.ListBox();
            this.pnlStatusBar.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.tbcScriptEigenschaften.SuspendLayout();
            this.tabScriptEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpCode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).BeginInit();
            this.tabError.SuspendLayout();
            this.tabCurrent.SuspendLayout();
            this.tabLastError.SuspendLayout();
            this.tabAssistent.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(784, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 537);
            this.pnlStatusBar.Size = new System.Drawing.Size(784, 24);
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabStart);
            this.ribMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribMain.HotTrack = true;
            this.ribMain.Location = new System.Drawing.Point(0, 0);
            this.ribMain.Name = "ribMain";
            this.ribMain.SelectedIndex = 0;
            this.ribMain.Size = new System.Drawing.Size(784, 110);
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
            this.tabStart.Size = new System.Drawing.Size(776, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpAktionen
            // 
            this.grpAktionen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAktionen.Controls.Add(this.btnSaveLoad);
            this.grpAktionen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAktionen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAktionen.Location = new System.Drawing.Point(152, 3);
            this.grpAktionen.Name = "grpAktionen";
            this.grpAktionen.Size = new System.Drawing.Size(64, 75);
            this.grpAktionen.TabIndex = 2;
            this.grpAktionen.TabStop = false;
            this.grpAktionen.Text = "Aktionen";
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnSaveLoad.ImageCode = "Diskette|16";
            this.btnSaveLoad.Location = new System.Drawing.Point(8, 2);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.QuickInfo = "Aktualisiert die Tabellen-Daten. (Speichern, neu Laden)";
            this.btnSaveLoad.Size = new System.Drawing.Size(48, 66);
            this.btnSaveLoad.TabIndex = 45;
            this.btnSaveLoad.Text = "Daten aktual.";
            this.btnSaveLoad.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // grpInfos
            // 
            this.grpInfos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpInfos.Controls.Add(this.btnBefehlsUebersicht);
            this.grpInfos.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpInfos.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpInfos.Location = new System.Drawing.Point(72, 3);
            this.grpInfos.Name = "grpInfos";
            this.grpInfos.Size = new System.Drawing.Size(80, 75);
            this.grpInfos.TabIndex = 1;
            this.grpInfos.TabStop = false;
            this.grpInfos.Text = "Infos";
            // 
            // btnBefehlsUebersicht
            // 
            this.btnBefehlsUebersicht.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnBefehlsUebersicht.ImageCode = "Bruchlinie";
            this.btnBefehlsUebersicht.Location = new System.Drawing.Point(8, 2);
            this.btnBefehlsUebersicht.Name = "btnBefehlsUebersicht";
            this.btnBefehlsUebersicht.Size = new System.Drawing.Size(64, 66);
            this.btnBefehlsUebersicht.TabIndex = 0;
            this.btnBefehlsUebersicht.Text = "Befehls-übersicht";
            this.btnBefehlsUebersicht.Click += new System.EventHandler(this.btnBefehlsUebersicht_Click);
            // 
            // grpAusführen
            // 
            this.grpAusführen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAusführen.Controls.Add(this.btnAusführen);
            this.grpAusführen.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAusführen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAusführen.Location = new System.Drawing.Point(3, 3);
            this.grpAusführen.Name = "grpAusführen";
            this.grpAusführen.Size = new System.Drawing.Size(69, 75);
            this.grpAusführen.TabIndex = 0;
            this.grpAusführen.TabStop = false;
            this.grpAusführen.Text = "Ausführen";
            // 
            // btnAusführen
            // 
            this.btnAusführen.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big_Borderless;
            this.btnAusführen.ImageCode = "Abspielen";
            this.btnAusführen.Location = new System.Drawing.Point(0, 2);
            this.btnAusführen.Name = "btnAusführen";
            this.btnAusführen.Size = new System.Drawing.Size(60, 66);
            this.btnAusführen.TabIndex = 3;
            this.btnAusführen.Text = "Aus-führen";
            this.btnAusführen.Click += new System.EventHandler(this.btnAusführen_Click);
            // 
            // tbcScriptEigenschaften
            // 
            this.tbcScriptEigenschaften.Controls.Add(this.tabScriptEditor);
            this.tbcScriptEigenschaften.Controls.Add(this.tabAssistent);
            this.tbcScriptEigenschaften.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbcScriptEigenschaften.HotTrack = true;
            this.tbcScriptEigenschaften.Location = new System.Drawing.Point(0, 110);
            this.tbcScriptEigenschaften.Name = "tbcScriptEigenschaften";
            this.tbcScriptEigenschaften.SelectedIndex = 0;
            this.tbcScriptEigenschaften.Size = new System.Drawing.Size(784, 427);
            this.tbcScriptEigenschaften.TabDefault = null;
            this.tbcScriptEigenschaften.TabDefaultOrder = new string[0];
            this.tbcScriptEigenschaften.TabIndex = 98;
            this.tbcScriptEigenschaften.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tbcScriptEigenschaften_Selecting);
            // 
            // tabScriptEditor
            // 
            this.tabScriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScriptEditor.Controls.Add(this.splitContainer1);
            this.tabScriptEditor.Location = new System.Drawing.Point(4, 25);
            this.tabScriptEditor.Name = "tabScriptEditor";
            this.tabScriptEditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabScriptEditor.Size = new System.Drawing.Size(776, 398);
            this.tabScriptEditor.TabIndex = 1;
            this.tabScriptEditor.Text = "Skript-Editor";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpCode);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.grpVariablen);
            this.splitContainer1.Panel2.Controls.Add(this.tabError);
            this.splitContainer1.Size = new System.Drawing.Size(770, 392);
            this.splitContainer1.SplitterDistance = 199;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 5;
            // 
            // grpCode
            // 
            this.grpCode.Controls.Add(this.txtSkript);
            this.grpCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCode.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.NormalBold;
            this.grpCode.Location = new System.Drawing.Point(0, 0);
            this.grpCode.Name = "grpCode";
            this.grpCode.Size = new System.Drawing.Size(770, 199);
            this.grpCode.TabIndex = 3;
            this.grpCode.TabStop = false;
            this.grpCode.Text = "Code";
            // 
            // txtSkript
            // 
            this.txtSkript.AllowMacroRecording = false;
            this.txtSkript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSkript.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.txtSkript.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);\r\n^\\s*(case|default)\\s*[^:]" +
    "*(?<range>:)\\s*(?<range>[^;]+);\r\n";
            this.txtSkript.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.txtSkript.BackBrush = null;
            this.txtSkript.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.txtSkript.CharHeight = 14;
            this.txtSkript.CharWidth = 8;
            this.txtSkript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSkript.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtSkript.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.txtSkript.IsReplaceMode = false;
            this.txtSkript.Language = FastColoredTextBoxNS.Language.CSharp;
            this.txtSkript.LeftBracket = '(';
            this.txtSkript.LeftBracket2 = '{';
            this.txtSkript.Location = new System.Drawing.Point(8, 32);
            this.txtSkript.Name = "txtSkript";
            this.txtSkript.Paddings = new System.Windows.Forms.Padding(0);
            this.txtSkript.RightBracket = ')';
            this.txtSkript.RightBracket2 = '}';
            this.txtSkript.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtSkript.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtSkript.ServiceColors")));
            this.txtSkript.Size = new System.Drawing.Size(754, 156);
            this.txtSkript.TabIndex = 2;
            this.txtSkript.Zoom = 100;
            this.txtSkript.ToolTipNeeded += new System.EventHandler<FastColoredTextBoxNS.ToolTipNeededEventArgs>(this.txtSkript_ToolTipNeeded);
            this.txtSkript.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TxtSkript_MouseUp);
            // 
            // tabError
            // 
            this.tabError.Controls.Add(this.tabCurrent);
            this.tabError.Controls.Add(this.tabLastError);
            this.tabError.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabError.HotTrack = true;
            this.tabError.Location = new System.Drawing.Point(360, 0);
            this.tabError.Name = "tabError";
            this.tabError.SelectedIndex = 0;
            this.tabError.Size = new System.Drawing.Size(410, 185);
            this.tabError.TabDefault = null;
            this.tabError.TabDefaultOrder = new string[0];
            this.tabError.TabIndex = 4;
            // 
            // tabCurrent
            // 
            this.tabCurrent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabCurrent.Controls.Add(this.txbSkriptInfo);
            this.tabCurrent.Location = new System.Drawing.Point(4, 25);
            this.tabCurrent.Name = "tabCurrent";
            this.tabCurrent.Padding = new System.Windows.Forms.Padding(3);
            this.tabCurrent.Size = new System.Drawing.Size(402, 156);
            this.tabCurrent.TabIndex = 0;
            this.tabCurrent.Text = "Ausgabe";
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSkriptInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbSkriptInfo.Location = new System.Drawing.Point(3, 3);
            this.txbSkriptInfo.Name = "txbSkriptInfo";
            this.txbSkriptInfo.Size = new System.Drawing.Size(396, 150);
            this.txbSkriptInfo.TabIndex = 1;
            this.txbSkriptInfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // tabLastError
            // 
            this.tabLastError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabLastError.Controls.Add(this.btnLeeren);
            this.tabLastError.Controls.Add(this.txbLastError);
            this.tabLastError.Location = new System.Drawing.Point(4, 25);
            this.tabLastError.Name = "tabLastError";
            this.tabLastError.Padding = new System.Windows.Forms.Padding(3);
            this.tabLastError.Size = new System.Drawing.Size(258, 156);
            this.tabLastError.TabIndex = 1;
            this.tabLastError.Text = "Letzer Fehler";
            // 
            // btnLeeren
            // 
            this.btnLeeren.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLeeren.ImageCode = "Papierkorb|16";
            this.btnLeeren.Location = new System.Drawing.Point(168, 112);
            this.btnLeeren.Name = "btnLeeren";
            this.btnLeeren.Size = new System.Drawing.Size(88, 40);
            this.btnLeeren.TabIndex = 3;
            this.btnLeeren.Text = "leeren";
            this.btnLeeren.Click += new System.EventHandler(this.btnLeeren_Click);
            // 
            // txbLastError
            // 
            this.txbLastError.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbLastError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbLastError.Location = new System.Drawing.Point(3, 3);
            this.txbLastError.Name = "txbLastError";
            this.txbLastError.Size = new System.Drawing.Size(252, 150);
            this.txbLastError.TabIndex = 2;
            this.txbLastError.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpVariablen
            // 
            this.grpVariablen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVariablen.Editabe = false;
            this.grpVariablen.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.NormalBold;
            this.grpVariablen.Location = new System.Drawing.Point(0, 0);
            this.grpVariablen.Name = "grpVariablen";
            this.grpVariablen.Size = new System.Drawing.Size(360, 185);
            this.grpVariablen.TabIndex = 3;
            this.grpVariablen.TabStop = false;
            this.grpVariablen.ToEdit = null;
            // 
            // tabAssistent
            // 
            this.tabAssistent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabAssistent.Controls.Add(this.lstAssistant);
            this.tabAssistent.Location = new System.Drawing.Point(4, 25);
            this.tabAssistent.Name = "tabAssistent";
            this.tabAssistent.Padding = new System.Windows.Forms.Padding(3);
            this.tabAssistent.Size = new System.Drawing.Size(776, 398);
            this.tabAssistent.TabIndex = 2;
            this.tabAssistent.Text = "Befehls-Assistent";
            // 
            // lstAssistant
            // 
            this.lstAssistant.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstAssistant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstAssistant.CheckBehavior = BlueControls.Enums.CheckBehavior.NoSelection;
            this.lstAssistant.Location = new System.Drawing.Point(8, 8);
            this.lstAssistant.Name = "lstAssistant";
            this.lstAssistant.Size = new System.Drawing.Size(416, 496);
            this.lstAssistant.TabIndex = 0;
            this.lstAssistant.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstAssistant_ItemClicked);
            // 
            // ScriptEditorGeneric
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tbcScriptEigenschaften);
            this.Controls.Add(this.ribMain);
            this.MinimizeBox = false;
            this.Name = "ScriptEditorGeneric";
            this.Text = "Skript Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.tbcScriptEigenschaften, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.tbcScriptEigenschaften.ResumeLayout(false);
            this.tabScriptEditor.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpCode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).EndInit();
            this.tabError.ResumeLayout(false);
            this.tabCurrent.ResumeLayout(false);
            this.tabLastError.ResumeLayout(false);
            this.tabAssistent.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private TabPage tabStart;
        protected GroupBox grpInfos;
        private Button btnBefehlsUebersicht;
        protected GroupBox grpAusführen;
        protected GroupBox grpAktionen;
        private Button btnSaveLoad;
        protected Button btnAusführen;
        protected Controls.TabControl tbcScriptEigenschaften;
        private TabPage tabScriptEditor;
        private SplitContainer splitContainer1;
        private GroupBox grpCode;
        private FastColoredTextBoxNS.FastColoredTextBox txtSkript;
        private VariableEditor grpVariablen;
        private Controls.TextBox txbSkriptInfo;
        private System.ComponentModel.IContainer components;
        private TabPage tabAssistent;
        private Controls.ListBox lstAssistant;
        protected RibbonBar ribMain;
        private Controls.TabControl tabError;
        private TabPage tabCurrent;
        private TabPage tabLastError;
        private Button btnLeeren;
        private Controls.TextBox txbLastError;
    }
}
