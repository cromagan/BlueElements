// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_FilterAllValues : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, FilterVar];
    public override string Command => "filterallvalues";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Tabelle sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück.\r\n\r\nBei Sort True  werden alle Suchergebnisse kombiniert, gemischt und sortiert.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nEs ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.\r\nDann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "FilterAllValues(ReturnColumn, Sort, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        if (allFi.Table is not { IsDisposed: false } db) {
            allFi.Dispose();
            return new DoItFeedback("Tabellefehler!", true, ld);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = db.Column[attvar.ValueStringGet(0)];
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true, ld); }
        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        List<string> list = [];
        foreach (var row in r) { list.AddRange(row.CellGetList(returncolumn)); }
        if (attvar.ValueBoolGet(1)) { list = list.SortedDistinctList(); }

        return new DoItFeedback(list);
    }

    #endregion
}