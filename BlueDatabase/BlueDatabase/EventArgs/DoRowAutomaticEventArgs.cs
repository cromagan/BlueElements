namespace BlueDatabase.EventArgs
{
    public class DoRowAutomaticEventArgs : RowEventArgs
    {

        public DoRowAutomaticEventArgs(RowItem Row) : base(Row)
        {
        }

        public string Feedback { get; set; }

        public ColumnItem FeedbackColumn { get; set; }

        public bool Done { get; set; }


    }
}
