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
using static BlueBasics.Extensions;

namespace BlueScript {

    public class Method_LookupExt : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String };

        public override string Description => "Lädt eine andere Datenbank (Database), sucht eine Zeile mit einem Filter (ColumToLook, FilterType, FilteValue) und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben. Aktuell wird nur der FilterTyp 'is' unterstützt. Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.List;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "LookupExt(Database, ColumnToLook, FilterType, KeyValue, ReturnColumn, NothingFoundValue, FoundToMuchValue)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "lookupext" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return new strDoItFeedback("System-Variable 'Filename' nicht gefunden."); }
            var newf = f.ValueString.FilePath() + attvar.Attributes[0].ValueString + ".mdb"; // 0 = Database
            var db2 = BlueBasics.MultiUserFile.clsMultiUserFile.GetByFilename(newf, true);

            Database db;
            if (db2 == null) {
                if (!FileOperations.FileExists(newf)) { return new strDoItFeedback("Datenbank nicht gefunden: " + newf); }
                db = new Database(newf, false, false);
            } else {
                db = (Database)db2;
            }

            var filterColumn = db.Column.Exists(attvar.Attributes[1].ValueString); // 1= filter-Column
            if (filterColumn == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[1].ValueString); }

            var returncolumn = db.Column.Exists(attvar.Attributes[4].ValueString); // 4 = Return-Column
            if (returncolumn == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[4].ValueString); }

            BlueDatabase.Enums.enFilterType filtertype;
            switch (attvar.Attributes[2].ValueString.ToLower()) {
                case "is":
                    filtertype = BlueDatabase.Enums.enFilterType.Istgleich_GroßKleinEgal;
                    break;

                default:
                    return new strDoItFeedback("Filtertype unbekannt: " + attvar.Attributes[2].ValueString);
            }

            var r = RowCollection.MatchesTo(new FilterItem(filterColumn, filtertype, attvar.Attributes[3].ValueString));
            if (r == null || r.Count == 0) {
                if (attvar.Attributes.Count > 3) {
                    attvar.Attributes[5].Readonly = false; // 5 = NothingFoundValue
                    attvar.Attributes[5].Type = enVariableDataType.List;
                    return new strDoItFeedback(attvar.Attributes[5].ValueString, enVariableDataType.List);
                }
                return new strDoItFeedback(string.Empty);
            }
            if (r.Count > 1) {
                if (attvar.Attributes.Count > 4) {
                    attvar.Attributes[6].Readonly = false; // 6 = to MuchFound
                    attvar.Attributes[6].Type = enVariableDataType.List;
                    return new strDoItFeedback(attvar.Attributes[6].ValueString, enVariableDataType.List);
                }
                return new strDoItFeedback(string.Empty);
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