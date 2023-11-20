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

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

public class Method_LookupFilter : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, StringVal, FilterVar };
    public override string Command => "lookupfilter";
    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück.\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nEs ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.\r\nDann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.\r\nÄhnlicher Befehl: CellGetRow";
    public override bool EndlessArgs => true;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "LookupFilter(ReturnColumn, NothingFoundValue, FoundToMuchValue, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 3);

        if (allFi is null) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        if (allFi.Database is not DatabaseAbstract db) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var returncolumn = db.Column.Exists(attvar.ValueStringGet(0));
        if (returncolumn == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ValueStringGet(0)); }

        var l = new List<string>();

        var r = allFi.Rows;
        if (r.Count == 0) {
            l.Add(attvar.ValueStringGet(1));
            return new DoItFeedback(l);
        }
        if (r.Count > 1) {
            l.Add(attvar.ValueStringGet(2));
            return new DoItFeedback(l);
        }

        var v = RowItem.CellToVariable(returncolumn, r[0]);
        if (v == null || v.Count != 1) { return new DoItFeedback(infos.Data, "Wert der Variable konnte nicht gelesen werden: " + attvar.ValueStringGet(4)); }

        if (v[0] is VariableListString vl) {
            l.AddRange(vl.ValueList);
        } else if (v[0] is VariableString vs) {
            var w = vs.ValueString;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else {
            return new DoItFeedback(infos.Data, "Spaltentyp nicht unterstützt.");
        }

        return new DoItFeedback(l);
    }

    #endregion
}