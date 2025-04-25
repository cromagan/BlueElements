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

using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallFilter : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, FilterVar];
    public List<List<string>> ArgsForButton => [StringVal, StringVal];
    public List<string> ArgsForButtonDescription => ["Auszuführendes Skript", "Attribut0"];
    public ButtonArgs ClickableWhen => ButtonArgs.Eine_oder_mehr_Zeilen;
    public override string Command => "callfilter";
    public override List<string> Constants => [];

    public override string Description => "Sucht Zeilen und ruft in dessen Datenbank ein Skript für jede Zeile aus.\r\n" +
                                                "Über den Filtern kann bestimmt werden, welche Zeilen es betrifft.\r\n" +
                                            "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
                                            "Kein Zugriff auf auf Datenbank-Variablen!";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.SpecialVariables;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Ein Skript für jede Zeile der Filterung ausführen";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "CallFilter(SubName, Attribut0, Filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, myDb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var r = allFi.Rows;
        allFi.Dispose();
        if (r.Count == 0) { return DoItFeedback.Null(); }

        List<string> a = [attvar.ValueStringGet(1)];
        var vs = attvar.ValueStringGet(0);

        foreach (var thisR in r) {
            if (thisR is { IsDisposed: false }) {
                //s.Sub++;
                var s2 = thisR.ExecuteScript(null, vs, scp.ProduktivPhase, 0, a, false, true);
                if (s2.Failed) {
                    ld.Protocol.AddRange(s2.Protocol);
                    return new DoItFeedback("'Subroutinen-Aufruf [" + vs + "]' wegen vorherhigem Fehler bei Zeile '" + thisR.CellFirstString() + "' abgebrochen", true, ld);
                }
            }
        }

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0] + "," + args[1] + "," + filterarg;

    #endregion
}