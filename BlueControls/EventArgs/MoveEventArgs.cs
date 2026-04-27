// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class MoveEventArgs : System.EventArgs {

    #region Constructors

    public MoveEventArgs(bool byMouse) => ByMouse = byMouse;

    #endregion

    #region Properties

    public bool ByMouse { get; }

    #endregion
}