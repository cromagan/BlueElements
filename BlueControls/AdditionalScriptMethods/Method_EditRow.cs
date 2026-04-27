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

public sealed class Method_EditRow : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [RowVar];
    public static string Command => "editrow";
    

    public static string Description => "Öffnet den Bearbeiten-Dialog der Zeile.\r\n" +
            "Die eigene Zeile kann nur bearbeitet werden, wenn das Skript ReadOnly ist.";


    public static int LastArgMinCount => 1;
    public static MethodType MethodLevel => MethodType.ManipulatesUser;


    
   
    public static string Syntax => "EditRow(Row);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) {
            return new DoItFeedback($"Tabellensperre: {f}", true, ld);
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