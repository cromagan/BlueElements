// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

internal class StringHTMLToAsciiScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringhtmltoascii";
    public override string Description => "Ersetzt einen HTML-StringScriptCommand zu normalen ASCII-StringScriptCommand. Beispiel: Aus &auml; wird ä.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "StringHTMLToAsciiScriptCommand(StringScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);

        using var e = new ExtText();
        e.HtmlText = txt;

        return new(e.PlainText);
    }

    #endregion
}