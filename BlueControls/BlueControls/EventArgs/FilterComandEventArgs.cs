using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.EventArgs
{
    public class FilterComandEventArgs : FilterEventArgs
    {
        // string Comand, ColumnItem ThisColumn, FilterItem NewFilter

        public FilterComandEventArgs(string comand, ColumnItem column, FilterItem newFilter) : base(newFilter)
        {
            Comand = comand;
            Column = column;
        }

        public string Comand { get; }
        public ColumnItem Column { get; }

    }
}
