// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_Filenpath : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "filepath";
    public override string Description => "Gibt den Dateipad eines Dateistrings zurück, mit abschließenden \\.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Filepath(FilePathAndName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).FilePath());

    #endregion
}