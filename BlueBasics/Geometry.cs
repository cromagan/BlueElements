﻿#region BlueElements - a collection of useful tools, database and controls
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

using System;
using System.Drawing;

namespace BlueBasics {
    public static class Geometry {
        private const decimal Rad = (decimal)(Math.PI / 180);

        public static double Sinus(double winkel) => Math.Sin(winkel * (double)Rad);

        public static double Cosinus(double winkel) => Math.Cos(winkel * (double)Rad);

        public static double Tangens(double winkel) => Math.Tan(winkel * (double)Rad);

        public static decimal RadToDeg(decimal radAngle) => (decimal)((double)radAngle * (180 / Math.PI));

        public static decimal DegToRad(decimal degAngle) => (decimal)((double)degAngle * (Math.PI / 180));

        public static decimal ArcTangens(decimal ta) => (decimal)(Math.Atan(Convert.ToDouble(ta)) / (double)Rad);

        public static decimal Länge(Point sP, Point ep) {
            // Berechnet die Länge einer Strecke

            decimal L1 = sP.X - ep.X;
            decimal L2 = sP.Y - ep.Y;

            // ^2 ist langsamer, laut Project Analyzer
            return (decimal)Math.Sqrt(Convert.ToDouble((L1 * L1) + (L2 * L2)));
        }

        public static decimal Länge(PointF sP, PointF ep) {
            // Berechnet die Länge einer Strecke

            var L1 = (decimal)(sP.X - ep.X);
            var L2 = (decimal)(sP.Y - ep.Y);

            // ^2 ist langsamer, laut Project Analyzer
            return (decimal)Math.Sqrt(Convert.ToDouble((L1 * L1) + (L2 * L2)));
        }

        public static decimal Winkel(PointF sp, PointF eP) => Winkel((decimal)sp.X, (decimal)sp.Y, (decimal)eP.X, (decimal)eP.Y);

        public static decimal Winkel(decimal x1, decimal y1, decimal x2, decimal y2) {
            // http://de.wikipedia.org/wiki/Polarkoordinaten

            var XA = x2 - x1;
            var YA = y2 - y1;

            return YA == 0M ? XA <= 0M ? 180M : 0M : YA < 0M ? 90M + ArcTangens(XA / YA) : 270M + ArcTangens(XA / YA);
        }
    }
}