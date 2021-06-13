
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
            this.tabCSckript = new BlueControls.Controls.TabControl();
            this.tabScriptAnzeige = new BlueControls.Controls.TabPage();
            this.txtSkript = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tabBefehle = new BlueControls.Controls.TabPage();
            this.txbComms = new BlueControls.Controls.TextBox();
            this.lstComands = new BlueControls.Controls.ListBox();
            this.tabVariablen = new BlueControls.Controls.TabPage();
            this.tableVariablen = new BlueControls.Controls.Table();
            this.filterVariablen = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.grpTextAllgemein = new BlueControls.Controls.GroupBox();
            this.txbTestZeile = new BlueControls.Controls.TextBox();
            this.capTestZeile = new BlueControls.Controls.Caption();
            this.txbSkriptInfo = new BlueControls.Controls.TextBox();
            this.btnTest = new BlueControls.Controls.Button();
            this.tabCSckript.SuspendLayout();
            this.tabScriptAnzeige.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).BeginInit();
            this.tabBefehle.SuspendLayout();
            this.tabVariablen.SuspendLayout();
            this.grpTextAllgemein.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCSckript
            // 
            this.tabCSckript.Controls.Add(this.tabScriptAnzeige);
            this.tabCSckript.Controls.Add(this.tabBefehle);
            this.tabCSckript.Controls.Add(this.tabVariablen);
            this.tabCSckript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCSckript.HotTrack = true;
            this.tabCSckript.Location = new System.Drawing.Point(3, 117);
            this.tabCSckript.Name = "tabCSckript";
            this.tabCSckript.SelectedIndex = 0;
            this.tabCSckript.Size = new System.Drawing.Size(597, 383);
            this.tabCSckript.TabIndex = 2;
            // 
            // tabScriptAnzeige
            // 
            this.tabScriptAnzeige.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScriptAnzeige.Controls.Add(this.txtSkript);
            this.tabScriptAnzeige.Location = new System.Drawing.Point(4, 25);
            this.tabScriptAnzeige.Name = "tabScriptAnzeige";
            this.tabScriptAnzeige.Size = new System.Drawing.Size(589, 354);
            this.tabScriptAnzeige.TabIndex = 0;
            this.tabScriptAnzeige.Text = "Skript-Text";
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
            this.txtSkript.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            this.txtSkript.BackBrush = null;
            this.txtSkript.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.txtSkript.CharHeight = 14;
            this.txtSkript.CharWidth = 8;
            this.txtSkript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSkript.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtSkript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSkript.Font = new System.Drawing.Font("Courier New", 9.75F);
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
            this.txtSkript.Size = new System.Drawing.Size(589, 354);
            this.txtSkript.TabIndex = 2;
            this.txtSkript.Zoom = 100;
            this.txtSkript.ToolTipNeeded += new System.EventHandler<FastColoredTextBoxNS.ToolTipNeededEventArgs>(this.txtSkript_ToolTipNeeded);
            // 
            // tabBefehle
            // 
            this.tabBefehle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabBefehle.Controls.Add(this.txbComms);
            this.tabBefehle.Controls.Add(this.lstComands);
            this.tabBefehle.Location = new System.Drawing.Point(4, 25);
            this.tabBefehle.Name = "tabBefehle";
            this.tabBefehle.Size = new System.Drawing.Size(589, 354);
            this.tabBefehle.TabIndex = 2;
            this.tabBefehle.Text = "Befehle";
            // 
            // txbComms
            // 
            this.txbComms.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbComms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txbComms.Location = new System.Drawing.Point(488, 0);
            this.txbComms.Name = "txbComms";
            this.txbComms.Size = new System.Drawing.Size(101, 354);
            this.txbComms.TabIndex = 2;
            this.txbComms.Verhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // lstComands
            // 
            this.lstComands.AddAllowed = BlueControls.Enums.enAddType.None;
            this.lstComands.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstComands.FilterAllowed = true;
            this.lstComands.Location = new System.Drawing.Point(0, 0);
            this.lstComands.Name = "lstComands";
            this.lstComands.Size = new System.Drawing.Size(488, 354);
            this.lstComands.TabIndex = 3;
            this.lstComands.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.lstComands_ItemClicked);
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.tableVariablen);
            this.tabVariablen.Controls.Add(this.filterVariablen);
            this.tabVariablen.Location = new System.Drawing.Point(4, 25);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Size = new System.Drawing.Size(589, 354);
            this.tabVariablen.TabIndex = 1;
            this.tabVariablen.Text = "Variablen";
            // 
            // tableVariablen
            // 
            this.tableVariablen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableVariablen.Location = new System.Drawing.Point(0, 40);
            this.tableVariablen.Name = "tableVariablen";
            this.tableVariablen.ShowWaitScreen = true;
            this.tableVariablen.Size = new System.Drawing.Size(589, 314);
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
            this.filterVariablen.Size = new System.Drawing.Size(589, 40);
            this.filterVariablen.TabIndex = 1;
            this.filterVariablen.TabStop = false;
            // 
            // grpTextAllgemein
            // 
            this.grpTextAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpTextAllgemein.Controls.Add(this.txbTestZeile);
            this.grpTextAllgemein.Controls.Add(this.capTestZeile);
            this.grpTextAllgemein.Controls.Add(this.txbSkriptInfo);
            this.grpTextAllgemein.Controls.Add(this.btnTest);
            this.grpTextAllgemein.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTextAllgemein.Location = new System.Drawing.Point(3, 16);
            this.grpTextAllgemein.Name = "grpTextAllgemein";
            this.grpTextAllgemein.Size = new System.Drawing.Size(597, 101);
            this.grpTextAllgemein.TabIndex = 3;
            this.grpTextAllgemein.TabStop = false;
            this.grpTextAllgemein.Text = "Allgemein";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbTestZeile.Location = new System.Drawing.Point(8, 64);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.Size = new System.Drawing.Size(144, 24);
            this.txbTestZeile.TabIndex = 2;
            // 
            // capTestZeile
            // 
            this.capTestZeile.CausesValidation = false;
            this.capTestZeile.Location = new System.Drawing.Point(8, 48);
            this.capTestZeile.Name = "capTestZeile";
            this.capTestZeile.Size = new System.Drawing.Size(72, 16);
            this.capTestZeile.Text = "Test-Zeile:";
            // 
            // txbSkriptInfo
            // 
            this.txbSkriptInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbSkriptInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbSkriptInfo.Location = new System.Drawing.Point(160, 16);
            this.txbSkriptInfo.Name = "txbSkriptInfo";
            this.txbSkriptInfo.Size = new System.Drawing.Size(428, 80);
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
            // ScriptEditor
            // 
            this.Controls.Add(this.tabCSckript);
            this.Controls.Add(this.grpTextAllgemein);
            this.Size = new System.Drawing.Size(603, 503);
            this.tabCSckript.ResumeLayout(false);
            this.tabScriptAnzeige.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSkript)).EndInit();
            this.tabBefehle.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.grpTextAllgemein.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
        private Controls.TabControl tabCSckript;
        private Controls.TabPage tabScriptAnzeige;
        private FastColoredTextBoxNS.FastColoredTextBox txtSkript;
        private Controls.TabPage tabBefehle;
        private Controls.TextBox txbComms;
        private Controls.ListBox lstComands;
        private Controls.TabPage tabVariablen;
        private Controls.Table tableVariablen;
        private BlueDatabaseDialogs.Filterleiste filterVariablen;
        private Controls.GroupBox grpTextAllgemein;
        private Controls.TextBox txbTestZeile;
        private Controls.Caption capTestZeile;
        private Controls.TextBox txbSkriptInfo;
        private Controls.Button btnTest;
    }
}
