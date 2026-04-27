// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;

namespace BlueControls.EventArgs;

public class FilterCommandEventArgs : FilterEventArgs {

    #region Constructors

    // string Command, ColumnItem ThisColumn, FilterItem NewFilter
    public FilterCommandEventArgs(string command, ColumnItem column, FilterItem? newFilter) : base(newFilter) {
        Command = command;
        Column = column;
    }

    #endregion

    #region Properties

    public ColumnItem Column { get; }
    public string Command { get; }

    #endregion
}