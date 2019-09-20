#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
// https://github.com/cromagan/BlueElements
// 
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
// DEALINGS IN THE SOFTWARE. 
#endregion


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


        public static decimal Winkel(PointF Sp, PointF EP)
        {
            return Winkel((decimal)Sp.X, (decimal)Sp.Y, (decimal)EP.X, (decimal)EP.Y);
        }



        public static decimal Winkel(decimal X1, decimal Y1, decimal X2, decimal Y2)
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