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
            this.BCB = new ComboBox();
            this.BTB = new TextBox();
            this.BTS = new TextBoxSuggestions();
            this.btnEdit = new Button();
            this.SuspendLayout();
            // 
            // BCB
            // 
            this.BCB.Cursor = Cursors.IBeam;
            this.BCB.Location = new Point(48, 120);
            this.BCB.Name = "BCB";
            this.BCB.Size = new Size(128, 32);
            this.BCB.TabIndex = 8;
            this.BCB.Visible = false;
            this.BCB.EnterKey += new EventHandler(this.BB_EnterKey);
            this.BCB.EscKey += new EventHandler(this.BB_EscKey);
            this.BCB.TabKey += new EventHandler(this.BB_TabKey);
            this.BCB.LostFocus += new EventHandler(this.BB_LostFocus);
            // 
            // BTB
            // 
            this.BTB.Cursor = Cursors.IBeam;
            this.BTB.Location = new Point(48, 88);
            this.BTB.Name = "BTB";
            this.BTB.Size = new Size(128, 32);
            this.BTB.TabIndex = 7;
            this.BTB.Visible = false;
            this.BTB.EnterKey += new EventHandler(this.BB_EnterKey);
            this.BTB.EscKey += new EventHandler(this.BB_EscKey);
            this.BTB.TabKey += new EventHandler(this.BB_TabKey);
            this.BTB.LostFocus += new EventHandler(this.BB_LostFocus);
            // 
            // BTS
            // 
            this.BTS.Cursor = Cursors.IBeam;
            this.BTS.Location = new Point(48, 56);
            this.BTS.Name = "BTS";
            this.BTS.Size = new Size(128, 64);
            this.BTS.TabIndex = 9;
            this.BTS.Visible = false;
            this.BTS.EnterKey += new EventHandler(this.BB_EnterKey);
            this.BTS.EscKey += new EventHandler(this.BB_EscKey);
            this.BTS.TabKey += new EventHandler(this.BB_TabKey);
            this.BTS.LostFocus += new EventHandler(this.BB_LostFocus);
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
            this.Controls.Add(this.BCB);
            this.Controls.Add(this.BTS);
            this.Controls.Add(this.BTB);
            this.Name = "TableView";
            this.Size = new Size(599, 388);
            this.AutoCenter = false;
            this.ResumeLayout(false);

        }
        private ComboBox BCB;
        private TextBox BTB;
        private TextBoxSuggestions BTS;
        private Button btnEdit;
    }
}