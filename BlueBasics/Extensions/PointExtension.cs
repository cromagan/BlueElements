using System.Drawing;

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static Point PointParse(string Code)
        {


            Code = Code.RemoveChars("{}XYxy= ");

            var P = new Point();


            if (string.IsNullOrEmpty(Code))
            {
                P.X = 0;
                P.Y = 0;
                return P;
            }


            var w = Code.Split(',');

            P.X = int.Parse(w[0]);
            P.Y = int.Parse(w[1]);
            return P;

        }
    }
}