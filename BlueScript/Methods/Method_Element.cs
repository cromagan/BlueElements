// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_Element : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, FloatVal];
    public override string Command => "element";
    public override string Description => "Gibt ein das Element der Liste mit der Indexnummer als Text zurück. Die Liste beginnt mit dem Element 0.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Element(VariableListe, Indexnummer)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var i = attvar.ValueIntGet(1);
        var list = attvar.ValueListStringGet(0);
        return i < 0 || i >= list.Count
            ? new DoItFeedback("Element '" + i + "' nicht in der Liste vorhanden", true, ld)
            : new DoItFeedback(list[i]);
    }

    #endregion
}