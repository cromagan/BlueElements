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

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_CellSetFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableDouble.ShortName_Plain], StringVal, FilterVar];
    public override string Command => "cellsetfilter";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Tabelle sucht eine Zeile mit einem Filter und setzt den Wert.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nGibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;

    // Manipulates User deswegen, weil dann der eigene Benutzer gesetzt wird und das Extended bearbeitungen auslösen könnte
    public override MethodType MethodType => MethodType.Database | MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellSetFilter(Value, Column, Filter,...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, myDb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var db = allFi.Database;
        if (db is not { IsDisposed: false }) {
            allFi.Dispose();
            return new DoItFeedback("Tabelle verworfen.", true, ld);
        }

        if (db.Column[attvar.ValueStringGet(1)] is not { IsDisposed: false } columnToSet) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(4), true, ld); }

        if (!columnToSet.CanBeChangedByRules()) { return new DoItFeedback("Spalte kann nicht bearbeitet werden: " + attvar.ValueStringGet(4), true, ld); }

        var r = allFi.Rows;
        allFi.Dispose();
        if (r.Count is 0 or > 1) {
            return DoItFeedback.Falsch();
        }

        if (r[0] == MyRow(scp)) {
            return new DoItFeedback("Die eigene Zelle kann nur über die Variablen geändert werden.", true, ld);
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = vl.ValueList.JoinWithCr(); }
        if (attvar.Attributes[0] is VariableDouble vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        r[0].CellSet(columnToSet, value, "Skript: '" + scp.ScriptName + "' aus '" + db.Caption + "'");

        columnToSet.AddSystemInfo("Edit with Script", db, scp.ScriptName);

        return r[0].CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}