// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_ExtractFirstText : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "extractfirsttext";
    public override List<string> Constants => [];

    public override string Description => "Extrahiert aus dem gegebenen String Textstellen und gibt einen String mit dem ersten Fund zurück.\r\n" +
                                              "Wird kein Text gefunden, wird der Defaultwert zurück gegeben.\r\n" +
                                          "Beispiel: Extract(\"Ein guter Tag\", \"Ein * Tag\"); gibt den Text \"guter\" zurück.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "ExtractFirstText(String, SearchPattern, Default);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var tags = attvar.ValueStringGet(0).ReduceToMulti(attvar.ValueStringGet(1), StringComparison.OrdinalIgnoreCase);

        return tags is not { Count: not 0 } ? new DoItFeedback(attvar.ValueStringGet(2)) : new DoItFeedback(tags[0]);
    }

    #endregion
}