// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_AddPadItem : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable], [VariablePadItem.ShortName_Variable]];
    public override string Command => "addpaditem";
    public override string Description => "Fügt einer ItemCollectionPadItem ein PadItem hinzu.\r\nAnschließend wird es mit JointPoints ausgerichtet.";
    public override string Syntax => "AddPadItem(Collection, PadItem);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        if (attvar.Attributes[1] is not VariablePadItem ici) { return DoItFeedback.InternerFehler(ld); }
        if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }

        if (iciv.Parent is not null) { return new DoItFeedback("Das Item gehört breits einer Collection an", true, ld); }

        icpv.Add(iciv);

        if (iciv.JointPoints.Count == 0) { return DoItFeedback.Null(); }

        foreach (var pt in iciv.JointPoints) {
            if (icpv.GetJointPoint(pt.KeyName, iciv) is { } p) {
                iciv.ConnectJointPoint(pt, p);
                return DoItFeedback.Null();
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}