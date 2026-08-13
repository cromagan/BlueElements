// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Enums;
using BlueTable.EventArgs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls {
    public partial class TableView {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.btnEdit = new Button();
            this.SuspendLayout();
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnEdit.ImageCode = "Stift|14";
            this.btnEdit.Location = new Point(550, 338);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new Size(24, 24);
            this.btnEdit.TabIndex = 49;
            this.btnEdit.Visible = false;
            this.btnEdit.Click += new EventHandler(this.btnEdit_Click);
            // 
            // TableView
            // 
            this.Controls.Add(this.btnEdit);
            this.Name = "TableView";
            this.Size = new Size(599, 388);
            this.AutoCenter = false;
            this.ResumeLayout(false);

        }
        private Button btnEdit;
    }
}
