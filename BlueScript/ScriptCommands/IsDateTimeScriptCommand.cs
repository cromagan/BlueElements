// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Prüft, ob der Inhalt der Variable ein gültiges Datum/Zeit-Format ist.
/// </summary>
internal class IsDateTimeScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "isdatetime";
    public override List<string> Constants => [.. DateTimeFormats];
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "IsDateTime(Value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var ok = DateTimeTryParse(attvar.ValueStringGet(0), out _);
        return ok ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}
