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

using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public class Method_Linkify : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, TableVar, StringVal, StringVal];
    public override string Command => "linkify";
    public override List<string> Constants => [];

    public override string Description => "Ersetzt Wörter im eingehenden Text durch Links.\r\nDie Funktion durchsucht eine andere Tabelle nach einer Spalte mit identischen Textinhalten. Wird ein vollständiger Zelleninhalt gefunden, wird dieser im Text durch einen Link zur entsprechenden Zeile ersetzt. Optional kann die zu durchsuchende Spalte explizit angegeben werden. Der verlinkte Text bleibt dabei textlich identisch – er wird lediglich verlinkt.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;

    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "linkify(Text, TargetTable, SearchColumn, ReplaceWihtColumn);";

    #endregion

    #region Methods

    public static string GenerateHtmlCellLink(string tableName, string columnKey, string rowKey) {
        return $"<CELLLINK={tableName}|{columnKey}|{rowKey}>";
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Parameter 1: Eingehender Text
        var inputText = attvar.ValueStringGet(0);

        // Parameter 2: Zieltabelle
        if (attvar.Attributes[1] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true, ld);
        }

        // Parameter 3: Zu durchsuchende Spalte
        var searchColumn = tb.Column[attvar.ValueStringGet(2)];
        if (searchColumn == null) {
            return new DoItFeedback($"Such-Spalte '{attvar.ValueStringGet(2)}' nicht gefunden", true, ld);
        }

        // Parameter 4: Spalte für Link-Text (optional, sonst = SearchColumn)
        var linkColumn = tb.Column[attvar.ValueStringGet(3)];
        if (linkColumn == null) {
            return new DoItFeedback($"Link-Spalte '{attvar.ValueStringGet(3)}' nicht gefunden", true, ld);
        }

        // Alle eindeutigen Werte nach Länge sortiert (längste zuerst)
        var searchData = searchColumn.GetCellContentsSortedByLength();
        var resultText = inputText;

        foreach (var (term, row) in searchData) {
            if (!resultText.Contains(term)) { continue; }

            var linkText = row.CellGetString(linkColumn);
            var link = GenerateHtmlCellLink(tb.KeyName, linkColumn.KeyName, row.KeyName);
            resultText = resultText.Replace(term, link);
        }

        return new(resultText);
    }

    #endregion
}