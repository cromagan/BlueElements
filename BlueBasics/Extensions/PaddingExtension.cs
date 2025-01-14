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

#nullable enable

using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Padding PaddingParse(this string toParse) {
        toParse = toParse.FromNonCritical().RemoveChars("{}LeftTopRightBm= ");
        var w = toParse.Split(',');
        Padding p = new() {
            Left = IntParse(w[0]),
            Top = IntParse(w[1]),
            Right = IntParse(w[2]),
            Bottom = IntParse(w[3])
        };
        return p;
    }

    #endregion
}