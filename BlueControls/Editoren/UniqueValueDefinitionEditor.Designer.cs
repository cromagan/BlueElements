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
            lbxKeyColumns.Location = new Point(8, 32);
            lbxKeyColumns.Name = "lbxKeyColumns";
            lbxKeyColumns.Size = new Size(336, 352);
            lbxKeyColumns.TabIndex = 8;
            lbxKeyColumns.ItemCheckedChanged += lbxKeyColumns_ItemCheckedChanged;
            // 
            // capKeyColumns
            // 
            capKeyColumns.CausesValidation = false;
            capKeyColumns.Location = new Point(8, 8);
            capKeyColumns.Name = "capKeyColumns";
            capKeyColumns.Size = new Size(160, 24);
            capKeyColumns.Text = "Unique-Spalten:";
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
