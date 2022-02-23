// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using System;
using System.Drawing;

namespace BlueBasics {

    public static class Geometry {

        #region Fields

        private const double _rad = Math.PI / 180;

        #endregion

        #region Methods

        public static float ArcTangens(float ta) => (float)(Math.Atan(ta) / _rad);

        public static float Cosinus(float winkel) => (float)Math.Cos(winkel * _rad);

        public static float DegToRad(float degAngle) => (float)((float)degAngle * (Math.PI / 180));

        /// <summary>
        ///  Calculate the distance between the point and the segment
        /// </summary>
        /// <param name="P"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <returns></returns>
        public static float DistanzZuStrecke(this PointF P, PointF P1, PointF P2) => P.DistanzZuStrecke(P1.X, P1.Y, P2.X, P2.Y);

        /// <summary>
        /// Calculate the distance between the point and the segment
        /// </summary>
        /// <param name="P"></param>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <returns></returns>
        public static float DistanzZuStrecke(this PointF P, float X1, float Y1, float X2, float Y2) {
            var SP = PointOnLine(new PointF(P.X, P.Y), X1, Y1, X2, Y2);
            PointF P1 = new(P.X, P.Y);
            return Extensions.PointInRect(SP, X1, Y1, X2, Y2, 5)
                ? Länge(P1, SP)
                : Math.Min(Länge(new PointF(X1, Y1), P1), Länge(new PointF(X2, X2), P1));
        }

        public static float GetAngle(PointF sp, PointF eP) => GetAngle(sp.X, sp.Y, eP.X, eP.Y);

        public static float GetAngle(float x1, float y1, float x2, float y2) {
            // http://de.wikipedia.org/wiki/Polarkoordinaten
            var XA = x2 - x1;
            var YA = y2 - y1;
            return YA == 0f ? XA <= 0f ? 180f : 0f : YA < 0f ? 90f + ArcTangens(XA / YA) : 270f + ArcTangens(XA / YA);
        }

        public static float GetLenght(Point sP, Point ep) {
            // Berechnet die Länge einer Strecke
            float L1 = sP.X - ep.X;
            float L2 = sP.Y - ep.Y;
            // ^2 ist langsamer, laut Project Analyzer
            return (float)Math.Sqrt((float)((L1 * L1) + (L2 * L2)));
        }

        public static float GetLenght(PointF sP, PointF ep) {
            // Berechnet die Länge einer Strecke
            var L1 = (float)(sP.X - ep.X);
            var L2 = (float)(sP.Y - ep.Y);
            // ^2 ist langsamer, laut Project Analyzer
            return (float)Math.Sqrt((L1 * L1) + (L2 * L2));
        }

        /// <summary>
        /// Berechnet die Länge einer Strecke
        /// </summary>
        /// <param name="SP"></param>
        /// <param name="Ep"></param>
        /// <returns></returns>
        public static float Länge(PointF SP, PointF Ep) {
            var L1 = SP.X - Ep.X;
            var L2 = SP.Y - Ep.Y;
            return (float)Math.Sqrt((L1 * L1) + (L2 * L2)); // ^ 2 ist langsamer, laut Project Analyzer
        }

        /// <summary>
        /// Berechnet, ob sich zwei geraden IRGENDWO treffen.
        /// </summary>
        /// <param name="Line1Start"></param>
        /// <param name="Line1End"></param>
        /// <param name="Line2Start"></param>
        /// <param name="Line2End"></param>
        /// <returns></returns>
        public static PointF LinesIntersect(PointF Line1Start, PointF Line1End, PointF Line2Start, PointF Line2End) {
            float a1, a2, b1, b2, c1, c2, denom;
            a1 = Line1End.Y - Line1Start.Y;
            b1 = Line1Start.X - Line1End.X;
            c1 = (Line1End.X * Line1Start.Y) - (Line1Start.X * Line1End.Y);
            a2 = Line2End.Y - Line2Start.Y;
            b2 = Line2Start.X - Line2End.X;
            c2 = (Line2End.X * Line2Start.Y) - (Line2Start.X * Line2End.Y);
            denom = (a1 * b2) - (a2 * b1);
            if (denom is < 0.0000001f and > (-0.0000001f)) {
                // Ergibt ansonsten zu große ergebnisse
                return PointF.Empty;
            }
            return new PointF(((b1 * c2) - (b2 * c1)) / denom, ((a2 * c1) - (a1 * c2)) / denom);
        }

        public static PointF LinesIntersect(PointF Line1Start, PointF Line1End, PointF Line2Start, PointF Line2End, bool considerEndpoints) {
            var sp = LinesIntersect(Line1Start, Line1End, Line2Start, Line2End);
            if (!considerEndpoints) {
                //    DebugPrint("considerEndpoints = false")
                return sp;
            }
            if (sp == null) { return PointF.Empty; }
            const float tol = 0.0001f;
            return sp.X < Math.Min(Line1Start.X, Line1End.X) - tol
                ? PointF.Empty
                : sp.X > Math.Max(Line1Start.X, Line1End.X) + tol
                ? PointF.Empty
                : sp.X < Math.Min(Line2Start.X, Line2End.X) - tol
                ? PointF.Empty
                : sp.X > Math.Max(Line2Start.X, Line2End.X) + tol
                ? PointF.Empty
                : sp.Y < Math.Min(Line1Start.Y, Line1End.Y) - tol
                ? PointF.Empty
                : sp.Y > Math.Max(Line1Start.Y, Line1End.Y) + tol
                ? PointF.Empty
                : sp.Y < Math.Min(Line2Start.Y, Line2End.Y) - tol ? PointF.Empty : sp.Y > Math.Max(Line2Start.Y, Line2End.Y) + tol ? PointF.Empty : sp;
        }

        public static PointF PointOnLine(PointF Maus, float P_X, float P_Y, float Q_X, float Q_Y) {
            //http://de.wikipedia.org/wiki/Geradengleichung
            // < 0.000001 ist 0 gleich, weil ansonsten zu große ergebnisse rauskommen
            if (Math.Abs(P_Y - Q_Y) < 0.0000001d) // genau Waagerecht
            {
                return new PointF(Maus.X, P_Y);
            }
            if (Math.Abs(P_X - Q_X) < 0.0000001d) // genau Senkrecht
            {
                return new PointF(P_X, Maus.Y);
            }
            var m1 = (P_Y - Q_Y) / (P_X - Q_X);
            var m2 = -1 / m1;
            var T2 = Maus.Y + (Maus.X / m1);
            var T1 = P_Y - (m1 * P_X);
            var SchnittX = (T2 - T1) / (m1 - m2);
            var Schnitty = (m1 * SchnittX) + T1;
            return new PointF(SchnittX, Schnitty);
        }

        public static PointF PolarToCartesian(float R, float Winkel) {
            // http://de.wikipedia.org/wiki/Polarkoordinaten
            Winkel %= 360;
            return Winkel switch {
                0 => new PointF(R, 0),
                90 => new PointF(0, -R),
                180 => new PointF(-R, 0),
                270 => new PointF(0, R),
                _ => new PointF((float)R * Cosinus(Winkel), -(float)R * Sinus(Winkel)),
            };
        }

        public static float RadToDeg(float radAngle) => (float)((float)radAngle * (180 / Math.PI));

        public static float Sinus(float winkel) => (float)Math.Sin(winkel * _rad);

        public static float Tangens(float winkel) => (float)Math.Tan(winkel * _rad);

        public static float Winkel(PointF Sp, PointF EP) => GetAngle(Sp.X, Sp.Y, EP.X, EP.Y);

        #endregion
    }
}