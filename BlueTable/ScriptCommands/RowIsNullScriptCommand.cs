// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public class RowIsNullScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [RowVar];
    public override string Command => "rowisnull";
    public override string Description => "Prüft, ob die übergebene Zeile NULL ist.";
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "RowIsNullScriptCommand(Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not RowScriptVariable vr) { return new DoItFeedback("Kein Zeilenobjekt übergeben.", true); }

        return new DoItFeedback(vr.IsNullOrEmpty);
    }

    #endregion
}