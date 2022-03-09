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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Enums;

namespace BlueScript.Methods {

    internal class Method_TrimSuffix : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String, VariableDataType.String };
        public override string Description => "Entfernt die angegebenen Suffixe und evtl. übrige Leerzeichen. Die Suffixe werden nur entfernt, wenn vor dem Suffix ein Leerzeichen oder eine Zahl ist. Groß- und Kleinschreibung wird ignoriert.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "TrimSuffix(string, suffix, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "trimsuffix" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
            var val = attvar.Attributes[0].ValueString;

            const string tmp = Constants.Char_Numerals + " ";

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                var suf = attvar.Attributes[z].ValueString.ToLower();
                if (val.Length <= suf.Length) {
                    continue;
                }

                if (val.ToLower().EndsWith(suf)) {
                    var c = val.Substring(val.Length - suf.Length - 1, 1);
                    if (tmp.Contains(c)) {
                        return new DoItFeedback(val.Substring(0, val.Length - suf.Length).TrimEnd(" "), VariableDataType.String);
                    }
                }
            }

            return new DoItFeedback(val, VariableDataType.String);
        }

        #endregion
    }
}