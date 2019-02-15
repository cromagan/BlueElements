using System.Drawing;

namespace BlueControls.Interfaces
{
    /// <summary>
    /// Wird verwendet, wenn das Steuerelement sich selbst in ein Bitmap zeichet.
    /// Dadurch ist es möglich, dass ein darüberliegendes, transparentes Steuerelement dieses Bitmap abgreift
    /// </summary>
    public interface IBackgroundBitmap
    {
        Bitmap BitmapOfControl();
    }
}