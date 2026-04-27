// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList.TableItems;

namespace BlueControls.EventArgs;

public class CellExtEventArgs : System.EventArgs {

    #region Constructors

    public CellExtEventArgs(ColumnViewItem? column, RowListItem? row) {
        ColumnView = column;
        RowData = row;
    }

    #endregion

    #region Properties

    public ColumnViewItem? ColumnView { get; }
    public RowListItem? RowData { get; }

    #endregion
}