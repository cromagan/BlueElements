// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Prüft, ob mindestens einer der angegebenen Werte gefunden wird.
/// Bei Listen: Prüft, ob einer der Werte als kompletter Eintrag in der Liste steht.
/// Bei Strings: Prüft, ob eine der Zeichenketten im Text vorkommt (auch mitten im Wort).
/// Das zweite Attribut legt fest, ob Groß- und Kleinschreibung beachtet wird (true) oder ignoriert wird (false).
/// Die Rückgabe ist true, sobald ein Wert gefunden wurde, sonst false.
/// </summary>
internal class ContainsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain], BoolVal, [StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain]];
    public override string Command => "contains";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;

    public override string Syntax => "Contains(String/Liste, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is StringScriptVariable vs1) {
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

        if (attvar.Attributes[0] is StringScriptVariable vs2) {
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
