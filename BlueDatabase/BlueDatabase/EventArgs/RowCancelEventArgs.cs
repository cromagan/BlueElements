namespace BlueDatabase.EventArgs
{

    public class RowCancelEventArgs : RowEventArgs
    {
        
        public RowCancelEventArgs(RowItem Row, string CancelReason) : base(Row)
        {
            this.CancelReason = CancelReason;
        }

        public string CancelReason { get; set; }

    }

}
