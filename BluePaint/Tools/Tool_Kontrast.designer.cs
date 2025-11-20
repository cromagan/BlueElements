using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BluePaint
{
    public partial class Tool_Kontrast 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
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
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.btnAlleFarbenSchwarz = new Button();
            this.btnKontrastErhoehen = new Button();
            this.btnGraustufen = new Button();
            this.btnPixelHinzu = new Button();
            this.btnAusdünnen = new Button();
            this.grpKontrast = new GroupBox();
            this.capKontrast = new Caption();
            this.sldKontrast = new Slider();
            this.grpGamma = new GroupBox();
            this.capGamma = new Caption();
            this.sldGamma = new Slider();
            this.btnGamma = new Button();
            this.grpHelligkeit = new GroupBox();
            this.capHelligkeit = new Caption();
            this.sldHelligkeit = new Slider();
            this.btnHelligkeit = new Button();
            this.grpKontrast.SuspendLayout();
            this.grpGamma.SuspendLayout();
            this.grpHelligkeit.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAlleFarbenSchwarz
            // 
            this.btnAlleFarbenSchwarz.ImageCode = "Kontrast";
            this.btnAlleFarbenSchwarz.Location = new Point(8, 328);
            this.btnAlleFarbenSchwarz.Name = "btnAlleFarbenSchwarz";
            this.btnAlleFarbenSchwarz.Size = new Size(336, 48);
            this.btnAlleFarbenSchwarz.TabIndex = 8;
            this.btnAlleFarbenSchwarz.Text = "Alle Farben zu Schwarz";
            this.btnAlleFarbenSchwarz.Click += new EventHandler(this.btnAlleFarbenSchwarz_Click);
            // 
            // btnKontrastErhoehen
            // 
            this.btnKontrastErhoehen.ImageCode = "Kontrast|24";
            this.btnKontrastErhoehen.Location = new Point(216, 48);
            this.btnKontrastErhoehen.Name = "btnKontrastErhoehen";
            this.btnKontrastErhoehen.Size = new Size(136, 32);
            this.btnKontrastErhoehen.TabIndex = 6;
            this.btnKontrastErhoehen.Text = "übernehmen";
            this.btnKontrastErhoehen.Click += new EventHandler(this.btnKontrastErhoehen_Click);
            // 
            // btnGraustufen
            // 
            this.btnGraustufen.ImageCode = "Graustufen";
            this.btnGraustufen.Location = new Point(8, 272);
            this.btnGraustufen.Name = "btnGraustufen";
            this.btnGraustufen.Size = new Size(336, 48);
            this.btnGraustufen.TabIndex = 7;
            this.btnGraustufen.Text = "In Graustufen umwandeln";
            this.btnGraustufen.Click += new EventHandler(this.btnGraustufen_Click);
            // 
            // btnPixelHinzu
            // 
            this.btnPixelHinzu.ImageCode = "Gewicht|30";
            this.btnPixelHinzu.Location = new Point(8, 384);
            this.btnPixelHinzu.Name = "btnPixelHinzu";
            this.btnPixelHinzu.Size = new Size(336, 48);
            this.btnPixelHinzu.TabIndex = 9;
            this.btnPixelHinzu.Text = "Schwarze Pixel hinzufügen";
            this.btnPixelHinzu.Click += new EventHandler(this.btnPixelHinzu_Click);
            // 
            // btnAusdünnen
            // 
            this.btnAusdünnen.ImageCode = "Feder|30";
            this.btnAusdünnen.Location = new Point(8, 440);
            this.btnAusdünnen.Name = "btnAusdünnen";
            this.btnAusdünnen.Size = new Size(336, 48);
            this.btnAusdünnen.TabIndex = 10;
            this.btnAusdünnen.Text = "Schwarze Pixel entfernen";
            this.btnAusdünnen.Click += new EventHandler(this.btnAusdünnen_Click);
            // 
            // grpKontrast
            // 
            this.grpKontrast.CausesValidation = false;
            this.grpKontrast.Controls.Add(this.capKontrast);
            this.grpKontrast.Controls.Add(this.sldKontrast);
            this.grpKontrast.Controls.Add(this.btnKontrastErhoehen);
            this.grpKontrast.Dock = DockStyle.Top;
            this.grpKontrast.Location = new Point(0, 0);
            this.grpKontrast.Name = "grpKontrast";
            this.grpKontrast.Size = new Size(360, 88);
            this.grpKontrast.Text = "Kontrast";
            // 
            // capKontrast
            // 
            this.capKontrast.Location = new Point(280, 16);
            this.capKontrast.Name = "capKontrast";
            this.capKontrast.Size = new Size(72, 24);
            // 
            // sldKontrast
            // 
            this.sldKontrast.Location = new Point(16, 16);
            this.sldKontrast.Minimum = -100f;
            this.sldKontrast.Name = "sldKontrast";
            this.sldKontrast.Size = new Size(248, 24);
            this.sldKontrast.ValueChanged += new EventHandler(this.sldKontrast_ValueChanged);
            // 
            // grpGamma
            // 
            this.grpGamma.CausesValidation = false;
            this.grpGamma.Controls.Add(this.capGamma);
            this.grpGamma.Controls.Add(this.sldGamma);
            this.grpGamma.Controls.Add(this.btnGamma);
            this.grpGamma.Dock = DockStyle.Top;
            this.grpGamma.Location = new Point(0, 88);
            this.grpGamma.Name = "grpGamma";
            this.grpGamma.Size = new Size(360, 88);
            this.grpGamma.Text = "Gamma";
            // 
            // capGamma
            // 
            this.capGamma.Location = new Point(280, 16);
            this.capGamma.Name = "capGamma";
            this.capGamma.Size = new Size(72, 24);
            // 
            // sldGamma
            // 
            this.sldGamma.LargeChange = 0.01f;
            this.sldGamma.Location = new Point(16, 16);
            this.sldGamma.Maximum = 2.2f;
            this.sldGamma.MouseChange = 0.01f;
            this.sldGamma.Name = "sldGamma";
            this.sldGamma.Size = new Size(248, 24);
            this.sldGamma.SmallChange = 0.01f;
            this.sldGamma.Value = 1f;
            this.sldGamma.ValueChanged += new EventHandler(this.sldGamma_ValueChanged);
            // 
            // btnGamma
            // 
            this.btnGamma.ImageCode = "Kontrast|24";
            this.btnGamma.Location = new Point(208, 48);
            this.btnGamma.Name = "btnGamma";
            this.btnGamma.Size = new Size(136, 32);
            this.btnGamma.TabIndex = 6;
            this.btnGamma.Text = "übernehmen";
            this.btnGamma.Click += new EventHandler(this.btnGamma_Click);
            // 
            // grpHelligkeit
            // 
            this.grpHelligkeit.CausesValidation = false;
            this.grpHelligkeit.Controls.Add(this.capHelligkeit);
            this.grpHelligkeit.Controls.Add(this.sldHelligkeit);
            this.grpHelligkeit.Controls.Add(this.btnHelligkeit);
            this.grpHelligkeit.Dock = DockStyle.Top;
            this.grpHelligkeit.Location = new Point(0, 176);
            this.grpHelligkeit.Name = "grpHelligkeit";
            this.grpHelligkeit.Size = new Size(360, 88);
            this.grpHelligkeit.Text = "Helligkeit";
            // 
            // capHelligkeit
            // 
            this.capHelligkeit.Location = new Point(280, 16);
            this.capHelligkeit.Name = "capHelligkeit";
            this.capHelligkeit.Size = new Size(72, 24);
            // 
            // sldHelligkeit
            // 
            this.sldHelligkeit.LargeChange = 0.01f;
            this.sldHelligkeit.Location = new Point(16, 16);
            this.sldHelligkeit.Maximum = 2f;
            this.sldHelligkeit.Minimum = 0f;
            this.sldHelligkeit.MouseChange = 0.01f;
            this.sldHelligkeit.Name = "sldHelligkeit";
            this.sldHelligkeit.Size = new Size(248, 24);
            this.sldHelligkeit.SmallChange = 0.01f;
            this.sldHelligkeit.Value = 1f;
            this.sldHelligkeit.ValueChanged += new EventHandler(this.sldHelligkeit_ValueChanged);
            // 
            // btnHelligkeit
            // 
            this.btnHelligkeit.ImageCode = "Kontrast|24";
            this.btnHelligkeit.Location = new Point(216, 48);
            this.btnHelligkeit.Name = "btnHelligkeit";
            this.btnHelligkeit.Size = new Size(136, 32);
            this.btnHelligkeit.TabIndex = 6;
            this.btnHelligkeit.Text = "übernehmen";
            this.btnHelligkeit.Click += new EventHandler(this.btnHelligkeit_Click);
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
            this.Size = new Size(360, 505);
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