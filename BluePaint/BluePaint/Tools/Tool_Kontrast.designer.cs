using BlueBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using BlueControls.Controls;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

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
            this.SuspendLayout();
            // 
            // btnAlleFarbenSchwarz
            // 
            this.btnAlleFarbenSchwarz.ImageCode = "HoherKontrast";
            this.btnAlleFarbenSchwarz.Location = new System.Drawing.Point(16, 120);
            this.btnAlleFarbenSchwarz.Name = "btnAlleFarbenSchwarz";
            this.btnAlleFarbenSchwarz.Size = new System.Drawing.Size(324, 48);
            this.btnAlleFarbenSchwarz.TabIndex = 8;
            this.btnAlleFarbenSchwarz.Text = "Alle Farben zu Schwarz";
            this.btnAlleFarbenSchwarz.Click += new System.EventHandler(this.btnAlleFarbenSchwarz_Click);
            // 
            // btnKontrastErhoehen
            // 
            this.btnKontrastErhoehen.ImageCode = "KontrastErhöhen";
            this.btnKontrastErhoehen.Location = new System.Drawing.Point(16, 8);
            this.btnKontrastErhoehen.Name = "btnKontrastErhoehen";
            this.btnKontrastErhoehen.Size = new System.Drawing.Size(324, 48);
            this.btnKontrastErhoehen.TabIndex = 6;
            this.btnKontrastErhoehen.Text = "Kontrast erhöhen";
            this.btnKontrastErhoehen.Click += new System.EventHandler(this.btnKontrastErhoehen_Click);
            // 
            // btnGraustufen
            // 
            this.btnGraustufen.ImageCode = "Graustufen";
            this.btnGraustufen.Location = new System.Drawing.Point(16, 64);
            this.btnGraustufen.Name = "btnGraustufen";
            this.btnGraustufen.Size = new System.Drawing.Size(324, 48);
            this.btnGraustufen.TabIndex = 7;
            this.btnGraustufen.Text = "In Graustufen umwandeln";
            this.btnGraustufen.Click += new System.EventHandler(this.btnGraustufen_Click);
            // 
            // btnPixelHinzu
            // 
            this.btnPixelHinzu.ImageCode = "Gewicht|30";
            this.btnPixelHinzu.Location = new System.Drawing.Point(16, 176);
            this.btnPixelHinzu.Name = "btnPixelHinzu";
            this.btnPixelHinzu.Size = new System.Drawing.Size(324, 48);
            this.btnPixelHinzu.TabIndex = 9;
            this.btnPixelHinzu.Text = "Schwarze Pixel hinzufügen";
            this.btnPixelHinzu.Click += new System.EventHandler(this.btnPixelHinzu_Click);
            // 
            // Tool_Kontrast
            // 
            this.Controls.Add(this.btnPixelHinzu);
            this.Controls.Add(this.btnAlleFarbenSchwarz);
            this.Controls.Add(this.btnKontrastErhoehen);
            this.Controls.Add(this.btnGraustufen);
            this.Name = "Tool_Kontrast";
            this.Size = new System.Drawing.Size(399, 363);
            this.ResumeLayout(false);

        }

        internal Button btnAlleFarbenSchwarz;
        internal Button btnKontrastErhoehen;
        internal Button btnGraustufen;
        internal Button btnPixelHinzu;
    }

}