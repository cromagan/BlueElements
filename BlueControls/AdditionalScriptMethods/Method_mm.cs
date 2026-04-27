// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Converter;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_mm : Method {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "mm";

    public override List<string> Constants => [];
    public override string Description => "Rechnet mm in Pixel um - bei 300 dpi.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "mm(Number)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(MmToPixel((float)attvar.ValueNumGet(0), ItemCollectionPadItem.Dpi));

    #endregion
}