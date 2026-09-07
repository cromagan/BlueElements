// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public sealed class TrimmedCanvasMouseEventArgs : CanvasMouseEventArgs {

    #region Constructors

    public TrimmedCanvasMouseEventArgs() : base(new CanvasMouseEventArgs()) {
        IsInPic = false;
        TrimmedCanvasX = 0;
        TrimmedCanvasY = 0;
    }

    public TrimmedCanvasMouseEventArgs(CanvasMouseEventArgs e, int trimmedCanvasX, int trimmedCanvasY, bool isinPic) : base(e) {
        IsInPic = isinPic;
        TrimmedCanvasX = trimmedCanvasX;
        TrimmedCanvasY = trimmedCanvasY;
    }

    #endregion

    #region Properties

    public bool IsInPic { get; }
    public int TrimmedCanvasX { get; }
    public int TrimmedCanvasY { get; }

    #endregion
}