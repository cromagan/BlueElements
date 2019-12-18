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
            this.sldKontrast = new BlueControls.Controls.Slider();
            this.capKontrast = new BlueControls.Controls.Caption();
            this.grpKontrast.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAlleFarbenSchwarz
            // 
            this.btnAlleFarbenSchwarz.ImageCode = "Kontrast";
            this.btnAlleFarbenSchwarz.Location = new System.Drawing.Point(16, 160);
            this.btnAlleFarbenSchwarz.Name = "btnAlleFarbenSchwarz";
            this.btnAlleFarbenSchwarz.Size = new System.Drawing.Size(324, 48);
            this.btnAlleFarbenSchwarz.TabIndex = 8;
            this.btnAlleFarbenSchwarz.Text = "Alle Farben zu Schwarz";
            this.btnAlleFarbenSchwarz.Click += new System.EventHandler(this.btnAlleFarbenSchwarz_Click);
            // 
            // btnKontrastErhoehen
            // 
            this.btnKontrastErhoehen.ImageCode = "Kontrast|24";
            this.btnKontrastErhoehen.Location = new System.Drawing.Point(216, 56);
            this.btnKontrastErhoehen.Name = "btnKontrastErhoehen";
            this.btnKontrastErhoehen.Size = new System.Drawing.Size(136, 32);
            this.btnKontrastErhoehen.TabIndex = 6;
            this.btnKontrastErhoehen.Text = "übernehmen";
            this.btnKontrastErhoehen.Click += new System.EventHandler(this.btnKontrastErhoehen_Click);
            // 
            // btnGraustufen
            // 
            this.btnGraustufen.ImageCode = "Graustufen";
            this.btnGraustufen.Location = new System.Drawing.Point(16, 104);
            this.btnGraustufen.Name = "btnGraustufen";
            this.btnGraustufen.Size = new System.Drawing.Size(324, 48);
            this.btnGraustufen.TabIndex = 7;
            this.btnGraustufen.Text = "In Graustufen umwandeln";
            this.btnGraustufen.Click += new System.EventHandler(this.btnGraustufen_Click);
            // 
            // btnPixelHinzu
            // 
            this.btnPixelHinzu.ImageCode = "Gewicht|30";
            this.btnPixelHinzu.Location = new System.Drawing.Point(16, 216);
            this.btnPixelHinzu.Name = "btnPixelHinzu";
            this.btnPixelHinzu.Size = new System.Drawing.Size(324, 48);
            this.btnPixelHinzu.TabIndex = 9;
            this.btnPixelHinzu.Text = "Schwarze Pixel hinzufügen";
            this.btnPixelHinzu.Click += new System.EventHandler(this.btnPixelHinzu_Click);
            // 
            // btnAusdünnen
            // 
            this.btnAusdünnen.ImageCode = "Feder|30";
            this.btnAusdünnen.Location = new System.Drawing.Point(16, 272);
            this.btnAusdünnen.Name = "btnAusdünnen";
            this.btnAusdünnen.Size = new System.Drawing.Size(324, 48);
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
            this.grpKontrast.Size = new System.Drawing.Size(360, 96);
            this.grpKontrast.Text = "Kontrast";
            // 
            // sldKontrast
            // 
            this.sldKontrast.Location = new System.Drawing.Point(16, 24);
            this.sldKontrast.MouseChange = 0.01D;
            this.sldKontrast.Maximum = 2D;
            this.sldKontrast.Minimum = 0.5D;
            this.sldKontrast.Name = "sldKontrast";
            this.sldKontrast.Size = new System.Drawing.Size(248, 24);
            this.sldKontrast.SmallChange = 0.01D;
            this.sldKontrast.Value = 1D;
            this.sldKontrast.ValueChanged += new System.EventHandler(this.sldKontrast_ValueChanged);
            // 
            // capKontrast
            // 
            this.capKontrast.Location = new System.Drawing.Point(280, 24);
            this.capKontrast.Name = "capKontrast";
            this.capKontrast.Size = new System.Drawing.Size(72, 24);
            // 
            // Tool_Kontrast
            // 
            this.Controls.Add(this.grpKontrast);
            this.Controls.Add(this.btnAusdünnen);
            this.Controls.Add(this.btnPixelHinzu);
            this.Controls.Add(this.btnAlleFarbenSchwarz);
            this.Controls.Add(this.btnGraustufen);
            this.Name = "Tool_Kontrast";
            this.Size = new System.Drawing.Size(360, 342);
            this.grpKontrast.ResumeLayout(false);
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
    }

}