// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class ZoomBitmapEventArgs : BitmapEventArgs {

    #region Constructors

    public ZoomBitmapEventArgs(Bitmap? bmp, bool doZoomFit) : base(bmp) => DoZoomFit = doZoomFit;

    #endregion

    #region Properties

    public bool DoZoomFit { get; init; }

    #endregion
}
