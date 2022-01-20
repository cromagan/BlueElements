
using System;
using System.Windows.Forms;

namespace BlueControls {
    partial class ScriptEditor {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditor));
            this.txtSkript = new FastColoredTextBoxNS.FastColoredTextBox();
            this.txbComms = new BlueControls.Controls.TextBox();
            this.lstComands = new BlueControls.Controls.ListBox();
            this.tableVariablen = new BlueControls.Controls.Table();
            this.filterVariablen = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.grpTextAllgemein = new BlueControls.Controls.GroupBox();
            this.txbSkriptInfo = new BlueControls.Controls.TextBox();
            this.btnTest = new BlueControls.Controls.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).BeginInit();
            this.grpTextAllgemein.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSkript
            // 
            this.txtSkript.AllowMacroRecording = false;
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
            this.txtSkript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSkript.IsReplaceMode = false;
            this.txtSkript.Language = FastColoredTextBoxNS.Language.CSharp;
            this.txtSkript.LeftBracket = '(';
            this.txtSkript.LeftBracket2 = '{';
            this.txtSkript.Location = new System.Drawing.Point(0, 0);
            this.txtSkript.Name = "txtSkript";
            this.txtSkript.Paddings = new System.Windows.Forms.Padding(0);
            this.txtSkript.RightBracket = ')';
            this.txtSkript.RightBracket2 = '}';
            this.txtSkript.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtSkript.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtSkript.ServiceColors")));
            this.txtSkript.Size = new System.Drawing.Size(328, 201);
            this.txtSkript.TabIndex = 2;
            this.txtSkript.Zoom = 100;
            this.txtSkript.ToolTipNeeded += new System.EventHandler<FastColoredTextBoxNS.ToolTipNeededEventArgs>(this.txtSkript_ToolTipNeeded);
            this.txtSkript.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TxtSkript_MouseUp);
            // 
            // txbComms
            // 
            this.txbComms.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbComms.Location = new System.Drawing.Point(28, 97);
            this.txbComms.Name = "txbComms";
            this.txbComms.Size = new System.Drawing.Size(101, 138);
            this.txbComms.TabIndex = 2;
            this.txbComms.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // lstComands
            // 
            this.lstComands.AddAllowed = BlueControls.Enums.enAddType.None;
            this.lstComands.FilterAllowed = true;
            this.lstComands.Location = new System.Drawing.Point(-36, -7);
            this.lstComands.Name = "lstComands";
            this.lstComands.Size = new System.Drawing.Size(136, 104);
            this.lstComands.TabIndex = 3;
            this.lstComands.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.lstComands_ItemClicked);
            // 
            // tableVariablen
            // 
            this.tableVariablen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableVariablen.Location = new System.Drawing.Point(0, 40);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.ShowWaitScreen = true;
            this.tableVariablen.Size = new System.Drawing.Size(603, 157);
            this.tableVariablen.TabIndex = 2;
            this.tableVariablen.Text = "tabVariablen";
            // 
            // filterVariablen
            // 
            this.filterVariablen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.filterVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.filterVariablen.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterVariablen.Location = new System.Drawing.Point(0, 0);
            this.filterVariablen.Name = "filterVariablen";
            this.filterVariablen.Size = new System.Drawing.Size(603, 40);
            this.filterVariablen.TabIndex = 1;
            this.filterVariablen.TabStop = false;
            // 
            // grpTextAllgemein
            // 
            this.grpTextAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpTextAllgemein.Controls.Add(this.txbSkriptInfo);
            this.grpTextAllgemein.Controls.Add(this.btnTest);
            this.grpTextAllgemein.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTextAllgemein.Location = new System.Drawing.Point(0, 0);
            this.grpTextAllgemein.Name = "grpTextAllgemein";
            this.grpTextAllgemein.Size = new System.Drawing.Size(603, 101);
            this.grpTextAllgemein.TabIndex = 3;
            this.grpTextAllgemein.TabStop = false;
            this.grpTextAllgemein.Text = "Allgemein";
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbSkriptInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSkriptInfo.Location = new System.Drawing.Point(160, 16);
            this.txbSkriptInfo.Name = "txbSkriptInfo";
            this.txbSkriptInfo.Size = new System.Drawing.Size(434, 80);
            this.txbSkriptInfo.TabIndex = 1;
            this.txbSkriptInfo.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(8, 16);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(136, 24);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Testen";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 101);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableVariablen);
            this.splitContainer1.Panel2.Controls.Add(this.filterVariablen);
            this.splitContainer1.Size = new System.Drawing.Size(603, 402);
            this.splitContainer1.SplitterDistance = 201;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.txtSkript);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txbComms);
            this.splitContainer2.Panel2.Controls.Add(this.lstComands);
            this.splitContainer2.Size = new System.Drawing.Size(603, 201);
            this.splitContainer2.SplitterDistance = 328;
            this.splitContainer2.TabIndex = 0;
            // 
            // ScriptEditor
            // 
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.grpTextAllgemein);
            this.Name = "ScriptEditor";
            this.Size = new System.Drawing.Size(603, 503);
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).EndInit();
            this.grpTextAllgemein.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion
        private FastColoredTextBoxNS.FastColoredTextBox txtSkript;
        private Controls.TextBox txbComms;
        private Controls.ListBox lstComands;
        private Controls.Table tableVariablen;
        private BlueDatabaseDialogs.Filterleiste filterVariablen;
        private Controls.GroupBox grpTextAllgemein;
        private Controls.TextBox txbSkriptInfo;
        private Controls.Button btnTest;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
    }
}
