using BlueControls.Controls;

namespace BluePaint
{
    public partial class Tool_Abspielen 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.txbQuelle = new BlueControls.Controls.TextBox();
            this.capQuell = new BlueControls.Controls.Caption();
            this.optUeberschreiben = new BlueControls.Controls.Button();
            this.optNeuerName = new BlueControls.Controls.Button();
            this.optZielordner = new BlueControls.Controls.Button();
            this.txbZielordner = new BlueControls.Controls.TextBox();
            this.btnAbspielen = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // txbQuelle
            // 
            this.txbQuelle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbQuelle.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbQuelle.Location = new System.Drawing.Point(16, 24);
            this.txbQuelle.Name = "txbQuelle";
            this.txbQuelle.Size = new System.Drawing.Size(380, 24);
            this.txbQuelle.TabIndex = 0;
            this.txbQuelle.TextChanged += new System.EventHandler(this.txbQuelle_TextChanged);
            // 
            // capQuell
            // 
            this.capQuell.Location = new System.Drawing.Point(16, 8);
            this.capQuell.Name = "capQuell";
            this.capQuell.Size = new System.Drawing.Size(120, 16);
            this.capQuell.Text = "Quell-Ordner:";
            // 
            // optUeberschreiben
            // 
            this.optUeberschreiben.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optUeberschreiben.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optUeberschreiben.Enabled = false;
            this.optUeberschreiben.Location = new System.Drawing.Point(16, 104);
            this.optUeberschreiben.Name = "optUeberschreiben";
            this.optUeberschreiben.Size = new System.Drawing.Size(373, 24);
            this.optUeberschreiben.TabIndex = 1;
            this.optUeberschreiben.Text = "Original-Datei überschreiben";
            this.optUeberschreiben.Click += new System.EventHandler(this.optUeberschreiben_Click);
            // 
            // optNeuerName
            // 
            this.optNeuerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optNeuerName.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optNeuerName.Location = new System.Drawing.Point(16, 80);
            this.optNeuerName.Name = "optNeuerName";
            this.optNeuerName.Size = new System.Drawing.Size(373, 24);
            this.optNeuerName.TabIndex = 2;
            this.optNeuerName.Text = "Ziel-Ordner = Quell-Ordner, aber neuen Dateinamen benutzen";
            this.optNeuerName.Click += new System.EventHandler(this.optNeuerName_Click);
            // 
            // optZielordner
            // 
            this.optZielordner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optZielordner.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.optZielordner.Location = new System.Drawing.Point(16, 128);
            this.optZielordner.Name = "btnZielordner";
            this.optZielordner.Size = new System.Drawing.Size(373, 24);
            this.optZielordner.TabIndex = 3;
            this.optZielordner.Text = "Neue Bilder in diesen Ordner speichern:";
            this.optZielordner.Click += new System.EventHandler(this.optZielordner_Click);
            // 
            // txbZielordner
            // 
            this.txbZielordner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbZielordner.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZielordner.Location = new System.Drawing.Point(32, 144);
            this.txbZielordner.Name = "txbZielordner";
            this.txbZielordner.Size = new System.Drawing.Size(364, 24);
            this.txbZielordner.TabIndex = 4;
            this.txbZielordner.TextChanged += new System.EventHandler(this.txbZielordner_TextChanged);
            // 
            // btnAbspielen
            // 
            this.btnAbspielen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbspielen.ImageCode = "Abspielen";
            this.btnAbspielen.Location = new System.Drawing.Point(269, 192);
            this.btnAbspielen.Name = "btnAbspielen";
            this.btnAbspielen.Size = new System.Drawing.Size(119, 56);
            this.btnAbspielen.TabIndex = 5;
            this.btnAbspielen.Text = "abspielen";
            this.btnAbspielen.Click += new System.EventHandler(this.btnAbspielen_Click);
            // 
            // Tool_Abspielen
            // 
            this.ClientSize = new System.Drawing.Size(405, 383);
            this.Controls.Add(this.btnAbspielen);
            this.Controls.Add(this.txbZielordner);
            this.Controls.Add(this.optZielordner);
            this.Controls.Add(this.optNeuerName);
            this.Controls.Add(this.optUeberschreiben);
            this.Controls.Add(this.capQuell);
            this.Controls.Add(this.txbQuelle);
            this.Name = "Tool_Abspielen";
            this.ResumeLayout(false);
        }
        private TextBox txbQuelle;
        private Caption capQuell;
        private Button optUeberschreiben;
        private Button optNeuerName;
        private Button optZielordner;
        private TextBox txbZielordner;
        private Button btnAbspielen;
    }
}