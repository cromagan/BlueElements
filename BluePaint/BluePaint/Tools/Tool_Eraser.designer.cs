using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;

namespace BluePaint
{
    public partial class Tool_Eraser 
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
            this.Razi = new Button();
            this.DrawBox = new Button();
            this.Eleminate = new Button();
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
            // Tool_Eraser
            // 
            this.Controls.Add(this.Razi);
            this.Controls.Add(this.DrawBox);
            this.Controls.Add(this.Eleminate);
            this.Name = "Tool_Eraser";
            this.Size = new Size(419, 274);
            this.ResumeLayout(false);
        }
        internal Button Razi;
        internal Button DrawBox;
        internal Button Eleminate;
    }
}