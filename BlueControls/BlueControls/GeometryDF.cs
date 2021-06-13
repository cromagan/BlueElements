#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
using BlueBasics;
using System;
using System.Drawing;

namespace BlueControls {
    public static class GeometryDF {
        /// Berechnet, ob sich zwei geraden IRGENDWO treffen.
        public static PointM LinesIntersect(PointM Line1Start, PointM Line1End, PointM Line2Start, PointM Line2End) {
            decimal a1, a2, b1, b2, c1, c2, denom;
            a1 = Line1End.Y - Line1Start.Y;
            b1 = Line1Start.X - Line1End.X;
            c1 = (Line1End.X * Line1Start.Y) - (Line1Start.X * Line1End.Y);
            a2 = Line2End.Y - Line2Start.Y;
            b2 = Line2Start.X - Line2End.X;
            c2 = (Line2End.X * Line2Start.Y) - (Line2Start.X * Line2End.Y);
            denom = (a1 * b2) - (a2 * b1);
            if (denom is < 0.0000001M and > (-0.0000001M)) {
                // Ergibt ansonsten zu große ergebnisse
                return null;
            }
            return new PointM(((b1 * c2) - (b2 * c1)) / denom, ((a2 * c1) - (a1 * c2)) / denom);
        }
        public static PointM LinesIntersect(PointM Line1Start, PointM Line1End, PointM Line2Start, PointM Line2End, bool considerEndpoints) {
            var sp = LinesIntersect(Line1Start, Line1End, Line2Start, Line2End);
            if (!considerEndpoints) {
                //    DebugPrint("considerEndpoints = false")
                return sp;
            }
            if (sp == null) { return null; }
            const decimal tol = 0.0001m;
            return sp.X < Math.Min(Line1Start.X, Line1End.X) - tol
                ? null
                : sp.X > Math.Max(Line1Start.X, Line1End.X) + tol
                ? null
                : sp.X < Math.Min(Line2Start.X, Line2End.X) - tol
                ? null
                : sp.X > Math.Max(Line2Start.X, Line2End.X) + tol
                ? null
                : sp.Y < Math.Min(Line1Start.Y, Line1End.Y) - tol
                ? null
                : sp.Y > Math.Max(Line1Start.Y, Line1End.Y) + tol
                ? null
                : sp.Y < Math.Min(Line2Start.Y, Line2End.Y) - tol ? null : sp.Y > Math.Max(Line2Start.Y, Line2End.Y) + tol ? null : sp;
        }
        /// <summary>
        ///  Calculate the distance between the point and the segment
        /// </summary>
        /// <param name="P"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <returns></returns>
        public static decimal DistanzZuStrecke(this PointF P, PointM P1, PointM P2) => P.DistanzZuStrecke(P1.X, P1.Y, P2.X, P2.Y);
        //public static decimal DistanzZuStrecke(this PointF P, Point P1, Point P2)
        //{
        //    return P.DistanzZuStrecke(P1.X, P1.Y, P2.X, P2.Y);
        //}
        public static PointM ToPointM(this PointF P) => new(P.X, P.Y);
        /// <summary>
        /// Calculate the distance between the point and the segment
        /// </summary>
        /// <param name="P"></param>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <returns></returns>
        public static decimal DistanzZuStrecke(this PointF P, decimal X1, decimal Y1, decimal X2, decimal Y2) {
            var SP = PointOnLine(new PointM(P.X, P.Y), X1, Y1, X2, Y2);
            PointM P1 = new(P.X, P.Y);
            return Extensions.PointInRect((PointF)SP, X1, Y1, X2, Y2, 5)
                ? Länge(P1, SP)
                : Math.Min(Länge(new PointM(X1, Y1), P1), Länge(new PointM(X2, X2), P1));
        }
        public static PointM PolarToCartesian(decimal R, double Winkel) {
            // http://de.wikipedia.org/wiki/Polarkoordinaten
            Winkel %= 360;
            return Winkel switch {
                0 => new PointM(R, 0),
                90 => new PointM(0, -R),
                180 => new PointM(-R, 0),
                270 => new PointM(0, R),
                _ => new PointM((double)R * Geometry.Cosinus(Winkel), -(double)R * Geometry.Sinus(Winkel)),
            };
        }
        public static decimal Winkel(PointM Sp, PointM EP) => Geometry.Winkel(Sp.X, Sp.Y, EP.X, EP.Y);
        public static PointM PointOnLine(PointM Maus, decimal P_X, decimal P_Y, decimal Q_X, decimal Q_Y) {
            //http://de.wikipedia.org/wiki/Geradengleichung
            // < 0.000001 ist 0 gleich, weil ansonsten zu große ergebnisse rauskommen
            if (Math.Abs(P_Y - Q_Y) < 0.0000001m) // genau Waagerecht
            {
                return new PointM(Maus.X, P_Y);
            }
            if (Math.Abs(P_X - Q_X) < 0.0000001m) // genau Senkrecht
            {
                return new PointM(P_X, Maus.Y);
            }
            var m1 = (P_Y - Q_Y) / (P_X - Q_X);
            var m2 = -1 / m1;
            var T2 = Maus.Y + (Maus.X / m1);
            var T1 = P_Y - (m1 * P_X);
            var SchnittX = (T2 - T1) / (m1 - m2);
            var Schnitty = (m1 * SchnittX) + T1;
            return new PointM(SchnittX, Schnitty);
        }
        public static decimal Länge(PointM SP, PointM Ep) {
            // Berechnet die Länge einer Strecke
            var L1 = SP.X - Ep.X;
            var L2 = SP.Y - Ep.Y;
            // ^ 2 ist langsamer, laut Project Analyzer
            return (decimal)Math.Sqrt(Convert.ToDouble((L1 * L1) + (L2 * L2)));
        }
    }
}
