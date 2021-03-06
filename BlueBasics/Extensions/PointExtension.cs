﻿// Authors:
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

using System.Drawing;

namespace BlueBasics {

    public static partial class Extensions {

        #region Methods

        public static Point PointParse(this string code) {
            code = code.RemoveChars("{}XYxy= ");
            Point P = new();
            if (string.IsNullOrEmpty(code)) {
                P.X = 0;
                P.Y = 0;
                return P;
            }
            var w = code.Split(',');
            P.X = int.Parse(w[0]);
            P.Y = int.Parse(w[1]);
            return P;
        }

        #endregion
    }
}