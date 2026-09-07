// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class ContextMenuEventArgs : ListItemEventArgs {

    #region Constructors

    public ContextMenuEventArgs(ListItem clickedCommand, object? hotItem) : base(clickedCommand) => HotItem = hotItem;

    #endregion

    #region Properties

    public object? HotItem { get; }

    #endregion
}