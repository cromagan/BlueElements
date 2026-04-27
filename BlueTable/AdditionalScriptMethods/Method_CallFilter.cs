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

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public sealed class Method_CallFilter : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal, FilterVar];
    public static string Command => "callfilter";
    

    public static string Description => "Sucht Zeilen und ruft in dessen Tabelle ein Skript für jede Zeile aus.\r\n" +
                                                "Über den Filtern kann bestimmt werden, welche Zeilen es betrifft.\r\n" +
                                            "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
                                            "Kein Zugriff auf auf Tabellen-Variablen!";



    public static int LastArgMinCount => 1;

    public static MethodType MethodLevel => MethodType.Sub;



    

   

    public static string Syntax => "CallFilter(SubName, Attribut0, Filter, ...);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var r = allFi.Rows;
        allFi.Dispose();
        if (r.Count == 0) { return DoItFeedback.Null(); }

        List<string> a = [attvar.ValueStringGet(1)];
        var vs = attvar.ValueStringGet(0);

        foreach (var thisR in r) {
            if (thisR is { IsDisposed: false }) {
                var scx = thisR.Table?.ExecuteScript(null, vs, scp.ProduktivPhase, thisR, a, false, true, 0);
                if (scx == null || scx.Failed) {
                    return new DoItFeedback("'Subroutinen-Aufruf [" + vs + "]' wegen vorherigem Fehler bei Zeile '" + thisR.CellFirstString() + "' abgebrochen", false, ld);
                }
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}