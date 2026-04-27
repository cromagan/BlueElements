// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;

namespace BlueControls.EventArgs;

public class AbstractListItemEventArgs : System.EventArgs {

    #region Constructors

    public AbstractListItemEventArgs(AbstractListItem item) => Item = item;

    #endregion

    #region Properties

    public AbstractListItem Item { get; }

    #endregion
}