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

using BlueBasics;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;


public class Method_AddRows : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, FloatVal, ListStringVar, FilterVar];
    public override string Command => "addrows";
    public override List<string> Constants => [];

    public override string Description => "Erstellt mehrere neue Zeilen.\r\n" +
                                          "Es werden nur neue Zeilen erstellt, die nicht vorhanden sind.\r\n" +
                                          "Ist sie bereits mehrfach vorhanden, werden diese zusammengefasst (maximal 5).\r\n" +
                                          "Leere KeyValues werden übersprungen.\r\n" +
                                          "Die Werte der Filter werden zusätzlich gesetzt.\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen\r\n" +
        "Mit AgeInDay kann angebeben werden, ab welchen Alter eine gefundene Zeile invalidiert werden soll.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 0;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.

    public override MethodType MethodLevel => MethodType.ManipulatesUser;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "AddRows(table, AgeInDays keyvalues, filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var myTb = MyTable(scp);
        var cap = myTb?.Caption ?? "Unbekannt";

        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }

        if (!tb.IsThisScriptBroken(BlueBasics.Enums.ScriptEventTypes.InitialValues, true)) { return new DoItFeedback($"In der Tabelle '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false, ld); }

        if (!tb.IsEditable(false)) { return new DoItFeedback($"Tabellesperre: {tb.IsNotEditableReason(false)}", false, ld); }

        var keys = attvar.ValueListStringGet(2);
        keys = keys.SortedDistinctList();

        StackTrace stackTrace = new();
        if (stackTrace.FrameCount > 400) {
            return new DoItFeedback("Stapelspeicherüberlauf", true, ld);
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (tb.Column.First is not { IsDisposed: false } c) { return new DoItFeedback("Erste Spalte nicht vorhanden", true, ld); }

        var d = attvar.ValueNumGet(1);

        foreach (var thisKey in keys) {

            #region  Filter ermitteln (allfi)

            var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 3, myTb, scp.ScriptName, false);
            if (!string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

            allFi ??= new FilterCollection(tb, "AddRows");

            #endregion

            if (allFi[c] is { }) {
                allFi.Dispose();
                return new DoItFeedback("Initialwert doppelt belegt", true, ld);
            }

            allFi.Add(new(c, FilterType.Istgleich_GroßKleinEgal, thisKey));

            var scx = Method_Row.UniqueRow(allFi, d, $"Skript-Befehl: 'AddRows' der Tabelle {cap}, Skript {scp.ScriptName}", scp, ld);
            allFi.Dispose();
            if (scx.Failed) { return scx; }
        }

        return DoItFeedback.Null();
    }

    #endregion
}