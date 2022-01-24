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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_TrimPrefix : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String };
        public override string Description => "Entfernt die angegebenen Präfixe und evtl. übrige Leerzeichen. Die Präfixe werden nur entfernt, wenn vor dem Präfix ein Leerzeichen oder eine Zahl ist. Groß- und Kleinschreibung wird ignoriert.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "TrimPrefix(string, prefix, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "trimprefix" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            var val = attvar.Attributes[0].ValueString;

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                var suf = attvar.Attributes[z].ValueString.ToLower();

                for (var y = 0; y < 2; y++) {
                    if (y == 1) { suf = suf + " "; }

                    if (val.Length > suf.Length) {
                        if (val.ToLower().StartsWith(suf)) {
                            var c = val.Substring(suf.Length, 1);
                            if (Constants.Char_Numerals.Contains(c)) {
                                return new strDoItFeedback(val.Substring(suf.Length, val.Length - suf.Length), enVariableDataType.String);
                            }
                        }
                    }
                }
            }

            return new strDoItFeedback(val, enVariableDataType.String);
        }

        #endregion
    }
}