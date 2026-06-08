// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_ScaledView : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable], StringVal, StringVal, FloatVal, FloatVal, StringVal];
    public override string Command => "scaledview";
    public override string Description => "Erstellt eine Skalierte Ansicht - mit den angegebenen JointPoints.";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override string Returns => VariablePadItem.ShortName_Variable;
    public override string Syntax => "ScaledView(Collection, LinkeObereEckePunkt, Überschrift, SchriftSkalierung, Skalierung, Einzuschließende Punkte, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        var p1 = icpv.GetJointPoint(attvar.ValueStringGet(1), null);
        var d = new ScaledViewPadItem();

        if (p1 is not null) {
            d.SetLeftTopPoint(p1.X, p1.Y);
        }

        d.Caption = attvar.ValueStringGet(2);
        d.TextScale = (float)attvar.ValueNumGet(3);
        d.Scale = (float)attvar.ValueNumGet(4);

        var n = new List<string>();

        for (var z = 5; z < attvar.Attributes.Count; z++) {
            n.AddIfNotExists(attvar.ValueStringGet(z));
        }
        d.IncludedJointPoints = n.AsReadOnly();

        return new DoItFeedback(new VariablePadItem(d));
    }

    #endregion
}