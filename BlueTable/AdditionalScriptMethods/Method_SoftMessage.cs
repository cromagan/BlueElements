// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_SoftMessage : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "softmessage";

    public override string Description => "Gibt in der Statusleiste einen Nachricht aus, wenn ein Steuerelement vorhanden ist, dass diese anzeigen kann.";

    public override string Syntax => "SoftMessage(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = "<b>Skript:</b> " + attvar.ValueStringGet(0);

        Develop.Message(ErrorType.Info, MyTable(scp), "Skript", ImageCode.Tabelle, txt, 0);

        return DoItFeedback.Null();
    }

    #endregion
}