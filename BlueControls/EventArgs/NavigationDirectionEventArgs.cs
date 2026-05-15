// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class NavigationDirectionEventArgs : System.EventArgs {

    #region Constructors

    public NavigationDirectionEventArgs(NavigationDirection direction) => Direction = direction;

    #endregion

    #region Properties

    public NavigationDirection Direction { get; }

    #endregion
}
