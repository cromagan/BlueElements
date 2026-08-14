// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class ListItemEventArgs : System.EventArgs {

    #region Constructors

    public ListItemEventArgs(ListItem item) => Item = item;

    #endregion

    #region Properties

    public ListItem Item { get; }

    #endregion
}