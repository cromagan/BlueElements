// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

internal class StringAsciiToHTMLScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringasciitohtml";
    public override string Description => "Ersetzt einen ASCII-StringScriptCommand zu einem HTML-StringScriptCommand. Beispiel: aus ä wird &auml;";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "StringAsciiToHTMLScriptCommand(StringScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);

        using var e = new ExtText();
        e.PlainText = txt;

        return new(e.HtmlText);
    }

    #endregion
}