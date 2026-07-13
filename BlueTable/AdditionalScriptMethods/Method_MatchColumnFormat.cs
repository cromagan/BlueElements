// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

internal class Method_MatchColumnFormat : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain], [Variable.Any_Variable]];
    public override string Command => "matchcolumnformat";
    public override string Description => "Prüft, ob der Inhalt der Variable mit dem Format der angegebenen Spalte übereinstimmt. Leere Inhalte sind dabei TRUE.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "MatchColumnFormat(Value, Column)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var column = Column(scp, attvar, 1);
        if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte in Tabelle nicht gefunden", true); }

        var tocheck = new List<string>();
        if (attvar.Attributes[0] is VariableListString vl) {
            tocheck.AddRange(vl.ValueList);
            tocheck = tocheck.SortedDistinctList();
        }
        if (attvar.Attributes[0] is VariableString vs) { tocheck.Add(vs.ValueString); }

        return tocheck.IsFormat(column, column.ValueRequired) ? DoItFeedback.Falsch() : DoItFeedback.Wahr();
    }

    #endregion
}