// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class MoveEventArgs : System.EventArgs {

    #region Constructors

    public MoveEventArgs(bool byMouse) => ByMouse = byMouse;

    #endregion

    #region Properties

    public bool ByMouse { get; }

    #endregion
}