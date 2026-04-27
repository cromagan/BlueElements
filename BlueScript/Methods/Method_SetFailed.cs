// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

public class Method_SetFailed : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "setfailed";
    public override List<string> Constants => [];

    public override string Description => "Markiert die Zeile als gescheitert, ohne sie als Fehlerhaft zu setzen.\r\n" +
                                            "Dient dazu, temporäre Fehler, wie Netzwerkabbruche zu kompensieren.\r\n" +
                                                "Beim nächsten Programmstart ist deser Fehlerspeicher wieder gelöscht.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;

    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SetFailed(Nachricht);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var r = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(r)) { return new DoItFeedback("Keine Fehlermeldung angegeben.", true, ld); }

        return new DoItFeedback(r, false, ld);
    }

    #endregion
}