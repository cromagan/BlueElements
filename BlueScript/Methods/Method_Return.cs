// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_Return : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "return";
    public override List<string> Constants => [];
    public override string Description => "Beendet das Skript oder Unterskript ohne Fehler und setzt den Rückgabewert für Call-Routinen.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;

    public override string Syntax => "Return \"ReturnValue\";";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) =>
        new(false, false, true, string.Empty, attvar.Attributes[0], ld);

    #endregion
}