// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Extended_Text;
using BlueScript.Classes;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_StringAsciiToHTML : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringasciitohtml";
    public override string Description => "Ersetzt einen ASCII-String zu einem HTML-String. Beispiel: aus ä wird &auml;";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "StringAsciiToHTML(String)";

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