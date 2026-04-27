// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class PositionEventArgs : System.EventArgs {

    #region Constructors

    public PositionEventArgs(float x, float y) {
        X = x;
        Y = y;
    }

    #endregion

    #region Properties

    public float X { get; set; }

    public float Y { get; set; }

    #endregion
}