// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Markiert die Zeile als gescheitert, ohne sie als Fehlerhaft zu setzen.
/// Dient dazu, temporäre Fehler, wie Netzwerkabbruche zu kompensieren.
/// Beim nächsten Programmstart ist deser Fehlerspeicher wieder gelöscht.
/// </summary>
public class SetFailedScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "setfailed";

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
