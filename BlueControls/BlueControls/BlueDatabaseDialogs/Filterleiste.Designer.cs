namespace BlueControls.BlueDatabaseDialogs
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
            this.btnPin = new BlueControls.Controls.Button();
            this.btnPinZurück = new BlueControls.Controls.Button();
            this.btnAdmin = new BlueControls.Controls.Button();
            this.pic = new System.Windows.Forms.PictureBox();
            this.btnÄhnliche = new BlueControls.Controls.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTextLöschen
            // 
            this.btnTextLöschen.ImageCode = "Kreuz|16";
            this.btnTextLöschen.Location = new System.Drawing.Point(144, 8);
            this.btnTextLöschen.Name = "btnTextLöschen";
            this.btnTextLöschen.Size = new System.Drawing.Size(24, 24);
            this.btnTextLöschen.TabIndex = 13;
            this.btnTextLöschen.Click += new System.EventHandler(this.btnTextLöschen_Click);
            // 
            // txbZeilenFilter
            // 
            this.txbZeilenFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeilenFilter.Location = new System.Drawing.Point(8, 8);
            this.txbZeilenFilter.Margin = new System.Windows.Forms.Padding(4);
            this.txbZeilenFilter.Name = "txbZeilenFilter";
            this.txbZeilenFilter.Size = new System.Drawing.Size(136, 24);
            this.txbZeilenFilter.TabIndex = 11;
            this.txbZeilenFilter.TextChanged += new System.EventHandler(this.txbZeilenFilter_TextChanged);
            this.txbZeilenFilter.Enter += new System.EventHandler(this.txbZeilenFilter_Enter);
            // 
            // btnAlleFilterAus
            // 
            this.btnAlleFilterAus.ImageCode = "Trichter|16|||||||||Kreuz";
            this.btnAlleFilterAus.Location = new System.Drawing.Point(176, 8);
            this.btnAlleFilterAus.Margin = new System.Windows.Forms.Padding(4);
            this.btnAlleFilterAus.Name = "btnAlleFilterAus";
            this.btnAlleFilterAus.Size = new System.Drawing.Size(128, 24);
            this.btnAlleFilterAus.TabIndex = 12;
            this.btnAlleFilterAus.Text = "alle Filter aus";
            this.btnAlleFilterAus.Click += new System.EventHandler(this.btnAlleFilterAus_Click);
            // 
            // btnPin
            // 
            this.btnPin.ImageCode = "Pinnadel|20";
            this.btnPin.Location = new System.Drawing.Point(312, 8);
            this.btnPin.Name = "btnPin";
            this.btnPin.QuickInfo = "Angezeigte Zeilen anpinnen";
            this.btnPin.Size = new System.Drawing.Size(24, 24);
            this.btnPin.TabIndex = 14;
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);
            // 
            // btnPinZurück
            // 
            this.btnPinZurück.ImageCode = "Pinnadel|20|||||||||Kreuz";
            this.btnPinZurück.Location = new System.Drawing.Point(336, 8);
            this.btnPinZurück.Name = "btnPinZurück";
            this.btnPinZurück.QuickInfo = "Angepinnte Zeilen zurücksetzen";
            this.btnPinZurück.Size = new System.Drawing.Size(24, 24);
            this.btnPinZurück.TabIndex = 15;
            this.btnPinZurück.Click += new System.EventHandler(this.btnPinZurück_Click);
            // 
            // btnAdmin
            // 
            this.btnAdmin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdmin.ImageCode = "Zahnrad|16|||FF0000";
            this.btnAdmin.Location = new System.Drawing.Point(928, 0);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(24, 24);
            this.btnAdmin.TabIndex = 16;
            this.btnAdmin.Visible = false;
            this.btnAdmin.Click += new System.EventHandler(this.btnAdmin_Click);
            // 
            // pic
            // 
            this.pic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pic.BackColor = System.Drawing.Color.White;
            this.pic.Location = new System.Drawing.Point(8, 40);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(936, 104);
            this.pic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic.TabIndex = 17;
            this.pic.TabStop = false;
            this.pic.Visible = false;
            // 
            // btnÄhnliche
            // 
            this.btnÄhnliche.ImageCode = "Fernglas|16|||||||||Istgleich";
            this.btnÄhnliche.Location = new System.Drawing.Point(8, 40);
            this.btnÄhnliche.Margin = new System.Windows.Forms.Padding(4);
            this.btnÄhnliche.Name = "btnÄhnliche";
            this.btnÄhnliche.Size = new System.Drawing.Size(136, 24);
            this.btnÄhnliche.TabIndex = 18;
            this.btnÄhnliche.Text = "ähnlich";
            this.btnÄhnliche.Visible = false;
            this.btnÄhnliche.Click += new System.EventHandler(this.btnÄhnliche_Click);
            // 
            // Filterleiste
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btnÄhnliche);
            this.Controls.Add(this.pic);
            this.Controls.Add(this.btnAdmin);
            this.Controls.Add(this.btnPinZurück);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.btnTextLöschen);
            this.Controls.Add(this.txbZeilenFilter);
            this.Controls.Add(this.btnAlleFilterAus);
            this.Name = "Filterleiste";
            this.Size = new System.Drawing.Size(951, 53);
            this.SizeChanged += new System.EventHandler(this.Filterleiste_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.Button btnTextLöschen;
        private Controls.TextBox txbZeilenFilter;
        private Controls.Button btnAlleFilterAus;
        private Controls.Button btnPin;
        private Controls.Button btnPinZurück;
        private Controls.Button btnAdmin;
        internal System.Windows.Forms.PictureBox pic;
        private Controls.Button btnÄhnliche;
    }
}
