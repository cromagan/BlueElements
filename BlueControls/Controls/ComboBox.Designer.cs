using System;
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
                ImageCode = null;
                DrawStyle = 0;
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
            this.btnDropDown = new Button();
            this.btnEdit = new Button();
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
            this.btnDropDown.MouseEnter += new EventHandler(this.btnDropDown_MouseEnter);
            this.btnDropDown.LostFocus += new EventHandler(this.btnDropDown_LostFocus);
            this.btnDropDown.MouseUp += new MouseEventHandler(this.ShowMenu);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                   | AnchorStyles.Right)));
            this.btnEdit.ImageCode = "Stift|14";
            this.btnEdit.Location = new Point(452, 0);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.QuickInfo = "Dahinter stehendes<br>Element bearbeiten";
            this.btnEdit.Size = new Size(24, 150);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Visible = false;
            this.btnEdit.Click += new EventHandler(this.btnEdit_Click);
            this.btnEdit.MouseLeave += new EventHandler(this.btnEdit_MouseLeave);
            // 
            // ComboBox
            // 
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDropDown);
            this.Name = "ComboBox";
            this.Size = new Size(500, 150);
            this.ResumeLayout(false);

        }
        internal Button btnDropDown;
        internal Button btnEdit;
    }
}
