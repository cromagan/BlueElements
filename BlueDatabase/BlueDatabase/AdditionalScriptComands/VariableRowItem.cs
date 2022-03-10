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

using BlueDatabase;
using BlueScript.Enums;
using System.Drawing;

namespace BlueScript {

    public class VariableRowItem : Variable {

        #region Fields

        private RowItem? _row;

        #endregion

        #region Constructors

        public VariableRowItem(string name, RowItem? value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) {
            _row = value;
        }

        public VariableRowItem(RowItem? value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        #endregion

        #region Properties

        public RowItem? RowItem {
            get => _row;
            set {
                if (Readonly) { return; }
                _row = value;
            }
        }

        public override string ShortName => "row";
        public override VariableDataType Type => VariableDataType.Object;

        #endregion
    }
}