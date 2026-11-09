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
    public partial class TextmarkerTool : Tool
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.optGelb = new Button();
            this.optGruen = new Button();
            this.optCyan = new Button();
            this.optBlau = new Button();
            this.optRot = new Button();
            this.optMagenta = new Button();
            this.optOrange = new Button();
            this.optViolett = new Button();
            this.optBraun = new Button();
            this.optSchwarz = new Button();
            this.optWeiss = new Button();
            this.grpSize = new GroupBox();
            this.capSize = new Caption();
            this.sldSize = new Slider();
            this.grpSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // optGelb
            // 
            this.optGelb.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optGelb.Checked = true;
            this.optGelb.ImageCode = "Kreis|32||||FFE840";
            this.optGelb.Location = new Point(16, 24);
            this.optGelb.Name = "optGelb";
            this.optGelb.Size = new Size(32, 32);
            this.optGelb.TabIndex = 3;
            this.optGelb.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optGruen
            // 
            this.optGruen.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optGruen.ImageCode = "Kreis|32||||8CD650";
            this.optGruen.Location = new Point(48, 24);
            this.optGruen.Name = "optGruen";
            this.optGruen.Size = new Size(32, 32);
            this.optGruen.TabIndex = 4;
            this.optGruen.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optCyan
            // 
            this.optCyan.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optCyan.ImageCode = "Kreis|32||||50D8F0";
            this.optCyan.Location = new Point(80, 24);
            this.optCyan.Name = "optCyan";
            this.optCyan.Size = new Size(32, 32);
            this.optCyan.TabIndex = 5;
            this.optCyan.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optBlau
            // 
            this.optBlau.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optBlau.ImageCode = "Kreis|32||||64A0FF";
            this.optBlau.Location = new Point(112, 24);
            this.optBlau.Name = "optBlau";
            this.optBlau.Size = new Size(32, 32);
            this.optBlau.TabIndex = 6;
            this.optBlau.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optRot
            // 
            this.optRot.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optRot.ImageCode = "Kreis|32||||FF6464";
            this.optRot.Location = new Point(144, 24);
            this.optRot.Name = "optRot";
            this.optRot.Size = new Size(32, 32);
            this.optRot.TabIndex = 7;
            this.optRot.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optMagenta
            // 
            this.optMagenta.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optMagenta.ImageCode = "Kreis|32||||F064F0";
            this.optMagenta.Location = new Point(176, 24);
            this.optMagenta.Name = "optMagenta";
            this.optMagenta.Size = new Size(32, 32);
            this.optMagenta.TabIndex = 8;
            this.optMagenta.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optOrange
            // 
            this.optOrange.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optOrange.ImageCode = "Kreis|32||||FFAA3C";
            this.optOrange.Location = new Point(208, 24);
            this.optOrange.Name = "optOrange";
            this.optOrange.Size = new Size(32, 32);
            this.optOrange.TabIndex = 9;
            this.optOrange.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optViolett
            // 
            this.optViolett.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optViolett.ImageCode = "Kreis|32||||AA6EFF";
            this.optViolett.Location = new Point(240, 24);
            this.optViolett.Name = "optViolett";
            this.optViolett.Size = new Size(32, 32);
            this.optViolett.TabIndex = 10;
            this.optViolett.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optBraun
            // 
            this.optBraun.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optBraun.ImageCode = "Kreis|32||||B48250";
            this.optBraun.Location = new Point(272, 24);
            this.optBraun.Name = "optBraun";
            this.optBraun.Size = new Size(32, 32);
            this.optBraun.TabIndex = 11;
            this.optBraun.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optSchwarz
            // 
            this.optSchwarz.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optSchwarz.ImageCode = "Kreis|32||||3C3C3C";
            this.optSchwarz.Location = new Point(304, 24);
            this.optSchwarz.Name = "optSchwarz";
            this.optSchwarz.Size = new Size(32, 32);
            this.optSchwarz.TabIndex = 12;
            this.optSchwarz.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // optWeiss
            // 
            this.optWeiss.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optWeiss.ImageCode = "Kreis|32||||FFFFFF";
            this.optWeiss.Location = new Point(336, 24);
            this.optWeiss.Name = "optWeiss";
            this.optWeiss.Size = new Size(32, 32);
            this.optWeiss.TabIndex = 13;
            this.optWeiss.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            // 
            // grpSize
            // 
            this.grpSize.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
            this.grpSize.Controls.Add(this.capSize);
            this.grpSize.Controls.Add(this.sldSize);
            this.grpSize.Location = new Point(16, 72);
            this.grpSize.Name = "grpSize";
            this.grpSize.Size = new Size(387, 64);
            this.grpSize.Text = "Strichstärke";
            // 
            // capSize
            // 
            this.capSize.Location = new Point(300, 24);
            this.capSize.Name = "capSize";
            this.capSize.Size = new Size(72, 24);
            this.capSize.Text = "24";
            // 
            // sldSize
            // 
            this.sldSize.LargeChange = 5f;
            this.sldSize.Location = new Point(16, 24);
            this.sldSize.Maximum = 60f;
            this.sldSize.Minimum = 4f;
            this.sldSize.MouseChange = 1f;
            this.sldSize.Name = "sldSize";
            this.sldSize.Size = new Size(270, 24);
            this.sldSize.SmallChange = 1f;
            this.sldSize.Value = 24f;
            this.sldSize.ValueChanged += new EventHandler(this.sldSize_ValueChanged);
            // 
            // Textmarker
            // 
            this.Controls.Add(this.grpSize);
            this.Controls.Add(this.optGelb);
            this.Controls.Add(this.optGruen);
            this.Controls.Add(this.optCyan);
            this.Controls.Add(this.optBlau);
            this.Controls.Add(this.optRot);
            this.Controls.Add(this.optMagenta);
            this.Controls.Add(this.optOrange);
            this.Controls.Add(this.optViolett);
            this.Controls.Add(this.optBraun);
            this.Controls.Add(this.optSchwarz);
            this.Controls.Add(this.optWeiss);
            this.Name = "Textmarker";
            this.Size = new Size(419, 152);
            this.grpSize.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        internal Button optGelb;
        internal Button optGruen;
        internal Button optCyan;
        internal Button optBlau;
        internal Button optRot;
        internal Button optMagenta;
        internal Button optOrange;
        internal Button optViolett;
        internal Button optBraun;
        internal Button optSchwarz;
        internal Button optWeiss;
        private GroupBox grpSize;
        private Caption capSize;
        private Slider sldSize;
    }
}
