// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Entfernt aus dem Text unnötige Leerzeichen, Tabs etc.
/// Kann dazu verwendet werden, um Code-Dateien (z.B. HTML) zu standardisieren.
/// </summary>
internal class RemoveDoubleSpacesScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "removedoublespaces";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "RemoveDoubleSpacesScriptCommand(text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var t = attvar.ValueStringGet(0);
        string ot;
        do {
            ot = t;
            t = t.Replace('\n', '\r');
            t = t.Replace('\t', '\r');
            t = t.Replace("  ", " ");
            t = t.Replace("\r ", "\r");
            t = t.Replace(" \r", "\r");
            t = t.Replace("\r\r", "\r");
            t = t.Replace(">\r", ">");
            t = t.Replace("\r<", "<");
            t = t.Replace(" <", "<");
            t = t.Replace("> ", ">");
        } while (ot != t);

        return new DoItFeedback(t);
    }

    #endregion
}
