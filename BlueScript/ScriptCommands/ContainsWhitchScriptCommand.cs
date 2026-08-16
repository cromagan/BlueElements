// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text.RegularExpressions;

namespace BlueScript.ScriptCommands;


internal class ContainsWhitchScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain], BoolVal, [StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain]];
    public override string Command => "containswhich";
    public override string Description => "Prüft ob eine der Zeichenketten als ganzes Wort vorkommt. Gibt dann alle gefundenen Strings als Liste a zurück.\r\nWort bedeutet, dass es als ganzes Wort vorkommen muss: 'Dach' gilt z.B. nicht als 'Hausdach'";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfStringsScriptVariable.ShortName_Plain;


    public override string Syntax => "ContainsWhich(StringScriptCommand, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var found = new List<string>();

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is StringScriptVariable vs1) { wordlist.Add(vs1.ValueString); }
            if (attvar.Attributes[z] is ListOfStringsScriptVariable vl1) { wordlist.AddRange(vl1.ValueList); }
        }
        wordlist = wordlist.SortedDistinctList();

        #endregion

        var rx = RegexOptions.IgnoreCase;
        if (attvar.ValueBoolGet(1)) { rx = RegexOptions.None; }

        if (attvar.Attributes[0] is StringScriptVariable vs2) {
            foreach (var thisW in wordlist) {
                if (vs2.ValueString.IndexOfWord(thisW, 0, rx) >= 0) {
                    found.AddIfNotExists(thisW);
                }
            }
        }

        if (attvar.Attributes[0] is ListOfStringsScriptVariable vl2) {
            foreach (var thiss in vl2.ValueList) {
                foreach (var thisW in wordlist) {
                    if (thiss.IndexOfWord(thisW, 0, rx) >= 0) {
                        found.AddIfNotExists(thisW);
                    }
                }
            }
        }

        return new DoItFeedback(found);
    }

    #endregion
}