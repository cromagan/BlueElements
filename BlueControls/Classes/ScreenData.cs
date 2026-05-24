// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes;

public class ScreenData {

    #region Properties

    public Bitmap? Area { get; set; }
    public Point Point1 { get; set; }
    public Point Point2 { get; set; }
    public Bitmap? Screen { get; set; }

    #endregion

    #region Methods

    public Rectangle AreaRectangle() =>
        new(Math.Min(Point1.X, Point2.X), Math.Min(Point1.Y, Point2.Y),
            Math.Max(Point1.X - Point2.X, Point2.X - Point1.X) + 1,
            Math.Max(Point1.Y - Point2.Y, Point2.Y - Point1.Y) + 1);

    #endregion
}