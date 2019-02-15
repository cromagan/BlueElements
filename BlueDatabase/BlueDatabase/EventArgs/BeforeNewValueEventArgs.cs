

namespace BlueDatabase.EventArgs
{
    public class BeforeNewValueEventArgs : CellCancelEventArgs
    {
        //   ColumnItem Column, RowItem Row, Point MousePos, string NewVal, ref string CancelReason
        public BeforeNewValueEventArgs(ColumnItem Column, RowItem Row, string NewVal, string CancelReason) : base(Column, Row, CancelReason)
        {
            this.NewVal = NewVal;
        }

        public string NewVal { get; set; }
    



    }
}
