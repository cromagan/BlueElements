// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_Lookup : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, StringVal, StringVal];
    public override string Command => "lookup";
    public override string Description => "Lädt eine andere Datenbank (Database), sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als Liste zurück.\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\nEs ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.\r\nDann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.\r\nÄhnlicher Befehl: CellGetRow";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.MyDatabaseRow | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Lookup(Database, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) {
            return DoItFeedback.AttributFehler(infos.Data, this, attvar);
        }

        var db = DatabaseOf(scp, attvar.ValueStringGet(0));
        if (db == null) {
            return new DoItFeedback(infos.Data, "Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden");
        }

        var c = db.Column[attvar.ValueStringGet(2)];
        if (c == null) {
            return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ValueStringGet(2));
        }

        if (db.Column.First() is not ColumnItem cf) {
            return new DoItFeedback(infos.Data, "Erste Spalte der Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden");
        }

        var r = RowCollection.MatchesTo(new FilterItem(cf, FilterType.Istgleich_GroßKleinEgal, attvar.ValueStringGet(1)));
        var l = new List<string>();

        if (r.Count == 0) {
            l.Add(attvar.ValueStringGet(3));
            return new DoItFeedback(l);
        }
        if (r.Count > 1) {
            l.Add(attvar.ValueStringGet(4));
            return new DoItFeedback(l);
        }

        //var v = RowItem.CellToVariable(c, r[0]);
        //if (v == null || v.Count != 1) {
        //    return new DoItFeedback(infos.Data, "Wert der Variable konnte nicht gelesen werden: " + attvar.ValueStringGet(2));
        //}

        //if (v[0] is VariableListString vl) {
        //    l.AddRange(vl.ValueList);
        //} else if (v[0] is VariableString vs) {
        //    var w = vs.ValueString;
        //    if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        //} else {
        //    return new DoItFeedback(infos.Data, "Spaltentyp nicht unterstützt.");
        //}

        return new DoItFeedback(r[0].CellGetList(c));
    }

    #endregion
}