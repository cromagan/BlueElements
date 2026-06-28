// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using Caption = BlueControls.Controls.Caption;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueTable.Editoren {
    public partial class FilterCollectionEditor {
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            capFilterItems = new Caption();
            lstFilterItems = new ListBox();
            grpFilterEditor = new GroupBox();
            filterItemEditor = new FilterItemEditor();
            grpFilterEditor.SuspendLayout();
            SuspendLayout();
            // 
            // capFilterItems
            // 
            capFilterItems.CausesValidation = false;
            capFilterItems.Location = new Point(8, 8);
            capFilterItems.Name = "capFilterItems";
            capFilterItems.Size = new Size(170, 18);
            capFilterItems.Text = "Filter:";
            // 
            // lstFilterItems
            // 
            lstFilterItems.AddAllowed = AddType.UserDef_NoText;
            lstFilterItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstFilterItems.FilterText = null;
            lstFilterItems.Location = new Point(8, 28);
            lstFilterItems.Name = "lstFilterItems";
            lstFilterItems.RemoveAllowed = true;
            lstFilterItems.Size = new Size(170, 244);
            lstFilterItems.TabIndex = 0;
            lstFilterItems.ItemClicked += lstFilterItems_ItemClicked;
            lstFilterItems.RemoveClicked += lstFilterItems_RemoveClicked;
            // 
            // grpFilterEditor
            // 
            grpFilterEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpFilterEditor.Controls.Add(filterItemEditor);
            grpFilterEditor.Location = new Point(186, 8);
            grpFilterEditor.Name = "grpFilterEditor";
            grpFilterEditor.Size = new Size(366, 264);
            grpFilterEditor.TabIndex = 1;
            grpFilterEditor.TabStop = false;
            grpFilterEditor.Text = "Filter bearbeiten";
            // 
            // filterItemEditor
            // 
            filterItemEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            filterItemEditor.Location = new Point(4, 20);
            filterItemEditor.Name = "filterItemEditor";
            filterItemEditor.Size = new Size(358, 240);
            filterItemEditor.TabIndex = 0;
            // 
            // FilterCollectionEditor
            // 
            Controls.Add(grpFilterEditor);
            Controls.Add(lstFilterItems);
            Controls.Add(capFilterItems);
            Name = "FilterCollectionEditor";
            Size = new Size(560, 280);
            grpFilterEditor.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Caption capFilterItems;
        private ListBox lstFilterItems;
        private GroupBox grpFilterEditor;
        private FilterItemEditor filterItemEditor;
    }
}
