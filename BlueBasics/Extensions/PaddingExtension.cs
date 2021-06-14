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

namespace BlueBasics {

    public static partial class Extensions {

        #region Methods

        public static System.Windows.Forms.Padding PaddingParse(this string code) {
            code = code.RemoveChars("{}LeftTopRightBm= ");
            var w = code.Split(',');
            System.Windows.Forms.Padding P = new() {
                Left = int.Parse(w[0]),
                Top = int.Parse(w[1]),
                Right = int.Parse(w[2]),
                Bottom = int.Parse(w[3])
            };
            return P;
        }

        #endregion
    }
}