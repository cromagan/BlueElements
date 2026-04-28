// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_FileExists : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "fileexists";
    public override string Description => "Prüft, ob eine Datei existiert";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "FileExists(FilePath)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        return !filn.IsFormat(FormatHolder_FilepathAndName.Instance)
            ? new DoItFeedback("Dateinamen-Fehler!", true, ld)
            : new DoItFeedback(IO.FileExists(filn));
    }

    #endregion
}