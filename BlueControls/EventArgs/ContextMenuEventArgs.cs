// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;

namespace BlueControls.EventArgs;

public class ContextMenuEventArgs : AbstractListItemEventArgs {

    #region Constructors

    public ContextMenuEventArgs(AbstractListItem clickedComand, object? hotItem) : base(clickedComand) => HotItem = hotItem;

    #endregion

    #region Properties

    public object? HotItem { get; }

    #endregion
}