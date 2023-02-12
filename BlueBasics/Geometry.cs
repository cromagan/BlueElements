// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

namespace BlueBasics;

public static class Geometry {

    #region Fields

    private const double Rad = Math.PI / 180;

    #endregion

    #region Methods

    public static float ArcTangens(float ta) => (float)(Math.Atan(ta) / Rad);

    public static float Cosinus(float winkel) => (float)Math.Cos(winkel * Rad);

    public static float DegToRad(float degAngle) => (float)(degAngle * (Math.PI / 180));

    /// <summary>
    ///  Calculate the distance between the point and the segment
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static float DistanzZuStrecke(this PointF p, PointF p1, PointF p2) => p.DistanzZuStrecke(p1.X, p1.Y, p2.X, p2.Y);

    /// <summary>
    /// Calculate the distance between the point and the segment
    /// </summary>
    /// <param name="p"></param>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    public static float DistanzZuStrecke(this PointF p, float x1, float y1, float x2, float y2) {
        var sp = PointOnLine(new PointF(p.X, p.Y), x1, y1, x2, y2);
        PointF p1 = new(p.X, p.Y);
        return sp.PointInRect(x1, y1, x2, y2, 5)
            ? Länge(p1, sp)
            : Math.Min(Länge(new PointF(x1, y1), p1), Länge(new PointF(x2, x2), p1));
    }

    public static float GetAngle(PointF sp, PointF eP) => GetAngle(sp.X, sp.Y, eP.X, eP.Y);

    public static float GetAngle(float x1, float y1, float x2, float y2) {
        // http://de.wikipedia.org/wiki/Polarkoordinaten
        var xa = x2 - x1;
        var ya = y2 - y1;
        return ya == 0f ? xa <= 0f ? 180f : 0f : ya < 0f ? 90f + ArcTangens(xa / ya) : 270f + ArcTangens(xa / ya);
    }

    public static float GetLenght(Point sP, Point ep) {
        // Berechnet die Länge einer Strecke
        float l1 = sP.X - ep.X;
        float l2 = sP.Y - ep.Y;
        // ^2 ist langsamer, laut Project Analyzer
        return (float)Math.Sqrt((l1 * l1) + (l2 * l2));
    }

    public static float GetLenght(PointF sP, PointF ep) {
        // Berechnet die Länge einer Strecke
        var l1 = sP.X - ep.X;
        var l2 = sP.Y - ep.Y;
        // ^2 ist langsamer, laut Project Analyzer
        return (float)Math.Sqrt((l1 * l1) + (l2 * l2));
    }

    /// <summary>
    /// Berechnet die Länge einer Strecke
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="ep"></param>
    /// <returns></returns>
    public static float Länge(PointF sp, PointF ep) {
        var l1 = sp.X - ep.X;
        var l2 = sp.Y - ep.Y;
        return (float)Math.Sqrt((l1 * l1) + (l2 * l2)); // ^ 2 ist langsamer, laut Project Analyzer
    }

    /// <summary>
    /// Berechnet, ob sich zwei geraden IRGENDWO treffen.
    /// </summary>
    /// <param name="line1Start"></param>
    /// <param name="line1End"></param>
    /// <param name="line2Start"></param>
    /// <param name="line2End"></param>
    /// <returns></returns>
    public static PointF LinesIntersect(PointF line1Start, PointF line1End, PointF line2Start, PointF line2End) {
        var a1 = line1End.Y - line1Start.Y;
        var b1 = line1Start.X - line1End.X;
        var c1 = (line1End.X * line1Start.Y) - (line1Start.X * line1End.Y);
        var a2 = line2End.Y - line2Start.Y;
        var b2 = line2Start.X - line2End.X;
        var c2 = (line2End.X * line2Start.Y) - (line2Start.X * line2End.Y);
        var denom = (a1 * b2) - (a2 * b1);
        if (denom is < 0.0000001f and > -0.0000001f) {
            // Ergibt ansonsten zu große ergebnisse
            return PointF.Empty;
        }
        return new PointF(((b1 * c2) - (b2 * c1)) / denom, ((a2 * c1) - (a1 * c2)) / denom);
    }

    public static PointF LinesIntersect(PointF line1Start, PointF line1End, PointF line2Start, PointF line2End, bool considerEndpoints) {
        var sp = LinesIntersect(line1Start, line1End, line2Start, line2End);
        if (!considerEndpoints) {
            //    DebugPrint("considerEndpoints = false")
            return sp;
        }
        if (sp == null) { return PointF.Empty; }
        const float tol = 0.0001f;
        if (sp.X < Math.Min(line1Start.X, line1End.X) - tol) {
            return PointF.Empty;
        }

        if (sp.X > Math.Max(line1Start.X, line1End.X) + tol) {
            return PointF.Empty;
        }
        if (sp.X < Math.Min(line2Start.X, line2End.X) - tol) {
            return PointF.Empty;
        }
        if (sp.X > Math.Max(line2Start.X, line2End.X) + tol) {
            return PointF.Empty;
        }
        if (sp.Y < Math.Min(line1Start.Y, line1End.Y) - tol) {
            return PointF.Empty;
        }
        if (sp.Y > Math.Max(line1Start.Y, line1End.Y) + tol) {
            return PointF.Empty;
        }
        if (sp.Y < Math.Min(line2Start.Y, line2End.Y) - tol) {
            return PointF.Empty;
        }
        if (sp.Y > Math.Max(line2Start.Y, line2End.Y) + tol) {
            return PointF.Empty;
        }
        return sp;
    }

    public static PointF PointOnLine(PointF maus, float pX, float pY, float qX, float qY) {
        //http://de.wikipedia.org/wiki/Geradengleichung
        // < 0.000001 ist 0 gleich, weil ansonsten zu große ergebnisse rauskommen
        if (Math.Abs(pY - qY) < 0.0000001d) // genau Waagerecht
        {
            return maus with { Y = pY };
        }
        if (Math.Abs(pX - qX) < 0.0000001d) // genau Senkrecht
        {
            return maus with { X = pX };
        }
        var m1 = (pY - qY) / (pX - qX);
        var m2 = -1 / m1;
        var t2 = maus.Y + (maus.X / m1);
        var t1 = pY - (m1 * pX);
        var schnittX = (t2 - t1) / (m1 - m2);
        var schnitty = (m1 * schnittX) + t1;
        return new PointF(schnittX, schnitty);
    }

    public static PointF PolarToCartesian(float r, float winkel) {
        // http://de.wikipedia.org/wiki/Polarkoordinaten
        winkel %= 360;
        return winkel switch {
            0 => new PointF(r, 0),
            90 => new PointF(0, -r),
            180 => new PointF(-r, 0),
            270 => new PointF(0, r),
            _ => new PointF(r * Cosinus(winkel), -r * Sinus(winkel))
        };
    }

    public static float RadToDeg(float radAngle) => (float)(radAngle * (180 / Math.PI));

    public static float Sinus(float winkel) => (float)Math.Sin(winkel * Rad);

    public static float Tangens(float winkel) => (float)Math.Tan(winkel * Rad);

    public static float Winkel(PointF sp, PointF ep) => GetAngle(sp.X, sp.Y, ep.X, ep.Y);

    #endregion
}