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

namespace BlueScript {

    public class VariableString : Variable {

        #region Fields

        private string _valueString = string.Empty;

        #endregion

        #region Constructors

        public VariableString(string name, string value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) { _valueString = value.RestoreCriticalVariableChars(); }

        public VariableString(string value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        public VariableString(string name, string value) : this(name, value, true, false, string.Empty) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gleichgesetzt mit ValueString
        /// </summary>
        public override string ReadableText { get => _valueString; }

        public override string ShortName => "str";
        public override VariableDataType Type => VariableDataType.String;
        public override string ValueForReplace { get => "\"" + _valueString.RemoveCriticalVariableChars() + "\""; }

        /// <summary>
        /// Gleichgesetzt mit ReadableText
        /// </summary>
        public string ValueString {
            get => _valueString;
            set {
                if (Readonly) { return; }
                _valueString = value.RestoreCriticalVariableChars(); // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
            }
        }

        #endregion
    }
}