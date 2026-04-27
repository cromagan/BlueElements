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

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal sealed class Method_AddPadItem : Method, IMethod {

    #region Properties

    public static List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable], [VariablePadItem.ShortName_Variable]];
    public static string Command => "addpaditem";
    public static List<string> Constants => [];
    public static string Description => "Fügt einer ItemCollectionPadItem ein PadItem hinzu.\r\nAnschließend wird es mit JointPoints ausgerichtet.";
    public static bool GetCodeBlockAfter => false;
    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.Standard;
    public static bool MustUseReturnValue => false;
    public static string Returns => string.Empty;
    public static string StartSequence => "(";
    public static string Syntax => "AddPadItem(Collection, PadItem);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        if (attvar.Attributes[1] is not VariablePadItem ici) { return DoItFeedback.InternerFehler(ld); }
        if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }

        if (iciv.Parent != null) { return new DoItFeedback("Das Item gehört breits einer Collection an", true, ld); }

        icpv.Add(iciv);

        if (iciv.JointPoints.Count == 0) { return DoItFeedback.Null(); }

        foreach (var pt in iciv.JointPoints) {
            var p = icpv.GetJointPoint(pt.KeyName, iciv);
            if (p != null) {
                iciv.ConnectJointPoint(pt, p);
                return DoItFeedback.Null();
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}