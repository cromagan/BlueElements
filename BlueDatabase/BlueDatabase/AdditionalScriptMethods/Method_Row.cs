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

    public List<List<string>> ArgsForButton => [];

    public List<string> ArgsForButtonDescription => [];

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

    public static DoItFeedback UniqueRow(VariableCollection varCol, LogData ld, FilterCollection fic, double invalidateinDays, string coment, ScriptProperties scp) {
        RowItem? newrow;
        string message;

        if (invalidateinDays < 0.01) { return new DoItFeedback(ld, "Intervall zu kurz."); }

        if (fic.Database is not { IsDisposed: false } db) { return new DoItFeedback(ld, "Fehler in der Filter"); }
        if (db.Column.SysRowState is not { IsDisposed: false } srs) { return new DoItFeedback(ld, "Zeilen-Status-Spalte nicht gefunden"); }

        foreach (var thisFi in fic) {
            if (thisFi.Column is not { IsDisposed: false } c) {
                return new DoItFeedback(ld, "Fehler im Filter, Spalte ungültig");
            }

            if (thisFi.FilterType is not FilterType.Istgleich and not FilterType.Istgleich_GroßKleinEgal) {
                return new DoItFeedback(ld, "Fehler im Filter, nur 'is' ist erlaubt");
            }

            if (thisFi.SearchValue.Count != 1) {
                return new DoItFeedback(ld, "Fehler im Filter, ein einzelner Suchwert wird benötigt");
            }

            if (FilterCollection.InitValue(c, true, fic.ToArray()) is not { } l) {
                return new DoItFeedback(ld, "Fehler im Filter, dieser Filtertyp kann nicht initialisiert werden.");
            }

            if (thisFi.SearchValue[0] != l) {
                return new DoItFeedback(ld, "Fehler im Filter, Wert '" + thisFi.SearchValue[0] + "' kann nicht gesetzt werden (-> '" + l + "')");
            }
        }

        var t = Stopwatch.StartNew();

        Develop.MonitorMessage?.Invoke(scp.MainInfo, "Skript", $"Parsen: {scp.Chain}\\Row-Befehl: {fic.ReadableText()}", scp.Stufe);

        do {
            (newrow, message, var stoptrying) = RowCollection.UniqueRow(fic, coment);

            if (newrow != null && string.IsNullOrEmpty(message)) { break; }
            if (stoptrying) { break; }
            if (t.Elapsed.TotalMinutes > 5) { break; }
            if (t.Elapsed.TotalSeconds > 10 && !scp.ProduktivPhase) { break; }

            Generic.Pause(5, false);
        } while (true);

        if (!string.IsNullOrEmpty(message)) { return new DoItFeedback(ld, message); }

        if (newrow is { } r) {
            _ = RowCollection.InvalidatedRowsManager.AddInvalidatedRow(r);
            //if (scp.AdditionalInfo is RowItem masterRow && r.Database is { } db) {
            //    masterRow.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, $"Zugehöriger Eintrag: {r.CellFirstString()} ({db.Caption})");
            //}

            var v = r.CellGetDateTime(srs);
            if (DateTime.UtcNow.Subtract(v).TotalDays >= invalidateinDays) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }
                var m = CellCollection.EditableErrorReason(srs, r, EditableErrorReasonType.EditAcut, false, false, true, false, null);
                if (!string.IsNullOrEmpty(m)) {
                    SetNotSuccesful(varCol, "Datenbanksperre: " + m);
                    return RowToObjectFeedback(null);
                }
                r.InvalidateRowState(coment);
            } else {
                Develop.MonitorMessage?.Invoke(scp.MainInfo, "Skript", $"Parsen: {scp.Chain}\\Kein Zeilenupdate, da Zeile aktuell ist.", scp.Stufe);
            }
        }

        return RowToObjectFeedback(newrow);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var mydb = MyDatabase(scp);
        if (mydb == null) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, errorreason) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, mydb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback(ld, $"Filter-Fehler: {errorreason}"); }

        var d = attvar.ValueNumGet(0);

        var fb = UniqueRow(varCol, ld, allFi, d, $"Script-Befehl: 'Row' der Tabelle {mydb.Caption}, Skript {scp.ScriptName}", scp);
        allFi.Dispose();

        return fb;
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => filterarg;

    #endregion
}