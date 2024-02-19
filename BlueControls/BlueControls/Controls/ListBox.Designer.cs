using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls
{
    public partial class ListBox 
    {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.SliderY = new BlueControls.Controls.Slider();
            this.btnDown = new BlueControls.Controls.Button();
            this.btnUp = new BlueControls.Controls.Button();
            this.btnPlus = new BlueControls.Controls.Button();
            this.btnMinus = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // SliderY
            // 
            this.SliderY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SliderY.CausesValidation = false;
            this.SliderY.Location = new System.Drawing.Point(159, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = BlueBasics.Enums.Orientation.Senkrecht;
            this.SliderY.Size = new System.Drawing.Size(18, 144);
            this.SliderY.Visible = false;
            this.SliderY.ValueChanged += new System.EventHandler(this.SliderY_ValueChange);
            // 
            // btnDown
            // 
            this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDown.ImageCode = "Pfeil_Unten_Scrollbar|8";
            this.btnDown.Location = new System.Drawing.Point(24, 144);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(24, 24);
            this.btnDown.TabIndex = 51;
            this.btnDown.Visible = false;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUp.ImageCode = "Pfeil_Oben_Scrollbar|8";
            this.btnUp.Location = new System.Drawing.Point(0, 144);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(24, 24);
            this.btnUp.TabIndex = 50;
            this.btnUp.Visible = false;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnPlus
            // 
            this.btnPlus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlus.ImageCode = "PlusZeichen|14";
            this.btnPlus.Location = new System.Drawing.Point(152, 144);
            this.btnPlus.Name = "btnPlus";
            this.btnPlus.Size = new System.Drawing.Size(64, 24);
            this.btnPlus.Text = "hinzu";
            this.btnPlus.TabIndex = 48;
            this.btnPlus.Visible = false;
            this.btnPlus.QuickInfo = "Neuen Eintrag hinzufügen";
            this.btnPlus.Click += new System.EventHandler(this.btnPlus_Click);
            // 
            // btnMinus
            // 
            this.btnMinus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinus.ImageCode = "MinusZeichen|14";
            this.btnMinus.Location = new System.Drawing.Point(128, 144);
            this.btnMinus.Name = "btnMinus";
            this.btnMinus.Size = new System.Drawing.Size(24, 24);
            this.btnMinus.TabIndex = 47;
            this.btnMinus.Visible = false;
            this.btnMinus.Click += new System.EventHandler(this.btnMinus_Click);
            // 
            // ListBox
            // 
            this.Controls.Add(this.btnPlus);
            this.Controls.Add(this.btnMinus);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.SliderY);
            this.Size = new System.Drawing.Size(177, 168);
            this.ResumeLayout(false);

        }
        private Button btnDown;
        private Button btnUp;
        private Button btnPlus;
        private Button btnMinus;
        private Slider SliderY;
    }
}
