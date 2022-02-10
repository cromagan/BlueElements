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
using System.Diagnostics;

namespace BlueScript {

    public class Method_AddRow : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.Bool };

        public override string Description => "Lädt eine andere Datenbank (Database) und erstellt eine neue Zeile. KeyValue muss einen Wert enthalten- zur Not kann UniqueRowId() benutzt werden.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Object;

        public override string StartSequence => "(";

        public override string Syntax => "AddRow(database, keyvalue, startScriptOfNewRow);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "addrow" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var db = DatabaseOf(s, attvar.Attributes[0].ValueString);
            if (db == null) { return new strDoItFeedback("Datenbank '" + attvar.Attributes[0].ValueString + "' nicht gefunden"); }

            if (db.ReadOnly) { return strDoItFeedback.Falsch(); }

            if (string.IsNullOrEmpty(attvar.Attributes[1].ValueString)) { return new strDoItFeedback("KeyValue muss einen Wert enthalten."); }
            //var r = db.Row[attvar.Attributes[1].ValueString];

            //if (r != null && !attvar.Attributes[2].ValueBool) { return Method_Row.RowToObject(r); }

            if (attvar.Attributes[2].ValueBool) {
                StackTrace stackTrace = new();

                if (stackTrace.FrameCount > 400) {
                    return new strDoItFeedback("Stapelspeicherüberlauf");
                }
            }

            var r = db.Row.Add(attvar.Attributes[1].ValueString, attvar.Attributes[2].ValueBool);

            return Method_Row.RowToObjectFeedback(r);
        }

        #endregion
    }
}