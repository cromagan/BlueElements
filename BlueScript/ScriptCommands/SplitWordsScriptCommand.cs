// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class SplitWordsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "splitwords";
    public override string Description => "Gibt eine Liste aller Wörter zurück.\r\nDie Liste ist nach die Zeichen-Länge der Wörter absteigend sortiert.\r\nJedes Wort ist nur einmal in der Liste.";
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfStringsScriptVariable.ShortName_Plain;
    public override string Syntax => "SplitWordsScriptCommand(StringScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);

        var list = txt.AllWords().SortedDistinctList();

        list.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));

        return new DoItFeedback(list);
    }

    #endregion
}