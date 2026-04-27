// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Classes;
using BlueTable.Interfaces;

namespace BlueTable.EventArgs;

public class TableFileGiveBackEventArgs : System.EventArgs, IHasTable {

    #region Properties

    public Table? Table { get; set; }

    #endregion
}