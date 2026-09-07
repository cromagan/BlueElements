// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführt!
/// </summary>
internal class IsTypeScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[ScriptVariable.Any_Variable], StringVal];
    public override string Command => "istype";
    public override List<string> Constants => ["NUM", "LST", "STR", "BOL", "UKN"];
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "isType(Variable, num/str/lst/bol/ukn)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => string.Equals(attvar.ReadableText(1), attvar.MyClassId(0), StringComparison.OrdinalIgnoreCase)
            ? DoItFeedback.Wahr()
            : DoItFeedback.Falsch();

    #endregion
}
