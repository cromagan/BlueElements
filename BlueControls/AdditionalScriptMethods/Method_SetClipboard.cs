// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_SetClipboard : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "setclipboard";
    public override List<string> Constants => [];
    public override string Description => "Speichert den Text im Clipboard.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;


    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "SetClipboard(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var vs = attvar.ValueStringGet(0);
        CopytoClipboard(vs);

        return DoItFeedback.Null();
    }


    #endregion
}