// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_RemoveDoubleSpaces : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "removedoublespaces";
    public override string Description => "Entfernt aus dem Text unnötige Leerzeichen, Tabs etc.\r\nKann dazu verwendet werden, um Code-Dateien (z.B. HTML) zu standardisieren.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "RemoveDoubleSpaces(text)";

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