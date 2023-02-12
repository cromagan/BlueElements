﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Forms;

namespace BlueControls.Controls
{
    public partial class ComboBox 
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
                FloatingInputBoxListBoxStyle.Close(this);
                _dropDownStyle = 0;
                _imageCode = null;
                _drawStyle = 0;
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
            this.btnDropDown = new  Button();
            this.SuspendLayout();
            // 
            // btnDropDown
            // 
            this.btnDropDown.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                       | AnchorStyles.Right)));
            this.btnDropDown.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
            this.btnDropDown.Location = new Point(476, 0);
            this.btnDropDown.Name = "btnDropDown";
            this.btnDropDown.Size = new Size(24, 150);
            this.btnDropDown.TabIndex = 1;
            this.btnDropDown.MouseUp += new MouseEventHandler(this.ShowMenu);
            this.btnDropDown.LostFocus += new EventHandler(this.btnDropDown_LostFocus);
            // 
            // ComboBox
            // 
            this.Controls.Add(this.btnDropDown);
            this.Name = "ComboBox";
            this.Size = new Size(500, 150);
            this.ResumeLayout(false);
        }
        internal Button btnDropDown;
    }
}
