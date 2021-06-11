using BlueDatabase;
namespace BlueControls.Forms {
    public partial class EditBoxRow : DialogWithOkAndCancel {
        #region Konstruktor
        private EditBoxRow() : base() => InitializeComponent();
        private EditBoxRow(string TXT, RowItem row) : this() {
            formToEdit.Database = row.Database;
            formToEdit.ShowingRowKey = row.Key;
            Setup(TXT, formToEdit, formToEdit.MinimumSize.Width + 50, false, true);
        }
        #endregion
        public static void Show(string TXT, RowItem row, bool IsDialog) {
            EditBoxRow MB = new(TXT, row);
            if (IsDialog) {
                MB.ShowDialog();
            } else {
                MB.Show();
            }
        }
        protected override void SetValue(bool canceled) {
            // Nix zu tun
        }
    }
}
