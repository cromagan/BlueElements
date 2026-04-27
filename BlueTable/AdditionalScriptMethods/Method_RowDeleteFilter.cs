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
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public sealed class Method_RowDeleteFilter : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [FilterVar];

    public static string Command => "rowdeletefilter";

    public static List<string> Constants => [];

    public static string Description => "Löscht die gefundenen Zeilen";



    public static int LastArgMinCount => 1;

    public static MethodType MethodLevel => MethodType.LongTime;

 // Auch nur zum Zeilen Anlegen

    public static string Returns => VariableBool.ShortName_Plain;

    public static string StartSequence => "(";
    public static string Syntax => "RowDeleteFilter(Filter, ...)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 0, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var rows = allFi.Rows;
        allFi.Dispose();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (BlockedRow(scp) is { } mr && rows.Contains(mr)) {
            return new DoItFeedback($"Der Löschen-Befehl würde die eigene Zeile löschen. Evtl. RowDelete benutzen", needsScriptFix, ld);
        }

        return new DoItFeedback(RowCollection.Remove(rows, "Script Command: RowDelete"));
    }

    #endregion
}