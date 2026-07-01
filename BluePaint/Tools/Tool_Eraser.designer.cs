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
    public partial class Tool_Eraser 
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Razi = new Button();
            this.DrawBox = new Button();
            this.Eleminate = new Button();
            this.grpSize = new GroupBox();
            this.capSize = new Caption();
            this.sldSize = new Slider();
            this.grpSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // Razi
            // 
            this.Razi.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
            this.Razi.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.Razi.Location = new Point(32, 136);
            this.Razi.Name = "Razi";
            this.Razi.Size = new Size(336, 56);
            this.Razi.TabIndex = 5;
            this.Razi.Text = "<b>Radiergummi</b><br><i>Übermalen sie mit der Maus Bereiche mit weißer Farbe.";
            this.Razi.CheckedChanged += new EventHandler(this.DrawBox_CheckedChanged);
            // 
            // DrawBox
            // 
            this.DrawBox.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                   | AnchorStyles.Right)));
            this.DrawBox.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.DrawBox.Checked = true;
            this.DrawBox.Location = new Point(32, 96);
            this.DrawBox.Name = "DrawBox";
            this.DrawBox.Size = new Size(336, 40);
            this.DrawBox.TabIndex = 4;
            this.DrawBox.Text = "<b>Bereich löschen</b><br><i>Ziehen sie einen Rahmen, dieser Bereich wird weiß.";
            this.DrawBox.CheckedChanged += new EventHandler(this.DrawBox_CheckedChanged);
            // 
            // Eleminate
            // 
            this.Eleminate.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                     | AnchorStyles.Right)));
            this.Eleminate.ButtonStyle = ButtonStyle.Optionbox_Text;
            this.Eleminate.Location = new Point(32, 56);
            this.Eleminate.Name = "Eleminate";
            this.Eleminate.Size = new Size(336, 40);
            this.Eleminate.TabIndex = 3;
            this.Eleminate.Text = "<b>Farbe entfernen</b><br><i>Klicken sie auf die Farbe, die zu weiß werden soll.";
            this.Eleminate.CheckedChanged += new EventHandler(this.DrawBox_CheckedChanged);
            // 
            // grpSize
            // 
            this.grpSize.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                   | AnchorStyles.Right)));
            this.grpSize.Controls.Add(this.capSize);
            this.grpSize.Controls.Add(this.sldSize);
            this.grpSize.Location = new Point(16, 208);
            this.grpSize.Name = "grpSize";
            this.grpSize.Size = new Size(387, 64);
            this.grpSize.Text = "Größe";
            // 
            // capSize
            // 
            this.capSize.Location = new Point(300, 24);
            this.capSize.Name = "capSize";
            this.capSize.Size = new Size(72, 24);
            this.capSize.Text = "4";
            // 
            // sldSize
            // 
            this.sldSize.LargeChange = 5f;
            this.sldSize.Location = new Point(16, 24);
            this.sldSize.Maximum = 50f;
            this.sldSize.Minimum = 1f;
            this.sldSize.MouseChange = 1f;
            this.sldSize.Name = "sldSize";
            this.sldSize.Size = new Size(270, 24);
            this.sldSize.SmallChange = 1f;
            this.sldSize.Value = 4f;
            this.sldSize.ValueChanged += new EventHandler(this.sldSize_ValueChanged);
            // 
            // Tool_Eraser
            // 
            this.Controls.Add(this.grpSize);
            this.Controls.Add(this.Razi);
            this.Controls.Add(this.DrawBox);
            this.Controls.Add(this.Eleminate);
            this.Name = "Tool_Eraser";
            this.Size = new Size(419, 290);
            this.grpSize.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        internal Button Razi;
        internal Button DrawBox;
        internal Button Eleminate;
        private GroupBox grpSize;
        private Caption capSize;
        private Slider sldSize;
    }
}