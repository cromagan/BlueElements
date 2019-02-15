using System;
using System.Drawing;
using BlueBasics.Enums;

namespace BlueBasics
{
    public static partial class Extensions
    {

        public static Point PointOf(this Rectangle r, enAlignment P)
        {


            switch (P)
            {
                case enAlignment.Bottom_Left:
                    return new Point(r.Left, r.Bottom);
                case enAlignment.Bottom_Right:
                    return new Point(r.Right, r.Bottom);
                case enAlignment.Top_Left:
                    return new Point(r.Left, r.Top);
                case enAlignment.Top_Right:
                    return new Point(r.Right, r.Top);
                case enAlignment.Bottom_HorizontalCenter:
                    return new Point(Convert.ToInt32(r.Left + r.Width / 2.0), r.Bottom);
                case enAlignment.Top_HorizontalCenter:
                    return new Point(Convert.ToInt32(r.Left + r.Width / 2.0), r.Top);
                case enAlignment.VerticalCenter_Left:
                    return new Point(r.Left, Convert.ToInt32(r.Top + r.Height / 2.0));
                case enAlignment.VerticalCenter_Right:
                    return new Point(r.Right, Convert.ToInt32(r.Top + r.Height / 2.0));
                case enAlignment.Horizontal_Vertical_Center:
                    return new Point(Convert.ToInt32(r.Left + r.Width / 2.0), Convert.ToInt32(r.Top + r.Height / 2.0));
                default:
                    Develop.DebugPrint(P);
                    return new Point();

            }
        }


        public static Point NearestCornerOF(this Rectangle r, Point P)
        {

            var LO = r.PointOf(enAlignment.Top_Left);
            var rO = r.PointOf(enAlignment.Top_Right);
            var ru = r.PointOf(enAlignment.Bottom_Right);
            var lu = r.PointOf(enAlignment.Bottom_Left);


            var llo = Geometry.Länge(P, LO);
            var lro = Geometry.Länge(P, rO);
            var llu = Geometry.Länge(P, lu);
            var lru = Geometry.Länge(P, ru);

            var Erg = Math.Min(Math.Min(llo, lro), Math.Min(llu, lru));

            if (Erg == llo) { return LO; }

            if (Erg == lro) { return rO; }

            if (Erg == llu) { return lu; }

            if (Erg == lru) { return ru; }

            return new Point();

        }
    }
}