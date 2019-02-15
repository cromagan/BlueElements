namespace BlueDatabase.EventArgs
{
    public class CellEventArgs : System.EventArgs
    {


        public CellEventArgs(ColumnItem Column, RowItem Row)
        {
            this.Column = Column;
            this.Row = Row;

        }

        public ColumnItem Column { get; set; }
        public RowItem Row { get; set; }
    }
}
