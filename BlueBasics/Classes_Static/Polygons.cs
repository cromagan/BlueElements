// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using BlueBasics.Enums;

namespace BlueBasics;

public static class Polygons {

    #region Methods

    public static void AddRad(this GraphicsPath gP, PointF middle, PointF startP, float wink) {
        var radius = Math.Abs(Geometry.GetLenght(middle, startP));
        var startw = Geometry.GetAngle(middle, startP);
        gP.AddArc(middle.X - radius, middle.Y - radius, radius * 2, radius * 2, -startw, -wink);
    }

    public static GraphicsPath Poly_Arrow(Rectangle rect) {
        GraphicsPath p = new();
        // --------+  >
        //         | /
        //         |/
        //
        PointF plusOben = new((float)(rect.Left + (rect.Width * 0.5)),
            (float)(rect.PointOf(Alignment.VerticalCenter_Right).Y - (rect.Height * 0.18)));
        PointF plusUnten = new((float)(rect.Left + (rect.Width * 0.5)),
            (float)(rect.PointOf(Alignment.VerticalCenter_Right).Y + (rect.Height * 0.18)));
        p.AddLine(rect.PointOf(Alignment.VerticalCenter_Right), plusUnten with { Y = rect.Bottom });
        p.AddLine(p.GetLastPoint(), plusUnten);
        p.AddLine(p.GetLastPoint(), plusUnten with { X = rect.Left });
        p.AddLine(p.GetLastPoint(), plusOben with { X = rect.Left });
        p.AddLine(p.GetLastPoint(), plusOben);
        p.AddLine(p.GetLastPoint(), plusOben with { Y = rect.Top });
        p.CloseFigure();

        return p;
    }

    public static GraphicsPath Poly_Bruchlinie(Rectangle rect) {
        GraphicsPath p = new();
        p.AddLine(rect.PointOf(Alignment.Top_Left), rect.PointOf(Alignment.Top_Right));
        p.AddLine(p.GetLastPoint(), rect.PointOf(Alignment.Bottom_Right));
        p.AddLine(p.GetLastPoint(), rect.PointOf(Alignment.Bottom_Left));
        var versX = rect.Width / 6;
        var versY = -rect.Height / 10;
        var pu = p.GetLastPoint();
        for (var z = 0; z < 10; z++) {
            pu.Y += versY;
            pu.X += versX;
            versX *= -1;
            p.AddLine(p.GetLastPoint(), pu);
        }
        p.CloseFigure();
        return p;
    }

    public static GraphicsPath Poly_Rechteck(Rectangle rect) {
        GraphicsPath tempPolyRechteck = new();
        tempPolyRechteck.AddRectangle(rect);
        tempPolyRechteck.CloseFigure();
        return tempPolyRechteck;
    }

    public static GraphicsPath? Poly_RoundRec(Rectangle rect, int radius) => Poly_RoundRec(rect.X, rect.Y, rect.Width, rect.Height, radius);

    public static GraphicsPath? Poly_RoundRec(int x, int y, int width, int height, int radius) {
        if (width < 1 || height < 1) { return null; }
        GraphicsPath tempPolyRoundRec = new();
        if (radius > (height / 2.0) + 2) { radius = (int)(height / 2.0) + 2; }
        if (radius > (width / 2.0) + 2) { radius = (int)(width / 2.0) + 2; }

        tempPolyRoundRec.AddLine(x + radius, y, x + width - radius, y);
        if (radius > 0) { AddRad90(x + width - radius, y, 270); }
        tempPolyRoundRec.AddLine(x + width, y + radius, x + width, y + height - radius);
        if (radius > 0) { AddRad90(x + width - radius, y + height - radius, 0); }
        tempPolyRoundRec.AddLine(x + width - radius, y + height, x + radius, y + height);
        if (radius > 0) { AddRad90(x, y + height - radius, 90); }
        tempPolyRoundRec.AddLine(x, y + height - radius, x, y + radius);
        if (radius > 0) { AddRad90(x, y, 180); }
        tempPolyRoundRec.CloseFigure();
        return tempPolyRoundRec;

        void AddRad90(int mxX, int mxY, int gradStart) => tempPolyRoundRec.AddArc(mxX, mxY, radius, radius, gradStart, 90);
    }

    public static GraphicsPath Poly_Triangle(PointF p1, PointF p2, PointF p3) {
        GraphicsPath p = new();
        p.AddLine(p1, p2);
        p.AddLine(p2, p3);
        p.CloseFigure();
        return p;
    }

    #endregion
}