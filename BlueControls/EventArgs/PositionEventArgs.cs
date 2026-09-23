// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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