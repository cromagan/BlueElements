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

internal sealed class Method_ImportCsv : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal];
    public static string Command => "importcsv";
    public static List<string> Constants => [];
    public static string Description => "Importiert den Inhalt, der als CSV vorliegen muss, in die Tabelle.";

    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.LongTime;

    public static string Returns => string.Empty;
    public static string StartSequence => "(";
    public static string Syntax => "ImportCSV(CSVText, Separator);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } tb) { return new DoItFeedback($"Import nur aus einer Datenbank heraus möglich.", true, ld); }

        if (BlockedRow(scp) != null) { return new DoItFeedback($"Import in einem Zeilenskript nicht möglich.", false, ld); }

        var txt = attvar.ValueStringGet(0);
        var sep = attvar.ValueStringGet(1);

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false, ld); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        var sx = tb.ImportCsv(txt, true, sep, false, false);

        return string.IsNullOrEmpty(sx) ? DoItFeedback.Null() : new DoItFeedback(sx, true, ld);
    }

    #endregion
}