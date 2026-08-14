// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class ContextMenuEventArgs : ListItemEventArgs {

    #region Constructors

    public ContextMenuEventArgs(ListItem clickedCommand, object? hotItem) : base(clickedCommand) => HotItem = hotItem;

    #endregion

    #region Properties

    public object? HotItem { get; }

    #endregion
}