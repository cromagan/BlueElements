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
    public partial class TextTool : Tool
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.grpText = new GroupBox();
            this.txbText = new BlueControls.Controls.TextBox();
            this.grpFarbe = new GroupBox();
            this.optSchwarz = new Button();
            this.optWeiss = new Button();
            this.optRot = new Button();
            this.optGelb = new Button();
            this.optGruen = new Button();
            this.optCyan = new Button();
            this.optBlau = new Button();
            this.grpSize = new GroupBox();
            this.capSize = new Caption();
            this.sldSize = new Slider();
            this.chkFett = new Button();
            this.chkKursiv = new Button();
            this.grpText.SuspendLayout();
            this.grpFarbe.SuspendLayout();
            this.grpSize.SuspendLayout();
            this.SuspendLayout();
            //
            //grpText
            //
            this.grpText.Controls.Add(this.txbText);
            this.grpText.GroupBoxStyle = GroupBoxStyle.Nothing;
            this.grpText.Location = new Point(8, 4);
            this.grpText.Name = "grpText";
            this.grpText.Size = new Size(252, 56);
            this.grpText.TabStop = false;
            //
            //txbText
            //
            this.txbText.Cursor = Cursors.IBeam;
            this.txbText.Location = new Point(8, 24);
            this.txbText.Name = "txbText";
            this.txbText.Size = new Size(236, 24);
            this.txbText.TabIndex = 0;
            this.txbText.TextChanged += new EventHandler(this.TxbText_TextChanged);
            //
            //grpFarbe
            //
            this.grpFarbe.Controls.Add(this.optSchwarz);
            this.grpFarbe.Controls.Add(this.optWeiss);
            this.grpFarbe.Controls.Add(this.optRot);
            this.grpFarbe.Controls.Add(this.optGelb);
            this.grpFarbe.Controls.Add(this.optGruen);
            this.grpFarbe.Controls.Add(this.optCyan);
            this.grpFarbe.Controls.Add(this.optBlau);
            this.grpFarbe.GroupBoxStyle = GroupBoxStyle.Nothing;
            this.grpFarbe.Location = new Point(8, 64);
            this.grpFarbe.Name = "grpFarbe";
            this.grpFarbe.Size = new Size(252, 48);
            this.grpFarbe.TabStop = false;
            //
            //optSchwarz
            //
            this.optSchwarz.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optSchwarz.Checked = true;
            this.optSchwarz.ImageCode = "Kreis|32||||000000";
            this.optSchwarz.Location = new Point(4, 4);
            this.optSchwarz.Name = "optSchwarz";
            this.optSchwarz.Size = new Size(32, 32);
            this.optSchwarz.TabIndex = 1;
            this.optSchwarz.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optWeiss
            //
            this.optWeiss.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optWeiss.ImageCode = "Kreis|32||||FFFFFF";
            this.optWeiss.Location = new Point(36, 4);
            this.optWeiss.Name = "optWeiss";
            this.optWeiss.Size = new Size(32, 32);
            this.optWeiss.TabIndex = 2;
            this.optWeiss.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optRot
            //
            this.optRot.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optRot.ImageCode = "Kreis|32||||FF0000";
            this.optRot.Location = new Point(68, 4);
            this.optRot.Name = "optRot";
            this.optRot.Size = new Size(32, 32);
            this.optRot.TabIndex = 3;
            this.optRot.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optGelb
            //
            this.optGelb.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optGelb.ImageCode = "Kreis|32||||FFE840";
            this.optGelb.Location = new Point(100, 4);
            this.optGelb.Name = "optGelb";
            this.optGelb.Size = new Size(32, 32);
            this.optGelb.TabIndex = 4;
            this.optGelb.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optGruen
            //
            this.optGruen.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optGruen.ImageCode = "Kreis|32||||8CD650";
            this.optGruen.Location = new Point(132, 4);
            this.optGruen.Name = "optGruen";
            this.optGruen.Size = new Size(32, 32);
            this.optGruen.TabIndex = 5;
            this.optGruen.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optCyan
            //
            this.optCyan.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optCyan.ImageCode = "Kreis|32||||50D8F0";
            this.optCyan.Location = new Point(164, 4);
            this.optCyan.Name = "optCyan";
            this.optCyan.Size = new Size(32, 32);
            this.optCyan.TabIndex = 6;
            this.optCyan.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //optBlau
            //
            this.optBlau.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.optBlau.ImageCode = "Kreis|32||||64A0FF";
            this.optBlau.Location = new Point(196, 4);
            this.optBlau.Name = "optBlau";
            this.optBlau.Size = new Size(32, 32);
            this.optBlau.TabIndex = 7;
            this.optBlau.CheckedChanged += new EventHandler(this.Farbe_CheckedChanged);
            //
            //grpSize
            //
            this.grpSize.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                 | AnchorStyles.Right)));
            this.grpSize.Controls.Add(this.capSize);
            this.grpSize.Controls.Add(this.sldSize);
            this.grpSize.Location = new Point(8, 116);
            this.grpSize.Name = "grpSize";
            this.grpSize.Size = new Size(252, 64);
            this.grpSize.Text = "Größe (Pixel)";
            //
            //capSize
            //
            this.capSize.Location = new Point(172, 24);
            this.capSize.Name = "capSize";
            this.capSize.Size = new Size(64, 24);
            this.capSize.Text = "32";
            //
            //sldSize
            //
            this.sldSize.LargeChange = 10f;
            this.sldSize.Location = new Point(8, 24);
            this.sldSize.Maximum = 200f;
            this.sldSize.Minimum = 8f;
            this.sldSize.MouseChange = 1f;
            this.sldSize.Name = "sldSize";
            this.sldSize.Size = new Size(156, 24);
            this.sldSize.SmallChange = 1f;
            this.sldSize.Value = 32f;
            this.sldSize.ValueChanged += new EventHandler(this.SldSize_ValueChanged);
            //
            //chkFett
            //
            this.chkFett.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkFett.Location = new Point(8, 184);
            this.chkFett.Name = "chkFett";
            this.chkFett.Size = new Size(100, 24);
            this.chkFett.TabIndex = 8;
            this.chkFett.Text = "Fett";
            this.chkFett.CheckedChanged += new EventHandler(this.Stil_CheckedChanged);
            //
            //chkKursiv
            //
            this.chkKursiv.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.chkKursiv.Location = new Point(112, 184);
            this.chkKursiv.Name = "chkKursiv";
            this.chkKursiv.Size = new Size(100, 24);
            this.chkKursiv.TabIndex = 9;
            this.chkKursiv.Text = "Kursiv";
            this.chkKursiv.CheckedChanged += new EventHandler(this.Stil_CheckedChanged);
            //
            //TextTool
            //
            this.Controls.Add(this.chkFett);
            this.Controls.Add(this.chkKursiv);
            this.Controls.Add(this.grpSize);
            this.Controls.Add(this.grpFarbe);
            this.Controls.Add(this.grpText);
            this.Name = "Text";
            this.Size = new Size(268, 216);
            this.grpText.ResumeLayout(false);
            this.grpFarbe.ResumeLayout(false);
            this.grpSize.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private GroupBox grpText;
        private BlueControls.Controls.TextBox txbText;
        private GroupBox grpFarbe;
        internal Button optSchwarz;
        internal Button optWeiss;
        internal Button optRot;
        internal Button optGelb;
        internal Button optGruen;
        internal Button optCyan;
        internal Button optBlau;
        private GroupBox grpSize;
        private Caption capSize;
        private Slider sldSize;
        private Button chkFett;
        private Button chkKursiv;
    }
}
