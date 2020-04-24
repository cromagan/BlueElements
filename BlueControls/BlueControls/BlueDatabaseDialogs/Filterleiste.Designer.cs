﻿namespace BlueControls.BlueDatabaseDialogs
{
    partial class Filterleiste
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnTextLöschen = new BlueControls.Controls.Button();
            this.txbZeilenFilter = new BlueControls.Controls.TextBox();
            this.btnAlleFilterAus = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // btnTextLöschen
            // 
            this.btnTextLöschen.ImageCode = "Kreuz|16";
            this.btnTextLöschen.Location = new System.Drawing.Point(144, 16);
            this.btnTextLöschen.Name = "btnTextLöschen";
            this.btnTextLöschen.Size = new System.Drawing.Size(24, 22);
            this.btnTextLöschen.TabIndex = 13;
            this.btnTextLöschen.Click += new System.EventHandler(this.btnTextLöschen_Click);
            // 
            // txbZeilenFilter
            // 
            this.txbZeilenFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeilenFilter.Location = new System.Drawing.Point(8, 16);
            this.txbZeilenFilter.Margin = new System.Windows.Forms.Padding(4);
            this.txbZeilenFilter.Name = "ZeilenFilter_TextFeld";
            this.txbZeilenFilter.QuickInfo = "Texte können mit <b>+</b> kombiniert werden.<br><i>Kein Leerzeichen beim +-Zeiche" +
    "n benutzen.";
            this.txbZeilenFilter.Size = new System.Drawing.Size(136, 22);
            this.txbZeilenFilter.TabIndex = 11;
            this.txbZeilenFilter.TextChanged += new System.EventHandler(this.txbZeilenFilter_TextChanged);
            this.txbZeilenFilter.Enter += new System.EventHandler(this.txbZeilenFilter_Enter);
            // 
            // btnAlleFilterAus
            // 
            this.btnAlleFilterAus.ImageCode = "Trichter|16|||||||||Kreuz";
            this.btnAlleFilterAus.Location = new System.Drawing.Point(176, 16);
            this.btnAlleFilterAus.Margin = new System.Windows.Forms.Padding(4);
            this.btnAlleFilterAus.Name = "AlleFilterAus";
            this.btnAlleFilterAus.Size = new System.Drawing.Size(128, 24);
            this.btnAlleFilterAus.TabIndex = 12;
            this.btnAlleFilterAus.Text = "alle Filter aus";
            this.btnAlleFilterAus.Click += new System.EventHandler(this.btnAlleFilterAus_Click);
            // 
            // Suchleiste
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnTextLöschen);
            this.Controls.Add(this.txbZeilenFilter);
            this.Controls.Add(this.btnAlleFilterAus);
            this.Name = "Suchleiste";
            this.Size = new System.Drawing.Size(951, 53);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.Button btnTextLöschen;
        private Controls.TextBox txbZeilenFilter;
        private Controls.Button btnAlleFilterAus;
    }
}
