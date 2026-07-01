// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

namespace BluePaint
{
    public partial class Tool_Paint : GenericTool
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Stift = new Button();
            this.grpSize = new GroupBox();
            this.capSize = new Caption();
            this.sldSize = new Slider();
            this.grpSize.SuspendLayout();
            this.SuspendLayout();
            //
            //Stift
            //
            this.Stift.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.Stift.Checked = true;
            this.Stift.Location = new Point(24, 64);
            this.Stift.Name = "Stift";
            this.Stift.Size = new Size(375, 56);
            this.Stift.TabIndex = 4;
            this.Stift.Text = "<b>Stift</b><br><i>Übermalen sie mit der Maus Bereiche mit schwarzer Farbe.";
            //
            //grpSize
            //
            this.grpSize.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                   | AnchorStyles.Right)));
            this.grpSize.Controls.Add(this.capSize);
            this.grpSize.Controls.Add(this.sldSize);
            this.grpSize.Location = new Point(16, 136);
            this.grpSize.Name = "grpSize";
            this.grpSize.Size = new Size(387, 64);
            this.grpSize.Text = "Größe";
            //
            //capSize
            //
            this.capSize.Location = new Point(300, 24);
            this.capSize.Name = "capSize";
            this.capSize.Size = new Size(72, 24);
            this.capSize.Text = "3";
            //
            //sldSize
            //
            this.sldSize.LargeChange = 5f;
            this.sldSize.Location = new Point(16, 24);
            this.sldSize.Maximum = 50f;
            this.sldSize.Minimum = 1f;
            this.sldSize.MouseChange = 1f;
            this.sldSize.Name = "sldSize";
            this.sldSize.Size = new Size(270, 24);
            this.sldSize.SmallChange = 1f;
            this.sldSize.Value = 3f;
            this.sldSize.ValueChanged += new EventHandler(this.sldSize_ValueChanged);
            //
            //Tool_Paint
            //
            this.Controls.Add(this.grpSize);
            this.Controls.Add(this.Stift);
            this.Name = "Tool_Paint";
            this.Size = new Size(419, 218);
            this.Name = "Zeichnen";
            this.grpSize.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        internal Button Stift;
        private GroupBox grpSize;
        private Caption capSize;
        private Slider sldSize;
    }
}