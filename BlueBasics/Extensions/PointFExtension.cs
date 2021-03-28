#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2020 Christian Peter
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

namespace BlueBasics
{
    public static partial class Extensions
    {
        public static bool PointInRect(PointF P, decimal X1, decimal Y1, decimal X2, decimal Y2, float Toleranz)
        {
            var r = new RectangleF((float)Math.Min(X1, X2), (float)Math.Min(Y1, Y2), (float)Math.Abs(X1 - X2), (float)Math.Abs(Y1 - Y2));

            r.Inflate(Toleranz, Toleranz);

            return r.Contains(P);
        }
    }
}