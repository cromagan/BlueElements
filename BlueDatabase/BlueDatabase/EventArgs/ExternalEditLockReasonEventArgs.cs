namespace BlueDatabase.EventArgs
{
    public class ExternalEditLockReasonEventArgs : CellEventArgs
    {


        public ExternalEditLockReasonEventArgs(ColumnItem Column, RowItem Row) : base(Column, Row) { }

        public string FeedbackReason { get; set; }

    }
}
