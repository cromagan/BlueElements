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

namespace BlueScript {

    internal class Method_Clear : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List };
        public override string Description => "Entfernt alle Einträge einer Liste";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "Clear(VariableListe);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "clear" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            if (attvar.Attributes[0].Readonly) { return strDoItFeedback.Schreibgschützt(); }
            attvar.Attributes[0].ValueListString = new List<string>();
            return strDoItFeedback.Null();
        }

        #endregion
    }
}