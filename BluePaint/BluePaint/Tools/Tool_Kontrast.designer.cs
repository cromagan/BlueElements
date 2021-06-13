using BlueControls.Controls;

namespace BluePaint
{
    public partial class Tool_Kontrast 
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
            this.btnAlleFarbenSchwarz = new BlueControls.Controls.Button();
            this.btnKontrastErhoehen = new BlueControls.Controls.Button();
            this.btnGraustufen = new BlueControls.Controls.Button();
            this.btnPixelHinzu = new BlueControls.Controls.Button();
            this.btnAusdünnen = new BlueControls.Controls.Button();
            this.grpKontrast = new BlueControls.Controls.GroupBox();
            this.capKontrast = new BlueControls.Controls.Caption();
            this.sldKontrast = new BlueControls.Controls.Slider();
            this.grpGamma = new BlueControls.Controls.GroupBox();
            this.capGamma = new BlueControls.Controls.Caption();
            this.sldGamma = new BlueControls.Controls.Slider();
            this.btnGamma = new BlueControls.Controls.Button();
            this.grpHelligkeit = new BlueControls.Controls.GroupBox();
            this.capHelligkeit = new BlueControls.Controls.Caption();
            this.sldHelligkeit = new BlueControls.Controls.Slider();
            this.btnHelligkeit = new BlueControls.Controls.Button();
            this.grpKontrast.SuspendLayout();
            this.grpGamma.SuspendLayout();
            this.grpHelligkeit.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAlleFarbenSchwarz
            // 
            this.btnAlleFarbenSchwarz.ImageCode = "Kontrast";
            this.btnAlleFarbenSchwarz.Location = new System.Drawing.Point(8, 328);
            this.btnAlleFarbenSchwarz.Name = "btnAlleFarbenSchwarz";
            this.btnAlleFarbenSchwarz.Size = new System.Drawing.Size(336, 48);
            this.btnAlleFarbenSchwarz.TabIndex = 8;
            this.btnAlleFarbenSchwarz.Text = "Alle Farben zu Schwarz";
            this.btnAlleFarbenSchwarz.Click += new System.EventHandler(this.btnAlleFarbenSchwarz_Click);
            // 
            // btnKontrastErhoehen
            // 
            this.btnKontrastErhoehen.ImageCode = "Kontrast|24";
            this.btnKontrastErhoehen.Location = new System.Drawing.Point(216, 48);
            this.btnKontrastErhoehen.Name = "btnKontrastErhoehen";
            this.btnKontrastErhoehen.Size = new System.Drawing.Size(136, 32);
            this.btnKontrastErhoehen.TabIndex = 6;
            this.btnKontrastErhoehen.Text = "übernehmen";
            this.btnKontrastErhoehen.Click += new System.EventHandler(this.btnKontrastErhoehen_Click);
            // 
            // btnGraustufen
            // 
            this.btnGraustufen.ImageCode = "Graustufen";
            this.btnGraustufen.Location = new System.Drawing.Point(8, 272);
            this.btnGraustufen.Name = "btnGraustufen";
            this.btnGraustufen.Size = new System.Drawing.Size(336, 48);
            this.btnGraustufen.TabIndex = 7;
            this.btnGraustufen.Text = "In Graustufen umwandeln";
            this.btnGraustufen.Click += new System.EventHandler(this.btnGraustufen_Click);
            // 
            // btnPixelHinzu
            // 
            this.btnPixelHinzu.ImageCode = "Gewicht|30";
            this.btnPixelHinzu.Location = new System.Drawing.Point(8, 384);
            this.btnPixelHinzu.Name = "btnPixelHinzu";
            this.btnPixelHinzu.Size = new System.Drawing.Size(336, 48);
            this.btnPixelHinzu.TabIndex = 9;
            this.btnPixelHinzu.Text = "Schwarze Pixel hinzufügen";
            this.btnPixelHinzu.Click += new System.EventHandler(this.btnPixelHinzu_Click);
            // 
            // btnAusdünnen
            // 
            this.btnAusdünnen.ImageCode = "Feder|30";
            this.btnAusdünnen.Location = new System.Drawing.Point(8, 440);
            this.btnAusdünnen.Name = "btnAusdünnen";
            this.btnAusdünnen.Size = new System.Drawing.Size(336, 48);
            this.btnAusdünnen.TabIndex = 10;
            this.btnAusdünnen.Text = "Schwarze Pixel entfernen";
            this.btnAusdünnen.Click += new System.EventHandler(this.btnAusdünnen_Click);
            // 
            // grpKontrast
            // 
            this.grpKontrast.CausesValidation = false;
            this.grpKontrast.Controls.Add(this.capKontrast);
            this.grpKontrast.Controls.Add(this.sldKontrast);
            this.grpKontrast.Controls.Add(this.btnKontrastErhoehen);
            this.grpKontrast.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpKontrast.Location = new System.Drawing.Point(0, 0);
            this.grpKontrast.Name = "grpKontrast";
            this.grpKontrast.Size = new System.Drawing.Size(360, 88);
            this.grpKontrast.Text = "Kontrast";
            // 
            // capKontrast
            // 
            this.capKontrast.Location = new System.Drawing.Point(280, 16);
            this.capKontrast.Name = "capKontrast";
            this.capKontrast.Size = new System.Drawing.Size(72, 24);
            // 
            // sldKontrast
            // 
            this.sldKontrast.Location = new System.Drawing.Point(16, 16);
            this.sldKontrast.Minimum = -100D;
            this.sldKontrast.Name = "sldKontrast";
            this.sldKontrast.Size = new System.Drawing.Size(248, 24);
            this.sldKontrast.ValueChanged += new System.EventHandler(this.sldKontrast_ValueChanged);
            // 
            // grpGamma
            // 
            this.grpGamma.CausesValidation = false;
            this.grpGamma.Controls.Add(this.capGamma);
            this.grpGamma.Controls.Add(this.sldGamma);
            this.grpGamma.Controls.Add(this.btnGamma);
            this.grpGamma.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpGamma.Location = new System.Drawing.Point(0, 88);
            this.grpGamma.Name = "grpGamma";
            this.grpGamma.Size = new System.Drawing.Size(360, 88);
            this.grpGamma.Text = "Gamma";
            // 
            // capGamma
            // 
            this.capGamma.Location = new System.Drawing.Point(280, 16);
            this.capGamma.Name = "capGamma";
            this.capGamma.Size = new System.Drawing.Size(72, 24);
            // 
            // sldGamma
            // 
            this.sldGamma.LargeChange = 0.01D;
            this.sldGamma.Location = new System.Drawing.Point(16, 16);
            this.sldGamma.Maximum = 2.2D;
            this.sldGamma.MouseChange = 0.01D;
            this.sldGamma.Name = "sldGamma";
            this.sldGamma.Size = new System.Drawing.Size(248, 24);
            this.sldGamma.SmallChange = 0.01D;
            this.sldGamma.Value = 1D;
            this.sldGamma.ValueChanged += new System.EventHandler(this.sldGamma_ValueChanged);
            // 
            // btnGamma
            // 
            this.btnGamma.ImageCode = "Kontrast|24";
            this.btnGamma.Location = new System.Drawing.Point(208, 48);
            this.btnGamma.Name = "btnGamma";
            this.btnGamma.Size = new System.Drawing.Size(136, 32);
            this.btnGamma.TabIndex = 6;
            this.btnGamma.Text = "übernehmen";
            this.btnGamma.Click += new System.EventHandler(this.btnGamma_Click);
            // 
            // grpHelligkeit
            // 
            this.grpHelligkeit.CausesValidation = false;
            this.grpHelligkeit.Controls.Add(this.capHelligkeit);
            this.grpHelligkeit.Controls.Add(this.sldHelligkeit);
            this.grpHelligkeit.Controls.Add(this.btnHelligkeit);
            this.grpHelligkeit.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpHelligkeit.Location = new System.Drawing.Point(0, 176);
            this.grpHelligkeit.Name = "grpHelligkeit";
            this.grpHelligkeit.Size = new System.Drawing.Size(360, 88);
            this.grpHelligkeit.Text = "Helligkeit";
            // 
            // capHelligkeit
            // 
            this.capHelligkeit.Location = new System.Drawing.Point(280, 16);
            this.capHelligkeit.Name = "capHelligkeit";
            this.capHelligkeit.Size = new System.Drawing.Size(72, 24);
            // 
            // sldHelligkeit
            // 
            this.sldHelligkeit.LargeChange = 0.01D;
            this.sldHelligkeit.Location = new System.Drawing.Point(16, 16);
            this.sldHelligkeit.Maximum = 2D;
            this.sldHelligkeit.Minimum = 0D;
            this.sldHelligkeit.MouseChange = 0.01D;
            this.sldHelligkeit.Name = "sldHelligkeit";
            this.sldHelligkeit.Size = new System.Drawing.Size(248, 24);
            this.sldHelligkeit.SmallChange = 0.01D;
            this.sldHelligkeit.Value = 1D;
            this.sldHelligkeit.ValueChanged += new System.EventHandler(this.sldHelligkeit_ValueChanged);
            // 
            // btnHelligkeit
            // 
            this.btnHelligkeit.ImageCode = "Kontrast|24";
            this.btnHelligkeit.Location = new System.Drawing.Point(216, 48);
            this.btnHelligkeit.Name = "btnHelligkeit";
            this.btnHelligkeit.Size = new System.Drawing.Size(136, 32);
            this.btnHelligkeit.TabIndex = 6;
            this.btnHelligkeit.Text = "übernehmen";
            this.btnHelligkeit.Click += new System.EventHandler(this.btnHelligkeit_Click);
            // 
            // Tool_Kontrast
            // 
            this.Controls.Add(this.grpHelligkeit);
            this.Controls.Add(this.grpGamma);
            this.Controls.Add(this.grpKontrast);
            this.Controls.Add(this.btnAusdünnen);
            this.Controls.Add(this.btnPixelHinzu);
            this.Controls.Add(this.btnAlleFarbenSchwarz);
            this.Controls.Add(this.btnGraustufen);
            this.Name = "Tool_Kontrast";
            this.Size = new System.Drawing.Size(360, 505);
            this.grpKontrast.ResumeLayout(false);
            this.grpGamma.ResumeLayout(false);
            this.grpHelligkeit.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        internal Button btnAlleFarbenSchwarz;
        internal Button btnKontrastErhoehen;
        internal Button btnGraustufen;
        internal Button btnPixelHinzu;
        internal Button btnAusdünnen;
        private GroupBox grpKontrast;
        private Caption capKontrast;
        private Slider sldKontrast;
        private GroupBox grpGamma;
        private Caption capGamma;
        private Slider sldGamma;
        internal Button btnGamma;
        private GroupBox grpHelligkeit;
        private Caption capHelligkeit;
        private Slider sldHelligkeit;
        internal Button btnHelligkeit;
    }
}