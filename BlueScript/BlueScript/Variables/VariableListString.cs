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

using BlueBasics;
using BlueScript.Enums;
using System;
using System.Collections.Generic;

namespace BlueScript {

    public class VariableListString : Variable {

        #region Fields

        private List<string> _list;

        #endregion

        #region Constructors

        public VariableListString(string name, List<string> value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) {
            _list = new List<string>();
            _list.AddRange(value);
        }

        public VariableListString(string name) : this(name, new List<string>(), true, false, string.Empty) { }

        public VariableListString(List<string> value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        #endregion

        #region Properties

        public override string ReadableText {
            get {
                if (_list == null || _list.Count == 0) { return "{ }"; }
                return "{\"" + _list.JoinWith("\", \"") + "\"}";
            }
        }

        public override string ShortName => "lst";
        public override VariableDataType Type => VariableDataType.List;
        public override string ValueForReplace { get => ReadableText; }

        public List<string> ValueList {
            get => _list;
            set {
                if (Readonly) { return; }
                _list = new List<string>();
                _list.AddRange(value);
            }
        }

        [Obsolete]
        public List<string> ValueListString {
            get => _list;
            set {
                if (Readonly) { return; }
                _list = new List<string>();
                _list.AddRange(value);
            }
        }

        #endregion
    }
}