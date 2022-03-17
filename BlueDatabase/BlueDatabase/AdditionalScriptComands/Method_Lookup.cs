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

using System.Collections.Generic;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands {

    public class Method_Lookup : MethodDatabase {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String, VariableDataType.String, VariableDataType.String, VariableDataType.String, VariableDataType.String };

        public override string Description => "Lädt eine andere Datenbank (Database), sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override VariableDataType Returns => VariableDataType.List;

        public override string StartSequence => "(";

        //public Method_Lookup(Script parent) : base(parent) { }
        public override string Syntax => "Lookup(Database, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "lookup" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) {
                return DoItFeedback.AttributFehler(this, attvar);
            }

            var db = DatabaseOf(s, ((VariableString)attvar.Attributes[0]).ValueString);
            if (db == null) {
                return new DoItFeedback("Datenbank '" + ((VariableString)attvar.Attributes[0]).ValueString +
                                        "' nicht gefunden");
            }

            var c = db.Column.Exists(((VariableString)attvar.Attributes[2]).ValueString);
            if (c == null) {
                return new DoItFeedback("Spalte nicht gefunden: " +
                                        ((VariableString)attvar.Attributes[2]).ValueString);
            }

            var r = RowCollection.MatchesTo(new FilterItem(db.Column[0], Enums.FilterType.Istgleich_GroßKleinEgal,
                ((VariableString)attvar.Attributes[1]).ValueString));
            if (r == null || r.Count == 0) {
                if (attvar.Attributes.Count > 3) {
                    return new DoItFeedback(new List<string> { ((VariableString)attvar.Attributes[3]).ValueString });
                }

                return new DoItFeedback(string.Empty);
            }

            if (r.Count > 1) {
                if (attvar.Attributes.Count > 4) {
                    return new DoItFeedback(new List<string> { ((VariableString)attvar.Attributes[4]).ValueString });
                }

                return new DoItFeedback(string.Empty);
            }

            var v = RowItem.CellToVariable(c, r[0]);
            if (v == null || v.Count != 1) {
                return new DoItFeedback("Wert konnte nicht erzeugt werden: " +
                                        ((VariableString)attvar.Attributes[2]).ValueString);
            }

            if (v[0] is VariableListString vl) { return new DoItFeedback(vl.ValueList); }

            return new DoItFeedback(new List<string> { v[0].ReadableText });
            //return new DoItFeedback(new List<string>{kv[0].ReadableText

            //return new DoItFeedback(v[0]);
            //return v[0].Type != VariableDataType.List
            //    ? new DoItFeedback(v[0].ValueString + "\r", VariableDataType.List)
            //    : new DoItFeedback(v[0].ValueString, VariableDataType.List);
        }

        #endregion
    }
}