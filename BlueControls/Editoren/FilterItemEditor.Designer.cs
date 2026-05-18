// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Diagnostics;
using BlueControls.Controls;
using BlueControls.EventArgs;

namespace BlueTable.Editoren
{
    public partial class FilterItemEditor
    {
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            lstColumns = new ListBox();
            capColumn = new Caption();
            cmbMethod = new ComboBox();
            cmbLogic = new ComboBox();
            chkIgnoreCase = new Button();
            txtSearchValue = new TextBoxSuggestions();
            capSearch = new Caption();
            SuspendLayout();
            // 
            // lstColumns
            // 
            lstColumns.AddAllowed = BlueControls.Enums.AddType.None;
            lstColumns.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lstColumns.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            lstColumns.AutoSort = false;
            lstColumns.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            lstColumns.Location = new Point(8, 24);
            lstColumns.Name = "lstColumns";
            lstColumns.RemoveAllowed = false;
            lstColumns.Size = new Size(150, 208);
            lstColumns.TabIndex = 0;
            // 
            // capColumn
            // 
            capColumn.CausesValidation = false;
            capColumn.Location = new Point(8, 8);
            capColumn.Name = "capColumn";
            capColumn.Size = new Size(100, 18);
            capColumn.Text = "Spalte:";
            // 
            // cmbMethod
            // 
            cmbMethod.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbMethod.Location = new Point(168, 24);
            cmbMethod.Name = "cmbMethod";
            cmbMethod.Size = new Size(176, 21);
            cmbMethod.TabIndex = 1;
            // 
            // cmbLogic
            // 
            cmbLogic.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmbLogic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLogic.Location = new Point(168, 55);
            cmbLogic.Name = "cmbLogic";
            cmbLogic.Size = new Size(176, 21);
            cmbLogic.TabIndex = 2;
            cmbLogic.Visible = false;
            // 
            // chkIgnoreCase
            // 
            chkIgnoreCase.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            chkIgnoreCase.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkIgnoreCase.Location = new Point(168, 85);
            chkIgnoreCase.Name = "chkIgnoreCase";
            chkIgnoreCase.Size = new Size(176, 20);
            chkIgnoreCase.TabIndex = 3;
            chkIgnoreCase.Text = "Groß/Klein egal";
            // 
            // txtSearchValue
            // 
            txtSearchValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtSearchValue.Location = new Point(168, 130);
            txtSearchValue.Name = "txtSearchValue";
            txtSearchValue.MultiLine = true;
            txtSearchValue.Size = new Size(176, 102);
            txtSearchValue.TabIndex = 4;
            txtSearchValue.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // capSearch
            // 
            capSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            capSearch.CausesValidation = false;
            capSearch.Location = new Point(168, 112);
            capSearch.Name = "capSearch";
            capSearch.Size = new Size(176, 18);
            capSearch.Text = "Suchwert:";
            // 
            // FilterItemEditor
            // 
            Controls.Add(capSearch);
            Controls.Add(txtSearchValue);
            Controls.Add(chkIgnoreCase);
            Controls.Add(cmbLogic);
            Controls.Add(cmbMethod);
            Controls.Add(capColumn);
            Controls.Add(lstColumns);
            Name = "FilterItemEditor";
            Size = new Size(350, 240);
            ResumeLayout(false);
        }

        internal ListBox lstColumns;
        internal Caption capColumn;
        internal ComboBox cmbMethod;
        internal ComboBox cmbLogic;
        internal Button chkIgnoreCase;
        internal TextBoxSuggestions txtSearchValue;
        internal Caption capSearch;
    }
}