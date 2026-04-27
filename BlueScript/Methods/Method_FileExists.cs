// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_FileExists : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "fileexists";
    public override List<string> Constants => [];
    public override string Description => "Prüft, ob eine Datei existiert";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "FileExists(FilePath)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        return !filn.IsFormat(FormatHolder.FilepathAndName)
            ? new DoItFeedback("Dateinamen-Fehler!", true, ld)
            : new DoItFeedback(IO.FileExists(filn));
    }

    #endregion
}