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

using BlueControls.Editoren;
using BlueControls.Forms;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

public class Method_EditRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [RowVar];
    public override string Command => "editrow";
    public override List<string> Constants => [];

    public override string Description => "Öffnet den Bearbeiten-Dialog der Zeile.\r\n" +
            "Die eigene Zeile kann nur bearbeitet werden, wenn das Skript ReadOnly ist.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.ManipulatesUser;
    public override bool MustUseReturnValue => false;

    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "EditRow(Row);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        if (!tb.IsEditable(false)) {
            return new DoItFeedback($"Tabellensperre: {tb.IsNotEditableReason(false)}", true, ld);
        }

        if (row == BlockedRow(scp)) {
            MessageBox.Show("Bearbeitung aktuell nicht möglich.", BlueBasics.Enums.ImageCode.Warnung, "OK");
            return new DoItFeedback("Die Zeile kann aktuell nicht bearbeitet werden.", false, ld);
        }
        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        row.Edit(typeof(RowEditor), true);

        return DoItFeedback.Null();
    }

    #endregion
}