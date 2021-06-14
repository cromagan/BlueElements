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
using static BlueBasics.Constants;
using static BlueBasics.modConverter;

namespace BlueScript {

    internal class Method_AddDays : Method {
        public override string Syntax => "AddDays(DateTimeString, Days)";
        public override string Description => "Fügt dem Datum die angegeben Anzahl Tage hinzu. Dabei können auch Gleitkommazahlen benutzt werden, so werden z.B. bei 0.25 nur 6 Stunden hinzugefügt. Der Rückgabwert erfolgt immer im Format " + Format_Date7;

        public override List<string> Comand(Script s) => new() { "adddays" };

        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.Numeral };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            var ok = DateTimeTryParse(attvar.Attributes[0].ValueString, out var d);
            if (!ok) {
                return new strDoItFeedback("Der Wert '" + attvar.Attributes[0].ValueString + "' wurde nicht als Zeitformat erkannt.");
            }
            d = d.AddDays(attvar.Attributes[1].ValueDouble);
            return new strDoItFeedback(d.ToString(Format_Date7), enVariableDataType.String);
        }
    }
}