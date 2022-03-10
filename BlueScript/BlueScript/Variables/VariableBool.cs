﻿// Authors:
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

    public class VariableBool : Variable {

        #region Fields

        private bool _valuebool = false;

        #endregion

        #region Constructors

        public VariableBool(string name, bool value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) { _valuebool = value; }

        public VariableBool(bool value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        #endregion

        #region Properties

        public override string ShortName => "bol";
        public override VariableDataType Type => VariableDataType.String;

        public bool ValueBool {
            get => _valuebool;
            set {
                if (Readonly) { return; }
                _valuebool = value; // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
            }
        }

        public override string ValueForReplace { get => _valuebool ? "true" : "false"; }

        #endregion
    }
}