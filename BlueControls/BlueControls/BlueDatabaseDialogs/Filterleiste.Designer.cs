
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs
{
    partial class Filterleiste
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;
        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components?.Dispose();
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
            this.btnTextLöschen = new Button();
            this.txbZeilenFilter = new TextBox();
            this.btnAlleFilterAus = new Button();
            this.btnPin = new Button();
            this.btnPinZurück = new Button();
            this.btnÄhnliche = new Button();
            this.SuspendLayout();
            // 
            // btnTextLöschen
            // 
            this.btnTextLöschen.ImageCode = "Kreuz|16";
            this.btnTextLöschen.Location = new Point(144, 8);
            this.btnTextLöschen.Name = "btnTextLöschen";
            this.btnTextLöschen.Size = new Size(24, 24);
            this.btnTextLöschen.TabIndex = 13;
            this.btnTextLöschen.Click += new EventHandler(this.btnTextLöschen_Click);
            // 
            // txbZeilenFilter
            // 
            this.txbZeilenFilter.Cursor = Cursors.IBeam;
            this.txbZeilenFilter.Location = new Point(8, 8);
            this.txbZeilenFilter.Margin = new Padding(4);
            this.txbZeilenFilter.Name = "txbZeilenFilter";
            this.txbZeilenFilter.Size = new Size(136, 24);
            this.txbZeilenFilter.TabIndex = 11;
            this.txbZeilenFilter.TextChanged += new EventHandler(this.txbZeilenFilter_TextChanged);
            this.txbZeilenFilter.Enter += new EventHandler(this.txbZeilenFilter_Enter);
            // 
            // btnAlleFilterAus
            // 
            this.btnAlleFilterAus.ImageCode = "Trichter|16|||||||||Kreuz";
            this.btnAlleFilterAus.Location = new Point(176, 8);
            this.btnAlleFilterAus.Margin = new Padding(4);
            this.btnAlleFilterAus.Name = "btnAlleFilterAus";
            this.btnAlleFilterAus.Size = new Size(128, 24);
            this.btnAlleFilterAus.TabIndex = 12;
            this.btnAlleFilterAus.Text = "alle Filter aus";
            this.btnAlleFilterAus.Click += new EventHandler(this.btnAlleFilterAus_Click);
            // 
            // btnPin
            // 
            this.btnPin.ImageCode = "Pinnadel|20";
            this.btnPin.Location = new Point(312, 8);
            this.btnPin.Name = "btnPin";
            this.btnPin.QuickInfo = "Angezeigte Zeilen anpinnen";
            this.btnPin.Size = new Size(24, 24);
            this.btnPin.TabIndex = 14;
            this.btnPin.Click += new EventHandler(this.btnPin_Click);
            // 
            // btnPinZurück
            // 
            this.btnPinZurück.ImageCode = "Pinnadel|20|||||||||Kreuz";
            this.btnPinZurück.Location = new Point(336, 8);
            this.btnPinZurück.Name = "btnPinZurück";
            this.btnPinZurück.QuickInfo = "Angepinnte Zeilen zurücksetzen";
            this.btnPinZurück.Size = new Size(24, 24);
            this.btnPinZurück.TabIndex = 15;
            this.btnPinZurück.Click += new EventHandler(this.btnPinZurück_Click);
            // 
            // btnÄhnliche
            // 
            this.btnÄhnliche.ImageCode = "Fernglas|16|||||||||HäkchenDoppelt";
            this.btnÄhnliche.Location = new Point(8, 40);
            this.btnÄhnliche.Margin = new Padding(4);
            this.btnÄhnliche.Name = "btnÄhnliche";
            this.btnÄhnliche.Size = new Size(136, 24);
            this.btnÄhnliche.TabIndex = 18;
            this.btnÄhnliche.Text = "ähnlich";
            this.btnÄhnliche.Visible = false;
            this.btnÄhnliche.Click += new EventHandler(this.btnÄhnliche_Click);
            // 
            // Filterleiste
            // 
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btnÄhnliche);
            this.Controls.Add(this.btnPinZurück);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.btnTextLöschen);
            this.Controls.Add(this.txbZeilenFilter);
            this.Controls.Add(this.btnAlleFilterAus);
            this.Name = "Filterleiste";
            this.Size = new Size(951, 53);
            this.SizeChanged += new EventHandler(this.Filterleiste_SizeChanged);
            this.ResumeLayout(false);
        }
        #endregion

        private Button btnTextLöschen;
        private TextBox txbZeilenFilter;
        private Button btnAlleFilterAus;
        private Button btnPin;
        private Button btnPinZurück;
        private Button btnÄhnliche;
    }
}
