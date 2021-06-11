using BlueControls.Controls;
using BlueBasics.Enums;
namespace BluePaint
{
    public partial class Tool_Clipping : GenericTool
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
            this.ZuschnittOK = new Button();
            this.Unten = new Slider();
            this.Oben = new Slider();
            this.Recht = new Slider();
            this.PictureBox2 = new System.Windows.Forms.PictureBox();
            this.Links = new Slider();
            this.AutoZ = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // ZuschnittOK
            // 
            this.ZuschnittOK.ImageCode = "Häkchen|16";
            this.ZuschnittOK.Location = new System.Drawing.Point(168, 368);
            this.ZuschnittOK.Name = "ZuschnittOK";
            this.ZuschnittOK.Size = new System.Drawing.Size(112, 32);
            this.ZuschnittOK.TabIndex = 9;
            this.ZuschnittOK.Text = "übernehmen";
            this.ZuschnittOK.Click += new System.EventHandler(this.ZuschnittOK_Click);
            // 
            // Unten
            // 
            this.Unten.LargeChange = 1D;
            this.Unten.Location = new System.Drawing.Point(64, 256);
            this.Unten.Maximum = 0D;
            this.Unten.Minimum = -500D;
            this.Unten.Name = "Unten";
            this.Unten.Orientation = enOrientation.Senkrecht;
            this.Unten.Size = new System.Drawing.Size(144, 36);
            this.Unten.Text = "Slider1";
            this.Unten.ValueChanged += new System.EventHandler(this.ValueChangedByClicking);
            // 
            // Oben
            // 
            this.Oben.LargeChange = 1D;
            this.Oben.Location = new System.Drawing.Point(64, 56);
            this.Oben.Maximum = 500D;
            this.Oben.Name = "Oben";
            this.Oben.Orientation = enOrientation.Senkrecht;
            this.Oben.Size = new System.Drawing.Size(144, 36);
            this.Oben.Text = "Slider1";
            this.Oben.ValueChanged += new System.EventHandler(this.ValueChangedByClicking);
            // 
            // Recht
            // 
            this.Recht.LargeChange = 1D;
            this.Recht.Location = new System.Drawing.Point(216, 104);
            this.Recht.Maximum = 0D;
            this.Recht.Minimum = -500D;
            this.Recht.Name = "Recht";
            this.Recht.Size = new System.Drawing.Size(36, 144);
            this.Recht.Text = "Slider2";
            this.Recht.ValueChanged += new System.EventHandler(this.ValueChangedByClicking);
            // 
            // PictureBox2
            // 
            this.PictureBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.PictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBox2.Location = new System.Drawing.Point(64, 104);
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.Size = new System.Drawing.Size(144, 144);
            this.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox2.TabIndex = 7;
            this.PictureBox2.TabStop = false;
            // 
            // Links
            // 
            this.Links.LargeChange = 1D;
            this.Links.Location = new System.Drawing.Point(16, 104);
            this.Links.Maximum = 500D;
            this.Links.Name = "Links";
            this.Links.Size = new System.Drawing.Size(36, 144);
            this.Links.Text = "Slider1";
            this.Links.ValueChanged += new System.EventHandler(this.ValueChangedByClicking);
            // 
            // AutoZ
            // 
            this.AutoZ.ImageCode = "Schere|16";
            this.AutoZ.Location = new System.Drawing.Point(16, 312);
            this.AutoZ.Name = "AutoZ";
            this.AutoZ.Size = new System.Drawing.Size(200, 32);
            this.AutoZ.TabIndex = 10;
            this.AutoZ.Text = "Automtisch Zuschneiden";
            this.AutoZ.Click += new System.EventHandler(this.AutoZ_Click);
            // 
            // Tool_Clipping
            // 
            this.Controls.Add(this.AutoZ);
            this.Controls.Add(this.PictureBox2);
            this.Controls.Add(this.ZuschnittOK);
            this.Controls.Add(this.Unten);
            this.Controls.Add(this.Oben);
            this.Controls.Add(this.Recht);
            this.Controls.Add(this.Links);
            this.Name = "Tool_Clipping";
            this.Size = new System.Drawing.Size(297, 424);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).EndInit();
            this.ResumeLayout(false);
        }
        internal Button ZuschnittOK;
        internal Slider Unten;
        internal Slider Oben;
        internal Slider Recht;
        internal System.Windows.Forms.PictureBox PictureBox2;
        internal Slider Links;
        internal Button AutoZ;
    }
}