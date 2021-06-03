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

using System;
using System.Drawing;

namespace BlueBasics {
    public static partial class Extensions {
        public static void DrawImageInRectAspectRatio(this Graphics gR, Bitmap bmp, int x, int y, int width, int height) {
            var Sc = Math.Min((float)width / bmp.Width, (float)height / bmp.Height);
            var dw = (int)(bmp.Width * Sc);
            var dh = (int)(bmp.Height * Sc);
            gR.DrawImage(bmp, x + ((width - dw) / 2), y + ((height - dh) / 2), dw, dh);
        }

        public static void DrawImageInRectAspectRatio(this Graphics gR, Bitmap bmp, Rectangle r) {
            DrawImageInRectAspectRatio(gR, bmp, r.Left, r.Top, r.Width, r.Height);
        }

        public static void DrawRectangle(this Graphics gR, Pen pen, RectangleF r) {
            gR.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
        }

        public static void DrawRad(this Graphics gR, Pen pen, PointF middle, PointF startP, float wink) {
            var radius = (float)Math.Abs(Geometry.Länge(middle, startP));
            var startw = (float)Geometry.Winkel(middle, startP);

            gR.DrawArc(pen, middle.X - radius, middle.Y - radius, radius * 2, radius * 2, -startw, -wink);
        }
    }
}
