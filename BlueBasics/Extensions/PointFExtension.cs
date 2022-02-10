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

    public static partial class Extensions {
        //public static PointF ZoomAndMove(this PointF p, AdditionalDrawing e) => ZoomAndMove(e.Zoom, e.ShiftX, e.ShiftY);

        #region Methods

        public static bool PointInRect(this PointF p, double x1, double y1, double x2, double y2, float toleranz) {
            RectangleF r = new((float)Math.Min(x1, x2), (float)Math.Min(y1, y2), (float)Math.Abs(x1 - x2), (float)Math.Abs(y1 - y2));
            r.Inflate(toleranz, toleranz);
            return r.Contains(p);
        }

        public static PointF ZoomAndMove(this PointF p, float zoom, float shiftX, float shiftY) => new((float)((p.X * zoom) - shiftX + (zoom / 2)), (float)((p.Y * zoom) - shiftY + (zoom / 2)));

        #endregion
    }
}