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
using BlueScript.Structures;
using System;
using BlueBasics;
using BlueScript.Methods;
using System.Collections.Generic;
using System.Linq;

namespace BlueScript.Variables {

    public class VariableListString : Variable {

        #region Fields

        private List<string> _list;

        #endregion

        #region Constructors

        public VariableListString(string name, List<string>? value, bool ronly, bool system, string coment) : base(name,
            ronly, system, coment) {
            _list = new List<string>();
            if (value != null) {
                _list.AddRange(value);
            }
        }

        public VariableListString(string name) : this(name, null, true, false, string.Empty) { }

        public VariableListString(List<string>? value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        public VariableListString(string[] value) : this(value.ToList()) { }

        #endregion

        #region Properties

        public override string ReadableText {
            get {
                if (_list.Count == 0) {
                    return "{ }";
                }

                return "{\"" + _list.JoinWith("\", \"") + "\"}";
            }
        }

        public override string ShortName => "lst";
        public override bool Stringable => true;
        public override VariableDataType Type => VariableDataType.List;

        public override string ValueForReplace {
            get => ReadableText;
        }

        public List<string> ValueList {
            get => _list;
            set {
                if (Readonly) {
                    return;
                }

                _list = new List<string>();
                if (value != null) {
                    _list.AddRange(value);
                }
            }
        }

        [Obsolete]
        public List<string> ValueListString {
            get => _list;
            set {
                if (Readonly) {
                    return;
                }

                _list = new List<string>();
                if (value != null) {
                    _list.AddRange(value);
                }
            }
        }

        #endregion

        #region Methods

        public override DoItFeedback GetValueFrom(Variable variable) {
            if (variable is not VariableListString v) {
                return DoItFeedback.VerschiedeneTypen(this, variable);
            }

            if (Readonly) {
                return DoItFeedback.Schreibgschützt();
            }

            ValueList = v.ValueList;
            return DoItFeedback.Null();
        }

        protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
            succesVar = null;
            if (txt.Length > 1 && txt.StartsWith("{") && txt.EndsWith("}")) {
                var t = txt.DeKlammere(false, true, false, true);

                if (string.IsNullOrEmpty(t)) {
                    succesVar = new VariableListString(new List<string>()); // Leere Liste
                    return true;
                }

                var l = Method.SplitAttributeToVars(t, s, new List<VariableDataType> { VariableDataType.String }, true);
                if (!string.IsNullOrEmpty(l.ErrorMessage)) {
                    return false;
                }

                succesVar = new VariableListString(l.Attributes.AllValues());
                return true;
            }

            return false;
        }

        #endregion
    }
}