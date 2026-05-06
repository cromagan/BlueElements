// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;
using static BlueTable.AdditionalScriptMethods.Method_TableGeneric;

namespace BlueTable.AdditionalScriptMethods;

public class Method_RowIsNull : Method {

    #region Properties

    public override List<List<string>> Args => [RowVar];
    public override string Command => "rowisnull";
    public override string Description => "Prüft, ob die übergebene Zeile NULL ist.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "RowIsNull(Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableRowItem vr) { return new DoItFeedback("Kein Zeilenobjekt übergeben.", true, ld); }

        return new DoItFeedback(vr.IsNullOrEmpty);
    }

    #endregion
}