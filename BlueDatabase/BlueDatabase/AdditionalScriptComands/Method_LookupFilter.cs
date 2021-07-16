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

    public class Method_LookupFilter : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String_or_List, enVariableDataType.String, enVariableDataType.String, enVariableDataType.String };

        public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben. Ein Filter kann mit dem Befehl 'Filter' erstellt werden.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.List;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "LookupFilter(Filter, ReturnColumn, NothingFoundValue, FoundToMuchValue)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "lookupfilter" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var fi = Method_Filter.FilterStringToFilterItem(s, attvar.Attributes[0]);
            if (fi == null || fi.Count < 1) { return new strDoItFeedback("Fehler im Filter"); }

            var returncolumn = fi[0].Database.Column.Exists(attvar.Attributes[1].ValueString); // 4 = Return-Column
            if (returncolumn == null) { return new strDoItFeedback("Spalte nicht gefunden: " + attvar.Attributes[4].ValueString); }

            var r = RowCollection.MatchesTo(fi);
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

            //var v = RowItem.CellToVariable(returncolumn, r[0]);
            //if (v == null || v.Count != 1) { return new strDoItFeedback("Wert konnte nicht erzeugt werden: " + attvar.Attributes[4].ValueString); }

            //v[0].Readonly = false;
            //v[0].Type = enVariableDataType.List;
            //return new strDoItFeedback(v[0].ValueString, enVariableDataType.List);
        }

        #endregion
    }
}