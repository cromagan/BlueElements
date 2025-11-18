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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Enums;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


public class Method_ImportLinked : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "importlinked";
    public override List<string> Constants => [];
    public override string Description => "Lädt alle verlinkte Zellen mit dem aktuellsten Wert in den Variablen-Speicher.\r\nVorherige Variablen, die über den Befehl geladen wurden, werden gelöscht.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ImportLinked();";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var t = "Befehl: ImportLinked";

        varCol.RemoveWithComment(t);

        #region  Meine Zeile ermitteln (r)

        var r = MyRow(scp);
        if (r?.Table is not { IsDisposed: false } db) { return new DoItFeedback("Zeilenfehler!", true, ld); }

        #endregion

        foreach (var thisColumn in db.Column) {
            if (thisColumn.IsDisposed) { continue; }

            if (thisColumn.RelationType != RelationType.CellValues) { continue; }

            var linkedTable = thisColumn.LinkedTable;
            if (linkedTable is not { IsDisposed: false }) { return new DoItFeedback("Verlinkte Tabelle nicht vorhanden", true, ld); }

            //if (!linkedTable.AreScriptsExecutable()) { return new DoItFeedback("In der Tabelle '" + linkedTable.Caption + "' sind die Skripte defekt", false, ld); }

            var targetColumn = linkedTable.Column[thisColumn.ColumnNameOfLinkedTable];
            if (targetColumn == null) { return new DoItFeedback($"Die verlinkte Spalte {thisColumn.ColumnNameOfLinkedTable} ist in der Zieltabelle {linkedTable.Caption} nicht vorhanden. Auslösende Spalte: {thisColumn.KeyName}", true, ld); }

            var (fc, info) = CellCollection.GetFilterFromLinkedCellData(linkedTable, thisColumn, r, varCol);
            if (fc == null || !string.IsNullOrEmpty(info)) { return new DoItFeedback("Berechnungsfehler im Tabellekopf der verlinkten Zellen: " + info, true, ld); }

            var rows = fc.Rows;
            if (rows.Count > 1) { return new DoItFeedback("Suchergebnis liefert mehrere Ergebnisse.", true, ld); }

            var v = RowItem.CellToVariable(targetColumn, null, true, false);

            if (rows.Count == 1) {
                v = RowItem.CellToVariable(targetColumn, rows[0], true, false);
            }
            v ??= new VariableUnknown("xxx");
            v.KeyName = "Linked_" + thisColumn.KeyName;
            v.Comment = t;
            varCol.Add(v);
        }

        return DoItFeedback.Null();
    }

    #endregion
}