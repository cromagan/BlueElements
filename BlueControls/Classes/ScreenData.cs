// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing;

namespace BlueControls.Classes;

public class ScreenData {

    #region Fields

    public Bitmap? Area;
    public Point Point1;
    public Point Point2;
    public Bitmap? Screen;

    #endregion

    #region Methods

    public Rectangle AreaRectangle() =>
        new(Math.Min(Point1.X, Point2.X), Math.Min(Point1.Y, Point2.Y),
            Math.Max(Point1.X - Point2.X, Point2.X - Point1.X) + 1,
            Math.Max(Point1.Y - Point2.Y, Point2.Y - Point1.Y) + 1);

    #endregion
}