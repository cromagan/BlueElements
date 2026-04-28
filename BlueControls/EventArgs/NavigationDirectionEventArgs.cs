// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Enums;

namespace BlueControls.EventArgs;

public class NavigationDirectionEventArgs : System.EventArgs {

    #region Constructors

    public NavigationDirectionEventArgs(NavigationDirection direction) => Direction = direction;

    #endregion

    #region Properties

    public NavigationDirection Direction { get; }

    #endregion
}
