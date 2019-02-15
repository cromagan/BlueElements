using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;

namespace BlueControls.Controls
{
    public partial class ListBox : GenericControl
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
            this.SliderY = new  Slider();
            this.Down = new  Button();
            this.Up = new  Button();
            this.Plus = new  Button();
            this.Minus = new  Button();
            this.FilterTxt = new  TextBox();
            this.FilterCap = new  Caption();
            this.SuspendLayout();
            // 
            // SliderY
            // 
            this.SliderY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                   | System.Windows.Forms.AnchorStyles.Right)));
            this.SliderY.CausesValidation = false;
            this.SliderY.Location = new Point(159, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = enOrientation.Senkrecht;
            this.SliderY.Size = new Size(18, 144);
            this.SliderY.Visible = false;
            this.SliderY.ValueChanged += new EventHandler(this.SliderY_ValueChange);
            // 
            // Down
            // 
            this.Down.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Down.ImageCode = "Pfeil_Oben|16||4|00FF00";
            this.Down.Location = new Point(24, 144);
            this.Down.Name = "Down";
            this.Down.Size = new Size(24, 24);
            this.Down.TabIndex = 51;
            this.Down.Click += new EventHandler(this.Down_Click);
            // 
            // Up
            // 
            this.Up.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Up.ImageCode = "Pfeil_Oben|16|||00FF00";
            this.Up.Location = new Point(0, 144);
            this.Up.Name = "Up";
            this.Up.Size = new Size(24, 24);
            this.Up.TabIndex = 50;
            this.Up.Click += new EventHandler(this.Up_Click);
            // 
            // Plus
            // 
            this.Plus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Plus.ImageCode = "PlusZeichen|16";
            this.Plus.Location = new Point(152, 144);
            this.Plus.Name = "Plus";
            this.Plus.Size = new Size(24, 24);
            this.Plus.TabIndex = 48;
            this.Plus.Click += new EventHandler(this.Plus_Click);
            // 
            // Minus
            // 
            this.Minus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Minus.ImageCode = "MinusZeichen|16";
            this.Minus.Location = new Point(128, 144);
            this.Minus.Name = "Minus";
            this.Minus.Size = new Size(24, 24);
            this.Minus.TabIndex = 47;
            this.Minus.Click += new EventHandler(this.Minus_Click);
            // 
            // FilterTxt
            // 
            this.FilterTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterTxt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.FilterTxt.Location = new Point(40, 144);
            this.FilterTxt.Name = "FilterTxt";
            this.FilterTxt.Size = new Size(80, 24);
            this.FilterTxt.TabIndex = 52;
            this.FilterTxt.Visible = false;
            this.FilterTxt.TextChanged += new EventHandler(this.FilterTxt_TextChanged);
            // 
            // FilterCap
            // 
            this.FilterCap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FilterCap.CausesValidation = false;
            this.FilterCap.Location = new Point(0, 144);
            this.FilterCap.Name = "FilterCap";
            this.FilterCap.Size = new Size(40, 24);
            this.FilterCap.Text = "Filter:";
            this.FilterCap.Visible = false;
            // 
            // ListBox
            // 
            this.Controls.Add(this.FilterCap);
            this.Controls.Add(this.FilterTxt);
            this.Controls.Add(this.Plus);
            this.Controls.Add(this.Minus);
            this.Controls.Add(this.Down);
            this.Controls.Add(this.Up);
            this.Controls.Add(this.SliderY);
            this.Name = "ListBox";
            this.Size = new Size(177, 168);
            this.ResumeLayout(false);

        }

        private Button Down;
        private Button Up;
        private Button Plus;
        private Button Minus;
        private Slider SliderY;
        internal TextBox FilterTxt;
        internal Caption FilterCap;
    }
}
