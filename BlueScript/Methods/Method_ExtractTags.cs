// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_ExtractTags : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain], StringVal];
    public override string Command => "extracttags";

    public override string Description => "Extrahiert aus dem gegebenen String oder Liste die Schlagwörter und erstellt neue String-Variablen.\r\n" +
                                              "Das zweite Attribut dient als Erkennungszeichen, welche das Ende eine Schlagwortes angibt. Zuvor extrahierte Variablen werden wieder entfernt.\r\n" +
                                          "Beispiel: ExtractTags(\"Farbe: Blau\", \":\"); erstellt eine neue Variable 'extracted_farbe' mit dem Inhalt 'Blau'";

    public override string Syntax => "ExtractTags(String, Delemiter);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        const string comment = "Mit dem Befehl 'ExtractTags' erstellt";
        varCol.RemoveWithComment(comment);

        var tags = new List<string>();
        if (attvar.Attributes[0] is VariableString vs) { tags.Add(vs.ValueString); }
        if (attvar.Attributes[0] is VariableListString vl) { tags.AddRange(vl.ValueList); }

        foreach (var thisw in tags) {
            var x = thisw.SplitBy(attvar.ValueStringGet(1));

            if (x.Length == 2) {
                var vn = x[0].ToLowerInvariant().ReduceToChars(BlueBasics.ClassesStatic.Constants.AllowedCharsVariableName);
                var thisv = x[1].Trim();
                if (!string.IsNullOrEmpty(vn) && !string.IsNullOrEmpty(thisv)) {
                    varCol.Add(new VariableString("extracted_" + vn, thisv, true, comment));
                }
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}