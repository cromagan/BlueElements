// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs {
    internal sealed partial class SearchAndReplaceInTbScripts {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            btnErsetzen = new Button();
            txbNeu = new TextBox();
            txbAlt = new TextBox();
            grpSuche = new GroupBox();
            btnSuche = new Button();
            grpErsetzen = new GroupBox();
            grpSonderzeichen = new GroupBox();
            capSonderzeichen = new Caption();
            btnFehler = new Button();
            grpSuche.SuspendLayout();
            grpErsetzen.SuspendLayout();
            grpSonderzeichen.SuspendLayout();
            SuspendLayout();
            // 
            // btnErsetzen
            // 
            btnErsetzen.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnErsetzen.Enabled = false;
            btnErsetzen.ImageCode = "Stift|20";
            btnErsetzen.Location = new Point(448, 96);
            btnErsetzen.Name = "btnErsetzen";
            btnErsetzen.Size = new Size(120, 32);
            btnErsetzen.TabIndex = 4;
            btnErsetzen.Text = "Ersetzen";
            btnErsetzen.Click += btnErsetzen_Click;
            // 
            // txbNeu
            // 
            txbNeu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbNeu.Cursor = Cursors.IBeam;
            txbNeu.Location = new Point(8, 16);
            txbNeu.Name = "txbNeu";
            txbNeu.Size = new Size(560, 72);
            txbNeu.TabIndex = 3;
            txbNeu.TextChanged += AltNeu_TextChanged;
            // 
            // txbAlt
            // 
            txbAlt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txbAlt.Cursor = Cursors.IBeam;
            txbAlt.Location = new Point(8, 16);
            txbAlt.Name = "txbAlt";
            txbAlt.Size = new Size(560, 64);
            txbAlt.TabIndex = 2;
            txbAlt.TextChanged += AltNeu_TextChanged;
            // 
            // grpSuche
            // 
            grpSuche.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSuche.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            grpSuche.CausesValidation = false;
            grpSuche.Controls.Add(btnSuche);
            grpSuche.Controls.Add(txbAlt);
            grpSuche.Location = new Point(8, 8);
            grpSuche.Name = "grpSuche";
            grpSuche.Size = new Size(575, 128);
            grpSuche.TabIndex = 3;
            grpSuche.TabStop = false;
            grpSuche.Text = "Suche";
            // 
            // btnSuche
            // 
            btnSuche.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSuche.Enabled = false;
            btnSuche.ImageCode = "Lupe|20";
            btnSuche.Location = new Point(448, 88);
            btnSuche.Name = "btnSuche";
            btnSuche.Size = new Size(120, 32);
            btnSuche.TabIndex = 5;
            btnSuche.Text = "Suchen";
            btnSuche.Click += btnSuche_Click;
            // 
            // grpErsetzen
            // 
            grpErsetzen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpErsetzen.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            grpErsetzen.CausesValidation = false;
            grpErsetzen.Controls.Add(txbNeu);
            grpErsetzen.Controls.Add(btnErsetzen);
            grpErsetzen.Location = new Point(8, 136);
            grpErsetzen.Name = "grpErsetzen";
            grpErsetzen.Size = new Size(575, 136);
            grpErsetzen.TabIndex = 2;
            grpErsetzen.TabStop = false;
            grpErsetzen.Text = "Ersetzen";
            // 
            // grpSonderzeichen
            // 
            grpSonderzeichen.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            grpSonderzeichen.CausesValidation = false;
            grpSonderzeichen.Controls.Add(capSonderzeichen);
            grpSonderzeichen.Location = new Point(8, 272);
            grpSonderzeichen.Name = "grpSonderzeichen";
            grpSonderzeichen.Size = new Size(160, 56);
            grpSonderzeichen.TabIndex = 0;
            grpSonderzeichen.TabStop = false;
            grpSonderzeichen.Text = "Sonderzeichen";
            // 
            // capSonderzeichen
            // 
            capSonderzeichen.CausesValidation = false;
            capSonderzeichen.Location = new Point(8, 16);
            capSonderzeichen.Name = "capSonderzeichen";
            capSonderzeichen.Size = new Size(128, 32);
            capSonderzeichen.Text = "\\r = Zeilenumbruch<br>\\t = Tabulator";
            // 
            // btnFehler
            // 
            btnFehler.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFehler.ImageCode = "Warnung|20";
            btnFehler.Location = new Point(184, 288);
            btnFehler.Name = "btnFehler";
            btnFehler.Size = new Size(168, 32);
            btnFehler.TabIndex = 5;
            btnFehler.Text = "Alle Fehler zurücksetzen";
            btnFehler.Click += btnFehler_Click;
            // 
            // SearchAndReplaceInTbScripts
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(589, 336);
            Controls.Add(btnFehler);
            Controls.Add(grpSonderzeichen);
            Controls.Add(grpErsetzen);
            Controls.Add(grpSuche);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "SearchAndReplaceInTbScripts";
            ShowInTaskbar = false;
            Text = "Suchen und Ersetzen in allen Tabelle Skripten";
            TopMost = true;
            grpSuche.ResumeLayout(false);
            grpErsetzen.ResumeLayout(false);
            grpSonderzeichen.ResumeLayout(false);
            ResumeLayout(false);

        }
        private TextBox txbAlt;
        private TextBox txbNeu;
        private Button btnErsetzen;
        private GroupBox grpSuche;
        private GroupBox grpErsetzen;
        private GroupBox grpSonderzeichen;
        private Caption capSonderzeichen;
        private Button btnSuche;
        private Button btnFehler;
    }
}
