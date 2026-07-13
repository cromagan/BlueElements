// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_ExtractText : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "extracttext";

    public override string Description => "Extrahiert aus dem gegebenen String Textstellen und gibt eine Liste mit allen Funden zurück.\r\n" +
                                              "Beispiel: Extract(\"Ein guter Tag\", \"Ein * Tag\"); erstellt liste mit dem Inhalt \"guter\"";

    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string Syntax => "ExtractText(String, SearchPattern);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var tags = attvar.ValueStringGet(0).ReduceToMulti(attvar.ValueStringGet(1), StringComparison.OrdinalIgnoreCase);

        return tags is null ? new DoItFeedback("Nichts extrahiert - Searchpattern fehlerhaft?", true) : new DoItFeedback(tags);
    }

    #endregion
}