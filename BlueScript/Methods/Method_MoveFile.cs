// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;

internal class Method_MoveFile : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "movefile";
    public override List<string> Constants => [];
    public override string Description => "Verschiebt eine Datei.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false;

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "MoveFile(SourceCompleteName, DestinationCompleteName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var sop = attvar.ValueStringGet(0);
        var dep = attvar.ValueStringGet(1);

        if (!DirectoryExists(dep.FilePath())) { return new DoItFeedback("Ziel-Verzeichnis existiert nicht:" + dep.FilePath(), true, ld); }
        if (!FileExists(sop)) { return new DoItFeedback("Quelldatei existiert nicht.", true, ld); }

        if (FileExists(dep)) { return DoItFeedback.Falsch(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (!MoveFile(sop, dep, false)) {
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}