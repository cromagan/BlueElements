// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;


public class Method_SoftMessage : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal];



    public override string Command => "softmessage";

    public override List<string> Constants => [];
    public override string Description => "Gibt in der Statusleiste einen Nachricht aus, wenn ein Steuerelement vorhanden ist, dass diese anzeigen kann.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.Standard;

    public override bool MustUseReturnValue => false;



    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "SoftMessage(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = "<b>Skript:</b> " + attvar.ValueStringGet(0);

        Develop.Message(ErrorType.Info, MyTable(scp), "Skript", ImageCode.Tabelle, txt, 0);

        return DoItFeedback.Null();
    }

 

    #endregion
}