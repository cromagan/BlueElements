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

    internal class Method_Sub : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Any };
        public override string Description => "Bezeichnet den Start einer Subroutine.";
        public override bool EndlessArgs => false;
        public override string EndSequence => "()";
        public override bool GetCodeBlockAfter => true;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "";
        public override string Syntax => "Sub SubName() {Code }";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "sub" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }
            if (!Variable.IsValidName(infos.AttributText)) { return new strDoItFeedback(infos.AttributText + " ist kein gültiger Subroutinen-Name."); }

            //Subroutinen werden einfach übersprungen
            s.Line += infos.LineBreakInCodeBlock;
            return new strDoItFeedback(string.Empty);
        }

        #endregion
    }
}