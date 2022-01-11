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

        public static double ArcTangens(double ta) => (double)(Math.Atan(Convert.ToDouble(ta)) / _rad);

        public static double Cosinus(double winkel) => Math.Cos(winkel * _rad);

        public static double DegToRad(double degAngle) => (double)((double)degAngle * (Math.PI / 180));

        public static double GetAngle(PointF sp, PointF eP) => GetAngle(sp.X, sp.Y, eP.X, eP.Y);

        public static double GetAngle(double x1, double y1, double x2, double y2) {
            // http://de.wikipedia.org/wiki/Polarkoordinaten
            var XA = x2 - x1;
            var YA = y2 - y1;
            return YA == 0d ? XA <= 0d ? 180d : 0d : YA < 0d ? 90d + ArcTangens(XA / YA) : 270d + ArcTangens(XA / YA);
        }

        public static double GetLenght(Point sP, Point ep) {
            // Berechnet die Länge einer Strecke
            double L1 = sP.X - ep.X;
            double L2 = sP.Y - ep.Y;
            // ^2 ist langsamer, laut Project Analyzer
            return Math.Sqrt((double)((L1 * L1) + (L2 * L2)));
        }

        public static double GetLenght(PointF sP, PointF ep) {
            // Berechnet die Länge einer Strecke
            var L1 = (double)(sP.X - ep.X);
            var L2 = (double)(sP.Y - ep.Y);
            // ^2 ist langsamer, laut Project Analyzer
            return Math.Sqrt((double)((L1 * L1) + (L2 * L2)));
        }

        public static double RadToDeg(double radAngle) => (double)((double)radAngle * (180 / Math.PI));

        public static double Sinus(double winkel) => Math.Sin(winkel * _rad);

        public static double Tangens(double winkel) => Math.Tan(winkel * _rad);

        #endregion
    }
}