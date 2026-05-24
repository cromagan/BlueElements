// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class ZoomBitmapEventArgs : BitmapEventArgs {

    #region Constructors

    public ZoomBitmapEventArgs(Bitmap? bmp, bool doZoomFit) : base(bmp) => DoZoomFit = doZoomFit;

    #endregion

    #region Properties

    public bool DoZoomFit { get; init; }

    #endregion
}
