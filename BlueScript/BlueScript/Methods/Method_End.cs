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

    internal class Method_End : Method {

        #region Properties

        public override List<enVariableDataType> Args => new();

        public override string Description => "Beendet das Skript ohne Fehler.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ";";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Null;

        public override string StartSequence => "";

        //public Method_var(Script parent) : base(parent) { }
        public override string Syntax => "End;";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "end" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            s.EndSkript = true;
            return strDoItFeedback.Null();
        }

        #endregion
    }
}