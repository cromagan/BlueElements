// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Prüft, ob die übergebene Zeile NULL ist.
/// </summary>
public class RowIsNullScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[RowScriptVariable.ClassId]];
    public override string Command => "rowisnull";
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ClassId;
    public override string Syntax => "RowIsNull(Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not RowScriptVariable vr) { return new DoItFeedback("Kein Zeilenobjekt übergeben.", true); }

        return new DoItFeedback(vr.IsNullOrEmpty);
    }

    #endregion
}
