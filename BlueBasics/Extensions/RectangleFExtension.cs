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
using BlueBasics.Enums;
using System;
using System.Drawing;
namespace BlueBasics {
    public static partial class Extensions {
        public static PointF PointOf(this RectangleF r, enAlignment p) {
            switch (p) {
                case enAlignment.Bottom_Left:
                    return new PointF(r.Left, r.Bottom);
                case enAlignment.Bottom_Right:
                    return new PointF(r.Right, r.Bottom);
                case enAlignment.Top_Left:
                    return new PointF(r.Left, r.Top);
                case enAlignment.Top_Right:
                    return new PointF(r.Right, r.Top);
                case enAlignment.Bottom_HorizontalCenter:
                    return new PointF((int)(r.Left + (r.Width / 2.0)), r.Bottom);
                case enAlignment.Top_HorizontalCenter:
                    return new PointF((int)(r.Left + (r.Width / 2.0)), r.Top);
                case enAlignment.VerticalCenter_Left:
                    return new PointF(r.Left, (int)(r.Top + (r.Height / 2.0)));
                case enAlignment.VerticalCenter_Right:
                    return new PointF(r.Right, (int)(r.Top + (r.Height / 2.0)));
                case enAlignment.Horizontal_Vertical_Center:
                    return new Point((int)(r.Left + (r.Width / 2.0)), (int)(r.Top + (r.Height / 2.0)));
                default:
                    Develop.DebugPrint(p);
                    return Point.Empty;
            }
        }
        public static PointF NearestCornerOF(this RectangleF r, Point p) {
            var LO = r.PointOf(enAlignment.Top_Left);
            var rO = r.PointOf(enAlignment.Top_Right);
            var ru = r.PointOf(enAlignment.Bottom_Right);
            var lu = r.PointOf(enAlignment.Bottom_Left);
            var llo = Geometry.Länge(p, LO);
            var lro = Geometry.Länge(p, rO);
            var llu = Geometry.Länge(p, lu);
            var lru = Geometry.Länge(p, ru);
            var Erg = Math.Min(Math.Min(llo, lro), Math.Min(llu, lru));
            return Erg == llo ? LO : Erg == lro ? rO : Erg == llu ? lu : Erg == lru ? ru : Point.Empty;
        }
        public static Rectangle ToRect(this RectangleF r) => new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
    }
}