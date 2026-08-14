// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

internal class IsDropDownItemScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain], [ScriptVariable.Any_Variable]];
    public override string Command => "isdropdownitem";
    public override string Description => "Prüft, ob der Inhalt oder die Inhalte der Variable im Dropdownmenu der Spalte vorkommt.\r\nEs werden nur fest eingegebene Dropdown-Werte berücksichtigt - keine 'Werte anderer Zellen'.\r\nEs wird streng auf die Groß/Kleinschreibung geachtet.";
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "IsDropDownItemScriptCommand(Value, Column)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var column = Column(scp, attvar, 1);
        if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte in Tabelle nicht gefunden", true); }

        var tocheck = new List<string>();
        if (attvar.Attributes[0] is ListOfStringsScriptVariable vl) { tocheck.AddRange(vl.ValueList); }
        if (attvar.Attributes[0] is StringScriptVariable vs) { tocheck.Add(vs.ValueString); }

        tocheck = tocheck.SortedDistinctList();

        return tocheck.Exists(thisstring => !column.DropDownItems.Contains(thisstring)) ? DoItFeedback.Falsch() : DoItFeedback.Wahr();
    }

    #endregion
}