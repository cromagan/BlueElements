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

#nullable enable

using BlueScript.Enums;
using System.Drawing;

namespace BlueScript {

    public class VariableBitmap : Variable {

        #region Fields

        private Bitmap _bmp;

        #endregion

        #region Constructors

        public VariableBitmap(string name, Bitmap value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) {
            _bmp = value;
        }

        public VariableBitmap(Bitmap value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        #endregion

        #region Properties

        public override string ShortName => "str";
        public override VariableDataType Type => VariableDataType.String;

        public Bitmap ValueBitmap {
            get => _bmp;
            set {
                if (Readonly) { return; }
                _bmp = value;
            }
        }

        #endregion
    }
}