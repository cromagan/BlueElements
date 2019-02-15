using System.Collections.Generic;

namespace BlueDatabase.EventArgs
{
    public class RowCheckedEventArgs : RowEventArgs
    {
        public RowCheckedEventArgs(RowItem Row, List<string> ColumnsWithErrors) : base(Row)
        {
            this.ColumnsWithErrors = ColumnsWithErrors;
        }

       public List<string> ColumnsWithErrors { get; set; }

    }
}
