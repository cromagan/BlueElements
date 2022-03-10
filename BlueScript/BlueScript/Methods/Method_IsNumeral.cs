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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Enums;

namespace BlueScript.Methods {

    internal class Method_IsNumeral : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Nummeral_or_String };
        public override string Description => "Prüft, ob der Inhalt der Variable eine gültige Zahl ist. ";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Bool;
        public override string StartSequence => "(";
        public override string Syntax => "isNumeral(Value)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "isnumeral" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.Falsch(); }
            if (attvar.Attributes[0] is VariableFloat) { return DoItFeedback.Wahr(); }
            if (attvar.Attributes[0] is VariableString vs) {
                if (vs.ValueString.IsNumeral()) { return DoItFeedback.Wahr(); }
            }
            return DoItFeedback.Falsch();
        }

        #endregion
    }
}