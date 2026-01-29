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

using BlueBasics.ClassesStatic;
using BlueControls.Classes;
using BlueControls.Forms;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using BlueTable.Classes;
using System.Collections.Generic;
using static BlueTable.AdditionalScriptMethods.Method_TableGeneric;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_OpenTab : Method {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "opentab";
    public override List<string> Constants => [];
    public override string Description => "Öffent einen neuen Tab in allen TableViews.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.GUI;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "OpenTab(Table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true, ld);
        }

        if (string.IsNullOrWhiteSpace(tb.Caption)) {

            if(tb is TableFile tbf) {
                return new DoItFeedback($"Die Benennung der Tabelle '{tbf.Filename.FileNameWithSuffix()}' fehlt.", true, ld);
            }

            return new DoItFeedback("Die Benennung der Tabelle fehlt.", true, ld);
        }

        foreach (var thisForm in FormManager.Forms) {
            if (thisForm is TableViewForm tbf && tbf.TabExists(tb.Caption) == null) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }
                tbf.AddTabPage(tb.Caption);
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}