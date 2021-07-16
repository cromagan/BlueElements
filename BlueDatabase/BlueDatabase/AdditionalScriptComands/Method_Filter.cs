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
using BlueDatabase.Enums;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.modConverter;

namespace BlueScript {

    public class Method_Filter : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String };

        public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann. Aktuell wird nur der FilterTyp 'is' unterstützt. Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.String;

        public override string StartSequence => "(";

        public override string Syntax => "Filter(Datenbank, Spalte, Filtertyp, Wert)";

        #endregion

        #region Methods

        public static List<FilterItem> FilterStringToFilterItem(Script s, Variable filterv) {
            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return null; }

            var fi = filterv.Type == enVariableDataType.List
                ? filterv.ValueListString
                : new List<string>() { filterv.ValueString };

            if (fi.Count == 0) { return null; }

            Database db = null;

            var filter = new List<FilterItem>();

            foreach (var thisFilterString in fi) {
                if (!thisFilterString.StartsWith("@Filter;") || !thisFilterString.EndsWith(";@Filter")) { return null; }
                var x = thisFilterString.SplitBy(";");

                if (x.GetUpperBound(0) != 5) { return null; }

                #region Datenbank ermitteln

                var newf = f.ValueString.FilePath() + x[1].FromNonCritical() + ".mdb"; // 0 = Database

                if (db != null) {
                    if (newf.ToLower() != db.Filename.ToLower()) { return null; }
                } else {
                    var db2 = BlueBasics.MultiUserFile.clsMultiUserFile.GetByFilename(newf, true);
                    if (db2 == null) {
                        if (!FileOperations.FileExists(newf)) { return null; }
                        db = new Database(newf, false, false);
                    } else {
                        db = (Database)db2;
                    }
                }

                #endregion

                #region Spalte ermitteln

                var filterColumn = db.Column.Exists(x[2].FromNonCritical());
                if (filterColumn == null) { return null; }

                #endregion

                #region FilterType

                var filtertype = (enFilterType)IntParse(x[3]);

                #endregion

                filter.Add(new FilterItem(filterColumn, filtertype, x[4].FromNonCritical()));
            }

            return filter;
        }

        public override List<string> Comand(Script s) => new() { "filter" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            BlueDatabase.Enums.enFilterType filtertype;
            switch (attvar.Attributes[2].ValueString.ToLower()) {
                case "is":
                    filtertype = BlueDatabase.Enums.enFilterType.Istgleich_GroßKleinEgal;
                    break;

                default:
                    return new strDoItFeedback("Filtertype unbekannt: " + attvar.Attributes[2].ValueString);
            }

            var fi = "@Filter;" +
                    attvar.Attributes[0].ValueString.ToNonCritical() + ";" +
                    attvar.Attributes[1].ValueString.ToNonCritical() + ";" +
                    ((int)filtertype).ToString() + ";" +
                    attvar.Attributes[3].ValueString.ToNonCritical() + ";@Filter";

            return new strDoItFeedback(fi, enVariableDataType.String);
        }

        #endregion
    }
}