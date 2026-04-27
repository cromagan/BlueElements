// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueTable.AdditionalScriptMethods;

internal class Method_IsDropDownItem : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain], [Variable.Any_Variable]];
    public override string Command => "isdropdownitem";
    public override List<string> Constants => [];
    public override string Description => "Prüft, ob der Inhalt oder die Inhalte der Variable im Dropdownmenu der Spalte vorkommt.\r\nEs werden nur fest eingegebene Dropdown-Werte berücksichtigt - keine 'Werte anderer Zellen'.\r\nEs wird streng auf die Groß/Kleinschreibung geachtet.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "IsDropDownItem(Value, Column)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var column = Column(scp, attvar, 1);
        if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte in Tabelle nicht gefunden", true, ld); }

        var tocheck = new List<string>();
        if (attvar.Attributes[0] is VariableListString vl) { tocheck.AddRange(vl.ValueList); }
        if (attvar.Attributes[0] is VariableString vs) { tocheck.Add(vs.ValueString); }

        tocheck = tocheck.SortedDistinctList();

        return tocheck.Exists(thisstring => !column.DropDownItems.Contains(thisstring)) ? DoItFeedback.Falsch() : DoItFeedback.Wahr();
    }

    #endregion
}