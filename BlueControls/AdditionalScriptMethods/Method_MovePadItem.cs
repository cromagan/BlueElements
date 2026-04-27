// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_MovePadItem : Method {

    #region Properties

    public override List<List<string>> Args => [[VariablePadItem.ShortName_Variable, VariableItemCollectionPad.ShortName_Variable], FloatVal, FloatVal];
    public override string Command => "movepaditem";
    public override List<string> Constants => [];
    public override string Description => "Verschiebt das vorhandene PadItem (oder alle Paditems in der Sammlung) um die angegebenen Pixel.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "MovePadItem(PadItem/Collection, X, Y);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is VariableItemCollectionPad icp) {
            if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }
            icpv.Items_Move(attvar.ValueIntGet(1), attvar.ValueIntGet(2));
        }

        if (attvar.Attributes[0] is VariablePadItem ici) {
            if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }
            if (iciv.Parent is not ItemCollectionPadItem { IsDisposed: false }) { return new DoItFeedback("Das Item gehört keiner Collection an", true, ld); }
            iciv.Move(attvar.ValueIntGet(1), attvar.ValueIntGet(2), false);
        }

        return DoItFeedback.Null();
    }

    #endregion
}