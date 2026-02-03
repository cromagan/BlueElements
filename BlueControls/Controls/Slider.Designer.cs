using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class Slider 
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
            this.But1 = new Button();
            this.But2 = new Button();
            this.SuspendLayout();
            //
            //But1
            //
            this.But1.ButtonStyle = ButtonStyle.SliderButton;
            this.But1.Checked = false;
            this.But1.ImageCode = "Pfeil_Oben_Scrollbar|8|||||0";
            this.But1.Location = new Point(56, 48);
            this.But1.Name = "But1";
            this.But1.Size = new Size(24, 24);
            this.But1.TabIndex = 0;
            But1.Click += new EventHandler(But1_Click);
            //
            //But2
            //
            this.But2.ButtonStyle = ButtonStyle.SliderButton;
            this.But2.Checked = false;
            this.But2.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
            this.But2.Location = new Point(24, 24);
            this.But2.Name = "But2";
            this.But2.Size = new Size(24, 24);
            this.But2.TabIndex = 0;
            But2.Click += new EventHandler(But2_Click);
            //
            //Slider
            //
            this.Controls.Add(this.But2);
            this.Controls.Add(this.But1);
            this.Name = "Slider";
            this.ResumeLayout(false);
        }
        internal Button But1;
        internal Button But2;
    }
}
