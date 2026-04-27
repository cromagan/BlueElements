// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_TrimStart : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "trimstart";
    public override List<string> Constants => [];
    public override string Description => "Entfernt die angegebenen Texte am Anfang des Strings. Groß und Kleinschreibung wird ignoriert.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "TrimStart(String, TexttoTrim, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var val = attvar.ValueStringGet(0);

        string txt;

        do {
            txt = val;
            for (var z = 1; z < attvar.Attributes.Count; z++) {
                val = val.TrimStart(attvar.ValueStringGet(z));
            }
        } while (txt != val);

        return new DoItFeedback(val);
    }

    #endregion
}