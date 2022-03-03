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

namespace BlueScript {

    public class Method_Lookup : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String };

        public override string Description => "Lädt eine andere Datenbank (Database), sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.List;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "Lookup(Database, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "lookup" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var db = DatabaseOf(s, attvar.Attributes[0].ValueString);
            if (db == null) { return new strDoItFeedback("Datenbank '" + attvar.Attributes[0].ValueString + "' nicht gefunden"); }

            var c = db.Column.Exists(attvar.Attributes[2].ValueString);
            if (c == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[2].ValueString); }

            var r = RowCollection.MatchesTo(new FilterItem(db.Column[0], BlueDatabase.Enums.enFilterType.Istgleich_GroßKleinEgal, attvar.Attributes[1].ValueString));
            if (r == null || r.Count == 0) {
                if (attvar.Attributes.Count > 3) {
                    attvar.Attributes[3].Readonly = false;
                    attvar.Attributes[3].Type = enVariableDataType.List;
                    return new strDoItFeedback(attvar.Attributes[3].ValueString + "\r", enVariableDataType.List);
                }
                return new strDoItFeedback(string.Empty);
            }
            if (r.Count > 1) {
                if (attvar.Attributes.Count > 4) {
                    attvar.Attributes[4].Readonly = false;
                    attvar.Attributes[4].Type = enVariableDataType.List;
                    return new strDoItFeedback(attvar.Attributes[4].ValueString + "\r", enVariableDataType.List);
                }
                return new strDoItFeedback(string.Empty);
            }

            var v = RowItem.CellToVariable(c, r[0]);
            if (v == null || v.Count != 1) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + attvar.Attributes[2].ValueString); }

            return v[0].Type != enVariableDataType.List
                ? new strDoItFeedback(v[0].ValueString + "\r", enVariableDataType.List)
                : new strDoItFeedback(v[0].ValueString, enVariableDataType.List);
        }

        #endregion
    }
}