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

using BlueDatabase;
using Skript.Enums;
using System.Collections.Generic;
using BlueScript.Structuren;

namespace BlueScript {

    public class Method_CellSetFilter : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.Object };

        public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und setzt den Wert. Ein Filter kann mit dem Befehl 'Filter' erstellt werden. Gibt TRUE zurück, wenn der Wert erfolgreich gesetzt wurde.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override string StartSequence => "(";

        public override string Syntax => "CellSetFilter(Value, Column, Filter,...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "cellsetfilter" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2);
            if (allFi is null) { return new strDoItFeedback("Fehler im Filter"); }

            var columnToSet = allFi[0].Database.Column.Exists(attvar.Attributes[1].ValueString);
            if (columnToSet == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[4].ValueString); }

            var r = RowCollection.MatchesTo(allFi);
            if (r == null || r.Count is 0 or > 1) {
                return strDoItFeedback.Falsch();
            }

            r[0].CellSet(columnToSet, attvar.Attributes[0].ValueString);

            return r[0].CellGetString(columnToSet) == attvar.Attributes[0].ValueString ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();
        }

        #endregion
    }
}