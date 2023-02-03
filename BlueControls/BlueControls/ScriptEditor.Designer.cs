
using System;
using System.Windows.Forms;

namespace BlueControls {
    partial class ScriptEditor {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditor));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpCode = new BlueControls.Controls.GroupBox();
            this.txtSkript = new FastColoredTextBoxNS.FastColoredTextBox();
            this.grpVariablen = new BlueControls.VariableEditor();
            this.grpAusgabeFenster = new BlueControls.Controls.GroupBox();
            this.txbSkriptInfo = new BlueControls.Controls.TextBox();
            this.grpMainBar = new BlueControls.Controls.GroupBox();
            this.btnZusatzDateien = new BlueControls.Controls.Button();
            this.btnBefehlsUebersicht = new BlueControls.Controls.Button();
            this.btnTest = new BlueControls.Controls.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpCode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).BeginInit();
            this.grpAusgabeFenster.SuspendLayout();
            this.grpMainBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 168);
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
            this.splitContainer1.Size = new System.Drawing.Size(603, 335);
            this.splitContainer1.SplitterDistance = 154;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 4;
            // 
            // grpCode
            // 
            this.grpCode.Controls.Add(this.txtSkript);
            this.grpCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCode.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.NormalBold;
            this.grpCode.Location = new System.Drawing.Point(0, 0);
            this.grpCode.Name = "grpCode";
            this.grpCode.Size = new System.Drawing.Size(603, 154);
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
            this.txtSkript.Size = new System.Drawing.Size(587, 113);
            this.txtSkript.TabIndex = 2;
            this.txtSkript.Zoom = 100;
            this.txtSkript.ToolTipNeeded += new System.EventHandler<FastColoredTextBoxNS.ToolTipNeededEventArgs>(this.txtSkript_ToolTipNeeded);
            this.txtSkript.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TxtSkript_TextChanged);
            this.txtSkript.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TxtSkript_MouseUp);
            // 
            // grpVariablen
            // 
            this.grpVariablen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVariablen.Location = new System.Drawing.Point(0, 0);
            this.grpVariablen.Name = "grpVariablen";
            this.grpVariablen.Size = new System.Drawing.Size(603, 173);
            this.grpVariablen.TabIndex = 3;
            this.grpVariablen.TabStop = false;
            // 
            // grpAusgabeFenster
            // 
            this.grpAusgabeFenster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAusgabeFenster.Controls.Add(this.txbSkriptInfo);
            this.grpAusgabeFenster.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpAusgabeFenster.Location = new System.Drawing.Point(0, 48);
            this.grpAusgabeFenster.Name = "grpAusgabeFenster";
            this.grpAusgabeFenster.Size = new System.Drawing.Size(603, 120);
            this.grpAusgabeFenster.TabIndex = 3;
            this.grpAusgabeFenster.TabStop = false;
            this.grpAusgabeFenster.Text = "Ausgabe";
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbSkriptInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSkriptInfo.Location = new System.Drawing.Point(8, 24);
            this.txbSkriptInfo.Name = "txbSkriptInfo";
            this.txbSkriptInfo.Size = new System.Drawing.Size(588, 91);
            this.txbSkriptInfo.TabIndex = 1;
            this.txbSkriptInfo.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpMainBar
            // 
            this.grpMainBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpMainBar.Controls.Add(this.btnZusatzDateien);
            this.grpMainBar.Controls.Add(this.btnBefehlsUebersicht);
            this.grpMainBar.Controls.Add(this.btnTest);
            this.grpMainBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpMainBar.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.grpMainBar.Location = new System.Drawing.Point(0, 0);
            this.grpMainBar.Name = "grpMainBar";
            this.grpMainBar.Size = new System.Drawing.Size(603, 48);
            this.grpMainBar.TabIndex = 5;
            this.grpMainBar.TabStop = false;
            // 
            // btnZusatzDateien
            // 
            this.btnZusatzDateien.ImageCode = "Ordner|16";
            this.btnZusatzDateien.Location = new System.Drawing.Point(272, 8);
            this.btnZusatzDateien.Name = "btnZusatzDateien";
            this.btnZusatzDateien.QuickInfo = "Den Ordner der Zusatzdatein öffnen.\r\nIn diesen können z.B. Skript-Routinen enthal" +
    "ten sein\r\ndie mit CallByFilename aufgerufen werden können.";
            this.btnZusatzDateien.Size = new System.Drawing.Size(120, 32);
            this.btnZusatzDateien.TabIndex = 2;
            this.btnZusatzDateien.Text = "Zusatzdateien";
            this.btnZusatzDateien.Click += new System.EventHandler(this.btnZusatzDateien_Click);
            // 
            // btnBefehlsUebersicht
            // 
            this.btnBefehlsUebersicht.ImageCode = "Tabelle|16";
            this.btnBefehlsUebersicht.Location = new System.Drawing.Point(128, 8);
            this.btnBefehlsUebersicht.Name = "btnBefehlsUebersicht";
            this.btnBefehlsUebersicht.Size = new System.Drawing.Size(136, 32);
            this.btnBefehlsUebersicht.TabIndex = 1;
            this.btnBefehlsUebersicht.Text = "Befehls-Übersicht";
            this.btnBefehlsUebersicht.Click += new System.EventHandler(this.btnBefehlsUebersicht_Click);
            // 
            // btnTest
            // 
            this.btnTest.ImageCode = "Abspielen|16";
            this.btnTest.Location = new System.Drawing.Point(16, 8);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(104, 32);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Testen";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // ScriptEditor
            // 
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.grpAusgabeFenster);
            this.Controls.Add(this.grpMainBar);
            this.Name = "ScriptEditor";
            this.Size = new System.Drawing.Size(603, 503);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpCode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).EndInit();
            this.grpAusgabeFenster.ResumeLayout(false);
            this.grpMainBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion
        private FastColoredTextBoxNS.FastColoredTextBox txtSkript;
        private Controls.GroupBox grpAusgabeFenster;
        private Controls.TextBox txbSkriptInfo;
        private SplitContainer splitContainer1;
        private Controls.GroupBox grpCode;
        private VariableEditor grpVariablen;
        protected Controls.GroupBox grpMainBar;
        protected Controls.Button btnBefehlsUebersicht;
        protected Controls.Button btnTest;
        protected Controls.Button btnZusatzDateien;
    }
}
