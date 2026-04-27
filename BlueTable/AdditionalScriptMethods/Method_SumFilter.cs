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

public sealed class Method_SumFilter : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [StringVal, FilterVar];
    public static string Command => "sumfilter";
    public static List<string> Constants => [];
    public static string Description => "Lädt eine andere Tabelle (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge summiert zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";

    public static int LastArgMinCount => 1;
    public static MethodType MethodLevel => MethodType.LongTime;
    public static bool MustUseReturnValue => true;
    public static string Returns => VariableDouble.ShortName_Plain;

    public static string StartSequence => "(";

    public static string Syntax => "SumFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, errorreason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback($"Filter-Fehler: {errorreason}", needsScriptFix, ld); }

        if (allFi.Table is not { IsDisposed: false } tb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellefehler!", true, ld);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = tb.Column[attvar.ReadableText(0)];
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ReadableText(0), true, ld); }

        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        var x = returncolumn.Summe(r);

        return x is not { } xd ? new DoItFeedback("Summe konnte nicht berechnet werden.", true, ld) : new DoItFeedback(xd);
    }

    #endregion
}