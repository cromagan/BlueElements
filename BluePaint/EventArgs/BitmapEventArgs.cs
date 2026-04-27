// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BluePaint.EventArgs;

public class BitmapEventArgs : System.EventArgs {

    #region Constructors

    public BitmapEventArgs(Bitmap? bmp) => Bmp = bmp;

    #endregion

    #region Properties

    public Bitmap? Bmp { get; set; }

    #endregion
}