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
public class Method_CellSetRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableDouble.ShortName_Plain], StringVal, RowVar];
    public override string Command => "cellsetrow";
    public override List<string> Constants => [];
    public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;

    // Manipulates User deswegen, weil dann der eigene Benutzer gesetzt wird und das Extended bearbeitungen auslösen könnte
    public override MethodType MethodLevel => MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellSetRow(Value, Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } myTb) { return DoItFeedback.InternerFehler(ld); }
        if (attvar.ValueRowGet(2) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }
        if (tb != myTb && !tb.IsThisScriptBroken(BlueBasics.Enums.ScriptEventTypes.value_changed, true)) { return new DoItFeedback($"In der Tabelle '{tb.Caption}' sind die Skripte defekt", false, ld); }

        var columnToSet = tb.Column[attvar.ValueStringGet(1)];
        if (columnToSet == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(1), true, ld); }

        if (row == MyRow(scp)) {
            return new DoItFeedback("Die eigene Zelle kann nur über die Variablen geändert werden.", true, ld);
        }

        if (!columnToSet.CanBeChangedByRules()) {
            return new DoItFeedback("Spalte kann nicht bearbeitet werden: " + attvar.ValueStringGet(1), true, ld);
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = vl.ValueList.JoinWithCr(); }
        if (attvar.Attributes[0] is VariableDouble vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        var newchunkval = row.ChunkValue;

        if (columnToSet == tb.Column.ChunkValueColumn) { newchunkval = value; }

        var m = CellCollection.GrantWriteAccess(columnToSet, row, newchunkval, 120, false);
        if (!string.IsNullOrEmpty(m)) { return DoItFeedback.Falsch(); }

        if (!scp.ProduktivPhase) {
            if (row.CellGetString(columnToSet) != value) { return DoItFeedback.TestModusInaktiv(ld); }
            return DoItFeedback.Wahr();
        }

        row.CellSet(columnToSet, value, "Skript: '" + scp.ScriptName + "' aus '" + tb.Caption + "'");
        columnToSet.AddSystemInfo("Edit with Script", tb, scp.ScriptName);

        return row.CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}