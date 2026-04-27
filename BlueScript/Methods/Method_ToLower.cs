// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_ToLower : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "tolower";
    public override string Description => "Gibt den Text in Kleinbuchstaben zurück";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "ToLower(OriginalString)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).ToLowerInvariant());

    #endregion
}