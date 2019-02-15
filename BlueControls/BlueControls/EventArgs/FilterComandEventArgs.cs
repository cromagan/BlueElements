using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.EventArgs
{
    public class FilterComandEventArgs : FilterEventArgs
    {
        // string Comand, ColumnItem ThisColumn, FilterItem NewFilter

        public FilterComandEventArgs(string Comand, ColumnItem Column, FilterItem NewFilter) : base(NewFilter)
        {
            this.Comand = Comand;
            this.Column = Column;
        }

        public string Comand { get; set; }
     public ColumnItem Column { get; set; }

    }
}
