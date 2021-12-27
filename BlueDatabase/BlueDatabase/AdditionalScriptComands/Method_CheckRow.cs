// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueDatabase;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    public class Method_CheckRow : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Object };

        public override string Description => "Prüft die angegebene Zeile mit der Startroutine 'script'. Wenn die Zeile Null ist, wird kein Fehler ausgegeben.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ");";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Null;

        public override string StartSequence => "(";

        public override string Syntax => "CheckRow(Row);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "checkrow" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var row = Method_Row.ObjectToRow(attvar.Attributes[0]);
            row?.DoAutomatic("script");
            return strDoItFeedback.Null();
        }

        #endregion
    }
}