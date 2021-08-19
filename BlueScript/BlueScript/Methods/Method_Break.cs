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

using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_Break : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { };

        public override string Description => "Beendet eine Schleife sofort. Kann auch nur innerhalb von Schleifen erwendet werden.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ";";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Null;

        public override string StartSequence => "";

        public override string Syntax => "Break;";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "break" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            if (s.Schleife < 1) { return new strDoItFeedback("Break nur innerhalb einer Schleife erlaubt."); }

            if (s.BreakFired) { return new strDoItFeedback("Break doppelt ausgelöst."); }
            s.BreakFired = true;
            return new strDoItFeedback();
        }

        #endregion
    }
}