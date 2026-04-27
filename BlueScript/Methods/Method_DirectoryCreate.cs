// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.Methods;


internal class Method_DirectoryCreate : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "directorycreate";
    public override string Description => "Erstellt ein Verzeichnis, falls dieses nicht existert. Gibt TRUE zurück, erstellt wurde oder bereits existierte.";



    public override MethodType MethodLevel => MethodType.LongTime;


    public override string Returns => VariableBool.ShortName_Plain;


    public override string Syntax => "DirectoryCreate(Path)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var p = attvar.ValueStringGet(0).TrimEnd('\\');
        return CreateDirectory(p) ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}