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

    public class Method_LookupFilter : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.String, enVariableDataType.String, enVariableDataType.Object };

        public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben. Ein Filter kann mit dem Befehl 'Filter' erstellt werden.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.List;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "LookupFilter(ReturnColumn, NothingFoundValue, FoundToMuchValue, Filter, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "lookupfilter" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 3);

            if (allFi is null) { return new strDoItFeedback("Fehler im Filter"); }

            var returncolumn = allFi[0].Database.Column.Exists(attvar.Attributes[0].ValueString);
            if (returncolumn == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[0].ValueString); }

            var r = RowCollection.MatchesTo(allFi);
            if (r == null || r.Count == 0) {
                attvar.Attributes[1].Readonly = false; // 5 = NothingFoundValue
                attvar.Attributes[1].Type = enVariableDataType.List;
                return new strDoItFeedback(attvar.Attributes[1].ValueString + "\r", enVariableDataType.List);
            }
            if (r.Count > 1) {
                attvar.Attributes[2].Readonly = false; // 6 = to MuchFound
                attvar.Attributes[2].Type = enVariableDataType.List;
                return new strDoItFeedback(attvar.Attributes[2].ValueString + "\r", enVariableDataType.List);
            }

            var v = RowItem.CellToVariable(returncolumn, r[0]);
            if (v == null || v.Count != 1) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + attvar.Attributes[4].ValueString); }

            if (v[0].Type != enVariableDataType.List) {
                return new strDoItFeedback(v[0].ValueString + "\r", enVariableDataType.List);
            }
            return new strDoItFeedback(v[0].ValueString, enVariableDataType.List);
        }

        #endregion
    }
}