// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_IsDateTime : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "isdatetime";
    public override List<string> Constants => [.. BlueBasics.ClassesStatic.Constants.DateTimeFormats];
    public override string Description => "Prüft, ob der Inhalt der Variable ein gültiges Datum/Zeit-Format ist. ";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "IsDateTime(Value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var ok = DateTimeTryParse(attvar.ValueStringGet(0), out _);
        return ok ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}