namespace BlueDatabase.EventArgs
{
   public class ColumnEventArgs : System.EventArgs
    {

        public ColumnEventArgs(ColumnItem column)
        {
            Column = column;
        }

       public ColumnItem Column { get; set; }
    }
}
