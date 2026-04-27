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

using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal sealed class Method_MirrorPadItem : Method, IMethod {

    #region Properties

    public static List<List<string>> Args => [[VariablePadItem.ShortName_Variable, VariableItemCollectionPad.ShortName_Variable], StringVal, BoolVal, BoolVal];
    public static string Command => "mirrorpaditem";
    public static List<string> Constants => [];
    public static string Description => "Spiegelt das vorhandene PadItem (oder alle Paditems in der Sammlung) um den angegebenen Punkt.";
    public static bool GetCodeBlockAfter => false;
    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.Standard;
    public static bool MustUseReturnValue => false;
    public static string Returns => string.Empty;
    public static string StartSequence => "(";
    public static string Syntax => "MirrorPadItem(PadItem/Collection,JointPoint, Vertikal, Horizontal);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is VariableItemCollectionPad icp) {
            if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }
            var p1 = icpv.GetJointPoint(attvar.ValueStringGet(1), null);
            icpv.Items_Mirror(p1, attvar.ValueBoolGet(2), attvar.ValueBoolGet(3));
        }

        if (attvar.Attributes[0] is VariablePadItem ici) {
            if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }
            if (iciv.Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return new DoItFeedback("Das Item gehört keiner Collection an", true, ld); }
            var p1 = icpi.GetJointPoint(attvar.ValueStringGet(1), null);

            if (iciv is IMirrorable m) {
                m.Mirror(p1, attvar.ValueBoolGet(2), attvar.ValueBoolGet(3));
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}