using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Forms {
    public partial class UniqueValueDefinitionEditor {
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                }
            } finally {
                base.Dispose(disposing);
            }
        }
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            lbxKeyColumns = new ListBox();
            capKeyColumns = new Caption();
            SuspendLayout();
            // 
            // lbxKeyColumns
            // 
            lbxKeyColumns.AddAllowed = AddType.None;
            lbxKeyColumns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbxKeyColumns.Appearance = ListBoxAppearance.Listbox_Boxes;
            lbxKeyColumns.AutoSort = false;
            lbxKeyColumns.CheckBehavior = CheckBehavior.MultiSelection;
            lbxKeyColumns.FilterText = null;
            lbxKeyColumns.Location = new Point(0, 24);
            lbxKeyColumns.Name = "lbxKeyColumns";
            lbxKeyColumns.Size = new Size(352, 368);
            lbxKeyColumns.TabIndex = 8;
            lbxKeyColumns.ItemCheckedChanged += lbxKeyColumns_ItemCheckedChanged;
            // 
            // capKeyColumns
            // 
            capKeyColumns.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capKeyColumns.CausesValidation = false;
            capKeyColumns.Location = new Point(0, 0);
            capKeyColumns.Name = "capKeyColumns";
            capKeyColumns.Size = new Size(352, 24);
            capKeyColumns.Text = "<b><u>Einzigartige Spaltenkombination:</b><u>";
            // 
            // UniqueValueDefinitionEditor
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(lbxKeyColumns);
            Controls.Add(capKeyColumns);
            Name = "UniqueValueDefinitionEditor";
            Size = new Size(355, 392);
            ResumeLayout(false);

        }
        private ListBox lbxKeyColumns;
        private Caption capKeyColumns;
    }
}
