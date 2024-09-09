// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueScript;
using BlueScript.Enums;
using BlueDatabase.Interfaces;
using BlueDatabase.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics;
using BlueBasics;

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

    public static DoItFeedback UniqueRow(LogData ld, FilterCollection fic, string coment) {
        RowItem? newrow;
        string message;
        var t = Stopwatch.StartNew();

        do {
            (newrow, message) = RowCollection.UniqueRow(fic, coment);

            if (newrow != null && string.IsNullOrEmpty(message)) { break; }
            if (t.Elapsed.TotalMinutes > 5) { break; }
            Generic.Pause(5, false);
        } while (true);

        if (!string.IsNullOrEmpty(message)) { return new DoItFeedback(ld, message); }

        return Method_Row.RowToObjectFeedback(newrow);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var mydb = MyDatabase(scp);
        if (mydb == null) { return new DoItFeedback(ld, "Interner Fehler"); }

        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null || allFi.Count == 0) {
            return new DoItFeedback(ld, "Fehler im Filter");
        }

        foreach (var thisFi in allFi) {
            if (thisFi.Column is not { IsDisposed: false } c) {
                return new DoItFeedback(ld, "Fehler im Filter, Spalte ungültig");
            }

            if (thisFi.FilterType is not FilterType.Istgleich and not FilterType.Istgleich_GroßKleinEgal) {
                return new DoItFeedback(ld, "Fehler im Filter, nur 'is' ist erlaubt");
            }

            if (thisFi.SearchValue.Count != 1) {
                return new DoItFeedback(ld, "Fehler im Filter, ein einzelner Suchwert wird benötigt");
            }
            var l = allFi.InitValue(c, true);
            if (thisFi.SearchValue[0] != l) {
                return new DoItFeedback(ld, "Fehler im Filter, Wert '" + thisFi.SearchValue[0] + "' kann nicht gesetzt werden (-> '" + l + "')");
            }
        }

        //var r = allFi.Rows;

        //if (r.Count > 1) {
        //    q
        //    return new DoItFeedback(ld, "RowUnique gescheitert, da bereits mehrere Zeilen vorhanden sind: " + allFi.ReadableText());
        //}

        //if (r.Count == 0) {
        //    if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zeile anlegen im Testmodus deaktiviert."); }
        //    var nr = RowCollection.GenerateAndAdd(allFi, "Script-Befehl: 'RowUnique' von " + mydb.Caption);
        //    if (nr.newrow == null) { return new DoItFeedback(ld, "Neue Zeile konnte nicht erstellt werden: " + nr.message); }
        //    return Method_Row.RowToObjectFeedback(nr.newrow);
        //}

        return UniqueRow(ld, allFi, $"Script-Befehl: 'RowUnique' der Tabelle {mydb.Caption}, Skript {scp.ScriptName}");
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => filterarg;

    #endregion
}