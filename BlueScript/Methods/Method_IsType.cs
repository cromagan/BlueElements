// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_IsType : Method {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable], StringVal];
    public override string Command => "istype";
    public override List<string> Constants => ["NUM", "LST", "STR", "BOL", "UKN"];
    public override string Description => "Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführt!";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "isType(Variable, num/str/lst/bol/ukn)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => string.Equals(attvar.ReadableText(1), attvar.MyClassId(0), StringComparison.OrdinalIgnoreCase)
            ? DoItFeedback.Wahr()
            : DoItFeedback.Falsch();

    #endregion
}