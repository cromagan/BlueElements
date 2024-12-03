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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_LookupFilterFirstValue : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, FilterVar];
    public override string Command => "lookupfilterfirstvalue";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück.\r\n\r\nAchtung: Das Laden einer Datenbank kann sehr Zeitintensiv sein.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird der nächstbeste zurückgegeben.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nEs ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.\r\nDann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "LookupFilterFirstValue(ReturnColumn, NothingFoundValue, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyDatabase(scp), scp.ScriptName);

        if (allFi is null) { return new DoItFeedback(ld, "Fehler im Filter"); }

        if (allFi.Database is not { IsDisposed: false } db) { return new DoItFeedback(ld, "Datenbankfehler!"); }

        var returncolumn = db.Column[attvar.ValueStringGet(0)];
        if (returncolumn == null) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.ValueStringGet(0)); }

        var l = new List<string>();

        var r = allFi.Rows;
        if (r.Count == 0) {
            l.Add(attvar.ValueStringGet(1));
            return new DoItFeedback(l);
        }

        var v = RowItem.CellToVariable(returncolumn, r[0], true, false);
        if (v == null) { return new DoItFeedback(ld, $"Wert der Variable konnte nicht gelesen werden - ist die Spalte {returncolumn.KeyName} 'im Skript vorhanden'?"); }
        if (v is VariableListString vl) {
            l.AddRange(vl.ValueList);
        } else if (v is VariableString vs) {
            var w = vs.ValueString;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else if (v is VariableFloat vf) {
            var w = vf.ValueForReplace;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else {
            return new DoItFeedback(ld, "Spaltentyp nicht unterstützt.");
        }

        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        return new DoItFeedback(l);
    }

    #endregion
}