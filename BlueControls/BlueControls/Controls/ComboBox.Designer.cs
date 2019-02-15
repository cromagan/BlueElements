using BlueControls.DialogBoxes;
using System;
using System.Diagnostics;
using System.Drawing;


namespace BlueControls.Controls
{
    public partial class ComboBox : TextBox
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
                _DropDownStyle = 0;
                _ImageCode = null;
                _DrawStyle = 0;



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
            this.BB = new  Button();
            this.SuspendLayout();
            // 
            // BB
            // 
            this.BB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                              | System.Windows.Forms.AnchorStyles.Right)));
            this.BB.ImageCode = "Pfeil_Unten_Scrollbar";
            this.BB.Location = new Point(476, 0);
            this.BB.Name = "BB";
            this.BB.Size = new Size(24, 150);
            this.BB.TabIndex = 1;
            this.BB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BB_MouseUp);
            this.BB.LostFocus += new EventHandler(this.BB_LostFocus);
            // 
            // ComboBox
            // 
            this.Controls.Add(this.BB);
            this.Name = "ComboBox";
            this.Size = new Size(500, 150);
            this.ResumeLayout(false);

        }

        internal Button BB;
    }
}
