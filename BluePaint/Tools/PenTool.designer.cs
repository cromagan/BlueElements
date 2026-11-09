// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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
    public partial class PenTool : Tool
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.grpFarbe = new GroupBox();
            this.optSchwarz = new Button();
            this.optBlau = new Button();
            this.optRot = new Button();
            this.optGruen = new Button();
            this.optWeiss = new Button();
            this.grpSize = new GroupBox();
            this.capSize = new Caption();
            this.sldSize = new Slider();
            this.grpFarbe.SuspendLayout();
            this.grpSize.SuspendLayout();
            this.SuspendLayout();
            //
            //grpFarbe
            //
            this.grpFarbe.Controls.Add(this.optSchwarz);
            this.grpFarbe.Controls.Add(this.optBlau);
            this.grpFarbe.Controls.Add(this.optRot);
            this.grpFarbe.Controls.Add(this.optGruen);
            this.grpFarbe.Controls.Add(this.optWeiss);
            this.grpFarbe.GroupBoxStyle = GroupBoxStyle.Nothing;
            this.grpFarbe.Location = new Point(16, 20);
            this.grpFarbe.Name = "grpFarbe";
            this.grpFarbe.Size = new Size(160, 40);
            this.grpFarbe.TabStop = false;
            //
            //optSchwarz
            //
            this.optSchwarz.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optSchwarz.Checked = true;
            this.optSchwarz.ImageCode = "Kreis|32||||000000";
            this.optSchwarz.Location = new Point(0, 4);
            this.optSchwarz.Name = "optSchwarz";
            this.optSchwarz.Size = new Size(32, 32);
            this.optSchwarz.TabIndex = 5;
            this.optSchwarz.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optBlau
            //
            this.optBlau.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optBlau.ImageCode = "Kreis|32||||0078D7";
            this.optBlau.Location = new Point(32, 4);
            this.optBlau.Name = "optBlau";
            this.optBlau.Size = new Size(32, 32);
            this.optBlau.TabIndex = 6;
            this.optBlau.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optRot
            //
            this.optRot.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optRot.ImageCode = "Kreis|32||||FF0000";
            this.optRot.Location = new Point(64, 4);
            this.optRot.Name = "optRot";
            this.optRot.Size = new Size(32, 32);
            this.optRot.TabIndex = 7;
            this.optRot.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optGruen
            //
            this.optGruen.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optGruen.ImageCode = "Kreis|32||||008000";
            this.optGruen.Location = new Point(96, 4);
            this.optGruen.Name = "optGruen";
            this.optGruen.Size = new Size(32, 32);
            this.optGruen.TabIndex = 8;
            this.optGruen.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optWeiss
            //
            this.optWeiss.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optWeiss.ImageCode = "Kreis|32||||FFFFFF";
            this.optWeiss.Location = new Point(128, 4);
            this.optWeiss.Name = "optWeiss";
            this.optWeiss.Size = new Size(32, 32);
            this.optWeiss.TabIndex = 9;
            this.optWeiss.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //grpSize
            //
            this.grpSize.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                   | AnchorStyles.Right)));
            this.grpSize.Controls.Add(this.capSize);
            this.grpSize.Controls.Add(this.sldSize);
            this.grpSize.Location = new Point(16, 76);
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
            //Paint
            //
            this.Controls.Add(this.grpSize);
            this.Controls.Add(this.grpFarbe);
            this.Name = "Paint";
            this.Size = new Size(419, 156);
            this.Name = "Zeichnen";
            this.grpFarbe.ResumeLayout(false);
            this.grpSize.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private GroupBox grpFarbe;
        internal Button optSchwarz;
        internal Button optBlau;
        internal Button optRot;
        internal Button optGruen;
        internal Button optWeiss;
        private GroupBox grpSize;
        private Caption capSize;
        private Slider sldSize;
    }
}
