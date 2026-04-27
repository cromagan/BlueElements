// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;


internal class Method_ReduceToChars : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "reducetochars";
    public override string Description => "Entfernt aus dem Text alle Zeichen die nicht erlaubt sind";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "ReduceToChars(OriginalString, ErlaubteZeichenString)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).ReduceToChars(attvar.ValueStringGet(1)));

    #endregion
}