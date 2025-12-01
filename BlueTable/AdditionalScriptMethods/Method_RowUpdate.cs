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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.Enums;
using System;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;


public class Method_RowUpdate : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [RowVar, FloatVal, FloatVal];

    public List<List<string>> ArgsForButton => [FloatVal, FloatVal];

    public List<string> ArgsForButtonDescription => ["MinAlter", "MaxAlter"];

    public ButtonArgs ClickableWhen => ButtonArgs.Eine_oder_mehr_Zeilen;

    public override string Command => "rowupdate";

    public override List<string> Constants => [];

    public override string Description => "Aktualisiert die Zeile, wenn das alter innerhalb des angegebenen Bereiches ist.\r\n" +
        "Gibt true zurück, wenn die Zeile im Bereich ist oder aktualisiert wurde.\r\n" +
        "Beispiel: RowUpdate(Row,2,10) aktualisiert nur, wenn die Zeile zwischen 2 und 10 Tagen alt ist.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public string NiceTextForUser => "Die gefundenen Zeilen aktualisieren";

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";
    public override string Syntax => "RowUpdate(Row, MinAgeInDays, MaxAgeInDays)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (scp.Stufe > 10) {
            return new DoItFeedback("'RowUpdate' wird zu verschachtelt aufgerufen.", true, ld);
        }

        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        if (row == MyRow(scp)) {
            return new DoItFeedback("Die eigene Zeile kann nicht aktualisiert werden.", true, ld);
        }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return new DoItFeedback($"Zeilen-Status-Spalte in '{tb.KeyName}' nicht gefunden", true, ld); }

        var minage = attvar.ValueNumGet(1);
        var maxage = attvar.ValueNumGet(2);

        if (minage < 0 || minage > maxage) {
            return new DoItFeedback("Die Zeitangaben sind ungültig.", true, ld);
        }

        var myTb = MyTable(scp);
        var cap = myTb?.Caption ?? "Unbekannt";

        var coment = $"Skript-Befehl: 'RowUpdate' der Tabelle {cap}, Skript {scp.ScriptName}";

        var v = row.CellGetDateTime(srs);
        var age = DateTime.UtcNow.Subtract(v).TotalDays;

        if ((age >= minage && age <= maxage) || age > 10000) {
            if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }
            var m = Table.GrantWriteAccess(srs, row, row.ChunkValue, 120, false);
            if (!string.IsNullOrEmpty(m)) { return new DoItFeedback($"Tabellesperre: {m}", false, ld); }
            row.InvalidateRowState(coment);
            var sce = row.UpdateRow(true, coment);

            if (sce.Failed) { return DoItFeedback.Falsch(); }
        }

        return DoItFeedback.Wahr();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => rowarg + ", " + args[0] + ", " + args[1];

    #endregion
}