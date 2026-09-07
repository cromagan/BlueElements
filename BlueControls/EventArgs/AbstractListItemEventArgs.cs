// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class ListItemEventArgs : System.EventArgs {

    #region Constructors

    public ListItemEventArgs(ListItem item) => Item = item;

    #endregion

    #region Properties

    public ListItem Item { get; }

    #endregion
}