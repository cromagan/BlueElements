// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class ContainsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Variable, ListOfStringsScriptVariable.ShortName_Variable], BoolVal, [StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain]];
    public override string Command => "contains";
    public override string Description => "Bei Listen: Prüft, ob einer der Werte in der Liste steht. Bei String: Prüft ob eine der Zeichenketten vorkommt.";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;

    public override string Syntax => "Contains(ListVariable/StringVariable, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is ScriptVariables.StringScriptVariable vs1) {
                wordlist.Add(vs1.ValueString);
            } else if (attvar.Attributes[z] is ListOfStringsScriptVariable vl1) {
                wordlist.AddRange(vl1.ValueList);
            }
        }
        wordlist = wordlist.SortedDistinctList();

        #endregion

        // Der Comparer muss hier definiert werden, damit er für beide Blöcke gültig ist.
        var comparer = attvar.ValueBoolGet(1) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var comparison = attvar.ValueBoolGet(1) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (attvar.Attributes[0] is ListOfStringsScriptVariable vl2) {
            var x = vl2.ValueList;
            return wordlist.Exists(thisW => x.Contains(thisW, comparer)) ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
        }

        if (attvar.Attributes[0] is ScriptVariables.StringScriptVariable vs2) {
            foreach (var thisW in wordlist) {
                // ScriptVariables.String.Contains benötigt StringComparison, nicht StringComparer.
                if (vs2.ValueString.Contains(thisW, comparison)) {
                    return DoItFeedback.Wahr();
                }
            }
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.FalscherDatentyp();
    }

    #endregion
}