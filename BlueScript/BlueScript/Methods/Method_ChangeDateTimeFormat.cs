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
#endregion

using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Constants;
using static BlueBasics.modConverter;

namespace BlueScript {
    internal class Method_ChangeDateTimeFormat : Method {

        public override string Syntax => "ChangeDateTimeFormat(DateTimeString, string)";

        public override string Description => "Gibt einen neuen DateTime-String zurück, der mittels des zweiten String definiert ist. Beispiel eine solchen Strings:  " + Format_Date7 + " Achtung: Groß-Kleinschreibung ist wichtig!";
        public override List<string> Comand(Script s) => new() { "changedatetimeformat" };
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var ok = DateTimeTryParse(attvar.Attributes[0].ValueString, out var d);
            if (!ok) {
                return new strDoItFeedback("Der Wert '" + attvar.Attributes[0].ValueString + "' wurde nicht als Zeitformat erkannt.");
            }

            if (string.IsNullOrEmpty(d.ToString(attvar.Attributes[1].ValueString))) {
                return new strDoItFeedback("Kein Unwandlungs-String erhalten.");
            }

            try {
                return new strDoItFeedback(d.ToString(attvar.Attributes[1].ValueString), enVariableDataType.String);
            } catch {
                return new strDoItFeedback("Der Umwandlungs-String '" + attvar.Attributes[1].ValueString + "' ist fehlerhaft.");
            }
        }
    }
}
