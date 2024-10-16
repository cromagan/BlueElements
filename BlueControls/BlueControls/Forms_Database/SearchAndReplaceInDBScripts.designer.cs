using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    internal sealed partial class SearchAndReplaceInDBScripts {
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
            this.btnAusfuehren = new Button();
            this.txbNeu = new TextBox();
            this.txbAlt = new TextBox();
            this.grpSuche = new GroupBox();
            this.grpErsetzen = new GroupBox();
            this.grpSonderzeichen = new GroupBox();
            this.capSonderzeichen = new Caption();
            this.grpSuche.SuspendLayout();
            this.grpErsetzen.SuspendLayout();
            this.grpSonderzeichen.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAusfuehren
            // 
            this.btnAusfuehren.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnAusfuehren.Enabled = false;
            this.btnAusfuehren.ImageCode = "Häkchen|24";
            this.btnAusfuehren.Location = new Point(462, 225);
            this.btnAusfuehren.Name = "btnAusfuehren";
            this.btnAusfuehren.Size = new Size(120, 32);
            this.btnAusfuehren.TabIndex = 4;
            this.btnAusfuehren.Text = "Ausführen";
            this.btnAusfuehren.Click += new EventHandler(this.ers_Click);
            // 
            // txbNeu
            // 
            this.txbNeu.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                   | AnchorStyles.Left) 
                                                  | AnchorStyles.Right)));
            this.txbNeu.Cursor = Cursors.IBeam;
            this.txbNeu.Location = new Point(8, 16);
            this.txbNeu.Name = "txbNeu";
            this.txbNeu.Size = new Size(560, 80);
            this.txbNeu.TabIndex = 3;
            this.txbNeu.TextChanged += new EventHandler(this.AltNeu_TextChanged);
            // 
            // txbAlt
            // 
            this.txbAlt.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                   | AnchorStyles.Left) 
                                                  | AnchorStyles.Right)));
            this.txbAlt.Cursor = Cursors.IBeam;
            this.txbAlt.Location = new Point(8, 16);
            this.txbAlt.Name = "txbAlt";
            this.txbAlt.Size = new Size(560, 64);
            this.txbAlt.TabIndex = 2;
            this.txbAlt.TextChanged += new EventHandler(this.AltNeu_TextChanged);
            // 
            // grpSuche
            // 
            this.grpSuche.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                    | AnchorStyles.Right)));
            this.grpSuche.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSuche.CausesValidation = false;
            this.grpSuche.Controls.Add(this.txbAlt);
            this.grpSuche.Location = new Point(8, 8);
            this.grpSuche.Name = "grpSuche";
            this.grpSuche.Size = new Size(575, 88);
            this.grpSuche.TabIndex = 3;
            this.grpSuche.TabStop = false;
            this.grpSuche.Text = "Suche";
            // 
            // grpErsetzen
            // 
            this.grpErsetzen.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                       | AnchorStyles.Right)));
            this.grpErsetzen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpErsetzen.CausesValidation = false;
            this.grpErsetzen.Controls.Add(this.txbNeu);
            this.grpErsetzen.Location = new Point(8, 96);
            this.grpErsetzen.Name = "grpErsetzen";
            this.grpErsetzen.Size = new Size(575, 104);
            this.grpErsetzen.TabIndex = 2;
            this.grpErsetzen.TabStop = false;
            this.grpErsetzen.Text = "Ersetzen";
            // 
            // grpSonderzeichen
            // 
            this.grpSonderzeichen.BackColor = Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.grpSonderzeichen.CausesValidation = false;
            this.grpSonderzeichen.Controls.Add(this.capSonderzeichen);
            this.grpSonderzeichen.Location = new Point(8, 200);
            this.grpSonderzeichen.Name = "grpSonderzeichen";
            this.grpSonderzeichen.Size = new Size(235, 56);
            this.grpSonderzeichen.TabIndex = 0;
            this.grpSonderzeichen.TabStop = false;
            this.grpSonderzeichen.Text = "Sonderzeichen";
            // 
            // capSonderzeichen
            // 
            this.capSonderzeichen.CausesValidation = false;
            this.capSonderzeichen.Location = new Point(8, 16);
            this.capSonderzeichen.Name = "capSonderzeichen";
            this.capSonderzeichen.Size = new Size(128, 32);
            this.capSonderzeichen.Text = "\\r = Zeilenumbruch<br>\\t = Tabulator";
            // 
            // SearchAndReplaceInDBScripts
            // 
            this.ClientSize = new Size(589, 260);
            this.Controls.Add(this.grpSonderzeichen);
            this.Controls.Add(this.grpErsetzen);
            this.Controls.Add(this.grpSuche);
            this.Controls.Add(this.btnAusfuehren);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = "SearchAndReplaceInDBScripts";
            this.ShowInTaskbar = false;
            this.Text = "Suchen und Ersetzen in allen Datenbank Skripten";
            this.TopMost = true;
            this.grpSuche.ResumeLayout(false);
            this.grpErsetzen.ResumeLayout(false);
            this.grpSonderzeichen.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private TextBox txbAlt;
        private TextBox txbNeu;
        private Button btnAusfuehren;
        private GroupBox grpSuche;
        private GroupBox grpErsetzen;
        private GroupBox grpSonderzeichen;
        private Caption capSonderzeichen;
    }
}
