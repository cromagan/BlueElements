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
using System.Diagnostics;
using System.Linq;
using BlueBasics;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowUnique : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public List<List<string>> ArgsForButton => [];

    public List<string> ArgsForButtonDescription => [];

    public ButtonArgs ClickableWhen => ButtonArgs.Keine_Zeile;

    public override string Command => "rowunique";

    public override List<string> Constants => [];

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                              "Wird keine Zeile gefunden, wird eine neue Zeile erstellt.\r\n" +
                                          "Ist sie bereits mehrfach vorhanden, werden diese zusammengefasst (maximal 5!).\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.
    public override MethodType MethodType => MethodType.Database | MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public string NiceTextForUser => "Eine neue Zeile mit den eingehenden Filterwerten anlegen, wenn diese noch nicht vorhanden ist.";

    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";
    public override string Syntax => "RowUnique(Filter, ...)";

    #endregion

    #region Methods

    public static DoItFeedback UniqueRow(LogData ld, FilterCollection fic, string coment, ScriptProperties scp) {
        RowItem? newrow;
        string message;
        var t = Stopwatch.StartNew();

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
            RowCollection.InvalidatedRows.AddIfNotExists(newrow);
            if (scp.AdditionalInfo is RowItem masterRow && r.Database is { } db) {
                masterRow.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, $"Zugehöriger Eintrag: {r.CellFirstString()} ({db.Caption})");
            }
        }
        return Method_Row.RowToObjectFeedback(newrow);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var mydb = MyDatabase(scp);
        if (mydb == null) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, errorreason) = Method_Filter.ObjectToFilter(attvar.Attributes, 0, mydb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback(ld, $"Filter-Fehler: {errorreason}"); }

        foreach (var thisFi in allFi) {
            if (thisFi.Column is not { IsDisposed: false } c) {
                allFi.Dispose();
                return new DoItFeedback(ld, "Fehler im Filter, Spalte ungültig");
            }

            if (thisFi.FilterType is not FilterType.Istgleich and not FilterType.Istgleich_GroßKleinEgal) {
                allFi.Dispose();
                return new DoItFeedback(ld, "Fehler im Filter, nur 'is' ist erlaubt");
            }

            if (thisFi.SearchValue.Count != 1) {
                allFi.Dispose();
                return new DoItFeedback(ld, "Fehler im Filter, ein einzelner Suchwert wird benötigt");
            }
            var l = FilterCollection.InitValue(c, true, allFi.ToArray());
            if (thisFi.SearchValue[0] != l) {
                allFi.Dispose();
                return new DoItFeedback(ld, "Fehler im Filter, Wert '" + thisFi.SearchValue[0] + "' kann nicht gesetzt werden (-> '" + l + "')");
            }
        }

        var fb = UniqueRow(ld, allFi, $"Script-Befehl: 'RowUnique' der Tabelle {mydb.Caption}, Skript {scp.ScriptName}", scp);
        allFi.Dispose();
        return fb;
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => filterarg;

    #endregion
}