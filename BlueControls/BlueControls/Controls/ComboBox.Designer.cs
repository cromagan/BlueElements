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
            this.btnDropDown = new BlueControls.Controls.Button();
            this.btnEdit = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // btnDropDown
            // 
            this.btnDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDropDown.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
            this.btnDropDown.Location = new System.Drawing.Point(476, 0);
            this.btnDropDown.Name = "btnDropDown";
            this.btnDropDown.Size = new System.Drawing.Size(24, 150);
            this.btnDropDown.TabIndex = 1;
            this.btnDropDown.LostFocus += new System.EventHandler(this.btnDropDown_LostFocus);
            this.btnDropDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ShowMenu);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.ImageCode = "Stift|14";
            this.btnEdit.Location = new System.Drawing.Point(452, 0);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.QuickInfo = "Dahinter stehendes<br>Element bearbeiten";
            this.btnEdit.Size = new System.Drawing.Size(24, 150);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Visible = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            this.btnEdit.MouseLeave += new System.EventHandler(this.btnEdit_MouseLeave);
            // 
            // ComboBox
            // 
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDropDown);
            this.Name = "ComboBox";
            this.Size = new System.Drawing.Size(500, 150);
            this.ResumeLayout(false);

        }
        internal Button btnDropDown;
        internal Button btnEdit;
    }
}
