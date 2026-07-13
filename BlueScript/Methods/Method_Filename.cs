// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Filename : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "filename";
    public override string Description => "Gibt den Dateinamen ohne Pfad und ohne Suffix zurück";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Filename(FilePathAndName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).FileNameWithoutSuffix());

    #endregion
}