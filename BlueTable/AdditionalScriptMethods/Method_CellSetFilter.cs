// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public sealed class Method_CellSetFilter : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableDouble.ShortName_Plain], StringVal, FilterVar];
    public static string Command => "cellsetfilter";
    public static List<string> Constants => [];
    public static string Description => "Sucht die Zeile mit dem angegebenen Filter und setzt den Wert.\r\nWerden mehrere Zeilen gefunden, wird der Befehl ignoriert.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nGibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";
    public static bool GetCodeBlockAfter => false;
    public static int LastArgMinCount => 1;

    // Manipulates User deswegen, weil dann der eigene Benutzer gesetzt wird und das Extended bearbeitungen auslösen könnte
    public static MethodType MethodLevel => MethodType.ManipulatesUser;

    public static bool MustUseReturnValue => false;
    public static string Returns => VariableBool.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "CellSetFilter(Value, Column, Filter,...)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var tb = allFi.Table;
        if (tb is not { IsDisposed: false }) {
            allFi.Dispose();
            return new DoItFeedback("Tabelle verworfen.", true, ld);
        }

        if (tb.Column[attvar.ValueStringGet(1)] is not { IsDisposed: false } columnToSet) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(4), true, ld); }

        if (!columnToSet.CanBeChangedByRules()) { return new DoItFeedback("Spalte kann nicht bearbeitet werden: " + attvar.ValueStringGet(4), true, ld); }

        var r = allFi.Rows;
        allFi.Dispose();
        if (r.Count is 0 or > 1) { return DoItFeedback.Falsch(); }

        if (r[0] == MyRow(scp)) {
            return new DoItFeedback("Die eigene Zelle kann nur über die Variablen geändert werden.", true, ld);
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = string.Join('\r', vl.ValueList); }
        if (attvar.Attributes[0] is VariableDouble vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        if (!scp.ProduktivPhase) {
            if (r[0].CellGetString(columnToSet) != value) { return DoItFeedback.TestModusInaktiv(ld); }
            return DoItFeedback.Wahr();
        }

        r[0].CellSet(columnToSet, value, "Skript: '" + scp.ScriptName + "' aus '" + tb.Caption + "'");

        columnToSet.AddSystemInfo("Edit with Script", tb, scp.ScriptName);

        return r[0].CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}