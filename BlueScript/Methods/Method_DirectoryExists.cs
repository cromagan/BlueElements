// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_DirectoryExists : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "directoryexists";
    public override string Description => "Prüft, ob ein Verzeichnis existiert";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "DirectoryExists(FilePath)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var pf = attvar.ValueStringGet(0);

        if (!pf.IsFormat(FormatHolder.Filepath)) {
            return new DoItFeedback("Dateipfad ungültig: " + pf, true, ld);
        }
        return new DoItFeedback(IO.DirectoryExists(pf));
    }

    #endregion
}