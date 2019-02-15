using BlueDatabase;

namespace BlueControls.DialogBoxes
{
    public partial class EditBoxRow : DialogBoxes.DialogWithOkAndCancel
    {


        public EditBoxRow()
        {
            InitializeComponent();
        }

        public EditBoxRow(string TXT, RowItem row)
        {
            InitializeComponent();
            formToEdit.Database = row.Database;

            formToEdit.ShowingRowKey = row.Key;
            Setup(TXT, formToEdit, formToEdit.MinimumSize.Width + 50, false, true);
        }


        public static void Show(string TXT, RowItem row, bool IsDialog)
        {
            var MB = new EditBoxRow(TXT, row);

            if (IsDialog)
            {
                MB.ShowDialog();
            }
            else
            {
                MB.Show();
            }



        }





        protected override void SetValue()
        {
            // Nix zu tun
        }
    }
}
