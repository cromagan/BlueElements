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

using Skript.Enums;
using System.Collections.Generic;
using BlueScript.Structuren;

namespace BlueScript {

    internal class Method_Element : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List, enVariableDataType.Integer };
        public override string Description => "Gibt ein das Element der Liste mit der Indexnummer als Text zurück. Die Liste beginnt mit dem Element 0.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "Element(VariableListe, Indexnummer)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "element" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            var i = attvar.Attributes[1].ValueInt;
            var list = attvar.Attributes[0].ValueListString;
            return i < 0 || i >= list.Count ? new strDoItFeedback("Element nicht in Liste")
                                            : new strDoItFeedback(list[i], enVariableDataType.String);
        }

        #endregion
    }
}