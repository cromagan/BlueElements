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
using BlueBasics.Enums;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_Row : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [FloatVal, FilterVar];

    public List<List<string>> ArgsForButton => [FloatVal];

    public List<string> ArgsForButtonDescription => ["Invalidieren nach X Tagen"];

    public ButtonArgs ClickableWhen => ButtonArgs.Keine_Zeile;

    public override string Command => "row";

    public override List<string> Constants => [];

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                              "Wird keine Zeile gefunden, wird eine neue Zeile erstellt.\r\n" +
                                          "Ist sie bereits mehrfach vorhanden, werden diese zusammengefasst (maximal 5!).\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen.\r\n" +
        "Mit AgeInDay kann angebeben werden, ab welchen Alter eine gefundene Zeile invalidiert werden soll.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.
    public override MethodType MethodType => MethodType.Database | MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public string NiceTextForUser => "Eine neue Zeile mit den eingehenden Filterwerten anlegen, wenn diese noch nicht vorhanden ist.";

    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "Row(AgeInDays, Filter, ...)";

    #endregion

    #region Methods

    public static RowItem? ObjectToRow(Variable? attribute) => attribute is not VariableRowItem vro ? null : vro.RowItem;

    public static DoItFeedback RowToObjectFeedback(RowItem? row) => new(new VariableRowItem(row));

    public static DoItFeedback UniqueRow(FilterCollection fic, double invalidateinDays, string coment, ScriptProperties scp, LogData ld) {
        if (invalidateinDays < 0.01) { return new DoItFeedback("Intervall zu kurz.", true, ld); }

        if (fic.Database is not { IsDisposed: false } db) { return new DoItFeedback("Fehler in der Filter", true, ld); }
        if (db.Column.SysRowState is not { IsDisposed: false } srs) { return new DoItFeedback("Zeilen-Status-Spalte nicht gefunden", true, ld); }

        foreach (var thisFi in fic) {
            if (thisFi.Column is not { IsDisposed: false } c) {
                return new DoItFeedback("Fehler im Filter, Spalte ungültig", true, ld);
            }

            //if (thisFi.FilterType is not FilterType.Istgleich and not FilterType.Istgleich_GroßKleinEgal) {
            //    return new DoItFeedback("Fehler im Filter, nur 'is' ist erlaubt", true, ld);
            //}

            //if (thisFi.SearchValue.Count != 1) {
            //    return new DoItFeedback("Fehler im Filter, ein einzelner Suchwert wird benötigt", true, ld);
            //}

            if (FilterCollection.InitValue(c, true, fic.ToArray()) is not { } l) {
                return new DoItFeedback("Fehler im Filter, dieser Filtertyp kann nicht initialisiert werden.", true, ld);
            }

            if (thisFi.SearchValue[0] != l) {
                return new DoItFeedback("Fehler im Filter, Wert '" + thisFi.SearchValue[0] + "' kann nicht gesetzt werden (-> '" + l + "')", true, ld);
            }
        }

        Develop.Message?.Invoke(BlueBasics.Enums.ErrorType.Info, null,  scp.MainInfo, "Skript", $"Parsen: {scp.Chain}\\Row-Befehl: {fic.ReadableText()}", scp.Stufe);

        RowItem? newrow;

        if (scp.ProduktivPhase) {
            var t = Stopwatch.StartNew();
            string message;
            do {
                (newrow, message, var stoptrying) = RowCollection.UniqueRow(fic, coment);

                if (newrow != null && string.IsNullOrEmpty(message)) { break; }
                if (stoptrying) { break; }
                if (t.Elapsed.TotalMinutes > 5) { break; }
                if (t.Elapsed.TotalSeconds > 12 && !scp.ProduktivPhase) { break; }

                Generic.Pause(5, false);
            } while (true);

            t.Stop();
            if (!string.IsNullOrEmpty(message)) { return new DoItFeedback(message, false, ld); }
        } else {
            if (fic.Rows.Count != 1) { return DoItFeedback.TestModusInaktiv(ld); }
            newrow = fic.Rows[0];
        }

        if (newrow is { IsDisposed: false } r) {
            var v = r.CellGetDateTime(srs);
            if (DateTime.UtcNow.Subtract(v).TotalDays >= invalidateinDays) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }
                var m = CellCollection.GrantWriteAccess(srs, r, fic.ChunkVal, 60, false);
                if (!string.IsNullOrEmpty(m)) { return new DoItFeedback($"Datenbanksperre: {m}", false, ld); }
                r.InvalidateRowState(coment);
            } else {
                Develop.Message?.Invoke(ErrorType.Info, null, scp.MainInfo, "Skript", $"Parsen: {scp.Chain}\\Kein Zeilenupdate ({r.CellFirstString()}, {r.Database?.Caption ?? "?"}), da Zeile aktuell ist.", scp.Stufe);
            }
        } else {
            return new DoItFeedback("Zeile konnte nicht angelegt werden", false, ld);
        }

        return RowToObjectFeedback(newrow);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, myDb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var d = attvar.ValueNumGet(0);

        var fb = UniqueRow(allFi, d, $"Script-Befehl: 'Row' der Tabelle {myDb.Caption}, Skript {scp.ScriptName}", scp, ld);
        allFi.Dispose();

        return fb;
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0] + "," + filterarg;

    #endregion
}