// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

public class Method_SetFailed : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "setfailed";

    public override string Description => "Markiert die Zeile als gescheitert, ohne sie als Fehlerhaft zu setzen.\r\n" +
                                            "Dient dazu, temporäre Fehler, wie Netzwerkabbruche zu kompensieren.\r\n" +
                                                "Beim nächsten Programmstart ist deser Fehlerspeicher wieder gelöscht.";

    public override string Syntax => "SetFailed(Nachricht);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var r = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(r)) { return new DoItFeedback("Keine Fehlermeldung angegeben.", true); }

        return new DoItFeedback(r, false);
    }

    #endregion
}