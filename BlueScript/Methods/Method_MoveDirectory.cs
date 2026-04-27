// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;


internal class Method_MoveDirectory : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "movedirectory";
    public override string Description => "Verschiebt einen Ordner.";



    public override MethodType MethodLevel => MethodType.LongTime;


    public override string Returns => VariableBool.ShortName_Plain;


    public override string Syntax => "MoveDirectory(SourceCompleteName, DestinationCompleteName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var sop = attvar.ValueStringGet(0);
        if (!DirectoryExists(sop)) { return new DoItFeedback("Quell-Verzeichnis existiert nicht.", true, ld); }
        var dep = attvar.ValueStringGet(1);

        if (DirectoryExists(dep)) { return DoItFeedback.Falsch(); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (!DirectoryMove(sop, dep, false)) {
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}