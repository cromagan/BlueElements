// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class BitmapEventArgs : System.EventArgs {

    #region Constructors

    public BitmapEventArgs(Bitmap? bmp) => Bmp = bmp;

    #endregion

    #region Properties

    public Bitmap? Bmp { get; set; }

    #endregion
}
