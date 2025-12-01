// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Collections.Generic;
using System.Drawing;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Point CanvasToControl(this PointF p, float zoom, float offsetX, float offsetY) => new(p.X.CanvasToControl(zoom, offsetX), p.Y.CanvasToControl(zoom, offsetY));

    public static PointF NearestPoint(this PointF p, List<PointF> pl) {
        if (pl.Count == 0) { return PointF.Empty; }
        var minl = float.MaxValue;
        var rp = PointF.Empty;

        foreach (var thisP in pl) {
            var l = Geometry.GetLength(p, thisP);
            if (l > minl) { continue; }

            minl = l;
            rp = thisP;
        }

        return rp;
    }

    public static bool PointInRect(this PointF p, float x1, float y1, float x2, float y2, float toleranz) {
        RectangleF r = new(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        r.Inflate(toleranz, toleranz);
        return r.Contains(p);
    }

    #endregion
}