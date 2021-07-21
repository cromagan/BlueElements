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

using BlueBasics;
using BlueDatabase;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    public class Method_SetLink : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List, enVariableDataType.String, enVariableDataType.Object };

        public override string Description => "Setzt in der verlinkten Datenbank den Link zur Zelle manuell. Ein Filter kann mit dem Befehl 'Filter' erstellt werden. Gibt zurück, ob der Link erstellt werden konnte.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ");";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "SetLink(ColumnInThisDatabase, ColumnInLinkedDatabase, Filter, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "setlink" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var allFi = new List<FilterItem>();

            for (var z = 3; z < attvar.Attributes.Count; z++) {
                if (!attvar.Attributes[z].ObjectType("rowfilter")) { return new strDoItFeedback("Kein Filter übergeben."); }

                var fi = new BlueDatabase.FilterItem(attvar.Attributes[z].ObjectData());

                if (!fi.IsOk()) { return new strDoItFeedback("Filter fehlerhaft"); }

                if (z > 3) {
                    if (fi.Database != allFi[0].Database) { return new strDoItFeedback("Filter über verschiedene Datenbanken wird nicht unterstützt."); }
                }
                allFi.Add(fi);
            }

            if (allFi.Count < 1) { return new strDoItFeedback("Fehler im Filter"); }

            var returncolumn = allFi[0].Database.Column.Exists(attvar.Attributes[0].ValueString);
            if (returncolumn == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[4].ValueString); }

            var r = RowCollection.MatchesTo(allFi);
            if (r == null || r.Count == 0) {
                attvar.Attributes[1].Readonly = false; // 5 = NothingFoundValue
                attvar.Attributes[1].Type = enVariableDataType.List;
                return new strDoItFeedback(attvar.Attributes[1].ValueString, enVariableDataType.List);
            }
            if (r.Count > 1) {
                attvar.Attributes[2].Readonly = false; // 6 = to MuchFound
                attvar.Attributes[2].Type = enVariableDataType.List;
                return new strDoItFeedback(attvar.Attributes[2].ValueString, enVariableDataType.List);
            }

            var v = RowItem.CellToVariable(returncolumn, r[0]);
            if (v == null || v.Count != 1) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + attvar.Attributes[4].ValueString); }

            v[0].Readonly = false;
            v[0].Type = enVariableDataType.List;
            return new strDoItFeedback(v[0].ValueString, enVariableDataType.List);
        }

        #endregion
    }
}