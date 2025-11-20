using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using Orientation = BlueBasics.Enums.Orientation;

namespace BluePaint
{
    public partial class Tool_Clipping : GenericTool
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
            this.ZuschnittOK = new Button();
            this.Unten = new Slider();
            this.Oben = new Slider();
            this.Recht = new Slider();
            this.PictureBox2 = new PictureBox();
            this.Links = new Slider();
            this.btnAutoZ = new Button();
            ((ISupportInitialize)(this.PictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // ZuschnittOK
            // 
            this.ZuschnittOK.ImageCode = "Häkchen|16";
            this.ZuschnittOK.Location = new Point(168, 368);
            this.ZuschnittOK.Name = "ZuschnittOK";
            this.ZuschnittOK.Size = new Size(112, 32);
            this.ZuschnittOK.TabIndex = 9;
            this.ZuschnittOK.Text = "übernehmen";
            this.ZuschnittOK.Click += new EventHandler(this.ZuschnittOK_Click);
            // 
            // Unten
            // 
            this.Unten.LargeChange = 1f;
            this.Unten.Location = new Point(64, 256);
            this.Unten.Maximum = 0f;
            this.Unten.Minimum = -500f;
            this.Unten.Name = "Unten";
            this.Unten.Orientation = Orientation.Senkrecht;
            this.Unten.Size = new Size(144, 36);
            this.Unten.Text = "Slider1";
            this.Unten.ValueChanged += new EventHandler(this.ValueChangedByClicking);
            // 
            // Oben
            // 
            this.Oben.LargeChange = 1f;
            this.Oben.Location = new Point(64, 56);
            this.Oben.Maximum = 500f;
            this.Oben.Name = "Oben";
            this.Oben.Orientation = Orientation.Senkrecht;
            this.Oben.Size = new Size(144, 36);
            this.Oben.Text = "Slider1";
            this.Oben.ValueChanged += new EventHandler(this.ValueChangedByClicking);
            // 
            // Recht
            // 
            this.Recht.LargeChange = 1f;
            this.Recht.Location = new Point(216, 104);
            this.Recht.Maximum = 0f;
            this.Recht.Minimum = -500f;
            this.Recht.Name = "Recht";
            this.Recht.Size = new Size(36, 144);
            this.Recht.Text = "Slider2";
            this.Recht.ValueChanged += new EventHandler(this.ValueChangedByClicking);
            // 
            // PictureBox2
            // 
            this.PictureBox2.BackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.PictureBox2.BorderStyle = BorderStyle.FixedSingle;
            this.PictureBox2.Location = new Point(64, 104);
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.Size = new Size(144, 144);
            this.PictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            this.PictureBox2.TabIndex = 7;
            this.PictureBox2.TabStop = false;
            // 
            // Links
            // 
            this.Links.LargeChange = 1f;
            this.Links.Location = new Point(16, 104);
            this.Links.Maximum = 500f;
            this.Links.Name = "Links";
            this.Links.Size = new Size(36, 144);
            this.Links.Text = "Slider1";
            this.Links.ValueChanged += new EventHandler(this.ValueChangedByClicking);
            // 
            // btnAutoZ
            // 
            this.btnAutoZ.ImageCode = "Schere|16";
            this.btnAutoZ.Location = new Point(16, 312);
            this.btnAutoZ.Name = "btnAutoZ";
            this.btnAutoZ.Size = new Size(200, 32);
            this.btnAutoZ.TabIndex = 10;
            this.btnAutoZ.Text = "Automatisch zuschneiden";
            this.btnAutoZ.Click += new EventHandler(this.btnAutoZ_Click);
            // 
            // Tool_Clipping
            // 
            this.Controls.Add(this.btnAutoZ);
            this.Controls.Add(this.PictureBox2);
            this.Controls.Add(this.ZuschnittOK);
            this.Controls.Add(this.Unten);
            this.Controls.Add(this.Oben);
            this.Controls.Add(this.Recht);
            this.Controls.Add(this.Links);
            this.Name = "Tool_Clipping";
            this.Size = new Size(297, 424);
            ((ISupportInitialize)(this.PictureBox2)).EndInit();
            this.ResumeLayout(false);
        }
        internal Button ZuschnittOK;
        internal Slider Unten;
        internal Slider Oben;
        internal Slider Recht;
        internal PictureBox PictureBox2;
        internal Slider Links;
        internal Button btnAutoZ;
    }
}