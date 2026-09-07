// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class NavigationDirectionEventArgs : System.EventArgs {

    #region Constructors

    public NavigationDirectionEventArgs(NavigationDirection direction) => Direction = direction;

    #endregion

    #region Properties

    public NavigationDirection Direction { get; }

    #endregion
}
