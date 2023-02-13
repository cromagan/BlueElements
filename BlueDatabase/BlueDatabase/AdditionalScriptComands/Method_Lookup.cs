// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using System.Collections.Generic;
using BlueDatabase.Enums;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_Lookup : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Lädt eine andere Datenbank (Database), sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    //public Method_Lookup(Script parent) : base(parent) { }
    public override string Syntax => "Lookup(Database, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "lookup" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) {
            return DoItFeedback.AttributFehler(infos, this, attvar);
        }

        var db = DatabaseOf(s.Variables, ((VariableString)attvar.Attributes[0]).ValueString);
        if (db == null) {
            return new DoItFeedback(infos, "Datenbank '" + ((VariableString)attvar.Attributes[0]).ValueString + "' nicht gefunden");
        }

        var c = db.Column.Exists(((VariableString)attvar.Attributes[2]).ValueString);
        if (c == null) {
            return new DoItFeedback(infos, "Spalte nicht gefunden: " + ((VariableString)attvar.Attributes[2]).ValueString);
        }

        var r = RowCollection.MatchesTo(new FilterItem(c, FilterType.Istgleich_GroßKleinEgal, ((VariableString)attvar.Attributes[1]).ValueString));

        if (r == null || r.Count == 0) {
            if (attvar.Attributes.Count > 3) {
                return new DoItFeedback(infos, new List<string> { ((VariableString)attvar.Attributes[3]).ValueString });
            }

            return DoItFeedback.Null(infos);
        }

        if (r.Count > 1) {
            if (attvar.Attributes.Count > 4) {
                return new DoItFeedback(infos, new List<string> { ((VariableString)attvar.Attributes[4]).ValueString });
            }

            return DoItFeedback.Null(infos);
        }

        var v = RowItem.CellToVariable(c, r[0]);
        if (v == null || v.Count != 1) {
            return new DoItFeedback(infos, "Wert konnte nicht erzeugt werden: " + ((VariableString)attvar.Attributes[2]).ValueString);
        }

        if (v[0] is VariableListString vl) { return new DoItFeedback(infos, vl.ValueList); }

        return new DoItFeedback(infos, new List<string> { v[0].ReadableText });
        //return new DoItFeedback(infos, s, new List<string>{kv[0].ReadableText

        //return new DoItFeedback(infos, s, v[0]);
        //return v[0].Type != VariableDataType.List
        //    ? new DoItFeedback(infos, s, v[0].ValueString + "\r", VariableDataType.List)
        //    : new DoItFeedback(infos, s, v[0].ValueString, VariableDataType.List);
    }

    #endregion
}