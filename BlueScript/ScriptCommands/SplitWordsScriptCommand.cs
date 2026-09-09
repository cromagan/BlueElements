// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Gibt eine Liste aller Wörter zurück.
/// Die Liste ist nach die Zeichen-Länge der Wörter absteigend sortiert.
/// Jedes Wort ist nur einmal in der Liste.
/// </summary>
internal class SplitWordsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "splitwords";
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfStringsScriptVariable.ShortName_Plain;
    public override string Syntax => "SplitWords(StringScriptCommand)";

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
