
using System;
using System.Drawing;

namespace BlueBasics
{
    public static class Geometry
    {

        private const decimal Rad = (decimal)(Math.PI / 180);


        public static double Sinus(double Winkel)
        {
            return Math.Sin(Winkel * (double)Rad);
        }
        public static double Cosinus(double Winkel)
        {
            return Math.Cos(Winkel * (double)Rad);
        }
        public static double Tangens(double Winkel)
        {
            return Math.Tan(Winkel * (double)Rad);
        }
        public static decimal RadToDeg(decimal RadAngle)
        {
            return (decimal)((double)RadAngle * (180 / Math.PI));
        }

        public static decimal DegToRad(decimal DegAngle)
        {
            return (decimal)((double)DegAngle * (Math.PI / 180));
        }

        public static decimal ArcTangens(decimal Ta)
        {
            return (decimal)(Math.Atan(Convert.ToDouble(Ta)) / (double)Rad);
        }


        public static decimal Länge(Point SP, Point Ep)
        {
            // Berechnet die Länge einer Strecke

            decimal L1 = SP.X - Ep.X;
            decimal L2 = SP.Y - Ep.Y;


            // ^2 ist langsamer, laut Project Analyzer
            return (decimal)Math.Sqrt(Convert.ToDouble(L1 * L1 + L2 * L2));
        }

        public static decimal Länge(PointF SP, PointF Ep)
        {
            // Berechnet die Länge einer Strecke

            var L1 = (decimal)(SP.X - Ep.X);
            var L2 = (decimal)(SP.Y - Ep.Y);

            // ^2 ist langsamer, laut Project Analyzer
            return (decimal)Math.Sqrt(Convert.ToDouble(L1 * L1 + L2 * L2));
        }


        public static decimal Winkelx(PointF Sp, PointF EP)
        {
            return Winkelx((decimal)Sp.X, (decimal)Sp.Y, (decimal)EP.X, (decimal)EP.Y);
        }



        public static decimal Winkelx(decimal X1, decimal Y1, decimal X2, decimal Y2)
        {

            // http://de.wikipedia.org/wiki/Polarkoordinaten

            var XA = X2 - X1;
            var YA = Y2 - Y1;



            if (YA == 0M)
            {
                if (XA <= 0M)
                {
                    return 180M;
                }
                return 0M;
            }

            if (YA < 0M)
            {
                return 90M + ArcTangens(XA / YA);
            }

            return 270M + ArcTangens(XA / YA);


        }

    }

}