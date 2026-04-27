// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Split : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "split";
    public override string Description => "Wandelt einen Text in eine Liste um.\r\nEs trennt den Text dabei mitteles dem angegebenen Trennzeichen.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string Syntax => "Split(String, Trennzeichen)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).SplitBy(attvar.ValueStringGet(1)));

    #endregion
}