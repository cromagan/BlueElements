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

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal sealed class Method_DeletePoints : Method, IMethod {

    #region Properties

    public static List<List<string>> Args => [[VariablePadItem.ShortName_Variable, VariableItemCollectionPad.ShortName_Variable], StringVal];
    public static string Command => "deletepoints";
    public static List<string> Constants => [];
    public static string Description => "Löscht die angegebenen Punkte zu verbinden.\r\nWird keine Name angegeben, werden alle Punkte gelöscht.\r\nWird keinen Fehler auslösen.";
    public static bool GetCodeBlockAfter => false;
    public static int LastArgMinCount => 0;
    public static MethodType MethodLevel => MethodType.Standard;
    public static bool MustUseReturnValue => false;
    public static string Returns => string.Empty;
    public static string StartSequence => "(";
    public static string Syntax => "DeletePoints(PadItem/Collection, PointName, ...);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        List<string> names = [];
        for (var z = 1; z < attvar.Attributes.Count; z++) {
            names.Add(attvar.ValueStringGet(z));
        }

        if (attvar.Attributes[0] is VariableItemCollectionPad icp) {
            if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }
            icpv.Items_DeleteJointPoints(names);
            icpv.DeleteJointPoints(names);
        }

        if (attvar.Attributes[0] is VariablePadItem ici) {
            if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }
            if (iciv.Parent is not ItemCollectionPadItem { IsDisposed: false }) { return new DoItFeedback("Das Item gehört keiner Collection an", true, ld); }
            iciv.DeleteJointPoints(names);
        }

        return DoItFeedback.Null();
    }

    #endregion
}