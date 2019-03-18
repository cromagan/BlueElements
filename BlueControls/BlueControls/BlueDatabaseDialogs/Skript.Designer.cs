namespace BlueControls.BlueDatabaseDialogs
{
    partial class Skript
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpOptionen = new BlueControls.Controls.GroupBox();
            this.optVorhandenZeile = new BlueControls.Controls.Button();
            this.opbNeueZeile = new BlueControls.Controls.Button();
            this.grpImportSkript = new BlueControls.Controls.GroupBox();
            this.chkFehlgeschlageneSpalten = new BlueControls.Controls.Button();
            this.caption1 = new BlueControls.Controls.Caption();
            this.txbImportSkript = new BlueControls.Controls.TextBox();
            this.btnClipboard = new BlueControls.Controls.Button();
            this.grpOptionen.SuspendLayout();
            this.grpImportSkript.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpOptionen
            // 
            this.grpOptionen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOptionen.CausesValidation = false;
            this.grpOptionen.Controls.Add(this.optVorhandenZeile);
            this.grpOptionen.Controls.Add(this.opbNeueZeile);
            this.grpOptionen.Location = new System.Drawing.Point(8, 8);
            this.grpOptionen.Name = "grpOptionen";
            this.grpOptionen.Size = new System.Drawing.Size(664, 72);
            this.grpOptionen.Text = "Optionen";
            // 
            // optVorhandenZeile
            // 
            this.optVorhandenZeile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optVorhandenZeile.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optVorhandenZeile.Enabled = false;
            this.optVorhandenZeile.Location = new System.Drawing.Point(8, 48);
            this.optVorhandenZeile.Name = "optVorhandenZeile";
            this.optVorhandenZeile.Size = new System.Drawing.Size(648, 16);
            this.optVorhandenZeile.TabIndex = 1;
            this.optVorhandenZeile.Text = "Aktuell angewählte Zeile überschreiben";
            // 
            // opbNeueZeile
            // 
            this.opbNeueZeile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.opbNeueZeile.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.opbNeueZeile.Location = new System.Drawing.Point(8, 24);
            this.opbNeueZeile.Name = "opbNeueZeile";
            this.opbNeueZeile.Size = new System.Drawing.Size(648, 16);
            this.opbNeueZeile.TabIndex = 0;
            this.opbNeueZeile.Text = "Neue Zeile erstellen";
            // 
            // grpImportSkript
            // 
            this.grpImportSkript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpImportSkript.CausesValidation = false;
            this.grpImportSkript.Controls.Add(this.chkFehlgeschlageneSpalten);
            this.grpImportSkript.Controls.Add(this.caption1);
            this.grpImportSkript.Controls.Add(this.txbImportSkript);
            this.grpImportSkript.Location = new System.Drawing.Point(8, 88);
            this.grpImportSkript.Name = "grpImportSkript";
            this.grpImportSkript.Size = new System.Drawing.Size(664, 440);
            this.grpImportSkript.Text = "Import-Skript";
            // 
            // chkFehlgeschlageneSpalten
            // 
            this.chkFehlgeschlageneSpalten.ButtonStyle = BlueControls.Enums.enButtonStyle.Checkbox_Text;
            this.chkFehlgeschlageneSpalten.Checked = true;
            this.chkFehlgeschlageneSpalten.Location = new System.Drawing.Point(368, 24);
            this.chkFehlgeschlageneSpalten.Name = "chkFehlgeschlageneSpalten";
            this.chkFehlgeschlageneSpalten.Size = new System.Drawing.Size(264, 40);
            this.chkFehlgeschlageneSpalten.TabIndex = 1;
            this.chkFehlgeschlageneSpalten.Text = "Melde auch Fehler, wenn ein Eintrag nicht im Quelltext vorhanen ist.";
            // 
            // caption1
            // 
            this.caption1.CausesValidation = false;
            this.caption1.Location = new System.Drawing.Point(8, 24);
            this.caption1.Name = "caption1";
            this.caption1.Size = new System.Drawing.Size(352, 72);
            this.caption1.Text = "Import1|ZielSpalte|Vortext|Nachtext - überschreiben<br>Import2|ZielSpalte|Vortext" +
    "|Nachtext - ergänzten<br>;cr; = Zeilenumbruch<br>;tab; = Tabulator";
            // 
            // txbImportSkript
            // 
            this.txbImportSkript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbImportSkript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbImportSkript.Location = new System.Drawing.Point(8, 96);
            this.txbImportSkript.MultiLine = true;
            this.txbImportSkript.Name = "txbImportSkript";
            this.txbImportSkript.Size = new System.Drawing.Size(648, 339);
            this.txbImportSkript.TabIndex = 0;
            // 
            // btnClipboard
            // 
            this.btnClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClipboard.ImageCode = "Clipboard|24";
            this.btnClipboard.Location = new System.Drawing.Point(536, 536);
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new System.Drawing.Size(136, 40);
            this.btnClipboard.TabIndex = 1;
            this.btnClipboard.Text = "aus Clipboard";
            this.btnClipboard.Click += new System.EventHandler(this.btnClipboard_Click);
            // 
            // Skript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 579);
            this.Controls.Add(this.btnClipboard);
            this.Controls.Add(this.grpImportSkript);
            this.Controls.Add(this.grpOptionen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Skript";
            this.Text = "Skript";
            this.grpOptionen.ResumeLayout(false);
            this.grpImportSkript.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.GroupBox grpOptionen;
        private Controls.Button optVorhandenZeile;
        private Controls.Button opbNeueZeile;
        private Controls.GroupBox grpImportSkript;
        private Controls.TextBox txbImportSkript;
        private Controls.Caption caption1;
        private Controls.Button btnClipboard;
        private Controls.Button chkFehlgeschlageneSpalten;
    }
}