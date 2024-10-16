﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using FastColoredTextBoxNS;
using GroupBox = BlueControls.Controls.GroupBox;
using SplitContainer = System.Windows.Forms.SplitContainer;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls {
    sealed partial class ScriptEditor {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;
        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ScriptEditor));
            this.splitContainer1 = new SplitContainer();
            this.grpCode = new GroupBox();
            this.txtSkript = new FastColoredTextBox();
            this.grpVariablen = new VariableEditor();
            this.grpAusgabeFenster = new GroupBox();
            this.txbSkriptInfo = new TextBox();
            ((ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpCode.SuspendLayout();
            ((ISupportInitialize)(this.txtSkript)).BeginInit();
            this.grpAusgabeFenster.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpCode);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.grpVariablen);
            this.splitContainer1.Panel2.Controls.Add(this.grpAusgabeFenster);
            this.splitContainer1.Size = new Size(603, 503);
            this.splitContainer1.SplitterDistance = 258;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 4;
            // 
            // grpCode
            // 
            this.grpCode.Controls.Add(this.txtSkript);
            this.grpCode.Dock = DockStyle.Fill;
            this.grpCode.GroupBoxStyle = GroupBoxStyle.NormalBold;
            this.grpCode.Location = new Point(0, 0);
            this.grpCode.Name = "grpCode";
            this.grpCode.Size = new Size(603, 258);
            this.grpCode.TabIndex = 3;
            this.grpCode.TabStop = false;
            this.grpCode.Text = "Code";
            // 
            // txtSkript
            // 
            this.txtSkript.AllowMacroRecording = false;
            this.txtSkript.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                      | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
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
            this.txtSkript.AutoScrollMinSize = new Size(27, 14);
            this.txtSkript.BackBrush = null;
            this.txtSkript.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            this.txtSkript.CharHeight = 14;
            this.txtSkript.CharWidth = 8;
            this.txtSkript.Cursor = Cursors.IBeam;
            this.txtSkript.DisabledColor = Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtSkript.Font = new Font("Courier New", 9.75F);
            this.txtSkript.IsReplaceMode = false;
            this.txtSkript.Language = Language.CSharp;
            this.txtSkript.LeftBracket = '(';
            this.txtSkript.LeftBracket2 = '{';
            this.txtSkript.Location = new Point(8, 32);
            this.txtSkript.Name = "txtSkript";
            this.txtSkript.Paddings = new Padding(0);
            this.txtSkript.RightBracket = ')';
            this.txtSkript.RightBracket2 = '}';
            this.txtSkript.SelectionColor = Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtSkript.ServiceColors = ((ServiceColors)(resources.GetObject("txtSkript.ServiceColors")));
            this.txtSkript.Size = new Size(587, 217);
            this.txtSkript.TabIndex = 2;
            this.txtSkript.Zoom = 100;
            this.txtSkript.ToolTipNeeded += new EventHandler<ToolTipNeededEventArgs>(this.txtSkript_ToolTipNeeded);
            this.txtSkript.TextChanged += new EventHandler<TextChangedEventArgs>(this.TxtSkript_TextChanged);
            this.txtSkript.MouseUp += new MouseEventHandler(this.TxtSkript_MouseUp);
            // 
            // grpVariablen
            // 
            this.grpVariablen.Dock = DockStyle.Fill;
            this.grpVariablen.Editabe = false;
            this.grpVariablen.GroupBoxStyle = GroupBoxStyle.NormalBold;
            this.grpVariablen.Location = new Point(0, 0);
            this.grpVariablen.Name = "grpVariablen";
            this.grpVariablen.Size = new Size(192, 237);
            this.grpVariablen.TabIndex = 3;
            this.grpVariablen.TabStop = false;
            this.grpVariablen.ToEdit = null;
            // 
            // grpAusgabeFenster
            // 
            this.grpAusgabeFenster.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpAusgabeFenster.Controls.Add(this.txbSkriptInfo);
            this.grpAusgabeFenster.Dock = DockStyle.Right;
            this.grpAusgabeFenster.GroupBoxStyle = GroupBoxStyle.NormalBold;
            this.grpAusgabeFenster.Location = new Point(192, 0);
            this.grpAusgabeFenster.Name = "grpAusgabeFenster";
            this.grpAusgabeFenster.Size = new Size(411, 237);
            this.grpAusgabeFenster.TabIndex = 3;
            this.grpAusgabeFenster.TabStop = false;
            this.grpAusgabeFenster.Text = "Ausgabe";
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                          | AnchorStyles.Left) 
                                                         | AnchorStyles.Right)));
            this.txbSkriptInfo.Cursor = Cursors.IBeam;
            this.txbSkriptInfo.Location = new Point(8, 24);
            this.txbSkriptInfo.Name = "txbSkriptInfo";
            this.txbSkriptInfo.Size = new Size(396, 208);
            this.txbSkriptInfo.TabIndex = 1;
            this.txbSkriptInfo.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // ScriptEditor
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "ScriptEditor";
            this.Size = new Size(603, 503);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpCode.ResumeLayout(false);
            ((ISupportInitialize)(this.txtSkript)).EndInit();
            this.grpAusgabeFenster.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion
        private FastColoredTextBox txtSkript;
        private GroupBox grpAusgabeFenster;
        private TextBox txbSkriptInfo;
        private SplitContainer splitContainer1;
        private GroupBox grpCode;
        private VariableEditor grpVariablen;
    }
}
