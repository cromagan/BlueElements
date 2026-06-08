// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_MirrorPadItem : Method {

    #region Properties

    public override List<List<string>> Args => [[VariablePadItem.ShortName_Variable, VariableItemCollectionPad.ShortName_Variable], StringVal, BoolVal, BoolVal];
    public override string Command => "mirrorpaditem";
    public override string Description => "Spiegelt das vorhandene PadItem (oder alle Paditems in der Sammlung) um den angegebenen Punkt.";
    public override string Syntax => "MirrorPadItem(PadItem/Collection,JointPoint, Vertikal, Horizontal);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
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