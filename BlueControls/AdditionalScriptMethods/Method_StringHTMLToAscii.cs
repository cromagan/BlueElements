// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Extended_Text;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_StringHTMLToAscii : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringhtmltoascii";
    public override string Description => "Ersetzt einen HTML-String zu normalen ASCII-String. Beispiel: Aus &auml; wird ä.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "StringHTMLToAscii(String)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);

        using var e = new ExtText();
        e.HtmlText = txt;

        return new(e.PlainText);
    }

    #endregion
}