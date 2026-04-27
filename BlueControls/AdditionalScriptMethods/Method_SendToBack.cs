// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_SendToBack : Method {

    #region Properties

    public override List<List<string>> Args => [[VariablePadItem.ShortName_Variable]];
    public override string Command => "sendtoback";
    public override string Description => "Verschiebt das vorhandene PadItem in den Hintergrund.";
    public override string Syntax => "SendToBack(PadItem);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        //if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        //if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        if (attvar.Attributes[0] is not VariablePadItem ici) { return DoItFeedback.InternerFehler(ld); }
        if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }

        if (iciv.Parent is not ItemCollectionPadItem { IsDisposed: false } icpv) { return new DoItFeedback("Das Item gehört keiner Collection an", true, ld); }

        icpv.SendToBack(iciv);
        return DoItFeedback.Null();

        //return new DoItFeedback(ld, "Keine übereinstimmenden JointPoints gefunden.");
    }

    #endregion
}