// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Int : Method {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "int";

    public override string Description => "Schneidet Nachkommastellen ab. Um einen Text in einen Zahlenwert zu verwandeln, ist der Befehl Number() zu benutzen.";

    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Int(Number)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueIntGet(0));

    #endregion
}