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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_LookupFilterAllValues : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, FilterVar];
    public override string Command => "lookupfilterallvalues";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück.\r\n\r\nAchtung: Das Laden einer Datenbank kann sehr Zeitintensiv sein.\r\n\r\nDabei werdenn alles Suchergebisse kombiniert, gemischt und sortiert.\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nEs ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.\r\nDann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "LookupFilterAllValues(ReturnColumn, NothingFoundValue, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyDatabase(scp), scp.ScriptName);

        if (allFi is null) { return new DoItFeedback(ld, "Fehler im Filter"); }

        if (allFi.Database is not { IsDisposed: false } db) { return new DoItFeedback(ld, "Datenbankfehler!"); }

        var returncolumn = db.Column[attvar.ValueStringGet(0)];
        if (returncolumn == null) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.ValueStringGet(0)); }

        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        var l = new List<string>();

        var r = allFi.Rows;
        if (r.Count == 0) {
            l.Add(attvar.ValueStringGet(1));
            return new DoItFeedback(l);
        }

        List<string> list = [];
        foreach (var row in r) {
            list.AddRange(row.CellGetList(returncolumn));
        }

        return new DoItFeedback(list.SortedDistinctList());
    }

    #endregion
}