// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Prüft, ob der Inhalt der Variable mit dem Format der angegebenen Spalte übereinstimmt. Gibt bei Erfolg einen leeren Text zurück, andernfalls eine lesbare Begründung.
/// </summary>
internal class MatchColumnFormatScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ClassId, ListOfStringsScriptVariable.ClassId], [ScriptVariable.Any_Plain]];
    public override string Command => "matchcolumnformat";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ClassId;
    public override string Syntax => "MatchColumnFormat(Value, Column)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var column = Column(scp, attvar, 1);
        if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte in Tabelle nicht gefunden", true); }

        var tocheck = new List<string>();
        if (attvar.Attributes[0] is ListOfStringsScriptVariable vl) {
            tocheck.AddRange(vl.ValueList);
            tocheck = tocheck.SortedDistinctList();
        }
        if (attvar.Attributes[0] is StringScriptVariable vs) { tocheck.Add(vs.ValueString); }

        return new DoItFeedback(tocheck.IsFormat(column));
    }

    #endregion
}
