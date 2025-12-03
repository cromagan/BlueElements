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

using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable;
using BlueTable.AdditionalScriptMethods;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

public class Method_SortedRows : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "sortedrows";
    public override List<string> Constants => [];
    public override string Description => "Gibt die Zeilen der Tabelle in der Standard Sortierung zurück.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListRow.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "SortedRows(table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }

        var r = tb.SortDefinition?.SortedRows(tb.Row) ?? new RowSortDefinition(tb, tb.Column.First, false).SortedRows(tb.Row);
        return new DoItFeedback(new VariableListRow(r));
    }

    #endregion
}