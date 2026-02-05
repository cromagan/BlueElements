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

public class Method_RowDelete : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [RowVar];

    public override string Command => "rowdelete";

    public override List<string> Constants => [];

    public override string Description => "Löscht die gefundene Zeile. Kann auch die eigene Zele löschen - die Skriptausführung wird dann ohne Fehler abgebrochen.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";
    public override string Syntax => "RowDelete(Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        if (!scp.ProduktivPhase) {
            return DoItFeedback.TestModusInaktiv(ld);
        }

        var myrow = row == MyRow(scp);

        var r = RowCollection.Remove(row, "Script Command: RowDelete");

        if (myrow && r) {
            return new DoItFeedback("Eigene Zeile gelöscht", false, ld);
        } else {
            return new DoItFeedback(r);
        }
    }

    #endregion
}