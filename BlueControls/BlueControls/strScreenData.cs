using System;
using System.Drawing;

namespace BlueControls
{
    public struct strScreenData
    {
        public Bitmap Pic;
        public bool IsResized;

        public Point Point1;
        public Point Point2;




        public Point HookP1;
        public Point HookP2;

        public Rectangle GrabedArea()
        {


            return new Rectangle(Math.Min(Point1.X, Point2.X), Math.Min(Point1.Y, Point2.Y), Math.Max(Point1.X - Point2.X, Point2.X - Point1.X) + 1, Math.Max(Point1.Y - Point2.Y, Point2.Y - Point1.Y) + 1);
        }


    }
}