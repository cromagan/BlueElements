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

    public class Method_Filter : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String };

        public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann. Aktuell wird nur der FilterTyp 'is' unterstützt. Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Object;

        public override string StartSequence => "(";

        public override string Syntax => "Filter(Datenbank, Spalte, Filtertyp, Wert)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "filter" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            #region Datenbank

            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return new strDoItFeedback("System-Variable 'Filename' nicht gefunden."); }
            var newf = f.ValueString.FilePath() + attvar.Attributes[0].ValueString + ".mdb";

            var db = Database.GetByFilename(newf, true);
            if (db == null) { return new strDoItFeedback("Datenbank nicht gefunden: " + newf); }

            #endregion

            #region Spalte ermitteln

            var filterColumn = db.Column.Exists(attvar.Attributes[1].ValueString);
            if (filterColumn == null) { new strDoItFeedback("Spalte in Ziel-Datenbank nicht gefunden"); }

            #endregion

            #region Typ ermitteln

            BlueDatabase.Enums.enFilterType filtertype;
            switch (attvar.Attributes[2].ValueString.ToLower()) {
                case "is":
                    filtertype = BlueDatabase.Enums.enFilterType.Istgleich_GroßKleinEgal;
                    break;

                default:
                    return new strDoItFeedback("Filtertype unbekannt: " + attvar.Attributes[2].ValueString);
            }

            #endregion

            var fii = new BlueDatabase.FilterItem(filterColumn, filtertype, attvar.Attributes[3].ValueString);
            return new strDoItFeedback(fii.ToString(true), "rowfilter");
        }

        #endregion
    }
}