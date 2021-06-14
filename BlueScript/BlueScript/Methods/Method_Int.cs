#region BlueElements - a collection of useful tools, database and controls

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

#endregion BlueElements - a collection of useful tools, database and controls

using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_Int : Method {
        public override string Syntax => "Int(Number)";
        public override string Description => "Schneidet Nachkommastellen ab. Um einen Text in einen Zahlenwert zu verwandeln, ist der Befehl Number() zu benutzen.";

        public override List<string> Comand(Script s) => new() { "int" };

        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Numeral;
        public override List<enVariableDataType> Args => new() { enVariableDataType.Numeral };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return !string.IsNullOrEmpty(attvar.ErrorMessage)
                ? strDoItFeedback.AttributFehler(this, attvar)
                : new strDoItFeedback(attvar.Attributes[0].ValueInt.ToString(), enVariableDataType.Numeral);
        }
    }
}