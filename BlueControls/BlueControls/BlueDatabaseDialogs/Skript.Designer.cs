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
            this.opbNeueZeile = new BlueControls.Controls.Button();
            this.optVorhandenZeile = new BlueControls.Controls.Button();
            this.grpImportSkript = new BlueControls.Controls.GroupBox();
            this.txbImportSkript = new BlueControls.Controls.TextBox();
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
            // grpImportSkript
            // 
            this.grpImportSkript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpImportSkript.CausesValidation = false;
            this.grpImportSkript.Controls.Add(this.txbImportSkript);
            this.grpImportSkript.Location = new System.Drawing.Point(8, 88);
            this.grpImportSkript.Name = "grpImportSkript";
            this.grpImportSkript.Size = new System.Drawing.Size(664, 408);
            this.grpImportSkript.Text = "Import-Skript";
            // 
            // txbImportSkript
            // 
            this.txbImportSkript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbImportSkript.Location = new System.Drawing.Point(8, 24);
            this.txbImportSkript.Name = "txbImportSkript";
            this.txbImportSkript.Size = new System.Drawing.Size(648, 344);
            this.txbImportSkript.TabIndex = 0;
            // 
            // Skript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 590);
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
    }
}