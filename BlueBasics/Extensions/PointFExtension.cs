using System;
using System.Drawing;

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static bool PointInRect(PointF P, decimal X1, decimal Y1, decimal X2, decimal Y2, float Toleranz)
        {

            var r = new RectangleF((float)Math.Min(X1, X2), (float)Math.Min(Y1, Y2), (float)Math.Abs(X1 - X2), (float)Math.Abs(Y1 - Y2));

            r.Inflate(Toleranz, Toleranz);

            return r.Contains(P);

        }
    }
}