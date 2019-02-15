namespace BlueDatabase.EventArgs
{
    public class RowEventArgs : System.EventArgs
    {
        public RowEventArgs(RowItem Row)
        {
            this.Row = Row;
        }

        public RowItem Row { get; set; }
    }
}
