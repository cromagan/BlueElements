namespace BlueDatabase.EventArgs
{
    public class CellCancelEventArgs : CellEventArgs
    {


        public CellCancelEventArgs(ColumnItem Column, RowItem Row, string CancelReason) : base(Column, Row)
        {
            this.CancelReason = CancelReason;
        }

        public string CancelReason { get; set; }

    }
}
