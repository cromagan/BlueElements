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

using BlueBasics.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowUniqueAndUpdade : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, FilterVar];

    public List<List<string>> ArgsForButton => [];

    public List<string> ArgsForButtonDescription => [];

    public ButtonArgs ClickableWhen => ButtonArgs.Keine_Zeile;

    public override string Command => "rowuniqueandupdate";

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                          "Wird keine Zeile gefunden, wird eine neue Zeile erstellt.\r\n" +
                                          "Werden mehrere Zeilen gefunden, werden diese zusammengefasst (maximal 5!).\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen\r\n" +
                                          "Zusätzlich wird die Zeile komplett neu durchgerechnet:\r\nNeueZeile, AngegebenesSkript, KeyValueScript, ValueChangesScript und FormularVorbereitenSkript";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.Database | MethodType.IO | MethodType.NeedLongTime;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public string NiceTextForUser => "Eine neue Zeile mit den eingehenden Filterwerten anlegen und durchrechnen";

    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "RowUniqueAndUpdate(ZusätzlichesSkript, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1);
        if (allFi is null || allFi.Count == 0) {
            return new DoItFeedback(infos.Data, "Fehler im Filter");
        }

        foreach (var thisFi in allFi) {
            if (thisFi.Column is not ColumnItem c) {
                return new DoItFeedback(infos.Data, "Fehler im Filter, Spalte ungültig");
            }

            if (thisFi.FilterType is not Enums.FilterType.Istgleich and not Enums.FilterType.Istgleich_GroßKleinEgal) {
                return new DoItFeedback(infos.Data, "Fehler im Filter, nur 'is' ist erlaubt");
            }

            if (thisFi.SearchValue.Count != 1) {
                return new DoItFeedback(infos.Data, "Fehler im Filter, ein einzelner Suchwert wird benötigt");
            }
            var l = allFi.InitValue(c, true);
            if (thisFi.SearchValue[0] != l) {
                return new DoItFeedback(infos.Data, "Fehler im Filter, Wert '" + thisFi.SearchValue[0] + "' kann nicht gesetzt werden (-> '" + l + "')");
            }
        }

        var r = allFi.Rows;

        if (r.Count > 5) {
            return new DoItFeedback(infos.Data, "RowUniqueAndUpdate gescheitert, da bereits zu viele Zeilen vorhanden sind: " + allFi.ReadableText());
        }

        if (r.Count > 1) {
            if (!scp.ProduktivPhase) { return new DoItFeedback(infos.Data, "Zeile anlegen im Testmodus deaktiviert."); }

            r[0].Database?.Row.Combine(r);
            r[0].Database?.Row.RemoveYoungest(r, true);
            r = allFi.Rows;
            if (r.Count > 1) {
                return new DoItFeedback(infos.Data, "RowUniqueAndUpdate gescheitert, Aufräumen fehlgeschlagen: " + allFi.ReadableText());
            }
        }

        RowItem? myRow = null;

        if (r.Count == 0) {
            var mydb = MyDatabase(scp);
            if (mydb == null) { return new DoItFeedback(infos.Data, "Interner Fehler"); }
            if (!scp.ProduktivPhase) { return new DoItFeedback(infos.Data, "Zeile anlegen im Testmodus deaktiviert."); }
            var nr = RowCollection.GenerateAndAdd(allFi, "Script-Befehl: 'RowUniqueAndUpdate' von " + mydb.Caption);
            if (nr.newrow == null) { return new DoItFeedback(infos.Data, "Neue Zeile konnte nicht erstellt werden: " + nr.message); }
            myRow = nr.newrow;
        } else {
            myRow = r[0];

            var ok1 = myRow.ExecuteScript(ScriptEventTypes.new_row, string.Empty, true, true, true, 2, null, true, true);

            if (!ok1.AllOk) { { return new DoItFeedback(infos.Data, "RowUniqueAndUpdate fehlgeschlagen bei Skript: NewRow."); } }
        }

        var scriptn = attvar.ValueStringGet(0);
        if (!string.IsNullOrEmpty(scriptn)) {
            var ok1 = myRow.ExecuteScript(null, scriptn, true, true, true, 2, null, true, true);
            if (!ok1.AllOk) { { return new DoItFeedback(infos.Data, $"RowUniqueAndUpdate fehlgeschlagen bei Skript: {scriptn}."); } }
        }

        var ok = myRow.ExecuteScript(ScriptEventTypes.keyvalue_changed, string.Empty, true, true, true, 2, null, true, true);
        if (!ok.AllOk) { { return new DoItFeedback(infos.Data, "RowUniqueAndUpdate fehlgeschlagen bei Skript: keyvalue_changed."); } }
        myRow.ExecuteScript(ScriptEventTypes.value_changed, string.Empty, true, true, true, 0.1f, null, true, true);
        if (!ok.AllOk) { { return new DoItFeedback(infos.Data, "RowUniqueAndUpdate fehlgeschlagen bei Skript: value_changed."); } }
        myRow.InvalidateCheckData();
        myRow.CheckRowDataIfNeeded();

        return Method_Row.RowToObjectFeedback(r[0]);
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => filterarg;

    #endregion
}